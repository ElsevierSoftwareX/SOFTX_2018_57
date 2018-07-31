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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports 

''' <summary>
''' Layer providing access to Ecospace vector data.
''' </summary>
Public Class cEcospaceLayerWind
    Inherits cEcospaceLayerSingle

#Region " Construction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for the wind layer.
    ''' </summary>
    ''' <param name="theCore"></param>
    ''' <param name="manager"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal theCore As cCore, ByVal manager As cEcospaceBasemap, ByVal iIndex As Integer)
        MyBase.New(theCore, manager, "", eVarNameFlags.LayerWind, iIndex)
        Me.m_dataType = eDataTypes.EcospaceLayerWind
        Me.m_ccSecundaryIndex = eCoreCounterTypes.nMonths
    End Sub

#End Region ' Construction

#Region " Overrides "

    Public Overrides Property Cell(iRow As Integer, iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object
        Get
            If (iIndexSec = cCore.NULL_VALUE) Then iIndexSec = Me.SecundaryIndex
            Return DirectCast(Me.Data, Single(,,))(iRow, iCol, iIndexSec)
        End Get
        Set(ByVal value As Object)
            Dim d As Single(,,) = DirectCast(Me.Data, Single(,,))
            Dim s As Single = Convert.ToSingle(value)
            If (iIndexSec = cCore.NULL_VALUE) Then iIndexSec = Me.SecundaryIndex
            d(iRow, iCol, iIndexSec) = s
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides Function DefaultName() As String
        Return cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_WIND,
                                     If(Me.Index = 1, My.Resources.CoreDefaults.CORE_DEFAULT_X_VELOCITY, My.Resources.CoreDefaults.CORE_DEFAULT_Y_VELOCITY))
    End Function

#End Region ' Overrides

End Class
