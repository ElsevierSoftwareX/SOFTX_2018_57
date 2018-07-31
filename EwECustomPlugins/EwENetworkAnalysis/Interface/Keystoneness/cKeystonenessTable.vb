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
Imports ScientificInterfaceShared.Style.cStyleGuide

#End Region ' Imports

<CLSCompliant(False)> _
Public Class cKeystonenessTable
    Inherits cContentManager

    Private Enum eColumnTypes As Integer
        Index = 0
        Name
        KS1
        KS2
        RelTotImpact
        KS3
    End Enum

    Public Sub New()
        ' Just needs main network to run
    End Sub

    Public Overrides Function PageTitle() As String
        Return My.Resources.PAGE_KEYSTONENESS
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

        SetUpGridColumn()

        'Set up grid rows
        Me.Grid.RowHeadersVisible = False
        Me.Grid.RowCount = Me.NetworkManager.nLivingGroups + 1
        Me.Grid.Rows(0).DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Me.Grid.Rows(0).DefaultCellStyle.BackColor = Drawing.SystemColors.Control
        Me.Grid.Rows(0).Frozen = True
        Me.Grid.Rows(0).Height = FIRST_ROW_HEIGHT

        ReDim astrRowContent(Grid.Columns.Count)
        astrRowContent(eColumnTypes.Index) = ""
        astrRowContent(eColumnTypes.Name) = My.Resources.COL_HDR_GRP_NAME
        astrRowContent(eColumnTypes.KS1) = My.Resources.COL_HDR_KEYSTONEINDEX1
        astrRowContent(eColumnTypes.KS2) = My.Resources.COL_HDR_KEYSTONEINDEX2
        astrRowContent(eColumnTypes.KS3) = My.Resources.COL_HDR_KEYSTONEINDEX3
        astrRowContent(eColumnTypes.RelTotImpact) = My.Resources.COL_HDR_RELTOTALIMPACT
        Me.Grid.Rows(0).SetValues(astrRowContent)
        Me.Grid.Rows(0).Visible = True

        For i As Integer = 1 To Me.NetworkManager.nLivingGroups
            astrRowContent(eColumnTypes.Index) = CStr(i)
            astrRowContent(eColumnTypes.Name) = Me.NetworkManager.GroupName(i)
            astrRowContent(eColumnTypes.KS1) = Me.StyleGuide.FormatNumber(Me.NetworkManager.KeystoneIndex1(i))
            astrRowContent(eColumnTypes.KS2) = Me.StyleGuide.FormatNumber(Me.NetworkManager.KeystoneIndex2(i))
            astrRowContent(eColumnTypes.KS3) = Me.StyleGuide.FormatNumber(Me.NetworkManager.KeystoneIndex3(i))
            astrRowContent(eColumnTypes.RelTotImpact) = Me.StyleGuide.FormatNumber(Me.NetworkManager.RelativeTotalImpact(i))
            Me.Grid.Rows(i).SetValues(astrRowContent)
            Me.Grid.Rows(i).Visible = True
        Next

        Me.Grid.ClearSelection()
    End Sub

    Private Sub SetUpGridColumn()

        Me.Graph.Visible = False
        Me.Grid.ReadOnly = True
        Me.Grid.Visible = True
        Me.Grid.ColumnCount = [Enum].GetValues(GetType(eColumnTypes)).Length

        SetGridColumnPropertyDefault(Me.Grid)

        Me.Grid.Columns(eColumnTypes.Index).DefaultCellStyle.BackColor = Me.StyleGuide.ApplicationColor(eApplicationColorType.NAMES_BACKGROUND)
        Me.Grid.Columns(eColumnTypes.Index).Frozen = True
        Me.Grid.Columns(eColumnTypes.Index).Width = ID_COL_WIDTH

        Me.Grid.Columns(eColumnTypes.Name).DefaultCellStyle.BackColor = Me.StyleGuide.ApplicationColor(eApplicationColorType.NAMES_BACKGROUND)
        Me.Grid.Columns(eColumnTypes.Name).Frozen = True
        Me.Grid.Columns(eColumnTypes.Name).Width = GRP_NAME_COL_WIDTH
        Me.Grid.Columns(eColumnTypes.Name).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft

    End Sub

End Class
