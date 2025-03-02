Imports System.IO
Imports System.Diagnostics
Imports System.Text
Imports System.Drawing

Public Class Form1
    Dim GameFolder As String = Path.Combine(Application.StartupPath, "Games\Retail")
    Dim ImageFolder As String = Path.Combine(Application.StartupPath, "Images")
    Dim ManualFolder As String = Path.Combine(Application.StartupPath, "Manuals")
    Private tzxPlayProcess As Process = Nothing
    Private isPaused As Boolean = False
    Private controlFilePath As String = Path.Combine(Path.GetTempPath(), "tzx_control.txt")
    Private totalBlocksFilePath As String = Path.Combine(Path.GetTempPath(), "total_blocks.txt")
    Private tapeCounter As Integer = 0
    Private tapeCounterTimer As New Timer()
    Private totalBlocks As Integer = 0 ' Stores the total number of blocks
    Private zeroedBlock As Integer = 0 ' Stores the block number when Set 000 is pressed

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Text = "ZX Spectrum Game Loader"
        InitializeMenuStrip() ' Initialize the menu bar
        LoadSettings() ' Load folder paths from settings.txt
        LoadGames()

        ' Set up the tape counter timer
        tapeCounterTimer.Interval = 1000 ' Update every second
        AddHandler tapeCounterTimer.Tick, AddressOf UpdateTapeCounter
    End Sub

    Private Sub InitializeMenuStrip()
        Dim menuStrip As New MenuStrip()
        Dim settingsMenuItem As New ToolStripMenuItem("Settings")
        AddHandler settingsMenuItem.Click, AddressOf OpenSettingsForm
        menuStrip.Items.Add(settingsMenuItem)
        Me.Controls.Add(menuStrip)
        menuStrip.Dock = DockStyle.Top
    End Sub

    Private Sub OpenSettingsForm(sender As Object, e As EventArgs)
        Dim settingsForm As New SettingsForm()
        settingsForm.ShowDialog()
        ' Reload games and other resources after settings are updated
        LoadSettings()
        LoadGames()
    End Sub

    Private Sub LoadSettings()
        Dim settingsFilePath As String = Path.Combine(Application.StartupPath, "settings.txt")
        If File.Exists(settingsFilePath) Then
            Dim lines() As String = File.ReadAllLines(settingsFilePath)
            If lines.Length >= 3 Then
                GameFolder = Path.Combine(Application.StartupPath, lines(0))
                ImageFolder = Path.Combine(Application.StartupPath, lines(1))
                ManualFolder = Path.Combine(Application.StartupPath, lines(2))
            End If
        End If
    End Sub

    Private Sub LoadGames()
        If Directory.Exists(GameFolder) Then
            ListBox1.Items.Clear()
            Dim games As New List(Of String)

            For Each file In Directory.GetFiles(GameFolder, "*.tzx", SearchOption.AllDirectories).Concat(Directory.GetFiles(GameFolder, "*.tap", SearchOption.AllDirectories))
                games.Add(Path.GetFileNameWithoutExtension(file))
            Next

            games.Sort()
            ListBox1.Items.AddRange(games.ToArray())

            If ListBox1.Items.Count > 0 Then
                ListBox1.SelectedIndex = 0
                ListBox1_SelectedIndexChanged(ListBox1, EventArgs.Empty)
            End If
        Else
            MessageBox.Show("Game folder not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If ListBox1.SelectedIndex = -1 Then Exit Sub
        Dim selectedGame As String = ListBox1.SelectedItem.ToString()

        ' Update the current game label
        LblCurrentGame.Text = selectedGame

        ' Run DeleteTempFiles.bat silently
        RunDeleteTempFiles()

        ' Load game image
        Dim imagePathPNG As String = Path.Combine(ImageFolder, selectedGame & ".png")
        Dim imagePathJPG As String = Path.Combine(ImageFolder, selectedGame & ".jpg")
        Dim imagePathGIF As String = Path.Combine(ImageFolder, selectedGame & ".gif")

        If PictureBox1 IsNot Nothing Then
            Try
                If File.Exists(imagePathPNG) Then
                    PictureBox1.Image = Image.FromFile(imagePathPNG)
                ElseIf File.Exists(imagePathJPG) Then
                    PictureBox1.Image = Image.FromFile(imagePathJPG)
                ElseIf File.Exists(imagePathGIF) Then
                    PictureBox1.Image = Image.FromFile(imagePathGIF)
                Else
                    PictureBox1.Image = Nothing
                End If
                PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
            Catch ex As Exception
                PictureBox1.Image = Nothing
                PictureBox1.SizeMode = PictureBoxSizeMode.Normal
            End Try
        End If

        ' Load Manual (TXT format)
        Dim manualPath As String = Path.Combine(ManualFolder, selectedGame & ".txt")
        If RichTextBox1 IsNot Nothing Then
            If File.Exists(manualPath) Then
                RichTextBox1.Text = File.ReadAllText(manualPath)
            Else
                RichTextBox1.Text = "No manual available."
            End If
        End If
    End Sub

    Private Sub RunDeleteTempFiles()
        ' Create the batch file content
        Dim batchFilePath As String = Path.Combine(Path.GetTempPath(), "DeleteTempFiles.bat")
        Dim batchFileContent As String = "@echo off" & Environment.NewLine &
                                        "set ""TempPath=%LocalAppData%\Temp""" & Environment.NewLine &
                                        "del ""%TempPath%\current_block.txt"" /F /Q" & Environment.NewLine &
                                        "del ""%TempPath%\total_blocks.txt"" /F /Q" & Environment.NewLine &
                                        "exit"

        ' Write the batch file
        File.WriteAllText(batchFilePath, batchFileContent)

        ' Run the batch file silently
        Dim processStartInfo As New ProcessStartInfo()
        processStartInfo.FileName = "cmd.exe"
        processStartInfo.Arguments = "/c """ & batchFilePath & """"
        processStartInfo.UseShellExecute = False
        processStartInfo.CreateNoWindow = True
        Dim process As Process = Process.Start(processStartInfo)
        process.WaitForExit()
    End Sub

    Private Sub BtnLoadGame_Click(sender As Object, e As EventArgs) Handles BtnLoadGame.Click
        If ListBox1.SelectedIndex = -1 Then
            MessageBox.Show("Please select a game first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim selectedGame As String = ListBox1.SelectedItem.ToString()
        Dim gameFile As String = Directory.GetFiles(GameFolder, selectedGame & ".*", SearchOption.AllDirectories).
    FirstOrDefault(Function(f) f.EndsWith(".tzx") Or f.EndsWith(".tap"))

        If Not String.IsNullOrEmpty(gameFile) Then
            Try
                ' Clear any existing control file
                If File.Exists(controlFilePath) Then
                    File.Delete(controlFilePath)
                End If

                ' Start the Python script
                Dim startInfo As New ProcessStartInfo()
                startInfo.FileName = Path.Combine(Application.StartupPath, "python", "python.exe") ' Use embedded Python
                startInfo.Arguments = """" & Path.Combine(Application.StartupPath, "tzxplay.py") & """ """ & gameFile & """" ' Correct tzxplay.py path
                startInfo.UseShellExecute = False
                startInfo.CreateNoWindow = True
                startInfo.RedirectStandardError = True ' Redirect standard error to capture any errors
                startInfo.RedirectStandardOutput = True ' Redirect standard output to capture any output
                tzxPlayProcess = Process.Start(startInfo)

                ' Log process start
                Debug.WriteLine("Python process started with PID: " & tzxPlayProcess.Id)

                ' Read errors and output asynchronously
                AddHandler tzxPlayProcess.ErrorDataReceived, AddressOf ProcessErrorDataReceived
                AddHandler tzxPlayProcess.OutputDataReceived, AddressOf ProcessOutputDataReceived
                tzxPlayProcess.BeginErrorReadLine()
                tzxPlayProcess.BeginOutputReadLine()

                ' Lock the game list
                ListBox1.Enabled = False

                ' Start the tape counter
                tapeCounter = 0
                tapeCounterTimer.Start()

                ' Read the total number of blocks from the Python script
                totalBlocks = GetTotalBlocks()

                ' Reset the Pause/Resume button to "Pause"
                isPaused = False
                BtnPause.Text = "Pause"

                ' Reset the zeroed block reference
                zeroedBlock = 0
                LblZeroedBlock.Text = "Reset at Block 0"
            Catch ex As Exception
                MessageBox.Show($"Failed to start Python script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) ' Display specific error
            End Try
        Else
            MessageBox.Show("Game file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub ProcessErrorDataReceived(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then
            Debug.WriteLine("Python Error: " & e.Data)
        End If
    End Sub

    Private Sub ProcessOutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then
            Debug.WriteLine("Python Output: " & e.Data)
        End If
    End Sub


    Private Sub BtnPause_Click(sender As Object, e As EventArgs) Handles BtnPause.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Toggle pause by writing to the control file
        Dim command As String = If(isPaused, "resume", "pause")
        File.WriteAllText(controlFilePath, command)
        isPaused = Not isPaused
        BtnPause.Text = If(isPaused, "Resume", "Pause")

        ' Pause or resume the tape counter
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

        ' Stop playback by writing to the control file
        File.WriteAllText(controlFilePath, "stop")
        tzxPlayProcess.Kill()
        tzxPlayProcess = Nothing
        isPaused = False
        BtnPause.Text = "Pause" ' Reset Pause button to "Pause"

        ' Unlock the game list
        ListBox1.Enabled = True

        ' Stop the tape counter
        tapeCounterTimer.Stop()
    End Sub

    Private Sub BtnRewind_Click(sender As Object, e As EventArgs) Handles BtnRewind.Click
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Get the current block
        Dim currentBlock As Integer = GetCurrentBlock()

        ' Move to the previous block
        If currentBlock > 0 Then
            currentBlock -= 1
        End If

        ' Ensure currentBlock is within bounds
        If currentBlock < 0 Then
            currentBlock = 0
        End If

        ' Send the target block to the Python script
        File.WriteAllText(controlFilePath, $"rewind:{currentBlock}")

        ' Update the UI immediately
        LblTapeCounter.Text = $"Current Block: {currentBlock}"
    End Sub

    Private Sub BtnFastForward_Click(sender As Object, e As EventArgs)
        If tzxPlayProcess Is Nothing OrElse tzxPlayProcess.HasExited Then
            MessageBox.Show("No game is currently playing.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Get the current block
        Dim currentBlock = GetCurrentBlock()

        ' Move to the next block, but stop at the last playable block
        If currentBlock < totalBlocks - 1 Then
            currentBlock += 1
        End If

        ' Ensure currentBlock is within bounds
        If currentBlock >= totalBlocks Then
            currentBlock = totalBlocks - 1 ' Stop at the last playable block
        End If

        ' Send the target block to the Python script
        File.WriteAllText(controlFilePath, $"rewind:{currentBlock}")

        ' Display the current block
        LblTapeCounter.Text = $"Current Block: {currentBlock}"
    End Sub

    Private Sub BtnSetZero_Click(sender As Object, e As EventArgs) Handles BtnSetZero.Click
        ' Log the current block number when Set 000 is pressed
        zeroedBlock = GetCurrentBlock()
        LblZeroedBlock.Text = $"Reset at Block {zeroedBlock}" ' Display as "Reset at Block 3" or "Reset at Block 11"
    End Sub

    Private Sub UpdateTapeCounter(sender As Object, e As EventArgs)
        ' Update the current block display
        If tzxPlayProcess IsNot Nothing AndAlso Not tzxPlayProcess.HasExited Then
            Dim currentBlock As Integer = GetCurrentBlock()
            LblTapeCounter.Text = $"Current Block: {currentBlock}"
        Else
            ' Playback has finished
            ListBox1.Enabled = True ' Unlock the game list
            RunDeleteTempFiles() ' Delete temporary files
            BtnPause.Text = "Pause" ' Reset Pause button
            tapeCounterTimer.Stop() ' Stop the timer
        End If
    End Sub

    Private Function GetCurrentBlock() As Integer
        Dim currentBlockFile As String = Path.Combine(Path.GetTempPath(), "current_block.txt")
        If File.Exists(currentBlockFile) Then
            Try
                Dim blockText As String = File.ReadAllText(currentBlockFile)
                Return Integer.Parse(blockText)
            Catch ex As Exception
                MessageBox.Show("Failed to read current block index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return 0
            End Try
        Else
            Return 0
        End If
    End Function

    Private Function GetTotalBlocks() As Integer
        Dim totalBlocksFile As String = Path.Combine(Path.GetTempPath(), "total_blocks.txt")
        If File.Exists(totalBlocksFile) Then
            Try
                Dim blockText As String = File.ReadAllText(totalBlocksFile)
                Return Integer.Parse(blockText)
            Catch ex As Exception
                MessageBox.Show("Failed to read total number of blocks.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return 0
            End Try
        Else
            Return 0
        End If
    End Function
End Class
