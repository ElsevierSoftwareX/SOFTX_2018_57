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
Imports EwECore
Imports EwEUtils.Utilities
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecospace.Basemap

    <CLSCompliant(False)> _
    Public Class gridExportLayerMappings
        Inherits EwEGrid

#Region " Private vars "

        ''' <summary>The layers to map upon.</summary>
        Private m_aLayers As cEcospaceLayer()

        Private Enum eColumnTypes As Integer
            ColumnParent = 0
            ColumnLayer
            ColumnExport
            ColumnField
        End Enum

#End Region ' Private vars

#Region " Construction "

        Public Sub New()

        End Sub

#End Region ' Construction

#Region " Public interfaces "

        Public Property Layers() As cEcospaceLayer()
            Get
                Return Me.m_aLayers
            End Get
            Set(ByVal value As cEcospaceLayer())
                Me.m_aLayers = value
                Me.RefreshContent()
            End Set
        End Property

        Public Function Mappings() As Dictionary(Of cEcospaceLayer, String)

            ' Prepare mappings
            Dim dtOut As New Dictionary(Of cEcospaceLayer, String)
            Dim l As cEcospaceLayer = Nothing
            Dim strField As String = ""

            For i As Integer = 1 To Me.RowsCount
                l = Me.LayerAtRow(i)
                strField = Me.FieldAtRow(i)
                If Not String.IsNullOrWhiteSpace(strField) Then
                    dtOut(l) = strField
                End If
            Next
            Return dtOut

        End Function

#End Region ' Public interfaces

#Region " Overrides "

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            Me.Redim(1, System.Enum.GetValues(GetType(eColumnTypes)).Length)

            Me(0, eColumnTypes.ColumnParent) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.ColumnLayer) = New EwEColumnHeaderCell(SharedResources.HEADER_LAYER)
            Me(0, eColumnTypes.ColumnField) = New EwEColumnHeaderCell(SharedResources.HEADER_FIELD)
            Me(0, eColumnTypes.ColumnExport) = New EwEColumnHeaderCell(SharedResources.HEADER_EXPORT)

            Me.FixedColumns = 1
            Me.FixedColumnWidths = False

        End Sub

        Protected Overrides Sub FillData()

            If (Not Me.HasData) Then Return

            Me.RowsCount = 1
            Dim lfbase As New cLayerFactoryInternal()
            Dim dtParents As New Dictionary(Of String, EwEHierarchyGridCell)
            Dim dtHierarchy As New Dictionary(Of String, cCheckboxHierarchy)
            Dim layer As cEcospaceLayer = Nothing
            Dim cell As SourceGrid2.Cells.ICell = Nothing
            Dim cellParent As EwEHierarchyGridCell = Nothing
            Dim hrParent As cCheckboxHierarchy = Nothing
            Dim checkParent As EwECheckboxCell = Nothing
            Dim strGroup As String = ""
            Dim iRow As Integer = 0
            Dim vizChild As New cVisualizerEwEChildRowHeader()

            For iLayer As Integer = 0 To Me.m_aLayers.Length - 1

                layer = Me.m_aLayers(iLayer)

                strGroup = lfbase.GetLayerGroup(layer.VarName)
                If Not dtParents.ContainsKey(strGroup) Then
                    iRow = Me.AddRow()
                    For j As Integer = 0 To Me.ColumnsCount - 1 : Me(iRow, j) = New EwERowHeaderCell() : Next

                    ' Hierarchy cell
                    cellParent = New EwEHierarchyGridCell()
                    dtParents(strGroup) = cellParent
                    Me(iRow, eColumnTypes.ColumnParent) = cellParent

                    ' Layer group name
                    Me(iRow, eColumnTypes.ColumnLayer) = New EwERowHeaderCell(strGroup)

                    ' Hierarchy check cell
                    checkParent = New EwECheckboxCell(True)
                    Me(iRow, eColumnTypes.ColumnExport) = checkParent
                    hrParent = New cCheckboxHierarchy(checkParent)
                    dtHierarchy(strGroup) = hrParent

                    ' New row for data
                    iRow = Me.AddRow()

                Else
                    cellParent = dtParents(strGroup)
                    hrParent = dtHierarchy(strGroup)
                    iRow = Me.AddRow(cellParent.Row + cellParent.NumChildRows + 1)
                End If

                If (layer.Index <= 0) Then
                    cell = New EwERowHeaderCell("")
                Else
                    cell = New EwERowHeaderCell(CStr(layer.Index))
                End If
                Me(iRow, eColumnTypes.ColumnParent) = cell

                cell = New EwERowHeaderCell(layer.Name)
                cell.VisualModel = New cVisualizerEwEChildRowHeader
                Me(iRow, eColumnTypes.ColumnLayer) = cell

                cell = New EwECheckboxCell(True)
                cell.Behaviors.Add(Me.EwEEditHandler)
                Me(iRow, eColumnTypes.ColumnExport) = cell
                hrParent.Add(DirectCast(cell, EwECheckboxCell), Nothing)

                cell = New EwECell(layer.Name, GetType(String))
                cell.Behaviors.Add(Me.EwEEditHandler)
                Me(iRow, eColumnTypes.ColumnField) = cell

                Me.Rows(iRow).Tag = layer
                cellParent.AddChildRow(iRow)

            Next iLayer

            ' Run!
            For Each hr As cCheckboxHierarchy In dtHierarchy.Values
                hr.ManageCheckedStates = True
            Next

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.Columns(eColumnTypes.ColumnParent).Width = 20
            Me.Columns(eColumnTypes.ColumnParent).AutoSizeMode = SourceGrid2.AutoSizeMode.None
            Me.Columns(eColumnTypes.ColumnLayer).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            Me.Columns(eColumnTypes.ColumnField).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.StretchColumnsToFitWidth()
        End Sub

        Protected Overrides Function OnCellValueChanged(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

            MyBase.OnCellValueChanged(p, cell)

            Select Case DirectCast(p.Column, eColumnTypes)
                Case eColumnTypes.ColumnExport

                    Try
                        Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
                        If CBool(cell.GetValue(p)) Then
                            style = cStyleGuide.eStyleFlags.OK
                        Else
                            style = cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable
                        End If
                        DirectCast(Me(p.Row, eColumnTypes.ColumnField), EwECell).Style = style
                        Me(p.Row, eColumnTypes.ColumnField).Invalidate()
                    Catch ex As Exception

                    End Try

                Case eColumnTypes.ColumnField
                    Dim strName As String = Me.FieldAtRow(p.Row)
                    If String.IsNullOrWhiteSpace(strName) Then
                        cell.SetValue(p, Me.LayerAtRow(p.Row).Name)
                    End If

            End Select

            Return True

        End Function

        Private Function LayerAtRow(ByVal iRow As Integer) As cEcospaceLayer
            If iRow > 0 And iRow < Me.RowsCount Then
                Return DirectCast(Me.Rows(iRow).Tag, cEcospaceLayer)
            End If
            Return Nothing
        End Function

        Private Function FieldAtRow(ByVal iRow As Integer) As String
            If iRow > 0 And iRow < Me.RowsCount Then
                If CBool(Me(iRow, eColumnTypes.ColumnExport).Value) Then
                    Return CStr(Me(iRow, eColumnTypes.ColumnField).Value).Trim
                End If
            End If
            Return ""
        End Function

        Private Function HasData() As Boolean
            Return (Me.m_aLayers IsNot Nothing)
        End Function

#End Region ' Overrides

    End Class

End Namespace
