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
' Copyright 1991-2012 UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'
#Region " Imports "

Option Strict On

Imports ZedGraph
Imports EwECore
Imports System.Drawing.Drawing2D

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Exploratory kite diagram in a ZedGraph.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class cZedGraphBoxPlotHelper
        Inherits cZedGraphHelper

#Region " Private vars "

        Private m_lScaleCircles As New List(Of LineItem)

#End Region ' Private vars

#Region " Public interfaces "

        Public Shadows Function ConfigurePane(ByVal strTitle As String, _
                                              ByVal bShowLegend As Boolean, _
                                              Optional ByVal legendPos As ZedGraph.LegendPos = ZedGraph.LegendPos.TopCenter, _
                                              Optional ByVal iPane As Integer = 1) As ZedGraph.GraphPane

            Return Me.ConfigurePane(strTitle, "", "", bShowLegend, legendPos, iPane)

        End Function

        Public Shadows Function ConfigurePane(ByVal strTitle As String, _
                                              ByVal strXAxisLabel As String, _
                                              ByVal strYAxisLabel As String, _
                                              ByVal bShowLegend As Boolean, _
                                              Optional ByVal legendPos As ZedGraph.LegendPos = ZedGraph.LegendPos.TopCenter, _
                                              Optional ByVal iPane As Integer = 1) As ZedGraph.GraphPane

            Dim gp As GraphPane = MyBase.ConfigurePane(strTitle, strXAxisLabel, Nothing, strYAxisLabel, Nothing, bShowLegend, legendPos, iPane)

            ' Use secundary axis pair to render the kite center
            With gp
                .X2Axis.Cross = 0
                .X2Axis.IsVisible = True
                .X2Axis.Scale.IsVisible = False
                .X2Axis.MinorTic.IsAllTics = False
                .X2Axis.MinorTic.IsOpposite = False
                .X2Axis.MajorTic.IsOpposite = False

                .Y2Axis.Cross = 0
                .Y2Axis.IsVisible = True
                .Y2Axis.Scale.IsVisible = False
                .Y2Axis.MinorTic.IsAllTics = False
                .Y2Axis.MinorTic.IsOpposite = False
                .Y2Axis.MajorTic.IsOpposite = False
            End With

            Me.SetScaleCircles(iPane)

            Return gp

        End Function

  
        Public Shadows Function CreateLineItem(ByVal iGroup As Integer, _
                                               ByVal asValues() As Single) As LineItem

            Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)
            Return Me.CreateLineItem(group.Name, Me.StyleGuide.GroupColor(Me.Core, group.Index), asValues)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strName"></param>
        ''' <param name="clr"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shadows Function CreateLineItem(ByVal strName As String, _
                                               ByVal clr As Color, _
                                               ByVal asValues() As Single) As LineItem

            Dim rpl As New RadarPointList()
            For i As Integer = 0 To asValues.Length - 1
                rpl.Add(asValues(i), 1.0#)
            Next
            Return New LineItem(strName, rpl, clr, SymbolType.Circle)

        End Function

      Private Sub BoxPlot(data As List(Of Double()), names As List(Of String))
            Dim myPane As GraphPane = Me.GetPane(1)

            For i As Integer = 0 To data.Count - 1
                'median of each array
                Dim medians As New PointPairList()
                '75th and 25th percentile, defines the box
                Dim hiLowList As New PointPairList()
                '+/- 1.5*Interquartile range, extentent of wiskers
                Dim barList As New PointPairList()
                'outliers
                Dim outs As New PointPairList()
                'Add the values
                medians.Add(i, percentile(data(i), 50))
                hiLowList.Add(i, percentile(data(i), 75), percentile(data(i), 25))
                Dim iqr As Double = 1.5 * (percentile(data(i), 75) - percentile(data(i), 25))
                Dim upperLimit As Double = percentile(data(i), 75) + iqr
                Dim lowerLimit As Double = percentile(data(i), 25) - iqr
                'The wiskers must end on an actual data point
                barList.Add(i, ValueNearestButGreater(data(i), lowerLimit), ValueNearestButLess(data(i), upperLimit))
                'Sort out the outliers
                For Each aValue As Double In data(i)
                    If aValue > upperLimit Then
                        outs.Add(i, aValue)
                    End If
                    If aValue < lowerLimit Then
                        outs.Add(i, aValue)
                    End If
                Next
                'Plot the items, first the median values
                Dim meadian As CurveItem = myPane.AddCurve("", medians, Color.Black, SymbolType.HDash)
                Dim myLine As LineItem = DirectCast(meadian, LineItem)
                myLine.Line.IsVisible = False
                myLine.Symbol.Fill.Type = FillType.Solid
                'Box
                Dim myCurve As HiLowBarItem = myPane.AddHiLowBar(names(i), hiLowList, theColours(i))
                myCurve.Bar.Fill.Type = FillType.Solid
                'Wiskers
                Dim myerror As ErrorBarItem = myPane.AddErrorBar("", barList, theColours(i))
                'Outliers
                Dim upper As CurveItem = myPane.AddCurve("", outs, theColours(i), SymbolType.Circle)
                Dim bLine As LineItem = DirectCast(upper, LineItem)
                bLine.Symbol.Size = 3
                bLine.Line.IsVisible = False
            Next
        End Sub

#End Region ' Public interfaces

#Region " Internals "

        Private Function ValueNearestButLess(data As Double(), number As Double) As Double
            Dim lowNums As Double() = From n In data Where n <= numbern
            Return lowNums.Max()
        End Function

        Private Function ValueNearestButGreater(data As Double(), number As Double) As Double
            Dim lowNums = From n In data Where n >= numbern
            Return lowNums.Min()
        End Function

        Private Function Percentile(data As Double(), iPerc As Integer) As Double
            Array.Sort(data)
            Dim numberOfValues As Integer = data.Length
            Dim i As Double = 0.5 + ((numberOfValues * (iPerc * 1.0)) / 100)
            Dim whole As Integer = CInt(Math.Floor(i))
            Dim frac As Double = i - whole
            If frac = 0 Then
                Return data(whole - 1)
            Else
                Return data(whole - 1) * (1 - frac) + frac * data(whole)
            End If
        End Function

#End Region ' Internals

    End Class

End Namespace
