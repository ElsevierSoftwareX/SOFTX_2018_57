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
Imports SharedResources = ScientificInterfaceShared.My.Resources

Partial Class frmEcotracerOutput
    Inherits frmEwE

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcotracerOutput))
        Me.m_zgc = New ZedGraph.ZedGraphControl()
        Me.m_lbGroups = New ScientificInterfaceShared.Controls.cGroupListBox()
        Me.m_scMain = New System.Windows.Forms.SplitContainer()
        Me.m_tlpControls = New System.Windows.Forms.TableLayoutPanel()
        Me.m_plGroups = New System.Windows.Forms.Panel()
        Me.m_hdrGroups = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_plOpions = New System.Windows.Forms.Panel()
        Me.m_hdrPlotOptions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_cmbRegions = New System.Windows.Forms.ComboBox()
        Me.m_btnShowHideGroups = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.m_lblRegion = New System.Windows.Forms.Label()
        Me.m_rbCB = New System.Windows.Forms.RadioButton()
        Me.m_rbConc = New System.Windows.Forms.RadioButton()
        Me.m_plRun = New System.Windows.Forms.Panel()
        Me.m_cbAutosaveResults = New System.Windows.Forms.CheckBox()
        Me.m_hdrRun = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_btnRunSpace = New System.Windows.Forms.Button()
        Me.m_btnRunSim = New System.Windows.Forms.Button()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scMain.Panel1.SuspendLayout()
        Me.m_scMain.Panel2.SuspendLayout()
        Me.m_scMain.SuspendLayout()
        Me.m_tlpControls.SuspendLayout()
        Me.m_plGroups.SuspendLayout()
        Me.m_plOpions.SuspendLayout()
        Me.m_plRun.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_zgc
        '
        resources.ApplyResources(Me.m_zgc, "m_zgc")
        Me.m_zgc.Name = "m_zgc"
        Me.m_zgc.ScrollGrace = 0R
        Me.m_zgc.ScrollMaxX = 0R
        Me.m_zgc.ScrollMaxY = 0R
        Me.m_zgc.ScrollMaxY2 = 0R
        Me.m_zgc.ScrollMinX = 0R
        Me.m_zgc.ScrollMinY = 0R
        Me.m_zgc.ScrollMinY2 = 0R
        '
        'm_lbGroups
        '
        Me.m_lbGroups.AllGroupsItemColor = System.Drawing.Color.Black
        Me.m_lbGroups.AllGroupsItemText = "(Environment)"
        resources.ApplyResources(Me.m_lbGroups, "m_lbGroups")
        Me.m_lbGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.m_lbGroups.FormattingEnabled = True
        Me.m_lbGroups.GroupDisplayStyle = ScientificInterfaceShared.Controls.cGroupListBox.eGroupDisplayStyleTypes.DisplayVisibleOnly
        Me.m_lbGroups.IsAllGroupsItemSelected = False
        Me.m_lbGroups.Name = "m_lbGroups"
        Me.m_lbGroups.SelectedGroup = Nothing
        Me.m_lbGroups.SelectedGroupIndex = -1
        Me.m_lbGroups.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.m_lbGroups.SortThreshold = -9999.0!
        '
        'm_scMain
        '
        resources.ApplyResources(Me.m_scMain, "m_scMain")
        Me.m_scMain.Name = "m_scMain"
        '
        'm_scMain.Panel1
        '
        Me.m_scMain.Panel1.Controls.Add(Me.m_tlpControls)
        '
        'm_scMain.Panel2
        '
        Me.m_scMain.Panel2.Controls.Add(Me.m_zgc)
        '
        'm_tlpControls
        '
        resources.ApplyResources(Me.m_tlpControls, "m_tlpControls")
        Me.m_tlpControls.Controls.Add(Me.m_plGroups, 0, 0)
        Me.m_tlpControls.Controls.Add(Me.m_plOpions, 0, 1)
        Me.m_tlpControls.Controls.Add(Me.m_plRun, 0, 2)
        Me.m_tlpControls.Name = "m_tlpControls"
        '
        'm_plGroups
        '
        Me.m_plGroups.Controls.Add(Me.m_hdrGroups)
        Me.m_plGroups.Controls.Add(Me.m_lbGroups)
        resources.ApplyResources(Me.m_plGroups, "m_plGroups")
        Me.m_plGroups.Name = "m_plGroups"
        '
        'm_hdrGroups
        '
        resources.ApplyResources(Me.m_hdrGroups, "m_hdrGroups")
        Me.m_hdrGroups.CanCollapseParent = False
        Me.m_hdrGroups.CollapsedParentHeight = 0
        Me.m_hdrGroups.IsCollapsed = False
        Me.m_hdrGroups.Name = "m_hdrGroups"
        '
        'm_plOpions
        '
        Me.m_plOpions.Controls.Add(Me.m_hdrPlotOptions)
        Me.m_plOpions.Controls.Add(Me.m_cmbRegions)
        Me.m_plOpions.Controls.Add(Me.m_btnShowHideGroups)
        Me.m_plOpions.Controls.Add(Me.Label1)
        Me.m_plOpions.Controls.Add(Me.m_lblRegion)
        Me.m_plOpions.Controls.Add(Me.m_rbCB)
        Me.m_plOpions.Controls.Add(Me.m_rbConc)
        resources.ApplyResources(Me.m_plOpions, "m_plOpions")
        Me.m_plOpions.Name = "m_plOpions"
        '
        'm_hdrPlotOptions
        '
        resources.ApplyResources(Me.m_hdrPlotOptions, "m_hdrPlotOptions")
        Me.m_hdrPlotOptions.CanCollapseParent = True
        Me.m_hdrPlotOptions.CollapsedParentHeight = 0
        Me.m_hdrPlotOptions.IsCollapsed = False
        Me.m_hdrPlotOptions.Name = "m_hdrPlotOptions"
        '
        'm_cmbRegions
        '
        resources.ApplyResources(Me.m_cmbRegions, "m_cmbRegions")
        Me.m_cmbRegions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbRegions.FormattingEnabled = True
        Me.m_cmbRegions.Name = "m_cmbRegions"
        '
        'm_btnShowHideGroups
        '
        resources.ApplyResources(Me.m_btnShowHideGroups, "m_btnShowHideGroups")
        Me.m_btnShowHideGroups.Name = "m_btnShowHideGroups"
        Me.m_btnShowHideGroups.UseVisualStyleBackColor = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'm_lblRegion
        '
        resources.ApplyResources(Me.m_lblRegion, "m_lblRegion")
        Me.m_lblRegion.Name = "m_lblRegion"
        '
        'm_rbCB
        '
        resources.ApplyResources(Me.m_rbCB, "m_rbCB")
        Me.m_rbCB.Checked = True
        Me.m_rbCB.Name = "m_rbCB"
        Me.m_rbCB.TabStop = True
        Me.m_rbCB.UseVisualStyleBackColor = True
        '
        'm_rbConc
        '
        resources.ApplyResources(Me.m_rbConc, "m_rbConc")
        Me.m_rbConc.Name = "m_rbConc"
        Me.m_rbConc.UseVisualStyleBackColor = True
        '
        'm_plRun
        '
        Me.m_plRun.Controls.Add(Me.m_cbAutosaveResults)
        Me.m_plRun.Controls.Add(Me.m_hdrRun)
        Me.m_plRun.Controls.Add(Me.m_btnRunSpace)
        Me.m_plRun.Controls.Add(Me.m_btnRunSim)
        resources.ApplyResources(Me.m_plRun, "m_plRun")
        Me.m_plRun.Name = "m_plRun"
        '
        'm_cbAutosaveResults
        '
        resources.ApplyResources(Me.m_cbAutosaveResults, "m_cbAutosaveResults")
        Me.m_cbAutosaveResults.Name = "m_cbAutosaveResults"
        Me.m_cbAutosaveResults.UseVisualStyleBackColor = True
        '
        'm_hdrRun
        '
        resources.ApplyResources(Me.m_hdrRun, "m_hdrRun")
        Me.m_hdrRun.CanCollapseParent = False
        Me.m_hdrRun.CollapsedParentHeight = 0
        Me.m_hdrRun.IsCollapsed = False
        Me.m_hdrRun.Name = "m_hdrRun"
        '
        'm_btnRunSpace
        '
        resources.ApplyResources(Me.m_btnRunSpace, "m_btnRunSpace")
        Me.m_btnRunSpace.Name = "m_btnRunSpace"
        Me.m_btnRunSpace.UseVisualStyleBackColor = True
        '
        'm_btnRunSim
        '
        resources.ApplyResources(Me.m_btnRunSim, "m_btnRunSim")
        Me.m_btnRunSim.Name = "m_btnRunSim"
        Me.m_btnRunSim.UseVisualStyleBackColor = True
        '
        'frmEcotracerOutput
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_scMain)
        Me.Name = "frmEcotracerOutput"
        Me.TabText = ""
        Me.m_scMain.Panel1.ResumeLayout(False)
        Me.m_scMain.Panel2.ResumeLayout(False)
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scMain.ResumeLayout(False)
        Me.m_tlpControls.ResumeLayout(False)
        Me.m_plGroups.ResumeLayout(False)
        Me.m_plOpions.ResumeLayout(False)
        Me.m_plOpions.PerformLayout()
        Me.m_plRun.ResumeLayout(False)
        Me.m_plRun.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_hdrRun As cEwEHeaderLabel
    Private WithEvents m_btnRunSim As System.Windows.Forms.Button
    Private WithEvents m_btnRunSpace As System.Windows.Forms.Button
    Private WithEvents m_hdrPlotOptions As cEwEHeaderLabel
    Private WithEvents m_btnShowHideGroups As System.Windows.Forms.Button
    Private WithEvents m_rbCB As System.Windows.Forms.RadioButton
    Private WithEvents m_rbConc As System.Windows.Forms.RadioButton
    Private WithEvents m_zgc As ZedGraph.ZedGraphControl
    Private WithEvents m_cmbRegions As System.Windows.Forms.ComboBox
    Private WithEvents m_lbGroups As cGroupListBox
    Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
    Private WithEvents m_lblRegion As System.Windows.Forms.Label
    Private WithEvents m_hdrGroups As cEwEHeaderLabel
    Private WithEvents m_tlpControls As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_plGroups As System.Windows.Forms.Panel
    Private WithEvents m_plOpions As System.Windows.Forms.Panel
    Private WithEvents m_plRun As System.Windows.Forms.Panel
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents m_cbAutosaveResults As System.Windows.Forms.CheckBox
End Class
