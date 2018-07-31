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
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

<CLSCompliant(False)> _
Public Class cTotal
    Inherits cContentManager

    Public Sub New()
        '
    End Sub

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

        SetUpGridColumn()

        'Set up grid rows
        Grid.RowHeadersVisible = False
        Grid.RowCount = 6
        Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Rows(0).Frozen = True
        Grid.Rows(0).Height = FIRST_ROW_HEIGHT

        ReDim astrRowContent(Grid.Columns.Count)
        astrRowContent(0) = My.Resources.COL_HDR_SOURCE
        astrRowContent(1) = My.Resources.COL_HDR_ASCEND_FLOWBIT
        astrRowContent(2) = My.Resources.COL_HDR_ASCEND_PCT
        astrRowContent(3) = My.Resources.COL_HDR_OVERHEAD_FLOWBIT
        astrRowContent(4) = My.Resources.COL_HDR_OVERHEAD_PCT
        astrRowContent(5) = My.Resources.COL_HDR_CAPACITY_FLOWBIT
        astrRowContent(6) = My.Resources.COL_HDR_CAPACITY_PCT
        Grid.Rows(0).SetValues(astrRowContent)
        Grid.Rows(0).Visible = True

        astrRowContent(0) = My.Resources.ROW_HDR_IMPORT
        astrRowContent(1) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyImportTotal)
        astrRowContent(2) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyImportPer)
        astrRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadImportTotal)
        astrRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadImportPer)
        astrRowContent(5) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityImportTotal)
        astrRowContent(6) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityImportPer)
        Grid.Rows(1).SetValues(astrRowContent)
        Grid.Rows(1).Visible = True

        astrRowContent(0) = My.Resources.ROW_HDR_INTN_FLOW
        astrRowContent(1) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyInternalFlowTotal)
        astrRowContent(2) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyInternalFlowPer)
        astrRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadFlowTotal)
        astrRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadFlowPer)
        astrRowContent(5) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityFlowTotal)
        astrRowContent(6) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityFlowPer)
        Grid.Rows(2).SetValues(astrRowContent)
        Grid.Rows(2).Visible = True

        astrRowContent(0) = My.Resources.ROW_HDR_EXPORT
        astrRowContent(1) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyExportTotal)
        astrRowContent(2) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyExportPer)
        astrRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadExportTotal)
        astrRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadExportPer)
        astrRowContent(5) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityExportTotal)
        astrRowContent(6) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityExportPer)
        Grid.Rows(3).SetValues(astrRowContent)
        Grid.Rows(3).Visible = True

        astrRowContent(0) = My.Resources.ROW_HDR_RESP
        astrRowContent(1) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyRespTotal)
        astrRowContent(2) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyRespPer)
        astrRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadRespTotal)
        astrRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadRespPer)
        astrRowContent(5) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityRespTotal)
        astrRowContent(6) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityRespPer)
        Grid.Rows(4).SetValues(astrRowContent)
        Grid.Rows(4).Visible = True

        astrRowContent(0) = My.Resources.ROW_HDR_TOTAL
        astrRowContent(1) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyTotalsTotal)
        astrRowContent(2) = Me.StyleGuide.FormatNumber(NetworkManager.AscendancyTotalsPer)
        astrRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadTotalsTotal)
        astrRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.OverheadTotalsPer)
        astrRowContent(5) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityTotalsTotal)
        astrRowContent(6) = Me.StyleGuide.FormatNumber(NetworkManager.CapacityTotalsPer)
        Grid.Rows(5).SetValues(astrRowContent)
        Grid.Rows(5).Visible = True

        Grid.ClearSelection()

    End Sub

    Public Overrides Function PageTitle() As String
        Return "Total ascendency"
    End Function

    Private Sub SetUpGridColumn()

        Grid.ReadOnly = True
        'DataGrid.RowCount = 1
        Grid.ColumnCount = 7

        SetGridColumnPropertyDefault(Grid)

        Grid.Columns(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Columns(0).Frozen = True

    End Sub

End Class
