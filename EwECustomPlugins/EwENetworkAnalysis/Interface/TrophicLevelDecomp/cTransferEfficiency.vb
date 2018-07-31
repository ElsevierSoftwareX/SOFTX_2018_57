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

Imports System.Windows.Forms
Imports EwECore
Imports ZedGraph
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

<CLSCompliant(False)> _
Public Class cTransferEfficiency
    Inherits cContentManager

    Public Sub New()
        '
    End Sub

    Public Overrides Function PageTitle() As String
        Return "Transfer efficiency"
    End Function

    Public Overrides Function Attach(ByVal manager As cNetworkManager, _
                                     ByVal datagrid As DataGridView, _
                                     ByVal graph As ZedGraphControl, _
                                     ByVal plot As ucPlot, _
                                     ByVal toolstrip As ToolStrip, _
                                     ByVal uic As cUIContext) As Boolean
        Dim bSucces As Boolean = MyBase.Attach(manager, datagrid, graph, plot, toolstrip, uic)
        Me.Grid.Visible = bSucces
        Return bSucces
    End Function

    Public Overrides Sub DisplayData()

        Dim strRowContent() As String
        Dim TRavgP(4) As Single
        Dim TRavgD(4) As Single
        Dim TRavgT(4) As Single

        SetUpGridColumn(NetworkManager.nTrophicLevels)

        'Set up grid rows
        Grid.RowHeadersVisible = False
        Grid.RowCount = 9
        Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Rows(0).Frozen = True
        Grid.Rows(0).Height = FIRST_ROW_HEIGHT

        ReDim strRowContent(Grid.Columns.Count)
        strRowContent(0) = My.Resources.COL_HDR_SOURCE_TRP_LVL
        For i As Integer = 2 To NetworkManager.nTrophicLevels
            strRowContent(i - 1) = cStringUtils.ToRoman(i)
        Next
        Grid.Rows(0).SetValues(strRowContent)
        Grid.Rows(0).Visible = True

        strRowContent(0) = My.Resources.ROW_HDR_PRODUCER
        For i As Integer = 2 To NetworkManager.nTrophicLevels
            strRowContent(i - 1) = ""
            Dim sTemp As Single = Me.NetworkManager.PPTransferEfficiency(i)
            If (100.0 * sTemp) > 0 Then
                strRowContent(i - 1) = Me.StyleGuide.FormatNumber(100.0 * sTemp)
                If i <= 4 Then TRavgP(i) = sTemp
            End If
        Next
        Grid.Rows(1).SetValues(strRowContent)
        Grid.Rows(1).Visible = True

        strRowContent(0) = My.Resources.ROW_HDR_DET
        For i As Integer = 2 To NetworkManager.nTrophicLevels
            strRowContent(i - 1) = ""
            Dim sTmp As Single = NetworkManager.DetTransferEfficiency(i)
            If (100.0 * sTmp) > 0 Then
                strRowContent(i - 1) = Me.StyleGuide.FormatNumber(100.0 * sTmp)
                If i <= 4 Then TRavgD(i) = sTmp
            End If
        Next
        Grid.Rows(2).SetValues(strRowContent)
        Grid.Rows(2).Visible = True

        strRowContent(0) = My.Resources.ROW_HDR_ALL_FLOWS
        For i As Integer = 2 To NetworkManager.nTrophicLevels
            Dim sTr1 As Single = NetworkManager.PPConsByPred(i) + NetworkManager.DetConsByPred(i)
            If sTr1 > 0 Then
                If NetworkManager.PPThroughtput(i) + NetworkManager.DetThroughtput(i) > 0 Then
                    NetworkManager.TrEm1(i) = sTr1 / (NetworkManager.PPThroughtput(i) + NetworkManager.DetThroughtput(i))
                End If
            End If
            Dim sTotTr As Single = NetworkManager.TotTransferEfficiency(i)
            If (sTotTr > 0) Then
                strRowContent(i - 1) = Me.StyleGuide.FormatNumber(100.0 * sTotTr)
                If i <= 4 Then TRavgT(i) = sTotTr
            Else
                NetworkManager.TrEm1(i) = 0
                strRowContent(i - 1) = ""
            End If
        Next i
        Grid.Rows(3).SetValues(strRowContent)
        Grid.Rows(3).Visible = True

        strRowContent(0) = My.Resources.STR_PROP_TOTAL_FLOW + NetworkManager.FlowFromDetritus.ToString("F2")
        For i As Integer = 1 To Grid.Columns.Count - 1
            strRowContent(i) = ""
        Next
        Grid.Rows(4).SetValues(strRowContent)
        Grid.Rows(4).Visible = True

        strRowContent(0) = My.Resources.STR_TRANSFER_EFF
        For i As Integer = 1 To Grid.Columns.Count - 1
            strRowContent(i) = ""
        Next
        Grid.Rows(5).SetValues(strRowContent)
        Grid.Rows(5).Visible = True

        If TRavgP(2) > 0 And TRavgP(3) > 0 And TRavgP(4) > 0 Then
            TRavgP(0) = CSng((TRavgP(2) * TRavgP(3) * TRavgP(4)) ^ (1 / 3))
            strRowContent(0) = cStringUtils.Localize(My.Resources.STR_FROM_PRIM_PRODUCER, Me.UIContext.StyleGuide.FormatNumber(100.0 * TRavgP(0)))
        End If
        For i As Integer = 1 To Grid.Columns.Count - 1
            strRowContent(i) = ""
        Next
        Grid.Rows(6).SetValues(strRowContent)
        Grid.Rows(6).Visible = True

        If TRavgD(2) > 0 And TRavgD(3) > 0 And TRavgD(4) > 0 Then
            TRavgD(0) = CSng((TRavgD(2) * TRavgD(3) * TRavgD(4)) ^ (1 / 3))
            strRowContent(0) = cStringUtils.Localize(My.Resources.STR_FROM_DET, Me.UIContext.StyleGuide.FormatNumber(100.0 * TRavgD(0)))
        End If
        For i As Integer = 1 To Grid.Columns.Count - 1
            strRowContent(i) = ""
        Next
        Grid.Rows(7).SetValues(strRowContent)
        Grid.Rows(7).Visible = True

        If TRavgT(2) > 0 And TRavgT(3) > 0 And TRavgT(4) > 0 Then
            TRavgT(0) = CSng((TRavgT(2) * TRavgT(3) * TRavgT(4)) ^ (1 / 3))
            strRowContent(0) = cStringUtils.Localize(My.Resources.STR_TOTAL, Me.UIContext.StyleGuide.FormatNumber(100.0 * TRavgT(0)))
        End If
        For i As Integer = 1 To Grid.Columns.Count - 1
            strRowContent(i) = ""
        Next
        Grid.Rows(8).SetValues(strRowContent)
        Grid.Rows(8).Visible = True
        Grid.ClearSelection()
    End Sub

    Private Sub SetUpGridColumn(ByVal iNumTrophicLevel As Integer)

        Grid.ColumnCount = iNumTrophicLevel

        SetGridColumnPropertyDefault(Grid)

        Grid.Columns(0).Width = 330
        Grid.Columns(0).Frozen = True
        Grid.Columns(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Columns(0).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft

    End Sub

End Class
