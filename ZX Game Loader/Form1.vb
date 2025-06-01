Imports System.IO
Imports System.Diagnostics
Imports System.Drawing
Imports System.Threading.Tasks

Public Class Form1
    ' Folder paths and settings
    Dim GameFolder As String = Path.Combine(Application.StartupPath, "Games")
    Dim ImageFolder As String = Path.Combine(Application.StartupPath, "Images")
    Dim ManualFolder As String = Path.Combine(Application.StartupPath, "Manuals")
    Private ReadOnly _settingsFilePath As String = Path.Combine(Application.StartupPath, "settings.txt")
    Private _lastSelectedGame As String = String.Empty
    Private _allGames As New List(Of String)()
    Private _currentSearchTerm As String = ""
    Private _currentImagePath As String = ""
    Private _currentManualPath As String = ""
    Private _editModeEnabled As Boolean = False ' Edit Mode flag
    Private _editModeMenuItem As ToolStripMenuItem = Nothing ' Reference to the Edit Mode menu item

    ' Allowed file extensions
    Private ReadOnly _allowedImageExtensions As String() = {".png", ".jpg", ".jpeg", ".gif", ".bmp"}
    Private ReadOnly _allowedManualExtensions As String() = {".txt"}
    Private ReadOnly _allowedGameExtensions As String() = {".tzx", ".tap"}

    ' Process and playback control
    Private tzxPlayProcess As Process = Nothing
    Private _childProcesses As New List(Of Process)()
    Private controlFilePath As String = Path.Combine(Path.GetTempPath(), "tzx_control.txt")
    Private totalBlocksFilePath As String = Path.Combine(Path.GetTempPath(), "total_blocks.txt")
    Private nextBlockFilePath As String = Path.Combine(Path.GetTempPath(), "next_block.txt")
    Private saveStatusFile As String = Path.Combine(Path.GetTempPath(), "save_status.txt")
    Private saveControlFile As String = Path.Combine(Path.GetTempPath(), "save_control.txt")

    ' File system watcher
    Private WithEvents _gameFolderWatcher As FileSystemWatcher

    ' Counters and timers
    Private tapeCounter As Integer = 0
    Private totalBlocks As Integer = 0
    Private zeroedBlock As Integer = 0
    Private tapeCounterTimer As New Timer() With {.Interval = 1000}
    Private WithEvents TimerSaveStatus As New Timer() With {.Interval = 500}
    Private WithEvents TimerResetStatus As New Timer() With {.Interval = 10000, .Enabled = False}
    Private isRecording As Boolean = False
    Private isStopped As Boolean = False

    ' Scrolling text variables
    Private WithEvents scrollTimer As New Timer() With {.Interval = 100} ' Lower = faster scrolling
    Private scrollText As String = ""
    Private scrollIndex As Integer = 0
    Private Const DISPLAY_CHARS As Integer = 78

    ' Edit Mode Management

    Private Sub ToggleEditMode(enable As Boolean)
        _editModeEnabled = enable
        ListBox1.AllowDrop = enable
        PictureBox1.AllowDrop = enable
        RichTextBox1.AllowDrop = enable

        If ListBox1.ContextMenuStrip IsNot Nothing Then
            ListBox1.ContextMenuStrip.Enabled = enable
        End If

        ' Update menu item text
        UpdateEditModeMenuText()
    End Sub

    Private Sub UpdateEditModeMenuText()
        If _editModeMenuItem IsNot Nothing Then
            _editModeMenuItem.Text = If(_editModeEnabled, "Editor On", "Editor Off")
        End If
    End Sub

    Private Sub EditModeToolStripMenuItem_Click(sender As Object, e As EventArgs)
        ToggleEditMode(Not _editModeEnabled)
    End Sub

    ' Resource Management Methods

    Private Sub ReleaseImageResources()
        If PictureBox1.Image IsNot Nothing Then
            Dim oldImage = PictureBox1.Image
            PictureBox1.Image = Nothing
            oldImage.Dispose()
            _currentImagePath = ""
        End If
    End Sub

    Private Sub ReleaseManualResources()
        RichTextBox1.Text = ""
        _currentManualPath = ""
    End Sub

    Public Sub PrepareForFileOperation()
        ReleaseImageResources()
        ReleaseManualResources()
        Application.DoEvents()
    End Sub

    ' Safe File Operations

    Public Function SafeDeleteFile(filePath As String, Optional maxRetries As Integer = 2) As Boolean
        For retry = 1 To maxRetries
            Try
                PrepareForFileOperation()
                If File.Exists(filePath) Then
                    File.Delete(filePath)
                    Return True
                End If
                Return False
            Catch ex As Exception When retry < maxRetries
                Threading.Thread.Sleep(100)
            Catch ex As Exception
                Debug.WriteLine($"Failed to delete {filePath}: {ex.Message}")
                Return False
            End Try
        Next
        Return False
    End Function

    Public Function SafeDeleteGameImage(gameName As String) As Boolean
        Dim success As Boolean = True
        For Each ext In _allowedImageExtensions
            Dim imagePath = Path.Combine(ImageFolder, gameName & ext)
            If File.Exists(imagePath) AndAlso Not SafeDeleteFile(imagePath) Then
                success = False
            End If
        Next
        Return success
    End Function

    Public Function SafeDeleteManual(gameName As String) As Boolean
        Dim manualPath = Path.Combine(ManualFolder, gameName & ".txt")
        Return SafeDeleteFile(manualPath)
    End Function

    Public Function SafeRenameFile(oldPath As String, newPath As String) As Boolean
        Try
            PrepareForFileOperation()
            If File.Exists(oldPath) Then
                Directory.CreateDirectory(Path.GetDirectoryName(newPath))
                File.Move(oldPath, newPath)
                Return True
            End If
            Return False
        Catch ex As Exception
            Debug.WriteLine($"Failed to rename {oldPath} to {newPath}: {ex.Message}")
            Return False
        End Try
    End Function

    ' Drag-and-Drop Handlers (with Edit Mode check)

    Private Sub ListBox1_DragEnter(sender As Object, e As DragEventArgs) Handles ListBox1.DragEnter
        If Not _editModeEnabled Then
            e.Effect = DragDropEffects.None
            Return
        End If

        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
            If files.Any(Function(f) _allowedGameExtensions.Contains(Path.GetExtension(f).ToLower())) Then
                e.Effect = DragDropEffects.Copy
            Else
                e.Effect = DragDropEffects.None
            End If
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub ListBox1_DragDrop(sender As Object, e As DragEventArgs) Handles ListBox1.DragDrop
        If Not _editModeEnabled Then Return

        Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        If files.Length = 0 Then Return

        Dim gameFiles = files.Where(Function(f) _allowedGameExtensions.Contains(Path.GetExtension(f).ToLower())).ToArray()
        If gameFiles.Length = 0 Then Return

        Try
            Dim successCount As Integer = 0
            Dim errorMessages As New List(Of String)

            For Each sourceFile In gameFiles
                Try
                    Dim fileName = Path.GetFileName(sourceFile)
                    Dim destPath = Path.Combine(GameFolder, fileName)

                    If File.Exists(destPath) Then
                        Dim result = MessageBox.Show($"The file {fileName} already exists. Overwrite?",
                                                  "File Exists",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question)
                        If result <> DialogResult.Yes Then
                            Continue For
                        End If
                    End If

                    File.Copy(sourceFile, destPath, True)
                    successCount += 1
                Catch ex As Exception
                    errorMessages.Add($"Error copying {Path.GetFileName(sourceFile)}: {ex.Message}")
                End Try
            Next

            LoadGames()

            Dim message = $"{successCount} game(s) added successfully."
            If errorMessages.Count > 0 Then
                message += Environment.NewLine & Environment.NewLine & "Errors:" & Environment.NewLine & String.Join(Environment.NewLine, errorMessages)
            End If

            MessageBox.Show(message, "Import Complete", MessageBoxButtons.OK,
                          If(errorMessages.Count > 0, MessageBoxIcon.Warning, MessageBoxIcon.Information))
        Catch ex As Exception
            MessageBox.Show($"Error processing files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub InitializeGameFolderWatcher()
        _gameFolderWatcher = New FileSystemWatcher() With {
            .Path = GameFolder,
            .NotifyFilter = NotifyFilters.FileName Or NotifyFilters.LastWrite,
            .Filter = "*.*",
            .IncludeSubdirectories = True,
            .EnableRaisingEvents = True
        }
    End Sub

    Private Sub GameFolderWatcher_Changed(sender As Object, e As FileSystemEventArgs) Handles _gameFolderWatcher.Changed, _gameFolderWatcher.Created, _gameFolderWatcher.Deleted, _gameFolderWatcher.Renamed
        ' Only react to game file extensions
        If Not _allowedGameExtensions.Contains(Path.GetExtension(e.FullPath).ToLower()) Then Return

        Me.Invoke(Sub()
                      Dim currentSelection As String = If(ListBox1.SelectedItem?.ToString(), String.Empty)
                      Dim currentIndex As Integer = ListBox1.SelectedIndex

                      LoadGames()

                      If Not String.IsNullOrEmpty(currentSelection) Then
                          Dim newIndex = ListBox1.Items.IndexOf(currentSelection)
                          If newIndex >= 0 Then
                              ListBox1.SelectedIndex = newIndex
                          ElseIf currentIndex >= 0 AndAlso currentIndex < ListBox1.Items.Count Then
                              ListBox1.SelectedIndex = currentIndex
                          End If
                      End If
                  End Sub)
    End Sub

    Private Sub InitializeContextMenu()
        Dim contextMenu As New ContextMenuStrip()

        Dim renameItem As New ToolStripMenuItem("Rename")
        AddHandler renameItem.Click, AddressOf RenameGame
        contextMenu.Items.Add(renameItem)

        Dim moveItem As New ToolStripMenuItem("Move")
        AddHandler moveItem.Click, AddressOf MoveGame
        contextMenu.Items.Add(moveItem)

        Dim deleteItem As New ToolStripMenuItem("Delete")
        AddHandler deleteItem.Click, AddressOf DeleteGame
        contextMenu.Items.Add(deleteItem)

        ListBox1.ContextMenuStrip = contextMenu
        contextMenu.Enabled = _editModeEnabled ' Initial state matches Edit Mode
    End Sub

    Private Sub RenameGame(sender As Object, e As EventArgs)
        If Not _editModeEnabled OrElse ListBox1.SelectedIndex = -1 Then Return

        Dim oldName As String = ListBox1.SelectedItem.ToString()
        Dim newName As String = InputBox("Enter new name for the game:", "Rename Game", oldName)

        If String.IsNullOrWhiteSpace(newName) OrElse newName = oldName Then Return

        Try
            Dim oldGamePath As String = Directory.GetFiles(GameFolder, oldName & ".*", SearchOption.AllDirectories).
                                     FirstOrDefault(Function(f) _allowedGameExtensions.Contains(Path.GetExtension(f).ToLower()))
            If oldGamePath Is Nothing Then
                MessageBox.Show("Original game file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            Dim gameDirectory As String = Path.GetDirectoryName(oldGamePath)
            Dim ext As String = Path.GetExtension(oldGamePath)
            Dim newGamePath As String = Path.Combine(gameDirectory, newName & ext)

            If Not SafeRenameFile(oldGamePath, newGamePath) Then
                MessageBox.Show("Failed to rename game file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' Rename associated files
            For Each imgExt In _allowedImageExtensions
                Dim oldImagePath As String = Path.Combine(ImageFolder, oldName & imgExt)
                If File.Exists(oldImagePath) Then
                    SafeRenameFile(oldImagePath, Path.Combine(ImageFolder, newName & imgExt))
                End If
            Next

            Dim oldManualPath As String = Path.Combine(ManualFolder, oldName & ".txt")
            If File.Exists(oldManualPath) Then
                SafeRenameFile(oldManualPath, Path.Combine(ManualFolder, newName & ".txt"))
            End If

            LoadGames()

            Dim newIndex As Integer = ListBox1.Items.IndexOf(newName)
            If newIndex >= 0 Then
                ListBox1.SelectedIndex = newIndex
            End If

            MessageBox.Show("Game renamed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Error renaming game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub MoveGame(sender As Object, e As EventArgs)
        If Not _editModeEnabled OrElse ListBox1.SelectedIndex = -1 Then Return

        Dim gameName As String = ListBox1.SelectedItem.ToString()
        Dim oldGamePath As String = Directory.GetFiles(GameFolder, gameName & ".*", SearchOption.AllDirectories).
                                 FirstOrDefault(Function(f) _allowedGameExtensions.Contains(Path.GetExtension(f).ToLower()))

        If oldGamePath Is Nothing Then
            MessageBox.Show("Game file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Using folderDialog As New FolderBrowserDialog()
            folderDialog.Description = "Select destination folder for the game"
            folderDialog.SelectedPath = GameFolder

            If folderDialog.ShowDialog() = DialogResult.OK Then
                Try
                    If folderDialog.SelectedPath = Path.GetDirectoryName(oldGamePath) Then
                        MessageBox.Show("Game is already in the selected folder.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Return
                    End If

                    Directory.CreateDirectory(folderDialog.SelectedPath)

                    Dim newGamePath As String = Path.Combine(folderDialog.SelectedPath, gameName & Path.GetExtension(oldGamePath))
                    If Not SafeRenameFile(oldGamePath, newGamePath) Then
                        MessageBox.Show("Failed to move game file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return
                    End If

                    LoadGames()

                    Dim newIndex As Integer = ListBox1.Items.IndexOf(gameName)
                    If newIndex >= 0 Then
                        ListBox1.SelectedIndex = newIndex
                    End If

                    MessageBox.Show("Game moved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show($"Error moving game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub DeleteGame(sender As Object, e As EventArgs)
        If Not _editModeEnabled OrElse ListBox1.SelectedIndex = -1 Then Return

        Dim gameName As String = ListBox1.SelectedItem.ToString()
        Dim gamePath As String = Directory.GetFiles(GameFolder, gameName & ".*", SearchOption.AllDirectories).
                                    FirstOrDefault(Function(f) _allowedGameExtensions.Contains(Path.GetExtension(f).ToLower()))
        Dim currentIndex As Integer = ListBox1.SelectedIndex ' Store current index before deletion

        If gamePath Is Nothing Then
            MessageBox.Show("Game file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Dim result As DialogResult = MessageBox.Show(
        $"Are you sure you want to delete '{gameName}'?" & Environment.NewLine &
        "This will move the game file and associated files to the Recycle Bin." & Environment.NewLine &
        "This action cannot be undone!",
        "Confirm Deletion",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Warning,
        MessageBoxDefaultButton.Button2)

        If result <> DialogResult.OK Then Return

        Try
            PrepareForFileOperation()
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
            gamePath,
            Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
            Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin)

            ' Delete image files
            For Each ext In _allowedImageExtensions
                Dim imagePath As String = Path.Combine(ImageFolder, gameName & ext)
                If File.Exists(imagePath) Then
                    PrepareForFileOperation()
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                    imagePath,
                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin)
                End If
            Next

            ' Delete manual file
            Dim manualPath As String = Path.Combine(ManualFolder, gameName & ".txt")
            If File.Exists(manualPath) Then
                PrepareForFileOperation()
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                manualPath,
                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin)
            End If

            ' Store the count before reloading
            Dim itemCountBeforeDelete As Integer = ListBox1.Items.Count

            ' Reload the game list
            LoadGames()

            ' Determine which item to select after deletion
            If ListBox1.Items.Count > 0 Then
                ' If we deleted the last item, select the new last item
                If currentIndex >= ListBox1.Items.Count Then
                    ListBox1.SelectedIndex = ListBox1.Items.Count - 1
                Else
                    ' Otherwise select the item that was in the next position
                    ListBox1.SelectedIndex = currentIndex
                End If
            End If

            MessageBox.Show("Game deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Error deleting game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Original ZXGL functionality with enhancements

    Private Sub ResetBlockLabels()
        LblTapeCounter.Text = "Current Block: 0"
        LblZeroedBlock.Text = "Reset at Block: 0"
        zeroedBlock = 0
    End Sub

    Private Sub UpdateTapeCounter(sender As Object, e As EventArgs)
        If tzxPlayProcess IsNot Nothing AndAlso Not tzxPlayProcess.HasExited Then
            Dim currentBlock As Integer = GetCurrentBlock()
            LblTapeCounter.Text = $"Current Block: {currentBlock}"
        Else
            ListBox1.Enabled = True
            RunDeleteTempFiles()
            BtnStop.Text = "Stop"
            tapeCounterTimer.Stop()
        End If
    End Sub

    Private Sub StartScrollingText(text As String)
        scrollText = text & "    " ' Add gap for readability
        scrollIndex = 0
        If scrollText.Length > DISPLAY_CHARS Then
            scrollTimer.Start()
        Else
            scrollTimer.Stop()
            LblCurrentGame.Text = scrollText
        End If
    End Sub

    Private Sub ScrollTimer_Tick(sender As Object, e As EventArgs) Handles scrollTimer.Tick
        If scrollText.Length <= DISPLAY_CHARS Then
            LblCurrentGame.Text = scrollText
            scrollTimer.Stop()
            Return
        End If

        Dim loopedText As String = scrollText & scrollText.Substring(0, DISPLAY_CHARS)
        If scrollIndex + DISPLAY_CHARS > loopedText.Length Then scrollIndex = 0
        LblCurrentGame.Text = loopedText.Substring(scrollIndex, DISPLAY_CHARS)
        scrollIndex += 1
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.StartPosition = FormStartPosition.Manual
        Me.Location = New Point(Screen.PrimaryScreen.WorkingArea.Left, Screen.PrimaryScreen.WorkingArea.Top)
        Me.Text = "ZX Game Loader - v1.5.0"

        ' Initialize with Edit Mode OFF by default
        ToggleEditMode(False)

        RunDeleteTempFiles()
        ResetBlockLabels()
        InitializeMenuStrip()
        LoadSettings()
        InitializeGameFolderWatcher()
        InitializeContextMenu()

        ' Initialize the timer (but don't start it yet)
        tapeCounterTimer = New Timer() With {.Interval = 1000}
        AddHandler tapeCounterTimer.Tick, AddressOf UpdateTapeCounter

        ' Load games first
        LoadGames()

        ' Then try to restore last selected game if setting is enabled
        If File.Exists(_settingsFilePath) Then
            Dim lines() As String = File.ReadAllLines(_settingsFilePath)
            If lines.Length >= 5 AndAlso Boolean.Parse(lines(3)) AndAlso Not String.IsNullOrEmpty(lines(4)) Then
                Dim lastGame = lines(4)
                If _allGames.Contains(lastGame) Then
                    Dim index = ListBox1.Items.IndexOf(lastGame)
                    If index >= 0 Then
                        ListBox1.SelectedIndex = index
                        ' Force load the assets for the selected game
                        _lastSelectedGame = "" ' Reset to force reload
                        ListBox1_SelectedIndexChanged(ListBox1, EventArgs.Empty)
                        Return
                    End If
                End If
            End If
        End If

        ' If no last game or not found, select first item if available
        If ListBox1.Items.Count > 0 Then
            ListBox1.SelectedIndex = 0
        End If

        AddHandler TimerResetStatus.Tick, AddressOf ResetStatusMessage
    End Sub

    Private Sub InitializeMenuStrip()
        Dim menuStrip As New MenuStrip()

        ' File menu
        Dim fileMenuItem As New ToolStripMenuItem("File")
        Dim exitMenuItem As New ToolStripMenuItem("Exit")
        AddHandler exitMenuItem.Click, Sub(sender, e) Me.Close()
        fileMenuItem.DropDownItems.Add(exitMenuItem)
        menuStrip.Items.Add(fileMenuItem)

        ' Edit Mode menu
        Dim editModeMenuItem As New ToolStripMenuItem("Edit Mode")
        _editModeMenuItem = New ToolStripMenuItem("Editor Off")
        AddHandler _editModeMenuItem.Click, AddressOf EditModeToolStripMenuItem_Click
        editModeMenuItem.DropDownItems.Add(_editModeMenuItem)
        menuStrip.Items.Add(editModeMenuItem)

        ' Settings menu
        Dim settingsMenuItem As New ToolStripMenuItem("Settings")
        AddHandler settingsMenuItem.Click, AddressOf OpenSettingsForm
        menuStrip.Items.Add(settingsMenuItem)

        ' Help menu
        Dim helpMenuItem As New ToolStripMenuItem("Help")
        AddHandler helpMenuItem.Click, AddressOf ShowFormattedHelp
        menuStrip.Items.Add(helpMenuItem)

        ' About menu
        Dim aboutMenuItem As New ToolStripMenuItem("About")
        AddHandler aboutMenuItem.Click, AddressOf OpenAboutForm
        menuStrip.Items.Add(aboutMenuItem)

        Me.Controls.Add(menuStrip)
        menuStrip.Dock = DockStyle.Top
    End Sub

    Private Sub ResetStatusMessage(sender As Object, e As EventArgs)
        TimerResetStatus.Stop()
        LblTapeStatus.Text = "Ready"
    End Sub

    Private Sub OpenSettingsForm(sender As Object, e As EventArgs)
        Dim settingsForm As New SettingsForm()
        settingsForm.ShowDialog()
        LoadSettings()
        LoadGames()
    End Sub

    Private Sub ShowFormattedHelp(sender As Object, e As EventArgs)
        Dim helpForm As New HelpForm()
        helpForm.ShowDialog()
    End Sub

    Private Sub OpenAboutForm(sender As Object, e As EventArgs)
        Dim aboutForm As New AboutForm()
        aboutForm.ShowDialog()
    End Sub

    Private Sub LoadSettings()
        If Not File.Exists(_settingsFilePath) Then Return

        Dim lines() As String = File.ReadAllLines(_settingsFilePath)
        If lines.Length < 3 Then Return

        GameFolder = If(Path.IsPathRooted(lines(0)), lines(0), Path.Combine(Application.StartupPath, lines(0)))
        ImageFolder = If(Path.IsPathRooted(lines(1)), lines(1), Path.Combine(Application.StartupPath, lines(1)))
        ManualFolder = If(Path.IsPathRooted(lines(2)), lines(2), Path.Combine(Application.StartupPath, lines(2)))

        Directory.CreateDirectory(GameFolder)
        Directory.CreateDirectory(ImageFolder)
        Directory.CreateDirectory(ManualFolder)

        If _gameFolderWatcher Is Nothing Then
            InitializeGameFolderWatcher()
        Else
            _gameFolderWatcher.Path = GameFolder
        End If
    End Sub

    Private Sub LoadGames()
        If Not Directory.Exists(GameFolder) Then
            MessageBox.Show("Game folder not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        ' Store the current selection if it exists
        Dim currentSelection As String = If(ListBox1.SelectedItem?.ToString(), String.Empty)
        Dim currentIndex As Integer = ListBox1.SelectedIndex

        _allGames.Clear()
        _allGames.AddRange(
            Directory.GetFiles(GameFolder, "*.tzx", SearchOption.AllDirectories).
            Concat(Directory.GetFiles(GameFolder, "*.tap", SearchOption.AllDirectories)).
            Select(Function(f) Path.GetFileNameWithoutExtension(f)))
        _allGames.Sort()

        ApplyGameFilter()

        ' Try to maintain selection if possible
        If Not String.IsNullOrEmpty(currentSelection) Then
            Dim newIndex = ListBox1.Items.IndexOf(currentSelection)
            If newIndex >= 0 Then
                ListBox1.SelectedIndex = newIndex
                ' Force load the assets for the selected game
                _lastSelectedGame = "" ' Reset to force reload
                ListBox1_SelectedIndexChanged(ListBox1, EventArgs.Empty)
                Return
            End If
        End If

        If ListBox1.Items.Count > 0 Then
            ' If we had a previous selection that's no longer available,
            ' try to select the item at the same index if possible
            If currentIndex >= 0 AndAlso currentIndex < ListBox1.Items.Count Then
                ListBox1.SelectedIndex = currentIndex
            Else
                ListBox1.SelectedIndex = 0
            End If
            ' Force load the assets for the selected game
            _lastSelectedGame = "" ' Reset to force reload
            ListBox1_SelectedIndexChanged(ListBox1, EventArgs.Empty)
        End If
    End Sub

    Private Sub ApplyGameFilter()
        ListBox1.BeginUpdate()
        ListBox1.Items.Clear()

        If String.IsNullOrWhiteSpace(_currentSearchTerm) Then
            ListBox1.Items.AddRange(_allGames.ToArray())
        Else
            Dim searchTerms = _currentSearchTerm.ToLower().Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
            ListBox1.Items.AddRange(
                _allGames.Where(Function(game)
                                    Return searchTerms.All(Function(term) game.ToLower().Contains(term))
                                End Function).ToArray())
        End If

        If ListBox1.Items.Count > 0 Then ListBox1.SelectedIndex = 0
        ListBox1.EndUpdate()
    End Sub

    Private Sub TerminateAllChildProcesses()
        tapeCounterTimer.Stop()

        For Each proc In _childProcesses.ToList()
            Try
                If proc IsNot Nothing Then
                    Try
                        If Not proc.HasExited Then
                            proc.Kill()
                        End If
                    Catch ex As InvalidOperationException
                    Catch ex As NotSupportedException
                    Catch ex As Exception
                        Debug.WriteLine($"Error stopping process: {ex.Message}")
                    End Try
                    proc.Dispose()
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error disposing process: {ex.Message}")
            Finally
                If proc IsNot Nothing AndAlso _childProcesses.Contains(proc) Then
                    _childProcesses.Remove(proc)
                End If
            End Try
        Next
        _childProcesses.Clear()

        If tzxPlayProcess IsNot Nothing Then
            Try
                Try
                    If Not tzxPlayProcess.HasExited Then
                        tzxPlayProcess.Kill()
                    End If
                Catch ex As InvalidOperationException
                Catch ex As NotSupportedException
                Catch ex As Exception
                    Debug.WriteLine($"Error stopping tzxPlayProcess: {ex.Message}")
                End Try
            Finally
                tzxPlayProcess?.Dispose()
                tzxPlayProcess = Nothing
                If _childProcesses.Contains(tzxPlayProcess) Then
                    _childProcesses.Remove(tzxPlayProcess)
                End If
            End Try
        End If

        RunDeleteTempFiles()
    End Sub

    Private Sub RunDeleteTempFiles()
        Dim tempFilesToDelete As String() = {
        Path.Combine(Path.GetTempPath(), "current_block.txt"),
        Path.Combine(Path.GetTempPath(), "total_blocks.txt"),
        Path.Combine(Path.GetTempPath(), "next_block.txt"),
        Path.Combine(Path.GetTempPath(), "tzx_control.txt"),
        Path.Combine(Path.GetTempPath(), "save_status.txt"),
        Path.Combine(Path.GetTempPath(), "save_control.txt")
    }

        For Each filePath In tempFilesToDelete
            Try
                If File.Exists(filePath) Then
                    File.Delete(filePath)
                End If
            Catch ex As Exception
                Debug.WriteLine($"Failed to delete temp file {filePath}: {ex.Message}")
            End Try
        Next
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        SaveLastGame()
        TerminateAllChildProcesses()

        If _gameFolderWatcher IsNot Nothing Then
            _gameFolderWatcher.Dispose()
        End If
    End Sub

    Private Sub SaveLastGame()
        If Not File.Exists(_settingsFilePath) Then Return

        Dim lines = File.ReadAllLines(_settingsFilePath).ToList()
        If lines.Count < 4 OrElse Not Boolean.Parse(lines(3)) Then Return

        Dim currentGame = If(ListBox1.SelectedItem?.ToString(), "")

        If lines.Count >= 5 Then
            lines(4) = currentGame
        Else
            lines.Add(currentGame)
        End If

        File.WriteAllLines(_settingsFilePath, lines)
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If ListBox1.SelectedIndex = -1 Then Exit Sub

        ' Stop the timer if it's running
        If tapeCounterTimer.Enabled Then
            tapeCounterTimer.Stop()
        End If

        Dim selectedGame As String = ListBox1.SelectedItem.ToString()
        StartScrollingText(selectedGame)

        If selectedGame.Length > DISPLAY_CHARS Then
            scrollTimer.Start()
        End If

        ResetBlockLabels()
        RunDeleteTempFiles()

        ' Always reload assets when selection changes
        LoadGameAssets(selectedGame)
        _lastSelectedGame = selectedGame

        ' If a game is playing, restart the timer
        If tzxPlayProcess IsNot Nothing AndAlso Not tzxPlayProcess.HasExited Then
            tapeCounterTimer.Start()
        End If
    End Sub

    Private Sub LoadGameAssets(gameName As String)
        ' Load game image
        Dim imagePath = _allowedImageExtensions.
            Select(Function(ext) Path.Combine(ImageFolder, gameName & ext)).
            FirstOrDefault(Function(path) File.Exists(path))

        Try
            ReleaseImageResources()
            If imagePath IsNot Nothing Then
                ' Create a copy of the image to avoid file locking
                Dim imageBytes As Byte() = File.ReadAllBytes(imagePath)
                Using ms As New MemoryStream(imageBytes)
                    PictureBox1.Image = Image.FromStream(ms)
                End Using
                _currentImagePath = imagePath
                PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
            Else
                PictureBox1.Image = Nothing
                PictureBox1.SizeMode = PictureBoxSizeMode.Normal
            End If
        Catch ex As Exception
            PictureBox1.Image = Nothing
            PictureBox1.SizeMode = PictureBoxSizeMode.Normal
        End Try

        ' Load manual
        Dim manualPath As String = Path.Combine(ManualFolder, gameName & ".txt")
        Try
            If File.Exists(manualPath) Then
                RichTextBox1.Text = File.ReadAllText(manualPath)
                _currentManualPath = manualPath
            Else
                RichTextBox1.Text = "No manual available."
                _currentManualPath = ""
            End If
        Catch ex As Exception
            RichTextBox1.Text = "Unable to load manual"
            _currentManualPath = ""
        End Try
    End Sub

    Private Sub SearchBox_TextChanged(sender As Object, e As EventArgs) Handles SearchBox.TextChanged
        Dim newSearchTerm = SearchBox.Text.Trim().ToLower()
        If newSearchTerm <> _currentSearchTerm Then
            _currentSearchTerm = newSearchTerm
            ApplyGameFilter()
        End If
    End Sub

    Private Sub BtnPlay_Click(sender As Object, e As EventArgs) Handles BtnPlay.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            If ListBox1.SelectedIndex = -1 Then
                MessageBox.Show("Please select a game first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim selectedGame As String = ListBox1.SelectedItem.ToString()
            Dim gameFile = Directory.GetFiles(GameFolder, selectedGame & ".*", SearchOption.AllDirectories).
             FirstOrDefault(Function(f) f.EndsWith(".tzx") Or f.EndsWith(".tap"))

            If gameFile Is Nothing Then
                MessageBox.Show("Game file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            Try
                If File.Exists(controlFilePath) Then File.Delete(controlFilePath)

                Dim startInfo As New ProcessStartInfo() With {
                .FileName = Path.Combine(Application.StartupPath, "python", "python.exe"),
                .Arguments = $"""{Path.Combine(Application.StartupPath, "tzxplay.py")}"" ""{gameFile}""",
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardError = True,
                .RedirectStandardOutput = True
            }

                tzxPlayProcess = Process.Start(startInfo)
                _childProcesses.Add(tzxPlayProcess)

                AddHandler tzxPlayProcess.ErrorDataReceived, Sub(s, args)
                                                                 If Not String.IsNullOrEmpty(args.Data) Then
                                                                     Debug.WriteLine("Python Error: " & args.Data)
                                                                 End If
                                                             End Sub

                AddHandler tzxPlayProcess.OutputDataReceived, Sub(s, args)
                                                                  If Not String.IsNullOrEmpty(args.Data) Then
                                                                      Debug.WriteLine("Python Output: " & args.Data)
                                                                  End If
                                                              End Sub

                tzxPlayProcess.BeginErrorReadLine()
                tzxPlayProcess.BeginOutputReadLine()

                ListBox1.Enabled = False
                tapeCounter = 0
                totalBlocks = GetTotalBlocks()
                isStopped = False
                BtnStop.Text = "Stop"
                zeroedBlock = 0
                LblZeroedBlock.Text = "Reset at Block: 0"

                ' Ensure the timer is properly started
                tapeCounterTimer.Stop() ' Stop if already running
                tapeCounterTimer.Start() ' Start fresh

                ' Force an immediate UI update
                UpdateTapeCounter(Me, EventArgs.Empty)

            Catch ex As Exception
                MessageBox.Show($"Failed to start Python script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                tapeCounterTimer.Stop() ' Ensure timer is stopped if there's an error
            End Try
        Else
            If isStopped Then
                File.WriteAllText(controlFilePath, "resume")
                isStopped = False
                BtnStop.Text = "Stop"
                tapeCounterTimer.Start() ' Restart the timer when resuming

                ' Force an immediate UI update
                UpdateTapeCounter(Me, EventArgs.Empty)
            End If
        End If
    End Sub

    Private Sub BtnStop_Click(sender As Object, e As EventArgs) Handles BtnStop.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        File.WriteAllText(controlFilePath, "pause")
        isStopped = True
        tapeCounterTimer.Stop()
    End Sub

    Private Async Sub BtnRewind_Click(sender As Object, e As EventArgs) Handles BtnRewind.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        File.WriteAllText(controlFilePath, "pause")
        isStopped = True
        tapeCounterTimer.Stop()

        Await Task.Delay(100)

        Dim currentBlock As Integer = GetCurrentBlock()
        currentBlock = Math.Max(0, currentBlock - 1)
        File.WriteAllText(controlFilePath, $"rewind:{currentBlock}")
        LblTapeCounter.Text = $"Current Block: {currentBlock}"
    End Sub

    Private Async Sub BtnFastForward_Click(sender As Object, e As EventArgs) Handles BtnFastForward.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        File.WriteAllText(controlFilePath, "pause")
        isStopped = True
        tapeCounterTimer.Stop()

        Await Task.Delay(100)

        If File.Exists(nextBlockFilePath) Then
            Try
                Dim targetBlock As Integer = Integer.Parse(File.ReadAllText(nextBlockFilePath))
                File.WriteAllText(controlFilePath, $"jump:{targetBlock}")
                LblTapeCounter.Text = $"Current Block: {targetBlock}"
            Catch ex As Exception
                Debug.WriteLine($"Fast-forward error: {ex.Message}")
            End Try
        End If
    End Sub

    Private Sub BtnEject_Click(sender As Object, e As EventArgs) Handles BtnEject.Click
        ' Stop playback if running
        If tzxPlayProcess IsNot Nothing AndAlso Not tzxPlayProcess.HasExited Then
            File.WriteAllText(controlFilePath, "stop")
            Try
                If Not tzxPlayProcess.HasExited Then
                    tzxPlayProcess.Kill()
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error stopping process: {ex.Message}")
            Finally
                tzxPlayProcess?.Dispose()
                tzxPlayProcess = Nothing
                If _childProcesses.Contains(tzxPlayProcess) Then
                    _childProcesses.Remove(tzxPlayProcess)
                End If
            End Try
        End If

        isStopped = False
        ListBox1.Enabled = True
        tapeCounterTimer.Stop()
        ResetBlockLabels()
        RunDeleteTempFiles()
    End Sub

    Private Sub BtnCounterReset_Click(sender As Object, e As EventArgs) Handles BtnCounterReset.Click
        zeroedBlock = GetCurrentBlock()
        LblZeroedBlock.Text = $"Reset at Block: {zeroedBlock}"
    End Sub

    Private Sub Jump_Click(sender As Object, e As EventArgs) Handles Jump.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If zeroedBlock > 0 Then
            File.WriteAllText(controlFilePath, $"jump:{zeroedBlock}")
            LblTapeCounter.Text = $"Current Block: {zeroedBlock}"
        Else
            MessageBox.Show("No reset block position set. Use Counter Reset first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub BtnSaveState_Click(sender As Object, e As EventArgs) Handles BtnSaveState.Click
        If ListBox1.SelectedIndex = -1 Then
            LblTapeStatus.Text = "No game selected"
            Return
        End If

        If isRecording Then
            File.WriteAllText(saveControlFile, "stop")
            isRecording = False
            BtnSaveState.Text = "Save"
            Return
        End If

        Dim selectedGame = ListBox1.SelectedItem.ToString()
        LblTapeStatus.Text = "Preparing to save game..."
        Application.DoEvents()

        Try
            If File.Exists(saveStatusFile) Then
                Try
                    File.Delete(saveStatusFile)
                Catch ex As IOException
                    Threading.Thread.Sleep(100)
                    File.Delete(saveStatusFile)
                End Try
            End If

            Dim startInfo As New ProcessStartInfo() With {
                .FileName = Path.Combine(Application.StartupPath, "python", "python.exe"),
                .Arguments = $"""{Path.Combine(Application.StartupPath, "savegame.py")}"" save ""{selectedGame}""",
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True
            }

            Dim saveProcess = Process.Start(startInfo)
            _childProcesses.Add(saveProcess)
            AddHandler saveProcess.OutputDataReceived, Sub(s, args)
                                                           If Not String.IsNullOrEmpty(args.Data) Then
                                                               Me.Invoke(Sub()
                                                                             LblTapeStatus.Text = "Status: " & args.Data
                                                                         End Sub)
                                                           End If
                                                       End Sub

            LblTapeStatus.Text = "Waiting for signal..."
            TimerSaveStatus.Start()
            isRecording = True
            BtnSaveState.Text = "Stop"
        Catch ex As Exception
            LblTapeStatus.Text = "Save failed to start: " & ex.Message
        End Try
    End Sub

    Private Sub BtnLoadState_Click(sender As Object, e As EventArgs) Handles BtnLoadState.Click
        If ListBox1.SelectedIndex = -1 Then
            LblTapeStatus.Text = "No game selected"
            Return
        End If

        Dim selectedGame = ListBox1.SelectedItem.ToString()
        Dim cleanName = New String(selectedGame.Where(Function(c) Char.IsLetterOrDigit(c) Or c = " " Or c = "_" Or c = "-").ToArray()).Trim()
        Dim saveDir = Path.Combine(Application.StartupPath, "Saves", cleanName)

        If Not Directory.Exists(saveDir) Then
            LblTapeStatus.Text = "No saves found for this game"
            Return
        End If

        Using openFileDialog As New OpenFileDialog()
            openFileDialog.InitialDirectory = saveDir
            openFileDialog.Filter = "Save games (*.wav)|*.wav"
            openFileDialog.Title = "Select game save to load"

            If openFileDialog.ShowDialog() = DialogResult.OK Then
                Try
                    If File.Exists(saveStatusFile) Then
                        Try
                            File.Delete(saveStatusFile)
                        Catch ex As IOException
                            Threading.Thread.Sleep(100)
                            File.Delete(saveStatusFile)
                        End Try
                    End If

                    LblTapeStatus.Text = "Preparing to load save..."
                    Application.DoEvents()

                    Dim startInfo As New ProcessStartInfo() With {
                        .FileName = Path.Combine(Application.StartupPath, "python", "python.exe"),
                        .Arguments = $"""{Path.Combine(Application.StartupPath, "savegame.py")}"" load ""{selectedGame}"" ""{openFileDialog.FileName}""",
                        .UseShellExecute = False,
                        .CreateNoWindow = True,
                        .RedirectStandardOutput = True,
                        .RedirectStandardError = True
                    }

                    Dim loadProcess = Process.Start(startInfo)
                    _childProcesses.Add(loadProcess)
                    AddHandler loadProcess.OutputDataReceived, Sub(s, args)
                                                                   If Not String.IsNullOrEmpty(args.Data) Then
                                                                       Me.Invoke(Sub()
                                                                                     LblTapeStatus.Text = "Status: " & args.Data
                                                                                 End Sub)
                                                                   End If
                                                               End Sub

                    LblTapeStatus.Text = "Loading game save..."
                    TimerSaveStatus.Start()
                Catch ex As Exception
                    LblTapeStatus.Text = "Load failed to start: " & ex.Message
                End Try
            End If
        End Using
    End Sub

    Private Sub TimerSaveStatus_Tick(sender As Object, e As EventArgs) Handles TimerSaveStatus.Tick
        If Not File.Exists(saveStatusFile) Then Return

        Dim status As String = ""
        Try
            For i As Integer = 1 To 5
                Try
                    status = File.ReadAllText(saveStatusFile)
                    Exit For
                Catch ex As IOException When i < 5
                    Threading.Thread.Sleep(100)
                End Try
            Next

            status = status.ToLower()
            Me.Invoke(Sub()
                          If status.StartsWith("recording_complete:") Then
                              LblTapeStatus.Text = "Game saved successfully!"
                              TimerSaveStatus.Stop()
                              TimerResetStatus.Start()
                              isRecording = False
                              BtnSaveState.Text = "Save"
                          ElseIf status.StartsWith("playback_complete") Then
                              LblTapeStatus.Text = "Save loaded successfully!"
                              TimerSaveStatus.Stop()
                              TimerResetStatus.Start()
                          ElseIf status.StartsWith("error:") Then
                              LblTapeStatus.Text = "Error: " & status.Substring(6)
                              TimerSaveStatus.Stop()
                              TimerResetStatus.Start()
                              isRecording = False
                              BtnSaveState.Text = "Save"
                          Else
                              Select Case status
                                  Case "waiting_for_signal"
                                      LblTapeStatus.Text = "Waiting for signal..."
                                  Case "signal_detected"
                                      LblTapeStatus.Text = "Signal detected..."
                                  Case "recording"
                                      LblTapeStatus.Text = "Saving game..."
                                  Case "processing"
                                      LblTapeStatus.Text = "Processing save data..."
                                  Case "playback_started"
                                      LblTapeStatus.Text = "Loading save data..."
                                  Case "no_signal_detected"
                                      LblTapeStatus.Text = "No signal detected - try again"
                                      TimerSaveStatus.Stop()
                                      isRecording = False
                                      BtnSaveState.Text = "Save"
                                  Case Else
                                      LblTapeStatus.Text = "Status: " & status
                              End Select
                          End If
                      End Sub)
        Catch ex As Exception
            Me.Invoke(Sub()
                          LblTapeStatus.Text = "Error reading status"
                          TimerSaveStatus.Stop()
                          isRecording = False
                          BtnSaveState.Text = "Save"
                      End Sub)
        End Try
    End Sub

    Private Function GetCurrentBlock() As Integer
        Dim currentBlockFile As String = Path.Combine(Path.GetTempPath(), "current_block.txt")
        If Not File.Exists(currentBlockFile) Then Return 0

        Try
            Return Integer.Parse(File.ReadAllText(currentBlockFile))
        Catch
            Return 0
        End Try
    End Function

    Private Function GetTotalBlocks() As Integer
        Dim totalBlocksFile As String = Path.Combine(Path.GetTempPath(), "total_blocks.txt")
        If Not File.Exists(totalBlocksFile) Then Return 0

        Try
            Return Integer.Parse(File.ReadAllText(totalBlocksFile))
        Catch
            Return 0
        End Try
    End Function

    ' Drag-Drop functionality for PictureBox (images)
    Private Sub PictureBox1_DragEnter(sender As Object, e As DragEventArgs) Handles PictureBox1.DragEnter
        If Not _editModeEnabled Then
            e.Effect = DragDropEffects.None
            Return
        End If

        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
            If files.Length = 1 AndAlso _allowedImageExtensions.Contains(Path.GetExtension(files(0)).ToLower()) Then
                e.Effect = DragDropEffects.Copy
            Else
                e.Effect = DragDropEffects.None
            End If
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub PictureBox1_DragDrop(sender As Object, e As DragEventArgs) Handles PictureBox1.DragDrop
        If Not _editModeEnabled OrElse ListBox1.SelectedIndex = -1 Then
            MessageBox.Show("Please enable Edit Mode and select a game first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        If files.Length = 0 Then Return

        Try
            Dim selectedGame As String = ListBox1.SelectedItem.ToString()
            Dim sourceFile = files(0)
            Dim destPath As String = Path.Combine(ImageFolder, selectedGame & Path.GetExtension(sourceFile).ToLower())

            SafeDeleteGameImage(selectedGame)

            File.Copy(sourceFile, destPath, True)
            PictureBox1.Image = Image.FromFile(destPath)
            _currentImagePath = destPath
            MessageBox.Show($"Image saved as {Path.GetFileName(destPath)}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Drag-Drop functionality for RichTextBox (manuals)
    Private Sub RichTextBox1_DragEnter(sender As Object, e As DragEventArgs) Handles RichTextBox1.DragEnter
        If Not _editModeEnabled Then
            e.Effect = DragDropEffects.None
            Return
        End If

        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
            If files.Length = 1 AndAlso _allowedManualExtensions.Contains(Path.GetExtension(files(0)).ToLower()) Then
                e.Effect = DragDropEffects.Copy
            Else
                e.Effect = DragDropEffects.None
            End If
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub RichTextBox1_DragDrop(sender As Object, e As DragEventArgs) Handles RichTextBox1.DragDrop
        If Not _editModeEnabled OrElse ListBox1.SelectedIndex = -1 Then
            MessageBox.Show("Please enable Edit Mode and select a game first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        If files.Length = 0 Then Return

        Try
            Dim selectedGame As String = ListBox1.SelectedItem.ToString()
            Dim destPath As String = Path.Combine(ManualFolder, selectedGame & ".txt")

            SafeDeleteManual(selectedGame)

            File.Copy(files(0), destPath, True)
            RichTextBox1.Text = File.ReadAllText(destPath)
            _currentManualPath = destPath
            MessageBox.Show($"Manual saved as {Path.GetFileName(destPath)}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Error saving manual: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class