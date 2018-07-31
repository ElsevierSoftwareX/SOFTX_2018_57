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
Imports EwECore.SpatialData
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports SourceGrid2
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecospace.Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' EwE grid for displaying datasets
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class gridDefineExternalSpatialData
        Inherits EwEGrid

#Region " Private classes "

        ''' <summary>
        ''' Sort datasets alphabetically, first by varname, then by name
        ''' </summary>
        Private Class cDatasetSorter
            Implements IComparer(Of ISpatialDataSet)

            Private m_fmt As New EwECore.Style.cVarnameTypeFormatter()

            Public Function Compare(x As ISpatialDataSet, y As ISpatialDataSet) As Integer _
                Implements System.Collections.Generic.IComparer(Of EwEUtils.SpatialData.ISpatialDataSet).Compare
                Dim iOrder As Integer = String.Compare(Me.m_fmt.GetDescriptor(x.VarName), Me.m_fmt.GetDescriptor(y.VarName))
                If (iOrder = 0) Then
                    iOrder = String.Compare(x.DisplayName, y.DisplayName)
                End If
                Return iOrder
            End Function

        End Class

#End Region ' Private classes

#Region " Private vars "

        Private m_mhEcospace As cMessageHandler = Nothing
        Private m_man As cSpatialDataConnectionManager = Nothing
        Private m_manSets As cSpatialDataSetManager = Nothing
        Private m_vmDescriptionHdr As SourceGrid2.VisualModels.IVisualModel
        Private m_vmDescriptionCell As SourceGrid2.VisualModels.IVisualModel

        Private Enum eColumnTypes As Integer
            Index = 0
            Name
            Applied
            Status
            DateFrom
            DateTo
            Description
        End Enum

#End Region ' Private vars

#Region " Construction / destruction "

        Public Sub New()
            MyBase.New()
            Me.m_vmDescriptionHdr = New cEwEGridColumnHeaderVisualizer(ContentAlignment.MiddleLeft)
            Me.m_vmDescriptionCell = New cEwECellVisualizer(ContentAlignment.MiddleLeft)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If (Me.m_mhEcospace IsNot Nothing) Then
                Me.m_mhEcospace.Dispose()
                Me.m_mhEcospace = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub

#End Region ' Construction / destruction

#Region " Internals "

        Public Overrides Property UIContext As cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(value As cUIContext)

                ' Deconfigure
                If (Me.UIContext IsNot Nothing) Then
                    Me.Core.Messages.RemoveMessageHandler(Me.m_mhEcospace)
                    Me.m_mhEcospace.Dispose()
                    Me.m_man = Nothing
                    Me.m_manSets = Nothing
                End If

                ' Apply
                MyBase.UIContext = value

                ' Configure
                If (value IsNot Nothing) Then
                    Me.m_man = Me.Core.SpatialDataConnectionManager
                    Me.m_manSets = Me.m_man.DatasetManager
                    Me.m_mhEcospace = New cMessageHandler(AddressOf OnCoreMessage, EwEUtils.Core.eCoreComponentType.External, eMessageType.Progress, Me.UIContext.SyncObject)
                    Me.Core.Messages.AddMessageHandler(Me.m_mhEcospace)
#If DEBUG Then
                    Me.m_mhEcospace.Name = "gridDefineExternalSpatialData"
#End If
                End If

            End Set
        End Property

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            ' ToDo_JS: globalize this
            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_NAME)
            Me(0, eColumnTypes.Applied) = New EwEColumnHeaderCell("Used")
            Me(0, eColumnTypes.Status) = New EwEColumnHeaderCell("Status")
            Me(0, eColumnTypes.DateFrom) = New EwEColumnHeaderCell(SharedResources.HEADER_FROM)
            Me(0, eColumnTypes.DateTo) = New EwEColumnHeaderCell(SharedResources.HEADER_TO)
            Me(0, eColumnTypes.Description) = New EwEColumnHeaderCell(SharedResources.HEADER_DESCRIPTION)
            Me(0, eColumnTypes.Description).VisualModel = Me.m_vmDescriptionHdr

            Me.Selection.SelectionMode = GridSelectionMode.Row
            Me.Selection.EnableMultiSelection = False
            Me.AllowBlockSelect = False

            Me.Columns(eColumnTypes.Index).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize

        End Sub

        Protected Overrides Sub FillData()

            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_manSets Is Nothing) Then Return

            Dim ds As ISpatialDataSet = Nothing
            Dim iRow As Integer = 0
            Dim iDS As Integer = 1
            Dim cell As EwECell = Nothing
            Dim vnLast As eVarNameFlags = eVarNameFlags.HabitatArea
            Dim hgc As EwEHierarchyGridCell = Nothing
            Dim vizParent As New cVisualizerEwEParentRowHeader()
            Dim vizKiddo As New cVisualizerEwEChildRowHeader()
            Dim strVar As String = ""
            Dim fmt As New cVarnameTypeFormatter()

            ' Get sorted list of datasets
            Dim datasets As New List(Of ISpatialDataSet)
            datasets.AddRange(Me.m_manSets)
            datasets.Sort(New cDatasetSorter)

            ' Add dataset rows
            For i As Integer = 0 To datasets.Count - 1
                ds = datasets(i)

                If (vnLast <> ds.VarName) Then
                    vnLast = ds.VarName

                    ' Create header row
                    iRow = Me.AddRow()
                    hgc = New EwEHierarchyGridCell()
                    Me(iRow, 0) = hgc

                    ' ToDo_JS: globalize this
                    If (vnLast = eVarNameFlags.NotSet) Then
                        strVar = "(Generic)"
                    Else
                        strVar = fmt.GetDescriptor(vnLast)
                    End If

                    Me(iRow, 1) = New EwEColumnHeaderCell(strVar)
                    Me(iRow, 1).VisualModel = vizParent
                    For iCol As Integer = 2 To Me.ColumnsCount - 1
                        Me(iRow, iCol) = New EwEColumnHeaderCell()
                    Next

                End If

                Dim comp As cDatasetCompatilibity = Me.m_manSets.Compatibility(ds)

                Dim strTStart As String = SharedResources.GENERIC_VALUE_FIRSTTIMESTEP
                Dim strTEnd As String = ""
                If (ds.TimeStart > Date.MinValue) And (ds.TimeStart < Date.MaxValue) Then strTStart = Me.StyleGuide.FormatDate(ds.TimeStart, False)
                If (ds.TimeEnd <> Date.MinValue) And (ds.TimeEnd < Date.MaxValue) Then strTEnd = Me.StyleGuide.FormatDate(ds.TimeEnd, False)

                iRow = Me.AddRow()
                Me(iRow, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iDS))
                Me(iRow, eColumnTypes.Name) = New EwECell(ds.DisplayName, GetType(String), cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable)
                Me(iRow, eColumnTypes.Name).VisualModel = vizKiddo
                Me(iRow, eColumnTypes.Applied) = New EwECheckboxCell(Me.m_man.IsApplied(ds), cStyleGuide.eStyleFlags.NotEditable)
                cell = New EwECell("", GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                Select Case comp.Compatibility
                    Case cDatasetCompatilibity.eCompatibilityTypes.Errors
                        cell.Image = SharedResources.Critical
                    Case cDatasetCompatilibity.eCompatibilityTypes.NoSpatial, cDatasetCompatilibity.eCompatibilityTypes.NoTemporal
                        cell.Image = SharedResources.Warning
                    Case Else
                        cell.Image = SharedResources.OK
                End Select
                Me(iRow, eColumnTypes.Status) = cell
                Me(iRow, eColumnTypes.Description) = New EwECell(ds.DataDescription, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                Me(iRow, eColumnTypes.Description).VisualModel = Me.m_vmDescriptionCell
                Me(iRow, eColumnTypes.DateFrom) = New EwECell(strTStart, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                Me(iRow, eColumnTypes.DateTo) = New EwECell(strTEnd, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                Me.Rows(iRow).Tag = ds
                hgc.AddChildRow(iRow)

                iDS += 1
            Next

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()

            Me.Columns(eColumnTypes.Description).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            Me.AutoSizeAll()
            Me.AutoStretchColumnsToFitWidth = True
            Me.FixedColumns = 2
            Me.FixedColumnWidths = False

        End Sub

        Public Event OnConfigDS(ds As ISpatialDataSet)

        Protected Overrides Sub OnCellDoubleClicked(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual)
            MyBase.OnCellDoubleClicked(p, cell)

            Try
                Dim item As Object = Me.Rows(p.Row).Tag
                If (item Is Nothing) Then Return
                If (Not TypeOf (item) Is ISpatialDataSet) Then Return

                RaiseEvent OnConfigDS(DirectCast(item, ISpatialDataSet))
            Catch ex As Exception

            End Try
        End Sub

        Private Function DatasetRowIndex(ds As ISpatialDataSet) As Integer
            For iRow As Integer = 1 To Me.RowsCount - 1
                Dim ri As RowInfo = Me.Rows(iRow)
                If (ReferenceEquals(ri.Tag, ds)) Then
                    Return iRow
                End If
            Next
            Return -1
        End Function

#End Region ' Internals

        Public Sub Fill(Optional ByVal dsSelect As ISpatialDataSet = Nothing)

            Me.RefreshContent()
            Me.SelectedDataset = dsSelect

        End Sub

        Public Property SelectedDataset As ISpatialDataSet
            Get
                If Me.SelectedRow < 1 Then Return Nothing
                Return DirectCast(Me.Rows(Me.SelectedRow).Tag, ISpatialDataSet)
            End Get
            Set(ds As ISpatialDataSet)
                Me.SelectRow(Me.DatasetRowIndex(ds))
            End Set
        End Property

        Public ReadOnly Property SelectedDatasets As ISpatialDataSet()
            Get
                Dim l As New List(Of ISpatialDataSet)
                Dim ds As ISpatialDataSet = Nothing
                For Each row As RowInfo In Me.Selection.SelectedRows
                    If (row.Tag IsNot Nothing) Then
                        If (TypeOf row.Tag Is ISpatialDataSet) Then
                            l.Add(DirectCast(row.Tag, ISpatialDataSet))
                        End If
                    End If
                Next
                Return l.ToArray
            End Get
        End Property
    End Class

End Namespace
