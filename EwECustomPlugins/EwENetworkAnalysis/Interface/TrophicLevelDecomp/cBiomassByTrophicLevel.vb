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
Public Class cBiomassByTrophicLevel
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
        Me.Toolstrip.Visible = bSucces
        Me.ToolstripShowDisplayGroups(bSucces)
        Return bSucces
    End Function

    Public Overrides Function PageTitle() As String
        Return "Biomass by tropic level"
    End Function

    Public Overrides Sub DisplayData()

        Dim astrRowContent() As String

        Dim core As cCore = Me.UIContext.Core
        Dim bShowItem As Boolean = True
        Dim asBiomassGroupsShown() As Single
        Dim asMassDetritusShown() As Single

        SetUpGridColumn()

        'Set up grid rows
        Grid.RowHeadersVisible = False
        Grid.RowCount = NetworkManager.nTrophicLevels + 1
        Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Grid.Rows(0).Frozen = True
        Grid.Rows(0).Height = FIRST_ROW_HEIGHT

        'Calculate non-hidden data
        ReDim asBiomassGroupsShown(NetworkManager.nGroups)
        ReDim asMassDetritusShown(NetworkManager.nGroups)

        For i As Integer = 1 To NetworkManager.nGroups

            If Me.StyleGuide.GroupVisible(i) Then
                For j As Integer = 1 To NetworkManager.nTrophicLevels
                    If NetworkManager.RelativeFlow(i, j) = 0 Then
                    Else
                        If i <= core.nLivingGroups Then
                            asBiomassGroupsShown(j) += NetworkManager.RelativeFlow(i, j) * NetworkManager.BiomassByGroup(i)
                        Else
                            asMassDetritusShown(j) += NetworkManager.RelativeFlow(i, j) * NetworkManager.BiomassByGroup(i)
                        End If
                    End If
                Next
            End If
        Next

        ReDim astrRowContent(Grid.Columns.Count)
        astrRowContent(0) = My.Resources.COL_HDR_TRP_LVL
        astrRowContent(1) = My.Resources.COL_HDR_LIVING_TKM2
        astrRowContent(2) = My.Resources.COL_HDR_DETRITUS_TKM2
        astrRowContent(3) = My.Resources.COL_HDR_TOTAL_TKM2
        astrRowContent(4) = My.Resources.COL_HDR_NONHIDDEN
        Grid.Rows(0).SetValues(astrRowContent)
        Grid.Rows(0).Visible = True

        For i As Integer = NetworkManager.nTrophicLevels To 1 Step -1
            astrRowContent(0) = cStringUtils.ToRoman(i)
            astrRowContent(1) = Me.StyleGuide.FormatNumber(asBiomassGroupsShown(i))
            If i = 1 Then
                astrRowContent(2) = Me.StyleGuide.FormatNumber(asMassDetritusShown(i))
            Else
                astrRowContent(2) = ""
            End If
            astrRowContent(3) = Me.StyleGuide.FormatNumber(asBiomassGroupsShown(i) + asMassDetritusShown(i))
            astrRowContent(4) = Me.StyleGuide.FormatNumber(NetworkManager.BiomassByTrophicLevel(i) + NetworkManager.DetritusByTrophicLevel(i))
            Grid.Rows(NetworkManager.nTrophicLevels - i + 1).SetValues(astrRowContent)
            Grid.Rows(NetworkManager.nTrophicLevels - i + 1).Visible = True
        Next

        Grid.ClearSelection()
    End Sub

    Private Sub SetUpGridColumn()

        ' JS: add columns Living, detritus

        Grid.ReadOnly = True
        'DataGrid.RowCount = 1
        Grid.ColumnCount = 5

        SetGridColumnPropertyDefault(Grid)

        Grid.Columns(0).Frozen = True
        Grid.Columns(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control

    End Sub

End Class
