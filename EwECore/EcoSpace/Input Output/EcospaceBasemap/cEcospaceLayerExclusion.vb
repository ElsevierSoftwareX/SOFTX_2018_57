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

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Layer providing access to Ecospace excluded cells.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcospaceLayerExclusion
    Inherits cEcospaceLayerBoolean

    Public Sub New(ByVal theCore As cCore, ByVal manager As cEcospaceBasemap)
        MyBase.New(theCore, manager, My.Resources.CoreDefaults.CORE_DEFAULT_EXCLUSION, eVarNameFlags.LayerExclusion, 1)
        Me.m_dataType = eDataTypes.EcospaceLayerExclusion
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States if a given cell is an excluded cell.
    ''' </summary>
    ''' <param name="iRow">The row of the cell to check.</param>
    ''' <param name="iCol">The column of the cell to check.</param>
    ''' <returns>True if the given cell is an excluded cell.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsExcludedCell(ByVal iRow As Integer, ByVal iCol As Integer) As Boolean
        If Not Me.ValidateCellPosition(iRow, iCol) Then Return False
        Return (CBool(Me.Cell(iRow, iCol)) = True)
    End Function

End Class
