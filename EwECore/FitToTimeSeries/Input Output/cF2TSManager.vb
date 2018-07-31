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

Imports EwECore.ValueWrapper
Imports System.Threading
Imports EwECore.FitToTimeSeries
Imports EwEUtils.Core
Imports EwECore.SearchObjectives
Imports System.IO
Imports EwEUtils.Utilities

Public Class cF2TSManager
    Inherits cCoreInputOutputBase
    Implements SearchObjectives.ISearchObjective
    Implements IThreadedProcess

    'ToDo_jb Firstyear and LastYear need to be set for the time series data

#Region " Construction, Initialization and Destruction"

    Private m_PPIs As New Dictionary(Of String, cMediatedInteraction)
    Private m_EPData As cEcopathDataStructures = Nothing
    Private m_ESData As cEcosimDatastructures = Nothing
    Private m_model As cF2TSModel = Nothing

    ' Received delegate instances to report progress to
    Private m_runstartedHandler As RunStartedDelegate = Nothing
    Private m_runstepHandler As RunStepDelegate = Nothing
    Private m_runstoppedHandler As RunStoppedDelegate = Nothing
    Private m_runModelHandler As RunModelDelegate = Nothing



    'Messaging 
    'list of messages sent from the model
    Private m_lstMessages As List(Of cMessage)
    'definition of delegate used to send messages to the core
    'this is for marshalling messages across thread boundaries
    Private Delegate Sub SendMessagesToCoreDelegate()

    Private m_searchObjective As cSearchObjective

    Private m_nonBlockingWait As cNonBlockingWaitHandle


    Friend Sub New(ByRef theCore As cCore)
        MyBase.New(theCore)

        Me.m_lstMessages = New List(Of cMessage)

        Dim val As cValue = Nothing

        Me.AllowValidation = False
        Me.m_coreComponent = eCoreComponentType.EcoSimFitToTimeSeries
        Me.m_dataType = eDataTypes.FitToTimeSeries

        Me.m_core = theCore
        Me.m_EPData = theCore.m_EcoPathData
        Me.m_ESData = theCore.m_EcoSimData

        m_searchObjective = theCore.SearchObjective
        Me.m_nonBlockingWait = New cNonBlockingWaitHandle

        'default OK status used for setVariable
        m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        'boolean
        ' F2TSVulnerabilitySearch
        val = New cValue(New Boolean, eVarNameFlags.F2TSVulnerabilitySearch, eStatusFlags.Null, eValueTypes.Bool)
        m_values.Add(val.varName, val)

        'boolean
        ' AnomalySearch
        val = New cValue(New Boolean, eVarNameFlags.F2TSAnomalySearch, eStatusFlags.Null, eValueTypes.Bool)
        m_values.Add(val.varName, val)

        ' UseDefaultVs
        val = New cValue(New Boolean, eVarNameFlags.F2TSUseDefaultV, eStatusFlags.Null, eValueTypes.Bool)
        m_values.Add(val.varName, val)

        ' F2TSCatchAnomaly
        val = New cValue(New Boolean, eVarNameFlags.F2TSCatchAnomaly, eStatusFlags.Null, eValueTypes.Bool)
        m_values.Add(val.varName, val)

        'singles
        ' F2TSFirstYear
        val = New cValue(New Integer, eVarNameFlags.F2TSFirstYear, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        ' F2TSLastYear
        val = New cValue(New Integer, eVarNameFlags.F2TSLastYear, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        ' F2TSVulnerabilityVariance
        val = New cValue(New Single, eVarNameFlags.F2TSVulnerabilityVariance, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        ' F2TSPPVariance
        val = New cValue(New Single, eVarNameFlags.F2TSPPVariance, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'integers
        ' F2TSCatchAnomalySearchShape
        val = New cValue(New Integer, eVarNameFlags.F2TSCatchAnomalySearchShapeNumber, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        ' F2TSNumSplinePoints
        val = New cValue(New Integer, eVarNameFlags.F2TSNumSplinePoints, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        'Singlearray
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.F2TSAppliedWeights, eStatusFlags.Null, eCoreCounterTypes.nTimeSeriesApplied, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        ' AIC N Data points
        val = New cValue(New Integer, eVarNameFlags.F2TSNAICData, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        Me.ResetStatusFlags()
        Me.AllowValidation = True

        ' Create and configure model
        Me.m_model = New cF2TSModel(Me.m_core, Me.m_core.m_EcoSim, Me.m_EPData, Me.m_ESData)
        Me.m_model.Init(AddressOf RunStartedCallback, AddressOf RunStepCallback, AddressOf RunStoppedCallback, _
                        AddressOf AddMessageCallback, AddressOf RunModelCallBack, AddressOf Me.SendMessageCallback)

    End Sub

    Friend Function Init(ByRef theCore As cCore) As Boolean Implements ISearchObjective.Init
        Me.m_core.m_FitToTimeSeriesData.FirstYear = 1
        Me.m_core.m_FitToTimeSeriesData.LastYear = 1
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Loads local values in the manager from the underlying data structures.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function Load() As Boolean Implements ISearchObjective.Load

        If (Me.m_EPData.ActiveEcosimScenario <= 0) Then Return False

        Dim f2tsDS As cF2TSDataStructures = Me.m_core.m_FitToTimeSeriesData
        Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData

        Me.setDefaultAICNData()

        Me.AllowValidation = False
        Me.VulnerabilitySearch = f2tsDS.bVulnerabilitySearch
        Me.CatchAnomaly = f2tsDS.bCatchAnomaly
        Me.AnomalySearchShapeNumber = f2tsDS.iCatchAnomalySearchShapeNumber
        Me.NumSplinePoints = f2tsDS.nNumSplinePoints
        Me.FirstYear = f2tsDS.FirstYear
        Me.LastYear = f2tsDS.LastYear
        Me.PPVariance = f2tsDS.PPVariance
        Me.VulnerabilityVariance = f2tsDS.VulnerabilityVariance

        Me.NAICDataPoints = f2tsDS.nAICData
        Me.UseDefaultV = f2tsDS.UseDefaultV

        ' Use DBID from current Ecosim scenario
        Me.DBID = Me.m_EPData.EcosimScenarioDBID(Me.m_EPData.ActiveEcosimScenario)

        Me.AllowValidation = True

    End Function

    Public Overrides Sub Clear() Implements ISearchObjective.Clear
        MyBase.Clear()

        Try
            Me.m_SyncObject = Nothing
            Me.m_runstartedHandler = Nothing
            Me.m_runstepHandler = Nothing
            Me.m_runstoppedHandler = Nothing
            Me.m_runModelHandler = Nothing

            'kill the thread if it is still alive
            If Me.m_thrdRun IsNot Nothing Then
                Me.m_thrdRun.Abort()
                Me.m_thrdRun = Nothing
            End If

        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    Public Sub Connect(ByVal syncObject As System.ComponentModel.ISynchronizeInvoke, _
                       ByVal runStartedCallback As RunStartedDelegate, _
                       ByVal runStepCallback As RunStepDelegate, _
                       ByVal runStoppedCallback As RunStoppedDelegate, _
                       ByVal RunModelCallBack As RunModelDelegate)

        Debug.Assert(m_runstartedHandler = Nothing, "Manager already connected?")

        Try
            Me.m_SyncObject = syncObject
            Me.m_runstartedHandler = runStartedCallback
            Me.m_runstepHandler = runStepCallback
            Me.m_runstoppedHandler = runStoppedCallback
            Me.m_runModelHandler = RunModelCallBack
        Catch ex As Exception
            cLog.Write(ex)
            Throw New ApplicationException(Me.ToString & ".Connect() Error.", ex)
        End Try

    End Sub

    <Obsolete("Use parameterless Disconnect() instead")> _
    Public Sub Disconnect(ByVal runStartedCallback As RunStartedDelegate, _
                          ByVal runStepCallback As RunStepDelegate, _
                          ByVal runStoppedCallback As RunStoppedDelegate, _
                          ByVal RunModelCallBack As RunModelDelegate)
        Me.Disconnect()
    End Sub

    Public Sub Disconnect()
        Me.Clear()
    End Sub


    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Stores the values in the manager back to the underlying data structures.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function Update(ByVal DataType As EwEUtils.Core.eDataTypes) As Boolean Implements SearchObjectives.ISearchObjective.Update

        Dim f2tsDS As cF2TSDataStructures = Me.m_core.m_FitToTimeSeriesData
        Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData

        f2tsDS.bVulnerabilitySearch = Me.VulnerabilitySearch
        f2tsDS.bCatchAnomaly = Me.CatchAnomaly
        f2tsDS.bAnomalySearch = Me.AnomalySearch

        f2tsDS.iCatchAnomalySearchShapeNumber = Me.AnomalySearchShapeNumber
        f2tsDS.nNumSplinePoints = Me.NumSplinePoints
        f2tsDS.FirstYear = Me.FirstYear
        f2tsDS.LastYear = Me.LastYear
        f2tsDS.PPVariance = Me.PPVariance
        f2tsDS.VulnerabilityVariance = Me.VulnerabilityVariance

        f2tsDS.nAICData = Me.NAICDataPoints

        f2tsDS.UseDefaultV = Me.UseDefaultV
        Return True

    End Function

    ''' <summary>
    ''' Compute the default value for nAICData number of AIC data points
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub setDefaultAICNData()

        Dim f2tsDS As cF2TSDataStructures = Me.m_core.m_FitToTimeSeriesData
        Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData
        Dim count As Integer = 0

        For iTS As Integer = 1 To tsDS.NdatType
            If (tsDS.UseForAIC(tsDS.DatType(iTS))) Then
                For iPt As Integer = 0 To tsDS.nDatPoints
                    If tsDS.DatVal(iPt, iTS) > 0 Then count += 1
                Next
            End If
        Next
        f2tsDS.nAICData = count

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()

        'I dont think this can happen
        'the thread is killed when the form is unloaded but just in case
        Me.Clear()

    End Sub

#End Region

#Region " Generic variable access "

    ''' <summary>
    ''' States whether there is a link between a given predator and prey.
    ''' </summary>
    ''' <param name="iPred"></param>
    ''' <param name="iPrey"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' This code encapsulates working with ecosim inlinks, ilink and jlink variables.
    ''' </remarks>
    Public Function isPredPrey(ByVal iPred As Integer, ByVal iPrey As Integer) As Boolean
        For i As Integer = 1 To Me.m_ESData.Narena
            If Me.m_ESData.Iarena(i) = iPrey And Me.m_ESData.Jarena(i) = iPred Then Return True
        Next
        Return False
    End Function

    Public ReadOnly Property TotalTime() As Integer
        Get
            Return Me.m_ESData.NumYears ' ?
        End Get
    End Property

    Public Property VulnerabilitySearch() As Boolean
        Get
            Return CBool(Me.GetVariable(eVarNameFlags.F2TSVulnerabilitySearch))
        End Get
        Set(ByVal value As Boolean)
            Me.SetVariable(eVarNameFlags.F2TSVulnerabilitySearch, value)
        End Set
    End Property

    Public Property CatchAnomaly() As Boolean
        Get
            Return CBool(Me.GetVariable(eVarNameFlags.F2TSCatchAnomaly))
        End Get
        Set(ByVal value As Boolean)
            Me.SetVariable(eVarNameFlags.F2TSCatchAnomaly, value)
        End Set
    End Property

    Public Property AnomalySearch() As Boolean
        Get
            Return CBool(Me.GetVariable(eVarNameFlags.F2TSAnomalySearch))
        End Get
        Set(ByVal value As Boolean)
            Me.SetVariable(eVarNameFlags.F2TSAnomalySearch, value)
        End Set
    End Property

    Public Property UseDefaultV() As Boolean
        Get
            Return CBool(Me.GetVariable(eVarNameFlags.F2TSUseDefaultV))
        End Get
        Set(ByVal value As Boolean)
            Me.SetVariable(eVarNameFlags.F2TSUseDefaultV, value)
        End Set
    End Property

    Public Property AnomalySearchShapeNumber() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.F2TSCatchAnomalySearchShapeNumber))
        End Get
        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.F2TSCatchAnomalySearchShapeNumber, value)
        End Set
    End Property

    Public Property FirstYear() As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.F2TSFirstYear))
        End Get
        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.F2TSFirstYear, value)
        End Set
    End Property

    Public Property LastYear() As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.F2TSLastYear))
        End Get
        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.F2TSLastYear, value)
        End Set
    End Property

    Public Property VulnerabilityVariance() As Single
        Get
            Return CSng(Me.GetVariable(eVarNameFlags.F2TSVulnerabilityVariance))
        End Get
        Set(ByVal value As Single)
            Me.SetVariable(eVarNameFlags.F2TSVulnerabilityVariance, value)
        End Set
    End Property

    Public Property PPVariance() As Single
        Get
            Return CSng(Me.GetVariable(eVarNameFlags.F2TSPPVariance))
        End Get
        Set(ByVal value As Single)
            Me.SetVariable(eVarNameFlags.F2TSPPVariance, value)
        End Set
    End Property

    Public Property NumSplinePoints() As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.F2TSNumSplinePoints))
        End Get
        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.F2TSNumSplinePoints, value)
        End Set
    End Property


    ''' <summary>
    ''' Number of data points for the AIC indicator
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAICDataPoints() As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.F2TSNAICData))
        End Get
        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.F2TSNAICData, value)
        End Set
    End Property


    Public ReadOnly Property nTimeSeriesYears() As Integer
        Get
            Return Me.m_core.m_TSData.nMaxYears
        End Get
    End Property

    ''' <summary>
    ''' Get/set the vulnerability block to use for pred,prey
    ''' </summary>
    Public Property VulnerabilityBlocks() As Integer(,)
        Get
            ' Translate m_model.Vblockcode into pred/prey array
            Dim a2iVulnerabilityBlocks(Me.m_EPData.NumGroups, Me.m_EPData.NumGroups) As Integer
            For iLink As Integer = 1 To Me.m_ESData.Narena
                a2iVulnerabilityBlocks(Me.m_ESData.Jarena(iLink), Me.m_ESData.Iarena(iLink)) = Me.m_model.VblockCode(iLink)
            Next
            Return a2iVulnerabilityBlocks
        End Get
        Set(ByVal value(,) As Integer)
            ' Copy pred/prey array into inlinks array
            Dim aiVblockCode(Me.m_ESData.Narena) As Integer
            Dim iLink As Integer = 1

            For j As Integer = 1 To Me.m_EPData.NumGroups      'all living groups; consumers
                For i As Integer = 0 To Me.m_EPData.NumGroups 'prey
                    If (Me.isPredPrey(i, j) = True) Then
                        aiVblockCode(iLink) = value(i, j)
                        iLink += 1
                    End If
                Next i
            Next j
            Me.m_model.VblockCode = aiVblockCode
        End Set
    End Property

    ''' <summary>
    ''' Get/set the number of blocks to search for.
    ''' </summary>
    Public Property nBlockCodes() As Integer
        Get
            Return m_model.nBlockCodes
        End Get
        Set(ByVal value As Integer)
            m_model.nBlockCodes = value
        End Set
    End Property

#End Region ' Generic variable access

#Region " Public access "

#Region " Model state access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether F2TS models can run.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function CanRun() As Boolean
        ' Check all pre-run states

        ' jb replaced the is running check with a semaphore
        Dim bCanRun As Boolean '= Not Me.IsRunning

        Try

            bCanRun = Me.m_core.StateMonitor.HasEcosimLoaded

            If (Me.AnomalySearch) Then bCanRun = bCanRun And (Me.AnomalySearchShapeNumber > 0)
            'isRefDataLoaded() will send a message if there is not data loaded
            bCanRun = bCanRun And isRefDataLoaded()
            ' bCanRun = bCanRun And Me.m_SyncObject IsNot Nothing

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & " Error not properly initialized.")
            bCanRun = False
        End Try

        If Not bCanRun Then
            ' ToDo: globalize this
            m_core.Messages.SendMessage(New cMessage("Fit to Time Series not all the parameters have been set correctly.", eMessageType.ErrorEncountered, eCoreComponentType.EcoSimFitToTimeSeries, eMessageImportance.Warning))
        End If

        Return bCanRun

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether a F2TS model is running.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsRunning() As Boolean Implements IThreadedProcess.IsRunning
        Get
            Return Me.m_nonBlockingWait.IsRunning
        End Get
    End Property


    ''' <summary>
    ''' Block the calling thread until the model has finished running
    ''' </summary>
    ''' <remarks>This can be used by an interface to call the model then wait for results before continuing processing.</remarks>
    Public Function Wait(Optional ByVal WaitTimeInMilSec As Integer = -1) As Boolean Implements IThreadedProcess.Wait
        Return Me.m_nonBlockingWait.Wait(WaitTimeInMilSec)
    End Function


    Public Sub ReleaseWait() Implements IThreadedProcess.ReleaseWait
        Me.m_nonBlockingWait.ReleaseWait()
    End Sub

    Public Sub SetWait() Implements IThreadedProcess.SetWait
        Me.m_nonBlockingWait.SetWait()
    End Sub


    Private Function isRefDataLoaded() As Boolean

        If m_core.m_TSData.NdatType > 0 Then
            Return True
        End If

        'jb this should never happen but if it does we better tell the interface why this could not be run
        m_core.Messages.SendMessage(New cMessage(My.Resources.CoreMessages.F2TS_ERROR_NO_TS, _
                                                 eMessageType.ErrorEncountered, eCoreComponentType.EcoSimFitToTimeSeries, eMessageImportance.Warning))

        Return False
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Stops a running F2TS model
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function StopRun(Optional ByVal WaitTimeInMillSec As Integer = -1) As Boolean Implements IThreadedProcess.StopRun
        Dim result As Boolean = True
        Try
            'the model will keep running until it hits the StopRun flag
            'at which point it will call the RunStoppedDelegate(eRunType)
            'this lets it die gracefully
            m_model.StopRun = True

            result = Me.Wait(WaitTimeInMillSec)

        Catch ex As Exception
            result = False
        End Try

        Return result

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the <see cref="eRunType">type of run</see> 
    ''' the current F2TS model is performing.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function GetRunType() As eRunType
        Return Me.m_model.RunState
    End Function

#End Region ' Model state access

#Region " SensitivitySS2VByPredPrey "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="bRunSilent">Optional parameter to run without sending any messages or requesting any feedback</param>
    ''' <param name="RunThreaded">Optional flag to state if the search runs multi-threaded. If <see cref="TriState.[False]"/>,
    ''' the search never runs theaded. If <see cref="TriState.[True]"/>the search always runs threaded. If <see cref="TriState.UseDefault"/>,
    ''' the search only runs threaded if a <see cref="System.ComponentModel.ISynchronizeInvoke">sync object</see> has been
    ''' provided when <see cref="Connect">connecting</see>.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function RunSensitivitySS2VByPredPrey(Optional ByVal bRunSilent As Boolean = False,
                                                 Optional ByVal RunThreaded As TriState = TriState.UseDefault) As Boolean

        ' Safety check
        If Not CanRun() Then Return False

        Try
            Me.SetWait()
            ' Sanity check
            Debug.Assert(Me.m_thrdRun Is Nothing)

            ' Make sure model can access manager variables from the shared data structures
            Me.Update(Me.m_dataType)

            ' Take a nasty shortcut to set the run silent flag. We cannot get m_model.Data, 
            ' because this is only initialized within RunSensitivitySS2VByPredPrey call, next.
            Dim data As cF2TSDataStructures = Me.m_core.m_FitToTimeSeriesData
            data.RunSilent = bRunSilent

            ' Launch requested analysis model 
            If (Me.m_SyncObject IsNot Nothing) And (RunThreaded <> TriState.False) Then
                m_thrdRun = New Thread(AddressOf Me.m_model.RunSensitivitySS2VByPredPrey)
                m_thrdRun.Start()
            Else
                Me.m_model.RunSensitivitySS2VByPredPrey()
                Me.ReleaseWait()
            End If

            Return True

        Catch ex As Exception

            cLog.Write(ex)
            ' ToDo: globalize this
            Me.SendMessageCallback(New cMessage("Fit to timeseries Error: Sensitvity to predator prey search. " & ex.Message, eMessageType.ErrorEncountered, _
                                    eCoreComponentType.EcoSimFitToTimeSeries, eMessageImportance.Critical, Me.m_dataType))
            'Finally
            '    Me.ReleaseWait()
        End Try

    End Function

#End Region ' SensitivitySS2VByPredPrey

#Region " SensitivitySS2VByPredator "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run the sensitivity to vulnerabilities search by predator.
    ''' </summary>
    ''' <param name="bRunSilent">Optional parameter to run without sending any messages or requesting any feedback</param>
    ''' <param name="RunThreaded">Optional flag to state if the search runs multi-threaded. If <see cref="TriState.[False]"/>,
    ''' the search never runs theaded. If <see cref="TriState.[True]"/>the search always runs threaded. If <see cref="TriState.UseDefault"/>,
    ''' the search only runs threaded if a <see cref="System.ComponentModel.ISynchronizeInvoke">sync object</see> has been
    ''' provided when <see cref="Connect">connecting</see>.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function RunSensitivitySS2VByPredator(Optional ByVal bRunSilent As Boolean = False,
                                                 Optional ByVal RunThreaded As TriState = TriState.UseDefault) As Boolean

        ' Safety check
        If Not CanRun() Then Return False

        Me.m_core.CheckResetDefaultVulnerabilities(bRunSilent)

        Try

            Me.SetWait()
            ' Sanity check
            Debug.Assert(Me.m_thrdRun Is Nothing)

            ' Make sure model can access manager variables from the shared data structures
            Me.Update(Me.m_dataType)

            ' Launch requested analysis model 
            If (Me.m_SyncObject IsNot Nothing) And (RunThreaded <> TriState.False) Then
                m_thrdRun = New Thread(AddressOf Me.m_model.RunSensitivitySS2VByPredator)
                m_thrdRun.Start()
            Else
                Me.m_model.RunSensitivitySS2VByPredator()
                Me.ReleaseWait()
            End If

            Return True

        Catch ex As Exception

            cLog.Write(ex)
            ' ToDo: globalize this
            Me.SendMessageCallback(New cMessage("Fit to timeseries Error: Sensitvity to predator search. " & ex.Message, eMessageType.ErrorEncountered, _
                                    eCoreComponentType.EcoSimFitToTimeSeries, eMessageImportance.Critical, Me.m_dataType))
            'Finally
            '    Me.ReleaseWait()
        End Try

    End Function

#End Region ' SensitivitySS2VByPredPrey

#Region " Search "

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="bRunSilent">Optional parameter to run without sending any messages or requesting any feedback</param>
    ''' <param name="RunThreaded">Optional flag to state if the search runs multi-threaded. If <see cref="TriState.[False]"/>,
    ''' the search never runs theaded. If <see cref="TriState.[True]"/>the search always runs threaded. If <see cref="TriState.UseDefault"/>,
    ''' the search only runs threaded if a <see cref="System.ComponentModel.ISynchronizeInvoke">sync object</see> has been
    ''' provided when <see cref="Connect">connecting</see>.</param>
    ''' <returns></returns>
    Public Function RunSearch(Optional ByVal bRunSilent As Boolean = False, _
                              Optional ByVal RunThreaded As TriState = TriState.UseDefault) As Boolean

        Dim iPPYear1 As Integer = 0
        Dim iPPYear2 As Integer = 0
        Dim bSucces As Boolean = True

        ' Safety check
        If Not CanRun() Then Return False

        Me.m_core.m_FitToTimeSeriesData.RunSilent = bRunSilent

        Try

            Me.SetWait()

            ' Make sure model can access manager variables from the shared data structures
            Me.Update(Me.m_dataType)

            ' Launch requested analysis model 
            If (Me.m_SyncObject IsNot Nothing) And (RunThreaded <> TriState.False) Then
                Me.m_thrdRun = New Thread(AddressOf Me.m_model.RunSearch)
                Me.m_thrdRun.Start()
            Else
                Me.m_model.RunSearch()
                Me.ReleaseWait()
            End If

        Catch ex As Exception
            bSucces = False
            Me.m_core.m_FitToTimeSeriesData.RunSilent = False
            Me.SendMessageCallback(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.F2TS_ERROR, ex.Message),
                                                eMessageType.ErrorEncountered, eCoreComponentType.EcoSimFitToTimeSeries, eMessageImportance.Critical, Me.m_dataType))
            cLog.Write(ex)

            Me.ReleaseWait()
        End Try

        Return bSucces

    End Function

    Public Function getAIC(ByVal nPars As Integer, ByVal nData As Integer, ByVal ss As Single) As Single
        Try
            If (Me.m_model.Data Is Nothing) Then Return 0

            Me.m_model.setAIC(nPars, nData, ss)
            Return Me.m_model.Data.AIC
        Catch ex As Exception

        End Try
    End Function

#End Region ' Search

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the most recent received results.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function Results() As cF2TSResults
        Return Me.m_model.Results
    End Function


    Public Sub setNBlocksFromSensitivity(ByVal nBlocks As Integer)
        Me.m_model.setNBlocksFromSensitivity(nBlocks)
    End Sub

    ''' <summary>
    ''' Get whether a sensitivity search has been ran.
    ''' </summary>
    Public ReadOnly Property HasRunSens() As Boolean
        Get
            Return Me.m_model.HasRunSens
        End Get
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Save blocks to CSV
    ''' </summary>
    ''' <param name="strFilename">The name of the file to save to.</param>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Public Function SaveToCSV(strFilename As String) As Boolean

        ' ToDo: localize this

        If (Me.HasRunSens = False) Then Return False

        Dim vblocks(,) As Integer = Me.VulnerabilityBlocks
        Dim sw As StreamWriter = Nothing
        Dim msg As cMessage = Nothing

        Try
            sw = New StreamWriter(strFilename, False)
        Catch ex As Exception
            ' ToDo: globalize this
            msg = New cMessage(cStringUtils.Localize("Unable to Sensitivity CSV file {0}. {1}", strFilename, ex.Message), _
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
            Me.m_core.Messages.SendMessage(msg)
            Return False
        End Try

        sw.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecosim))
        sw.WriteLine()
        For iPrey As Integer = 1 To Me.m_core.nGroups
            sw.Write("," & cStringUtils.ToCSVField(Me.m_core.EcoPathGroupInputs(iPrey).Name))
        Next
        sw.WriteLine()

        For iPred As Integer = 1 To Me.m_core.nGroups
            sw.Write(cStringUtils.ToCSVField(Me.m_core.EcoPathGroupInputs(iPred).Name) & ",")
            For iPrey As Integer = 1 To Me.m_core.nGroups
                If Me.isPredPrey(iPred, iPrey) Then
                    sw.Write(vblocks(iPred, iPrey))
                End If
                sw.Write(",")
            Next iPrey
            sw.WriteLine()
        Next iPred

        sw.Flush()
        sw.Close()

        ' ToDo: globalize this
        msg = New cMessage(cStringUtils.Localize("Saved sensitivity CSV file {0}.", strFilename), _
                           eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
        msg.Hyperlink = Path.GetDirectoryName(strFilename)
        Me.m_core.Messages.SendMessage(msg)
        Return True

    End Function

#End Region ' Public access

#Region " Internal model callback handlers "

    Private m_thrdRun As Threading.Thread = Nothing
    Private m_SyncObject As System.ComponentModel.ISynchronizeInvoke
    Private m_messages As New List(Of cMessage)
    Private m_results As cF2TSResults = Nothing

    ''' <summary>
    ''' Delegate handler called by the model when the run has started
    ''' </summary>
    ''' <param name="runType"></param>
    ''' <remarks>This handler is passed to the model during the contruction of the manager via cF2TSModel.Init()</remarks>
    Private Sub RunStartedCallback(ByVal runType As eRunType, ByVal nSteps As Integer)

        ' Clear previous results
        Me.m_results = Nothing

        Try
            ' Notify the world
            If (Me.m_runstartedHandler IsNot Nothing) Then
                If (Me.m_SyncObject IsNot Nothing) Then
                    Dim parms(1) As Object
                    parms(0) = runType
                    parms(1) = nSteps
                    m_SyncObject.Invoke(Me.m_runstartedHandler, parms)
                Else
                    Me.m_runstartedHandler.Invoke(runType, nSteps)
                End If
            End If
        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.StackTrace)
        End Try

    End Sub

    ''' <summary>
    ''' Delegate handler called by the model when the run has completed a step
    ''' </summary>
    ''' <remarks>This handler is passed to the model during the contruction of the manager via cF2TSModel.Init()</remarks>
    Private Sub RunStepCallback()

        Try

            'keep a reference
            m_results = m_model.Results

            'jb For debugging
            'If m_model.RunState = eRunType.Search Then
            '    Try 'incase m_results is not a cSearchResults object
            '        'System.Console.WriteLine("F2TS: Run Step. SS = " & DirectCast(m_results, cSearchResults).IterSS)
            '    Catch ex As Exception
            '        'dont need to do anything this is just for debugging
            '    End Try
            'End If

            ' Notify the world
            If (Me.m_runstartedHandler IsNot Nothing) Then
                If (Me.m_SyncObject IsNot Nothing) Then
                    m_SyncObject.Invoke(Me.m_runstepHandler, Nothing)
                Else
                    Me.m_runstepHandler.Invoke()
                End If
            End If

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, "The interface has thrown an exception that was handled by " & Me.ToString)
        End Try

    End Sub


    ''' <summary>
    ''' Delegate handler called by the model when the run has stopped
    ''' </summary>
    ''' <param name="runType"></param>
    ''' <remarks>This handler is passed to the model during the contruction of the manager via cF2TSModel.Init()</remarks>
    Private Sub RunStoppedCallback(ByVal runType As eRunType)

        Try
            'keep a reference
            m_results = m_model.Results

            'System.Console.WriteLine("F2TS: Run Stopped.")

            'call anything that needs to be called at the end of a model run via the m_SyncObject 
            'so that it will be marshalled to the interfaces thread
            Dim dlgRunStopped As RunStoppedDelegate = AddressOf Me.ThreadSafeRunStopped
            If (Me.m_SyncObject IsNot Nothing) Then
                Dim parms(0) As Object
                parms(0) = runType
                m_SyncObject.Invoke(dlgRunStopped, parms)
            Else
                dlgRunStopped.Invoke(runType)
            End If

            Me.ReleaseWait()

            Me.m_core.m_FitToTimeSeriesData.RunSilent = False
            Me.m_thrdRun = Nothing

        Catch ex As Exception

            Me.ReleaseWait()
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
        End Try


    End Sub

    Private Sub ThreadSafeRunStopped(ByVal runType As eRunType)

        Try
            m_core.VulnerabilitiesChanged()
            m_core.LoadEcosimStats()
            Me.m_thrdRun = Nothing

            If Not Me.m_core.m_FitToTimeSeriesData.RunSilent Then

                'send any messages created by the fit to time series
                For Each msg As cMessage In m_lstMessages
                    m_core.Messages.AddMessage(msg)
                Next
                m_core.Messages.sendAllMessages()
                m_lstMessages.Clear()
            End If

            ' Notify world
            If (Me.m_runstoppedHandler IsNot Nothing) Then
                If (Me.m_SyncObject IsNot Nothing) Then
                    Dim objs(0) As Object
                    objs(0) = runType
                    m_SyncObject.Invoke(Me.m_runstoppedHandler, objs)
                Else
                    Me.m_runstoppedHandler.Invoke(runType)
                End If
            End If

        Catch ex As Exception
            cLog.Write(ex)
            m_core.Messages.SendMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.F2TS_ERROR, ex.Message),
                                                     eMessageType.ErrorEncountered, eCoreComponentType.EcoSimFitToTimeSeries, eMessageImportance.Warning))
        End Try

    End Sub

    Private Sub RunModelCallBack(ByVal runType As eRunType, ByVal iCurrentIterationStep As Integer, ByVal nTotalInterationSteps As Integer)

        Try
            'System.Console.WriteLine("F2TS: Ecosim called.")

            ' Call delegate
            If (Me.m_SyncObject IsNot Nothing) Then
                Dim parms(2) As Object
                parms(0) = runType
                parms(1) = iCurrentIterationStep
                parms(2) = nTotalInterationSteps
                m_SyncObject.Invoke(Me.m_runModelHandler, parms)
            Else
                Me.m_runModelHandler.Invoke(runType, iCurrentIterationStep, nTotalInterationSteps)
            End If

        Catch ex As Exception
            cLog.Write(ex)

        End Try
    End Sub


    ''' <summary>
    ''' Delegate handler for Model to add a message to the managers list of messages
    ''' </summary>
    ''' <param name="msg"></param>
    Private Sub AddMessageCallback(ByVal msg As cMessage)
        Try
            ' Add the message to the list of messages
            Me.m_lstMessages.Add(msg)
        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    ''' <summary>
    ''' Delegate handler for Model to add a message to the managers list of messages
    ''' </summary>
    ''' <param name="msg"></param>
    Private Sub SendMessageCallback(ByVal msg As cMessage)
        Try
            Dim objs(0) As Object

            If Me.m_core.m_FitToTimeSeriesData.RunSilent Then
                System.Console.WriteLine(Me.ToString & " Tried to send message while running in Silent Mode.")
                Exit Sub
            End If

            objs(0) = msg

            'call ThreadSafeSendMessage() via the m_SyncObject 
            'this will put ThreadSafeSendMessage() on the interface thread
            If (m_SyncObject IsNot Nothing) Then
                Dim dlgSenMessage As RunMessageDelegate = AddressOf Me.SendMessage
                m_SyncObject.Invoke(dlgSenMessage, objs)
            Else
                Me.SendMessage(msg)
            End If

        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub SendMessage(ByVal msg As cMessage)
        Try
            m_core.Messages.SendMessage(msg)
        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

#End Region ' Internal model handling

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

    Public WriteOnly Property MessagePump As cCore.MessagePumpDelegate Implements IThreadedProcess.MessagePump
        Set(value As cCore.MessagePumpDelegate)
            Me.m_nonBlockingWait.MessagePump = value
        End Set
    End Property

    Private Class cNonBlockingWaitHandle
        Inherits cThreadWaitBase

        Public Overrides Function StopRun(Optional WaitTimeInMillSec As Integer = -1) As Boolean
            Return True
        End Function
    End Class

End Class
