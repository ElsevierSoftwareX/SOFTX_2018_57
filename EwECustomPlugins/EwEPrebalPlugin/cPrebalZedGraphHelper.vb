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

Imports System.Drawing
Imports System.Windows.Forms
Imports EwECore
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports ZedGraph
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

''' ---------------------------------------------------------------------------
''' <summary>
''' Controller for the Pre-bal comparison graphs.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cPrebalZedGraphHelper
    Inherits cZedGraphHelper

#Region " Internals "

    ''' <summary><see cref="cPrebalModel"/> instance to work with.</summary>
    Private m_model As cPrebalModel = Nothing

    Private m_itemShowHideTL As ToolStripButton = Nothing
    Private m_itemShowHideName As ToolStripButton = Nothing
    Private m_itemShowHideFormula As ToolStripButton = Nothing

#End Region ' Internals

#Region " Construction "

    Public Sub New()
        ' NOP
    End Sub

#End Region ' Construction

#Region " Overrides "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Attach the handler to the graph.
    ''' </summary>
    ''' <param name="uic"><see cref="cUIContext"/>.</param>
    ''' <param name="zgc"><see cref="ZedGraphControl"/>.</param>
    ''' <param name="model"><see cref="cPrebalModel"/>.</param>
    ''' <param name="strTitle">The title of the graph.</param>
    ''' -----------------------------------------------------------------------
    Public Shadows Sub Attach(uic As cUIContext, zgc As ZedGraph.ZedGraphControl, model As cPrebalModel, strTitle As String)

        MyBase.Attach(uic, zgc, 4)

        ' Store ref
        Me.m_model = model

        Dim vnf As New cVarnameTypeFormatter()

        ' Set the panels
        Me.Configure(strTitle)
        Me.ConfigurePane(SharedResources.HEADER_BIOMASS, My.Resources.LABEL_XAXIS, SharedResources.HEADER_BIOMASS, False, iPane:=1)
        Me.ConfigurePane(SharedResources.HEADER_PRODUCTION_OVER_BIOMASS, My.Resources.LABEL_XAXIS, SharedResources.HEADER_PRODUCTION_OVER_BIOMASS, False, iPane:=2)
        Me.ConfigurePane(SharedResources.HEADER_CONSUMPTION_OVER_BIOMASS, My.Resources.LABEL_XAXIS, SharedResources.HEADER_CONSUMPTION_OVER_BIOMASS, False, iPane:=3)
        Me.ConfigurePane(SharedResources.HEADER_PRODCONS, My.Resources.LABEL_XAXIS, SharedResources.HEADER_PRODCONS, False, iPane:=4)

        ' Data change callback
        AddHandler Me.m_model.OnUpdated, AddressOf OnUpdated

        ' X-axis label formatting handlers
        For i As Integer = 1 To NumPanes
            AddHandler Me.GetPane(i).XAxis.ScaleFormatEvent, AddressOf OnFormatXAxisLabel
        Next

        Me.ShowHoverMenu = True

        Me.m_itemShowHideTL = Me.HoverMenu.AddItem(vnf.GetDescriptor(eVarNameFlags.TL, eDescriptorTypes.Abbreviation), My.Resources.OPTION_SHOWTL, Nothing, AddressOf OnShowHideTrophicLevels)
        Me.m_itemShowHideName = Me.HoverMenu.AddItem(vnf.GetDescriptor(eVarNameFlags.Name, eDescriptorTypes.Abbreviation), My.Resources.OPTION_SHOWNAME, Nothing, AddressOf OnShowHideNames)
        Me.m_itemShowHideFormula = Me.HoverMenu.AddItem(SharedResources.FormulaEvaluatorHS, My.Resources.OPTION_SHOWREGFORMULA, Nothing, AddressOf OnShowHideFormula)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Clean-up.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Shadows Sub Detach()

        For i As Integer = 1 To Me.NumPanes
            RemoveHandler Me.GetPane(i).XAxis.ScaleFormatEvent, AddressOf OnFormatXAxisLabel
        Next

        MyBase.Detach()
        RemoveHandler Me.m_model.OnUpdated, AddressOf OnUpdated
        Me.m_model = Nothing

    End Sub

#End Region ' Overrides

#Region " Public bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether to show trophic levels in the axis labels.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ShowTL As Boolean = False

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether to show group names in the axis labels.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ShowName As Boolean = False

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether to show the regression formula in the regression label.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ShowRegressionFormula As Boolean = False

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Repopulate the graphs.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Refresh()

        If (Me.UIContext Is Nothing) Then Return

        Dim core As cCore = Me.UIContext.Core
        Dim sg As cStyleGuide = Me.UIContext.StyleGuide
        Dim grp As cEcoPathGroupOutput = Nothing

        ' Update all panes
        For iPane As Integer = 1 To Me.NumPanes

            Dim p As GraphPane = Me.GetPane(iPane)

            Dim bi As BarItem = Nothing
            Dim li As LineItem = Nothing
            Dim clr As Color = Nothing
            Dim sMin As Single = 1
            Dim sMax As Single = 1
            Dim j As Integer = 1
            Dim bIsComputed As Boolean = False
            Dim bShowRegression As Boolean = True
            Dim pd As cPrebalPlotData = Me.GetDataForPlot(iPane)

            Debug.Assert(pd IsNot Nothing)

            Dim pptlEntered As New PointPairList()
            Dim pptlComputed As New PointPairList()
            Dim pplRegression As New PointPairList()

            p.Tag = iPane

            For i As Integer = 1 To pd.nGroups

                grp = core.EcoPathGroupOutputs(pd.EcopathGroupIndexes(i))
                bIsComputed = ((pd.Status(i) And eStatusFlags.ValueComputed) = eStatusFlags.ValueComputed)

                If (sg.GroupVisible(grp.Index)) Then
                    If (bIsComputed) Then
                        pptlComputed.Add(New PointPair(j, pd.Data(i)))
                    Else
                        pptlEntered.Add(New PointPair(j, pd.Data(i)))
                    End If
                    j += 1
                    If (pd.Data(i) > 0) Then
                        sMax = Math.Max(sMax, pd.Data(i))
                        sMin = Math.Min(sMin, pd.Data(i))
                    End If
                End If

                pplRegression.Add(New PointPair(j, pd.Data(i)))

            Next i

            ' Define Y-axis scale
            Select Case pd.Result
                Case cPrebalModel.eResultTypes.B, _
                     cPrebalModel.eResultTypes.PB, _
                     cPrebalModel.eResultTypes.QB
                    p.YAxis.Type = AxisType.Linear
                    bShowRegression = True
                Case cPrebalModel.eResultTypes.PQ
                    p.YAxis.Type = AxisType.Linear
                    bShowRegression = False
                Case Else
                    Debug.Assert(False, "Unknown pane indicated")
            End Select

            p.YAxis.Scale.MinAuto = False
            p.YAxis.Scale.Min = sMin * 0.8!

            ' Scale X axis
            p.XAxis.Scale.Min = 0.5
            p.XAxis.Scale.Max = pd.nGroups + 0.5
            p.XAxis.Scale.MajorStep = 1.0#
            p.XAxis.MajorTic.IsInside = False

            ' Flip X-axis labels if displaying any additional data
            If (Me.ShowTL Or Me.ShowName) Then
                p.XAxis.Scale.FontSpec.Angle = 90
            Else
                p.XAxis.Scale.FontSpec.Angle = 0
            End If

            ' Format graph area
            p.BarSettings.Type = BarType.Overlay
            p.CurveList.Clear()

            If (bShowRegression) Then

                ' Calculate regression line
                Dim iSampleSize As Integer
                Dim sSlope, sSlopeStdErr, sIntercept, sInterceptStdErr, sCorrelation As Single
                Dim strLabel As String = ""

                FindRegression(pplRegression, sSlope, sSlopeStdErr, sIntercept, sInterceptStdErr, sCorrelation, sMin, sMax, iSampleSize)

                If Me.ShowRegressionFormula Then
                    strLabel = cStringUtils.Localize(My.Resources.LABEL_REGRESSION_FORMULA,
                                                     sg.FormatNumber(sSlope),
                                                     If(sIntercept < 0, "-", "+"), sg.FormatNumber(Math.Abs(sIntercept)))
                Else
                    strLabel = My.Resources.LABEL_REGRESSION
                End If

                ' Regression
                pplRegression.Clear()
                For i As Integer = 1 To pd.nGroups
                    pplRegression.Add(i, sIntercept + sSlope * (i - 1))
                Next

                li = p.AddCurve(strLabel, pplRegression, Color.Black)
                li.Symbol.Type = SymbolType.None

            End If

            ' Entered data
            bi = p.AddBar(My.Resources.LABEL_ENTERED, pptlEntered, Color.White)
            clr = sg.ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_TEXT)
            bi.Bar.Border.Color = clr
            bi.Bar.Fill.Type = FillType.Solid
            bi.Bar.Fill.Color = cColorUtils.GetVariant(clr, 0.5)

            ' Computed data
            bi = p.AddBar(My.Resources.LABEL_ESTIMATED, pptlComputed, Color.White)
            clr = sg.ApplicationColor(cStyleGuide.eApplicationColorType.COMPUTED_TEXT)
            bi.Bar.Border.Color = clr
            bi.Bar.Fill.Type = FillType.None ' cColorUtils.GetVariant(clr, 0.5)

        Next iPane

        ' Total visual refresh
        Me.RescaleAndRedraw()

    End Sub

#End Region ' Public bits

#Region " Events "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Data update callback.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="args"></param>
    ''' -----------------------------------------------------------------------
    Private Sub OnUpdated(sender As Object, args As EventArgs)
        Try
            Me.Refresh()
        Catch ex As Exception

        End Try
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' X-axis label format event handler.
    ''' </summary>
    ''' <param name="pane"></param>
    ''' <param name="axis"></param>
    ''' <param name="val"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function OnFormatXAxisLabel(pane As GraphPane, axis As Axis, val As Double, index As Integer) As String

        Dim core As cCore = Me.UIContext.Core
        Dim grp As cEcoPathGroupOutput = Nothing
        Dim strLabel As String = ""
        Dim iPane As Integer = CInt(pane.Tag)
        Dim pd As cPrebalPlotData = Me.GetDataForPlot(iPane)
        Dim i As Integer = CInt(val)

        If (1 <= i And i <= pd.nGroups) Then
            grp = core.EcoPathGroupOutputs(pd.EcopathGroupIndexes(i))

            If Me.ShowName Then
                Dim fmt As New cCoreInterfaceFormatter()
                strLabel = fmt.GetDescriptor(grp, eDescriptorTypes.Name)
            Else
                strLabel = CStr(grp.Index)
            End If

            If Me.ShowTL Then
                strLabel = String.Format(SharedResources.GENERIC_LABEL_DETAILED, strLabel, grp.TTLX)
            End If
        End If

        Return strLabel

    End Function

    Private Sub OnShowHideTrophicLevels(sender As Object, args As EventArgs)
        Try
            Me.ShowTL = Not Me.ShowTL
            Me.Refresh()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnShowHideNames(sender As Object, args As EventArgs)
        Try
            Me.ShowName = Not Me.ShowName
            Me.Refresh()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnShowHideFormula(sender As Object, args As EventArgs)
        Try
            Me.ShowRegressionFormula = Not Me.ShowRegressionFormula
            Me.Refresh()
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Events

#Region " Overrides "

    Protected Overrides Sub UpdateHoverMenuItems()

        MyBase.UpdateHoverMenuItems()

        Me.m_itemShowHideTL.Checked = Me.ShowTL
        Me.m_itemShowHideName.Checked = Me.ShowName
        Me.m_itemShowHideFormula.Checked = Me.ShowRegressionFormula

    End Sub

#End Region ' Overrides 

#Region " Private bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the <see cref="cPrebalPlotData"/> for a given <paramref name="iPane">graph pane</paramref>.
    ''' </summary>
    ''' <param name="iPane">The pane to obtain data for.</param>
    ''' <returns>A <see cref="cPrebalPlotData"/> instance, or nothing if there is no
    ''' data available for the indicated pane.</returns>
    ''' -----------------------------------------------------------------------
    Private Function GetDataForPlot(iPane As Integer) As cPrebalPlotData
        Select Case iPane
            Case 1
                Return Me.m_model.Data(cPrebalModel.eResultTypes.B)
            Case 2
                Return Me.m_model.Data(cPrebalModel.eResultTypes.PB)
            Case 3
                Return Me.m_model.Data(cPrebalModel.eResultTypes.QB)
            Case 4
                Return Me.m_model.Data(cPrebalModel.eResultTypes.PQ)
            Case Else
                ' Debug.Assert(False)
        End Select
        Return Nothing
    End Function

#End Region ' Private bits

End Class
