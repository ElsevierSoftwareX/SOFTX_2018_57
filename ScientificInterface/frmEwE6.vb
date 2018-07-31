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

Option Explicit On
Option Strict On

Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports EwECore
Imports EwECore.Database
Imports EwECore.DataSources
Imports EwECore.SpatialData
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterface.Ecopath
Imports ScientificInterface.Ecosim
Imports ScientificInterface.Ecospace
Imports ScientificInterface.Ecospace.Basemap
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports ScientificInterface.Ecospace.Controls
Imports ScientificInterface.Ecotracer
Imports ScientificInterface.Other
Imports ScientificInterface.Wizard
Imports ScientificInterfaceShared
Imports ScientificInterfaceShared.Integration
Imports WeifenLuo.WinFormsUI.Docking
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' The main form of the EwE6 Scientific Interface
''' </summary>
''' ---------------------------------------------------------------------------
Public Class frmEwE6
    Implements IUIElement

#Region " Variables "

    ' - Message handlers 
    Private m_mhProgress As cMessageHandler = Nothing
    Private m_mhEcosim As cMessageHandler = Nothing
    Private m_mhEcospace As cMessageHandler = Nothing
    Private m_mhEcotracer As cMessageHandler = Nothing
    Private m_mhTimeseries As cMessageHandler = Nothing

    ' - Big nasty UI objects
    Private m_coreController As cCoreController = Nothing
    Private m_pluginManager As cPluginManager = Nothing
    Private m_pluginMenuHandler As cPluginMenuHandler = Nothing
    Private m_formstatemanager As cEwEFormStateManager = Nothing
    Private m_styleguideupdater As cStyleGuideUpdater = Nothing
    Private m_autosavemanager As cAutosaveSettingsManager = Nothing

    ''' <summary>Foundation for undo stack?</summary>
    Private m_messageHistory As cMessageHistory = Nothing

    ''' <summary>Status messages stack.</summary>
    Private m_lstrStatus As New List(Of String)

#Region " Panels "

    Private Const cPANEL_REMARKS As String = "remarks"
    Private Const cPANEL_STATUS As String = "Status"
    Private Const cPANEL_NAV As String = "navigation"
    Private Const cPANEL_START As String = "start"

    Private m_DockPanel As DockPanel = Nothing
    Private m_dtPanels As New Dictionary(Of String, frmEwEDockContent)

#End Region ' Panels

#Region " Presentation mode "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class to toggle the EwE main form between a normal state
    ''' and a 'presentation mode' state.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Class cPresentationMode

#Region " Private vars "

        Private m_frm As frmEwE6 = Nothing
        Private m_bActive As Boolean = False

        ' -- cached main form states  --
        Private m_bShowMenu As Boolean
        Private m_bShowModelBar As Boolean
        Private m_bShowStatusBar As Boolean
        Private m_bShowNavPanel As Boolean
        Private m_bFormState As FormWindowState
        Private m_bBorderStyle As FormBorderStyle
        Private m_bControlBox As Boolean
        Private m_bBounds As Rectangle
        Private m_bUseOpacity As Boolean = False

#End Region ' Private vars

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="frm">The <see cref="frmEwE6"/> to toggle presentation mode for.</param>
        ''' <param name="bUseOpacity">If set to true, the main form will be totally 
        ''' opaque during a presentation mode switch.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(frm As frmEwE6, Optional bUseOpacity As Boolean = False)
            Me.m_frm = frm
            Me.m_bUseOpacity = bUseOpacity
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Toggle between presentation mode and regular mode.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Toggle()
            Me.Active = Not Me.Active
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether presentation mode is active.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Active As Boolean
            Get
                Return Me.m_bActive
            End Get
            Set(value As Boolean)
                If (value = Me.m_bActive) Then Return
                Me.m_bActive = value

                If (Me.m_bUseOpacity) Then Me.m_frm.Opacity = 0

                ' Presentation mode active?
                If (Me.m_bActive) Then
                    ' #Yes: hide bits and stretch form
                    Me.m_bShowMenu = Me.m_frm.m_menuMain.Visible : Me.m_frm.m_menuMain.Visible = Not My.Settings.PresentationModeHideMainMenu
                    Me.m_bShowModelBar = Me.m_frm.m_tsModel.Visible : Me.m_frm.m_tsModel.Visible = Not My.Settings.PresentationModeHideModelBar
                    Me.m_bShowStatusBar = Me.m_frm.m_ssMain.Visible : Me.m_frm.m_ssMain.Visible = Not My.Settings.PresentationModeHideStatusBar
                    Me.m_bShowNavPanel = Me.m_frm.Panel(cPANEL_NAV).IsHiding : Me.m_frm.Panel(cPANEL_NAV).AutoHide = My.Settings.PresentationModeCollapseNavPanel

                    ' JS 28Mar14: This now works
                    ' - Using screen bounds works better than maximizing frmEwE6
                    ' - TopMost is not needed anymore
                    ' - Do not change the order of the next three statements!
                    Me.m_bFormState = Me.m_frm.WindowState : Me.m_frm.WindowState = FormWindowState.Normal
                    Me.m_bBorderStyle = Me.m_frm.FormBorderStyle : Me.m_frm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
                    Me.m_bBounds = Me.m_frm.Bounds : Me.m_frm.Bounds = Screen.GetBounds(Me.m_frm)
                    Me.m_bControlBox = Me.m_frm.ControlBox : Me.m_frm.ControlBox = False
                    '.TopMost = Me.TopMost : Me.TopMost = True
                Else
                    Me.m_frm.FormBorderStyle = Me.m_bBorderStyle
                    Me.m_frm.WindowState = Me.m_bFormState
                    'Me.TopMost = .TopMost
                    Me.m_frm.Bounds = Me.m_bBounds
                    Me.m_frm.m_menuMain.Visible = Me.m_bShowMenu
                    Me.m_frm.m_tsModel.Visible = Me.m_bShowModelBar
                    Me.m_frm.m_ssMain.Visible = Me.m_bShowStatusBar
                    Me.m_frm.Panel(cPANEL_NAV).AutoHide = Me.m_bShowNavPanel
                    Me.m_frm.ControlBox = Me.m_bControlBox
                End If

                If (Me.m_bUseOpacity) Then Me.m_frm.Opacity = 1

            End Set
        End Property
    End Class

    Private m_presentationmode As cPresentationMode = Nothing

#End Region ' Presentation mode

#Region " Commands "

    Private WithEvents m_cmdFileOpen As cFileOpenCommand = Nothing
    Private WithEvents m_cmdFileSave As cFileSaveCommand = Nothing
    Private WithEvents m_cmdDirectoryOpen As cDirectoryOpenCommand = Nothing
    Private WithEvents m_cmdExecute As cExecuteCommand = Nothing
    Private WithEvents m_cmdNewModel As cCommand = Nothing
    Private WithEvents m_cmdLoadModel As cCommand = Nothing
    Private WithEvents m_cmdOpenOutput As cCommand = Nothing
    Private WithEvents m_cmdSave As cCommand = Nothing
    Private WithEvents m_cmdSaveModelAs As cCommand = Nothing
    Private WithEvents m_cmdCloseModel As cCommand = Nothing
    Private WithEvents m_cmdCompactModel As cCommand = Nothing
    Private WithEvents m_cmdCloseDocument As cCommand = Nothing
    Private WithEvents m_cmdNewEcosimScenario As cCommand = Nothing
    Private WithEvents m_cmdLoadEcosimScenario As cCommand = Nothing
    'Private WithEvents m_cmdSaveEcosimScenario As cCommand = Nothing
    Private WithEvents m_cmdCloseEcosimScenario As cCommand = Nothing
    Private WithEvents m_cmdSaveEcosimScenarioAs As cCommand = Nothing
    Private WithEvents m_cmdDeleteEcosimScenario As cCommand = Nothing
    Private WithEvents m_cmdNewEcospaceScenario As cCommand = Nothing
    Private WithEvents m_cmdLoadEcospaceScenario As cCommand = Nothing
    'Private WithEvents m_cmdSaveEcospaceScenario As cCommand = Nothing
    Private WithEvents m_cmdCloseEcospaceScenario As cCommand = Nothing
    Private WithEvents m_cmdSaveEcospaceScenarioAS As cCommand = Nothing
    Private WithEvents m_cmdDeleteEcospaceScenario As cCommand = Nothing
    Private WithEvents m_cmdNewEcotracerScenario As cCommand = Nothing
    Private WithEvents m_cmdLoadEcotracerScenario As cCommand = Nothing
    'Private WithEvents m_cmdSaveEcotracerScenario As cCommand = Nothing
    Private WithEvents m_cmdCloseEcotracerScenario As cCommand = Nothing
    Private WithEvents m_cmdSaveEcotracerScenarioAS As cCommand = Nothing
    Private WithEvents m_cmdDeleteEcotracerScenario As cCommand = Nothing
    Private WithEvents m_cmdCloseAllForms As cCommand = Nothing
    Private WithEvents m_cmdNavigate As cNavigationCommand = Nothing
    Private WithEvents m_cmdViewNavPane As cCommand = Nothing
    Private WithEvents m_cmdViewStatusPane As cCommand = Nothing
    Private WithEvents m_cmdBrowseURI As cBrowserCommand = Nothing
    Private WithEvents m_cmdViewRemarkPane As cCommand = Nothing
    Private WithEvents m_cmdViewMenu As cCommand = Nothing
    Private WithEvents m_cmdViewModelBar As cCommand = Nothing
    Private WithEvents m_cmdViewStatusbar As cCommand = Nothing
    Private WithEvents m_cmdViewPresentationMode As cCommand = Nothing
    Private WithEvents m_cmdAutosaveResults As cCommand = Nothing
    Private WithEvents m_cmdEditGroups As cCommand = Nothing
    Private WithEvents m_cmdEditMultiStanza As cCommand = Nothing
    Private WithEvents m_cmdEditFleets As cCommand = Nothing
    Private WithEvents m_cmdEditTaxonomy As cCommand = Nothing
    Private WithEvents m_cmdEditPedigree As cEditPedigreeCommand = Nothing
    Private WithEvents m_cmdImportTimeSeries As cCommand = Nothing
    Private WithEvents m_cmdEcosimLoadTimeSeries As cCommand = Nothing
    Private WithEvents m_cmdEcospaceLoadTimeSeries As cCommand = Nothing
    Private WithEvents m_cmdWeightTimeSeries As cCommand = Nothing
    Private WithEvents m_cmdExportTimeSeries As cCommand = Nothing
    Private WithEvents m_cmdEditBasemap As cCommand = Nothing
    Private WithEvents m_cmdEditHabitats As cCommand = Nothing
    Private WithEvents m_cmdEditRegions As cCommand = Nothing
    Private WithEvents m_cmdEditMPAs As cCommand = Nothing
    Private WithEvents m_cmdDefineImportanceMaps As cCommand = Nothing
    Private WithEvents m_cmdDefineInputLayers As cCommand = Nothing
    Private WithEvents m_cmdImportLayerData As cImportLayerCommand = Nothing
    Private WithEvents m_cmdExportLayerData As cExportLayerCommand = Nothing
    Private WithEvents m_cmdEditLayer As cEditLayerCommand = Nothing
    Private WithEvents m_cmdShowOptions As cShowOptionsCommand = Nothing
    Private WithEvents m_cmdShowTools As cCommand = Nothing
    Private WithEvents m_cmdEditReferenceMap As cCommand = Nothing
    Private WithEvents m_cmdPluginGUICommand As cPluginGUICommand = Nothing
    Private WithEvents m_cmdHelpAbout As cCommand = Nothing
    Private WithEvents m_cmdHelpReportIssue As cCommand = Nothing
    Private WithEvents m_cmdHelpRequestCodeAccess As cCommand
    Private WithEvents m_cmdHelpFeedback As cCommand = Nothing
    Private WithEvents m_cmdPropertySelection As cPropertySelectionCommand = Nothing
    Private WithEvents m_cmdShowHideItems As cDisplayGroupsCommand = Nothing
    Private WithEvents m_cmdEnableEcotracer As cCommand = Nothing
    Private WithEvents m_cmdEstimateVs As cCommand = Nothing
    Private WithEvents m_cmdExportEcosimResultsToCSV As cEcosimSaveDataCommand = Nothing
    Private WithEvents m_cmdPrint As cCommand = Nothing
    Private WithEvents m_cmdEcosimTrimShapes As cCommand = Nothing
    Private WithEvents m_cmdEcosimChangeShape As cCommand = Nothing
    Private WithEvents m_cmdPickColor As cPickColorCommand = Nothing


    Private WithEvents m_cmdEcospaceLoadXYRefData As cCommand = Nothing

    ' --- Ecospace external data ---

    ''' <summary>Command to define external spatial temporal data connections.</summary>
    Private WithEvents m_cmdDefineSpatialDatasets As cCommand = Nothing
    ''' <summary>Command to define export spatial temporal data connections.</summary>
    Private WithEvents m_cmdEcospaceExportSpatialDatasets As cCommand = Nothing
    ''' <summary>Command to edit an external data set.</summary>
    Private WithEvents m_cmdEditSpatialDataset As cEditSpatialDatasetCommand = Nothing
    ''' <summary>Command to configure the external data connection(s) to a single layer.</summary>
    Private WithEvents m_cmdEcospaceConfigureConnection As cEcospaceConfigureConnectionCommand = Nothing
    ''' <summary>Command to manage external data configurations.</summary>
    Private WithEvents m_cmdEcospaceManageConfigs As cCommand = Nothing

    ' --- Ecobase --

    Private WithEvents m_cmdEcobaseImport As cCommand = Nothing
    Private WithEvents m_cmdEcobaseExport As cCommand = Nothing

    ' --- EIIXML --

    Private WithEvents m_cmdEIIXMLExport As cCommand = Nothing

#End Region ' Commands

    ''' <summary>
    ''' Enumerated type, states how a database was loaded.
    ''' </summary>
    Private Enum eLoadSourceType As Integer
        ''' <summary>Database open attempt originated from the internal API.</summary>
        API = 0
        ''' <summary>Database open attempt originated from the command line.</summary>
        CommandLine
        ''' <summary>Database open attempt originated from the MRU list.</summary>
        MRU
        ''' <summary>Database open attempt originated from the user interface.</summary>
        User
    End Enum

#End Region ' Variables

#Region " Singleton "

    Private Shared __inst__ As frmEwE6 = Nothing

    Public Shared Function GetInstance() As frmEwE6
        Return frmEwE6.__inst__
    End Function

#End Region ' Singleton

#Region " Constructors "

    Public Sub New()

#If 0 Then
        ' Uncomment to torture EwE and see if all decimal comma / point issues have been solved
        Dim culture As CultureInfo = DirectCast(CultureInfo.CurrentCulture.Clone(), CultureInfo)
        culture.NumberFormat.NumberDecimalSeparator = ","
        Thread.CurrentThread.CurrentCulture = culture
        Thread.CurrentThread.CurrentUICulture = culture
#End If

        Me.InitializeComponent()

        Debug.Assert(frmEwE6.__inst__ Is Nothing, "Only one instance of frmEwE6 allowed")
        frmEwE6.__inst__ = Me
        cLog.VerboseLevel = DirectCast(My.Settings.LogVerboseLevel, eVerboseLevel)

        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.m_presentationmode = New cPresentationMode(Me)

        ' Prepare caption
        Me.Text = My.Resources.GENERIC_CAPTION
        ' Prepare Icon
        Me.Icon = cEwEIcon.Current()

    End Sub

#End Region ' Constructors

#Region " IUIElement implementation "

    Public Property UIContext() As cUIContext _
        Implements IUIElement.UIContext

    Public ReadOnly Property Core() As cCore
        Get
            Return Me.UIContext.Core
        End Get
    End Property

    Public ReadOnly Property CoreController() As cCoreController
        Get
            Return Me.m_coreController
        End Get
    End Property

    Public ReadOnly Property StyleGuide() As cStyleGuide
        Get
            Return Me.UIContext.StyleGuide
        End Get
    End Property

    Public ReadOnly Property Help() As cHelp
        Get
            Return Me.UIContext.Help
        End Get
    End Property

    Public ReadOnly Property PropertyManager() As cPropertyManager
        Get
            Return Me.UIContext.PropertyManager
        End Get
    End Property

    Public ReadOnly Property CommandHandler() As cCommandHandler
        Get
            Return Me.UIContext.CommandHandler
        End Get
    End Property

    Public ReadOnly Property SyncObject() As SynchronizationContext
        Get
            Return Me.UIContext.SyncObject
        End Get
    End Property

#End Region ' IUIElement implementation

#Region " Initialization "

    Private Sub ProcessCommandLine()

        Dim astrCmd As String() = Environment.GetCommandLineArgs

        ' Has args?
        If (astrCmd.Length > 1) Then
            ' #Yes: get database parameter
            Dim strDB As String = astrCmd(1).Replace("""", "")
            ' #Yes: is compatible?
            If (cDataSourceFactory.GetSupportedType(strDB) <> eDataSourceTypes.NotSet) Then
                ' #Yes: try to open the model
                Me.LoadEcopathModel(strDB, eLoadSourceType.CommandLine)
            End If
        End If

    End Sub

    Private Sub InitCommands()

        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler

        ' Create and configure File Open command
        Me.m_cmdFileOpen = New cFileOpenCommand(cmdh)

        ' Create and configure File Save command
        Me.m_cmdFileSave = New cFileSaveCommand(cmdh)

        ' Create and configure Directory Open command
        Me.m_cmdDirectoryOpen = New cDirectoryOpenCommand(cmdh)
        Me.m_cmdDirectoryOpen.Directory = Me.Core.OutputPath

        ' Create and configure Execute command
        Me.m_cmdExecute = New cExecuteCommand(cmdh)

        ' Create and configure new command
        Me.m_cmdNewModel = New cCommand(cmdh, "NewEcopathModel")
        Me.m_cmdNewModel.AddControl(Me.m_tsmiFileNew)

        ' Create and configure open command
        Me.m_cmdLoadModel = New cCommand(cmdh, "LoadEcopathModel")
        Me.m_cmdLoadModel.AddControl(Me.m_tsmiFileOpen)
        Me.m_cmdLoadModel.AddControl(Me.m_tsbEcopath)

        ' Create and configure open output location command
        Me.m_cmdOpenOutput = New cCommand(cmdh, "OpenOutputLocation")
        Me.m_cmdOpenOutput.AddControl(Me.m_tsmiOpenOutput)

        Me.m_cmdSave = New cCommand(cmdh, "SaveModel", My.Resources.COMMAND_SAVECHANGES)
        Me.m_cmdSave.AddControl(Me.m_tsmiFileSave)
        Me.m_cmdSave.AddControl(Me.m_tsbSave)

        ' Create and configure save commands
        Me.m_cmdSaveModelAs = New cCommand(cmdh, "SaveModelAs")
        Me.m_cmdSaveModelAs.AddControl(Me.m_tsmiFileSaveAs)

        ' Create and configure 'close model' command
        Me.m_cmdCloseModel = New cCommand(cmdh, "CloseModel")
        Me.m_cmdCloseModel.AddControl(Me.m_tsmiFileClose)

        ' Create and configure 'compact model' command
        Me.m_cmdCompactModel = New cCommand(cmdh, "CompactModel")
        Me.m_cmdCompactModel.AddControl(Me.m_tsmiFileCompact)

        ' Create and configure 'close document' command
        Me.m_cmdCloseDocument = New cCommand(cmdh, "CloseDocument")
        Me.m_cmdCloseDocument.AddControl(Me.m_tsmiWindowsClose)

        ' Create and configure navigate command
        Me.m_cmdNavigate = New cNavigationCommand(cmdh)

        ' Create and configure print command
        Me.m_cmdPrint = New cPrintCommand(cmdh)
        Me.m_cmdPrint.AddControl(Me.m_tsmiPrint)

        ' Create and configure 'close all forms' command
        Me.m_cmdCloseAllForms = New cCommand(cmdh, "CloseAllForms")
        Me.m_cmdCloseAllForms.AddControl(Me.m_tsmiWindowsCloseAll)

        'Create and configure 'new ecosim scenario' command
        Me.m_cmdNewEcosimScenario = New cCommand(cmdh, "NewEcosimScenario")
        Me.m_cmdNewEcosimScenario.AddControl(Me.m_tsmiEcosimNew)

        'Create and configure 'load ecosim scenario' command
        Me.m_cmdLoadEcosimScenario = New cCommand(cmdh, "LoadEcosimScenario")
        Me.m_cmdLoadEcosimScenario.AddControl(Me.m_tsmiEcosimLoad)
        Me.m_cmdLoadEcosimScenario.AddControl(Me.m_tsbEcosim)

        ''Create and configure 'save ecosim scenario' command
        'Me.m_cmdSaveEcosimScenario = New cCommand(cmdh, "SaveEcosimScenario")
        'Me.m_cmdSaveEcosimScenario.AddControl(Me.m_tsmiEcosimSave)

        'Create and configure 'close ecosim scenario' command
        Me.m_cmdCloseEcosimScenario = New cCommand(cmdh, "CloseEcosimScenario")
        Me.m_cmdCloseEcosimScenario.AddControl(Me.m_tsmiEcosimClose)

        'Create and configure 'save ecosim scenario as' command
        Me.m_cmdSaveEcosimScenarioAs = New cCommand(cmdh, "SaveEcosimScenarioAs")
        Me.m_cmdSaveEcosimScenarioAs.AddControl(Me.m_tsmiEcosimSaveAs)

        'Create and configure 'delete ecosim scenario' command
        Me.m_cmdDeleteEcosimScenario = New cCommand(cmdh, "DeleteEcosimScenarioAs")
        Me.m_cmdDeleteEcosimScenario.AddControl(Me.m_tsmiEcosimDelete)

        'Create and configure 'new ecospace scenario' command
        Me.m_cmdNewEcospaceScenario = New cCommand(cmdh, "NewEcospaceScenario")
        Me.m_cmdNewEcospaceScenario.AddControl(Me.m_tsmiEcospaceNew)

        'Create and configure 'load ecospace scenario' command
        Me.m_cmdLoadEcospaceScenario = New cCommand(cmdh, "LoadEcospaceScenario")
        Me.m_cmdLoadEcospaceScenario.AddControl(Me.m_tsmiEcospaceLoad)
        Me.m_cmdLoadEcospaceScenario.AddControl(Me.m_tsbEcospace)

        ''Create and configure 'save ecospace scenario' command
        'Me.m_cmdSaveEcospaceScenario = New cCommand(cmdh, "SaveEcospaceScenario")
        'Me.m_cmdSaveEcospaceScenario.AddControl(Me.m_tsmiEcospaceSave)

        'Create and configure 'close ecospace scenario' command
        Me.m_cmdCloseEcospaceScenario = New cCommand(cmdh, "CloseEcospaceScenario")
        Me.m_cmdCloseEcospaceScenario.AddControl(Me.m_tsmiEcospaceClose)

        'Create and configure 'save ecospace scenario as' command
        Me.m_cmdSaveEcospaceScenarioAS = New cCommand(cmdh, "SaveEcospaceScenarioAs")
        Me.m_cmdSaveEcospaceScenarioAS.AddControl(Me.m_tsmiEcospaceSaveAs)

        'Create and configure 'delete ecospace scenario' command
        Me.m_cmdDeleteEcospaceScenario = New cCommand(cmdh, "DeleteEcospaceScenario")
        Me.m_cmdDeleteEcospaceScenario.AddControl(Me.m_tsmiEcospaceDelete)

        'Create and configure 'new ecotracer scenario' command
        Me.m_cmdNewEcotracerScenario = New cCommand(cmdh, "NewEcotracerScenario")
        Me.m_cmdNewEcotracerScenario.AddControl(Me.m_tsmiEcotracerNew)

        'Create and configure 'load ecotracer scenario' command
        Me.m_cmdLoadEcotracerScenario = New cCommand(cmdh, "LoadEcotracerScenario")
        Me.m_cmdLoadEcotracerScenario.AddControl(Me.m_tsmiEcotracerLoad)
        Me.m_cmdLoadEcotracerScenario.AddControl(Me.m_tsbEcotracer)

        ''Create and configure 'save ecotracer scenario' command
        'Me.m_cmdSaveEcotracerScenario = New cCommand(cmdh, "SaveEcotracerScenario")
        'Me.m_cmdSaveEcotracerScenario.AddControl(Me.m_tsmiEcotracerSave)

        'Create and configure 'close ecotracer scenario' command
        Me.m_cmdCloseEcotracerScenario = New cCommand(cmdh, "CloseEcotracerScenario")

        'Create and configure 'save ecotracer scenario as' command
        Me.m_cmdSaveEcotracerScenarioAS = New cCommand(cmdh, "SaveEcotracerScenarioAs")
        Me.m_cmdSaveEcotracerScenarioAS.AddControl(Me.m_tsmiEcotracerSaveAs)

        'Create and configure 'delete ecospace scenario' command
        Me.m_cmdDeleteEcotracerScenario = New cCommand(cmdh, "DeleteEcotracerScenario")
        Me.m_cmdDeleteEcotracerScenario.AddControl(Me.m_tsmiEcotracerDelete)

        'Create and configure 'view Navtree' command
        Me.m_cmdViewNavPane = New cCommand(cmdh, "ViewNavPane")
        Me.m_cmdViewNavPane.AddControl(Me.m_tsmiViewNavigation)

        'Create and configure 'view start page' command
        Me.m_cmdBrowseURI = New cBrowserCommand(cmdh)
        Me.m_cmdBrowseURI.AddControl(Me.m_tsmiViewStartPage)

        'Create and configure 'view status pane' command
        Me.m_cmdViewStatusPane = New cCommand(cmdh, "ViewStatusPane")
        Me.m_cmdViewStatusPane.AddControl(Me.m_tsmiViewStatus)

        'Create and configure 'view properties pane' command
        Me.m_cmdViewRemarkPane = New cCommand(cmdh, "ViewPropertiesPane")
        Me.m_cmdViewRemarkPane.AddControl(Me.m_tsmiViewRemarks)

        'Create and configure 'view menu' command
        Me.m_cmdViewMenu = New cCommand(cmdh, "ViewMenu")
        Me.m_cmdViewMenu.AddControl(Me.m_tsmiViewMenu)

        'Create and configure 'view Buttonbar' command
        Me.m_cmdViewModelBar = New cCommand(cmdh, "ViewModelBar")
        Me.m_cmdViewModelBar.AddControl(Me.m_tsmiViewModelBar)

        'Create and configure 'view statusbar' command
        Me.m_cmdViewStatusbar = New cCommand(cmdh, "ViewStatusbar")
        Me.m_cmdViewStatusbar.AddControl(Me.m_tsmiViewStatusBar)

        'Create and configure 'presentation mode' command
        Me.m_cmdViewPresentationMode = New cCommand(cmdh, "ViewPresentationMode")
        Me.m_cmdViewPresentationMode.AddControl(Me.m_tsmiPresentation)

        'Create and configure 'show options' command
        Me.m_cmdShowOptions = New cShowOptionsCommand(cmdh)
        Me.m_cmdShowOptions.AddControl(Me.m_tsmiOptions)

        'Create and configure 'show tools' command
        Me.m_cmdShowTools = New cCommand(cmdh, "ShowTools")
        Me.m_cmdShowTools.AddControl(Me.m_tsmiExternalTools)

        'Create and configure 'edit reference map' command
        Me.m_cmdEditReferenceMap = New cCommand(cmdh, "EditRefMap")

        'Create and configure 'Autosave results' command
        Me.m_cmdAutosaveResults = New cCommand(cmdh, "AutosaveResults", My.Resources.COMMAND_AUTOSAVE)
        Me.m_cmdAutosaveResults.AddControl(Me.m_tsbnAutosaveResults)

        'Create and configure EditGroups command
        Me.m_cmdEditGroups = New cCommand(cmdh, "EditGroups")
        Me.m_cmdEditGroups.AddControl(Me.m_tsmiEcopathDefineGroups)

        'Create and configure EditMultiStanza command
        Me.m_cmdEditMultiStanza = New cCommand(cmdh, "EditMultiStanza")
        Me.m_cmdEditMultiStanza.AddControl(Me.m_tsmiEcopathDefineMultiStanza)

        'Create and configure EditFleets command
        Me.m_cmdEditFleets = New cCommand(cmdh, "EditFleets")
        Me.m_cmdEditFleets.AddControl(Me.m_tsmiEcopathDefineFleets)

        Me.m_cmdEditPedigree = New cEditPedigreeCommand(cmdh)
        Me.m_cmdEditPedigree.AddControl(Me.m_tsmiEcopathDefinePedigree)

        Me.m_cmdEditTaxonomy = New cCommand(cmdh, "EditTaxonomy")
        Me.m_cmdEditTaxonomy.AddControl(Me.m_tsmiEcopathDefineTraits)

        Me.m_cmdEditBasemap = New cCommand(cmdh, "EditBasemap")
        Me.m_cmdEditBasemap.AddControl(Me.m_tsmiEcospaceEditMap)

        Me.m_cmdEditHabitats = New cEditHabitatsCommand(cmdh)
        Me.m_cmdEditHabitats.AddControl(Me.m_tsmiEcospaceDefineHabitats)

        Me.m_cmdEditMPAs = New cEditMPAsCommand(cmdh)
        Me.m_cmdEditMPAs.AddControl(Me.m_tsmiEcospaceDefineMPAs)

        Me.m_cmdEditRegions = New cEditRegionsCommand(cmdh)
        Me.m_cmdEditRegions.AddControl(Me.m_tsmiEcospaceDefineRegions)

        Me.m_cmdDefineImportanceMaps = New cEditImportanceLayersCommand(cmdh)
        Me.m_cmdDefineImportanceMaps.AddControl(Me.m_tsmiEcospaceDefineImportanceMaps)

        Me.m_cmdDefineInputLayers = New cEditDriverLayersCommand(cmdh)
        Me.m_cmdDefineInputLayers.AddControl(Me.m_tsmiEcospaceInputMaps)

        Me.m_cmdDefineSpatialDatasets = New cCommand(cmdh, "EditSpatialDatasets")
        Me.m_cmdDefineSpatialDatasets.AddControl(Me.m_tsmiEcospaceDatasets)

        Me.m_cmdEditSpatialDataset = New cEditSpatialDatasetCommand(cmdh)

        Me.m_cmdEcospaceExportSpatialDatasets = New cCommand(cmdh, "ExportSpatialDatasets")
        Me.m_cmdEcospaceConfigureConnection = New cEcospaceConfigureConnectionCommand(cmdh)

        Me.m_cmdEcospaceManageConfigs = New cCommand(cmdh, "ManageSpatialDatasetConfigurations")

        Me.m_cmdImportLayerData = New cImportLayerCommand(cmdh)
        Me.m_cmdImportLayerData.AddControl(Me.m_tsmiEcospaceImportLayers)

        Me.m_cmdExportLayerData = New cExportLayerCommand(cmdh)
        Me.m_cmdExportLayerData.AddControl(Me.m_tsmiEcospaceExportLayers)

        Me.m_cmdEditLayer = New cEditLayerCommand(cmdh)

        Me.m_cmdEcosimTrimShapes = New cCommand(cmdh, "TrimUnusedShapeData")
        Me.m_cmdEcosimChangeShape = New cCommand(cmdh, "ChangeEcosimShape")

        Me.m_cmdImportTimeSeries = New cCommand(cmdh, "ImportTimeSeries")
        Me.m_cmdImportTimeSeries.AddControl(Me.m_tsmiTimeSeriesImport)

        Me.m_cmdEcosimLoadTimeSeries = New cCommand(cmdh, "LoadTimeSeries")
        Me.m_cmdEcosimLoadTimeSeries.AddControl(Me.m_tsmiTimeSeriesLoad)

        Me.m_cmdEcospaceLoadTimeSeries = New cCommand(cmdh, "LoadSpatialTemporalDataset")
        'Me.m_cmdEcospaceLoadTimeSeries.AddControl(Me.m_tsmiTimeSeriesLoad)

        Me.m_cmdWeightTimeSeries = New cCommand(cmdh, "WeightTimeSeries")
        Me.m_cmdWeightTimeSeries.AddControl(Me.m_tsmiTimeSeriesEditWeights)

        Me.m_cmdExportTimeSeries = New cCommand(cmdh, "ExportTimeSeries")
        Me.m_cmdExportTimeSeries.AddControl(Me.m_tsmiTimeSeriesExport)

        Me.m_cmdHelpAbout = New cCommand(cmdh, "HelpAbout")
        Me.m_cmdHelpAbout.AddControl(Me.m_tsmiHelpAbout)

        Me.m_cmdHelpReportIssue = New cCommand(cmdh, "ReportIssue")
        Me.m_cmdHelpReportIssue.AddControl(Me.m_tsmiHelpReportIssue)

        Me.m_cmdHelpRequestCodeAccess = New cCommand(cmdh, "RequesCodeAccess")
        Me.m_cmdHelpRequestCodeAccess.AddControl(Me.m_tsmiHelpRequestSourceCodeAccess)

        Me.m_cmdHelpFeedback = New cCommand(cmdh, "HelpFeedback")

        Me.m_cmdPickColor = New cPickColorCommand(cmdh)

#If BETA = 1 Then
        Me.m_cmdHelpReportIssue.AddControl(Me.m_tsbnPreview)
        Me.m_tsbnPreview.Visible = True

        Me.m_cmdHelpFeedback.AddControl(Me.m_tsbnFeedback)
        Me.m_cmdHelpFeedback.AddControl(Me.m_tsmiHelpFeedback)
        Me.m_tsbnFeedback.Visible = True
        Me.m_tsmiHelpFeedback.Visible = True
#Else
        Me.m_tsbnPreview.Visible = False
        Me.m_tsbnFeedback.Visible = False
        Me.m_tsmiHelpFeedback.Visible = False
#End If

        Me.m_cmdPluginGUICommand = New cPluginGUICommand(cmdh)

        Me.m_cmdPropertySelection = New cPropertySelectionCommand(cmdh)

        Me.m_cmdShowHideItems = New cDisplayGroupsCommand(cmdh)
        Me.m_cmdShowHideItems.AddControl(Me.m_tsmiViewItems)

        Me.m_cmdEnableEcotracer = New cCommand(cmdh, "EnableEcotracer")

        Me.m_cmdEstimateVs = New cCommand(cmdh, "EstimateVs")

        Me.m_cmdExportEcosimResultsToCSV = New cEcosimSaveDataCommand(cmdh)

        Me.m_cmdEcospaceLoadXYRefData = New cCommand(cmdh, "EcospaceLoadXYRefData")
        Me.m_cmdEcospaceLoadXYRefData.AddControl(Me.m_tsmiEcospaceLoadXYRefData)

        ' --- Ecobase ---

        Me.m_cmdEcobaseImport = New cCommand(cmdh, "EcobaseImport")
        Me.m_cmdEcobaseImport.AddControl(Me.m_tsmiEcobaseImport)
        'Me.m_tsmiEcobaseImport.Image = My.Resources.EcoBase

        Me.m_cmdEcobaseExport = New cCommand(cmdh, "EcobaseExport")
        Me.m_cmdEcobaseExport.AddControl(Me.m_tsmiEcobaseExport)

        ' --- EIIXML ---

        Me.m_cmdEIIXMLExport = New cCommand(cmdh, "EIIXMLExport")
        Me.m_cmdEIIXMLExport.AddControl(Me.m_tsmiEIIXMLExport)

        ' ---

        Me.m_tslbReadOnly.Image = SharedResources.ProtectFormHS
        Me.m_tslbReadOnly.Enabled = False

        ' Listen to application Idle events to update command states
        AddHandler Application.Idle, AddressOf cmdh.OnIdle
        AddHandler Application.Idle, AddressOf Me.m_pluginMenuHandler.OnIdle

    End Sub

    Private Sub InitPanels()

        ' Initialize panels
        Try
            Me.m_dtPanels(cPANEL_NAV) = New frmNavigationPanel(Me.UIContext, Me.m_pluginManager)
            Me.m_dtPanels(cPANEL_STATUS) = New frmStatusPanel(Me.UIContext, Me.m_messageHistory)
            Me.m_dtPanels(cPANEL_REMARKS) = New frmRemarkPanel(Me.UIContext)
            Me.m_dtPanels(cPANEL_START) = New frmStartPanel(Me.UIContext)
        Catch ex As Exception

        End Try

    End Sub

    Private Function Panel(ByVal strPanelName As String) As frmEwEDockContent
        Return Me.m_dtPanels(strPanelName)
    End Function

    Private Sub InitDockPanelPositions()

        If cSystemUtils.IsRightToLeft Then
            Me.Panel(cPANEL_NAV).Show(m_DockPanel, DockState.DockRight)
        Else
            Me.Panel(cPANEL_NAV).Show(m_DockPanel, DockState.DockLeft)
        End If
        Me.Panel(cPANEL_STATUS).Show(m_DockPanel, DockState.DockBottomAutoHide)
        Me.Panel(cPANEL_REMARKS).Show(m_DockPanel, DockState.DockBottomAutoHide)

    End Sub

    Private Sub InitCoreParams()

        Dim so As SynchronizationContext = SynchronizationContext.Current

        If so Is Nothing Then
            'create the sync object on the same thread that created the frmEwE6
            so = New SynchronizationContext()
        End If

        Dim core As New cCore()
        Dim sg As New cStyleGuide(core)
        Dim cmdh As New cCommandHandler()
        Dim pm As New cPropertyManager(core, so)
        Dim fps As New cFormSettings()
        Dim help As New cHelp(Me, "UserGuide\EwE6_userguide.chm", "User Interface.htm", "EWE_UsersGuide")

        Me.UIContext = New cUIContext(core, sg, pm, cmdh, Me, fps, help, so)

        ' Configure state monitor
        Me.Core.StateMonitor.SyncObject = Me
        Me.m_mhProgress = New cMessageHandler(AddressOf OnProgressMessage, eCoreComponentType.External, eMessageType.Progress, Me.SyncObject)
        Me.m_mhEcosim = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.EcoSim, eMessageType.DataAddedOrRemoved, Me.SyncObject)
        Me.m_mhEcospace = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.EcoSpace, eMessageType.DataAddedOrRemoved, Me.SyncObject)
        Me.m_mhEcotracer = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.Ecotracer, eMessageType.DataAddedOrRemoved, Me.SyncObject)
        Me.m_mhTimeseries = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.TimeSeries, eMessageType.DataAddedOrRemoved, Me.SyncObject)

#If DEBUG Then
        Me.m_mhProgress.Name = "frmEwE6:Progress"
        Me.m_mhEcosim.Name = "frmEwE6:Ecosim"
        Me.m_mhEcospace.Name = "frmEwE6:EcoSpace"
        Me.m_mhEcotracer.Name = "frmEwE6:EcoTracer"
        Me.m_mhTimeseries.Name = "frmEwE6:TimeSeries"
#End If

        Me.Core.Messages.AddMessageHandler(Me.m_mhProgress)
        Me.Core.Messages.AddMessageHandler(Me.m_mhEcosim)
        Me.Core.Messages.AddMessageHandler(Me.m_mhEcospace)
        Me.Core.Messages.AddMessageHandler(Me.m_mhEcotracer)
        Me.Core.Messages.AddMessageHandler(Me.m_mhTimeseries)

        ' Create message history
        Me.m_messageHistory = New cMessageHistory()
        Me.m_messageHistory.UIContext = Me.UIContext

        ' Create plug-in manager for this GUI
        Me.m_pluginManager = New cPluginManager()
        Me.m_pluginManager.UIContext = Me.UIContext
        Me.m_pluginManager.SyncObject = Me.UIContext.SyncObject

        ' Configure plug-in manager
        Me.m_pluginManager.Core = Me.Core
        Me.m_pluginManager.UIContext = Me.UIContext

        ' Distribute plug-in manager
        Me.Core.PluginManager = Me.m_pluginManager

        ' Create plug-in menu handler to position plug-in menu items in the main menu from this form
        Me.m_pluginMenuHandler = New cPluginMenuHandler(Me.MainMenuStrip, Me.m_pluginManager, Me.UIContext.CommandHandler)

        ' Initialize core controller
        Me.m_coreController = New cCoreController(Me.Core.StateMonitor, Me.Core.StateManager, Me)

        ' Initialize style guide updater
        Me.m_styleguideupdater = New cStyleGuideUpdater(Me.UIContext)
        Me.m_styleguideupdater.Load()

        ' Initialize autosave logic
        Me.m_autosavemanager = New cAutosaveSettingsManager(Me.UIContext.Core)

        Me.Core.SetMessagePumpDelegate(AddressOf Me.OnPumpCoreMessages)

    End Sub

    Private Sub OnPumpCoreMessages()
        Try
            System.Windows.Forms.Application.DoEvents()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub InitEventHandlers()

        AddHandler My.Settings.SettingsLoaded, AddressOf OnSettingsLoaded
        AddHandler My.Settings.SettingsSaving, AddressOf OnSettingsSaving
        AddHandler My.Settings.PropertyChanged, AddressOf OnSettingsChanged

        ' JS 27Apr10: ActiveContent seems to track much more accurately than ActiveDocument
        AddHandler Me.m_DockPanel.ActiveContentChanged, AddressOf OnTabFocusChanged

    End Sub

#End Region ' Initialization

#Region " Properties "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the file name of the current loaded model.
    ''' </summary>
    ''' <param name="bFullPath">Flag stating thether the full path needs to be 
    ''' returned.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property SelectedFileName(Optional ByVal bFullPath As Boolean = True) As String
        Get
            If (Me.Core Is Nothing) Then Return ""
            Dim ds As IEwEDataSource = Me.Core.DataSource
            If (ds Is Nothing) Then
                Return ""
            Else
                If bFullPath Then
                    Return ds.ToString()
                Else
                    Return Path.GetFileName(ds.ToString())
                End If
            End If
        End Get
    End Property

#End Region ' Properties

#Region " Messages "

    Private Delegate Sub SendMessageDelegate(ByVal strMsg As String, ByVal importance As eMessageImportance, ByVal component As eCoreComponentType)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Send a message via the core.
    ''' </summary>
    ''' <param name="strMsg">Message text to send.</param>
    ''' <param name="importance">Message importance.</param>
    ''' <param name="component">Core component to represent as message origin.</param>
    ''' -----------------------------------------------------------------------
    Public Sub SendMessage(ByVal strMsg As String,
                           Optional ByVal importance As eMessageImportance = eMessageImportance.Warning,
                           Optional ByVal component As eCoreComponentType = eCoreComponentType.External,
                           Optional strHyperlink As String = "")

        If Me.InvokeRequired() Then
            Me.Invoke(New SendMessageDelegate(AddressOf Me.SendMessage),
                                              New Object() {strMsg, importance, component})
            Return
        End If

        Dim msg As New cMessage(strMsg, eMessageType.Any, component, importance)
        msg.Hyperlink = strHyperlink

        Me.Core.Messages.SendMessage(msg)

    End Sub

    Private Delegate Function AskFeedbackDelegate(ByVal strMsg As String, ByVal importance As eMessageImportance, ByVal component As eCoreComponentType, ByVal replies As eMessageReplyStyle, ByVal defaultReply As eMessageReply, ByVal strHyperlink As String, ByVal vars As cVariableStatus()) As eMessageReply

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ask for user feedback via the core feedback messaging system.
    ''' </summary>
    ''' <param name="strMsg">Message text to send.</param>
    ''' <param name="importance">Message importance.</param>
    ''' <param name="component">Core component to represent as message origin.</param>
    ''' -----------------------------------------------------------------------
    Public Function AskFeedback(ByVal strMsg As String,
                                Optional ByVal importance As eMessageImportance = eMessageImportance.Warning,
                                Optional ByVal component As eCoreComponentType = eCoreComponentType.Core,
                                Optional ByVal replystyle As eMessageReplyStyle = eMessageReplyStyle.YES_NO_CANCEL,
                                Optional ByVal defaultreply As eMessageReply = eMessageReply.YES,
                                Optional strHyperlink As String = "",
                                Optional vars As cVariableStatus() = Nothing) As eMessageReply

        If Me.InvokeRequired() Then
            Dim dlgt As New AskFeedbackDelegate(AddressOf Me.AskFeedback)
            Dim aparms() As Object = New Object() {strMsg, importance, component, replystyle, defaultreply, vars}
            Return DirectCast(Me.Invoke(dlgt, aparms), eMessageReply)
        End If

        Dim fmsg As New cFeedbackMessage(strMsg, component, eMessageType.Any, importance, replystyle, eDataTypes.NotSet, defaultreply)
        If (vars IsNot Nothing) Then fmsg.Variables.AddRange(vars)
        fmsg.Hyperlink = strHyperlink
        Me.Core.Messages.SendMessage(fmsg)
        Return fmsg.Reply

    End Function

#End Region ' Messages

#Region " Form overrides "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Overridden to initialize the app launcer form.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

        Me.SuspendLayout()

        ' Add the dock panel 
        Me.m_DockPanel = New DockPanel()
        Me.m_DockPanel.Parent = Me
        Me.m_DockPanel.Dock = DockStyle.Fill
        Me.m_DockPanel.ShowDocumentIcon = True
        Me.m_DockPanel.BringToFront()

        My.Settings.Reload()

        ' Peek at key presses but does not consume them
        Me.KeyPreview = True

        Me.InitCoreParams()
        Me.InitCommands()
        Me.InitPanels()
        Me.InitEventHandlers()

        Me.InitDockPanelPositions()

#If Not DEBUG Then
        ' Show start page (but not in DEBUG mode)
        Me.Panel(cPANEL_START).Show(Me.m_DockPanel, DockState.Document)
#End If

        ' Start controlling the status strip
        Me.m_ssMain.Attach(Me.UIContext, Me)
        ' Start controlling forms
        Me.m_formstatemanager = New cEwEFormStateManager(Me.Core.StateMonitor, Me.m_coreController, Me.m_DockPanel)
        Me.Help.HelpTopic(Me.Panel(cPANEL_START)) = "Ecopath with Ecosim 6 Getting started.htm"

        ' Load plugins once GUI has been created.
        Me.LoadPlugins()
        ' Auto-launch plugins
        Me.AutolaunchPlugins()

        ' JS 11Sep14: this will be done when settings are loaded
        'Me.Core.SpatialDataConnectionManager.DatasetManager.Load(My.Settings.SpatialTemporalConfigFile)

        Me.ProcessCommandLine()
        Me.OnSettingsLoaded(Nothing, Nothing) ' Ugh!
        Me.UpdateModelControls()

        AddHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged

        Me.ResumeLayout()

        Try
            ' Dismiss splash screen, if any
            Dim splash As frmSplash = frmSplash.GetInstance()
            If (splash IsNot Nothing) Then
                splash.Close()
            End If
        Catch ex As Exception

        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, catches the form closing event to make sure the core is finalized.
    ''' Application shut-down is cancelled if the core does not finalize correctly.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnFormClosing(ByVal e As FormClosingEventArgs)

        Try
            ' Cancel application shut down if the core does not terminate succesfully.
            e.Cancel = Not Me.CloseEcopathModel()
            ' Abort if Ecopath model did not close sucessfully
            If e.Cancel Then Return

            ' Last-ditch cleanup
            cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_CLEANUP_TEMPFILES)
            cFileUtils.PurgeTempFiles()
            cApplicationStatusNotifier.EndProgress(Me.Core)

        Catch ex As Exception
            cLog.Write(ex, "frmEwE6.OnFormClosing")
        End Try

        ' Resume shutdown
        MyBase.OnFormClosing(e)

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        If (Me.UIContext IsNot Nothing) Then

            Try

                Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
                RemoveHandler Application.Idle, AddressOf cmdh.OnIdle
                RemoveHandler Application.Idle, AddressOf Me.m_pluginMenuHandler.OnIdle

                RemoveHandler Me.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged
                RemoveHandler Me.m_DockPanel.ActiveContentChanged, AddressOf OnTabFocusChanged
                RemoveHandler My.Settings.SettingsLoaded, AddressOf OnSettingsLoaded
                RemoveHandler My.Settings.SettingsSaving, AddressOf OnSettingsSaving
                RemoveHandler My.Settings.PropertyChanged, AddressOf OnSettingsChanged

                Me.m_formstatemanager.Dispose()
                Me.m_formstatemanager = Nothing

                Me.Core.Messages.RemoveMessageHandler(Me.m_mhProgress)
                Me.Core.Messages.RemoveMessageHandler(Me.m_mhEcosim)
                Me.Core.Messages.RemoveMessageHandler(Me.m_mhEcospace)
                Me.Core.Messages.RemoveMessageHandler(Me.m_mhEcotracer)
                Me.Core.Messages.RemoveMessageHandler(Me.m_mhTimeseries)
                Me.m_mhProgress = Nothing
                Me.m_mhEcosim = Nothing
                Me.m_mhEcospace = Nothing
                Me.m_mhEcotracer = Nothing
                Me.m_mhTimeseries = Nothing

                ' Terminate all model-independent UI components
                Me.CloseAllDocuments()
                Me.ClearScenarioDropdowns()
                Me.ClearModelMRUDropdowns()

                ' JS 13Dec10: Another attempt to free tooltip memory 
                Dim ts As cToolTipShared = cToolTipShared.GetInstance()
                ts.RemoveAll()
                ts.Dispose()

                ' JS 26Aug15: the start panel throws a null ref exception in release mode during explicit cleanup.
                ' This cannot be reproduced in debug mode> I suspect that the issue is caused by the nested version of IE
                ' Bypassing explicit destruction also makes the issue go away. Whoah.
                'For Each p As frmEwEDockContent In Me.m_dtPanels.Values
                '    Try
                '        p.Close()
                '        p.Dispose()
                '    Catch ex As Exception
                '        cLog.Write(ex, "frmEwE6.OnFormClosed(" & p.Name & ")")
                '    End Try
                'Next
                Me.m_dtPanels.Clear()

                Me.m_messageHistory.Dispose()
                Me.m_messageHistory = Nothing

                Me.UIContext.PropertyManager.Dispose()
                Me.UIContext.StyleGuide.Dispose()

                Me.m_pluginManager.UIContext = Nothing
                Me.UIContext = Nothing

                ' Clear commands after all UI elements have lost their UI context, which 
                ' should have triggered proper cleanups
                cmdh.Clear()

                Me.m_DockPanel.Dispose()

            Catch ex As Exception
                cLog.Write(ex, "frmEwE6.OnFormClosed")
            End Try
        End If

        MyBase.OnFormClosed(e)

    End Sub

#Region " KeyDown "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Cluck?
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnKeyDown(ByVal e As KeyEventArgs)

        Try
            ' Restore menu and full screen mode on 'Escape'
            If (e.KeyCode = Keys.Escape) Then
                If (Me.m_cmdViewPresentationMode.Checked) Then
                    Me.m_cmdViewPresentationMode.Invoke()
                End If
                If (Me.m_cmdViewMenu.Checked = False) Then
                    Me.m_cmdViewMenu.Invoke()
                End If
            End If

            ' Egg!
            If e.Alt And e.Control And e.Shift Then
                Dim strURL As String = ""
                Select Case e.KeyCode
                    Case Keys.Oemtilde : strURL = "http://farm1.static.flickr.com/160/374820104_5ec655655c.jpg"
                End Select

                If Not String.IsNullOrEmpty(strURL) Then
                    Dim startpage As frmStartPanel = DirectCast(Me.Panel(cPANEL_START), frmStartPanel)
                    startpage.URL = strURL
                    startpage.Show(Me.m_DockPanel, DockState.Document)
                End If

            End If
        Catch ex As Exception

        End Try
    End Sub

#End Region ' KeyDown

#Region " Drag and drop "

    Protected Overrides Sub OnDragOver(e As System.Windows.Forms.DragEventArgs)
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim astrFiles() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
            If astrFiles.Length > 0 Then
                If cDataSourceFactory.GetSupportedType(astrFiles(0)) <> eDataSourceTypes.NotSet Then
                    e.Effect = DragDropEffects.All
                End If
            End If
        End If
        MyBase.OnDragOver(e)
    End Sub

    Protected Overrides Sub OnDragDrop(e As System.Windows.Forms.DragEventArgs)
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Try
                Dim astrFiles() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
                If astrFiles.Length > 0 Then
                    Me.LoadEcopathModel(astrFiles(0), eLoadSourceType.User)
                End If
            Catch ex As Exception

            End Try
        End If
        MyBase.OnDragDrop(e)
    End Sub

#End Region ' Drag and drop

#End Region ' Form overrides

#Region " Status feedback "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Set the application status strip text and wait cursor.
    ''' </summary>
    ''' <param name="strText">Status text to display, if any.</param>
    ''' <param name="state">
    ''' <para>Flag stating whether a wait cursor should be shown.
    ''' Values are interpreted as follows:</para>
    ''' <list type="bullet">
    ''' <item><description><see cref="eProgressState.Start"/>: wait cursor will be set.</description></item>
    ''' <item><description><see cref="eProgressState.Finished"/>: wait cursor will be cleared.</description></item>
    ''' <item><description><see cref="eProgressState.Running"/>: wait cursor state will not change.</description></item>
    ''' </list>
    ''' </param>
    ''' <param name="sProgress">Ratio [0, 1] of progress to display. 0 to hide progress.</param>
    ''' <remarks>
    ''' Note that the wait cursor state is maintained via an internal counter. Setting
    ''' the wait cursor state will increment this counter, clearing the wait cursor state
    ''' decrements the counter. The actual wait cursor will be set when this counter is non-zero,
    ''' and is cleared when this counter reaches zero.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Sub ShowProgress(ByVal state As eProgressState, ByVal strText As String, ByVal sProgress As Single)

        ' Should have been handled
        If Me.InvokeRequired() Then Return

        ' ToDo_JS: Consider using a timer to clear any status text after a certain interval

        ' Update wait cursor
        Select Case state

            Case eProgressState.Start

                ' Push text to the status text stack
                Me.m_lstrStatus.Insert(0, strText)
                ' Set wait cursor
                Me.Cursor = Cursors.WaitCursor

            Case eProgressState.Finished

                ' Has wait cursors pending?
                If Me.m_lstrStatus.Count > 0 Then
                    ' #Yes: no text specified?
                    If String.IsNullOrEmpty(strText) Then
                        ' #Yes: obtain text from the status text stack
                        strText = Me.m_lstrStatus(0)
                    End If
                    ' Pop text from the status text stack
                    Me.m_lstrStatus.RemoveAt(0)
                End If

                ' Status stack empty?
                If Me.m_lstrStatus.Count = 0 Then
                    ' #Yes: restore default cursor
                    Me.Cursor = Cursors.Default
                    strText = ""
                    sProgress = 0
                End If

            Case eProgressState.Running
                ' Don't do anything. Really.

        End Select

        ' JS 12oct07: disabled total refresh to minimize screen flickering
        '' Redraw!
        'Me.Refresh()

        ' Update status text
        Me.m_ssMain.SetStatusText(strText, sProgress)

    End Sub

#End Region ' Status feedback

#Region " Plug-ins "

    Private Sub AutolaunchPlugins()
        Using pl As New cPluginAutolaunchHandler(Me.m_pluginManager, Me.UIContext.CommandHandler)
            ' Hah! The 'using' construction here will deal with proper disposal
        End Using
    End Sub

    Private Sub LoadPlugins()

        Dim strMessage As String = ""
        Dim reply As eMessageReply = eMessageReply.OK
        Dim bNeedReply As Boolean = False
        Dim alDisabledPlugins As ArrayList = My.Settings.DisabledPlugins

        Try
            ' Load plug-ins from EwE root folder
            Me.m_pluginManager.LoadPlugins(alDisabledPlugins, [option]:=SearchOption.TopDirectoryOnly)
            ' Load plug-ins from dedicated plug-ins subfolder, recursively
            Me.m_pluginManager.LoadPlugins(alDisabledPlugins, ".\plugins")
        Catch ex As Exception
            ' Ouch!
        End Try

    End Sub

#End Region ' Plug-ins

#Region " Database utils "

    Private Function CompactModel() As Boolean

        Dim ds As IEwEDataSource = Me.Core.DataSource
        Dim result As eDatasourceAccessType = eDatasourceAccessType.Success
        Dim strFileName As String = Me.SelectedFileName()
        Dim strMessage As String = ""
        Dim bSucces As Boolean = True

        If (Me.AskFeedback(My.Resources.PROMPT_MODEL_COMPACT) <> eMessageReply.YES) Then
            Return False
        End If

        If Me.CloseEcopathModel() = False Then Return False

        cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_MODEL_COMPACTING)
        result = ds.Compact(strFileName)
        cApplicationStatusNotifier.EndProgress(Me.Core)

        If result = eDatasourceAccessType.Success Then
            bSucces = Me.LoadEcopathModel(strFileName, eLoadSourceType.API)
            If bSucces Then
                strMessage = My.Resources.STATUS_MODEL_COMPACT_SUCCESS
            Else
                strMessage = My.Resources.STATUS_MODEL_COMPACT_RELOADFAIL
            End If
        Else
            ' Report error
            Select Case result
                Case eDatasourceAccessType.Failed_OSUnsupported
                    strMessage = My.Resources.STATUS_MODEL_COMPACTING_OS
                Case eDatasourceAccessType.Failed_CannotSave
                    strMessage = My.Resources.STATUS_MODEL_COMPACTING_TEMPFILE
                Case eDatasourceAccessType.Failed_FileNotFound,
                     eDatasourceAccessType.Failed_Unknown
                    strMessage = My.Resources.STATUS_MODEL_COMPACTING_FAILED
                Case eDatasourceAccessType.Failed_ReadOnly
                    strMessage = My.Resources.STATUS_MODEL_ACCESS_READONLY
            End Select
            bSucces = False
        End If

        If (bSucces) Then
            Me.SendMessage(strMessage, eMessageImportance.Information, eCoreComponentType.DataSource)
        Else
            Me.SendMessage(strMessage, eMessageImportance.Critical, eCoreComponentType.DataSource)
        End If

        Return bSucces

    End Function

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Test an incoming model link, and convert it to a local Ecopath 6 model if
    ''' possible.
    ''' </summary>
    ''' <param name="strFileName">File name of the Access database to convert. If a
    ''' conversion is necessary this parameter will receive the file name of the
    ''' converted file.</param>
    ''' <returns>A <see cref="cEwEDatabase.eCompatibilityTypes"/> value</returns>
    ''' <remarks>
    ''' This logic will need to change entirely. A database 
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Private Function CovertToEwE6(ByRef strFileName As String) As cEwEDatabase.eCompatibilityTypes

        Dim comp As cEwEDatabase.eCompatibilityTypes = cEwEDatabase.eCompatibilityTypes.Unknown
        Dim access As eDatasourceAccessType = eDatasourceAccessType.Failed_Unknown
        Dim links As New cWebLinks(Me.Core)

        ' Get compatibility
        comp = cDataSourceFactory.GetCompatibility(strFileName, access)

        ' Has access problems?
        If (access <> eDatasourceAccessType.Opened) Then
            ' #Yes: report access error
            Me.ReportFileAccessError(access, strFileName)
            Return cEwEDatabase.eCompatibilityTypes.Unknown
        End If

        ' Able to access ok; assess compatibility next
        Select Case comp

            Case cEwEDatabase.eCompatibilityTypes.TooOld
                Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_ERROR_IMPORT_EWE5_TOO_OLD, links.GetURL(cWebLinks.eLinkType.Home)),
                               strHyperlink:=links.GetURL(cWebLinks.eLinkType.Home))

            Case cEwEDatabase.eCompatibilityTypes.Importable

                If (File.Exists(strFileName)) Then
                    Me.AddModelMRU(strFileName)
                End If

                Dim dlg As New Import.dlgImportDatabase(Me.UIContext, strFileName)
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    ' Update file name
                    strFileName = dlg.ImportedFileName
                    comp = cEwEDatabase.eCompatibilityTypes.EwE6
                End If

            Case cEwEDatabase.eCompatibilityTypes.EwE6
                ' Yippee

            Case cEwEDatabase.eCompatibilityTypes.Future
                If Me.AskFeedback(cStringUtils.Localize(My.Resources.PROMPT_ERROR_IMPORT_EWE6_TOO_NEW, links.GetURL(cWebLinks.eLinkType.Home)),
                                  eMessageImportance.Question,
                                  eCoreComponentType.DataSource,
                                  eMessageReplyStyle.YES_NO,
                                  strHyperlink:=links.GetURL(cWebLinks.eLinkType.Home)) = eMessageReply.NO Then
                    comp = cEwEDatabase.eCompatibilityTypes.Unknown
                End If

            Case cEwEDatabase.eCompatibilityTypes.Unknown
                Me.SendMessage(My.Resources.PROMPT_ERROR_IMPORT_INVALIDDB)

            Case Else
                ' Unsupported enum value?!
                Debug.Assert(False)
                comp = cEwEDatabase.eCompatibilityTypes.Unknown

        End Select

        Return comp

    End Function

    Private Sub ReportFileAccessError(ByVal atResult As eDatasourceAccessType, ByVal strFileName As String)

        Dim strMessage As String = ""
        Dim strHyperlink As String = ""

        Select Case atResult
            Case eDatasourceAccessType.Failed_ReadOnly
                strMessage = cStringUtils.Localize(My.Resources.STATUS_MODEL_ACCESS_READONLY, strFileName)
            Case eDatasourceAccessType.Failed_OSUnsupported
                strMessage = cStringUtils.Localize(My.Resources.STATUS_MODEL_ACCESS_OS, strFileName)
                Dim link As New cWebLinks(Me.Core)
                strHyperlink = link.GetURL(cWebLinks.eLinkType.Access2010)
            Case eDatasourceAccessType.Failed_FileNotFound
                strMessage = cStringUtils.Localize(My.Resources.STATUS_MODEL_ACCESS_404, strFileName)
            Case eDatasourceAccessType.Failed_CannotSave
                strMessage = cStringUtils.Localize(My.Resources.STATUS_MODEL_SAVE_404, strFileName)
            Case Else
                strMessage = cStringUtils.Localize(My.Resources.STATUS_MODEL_ACCESS_FAILED, strFileName)
        End Select

        Me.SendMessage(strMessage, eMessageImportance.Warning, eCoreComponentType.DataSource, strHyperlink:=strHyperlink)

    End Sub

#End Region ' Database utils

#Region " UI updates "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, updates the state of controls reflecting the current model. 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub UpdateModelControls()

        Dim strCaption As String = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DOUBLE, My.Resources.GENERIC_CAPTION, cCore.Version.ToString())
        Dim model As cEwEModel = Me.Core.EwEModel
        Dim bIsReadOnly As Boolean = False

        Me.m_tsModel.Path = Me.SelectedFileName
        If Me.Core.StateMonitor.HasEcopathLoaded Then
            bIsReadOnly = Me.Core.DataSource.IsReadOnly
            strCaption = cStringUtils.Localize(SharedResources.GENERIC_LABEL_CAPTION, strCaption, model.Name)
            If (bIsReadOnly) Then
                ' Explicitly show read-only status in the caption text
                ' ToDo: Globalize this
                strCaption = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, strCaption, "read only")
            End If
        End If

        Me.Text = strCaption
        Me.m_tslbReadOnly.Visible = bIsReadOnly

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, populate the content of the scenario drop-down controls
    ''' with lists of scenarios available in the current model. 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub PopulateScenarioDropdowns()

        Dim tsmi As ToolStripMenuItem = Nothing
        Dim fmt As New cTimeSeriesDatasetIntervalTypeFormatter()

        Me.ClearScenarioDropdowns()

        ' Has a model loaded?
        If Me.Core.StateMonitor.HasEcopathLoaded() Then

            ' #Yes: add scenario lists

            ' VERIFY_JS: Should scenarios be sorted in the most recent load order, or is that going to be highly confusing?

            ' List available Ecosim scenarios.
            For i As Integer = 1 To Me.Core.nEcosimScenarios
                tsmi = New ToolStripMenuItem()
                tsmi.Text = Me.Core.EcosimScenarios(i).Name
                tsmi.Tag = Me.Core.EcosimScenarios(i)
                tsmi.Checked = (Me.Core.ActiveEcosimScenarioIndex = i)
                AddHandler tsmi.Click, AddressOf OnLoadEcosimScenarioOrDataset
                Me.m_tsbEcosim.DropDownItems.Add(tsmi)
            Next

            ' List available Ecosim time series datasets
            For i As Integer = 1 To Me.Core.nTimeSeriesDatasets

                ' Is first dataset?
                If (i = 1) Then
                    ' #Yes: add a separator
                    Me.m_tsbEcosim.DropDownItems.Add(New ToolStripSeparator())
                End If

                tsmi = New ToolStripMenuItem()
                tsmi.Text = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED,
                                          Me.Core.TimeSeriesDataset(i).Name,
                                          fmt.GetDescriptor(Me.Core.TimeSeriesDataset(i).TimeSeriesInterval).ToLower())
                tsmi.Tag = Me.Core.TimeSeriesDataset(i)
                tsmi.Checked = (Me.Core.ActiveTimeSeriesDatasetIndex = i)

                AddHandler tsmi.Click, AddressOf OnLoadEcosimScenarioOrDataset
                Me.m_tsbEcosim.DropDownItems.Add(tsmi)

            Next i

            ' List available Ecospace scenarios
            For i As Integer = 1 To Me.Core.nEcospaceScenarios
                tsmi = New ToolStripMenuItem()
                tsmi.Text = Me.Core.EcospaceScenarios(i).Name
                tsmi.Tag = Me.Core.EcospaceScenarios(i)
                tsmi.Checked = (Me.Core.ActiveEcospaceScenarioIndex = i)
                AddHandler tsmi.Click, AddressOf OnLoadEcospaceScenario
                Me.m_tsbEcospace.DropDownItems.Add(tsmi)
            Next

            '' List available spatial temporal datasets
            'Dim man As cSpatialDataSetManager = Me.Core.SpatialDataConnectionManager.DatasetManager
            'For i As Integer = 1 To man.ConfigFiles.Count

            '    ' Is first dataset?
            '    If (i = 1) Then
            '        ' #Yes: add a separator
            '        Me.m_tsbEcospace.DropDownItems.Add(New ToolStripSeparator())
            '        tsmi = New ToolStripMenuItem()
            '        tsmi.Text = SharedResources.GENERIC_VALUE_DEFAULT
            '        tsmi.Tag = ""
            '        tsmi.Checked = (cSpatialDataSetManager.DefaultConfigFile = man.CurrentConfigFile)

            '        AddHandler tsmi.Click, AddressOf OnLoadEcospaceScenario
            '        Me.m_tsbEcospace.DropDownItems.Add(tsmi)
            '    End If

            '    strItem = CStr(man.ConfigFiles(i - 1))
            '    tsmi = New ToolStripMenuItem()
            '    tsmi.Text = strItem
            '    tsmi.Tag = strItem
            '    tsmi.Checked = (strItem = man.CurrentConfigFile)

            '    AddHandler tsmi.Click, AddressOf OnLoadEcospaceScenario
            '    Me.m_tsbEcospace.DropDownItems.Add(tsmi)

            'Next i

            ' List available Ecotracer scenarios
            For i As Integer = 1 To Me.Core.nEcotracerScenarios
                tsmi = New ToolStripMenuItem()
                tsmi.Text = Me.Core.EcotracerScenarios(i).Name
                tsmi.Tag = Me.Core.EcotracerScenarios(i)
                tsmi.Checked = (Me.Core.ActiveEcotracerScenarioIndex = i)
                AddHandler tsmi.Click, AddressOf OnLoadEcotracerScenario
                Me.m_tsbEcotracer.DropDownItems.Add(tsmi)
            Next

        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, clear the content of the scenario drop-down controls. 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub ClearScenarioDropdowns()

        Dim tsi As ToolStripItem = Nothing

        ' Properly release sim menu items
        For Each tsi In Me.m_tsbEcosim.DropDownItems
            If Not (TypeOf tsi Is ToolStripSeparator) Then
                RemoveHandler tsi.Click, AddressOf OnLoadEcosimScenarioOrDataset
            End If
        Next
        Me.m_tsbEcosim.DropDownItems.Clear()

        ' Properly release space menu items
        For Each tsi In Me.m_tsbEcospace.DropDownItems
            If Not (TypeOf tsi Is ToolStripSeparator) Then
                RemoveHandler tsi.Click, AddressOf OnLoadEcospaceScenario
            End If
        Next
        Me.m_tsbEcospace.DropDownItems.Clear()

        ' Properly release tracer menu items
        For Each tsi In Me.m_tsbEcotracer.DropDownItems
            RemoveHandler tsi.Click, AddressOf OnLoadEcotracerScenario
        Next
        Me.m_tsbEcotracer.DropDownItems.Clear()

    End Sub

#End Region ' UI updates

#Region " Settings "

    Private Sub SaveMainFormSettings()

        If (Me.UIContext IsNot Nothing) Then
            Me.UIContext.FormSettings.Store(Me, False)
            Me.m_styleguideupdater.Save()
            My.Settings.FormSettings = Me.UIContext.FormSettings.Setting
        End If
        Me.SaveSettings()

    End Sub

    Private Sub SaveSettings()

        Dim man As cSpatialDataSetManager = Me.Core.SpatialDataConnectionManager.DatasetManager

        If (man IsNot Nothing) Then
            My.Settings.SpatialTempConfigurations = man.ConfigFiles
            My.Settings.SpatialTemporalConfigFile = man.CurrentConfigFile
        End If

        Dim pm As cPluginManager = Me.Core.PluginManager
        If (pm IsNot Nothing) Then
            My.Settings.DisabledPlugins = pm.DisabledPlugins
        End If

        My.Settings.Save()

    End Sub

#End Region ' Settings

#Region " MRU "

#Region " Models "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a EwE DB name to the top of the MRU list.
    ''' </summary>
    ''' <param name="strFileName">Name of the file to add.</param>
    ''' -----------------------------------------------------------------------
    Private Sub AddModelMRU(ByVal strFileName As String)

        Dim alMDBmru As ArrayList = My.Settings.MdbRecentlyUsedList

        If (alMDBmru Is Nothing) Then Return

        ' Insert at head
        alMDBmru.Insert(0, strFileName)
        ' Remove any occurrences further down the list
        Me.RemoveModelMRU(strFileName, 1)

        ' Update system settings
        My.Settings.MdbRecentlyUsedList = alMDBmru
        Me.SaveSettings()

        ' Update UI
        Me.PopulateModelMRUDropdown()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Remove a file name from the MRU list, if possible.
    ''' </summary>
    ''' <param name="strFileName">Name of the file to remove.</param>
    ''' <param name="iStartPos">Index in the MRU list to start searching for
    ''' the item to remove. If not provided, the search will start at the 
    ''' beginning of the list.</param>
    ''' -----------------------------------------------------------------------
    Private Sub RemoveModelMRU(ByVal strFileName As String,
                               Optional ByVal iStartPos As Integer = 0)

        Dim alMDBmru As ArrayList = My.Settings.MdbRecentlyUsedList

        If (alMDBmru Is Nothing) Then Return

        ' Remove all occurrences from the list
        While iStartPos < alMDBmru.Count - 1
            If (TypeOf alMDBmru(iStartPos) Is String) Then
                ' Get entry
                Dim strEntry As String = CStr(alMDBmru(iStartPos))
                ' Is same file?
                If (String.Compare(strEntry, strFileName, True) = 0) Then
                    ' #Yes: remove 
                    alMDBmru.RemoveAt(iStartPos)
                    iStartPos -= 1
                End If
            End If
            iStartPos += 1
        End While
        My.Settings.MdbRecentlyUsedList = alMDBmru

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Show the list of MRU items in the menu structure.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub PopulateModelMRUDropdown()

        Dim alMRU As ArrayList = My.Settings.MdbRecentlyUsedList
        Dim iNumItems As Integer = Math.Min(alMRU.Count - 1, My.Settings.MdbRecentlyUsedCount)
        Dim item As ToolStripMenuItem = Nothing
        Dim bHasMRU As Boolean = False

        ' Clear MRU list
        Me.ClearModelMRUDropdowns()

        If (alMRU IsNot Nothing) Then
            bHasMRU = (alMRU.Count > 1)
        End If

        ' No recently accessed files yet?
        If (bHasMRU = False) Then
            ' Always have 'None' item
            item = New ToolStripMenuItem()
            item.Text = SharedResources.GENERIC_VALUE_NONE
            item.Enabled = False
            Me.m_tsmiFileRecent.DropDownItems.Add(item)
            Return
        End If

        For i As Integer = 0 To iNumItems - 1

            Dim str As String() = CStr(alMRU.Item(i)).Split(New Char() {";"c})

            item = New ToolStripMenuItem()
            item.Text = cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, i + 1, str(0))
            item.Tag = str(0)

            'Add event handler to invoke the model
            AddHandler item.Click, AddressOf OnModelMRUItemClicked

            Me.m_tsmiFileRecent.DropDownItems.Add(item)

            item = New ToolStripMenuItem()
            item.Text = str(0)
            item.Tag = str(0)
            item.Checked = (String.Compare(str(0), Me.SelectedFileName, True) = 0)

            'Add event handler to invoke the model
            AddHandler item.Click, AddressOf OnModelMRUItemClicked

            Me.m_tsbEcopath.DropDownItems.Add(item)
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Clear the list of MRU items and attached event handlers.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub ClearModelMRUDropdowns()

        Dim item As ToolStripMenuItem = Nothing

        For Each item In Me.m_tsmiFileRecent.DropDownItems
            If (item.Tag IsNot Nothing) Then
                ' Remove dangling event handler
                RemoveHandler item.Click, AddressOf OnModelMRUItemClicked
            End If
        Next
        ' Eradicate menu items
        Me.m_tsmiFileRecent.DropDownItems.Clear()


        For Each item In Me.m_tsbEcopath.DropDownItems
            If (item.Tag IsNot Nothing) Then
                ' Remove dangling event handler
                RemoveHandler item.Click, AddressOf OnModelMRUItemClicked
            End If
        Next
        Me.m_tsbEcopath.DropDownItems.Clear()

    End Sub

#End Region ' Models

#End Region ' MRU

#Region " Content navigation "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Create a form or dock panel for a given type.
    ''' </summary>
    ''' <param name="strNavLink">Navigation descriptor that created the form.</param>
    ''' <param name="t"><see cref="Type">Type</see> of the form to create.</param>
    ''' <returns>A <see cref="Form">Form</see>-derived instance, or Nothing if the
    ''' form could not be created.
    ''' </returns>
    ''' ---------------------------------------------------------------------------
    Private Function LoadFormFromType(ByVal strNavLink As String,
                                      ByVal t As Type,
                                      ByVal state As eCoreExecutionState) As Form

        Dim classObject As Object
        Dim frmNew As Form = Nothing
        Dim strCaption As String = ""

        If t Is Nothing Then Return Nothing

        Try
            classObject = Activator.CreateInstance(t)

            If TypeOf classObject Is DockContent Then
                ' Is dock content
                frmNew = DirectCast(classObject, DockContent)
            ElseIf TypeOf classObject Is EwEGrid Then
                ' Is a grid
                Dim grid As EwEGrid = DirectCast(classObject, EwEGrid)
                ' Fill the form with griddibits
                grid.Dock = DockStyle.Fill
                frmNew = New frmEwEGrid(grid)
                ' Use grid text as form caption
                frmNew.Text = grid.Text
            ElseIf TypeOf classObject Is Form Then
                ' Is a generic form
                frmNew = DirectCast(classObject, Form)
                frmNew.Text = strNavLink
            End If

            If TypeOf frmNew Is frmEwE Then
                ' Provide form with state
                DirectCast(frmNew, frmEwE).CoreExecutionState = state
            End If

            If (TypeOf (frmNew) Is IUIElement) Then
                ' Configure new object with UI context
                DirectCast(frmNew, IUIElement).UIContext = Me.UIContext
            End If

            ' Fix form caption
            strCaption = frmNew.Text
            ' Use a default if necessary
            If String.IsNullOrEmpty(strCaption) Then strCaption = strNavLink
            ' Stick caption back into the form
            frmNew.Text = strCaption
            If (TypeOf frmNew Is DockContent) Then
                Dim cnt As DockContent = DirectCast(frmNew, DockContent)
                ' Use caption also for tab text
                cnt.TabText = strCaption
            End If

            ' Store nav link
            frmNew.Tag = strNavLink

            ' Set form icon based on core state
            Select Case state
                Case eCoreExecutionState.Idle
                    frmNew.Icon = My.Resources.Ecopath0
                Case eCoreExecutionState.EcopathLoaded, eCoreExecutionState.EcopathCompleted, eCoreExecutionState.EcopathRunning
                    frmNew.Icon = My.Resources.Ecopath0
                Case eCoreExecutionState.EcosimLoaded, eCoreExecutionState.EcosimRunning, eCoreExecutionState.EcosimCompleted
                    frmNew.Icon = My.Resources.Ecosim
                Case eCoreExecutionState.EcospaceLoaded, eCoreExecutionState.EcospaceRunning, eCoreExecutionState.EcospaceCompleted
                    frmNew.Icon = My.Resources.Ecospace
                Case eCoreExecutionState.EcotracerLoaded
                    frmNew.Icon = My.Resources.Ecotracer
            End Select

        Catch ex As Exception
            Debug.Assert(False, "Creation of Form was not successful.  Please contact help: '" & strNavLink & "' threw exception " & ex.ToString)
        End Try

        Return frmNew
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, tries to activate an opened dock panel or MDI child 
    ''' window.
    ''' </summary>
    ''' <param name="strNavLink">Navigation descriptor to find the panel with.</param>
    ''' <returns>True if an existing panel was found.</returns>
    ''' -----------------------------------------------------------------------
    Private Function ActivateForm(ByVal strNavLink As String) As Boolean

        Dim bFound As Boolean = False

        ' Dock settings, loop through current opened 
        For Each cnt As DockContent In m_DockPanel.Contents

            If (TypeOf cnt.Tag Is String) Then
                bFound = String.Compare(CStr(cnt.Tag), strNavLink, True) = 0
            End If

            If Not bFound Then
                bFound = (String.Compare(cnt.Text, strNavLink, True) = 0)
            End If

            If bFound Then
                ' JS 08aug07: work-around for bug 133 (http://www.ecopath.org/developers/bugtracker/view.php?id=133)
                ' Source:   Weifen Luo dock content xml section for "Document" state panel is improperly written or missing
                ' Effect:   Forms that are supposed to be docked in that panel are constructed with Unknown dock properties
                '           but are not docked into any panel. Upon Activation, this logic restores damaged dock styles to
                '           reveal forms affected by this bug.
                ' Solution: Fix imcomplete XML issues in the dock panel engine.
                '           Hahaha!
                With cnt
                    .IsHidden = False
                    If .DockState = DockState.Unknown Then .DockState = DockState.Document
                    If .VisibleState = DockState.Unknown Then .VisibleState = DockState.Document
                    If .WindowState = FormWindowState.Minimized Then .WindowState = FormWindowState.Normal
                    .BringToFront()
                    .Focus()
                End With

                Return True
            End If
        Next
        ' Failed to find an existing panel with this tab text.
        Return False
    End Function

    ''' <summary>Flag to prevent looped navigation chaos.</summary>
    Private m_bNavigating As Boolean = False
    Private m_strLastActiveContent As String = ""

    Private Sub UpdateSelectedNode(ByVal strNodeName As String,
                                   Optional ByVal bAllowDefault As Boolean = False)

        If Me.m_bNavigating Then Return

        ' Default switching?
        If String.IsNullOrEmpty(strNodeName) And bAllowDefault Then
            ' #Yes: can reactivate current node?
            If Me.ActivateForm(Me.m_strLastActiveContent) Then Return
        End If

        Me.m_bNavigating = True

        ' Remember new page
        Me.m_strLastActiveContent = strNodeName
        ' Kick nav panel
        DirectCast(Me.Panel(cPANEL_NAV), frmNavigationPanel).SelectedNodeName(bAllowDefault) = strNodeName

        Me.m_bNavigating = False

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Private method to close all open child forms of the parent form.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub CloseAllDocuments()

        Dim lForms As New List(Of Form)
        Dim bIsReserved As Boolean = False

        ' Make temp list of all documents that may be closed. This cannot
        ' be performed in a for..ech loop because that affects the iterator
        ' used in the loop.
        For Each f As DockContent In Me.m_DockPanel.Contents

            If TypeOf (f) Is frmEwEDockContent Then
                ' Keep system panels open
                bIsReserved = (DirectCast(f, frmEwEDockContent).PanelType = frmEwEDockContent.ePanelType.SystemPanel)
            Else
                bIsReserved = False
            End If

            If Not bIsReserved Then
                lForms.Add(f)
            End If
        Next

        ' Now close the forms
        For Each f As Form In lForms
            f.Close()
        Next
        lForms = Nothing

        Me.UpdateSelectedNode("", False)
        Me.UIContext.Help.Clear()

    End Sub

#End Region ' Content navigation

#Region " Ecopath "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Open Ecopath model from a given location.
    ''' </summary>
    ''' <param name="strFileName">Location of the model to open.</param>
    ''' <param name="loadsource">Flag indicating where the load request came from.</param>
    ''' <remarks>This code is designed for strFileName to indicate a path. It should 
    ''' be possible to indicate a database as well. One day...</remarks>
    ''' ---------------------------------------------------------------------------
    Private Function LoadEcopathModel(ByVal strFileName As String,
                                      ByVal loadsource As eLoadSourceType) As Boolean

        Dim ds As IEwEDataSource = Nothing
        Dim atResult As eDatasourceAccessType = eDatasourceAccessType.Failed_Unknown
        Dim bReadOnly As Boolean = False

        Select Case cDataSourceFactory.GetSupportedType(strFileName)

            Case eDataSourceTypes.Access2003, eDataSourceTypes.Access2007,
                 eDataSourceTypes.EII, eDataSourceTypes.EIIXML

                ' Check if target file exists at all before affecting anything
                If Not File.Exists(strFileName) Then

                    ' Handle failure
                    Select Case loadsource

                        Case eLoadSourceType.MRU
                            If Me.AskFeedback(cStringUtils.Localize(My.Resources.PROMPT_MODELNOTFOUND_REMOVEMRU, strFileName),
                                              replystyle:=eMessageReplyStyle.YES_NO) = eMessageReply.YES Then
                                Me.RemoveModelMRU(strFileName)
                                Me.PopulateModelMRUDropdown()
                            End If

                        Case eLoadSourceType.User,
                             eLoadSourceType.CommandLine
                            ' Unable to load model, show generic error
                            Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_MODELNOTFOUND, strFileName),
                                           eMessageImportance.Warning, eCoreComponentType.DataSource)

                        Case eLoadSourceType.API
                            ' Do not provide user feedback in response to an API call

                    End Select

                    ' Update system settings
                    Me.SaveSettings()
                    Return False
                End If

            Case Else
                'NOP

        End Select

        ' Can close the current open model, if any?
        If Not CloseEcopathModel() Then
            ' #No: cannot close - abort
            Return False
        End If

        Select Case CovertToEwE6(strFileName)
            Case cEwEDatabase.eCompatibilityTypes.EwE6
                ' EwE6 database? OK
            Case cEwEDatabase.eCompatibilityTypes.Future
                ' Newer version: try to open
                bReadOnly = True
            Case Else
                ' No EwE6 database? abort
                Return False
        End Select

        ' Abort if no new file name given
        If String.IsNullOrEmpty(strFileName) Then Return True

        ' Create datasource on the selected file
        ds = cDataSourceFactory.Create(strFileName)

        If (ds Is Nothing) Then
            Select Case loadsource

                Case eLoadSourceType.MRU
                    ' Should not occur

                Case eLoadSourceType.User, eLoadSourceType.CommandLine
                    ' Unable to load model, show generic error
                    Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_INVALIDMODEL, strFileName),
                                   eMessageImportance.Warning, eCoreComponentType.DataSource)

                Case eLoadSourceType.API
                    ' Ok then

            End Select
            Return False
        End If

        ' Update MRU
        Me.AddModelMRU(strFileName)

        ' Open the datasource
        atResult = ds.Open(strFileName, Me.Core, eDataSourceTypes.NotSet, bReadOnly)

        If (atResult <> eDatasourceAccessType.Success) Then
            Me.ReportFileAccessError(atResult, strFileName)
            Return False
        End If

        ' Ok, now let's see if the core can work with this
        If Me.Core.LoadModel(ds) Then
            ' Set core paths
            Me.UpdateCorePaths(True)
            ' Remember last used model directory
            My.Settings.LastSelectedDirectory = Path.GetDirectoryName(strFileName)
            My.Settings.Save()
            Return True
        Else
            Dim msg As New cMessage(cStringUtils.Localize(My.Resources.GENERIC_ERROR_FILEOPEN, strFileName), eMessageType.Any, eCoreComponentType.Core, eMessageImportance.Critical)
            Me.Core.Messages.SendMessage(msg)
            ds.Close()
            Return False
        End If

    End Function

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Save model to a different datasource and switch to that new datasource. 
    ''' </summary>
    ''' <param name="strFileName">Full path + extension of the file to save.</param>
    ''' ---------------------------------------------------------------------------
    Private Function SaveEcopathModelAs(ByVal strFileName As String) As Boolean

        If (Me.Core.Save(strFileName)) Then
            Me.AddModelMRU(strFileName)
            Me.UpdateModelControls()
            Return True
        End If
        Return False
    End Function

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Create a new Ecopath model at a requested location.
    ''' </summary>
    ''' <param name="strFileName">The name of the file to create.</param>
    ''' <param name="strModelName">The name of the model to create.</param>
    ''' <param name="format">The file format to create.</param>
    ''' <returns>An Ecopath database, if successful.</returns>
    ''' <remarks>
    ''' Note that this will NOT load the new model! For this, 
    ''' <see cref="LoadEcopathModel"/> will need to be called.
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Friend Function CreateEcopathModel(ByVal strFileName As String,
                                        ByVal strModelName As String,
                                        ByVal format As eDataSourceTypes) As cEwEDatabase

        Dim db As cEwEDatabase = Nothing
        Dim atResult As eDatasourceAccessType = eDatasourceAccessType.Failed_Unknown
        Dim strPrompt As String = ""
        Dim importance As eMessageImportance = eMessageImportance.Warning

        Select Case format
            Case eDataSourceTypes.Access2003, eDataSourceTypes.Access2007
                If File.Exists(strFileName) Then
                    Dim fmsg As New cFeedbackMessage(cStringUtils.Localize(My.Resources.GENERIC_PROMPT_OVERWRITEFILE, strFileName),
                                                     eCoreComponentType.DataSource, eMessageType.DataValidation,
                                                     eMessageImportance.Question, eMessageReplyStyle.YES_NO)
                    fmsg.Reply = eMessageReply.NO
                    Me.Core.Messages.SendMessage(fmsg)
                    If fmsg.Reply = eMessageReply.NO Then Return Nothing
                End If
                db = New cEwEAccessDatabase()
                atResult = db.Create(strFileName, strModelName, True, format, Me.Core.DefaultAuthor)

            Case eDataSourceTypes.EII
                atResult = eDatasourceAccessType.Failed_DeprecatedOperation

            Case eDataSourceTypes.NotSet
                atResult = eDatasourceAccessType.Failed_UnknownType
        End Select

        ' Provide status feedback
        Select Case atResult

            Case eDatasourceAccessType.Success, eDatasourceAccessType.Opened
                strPrompt = cStringUtils.Localize(My.Resources.PROMPT_MODELCREATED, strFileName)
                importance = eMessageImportance.Information

            Case eDatasourceAccessType.Failed_CannotSave
                strPrompt = cStringUtils.Localize(My.Resources.PROMPT_INVALIDTARGETPATH, strFileName)
                importance = eMessageImportance.Critical

                ' Should not occur
                'Case eDatasourceAccessType.Failed_ReadOnly 

            Case eDatasourceAccessType.Failed_OSUnsupported
                strPrompt = My.Resources.PROMPT_DRIVERERROR
                importance = eMessageImportance.Critical

            Case eDatasourceAccessType.Failed_UnknownType
                strPrompt = My.Resources.PROMPT_INVALIDFILE
                importance = eMessageImportance.Critical

            Case eDatasourceAccessType.Failed_DeprecatedOperation
                strPrompt = My.Resources.PROMPT_FILETYPEDEPRECATED
                importance = eMessageImportance.Critical

            Case eDatasourceAccessType.Failed_Unknown
                strPrompt = cStringUtils.Localize(My.Resources.PROMPT_CREATE_GENERICERROR, strFileName)
                importance = eMessageImportance.Critical

        End Select

        If Not String.IsNullOrEmpty(strPrompt) Then
            Me.SendMessage(strPrompt, importance, eCoreComponentType.DataSource)
        End If

        If importance = eMessageImportance.Critical Then
            db = Nothing
        End If

        Return db

    End Function

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Create a new Ecopath model at a requested location.
    ''' </summary>
    ''' <param name="strFileName">The name of the file to create.</param>
    ''' <param name="strModelName">The name of the model to create.</param>
    ''' <returns>An Ecopath database, if successful.</returns>
    ''' <remarks>
    ''' <para>Note that this will NOT load the new model! For this, 
    ''' <see cref="LoadEcopathModel"/> will need to be called.</para>
    ''' <para>This method distills the database type from the provided file name.</para>
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Friend Function CreateEcopathModel(ByVal strFileName As String,
                                        ByVal strModelName As String) As cEwEDatabase
        Return Me.CreateEcopathModel(strFileName,
                                     strModelName,
                                     cDataSourceFactory.GetSupportedType(strFileName))
    End Function

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Close the current open Ecopath Model
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Private Function CloseEcopathModel() As Boolean

        ' Save form settings
        Me.SaveMainFormSettings()

        If Not String.IsNullOrEmpty(Me.SelectedFileName) Then

            Me.m_cmdPropertySelection.Invoke()

            ' Not allowed to terminate core?
            If (Not Me.Core.CloseModel()) Then
                ' #Not allowed: abort
                Return False
            End If

            ' Close all open documents
            Me.CloseAllDocuments()
            Me.ClearScenarioDropdowns()

            Me.m_autosavemanager.GatherSettings()

            ' Reset components
            DirectCast(Me.Panel(cPANEL_NAV), frmNavigationPanel).Reset()
            DirectCast(Me.Panel(cPANEL_STATUS), frmStatusPanel).Reset()

            ' Clear the properties cache
            Me.UIContext.PropertyManager.Clear(eCoreComponentType.EcoPath)

            ' Clean up UI bits
            Me.UpdateModelControls()
            Me.ClearScenarioDropdowns()

            ' Take out the trash
            GC.Collect()

            ' Redraw everything immediately
            Me.Refresh()
        End If

        ' Report succes
        Return True

    End Function

#End Region ' Ecopath

#Region " Ecosim "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load or reload an Ecosim scenario.
    ''' </summary>
    ''' <param name="bTryReuse">Flag indicating whether current scenario should reused, not reloaded, if possible.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function LoadEcosimScenario(Optional ByVal bTryReuse As Boolean = False) As Boolean

        Dim dlg As EcosimScenarioDlg = Nothing
        Dim bSucces As Boolean = False
        Dim es As cEcoSimScenario = Nothing

        ' Try to obtain ecosim scenario to load

        ' Invoked from a command?
        If (Me.m_cmdLoadEcosimScenario.IsInvoking()) Then
            ' #Yes: try to obtain scenario from command
            es = DirectCast(Me.m_cmdLoadEcosimScenario.Tag, cEcoSimScenario)
            ' #No: Are we reloading and an active scenario is present
        ElseIf (bTryReuse = True) And (Me.Core.ActiveEcosimScenarioIndex >= 0) Then
            Return True
        ElseIf Me.Core.nEcosimScenarios = 1 Then
            ' Automatically load the only available scenario
            es = Me.Core.EcosimScenarios(1)
        End If

        ' No scenario found yet?
        If (es Is Nothing) Then
            ' #No scenario: invoke ecosim scenario selection dialog
            dlg = New EcosimScenarioDlg(Me.UIContext, EcosimScenarioDlg.eDialogModeType.LoadScenario)
            If (dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then

                Select Case dlg.Mode
                    Case EcosimScenarioDlg.eDialogModeType.CreateScenario
                        ' User wants to create a scenario instead
                        Return Me.CreateEcosimScenario(dlg.ScenarioName, dlg.ScenarioDescription, dlg.ScenarioAuthor, dlg.ScenarioContact)
                    Case EcosimScenarioDlg.eDialogModeType.LoadScenario
                        ' User wants to load a scenario
                        es = DirectCast(dlg.Scenario, cEcoSimScenario)
                    Case Else
                        Debug.Assert(False)
                End Select

            End If
        End If

        Return LoadEcosimScenario(es)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load an Ecosim scenario.
    ''' </summary>
    ''' <param name="es">The <see cref="cEcoSimScenario">Scenario</see> to load.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function LoadEcosimScenario(ByVal es As cEcoSimScenario) As Boolean

        Dim bSucces As Boolean = False

        If (es IsNot Nothing) Then
            ' #Yes: Load it
            cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOSIM_LOADING, es.Name))
            bSucces = Me.Core.LoadEcosimScenario(es)
            Me.m_autosavemanager.ApplySettingsAndEnsureDefaults()
            cApplicationStatusNotifier.EndProgress(Me.Core)
        End If
        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="strName"></param>
    ''' <param name="strDescription"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function CreateEcosimScenario(ByVal strName As String, ByVal strDescription As String, ByVal strAuthor As String, ByVal strContact As String) As Boolean

        Dim bSucces As Boolean = False

        cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOSIM_CREATING, strName))
        bSucces = Me.Core.NewEcosimScenario(strName, strDescription, strAuthor, strContact)
        cApplicationStatusNotifier.EndProgress(Me.Core)
        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Invoke the manage time series interface.
    ''' </summary>
    ''' <param name="mode"><see cref="dlgManageTimeSeries.eModeType">Mode</see>
    ''' specifying how to open the interface.</param>
    ''' -----------------------------------------------------------------------
    Private Sub ManageTimeSeries(ByVal mode As dlgManageTimeSeries.eModeType)

        Dim dlg As New dlgManageTimeSeries(Me.UIContext, mode)

        ' Hmm
        dlg.StartPosition = FormStartPosition.CenterParent
        dlg.ShowInTaskbar = False
        dlg.ShowDialog()

    End Sub

#End Region ' Ecosim

#Region " Ecospace "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load or reload an Ecospace scenario.
    ''' </summary>
    ''' <param name="bTryReuse">Flag indicating whether current scenario should reused, not reloaded, if possible.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function LoadEcospaceScenario(Optional ByVal bTryReuse As Boolean = False) As Boolean

        Dim dlg As dlgEcospaceScenario = Nothing
        Dim bSucces As Boolean = False
        Dim es As cEcospaceScenario = Nothing

        ' Try to obtain ecospace scenario to load

        ' Invoked from a command?
        If (Me.m_cmdLoadEcospaceScenario.IsInvoking()) Then
            ' #Yes: try to obtain scenario from command
            es = CType(Me.m_cmdLoadEcospaceScenario.Tag, cEcospaceScenario)
            ' #No: Are we reloading and an active scenario is present?
        ElseIf (bTryReuse = True) And (Me.Core.ActiveEcospaceScenarioIndex >= 0) Then
            Return True
        ElseIf (Me.Core.nEcospaceScenarios = 1) Then
            ' Automatically load the only available scenario
            es = Me.Core.EcospaceScenarios(1)
        End If

        ' No scenario found yet?
        If (es Is Nothing) Then
            ' #No scenario: invoke ecospace scenario selection dialog
            dlg = New dlgEcospaceScenario(Me.UIContext, dlgEcospaceScenario.eDialogModeType.LoadScenario)
            If (dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then

                Select Case dlg.Mode
                    Case dlgEcospaceScenario.eDialogModeType.CreateScenario
                        ' User wants to create a scenario instead
                        Return Me.CreateEcospaceScenario(dlg.ScenarioName, dlg.ScenarioDescription,
                                dlg.ScenarioAuthor, dlg.ScenarioContact,
                                10, 10, 0, 0, 0.5)
                    Case dlgEcospaceScenario.eDialogModeType.LoadScenario
                        ' User wants to load a scenario
                        es = DirectCast(dlg.Scenario, cEcospaceScenario)
                    Case Else
                        Debug.Assert(False)
                End Select

            End If
        End If

        Return Me.LoadEcospaceScenario(es)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="strName"></param>
    ''' <param name="strDescription"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function CreateEcospaceScenario(ByVal strName As String, ByVal strDescription As String,
            ByVal strAuthor As String, ByVal strContact As String,
            ByVal iNumRows As Integer, ByVal iNumCols As Integer,
            ByVal sLatTL As Single, ByVal sLonTL As Single, ByVal sCellSize As Single) As Boolean

        Dim bSucces As Boolean = False

        cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOSPACE_CREATING, strName))
        bSucces = Me.Core.NewEcospaceScenario(strName, strDescription,
            strAuthor, strContact, iNumRows, iNumCols, sLatTL, sLonTL, sCellSize)
        cApplicationStatusNotifier.EndProgress(Me.Core)
        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="es"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function LoadEcospaceScenario(ByVal es As cEcospaceScenario) As Boolean

        Dim bSucces As Boolean = False

        If (es IsNot Nothing) Then
            ' #Yes: Load it
            cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOSPACE_LOADING, es.Name))
            bSucces = Me.Core.LoadEcospaceScenario(es)
            Me.m_autosavemanager.ApplySettingsAndEnsureDefaults()
            cApplicationStatusNotifier.EndProgress(Me.Core)
        End If
        Return bSucces

    End Function

#End Region ' Ecospace

#Region " Ecotracer "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load or reload an Ecotracer scenario.
    ''' </summary>
    ''' <param name="bTryReuse">Flag indicating whether current scenario should reused, not reloaded, if possible.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function LoadEcotracerScenario(Optional ByVal bTryReuse As Boolean = False) As Boolean

        Dim dlg As dlgEcotracerScenario = Nothing
        Dim bSucces As Boolean = False
        Dim es As cEcotracerScenario = Nothing

        ' Prerequesite: Ecosim needs to be loaded
        Me.CoreController.LoadState(eCoreExecutionState.EcosimLoaded)
        ' Not successful? abort
        If Not Me.Core.StateMonitor.HasEcosimLoaded Then Return False

        ' Try to obtain ecotracer scenario to load

        ' Invoked from a command?
        If (Me.m_cmdLoadEcotracerScenario.IsInvoking()) Then
            ' #Yes: try to obtain scenario from command
            es = CType(Me.m_cmdLoadEcotracerScenario.Tag, cEcotracerScenario)
            ' #No: Are we reloading and an active scenario is present?
        ElseIf (bTryReuse = True) And (Me.Core.ActiveEcotracerScenarioIndex >= 0) Then
            Return True
        ElseIf (Me.Core.nEcotracerScenarios = 1) Then
            ' Automatically load the only available scenario
            es = Me.Core.EcotracerScenarios(1)
        End If

        ' No scenario found yet?
        If (es Is Nothing) Then
            ' #No scenario: invoke ecotracer scenario selection dialog
            dlg = New dlgEcotracerScenario(Me.UIContext, dlgEcotracerScenario.eDialogModeType.LoadScenario)
            If (dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then

                Select Case dlg.Mode
                    Case dlgEcotracerScenario.eDialogModeType.CreateScenario
                        ' User wants to create a scenario instead
                        Return Me.CreateEcotracerScenario(dlg.ScenarioName, dlg.ScenarioDescription, dlg.ScenarioAuthor, dlg.ScenarioContact)
                    Case dlgEcotracerScenario.eDialogModeType.LoadScenario
                        ' User wants to load a scenario
                        es = DirectCast(dlg.Scenario, cEcotracerScenario)
                    Case Else
                        Debug.Assert(False)
                End Select

            End If
        End If

        Return LoadEcotracerScenario(es)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="strName"></param>
    ''' <param name="strDescription"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function CreateEcotracerScenario(ByVal strName As String, ByVal strDescription As String, ByVal strAuthor As String, ByVal strContact As String) As Boolean

        Dim bSucces As Boolean = False

        cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOTRACER_CREATING, strName))
        bSucces = Me.Core.NewEcotracerScenario(strName, strDescription, strAuthor, strContact)
        cApplicationStatusNotifier.EndProgress(Me.Core)
        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="es"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function LoadEcotracerScenario(ByVal es As cEcotracerScenario) As Boolean

        Dim bSucces As Boolean = False

        If (es IsNot Nothing) Then
            ' #Yes: Load it
            cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOTRACER_LOADING, es.Name))
            bSucces = Me.Core.LoadEcotracerScenario(es)
            Me.m_autosavemanager.ApplySettingsAndEnsureDefaults()
            cApplicationStatusNotifier.EndProgress(Me.Core)
        End If
        Return bSucces

    End Function

#End Region ' Ecotracer

#Region " Command handlers "

#Region " Generic commands "

    Private Sub OnFileOpen(ByVal cmd As cCommand) Handles m_cmdFileOpen.OnInvoke

        Dim dlgLoad As OpenFileDialog = Nothing
        Dim foc As cFileOpenCommand = DirectCast(cmd, cFileOpenCommand)
        Dim strPath As String = foc.Directory

        dlgLoad = cEwEFileDialogHelper.OpenFileDialog(foc.Title, foc.FileName, foc.Filters, foc.FilterIndex, strPath, foc.AllowMultiple)

        foc.Result = dlgLoad.ShowDialog()

        If (foc.Result = System.Windows.Forms.DialogResult.OK) Then
            foc.FileName = dlgLoad.FileName
            foc.FileNames = dlgLoad.FileNames
            foc.FilterIndex = dlgLoad.FilterIndex

            If (foc.AllowMultiple = False) Then
                foc.Directory = Path.GetDirectoryName(dlgLoad.FileName)
            End If
        End If

    End Sub

    Private Sub OnFileSave(ByVal cmd As cCommand) Handles m_cmdFileSave.OnInvoke

        Dim dlgSave As SaveFileDialog = Nothing
        Dim fsc As cFileSaveCommand = DirectCast(cmd, cFileSaveCommand)
        Dim strPath As String = fsc.Directory

        dlgSave = cEwEFileDialogHelper.SaveFileDialog(fsc.Title, fsc.FileName, fsc.Filters, fsc.FilterIndex, strPath)

        fsc.Result = dlgSave.ShowDialog()

        If (fsc.Result = System.Windows.Forms.DialogResult.OK) Then
            fsc.FileName = dlgSave.FileName
            fsc.FilterIndex = dlgSave.FilterIndex
            fsc.Directory = Path.GetDirectoryName(fsc.FileName)
        End If

    End Sub

    Private Sub OnDirectoryOpen(ByVal cmd As cCommand) Handles m_cmdDirectoryOpen.OnInvoke

        ' JS 19Nov13: Restored old path if something went wrong
        Dim doc As cDirectoryOpenCommand = Me.m_cmdDirectoryOpen
        Dim strPath As String = doc.Directory

        Try
            Dim dlgLoad As FolderBrowserDialog = Nothing
            dlgLoad = cEwEFileDialogHelper.FolderBrowserDialog(doc.Prompt, strPath)
            doc.Result = dlgLoad.ShowDialog()
            strPath = dlgLoad.SelectedPath
        Catch ex As Exception
            cLog.Write(ex, "OnDirectoryOpen")
        End Try

        If (doc.Result = System.Windows.Forms.DialogResult.OK) Then
            doc.Directory = strPath
        End If

    End Sub

    Private Sub OnPickColor(ByVal cmd As cCommand) Handles m_cmdPickColor.OnInvoke

        Try
            Dim dlg As New ColorDialog()
            dlg.Color = Me.m_cmdPickColor.Color
            dlg.AllowFullOpen = True
            dlg.AnyColor = True

            If (My.Settings.ColorCustom IsNot Nothing) Then
                dlg.CustomColors = CType(My.Settings.ColorCustom.ToArray(GetType(Integer)), Integer())
            End If

            Me.m_cmdPickColor.Result = dlg.ShowDialog(Me)

            If (Me.m_cmdPickColor.Result = System.Windows.Forms.DialogResult.OK) Then
                Me.m_cmdPickColor.Color = dlg.Color
                Dim al As New ArrayList()
                al.AddRange(dlg.CustomColors)
                My.Settings.ColorCustom = al
            End If

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnOpenDocument(ByVal cmd As cCommand) Handles m_cmdNavigate.OnInvoke

        Dim nc As cNavigationCommand = Nothing
        Dim frm As Form = Nothing
        Dim strNavPageID As String = ""
        Dim strNavPageName As String = ""
        Dim strNavHelpURL As String = ""
        Dim tNavClassType As Type = Nothing
        Dim iNavCoreState As eCoreExecutionState = eCoreExecutionState.Idle

        ' Sanity checks
        If cmd Is Nothing Then Return
        If Not (TypeOf cmd Is cNavigationCommand) Then Return

        nc = DirectCast(cmd, cNavigationCommand)

        ' Preserve properties from Nav command, because the content of the nav command may change in response to actions in this method
        strNavPageID = nc.PageID
        strNavPageName = nc.PageName
        strNavHelpURL = nc.HelpURL
        tNavClassType = nc.ClassType
        iNavCoreState = nc.CoreExecutionState

        If strNavPageID = "ndScenario" Then
            m_coreController.LoadEcosimScenario()
            Return
        End If

        If strNavPageID = "ndEcospaceScenario" Then
            m_coreController.LoadEcospaceScenario()
            Return
        End If

        If strNavPageID = "ndEcotracerScenario" Then
            Me.CoreController.LoadEcotracerScenario()
            Return
        End If

        ' Check if core can be brought up to par
        If Me.CoreController.LoadState(iNavCoreState) Then
            ' Is form already loaded?
            If Not ActivateForm(strNavPageName) Then

                'cApplicationStatusNotifier.StartProgress(Me.Core)

                Try
                    ' Load instance of form for selected node
                    frm = Me.LoadFormFromType(strNavPageName, tNavClassType, iNavCoreState)
                    ' Was a form created?
                    If frm IsNot Nothing Then
                        ' #Yes
                        If frm.WindowState = FormWindowState.Minimized Then frm.WindowState = FormWindowState.Normal
                        ' Is this a dockable form? 
                        If (TypeOf frm Is DockContent) And (Me.m_DockPanel.DocumentStyle = DocumentStyle.DockingMdi) Then
                            ' #Yes: show the form in the dock panel
                            DirectCast(frm, DockContent).Show(Me.m_DockPanel, DockState.Document)
                        Else
                            ' #No: Just show the form
                            frm.MdiParent = Me
                            frm.Show()
                        End If
                        ' Switch help
                        Me.Help.HelpTopic(frm) = strNavHelpURL
                    End If
                Catch ex As Exception
                    ' Whoah!
                End Try

                'cApplicationStatusNotifier.EndProgress(Me.Core)

            End If
        End If

        ' JS Jan2408: Make sure the nav tree correctly reflects the current selected page.
        ' This is important if the navigation to the requested page failed, which can happen
        ' if the core controller is unable to bring the core to the requested state.
        Me.OnTabFocusChanged(Nothing, Nothing)

    End Sub

    ''' <summary>
    ''' Close the current active document.
    ''' </summary>
    Private Sub OnCloseDocument(ByVal cmd As cCommand) Handles m_cmdCloseDocument.OnInvoke
        ' Is the window docked?
        ' Check whether an active document exists; this will occur when all panels are already closed.
        If Me.m_DockPanel.ActiveDocument IsNot Nothing Then
            ' Close active doc
            Me.m_DockPanel.ActiveDocument.DockHandler.Close()
        End If

    End Sub

    ''' <summary>
    ''' Command handler; update the 'close document' command state
    ''' </summary>
    Private Sub OnUpdateCloseDocument(ByVal cmd As cCommand) Handles m_cmdCloseDocument.OnUpdate, m_cmdCloseAllForms.OnUpdate
        cmd.Enabled = False
        ' Is the window docked?
        cmd.Enabled = Me.m_DockPanel.ActiveDocument IsNot Nothing
    End Sub

    ''' <summary>
    ''' Command handler; closes all closable child forms.
    ''' </summary>
    Private Sub OnCloseAllForms(ByVal cmd As cCommand) Handles m_cmdCloseAllForms.OnInvoke
        ' Close all child forms of the parent
        Me.CloseAllDocuments()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, called when the MRU dropdown menu is about to open.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnMRUOpening(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsmiFileRecent.DropDownOpening
        Me.PopulateModelMRUDropdown()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, called when the MRU dropdown menu has closed.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnMRUClosed(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsmiFileRecent.DropDownClosed
        ' Ok, do NOT do this here; the dropdown is closed BEFORE a MRU invoke is called. Lovely!
        'Me.ResetMRU()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, called when the Exit menu item is selected.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnExit(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsmiFileExit.Click
        Me.Close()
    End Sub

#End Region ' Generic commands

#Region " File menu commands "

    ''' <summary>
    ''' Create new Ecopath model
    ''' </summary>
    Private Sub OnNewModel(ByVal cmd As cCommand) Handles m_cmdNewModel.OnInvoke

        Dim db As cEwEDatabase = Nothing
        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
        Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)

        cmdFS.Invoke(SharedResources.DEFAULT_NEWMODELNAME, SharedResources.FILEFILTER_MODEL_SAVE, 1)

        If (cmdFS.Result = System.Windows.Forms.DialogResult.OK) Then
            ' #Yes: able to create model at selected location?
            db = Me.CreateEcopathModel(cmdFS.FileName, Path.GetFileNameWithoutExtension(cmdFS.FileName))
            If db IsNot Nothing Then
                ' #Yes: Able to load model?
                Me.LoadEcopathModel(cmdFS.FileName, eLoadSourceType.User)
            End If
        End If

    End Sub

    ''' <summary>
    ''' Update new model command state
    ''' </summary>
    Private Sub OnUpdateNewModel(ByVal cmd As cCommand) Handles m_cmdNewModel.OnUpdate
        cmd.Enabled = True
    End Sub

    ''' <summary>
    ''' Open Ecopath model from a given location
    ''' </summary>
    Private Sub OnLoadModel(ByVal cmd As cCommand) Handles m_cmdLoadModel.OnInvoke

        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
        Dim cmdFO As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)
        Dim strFilter As String = SharedResources.FILEFILTER_MODEL_OPEN

        If String.IsNullOrWhiteSpace(cmdFO.Directory) Then
            cmdFO.Directory = My.Settings.LastSelectedDirectory
        End If

        If (cmd.Tag IsNot Nothing) Then
            cmdFO.Invoke(CStr(cmd.Tag), strFilter, 1)
        Else
            cmdFO.Invoke(strFilter, 1)
        End If

        If (cmdFO.Result = DialogResult.OK) Then

            ' Open the model
            cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_ECOPATH_LOADING)
            Me.LoadEcopathModel(cmdFO.FileName, eLoadSourceType.User)
            cApplicationStatusNotifier.EndProgress(Me.Core)

        End If

    End Sub

    ''' <summary>
    ''' Update Load Ecopath model command state
    ''' </summary>
    Private Sub OnUpdateLoadModel(ByVal cmd As cCommand) Handles m_cmdLoadModel.OnUpdate
        cmd.Enabled = Not Me.Core.StateMonitor.IsBusy
    End Sub

    ''' <summary>
    ''' Save the model
    ''' </summary>
    Private Sub OnSave(ByVal cmd As cCommand) Handles m_cmdSave.OnInvoke
        cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_MODEL_SAVING)
        Try
            Me.Core.Save()
            Me.SaveSettings()
        Catch ex As Exception
            ' Whoah!
        End Try
        cApplicationStatusNotifier.EndProgress(Me.Core)
    End Sub

    ''' <summary>
    ''' Update save model command state
    ''' </summary>
    Private Sub OnUpdateSave(ByVal cmd As cCommand) Handles m_cmdSave.OnUpdate
        cmd.Enabled = Me.Core.StateMonitor.IsModified And Not Me.Core.StateMonitor.IsBusy
    End Sub

    ''' <summary>
    ''' Save model under a different name
    ''' </summary>
    Private Sub OnSaveModelAs(ByVal cmd As cCommand) Handles m_cmdSaveModelAs.OnInvoke

        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
        Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)

        Dim strFileFilter As String = ""

        ' JS 27Jul08: Only able to save in current file format (save as between formats not supported by the core)
        Select Case cDataSourceFactory.GetSupportedType(Me.SelectedFileName)
            Case eDataSourceTypes.Access2003
                ' Only allow saving as MDB
                strFileFilter = SharedResources.FILEFILTER_SAVE_MDB
            Case eDataSourceTypes.Access2007
                ' Only allow saving as ACCDB
                strFileFilter = SharedResources.FILEFILTER_SAVE_ACCDB
            Case Else
                ' Not supported
                Debug.Assert(False, "Option should not have been available")
                Return
        End Select

        ' Special case: invoke save model command on last used model path
        If (String.IsNullOrWhiteSpace(cmdFS.Directory)) Then
            cmdFS.Directory = My.Settings.LastSelectedDirectory
        End If
        cmdFS.Invoke(SharedResources.DEFAULT_NEWMODELNAME, strFileFilter)

        If (cmdFS.Result = System.Windows.Forms.DialogResult.OK) Then

            ' Save the model
            cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_MODEL_SAVING)
            Try
                SaveEcopathModelAs(cmdFS.FileName)
            Catch ex As Exception

            End Try
            cApplicationStatusNotifier.EndProgress(Me.Core)

        End If

    End Sub

    ''' <summary>
    ''' Update save model command state
    ''' </summary>
    Private Sub OnUpdateSaveModelAs(ByVal cmd As cCommand) Handles m_cmdSaveModelAs.OnUpdate

        Dim bEnable As Boolean = Me.Core.StateMonitor.HasEcopathLoaded

        Select Case cDataSourceFactory.GetSupportedType(Me.SelectedFileName)
            Case eDataSourceTypes.Access2003, eDataSourceTypes.Access2007
                ' NOP
            Case Else
                ' Only allow save as when file was opened as MDB or ACCDB since the core does
                ' not support (yet: 27jul08) support saving from one file type to another)
                bEnable = False
        End Select
        ' Update command
        cmd.Enabled = bEnable

    End Sub

    ''' <summary>
    ''' Close the current open model
    ''' </summary>
    Private Sub OnCloseModel(ByVal cmd As cCommand) Handles m_cmdCloseModel.OnInvoke
        Me.CloseEcopathModel()
    End Sub

    ''' <summary>
    ''' Update close model command state
    ''' </summary>
    Private Sub OnUpdateCloseModel(ByVal cmd As cCommand) Handles m_cmdCloseModel.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Compact a model
    ''' </summary>
    Private Sub OnCompactModel(ByVal cmd As cCommand) Handles m_cmdCompactModel.OnInvoke
        Me.CompactModel()
    End Sub

    ''' <summary>
    ''' Update compact model command state
    ''' </summary>
    Private Sub OnUpdateCompactModel(ByVal cmd As cCommand) Handles m_cmdCompactModel.OnUpdate
        Dim ds As IEwEDataSource = Me.Core.DataSource
        If (ds Is Nothing) Then
            cmd.Enabled = False
        Else
            cmd.Enabled = (Me.Core.StateMonitor.HasEcopathLoaded) And ds.CanCompact(Me.SelectedFileName)
        End If
    End Sub

    ''' <summary>
    ''' Open the output file location
    ''' </summary>
    Private Sub OnOpenOutputLocation(ByVal cmd As cCommand) Handles m_cmdOpenOutput.OnInvoke
        Try
            Process.Start("explorer.exe", Me.Core.OutputPath)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnPrintInvoke(ByVal cmd As cCommand) Handles m_cmdPrint.OnInvoke

        Dim dlg As New PrintPreviewDialog()
        Dim cnt As IDockContent = Me.m_DockPanel.ActiveDocument

        If (TypeOf cnt Is frmEwE) Then
            Dim frm As frmEwE = DirectCast(cnt, frmEwE)
            dlg.Document = frm.BeginPrint
            dlg.ShowDialog()
            frm.EndPrint()
        End If

    End Sub

    Private Sub OnPrintEnable(ByVal cmd As cCommand) Handles m_cmdPrint.OnUpdate

        Dim cnt As IDockContent = Me.m_DockPanel.ActiveDocument
        Dim bEnable As Boolean = False

        If (cnt IsNot Nothing) Then
            bEnable = (TypeOf cnt Is frmEwE)
        End If

        cmd.Enabled = bEnable

    End Sub

    Private Sub OnEcobaseImportInvoke(ByVal cmd As cCommand) Handles m_cmdEcobaseImport.OnInvoke

        Dim strModel As String = ""

        If (String.IsNullOrWhiteSpace(strModel)) Then
            Dim frm As New dlgEcobaseImport(Me.UIContext)
            If (frm.ShowDialog() = DialogResult.OK) Then
                Dim model As EwECore.WebServices.Ecobase.cModelData = frm.SelectedModel
                strModel = "ewe-ecobase:" & model.EcobaseCode
            End If
        End If

        If (Not String.IsNullOrWhiteSpace(strModel)) Then
            Me.LoadEcopathModel(strModel, eLoadSourceType.User)
        End If

    End Sub

    Private Sub OnEcobaseImportEnable(ByVal cmd As cCommand) Handles m_cmdEcobaseImport.OnUpdate
        cmd.Enabled = Not Me.Core.StateMonitor.IsBusy
    End Sub

    Private Sub OnEcobaseExportInvoke(ByVal cmd As cCommand) _
        Handles m_cmdEcobaseExport.OnInvoke

        Try
            Me.m_coreController.LoadState(eCoreExecutionState.EcopathCompleted)

            ' Ecopath must run ok
            If (Not Me.Core.IsModelBalanced()) Then
                Me.SendMessage(My.Resources.ECOBASE_ERROR_BALANCE)
                Return
            End If

            ' All pending changes must be saved prior to this
            If (Not Me.Core.SaveChanges()) Then Return

            ' Export
            Dim dlg As New dlgEcobaseExport(Me.UIContext)
            dlg.ShowDialog(Me)

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnEcobaseExportEnable(ByVal cmd As cCommand) Handles m_cmdEcobaseExport.OnUpdate
        cmd.Enabled = Not Me.Core.StateMonitor.IsBusy And Me.Core.StateMonitor.HasEcopathLoaded
    End Sub

    Private Sub OnEIIXMLExportInvoke(ByVal cmd As cCommand) _
        Handles m_cmdEIIXMLExport.OnInvoke

        Dim ds As IEwEDataSource = Me.Core.DataSource
        If Not (TypeOf ds Is cDBDataSource) Then Return
        Dim dbds As cDBDataSource = DirectCast(ds, cDBDataSource)
        If Not (TypeOf dbds.Connection Is cEwEAccessDatabase) Then Return
        Dim db As cEwEAccessDatabase = DirectCast(dbds.Connection, cEwEAccessDatabase)
        Dim msg As cMessage = Nothing

        If Not Me.Core.SaveChanges(False) Then Return

        Try
            Dim strPath As String = Path.ChangeExtension(dbds.ToString, ".eiixml")
            ds = cDataSourceFactory.Create(eDataSourceTypes.EIIXML)
            If DirectCast(ds, cEIIXMLDataSource).SaveFromDB(db, strPath) Then
                msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_EXPORT_SUCCESS, strPath),
                                   eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
                msg.Hyperlink = Path.GetDirectoryName(strPath)
            Else
                msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_EXPORT_SUCCESS, strPath),
                   eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
            End If
            Me.Core.Messages.SendMessage(msg)
            ds.Close()

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnEIIXMLExportEnable(ByVal cmd As cCommand) Handles m_cmdEIIXMLExport.OnUpdate

        Dim bEnabled As Boolean = False

        If (Not Me.Core.StateMonitor.IsBusy) Then
            If (Me.Core.DataSource IsNot Nothing) Then
                If (TypeOf Me.Core.DataSource Is cDBDataSource) Then
                    Dim dbds As cDBDataSource = DirectCast(Me.Core.DataSource, cDBDataSource)
                    bEnabled = (TypeOf dbds.Connection Is cEwEAccessDatabase)
                End If
            End If
        End If

        cmd.Enabled = bEnabled

    End Sub

#End Region ' File commands

#Region " View commands "

    ''' <summary>
    ''' Command handler; toggles presentation mode
    ''' </summary>
    Private Sub OnViewPresentationMode(ByVal cmd As cCommand) Handles m_cmdViewPresentationMode.OnInvoke

        Me.m_presentationmode.Toggle()

    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the 
    ''' <see cref="m_cmdViewPresentationMode">View Presentation Mode command</see>.
    ''' </summary>
    Private Sub OnUpdateViewPresentationMode(ByVal cmd As cCommand) _
        Handles m_cmdViewPresentationMode.OnUpdate
        Me.m_cmdViewPresentationMode.Checked = Me.m_presentationmode.Active
    End Sub

    ''' <summary>
    ''' Command handler; toggles main statusbar visibility
    ''' </summary>
    Private Sub OnViewMainStatusbar(ByVal cmd As cCommand) Handles m_cmdViewStatusbar.OnInvoke
        Me.m_ssMain.Visible = Not cmd.Checked
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdViewStatusbar">View Statusbar command</see>.
    ''' </summary>
    Private Sub OnUpdateViewMainStatusbar(ByVal cmd As cCommand) Handles m_cmdViewStatusbar.OnUpdate
        cmd.Checked = Me.m_ssMain.Visible
    End Sub

    ''' <summary>
    ''' Command handler; toggles main menu visibility
    ''' </summary>
    Private Sub OnViewMenu(ByVal cmd As cCommand) Handles m_cmdViewMenu.OnInvoke
        Me.m_menuMain.Visible = Not cmd.Checked
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdViewMenu">View menu command</see>.
    ''' </summary>
    Private Sub OnUpdateViewMenu(ByVal cmd As cCommand) Handles m_cmdViewMenu.OnUpdate
        cmd.Checked = Me.m_menuMain.Visible
    End Sub

    ''' <summary>
    ''' Command handler; toggles auto save results
    ''' </summary>
    Private Sub OnAutosaveResults(ByVal cmd As cCommand) Handles m_cmdAutosaveResults.OnInvoke
        Me.m_cmdShowOptions.Invoke(eApplicationOptionTypes.Autosave)
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdAutosaveResults">Auto save results command</see>.
    ''' </summary>
    Private Sub OnUpdateAutosaveResults(ByVal cmd As cCommand) Handles m_cmdAutosaveResults.OnUpdate
        ' Check if any autosave option set
        Dim nAutoSaving As Integer = 0
        Dim nodes As eAutosaveTypes() = New eAutosaveTypes() {eAutosaveTypes.Ecosim, eAutosaveTypes.Ecospace}
        For Each setting As eAutosaveTypes In [Enum].GetValues(GetType(eAutosaveTypes))
            ' Exclude nodes
            If Me.Core.Autosave(setting) And Array.IndexOf(nodes, setting) = -1 Then
                nAutoSaving += 1
            End If
        Next
        If (Me.m_pluginManager IsNot Nothing) Then
            For Each pi As IAutoSavePlugin In Me.m_pluginManager.GetPlugins(GetType(IAutoSavePlugin))
                If pi.AutoSave Then nAutoSaving += 1
            Next
        End If
        cmd.Checked = (nAutoSaving > 0)
        ' cmd.Status = cStringUtils.Localize("{0} EwE component(s) are auto-saving results", nAutoSaving)
    End Sub

    ''' <summary>
    ''' Command handler; shows the start page.
    ''' </summary>
    Private Sub OnBrowseURI(ByVal cmd As cCommand) Handles m_cmdBrowseURI.OnInvoke

        Dim panel As frmStartPanel = DirectCast(Me.Panel(cPANEL_START), frmStartPanel)
        Dim bcmd As cBrowserCommand = DirectCast(cmd, cBrowserCommand)
        Dim strURL As String = bcmd.URL(New cWebLinks(Me.Core))

        ' Is a hyperlink?
        If cUriBuilder.IsValidURI(strURL) Or String.IsNullOrWhiteSpace(strURL) Then
            If (My.Settings.UseExternalBrowser) And Not String.IsNullOrWhiteSpace(strURL) Then
                Try
                    ' Fire off system default URL handling
                    System.Diagnostics.Process.Start(strURL)
                Catch ex As Exception
                    ' Failed to launch
                    Dim msg As New cMessage(cStringUtils.Localize(My.Resources.PROMPT_SHELL_FAILURE, ex.Message),
                                        eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
                    Me.Core.Messages.SendMessage(msg)
                End Try
            Else
                ' #Yes: extract hyperlink bit, and pass it to the desired browser
                If Not cmd.Checked Then
                    If panel.IsDisposed() Then
                        panel = New frmStartPanel(Me.UIContext)
                        Me.m_dtPanels(cPANEL_START) = panel
                    End If
                    If Not String.IsNullOrWhiteSpace(strURL) Then panel.URL = strURL
                    panel.Show(Me.m_DockPanel, DockState.Document)
                Else
                    If Not panel.IsDisposed Then
                        panel.Close()
                    End If
                End If
            End If
        ElseIf cStringUtils.BeginsWith(strURL, "command:", True) Then
            ' #No: Is command?
            Dim strCommand As String = strURL.Substring(8)
            cmd = Me.UIContext.CommandHandler.GetCommand(strCommand)
            ' Invoke command without any parameters
            If (cmd IsNot Nothing) Then
                cmd.Invoke()
            End If
        ElseIf cStringUtils.BeginsWith(strURL, "ewe-ecobase:", True) Then
            ' #No: Is ecobase link?
            Me.LoadEcopathModel(strURL, eLoadSourceType.User)
        Else
            ' #No: presume we're talking files here. Let the OS deal with it
            Try
                ' JS 10Jan15: Do not even use explorer; just use default protocol handlers
                'Process.Start("explorer.exe", strURL)
                System.Diagnostics.Process.Start(strURL)
            Catch ex As Exception
                ' Failed to launch
                Dim msg As New cMessage(cStringUtils.Localize(My.Resources.PROMPT_SHELL_FAILURE, ex.Message),
                                        eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
                Me.Core.Messages.SendMessage(msg)
            End Try
        End If

    End Sub

    ''' <summary>
    ''' Command update handler; manages the <see cref="m_cmdBrowseURI"/> state.
    ''' </summary>
    Private Sub OnUpdateBrowseURI(ByVal cmd As cCommand) Handles m_cmdBrowseURI.OnUpdate
        Dim p As frmEwEDockContent = Me.Panel(cPANEL_START)
        cmd.Checked = p.Visible
    End Sub

    ''' <summary>
    ''' Command handler; shows the navigation panel.
    ''' </summary>
    Private Sub OnViewNavPane(ByVal cmd As cCommand) Handles m_cmdViewNavPane.OnInvoke
        If cmd.Checked Then
            Me.Panel(cPANEL_NAV).DockState = DockState.Hidden
        Else
            Me.Panel(cPANEL_NAV).Show(m_DockPanel, DockState.DockLeft)
        End If
    End Sub

    ''' <summary>
    ''' Command update handler; manages the <see cref="m_cmdViewNavPane">View Navigation Panel command</see> state.
    ''' </summary>
    Private Sub OnUpdateViewNavPane(ByVal cmd As cCommand) Handles m_cmdViewNavPane.OnUpdate
        cmd.Checked = (Me.Panel(cPANEL_NAV).DockState <> DockState.Hidden)
    End Sub

    ''' <summary>
    ''' Show the remark pane
    ''' </summary>
    Private Sub OnViewRemarkPane(ByVal cmd As cCommand) Handles m_cmdViewRemarkPane.OnInvoke
        If cmd.Checked Then
            Me.Panel(cPANEL_REMARKS).DockState = DockState.Hidden
        Else
            Me.Panel(cPANEL_REMARKS).Show(m_DockPanel, DockState.DockBottomAutoHide)
        End If
    End Sub

    Private Sub OnUpdateViewRemarkPane(ByVal cmd As cCommand) Handles m_cmdViewRemarkPane.OnUpdate
        cmd.Checked = (Me.Panel(cPANEL_REMARKS).DockState <> DockState.Hidden)
    End Sub

    ''' <summary>
    ''' Show the status panel
    ''' </summary>
    Private Sub OnViewStatusPane(ByVal cmd As cCommand) Handles m_cmdViewStatusPane.OnInvoke
        If cmd.Checked Then
            Me.Panel(cPANEL_STATUS).DockState = DockState.Hidden
        Else
            Me.Panel(cPANEL_STATUS).Show(m_DockPanel, DockState.DockBottomAutoHide)
        End If
    End Sub

    Private Sub OnUpdateViewStatusPane(ByVal cmd As cCommand) Handles m_cmdViewStatusPane.OnUpdate
        cmd.Checked = (Me.Panel(cPANEL_STATUS).DockState <> DockState.Hidden)
    End Sub

    ''' <summary>
    ''' Show the button bar
    ''' </summary>
    Private Sub OnViewModelBar(ByVal cmd As cCommand) Handles m_cmdViewModelBar.OnInvoke
        Me.m_tsModel.Visible = Not cmd.Checked
    End Sub

    Private Sub OnUpdateViewModelBar(ByVal cmd As cCommand) Handles m_cmdViewModelBar.OnUpdate
        cmd.Checked = Me.m_tsModel.Visible
    End Sub

#End Region ' View commands

#Region " Tools commands "

    Private Sub OnShowOptions(ByVal cmd As cCommand) Handles m_cmdShowOptions.OnInvoke
        Try
            Dim dlgOptions As New dlgOptions(Me.UIContext, Me.m_cmdShowOptions.Option)
            cmd.UserHandled = (dlgOptions.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK)
            Me.SaveSettings()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnShowTools(ByVal cmd As cCommand) Handles m_cmdShowTools.OnInvoke
        Try
            Dim strPath As String = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Tools")
            Me.m_cmdBrowseURI.Invoke(strPath)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnEditRefMap(ByVal cmd As cCommand) Handles m_cmdEditReferenceMap.OnInvoke
        Me.m_cmdShowOptions.Invoke(eApplicationOptionTypes.ReferenceMaps)
    End Sub

#End Region ' Tools commands

#Region " Help commands "

    ''' <summary>
    ''' Command handler; invokes the About... dialog.
    ''' </summary>
    Private Sub OnShowAboutDialog(ByVal cmd As cCommand) Handles m_cmdHelpAbout.OnInvoke
        Dim dlgAbout As New frmAboutEwE(Me.UIContext)
        Me.Help.HelpTopic(dlgAbout) = ""
        dlgAbout.ShowDialog(Me)
    End Sub

    Private Sub OnHelpTOC(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsmiHelpContents.Click
        Me.Help.ShowHelp(HelpNavigator.TableOfContents)
    End Sub

    Private Sub OnHelpIndex(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsmiHelpIndex.Click
        Me.Help.ShowHelp(HelpNavigator.KeywordIndex)
    End Sub

    Private Sub OnHelpSearch(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_tsmiHelpSearch.Click
        Me.Help.ShowHelp(HelpNavigator.Find)
    End Sub

    Private Sub OnReportBug(cmd As cCommand) Handles m_cmdHelpReportIssue.OnInvoke
        Try
            Dim strReport As String = cBugReporter.BugReport(My.Resources.GENERIC_CAPTION, "ewedevteam@gmail.com", Me.m_pluginManager)
            Me.m_cmdBrowseURI.Invoke(strReport)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnRequestSourceCodeAccess(cmd As cCommand) Handles m_cmdHelpRequestCodeAccess.OnInvoke
        Try
            Me.m_cmdBrowseURI.Invoke("mailto:ewedevteam@gmail.com?subject=Request source code access")
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnProvideFeedback(cmd As cCommand) Handles m_cmdHelpFeedback.OnInvoke
        ' Survey monkey URL
        Me.m_cmdBrowseURI.Invoke(cWebLinks.eLinkType.Feedback)
    End Sub

    Private Sub m_tsmiHelpViewMainSite_Click(sender As System.Object, e As System.EventArgs) Handles m_tsmiHelpViewMainSite.Click
        Me.m_cmdBrowseURI.Invoke(cWebLinks.eLinkType.Home)
    End Sub

    Private Sub m_tsmiHelpViewFacebook_Click(sender As System.Object, e As System.EventArgs) Handles m_tsmiHelpViewFacebook.Click
        Me.m_cmdBrowseURI.Invoke(cWebLinks.eLinkType.Facebook)
    End Sub

    Private Sub m_tsmiHelpViewReports_Click(sender As System.Object, e As System.EventArgs) Handles m_tsmiHelpViewReports.Click
        Me.m_cmdBrowseURI.Invoke(cWebLinks.eLinkType.Trac)
    End Sub

    Private Sub m_tsmiViewLog_Click(sender As System.Object, e As System.EventArgs) Handles m_tsmiViewLog.Click
        Me.m_cmdBrowseURI.Invoke(cLog.LogFile)
    End Sub

#End Region ' Main Menu - Help

#Region " Ecopath commands "

    ''' <summary>
    ''' Command handler; invokes the edit groups interface
    ''' </summary>
    Private Sub OnEditGroups(ByVal cmd As cCommand) Handles m_cmdEditGroups.OnInvoke
        Dim dlg As New dlgDefineGroups(Me.UIContext, DirectCast(cmd.Tag, cEcoPathGroupInput))
        Me.Help.HelpTopic(dlg) = "Edit groups.htm"
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdEditGroups">Edit Groups command</see>.
    ''' </summary>
    Private Sub OnUpdateEditGroups(ByVal cmd As cCommand) Handles m_cmdEditGroups.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded() And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; invokes the edit multi stanza interface
    ''' </summary>
    Private Sub OnEditMultiStanza(ByVal cmd As cCommand) Handles m_cmdEditMultiStanza.OnInvoke

        ' Test if all stanza groups have at least one life stage
        Dim vars As New List(Of cVariableStatus)

        For i As Integer = 0 To Me.Core.nStanzas - 1
            If (Me.Core.StanzaGroups(i).nLifeStages = 0) Then
                vars.Add(New cVariableStatus(eStatusFlags.MissingParameter, cStringUtils.Localize(My.Resources.PROMPT_STANZA_MISSING_LIFESTAGES_DETAIL, Me.Core.StanzaGroups(i).Name),
                                             eVarNameFlags.NotSet, eDataTypes.Stanza, eCoreComponentType.Core, 0))
            End If
        Next

        If (vars.Count > 0) Then
            If Me.AskFeedback(My.Resources.PROMPT_STANZA_MISSING_LIFESTAGES,
                              eMessageImportance.Warning, eCoreComponentType.Core,
                              eMessageReplyStyle.YES_NO, vars:=vars.ToArray()) = eMessageReply.YES Then
                Me.m_cmdEditGroups.Invoke()
            End If
            Return
        End If

        Dim dlg As New EditMultiStanza(Me.UIContext)
        Me.Help.HelpTopic(dlg) = "Edit multi stanza.htm"
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdEditMultiStanza">Edit Multi-stanza command</see>.
    ''' </summary>
    Private Sub OnUpdateMultiStanza(ByVal cmd As cCommand) Handles m_cmdEditMultiStanza.OnUpdate
        ' MultiStanza can be edited when ecopath has loaded and the core has more than one stanza group
        cmd.Enabled = (Me.Core.StateMonitor.HasEcopathLoaded() = True) And
                      (Me.Core.nStanzas > 0) And
                      (Not Me.Core.StateMonitor.IsBusy)
    End Sub

    ''' <summary>
    ''' Command handler; invokes the edit fleets interface
    ''' </summary>
    Private Sub OnEditFleets(ByVal cmd As cCommand) Handles m_cmdEditFleets.OnInvoke
        Try
            Dim dlg As New EditFleets(Me.UIContext, DirectCast(cmd.Tag, cEcopathFleetInput))
            Me.Help.HelpTopic(dlg) = "Edit fleets.htm"
            dlg.ShowDialog(Me)
        Catch ex As Exception
            ' Woops
            Debug.Assert(False)
        End Try
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdEditFleets">Edit Fleets command</see>.
    ''' </summary>
    Private Sub OnUpdateEditFleets(ByVal cmd As cCommand) _
        Handles m_cmdEditFleets.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded() And Not m.IsBusy
    End Sub

    Private Sub OnEditPedigreeLevels(ByVal cmd As cCommand) _
        Handles m_cmdEditPedigree.OnInvoke
        Try
            Dim dlg As New dlgEditPedigree(Me.UIContext, DirectCast(cmd, cEditPedigreeCommand).Variable)
            dlg.ShowDialog(Me)
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6::OnEditPedigreeLevels")
        End Try
    End Sub

    Private Sub OnUpdateEditPedigreeLevels(ByVal cmd As cCommand) _
        Handles m_cmdEditPedigree.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded() And Not m.IsBusy
    End Sub

    Private Sub OnEditTaxonomy(ByVal cmd As cCommand) _
        Handles m_cmdEditTaxonomy.OnInvoke
        Dim dlg As New dlgDefineTaxonomy(Me.UIContext)
        dlg.ShowDialog(Me)
    End Sub

    Private Sub OnUpdateEditTaxonomy(ByVal cmd As cCommand) _
        Handles m_cmdEditTaxonomy.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded() And Not m.IsBusy
    End Sub

    Private Sub OnDisplayShowHideItems(ByVal cmd As cCommand) _
        Handles m_cmdShowHideItems.OnInvoke
        Dim dlg As New dlgShowHideItems(Me.UIContext, Me.m_cmdShowHideItems.GroupDisplayOptions)
        dlg.ShowDialog()
        cmd.Checked = Me.UIContext.StyleGuide.HasHiddenItems()
    End Sub

    Private Sub OnUpdateShowHideItems(ByVal cmd As cCommand) _
        Handles m_cmdShowHideItems.OnUpdate
        cmd.Enabled = Me.Core.StateMonitor.HasEcopathLoaded()
    End Sub

#End Region ' Main Menu - File

#Region " Ecosim commands "

    ''' <summary>
    ''' Command handler; creates a new Ecosim scenario
    ''' </summary>
    Private Sub OnNewEcosimScenario(ByVal cmd As cCommand) Handles m_cmdNewEcosimScenario.OnInvoke

        Dim dlg As New EcosimScenarioDlg(Me.UIContext, EcosimScenarioDlg.eDialogModeType.CreateScenario)

        If dlg.ShowDialog = System.Windows.Forms.DialogResult.OK Then

            Select Case dlg.Mode
                Case EcosimScenarioDlg.eDialogModeType.CreateScenario
                    Me.CreateEcosimScenario(dlg.ScenarioName, dlg.ScenarioDescription, dlg.ScenarioAuthor, dlg.ScenarioContact)
                Case EcosimScenarioDlg.eDialogModeType.LoadScenario
                    Me.LoadEcosimScenario(DirectCast(dlg.Scenario, cEcoSimScenario))
                Case Else
                    Debug.Assert(False)
            End Select

        End If

    End Sub

    ''' <summary>
    ''' Command update handler; takes care of enabling and disabling the
    ''' <see cref="m_cmdNewEcosimScenario">New Ecosim Scenario</see> command.
    ''' </summary>
    Private Sub OnUpdateNewEcosimScenario(ByVal cmd As cCommand) Handles m_cmdNewEcosimScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; loads a new Ecosim scenario
    ''' </summary>
    Private Sub OnLoadEcosimScenario(ByVal cmd As cCommand) Handles m_cmdLoadEcosimScenario.OnInvoke
        Me.CoreController.LoadEcosimScenario()
    End Sub

    ''' <summary>
    ''' Command update handler; takes care of enabling and disabling the 
    ''' <see cref="m_cmdLoadEcosimScenario">Load Ecosim Scenario</see> command.
    ''' </summary>
    Private Sub OnUpdateLoadEcosimScenario(ByVal cmd As cCommand) Handles m_cmdLoadEcosimScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; closes the current Ecosim scenario
    ''' </summary>
    Private Sub OnCloseEcosimScenario(ByVal cmd As cCommand) Handles m_cmdCloseEcosimScenario.OnInvoke
        Me.m_autosavemanager.GatherSettings()
        Me.Core.CloseEcosimScenario()
    End Sub

    ''' <summary>
    ''' Command update handler; takes care of enabling and disabling the 
    ''' <see cref="m_cmdCloseEcosimScenario">Close Ecosim Scenario</see> command.
    ''' </summary>
    Private Sub OnUpdateCloseEcosimScenario(ByVal cmd As cCommand) Handles m_cmdCloseEcosimScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcosimLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; saves an Ecosim scenario to a new name
    ''' </summary>
    Private Sub OnSaveEcosimScenarioAs(ByVal cmd As cCommand) Handles m_cmdSaveEcosimScenarioAs.OnInvoke

        Dim dlg As New EcosimScenarioDlg(Me.UIContext, EcosimScenarioDlg.eDialogModeType.SaveScenario,
                Me.Core.EcosimScenarios(Me.Core.ActiveEcosimScenarioIndex))

        If dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            ' Overwriting?
            If dlg.Scenario IsNot Nothing Then
                ' #Yes: prompt for overwrite confirmation
                Dim fmsg As New cFeedbackMessage(cStringUtils.Localize(My.Resources.SCENARIO_CONFIRMOVERWRITE_PROMPT, dlg.ScenarioName), eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
                Me.Core.Messages.SendMessage(fmsg)

                If (fmsg.Reply = eMessageReply.YES) Then
                    ' #Overwrite
                    cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOSIM_SAVING, dlg.ScenarioName))
                    Try
                        Me.Core.SaveEcosimScenarioAs(dlg.ScenarioName, dlg.ScenarioDescription)
                    Catch ex As Exception
                        cLog.Write(ex, "frmEwE6::SaveEcosimScenarioAs")
                    End Try
                    cApplicationStatusNotifier.EndProgress(Me.Core)

                End If
                ' User does not want to overwrite? Abort
                Return
            End If

            ' Add scenario under new name
            cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOSIM_CREATING, dlg.ScenarioName))
            Try
                Me.Core.SaveEcosimScenarioAs(dlg.ScenarioName, dlg.ScenarioDescription)
            Catch ex As Exception

            End Try
            cApplicationStatusNotifier.EndProgress(Me.Core)

        End If

    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the 'save ecosim scenario as' command
    ''' </summary>
    Private Sub OnUpdateSaveEcosimScenarioAs(ByVal cmd As cCommand) _
        Handles m_cmdSaveEcosimScenarioAs.OnUpdate
        cmd.Enabled = Me.Core.StateMonitor.HasEcosimLoaded
    End Sub

    ''' <summary>
    ''' Command handler; deletes an Ecosim scenario 
    ''' </summary>
    Private Sub OnInvokeDeleteEcosimScenario(ByVal cmd As cCommand) _
         Handles m_cmdDeleteEcosimScenario.OnInvoke
        Dim dlg As New EcosimScenarioDlg(Me.UIContext, EcosimScenarioDlg.eDialogModeType.DeleteScenario)
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the 'delete ecosim scenario' command
    ''' </summary>
    Private Sub OnUpdateDeleteEcosimScenario(ByVal cmd As cCommand) _
           Handles m_cmdDeleteEcosimScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = (m.HasEcopathLoaded) And
                      (Me.Core.nEcosimScenarios > 0) And
                      (Not m.IsBusy)
    End Sub

    ''' <summary>
    ''' Command handler; invokes the import time series dialog.
    ''' </summary>
    Private Sub m_cmdImportTimeSeries_OnInvoke(ByVal cmd As cCommand) _
        Handles m_cmdImportTimeSeries.OnInvoke
        Me.ManageTimeSeries(dlgManageTimeSeries.eModeType.Import)
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdImportTimeSeries">Import TimeSeries command</see>.
    ''' </summary>
    Private Sub m_cmdImportTimeSeries_OnUpdate(ByVal cmd As cCommand) Handles m_cmdImportTimeSeries.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcosimLoaded() And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; exports the currently loaded time series dataset to a CSV file.
    ''' </summary>
    Private Sub m_cmdExportTimeSeries_OnInvoke(ByVal cmd As cCommand) _
        Handles m_cmdExportTimeSeries.OnInvoke

        Dim sfd As New SaveFileDialog()
        Dim tsw As New cTimeSeriesCSVWriter(Me.Core)

        sfd.Filter = SharedResources.FILEFILTER_CSV
        sfd.FileName = tsw.DefaultFileName
        sfd.CheckFileExists = False
        sfd.CheckPathExists = True
        sfd.OverwritePrompt = True

        If (sfd.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK) Then
            tsw.Write(sfd.FileName, ",", ".")
        End If

    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdExportTimeSeries">Export TimeSeries command</see>.
    ''' </summary>
    Private Sub m_cmdExportTimeSeries_OnUpdate(ByVal cmd As cCommand) _
        Handles m_cmdExportTimeSeries.OnUpdate
        cmd.Enabled = (Me.Core.ActiveTimeSeriesDatasetIndex > -1)
    End Sub

    ''' <summary>
    ''' Command handler; invokes the apply time series dialog.
    ''' </summary>
    Private Sub m_cmdWeightTimeSeries_OnInvoke(ByVal cmd As cCommand) Handles m_cmdWeightTimeSeries.OnInvoke
        Me.ManageTimeSeries(dlgManageTimeSeries.eModeType.Weight)
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdWeightTimeSeries">Apply TimeSeries command</see>.
    ''' </summary>
    Private Sub m_cmdWeightTimeSeries_OnUpdate(ByVal cmd As cCommand) Handles m_cmdWeightTimeSeries.OnUpdate
        ' JS 23sept08: dialog will switch to load mode if no ts present
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcosimLoaded() And
                      Not m.IsBusy ' And Me.Core.HasTimeSeries()
    End Sub

    ''' <summary>
    ''' Command handler; invokes the load time series dialog, or loads a time
    ''' series dataset if this dataset is provided as a tag to the command.
    ''' </summary>
    Private Sub m_cmdEcosimLoadTimeSeries_OnInvoke(ByVal cmd As cCommand) _
        Handles m_cmdEcosimLoadTimeSeries.OnInvoke

        If Not Me.m_coreController.LoadState(eCoreExecutionState.EcosimLoaded) Then Return

        If (Me.m_cmdEcosimLoadTimeSeries.Tag Is Nothing) Then
            Me.ManageTimeSeries(dlgManageTimeSeries.eModeType.Load)
        ElseIf (TypeOf Me.m_cmdEcosimLoadTimeSeries.Tag Is cTimeSeriesDataset) Then
            Dim ds As cTimeSeriesDataset = DirectCast(Me.m_cmdEcosimLoadTimeSeries.Tag, cTimeSeriesDataset)
            cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_TIMESERIES_LOADING, ds.Name))
            Me.Core.LoadTimeSeries(ds, True)
            cApplicationStatusNotifier.EndProgress(Me.Core)
        End If

    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdEcosimLoadTimeSeries">Load TimeSeries command</see>.
    ''' </summary>
    Private Sub m_cmdEcosimLoadTimeSeries_OnUpdate(ByVal cmd As cCommand) _
        Handles m_cmdEcosimLoadTimeSeries.OnUpdate

        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded() And Not m.IsBusy

    End Sub

    Private Sub OnExportEcosimResultsToCSV(ByVal cmd As cCommand) _
        Handles m_cmdExportEcosimResultsToCSV.OnInvoke

        Dim writer As EwECore.Ecosim.cEcosimResultWriter = Nothing
        Dim strPath As String = ""

        Me.m_cmdDirectoryOpen.Invoke(Me.Core.DefaultOutputPath(eAutosaveTypes.Ecosim))
        If (Me.m_cmdDirectoryOpen.Result <> System.Windows.Forms.DialogResult.OK) Then Return
        strPath = Me.m_cmdDirectoryOpen.Directory

        writer = New EwECore.Ecosim.cEcosimResultWriter(Me.UIContext.Core)
        writer.WriteResults(strPath, DirectCast(cmd, cEcosimSaveDataCommand).Results)
        writer = Nothing

    End Sub

    Private Sub OnExportEcosimResultsToCSVUpdate(ByVal cmd As cCommand) _
        Handles m_cmdExportEcosimResultsToCSV.OnUpdate
        cmd.Enabled = Me.Core.StateMonitor.HasEcosimRan
    End Sub

    Private Sub OnEstimateVsInvoke(ByVal cmd As cCommand) _
        Handles m_cmdEstimateVs.OnInvoke
        Dim dlg As New dlgEstimateVs(Me.UIContext)
        dlg.ShowDialog(Me)
    End Sub

    Private Sub OnEstimateVsUpdate(ByVal cmd As cCommand) _
        Handles m_cmdEstimateVs.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcosimLoaded() And Not m.IsBusy
    End Sub

    Private Sub OnTrimEcosimShapesInvoke(cmd As cCommand) _
        Handles m_cmdEcosimTrimShapes.OnInvoke

        Dim fmsg As New cFeedbackMessage(My.Resources.PROMPT_TRIM_SHAPES,
                                         eCoreComponentType.ShapesManager, eMessageType.Any, eMessageImportance.Question,
                                         eMessageReplyStyle.YES_NO)
        Me.Core.Messages.SendMessage(fmsg)

        If fmsg.Reply = eMessageReply.YES Then
            Me.Core.TrimUnusedShapeData()
        End If

    End Sub

    Private Sub OnTrimEcosimShapesUpdate(cmd As cCommand) _
        Handles m_cmdEcosimTrimShapes.OnUpdate
        cmd.Enabled = Me.Core.HasUnusedShapeData And Not Me.Core.StateMonitor.IsBusy
    End Sub

    Private Sub OnEcosimChangeShapeInvoke(cmd As cCommand) _
        Handles m_cmdEcosimChangeShape.OnInvoke

        Try
            Dim dlg As New dlgChangeShape(Me.UIContext, DirectCast(cmd.Tag, cForcingFunction))
            dlg.ShowDialog(Me.UIContext.FormMain)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnEcosimChangeShapeUpdate(cmd As cCommand) _
        Handles m_cmdEcosimChangeShape.OnUpdate
        cmd.Enabled = Me.Core.StateMonitor.HasEcosimLoaded And Not Me.Core.StateMonitor.IsBusy
    End Sub

#End Region ' Ecosim commands

#Region " Ecospace commands "

    Private Sub OnNewEcospaceScenario(ByVal cmd As cCommand) _
        Handles m_cmdNewEcospaceScenario.OnInvoke

        Dim dlg As New dlgEcospaceScenario(Me.UIContext, dlgEcospaceScenario.eDialogModeType.CreateScenario)

        If dlg.ShowDialog = System.Windows.Forms.DialogResult.OK Then

            Select Case dlg.Mode
                Case dlgEcospaceScenario.eDialogModeType.CreateScenario
                    Me.CreateEcospaceScenario(dlg.ScenarioName, dlg.ScenarioDescription,
                            dlg.ScenarioAuthor, dlg.ScenarioContact,
                            10, 10, 0, 0, 0.5)
                Case dlgEcospaceScenario.eDialogModeType.LoadScenario
                    Me.LoadEcospaceScenario(DirectCast(dlg.Scenario, cEcospaceScenario))
                Case dlgEcospaceScenario.eDialogModeType.DeleteScenario
                    Return
                Case Else
                    Debug.Assert(False)
            End Select

        End If

    End Sub

    Private Sub OnUpdateNewEcospaceScenario(ByVal cmd As cCommand) _
        Handles m_cmdNewEcospaceScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcosimLoaded And Not m.IsBusy
    End Sub

    Private Sub OnLoadEcospaceScenario(ByVal cmd As cCommand) _
        Handles m_cmdLoadEcospaceScenario.OnInvoke
        Me.CoreController.LoadEcospaceScenario()
    End Sub

    Private Sub OnUpdateLoadEcospaceScenario(ByVal cmd As cCommand) _
        Handles m_cmdLoadEcospaceScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded And Not m.IsBusy
    End Sub

    Private Sub OnCloseEcospaceScenario(ByVal cmd As cCommand) _
        Handles m_cmdCloseEcospaceScenario.OnInvoke
        Me.m_autosavemanager.GatherSettings()
        Me.Core.CloseEcospaceScenario()
    End Sub

    Private Sub OnUpdateCloseEcospaceScenario(ByVal cmd As cCommand) _
        Handles m_cmdCloseEcospaceScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; saves the current active Ecospace scenario under a new name.
    ''' </summary>
    Private Sub OnSaveEcospaceScenarioAs(ByVal cmd As cCommand) _
        Handles m_cmdSaveEcospaceScenarioAS.OnInvoke

        Dim dlg As New dlgEcospaceScenario(Me.UIContext,
                                           dlgEcospaceScenario.eDialogModeType.SaveScenario,
                                           Me.Core.EcospaceScenarios(Me.Core.ActiveEcospaceScenarioIndex))
        Dim scenarioTarget As cEcospaceScenario = Nothing

        If dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            ' Has valid name?
            If Not String.IsNullOrEmpty(dlg.ScenarioName) Then
                ' #Cool. Now check if this will overwrite a scenario with the same name (case insensitive)
                scenarioTarget = Nothing
                For iScenario As Integer = 1 To Me.Core.nEcospaceScenarios
                    If (String.Compare(Me.Core.EcospaceScenarios(iScenario).Name, dlg.ScenarioName, True) = 0) Then
                        scenarioTarget = Me.Core.EcospaceScenarios(iScenario)
                    End If
                Next

                ' About to overwrite?
                If (scenarioTarget IsNot Nothing) Then
                    ' #Yes: prompt for overwrite confirmation
                    Dim fmsg As New cFeedbackMessage(cStringUtils.Localize(My.Resources.SCENARIO_CONFIRMOVERWRITE_PROMPT, dlg.ScenarioName), eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
                    Me.Core.Messages.SendMessage(fmsg)

                    If (fmsg.Reply = eMessageReply.YES) Then

                        ' #Overwrite
                        cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOSPACE_SAVING, dlg.ScenarioName))
                        Try
                            Me.Core.SaveEcospaceScenarioAs(dlg.ScenarioName, dlg.ScenarioDescription)
                        Catch ex As Exception
                            cLog.Write(ex, "frmEwE6::SaveEcopaceScenarioAs")
                        End Try
                        cApplicationStatusNotifier.EndProgress(Me.Core)

                    End If
                    ' User does not want to overwrite? Abort
                    Return
                End If

                ' Add scenario
                cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOSPACE_CREATING, dlg.ScenarioName))
                Try
                    Me.Core.SaveEcospaceScenarioAs(dlg.ScenarioName, dlg.ScenarioDescription)
                Catch ex As Exception

                End Try
                cApplicationStatusNotifier.EndProgress(Me.Core)

            End If
        End If

    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the 
    ''' <see cref="m_cmdSaveEcospaceScenarioAs">Save Ecospace Scenario As</see> command.
    ''' </summary>
    Private Sub OnUpdateSaveEcospaceScenarioAs(ByVal cmd As cCommand) Handles m_cmdSaveEcospaceScenarioAS.OnUpdate
        cmd.Enabled = Me.Core.StateMonitor.HasEcospaceLoaded
    End Sub

    ''' <summary>
    ''' Command handler; deletes an Ecosim scenario 
    ''' </summary>
    Private Sub OnInvokeDeleteEcospaceScenario(ByVal cmd As cCommand) _
         Handles m_cmdDeleteEcospaceScenario.OnInvoke
        Dim dlg As New dlgEcospaceScenario(Me.UIContext, dlgScenario.eDialogModeType.DeleteScenario)
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the 'delete ecospace scenario' command
    ''' </summary>
    Private Sub OnUpdateDeleteEcospaceScenario(ByVal cmd As cCommand) _
           Handles m_cmdDeleteEcospaceScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded And
                      Not m.IsBusy And
                      Me.Core.nEcospaceScenarios > 0
    End Sub

    ''' <summary>
    ''' Command handler; loads an Ecospace spatial temporal data set
    ''' </summary>
    Private Sub m_cmdEcospaceLoadTimeSeries_OnInvoke(ByVal cmd As cCommand) _
        Handles m_cmdEcospaceLoadTimeSeries.OnInvoke

        If Not Me.m_coreController.LoadState(eCoreExecutionState.EcospaceLoaded) Then Return

        If (Me.m_cmdEcospaceLoadTimeSeries.Tag IsNot Nothing) Then
            Dim strFile As String = CType(Me.m_cmdEcospaceLoadTimeSeries.Tag, String)
            Dim man As cSpatialDataSetManager = Me.Core.SpatialDataConnectionManager.DatasetManager
            man.Load(strFile)
        End If

    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the <see cref="m_cmdEcosimLoadTimeSeries">Load TimeSeries command</see>.
    ''' </summary>
    Private Sub m_cmdEcospaceLoadTimeSeries_OnUpdate(ByVal cmd As cCommand) _
        Handles m_cmdEcospaceLoadTimeSeries.OnUpdate

        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded() And Not m.IsBusy

    End Sub

    ''' <summary>
    ''' Command handler; invokes the Ecospace edit basemap dialog.
    ''' </summary>
    Private Sub OnEditEcospaceBasemap(ByVal cmd As cCommand) Handles m_cmdEditBasemap.OnInvoke
        Dim dlg As New dlgEditBasemap(Me.UIContext)
        Me.Help.HelpTopic(dlg) = "Edit basemap.htm"
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command handler; handles access to the Ecospace edit basemap dialog.
    ''' </summary>
    Private Sub OnUpdateEditEcospaceBasemap(ByVal cmd As cCommand) Handles m_cmdEditBasemap.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; invokes the Ecospace edit habitats dialog.
    ''' </summary>
    Private Sub OnEditEcospaceHabitats(ByVal cmd As cCommand) Handles m_cmdEditHabitats.OnInvoke
        Dim dlg As New dlgEditHabitats(Me.UIContext)
        Me.Help.HelpTopic(dlg) = "Edit habitats.htm"
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command handler; handles access to the Ecospace edit habitats dialog.
    ''' </summary>
    Private Sub OnUpdateEditEcospaceHabitats(ByVal cmd As cCommand) Handles m_cmdEditHabitats.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; handles access to the Ecospace edit regions dialog.
    ''' </summary>
    Private Sub OnUpdateEditEcospaceRegions(ByVal cmd As cCommand) Handles m_cmdEditRegions.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; invokes the Ecospace edit regions dialog.
    ''' </summary>
    Private Sub OnEditEcospaceRegions(ByVal cmd As cCommand) Handles m_cmdEditRegions.OnInvoke
        Dim dlg As New dlgDefineRegions(Me.UIContext)
        Me.Help.HelpTopic(dlg) = "Edit regions.htm"
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command handler; invokes the Ecospace edit MPAs dialog.
    ''' </summary>
    Private Sub OnEditEcospaceMPAs(ByVal cmd As cCommand) Handles m_cmdEditMPAs.OnInvoke
        Dim dlg As New dlgEditMPAs(Me.UIContext)
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command handler; handles access to the Ecospace edit MPAs dialog.
    ''' </summary>
    Private Sub OnUpdateEditEcospaceMPAs(ByVal cmd As cCommand) Handles m_cmdEditMPAs.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; invokes the Ecospace edit importance layers dialog.
    ''' </summary>
    Private Sub OnEditEcospaceImportanceLayers(ByVal cmd As cCommand) Handles m_cmdDefineImportanceMaps.OnInvoke
        Dim dlg As New dlgDefineImportanceMaps(Me.UIContext)
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command handler; updates the Ecospace edit importance layers command.
    ''' </summary>
    Private Sub OnUpdateEcospaceImportanceLayers(ByVal cmd As cCommand) Handles m_cmdDefineImportanceMaps.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; handles access to the Ecospace define input layers dialog.
    ''' </summary>
    Private Sub OnUpdateDefineInputLayers(ByVal cmd As cCommand) Handles m_cmdDefineInputLayers.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; invokes the Ecospace define input dialog.
    ''' </summary>
    Private Sub OnInvokeDefineInputLayers(ByVal cmd As cCommand) Handles m_cmdDefineInputLayers.OnInvoke
        Try
            Dim dlg As New dlgDefineEnvDriverMaps(Me.UIContext)
            dlg.ShowDialog(Me)
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Command handler
    ''' </summary>
    Private Sub OnEcospaceManageConfigs(cmd As cCommand) Handles m_cmdEcospaceManageConfigs.OnInvoke
        Try
            ' Reroute
            Me.m_cmdShowOptions.Invoke(eApplicationOptionTypes.SpatialTemporal)
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6:OnEcospaceManageConfigs")
        End Try
    End Sub

    ''' <summary>
    ''' Command handler
    ''' </summary>
    Private Sub OnDefineEcospaceDatasets(cmd As cCommand) Handles m_cmdDefineSpatialDatasets.OnInvoke
        Try
            Dim dlg As New Ecospace.Controls.dlgDefineExternalSpatialData()
            dlg.UIContext = Me.UIContext
            dlg.ShowDialog(Me)
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6:OnDefineEcospaceDatasets")
        End Try
    End Sub

    ''' <summary>
    ''' Command handler updater
    ''' </summary>
    Private Sub OnUpdateDefineEcospaceDatasetsInvoke(cmd As cCommand) Handles m_cmdDefineSpatialDatasets.OnUpdate
        Try
            Dim m As cCoreStateMonitor = Me.Core.StateMonitor
            cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Command handler
    ''' </summary>
    Private Sub OnEditEcospaceDataset(cmd As cCommand) Handles m_cmdEditSpatialDataset.OnInvoke

        Try
            Dim ds As EwEUtils.SpatialData.ISpatialDataSet = Me.m_cmdEditSpatialDataset.Dataset
            If (ds Is Nothing) Then Return
            If (Not TypeOf ds Is IConfigurable) Then Return

            '' This artifact should really not be necessary!!
            'If (TypeOf ds Is IPlugin) Then
            '    DirectCast(ds, IPlugin).Initialize(Me.Core)
            'End If

            Dim dsConf As IConfigurable = DirectCast(ds, IConfigurable)
            Dim ctrl As Control = dsConf.GetConfigUI()
            If (ctrl Is Nothing) Then Return

            Dim dlg As New dlgConfig(Me.UIContext)
            If dlg.ShowDialog(Me.FindForm, My.Resources.CAPTION_EXTERNAL_DATASET_CONFIGURE, ctrl) = System.Windows.Forms.DialogResult.OK Then
                Me.Core.SpatialDataConnectionManager.Update(ds)
            End If
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6:OnDefineEcospaceDatasets")
        End Try

    End Sub

    ''' <summary>
    ''' Command handler updater
    ''' </summary>
    Private Sub OnUpdateEditEcospaceDatasetInvoke(cmd As cCommand) Handles m_cmdDefineSpatialDatasets.OnUpdate
        Try
            Dim m As cCoreStateMonitor = Me.Core.StateMonitor
            cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Command handler
    ''' </summary>
    Private Sub OnEcospaceConfigureConnection(cmd As cCommand) Handles m_cmdEcospaceConfigureConnection.OnInvoke

        If (Me.m_cmdEcospaceConfigureConnection.Layer Is Nothing) Then Return
        Try
            Dim adt As cSpatialDataAdapter = Me.Core.SpatialDataConnectionManager.Adapter(Me.m_cmdEcospaceConfigureConnection.Layer.VarName)
            Dim dlg As New dlgApplyConnection(Me.UIContext, adt, Me.m_cmdEcospaceConfigureConnection.Layer, Me.m_cmdEcospaceConfigureConnection.Connection)

            If dlg.ShowDialog() = DialogResult.OK Then
                Me.Core.SpatialDataConnectionManager.NotifyCore(eMessageType.DataModified)
            End If
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6:OnEcospaceConfigureConnection")
        End Try
    End Sub

    ''' <summary>
    ''' Command handler
    ''' </summary>
    Private Sub OnExportEcospaceDatasets(cmd As cCommand) Handles m_cmdEcospaceExportSpatialDatasets.OnInvoke
        Try
            Dim dlg As New Ecospace.Controls.dlgExportSpatialData(Me.UIContext)
            dlg.ShowDialog(Me)
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6:OnExportEcospaceDatasets")
        End Try
    End Sub

    ''' <summary>
    ''' Command handler updater
    ''' </summary>
    Private Sub OnUpdateExportEcospaceDatasets(cmd As cCommand) Handles m_cmdEcospaceExportSpatialDatasets.OnUpdate
        Try
            Dim m As cCoreStateMonitor = Me.Core.StateMonitor
            cmd.Enabled = Not m.IsBusy And (Me.Core.SpatialDataConnectionManager.DatasetManager.Count > 0)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnImportLayerData(ByVal cmd As cCommand) _
        Handles m_cmdImportLayerData.OnInvoke

        Dim msg As cMessage = Nothing
        Try
            Select Case Me.m_cmdImportLayerData.Format
                Case eNativeLayerFileFormatTypes.Default,
                     eNativeLayerFileFormatTypes.XYZ
                    Dim dlg As New dlgImportLayerDataXYZ(Me.UIContext)
                    dlg.Layers = Me.m_cmdImportLayerData.Layers
                    dlg.ShowDialog(Me)
                Case eNativeLayerFileFormatTypes.ASCII
                    Dim ofd As New OpenFileDialog()
                    ofd.Title = SharedResources.CAPTION_SELECT_FILE
                    ofd.Filter = SharedResources.FILEFILTER_ASC
                    If (ofd.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then
                        Dim imp As New cEcospaceImportExportASCIIData(Me.Core)
                        If imp.Read(ofd.FileName) Then
                            Dim rs As EwEUtils.SpatialData.ISpatialRaster = imp.ToRaster
                            Dim l As cEcospaceLayer = Me.m_cmdImportLayerData.Layers(0)
                            For ir As Integer = 1 To rs.NumRows
                                For ic As Integer = 1 To rs.NumCols
                                    l.Cell(ir, ic) = rs.Cell(ir, ic)
                                Next
                            Next
                            l.Invalidate()
                            msg = New cMessage(cStringUtils.Localize(My.Resources.IMPORT_FILE_SUCCESS, ofd.FileName), eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Information)
                        Else
                            msg = New cMessage(cStringUtils.Localize(My.Resources.IMPORT_FILE_FAILED, ofd.FileName), eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Critical)
                        End If
                    End If
            End Select
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6:OnImportLayerData")
        End Try

        If (msg IsNot Nothing) Then
            Me.Core.Messages.SendMessage(msg)
        End If

    End Sub

    ''' <summary>
    ''' Command handler
    ''' </summary>
    Private Sub OnUpdateImportLayer(ByVal cmd As cCommand) _
        Handles m_cmdImportLayerData.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded() And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; invokes the export layers dialog to export data in XYZ format.
    ''' </summary>
    Private Sub OnExportLayerData(ByVal cmd As cCommand) _
        Handles m_cmdExportLayerData.OnInvoke
        Try
            Select Case Me.m_cmdExportLayerData.Format
                Case eNativeLayerFileFormatTypes.Default,
                     eNativeLayerFileFormatTypes.XYZ
                    Dim dlg As New dlgExportLayerDataXYZ(Me.UIContext)
                    dlg.Layers = Me.m_cmdExportLayerData.Layers
                    dlg.ShowDialog(Me)
                Case eNativeLayerFileFormatTypes.ASCII
                    Dim sfd As SaveFileDialog = cEwEFileDialogHelper.SaveFileDialog(SharedResources.CAPTION_SELECT_FILE, "", SharedResources.FILEFILTER_ASC)
                    If (sfd.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then
                        Dim imp As New cEcospaceImportExportASCIIData(Me.Core)
                        If imp.Read(Me.m_cmdExportLayerData.Layers(0)) Then
                            Dim bSuccess As Boolean = imp.Save(sfd.FileName)
                            Dim msg As cMessage = Nothing
                            If (bSuccess) Then
                                msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_DATA_SAVING_SUCCESS, sfd.FileName),
                                                   eMessageType.DataExport, eCoreComponentType.EcoSpace, eMessageImportance.Information)
                                msg.Hyperlink = Path.GetDirectoryName(sfd.FileName)
                            Else
                                msg = New cMessage(cStringUtils.Localize(My.Resources.STATUS_DATA_SAVING_FAILURE, sfd.FileName),
                                                   eMessageType.DataExport, eCoreComponentType.EcoSpace, eMessageImportance.Critical)
                            End If

                            Me.Core.Messages.SendMessage(msg)
                            ' ToDo: throw message
                        End If
                    End If
            End Select
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6:OnExportLayerData")
        End Try
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the 
    ''' <see cref="m_cmdImportLayerData">export layer data command</see>.
    ''' </summary>
    Private Sub OnUpdateExportLayerData(ByVal cmd As cCommand) _
        Handles m_cmdExportLayerData.OnUpdate

        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded() And Not m.IsBusy

    End Sub

    ''' <summary>
    ''' Command handler; invokes the edit layers dialog.
    ''' </summary>
    Private Sub OnInvokeEditLayer(ByVal cmd As cCommand) _
        Handles m_cmdEditLayer.OnInvoke

        Try
            Dim cmdEditLayer As cEditLayerCommand = DirectCast(cmd, cEditLayerCommand)
            Dim dlg As New dlgEditLayer(Me.UIContext, cmdEditLayer.Layer, cmdEditLayer.EditType)
            dlg.ShowDialog()
        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the 
    ''' <see cref="m_cmdImportLayerData">export layer data command</see>.
    ''' </summary>
    Private Sub OnUpdateEditLayer(ByVal cmd As cCommand) _
        Handles m_cmdEditLayer.OnUpdate

        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        ' Allow layer edits when Ecospace is paused
        cmd.Enabled = m.HasEcospaceLoaded() And (Not m.IsBusy Or Me.Core.EcospacePaused)

    End Sub

    Private Sub m_cmdEcospaceLoadXYRefData_OnUpdate(cmd As cCommand) Handles m_cmdEcospaceLoadXYRefData.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcospaceLoaded And Not m.IsBusy
    End Sub

    Private Sub m_cmdEcospaceLoadXYRefData_OnInvoke(cmd As cCommand) Handles m_cmdEcospaceLoadXYRefData.OnInvoke
        Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
        Dim cmdFO As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)

        cmdFO.Invoke(SharedResources.FILEFILTER_CSV & "|" & SharedResources.FILEFILTER_XYZ & "|" & SharedResources.FILEFILTER_TEXT)
        If cmdFO.Result = System.Windows.Forms.DialogResult.OK Then
            Dim manager As EcospaceTimeSeries.cEcospaceTimeSeriesManager = Me.Core.EcospaceTimeSeriesManager
            Dim InputFile As String = cmdFO.FileNames(0)
            manager.Load(InputFile, "", eVarNameFlags.EcospaceMapBiomass) ' Load with default output file name
        End If

    End Sub

#End Region ' Ecospace commands

#Region " Ecotracer commands "

    ''' <summary>
    ''' Command handler; creates a new Ecotracer scenario
    ''' </summary>
    Private Sub OnNewEcotracerScenario(ByVal cmd As cCommand) _
        Handles m_cmdNewEcotracerScenario.OnInvoke

        ' Prerequesite: Ecosim needs to be loaded
        Me.CoreController.LoadState(eCoreExecutionState.EcosimLoaded)
        ' Not successful? abort
        If Not Me.Core.StateMonitor.HasEcosimLoaded Then Return

        Dim dlg As New dlgEcotracerScenario(Me.UIContext, dlgEcotracerScenario.eDialogModeType.CreateScenario)

        If dlg.ShowDialog = System.Windows.Forms.DialogResult.OK Then

            Select Case dlg.Mode
                Case dlgEcotracerScenario.eDialogModeType.CreateScenario
                    Me.CreateEcotracerScenario(dlg.ScenarioName, dlg.ScenarioDescription, dlg.ScenarioAuthor, dlg.ScenarioContact)
                Case dlgEcotracerScenario.eDialogModeType.LoadScenario
                    Me.LoadEcotracerScenario(DirectCast(dlg.Scenario, cEcotracerScenario))
                Case Else
                    Debug.Assert(False)
            End Select

        End If

    End Sub

    ''' <summary>
    ''' Command update handler; takes care of enabling and disabling the
    ''' <see cref="m_cmdNewEcotracerScenario">New Ecotracer Scenario</see> command.
    ''' </summary>
    Private Sub OnUpdateNewEcotracerScenario(ByVal cmd As cCommand) _
        Handles m_cmdNewEcotracerScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; loads a new Ecotracer scenario
    ''' </summary>
    Private Sub OnLoadEcotracerScenario(ByVal cmd As cCommand) _
        Handles m_cmdLoadEcotracerScenario.OnInvoke
        Me.LoadEcotracerScenario()
    End Sub

    ''' <summary>
    ''' Command update handler; takes care of enabling and disabling the 
    ''' <see cref="m_cmdLoadEcotracerScenario">Load Ecotracer Scenario</see> command.
    ''' </summary>
    Private Sub OnUpdateLoadEcotracerScenario(ByVal cmd As cCommand) _
        Handles m_cmdLoadEcotracerScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded And Not m.IsBusy
    End Sub

    ''' <summary>
    ''' Command handler; closes the current Ecotracer scenario
    ''' </summary>
    Private Sub OnCloseEcotracerScenario(ByVal cmd As cCommand) _
        Handles m_cmdCloseEcotracerScenario.OnInvoke
        Me.m_autosavemanager.GatherSettings()
        Me.Core.CloseEcotracerScenario()
    End Sub

    ''' <summary>
    ''' Command update handler; takes care of enabling and disabling the 
    ''' <see cref="m_cmdCloseEcotracerScenario">Close Ecotracer Scenario</see> command.
    ''' </summary>
    Private Sub OnUpdateCloseEcotracerScenario(ByVal cmd As cCommand) _
        Handles m_cmdCloseEcotracerScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcotracerLoaded And Not m.IsBusy
    End Sub

    Private Sub OnSaveEcotracerScenarioAs(ByVal cmd As cCommand) _
        Handles m_cmdSaveEcotracerScenarioAS.OnInvoke

        Dim dlg As New dlgEcotracerScenario(Me.UIContext,
                                            dlgEcotracerScenario.eDialogModeType.SaveScenario,
                                            Me.Core.EcotracerScenarios(Me.Core.ActiveEcotracerScenarioIndex))

        If dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            ' Overwriting?
            If (dlg.Scenario IsNot Nothing) Then
                ' #Yes: prompt for overwrite confirmation
                Dim fmsg As New cFeedbackMessage(cStringUtils.Localize(My.Resources.SCENARIO_CONFIRMOVERWRITE_PROMPT, dlg.ScenarioName), eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
                Me.Core.Messages.SendMessage(fmsg)

                If (fmsg.Reply = eMessageReply.YES) Then
                    ' #Overwrite
                    cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOTRACER_SAVING, dlg.ScenarioName))
                    Try
                        Me.Core.SaveEcotracerScenario(DirectCast(dlg.Scenario, cEcotracerScenario))
                    Catch ex As Exception
                        cLog.Write(ex, "frmEwE6::SaveEcotracerScenarioAs")
                    End Try
                    cApplicationStatusNotifier.EndProgress(Me.Core)
                End If
                ' User does not want to overwrite? Abort
                Return
            End If

            ' Add scenario under new name
            cApplicationStatusNotifier.StartProgress(Me.Core, cStringUtils.Localize(My.Resources.STATUS_ECOTRACER_CREATING, dlg.ScenarioName))
            Me.Core.SaveEcotracerScenarioAs(dlg.ScenarioName, dlg.ScenarioDescription)
            cApplicationStatusNotifier.EndProgress(Me.Core)

        End If

    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the 'save ecotracer scenario as' command
    ''' </summary>
    Private Sub OnUpdateSaveEcotracerScenarioAs(ByVal cmd As cCommand) _
        Handles m_cmdSaveEcotracerScenarioAS.OnUpdate
        cmd.Enabled = Me.Core.StateMonitor.HasEcotracerLoaded()
    End Sub

    ''' <summary>
    ''' Command update handler; invokes the 'delete ecotracer scenario' command
    ''' </summary>
    Private Sub OnDeleteEcotracerScenario(ByVal cmd As cCommand) _
         Handles m_cmdDeleteEcotracerScenario.OnInvoke
        Dim dlg As New dlgEcotracerScenario(Me.UIContext, dlgScenario.eDialogModeType.DeleteScenario)
        dlg.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Command update handler; enables and disables the 'delete ecotracer scenario' command
    ''' </summary>
    Private Sub OnUpdateDeleteEcotracerScenario(ByVal cmd As cCommand) _
        Handles m_cmdDeleteEcotracerScenario.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded And
                      Not m.IsBusy And
                      Me.Core.nEcotracerScenarios > 0
    End Sub

    Private Sub OnEnableEcotracer(ByVal cmd As cCommand) _
        Handles m_cmdEnableEcotracer.OnInvoke

        Dim ecosimModelParams As cEcoSimModelParameters = Nothing
        Dim propSimConTracing As cBooleanProperty = Nothing
        Dim ecospaceModelParams As cEcospaceModelParameters = Nothing
        Dim propSpaceConTracing As cBooleanProperty = Nothing
        Dim tracerRunMode As eTracerRunModeTypes = CType(cmd.Tag, eTracerRunModeTypes)

        ' Try to update the core run state to satisfy the requested tracer setting
        Select Case tracerRunMode
            Case eTracerRunModeTypes.Disabled ' Ecotracer off
                ' NOP

            Case eTracerRunModeTypes.RunSim ' Ecosim
                ' Load sim
                Me.CoreController.LoadState(eCoreExecutionState.EcosimLoaded)
                ' Not successful? abort
                If Not Me.Core.StateMonitor.HasEcosimLoaded Then Return
                ' Get property to enable tracer for Sim
                ecosimModelParams = Me.Core.EcoSimModelParameters
                propSimConTracing = DirectCast(Me.PropertyManager.GetProperty(ecosimModelParams, eVarNameFlags.ConSimOnEcoSim), cBooleanProperty)
                ' Try to load tracer
                Me.CoreController.LoadState(eCoreExecutionState.EcotracerLoaded)

            Case eTracerRunModeTypes.RunSpace ' Ecospace
                ' Load space
                Me.CoreController.LoadState(eCoreExecutionState.EcospaceLoaded)
                ' Not successful? abort
                If Not Me.Core.StateMonitor.HasEcospaceLoaded Then Return
                ' Get property to enable tracer for Space
                ecospaceModelParams = Me.Core.EcospaceModelParameters
                propSpaceConTracing = DirectCast(Me.PropertyManager.GetProperty(ecospaceModelParams, eVarNameFlags.ConSimOnEcoSpace), cBooleanProperty)
                ' Try to load tracer
                Me.CoreController.LoadState(eCoreExecutionState.EcotracerLoaded)

        End Select

        ' Tracer not loaded?
        If Not Me.Core.StateMonitor.HasEcotracerLoaded Then tracerRunMode = eTracerRunModeTypes.Disabled

        ' Configure properties
        If propSimConTracing IsNot Nothing Then
            propSimConTracing.SetValue(tracerRunMode = eTracerRunModeTypes.RunSim)
        End If

        If propSpaceConTracing IsNot Nothing Then
            propSpaceConTracing.SetValue(tracerRunMode = eTracerRunModeTypes.RunSpace)
        End If

    End Sub

    Private Sub OnUpdateEnableEcotracer(ByVal cmd As cCommand) _
        Handles m_cmdEnableEcotracer.OnUpdate
        Dim m As cCoreStateMonitor = Me.Core.StateMonitor
        cmd.Enabled = m.HasEcopathLoaded And Not m.IsBusy
    End Sub

#End Region ' Ecotracer commands

#Region " Plug-in commands "

    Private Sub OnRunGUIPlugin(ByVal cmd As cCommand) Handles m_cmdPluginGUICommand.OnInvoke

        ' Sanity checks
        If Not (TypeOf cmd Is cPluginGUICommand) Then Return

        ' Phew
        Dim pgcmd As cPluginGUICommand = DirectCast(cmd, cPluginGUICommand)
        Dim iDockState As Integer = 0

        ' Check if core can be brought up to par
        If Me.CoreController.LoadState(pgcmd.CoreExecutionState) Then
            ' Invoke plugin. This code does not - and cannot - verify whether the plugin has already ran,
            ' and whether any plug-in UI elements are still active. The plug-in is responsible for dealing
            ' with consecutive run requests.

            cApplicationStatusNotifier.StartProgress(Me.Core, SharedResources.GENERIC_STATUS_RUNNINGPLUGIN)
            Try
                pgcmd.RunPlugin()
            Catch ex As Exception

            End Try
            cApplicationStatusNotifier.EndProgress(Me.Core)

            ' See if the plug-in attached any form to the command. This form will be nested in the interface
            ' if possible.
            If (pgcmd.Form IsNot Nothing) Then
                ' #Yes: form detected

                ' Set form icon based on core state
                Select Case pgcmd.CoreExecutionState
                    Case eCoreExecutionState.Idle
                        pgcmd.Form.Icon = My.Resources.Ecopath0
                    Case eCoreExecutionState.EcopathLoaded, eCoreExecutionState.EcopathCompleted, eCoreExecutionState.EcopathRunning
                        pgcmd.Form.Icon = My.Resources.Ecopath0
                    Case eCoreExecutionState.EcosimLoaded, eCoreExecutionState.EcosimRunning, eCoreExecutionState.EcosimCompleted
                        pgcmd.Form.Icon = My.Resources.Ecosim
                    Case eCoreExecutionState.EcospaceLoaded, eCoreExecutionState.EcospaceRunning, eCoreExecutionState.EcospaceCompleted
                        pgcmd.Form.Icon = My.Resources.Ecospace
                    Case eCoreExecutionState.EcotracerLoaded
                        pgcmd.Form.Icon = My.Resources.Ecotracer
                End Select

                ' Inherit plug-in execution state if needed
                If (TypeOf pgcmd.Form Is frmEwE) Then
                    Dim frmEwE As frmEwE = DirectCast(pgcmd.Form, frmEwE)
                    If (frmEwE.CoreExecutionState = eCoreExecutionState.Idle) Then
                        frmEwE.CoreExecutionState = pgcmd.CoreExecutionState
                    End If
                End If

                ' Able to activate this form from the open tabs?
                If Not ActivateForm(pgcmd.Form.Text) Then
                    ' #No: form is not currently integrated in the dock panel, it must be nested in the GUI.

                    ' Make sure it is not already shown; a visible form cannot be docked.
                    If pgcmd.Form.Visible Then
                        pgcmd.Form.Hide()
                    End If

                    ' Is this a dockable form? 
                    If (TypeOf pgcmd.Form Is DockContent) And (m_DockPanel.DocumentStyle = DocumentStyle.DockingMdi) Then
                        ' #Yes
                        ' Fix dockstyle
                        iDockState = pgcmd.DockState
                        If iDockState = 0 Then iDockState = DockState.Document

                        Try
                            ' Show the form in the dock panel
                            DirectCast(pgcmd.Form, DockContent).Show(Me.m_DockPanel, DirectCast(iDockState, DockState))
                        Catch ex As Exception

                        End Try

                        ' Fix window state
                        If pgcmd.Form.WindowState = FormWindowState.Minimized Then
                            pgcmd.Form.WindowState = FormWindowState.Normal
                            pgcmd.Form.Show()
                        End If

                    Else
                        ' Show form
                        pgcmd.Form.MdiParent = Me
                        pgcmd.Form.Show()
                    End If
                    ' Attach to help
                    Me.Help.HelpTopic(pgcmd.Form, pgcmd.HelpURL) = pgcmd.HelpTopic
                End If
            End If
        End If
    End Sub

#End Region ' Plug-in commands

#End Region ' Command handlers 

#Region " Big and evil event handlers "

    Private Sub OnModelMRUItemClicked(ByVal sender As Object, ByVal e As System.EventArgs)
        Try
            Dim mnuItem As ToolStripMenuItem = DirectCast(sender, ToolStripMenuItem)
            Dim strFileName As String = CStr(mnuItem.Tag)
            cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_ECOPATH_LOADING)
            Me.LoadEcopathModel(strFileName, eLoadSourceType.MRU)
            cApplicationStatusNotifier.EndProgress(Me.Core)
        Catch ex As Exception
            ' Whoah!
        End Try
    End Sub

    Private Sub OnSpatialTempMRUItemClicked(ByVal sender As Object, ByVal e As System.EventArgs)
        Try
            Dim mnuItem As ToolStripMenuItem = DirectCast(sender, ToolStripMenuItem)
            Me.m_cmdEcospaceLoadTimeSeries.Tag = mnuItem.Tag
            Me.m_cmdEcospaceLoadTimeSeries.Invoke()
            Me.m_cmdEcospaceLoadTimeSeries.Tag = Nothing
        Catch ex As Exception
            ' Whoah!
        End Try
    End Sub

    Private Sub OnLoadEcosimScenarioOrDataset(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim mnuItem As ToolStripMenuItem = CType(sender, ToolStripMenuItem)

        If (mnuItem.Tag Is Nothing) Then Return

        If (TypeOf mnuItem.Tag Is cEcoSimScenario) Then
            Me.m_cmdLoadEcosimScenario.Tag = mnuItem.Tag
            Me.m_cmdLoadEcosimScenario.Invoke()
            Me.m_cmdLoadEcosimScenario.Tag = Nothing
        ElseIf (TypeOf mnuItem.Tag Is cTimeSeriesDataset) Then
            Me.m_cmdEcosimLoadTimeSeries.Tag = DirectCast(mnuItem.Tag, cTimeSeriesDataset)
            Me.m_cmdEcosimLoadTimeSeries.Invoke()
            Me.m_cmdEcosimLoadTimeSeries.Tag = Nothing
        End If

    End Sub

    Private Sub OnLoadEcospaceScenario(ByVal sender As Object, ByVal e As System.EventArgs)

        Dim mnuItem As ToolStripMenuItem = CType(sender, ToolStripMenuItem)

        If (mnuItem.Tag Is Nothing) Then Return

        Me.m_cmdLoadEcospaceScenario.Tag = mnuItem.Tag
        Me.m_cmdLoadEcospaceScenario.Invoke()
        Me.m_cmdLoadEcospaceScenario.Tag = Nothing

    End Sub

    Private Sub OnLoadEcotracerScenario(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim mnuItem As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        Me.m_cmdLoadEcotracerScenario.Tag = mnuItem.Tag
        Me.m_cmdLoadEcotracerScenario.Invoke()
        Me.m_cmdLoadEcotracerScenario.Tag = Nothing
    End Sub

#Region " Settings handling "

    Private Sub OnSettingsLoaded(ByVal sender As Object, ByVal e As System.Configuration.SettingsLoadedEventArgs)

        Try

            ' Fix last selected dir
            If Not Directory.Exists(My.Settings.LastSelectedDirectory) Then
                My.Settings.LastSelectedDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            End If

            ' Read form positions
            Me.UIContext.FormSettings.Setting = My.Settings.FormSettings

            ' Get the form position from user settings
            Me.StartPosition = FormStartPosition.Manual
            Me.UIContext.FormSettings.Apply(Me, False)

            ' Kick the core
            Me.UpdateCorePaths(True)

            If String.IsNullOrWhiteSpace(My.Settings.Author) Then
                My.Settings.Author = Environment.UserName
            End If
            Me.Core.DefaultAuthor = My.Settings.Author
            Me.Core.DefaultContact = My.Settings.Contact

            Me.Core.SaveWithFileHeader = My.Settings.AutosaveHeaders
            Me.m_autosavemanager.Settings = My.Settings.AutosaveResults

            Dim man As cSpatialDataSetManager = Me.Core.SpatialDataConnectionManager.DatasetManager
            man.IsIndexingEnabled = My.Settings.SpatialTempAllowIndexing
            man.ConfigFiles = My.Settings.SpatialTempConfigurations

            ' Wait for Ecospace
            man.Load(My.Settings.SpatialTemporalConfigFile)

        Catch ex As Exception

        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler to respond to individual settings changes.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' -----------------------------------------------------------------------
    Private Sub OnSettingsChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)

        Try

            Select Case e.PropertyName

                Case "StatusMaxMessages", "StatusShowTime", "StatusSortNewestFirst"
                    If (Me.m_messageHistory IsNot Nothing) Then Me.m_messageHistory.Refresh()

                Case "MdbRecentlyUsedCount"
                    Me.PopulateModelMRUDropdown()

                Case "BackupFileMask", "OutputPathMask"
                    Me.UpdateCorePaths(False)

                Case "LogVerboseLevel"
                    Try
                        cLog.VerboseLevel = DirectCast(My.Settings.LogVerboseLevel, eVerboseLevel)
                    Catch ex As Exception
                        cLog.VerboseLevel = eVerboseLevel.Standard
                    End Try

                Case "Author"
                    Me.Core.DefaultAuthor = My.Settings.Author

                Case "Contact"
                    Me.Core.DefaultContact = My.Settings.Contact

                Case "SpatialTempAllowIndexing"
                    Me.Core.SpatialDataConnectionManager.DatasetManager.IsIndexingEnabled = My.Settings.SpatialTempAllowIndexing

                Case "AutosaveResults"
                    Me.m_autosavemanager.ApplySettingsAndEnsureDefaults()

                Case "AutosaveHeaders"
                    Me.Core.SaveWithFileHeader = My.Settings.AutosaveHeaders

                Case "Author"
                    Me.Core.DefaultAuthor = My.Settings.Author

                Case "Contact"
                    Me.Core.DefaultContact = My.Settings.Contact

            End Select

            Me.m_ssMain.UpdateModelPanes()

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnSettingsSaving(ByVal sender As Object, args As CancelEventArgs)

        My.Settings.AutosaveResults = Me.m_autosavemanager.Settings()
        My.Settings.SpatialTempAllowIndexing = Me.Core.SpatialDataConnectionManager.DatasetManager.IsIndexingEnabled

        args.Cancel = False

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update the directories in the Core to match any regular expressions.
    ''' </summary>
    ''' <remarks>
    ''' Note that this will also reset the base directory for commands 
    ''' <see cref="m_cmdFileOpen"/> and <see cref="m_cmdDirectoryOpen"/>.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Sub UpdateCorePaths(Optional ByVal bResetUI As Boolean = False)

        Dim strPath As String = ""

        If String.IsNullOrWhiteSpace(My.Settings.BackupFileMask) Then
            My.Settings.BackupFileMask = CStr(My.Settings.GetDefaultValue("BackupFileMask"))
        End If

        If cPathUtility.ResolvePath(My.Settings.BackupFileMask, Me.Core, strPath) Then
            ' Pass MASK in because the core will need to substitute fields into the mask
            Me.Core.BackupFileMask = My.Settings.BackupFileMask
        End If

        If String.IsNullOrWhiteSpace(My.Settings.OutputPathMask) Then
            My.Settings.OutputPathMask = CStr(My.Settings.GetDefaultValue("OutputPathMask"))
        End If

        If cPathUtility.ResolvePath(My.Settings.OutputPathMask, Me.Core, strPath) Then
            ' Pass actual formatted path because the core will not change this further.
            Me.Core.OutputPath = Path.GetFullPath(strPath)

            If (bResetUI) Then
                ' Also reset file and directory commands to use output dir by default
                Me.m_cmdFileOpen.Directory = Me.Core.OutputPath
                Me.m_cmdFileSave.Directory = Me.Core.OutputPath
                Me.m_cmdDirectoryOpen.Directory = Me.Core.OutputPath
            End If
        End If

    End Sub

#End Region ' Settings handling

    Private Sub OnTabFocusChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim idc As IDockContent = m_DockPanel.ActiveDocument
        Dim dch As DockContentHandler = Nothing
        Dim strNewNodeName As String = String.Empty
        Dim stateNew As eCoreExecutionState = eCoreExecutionState.Idle

        ' UI is CONTROLLING the nav tree, do NOT respond to events
        If Me.m_bNavigating Then Return

        If idc IsNot Nothing Then
            dch = idc.DockHandler

            If dch IsNot Nothing Then
                ' Get default nav link
                strNewNodeName = dch.TabText
            End If

            If (TypeOf idc Is frmEwE) Then
                ' Get form specific nav link
                If TypeOf DirectCast(idc, frmEwE).Tag Is String Then
                    strNewNodeName = CStr(DirectCast(idc, frmEwE).Tag)
                End If
                stateNew = DirectCast(idc, frmEwE).CoreExecutionState
            End If
        End If

        ' About to change?
        If (String.Compare(Me.m_strLastActiveContent, strNewNodeName) <> 0) Then

            ' Update core state if possible
            Me.CoreController.LoadState(stateNew)
            ' Update help
            Me.Help.ActiveHelpControl = CType(m_DockPanel.ActiveDocument, Control)
            ' Switch
            Me.UpdateSelectedNode(strNewNodeName)
        End If
    End Sub

    Private Sub OnModelPathAreaClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tsModel.OnPathAreaClicked
        Me.m_cmdLoadModel.Tag = Me.m_tsModel.Path
        Me.m_cmdLoadModel.Invoke()
        Me.m_cmdLoadModel.Tag = Nothing
    End Sub

    Private Sub OnCoreExecutionStateChanged(ByVal csm As cCoreStateMonitor)

        Try
            ' Busy loading or unloading Ecopath?
            If (csm.CoreExecutionState = eCoreExecutionState.Idle) Or
               (csm.CoreExecutionState = eCoreExecutionState.EcopathLoaded) Then
                ' Set or clear initial nav node
                Me.UpdateSelectedNode("", (csm.CoreExecutionState = eCoreExecutionState.EcopathLoaded))
            End If

            Me.UpdateModelControls()
            Me.PopulateModelMRUDropdown()
            Me.PopulateScenarioDropdowns()
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6::OnCoreExecutionStateChanged(" & csm.CoreExecutionState.ToString() & ")")
        End Try

    End Sub

    Private Sub OnCoreMessage(ByRef msg As cMessage)
        Try
            If msg.Type = eMessageType.DataAddedOrRemoved Then
                If (msg.DataType = eDataTypes.EcoSimScenario) Or
                   (msg.DataType = eDataTypes.EcoSpaceScenario) Or
                   (msg.DataType = eDataTypes.EcotracerScenario) Or
                   (msg.DataType = eDataTypes.TimeSeriesDataset) Or
                   (msg.DataType = eDataTypes.EcospaceSpatialDataConnection) Then
                    Me.PopulateScenarioDropdowns()
                End If
            End If
            If msg.Source = eCoreComponentType.Core Then
                If (msg.Type = eMessageType.GlobalSettingsChanged) Then
                    My.Settings.Save()
                End If
            End If
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6::OnCoreMessage(" & msg.Message & ")")
        End Try
    End Sub

    Private Sub OnProgressMessage(ByRef msg As cMessage)
        If Not TypeOf (msg) Is cProgressMessage Then Return
        Debug.Assert(msg.Type = eMessageType.Progress)
        Try
            Dim pmsg As cProgressMessage = DirectCast(msg, cProgressMessage)
            Me.ShowProgress(pmsg.ProgressState, pmsg.Message, pmsg.Progress)
        Catch ex As Exception
            cLog.Write(ex, "frmEwE6::OnProgressMessage(" & msg.Message & ")")
        End Try
    End Sub

#End Region  ' Big and evil event handlers

End Class