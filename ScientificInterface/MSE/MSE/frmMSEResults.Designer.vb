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

Imports ScientificInterfaceShared.Forms

Partial Class frmMSEResults
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
        Me.rbGroup = New System.Windows.Forms.RadioButton()
        Me.rbFleet = New System.Windows.Forms.RadioButton()
        Me.m_grid = New ScientificInterface.gridRiskResults()
        Me.pnlGrid = New System.Windows.Forms.Panel()
        Me.pnlGrid.SuspendLayout()
        Me.SuspendLayout()
        '
        'rbGroup
        '
        Me.rbGroup.AutoSize = True
        Me.rbGroup.Checked = True
        Me.rbGroup.Location = New System.Drawing.Point(12, 10)
        Me.rbGroup.Name = "rbGroup"
        Me.rbGroup.Size = New System.Drawing.Size(54, 17)
        Me.rbGroup.TabIndex = 0
        Me.rbGroup.TabStop = True
        Me.rbGroup.Text = "&Group"
        Me.rbGroup.UseVisualStyleBackColor = True
        '
        'rbFleet
        '
        Me.rbFleet.AutoSize = True
        Me.rbFleet.Location = New System.Drawing.Point(72, 10)
        Me.rbFleet.Name = "rbFleet"
        Me.rbFleet.Size = New System.Drawing.Size(48, 17)
        Me.rbFleet.TabIndex = 3
        Me.rbFleet.Text = "&Fleet"
        Me.rbFleet.UseVisualStyleBackColor = True
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = True
        Me.m_grid.AutoSizeMinHeight = 10
        Me.m_grid.AutoSizeMinWidth = 10
        Me.m_grid.AutoStretchColumnsToFitWidth = False
        Me.m_grid.AutoStretchRowsToFitHeight = False
        Me.m_grid.BackColor = System.Drawing.Color.White
        Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_grid.CustomSort = False
        Me.m_grid.DataName = "grid content"
        Me.m_grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_grid.FixedColumnWidths = True
        Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_grid.GridToolTipActive = True
        Me.m_grid.GridType = ScientificInterface.gridRiskResults.eGridType.Group
        Me.m_grid.IsLayoutSuspended = False
        Me.m_grid.IsOutputGrid = True
        Me.m_grid.Location = New System.Drawing.Point(0, 0)
        Me.m_grid.Name = "m_grid"
        Me.m_grid.Size = New System.Drawing.Size(640, 393)
        Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_grid.TabIndex = 4
        Me.m_grid.UIContext = Nothing
        '
        'pnlGrid
        '
        Me.pnlGrid.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlGrid.Controls.Add(Me.m_grid)
        Me.pnlGrid.Location = New System.Drawing.Point(1, 33)
        Me.pnlGrid.Name = "pnlGrid"
        Me.pnlGrid.Size = New System.Drawing.Size(640, 393)
        Me.pnlGrid.TabIndex = 5
        '
        'frmMSEResults
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(639, 425)
        Me.Controls.Add(Me.pnlGrid)
        Me.Controls.Add(Me.rbFleet)
        Me.Controls.Add(Me.rbGroup)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "frmMSEResults"
        Me.TabText = ""
        Me.Text = "MSE results"
        Me.pnlGrid.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents rbFleet As System.Windows.Forms.RadioButton
    Friend WithEvents pnlGrid As System.Windows.Forms.Panel
    Private WithEvents m_grid As ScientificInterface.gridRiskResults
    Private WithEvents rbGroup As System.Windows.Forms.RadioButton
End Class
