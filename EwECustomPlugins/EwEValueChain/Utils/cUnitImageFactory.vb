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

#Region " Options "

Option Strict On
Imports System.Drawing

#End Region ' Options

Public Class cUnitImageFactory

    Public Shared Function GetImage(ByVal unitType As cUnitFactory.eUnitType) As Image

        Select Case unitType
            Case cUnitFactory.eUnitType.Producer
                Return My.Resources.icons8_fishing_32
            Case cUnitFactory.eUnitType.Processing
                Return My.Resources.icons8_factory_32
            Case cUnitFactory.eUnitType.Distribution
                Return My.Resources.icons8_shipped_32
            Case cUnitFactory.eUnitType.Wholesaler
                Return My.Resources.icons8_depot_32
            Case cUnitFactory.eUnitType.Retailer
                Return My.Resources.icons8_shopping_cart_32
            Case cUnitFactory.eUnitType.Consumer
                Return My.Resources.icons8_meal_32
        End Select
        Return Nothing
    End Function

End Class
