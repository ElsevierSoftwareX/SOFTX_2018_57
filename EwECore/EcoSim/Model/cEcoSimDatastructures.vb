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

'ToDo: Enable Option Strict On
'Option Strict On

Imports EwEUtils.Core


''' <summary>
''' This class wraps the underlying EcoSim data structures
''' </summary>
''' <remarks>
''' 
''' </remarks>
Public Class cEcosimDatastructures

    Public Const DEFAULT_N_FORCINGPOINTS As Integer = 1200 'min number of forcing point 100 years * FORCING_POINTS_PER_YEAR
    Public Const FORCING_POINTS_PER_YEAR As Integer = 12

    ''' <summary>
    ''' Enumerated index to the type of Ecosim Data saved over time
    ''' </summary>
    ''' <remarks>This is the index to the first element in ResultsOverTime(eEcosimResults, igroup, itime) that specifies the data being saved at each time step across groups</remarks>
    Public Enum eEcosimResults
        Biomass
        BiomassRel
        Yield
        YieldRel
        FeedingTime
        ConsumpBiomass
        TotalMort
        PredMort
        FishMort
        ProdConsump
        AvgWeight
        MortVPred
        MortVFishing
        EcoSysStructure
        TL
    End Enum

    Public Enum eEcosimPreyPredResults
        Prey
        Pred
        Consumption
    End Enum

    ''' <summary>
    ''' Mediation data for Biomass Mediation
    ''' </summary>
    ''' <remarks></remarks>
    Public BioMedData As New cMediationDataStructures

    ''' <summary>
    ''' Mediation data for Price Elasticity (mediation function)
    ''' </summary>
    ''' <remarks></remarks>
    Public PriceMedData As New cMediationDataStructures

    ''' <summary>
    ''' Capacity Environmental Response functions (mediation functions). 
    ''' Shape to convert input value to capacity value e.g. CapacityMap(irow,icol) = F(InputMap(irow,icol)) (capacity as a function of X)
    ''' </summary>
    ''' <remarks>
    ''' Generical Environmental Response functions are used to convert an enviromental input value to a value used in either Ecosim(time series input data) or Ecospace(spatial/temporal data)
    ''' For Capacity Environmental Response functions are used in conjunction with a cEnviroInputMap(of T) to populate the Capacity map
    ''' </remarks>
    Public CapEnvResData As New cMediationDataStructures

    ''' <summary>
    ''' Boolean flag set by the calling routine to tell Ecosim if it should process the output timestep data cEcoSImModel.ProcessTimeStep()
    ''' </summary>
    ''' <remarks>If true then Ecosim will compute all output data including data over time and call the timestep delegate. If false then it will run in a silent mode and not compute output data  </remarks>
    Public bTimestepOutput As Boolean

    ''' <summary>Array of Ecosim group database IDs.</summary>
    Public GroupDBID() As Integer
    ''' <summary>Array of Ecosim fleet database IDs.</summary>
    Public FleetDBID() As Integer

    ''' <summary>Total number of groups in the model.</summary>
    Public nGroups As Integer
    ''' <summary>Total number of fleets in the model.</summary>
    Public nGear As Integer

    Public FirstTime As Boolean

    ' Public ConSimOn As Boolean
    Public TrophicOff As Boolean
    Public IndicesOn As Boolean
    Public UseVarPQ As Boolean
    Public NudgeChecked As Boolean
    Public Integrate As Boolean
    Public AbortRun As Boolean
    Public EvolveIsOn As Boolean
    Public BiomassOn As Boolean
    Public ActivePair As Integer
    Public ForagingTimeLowerLimit As Single = 0.01!

    ''In the original code
    ''CperFlag is set to true when the EcoSim main form is loaded it is never set to false 
    ''this means it has no effect as EcoSim cannot be run without loading the form
    'Public CperFlag As Boolean

    ''' <summary>Duration of simulation (years).</summary>
    Public NumYears As Integer
    ''' <summary>Number of steps per year.</summary>
    Public NumStepsPerYear As Integer = cCore.N_MONTHS
    ''' <summary>Integration steps (per year).</summary>
    Public StepSize As Single

    ''' <summary>Relaxation parameter [0,1].</summary>
    Public SorWt As Single
    ''' <summary>Discount rate (% per year).</summary>
    Public Discount As Single
    ''' <summary>Equilibrium step size.</summary>
    Public EquilibriumStepSize As Single
    ''' <summary>Equilibrium max. fishing rate (relative).</summary>
    Public EquilScaleMax As Single
    ''' <summary>Base proportion of free nutrients.</summary>
    Public NutBaseFreeProp As Single

    Public ReadOnly Property NumEnvResponseFunctions As Integer
        Get
            Return Me.NumForcingShapes
        End Get
    End Property

    ''' <summary>
    ''' Index of the Response function that has been applied to this EnviromentalDrive and Group (driver,group)
    ''' </summary>
    ''' <remarks>EnvRespFuncIndex(1,2) = 10 means that the tenth response function has been applied to the first environmental driver and second group</remarks>
    Public EnvRespFuncIndex(,) As Integer

    'dimensions for nutrient calculation
    Public NutMin As Single

    ''' <summary>Sum of biomass across all groups </summary>
    Public NutBiom As Single

    ''' <summary>Total nutrient bound in system </summary>
    ''' <remarks>NutTot = NutBiom / (1 - NutBaseFreeProp)</remarks>
    Public NutTot As Single

    ''' <summary>Nutrient free in the enviroment  </summary>
    ''' <remarks>NutFree = NutTot - NutBiom</remarks>
    Public NutFree As Single
    Public NutFreeBase() As Single

    Public VulMultAll As Single

    ''' <summary>
    ''' Vulnerability multiplier of a prey to a predator
    ''' </summary>
    ''' <remarks>VulMult(iPrey,iPred) User entered value to increase the vulnerability of a prey</remarks>
    Public VulMult(,) As Single
    Public vulrate(,) As Single

    Public Epower() As Single
    Public PcapBase() As Single
    Public CapDepreciate() As Single
    Public CapBaseGrowth() As Single

    ''' <summary>Toggle to enable TL calculations during searches. Normally this 
    ''' is not enabled because TL info is not used by searches.</summary>
    Public bAlwaysCalcTLc As Boolean = False

    ''' <summary>TL of catch (x time)</summary>
    Public TLC() As Single
    ''' <summary>FIB index (x time)</summary>
    Public FIB() As Single
    ''' <summary>TL based on Ecosim diets (x group)</summary>
    Public TLSim() As Single
    ''' <summary>Total catch per timestep</summary>
    Public CatchSim() As Single
    ''' <summary>Kemptons's Q</summary>
    Public Kemptons() As Single
    ''' <summary>Shannon Diversity Index</summary>
    Public ShannonDiversity() As Single


    ''' <summary> Max vulnerability across all prey for this predator VulnerabilityPredator(pred) = max(VulMult(prey,pred))</summary>
    Public VulnerabilityPredator() As Single

    Public maxflow(,) As Single

    Public FlowType(,) As Single

    Public Eatenof() As Single
    Public Eatenby() As Single
    Public simDCAtT(,) As Single

    ''' <summary>Nutrient loading forcing function number. This is an index in the tval() array.</summary>
    Public NutForceNumber As Integer
    ''' <summary>Max PB/(Base PB) due to nutrient concent.</summary>
    Public NutPBmax As Single
    ''' <summary>System recovery (+/- %).</summary>
    Public SystemRecovery As Single

    'dimensioned by nGroups
    ''' <summary>Max relative feeding time.</summary>
    Public FtimeMax() As Single
    ''' <summary>Feeding time adjustment rate (0-1).</summary>
    Public FtimeAdjust() As Single
    ''' <summary>Fraction of other mortality.</summary>
    Public MoPred() As Single

    ''' <summary>
    ''' Mortality other computed as (1-ee)*pb
    ''' </summary>
    ''' <remarks></remarks>
    Public mo() As Single

    ''' <summary>Predation effect on feeding time (0-1).</summary>
    Public RiskTime() As Single
    ''' <summary>Density-dependant catchability QMax/Qo.</summary> 
    Public QmQo() As Single
    ''' <summary>QBmax/QBo for handling time > 1.</summary> 
    Public CmCo() As Single
    ''' <summary>Switching power parameter (0-2).</summary>
    Public SwitchPower() As Single

    Public BaseTimeSwitch() As Single

    'jb moved vbK to Ecopath
    'Public vbK() As Single 'VBGF curvature parameter K (/year)
    Public PBmaxs() As Single 'max relative P/B

    Public RecPower() As Single

    Public Emig() As Single    'relative to biomass,

    ''' <summary>
    ''' Base consumption on a prey by a predator 
    ''' </summary>
    ''' <remarks>
    ''' Consumption(iPrey,iPred) 
    ''' computed in <see cref="Ecosim.cEcoSimModel.CalcEatenOfBy">Ecosim.CalcEatenOfBy</see>
    ''' </remarks>
    Public Consumption(,) As Single

    Public Htime() As Single

    Public SimDC(,) As Single 'diet composition for EcoSim
    'dimensioned by nPairs holds the iGroup (index) of the adult for this pair
    'i.e. GroupName(iadult(1)) is the group name of the adult for the first pair (assuming there is at least one pair)
    Public iadult() As Single
    Public ijuv() As Single 'same as iadult() but for the juvenile

    Public TimeJuv() As Single
    Public maxtimejuv() As Single
    Public mintimejuv() As Single

    ''' <summary>
    ''' Flag for doing integration in rk4 for each group
    ''' </summary>
    ''' <remarks>This can be set to one of several value:
    ''' value = i "NoIntegrate(1) = 1" do the normal integration,
    ''' value = 0 "NoIntegrate(1) = 0" no integration,
    ''' value less than 0 "NoIntegrate(2)= -2" this is a stanza group the final integration is handled by SplitUpdate()
    ''' this was also used to tell if a group is part of a splitpool 
    ''' </remarks>
    Public NoIntegrate() As Integer

    ''' <summary>Mortality due to fishing FCatch(group) / EcopathBiomass(group) by group </summary>
    ''' <remarks>Initialized in SetupSimVariables() </remarks>
    Public Fish1() As Single

    ''' <summary>
    ''' Mortality due to fishing at the current time step
    ''' </summary>
    Public FishTime() As Single

    ''' <summary>Max catch rate ??? </summary>
    ''' <remarks>stored in database defaults set in SetUpSimVariables(). This may not be neccessary in EwE6 it was only used for scaling in EwE5</remarks>
    Public FishRateMax() As Single

    ''' <summary>
    ''' Fishing mortality by Fleet, Group
    ''' </summary>
    ''' <remarks> Array element FishMGear(nFleets + 1,iGroup) will contain the sum across all fleets and should be the same as Fish1()</remarks>
    Public FishMGear(,) As Single

    ''' <summary>
    ''' Fishing mortality over time for each group.
    ''' </summary>
    ''' <remarks>Fishing mortality by group-time. It's default value is Fish1() Catch/Biomass set in DefaultFishMortalityRates(). 
    ''' It is used as a forcing/driving value over time. It is used to compute FishTime() </remarks>
    Public FishRateNo(,) As Single
    Public FishRateNoDBID() As Integer
    Public FishRateNoTitle() As String
    ''' <summary>Fish rate no shape DBID per group</summary>
    Friend GroupFishRateNoDBID() As Integer

    ''' <summary>
    ''' Fishing Effort multiplier relative to Ecopath base, by Fleet, Time.
    ''' </summary>
    ''' <remarks>Zero removes all fishing, one sets fishing effort to Ecopath value, two would double the fishing mortality for all groups fished by this fleet.
    '''  Used to scale the FishRateNo() for all the groups fished by a fleet.</remarks>
    Public FishRateGear(,) As Single
    Public FishRateGearBasis() As Single
    Public FishRateGearDBID() As Integer
    Public FishRateGearTitle() As String

    ''' <summary>
    ''' Feeding Time scaling value
    ''' </summary>
    ''' <remarks>Default value = one set in InitialState. Computed at the end of each time step in rk4()</remarks>
    Public Ftime() As Single
    Public Hden() As Single

    Public QBoutside() As Single

    ''' <summary>
    ''' Base rate of Detritus accumulation ([accumulated detritus biomass]/[biomass t=0]) calculated in <see cref="EwECore.Ecosim.cEcoSimModel.SimDetritusMT">SimDetritusMT</see>.
    ''' </summary>
    ''' <remarks>
    ''' Calculated during the initialization of Ecosim by <see cref="EwECore.Ecosim.cEcoSimModel.Init">Init()</see> and <see cref="EwECore.Ecosim.cEcoSimModel.SetTimeSteps">SetTimeSteps()</see>. 
    ''' Used by both Ecosim and Ecospace. When Ecospace initializes it calls Ecosim.Init() this sets DetritusOut() to the base Ecosim values. 
    ''' This avoids any issues with setting the initial biomass or threading races. 
    ''' </remarks>
    Public DetritusOut() As Single
    Public AssimEff() As Single
    Public SimGE() As Single

    Public StartBiomass() As Single

    ''' <summary>pbbiomass = (PB * MaxPB / PB - 1) / B </summary>
    ''' <remarks>For primary producers only. Will be zero for all other groups</remarks>
    Public pbbiomass() As Single
    Public loss() As Single

    Public Cbase() As Single

    ''' <summary>
    '''  Catch Rate at Ecopath base. Include all catch even discards that survive. set in SetRelativeCatchabilities
    ''' </summary>
    ''' <remarks>EcopathCatch / StartBiomass</remarks>
    Public relQ(,) As Single

    Public relQt(,,) As Single ' ToDo: Init to cCore.NULL when redimensioning

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'Forcing & Mediation Functions
    'Added or Changed Varaibles
    'jb April-06-2006 added to keep track of the type of a forcing shape (time or Egg)this does not include mediation shapes as they are stored seperately
    Public ForcingShapeType() As eDataTypes

    Public Elect(,,) As Single
    ''' <summary>
    '''  Mortalily rate due to predation by Link
    ''' </summary>
    Public MPred() As Single

    ''' <summary>
    '''  Detritus from all sources by group
    ''' </summary>
    Public GroupDetritus() As Single

    ''' <summary>
    ''' Structure to contain all settings that wrap the primitive defining the contents of a forcing or time shape
    ''' </summary>
    ''' <remarks>
    ''' jb April-07-2006 replaced Shapes() with ShapeParameters
    ''' </remarks>
    Public Structure ShapeParameters

        ''' <summary>
        ''' The <seealso cref="eShapeFunctionType">primite function</seealso> 
        ''' that defined the content of a forcing or time shape.
        ''' </summary>
        ''' <remarks>
        ''' 0 must be supplied if there is no underlying primitive function. 
        ''' Note that plug-ins can add their own ShapeFunctionType, values will
        ''' not be restricted to <see cref="eShapeFunctionType"/>.
        ''' </remarks>
        Public ShapeFunctionType As Long

        ''' <summary>
        ''' The parameters for <see cref="ShapeFunctionType"/>.
        ''' </summary>
        Public ShapeFunctionParams As Single()

        ''' <summary>
        ''' The number of parameters for <see cref="ShapeFunctionType"/>.
        ''' </summary>
        Public ReadOnly Property nShapeFunctionParams As Integer
            Get
                If (Me.ShapeFunctionParams Is Nothing) Then Return 0
                Return Me.ShapeFunctionParams.Length
            End Get
        End Property

        ''' <summary>
        ''' Get/set the value of a <see cref="ShapeFunctionParams"/>.
        ''' </summary>
        ''' <param name="iParam">Zero-based param index</param>
        Public Property ShapeFunctionParam(iParam As Integer) As Single
            Get
                If (iParam < 0 Or iParam >= Me.nShapeFunctionParams) Then Return 0
                Return Me.ShapeFunctionParams(iParam)
            End Get
            Set(value As Single)
                If (iParam < 0 Or iParam >= Me.nShapeFunctionParams) Then Return
                Me.ShapeFunctionParams(iParam) = value
            End Set
        End Property

    End Structure

    'there is one ShapeParameters array for each type of shape that has parameters Mediation and Forcing
    'Public MediationShapeParams() As ShapeParameters 'parameters that where used to create a curve from the Database Table and Fields i.e. EcoSimShapes.YZero
    Public ForcingShapeParams() As ShapeParameters 'Time and EggProd

    ''' <summary>
    ''' Unique database IDs for forcing shapes.
    ''' </summary>
    ''' <remarks>
    ''' Because Time(Forcing) and EggProd shapes are stored in the same arrays and this is for both shape types
    ''' </remarks>
    Public ForcingDBIDs() As Integer
    'is this shape a seasonal forcing shape
    Public isSeasonal() As Boolean

    ''' <summary>Total number of links/flow between groups</summary>
    Public inlinks As Integer
    ''' <summary>iPrey for inlinks</summary>
    Public ilink() As Integer '
    ''' <summary>iPred for inlinks</summary>
    Public jlink() As Integer

    'Forcing
    Public NumForcingShapes As Integer
    Public ForcingTitles() As String

    Public ForcePoints As Integer = DEFAULT_N_FORCINGPOINTS 'number of points per forcing function
    Public ZmaxScale As Single
    Public zscale(,) As Single 'ReDim Preserve zscale(ForcePoints, ForcingShapes + 3) 
    Public tval() As Single

    Public EggProdShape() As Single
    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    ''' <summary> Max PB = PBmax*PB </summary>
    ''' <remarks></remarks>
    Public pbm() As Single
    Public pred() As Single
    Public Qmain() As Single
    Public Qrisk() As Single

    Public Consumpt(,) As Single

    Public DCPct(,) As Single

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'Model Results Over Time for selected variable
    '
    ''' <summary>Number of timesteps in the summary data</summary>
    Public nSumTimeSteps As Integer

    ''' <summary>
    ''' Model results over time (number of variables x groups x time)
    ''' </summary>
    Public ResultsOverTime(,,) As Single
    ''' <summary>pred/prey(2) x groups x groups x time</summary>
    Public PredPreyResultsOverTime(,,,) As Single
    ''' <summary>pred/prey(2) x groups x groups</summary>
    Public ResultsAvgByPreyPred(,,) As Single
    ''' <summary>Group x fleets x time.</summary>
    Public ResultsSumCatchByGroupGear(,,) As Single
    ''' <summary>fleets x time</summary>
    Public ResultsSumCatchByGear(,) As Single

    ''' <summary>Landings by Group, Fleet</summary>
    Public ResultsLandings(,) As Single

    Public ResultsDiscardsMort(,) As Single
    Public ResultsDiscardsSurvived(,) As Single

    ''' <summary>Total discards by Group, fleets, time.</summary>
    Public ResultsTimeDiscardsGroupGear(,,) As Single
    ''' <summary>Discards that suffered mortality by Group, fleets, time.</summary>
    Public ResultsTimeDiscardsMortGroupGear(,,) As Single
    ''' <summary>Discards that survived by Group, fleets, time.</summary>
    Public ResultsTimeDiscardsSurvivedGroupGear(,,) As Single

    ''' <summary>
    ''' Landing by Group, Fleet, Time.
    ''' </summary>
    Public ResultsTimeLandingsGroupGear(,,) As Single

    ''' <summary>
    ''' Fishing mortality by time
    ''' </summary>
    Public ResultsSumFMortByGroupGear(,,) As Single ' groups,fleets,time

    Public ResultsSumValueByGroupGear(,,) As Single
    Public ResultsSumValueByGear(,) As Single
    Public ResultsEffort(,) As Single

    Public ResultsSumRelValueByGroup(,) As Single

    ''' <summary>Summarized Profit from results </summary>
    Public ProfitByFleet() As Single
    ''' <summary>Summarized Jobs from results </summary>
    Public EmploymentValueByFleet() As Single

    'xxxxxxxxxxxxxxxxxxxxxxxx

    ''' <summary>Number of time steps for averaging results.</summary>
    Public NumStep As Integer
    Public NumStep0 As Integer      'Actual number of steps for the zero element of the summary arrays Start summary time period
    Public NumStep1 As Integer      'Actual number of steps for the one element of the summary arrays end summary time peroid

    ''' <summary>Start time of the first and second summary data period. In Years </summary>
    ''' <remarks> Data is summarized over two time periods set by SumStart(0) and SumStart(1). The number of time steps to summarize over is set in NumStep.
    ''' Defaults are set in redimTimeVaraibles().
    ''' Used in cEcospace.summarySetTimeStep() to set the index to store the summary data in. The first or second summary period.
    ''' </remarks>
    Public SumStart(1) As Single

    'storage for summary time period data
    Public SumBiomass(,) As Single 'SumBiomass(iTimePeriod,iGroup)
    'catch by group
    Public SumCatch(,) As Single

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'new foraging arena variables
    Public Narena As Integer, Iarena() As Integer, Jarena() As Integer
    Public ArenaNo(,) As Integer
    Public VulArena() As Single
    Public Alink() As Single
    Public IlinkSet() As Integer 'i index of foraging arena with positive feeding on prey i by predator k
    Public JlinkSet() As Integer 'j index of foraging arena having positive feeding on i by predator k
    Public KlinkSet() As Integer ' index of predator whose peatarea for arenai,j is stored in list element
    Public PeatArena(,) As Single 'diet proportions by foraging arena from/to database
    Public ArenaLink() As Integer
    Public Qlink() As Single 'total ecopath base consumption by trophic link
    Public NlinksSet As Integer 'note number of arena foraging links set from or to database
    Public PeatArenaSetFromDataBase As Boolean
    Public BoutFeeding As Boolean 'this needs an interface
    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    Public RelaSwitch() As Single

    'moved here for Network Analysis plugin
    Public ToDetritus() As Single

    Public FisForced() As Boolean

    ''' <summary>
    ''' Sum of squares fit to reference data
    ''' </summary>
    Public SS As Single

    ''' <summary>
    ''' Sum of squares by group
    ''' </summary>
    ''' <remarks></remarks>
    Public SSGroup() As Single

    Public PredictSimEffort As Boolean

    ''' <summary>
    ''' Boolean flag that tells Ecosim to run on it's own thread.
    ''' </summary>
    ''' <remarks>
    ''' True if Ecosim is running on a seperate thread. False otherwise. 
    ''' This flag can only be set from code and it not available from the scientific interface. 
    ''' The scientific interface is suppose to be thread safe but plugin interfaces may NOT be. 
    ''' Once the Ecosim run has completed bMultiThreaded will be set to False. bMultiThreaded will only be true for one run.
    ''' </remarks>
    Public bMultiThreaded As Boolean

    ''' <summary>
    ''' Number of sub timesteps Ecosim will run per month 
    ''' </summary>
    ''' <remarks>
    ''' Monthly timesteps in Ecosim can be divided into multiple sub timesteps. The number of sub timesteps in set via the cEcosimDatastructures.StepsPerMonth which has a default of one.  
    ''' This allows a plugin to run Ecosim with more then 12 timesteps per year. Once Ecosim has run it will reset cEcosimDatastructures.StepsPerMonth to its default value of one 
    ''' all subsequent runs of Ecosim will be on a monthly timestep unless cEcosimDatastructures.StepsPerMonth has been reset before the next run. 
    ''' This funtionality is only available via code and has no user interface. 
    ''' User interface objects e.g. cCore.EcosimGroupOutputs are NOT update for sub timesteps and will not be updated until the end of the monthly timestep. 
    ''' </remarks>
    Public StepsPerMonth As Integer

    ''' <summary>Proportion of regulated landings (by gear group) for the current time step</summary>
    Public PropLandedTime(,) As Single

    ''' <summary>
    ''' Proportion of the total catch that suffered mortality for the current time step (by gear group). Does not include discards that survived.
    ''' Initialized in cEcosim.InitPropLanded() Propdiscardtime(iflt, igrp) = PropDiscard(iflt, igrp) * PropDiscardMort(iflt, igrp)
    ''' </summary>
    Public Propdiscardtime(,) As Single

    Public PropDiscardMortTime(,) As Single


    ''' <summary>
    ''' Unit conversion factor for fishing effort 
    ''' </summary>
    ''' <remarks>Used to sum effort into a single output map</remarks>
    Public EffortConversionFactor() As Single


    Public lstEnviroInputData As List(Of IEnviroInputData)

    Public EcosimEnvResFunctions As New cMediationDataStructures

    Public Sub RedimVars()

        'jb I don't know why these where split up there may be some kind of a reason
        RedimVariabs1()
        RedimVariabs2()
        'jb added this was in ReadStanza
        '  redimStanza()

    End Sub

    Private Sub RedimVariabs2()

        ReDim Consumption(nGroups, nGroups)
        ReDim Consumpt(nGroups, nGroups)
        ReDim Eatenby(nGroups)
        ReDim Eatenof(nGroups)
        ReDim pred(nGroups)
        ReDim simDCAtT(nGroups, nGroups)
        ReDim DCPct(nGroups, 3) 'used for B1Round, B2Round, QB, derivt (BA)

        'ReDim rzero(nGroups)
        'ReDim wzero(nGroups)  'weight at recruitment to juvenile stage

    End Sub


    Private Sub RedimVariabs1()
        Dim i, j As Integer

        ReDim GroupDBID(nGroups)

        'ReDim BaseTimeSwitch(nGroups)
        ReDim SwitchPower(nGroups)
        NutBaseFreeProp = 0.9999
        NutForceNumber = 0
        NutPBmax = 1.5

        ReDim Emig(nGroups)
        ReDim QmQo(nGroups), Htime(nGroups) ', Hden(nGroups)
        ReDim CmCo(nGroups)

        ReDim Qmain(nGroups)
        ReDim Qrisk(nGroups)
        ReDim RiskTime(nGroups)
        ReDim Consumption(nGroups, nGroups)

        ReDim Eatenby(nGroups)
        ReDim Eatenof(nGroups)
        ReDim EggProdShape(CInt(nGroups / 2))

        ReDim FtimeAdjust(nGroups)
        ReDim MoPred(nGroups)

        ReDim iadult(CInt(nGroups / 2))
        ReDim ijuv(CInt(nGroups / 2))

        ReDim TimeJuv(CInt(nGroups / 2)) 'Time spent in juv stage
        ReDim maxtimejuv(CInt(nGroups / 2))
        ReDim mintimejuv(CInt(nGroups / 2))
        ReDim NoIntegrate(nGroups)
        ReDim pbm(nGroups)
        ReDim PBmaxs(nGroups)
        ReDim pred(nGroups)
        ReDim RecPower(CInt(nGroups / 2))

        ReDim ilink(nGroups * nGroups)
        ReDim jlink(nGroups * nGroups)
        ReDim SimDC(nGroups, nGroups)
        ReDim MPred(nGroups * nGroups)

        ReDim vulrate(nGroups, nGroups)
        ReDim VulMult(nGroups, nGroups)
        For i = 1 To nGroups : For j = 1 To nGroups : vulrate(i, j) = 1.0! : VulMult(i, j) = 2.0! : Next j : Next i
        ReDim VulnerabilityPredator(nGroups)

        ReDim Fish1(nGroups)
        ReDim FishRateNoDBID(nGroups)
        ReDim FishRateNoTitle(nGroups)
        ReDim GroupFishRateNoDBID(nGroups)

        'the plus one is for combined fleets
        ReDim FleetDBID(nGear + 1)
        ReDim FishRateGearDBID(nGear + 1)
        ReDim FishRateGearBasis(nGear + 1)
        ReDim FishRateGearTitle(nGear + 1)
        ReDim FishMGear(nGear + 1, nGroups)

        ReDim FishRateMax(nGroups)

        ReDim FisForced(nGroups)

        ReDim relQ(nGear, nGroups)

        ReDim SSGroup(nGroups)

        ReDim TLSim(nGroups)

        ReDim GroupDetritus(nGroups)

        ReDim Epower(nGear)
        ReDim PcapBase(nGear)
        ReDim CapDepreciate(nGear)
        ReDim CapBaseGrowth(nGear)

        ReDim PropLandedTime(nGear, nGroups)
        ReDim Propdiscardtime(nGear, nGroups)

        ReDim PropDiscardMortTime(nGear, nGroups)

        ReDim EffortConversionFactor(nGear)

        ' JS 3May16: make sure there is no overhang from past scenarios
        Me.lstEnviroInputData.Clear()

    End Sub

    Public Sub RedimOutputsByTime(ByVal nTimesteps As Integer)
        ReDim FIB(nTimesteps)
        ReDim TLC(nTimesteps)     'TL of catch in Ecosim
        ReDim Kemptons(nTimesteps)
        ReDim ShannonDiversity(nTimesteps)
        ReDim CatchSim(nTimesteps)

    End Sub

    ''' <summary>
    ''' Set the FisForced() array to False of all groups
    ''' </summary>
    ''' <remarks>This is called before loading forcing data (DoDatValCalulations())to clear out the old flags. 
    '''  EwE5 never clears this flag once set to True when forcing data is loaded this stays set and FishRateNo() is reset via a the interface, strange?</remarks>
    Public Sub clearFishForced()
        For igrp As Integer = 1 To nGroups
            FisForced(igrp) = False
        Next
    End Sub


    Public Sub Clear()
        Me.nGroups = 0
        Me.nGear = 0

        Me.eraseResults()

        'NTimes is the number of time step for the current number of years
        Me.FishRateNo = Nothing ' (nGroups, nTimeSteps))  'was 1200
        Me.FishRateGear = Nothing '  (nGear + 1, nTimeSteps))  'was 1200

        Me.FIB = Nothing ' (nTimesteps)
        Me.TLC = Nothing ' (nTimesteps)     'TL of catch in Ecosim
        Me.Kemptons = Nothing ' (nTimesteps)
        Me.ShannonDiversity = Nothing
        Me.CatchSim = Nothing ' (nTimesteps)

        Me.GroupDBID = Nothing ' (nGroups)

        'me.BaseTimeSwitch = nothing ' (nGroups)
        Me.SwitchPower = Nothing ' (nGroups)

        Me.Emig = Nothing ' (nGroups)
        Me.QmQo = Nothing ' (nGroups), Htime = nothing ' (nGroups) ', Hden = nothing ' (nGroups)
        Me.CmCo = Nothing ' (nGroups)

        Me.Qmain = Nothing ' (nGroups)
        Me.Qrisk = Nothing ' (nGroups)
        Me.RiskTime = Nothing ' (nGroups)
        Me.Consumption = Nothing ' (nGroups, nGroups)

        Me.Eatenby = Nothing ' (nGroups)
        Me.Eatenof = Nothing ' (nGroups)
        Me.EggProdShape = Nothing ' (CInt = nothing ' (nGroups / 2))
        Me.FtimeAdjust = Nothing ' (nGroups)
        Me.MoPred = Nothing ' (nGroups)

        Me.iadult = Nothing ' (CInt = nothing ' (nGroups / 2))
        Me.ijuv = Nothing ' (CInt = nothing ' (nGroups / 2))

        Me.TimeJuv = Nothing ' (CInt = nothing ' (nGroups / 2)) 'Time spent in juv stage
        Me.maxtimejuv = Nothing ' (CInt = nothing ' (nGroups / 2))
        Me.mintimejuv = Nothing ' (CInt = nothing ' (nGroups / 2))
        Me.NoIntegrate = Nothing ' (nGroups)
        Me.pbm = Nothing ' (nGroups)
        Me.PBmaxs = Nothing ' (nGroups)
        Me.pred = Nothing ' (nGroups)
        Me.RecPower = Nothing ' (CInt = nothing ' (nGroups / 2))

        Me.ilink = Nothing ' (nGroups * nGroups)
        Me.jlink = Nothing ' (nGroups * nGroups)
        Me.SimDC = Nothing ' (nGroups, nGroups)
        Me.MPred = Nothing ' (nGroups * nGroups)

        Me.vulrate = Nothing ' (nGroups, nGroups)
        Me.VulMult = Nothing ' (nGroups, nGroups)
        Me.VulnerabilityPredator = Nothing ' (nGroups)

        Me.Fish1 = Nothing ' (nGroups)
        Me.FishRateNoDBID = Nothing ' (nGroups)
        Me.FishRateNoTitle = Nothing ' (nGroups)
        Me.GroupFishRateNoDBID = Nothing ' (nGroups)

        'the plus one is for combined fleets
        Me.FleetDBID = Nothing ' (nGear + 1)
        Me.FishRateGearDBID = Nothing ' (nGear + 1)
        Me.FishRateGearBasis = Nothing ' (nGear + 1)
        Me.FishRateGearTitle = Nothing ' (nGear + 1)
        Me.FishMGear = Nothing ' (nGear + 1, nGroups)

        Me.FishRateMax = Nothing ' (nGroups)

        Me.FisForced = Nothing ' (nGroups)

        Me.relQ = Nothing ' (nGear, nGroups)

        Me.SSGroup = Nothing ' (nGroups)

        Me.TLSim = Nothing ' (nGroups)

        Me.GroupDetritus = Nothing ' (nGroups)

        Me.Epower = Nothing ' (nGear)
        Me.PcapBase = Nothing ' (nGear)
        Me.CapDepreciate = Nothing ' (nGear)
        Me.CapBaseGrowth = Nothing ' (nGear)

        Me.PropLandedTime = Nothing ' (nGear, nGroups)
        Me.Propdiscardtime = Nothing ' (nGear, nGroups)
        Me.PropDiscardMortTime = Nothing
        Me.Consumption = Nothing ' (nGroups, nGroups)
        Me.Consumpt = Nothing ' (nGroups, nGroups)
        Me.Eatenby = Nothing ' (nGroups)
        Me.Eatenof = Nothing ' (nGroups)
        Me.pred = Nothing ' (nGroups)
        Me.simDCAtT = Nothing ' (nGroups, nGroups)
        Me.DCPct = Nothing ' (nGroups, 3) 'used for B1Round, B2Round, QB, derivt  = nothing ' (BA)
        Me.zscale = Nothing
        Me.PeatArena = Nothing

        Me.lstEnviroInputData.Clear()

    End Sub

    ''' <summary>
    ''' Initialize the forcing shapes to a value of one. This will overwrite  an existing values
    ''' </summary>
    ''' <remarks>In EwE5 this is called RedimZMax(). It gets called before the shapes are populated. </remarks>
    Public Function InitForcingShapes() As Boolean
        Dim i As Integer
        Dim j As Integer

        'I have altered this to just populate the arrays with some default values
        'the dimensioning happens in redimForcingShapes()
        'this separates the dimensioning from setting of default values
        'so that Forcing functions with valid values can be added from an interface and not get over written
        Try

            ZmaxScale = 2

            'this will over write any values already in the shape arrays
            'so after this they must be repopulated
            For i = 0 To NumForcingShapes

                tval(i) = 1      'For forcing functions
                ForcingDBIDs(i) = cCore.NULL_VALUE 'default un-initialized database ID

                For j = 0 To ForcePoints
                    'this will make it so that a forcing function that has not had any values set will have no effect on the model
                    zscale(j, i) = 1   'Default value is half the max
                Next

            Next

            Return True

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".InitForcingShapes() Error: " & ex.Message)
            cLog.Write(Me.ToString & ".InitForcingShapes() Error: " & ex.Message)
            Return False
        End Try

    End Function


    ''' <summary>
    ''' Resize the Forcing Shape Data to the new size this can be bigger or smaller then the existing number of elements
    ''' </summary>
    ''' <param name="newNumberOfShapes">The new size of the arrays</param>
    ''' <param name="newEcoSimIndex">optional Index of the last array element this is used for an AddShape functionality</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ResizeForcingShapes(ByVal newNumberOfShapes As Integer, Optional ByRef newEcoSimIndex As Integer = cCore.NULL_VALUE) As Boolean

        'this is still call by cEIIDataSource which is not used!!!! Hack hack hhhhhhhaaaa
        Debug.Assert(False, "ResizeForcingShapes() no longer implemented!")

        'Try

        '    'set the new number of shapes this was decided by the database and passed in for robustness
        '    'this way the number of shapes is controlled by the datasource
        '    ForcingShapes = newNumberOfShapes
        '    redimForcingShapes()
        '    InitForcingShapes()
        '    newEcoSimIndex = ForcingShapes
        '    Return True

        'Catch ex As Exception
        '    'ToDo_jb  cEcoSimDataStructures.AddShape() Error message
        '    Debug.Assert(False, Me.ToString & ".AddForcingShape() Error: " & ex.Message)
        '    Return False
        'End Try


    End Function

    ''' <summary>
    ''' Resize the Forcing Shape Data to the new size this can be bigger or smaller then the existing number of elements
    ''' </summary>
    ''' <param name="newNumberOfShapes">The new size of the arrays</param>
    ''' <param name="newEcoSimIndex">optional Index of the last array element this is used for an AddShape functionality</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ResizeMediationShapes(ByVal newNumberOfShapes As Integer, Optional ByRef newEcoSimIndex As Integer = cCore.NULL_VALUE) As Boolean

        Try

            'set the new number of shapes this was decided by the database and passed in for robustness
            'this way the number of shapes is controlled by the datasource
            Me.BioMedData.MediationShapes = newNumberOfShapes
            Me.BioMedData.ReDimMediation(Me.nGroups, Me.nGear)
            Return True

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".ResizeMediationShapes() Error: " & ex.Message)
            Return False
        End Try


    End Function



    ''' <summary>
    ''' Dimension all forcing function variables by ForcingPoints (number of forcing points/simulation years) and or ForcingShapes (number of forcing shapes)
    ''' </summary>
    ''' <remarks>Call this any time the number of ForcingPoints(this is the number of simulation years * 12 months) or ForcingShapes changes.
    ''' This gets called to added or remove a forcing function from the EcoSim data. The data will have to be repopulated after this has been run. 
    ''' Core.CoreForcingFunctionUpdater() will update all EcoSim Forcing and Mediation function data with the data held currently in memory by the Shape Managers. 
    ''' </remarks>
    Public Function DimForcingShapes() As Boolean

        Try
            Debug.Assert(NumYears > 0, Me.ToString & ".redimForcingShapes() TotalTime must be set to redim Forcing Shapes.")
            ReDim EnvRespFuncIndex(NumEnvResponseFunctions, nGroups)

            ReDim zscale(ForcePoints, NumForcingShapes)
            ReDim tval(NumForcingShapes)
            ReDim ForcingTitles(NumForcingShapes)

            'variable added for EwE6
            ReDim ForcingShapeType(NumForcingShapes) 'Time or Egg Prod
            ReDim ForcingShapeParams(NumForcingShapes)
            ReDim ForcingDBIDs(NumForcingShapes)

            ReDim isSeasonal(NumForcingShapes)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".redimForcingShapes() Error: " & ex.Message)
            cLog.Write(Me.ToString & ".redimForcingShapes() Error: " & ex.Message)
            Return False
        End Try

        Return True

    End Function

    ''' <summary>
    ''' Resize and set time data to the new number of years set by a user
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub redimTime(ByVal newNumberOfYears As Integer, ByVal nRefDataYears As Integer, ByVal bCopyLastShapePoint As Boolean)
        Dim ipt As Integer, ishape As Integer
        Dim orgNYears As Integer, orgNTimes As Integer

        'get the original number of years and time steps
        orgNYears = NumYears
        orgNTimes = NTimes

        'set number of years to the new value this will also set NTimes (number of time steps)
        Me.NumYears = newNumberOfYears

        'Fishing rate shapes need to be big enough to hold reference data see DoDataValCalculations()
        Dim ntimesteps As Integer = Me.NTimes
        If Me.NumYears < nRefDataYears Then
            ntimesteps = nRefDataYears * Me.NumStepsPerYear
        End If

        'redim preserve the fishing rate and fishmortality data
        redimFishingRates(ntimesteps)

        'set the summary periods to the first and last year
        DefaultSummaryPeriods()

        RedimOutputsByTime(ntimesteps)

        'Only resize the forcing data if the new run length is greater then the existing run length
        'on the database load ForcePoints was set to a min of DEFAULT_N_FORCINGPOINTS (100 years, 1200 points) or the number of years in database * 12 see DimForcingShapes
        'this preserves the originally loaded forcing data if the new number of years is less the data that is already loaded
        If NTimes > ForcePoints Then

            'this means ForcePoints is >=  DEFAULT_N_FORCINGPOINTS and can only grow
            Dim orgPts As Single = ForcePoints
            ForcePoints = NTimes

            'Can't Redim Preserve the first dimension
            'so we need to copy the values back into the new zscale()
            Dim orgZscale(,) As Single
            ReDim orgZscale(orgPts, NumForcingShapes)
            Array.Copy(zscale, orgZscale, orgZscale.Length)

            ReDim zscale(ForcePoints, NumForcingShapes)
            ReDim tval(NumForcingShapes)

            For ishape = 0 To NumForcingShapes
                tval(0) = 1      'For forcing functions
                ZmaxScale = 2
                'copy the values from the original zscale() into the new zscale()
                Dim sLast As Single = 1.0
                For ipt = 0 To orgPts
                    sLast = orgZscale(ipt, ishape)
                    zscale(ipt, ishape) = sLast
                Next
                ' populate extra time (fixes #1427, #1557)
                If isSeasonal(ishape) Then
                    For ipt = orgPts + 1 To ForcePoints
                        zscale(ipt, ishape) = orgZscale(1 + ((ipt - 1) Mod cCore.N_MONTHS), ishape)
                    Next
                Else
                    For ipt = orgPts + 1 To ForcePoints
                        zscale(ipt, ishape) = sLast
                    Next
                End If
            Next

        End If

        'copy the last point from the original data to the end of the new data
        If bCopyLastShapePoint Then
            If NumYears > orgNYears Then

                'for the fishing rate and fish mort data copy the last point into the new points
                For igrp As Integer = 1 To nGroups
                    For ipt = orgNTimes + 1 To NTimes
                        FishRateNo(igrp, ipt) = FishRateNo(igrp, orgNTimes)
                    Next
                Next

                For iflt As Integer = 1 To nGear
                    For ipt = orgNTimes + 1 To NTimes
                        FishRateGear(iflt, ipt) = FishRateGear(iflt, orgNTimes)
                    Next
                Next

            End If
        End If

    End Sub


    '''' <summary>
    '''' Redimension all variables by NMedPoints and/or MediationShapes
    '''' </summary>
    '''' <remarks>Call this any time the number of MediationShapes has changed. This will clear out any data that was in memory.
    ''''  Core.CoreForcingFunctionUpdater() will update all EcoSim Forcing and Mediation function data with the data held currently in memory by the Shape Managers.</remarks>
    'Public Sub ReDimMediation()
    '    Dim i, j As Integer
    '    'following is for Mediation:
    '    NMedPoints = 1200
    '    ' JS18apr09: spawning 9 dummy mediation shapes without any valid database IDS screws up the database
    '    '            I tested Ecosim without mediation shapes and both core and GUI behave well
    '    'If MediationShapes <= 0 Then MediationShapes = 9
    '    ReDim Medpoints(NMedPoints, MediationShapes)
    '    ReDim MedWeights(nGroups + nGear, MediationShapes)
    '    ReDim NMedXused(MediationShapes)
    '    ReDim IMedUsed(nGroups + nGear, MediationShapes)
    '    ReDim MedXbase(MediationShapes)
    '    ReDim MedYbase(MediationShapes)
    '    ReDim MedIsUsed(MediationShapes)
    '    ReDim MedVal(MediationShapes)
    '    ReDim IMedBase(MediationShapes)

    '    'jb added
    '    ReDim MediationTitles(MediationShapes)
    '    ReDim MediationShapeParams(MediationShapes)
    '    ReDim MediationDBIDs(MediationShapes)

    '    'jb this is now handled by MedShapeParams() above
    '    'If ForcingShapes > MediationShapes Then
    '    '    ReDim Preserve Shapes(5, ForcingShapes)
    '    'Else
    '    '    ReDim Preserve Shapes(5, MediationShapes)
    '    'End If

    '    'ToDo: Sort out XBaseLine()what is it used for
    '    'ReDim XBaseLine(MediationShapes)
    '    For i = 0 To MediationShapes
    '        IMedBase(i) = NMedPoints \ 3
    '        For j = 0 To NMedPoints
    '            Medpoints(j, i) = 0.5
    '        Next
    '    Next

    'End Sub




    ''' <summary>
    ''' Hardwire some default values
    ''' </summary>
    ''' <remarks>
    ''' In the original code this was called "EcoSimFileOpen()"
    ''' </remarks>
    Friend Sub SetDefaultParameters()

        'jb
        'in the original code SetupParametersDefault1() was called before reading the ini file default values
        'that doesn't make sense to me as SetupParametersDefault1() uses values that are set from the ini read 
        'so I have switched it to SetupParametersDefault1 after the defaults are read (ini)
        'SetupParametersDefault1()


        'read ini file stored defaults here 
        'at this time there is no mechanisim for storing defaults 
        'so I have just hardwired the same values as are in the default ini file 
        'see original code "SetupParametersRead()"

        'the commented out variables where in the original code but are not declared at this time
        VulMultAll = 0.3
        'StepsPerYear = 12
        TimeJuv(0) = 1
        mintimejuv(0) = 1
        maxtimejuv(0) = 1.0001
        RecPower(0) = 1
        FtimeMax(0) = 2
        FtimeAdjust(0) = 0.5
        MoPred(0) = 0
        'Next other parameters
        Discount = 5
        NumYears = 20
        StepSize = 100
        SystemRecovery = 1
        SorWt = 0.5
        EquilibriumStepSize = 0.003
        StepsPerMonth = 1
        'StepsPerMonth = 30
        'MsgBox("Warning daily time step.")

        'Hack warning temp hard wire of summary time periods
        SumStart(0) = 0
        SumStart(1) = NumYears - 1
        NumStep = NumStepsPerYear

        'DoIntegrate=1 in the ini file 
        Integrate = True

        VulMultAll = 2

        Dim i As Integer

        For i = 1 To nGroups     'prey
            QmQo(i) = 1
            CmCo(i) = 1000
            SwitchPower(i) = 0
            PBmaxs(i) = 2
            'jb price(nGroups) is not used anywhere
            ' price(i) = 1

            NoIntegrate(i) = i
            FtimeMax(i) = FtimeMax(0)
            FtimeAdjust(i) = FtimeAdjust(0)
            MoPred(i) = MoPred(0)
            RiskTime(i) = 0

        Next

        For iflt As Integer = 1 To Me.nGear
            EffortConversionFactor(iflt) = 1
        Next

        'Next from CJW's TemporaryRead
        If FtimeMax(0) <= 0 Then FtimeMax(0) = 2
        If FtimeAdjust(0) < 0 Then FtimeAdjust(0) = 0.5
        If MoPred(0) <= 0 Then MoPred(0) = 1


    End Sub

    Public Sub setRelQToT(iTimestep As Integer, Optional bUseNullValues As Integer = True)
        ' Debug.Assert(iTimestep <> 25)
        For iflt As Integer = 1 To nGear
            For igrp As Integer = 1 To nGroups
                If bUseNullValues Then
                    Me.relQ(iflt, igrp) = relQt(iflt, igrp, iTimestep)
                Else
                    If relQt(iflt, igrp, iTimestep) <> cCore.NULL_VALUE Then
                        Me.relQ(iflt, igrp) = relQt(iflt, igrp, iTimestep)
                    Else
                        Me.relQ(iflt, igrp) = 0
                    End If

                End If
            Next
        Next

    End Sub


    ''' <summary>
    ''' Set the summary time periods to using the Ecoism run length (NTime)
    ''' </summary>
    Public Sub DefaultSummaryPeriods()
        Try
            Debug.Assert(NumYears <> 0 And NumStep <> 0 And NumStepsPerYear <> 0, "DefaultSummaryPeriods() could not be set!")
            SumStart(0) = 0
            SumStart(1) = NumYears - NumStep / NumStepsPerYear
        Catch ex As Exception
            cLog.Write(ex)
            'the model can still run if the summary time periods are messed up
        End Try

    End Sub


    Public Sub RedimTime()
        'Dim MaxTime As Integer

        Debug.Assert(NumYears <> 0, Me.ToString & ".RedimTotalTimeVariables() TotalTime = 0 Something is very wrong......")
        ReDim FishRateNo(nGroups, NTimes)  'was 1200
        ReDim FishRateGear(nGear + 1, NTimes)  'was 1200

        RedimOutputsByTime(NTimes)

        'reset some default values before the data is populated by an interface or a datasource
        'this worked differently in EwE5
        DefaultFishMortalityRates()
        DefaultFishingRates()

        ' DefaultCatchabilities()

    End Sub


    ''' <summary>
    ''' Redim preserve the Fishing Rate and Fish Mort arrays to the number of time steps the model will run for.
    ''' </summary>
    Private Sub redimFishingRates(ByVal nTimeSteps As Integer)

        'NTimes is the number of time step for the current number of years
        ReDim Preserve FishRateNo(nGroups, nTimeSteps)  'was 1200
        ReDim Preserve FishRateGear(nGear + 1, nTimeSteps)  'was 1200

    End Sub

    ''' <summary>
    ''' Dimension the results over time arrays i.e. ResultsOverTime(),ResultsSumCatchByGroupGear()
    ''' </summary>
    ''' <remarks>This only gets called if/when the model is actually run <see>cEcoSimModel.RunModelValue</see>
    ''' This reduces the memory needs of ecosim so that it can be initialized but not run. 
    ''' Ecosim is initialized but not run when Ecospace is loaded.
    ''' This would also allow for a flag to turn of the saving of results over time.
    ''' </remarks>
    Public Sub dimResults(ByVal NumberOfYears As Integer)

        'reset the number of time steps in the summary data
        nSumTimeSteps = 0

        Dim nt As Integer = NumberOfYears * NumStepsPerYear

        'jb 15-Nov-2010 force garbage collection on large blocks of memory
        Erase ResultsOverTime
        Erase PredPreyResultsOverTime
        Erase ResultsAvgByPreyPred
        Erase ResultsSumCatchByGroupGear
        Erase ResultsSumFMortByGroupGear
        Erase ResultsSumValueByGroupGear
        Erase ResultsTimeLandingsGroupGear
        Erase ResultsEffort
        Erase Elect

        Erase ResultsSumRelValueByGroup
        Erase ResultsTimeDiscardsGroupGear
        Erase ResultsTimeDiscardsMortGroupGear
        Erase ResultsTimeDiscardsSurvivedGroupGear

        GC.Collect()

        ReDim ResultsOverTime([Enum].GetValues(GetType(eEcosimResults)).Length - 1, nGroups, nt)
        ReDim PredPreyResultsOverTime(2, nGroups, nGroups, nt)
        ReDim ResultsAvgByPreyPred(1, nGroups, nGroups)


        'fisheries data
        ReDim ResultsSumCatchByGroupGear(nGroups, nGear, nt) ' groups,fleets,time
        ReDim ResultsSumFMortByGroupGear(nGroups, nGear, nt)

        ReDim ResultsTimeLandingsGroupGear(nGroups, nGear, nt)
        ReDim ResultsTimeDiscardsGroupGear(nGroups, nGear, nt)
        ReDim ResultsTimeDiscardsMortGroupGear(nGroups, nGear, nt)
        ReDim ResultsTimeDiscardsSurvivedGroupGear(nGroups, nGear, nt)

        ReDim ResultsSumCatchByGear(nGear, nt)
        ReDim ResultsSumValueByGroupGear(nGroups, nGear, nt)
        ReDim ResultsSumValueByGear(nGear, nt)
        ReDim ResultsEffort(nGear, nt)
        ReDim Elect(nGroups, nGroups, nt)

        ReDim ProfitByFleet(Me.nGear)
        ReDim EmploymentValueByFleet(Me.nGear)

        ReDim ResultsLandings(Me.nGroups, Me.nGear)
        ReDim ResultsSumRelValueByGroup(nGroups, nt)

        ReDim ResultsDiscardsMort(Me.nGroups, Me.nGear)
        ReDim ResultsDiscardsSurvived(Me.nGroups, Me.nGear)

    End Sub


    ''' <summary>
    ''' Erase all the results arrays 
    ''' </summary>
    Public Sub eraseResults()

        Erase ResultsOverTime
        Erase PredPreyResultsOverTime
        Erase ResultsAvgByPreyPred

        'fisheries data
        Erase ResultsSumCatchByGroupGear ' groups,fleets,time
        Erase ResultsSumCatchByGear
        Erase ResultsSumValueByGroupGear
        Erase ResultsSumValueByGear
        Erase ResultsEffort
        Erase Elect
        Erase ResultsSumFMortByGroupGear

    End Sub


    ''' <summary>
    ''' Number of time steps to run the model for
    ''' </summary>
    ''' <remarks>[number of years]*[number of time steps per year]</remarks>
    Public ReadOnly Property NTimes() As Integer
        Get
            Return Me.NumYears * Me.NumStepsPerYear
        End Get
    End Property


    ''' <summary>
    ''' Set default fish rate values
    ''' </summary>
    ''' <remarks>
    ''' <para>This gets called after the data has been dimensioned and before it is populated by the database</para>
    ''' <para>The interface may call this as well to reset fish rate values.</para>
    ''' </remarks>
    Public Sub DefaultFishingRates()

        Dim i As Integer
        Dim j As Integer

        For i = 1 To nGear + 1
            FishRateGearBasis(i) = 1
            FishRateGear(i, 0) = 1

            For j = 0 To NTimes
                FishRateGear(i, j) = 1
            Next
        Next

    End Sub

    'Public Sub DefaultCatchabilities()
    '    Dim i As Integer
    '    Dim j As Integer
    '    'set relative catchabilities by gear type, treating effort for each gear as starting at base
    '    'value of 1.0 so that F for the gear (F=qE=C/B) is 1.0xq where q is relative catchability
    '    'this avoids measuring effort in some unnecessary data units

    '    For i = 1 To nGear
    '        For j = 1 To nGroups
    '            'total catch rate 
    '            'Includes discards that survive
    '            'relQ(i, j) = (m_EPData.Landing(i, j) + m_EPData.Discard(i, j)) / m_Data.StartBiomass(j)

    '            For it As Integer = 1 To NTimes
    '                relQt(i, j, it) = (PropLandedTime(i, j) + Propdiscardtime(i, j)) / StartBiomass(j)
    '            Next

    '        Next
    '    Next

    'End Sub




    ''' <summary>
    ''' Set effort to default value for all the fleets in list
    ''' </summary>
    ''' <param name="lstFleetsIndexesToSet">List for fleets to set to default</param>
    ''' <remarks>Call when an effort timeseries has been unloaded to reset effort to default values</remarks>
    Sub setEffortToDefault(ByVal lstFleetsIndexesToSet As List(Of Integer))
        Try
            'reset effort to 1 for all fleets that where disabled
            For Each flt As Integer In lstFleetsIndexesToSet
                For it As Integer = 1 To Me.NTimes
                    Me.FishRateGear(flt, it) = 1
                Next
            Next
        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".setEffortToDefault() Exception: " & ex.Message)
            cLog.Write(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Set default fish mortality values
    ''' </summary>
    ''' <remarks>
    ''' <para>This gets called after the data has been dimensioned and before it is populated by the database</para>
    ''' <para>The interface may call this as well to reset fish rate values.</para>
    ''' </remarks>
    Public Sub DefaultFishMortalityRates()

        Dim i As Integer
        Dim j As Integer

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'ToDo JB Forced F'
        'Check if this is correct once we changed to F forced for partial run 
        'and 
        'relQ() has a time component
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        For i = 1 To nGroups
            For j = 1 To NTimes
                'set FishRateNo(group,time) to CatchRate
                'for the full run time
                FishRateNo(i, j) = Fish1(i)
            Next
        Next

    End Sub

    ''' <summary>
    ''' Deep-copy Ecosim data structures to another instance.
    ''' </summary>
    ''' <param name="d">The instance to copy to.</param>
    Public Sub CopyTo(ByRef d As cEcosimDatastructures)
        Try

            d.GroupDBID = GroupDBID.Clone
            d.nGroups = nGroups
            d.nGear = nGear

            'now we can redim
            d.RedimVars()

            d.FirstTime = FirstTime
            '    d.ConSimOn = ConSimOn
            d.TrophicOff = TrophicOff
            d.IndicesOn = IndicesOn
            d.UseVarPQ = UseVarPQ
            d.NudgeChecked = NudgeChecked
            d.Integrate = Integrate
            d.AbortRun = AbortRun
            d.EvolveIsOn = EvolveIsOn
            d.BiomassOn = BiomassOn
            d.ActivePair = ActivePair
            'd.CperFlag = CperFlag
            d.NumYears = NumYears
            d.NumStepsPerYear = NumStepsPerYear
            d.StepSize = StepSize
            d.SorWt = SorWt
            d.Discount = Discount
            d.EquilibriumStepSize = EquilibriumStepSize
            d.EquilScaleMax = EquilScaleMax
            d.NutBaseFreeProp = NutBaseFreeProp
            d.NutMin = NutMin
            d.NutBiom = NutBiom
            d.NutTot = NutTot
            d.NutFree = NutFree
            d.NutFreeBase = NutFreeBase.Clone
            d.VulMultAll = VulMultAll
            d.VulMult = VulMult.Clone
            d.vulrate = vulrate.Clone
            d.maxflow = maxflow.Clone
            d.FlowType = FlowType.Clone
            'd.PoolForceCatch = PoolForceCatch.Clone
            d.Eatenof = Eatenof.Clone
            d.Eatenby = Eatenby.Clone
            d.NutForceNumber = NutForceNumber
            d.NutPBmax = NutPBmax
            d.SystemRecovery = SystemRecovery
            d.FtimeMax = FtimeMax.Clone
            d.FtimeAdjust = FtimeAdjust.Clone
            d.MoPred = MoPred.Clone
            d.mo = mo.Clone
            d.RiskTime = RiskTime.Clone
            d.QmQo = QmQo.Clone
            d.CmCo = CmCo.Clone
            d.SwitchPower = SwitchPower.Clone
            d.BaseTimeSwitch = BaseTimeSwitch.Clone
            d.PBmaxs = PBmaxs.Clone
            d.RecPower = RecPower.Clone
            d.Emig = Emig.Clone
            d.Consumption = Consumption.Clone
            d.Htime = Htime.Clone
            d.SimDC = SimDC.Clone
            d.iadult = iadult.Clone
            d.ijuv = ijuv.Clone
            d.TimeJuv = TimeJuv.Clone
            d.maxtimejuv = maxtimejuv.Clone
            d.mintimejuv = mintimejuv.Clone
            d.NoIntegrate = NoIntegrate.Clone
            d.Fish1 = Fish1.Clone
            d.FishTime = FishTime.Clone
            d.FishRateMax = FishRateMax.Clone
            d.FishMGear = FishMGear.Clone
            d.FishRateNo = FishRateNo.Clone
            d.FishRateNoDBID = FishRateNoDBID.Clone
            d.FishRateNoTitle = FishRateNoTitle.Clone
            d.GroupFishRateNoDBID = GroupFishRateNoDBID.Clone
            d.FishRateGear = FishRateGear.Clone
            d.FishRateGearBasis = FishRateGearBasis.Clone
            d.FishRateGearDBID = FishRateGearDBID.Clone
            d.FishRateGearTitle = FishRateGearTitle.Clone
            d.Ftime = Ftime.Clone
            d.Hden = Hden.Clone
            d.QBoutside = QBoutside.Clone
            d.DetritusOut = DetritusOut.Clone
            d.AssimEff = AssimEff.Clone
            d.SimGE = SimGE.Clone
            d.StartBiomass = StartBiomass.Clone
            d.pbbiomass = pbbiomass.Clone
            d.loss = loss.Clone
            d.Cbase = Cbase.Clone
            d.relQ = relQ.Clone

            d.ForcingShapeType = ForcingShapeType.Clone
            'ShapeParameters = ShapeParameters.clone

            '    d.MediationShapeParams = MediationShapeParams.Clone
            d.ForcingShapeParams = ForcingShapeParams.Clone
            '   d.MediationTitles = MediationTitles.Clone
            '   d.MediationDBIDs = MediationDBIDs.Clone
            d.ForcingDBIDs = ForcingDBIDs.Clone
            d.isSeasonal = isSeasonal.Clone

            ''Mediation vars 
            'd.MediationShapes = MediationShapes
            'd.NMedPoints = NMedPoints
            'd.Medpoints = Medpoints.Clone
            'd.MedWeights = MedWeights.Clone
            'd.NMedXused = NMedXused.Clone
            'd.IMedUsed = IMedUsed.Clone
            'd.MedXbase = MedXbase.Clone
            'd.MedYbase = MedYbase.Clone
            'd.MedIsUsed = MedIsUsed.Clone  '
            'd.MedVal = MedVal.Clone
            'd.IMedBase = IMedBase.Clone
            d.inlinks = inlinks
            d.ilink = ilink.Clone
            d.jlink = jlink.Clone

            'Forcing
            d.NumForcingShapes = NumForcingShapes
            d.ForcingTitles = ForcingTitles.Clone
            d.ForcePoints = ForcePoints
            d.ZmaxScale = ZmaxScale
            d.zscale = zscale.Clone
            d.tval = tval.Clone
            d.EggProdShape = EggProdShape.Clone
            d.pbm = pbm.Clone
            d.pred = pred.Clone
            d.Qmain = Qmain.Clone
            d.Qrisk = Qrisk.Clone
            d.Consumpt = Consumpt.Clone
            'd.DCPct = DCPct.Clone
            d.ResultsOverTime = ResultsOverTime.Clone
            d.PredPreyResultsOverTime = PredPreyResultsOverTime.Clone
            d.ResultsAvgByPreyPred = ResultsAvgByPreyPred.Clone
            d.NumStep = NumStep
            d.NumStep0 = NumStep0
            d.NumStep1 = NumStep1
            d.SumStart = SumStart.Clone
            d.Narena = Narena
            d.Iarena = Iarena.Clone
            d.Jarena = Jarena.Clone
            d.ArenaNo = ArenaNo.Clone
            d.VulArena = VulArena.Clone
            d.Alink = Alink.Clone
            d.IlinkSet = IlinkSet.Clone
            d.JlinkSet = JlinkSet.Clone
            d.KlinkSet = KlinkSet.Clone
            d.PeatArena = PeatArena.Clone
            d.ArenaLink = ArenaLink.Clone
            d.Qlink = Qlink.Clone
            d.NlinksSet = NlinksSet
            d.PeatArenaSetFromDataBase = PeatArenaSetFromDataBase
            d.BoutFeeding = BoutFeeding
            d.RelaSwitch = RelaSwitch.Clone
            d.ToDetritus = ToDetritus.Clone
            d.FisForced = FisForced.Clone
            d.SS = SS

            d.Epower = Epower
            d.PcapBase = PcapBase
            d.CapDepreciate = CapDepreciate
            d.CapBaseGrowth = CapBaseGrowth

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Get the time index for the Summary Start and End Time
    ''' </summary>
    ''' <param name="iSummary">0 = Start Summary Time Period. 1 = End Summary Time Period</param>
    ''' <returns>iTime Index</returns>
    ''' <remarks></remarks>
    Private Function summaryTimeIndex(ByVal iSummary As Integer) As Integer
        Dim itime As Integer
        itime = CInt(SumStart(iSummary) * NumStepsPerYear) + 1
        If itime > NumYears * NumStepsPerYear Then itime = NumYears * NumStepsPerYear - NumStep
        Return itime
    End Function

    Public Function getSummaryBioForGroup(ByVal iGroup As Integer, ByRef startBio As Single, ByRef endBio As Single) As Single
        Dim bsum As Single, nbsum As Integer, stime As Integer, itime As Integer
        Dim bio(1) As Single

        Try

            For isum As Integer = 0 To 1
                bsum = 0
                nbsum = 0

                stime = summaryTimeIndex(isum)

                For itime = stime To stime + NumStep - 1
                    bsum = bsum + ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, iGroup, itime)
                    nbsum += 1
                Next itime

                bio(isum) = bsum / nbsum

            Next isum

            startBio = bio(0)
            endBio = bio(1)

            Return True

        Catch ex As Exception
            cLog.Write(ex)

            'if there is an error startBio and endBio will be zero
            startBio = 0
            endBio = 0
            Return False
        End Try


    End Function

    Public Function getSummaryValueByGroup(ByVal iGroup As Integer, ByVal iFleet As Integer, ByRef startCatch As Single, ByRef endCatch As Single) As Boolean
        Return Me.getSummarybyGroupFleet(Me.ResultsSumValueByGroupGear, iGroup, iFleet, startCatch, endCatch)
    End Function


    Public Function getSummaryCostByCatch(ByVal iFleet As Integer, ByRef startCost As Single, ByRef endcost As Single) As Boolean
        Me.getSummaryByFleet(Me.ResultsEffort, iFleet, startCost, endcost)
    End Function


    Public Function getSummaryCatchByGroup(ByVal iGroup As Integer, ByVal iFleet As Integer, ByRef startCatch As Single, ByRef endCatch As Single) As Boolean
        Return Me.getSummarybyGroupFleet(ResultsSumCatchByGroupGear, iGroup, iFleet, startCatch, endCatch)
    End Function

    Public Function getSummaryBioOfCatch(ByVal iFleet As Integer, ByRef startCatch As Single, ByRef endCatch As Single) As Boolean
        Return Me.getSummaryByFleet(ResultsSumCatchByGear, iFleet, startCatch, endCatch)
    End Function

    Public Function getSummaryValueOfCatch(ByVal iFleet As Integer, ByRef startCatch As Single, ByRef endCatch As Single) As Boolean
        Return Me.getSummaryByFleet(ResultsSumValueByGear, iFleet, startCatch, endCatch)
    End Function

    Private Function getSummarybyGroupFleet(ByRef values(,,) As Single, ByVal iGroup As Integer, ByVal iFleet As Integer, ByRef startVal As Single, ByRef endVal As Single) As Boolean

        Dim bsum As Single, nbsum As Integer, stime As Integer, itime As Integer
        Dim sumValues(1) As Single

        Try
            For isum As Integer = 0 To 1
                bsum = 0
                nbsum = 0

                stime = summaryTimeIndex(isum)

                For itime = stime To stime + NumStep - 1
                    bsum = bsum + values(iGroup, iFleet, itime)
                    nbsum += 1
                Next itime

                sumValues(isum) = bsum / nbsum

            Next isum

            startVal = sumValues(0)
            endVal = sumValues(1)

            Return True

        Catch ex As Exception
            cLog.Write(ex)

            'if there is an error startBio and endBio will be zero
            startVal = 0
            endVal = 0
            Return False
        End Try

    End Function


    Private Function getSummaryByFleet(ByRef values(,) As Single, ByVal iFleet As Integer, ByRef startVal As Single, ByRef endVal As Single) As Boolean
        Dim bsum As Single, nbsum As Integer, stime As Integer, itime As Integer
        Dim sumValues(1) As Single

        Try
            For isum As Integer = 0 To 1
                bsum = 0
                nbsum = 0

                stime = summaryTimeIndex(isum)

                For itime = stime To stime + NumStep - 1
                    bsum = bsum + values(iFleet, itime)
                    nbsum += 1
                Next itime

                sumValues(isum) = bsum / NumStep

            Next isum

            startVal = sumValues(0)
            endVal = sumValues(1)

            Return True

        Catch ex As Exception
            cLog.Write(ex)

            'if there is an error startVal and endVal will be zero
            startVal = 0
            endVal = 0
            Return False
        End Try

    End Function


    ''' <summary>
    ''' Computed summarized results for Ecosim
    ''' </summary>
    ''' <param name="EcopathCost">Ecopath precentage of Cost CostPct(3,nfleets)</param>
    ''' <param name="JobMultiplier">Jobs multiplier from the Search data</param>
    ''' <remarks>Computes ProfitByFleet(nFleets), JobsByFleet(nfleets), Prey Pred consumption</remarks>
    Public Sub SummarizeResults(ByVal EcopathCost(,) As Single, ByVal JobMultiplier() As Single)

        For iPrey As Integer = 1 To Me.nGroups
            For iPred As Integer = 1 To Me.nGroups
                Me.ResultsAvgByPreyPred(0, iPrey, iPred) = Me.ResultsAvgByPreyPred(0, iPrey, iPred) / Me.nSumTimeSteps
                Me.ResultsAvgByPreyPred(1, iPrey, iPred) = Me.ResultsAvgByPreyPred(1, iPrey, iPred) / Me.nSumTimeSteps
            Next
        Next

        ReDim ProfitByFleet(Me.nGear)
        ReDim EmploymentValueByFleet(Me.nGear)

        Dim sumValue As Single, sumEffort As Single
        'number of years the data was summarized over
        Dim nYears As Single = Me.nSumTimeSteps / 12
        For iflt As Integer = 0 To Me.nGear
            sumValue = 0
            For it As Integer = 1 To Me.nSumTimeSteps
                sumValue += Me.ResultsSumValueByGear(iflt, it)
            Next

            sumEffort = 0
            For it As Integer = 1 To Me.nSumTimeSteps
                sumEffort += Me.ResultsEffort(iflt, it)
            Next

            'average profit
            '[sum of value] * [ecopath profit (percentage of catch value that is profit /per unit of effort)]
            ProfitByFleet(iflt) = sumValue * (EcopathCost(iflt, eCostIndex.Profit) / 100) * sumEffort / nYears

            'TEMP just for something to work with until we have ECost up and running
            '[value of catch] * [Jobs(fleet) from the search forms]
            EmploymentValueByFleet(iflt) = sumValue * JobMultiplier(iflt) / nYears 'Jobs(Fleet) percentage of value that goes to Jobs default=1

        Next iflt

    End Sub

    Public Sub ClearSummaryResults()

        NumStep0 = 0     'Actual number of steps for the zero element of the summary arrays Start summary time period
        NumStep1 = 0  'Actual number of steps for the one element of the summary arrays end summary time peroid
        'storage for summary time period data
        ReDim SumBiomass(2, nGroups) 'SumBiomass(iTimePeriod,iGroup)
        'catch by group
        ReDim SumCatch(2, nGroups)

    End Sub

    ''' <summary>
    ''' An Ecosim run has completed
    ''' </summary>
    ''' <remarks>Sets StepsPerMonth and bMultiThreaded to default values</remarks>
    Public Sub onEcosimRunCompleted()
        Me.StepsPerMonth = 1
        Me.bMultiThreaded = False
    End Sub

End Class


