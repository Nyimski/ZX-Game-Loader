﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
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
        BtnLoadGame = New Button()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        BtnPause = New Button()
        BtnStop = New Button()
        BtnRewind = New Button()
        BtnSetZero = New Button()
        LblTapeCounter = New Label()
        LblZeroedBlock = New Label()
        LblCurrentGame = New Label()
        MenuStrip1 = New MenuStrip()
        Label1 = New Label()
        SearchBox = New TextBox()
        BtnFastForward = New Button()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
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
        ' BtnLoadGame
        ' 
        BtnLoadGame.Location = New Point(12, 580)
        BtnLoadGame.Name = "BtnLoadGame"
        BtnLoadGame.Size = New Size(75, 23)
        BtnLoadGame.TabIndex = 3
        BtnLoadGame.Text = "Play"
        BtnLoadGame.UseVisualStyleBackColor = True
        ' 
        ' BtnPause
        ' 
        BtnPause.Location = New Point(336, 580)
        BtnPause.Name = "BtnPause"
        BtnPause.Size = New Size(75, 23)
        BtnPause.TabIndex = 5
        BtnPause.Text = "Pause"
        BtnPause.UseVisualStyleBackColor = True
        ' 
        ' BtnStop
        ' 
        BtnStop.Location = New Point(255, 580)
        BtnStop.Name = "BtnStop"
        BtnStop.Size = New Size(75, 23)
        BtnStop.TabIndex = 6
        BtnStop.Text = "Stop"
        BtnStop.UseVisualStyleBackColor = True
        ' 
        ' BtnRewind
        ' 
        BtnRewind.Location = New Point(93, 580)
        BtnRewind.Name = "BtnRewind"
        BtnRewind.Size = New Size(75, 23)
        BtnRewind.TabIndex = 7
        BtnRewind.Text = "Rewind"
        BtnRewind.UseVisualStyleBackColor = True
        ' 
        ' BtnSetZero
        ' 
        BtnSetZero.Location = New Point(417, 580)
        BtnSetZero.Name = "BtnSetZero"
        BtnSetZero.Size = New Size(75, 23)
        BtnSetZero.TabIndex = 8
        BtnSetZero.Text = "Set 000"
        BtnSetZero.UseVisualStyleBackColor = True
        ' 
        ' LblTapeCounter
        ' 
        LblTapeCounter.AutoSize = True
        LblTapeCounter.Location = New Point(522, 584)
        LblTapeCounter.Name = "LblTapeCounter"
        LblTapeCounter.Size = New Size(82, 15)
        LblTapeCounter.TabIndex = 9
        LblTapeCounter.Text = "Current Block:"
        ' 
        ' LblZeroedBlock
        ' 
        LblZeroedBlock.AutoSize = True
        LblZeroedBlock.Location = New Point(648, 584)
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
        BtnFastForward.Location = New Point(174, 580)
        BtnFastForward.Name = "BtnFastForward"
        BtnFastForward.Size = New Size(75, 23)
        BtnFastForward.TabIndex = 16
        BtnFastForward.Text = "Forward"
        BtnFastForward.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(988, 655)
        Controls.Add(BtnFastForward)
        Controls.Add(SearchBox)
        Controls.Add(Label1)
        Controls.Add(BtnPause)
        Controls.Add(BtnLoadGame)
        Controls.Add(LblCurrentGame)
        Controls.Add(BtnStop)
        Controls.Add(BtnRewind)
        Controls.Add(LblZeroedBlock)
        Controls.Add(LblTapeCounter)
        Controls.Add(BtnSetZero)
        Controls.Add(RichTextBox1)
        Controls.Add(PictureBox1)
        Controls.Add(ListBox1)
        Controls.Add(MenuStrip1)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MainMenuStrip = MenuStrip1
        Name = "Form1"
        Text = "Form1"
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents RichTextBox1 As RichTextBox
    Friend WithEvents BtnLoadGame As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents BtnPause As Button
    Friend WithEvents BtnStop As Button
    Friend WithEvents BtnRewind As Button
    Friend WithEvents BtnSetZero As Button
    Friend WithEvents LblTapeCounter As Label
    Friend WithEvents LblZeroedBlock As Label
    Friend WithEvents LblCurrentGame As Label
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents Label1 As Label
    Friend WithEvents SearchBox As TextBox
    Friend WithEvents BtnFastForward As Button

End Class
