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
Imports EwECore
Imports EwEUtils.Utilities
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style
Imports System.Text

#End Region ' Imports

''' <summary>
''' 
''' </summary>
<TypeConverter(GetType(cPropertySorter)), _
    DefaultProperty("Name"), _
    Serializable()> _
Public Class cProducerUnit
    Inherits cEconomicUnit

    Public Sub New()
        MyBase.New()
    End Sub

#Region " Helper classes "

    ''' =======================================================================
    ''' <summary>
    ''' Helper class; allows the property grid to pick a fleet DBID from a list 
    ''' of group names
    ''' </summary>
    ''' =======================================================================
    Public Class cFleetConverter
        Inherits TypeConverter

        Private Function FleetList() As List(Of cEcopathFleetInput)
            Dim lFleets As New List(Of cEcopathFleetInput)
            Dim core As cCore = cData.GetInstance().Core
            For iFleet As Integer = 1 To core.nFleets
                lFleets.Add(core.EcopathFleetInputs(iFleet))
            Next
            Return lFleets
        End Function

        Public Overrides Function GetStandardValuesSupported(ByVal context As ITypeDescriptorContext) As Boolean
            ' Show combo
            Return True
        End Function

        Public Overrides Function GetStandardValuesExclusive(ByVal context As ITypeDescriptorContext) As Boolean
            ' Do not edit combo
            Return True
        End Function

        ''' <summary>
        ''' Override the GetStandardValues method and return a 
        ''' StandardValuesCollection filled with your standard values
        ''' </summary>
        Public Overrides Function GetStandardValues(ByVal context As ITypeDescriptorContext) As TypeConverter.StandardValuesCollection
            Dim lFleets As List(Of cEcopathFleetInput) = Me.FleetList
            Dim lFleetNames As New List(Of String)
            Dim fleet As cEcopathFleetInput = Nothing

            lFleetNames.Add("<None>")
            For iFleet As Integer = 0 To lFleets.Count - 1
                fleet = lFleets(iFleet)
                lFleetNames.Add(fleet.Name)
            Next
            Return New StandardValuesCollection(lFleetNames)
        End Function

        Public Overrides Function CanConvertFrom(ByVal context As ITypeDescriptorContext, ByVal sourceType As System.Type) As Boolean
            Return sourceType Is GetType(String)
        End Function

        Public Overrides Function CanConvertTo(ByVal context As ITypeDescriptorContext, ByVal destinationType As System.Type) As Boolean
            Return destinationType Is GetType(Integer)
        End Function

        ''' <summary>
        ''' Convert fleet name to DBID
        ''' </summary>
        Public Overrides Function ConvertFrom(ByVal context As ITypeDescriptorContext, _
                ByVal culture As System.Globalization.CultureInfo, _
                ByVal value As Object) As Object

            If TypeOf value Is String Then
                If Not String.IsNullOrEmpty(CStr(value)) Then
                    Dim lFleets As List(Of cEcopathFleetInput) = Me.FleetList
                    Dim iDBID As Integer = 0

                    For Each fleet As cEcopathFleetInput In lFleets
                        If (fleet.Name = CStr(value)) Then
                            iDBID = CInt(fleet.GetVariable(eVarNameFlags.DBID))
                            Exit For
                        End If
                    Next
                    Return iDBID
                End If
            End If

            Return MyBase.ConvertFrom(context, culture, value)
        End Function

        ''' <summary>
        ''' Convert DBID to fleet name
        ''' </summary>
        Public Overrides Function ConvertTo(ByVal context As ITypeDescriptorContext, _
                ByVal culture As System.Globalization.CultureInfo, _
                ByVal value As Object, _
                ByVal destinationType As System.Type) As Object

            If TypeOf value Is Integer Then
                Dim lFleets As List(Of cEcopathFleetInput) = Me.FleetList
                Dim strName As String = "<None>"

                For Each fleet As cEcopathFleetInput In lFleets
                    If (CInt(fleet.GetVariable(eVarNameFlags.DBID)) = CInt(value)) Then
                        strName = fleet.Name
                        Exit For
                    End If
                Next
                Return strName
            End If

            Return MyBase.ConvertTo(context, culture, value, destinationType)

        End Function

    End Class

#End Region ' Helper classes

#Region " Private vars "

    ''' <summary>Ecopath group that this metier catches.</summary>
    Private m_iEcopathGroupID As Integer = 0
    ''' <summary>Ecopath fleet that this metier fishes with.</summary>
    Private m_iEcopathFleetID As Integer = 0

    Private m_fleet As cEcopathFleetInput = Nothing

    Private m_sObserverCost As Single = 0.0!
    Private m_sObserverRate As Single = 1.0!
    Private m_sOriginalOutputBiomass As Single = 0.0!

    Private m_sEffort As Single = 1.0
    Private m_bUseEffort As Boolean = False

    Private m_sTicketProducts As Single = 0
    Private m_asLandings() As Single
    Private m_asLandingsValue() As Single

#End Region ' Public vars

#Region " Overrides "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialize the unit for a new run.
    ''' </summary>
    ''' <param name="iSequence"></param>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub InitRun(ByVal iSequence As Integer)
        MyBase.InitRun(iSequence)
        ' Reset local vars for the next run
        Me.m_sOriginalOutputBiomass = 0.0!

        ReDim Me.m_asLandings(core.nGroups)
        ReDim Me.m_asLandingsValue(core.nGroups)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialize the unit for a new time step.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub Clear()
        MyBase.Clear()
        ' Reset effort to 1 at the beginning of every time step
        Me.m_sEffort = 1.0!
        ' Clear totals prior to a run!
        If (Me.m_asLandings IsNot Nothing) Then
            Array.Clear(Me.m_asLandings, 0, Me.m_asLandings.Length - 1)
            Array.Clear(Me.m_asLandingsValue, 0, Me.m_asLandingsValue.Length - 1)
        End If
    End Sub

    Public Shadows Function HasTarget(ByVal unit As cUnit, ByVal group As cEcoPathGroupInput) As Boolean

        ' Follow each output link
        For iLink As Integer = 0 To Me.LinkOutCount - 1
            Dim link As cLink = Me.LinkOut(iLink)
            If TypeOf link Is cLinkLandings Then
                Dim linkSpec As cLinkLandings = DirectCast(link, cLinkLandings)
                If ReferenceEquals(linkSpec.Target, unit) And ReferenceEquals(linkSpec.Group, group) Then Return True
            Else
                ' See the target link is the requesting unit
                If ReferenceEquals(link.Target, unit) Then Return True
            End If
        Next iLink
        Return False

    End Function

#End Region ' Overrides

#Region " Calculations "

    <Obsolete("cProducerUnit.Process override should not be called anymore")> _
    Public Overrides Sub Process(ByVal results As cResults, _
                                 ByVal input As cInput, _
                                 ByVal iTimeStep As Integer, _
                                 ByVal iUnit As Integer)

        Throw New NotImplementedException("cProducerUnit.Process override should not be called anymore")
        '' Store landings and landings price only for producers
        'results.Store(Me, cResults.eVariableType.Landings, input.Tons, iTimeStep)
        'results.Store(Me, cResults.eVariableType.LandingsPrice, input.Value, iTimeStep)

        'MyBase.Process(results, input, iTimeStep, iUnit)

        

    End Sub

    Protected Overrides Function Calculate(ByVal results As cResults, _
            ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
            ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
            ByVal iTimeStep As Integer) As Boolean
        Dim bSucces As Boolean

        'VC090310: Producer cost needs to reflect ecosim effort. 
        'We need to calculate the base cost from the standard calculations
        'below, but then change the effort-related cost based on Ecosim effort.

        ' First time step?
        ' VC090808: problem with this is that the user may have changed effort even in the first time step.
        ' this will mess up calculations, but can't find an easy way to calculate ecopath baseline???????? 
        '
        ' VC:  because of the problem above, I force the effort to be 1 at timestep 1.

        ' JS110325: Added sanity check
        If (results.RunType = cModel.eRunTypes.Ecopath) Then
            Debug.Assert(iTimeStep = 1, "Ecopath should use time step 1 only")
        End If

        If (iTimeStep = 1) Then
            ' #Yes: store base biomass
            Me.m_sOriginalOutputBiomass = sOutputBiomass
            ' Do not use effort this time step
            Me.m_bUseEffort = False
        Else
            ' #No: use effort-based biomass
            Me.m_bUseEffort = True

            ' Get effort
            If (Me.m_fleet IsNot Nothing) Then
                ' Try to get fishing rate shape
                '   NB: shapes are stored 0-based
                Dim shp As cShapeData = Me.Core.FishingEffortShapeManager.Item(Me.Fleet.Index - 1)
                ' Sanity check
                Debug.Assert((shp IsNot Nothing), "Error on timestep " & CStr(iTimeStep) & ": fishing rate shape not available")
                ' Get effort
                'We run the policy search for 20 extra years for which there is no effort, so use last effort for those years
                If iTimeStep <= Core.EcoSimModelParameters.NumberYears * 12 Then
                    Me.m_sEffort = shp.ShapeData(iTimeStep)
                Else
                    Me.m_sEffort = shp.ShapeData(Core.EcoSimModelParameters.NumberYears * 12)
                End If
            End If
        End If

        'The production unit needs to do the same calculations as the MyBase=cEconomicUnit, but:
        bSucces = MyBase.Calculate(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)

        ' Calc AddsObserver costs
        bSucces = bSucces And Me.CalcObserverCost(results, sOutputBiomass, iTimeStep)

        'VC090310: Categories of costs and how they are handled:
        'Commercial fisheries
        'Related to tonnes: Pay/Share, all taxes, revenue (apart from subsidies), certification cost
        'Related to effort: Energy, Industrial, services, capital, observers, management, license, subsidies

        'Recreational fisheries
        'Effort: related to biomass of target species (sigmoid relationship)
        'Income: related to effort (for guide operations); 0 if private boats
        'Cost: modeled same way as for commercial fisheries

        'Eco tours
        'Effort: related to biomass of target species (sigmoid relationship)
        'Income: related to effort; using ticket revenue: m_sTicketProducts 
        'Cost: modeled same way as for commercial fisheries 

        Return bSucces

    End Function

    Protected Overrides Function CalcProducts(ByVal results As cResults, _
             ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
             ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
             ByVal iTimeStep As Integer) As Boolean


        'Now add to this the revenue from paying customers
        Dim sSum As Single = Me.m_sEffort * Me.m_sTicketProducts
        results.Store(Me, cResults.eVariableType.RevenueTickets, sSum, iTimeStep)

        ' Use standard calculations, which is desirable so we do not have to keep 
        ' updating formulas in different places in case standard calculations were 
        ' to change       '
        'Last part is the usual biomass related part:
        'Dim sSum As Single = sOutputBiomass * (Me.EnergyProducts + Me.IndustrialProducts + Me.ServiceProducts)
        Return MyBase.CalcProducts(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)

    End Function

    Protected Overrides Function CalcRawmaterialCost(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Return results.Store(Me, cResults.eVariableType.CostRawmaterial, 0, iTimeStep)

    End Function

    Protected Overrides Function CalcInputCost(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        ' Need to include effort in our calculations
        If Me.m_bUseEffort Then
            ' #Yes: do NOT use sOutputBiomass, but instead use base biomass x effort
            Dim sSum As Single = Me.m_sOriginalOutputBiomass * Me.m_sEffort * _
                                 (Me.CapitalInput + Me.EnergyCost + Me.IndustrialCost + Me.ServiceCost)
            Return results.Store(Me, cResults.eVariableType.CostInput, sSum, iTimeStep)
        Else
            Return MyBase.CalcInputCost(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        End If

    End Function

    Protected Overrides Function CalcManagementRoyaltyCertificationCost(ByVal results As cResults, _
               ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
               ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
               ByVal iTimeStep As Integer) As Boolean

        'the costs for management and royalties are proportional to effort
        If Me.m_bUseEffort Then
            Dim sSum As Single = Me.m_sEffort * Me.m_sOriginalOutputBiomass * (Me.ManagementCost + Me.RoyaltyCost)

            'the cost for certification is assumed proportional to landings, so add this
            sSum += sOutputBiomass * Me.CertificationCost

            Return results.Store(Me, cResults.eVariableType.CostManagementRoyaltyCertification, sSum, iTimeStep)
        Else  'just like other calculations:
            Return MyBase.CalcManagementRoyaltyCertificationCost(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        End If

    End Function

    Protected Overrides Function CalcSubsidy(ByVal results As cResults, _
               ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
               ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
               ByVal iTimeStep As Integer) As Boolean

        If Me.m_bUseEffort Then
            Dim sSum As Single = Me.m_sEffort * Me.m_sOriginalOutputBiomass * (Me.SubsidyEnergy + Me.SubsidyOther)
            results.Store(Me, cResults.eVariableType.RevenueSubsidies, sSum, iTimeStep)
            Return True
        Else
            Return MyBase.CalcSubsidy(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        End If
    End Function

    Protected Overridable Function CalcObserverCost(ByVal results As cResults, ByVal sOutputBiomass As Single, _
                ByVal iTimeStep As Integer) As Boolean

        Dim sObsCost As Single = 0
        If Me.m_bUseEffort Then
            sObsCost = Me.m_sOriginalOutputBiomass * Me.m_sEffort * (Me.ObserverCost * Me.ObserverRate)
        Else
            sObsCost = sOutputBiomass * (Me.ObserverCost * Me.ObserverRate)
        End If
        Return results.Store(Me, cResults.eVariableType.CostObserver, sObsCost, iTimeStep)

    End Function

    ''' <summary>
    ''' The number of jobs for producers is a function of effort, while their salary isn't
    ''' </summary>
    ''' <param name="results"></param>
    ''' <param name="sInputBiomass"></param>
    ''' <param name="sInputValue"></param>
    ''' <param name="sOutputBiomass"></param>
    ''' <param name="sOutputValue"></param>
    ''' <param name="iTimeStep"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overrides Function CalcWorkerFemales(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean
        If Me.m_bUseEffort Then
            Dim sSum As Single = Me.m_sEffort * Me.m_sOriginalOutputBiomass * Me.WorkerFemale
            Return results.Store(Me, cResults.eVariableType.NumberOfWorkerFemales, sSum, iTimeStep)
        Else
            Return MyBase.CalcWorkerFemales(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        End If

    End Function

    Protected Overrides Function CalcWorkerMales(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        If Me.m_bUseEffort Then
            Dim sSum As Single = Me.m_sEffort * Me.m_sOriginalOutputBiomass * Me.WorkerMale
            Return results.Store(Me, cResults.eVariableType.NumberOfWorkerMales, sSum, iTimeStep)
        Else
            Return MyBase.CalcWorkerMales(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        End If

    End Function

    Protected Overrides Function CalcOwnerMales(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        If Me.m_bUseEffort Then
            Dim sSum As Single = Me.m_sEffort * Me.m_sOriginalOutputBiomass * Me.OwnerMale
            Return results.Store(Me, cResults.eVariableType.NumberOfOwnerMales, sSum, iTimeStep)
        Else
            Return MyBase.CalcOwnerMales(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        End If

    End Function

    Protected Overrides Function CalcOwnerFemales(ByVal results As cResults, _
                ByVal sInputBiomass As Single, ByVal sInputValue As Single, _
                ByVal sOutputBiomass As Single, ByVal sOutputValue As Single, _
                ByVal iTimeStep As Integer) As Boolean

        If Me.m_bUseEffort Then
            Dim sSum As Single = Me.m_sEffort * Me.m_sOriginalOutputBiomass * Me.OwnerFemale
            Return results.Store(Me, cResults.eVariableType.NumberOfOwnerFemales, sSum, iTimeStep)
        Else
            Return MyBase.CalcOwnerFemales(results, sInputBiomass, sInputValue, sOutputBiomass, sOutputValue, iTimeStep)
        End If

    End Function

#End Region ' Calculations

#Region " Overrides "

    <Browsable(False)> _
    Public Overrides ReadOnly Property HasError() As Boolean
        Get
            Return (Me.m_fleet Is Nothing) Or (Not String.IsNullOrWhiteSpace(Me.UnlikelyOutputs))
        End Get
    End Property

    <Browsable(False)> _
    Public Overrides ReadOnly Property Style() As cStyleGuide.eStyleFlags
        Get
            Dim st As cStyleGuide.eStyleFlags = MyBase.Style
            If (Me.m_fleet IsNot Nothing) Then st = st Or cStyleGuide.eStyleFlags.ValueComputed
            If (Me.HasError) Then st = st Or cStyleGuide.eStyleFlags.ErrorEncountered
            Return st
        End Get
    End Property

#End Region ' Overrides

#Region " Alternate name "

    Private Function GenerateName() As String
        If (Me.m_fleet Is Nothing) Then Return "! No fleet"
        Return Me.m_fleet.Name
    End Function

    Public Overrides Property Name() As String
        Get
            Dim strName As String = MyBase.Name
            If String.IsNullOrEmpty(strName) Then
                strName = Me.GenerateName()
            End If
            Return strName
        End Get
        Set(ByVal value As String)
            ' Setting generated name?
            If (String.Compare(value, Me.GenerateName()) = 0) Then
                ' #Yes: Clear the base name
                MyBase.Name = ""
            Else
                ' #No: Set the base name
                MyBase.Name = value
            End If
        End Set
    End Property

#End Region ' Alternate name

#Region " Properties "

    Public Overrides ReadOnly Property BiomassRatio As String
        Get
            ' Count # of active links
            Dim iNumActiveLinks As Integer = 0
            For i As Integer = 0 To Me.LinkOutCount - 1
                If Me.LinkOut(i).BiomassRatio > 0 Then
                    iNumActiveLinks += 1
                End If
            Next
            Return MyBase.BiomassRatio & " / " & iNumActiveLinks.ToString()
        End Get
    End Property

    <Browsable(True), _
        Category(sPROPCAT_VALIDATION), _
        DisplayName("Unlikely outputs"), _
        Description("Names of groups that are landed and transferred through the chain with an unlikely biomass ratios that exceed 1"), _
        cPropertySorter.PropertyOrder(7)> _
    Public ReadOnly Property UnlikelyOutputs As String
        Get

            If Me.Core Is Nothing Then Return ""

            Dim sTotal(Me.Core.nGroups) As Single
            Dim sbError As New StringBuilder()
            Dim fmt As New cCoreInterfaceFormatter()

            For i As Integer = 0 To Me.LinkOutCount - 1
                Dim ll As cLinkLandings = DirectCast(Me.LinkOut(i), cLinkLandings)
                If (ll.Group IsNot Nothing) Then
                    sTotal(ll.Group.Index) += ll.BiomassRatio
                End If
            Next

            For i As Integer = 1 To Me.Core.nGroups
                If sTotal(i) > 1.0! Then
                    If (sbError.Length > 0) Then
                        sbError.Append(",")
                    End If
                    sbError.Append(String.Format(ScientificInterfaceShared.My.Resources.GENERIC_LABEL_DETAILED, _
                                                 fmt.GetDescriptor(Me.Core.EcoPathGroupInputs(i)), _
                                                 sTotal(i).ToString("R")))
                End If
            Next
            Return sbError.ToString
        End Get
    End Property

    <Browsable(True), _
    Category(sPROPCAT_INPUTCOST), _
    DisplayName("Monitoring cost"), _
    Description("Cost for monitors (if on board) per tonnes. Assumed to vary with effort"), _
    DefaultValue(0.0!), _
    cPropertySorter.PropertyOrder(20)> _
    Public Property ObserverCost() As Single
        Get
            Return Me.m_sObserverCost
        End Get
        Set(ByVal value As Single)
            Me.m_sObserverCost = value
            Me.SetChanged()
        End Set
    End Property

    <Browsable(True), _
     Category(sPROPCAT_INPUTCOST), _
     DisplayName("Monitor coverage rate"), _
     Description("Monitor coverage rate, (proportion of boats with observers onboard)"), _
     DefaultValue(0.0!), _
     cPropertySorter.PropertyOrder(21)> _
    Public Property ObserverRate() As Single
        Get
            Return Me.m_sObserverRate
        End Get
        Set(ByVal value As Single)
            Me.m_sObserverRate = value
            Me.SetChanged()
        End Set
    End Property

    <Browsable(True), _
     Category(sPROPCAT_REVENUE), _
     DisplayName("Ticket revenue"), _
     Description("Revenue from paying customers at Ecopath baseline effort (unity effort). Revenue assumed proportional to effort."), _
     DefaultValue(0.0!), _
     cPropertySorter.PropertyOrder(1)> _
    Public Property TicketProducts() As Single
        Get
            Return m_sTicketProducts
        End Get
        Set(ByVal value As Single)
            m_sTicketProducts = value
            SetChanged()
        End Set
    End Property

    <Browsable(True), _
        Category(cUnit.sPROPCAT_GENERAL), _
        DisplayName("Ecopath fleet"), _
        Description("Ecopath fleet for this producer"), _
        DefaultValue(0.0!), _
        cPropertySorter.PropertyOrder(11), _
        TypeConverter(GetType(cFleetConverter))> _
    Public Overridable Property EcopathFleetID() As Integer
        Get
            Return Me.m_iEcopathFleetID
        End Get
        Set(ByVal value As Integer)
            Me.m_iEcopathFleetID = value
            Me.SetChanged()
        End Set
    End Property

    Public Overrides ReadOnly Property Category() As String
        Get
            Return "Producer"
        End Get
    End Property

    <Browsable(False)> _
    Public Overrides ReadOnly Property UnitType() As cUnitFactory.eUnitType
        Get
            Return cUnitFactory.eUnitType.Producer
        End Get
    End Property

    <Browsable(False)> _
    Public Overrides ReadOnly Property CanCompute() As Boolean
        Get
            Return True
        End Get
    End Property

#Region " Ecopath integration "

    <Browsable(False)> _
    Public Overridable Property Fleet() As cEcopathFleetInput
        Get
            Return Me.m_fleet
        End Get
        Friend Set(ByVal value As cEcopathFleetInput)
            Me.m_fleet = value
            If (Fleet IsNot Nothing) Then
                Me.EcopathFleetID = CInt(Fleet.GetVariable(eVarNameFlags.DBID))
            Else
                Me.EcopathFleetID = 0
            End If
        End Set
    End Property

#End Region ' Ecopath integration

#End Region ' Properties

#Region " Landings "

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="iGroup"></param>
    ''' <param name="sBiomass">Total biomass landed in area</param>
    ''' <param name="sValue">Total value landed in area</param>
    Public Sub SetLandings(ByVal iGroup As Integer, ByVal sBiomass As Single, ByVal sValue As Single)
        Me.m_asLandings(iGroup) = sBiomass
        Me.m_asLandingsValue(iGroup) = sValue
    End Sub

    Public Overloads Sub Process(ByVal results As cResults, _
                                 ByVal iTimeStep As Integer, _
                                 ByVal iItem As Integer)

        Dim sTotalOutputBiomass As Single = 0
        Dim sTotalOutputValue As Single = 0

        Dim sBTot As Single = 0
        Dim sValTot As Single = 0
        For iGroup As Integer = 1 To Me.Core.nGroups
            sBTot += Me.m_asLandings(iGroup)
            sValTot += Me.m_asLandingsValue(iGroup)
        Next

        ' No item specified?
        If iItem = 0 Then
            ' #Yes: perform all calculations
            Me.Calculate(results, sBTot, 0, sBTot, sValTot, iTimeStep)
        End If

        ' Determine outgoing biomass ratios for each group
        Dim asTotalBGroup(Me.Core.nGroups) As Single
        For Each link As cLink In Me.m_llinkOutput
            ' Sanity check
            If (TypeOf link Is cLinkLandings) Then
                Dim ll As cLinkLandings = DirectCast(link, cLinkLandings)
                If (ll.Group IsNot Nothing) And (ll.IsVisible) Then
                    asTotalBGroup(ll.Group.Index) += ll.BiomassRatio
                End If
            End If
        Next

        ' Quick fix to avoid divisions by zero later on
        'For iGroup As Integer = 1 To Me.Core.nGroups
        '  If asTotalBGroup(iGroup) = 0.0! Then asTotalBGroup(iGroup) = 1.0!
        'Next

        ' Determine outgoing biomass
        For Each link As cLink In Me.m_llinkOutput

            Dim sBiomass As Single = 0.0
            Dim sValue As Single = 0.0
            'the above was called sPrice, but it is value, so renamed

            Debug.Assert(TypeOf link Is cLinkLandings)

            Dim ll As cLinkLandings = DirectCast(link, cLinkLandings)
            If (ll.Group IsNot Nothing) And (ll.IsVisible) Then
                Dim iGroup As Integer = ll.Group.Index
                If asTotalBGroup(iGroup) > 0 Then
                    'If asTotalBGroup(iGroup) > 0 And m_asLandings(iGroup) > 0 Then
                    sBiomass += Me.m_asLandings(iGroup) * ll.BiomassRatio / asTotalBGroup(iGroup)

                    If (ll.ValueRatio = 1.0!) Then
                        sValue += Me.m_asLandingsValue(iGroup) * ll.BiomassRatio / asTotalBGroup(iGroup)
                    Else
                        sValue += ll.ValueRatio * Me.m_asLandings(iGroup) * ll.BiomassRatio / asTotalBGroup(iGroup)
                    End If

                End If
            End If

            ' Process every link to ensure that target units receive all inputs!
            If (sBiomass > 0) Then
                'VC: I changed the process line to pass sPrice/sBiomass as the third parameter (instead of sPrice). 
                'it is supposed to be the price per unit biomass
                'it was multiplying an extra time with the total catches (sBiomass) as it was.
                link.Target.Process(results, New cInput(Me, sBiomass, sValue), iTimeStep, iItem)
            Else
                ' Process link to make the chain work, even though no data travels over this link!
                link.Target.Process(results, New cInput(Me, sBiomass, sValue), iTimeStep, iItem)
            End If

            sTotalOutputBiomass += sBiomass
            sTotalOutputValue += sValue '* sBiomass

        Next

        results.StoreContribution(iItem, Me, iTimeStep, sValTot, sBTot)

    End Sub

#End Region ' Landings

End Class
