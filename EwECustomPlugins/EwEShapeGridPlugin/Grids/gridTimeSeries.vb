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
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Grid displaying <see cref="cTimeSeries"/>.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class gridTimeSeries
    Inherits gridShapeBase

    ''' <summary>Rows in the grid</summary>
    Private Enum eRowType As Integer
        Header = 0
        Thumbnail
        Name
        Weight
        PoolPrimary
        PoolSecundary
        Type
        Interval
        FirstTime
    End Enum

    ''' <summary>Time series UI display handler thingy</summary>
    Private m_handler As cTimeSeriesShapeGUIHandler = Nothing

    Public Sub New()
        MyBase.New()
    End Sub

#Region " Grid overrides "

    Protected Overrides Sub FillData()

        If Me.UIContext Is Nothing Then Return

        cApplicationStatusNotifier.StartProgress(Me.UIContext.Core, SharedResources.STATUS_UPDATING)

        Dim edt As EwEComboBoxCellEditor = Nothing

        Dim aGroupTSTypes As eTimeSeriesType() = Me.GroupTSTypes()
        Dim aFleetTSTypes As eTimeSeriesType() = Me.FleetTSTypes()

        Dim nPoints As Integer = Me.Core.nTimeSeriesYears
        Dim ats As cShapeData() = Me.Shapes
        Dim nTS As Integer = ats.Length
        Dim ts As cTimeSeries = Nothing
        Dim cell As SourceGrid2.Cells.ICell = Nothing
        Dim cmb As SourceGrid2.Cells.Real.ComboBox = Nothing

        Dim lGroups As New List(Of cCoreInputOutputBase)
        Dim lFleets As New List(Of cCoreInputOutputBase)

        For igroup As Integer = 1 To Me.Core.nGroups
            lGroups.Add(Me.Core.EcoPathGroupInputs(igroup))
        Next

        For ifleet As Integer = 1 To Me.Core.nFleets
            lFleets.Add(Me.Core.EcopathFleetInputs(ifleet))
        Next

        Dim collPoolPrim As ICollection = Nothing
        Dim selDatTypePrim As cCoreInputOutputBase = Nothing
        Dim collPoolSec As ICollection = Nothing
        Dim selDatTypeSec As cCoreInputOutputBase = Nothing
        Dim aTypes As eTimeSeriesType() = Nothing
        Dim fmt As New cTimeSeriesDatasetIntervalTypeFormatter()

        Me.Redim(nPoints + [Enum].GetValues(GetType(eRowType)).Length, nTS + 1)

        ' Create row headers
        Me(eRowType.Header, 0) = New EwEColumnHeaderCell(SharedResources.HEADER_INDEX)
        Me(eRowType.Thumbnail, 0) = New EwERowHeaderCell(SharedResources.HEADER_IMAGE)
        Me(eRowType.Name, 0) = New EwERowHeaderCell(SharedResources.HEADER_NAME)
        Me(eRowType.PoolPrimary, 0) = New EwERowHeaderCell(SharedResources.HEADER_TARGET)
        Me(eRowType.PoolSecundary, 0) = New EwERowHeaderCell(My.Resources.HEADER_TARGET_SECOND)
        Me(eRowType.Type, 0) = New EwERowHeaderCell(SharedResources.HEADER_TYPE)
        Me(eRowType.Weight, 0) = New EwERowHeaderCell(SharedResources.HEADER_WEIGHT)
        Me(eRowType.Interval, 0) = New EwERowHeaderCell(My.Resources.HEADER_INTERVAL)
        Me(eRowType.Type, 0) = New EwERowHeaderCell(SharedResources.HEADER_TYPE)

        For i As Integer = 0 To nPoints - 1
            Me(eRowType.FirstTime + i, 0) = New EwERowHeaderCell(Me.Label(i))
        Next

        For i As Integer = 0 To nTS - 1
            ts = DirectCast(ats(i), cTimeSeries)

            Me.Shape(i + 1) = ts

            Me(eRowType.Header, i + 1) = New EwEColumnHeaderCell(CStr(ts.Index))

            cell = New EwECell(ts, cStyleGuide.eStyleFlags.NotEditable)
            cell.VisualModel = New cVisualModelThumbnail(Me.Handler)
            Me(eRowType.Thumbnail, i + 1) = cell

            cell = New EwECell(ts.Name, GetType(String))
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(eRowType.Name, i + 1) = cell

            selDatTypePrim = Nothing
            If (TypeOf ts Is cGroupTimeSeries) Then
                Dim gts As cGroupTimeSeries = DirectCast(ts, cGroupTimeSeries)
                collPoolPrim = lGroups
                If (gts.GroupIndex >= 1) Then
                    selDatTypePrim = Me.Core.EcoPathGroupInputs(gts.GroupIndex)
                End If
                aTypes = Me.GroupTSTypes
            Else
                Dim fts As cFleetTimeSeries = DirectCast(ts, cFleetTimeSeries)
                collPoolPrim = lFleets
                If (fts.FleetIndex >= 1) Then
                    selDatTypePrim = Me.Core.EcopathFleetInputs(fts.FleetIndex)
                End If
                aTypes = Me.FleetTSTypes

                If (cTimeSeriesFactory.TimeSeriesCategory(ts.TimeSeriesType) = eTimeSeriesCategoryType.FleetGroup) Then
                    collPoolSec = lGroups
                    If (fts.GroupIndex >= 1) Then
                        selDatTypeSec = Me.Core.EcoPathGroupInputs(fts.GroupIndex)
                    End If
                    aTypes = Me.FleetGroupTSTypes()
                End If
            End If

            edt = New EwEComboBoxCellEditor(New cCoreInterfaceFormatter(), collPoolPrim)
            cell = New SourceGrid2.Cells.Real.Cell(selDatTypePrim, edt)
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(eRowType.PoolPrimary, i + 1) = cell

            If (collPoolSec IsNot Nothing) Then
                edt = New EwEComboBoxCellEditor(New cCoreInterfaceFormatter(), collPoolSec)
                cell = New SourceGrid2.Cells.Real.Cell(selDatTypeSec, edt)
                cell.Behaviors.Add(Me.EwEEditHandler)
                Me(eRowType.PoolSecundary, i + 1) = cell
            Else
                Me(eRowType.PoolSecundary, i + 1) = New EwECell("", cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable)
            End If

            edt = New EwEComboBoxCellEditor(New cTimeSeriesTypeFormatter(), aTypes)
            cell = New SourceGrid2.Cells.Real.Cell(ts.TimeSeriesType, edt)
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(eRowType.Type, i + 1) = cell

            cell = New EwECell(fmt.GetDescriptor(ts.Interval), GetType(String), cStyleGuide.eStyleFlags.NotEditable)
            Me(eRowType.Interval, i + 1) = cell

            cell = New EwECell(ts.WtType, GetType(Single))
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(eRowType.Weight, i + 1) = cell

            For j As Integer = 0 To nPoints - 1
                cell = New EwECell(ts.ShapeData(j + 1), GetType(Single))
                DirectCast(cell, EwECell).SuppressZero = Not ts.SupportsNull()
                cell.Behaviors.Add(Me.EwEEditHandler)
                Me(eRowType.FirstTime + j, i + 1) = cell
            Next
        Next

        cApplicationStatusNotifier.EndProgress(Me.UIContext.Core)

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        Me.Rows(eRowType.Thumbnail).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
        Me.Rows(eRowType.Thumbnail).Height = 48
        For i As Integer = 1 To Me.ColumnsCount - 1
            Me.Columns(i).Width = Math.Max(Me.Columns(i).Width, 48)
        Next
        ' Fix all descriptive rows
        Me.FixedRows = 2
        ' Fix header column
        Me.FixedColumns = 1
    End Sub

    Public Overrides ReadOnly Property Handler() As ScientificInterfaceShared.Controls.cShapeGUIHandler
        Get
            If (Me.m_handler Is Nothing) Then
                Me.m_handler = New cTimeSeriesShapeGUIHandler(Me.UIContext)
            End If
            Return Me.m_handler
        End Get
    End Property

    Public Overrides ReadOnly Property Manager() As IEnumerable
        Get
            If Me.Core.ActiveTimeSeriesDatasetIndex <= 0 Then Return Nothing
            Return Me.Core.TimeSeriesDataset(Me.Core.ActiveTimeSeriesDatasetIndex)
        End Get
    End Property

#End Region ' Grid overrides

#Region " Edits "

    Dim m_bInEdit As Boolean = False

    Protected Overrides Function OnCellEdited(ByVal p As SourceGrid2.Position, ByVal cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        Dim ts As cTimeSeries = DirectCast(Me.Shape(p.Column), cTimeSeries)
        Select Case DirectCast(p.Row, eRowType)
            Case eRowType.Name
                ts.Name = CStr(cell.GetValue(p))
            Case eRowType.PoolPrimary
                ts.DatPool = DirectCast(cell.GetValue(p), cCoreInputOutputBase).Index
            Case eRowType.Type
                ts.TimeSeriesType = DirectCast(cell.GetValue(p), eTimeSeriesType)
            Case eRowType.Weight
                ts.WtType = CSng(cell.GetValue(p))
            Case Else
                Dim iTime As Integer = p.Row - eRowType.FirstTime
                ts.ShapeData(iTime + 1) = CSng(cell.GetValue(p))
        End Select

        ' Do not invalidate individual shapes on a batch cell edit
        If Me.IsInBatchEdit Then
            Me.InvalidateShape(p.Column)
        Else
            ' Stop local cell edits from refreshing the content of the entire grid (see OnRefreshed)
            Me.m_bInEdit = True
            ts.Update()
            Me.m_bInEdit = False
        End If

        Return MyBase.OnCellEdited(p, cell)
    End Function

    Protected Overrides Function OnCellValueChanged(ByVal p As SourceGrid2.Position, ByVal cell As SourceGrid2.Cells.ICellVirtual) As Boolean
        Me.OnCellEdited(p, cell)
        Return MyBase.OnCellValueChanged(p, cell)
    End Function

#End Region ' Edits

#Region " Updates "

    Protected Overrides Sub OnRefreshed(ByVal sender As ScientificInterfaceShared.Controls.cShapeGUIHandler)
        ' Unpleasant: a refresh can be triggered from an external edit or by 
        ' this very interface in response to a cell edit. If a cell edit is in
        ' progress the grid content cannot be refreshed.

        ' In local cell edit?
        If Me.m_bInEdit Then
            ' #Yes: just invalidate the thumbnail
            Me.InvalidateRange(New SourceGrid2.Range(eRowType.Thumbnail, 0, eRowType.Thumbnail, Me.ColumnsCount - 1))
        Else
            ' #No: refresh the whole lot
            Me.RefreshContent()
        End If
    End Sub

#End Region ' Updates

#Region " Helper methods "

    Protected Function GroupIndexes() As Integer()
        Dim lIndexes As New List(Of Integer)
        For i As Integer = 1 To Me.Core.nGroups
            lIndexes.Add(i)
        Next
        Return lIndexes.ToArray
    End Function

    Protected Function GroupNames(ByVal aIndexes As Integer()) As String()
        Dim fmt As New cCoreInterfaceFormatter()
        Dim lstrNames As New List(Of String)

        For i As Integer = 0 To aIndexes.Length - 1
            Dim iGroup As Integer = aIndexes(i)
            Dim group As cCoreInputOutputBase = Me.Core.EcoPathGroupInputs(iGroup)
            lstrNames.Add(fmt.GetDescriptor(group))
        Next
        Return lstrNames.ToArray
    End Function

    Protected Function FleetIndexes() As Integer()
        Dim lIndexes As New List(Of Integer)
        For i As Integer = 1 To Me.Core.nFleets
            lIndexes.Add(i)
        Next
        Return lIndexes.ToArray
    End Function

    Protected Function FleetNames(ByVal aIndexes As Integer()) As String()
        Dim fmt As New cCoreInterfaceFormatter()
        Dim lstrNames As New List(Of String)
        For i As Integer = 0 To aIndexes.Length - 1
            Dim iFleet As Integer = aIndexes(i)
            Dim fleet As cCoreInputOutputBase = Me.Core.EcopathFleetInputs(iFleet)
            lstrNames.Add(fmt.GetDescriptor(fleet))
        Next
        Return lstrNames.ToArray
    End Function

    Protected Function GroupTSTypes() As eTimeSeriesType()
        Return cTimeSeriesFactory.CompatibleTypes(eTimeSeriesCategoryType.Group)
    End Function

    Protected Function FleetTSTypes() As eTimeSeriesType()
        Return cTimeSeriesFactory.CompatibleTypes(eTimeSeriesCategoryType.Fleet)
    End Function

    Protected Function FleetGroupTSTypes() As eTimeSeriesType()
        Return cTimeSeriesFactory.CompatibleTypes(eTimeSeriesCategoryType.FleetGroup)
    End Function

    Protected Function TSTypeNames(ByVal aTypes As eTimeSeriesType()) As String()
        Dim lstrNames As New List(Of String)
        Dim desc As New cTimeSeriesTypeFormatter()
        For Each tst As eTimeSeriesType In aTypes
            lstrNames.Add(desc.GetDescriptor(tst))
        Next tst
        Return lstrNames.ToArray
    End Function

    Protected Overrides Function Label(ByVal iPoint As Integer) As String
        Dim ds As cTimeSeriesDataset = Nothing
        If (Me.Core.ActiveTimeSeriesDatasetIndex = -1) Then Return "?"
        ds = Me.Core.TimeSeriesDataset(Me.Core.ActiveTimeSeriesDatasetIndex)
        Select Case ds.TimeSeriesInterval
            Case eTSDataSetInterval.Annual
                Return CStr(iPoint + Me.Core.EcosimFirstYear)
            Case eTSDataSetInterval.TimeStep
                Dim iMonth As Integer = (iPoint Mod 12) + 1
                If Not Me.IsSeasonal And (iMonth = 1) Then
                    Return CStr(Math.Floor(iPoint / cCore.N_MONTHS) + Me.Core.EcosimFirstYear)
                End If
                Return cDateUtils.GetMonthName(iMonth)
            Case Else
                Debug.Assert(False)
        End Select
        Return "?"
    End Function

#End Region ' Helper methods

End Class
