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

Imports SharedResources = ScientificInterfaceShared.My.Resources

Partial Public Class frmEwE6
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
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
        Dim m_tssHelp2 As System.Windows.Forms.ToolStripSeparator
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEwE6))
        Dim m_tssFile1 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssFile2 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssFile3 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssView1 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssEcosim1 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssEcosim2 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssEcospace1 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssEcospace2 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssEcospace3 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssHelp1 As System.Windows.Forms.ToolStripSeparator
        Dim MenuEcospace As System.Windows.Forms.ToolStripMenuItem
        Dim sep1 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssHelp3 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssEcotracer1 As System.Windows.Forms.ToolStripSeparator
        Dim m_tssTools1 As System.Windows.Forms.ToolStripSeparator
        Dim sep2 As System.Windows.Forms.ToolStripSeparator
        Me.m_tsmiEcospaceNew = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceLoad = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceClose = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceSave = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceSaveAs = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceImportLayers = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceExportLayers = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceEditMap = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceDefineHabitats = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceDefineMPAs = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceDefineRegions = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceDefineImportanceMaps = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceInputMaps = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tssEcospace4 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tsmiEcospaceDatasets = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcospaceLoadXYRefData = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpContents = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuHelp = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpIndex = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpSearch = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpViewMainSite = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpViewFacebook = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiViewLog = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpViewReports = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpReportIssue = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpRequestSourceCodeAccess = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpAbout = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiHelpFeedback = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuWindows = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiWindowsClose = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiWindowsCloseAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiOptions = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiFileExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuFile = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiFileNew = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiFileOpen = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiFileRecent = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiNone = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiOpenOutput = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ImportModel = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcobaseImport = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExportModel = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcobaseExport = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEIIXMLExport = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiFileClose = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiFileSave = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiFileSaveAs = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiFileCompact = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiPrint = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator4 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_menuMain = New ScientificInterfaceShared.Controls.cEwEMenustrip()
        Me.MenuView = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiViewStartPage = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiViewNavigation = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiViewStatus = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiViewRemarks = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiViewMenu = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiViewModelBar = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiViewStatusBar = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tsmiPresentation = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tsmiViewItems = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuEcopath = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcopathDefineGroups = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcopathDefineMultiStanza = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcopathDefineFleets = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcopathDefinePedigree = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcopathDefineTraits = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuEcosim = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcosimNew = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcosimLoad = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcosimClose = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcosimSave = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcosimSaveAs = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcosimDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiTimeSeriesImport = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiTimeSeriesExport = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiTimeSeriesLoad = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiTimeSeriesEditWeights = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuEcotracer = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcotracerNew = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcotracerLoad = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcotracerSave = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcotracerSaveAs = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiEcotracerDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuTools = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiExternalTools = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsModel = New ScientificInterfaceShared.Controls.cModelPathToolStrip()
        Me.m_tsbnPreview = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbSave = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbEcopath = New System.Windows.Forms.ToolStripSplitButton()
        Me.m_tsbEcosim = New System.Windows.Forms.ToolStripSplitButton()
        Me.m_tsbEcospace = New System.Windows.Forms.ToolStripSplitButton()
        Me.m_tsbEcotracer = New System.Windows.Forms.ToolStripSplitButton()
        Me.m_tsbnAutosaveResults = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnFeedback = New System.Windows.Forms.ToolStripButton()
        Me.m_tslbReadOnly = New System.Windows.Forms.ToolStripLabel()
        Me.m_ssMain = New ScientificInterface.cEwEStatusBar()
        m_tssHelp2 = New System.Windows.Forms.ToolStripSeparator()
        m_tssFile1 = New System.Windows.Forms.ToolStripSeparator()
        m_tssFile2 = New System.Windows.Forms.ToolStripSeparator()
        m_tssFile3 = New System.Windows.Forms.ToolStripSeparator()
        m_tssView1 = New System.Windows.Forms.ToolStripSeparator()
        m_tssEcosim1 = New System.Windows.Forms.ToolStripSeparator()
        m_tssEcosim2 = New System.Windows.Forms.ToolStripSeparator()
        m_tssEcospace1 = New System.Windows.Forms.ToolStripSeparator()
        m_tssEcospace2 = New System.Windows.Forms.ToolStripSeparator()
        m_tssEcospace3 = New System.Windows.Forms.ToolStripSeparator()
        m_tssHelp1 = New System.Windows.Forms.ToolStripSeparator()
        MenuEcospace = New System.Windows.Forms.ToolStripMenuItem()
        sep1 = New System.Windows.Forms.ToolStripSeparator()
        m_tssHelp3 = New System.Windows.Forms.ToolStripSeparator()
        m_tssEcotracer1 = New System.Windows.Forms.ToolStripSeparator()
        m_tssTools1 = New System.Windows.Forms.ToolStripSeparator()
        sep2 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_menuMain.SuspendLayout()
        Me.m_tsModel.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_tssHelp2
        '
        m_tssHelp2.Name = "m_tssHelp2"
        resources.ApplyResources(m_tssHelp2, "m_tssHelp2")
        '
        'm_tssFile1
        '
        m_tssFile1.Name = "m_tssFile1"
        resources.ApplyResources(m_tssFile1, "m_tssFile1")
        '
        'm_tssFile2
        '
        m_tssFile2.Name = "m_tssFile2"
        resources.ApplyResources(m_tssFile2, "m_tssFile2")
        '
        'm_tssFile3
        '
        m_tssFile3.Name = "m_tssFile3"
        resources.ApplyResources(m_tssFile3, "m_tssFile3")
        '
        'm_tssView1
        '
        m_tssView1.Name = "m_tssView1"
        resources.ApplyResources(m_tssView1, "m_tssView1")
        '
        'm_tssEcosim1
        '
        m_tssEcosim1.Name = "m_tssEcosim1"
        resources.ApplyResources(m_tssEcosim1, "m_tssEcosim1")
        '
        'm_tssEcosim2
        '
        m_tssEcosim2.Name = "m_tssEcosim2"
        resources.ApplyResources(m_tssEcosim2, "m_tssEcosim2")
        '
        'm_tssEcospace1
        '
        m_tssEcospace1.Name = "m_tssEcospace1"
        resources.ApplyResources(m_tssEcospace1, "m_tssEcospace1")
        '
        'm_tssEcospace2
        '
        m_tssEcospace2.Name = "m_tssEcospace2"
        resources.ApplyResources(m_tssEcospace2, "m_tssEcospace2")
        '
        'm_tssEcospace3
        '
        m_tssEcospace3.Name = "m_tssEcospace3"
        resources.ApplyResources(m_tssEcospace3, "m_tssEcospace3")
        '
        'm_tssHelp1
        '
        m_tssHelp1.Name = "m_tssHelp1"
        resources.ApplyResources(m_tssHelp1, "m_tssHelp1")
        '
        'MenuEcospace
        '
        MenuEcospace.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiEcospaceNew, Me.m_tsmiEcospaceLoad, Me.m_tsmiEcospaceClose, m_tssEcospace1, Me.m_tsmiEcospaceSave, Me.m_tsmiEcospaceSaveAs, Me.m_tsmiEcospaceDelete, m_tssEcospace2, Me.m_tsmiEcospaceImportLayers, Me.m_tsmiEcospaceExportLayers, m_tssEcospace3, Me.m_tsmiEcospaceEditMap, Me.m_tsmiEcospaceDefineHabitats, Me.m_tsmiEcospaceDefineMPAs, Me.m_tsmiEcospaceDefineRegions, Me.m_tsmiEcospaceDefineImportanceMaps, Me.m_tsmiEcospaceInputMaps, Me.m_tssEcospace4, Me.m_tsmiEcospaceDatasets, Me.m_tsmiEcospaceLoadXYRefData})
        MenuEcospace.Name = "MenuEcospace"
        resources.ApplyResources(MenuEcospace, "MenuEcospace")
        '
        'm_tsmiEcospaceNew
        '
        resources.ApplyResources(Me.m_tsmiEcospaceNew, "m_tsmiEcospaceNew")
        Me.m_tsmiEcospaceNew.Name = "m_tsmiEcospaceNew"
        '
        'm_tsmiEcospaceLoad
        '
        resources.ApplyResources(Me.m_tsmiEcospaceLoad, "m_tsmiEcospaceLoad")
        Me.m_tsmiEcospaceLoad.Name = "m_tsmiEcospaceLoad"
        '
        'm_tsmiEcospaceClose
        '
        Me.m_tsmiEcospaceClose.Name = "m_tsmiEcospaceClose"
        resources.ApplyResources(Me.m_tsmiEcospaceClose, "m_tsmiEcospaceClose")
        '
        'm_tsmiEcospaceSave
        '
        Me.m_tsmiEcospaceSave.Name = "m_tsmiEcospaceSave"
        resources.ApplyResources(Me.m_tsmiEcospaceSave, "m_tsmiEcospaceSave")
        '
        'm_tsmiEcospaceSaveAs
        '
        resources.ApplyResources(Me.m_tsmiEcospaceSaveAs, "m_tsmiEcospaceSaveAs")
        Me.m_tsmiEcospaceSaveAs.Name = "m_tsmiEcospaceSaveAs"
        '
        'm_tsmiEcospaceDelete
        '
        resources.ApplyResources(Me.m_tsmiEcospaceDelete, "m_tsmiEcospaceDelete")
        Me.m_tsmiEcospaceDelete.Name = "m_tsmiEcospaceDelete"
        '
        'm_tsmiEcospaceImportLayers
        '
        resources.ApplyResources(Me.m_tsmiEcospaceImportLayers, "m_tsmiEcospaceImportLayers")
        Me.m_tsmiEcospaceImportLayers.Name = "m_tsmiEcospaceImportLayers"
        '
        'm_tsmiEcospaceExportLayers
        '
        resources.ApplyResources(Me.m_tsmiEcospaceExportLayers, "m_tsmiEcospaceExportLayers")
        Me.m_tsmiEcospaceExportLayers.Name = "m_tsmiEcospaceExportLayers"
        '
        'm_tsmiEcospaceEditMap
        '
        Me.m_tsmiEcospaceEditMap.Name = "m_tsmiEcospaceEditMap"
        resources.ApplyResources(Me.m_tsmiEcospaceEditMap, "m_tsmiEcospaceEditMap")
        '
        'm_tsmiEcospaceDefineHabitats
        '
        Me.m_tsmiEcospaceDefineHabitats.Name = "m_tsmiEcospaceDefineHabitats"
        resources.ApplyResources(Me.m_tsmiEcospaceDefineHabitats, "m_tsmiEcospaceDefineHabitats")
        '
        'm_tsmiEcospaceDefineMPAs
        '
        Me.m_tsmiEcospaceDefineMPAs.Name = "m_tsmiEcospaceDefineMPAs"
        resources.ApplyResources(Me.m_tsmiEcospaceDefineMPAs, "m_tsmiEcospaceDefineMPAs")
        '
        'm_tsmiEcospaceDefineRegions
        '
        Me.m_tsmiEcospaceDefineRegions.Name = "m_tsmiEcospaceDefineRegions"
        resources.ApplyResources(Me.m_tsmiEcospaceDefineRegions, "m_tsmiEcospaceDefineRegions")
        '
        'm_tsmiEcospaceDefineImportanceMaps
        '
        Me.m_tsmiEcospaceDefineImportanceMaps.Name = "m_tsmiEcospaceDefineImportanceMaps"
        resources.ApplyResources(Me.m_tsmiEcospaceDefineImportanceMaps, "m_tsmiEcospaceDefineImportanceMaps")
        '
        'm_tsmiEcospaceInputMaps
        '
        Me.m_tsmiEcospaceInputMaps.Name = "m_tsmiEcospaceInputMaps"
        resources.ApplyResources(Me.m_tsmiEcospaceInputMaps, "m_tsmiEcospaceInputMaps")
        '
        'm_tssEcospace4
        '
        Me.m_tssEcospace4.Name = "m_tssEcospace4"
        resources.ApplyResources(Me.m_tssEcospace4, "m_tssEcospace4")
        '
        'm_tsmiEcospaceDatasets
        '
        resources.ApplyResources(Me.m_tsmiEcospaceDatasets, "m_tsmiEcospaceDatasets")
        Me.m_tsmiEcospaceDatasets.Name = "m_tsmiEcospaceDatasets"
        '
        'm_tsmiEcospaceLoadXYRefData
        '
        Me.m_tsmiEcospaceLoadXYRefData.Name = "m_tsmiEcospaceLoadXYRefData"
        resources.ApplyResources(Me.m_tsmiEcospaceLoadXYRefData, "m_tsmiEcospaceLoadXYRefData")
        '
        'sep1
        '
        sep1.Name = "sep1"
        resources.ApplyResources(sep1, "sep1")
        '
        'm_tssHelp3
        '
        m_tssHelp3.Name = "m_tssHelp3"
        resources.ApplyResources(m_tssHelp3, "m_tssHelp3")
        '
        'm_tssEcotracer1
        '
        m_tssEcotracer1.Name = "m_tssEcotracer1"
        resources.ApplyResources(m_tssEcotracer1, "m_tssEcotracer1")
        '
        'm_tssTools1
        '
        m_tssTools1.Name = "m_tssTools1"
        resources.ApplyResources(m_tssTools1, "m_tssTools1")
        '
        'sep2
        '
        sep2.Name = "sep2"
        resources.ApplyResources(sep2, "sep2")
        '
        'm_tsmiHelpContents
        '
        Me.m_tsmiHelpContents.Name = "m_tsmiHelpContents"
        resources.ApplyResources(Me.m_tsmiHelpContents, "m_tsmiHelpContents")
        '
        'MenuHelp
        '
        Me.MenuHelp.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiHelpContents, Me.m_tsmiHelpIndex, Me.m_tsmiHelpSearch, m_tssHelp1, Me.m_tsmiHelpViewMainSite, Me.m_tsmiHelpViewFacebook, m_tssHelp3, Me.m_tsmiViewLog, Me.m_tsmiHelpViewReports, Me.m_tsmiHelpReportIssue, Me.m_tsmiHelpRequestSourceCodeAccess, m_tssHelp2, Me.m_tsmiHelpAbout, Me.m_tsmiHelpFeedback})
        Me.MenuHelp.Name = "MenuHelp"
        resources.ApplyResources(Me.MenuHelp, "MenuHelp")
        '
        'm_tsmiHelpIndex
        '
        resources.ApplyResources(Me.m_tsmiHelpIndex, "m_tsmiHelpIndex")
        Me.m_tsmiHelpIndex.Name = "m_tsmiHelpIndex"
        '
        'm_tsmiHelpSearch
        '
        resources.ApplyResources(Me.m_tsmiHelpSearch, "m_tsmiHelpSearch")
        Me.m_tsmiHelpSearch.Name = "m_tsmiHelpSearch"
        '
        'm_tsmiHelpViewMainSite
        '
        Me.m_tsmiHelpViewMainSite.Name = "m_tsmiHelpViewMainSite"
        resources.ApplyResources(Me.m_tsmiHelpViewMainSite, "m_tsmiHelpViewMainSite")
        '
        'm_tsmiHelpViewFacebook
        '
        Me.m_tsmiHelpViewFacebook.Name = "m_tsmiHelpViewFacebook"
        resources.ApplyResources(Me.m_tsmiHelpViewFacebook, "m_tsmiHelpViewFacebook")
        '
        'm_tsmiViewLog
        '
        Me.m_tsmiViewLog.Name = "m_tsmiViewLog"
        resources.ApplyResources(Me.m_tsmiViewLog, "m_tsmiViewLog")
        '
        'm_tsmiHelpViewReports
        '
        resources.ApplyResources(Me.m_tsmiHelpViewReports, "m_tsmiHelpViewReports")
        Me.m_tsmiHelpViewReports.Name = "m_tsmiHelpViewReports"
        '
        'm_tsmiHelpReportIssue
        '
        resources.ApplyResources(Me.m_tsmiHelpReportIssue, "m_tsmiHelpReportIssue")
        Me.m_tsmiHelpReportIssue.Name = "m_tsmiHelpReportIssue"
        '
        'm_tsmiHelpRequestSourceCodeAccess
        '
        resources.ApplyResources(Me.m_tsmiHelpRequestSourceCodeAccess, "m_tsmiHelpRequestSourceCodeAccess")
        Me.m_tsmiHelpRequestSourceCodeAccess.Name = "m_tsmiHelpRequestSourceCodeAccess"
        '
        'm_tsmiHelpAbout
        '
        Me.m_tsmiHelpAbout.Name = "m_tsmiHelpAbout"
        resources.ApplyResources(Me.m_tsmiHelpAbout, "m_tsmiHelpAbout")
        '
        'm_tsmiHelpFeedback
        '
        Me.m_tsmiHelpFeedback.Image = Global.ScientificInterface.My.Resources.Resources.logo_sm
        Me.m_tsmiHelpFeedback.Name = "m_tsmiHelpFeedback"
        resources.ApplyResources(Me.m_tsmiHelpFeedback, "m_tsmiHelpFeedback")
        '
        'MenuWindows
        '
        Me.MenuWindows.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiWindowsClose, Me.m_tsmiWindowsCloseAll})
        Me.MenuWindows.Name = "MenuWindows"
        resources.ApplyResources(Me.MenuWindows, "MenuWindows")
        '
        'm_tsmiWindowsClose
        '
        Me.m_tsmiWindowsClose.Name = "m_tsmiWindowsClose"
        resources.ApplyResources(Me.m_tsmiWindowsClose, "m_tsmiWindowsClose")
        '
        'm_tsmiWindowsCloseAll
        '
        Me.m_tsmiWindowsCloseAll.Name = "m_tsmiWindowsCloseAll"
        resources.ApplyResources(Me.m_tsmiWindowsCloseAll, "m_tsmiWindowsCloseAll")
        '
        'm_tsmiOptions
        '
        resources.ApplyResources(Me.m_tsmiOptions, "m_tsmiOptions")
        Me.m_tsmiOptions.Name = "m_tsmiOptions"
        '
        'm_tsmiFileExit
        '
        Me.m_tsmiFileExit.Name = "m_tsmiFileExit"
        resources.ApplyResources(Me.m_tsmiFileExit, "m_tsmiFileExit")
        '
        'MenuFile
        '
        Me.MenuFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiFileNew, Me.m_tsmiFileOpen, Me.m_tsmiFileRecent, Me.m_tsmiOpenOutput, Me.ToolStripSeparator2, Me.ImportModel, Me.ExportModel, m_tssFile1, Me.m_tsmiFileClose, m_tssFile2, Me.m_tsmiFileSave, Me.m_tsmiFileSaveAs, Me.m_tsmiFileCompact, m_tssFile3, Me.m_tsmiPrint, Me.ToolStripSeparator4, Me.m_tsmiFileExit})
        resources.ApplyResources(Me.MenuFile, "MenuFile")
        Me.MenuFile.Name = "MenuFile"
        '
        'm_tsmiFileNew
        '
        resources.ApplyResources(Me.m_tsmiFileNew, "m_tsmiFileNew")
        Me.m_tsmiFileNew.Name = "m_tsmiFileNew"
        '
        'm_tsmiFileOpen
        '
        resources.ApplyResources(Me.m_tsmiFileOpen, "m_tsmiFileOpen")
        Me.m_tsmiFileOpen.Name = "m_tsmiFileOpen"
        '
        'm_tsmiFileRecent
        '
        Me.m_tsmiFileRecent.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiNone})
        Me.m_tsmiFileRecent.Name = "m_tsmiFileRecent"
        resources.ApplyResources(Me.m_tsmiFileRecent, "m_tsmiFileRecent")
        '
        'm_tsmiNone
        '
        resources.ApplyResources(Me.m_tsmiNone, "m_tsmiNone")
        Me.m_tsmiNone.Name = "m_tsmiNone"
        '
        'm_tsmiOpenOutput
        '
        resources.ApplyResources(Me.m_tsmiOpenOutput, "m_tsmiOpenOutput")
        Me.m_tsmiOpenOutput.Name = "m_tsmiOpenOutput"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        '
        'ImportModel
        '
        Me.ImportModel.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiEcobaseImport})
        Me.ImportModel.Name = "ImportModel"
        resources.ApplyResources(Me.ImportModel, "ImportModel")
        '
        'm_tsmiEcobaseImport
        '
        Me.m_tsmiEcobaseImport.Name = "m_tsmiEcobaseImport"
        resources.ApplyResources(Me.m_tsmiEcobaseImport, "m_tsmiEcobaseImport")
        '
        'ExportModel
        '
        Me.ExportModel.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiEcobaseExport, Me.m_tsmiEIIXMLExport})
        Me.ExportModel.Name = "ExportModel"
        resources.ApplyResources(Me.ExportModel, "ExportModel")
        '
        'm_tsmiEcobaseExport
        '
        Me.m_tsmiEcobaseExport.Name = "m_tsmiEcobaseExport"
        resources.ApplyResources(Me.m_tsmiEcobaseExport, "m_tsmiEcobaseExport")
        '
        'm_tsmiEIIXMLExport
        '
        Me.m_tsmiEIIXMLExport.Name = "m_tsmiEIIXMLExport"
        resources.ApplyResources(Me.m_tsmiEIIXMLExport, "m_tsmiEIIXMLExport")
        '
        'm_tsmiFileClose
        '
        Me.m_tsmiFileClose.Name = "m_tsmiFileClose"
        resources.ApplyResources(Me.m_tsmiFileClose, "m_tsmiFileClose")
        '
        'm_tsmiFileSave
        '
        resources.ApplyResources(Me.m_tsmiFileSave, "m_tsmiFileSave")
        Me.m_tsmiFileSave.Name = "m_tsmiFileSave"
        '
        'm_tsmiFileSaveAs
        '
        Me.m_tsmiFileSaveAs.Name = "m_tsmiFileSaveAs"
        resources.ApplyResources(Me.m_tsmiFileSaveAs, "m_tsmiFileSaveAs")
        '
        'm_tsmiFileCompact
        '
        Me.m_tsmiFileCompact.Name = "m_tsmiFileCompact"
        resources.ApplyResources(Me.m_tsmiFileCompact, "m_tsmiFileCompact")
        '
        'm_tsmiPrint
        '
        Me.m_tsmiPrint.Name = "m_tsmiPrint"
        resources.ApplyResources(Me.m_tsmiPrint, "m_tsmiPrint")
        '
        'ToolStripSeparator4
        '
        Me.ToolStripSeparator4.Name = "ToolStripSeparator4"
        resources.ApplyResources(Me.ToolStripSeparator4, "ToolStripSeparator4")
        '
        'm_menuMain
        '
        Me.m_menuMain.GripMargin = New System.Windows.Forms.Padding(0)
        Me.m_menuMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuFile, Me.MenuView, Me.MenuEcopath, Me.MenuEcosim, MenuEcospace, Me.MenuEcotracer, Me.MenuTools, Me.MenuWindows, Me.MenuHelp})
        resources.ApplyResources(Me.m_menuMain, "m_menuMain")
        Me.m_menuMain.MdiWindowListItem = Me.MenuWindows
        Me.m_menuMain.Name = "m_menuMain"
        Me.m_menuMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'MenuView
        '
        Me.MenuView.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiViewStartPage, Me.m_tsmiViewNavigation, Me.m_tsmiViewStatus, Me.m_tsmiViewRemarks, m_tssView1, Me.m_tsmiViewMenu, Me.m_tsmiViewModelBar, Me.m_tsmiViewStatusBar, Me.ToolStripSeparator3, Me.m_tsmiPresentation, Me.ToolStripSeparator1, Me.m_tsmiViewItems})
        Me.MenuView.Name = "MenuView"
        resources.ApplyResources(Me.MenuView, "MenuView")
        '
        'm_tsmiViewStartPage
        '
        Me.m_tsmiViewStartPage.Name = "m_tsmiViewStartPage"
        resources.ApplyResources(Me.m_tsmiViewStartPage, "m_tsmiViewStartPage")
        '
        'm_tsmiViewNavigation
        '
        Me.m_tsmiViewNavigation.Name = "m_tsmiViewNavigation"
        resources.ApplyResources(Me.m_tsmiViewNavigation, "m_tsmiViewNavigation")
        '
        'm_tsmiViewStatus
        '
        Me.m_tsmiViewStatus.Name = "m_tsmiViewStatus"
        resources.ApplyResources(Me.m_tsmiViewStatus, "m_tsmiViewStatus")
        '
        'm_tsmiViewRemarks
        '
        Me.m_tsmiViewRemarks.Name = "m_tsmiViewRemarks"
        resources.ApplyResources(Me.m_tsmiViewRemarks, "m_tsmiViewRemarks")
        '
        'm_tsmiViewMenu
        '
        Me.m_tsmiViewMenu.CheckOnClick = True
        Me.m_tsmiViewMenu.Name = "m_tsmiViewMenu"
        resources.ApplyResources(Me.m_tsmiViewMenu, "m_tsmiViewMenu")
        '
        'm_tsmiViewModelBar
        '
        Me.m_tsmiViewModelBar.Name = "m_tsmiViewModelBar"
        resources.ApplyResources(Me.m_tsmiViewModelBar, "m_tsmiViewModelBar")
        '
        'm_tsmiViewStatusBar
        '
        Me.m_tsmiViewStatusBar.CheckOnClick = True
        Me.m_tsmiViewStatusBar.Name = "m_tsmiViewStatusBar"
        resources.ApplyResources(Me.m_tsmiViewStatusBar, "m_tsmiViewStatusBar")
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        resources.ApplyResources(Me.ToolStripSeparator3, "ToolStripSeparator3")
        '
        'm_tsmiPresentation
        '
        resources.ApplyResources(Me.m_tsmiPresentation, "m_tsmiPresentation")
        Me.m_tsmiPresentation.Name = "m_tsmiPresentation"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        '
        'm_tsmiViewItems
        '
        Me.m_tsmiViewItems.Name = "m_tsmiViewItems"
        resources.ApplyResources(Me.m_tsmiViewItems, "m_tsmiViewItems")
        '
        'MenuEcopath
        '
        Me.MenuEcopath.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiEcopathDefineGroups, Me.m_tsmiEcopathDefineMultiStanza, Me.m_tsmiEcopathDefineFleets, Me.m_tsmiEcopathDefinePedigree, Me.m_tsmiEcopathDefineTraits})
        Me.MenuEcopath.Name = "MenuEcopath"
        resources.ApplyResources(Me.MenuEcopath, "MenuEcopath")
        '
        'm_tsmiEcopathDefineGroups
        '
        Me.m_tsmiEcopathDefineGroups.Name = "m_tsmiEcopathDefineGroups"
        resources.ApplyResources(Me.m_tsmiEcopathDefineGroups, "m_tsmiEcopathDefineGroups")
        '
        'm_tsmiEcopathDefineMultiStanza
        '
        Me.m_tsmiEcopathDefineMultiStanza.Name = "m_tsmiEcopathDefineMultiStanza"
        resources.ApplyResources(Me.m_tsmiEcopathDefineMultiStanza, "m_tsmiEcopathDefineMultiStanza")
        '
        'm_tsmiEcopathDefineFleets
        '
        Me.m_tsmiEcopathDefineFleets.Name = "m_tsmiEcopathDefineFleets"
        resources.ApplyResources(Me.m_tsmiEcopathDefineFleets, "m_tsmiEcopathDefineFleets")
        '
        'm_tsmiEcopathDefinePedigree
        '
        Me.m_tsmiEcopathDefinePedigree.Name = "m_tsmiEcopathDefinePedigree"
        resources.ApplyResources(Me.m_tsmiEcopathDefinePedigree, "m_tsmiEcopathDefinePedigree")
        '
        'm_tsmiEcopathDefineTraits
        '
        resources.ApplyResources(Me.m_tsmiEcopathDefineTraits, "m_tsmiEcopathDefineTraits")
        Me.m_tsmiEcopathDefineTraits.Name = "m_tsmiEcopathDefineTraits"
        '
        'MenuEcosim
        '
        Me.MenuEcosim.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiEcosimNew, Me.m_tsmiEcosimLoad, Me.m_tsmiEcosimClose, m_tssEcosim1, Me.m_tsmiEcosimSave, Me.m_tsmiEcosimSaveAs, Me.m_tsmiEcosimDelete, m_tssEcosim2, Me.m_tsmiTimeSeriesImport, Me.m_tsmiTimeSeriesExport, Me.m_tsmiTimeSeriesLoad, Me.m_tsmiTimeSeriesEditWeights})
        Me.MenuEcosim.Name = "MenuEcosim"
        resources.ApplyResources(Me.MenuEcosim, "MenuEcosim")
        '
        'm_tsmiEcosimNew
        '
        resources.ApplyResources(Me.m_tsmiEcosimNew, "m_tsmiEcosimNew")
        Me.m_tsmiEcosimNew.Name = "m_tsmiEcosimNew"
        '
        'm_tsmiEcosimLoad
        '
        resources.ApplyResources(Me.m_tsmiEcosimLoad, "m_tsmiEcosimLoad")
        Me.m_tsmiEcosimLoad.Name = "m_tsmiEcosimLoad"
        '
        'm_tsmiEcosimClose
        '
        Me.m_tsmiEcosimClose.Name = "m_tsmiEcosimClose"
        resources.ApplyResources(Me.m_tsmiEcosimClose, "m_tsmiEcosimClose")
        '
        'm_tsmiEcosimSave
        '
        Me.m_tsmiEcosimSave.Name = "m_tsmiEcosimSave"
        resources.ApplyResources(Me.m_tsmiEcosimSave, "m_tsmiEcosimSave")
        '
        'm_tsmiEcosimSaveAs
        '
        resources.ApplyResources(Me.m_tsmiEcosimSaveAs, "m_tsmiEcosimSaveAs")
        Me.m_tsmiEcosimSaveAs.Name = "m_tsmiEcosimSaveAs"
        '
        'm_tsmiEcosimDelete
        '
        resources.ApplyResources(Me.m_tsmiEcosimDelete, "m_tsmiEcosimDelete")
        Me.m_tsmiEcosimDelete.Name = "m_tsmiEcosimDelete"
        '
        'm_tsmiTimeSeriesImport
        '
        resources.ApplyResources(Me.m_tsmiTimeSeriesImport, "m_tsmiTimeSeriesImport")
        Me.m_tsmiTimeSeriesImport.Name = "m_tsmiTimeSeriesImport"
        '
        'm_tsmiTimeSeriesExport
        '
        resources.ApplyResources(Me.m_tsmiTimeSeriesExport, "m_tsmiTimeSeriesExport")
        Me.m_tsmiTimeSeriesExport.Name = "m_tsmiTimeSeriesExport"
        '
        'm_tsmiTimeSeriesLoad
        '
        Me.m_tsmiTimeSeriesLoad.Name = "m_tsmiTimeSeriesLoad"
        resources.ApplyResources(Me.m_tsmiTimeSeriesLoad, "m_tsmiTimeSeriesLoad")
        '
        'm_tsmiTimeSeriesEditWeights
        '
        Me.m_tsmiTimeSeriesEditWeights.Name = "m_tsmiTimeSeriesEditWeights"
        resources.ApplyResources(Me.m_tsmiTimeSeriesEditWeights, "m_tsmiTimeSeriesEditWeights")
        '
        'MenuEcotracer
        '
        Me.MenuEcotracer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.MenuEcotracer.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiEcotracerNew, Me.m_tsmiEcotracerLoad, m_tssEcotracer1, Me.m_tsmiEcotracerSave, Me.m_tsmiEcotracerSaveAs, Me.m_tsmiEcotracerDelete})
        resources.ApplyResources(Me.MenuEcotracer, "MenuEcotracer")
        Me.MenuEcotracer.Name = "MenuEcotracer"
        '
        'm_tsmiEcotracerNew
        '
        resources.ApplyResources(Me.m_tsmiEcotracerNew, "m_tsmiEcotracerNew")
        Me.m_tsmiEcotracerNew.Name = "m_tsmiEcotracerNew"
        '
        'm_tsmiEcotracerLoad
        '
        resources.ApplyResources(Me.m_tsmiEcotracerLoad, "m_tsmiEcotracerLoad")
        Me.m_tsmiEcotracerLoad.Name = "m_tsmiEcotracerLoad"
        '
        'm_tsmiEcotracerSave
        '
        Me.m_tsmiEcotracerSave.Name = "m_tsmiEcotracerSave"
        resources.ApplyResources(Me.m_tsmiEcotracerSave, "m_tsmiEcotracerSave")
        '
        'm_tsmiEcotracerSaveAs
        '
        resources.ApplyResources(Me.m_tsmiEcotracerSaveAs, "m_tsmiEcotracerSaveAs")
        Me.m_tsmiEcotracerSaveAs.Name = "m_tsmiEcotracerSaveAs"
        '
        'm_tsmiEcotracerDelete
        '
        resources.ApplyResources(Me.m_tsmiEcotracerDelete, "m_tsmiEcotracerDelete")
        Me.m_tsmiEcotracerDelete.Name = "m_tsmiEcotracerDelete"
        '
        'MenuTools
        '
        Me.MenuTools.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiOptions, Me.m_tsmiExternalTools, m_tssTools1})
        Me.MenuTools.Name = "MenuTools"
        resources.ApplyResources(Me.MenuTools, "MenuTools")
        '
        'm_tsmiExternalTools
        '
        Me.m_tsmiExternalTools.Name = "m_tsmiExternalTools"
        resources.ApplyResources(Me.m_tsmiExternalTools, "m_tsmiExternalTools")
        '
        'm_tsModel
        '
        resources.ApplyResources(Me.m_tsModel, "m_tsModel")
        Me.m_tsModel.CanOverflow = False
        Me.m_tsModel.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsModel.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnPreview, sep1, Me.m_tsbSave, Me.m_tsbnAutosaveResults, sep2, Me.m_tsbEcopath, Me.m_tsbEcosim, Me.m_tsbEcospace, Me.m_tsbEcotracer, Me.m_tsbnFeedback, Me.m_tslbReadOnly})
        Me.m_tsModel.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow
        Me.m_tsModel.Name = "m_tsModel"
        Me.m_tsModel.Path = ""
        Me.m_tsModel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.m_tsModel.Stretch = True
        '
        'm_tsbnPreview
        '
        Me.m_tsbnPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.m_tsbnPreview.DoubleClickEnabled = True
        resources.ApplyResources(Me.m_tsbnPreview, "m_tsbnPreview")
        Me.m_tsbnPreview.ForeColor = System.Drawing.SystemColors.Highlight
        Me.m_tsbnPreview.Name = "m_tsbnPreview"
        '
        'm_tsbSave
        '
        Me.m_tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbSave, "m_tsbSave")
        Me.m_tsbSave.Name = "m_tsbSave"
        '
        'm_tsbEcopath
        '
        Me.m_tsbEcopath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbEcopath, "m_tsbEcopath")
        Me.m_tsbEcopath.Name = "m_tsbEcopath"
        '
        'm_tsbEcosim
        '
        Me.m_tsbEcosim.BackColor = System.Drawing.SystemColors.Control
        Me.m_tsbEcosim.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.m_tsbEcosim.DropDownButtonWidth = 16
        resources.ApplyResources(Me.m_tsbEcosim, "m_tsbEcosim")
        Me.m_tsbEcosim.Name = "m_tsbEcosim"
        '
        'm_tsbEcospace
        '
        Me.m_tsbEcospace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.m_tsbEcospace.DropDownButtonWidth = 16
        resources.ApplyResources(Me.m_tsbEcospace, "m_tsbEcospace")
        Me.m_tsbEcospace.Name = "m_tsbEcospace"
        '
        'm_tsbEcotracer
        '
        Me.m_tsbEcotracer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbEcotracer, "m_tsbEcotracer")
        Me.m_tsbEcotracer.Name = "m_tsbEcotracer"
        '
        'm_tsbnAutosaveResults
        '
        Me.m_tsbnAutosaveResults.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.m_tsbnAutosaveResults.DoubleClickEnabled = True
        resources.ApplyResources(Me.m_tsbnAutosaveResults, "m_tsbnAutosaveResults")
        Me.m_tsbnAutosaveResults.Name = "m_tsbnAutosaveResults"
        '
        'm_tsbnFeedback
        '
        Me.m_tsbnFeedback.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.m_tsbnFeedback.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnFeedback, "m_tsbnFeedback")
        Me.m_tsbnFeedback.ForeColor = System.Drawing.SystemColors.Highlight
        Me.m_tsbnFeedback.Name = "m_tsbnFeedback"
        '
        'm_tslbReadOnly
        '
        Me.m_tslbReadOnly.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.m_tslbReadOnly.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tslbReadOnly, "m_tslbReadOnly")
        Me.m_tslbReadOnly.Name = "m_tslbReadOnly"
        '
        'm_ssMain
        '
        resources.ApplyResources(Me.m_ssMain, "m_ssMain")
        Me.m_ssMain.Name = "m_ssMain"
        Me.m_ssMain.ShowItemToolTips = True
        '
        'frmEwE6
        '
        Me.AllowDrop = True
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_ssMain)
        Me.Controls.Add(Me.m_tsModel)
        Me.Controls.Add(Me.m_menuMain)
        Me.IsMdiContainer = True
        Me.MainMenuStrip = Me.m_menuMain
        Me.Name = "frmEwE6"
        Me.m_menuMain.ResumeLayout(False)
        Me.m_menuMain.PerformLayout()
        Me.m_tsModel.ResumeLayout(False)
        Me.m_tsModel.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_tsmiHelpContents As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MenuHelp As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiHelpIndex As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiHelpSearch As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiHelpAbout As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MenuWindows As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiOptions As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_menuMain As ScientificInterfaceShared.Controls.cEwEMenustrip
    Private WithEvents ToolBarToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MenuTools As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiWindowsCloseAll As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcosimLoad As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcosimSave As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiTimeSeriesImport As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiNone As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsModel As cModelPathToolStrip
    Private WithEvents m_tsmiEcospaceLoad As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceSave As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiWindowsClose As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcosimSaveAs As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceSaveAs As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcosimNew As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceNew As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceDefineHabitats As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceDefineMPAs As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceEditMap As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiTimeSeriesLoad As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsbEcosim As System.Windows.Forms.ToolStripSplitButton
    Private WithEvents m_tsbEcospace As System.Windows.Forms.ToolStripSplitButton
    Private WithEvents m_tsmiTimeSeriesEditWeights As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceDefineImportanceMaps As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiHelpReportIssue As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MenuEcopath As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcopathDefineGroups As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcopathDefineMultiStanza As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcopathDefineFleets As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MenuView As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiViewStatusBar As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiViewRemarks As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiViewStartPage As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiViewNavigation As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiViewStatus As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiViewModelBar As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MenuFile As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiFileOpen As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiFileClose As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiFileNew As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiFileSave As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiFileSaveAs As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiFileRecent As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiFileExit As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiFileCompact As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MenuEcosim As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_ssMain As ScientificInterface.cEwEStatusBar
    Private WithEvents m_tsmiTimeSeriesExport As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsmiViewItems As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcosimDelete As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceDelete As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsbSave As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbEcotracer As System.Windows.Forms.ToolStripSplitButton
    Private WithEvents m_tsmiEcopathDefineTraits As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsbEcopath As System.Windows.Forms.ToolStripSplitButton
    Private WithEvents m_tsmiEcopathDefinePedigree As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiPresentation As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsmiViewMenu As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiPrint As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator4 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsmiEcospaceInputMaps As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceImportLayers As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceExportLayers As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tssEcospace4 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsmiOpenOutput As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsbnAutosaveResults As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnPreview As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsmiViewLog As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceDefineRegions As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiHelpViewReports As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiHelpViewFacebook As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiHelpViewMainSite As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsbnFeedback As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsmiHelpFeedback As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceDatasets As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceClose As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcosimClose As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents m_tslbReadOnly As System.Windows.Forms.ToolStripLabel
    Private WithEvents ImportModel As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcobaseImport As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents ExportModel As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcobaseExport As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiExternalTools As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiEcospaceLoadXYRefData As ToolStripMenuItem
    Friend WithEvents m_tsmiHelpRequestSourceCodeAccess As ToolStripMenuItem
    Private WithEvents MenuEcotracer As ToolStripMenuItem
    Private WithEvents m_tsmiEcotracerNew As ToolStripMenuItem
    Private WithEvents m_tsmiEcotracerLoad As ToolStripMenuItem
    Private WithEvents m_tsmiEcotracerSave As ToolStripMenuItem
    Private WithEvents m_tsmiEcotracerSaveAs As ToolStripMenuItem
    Private WithEvents m_tsmiEcotracerDelete As ToolStripMenuItem
    Friend WithEvents m_tsmiEIIXMLExport As ToolStripMenuItem
End Class

