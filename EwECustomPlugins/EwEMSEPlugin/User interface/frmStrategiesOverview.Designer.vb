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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

Imports ScientificInterfaceShared.Forms
Imports ScientificInterfaceShared.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmStrategiesOverview
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
        Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_btnSave = New System.Windows.Forms.Button()
        Me.m_grid = New EwEMSEPlugin.gridStrategiesOverview()
        Me.m_btnCheckAll = New System.Windows.Forms.Button()
        Me.m_btnCheckNone = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'm_ts
        '
        Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_ts.Location = New System.Drawing.Point(0, 0)
        Me.m_ts.Name = "m_ts"
        Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.m_ts.Size = New System.Drawing.Size(512, 25)
        Me.m_ts.TabIndex = 1
        Me.m_ts.Text = "CEwEToolstrip1"
        '
        'm_btnCancel
        '
        Me.m_btnCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnCancel.Location = New System.Drawing.Point(355, 489)
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.Size = New System.Drawing.Size(65, 24)
        Me.m_btnCancel.TabIndex = 2
        Me.m_btnCancel.Text = "Cancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'm_btnSave
        '
        Me.m_btnSave.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnSave.Location = New System.Drawing.Point(426, 489)
        Me.m_btnSave.Name = "m_btnSave"
        Me.m_btnSave.Size = New System.Drawing.Size(70, 23)
        Me.m_btnSave.TabIndex = 3
        Me.m_btnSave.Text = "OK"
        Me.m_btnSave.UseVisualStyleBackColor = True
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = False
        Me.m_grid.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
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
        Me.m_grid.FixedColumnWidths = False
        Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_grid.GridToolTipActive = True
        Me.m_grid.IsLayoutSuspended = False
        Me.m_grid.IsOutputGrid = True
        Me.m_grid.Location = New System.Drawing.Point(12, 70)
        Me.m_grid.Name = "m_grid"
        Me.m_grid.Size = New System.Drawing.Size(484, 413)
        Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_grid.TabIndex = 0
        Me.m_grid.UIContext = Nothing
        '
        'm_btnCheckAll
        '
        Me.m_btnCheckAll.Location = New System.Drawing.Point(338, 40)
        Me.m_btnCheckAll.Name = "m_btnCheckAll"
        Me.m_btnCheckAll.Size = New System.Drawing.Size(76, 24)
        Me.m_btnCheckAll.TabIndex = 4
        Me.m_btnCheckAll.Text = "Check All"
        Me.m_btnCheckAll.UseVisualStyleBackColor = True
        '
        'm_btnCheckNone
        '
        Me.m_btnCheckNone.Location = New System.Drawing.Point(420, 40)
        Me.m_btnCheckNone.Name = "m_btnCheckNone"
        Me.m_btnCheckNone.Size = New System.Drawing.Size(76, 24)
        Me.m_btnCheckNone.TabIndex = 5
        Me.m_btnCheckNone.Text = "Uncheck All"
        Me.m_btnCheckNone.UseVisualStyleBackColor = True
        '
        'frmStrategiesOverview
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(512, 525)
        Me.Controls.Add(Me.m_btnCheckNone)
        Me.Controls.Add(Me.m_btnCheckAll)
        Me.Controls.Add(Me.m_btnSave)
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_ts)
        Me.Controls.Add(Me.m_grid)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "frmStrategiesOverview"
        Me.TabText = ""
        Me.Text = "Strategies Overview"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private WithEvents m_grid As gridStrategiesOverview
    Friend WithEvents m_ts As cEwEToolstrip
    Friend WithEvents m_btnCancel As Button
    Friend WithEvents m_btnSave As Button
    Friend WithEvents m_btnCheckAll As Button
    Friend WithEvents m_btnCheckNone As Button
End Class
