<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        ListBox1 = New ListBox()
        PictureBox1 = New PictureBox()
        RichTextBox1 = New RichTextBox()
        BtnPlay = New Button()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        BtnStop = New Button()
        BtnRewind = New Button()
        BtnCounterReset = New Button()
        LblTapeCounter = New Label()
        LblZeroedBlock = New Label()
        LblCurrentGame = New Label()
        MenuStrip1 = New MenuStrip()
        Label1 = New Label()
        SearchBox = New TextBox()
        BtnFastForward = New Button()
        BtnSaveState = New Button()
        BtnLoadState = New Button()
        LblTapeStatus = New Label()
        GroupBox1 = New GroupBox()
        GroupBox2 = New GroupBox()
        Jump = New Button()
        GroupBox3 = New GroupBox()
        BtnEject = New Button()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox1.SuspendLayout()
        GroupBox2.SuspendLayout()
        GroupBox3.SuspendLayout()
        SuspendLayout()
        ' 
        ' ListBox1
        ' 
        ListBox1.FormattingEnabled = True
        ListBox1.ItemHeight = 15
        ListBox1.Location = New Point(0, 57)
        ListBox1.Name = "ListBox1"
        ListBox1.Size = New Size(620, 499)
        ListBox1.TabIndex = 0
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Location = New Point(631, 27)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(353, 263)
        PictureBox1.TabIndex = 1
        PictureBox1.TabStop = False
        ' 
        ' RichTextBox1
        ' 
        RichTextBox1.Location = New Point(631, 296)
        RichTextBox1.Name = "RichTextBox1"
        RichTextBox1.Size = New Size(353, 260)
        RichTextBox1.TabIndex = 2
        RichTextBox1.Text = ""
        ' 
        ' BtnPlay
        ' 
        BtnPlay.Location = New Point(15, 20)
        BtnPlay.Name = "BtnPlay"
        BtnPlay.Size = New Size(75, 23)
        BtnPlay.TabIndex = 3
        BtnPlay.Text = "Play"
        BtnPlay.UseVisualStyleBackColor = True
        ' 
        ' BtnStop
        ' 
        BtnStop.Location = New Point(258, 21)
        BtnStop.Name = "BtnStop"
        BtnStop.Size = New Size(75, 23)
        BtnStop.TabIndex = 6
        BtnStop.Text = "Stop"
        BtnStop.UseVisualStyleBackColor = True
        ' 
        ' BtnRewind
        ' 
        BtnRewind.Location = New Point(96, 21)
        BtnRewind.Name = "BtnRewind"
        BtnRewind.Size = New Size(75, 23)
        BtnRewind.TabIndex = 7
        BtnRewind.Text = "Rewind"
        BtnRewind.UseVisualStyleBackColor = True
        ' 
        ' BtnCounterReset
        ' 
        BtnCounterReset.Location = New Point(16, 22)
        BtnCounterReset.Name = "BtnCounterReset"
        BtnCounterReset.Size = New Size(92, 23)
        BtnCounterReset.TabIndex = 8
        BtnCounterReset.Text = "Counter Reset"
        BtnCounterReset.UseVisualStyleBackColor = True
        ' 
        ' LblTapeCounter
        ' 
        LblTapeCounter.AutoSize = True
        LblTapeCounter.Location = New Point(114, 26)
        LblTapeCounter.Name = "LblTapeCounter"
        LblTapeCounter.Size = New Size(82, 15)
        LblTapeCounter.TabIndex = 9
        LblTapeCounter.Text = "Current Block:"
        ' 
        ' LblZeroedBlock
        ' 
        LblZeroedBlock.AutoSize = True
        LblZeroedBlock.Location = New Point(114, 56)
        LblZeroedBlock.Name = "LblZeroedBlock"
        LblZeroedBlock.Size = New Size(83, 15)
        LblZeroedBlock.TabIndex = 11
        LblZeroedBlock.Text = "Reset at Block:"
        ' 
        ' LblCurrentGame
        ' 
        LblCurrentGame.AutoSize = True
        LblCurrentGame.Location = New Point(102, 630)
        LblCurrentGame.Name = "LblCurrentGame"
        LblCurrentGame.Size = New Size(0, 15)
        LblCurrentGame.TabIndex = 12
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Size = New Size(988, 24)
        MenuStrip1.TabIndex = 13
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(12, 630)
        Label1.Name = "Label1"
        Label1.Size = New Size(84, 15)
        Label1.TabIndex = 14
        Label1.Text = "Current Game:"
        ' 
        ' SearchBox
        ' 
        SearchBox.Location = New Point(2, 27)
        SearchBox.Name = "SearchBox"
        SearchBox.PlaceholderText = "Search Games...."
        SearchBox.Size = New Size(618, 23)
        SearchBox.TabIndex = 15
        ' 
        ' BtnFastForward
        ' 
        BtnFastForward.Location = New Point(177, 21)
        BtnFastForward.Name = "BtnFastForward"
        BtnFastForward.Size = New Size(75, 23)
        BtnFastForward.TabIndex = 16
        BtnFastForward.Text = "Forward"
        BtnFastForward.UseVisualStyleBackColor = True
        ' 
        ' BtnSaveState
        ' 
        BtnSaveState.Location = New Point(18, 22)
        BtnSaveState.Name = "BtnSaveState"
        BtnSaveState.Size = New Size(75, 23)
        BtnSaveState.TabIndex = 17
        BtnSaveState.Text = "Save"
        BtnSaveState.UseVisualStyleBackColor = True
        ' 
        ' BtnLoadState
        ' 
        BtnLoadState.Location = New Point(109, 22)
        BtnLoadState.Name = "BtnLoadState"
        BtnLoadState.Size = New Size(75, 23)
        BtnLoadState.TabIndex = 18
        BtnLoadState.Text = "Load"
        BtnLoadState.UseVisualStyleBackColor = True
        ' 
        ' LblTapeStatus
        ' 
        LblTapeStatus.AutoSize = True
        LblTapeStatus.Location = New Point(6, 56)
        LblTapeStatus.Name = "LblTapeStatus"
        LblTapeStatus.Size = New Size(39, 15)
        LblTapeStatus.TabIndex = 19
        LblTapeStatus.Text = "Ready"
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(BtnSaveState)
        GroupBox1.Controls.Add(LblTapeStatus)
        GroupBox1.Controls.Add(BtnLoadState)
        GroupBox1.Location = New Point(776, 562)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(200, 84)
        GroupBox1.TabIndex = 20
        GroupBox1.TabStop = False
        GroupBox1.Text = "Game Saves"
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(Jump)
        GroupBox2.Controls.Add(BtnCounterReset)
        GroupBox2.Controls.Add(LblZeroedBlock)
        GroupBox2.Controls.Add(LblTapeCounter)
        GroupBox2.Location = New Point(548, 562)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(222, 83)
        GroupBox2.TabIndex = 21
        GroupBox2.TabStop = False
        GroupBox2.Text = "Tape Counter"
        ' 
        ' Jump
        ' 
        Jump.Location = New Point(16, 51)
        Jump.Name = "Jump"
        Jump.Size = New Size(92, 23)
        Jump.TabIndex = 12
        Jump.Text = "Jump"
        Jump.UseVisualStyleBackColor = True
        ' 
        ' GroupBox3
        ' 
        GroupBox3.Controls.Add(BtnEject)
        GroupBox3.Controls.Add(BtnPlay)
        GroupBox3.Controls.Add(BtnRewind)
        GroupBox3.Controls.Add(BtnFastForward)
        GroupBox3.Controls.Add(BtnStop)
        GroupBox3.Location = New Point(12, 563)
        GroupBox3.Name = "GroupBox3"
        GroupBox3.Size = New Size(430, 56)
        GroupBox3.TabIndex = 22
        GroupBox3.TabStop = False
        GroupBox3.Text = "Tape Control"
        ' 
        ' BtnEject
        ' 
        BtnEject.Location = New Point(339, 20)
        BtnEject.Name = "BtnEject"
        BtnEject.Size = New Size(75, 23)
        BtnEject.TabIndex = 17
        BtnEject.Text = "Eject"
        BtnEject.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(988, 655)
        Controls.Add(GroupBox3)
        Controls.Add(GroupBox2)
        Controls.Add(GroupBox1)
        Controls.Add(SearchBox)
        Controls.Add(Label1)
        Controls.Add(LblCurrentGame)
        Controls.Add(RichTextBox1)
        Controls.Add(PictureBox1)
        Controls.Add(ListBox1)
        Controls.Add(MenuStrip1)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MainMenuStrip = MenuStrip1
        Name = "Form1"
        Text = "Form1"
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        GroupBox3.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents RichTextBox1 As RichTextBox
    Friend WithEvents BtnPlay As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents BtnStop As Button
    Friend WithEvents BtnRewind As Button
    Friend WithEvents BtnCounterReset As Button
    Friend WithEvents LblTapeCounter As Label
    Friend WithEvents LblZeroedBlock As Label
    Friend WithEvents LblCurrentGame As Label
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents Label1 As Label
    Friend WithEvents SearchBox As TextBox
    Friend WithEvents BtnFastForward As Button
    Friend WithEvents BtnSaveState As Button
    Friend WithEvents BtnLoadState As Button
    Friend WithEvents LblTapeStatus As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents Jump As Button
    Friend WithEvents BtnEject As Button

End Class
