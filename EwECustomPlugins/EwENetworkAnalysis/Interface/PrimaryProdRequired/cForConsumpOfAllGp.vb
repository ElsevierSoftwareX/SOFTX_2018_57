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
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

<CLSCompliant(False)> _
Public Class cForConsumpOfAllGp
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

    Public Overrides Function PageTitle() As String
        Return "Primary production required for comsumption of all groups"
    End Function

    Public Overrides Sub DisplayData()

        Dim strRowContent() As String
        Dim sngTotalPPRCons As Single

        SetUpGridColumn()

        'Set up grid rows
        Grid.RowHeadersVisible = False
        Grid.RowCount = Me.NetworkManager.Core.nLivingGroups + 2
        Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Rows(0).Frozen = True
        Grid.Rows(0).Height = FIRST_ROW_HEIGHT

        ReDim strRowContent(Grid.Columns.Count)
        strRowContent(0) = ""
        strRowContent(1) = My.Resources.COL_HDR_GRP_NAME
        strRowContent(2) = My.Resources.COL_HDR_NUM_PATH
        strRowContent(3) = My.Resources.COL_HDR_TL
        strRowContent(4) = My.Resources.COL_HDR_PPR_PP
        strRowContent(5) = My.Resources.COL_HDR_PPR_DET
        strRowContent(6) = My.Resources.COL_HDR_PPR
        strRowContent(7) = My.Resources.COL_HDR_CONSUM
        strRowContent(8) = My.Resources.COL_HDR_PPR_COMSUM
        strRowContent(9) = My.Resources.COL_HDR_PPR_TOTAL_PP
        strRowContent(10) = My.Resources.COL_HDR_PPR_U_BIOMASS
        Grid.Rows(0).SetValues(strRowContent)
        Grid.Rows(0).Visible = True

        For i As Integer = 1 To Me.NetworkManager.Core.nLivingGroups
            strRowContent(0) = CStr(i)
            strRowContent(1) = NetworkManager.GroupName(i)
            strRowContent(2) = CStr(NetworkManager.NumOfPaths(i))
            strRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.TrophicLevel(i))
            strRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.PPRRequired(i))
            strRowContent(5) = Me.StyleGuide.FormatNumber(NetworkManager.PPRRequiredDet(i))
            strRowContent(6) = Me.StyleGuide.FormatNumber(NetworkManager.PPRRequiredSum(i))
            strRowContent(7) = Me.StyleGuide.FormatNumber(NetworkManager.PPRCons(i))
            sngTotalPPRCons = sngTotalPPRCons + NetworkManager.PPRCons(i)
            If NetworkManager.PPRCons(i) > 0.0 Then
                strRowContent(8) = Me.StyleGuide.FormatNumber(NetworkManager.PPROverCons(i))
            Else
                strRowContent(8) = ""
            End If
            strRowContent(9) = Me.StyleGuide.FormatNumber(NetworkManager.PPRTotPP(i))
            If NetworkManager.TotalPrimaryProduction > 0.0 Then
                strRowContent(10) = Me.StyleGuide.FormatNumber(NetworkManager.PPRU(i))
            Else
                strRowContent(10) = ""
            End If
            Grid.Rows(i).SetValues(strRowContent)
            Grid.Rows(i).Visible = True

            'DataGrid.Rows(i - 1).HeaderCell.Value = CStr(i)
            'DataGrid.Rows(i - 1).HeaderCell.Style.BackColor = Drawing.Color.Beige
        Next

        'Display total
        For i As Integer = 0 To Grid.Columns.Count - 1
            strRowContent(i) = ""
        Next
        strRowContent(1) = My.Resources.ROW_HDR_TOTAL
        strRowContent(2) = CStr((NetworkManager.NumLivPath + NetworkManager.NumDetPath))
        strRowContent(7) = Me.StyleGuide.FormatNumber(sngTotalPPRCons)
        Grid.Rows(Grid.Rows.Count - 1).SetValues(strRowContent)
        Grid.Rows(Grid.Rows.Count - 1).Visible = True

        'Hide some rows
        For i As Integer = 1 To Me.NetworkManager.Core.nLivingGroups
            If NetworkManager.PPRCons(i) <= 0.0 Or _
                NetworkManager.TotalPrimaryProduction <= 0.0 Then
                Grid.Rows(i).Visible = False
            End If
        Next
        Grid.ClearSelection()
        Cursor.Current = Cursors.Default
    End Sub

    Private Sub SetUpGridColumn()

        Grid.ReadOnly = True
        'DataGrid.RowCount = 1
        Grid.ColumnCount = 11

        SetGridColumnPropertyDefault(Grid)

        Grid.Columns(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Columns(0).Frozen = True
        Grid.Columns(0).Width = ID_COL_WIDTH

        Grid.Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        Grid.Columns(1).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Columns(1).Frozen = True
        Grid.Columns(1).Width = GRP_NAME_COL_WIDTH

    End Sub

End Class
