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

Partial Class frmMSEAssessFleets
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMSEAssessFleets))
        Me.m_split = New System.Windows.Forms.SplitContainer()
        Me.m_blocks = New ScientificInterface.Ecosim.ucPolicyColorBlocks()
        Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_grid = New ScientificInterface.gridFishingCV()
        CType(Me.m_split, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_split.Panel1.SuspendLayout()
        Me.m_split.Panel2.SuspendLayout()
        Me.m_split.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_split
        '
        resources.ApplyResources(Me.m_split, "m_split")
        Me.m_split.Name = "m_split"
        '
        'm_split.Panel1
        '
        Me.m_split.Panel1.Controls.Add(Me.m_blocks)
        '
        'm_split.Panel2
        '
        Me.m_split.Panel2.Controls.Add(Me.m_tsMain)
        Me.m_split.Panel2.Controls.Add(Me.m_grid)
        '
        'm_blocks
        '
        Me.m_blocks.ControlPanelVisible = False
        Me.m_blocks.CurColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.m_blocks, "m_blocks")
        Me.m_blocks.Name = "m_blocks"
        Me.m_blocks.ParmBlockCodes = Nothing
        Me.m_blocks.ShowTooltip = True
        Me.m_blocks.UIContext = Nothing
        '
        'm_tsMain
        '
        Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        resources.ApplyResources(Me.m_tsMain, "m_tsMain")
        Me.m_tsMain.Name = "m_tsMain"
        Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = True
        resources.ApplyResources(Me.m_grid, "m_grid")
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
        Me.m_grid.Name = "m_grid"
        Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_grid.UIContext = Nothing
        '
        'frmMSEAssessFleets
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_split)
        Me.Name = "frmMSEAssessFleets"
        Me.TabText = ""
        Me.m_split.Panel1.ResumeLayout(False)
        Me.m_split.Panel2.ResumeLayout(False)
        Me.m_split.Panel2.PerformLayout()
        CType(Me.m_split, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_split.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_blocks As ScientificInterface.Ecosim.ucPolicyColorBlocks
    Private WithEvents m_grid As ScientificInterface.gridFishingCV
    Private WithEvents m_split As System.Windows.Forms.SplitContainer
    Private WithEvents m_tsMain As cEwEToolstrip
End Class
