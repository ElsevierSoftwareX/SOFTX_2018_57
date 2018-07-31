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
Public Class cFromAllCombined
    Inherits cContentManager

    Public Sub New()
    End Sub

    Public Overrides Function PageTitle() As String
        Return "Flows and biomasses from all, combined"
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

        Dim astrRowContent() As String
        Dim asSum() As Single

        SetUpGridColumn()

        'Set up grid rows
        Grid.RowHeadersVisible = False
        Grid.RowCount = NetworkManager.nTrophicLevels + 5
        Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Rows(0).Frozen = True
        Grid.Rows(0).Height = FIRST_ROW_HEIGHT

        ReDim astrRowContent(Grid.Columns.Count)
        ReDim asSum(Grid.Columns.Count)
        astrRowContent(0) = My.Resources.COL_HDR_TRP_LVL_FLOW
        astrRowContent(1) = My.Resources.COL_HDR_IMPORT
        astrRowContent(2) = My.Resources.COL_HDR_CONSUM_PREDAT
        astrRowContent(3) = My.Resources.COL_HDR_EXPORT
        astrRowContent(4) = My.Resources.COL_HDR_FLOW_DET
        astrRowContent(5) = My.Resources.COL_HDR_RESP
        astrRowContent(6) = My.Resources.COL_HDR_THROUGHPUT
        Grid.Rows(0).SetValues(astrRowContent)
        Grid.Rows(0).Visible = True

        For i As Integer = NetworkManager.nTrophicLevels To 1 Step -1
            astrRowContent(0) = cStringUtils.ToRoman(i)
            If i = 1 Then
                astrRowContent(1) = Me.StyleGuide.FormatNumber(NetworkManager.DetImport(i) + NetworkManager.PPImport(i))
                asSum(1) = asSum(1) + NetworkManager.DetImport(i) + NetworkManager.PPImport(i)
            Else
                astrRowContent(1) = ""
            End If
            astrRowContent(2) = Me.StyleGuide.FormatNumber(NetworkManager.DetConsByPred(i) + NetworkManager.PPConsByPred(i))
            asSum(2) = asSum(2) + NetworkManager.DetConsByPred(i) + NetworkManager.PPConsByPred(i)
            astrRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.DetExport(i) + NetworkManager.PPExport(i))
            asSum(3) = asSum(3) + NetworkManager.DetExport(i) + NetworkManager.PPExport(i)
            astrRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.DetToDetritus(i) + NetworkManager.PPToDetritus(i))
            asSum(4) = asSum(4) + NetworkManager.DetToDetritus(i) + NetworkManager.PPToDetritus(i)
            astrRowContent(5) = Me.StyleGuide.FormatNumber(NetworkManager.DetRespiration(i) + NetworkManager.PPRespiration(i))
            asSum(5) = asSum(5) + NetworkManager.DetRespiration(i) + NetworkManager.PPRespiration(i)
            astrRowContent(6) = Me.StyleGuide.FormatNumber(NetworkManager.DetThroughtput(i) + NetworkManager.PPThroughtput(i))
            asSum(6) = asSum(6) + NetworkManager.DetThroughtput(i) + NetworkManager.PPThroughtput(i)
            Grid.Rows(NetworkManager.nTrophicLevels - i + 1).SetValues(astrRowContent)
            Grid.Rows(NetworkManager.nTrophicLevels - i + 1).Visible = True
        Next

        astrRowContent(0) = My.Resources.ROW_HDR_SUM
        For i As Integer = 1 To Grid.Columns.Count - 1
            astrRowContent(i) = Me.StyleGuide.FormatNumber(asSum(i))
        Next
        Grid.Rows(Grid.RowCount - 4).SetValues(astrRowContent)
        Grid.Rows(Grid.RowCount - 4).Visible = True

        astrRowContent(0) = My.Resources.ROW_HDR_EXTRACT_BREAK_CYC
        For i As Integer = 1 To Grid.Columns.Count - 2
            astrRowContent(i) = ""
        Next
        astrRowContent(Grid.Columns.Count - 1) = Me.StyleGuide.FormatNumber(NetworkManager.ExtractedToBreakCycles)
        Grid.Rows(Grid.RowCount - 3).SetValues(astrRowContent)
        Grid.Rows(Grid.RowCount - 3).Visible = True

        astrRowContent(0) = My.Resources.ROW_HDR_INPUT_TRP_LVL_II_PLUS
        For i As Integer = 1 To Grid.Columns.Count - 2
            astrRowContent(i) = ""
        Next
        astrRowContent(Grid.Columns.Count - 1) = Me.StyleGuide.FormatNumber(NetworkManager.InputTLIIPlus)
        Grid.Rows(Grid.RowCount - 2).SetValues(astrRowContent)
        Grid.Rows(Grid.RowCount - 2).Visible = True

        astrRowContent(0) = My.Resources.ROW_HDR_TOTAL_THROUGHPUT
        For i As Integer = 1 To Grid.Columns.Count - 2
            astrRowContent(i) = ""
        Next
        astrRowContent(Grid.Columns.Count - 1) = Me.StyleGuide.FormatNumber(NetworkManager.TotalThroughput + _
            NetworkManager.ExtractedToBreakCycles + NetworkManager.InputTLIIPlus)
        Grid.Rows(Grid.RowCount - 1).SetValues(astrRowContent)
        Grid.Rows(Grid.RowCount - 1).Visible = True
        Grid.ClearSelection()
        Cursor.Current = Cursors.Default

    End Sub

    Private Sub SetUpGridColumn()

        Grid.ReadOnly = True
        'DataGrid.RowCount = 1
        Grid.ColumnCount = 7

        SetGridColumnPropertyDefault(Grid)

        Grid.Columns(0).Width = 160
        Grid.Columns(0).Frozen = True
        Grid.Columns(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control

    End Sub

End Class
