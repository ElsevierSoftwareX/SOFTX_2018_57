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

Option Strict On
Option Explicit On

Imports System.Windows.Forms
Imports ZedGraph
Imports ScientificInterfaceShared.Controls

<CLSCompliant(False)> _
Public Class cByGroup
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
        Me.Grid.RowHeadersVisible = False
        Me.Grid.RowCount = Me.NetworkManager.nGroups + 4
        Me.Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Me.Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Me.Grid.Rows(0).Frozen = True
        Me.Grid.Rows(0).Height = FIRST_ROW_HEIGHT

        ReDim astrRowContent(Grid.Columns.Count)
        astrRowContent(0) = ""
        astrRowContent(1) = My.Resources.COL_HDR_GRP_NAME
        astrRowContent(2) = My.Resources.COL_HDR_ASCEND
        astrRowContent(3) = My.Resources.COL_HDR_OVERHEAD
        astrRowContent(4) = My.Resources.COL_HDR_CAPACITY
        astrRowContent(5) = My.Resources.COL_HDR_INFO
        astrRowContent(6) = My.Resources.COL_HDR_THROUGHPUT_UNIT
        Me.Grid.Rows(0).SetValues(astrRowContent)
        Me.Grid.Rows(0).Visible = True

        For i As Integer = 1 To Me.NetworkManager.nGroups
            astrRowContent(0) = CStr(i)
            astrRowContent(1) = Me.NetworkManager.GroupName(i)
            astrRowContent(2) = Me.StyleGuide.FormatNumber(Me.NetworkManager.AscendancyByGroup(i))
            astrRowContent(3) = Me.StyleGuide.FormatNumber(Me.NetworkManager.OverheadByGroup(i))
            astrRowContent(4) = Me.StyleGuide.FormatNumber(Me.NetworkManager.CapacityByGroup(i))
            astrRowContent(5) = Me.StyleGuide.FormatNumber(Me.NetworkManager.InformationByGroup(i))
            astrRowContent(6) = Me.StyleGuide.FormatNumber(Me.NetworkManager.ThroughputByGroup(i))
            Me.Grid.Rows(i).SetValues(astrRowContent)
            Me.Grid.Rows(i).Visible = True
        Next

        astrRowContent(0) = ""
        astrRowContent(1) = My.Resources.ROW_HDR_IMPORT
        astrRowContent(2) = ""
        astrRowContent(3) = ""
        astrRowContent(4) = ""
        astrRowContent(5) = ""
        astrRowContent(6) = Me.StyleGuide.FormatNumber(NetworkManager.ThroughputByGroup(Me.NetworkManager.nGroups + 1))
        Me.Grid.Rows(NetworkManager.nGroups + 1).SetValues(astrRowContent)
        Me.Grid.Rows(NetworkManager.nGroups + 1).Visible = True

        astrRowContent(0) = ""
        astrRowContent(1) = My.Resources.ROW_HDR_TOTAL
        astrRowContent(2) = Me.StyleGuide.FormatNumber(Me.NetworkManager.AscendencyTotal)
        astrRowContent(3) = Me.StyleGuide.FormatNumber(Me.NetworkManager.OverheadTotal)
        astrRowContent(4) = Me.StyleGuide.FormatNumber(Me.NetworkManager.CapacityTotal)
        If Me.NetworkManager.ThroughputTotal > 0 Then
            astrRowContent(5) = Me.StyleGuide.FormatNumber((Me.NetworkManager.AscendencyTotal / Me.NetworkManager.ThroughputTotal))
        Else
            astrRowContent(5) = ""
        End If
        astrRowContent(6) = Me.StyleGuide.FormatNumber(Me.NetworkManager.ThroughputTotal)
        Me.Grid.Rows(Me.NetworkManager.nGroups + 2).SetValues(astrRowContent)
        Me.Grid.Rows(Me.NetworkManager.nGroups + 2).Visible = True

        astrRowContent(0) = ""
        astrRowContent(1) = My.Resources.ROW_HDR_PCT
        astrRowContent(2) = Me.StyleGuide.FormatNumber((Me.NetworkManager.AscendencyTotal / Me.NetworkManager.CapacityTotal * 100.0))
        astrRowContent(3) = Me.StyleGuide.FormatNumber((Me.NetworkManager.OverheadTotal / Me.NetworkManager.CapacityTotal * 100.0))
        astrRowContent(4) = Me.StyleGuide.FormatNumber((Me.NetworkManager.CapacityTotal / Me.NetworkManager.CapacityTotal * 100.0))
        astrRowContent(5) = ""
        astrRowContent(6) = ""
        Me.Grid.Rows(Me.NetworkManager.nGroups + 3).SetValues(astrRowContent)
        Me.Grid.Rows(Me.NetworkManager.nGroups + 3).Visible = True

        Me.Grid.ClearSelection()
    End Sub

    Public Overrides Function PageTitle() As String
        Return "Ascendency by group"
    End Function

    Private Sub SetUpGridColumn()

        Me.Graph.Visible = False
        Me.Grid.ReadOnly = True
        Me.Grid.Visible = True
        Me.Grid.ColumnCount = 7

        SetGridColumnPropertyDefault(Me.Grid)

        Me.Grid.Columns(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Me.Grid.Columns(0).Frozen = True
        Me.Grid.Columns(0).Width = ID_COL_WIDTH

        Grid.Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        Grid.Columns(1).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Columns(1).Frozen = True
        Grid.Columns(1).Width = GRP_NAME_COL_WIDTH

    End Sub

End Class
