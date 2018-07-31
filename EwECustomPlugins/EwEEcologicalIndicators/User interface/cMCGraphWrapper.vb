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
Imports System.Drawing
Imports System.Text
Imports EwECore
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Definitions
Imports ZedGraph
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class to to update the graph that reflects MC sim biodiversity indicators.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cMCGraphWrapper
    Inherits cZedGraphHelper

#Region " Private variables "

    ''' <summary>Indicator grouping etc as centrally defined in the plug-in.</summary>
    Private m_settings As cIndicatorSettings = Nothing
    ''' <summary>List of Ecosim indicators.</summary>
    Private m_lind As List(Of List(Of cMCIndicators)) = Nothing

    ''' <summary>Current indicator group to display in the graph.</summary>
    Private m_groupCurrent As cIndicatorInfoGroup = Nothing
    ''' <summary>Current indicator to display in the graph.</summary>
    Private m_indCurrent As cIndicatorInfo = Nothing

    ''' <summary>The MC manager.</summary>
    Private m_man As cMonteCarloManager = Nothing

    Private m_iBest As Integer = -1

#End Region ' Private variables

#Region " Attach + detach "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Attach this class to a zedgraph control.
    ''' </summary>
    ''' <param name="uic"><see cref="cUIContext"/> providing UI contextual information.</param>
    ''' <param name="zgc"><see cref="ZedGraphControl"/> to style and interact with.</param>
    ''' <param name="settings"><see cref="cIndicatorSettings"/> defined centrally in the plug-in.</param>
    ''' <param name="indicators">List of (hopefully computed) MC indicators.</param>
    ''' -------------------------------------------------------------------
    Public Shadows Sub Attach(ByVal uic As ScientificInterfaceShared.Controls.cUIContext, _
                                ByVal zgc As ZedGraph.ZedGraphControl, _
                                ByVal settings As cIndicatorSettings, _
                                ByVal indicators As List(Of List(Of cMCIndicators)))
        MyBase.Attach(uic, zgc, 1)
        ' Store important bits
        Me.m_settings = settings
        Me.m_lind = indicators
        Me.m_man = uic.Core.EcosimMonteCarlo
        Me.ShowPointValue = True
    End Sub

    ''' -------------------------------------------------------------------
    ''' <inheritdocs cref="cZedGraphHelper.Detach"/>
    ''' -------------------------------------------------------------------
    Public Overrides Sub Detach()
        Me.m_lind = Nothing
        Me.m_settings = Nothing
        Me.m_man = Nothing
        MyBase.Detach()
    End Sub

#End Region ' Attach + detach

#Region " Refreshing "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Refresh the graph to show a <see cref="cIndicatorSettings.IndicatorGroup">group of indicators</see>.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Sub RefreshContent(indSingle As cIndicatorInfo, indGroup As cIndicatorInfoGroup)

        Dim lInfo As New List(Of cIndicatorInfo)
        Dim info As cIndicatorInfo = Nothing
        Dim gp As GraphPane = Nothing
        Dim fmt As New EwECore.Style.cUnits(Me.UIContext.Core)
        Dim strLabelTime As String = fmt.ToString("[time]")
        Dim strLabelValue As String = ""
        Dim settings As cIndicatorSettings = Me.m_settings
        Dim ind As cEcosimIndicators = Nothing
        Dim ppl As PointPairList = Nothing
        Dim sValue As Single = 0
        Dim sXMin As Single = 0
        Dim sXMax As Single = 0
        Dim clr As Color = Color.Blue

        If (indSingle Is Nothing) Then
            ' Group mode
            If Not ReferenceEquals(indGroup, Me.m_groupCurrent) Then
                For i As Integer = 0 To indGroup.NumIndicators - 1
                    lInfo.Add(indGroup.Indicator(i))
                Next
            End If
        Else
            ' Indicator mode
            If Not ReferenceEquals(indSingle, Me.m_indCurrent) Then
                lInfo.Add(indSingle)
            End If
        End If

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
                ' Make indicator panel pretty
                Me.ConfigurePane(info.Name, strLabelTime, strLabelValue, False, iPane:=iPane)
            Next
        End If

        ' Determine best
        If (Me.m_man.nTrialIterations <= 1) Or (Me.m_man.SS = Me.m_man.SSBestFit) Then
            Me.m_iBest = CInt(Me.m_man.nTrialIterations)
        End If

        ' Next populate all panels
        For iPane As Integer = 1 To Me.NumPanes
            ' Get pane for indicator iInd
            gp = Me.GetPane(iPane)
            ' Prepare for determining axis range
            sXMin = Single.MaxValue : sXMax = Single.MinValue
            ' Prepare structures for creating point list for indicator
            info = DirectCast(gp.Tag, cIndicatorInfo)

            Try
                ' For all iterations
                For iTrial As Integer = 0 To Me.m_lind.Count - 1

                    If (iTrial = Me.m_iBest) Then
                        clr = Color.Green
                    Else
                        clr = Color.Blue
                    End If

                    ppl = New PointPairList()

                    ' For all times
                    For iTime As Integer = 0 To Me.m_lind(iTrial).Count - 1
                        ' Get indicator
                        ind = Me.m_lind(iTrial)(iTime)
                        ' Get indicator value
                        sValue = info.GetValue(ind)
                        ' Has a positive non-zero value?
                        If (sValue >= 0) Then
                            ' #Si: add point and update value min/max range
                            Dim pt As New PointPair(Me.Core.EcosimFirstYear + (ind.Time / cCore.N_MONTHS), sValue)
                            ppl.Add(pt)
                            sXMax = CSng(Math.Max(sXMax, pt.X))
                            sXMin = CSng(Math.Min(sXMin, pt.X))
                        End If
                    Next

                    ' No points added?
                    If ppl.Count = 0 Then
                        ' #Oui: clear the list
                        gp.CurveList.Clear()
                    Else

                        ' #Non: plot the line and configure the axis min/max range
                        Me.PlotLines(New LineItem() {Me.CreateLineItem(String.Format(My.Resources.HEADER_TRIAL_N, (iTrial + 1), info.Name),
                                                                       eSketchDrawModeTypes.Line, clr, ppl, info)},
                                     iPane, True, (iTrial = 0))
                        gp.XAxis.Scale.Min = sXMin
                        gp.XAxis.Scale.Max = sXMax
                    End If
                Next

                gp.AxisChange()

            Catch ex As Exception
                ' Whoah
                Debug.Assert(False, ex.Message)
            End Try

        Next

    End Sub

#End Region ' Refreshing

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
