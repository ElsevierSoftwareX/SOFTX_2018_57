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

Option Strict On
Imports EwECore.Ecosim
Imports System.Threading
Imports EwECore.SearchObjectives
Imports EwEUtils.Core


Namespace FishingPolicy

    Public Class cFishingPolicyManager
        Inherits cThreadWaitBase 'provides the Wait() method
        Implements ICoreInterface
        Implements ISearchObjective

#Region "Private Variables"

        Private m_FPsearch As cFishingPolicySearch
        Private m_core As cCore

        ' Private m_lstGroups As New cCoreInputOutputList(Of cSearchObjectiveGroupInput)(eDataTypes.FishingPolicyGroupInput, 1)
        Private m_lstFleets As New cCoreInputOutputList(Of cFishingPolicySearchBlock)(eDataTypes.FishingPolicySearchBlocks, 1)
        Private m_parameters As cFishingPolicyParameters
        Private m_lstMessages As New List(Of cMessage)
        Private m_results As cFPSSearchResults
        Private m_searchObjective As cSearchObjective

        Private m_syncObject As System.ComponentModel.ISynchronizeInvoke
        Private m_SearchCompletedDelegate As SearchCompletedDelegate
        Private m_RunCompletedDelegate As RunCompletedDelegate
        Private m_ProgressDelegate As ProgressDelegate
        Private m_StartRunDelegate As RunStartedDelegate

        'Private m_PluginManager As c
        Private Delegate Sub CallingThreadDelegate()

#End Region

#Region "Construction and Initialization"

        ''' <summary>
        ''' Connect an interface to the Fishing Policy Search
        ''' </summary>
        ''' <param name="RunStartedCallBack">Callback a search run is about to start. If ModelParameters.nRun > 1 this will be call at the start of each run.</param>
        ''' <param name="RunCompletedBack">Callback a search run has completed. If ModelParameters.nRun > 1 this will be call at the end of each run.</param>
        ''' <param name="ProgressCallBack">Callback reports progress of the search</param>
        ''' <param name="SearchCompletedCallBack">Calback all search runs have completed.</param>
        ''' <remarks></remarks>
        Public Sub Connect(ByVal RunStartedCallBack As RunStartedDelegate, ByVal RunCompletedBack As RunCompletedDelegate, _
                            ByVal ProgressCallBack As ProgressDelegate, ByVal SearchCompletedCallBack As SearchCompletedDelegate)

            m_StartRunDelegate = RunStartedCallBack
            m_RunCompletedDelegate = RunCompletedBack
            m_ProgressDelegate = ProgressCallBack
            m_SearchCompletedDelegate = SearchCompletedCallBack

        End Sub


        Public Sub DisConnect()

            m_StartRunDelegate = Nothing
            m_RunCompletedDelegate = Nothing
            m_ProgressDelegate = Nothing
            m_SearchCompletedDelegate = Nothing



        End Sub



        Friend Sub New()

        End Sub

        ''' <summary>
        ''' Build interface objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function Init(ByRef theCore As cCore) As Boolean Implements ISearchObjective.Init
            Try

                m_core = theCore

                'init the Fihsing Policy Search model
                m_FPsearch = New cFishingPolicySearch
                m_FPsearch.init(m_core)
                m_FPsearch.SearchCompletedCallBack = AddressOf Me.OnFPSCompletedHandler
                m_FPsearch.AddMessageCallBack = AddressOf Me.OnFPSAddMessageHandler
                m_FPsearch.ProgressCallBack = AddressOf OnFPSProgressHandler
                m_FPsearch.SearchStartedCallBack = AddressOf OnFPSRunStartedHandler
                m_FPsearch.RunCompletedCallBack = AddressOf OnFPSRunCompletedHandler

                'init object for interface
                m_parameters = New cFishingPolicyParameters(m_core, cCore.NULL_VALUE)

                'get the search objective object from the core
                'this is Group, Fleet and Parameters for the shared search interface ISearchObjective
                m_searchObjective = m_core.SearchObjective

                'Init the search data
                Dim search As cSearchDatastructures = m_core.m_SearchData

                'redims and sets frate, Jobs and TargetProfitability to default values
                'search.bInSearch = True
                search.SearchMode = eSearchModes.FishingPolicy

                ''sets BGoalValue() as a function of PB from last ecopath run
                'search.setDefaultBGoal(m_core.m_EcoPathData.PB)

                'set block codes to defaults, a code for each fleet
                search.setDefaultFBlockCodes()

                'this will set ParNumber() and BlockNumber() based on the defaults set above
                search.SetFletchPars()

                m_lstFleets.Clear()
                Dim flt As cFishingPolicySearchBlock
                For iflt As Integer = 1 To m_core.nFleets
                    'use the database ID for the Fleets
                    flt = New cFishingPolicySearchBlock(m_core, m_core.m_EcoPathData.FleetDBID(iflt))
                    m_lstFleets.Add(flt)
                Next

                'set the search back to false 
                ' search.bInSearch = False
                search.SearchMode = eSearchModes.NotInSearch
                Return True

            Catch ex As Exception
                cLog.Write(ex)
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Load data into existing interface objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function Load() As Boolean Implements ISearchObjective.Load

            Try
                Dim iflt As Integer

                Dim coreData As cSearchDatastructures = m_core.m_SearchData

                'Model Parameters
                m_parameters.AllowValidation = False
                m_parameters.InitOption = eInitOption.EcopathBaseF

                m_parameters.MaxNumEval = coreData.nInterations
                m_parameters.nRuns = coreData.nRuns
                m_parameters.IncludeComp = coreData.IncludeCompetitiveImpact
                m_parameters.MaxEffChange = coreData.MaxEffortChange
                m_parameters.UseEconomicPlugin = coreData.FPSUseEconomicPlugin

                m_parameters.ResetStatusFlags()

                m_parameters.AllowValidation = True

                For Each flt As cFishingPolicySearchBlock In m_lstFleets
                    flt.AllowValidation = False

                    iflt = Array.IndexOf(m_core.m_EcoPathData.FleetDBID, flt.DBID)
                    flt.Index = iflt

                    flt.Resize()
                    flt.Name = m_core.m_EcoPathData.FleetName(iflt)

                    For it As Integer = 1 To m_core.nEcosimYears
                        flt.SearchBlocks(it) = coreData.FblockCode(iflt, it)
                    Next it

                    flt.AllowValidation = True

                Next

            Catch ex As Exception

            End Try

        End Function

        Public Sub Clear() Implements ISearchObjective.Clear

            Try

                If Me.m_lstFleets IsNot Nothing Then Me.m_lstFleets.Clear()
                If Me.m_parameters IsNot Nothing Then Me.m_parameters.Clear()

                If Me.m_FPsearch IsNot Nothing Then
                    Me.m_FPsearch.SearchCompletedCallBack = Nothing
                    Me.m_FPsearch.AddMessageCallBack = Nothing
                    Me.m_FPsearch.ProgressCallBack = Nothing
                    Me.m_FPsearch.SearchStartedCallBack = Nothing
                    Me.m_FPsearch.RunCompletedCallBack = Nothing
                End If
                Me.m_FPsearch = Nothing

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub

        ''' <summary>
        ''' Update the underlying data with values from the interface
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Update(ByVal DataType As eDataTypes) As Boolean Implements ISearchObjective.Update
            Dim coreData As cSearchDatastructures = m_core.m_SearchData

            'this will set the number of Frate() dimensions and populate it with default values
            'updating Frates() with FblockCode() for a time step is done by the model 
            coreData.nBlocks = Me.nSearchBlocks

            'load the code blocks
            For Each flt As cFishingPolicySearchBlock In m_lstFleets

                'coreData.Jobs(flt.Index) = flt.JobCatchValue
                'coreData.TargetProfitability(flt.Index) = flt.TargetProfitability

                For it As Integer = 1 To m_core.nEcosimYears
                    coreData.FblockCode(flt.Index, it) = flt.SearchBlocks(it)
                Next it
            Next

            'Model Parameters
            coreData.SearchMethod = Me.m_parameters.SearchOption

            coreData.InitOption = Me.m_parameters.InitOption
            coreData.SearchMethod = Me.m_parameters.SearchOption
            coreData.IncludeCompetitiveImpact = Me.m_parameters.IncludeComp
            coreData.FPSUseEconomicPlugin = Me.m_parameters.UseEconomicPlugin

            coreData.PortFolio = Me.m_parameters.MaxPortUtil

            coreData.nInterations = CInt(m_parameters.MaxNumEval)
            coreData.nRuns = m_parameters.nRuns

            coreData.MaxEffortChange = m_parameters.MaxEffChange
            If m_parameters.MaxEffChange > 0 Then
                coreData.MinimizeEffortChange = True
            Else
                coreData.MinimizeEffortChange = False
            End If

            'strangeness 
            'if OptimizeApproach is 'System Objective' then the SearchMethod is flet or dfpmin
            'if  OptimizeApproach is 'Base profitability' then SearchMethod is eSearchOption.BaseProfitability
            'this comes from EwE5
            If m_parameters.OptimizeApproach = eOptimizeApproachTypes.FleetValues Then
                coreData.SearchMethod = eSearchOptionTypes.BaseProfitability
            End If

        End Function

        '''' <summary>
        '''' Set the base year in the Searchblocks
        '''' </summary>
        '''' <remarks></remarks>
        'Friend Sub UpdateBaseYear()

        '    Try

        '        Dim coreData As cSearchDatastructures = m_core.m_SearchData

        '        'get the new base year
        '        Dim newBaseYear As Integer = Me.m_searchObjective.ObjectiveParameters.BaseYear
        '        Dim iOrgBaseYear As Integer
        '        Dim yearOffset As Integer = 1

        '        'the base year has not been set in the core data yet 
        '        'so use coreData.BaseYear to set the old value to something usefull
        '        If coreData.BaseYear = m_core.nEcosimYears Then
        '            yearOffset = -1
        '        End If

        '        iOrgBaseYear = coreData.BaseYear
        '        For iflt As Integer = 1 To m_core.nFleets

        '            Dim clearCode As Integer = coreData.FblockCode(iflt, iOrgBaseYear + yearOffset)
        '            coreData.FblockCode(iflt, iOrgBaseYear) = clearCode 'reset the original search code
        '            coreData.FblockCode(iflt, Me.m_searchObjective.ObjectiveParameters.BaseYear) = 0 'set the new base year

        '        Next iflt

        '        For Each flt As cFishingPolicySearchBlock In m_lstFleets
        '            For it As Integer = 1 To m_core.nEcosimYears
        '                flt.SearchBlocks(it) = coreData.FblockCode(flt.Index, it)
        '            Next it
        '        Next

        '        coreData.BaseYear = Me.m_searchObjective.ObjectiveParameters.BaseYear

        '    Catch ex As Exception
        '        cLog.Write(ex)
        '    End Try
        'End Sub


#End Region

#Region "Public Properties"

        Public ReadOnly Property SearchBlocks(ByVal iFleet As Integer) As cFishingPolicySearchBlock
            Get
                Return m_lstFleets(iFleet)
            End Get
        End Property

        Public ReadOnly Property ModelParameters() As cFishingPolicyParameters
            Get
                Return m_parameters
            End Get
        End Property

        ''' <summary>
        ''' Number of unique search blocks across all the fleets
        ''' </summary>
        Public ReadOnly Property nSearchBlocks() As Integer
            Get
                Dim nblocks As New List(Of Integer)
                Dim bindex As Integer
                Dim value As Integer
                For Each flt As cFishingPolicySearchBlock In m_lstFleets

                    For i As Integer = 1 To m_core.nEcosimYears
                        value = flt.SearchBlocks(i)
                        ' Only count search (non-zero) blocks
                        If (value > 0) Then
                            bindex = nblocks.IndexOf(value)
                            If (bindex < 0) Then
                                nblocks.Add(value)
                            End If
                        End If
                    Next i
                Next
                Return nblocks.Count

            End Get
        End Property

        ''' <summary>
        ''' Progress results of the search
        ''' </summary>
        ''' <remarks>This object will be populated at for each call to the ProgressHandler() delegate</remarks>
        Public ReadOnly Property SearchResults() As cFPSSearchResults
            Get
                Return m_FPsearch.Results
            End Get
        End Property


        ''' <summary>
        ''' Count of the current search run
        ''' </summary>
        ''' <remarks>if isRunning = True then iRun will be the count of the current run out of ModelParameters.nRuns</remarks>
        Public ReadOnly Property iRun() As Integer
            Get
                Return Me.m_FPsearch.iRun
            End Get
        End Property

        ''' <summary>
        ''' Stop the Fishing Policy Search run
        ''' </summary>
        ''' <remarks>This will not do anything if the search is not running</remarks>
        Public Overrides Function StopRun(Optional ByVal WaitTimeInMillSec As Integer = -1) As Boolean 'Implements SearchObjectives.ISearchObjective.StopRun

            Dim result As Boolean = True
            Try
                If Me.m_FPsearch IsNot Nothing Then
                    Me.m_FPsearch.SearchFailed = True
                    Me.m_FPsearch.StopEstimation = True

                    result = Me.Wait(WaitTimeInMillSec)

                End If

            Catch ex As Exception
                result = False
            End Try
            Return result

        End Function

#End Region

#Region "private handlers for search callbacks/delegates"

        Private Sub OnFPSCompletedHandler()

            Try

                m_core.m_SearchData.SearchMode = eSearchModes.NotInSearch

                'release any waiting threads
                Me.ReleaseWait()

                'send any messages that the model added to the managers list of messages
                'by using the m_syncObject the messages will be sent on the Interfaces thread not the FPS thread
                Dim ctd As CallingThreadDelegate = AddressOf Me.OnSendCoreMessages
                m_syncObject.BeginInvoke(ctd, Nothing)

                ctd = AddressOf Me.OnChanged
                m_syncObject.BeginInvoke(ctd, Nothing)

                If m_SearchCompletedDelegate IsNot Nothing Then
                    'call the delegate supplied by the interface
                    m_syncObject.BeginInvoke(m_SearchCompletedDelegate, Nothing)
                End If

                ctd = Nothing
                Me.m_syncObject = Nothing

            Catch ex As Exception
                cLog.Write(ex)
                m_core.m_SearchData.SearchMode = eSearchModes.NotInSearch
                Me.ReleaseWait()
            End Try

        End Sub


        Private Sub OnFPSProgressHandler()

            Try

                m_results = Me.m_FPsearch.Results

                If m_ProgressDelegate IsNot Nothing Then
                    'call the delegate supplied by the interface
                    m_syncObject.BeginInvoke(Me.m_ProgressDelegate, Nothing)
                End If

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub

        Private Sub OnFPSRunCompletedHandler()

            Try

                m_results = Me.m_FPsearch.Results

                If m_RunCompletedDelegate IsNot Nothing Then
                    'call the delegate supplied by the interface
                    m_syncObject.Invoke(Me.m_RunCompletedDelegate, Nothing)
                End If

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub


        Private Sub OnFPSAddMessageHandler(ByRef message As cMessage)
            'add the message to the managers list of mesasges
            'these messages will be sent at the end of the run
            m_lstMessages.Add(message)

        End Sub

        Private Sub OnFPSRunStartedHandler()
            Dim ctd As CallingThreadDelegate = Nothing

            Try

                ' Debug.Assert(Me.m_StartRunDelegate IsNot Nothing, "Fishing Policy Manager SearchStarted() has not been set.")
                If m_StartRunDelegate IsNot Nothing Then
                    'call the delegate supplied by the interface
                    m_syncObject.BeginInvoke(Me.m_StartRunDelegate, Nothing)
                End If

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub

        Private Sub OnSendCoreMessages()
            Try
                For Each msg As cMessage In m_lstMessages
                    m_core.Messages.AddMessage(msg)
                Next
                m_core.Messages.sendAllMessages()
                m_lstMessages.Clear()
            Catch ex As Exception
                'this should never happen!!!!! ehhhh
                cLog.Write(ex)
            End Try
        End Sub

        Private Sub OnChanged()
            Try
                m_core.onChanged(Me)
            Catch ex As Exception
                'this should never happen!!!!! ehhhh
                cLog.Write(ex)
            End Try
        End Sub

#End Region

#Region "Running the model"

        Public Function Run(ByVal SyncObject As System.ComponentModel.ISynchronizeInvoke) As Boolean

            m_syncObject = SyncObject
            Dim FPSthread As Thread
            Dim search As cSearchDatastructures = m_core.m_SearchData
            Dim bsuccess As Boolean

            Try

                If Me.IsRunning Then
                    ' ToDo: globalize this
                    m_core.Messages.SendMessage(New cMessage("A Fishing Policy Search is already running. Only one search can be run at a time.", eMessageType.ErrorEncountered, _
                                                eCoreComponentType.FishingPolicySearch, eMessageImportance.Critical, eDataTypes.MonteCarlo))
                    Return False
                End If

                bsuccess = True

                Me.SetWait()

                search.SearchMode = eSearchModes.FishingPolicy
                Me.m_core.m_EcoSimData.bTimestepOutput = True
                Me.Update(Me.DataType)

                FPSthread = New Thread(AddressOf Me.m_FPsearch.Run)
                FPSthread.Start()

            Catch ex As Exception
                cLog.Write(ex)
                'unblock the thread before doing anything incase something has called Wait()

                search.SearchMode = eSearchModes.NotInSearch
                ' ToDo: globalize this
                m_core.Messages.SendMessage(New cMessage("Error running the Fishing Policy Search.", eMessageType.ErrorEncountered, _
                                            eCoreComponentType.FishingPolicySearch, eMessageImportance.Critical, eDataTypes.FishingPolicyManager))

                'if an error has been thrown make sure the SearchCompletedCallBack delegate is called
                'this way an interface can responded 
                OnFPSCompletedHandler()
                bsuccess = False

            End Try

            'send any messages generated from starting the search
            Me.OnSendCoreMessages()
            Return bsuccess

        End Function

#End Region

#Region "ICoreInterface implementation"

        Public ReadOnly Property DataType() As eDataTypes Implements ICoreInterface.DataType
            Get
                Return eDataTypes.FishingPolicyManager
            End Get
        End Property

        Public ReadOnly Property CoreComponent() As eCoreComponentType Implements ICoreInterface.CoreComponent
            Get
                Return eCoreComponentType.EcoSim
            End Get
        End Property

        Public Property DBID() As Integer Implements ICoreInterface.DBID
            Get
                Return cCore.NULL_VALUE
            End Get
            Set(ByVal value As Integer)

            End Set
        End Property

        Public Function GetID() As String Implements ICoreInterface.GetID
            Return Me.ToString
        End Function

        Public Property Index() As Integer Implements ICoreInterface.Index
            Get

            End Get
            Set(ByVal value As Integer)
                Debug.Assert(False, "Can not set the Index of " & Me.ToString)
            End Set
        End Property

        Public Property Name() As String Implements ICoreInterface.Name
            Get
                Return Me.ToString
            End Get
            Set(ByVal value As String)
                Debug.Assert(False, "Can not set the Name of " & Me.ToString)
            End Set
        End Property

#End Region

#Region "ISearchObjective implementation"

        Public ReadOnly Property FleetObjectives(ByVal iFleet As Integer) As cSearchObjectiveFleetInput Implements ISearchObjective.FleetObjectives
            Get
                Return Me.m_searchObjective.FleetObjectives(iFleet)
            End Get
        End Property

        Public ReadOnly Property GroupObjectives(ByVal iGroup As Integer) As cSearchObjectiveGroupInput Implements ISearchObjective.GroupObjectives
            Get
                Return Me.m_searchObjective.GroupObjectives(iGroup)
            End Get
        End Property

        Public ReadOnly Property ObjectiveParameters() As SearchObjectives.cSearchObjectiveParameters Implements SearchObjectives.ISearchObjective.ObjectiveParameters
            Get
                Return Me.m_searchObjective.ObjectiveParameters
            End Get
        End Property

        Public ReadOnly Property ValueWeights() As cSearchObjectiveWeights Implements ISearchObjective.ValueWeights
            Get
                Return Me.m_searchObjective.ValueWeights
            End Get
        End Property

#End Region

    End Class

End Namespace