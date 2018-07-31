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
Imports EwECore.Core
Imports EwEUtils.Core

#End Region ' Imports

''' <summary>
''' Base layer providing access to Ecospace data as cells of integer values.
''' </summary>
Public Class cEcospaceLayerInteger
    Inherits cEcospaceLayer

#Region " Private variables "

    ''' <summary>Layer max value.</summary>
    Protected m_iMaxValue As Integer = 0
    ''' <summary>Layer min value.</summary>
    Protected m_iMinValue As Integer = 0
    ''' <summary>Layer num of cells with a value.</summary>
    Protected m_iNumValueCells As Integer = 0

#End Region ' Private variables

#Region " Construction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for a NxN layer of integer values that derives its data and 
    ''' identity from a manager, but that is a unique data entity in the EwE core.
    ''' </summary>
    ''' <param name="core"></param>
    ''' <param name="manager"></param>
    ''' <param name="varName"></param>
    ''' <param name="iIndex"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore, _
                   ByVal manager As IEcospaceLayerManager, _
                   ByVal strName As String, _
                   ByVal varName As eVarNameFlags, _
                   Optional ByVal iIndex As Integer = cCore.NULL_VALUE)

        MyBase.New(core, core.m_EcoSpaceData.getLayerID(varName, iIndex), manager, strName, varName, iIndex, GetType(Integer))

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for a NxN layer that is hard-linked to an array of data.
    ''' </summary>
    ''' <param name="theCore"></param>
    ''' <param name="data"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal theCore As cCore, _
                   ByVal data As Integer(,), _
                   ByVal strName As String, _
                   Optional ByVal meta As cVariableMetaData = Nothing, _
                   Optional ByVal vn As eVarNameFlags = eVarNameFlags.NotSet)

        MyBase.New(theCore, CObj(data), strName, GetType(Integer), meta, vn)

    End Sub

#End Region ' Construction

#Region " Cell interaction "

    ''' <inheritdocs cref="cEcospaceLayer.Cell"/>
    Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object
        Get
            Return DirectCast(Me.Data, Integer(,))(iRow, iCol)
        End Get
        Set(ByVal value As Object)
            Dim d As Integer(,) = DirectCast(Me.Data, Integer(,))
            Dim i As Integer = CInt(value)
            If Me.ValidateCellValue(i) Then
                If Me.ValidateCellPosition(iRow, iCol) Then
                    d(iRow, iCol) = i
                    If (Me.m_bInvalidateStats = False) Then
                        Me.m_bInvalidateStats = (i < Me.m_iMinValue) Or (i > Me.m_iMaxValue)
                    End If
                End If
            End If
        End Set
    End Property

    ''' <inheritdocs cref="cEcospaceLayer.MaxValue"/>
    Public Overrides ReadOnly Property MaxValue() As Single
        Get
            If Me.m_bInvalidateStats Then Me.RecalcStats()
            Return Me.m_iMaxValue
        End Get
    End Property

    ''' <inheritdocs cref="cEcospaceLayer.MinValue"/>
    Public Overrides ReadOnly Property MinValue() As Single
        Get
            If Me.m_bInvalidateStats Then Me.RecalcStats()
            Return Me.m_iMinValue
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
        ' Set invalidated flag
        Me.m_bInvalidateStats = True
    End Sub

#End Region ' Cell interaction

#Region " Internals "

    Protected Overrides Function ValidateCellValue(ByVal value As Object) As Boolean

        If (Convert.IsDBNull(value)) Then Return False
        Dim iValue As Integer = CInt(value)
        If (iValue = cCore.NULL_VALUE) Then Return False

        Dim md As cVariableMetaData = Me.MetadataCell
        If (md Is Nothing) Then Return True
        If (md.MinOperator Is Nothing) Or (md.MaxOperator Is Nothing) Then Return True

        Return md.MinOperator.Compare(iValue, md.Min) And
               md.MaxOperator.Compare(iValue, md.Max)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Recalculate layer statistics
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overridable Sub RecalcStats()

        Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
        Dim iRows As Integer = bm.InRow
        Dim iCols As Integer = bm.InCol

        Me.m_iMaxValue = Integer.MinValue
        Me.m_iMinValue = Integer.MaxValue
        Me.m_iNumValueCells = 0

        For iRow As Integer = 1 To iRows
            For iCol As Integer = 1 To iCols
                If (bm.IsModelledCell(iRow, iCol)) Then
                    Dim i As Integer = CInt(Me.Cell(iRow, iCol))
                    If i <> cCore.NULL_VALUE Then
                        Me.m_iMaxValue = Math.Max(i, Me.m_iMaxValue)
                        Me.m_iMinValue = Math.Min(i, Me.m_iMinValue)
                        Me.m_iNumValueCells += 1
                    End If
                End If
            Next iCol
        Next iRow

        If (Me.m_iMaxValue = Me.m_iMinValue) Then
            Me.m_iMinValue = 0
        End If

        Me.m_bInvalidateStats = False

    End Sub

#End Region ' Internals

End Class