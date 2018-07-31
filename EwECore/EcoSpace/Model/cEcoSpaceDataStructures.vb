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
Imports EwEUtils.Core
Imports EwEUtils.Extensions
Imports System.IO

#Region "Public Class definitions"

Public Class cRowCol
    Public Property Row As Integer
    Public Property Col As Integer
    Public Sub New(ByVal theRow As Integer, ByVal theCol As Integer)
        Me.Row = theRow
        Me.Col = theCol
    End Sub
    Public Overrides Function ToString() As String
        Return "Row: " & Me.Row & ", col: " & Me.Col
    End Function
End Class

#End Region

Public Class cEcospaceDataStructures

#Region "Public Fields"

    ''' <summary>ESRI projection string of the default WGS_84 projection of Ecospace.</summary>
    Public Const DEFAULT_COORDINATESYSTEM As String = "GEOGCS[""WGS 84"", DATUM[""WGS_1984"", SPHEROID[""WGS 84"",6378137,298.257223563, AUTHORITY[""EPSG"",""7030""]], AUTHORITY[""EPSG"",""6326""]], PRIMEM[""Greenwich"",0, AUTHORITY[""EPSG"",""8901""]], UNIT[""degree"",0.01745329251994328, AUTHORITY[""EPSG"",""9122""]], AUTHORITY[""EPSG"",""4326""]]"

    Public EcosimScenarioDBID As Integer
    ''' <summary>Array of ecospace group database IDs.</summary>
    Public GroupDBID() As Integer
    ''' <summary>Array of mappings to ecopath group database IDs.</summary>
    Public EcopathGroupDBID() As Integer
    ''' <summary>Array of ecospace region database IDs.</summary>
    Public RegionDBID() As Integer
    ''' <summary>Array of ecospace habitat database IDs.</summary>
    Public HabitatDBID() As Integer
    ''' <summary>Array of ecospace MPA database IDs.</summary>
    Public MPADBID() As Integer
    ''' <summary>Array of ecospace Fleet database IDs.</summary>
    Public FleetDBID() As Integer
    ''' <summary>Array of mappings to ecopath fleet database IDs.</summary>
    Public EcopathFleetDBID() As Integer

    'number of years to run the simulation for
    Public Property TotalTime As Single

    ''' <summary>
    ''' Predict fishing effort via the Gravity attraction model
    ''' </summary>
    ''' <remarks>If = True Predict fishing effort based on Fishing Cost Map, Catch Value and Area Fished. If PredictEffort = False then use the Ecopath Effort.</remarks>
    Public PredictEffort As Boolean
    Public AdjustSpace As Boolean
    Public SpaceTime As Boolean
    Public IsFishRateSet As Boolean

    ''' <summary>
    ''' Get/set whether Ecospace will use square cells, e.g. will bypass cell width corrections.
    ''' </summary>
    Public AssumeSquareCells As Boolean = False
    ''' <summary>
    ''' Bad-ass flag, stating whether cell length can be computed from cell size and vice-versa.
    ''' This should really be properly determined from proper projections
    ''' </summary>
    Public LinkCellWidthAndSize As Boolean = True
    ''' <summary>
    ''' WKT projection string for the Ecospace coordinate system
    ''' </summary>
    Friend ProjectionString As String = DEFAULT_COORDINATESYSTEM

    Public CurrentForce As Boolean
    'jb Ecoseed may get move to an object
    'for now this will let the code function
    Public EcoseedOn As Boolean

    ''' <summary>Current model time step in years. Incremented by <see cref="TimeStep">TimeStep</see> at the end of the timestep.</summary>
    ''' <remarks>This is the time in years, not the array index.</remarks>
    Public TimeNow As Double

    ''' <summary>
    ''' Length of the time step in years 
    ''' </summary>
    ''' <remarks>1 month = 0.083333</remarks>
    Public TimeStep As Double = 1 / cCore.N_MONTHS

    ''' <summary>Current year that is being executed.</summary>
    Public YearNow As Integer = 0

    ''' <summary>Current month that is being executed.</summary>
    Public MonthNow As Integer = 0

    'jb ??? this may be temporary
    'setting of default values need to have access to Stanza and Ecosim data
    Public StanzaGroups As cStanzaDatastructures
    Public EcoPathData As cEcopathDataStructures

    ''' <summary>Number of Fishing Fleets </summary>
    ''' <remarks></remarks>
    Public nFleets As Integer

    ''' <summary>Number of Habitat types defined by the user</summary>
    Public NoHabitats As Integer

    Public nLiving As Integer

    ''' <summary>Descriptive text of habitat type (name) </summary>
    Public HabitatText() As String

    ''' <summary>The proportion to which a group prefers a habitat.</summary>
    ''' <remarks>Indexed Group,Habitat</remarks>
    Public PrefHab(,) As Single

    ''' <summary>The proportion of habitat type in a map cell.</summary>
    ''' <remarks>Sparse array (Habitat)(Row,Col)</remarks>
    Public PHabType()(,) As Single

    ''' <summary>The proportion of map cell that is fished by a fleet.</summary>
    ''' <remarks>
    ''' Computed in cEcoSpace.SetEffortParameters()
    ''' Spares Indexed (Gear)(Row,Col)
    ''' </remarks>
    Public PAreaFished()(,) As Single
    ''' <summary>Does this Fishing fleet use this habitat type </summary>
    ''' <remarks>Indexed Fleet,Habitat</remarks>
    Public GearHab(,) As Boolean

    ''' <summary>
    ''' Total number of habitat cells by habitat type
    ''' </summary>
    ''' <remarks>Caluclated in CalcHabitatArea()</remarks>
    Public HabArea() As Single

    ''' <summary>
    ''' Proportion of total area used by a habitat type
    ''' </summary>
    ''' <remarks>HabAreaProportion(iHab) = HabArea(iHab) / TotalHabitatArea </remarks>
    Public HabAreaProportion() As Single

    Public AdvectSpeed As Single
    Public jord(1000) As Integer

    Public MoveScale As Single

    ''' <summary>
    ''' Inverse of emigration response to fitness
    ''' </summary>
    ''' <remarks>In EwE5 there is no variable for this it is read from the interface when it is needed</remarks>
    Public FitnessResp As Single

    ' ''' <summary>Number of habitat time changes</summary>
    'Public NoHabChanges As Integer
    ' ''' <summary>Habitat time for NoHabChange #</summary>
    'Public HabTime() As Single
    ' ''' <summary>Habitat changes for NoHabChange #</summary>
    'Public HabChange(,) As Integer

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'Map Variables

    ''' <summary>Number of rows in the current base map</summary>
    Public InRow As Integer
    ''' <summary>Number of rows in the current base map</summary>
    Public InCol As Integer
    ''' <summary>Length in KM of a cell, used for dispersal etc.</summary>
    Public CellLength As Single
    ''' <summary>Latitude of upper left coordinate of the current basemap.</summary>
    Public Lat1 As Single
    ''' <summary>Longitude of upper left coordinate of the current basemap.</summary>
    Public Lon1 As Single
    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    ''' <summary> Total number of stanza groups </summary>
    ''' <remarks>Sum of nStanza(isplit) for each stanza. Set in RedimMapVars()</remarks>
    Public Nvarsplit As Integer

    ''' <summary>total number of all groups </summary>
    ''' <remarks>Nvarsplit + nGroups. Set in RedimMapVars() Used for dimensioning</remarks>
    Public nvartot As Integer

    ''' <summary>Total number of cells that have water </summary>
    ''' <remarks>computed in ScaleRelativePrimaryProductivityToEcopathLevel()</remarks>
    Public nWaterCells As Integer

    Public Basebiomass() As Single
    Public Bnew() As Single
    Public der() As Single
    Public EatEffBad() As Single
    Public MPABiomass() As Single
    ''' <summary>Movement rate?!</summary>
    Public Mrate() As Single
    ''' <summary>Base dispersal rate as entered by the user</summary>
    Public Mvel() As Single
    Public RelMoveBad() As Single
    Public RelVulBad() As Single
    Public IsAdvected() As Boolean

    ''' <summary> Weighting for directed movement within a migratory area (groups)</summary>
    ''' <remarks></remarks>
    Public InMigAreaMovement() As Single

    ''' <summary>Biomass by cell (row, col, group)</summary>
    Public Bcell(,,) As Single
    ''' <summary>Contaminant by cell (row, col, group)</summary>
    Public Ccell(,,) As Single
    Public Clast(,,) As Single
    Public AMmTr(,,) As Single
    Public Ftr(,,) As Single

    Public Blast(,,) As Single
    ''' <summary>Actual depth map as used by Ecospace, computed from <see cref="DepthInput"/> and <see cref="Excluded"/>.</summary>
    Public Depth(,) As Single
    Public DepthA(,) As Single
    Public DepthX(,) As Integer
    Public DepthY(,) As Single

    ''' <summary>Catch by Row, Col, Group.</summary>
    Public CatchMap(,,) As Single

    ''' <summary>Catch by Row, Col, Fleet.</summary>
    Public CatchFleetMap(,,) As Single

    ' DISCARDLESS: explicitly state what this map contains. All discards? Dead discards?
    ''' <summary>Discards (all? mortality?) by Row, Col, Group.</summary>
    ''' <remarks>This is not exposed by the interface at this time. It was included for the Biodiversity plugin and can only be accessed via code.</remarks>
    Public DiscardsMap(,,) As Single

    ''' <summary>User-entered depth map</summary>
    Public DepthInput(,) As Single
    ''' <summary>Is a cell included in modeling by Row, Col.</summary>
    Public Excluded(,) As Boolean

    ''' <summary>Trophic Level by Row, Col, Group.</summary>
    Public TL(,,) As Single
    ''' <summary>Trophic Level of the catch by Row, Col.</summary>
    Public TLc(,) As Single
    ''' <summary>Kemptons Q by Row, Col.</summary>
    Public KemptonsQ(,) As Single
    ''' <summary>ShannonDiversity by Row, Col.</summary>
    Public ShannonDiversity(,) As Single

    'these are all part of velmaker
    'velmaker may become its own class
    Public Xvel(,) As Single, Yvel(,) As Single
    Public Xvloc(,) As Single, Yvloc(,) As Single
    Public UpVel(,) As Single
    ''' <summary>Wind X velocity (i x j x month)</summary>
    Public Xv(,,) As Single
    ''' <summary>Wind Y velocity (i x j x month)</summary>
    Public Yv(,,) As Single

    Public MonthlyXvel()(,) As Single
    Public MonthlyYvel()(,) As Single
    Public MonthlyUpWell()(,) As Single

    Public flow(,) As Single

    Public Region(,) As Integer
    Public RelPP(,) As Single
    Public RelCin(,) As Single

    ''' <summary>
    ''' MPA layout, dimensioned as mpa x (row, col)
    ''' </summary>
    Public MPA()(,) As Boolean

    ''' <summary>
    ''' Base value for relative PP (relative PP at t=0). Set after PP has been read from the database.
    ''' </summary>
    ''' <remarks>RelPP can be changed by external data this is use to restore RelPP to its original value</remarks>
    Public RelPP0(,) As Single

    ''' <summary>
    ''' Sailing cost (fleet x row x col)
    ''' </summary>
    Public Sail()(,) As Single
    'Public Sail(,,) As Single 'effort to fish a map cell, used as a multiplier with effort, Scaled to Ecopath ScaleSailingToUnity() in InitSpatialEqulibrium()
    'Public Port(,,) As Boolean

    Public Port()(,) As Boolean

    Public EffPower() As Single

    ''' <summary>
    ''' Ecospace base biomass gathered at the end of the first timestep after any spinup period.
    ''' </summary>
    ''' <remarks></remarks>
    Public BBase() As Single
    Public nRegions As Integer

    ''' <summary>Number of Importance layers</summary>
    Public nImportanceLayers As Integer
    Public ImportanceLayerDBID() As Integer
    Public ImportanceLayerName() As String
    Public ImportanceLayerDescription() As String
    Public ImportanceLayerWeight() As Single
    ''' <summary>Importance layer data (layer)(row, col)</summary>
    Public ImportanceLayerMap()(,) As Single

    ''' <summary>Number of environmental layers</summary>
    Public nEnvironmentalDriverLayers As Integer
    ''' <summary>Environmental layer database IDS</summary>
    Public EnvironmentalLayerDBID() As Integer
    ''' <summary>Environmental layer names</summary>
    Public EnvironmentalLayerName() As String
    ''' <summary>Environmental layer descriptions</summary>
    Public EnvironmentalLayerDescription() As String
    ''' <summary>Environmental layer units</summary>
    Public EnvironmentalLayerUnits() As String
    ''' <summary>Environmental layer data (layer)(row, col)</summary>
    Public EnvironmentalLayerMap()(,) As Single

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'Summary data

    ''' <summary>
    ''' Number of time steps for averaging summary window data
    ''' </summary>
    ''' <remarks></remarks>
    Public NumStep As Integer

    ''' <summary>Start time of the first and second summary data period. In Years </summary>
    ''' <remarks> Data is summarized over two time periods set by SumStart(0) and SumStart(1). The number of time steps to summarize over is set in NumStep.
    ''' Defaults are set in redimTimeVaraibles().
    ''' Used in cEcospace.summarySetTimeStep() to set the index to store the summary data in. The first or second summary period.
    ''' </remarks>
    Public SumStart(1) As Single

    Public lstRegions As New List(Of Single(,,))

    ''' <summary>ResultsCatchRegionGearGroup( nRegions, nFleets, nGroups, nTimesteps)</summary>
    Public ResultsCatchRegionGearGroup(,,,) As Single
    ''' <summary>ResultsCatchRegionGearGroup( nRegions, nFleets, nGroups, nYears)</summary>
    Public ResultsCatchRegionGearGroupYear(,,,) As Single
    ''' <summary>ResultsByFleet(nvars,nFleets,NumberOfTimeSteps)</summary>
    Public ResultsByFleet(,,) As Single
    ''' <summary>ResultsByFleetGroup(nvars, nFleets, nGroups, nTimesteps)</summary>
    Public ResultsByFleetGroup(,,,) As Single
    ''' <summary>ResultsRegionGroup(nRegions, nGroups, nTimesteps)</summary>
    Public ResultsRegionGroup(,,) As Single
    ''' <summary>ResultsRegionGroup(nRegions, nGroups, nYears)</summary>
    Public ResultsRegionGroupYear(,,) As Single

    ''' <summary> Summarized time step data </summary>
    ''' <remarks>populated in sumarizeTimeStepData()</remarks>
    Public ResultsByGroup(,,) As Single 'ResultsByGroup(nVars,Ngroups,  NumberOfTimeSteps)

    Public ResultsSummaryByFleet(,) As Single 'vars, fleets

    ''' <summary> Sum of landings across all cells by Group/Fleet for the current timestep </summary>
    Public Landings(,) As Single

    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

    Public PPupWell As Single

    ''' <summary>IsMigratory flag, per group.</summary>
    Public IsMigratory() As Boolean

    ''' <summary>Migration preferred row (group, month).</summary>
    ''' <remarks>In the original version of Ecospace this value was entered as point.
    ''' In the migration update of Ecospace this value will be calculated from a monthly migration map</remarks>
    Friend PrefRow(,) As Integer
    ''' <summary>Migration preferred column (group, month).</summary>
    ''' <remarks>In the original version of Ecospace this value was entered as point.
    ''' In the migration update of Ecospace this value will be calculated from a monthly migration map</remarks>
    Friend Prefcol(,) As Integer

    ''' <summary>North-south migration concentration, per group.</summary>
    ''' <remarks>In the oringial version of Ecospace this value was entered as a fixed value per group.
    ''' In the migration update of Ecospace this value will be calculated from a monthly migration map</remarks>
    Friend MigConcRow() As Single
    ''' <summary>East-west migration concentration, per group.</summary>
    ''' <remarks>In the oringial version of Ecospace this value was entered as a fixed value per group.
    ''' In the migration update of Ecospace this value will be calculated from a monthly migration map</remarks>
    Friend MigConcCol() As Single

    ''' <summary>
    ''' Average value in the <see cref="cEcospaceDataStructures.Sail">Sail(fleet,row,col)</see> map for all water cells.
    ''' </summary>
    ''' <remarks>
    ''' Used only to distribute total effort across the cells. Does not effect the total effort.
    ''' If <see cref="cEcospaceDataStructures.bUseEffortDistThreshold">bUseEffortDistThreshold</see> = True this will only include cells below the fishing effort threshold</remarks>
    Public SailScale() As Single

    Public FitRespType As Integer

    ''' <summary>
    ''' Sailing Effort Multiplier by Fleet
    ''' </summary>
    ''' <remarks></remarks>
    Public SEmult() As Single

    ''' <summary>
    ''' Total fishing mortality by group,row,col
    ''' </summary>
    ''' <remarks>calculated in PredictEffortDistribution() Sum of EffortSpace() * catchability (EcoSim.relQ)</remarks>
    Public Ftot(,,) As Single

    ''' <summary>
    ''' Fishing Mortality (catchrate) by a fleet for each cell fleet,row,col
    ''' </summary>
    ''' <remarks>Computed from Ecosim.FishRateGear(fleet,time) and "gravity attraction" in PredictEffortDistribution()  </remarks>
    Public EffortSpace(,,) As Single

    ''' <summary>Number of MPAs</summary>
    Public MPAno As Integer
    Public MPAname() As String
    ''' <summary>MPA monthly open/closed state (Month x MPA), true when open for fishing.</summary>
    Public MPAmonth(,) As Boolean
    ''' <summary>MPA enforcement (fleet x MPA), true if an MPA is open to fishing for a given fleet</summary>
    Public MPAfishery(,) As Boolean

    ''' <summary>Fleet/Gear cell access (fleet, row, col).</summary>
    ''' <remarks>JS added 12 Sept 12 to determine fleet cell fishing access for the mao once 
    ''' for every time step. This is done to facilitate overlapping MPAs.</remarks>
    Friend IsFished(,,) As Boolean

    ''' <summary>
    ''' SOR weight 
    ''' </summary>
    ''' <remarks></remarks>
    Public W As Single

    ''' <summary>
    ''' Iteration tolerance for solvegrid.
    ''' </summary>
    ''' <remarks>
    ''' High values will be less accurate, but with less computing time. Reasonable values: 0.1-0.000001.
    ''' </remarks>
    Public Tol As Single

    ''' <summary>
    ''' Maximum number of iterations that solvegrid will use to find the implicit solution for the next timestep
    ''' </summary>
    ''' <remarks>
    ''' Lower numbers will be faster but less accurate. needs to be set in reasonable accord to Tol. Reasonable values: 10-100
    ''' </remarks>
    Public maxIter As Integer

    ''' <summary>
    ''' Number of threads to use for the grid solvers
    ''' </summary>
    Public nGridSolverThreads As Integer

    ''' <summary>
    ''' Number of threads to run the groups biomass calculations on 
    ''' </summary>
    Public nSpaceSolverThreads As Integer

    ''' <summary>
    ''' Number of effort distribution threads
    ''' </summary>
    Public nEffortDistThreads As Integer

    'number of species per thread for the IBM stuff
    Public nIBMGroupsPerThread As Integer

    Public nIBMPacketsPerThread As Integer

    Public SpDat As Integer
    Public SpDatYear As Integer

    Public SpName() As String
    Public SpPool() As Integer
    Public SpType() As Integer
    Public SpWt() As Single
    Public SpVal(,) As Single
    Public SpYear() As Integer
    Public SpForceBB(,) As Single
    Public SpForceCatch(,) As Single
    Public SpForceZ(,) As Single
    Public IsSpShown() As Boolean
    Public SpRegion() As Integer

    'for reference data
    Public SpaceBiomassByRegion(,,) As Single
    Public SpaceBiomassByRegionCount(,,) As Single
    Public SpaceCatchByRegion(,,) As Single
    Public SpaceCatchByRegionCount(,,) As Single
    Public SpaceEffortByRegionFleet(,,) As Single
    Public SpaceEffortByRegionFleetCount(,,) As Single

    '***************** new multistanza variables
    'Dim TotLoss() As Single, TotEatenBy() As Single, TotBiom() As Single, TotPred() As Single, IFDweight() As Single, TotIFDweight() As Single, PredCell() As Single, Blocal() As Single
    Public PredCell(,,) As Single
    Public IFDweight(,,) As Single
    Public NewMultiStanza As Boolean, IFDPower As Single
    Public ByPassIntegrate() As Boolean

    Public UseIBM As Boolean
    Public UseExact As Boolean

    'these are used to split up the species properly for threading 
    'according to # of species that are actually being integrated
    'contains the indices of ByPassIntegrate() that are FALSE
    Public integratedGroups() As Integer
    Public totalIntegratedGroups As Integer

    'these are the bounds of the water squares for each column
    'solvegrid will go from istartrow(j) to iendrow(j)
    Public iStartRow() As Integer
    Public iEndRow() As Integer
    Public jStartCol() As Integer
    Public jEndCol() As Integer


    'total number of water cells on the map
    'used by spaceSolver to split up the cells to each thread according to # of water cells
    Public iTotalWaterCells As Integer
    'for each water cell, these give the i and j coordinate of that cell
    'used by solvecell to find out which i,j to use for their current water cell
    Public iWaterCellIndex() As Integer
    Public jWaterCellIndex() As Integer

    ''' <summary>
    ''' Sum of Squares fit to reference data
    ''' </summary>
    Public SS As Single

    Public Aspace() As Single 'this is a modified Alink (from ecosim)
    Public Vspace() As Single 'this is a modified VulArena (from ecosim)

    ''' <summary>
    ''' <para>This determines how much weight is put into the pathfinding movement algorithm for migratory species.
    ''' If fish are getting caught in complex habitat, increasing this value will help the fish get "un-stuck".</para>
    ''' <para>Possible values [0-1]</para>
    ''' <para>Increasing this will increase the concentration of the fish, so the regular NS/EW concentrations should
    ''' be lowered to keep the concentration the same.</para>
    ''' </summary>
    Public barrierAvoidanceWeight() As Single

    ''' <summary>Predation rate by Row, Col, Prey/Pred link</summary>
    ''' <remarks>Added for Model coupling</remarks>
    Public MPred(,,) As Single

    ''' <summary>Detritus by Row, Col, group</summary>
    ''' <remarks>Added for Model coupling</remarks>
    Public GroupDetritus(,,) As Single

    ''' <summary>
    ''' Habitat Capacity by Row,Col,Group
    ''' </summary>
    ''' <remarks>Habitat capacity is the normalized Capacity of all inputs (maps and response functions)  <see cref="cEcoSpace.SetHabCap">Ecospace.SetHabCap</see> </remarks>
    Public HabCap()(,) As Single
    'Public HabCap(,,) As Single
    ''' <summary>
    ''' User defined input habitat capacity.
    ''' </summary>
    Public HabCapInput()(,) As Single
    'Public HabCapInput(,,) As Single

    ''' <summary> Sum of Capacity across the map cells by group </summary>
    Public TotHabCap() As Single

    '''<summary>max capacity by group</summary>
    ''' <remarks>Used to check that the user has set capacities for all groups </remarks>
    Public MaxHabCap() As Single

    Public Width() As Single

    Public SaveAnnual As Boolean = True

    ''' <summary>
    ''' Use the Ecospace Output directory defined by the core. If True this path will include Model-name/Ecopath_6. Scenario-name/
    ''' If False just the core output directory.
    ''' </summary>
    ''' <remarks>
    ''' This allows you to set the Ecospace output directory from code. 
    ''' You could loop over a bunch of different cases and set the output dir for each case.
    ''' </remarks>
    Public UseCoreOutputDir As Boolean = True

    Public CapMapFunctions(,) As Integer

    ' Generate for each driver layer + 0 which is depth
    Public CapMaps As IEnviroInputData()

    ''' <summary>
    ''' Capacity calculation type per group
    ''' </summary>
    Public CapCalType() As EwEUtils.Core.eEcospaceCapacityCalType

    ''' <summary>
    ''' Nearest suitable map row (iPacket) for an IBM Packet by nStanzaGroups(nSplit), MaxStanzas, row, col
    ''' </summary>
    ''' <remarks></remarks>
    Public ItoUse(,,,) As Integer

    ''' <summary>
    ''' Nearest suitable map col (jPacket) for an IBM Packet by nStanzaGroups(nSplit), MaxStanzas, row, col
    ''' </summary>
    ''' <remarks></remarks>
    Public JtoUse(,,,) As Integer

    Public MovePacketsAtStanzaEntry As Boolean

    ''' <summary>
    ''' Primary Production Scaler average value of relPP(row,col) for all water cells
    ''' </summary>
    ''' <remarks>computed by ScaleRelativePrimaryProductivityToEcopathLevel() set in InitSpatialEquilibrium. 
    ''' In EwE5 this was local to FindSpatialEquilibrium. Here it has been move up in scope so that FindSpatialEquilibrium() can be split up into components.
    ''' Init (InitSpatialEquilibrium), run (FindSpatialEquilibrium) ......
    ''' 10-May-2012 Moved to cEcoSpaceDataStructures so PPScale can be set by the External PP Spatial Temporal data 
    ''' </remarks>
    Public PPScale As Double

    Public SaveASC As Boolean = False
    Public SaveCSV As Boolean = False

    ''' <summary>
    ''' Ratio of habitat area to total habitat capacity 
    ''' </summary>
    ''' <remarks>
    ''' BRatio(group) = ThabArea / TotHabCap(group) 
    ''' [total habitat area] / [sum of habitat capacity by group] 
    ''' </remarks>
    Public BRatio() As Single


    ''' <summary>
    ''' List of map cells that have a value in the Sail(,,)array less than EffortDistThreshold
    ''' </summary>
    ''' <remarks>Populate in <see cref="PopulateFleetCells"></see></remarks>
    Public FleetSailCells() As List(Of cRowCol)

    ''' <summary>
    ''' Boolean flag is fishing effort restricted to cells with a Sail() of less than EffortDistThreshold
    ''' </summary>
    ''' <remarks>True if fishing effort is restricted. False effort is distributed over all water cells.</remarks>
    Public bUseEffortDistThreshold As Boolean

    ''' <summary>
    ''' Threshold value in the Sail(fleet,row,col) [sailing cost map] for a cells inclusion in effort distribution. 
    ''' </summary>
    ''' <remarks></remarks>
    Public EffortDistThreshold As Single

    Public bUseLocalMemory As Boolean

    ''' <summary>
    ''' Total number of habitat area cells
    ''' Any cell with a depth > 0 of any habitat type
    ''' </summary>
    ''' <remarks>computed in CalcHabitatArea()</remarks>
    Public ThabArea As Single

    ''' <summary>
    ''' Calculate the TrophicLevel map in Ecospace. 
    ''' True Ecospace will populate the <see cref="cEcospaceDataStructures.TL">TrophicLevel</see> map in cEcospaceDataStructures.TL. 
    ''' </summary>
    ''' <remarks>This incurs significant overhead so it is Off(False) by default. At this time is can only be turned ON(True) via code.</remarks>
    Public bCalTrophicLevel As Boolean


    ''' <summary>
    ''' Number of fishing effort zones (LME's, EEZ...)
    ''' </summary>
    ''' <remarks></remarks>
    Public nEffZones As Integer

    ''' <summary>
    ''' Proportion of relative fishing effort for a fleet in an zone(LME,Region....) by nFleets, nEffZones
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    Public PropEffortFleetZone(,) As Single

    ''' <summary>
    ''' Index of the Effort Zone a cell is in by Row Col
    ''' </summary>
    ''' <remarks></remarks>
    Public EffZones(,) As Integer

    ''' <summary>
    ''' Sum of Effort modified by proportion of area fished in a cell.
    ''' </summary>
    ''' <remarks>
    ''' Set in <see cref="cEcospace.SetEffortParameters"> cEcospace.SetEffortParameter()
    ''' </see></remarks>
    Public TotEffort() As Single

    ''' <summary>
    ''' Use a "Spin-Up" period for Ecospace
    ''' </summary>
    ''' <remarks>Only Accessible from code at this time</remarks>
    Public UseSpinUp As Boolean

    ''' <summary>Are we in a Spin-Up period </summary>   
    Public bInSpinUp As Boolean

    Public UseSpinUpPlot As Boolean

    Public UseSpinUpBase As Boolean

    ''' <summary>
    ''' Number of years to run the Spin-Up for
    ''' </summary>
    ''' <remarks></remarks>
    Public SpinUpYears As Single

    ''' <summary>
    ''' Ecospace base biomass before the Spin-Up period. Gathered at the end of the first timestep.
    ''' </summary>
    ''' <remarks>Only populted if UseSpin = True</remarks>
    Public SpinUpBBase() As Single

    Public BaseFishMort() As Single
    Public BaseCatch() As Single
    Public BaseConsump() As Single
    Public BasePredMort() As Single

    Public isGroupHabCapChanged() As Boolean

    ''' <summary>
    ''' The Capacity model has a "one time" initialization of  <see cref="MaxHabCap"></see> value used for normalization of inputs.
    ''' This Flag gets set to True once MaxHabCap() has been set 
    ''' </summary>
    ''' <remarks></remarks>
    Public hasCapInitialized As Boolean

    ''' <summary>
    ''' User defined output directory for Ecospace output Maps
    ''' </summary>
    ''' <remarks>
    ''' Not used by the Scientic Interface. 
    ''' This is only used if UseCoreOutputDir = False and EcospaceMapOutputDir is not null.   
    ''' </remarks>
    Public EcospaceMapOutputDir As String

    ''' <summary>
    ''' User defined output directory for Ecospace Area Averaged outputs
    ''' </summary>
    ''' <remarks>
    ''' Not used by the Scientic Interface. 
    ''' This is only used if UseCoreOutputDir = False and EcospaceAreaOutputDir is not null.
    ''' </remarks>
    Public EcospaceAreaOutputDir As String


    ''' <summary>
    ''' First model time step to being writing Ecospace output files
    ''' </summary>
    ''' <remarks></remarks>
    Public FirstOutputTimeStep As Integer = 1

    ''' <summary>
    ''' Monthly Migration maps stored in a ragged array 
    ''' Dimensioned by (group,month)(row,col)
    ''' </summary>
    ''' <remarks></remarks>
    Public MigMaps(,)(,) As Single


    ''' <summary>
    ''' Is the Ecosim biomass time series forcing enabled for this group
    ''' </summary>
    Public IsEcosimBioForcingGroup() As Boolean
    Public UseEcosimBiomassForcing As Boolean = False

    Public IsEcosimDiscardForcingGroup() As Boolean
    Public UseEcosimDiscardForcing As Boolean = False

    Public dctENACells As Dictionary(Of String, cENACellData)

    Public bENA As Boolean

#End Region

#Region "Private Data"

    'not much
    Private m_ngroups As Integer
    Private m_publisher As cMessagePublisher
    Private m_bHasCapacityChanged As Boolean

#End Region

#Region "Construction"

    Public Sub New(ByVal MessagePublisher As cMessagePublisher)
        Me.m_publisher = MessagePublisher
    End Sub

#End Region

#Region "Public Properties"

    '''<summary>
    ''' Have any of the capacity input layers changed
    ''' </summary>
    ''' <remarks>Capacity Inputs, Habitats, Environmental layers, depth....</remarks>
    Public Property isCapacityChanged() As Boolean

        Get
            Return Me.m_bHasCapacityChanged
        End Get
        Set(value As Boolean)
            Me.m_bHasCapacityChanged = value
        End Set

    End Property

    ''' <summary>
    ''' Set the isGroupHabCapChanged() flag to True or False for all the groups
    ''' </summary>
    ''' <param name="Value"></param>
    ''' <remarks></remarks>
    Public Sub setHabCapGroupIsChanged(ByVal Value As Boolean)
        For igrp As Integer = 1 To NGroups
            Me.isGroupHabCapChanged(igrp) = Value
        Next
    End Sub



    ''' <summary>Number of Base Groups (Ecopath) </summary>
    ''' <remarks>This was nvar in EwE5</remarks>
    Public Property NGroups() As Integer
        Get
            Return m_ngroups
        End Get
        Set(ByVal value As Integer)
            m_ngroups = value
            RedimGroups() 'implicit ??????
            'this is different then the other counters (nFleets....) 
            'which delay the dimensioning until the data is loaded
            'this may not be a good idea
        End Set
    End Property

    Public ReadOnly Property nCellsInRegion(ByVal iRegion As Integer) As Integer

        Get
            Try
                Dim n As Integer = 0
                For irow As Integer = 1 To InRow
                    For icol As Integer = 1 To InCol
                        If Me.Depth(irow, icol) > 0 Then
                            If Region(irow, icol) = iRegion Then
                                n += 1
                            End If
                        End If
                    Next
                Next
                Return n

            Catch ex As Exception
                cLog.Write(ex)
                Return 0
            End Try
        End Get

    End Property

    Public ReadOnly Property nTimeSteps() As Integer

        Get
            Return CInt(TotalTime * (1 / TimeStep))
        End Get

    End Property

    ''' <summary>
    ''' Number of Ecospace time steps per year at the current <see cref="TimeStep">time step</see>
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property nTimeStepsPerYear As Integer
        Get
            Return CInt(1 / TimeStep)
        End Get
    End Property

    ''' <summary>
    ''' Returns whether any group is Advected
    ''' </summary>
    Public ReadOnly Property isAdvectionActive As Boolean
        Get
            For igrp As Integer = 1 To Me.m_ngroups
                If Me.IsAdvected(igrp) Then Return True
            Next
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Returns whether any group is forced through biomass timeseries in Ecosim.
    ''' </summary>
    Public ReadOnly Property isEcosimBiomassForcingLoaded As Boolean
        Get
            For igrp As Integer = 1 To Me.NGroups
                If Me.IsEcosimBioForcingGroup(igrp) Then
                    Return True
                End If
            Next
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Returns whether any group is forced through discards timeseries in Ecosim.
    ''' </summary>
    Public ReadOnly Property isEcosimDiscardForcingLoaded As Boolean
        Get
            For igrp As Integer = 1 To Me.NGroups
                If Me.IsEcosimDiscardForcingGroup(igrp) Then
                    Return True
                End If
            Next
            Return False
        End Get
    End Property

#End Region

#Region "Public Methods"


    Public Sub Clear()
        Me.m_ngroups = 0
        Me.nFleets = 0
        Me.TotalTime = 0
        Me.TotalTime = 0
        Me.nRegions = 0
        Me.InCol = 0
        Me.InRow = 0
        Me.nvartot = 0
        Me.NoHabitats = 0
        Me.UseEcosimBiomassForcing = False

        Try

            Depth = Nothing
            DepthA = Nothing
            DepthX = Nothing
            DepthY = Nothing
            Xvel = Nothing
            Yvel = Nothing
            Xvloc = Nothing
            Yvloc = Nothing
            UpVel = Nothing
            Xv = Nothing
            Yv = Nothing
            flow = Nothing
            Region = Nothing
            MPA = Nothing
            RelPP = Nothing
            RelCin = Nothing
            Sail = Nothing
            GroupDetritus = Nothing

            Basebiomass = Nothing
            Bnew = Nothing
            der = Nothing
            EatEffBad = Nothing
            MPABiomass = Nothing
            Mrate = Nothing
            Mvel = Nothing
            RelMoveBad = Nothing
            RelVulBad = Nothing
            IsAdvected = Nothing

            PrefRow = Nothing
            Prefcol = Nothing
            IsMigratory = Nothing
            MigConcRow = Nothing
            MigConcCol = Nothing
            barrierAvoidanceWeight = Nothing
            MigMaps = Nothing

            MPADBID = Nothing '(Me.MPAno)
            MPAname = Nothing '(Me.MPAno)
            MPAmonth = Nothing '(12, Me.MPAno)
            MPAfishery = Nothing '(Me.nFleets, Me.MPAno)

            ResultsByGroup = Nothing ', N_RESULTS_GROUPS, m_ngroups, NumberOfTimeSteps)
            ResultsByFleet = Nothing ', N_RESULTS_FLEETS, nFleets, NumberOfTimeSteps)
            ResultsByFleetGroup = Nothing ', N_RESULTS_FLEETGROUPS, nFleets, NGroups, NumberOfTimeSteps)

            ResultsRegionGroup = Nothing ', NoRegions, NGroups, NumberOfTimeSteps)
            ResultsCatchRegionGearGroup = Nothing ', NoRegions, nFleets, NGroups, NumberOfTimeSteps)
            MPred = Nothing
            EffortSpace = Nothing
            PredCell = Nothing
            IFDweight = Nothing
            Ftot = Nothing
            EffPower = Nothing
            SEmult = Nothing
            HabAreaProportion = Nothing
            HabArea = Nothing
            PHabType = Nothing
            FleetSailCells = Nothing

            MigMaps = Nothing

            If dctENACells IsNot Nothing Then dctENACells.Clear()
            dctENACells = Nothing

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Clear() Exception: " & ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Set default values and dimemsion basic arrays
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>This should be called before reading values from database. I think..... I hope!!!!!!!!!!</remarks>
    Public Function SetDefaults() As Boolean
        Dim i As Integer

        Try

            'EwE5 default value hardwired into the interface
            FitnessResp = 100
            PPupWell = 0.01
            PredictEffort = True

            'SOR weight from EwE5 interface frmSpace.text3
            W = 0.9

            TimeStep = 1 / 12 'monthly time steps. In EwE5 this is set all over the place 

            'EwE5 set to True in frmSpace.Form_Activate()
            'its value is then changed from an option radio button SpaceInit() on the run tab
            AdjustSpace = True

            'jb SpaceTime and CurrentForce defaults from EwE5 frmSpace.Load()
            SpaceTime = True 'in EwE5 the check box that controls this is labled 'Integrate' on the run tab
            CurrentForce = False

            InRow = 0
            InCol = 0

            AdvectSpeed = 0.1

            CellLength = 100 'this is from the EwE5 database

            MoveScale = 2.0 '0.2
            If TotalTime = 0 Then TotalTime = 50 'default of 50 year simulation

            'redimTimeVaraibles()
            setDefaultSummaryPeriod()

            NoHabitats = 1
            'requires NoHabitats, nGroups, nFleets, NoHabChanges
            RedimHabitatVariables()

            'dimension arrays to current problem size
            'DefaultBasemapDimensions()
            ReDimMapVars()

            'requires nGroups, calculates nvartot and Nvarsplit
            ' Debug.Assert(False, "Removed ReDimMapDims from SetDefaults")
            'ReDimMapDims()

            RedimMigratoryVariables()

            SetDefaultMeanVelocityMvel()

            For i = 1 To NGroups                            'CJW had nvar not n1
                PrefHab(i, 0) = 1.0! ' True
                InMigAreaMovement(i) = 0.1F
            Next 'set preferred habitat to 1 (pelagic) by default

            ReDimFleets()

            Me.bUseEffortDistThreshold = False
            EffortDistThreshold = 10000

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Spin Up
            Me.UseSpinUp = False
            Me.UseSpinUpPlot = False
            Me.SpinUpYears = 10
            'xxxxxxxxxxxxxxxxxxxxxxxxx

            Me.EcospaceAreaOutputDir = ""
            Me.EcospaceMapOutputDir = ""

            Return True
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            Return False
        End Try

    End Function


    Public Sub SetDefaultThreads()
        'multi threading defaults
        ' JS 08jun07: added 0 check since the datasource may have provided these values
        If (Me.nSpaceSolverThreads <= 0) Then
            Me.nGridSolverThreads = System.Environment.ProcessorCount
            Me.nSpaceSolverThreads = System.Environment.ProcessorCount
            Me.nEffortDistThreads = System.Environment.ProcessorCount
        End If

    End Sub

    Private Sub SetDefaultMeanVelocityMvel()
        Dim i As Integer
        Dim j As Integer

        Try

            Debug.Assert(EcoPathData IsNot Nothing, "Ecospace must have a reference to Ecopath data to initialize.")

            'Dim MaxTL As Single
            'MaxTL = 0
            'For j = 1 To NumLiving
            '    If TTLX(j) > MaxTL Then MaxTL = TTLX(j)
            'Next
            'MaxTL = MaxTL - 1
            'Set max average velocity movement to 100 km/year and the others linearly scaled after trophic level
            For j = 1 To NGroups  'NumLiving
                Mvel(j) = 300   'CInt(99 * (1 - (MaxTL - (TTLX(j) - 1)) / MaxTL)) + 1
            Next
            'For j = NumLiving + 1 To NumGroups
            '    Mvel(j) = 1
            'Next
            'How about discards they should have a lower dispersal rate:
            'check the discard fate
            'DiscardFate(NumGear, NumGroups - NumLiving)
            For j = nLiving + 1 To NGroups
                For i = 1 To nFleets
                    If EcoPathData.DiscardFate(i, j - nLiving) > 0 Then
                        Mvel(j) = 10
                        Exit For
                    End If

                Next
            Next

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".SetDefaultMeanVelocityMvel() Error: " & ex.Message)
            Throw New System.Exception(Me.ToString & ".SetDefaultMeanVelocityMvel() Error: " & ex.Message)
        End Try


    End Sub

    ''' <summary>
    ''' Redim variables for MPAs
    ''' </summary>
    ''' <remarks>In EwE5 this was handled when Ecosim loaded</remarks>
    Public Sub RedimMPAVariables()
        Try
            ReDim Me.MPADBID(Me.MPAno)
            ReDim MPAname(Me.MPAno)
            ReDim MPAmonth(12, Me.MPAno)
            ReDim MPAfishery(Me.nFleets, Me.MPAno)
            Me.allocate(Me.MPA, Me.MPAno, InRow + 1, InCol + 1)
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".RedimMPAVariables() Error: " & ex.Message)
            Throw New System.Exception(Me.ToString & ".RedimMPAVariables() Error: " & ex.Message)
        End Try


    End Sub

    ''' <summary>
    ''' Redim variables for migratory preferences
    ''' </summary>
    ''' <remarks>In EwE5 this was handled when Ecosim loaded</remarks>
    Public Sub RedimMigratoryVariables()
        Try

            ReDim IsMigratory(nvartot)
            ReDim PrefRow(NGroups, 12)
            ReDim Prefcol(NGroups, 12)
            ReDim MigConcRow(NGroups)
            ReDim MigConcCol(NGroups)
            ReDim barrierAvoidanceWeight(NGroups)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".RedimMigratoryVariables() Error: " & ex.Message)
            Throw New System.Exception(Me.ToString & ".RedimMigratoryVariables() Error: " & ex.Message)
        End Try


    End Sub

    ''' <summary>
    '''  Re-dimension the habitat variables
    ''' </summary>
    ''' <param name="PreserveHabitat">True to preserve the existing data in the habitat array. False to clear out this data (load a new model)</param>
    ''' <remarks>
    ''' This is called when ever the number of groups or habitat types changes.
    ''' Called when a new model is loaded (PreserveHabitat = False) or the user has changed the number of habitat types (PreserveHabitat = True).
    ''' If only the number of habitats has changed then it will keep the existing data (PreserveHabitat = True). 
    ''' If the number of groups has changed then all the data must be re-initialized (from the datasource).
    '''</remarks>
    Public Sub RedimHabitatVariables(Optional ByVal PreserveHabitat As Boolean = False)

        Try

            If Not PreserveHabitat Then
                'new model is being read
                'clear out the exiting data
                ReDim PrefHab(NGroups, NoHabitats)
                ReDim GearHab(nFleets, NoHabitats)
                ReDim HabitatText(NoHabitats)
                ReDim HabArea(NoHabitats)
                ReDim HabAreaProportion(NoHabitats)
                ReDim HabitatDBID(NoHabitats)

                allocate(Me.PHabType, Me.NoHabitats, Me.InRow, Me.InCol)

                ' JS 15oct07: fix for bug 289 - By default, GearHab and PrefHab are True for 'All' habitat
                For iGroup As Integer = 0 To NGroups
                    PrefHab(iGroup, 0) = 1.0! ' True
                Next

                For iFleet As Integer = 0 To nFleets
                    GearHab(iFleet, 0) = True
                Next

            Else
                'only the number of habitats has changed 
                'keep the existing data
                ReDim Preserve PrefHab(NGroups, NoHabitats)
                ReDim Preserve GearHab(nFleets, NoHabitats)
                ReDim Preserve HabitatText(NoHabitats)
                ReDim Preserve HabArea(NoHabitats)
                ReDim Preserve HabAreaProportion(NoHabitats)
                ReDim Preserve HabitatDBID(NoHabitats)

            End If

            Me.allocate(PHabType, NoHabitats, InRow, InCol)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".RedimHabitatVariables() Error: " & ex.Message)
            Throw New System.Exception(Me.ToString & ".RedimHabitatVariables() Error: " & ex.Message)
        End Try


    End Sub

    ''' <summary>
    ''' Set the Map to its default size
    ''' </summary>
    Public Sub DefaultBasemapDimensions()

        If InRow = 0 Then InRow = 20 'number of map cell rows
        If InCol = 0 Then InCol = 20 'number of map cell columns
        If CellLength = 0 Then CellLength = 5 'map cell size, in degrees

    End Sub

    Sub ReDimMapVars()
        Dim i As Integer, j As Integer

        Try

            Debug.Assert(StanzaGroups IsNot Nothing, Me.ToString & ".ReDimMapVars() Stanzagroups needs to be set.")

            'count up the total number of stanza groups
            Nvarsplit = 0
            For i = 1 To StanzaGroups.Nsplit
                For j = 1 To StanzaGroups.Nstanza(i)
                    Nvarsplit = Nvarsplit + 1
                Next
            Next

            'jb EwE5 EwE6 does not have Pairs (split pools)
            'nvartot = NumGroups + 2 * npairs + Nvarsplit
            nvartot = NGroups + Nvarsplit

            ReDim Basebiomass(nvartot)
            ReDim Bnew(nvartot)
            ReDim der(nvartot)
            'ReDim EatEff(nvartot)
            ReDim EatEffBad(nvartot)
            'ReDim Flowin(nvartot)
            'ReDim FlowoutRate(nvartot)
            ReDim MPABiomass(nvartot)
            ReDim Mrate(nvartot)
            ReDim Mvel(nvartot)
            ReDim RelMoveBad(nvartot)
            ReDim RelVulBad(nvartot)
            ReDim IsAdvected(NGroups)
            ReDim Me.TotHabCap(NGroups)
            ReDim Me.MaxHabCap(NGroups)

            ReDim InMigAreaMovement(NGroups)

            ' Allocate room for Depth map
            ReDim Me.CapMapFunctions(Me.nEnvironmentalDriverLayers + 1, Me.NGroups)

            ReDim Me.ImportanceLayerDBID(nImportanceLayers)
            ReDim Me.ImportanceLayerName(nImportanceLayers)
            ReDim Me.ImportanceLayerDescription(nImportanceLayers)
            ReDim Me.ImportanceLayerWeight(nImportanceLayers)

            ReDim Me.EnvironmentalLayerDBID(nEnvironmentalDriverLayers)
            ReDim Me.EnvironmentalLayerName(nEnvironmentalDriverLayers)
            ReDim Me.EnvironmentalLayerDescription(nEnvironmentalDriverLayers)
            ReDim Me.EnvironmentalLayerUnits(nEnvironmentalDriverLayers)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".ReDimMapVars() Error: " & ex.Message)
            Throw New System.Exception(Me.ToString & ".ReDimMapVars() Error: " & ex.Message)
        End Try

    End Sub

    Public Sub ReDimFleets()
        Try

            ReDim Me.FleetDBID(nFleets)
            ReDim Me.EcopathFleetDBID(nFleets)
            ReDim Me.SEmult(nFleets)
            ReDim Me.EffPower(nFleets)

            'Sets the number of Effort Areas to a default of one
            Me.ReDimEffortZones(1)

            Me.setFleetDefaults()

            ReDim FleetSailCells(nFleets)
            For iflt As Integer = 1 To nFleets
                FleetSailCells(iflt) = New List(Of cRowCol)
            Next

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".ReDimFleets() Error: " & ex.Message)
            Throw New System.Exception(Me.ToString & ".ReDimFleets() Error: " & ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Dimensions and sets the number of Effort Zones
    ''' </summary>
    ''' <param name="NumberOfZones">Number of Effort Zones</param>
    ''' <remarks>Sets PropEffortFleetArea(nFleets,nAreas) to a default of one</remarks>
    Public Sub ReDimEffortZones(ByVal NumberOfZones As Integer)
        Debug.Assert(NumberOfZones > 0, "ReDimPropEffortArea(nAreas) NumberOfAreas must be greater than 0.")

        Me.nEffZones = NumberOfZones
        Me.PropEffortFleetZone = New Single(nFleets, nEffZones) {}

        For iflt As Integer = 1 To nFleets
            'Default proportion of effort in an area = 1
            For iarea As Integer = 0 To nEffZones
                PropEffortFleetZone(iflt, iarea) = 1
            Next iarea
        Next

    End Sub


    Private Sub setFleetDefaults()

        'jb just set to default of one
        For i As Integer = 1 To nFleets
            EffPower(i) = 1
            SEmult(i) = 1
        Next

    End Sub

    ''' <summary>
    ''' Make sure there are migration maps for all migrating groups.
    ''' </summary>
    ''' <remarks>
    ''' On the first load new maps should be allocated for all the groups. 
    ''' Once a model has loaded all the existing maps should be preserved and only new maps should get new blank maps.
    ''' This should preserve the users configuration if the turn a migrating group on or off. 
    ''' </remarks>
    Friend Sub RedimMigrationMaps(bClearExisting As Boolean)

        If bClearExisting Then
            Me.MigMaps = Nothing
        End If

        If (Me.MigMaps Is Nothing) Then
            Me.MigMaps = New Single(NGroups, 12)(,) {}
        End If

        '  Me.MigMaps = New Single(NGroups, 12)(,) {}
        For iGrp As Integer = 1 To Me.NGroups
            If IsMigratory(iGrp) Then
                If (MigMaps(iGrp, 1) Is Nothing) Then
                    For iMonth As Integer = 1 To 12
                        Me.MigMaps(iGrp, iMonth) = New Single(InRow + 1, InCol + 1) {}
                    Next
                End If
            End If
        Next
    End Sub

    Friend Sub DebugTestEffortZones()

        'Warning
        Debug.Assert(False, "Effort Zones have been set for debugging.")
        'set the number of zones to 4
        Me.ReDimEffortZones(4)

        For iflt As Integer = 1 To Me.nFleets
            For iz As Integer = 1 To Me.nEffZones
                'Effort by zone
                Me.PropEffortFleetZone(iflt, iz) = CSng(iz / nEffZones)
            Next
        Next
        Dim iseq As Integer
        For ir As Integer = 1 To InRow
            For ic As Integer = 1 To InCol
                iseq += 1
                Me.EffZones(ir, ic) = 1 + CInt((nEffZones - 1) * (iseq / (InRow * InCol)))
            Next
        Next

    End Sub


    'Friend Sub debugSetAdvectionVectors()
    '    Debug.Assert(False, "Warning Advection Vectors have been hardcoded for debuging...")
    '    ReDim Me.Xvel(Me.InRow + 1, Me.InCol + 1)
    '    ReDim Me.Yvel(Me.InRow + 1, Me.InCol + 1)
    '    Dim vel As Single = 0
    '    For i As Integer = 0 To Me.InRow + 1
    '        For j As Integer = 0 To Me.InCol + 1
    '            '  If Me.Depth(i, j) > 0 Then
    '            Me.Xvel(i, j) = vel
    '            Me.Yvel(i, j) = vel
    '            vel += 1
    '            '  End If
    '        Next j
    '    Next i
    'End Sub

    'Friend Sub debugSetMigMapsFromPrefRowCol()

    '    Debug.Assert(False, "Warning debugSetMigrationMaps() Setting Migration Maps with values in PrefRow() and PrefCol()")
    '    Dim OffSet As Integer = 1
    '    Dim i1 As Integer, i2 As Integer, j1 As Integer, j2 As Integer
    '    For igrp As Integer = 1 To NGroups
    '        For imon As Integer = 1 To 12
    '            If IsMigratory(igrp) Then
    '                For irow As Integer = 1 To InRow
    '                    For icol As Integer = 1 To InCol

    '                        If PrefRow(igrp, imon) = irow And icol = Prefcol(igrp, imon) Then

    '                            i1 = irow - OffSet : If i1 < 1 Then i1 = 1
    '                            i2 = irow + OffSet : If i2 > InRow Then i2 = InRow
    '                            j1 = icol - OffSet : If j1 < 1 Then j1 = 1
    '                            j2 = icol + OffSet : If j2 > InCol Then j2 = InCol
    '                            For ii As Integer = i1 To i2
    '                                For jj As Integer = j1 To j2
    '                                    Me.MigMaps(igrp, imon)(ii, jj) = True
    '                                Next
    '                            Next


    '                        End If

    '                    Next icol
    '                Next irow
    '            End If 'If IsMigratory(igrp) Then
    '        Next imon
    '    Next igrp
    'End Sub

    Friend Sub calcPrefRowColFromMigrationMap()

        Debug.Assert(False, "Warning debugCalcPrefRowColFromMap() Calculating PrefRow() PrefCol() from Migration Maps")
        Dim minRow As Integer, maxRow As Integer, minCol As Integer, maxCol As Integer
        For igrp As Integer = 1 To NGroups
            If IsMigratory(igrp) Then
                For imon As Integer = 1 To 12
                    minRow = InRow + 1
                    minCol = InCol + 1
                    maxRow = 0
                    maxCol = 0

                    For irow As Integer = 1 To InRow
                        For icol As Integer = 1 To InCol

                            If (Me.MigMaps(igrp, imon)(irow, icol) > cEcoSpace.MIN_MIG_PROB) Then
                                minRow = Math.Min(irow, minRow)
                                minCol = Math.Min(icol, minCol)
                                maxRow = Math.Max(irow, maxRow)
                                maxCol = Math.Max(icol, maxCol)
                            End If
                        Next icol
                    Next irow
                    PrefRow(igrp, imon) = (minRow + maxRow) \ 2
                    Prefcol(igrp, imon) = (minCol + maxCol) \ 2

                Next imon
            End If 'If IsMigratory(igrp) Then
        Next igrp
    End Sub


    Friend Sub debugTestDiscardsMaps()
        Dim sumDiscards As Single
        Dim n As Integer

        System.Console.WriteLine("---------------Discards Dump-------------------")

        For igrp As Integer = 1 To Me.NGroups
            sumDiscards = 0
            n = 0
            For ir As Integer = 1 To InRow
                For ic As Integer = 1 To InCol
                    If DiscardsMap(ir, ic, igrp) > 0 Then
                        sumDiscards += DiscardsMap(ir, ic, igrp)
                        n += 1
                    End If
                Next ic
            Next ir

            If sumDiscards > 0 Then
                System.Console.WriteLine("Discards for group " + igrp.ToString + " = " + (sumDiscards / n).ToString)
            End If

        Next igrp

        System.Console.WriteLine("---------------END Discards Dump-------------------")


    End Sub


    Friend Sub debugDumpContaminantMap(foriGroup As Integer)
        System.Console.WriteLine("-------------Contaminants for " + foriGroup.ToString + "----------")
        Dim sumC As Single
        For ir As Integer = 1 To Me.InRow
            For ic As Integer = 1 To Me.InCol
                'System.Console.Write(Me.Ccell(ir, ic, foriGroup).ToString)
                'If ic <> Me.InCol Then
                '    System.Console.Write(", ")
                'Else
                '    System.Console.WriteLine()
                'End If

                sumC += (Me.Ccell(ir, ic, foriGroup))
            Next
        Next


        System.Console.WriteLine("Sum, " + sumC.ToString)
        System.Console.WriteLine("-----------------------")
    End Sub



    ''' <summary>
    ''' Populate <see cref="cEcospaceDataStructures.FleetSailCells"></see> with a list of map cells in <see cref="cEcospaceDataStructures.Sail"></see> that are less than <see cref="cEcospaceDataStructures.EffortDistThreshold"></see>
    ''' </summary>
    ''' <remarks> FleetSailCells is used by <see cref="cEcoSpace.PredictEffortDistributionThreadedLoadShared"></see> to calculate effort distribution only on cells in the list</remarks>
    Public Sub PopulateFleetCells()

        If Not Me.bUseEffortDistThreshold Then Return

        System.Console.WriteLine("Calculating map cells per fleet.")
        System.Console.WriteLine("Number of water cells " + Me.nWaterCells.ToString)
        For iflt As Integer = 1 To Me.nFleets
            Me.FleetSailCells(iflt).Clear()
            For ir As Integer = 1 To Me.InRow
                For ic As Integer = 1 To Me.InCol
                    If Depth(ir, ic) > 0 Then
                        If Me.Sail(iflt)(ir, ic) < Me.EffortDistThreshold Then
                            Me.FleetSailCells(iflt).Add(New cRowCol(ir, ic))
                        End If 'Me.Sail(iflt, ir, ic) < Me.FleetSailThreshold 
                    End If 'Depth(ir, ic) > 0
                Next ic
            Next ir

            System.Console.WriteLine("  Fleet " + iflt.ToString + " n cells, " + FleetSailCells(iflt).Count.ToString)

        Next iflt

    End Sub

    ''' <summary>
    ''' Allocate memory for an array with 4 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array(,,,) As Single, ByVal d1 As Integer, ByVal d2 As Integer, ByVal d3 As Integer, ByVal d4 As Integer)

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) * (d2 + 1) * (d3 + 1) * (d4 + 1) Then
                System.Array.Clear(array, 0, array.Length)
                Return
            End If
        End If

        Erase array
        array = Nothing
        'Dim mgs As Single = CSng(d1 * d2 * d3 * d4 * 4 / 1048576)
        'System.Console.WriteLine("Allocating=" & mgs.ToString & " Memory=" & (GC.GetTotalMemory(True) / 1048576).ToString)
        'GC.Collect()

        ReDim array(d1, d2, d3, d4)

    End Sub

    ''' <summary>
    ''' Allocate memory for an array with 3 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array(,,) As Single, ByVal d1 As Integer, ByVal d2 As Integer, ByVal d3 As Integer)

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) * (d2 + 1) * (d3 + 1) Then
                System.Array.Clear(array, 0, array.Length)
                Return
            End If
        End If

        Erase array
        array = Nothing

        'GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced)
        'Dim mgs As Single = CSng(d1 * d2 * d3 * 4 / 1048576)
        'System.Console.WriteLine("Allocating=" & mgs.ToString & " Memory=" & (GC.GetTotalMemory(True) / 1048576).ToString)

        GC.Collect()
        ReDim array(d1, d2, d3)

    End Sub

    ''' <summary>
    ''' Allocate memory for an array with 3 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array()(,) As Single, ByVal d1 As Integer, ByVal d2 As Integer, ByVal d3 As Integer)
        Dim bCleared As Boolean = True

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) Then

                For i As Integer = 0 To d1
                    If array(i).Length = (d2 + 1) * (d3 + 1) Then
                        System.Array.Clear(array(i), 0, array(i).Length)
                    Else
                        bCleared = False
                        Exit For
                    End If
                Next

                'If we managed to clear the array then Return
                'If NOT the allocate a new array
                If bCleared Then
                    Return
                End If

            End If
        End If

        Erase array
        array = Nothing

        array = New Single(d1)(,) {}
        For i As Integer = 0 To d1
            array(i) = New Single(d2, d3) {}
        Next
        GC.Collect()

    End Sub

    ''' <summary>
    ''' Allocate memory for an array with 3 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array()(,) As Boolean, ByVal d1 As Integer, ByVal d2 As Integer, ByVal d3 As Integer)
        Dim bCleared As Boolean = True

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) Then

                For i As Integer = 0 To d1
                    If array(i).Length = (d2 + 1) * (d3 + 1) Then
                        System.Array.Clear(array(i), 0, array(i).Length)
                    Else
                        bCleared = False
                        Exit For
                    End If
                Next

                'If we managed to clear the array then Return
                'If NOT the allocate a new array
                If bCleared Then
                    Return
                End If

            End If
        End If

        Erase array
        array = Nothing

        array = New Boolean(d1)(,) {}
        For i As Integer = 0 To d1
            array(i) = New Boolean(d2, d3) {}
        Next
        GC.Collect()

    End Sub



    ''' <summary>
    ''' Allocate memory for an array with 3 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array(,,) As Boolean, ByVal d1 As Integer, ByVal d2 As Integer, ByVal d3 As Integer)

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) * (d2 + 1) * (d3 + 1) Then
                System.Array.Clear(array, 0, array.Length)
                Return
            End If
        End If

        Erase array
        array = Nothing

        'GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced)
        'Dim mgs As Single = CSng(d1 * d2 * d3 * 4 / 1048576)
        'System.Console.WriteLine("Allocating=" & mgs.ToString & " Memory=" & (GC.GetTotalMemory(True) / 1048576).ToString)

        GC.Collect()
        ReDim array(d1, d2, d3)

    End Sub


    ''' <summary>
    ''' Allocate memory for an array of singles with 2 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array(,) As Single, ByVal d1 As Integer, ByVal d2 As Integer)

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) * (d2 + 1) Then
                System.Array.Clear(array, 0, array.Length)
                Return
            End If
        End If

        Erase array
        array = Nothing
        GC.Collect()

        ReDim array(d1, d2)

    End Sub

    ''' <summary>
    ''' Allocate memory for an array with 2 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array(,) As Integer, ByVal d1 As Integer, ByVal d2 As Integer)

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) * (d2 + 1) Then
                System.Array.Clear(array, 0, array.Length)
                Return
            End If
        End If

        'Erase array
        'array = Nothing
        'GC.Collect()

        ReDim array(d1, d2)

    End Sub

    ''' <summary>
    ''' Allocate memory for an array with 3 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array(,,) As Integer, ByVal d1 As Integer, ByVal d2 As Integer, d3 As Integer)

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) * (d2 + 1) * (d3 + 1) Then
                System.Array.Clear(array, 0, array.Length)
                Return
            End If
        End If

        Erase array
        array = Nothing

        'GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced)
        'Dim mgs As Single = CSng(d1 * d2 * d3 * 4 / 1048576)
        'System.Console.WriteLine("Allocating=" & mgs.ToString & " Memory=" & (GC.GetTotalMemory(True) / 1048576).ToString)

        GC.Collect()
        ReDim array(d1, d2, d3)

    End Sub

    ''' <summary>
    ''' Allocate memory for an array of boolean values with 2 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array(,) As Boolean, ByVal d1 As Integer, ByVal d2 As Integer)

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) * (d2 + 1) Then
                System.Array.Clear(array, 0, array.Length)
                Return
            End If
        End If

        'Erase array
        'array = Nothing
        'GC.Collect()

        ReDim array(d1, d2)

    End Sub

    ' == JS added to move overlapping MPA logic to sparse arrays ==

    ''' <summary>
    ''' Allocate memory for an array with 3 dimensions
    ''' </summary>
    ''' <remarks>Do garbage collection on the discarded memory so memory in never allocated twice.</remarks>
    Friend Sub allocate(ByRef array()(,) As Integer, ByVal d1 As Integer, ByVal d2 As Integer, ByVal d3 As Integer)
        Dim bCleared As Boolean = True

        If array IsNot Nothing Then
            If array.Length = (d1 + 1) Then

                For i As Integer = 0 To d1
                    If array(i).Length = (d2 + 1) * (d3 + 1) Then
                        System.Array.Clear(array(i), 0, array(i).Length)
                    Else
                        bCleared = False
                        Exit For
                    End If
                Next

                'If we managed to clear the array then Return
                'If NOT the allocate a new array
                If bCleared Then
                    Return
                End If

            End If
        End If

        Erase array
        array = Nothing

        array = New Integer(d1)(,) {}
        For i As Integer = 0 To d1
            array(i) = New Integer(d2, d3) {}
        Next
        GC.Collect()

    End Sub

    Public Sub ReDimMapDims()
        'NvarTot = nvar + 2 * npairs
        Dim i As Integer, j As Integer, k As Integer

        Debug.Assert(StanzaGroups IsNot Nothing, Me.ToString & ".ReDimMapDims() Stanzagroups needs to be set.")

        Try

            'jb this is also set in ReDimMapVars()
            Nvarsplit = 0
            For i = 1 To StanzaGroups.Nsplit
                For j = 1 To StanzaGroups.Nstanza(i)
                    Nvarsplit = Nvarsplit + 1
                Next
            Next
            nvartot = NGroups + Nvarsplit

            'force the garbage collection
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced)

            Me.allocate(Bcell, InRow + 1, InCol + 1, nvartot)
            Me.allocate(Blast, InRow + 1, InCol + 1, nvartot)

            Me.allocate(CatchMap, InRow, InCol, NGroups)
            Me.allocate(DiscardsMap, InRow, InCol, NGroups)
            Me.allocate(CatchFleetMap, InRow, InCol, nFleets)

            'For Nereus EcoOcean there are more fleets than groups
            'so dimension the fleets first
            Me.allocate(Port, nFleets, InRow, InCol)
            Me.allocate(PAreaFished, nFleets, InRow, InCol)
            Me.allocate(Sail, nFleets, InRow + 1, InCol + 1)

            Me.allocate(Me.HabCapInput, NGroups, InRow + 1, InCol + 1)
            For i = 1 To InRow : For j = 1 To InCol : For k = 1 To NGroups : HabCapInput(k)(i, j) = 1 : Next : Next : Next
            Me.allocate(Me.HabCap, NGroups, InRow + 1, InCol + 1)

            Me.allocate(PHabType, NoHabitats, InRow, InCol)

            Me.allocate(Xv, InRow + 1, InCol + 1, cCore.N_MONTHS)
            Me.allocate(Yv, InRow + 1, InCol + 1, cCore.N_MONTHS)
            '  For i = 1 To InRow : For j = 1 To InCol : For k = 1 To cCore.N_MONTHS : Xv(i, j, k) = 1 : Yv(i, j, k) = 1 : Next : Next : Next

            Me.allocate(DepthInput, InRow + 1, InCol + 1)
            'Resized basemap should have water everywhere
            DepthInput.Fill(1)
            Me.allocate(Excluded, InRow + 1, InCol + 1)

            Me.allocate(Depth, InRow + 1, InCol + 1)
            Me.allocate(DepthA, InRow + 1, InCol + 1)
            Me.allocate(DepthX, InRow + 1, InCol + 1)
            Me.allocate(DepthY, InRow + 1, InCol + 1)
            Me.allocate(Xvel, InRow + 1, InCol + 1)
            Me.allocate(Yvel, InRow + 1, InCol + 1)
            Me.allocate(Xvloc, InRow + 1, InCol + 1)
            Me.allocate(Yvloc, InRow + 1, InCol + 1)
            Me.allocate(UpVel, InRow + 1, InCol + 1)
            Me.allocate(flow, InRow + 1, InCol + 1)

            Me.allocate(Region, InRow + 1, InCol + 1)
            Me.allocate(RelPP, InRow + 1, InCol + 1)
            Me.allocate(RelCin, InRow + 1, InCol + 1)

            ' JS 14May16: Only allocate this temporary array when a relPP backup is made
            'Me.allocate(relPP0, InRow + 1, InCol + 1)
            Me.RelPP0 = Nothing

            Me.allocate(TL, InRow, InCol, NGroups)
            Me.allocate(TLc, InRow, InCol)
            Me.allocate(KemptonsQ, InRow, InCol)
            Me.allocate(ShannonDiversity, InRow, InCol)

            Me.allocate(ImportanceLayerMap, Me.nImportanceLayers, InRow + 1, InCol + 1)
            Me.allocate(EnvironmentalLayerMap, Me.nEnvironmentalDriverLayers, InRow + 1, InCol + 1)

            ReDim MPAfishery(nFleets, 1)
            ReDim MPAmonth(12, 1)
            ReDim IsFished(nFleets, Me.InRow, Me.InCol)
            ReDim EffZones(InRow, InCol)
            ReDim Width(InRow)

            Me.allocate(MonthlyXvel, 12, InRow + 1, InCol + 1)
            Me.allocate(MonthlyYvel, 12, InRow + 1, InCol + 1)
            Me.allocate(MonthlyUpWell, 12, InRow + 1, InCol + 1)


            'jb move this here to set a few defaults this will have to change
            For i = 1 To NGroups                            'CJW had nvar not n1
                PrefHab(i, 0) = 1.0! ' True
            Next 'set preferred habitat to 1 (pelagic) by default

            'Populate the Width() array
            Me.CalculateCellWidth()

            For i = 1 To InRow


                For j = 1 To InCol      'Default Values for new maps
                    Depth(i, j) = 1
                    DepthA(i, j) = Depth(i, j)
                    ' HabType(i, j) = 1
                    RelPP(i, j) = 1
                    RelCin(i, j) = 1
                    For k = 1 To nFleets
                        Sail(k)(i, j) = 1
                    Next

                    'Use all habitats
                    PHabType(0)(i, j) = 1.0F

                    'Default Areas=1
                    Me.EffZones(i, j) = 1

                Next j
            Next i

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".ReDimMapDims() Error: " & ex.Message)
            Throw New System.Exception(Me.ToString & ".ReDimMapDims() Error: " & ex.Message)
        End Try

    End Sub

    Friend Sub CalculateCellWidth()

        Dim halfcell As Single = Me.CellLength / 2 / (60 * 1.852F)
        Dim dtLat As Single
        For i As Integer = 1 To InRow

            dtLat = Me.CellLength * (i - 1) / (60 * 1.852F) - halfcell
            'System.Console.WriteLine((Lat1 - dtLat).ToString + ", ")

            'jb 28-Nov-2013 find width for the center of the cell
            If (Me.AssumeSquareCells) Then
                Width(i) = 1
            Else
                'half a cell height in degrees 
                Dim Lat As Single = Lat1 - dtLat
                Width(i) = CSng(Math.Cos(Lat / 90.0 * Math.PI / 2.0))
            End If

        Next i


    End Sub


    Public Sub RedimConSimVars()

        ReDim Ccell(InRow + 1, InCol + 1, NGroups)
        ReDim Clast(InRow + 1, InCol + 1, NGroups)
        ReDim AMmTr(InRow + 1, InCol + 1, NGroups)
        ReDim Ftr(InRow + 1, InCol + 1, NGroups)

    End Sub

    Public Sub RedimGroups()
        Try
            ReDim GroupDBID(m_ngroups)
            ReDim EcopathGroupDBID(m_ngroups)
            ReDim CapCalType(m_ngroups)

            ReDim IsEcosimBioForcingGroup(m_ngroups)
            ReDim IsEcosimDiscardForcingGroup(m_ngroups)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".redimGroupDBID() Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Redim the data that saves the Ecospace results over time
    ''' </summary>
    ''' <remarks>This must be called by Ecospace at the start of a run to clear out any existing data.</remarks>
    Public Function redimTimeStepResults(ByVal NumberOfTimeSteps As Integer) As Boolean

        'Debug.Assert(TimeStep > 0 And TotalTime > 0)
        Dim success As Boolean = True

        'reset the number of time steps the model ran for
        'nSumTimeSteps = 0
        Dim message As cMessage

        Try

            Me.allocate(ResultsByGroup, [Enum].GetValues(GetType(eSpaceResultsGroups)).Length, NGroups, NumberOfTimeSteps)
            Me.allocate(ResultsByFleet, [Enum].GetValues(GetType(eSpaceResultsFleets)).Length, nFleets, NumberOfTimeSteps)
            Me.allocate(ResultsByFleetGroup, [Enum].GetValues(GetType(eSpaceResultsFleetsGroups)).Length, nFleets, NGroups, NumberOfTimeSteps)

            Me.allocate(ResultsRegionGroup, nRegions, NGroups, NumberOfTimeSteps)
            Me.allocate(ResultsRegionGroupYear, nRegions, NGroups, CInt(NumberOfTimeSteps / Math.Max(Me.NumStep, 1) + 1))
            Me.allocate(ResultsCatchRegionGearGroup, nRegions, nFleets, NGroups, NumberOfTimeSteps)
            Me.allocate(ResultsCatchRegionGearGroupYear, nRegions, nFleets, NGroups, CInt(NumberOfTimeSteps / Math.Max(Me.NumStep, 1) + 1))

        Catch exmem As OutOfMemoryException
            System.Console.WriteLine(Me.ToString & ".redimTimeStepResults() Out of memory: " & exmem.Message)
            message = New cMessage(My.Resources.CoreMessages.ECOSPACE_OUT_OF_MEMORY,
                                   eMessageType.Any, EwEUtils.Core.eCoreComponentType.EcoSpace, eMessageImportance.Critical)
        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".redimTimeStepResults(): " & ex.Message)
            message = New cMessage(ex.Message, eMessageType.Any, EwEUtils.Core.eCoreComponentType.EcoSpace, eMessageImportance.Critical)
        End Try

        If message IsNot Nothing Then
            Me.m_publisher.AddMessage(message)
            success = False
        End If

        Return success

    End Function


    Public Sub setDefaultSummaryPeriod()
        Try
            Debug.Assert(TimeStep > 0)
            'set the summary data to be over the total time
            SumStart(0) = 0 'start of first summary period
            SumStart(1) = TotalTime - 1 'start of last summary perion
            NumStep = Math.Max(1, CInt(1.0 / TimeStep)) 'number of time steps to summarize over one year for the default summary
        Catch ex As Exception
            SumStart(0) = 0 'start of first summary period
            SumStart(1) = TotalTime - 1 'start of last summary period
            NumStep = 1 'number of time steps to summarize over one year for the default summary
            Debug.Assert(False)
        End Try
    End Sub

    ''' <summary>
    ''' Ecospace spatial reference data not used but left in place for legacy reasons 
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub redimForReferenceData()

        Debug.Assert(False, "Ecospace spatial reference data has not been implemented yet!")

        Dim ttYears As Integer = CInt(TotalTime)
        ReDim SpaceBiomassByRegion(ttYears, NGroups, nRegions)
        ReDim SpaceBiomassByRegionCount(ttYears, NGroups, nRegions)
        ReDim SpaceCatchByRegion(ttYears, NGroups, nRegions)
        ReDim SpaceCatchByRegionCount(ttYears, NGroups, nRegions)
        ReDim SpaceEffortByRegionFleet(ttYears, nFleets, nRegions)
        ReDim SpaceEffortByRegionFleetCount(ttYears, nFleets, nRegions)

    End Sub


    ''' <summary>
    ''' Get sum of Biomass by Region Group for the Start and End summary period
    ''' </summary>
    ''' <remarks>Summary time windows are defined by the user</remarks>
    Public Sub getSumBiomByRegion(ByVal iRegion As Integer, ByVal iGroup As Integer, ByRef startBio As Single, ByRef endBio As Single)
        Dim st As Integer, et As Integer, nts As Integer
        startBio = 0
        endBio = 0

        'get the start and end time indexes and number of time steps to sum over
        'getStartEndSumIndex() will figure out the one based indexes
        Me.getStartEndSumIndex(st, et, nts)

        For it As Integer = st To st + nts - 1
            startBio = startBio + Me.ResultsRegionGroup(iRegion, iGroup, it)
        Next
        startBio = startBio / nts

        For it As Integer = et To et + nts - 1
            endBio = endBio + Me.ResultsRegionGroup(iRegion, iGroup, it)
        Next
        endBio = endBio / nts

    End Sub
    ''' <summary>
    ''' Get Biomass for summary periods
    ''' </summary>
    Public Sub getSumBiom(ByVal iGroup As Integer, ByRef startBio As Single, ByRef endBio As Single)
        Dim st As Integer, et As Integer, nts As Integer
        startBio = 0
        endBio = 0

        'get the start and end time indexes and number of time steps to sum over
        'getStartEndSumIndex() will figure out the one based indexes
        Me.getStartEndSumIndex(st, et, nts)

        For it As Integer = st To st + nts - 1
            startBio = startBio + Me.ResultsByGroup(eSpaceResultsGroups.Biomass, iGroup, it)
        Next
        startBio = startBio / nts

        For it As Integer = et To et + nts - 1
            endBio = endBio + Me.ResultsByGroup(eSpaceResultsGroups.Biomass, iGroup, it)
        Next
        endBio = endBio / nts

    End Sub

    ''' <summary>
    ''' Get Catch by Fleet Group for summary periods
    ''' </summary>
    Public Sub getSumCatchFleetGroup(ByVal iFleet As Integer, ByVal iGroup As Integer, ByRef startCatch As Single, ByRef endCatch As Single)
        Dim st As Integer, et As Integer, nts As Integer
        startCatch = 0
        endCatch = 0

        'get the start and end time indexes and number of time steps to sum over
        'getStartEndSumIndex() will figure out the one based indexes
        Me.getStartEndSumIndex(st, et, nts)

        For it As Integer = st To st + nts - 1
            startCatch = startCatch + Me.ResultsByFleetGroup(eSpaceResultsFleetsGroups.CatchBio, iFleet, iGroup, it)
        Next
        startCatch = startCatch / nts

        For it As Integer = et To et + nts - 1
            endCatch = endCatch + Me.ResultsByFleetGroup(eSpaceResultsFleetsGroups.CatchBio, iFleet, iGroup, it)
        Next
        endCatch = endCatch / nts

    End Sub

    ''' <summary>
    ''' Get Value by Fleet Group for summary periods
    ''' </summary>
    Public Sub getSumValueFleetGroup(ByVal iFleet As Integer, ByVal iGroup As Integer, ByRef startCatch As Single, ByRef endCatch As Single)
        Dim st As Integer, et As Integer, nts As Integer
        startCatch = 0
        endCatch = 0

        'get the start and end time indexes and number of time steps to sum over
        'getStartEndSumIndex() will figure out the one based indexes
        Me.getStartEndSumIndex(st, et, nts)

        For it As Integer = st To st + nts - 1
            startCatch = startCatch + Me.ResultsByFleetGroup(eSpaceResultsFleetsGroups.Value, iFleet, iGroup, it)
        Next
        startCatch = startCatch / nts

        For it As Integer = et To et + nts - 1
            endCatch = endCatch + Me.ResultsByFleetGroup(eSpaceResultsFleetsGroups.Value, iFleet, iGroup, it)
        Next
        endCatch = endCatch / nts

    End Sub

    ''' <summary>
    ''' Get Catch by Fleet for summary periods
    ''' </summary>
    Public Sub getSumCatchFleet(ByVal iFleet As Integer, ByRef startCatch As Single, ByRef endCatch As Single)
        Dim st As Integer, et As Integer, nts As Integer
        startCatch = 0
        endCatch = 0

        'get the start and end time indexes and number of time steps to sum over
        'getStartEndSumIndex() will figure out the one based indexes
        Me.getStartEndSumIndex(st, et, nts)

        For it As Integer = st To st + nts - 1
            startCatch = startCatch + Me.ResultsByFleet(eSpaceResultsFleets.CatchBio, iFleet, it)
        Next
        startCatch = startCatch / nts

        For it As Integer = et To et + nts - 1
            endCatch = endCatch + Me.ResultsByFleet(eSpaceResultsFleets.CatchBio, iFleet, it)
        Next
        endCatch = endCatch / nts

    End Sub


    ''' <summary>
    ''' Get Cost by Fleet for summary periods
    ''' </summary>
    ''' <param name="EcopathCost">Cost from Ecopath actual cost in Ecopath dollars for one unit of Ecopath fishing</param>
    ''' <remarks>Cost is computed from values saved over time because of the was it's calculated</remarks>
    Public Sub getSumCostFleet(ByVal EcopathCost(,) As Single, ByVal iFleet As Integer, ByRef startCost As Single, ByRef endCost As Single)
        Dim st As Integer, et As Integer, nts As Integer
        Dim sSailEffort As Single, eSailEffort As Single
        Dim sFishEffort As Single, eFishEffort As Single
        startCost = 0
        endCost = 0

        'get the start and end time indexes and number of time steps to sum over
        'getStartEndSumIndex() will figure out the one based indexes
        Me.getStartEndSumIndex(st, et, nts)

        'eSpaceResultsFleets.SailingEffort and FishingEffort are spatially averaged cEcospace.accumCatchData() and me.AverageSpatialResults()
        For it As Integer = st To st + nts - 1
            sSailEffort += Me.ResultsByFleet(eSpaceResultsFleets.SailingEffort, iFleet, it)
            sFishEffort += Me.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iFleet, it)
        Next
        'in EwE5 Effort is averaged over time steps
        'sailing effort is not
        sFishEffort = sFishEffort / nts

        For it As Integer = et To et + nts - 1
            eSailEffort += Me.ResultsByFleet(eSpaceResultsFleets.SailingEffort, iFleet, it)
            eFishEffort += Me.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iFleet, it)
        Next
        eFishEffort = eFishEffort / nts

        'cost = [fixed cost] + ([fishing effort] * [ecopath effort cost] + [sailing effort] * [ecopath sailing cost])
        startCost = EcopathCost(iFleet, 1) + (sFishEffort * EcopathCost(iFleet, 2) + sSailEffort * EcopathCost(iFleet, 3))
        endCost = EcopathCost(iFleet, 1) + (eFishEffort * EcopathCost(iFleet, 2) + eSailEffort * EcopathCost(iFleet, 3))

        'Console.WriteLine("Effort Fleet = " & iFleet.ToString & ", Start = " & sFishEffort.ToString & ", End = " & eFishEffort.ToString)
        'Console.WriteLine("Sail Fleet = " & iFleet.ToString & ", Start = " & sSailEffort.ToString & ", End = " & eSailEffort.ToString)

    End Sub



    ''' <summary>
    ''' Get Value by Fleet for summary periods
    ''' </summary>
    Public Sub getSumValueFleet(ByVal iFleet As Integer, ByRef startValue As Single, ByRef endValue As Single)
        Dim st As Integer, et As Integer, nts As Integer
        startValue = 0
        endValue = 0

        'get the start and end time indexes and number of time steps to sum over
        'getStartEndSumIndex() will figure out the one based indexes
        Me.getStartEndSumIndex(st, et, nts)

        For it As Integer = st To st + nts - 1
            startValue = startValue + Me.ResultsByFleet(eSpaceResultsFleets.Value, iFleet, it)
        Next
        startValue = startValue / nts

        For it As Integer = et To et + nts - 1
            endValue = endValue + Me.ResultsByFleet(eSpaceResultsFleets.Value, iFleet, it)
        Next
        endValue = endValue / nts

    End Sub


    ''' <summary>
    ''' Get Value by Fleet for summary periods
    ''' </summary>
    Public Sub getSumEffortES(ByVal iFleet As Integer, ByRef EndoverStart As Single)
        Dim st As Integer, et As Integer, nts As Integer
        Dim s As Single, e As Single
        'get the start and end time indexes and number of time steps to sum over
        'getStartEndSumIndex() will figure out the one based indexes
        Me.getStartEndSumIndex(st, et, nts)

        For it As Integer = st To st + nts - 1
            s = s + Me.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iFleet, it)
        Next
        s = s / nts

        For it As Integer = et To et + nts - 1
            e = e + Me.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iFleet, it)
        Next
        e = e / nts

        If s = 0 Then s = 1
        EndoverStart = e / s

    End Sub


    ''' <summary>
    ''' Get Catch by REgion, Fleet, Group for summary periods
    ''' </summary>
    Public Sub getSumCatchRegionGearGroup(ByVal iRegion As Integer, ByVal iFleet As Integer, ByVal iGroup As Integer, ByRef startCatch As Single, ByRef endCatch As Single)
        Dim st As Integer, et As Integer, nts As Integer
        startCatch = 0
        endCatch = 0

        'get the start and end time indexes and number of time steps to sum over
        'getStartEndSumIndex() will figure out the one based indexes
        Me.getStartEndSumIndex(st, et, nts)

        For it As Integer = st To st + nts - 1
            startCatch = startCatch + Me.ResultsCatchRegionGearGroup(iRegion, iFleet, iGroup, it)
        Next
        startCatch = startCatch / nts

        For it As Integer = et To et + nts - 1
            endCatch = endCatch + Me.ResultsCatchRegionGearGroup(iRegion, iFleet, iGroup, it)
        Next
        endCatch = endCatch / nts

    End Sub


    ''' <summary>
    ''' Average the results values over number of water cells
    ''' </summary>
    Public Sub AverageSpatialResults()
        Dim iflt As Integer, igrp As Integer, it As Integer, ivar As Integer, irgn As Integer
        Dim ncells As Integer
        Try

            For ivar = 0 To [Enum].GetValues(GetType(eSpaceResultsFleets)).Length
                For iflt = 0 To Me.nFleets
                    For it = 1 To nTimeSteps
                        Me.ResultsByFleet(ivar, iflt, it) /= Me.nWaterCells
                    Next it
                Next iflt
            Next ivar

            For ivar = 0 To [Enum].GetValues(GetType(eSpaceResultsFleetsGroups)).Length
                For iflt = 0 To Me.nFleets
                    For igrp = 1 To Me.NGroups
                        For it = 1 To nTimeSteps
                            Me.ResultsByFleetGroup(ivar, iflt, igrp, it) /= Me.nWaterCells
                        Next it
                    Next igrp
                Next iflt
            Next ivar

            For irgn = 0 To Me.nRegions
                ncells = Me.nCellsInRegion(irgn)
                If ncells = 0 Then ncells = 1
                For igrp = 1 To Me.NGroups
                    For it = 1 To nTimeSteps
                        Me.ResultsRegionGroup(irgn, igrp, it) /= ncells
                    Next it
                Next igrp
            Next irgn

            For irgn = 0 To Me.nRegions
                ncells = Me.nCellsInRegion(irgn)
                If ncells = 0 Then ncells = 1
                For iflt = 0 To Me.nFleets
                    For igrp = 1 To Me.NGroups
                        For it = 1 To nTimeSteps
                            Me.ResultsCatchRegionGearGroup(irgn, iflt, igrp, it) /= ncells
                        Next it
                    Next igrp
                Next iflt
            Next irgn

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            cLog.Write(ex)
        End Try

    End Sub


    Public Sub SummarizeResultsByFleet(nTimeSteps As Integer, ByVal EcopathCost(,) As Single, ByVal JobMultiplier() As Single)
        Dim SailEffort As Single, FishEffort As Single
        Dim cost As Single, value As Single

        'Me.nSumTimeSteps = 0
        Debug.Assert(nTimeSteps <= ResultsByFleet.GetUpperBound(2), "EcoSpace summary data time step counter not set correctly!")

        'number of years the model actually ran for, computed in case the model run was stopped by the user
        Dim nYears As Single = CSng(nTimeSteps / (1 / TimeStep))

        ReDim Me.ResultsSummaryByFleet(1, Me.nFleets)

        'All values in ResultsByFleet() have been averaged over space
        For iflt As Integer = 0 To Me.nFleets
            SailEffort = 0
            FishEffort = 0
            value = 0
            For it As Integer = 1 To nTimeSteps
                SailEffort += Me.ResultsByFleet(eSpaceResultsFleets.SailingEffort, iflt, it)
                FishEffort += Me.ResultsByFleet(eSpaceResultsFleets.FishingEffort, iflt, it)
                value += Me.ResultsByFleet(eSpaceResultsFleets.Value, iflt, it)
            Next

            cost = EcopathCost(iflt, 1) + (FishEffort * EcopathCost(iflt, 2) + SailEffort * EcopathCost(iflt, 3))

            'profit average yearly
            ResultsSummaryByFleet(0, iflt) = (value - cost) / nYears
            'jobs average yearly
            ResultsSummaryByFleet(1, iflt) = value * JobMultiplier(iflt) / nYears

        Next

    End Sub


    ''' <summary>
    ''' Get the indexes for the user defined time windows that the results data is summarized over
    ''' </summary>
    ''' <param name="startIndex">Index for the first time window</param>
    ''' <param name="endIndex">Index for the end/last time window</param>
    ''' <param name="nIndexes">Number of time steps the user defined to summarize the data over</param>
    ''' <remarks></remarks>
    Private Sub getStartEndSumIndex(ByRef startIndex As Integer, ByRef endIndex As Integer, ByRef nIndexes As Integer)

        Dim nSteps As Integer = CInt(1.0 / Me.TimeStep)
        startIndex = CInt(Me.SumStart(0) * nSteps) + 1
        endIndex = CInt(Me.SumStart(1) * nSteps) + 1
        If startIndex > Me.nTimeSteps Then startIndex = 1
        If endIndex > Me.nTimeSteps Then endIndex = Me.nTimeSteps - Me.NumStep
        nIndexes = Me.NumStep
    End Sub

    ''' <summary>
    ''' Preserve RelPP map in the <see cref="relPP0"/> temporary array.
    ''' </summary>
    Public Sub setBaseRelPP()
        Me.allocate(Me.RelPP0, InRow + 1, InCol + 1)
        Array.Copy(Me.RelPP, Me.RelPP0, Me.RelPP.Length)
    End Sub

    ''' <summary>
    ''' Restore RelPP map from the <see cref="relPP0"/> temporary array.
    ''' </summary>
    ''' <remarks>
    ''' This will clear the relPP0 temporary array.
    ''' </remarks>
    Public Sub restoreBaseRelPP()
        If (RelPP0 IsNot Nothing) Then
            Array.Copy(Me.RelPP0, Me.RelPP, Me.RelPP.Length)
            Me.RelPP0 = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Hardwire some Capacity map values
    ''' </summary>
    ''' <remarks>FOR DEBUGGING ONLY</remarks>
    Public Sub setDebugCapMaps(ByVal CapEnvResData As cMediationDataStructures)

        Try
            ''set PHabType(,,) to 100% for cells that are loaded as a HabType from the database 
            'For irow As Integer = 1 To InRow
            '    For icol As Integer = 1 To InCol
            '        PHabType(irow, icol, HabType(irow, icol)) = 1
            '    Next icol
            'Next irow

            ''set Input habitat capacity to 1 for all groups 
            'For irow As Integer = 1 To InRow
            '    For icol As Integer = 1 To InCol
            '        For igrp As Integer = 1 To Me.NGroups
            '            Me.HabCapInput(irow, icol, igrp) = 1
            '        Next
            '    Next icol
            'Next irow

        Catch ex As Exception
            Debug.Assert(False, "Failed to init debug capacity map")
        End Try


    End Sub


    ''' <summary>
    ''' Count the number of water cells and sets public property nWaterCells
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function setNWaterCells() As Integer
        Me.nWaterCells = 0
        For i As Integer = 1 To Me.InRow
            For j As Integer = 1 To Me.InCol
                If Me.Depth(i, j) > 0 Then 'Water
                    Me.nWaterCells += 1
                End If
            Next
        Next

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Return the <see cref="cCoreInputOutputBase.DBID">unique database ID</see>
    ''' for any Ecospace map layer.
    ''' </summary>
    ''' <param name="varname">The <see cref="eVarNameFlags"/> of the layer to find the database ID for.</param>
    ''' <param name="iIndex">The <see cref="cCoreInputOutputBase.Index"/> of the layer to find the database ID for.</param>
    ''' <returns>An integer, or <see cref="cCore.NULL_VALUE"/> if the requested
    ''' layer was not found.</returns>
    ''' <remarks>
    ''' This method is robust to any type of abuse; non-registered <paramref name="varname">variables</paramref>
    ''' and <paramref name="iIndex">indexes</paramref> are dealt with properly.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Public Function getLayerID(varname As eVarNameFlags, iIndex As Integer) As Integer
        Dim arr As Integer() = Me.getLayerIDs(varname)
        If ((iIndex < 0) Or (iIndex >= arr.Length)) Then Return cCore.NULL_VALUE
        Return arr(iIndex)
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Obtain a layer DBID for any varname and index.
    ''' </summary>
    ''' <param name="varname"></param>
    ''' <remarks>
    ''' This method is robust to any type of abuse; non-registered <paramref name="varname">variables</paramref>
    ''' are dealt with properly.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Public ReadOnly Property getLayerIDs(varname As eVarNameFlags) As Integer()
        Get
            Select Case varname
                Case eVarNameFlags.LayerBiomassForcing : Return Me.GroupDBID
                Case eVarNameFlags.LayerBiomassRelativeForcing : Return Me.GroupDBID
                Case eVarNameFlags.LayerDriver : Return Me.EnvironmentalLayerDBID
                Case eVarNameFlags.LayerHabitat : Return Me.HabitatDBID
                Case eVarNameFlags.LayerHabitatCapacity : Return Me.GroupDBID
                Case eVarNameFlags.LayerHabitatCapacityInput : Return Me.GroupDBID
                Case eVarNameFlags.LayerImportance : Return Me.ImportanceLayerDBID
                Case eVarNameFlags.LayerMigration : Return Me.GroupDBID
                Case eVarNameFlags.LayerMPA : Return Me.MPADBID
                Case eVarNameFlags.LayerPort : Return Me.FleetDBID
                Case eVarNameFlags.LayerSail : Return Me.FleetDBID
                Case eVarNameFlags.LayerAdvection : Return New Integer() {0, 1, 2}
            End Select
            Return New Integer() {0, 1}
        End Get
    End Property

#End Region

    ''' <summary>Equator length in km.</summary>
    ''' <remarks>http://en.wikipedia.org/wiki/Equator#Exact_length_of_the_Equator</remarks>
    Friend Shared c_sEquatorLength As Single = 40007.862917

    Friend Shared ReadOnly Property DegreeToKm() As Single
        Get
            Return c_sEquatorLength / 360.0!
        End Get
    End Property

    Public Shared Function ToCellSize(ByVal sCellLength As Single, ByVal bAssumeSquareCells As Boolean) As Single
        If (bAssumeSquareCells) Then
            Return sCellLength * 1000.0!
        End If
        Return sCellLength / DegreeToKm
    End Function

    Public Shared Function ToCellLength(ByVal sCellSize As Single, ByVal bAssumeSquareCells As Boolean) As Single
        If (bAssumeSquareCells) Then
            Return sCellSize / 1000.0!
        End If
        Return sCellSize * DegreeToKm
    End Function

End Class



