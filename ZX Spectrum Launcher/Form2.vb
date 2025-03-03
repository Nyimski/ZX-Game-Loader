Imports System.IO

Public Class SettingsForm
    Private settingsFilePath As String = Path.Combine(Application.StartupPath, "settings.txt")

    Private Sub SettingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Settings" ' Set the form title
        ' Load saved folder paths
        If File.Exists(settingsFilePath) Then
            Dim lines() As String = File.ReadAllLines(settingsFilePath)
            If lines.Length >= 3 Then
                TxtGamesFolder.Text = lines(0)
                TxtScreenshotsFolder.Text = lines(1)
                TxtManualsFolder.Text = lines(2)
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
        ' Save folder paths to settings file
        Dim lines() As String = {
            TxtGamesFolder.Text,
            TxtScreenshotsFolder.Text,
            TxtManualsFolder.Text
        }
        File.WriteAllLines(settingsFilePath, lines)
        MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Me.Close()
    End Sub
End Class