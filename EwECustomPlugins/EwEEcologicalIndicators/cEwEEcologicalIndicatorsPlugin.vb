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
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Class that connects to the core plug-in points. All indicator computations
''' are triggered from within this class.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEwEEcologicalIndicatorsPlugin
    Implements EwEPlugin.ICorePlugin
    Implements EwEPlugin.IDisposedPlugin
    Implements EwEPlugin.IEcopathPlugin
    Implements EwEPlugin.IEcopathRunCompleted2Plugin
    Implements EwEPlugin.IEcopathRunInvalidatedPlugin
    Implements EwEPlugin.IEcosimPlugin
    Implements EwEPlugin.IEcosimInitializedPlugin
    Implements EwEPlugin.IEcosimRunCompletedPostPlugin
    Implements EwEPlugin.IEcosimRunInvalidatedPlugin
    Implements EwEPlugin.IEcosimRunInitializedPlugin
    Implements EwEPlugin.IEcospacePlugin
    Implements EwEPlugin.IEcospaceEndTimestepPostPlugin
    Implements EwEPlugin.IEcospaceRunInvalidatedPlugin
    Implements EwEPlugin.IEcospaceInitRunCompletedPlugin
    Implements EwEPlugin.IEcospaceRunCompletedPlugin
    Implements EwEPlugin.IMenuItemPlugin
    Implements EwEPlugin.INavigationTreeItemPlugin
    Implements EwEPlugin.IUIContextPlugin
    Implements EwEPlugin.IMonteCarloPlugin
    Implements EwEPlugin.IAutoSavePlugin

#Region " Variables "

    Friend Enum eComponentType As Integer
        Ecopath
        Ecosim
        MonteCarlo
        Ecospace
        Any
    End Enum

    Private m_core As cCore = Nothing
    Private m_uic As cUIContext = Nothing

    Private m_ecopathDS As cEcopathDataStructures = Nothing
    Private m_ecosimDS As cEcosimDatastructures = Nothing
    Private m_ecospaceDS As cEcospaceDataStructures = Nothing
    Private m_stanzaDS As cStanzaDatastructures = Nothing
    Private m_taxonDS As cTaxonDataStructures = Nothing

    ''' <summary>Indicators for Ecopath.</summary>
    Friend m_indEcopath As cEcopathIndicators = Nothing
    ''' <summary>Indicators for each Ecosim time step.</summary>
    Friend m_lIndEcosim As List(Of cEcosimIndicators) = Nothing
    ''' <summary>Indicators for each MC trial and time step.</summary>
    Friend m_lIndMCsim As List(Of List(Of cMCIndicators)) = Nothing
    Friend m_lIndMCpath As List(Of cEcopathIndicators)

    ''' <summary>Indicators for each Ecospace cell.</summary>
    Friend m_dtIndEcospace As Dictionary(Of Point, cEcospaceIndicators)
    ''' <summary>Indicators grouping.</summary>
    Friend m_settings As New cIndicatorSettings()

    ''' <summary>The UI.</summary>
    Private m_frm As frmMain = Nothing
    ''' <summary>File save status message.</summary>
    Private m_msgStatus As cMessage = Nothing

    ' Fix run states to prevent surprises when user changes settings mid-run
    Private m_bRunWithEcopath As Boolean = False
    Private m_bRunWithEcosim As Boolean = False
    Private m_bRunWithEcospace As Boolean = False
    Private m_bRunWithMonteCarlo As Boolean = False
    Private m_bCalcExtrasOld As Boolean = False

#End Region ' Variables

#Region " Plug-in points "

#Region " Generic "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the author(s) of this plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Author() As String Implements EwEPlugin.IPlugin.Author
        Get
            Return "Marta Coll Montón, Jeroen Steenbeek"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the contact information for this plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Contact() As String Implements EwEPlugin.IPlugin.Contact
        Get
            Return "mailto:martacoll@yahoo.com"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the assembly description for this plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Description() As String Implements EwEPlugin.IPlugin.Description
        Get
            Return Me.ControlTooltipText
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the internal name for this plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Name() As String Implements EwEPlugin.IPlugin.Name
        Get
            Return "EwEBiomassIndicatorsPlugin"
        End Get
    End Property

#End Region ' Generic

#Region " Life span "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when the core has initialized and is
    ''' ready to be used by plug-ins.
    ''' </summary>
    ''' <param name="core">The <see cref="cCore">core</see> that this plug-in
    ''' can connect to.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Initialize(ByVal core As Object) Implements EwEPlugin.IPlugin.Initialize

        ' Grab and remember core when it is provided via the plug-in mechanism
        Me.m_core = DirectCast(core, cCore)

        ' Prepare data
        Me.m_indEcopath = Nothing
        Me.m_lIndEcosim = New List(Of cEcosimIndicators)
        Me.m_lIndMCpath = New List(Of cEcopathIndicators)
        Me.m_lIndMCsim = New List(Of List(Of cMCIndicators))
        Me.m_dtIndEcospace = New Dictionary(Of Point, cEcospaceIndicators)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that delivers ecopath, ecosim and ecospace models!
    ''' </summary>
    ''' <param name="objEcoPath"></param>
    ''' <param name="objEcoSim"></param>
    ''' <param name="objEcoSpace"></param>
    ''' -----------------------------------------------------------------------
    Public Sub CoreInitialized(ByRef objEcoPath As Object, _
                               ByRef objEcoSim As Object, _
                               ByRef objEcoSpace As Object) _
                           Implements EwEPlugin.ICorePlugin.CoreInitialized
        ' Not needed at this moment
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="EwEPlugin.IDisposedPlugin.Dispose"/>
    ''' -----------------------------------------------------------------------
    Public Sub Dispose() _
        Implements EwEPlugin.IDisposedPlugin.Dispose

        If Me.HasUI Then Me.m_frm.Close()
        If Me.m_frm IsNot Nothing Then Me.m_frm.Dispose()
        Me.m_frm = Nothing

        Me.m_indEcopath = Nothing
        Me.m_dtIndEcospace.Clear()
        Me.m_dtIndEcospace = Nothing

        Me.m_ecopathDS = Nothing
        Me.m_ecosimDS = Nothing
        Me.m_ecospaceDS = Nothing
        Me.m_taxonDS = Nothing
        Me.m_stanzaDS = Nothing

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Not used.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function LoadModel(ByVal dataSource As Object) As Boolean _
        Implements EwEPlugin.IEcopathPlugin.LoadModel

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Not used.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function SaveModel(ByVal dataSource As Object) As Boolean _
        Implements EwEPlugin.IEcopathPlugin.SaveModel

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Clean up neatly.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function Closemodel() As Boolean _
         Implements EwEPlugin.IEcopathPlugin.CloseModel

        ' Clear previous results
        Me.m_indEcopath = Nothing
        Me.m_lIndEcosim.Clear()
        Me.m_lIndMCpath.Clear()
        Me.m_lIndMCsim.Clear()
        Me.m_dtIndEcospace.Clear()

    End Function

#End Region ' Life span

#Region " Ecopath "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that delivers ecopath, taxon and stanza data structures when
    ''' Ecopath has finished a run.
    ''' </summary>
    ''' <param name="EcopathDataStructures">The <see cref="cEcopathDataStructures">ecopath data</see> with results.</param>
    ''' <param name="TaxonDataStructures">The <see cref="cTaxonDataStructures">taxonomy data</see> with supporting information for Ecopath.</param>
    ''' <param name="StanzaDataStructures">The <see cref="cStanzaDatastructures">stanza data</see> with supporting information for Ecopath.</param>
    ''' -----------------------------------------------------------------------
    Public Sub EcopathRunCompleted(ByRef EcopathDataStructures As Object, _
                                   ByRef TaxonDataStructures As Object, _
                                   ByRef StanzaDataStructures As Object) Implements EwEPlugin.IEcopathRunCompleted2Plugin.EcopathRunCompleted

        ' Grab and remember ecopath data structures when provided via the plug-in mechanism
        Me.m_ecopathDS = DirectCast(EcopathDataStructures, cEcopathDataStructures)
        Me.m_taxonDS = DirectCast(TaxonDataStructures, cTaxonDataStructures)
        Me.m_stanzaDS = DirectCast(StanzaDataStructures, cStanzaDatastructures)

        ' Do not calculate if not supposed to run with Ecospath
        If (Not My.Settings.RunWithEcopath) Then Return
        ' Do not calculate when Ecopath is running as part of a searches
        If (Me.m_core.StateMonitor.IsSearching()) Then Return

        ' Compute
        Me.m_indEcopath = New cEcopathIndicators(Me.m_core, Me.m_ecopathDS, Me.m_stanzaDS, Me.m_taxonDS, Me.m_core.TaxonAnalysis)
        Me.m_indEcopath.Compute()

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.Ecopath)
        End If

        If (Me.AutoSave And My.Settings.RunWithEcopath) Then
            If Me.BeginSave(eComponentType.Ecopath) Then
                Me.PerformSave(eComponentType.Ecopath)
                Me.EndSave()
            End If
        End If

    End Sub

    Public Sub EcopathRunInvalidated() Implements EwEPlugin.IEcopathRunInvalidatedPlugin.EcopathRunInvalidated

        ' Do not calculate if not supposed to run with Ecospath
        If (Not My.Settings.RunWithEcopath) Then Return
        ' Clear
        Me.ClearEcopathIndicators()

    End Sub

#End Region ' Ecopath

#Region " Ecosim "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ecosim has loaded
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub LoadEcosimScenario(dataSource As Object) _
        Implements EwEPlugin.IEcosimPlugin.LoadEcosimScenario
        Me.ClearEcosimIndicators()
    End Sub

    Public Sub EcosimInitialized(EcosimDatastructures As Object) _
        Implements EwEPlugin.IEcosimInitializedPlugin.EcosimInitialized

        ' Grab and remember ecosim data structures when provided via the plug-in mechanism
        Me.m_ecosimDS = DirectCast(EcosimDatastructures, cEcosimDatastructures)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ecosim about to start time stepping
    ''' </summary>
    ''' <param name="EcosimDatastructures"></param>
    ''' -----------------------------------------------------------------------
    Public Sub EcosimRunInitialized(EcosimDatastructures As Object) _
        Implements EwEPlugin.IEcosimRunInitializedPlugin.EcosimRunInitialized

        If (Me.m_core.StateMonitor.IsSearching()) Then
            Me.m_bRunWithEcosim = False
        Else
            Me.m_bRunWithEcosim = My.Settings.RunWithEcosim
        End If

        If (Not Me.m_bRunWithEcosim) Then Return

        Me.m_bCalcExtrasOld = Me.m_ecosimDS.bAlwaysCalcTLc
        Me.m_ecosimDS.bAlwaysCalcTLc = True

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ecosim finished running. Now compute indicators.
    ''' </summary>
    ''' <param name="EcosimDatastructures">The <see cref="cEcosimDatastructures">Ecosim data</see> with results.</param>
    ''' -----------------------------------------------------------------------
    Public Sub EcosimRunCompletedPost(ByVal EcosimDatastructures As Object) _
        Implements EwEPlugin.IEcosimRunCompletedPostPlugin.EcosimRunCompletedPost

        If (Not Me.m_bRunWithEcosim) Then Return
        ' Do not calculate when Ecosim is running as part of a searches

        ' Get ready to calculate
        Me.m_lIndEcosim.Clear()
        Dim lookup As cTaxonAnalysis = Me.m_core.TaxonAnalysis
        For iTime As Integer = 1 To Me.m_ecosimDS.NTimes
            Dim ind As New cEcosimIndicators(Me.m_core, Me.m_ecopathDS, Me.m_ecosimDS, iTime, Me.m_stanzaDS, Me.m_taxonDS, lookup)
            Me.m_lIndEcosim.Add(ind)
            ind.Compute()
        Next

        ' Need to save?
        If (My.Settings.AutoSaveCSV) Then
            ' #Yes: Save
            If Me.BeginSave(eComponentType.Ecosim) Then
                Me.PerformSave(eComponentType.Ecosim)
                Me.EndSave()
            End If
        End If

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.Ecosim)
        End If

        ' Restore preservation flag
        Me.m_ecosimDS.bAlwaysCalcTLc = Me.m_bCalcExtrasOld

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ecosim data has changed. Discard any results.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub EcosimRunInvalidated() _
        Implements EwEPlugin.IEcosimRunInvalidatedPlugin.EcosimRunInvalidated
        Me.ClearEcosimIndicators()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ecosim is closing. Forget Sim and MC indicators.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub CloseEcosimScenario() Implements EwEPlugin.IEcosimPlugin.CloseEcosimScenario
        Me.ClearEcosimIndicators()
        Me.ClearMCIndicators()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ecosim scenario is saved. Take no action.
    ''' </summary>
    ''' <param name="dataSource"></param>
    ''' -----------------------------------------------------------------------
    Public Sub SaveEcosimScenario(dataSource As Object) _
        Implements EwEPlugin.IEcosimPlugin.SaveEcosimScenario
        ' NOP
    End Sub

#End Region ' Ecosim

#Region " Monte Carlo "

    Public Sub MontCarloInitialized(MonteCarloAsObject As Object) _
        Implements EwEPlugin.IMonteCarloPlugin.MontCarloInitialized
        ' NOP
    End Sub

    Private m_bInitialized As Boolean = False

    Public Sub MonteCarloRunInitialized() _
        Implements EwEPlugin.IMonteCarloPlugin.MonteCarloRunInitialized

        ' Sanity checks
        Debug.Assert(Me.m_ecopathDS IsNot Nothing)
        Debug.Assert(Me.m_ecosimDS IsNot Nothing)

        If (Me.m_bInitialized) Then Return

        Me.m_bRunWithMonteCarlo = My.Settings.RunWithMC

        If (Me.m_bRunWithEcosim Or Me.m_bRunWithMonteCarlo) Then
            Me.m_bCalcExtrasOld = Me.m_ecosimDS.bAlwaysCalcTLc
            Me.m_ecosimDS.bAlwaysCalcTLc = True
        End If

        Me.ClearMCIndicators()

    End Sub

    Public Sub MonteCarloBalancedEcopathModel(TrialNumber As Integer, nIterations As Integer) _
        Implements EwEPlugin.IMonteCarloPlugin.MonteCarloBalancedEcopathModel

        ' Calculate only if supposed to run with MC
        If (Not Me.m_bRunWithMonteCarlo) Then Return

        Dim ind As New cEcopathIndicators(Me.m_core, Me.m_ecopathDS, Me.m_stanzaDS, Me.m_taxonDS, Me.m_core.TaxonAnalysis)
        ind.Compute()
        Me.m_lIndMCpath.Add(ind)

    End Sub

    Public Sub MonteCarloEcosimRunCompleted() _
        Implements EwEPlugin.IMonteCarloPlugin.MonteCarloEcosimRunCompleted

        Dim man As cMonteCarloManager = Me.m_core.EcosimMonteCarlo
        Dim lIter As New List(Of cMCIndicators)
        Dim lookup As cTaxonAnalysis = Me.m_core.TaxonAnalysis

        ' Calculate only if supposed to run with MC
        If (Not Me.m_bRunWithMonteCarlo) Then Return

        ' Calculate indicators for this MC iteration
        For iTime As Integer = 1 To Me.m_ecosimDS.NTimes
            Dim ind As New cMCIndicators(Me.m_core, Me.m_ecopathDS, Me.m_ecosimDS, CInt(man.nTrialIterations), iTime, Me.m_stanzaDS, Me.m_taxonDS, lookup)
            ind.Compute()
            lIter.Add(ind)
        Next
        Me.m_lIndMCsim.Add(lIter)

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.MonteCarlo)
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Search is finished. Restore Sim CalcTL flag
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub MonteCarloCompleted() _
        Implements EwEPlugin.IMonteCarloPlugin.MonteCarloRunCompleted

        If (Me.m_bRunWithEcosim Or Me.m_bRunWithMonteCarlo) Then
            Me.m_ecosimDS.bAlwaysCalcTLc = Me.m_bCalcExtrasOld
        End If
        Me.m_bInitialized = False

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.MonteCarlo)
        End If

        ' Need to save?
        If (My.Settings.AutoSaveCSV) Then
            ' #Yes: Save
            If Me.BeginSave(eComponentType.MonteCarlo) Then
                Me.PerformSave(eComponentType.MonteCarlo)
                Me.EndSave()
            End If
        End If

    End Sub

#End Region ' Monte Carlo

#Region " Ecospace "

    Public Sub LoadEcospaceScenario(ByVal dataSource As Object) _
        Implements EwEPlugin.IEcospacePlugin.LoadEcospaceScenario
        Me.ClearEcospaceIndicators()
    End Sub

    Public Sub SaveEcospaceScenario(ByVal dataSource As Object) _
        Implements EwEPlugin.IEcospacePlugin.SaveEcospaceScenario
        ' NOP
    End Sub

    Public Sub CloseEcospaceScenario() _
        Implements EwEPlugin.IEcospacePlugin.CloseEcospaceScenario
        Me.ClearEcospaceIndicators()
    End Sub

    Private m_bSavingEcospace As Boolean = False

    Public Sub EcospaceInitRunCompleted(EcospaceDatastructures As Object) _
        Implements EwEPlugin.IEcospaceInitRunCompletedPlugin.EcospaceInitRunCompleted

        Me.m_bRunWithEcospace = My.Settings.RunWithEcospace

        ' Calculate only if supposed to run with Ecospace
        If (Me.m_bRunWithEcospace = False) Then Return
        ' Do not calculate when Ecospace is running as part of a searches
        If (Me.m_core.StateMonitor.IsSearching()) Then Return

        ' Grab and remember ecosim data structures when provided via the plug-in mechanism
        Me.m_ecospaceDS = DirectCast(EcospaceDatastructures, cEcospaceDataStructures)

        ' Create indicators for water cells only
        Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
        Dim depth As cEcospaceLayerDepth = bm.LayerDepth
        Dim ptCell As Point = Nothing
        Dim lookup As cTaxonAnalysis = Me.m_core.TaxonAnalysis

        For iRow As Integer = 1 To bm.InRow
            For iCol As Integer = 1 To bm.InCol
                If (depth.IsWaterCell(iRow, iCol)) Then
                    ptCell = New Point(iCol, iRow)
                    Me.m_dtIndEcospace(ptCell) = New cEcospaceIndicators(Me.m_core, Me.m_ecopathDS, Me.m_ecospaceDS, New Point(iCol, iRow), Me.m_stanzaDS, Me.m_taxonDS, lookup)
                End If
            Next iCol
        Next iRow

        ' Preserve old TL calc setting
        Me.m_bCalcExtrasOld = Me.m_ecospaceDS.bCalTrophicLevel
        ' Enable trophic level calculations when plugin is configured to run with Ecospace
        Me.m_ecospaceDS.bCalTrophicLevel = True

        Me.m_bSavingEcospace = My.Settings.AutoSaveCSV
        If Me.m_bSavingEcospace Then Me.BeginSave(eComponentType.Ecospace)

    End Sub

    Public Sub EcospaceEndTimeStepPost(ByVal EcospaceDatastructures As Object, ByVal iTime As Integer) _
        Implements EwEPlugin.IEcospaceEndTimestepPostPlugin.EcospaceEndTimeStepPost

        ' Calculate only if supposed to run with Ecospace
        If (Me.m_bRunWithEcospace = False) Then Return
        ' Do not calculate when Ecospace is running as part of a searches
        If (Me.m_core.StateMonitor.IsSearching()) Then Return

        Try
            ' Compute
            For Each ind As cIndicators In Me.m_dtIndEcospace.Values
                ind.Compute()
            Next
            If Me.m_bSavingEcospace Then Me.PerformSave(eComponentType.Ecospace)
        Catch ex As Exception

        End Try

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.Ecospace)
        End If

    End Sub

    Public Sub EcospaceRunCompleted(EcoSpaceDatastructures As Object) _
        Implements EwEPlugin.IEcospaceRunCompletedPlugin.EcospaceRunCompleted

        ' Calculate only if supposed to run with Ecospace
        If (Me.m_bRunWithEcospace = False) Then Return
        ' Do not calculate when Ecospace is running as part of a searches
        If (Me.m_core.StateMonitor.IsSearching()) Then Return

        ' Restore old TL calc setting
        Me.m_ecospaceDS.bCalTrophicLevel = Me.m_bCalcExtrasOld

        If Me.m_bSavingEcospace Then Me.EndSave()

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.Ecospace)
        End If

    End Sub

    Public Sub EcospaceRunInvalidated() _
        Implements EwEPlugin.IEcospaceRunInvalidatedPlugin.EcospaceRunInvalidated
        ' Clear
        Me.ClearEcospaceIndicators()
        Me.m_bRunWithEcospace = False

    End Sub

#End Region ' Ecospace

#Region " UI "

    Public Sub UIContext(ByVal uic As Object) Implements EwEPlugin.IUIContextPlugin.UIContext
        Me.m_uic = DirectCast(uic, cUIContext)
    End Sub

    Public ReadOnly Property ControlImage As System.Drawing.Image Implements EwEPlugin.IGUIPlugin.ControlImage
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property ControlText As String Implements EwEPlugin.IGUIPlugin.ControlText
        Get
            Return My.Resources.CAPTION
        End Get
    End Property

    Public ReadOnly Property ControlTooltipText As String Implements EwEPlugin.IGUIPlugin.ControlTooltipText
        Get
            Return My.Resources.CAPTION_INFO
        End Get
    End Property

    Public ReadOnly Property EnabledState As EwEUtils.Core.eCoreExecutionState Implements EwEPlugin.IGUIPlugin.EnabledState
        Get
            Return EwEUtils.Core.eCoreExecutionState.EcopathLoaded
        End Get
    End Property

    Public Sub OnControlClick(ByVal sender As Object, ByVal e As System.EventArgs, ByRef frmPlugin As System.Windows.Forms.Form) _
        Implements EwEPlugin.IGUIPlugin.OnControlClick

        If (Not Me.HasUI) Then
            Me.m_frm = New frmMain(Me.m_uic, Me)
            Me.m_frm.Text = Me.ControlText
        End If
        frmPlugin = Me.m_frm

    End Sub

    Public ReadOnly Property MenuItemLocation As String Implements EwEPlugin.IMenuItemPlugin.MenuItemLocation
        Get
            Return "MenuTools"
        End Get
    End Property

    Public ReadOnly Property NavigationTreeItemLocation As String _
        Implements EwEPlugin.INavigationTreeItemPlugin.NavigationTreeItemLocation
        Get
            Return "ndTimeDynamic\ndEcosimTools"
        End Get
    End Property
#End Region ' UI

#End Region ' Plug-in points

#Region " Friend interfaces "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the default output folder.
    ''' </summary>
    ''' <returns>The default output folder, as specified in the EwE application options.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function DefaultFolder() As String
        Return Me.m_core.OutputPath
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the output folder that the user selected.
    ''' </summary>
    ''' <returns>The output folder that the user selected.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function OutputFolder(component As eComponentType) As String
        If My.Settings.SaveToDefault Then
            Select Case component
                Case eComponentType.Ecopath : Return Me.m_core.DefaultOutputPath(eAutosaveTypes.Ecopath)
                Case eComponentType.Ecosim : Return Me.m_core.DefaultOutputPath(eAutosaveTypes.Ecosim)
                Case eComponentType.Ecospace : Return Me.m_core.DefaultOutputPath(eAutosaveTypes.EcospaceResults)
                Case eComponentType.MonteCarlo : Return Me.m_core.DefaultOutputPath(eAutosaveTypes.MonteCarlo)
            End Select
            Debug.Assert(False)
        End If
        Return My.Settings.CustomFolder
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Central point to manually save computed indicators to a CSV file.
    ''' </summary>
    ''' <param name="component">The <see cref="eComponentType"/> to save indicators for.</param>
    ''' -----------------------------------------------------------------------
    Friend Sub SaveToCSVManual(component As eComponentType)

        If Me.BeginSave(component) Then
            Me.PerformSave(component)
            Me.EndSave()
        End If

    End Sub

    Friend Sub ClearEcopathIndicators()

        ' Eradicate computed Ecopath indicators
        Me.m_indEcopath = Nothing

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.Ecopath)
        End If

    End Sub

    Friend Sub ClearEcosimIndicators()

        ' Eradicate computed Ecosim indicators
        Me.m_lIndEcosim.Clear()

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.Ecosim)
        End If

    End Sub

    Friend Sub ClearEcospaceIndicators()

        ' Eradicate computed Ecospace indicators
        Me.m_dtIndEcospace.Clear()

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.Ecospace)
        End If

    End Sub

    Friend Sub ClearMCIndicators()

        ' Eradicate computed MC indicators
        Me.m_lIndMCpath.Clear()
        Me.m_lIndMCsim.Clear()

        ' Has UI?
        If (Me.HasUI) Then
            ' #Yes: Update UI
            Me.m_frm.UpdateIndicators(eComponentType.MonteCarlo)
        End If

    End Sub

#End Region ' Friend interfaces

#Region " Internal helpers "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the plug-in has an active user interface.
    ''' </summary>
    ''' <returns>True if the plug-in has an active user interface.</returns>
    ''' -----------------------------------------------------------------------
    Private Function HasUI() As Boolean
        If (Me.m_frm Is Nothing) Then Return False
        If (Me.m_frm.IsDisposed) Then Return False
        Return True
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Append a status to the status message.
    ''' </summary>
    ''' <param name="strMessage">The message to send.</param>
    ''' -----------------------------------------------------------------------
    Private Sub ReportStatus(strMessage As String, status As eStatusFlags)

        If (Me.m_msgStatus Is Nothing) Then Return

        Dim vs As New cVariableStatus(status, strMessage, eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
        Me.m_msgStatus.AddVariable(vs)

    End Sub

#End Region ' Internal helpers

#Region " Saving "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Begin the (auto)save progress by ensuring that destination directories
    ''' are available and that the save progress message is ready for gathering
    ''' save details.
    ''' </summary>
    ''' <param name="component">The <see cref="eComponentType"/> that is starting 
    ''' the save process.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function BeginSave(component As eComponentType) As Boolean

        Dim strPath As String = Me.OutputFolder(component)

        Me.m_msgStatus = Nothing

        If Not cFileUtils.IsDirectoryAvailable(strPath, True) Then
            Dim msg As New cMessage(String.Format(My.Resources.STATUS_INVALID_FOLDER, strPath), eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
            Me.m_core.Messages.SendMessage(msg)
            Return False
        End If

        Me.m_msgStatus = New cMessage(String.Format(SharedResources.GENERIC_FILESAVE_SUCCES, My.Resources.CAPTION, strPath), _
                                        eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
        Me.m_msgStatus.Hyperlink = strPath

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Perform an (auto)save step.
    ''' </summary>
    ''' <param name="component">The <see cref="eComponentType"/> that is saving.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function PerformSave(component As eComponentType) As Boolean

        ' Save prepartion failed? Abort!
        If (Me.m_msgStatus Is Nothing) Then Return False

        ' Safely encase file access logic to make sure that this method will not get interrupted
        Try
            Select Case component
                Case eComponentType.Ecopath
                    Me.SaveEcopathCSV()
                Case eComponentType.Ecosim
                    Me.SaveEcosimCSV()
                Case eComponentType.Ecospace
                    Me.SaveEcospaceCSV()
                Case eComponentType.MonteCarlo
                    Me.SaveMCCSV()
            End Select

        Catch ex As Exception
            Me.ReportStatus(String.Format(My.Resources.STATUS_SAVE_FAILED, Me.ComponentName(component), ex.Message), eStatusFlags.ErrorEncountered)
        End Try

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Ends the (auto)save progress by sending the save progress message.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function EndSave() As Boolean

        ' Save prepartion failed? Abort!
        If (Me.m_msgStatus Is Nothing) Then Return False

        Me.m_core.Messages.SendMessage(Me.m_msgStatus)
        Me.m_msgStatus = Nothing

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the full file name (not file path!) to save computed indicators 
    ''' to.
    ''' </summary>
    ''' <param name="component">The <see cref="eComponentType"/> to save indicators for.</param>
    ''' <param name="strStep">The time step, if any, to save indicators for. This
    ''' value can be left empty.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function FileName(ByVal component As eComponentType, ByVal strStep As String) As String
        Return cFileUtils.ToValidFileName(String.Format("biodiv_ind_{0}{1}.csv", Me.ComponentName(component), strStep), False)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the localized file name part that indicates the <see cref="eComponentType"/>
    ''' to save for.
    ''' </summary>
    ''' <param name="component">The <see cref="eComponentType"/> to save for.</param>
    ''' <returns>A localized file name part.</returns>
    ''' -----------------------------------------------------------------------
    Private Function ComponentName(component As eComponentType) As String
        Select Case component
            Case eComponentType.Ecopath : Return SharedResources.HEADER_ECOPATH
            Case eComponentType.Ecosim : Return SharedResources.HEADER_ECOSIM
            Case eComponentType.Ecospace : Return SharedResources.HEADER_ECOSPACE
            Case eComponentType.MonteCarlo : Return SharedResources.HEADER_MONTECARLO
        End Select
        Return ""
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save calculated Ecopath indicators to a CSV file.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub SaveEcopathCSV()

        If (Me.m_msgStatus Is Nothing) Then Return

        ' Sanity check
        Debug.Assert(Me.m_indEcopath.IsComputed, "Application flow error, ecopath indicators not calculated yet")

        Dim strPath As String = Me.OutputFolder(eComponentType.Ecopath)
        Dim strFile As String = Path.Combine(strPath, Me.FileName(eComponentType.Ecopath, ""))
        Dim sw As New StreamWriter(strFile)

        If Me.m_core.SaveWithFileHeader Then
            sw.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecopath))
            sw.WriteLine()
        End If

        ' Write header line
        sw.WriteLine("{0},{1}", cStringUtils.ToCSVField(SharedResources.HEADER_INDICATOR), cStringUtils.ToCSVField(SharedResources.HEADER_VALUE))

        ' Write a line for each indicator
        For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
            Dim grp As cIndicatorInfoGroup = Me.m_settings.IndicatorGroup(iGrp)
            For iInfo As Integer = 0 To grp.NumIndicators - 1
                Dim info As cIndicatorInfo = grp.Indicator(iInfo)
                sw.WriteLine("{0},{1}", cStringUtils.ToCSVField(info.Name), cStringUtils.FormatSingle(info.GetValue(Me.m_indEcopath)))
            Next
        Next

        ' Done
        sw.Flush()
        sw.Close()

        Me.ReportStatus(String.Format(My.Resources.STATUS_SAVE_SUCCESS, Me.ComponentName(eComponentType.Ecopath), strFile), eStatusFlags.OK)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save calculated Ecosim indicators to a CSV file.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub SaveEcosimCSV()

        Dim strPath As String = Me.OutputFolder(eComponentType.Ecosim)
        Dim strFile As String = Path.Combine(strPath, Me.FileName(eComponentType.Ecosim, ""))
        Dim sw As New StreamWriter(strFile)
        Dim sb As New StringBuilder()

        If Me.m_core.SaveWithFileHeader Then
            sw.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            sw.WriteLine()
        End If

        ' Write header line
        sb.Append(SharedResources.HEADER_TIME)
        For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
            Dim grp As cIndicatorInfoGroup = Me.m_settings.IndicatorGroup(iGrp)
            For iInfo As Integer = 0 To grp.NumIndicators - 1
                Dim info As cIndicatorInfo = grp.Indicator(iInfo)
                sb.Append(",")
                sb.Append(cStringUtils.ToCSVField(info.Name))
            Next
        Next
        sw.WriteLine(sb.ToString())

        ' Write a line for each time step
        For Each ind As cEcosimIndicators In Me.m_lIndEcosim

            ' Sanity check
            Debug.Assert(ind.IsComputed, "Application flow error, ecosim indicators not calculated yet")

            sb.Length = 0
            sb.Append(ind.Time)
            For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
                Dim grp As cIndicatorInfoGroup = Me.m_settings.IndicatorGroup(iGrp)
                For iInfo As Integer = 0 To grp.NumIndicators - 1
                    Dim info As cIndicatorInfo = grp.Indicator(iInfo)
                    sb.Append(",")
                    sb.Append(cStringUtils.FormatSingle(info.GetValue(ind)))
                Next iInfo
            Next iGrp
            sw.WriteLine(sb.ToString())
        Next ind

        ' Done
        sw.Flush()
        sw.Close()

        Me.ReportStatus(String.Format(My.Resources.STATUS_SAVE_SUCCESS, Me.ComponentName(eComponentType.Ecosim), strFile), eStatusFlags.OK)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save calculated MCMC indicators to a CSV file, both for path and sim.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub SaveMCCSV()
        Me.SaveMCCSVSpath()
        Me.SaveMCCSVSsim()
    End Sub

    Private Sub SaveMCCSVSpath()

        Dim core As cCore = Me.m_uic.Core
        Dim strPath As String = Me.OutputFolder(eComponentType.MonteCarlo)
        Dim strFile As String = Path.Combine(strPath, Me.FileName(eComponentType.MonteCarlo, "_uncertainty"))
        Dim sw As New StreamWriter(strFile)

        If core.SaveWithFileHeader Then
            sw.WriteLine(core.DefaultFileHeader(eAutosaveTypes.Ecopath))
            sw.WriteLine()
        End If

        ' Write header line
        sw.Write(cStringUtils.ToCSVField(SharedResources.HEADER_TRIAL))
        For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
            Dim grp As cIndicatorInfoGroup = Me.m_settings.IndicatorGroup(iGrp)
            For iInfo As Integer = 0 To grp.NumIndicators - 1
                Dim info As cIndicatorInfo = grp.Indicator(iInfo)
                sw.Write("," & cStringUtils.ToCSVField(info.Name))
            Next
        Next
        sw.WriteLine()

        Dim iTrial As Integer = 1
        For Each ind As cEcopathIndicators In Me.m_lIndMCpath
            sw.Write(iTrial)
            For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
                Dim grp As cIndicatorInfoGroup = Me.m_settings.IndicatorGroup(iGrp)
                For iInfo As Integer = 0 To grp.NumIndicators - 1
                    Dim info As cIndicatorInfo = grp.Indicator(iInfo)
                    sw.Write("," & cStringUtils.ToCSVField(info.GetValue(ind)))
                Next
            Next
            sw.WriteLine()
            iTrial += 1
        Next

        ' Done
        sw.Flush()
        sw.Close()

        Me.ReportStatus(String.Format(My.Resources.STATUS_SAVE_SUCCESS, Me.ComponentName(eComponentType.Ecopath), strFile), eStatusFlags.OK)

    End Sub

    Private Sub SaveMCCSVSsim()

        Dim core As cCore = Me.m_uic.Core
        Dim strPath As String = Me.OutputFolder(eComponentType.MonteCarlo)
        Dim strFile As String = Path.Combine(strPath, Me.FileName(eComponentType.MonteCarlo, ""))
        Dim sw As New StreamWriter(strFile)
        Dim sb As New StringBuilder()

        ' Write header 
        If core.SaveWithFileHeader Then
            sw.WriteLine(core.DefaultFileHeader(eAutosaveTypes.MonteCarlo))
        End If
        sw.WriteLine()

        sb.Append(My.Resources.HEADER_TRIAL)
        sb.Append(",")
        sb.Append(SharedResources.HEADER_TIME)
        For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
            Dim grp As cIndicatorInfoGroup = Me.m_settings.IndicatorGroup(iGrp)
            For iInfo As Integer = 0 To grp.NumIndicators - 1
                Dim info As cIndicatorInfo = grp.Indicator(iInfo)
                sb.Append(",")
                sb.Append(cStringUtils.ToCSVField(info.Name))
            Next
        Next
        sw.WriteLine(sb.ToString())

        ' Write a line for each trial + time step
        For iTrial As Integer = 0 To Me.m_lIndMCsim.Count - 1
            Dim lInd As List(Of cMCIndicators) = Me.m_lIndMCsim(iTrial)
            For Each ind As cMCIndicators In lInd

                ' Sanity check
                Debug.Assert(ind.IsComputed, "Application flow error, MC indicators not calculated yet")

                sb.Length = 0
                sb.Append(iTrial + 1)
                sb.Append(",")
                sb.Append(ind.Time)
                For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
                    Dim grp As cIndicatorInfoGroup = Me.m_settings.IndicatorGroup(iGrp)
                    For iInfo As Integer = 0 To grp.NumIndicators - 1
                        Dim info As cIndicatorInfo = grp.Indicator(iInfo)
                        sb.Append(",")
                        sb.Append(cStringUtils.FormatSingle(info.GetValue(ind)))
                    Next iInfo
                Next iGrp
                sw.WriteLine(sb.ToString())

            Next ind
        Next iTrial

        ' Done
        sw.Flush()
        sw.Close()

        Me.ReportStatus(String.Format(My.Resources.STATUS_SAVE_SUCCESS, Me.ComponentName(eComponentType.MonteCarlo), strFile), eStatusFlags.OK)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save calculated Ecospace indicators to a CSV file.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub SaveEcospaceCSV()

        Dim iTS As Integer = CInt(Me.m_ecospaceDS.TimeNow * 12 + 1)
        Dim strPath As String = Me.OutputFolder(eComponentType.Ecospace)
        Dim strFile As String = Path.Combine(strPath, Me.FileName(eComponentType.Ecospace, CStr(iTS)))
        Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
        Dim astrFields As New List(Of String)

        For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
            Dim grp As cIndicatorInfoGroup = Me.m_settings.IndicatorGroup(iGrp)
            For iInfo As Integer = 0 To grp.NumIndicators - 1
                Dim info As cIndicatorInfo = grp.Indicator(iInfo)
                astrFields.Add(info.Name)
            Next
        Next

        Dim exp As New cEcospaceImportExportXYData(Me.m_core, astrFields.ToArray())
        ' Write line for cell
        For Each ind As cEcospaceIndicators In Me.m_dtIndEcospace.Values
            If (ind.IsComputed) Then
                For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
                    Dim grp As cIndicatorInfoGroup = Me.m_settings.IndicatorGroup(iGrp)
                    For iInfo As Integer = 0 To grp.NumIndicators - 1
                        Dim info As cIndicatorInfo = grp.Indicator(iInfo)
                        exp.Value(ind.Location.Y, ind.Location.X, info.Name) = info.GetValue(ind)
                    Next iInfo
                Next iGrp
            End If
        Next ind

        ' Done
        exp.WriteXYFile(strFile, SharedResources.HEADER_COL, SharedResources.HEADER_ROW)

        Me.ReportStatus(String.Format(My.Resources.STATUS_SAVE_SUCCESS, Me.ComponentName(eComponentType.Ecospace), strFile), eStatusFlags.OK)

    End Sub

#End Region ' Saving

#Region " Autosave "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="EwEPlugin.IAutoSavePlugin.AutoSave"/>
    ''' -----------------------------------------------------------------------
    Public Property AutoSave As Boolean _
        Implements EwEPlugin.IAutoSavePlugin.AutoSave
        Get
            Return My.Settings.AutoSaveCSV
        End Get
        Set(value As Boolean)
            My.Settings.AutoSaveCSV = value
            My.Settings.Save()
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="EwEPlugin.IAutoSavePlugin.AutoSaveName"/>
    ''' -----------------------------------------------------------------------
    Public Function AutoSaveName() As String _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveName
        Return My.Resources.CAPTION
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="EwEPlugin.IAutoSavePlugin.AutoSaveOutputPath"/>
    ''' -----------------------------------------------------------------------
    Public Function AutoSaveSubPath() As String _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveOutputPath
        ' Not used
        Return ""
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="EwEPlugin.IAutoSavePlugin.AutoSaveType"/>
    ''' -----------------------------------------------------------------------
    Public Function AutoSaveType() As EwEUtils.Core.eAutosaveTypes _
        Implements EwEPlugin.IAutoSavePlugin.AutoSaveType
        Return eAutosaveTypes.NotSet
    End Function

#End Region ' Autosave

    Public ReadOnly Property HasMonteCarloRan As Boolean
        Get
            Return (Me.m_lIndMCsim.Count > 0) And Not Me.m_core.StateMonitor.IsSearching
        End Get
    End Property

End Class
