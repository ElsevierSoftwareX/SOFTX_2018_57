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
Imports ZedGraph
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

<CLSCompliant(False)> _
Public Class cFromPrimaryProd
    Inherits cContentManager

    Public Sub New()
    End Sub

    Public Overrides Function PageTitle() As String
        Return "Flows and biomasses from primary producers"
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
        Dim sSum() As Single

        SetUpGridColumn()

        'Set up grid rows
        Grid.RowHeadersVisible = False
        Grid.RowCount = NetworkManager.nTrophicLevels + 2
        Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Rows(0).Frozen = True
        Grid.Rows(0).Height = FIRST_ROW_HEIGHT

        ReDim strRowContent(Grid.Columns.Count)
        ReDim sSum(Grid.Columns.Count)
        strRowContent(0) = My.Resources.COL_HDR_TRP_LVL_FLOW
        strRowContent(1) = My.Resources.COL_HDR_IMPORT
        strRowContent(2) = My.Resources.COL_HDR_CONSUM_PREDAT
        strRowContent(3) = My.Resources.COL_HDR_EXPORT
        strRowContent(4) = My.Resources.COL_HDR_FLOW_DET
        strRowContent(5) = My.Resources.COL_HDR_RESP
        strRowContent(6) = My.Resources.COL_HDR_THROUGHPUT
        Grid.Rows(0).SetValues(strRowContent)
        Grid.Visible = True

        For i As Integer = NetworkManager.nTrophicLevels To 1 Step -1
            'strRowContent(0) = CStr(i)
            strRowContent(0) = cStringUtils.ToRoman(i)
            If i = 1 Then
                strRowContent(1) = Me.StyleGuide.FormatNumber(NetworkManager.PPImport(i))
                sSum(1) = sSum(1) + NetworkManager.PPImport(i)
            Else
                strRowContent(1) = ""
            End If
            strRowContent(2) = Me.StyleGuide.FormatNumber(NetworkManager.PPConsByPred(i))
            sSum(2) = sSum(2) + NetworkManager.PPConsByPred(i)
            strRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.PPExport(i))
            sSum(3) = sSum(3) + NetworkManager.PPExport(i)
            strRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.PPToDetritus(i))
            sSum(4) = sSum(4) + NetworkManager.PPToDetritus(i)
            strRowContent(5) = Me.StyleGuide.FormatNumber(NetworkManager.PPRespiration(i))
            sSum(5) = sSum(5) + NetworkManager.PPRespiration(i)
            strRowContent(6) = Me.StyleGuide.FormatNumber(NetworkManager.PPThroughtput(i))
            sSum(6) = sSum(6) + NetworkManager.PPThroughtput(i)
            Grid.Rows(NetworkManager.nTrophicLevels - i + 1).SetValues(strRowContent)
            Grid.Rows(NetworkManager.nTrophicLevels - i + 1).Visible = True
        Next

        strRowContent(0) = My.Resources.ROW_HDR_SUM
        For i As Integer = 1 To Grid.Columns.Count - 1
            strRowContent(i) = Me.StyleGuide.FormatNumber(sSum(i))
        Next
        Grid.Rows(Grid.RowCount - 1).SetValues(strRowContent)
        Grid.Rows(Grid.RowCount - 1).Visible = True
        Grid.ClearSelection()
    End Sub

    Private Sub SetUpGridColumn()

        Grid.ReadOnly = True
        'DataGrid.RowCount = 1
        Grid.ColumnCount = 7

        SetGridColumnPropertyDefault(Grid)

        Grid.Columns(0).Frozen = True
        Grid.Columns(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control

    End Sub

End Class
