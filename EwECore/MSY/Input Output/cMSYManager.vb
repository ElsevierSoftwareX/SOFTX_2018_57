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

#Region "Imports "

Option Strict On

Imports System.Threading
Imports EwECore.SearchObjectives
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace MSY

    ''' <summary>
    ''' Delegate that allows MSY / FMSY run progress to be broadcasted.
    ''' </summary>
    ''' <param name="RunStateType"></param>
    Public Delegate Sub MSYRunStateDelegate(ByVal RunStateType As eMSYRunStates)

    ''' <summary>
    ''' Manager for interacting with the MSY routines.
    ''' </summary>
    Public Class cMSYManager
        Inherits cThreadWaitBase
        Implements ICoreInterface
        Implements ISearchObjective

#Region "Private variables"

        Private m_Core As cCore = Nothing
        Private m_MSY As cMSY = Nothing
        Private m_msyData As cMSYDataStructures = Nothing

        Private m_search As cSearchDatastructures = Nothing
        Private m_searchObjective As cSearchObjective = Nothing

        Private m_SyncOb As System.Threading.SynchronizationContext = Nothing

        Private m_parameters As cMSYParameters = Nothing
        Private m_fmsyresults As cFMSYResults = Nothing

        Private m_RunStateDelegate As MSYRunStateDelegate
        Private m_thread As Thread = Nothing

#End Region

#Region "Construction Initialization"

        Public Sub New(ByVal theCore As cCore, MSYData As cMSYDataStructures)

            Debug.Assert(theCore IsNot Nothing, Me.ToString & ".New() Invalid core object!")
            Debug.Assert(MSYData IsNot Nothing, Me.ToString & ".New() Invalid MSY Data object!")
            Debug.Assert(theCore.m_EcoSim IsNot Nothing, Me.ToString & ".New() Invalid Ecosim Model object!")

            Me.m_Core = theCore
            Me.m_msyData = MSYData

            Me.m_MSY = New cMSY(Me.m_Core.m_EcoSim, Me.m_msyData, Me.m_Core.m_EcoPathData, Me.m_Core.m_EcoSimData)
            Me.m_parameters = New cMSYParameters(Me.m_Core, Me.m_msyData)

        End Sub

#End Region

#Region " Running "

        ''' <summary>
        ''' Returns whether the MSY can run.
        ''' </summary>
        ''' <returns>True if allowed to run.</returns>
        ''' <remarks>
        ''' JS: Separated run check from run method because user may get prompted
        ''' for every run, and run may be called several times in succession.
        ''' </remarks>
        Public Function IsAllowedToRun() As Boolean

            Dim bEnabledTS As Boolean = False
            Dim bOkToRun As Boolean = True

            ' Cannot run if already running
            If (Me.m_thread IsNot Nothing) Then
                bOkToRun = Not Me.m_thread.IsAlive()
            End If

            Try

                If Me.m_Core.ActiveTimeSeriesDatasetIndex > 0 Then
                    Dim tsds As cTimeSeriesDataset = Me.m_Core.TimeSeriesDataset(Me.m_Core.ActiveTimeSeriesDatasetIndex)
                    For Each ts As cTimeSeries In tsds
                        If ts.Enabled Then
                            bEnabledTS = True
                            Exit For
                        End If
                    Next

                    'Is a time series loaded?
                    If bEnabledTS Then
                        ' #Yep: Ask the user what to do
                        Dim fbMsg As cFeedbackMessage = New cFeedbackMessage(My.Resources.CoreMessages.MSY_WARNING_TIMESERIES, _
                                                                             eCoreComponentType.MSY, eMessageType.StateNotMet, eMessageImportance.Question, _
                                                                             eMessageReplyStyle.YES_NO, eDataTypes.NotSet, eMessageReply.NO)
                        fbMsg.Suppressable = True
                        Me.m_Core.Messages.SendMessage(fbMsg)
                        bOkToRun = (fbMsg.Reply <> eMessageReply.NO)
                    End If 'bEnabledTS

                End If 'Me.m_Core.ActiveTimeSeriesDatasetIndex > 0

                If bOkToRun Then
                    ' ToDo: notify users if forcing functions are loaded
                End If

            Catch ex As Exception
                cLog.Write(ex, "cMSYManager::IsAllowedToRun")
                System.Console.WriteLine(Me.ToString & ".IsAllowedToRun() Exception: " & ex.Message)
                Dim msg As New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.MSY_ERROR_RUN, ex.Message), eMessageType.Any, eCoreComponentType.MSY, eMessageImportance.Critical)
                Me.SendMessage(msg)
            End Try

            Return bOkToRun

        End Function

        Public Function RunMSY() As Boolean
            Try
                If Not Me.IsAllowedToRun Then Return False
                Me.m_thread = New Thread(AddressOf Me.FullMSYSearchThreaded)
                Me.m_thread.Start()
            Catch ex As Exception
                cLog.Write(ex, "cMSYManager.RunMSY")
                Return False
            End Try
            Return True
        End Function

        Public Function RunFindFMSY() As Boolean
            Try
                If Not Me.IsAllowedToRun Then Return False
                Me.m_thread = New Thread(AddressOf Me.RunFindFMSYThreaded)
                Me.m_thread.Start()
            Catch ex As Exception
                cLog.Write(ex, "cMSYManager.RunFindFMSY")
                Return False
            End Try
            Return True
        End Function

#End Region ' Running

#Region " Internal threading "

        Private Sub FullMSYSearchThreaded()

            Dim runState As eMSYRunStates = eMSYRunStates.MSYRunComplete

            Me.SetWait()
            Me.m_Core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf Me.StopRun))
            Me.m_search.SearchMode = eSearchModes.MSY
            Me.m_msyData.bStopRun = False

            Try

                Me.m_parameters.Assessment = eMSYAssessmentTypes.FullCompensation
                If Me.m_MSY.RunMSY() Then
                    If Me.IsAutoSaveOutput Then Me.SaveMSYOutput()
                End If

                ' Process may have been stopped in the interim. Do not continue; RunMSY will ignore and reset the stop flag
                If Not Me.m_msyData.bStopRun Then
                    Me.m_parameters.Assessment = eMSYAssessmentTypes.StationarySystem
                    If Me.m_MSY.RunMSY() Then
                        If Me.IsAutoSaveOutput Then Me.SaveMSYOutput()
                    End If
                End If

                If Me.m_msyData.bStopRun Then runState = eMSYRunStates.MSYRunStopped

            Catch ex As Exception

            End Try

            'tell the interface that we are done
            Me.OnRunStateChanged(runState)
            Me.m_Core.SetStopRunDelegate(Nothing)
            Me.m_search.SearchMode = eSearchModes.NotInSearch

            Me.ReleaseWait()
            Me.m_thread = Nothing

        End Sub

        ''' <summary>
        ''' Run the FMSY search, quietly.
        ''' </summary>
        Private Sub RunFindFMSYThreaded()

            Dim runState As eMSYRunStates = eMSYRunStates.MSYRunComplete
            Dim assignments As eMSYAssessmentTypes() = New eMSYAssessmentTypes() {eMSYAssessmentTypes.FullCompensation, eMSYAssessmentTypes.StationarySystem}

            Me.SetWait()
            Me.m_Core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf Me.StopRun))
            Me.m_search.SearchMode = eSearchModes.MSY
            Me.m_msyData.bStopRun = False
            Me.m_fmsyresults = Nothing

            Try
                For Each assignment As eMSYAssessmentTypes In assignments

                    Me.m_msyData.AssessmentType = assignment

                    If Not Me.m_msyData.bStopRun Then
                        If Not Me.m_MSY.RunFindFMSY() Then
                            ' Apply handbrake
                            Me.m_msyData.bStopRun = True
                        End If
                    End If

                    If Not Me.m_msyData.bStopRun Then
                        ' Only process results when completed successfully
                        Dim results As New cFMSYResults(Me.m_Core.nGroups)
                        For i As Integer = 1 To Me.m_Core.nGroups

                            results.FMSY(i) = Me.m_MSY.FmsySS(i)
                            results.CMSY(i) = Me.m_MSY.CmsySS(i)

                            results.CatchAtFMSY(i) = Me.m_MSY.CatchAtFmsy(i)
                            results.ValueAtFMSY(i) = Me.m_MSY.ValueAtFmsy(i)

                            'get the base value of catch and F from the Core
                            results.CMSYBase(i) = Me.m_Core.m_EcoPathData.fCatch(i)
                            results.FBase(i) = Me.m_Core.m_EcoSimData.Fish1(i)

                            results.Value(i) = Me.m_MSY.VmsySS(i)
                            results.ValueBase(i) = Me.m_MSY.ValSumBase(i)

                            Dim t As cMSY.cFoptTracker = Me.m_MSY.m_FOptTracker(i)
                            results.IsFopt(i) = t.IsFopt()
                        Next
                        Me.m_fmsyresults = results

                        ' JS 04Nov12: Always save FMSY results until FMSY has a user interface.
                        ' ToDo_JS: Use Autosave settings for FMSY when there is a user interface.
                        'If Me.IsAutoSaveOutput Then
                        Me.SaveFMSYOutput()
                        'End If
                    End If
                Next

            Catch ex As Exception
                cLog.Write(ex, "cMSYManager::RunFMSY")
                System.Console.WriteLine(Me.ToString & ".RunFMSY() Exception: " & ex.Message)
                Dim msg As New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.MSY_ERROR_RUN_FMSY, ex.Message), eMessageType.Any, eCoreComponentType.EcoSim, eMessageImportance.Critical)
                Me.SendMessage(msg)
            End Try

            'tell the interface that we are done
            Me.OnRunStateChanged(runState)
            Me.m_Core.SetStopRunDelegate(Nothing)
            Me.m_search.SearchMode = eSearchModes.NotInSearch

            Me.ReleaseWait()

        End Sub

#End Region ' Internal threading

#Region " Notifications "

        Private Sub OnRunStateChanged(ByVal RunState As eMSYRunStates)
            Try
                m_SyncOb.Send(New System.Threading.SendOrPostCallback(AddressOf Me.fireRunStateDelegate), RunState)
            Catch ex As Exception

            End Try
        End Sub

        Private Sub SendMessage(ByRef msg As cMessage)
            If Me.m_Core IsNot Nothing Then
                Me.m_Core.Messages.SendMessage(msg)
            End If
        End Sub

        Private Sub fireRunStateDelegate(ByVal obj As Object)
            Try
                'Debug.Assert(m_SyncOb IsNot Nothing And m_MSECallback IsNot Nothing, Me.ToString & ".OnMSECallBack() not connected properly.")
                If Me.m_RunStateDelegate IsNot Nothing Then
                    Dim cbType As eMSYRunStates = DirectCast(obj, eMSYRunStates)
                    m_RunStateDelegate.Invoke(cbType)
                End If
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & " Error sending message to interface.")
            End Try
        End Sub

#End Region ' Notifications

#Region " Saving "

        Public Sub SaveMSYOutput()
            Try

                If (Me.MSYResults Is Nothing) Then Return
                If (Me.MSYResults.Length = 0) Then Return

                Dim writer As New cMSYResultWriterMSY(Me.m_Core)
                Dim bSuccess As Boolean = True

                Select Case Me.m_parameters.FSelectionMode

                    Case eMSYFSelectionModeType.Groups
                        bSuccess = writer.WriteGroupResults(Me.m_Core.DefaultOutputPath(eAutosaveTypes.MSY), _
                                                            Me.m_parameters.SelGroupFleetIndex, _
                                                            Me.m_parameters.Assessment, _
                                                            Me.BaseLineResults.FCur, _
                                                            Me.MSYResults, _
                                                            Me.FMSY)

                    Case eMSYFSelectionModeType.Fleets
                        bSuccess = writer.WriteFleetResults(Me.m_Core.DefaultOutputPath(eAutosaveTypes.MSY), _
                                                            Me.m_parameters.SelGroupFleetIndex, _
                                                            Me.m_parameters.Assessment, _
                                                            Me.MSYResults, _
                                                            Me.FMSY)

                End Select

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

        Public Function SaveFMSYOutput() As Boolean

            Try
                Dim w As New cMSYResultWriterFMSY(Me.m_Core)
                Return w.WriteCSV(Me.m_Core.DefaultOutputPath(eAutosaveTypes.MSY), _
                                  Me.m_parameters.Assessment, _
                                  Me.m_fmsyresults)

            Catch ex As Exception
                cLog.Write(ex)
                Debug.Assert(False, ex.Message)
            End Try
            Return False

        End Function

#End Region ' Saving

#Region " Public Properties "

        ''' <summary>
        ''' Returns the <see cref="cMSYParameters"/> to configure the MSY run.
        ''' </summary>
        Public Function Parameters() As cMSYParameters
            Return Me.m_parameters
        End Function

        ''' <summary>
        ''' Returns the <see cref="cMSYFResult">results list</see>, sorted by F.
        ''' </summary>
        Public Function MSYResults() As cMSYFResult()
            Return Me.m_msyData.lstResults.ToArray
        End Function

        ''' <summary>
        ''' Returns the <see cref="cMSYFResult">results</see> of a base line run.
        ''' </summary>
        Public Function BaseLineResults() As cMSYFResult
            Return Me.m_msyData.BaseLineResult
        End Function

        ''' <summary>
        ''' Returns the <see cref="cMSYOptimum">FMSY optimum results</see>.
        ''' </summary>
        Public Function FMSY() As cMSYOptimum
            Return Me.m_msyData.Optimum
        End Function

        ''' <summary>
        ''' Get/set whether output should be automatically saved to file.
        ''' </summary>
        Public Property IsAutoSaveOutput As Boolean
            Get
                Return Me.m_Core.Autosave(eAutosaveTypes.MSY)
            End Get
            Set(value As Boolean)
                Me.m_Core.Autosave(eAutosaveTypes.MSY) = value
            End Set
        End Property

        Public ReadOnly Property FMSYResults As cFMSYResults
            Get
                Return Me.m_fmsyresults
            End Get
        End Property

        Public ReadOnly Property RunType As eMSYRunTypes
            Get
                Return Me.m_msyData.MSYRunType
            End Get
        End Property

        Public Property RunStateChangedDelegate As MSYRunStateDelegate
            Get
                Return Me.m_RunStateDelegate
            End Get

            Set(value As MSYRunStateDelegate)
                Me.m_RunStateDelegate = Nothing
                Me.m_RunStateDelegate = value
            End Set

        End Property

#End Region ' Public Properties

#Region " ISearchObjective "

        Friend Function Init(ByRef theCore As cCore) As Boolean Implements ISearchObjective.Init

            m_Core = theCore
            m_searchObjective = m_Core.SearchObjective
            m_search = theCore.m_SearchData

            Me.m_SyncOb = System.Threading.SynchronizationContext.Current
            'if there is no current context then create a new one on this thread.
            If (Me.m_SyncOb Is Nothing) Then Me.m_SyncOb = New System.Threading.SynchronizationContext()

            'Connect to the MSY callback
            Me.m_MSY.Connect(AddressOf Me.OnRunStateChanged, AddressOf Me.SendMessage)

        End Function

        Friend Function Load() As Boolean Implements ISearchObjective.Load
            ' NOP
        End Function

        ''' <summary>
        ''' Update the underlying core data with edits from the interface
        ''' </summary>
        ''' <remarks>This is called by the core when a variable passes validation via cCore.OnValidated()</remarks>
        Public Function Update(ByVal DataType As eDataTypes) As Boolean Implements ISearchObjective.Update
            ' NOP
        End Function

        Friend Sub Clear() Implements ISearchObjective.Clear
            ' NOP
        End Sub

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

        Public ReadOnly Property ValueWeights() As cSearchObjectiveWeights Implements ISearchObjective.ValueWeights
            Get
                Return Me.m_searchObjective.ValueWeights
            End Get
        End Property

        Public ReadOnly Property ObjectiveParameters() As SearchObjectives.cSearchObjectiveParameters Implements SearchObjectives.ISearchObjective.ObjectiveParameters
            Get
                Return Me.m_searchObjective.ObjectiveParameters
            End Get
        End Property

#End Region ' ISearchObjective

#Region " ICoreInterface "

        Public ReadOnly Property DataType() As eDataTypes _
            Implements ICoreInterface.DataType
            Get
                Return eDataTypes.MSYManager
            End Get
        End Property

        Public ReadOnly Property CoreComponent() As eCoreComponentType _
            Implements ICoreInterface.CoreComponent
            Get
                Return eCoreComponentType.MSY
            End Get
        End Property

        Public Property DBID() As Integer _
            Implements ICoreInterface.DBID
            Get
                Return cCore.NULL_VALUE
            End Get
            Private Set(ByVal value As Integer)
                ' NOP
            End Set
        End Property

        Public Function GetID() As String _
            Implements ICoreInterface.GetID
            Return cCore.NULL_VALUE.ToString
        End Function

        Public Property Index() As Integer _
            Implements ICoreInterface.Index
            Get
                Return cCore.NULL_VALUE
            End Get
            Private Set(ByVal value As Integer)
                ' NOP
            End Set
        End Property

        Public Property Name() As String Implements ICoreInterface.Name
            Get
                Return "MSYmanager"
            End Get
            Private Set(ByVal value As String)
                ' NOP
            End Set
        End Property

#End Region ' ICoreInterface

#Region "cThreadWaitBase Overrides"

        Public Overrides Function StopRun(Optional ByVal WaitTimeInMillSec As Integer = -1) As Boolean ' Implements SearchObjectives.ISearchObjective.StopRun
            Dim result As Boolean = True

            If (Me.m_Core Is Nothing) Then Return True

            Try
                Me.m_msyData.bStopRun = True
                result = Me.Wait(WaitTimeInMillSec)
                Me.m_Core.SetStopRunDelegate(Nothing)
            Catch ex As Exception
                result = False
            End Try
            Return result
        End Function

#End Region

#Region " .Net Framework stuff "

        Protected Overrides Sub Finalize()
            Me.m_MSY.Disconnect()
            MyBase.Finalize()
        End Sub

#End Region ' .Net Framework stuff 

#Region " Unit tests "

        Public Sub RunMSYEcosimUnitTest()

            Try

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Run the MSY with no groups frozen and the selected group at some value (Ecopath base by default but that should be changed for testing)
                'Load an F time series for the selected group into Ecosim at the Ecopath Base
                'Run Ecosim  for the same number of years as we ran the MSY
                'We should be able to compare the Biomass outputs from both models
                'to see how they match up
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                Dim Fs() As Single
                Dim tsID As Integer
                Dim dsID As Integer = 1
                Dim tsName As String
                Dim dsTS As cTimeSeriesDataset
                Dim ActiveTS As cTimeSeries
                Dim FishMortForTest As Single = Me.m_Core.m_EcoSimData.Fish1(Me.m_msyData.iSelGroupFleet)

                Debug.Assert(Me.m_Core.nTimeSeriesDatasets > 0, "Oppss This model does not contain any Time Series Datasets. You can not run the MSY-Ecosim Unit Test.")
                If Me.m_Core.nTimeSeriesDatasets < 1 Then
                    Return
                End If

                System.Console.WriteLine("---------------Starting MSY Ecosim unit test---------------------")

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Run MSY
                Me.m_msyData.AssessmentType = eMSYAssessmentTypes.FullCompensation
                'Init MSY Varible
                Me.m_MSY.InitForSingleRun()
                'Init Ecosim Variables to run the RK4 without calling Ecosim.RunModelValue()
                Me.m_MSY.InitEcosimForRK4()

                Me.m_MSY.setFishingRates(FishMortForTest)

                'Run the RK4. This runs the core ecosim (derivt()) calculations without any support from the core or Ecosim it self
                Me.m_MSY.EcosimRK4(Me.m_msyData.nYearsPerTrial)
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Create an F time series from the Ecopath F for the selected group
                'All other timeseries will be disabled
                Me.m_Core.LoadTimeSeries(dsID, True)
                dsTS = Me.m_Core.TimeSeriesDataset(dsID)
                Debug.Assert(dsTS IsNot Nothing, "MSY-Ecosim Unit Test Dataset not found.")

                ReDim Fs(Me.m_msyData.nYears)
                For iyr As Integer = 0 To Me.m_msyData.nYears
                    Fs(iyr) = FishMortForTest
                Next

                tsName = "MSY_F_Test_" & Me.m_Core.m_EcoPathData.GroupName(Me.m_msyData.iSelGroupFleet)
                If Me.m_Core.AddTimeSeries(tsName, Me.m_msyData.iSelGroupFleet, 0, eTimeSeriesType.FishingMortality, 1.0, Fs, tsID) Then

                    Me.m_Core.LoadTimeSeries(Me.m_Core.ActiveTimeSeriesDatasetIndex, True)
                    dsTS = Me.m_Core.TimeSeriesDataset(dsID)

                    For its As Integer = 0 To Me.m_Core.nTimeSeries - 1
                        dsTS.Item(its).Enabled = False
                    Next

                    ActiveTS = dsTS.Item(Me.m_Core.nTimeSeries - 1)
                    ActiveTS.Enabled = True
                    Me.m_Core.UpdateTimeSeries()
                End If
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Run Ecosim
                Me.m_Core.EcoSimModelParameters.NumberYears = Me.m_msyData.nYearsPerTrial
                Dim ntimesteps As Integer = Me.m_Core.nEcosimTimeSteps
                Me.m_Core.m_EcoSimData.bTimestepOutput = True
                Me.m_Core.RunEcoSim()

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Dump out the comparison
                Dim ssError As Single
                System.Console.WriteLine("---------------MSY Ecosim unit test output---------------------")
                System.Console.WriteLine("Selected Group, " + Me.m_Core.m_EcoPathData.GroupName(Me.m_msyData.iSelGroupFleet))
                System.Console.WriteLine("Test F, " + FishMortForTest.ToString)

                System.Console.WriteLine("Group Name,F ,MSY/Ecosim , MSY Biomass, Ecosim Biomass")

                For igrp As Integer = 1 To Me.m_msyData.nGroups
                    Dim msyB As Single = Me.m_MSY.bb(igrp)
                    Dim simB As Single = Me.m_Core.m_EcoSimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, igrp, ntimesteps)
                    ssError += CSng((msyB - simB) ^ 2.0)
                    Dim msyError As Single = msyB / simB
                    Dim F As Single = Me.m_Core.m_EcoSimData.FishTime(igrp)
                    System.Console.WriteLine(Me.m_Core.m_EcoPathData.GroupName(igrp) + ", " + F.ToString + _
                                             ", " + msyError.ToString + ", " + msyB.ToString + ", " + simB.ToString)
                Next

                System.Console.WriteLine("Last timestep Sum of Square error, " + ssError.ToString)
                System.Console.WriteLine("---------------Done MSY Ecosim unit test---------------------")

                'Remove the time series we created above
                Me.m_Core.RemoveTimeSeries(ActiveTS)
                Me.m_Core.UpdateTimeSeries()

                'Unload the time series
                Me.m_Core.LoadTimeSeries(0)

            Catch ex As Exception
                cLog.Write(ex)
                System.Console.WriteLine(Me.ToString & ".RunMSYUnitTest() Exception: " & ex.Message)
            End Try

        End Sub

#End Region ' Unit tests

    End Class

End Namespace
