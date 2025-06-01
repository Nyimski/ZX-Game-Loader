Public Class AboutForm
    Private _linkHandled As Boolean = False ' Flag to prevent multiple openings

    Private Sub AboutForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Remove previous handlers if any (prevents duplication)
        RemoveHandler lblWebsite.LinkClicked, AddressOf OpenWebsite
        RemoveHandler lblEmail.LinkClicked, AddressOf SendEmail
        RemoveHandler lblGitHub.LinkClicked, AddressOf OpenGitHub

        ' Add fresh handlers
        AddHandler lblWebsite.LinkClicked, AddressOf OpenWebsite
        AddHandler lblEmail.LinkClicked, AddressOf SendEmail
        AddHandler lblGitHub.LinkClicked, AddressOf OpenGitHub
    End Sub

    Private Sub OpenWebsite(sender As Object, e As LinkLabelLinkClickedEventArgs)
        If _linkHandled Then Return
        _linkHandled = True

        Try
            Process.Start(New ProcessStartInfo("https://nyimski.net") With {
                .UseShellExecute = True,
                .Verb = "open"
            })
        Catch ex As Exception
            MessageBox.Show("Unable to open website: " & ex.Message, "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _linkHandled = False ' Reset flag after operation
        End Try
    End Sub

    Private Sub SendEmail(sender As Object, e As LinkLabelLinkClickedEventArgs)
        If _linkHandled Then Return
        _linkHandled = True

        Try
            Process.Start(New ProcessStartInfo("mailto:nyimski@nyimski.net") With {
                .UseShellExecute = True,
                .Verb = "open"
            })
        Catch ex As Exception
            MessageBox.Show("Unable to open email client: " & ex.Message, "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _linkHandled = False
        End Try
    End Sub

    Private Sub OpenGitHub(sender As Object, e As LinkLabelLinkClickedEventArgs)
        If _linkHandled Then Return
        _linkHandled = True

        Try
            Process.Start(New ProcessStartInfo("https://github.com/Nyimski/ZX-Game-Loader") With {
                .UseShellExecute = True,
                .Verb = "open"
            })
        Catch ex As Exception
            MessageBox.Show("Unable to open GitHub: " & ex.Message, "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _linkHandled = False
        End Try
    End Sub
End Class