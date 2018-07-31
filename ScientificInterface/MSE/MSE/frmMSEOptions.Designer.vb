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
Imports ScientificInterfaceShared.Controls

Partial Class frmMSEOptions
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
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMSEOptions))
        Me.m_pnlRegOpt = New System.Windows.Forms.Panel()
        Me.m_panelRegControls = New System.Windows.Forms.Panel()
        Me.m_rbQuotaControls = New System.Windows.Forms.RadioButton()
        Me.m_rbEffortControls = New System.Windows.Forms.RadioButton()
        Me.m_rbUseRegs = New System.Windows.Forms.RadioButton()
        Me.m_rbNoRegs = New System.Windows.Forms.RadioButton()
        Me.m_hdrOptions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_panelNoReg = New System.Windows.Forms.Panel()
        Me.m_txSBPower = New System.Windows.Forms.TextBox()
        Me.m_hdrNoReg = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lblSBPower = New System.Windows.Forms.Label()
        Me.m_rbExact = New System.Windows.Forms.RadioButton()
        Me.m_rbCatchEstBio = New System.Windows.Forms.RadioButton()
        Me.m_rbDirectExp = New System.Windows.Forms.RadioButton()
        Me.m_panelEffortControls = New System.Windows.Forms.Panel()
        Me.m_hdrEffort = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_gridFleetLPEffortBounds = New ScientificInterface.gridFleetLPEffortBounds()
        Me.m_panelQuotaControls = New System.Windows.Forms.Panel()
        Me.m_hdrQuota = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrEffortRegOptions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_txMaxEffort = New System.Windows.Forms.TextBox()
        Me.m_rbEffortEcosim = New System.Windows.Forms.RadioButton()
        Me.m_rbEffortNoCap = New System.Windows.Forms.RadioButton()
        Me.m_rbEffortPredicted = New System.Windows.Forms.RadioButton()
        Me.m_gridRegOptions = New ScientificInterface.Ecosim.gridRegulatoryOptions()
        Me.m_plControls = New System.Windows.Forms.Panel()
        Me.m_pnlRegOpt.SuspendLayout()
        Me.m_panelRegControls.SuspendLayout()
        Me.m_panelNoReg.SuspendLayout()
        Me.m_panelEffortControls.SuspendLayout()
        Me.m_panelQuotaControls.SuspendLayout()
        Me.m_plControls.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_pnlRegOpt
        '
        resources.ApplyResources(Me.m_pnlRegOpt, "m_pnlRegOpt")
        Me.m_pnlRegOpt.Controls.Add(Me.m_panelRegControls)
        Me.m_pnlRegOpt.Controls.Add(Me.m_rbUseRegs)
        Me.m_pnlRegOpt.Controls.Add(Me.m_rbNoRegs)
        Me.m_pnlRegOpt.Controls.Add(Me.m_hdrOptions)
        Me.m_pnlRegOpt.Name = "m_pnlRegOpt"
        '
        'm_panelRegControls
        '
        Me.m_panelRegControls.Controls.Add(Me.m_rbQuotaControls)
        Me.m_panelRegControls.Controls.Add(Me.m_rbEffortControls)
        resources.ApplyResources(Me.m_panelRegControls, "m_panelRegControls")
        Me.m_panelRegControls.Name = "m_panelRegControls"
        '
        'm_rbQuotaControls
        '
        resources.ApplyResources(Me.m_rbQuotaControls, "m_rbQuotaControls")
        Me.m_rbQuotaControls.Name = "m_rbQuotaControls"
        Me.m_rbQuotaControls.UseVisualStyleBackColor = True
        '
        'm_rbEffortControls
        '
        resources.ApplyResources(Me.m_rbEffortControls, "m_rbEffortControls")
        Me.m_rbEffortControls.Checked = True
        Me.m_rbEffortControls.Name = "m_rbEffortControls"
        Me.m_rbEffortControls.TabStop = True
        Me.m_rbEffortControls.UseVisualStyleBackColor = True
        '
        'm_rbUseRegs
        '
        resources.ApplyResources(Me.m_rbUseRegs, "m_rbUseRegs")
        Me.m_rbUseRegs.Checked = True
        Me.m_rbUseRegs.Name = "m_rbUseRegs"
        Me.m_rbUseRegs.TabStop = True
        Me.m_rbUseRegs.UseVisualStyleBackColor = True
        '
        'm_rbNoRegs
        '
        resources.ApplyResources(Me.m_rbNoRegs, "m_rbNoRegs")
        Me.m_rbNoRegs.Name = "m_rbNoRegs"
        Me.m_rbNoRegs.UseVisualStyleBackColor = True
        '
        'm_hdrOptions
        '
        Me.m_hdrOptions.CanCollapseParent = False
        Me.m_hdrOptions.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrOptions, "m_hdrOptions")
        Me.m_hdrOptions.IsCollapsed = False
        Me.m_hdrOptions.Name = "m_hdrOptions"
        '
        'm_panelNoReg
        '
        resources.ApplyResources(Me.m_panelNoReg, "m_panelNoReg")
        Me.m_panelNoReg.Controls.Add(Me.m_txSBPower)
        Me.m_panelNoReg.Controls.Add(Me.m_hdrNoReg)
        Me.m_panelNoReg.Controls.Add(Me.m_lblSBPower)
        Me.m_panelNoReg.Controls.Add(Me.m_rbExact)
        Me.m_panelNoReg.Controls.Add(Me.m_rbCatchEstBio)
        Me.m_panelNoReg.Controls.Add(Me.m_rbDirectExp)
        Me.m_panelNoReg.Name = "m_panelNoReg"
        '
        'm_txSBPower
        '
        resources.ApplyResources(Me.m_txSBPower, "m_txSBPower")
        Me.m_txSBPower.Name = "m_txSBPower"
        '
        'm_hdrNoReg
        '
        resources.ApplyResources(Me.m_hdrNoReg, "m_hdrNoReg")
        Me.m_hdrNoReg.CanCollapseParent = False
        Me.m_hdrNoReg.CollapsedParentHeight = 0
        Me.m_hdrNoReg.IsCollapsed = False
        Me.m_hdrNoReg.Name = "m_hdrNoReg"
        '
        'm_lblSBPower
        '
        resources.ApplyResources(Me.m_lblSBPower, "m_lblSBPower")
        Me.m_lblSBPower.Name = "m_lblSBPower"
        '
        'm_rbExact
        '
        resources.ApplyResources(Me.m_rbExact, "m_rbExact")
        Me.m_rbExact.Name = "m_rbExact"
        Me.m_rbExact.TabStop = True
        Me.m_rbExact.UseVisualStyleBackColor = True
        '
        'm_rbCatchEstBio
        '
        resources.ApplyResources(Me.m_rbCatchEstBio, "m_rbCatchEstBio")
        Me.m_rbCatchEstBio.Checked = True
        Me.m_rbCatchEstBio.Name = "m_rbCatchEstBio"
        Me.m_rbCatchEstBio.TabStop = True
        Me.m_rbCatchEstBio.UseVisualStyleBackColor = True
        '
        'm_rbDirectExp
        '
        resources.ApplyResources(Me.m_rbDirectExp, "m_rbDirectExp")
        Me.m_rbDirectExp.Name = "m_rbDirectExp"
        Me.m_rbDirectExp.UseVisualStyleBackColor = True
        '
        'm_panelEffortControls
        '
        Me.m_panelEffortControls.Controls.Add(Me.m_hdrEffort)
        Me.m_panelEffortControls.Controls.Add(Me.m_gridFleetLPEffortBounds)
        resources.ApplyResources(Me.m_panelEffortControls, "m_panelEffortControls")
        Me.m_panelEffortControls.Name = "m_panelEffortControls"
        '
        'm_hdrEffort
        '
        resources.ApplyResources(Me.m_hdrEffort, "m_hdrEffort")
        Me.m_hdrEffort.CanCollapseParent = False
        Me.m_hdrEffort.CollapsedParentHeight = 0
        Me.m_hdrEffort.IsCollapsed = False
        Me.m_hdrEffort.Name = "m_hdrEffort"
        '
        'm_gridFleetLPEffortBounds
        '
        Me.m_gridFleetLPEffortBounds.AllowBlockSelect = True
        resources.ApplyResources(Me.m_gridFleetLPEffortBounds, "m_gridFleetLPEffortBounds")
        Me.m_gridFleetLPEffortBounds.AutoSizeMinHeight = 10
        Me.m_gridFleetLPEffortBounds.AutoSizeMinWidth = 10
        Me.m_gridFleetLPEffortBounds.AutoStretchColumnsToFitWidth = False
        Me.m_gridFleetLPEffortBounds.AutoStretchRowsToFitHeight = False
        Me.m_gridFleetLPEffortBounds.BackColor = System.Drawing.Color.White
        Me.m_gridFleetLPEffortBounds.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_gridFleetLPEffortBounds.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_gridFleetLPEffortBounds.CustomSort = False
        Me.m_gridFleetLPEffortBounds.DataName = "grid content"
        Me.m_gridFleetLPEffortBounds.FixedColumnWidths = False
        Me.m_gridFleetLPEffortBounds.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_gridFleetLPEffortBounds.GridToolTipActive = True
        Me.m_gridFleetLPEffortBounds.IsLayoutSuspended = False
        Me.m_gridFleetLPEffortBounds.IsOutputGrid = True
        Me.m_gridFleetLPEffortBounds.Name = "m_gridFleetLPEffortBounds"
        Me.m_gridFleetLPEffortBounds.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_gridFleetLPEffortBounds.UIContext = Nothing
        '
        'm_panelQuotaControls
        '
        Me.m_panelQuotaControls.Controls.Add(Me.m_hdrQuota)
        Me.m_panelQuotaControls.Controls.Add(Me.m_hdrEffortRegOptions)
        Me.m_panelQuotaControls.Controls.Add(Me.m_txMaxEffort)
        Me.m_panelQuotaControls.Controls.Add(Me.m_rbEffortEcosim)
        Me.m_panelQuotaControls.Controls.Add(Me.m_rbEffortNoCap)
        Me.m_panelQuotaControls.Controls.Add(Me.m_rbEffortPredicted)
        Me.m_panelQuotaControls.Controls.Add(Me.m_gridRegOptions)
        resources.ApplyResources(Me.m_panelQuotaControls, "m_panelQuotaControls")
        Me.m_panelQuotaControls.Name = "m_panelQuotaControls"
        '
        'm_hdrQuota
        '
        resources.ApplyResources(Me.m_hdrQuota, "m_hdrQuota")
        Me.m_hdrQuota.CanCollapseParent = False
        Me.m_hdrQuota.CollapsedParentHeight = 0
        Me.m_hdrQuota.IsCollapsed = False
        Me.m_hdrQuota.Name = "m_hdrQuota"
        '
        'm_hdrEffortRegOptions
        '
        resources.ApplyResources(Me.m_hdrEffortRegOptions, "m_hdrEffortRegOptions")
        Me.m_hdrEffortRegOptions.CanCollapseParent = False
        Me.m_hdrEffortRegOptions.CollapsedParentHeight = 0
        Me.m_hdrEffortRegOptions.IsCollapsed = False
        Me.m_hdrEffortRegOptions.Name = "m_hdrEffortRegOptions"
        '
        'm_txMaxEffort
        '
        resources.ApplyResources(Me.m_txMaxEffort, "m_txMaxEffort")
        Me.m_txMaxEffort.Name = "m_txMaxEffort"
        '
        'm_rbEffortEcosim
        '
        resources.ApplyResources(Me.m_rbEffortEcosim, "m_rbEffortEcosim")
        Me.m_rbEffortEcosim.Name = "m_rbEffortEcosim"
        Me.m_rbEffortEcosim.UseVisualStyleBackColor = True
        '
        'm_rbEffortNoCap
        '
        resources.ApplyResources(Me.m_rbEffortNoCap, "m_rbEffortNoCap")
        Me.m_rbEffortNoCap.Checked = True
        Me.m_rbEffortNoCap.Name = "m_rbEffortNoCap"
        Me.m_rbEffortNoCap.TabStop = True
        Me.m_rbEffortNoCap.UseVisualStyleBackColor = True
        '
        'm_rbEffortPredicted
        '
        resources.ApplyResources(Me.m_rbEffortPredicted, "m_rbEffortPredicted")
        Me.m_rbEffortPredicted.Name = "m_rbEffortPredicted"
        Me.m_rbEffortPredicted.UseVisualStyleBackColor = True
        '
        'm_gridRegOptions
        '
        Me.m_gridRegOptions.AllowBlockSelect = True
        resources.ApplyResources(Me.m_gridRegOptions, "m_gridRegOptions")
        Me.m_gridRegOptions.AutoSizeMinHeight = 10
        Me.m_gridRegOptions.AutoSizeMinWidth = 10
        Me.m_gridRegOptions.AutoStretchColumnsToFitWidth = False
        Me.m_gridRegOptions.AutoStretchRowsToFitHeight = False
        Me.m_gridRegOptions.BackColor = System.Drawing.Color.White
        Me.m_gridRegOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_gridRegOptions.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_gridRegOptions.CustomSort = False
        Me.m_gridRegOptions.DataName = "grid content"
        Me.m_gridRegOptions.FixedColumnWidths = True
        Me.m_gridRegOptions.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_gridRegOptions.GridToolTipActive = True
        Me.m_gridRegOptions.IsLayoutSuspended = False
        Me.m_gridRegOptions.IsOutputGrid = True
        Me.m_gridRegOptions.Name = "m_gridRegOptions"
        Me.m_gridRegOptions.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_gridRegOptions.UIContext = Nothing
        '
        'm_plControls
        '
        resources.ApplyResources(Me.m_plControls, "m_plControls")
        Me.m_plControls.Controls.Add(Me.m_panelNoReg)
        Me.m_plControls.Controls.Add(Me.m_panelEffortControls)
        Me.m_plControls.Controls.Add(Me.m_panelQuotaControls)
        Me.m_plControls.Name = "m_plControls"
        '
        'frmMSEOptions
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_plControls)
        Me.Controls.Add(Me.m_pnlRegOpt)
        Me.Name = "frmMSEOptions"
        Me.TabText = ""
        Me.m_pnlRegOpt.ResumeLayout(False)
        Me.m_pnlRegOpt.PerformLayout()
        Me.m_panelRegControls.ResumeLayout(False)
        Me.m_panelRegControls.PerformLayout()
        Me.m_panelNoReg.ResumeLayout(False)
        Me.m_panelNoReg.PerformLayout()
        Me.m_panelEffortControls.ResumeLayout(False)
        Me.m_panelQuotaControls.ResumeLayout(False)
        Me.m_panelQuotaControls.PerformLayout()
        Me.m_plControls.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_pnlRegOpt As System.Windows.Forms.Panel
    Private WithEvents m_hdrOptions As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_gridFleetLPEffortBounds As ScientificInterface.gridFleetLPEffortBounds
    Private WithEvents m_hdrQuota As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_hdrEffortRegOptions As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Friend WithEvents m_txMaxEffort As System.Windows.Forms.TextBox
    Private WithEvents m_rbEffortEcosim As System.Windows.Forms.RadioButton
    Private WithEvents m_rbEffortNoCap As System.Windows.Forms.RadioButton
    Private WithEvents m_rbEffortPredicted As System.Windows.Forms.RadioButton
    Friend WithEvents m_gridRegOptions As ScientificInterface.Ecosim.gridRegulatoryOptions
    Friend WithEvents m_rbQuotaControls As System.Windows.Forms.RadioButton
    Friend WithEvents m_rbEffortControls As System.Windows.Forms.RadioButton
    Private WithEvents m_rbUseRegs As System.Windows.Forms.RadioButton
    Private WithEvents m_rbNoRegs As System.Windows.Forms.RadioButton
    Private WithEvents m_hdrEffort As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_panelNoReg As System.Windows.Forms.Panel
    Private WithEvents m_txSBPower As System.Windows.Forms.TextBox
    Private WithEvents m_hdrNoReg As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_lblSBPower As System.Windows.Forms.Label
    Private WithEvents m_rbExact As System.Windows.Forms.RadioButton
    Private WithEvents m_rbCatchEstBio As System.Windows.Forms.RadioButton
    Private WithEvents m_rbDirectExp As System.Windows.Forms.RadioButton
    Private WithEvents m_panelEffortControls As System.Windows.Forms.Panel
    Private WithEvents m_panelQuotaControls As System.Windows.Forms.Panel
    Private WithEvents m_panelRegControls As System.Windows.Forms.Panel
    Private WithEvents m_plControls As System.Windows.Forms.Panel
End Class
