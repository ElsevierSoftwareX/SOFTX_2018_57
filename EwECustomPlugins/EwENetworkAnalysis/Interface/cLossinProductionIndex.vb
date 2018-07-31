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
Public Class cLossinProductionIndex
    Inherits cContentManager

    Public Sub New()
        '
    End Sub

    Public Overrides Function PageTitle() As String
        ' ToDo: globalize this
        Return "Loss of Production index"
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
        Dim LindexTot As Single = 0

        Me.NetworkManager.RunRequiredPrimaryProd()

        SetUpGridColumn()

        'Set up grid rows
        Grid.RowHeadersVisible = False
        Grid.RowCount = NetworkManager.nLivingGroups + 2
        Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Rows(0).Frozen = True
        Grid.Rows(0).Height = FIRST_ROW_HEIGHT

        ReDim astrRowContent(Grid.Columns.Count)
        astrRowContent(0) = ""
        astrRowContent(1) = My.Resources.COL_HDR_GRP_NAME
        astrRowContent(2) = My.Resources.COL_HDR_LINDEX
        astrRowContent(3) = My.Resources.COL_HDR_PSUST
        'astrRowContent(4) = My.Resources.COL_HDR_PSUST_SDLOWER
        'astrRowContent(5) = My.Resources.COL_HDR_PSUST_SDUPPER

        Grid.Rows(0).SetValues(astrRowContent)
        Grid.Rows(0).Visible = True

        For i As Integer = 1 To NetworkManager.nLivingGroups
            astrRowContent(0) = CStr(i)
            astrRowContent(1) = NetworkManager.GroupName(i)
            astrRowContent(2) = Me.StyleGuide.FormatNumber(NetworkManager.Lindex(i))
            astrRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.Psust(i))
            'astrRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.PsustSDlower(i))
            'astrRowContent(5) = Me.StyleGuide.FormatNumber(NetworkManager.PsustSDupper(i))
            LindexTot += NetworkManager.Lindex(i)
            Grid.Rows(i).SetValues(astrRowContent)
            Grid.Rows(i).Visible = True
        Next

        astrRowContent(0) = ""
        astrRowContent(1) = My.Resources.ROW_HDR_TOTAL
        astrRowContent(2) = Me.StyleGuide.FormatNumber(LindexTot)
        astrRowContent(3) = Me.StyleGuide.FormatNumber(NetworkManager.CalcPsust(LindexTot))
        Grid.Rows(NetworkManager.nLivingGroups + 1).SetValues(astrRowContent)

        For i As Integer = 1 To Me.NetworkManager.Core.nLivingGroups
            If NetworkManager.PPRCatchHarvest(i) <= 0.0 Or _
                NetworkManager.PPRCatchHarvest(i) <= 0.0 And NetworkManager.TotalPrimaryProduction <= 0.0 Then
                Grid.Rows(i).Visible = False
            End If
        Next


        Grid.ClearSelection()

    End Sub

    Private Sub SetUpGridColumn()

        'DataGrid.RowCount = 1
        Grid.ColumnCount = 4

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
