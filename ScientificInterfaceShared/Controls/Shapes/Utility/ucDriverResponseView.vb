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

Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports ZedGraph

#End Region ' Imports

''' <summary>
''' User control that allows users to align a response function to environmental 
''' data. This control contains a graph showing a histogram for selected environmental 
''' driver data, onto which the driver function is placed. Controls are available to
''' customize the lower and upper limit of the response function, and to set the mean
''' of the response function. These controls are enabled only for specific 
''' <see cref="IShapeFunction">shape functions</see>.
''' </summary>
Public Class ucDriverResponseView
    Implements IUIElement
    Implements IDisposable

#Region " Private vars "

    Private m_shape As EwECore.cEnviroResponseFunction = Nothing
    Private m_shapefunction As IShapeFunction = Nothing

    Private m_zgh As cZedGraphMediationHelper = Nothing
    Private m_driver As IEnviroInputData = Nothing
    Private m_fpMin As cEwEFormatProvider = Nothing
    Private m_fpMax As cEwEFormatProvider = Nothing
    Private m_fpMean As cEwEFormatProvider = Nothing

    Private m_bInUpdate As Boolean = False

#End Region ' Private vars

#Region " Construction / destruction "

    Public Sub New()

        Me.InitializeComponent()

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then

                Me.m_zgh.Detach()
                Me.m_zgh = Nothing

                Me.Shape = Nothing

                Me.m_fpMin.Release()
                Me.m_fpMax.Release()
                Me.m_fpMean.Release()

                components.Dispose()

            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region ' Construction / destruction

#Region " Public bits "

    Public Sub Init(uic As cUIContext)

        Me.UIContext = uic

        Me.m_zgh = New cZedGraphMediationHelper()
        Me.m_zgh.Attach(Me.UIContext, Me.m_graph)
        Me.m_zgh.ShowPointValue = True

        Me.m_zgh.ConfigurePane(My.Resources.RESPONSE_GRAPH_TITLE, My.Resources.RESPONSE_GRAPH_XLABEL, My.Resources.RESPONSE_GRAPH_YLABEL, True)

        'Yaxis (left) grid lines
        Me.m_zgh.GetPane(1).YAxis.MajorGrid.IsVisible = True

        ' JB: the cool thing to do here would be to only show the 1.0 grid line;not all the grid line....
        ' JS: This should help
        Me.m_zgh.GetPane(1).YAxis.MajorTic.IsAllTics = False

        Me.m_zgh.GetPane(1).Y2Axis.IsVisible = True

        Me.m_zgh.GetPane(1).Y2Axis.Title.Text = My.Resources.HEADER_DRIVER_HISTOGRAM
        Me.m_zgh.GetPane(1).Y2Axis.Title.IsVisible = True
        Me.m_zgh.GetPane(1).Y2Axis.Title.FontSpec = Me.m_zgh.GetPane(1).YAxis.Title.FontSpec

        Me.m_zgh.GetPane(1).Y2Axis.MinorTic.IsAllTics = False
        Me.m_zgh.GetPane(1).Y2Axis.MinorTic.IsOpposite = False
        Me.m_zgh.GetPane(1).Y2Axis.MajorTic.IsOpposite = False

        'somehow set the Y2Axis label font size
        Me.m_zgh.GetPane(1).Y2Axis.Scale.MaxAuto = True

        Me.m_fpMin = New cEwEFormatProvider(Me.UIContext, Me.m_tbxXMin, GetType(Single))
        Me.m_fpMax = New cEwEFormatProvider(Me.UIContext, Me.m_tbxXMax, GetType(Single))
        Me.m_fpMean = New cEwEFormatProvider(Me.UIContext, Me.m_tbxMean, GetType(Single))

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IUIElement.UIContext"/>
    ''' -----------------------------------------------------------------------
    Public Property UIContext As cUIContext _
        Implements IUIElement.UIContext

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="IEnviroInputData">driver</see> to display in the control.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Driver As IEnviroInputData
        Get
            Return Me.m_driver
        End Get
        Set(value As IEnviroInputData)
            Me.m_driver = value
            Me.UpdatePlot()
            Me.UpdateControls()
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether the shape controls should be show, such as the 'change shape' 
    ''' button and the controls to alter the shape mean.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ShowShapeControls As Boolean
        Get
            Return Me.m_btnChangeShape.Visible
        End Get
        Set(value As Boolean)
            Me.m_btnChangeShape.Visible = value
            Me.m_lblMean.Visible = value
            Me.m_tbxMean.Visible = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEnviroResponseFunction"/> to display in the control.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Shape As cEnviroResponseFunction
        Get
            Return Me.m_shape
        End Get
        Set(value As cEnviroResponseFunction)
            Me.m_shape = value
            Me.m_shapefunction = cShapeFunctionFactory.GetShapeFunction(value)
            Me.InitToShapeType()
            Me.UpdatePlot()
            Me.UpdateControls()
        End Set
    End Property

#End Region ' Public bits

#Region " Control Event Handlers "

    Private Sub OnMinMaxValueChanged(ByVal sender As Object, args As EventArgs)
        Me.ApplyMinMax()
    End Sub

    Private Sub OnSetDefaultMinMax(ByVal sender As Object, ByVal e As EventArgs) _
        Handles m_btnDefaultMinMax.Click
        Me.SetDefaultMinMax()
    End Sub

    Private Sub OnMeanValueChanged(sender As System.Object, e As System.EventArgs)
        Try
            If Me.m_bInUpdate Then Return

            Debug.Assert(Me.CanEditMean(), "Oppss BUG! should not be setting the Mean for this type of shape.")
            'Mean is stored in the Steep variable

            Dim normdist As cNormalShapeFunction = DirectCast(Me.m_shapefunction, cNormalShapeFunction)
            normdist.Mean = CSng(Me.m_fpMean.Value)
            normdist.Apply(Me.m_shape)

            Me.UpdatePlot()
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnChangeShape(sender As System.Object, e As System.EventArgs) _
        Handles m_btnChangeShape.Click
        Try
            Me.ChangeFFShape()
            ' Type of shape may have changed
            Me.InitToShapeType()
            Me.UpdateControls()
            Me.UpdatePlot()

        Catch ex As Exception

        End Try
    End Sub

#End Region ' Control Event Handlers

#Region " Internals "

    Protected Sub InitToShapeType()

        If (Me.m_zgh Is Nothing) Then Return

        If (Me.ShowMinMax) Then
            RemoveHandler Me.m_fpMin.OnValueChanged, AddressOf OnMinMaxValueChanged
            RemoveHandler Me.m_fpMax.OnValueChanged, AddressOf OnMinMaxValueChanged
        End If

        If (Me.CanEditMean) Then
            RemoveHandler Me.m_fpMean.OnValueChanged, AddressOf OnMeanValueChanged
        End If

        If (Me.m_shape Is Nothing) Then Return

        If (Me.ShowMinMax) Then

            ' Set min and max
            Me.m_fpMin.Value = Me.m_shape.ResponseLeftLimit
            Me.m_fpMax.Value = Me.m_shape.ResponseRightLimit

            AddHandler Me.m_fpMin.OnValueChanged, AddressOf OnMinMaxValueChanged
            AddHandler Me.m_fpMax.OnValueChanged, AddressOf OnMinMaxValueChanged

        End If

        If (Me.CanEditMean) Then

            Dim normdist As cNormalShapeFunction = DirectCast(Me.m_shapefunction, cNormalShapeFunction)
            Me.m_fpMean.Value = normdist.Mean

            AddHandler Me.m_fpMean.OnValueChanged, AddressOf OnMeanValueChanged
        End If

    End Sub

    Private Sub UpdatePlot()

        If (Me.m_zgh Is Nothing) Then Return

        Try
            Me.m_zgh.GetPane(1).CurveList.Clear()

            Me.PlotShape()
            Me.PlotDriver()

        Catch ex As Exception

        End Try

    End Sub

    Private Sub UpdateControls()

        If (Me.m_zgh Is Nothing) Then Return

        Dim bHasShape As Boolean = (Me.Shape IsNot Nothing)
        Dim bCanSetMinMax As Boolean = bHasShape And Me.CanEditMinMax()
        Dim bCanSetMeanSD As Boolean = bHasShape And Me.CanEditMean()

        Dim strXMin As String = My.Resources.HEADER_X_MIN
        Dim strXMax As String = My.Resources.HEADER_X_MAX
        Dim strMean As String = My.Resources.HEADER_MEAN

        If (bHasShape) Then

            Select Case Me.Shape.ShapeFunctionType

                Case eShapeFunctionType.Normal
                    strXMin = My.Resources.HEADER_PLOT_MIN
                    strXMax = My.Resources.HEADER_PLOT_MAX

                Case eShapeFunctionType.LeftShoulder,
                     eShapeFunctionType.RightShoulder,
                     eShapeFunctionType.Trapezoid
                    strXMin = My.Resources.HEADER_PLOT_MIN
                    strXMax = My.Resources.HEADER_PLOT_MAX

                Case Else
                    ' NOP
            End Select
        End If

        Me.m_lblXMin.Text = cStyleGuide.ToControlLabel(strXMin)
        Me.m_lblXMax.Text = cStyleGuide.ToControlLabel(strXMax)
        Me.m_fpMin.Enabled = bCanSetMinMax
        Me.m_fpMax.Enabled = bCanSetMinMax

        Me.m_lblMean.Text = cStyleGuide.ToControlLabel(strMean)
        Me.m_fpMean.Enabled = bCanSetMeanSD

        Me.m_btnChangeShape.Enabled = bHasShape

    End Sub

    Private Function ShowMinMax() As Boolean

        Return (Me.m_shape IsNot Nothing)

    End Function

    Private Function CanEditMinMax() As Boolean

        If (Me.m_shapefunction Is Nothing) Then Return True
        Return Not Me.m_shapefunction.IsDistribution()

    End Function

    Private Function CanEditMean() As Boolean

        If (Me.m_shapefunction Is Nothing) Then Return False
        Return (Me.m_shape.ShapeFunctionType = eShapeFunctionType.Normal)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Launch EwE 'change shape' interface.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub ChangeFFShape()
        Debug.Assert(Me.m_shape IsNot Nothing)
        Try
            Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("ChangeEcosimShape")
            cmd.Tag = Me.m_shape
            cmd.Invoke()
            cmd.Tag = Nothing
        Catch ex As Exception

        End Try
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' ToDo: document this
    ''' </summary>
    ''' <param name="sShapeMin"></param>
    ''' <param name="sShapeMax"></param>
    ''' <param name="sPlotMin"></param>
    ''' <param name="sPlotMax"></param>
    ''' -----------------------------------------------------------------------
    Private Sub GetPlotMinMax(ByRef sShapeMin As Single, ByRef sShapeMax As Single,
                              ByRef sPlotMin As Single, ByRef sPlotMax As Single)

        Select Case Me.m_shape.ShapeFunctionType

            Case eShapeFunctionType.Normal
                'Normal distribution shape min and max are set from the Mean and SD values

                'Use the Min Max on the interface to set the plot window size
                sPlotMin = CSng(Me.m_fpMin.Value)
                sPlotMax = CSng(Me.m_fpMax.Value)

            Case eShapeFunctionType.LeftShoulder, eShapeFunctionType.RightShoulder, eShapeFunctionType.Trapezoid, eShapeFunctionType.Sigmoid
                'Shoulder shape min and max can not be set here
                'They only get set from the ChangeShape dialogue

                'Min and Max of the plot window NOT the data
                sPlotMin = CSng(Me.m_fpMin.Value)
                sPlotMax = CSng(Me.m_fpMax.Value)

                'The min and max of the data cannot be changed here
                sShapeMin = Me.m_shape.ResponseLeftLimit
                sShapeMax = Me.m_shape.ResponseRightLimit

            Case Else
                'For all other shape the Min Max get set for the Min and Max textbox on this form
                sShapeMin = CSng(Me.m_tbxXMin.Text)
                sShapeMax = CSng(Me.m_tbxXMax.Text)
                sPlotMin = sShapeMin
                sPlotMax = sShapeMax

        End Select

    End Sub

    Private Sub PlotShape()

        If (Me.m_shape Is Nothing) Then Return

        Try
            ' Obtain Min and Max from the response function
            ' this is what the core will use to find the x value
            Dim XDataMin As Single = Me.m_shape.ResponseLeftLimit
            Dim XDataMax As Single = Me.m_shape.ResponseRightLimit

            Dim XWinMax As Single
            Dim XWinMin As Single

            Me.GetPlotMinMax(XDataMin, XDataMax, XWinMin, XWinMax)

            Dim Xrange As Single = XDataMax - XDataMin
            Dim fmt As New cCoreInterfaceFormatter()

            Dim dx As Single = Xrange / Me.m_shape.nPoints

            Dim YScale As Single = 1
            Dim lstPts As New PointPairList

            Dim x As Double
            For ipt As Integer = 1 To Me.m_shape.nPoints
                x = XDataMin + dx * (ipt - 1)
                If (ipt = 1) Then lstPts.Add(x, 0.0!)
                lstPts.Add(x, Me.m_shape.ShapeData(ipt) * YScale)
                If (ipt = Me.m_shape.nPoints) Then lstPts.Add(x, 0.0!)
            Next

            'add the last point out at the end of the graph
            lstPts.Add(XDataMax, Me.m_shape.ShapeData(Me.m_shape.nPoints) * YScale)

            Dim il As LineItem = Me.m_zgh.CreateLineItem(cStringUtils.Localize(My.Resources.HEADER_RESPONSE_TARGET, fmt.GetDescriptor(Me.m_shape)),
                                                         lstPts, cZedGraphMediationHelper.eEnvResponseLineType.Response)
            Me.m_zgh.GetPane(1).CurveList.Add(il)

            'X axis for plotting
            Me.m_zgh.XScaleMin = XWinMin
            Me.m_zgh.XScaleMax = XWinMax
            Me.m_zgh.YScaleMax = Me.m_shape.YMax + Me.m_shape.YMax * 0.1
            Me.m_zgh.YScaleMin = 0

        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

    Private Sub SetDefaultMinMax()

        If (Me.m_driver Is Nothing) Then Return

        Me.m_bInUpdate = True

        Me.m_fpMin.Value = Me.m_driver.Min
        Me.m_fpMax.Value = Me.m_driver.Max

        Me.m_bInUpdate = False
        Me.ApplyMinMax()

    End Sub

    Private Sub ApplyMinMax()
        If Me.m_bInUpdate Then Return

        Debug.Assert(Me.ShowMinMax())

        'Not all shapes use the Min and Mix data range
        If Me.CanEditMinMax() Then
            Try
                Me.m_shape.LockUpdates()
                Me.m_shape.ResponseLeftLimit = CSng(Me.m_fpMin.Value)
                Me.m_shape.ResponseRightLimit = CSng(Me.m_fpMax.Value)
                Me.m_shape.UnlockUpdates(True)
            Catch ex As Exception

            End Try
        End If ' If Me.CanEditMinMax() Then

        Me.UpdatePlot()

    End Sub

    Private Sub PlotDriver()
        Try
            If (Me.m_driver Is Nothing) Then Return

            Dim histPts() As Drawing.PointF = Me.m_driver.Histogram()
            Dim binWidth As Single = Me.m_driver.HistogramBinWidth
            Dim lstPts As New PointPairList()
            Dim fmt As New cCoreInterfaceFormatter()

            'The X value in the histogram is the max value of the bin, right hand side of the bin
            'So an input value of 1.0 will be in the .X = 1.0 bin
            For ipt As Integer = 1 To histPts.Length - 1
                lstPts.Add(histPts(ipt).X - binWidth, histPts(ipt).Y)
                lstPts.Add(histPts(ipt).X, histPts(ipt).Y)
            Next

            Dim li As LineItem = Me.m_zgh.CreateLineItem(cStringUtils.Localize(My.Resources.HEADER_HISTOGRAM_TARGET, Me.m_driver.Name),
                                                         lstPts, cZedGraphMediationHelper.eEnvResponseLineType.Histogram)

            li.IsY2Axis = True
            li.Line.Fill = New Fill(cColorUtils.GetVariant(li.Color, 0.5))
            Me.m_zgh.GetPane(1).CurveList.Add(li)

            'Let the response function decide the plot window size
            'Me.m_zgh.XScaleMax = Me.m_map.Max
            Me.m_zgh.YScaleMin = 0

        Catch ex As Exception
            Debug.Assert(False, "PlotMap " & ex.Message)
            cLog.Write(ex)
        End Try

    End Sub
#End Region ' Internals 

End Class
