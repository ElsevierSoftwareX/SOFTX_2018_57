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

''' <summary>
''' Layer providing access to Ecospace port data.
''' </summary>
Public Class cEcospaceLayerPort
    Inherits cEcospaceLayerBoolean

    Public Sub New(ByVal theCore As cCore, ByVal manager As cEcospaceBasemap, iIndex As Integer)
        MyBase.New(theCore, manager, "", eVarNameFlags.LayerPort, iIndex)
        Me.m_dataType = eDataTypes.EcospaceLayerPort
    End Sub

#Region " Cell interaction "

    Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional ByVal iIndexSec As Integer = cCore.NULL_VALUE) As Object
        Get
            Dim data As Boolean()(,) = DirectCast(Me.Data, Boolean()(,))
            If (Me.Index = 0) Then
                For iFleet As Integer = 1 To Me.m_core.nFleets
                    If data(iFleet)(iRow, iCol) Then Return True
                Next
                Return False
            Else
                Return data(Me.Index)(iRow, iCol)
            End If
        End Get
        Set(ByVal value As Object)
            Dim data As Boolean()(,) = DirectCast(Me.Data, Boolean()(,))
            ' ToDo: only allow coastal cells to be set
            If (Me.Index = 0) Then
                For iFleet As Integer = 1 To Me.m_core.nFleets
                    data(iFleet)(iRow, iCol) = CBool(value)
                Next
            Else
                data(Me.Index)(iRow, iCol) = CBool(value)
            End If
        End Set
    End Property

#End Region ' Cell interaction

#Region " Overrides "

    Protected Overrides Function DefaultName() As String
        If (Me.Index = 0) Then Return My.Resources.CoreDefaults.CORE_ALL_FLEETS
        Return Me.m_core.EcopathFleetInputs(Me.Index).Name
    End Function

#End Region ' Overrides

End Class
