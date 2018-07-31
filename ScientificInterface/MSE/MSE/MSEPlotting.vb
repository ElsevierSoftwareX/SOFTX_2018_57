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
Option Explicit On

Imports EwECore
Imports EwECore.MSE
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

Imports EwEUtils.Utilities
Imports ZedGraph
Imports EwECore.Style

#End Region

#Region "Enumerators"

''' -----------------------------------------------------------------------
''' <summary>
''' Type of data being plotted.
''' </summary>
''' <remarks>This can't be EwECore.eDataTypes because the data comes from the same Core objects. 
''' Can't use eVarNameFlags because there is more than one type of data in a plot.</remarks>
''' -----------------------------------------------------------------------
Public Enum eMSEPlotData As Integer
    NotSet = 0
    Biomass
    GroupCatch
    FleetValue
    Effort
    BioEst
    FleetTotValue
    Value
    FishingMortalityComparison
End Enum

''' -----------------------------------------------------------------------
''' <summary>
''' Enumerated type, stating supported types of MSE plots.
''' </summary>
''' -----------------------------------------------------------------------
Friend Enum eMSEPlotTypes
    ''' <summary>Plot a historgram.</summary>
    Histogram
    ''' <summary>Plot a mass lines plot.</summary>
    Values
    ''' <summary>Plot a single line.</summary>
    Line
End Enum

#End Region

#Region "Plotting Class"

Friend Class cMSERefPoint

    Private m_low As Single
    Private m_upper As Single

    Public Sub New(ByVal LowerRef As Single, ByVal UpperRef As Single)
        Me.m_low = LowerRef
        Me.m_upper = UpperRef
    End Sub

    Public Property LowerReference() As Single
        Get
            Return Me.m_low
        End Get
        Set(ByVal value As Single)
            Me.m_low = value
        End Set
    End Property

    Public Property UpperReference() As Single
        Get
            Return Me.m_upper
        End Get
        Set(ByVal value As Single)
            Me.m_upper = value
        End Set
    End Property

End Class

Friend Class cMSEPlotter

#Region "Private data"

    Private Const LB_TAG As String = "LB"
    Private Const UB_TAG As String = "UB"

    Private m_uic As cUIContext = Nothing
    Private m_zgh As cZedGraphHelper = New cZedGraphHelper()
    Private m_zdGraph As ZedGraphControl
    Private m_manager As cMSEManager
    Private m_nvis As Integer
    Private m_type As eMSEPlotTypes
    Private m_dataType As eMSEPlotData
    Private m_Data As List(Of cCoreGroupBase)
    Private m_RefPoints As List(Of cMSERefPoint)
    Private m_nLines As Integer
    Private m_iIterMin As Integer = Integer.MinValue
    Private m_iIterMax As Integer = Integer.MaxValue

#End Region

#Region "Public interface"

    ''' <summary>
    ''' Initialize to ZedGraphHelper and a Zedgraph control
    ''' </summary>
    ''' <param name="uic">UI context to use.</param>
    ''' <param name="graph">Graph to use.</param>
    Public Sub Init(ByVal uic As cUIContext, _
                    ByVal MSEManager As cMSEManager, _
                    ByVal graph As ZedGraphControl)

        Me.m_uic = uic
        Me.m_zdGraph = graph
        Me.m_manager = MSEManager

        Me.m_zgh.Attach(Me.m_uic, Me.m_zdGraph, Me.NumVisPanes)

    End Sub

    Public Sub Detach()
        Try
            If Me.m_Data IsNot Nothing Then
                For Each ob As cCoreGroupBase In Me.m_Data
                    ob.Clear()
                Next
                Me.m_Data.Clear()
            End If

            If Me.m_RefPoints IsNot Nothing Then Me.m_RefPoints.Clear()
            Me.m_Data = Nothing
            Me.m_RefPoints = Nothing
            Me.m_zgh.Detach()
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Get/set how the current data is to be plotted.
    ''' </summary>
    Public Property PlotType() As eMSEPlotTypes
        Get
            Return Me.m_type
        End Get
        Set(ByVal value As eMSEPlotTypes)
            m_type = value
        End Set
    End Property

    ''' <summary>
    ''' What type of data is being plotted. Used mostly for labels
    ''' </summary>
    Public Property DataType() As eMSEPlotData
        Get
            Return Me.m_dataType
        End Get
        Set(ByVal value As eMSEPlotData)
            m_dataType = value
        End Set
    End Property

    Public Sub PlotIterations(ByVal iIterMin As Integer, ByVal iIterMax As Integer)
        Me.m_iIterMin = iIterMin
        Me.m_iIterMax = iIterMax
        ' Redraw if need be
        If Me.m_Data IsNot Nothing Then
            Me.Draw()
        End If
    End Sub

    Public Sub Clear()
        Try
            Me.m_zgh.NumPanes = Me.NumVisPanes

            Me.ClearData()
            Me.ClearGraphs()
            Me.ConfigPanes()
            Me.m_nLines = 0

            'this forces zedgraph to recalculte the layout of the new panes
            Me.m_zdGraph.MasterPane.DoLayout(Nothing)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Draw() Exception: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Plot the current data
    ''' </summary>
    Public Sub Draw()
        Try

            If Me.m_Data IsNot Nothing Then

                If Me.m_type <> eMSEPlotTypes.Line Then
                    Me.m_zgh.NumPanes = Me.NumVisPanes
                    Me.ClearGraphs()
                    Me.ConfigPanes()
                End If

                Me.plotRefLines()

                Select Case m_type
                    Case eMSEPlotTypes.Histogram
                        Me.PlotHistoGram()
                    Case eMSEPlotTypes.Values
                        Me.PlotValues()
                    Case eMSEPlotTypes.Line
                        Me.Plotline()
                End Select

            End If

            'this forces zedgraph to recalculte the layout of the new panes
            Me.m_zdGraph.MasterPane.DoLayout(Nothing)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Draw() Exception: " & ex.Message)
            cLog.Write(ex)
        End Try

    End Sub

    ''' <summary>
    ''' Added data to be plotted
    ''' </summary>
    ''' <param name="ListOfData"></param>
    Public Sub AddData(ByVal ListOfData As List(Of cCoreGroupBase))

        Try

            If Me.m_Data IsNot Nothing Then
                Me.m_Data.Clear()
            End If
            Me.m_Data = ListOfData

            If Me.m_Data IsNot Nothing Then
                Me.m_zgh.IsAxisLabelsVisible = (Me.m_Data.Count < 10)
            End If

            'if we are adding one line at a time
            'and this is the first line then configure the panes
            If Me.m_type = eMSEPlotTypes.Line And Me.m_nLines = 0 Then
                Me.ConfigValuePanes()
            End If

            Me.m_nLines += 1

        Catch ex As Exception
            System.Console.WriteLine(ex.Message)
            cLog.Write(ex)
        End Try

    End Sub

    ''' <summary>
    ''' Added Mean data to be plotted
    ''' </summary>
    Public Sub AddMean()

        Dim stats As cMSEStats
        Dim ipane As Integer
        Try

            'Only data added one line at a time should be plotted this way
            If Me.m_type <> eMSEPlotTypes.Line Then Exit Sub

            For Each data As cCoreGroupBase In Me.m_Data
                ipane += 1
                stats = Me.m_manager.BiomassStats(data.Index)
                '   Me.m_zgh.AutoScaleYOption(ipane) = cZedGraphHelper.eScaleOptionTypes.None
                Me.PlotMean(stats, ipane)
            Next

        Catch ex As Exception
            System.Console.WriteLine(ex.Message)
            cLog.Write(ex)
        End Try


    End Sub

    ''' <summary>
    ''' Added the Reference lines to the plots
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub AddReference()

        plotRefLines()

    End Sub

    Private Sub plotRefLines()
        Dim ipane As Integer

        Try

            'Reference lines are retrieved from MSEManager based on the type of data that is being plotted
            'this should mean the reference lines are always in sync with the current data
            For Each statobj As cCoreGroupBase In Me.m_Data
                ipane += 1
                'get the reference data from the core for this datatype
                Dim RefPoint As cMSERefPoint = Me.getRefPoint(statobj.Index)
                If Not RefPoint Is Nothing Then
                    'Add the data to the graph pane, this will remove existing reference lines
                    Me.PlotRefLine(RefPoint.LowerReference, RefPoint.UpperReference, Me.m_zgh.GetPane(ipane))
                End If

                'Do NOT rescale if this is a Histogram
                If Me.m_type <> eMSEPlotTypes.Histogram Then
                    ' Me.m_zgh.AutoscalePane(ipane) = True
                End If

            Next

            Me.m_zgh.Redraw()

        Catch ex As Exception
            System.Console.WriteLine(ex.Message)
            cLog.Write(ex)
        End Try

    End Sub


    Private Function getRefPoint(ByVal ItemIndex As Integer) As cMSERefPoint

        Dim refPoint As cMSERefPoint = Nothing

        Try
            Select Case Me.m_dataType

                Case eMSEPlotData.Biomass
                    Dim grp As cMSEGroupInput = Me.m_manager.GroupInputs(ItemIndex)
                    refPoint = New cMSERefPoint(grp.BiomassRefLower, grp.BiomassRefUpper)

                Case eMSEPlotData.BioEst
                    Dim grp As cMSEGroupInput = Me.m_manager.GroupInputs(ItemIndex)
                    refPoint = New cMSERefPoint(grp.BiomassEstRefLower, grp.BiomassEstRefUpper)

                Case eMSEPlotData.GroupCatch
                    Dim grp As cMSEGroupInput = Me.m_manager.GroupInputs(ItemIndex)
                    refPoint = New cMSERefPoint(grp.CatchRefLower, grp.CatchRefUpper)
                Case eMSEPlotData.FleetValue
                    Dim flt As cMSEFleetInput = Me.m_manager.EcopathFleetInputs(ItemIndex)
                    refPoint = New cMSERefPoint(flt.CatchRefLower, flt.CatchRefUpper)
                Case eMSEPlotData.Effort
                    Dim flt As cMSEFleetInput = Me.m_manager.EcopathFleetInputs(ItemIndex)
                    refPoint = New cMSERefPoint(flt.EffortRefLower, flt.EffortRefUpper)

            End Select

        Catch ex As Exception
            System.Console.WriteLine(ex.Message)
            cLog.Write(ex)
        End Try


        Return refPoint

    End Function

#End Region

#Region "Private methods"

    Private Sub ConfigPanes()
        If Me.m_type = eMSEPlotTypes.Histogram Then
            ConfigHistoPanes()
        ElseIf Me.m_type = eMSEPlotTypes.Values Or Me.m_type = eMSEPlotTypes.Line Then
            ConfigValuePanes()
        End If
    End Sub

    Private Sub ConfigHistoPanes()

        Me.m_zgh.YScaleGrace = 1.1
        Dim ipane As Integer
        Dim min As Single, max As Single
        For Each data As cMSEStats In m_Data
            ipane += 1
            min = data.Min
            max = data.Max
            If data.Max = 0.0F Then
                min = 0
                max = 1
            End If
            Me.m_zgh.ConfigurePane(data.Name, Me.XLabel, data.Min, data.Max, Me.YLabel, 0, 1, False, LegendPos.Top, ipane)
            Me.m_zgh.AutoscalePane(ipane) = False
        Next

    End Sub

    Friend Function IsGroupVisible(ByVal GroupIndex As Integer) As Boolean

        If Me.m_uic.StyleGuide.GroupVisible(GroupIndex) Then

            If Me.m_dataType <> eMSEPlotData.GroupCatch Then
                Return True
            End If

            Dim grp As cEcoPathGroupInput = Me.m_uic.Core.EcoPathGroupInputs(GroupIndex)
            If Me.m_dataType = eMSEPlotData.GroupCatch And grp.IsFished Then
                'For ePlotData.GroupCatch only fished groups are visible
                Return True
            End If

        End If

        Return False

    End Function

    Private Sub ConfigValuePanes()
        Dim ipane As Integer
        Dim xStart As Double = CDbl(Me.m_uic.Core.EcosimFirstYear)

        Select Case Me.m_dataType

            Case eMSEPlotData.Biomass, eMSEPlotData.GroupCatch, eMSEPlotData.BioEst
                'By group
                Dim grp As cCoreGroupBase = Nothing
                For i As Integer = 1 To Me.m_manager.NumGroups
                    grp = Me.m_manager.GroupInputs(i)
                    'figure out if this group is visible
                    If Me.IsGroupVisible(grp.Index) Then
                        'Only configure the pane if this group is visible
                        ipane += 1
                        Me.m_zgh.ConfigurePane(grp.Name, Me.XLabel, xStart, _
                                               CDbl(Me.m_uic.Core.EcosimFirstYear + (Me.m_uic.Core.nEcosimTimeSteps / cCore.N_MONTHS)), _
                                               Me.YLabel, 0, 0, False, LegendPos.Top, ipane)
                        Me.m_zgh.AutoscalePane(ipane) = True
                    End If

                Next

            Case eMSEPlotData.FishingMortalityComparison
                'By group
                Dim grp As cCoreGroupBase = Nothing
                For i As Integer = 1 To Me.m_manager.NumGroups
                    grp = Me.m_manager.GroupInputs(i)
                    'figure out if this group is visible
                    If Me.IsGroupVisible(grp.Index) Then
                        'Only configure the pane if this group is visible
                        ipane += 1
                        Me.m_zgh.ConfigurePane(grp.Name, Me.XLabel, xStart, _
                                               CDbl(Me.m_uic.Core.EcosimFirstYear + (Me.m_uic.Core.nEcosimTimeSteps / cCore.N_MONTHS)), _
                                               Me.YLabel, 0, 1.0, False, LegendPos.Top, ipane)
                        Me.m_zgh.AutoscalePane(ipane) = True
                    End If

                Next

            Case eMSEPlotData.Effort, eMSEPlotData.FleetValue
                'By Fleet
                Dim flt As cEcopathFleetInput
                For iflt As Integer = 1 To Me.m_uic.Core.nFleets
                    flt = Me.m_uic.Core.EcopathFleetInputs(iflt)
                    If Me.m_uic.StyleGuide.FleetVisible(flt.Index) Then
                        ipane += 1
                        Me.m_zgh.ConfigurePane(flt.Name, Me.XLabel, xStart, _
                                               CDbl(Me.m_uic.Core.EcosimFirstYear + (Me.m_uic.Core.nEcosimTimeSteps / cCore.N_MONTHS)), _
                                               Me.YLabel, 0, 0, _
                                               False, LegendPos.Top, ipane)
                        Me.m_zgh.AutoscalePane(ipane) = True
                    End If
                Next

            Case eMSEPlotData.FleetTotValue

                'By Fleet

                Me.m_zgh.ConfigurePane("Value combined fleets", Me.XLabel, xStart, _
                                       CDbl(Me.m_uic.Core.EcosimFirstYear + (Me.m_uic.Core.nEcosimTimeSteps / cCore.N_MONTHS)), _
                                       Me.YLabel, 0, 0, _
                                       False, LegendPos.Top, 1)
                Me.m_zgh.AutoscalePane(1) = True

        End Select

        ' Show values under the cursor
        Me.m_zgh.ShowPointValue = True

    End Sub

    Private ReadOnly Property YLabel() As String
        Get
            Select Case Me.m_type

                Case eMSEPlotTypes.Histogram
                    Return SharedResources.HEADER_PROBABILITY

                Case eMSEPlotTypes.Line, eMSEPlotTypes.Values
                    Select Case Me.m_dataType
                        Case eMSEPlotData.Biomass
                            Return SharedResources.HEADER_BIOMASS
                        Case eMSEPlotData.Effort
                            Return SharedResources.HEADER_EFFORT
                        Case eMSEPlotData.FleetValue
                            Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, SharedResources.HEADER_CATCHVALUE, m_uic.StyleGuide.FormatUnitString(cUnits.Monetary))
                        Case eMSEPlotData.GroupCatch
                            Return SharedResources.HEADER_CATCH_WEIGHT
                        Case eMSEPlotData.FishingMortalityComparison
                            ' ToDo: add Y label
                            Return ""
                    End Select

            End Select


            Return ""

        End Get
    End Property


    Private ReadOnly Property XLabel() As String
        Get
            Select Case Me.m_type

                Case eMSEPlotTypes.Histogram

                    Select Case Me.m_dataType
                        Case eMSEPlotData.Biomass
                            ' ToDo: add unit
                            Return SharedResources.HEADER_BIOMASS
                        Case eMSEPlotData.Effort
                            ' ToDo: add unit
                            Return SharedResources.HEADER_EFFORT
                        Case eMSEPlotData.FleetValue
                            'Return Me.m_uic.StyleGuide.FormatUnitString(SharedResources.HEADER_CATCHVALUE_UNIT, _
                            '                                            New cUnitFormatter() {cUnitFormatter.Monetary})
                        Case eMSEPlotData.GroupCatch
                            ' ToDo: add unit
                            Return SharedResources.HEADER_CATCH_WEIGHT
                    End Select

                Case eMSEPlotTypes.Line, eMSEPlotTypes.Values

                    Return SharedResources.HEADER_YEAR

            End Select

            Return ""
        End Get
    End Property

    Private Sub ClearData()

        If Me.m_Data IsNot Nothing Then
            Me.m_Data.Clear()
        End If

        If Me.m_RefPoints IsNot Nothing Then
            Me.m_RefPoints.Clear()
        End If

    End Sub

    Private Sub ClearGraphs()
        For Each Pane As GraphPane In Me.m_zgh.Graph.MasterPane.PaneList
            Pane.CurveList.Clear()
        Next
    End Sub

    Private Sub PlotHistoGram()

        Try

            Dim ipane As Integer
            Dim dx As Double
            Dim binWidth As Single
            Dim min As Single

            For Each data As cMSEStats In Me.m_Data '
                ipane += 1
                binWidth = data.BinWidths
                min = data.Min
                Dim max As Single = Single.MinValue
                Dim ppl As New PointPairList
                For ibin As Integer = 1 To data.nBins
                    dx = min + binWidth * (ibin - 1)
                    ppl.Add(dx, data.Histogram(ibin))

                    dx = min + binWidth * ibin
                    ppl.Add(dx, data.Histogram(ibin))

                    If ibin <> data.nBins Then
                        'draw the start of the next bin/column if this is not the last bin
                        ppl.Add(dx, data.Histogram(ibin + 1))
                    End If

                    max = Math.Max(max, data.Histogram(ibin))
                Next

                Dim pane As ZedGraph.GraphPane = Me.m_zgh.GetPane(ipane)
                Dim li As LineItem = pane.AddCurve(data.Name, ppl, System.Drawing.Color.Black, SymbolType.None)
                li.Line.Fill = New Fill(System.Drawing.Color.Gray)
                li.IsOverrideOrdinal = True

                pane.XAxis.Type = AxisType.Linear
                pane.YAxis.Scale.Max = max * Me.m_zgh.YScaleGrace

            Next

            Me.m_zgh.RescaleAndRedraw()

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".AddLineToGraph() Error: " & ex.Message)
            cLog.Write(ex)
        End Try

    End Sub


    Private Sub PlotValues()

        Try

            Dim ipane As Integer
            Dim dx As Double
            Dim x As Double
            Dim values() As Single
            Dim lstLines As New List(Of ZedGraph.LineItem)

            For Each data As cMSEStats In Me.m_Data '
                ipane += 1
                dx = 1 / data.nStepsPerYear
                lstLines.Clear()

                For iter As Integer = Math.Max(1, Math.Min(Me.m_iIterMin, Me.m_iIterMax)) To Math.Min(data.nIterations, Math.Max(Me.m_iIterMin, Me.m_iIterMax))
                    Dim ppl As New PointPairList
                    'get the values for this interation
                    values = data.Values(iter)
                    'reset the x starting value
                    x = Me.m_uic.Core.EcosimFirstYear
                    'add a point for each value
                    For iTime As Integer = 1 To data.nTimeSteps
                        ppl.Add(x, values(iTime))
                        x += dx
                    Next

                    Dim Line As LineItem = Me.m_zgh.CreateLineItem(data.Name, eSketchDrawModeTypes.Line, Color.Gray, ppl)
                    lstLines.Add(Line)

                Next

                ''set the y max
                'Me.m_zgh.YScaleMax(ipane) = data.Max * Me.m_zgh.YScaleGrace
                'Me.m_zgh.YScaleMin(ipane) = 0

                Me.m_zgh.PlotLines(lstLines.ToArray, ipane, False, False)
                Me.PlotMean(data, ipane)
            Next

            Me.m_zgh.RescaleAndRedraw()

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".AddLineToGraph() Error: " & ex.Message)
            cLog.Write(ex)
        End Try

    End Sub

    ''' <summary>
    ''' Plot one line across all the groups/fleets
    ''' </summary>
    Private Sub Plotline()

        Try

            Dim ipane As Integer
            Dim dx As Double = 1 / cCore.N_MONTHS
            Dim x As Double
            Dim lstLines As New List(Of ZedGraph.LineItem)

            For Each data As cMSEGroupOutput In Me.m_Data '
                ipane += 1
                lstLines.Clear()

                Dim ppl As New PointPairList
                x = Me.m_uic.Core.EcosimFirstYear
                For iTime As Integer = 1 To Me.m_uic.Core.nEcosimTimeSteps

                    ppl.Add(x, data.Biomass(iTime))
                    x += dx
                Next

                Dim Line As LineItem = Me.m_zgh.CreateLineItem(data.Name, eSketchDrawModeTypes.Line, Color.Gray, ppl)
                lstLines.Add(Line)

                Me.m_zgh.PlotLines(lstLines.ToArray, ipane, True, False)
            Next

            Me.m_zgh.RescaleAndRedraw()

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".AddLineToGraph() Error: " & ex.Message)
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub PlotComparison()

        ' ToDo: localize this

        Try

            Dim ipane As Integer
            Dim dx As Double
            Dim x As Double
            Dim values() As Single
            Dim lstLines As New List(Of ZedGraph.LineItem)

            Debug.Assert(Me.m_Data.Count = 2)

            For iData As Integer = 0 To 1
                Dim data As cMSEStats = DirectCast(Me.m_Data(iData), cMSEStats)
                Dim strLabel As String = ""
                Dim clr As Color = Color.Gray

                ipane += 1
                dx = 1 / data.nStepsPerYear
                lstLines.Clear()

                For iter As Integer = Math.Max(1, Math.Min(Me.m_iIterMin, Me.m_iIterMax)) To Math.Min(data.nIterations, Math.Max(Me.m_iIterMin, Me.m_iIterMax))
                    Dim ppl As New PointPairList
                    'get the values for this interation
                    values = data.Values(iter)
                    'reset the x starting value
                    x = Me.m_uic.Core.EcosimFirstYear
                    'add a point for each value
                    For iTime As Integer = 1 To data.nTimeSteps
                        ppl.Add(x, values(iTime))
                        x += dx
                    Next

                    If iData = 0 Then
                        strLabel = cStringUtils.Localize("Label 1 {0}", data.Name)
                        clr = Color.Blue
                    Else
                        strLabel = cStringUtils.Localize("Label 2 {0}", data.Name)
                        clr = Color.Red
                    End If
                    Dim Line As LineItem = Me.m_zgh.CreateLineItem(strLabel, eSketchDrawModeTypes.Line, clr, ppl)
                    lstLines.Add(Line)
                Next

                'set the y max
                ' JS: this should not be necessary
                Me.m_zgh.YScaleMax(ipane) = data.Max * Me.m_zgh.YScaleGrace
                Me.m_zgh.YScaleMin(ipane) = 0

                Me.m_zgh.PlotLines(lstLines.ToArray, ipane, False, False)
                Me.PlotMean(data, ipane)
            Next

            Me.m_zgh.RescaleAndRedraw()

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".AddLineToGraph() Error: " & ex.Message)
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub PlotMean(ByVal StatsData As cMSEStats, ByVal ipane As Integer)

        Dim x As Double, dx As Double
        Dim ppl As PointPairList = Nothing
        Dim li As LineItem = Nothing

        'time varing mean
        ppl = New PointPairList()
        x = Me.m_uic.Core.EcosimFirstYear
        dx = 1 / StatsData.nStepsPerYear
        For iTime As Integer = 1 To StatsData.nTimeSteps
            ppl.Add(x, StatsData.Mean(iTime))
            x += dx
        Next
        li = Me.m_zgh.CreateLineItem("", eSketchDrawModeTypes.NotSet, Me.GetLineColour(StatsData), ppl)
        li.Line.Width = 2

        Me.m_zgh.GetPane(ipane).CurveList.Insert(0, li)

        'mean over all the data(solid blue line)
        ppl = New PointPairList()
        ppl.Add(0, StatsData.Mean)
        ppl.Add(x, StatsData.Mean)
        li = Me.m_zgh.CreateLineItem("", eSketchDrawModeTypes.NotSet, Color.Blue, ppl)
        Me.m_zgh.GetPane(ipane).CurveList.Insert(0, li)

        'jb 26-Sept-2012 removed to upper and lower standard deviation lines for formating of graph 
        'in some circumstances the lines caused the graph to scale < 0
        ''2 standard deviation lines
        'Dim std2 As Single = 2 * StatsData.Std
        'ppl = New PointPairList()
        'ppl.Add(0, StatsData.Mean + std2)
        'ppl.Add(x, StatsData.Mean + std2)
        'li = Me.m_zgh.CreateLineItem("", eLineType.NotSet, Color.Blue, ppl)
        'li.Line.Style = Drawing2D.DashStyle.Dot
        'li.Line.Width = 0.5
        'Me.m_zgh.GetPane(ipane).CurveList.Insert(0, li)

        'ppl = New PointPairList()
        'ppl.Add(0, StatsData.Mean - std2)
        'ppl.Add(x, StatsData.Mean - std2)
        'li = Me.m_zgh.CreateLineItem("", eLineType.NotSet, Color.Blue, ppl)
        'li.Line.Style = Drawing2D.DashStyle.Dot
        'li.Line.Width = 0.5
        'Me.m_zgh.GetPane(ipane).CurveList.Insert(0, li)

    End Sub

    Private Sub PlotRefLine(ByVal LowerBound As Single, ByVal UpperBound As Single, ByVal pane As ZedGraph.GraphPane)
        'Dim dx As Double

        Me.RemoveRefLines(pane)

        If Me.m_type = eMSEPlotTypes.Histogram Then
            'Histogram plot

            Dim pplLB As New PointPairList
            Dim pplUB As New PointPairList

            pplLB.Add(LowerBound, 0)
            pplLB.Add(LowerBound, 1)
            pplUB.Add(UpperBound, 0)
            pplUB.Add(UpperBound, 1)
            Dim crv As ZedGraph.CurveItem
            crv = pane.AddCurve("", pplLB, System.Drawing.Color.Pink, SymbolType.None)
            crv.IsOverrideOrdinal = True
            crv.Tag = LB_TAG

            crv = pane.AddCurve("", pplUB, System.Drawing.Color.Pink, SymbolType.None)
            crv.IsOverrideOrdinal = True
            crv.Tag = UB_TAG

        Else
            'Line plot
            Dim pplLB As New PointPairList
            Dim pplUB As New PointPairList
            Dim xlast As Double = Me.m_uic.Core.nEcosimTimeSteps / cCore.N_MONTHS

            pplLB.Add(0, LowerBound)
            pplLB.Add(xlast, LowerBound)

            pplUB.Add(0, UpperBound)
            pplUB.Add(xlast, UpperBound)

            Dim LBItem As LineItem = Me.m_zgh.CreateLineItem("", eSketchDrawModeTypes.NotSet, Color.Pink, pplLB)
            LBItem.Line.Style = Drawing2D.DashStyle.Dash

            Dim UBItem As LineItem = Me.m_zgh.CreateLineItem("", eSketchDrawModeTypes.NotSet, Color.Pink, pplUB)
            UBItem.Line.Style = Drawing2D.DashStyle.Dash
            LBItem.Tag = LB_TAG
            UBItem.Tag = UB_TAG
            Try
                pane.CurveList.Insert(0, LBItem)
                pane.CurveList.Insert(0, UBItem)
            Catch ex As Exception
                System.Console.WriteLine(Me.ToString & ".AddMeanLineToGraph() Error: " & ex.Message)
            End Try

        End If

    End Sub

    Private Sub RemoveRefLines(ByVal pane As ZedGraph.GraphPane)

        Dim lbIndex As Integer = pane.CurveList.IndexOfTag(LB_TAG)
        If lbIndex > -1 Then
            pane.CurveList.RemoveAt(lbIndex)
        End If

        Dim ubIndex As Integer = pane.CurveList.IndexOfTag(UB_TAG)
        If ubIndex > -1 Then
            pane.CurveList.RemoveAt(ubIndex)
        End If

    End Sub

    Private Function GetLineColour(ByVal StatsData As cMSEStats) As Color

        Try
            'if this is group data then get the colour from the style guide
            If StatsData.DataType = eDataTypes.MSECatchByGroupStats Or StatsData.DataType = eDataTypes.MSEBiomassStats Then
                Return Me.m_uic.StyleGuide.GroupColor(Me.m_uic.Core, StatsData.Index)
            End If
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".getLineColour() Exception thrown from the Style Guide. Default colour will be used.")
        End Try

        'Not group data or the style guide through an error so just return a colour
        Return Color.Red

    End Function

    Private Function NumVisGroups() As Integer
        Dim n As Integer
        For igrp As Integer = 1 To Me.m_uic.Core.nLivingGroups
            If Me.IsGroupVisible(igrp) Then
                n += 1
            End If
        Next
        Return n
    End Function

    Private Function NumVisFleets() As Integer
        Dim n As Integer
        For igrp As Integer = 1 To Me.m_uic.Core.nFleets
            If Me.m_uic.StyleGuide.FleetVisible(igrp) Then
                n += 1
            End If
        Next
        Return n
    End Function

    Friend Function NumVisPanes() As Integer

        If Me.m_dataType = eMSEPlotData.FleetTotValue Then
            Return 1
        End If

        If Me.m_dataType = eMSEPlotData.Effort Or Me.m_dataType = eMSEPlotData.FleetValue Then
            Return NumVisFleets()
        End If

        Return NumVisGroups()

    End Function

#End Region

End Class

#End Region

