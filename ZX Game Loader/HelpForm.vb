Imports System.IO

Public Class HelpForm
    Inherits Form

    Public Sub New()
        ' Form setup
        Me.Text = "ZX Game Loader Help"
        Me.Icon = Form1.Icon
        Me.Size = New Size(800, 600)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Font = New Font("Segoe UI", 9)
        Me.BackColor = Color.White
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        ' Create main container
        Dim mainPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(10),
            .AutoScroll = True
        }

        ' Create RichTextBox for help content
        Dim rtbHelp As New RichTextBox With {
            .Dock = DockStyle.Fill,
            .ReadOnly = True,
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .Margin = New Padding(10),
            .ScrollBars = RichTextBoxScrollBars.Vertical
        }

        ' Load and format help text
        Try
            Dim helpFilePath = Path.Combine(Application.StartupPath, "Instructions.txt")
            If File.Exists(helpFilePath) Then
                rtbHelp.Text = File.ReadAllText(helpFilePath)
                FormatHelpText(rtbHelp)
            Else
                rtbHelp.Text = "Help file not found. Please ensure Instructions.txt exists in the application folder."
            End If
        Catch ex As Exception
            rtbHelp.Text = $"Error loading help content: {ex.Message}"
        End Try

        ' Create OK button
        Dim btnOK As New Button With {
            .Text = "OK",
            .DialogResult = DialogResult.OK,
            .Dock = DockStyle.Bottom,
            .Height = 40,
            .Margin = New Padding(0, 10, 0, 0)
        }

        ' Add controls to form
        mainPanel.Controls.Add(rtbHelp)
        mainPanel.Controls.Add(btnOK)
        Me.Controls.Add(mainPanel)

        ' Set accept button
        Me.AcceptButton = btnOK

        ' Ensure we start at the beginning with no selection
        AddHandler Me.Load, Sub(sender, e)
                                rtbHelp.SelectionStart = 0
                                rtbHelp.SelectionLength = 0
                                rtbHelp.ScrollToCaret()
                            End Sub
    End Sub

    Private Sub FormatHelpText(rtb As RichTextBox)
        ' Store original position
        Dim originalPosition = rtb.SelectionStart

        ' Set default font
        rtb.SelectAll()
        rtb.SelectionFont = New Font("Segoe UI", 10)
        rtb.SelectionColor = Color.Black

        ' Format main title
        Dim titleIndex = rtb.Text.IndexOf("ZX Game Loader - Complete User Guide")
        If titleIndex >= 0 Then
            rtb.Select(titleIndex, "ZX Game Loader - Complete User Guide".Length)
            rtb.SelectionFont = New Font("Segoe UI", 14, FontStyle.Bold)
            rtb.SelectionColor = Color.DarkBlue
        End If

        ' Format section headers
        Dim sections() As String = {
           "HOW TO USE (QUICK START)", "SETTINGS GUIDE", "SAVE/LOAD STATES (AUDIO)", "[Tape Status]", "[Settings]",
"[Help]",
            "APPLICATION OVERVIEW", "MAIN INTERFACE CONTROLS", "TROUBLESHOOTING", "PLAY CONTROLS", "[Buttons]",
            "FOLDER STRUCTURE", "VERSION NOTES", "SEARCH FUNCTIONALITY", "SAVING:", "LOADING:", "[Game List]",
            "SETTINGS (Menu → Settings)", "MENU OPTIONS", "TAPE COUNTER", "CONTENT", "SELECT A GAME", "OPTIONS", "[File]", "[Edit Mode]", "[About]"
        }

        For Each section In sections
            Dim index = rtb.Text.IndexOf(section)
            While index >= 0
                rtb.Select(index, section.Length)
                rtb.SelectionFont = New Font(rtb.Font, FontStyle.Bold)
                rtb.SelectionColor = Color.DarkBlue
                index = rtb.Text.IndexOf(section, index + section.Length)
            End While
        Next

        ' Format numbered sections (1. 2. etc.)
        For i As Integer = 1 To 9
            Dim sectionHeader = i.ToString() & "."
            Dim index = rtb.Text.IndexOf(sectionHeader)
            While index >= 0
                ' Check if this is a numbered section (followed by space and capital letter)
                If index + 2 < rtb.Text.Length AndAlso Char.IsWhiteSpace(rtb.Text.Chars(index + 2)) Then
                    rtb.Select(index, 2)
                    rtb.SelectionFont = New Font(rtb.Font, FontStyle.Bold)
                    rtb.SelectionColor = Color.DarkBlue
                End If
                index = rtb.Text.IndexOf(sectionHeader, index + 2)
            End While
        Next

        ' Format button names and key actions
        Dim actions() As String = {
            "PLAY", "EJECT", "STOP", "REWIND", "FORWARD", "COUNTER RESET",
            "SAVE", "LOAD", "SAVE", "STOP", "FORWARD", "Eject", "Browse", "SAVE STATE", "LOAD STATE", "JUMP", "Current Block", "Reset at Block"
        }

        For Each action In actions
            Dim index = rtb.Text.IndexOf(action)
            While index >= 0
                rtb.Select(index, action.Length)
                rtb.SelectionFont = New Font(rtb.Font, FontStyle.Bold)
                rtb.SelectionColor = Color.DarkGreen
                index = rtb.Text.IndexOf(action, index + action.Length)
            End While
        Next

        ' Fix "LOADING" to match "SAVING" formatting
        Dim loadingIndex = rtb.Text.IndexOf("LOADING:")
        If loadingIndex >= 0 Then
            rtb.Select(loadingIndex, "LOADING:".Length)
            rtb.SelectionFont = New Font(rtb.Font, FontStyle.Bold)
            rtb.SelectionColor = Color.Black
        End If

        ' Format folder paths and other special elements
        Dim pathsIndex = rtb.Text.IndexOf("FOLDER PATHS:")
        If pathsIndex >= 0 Then
            rtb.Select(pathsIndex, "FOLDER PATHS:".Length)
            rtb.SelectionFont = New Font(rtb.Font, FontStyle.Bold)
            rtb.SelectionColor = Color.DarkBlue
        End If

        ' Clear selection and reset to top
        rtb.SelectionStart = 0
        rtb.SelectionLength = 0
        rtb.ScrollToCaret()
    End Sub
End Class