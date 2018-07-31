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
Imports ScientificInterfaceShared.Controls
Imports ZedGraph
Imports System.Text
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ScientificInterfaceShared.Definitions

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class to to update the graph that reflects Ecospace biodiversity indicators.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cMCHistogramGraphWrapper
    Inherits cZedGraphHelper

#Region " Private variables "

    ''' <summary>Indicator grouping etc as centrally defined in the plug-in.</summary>
    Private m_settings As cIndicatorSettings = Nothing
    ''' <summary>List of Ecopath indicators to show histograms for.</summary>
    Private m_lind As List(Of cEcopathIndicators) = Nothing

    ''' <summary>Current indicator group to display in the graph.</summary>
    Private m_groupCurrent As cIndicatorInfoGroup = Nothing
    ''' <summary>Current indicator to display in the graph.</summary>
    Private m_indCurrent As cIndicatorInfo = Nothing

#End Region ' Private variables

#Region " Attach + detach "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Attach this class to a zedgraph control.
    ''' </summary>
    ''' <param name="uic"><see cref="cUIContext"/> providing UI contextual information.</param>
    ''' <param name="zgc"><see cref="ZedGraphControl"/> to style and interact with.</param>
    ''' <param name="settings"><see cref="cIndicatorSettings"/> defined centrally in the plug-in.</param>
    ''' -------------------------------------------------------------------
    Public Shadows Sub Attach(ByVal uic As ScientificInterfaceShared.Controls.cUIContext, _
                                ByVal zgc As ZedGraph.ZedGraphControl, _
                                ByVal settings As cIndicatorSettings, _
                                ByVal lind As List(Of cEcopathIndicators))
        MyBase.Attach(uic, zgc, 1)
        ' Store important bits
        Me.m_settings = settings

        Me.m_lind = lind

        Me.ShowPointValue = True
    End Sub

    ''' -------------------------------------------------------------------
    ''' <inheritdocs cref="cZedGraphHelper.Detach"/>
    ''' -------------------------------------------------------------------
    Public Overrides Sub Detach()
        Me.m_settings = Nothing
        MyBase.Detach()
    End Sub

#End Region ' Attach + detach

#Region " Refreshing "

    Public Sub RefreshContent(indSingle As cIndicatorInfo, indGroup As cIndicatorInfoGroup)

        Dim lInfo As New List(Of cIndicatorInfo)
        Dim info As cIndicatorInfo = Nothing
        Dim gp As GraphPane = Nothing
        Dim strLabelPane As String = ""
        Dim strLabelValue As String = ""
        Dim settings As cIndicatorSettings = Me.m_settings
        Dim ind As cEcosimIndicators = Nothing
        Dim ppl As PointPairList = Nothing
        Dim sValue As Single = 0
        Dim sXMin As Single = 0
        Dim sXMax As Single = 0

        If (indSingle Is Nothing) Then
            ' Group mode
            If Not ReferenceEquals(indGroup, Me.m_groupCurrent) Then
                For i As Integer = 0 To indGroup.NumIndicators - 1
                    lInfo.Add(indGroup.Indicator(i))
                Next
            End If
            strLabelPane = indGroup.Name
        Else
            ' Indicator mode
            If Not ReferenceEquals(indSingle, Me.m_indCurrent) Then
                lInfo.Add(indSingle)
            End If
            strLabelPane = indSingle.Name
        End If

        ' Set master pane title
        Me.Configure(strLabelPane)

        If (lInfo.Count > 0) Then
            ' Create and configure panes
            Me.NumPanes = lInfo.Count
            For iPane As Integer = 1 To Me.NumPanes
                info = lInfo(iPane - 1)
                gp = Me.GetPane(iPane)
                gp.Tag = info
                If String.IsNullOrWhiteSpace(info.Units) Then
                    strLabelValue = info.ValueDescription
                Else
                    strLabelValue = String.Format(SharedResources.GENERIC_LABEL_DETAILED, info.ValueDescription, info.Units)
                End If
                Me.ConfigurePane(info.Name, strLabelValue, My.Resources.HEADER_OCCURRENCE, False, iPane:=iPane)
            Next
        End If

        Try
            ' Next populate all panels
            For iPane As Integer = 1 To Me.NumPanes
                ' Get pane for indicator iInd
                gp = Me.GetPane(iPane)
                ' Prepare for determining axis range
                sXMin = Single.MaxValue : sXMax = Single.MinValue
                ' Prepare structures for creating point list for indicator
                info = DirectCast(gp.Tag, cIndicatorInfo)

                ppl = New PointPairList()

                Dim sBinWidth As Single = 1
                Dim hist() As Drawing.PointF = Me.Histogram(info, sBinWidth)

                'The X value in the histogram is the max value of the bin, right hand side of the bin
                'So an input value of 1.0 will be in the .X = 1.0 bin
                For ipt As Integer = 1 To hist.Length - 1
                    ppl.Add(hist(ipt).X - sBinWidth, hist(ipt).Y)
                    ppl.Add(hist(ipt).X, hist(ipt).Y)

                    sXMin = Math.Min(sXMin, hist(ipt).X - sBinWidth)
                    sXMax = Math.Max(sXMax, hist(ipt).X)
                Next

                Dim il As LineItem = Me.CreateLineItem(info.Name, eSketchDrawModeTypes.NotSet, Drawing.Color.RoyalBlue, ppl)
                Me.PlotLines(New LineItem() {il}, iPane)

                gp.XAxis.Scale.MinAuto = False
                gp.XAxis.Scale.MinGrace = 0
                gp.XAxis.Scale.Min = sXMin
                gp.XAxis.Scale.MaxAuto = False
                gp.XAxis.Scale.MaxGrace = 0
                gp.XAxis.Scale.Max = sXMax
                gp.AxisChange()

            Next iPane

        Catch ex As Exception
            ' Ouch
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

#End Region ' Refreshing

    Private Function Histogram(info As cIndicatorInfo, ByRef sBinWidth As Single) As Drawing.PointF()

        Dim nBins As Integer = 100
        Dim pts(nBins) As Drawing.PointF

        Dim nValues As Integer = Math.Max(1, Me.m_lind.Count)

        Dim sMin As Single = Single.MaxValue
        Dim sMax As Single = Single.MinValue

        If (Me.m_lind.Count > 0) Then
            For Each ind As cEcopathIndicators In Me.m_lind
                Dim sVal As Single = info.GetValue(ind)
                sMin = Math.Min(sVal, sMin)
                sMax = Math.Max(sVal, sMax)
            Next
        Else
            sMin = 0
            sMax = 100
        End If

        Dim sRange As Single = sMax - sMin
        If (sRange > 0) Then
            sBinWidth = sRange / nBins
        Else
            'No data in the map so just set a default binwidth 
            'this will dump all the data into the zero bin
            sBinWidth = 1.0F / CSng(nBins)
        End If

        For Each ind As cEcopathIndicators In Me.m_lind
            Dim sVal As Single = info.GetValue(ind)
            Dim ipt As Integer = CInt(Math.Truncate((sVal - sMin) / sBinWidth)) + 1
            ipt = Math.Max(1, Math.Min(nBins, ipt))
            pts(ipt).Y += 1
        Next

        For i As Integer = 1 To nBins
            pts(i).X = CSng(sMin + sBinWidth * i)
            'pts(i).Y = pts(i).Y / nValues
        Next
        Return pts

    End Function

#Region " Tooltip "

    Protected Overrides Function FormatTooltip(pane As ZedGraph.GraphPane, curve As ZedGraph.CurveItem, iPoint As Integer) As String

        Dim ind As cIndicatorInfo = DirectCast(pane.Tag, cIndicatorInfo)
        Dim sb As New StringBuilder()

        ' Tooltip should show the indicator description, if available, instead of repeating the pane title
        If Not String.IsNullOrEmpty(ind.Description) Then
            sb.Append(ind.Description)
        Else
            sb.Append(curve.Label.Text)
        End If

        Dim strValueBit As String = Me.FormatTooltipValue(pane, curve, iPoint)
        If Not String.IsNullOrEmpty(strValueBit) Then
            If sb.Length > 0 Then sb.AppendLine("")
            sb.Append(strValueBit)
        End If
        Return sb.ToString

    End Function

#End Region ' Tooltip

End Class
