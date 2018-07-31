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
Imports DefaultRes = EwECore.My.Resources.CoreDefaults

Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities


#End Region ' Imports

''' <summary>
''' Layer providing access to Ecospace habitat capacity data.
''' </summary>
Public Class cEcospaceLayerHabitatCapacity
    Inherits cEcospaceLayerSingle

    Public Sub New(ByVal theCore As cCore, ByVal manager As cEcospaceBasemap, ByVal dt As eDataTypes, ByVal vn As eVarNameFlags, iIndex As Integer)
        MyBase.New(theCore, manager, "", vn, iIndex)
        Me.m_dataType = dt
    End Sub

#Region " Cell interaction "

    Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object
        Get
            Dim data As Single()(,) = DirectCast(Me.Data, Single()(,))
            If Me.ValidateCellPosition(iRow, iCol) Then Return data(Me.Index)(iRow, iCol)
            Return 0
        End Get
        Set(ByVal value As Object)
            Dim data As Single()(,) = DirectCast(Me.Data, Single()(,))
            If Me.ValidateCellPosition(iRow, iCol) Then data(Me.Index)(iRow, iCol) = CSng(value)
        End Set
    End Property

#End Region ' Cell interaction

    Protected Overrides Function DefaultName() As String
        Return Me.m_core.EcoPathGroupInputs(Me.Index).Name
    End Function

End Class
