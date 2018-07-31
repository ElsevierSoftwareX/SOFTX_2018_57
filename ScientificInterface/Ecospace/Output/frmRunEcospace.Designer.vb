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
Imports ZedGraph

Namespace Ecospace

    Partial Class frmRunEcospace
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmRunEcospace))
            Me.m_btnRun = New System.Windows.Forms.Button()
            Me.m_cmbDisplayItem = New System.Windows.Forms.ComboBox()
            Me.m_rbShowSingle = New System.Windows.Forms.RadioButton()
            Me.m_rbShowNonHidden = New System.Windows.Forms.RadioButton()
            Me.m_rbShowAll = New System.Windows.Forms.RadioButton()
            Me.m_cbOverlay = New System.Windows.Forms.CheckBox()
            Me.m_btnStop = New System.Windows.Forms.Button()
            Me.m_pbMap = New System.Windows.Forms.PictureBox()
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_tlpOptions = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plMapSaveImages = New System.Windows.Forms.Panel()
            Me.m_tbxAutosaveTimeSteps = New System.Windows.Forms.TextBox()
            Me.m_lblAutosaveTimeSteps = New System.Windows.Forms.Label()
            Me.m_cbAutoSavePNG = New System.Windows.Forms.CheckBox()
            Me.m_hdrAutosave = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plMapLabels = New System.Windows.Forms.Panel()
            Me.m_cbInvertColor = New System.Windows.Forms.CheckBox()
            Me.m_cmbLabelPos = New System.Windows.Forms.ComboBox()
            Me.m_cbShowDateInLabel = New System.Windows.Forms.CheckBox()
            Me.m_cbShowLabels = New System.Windows.Forms.CheckBox()
            Me.m_hdrLabelOptions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plMapData = New System.Windows.Forms.Panel()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.m_txFMax = New System.Windows.Forms.TextBox()
            Me.m_rbDisplayF = New System.Windows.Forms.RadioButton()
            Me.m_rbDisplayRelBiomass = New System.Windows.Forms.RadioButton()
            Me.m_rbDisplayFOverB = New System.Windows.Forms.RadioButton()
            Me.m_rbDisplayFishingEffort = New System.Windows.Forms.RadioButton()
            Me.m_rbDisplayContaminantC = New System.Windows.Forms.RadioButton()
            Me.m_hdrDist = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_rbDisplayDiscards = New System.Windows.Forms.RadioButton()
            Me.m_rbDisplayComputedHabitatCapacity = New System.Windows.Forms.RadioButton()
            Me.m_rbDisplayEnvDriver = New System.Windows.Forms.RadioButton()
            Me.m_rbDisplayCoverB = New System.Windows.Forms.RadioButton()
            Me.m_plDisplayOptions = New System.Windows.Forms.Panel()
            Me.m_btnDisplayGroups1 = New System.Windows.Forms.Button()
            Me.m_cbShowIBMPackets = New System.Windows.Forms.CheckBox()
            Me.m_hdrDispOpt = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_cbMPA = New System.Windows.Forms.CheckBox()
            Me.m_plRun = New System.Windows.Forms.Panel()
            Me.m_hdrRunning = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tlpRun = New System.Windows.Forms.TableLayoutPanel()
            Me.m_btnPause = New System.Windows.Forms.Button()
            Me.m_cmbRunType = New System.Windows.Forms.ComboBox()
            Me.m_plGraphData = New System.Windows.Forms.Panel()
            Me.m_rbCatchGraph = New System.Windows.Forms.RadioButton()
            Me.m_rbConsumpGraph = New System.Windows.Forms.RadioButton()
            Me.m_rbPredMortGraph = New System.Windows.Forms.RadioButton()
            Me.m_rbFishMortGraph = New System.Windows.Forms.RadioButton()
            Me.m_rbRelBiomassGraph = New System.Windows.Forms.RadioButton()
            Me.m_hdrGraphTypes = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tcOutputs = New System.Windows.Forms.TabControl()
            Me.m_tabMap = New System.Windows.Forms.TabPage()
            Me.m_legend = New ScientificInterfaceShared.Controls.ucLegendBar()
            Me.m_tabGraph = New System.Windows.Forms.TabPage()
            Me.m_zgPlotLarge = New ZedGraph.ZedGraphControl()
            CType(Me.m_pbMap, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            Me.m_tlpOptions.SuspendLayout()
            Me.m_plMapSaveImages.SuspendLayout()
            Me.m_plMapLabels.SuspendLayout()
            Me.m_plMapData.SuspendLayout()
            Me.m_plDisplayOptions.SuspendLayout()
            Me.m_plRun.SuspendLayout()
            Me.m_tlpRun.SuspendLayout()
            Me.m_plGraphData.SuspendLayout()
            Me.m_tcOutputs.SuspendLayout()
            Me.m_tabMap.SuspendLayout()
            Me.m_tabGraph.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_btnRun
            '
            resources.ApplyResources(Me.m_btnRun, "m_btnRun")
            Me.m_btnRun.Name = "m_btnRun"
            Me.m_btnRun.UseVisualStyleBackColor = True
            '
            'm_cmbDisplayItem
            '
            resources.ApplyResources(Me.m_cmbDisplayItem, "m_cmbDisplayItem")
            Me.m_cmbDisplayItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbDisplayItem.FormattingEnabled = True
            Me.m_cmbDisplayItem.Name = "m_cmbDisplayItem"
            '
            'm_rbShowSingle
            '
            resources.ApplyResources(Me.m_rbShowSingle, "m_rbShowSingle")
            Me.m_rbShowSingle.Name = "m_rbShowSingle"
            Me.m_rbShowSingle.UseVisualStyleBackColor = True
            '
            'm_rbShowNonHidden
            '
            resources.ApplyResources(Me.m_rbShowNonHidden, "m_rbShowNonHidden")
            Me.m_rbShowNonHidden.Name = "m_rbShowNonHidden"
            Me.m_rbShowNonHidden.UseVisualStyleBackColor = True
            '
            'm_rbShowAll
            '
            resources.ApplyResources(Me.m_rbShowAll, "m_rbShowAll")
            Me.m_rbShowAll.Checked = True
            Me.m_rbShowAll.Name = "m_rbShowAll"
            Me.m_rbShowAll.TabStop = True
            Me.m_rbShowAll.UseVisualStyleBackColor = True
            '
            'm_cbOverlay
            '
            resources.ApplyResources(Me.m_cbOverlay, "m_cbOverlay")
            Me.m_cbOverlay.Name = "m_cbOverlay"
            Me.m_cbOverlay.UseVisualStyleBackColor = True
            '
            'm_btnStop
            '
            resources.ApplyResources(Me.m_btnStop, "m_btnStop")
            Me.m_btnStop.Name = "m_btnStop"
            Me.m_btnStop.UseVisualStyleBackColor = True
            '
            'm_pbMap
            '
            resources.ApplyResources(Me.m_pbMap, "m_pbMap")
            Me.m_pbMap.BackColor = System.Drawing.SystemColors.ControlDark
            Me.m_pbMap.Name = "m_pbMap"
            Me.m_pbMap.TabStop = False
            '
            'm_scMain
            '
            resources.ApplyResources(Me.m_scMain, "m_scMain")
            Me.m_scMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            Me.m_scMain.Panel1.Controls.Add(Me.m_tlpOptions)
            '
            'm_scMain.Panel2
            '
            Me.m_scMain.Panel2.Controls.Add(Me.m_tcOutputs)
            '
            'm_tlpOptions
            '
            resources.ApplyResources(Me.m_tlpOptions, "m_tlpOptions")
            Me.m_tlpOptions.Controls.Add(Me.m_plMapSaveImages, 0, 3)
            Me.m_tlpOptions.Controls.Add(Me.m_plMapLabels, 0, 2)
            Me.m_tlpOptions.Controls.Add(Me.m_plMapData, 0, 1)
            Me.m_tlpOptions.Controls.Add(Me.m_plDisplayOptions, 0, 0)
            Me.m_tlpOptions.Controls.Add(Me.m_plRun, 0, 6)
            Me.m_tlpOptions.Controls.Add(Me.m_plGraphData, 0, 4)
            Me.m_tlpOptions.Name = "m_tlpOptions"
            '
            'm_plMapSaveImages
            '
            Me.m_plMapSaveImages.Controls.Add(Me.m_tbxAutosaveTimeSteps)
            Me.m_plMapSaveImages.Controls.Add(Me.m_lblAutosaveTimeSteps)
            Me.m_plMapSaveImages.Controls.Add(Me.m_cbAutoSavePNG)
            Me.m_plMapSaveImages.Controls.Add(Me.m_hdrAutosave)
            resources.ApplyResources(Me.m_plMapSaveImages, "m_plMapSaveImages")
            Me.m_plMapSaveImages.Name = "m_plMapSaveImages"
            '
            'm_tbxAutosaveTimeSteps
            '
            resources.ApplyResources(Me.m_tbxAutosaveTimeSteps, "m_tbxAutosaveTimeSteps")
            Me.m_tbxAutosaveTimeSteps.Name = "m_tbxAutosaveTimeSteps"
            '
            'm_lblAutosaveTimeSteps
            '
            resources.ApplyResources(Me.m_lblAutosaveTimeSteps, "m_lblAutosaveTimeSteps")
            Me.m_lblAutosaveTimeSteps.Name = "m_lblAutosaveTimeSteps"
            '
            'm_cbAutoSavePNG
            '
            resources.ApplyResources(Me.m_cbAutoSavePNG, "m_cbAutoSavePNG")
            Me.m_cbAutoSavePNG.Name = "m_cbAutoSavePNG"
            Me.m_cbAutoSavePNG.UseVisualStyleBackColor = True
            '
            'm_hdrAutosave
            '
            Me.m_hdrAutosave.CanCollapseParent = True
            Me.m_hdrAutosave.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrAutosave, "m_hdrAutosave")
            Me.m_hdrAutosave.IsCollapsed = False
            Me.m_hdrAutosave.Name = "m_hdrAutosave"
            '
            'm_plMapLabels
            '
            Me.m_plMapLabels.Controls.Add(Me.m_cbInvertColor)
            Me.m_plMapLabels.Controls.Add(Me.m_cmbLabelPos)
            Me.m_plMapLabels.Controls.Add(Me.m_cbShowDateInLabel)
            Me.m_plMapLabels.Controls.Add(Me.m_cbShowLabels)
            Me.m_plMapLabels.Controls.Add(Me.m_hdrLabelOptions)
            resources.ApplyResources(Me.m_plMapLabels, "m_plMapLabels")
            Me.m_plMapLabels.Name = "m_plMapLabels"
            '
            'm_cbInvertColor
            '
            resources.ApplyResources(Me.m_cbInvertColor, "m_cbInvertColor")
            Me.m_cbInvertColor.Name = "m_cbInvertColor"
            Me.m_cbInvertColor.UseVisualStyleBackColor = True
            '
            'm_cmbLabelPos
            '
            resources.ApplyResources(Me.m_cmbLabelPos, "m_cmbLabelPos")
            Me.m_cmbLabelPos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbLabelPos.FormattingEnabled = True
            Me.m_cmbLabelPos.Items.AddRange(New Object() {resources.GetString("m_cmbLabelPos.Items"), resources.GetString("m_cmbLabelPos.Items1"), resources.GetString("m_cmbLabelPos.Items2"), resources.GetString("m_cmbLabelPos.Items3"), resources.GetString("m_cmbLabelPos.Items4"), resources.GetString("m_cmbLabelPos.Items5"), resources.GetString("m_cmbLabelPos.Items6"), resources.GetString("m_cmbLabelPos.Items7"), resources.GetString("m_cmbLabelPos.Items8")})
            Me.m_cmbLabelPos.Name = "m_cmbLabelPos"
            '
            'm_cbShowDateInLabel
            '
            resources.ApplyResources(Me.m_cbShowDateInLabel, "m_cbShowDateInLabel")
            Me.m_cbShowDateInLabel.Checked = True
            Me.m_cbShowDateInLabel.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_cbShowDateInLabel.Name = "m_cbShowDateInLabel"
            Me.m_cbShowDateInLabel.UseVisualStyleBackColor = True
            '
            'm_cbShowLabels
            '
            resources.ApplyResources(Me.m_cbShowLabels, "m_cbShowLabels")
            Me.m_cbShowLabels.Checked = True
            Me.m_cbShowLabels.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_cbShowLabels.Name = "m_cbShowLabels"
            Me.m_cbShowLabels.UseVisualStyleBackColor = True
            '
            'm_hdrLabelOptions
            '
            resources.ApplyResources(Me.m_hdrLabelOptions, "m_hdrLabelOptions")
            Me.m_hdrLabelOptions.CanCollapseParent = True
            Me.m_hdrLabelOptions.CollapsedParentHeight = 0
            Me.m_hdrLabelOptions.IsCollapsed = False
            Me.m_hdrLabelOptions.Name = "m_hdrLabelOptions"
            '
            'm_plMapData
            '
            Me.m_plMapData.Controls.Add(Me.Label1)
            Me.m_plMapData.Controls.Add(Me.m_txFMax)
            Me.m_plMapData.Controls.Add(Me.m_rbDisplayF)
            Me.m_plMapData.Controls.Add(Me.m_rbDisplayRelBiomass)
            Me.m_plMapData.Controls.Add(Me.m_rbDisplayFOverB)
            Me.m_plMapData.Controls.Add(Me.m_rbDisplayFishingEffort)
            Me.m_plMapData.Controls.Add(Me.m_rbDisplayContaminantC)
            Me.m_plMapData.Controls.Add(Me.m_hdrDist)
            Me.m_plMapData.Controls.Add(Me.m_rbDisplayDiscards)
            Me.m_plMapData.Controls.Add(Me.m_rbDisplayComputedHabitatCapacity)
            Me.m_plMapData.Controls.Add(Me.m_rbDisplayEnvDriver)
            Me.m_plMapData.Controls.Add(Me.m_rbDisplayCoverB)
            resources.ApplyResources(Me.m_plMapData, "m_plMapData")
            Me.m_plMapData.Name = "m_plMapData"
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'm_txFMax
            '
            resources.ApplyResources(Me.m_txFMax, "m_txFMax")
            Me.m_txFMax.Name = "m_txFMax"
            '
            'm_rbDisplayF
            '
            resources.ApplyResources(Me.m_rbDisplayF, "m_rbDisplayF")
            Me.m_rbDisplayF.Name = "m_rbDisplayF"
            Me.m_rbDisplayF.UseVisualStyleBackColor = True
            '
            'm_rbDisplayRelBiomass
            '
            resources.ApplyResources(Me.m_rbDisplayRelBiomass, "m_rbDisplayRelBiomass")
            Me.m_rbDisplayRelBiomass.Checked = True
            Me.m_rbDisplayRelBiomass.Name = "m_rbDisplayRelBiomass"
            Me.m_rbDisplayRelBiomass.TabStop = True
            Me.m_rbDisplayRelBiomass.UseVisualStyleBackColor = True
            '
            'm_rbDisplayFOverB
            '
            resources.ApplyResources(Me.m_rbDisplayFOverB, "m_rbDisplayFOverB")
            Me.m_rbDisplayFOverB.Name = "m_rbDisplayFOverB"
            Me.m_rbDisplayFOverB.UseVisualStyleBackColor = True
            '
            'm_rbDisplayFishingEffort
            '
            resources.ApplyResources(Me.m_rbDisplayFishingEffort, "m_rbDisplayFishingEffort")
            Me.m_rbDisplayFishingEffort.Name = "m_rbDisplayFishingEffort"
            Me.m_rbDisplayFishingEffort.UseVisualStyleBackColor = True
            '
            'm_rbDisplayContaminantC
            '
            resources.ApplyResources(Me.m_rbDisplayContaminantC, "m_rbDisplayContaminantC")
            Me.m_rbDisplayContaminantC.Name = "m_rbDisplayContaminantC"
            Me.m_rbDisplayContaminantC.UseVisualStyleBackColor = True
            '
            'm_hdrDist
            '
            resources.ApplyResources(Me.m_hdrDist, "m_hdrDist")
            Me.m_hdrDist.CanCollapseParent = True
            Me.m_hdrDist.CollapsedParentHeight = 0
            Me.m_hdrDist.IsCollapsed = False
            Me.m_hdrDist.Name = "m_hdrDist"
            '
            'm_rbDisplayDiscards
            '
            resources.ApplyResources(Me.m_rbDisplayDiscards, "m_rbDisplayDiscards")
            Me.m_rbDisplayDiscards.Name = "m_rbDisplayDiscards"
            Me.m_rbDisplayDiscards.UseVisualStyleBackColor = True
            '
            'm_rbDisplayComputedHabitatCapacity
            '
            resources.ApplyResources(Me.m_rbDisplayComputedHabitatCapacity, "m_rbDisplayComputedHabitatCapacity")
            Me.m_rbDisplayComputedHabitatCapacity.Name = "m_rbDisplayComputedHabitatCapacity"
            Me.m_rbDisplayComputedHabitatCapacity.UseVisualStyleBackColor = True
            '
            'm_rbDisplayEnvDriver
            '
            resources.ApplyResources(Me.m_rbDisplayEnvDriver, "m_rbDisplayEnvDriver")
            Me.m_rbDisplayEnvDriver.Name = "m_rbDisplayEnvDriver"
            Me.m_rbDisplayEnvDriver.UseVisualStyleBackColor = True
            '
            'm_rbDisplayCoverB
            '
            resources.ApplyResources(Me.m_rbDisplayCoverB, "m_rbDisplayCoverB")
            Me.m_rbDisplayCoverB.Name = "m_rbDisplayCoverB"
            Me.m_rbDisplayCoverB.UseVisualStyleBackColor = True
            '
            'm_plDisplayOptions
            '
            Me.m_plDisplayOptions.Controls.Add(Me.m_btnDisplayGroups1)
            Me.m_plDisplayOptions.Controls.Add(Me.m_rbShowAll)
            Me.m_plDisplayOptions.Controls.Add(Me.m_cbShowIBMPackets)
            Me.m_plDisplayOptions.Controls.Add(Me.m_hdrDispOpt)
            Me.m_plDisplayOptions.Controls.Add(Me.m_cbMPA)
            Me.m_plDisplayOptions.Controls.Add(Me.m_cbOverlay)
            Me.m_plDisplayOptions.Controls.Add(Me.m_rbShowNonHidden)
            Me.m_plDisplayOptions.Controls.Add(Me.m_rbShowSingle)
            Me.m_plDisplayOptions.Controls.Add(Me.m_cmbDisplayItem)
            resources.ApplyResources(Me.m_plDisplayOptions, "m_plDisplayOptions")
            Me.m_plDisplayOptions.Name = "m_plDisplayOptions"
            '
            'm_btnDisplayGroups1
            '
            resources.ApplyResources(Me.m_btnDisplayGroups1, "m_btnDisplayGroups1")
            Me.m_btnDisplayGroups1.Name = "m_btnDisplayGroups1"
            Me.m_btnDisplayGroups1.UseVisualStyleBackColor = True
            '
            'm_cbShowIBMPackets
            '
            resources.ApplyResources(Me.m_cbShowIBMPackets, "m_cbShowIBMPackets")
            Me.m_cbShowIBMPackets.Checked = True
            Me.m_cbShowIBMPackets.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_cbShowIBMPackets.Name = "m_cbShowIBMPackets"
            Me.m_cbShowIBMPackets.UseVisualStyleBackColor = True
            '
            'm_hdrDispOpt
            '
            resources.ApplyResources(Me.m_hdrDispOpt, "m_hdrDispOpt")
            Me.m_hdrDispOpt.CanCollapseParent = True
            Me.m_hdrDispOpt.CollapsedParentHeight = 0
            Me.m_hdrDispOpt.IsCollapsed = False
            Me.m_hdrDispOpt.Name = "m_hdrDispOpt"
            '
            'm_cbMPA
            '
            resources.ApplyResources(Me.m_cbMPA, "m_cbMPA")
            Me.m_cbMPA.Checked = True
            Me.m_cbMPA.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_cbMPA.Name = "m_cbMPA"
            Me.m_cbMPA.UseVisualStyleBackColor = True
            '
            'm_plRun
            '
            resources.ApplyResources(Me.m_plRun, "m_plRun")
            Me.m_plRun.Controls.Add(Me.m_hdrRunning)
            Me.m_plRun.Controls.Add(Me.m_tlpRun)
            Me.m_plRun.Name = "m_plRun"
            '
            'm_hdrRunning
            '
            resources.ApplyResources(Me.m_hdrRunning, "m_hdrRunning")
            Me.m_hdrRunning.CanCollapseParent = False
            Me.m_hdrRunning.CollapsedParentHeight = 0
            Me.m_hdrRunning.IsCollapsed = False
            Me.m_hdrRunning.Name = "m_hdrRunning"
            '
            'm_tlpRun
            '
            resources.ApplyResources(Me.m_tlpRun, "m_tlpRun")
            Me.m_tlpRun.Controls.Add(Me.m_btnRun, 0, 0)
            Me.m_tlpRun.Controls.Add(Me.m_btnPause, 0, 1)
            Me.m_tlpRun.Controls.Add(Me.m_btnStop, 1, 1)
            Me.m_tlpRun.Controls.Add(Me.m_cmbRunType, 1, 0)
            Me.m_tlpRun.Name = "m_tlpRun"
            '
            'm_btnPause
            '
            resources.ApplyResources(Me.m_btnPause, "m_btnPause")
            Me.m_btnPause.Name = "m_btnPause"
            Me.m_btnPause.UseVisualStyleBackColor = True
            '
            'm_cmbRunType
            '
            resources.ApplyResources(Me.m_cmbRunType, "m_cmbRunType")
            Me.m_cmbRunType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbRunType.FormattingEnabled = True
            Me.m_cmbRunType.Items.AddRange(New Object() {resources.GetString("m_cmbRunType.Items"), resources.GetString("m_cmbRunType.Items1"), resources.GetString("m_cmbRunType.Items2")})
            Me.m_cmbRunType.Name = "m_cmbRunType"
            '
            'm_plGraphData
            '
            Me.m_plGraphData.Controls.Add(Me.m_rbCatchGraph)
            Me.m_plGraphData.Controls.Add(Me.m_rbConsumpGraph)
            Me.m_plGraphData.Controls.Add(Me.m_rbPredMortGraph)
            Me.m_plGraphData.Controls.Add(Me.m_rbFishMortGraph)
            Me.m_plGraphData.Controls.Add(Me.m_rbRelBiomassGraph)
            Me.m_plGraphData.Controls.Add(Me.m_hdrGraphTypes)
            resources.ApplyResources(Me.m_plGraphData, "m_plGraphData")
            Me.m_plGraphData.Name = "m_plGraphData"
            '
            'm_rbCatchGraph
            '
            resources.ApplyResources(Me.m_rbCatchGraph, "m_rbCatchGraph")
            Me.m_rbCatchGraph.Name = "m_rbCatchGraph"
            Me.m_rbCatchGraph.TabStop = True
            Me.m_rbCatchGraph.UseVisualStyleBackColor = True
            '
            'm_rbConsumpGraph
            '
            resources.ApplyResources(Me.m_rbConsumpGraph, "m_rbConsumpGraph")
            Me.m_rbConsumpGraph.Name = "m_rbConsumpGraph"
            Me.m_rbConsumpGraph.TabStop = True
            Me.m_rbConsumpGraph.UseVisualStyleBackColor = True
            '
            'm_rbPredMortGraph
            '
            resources.ApplyResources(Me.m_rbPredMortGraph, "m_rbPredMortGraph")
            Me.m_rbPredMortGraph.Name = "m_rbPredMortGraph"
            Me.m_rbPredMortGraph.TabStop = True
            Me.m_rbPredMortGraph.UseVisualStyleBackColor = True
            '
            'm_rbFishMortGraph
            '
            resources.ApplyResources(Me.m_rbFishMortGraph, "m_rbFishMortGraph")
            Me.m_rbFishMortGraph.Name = "m_rbFishMortGraph"
            Me.m_rbFishMortGraph.TabStop = True
            Me.m_rbFishMortGraph.UseVisualStyleBackColor = True
            '
            'm_rbRelBiomassGraph
            '
            resources.ApplyResources(Me.m_rbRelBiomassGraph, "m_rbRelBiomassGraph")
            Me.m_rbRelBiomassGraph.Checked = True
            Me.m_rbRelBiomassGraph.Name = "m_rbRelBiomassGraph"
            Me.m_rbRelBiomassGraph.TabStop = True
            Me.m_rbRelBiomassGraph.UseVisualStyleBackColor = True
            '
            'm_hdrGraphTypes
            '
            resources.ApplyResources(Me.m_hdrGraphTypes, "m_hdrGraphTypes")
            Me.m_hdrGraphTypes.CanCollapseParent = True
            Me.m_hdrGraphTypes.CollapsedParentHeight = 0
            Me.m_hdrGraphTypes.IsCollapsed = False
            Me.m_hdrGraphTypes.Name = "m_hdrGraphTypes"
            '
            'm_tcOutputs
            '
            resources.ApplyResources(Me.m_tcOutputs, "m_tcOutputs")
            Me.m_tcOutputs.Controls.Add(Me.m_tabMap)
            Me.m_tcOutputs.Controls.Add(Me.m_tabGraph)
            Me.m_tcOutputs.Name = "m_tcOutputs"
            Me.m_tcOutputs.SelectedIndex = 0
            '
            'm_tabMap
            '
            Me.m_tabMap.Controls.Add(Me.m_legend)
            Me.m_tabMap.Controls.Add(Me.m_pbMap)
            resources.ApplyResources(Me.m_tabMap, "m_tabMap")
            Me.m_tabMap.Name = "m_tabMap"
            Me.m_tabMap.UseVisualStyleBackColor = True
            '
            'm_legend
            '
            resources.ApplyResources(Me.m_legend, "m_legend")
            Me.m_legend.BarWidthPercentage = 80
            Me.m_legend.ColorHigh = System.Drawing.Color.DarkGreen
            Me.m_legend.ColorLow = System.Drawing.Color.Red
            Me.m_legend.Colors = CType(resources.GetObject("m_legend.Colors"), System.Collections.Generic.List(Of System.Drawing.Color))
            Me.m_legend.LabelHigh = "High"
            Me.m_legend.LabelLow = "Low"
            Me.m_legend.Name = "m_legend"
            Me.m_legend.UIContext = Nothing
            '
            'm_tabGraph
            '
            Me.m_tabGraph.Controls.Add(Me.m_zgPlotLarge)
            resources.ApplyResources(Me.m_tabGraph, "m_tabGraph")
            Me.m_tabGraph.Name = "m_tabGraph"
            Me.m_tabGraph.UseVisualStyleBackColor = True
            '
            'm_zgPlotLarge
            '
            resources.ApplyResources(Me.m_zgPlotLarge, "m_zgPlotLarge")
            Me.m_zgPlotLarge.Name = "m_zgPlotLarge"
            Me.m_zgPlotLarge.ScrollGrace = 0R
            Me.m_zgPlotLarge.ScrollMaxX = 0R
            Me.m_zgPlotLarge.ScrollMaxY = 0R
            Me.m_zgPlotLarge.ScrollMaxY2 = 0R
            Me.m_zgPlotLarge.ScrollMinX = 0R
            Me.m_zgPlotLarge.ScrollMinY = 0R
            Me.m_zgPlotLarge.ScrollMinY2 = 0R
            '
            'frmRunEcospace
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_scMain)
            Me.Name = "frmRunEcospace"
            Me.TabText = "Run Ecospace"
            CType(Me.m_pbMap, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel2.ResumeLayout(False)
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            Me.m_tlpOptions.ResumeLayout(False)
            Me.m_plMapSaveImages.ResumeLayout(False)
            Me.m_plMapSaveImages.PerformLayout()
            Me.m_plMapLabels.ResumeLayout(False)
            Me.m_plMapLabels.PerformLayout()
            Me.m_plMapData.ResumeLayout(False)
            Me.m_plMapData.PerformLayout()
            Me.m_plDisplayOptions.ResumeLayout(False)
            Me.m_plDisplayOptions.PerformLayout()
            Me.m_plRun.ResumeLayout(False)
            Me.m_tlpRun.ResumeLayout(False)
            Me.m_plGraphData.ResumeLayout(False)
            Me.m_plGraphData.PerformLayout()
            Me.m_tcOutputs.ResumeLayout(False)
            Me.m_tabMap.ResumeLayout(False)
            Me.m_tabGraph.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_btnRun As System.Windows.Forms.Button
        Private WithEvents m_cmbDisplayItem As System.Windows.Forms.ComboBox
        Private WithEvents m_rbShowSingle As System.Windows.Forms.RadioButton
        Private WithEvents m_rbShowNonHidden As System.Windows.Forms.RadioButton
        Private WithEvents m_rbShowAll As System.Windows.Forms.RadioButton
        Private WithEvents m_btnStop As System.Windows.Forms.Button
        Private WithEvents m_pbMap As System.Windows.Forms.PictureBox
        Private WithEvents m_cbOverlay As System.Windows.Forms.CheckBox
        Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
        Private WithEvents m_tcOutputs As System.Windows.Forms.TabControl
        Private WithEvents m_tabMap As System.Windows.Forms.TabPage
        Private WithEvents m_tabGraph As System.Windows.Forms.TabPage
        Private WithEvents m_tlpRun As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_hdrDispOpt As cEwEHeaderLabel
        Private WithEvents m_hdrDist As cEwEHeaderLabel
        Private WithEvents m_plDisplayOptions As System.Windows.Forms.Panel
        Private WithEvents m_zgPlotLarge As ZedGraphControl
        Private WithEvents m_btnDisplayGroups1 As System.Windows.Forms.Button
        Private WithEvents m_plMapData As System.Windows.Forms.Panel
        Private WithEvents m_rbDisplayRelBiomass As System.Windows.Forms.RadioButton
        Private WithEvents m_rbDisplayFishingEffort As System.Windows.Forms.RadioButton
        Private WithEvents m_rbDisplayContaminantC As System.Windows.Forms.RadioButton
        Private WithEvents m_rbDisplayCoverB As System.Windows.Forms.RadioButton
        Private WithEvents m_cbMPA As System.Windows.Forms.CheckBox
        Private WithEvents m_hdrLabelOptions As cEwEHeaderLabel
        Private WithEvents m_cmbLabelPos As System.Windows.Forms.ComboBox
        Private WithEvents m_cbShowLabels As System.Windows.Forms.CheckBox
        Private WithEvents m_plMapLabels As System.Windows.Forms.Panel
        Private WithEvents m_cbInvertColor As System.Windows.Forms.CheckBox
        Private WithEvents m_hdrRunning As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_btnPause As System.Windows.Forms.Button
        Private WithEvents m_cmbRunType As System.Windows.Forms.ComboBox
        Private WithEvents m_cbShowIBMPackets As System.Windows.Forms.CheckBox
        Private WithEvents m_rbDisplayF As System.Windows.Forms.RadioButton
        Private WithEvents m_plRun As System.Windows.Forms.Panel
        Private WithEvents m_rbDisplayFOverB As System.Windows.Forms.RadioButton
        Private WithEvents m_tlpOptions As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents m_txFMax As System.Windows.Forms.TextBox
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Private WithEvents m_legend As ScientificInterfaceShared.Controls.ucLegendBar
        Friend WithEvents m_rbConsumpGraph As System.Windows.Forms.RadioButton
        Friend WithEvents m_rbPredMortGraph As System.Windows.Forms.RadioButton
        Friend WithEvents m_rbRelBiomassGraph As System.Windows.Forms.RadioButton
        Friend WithEvents m_rbCatchGraph As System.Windows.Forms.RadioButton
        Private WithEvents m_hdrGraphTypes As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_cbShowDateInLabel As System.Windows.Forms.CheckBox
        Private WithEvents m_plMapSaveImages As System.Windows.Forms.Panel
        Private WithEvents m_tbxAutosaveTimeSteps As System.Windows.Forms.TextBox
        Private WithEvents m_hdrAutosave As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_plGraphData As System.Windows.Forms.Panel
        Private WithEvents m_cbAutoSavePNG As System.Windows.Forms.CheckBox
        Private WithEvents m_lblAutosaveTimeSteps As System.Windows.Forms.Label
        Private WithEvents m_rbDisplayEnvDriver As RadioButton
        Private WithEvents m_rbDisplayDiscards As RadioButton
        Private WithEvents m_rbDisplayComputedHabitatCapacity As RadioButton
        Private WithEvents m_rbFishMortGraph As RadioButton
    End Class

End Namespace

