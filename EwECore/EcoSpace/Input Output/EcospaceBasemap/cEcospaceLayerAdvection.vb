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
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports 

''' <summary>
''' Layer providing access to Ecospace advection data.
''' </summary>
Public Class cEcospaceLayerAdvection
    Inherits cEcospaceLayerSingle

#Region " Construction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for the advection layer.
    ''' </summary>
    ''' <param name="theCore"></param>
    ''' <param name="manager"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal theCore As cCore, ByVal manager As cEcospaceBasemap, ByVal iIndex As Integer)

        MyBase.New(theCore, manager, "", eVarNameFlags.LayerAdvection, iIndex)
        Me.m_dataType = eDataTypes.EcospaceLayerAdvection
        Me.m_ccSecundaryIndex = eCoreCounterTypes.nMonths

    End Sub

#End Region ' Construction

#Region " Overrides "

    Public Overrides Property Cell(iRow As Integer, iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object
        Get
            Dim data As Single()(,) = DirectCast(Me.Data, Single()(,))
            If (iIndexSec = cCore.NULL_VALUE) Then iIndexSec = Me.SecundaryIndex
            Return data(iIndexSec)(iRow, iCol)
        End Get
        Set(ByVal value As Object)
            Dim d As Single()(,) = DirectCast(Me.Data, Single()(,))
            Dim s As Single = Convert.ToSingle(value)
            If (iIndexSec = cCore.NULL_VALUE) Then iIndexSec = Me.SecundaryIndex
            d(iIndexSec)(iRow, iCol) = s
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides Function DefaultName() As String
        Return cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_ADVECTION,
                                     If(Me.Index = 1, My.Resources.CoreDefaults.CORE_DEFAULT_X_VELOCITY, My.Resources.CoreDefaults.CORE_DEFAULT_Y_VELOCITY))
    End Function

    Protected Overrides Sub RecalcStats()

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
                    Dim dx As Single = CSng(Me.Cell(iRow, iCol, 0))
                    Dim dy As Single = CSng(Me.Cell(iRow, iCol, 0))
                    If (dx <> cCore.NULL_VALUE And dy <> cCore.NULL_VALUE) Then
                        s = CSng(Math.Sqrt(dx * dx + dy * dy))
                        Me.m_sMaxValue = Math.Max(s, Me.m_sMaxValue)
                        Me.m_sMinValue = Math.Min(s, Me.m_sMinValue)
                        Me.m_iNumValueCells += 1
                        sTot += s
                    End If
                End If
            Next iCol
        Next iRow

        If (m_iNumValueCells > 0) Then
            Me.m_sMeanValue = sTot / Me.m_iNumValueCells
        Else
            Me.m_sMeanValue = cCore.NULL_VALUE
        End If

        Me.m_bInvalidateStats = False

    End Sub

#End Region ' Overrides

End Class
