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
''' Base layer providing access to Ecospace data as cells of single values.
''' </summary>
Public Class cEcospaceLayerSingle
    Inherits cEcospaceLayer

#Region " Private variables "

    ''' <summary>Layer max value.</summary>
    Protected m_sMaxValue As Single = 0.0!
    ''' <summary>Layer min value.</summary>
    Protected m_sMinValue As Single = 0.0!
    ''' <summary>Layer mean value.</summary>
    Protected m_sMeanValue As Single = 0.0!
    ''' <summary>Layer num of cells with a value.</summary>
    Protected m_iNumValueCells As Integer = 0

#End Region ' Private variables

#Region " Construction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for an NxN layer of Single values, that derives its data and 
    ''' identity from a manager.
    ''' </summary>
    ''' <param name="core"></param>
    ''' <param name="manager"></param>
    ''' <param name="varName"></param>
    ''' <param name="iIndex"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore,
                   ByVal manager As IEcospaceLayerManager,
                   ByVal strName As String,
                   ByVal varName As eVarNameFlags,
                   Optional ByVal iIndex As Integer = cCore.NULL_VALUE)
        MyBase.New(core, core.m_EcoSpaceData.getLayerID(varName, iIndex), manager, strName, varName, iIndex, GetType(Single))
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for a NxN layer that is hard-linked to an array of data.
    ''' </summary>
    ''' <param name="core"></param>
    ''' <param name="data"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore,
                   ByVal data As Single(,),
                   ByVal strName As String,
                   Optional ByVal meta As cVariableMetaData = Nothing,
                   Optional ByVal vn As eVarNameFlags = eVarNameFlags.NotSet)

        MyBase.New(core, CObj(data), strName, GetType(Single), meta, vn)

    End Sub

#End Region ' Construction

#Region " Cell interaction "

    ''' <inheritdocs cref="cEcospaceLayer.Cell"/>
    Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object
        Get
            Return DirectCast(Me.Data, Single(,))(iRow, iCol)
        End Get
        Set(ByVal value As Object)
            Dim d As Single(,) = DirectCast(Me.Data, Single(,))
            Dim s As Single = Convert.ToSingle(value)
            If Me.ValidateCellValue(value) Then
                If Me.ValidateCellPosition(iRow, iCol) Then
                    d(iRow, iCol) = s
                    If (Me.m_bInvalidateStats = False) Then
                        Me.m_bInvalidateStats = (s < Me.m_sMinValue) Or (s > Me.m_sMaxValue)
                    End If
                End If
            End If
        End Set
    End Property

    ''' <inheritdocs cref="cEcospaceLayer.MaxValue"/>
    Public Overrides ReadOnly Property MaxValue() As Single
        Get
            If Me.m_bInvalidateStats Then Me.RecalcStats()
            Return Me.m_sMaxValue
        End Get
    End Property

    ''' <inheritdocs cref="cEcospaceLayer.MinValue"/>
    Public Overrides ReadOnly Property MinValue() As Single
        Get
            If Me.m_bInvalidateStats Then Me.RecalcStats()
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

    Public Overridable ReadOnly Property MeanValue As Single
        Get
            If Me.m_bInvalidateStats Then Me.RecalcStats()
            Return Me.m_sMeanValue
        End Get
    End Property

    Public Overrides Sub Invalidate()
        Me.m_bInvalidateStats = True
    End Sub

#End Region ' Cell interaction

#Region " Internals "

    Protected Overrides Function ValidateCellValue(ByVal value As Object) As Boolean

        If (Convert.IsDBNull(value)) Then Return False
        Dim sValue As Single = Convert.ToSingle(value)
        If (sValue = cCore.NULL_VALUE) Then Return False

        Dim md As cVariableMetaData = Me.MetadataCell
        If (md Is Nothing) Then Return True
        If (md.MinOperator Is Nothing) Or (md.MaxOperator Is Nothing) Then Return True

        Return md.MinOperator.Compare(sValue, md.Min) And
               md.MaxOperator.Compare(sValue, md.Max)

    End Function

    Protected Overridable Sub RecalcStats()

        Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
        Dim s As Single = 0.0!
        Dim sTot As Single = 0
        Dim iRows As Integer = bm.InRow
        Dim iCols As Integer = bm.InCol

        Me.m_sMaxValue = Single.MinValue
        Me.m_sMinValue = Single.MaxValue
        Me.m_iNumValueCells = 0

        For iRow As Integer = 1 To iRows
            For iCol As Integer = 1 To iCols
                If (bm.IsModelledCell(iRow, iCol)) Then
                    s = CSng(Me.Cell(iRow, iCol))
                    If (s <> cCore.NULL_VALUE) Then
                        Me.m_sMaxValue = Math.Max(s, Me.m_sMaxValue)
                        Me.m_sMinValue = Math.Min(s, Me.m_sMinValue)
                        Me.m_iNumValueCells += 1
                        sTot += s
                    End If
                End If
            Next iCol
        Next iRow

        'If (Me.m_sMaxValue = Me.m_sMinValue) Then
        '    Me.m_sMinValue = 0
        'End If

        If (m_iNumValueCells > 0) Then
            Me.m_sMeanValue = sTot / Me.m_iNumValueCells
        Else
            Me.m_sMeanValue = cCore.NULL_VALUE
        End If

        Me.m_bInvalidateStats = False

    End Sub

#End Region ' Internals

End Class
