' ===============================================================================
' This file is part of Ecopath with Ecosim (EwE)
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class lstCreateMember
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lstNonMember = New System.Windows.Forms.ListBox()
        Me.lstMember = New System.Windows.Forms.ListBox()
        Me.btnRemoveSelected = New System.Windows.Forms.Button()
        Me.btnRemoveAll = New System.Windows.Forms.Button()
        Me.btnAddSelected = New System.Windows.Forms.Button()
        Me.btnAddAll = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lstNonMember
        '
        Me.lstNonMember.FormattingEnabled = True
        Me.lstNonMember.Location = New System.Drawing.Point(3, 3)
        Me.lstNonMember.Name = "lstNonMember"
        Me.lstNonMember.Size = New System.Drawing.Size(160, 160)
        Me.lstNonMember.TabIndex = 0
        '
        'lstMember
        '
        Me.lstMember.FormattingEnabled = True
        Me.lstMember.Location = New System.Drawing.Point(169, 3)
        Me.lstMember.Name = "lstMember"
        Me.lstMember.Size = New System.Drawing.Size(160, 160)
        Me.lstMember.TabIndex = 1
        '
        'btnRemoveSelected
        '
        Me.btnRemoveSelected.Location = New System.Drawing.Point(137, 169)
        Me.btnRemoveSelected.Name = "btnRemoveSelected"
        Me.btnRemoveSelected.Size = New System.Drawing.Size(26, 21)
        Me.btnRemoveSelected.TabIndex = 2
        Me.btnRemoveSelected.Text = "<"
        Me.btnRemoveSelected.UseVisualStyleBackColor = True
        '
        'btnRemoveAll
        '
        Me.btnRemoveAll.Location = New System.Drawing.Point(99, 169)
        Me.btnRemoveAll.Name = "btnRemoveAll"
        Me.btnRemoveAll.Size = New System.Drawing.Size(32, 21)
        Me.btnRemoveAll.TabIndex = 3
        Me.btnRemoveAll.Text = "<<"
        Me.btnRemoveAll.UseVisualStyleBackColor = True
        '
        'btnAddSelected
        '
        Me.btnAddSelected.Location = New System.Drawing.Point(169, 169)
        Me.btnAddSelected.Name = "btnAddSelected"
        Me.btnAddSelected.Size = New System.Drawing.Size(26, 21)
        Me.btnAddSelected.TabIndex = 4
        Me.btnAddSelected.Text = ">"
        Me.btnAddSelected.UseVisualStyleBackColor = True
        '
        'btnAddAll
        '
        Me.btnAddAll.Location = New System.Drawing.Point(201, 169)
        Me.btnAddAll.Name = "btnAddAll"
        Me.btnAddAll.Size = New System.Drawing.Size(32, 21)
        Me.btnAddAll.TabIndex = 5
        Me.btnAddAll.Text = ">>"
        Me.btnAddAll.UseVisualStyleBackColor = True
        '
        'lstCreateMember
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Controls.Add(Me.btnAddAll)
        Me.Controls.Add(Me.btnAddSelected)
        Me.Controls.Add(Me.btnRemoveAll)
        Me.Controls.Add(Me.btnRemoveSelected)
        Me.Controls.Add(Me.lstMember)
        Me.Controls.Add(Me.lstNonMember)
        Me.Name = "lstCreateMember"
        Me.Size = New System.Drawing.Size(333, 194)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lstNonMember As System.Windows.Forms.ListBox
    Friend WithEvents lstMember As System.Windows.Forms.ListBox
    Friend WithEvents btnRemoveSelected As System.Windows.Forms.Button
    Friend WithEvents btnRemoveAll As System.Windows.Forms.Button
    Friend WithEvents btnAddSelected As System.Windows.Forms.Button
    Friend WithEvents btnAddAll As System.Windows.Forms.Button

End Class
