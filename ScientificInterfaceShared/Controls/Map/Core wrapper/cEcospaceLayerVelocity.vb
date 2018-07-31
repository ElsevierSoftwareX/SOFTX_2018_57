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

#End Region ' Imports

Namespace Controls.Map.Layers

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Base layer providing access to Ecospace data as cells, each representing a
    ''' vector with a X and Y component.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cEcospaceLayerVelocity
        Inherits cEcospaceLayer

#Region " Private variables "

        ''' <summary>Layer max velocity value.</summary>
        Protected m_sMaxValue As Single = 0.0!
        ''' <summary>Layer min velocity value.</summary>
        Protected m_sMinValue As Single = 0.0!
        ''' <summary>Layer num of cells with a value.</summary>
        Private m_iNumValueCells As Integer = 0

        Private m_lXVel As cEcospaceLayerSingle = Nothing
        Private m_lYVel As cEcospaceLayerSingle = Nothing

#End Region ' Private variables

#Region " Construction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor for an NxN layer of vectors that derives its data and identity 
        ''' from a manager.
        ''' </summary>
        ''' <param name="theCore"></param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal theCore As cCore,
                   ByVal strName As String,
                   ByVal manager As cEcospaceBasemap,
                   ByVal vn As eVarNameFlags)
            MyBase.New(theCore, Nothing, strName, GetType(Single), Nothing, vn)

            Me.m_lXVel = DirectCast(manager.Layers(vn)(0), cEcospaceLayerSingle)
            Me.m_lYVel = DirectCast(manager.Layers(vn)(1), cEcospaceLayerSingle)

            ' Facade
            Me.m_dataType = Me.m_lXVel.DataType
            Me.m_coreComponent = Me.m_lXVel.CoreComponent
            Me.m_ccSecundaryIndex = Me.m_lXVel.SecundaryIndexCounter
            Me.m_metadata = Me.m_lXVel.MetadataCell
            Me.m_manager = manager

        End Sub

#End Region ' Construction

#Region " Cell interaction "

        ' Aargh, this needs some serious refactoring

        Public Property Month As Integer
            Get
                If (Me.m_lXVel.SecundaryIndexCounter <> eCoreCounterTypes.nMonths) Then Return cCore.NULL_VALUE
                Return Me.m_lXVel.SecundaryIndex
            End Get
            Set(value As Integer)
                If (Me.m_lXVel.SecundaryIndexCounter <> eCoreCounterTypes.nMonths) Then Return
                Me.m_lXVel.SecundaryIndex = value
                Me.m_lYVel.SecundaryIndex = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Get/set a cell value in the form of Single(2), where index 0 represents
        ''' the X velocity, and index 1 represents the Y velocity of the value.
        ''' </summary>
        ''' <param name="iRow"></param>
        ''' <param name="iCol"></param>
        Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object
            Get
                Return New Single() {Me.XVelocity(iRow, iCol, iIndexSec), Me.YVelocity(iRow, iCol, iIndexSec)}
            End Get
            Set(ByVal value As Object)
                Dim asValues As Single() = DirectCast(value, Single())
                Me.XVelocity(iRow, iCol, iIndexSec) = asValues(0)
                Me.YVelocity(iRow, iCol, iIndexSec) = asValues(1)
            End Set
        End Property

        Public Property XVelocity(iRow As Integer, iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Single
            Get
                Return CSng(Me.m_lXVel.Cell(iRow, iCol, iIndexSec))
            End Get
            Set(value As Single)
                Me.m_lXVel.Cell(iRow, iCol, iIndexSec) = value
            End Set
        End Property

        Public Property YVelocity(iRow As Integer, iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Single
            Get
                Return CSng(Me.m_lYVel.Cell(iRow, iCol, iIndexSec))
            End Get
            Set(value As Single)
                Me.m_lYVel.Cell(iRow, iCol, iIndexSec) = value
            End Set
        End Property

        ''' <summary>
        ''' Get the max magnitude of all cells in the layer.
        ''' </summary>
        Public Overrides ReadOnly Property MaxValue() As Single
            Get
                If Me.m_bInvalidateStats Then Me.RecalcStats()
                Return Me.m_sMaxValue
            End Get
        End Property

        ''' <summary>
        ''' Get the min magnitude of all cells in the layer.
        ''' </summary>
        Public Overrides ReadOnly Property MinValue() As Single
            Get
                Return Me.m_sMinValue
            End Get
        End Property

        ''' <inheritdocs cref="cEcospaceLayer.NumValueCells"/>
        Public Overrides ReadOnly Property NumValueCells As Integer
            Get
                If Me.m_bInvalidateStats Then Me.RecalcStats()
                Return Me.m_iNumValueCells
            End Get
        End Property

        Public Overrides Sub Invalidate()
            Me.m_bInvalidateStats = True
        End Sub

        Public ReadOnly Property VelocityLayers() As cEcospaceLayerSingle()
            Get
                Return New cEcospaceLayerSingle() {Me.m_lXVel, Me.m_lYVel}
            End Get
        End Property

#End Region ' Cell interaction

#Region " Internals "

        Protected Overrides Function ValidateCellValue(value As Object) As Boolean
            Return True
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Calc max vector size in data layer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overridable Sub RecalcStats()

            Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
            Dim depth As cEcospaceLayerDepth = bm.LayerDepth
            Dim iRows As Integer = bm.InRow
            Dim iCols As Integer = bm.InCol

            Me.m_sMaxValue = 0
            Me.m_sMinValue = Single.MaxValue
            Me.m_iNumValueCells = 0

            For iRow As Integer = 1 To iRows
                For iCol As Integer = 1 To iCols
                    If depth.IsWaterCell(iRow, iCol) Then
                        Dim dx As Single = CSng(Me.m_lXVel.Cell(iRow, iCol))
                        Dim dy As Single = CSng(Me.m_lYVel.Cell(iRow, iCol))
                        If (dx <> cCore.NULL_VALUE And dy <> cCore.NULL_VALUE) Then
                            Me.m_sMaxValue = Math.Max(Me.m_sMaxValue, Math.Max(Math.Abs(dx), Math.Abs(dy)))
                            Me.m_sMinValue = Math.Min(Me.m_sMinValue, Math.Max(Math.Abs(dx), Math.Abs(dy)))
                            If (dx <> 0 And dy <> 0) Then
                                Me.m_iNumValueCells += 1
                            End If
                        End If
                    End If
                Next iCol
            Next iRow

            Me.m_bInvalidateStats = False

        End Sub

#End Region ' Internals

    End Class

End Namespace
