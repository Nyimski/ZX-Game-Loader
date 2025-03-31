<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SettingsForm
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
        LblGamesFolder = New Label()
        LblScreenshotsFolder = New Label()
        LblManualsFolder = New Label()
        TxtScreenshotsFolder = New TextBox()
        TxtManualsFolder = New TextBox()
        TxtGamesFolder = New TextBox()
        BtnBrowseGames = New Button()
        BtnBrowseManuals = New Button()
        BtnBrowseScreenshots = New Button()
        BtnSave = New Button()
        Label1 = New Label()
        ChkRememberLastGame = New CheckBox()
        SuspendLayout()
        ' 
        ' LblGamesFolder
        ' 
        LblGamesFolder.AutoSize = True
        LblGamesFolder.Location = New Point(23, 66)
        LblGamesFolder.Name = "LblGamesFolder"
        LblGamesFolder.Size = New Size(43, 15)
        LblGamesFolder.TabIndex = 0
        LblGamesFolder.Text = "Games"
        ' 
        ' LblScreenshotsFolder
        ' 
        LblScreenshotsFolder.AutoSize = True
        LblScreenshotsFolder.Location = New Point(23, 149)
        LblScreenshotsFolder.Name = "LblScreenshotsFolder"
        LblScreenshotsFolder.Size = New Size(70, 15)
        LblScreenshotsFolder.TabIndex = 1
        LblScreenshotsFolder.Text = "Screenshots"
        ' 
        ' LblManualsFolder
        ' 
        LblManualsFolder.AutoSize = True
        LblManualsFolder.Location = New Point(23, 107)
        LblManualsFolder.Name = "LblManualsFolder"
        LblManualsFolder.Size = New Size(52, 15)
        LblManualsFolder.TabIndex = 2
        LblManualsFolder.Text = "Manuals"
        ' 
        ' TxtScreenshotsFolder
        ' 
        TxtScreenshotsFolder.Location = New Point(108, 146)
        TxtScreenshotsFolder.Name = "TxtScreenshotsFolder"
        TxtScreenshotsFolder.Size = New Size(412, 23)
        TxtScreenshotsFolder.TabIndex = 3
        ' 
        ' TxtManualsFolder
        ' 
        TxtManualsFolder.Location = New Point(108, 104)
        TxtManualsFolder.Name = "TxtManualsFolder"
        TxtManualsFolder.Size = New Size(412, 23)
        TxtManualsFolder.TabIndex = 4
        ' 
        ' TxtGamesFolder
        ' 
        TxtGamesFolder.Location = New Point(108, 63)
        TxtGamesFolder.Name = "TxtGamesFolder"
        TxtGamesFolder.Size = New Size(412, 23)
        TxtGamesFolder.TabIndex = 5
        ' 
        ' BtnBrowseGames
        ' 
        BtnBrowseGames.Location = New Point(549, 63)
        BtnBrowseGames.Name = "BtnBrowseGames"
        BtnBrowseGames.Size = New Size(75, 23)
        BtnBrowseGames.TabIndex = 6
        BtnBrowseGames.Text = "Browse"
        BtnBrowseGames.UseVisualStyleBackColor = True
        ' 
        ' BtnBrowseManuals
        ' 
        BtnBrowseManuals.Location = New Point(549, 104)
        BtnBrowseManuals.Name = "BtnBrowseManuals"
        BtnBrowseManuals.Size = New Size(75, 23)
        BtnBrowseManuals.TabIndex = 7
        BtnBrowseManuals.Text = "Browse"
        BtnBrowseManuals.UseVisualStyleBackColor = True
        ' 
        ' BtnBrowseScreenshots
        ' 
        BtnBrowseScreenshots.Location = New Point(549, 146)
        BtnBrowseScreenshots.Name = "BtnBrowseScreenshots"
        BtnBrowseScreenshots.Size = New Size(75, 23)
        BtnBrowseScreenshots.TabIndex = 8
        BtnBrowseScreenshots.Text = "Browse"
        BtnBrowseScreenshots.UseVisualStyleBackColor = True
        ' 
        ' BtnSave
        ' 
        BtnSave.Location = New Point(540, 197)
        BtnSave.Name = "BtnSave"
        BtnSave.Size = New Size(84, 23)
        BtnSave.TabIndex = 9
        BtnSave.Text = "Save Settings"
        BtnSave.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(54, 22)
        Label1.Name = "Label1"
        Label1.Size = New Size(558, 15)
        Label1.TabIndex = 10
        Label1.Text = "Select folders containing your Games, Instruction Manuals and Screenshots and then click 'Save Settings'"
        ' 
        ' ChkRememberLastGame
        ' 
        ChkRememberLastGame.AutoSize = True
        ChkRememberLastGame.CheckAlign = ContentAlignment.MiddleRight
        ChkRememberLastGame.Location = New Point(23, 197)
        ChkRememberLastGame.Name = "ChkRememberLastGame"
        ChkRememberLastGame.Size = New Size(142, 19)
        ChkRememberLastGame.TabIndex = 11
        ChkRememberLastGame.Text = "Remember Last Game"
        ChkRememberLastGame.UseVisualStyleBackColor = True
        ' 
        ' SettingsForm
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(659, 239)
        Controls.Add(ChkRememberLastGame)
        Controls.Add(Label1)
        Controls.Add(BtnSave)
        Controls.Add(BtnBrowseScreenshots)
        Controls.Add(BtnBrowseManuals)
        Controls.Add(BtnBrowseGames)
        Controls.Add(TxtGamesFolder)
        Controls.Add(TxtManualsFolder)
        Controls.Add(TxtScreenshotsFolder)
        Controls.Add(LblManualsFolder)
        Controls.Add(LblScreenshotsFolder)
        Controls.Add(LblGamesFolder)
        Name = "SettingsForm"
        Text = "Settings"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents LblGamesFolder As Label
    Friend WithEvents LblScreenshotsFolder As Label
    Friend WithEvents LblManualsFolder As Label
    Friend WithEvents TxtScreenshotsFolder As TextBox
    Friend WithEvents TxtManualsFolder As TextBox
    Friend WithEvents TxtGamesFolder As TextBox
    Friend WithEvents BtnBrowseGames As Button
    Friend WithEvents BtnBrowseManuals As Button
    Friend WithEvents BtnBrowseScreenshots As Button
    Friend WithEvents BtnSave As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents ChkRememberLastGame As CheckBox
End Class
