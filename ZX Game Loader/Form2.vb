Imports System.IO

Public Class SettingsForm
    Private ReadOnly _settingsFilePath As String = Path.Combine(Application.StartupPath, "settings.txt")

    Private Sub SettingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Settings"
        Me.Icon = Form1.Icon
        If File.Exists(_settingsFilePath) Then
            Dim lines() As String = File.ReadAllLines(_settingsFilePath)
            If lines.Length >= 4 Then
                TxtGamesFolder.Text = lines(0)
                TxtScreenshotsFolder.Text = lines(1)
                TxtManualsFolder.Text = lines(2)

                ' Explicitly handle the boolean parsing
                Dim rememberLastGame As Boolean
                If Boolean.TryParse(lines(3), rememberLastGame) Then
                    ChkRememberLastGame.Checked = rememberLastGame
                Else
                    ChkRememberLastGame.Checked = False ' Default value if parsing fails
                End If
            End If
        End If
    End Sub

    Private Sub BtnBrowseGames_Click(sender As Object, e As EventArgs) Handles BtnBrowseGames.Click
        Using folderDialog As New FolderBrowserDialog()
            If folderDialog.ShowDialog() = DialogResult.OK Then
                TxtGamesFolder.Text = folderDialog.SelectedPath
            End If
        End Using
    End Sub

    Private Sub BtnBrowseScreenshots_Click(sender As Object, e As EventArgs) Handles BtnBrowseScreenshots.Click
        Using folderDialog As New FolderBrowserDialog()
            If folderDialog.ShowDialog() = DialogResult.OK Then
                TxtScreenshotsFolder.Text = folderDialog.SelectedPath
            End If
        End Using
    End Sub

    Private Sub BtnBrowseManuals_Click(sender As Object, e As EventArgs) Handles BtnBrowseManuals.Click
        Using folderDialog As New FolderBrowserDialog()
            If folderDialog.ShowDialog() = DialogResult.OK Then
                TxtManualsFolder.Text = folderDialog.SelectedPath
            End If
        End Using
    End Sub

    Private Sub BtnSave_Click(sender As Object, e As EventArgs) Handles BtnSave.Click
        Dim lines() As String = {
            TxtGamesFolder.Text,
            TxtScreenshotsFolder.Text,
            TxtManualsFolder.Text,
            ChkRememberLastGame.Checked.ToString()
        }
        File.WriteAllLines(_settingsFilePath, lines)
        MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Me.Close()
    End Sub
End Class