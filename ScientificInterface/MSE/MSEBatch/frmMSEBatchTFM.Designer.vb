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

Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Forms

Partial Class frmMSEBatchTFM
    Inherits frmEwE

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
        Me.btCalcIters = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txNTFM = New System.Windows.Forms.TextBox()
        Me.rbCalcTypePercent = New System.Windows.Forms.RadioButton()
        Me.rbCalcTypeValue = New System.Windows.Forms.RadioButton()
        Me.tbGrids = New System.Windows.Forms.TabControl()
        Me.pageGroups = New System.Windows.Forms.TabPage()
        Me.grdGroups = New ScientificInterface.gridMSEBatchTFM()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.UpDwnIter = New System.Windows.Forms.NumericUpDown()
        Me.pageIters = New System.Windows.Forms.TabPage()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cbGroups = New System.Windows.Forms.ComboBox()
        Me.grdIters = New ScientificInterface.gridMSEBatchTFMIter()
        Me.tbGrids.SuspendLayout()
        Me.pageGroups.SuspendLayout()
        CType(Me.UpDwnIter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pageIters.SuspendLayout()
        Me.SuspendLayout()
        '
        'btCalcIters
        '
        Me.btCalcIters.Location = New System.Drawing.Point(222, 5)
        Me.btCalcIters.Name = "btCalcIters"
        Me.btCalcIters.Size = New System.Drawing.Size(212, 20)
        Me.btCalcIters.TabIndex = 2
        Me.btCalcIters.Text = "Calculate iteration values"
        Me.btCalcIters.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(9, 9)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(151, 13)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "Number of parameter iterations"
        '
        'txNTFM
        '
        Me.txNTFM.Location = New System.Drawing.Point(166, 4)
        Me.txNTFM.Name = "txNTFM"
        Me.txNTFM.Size = New System.Drawing.Size(50, 20)
        Me.txNTFM.TabIndex = 1
        '
        'rbCalcTypePercent
        '
        Me.rbCalcTypePercent.AutoSize = True
        Me.rbCalcTypePercent.Checked = True
        Me.rbCalcTypePercent.Location = New System.Drawing.Point(222, 30)
        Me.rbCalcTypePercent.Name = "rbCalcTypePercent"
        Me.rbCalcTypePercent.Size = New System.Drawing.Size(80, 17)
        Me.rbCalcTypePercent.TabIndex = 6
        Me.rbCalcTypePercent.TabStop = True
        Me.rbCalcTypePercent.Text = "Percentage"
        Me.rbCalcTypePercent.UseVisualStyleBackColor = True
        '
        'rbCalcTypeValue
        '
        Me.rbCalcTypeValue.AutoSize = True
        Me.rbCalcTypeValue.Location = New System.Drawing.Point(324, 30)
        Me.rbCalcTypeValue.Name = "rbCalcTypeValue"
        Me.rbCalcTypeValue.Size = New System.Drawing.Size(120, 17)
        Me.rbCalcTypeValue.TabIndex = 7
        Me.rbCalcTypeValue.TabStop = True
        Me.rbCalcTypeValue.Text = "Upper lower bounds"
        Me.rbCalcTypeValue.UseVisualStyleBackColor = True
        '
        'tbGrids
        '
        Me.tbGrids.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbGrids.Controls.Add(Me.pageGroups)
        Me.tbGrids.Controls.Add(Me.pageIters)
        Me.tbGrids.Location = New System.Drawing.Point(2, 79)
        Me.tbGrids.Name = "tbGrids"
        Me.tbGrids.SelectedIndex = 0
        Me.tbGrids.Size = New System.Drawing.Size(885, 455)
        Me.tbGrids.TabIndex = 10
        '
        'pageGroups
        '
        Me.pageGroups.Controls.Add(Me.grdGroups)
        Me.pageGroups.Controls.Add(Me.Label1)
        Me.pageGroups.Controls.Add(Me.UpDwnIter)
        Me.pageGroups.Location = New System.Drawing.Point(4, 22)
        Me.pageGroups.Name = "pageGroups"
        Me.pageGroups.Padding = New System.Windows.Forms.Padding(3)
        Me.pageGroups.Size = New System.Drawing.Size(877, 429)
        Me.pageGroups.TabIndex = 0
        Me.pageGroups.Text = "By groups"
        Me.pageGroups.UseVisualStyleBackColor = True
        '
        'grdGroups
        '
        Me.grdGroups.AllowBlockSelect = True
        Me.grdGroups.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grdGroups.AutoSizeMinHeight = 10
        Me.grdGroups.AutoSizeMinWidth = 10
        Me.grdGroups.AutoStretchColumnsToFitWidth = False
        Me.grdGroups.AutoStretchRowsToFitHeight = False
        Me.grdGroups.BackColor = System.Drawing.Color.White
        Me.grdGroups.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.grdGroups.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.grdGroups.CustomSort = False
        Me.grdGroups.DataName = "grid content"
        Me.grdGroups.FixedColumnWidths = False
        Me.grdGroups.FocusStyle = SourceGrid2.FocusStyle.None
        Me.grdGroups.GridToolTipActive = True
        Me.grdGroups.iCurIter = 1
        Me.grdGroups.IsLayoutSuspended = False
        Me.grdGroups.IsOutputGrid = True
        Me.grdGroups.Location = New System.Drawing.Point(0, 31)
        Me.grdGroups.Name = "grdGroups"
        Me.grdGroups.Size = New System.Drawing.Size(877, 398)
        Me.grdGroups.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.grdGroups.TabIndex = 9
        Me.grdGroups.UIContext = Nothing
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(0, 7)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(77, 13)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "Show iteration "
        '
        'UpDwnIter
        '
        Me.UpDwnIter.Location = New System.Drawing.Point(80, 5)
        Me.UpDwnIter.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.UpDwnIter.Name = "UpDwnIter"
        Me.UpDwnIter.Size = New System.Drawing.Size(60, 20)
        Me.UpDwnIter.TabIndex = 8
        Me.UpDwnIter.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'pageIters
        '
        Me.pageIters.Controls.Add(Me.Label3)
        Me.pageIters.Controls.Add(Me.cbGroups)
        Me.pageIters.Controls.Add(Me.grdIters)
        Me.pageIters.Location = New System.Drawing.Point(4, 22)
        Me.pageIters.Name = "pageIters"
        Me.pageIters.Padding = New System.Windows.Forms.Padding(3)
        Me.pageIters.Size = New System.Drawing.Size(877, 429)
        Me.pageIters.TabIndex = 1
        Me.pageIters.Text = "By iterations"
        Me.pageIters.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(9, 9)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(64, 13)
        Me.Label3.TabIndex = 11
        Me.Label3.Text = "Show group"
        '
        'cbGroups
        '
        Me.cbGroups.FormattingEnabled = True
        Me.cbGroups.Location = New System.Drawing.Point(79, 3)
        Me.cbGroups.Name = "cbGroups"
        Me.cbGroups.Size = New System.Drawing.Size(164, 21)
        Me.cbGroups.TabIndex = 10
        '
        'grdIters
        '
        Me.grdIters.AllowBlockSelect = True
        Me.grdIters.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grdIters.AutoSizeMinHeight = 10
        Me.grdIters.AutoSizeMinWidth = 10
        Me.grdIters.AutoStretchColumnsToFitWidth = False
        Me.grdIters.AutoStretchRowsToFitHeight = False
        Me.grdIters.BackColor = System.Drawing.Color.White
        Me.grdIters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.grdIters.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.grdIters.CustomSort = False
        Me.grdIters.DataName = "grid content"
        Me.grdIters.FixedColumnWidths = False
        Me.grdIters.FocusStyle = SourceGrid2.FocusStyle.None
        Me.grdIters.GridToolTipActive = True
        Me.grdIters.iSelGroup = 1
        Me.grdIters.IsLayoutSuspended = False
        Me.grdIters.IsOutputGrid = True
        Me.grdIters.Location = New System.Drawing.Point(0, 29)
        Me.grdIters.Name = "grdIters"
        Me.grdIters.Size = New System.Drawing.Size(871, 402)
        Me.grdIters.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.grdIters.TabIndex = 9
        Me.grdIters.UIContext = Nothing
        '
        'frmMSEBatchTFM
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(887, 534)
        Me.Controls.Add(Me.tbGrids)
        Me.Controls.Add(Me.rbCalcTypeValue)
        Me.Controls.Add(Me.rbCalcTypePercent)
        Me.Controls.Add(Me.txNTFM)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.btCalcIters)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "frmMSEBatchTFM"
        Me.TabText = ""
        Me.Text = "MSE batch TFM"
        Me.tbGrids.ResumeLayout(False)
        Me.pageGroups.ResumeLayout(False)
        Me.pageGroups.PerformLayout()
        CType(Me.UpDwnIter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pageIters.ResumeLayout(False)
        Me.pageIters.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btCalcIters As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txNTFM As System.Windows.Forms.TextBox
    Friend WithEvents rbCalcTypePercent As System.Windows.Forms.RadioButton
    Friend WithEvents rbCalcTypeValue As System.Windows.Forms.RadioButton
    Friend WithEvents tbGrids As System.Windows.Forms.TabControl
    Friend WithEvents pageGroups As System.Windows.Forms.TabPage
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents UpDwnIter As System.Windows.Forms.NumericUpDown
    Friend WithEvents pageIters As System.Windows.Forms.TabPage
    Friend WithEvents grdIters As ScientificInterface.gridMSEBatchTFMIter
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents cbGroups As System.Windows.Forms.ComboBox
    Friend WithEvents grdGroups As ScientificInterface.gridMSEBatchTFM
End Class
