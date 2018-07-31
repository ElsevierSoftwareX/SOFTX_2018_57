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
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2.Cells

#End Region ' Imports

<CLSCompliant(False)> _
Public Class gridWeightTS
    Inherits EwEGrid

    Private Enum eColumnTypes As Integer
        Name = 0
        [Type]
        Enabled
        Weight
        CV
    End Enum

    Public Sub New()
        MyBase.New()
        Me.FixedColumnWidths = False
    End Sub

    Public Sub CheckAll(ByVal bCheck As Boolean)
        Dim cbc As SourceGrid2.Cells.Real.CheckBox = Nothing
        For iRow As Integer = 1 To Me.RowsCount - 1
            cbc = DirectCast(Me(iRow, CInt(eColumnTypes.Enabled)), SourceGrid2.Cells.Real.CheckBox)
            cbc.Checked = bCheck
        Next
    End Sub

    Public Function Apply(ByVal bIsLoading As Boolean) As Boolean

        ' Make sure this method is executed only when allowed
        If (Me.Core.ActiveTimeSeriesDatasetIndex <= 0) Then Return True

        Try
            Dim cbc As SourceGrid2.Cells.Real.CheckBox = Nothing
            Dim ts As cTimeSeries = Nothing
            Dim bChanged As Boolean = False

            For iRow As Integer = 1 To Me.RowsCount - 1
                cbc = DirectCast(Me(iRow, CInt(eColumnTypes.Enabled)), SourceGrid2.Cells.Real.CheckBox)
                ts = DirectCast(cbc.Tag, cTimeSeries)

                bChanged = bChanged Or (ts.Enabled <> cbc.Checked)
                ts.Enabled = bIsLoading Or cbc.Checked

                bChanged = bChanged Or (ts.WtType <> CSng(Me(iRow, CInt(eColumnTypes.Weight)).Value))
                ts.WtType = CSng(Me(iRow, CInt(eColumnTypes.Weight)).Value)

                bChanged = bChanged Or (ts.WtType <> CSng(Me(iRow, CInt(eColumnTypes.CV)).Value))
                ts.CV = CSng(Me(iRow, CInt(eColumnTypes.CV)).Value)
            Next
            Me.UIContext.Core.UpdateTimeSeries(bChanged And Not bIsLoading)
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Protected Overrides Sub InitStyle()

        MyBase.InitStyle()

        Me.FixedColumns = 1
        Me.Selection.SelectionMode = SourceGrid2.GridSelectionMode.Row

        Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

        Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_NAME)
        Me(0, eColumnTypes.Type) = New EwEColumnHeaderCell(SharedResources.HEADER_TYPE)
        Me(0, eColumnTypes.Enabled) = New EwEColumnHeaderCell(SharedResources.HEADER_ENABLE)
        Me(0, eColumnTypes.Weight) = New EwEColumnHeaderCell(SharedResources.HEADER_WEIGHT)
        Me(0, eColumnTypes.CV) = New EwEColumnHeaderCell(SharedResources.HEADER_CV)

    End Sub

    Protected Overrides Sub FillData()
        Dim ds As cTimeSeriesDataset = Nothing
        Dim ts As cTimeSeries = Nothing
        Dim strTarget As String = ""

        If (Me.UIContext Is Nothing) Then Return

        cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_PLEASE_WAIT)

        For iDS As Integer = 1 To Me.UIContext.Core.nTimeSeriesDatasets

            ' Get dataset
            ds = Me.UIContext.Core.TimeSeriesDataset(iDS)
            ' Is this dataset loaded?
            If ds.IsLoaded() Then

                ' #Yes: For all timeseries in the dataset
                For iTS As Integer = 0 To ds.Count - 1
                    ' Get TS
                    ts = ds.Item(iTS)
                    ' #Yes: create new ts item
                    Me.AddTimeSeriesRow(ts)
                Next iTS
            End If
        Next

        cApplicationStatusNotifier.EndProgress(Me.Core)

    End Sub

    Public Sub AddTimeSeriesRow(ByVal ts As cTimeSeries)

        Dim iRow As Integer = Me.AddRow()
        Dim cell As SourceGrid2.Cells.ICell = Nothing
        Dim bCanEnable As Boolean = (ts.ValidationStatus = eStatusFlags.OK)
        Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
        Dim fmt As New cTimeSeriesTypeFormatter()

        If Not bCanEnable Then style = cStyleGuide.eStyleFlags.NotEditable

        cell = New EwERowHeaderCell(ts.Name)
        Me(iRow, eColumnTypes.Name) = cell

        cell = New EwERowHeaderCell(fmt.GetDescriptor(ts.TimeSeriesType))
        Me(iRow, eColumnTypes.Type) = cell

        cell = New SourceGrid2.Cells.Real.CheckBox(ts.Enabled)
        cell.Tag = ts
        cell.DataModel.EnableEdit = bCanEnable
        Me(iRow, eColumnTypes.Enabled) = cell

        '' #1079: only allow weight for reference series
        'If ts.IsReference Then style = cStyleGuide.eStyleFlags.OK Else style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null

        cell = New EwECell(ts.WtType, GetType(Single), style)
        Me(iRow, eColumnTypes.Weight) = cell

        cell = New EwECell(ts.CV, GetType(Single), style)
        cell.Behaviors.Add(Me.EwEEditHandler)
        DirectCast(cell, EwECell).SuppressZero = True
        Me(iRow, eColumnTypes.CV) = cell

        'If Not ts.IsReference Then
        Me.UpdateRow(iRow)
        'End If

    End Sub

    Protected Overrides Function OnCellValueChanged(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean
        MyBase.OnCellValueChanged(p, cell)
        If MyBase.OnCellEdited(p, cell) Then
            If p.Column = eColumnTypes.CV Then
                UpdateRow(p.Row)
            End If
        End If
    End Function

    Protected Sub UpdateRow(iRow As Integer)

        Dim cellCV As EwECell = DirectCast(Me(iRow, eColumnTypes.CV), EwECell)
        Dim sCV As Single = CSng(cellCV.GetValue(New SourceGrid2.Position(iRow, eColumnTypes.CV)))
        Dim cellWeight As EwECell = DirectCast(Me(iRow, eColumnTypes.Weight), EwECell)
        Dim sWeight As Single = CSng(cellWeight.GetValue(New SourceGrid2.Position(iRow, eColumnTypes.Weight)))
        Dim cellEnable As ICell = DirectCast(Me(iRow, CInt(eColumnTypes.Enabled)), SourceGrid2.Cells.Real.CheckBox)
        Dim ts As cTimeSeries = DirectCast(cellEnable.Tag, cTimeSeries)

        If (sCV <= 0) Then
            cellWeight.Style = cStyleGuide.eStyleFlags.OK
            sWeight = ts.WtType
            cellWeight.SetValue(New SourceGrid2.Position(iRow, eColumnTypes.Weight), sWeight)
        Else
            cellWeight.Style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.ValueComputed
            sWeight = CSng(Math.Pow(sCV, -2))
            cellWeight.SetValue(New SourceGrid2.Position(iRow, eColumnTypes.Weight), sWeight)
        End If

    End Sub

End Class
