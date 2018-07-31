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



Partial Class frmTFMpolicy
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTFMpolicy))
        Me.m_scMain = New System.Windows.Forms.SplitContainer()
        Me.m_chkUnits = New System.Windows.Forms.CheckBox()
        Me.m_graph = New ZedGraph.ZedGraphControl()
        Me.m_tsStrategy = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tslSelectStratagy = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscmStrategies = New System.Windows.Forms.ToolStripComboBox()
        Me.m_tsbnAddStrategy = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnDeleteStrategy = New System.Windows.Forms.ToolStripButton()
        Me.m_scDetails = New System.Windows.Forms.SplitContainer()
        Me.m_tlpHCR = New System.Windows.Forms.TableLayoutPanel()
        Me.m_grid = New EwEMSEPlugin.gridTargetFishingMortalityPolicy()
        Me.m_tsHCR = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tsbnAddHCR = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnEditHCR = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnDeleteHCR = New System.Windows.Forms.ToolStripButton()
        Me.m_hdrHCR = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tlpRegulations = New System.Windows.Forms.TableLayoutPanel()
        Me.m_hdrRegulations = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_gridRegulations = New EwEMSEPlugin.gridRegulations()
        Me.m_btnOK = New System.Windows.Forms.Button()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scMain.Panel1.SuspendLayout()
        Me.m_scMain.Panel2.SuspendLayout()
        Me.m_scMain.SuspendLayout()
        Me.m_tsStrategy.SuspendLayout()
        CType(Me.m_scDetails, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scDetails.Panel1.SuspendLayout()
        Me.m_scDetails.Panel2.SuspendLayout()
        Me.m_scDetails.SuspendLayout()
        Me.m_tlpHCR.SuspendLayout()
        Me.m_tsHCR.SuspendLayout()
        Me.m_tlpRegulations.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_scMain
        '
        resources.ApplyResources(Me.m_scMain, "m_scMain")
        Me.m_scMain.Name = "m_scMain"
        '
        'm_scMain.Panel1
        '
        Me.m_scMain.Panel1.Controls.Add(Me.m_chkUnits)
        Me.m_scMain.Panel1.Controls.Add(Me.m_graph)
        Me.m_scMain.Panel1.Controls.Add(Me.m_tsStrategy)
        '
        'm_scMain.Panel2
        '
        Me.m_scMain.Panel2.Controls.Add(Me.m_scDetails)
        '
        'm_chkUnits
        '
        resources.ApplyResources(Me.m_chkUnits, "m_chkUnits")
        Me.m_chkUnits.Checked = True
        Me.m_chkUnits.CheckState = System.Windows.Forms.CheckState.Checked
        Me.m_chkUnits.Name = "m_chkUnits"
        Me.m_chkUnits.UseVisualStyleBackColor = True
        '
        'm_graph
        '
        resources.ApplyResources(Me.m_graph, "m_graph")
        Me.m_graph.EditModifierKeys = System.Windows.Forms.Keys.None
        Me.m_graph.Name = "m_graph"
        Me.m_graph.ScrollGrace = 0R
        Me.m_graph.ScrollMaxX = 0R
        Me.m_graph.ScrollMaxY = 0R
        Me.m_graph.ScrollMaxY2 = 0R
        Me.m_graph.ScrollMinX = 0R
        Me.m_graph.ScrollMinY = 0R
        Me.m_graph.ScrollMinY2 = 0R
        Me.m_graph.ZoomButtons = System.Windows.Forms.MouseButtons.None
        '
        'm_tsStrategy
        '
        Me.m_tsStrategy.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsStrategy.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tslSelectStratagy, Me.m_tscmStrategies, Me.m_tsbnAddStrategy, Me.m_tsbnDeleteStrategy})
        resources.ApplyResources(Me.m_tsStrategy, "m_tsStrategy")
        Me.m_tsStrategy.Name = "m_tsStrategy"
        Me.m_tsStrategy.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tslSelectStratagy
        '
        Me.m_tslSelectStratagy.Name = "m_tslSelectStratagy"
        resources.ApplyResources(Me.m_tslSelectStratagy, "m_tslSelectStratagy")
        '
        'm_tscmStrategies
        '
        Me.m_tscmStrategies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscmStrategies.Name = "m_tscmStrategies"
        resources.ApplyResources(Me.m_tscmStrategies, "m_tscmStrategies")
        '
        'm_tsbnAddStrategy
        '
        Me.m_tsbnAddStrategy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnAddStrategy, "m_tsbnAddStrategy")
        Me.m_tsbnAddStrategy.Name = "m_tsbnAddStrategy"
        '
        'm_tsbnDeleteStrategy
        '
        Me.m_tsbnDeleteStrategy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnDeleteStrategy, "m_tsbnDeleteStrategy")
        Me.m_tsbnDeleteStrategy.Name = "m_tsbnDeleteStrategy"
        '
        'm_scDetails
        '
        resources.ApplyResources(Me.m_scDetails, "m_scDetails")
        Me.m_scDetails.Name = "m_scDetails"
        '
        'm_scDetails.Panel1
        '
        Me.m_scDetails.Panel1.Controls.Add(Me.m_tlpHCR)
        '
        'm_scDetails.Panel2
        '
        Me.m_scDetails.Panel2.Controls.Add(Me.m_tlpRegulations)
        '
        'm_tlpHCR
        '
        resources.ApplyResources(Me.m_tlpHCR, "m_tlpHCR")
        Me.m_tlpHCR.Controls.Add(Me.m_grid, 0, 2)
        Me.m_tlpHCR.Controls.Add(Me.m_tsHCR, 0, 1)
        Me.m_tlpHCR.Controls.Add(Me.m_hdrHCR, 0, 0)
        Me.m_tlpHCR.Name = "m_tlpHCR"
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = False
        Me.m_grid.AutoSizeMinHeight = 10
        Me.m_grid.AutoSizeMinWidth = 10
        resources.ApplyResources(Me.m_grid, "m_grid")
        Me.m_grid.AutoStretchColumnsToFitWidth = False
        Me.m_grid.AutoStretchRowsToFitHeight = False
        Me.m_grid.BackColor = System.Drawing.Color.White
        Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_grid.CustomSort = False
        Me.m_grid.DataName = "grid content"
        Me.m_grid.DisplayRelativeValues = False
        Me.m_grid.FixedColumnWidths = True
        Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_grid.GridToolTipActive = True
        Me.m_grid.HarvestControlRule = Nothing
        Me.m_grid.IsLayoutSuspended = False
        Me.m_grid.IsOutputGrid = False
        Me.m_grid.Name = "m_grid"
        Me.m_grid.SelectedStrategy = Nothing
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
        'm_tsHCR
        '
        resources.ApplyResources(Me.m_tsHCR, "m_tsHCR")
        Me.m_tsHCR.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsHCR.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnAddHCR, Me.m_tsbnEditHCR, Me.m_tsbnDeleteHCR})
        Me.m_tsHCR.Name = "m_tsHCR"
        Me.m_tsHCR.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tsbnAddHCR
        '
        Me.m_tsbnAddHCR.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnAddHCR, "m_tsbnAddHCR")
        Me.m_tsbnAddHCR.Name = "m_tsbnAddHCR"
        '
        'm_tsbnEditHCR
        '
        Me.m_tsbnEditHCR.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnEditHCR, "m_tsbnEditHCR")
        Me.m_tsbnEditHCR.Name = "m_tsbnEditHCR"
        '
        'm_tsbnDeleteHCR
        '
        Me.m_tsbnDeleteHCR.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnDeleteHCR, "m_tsbnDeleteHCR")
        Me.m_tsbnDeleteHCR.Name = "m_tsbnDeleteHCR"
        '
        'm_hdrHCR
        '
        Me.m_hdrHCR.CanCollapseParent = False
        Me.m_hdrHCR.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrHCR, "m_hdrHCR")
        Me.m_hdrHCR.IsCollapsed = False
        Me.m_hdrHCR.Name = "m_hdrHCR"
        '
        'm_tlpRegulations
        '
        resources.ApplyResources(Me.m_tlpRegulations, "m_tlpRegulations")
        Me.m_tlpRegulations.Controls.Add(Me.m_hdrRegulations, 0, 0)
        Me.m_tlpRegulations.Controls.Add(Me.m_gridRegulations, 0, 1)
        Me.m_tlpRegulations.Name = "m_tlpRegulations"
        '
        'm_hdrRegulations
        '
        Me.m_hdrRegulations.CanCollapseParent = False
        Me.m_hdrRegulations.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrRegulations, "m_hdrRegulations")
        Me.m_hdrRegulations.IsCollapsed = False
        Me.m_hdrRegulations.Name = "m_hdrRegulations"
        '
        'm_gridRegulations
        '
        Me.m_gridRegulations.AllowBlockSelect = False
        Me.m_gridRegulations.AutoSizeMinHeight = 10
        Me.m_gridRegulations.AutoSizeMinWidth = 10
        Me.m_gridRegulations.AutoStretchColumnsToFitWidth = True
        Me.m_gridRegulations.AutoStretchRowsToFitHeight = False
        Me.m_gridRegulations.BackColor = System.Drawing.Color.White
        Me.m_gridRegulations.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_gridRegulations.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_gridRegulations.CustomSort = False
        Me.m_gridRegulations.DataName = "grid content"
        resources.ApplyResources(Me.m_gridRegulations, "m_gridRegulations")
        Me.m_gridRegulations.FixedColumnWidths = False
        Me.m_gridRegulations.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_gridRegulations.GridToolTipActive = True
        Me.m_gridRegulations.IsLayoutSuspended = False
        Me.m_gridRegulations.IsOutputGrid = False
        Me.m_gridRegulations.Name = "m_gridRegulations"
        Me.m_gridRegulations.SelectedStrategy = Nothing
        Me.m_gridRegulations.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_gridRegulations.UIContext = Nothing
        '
        'm_btnOK
        '
        resources.ApplyResources(Me.m_btnOK, "m_btnOK")
        Me.m_btnOK.Name = "m_btnOK"
        Me.m_btnOK.UseVisualStyleBackColor = True
        '
        'm_btnCancel
        '
        resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
        Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'frmTFMpolicy
        '
        Me.AcceptButton = Me.m_btnOK
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_scMain)
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_btnOK)
        Me.MinimizeBox = False
        Me.Name = "frmTFMpolicy"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.m_scMain.Panel1.ResumeLayout(False)
        Me.m_scMain.Panel1.PerformLayout()
        Me.m_scMain.Panel2.ResumeLayout(False)
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scMain.ResumeLayout(False)
        Me.m_tsStrategy.ResumeLayout(False)
        Me.m_tsStrategy.PerformLayout()
        Me.m_scDetails.Panel1.ResumeLayout(False)
        Me.m_scDetails.Panel2.ResumeLayout(False)
        CType(Me.m_scDetails, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scDetails.ResumeLayout(False)
        Me.m_tlpHCR.ResumeLayout(False)
        Me.m_tlpHCR.PerformLayout()
        Me.m_tsHCR.ResumeLayout(False)
        Me.m_tsHCR.PerformLayout()
        Me.m_tlpRegulations.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
    Private WithEvents m_tsHCR As cEwEToolstrip
    Private WithEvents m_grid As EwEMSEPlugin.gridTargetFishingMortalityPolicy
    Private WithEvents m_tsStrategy As cEwEToolstrip
    Private WithEvents m_tslSelectStratagy As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tscmStrategies As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_tsbnAddStrategy As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnDeleteStrategy As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnAddHCR As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnDeleteHCR As System.Windows.Forms.ToolStripButton
    Private WithEvents m_btnOK As System.Windows.Forms.Button
    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_scDetails As System.Windows.Forms.SplitContainer
    Private WithEvents m_tlpHCR As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_hdrHCR As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_tlpRegulations As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_hdrRegulations As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_gridRegulations As gridRegulations
    Private WithEvents m_graph As ZedGraph.ZedGraphControl
    Private WithEvents m_tsbnEditHCR As System.Windows.Forms.ToolStripButton
    Friend WithEvents m_chkUnits As CheckBox
End Class


