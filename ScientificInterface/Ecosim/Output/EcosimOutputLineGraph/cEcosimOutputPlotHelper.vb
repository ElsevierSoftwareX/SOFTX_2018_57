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
Imports EwECore
Imports ZedGraph
Imports ScientificInterfaceShared.Style
Imports EwEUtils.Core

#End Region ' Imports

Namespace Controls

    <CLSCompliant(False)> _
    Public Class cEcosimOutputPlotHelper
        Inherits cZedGraphHelper

#Region " Private vars "

        Private m_graphPane As GraphPane = Nothing

        Private m_bShowMultipleRuns As Boolean = False
        Private m_lRuns As New List(Of cRun)
        Private m_runCurrent As cRun = Nothing
        Private m_curvelistTimeSeries As New CurveList()

#End Region ' Private vars

#Region " Private helper classes "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper class, holds the plotted results of an Ecosim run.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Class cRun

            Private m_strName As String = ""
            Private m_curvelist As CurveList = Nothing

            Public Sub New(ByVal strName As String)
                Me.m_strName = strName
                Me.m_curvelist = New CurveList()
            End Sub

            Public ReadOnly Property Name() As String
                Get
                    Return Me.m_strName
                End Get
            End Property

            Public ReadOnly Property Lines() As CurveList
                Get
                    Return Me.m_curvelist
                End Get
            End Property
        End Class

#End Region ' Private helper classes

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New()
        End Sub

#End Region

#Region " Public interfaces "

        Public Overrides Sub Attach(ByVal uic As cUIContext, _
                                    ByVal zgc As ZedGraph.ZedGraphControl, _
                                    Optional ByVal iNumPanes As Integer = 1)

            Debug.Assert(iNumPanes = 1)

            MyBase.Attach(uic, zgc, 1)

            Me.m_graphPane = Me.GetPane(1)
        End Sub

        Public Overrides Sub Detach()
            Me.Clear()
            MyBase.Detach()
        End Sub

#End Region

#Region " Public Properties "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' ..exterminate.. exterminate..
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Clear()
            Me.m_lRuns.Clear()
            Me.m_runCurrent = Nothing
            Me.m_graphPane.CurveList.Clear()
            Me.m_curvelistTimeSeries.Clear()
            Me.RescaleAndRedraw()
        End Sub

        Public Sub ClearTimeSeries()
            ' Remove all lines for the local run from the graph pane
            For Each li As LineItem In Me.m_curvelistTimeSeries
                Me.m_graphPane.CurveList.Remove(li)
            Next
            Me.m_curvelistTimeSeries.Clear()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Prepare a new run for display.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub CreateRun(ByVal strLabel As String)
            If (Me.m_bShowMultipleRuns = False) Then
                Me.Clear()
            End If
            Me.m_runCurrent = New cRun(strLabel)
            Me.m_lRuns.Add(Me.m_runCurrent)
        End Sub

        Public Sub ResetRun()
            If (Me.m_runCurrent Is Nothing) Then Return

            ' Remove all lines for the local run from the graph pane
            For Each li As LineItem In Me.m_runCurrent.Lines
                Me.m_graphPane.CurveList.Remove(li)
            Next
            ' Clear current run
            Me.m_runCurrent.Lines.Clear()
            ' Clear time series
            Me.ClearTimeSeries()
        End Sub

        ''' <summary>
        ''' Is the graph ready to plot? Has it been initialized?
        ''' </summary>
        ''' <returns>True if the graph is ready to plot.</returns>
        Public ReadOnly Property IsReady() As Boolean
            Get
                Return (Me.m_runCurrent IsNot Nothing)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a single line to the current run.
        ''' </summary>
        ''' <param name="strLabel">Label of the line to add.</param>
        ''' <param name="list">Data for the line.</param>
        ''' <remarks>
        ''' Note that this does NOT add the line to ZedGraph.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Function CreateLine(ByVal src As ICoreInterface, _
                                   ByVal list As PointPairList, _
                                   Optional ByVal strLabel As String = "") As LineItem

            Dim crv As LineItem = Me.CreateLineItem(src, list, strLabel)
            Select Case Me.CurveType(crv)

                Case eSketchDrawModeTypes.TimeSeriesDriver, eSketchDrawModeTypes.TimeSeriesRefAbs, eSketchDrawModeTypes.TimeSeriesRefRel
                    Me.m_curvelistTimeSeries.Add(crv)
                Case eSketchDrawModeTypes.Line
                    Me.m_runCurrent.Lines.Add(crv)
                Case Else
                    Debug.Assert(False)
            End Select

            Return crv

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the value from the plot for a given group and run at a given time step.
        ''' </summary>
        ''' <param name="iGroup"></param>
        ''' <param name="iRun"></param>
        ''' <param name="iTimeStep"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function GetValueAt(ByVal iGroup As Integer, _
                                   ByVal iRun As Integer, _
                                   ByVal iTimeStep As Integer) As Double

            Dim run As cRun = Nothing
            Dim line As CurveItem = Nothing
            Dim dValue As Double = 0.0#

            Try
                If iRun >= 0 And iRun < Me.m_lRuns.Count Then
                    run = Me.m_lRuns(iRun)
                    If iGroup > 0 And iGroup <= run.Lines.Count Then
                        line = run.Lines(iGroup - 1)
                        If (iTimeStep >= 0 And iTimeStep < line.NPts) Then
                            dValue = line(iTimeStep).Y
                        End If
                    End If
                End If
            Catch ex As Exception
                Debug.Assert(False)
            End Try

            Return dValue

        End Function

        Public Sub ClearHighlights()
            ' Clear all colors
            Me.SetAllToColors(False)
            Me.UpdateColours()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Highlight data in the graph by colouring highlighted data, and
        ''' greying out data that is not highlighted.
        ''' </summary>
        ''' <param name="iGroup">Group index to select</param>
        ''' <param name="iRun">The run to highlight.</param>
        ''' -------------------------------------------------------------------
        Public Sub Highlight(ByVal iGroup As Integer, _
                             ByVal iRun As Integer)

            Dim run As cRun = Nothing
            Dim crv As CurveItem = Nothing

            If iGroup <= 0 And iRun < 0 Then
                ' Set set all to normal color
                Me.SetAllToColors(True)

            ElseIf iGroup > 0 And iRun < 0 Then

                ' Set only group for all runs
                For iRunTest As Integer = 0 To m_lRuns.Count - 1

                    run = Me.m_lRuns.Item(iRunTest)
                    If (run.Lines.Count > iGroup) Then
                        crv = run.Lines(iGroup - 1)
                        Me.SetCurveAppearance(crv, True, Me.IsPaneCumulative = False)
                    End If

                Next iRunTest

            ElseIf iGroup <= 0 And iRun >= 0 Then

                ' Only single run to highlight
                run = Me.m_lRuns(iRun)

                For iGroup = run.Lines.Count - 1 To 0 Step -1
                    crv = run.Lines(iGroup)
                    Me.SetCurveAppearance(crv, True, False)
                Next

            ElseIf iGroup > 0 And iRun >= 0 Then

                ' Set only one line
                run = Me.m_lRuns(iRun)
                crv = run.Lines(iGroup - 1)
                Me.SetCurveAppearance(crv, True, True)

            End If

            If (iGroup > 0) Then
                For Each crv In Me.m_curvelistTimeSeries
                    Dim info As cCurveInfo = Me.CurveInfo(crv)
                    If (info IsNot Nothing) Then
                        Me.SetCurveAppearance(crv, (info.Index = iGroup), True)
                    End If
                Next
            End If

            Me.UpdateColours()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether multiple runs should be shown.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowMultipleRuns() As Boolean
            Get
                Return Me.m_bShowMultipleRuns
            End Get
            Set(ByVal bShowMultipleRuns As Boolean)
                ' Update flag
                Me.m_bShowMultipleRuns = bShowMultipleRuns
                ' Switched to single view?
                If (bShowMultipleRuns = False) Then
                    ' #Yes: clear 
                    Me.Clear()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the number of runs.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property NumRuns() As Integer
            Get
                Return Me.m_lRuns.Count
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the label of a run.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property RunLabel(ByVal iRun As Integer) As String
            Get
                If (iRun < 0 Or iRun >= Me.m_lRuns.Count) Then Return ""
                Return Me.m_lRuns(iRun).Name
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set the graph title.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property DataName() As String
            Set(ByVal value As String)
                Me.m_graphPane.Title.Text = value
                Me.m_graphPane.YAxis.Title.Text = value
            End Set
        End Property

#End Region ' Public Properties

#Region " Private Helpers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set all the colors either original or gray.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub SetAllToColors(Optional ByVal bUseOriginalColor As Boolean = True)

            Dim crv As CurveItem = Nothing

            ' Colour all run lines
            For Each run As cRun In Me.m_lRuns
                For iCurve As Integer = run.Lines.Count - 1 To 0 Step -1

                    crv = run.Lines(iCurve)
                    If bUseOriginalColor Then
                        Me.SetCurveAppearance(crv, True, False)
                    Else
                        Me.SetCurveAppearance(crv, False, False)
                    End If

                Next iCurve
            Next run

            ' Colour all time series lines
            For Each crv In Me.m_curvelistTimeSeries
                If bUseOriginalColor Then
                    Me.SetCurveAppearance(crv, True, False)
                Else
                    Me.SetCurveAppearance(crv, False, False)
                End If
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary> 
        ''' Change the appearance of a curve.
        ''' </summary>
        ''' <param name="crv">The curve to prettify.</param>
        ''' <param name="bUseColor">Flag stating whether the curve should be coloured.</param>
        ''' <param name="bUseHighlight">Flag stating whether curve should be higlighted.</param>
        ''' -------------------------------------------------------------------
        Private Sub SetCurveAppearance(ByVal crv As CurveItem, _
                                       Optional ByVal bUseColor As Boolean = True, _
                                       Optional ByVal bUseHighlight As Boolean = False)

            ' Safety first
            If Not TypeOf (crv) Is LineItem Then Return

            'Dim li As LineItem = DirectCast(crv, LineItem)
            Dim info As cCurveInfo = Me.CurveInfo(crv)

            Debug.Assert(info IsNot Nothing)

            info.IsHighlighted = bUseHighlight
            info.IsGrayedOut = (Not bUseColor)

        End Sub

#End Region ' Private Helpers

    End Class

End Namespace
