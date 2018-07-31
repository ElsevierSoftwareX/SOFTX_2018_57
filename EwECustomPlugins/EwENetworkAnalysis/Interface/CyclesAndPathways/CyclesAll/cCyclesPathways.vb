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

Namespace CyclesAll

    <CLSCompliant(False)> _
    Public Class cCyclesPathways
        Inherits cContentManager

        Public Sub New()
        End Sub

        Public Overrides Function PageTitle() As String
            Return "All cycles and pathways"
        End Function

        Public Overrides Function Attach(ByVal manager As cNetworkManager, _
                                         ByVal datagrid As DataGridView, _
                                         ByVal graph As ZedGraphControl, _
                                         ByVal plot As ucPlot, _
                                         ByVal toolstrip As ToolStrip, _
                                         ByVal uic As cUIContext) As Boolean

            Dim bSucces As Boolean = MyBase.Attach(manager, datagrid, graph, plot, toolstrip, uic)

            If Me.NetworkManager.AskUserConfirmation(My.Resources.PROMPT_COMPUTE_ALL_CYCLES) Then
                Me.Grid.Visible = bSucces And Me.NetworkManager.FindPathwaysCyclesAll()
            End If
            Return Me.Grid.Visible

        End Function

        Public Overrides Sub DisplayData()
            Dim strRowContent() As String

            SetUpGridColumn()

            'Set up grid rows
            Grid.RowHeadersVisible = False

            ReDim strRowContent(Grid.Columns.Count)
            'm_NetworkManager.FindPathwaysCyclesAll()
            If NetworkManager.PathWays.Count > 0 Then
                Grid.RowCount = NetworkManager.PathWays.Count + 1
                Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
                Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
                Grid.Rows(0).Frozen = True
                Grid.Rows(0).Height = FIRST_ROW_HEIGHT

                strRowContent(0) = My.Resources.COL_HDR_PATH_NUM
                strRowContent(1) = My.Resources.COL_HDR_CYC
                Grid.Rows(0).SetValues(strRowContent)
                Grid.Rows(0).Visible = True

                For intPathwayIndex As Integer = 0 To NetworkManager.PathWays.Count - 1
                    strRowContent(0) = CStr(intPathwayIndex + 1)
                    strRowContent(1) = CStr(NetworkManager.PathWays.Item(intPathwayIndex))
                    Grid.Rows(intPathwayIndex + 1).SetValues(strRowContent)
                    Grid.Rows(intPathwayIndex + 1).Visible = True
                Next
            Else
                Grid.RowCount = 2
                Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
                Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
                Grid.Rows(0).Frozen = True
                Grid.Rows(0).Height = FIRST_ROW_HEIGHT

                strRowContent(0) = My.Resources.COL_HDR_PATH_NUM
                strRowContent(1) = My.Resources.COL_HDR_CYC
                Grid.Rows(0).SetValues(strRowContent)
                Grid.Rows(0).Visible = True

                strRowContent(0) = My.Resources.ROW_HDR_NO_PATH_FOUND
                strRowContent(1) = ""
                Grid.Rows(1).SetValues(strRowContent)
                Grid.Rows(1).Visible = True
            End If
            Grid.ClearSelection()
        End Sub

        Private Sub SetUpGridColumn()

            Grid.ReadOnly = True
            Grid.ColumnCount = 2

            SetGridColumnPropertyDefault(Grid)

            Grid.Columns(0).Frozen = True
            Grid.Columns(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control

            Grid.Columns(1).Width = 660
            Grid.Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        End Sub

    End Class

End Namespace

