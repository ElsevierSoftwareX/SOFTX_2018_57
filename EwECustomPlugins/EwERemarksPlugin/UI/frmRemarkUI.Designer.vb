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

#Region " Imports "

Option Strict On
Imports ScientificInterfaceShared.Forms
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

Partial Class frmRemarkUI
    Inherits frmEwEGrid

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
        Dim sep1 As System.Windows.Forms.ToolStripSeparator
        Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tslSort = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscmbSort = New System.Windows.Forms.ToolStripComboBox()
        Me.m_grid = New EwERemarksPlugin.cRemarksGrid()
        sep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_ts.SuspendLayout()
        Me.SuspendLayout()
        '
        'sep1
        '
        sep1.Name = "sep1"
        sep1.Size = New System.Drawing.Size(6, 25)
        '
        'm_ts
        '
        Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {sep1, Me.m_tslSort, Me.m_tscmbSort})
        Me.m_ts.Location = New System.Drawing.Point(0, 0)
        Me.m_ts.Name = "m_ts"
        Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.m_ts.Size = New System.Drawing.Size(533, 25)
        Me.m_ts.TabIndex = 3
        '
        'm_tslSort
        '
        Me.m_tslSort.Name = "m_tslSort"
        Me.m_tslSort.Size = New System.Drawing.Size(47, 22)
        Me.m_tslSort.Text = "Sort by:"
        '
        'm_tscmbSort
        '
        Me.m_tscmbSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscmbSort.Name = "m_tscmbSort"
        Me.m_tscmbSort.Size = New System.Drawing.Size(121, 25)
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = False
        Me.m_grid.AutoSizeMinHeight = 10
        Me.m_grid.AutoSizeMinWidth = 10
        Me.m_grid.AutoStretchColumnsToFitWidth = True
        Me.m_grid.AutoStretchRowsToFitHeight = False
        Me.m_grid.BackColor = System.Drawing.Color.White
        Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_grid.CustomSort = False
        Me.m_grid.DataName = "grid content"
        Me.m_grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_grid.FixedColumnWidths = False
        Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_grid.GridToolTipActive = True
        Me.m_grid.IsLayoutSuspended = False
        Me.m_grid.IsOutputGrid = True
        Me.m_grid.Location = New System.Drawing.Point(0, 25)
        Me.m_grid.Name = "m_grid"
        Me.m_grid.Size = New System.Drawing.Size(533, 402)
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
        Me.m_grid.TrackPropertySelection = False
        Me.m_grid.UIContext = Nothing
        '
        'frmRemarkUI
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(533, 427)
        Me.Controls.Add(Me.m_grid)
        Me.Controls.Add(Me.m_ts)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmRemarkUI"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.Text = "Ecowriter"
        Me.m_ts.ResumeLayout(False)
        Me.m_ts.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private WithEvents m_ts As cEwEToolstrip
    Private WithEvents m_tslSort As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tscmbSort As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_grid As EwERemarksPlugin.cRemarksGrid

End Class
