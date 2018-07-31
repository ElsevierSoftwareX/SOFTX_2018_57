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
Imports EwEUtils.Utilities

#End Region ' Imports

''' <summary>
''' Layer providing access to Ecospace habitat data.
''' </summary>
Public Class cEcospaceLayerHabitat
    Inherits cEcospaceLayerSingle

    Public Sub New(ByVal theCore As cCore, ByVal manager As cEcospaceBasemap, ByVal iIndex As Integer)
        MyBase.New(theCore, manager, "", eVarNameFlags.LayerHabitat, iIndex)
        Me.m_dataType = eDataTypes.EcospaceLayerHabitat
    End Sub

    Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object
        Get
            Dim data As Single()(,) = DirectCast(Me.Data, Single()(,))
            If Me.ValidateCellPosition(iRow, iCol) Then Return data(Me.Index)(iRow, iCol) Else Return cCore.NULL_VALUE
        End Get
        Set(ByVal value As Object)
            Dim data As Single()(,) = DirectCast(Me.Data, Single()(,))
            Dim s As Single = Convert.ToSingle(value)
            If Me.ValidateCellValue(value) Then
                If Me.ValidateCellPosition(iRow, iCol) Then
                    data(Me.Index)(iRow, iCol) = s
                    Me.Invalidate()
                End If
            End If
        End Set
    End Property

    Protected Overrides Function DefaultName() As String
        If (Me.Index = 0) Then Return My.Resources.CoreDefaults.CORE_DEFAULT_HABITAT_ALL
        Return cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_HABITAT, Me.m_core.EcospaceHabitats(Me.Index).Name)
    End Function

End Class
