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

Option Strict On

Imports System

Namespace Core

#Region " Core execution state "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type identifying known core states.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eCoreExecutionState As Integer
        ''' <summary>The core is initialized and ready for use</summary>
        Idle
        ''' <summary>Ecopath model data has been loaded</summary>
        EcopathLoaded
        ''' <summary>Ecopath model data has been initialized</summary>
        EcopathInitialized = EcopathLoaded
        ''' <summary>Ecopath scenario is ready to run</summary>
        EcopathRunning
        ''' <summary>Ecopath model run is completed</summary>
        EcopathCompleted
        ''' <summary>Ecopath PSD model run is completed</summary>
        PSDCompleted
        ''' <summary>Ecotracer scenario data has been loaded</summary>
        EcotracerLoaded
        ''' <summary>Ecosim scenario data has been loaded</summary>
        EcosimLoaded
        ''' <summary>Ecosim scenario has been initialized</summary>
        EcosimInitialized
        ''' <summary>Ecosim scenario is running</summary>
        EcosimRunning
        ''' <summary>Ecosim scenario run is completed</summary>
        EcosimCompleted
        ''' <summary>Ecospace scenario data has been loaded</summary>
        EcospaceLoaded
        ''' <summary>Ecospace scenario has been initialized</summary>
        EcospaceInitialized = EcospaceLoaded
        ''' <summary>Ecospace scenario is running</summary>
        EcospaceRunning
        ''' <summary>Ecospace scenario run is completed</summary>
        EcospaceCompleted
    End Enum

    Public Enum eEcotracerRunState As Integer
        None
        Ecosim
        Ecospace
    End Enum

#End Region ' Core execution state

#Region " Variable names "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type used for exposing variables a.k.a. parameters provided by
    ''' the Core models.
    '''</summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eVarNameFlags As Integer

        ''' <summary>Variable name is not specified</summary>
        NotSet
        ''' <summary>Production over Biomass (ratio)</summary>
        ''' <remarks>Also referred to as Mortality or Z.</remarks>
        PBInput
        ''' <summary>Production over Biomass (computed)</summary>
        PBOutput
        ''' <summary>Ecotrophic Efficiency</summary>
        EEInput
        ''' <summary>Ecotrophic Efficiency (computed)</summary>
        EEOutput
        ''' <summary>Consumption over Biomass</summary>
        QBInput
        ''' <summary>Consumption over Biomass (computed)</summary>
        QBOutput
        ''' <summary>Gross efficiency (P/Q)</summary>
        GEInput
        ''' <summary>Gross efficiency (P/Q, computed)</summary>
        GEOutput

        ''' <summary>Generic item names</summary>
        Name
        ''' <summary>Numerical position of an item in a list</summary>
        ''' <remarks>This value has replaced former EwE5 indices such as iGroup.</remarks>
        Index
        ''' <summary>Model area</summary>
        Area
        ''' <summary>Habitat area fraction</summary>
        HabitatArea
        ''' <summary>Biomass</summary>
        Biomass
        ''' <summary><see cref="eVarNameFlags.Biomass">Biomass</see> per <see cref="eVarNameFlags.HabitatArea">Area</see></summary>
        BiomassAreaInput
        ''' <summary><see cref="eVarNameFlags.Biomass">Biomass</see> per <see cref="eVarNameFlags.HabitatArea">Area</see></summary>
        BiomassAreaOutput
        ''' <summary>Net. biomass accumulation as entered by the user</summary>
        BioAccumInput
        ''' <summary>Biomass accumulation rate per year</summary>
        BioAccumRatePerYear
        ''' <summary>Net. biomass accumulation</summary>
        BioAccumOutput
        ''' <summary>To document</summary>
        GS
        ''' <summary>To document</summary>
        DetImp
        ''' <summary>Trophic Level</summary>
        TTLX
        ''' <summary>Immigration</summary>
        Immig
        ''' <summary>Net. Emigration</summary>
        Emig
        ''' <summary>Emigration rate</summary>
        EmigRate
        ''' <summary>To document</summary>
        BioAccumRate
        ''' <summary>Diet composition</summary>
        DietComp
        ''' <summary>To document</summary>
        ImpDiet
        ''' <summary>To document</summary>
        DetritusFate
        'Fleet definition table parameter names; Added by FG on Jan 26 2006
        ''' <summary>To document</summary>
        FixedCost
        ''' <summary>To document</summary>
        CPUECost
        ''' <summary>To document</summary>
        SailCost
        ''' <summary>To document</summary>
        EPower
        ''' <summary>To document</summary>
        PcapBase
        ''' <summary>To document</summary>
        CapDepreciate
        ''' <summary>To document</summary>
        CapBaseGrowth
        'Mortality - Coefficients parameter names; Added by FG on Jan 26 2006
        ''' <summary>To document</summary>
        MortCoPB
        ''' <summary>To document</summary>
        MortCoFishRate
        ''' <summary>To document</summary>
        MortCoPredMort
        ''' <summary>To document</summary>
        MortCoBioAcumRate
        ''' <summary>To document</summary>
        MortCoNetMig
        ''' <summary>To document</summary>
        MortCoOtherMort
        ''' <summary>[Fishing Mort] / [Total mort]</summary>
        FishMortTotMort
        ''' <summary>1- FishMortTotMort</summary>
        NatMortPerTotMort
        ''' <summary>To document</summary>
        Consumption
        ''' <summary>To document</summary>
        ImportedConsumption
        ''' <summary>Predation mortality</summary>
        PredMort
        ''' <summary>To document</summary>
        Landings
        ''' <summary>To document</summary>
        Discards
        ''' <summary>To document</summary>
        OffVesselPrice
        ''' <summary>To document</summary>
        NonMarketValue
        ''' <summary>To document</summary>
        DiscardFate

        EcopathCatchTotalByFleetGroup

        EcopathCatchMortByFleetGroup

        EcopathLandingsByFleetGroup

        EcopathDiscardsByFleetGroup
        EcopathDiscardsMortByFleetGroup
        EcopathDiscardsSurvivalByFleetGroup

        ''' <summary>Sum of all consumption</summary>
        EcopathStatsTotalConsumption
        ''' <summary>Sum of all exports</summary>
        EcopathStatsTotalExports
        ''' <summary>Sum of all respiratory flows</summary>
        EcopathStatsTotalRespFlow
        ''' <summary>Sum of all flows into detritus</summary>
        EcopathStatsTotalFlowDetritus
        ''' <summary>Total system throughput</summary>
        EcopathStatsTotalThroughput
        ''' <summary>Sum of all production</summary>
        EcopathStatsTotalProduction
        ''' <summary>Mean trophic level of the catch</summary>
        EcopathStatsMeanTrophicLevelCatch
        ''' <summary>Gross efficiency (catch/net p.p.)</summary>
        EcopathStatsGrossEfficiency
        ''' <summary>Calculated total net primary production</summary>
        EcopathStatsTotalNetPP
        ''' <summary>Total primary production/total respiration</summary>
        EcopathStatsTotalPResp
        ''' <summary>Net system production</summary>
        EcopathStatsNetSystemProduction
        ''' <summary>Total primary production/total biomass</summary>
        EcopathStatsTotalPB
        ''' <summary>Total biomass/total throughput</summary>
        EcopathStatsTotalBT
        ''' <summary>Total biomass (excluding detritus)</summary>
        EcopathStatsTotalBNonDet
        ''' <summary>Total catches</summary>
        EcopathStatsTotalCatch
        ''' <summary>Connectance Index</summary>
        EcopathStatsConnectanceIndex
        ''' <summary>System Omnivory Index</summary>
        EcopathStatsOmnivIndex
        ''' <summary>Total market value</summary>
        EcopathStatsTotalMarketValue
        ''' <summary>Total shadow value</summary>
        EcopathStatsTotalShadowValue
        ''' <summary>Total value</summary>
        EcopathStatsTotalValue
        ''' <summary>Total fixed cost</summary>
        EcopathStatsTotalFixedCost
        ''' <summary>Total variable cost</summary>
        ''' <remarks>This variable may exist under a different name.</remarks>
        EcopathStatsTotalVarCost
        ''' <summary>Total cost</summary>
        EcopathStatsTotalCost
        ''' <summary>Profit</summary>
        EcopathStatsProfit
        ''' <summary>Pedigree index</summary>
        EcopathStatsPedigreeIndex
        ''' <summary>Pedigree CV</summary>
        EcopathStatsPedigreeCV
        ''' <summary>Measure of pedigree fit</summary>
        EcopathStatsMeasureOfFit
        ''' <summary>Selected diversity indicator</summary>
        EcopathStatsDiversity
        ''' <summary>To document</summary>
        MaxRelPB
        ''' <summary>To document</summary>
        MaxRelFeedingTime
        ''' <summary>To document</summary>
        FeedingTimeAdjRate
        ''' <summary>To document</summary>
        OtherMortFeedingTime
        ''' <summary>To document</summary>
        PredEffectFeedingTime
        ''' <summary>To document</summary>
        DenDepCatchability
        ''' <summary>To document</summary>
        QBMaxQBio
        ''' <summary>To document</summary>
        SwitchingPower
        ''' <summary>To document</summary>
        VulMult
        ''' <summary>To document</summary>
        MedFunctNumber
        ''' <summary>To document</summary>
        StepSize
        ''' <summary>To document</summary>
        Discount
        ''' <summary>To document</summary>
        EquilibriumStepSize
        ''' <summary>To document</summary>
        EquilMaxFishingRate
        ''' <summary>To document</summary>
        NumStepAvg
        ''' <summary>To document</summary>
        NutBaseFreeProp
        ''' <summary>To document</summary>
        NutForceFunctionNumber
        ''' <summary>To document</summary>
        NutPBMax
        ''' <summary>To document</summary>
        SystemRecovery
        ''' <summary>To document</summary>
        NudgeChecked
        ''' <summary>To document</summary>
        UseVarPQ
        ''' <summary>To document</summary>
        BiomassOn
        ''' <summary>To document</summary>
        EcoSimNYears
        ''' <summary>Maximum effort of a fleet</summary>
        MaxEffort
        ''' <summary>Quota type imposed on a fleet</summary>
        QuotaType
        ''' <summary>Quota set for a gear/group combination</summary>
        QuotaShare
        ''' <summary>Proportion of discards that dies</summary>
        DiscardMortality
        ''' <summary>BBase for target fishing mortality policy. Upper biomass boundary</summary>
        MSEBBase
        ''' <summary>BLimit for target fishing mortality policy. Lower biomass boundary</summary>
        MSEBLim
        ''' <summary>Mortality/Fmsy for target fishing mortality policy</summary>
        MSEFmax
        ''' <summary>Mortality when biomass at or below BLim(lower boundry)</summary>
        ''' <remarks>Added for MSEBatch command file.</remarks>
        MSEFmin
        ''' <summary>To document</summary>
        MSELowerLPEffort
        ''' <summary>To document</summary>
        MSEUpperLPEffort
        ''' <summary>
        ''' Foraging time adjustment cannot drop below 0.1 as was the case
        ''' in EwE all the way up to release 6.4. In later versions of EwE
        ''' the foraging time adjustment was allowed to drop lower.
        '''</summary>
        ''' <comment>
        ''' VC: Arrow lake model, big increase in prey, top predator foraging 
        ''' time can't go below 0.1, so changed bount to 0.01 
        ''' </comment>
        ForagingTimeLowerLimit
        ''' <summary>Contaminant tracing on/off for Ecosim</summary>
        ConSimOnEcoSim
        ''' <summary>Contaminant tracing on/off for Ecospace</summary>
        ConSimOnEcoSpace
        ''' <summary>Predict Ecosim Fishing Effort</summary>
        PredictEffort
        ''' <summary>Start of summary time period in years</summary>
        EcosimSumStart
        ''' <summary>end of summary time period in years</summary>
        EcosimSumEnd
        ''' <summary>number of time steps to summarize ecosim data over</summary>
        EcosimSumNTimeSteps
        ''' <summary>Database ID</summary>
        DBID
        ''' <summary>Percentage of primary production</summary>
        PP
        ''' <summary>Generic description</summary>
        Description
        ''' <summary>Number of digits to display</summary>
        NumDigits
        ''' <summary>Group display digits</summary>
        GroupDigits
        ''' <summary>Unit enumerated value for text-based values</summary>
        UnitTime
        ''' <summary>Unit text for time-based values</summary>
        UnitTimeCustomText
        ''' <summary>Unit enumerated value for area-based values</summary>
        UnitArea
        ''' <summary>Unit text for area-based values</summary>
        UnitAreaCustomText
        ''' <summary>Unit enumerated value for currency-based values</summary>
        UnitCurrency
        ''' <summary>Unit text for currency-based values</summary>
        UnitCurrencyCustomText
        ''' <summary>Unit enumerated value for monetary values</summary>
        UnitMonetary
        ''' <summary>Unit value for georeferencing</summary>
        UnitMapRef
        ''' <summary>Unit value for environmental drivers</summary>
        UnitEnvDriver
        ''' <summary>Author of an EwE component</summary>
        Author
        ''' <summary>Contact info of an EwE component</summary>
        Contact
        ''' <summary>Julian day an EwE component was last saved</summary>
        LastSaved
        ''' <summary>To document</summary>
        NetMigration
        ''' <summary>To document</summary>
        FlowToDet
        ''' <summary>To document</summary>
        NetEfficiency
        ''' <summary>To document</summary>
        OmnivoryIndex
        ''' <summary>To document</summary>
        Respiration
        ''' <summary>To document</summary>
        Assimilation
        ''' <summary>Resp / Assim</summary>
        RespAssim
        ''' <summary>Prod / Resp</summary>
        ProdResp
        ''' <summary>Resp / Biomass</summary>
        RespBiom
        ''' <summary>To document</summary>
        SearchRate
        ''' <summary>To document</summary>
        Hlap
        ''' <summary>To document</summary>
        Plap
        ''' <summary>Colour value to represent an exposed core I/O object</summary>
        PoolColor
        ''' <summary>To document</summary>
        Alpha ' Borrowed from EwE5 EcoRanger
        ''' <summary>Recruitment power</summary>
        RecPowerSplit
        ''' <summary>Relative biomass accumulation rate (ratio)</summary>
        BABsplit
        ''' <summary>Weight at maturity over weight at infancy (ratio)</summary>
        WmatWinf
        ''' <summary>Forcing function number for hathery stocking (scalar)</summary>
        HatchCode
        ''' <summary>Stanza use fixed fecundity</summary>
        FixedFecundity
        ''' <summary>To document</summary>
        EggAtSpawn
        ''' <summary>Stanza parameter; used to indicate the group that leads 
        ''' <see cref="eVarNameFlags.Biomass">biomass</see> in a multi-stanza
        ''' configuration</summary>
        LeadingBiomass
        ''' <summary>Stanza parameter; used to indicate the group that leads 
        ''' <see cref="eVarNameFlags.QBInput">QB</see> in a multi-stanza
        ''' configuration</summary>
        LeadingCB
        ''' <summary>BaB * Bio</summary>
        Bat
        ''' <summary>Start age of a group in a stanza configuration (in months)</summary>
        StartAge
        ''' <summary>A multiplier to change the number of packets for the IBM model</summary>
        ''' <remarks>..but what about Dell? Acer? Toshiba? This is simply not fair!</remarks>
        PacketsMultiplier
        ''' <summary>Number of rows in the Ecospace basemap</summary>
        InRow
        ''' <summary>Number of columns in the Ecospace basemap</summary>
        InCol
        ''' <summary>Length of a cell (km)</summary>
        CellLength
        ''' <summary>Size of a cell (in map units)</summary>
        CellSize
        ''' <summary>Latitude of spatial data</summary>
        Latitude
        ''' <summary>Longitude of spatial data</summary>
        Longitude
        ''' <summary>Relative catchability per fleet/gear type (multiplier)</summary>
        EffectivePower
        ''' <summary>Base dispersal</summary>
        MVel
        ''' <summary>Relative dispersal in bad habitat</summary>
        RelMoveBad
        ''' <summary>Relative vulnerability in bad habitat</summary>
        RelVulBad
        ''' <summary>Relative feeding in bad habitat</summary>
        EatEffBad
        ''' <summary>To document</summary>
        IsAdvected
        ''' <summary>To document</summary>
        IsMigratory
        ''' <summary>To document</summary>
        PreferredHabitat
        ''' <summary>A habitat that a given fleet is allowed to fish in</summary>
        HabitatFishery
        ''' <summary>A MPA that a given fleet is allowed to fish in</summary>
        MPAFishery
        ''' <summary>Which months of the year a MPA is open for fishing</summary>
        MPAMonth
        ''' <summary>Ecospace cell depth assignments</summary>
        LayerDepth
        ''' <summary>Ecospace cell habitat assignments</summary>
        LayerHabitat
        ''' <summary>Ecospace cell habitat capacity, input</summary>
        LayerHabitatCapacityInput
        ''' <summary>Ecospace cell habitat capacity, computed</summary>
        LayerHabitatCapacity
        ''' <summary>Ecospace cell MPA assignments</summary>
        LayerMPA
        ''' <summary>Ecospace cell relative primary production</summary>
        LayerRelPP
        ''' <summary>Ecospace cell relative level of contaminants</summary>
        LayerRelCin
        ''' <summary>Ecospace cell region assignments</summary>
        LayerRegion
        ''' <summary>Ecospace cell migration assignments</summary>
        LayerMigration
        ''' <summary>Ecospace cell advection assignments</summary>
        LayerAdvection
        ''' <summary>Ecospace wind layer</summary>
        LayerWind
        ''' <summary>Ecospace upwelling layer</summary>
        LayerUpwelling
        ''' <summary>Ecospace MPA importance</summary>
        LayerImportance
        ''' <summary>Ecospace external driver layer</summary>
        LayerDriver
        ''' <summary>Ecospace cell port assignments</summary>
        LayerPort
        ''' <summary>Ecospace sailing cost</summary>
        LayerSail
        ''' <summary>To document</summary>
        LayerBiomassForcing
        ''' <summary>To document</summary>
        LayerBiomassRelativeForcing
        ''' <summary>Ecospace/MPA importance weight of the weight layer</summary>
        ImportanceWeight
        ''' <summary>Proportion of total habitat area by Habitat type</summary>
        HabAreaProportion
        ''' <summary>Ecospace excluded cells layer</summary>
        LayerExclusion
        ''' <summary>Total Effort multiplier</summary>
        SEmult
        ''' <summary>
        ''' Ecospace: Habitat-adjusted biomass = True. Ecopath base biomass = False
        '''</summary>
        AdjustSpace
        ''' <summary>Conversion factor for fishing effort</summary>
        FleetEffortConversion
        ''' <summary>Biomass map as computed by Ecospace.</summary>
        EcospaceMapBiomass
        ''' <summary>Catch map as computed by Ecospace.</summary>
        EcospaceMapCatch
        ''' <summary>Sum of effort map as computed by Ecospace.</summary>
        EcospaceMapSumEffort
        ''' <summary>Effort map as computed by Ecospace.</summary>
        EcospaceMapEffort
        ''' <summary>Shannon Diversity indicator map as computed by Ecospace.</summary>
        EcospaceMapShannonDiversity
        ''' <summary>KemptonsQ indicator map as computed by Ecospace.</summary>
        EcospaceMapKemptonsQ
        ''' <summary>To document</summary>
        EcospaceGroupBiomassStart
        ''' <summary>To document</summary>
        EcospaceGroupBiomassEnd
        ''' <summary>To document</summary>
        EcospaceGroupCatchStart
        ''' <summary>To document</summary>
        EcospaceGroupCatchEnd
        ''' <summary>To document</summary>
        EcospaceGroupValueStart
        ''' <summary>To document</summary>
        EcospaceGroupValueEnd
        ''' <summary>To document</summary>
        EcospaceFleetCatchStart
        ''' <summary>To document</summary>
        EcospaceFleetCatchEnd
        ''' <summary>To document</summary>
        EcospaceFleetValueStart
        ''' <summary>To document</summary>
        EcospaceFleetValueEnd
        ''' <summary>To document</summary>
        EcospaceFleetCostStart
        ''' <summary>To document</summary>
        EcospaceFleetCostEnd
        ''' <summary>Ecospace [Effort End] / [Effort Start]</summary>
        EcospaceFleetEffortES
        ''' <summary>Ecospace Catch by Fleet Time</summary>
        EcospaceFleetCatch
        ''' <summary>Ecospace Value by Fleet Time</summary>
        EcospaceFleetValue
        ''' <summary>Biomass of a group in a region for the start summary period</summary>
        EcospaceRegionBiomassStart
        ''' <summary>Biomass of a group in a region for the end summary period</summary>
        EcospaceRegionBiomassEnd
        ''' <summary>Biomass of catch in a region for the start summary period</summary>
        EcospaceRegionCatchStart
        ''' <summary>Biomass of catch in a region for the end summary period</summary>
        EcospaceRegionCatchEnd
        ''' <summary>Biomass of catch in a region by fleet, group, and time step</summary>
        EcospaceRegionFleetGroupCatch
        ''' <summary>Biomass of catch in a region by fleet, group, and year</summary>
        EcospaceRegionFleetGroupCatchYear
        ''' <summary>Time in Years of the Start summary time period</summary>
        EcospaceSummaryTimeStart
        ''' <summary>Time in Years of the End summary time period</summary>
        EcospaceSummaryTimeEnd
        ''' <summary>Number of time steps in the summary periods</summary>
        EcospaceNumberSummaryTimeSteps
        ''' <summary>Ecospace output biomass averaged over all the cells for each timestep</summary>
        EcospaceBiomassOverTime
        ''' <summary>Ecospace [computed biomass] / [base biomass] averaged over all the cells for each timestep</summary>
        EcospaceRelativeBiomassOverTime
        ''' <summary>Ecospace Catch over time</summary>
        EcospaceGroupCatchOverTime
        ''' <summary>Ecospace Value over time</summary>
        EcospaceGroupValueOverTime
        ''' <summary>Ecospace Biomass by region over time averaged over all the cells in a region for each timestep.
        ''' <seealso cref="EcospaceRegionBiomassYear"/></summary>
        EcospaceRegionBiomass
        ''' <summary>Ecospace Biomass by region over time averaged over all the cells in a region, per year.
        ''' <seealso cref="EcospaceRegionBiomass"/></summary>
        EcospaceRegionBiomassYear
        ''' <summary>Ecospace yearly average profit by fleet</summary>
        EcospaceFleetProfit
        ''' <summary>Ecospace yearly average jobs [value of catch] * [jobs]</summary>
        EcospaceFleetJobs
        ''' <summary>Number of fish in a monthly stanza age group</summary>
        StanzaNumberAtAge
        ''' <summary>Weight of individual fish in a monthly stanza age group</summary>
        StanzaWeightAtAge
        ''' <summary>Biomass in a monthly stanza age group [StanzaNumberAtAge]*[StanzaWeightAtAge]</summary>
        StanzaBiomassAtAge
        ''' <summary>Index to the Ecopath Groups in the Stanza Group</summary>
        StanzaGroup
        ''' <summary>Biomass for this a stanza iStanzaGroup</summary>
        StanzaBiomass
        ''' <summary>Consumption/Biomass for this a stanza iStanzaGroup Ecopath QB</summary>
        StanzaCB
        ''' <summary>Mortality for this a stanza iStanzaGroup Ecopath PB</summary>
        StanzaMortaility

        'StanzaVBGF

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'Ecospace multi thread vars
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''

        ''' <summary>To document</summary>
        nGridSolverThreads
        ''' <summary>To document</summary>
        nSpaceThreads
        ''' <summary>To document</summary>
        nEffortDistThreads
        ''' <summary>To document</summary>
        IFDPower
        ''' <summary>To document</summary>
        UseIBM
        ''' <summary>To document</summary>
        UseNewMultiStanza
        ''' <summary>Flag stating whether to use exact calculations or iterations for Ecospace migratory species</summary>
        UseExact
        ''' <summary>Ecospace run time</summary>
        TotalTime
        ''' <summary>Number of time steps per year</summary>
        NumTimeStepsPerYear
        ''' <summary>Ecospace Tolerance</summary>
        Tolerance
        ''' <summary>Ecospace successive over-relaxation</summary>
        SOR
        ''' <summary>Ecospace maximum number of iterations</summary>
        MaxIterations
        ''' <summary>How capacity is calculated in Ecospace</summary>
        EcospaceCapCalType
        ''' <summary>Use the default Ecospace output directory structure as defined by the core</summary>
        EcospaceUseCoreOutputDir
        ''' <summary>Save Ecospace out annually only</summary>
        EcospaceUseAnnualOutput
        ''' <summary>To document</summary>
        UseEffortDistThreshold
        ''' <summary>To document</summary>
        EffortDistThreshold
        ''' <summary>To document</summary>
        EcospaceUseLocalMemory
        ''' <summary>To document</summary>
        EcospaceIBMMovePacketOnStanza
        ''' <summary>User-defined output directory for Ecospace Map output</summary>
        EcospaceMapOutputDir
        ''' <summary>User-defined output directory for Ecospace Area Averaged outputs</summary>
        EcospaceAreaOutputDir

        EcospaceFirstOutputTimeStep

        ''' <summary>Can Ecospace use Ecosim biomass forcing time series</summary>
        EcospaceUseEcosimBiomassForcing
        ''' <summary>Maintenance flag, used by Ecospace, to tell if there is Ecosim biomass forcing time series loaded. 
        ''' This does not tell Ecospace to use the time series just that it is available to be used.</summary>
        EcospaceIsEcosimBiomassForcingLoaded

        ''' <summary>Can Ecospace use Ecosim discards forcing time series</summary>
        EcospaceUseEcosimDiscardForcing
        ''' <summary>Maintenance flag, used by Ecospace, to tell if there is Ecosim biomass forcing time series loaded. 
        ''' This does not tell Ecospace to use the time series just that it is available to be used.</summary>
        EcospaceIsEcosimDiscardForcingLoaded

        ''' <summary>Ecospace Discards Map</summary>
        EcospaceMapDiscards

        ''''''''''''''''''''''''''''
        ' Ecosim ouput data over time
        '''''''''''''''''''''
        ''' <summary>Ecosim absolute biomass over time</summary>
        EcosimBiomass
        ''' <summary>Ecosim relative biomass over time</summary>
        EcosimBiomassRel
        ''' <summary>To document</summary>
        EcosimYield
        ''' <summary>[catch(t)]/[catch(0)]</summary>
        EcosimYieldRel
        ''' <summary>To document</summary>
        EcosimCatchGroupGear
        ''' <summary>Ecosim value by group over time</summary>
        EcosimValueGroup
        ''' <summary>Ecosim relative value by group, time</summary>
        EcosimValueGroupRel
        ''' <summary>Ecosim value by group fleet over time</summary>
        EcosimValueGroupFleet
        ''' <summary>Fishing mortality by group fleet</summary>
        EcosimFishingMortGroupGear

        EcosimLandingsGroupGear
        EcosimDiscardsGroupGear
        EcosimDiscardsMortGroupGear
        EcosimDiscardsSurvivedGroupGear

        ''' <summary>To document</summary>
        EcosimTotalMort
        ''' <summary>To document</summary>
        EcosimConsumpBiomass
        ''' <summary>To document</summary>
        EcosimFeedingTime
        ''' <summary>To document</summary>
        EcosimPredMort
        ''' <summary>To document</summary>
        EcosimFishMort
        ''' <summary>To document</summary>
        EcosimProdConsump
        ''' <summary>To document</summary>
        EcosimAvgWeight
        ''' <summary>To document</summary>
        EcosimAvgPrey
        ''' <summary>To document</summary>
        EcosimAvgPred
        ''' <summary>[predation mortality]/[total mortality]</summary>
        EcosimMortVPred
        ''' <summary>[fishing mortality]/[total mortality]</summary>
        EcosimMortVFishing
        ''' <summary>To document</summary>
        EcosimMortVPredPM
        ''' <summary>To document</summary>
        EcosimMortVFishingPM
        ''' <summary>To document</summary>
        EcosimEcoSystemStruct
        ''' <summary>Is the catch for this group aggregated across all the fleets</summary>
        EcosimIsCatchAggregated
        ''' <summary>Ecopath ouput data over time</summary>
        EcopathWeight
        ''' <summary>To document</summary>
        EcopathNumber
        ''' <summary>To document</summary>
        EcopathBiomass
        ''' <summary>To document</summary>
        LorenzenMortality
        ''' <summary>Particle size distribution</summary>
        PSD
        ''' <summary>Consumption by Pred of this Prey over time</summary>
        EcosimPredConsumpTime
        ''' <summary>Consumption Rate by Pred of this Prey over time (consumpt(prey,pred)/b(prey)) over time</summary>
        EcosimPredRateTime
        ''' <summary>To document</summary>
        EcosimElectivityTime
        ''' <summary>Percentage of a group this group consumes over time</summary>
        EcosimPreyPercentageTime
        ''' <summary>To document</summary>
        IsPred
        ''' <summary>To document</summary>
        IsPrey

        'Ecosim Group summary output
        ''' <summary>To document</summary>
        EcosimGroupBiomassStart
        ''' <summary>To document</summary>
        EcosimGroupBiomassEnd

        ''' <summary>To document</summary>
        EcosimGroupCatchStart
        ''' <summary>To document</summary>
        EcosimGroupCatchEnd
        ''' <summary>To document</summary>
        EcosimGroupMaxMort

        ''' <summary>To document</summary>
        EcosimGroupValueStart
        ''' <summary>To document</summary>
        EcosimGroupValueEnd

        'Ecosim Fleet output
        ''' <summary>To document</summary>
        EcosimFleetCatchStart
        ''' <summary>To document</summary>
        EcosimFleetCatchEnd

        ''' <summary>To document</summary>
        EcosimFleetValueStart
        ''' <summary>To document</summary>
        EcosimFleetValueEnd

        ''' <summary>To document</summary>
        EcosimFleetCostStart
        ''' <summary>To document</summary>
        EcosimFleetCostEnd
        ''' <summary>To document</summary>
        EcosimFleetEffort
        ''' <summary>To document</summary>
        EcosimFleetJobs
        ''' <summary>To document</summary>
        EcosimFleetProfit
        ''' <summary>To document</summary>
        EcosimFleetValueTime
        ''' <summary>To document</summary>
        EcosimFleetCatchTime

       ''' <summary>Sum of squares fit of Ecospace predicted values to all reference data across all groups</summary>
        EcospaceSS
        ''' <summary>Sum of squares fit of Ecospace predicted values by group</summary>
        EcospaceSSGroup
        ''' <summary>
        ''' Has SS been calculated. Is there Ecospace Timeseries data loaded.
        ''' </summary>
        EcospaceSSCalculated
        ''' <summary>Sum of squares fit of Ecosim predicted values to all reference data across all the groups and data</summary>
        EcosimSS
        ''' <summary>Sum of squares fit of Ecosim predicted values to reference data by group</summary>
        EcosimSSGroup
        ''' <summary>Monte Carlo diet multiplier</summary>
        mcDietMult

        ''' <summary>Monte Carlo sampled <see cref="Biomass">B</see></summary>
        mcB
        ''' <summary>Monte Carlo sampled <see cref="PBInput">PB</see></summary>
        mcPB
        ''' <summary>Monte Carlo sampled <see cref="QBInput">QB</see></summary>
        mcQB
        ''' <summary>Monte Carlo sampled <see cref="BioAccumInput">BA</see></summary>
        mcBA
        ''' <summary>Monte Carlo sampled <see cref="EEInput">EE</see></summary>
        mcEE
        ''' <summary>Monte Carlo sampled <see cref="VulMult">Vulnerability</see></summary>
        mcVU
        ''' <summary>Monte Carlo sampled <see cref="DietComp">DC</see></summary>
        mcDC
        ''' <summary>Monte Carlo sampled <see cref="Discards">Discards</see></summary>
        mcDiscards
        ''' <summary>Monte Carlo sampled <see cref="Landings">Landings</see></summary>
        mcLandings
        ''' <summary>Monte Carlo sampled <see cref="BioAccumRate">BA rate</see></summary>
        mcBaBi

        ''' <summary>Monte Carlo best fitting <see cref="Biomass">B</see></summary>
        mcBbf
        ''' <summary>Monte Carlo best fitting <see cref="PBInput">PB</see></summary>
        mcPBbf
        ''' <summary>Monte Carlo best fitting <see cref="QBInput">QB</see></summary>
        mcQBbf
        ''' <summary>Monte Carlo best fitting <see cref="BioAccumInput">BA</see></summary>
        mcBAbf
        ''' <summary>Monte Carlo best fitting <see cref="EEInput">EE</see></summary>
        mcEEbf
        ''' <summary>Monte Carlo best fitting <see cref="VulMult">Vulnerability</see></summary>
        mcVUbf
        ''' <summary>Monte Carlo best fitting <see cref="DietComp">DC</see></summary>
        mcDCbf
        ''' <summary>Monte Carlo best fitting <see cref="Discards">Discards</see></summary>
        mcDiscardsbf
        ''' <summary>Monte Carlo best fitting <see cref="Landings">Landings</see></summary>
        mcLandingsbf
        ''' <summary>Monte Carlo best fitting <see cref="BioAccumRate">BA rate</see></summary>
        mcBaBibf

        ''' <summary>Monte Carlo <see cref="Biomass">B</see> lower sample limit</summary>
        mcBLower
        ''' <summary>To document</summary>
        mcPBLower
        ''' <summary>To document</summary>
        mcQBLower
        ''' <summary>To document</summary>
        mcBALower
        ''' <summary>To document</summary>
        mcEELower
        ''' <summary>To document</summary>
        mcVULower
        mcDiscardsLower
        mcLandingsLower
        ''' <summary>Monte Carlo biomass accum rate upper limit</summary>
        mcBaBiLower

        ''' <summary>Monte Carlo B upper limit</summary>
        mcBUpper
        ''' <summary>Monte Carlo P over B upper limit</summary>
        mcPBUpper
        ''' <summary>Monte Carlo Q over B upper limit</summary>
        mcQBUpper
        ''' <summary>Monte Carlo Bimass accummulation upper limit</summary>
        mcBAUpper
        ''' <summary>Monte Carlo EE upper limit</summary>
        mcEEUpper
        ''' <summary>Monte Carlo vulnerabilities upper limit</summary>
        mcVUUpper
        ''' <summary>Monte Carlo discards upper limit</summary>
        mcDiscardsUpper
        ''' <summary>Monte Carlo landings upper limit</summary>
        mcLandingsUpper
        ''' <summary>Monte Carlo biomass accum rate upper limit</summary>
        mcBaBiUpper

        ''' <summary>Monte Carlo biomass cv</summary>
        mcBcv
        ''' <summary>Monte Carlo P over B cv</summary>
        mcPBcv
        ''' <summary>Monte Carlo Q over B cv</summary>
        mcQBcv
        ''' <summary>Monte Carlo biomass accummulation cv</summary>
        mcBAcv
        ''' <summary>Monte Carlo EE cv</summary>
        mcEEcv
        ''' <summary>Monte Carlo vulnerabilities cv</summary>
        mcVUcv
        ''' <summary>Monte Carlo discards cv</summary>
        mcDiscardscv
        ''' <summary>Monte Carlo landings cv</summary>
        mcLandingscv
        ''' <summary>Monte Carlo biomass accummulation rate cv</summary>
        mcBaBicv
        mcDCcv

        'end monte carlo variables
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        ''' <summary>To document</summary>
        BarrierAvoidanceWeight

        ''' <summary>
        ''' Migration in area movement weight.
        ''' </summary>
        ''' <remarks>Movement weight for preferred direction within a migartion area. </remarks>
        InMigAreaMoveWeight

        ' Fishing Policy Search varaibles
        ''' <summary>To document</summary>
        SearchBlock 'codeblock in EwE5

        ' Generic search parameters
        ''' <summary>To document</summary>
        SearchDiscountRate
        ''' <summary>To document</summary>
        SearchGenDiscRate
        ''' <summary>To document</summary>
        SearchBaseYear
        ''' <summary>To document</summary>
        SearchFishingMortalityPenalty

        'isEconomicAvailable

        ''' <summary>To document</summary>
        FPSValueComponentType
        ''' <summary>To document</summary>
        FPSGroupStrucRelWeight

        ''' <summary>To document</summary>
        FPSFleetJobCatchValue
        ''' <summary>To document</summary>
        FPSFleetTargetProfit

        'Model Parameters
        ''' <summary>To document</summary>
        FPSNRuns 'number of runs
        ''' <summary>To document</summary>
        FPSGroupMandRelBiom
        ''' <summary>To document</summary>
        FPSMaxNumEval
        ''' <summary>To document</summary>
        FPSMaxEffChange
        ''' <summary>To document</summary>
        FPSInitOption
        ''' <summary>To document</summary>
        FPSSearchOption
        ''' <summary>To document</summary>
        FPSOptimizeApproach
        ''' <summary>To document</summary>
        FPSOptimizeOptions

        ''' <summary>To document</summary>
        FPSEconomicWeight
        ''' <summary>To document</summary>
        FPSSocialWeight
        ''' <summary>To document</summary>
        FPSMandatedRebuildingWeight
        ''' <summary>To document</summary>
        FPSEcoSystemWeight
        ''' <summary>To document</summary>
        FPSBiomassDiversityWeight

        ''' <summary>To document</summary>
        FPSMaxPortUtil
        ''' <summary>To document</summary>
        SearchPrevCostEarning
        ''' <summary>To document</summary>
        FPSIncludeComp
        'UseEcospace and BatchRun have not been implemented yet
        ''' <summary>To document</summary>
        FPSBatchRun
        ''' <summary>To document</summary>
        FPSUseEcospace
        ''' <summary>To document</summary>
        FPSFishingLimit
        ''' <summary>To document</summary>
        FPSPredictionVariance
        ''' <summary>To document</summary>
        FPSExistenceValue
        ''' <summary>To document</summary>
        FPSUseEconomicPlugin

        ' Fit to time series
        ''' <summary>To document</summary>
        F2TSVulnerabilitySearch
        ''' <summary>To document</summary>
        F2TSAnomalySearch
        ''' <summary>To document</summary>
        F2TSCatchAnomaly
        ''' <summary>To document</summary>
        F2TSCatchAnomalySearchShapeNumber
        ''' <summary>To document</summary>
        F2TSFirstYear
        ''' <summary>To document</summary>
        F2TSLastYear
        ''' <summary>To document</summary>
        F2TSNumSplinePoints
        ''' <summary>Weights of applied TS in search algorithm</summary>
        F2TSAppliedWeights
        ''' <summary>Variance of Vulnerability</summary>
        F2TSVulnerabilityVariance

        ''' <summary>Variance of Primary Production</summary>
        F2TSPPVariance

        ''' <summary>Number of data points for AIC</summary>
        F2TSNAICData
        ''' <summary>Use default V's instead of currect V's</summary>
        F2TSUseDefaultV

        ' Ecotracer
        ''' <summary>To document</summary>
        CZero
        ''' <summary>To document</summary>
        CInflow
        ''' <summary>To document</summary>
        COutflow
        ''' <summary>Physical contaminant decay rate</summary>
        CPhysicalDecayRate
        ''' <summary>To document</summary>
        ConForceNumber
        ''' <summary>To document</summary>
        CImmig
        ''' <summary>To document</summary>
        CEnvironment
        ''' <summary>To document</summary>
        CBEnvironment
        ''' <summary>Proportion of contaminant excreted</summary>
        CAssimilationProp
        ''' <summary>Contaminant excretion or metabolism rate</summary>
        CMetablismRate
        ''' <summary>To document</summary>
        Concentration
        ''' <summary>To document</summary>
        ConcBio
        ''' <summary>To document</summary>
        CSum

        ''' <summary>
        ''' Max number of sub time steps for the contaminant tracer model
        ''' </summary>
        ConMaxTimeSteps

        'MPA Optimization EcoSeed RandomSearch
        ''' <summary>To document</summary>
        MPAOptEconomicValue
        ''' <summary>To document</summary>
        MPAOptSocialValue
        ''' <summary>To document</summary>
        MPAOptMandatedValue
        ''' <summary>To document</summary>
        MPAOptEcologicalValue
        ''' <summary>To document</summary>
        MPAOptBiomassDiversityValue
        ''' <summary>To document</summary>
        MPAOptBestRow
        ''' <summary>To document</summary>
        MPAOptBestCol
        ''' <summary>To document</summary>
        MPAOptCurRow
        ''' <summary>To document</summary>
        MPAOptCurCol

        ''' <summary>To document</summary>
        MPAOptBoundaryWeight
        ''' <summary>To document</summary>
        MPAOptSearchType

        ''' <summary>To document</summary>
        MPAOptStepSize
        ''' <summary>To document</summary>
        MPAOptIterations
        ''' <summary>To document</summary>
        MPAOptMaxArea
        ''' <summary>To document</summary>
        MPAOptMinArea
        ''' <summary>To document</summary>
        iMPAOptToUse
        ''' <summary>To document</summary>
        MPAUseCellWeight

        ''' <summary>To document</summary>
        MPAOptStartYear
        ''' <summary>To document</summary>
        MPAOptEndYear

        ''' <summary>To document</summary>
        MPAOptPercentageClosed
        ''' <summary>To document</summary>
        MPAOptTotalValue
        ''' <summary>To document</summary>
        MPAOptAreaBoundary

        ''' <summary>Ecospace cell MPA seed assignments</summary>
        LayerMPASeed
        ''' <summary>To document</summary>
        LayerMPASeedCurrent
        ''' <summary>To document</summary>
        LayerMPASeedBest
        ''' <summary>Ecospace cell MPA Random assignments</summary>
        LayerMPARandom
        ''' <summary>MSE coefficient of variation for biomass</summary>
        MSEBioCV
        ''' <summary>MSE coefficient of variation for fishing fleets</summary>
        MSEFleetCV
        ''' <summary>MSE increase in catchability by group per year (multiplier)</summary>
        MSEQIncrease
        ''' <summary>MSE importance weight in assuming impact of fleet on a group (multiplier)</summary>
        MSEFleetWeight
        ''' <summary>Lower biomass bounds for risk analysis</summary>
        MSELowerRisk
        ''' <summary>Upper biomass bounds for risk analysis</summary>
        MSEUpperRisk
        ''' <summary>Number of trial that exceeded the lower biomass bounds for risk analysis</summary>
        MSELowerRiskPercent
        ''' <summary>Number of trial that exceeded the upper biomass bounds for risk analysis</summary>
        MSEUpperRiskPercent

        ''' <summary>Sum of all economic values for the current MSE output object (results)</summary>
        MSEEconomicValue
        ''' <summary>Sum of employment values for the current MSE output object (results)</summary>
        MSEEmployValue
        MSEMandatedValue
        ''' <summary>Sum of biomass for the current MSE output object (results)</summary>
        MSEEcologicalValue

        ''' <summary>Weighted sum of all mean values</summary>
        MSEWeightedTotalValue
        ''' <summary>To document</summary>
        MSEMeanEconomicValue
        ''' <summary>To document</summary>
        MSEMeanEmployValue
        ''' <summary>To document</summary>
        MSEMeanMandatedValue
        ''' <summary>To document</summary>
        MSEMeanEcologicalValue
        ''' <summary>To document</summary>
        MSEBestTotalValue

        ''' <summary>Trial number for the current MSE output object (results)</summary>
        MSETrialNumber

        ''' <summary>To document</summary>
        MSEAssessMethod
        'MSEKalmanGain
        ''' <summary>To document</summary>
        MSEForcastGain
        ''' <summary>To document</summary>
        MSEAssessPower
        ''' <summary>To document</summary>
        MSENTrials
        ''' <summary>To document</summary>
        MSEUseEconomicPlugin
        ''' <summary>To document</summary>
        RHalfB0Ratio
        ''' <summary>To document</summary>
        MSEFixedF
        ''' <summary>To document</summary>
        MSERecruitmentCV
        ''' <summary>Total allowable catch</summary>
        MSETAC

        ''' <summary>Max Effort for the MSE</summary>
        MSEMaxEffort
        ''' <summary>To document</summary>
        MSELPSolution

        'data by iteration
        ''' <summary>To document</summary>
        MSEBiomass
        ''' <summary>To document</summary>
        MSECatchByGroup
        ''' <summary>To document</summary>
        MSECatchByFleet
        ''' <summary>To document</summary>
        MSEValueByFleet
        ''' <summary>To document</summary>
        MSEEffort

        ''' <summary>True = Use predicted Effort False = user input Effort</summary>
        MSEPredictEffort
        ''' <summary>Biomass by group</summary>
        MSEFixedEscapement

        ''' <summary>Stop the current MSE run</summary>
        MSEStop

        ''' <summary>Effort type the MSE is to use</summary>
        MSEEffortSource

        ''' <summary>Regulatory type to be used y MSE</summary>
        MSERegulatoryMode

        ''' <summary>To document</summary>
        MSERefBioLower
        ''' <summary>To document</summary>
        MSERefBioUpper

        ''' <summary>To document</summary>
        MSERefBioEstLower
        ''' <summary>To document</summary>
        MSERefBioEstUpper
        ''' <summary>To document</summary>
        MSERefGroupCatchLower
        ''' <summary>To document</summary>
        MSERefGroupCatchUpper

        ''' <summary>To document</summary>
        MSERefFleetCatchLower
        ''' <summary>To document</summary>
        MSERefFleetCatchUpper

        ''' <summary>To document</summary>
        MSERefFleetEffortLower
        ''' <summary>To document</summary>
        MSERefFleetEffortUpper

        'MSE Stats
        ''' <summary>To document</summary>
        MSEBiomassHistogram
        ''' <summary>To document</summary>
        MSEBiomassMeanValues
        ''' <summary>To document</summary>
        MSEBiomassMin
        ''' <summary>To document</summary>
        MSEBiomassMax
        ''' <summary>To document</summary>
        MSEBiomassCV
        ''' <summary>To document</summary>
        MSEBiomassSdt
        ''' <summary>To document</summary>
        MSEBiomassBins
        ''' <summary>To document</summary>
        MSEBiomassBinWidths
        ''' <summary>To document</summary>
        MSEBiomassValues
        ''' <summary>To document</summary>
        MSEBiomassAboveLimit
        ''' <summary>To document</summary>
        MSEBiomassBelowLimit

        ''' <summary>To document</summary>
        MSEBiomassAboveLimitPM
        ''' <summary>To document</summary>
        MSEBiomassBelowLimitPM
        ''' <summary>To document</summary>
        MSEBiomassCVPM

        ''' <summary>To document</summary>
        MSEGroupCatchHistogram
        ''' <summary>To document</summary>
        MSEGroupCatchMeanValues
        ''' <summary>To document</summary>
        MSEGroupCatchMin
        ''' <summary>To document</summary>
        MSEGroupCatchMax
        ''' <summary>To document</summary>
        MSEGroupCatchCV
        ''' <summary>To document</summary>
        MSEGroupCatchStd
        ''' <summary>To document</summary>
        MSEGroupCatchBins
        ''' <summary>To document</summary>
        MSEGroupCatchBinWidths
        ''' <summary>To document</summary>
        MSEGroupCatchValues
        ''' <summary>To document</summary>
        MSEGroupCatchAboveLimit
        ''' <summary>To document</summary>
        MSEGroupCatchBelowLimit

        ''' <summary>To document</summary>
        MSEFleetValueHistogram
        ''' <summary>To document</summary>
        MSEFleetValueMeanValues
        ''' <summary>To document</summary>
        MSEFleetValueMin
        ''' <summary>To document</summary>
        MSEFleetValueMax
        ''' <summary>To document</summary>
        MSEFleetValueCV
        ''' <summary>To document</summary>
        MSEFleetValueStd
        ''' <summary>To document</summary>
        MSEFleetValueBins
        ''' <summary>To document</summary>
        MSEFleetValueBinWidths
        ''' <summary>To document</summary>
        MSEFleetValueValues
        ''' <summary>To document</summary>
        MSEFleetValueAboveLimit
        ''' <summary>To document</summary>
        MSEFleetValueBelowLimit

        ''' <summary>To document</summary>
        MSEEffortHistogram
        ''' <summary>To document</summary>
        MSEEffortMeanValues
        ''' <summary>To document</summary>
        MSEEffortMin
        ''' <summary>To document</summary>
        MSEEffortMax
        ''' <summary>To document</summary>
        MSEEffortCV
        ''' <summary>To document</summary>
        MSEEffortStd
        ''' <summary>To document</summary>
        MSEEffortBins
        ''' <summary>To document</summary>
        MSEEffortBinWidths
        ''' <summary>To document</summary>
        MSEEffortValues
        ''' <summary>To document</summary>
        MSEEffortAboveLimit
        ''' <summary>To document</summary>
        MSEEffortBelowLimit

        ''' <summary>To document</summary>
        MSEBioEstHistogram
        ''' <summary>To document</summary>
        MSEBioEstMeanValues
        ''' <summary>To document</summary>
        MSEBioEstMin
        ''' <summary>To document</summary>
        MSEBioEstMax
        ''' <summary>To document</summary>
        MSEBioEstCV
        ''' <summary>To document</summary>
        MSEBioEstStd
        ''' <summary>To document</summary>
        MSEBioEstBins
        ''' <summary>To document</summary>
        MSEBioEstBinWidths
        ''' <summary>To document</summary>
        MSEBioEstValues
        ''' <summary>To document</summary>
        MSEBioEstAboveLimit
        ''' <summary>To document</summary>
        MSEBioEstBelowLimit

        ''' <summary>To document</summary>
        MSEFStatHistogram
        ''' <summary>To document</summary>
        MSEFStatMeanValues
        ''' <summary>To document</summary>
        MSEFStatMin
        ''' <summary>To document</summary>
        MSEFStatMax
        ''' <summary>To document</summary>
        MSEFStatCV
        ''' <summary>To document</summary>
        MSEFStatStd
        ''' <summary>To document</summary>
        MSEFStatBins
        ''' <summary>To document</summary>
        MSEFStatBinWidths
        ''' <summary>To document</summary>
        MSEFStatValues
        ''' <summary>To document</summary>
        MSEFStatAboveLimit
        ''' <summary>To document</summary>
        MSEFStatBelowLimit

        ''' <summary>To document</summary>
        MSEStartYear
        ''' <summary>To document</summary>
        MSEResultsStartYear
        ''' <summary>To document</summary>
        MSEResultsEndYear

        ' ToDo_JB: evaluate if we still need these
        ''' <summary>To document</summary>
        MSYRunSilent
        ''' <summary>To document</summary>
        MSYEvalValue
        ''' <summary>To document</summary>
        MSYStartTime
        ''' <summary>To document</summary>
        MSYEvaluateFleet

        'MSE Batch Variables
        'Target Fishing Mortality(hockey stick)
        ''' <summary>To document</summary>
        MSETFMNIteration
        ''' <summary>To document</summary>
        MSETFMBLimLower
        ''' <summary>To document</summary>
        MSETFMBLimUpper
        ''' <summary>To document</summary>
        MSETFMBLimValues

        ''' <summary>To document</summary>
        MSETFMBBaseLower
        ''' <summary>To document</summary>
        MSETFMBBaseUpper
        ''' <summary>To document</summary>
        MSETFMBBaseValues

        ''' <summary>To document</summary>
        MSETFMFOptLower
        ''' <summary>To document</summary>
        MSETFMFOptUpper
        ''' <summary>To document</summary>
        MSETFMFOptValues

        'TAC total allowable catch
        ''' <summary>To document</summary>
        MSEBatchTACNIteration
        ''' <summary>To document</summary>
        MSEBatchTACLower
        ''' <summary>To document</summary>
        MSEBatchTACUpper
        ''' <summary>To document</summary>
        MSEBatchTACValues
        ''' <summary>To document</summary>
        MSEBatchTACManaged

        'F fishing mortality
        ''' <summary>To document</summary>
        MSEBatchFNIteration
        ''' <summary>To document</summary>
        MSEBatchFLower
        ''' <summary>To document</summary>
        MSEBatchFUpper
        ''' <summary>To document</summary>
        MSEBatchFValues
        ''' <summary>To document</summary>
        MSEBatchFManaged

        'MSE Batch output types
        ''' <summary>To document</summary>
        MSEBatchOutputBiomass
        ''' <summary>To document</summary>
        MSEBatchOutputConBio
        ''' <summary>To document</summary>
        MSEBatchOutputFeedingTime
        ''' <summary>To document</summary>
        MSEBatchOutputPredRate
        ''' <summary>To document</summary>
        MSEBatchOutputCatch
        ''' <summary>To document</summary>
        MSEBatchOutputFishingMortRate
        ''' <summary>To document</summary>
        MSEBatchOuputDir
        ''' <summary>To document</summary>
        MSEBatchGroupRunType
        ''' <summary>
        ''' Type of calculation to use when setting MSE Batch iterations values % or +- Value 
        ''' Boolean is this group managed using the TFM
        '''</summary>
        MSEBatchTFMManaged
        ''' <summary>
        ''' Type of calculation to use when setting MSE Batch iterations values % or +- Value 
        '''</summary>
        MSEBatchIterCalcType

        ' Pedigree

        ''' <summary>To document</summary>
        VariableName
        ''' <summary>To document</summary>
        IndexValue
        ''' <summary>Assigned pedigree level</summary>
        PedigreeLevel
        ''' <summary>Custom confidence interval for pedigree</summary>
        ConfidenceInterval

        'Varnames added for Game Server

        ''' <summary>Game server loaded model</summary>
        GameModel
        ''' <summary>Game server run state</summary>
        GameState
        ''' <summary>Game client moderator state</summary>
        GameModeratorState
        ''' <summary>Items the client is allowed to show</summary>
        GameViewVisibleItems
        ''' <summary>Items the client can request from the server</summary>
        GameViewAvailableItems
        ''' <summary>Limits imposed on variables</summary>
        GameDataLimits
        ''' <summary>User entered fishing rate modifiers/shapes for fleets</summary>
        GameFleetFishingRates
        ''' <summary>User entered mortality/fishing rate modifiers/shapes for groups</summary>
        GameGroupFishingMortRates
        ''' <summary>Traffic lights the client can request from the server</summary>
        GameViewTrafficLights
        ''' <summary>Type of data available during a simulation (TimeStep or Progress)</summary>
        GameAvailableRunData

        ''' <summary>To document</summary>
        GameSimulationTimeStep
        ''' <summary>Game absolute biomass</summary>
        GameBiomass
        ''' <summary>To document</summary>
        GameBiomassPM
        ''' <summary>Game generic relative biomass over time (no specific source)</summary>
        GameBiomassRel

        ''' <summary>Game biomass with Fishing regulation</summary>
        GameBiomassFishRegulation
        ''' <summary>To document</summary>
        GameBiomassFishRegulationPM

        ''' <summary>To document</summary>
        GameBiomassByRegion
        ''' <summary>To document</summary>
        GameCatchRegionFleetGroup
        ''' <summary>To document</summary>
        GameGroupValue
        ''' <summary>To document</summary>
        GameGroupFleetValue

        ''' <summary>Profit by Fleet</summary>      
        GameFleetProfitSummary
        ''' <summary>Jobs(?) by Fleet</summary>    
        GameFleetJobsSummary

        ''' <summary>To document</summary>
        GameFleetValue
        ''' <summary>To document</summary>
        GameFleetCatch
        ''' <summary>To document</summary>
        GameFleetCatchPM
        ''' <summary>To document</summary>
        GameFleetValuePM

        ''' <summary>For Ecosim Yield from Ecosim Plots Biomass * FishTime</summary>
        GameGroupCatch
        ''' <summary>To document</summary>
        GameGroupCatchPM

        'Economic data for the game
        ''' <summary>To document</summary>
        GameEconomicCost
        ''' <summary>To document</summary>
        GameEconomicCostPM
        ''' <summary>To document</summary>
        GameEconomicCostByFleet
        ''' <summary>To document</summary>
        GameEconomicCostByFleetPM
        ''' <summary>To document</summary>
        GameEconomicProfit
        ''' <summary>To document</summary>
        GameEconomicProfitByFleet
        ''' <summary>To document</summary>
        GameEconomicProfitByFleetPM
        ''' <summary>To document</summary>
        GameEconomicProfitPM
        ''' <summary>To document</summary>
        GameEconomicJobsTotal
        ''' <summary>To document</summary>
        GameEconomicJobsTotalPM
        ''' <summary>To document</summary>
        GameEconomicJobsByFleet
        ''' <summary>To document</summary>
        GameEconomicJobsByFleetPM
        ''' <summary>To document</summary>
        GameEconomicProduction
        ''' <summary>To document</summary>
        GameEconomicProductionPM

        ''' <summary>To document</summary>
        GameEconomicTaxes
        ''' <summary>To document</summary>
        GameEconomicTaxesPM
        ''' <summary>To document</summary>
        GameEconomicTaxesByFleet
        ''' <summary>To document</summary>
        GameEconomicTaxesByFleetPM

        ''' <summary>To document</summary>
        GameEconomicSubsidies
        ''' <summary>To document</summary>
        GameEconomicSubsidiesPM
        ''' <summary>To document</summary>
        GameEconomicSubsidiesByFleet
        ''' <summary>To document</summary>
        GameEconomicSubsidiesByFleetPM

        ''' <summary>Eco system structure 1/pb * b(t)</summary>    
        GameEcoSystemStruct
        ''' <summary>To document</summary>
        GameEcoSystemStructPM

        ''' <summary>Game names added for the Game data because EwE6 uses Name for all names</summary>
        GameFleetName
        ''' <summary>To document</summary>
        GameMPAName
        ''' <summary>To document</summary>
        GameHabitatName

        ''' <summary>To document</summary>
        GameFleetFishingRatesPM
        ''' <summary>To document</summary>
        GameForceSalinity
        ''' <summary>To document</summary>
        GameForceNutrient
        ''' <summary>To document</summary>
        GameForceTemperature
        ''' <summary>To document</summary>
        GameForcePrimaryProducer

        ''' <summary>To document</summary>
        GameForceSalinityPM
        ''' <summary>To document</summary>
        GameForceNutrientPM
        ''' <summary>To document</summary>
        GameForceTemperaturePM

        ''' <summary>To document</summary>
        GameForceSalinityName
        ''' <summary>To document</summary>
        GameForceNutrientName
        ''' <summary>To document</summary>
        GameForceTemperatureName
        ''' <summary>To document</summary>
        GameForcePrimaryProducerName

        ''' <summary>To document</summary>
        GameForcePrimaryProducerNumber

        ''' <summary>Game biomass for an interation</summary>
        GameBiomassIteration
        ''' <summary>Game catch by group for an interation</summary>
        GameGroupCatchIteration
        ''' <summary>Game effort by fleet for an interation</summary>
        GameFleetEffortIteration
        ''' <summary>To document</summary>
        GameFleetValueIteration

        ''' <summary>Visualization settings for a 3D game environment</summary>
        Game3DVizSettings

        ' PMs for NA vars 
        ''' <summary>To document</summary>
        GameTLCatchPM
        ''' <summary>To document</summary>
        GameTLPM
        ''' <summary>To document</summary>
        GameFIBPM
        ''' <summary>To document</summary>
        GameKemptonsQPM

        ''' <summary>To document</summary>
        PSDEnabled
        ''' <summary>To document</summary>
        PSDComputed
        ''' <summary>Von Bertalanffy growth curvature (VBGF) parameter.</summary>
        ''' <remarks>http://www.fao.org/docrep/w5449e/w5449e05.htm</remarks>
        VBK
        ''' <summary>To document</summary>
        BiomassAvgSzWt
        ''' <summary>To document</summary>
        BiomassSzWt
        ''' <summary>To document</summary>
        AinLWInput
        ''' <summary>To document</summary>
        AinLWOutput
        ''' <summary>To document</summary>
        BinLWInput
        ''' <summary>To document</summary>
        BinLWOutput
        ''' <summary>Length at infinity</summary>
        LooInput
        ''' <summary>Length at infinity</summary>
        LooOutput
        ''' <summary>To document</summary>
        WinfInput
        ''' <summary>To document</summary>
        WinfOutput
        ''' <summary>To document</summary>
        t0Input
        ''' <summary>To document</summary>
        t0Output
        ''' <summary>Age at first capture</summary>
        TCatchInput
        ''' <summary>Age at first capture</summary>
        TCatchOutput
        ''' <summary>Max. age</summary>
        TmaxInput
        ''' <summary>Max. age</summary>
        TmaxOutput
        ''' <summary>To document</summary>
        PSDIncluded
        ''' <summary>To document</summary>
        PSDMortalityType
        ''' <summary>To document</summary>
        PSDFirstWeightClass
        ''' <summary>To document</summary>
        PSDNumWeightClasses
        ''' <summary>To document</summary>
        ClimateType
        ''' <summary>To document</summary>
        NumPtsMovAvg

        ''' <summary>Trophic level of catch</summary>
        TLCatch
        ''' <summary>Trophic level of groups</summary>
        TL
        ''' <summary>Fishing in-balance (FIB) index</summary>
        FIB
        ''' <summary>Kemptons' Q</summary>
        KemptonsQ
        ''' <summary>Shannon's diversity index</summary>
        ShannonDiversity
        ''' <summary>User selection of biodiversity indicator</summary>
        DiversityIndex
        ''' <summary>Value of selected biodiversity indicator</summary>
        BiodiversityIndicator

        ''' <summary>Total catch</summary>
        TotalCatch

        ''' <summary>Start year of the Ecopath model</summary>
        EcopathFirstYear
        ''' <summary>Number of years that the Ecopath model represents</summary>
        EcopathNumYears
        ''' <summary>Southern extent of the EwE model</summary>
        South
        ''' <summary>Northern extent of the EwE model</summary>
        North
        ''' <summary>Western extent of the EwE model</summary>
        West
        ''' <summary>Eastern extent of the EwE model</summary>
        East
        ''' <summary>Is this model coupled to an external model Ecospace model?</summary>
        IsEcospaceModelCoupled
        ''' <summary>Group assigned to a given taxon</summary>
        TaxonGroup
        ''' <summary>Stanza assigned to a given taxon</summary>
        TaxonStanza
        ''' <summary>Taxon biomass proportion</summary>
        TaxonPropBiomass
        ''' <summary>Taxon catch proportion</summary>
        TaxonPropCatch
        ''' <summary>Taxon phylum</summary>
        Phylum
        ''' <summary>Taxon class</summary>
        [Class]
        ''' <summary>Taxon order</summary>
        Order
        ''' <summary>Taxon family</summary>
        Family
        ''' <summary>Taxon genus</summary>
        Genus
        ''' <summary>Taxon species</summary>
        Species
        ''' <summary>Sea Around Us Project code for a species</summary>
        CodeSAUP
        ''' <summary>Sea Life Base code for a species</summary>
        CodeSLB
        ''' <summary>FishBase code for a species</summary>
        CodeFB
        ''' <summary>FAO code for a species</summary>
        CodeFAO
        ''' <summary>Life Science indentifier for a species (http://en.wikipedia.org/wiki/LSID)</summary>
        CodeLSID
        ''' <summary>URL to the species information</summary>
        Source
        ''' <summary>Search term for a species</summary>
        SourceKey
        ''' <summary>Date that data was last actualized</summary>
        LastUpdated
        ''' <summary>To document</summary>
        OrganismType
        ''' <summary>To document</summary>
        EcologyType
        ''' <summary>To document</summary>
        IUCNConservationStatus
        ''' <summary>To document</summary>
        ExploitationStatus
        ''' <summary>To document</summary>
        OccurrenceStatus
        ''' <summary>To document</summary>
        TaxonMeanWeight
        ''' <summary>To document</summary>
        TaxonMeanLength
        ''' <summary>To document</summary>
        TaxonMaxLength
        ''' <summary>To document</summary>
        TaxonMeanLifespan
        ''' <summary>To document</summary>
        TaxonWinf
        ''' <summary>To document</summary>
        TaxonvbgfK
        ''' <summary>To document</summary>
        TaxonVulnerabilityIndex
        ''' <summary>To document</summary>
        TaxonSearchFields

        ''' <summary>Taxa assigned to a given group</summary>
        GroupTaxa

        ''' <summary>To document</summary>
        Coriolis
        ''' <summary>To document</summary>
        XVelocity
        ''' <summary>To document</summary>
        YVelocity
        SorWv

        AdvectionUpwellingThreshold

        AdvectionUpwellingPPMultiplier

        ''' <summary>States if a value is estimated by Ewe</summary>
        Estimated
        ''' <summary>1 - EE</summary>
        OtherMortInput
        ''' <summary>Helper flag to determine whether a group is fished</summary>
        IsFished
        ''' <summary>No. regions in Ecospace</summary>
        EcospaceRegionNumber

        ''' <summary>Group or fleet index targeted by MSY</summary>
        MSYFSelection
        ''' <summary><see cref="eVarNameFlags.MSYFSelection"/> interpretation flag, 
        ''' should be one of <see cref="eMSYFSelectionModeType">values</see></summary>
        MSYFSelectionMode
        ''' <summary>MSY assessment type</summary>
        MSYAssessment
        ''' <summary>MSY equilibrium step size, as a fractio of 0 to <see cref="eVarNameFlags.MSYMaxFishingRate"/></summary>
        MSYEquilibriumStepSize
        ''' <summary>MSY max F to vary to</summary>
        MSYMaxFishingRate
        ''' <summary>Number of MSY trial years</summary>
        MSYNumTrialYears
        ''' <summary>Number of MSY trial years</summary>
        MSYRunLengthMode

        ''' <summary>Proj4String</summary>
        AssumeSquareCells
        ''' <summary>WKT (ESRI) string describing the Ecospace projection</summary>
        ProjectionString

        ''' <summary>Country that a model represents.</summary>
        Country
        ''' <summary>Type of ecosystem that a model represents (estuary, reef, etc).</summary>
        EcosystemType
        ''' <summary>Code of a model in the EcoBase repository.</summary>
        CodeEcobase

        ''' <summary>Digital Object Identifier of the publication for this model.</summary>
        PublicationDOI
        ''' <summary>URI of the publication for this model.</summary>
        PublicationURI
        ''' <summary>Formatted reference to the publication for this model.</summary>
        PublicationReference

        SampleRating

        Z

        ''' <summary>Ecosim SOR Weight for groups with fast production.</summary>
        EcosimSORWt

        ''' <summary>Ecosim base catchability at time.</summary>
        RelQt
    End Enum

#End Region ' Variable names

#Region " Data Types "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type that indicates a class of data in the EwE core.
    '''</summary>
    ''' <remarks>
    ''' These enums have fixed values since values may be used to identify 
    ''' items in the EwE6 database system.
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Public Enum eDataTypes

        ''' <summary>
        ''' Data type is not specified.
        '''</summary>
        NotSet = 0

        ''' <summary>
        ''' Data belongs to the EwE model.
        '''</summary>
        EwEModel = 1

        ''' <summary>
        ''' Data belongs to the Ecopath group inputs,
        ''' which are provided to perform a parameter estimation run. 
        '''</summary>
        EcoPathGroupInput = 2

        ''' <summary>
        ''' Data belongs to the Ecopath group outputs,
        ''' which are produced by a parameter estimation run.
        '''</summary>
        EcoPathGroupOutput = 3

        ''' <summary>
        ''' Data belongs to the Ecopath fleet inputs,
        ''' which are provided for a parameter estimation run.
        '''</summary>
        FleetInput = 4

        ''' <summary>
        ''' Data belongs to an Ecosim scenario.
        '''</summary>
        EcoSimScenario = 5

        ''' <summary>
        ''' Data belongs to the Ecosim model parameters,
        ''' which instruct how to run an Ecosim scenario.
        '''</summary>
        EcoSimModelParameter = 6

        ''' <summary>
        ''' Data belongs to an Ecosim group input.
        '''</summary>
        EcoSimGroupInput = 7

        ''' <summary>
        ''' Data belongs to a Time Forcing Function.
        '''</summary>
        Forcing = 8

        ''' <summary>
        ''' Data belongs to an Egg Production Forcing Function.
        '''</summary>
        EggProd = 9

        ''' <summary>
        ''' Data belongs to a Mediation Function.
        '''</summary>
        Mediation = 10

        ''' <summary>
        ''' Data belongs to a Fishing Rate shape.
        '''</summary>
        FishingEffort = 11

        ''' <summary>
        ''' Data belongs to a Fishing Mortality shape.
        '''</summary>
        FishMort = 12

        ''' <summary>
        ''' Data belongs to an EwE multi-stanza configuration.
        '''</summary>
        Stanza = 13 'jb June-14-06 added for Stanza data types

        ''' <summary>
        ''' Data belongs to an Ecospace scenario.
        '''</summary>
        EcoSpaceScenario = 14

        ''' <summary>
        ''' Data belongs to an Ecospace habitat.
        '''</summary>
        EcospaceHabitat = 15

        ' ''' <summary>
        ' ''' Data belongs to an Ecospace region.
        ' '''</summary>
        'EcospaceRegion = 16

        ''' <summary>
        ''' Data belongs to an Ecospace group.
        '''</summary>
        EcospaceGroup = 17

        ''' <summary>
        ''' Data belongs to an Ecospace fleet.
        '''</summary>
        EcospaceFleet = 18

        ''' <summary>
        ''' Data belongs to an Ecospace MPA.
        '''</summary>
        EcospaceMPA = 19

        ''' <summary>
        ''' Data belongs to the Ecospace model parameters,
        ''' which instruct how to run an Ecopace scenario.
        '''</summary>
        EcospaceModelParameter = 20

        ''' <summary>
        ''' Data belongs to a cEcospaceModelBasemaps instance.
        '''</summary>
        EcospaceBasemap = 21

        ''' <summary>
        ''' Data belongs to an ecospace importance layer instance.
        '''</summary>
        ''' <remarks>The enum value </remarks>
        EcospaceLayerImportance = 22

        ''' <summary>
        ''' cPredPreyInteraction object
        '''</summary>
        PredPreyInteraction = 23

        ''' <summary>
        ''' Time step results of the currently running Ecospace model
        '''</summary>
        EcospaceTimestepResults = 24

        ''' <summary>
        ''' Data belongs to Ecospace calculated values for a single group.
        '''</summary>
        EcospaceGroupOuput = 25

        ''' <summary>
        ''' Data belongs to Ecospace calculated values for a single fleet.
        '''</summary>
        EcospaceFleetOuput = 26

        ''' <summary>
        ''' Data belongs to Ecospace calculated values for a single region.
        '''</summary>
        EcospaceRegionResults = 27

        '''' <summary>
        '''' Data belongs to Network Analysis.
        ''''</summary>
        'NetworkFlowOutput = 28

        ''' <summary>
        ''' Data belongs to a time series that applies to groups.
        '''</summary>
        GroupTimeSeries = 29

        ''' <summary>
        ''' Data belongs to a time series that applies to fleets.
        '''</summary>
        FleetTimeSeries = 30

        ''' <summary>
        ''' Data belongs to a Time Series data set.
        '''</summary>
        TimeSeriesDataset = 31

        ''' <summary>
        ''' Data belongs to a Ecosim Monte Carlo model parameters.
        '''</summary>
        MonteCarlo = 32

        ''' <summary>
        ''' Data belongs to values calculated by Ecosim for a single group.
        '''</summary>
        EcoSimGroupOutput = 33

        ''' <summary>
        ''' Data belongs to values calculated by Ecosim for a single fleet.
        '''</summary>
        EcosimFleetOutput = 34

        ''' <summary>
        ''' Data belongs to Ecosim Fit To Time Series model parameters.
        '''</summary>
        FitToTimeSeries = 35

        ''' <summary>
        ''' Data belongs to an Ecotracer scenario.
        '''</summary>
        EcotracerScenario = 36

        ''' <summary>
        ''' Data belongs to Ecotracer model parameters.
        '''</summary>
        EcotracerModelParameters = 37

        ''' <summary>
        ''' Data belongs to an Ecotracer input group.
        '''</summary>
        EcotracerGroupInput = 38

        ''' <summary>
        ''' Data belongs to an Ecotracer Ecosim results for a single group.
        '''</summary>
        EcotracerSimOutput = 39

        ''' <summary>
        ''' Data belongs to an Ecotracer Ecospace results for a single group.
        '''</summary>
        EcotracerSpaceOutput = 40

        ''' <summary>
        ''' Data belongs to a search objectives manager.
        '''</summary>
        ''' <remarks>
        ''' Search Objectives form the base for the shared search interface 
        ''' ISearchObjective used by Fishing Policy, Ecoseed, MSE and possibly
        ''' other searches. This system is flexible and be extended.
        ''' </remarks>
        SearchObjectiveManager = 41

        ''' <summary>
        ''' Data belongs to search objectives generic parameters.
        '''</summary>
        ''' <remarks>Don't panic.</remarks>
        SearchObjectiveParameters = 42

        ''' <summary>
        ''' Data belongs to search objectives parameters for a single fleet.
        '''</summary>
        SearchObjectiveFleetInput = 43

        ''' <summary>
        ''' Data belongs to search objective weights.
        '''</summary>
        SearchObjectiveWeights = 44

        ''' <summary>
        ''' Data belongs to search objectives parameters for a single group.
        '''</summary>
        SearchObjectiveGroupInput = 45

        ''' <summary>
        ''' Data belongs to the Fishing Policy search manager.
        '''</summary>
        ''' <remarks>
        ''' Note that the Fishing Policy manager may use the SearchObjectivexxxx data types as well.
        ''' </remarks>
        FishingPolicyManager = 46

        ''' <summary>
        ''' Data belongs to fishing policy search generic parameters.
        '''</summary>
        FishingPolicyParameters = 47

        ''' <summary>
        ''' Data belongs to fishing policy search search blocks settings.
        '''</summary>
        FishingPolicySearchBlocks = 48

        ''' <summary>
        ''' Data belongs to the MPA optimizations/Ecoseed search manager.
        '''</summary>
        MPAOptManager = 49

        ''' <summary>
        ''' Data belongs to the MPA optimizations/Ecoseed results.
        '''</summary>
        MPAOptOuput = 50

        ''' <summary>
        ''' Data belongs to the MPA optimizations/Ecoseed generic parameters.
        '''</summary>
        MPAOptParameters = 51

        ''' <summary>
        ''' Data belons to the Management Strategy Evaluator.
        '''</summary>
        MSEManager = 52

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluator parameters for a single fleet.
        '''</summary>
        MSEFleetInput = 53

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluator parameters for a single group.
        '''</summary>
        MSEGroupInput = 54

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluator generic results.
        '''</summary>
        MSEOutput = 55

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluator generic parameters.
        '''</summary>
        MSEParameters = 56

        ''' <summary>
        ''' Data belongs to a single Pedigree level.
        '''</summary>
        PedigreeLevel = 57

        ''' <summary>
        ''' Data belongs to the EwE game engine data.
        '''</summary>    
        GameData = 58

        '''' <summary>
        '''' Data belongs to the Ecosim fisheries regulation engine.
        ''''</summary>    
        'EcosimFisheriesRegulation = 59

        ''' <summary>
        ''' Data belongs to Ecopath statistics.
        '''</summary>
        EcoPathStatistics = 60

        ''' <summary>
        ''' Data belongs to Ecosim statistics.
        '''</summary>
        EcoSimStatistics = 61

        ''' <summary>
        ''' Data belongs to Ecospace statistics.
        '''</summary>
        EcospaceStatistics = 62

        ''' <summary>
        ''' Data belongs to Particle Size Distribution generic parameters.
        '''</summary>
        ParticleSizeDistribution = 63

        ''' <summary>
        ''' Data belongs to the Ecospace Depth layer.
        '''</summary>
        EcospaceLayerDepth = 64

        ''' <summary>
        ''' Data belongs to the Ecospace Marine Protected Areas layer.
        '''</summary>
        EcospaceLayerMPA = 65

        ''' <summary>
        ''' Data belongs to the Ecospace MPA seed layer.
        '''</summary>
        ''' <remarks>
        ''' MPA seeds are used in the MPA optimizations/Ecoseed searches.
        ''' </remarks>
        EcospaceLayerMPASeed = 66

        ''' <summary>
        ''' Data belongs to the Ecospace Habitat layer.
        '''</summary>
        EcospaceLayerHabitat = 67

        ''' <summary>
        ''' Data belongs to the Ecospace Regions layer.
        '''</summary>
        EcospaceLayerRegion = 68

        ''' <summary>
        ''' Data belongs to the Ecospace relative primary production layer.
        '''</summary>
        EcospaceLayerRelPP = 69

        ''' <summary>
        ''' Data belongs to the Ecospace relative contaminant layer.
        '''</summary>
        EcospaceLayerRelCin = 70

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluation results for a single group.
        '''</summary>
        MSEGroupOutputs = 71

        ''' <summary>
        ''' Data belongs to the Ecospace layer representing the spread and quantities
        ''' of Individual Based Model packets.
        '''</summary>
        EcospaceLayerIBMPackets = 72

        ''' <summary>
        ''' Data belongs to the Ecospace layer representing fishing ports.
        '''</summary>
        EcospaceLayerPort = 73

        ''' <summary>
        ''' Data belongs to the Ecospace layer representing cost of sailing.
        '''</summary>
        EcospaceLayerSail = 74

        ''' <summary>
        ''' Data belongs to Ecosim input data for single group.
        '''</summary>
        EcosimFleetInput = 75

        ''' <summary>
        ''' Data belongs to Ecosim results for single group.
        '''</summary>
        EcosimOutput = 76

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluation results for a single fleet.
        '''</summary>
        MSEFleetOutputs = 76

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluation biomass statistical results.
        '''</summary>
        MSEBiomassStats = 77

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluation statistical results on catches by group.
        '''</summary>
        MSECatchByGroupStats = 78

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluation statistical results on catches by fleet.
        '''</summary>
        MSECatchByFleetStats = 79

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluation statistical results on fishing effort.
        '''</summary>
        MSEEffortStats = 80

        ''' <summary>
        ''' Data belongs to the Ecospace Migration layer.
        '''</summary>
        EcospaceLayerMigration = 81

        ''' <summary>
        ''' Data belongs to the Ecospace Advection layer.
        '''</summary>
        EcospaceLayerAdvection = 82

        ''' <summary>
        ''' Data belongs to Auxillary data.
        '''</summary>
        Auxillary = 83

        MSEBioEstStats = 84

        ''' <summary>
        ''' Data belongs to the Ecospace Distribution envelope layer.
        '''</summary>
        EcospaceLayerDistribution = 85

        ''' <summary>
        ''' Data belongs to the Ecospace wind layer.
        '''</summary>
        EcospaceLayerWind = 86

        ''' <summary>
        ''' Data belongs to the Ecospace transport rate layer.
        '''</summary>
        EcospaceLayerTransportRate = 87

        ''' <summary>
        ''' Data belongs to the Ecospace flow layer.
        '''</summary>
        EcospaceLayerFlow = 88

        ''' <summary>
        ''' Data belongs to the Ecospace mixed layer depth layer.
        '''</summary>
        EcospaceLayerMLD = 89

        ''' <summary>
        ''' Data belongs to the Ecospace upwelling layer.
        '''</summary>
        EcospaceLayerUpwelling = 90

        EcospaceAdvectionManager = 91
        EcospaceAdvectionParameters = 92

        PedigreeManager = 93

        ''' <summary>
        ''' Data belongs to Management Strategy Evaluation statistical results total value.
        '''</summary>
        MSEValueTotalStats = 94

        ''' <summary>
        ''' Data belongs to a taxonomy definition.
        '''</summary>
        Taxon = 95

        ''' <summary>
        ''' Data belongs to landings mediation function (as opposed to a 'regular', Pred-Prey mediation function.
        '''</summary>
        PriceMediation = 96

        ''' <summary>
        ''' Data belongs to a landings-mediated interaction.
        '''</summary>
        LandingInteraction = 97

        EcospaceLayerHabitatCapacity = 98
        EcospaceLayerHabitatCapacityInput = 101

        ''' <summary>
        ''' Data belongs to a Capacity Mediation.
        '''</summary>
        CapacityMediation = 99

        ''' <summary>
        ''' Data belongs to a IEnviroInputMap
        '''</summary>
        EcospaceMapResponse = 100

        MSEBatchManager = 101
        MSEBatchParameters = 102
        MSEBatchTFMInput = 103
        MSEBatchFixedFInput = 104
        MSEBatchTACInput = 105

        EcospaceLayerDriver = 106
        EcospaceSpatialDataConnection = 107
        EcospaceSpatialDataSource = 108

        MSEFStats = 109

        MSYManager = 110
        MSYParameters = 111
        EcospaceLayerExclusion = 112
        EcospaceLayerBiomassForcing = 113

        EcospaceLayerBiomassRelativeForcing = 114

        EcosimEnviroResponseFunctionManager = 115
        EcopathSample = 116

        EcoPathFleetOutput = 117

        FleetGroupCatchability = 118

        ''' <summary>
        ''' Data belongs to an external, unspecified source.
        '''</summary>
        External = -9999

    End Enum

#End Region ' Data Types

#Region " Core Counters "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated types indicating the EwE counters that define data structure
    ''' sizes in the various models.
    '''</summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eCoreCounterTypes

        ''' <summary>Unspecified counter</summary>
        NotSet = 0
        ''' <summary>Number of groups across all models</summary>
        nGroups
        ''' <summary>Number of detritus groups across all models</summary>
        nDetritus
        ''' <summary>Number of living groups across all models</summary>
        nLivingGroups
        ''' <summary>Number of fishing fleets across all models</summary>
        nFleets
        ''' <summary>Max number of groups in a single stanza configuration over all stanza groups</summary>
        nMaxStanza
        ''' <summary>Max age for a stanza group</summary>
        ''' <remarks>Age2(iStanza, m_Stanza.Nstanza(iStanza))</remarks>
        nMaxStanzaAge
        ''' <summary>Number of stanza configuratons</summary>
        nStanzas
        ''' <summary>Number of stanzas for a stanza group</summary>
        ''' <remarks>Nstanza(iStanza)</remarks>
        nStanzasForStanzaGroup
        ''' <summary>Number of years to run an Ecosim model</summary>
        nEcosimYears
        ''' <summary>Number of time steps in an Ecosim run</summary>
        nEcosimTimeSteps
        ''' <summary>Number of years to run an Ecospace model</summary>
        nEcospaceYears
        ''' <summary>Number time steps in an Ecospace model</summary>
        nEcospaceTimeSteps
        ''' <summary>Number of Ecospace habitats</summary>
        nHabitats
        ''' <summary>Number of Ecospace regions</summary>
        nRegions
        ''' <summary>Number of months per year</summary>
        ''' <remarks>Albeit quite obvious and constant, this value is added to facilitate automatic array resizing.</remarks>
        nMonths
        ''' <summary>Number of Ecospace MPAs</summary>
        nMPAs
        ''' <summary>Number of trophic levels from the Network analysis</summary>
        nTrophicLevels
        ''' <summary>Number of available time series</summary>
        nTimeSeries
        ''' <summary>Number of applied time series</summary>
        nTimeSeriesApplied
        ''' <summary>Max number of years over all time series</summary>
        nTimeSeriesYears
        ''' <summary>Number of time series datasets</summary>
        nTimeSeriesDatasets
        ''' <summary>Number of importance layers</summary>
        nImportanceLayers
        ''' <summary>Number of environmental driver layers</summary>
        nEnvironmentalDriverLayers
        ''' <summary>Number of years the game simulation can run for</summary>
        nGameSimYears
        ''' <summary>Number of timesteps the game simulation can run for</summary>
        nGameSimTimeSteps
        ''' <summary>Number of timesteps per year</summary>
        nGameSimTimeStepsPerYear

        ''' <summary>Number of rows in the Ecospace basemap</summary>
        nRows
        ''' <summary>Number of columns in the Ecospace basemap</summary>
        nCols

        ''' <summary>Number of timesteps in the Ecopath Weight, Number and Biomass</summary>
        nEcopathAgeSteps
        ''' <summary>Number of weight classes in the particle size distribution</summary>
        nWeightClasses

        ''' <summary>Number of forcing function that are for Salinity</summary>
        ''' <remarks>At this time this is only used by the Decision Support Tool(game).</remarks>
        nSalinityForcingFunctions

        ''' <summary>Number of forcing function that are for Salinity</summary>
        ''' <remarks>At this time this is only used by the Decision Support Tool(game).</remarks>
        nNutrientForcingFunctions
        ''' <summary>Number of forcing function that are for Salinity</summary>
        ''' <remarks>At this time this is only used by the Decision Support Tool(game).</remarks>
        nTempForcingFunctions

        ''' <summary>Number of forcing function that are for Primary Producer</summary>
        ''' <remarks>At this time this is only used by the Decision Support Tool(game).</remarks>
        nPPForcingFunctions

        ''' <summary>The number of iterations running in the game</summary>
        nGameIterations

        ''' <summary>The number of taxonomy groups</summary>
        nTaxon
        ''' <summary>The number of taxa assigned to a given group</summary>
        nTaxonForGroup

        ''' <summary>The number of supported pedigree variables</summary>
        nPedigreeVariables
        ''' <summary>The number of supported capacity maps</summary>
        nCapacityMaps

        ''' <summary>Number of Target Fishing Mort iterations</summary>
        nMSEBatchTFM
        ''' <summary>Number of Fixed F iterations</summary>
        nMSEBATCHFixedF
        ''' <summary>Number of TAC iterations</summary>
        nMSEBATCHTAC

        ''' <summary>Number of available Ecospace result writers <seealso cref="IEcospaceResultsWriter"></seealso></summary>
        nEcospaceResultWriters
        nVectorFields

    End Enum

#End Region ' Core counters

#Region " Status flags "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Public enumerator stating the status of a variable used by cVariableStatus class to state the status of the parameter.
    ''' Used by the data wrapper classes to state the status of a variable see cEcoPathGroupInputs.EEStatus
    ''' </summary>
    ''' <remarks>
    ''' <para>Can be used in combination with eVarNameFlags to tell the status of a parameter</para>
    ''' <para>Mulitple eStatusFlags can be joined together using the bitwise OR operator to signify 
    ''' multiple statuses for a variable.</para>
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    <Flags()>
    Public Enum eStatusFlags

        ''' <summary>
        ''' All is well.
        ''' </summary>
        OK = 1

        ''' <summary>
        ''' Failed data validation.
        ''' </summary>
        FailedValidation = 2

        ''' <summary>
        ''' Value is computed from other values.
        ''' </summary>
        ValueComputed = 4

        ''' <summary>
        ''' Model computed an invalid result.
        ''' </summary>
        InvalidModelResult = 8

        ''' <summary>
        ''' Value is not editable because other related variables imply their value.
        ''' </summary>
        ''' <remarks>
        ''' This flag is also known as ReadOnly (Windows) or BlockedForInput (EwE5).
        ''' </remarks>
        NotEditable = 16

        ''' <summary>
        ''' Unknown error encountered.
        ''' </summary>
        ''' 
        ErrorEncountered = 32

        ''' <summary>
        ''' Value should have been provided at the start of a model run.
        ''' </summary>
        ''' <remarks>
        ''' This flag resembles <see cref="eStatusFlags.FailedValidation">FailedValidation</see>
        ''' but the reason for the failure is specific to the flag.
        ''' </remarks>
        MissingParameter = 64

        ''' <summary>
        ''' Value should be highlighted as decreed by the core for whatever reason.
        ''' </summary>
        ''' <remarks>
        ''' This can occur when the core determines that particular values have relevant
        ''' links to other values. The core can only know this and can request any GUI
        ''' to hightlight such values.
        ''' </remarks>
        CoreHighlight = 128

        ''' <summary>
        ''' Variable is null, its value has not been set.
        ''' </summary>
        Null = 256

        ''' <summary>
        ''' Variable is being stored in the EwE database system.
        ''' </summary>
        Stored = 512

    End Enum

#End Region ' Status flags

#Region " System units "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated types providing currency types.
    '''</summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eUnitCurrencyType As Integer
        ''' <summary>Unit currency type not set</summary>
        NotSet = 0
        ''' <summary>Currency expressed in j/m²</summary>
        Joules = 1
        ''' <summary>Currency expressed in kcal/m²</summary>
        Calorie = 2
        ''' <summary>Currency expressed in g/m²</summary>
        Carbon = 3
        ''' <summary>Currency expressed in dry weight (g/m²)</summary>
        DryWeight = 4
        ''' <summary>Currency expressed in wet weight (t/km²)</summary>
        WetWeight = 5
        ''' <summary>Custom currency unit</summary>
        CustomEnergy = 6
        ''' <summary>Currency expressed in mg n/m²</summary>
        Nitrogen = 7
        ''' <summary>Currency expressed in mg p/m²</summary>
        Phosporous = 8
        ''' <summary>Custom currency unit</summary>
        CustomNutrient = 9
    End Enum

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type listing time units.
    '''</summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eUnitTimeType As Integer
        ''' <summary>User has specified a custom time unit</summary>
        Custom = 0
        ''' <summary>Time expressed in years</summary>
        Year
        ''' <summary>Time expressed in days</summary>
        Day
    End Enum

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated types providing area types.
    '''</summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eUnitAreaType As Integer
        ''' <summary>Custom area measure</summary>
        Custom = 0
        ''' <summary>Area expressed in square km</summary>
        Km2
        ''' <summary>Area expressed in square miles</summary>
        Mi2
    End Enum

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated types providing map unit types.
    '''</summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eUnitMapRefType As Integer
        ''' <summary>Custom area measure</summary>
        Custom = 0
        ''' <summary>Meters</summary>
        m
        ''' <summary>Kilometers</summary>
        km
        ''' <summary>Decimal degrees</summary>
        dd
    End Enum

#End Region ' System units

#Region " Quota types "

    'enum values are hard coded so that they can be stored in the database 
    Public Enum eQuotaTypes
        ''' <summary>No Quota controls are used</summary>
        NoControls
        ''' <summary>Quota options apply to the weakest stock</summary>
        Weakest
        ''' <summary>Quota options apply to the strongest stock plus discards</summary>
        HighestValue
        ''' <summary>Quota options apply to selective fishing</summary>
        Selective
        ''' <summary>Quota options apply to effort</summary>
        Effort

        LinearProgramming

    End Enum

#End Region ' Quota types

#Region " Datasource types "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Supported types of data sources.
    '''</summary>
    ''' -------------------------------------------------------------------
    Public Enum eDataSourceTypes
        ''' <summary>No support</summary>
        NotSet = 0
        ''' <summary>Datasource capable of handling EII formatted data.</summary>
        EII = 1
        ''' <summary>Datasource capable of handling MDB formatted data.</summary>
        Access2003 = 2
        ''' <summary>Datasource capable of handling ACCDB and MDB formatted data.</summary>
        Access2007 = 3
        ''' <summary>Datasource capable of handling EIIXML formatted data.</summary>
        EIIXML = 5
        ''' <summary>Data importable from EcoBase.</summary>
        EcoBase
    End Enum

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type describing the result of datasource access attempts.
    '''</summary>
    ''' -------------------------------------------------------------------
    Public Enum eDatasourceAccessType As Integer
        ''' <summary>Database operation successful</summary>
        Success = 0
        ''' <summary>Database could not be saved in the indicated location</summary>
        Failed_CannotSave
        ''' <summary>An unknown database type was requested</summary>
        Failed_UnknownType
        ''' <summary>System does not have the correct drivers installed to
        ''' support the requested database type</summary>
        Failed_OSUnsupported
        ''' <summary>An unknown error has occurred</summary>
        Failed_Unknown
        ''' <summary>No permissions to write to the database</summary>
        Failed_ReadOnly
        ''' <summary>Cannot switch from one type of database to another</summary>
        Failed_TransferTypes
        ''' <summary>Cannot perform requested operation on this type of file</summary>
        Failed_DeprecatedOperation
        ''' <summary>File is not found</summary>
        Failed_FileNotFound
        ''' <summary>Deprecated, use <see cref="eDatasourceAccessType.Success">Sccess</see> instead</summary>
        Opened = Success
        ''' <summary>Deprecated, use <see cref="eDatasourceAccessType.Success">Sccess</see> instead</summary>
        Created = Success
    End Enum

#End Region ' Datasource types

#Region " Search criteria results "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Search criteria result types
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eSearchCriteriaResultTypes As Integer
        TotalValue = 1
        Employment = 2
        MandateReb = 3
        Ecological = 4
        BioDiversity = 5
    End Enum

#End Region ' Search criteria results

#Region " CoreComponentType "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type, identifying sources of messages being broadcasted by the Core.
    '''</summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eCoreComponentType
        ''' <summary>The message source is not specified</summary>
        NotSet
        ''' <summary>The message originated from the Ecopath module of EwE</summary>
        EcoPath
        ''' <summary>The message originated from the Ecosim module of EwE</summary>
        EcoSim
        ''' <summary>The message originated from the Ecospace module of EwE</summary>
        EcoSpace
        ''' <summary>The message originated from the Forcing shapes manager(s) in EwE</summary>
        ShapesManager
        ''' <summary>The message originated from a datasource</summary>
        DataSource
        ''' <summary>The message originated from the core itself</summary>
        Core
        ''' <summary>The message originated from a Plugin</summary>
        Plugin
        ''' <summary>The message originated from the Monte Carlo routines in Ecosim</summary>
        EcoSimMonteCarlo
        ''' <summary>The message originated from the Fit to Time Series routines in Ecosim</summary>
        EcoSimFitToTimeSeries
        ''' <summary>The message originated from a change in loaded Time Series</summary>
        TimeSeries
        ''' <summary>The message originated from the pred/prey interaction</summary>
        MediatedInteractionManager
        ''' <summary>The message originated from Ecotracer</summary>
        Ecotracer
        ''' <summary>The message originated from an external source (such as the user interface)</summary>
        External
        ''' <summary>The message source is one of the Search Objective classes</summary>
        SearchObjective
        ''' <summary>The message originated from Fishing Policy Search</summary>
        FishingPolicySearch
        ''' <summary>Management Strategy Evaluation</summary>
        MSE
        ''' <summary>EcoSeed</summary>
        MPAOptimization
        ''' <summary>The message originated from the MSY module of EwE</summary>
        MSY

        EcospaceResponseInteractionManager
        EcosimResponseInteractionManager
        EcopathSample

    End Enum

#End Region ' CoreComponentType

#Region " Message Type "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type, identifying types of messages being broadcasted by the Core.
    '''</summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eMessageType
        ''' <summary>Message type has not been set</summary>
        NotSet = 0
        ''' <summary>This message could be of any message type</summary>
        Any
        ''' <summary>Diet Comp out of range</summary>
        DietComp
        ''' <summary>Diet Comp correct to 15 percent prompt</summary>
        DietComp_CorrectTo15Perc
        ''' <summary>EE out of range</summary>
        EE
        ''' <summary>Parameters could not be computed because of missing data in input parameters</summary>
        TooManyMissingParameters
        ''' <summary>No Catch for a Fishing Fleet</summary>
        NoCatchForFleet
        ''' <summary>Error encountered during model run</summary>
        ErrorEncountered
        ''' <summary>Data validation message</summary>
        DataValidation
        ''' <summary>Data from the source has been modified</summary>
        DataModified
        ''' <summary>Data has been added to, or removed from, the source</summary>
        DataAddedOrRemoved
        ''' <summary>Data import related issue</summary>
        DataImport
        ''' <summary>Data export related issue</summary>
        DataExport

        '''' <summary>Time step in Ecospace</summary>
        '''' <remarks>This was added for testing and is not used at this time</remarks>
        'EcospaceTimeStep

        ''' <summary>Ecospace has completed a model run</summary>
        EcospaceRunCompleted

        ''' <summary>Sent by any message source when the State Monitor's state not met to run a method</summary>
        StateNotMet

        Progress

        EcosimRunCompleted

        EcosimNYearsChanged
        MassBalance_InsufficientData
        RespirationExceeedsDetritus
        InvalidModel_PB0_Generic
        InvalidModel_QB0_Generic
        InvalidModel_B_Detritus

        ''' <summary>MSE has completed a model run of some sort</summary>
        MSERunCompleted
        Estimate_BA
        Estimate_Net_Migration

        ''' <summary>MSE Batch has updated the iteration values</summary>
        ''' <remarks>
        ''' This was added to deal with a bug in the interface that caused the grid to assert 
        ''' when it refreshed in response to edits. This prevents that by only updating when the message is of this type.
        ''' </remarks>
        MSEBatch_IterationDataUpdated
        ''' <summary>A GIS operation was performed</summary>
        GISOperation
        ''' <summary>EwE-wide settings have changed</summary>
        GlobalSettingsChanged

    End Enum

#End Region ' Message type

#Region " Message Importance "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Flag indicating the relative importance/severity of a message.
    '''</summary>
    ''' <remarks>
    ''' Per 21 November 2014 importance values are ordered by severity.
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Public Enum eMessageImportance
        ''' <summary>Maintenance messages typically indicate a synchronization event
        ''' in the EwE application</summary>
        Maintenance = 0
        ''' <summary>Progress messages typically indicate incremental status
        ''' information about a lengthy operation</summary>
        Progress = 1
        ''' <summary>Information messages typically indicate an event that may be of
        ''' interest to a human user of EwE</summary>
        Information = 2
        ''' <summary>Questions are used to poll the user for regular info</summary>
        Question = 3
        ''' <summary>Warning messages indicating that the system has run in a problem
        ''' and could not complete an operation</summary>
        Warning = 4
        ''' <summary>Critical messages indicate the the system has run into an error
        ''' that it could not recover from. This is the most severe type of message</summary>
        Critical = 5
    End Enum

#End Region ' Message Importance

#Region " Feedback message "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type that defines possible replies to a <see cref="IFeedbackMessage"/>.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eMessageReply As Byte
        ''' <summary>This reply indicates that the situation pertaining to the message has to be aborted</summary>
        CANCEL = 0
        ''' <summary><para>This reply indicates that the situation pertaining to the message is positively confirmed.</para>
        ''' <para>A YES reply is identical to an <see cref="eMessageReply.OK"/> reply.</para></summary>
        YES
        ''' <summary><para>This reply indicates that the situation pertaining to the message is positively confirmed.</para>
        ''' <para>An OK reply is identical to a <see cref="eMessageReply.YES"/> reply.</para></summary>
        OK = YES
        ''' <summary>This reply indicates that the situation pertaining to the message is negatively confirmed</summary>
        NO
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type that defines possible replie styles that a 
    ''' <see cref="IFeedbackMessage"/> can handle.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eMessageReplyStyle As Byte
        ''' <summary>
        ''' The reply expected by a message with this reply style is either 
        ''' <see cref="eMessageReply.OK"/> or <see cref="eMessageReply.CANCEL"/>.
        '''</summary>
        OK_CANCEL
        ''' <summary>
        ''' The reply expected by a message with this reply style is either 
        ''' <see cref="eMessageReply.YES"/> or <see cref="eMessageReply.NO"/>.
        '''</summary>
        YES_NO
        ''' <summary>
        ''' The reply expected by a message with this reply style must be 
        ''' <see cref="eMessageReply.YES"/>, <see cref="eMessageReply.NO"/> 
        ''' or <see cref="eMessageReply.CANCEL"/>.
        '''</summary>
        YES_NO_CANCEL
        ''' <summary>
        ''' The reply expected by a message with this reply style can only be 
        ''' <see cref="eMessageReply.OK"/>.
        '''</summary>
        OK
    End Enum

#End Region ' Feedback message

#Region " Forcing function application targets "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Enumerator for forcing functions, describing the target of a Predator/Prey or 
    ''' Fleet/Group interaction forcing application.
    '''</summary>
    ''' ---------------------------------------------------------------------------
    Public Enum eForcingFunctionApplication As Integer
        NotSet = 0
        SearchRate = 1
        Vulnerability = 2
        ArenaArea = 3
        VulAndArea = 4
        OffVesselPrice = 5
        ProductionRate = 6
        Import = 7
        MortOther = 8
    End Enum

#End Region ' Forcing function application targets

#Region " IUCN threat classifications "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating the IUCN Red List of Threatened Species conservation categories.
    '''</summary>
    ''' <remarks>
    ''' http://www.eoearth.org/article/IUCN_Red_List_Criteria_for_Endangered
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Enum eIUCNConservationStatusTypes As Integer
        ''' <summary>Conservation status has not been specified</summary>
        NotSet = 0
        ''' <summary>Not Evaluated (NE)</summary>
        NotEvaluated
        ''' <summary>Data Deficient (DD)</summary>
        DataDeficient
        ''' <summary>Least Concern (LC)</summary>
        LeastConcern
        ''' <summary>Near Threatened (NT)</summary>
        NearThreatened
        ''' <summary>Vulnerable (VU)</summary>
        Vulnerable
        ''' <summary>Endangered (EN)</summary>
        Endangered
        ''' <summary>Critically Endangered (CR)</summary>
        CriticallyEndangered
        ''' <summary>Extinct in the Wild (EW)</summary>
        ExtinctInWild
        ''' <summary>Extinct (EX)</summary>
        Extinct
    End Enum

#End Region ' IUCN threat classifications

#Region " Ecology types "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating supported taxonomy Habitat classifications, where 
    ''' taxa prefer to dwell. Not necessarily related to Ecospace habitats.
    '''</summary>
    ''' <remarks>
    ''' As defined in FishBase (http://www.fishbase.org)
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Enum eEcologyTypes As Integer
        ''' <summary>Ecology type has not been specified</summary>
        NotSet = 0
        BathyDemersal
        BathyPelagic
        Bethic
        BenthoPelagic
        Demersal
        Pelagic
        PelagicNeritic
        PelagicOceanic
        ReefAssociated
        ''' <summary>Taxon occurs on land</summary>
        ''' <remarks>To be specified further. Where on land? In a tree? A cave? A sub-urban dwelling made of clay?</remarks>
        LandBased
    End Enum

#End Region ' Ecology types

#Region " Occurrence status types "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating supported taxonomy Occurrence classifications.
    '''</summary>
    ''' <remarks>
    ''' As defined in FishBase (http://www.fishbase.org)
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Enum eOccurrenceStatusTypes As Integer
        ''' <summary>Occurrence status has not been specified</summary>
        NotSet = 0
        Native
        Introduced
        Endemic
        Questionable
    End Enum

#End Region ' Occurrence status types

#Region " Organism types "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating supported taxonomy Organism classifications.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eOrganismTypes As Integer
        ''' <summary>Organism type has not been specified</summary>
        NotSet = 0
        Bacteria
        Fungi
        Algae
        Plants
        Invertebrates
        Fishes
        Birds
        Mammals
        Reptiles
        ''' <summary>Organism type does not fit existing classifications</summary>
        Other
    End Enum

#End Region ' Organism types

#Region " Exploitation types "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating FAO classifications of commercial exploitation of
    ''' species.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eExploitationTypes As Integer
        ''' <summary>Exploitation status is unknown</summary>
        NotSet = 0
        ''' <summary>Exploitation status is not assessed</summary>
        NotAssessed
        ''' <summary>Not being exploited</summary>
        ''' <remarks>Wouldn't that be nice...?</remarks>
        NotExploited
        ''' <summary>
        ''' Undeveloped or new fishery. Believed to have a significant potential 
        ''' for expansion in total production.
        '''</summary>
        UnderExploited
        ''' <summary>
        ''' Exploited with a low level of fishing effort. Believed to have some 
        ''' limited potential for expansion in total production.
        '''</summary>
        ModeratelyExploited
        ''' <summary>The fishery is operating at or close to an optimal yield level, 
        ''' with no expected room for further expansion
        '''</summary>
        FullyExploited
        ''' <summary>The fishery is being exploited at above a level that is believed 
        ''' to be sustainable in the long term, with no potential room for further 
        ''' expansion and a higher risk of stock depletion/collapse</summary>
        OverExploited
        ''' <summary>Catches are well below historical levels, irrespective of the 
        ''' amount of fishing effort exerted</summary>
        Depleted
        ''' <summary>Catches are again increasing after having been depleted</summary>
        Recovering
    End Enum

#End Region ' Exploitation types

#Region " Automated update types "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating possible automated update statuses.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eAutoUpdateTypes As Integer
        ''' <summary>Checking for possible update</summary>
        Checking
        ''' <summary>Downloading update</summary>
        Downloading
        ''' <summary>Update done</summary>
        Done
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating possible automated update results.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eAutoUpdateResultTypes As Integer
        ''' <summary>All good. Blue skies, happy children, money in the bank; the works - no need to leave the couch</summary>
        Success_NoActionRequired = 0
        ''' <summary>Indicated component is not part of the auto-update structure</summary>
        Success_NoEwEComponent
        ''' <summary>Component successfully updated</summary>
        Success_Updated
        ''' <summary>A migration is available</summary>
        Info_CanMigrate
        ''' <summary>An update is available</summary>
        Info_CanUpdate
        ''' <summary>Update webservice could not be connected</summary>
        Error_Connection
        ''' <summary>File failed to download</summary>
        Error_Download
        ''' <summary>Failed to replace a plug-in on the system</summary>
        Error_Replace
        ''' <summary>A generic error occurred</summary>
        Error_Generic
    End Enum

#End Region ' Automated update types

#Region " Ecospace Capacity and Habitat "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating supported Ecospace habitat foraging capacity calculation methods.
    '''</summary>
    ''' -----------------------------------------------------------------------
    <Flags()>
    Public Enum eEcospaceCapacityCalType As Integer
        ''' <summary>Default; base capacity is obtained from input maps.</summary>
        Input = 0
        ''' <summary>Include environmental responses in the capacity calculations.</summary>
        EnvResponses = 1
        ''' <summary>Include habitats in the capacity calculations.</summary>
        Habitat = 2
    End Enum

    ''' <summary>
    ''' Averaging/summary time periods for the Ecospace regions results files
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum eEcospaceResultsAverageType
        TimeStep
        Annual
    End Enum


#End Region ' Ecospace Capacity and Habitat

#Region " MSE Batch "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating supported types of MSE Batch runs.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eMSEBatchIterCalcTypes As Integer
        ''' <summary>To document</summary>
        Percent
        ''' <summary>To document</summary>
        UpperLowerValues
    End Enum

#End Region ' MSE Batch

#Region " MSY "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating how the MSY selects the groups that will have 
    ''' their F varied.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eMSYFSelectionModeType As Integer
        ''' <summary>All Groups that are fishing by this fleet will have there 
        ''' F varied. F for all other groups will remain at Ecopath base F</summary>
        Fleets = 0
        ''' <summary>Only this group will have its F varied. F for all other 
        ''' groups will remain at Ecopath base F</summary>
        Groups
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating the supported types of MSY assessments.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eMSYAssessmentTypes As Integer
        ''' <summary>MSY assessment with frozen pools</summary>
        StationarySystem = 0
        ''' <summary>Full system assessment</summary>
        FullCompensation
        '''' <summary>Redundant flag to tie EwE5 interface and code implementation together</summary>
        'FreezePools = StationarySystem
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating types of supported MSY runs.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eMSYRunTypes As Integer
        ''' <summary>MSY is running for a single Group or Fleet, in all supported
        ''' <see cref="eMSYAssessmentTypes">assessment types</see></summary>
        SingleRunMSY
        ''' <summary>MSY is running the FMSY. Looping over all the groups</summary>
        FMSY
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating how the MSY selects the groups that will have there F varied
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eMSYRunLengthModeTypes As Integer
        ''' <summary>MSY is running to a fixed (relative) F</summary>
        FixedF
        ''' <summary>MSY is running until all catches are depleted</summary>
        ToDepletion
    End Enum

    ''' <summary>
    ''' Run states of the MSE 
    '''</summary>
    ''' <remarks>Passed out via the MSEProgressDelegate</remarks>
    Public Enum eMSYRunStates
        MSYRunStarted
        FullCompRunCompleted
        StationaryRunCompleted
        MSYRunComplete
        MSYRunStopped
    End Enum

#End Region ' MSY

#Region " TriState "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Alternative enumerator to Microsoft.VisualBasic.TriState, added for Mono compatibility.
    '''</summary>
    ''' <remarks>
    ''' The Microsoft.VisualBasic assembly is known to cause problems under Mono and should not be used.
    ''' For full Mono compliance this definition of TriState should be used instead.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Enum TriState As Integer
        UseDefault = -2
        [False] = -1
        [True] = 0
    End Enum

#End Region ' TriState

#Region " Autosave "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating aspects of EwE that can save automatically.
    '''</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eAutosaveTypes As Integer
        ''' <summary>Hmm</summary>
        NotSet = 0
        ''' <summary>Ecopath run results</summary>
        Ecopath
        ''' <summary>Ecosim category</summary>
        Ecosim
        ''' <summary>Ecosim run results</summary>
        EcosimResults
        ''' <summary>Monte Carlo results</summary>
        MonteCarlo
        ''' <summary>MSE results</summary>
        MSE
        ''' <summary>MSY results</summary>
        MSY
        ''' <summary>Ecospace category</summary>
        Ecospace
        ''' <summary>Ecospace run results</summary>
        EcospaceResults
        ''' <summary>MPA optimizations</summary>
        MPAOpt
        ''' <summary>Ecotracer run results</summary>
        Ecotracer
    End Enum

#End Region ' Autosave

#Region " Log levels "

    ''' <summary>
    ''' Enumerated type stating supported levels of detail for the content of log files.
    '''</summary>
    Public Enum eVerboseLevel As Integer
        Disabled = -1
        ''' <summary>Log all generic errors</summary>
        Standard = 0
        ''' <summary>Log details to track application flow in more detail</summary>
        Detailed = 1
    End Enum

#End Region ' Log levels

#Region " Shapes "

    ''' <summary>
    ''' The type of function used to create a shape.
    '''</summary>
    '''<remarks>These enum values are stored in the database. Please do not 
    '''alter the numerical value below, but feel free to add new function 
    '''types.</remarks>
    Public Enum eShapeFunctionType As Long
        NotSet = 0
        Linear = 1
        Sigmoid_Legacy = 2
        Hyperbolic = 3
        Exponential = 4
        Betapdf = 5
        Normal = 6
        RightShoulder = 7
        LeftShoulder = 8
        Trapezoid = 9
        Sigmoid = 10
    End Enum

#End Region ' Shape resolution

#Region " LP Solver "

    Public Enum eSolverReturnValues As Integer
        NOMEMORY = -2
        OPTIMAL = 0
        SUBOPTIMAL = 1
        INFEASIBLE = 2
        UNBOUNDED = 3
        DEGENERATE = 4
        NUMFAILURE = 5
        USERABORT = 6
        TIMEOUT = 7
        PRESOLVED = 9
        PROCFAIL = 10
        PROCBREAK = 11
        FEASFOUND = 12
        NOFEASFOUND = 13
        [ERROR] = 14
    End Enum

#End Region ' LP Solver

#Region " Taxonomy "

    ''' <summary>Enumerated type, identifying taxonomic classification searchable fields.</summary>
    Public Enum eTaxonClassificationType As Long
        Latin = &H1
        Species = &H2
        Genus = &H4
        Family = &H8
        Order = &H10
        [Class] = &H20
        Phylum = &H40
        <Obsolete("Kingdom not supported yet but added for future use")>
        Kingdom = &H80
    End Enum

#End Region ' Taxonomy

End Namespace ' Core
