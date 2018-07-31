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
    Public Class cZedGraphKiteHelper
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

            Dim gp As GraphPane = MyBase.ConfigurePane(strTitle, strXAxisLabel, strYAxisLabel, bShowLegend, legendPos, iPane)

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

        Public Overridable Sub ClearScaleCircles(Optional ByVal iPane As Integer = -1)

            ' Render the simulated polar decorations:
            Dim gp As GraphPane = Me.GetPane(iPane)
            For Each cu As CurveItem In Me.m_lScaleCircles
                gp.CurveList.Remove(cu)
            Next
            Me.m_lScaleCircles.Clear()

        End Sub

        Public Overridable Sub SetScaleCircles(Optional ByVal iPane As Integer = -1)

            ' Render the simulated polar decorations:
            ' Use secundary axis pair to render the kite center
            Dim gp As GraphPane = Me.GetPane(iPane)

            ' Calc absolute max
            gp.XAxis.Scale.MaxAuto = True
            gp.XAxis.Scale.MinAuto = True
            gp.YAxis.Scale.MaxAuto = True
            gp.YAxis.Scale.MinAuto = True
            gp.AxisChange()

            Dim dMaxX As Double = Math.Max(gp.XAxis.Scale.Max, gp.XAxis.Scale.Min)
            Dim dMaxY As Double = Math.Max(gp.YAxis.Scale.Max, gp.YAxis.Scale.Min)

            If dMaxX = 0.0# Then dMaxX = 1.0#
            If dMaxY = 0.0# Then dMaxY = 1.0#

            gp.XAxis.Scale.Max = Math.Max(dMaxX, dMaxY)
            gp.X2Axis.Scale.Max = Math.Max(dMaxX, dMaxY)
            gp.XAxis.Scale.Min = -Math.Max(dMaxX, dMaxY)
            gp.X2Axis.Scale.Min = -Math.Max(dMaxX, dMaxY)
            gp.YAxis.Scale.Max = Math.Max(dMaxX, dMaxY)
            gp.Y2Axis.Scale.Max = Math.Max(dMaxX, dMaxY)
            gp.YAxis.Scale.Min = -Math.Max(dMaxX, dMaxY)
            gp.Y2Axis.Scale.Min = -Math.Max(dMaxX, dMaxY)

            Dim dTickSize As Double = gp.XAxis.Scale.MajorStep
            Dim iNumScaleCircles As Integer = CInt(Math.Floor(gp.XAxis.Scale.Max / dTickSize))
            Dim rpl As RadarPointList = Nothing
            Dim circle As LineItem = Nothing

            If Me.m_lScaleCircles.Count > 0 Then
                Me.ClearScaleCircles(iPane)
            End If

            For j As Integer = 1 To iNumScaleCircles

                rpl = New RadarPointList()
                For i As Integer = 0 To 20 : rpl.Add(j * dTickSize, 1) : Next i

                circle = New LineItem("", rpl, Color.Gray, SymbolType.None)
                circle.Line.IsSmooth = True
                circle.Line.SmoothTension = 0.6F
                circle.Line.Style = DashStyle.Custom
                circle.Line.DashOff = 4
                circle.Line.DashOn = 1

                Me.m_lScaleCircles.Add(circle)
                gp.CurveList.Insert(0, circle)

            Next

        End Sub

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

        Public Shadows Sub PlotLines(ByVal lines() As ZedGraph.LineItem, Optional ByVal iPane As Integer = 1)
            Me.ClearScaleCircles(iPane)
            MyBase.PlotLines(lines, iPane, True, True, True)
            Me.SetScaleCircles(iPane)
        End Sub

        Public Overrides Sub RescaleAndRedraw(Optional ByVal iPane As Integer = -1)
            MyBase.RescaleAndRedraw(iPane)
        End Sub

#End Region ' Public interfaces

#Region " Internals "

        ''' <summary>
        ''' Overridden to prevent summing of radial items.
        ''' </summary>
        ''' <param name="liOffset"></param>
        ''' <param name="lTarget"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub SumLines(ByVal liOffset As ZedGraph.LineItem, ByVal lTarget As ZedGraph.LineItem)

        End Sub

#End Region ' Internals

    End Class

End Namespace
