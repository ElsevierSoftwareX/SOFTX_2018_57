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
Partial Class CreateCollectionForData
    'Inherits System.Windows.Forms.Form
    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    'Form overrides dispose to clean up the component list.
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
        Me.lstUnSelected = New System.Windows.Forms.ListBox()
        Me.lstSelected = New System.Windows.Forms.ListBox()
        Me.chklstAttached = New System.Windows.Forms.CheckedListBox()
        Me.btnRemoveAll = New System.Windows.Forms.Button()
        Me.btnRemoveSelected = New System.Windows.Forms.Button()
        Me.btnAddSelected = New System.Windows.Forms.Button()
        Me.btnAddAll = New System.Windows.Forms.Button()
        Me.btnAttachAll = New System.Windows.Forms.Button()
        Me.btnAttachNone = New System.Windows.Forms.Button()
        Me.btnOk = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lstUnSelected
        '
        Me.lstUnSelected.FormattingEnabled = True
        Me.lstUnSelected.Location = New System.Drawing.Point(12, 12)
        Me.lstUnSelected.Name = "lstUnSelected"
        Me.lstUnSelected.Size = New System.Drawing.Size(161, 485)
        Me.lstUnSelected.Sorted = True
        Me.lstUnSelected.TabIndex = 0
        '
        'lstSelected
        '
        Me.lstSelected.FormattingEnabled = True
        Me.lstSelected.Location = New System.Drawing.Point(194, 12)
        Me.lstSelected.Name = "lstSelected"
        Me.lstSelected.Size = New System.Drawing.Size(161, 485)
        Me.lstSelected.Sorted = True
        Me.lstSelected.TabIndex = 1
        '
        'chklstAttached
        '
        Me.chklstAttached.CheckOnClick = True
        Me.chklstAttached.FormattingEnabled = True
        Me.chklstAttached.Location = New System.Drawing.Point(378, 12)
        Me.chklstAttached.Name = "chklstAttached"
        Me.chklstAttached.Size = New System.Drawing.Size(161, 484)
        Me.chklstAttached.Sorted = True
        Me.chklstAttached.TabIndex = 2
        '
        'btnRemoveAll
        '
        Me.btnRemoveAll.Location = New System.Drawing.Point(96, 510)
        Me.btnRemoveAll.Name = "btnRemoveAll"
        Me.btnRemoveAll.Size = New System.Drawing.Size(35, 32)
        Me.btnRemoveAll.TabIndex = 3
        Me.btnRemoveAll.Text = "<<"
        Me.btnRemoveAll.UseVisualStyleBackColor = True
        '
        'btnRemoveSelected
        '
        Me.btnRemoveSelected.Location = New System.Drawing.Point(138, 510)
        Me.btnRemoveSelected.Name = "btnRemoveSelected"
        Me.btnRemoveSelected.Size = New System.Drawing.Size(35, 32)
        Me.btnRemoveSelected.TabIndex = 4
        Me.btnRemoveSelected.Text = "<"
        Me.btnRemoveSelected.UseVisualStyleBackColor = True
        '
        'btnAddSelected
        '
        Me.btnAddSelected.Location = New System.Drawing.Point(194, 510)
        Me.btnAddSelected.Name = "btnAddSelected"
        Me.btnAddSelected.Size = New System.Drawing.Size(35, 32)
        Me.btnAddSelected.TabIndex = 5
        Me.btnAddSelected.Text = ">"
        Me.btnAddSelected.UseVisualStyleBackColor = True
        '
        'btnAddAll
        '
        Me.btnAddAll.Location = New System.Drawing.Point(235, 510)
        Me.btnAddAll.Name = "btnAddAll"
        Me.btnAddAll.Size = New System.Drawing.Size(35, 32)
        Me.btnAddAll.TabIndex = 6
        Me.btnAddAll.Text = ">>"
        Me.btnAddAll.UseVisualStyleBackColor = True
        '
        'btnAttachAll
        '
        Me.btnAttachAll.Location = New System.Drawing.Point(545, 12)
        Me.btnAttachAll.Name = "btnAttachAll"
        Me.btnAttachAll.Size = New System.Drawing.Size(75, 31)
        Me.btnAttachAll.TabIndex = 7
        Me.btnAttachAll.Text = Global.EwEResultsExtractor.My.Resources.Resources.ATTACH_ALL
        Me.btnAttachAll.UseVisualStyleBackColor = True
        '
        'btnAttachNone
        '
        Me.btnAttachNone.Location = New System.Drawing.Point(544, 49)
        Me.btnAttachNone.Name = "btnAttachNone"
        Me.btnAttachNone.Size = New System.Drawing.Size(76, 31)
        Me.btnAttachNone.TabIndex = 8
        Me.btnAttachNone.Text = Global.EwEResultsExtractor.My.Resources.Resources.DETACH_ALL
        Me.btnAttachNone.UseVisualStyleBackColor = True
        '
        'btnOk
        '
        Me.btnOk.Location = New System.Drawing.Point(544, 511)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(75, 31)
        Me.btnOk.TabIndex = 9
        Me.btnOk.Text = Global.EwEResultsExtractor.My.Resources.Resources.OK
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'CreateCollectionForData
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(631, 554)
        Me.CloseButton = False
        Me.CloseButtonVisible = False
        Me.ControlBox = False
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.btnAttachNone)
        Me.Controls.Add(Me.btnAttachAll)
        Me.Controls.Add(Me.btnAddAll)
        Me.Controls.Add(Me.btnAddSelected)
        Me.Controls.Add(Me.btnRemoveSelected)
        Me.Controls.Add(Me.btnRemoveAll)
        Me.Controls.Add(Me.chklstAttached)
        Me.Controls.Add(Me.lstSelected)
        Me.Controls.Add(Me.lstUnSelected)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "CreateCollectionForData"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Select which groups for which you would like to extract results"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lstUnSelected As System.Windows.Forms.ListBox
    Friend WithEvents lstSelected As System.Windows.Forms.ListBox
    Friend WithEvents chklstAttached As System.Windows.Forms.CheckedListBox
    Friend WithEvents btnRemoveAll As System.Windows.Forms.Button
    Friend WithEvents btnRemoveSelected As System.Windows.Forms.Button
    Friend WithEvents btnAddSelected As System.Windows.Forms.Button
    Friend WithEvents btnAddAll As System.Windows.Forms.Button
    Friend WithEvents btnAttachAll As System.Windows.Forms.Button
    Friend WithEvents btnAttachNone As System.Windows.Forms.Button
    Friend WithEvents btnOk As System.Windows.Forms.Button
End Class
