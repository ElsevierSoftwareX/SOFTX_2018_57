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
Imports EwECore.EcoSeed
Imports System.Threading
Imports EwECore.SearchObjectives
Imports EwEUtils.Core

#End Region ' Imports

#Region "Optimization Manager"

Public Class cMPAOptManager
    Inherits cThreadWaitBase 'for thread blocking
    Implements ICoreInterface
    Implements SearchObjectives.ISearchObjective

#Region "Enums"

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' ENumerated type, defines MPA search run states.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eRunStates As Integer
        ''' <summary>Search is not active.</summary>
        Idle
        ''' <summary>Search is initializing.</summary>
        Initializing
        ''' <summary>Search is in progress.</summary>
        Searching
        ''' <summary>Search is completed.</summary>
        Completed
        ''' <summary>A new cell is selected to add to the current MPA configuration</summary>
        NewCellSelected
        ''' <summary>A new best result has been found.</summary>
        NewBestResultFound
    End Enum

#End Region

#Region "Delegates for model to comunicate back to the manager"

    ''' <summary>
    ''' MPA Search has computed the value of a cell
    ''' </summary>
    Public Delegate Sub SearchIterationDelegate()

    ''' <summary>
    ''' MPA Search run state delegate.
    ''' </summary>
    Public Delegate Sub SearchRunStateDelegate(ByVal RunState As eRunStates)

    ''' <summary>
    ''' Message sending delegate
    ''' </summary>
    ''' <param name="message"></param>
    Public Delegate Sub SendMessageDelegate(ByVal message As EwECore.cMessage)

#End Region

#Region "Private Data"

    Private m_syncObject As System.ComponentModel.ISynchronizeInvoke
    Private m_bConnected As Boolean

    Private m_MPASearch As IMPASearchModel
    Private m_core As cCore
    Private m_searchObjectives As cSearchObjective

    Private m_SeedCellComputedCallback As SearchIterationDelegate
    Private m_SeedRunStateCallback As SearchRunStateDelegate
    Private m_thrSeed As Threading.Thread
    Private m_curRowCol As cMPAOptOutput

    Private m_parameters As cMPAOptParameters

    ''' <summary>
    ''' Original MPA configuration (row x col x mpa)
    ''' </summary>
    Private m_orgMPAConfig()(,) As Integer

    ''' <summary>directory for the output data</summary>
    Private m_dataDir As String = ""

#End Region

#Region "Construction and Initialization"


    Friend Function Init(ByRef theCore As cCore) As Boolean Implements ISearchObjective.Init

        Try
            m_core = theCore
            m_searchObjectives = m_core.SearchObjective
            m_curRowCol = New cMPAOptOutput(theCore)
            m_parameters = New cMPAOptParameters(theCore)

            Me.setDefaults()

            Me.setActiveSearch(Me.m_core.m_MPAOptData.SearchType, True)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & " Failed to initialize EcoSeed.")
        End Try

    End Function

    Public Sub Connect(ByVal syncObject As System.ComponentModel.ISynchronizeInvoke, ByVal SeedCellCallback As SearchIterationDelegate, ByVal RunStateCallback As SearchRunStateDelegate)

        m_syncObject = syncObject
        m_SeedCellComputedCallback = SeedCellCallback
        m_SeedRunStateCallback = RunStateCallback

        Debug.Assert(m_syncObject IsNot Nothing, Me.ToString & ".Connect() syncObject is null.")
        Debug.Assert(SeedCellCallback IsNot Nothing, Me.ToString & ".Connect() SeedCellCallback is null.")
        Debug.Assert(m_SeedRunStateCallback IsNot Nothing, Me.ToString & ".Connect() SeedCellCallback is null.")

        If m_syncObject IsNot Nothing And m_SeedCellComputedCallback IsNot Nothing And m_SeedRunStateCallback IsNot Nothing Then
            m_bConnected = True
        Else
            m_bConnected = False
            cLog.Write("EcoSeedManager is not connected to an interface.")
        End If

    End Sub

    Private Sub setDefaults()
        'set the default period to run the model to be the same as the interface
        Me.m_core.MPAOptData.EcoSpaceEndYear = Me.m_core.nEcospaceYears
    End Sub

#End Region

#Region "Private methods"

#Region "Changing search models"

    Private Function SearchModelFactory(ByVal SearchType As eMPAOptimizationModels) As IMPASearchModel

        Debug.Assert(Me.IsRunning = False, Me.ToString & " Cannot change the search type while a search is running.")
        If Me.IsRunning Then
            Return Me.m_MPASearch
        End If

        Dim search As IMPASearchModel = Nothing

        Select Case SearchType

            Case eMPAOptimizationModels.EcoSeed
                search = New cEcoSeed()

            Case eMPAOptimizationModels.RandomSearch
                search = New cMPARandomSearch()

        End Select

        Return search

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Toggles the active search model to use.
    ''' </summary>
    ''' <param name="newActiveSearch">New search model to use.</param>
    ''' <param name="bForceInit">Flag indicating whether the search should be
    ''' initialized, even when the active search type does not change.</param>
    ''' -----------------------------------------------------------------------
    Private Sub setActiveSearch(ByVal newActiveSearch As eMPAOptimizationModels, ByVal bForceInit As Boolean)

        'if no search has been created then make sure the factory runs
        If Me.m_MPASearch IsNot Nothing Then

            Debug.Assert(Me.IsRunning = False, Me.ToString & " Can not change the search type while a search is running.")
            If Me.IsRunning Then
                System.Console.WriteLine(Me.ToString & " tried to change the search model while a search is running. Sorry Dude!!!")
                Exit Sub
            End If

            'the search type being set is the same as the current one so don't do anything
            If (newActiveSearch = Me.m_core.m_MPAOptData.SearchType) And (bForceInit = False) Then
                Exit Sub
            End If

        End If

        Try
            Me.m_MPASearch = Me.SearchModelFactory(newActiveSearch)
            Me.m_MPASearch.Init(Me.m_core.m_Ecospace, Me.m_core.MPAOptData)
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            Throw New ApplicationException("Error changing search models " & ex.Message, ex)
        End Try

        Me.m_core.m_MPAOptData.SearchType = newActiveSearch

    End Sub

#End Region

#Region "Events callbacks from Search Models"

    Private Sub OnSearchIteration()

        Try

            'populate the current row col
            m_curRowCol.Init(m_MPASearch.MPAOptData, Me.m_core.m_EcoSpaceData)

            If m_bConnected Then
                m_syncObject.BeginInvoke(Me.m_SeedCellComputedCallback, Nothing)
            Else
                System.Console.WriteLine("EcoSeedManager not connected to an interface.")
            End If

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnRunStateChanged(ByVal RunState As eRunStates)

        Try

            If RunState = eRunStates.NewBestResultFound Then
                m_curRowCol.Init(m_MPASearch.MPAOptData, Me.m_core.m_EcoSpaceData)
            End If

            If RunState = eRunStates.Completed Then
                'the run has completed release any waiting threads
                Me.ReleaseWait()
                Me.m_core.m_SearchData.SearchMode = eSearchModes.NotInSearch
            End If

            If m_bConnected Then

                'Invoke will wait for the function to return 
                'this lets the interface gather data before it has changed is response to a new best cell selected
                Me.m_syncObject.Invoke(Me.m_SeedRunStateCallback, New Object() {RunState})

            Else
                System.Console.WriteLine("EcoSeedManager not connected to an interface.")
            End If

        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub OnSendMessage(ByVal Message As EwECore.cMessage)
        Debug.Assert(False)
    End Sub

#End Region

#End Region

#Region "Public methods"

    Public Function Run() As Boolean

        Try
            Me.m_MPASearch.Connect(AddressOf OnSearchIteration, AddressOf Me.OnRunStateChanged, AddressOf Me.OnSendMessage)

            If Me.IsRunning Then
                Me.m_core.Messages.SendMessage(New cMessage(My.Resources.CoreMessages.MPAOPT_RUNNING, _
                                                            eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, _
                                                            eMessageImportance.Critical))
                Return False
            End If

            ' Test if no seed cells nor MPA
            If Not Me.m_MPASearch.OKtoRun Then
                Dim msg As New cFeedbackMessage(My.Resources.CoreMessages.MPAOPT_NODATA_RESUME, _
                                                eCoreComponentType.MPAOptimization, eMessageType.Any, _
                                                eMessageImportance.Warning, eMessageReplyStyle.YES_NO, _
                                                eDataTypes.MPAOptParameters, eMessageReply.NO)
                Me.m_core.Messages.SendMessage(msg)
                If msg.Reply = eMessageReply.NO Then Return False
            End If

            Dim strBaseDir As String = Me.m_dataDir
            If String.IsNullOrWhiteSpace(strBaseDir) Then
                strBaseDir = Me.m_core.DefaultOutputPath(eAutosaveTypes.MPAOpt)
            End If

            Dim strHeader As String = ""
            If Me.m_core.SaveWithFileHeader Then Me.m_core.DefaultFileHeader(eAutosaveTypes.MPAOpt)

            ' Configure autosave behaviour
            Me.m_MPASearch.ConfigureAutosave(Me.m_core.Autosave(eAutosaveTypes.MPAOpt), strBaseDir, strHeader)

            Me.SetWait()

            ' Keep a copy of the original MPA configuration
            Dim map(,) As Integer
            Me.m_orgMPAConfig = New Integer(Me.m_core.nMPAs)(,) {}
            For i As Integer = 1 To Me.m_core.nMPAs
                ReDim map(Me.m_core.EcospaceBasemap.InRow + 1, Me.m_core.EcospaceBasemap.InCol + 1)
                Me.m_orgMPAConfig(i) = map
                Array.Copy(Me.m_core.m_EcoSpaceData.MPA(i), Me.m_orgMPAConfig(i), Me.m_core.m_EcoSpaceData.MPA(i).Length)
            Next

            Me.m_core.m_SearchData.SearchMode = eSearchModes.SpatialOpt

            Me.m_thrSeed = New Threading.Thread(AddressOf Me.m_MPASearch.Run)
            Me.m_thrSeed.Start()

        Catch ex As Exception
            cLog.Write(ex)
            Me.m_core.m_SearchData.SearchMode = eSearchModes.NotInSearch
            Me.m_core.Messages.SendMessage(New cMessage(String.Format(My.Resources.CoreMessages.MPAOPT_ERROR, ex.Message), _
                                                        eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, _
                                                        eMessageImportance.Critical))
            Me.ReleaseWait()
            Return False
        End Try

        Return True

    End Function

    Public Sub YearTimeStep(ByRef iYear As Integer, ByVal Biomass() As Single)
        m_MPASearch.YearTimeStep(iYear, Biomass)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the run state of the current active <see cref="IMPASearchModel">MPA search</see>, if any.
    ''' </summary>
    ''' <returns>The run state of the current active <see cref="IMPASearchModel">MPA search</see>, if any.</returns>
    ''' <remarks>If no search is defined yet, the state <see cref="eRunStates.Idle"/> is returned.</remarks>
    ''' -----------------------------------------------------------------------
    Public Function RunState() As eRunStates

        If (Me.m_MPASearch Is Nothing) Then Return eRunStates.Idle
        Return (Me.m_MPASearch.RunState)

    End Function

    Public Overrides Function StopRun(Optional ByVal WaitTimeInMillSec As Integer = -1) As Boolean ' Implements SearchObjectives.ISearchObjective.StopRun
        Dim result As Boolean = True
        Try
            If Me.m_MPASearch IsNot Nothing Then
                Me.m_MPASearch.StopRun()
                result = Me.Wait(WaitTimeInMillSec)
            End If
        Catch ex As Exception
            result = False
        End Try

        Return result

    End Function

    Public Sub clearMPAs()
        Me.m_MPASearch.clearMPAs()
    End Sub

    Public Sub clearSeedCells()
        Me.m_MPASearch.clearSeedCells()
    End Sub

    Public Function setAllCellsToMPA(ByVal iMPA As Integer) As Boolean
        Return Me.m_MPASearch.setAllCellsToMPA(iMPA)
    End Function

    Public Function setAllCellsToSeed(ByVal iMPA As Integer) As Boolean
        Return Me.m_MPASearch.setAllCellsToSeed(iMPA)
    End Function

    ''' <summary>
    ''' Array of the number of times a cell was selected during the search
    ''' </summary>
    ''' <param name="TopPercentile">Top percentile of search results to include in the map</param>
    ''' <param name="NumberOfResults">Number of results in the top percentile</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CellSelectedMap(ByVal TopPercentile As Single, ByVal PercentAreaClosedFilter As Integer, ByRef NumberOfResults As Integer) As Integer(,)
        Dim map(,) As Integer
        Dim nResults As Integer
        Dim obj As cObjectiveResult
        Dim lstTemp As List(Of cObjectiveResult)

        Try

            Dim nR As Integer = Me.m_core.m_EcoSpaceData.InRow
            Dim nC As Integer = Me.m_core.m_EcoSpaceData.InCol
            ReDim map(nR, nC)

            'get a list of maps where the percentage of cells closed is what the user asked for
            lstTemp = Me.getPercentageList(PercentAreaClosedFilter)
            'sort the maps according the the objective function
            lstTemp.Sort()

            'bound the percentile
            TopPercentile = Math.Min(100, Math.Max(1, TopPercentile))

            'turn the TopPercentile into the number of results
            nResults = CInt(Math.Ceiling(lstTemp.Count * TopPercentile / 100.0!))

            'bound nResults
            If nResults < 1 Then nResults = 1
            If nResults > Me.m_MPASearch.Results.Count Then nResults = Me.m_MPASearch.Results.Count

            'populate the results map (an integer matrix) with the number of times a cell was included in the search
            For ires As Integer = 0 To nResults - 1

                obj = lstTemp.Item(ires)

                If obj.PercentageClosed = PercentAreaClosedFilter Then
                    NumberOfResults += 1
                    'count the number of hits to each cell in the map
                    For Each cell As cMPACell In obj.Cells
                        map(cell.Row, cell.Col) += 1
                    Next cell
                End If
            Next ires

            'return the hit count map
            Return map

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Me.m_core.Messages.SendMessage(New cMessage("MPA Optimization Error: " & ex.Message, eMessageType.ErrorEncountered, eCoreComponentType.SearchObjective, eMessageImportance.Critical))
            Return Nothing

        End Try

    End Function

    ''' <summary>
    ''' Return a list of result maps where the area closed = PercentAreaClosedFilter
    ''' </summary>
    ''' <param name="PercentAreaClosedFilter">Area of the map that is Closed (has an MPA set)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getPercentageList(ByVal PercentAreaClosedFilter As Integer) As List(Of cObjectiveResult)
        Dim lstOut As New List(Of cObjectiveResult)

        For iResult As Integer = 0 To Me.Results.Count - 1
            If Me.Results(iResult).PercentageClosed = PercentAreaClosedFilter Then
                lstOut.Add(Me.Results(iResult))
            End If
        Next

        Return lstOut

    End Function

#End Region

#Region " Public properties "

    ''' <summary>
    ''' Output object for the current Ecoseed interation 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property CurrentRowColResults() As cMPAOptOutput

        Get
            Return m_curRowCol
        End Get

    End Property


    Public ReadOnly Property MPAOptimizationParamters() As cMPAOptParameters
        Get
            Return m_parameters
        End Get
    End Property

    '''' <summary>
    '''' Best search result up to the current search iteration
    '''' </summary>
    '''' <value></value>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public ReadOnly Property BestResult() As cObjectiveResult
    '    Get
    '        Return Me.m_MPASearch.BestResult
    '    End Get
    'End Property


    Public ReadOnly Property Results() As List(Of cObjectiveResult)
        Get
            ' VC 13Nov08: Only sort for MPA, not for Ecoseed. 
            ' JS 13Nov08: The GUI will take care of sorting

            ''make sure the results are sorted
            'Me.m_MPASearch.Results.Sort()
            Return Me.m_MPASearch.Results
        End Get
    End Property

    Public ReadOnly Property OrgMPA() As Integer()(,)
        Get
            Return Me.m_orgMPAConfig
        End Get
    End Property

    Public ReadOnly Property nIterationsCompleted() As Integer
        Get
            Return Me.m_MPASearch.nInterationsCompleted
        End Get
    End Property

#End Region ' Public properties

#Region "ICoreInterface"

    Public ReadOnly Property DataType() As eDataTypes Implements ICoreInterface.DataType
        Get
            Return eDataTypes.MPAOptManager
        End Get
    End Property

    Public ReadOnly Property CoreComponent() As eCoreComponentType Implements ICoreInterface.CoreComponent
        Get
            Return eCoreComponentType.EcoSpace
        End Get
    End Property

    Public Property DBID() As Integer Implements ICoreInterface.DBID
        Get
            Return cCore.NULL_VALUE
        End Get
        Set(ByVal value As Integer)
            Debug.Assert(False, Me.ToString & ".DBID no implementation.")
        End Set
    End Property

    Public Function GetID() As String Implements ICoreInterface.GetID
        Return cCore.NULL_VALUE.ToString
    End Function

    Public Property Index() As Integer Implements ICoreInterface.Index
        Get
            Return cCore.NULL_VALUE
        End Get
        Set(ByVal value As Integer)
            Debug.Assert(False, Me.ToString & ".Index no implementation.")
        End Set
    End Property

    Public Property Name() As String Implements ICoreInterface.Name
        Get
            Return Me.ToString
        End Get
        Set(ByVal value As String)
            Debug.Assert(False, Me.ToString & ".Name no implementation.")
        End Set
    End Property

#End Region

#Region "ISearchObjective implementation"

    Public ReadOnly Property FleetObjectives(ByVal iFleet As Integer) As cSearchObjectiveFleetInput Implements ISearchObjective.FleetObjectives
        Get
            Return Me.m_searchObjectives.FleetObjectives(iFleet)
        End Get
    End Property

    Public ReadOnly Property GroupObjectives(ByVal iGroup As Integer) As cSearchObjectiveGroupInput Implements ISearchObjective.GroupObjectives
        Get
            Return Me.m_searchObjectives.GroupObjectives(iGroup)
        End Get
    End Property

    Public Function Load() As Boolean Implements ISearchObjective.Load

        Try
            Dim coreData As cMPAOptDataStructures = Me.m_core.MPAOptData

            Me.m_parameters.AllowValidation = False
            Me.m_parameters.SearchType = coreData.SearchType
            Me.m_parameters.StepSize = coreData.stepSize
            Me.m_parameters.BoundaryWeight = coreData.BoundaryWeight
            Me.m_parameters.MaxArea = coreData.MaxArea
            Me.m_parameters.MinArea = coreData.MinArea
            Me.m_parameters.nIterations = coreData.nIterations
            Me.m_parameters.iMPAToUse = coreData.iMPAtoUse
            Me.m_parameters.bUseCellWeight = coreData.bUseCellWeight

            Me.m_parameters.StartYear = coreData.EcoSpaceStartYear
            Me.m_parameters.EndYear = coreData.EcoSpaceEndYear

            Me.m_parameters.AllowValidation = True

            Me.m_parameters.ResetStatusFlags()

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Me.m_parameters.AllowValidation = True
            Return False
        End Try

        Return True

    End Function

    Public Sub Clear() Implements ISearchObjective.Clear
        ' NOP
        Try
            If Me.m_parameters IsNot Nothing Then Me.m_parameters.Clear()
            If Me.m_curRowCol IsNot Nothing Then Me.m_curRowCol.Clear()
        Catch ex As Exception

        End Try

    End Sub

    Public Function Update(ByVal DataType As eDataTypes) As Boolean Implements ISearchObjective.Update

        Try

            Dim coreData As cMPAOptDataStructures = Me.m_core.MPAOptData

            coreData.stepSize = Me.m_parameters.StepSize
            coreData.BoundaryWeight = Me.m_parameters.BoundaryWeight
            coreData.MaxArea = Me.m_parameters.MaxArea
            coreData.MinArea = Me.m_parameters.MinArea
            coreData.nIterations = Me.m_parameters.nIterations
            coreData.iMPAtoUse = Me.m_parameters.iMPAToUse
            coreData.bUseCellWeight = Me.m_parameters.bUseCellWeight

            coreData.EcoSpaceStartYear = Me.m_parameters.StartYear
            coreData.EcoSpaceEndYear = Me.m_parameters.EndYear

            Me.setActiveSearch(Me.m_parameters.SearchType, False)

        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.Message)
            Return False
        End Try

        Return True

    End Function

    Public ReadOnly Property ValueWeights() As cSearchObjectiveWeights Implements ISearchObjective.ValueWeights
        Get
            Return Me.m_searchObjectives.ValueWeights
        End Get
    End Property

    Public ReadOnly Property ObjectiveParameters() As SearchObjectives.cSearchObjectiveParameters Implements SearchObjectives.ISearchObjective.ObjectiveParameters
        Get
            Return Me.m_searchObjectives.ObjectiveParameters
        End Get
    End Property

#End Region

End Class

#End Region

#Region "IMPASearchModel definition"

Public Interface IMPASearchModel

    Sub Run()
    ReadOnly Property OKtoRun() As Boolean


    Function Init(ByRef EcoSpaceModel As cEcoSpace, ByRef MPAOptData As cMPAOptDataStructures) As Boolean

    Sub Connect(ByVal OnSearchInteration As cMPAOptManager.SearchIterationDelegate, _
                ByVal OnRunStateChanged As cMPAOptManager.SearchRunStateDelegate, _
                ByVal OnSendMessage As cMPAOptManager.SendMessageDelegate)

    Sub StopRun()
    Sub clearMPAs()
    Sub clearSeedCells()
    Function setAllCellsToMPA(ByVal iMPA As Integer) As Boolean
    Function setAllCellsToSeed(ByVal iMPA As Integer) As Boolean

    Property MPAOptData() As cMPAOptDataStructures
    ReadOnly Property isRunning() As Boolean
    ReadOnly Property EcospaceStartTime() As Single
    ReadOnly Property Results() As List(Of cObjectiveResult)
    ReadOnly Property nInterationsCompleted() As Integer

    Sub YearTimeStep(ByRef iYear As Integer, ByVal Biomass() As Single)

    ''' <summary>
    ''' Configure the search for auto-saving.
    ''' </summary>
    ''' <param name="bAutosave">Turn auto-saving on or off.</param>
    ''' <param name="strOutputPath">Path to auto-save to.</param>
    ''' <param name="strHeader">Header information to report when auto-saving.</param>
    Sub ConfigureAutosave(ByVal bAutosave As Boolean, ByVal strOutputPath As String, ByVal strHeader As String)

    Function RunState() As cMPAOptManager.eRunStates

End Interface

#End Region

#Region "cObjectiveResult class definition"

Public Class cObjectiveResult
    Implements IComparable(Of cObjectiveResult)

    Public Row As Integer
    Public Col As Integer
    Public objFuncEconomicValue As Single
    Public objFuncMandatedValue As Single
    Public objFuncSocialValue As Single
    Public objFuncEcologicalValue As Single
    Public objFuncAreaBorder As Single
    Public objBiomassDiversity As Single


    ''' <summary>
    ''' Includes weights
    ''' </summary>
    ''' <remarks></remarks>
    Public objFuncTotal As Single

    Public SearchType As eMPAOptimizationModels
    Public Cells As List(Of cMPACell)
    Public PercentageClosed As Integer


    Public Sub New(ByRef MPAData As cMPAOptDataStructures, ByRef SpaceData As cEcospaceDataStructures)

        Row = MPAData.bestrow
        Col = MPAData.bestcol

        objFuncEconomicValue = MPAData.objFuncEconomicValue
        objFuncMandatedValue = MPAData.objFuncMandatedValue
        objFuncSocialValue = MPAData.objFuncSocialValue
        objFuncEcologicalValue = MPAData.objFuncEcologicalValue
        objBiomassDiversity = MPAData.objFuncBiodiversity

        objFuncAreaBorder = MPAData.objFuncAreaBorder

        objFuncTotal = MPAData.objFuncTotal

        SearchType = MPAData.SearchType

        'copy the list of cells into a new list 
        Cells = New List(Of cMPACell)(MPAData.Cells)

        calcPercentageClosed(MPAData, SpaceData)

    End Sub

    Public Sub Init(ByRef MPAData As cMPAOptDataStructures, ByRef SpaceData As cEcospaceDataStructures)


        Try
            objFuncEconomicValue = MPAData.objFuncEconomicValue
            objFuncMandatedValue = MPAData.objFuncMandatedValue
            objFuncSocialValue = MPAData.objFuncSocialValue
            objFuncEcologicalValue = MPAData.objFuncEcologicalValue
            objFuncAreaBorder = MPAData.objFuncAreaBorder
            objBiomassDiversity = MPAData.objFuncBiodiversity
            objFuncTotal = MPAData.objFuncTotal

            Select Case MPAData.SearchType

                Case eMPAOptimizationModels.EcoSeed

                    Debug.Assert(SpaceData IsNot Nothing, Me.ToString & ".Init() SpaceData must be passed in!")
                    Cells.Clear()
                    For ir As Integer = 1 To SpaceData.InRow
                        For ic As Integer = 1 To SpaceData.InCol
                            'If SpaceData.MPA(ir, ic) <> 0 Then
                            '    Cells.Add(New cMPACell(ir, ic, SpaceData.MPA(ir, ic)))
                            'End If
                            For impa As Integer = 1 To SpaceData.MPAno
                                If SpaceData.MPA(impa)(ir, ic) Then
                                    Cells.Add(New cMPACell(ir, ic, impa))
                                End If
                            Next
                        Next
                    Next

                Case eMPAOptimizationModels.RandomSearch
                    Cells = New List(Of cMPACell)(MPAData.Cells)

            End Select

            calcPercentageClosed(MPAData, SpaceData)

        Catch ex As Exception
            cLog.Write(ex)
            Throw New ApplicationException(Me.ToString & ".Init() Error: " & ex.Message, ex)
        End Try

    End Sub


    Private Sub calcPercentageClosed(ByRef MPAData As cMPAOptDataStructures, ByRef SpaceData As cEcospaceDataStructures)
        'what percentage of the area is closed
        Dim nTotCells As Integer = SpaceData.nWaterCells
        Dim nMPACells As Integer
        For ir As Integer = 1 To SpaceData.InRow
            For ic As Integer = 1 To SpaceData.InCol
                'If SpaceData.MPA(ir, ic) = MPAData.iMPAtoUse Then
                If SpaceData.MPA(MPAData.iMPAtoUse)(ir, ic) = True Then
                    nMPACells += 1
                End If
            Next
        Next
        Me.PercentageClosed = CInt(nMPACells / nTotCells * 100)
    End Sub

    Public Overrides Function ToString() As String

        Return "Total weighted value = " & objFuncTotal.ToString & ", Economic = " & objFuncEconomicValue.ToString & ", Mandated = " & objFuncMandatedValue.ToString _
                & ", Social = " & objFuncSocialValue.ToString & ", Ecological = " & objFuncEcologicalValue.ToString
    End Function

    Public Function CompareTo(ByVal other As cObjectiveResult) As Integer Implements System.IComparable(Of cObjectiveResult).CompareTo

        'Sort in reverse order
        'Biggest first
        If Me.objFuncTotal < other.objFuncTotal Then
            Return 1
        ElseIf Me.objFuncTotal = other.objFuncTotal Then
            Return 0
        ElseIf Me.objFuncTotal > other.objFuncTotal Then
            Return -1
        End If

    End Function
End Class


#End Region


