Imports System.IO
Imports System.Diagnostics
Imports System.Drawing

Public Class Form1
    ' Folder paths and settings
    Dim GameFolder As String = Path.Combine(Application.StartupPath, "Games")
    Dim ImageFolder As String = Path.Combine(Application.StartupPath, "Images")
    Dim ManualFolder As String = Path.Combine(Application.StartupPath, "Manuals")
    Private ReadOnly _settingsFilePath As String = Path.Combine(Application.StartupPath, "settings.txt")
    Private _lastSelectedGame As String = String.Empty
    Private _allGames As New List(Of String)()
    Private _currentSearchTerm As String = ""

    ' Process and playback control
    Private tzxPlayProcess As Process = Nothing
    Private isPaused As Boolean = False
    Private _childProcesses As New List(Of Process)()
    Private controlFilePath As String = Path.Combine(Path.GetTempPath(), "tzx_control.txt")
    Private totalBlocksFilePath As String = Path.Combine(Path.GetTempPath(), "total_blocks.txt")
    Private nextBlockFilePath As String = Path.Combine(Path.GetTempPath(), "next_block.txt")
    Private saveStatusFile As String = Path.Combine(Path.GetTempPath(), "save_status.txt")
    Private saveControlFile As String = Path.Combine(Path.GetTempPath(), "save_control.txt")

    ' Counters and timers
    Private tapeCounter As Integer = 0
    Private totalBlocks As Integer = 0
    Private zeroedBlock As Integer = 0
    Private tapeCounterTimer As New Timer() With {.Interval = 1000}
    Private WithEvents TimerSaveStatus As New Timer() With {.Interval = 500}
    Private WithEvents TimerResetStatus As New Timer() With {.Interval = 10000, .Enabled = False}
    Private isRecording As Boolean = False

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
            BtnPause.Text = "Pause"
            tapeCounterTimer.Stop()
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.StartPosition = FormStartPosition.Manual
        Me.Location = New Point(Screen.PrimaryScreen.WorkingArea.Left, Screen.PrimaryScreen.WorkingArea.Top)
        Me.Text = "ZX Game Loader"

        RunDeleteTempFiles()
        ResetBlockLabels()
        InitializeMenuStrip()
        LoadSettings()

        If File.Exists(_settingsFilePath) Then
            Dim lines() As String = File.ReadAllLines(_settingsFilePath)
            If lines.Length >= 5 AndAlso Boolean.Parse(lines(3)) Then
                _lastSelectedGame = lines(4)
            End If
        End If

        LoadGames()
        AddHandler tapeCounterTimer.Tick, AddressOf UpdateTapeCounter
        AddHandler TimerResetStatus.Tick, AddressOf ResetStatusMessage
    End Sub

    Private Sub ResetStatusMessage(sender As Object, e As EventArgs)
        TimerResetStatus.Stop()
        LblTapeStatus.Text = "Ready"
    End Sub

    Private Sub InitializeMenuStrip()
        Dim menuStrip As New MenuStrip()

        ' File Menu
        Dim fileMenuItem As New ToolStripMenuItem("File")

        ' Exit option
        Dim exitMenuItem As New ToolStripMenuItem("Exit")
        AddHandler exitMenuItem.Click, Sub(sender, e) Me.Close()
        fileMenuItem.DropDownItems.Add(exitMenuItem)

        menuStrip.Items.Add(fileMenuItem)

        ' Settings Menu
        Dim settingsMenuItem As New ToolStripMenuItem("Settings")
        AddHandler settingsMenuItem.Click, AddressOf OpenSettingsForm
        menuStrip.Items.Add(settingsMenuItem)

        ' Help Menu
        Dim helpMenuItem As New ToolStripMenuItem("Help")
        AddHandler helpMenuItem.Click, AddressOf ShowFormattedHelp
        menuStrip.Items.Add(helpMenuItem)

        Me.Controls.Add(menuStrip)
        menuStrip.Dock = DockStyle.Top
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

    Private Sub LoadSettings()
        If Not File.Exists(_settingsFilePath) Then Return

        Dim lines() As String = File.ReadAllLines(_settingsFilePath)
        If lines.Length < 3 Then Return

        GameFolder = If(Path.IsPathRooted(lines(0)), lines(0), Path.Combine(Application.StartupPath, lines(0)))
        ImageFolder = If(Path.IsPathRooted(lines(1)), lines(1), Path.Combine(Application.StartupPath, lines(1)))
        ManualFolder = If(Path.IsPathRooted(lines(2)), lines(2), Path.Combine(Application.StartupPath, lines(2)))
    End Sub

    Private Sub LoadGames()
        If Not Directory.Exists(GameFolder) Then
            MessageBox.Show("Game folder not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        _allGames.Clear()
        _allGames.AddRange(
            Directory.GetFiles(GameFolder, "*.tzx", SearchOption.AllDirectories).
            Concat(Directory.GetFiles(GameFolder, "*.tap", SearchOption.AllDirectories)).
            Select(Function(f) Path.GetFileNameWithoutExtension(f)))
        _allGames.Sort()

        ApplyGameFilter()

        If Not String.IsNullOrEmpty(_lastSelectedGame) Then
            Dim index = ListBox1.Items.IndexOf(_lastSelectedGame)
            If index >= 0 Then
                ListBox1.SelectedIndex = index
                ListBox1_SelectedIndexChanged(ListBox1, EventArgs.Empty)
                Return
            End If
        End If

        If ListBox1.Items.Count > 0 Then
            ListBox1.SelectedIndex = 0
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
                        ' Process already exited or not associated
                    Catch ex As NotSupportedException
                        ' Process is on remote computer
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
                    ' Process already exited or not associated
                Catch ex As NotSupportedException
                    ' Process is on remote computer
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
        Dim batchContent = "@echo off" & Environment.NewLine &
                         "del ""%TEMP%\current_block.txt"" /F /Q" & Environment.NewLine &
                         "del ""%TEMP%\total_blocks.txt"" /F /Q" & Environment.NewLine &
                         "del ""%TEMP%\next_block.txt"" /F /Q" & Environment.NewLine &
                         "del ""%TEMP%\tzx_control.txt"" /F /Q" & Environment.NewLine &
                         "del ""%TEMP%\save_status.txt"" /F /Q" & Environment.NewLine &
                         "del ""%TEMP%\save_control.txt"" /F /Q"

        Dim batchPath = Path.Combine(Path.GetTempPath(), "DeleteTempFiles.bat")
        File.WriteAllText(batchPath, batchContent)

        Dim psi As New ProcessStartInfo("cmd.exe", "/c """ & batchPath & """") With {
            .UseShellExecute = False,
            .CreateNoWindow = True
        }

        Using process As Process = Process.Start(psi)
            process.WaitForExit(1000)
        End Using

        Try
            File.Delete(batchPath)
        Catch
        End Try
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        SaveLastGame()
        TerminateAllChildProcesses()
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

        ResetBlockLabels()
        Dim selectedGame As String = ListBox1.SelectedItem.ToString()
        LblCurrentGame.Text = selectedGame
        RunDeleteTempFiles()

        ' Load game image
        Try
            PictureBox1.Image = Nothing
            Dim imageExtensions As String() = {".png", ".jpg", ".gif"}
            Dim imagePath = imageExtensions.
                Select(Function(ext) Path.Combine(ImageFolder, selectedGame & ext)).
                FirstOrDefault(Function(path) File.Exists(path))

            If imagePath IsNot Nothing Then
                PictureBox1.Image = Image.FromFile(imagePath)
            End If
            PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
        Catch ex As Exception
            PictureBox1.Image = Nothing
            PictureBox1.SizeMode = PictureBoxSizeMode.Normal
        End Try

        ' Load manual
        Dim manualPath As String = Path.Combine(ManualFolder, selectedGame & ".txt")
        RichTextBox1.Text = If(File.Exists(manualPath), File.ReadAllText(manualPath), "No manual available.")
    End Sub

    Private Sub SearchBox_TextChanged(sender As Object, e As EventArgs) Handles SearchBox.TextChanged
        Dim newSearchTerm = SearchBox.Text.Trim().ToLower()
        If newSearchTerm <> _currentSearchTerm Then
            _currentSearchTerm = newSearchTerm
            ApplyGameFilter()
        End If
    End Sub

    Private Sub BtnLoadGame_Click(sender As Object, e As EventArgs) Handles BtnLoadGame.Click
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
            tapeCounterTimer.Start()
            totalBlocks = GetTotalBlocks()
            isPaused = False
            BtnPause.Text = "Pause"
            zeroedBlock = 0
            LblZeroedBlock.Text = "Reset at Block 0"
        Catch ex As Exception
            MessageBox.Show($"Failed to start Python script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnPause_Click(sender As Object, e As EventArgs) Handles BtnPause.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim command As String = If(isPaused, "resume", "pause")
        File.WriteAllText(controlFilePath, command)
        isPaused = Not isPaused
        BtnPause.Text = If(isPaused, "Resume", "Pause")

        If isPaused Then
            tapeCounterTimer.Stop()
        Else
            tapeCounterTimer.Start()
        End If
    End Sub

    Private Sub BtnStop_Click(sender As Object, e As EventArgs) Handles BtnStop.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

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

        isPaused = False
        BtnPause.Text = "Pause"
        ListBox1.Enabled = True
        tapeCounterTimer.Stop()
        ResetBlockLabels()
    End Sub

    Private Sub BtnRewind_Click(sender As Object, e As EventArgs) Handles BtnRewind.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim currentBlock As Integer = GetCurrentBlock()
        currentBlock = Math.Max(0, currentBlock - 1)
        File.WriteAllText(controlFilePath, $"rewind:{currentBlock}")
        LblTapeCounter.Text = $"Current Block: {currentBlock}"
    End Sub

    Private Sub BtnFastForward_Click(sender As Object, e As EventArgs) Handles BtnFastForward.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

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

    Private Sub BtnSetZero_Click(sender As Object, e As EventArgs) Handles BtnSetZero.Click
        zeroedBlock = GetCurrentBlock()
        LblZeroedBlock.Text = $"Reset at Block {zeroedBlock}"
    End Sub

    Private Sub BtnSaveState_Click(sender As Object, e As EventArgs) Handles BtnSaveState.Click
        If ListBox1.SelectedIndex = -1 Then
            LblTapeStatus.Text = "No game selected"
            Return
        End If

        If isRecording Then
            ' Stop recording
            File.WriteAllText(saveControlFile, "stop")
            isRecording = False
            BtnSaveState.Text = "Save"
            Return
        End If

        ' Start recording
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

            LblTapeStatus.Text = "Waiting for signal from Spectrum..."
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
                                      LblTapeStatus.Text = "Waiting for signal from Spectrum..."
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
End Class