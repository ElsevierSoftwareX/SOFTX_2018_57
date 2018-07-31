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
Imports ScientificInterface.Ecospace
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports SourceGrid2
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared

#End Region ' Imports

<CLSCompliant(False)>
Public Class gridLayerData
    Inherits EwEGrid

    Private Enum eDataModes As Integer
        Value = 0
        ValueX = 1
        ValueY = 2
    End Enum

    Private m_basemap As cEcospaceBasemap = Nothing
    Private m_layer As cDisplayLayerRaster = Nothing
    Private m_iVectorField As Integer = 0
    Private m_bReadOnly As Boolean = False
    Private m_bInvalid As Boolean = False

    Public Sub New()
        MyBase.New()
        Me.TrackPropertySelection = False
        Me.IsOutputGrid = False
    End Sub

    Public Overrides Property UIContext() As ScientificInterfaceShared.Controls.cUIContext
        Get
            Return MyBase.UIContext
        End Get
        Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
            If (Me.UIContext IsNot Nothing) Then
                Me.Layer = Nothing
                Me.m_basemap = Nothing
            End If
            MyBase.UIContext = value
            If (Me.UIContext IsNot Nothing) Then
                Me.m_basemap = value.Core.EcospaceBasemap
            End If
        End Set
    End Property

    Protected Overrides Sub InitLayout()

        If (Me.UIContext Is Nothing) Then Return
        If (Me.m_layer Is Nothing) Then Return

        Me.Redim(Me.m_basemap.InRow + 1, Me.m_basemap.InCol + 1)

        Me.FixedColumns = 1
        Me.FixedColumnWidths = False

        MyBase.InitLayout()
    End Sub

    Protected Overrides Sub InitStyle()

        Dim data As cEcospaceLayer = Nothing

        MyBase.InitStyle()

        ' Test for UI context to prevent core from being accessed
        If (Me.UIContext Is Nothing) Then Return
        If (Me.m_layer Is Nothing) Then Return

        ' Grab the data
        data = Me.m_layer.Data

        Me.Redim(1, Me.m_basemap.InCol + 1)
        Me(0, 0) = New EwEColumnHeaderCell("")
        For iCol As Integer = 1 To Me.m_basemap.InCol
            Me(0, iCol) = New EwEColumnHeaderCell(CStr(iCol))
        Next

        Me.FixedColumns = 1

    End Sub

    Protected Overrides Sub FillData()

        If (Me.UIContext Is Nothing) Then Return
        If (Me.m_layer Is Nothing) Then Return

        Dim cell As Cells.ICell = Nothing
        Dim tCell As Type = Nothing
        Dim data As cEcospaceLayer = Nothing
        Dim dataV As cEcospaceLayerVelocity = Nothing
        Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
        Dim value As Object = Nothing

        If (Me.m_bReadOnly) Then
            style = cStyleGuide.eStyleFlags.NotEditable
        End If
        Dim bChanged As Boolean = False


        ' Grab the data
        data = Me.m_layer.Data
        tCell = data.ValueType

        If (Me.DataMode > 0) Then
            dataV = DirectCast(data, cEcospaceLayerVelocity)
        End If

        Me.SuspendLayoutGrid()

        ' Prepare grid
        Me.RowsCount = 1

        ' Create cells
        For iRow As Integer = 1 To Me.m_basemap.InRow
            ' Add row
            Me.AddRow()
            ' Add row header cell
            Me(iRow, 0) = New EwERowHeaderCell(CStr(iRow))
            ' Add row value cells
            For iCol As Integer = 1 To Me.m_basemap.InCol
                ' Prepare cell

                Select Case Me.DataMode
                    Case eDataModes.Value
                        value = data.Cell(iRow, iCol)
                    Case eDataModes.ValueX
                        value = dataV.XVelocity(iRow, iCol)
                    Case eDataModes.ValueY
                        value = dataV.YVelocity(iRow, iCol)
                    Case Else
                        Debug.Assert(False)
                End Select

                cell = New EwECell(value, tCell, style)
                If (Not Me.m_bReadOnly) Then cell.Behaviors.Add(Me.EwEEditHandler)

                Me(iRow, iCol) = cell
            Next iCol
        Next iRow

        Me.ResumeLayoutGrid()

    End Sub

    Private Property DataMode As eDataModes = eDataModes.Value

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the layer to display in the grid
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Layer() As cDisplayLayerRaster
        Get
            Return Me.m_layer
        End Get
        Set(ByVal value As cDisplayLayerRaster)

            'If Me.m_layer IsNot Nothing Then
            '    RemoveHandler Me.m_layer.LayerChanged, AddressOf OnLayerChanged
            'End If

            If Not ReferenceEquals(Me.m_layer, value) Then

                Me.m_layer = value
                Me.m_bReadOnly = True
                Me.DataMode = eDataModes.Value

                If (Me.m_layer IsNot Nothing) Then

                    ' Assess layer
                    If (TypeOf Me.m_layer.Data Is cEcospaceLayerVelocity) Then
                        Me.DataMode = eDataModes.ValueX
                    End If

                    If (Me.m_layer.Editor IsNot Nothing) Then
                        Me.m_bReadOnly = Not Me.Layer.Editor.IsEditable()
                    End If
                End If
            End If

            'If Me.m_layer IsNot Nothing Then
            '    AddHandler Me.m_layer.LayerChanged, AddressOf OnLayerChanged
            'End If

        End Set
    End Property

    Public Property VectorFieldIndex As Integer
        Get
            Return Me.m_iVectorField
        End Get
        Set(value As Integer)
            Me.m_iVectorField = value
            If (Me.DataMode > 0) Then
                Me.DataMode = CType(1 + Me.m_iVectorField, eDataModes)
            End If
        End Set
    End Property

    Protected Overrides Function OnCellValueChanged(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean
        Me.InvalidateLayer()
        Return MyBase.OnCellValueChanged(p, cell)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Apply the grid data
    ''' </summary>
    ''' <param name="layTarget"></param>
    ''' <returns>True when the layer data was changed.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Apply(Optional ByVal layTarget As cDisplayLayerRaster = Nothing) As Boolean

        Dim p As SourceGrid2.Position = Nothing
        Dim sNew As Single = 0.0!
        Dim sOrg As Single = 0.0!
        Dim data As cEcospaceLayer = Nothing
        Dim dataV As cEcospaceLayerVelocity = Nothing
        Dim bChanged As Boolean = False

        If Me.m_layer.Editor IsNot Nothing Then
            If (Me.m_layer.Editor.IsReadOnly() = True) Then
                Return False
            End If
        End If

        If (layTarget Is Nothing) Then layTarget = Me.m_layer
        If (layTarget Is Nothing) Then Return False

        data = layTarget.Data
        If (Me.DataMode > 0) Then
            dataV = DirectCast(data, cEcospaceLayerVelocity)
        End If

        For iRow As Integer = 1 To Me.m_basemap.InRow
            For iCol As Integer = 1 To Me.m_basemap.InCol

                ' Get original value from layer
                Select Case Me.DataMode
                    Case eDataModes.Value
                        sOrg = CSng(data.Cell(iRow, iCol))
                    Case eDataModes.ValueX
                        sOrg = dataV.XVelocity(iRow, iCol)
                    Case eDataModes.ValueY
                        sOrg = dataV.YVelocity(iRow, iCol)
                End Select

                ' Get grid value
                p = New SourceGrid2.Position(iRow, iCol)
                sNew = CSng(Me(iRow, iCol).GetValue(p))

                ' Has the user modified this value?
                If (sNew <> sOrg) Then
                    ' #Yes: set it
                    Select Case Me.DataMode
                        Case eDataModes.Value
                            data.Cell(iRow, iCol) = sNew
                        Case eDataModes.ValueX
                            dataV.XVelocity(iRow, iCol) = sNew
                        Case eDataModes.ValueY
                            dataV.YVelocity(iRow, iCol) = sNew
                        Case Else
                            Debug.Assert(False)
                    End Select

                    ' Remember the change
                    bChanged = True
                End If
            Next iCol
        Next iRow

        Return bChanged

    End Function

    Private Sub InvalidateLayer()
        Me.m_bInvalid = True
        Me.BeginInvoke(New MethodInvoker(AddressOf Me.ApplyChanges))
    End Sub

    Private Sub ApplyChanges()
        If Me.m_bInvalid Then
            Me.m_bInvalid = False
            Me.Apply()
            Me.m_layer.Update(cDisplayLayer.eChangeFlags.Map)
        End If
    End Sub

End Class
