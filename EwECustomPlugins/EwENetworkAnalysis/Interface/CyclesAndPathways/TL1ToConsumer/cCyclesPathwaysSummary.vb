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

Namespace TL1ToConsumer

    <CLSCompliant(False)> _
    Public Class cCyclesPathwaysSummary
        Inherits cContentManager

        Public Sub New()
            '
        End Sub

        Public Overrides Function PageTitle() As String
            Return "Summary cycles and pathways TL1 to consumer"
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

            SetUpGridColumn()

            'Set up grid rows
            Grid.RowHeadersVisible = False
            Grid.RowCount = 3
            Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
            Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
            Grid.Rows(0).Frozen = True
            Grid.Rows(0).Height = FIRST_ROW_HEIGHT

            ReDim strRowContent(Grid.Columns.Count)
            strRowContent(0) = My.Resources.COL_HDR_PARAM
            strRowContent(1) = My.Resources.COL_HDR_VALUE
            Grid.Rows(0).SetValues(strRowContent)
            Grid.Rows(0).Visible = True

            strRowContent(0) = My.Resources.ROW_HDR_TOTAL_NUM_PATH
            strRowContent(1) = CStr(NetworkManager.PathWays.Count)
            Grid.Rows(1).SetValues(strRowContent)
            Grid.Rows(1).Visible = True

            strRowContent(0) = My.Resources.ROW_HDR_MEAN_PATH_LEN
            If NetworkManager.PathWays.Count = 0 Then
                strRowContent(1) = My.Resources.ROW_HDR_NOT_APP
            Else
                strRowContent(1) = Me.StyleGuide.FormatNumber(NetworkManager.NumArrows / NetworkManager.PathWays.Count)
            End If
            Grid.Rows(2).SetValues(strRowContent)
            Grid.Rows(2).Visible = True

            Grid.ClearSelection()

        End Sub

        Private Sub SetUpGridColumn()

            Grid.ColumnCount = 2

            SetGridColumnPropertyDefault(Grid)

            Grid.Columns(0).Frozen = True
            Grid.Columns(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
            Grid.Columns(0).Width = 400

        End Sub

    End Class

End Namespace

