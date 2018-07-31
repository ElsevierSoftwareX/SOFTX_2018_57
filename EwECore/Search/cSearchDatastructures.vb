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
Imports EwEUtils.SystemUtilities.cSystemUtils

#End Region ' Imports

''' <summary>
''' Enumerated type, indicating the different search and optimization modes.
''' </summary>
Public Enum eSearchModes
    ''' <summary>Not in any kind of a search mode</summary>
    NotInSearch
    ''' <summary>A search of some type is initializing the data. 
    ''' All data will be initialized but the code will not go into specific search routines.
    ''' </summary>
    InitializingSearch
    ''' <summary>Fit to time series search is active.</summary>
    FitToTimeSeries
    ''' <summary>Fishing policy search is active.</summary>
    FishingPolicy
    ''' <summary>Monte Carlo is active.</summary>
    MonteCarlo
    ''' <summary>MSE search is active.</summary>
    MSE
    ''' <summary>Spatial optimization is active.</summary>
    SpatialOpt
    ''' <summary>MSY search is active.</summary>
    MSY
    ''' <summary>FMSY search is active.</summary>
    FMSY
    ''' <summary>An external search is active, for instance triggered by a plug-in.</summary>
    External
End Enum

''' <summary>
''' Data used by Search and Optimization routines
''' </summary>
Public Class cSearchDatastructures

#Region "Public data"

    Public Shared N_CRIT_RESULTS As Integer = [Enum].GetValues(GetType(eSearchCriteriaResultTypes)).Length

    '''' <summary>
    '''' Turn the Fishing Policy Search On or Off.
    '''' </summary>
    '''' <remarks>True search is on, False search is off.</remarks>
    ' Private m_bDoSearch As Boolean

    ''' <summary>Current search mode</summary>
    Private m_SearchMode As eSearchModes

    ''' <summary>Fishing policy search method</summary>
    Public SearchMethod As eSearchOptionTypes
    ''' <summary>Fishing policy initialization method</summary>
    Public InitOption As eInitOption
    Public IncludeCompetitiveImpact As Boolean

    Public MinimizeEffortChange As Boolean
    Public MaxEffortChange As Single

    Public LimitFishingMortality As Boolean
    Public PortFolio As Boolean

    Public NYears As Integer
    Public m_NBlocks As Integer 'Number of block for fishing policy explorations
    Public MaxEffort As Single

    Public DiscountFactor As Single
    Public GenDiscountFactor As Single


    'jb in EwE5 these are defined in Fletch.bas
    Public Ecodistance As Single
    Public ExistValue As Single

    Public FblockCode(,) As Integer
    Public ParNumber() As Integer
    Public BlockNumber() As Integer 'Number of a FblockCode() see setFletchPars()
    Public FcodeIsSet As Boolean, LastTotalTime As Integer
    Public ValWeight(N_CRIT_RESULTS) As Single, Jobs() As Single

    ''' <summary>Structure rel weight </summary>
    Public BGoalValue() As Single
    ''' <summary>Mandated rebuilding </summary>
    Public MGoalValue() As Single

    'Public LastYearCost() As Single
    Public LastYearIncomeSpecies(,) As Single
    Public LastYearIncome() As Single

    Public Frates() As Double
    Private m_storedFrates() As Single ' set in saveInitialFishingRate

    Public TargetProfitability() As Single

    ''' <summary>
    ''' Has the base year been set
    ''' </summary>
    ''' <remarks>During an optimization the base year is only set once</remarks>
    Public bBaseYearSet As Boolean
    Public BaseYear As Integer = 1
    Public BaseYearCost() As Single
    Public BaseYearEffort() As Single
    Public BaseYearIncome() As Single
    'BaseYearCost
    Public UseFishingMortalityPenalty As Boolean
    Public FLimit() As Single

    Public nRuns As Integer
    Public nInterations As Integer

    Public UseCostPenalty As Boolean
    Public CostRatio() As Single

    'results of the search
    Public totval As Single
    Public Employ As Single
    Public manvalue As Single
    Public ecovalue As Single
    Public profit As Single

    ''' <summary>Value of Catch</summary>
    ''' <remarks>By (fleet, livingGroup)
    ''' indicies.ValCatch(i, j) = indicies.ValCatch(i, j) + Cloc * DF
    ''' </remarks>
    Public ValCatch(,) As Single

    ''' <summary>Net cost</summary>
    ''' <remarks>NetCost(nfleets)</remarks>
    Public NetCost() As Single

    ''' <summary>Value of catch gear</summary>
    ''' <remarks>
    ''' Valcatch(fleet) = ( ValCatch(fleet, j) + m_EPData.PropLanded(fleet, j) * biomeq(j) _
    '''                  * Fgear(fleet) * m_Data.relQ(fleet, j) * LTV) * m_EPData.Market(fleet, j)</remarks>
    Public ValCatchGear() As Single

    ''' <summary>Discount Factor</summary>
    ''' <remarks>
    ''' <code>If Dgen = Din Then
    '''          indic.DF = Din ^ (iyr - 1) + (Dgen * (Din ^ (iyr - 2)) / 20) * (iyr - 1)
    '''       Else
    '''         indic.DF = (1 + Dalpha) * Din ^ (iyr - 1) - Dalpha * (Din * Dratio) ^ (iyr - 1)
    '''       End If</code>
    ''' </remarks>
    Public DF As Single = 0

    '''' <summary></summary>
    '''' <remarks>
    '''' <code>indic.Cloc = m_EPData.PropLanded(i, j) * BB(j) * Qyear(i) * Fgear(i) * m_Data.relQ(i, j) / 12.0#
    '''' </code>
    '''' </remarks>
    'Public Cloc As Single = 0

    ''' <summary>Sum of Catch for year by fleet, group</summary>
    ''' <remarks>index = (fleet, living)</remarks>
    Public CatchYear(,) As Single

    ''' <summary>Sum of Catch for year group</summary>
    ''' <remarks>index = (living)</remarks>
    Public CatchYearGroup() As Single

    ''' <summary>Total Fishing mortality by group</summary>
    ''' <remarks>FishYear = effort * F * QYear</remarks>
    Public FishYear() As Single

    ' Public CatchYearGroup() As Single

    ''' <summary>
    ''' Semaphor provides single thread access to calcEcospaceMonthlyCatch()
    ''' </summary>
    ''' <remarks></remarks>
    Private m_SearchCatchSemaphor As System.Threading.Semaphore

    Public DiversityIndex As Single

    Public FPSUseEconomicPlugin As Boolean
    Public MSEUseEconomicPlugin As Boolean

    'needed for KemptonsQ
    Private m_EcoFunctions As cEcoFunctions

    Private m_EPdata As cEcopathDataStructures

#End Region

#Region "Private data"

    Private m_ExtraYears As Integer

    ''' <summary>Conventional discount rate for intergeneration discount rate computations</summary>
    ''' <remarks>Drate = 1/(1+DiscountFactor)</remarks>
    Private Drate As Single

    ''' <summary>Intergenerational discount rate for computations</summary>
    ''' <remarks>Dfgrate = 1/(1+GenDiscountFactor)</remarks>
    Private Dfgrate As Single

    ''' <summary>Ratio of intergenerational to standard discount rate </summary>
    '''  <remarks>deltaDDfg = Dfgrate/Drate</remarks>
    Private deltaDDfg As Single

    ''' <summary>Number of years for one generation </summary>
    '''<remarks>GenT = 20</remarks>
    Private GenT As Single

    Private Din As Single, Dgen As Single, Dratio As Single
    Private Dalpha As Single
    Private m_ecopathData As cEcopathDataStructures
    Private m_ecosimData As cEcosimDatastructures

#End Region

#Region " Construction Destruction Cleanup "

    Public Sub New(ByVal EcoFunctions As cEcoFunctions, ByVal EPData As cEcopathDataStructures)

        m_EcoFunctions = EcoFunctions

        Me.m_SearchCatchSemaphor = New System.Threading.Semaphore(1, 1, "SearchMontlyCatch")

        Me.m_EPdata = EPData

        'redim some of the data
        Me.RedimFleets()
        Me.RedimGroups()

        'set some default values
        DiscountFactor = 0.04F
        GenDiscountFactor = 0.0F
        GenT = 20
        nRuns = 1
        nInterations = 2000

    End Sub

    Public Sub Clear()

        Me.LastYearIncome = Nothing ' (NumFleets)
        Me.Jobs = Nothing ' (NumFleets)
        Me.TargetProfitability = Nothing ' (NumFleets)
        Me.BaseYearIncome = Nothing ' (NumFleets)
        Me.BaseYearCost = Nothing ' (NumFleets)
        Me.BaseYearEffort = Nothing ' (NumFleets)
        Me.CostRatio = Nothing ' (NumFleets)
        Me.FblockCode = Nothing ' (NumFleets, NYears)
        Me.Jobs = Nothing ' (NumFleets)
        Me.TargetProfitability = Nothing ' (NumFleets)
        Me.BGoalValue = Nothing ' (NumGroups)
        Me.MGoalValue = Nothing ' (NumGroups)
        Me.ValCatch = Nothing ' (NumFleets, NumGroups)
        Me.NetCost = Nothing ' (NumFleets)
        Me.ValCatchGear = Nothing ' (NumFleets)
        Me.BaseYearIncome = Nothing ' (NumFleets)
        Me.BaseYearCost = Nothing ' (NumFleets)
        Me.CatchYear = Nothing ' (NumFleets, NumGroups)
        Me.CatchYearGroup = Nothing ' (NumGroups)
        Me.FblockCode = Nothing ' (NumFleets, NYears)

        Me.m_SearchCatchSemaphor = Nothing

    End Sub


#End Region ' Construction

#Region "Public properties"

    Friend Event OnSearchStateChanged(ByVal searchMode As eSearchModes)

    Public Property SearchMode() As eSearchModes
        Get
            Return m_SearchMode
        End Get
        Set(ByVal value As eSearchModes)
            Me.m_SearchMode = value
            Me.InitSearch()
            RaiseEvent OnSearchStateChanged(Me.m_SearchMode)
        End Set
    End Property

    ''' <summary>
    ''' Get whether a search is currently in progress.
    ''' </summary>
    Public ReadOnly Property bInSearch() As Boolean
        Get
            Return (Me.m_SearchMode <> eSearchModes.NotInSearch)
        End Get
    End Property

    Public Property bUseFishingMortalityPenality() As Boolean
        Get
            Return Me.UseFishingMortalityPenalty
        End Get
        Set(ByVal value As Boolean)
            Me.UseFishingMortalityPenalty = value
        End Set
    End Property

    ''' <summary>
    ''' The Fishing Policy Search needs Ecosim to run for an additional 20 years
    ''' </summary>
    Public ReadOnly Property ExtraYearsForSearch() As Integer
        Get
            Return m_ExtraYears
        End Get
    End Property

    Public Property nBlocks() As Integer
        Get
            Return m_NBlocks
        End Get

        Set(ByVal value As Integer)
            m_NBlocks = value
            setDefaultFRates()
        End Set

    End Property

    Public ReadOnly Property WeightedTotal() As Single
        Get
            Return Me.ValWeight(eSearchCriteriaResultTypes.TotalValue) * Me.totval + _
                Me.ValWeight(eSearchCriteriaResultTypes.Employment) * Me.Employ + _
                Me.ValWeight(eSearchCriteriaResultTypes.MandateReb) * Me.manvalue + _
                Me.ValWeight(eSearchCriteriaResultTypes.Ecological) * Me.ecovalue + _
                Me.ValWeight(eSearchCriteriaResultTypes.BioDiversity) * Me.DiversityIndex
        End Get
    End Property

    Public ReadOnly Property NumGroups() As Integer
        Get
            Return Me.m_EPdata.NumGroups
        End Get
    End Property

    Public ReadOnly Property NumFleets() As Integer
        Get
            Return Me.m_EPdata.NumFleet
        End Get
    End Property

    Public ReadOnly Property NumLiving() As Integer
        Get
            Return Me.m_EPdata.NumLiving
        End Get
    End Property

#End Region

#Region "Dimensioning"

    Private Function RedimFleets() As Boolean
        Try

            ' JS 08Jul08: this assert is not valid; a new model has no living groups
            'Debug.Assert(NumLiving > 0, "Number of living has not been set.")

            'ReDim LastYearCost(NumFleets)
            ReDim LastYearIncome(NumFleets)
            ReDim Jobs(NumFleets)
            ReDim TargetProfitability(NumFleets)
            ReDim BaseYearIncome(NumFleets)
            ReDim BaseYearCost(NumFleets)
            ReDim BaseYearEffort(NumFleets)
            ReDim CostRatio(NumFleets)

            ReDim FblockCode(NumFleets, NYears)

            ReDim Jobs(NumFleets)
            ReDim TargetProfitability(NumFleets)
            For i As Integer = 1 To NumFleets
                Jobs(i) = 1
                TargetProfitability(i) = 0.1
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Friend Function redimTime(ByVal NumberOfYears As Integer) As Boolean
        Try

            NYears = NumberOfYears
            ReDim FblockCode(NumFleets, NYears)

            setDefaultFBlockCodes()

        Catch ex As Exception
            Debug.Assert(False)
            Return False
        End Try

        Return True
    End Function


    Public Function RedimToSimScenario(ByVal NumberOfYears As Integer) As Boolean
        Dim bSuccess As Boolean = True
        bSuccess = bSuccess And Me.RedimGroups()
        bSuccess = bSuccess And Me.RedimFleets()
        bSuccess = bSuccess And Me.redimTime(NumberOfYears)
        Return bSuccess
    End Function



    Private Function RedimGroups() As Boolean

        Try

            ReDim BGoalValue(NumGroups)
            ReDim MGoalValue(NumGroups)

            ReDim ValCatch(NumFleets, NumGroups)
            ReDim NetCost(NumFleets)
            ReDim ValCatchGear(NumFleets)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".RedimGroups() Exception: " & ex.Message)
            cLog.Write(ex)
            Return False
        End Try
        Return True
    End Function


    Public Sub redimForRun()
        ReDim BaseYearIncome(NumFleets)
        ReDim BaseYearCost(NumFleets)

        ReDim CatchYear(NumFleets, NumGroups)
        ReDim CatchYearGroup(NumGroups)
    End Sub

#End Region

#Region "Search initialization and running"

    Public Sub InitSearch()

        If Me.bInSearch Then

            'Extra Years are only for Fishing policy and MSE search
            'See EwE5 RunModelValue()
            m_ExtraYears = 0
            'If Me.SearchMode = eSearchModes.FishingPolicy Or Me.SearchMode = eSearchModes.MSE Or Me.SearchMode = eSearchModes.InitializingSearch Then
            'jb 30-Dec-09 removed the extra years from the MSE search
            If Me.SearchMode = eSearchModes.FishingPolicy Then
                m_ExtraYears = 20
            End If

            setDefaultOptimizationValues()

        Else

            m_ExtraYears = 0
            m_NBlocks = 0

        End If

    End Sub

    ''' <summary>
    ''' Set default values for the optimiation. These are values that are set the same for every run.
    ''' </summary>
    ''' <remarks>from frmOptF.Load_Form() </remarks>
    Public Sub setDefaultOptimizationValues()

        'If DiscountFactor = 0 Then DiscountFactor = 0.04
        'If GenDiscountFactor = 0 Then GenDiscountFactor = 0.1
        'If BaseYear = 0 Then BaseYear = 1

        setDefaultFRates()

    End Sub


    Private Sub setDefaultFRates()
        Dim i As Integer

        If m_NBlocks = 0 Then
            m_NBlocks = NumFleets + 1
        End If

        ReDim Frates(m_NBlocks)
        ReDim m_storedFrates(m_NBlocks)
        For i = 1 To m_NBlocks
            Frates(i) = 0.01
        Next

    End Sub

    Public Sub setDefaultBGoal(ByVal PB() As Single)
        Dim i As Integer

        For i = 1 To NumLiving
            If PB(i) > 0 Then BGoalValue(i) = 1 / PB(i)
            BGoalValue(i) = CInt(BGoalValue(i) * 5) / 5.0F
        Next

    End Sub


    Public Function SetFletchPars() As Integer
        'determines number of fishing rate parameters to be varied and
        'assigns parameter number to each fishing rate block code used
        Dim i As Integer, j As Integer
        Dim n As Integer
        Dim maxblkNumber As Integer

        'find the biggest block code number and use that to dim ParNumber
        'this allows us to use the existing EwE5 ParNumber(FblockCode(i, j)) code without any changes
        For i = 1 To NumFleets
            For j = 1 To NYears
                If FblockCode(i, j) > maxblkNumber Then maxblkNumber = CInt(FblockCode(i, j))
            Next
        Next

        'ParNumber() needs to be indexed by the max value in FblockCode(i, j) 
        'because it turns FblockCode(i, j) into the sequential index of 1 to nBlocks(variables to be varied)
        ReDim ParNumber(maxblkNumber)
        'block number could be dimed by nblocks but we don't know what that is at this time
        ReDim BlockNumber(maxblkNumber)

        n = 0
        For i = 1 To NumFleets
            For j = 1 To NYears
                If FblockCode(i, j) > 0 And ParNumber(CInt(FblockCode(i, j))) = 0 Then
                    n = n + 1
                    'keep that actual block number 
                    'so it can be used by the result object to turn and BlockResults(iBlock) index back into its original BlockNumber
                    BlockNumber(n) = FblockCode(i, j)

                    'the sequential parameter number for this FblockCode(i, j)
                    ParNumber(FblockCode(i, j)) = n
                End If
            Next
        Next

        Debug.Assert(n <= maxblkNumber, "SetFletchPars() nblocks to large!!!.")
        Return n

    End Function
    ''' <summary>
    ''' Sets the search blocks to the minimum number needed to have the search turned on
    ''' </summary>
    ''' <remarks>Used when the search blocks need to be dimensioned but not used e.g. Ecoseed</remarks>
    Public Sub setMinSearchBlocks()

        Me.nBlocks = 1
        ReDim FblockCode(NumFleets, NYears)

    End Sub

    ''' <summary>
    ''' Set default FBlockCodes to a unique code for each fleet
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub setDefaultFBlockCodes()

        For iFlt As Integer = 1 To NumFleets
            For j As Integer = 2 To NYears
                FblockCode(iFlt, j) = iFlt
            Next
        Next

    End Sub


    Public Sub setRandomFRates()
        Dim rd As New Random

        For i As Integer = 1 To nBlocks
            Frates(i) = -1 + 2 * rd.NextDouble
        Next

    End Sub


    Public Sub setBaseYearEffort(ByRef EcosimData As cEcosimDatastructures)
        If BaseYear = 0 Then BaseYear = 1
        For iflt As Integer = 1 To NumFleets
            BaseYearEffort(iflt) = EcosimData.FishRateGear(iflt, 12 * BaseYear - 11)
            If BaseYearEffort(iflt) = 0 Then BaseYearEffort(iflt) = 1
        Next

    End Sub

    ''' <summary>
    ''' Save the Fishing Rates (Frates(nBlocks)) before they have been changed by the Search Algo 
    ''' so they can be set back to initial values at the start of each Search Run.
    ''' </summary>
    ''' <param name="EcosimData"></param>
    ''' <remarks></remarks>
    Public Sub saveInitialFishingRate(ByRef EcosimData As cEcosimDatastructures)
        Dim j As Integer, i As Integer
        Dim iyr As Integer

        Try
            'clear out the old data
            setDefaultFRates()

            'needed for EcopathBaseF get the Ecopath fishing rate for the base year
            setBaseYearEffort(EcosimData)

            If Me.InitOption = eInitOption.EcopathBaseF Then

                For i = 1 To m_NBlocks : m_storedFrates(i) = 0.01 : Next

                If BaseYear > 0 Then 'in EwE5 baseYear could not be zero when this is set
                    j = 1
                    For i = 1 To NumFleets
                        For iyr = 1 To NYears 'so that it takes the F in the first year with color coding
                            If ParNumber(FblockCode(i, iyr)) = j Then
                                If BaseYearEffort(i) > 0 Then m_storedFrates(j) = CSng(Math.Log(BaseYearEffort(i) * 1.001))
                                j = j + 1
                            End If
                        Next
                    Next
                Else
                    For i = 1 To m_NBlocks : m_storedFrates(i) = 0.0001 : Next
                End If


            ElseIf Me.InitOption = eInitOption.CurrentF Then

                'Fishing Rates from 
                For j = 1 To m_NBlocks : m_storedFrates(j) = 0.01 : Next
                j = 1
                For i = 1 To NumFleets
                    For iyr = 1 To NYears 'so that it takes the F in the first year with color coding
                        If ParNumber(FblockCode(i, iyr)) = j Then
                            If EcosimData.FishRateGear(i, 12 * iyr - 11) > 0 Then m_storedFrates(j) = CSng(Math.Log(EcosimData.FishRateGear(i, 12 * iyr - 11)))
                            j = j + 1
                        End If
                    Next
                Next

            ElseIf Me.InitOption = eInitOption.RandomF Then

                'Random fishing rates are set each run so no need to save the initial values

            End If


        Catch ex As Exception
            cLog.Write(ex)
            Throw New ApplicationException("Error setting fishing rate to initial values.", ex)
        End Try


    End Sub

    ''' <summary>
    ''' Restore the Fishing Rates (Frates()) to there initial values saved via saveInitialFishingRate
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub restoreSavedFishingRates()

        Try

            If InitOption = eInitOption.RandomF Then
                'get a new set of random fishing rates
                setRandomFRates()
            Else
                Array.Clear(Frates, 0, Frates.Length)
                For i As Integer = 1 To m_NBlocks : Frates(i) = m_storedFrates(i) : Next
                '  Array.Copy(m_storedFrates, Frates, Frates.Length)

            End If

        Catch ex As Exception
            cLog.Write(ex)
            Throw New ApplicationException("restoreSavedFishingRates()", ex)
        End Try

    End Sub


    Public Sub setMaxEffort(ByVal nSearchBlocks As Integer)
        MaxEffort = 10000
        For i As Integer = 1 To nSearchBlocks
            If Frates(i) * 3 > Math.Log(MaxEffort) Then
                MaxEffort = CSng(Math.Exp(Frates(i) * 3))
            End If
        Next
    End Sub

    Public Sub setLimitFishingMortality()
        LimitFishingMortality = False
        For i As Integer = 1 To NumLiving
            If FLimit(i) < 1000 And FLimit(i) > 0 Then
                LimitFishingMortality = True
            End If
        Next
    End Sub


    Public Sub initForRun(ByVal EcoPathData As cEcopathDataStructures, ByVal EcosimData As cEcosimDatastructures)

        ReDim ValCatch(EcoPathData.NumFleet, EcoPathData.NumGroups)
        ReDim NetCost(EcoPathData.NumFleet)
        ReDim ValCatchGear(EcoPathData.NumFleet)

        ReDim CatchYear(NumFleets, NumGroups)
        ReDim CatchYearGroup(NumGroups)
        ReDim FishYear(NumGroups)

        For igrp As Integer = 1 To EcoPathData.NumGroups
            CatchYearGroup(igrp) = EcoPathData.fCatch(igrp)
            For iflt As Integer = 1 To EcoPathData.NumFleet
                CatchYear(iflt, igrp) = EcoPathData.Landing(iflt, igrp)
            Next
        Next

        m_ecopathData = EcoPathData
        m_ecosimData = EcosimData

        Din = 1 - DiscountFactor 'jb DiscountFactor was set to a default of 0.01 in setDefaultParamaters
        If GenDiscountFactor > 0 Then

            Me.Dfgrate = 1.0F / (1 + GenDiscountFactor)
            Me.Drate = 1.0F / (1 + DiscountFactor)
            'if Dfgrate = Drate then tweak Dfgrate so calcDiscoutRate(t) can use the same equation for both cases
            If Dfgrate = Drate Then Dfgrate += CSng(0.0000001)
            Me.deltaDDfg = Dfgrate / Drate
            Me.GenT = 20.0F

            Dgen = 1 - GenDiscountFactor / 20
            If Din <> 0 Then Dratio = Dgen / Din Else Dratio = CSng(Dgen / 0.01)
            If Dratio = 1 Then Dratio = 0.9999
            If Dgen <= 0 Then Dgen = 0.01
            Dalpha = Dratio / (20 * (1 - Dratio))
        End If

        Ecodistance = 0
        ExistValue = 0
        ecovalue = 0
        totval = 0
        profit = 0
        Employ = 0
        manvalue = 0
        DiversityIndex = 0

    End Sub


#End Region

#Region "Calculations and summary"

    ''' <summary>
    ''' Caculate NetCost from Fgear()
    ''' </summary>
    ''' <param name="Fgear">Fishing Effort</param>
    ''' <param name="iYear"></param>
    ''' <remarks>Calculates NetCost() </remarks>
    Public Sub calcNetCost(ByRef Fgear() As Single, ByVal iYear As Integer)

        'calculate discount factor DF
        'VC080523: if Year>1 then DF returns a value bigger than 1. this means cost will be increasing in the future, 
        'but given that we're calculating present value, shouldn't it be decreasing instead?
        'Should we be dividing with DF in the calc's below?
        'I'm making the DF its inverse 

        If iYear > BaseYear Then
            DF = calcDiscountRate(iYear - BaseYear)
        Else
            DF = 1
        End If
        'DF = 1

        For iFlt As Integer = 1 To m_ecopathData.NumFleet

            If BaseYearCost(iFlt) > 0 Then
                NetCost(iFlt) += DF * BaseYearCost(iFlt) * (Fgear(iFlt) / BaseYearEffort(iFlt))
            End If

        Next iFlt

    End Sub

    ''' <summary>
    ''' Set Fishing Effort for a search 
    ''' </summary>
    ''' <param name="Fgear"></param>
    ''' <param name="RelFopt"></param>
    ''' <param name="iYear"></param>
    Public Sub SetFGear(ByRef Fgear() As Single, ByVal RelFopt() As Single, ByVal iYear As Integer)

        'When Ecosim is run for a Fishing Policy Search it is run for 20 years past the end of the regular run length(cSearchDataStructures.ExtraYearsForSearch)
        'Constrain the year index to the Ecosim run length so the effort set by the FPS for the last year is used for the extra years
        Dim iyf As Integer = CInt(if(iYear <= m_ecosimData.NumYears, iYear, m_ecosimData.NumYears))

        Debug.Assert(Me.SearchMode = eSearchModes.FishingPolicy, "Ecosim BUG warning: setting fishing effort to values computed by Fishing Policy Search when not in search!")

        'in the Fishing policy search
        'If the search has set an effort then use that value to populate Fgear()
        For iFlt As Integer = 1 To m_ecopathData.NumFleet

            If FblockCode(iFlt, iyf) > 0 Then  'And bBaseYearSet?
                'Fishing policy search has set an effort 
                'copy that value into Fgear(nfleets)
                Fgear(iFlt) = RelFopt(ParNumber(FblockCode(iFlt, iyf)))
            Else
                'No effort has been set by the Fishing policy search
                'use the effort in FishRateGear()
                Fgear(iFlt) = m_ecosimData.FishRateGear(iFlt, 12 * iyf - 11)
            End If

        Next iFlt

    End Sub


    Public Sub ClearYearlyData()
        Array.Clear(CatchYear, 0, CatchYear.Length)
        Array.Clear(CatchYearGroup, 0, CatchYearGroup.Length)
    End Sub

    ''' <summary>
    ''' Calculates DF (Discount factor), Fgear(), Qyear(),  and FishYear() for Ecospace
    ''' </summary>
    ''' <param name="iYear"></param>
    ''' <param name="RelFopt"></param>
    Public Sub YearTimeStepEcoSpace(ByVal Biomass() As Single, ByRef Fgear() As Single, ByVal iYear As Integer, ByVal nWaterCells As Integer, ByVal RelFopt() As Single)

        ' Sanity checks
        Debug.Assert(Me.bInSearch, "Not in search?!")

        Try

            Me.ClearYearlyData()

            If Me.bInSearch Then
                'in some search mode so collect the summary data

                'calculate discount factor DF
                If iYear > BaseYear Then
                    DF = calcDiscountRate(iYear - BaseYear)
                Else
                    DF = 1
                End If
                'DF = 1

                For i As Integer = 1 To m_ecopathData.NumFleet

                    If FblockCode(i, iYear) > 0 Then
                        Fgear(i) = RelFopt(ParNumber(FblockCode(i, iYear)))
                    Else
                        Fgear(i) = m_ecosimData.FishRateGear(i, 12 * iYear - 11)
                    End If

                    For j As Integer = 1 To m_ecopathData.NumGroups
                        FishYear(j) = FishYear(j) + Fgear(i) * m_ecosimData.relQ(i, j)
                    Next j
                Next i

            End If ' If bInSearch Then

        Catch ex As Exception
            cLog.Write(ex)
            Throw New ApplicationException("Ecospace.YearTimeStepEcoSpace() Error: " & ex.Message, ex)
        End Try

    End Sub


    ''' <summary>
    ''' Calculate yearly summary data based on Biomass for this time step
    ''' </summary>
    ''' <param name="Biomass"></param>
    ''' <remarks>calculates ecovalue, manvalue, Ecodistance and ExistValue used for the Objective function</remarks>
    Public Sub calcYearlySummaryValues(ByVal Biomass() As Single)
        Dim i As Integer

        Dim LogB As Single
        Dim EcoDistTime As Single
        For i = 1 To m_ecopathData.NumLiving
            'Ecovalue = Ecovalue - BGoalValue(i) * (bb(i) / m_data.StartBiomass(i) - BGoal(i)) ^ 2
            ecovalue = ecovalue + BGoalValue(i) * Biomass(i) / m_ecosimData.StartBiomass(i)
            LogB = CSng(Math.Log(Biomass(i) / m_ecosimData.StartBiomass(i)))
            EcoDistTime = CSng(EcoDistTime + LogB ^ 2)
            ExistValue = ExistValue + LogB * BGoalValue(i)

            If MGoalValue(i) > 0 Then
                If Biomass(i) / m_ecosimData.StartBiomass(i) < MGoalValue(i) Then 'not yet reached the mandated threshold
                    manvalue = manvalue + Biomass(i) / m_ecosimData.StartBiomass(i)
                Else
                    manvalue = manvalue + MGoalValue(i)
                End If
            End If

        Next i

        'System.Console.WriteLine(ecovalue.ToString)
        Select Case Me.m_ecopathData.DiversityIndexType
            Case eDiversityIndexType.KemptonsQ
                DiversityIndex = DiversityIndex + Me.m_EcoFunctions.KemptonsQ(Me.m_ecopathData.NumLiving, Me.m_ecopathData.TTLX, Biomass, 0.25)
            Case eDiversityIndexType.Shannon
                DiversityIndex = DiversityIndex + Me.m_EcoFunctions.ShannonDiversityIndex(Me.m_ecopathData.NumLiving, Biomass)
        End Select

        EcoDistTime = CSng(Math.Sqrt(EcoDistTime))
        Ecodistance = Ecodistance + DF * EcoDistTime

        'For i = 1 To m_ecopathData.NumLiving
        '    'Ecovalue = Ecovalue - BGoalValue(i) * (bb(i) / m_data.StartBiomass(i) - BGoal(i)) ^ 2
        '    If MGoalValue(i) > 0 Then
        '        If Biomass(i) / m_ecosimData.StartBiomass(i) < MGoalValue(i) Then 'not yet reached the mandated threshold
        '            manvalue = manvalue + Biomass(i) / m_ecosimData.StartBiomass(i)
        '        Else
        '            manvalue = manvalue + MGoalValue(i)
        '        End If
        '    End If
        'Next i

    End Sub


    Public Sub calcBaseYearCost(ByVal iYear As Integer, ByVal nSpatialCells As Integer)

        Dim CV As Single

        'xxxxxx EwE6 and EwE5 differences BaseYearIncome
        'jb removed BaseYearIncome() because it was not used for anything other than testing if it had been calculated
        'and would cause a crash if NumFleet = 0 in BaseYearIncome(1) = 0 BaseYearIncome() was dimmed by NumFleet
        'Here it is replaced with bBaseYearSet to set the base year only once at the start of a run

        If iYear = BaseYear And Not bBaseYearSet Then
            bBaseYearSet = True
            For i As Integer = 1 To m_ecopathData.NumFleet

                CV = 0
                For j As Integer = 1 To m_ecopathData.NumLiving
                    CV = CV + CatchYear(i, j) / nSpatialCells * m_ecopathData.Market(i, j) '* m_ecopathData.PropLanded(i, j)
                Next
                '   m_search.BaseYearIncome(i) = CV
                BaseYearCost(i) = CV * (m_ecopathData.CostPct(i, 2) + m_ecopathData.CostPct(i, 3)) / 100
                '061221VC: the baseyearcost above doesn't include fixed costs
                'this is probably how it should be, as it troublesome to include it in all
                'other cost calculations in the optimizations, it should [costpct(i,1)]
            Next
        End If

        'jb CatchCost is never used for anything
        ''Get the cost and value by year for later plotting
        'If m_search.BaseYearCost(1) > 0 And iyr <= TotalTime Then
        '    CatchVal(iyr) = CV
        '    For i = 1 To NumGear
        '        CatchCost(i) = Fgear(i) * m_search.BaseYearCost(i) / m_search.BaseYearEffort(i)
        '    Next
        'End If
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    End Sub

    Public Sub EcosimSummarizeIndicators(ByVal Biomass() As Single, ByVal Fgear() As Single, _
                                            ByVal ModelRunLength As Integer, ByVal ModelRunLengthPostBaseYear As Integer, _
                                            ByVal PriceMedData As cMediationDataStructures)
        Dim LTV As Single 'Long term value
        Dim CV As Single
        Dim iFlt As Integer, iGrp As Integer
        Dim CostPenalty As Single
        Dim CostPenaltyConstant As Integer = 1

        ReDim CostRatio(m_ecopathData.NumFleet)

        totval = 0
        Employ = 0
        profit = 0

        DiversityIndex = DiversityIndex / ModelRunLength

        ' calculate last year incomes and costs by gear
        For iFlt = 1 To m_ecopathData.NumFleet
            'If BaseYearCost(i) > 0 Then
            'LastYearCost(i) = Fgear(i) * BaseYearCost(i) / BaseYearEffort(i)
            'Else
            '    LastYearCost(i) = Fgear(i) * (m_ecopathData.cost(i, 2) + m_ecopathData.cost(i, 3))
            'End If

            CV = 0
            For iGrp = 1 To m_ecopathData.NumLiving
                If CatchYear(iFlt, iGrp) > 0 Then
                    LastYearIncomeSpecies(iFlt, iGrp) = CatchYear(iFlt, iGrp) * m_ecopathData.Market(iFlt, iGrp) * m_ecopathData.PropLanded(iFlt, iGrp)
                    CV = CV + LastYearIncomeSpecies(iFlt, iGrp)
                End If
            Next
            LastYearIncome(iFlt) = CV
        Next

        ExistValue = ExistValue / (m_ecopathData.NumLiving * ModelRunLengthPostBaseYear)

        LTV = CalcLTV(ModelRunLengthPostBaseYear)

        For iFlt = 1 To m_ecopathData.NumFleet
            For iGrp = 1 To m_ecopathData.NumLiving
                If ValCatch(iFlt, iGrp) > 0 Then
                    'Was done using end biomass:
                    'Vlocal = (ValCatch(i, j) + bb(j) * Fgear(i) * relQ(i, j) * LTV) * Market(i, j)
                    'In connection with including discounting during run changed the long term
                    'addition to be based on equilibrium biomass
                    'If Abs((bb(j) - biomeq(j)) / m_data.StartBiomass(j)) > 1 Then Stop

                    'Dim Vlocal As Single = ValCatch(iFlt, iGrp) + (m_ecopathData.PropLanded(iFlt, iGrp) * Biomass(iGrp) _
                    '* Fgear(iFlt) * m_ecosimData.relQ(iFlt, iGrp) * LTV) * m_ecopathData.Market(iFlt, iGrp)

                    'jb 27-Apr-2011 changed again to include Price Elasticity of Supply
                    'Assumes 
                    'PriceMedData.SetPriceMedFunctions() was called with the landings from the last timestep in Ecosim
                    'Both ValCatch(fleets,groups) and CatchYear(fleets,groups) are from the last Ecosim timestep 
                    Dim Vlocal As Single = ValCatch(iFlt, iGrp) + CatchYear(iFlt, iGrp) * m_ecopathData.Market(iFlt, iGrp) * PriceMedData.getPESMult(iGrp, iFlt) * LTV

                    totval = totval + Vlocal
                    ValCatchGear(iFlt) = ValCatchGear(iFlt) + Vlocal
                End If
            Next
        Next

        For iFlt = 1 To m_ecopathData.NumFleet

            'NetCost() = [Sum of NetCost] + [long term value of the last time step]
            'totval includes the same accounting
            If BaseYearCost(iFlt) > 0 And BaseYearEffort(iFlt) > 0 Then
                NetCost(iFlt) += Fgear(iFlt) * BaseYearCost(iFlt) / BaseYearEffort(iFlt) * LTV
            End If

            CostPenalty = 0
            If ValCatchGear(iFlt) > 0 And UseCostPenalty Then
                CostRatio(iFlt) = NetCost(iFlt) / ValCatchGear(iFlt)
                If CostRatio(iFlt) < 3.0# Then
                    CostPenalty = CSng(CostPenaltyConstant * CostRatio(iFlt) ^ 7)
                    If CostRatio(iFlt) < 3 Then
                        CostPenalty = CSng(CostPenaltyConstant * CostRatio(iFlt) ^ 7)
                    Else
                        CostPenalty = CSng(CostPenaltyConstant * 3 ^ 7 + 1000 * (CostRatio(iFlt) - 3))
                    End If
                End If ' If ValCatchGear(i) > 0 And UseCostPenalty Then
                '      System.Console.WriteLine("Cost R and P = " & CostRatio(i).ToString & ", " & CostPenalty)
            End If
            totval = totval - NetCost(iFlt) - CostPenalty

            Employ = Employ + (ValCatchGear(iFlt) - CostPenalty) * Jobs(iFlt)
            ValCatchGear(iFlt) = ValCatchGear(iFlt) - NetCost(iFlt) - CostPenalty

        Next

    End Sub


    Public Sub EcoSpaceSummarizeIndicators(ByVal Fgear() As Single, ByVal ModelRunLength As Integer, _
                                           ByVal ModelRunLengthPostBaseYear As Integer, _
                                           ByVal nWaterCells As Integer)
        Dim LTV As Single 'Long term value
        Dim iflt As Integer, igrp As Integer

        Dim CostPenaltyConstant As Integer = 1

        totval = 0
        Employ = 0

        ExistValue = ExistValue / (m_ecopathData.NumLiving * ModelRunLength)

        'Diversity index (either KemptonQ or Shannon index) is the sum of KemptonQ across all time steps
        DiversityIndex = DiversityIndex / ModelRunLength

        LTV = CalcLTV(ModelRunLengthPostBaseYear)

        For iflt = 1 To m_ecopathData.NumFleet
            For igrp = 1 To m_ecopathData.NumLiving
                If ValCatch(iflt, igrp) > 0 Then
                    ValCatch(iflt, igrp) = ValCatch(iflt, igrp) / nWaterCells
                    'Was done using end biomass:
                    'Vlocal = (ValCatch(i, j) + bb(j) * Fgear(i) * relQ(i, j) * LTV) * Market(i, j)
                    'In connection with including discounting during run changed the long term
                    'addition to be based on equilibrium biomass
                    'If Abs((bb(j) - biomeq(j)) / m_data.StartBiomass(j)) > 1 Then Stop

                    Dim Vlocal As Single = ValCatch(iflt, igrp) + CatchYear(iflt, igrp) / nWaterCells * LTV * m_ecopathData.Market(iflt, igrp)

                    totval = totval + Vlocal
                    ValCatchGear(iflt) = ValCatchGear(iflt) + Vlocal

                    System.Console.Write(CSng(ValCatch(iflt, igrp) / ModelRunLengthPostBaseYear).ToString & ", ")
                End If
                'employ = employ + Vlocal * Jobs(i)
            Next
            System.Console.WriteLine()
        Next

        Dim CostPenalty As Single ', TotalFishingCost As Single
        ReDim CostRatio(m_ecopathData.NumFleet)
        For iflt = 1 To m_ecopathData.NumFleet

            If BaseYearCost(iflt) > 0 And BaseYearEffort(iflt) > 0 Then
                NetCost(iflt) += Fgear(iflt) * BaseYearCost(iflt) / BaseYearEffort(iflt) * LTV
            End If


            'NetCost() = [Sum of NetCost] + [long term value of the last time step]
            'totval includes the same accounting
            '  NetCost(iflt) = NetCost(iflt) + Fgear(iflt) * nWaterCells * (m_ecopathData.cost(iflt, 2) + m_ecopathData.cost(iflt, 3)) * LTV
            ' TotalFishingCost = TotalFishingCost + NetCost(iflt)

            CostPenalty = 0
            If ValCatchGear(iflt) > 0 And UseCostPenalty Then
                CostRatio(iflt) = NetCost(iflt) / ValCatchGear(iflt)
                If CostRatio(iflt) < 3.0# Then
                    CostPenalty = CSng(CostPenaltyConstant * CostRatio(iflt) ^ 50)
                Else
                    CostPenalty = CSng(CostPenaltyConstant * 3.0! ^ 50.0! + 1000.0! * (CostRatio(iflt) - 3.0!))
                End If
            End If

            totval = totval - NetCost(iflt) - CostPenalty

            Employ = Employ + (ValCatchGear(iflt) - CostPenalty) * Jobs(iflt)
            ValCatchGear(iflt) = ValCatchGear(iflt) - NetCost(iflt) - CostPenalty
        Next

    End Sub

    ''' <summary>
    ''' Calculate long term value, based on either generational or standard discounting
    ''' </summary>
    ''' <param name="YearPastBaseYear"></param>
    ''' <returns></returns>
    Public Function CalcLTV(ByVal YearPastBaseYear As Integer) As Single
        Dim LTV As Single
        If GenDiscountFactor > 0 Then

            'Use the Intergenerational discount for the extra years of the run
            'For iyr As Integer = Me.m_ecosimData.NumYears To Me.m_ecosimData.NumYears + ExtraYearsForSearch
            '    LTV += calcDiscountRate(iyr)
            'Next

            'Long term value using the Intergenerational discount rate from EwE5
            If DiscountFactor > 0 Then
                'LTV = DF / DiscountFactor
                LTV = CSng((1 + Dalpha) / DiscountFactor * Din ^ YearPastBaseYear - Dalpha * (Dgen) ^ YearPastBaseYear / (1 - Dgen))
            Else
                If Dgen <> 1 Then LTV = CSng((1 + Dalpha) / 0.01 * Din ^ YearPastBaseYear - Dalpha * Dgen ^ YearPastBaseYear / (1 - Dgen))
            End If
        Else
            'using standard discounting, take last years catch and discount it for 20 years, multiplying it with this factor:
            LTV = 0
            For i As Integer = 0 To 19
                LTV += CSng((1 + DiscountFactor) ^ -i)
            Next

        End If

        'System.Console.WriteLine("LTV = " & LTV.ToString)
        Return LTV
    End Function


    ''' <summary>
    ''' JB 3-Nov-201 GenDiscountFactor removed from interface until we get this sorted out
    ''' 
    ''' Calculates discount factor based on either generational (if GenDiscountFactor>0) or standard discounting. 
    ''' For some reason carl has made this a calculation of future value not of present. 
    '''     ''' if Dgenfactor = 0 then uses Traditional discount factor calculating present value of future cost and revenue
    ''' Enter here if GenDiscountFactor = 0 
    ''' </summary>
    ''' <param name="iYear"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function calcDiscountRate(ByVal iYear As Integer) As Single
        Dim df As Single

        If GenDiscountFactor > 0 Then

            'From 
            'Intergenerational discounting: a new intuitive approach
            'Ussif R. Sumaila, Carl Walters

            'If Dfgrate = Drate then Dfgrate should have been altered by a small amount see cSearchDataStructures.initForRun()
            'so we can use the same equation for both cases
            Debug.Assert(deltaDDfg <> 1, Me.ToString & ".calcDiscountRate() Dfgrate and Drate can not be equal. Check cSearchDataStructures.InitForRun()")

            df = CSng(Drate ^ iYear + Dfgrate * Drate ^ (iYear - 1) / GenT * ((1 - deltaDDfg ^ iYear) / (1 - deltaDDfg)))

            'Carls original generational discount returns an increasing DF (from EwE5)
            'DF in Year 1 = 1.0 , year 2 = 1.009...
            'If Dgen = Din Then
            '    df = CSng(Din ^ (iYear - 1) + (Dgen * (Din ^ (iYear - 2)) / 20) * (iYear - 1))
            'Else
            '    df = CSng((1 + Dalpha) * Din ^ (iYear - 1) - Dalpha * (Din * Dratio) ^ (iYear - 1))
            'End If
            'df = 1 - (df - 1)

        Else    'traditional discounting

            'Present Value = [Value at t] / (1 + [Interest rate]) ^ t
            'this returns the multiplier
            df = CSng(1.0F / (1 + DiscountFactor) ^ iYear)
            ' Villy's discount factor always returns 1
            'df = CSng(1 ^ -(iYear - 1))
        End If

        'System.Console.WriteLine("Discount rate = " & df.ToString & " at " & iYear.ToString)
        Return df

    End Function


#End Region

End Class



