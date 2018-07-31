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
Imports EwEUtils.Utilities
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
    Public Class gridApplyDatasets
        Inherits EwEGrid

#Region " Private vars "

        Private m_mhEcospace As cMessageHandler = Nothing
        Private m_man As cSpatialDataConnectionManager = Nothing
        Private m_manSets As cSpatialDataSetManager = Nothing
        Private m_lConnections As New List(Of cSpatialDataConnection)

        Private Enum eColumnTypes As Integer
            Index = 0
            Name
            DateFrom
            DateTo
            TempOverlap
            SpatOverlap
            Description
        End Enum

#End Region ' Private vars

#Region " Construction / destruction "

        Public Sub New()
            MyBase.New()
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
                    Me.m_manSets = Nothing
                    Me.m_man = Nothing
                End If

                ' Apply
                MyBase.UIContext = value

                ' Configure
                If (value IsNot Nothing) Then
                    Me.m_man = Me.UIContext.Core.SpatialDataConnectionManager
                    Me.m_manSets = Me.m_man.DatasetManager

                    Me.m_mhEcospace = New cMessageHandler(AddressOf OnCoreMessage, EwEUtils.Core.eCoreComponentType.External, eMessageType.Progress, Me.UIContext.SyncObject)
                    Me.Core.Messages.AddMessageHandler(Me.m_mhEcospace)
#If DEBUG Then
                    Me.m_mhEcospace.Name = "gridDatasets"
#End If
                End If

            End Set
        End Property

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_NAME)
            Me(0, eColumnTypes.DateFrom) = New EwEColumnHeaderCell(SharedResources.HEADER_FROM)
            Me(0, eColumnTypes.DateTo) = New EwEColumnHeaderCell(SharedResources.HEADER_TO)
            Me(0, eColumnTypes.SpatOverlap) = New EwEColumnHeaderCell(SharedResources.HEADER_OVERLAP_SPATIAL)
            Me(0, eColumnTypes.TempOverlap) = New EwEColumnHeaderCell(SharedResources.HEADER_OVERLAP_TEMPORAL)
            Me(0, eColumnTypes.Description) = New EwEColumnHeaderCell(SharedResources.HEADER_DESCRIPTION)

            Me.Selection.SelectionMode = GridSelectionMode.Row
            Me.Selection.EnableMultiSelection = False
            Me.AllowBlockSelect = False

        End Sub

        Protected Overrides Sub FillData()

            If (Me.UIContext Is Nothing) Then Return

            Dim vfmt As New cVarnameTypeFormatter()
            Dim conn As cSpatialDataConnection = Nothing
            Dim iRow As Integer = 0
            Dim cell As EwECell = Nothing

            ' Dataset rows
            For i As Integer = 0 To Me.m_lConnections.Count - 1
                conn = Me.m_lConnections(i)
                iRow = Me.AddRow()

                Dim strTStart As String = SharedResources.GENERIC_VALUE_FIRSTTIMESTEP
                Dim strTEnd As String = ""
                Dim ds As ISpatialDataSet = conn.Dataset

                If (ds.TimeStart > Date.MinValue) And (ds.TimeStart < Date.MaxValue) Then strTStart = ds.TimeStart.ToShortDateString
                If (ds.TimeEnd <> Date.MinValue) And (ds.TimeEnd < Date.MaxValue) Then strTEnd = ds.TimeEnd.ToShortDateString

                Me(iRow, eColumnTypes.Index) = New EwERowHeaderCell(CStr(i + 1))
                Me(iRow, eColumnTypes.Name) = New EwECell(ds.DisplayName, GetType(String), cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable)
                Me(iRow, eColumnTypes.DateFrom) = New EwECell(strTStart, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                Me(iRow, eColumnTypes.DateTo) = New EwECell(strTEnd, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                Me(iRow, eColumnTypes.SpatOverlap) = New EwECell("", GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                Me(iRow, eColumnTypes.TempOverlap) = New EwECell("", GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                Me(iRow, eColumnTypes.Description) = New EwECell(ds.DataDescription, GetType(String), cStyleGuide.eStyleFlags.NotEditable)

                Me.ConnectionAtRow(iRow) = conn

                Me.UpdateConnectionRow(iRow)
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

        Private Sub UpdateConnectionRow(iRow As Integer)

            If (iRow < 1) Then Return

            ' ToDo: globalize this

            Dim cache As cSpatialDataCache = cSpatialDataCache.DefaultDataCache
            Dim conn As cSpatialDataConnection = Me.ConnectionAtRow(iRow)
            Dim ds As ISpatialDataSet = conn.Dataset
            Dim comp As cDatasetCompatilibity = Me.m_manSets.Compatibility(ds)
            Dim iNumTS As Integer = Math.Max(Core.nEcospaceTimeSteps, 1)
            Dim strVal As String = ""
            Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.NotEditable

            ' Temporal overlap
            Me(iRow, eColumnTypes.TempOverlap).Value = CStr(comp.NumOverlappingTimeSteps) & " time step(s)"

            ' Spatial overlap col
            If (comp.NumIndexed < comp.NumOverlappingTimeSteps) Then
                strVal = cStringUtils.Localize(SharedResources.VALUE_INDEXED_PERCENT, CInt(Math.Ceiling(100 * comp.NumIndexed / (comp.NumOverlappingTimeSteps + 1))))
            Else
                Dim fmt As New cSpatialDatasetCompatibilityFormatter()
                strVal = fmt.GetDescriptor(comp, eDescriptorTypes.Abbreviation)
            End If
            Me(iRow, eColumnTypes.SpatOverlap).Value = strVal

            Me.InvalidateCells()

        End Sub

        Public Overrides Sub OnCoreMessage(ByRef msg As cMessage)

            Try
                ' May have been disposed already
                If (msg.DataType = EwEUtils.Core.eDataTypes.EcospaceSpatialDataConnection) Then
                    For iRow As Integer = 1 To Me.RowsCount - 1
                        Dim conn As cSpatialDataConnection = Me.ConnectionAtRow(iRow)
                        Debug.Assert(conn IsNot Nothing)
                        If (ReferenceEquals(conn.Dataset, Me.m_manSets.IndexDataset)) Then
                            Me.UpdateConnectionRow(iRow)
                        End If
                    Next
                Else
                    MyBase.OnCoreMessage(msg)
                End If

            Catch ex As Exception

            End Try

        End Sub

#End Region ' Internals

        Public Sub AddConnection(conn As cSpatialDataConnection, bSelectRow As Boolean)
            Me.m_lConnections.Add(conn)
            Me.RefreshContent()
            If (bSelectRow) Then
                Me.SelectRow(Me.m_lConnections.Count)
            End If
        End Sub

        Public Sub RemoveConnection(conn As cSpatialDataConnection)
            Dim iRow As Integer = Me.SelectedRow
            Me.m_lConnections.Remove(conn)
            Me.RefreshContent()
            If Me.RowsCount > 1 Then
                Me.SelectRow(Math.Min(Me.m_lConnections.Count, Math.Max(1, iRow)))
            End If
        End Sub

        Public Property SelectedConnection As cSpatialDataConnection
            Get
                Return Me.ConnectionAtRow(Me.SelectedRow)
            End Get
            Set(value As cSpatialDataConnection)
                For iRow As Integer = 1 To Me.RowsCount
                    If (ReferenceEquals(value, ConnectionAtRow(iRow))) Then
                        Me.SelectRow(iRow)
                        Return
                    End If
                Next
                Me.SelectRow(-1)
            End Set
        End Property

        Private Property ConnectionAtRow(ByVal iRow As Integer) As cSpatialDataConnection
            Get
                If (iRow < 1) Then Return Nothing
                Return DirectCast(Me.Rows(iRow).Tag, cSpatialDataConnection)
            End Get
            Set(value As cSpatialDataConnection)
                Me.Rows(iRow).Tag = value
            End Set
        End Property

    End Class

End Namespace
