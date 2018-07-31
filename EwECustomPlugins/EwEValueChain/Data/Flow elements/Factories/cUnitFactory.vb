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

#End Region ' Imports

Public Class cUnitFactory

    Public Enum eUnitType As Integer
        All = 0
        Producer
        Processing
        Distribution
        Wholesaler
        Retailer
        Consumer
    End Enum

    Public Shared Function CreateUnit(ByVal tClass As Type) As cUnit
        If Not GetType(cUnit).IsAssignableFrom(tClass) Then Return Nothing
        Return CType(System.Activator.CreateInstance(tClass), cUnit)
    End Function

    Public Shared Function CreateUnit(ByVal unitType As eUnitType) As cUnit
        Return CreateUnit(MapType(unitType))
    End Function

    Public Shared Function MapType(ByVal unitType As eUnitType) As Type
        Dim t As Type = Nothing
        Select Case unitType
            Case eUnitType.Producer : t = GetType(cProducerUnit)
            Case eUnitType.Processing : t = GetType(cProcessingUnit)
            Case eUnitType.Distribution : t = GetType(cDistributionUnit)
            Case eUnitType.Wholesaler : t = GetType(cWholesalerUnit)
            Case eUnitType.Retailer : t = GetType(cRetailerUnit)
            Case eUnitType.Consumer : t = GetType(cConsumerUnit)
        End Select
        Return t
    End Function

    Public Shared Function CreateUnitDefault(ByVal unitType As eUnitType) As cUnit
        Dim t As Type = Nothing
        Select Case unitType
            Case eUnitType.Producer : t = GetType(cProducerUnitDefault)
            Case eUnitType.Processing : t = GetType(cProcessingUnitDefault)
            Case eUnitType.Distribution : t = GetType(cDistributionUnitDefault)
            Case eUnitType.Wholesaler : t = GetType(cWholesalerUnitDefault)
            Case eUnitType.Retailer : t = GetType(cRetailerUnitDefault)
            Case eUnitType.Consumer : t = GetType(cConsumerUnitDefault)
        End Select
        Return CType(System.Activator.CreateInstance(t), cUnit)
    End Function

End Class
