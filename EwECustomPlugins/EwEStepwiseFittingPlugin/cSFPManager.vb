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
'    UBC Centre for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
'
' Stepwise Fitting Procedure by Sheila Heymans, Erin Scott, Jeroen Steenbeek
' Copyright 2015- Scottish Association for Marine Science, Oban, Scotland
'
' Erin Scott was funded by the Scottish Informatics and Computer Science
' Alliance (SICSA) Postgraduate Industry Internship Programme.
' ===============================================================================
'
#Region " Imports "

Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports EwECore
Imports EwECore.Ecosim
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

Public Class cSFPManager

    Private m_core As cCore
    Private m_strModelFileName As String
    Private m_scenario As cEcoSimScenario
    Private m_iTimeSeries As Integer
    Private m_parameters As cSFPParameters

    Private m_iterations As New List(Of ISFPIterations)
    Private m_bIsBaseline As Boolean = True
    Private m_bIsFishing As Boolean = False

    Private m_frmMain As Form

    ' -- State flags --

    ''' <summary>Flag, stating whether a run is in progress.</summary>
    Private m_bIsRunning As Boolean = False
    ''' <summary>Flag, stating whether a run abortion has been requested.</summary>
    Private m_bStopRun As Boolean = False

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for stand-alone app
    ''' </summary>
    ''' <remarks>
    ''' In this modus, the SFP manager is in full control over its own core.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Sub New()
        'Create a new core
        Me.New(New cCore(), Nothing)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor when used in a plug-in environment.
    ''' </summary>
    ''' <remarks>
    ''' In this modus, the SFP manager adheres to choices made in a core managed
    ''' by EwE.
    ''' </remarks>
    ''' <param name="core">The core instance to initialize to.</param>
    ''' <param name="mFrm">The main UI form to use for thread marshalling.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore, ByVal mFrm As Form)
        Me.m_core = core
        Me.Parameters = New cSFPParameters(core)
        Me.m_frmMain = mFrm
    End Sub

#Region " Load user Inputs "

    ''' <summary>
    ''' Load the model in the selected file and keep a reference of the file path
    ''' </summary>
    ''' <returns>True if load successful</returns>
    Public Function LoadModel(ByVal strFileName As String) As Boolean
        Me.m_strModelFileName = strFileName
        Return Me.m_core.LoadModel(m_strModelFileName)
    End Function

    ''' <summary>
    ''' Load the Ecosim Scenario from selected index and keep a reference of the scenario
    ''' </summary>
    ''' <param name="iScenario">One-based Ecosim scenario index.</param>
    ''' <returns>True if load successful</returns>
    Public Function LoadEcoSimScenario(ByVal iScenario As Integer) As Boolean

        'Try to load scenario
        If m_core.LoadEcosimScenario(iScenario) Then
            'Store a reference to scenario in SFPManager
            m_scenario = m_core.EcosimScenarios(iScenario)
            Return True
        End If
        Return False

    End Function

    ''' <summary>
    ''' Gets a list of names of all the Ecosim Scenarios from the core
    ''' </summary>
    ''' <returns>String List of Ecosim Scenario names </returns>
    Public Function GetAvailableScenarioNames() As List(Of String)
        Dim lscenarios As New List(Of String)
        Dim scenario As cEcoSimScenario

        For iScenario As Integer = 1 To m_core.nEcosimScenarios
            scenario = m_core.EcosimScenarios(iScenario)
            lscenarios.Add(scenario.Name)
        Next
        Return lscenarios
    End Function

    ''' <summary>
    ''' Load the Time Series from selected index and keep a reference of the Time Series
    ''' </summary>
    ''' <param name="tsi">One-based time series dataset index, just as used in the
    ''' EwE core.</param>
    ''' <returns>True if load successful</returns>
    Public Function LoadTimeSeries(ByVal tsi As Integer) As Boolean

        Dim bSuccess As Boolean = False

        'Try to load time series
        If m_core.LoadTimeSeries(tsi) Then
            'Store a reference to time series index in SFPManager
            Me.m_iTimeSeries = tsi
            Console.WriteLine("Time Series : " & m_core.TimeSeriesDataset(tsi).Name & " Loaded successfully")
            bSuccess = True
        Else
            Console.WriteLine("Time Series could not Load")
            Me.m_iTimeSeries = -1
            bSuccess = False
        End If

        Me.Refresh(0)
        Return bSuccess

    End Function

    ''' <summary>
    ''' Gets a list of names of all the Time Series from the core
    ''' </summary>
    ''' <returns>String List of Time Series names </returns>
    Public Function GetAvailableTimeSeriesNames() As List(Of String)
        Dim lTimeSeries As New List(Of String)
        Dim TimeSeries As cTimeSeriesDataset = Nothing

        For iTimeSeries As Integer = 1 To m_core.nTimeSeriesDatasets
            TimeSeries = m_core.TimeSeriesDataset(iTimeSeries)
            lTimeSeries.Add(TimeSeries.Name)
        Next
        Return lTimeSeries
    End Function

    Public Function GetAvailableAnomalyShapes() As cShapeData()

        Dim interactions As cMediatedInteractionManager = m_core.MediatedInteractionManager
        Dim shapes As New List(Of cShapeData)

        Dim lPP As New List(Of Integer)
        For iGroup As Integer = 1 To m_core.nGroups
            Dim grp As cEcoPathGroupInput = m_core.EcoPathGroupInputs(iGroup)
            If (grp.IsProducer) Then
                lPP.Add(iGroup)
            End If
        Next

        For Each iGroup As Integer In lPP
            Dim interact As cPredPreyInteraction = interactions.PredPreyInteraction(iGroup, iGroup)
            If (interact IsNot Nothing) Then
                Dim shape As cForcingFunction = Nothing
                Dim ft As eForcingFunctionApplication = eForcingFunctionApplication.NotSet
                For i As Integer = 1 To interact.nAppliedShapes
                    If (interact.getShape(i, shape, ft)) Then
                        If (Not shapes.Contains(shape)) Then
                            shapes.Add(shape)
                        End If
                    End If
                Next i
            End If
        Next iGroup
        Return shapes.ToArray()

    End Function

    ''' <summary>
    ''' Set the value of PredOrPredPreySSToV from selected String
    ''' </summary>
    Public Sub SetPredOrPredPreySSToV(ByVal SSToVChoice As String)

        ' ToDo: how about using an enum here? ;)

        Dim choice As String = SSToVChoice
        Select Case choice

            Case "Predator"
                Me.Parameters.PredOrPredPreySSToV = True
                'Console.WriteLine("Sensitivity of SS to V set by : " & choice)

            Case "Predator/Prey"
                Me.Parameters.PredOrPredPreySSToV = False
                'Console.WriteLine("Sensitivity of SS to V set by : " & choice)

        End Select

    End Sub

    Public Property K As Integer
        Get
            Return Me.Parameters.K
        End Get
        Set(value As Integer)
            If (value <> Me.Parameters.K) Then
                Me.Parameters.K = value
                Me.Refresh(Me.K)
            End If
        End Set
    End Property

    Public Property AnomalySearchSplineStepSize As Integer
        Get
            Return Me.Parameters.AnomalySearchSplineStepSize
        End Get
        Set(ByVal value As Integer)
            If (value <> Me.Parameters.AnomalySearchSplineStepSize) Then
                Me.Parameters.AnomalySearchSplineStepSize = value
                Me.Refresh(Me.K)
            End If
        End Set
    End Property

    Public Sub Refresh(ByVal iPrefK As Integer)
        ' Always do this
        Me.Parameters.CalculateParameters(iPrefK)
        ' Create list of ISFPIterations
        Me.LoadSFPIterationsList()
    End Sub

#End Region ' Load user inputs

#Region " Load to EwE state "

    Public Sub UpdateToCore()

        ' Sanity checks
        Debug.Assert(Me.m_core IsNot Nothing)
        If (Me.m_core.StateMonitor.HasEcosimLoaded) Then
            Me.m_scenario = Me.m_core.EcosimScenarios(Me.m_core.ActiveEcosimScenarioIndex)
            Me.m_iTimeSeries = Me.m_core.ActiveTimeSeriesDatasetIndex
            Me.Refresh(0)
        Else
            Me.m_scenario = Nothing
            Me.m_iTimeSeries = -1
            Me.m_iterations.Clear()
        End If

    End Sub

#End Region ' Load to EwE state

#Region " Run Iterations "

    Public Sub RunSFPIterationsThreaded()

        If (Me.IsRunning) Then Return

        Dim thread As New Threading.Thread(AddressOf RunSFPIterations)
        thread.Start()

    End Sub

    Public Sub RunSFPIterations()

        If (Me.IsRunning) Then Return

        Me.m_bIsRunning = True
        Me.m_bStopRun = False

        Dim iNumSteps As Integer = 1
        Dim iStep As Integer = 0
        Dim msg As New cMessage(cStringUtils.Localize(My.Resources.STATUS_SAVE_SUCCESS, My.Resources.CAPTION), _
                                eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
        msg.Hyperlink = Me.OutputFolder
        Dim bSuccess As Boolean

        'Debug.WriteLine("Number of Observations = " & Parameters.NumberOfObservations)

        ' Set a core batch lock to keep the state monitor busy. This keeps the stop option enabled
        ' Note: the busy flag depends either on the core search mode or a core batch lock.
        '       The search mode is reset every time a F2TS search finishes, which is not practical for our purposes
        '       Batch locks are incremental, which is why the batch lock is chosen here instead of the search mode.

        Me.m_core.SetBatchLock(cCore.eBatchLockType.Update)
        Me.m_core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf Me.StopRun))
        cApplicationStatusNotifier.StartProgress(Me.m_core)

        Try

            ' Clear all
            For Each Iteration As ISFPIterations In m_iterations
                Iteration.RunState = ISFPIterations.eRunState.Idle
                Iteration.IsBestFit = False
                If (Iteration.Enabled) Then iNumSteps += 1
            Next

            'Go through each iteration
            For Each Iteration As ISFPIterations In m_iterations

                'Check if iteration is enabled to run
                If Iteration.Enabled And Not m_bStopRun Then

                    cApplicationStatusNotifier.UpdateProgress(Me.m_core, _
                                                              cStringUtils.Localize(My.Resources.STATUS_RUNNING, _
                                                                                    My.Resources.CAPTION, _
                                                                                    Iteration.Name), _
                                                              CSng((iStep + 0.5) / iNumSteps))

                    ' Assume the worst
                    bSuccess = False

                    'Changed Runstate to running instead of error as it confuses the users
                    Iteration.RunState = ISFPIterations.eRunState.Running
                    Iteration.Init(m_core, m_iTimeSeries, Me.Parameters.PredOrPredPreySSToV, Parameters, m_frmMain)
                    Iteration.Load()
                    If Iteration.Run() Then
                        If (Not m_bStopRun) Then
                            Debug.WriteLine(Iteration.Name & " SS= " & Iteration.SS & " AIC= " & Iteration.AIC & " AICc= " & Iteration.AICc & ", " & Iteration.RunState)

                            ' Make sure Iteration will save
                            Iteration.RunState = ISFPIterations.eRunState.Completed
                            ' Save Ecosim results if requested
                            SaveIterationResults(Iteration, msg)
                            ' Save content of iteration for later reloading
                            SaveIterationConfiguration(Iteration, msg)
                            ' Determine the best fitting iteration
                            DetermineBestFit()

                            bSuccess = True
                        End If
                    End If

                    ' Finalize iteration status
                    If Not bSuccess Then
                        Iteration.RunState = ISFPIterations.eRunState.Error
                    End If

                    Iteration.Clear()
                    SendIterationCompleted(Iteration)

                    iStep += 1

                Else
                    Debug.WriteLine(Iteration.Name & ", " & Iteration.RunState)
                End If
            Next

            'Save results to CSV file
            iStep += 1
            cApplicationStatusNotifier.UpdateProgress(Me.m_core, _
                                                      cStringUtils.Localize(My.Resources.STATUS_SAVING, My.Resources.CAPTION), _
                                                      sProgress:=CSng(iStep / iNumSteps))
            SaveResultsToCSV(msg)

            If (Me.Parameters.AutosaveMode <> cSFPParameters.eAutosaveMode.None) Then
                SaveAllAnomalyResultsToCSV(msg)
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        Me.m_core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.NotSet)
        Me.m_core.SetStopRunDelegate(Nothing)
        cApplicationStatusNotifier.EndProgress(Me.m_core)

        Me.m_bIsRunning = False
        SendIterationCompleted(Nothing)

        If (msg IsNot Nothing) Then
            If msg.Importance = eMessageImportance.Critical Then
                msg.Message = cStringUtils.Localize(My.Resources.STATUS_SAVE_FAILED, My.Resources.CAPTION)
            End If
            Me.m_core.Messages.SendMessage(msg)
        End If

        'Reload Ecosim Scenario 
        'If we reload scenario the iteration list disappears!
        'LoadEcoSimScenario(EcoSimScenario.Index)
        'LoadTimeSeries(TimeSeriesIndex)

        'Load best fitted iteration
        For Each Iteration As ISFPIterations In m_iterations
            If Iteration.IsBestFit Then
                Iteration.Apply()
                'LoadIterationConfiguration(Iteration)
                Exit For
            End If
        Next

    End Sub

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return Me.m_bIsRunning
        End Get
    End Property

    Public Sub StopRun()

        If (Me.IsRunning) Then
            Me.m_bStopRun = True
            Me.m_core.EcosimFitToTimeSeries.StopRun()
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event to notify that an iteraton has been completed during a <see cref="IsRunning">run</see>.
    ''' <see cref="RunSFPIterations"/>
    ''' <see cref="IsRunning"/>
    ''' <see cref="StopRun"/>
    ''' </summary>
    ''' <param name="sender">This class.</param>
    ''' <param name="iteration">The iteration that completed.</param>
    ''' -----------------------------------------------------------------------
    Public Event OnIterationCompleted(ByVal sender As cSFPManager, ByVal iteration As ISFPIterations)

    Private Sub SendIterationCompleted(ByVal iteration As ISFPIterations)
        Try
            ' Notify the world that the run is over
            RaiseEvent OnIterationCompleted(Me, Nothing)
        Catch ex As Exception
            ' This should not happen
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    Private Sub LoadSFPIterationsList()

        m_iterations.Clear()

        'Only add iterations if time series is loaded
        If TSIndex >= 1 Then

            'Load Fishing iteration
            m_iterations.Add(New cSFPEcosimRun(m_bIsFishing))

            'Load Fishing Vunerability Search iterations
            For i = Parameters.MinK To Parameters.K
                m_iterations.Add(New cSFPVulnerabilitySearch(m_bIsFishing, i))
            Next

            'If there is a current FF applied to PP
            If (Parameters.AppliedShape IsNot Nothing) Then

                'Load Fishing Anomaly Search iterations
                For i = Parameters.MinSplinePoints To Parameters.MaxSplinePoints Step Parameters.AnomalySearchSplineStepSize
                    m_iterations.Add(New cSFPAnomalySearch(m_bIsFishing, i))
                Next

                'Load Fishing V and A Search iterations
                For i = Parameters.MinK To Parameters.K
                    For j = Parameters.MinSplinePoints To Parameters.MaxSplinePoints Step Parameters.AnomalySearchSplineStepSize
                        Dim estParams As Integer = i + j
                        If estParams <= Parameters.K Then
                            m_iterations.Add(New cSFPVandASearch(m_bIsFishing, i, j))
                        End If
                    Next
                Next

            End If

            'Load Baseline iteration
            m_iterations.Add(New cSFPEcosimRun(m_bIsBaseline))

            'Load Baseline Vunerability Search iterations
            For i = Parameters.MinK To Parameters.K
                m_iterations.Add(New cSFPVulnerabilitySearch(m_bIsBaseline, i))
            Next

            'If there is a current FF applied to PP
            If (Parameters.AppliedShape IsNot Nothing) Then

                'Load Baseline Anomaly Search iterations
                For i = Parameters.MinSplinePoints To Parameters.MaxSplinePoints Step Parameters.AnomalySearchSplineStepSize
                    m_iterations.Add(New cSFPAnomalySearch(m_bIsBaseline, i))
                Next

                'Load Baseline V and A Search iterations
                For i = Parameters.MinK To Parameters.K
                    For j = Parameters.MinSplinePoints To Parameters.MaxSplinePoints Step Parameters.AnomalySearchSplineStepSize
                        Dim estParams As Integer = i + j
                        If estParams <= Parameters.K Then
                            m_iterations.Add(New cSFPVandASearch(m_bIsBaseline, i, j))
                        End If
                    Next
                Next
            End If

        End If

    End Sub

    Private Sub DetermineBestFit()

        Dim BestAICc As Single = Single.MaxValue
        Dim BestIteration As ISFPIterations = Nothing

        ' Clear all best fit flags, and determine the best fit
        For Each it As ISFPIterations In Me.Iterations
            it.IsBestFit = False
            If (it.RunState = ISFPIterations.eRunState.Completed) And (it.AICc < BestAICc) Then
                BestIteration = it
                BestAICc = it.AICc
            End If
        Next

        ' Set best fit
        If (BestIteration IsNot Nothing) Then
            BestIteration.IsBestFit = True
        End If

    End Sub

#End Region ' Run Iterations

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get an array with all available <see cref="ISFPIterations"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Iterations As ISFPIterations()
        Get
            Return Me.m_iterations.ToArray()
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the one instance of run configuration <see cref="cSFPParameters"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Parameters As cSFPParameters
        Get
            Return Me.m_parameters
        End Get
        Private Set(ByVal value As cSFPParameters)
            Me.m_parameters = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the one-based index of the currently loaded <see cref="cTimeSeriesDataset"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property TSIndex As Integer
        Get
            Return Me.m_core.ActiveTimeSeriesDatasetIndex
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the output folder for storing Stepwise Fitting results to.
    ''' <seealso cref="cSFPParameters.CustomOutputFolder"/>
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property OutputFolder As String
        Get
            If String.IsNullOrWhiteSpace(Me.Parameters.CustomOutputFolder) Then
                Return Path.Combine(Me.m_core.DefaultOutputPath(eAutosaveTypes.Ecosim), cFileUtils.ToValidFileName(My.Resources.CAPTION, False))
            End If
            Return Me.Parameters.CustomOutputFolder
        End Get
    End Property

    Public ReadOnly Property Core As cCore
        Get
            Return Me.m_core
        End Get
    End Property

#End Region ' Public access

#Region " File IO "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save iteration results to CSV.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function SaveResultsToCSV(ByVal msg As cMessage) As Boolean

        ' Note on globalization: 
        '  - All messages presented to users should be localized, e.g., obtained from the resources;
        '  - All text written to CSV files is written in English, and cannot be localized in case EwE needs to parse this data one day.
        '  - File names are thus also not localized.

        Dim strPath As String = Me.OutputFolder
        Dim CSVfileSimple As String = Path.Combine(strPath, "Stepwise_Fitting_Procedure_Iteration_Results.csv")
        Dim writer As StreamWriter = Nothing
        Dim bSuccess As Boolean = True
        Dim TimeSeries As cTimeSeriesDataset = m_core.TimeSeriesDataset(m_core.ActiveTimeSeriesDatasetIndex)

        If cFileUtils.IsDirectoryAvailable(strPath, True) Then

            ' ToDo: clear the content of the directory?

            Try
                writer = New StreamWriter(CSVfileSimple)
            Catch ex As Exception
                Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_FAILED, My.Resources.DETAIL_SUMMARY, ex.Message), eStatusFlags.ErrorEncountered)
                bSuccess = False
            End Try

            If (writer IsNot Nothing) Then

                ' Include default header if needed
                If Me.m_core.SaveWithFileHeader Then
                    writer.WriteLine(m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                    writer.WriteLine(cStringUtils.ToCSVField("Number of Observations") & "," & cStringUtils.ToCSVField(Parameters.NumberOfObservations))
                End If

                ' -- Write header --
                writer.WriteLine(",,,,,,,{0}", cStringUtils.ToCSVField("Time Series SS results"))
                writer.Write("Name,K,NVs,NSpline,SS,AIC,AICc")
                For i As Integer = 1 To TimeSeries.nTimeSeries
                    writer.Write("," & cStringUtils.ToCSVField(TimeSeries.TimeSeries(i).Name))
                Next
                writer.WriteLine()

                Try

                    'Go through each iteration_EC
                    For Each Iteration As ISFPIterations In m_iterations
                        If (Iteration.RunState = ISFPIterations.eRunState.Completed) Then

                            ' Write iteration info line
                            writer.Write(cStringUtils.ToCSVField(Iteration.Name) & "," & _
                                         cStringUtils.ToCSVField(Iteration.K) & "," & _
                                         cStringUtils.ToCSVField(Iteration.EstimatedV) & "," & _
                                         cStringUtils.ToCSVField(Iteration.SplinePoints) & "," & _
                                         cStringUtils.ToCSVField(Iteration.SS) & "," & _
                                         cStringUtils.ToCSVField(Iteration.AIC) & "," & _
                                         cStringUtils.ToCSVField(Iteration.AICc))

                            For i As Integer = 1 To TimeSeries.nTimeSeries
                                writer.Write(",")
                                If (Iteration.TimeSeriesSS(i) > 0) Then
                                    writer.Write(cStringUtils.ToCSVField(Iteration.TimeSeriesSS(i)))
                                End If
                            Next
                            writer.WriteLine()
                        End If
                    Next
                    Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_SUCCESS, My.Resources.DETAIL_SUMMARY, CSVfileSimple), eStatusFlags.OK)
                Catch ex As Exception
                    Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_FAILED, My.Resources.DETAIL_SUMMARY, ex.Message), eStatusFlags.ErrorEncountered)
                End Try

                writer.Close()

            End If
        Else
            Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.FAILURE_DIRECTORY, strPath), eStatusFlags.ErrorEncountered)
        End If

        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save Ecosim run results of an iteration to file.
    ''' </summary>
    ''' <param name="iteration">The iteration that needs saving.</param>
    ''' <param name="msg">Status message to append information to.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function SaveIterationResults(ByVal iteration As ISFPIterations, ByVal msg As cMessage) As Boolean

        ' Sanity checks
        Debug.Assert(iteration IsNot Nothing)

        Dim strIterationPath As String = Path.Combine(Me.OutputFolder, cFileUtils.ToValidFileName(iteration.Name, False))
        Dim bSuccess As Boolean = True

        If (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.Ecosim) Or _
           (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.All) Then

            If cFileUtils.IsDirectoryAvailable(strIterationPath, True) Then
                Dim wsim As New Ecosim.cEcosimResultWriter(Me.m_core)
                Try
                    If wsim.WriteResults(strIterationPath, bQuiet:=True) Then
                        Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_SUCCESS, My.Resources.DETAIL_ECOSIM, strIterationPath), eStatusFlags.OK)
                        bSuccess = True
                    Else
                        Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_FAILED, My.Resources.DETAIL_ECOSIM, ""), eStatusFlags.ErrorEncountered)
                        bSuccess = False
                    End If
                Catch ex As Exception
                    ' This REALLY should not happen
                    cLog.Write(ex, "cSFPManager.SaveIterationResults(Ecosim)")
                    Debug.Assert(False, ex.Message)
                End Try
            End If
        End If

        If (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.Aggregated) Or _
           (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.All) Then

            'Save output results in Monthly and Yearly format 
            SaveAggregatedResults(iteration, True, msg)
            SaveAggregatedResults(iteration, False, msg)

        End If

        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get all group names from Ecosim run and return them as a comma separated string
    ''' </summary>
    ''' <returns>String of comma separated group names.</returns>
    ''' -----------------------------------------------------------------------
    Private Function GetAllGroupNames() As String

        Dim str As New StringBuilder()

        For i As Integer = 1 To m_core.nGroups
            str.Append(cStringUtils.ToCSVField(m_core.EcoSimGroupOutputs(i).Name))
            If i <> m_core.nGroups Then str.Append(",")
        Next

        Return str.ToString()

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save specific Ecosim results (Biomass,Mortality and Yield) of iteration to a CSV file.
    ''' </summary>
    ''' <param name="iteration"> The iteration the results come from </param>
    ''' <param name="tsMonthly"> True for results to be saved monthly and false to save annually </param>
    ''' <param name="msg"> The message to append status information to </param>
    ''' <returns>Always returns true.</returns>
    ''' -----------------------------------------------------------------------
    Private Function SaveAggregatedResults(ByVal iteration As ISFPIterations, _
                                           ByVal tsMonthly As Boolean, _
                                           ByVal msg As cMessage) As Boolean

        For Each outputtype As cEcosimResultWriter.eResultTypes In [Enum].GetValues(GetType(cEcosimResultWriter.eResultTypes))
            Select Case outputtype
                Case cEcosimResultWriter.eResultTypes.Biomass, _
                     cEcosimResultWriter.eResultTypes.Mortality, _
                     cEcosimResultWriter.eResultTypes.Catch
                    SaveAggregatedTypeResult(outputtype, iteration, tsMonthly, msg)
            End Select
        Next

        Return True
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save a specific result (Biomass,Mortality or Yield) of iteration to a CSV file.
    ''' </summary>
    ''' <param name="ResultType">The Result type to save.</param>
    ''' <param name="iteration">The iteration the results come from.</param>
    ''' <param name="tsMonthly">True for results to be saved monthly and false to save annually.</param>
    ''' <param name="msg">The message to append status information to.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function SaveAggregatedTypeResult(ByVal ResultType As cEcosimResultWriter.eResultTypes, _
                                              ByVal iteration As ISFPIterations, _
                                              ByVal tsMonthly As Boolean, _
                                              ByVal msg As cMessage) As Boolean

        ' Note on globalization: 
        '  - All messages presented to users should be localized, e.g., obtained from the resources;
        '  - All text written to CSV files is written in English, and cannot be localized in case EwE needs to parse this data one day.
        '  - File names are thus also not localized.

        Dim strPath As String = Me.OutputFolder
        Dim CSVfile As String
        'Set file name
        If (tsMonthly) Then
            CSVfile = Path.Combine(strPath, iteration.Name + "_" + ResultType.ToString + ".csv")
        Else
            CSVfile = Path.Combine(strPath, iteration.Name + "_" + ResultType.ToString + "_Annual.csv")
        End If

        Dim writer As StreamWriter = Nothing
        Dim bSuccess As Boolean = True
        Dim data(m_core.nGroups, m_core.nEcosimTimeSteps) As Single
        Dim grpOutput As cEcosimGroupOutput = Nothing
        Dim GroupNames As String = Me.GetAllGroupNames()

        If cFileUtils.IsDirectoryAvailable(strPath, True) Then

            ' ToDo: clear the content of the directory?

            Try
                writer = New StreamWriter(CSVfile)
            Catch ex As Exception
                Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_FAILED, My.Resources.DETAIL_ITERATION_AGGREGATED, ex.Message), eStatusFlags.ErrorEncountered)
                bSuccess = False
            End Try

            If (writer IsNot Nothing) Then

                ' Include default header if needed
                If Me.m_core.SaveWithFileHeader Then
                    writer.WriteLine(m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                End If


                ' -- Write header --
                writer.WriteLine("Iteration Name," + iteration.Name)
                writer.WriteLine("Data," + ResultType.ToString)
                writer.WriteLine()
                writer.WriteLine(GroupNames)

                Try

                    If (iteration.RunState = ISFPIterations.eRunState.Completed) Then

                        For i As Integer = 1 To m_core.nGroups
                            grpOutput = m_core.EcoSimGroupOutputs(i)
                            For j As Integer = 1 To m_core.nEcosimTimeSteps
                                Select Case ResultType
                                    Case cEcosimResultWriter.eResultTypes.Biomass
                                        data(i, j) = grpOutput.Biomass(j)
                                    Case cEcosimResultWriter.eResultTypes.Mortality
                                        data(i, j) = grpOutput.TotalMort(j)
                                    Case cEcosimResultWriter.eResultTypes.Catch
                                        data(i, j) = grpOutput.Catch(j)
                                End Select
                            Next
                        Next

                        'Output Monthly
                        If (tsMonthly) Then
                            'Each time steps
                            For j As Integer = 1 To data.GetLength(1) - 1
                                'For every group
                                For i As Integer = 1 To data.GetLength(0) - 1
                                    If i > 1 Then writer.Write(", ")
                                    writer.Write(cStringUtils.FormatSingle(data(i, j)))
                                Next
                                writer.WriteLine()
                            Next
                        Else ' Output Yearly
                            Dim simYears As Integer = CInt(Math.Floor((data.GetLength(1) - 1) / cCore.N_MONTHS))
                            Dim nGroups As Integer = data.GetLength(0) - 1
                            Dim sum(nGroups) As Single
                            For j As Integer = 1 To simYears
                                For i As Integer = 1 To nGroups
                                    For k As Integer = 1 To cCore.N_MONTHS
                                        If (k = 1) Then sum(i) = 0
                                        sum(i) += data(i, (j - 1) * cCore.N_MONTHS + k)
                                    Next
                                    If i > 1 Then writer.Write(", ")
                                    writer.Write(cStringUtils.FormatSingle(sum(i) / cCore.N_MONTHS))
                                Next
                                writer.WriteLine()
                            Next
                        End If

                        ' ToDo: Consider if we also need to write any information if iterations somehow failed. The run is not complete then...
                        Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_SUCCESS, My.Resources.DETAIL_ITERATION_AGGREGATED, CSVfile), eStatusFlags.OK)
                    End If

                Catch ex As Exception
                    Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_FAILED, My.Resources.DETAIL_ITERATION_AGGREGATED, ex.Message), eStatusFlags.ErrorEncountered)
                    bSuccess = False
                End Try

                writer.Close()

            End If
        Else
            ' Panic!
            Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.FAILURE_DIRECTORY, strPath), eStatusFlags.ErrorEncountered)
        End If

        Return bSuccess
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save the configuration of an iteration to file for later reloading.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function SaveIterationConfiguration(ByVal iteration As ISFPIterations, ByVal msg As cMessage) As Boolean

        ' Sanity checks
        Debug.Assert(iteration IsNot Nothing)

        Dim strIterationPath As String = Path.Combine(Me.OutputFolder, cFileUtils.ToValidFileName(iteration.Name, False))
        Dim writer As StreamWriter = Nothing
        Dim bSuccess As Boolean = True

        ' Abort if not ran completed
        ' Note that this assumes that the directory is vigin territory... failed iterations are not obliterated. EwE always makes this harsh assumption, eek
        If (Not iteration.RunState = ISFPIterations.eRunState.Completed) Then Return False

        If cFileUtils.IsDirectoryAvailable(strIterationPath, True) Then

            writer = New StreamWriter(Path.Combine(strIterationPath, ".classname"))
            writer.WriteLine(iteration.GetType().ToString)
            writer.Close()

            'Save vulnerabilities configuartion
            writer = New StreamWriter(Path.Combine(strIterationPath, ".vulnerabilities"))
            If (iteration.Vulnerabilities IsNot Nothing) Then
                For i As Integer = 1 To m_core.nGroups
                    If (i > 1) Then writer.WriteLine()
                    For j As Integer = 1 To m_core.nGroups
                        If (j > 1) Then writer.Write(",")
                        writer.Write(cStringUtils.ToCSVField(iteration.Vulnerabilities(i, j)))
                    Next
                Next
            End If
            writer.Close()

            'Output vulnerabilities to a csv file
            'If ecosim or all output is selected save the csv file to the named iteration folder
            If (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.Ecosim) Or
               (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.All) Then
                writer = New StreamWriter(Path.Combine(strIterationPath, "Vulnerabilities.csv"))
                If (iteration.Vulnerabilities IsNot Nothing) Then
                    ' Include default header if needed
                    If Me.m_core.SaveWithFileHeader Then
                        writer.WriteLine(m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                    End If

                    ' -- Write header --
                    writer.WriteLine("Iteration Name," + iteration.Name)
                    writer.WriteLine("Data,Vulnerabilities")
                    writer.WriteLine()

                    For i As Integer = 1 To m_core.nGroups
                        If (i > 1) Then writer.WriteLine()
                        For j As Integer = 1 To m_core.nGroups
                            If (j > 1) Then writer.Write(",")
                            writer.Write(cStringUtils.ToCSVField(iteration.Vulnerabilities(i, j)))
                        Next
                    Next
                End If
                writer.Close()
            End If

            'If aggregated or all output is selected save the csv file to the named iteration folder
            If (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.Aggregated) Or
               (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.All) Then
                Dim strPath As String = Me.OutputFolder
                If cFileUtils.IsDirectoryAvailable(strPath, True) Then
                    writer = New StreamWriter(Path.Combine(strPath, iteration.Name + "_Vulnerabilities.csv"))
                    If (iteration.Vulnerabilities IsNot Nothing) Then
                        ' Include default header if needed
                        If Me.m_core.SaveWithFileHeader Then
                            writer.WriteLine(m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                        End If

                        ' -- Write header --
                        writer.WriteLine("Iteration Name," + iteration.Name)
                        writer.WriteLine("Data,Vulnerabilities")
                        writer.WriteLine()

                        For i As Integer = 1 To m_core.nGroups
                            If (i > 1) Then writer.WriteLine()
                            For j As Integer = 1 To m_core.nGroups
                                If (j > 1) Then writer.Write(",")
                                writer.Write(cStringUtils.ToCSVField(iteration.Vulnerabilities(i, j)))
                            Next
                        Next
                    End If
                    writer.Close()
                End If
            End If

            'Save anomaly shape configuartion
            writer = New StreamWriter(Path.Combine(strIterationPath, ".anomaly"))
            If (iteration.AnomalyShape IsNot Nothing) Then
                For i As Integer = 0 To iteration.AnomalyShape.Length - 1
                    If (i >= 1) Then writer.Write(",")
                    writer.Write(cStringUtils.ToCSVField(iteration.AnomalyShape(i)))
                Next
            End If
            writer.Close()

            'Output Anomaly to a csv file
            'If ecosim or all output is selected save the csv file to the named iteration folder
            If (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.Ecosim) Or
               (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.All) Then
                writer = New StreamWriter(Path.Combine(strIterationPath, "Anomaly.csv"))
                If (iteration.AnomalyShape IsNot Nothing) Then
                    ' Include default header if needed
                    If Me.m_core.SaveWithFileHeader Then
                        writer.WriteLine(m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                    End If

                    ' -- Write header --
                    writer.WriteLine("Iteration Name," + iteration.Name)
                    writer.WriteLine("Data,Anomaly")
                    writer.WriteLine()
                    For i As Integer = 0 To iteration.AnomalyShape.Length - 1
                        If (i >= 1) Then writer.Write(",")
                        writer.Write(cStringUtils.ToCSVField(iteration.AnomalyShape(i)))
                    Next
                End If
                writer.Close()
            End If

            'If aggregated or all output is selected save the csv file to the named iteration folder
            If (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.Aggregated) Or
               (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.All) Then
                Dim strPath As String = Me.OutputFolder
                If cFileUtils.IsDirectoryAvailable(strPath, True) Then
                    writer = New StreamWriter(Path.Combine(strPath, iteration.Name + "_Anomaly.csv"))
                    If (iteration.AnomalyShape IsNot Nothing) Then
                        ' Include default header if needed
                        If Me.m_core.SaveWithFileHeader Then
                            writer.WriteLine(m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                        End If

                        ' -- Write header --
                        writer.WriteLine("Iteration Name," + iteration.Name)
                        writer.WriteLine("Data,Anomaly")
                        writer.WriteLine()

                        For i As Integer = 0 To iteration.AnomalyShape.Length - 1
                            If (i >= 1) Then writer.Write(",")
                            writer.Write(cStringUtils.ToCSVField(iteration.AnomalyShape(i)))
                        Next
                    End If
                    writer.Close()
                End If
            End If

            Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_SUCCESS, My.Resources.DETAIL_ITERATION_CONFIG, strIterationPath), eStatusFlags.OK)

        End If
        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Re-populate an iteration from file.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <param name="iteration">The iteration to repopulate.</param>
    ''' -----------------------------------------------------------------------
    Private Function LoadIterationConfiguration(ByVal iteration As ISFPIterations) As Boolean

        Dim strSimPath As String = Path.Combine(Me.OutputFolder, cFileUtils.ToValidFileName(iteration.Name, False))
        Dim bSuccess As Boolean = True

        If cFileUtils.IsDirectoryAvailable(strSimPath, False) Then

            ' -- Class name validation --
            Try
                Using reader As New StreamReader(Path.Combine(strSimPath, ".classname"))
                    Dim strClassName As String = reader.ReadLine().Trim()
                    bSuccess = (String.Compare(iteration.GetType().ToString(), strClassName, True) = 0)
                    reader.Close()
                End Using
            Catch ex As Exception
                bSuccess = False
            End Try

            ' -- Vulnerabilities --
            Try
                Using reader As New StreamReader(Path.Combine(strSimPath, ".vulnerabilities"))
                    Debug.Assert(iteration.Vulnerabilities IsNot Nothing)
                    For i As Integer = 1 To m_core.nGroups
                        Dim strLine As String = reader.ReadLine().Trim()
                        Dim astrValues As String() = cStringUtils.SplitQualified(strLine, ","c)
                        For j As Integer = 1 To m_core.nGroups
                            iteration.Vulnerabilities(i, j) = cStringUtils.ConvertToSingle(astrValues(j - 1))
                        Next
                    Next
                End Using


            Catch ex As Exception
                ' Let this code blunder into array bounds etc. No neat error trapping for now, we can always improve this checking later
                bSuccess = False
            End Try

            ' -- Anomaly shape --
            Try
                Using reader As New StreamReader(Path.Combine(strSimPath, ".anomaly"))

                    Debug.Assert(iteration.AnomalyShape IsNot Nothing)

                    Dim strLine As String = reader.ReadLine().Trim()
                    Dim astrValues As String() = cStringUtils.SplitQualified(strLine, ","c)
                    Dim shape As Single() = iteration.AnomalyShape

                    For i As Integer = 0 To astrValues.Length - 1
                        shape(i) = cStringUtils.ConvertToSingle(astrValues(i))
                    Next
                    For i As Integer = astrValues.Length - 1 To shape.Length - 1
                        shape(i) = 0
                    Next

                End Using
            Catch ex As Exception
                ' Let this code blunder into array bounds etc. No neat error trapping for now, we can always improve this checking later
                bSuccess = False
            End Try
        End If

        iteration.RunState = If(bSuccess, ISFPIterations.eRunState.Completed, ISFPIterations.eRunState.Error)
        Return bSuccess

    End Function

    Private Sub AppendStatus(ByVal msg As cMessage, ByVal strMessage As String, ByVal status As eStatusFlags)
        Dim vs As New cVariableStatus(status, strMessage, eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0)
        msg.Variables.Add(vs)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save all iteration Anomaly shape results to CSV.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function SaveAllAnomalyResultsToCSV(ByVal msg As cMessage) As Boolean

        ' Note on globalization: 
        '  - All messages presented to users should be localized, e.g., obtained from the resources;
        '  - All text written to CSV files is written in English, and cannot be localized in case EwE needs to parse this data one day.
        '  - File names are thus also not localized.

        Dim strPath As String = Me.OutputFolder
        Dim CSVfileSimple As String = Path.Combine(strPath, "Stepwise_Fitting_Procedure_Anomaly_Results.csv")
        Dim writer As StreamWriter = Nothing
        Dim bSuccess As Boolean = True
        Dim TimeSeries As cTimeSeriesDataset = m_core.TimeSeriesDataset(m_core.ActiveTimeSeriesDatasetIndex)

        If cFileUtils.IsDirectoryAvailable(strPath, True) Then

            ' ToDo: clear the content of the directory?

            Try
                writer = New StreamWriter(CSVfileSimple)
            Catch ex As Exception
                Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_FAILED, My.Resources.DETAIL_SUMMARY, ex.Message), eStatusFlags.ErrorEncountered)
                bSuccess = False
            End Try

            If (writer IsNot Nothing) Then

                ' Include default header if needed
                If Me.m_core.SaveWithFileHeader Then
                    writer.WriteLine(m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                    writer.WriteLine(cStringUtils.ToCSVField("Number of Observations") & "," & cStringUtils.ToCSVField(Parameters.NumberOfObservations))
                End If

                ' -- Write header --

                writer.WriteLine("Anomaly Results")
                writer.WriteLine()
                writer.Write("Iteration Name")
                writer.WriteLine()



                Try

                    'Go through each iteration_EC
                    For Each Iteration As ISFPIterations In m_iterations
                        If (Iteration.RunState = ISFPIterations.eRunState.Completed) Then

                            writer.Write(cStringUtils.ToCSVField(Iteration.Name) & ",")

                            ' Write iteration info line
                            For i As Integer = 0 To Iteration.AnomalyShape.Length - 1
                                If (i >= 1) Then writer.Write(",")
                                writer.Write(cStringUtils.ToCSVField(Iteration.AnomalyShape(i)))
                            Next

                            writer.WriteLine()
                        End If
                    Next
                    Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_SUCCESS, My.Resources.DETAIL_SUMMARY, CSVfileSimple), eStatusFlags.OK)
                Catch ex As Exception
                    Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.STATUS_SAVE_DETAIL_FAILED, My.Resources.DETAIL_SUMMARY, ex.Message), eStatusFlags.ErrorEncountered)
                End Try

                writer.Close()

            End If
        Else
            Me.AppendStatus(msg, cStringUtils.Localize(My.Resources.FAILURE_DIRECTORY, strPath), eStatusFlags.ErrorEncountered)
        End If

        Return bSuccess

    End Function

#End Region ' File IO

#Region " Dead code "

#If 0 Then

    'Method not currently used as time series SS are output in SaveResultsToCSV
    'Save all time series SS results of iteration to a CSV file.
    Private Function SaveSSForEachTimeSeriesOfIteration(ByVal iteration As ISFPIterations) As Boolean

        Dim bSuccess As Boolean = True

        'If ecosim or all output is selected save the csv file to the named iteration folder
        If (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.Ecosim) Or _
           (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.All) Then
            Dim strPath As String = Path.Combine(Me.OutputFolder, cFileUtils.ToValidFileName(iteration.Name, False))
            Dim CSVfileSimple As String = Path.Combine(strPath, "Time_Series_SS_Results.csv")
            bSuccess = SaveSSForEachTimeSeries(iteration, strPath, CSVfileSimple)
        End If
        'If aggregated or all output is selected save the csv file in root output folder and add iteration name to csv file name
        If (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.Aggregated) Or _
           (Me.Parameters.AutosaveMode = cSFPParameters.eAutosaveMode.All) Then
            Dim strPath As String = Me.OutputFolder
            Dim CSVfileSimple As String = Path.Combine(strPath, iteration.Name + "_Time_Series_SS_Results.csv")
            bSuccess = SaveSSForEachTimeSeries(iteration, strPath, CSVfileSimple)
        End If

        Return bSuccess

    End Function

    'Method not currently used as time series SS are output in SaveResultsToCSV
    ' Save all time series SS results of iteration to a CSV file.
    Private Function SaveSSForEachTimeSeries(ByVal iteration As ISFPIterations, ByVal sPath As String, ByVal fileName As String) As Boolean

        ' Note on globalization: 
        '  - All messages presented to users should be localized, e.g., obtained from the resources;
        '  - All text written to CSV files is written in English, and cannot be localized in case EwE needs to parse this data one day.
        '  - File names are thus also not localized.

        Dim msgStatus As cMessage = Nothing
        Dim strPath As String = sPath
        Dim CSVfileSimple As String = Path.Combine(strPath, fileName)
        Dim writer As StreamWriter = Nothing
        Dim bSuccess As Boolean = True
        Dim TotalSS As Single = 0

        If (cFileUtils.IsDirectoryAvailable(strPath, True)) Then

            ' ToDo: clear the content of the directory?

            Try
                writer = New StreamWriter(CSVfileSimple)
            Catch ex As Exception
                Dim strReason As String = ex.Message
                msgStatus = New cMessage(cStringUtils.Localize(My.Resources.STATUS_SAVE_FAILED, My.Resources.CAPTION, strReason), _
                                         eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
                bSuccess = False
            End Try

            If (writer IsNot Nothing) Then

                ' Include default header if needed
                If Me.core.SaveWithFileHeader Then
                    writer.WriteLine(core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                End If


                ' -- Write header --
                writer.WriteLine("Name,Pool Type,SS")

                Try

                    If (iteration.RunState = ISFPIterations.eRunState.Completed) Then

                        ' Create status message if need be
                        If (msgStatus IsNot Nothing) Then
                            msgStatus = New cMessage(cStringUtils.Localize(My.Resources.STATUS_SAVE_SUCCESS, My.Resources.CAPTION, CSVfileSimple), _
                                                     eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
                            msgStatus.Hyperlink = strPath
                        End If

                        Dim TimeSeries As cTimeSeriesDataset = core.TimeSeriesDataset(core.ActiveTimeSeriesDatasetIndex)
                        For i As Integer = 1 To TimeSeries.nTimeSeries
                            ' Write SS info line
                            'ToDo:Need to test type of time series to only print out SS if appropriate
                            writer.WriteLine(cStringUtils.ToCSVField(TimeSeries.TimeSeries(i).Name) & "," & _
                                                 cStringUtils.ToCSVField(TimeSeries.TimeSeries(i).TimeSeriesType) & "," & _
                                                 cStringUtils.ToCSVField(TimeSeries.TimeSeries(i).DataSS))
                            TotalSS += TimeSeries.TimeSeries(i).DataSS
                        Next

                        writer.WriteLine("Total SS" & "," & " " & "," & TotalSS)

                        ' ToDo: Consider if we also need to write any information if iterations somehow failed. The run is not complete then...

                    End If

                Catch ex As Exception
                    Dim strReason As String = ex.Message
                    msgStatus = New cMessage(cStringUtils.Localize(My.Resources.STATUS_SAVE_FAILED, My.Resources.CAPTION, strReason), _
                                             eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
                    bSuccess = False
                End Try

                writer.Close()

            End If
        Else
            ' Panic!
            Dim strReason As String = cStringUtils.Localize(My.Resources.FAILURE_DIRECTORY, CSVfileSimple)
            msgStatus = New cMessage(cStringUtils.Localize(My.Resources.STATUS_SAVE_FAILED, My.Resources.CAPTION, strReason), _
                                     eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
        End If

        ' Post output information to EwE (for when runnign as a plug-in)
        If (msgStatus IsNot Nothing) Then
            Me.core.Messages.SendMessage(msgStatus)
        End If

        Return bSuccess
    End Function


    'Method not currently used as time series SS are output in SaveResultsToCSV
    'Save all time series SS results of iteration to a CSV file.
    Private Function SaveSSForEachTimeSeriesToCSV() As Boolean

        ' Note on globalization: 
        '  - All messages presented to users should be localized, e.g., obtained from the resources;
        '  - All text written to CSV files is written in English, and cannot be localized in case EwE needs to parse this data one day.
        '  - File names are thus also not localized.

        Dim msgStatus As cMessage = Nothing
        Dim strPath As String = Me.OutputFolder
        Dim CSVfileSimple As String = Path.Combine(strPath, "Stepwise_Fitting_Procedure_Iteration_Time_Series_SS_Results.csv")
        Dim writer As StreamWriter = Nothing
        Dim bSuccess As Boolean = True
        Dim TotalSS As Single = 0
        Dim TimeSeries As cTimeSeriesDataset = core.TimeSeriesDataset(core.ActiveTimeSeriesDatasetIndex)

        If (cFileUtils.IsDirectoryAvailable(strPath, True)) Then

            ' ToDo: clear the content of the directory?

            Try
                writer = New StreamWriter(CSVfileSimple)
            Catch ex As Exception
                Dim strReason As String = ex.Message
                msgStatus = New cMessage(cStringUtils.Localize(My.Resources.STATUS_SAVE_FAILED, My.Resources.CAPTION, strReason), _
                                         eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
                bSuccess = False
            End Try

            If (writer IsNot Nothing) Then

                Try

                    ' Include default header if needed
                    If Me.core.SaveWithFileHeader Then
                        writer.WriteLine(core.DefaultFileHeader(eAutosaveTypes.Ecosim))
                    End If

                    ' -- Write header --
                    Dim header As String = "Iteration,"
                    For i As Integer = 1 To TimeSeries.nTimeSeries
                        header += TimeSeries.TimeSeries(i).Name & ","
                    Next
                    header += "Total SS"

                    writer.WriteLine(header)

                    'Dim Names As String = " ,"
                    'For i As Integer = 1 To TimeSeries.nTimeSeries
                    '    Names += TimeSeries.TimeSeries(i).Name & ","
                    'Next
                    'Names += " "

                    'writer.WriteLine(Names)

                    Dim ISSscores As String
                    'Go through each iteration
                    For Each Iteration As ISFPIterations In SFPIterations
                        If (Iteration.RunState = ISFPIterations.eRunState.Completed) Then

                            ' Create status message if need be
                            If (msgStatus IsNot Nothing) Then
                                msgStatus = New cMessage(cStringUtils.Localize(My.Resources.STATUS_SAVE_SUCCESS, My.Resources.CAPTION, CSVfileSimple), _
                                                         eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
                                msgStatus.Hyperlink = strPath
                            End If

                            ' Write iteration info line
                            ISSscores = ""
                            ISSscores += cStringUtils.ToCSVField(Iteration.Name) & ","

                            For i As Integer = 1 To TimeSeries.nTimeSeries
                                If Iteration.TimeSeriesSS(i) = 0 Then
                                    ISSscores += " ,"
                                Else
                                    ISSscores += cStringUtils.ToCSVField(Iteration.TimeSeriesSS(i)) & ","
                                End If
                            Next

                            ISSscores += cStringUtils.ToCSVField(Iteration.SS)
                            writer.WriteLine(ISSscores)

                            ' ToDo: Consider if we also need to write any information if iterations somehow failed. The run is not complete then...

                        End If
                    Next
                Catch ex As Exception
                    Dim strReason As String = ex.Message
                    msgStatus = New cMessage(cStringUtils.Localize(My.Resources.STATUS_SAVE_FAILED, My.Resources.CAPTION, strReason), _
                                             eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
                    bSuccess = False
                End Try

                writer.Close()

            End If
        Else
            ' Panic!
            Dim strReason As String = cStringUtils.Localize(My.Resources.FAILURE_DIRECTORY, CSVfileSimple)
            msgStatus = New cMessage(cStringUtils.Localize(My.Resources.STATUS_SAVE_FAILED, My.Resources.CAPTION, strReason), _
                                     eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
        End If

        ' Post output information to EwE (for when runnign as a plug-in)
        If (msgStatus IsNot Nothing) Then
            Me.core.Messages.SendMessage(msgStatus)
        End If

        Return bSuccess
    End Function

#End If

#End Region ' Dead code

End Class
