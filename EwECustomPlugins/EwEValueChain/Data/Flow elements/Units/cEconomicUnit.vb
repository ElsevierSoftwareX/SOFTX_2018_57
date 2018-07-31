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

<TypeConverter(GetType(cPropertySorter)), _
    DefaultProperty("Name"), _
    Serializable()> _
Public MustInherit Class cEconomicUnit
    Inherits cUnit

#Region " Private variables "

    Private m_WorkerFemale As Single = 0.0!
    Private m_WorkerMale As Single = 0.0!
    Private m_WorkerOther As Single = 0.0!
    Private m_WorkerMalePay As Single = 0.0!
    Private m_WorkerFemalePay As Single = 0.0!
    Private m_WorkerOtherPay As Single = 0.0!
    Private m_WorkerMaleShare As Single = 0.0!
    Private m_WorkerFemaleShare As Single = 0.0!
    Private m_WorkerOtherShare As Single = 0.0!
    Private m_WorkerMaleDependents As Single = 0.0!
    Private m_WorkerFemaleDependents As Single = 0.0!
    Private m_WorkerParttime As Single = 0.0!
    Private m_OwnerMale As Single = 0.0!
    Private m_OwnerFemale As Single = 0.0!
    Private m_OwnerMalePay As Single = 0.0!
    Private m_OwnerFemalePay As Single = 0.0!
    Private m_OwnerMaleShare As Single = 0.0!
    Private m_OwnerFemaleShare As Single = 0.0!
    Private m_OwnerMaleDependents As Single = 0
    Private m_OwnerFemaleDependents As Single = 0
    Private m_EnergyProducts As Single = 0
    Private m_IndustrialProducts As Single = 0
    Private m_ServiceProducts As Single = 0
    Private m_EnergyCost As Single = 0
    Private m_CapitalCost As Single = 0
    Private m_IndustrialCost As Single = 0
    Private m_ServiceCost As Single = 0
    Private m_ManagementCost As Single = 0
    Private m_RoyaltyCost As Single = 0
    Private m_CertificationCost As Single = 0
    Private m_TaxesLicense As Single = 0
    Private m_TaxesProfit As Single = 0
    Private m_TaxesVAT As Single = 0
    Private m_TaxesImport As Single = 0
    Private m_TaxesExport As Single = 0
    Private m_TaxesEnvironmental As Single = 0
    Private m_TaxesProduction As Single = 0
    Private m_SubsidyEnergy As Single = 0
    Private m_SubsidyOther As Single = 0

    'Public Amount As Single         'Amount in tonnes 
    'Public Benefit As Single        '
    'Public CapitalAmount As Single  'number of Capital units    VC DON'T THINK THIS IS NEEDED, ONLY PER TONNES
    'Public CapitalCost As Single    'per tons
    'Public EmployeesFemale As Single    'per tons
    'Public EmployeesMale As Single      'per tons
    'Public EmployersFemale As Single    'per tons
    'Public EmployersMale As Single      'per tons
    'Public LabourAmount As Single       'number of labour units per tons
    'Public LabourCost As Single         'per tons
    'Public ManagementCost As Single 'per unit produced?
    'Public ProductionUnits As Single     'Number of units per tons of product (boats, processors, distributors, etc)
    'Public RawAmount As Single      'Unit cost for buying a tonnes of raw material
    'Public RawCost As Single        'Unit cost for buying a tonnes of raw material
    'Public Price As Single          'Price for each product per tons
    'Public Revenue As Single        'Total revenue 
    'Public Subsidy As Single        'per tons produced
    'Public TaxEnvironmentalRate As Single   'per tonnes produced
    'Public TaxProductionRate As Single      'per tonnes produced
    'Public ProcessingCost As Single       'Cost for processing one tons (is in addition to raw cost)
    'Public Value As Single          'Value of production
    'Public WageFemale As Single     '$ per year
    'Public WageMale As Single       '$ per year

    Private m_bBroker As Boolean = False

#End Region ' Private variables

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub New()
        MyBase.New()
    End Sub

#End Region ' Constructor

#Region " Calculations "

    Protected Overrides Function Calculate(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        'The production unit needs to do the same calculations as the MyBase=cEconomicUnit, but:
        Dim bSucces As Boolean = MyBase.Calculate(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)

        'Production in weight
        results.Store(Me, cResults.eVariableType.Production, sOutputBiomass, iTimeStep)

        bSucces = bSucces And Me.CalcProductionLiveWeight(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)

        'Revenue
        bSucces = bSucces And Me.CalcProducts(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcSubsidy(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)

        'Cost
        bSucces = bSucces And Me.CalcRawmaterialCost(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcInputCost(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcManagementRoyaltyCertificationCost(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)

        bSucces = bSucces And Me.CalcTax(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcWorkerPay(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcOwnerPay(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)

        'Social
        bSucces = bSucces And Me.CalcWorkerFemales(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcWorkerMales(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcWorkerParttime(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcWorkerOther(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcOwnerFemales(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcOwnerMales(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcWorkerDependents(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        bSucces = bSucces And Me.CalcOwnerDependents(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)

        Return bSucces

    End Function

#Region " Production (weight)"

#End Region

    Protected Overridable Function CalcProductionLiveWeight(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim ToBeCalculated As Single = 0
        results.Store(Me, cResults.eVariableType.ProductionLive, ToBeCalculated, iTimeStep)

        Return True
    End Function

#Region " Revenue "

    Protected Overridable Function CalcProducts(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * (Me.EnergyProducts + Me.IndustrialProducts + Me.ServiceProducts)

        results.Store(Me, cResults.eVariableType.RevenueProductsOther, sSum, iTimeStep)
        If Me.Broker = False Then
            results.Store(Me, cResults.eVariableType.RevenueProductsMain, sOutputValue, iTimeStep)
        End If
        Return True
    End Function

    Protected Overridable Function CalcSubsidy(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * (Me.SubsidyEnergy + Me.SubsidyOther)
        results.Store(Me, cResults.eVariableType.RevenueSubsidies, sSum, iTimeStep)
        Return True
    End Function

#End Region ' Revenue

#Region " Cost "

    Protected Overridable Function CalcRawmaterialCost(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        If Me.Broker = False Then
            'Dim sSum As Single = sInputBiomass * sInputValue
            results.Store(Me, cResults.eVariableType.CostRawmaterial, sInputValue, iTimeStep)
        End If
        Return True
    End Function

    Protected Overridable Function CalcInputCost(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * (Me.CapitalInput + Me.EnergyCost + Me.IndustrialCost + Me.ServiceCost)
        results.Store(Me, cResults.eVariableType.CostInput, sSum, iTimeStep)
        Return True
    End Function

    Protected Overridable Function CalcManagementRoyaltyCertificationCost(ByVal results As cResults, _
               ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
               ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
               ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * (m_ManagementCost + m_RoyaltyCost + m_CertificationCost)
        results.Store(Me, cResults.eVariableType.CostManagementRoyaltyCertification, sSum, iTimeStep)
        Return True
    End Function



    Protected Overridable Function CalcTax(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * (Me.TaxEnvironmental + Me.TaxExport + Me.TaxProduction + Me.TaxVAT + Me.m_TaxesImport + Me.LicenseTax)
        ' profit tax is calculated later, after all revenue and (other) cost is known (VC111117)
        results.Store(Me, cResults.eVariableType.CostTaxes, sSum, iTimeStep)
        Return True
    End Function

    Protected Overridable Function CalcWorkerPay(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single
        If (Me.m_WorkerMalePay + Me.m_WorkerFemalePay) > 0 Then
            sSum = sOutputBiomass * (Me.m_WorkerMalePay + Me.m_WorkerFemalePay)
        Else
            sSum = sOutputValue * (Me.m_WorkerMaleShare + Me.m_WorkerFemaleShare) / 100
        End If
        results.Store(Me, cResults.eVariableType.CostWorker, sSum, iTimeStep)
        Return True
    End Function

    Protected Overridable Function CalcOwnerPay(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single
        If (Me.m_OwnerMalePay + Me.m_OwnerFemalePay) > 0 Then
            sSum = sOutputBiomass * (Me.m_OwnerMalePay + Me.m_OwnerFemalePay)
        Else
            sSum = sOutputValue * (Me.m_OwnerMaleShare + Me.m_OwnerFemaleShare) / 100
        End If
        results.Store(Me, cResults.eVariableType.CostOwner, sSum, iTimeStep)
        Return True
    End Function

#End Region ' Cost

#Region " Social "

    Protected Overridable Function CalcWorkerFemales(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * Me.m_WorkerFemale
        results.Store(Me, cResults.eVariableType.NumberOfWorkerFemales, sSum, iTimeStep)
        Return True
    End Function

    Protected Overridable Function CalcWorkerMales(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * Me.m_WorkerMale
        results.Store(Me, cResults.eVariableType.NumberOfWorkerMales, sSum, iTimeStep)

        Return True
    End Function

    Protected Overridable Function CalcWorkerParttime(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * Me.m_WorkerParttime
        results.Store(Me, cResults.eVariableType.NumberOfWorkerPartTime, sSum, iTimeStep)
        Return True
    End Function

    Protected Overridable Function CalcWorkerOther(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * Me.m_WorkerOther
        results.Store(Me, cResults.eVariableType.NumberOfWorkerOther, sSum, iTimeStep)

        Return True
    End Function

    Protected Overridable Function CalcOwnerMales(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * Me.m_OwnerMale
        results.Store(Me, cResults.eVariableType.NumberOfOwnerMales, sSum, iTimeStep)

        Return True
    End Function

    Protected Overridable Function CalcOwnerFemales(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * Me.m_OwnerFemale
        results.Store(Me, cResults.eVariableType.NumberOfOwnerFemales, sSum, iTimeStep)
        Return True
    End Function

    Protected Overridable Function CalcWorkerDependents(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * (Me.m_WorkerFemaleDependents * Me.m_WorkerFemale + Me.m_WorkerMaleDependents * Me.m_WorkerMale)
        results.Store(Me, cResults.eVariableType.NumberOfWorkerDependents, sSum, iTimeStep)
        Return True
    End Function

    Protected Overridable Function CalcOwnerDependents(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sSum As Single = sOutputBiomass * (Me.m_OwnerFemaleDependents * Me.m_OwnerFemale + Me.m_OwnerMaleDependents * Me.m_OwnerMale)
        results.Store(Me, cResults.eVariableType.NumberOfOwnerDependents, sSum, iTimeStep)
        Return True
    End Function

#End Region ' Social

#End Region ' Calculations

#Region " Properties "

#Region " Products "

    <Browsable(True), _
        Category(sPROPCAT_PRODUCTS), _
        DisplayName("Energy products"), _
        Description("Energy products per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(2)> _
    Public Property EnergyProducts() As Single
        Get
            Return m_EnergyProducts
        End Get
        Set(ByVal value As Single)
            m_EnergyProducts = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_PRODUCTS), _
        DisplayName("Industrial products"), _
        Description("Revenue of industrial products per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(3)> _
    Public Property IndustrialProducts() As Single
        Get
            Return m_IndustrialProducts
        End Get
        Set(ByVal value As Single)
            m_IndustrialProducts = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_PRODUCTS), _
        DisplayName("Service products"), _
        Description("Revenue of services per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(4)> _
    Public Property ServiceProducts() As Single
        Get
            Return m_ServiceProducts
        End Get
        Set(ByVal value As Single)
            m_ServiceProducts = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SUBSIDIES), _
        DisplayName("Energy subsidy"), _
        Description("Energy subsidy per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(1)> _
    Public Property SubsidyEnergy() As Single
        Get
            Return m_SubsidyEnergy
        End Get
        Set(ByVal value As Single)
            m_SubsidyEnergy = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SUBSIDIES), _
        DisplayName("Other subsidies"), _
        Description("Other subsidies per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(2)> _
    Public Property SubsidyOther() As Single
        Get
            Return m_SubsidyOther
        End Get
        Set(ByVal value As Single)
            m_SubsidyOther = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
     Category(sPROPCAT_GENERAL), _
     DisplayName("Broker"), _
     Description("States whether this unit functions as a broker"), _
     cPropertySorter.PropertyOrder(5)> _
    Public Overridable Property Broker() As Boolean
        Get
            Return m_bBroker
        End Get
        Set(ByVal value As Boolean)
            Me.m_bBroker = value
            Me.SetChanged()
        End Set
    End Property
#End Region ' Products

#Region " Pay "

    <Browsable(True), _
         Category(sPROPCAT_PAY), _
         DisplayName("Female worker pay"), _
         Description("Female worker pay per tonnes of product"), _
         DefaultValue(0.0!), _
         cPropertySorter.PropertyOrder(1)> _
      Public Property WorkerFemalePay() As Single
        Get
            Return Me.m_WorkerFemalePay
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerFemalePay = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_PAY), _
        DisplayName("Male worker pay"), _
        Description("Male worker pay per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(2)> _
    Public Property WorkerMalePay() As Single
        Get
            Return Me.m_WorkerMalePay
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerMalePay = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_PAY), _
        DisplayName("Female owners pay"), _
        Description("Female owners pay per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(3)> _
    Public Property OwnerFemalePay() As Single
        Get
            Return Me.m_OwnerFemalePay
        End Get
        Set(ByVal value As Single)
            Me.m_OwnerFemalePay = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_PAY), _
        DisplayName("Male owners pay"), _
        Description("Male owners pay per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(4)> _
    Public Property OwnerMalePay() As Single
        Get
            Return Me.m_OwnerMalePay
        End Get
        Set(ByVal value As Single)
            Me.m_OwnerMalePay = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_PAY), _
        DisplayName("Other worker pay"), _
        Description("Other worker pay per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(10)> _
    Public Property WorkerOtherPay() As Single
        Get
            Return Me.m_WorkerOtherPay
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerOtherPay = value
            SetChanged()
        End Set
    End Property

#End Region ' Pay

#Region " Share "

    <Browsable(True), _
         Category(sPROPCAT_SHARE), _
         DisplayName("Female worker share"), _
         Description("Female worker share in % of revenue"), _
         DefaultValue(0.0!), _
         cPropertySorter.PropertyOrder(1)> _
      Public Property WorkerFemaleshare() As Single
        Get
            Return Me.m_WorkerFemaleShare
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerFemaleShare = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SHARE), _
        DisplayName("Male worker share"), _
        Description("Male worker share in % of revenue"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(2)> _
    Public Property WorkerMaleshare() As Single
        Get
            Return Me.m_WorkerMaleShare
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerMaleShare = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SHARE), _
        DisplayName("Female owners share"), _
        Description("Female owners share in % of revenue"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(3)> _
    Public Property OwnerFemaleshare() As Single
        Get
            Return Me.m_OwnerFemaleShare
        End Get
        Set(ByVal value As Single)
            Me.m_OwnerFemaleShare = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SHARE), _
        DisplayName("Male owners share"), _
        Description("Male owners share in % of revenue"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(4)> _
 Public Property OwnerMaleshare() As Single
        Get
            Return Me.m_OwnerMaleShare
        End Get
        Set(ByVal value As Single)
            Me.m_OwnerMaleShare = value
            SetChanged()
        End Set
    End Property

#End Region ' Share

#Region " Input cost "

    <Browsable(True), _
        Category(sPROPCAT_INPUTCOST), _
        DisplayName("Capital cost"), _
        Description("Capital cost per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(2)> _
    Public Property CapitalInput() As Single
        Get
            Return m_CapitalCost
        End Get
        Set(ByVal value As Single)
            m_CapitalCost = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_INPUTCOST), _
        DisplayName("Energy cost"), _
        Description("Energy cost per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(3)> _
    Public Property EnergyCost() As Single
        Get
            Return m_EnergyCost
        End Get
        Set(ByVal value As Single)
            m_EnergyCost = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_INPUTCOST), _
        DisplayName("Industrial cost"), _
        Description("Industrial cost per tonnes of product"), _
        DefaultValue(0), _
        cPropertySorter.PropertyOrder(4)> _
    Public Property IndustrialCost() As Single
        Get
            Return m_IndustrialCost
        End Get
        Set(ByVal value As Single)
            m_IndustrialCost = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_INPUTCOST), _
        DisplayName("Services cost"), _
        Description("Services cost per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(5)> _
    Public Property ServiceCost() As Single
        Get
            Return m_ServiceCost
        End Get
        Set(ByVal value As Single)
            m_ServiceCost = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_INPUTCOST), _
        DisplayName("Management cost"), _
        Description("Management cost per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(6)> _
    Public Property ManagementCost() As Single
        Get
            Return m_ManagementCost
        End Get
        Set(ByVal value As Single)
            m_ManagementCost = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
    Category(sPROPCAT_INPUTCOST), _
    DisplayName("Royalty cost"), _
    Description("Royalty cost per tonnes of product"), _
    DefaultValue(0.0!), _
    cPropertySorter.PropertyOrder(7)> _
Public Property RoyaltyCost() As Single
        Get
            Return m_RoyaltyCost
        End Get
        Set(ByVal value As Single)
            m_RoyaltyCost = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
    Category(sPROPCAT_INPUTCOST), _
    DisplayName("Certification cost"), _
    Description("Certification cost per tonnes of product"), _
    DefaultValue(0.0!), _
    cPropertySorter.PropertyOrder(8)> _
Public Property CertificationCost() As Single
        Get
            Return m_CertificationCost
        End Get
        Set(ByVal value As Single)
            m_CertificationCost = value
            SetChanged()
        End Set
    End Property

#End Region ' Input

#Region " Taxes "

    <Browsable(True), _
        Category(sPROPCAT_TAXES), _
        DisplayName("Environmental tax"), _
        Description("Environmental tax per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(1)> _
    Public Property TaxEnvironmental() As Single
        Get
            Return m_TaxesEnvironmental
        End Get
        Set(ByVal value As Single)
            m_TaxesEnvironmental = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
         Category(sPROPCAT_TAXES), _
        DisplayName("Export tax"), _
         Description("Export tax per tonnes of product"), _
         DefaultValue(0.0!), _
         cPropertySorter.PropertyOrder(2)> _
     Public Property TaxExport() As Single
        Get
            Return m_TaxesExport
        End Get
        Set(ByVal value As Single)
            m_TaxesExport = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_TAXES), _
        DisplayName("Import tax"), _
        Description("Import tax per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(3)> _
    Public Property TaxImport() As Single
        Get
            Return m_TaxesImport
        End Get
        Set(ByVal value As Single)
            m_TaxesImport = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_TAXES), _
        DisplayName("Production tax"), _
        Description("Production tax per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(4)> _
     Public Property TaxProduction() As Single
        Get
            Return m_TaxesProduction
        End Get
        Set(ByVal value As Single)
            m_TaxesProduction = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
         Category(sPROPCAT_TAXES), _
         DisplayName("VAT tax"), _
         Description("VAT tax per tonnes of product"), _
         DefaultValue(0.0!), _
         cPropertySorter.PropertyOrder(6)> _
     Public Property TaxVAT() As Single
        Get
            Return m_TaxesVAT
        End Get
        Set(ByVal value As Single)
            m_TaxesVAT = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_TAXES), _
        DisplayName("Profit tax (prop.)"), _
        Description("Tax as proportion of profit"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(6)> _
    Public Property ProfitTax() As Single
        Get
            Return m_TaxesProfit
        End Get
        Set(ByVal value As Single)
            m_TaxesProfit = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_TAXES), _
        DisplayName("License tax"), _
        Description("License tax per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(7)> _
    Public Property LicenseTax() As Single
        Get
            Return m_TaxesLicense
        End Get
        Set(ByVal value As Single)
            m_TaxesLicense = value
            SetChanged()
        End Set
    End Property

#End Region ' Taxes

#Region " Social "

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("No. female workers"), _
        Description("Number of female workers per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(1)> _
    Public Property WorkerFemale() As Single
        Get
            Return Me.m_WorkerFemale
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerFemale = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("No. male workers"), _
        Description("Number of male workers per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(2)> _
    Public Property WorkerMale() As Single
        Get
            Return Me.m_WorkerMale
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerMale = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("No part-time workers"), _
        Description("Number of part-time workers per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(3)> _
    Public Property WorkerParttime() As Single
        Get
            Return Me.m_WorkerParttime
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerParttime = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("No. other workers"), _
        Description("Number of other workers per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(4)> _
    Public Property WorkerOther() As Single
        Get
            Return Me.m_WorkerOther
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerOther = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("No. female owners"), _
        Description("Number of female owners per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(10)> _
    Public Property OwnerFemale() As Single
        Get
            Return Me.m_OwnerFemale
        End Get
        Set(ByVal value As Single)
            Me.m_OwnerFemale = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("No. male owners"), _
        Description("Number of male owners per tonnes of product"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(11)> _
    Public Property OwnerMale() As Single
        Get
            Return Me.m_OwnerMale
        End Get
        Set(ByVal value As Single)
            Me.m_OwnerMale = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("Female worker dependents"), _
        Description("Number of dependents per female worker"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(20)> _
    Public Property WorkerFemaleDependents() As Single
        Get
            Return Me.m_WorkerFemaleDependents
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerFemaleDependents = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("Male worker dependents"), _
        Description("Number of dependents per male worker"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(21)> _
    Public Property WorkerMaleDependents() As Single
        Get
            Return Me.m_WorkerMaleDependents
        End Get
        Set(ByVal value As Single)
            Me.m_WorkerMaleDependents = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("Female owner dependents"), _
        Description("Number of dependents per female owner"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(30)> _
    Public Property OwnerFemaleDependents() As Single
        Get
            Return Me.m_OwnerFemaleDependents
        End Get
        Set(ByVal value As Single)
            Me.m_OwnerFemaleDependents = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(sPROPCAT_SOCIAL), _
        DisplayName("Male owner dependents"), _
        Description("Number of dependents per male owner"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(31)> _
    Public Property OwnerMaleDependents() As Single
        Get
            Return Me.m_OwnerMaleDependents
        End Get
        Set(ByVal value As Single)
            Me.m_OwnerMaleDependents = value
            SetChanged()
        End Set
    End Property

#End Region ' Social

#End Region ' Properties

End Class
