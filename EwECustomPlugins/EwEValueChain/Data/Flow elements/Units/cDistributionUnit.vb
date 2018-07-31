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
Imports System.ComponentModel
Imports EwEUtils.Utilities

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' This class represents a group of distribution units in the Ecost economic model.
''' </summary>
''' ===========================================================================
<TypeConverter(GetType(cPropertySorter)), _
    DefaultProperty("Name"), _
    Serializable()> _
Public Class cDistributionUnit
    Inherits cEconomicUnit

#If 0 Then

    Public m_DistributorUnitCost() As Single     'by Distributor           c mak   d pri
    Public m_DistributorRawUnitCost(,) As Single 'by Distributor,sp        P raw   s
    Public m_DistributorRawAmountIn(,) As Single   'by Distributor,sp        x raw   d s
    'x raw d s   in = out?
    Public m_DistributorProcessedUnitcost(,) As Single 'by Distributor,    sp  p pro   sp
    Public m_DistributorProcessedAmount(,) As Single 'by Distributor, sp    x rpo   d, sp
    Public m_DistributorOtherUnitcost(,) As Single 'by Distributor, other   P i
    Public m_DistributorOtherAmount(,) As Single 'by Distributor, other     x d i
    Public m_DistributorLabourUnitCost() As Single 'by Distributor         omega d
    Public m_DistributorLabourAmount() As Single 'by Distributor           l d
    Public m_DistributorCapitalUnitCost() As Single 'by Distributor         gamma d
    Public m_DistributorCapitalAmount() As Single 'by Distributor           k d
    Public m_DistributorProductionRevenue() As Single 'by Distributor       y mak   d pri
    Public m_DistributorRawUnitPrice(,) As Single   'by Distributor, species   P mak,raw   s
    Public m_DistributorRawAmountOut(,) As Single      'by Distributor, species   x raw   d s
    'x raw d s   in = out?
    Public m_DistributorProcessUnitPrice(,) As Single 'by Distributor, species P mak,pro  sp
    Public m_DistributorProcessAmount(,) As Single  'by Distributor, species   x pro   d sp
    Public m_DistributorProductionBenefit() As Single 'by Distributor       b mak   d pri
    Public m_DistributorPublicUnitCost() As Single     'by distributor     c mak   d pub
    Public m_DistributorTaxRate() As Single            'by distributor?    tau
    Public m_DistributorManagementCost() As Single     'by distributor (total) MC mak  d
    Public m_DistributorActivity() As Single           'by distributor     E mak   d
    Public m_DistributorBenefit() As Single            'by distributor     b mak   d pub
    Public m_DistributorUnitSubsidy() As Single        'by distributor     sub d
    Public m_DistributorCostFinal() As Single          'by distributor     c mak   d
    Public m_DistributorBenefitFinal() As Single       'by distributor     b mak   d

#End If

    Public Sub New()
        MyBase.New()
    End Sub

#Region " Properties "

#Region " General "

    Public Overrides ReadOnly Property Category() As String
        Get
            Return "Distribution"
        End Get
    End Property

#End Region ' General

    <Browsable(False)> _
    Public Overrides ReadOnly Property UnitType() As cUnitFactory.eUnitType
        Get
            Return cUnitFactory.eUnitType.Distribution
        End Get
    End Property

#End Region ' Properties

End Class
