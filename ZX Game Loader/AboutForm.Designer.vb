<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AboutForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AboutForm))
        Label1 = New Label()
        lblWebsite = New LinkLabel()
        lblEmail = New LinkLabel()
        lblGitHub = New LinkLabel()
        lblDescription = New Label()
        Label2 = New Label()
        Label3 = New Label()
        Label4 = New Label()
        Label5 = New Label()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        Label1.Location = New Point(76, 24)
        Label1.Name = "Label1"
        Label1.Size = New Size(207, 21)
        Label1.TabIndex = 0
        Label1.Text = "ZX Game Loader v1.5.0"
        ' 
        ' lblWebsite
        ' 
        lblWebsite.AutoSize = True
        lblWebsite.Location = New Point(151, 188)
        lblWebsite.Name = "lblWebsite"
        lblWebsite.Size = New Size(103, 15)
        lblWebsite.TabIndex = 1
        lblWebsite.TabStop = True
        lblWebsite.Text = "https://nyimski.net"
        ' 
        ' lblEmail
        ' 
        lblEmail.AutoSize = True
        lblEmail.Location = New Point(148, 217)
        lblEmail.Name = "lblEmail"
        lblEmail.Size = New Size(120, 15)
        lblEmail.TabIndex = 2
        lblEmail.TabStop = True
        lblEmail.Text = "nyimski@nyimski.net"
        ' 
        ' lblGitHub
        ' 
        lblGitHub.AutoSize = True
        lblGitHub.Location = New Point(173, 246)
        lblGitHub.Name = "lblGitHub"
        lblGitHub.Size = New Size(104, 15)
        lblGitHub.TabIndex = 3
        lblGitHub.TabStop = True
        lblGitHub.Text = "GitHub Repository"
        ' 
        ' lblDescription
        ' 
        lblDescription.Location = New Point(31, 66)
        lblDescription.Name = "lblDescription"
        lblDescription.Size = New Size(300, 50)
        lblDescription.TabIndex = 5
        lblDescription.Text = "A game loader utility for the ZX Spectrum" & vbCrLf & "Developed by Nyimski"
        lblDescription.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(18, 144)
        Label2.Name = "Label2"
        Label2.Size = New Size(323, 15)
        Label2.TabIndex = 6
        Label2.Text = "Licensed under the GNU General Public License v3.0 (GPLv3)"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(91, 246)
        Label3.Name = "Label3"
        Label3.Size = New Size(85, 15)
        Label3.TabIndex = 7
        Label3.Text = "Source Code  -"
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(91, 188)
        Label4.Name = "Label4"
        Label4.Size = New Size(60, 15)
        Label4.TabIndex = 8
        Label4.Text = "Website  -"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(91, 217)
        Label5.Name = "Label5"
        Label5.Size = New Size(60, 15)
        Label5.TabIndex = 9
        Label5.Text = "Contact  -"
        ' 
        ' AboutForm
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(368, 270)
        Controls.Add(Label5)
        Controls.Add(Label4)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(lblDescription)
        Controls.Add(lblGitHub)
        Controls.Add(lblEmail)
        Controls.Add(lblWebsite)
        Controls.Add(Label1)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MinimizeBox = False
        Name = "AboutForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "About"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents lblWebsite As LinkLabel
    Friend WithEvents lblEmail As LinkLabel
    Friend WithEvents lblGitHub As LinkLabel
    Friend WithEvents lblDescription As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
End Class