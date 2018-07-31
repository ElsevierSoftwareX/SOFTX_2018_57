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
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports SourceGrid2
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecospace

    <CLSCompliant(False)>
    Public Class gridExternalSpatialData
        Inherits EwEGrid

        Private Class cConnectionInfo
            Public Sub New(adt As cSpatialDataAdapter, layer As cEcospaceLayer, Optional iIndex As Integer = 0)
                Me.Adapter = adt
                Me.Layer = layer
                Me.Index = iIndex
            End Sub
            Public Property Adapter As cSpatialDataAdapter
            Public Property Layer As cEcospaceLayer
            Public Property Index As Integer
        End Class

        Private m_man As cSpatialDataConnectionManager
        Private m_manSets As cSpatialDataSetManager
        Private m_filterVarName As eVarNameFlags = eVarNameFlags.NotSet
        Private m_bOnlyShowConnected As Boolean = False
        Private m_nBaseCols As Integer = 0
        Private m_bmCell As BehaviorModels.CustomEvents = Nothing

        Private Enum eColumnTypes As Integer
            Index = 0
            Name
            Enabled
        End Enum

        Public Sub New()
            Me.m_nBaseCols = [Enum].GetValues(GetType(eColumnTypes)).Length
            Me.m_bmCell = New BehaviorModels.CustomEvents()
        End Sub

        Public Overrides Property UIContext As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(value As ScientificInterfaceShared.Controls.cUIContext)

                If (Me.UIContext IsNot Nothing) Then
                    Me.m_man = Nothing
                    Me.m_manSets = Nothing
                    RemoveHandler m_bmCell.Click, AddressOf CellClick
                End If
                ' Peek ahead...
                If (value IsNot Nothing) Then
                    Me.m_man = value.Core.SpatialDataConnectionManager
                    Me.m_manSets = Me.m_man.DatasetManager
                    AddHandler m_bmCell.Click, AddressOf CellClick

                    Dim bHasConnections As Boolean = False
                    For Each adt As cSpatialDataAdapter In Me.m_man.Adapters
                        bHasConnections = bHasConnections Or (adt.Connections().Length > 0)
                    Next
                    Me.m_bOnlyShowConnected = bHasConnections

                End If
                MyBase.UIContext = value
            End Set
        End Property

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            Me.Redim(1, Me.m_nBaseCols + cSpatialDataStructures.cMAX_CONN)

            Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
            Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(My.Resources.HEADER_MAP)
            Me(0, eColumnTypes.Enabled) = New EwEColumnHeaderCell(My.Resources.HEADER_DRIVER_ENABLED)

            For i As Integer = 1 To cSpatialDataStructures.cMAX_CONN
                Me(0, Me.m_nBaseCols + i - 1) = New EwEColumnHeaderCell(cStringUtils.Localize(SharedResources.HEADER_SLOT, i))
            Next

            Me.FixedColumns = Me.m_nBaseCols

        End Sub

        Protected Overrides Sub FillData()

            If (Me.UIContext Is Nothing) Then Return

            Dim bm As cEcospaceBasemap = Me.Core.EcospaceBasemap
            Dim ecospaceModelParams As cEcospaceModelParameters = Me.UIContext.Core.EcospaceModelParameters()
            Dim fmt As New cSpatialDataAdapterFormatter()
            Dim fact As New cLayerFactoryInternal()
            Dim strAdapter As String = ""
            Dim layer As cEcospaceLayer = Nothing
            Dim layers() As cEcospaceLayer = Nothing
            Dim hgcGroup As EwEHierarchyGridCell = Nothing
            Dim iRow As Integer = 0

            Dim vizParent As New cVisualizerEwEParentRowHeader()
            Dim vizChild As New cVisualizerEwEChildRowHeader()
            Dim bHasConnections As Boolean = False

            Me.RowsCount = 1

            For Each adt As cSpatialDataAdapter In Me.m_man.Adapters

                bHasConnections = (adt.Connections().Count > 0)

                If (((adt.VarName = Me.m_filterVarName) Or (Me.m_filterVarName = eVarNameFlags.NotSet)) And _
                    (bHasConnections Or (Me.m_bOnlyShowConnected = False))) Then

                    ' Get group name for the adapter
                    strAdapter = fmt.GetDescriptor(adt)
                    ' Get layers for the adapter
                    layers = bm.Layers(adt.VarName)

                    ' Header row
                    iRow = Me.AddRow()
                    hgcGroup = New EwEHierarchyGridCell()
                    Me(iRow, eColumnTypes.Index) = hgcGroup
                    Me(iRow, eColumnTypes.Name) = New EwERowHeaderCell(strAdapter)
                    Me(iRow, eColumnTypes.Name).VisualModel = vizParent
                    For i As Integer = 2 To Me.ColumnsCount - 1
                        Me(iRow, i) = New EwEColumnHeaderCell("")
                    Next

                    ' All layers
                    For i As Integer = 0 To layers.Count - 1

                        layer = layers(i)
                        bHasConnections = (adt.Connections(layer.Index).Count > 0)

                        If (bHasConnections Or (Me.m_bOnlyShowConnected = False)) Then

                            iRow = Me.AddRow()
                            Me(iRow, eColumnTypes.Index) = New EwERowHeaderCell(CStr(layer.Index))
                            Me(iRow, eColumnTypes.Name) = New EwERowHeaderCell(layer.Name)
                            Me(iRow, eColumnTypes.Name).VisualModel = vizChild

                            Me(iRow, eColumnTypes.Enabled) = New EwECheckboxCell(adt.IsEnabled(layer.Index))
                            Me(iRow, eColumnTypes.Enabled).Behaviors.Add(Me.EwEEditHandler)

                            Dim conns As cSpatialDataConnection() = adt.Connections(layer.Index)
                            Dim conn As cSpatialDataConnection = Nothing

                            For j As Integer = 0 To cSpatialDataStructures.cMAX_CONN - 1 'Me.m_nBaseCols To Me.ColumnsCount - 1
                                If (j < conns.Length) Then
                                    conn = conns(j)
                                Else
                                    conn = Nothing
                                End If
                                Me(iRow, j + Me.m_nBaseCols) = New EwECell("", GetType(String), cStyleGuide.eStyleFlags.OK)
                                Me(iRow, j + Me.m_nBaseCols).Behaviors.Add(Me.m_bmCell)
                            Next
                            hgcGroup.AddChildRow(iRow)

                            Me.InfoAtRow(iRow) = New cConnectionInfo(adt, layer)
                            Me.UpdateDatasetRow(iRow)
                        End If
                    Next
                End If

            Next
        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.AllowBlockSelect = False
            Me.FixedColumnWidths = True
        End Sub

        Protected Sub UpdateDatasetRow(iRow As Integer)

            Dim iNumDefined As Integer = 0
            Dim iNumConnected As Integer = 0
            Dim info As cConnectionInfo = Me.InfoAtRow(iRow)

            If (info Is Nothing) Then Return

            Dim bEnabled As Boolean = (Not info.Layer.CanDeactivate) Or (info.Layer.IsActive)

            Dim c As IEwECell = CType(Me(iRow, eColumnTypes.Enabled), IEwECell)
            If (bEnabled) Then
                c.Style = c.Style And Not cStyleGuide.eStyleFlags.NotEditable
            Else
                c.Style = c.Style Or cStyleGuide.eStyleFlags.NotEditable
            End If

            For j As Integer = Me.m_nBaseCols To Me.ColumnsCount - 1

                Dim strText As String = ""
                Dim conn As cSpatialDataConnection = Me.ConnectionAtCell(iRow, j)

                If (conn IsNot Nothing) Then
                    Dim ds As ISpatialDataSet = conn.Dataset()
                    If (ds IsNot Nothing) Then
                        strText = ds.DisplayName
                    End If
                End If

                c = CType(Me(iRow, j), IEwECell)
                If (bEnabled) Then
                    c.Style = c.Style And Not cStyleGuide.eStyleFlags.NotEditable
                Else
                    c.Style = c.Style Or cStyleGuide.eStyleFlags.NotEditable
                End If

                Me(iRow, j).Value = strText
                Me.AutoSizeColumn(j, 20)
            Next

        End Sub

        Protected Sub CellClick(ByVal sender As Object, ByVal e As PositionEventArgs)
            Try

                Dim info As cConnectionInfo = Me.InfoAtRow(e.Position.Row)
                If (info Is Nothing) Then Return

                ' Fire off a change
                Dim cmd As cEcospaceConfigureConnectionCommand = CType(Me.UIContext.CommandHandler.GetCommand(cEcospaceConfigureConnectionCommand.cCOMMAND_NAME), cEcospaceConfigureConnectionCommand)
                cmd.Invoke(info.Layer, Me.ConnectionAtCell(e.Position))

                Me.RefreshContent()

            Catch ex As Exception
                ' Whoah
            End Try
        End Sub

        Protected Overrides Function OnCellValueChanged(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean
            If (p.Column = eColumnTypes.Enabled) Then
                Dim layer As cEcospaceLayer = Me.InfoAtRow(p.Row).Layer
                Dim adt As cSpatialDataAdapter = Me.InfoAtRow(p.Row).Adapter
                adt.IsEnabled(layer.Index) = CBool(cell.GetValue(p))
            End If
        End Function

        Public Property Filter As eVarNameFlags
            Get
                Return Me.m_filterVarName
            End Get
            Set(value As eVarNameFlags)
                If (value = Me.m_filterVarName) Then Return
                Me.m_filterVarName = value
                Me.RefreshContent()
            End Set
        End Property

        Public Property OnlyShowConnected As Boolean
            Get
                Return Me.m_bOnlyShowConnected
            End Get
            Set(value As Boolean)
                If (value <> Me.m_bOnlyShowConnected) Then
                    Me.m_bOnlyShowConnected = value
                    Me.RefreshContent()
                End If
            End Set
        End Property

        Private ReadOnly Property ConnectionAtCell(p As Position) As cSpatialDataConnection
            Get
                Return Me.ConnectionAtCell(p.Row, p.Column)
            End Get
        End Property

        Private ReadOnly Property ConnectionAtCell(iRow As Integer, iCol As Integer) As cSpatialDataConnection
            Get
                Dim info As cConnectionInfo = Me.InfoAtRow(iRow)
                Dim conns As cSpatialDataConnection() = info.Adapter.Connections(info.Layer.Index)
                Dim conn As cSpatialDataConnection = Nothing
                Dim iConn As Integer = iCol - Me.m_nBaseCols

                If (iConn < 0) Or (iConn >= conns.Length) Then Return Nothing
                Return conns(iConn)
            End Get
        End Property

        Private Property InfoAtRow(iRow As Integer) As cConnectionInfo
            Get
                Return DirectCast(Me.Rows(iRow).Tag, cConnectionInfo)
            End Get
            Set(value As cConnectionInfo)
                Me.Rows(iRow).Tag = value
            End Set
        End Property

    End Class

End Namespace
