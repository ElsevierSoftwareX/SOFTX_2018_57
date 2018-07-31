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

#Region " Imports  "

Option Strict On
Imports EwEUtils.Core

#End Region ' Imports 

''' <summary>
''' Layer providing access to Ecospace migration data.
''' </summary>
Public Class cEcospaceLayerMigration
    Inherits cEcospaceLayerSingle

#Region " Construction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for an NxN layer that derives its data and identity from 
    ''' a manager.
    ''' </summary>
    ''' <param name="theCore"></param>
    ''' <param name="manager"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByRef theCore As cCore, ByVal manager As cEcospaceBasemap, iIndex As Integer)
        MyBase.New(theCore, manager, "", eVarNameFlags.LayerMigration, iIndex)
        Me.m_dataType = eDataTypes.EcospaceLayerMigration
        Me.m_ccSecundaryIndex = eCoreCounterTypes.nMonths
    End Sub

#End Region ' Construction

#Region " Cell interaction "

    Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer,
                                   Optional ByVal iIndexSec As Integer = cCore.NULL_VALUE) As Object
        Get
            If (iIndexSec = cCore.NULL_VALUE) Then iIndexSec = Me.SecundaryIndex
            Return DirectCast(Me.Data, Single(,)(,))(Me.Index, iIndexSec)(iRow, iCol)
        End Get
        Set(ByVal value As Object)
            If (iIndexSec = cCore.NULL_VALUE) Then iIndexSec = Me.SecundaryIndex
            DirectCast(Me.Data, Single(,)(,))(Me.Index, iIndexSec)(iRow, iCol) = CSng(value)
        End Set
    End Property

#End Region ' Cell interaction

#Region " Overrides "

    Protected Overrides Function DefaultName() As String
        Return Me.m_core.EcoPathGroupInputs(Me.Index).Name
    End Function

#End Region ' Overrides

End Class
