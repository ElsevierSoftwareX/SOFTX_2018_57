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

Imports System.Reflection
Imports EwECore
Imports EwEPlugin
Imports EwEPlugin.Data
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities

#End Region ' Imports

Public Class cEwENetworkAnalysisPlugin
    Inherits cNavTreeControlPlugin
    Implements IEcopathRunInitializedPlugin
    Implements IEcopathRunCompletedPlugin
    Implements IEcosimRunInitializedPlugin
    Implements IEcosimEndTimestepPlugin
    Implements IEcosimRunCompletedPlugin
    Implements IDataProducerPlugin
    Implements IUIContextPlugin
    Implements IDisposedPlugin

#Region " Private vars "

    Private m_uic As cUIContext = Nothing
    Private m_core As cCore = Nothing
    Private m_bInitOK As Boolean = False
    Private m_bDataEnabled As Boolean = True

    ''' <summary>
    ''' Network Analysis manager. Provide access to Network Analysis methods.
    ''' </summary>
    ''' <remarks>Because the plugin handles interactions with the core it manages the life span of the network manager and the interface. 
    ''' The plugin is responsible for telling the network manager when a plugin point has been invoked by the core. 
    ''' The plugin will pass a network manager reference to the interface when the user has clicked the plugins menu item or tree node in the main interface.</remarks>
    Private m_manager As cNetworkManager = Nothing

    ''' <summary>Interface form.</summary>
    Private m_frmNA As frmNetworkAnalysis = Nothing
    ''' <summary>Ooooh, that was long ago...</summary>
    Private m_ddx As cEwENetworkAnalysisData = Nothing

#End Region ' Private vars

#Region " Singleton "

    Public Shared thePlugin As cEwENetworkAnalysisPlugin = Nothing

    Public Sub New()
        If thePlugin Is Nothing Then
            thePlugin = Me
        End If
    End Sub

    Public Shared Function SwitchForm(ByVal page As frmNetworkAnalysis.eNetworkAnalysisPageTypes) As frmNetworkAnalysis

        ' Flag stating whether form is ready to be used. If so, we don't need to create it, do we?
        Dim bIsFormReady As Boolean = False
        Dim frm As frmNetworkAnalysis = Nothing

        'Interface item has been clicked
        'Show the Ecotroph interface
        If cEwENetworkAnalysisPlugin.thePlugin.m_bInitOK Then

            ' Does form still exist?
            If Not cEwENetworkAnalysisPlugin.thePlugin.HasUI() Then
                ' #No: create it
                frm = New frmNetworkAnalysis(cEwENetworkAnalysisPlugin.thePlugin.m_manager, cEwENetworkAnalysisPlugin.thePlugin.m_uic)
                cEwENetworkAnalysisPlugin.thePlugin.m_frmNA = frm
            Else
                frm = cEwENetworkAnalysisPlugin.thePlugin.m_frmNA
            End If
            frm.ShowPage(page)
        Else
            Debug.Assert(False, "Plugin was not initialized properly.")
        End If
        Return frm

    End Function

#End Region ' Singleton

#Region " Generic "

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "nwa00"
        End Get
    End Property

    Public Overrides ReadOnly Property ControlImage() As System.Drawing.Image
        Get
            Return SharedResources.nav_output
        End Get
    End Property

    Public Overrides ReadOnly Property ControlText() As String
        Get
            Return My.Resources.NAVITEM_ROOT
        End Get
    End Property

    Public Overrides ReadOnly Property NavigationTreeItemLocation() As String
        Get
            Return Me.NavTreeNodeRoot
        End Get
    End Property

    Public Overrides Function FormPage() As frmNetworkAnalysis.eNetworkAnalysisPageTypes
        Return frmNetworkAnalysis.eNetworkAnalysisPageTypes.Credits
    End Function

#End Region ' Generic

#Region " UIContext "

    Public Sub UIContext(uic As Object) Implements EwEPlugin.IUIContextPlugin.UIContext
        Me.m_uic = DirectCast(uic, cUIContext)
    End Sub

#End Region ' UIContext

#Region " Core "

    ''' <summary>
    ''' Initialize the Plugin. This is called when the core loads the Plugin. It will only be called once.
    ''' </summary>
    ''' <param name="core"></param>
    Public Overrides Sub Initialize(ByVal core As Object)

        m_bInitOK = False
        Try
            Me.m_core = DirectCast(core, EwECore.cCore)

            Me.m_manager = New cNetworkManager(Me.m_core)

            Me.m_bInitOK = True
            Me.m_ddx = New cEwENetworkAnalysisData(cTypeUtils.TypeToString(Me.GetType()), Me.m_manager)

        Catch ex As Exception
            cLog.Write(ex)
            System.Console.WriteLine(Me.ToString & ".Initialize() Error: " & ex.Message)
            Debug.Assert(False, ex.Message)
            Return
        End Try

    End Sub

    Public Sub Dispose() _
        Implements EwEPlugin.IDisposedPlugin.Dispose

        If Me.HasUI() Then
            Me.m_frmNA.Close()
            Me.m_frmNA = Nothing
        End If

        Me.m_manager = Nothing
        Me.m_core = Nothing
        Me.m_bInitOK = False

        thePlugin = Nothing

    End Sub

    Public ReadOnly Property Manager As cNetworkManager
        Get
            Return Me.m_manager
        End Get
    End Property

    Public ReadOnly Property UI As frmNetworkAnalysis
        Get
            If Me.HasUI() Then Return Me.m_frmNA
            Return Nothing
        End Get
    End Property

#End Region ' Core

#Region " Ecopath "

    Private m_bRunWithPathOld As Boolean = False

    Public Sub EcopathRunInitialized(EcopathDataAsObject As Object, TaxonDataAsObject As Object, StanzaDataAsObject As Object) Implements IEcopathRunInitializedPlugin.EcopathRunInitialized

        If (Me.Autosave(eAutosaveType.Ecopath)) Then
            Me.m_bRunWithPathOld = Me.Manager.RunWithEcopath
            Me.Manager.RunWithEcopath = True
        End If
    End Sub

    ''' <summary>
    ''' Called by the core when Ecopath has run successfuly 
    ''' </summary>
    ''' <param name="EcopathDataStructures"></param>
    ''' <remarks></remarks>
    Public Sub EcopathRunCompleted(ByRef EcopathDataStructures As Object) _
        Implements EwEPlugin.IEcopathRunCompletedPlugin.EcopathRunCompleted

        Debug.Assert(TypeOf EcopathDataStructures Is EwECore.cEcopathDataStructures, Me.ToString &
                            ".EcopathRan() argument EcopathDataStructure is not a cEcopathDataStructures object.")
        Try
            Me.m_manager.EcopathData = DirectCast(EcopathDataStructures, EwECore.cEcopathDataStructures)
            'Bug 252 fix by joeh
            Me.m_manager.IsMainNetworkRun = False
            Me.m_manager.IsRequiredPrimaryProdRun = False
            Me.m_manager.IsEcosimNetworkRun = False
            'End 252

            If Me.m_manager.RunWithEcopath Then
                Me.m_manager.RunMainNetwork()
                Me.BroadcastResults()

                ' JS 09No17: added autosave support
                If My.Settings.AutosaveEcopath Then
                    Dim wr As New cNetworkAnalysisEcopathResultWriter(Me.m_manager)
                    wr.WriteResults(Me.m_core.DefaultOutputPath(eAutosaveTypes.Ecopath))
                    Me.Manager.RunWithEcopath = Me.m_bRunWithPathOld
                End If
            End If

        Catch ex As Exception
            cLog.Write(ex, "cEwENetworkAnalysisPlugin.EcopathRunCompleted")
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

#End Region ' Ecopath

#Region " Ecosim "

    Private m_bRunWithSimOld As Boolean = False

    ''' <summary>
    ''' Ecosim is about to enter the time loop. All the data has been initialized
    ''' </summary>
    ''' <param name="EcosimDatastructures"></param>
    ''' <remarks></remarks>
    Public Sub EcosimRunInitialized(ByVal EcosimDatastructures As Object) Implements EwEPlugin.IEcosimRunInitializedPlugin.EcosimRunInitialized

        Debug.Assert(TypeOf EcosimDatastructures Is EwECore.cEcosimDatastructures, Me.ToString &
                            ".EcosimRunInitialized() argument EcosimDatastructures is not a cEcosimDatastructures object.")

        ' Need to turn on Ecosim Network Analysis?
        If Me.Autosave(eAutosaveType.Ecosim) And Not Me.m_manager.UseEcosimNetwork Then
            Me.m_bRunWithSimOld = Me.m_manager.UseEcosimNetwork
            Me.m_manager.UseEcosimNetwork = True
            Me.m_manager.EcosimPPROn = My.Settings.AutosaveEcosimWithPPR
        End If

        'Only proceed if Ecosim Network Analysis is turned on
        If Not m_manager.UseEcosimNetwork Then Return

        Try
            If Not Me.m_manager.IsMainNetworkRun Then
                If Not Me.Manager.RunMainNetwork() Then
                    Return
                End If
            End If

            'set the EcosimData data in the network manager object
            'this is the data the Network analysis will be run on
            Me.m_manager.EcosimData = DirectCast(EcosimDatastructures, EwECore.cEcosimDatastructures)
            'Initialize the Network Analysis for Ecosim
            Me.m_manager.InitNetworkForEcosim()

        Catch ex As Exception
            cLog.Write(ex, "cEwENetworkAnalysisPlugin.EcosimRunInitialized")
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Ecosim has completed the time step 'iTime' all computations related to this time step have been completed.
    ''' </summary>
    ''' <param name="EcosimDatastructures"></param>
    ''' <param name="iTime"></param>
    ''' <param name="Ecosimresults"></param>
    ''' <remarks></remarks>
    Public Sub EcosimEndTimeStep(ByRef BiomassAtTimestep() As Single,
                                 ByVal EcosimDatastructures As Object,
                                 ByVal iTime As Integer, ByVal Ecosimresults As Object) _
        Implements EwEPlugin.IEcosimEndTimestepPlugin.EcosimEndTimeStep

        Try
            'Only run the Ecosim Network Analysis if it is turned on
            If Not m_manager.UseEcosimNetwork Then
                Return
            End If

            If TypeOf EcosimDatastructures Is EwECore.cEcosimDatastructures Then
                'set the EcosimData data in the network manager object
                'this is the data the Network analysis will be run on
                Dim esData As cEcosimDatastructures = DirectCast(EcosimDatastructures, EwECore.cEcosimDatastructures)
                m_manager.EcosimTimeStep(BiomassAtTimestep, esData, iTime)
            Else
                Debug.Assert(False, Me.ToString & ".EcosimEndTimeStep() ")
            End If

        Catch ex As Exception
            cLog.Write(ex, "cEwENetworkAnalysisPlugin.EcosimEndTimeStep")
            Debug.Assert(False, ex.StackTrace)
        End Try

    End Sub

    Public Sub EcosimRunCompleted(ByVal EcosimDatastructures As Object) _
        Implements EwEPlugin.IEcosimRunCompletedPlugin.EcosimRunCompleted

        Try
            ' JS 170911: Sim broadcasting was never used, and the code below might tigger a new Sim run. Idea abadoned
            'If (Me.m_manager.RunWithEcosim) Then
            '    If Me.m_manager.RunEcosimNetwork() Then
            '        Me.BroadcastResults()
            '    End If
            'End If

            If Me.Autosave(eAutosaveType.Ecosim) Then
                ' Flag Ecosim run as completed
                Me.m_manager.IsEcosimNetworkRun = True

                Dim wr As New cNetworkAnalysisEcosimResultWriter(Me.m_manager)
                Dim strPath As String = Me.m_core.DefaultOutputPath(eAutosaveTypes.Ecosim)
                wr.WriteResults(strPath)
                Me.m_manager.UseEcosimNetwork = Me.m_bRunWithSimOld
            End If
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Ecosim

#Region " Data exchange "

    Private m_broadcaster As IDataBroadcaster = Nothing

    Public Sub Broadcaster(ByVal broadcaster As IDataBroadcaster) _
        Implements IDataProducerPlugin.Broadcaster
        Me.m_broadcaster = broadcaster
    End Sub

    Public Function ProducesData(ByVal typeData As System.Type, Optional ByVal runType As IRunType = Nothing) As Boolean _
        Implements IDataProducerPlugin.IsDataAvailable
        Return (typeData Is GetType(INetworkAnalysisData))
    End Function

    Public Function IsEnabled1() As Boolean Implements EwEPlugin.Data.IDataProducerPlugin.IsEnabled
        Return m_bDataEnabled
    End Function

    Public Function SetEnabled(ByVal bEnable As Boolean) As Boolean Implements EwEPlugin.Data.IDataProducerPlugin.SetEnabled
        Me.m_bDataEnabled = bEnable
    End Function

    Public Sub SetEnabled(ByVal typeData As System.Type, ByVal runType As IRunType, ByVal bEnabled As Boolean) _
        Implements EwEPlugin.Data.IDataProducerPlugin.SetEnabled

        If Not (typeData Is GetType(INetworkAnalysisData)) Then Return

        If TypeOf runType Is cEcopathRunType Then
            Me.m_manager.RunWithEcopath = bEnabled
        End If

        If TypeOf runType Is cEcosimRunType Then
            Me.m_manager.RunWithEcosim = bEnabled
        End If

    End Sub

    Public Function IsEnabled(ByVal typeData As System.Type, ByVal runType As IRunType) As Boolean _
        Implements EwEPlugin.Data.IDataProducerPlugin.IsEnabled

        If Not (typeData Is GetType(INetworkAnalysisData)) Then Return False

        If TypeOf runType Is cEcopathRunType Then
            Return Me.m_manager.RunWithEcopath
        End If

        If TypeOf runType Is cEcosimRunType Then
            Return Me.m_manager.RunWithEcosim
        End If

    End Function

    Public Function GetDataByType(ByVal typeData As System.Type, ByRef data As IPluginData) As Boolean _
        Implements IDataProducerPlugin.GetDataByType

        Try
            If typeData Is GetType(INetworkAnalysisData) Then
                Me.PopulateData()
                data = Me.m_ddx
            End If
        Catch ex As Exception
            data = Nothing
        End Try
        Return (data IsNot Nothing)

    End Function

    Private Sub PopulateData()

        ' Run network if needed
        If Not Me.m_manager.IsMainNetworkRun Then
            Me.m_manager.RunMainNetwork()
        End If

        Me.m_ddx.Resize(Me.m_core)

        Me.m_ddx.Ascendancy(1, 1) = m_manager.AscendancyImportTotal
        Me.m_ddx.Ascendancy(2, 1) = m_manager.AscendancyImportPer
        Me.m_ddx.Ascendancy(3, 1) = m_manager.OverheadImportTotal
        Me.m_ddx.Ascendancy(4, 1) = m_manager.OverheadImportPer
        Me.m_ddx.Ascendancy(5, 1) = m_manager.CapacityImportTotal
        Me.m_ddx.Ascendancy(6, 1) = m_manager.CapacityImportPer

        Me.m_ddx.Ascendancy(1, 2) = m_manager.AscendancyInternalFlowTotal
        Me.m_ddx.Ascendancy(2, 2) = m_manager.AscendancyInternalFlowPer
        Me.m_ddx.Ascendancy(3, 2) = m_manager.OverheadFlowTotal
        Me.m_ddx.Ascendancy(4, 2) = m_manager.OverheadFlowPer
        Me.m_ddx.Ascendancy(5, 2) = m_manager.CapacityFlowTotal
        Me.m_ddx.Ascendancy(6, 2) = m_manager.CapacityFlowPer

        Me.m_ddx.Ascendancy(1, 3) = m_manager.AscendancyExportTotal
        Me.m_ddx.Ascendancy(2, 3) = m_manager.AscendancyExportPer
        Me.m_ddx.Ascendancy(3, 3) = m_manager.OverheadExportTotal
        Me.m_ddx.Ascendancy(4, 3) = m_manager.OverheadExportPer
        Me.m_ddx.Ascendancy(5, 3) = m_manager.CapacityExportTotal
        Me.m_ddx.Ascendancy(6, 3) = m_manager.CapacityExportPer

        Me.m_ddx.Ascendancy(1, 4) = m_manager.AscendancyRespTotal
        Me.m_ddx.Ascendancy(2, 4) = m_manager.AscendancyRespPer
        Me.m_ddx.Ascendancy(3, 4) = m_manager.OverheadRespTotal
        Me.m_ddx.Ascendancy(4, 4) = m_manager.OverheadRespPer
        Me.m_ddx.Ascendancy(5, 4) = m_manager.CapacityRespTotal
        Me.m_ddx.Ascendancy(6, 4) = m_manager.CapacityRespPer

        Me.m_ddx.Ascendancy(1, 5) = m_manager.AscendancyTotalsTotal
        Me.m_ddx.Ascendancy(2, 5) = m_manager.AscendancyTotalsPer
        Me.m_ddx.Ascendancy(3, 5) = m_manager.OverheadTotalsTotal
        Me.m_ddx.Ascendancy(4, 5) = m_manager.OverheadTotalsPer
        Me.m_ddx.Ascendancy(5, 5) = m_manager.CapacityTotalsTotal
        Me.m_ddx.Ascendancy(6, 5) = m_manager.CapacityTotalsPer

        For i As Integer = 1 To Me.Manager.nGroups
            'Me.m_ddx.OmnivoryIndex(i) = m_manager.o
        Next

    End Sub

    Private Sub BroadcastResults()

        If (Me.m_broadcaster IsNot Nothing) And (Me.m_bDataEnabled = True) Then
            Me.PopulateData()
            Me.m_broadcaster.BroadcastData(Me.Name, Me.m_ddx)
        End If

    End Sub

#End Region ' Data exchange

#Region " Autosave "

    Friend Enum eAutosaveType As Integer
        Ecopath
        Ecosim
    End Enum

    Friend Property Autosave(savetype As eAutosaveType) As Boolean
        Get
            Select Case savetype
                Case eAutosaveType.Ecopath
                    Return My.Settings.AutosaveEcopath
                Case eAutosaveType.Ecosim
                    Return My.Settings.AutosaveEcosimWoPPR Or My.Settings.AutosaveEcosimWithPPR
            End Select
            Return False
        End Get
        Set(value As Boolean)
            If (value <> Autosave(savetype)) Then
                Select Case savetype
                    Case eAutosaveType.Ecopath
                        My.Settings.AutosaveEcopath = value
                    Case eAutosaveType.Ecosim
                        If (value = True) Then
                            ' Select default w/o PPR if none of the two Ecosim autosave options is selected
                            If (My.Settings.AutosaveEcosimWoPPR = False And My.Settings.AutosaveEcosimWithPPR = False) Then
                                My.Settings.AutosaveEcosimWoPPR = True
                            End If
                        Else
                            My.Settings.AutosaveEcosimWoPPR = False
                            My.Settings.AutosaveEcosimWithPPR = False
                        End If
                End Select
                My.Settings.Save()
            End If
        End Set
    End Property

#End Region ' Autosave

#Region " Internal helpers "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; states if the plug-in Form has been initialized as is
    ''' ready to be used.
    ''' </summary>
    ''' <returns>True if form is initialized and is ready to be used.</returns>
    ''' -----------------------------------------------------------------------
    Private Function HasUI() As Boolean

        ' No form? Whoah!
        If (Me.m_frmNA Is Nothing) Then Return False
        ' Form is ready to be used if it has not been disposed yet
        Return (Me.m_frmNA.IsDisposed = False)

    End Function

#End Region ' Internal helpers

End Class
