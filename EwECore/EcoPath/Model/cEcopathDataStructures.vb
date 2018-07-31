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

Option Strict Off ' OUCH
Imports EwEUtils.Extensions
Imports EwEUtils.Core

''' <summary>
''' Wrapper for the underlying data structures of the EcoPath model. 
''' Provides a way to wrap all the data from EcoPath into one place
''' </summary>
Public Class cEcopathDataStructures

#Region " Private data "

    Private m_messages As cMessagePublisher

#End Region ' Private data

#Region " Public Variables "

    Public bInitialized As Boolean = False

    Public ModelDBID As Integer = 0
    Public ModelName As String = ""
    Public ModelDescription As String = ""
    Public ModelArea As Single = 0
    Public ModelNumDigits As Integer = 0
    Public ModelGroupDigits As Boolean = False
    Public ModelUnitTime As eUnitTimeType = 0
    Public ModelUnitTimeCustom As String = ""
    ''' <summary>Index of current selected currency units.</summary>
    Public ModelUnitCurrency As Integer = eUnitCurrencyType.WetWeight
    Public ModelUnitCurrencyCustom As String = ""
    Public ModelUnitMonetary As String = ""
    Public ModelUnitArea As eUnitAreaType = 0
    Public ModelUnitAreaCustom As String = ""
    Public ModelAuthor As String = ""
    Public ModelContact As String = ""
    Public ModelLastSaved As Double = 0
    Public ModelSouth As Single = 0
    Public ModelNorth As Single = 0
    Public ModelWest As Single = 0
    Public ModelEast As Single = 0
    Public FirstYear As Integer = Date.Now.Year
    Public NumYears As Integer = 1
    Public ModelCountry As String = ""
    Public ModelEcosystemType As String = ""
    ''' <summary>Code of a model in the Ecobase repository, if any.</summary>
    Public ModelEcobaseCode As String = ""
    Public ModelPublicationDOI As String = ""
    Public ModelPublicationURI As String = ""
    Public ModelPublicationRef As String = ""

    ''' <summary>Group names.</summary>
    Public GroupName() As String
    ''' <summary>Group Database ID - uniquely identifies a group.</summary>
    Friend GroupDBID() As Integer

    ''' <summary>Number of Ecosim scenarios available in a loaded model.</summary>
    Public NumEcosimScenarios As Integer
    ''' <summary>Array of Ecosim scenario names.</summary>
    Public EcosimScenarioName() As String
    ''' <summary>Array of Ecosim scenario database IDs.</summary>
    Public EcosimScenarioDBID() As Integer
    ''' <summary>Array of Ecosim scenario descriptions.</summary>
    Public EcosimScenarioDescription() As String
    ''' <summary>Array of Ecosim scenario authors.</summary>
    Public EcosimScenarioAuthor() As String
    ''' <summary>Array of Ecosim scenario contacts.</summary>
    Public EcosimScenarioContact() As String
    ''' <summary>Array of Ecosim scenario save dates (in julian day format).</summary>
    Public EcosimScenarioLastSaved() As Double
    ''' <summary>Index of active Ecosim scenario.</summary>
    Public ActiveEcosimScenario As Integer = cCore.NULL_VALUE

    ''' <summary>Number of Ecospace scenarios available in a loaded model.</summary>
    Public NumEcospaceScenarios As Integer
    ''' <summary>Array of Ecospace scenario names.</summary>
    Public EcospaceScenarioName() As String
    ''' <summary>Array of Ecospace scenario database IDs.</summary>
    Public EcospaceScenarioDBID() As Integer
    ''' <summary>Array of Ecospace scenario descriptions.</summary>
    Public EcospaceScenarioDescription() As String
    ''' <summary>Array of Ecospace scenario authors.</summary>
    Public EcospaceScenarioAuthor() As String
    ''' <summary>Array of Ecospace scenario contacts.</summary>
    Public EcospaceScenarioContact() As String
    ''' <summary>Array of Ecospace scenario save dates (in julian day format).</summary>
    Public EcospaceScenarioLastSaved() As Double
    ''' <summary>Index of active Ecospace scenario.</summary>
    Public ActiveEcospaceScenario As Integer = cCore.NULL_VALUE

    ''' <summary>Number of Ecotracer scenarios available in a loaded model.</summary>
    Public NumEcotracerScenarios As Integer
    ''' <summary>Array of Ecotracer scenario names.</summary>
    Public EcotracerScenarioName() As String
    ''' <summary>Array of Ecotracer scenario database IDs.</summary>
    Public EcotracerScenarioDBID() As Integer
    ''' <summary>Array of Ecotracer scenario descriptions.</summary>
    Public EcotracerScenarioDescription() As String
    ''' <summary>Array of Ecotracer scenario authors.</summary>
    Public EcotracerScenarioAuthor() As String
    ''' <summary>Array of Ecotracer scenario contacts.</summary>
    Public EcotracerScenarioContact() As String
    ''' <summary>Array of Ecotracer scenario save dates (in julian day format).</summary>
    Public EcotracerScenarioLastSaved() As Double
    ''' <summary>Index of active Ecotracer scenario.</summary>
    Public ActiveEcotracerScenario As Integer = cCore.NULL_VALUE

    ''' <summary>Biomass (computed)</summary>
    Public B() As Single
    ''' <summary>Biomass in habitat area (t/km²)</summary>
    Public BH() As Single
    ''' <summary>Biomass accumulation (t/km²/year) as entered by the user</summary>
    Public BAInput() As Single
    ''' <summary>Biomass accumulation / biomass</summary>
    Public BaBi() As Single
    ''' <summary>Biomass accumulation (t/km²/year)</summary>
    Public BA() As Single
    ''' <summary>Production / biomass (/year)</summary>
    Public PB() As Single
    ''' <summary>Consumption / biomass (/year)</summary>
    Public QB() As Single
    ''' <summary>Ecotrophic efficiency (ratio)</summary>
    Public EE() As Single
    ''' <summary>Production / consumption (ratio)</summary>
    ''' <remarks>Fraction of the production that is passed up in the food web.</remarks>
    Public GE() As Single
    ''' <summary>Unassimilation / consumption (ratio)</summary>
    ''' <remarks>Fraction of the food that is not assimilated.</remarks>
    Public GS() As Single

    ''' <summary>Unassimilation / consumption (ratio) for Energy Currency units ONLY</summary>
    ''' <remarks>Fraction of the food that is not assimilated.</remarks>
    Public GSEng() As Single

    'Input Values are user entered values.
    'Inputs are the values that can be edited by a user, get saved to the database and displayed as basic inputs
    'each array will have a companion used for modeling that does not have 'input' i.e. EEinput() and EE() 
    'the input values are copied into the modeling array whenever the ecopath model is run CopyInputToModelArrays(...) 
    'these values are exposed via cEcoPathGroupOutputs

    ''' <summary>Ecotrophic efficiency (ratio) - original user input value of <see cref="EE">EE</see>.</summary>
    Public EEinput() As Single
    ''' <summary>Other mortaility (ratio) - defined as 1-<see cref="EE">EE</see>.</summary>
    Public OtherMortinput() As Single
    ''' <summary>Production / biomass (/year) - original user input of <see cref="PB">PB</see>.</summary>
    Public PBinput() As Single
    ''' <summary>Consumption / biomass (/year) - original user input of <see cref="QB">QB</see>.</summary>
    Public QBinput() As Single
    ''' <summary>Production / consumption (ratio) - original user input of <see cref="GE">GE</see>.</summary>
    Public GEinput() As Single

    ''' <summary>Biomass (input value)- original user input of <see cref="B">B</see>.</summary>
    Public Binput() As Single

    ''' <summary>Biomass habitat area (input value)- original user input of <see cref="BH">BH</see>.</summary>
    Public BHinput() As Single

    Private min_B_QB As Single 'minimum B*QB

    ''' <summary>Total number of groups (living and detritus)</summary>
    Public NumGroups As Integer
    ''' <summary>Total number of living groups.</summary>
    Public NumLiving As Integer
    ''' <summary>Total number of detritus groups.</summary>
    Public NumDetrit As Integer
    ''' <summary>Total number of fleets.</summary>
    Public NumFleet As Integer
    ''' <summary>User-provided name for time units.</summary>
    Public TimeUnitName As String
    ''' <summary>Index of current selected time unit.</summary>
    Public TimeUnitIndex As Integer
    ''' <summary>Flag stating whether diets have been modified since the last time Ecopath has ran.</summary>
    Public DietsModified As Boolean
    Public PProd As Single

    Public DietChanged(,) As Integer

    Public Ex() As Single

    ''' <summary>Sum (per <see cref="NumGroups">NumGroups</see>) of landings + discards.</summary>
    ''' <remarks>Computed in Catch_calculations(). was called Catch but this causes a naming conflict with Try Catch blocks</remarks>
    Public fCatch() As Single '
    ''' <summary>User input matrix for Diet composition(<see cref="NumGroups">Pred</see>, <see cref="NumGroups">Prey</see>) (ratio), a <see cref="NumGroups">NumGroups</see> * <see cref="NumGroups">NumGroups</see>
    ''' matrix of species consumption ratios.</summary>
    Public DCInput(,) As Single
    ''' <summary>Diet composition(per pred, prey) (ratio), a <see cref="NumGroups">NumGroups</see> * <see cref="NumGroups">NumGroups</see>
    ''' matrix of species consumption ratios.</summary>
    Public DC(,) As Single
    ''' <summary>Detritus fate(per <see cref="NumGroups">NumGroups</see>, <see cref="NumDetrit">NumDetrit</see>) (ratio)</summary>
    ''' <remarks>Matrix describing where to direct surplus detritus.</remarks>
    Public DF(,) As Single
    ''' <summary>Area (<see cref="NumGroups">NumGroups</see>)</summary>
    ''' <remarks>Fraction of the Area where a group occurs.</remarks>
    Public Area() As Single
    ''' <summary>Diet (<see cref="NumGroups">pred</see>, <see cref="NumGroups">prey</see>) change flags.</summary>
    Public DCChanged(,) As Boolean         'Diet composition

    Public BQB() As Single
    ''' <summary>All non-usable 'model currency' that leaves the box represented by a group.</summary>
    Public Resp() As Single
    Public PP() As Single           'TM Trophic Mode
    ''' <summary>Detritus flow (#groups + #fleet,#groups + #fleet)</summary>
    Public det(,) As Single
    ''' <summary>Diet Composition of Detritus  for fishery.</summary>
    Public DCDet(,) As Single
    Public DetEaten() As Single                 ' For multiple detritus
    Public DetPassedOn() As Single              ' For multiple detritus
    Public DetPassedProp() As Single              ' For multiple detritus
    ''' <summary>Flow to detritus (x (group + fleet)).</summary>
    Public FlowToDet() As Single
    ''' <summary>Input to detritus (x group).</summary>
    Public InputToDet() As Single

    ''' <summary>Migration into the area covered by the model (t/km²/year)</summary>
    ''' <remarks>Note that migration is not the same as import, refer to the manual for details.</remarks>
    Public Immig() As Single
    ''' <summary>Emigration (per group) out of the area covered by the model (t/km²/year)</summary>
    Public Emigration() As Single
    ''' <summary>Emigration (per group) relative to biomass (ratio)</summary>
    Public Emig() As Single    'relative to biomass, used in Ecosim
    Public Shadow() As Single
    ''' <summary>States which groups are fishes. There is no interface in EwE for this flag, and its function should be replaced by the taxonomy logic</summary>
    Public GroupIsFish() As Boolean
    ''' <summary>States which groups are invertebrates. There is no interface in EwE for this flag, and its function should be replaced by the taxonomy logic</summary>
    Public GroupIsInvert() As Boolean
    Public PropLanded(,) As Single
    ''' <summary>Trophic levels in Ecopath.</summary>
    Public TTLX() As Single
    'Public TLSim() As Single    'These TL's are recalculated for each time step in Ecosim
    Public NumCatchCodes As Integer = 30
    Public CatchCode(,) As Integer
    Public CVpar(,) As Single
    Public M0() As Single
    Public M2() As Single
    Public Path() As Integer
    Public LastComp() As Integer
    '  Public SpeciesCode(,) As Integer '0: Ecopath group no for this stanza, 1: Ecopath no for leading B stanza, 2: Ecopath no for leading QB stanza
    ''' <summary>Detritus import (ratio)</summary>
    Public DtImp() As Single
    Public StanzaGroup() As Boolean 'Dim: numgroups, True if this is a group with stanza's

    'fishing variables
    Public NoGearData As Boolean
    ''' <summary> cost(nFleets,3) '1 is fixed cost, 2 is cost per unit effort, 3 sailing cost </summary>
    Public cost(,) As Single
    Public CostPct(,) As Single

    ''' <summary>Discarded biomass by fleet group </summary>
    ''' <remarks>Includes survival!</remarks>
    Public Discard(,) As Single
    ''' <summary>Fate of discards (by fleet, #detritus)</summary>
    Public DiscardFate(,) As Single
    ''' <summary>Names of fleets.</summary>
    Public FleetName() As String
    ''' <summary>Database IDs per fleet.</summary>
    Friend FleetDBID() As Integer

    ''' <summary>Landinged biomass (by fleet,group)</summary>
    Public Landing(,) As Single
    ''' <summary>Market value of landings (by fleet,group)</summary>
    Public Market(,) As Single
    ''' <summary>Proportion of total catch that are discards (by fleet, group)</summary>
    ''' <remarks>This is proportion of the total catch that are discarded. Including mortality and survivals.</remarks>
    Public PropDiscard(,) As Single
    ''' <summary>Proportion of regulated discards that die (by fleet, group)</summary>
    Public PropDiscardMort(,) As Single ' gear group 0-1


    Public RTZ As Single 'sum of respiration
    Public Consum As Single
    Public SumBio As Single
    ''' <summary>Sum of catch.</summary>
    Public CatchSum As Single
    ''' <summary>Gross efficiency.</summary>
    Public GEff As Single
    Public Totpp As Single
    ''' <summary>Tropic level of the catch.</summary>
    Public TLcatch As Single
    ''' <summary>Total flow of detritus</summary>
    Public Dt As Single
    ''' <summary>Sum of exports.</summary>
    Public SumEx As Single
    ''' <summary>Sum of all production.</summary>
    Public SumP As Single
    ''' <summary>Connectance Index.</summary>
    Public Conn As Single
    Public SysOm As Single
    Public LandingValue As Single
    Public ShadowValue As Single
    Public Fixed As Single
    Public Variab As Single

    ''' <summary>VBGF curvature parameter K (/year).</summary>
    Public vbK() As Single
    Public Hlap(,) As Single
    Public Plap(,) As Single
    ''' <summary>Colours for groups in an interface (x group).</summary>
    Public GroupColor() As Integer
    ''' <summary>Colours for fleets in an interface (x fleet).</summary>
    Public FleetColor() As Integer
    Public Host(,) As Single  'last is for fishery (combined only)

    ' -- Pedigree

    Public NumPedigreeLevels As Integer
    Public PedigreeLevelDBID() As Integer
    Public PedigreeLevelName() As String
    Public PedigreeLevelColor() As Integer
    Public PedigreeLevelDescription() As String
    Public PedigreeLevelVarName() As eVarNameFlags
    ''' <summary>Index value expressed in ratio [0, 1]</summary>
    Public PedigreeLevelIndexValue() As Single
    ''' <summary>Confidence interval expressed in rounded percentages</summary>
    Public PedigreeLevelConfidence() As Integer
    Public PedigreeLevelEstimated() As Boolean

    ''' <summary>Array [#groups, #supported vars] = CV.</summary>
    Public PedigreeEcopathGroupCV(,) As Integer
    ''' <summary>Array [#groups, #supported vars] = Level index.</summary>
    Public PedigreeEcopathGroupLevel(,) As Integer
    ''' <summary>One-based array of variables supported by the pedigree system.</summary>
    Public PedigreeVariables As eVarNameFlags() = {eVarNameFlags.NotSet, eVarNameFlags.BiomassAreaInput, eVarNameFlags.PBInput, eVarNameFlags.QBInput, eVarNameFlags.DietComp, eVarNameFlags.TCatchInput}
    ''' <summary>Number of <see cref="PedigreeVariables"/></summary>
    Public NumPedigreeVariables As Integer = Me.PedigreeVariables.Length - 1

    Public PedigreeStatsModelIndex As Single
    Public PedigreeStatsModelCV As Single
    Public PedigreeStatsTStar As Single

    ''' <summary>
    ''' Number of missing variables per groups
    ''' </summary>
    ''' <remarks>These are the variables that need to be computed be Ecopath</remarks>
    Public mis() As Integer

    ''' <summary>
    ''' Is the currently loaded Ecospace model setup for coupling with an external model.
    ''' </summary>
    ''' <remarks>
    ''' Coupling joins Ecospace to an external model that is used to dynamically compute PP or other lower trophic level values. 
    ''' This flag is used to dimension variables during the load of an Ecospace model.
    ''' Stored with the Ecopath data because this needs to be set before an Ecospace scenario is loaded so it can be used for dimensioning.
    ''' </remarks>
    Public isEcospaceModelCoupled As Boolean

    Public isGroupLeadingB() As Boolean
    Public isGroupLeadingCB() As Boolean

    Public DiversityIndexType As eDiversityIndexType = eDiversityIndexType.Shannon
    Public KemptonsQ As Single
    Public Shannon As Single

    ''' <summary>
    ''' Returns the computed diversity index that is selected in <see cref="DiversityIndexType"/>
    ''' </summary>
    Public ReadOnly Property DiversityIndex As Single
        Get
            Select Case Me.DiversityIndexType
                Case eDiversityIndexType.KemptonsQ
                    Return KemptonsQ
                Case eDiversityIndexType.Shannon
                    Return Me.Shannon
                Case Else
                    Debug.Assert(False, "Diversity index type not supported")
            End Select
            Return cCore.NULL_VALUE
        End Get
    End Property

#End Region

#Region " Borrowed from EcoRanger "

    ' Borrowed from EcoRanger for Chesson calculation since this calculation is required
    ' for generating Ecopath output data.
    Public SumR() As Single
    Public Alpha(,) As Single

#End Region ' Borrowed from EcoRanger

#Region "Redimensioning"

    ''' <summary>
    ''' Redim All variables that in EcoPath that have an NGroup dimension
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>This act as a central location to change the number of groups in the EcoPath data</remarks>
    Public Function redimGroups() As Boolean

        Try

            redimGroupVariables() 'just ngroup variables
            RedimFleetVariables(True) 'fleets clear out the values
            Return True

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".redimGroups Error: " & ex.Message)
        End Try


    End Function

    ''' <summary>
    ''' redimension array variables 
    ''' called when a new model is loaded
    ''' </summary>
    ''' <returns></returns>
    ''' True if no error
    ''' <remarks></remarks>
    Public Function redimGroupVariables() As Boolean
        Dim i As Integer, j As Integer
        NumDetrit = NumGroups - NumLiving

        ' EstimateWhat(NumGroups)

        ReDim PB(NumGroups)
        ReDim EE(NumGroups)
        ReDim QB(NumGroups)
        ReDim GE(NumGroups)
        ReDim B(NumGroups)
        ReDim BH(NumGroups)    'habitat biomass

        ReDim GEinput(NumGroups)
        ReDim PBinput(NumGroups)
        ReDim EEinput(NumGroups)
        ReDim OtherMortinput(NumGroups)
        ReDim QBinput(NumGroups)
        ReDim Binput(NumGroups)
        ReDim BHinput(NumGroups)

        ReDim Ex(NumGroups)
        ReDim fCatch(NumGroups)
        ReDim Area(NumGroups)
        For i = 1 To NumGroups
            Area(i) = 1
        Next
        ReDim BAInput(NumGroups)
        ReDim BaBi(NumGroups)
        ReDim BA(NumGroups)
        ReDim DCInput(NumGroups + 1, NumGroups + 1)
        ReDim DC(NumGroups + 1, NumGroups + 1)
        ReDim DCChanged(NumGroups + 1, NumGroups + 1) 'jb added to tell the core which diet comp values where changed
        ReDim PP(NumGroups)
        ReDim GroupName(NumGroups)
        ReDim GroupDBID(NumGroups)
        ReDim GS(NumGroups)
        ReDim TTLX(NumGroups)     'Trophic levels in Ecopath
        'JS 08Jan09: SumDC and LHS were a global scratch variable, changed to local scope
        'ReDim LHS(NumGroups, NumGroups)
        'ReDim SumDC(NumGroups)
        ReDim BQB(NumGroups)

        ReDim Resp(NumGroups)
        ReDim DF(NumGroups, NumGroups - NumLiving)

        ReDim DtImp(NumGroups)
        ReDim DetEaten(NumGroups)
        ReDim DetPassedOn(NumGroups)
        ReDim DetPassedProp(NumGroups)
        ReDim InputToDet(NumGroups)
        ReDim M0(NumGroups)
        ReDim M2(NumGroups)
        ReDim Path(2 * NumGroups + 2)
        ReDim LastComp(2 * NumGroups + 1)
        ReDim Immig(NumGroups)
        ReDim Emigration(NumGroups)
        ReDim Emig(NumGroups)
        ReDim Shadow(NumGroups)
        ReDim GroupIsFish(NumGroups)
        ReDim GroupIsInvert(NumGroups)
        ReDim PropLanded(NumFleet, NumGroups)

        ReDim Host(NumGroups, NumGroups)
        ReDim Hlap(NumGroups, NumGroups)
        ReDim Plap(NumGroups, NumGroups)
        ReDim GroupColor(NumGroups)

        ReDim SumR(NumGroups)
        ReDim Alpha(NumGroups, NumGroups)
        ReDim vbK(NumGroups)

        'ReDim GrpsToShow(NumGroups + NumFleet + 2)

        'For i = 1 To NumGroups + NumFleet
        '    GrpsToShow(i) = True
        'Next

        'For i = NumGroups + NumFleet + 1 To NumGroups + NumFleet + 2
        '    GrpsToShow(i) = False
        'Next

        NumCatchCodes = 30
        ReDim CatchCode(NumCatchCodes, NumGroups)
        ReDim CVpar(5, NumGroups)

        For i = 1 To NumGroups
            For j = 0 To 4
                CVpar(j, i) = 0.1
            Next j
            CVpar(5, i) = 0.05
        Next i

        'Stanzagroup  needed when importing eii files
        ReDim StanzaGroup(NumGroups)

        ReDim mis(NumGroups)

        'is the Ecopath group the leading B or QB for a MultiStanza group
        ReDim isGroupLeadingB(NumGroups)
        ReDim isGroupLeadingCB(NumGroups)

        ' GearVariables(True)
        '   CinfoDeclare()    'The variables for Ecotracer: all using numgroups

        Return True
    End Function


    ''' <summary>
    ''' Redimension all fishing variables
    ''' </summary>
    ''' <param name="NoPreserve">
    ''' A flag to keep the existing values in the arrays 
    ''' True means do NOT keep the original values NO preserve.
    ''' False to KEEP the values.
    ''' </param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function RedimFleetVariables(ByVal NoPreserve As Boolean) As Boolean

        'det() is not saved to database
        ReDim det(NumGroups + NumFleet, NumGroups + NumFleet)
        If NoPreserve Then
            ReDim DCDet(NumGroups - NumLiving, NumFleet)        'Diet composition of detritus
            ReDim FlowToDet(NumGroups + NumFleet)
        Else
            ReDim Preserve DCDet(NumGroups - NumLiving, NumFleet)       'Diet composition of detritus
            ReDim Preserve FlowToDet(NumGroups + NumFleet)
        End If
        'Next in Gear
        ReDim cost(NumFleet, 3)       '1 is fixed cost, 2 is cost per unit effort, 3 sailing cost
        ReDim CostPct(NumFleet, 3)       '1 is fixed cost, 2 is cost per unit effort, 3 sailing cost
        ReDim FleetName(NumFleet + 1)
        ReDim FleetDBID(NumFleet + 1)
        'Next in Catch
        ReDim Landing(NumFleet, NumGroups)
        ReDim Discard(NumFleet, NumGroups)
        ReDim DiscardFate(NumFleet, NumGroups - NumLiving)
        ReDim PropLanded(NumFleet, NumGroups)
        ReDim PropDiscard(NumFleet, NumGroups)
        ReDim PropDiscardMort(NumFleet, NumGroups)
        ReDim Market(NumFleet, NumGroups)
        ReDim FleetColor(NumFleet)

        ' Set default market (off-vessel) prices
        For iFleet As Integer = 1 To NumFleet
            For iGroup As Integer = 1 To NumGroups
                Market(iFleet, iGroup) = 1.0!
                PropDiscardMort(iFleet, iGroup) = 1.0!
            Next iGroup
        Next iFleet

        Return True

    End Function

    Public Sub RedimEcosimScenarios()

        ReDim Me.EcosimScenarioName(Me.NumEcosimScenarios)
        ReDim Me.EcosimScenarioDBID(Me.NumEcosimScenarios)
        ReDim Me.EcosimScenarioDescription(Me.NumEcosimScenarios)
        ReDim Me.EcosimScenarioAuthor(Me.NumEcosimScenarios)
        ReDim Me.EcosimScenarioContact(Me.NumEcosimScenarios)
        ReDim Me.EcosimScenarioLastSaved(Me.NumEcosimScenarios)

        Me.ActiveEcosimScenario = cCore.NULL_VALUE

    End Sub

    Public Sub RedimEcospaceScenarios()

        ReDim Me.EcospaceScenarioName(Me.NumEcospaceScenarios)
        ReDim Me.EcospaceScenarioDBID(Me.NumEcospaceScenarios)
        ReDim Me.EcospaceScenarioDescription(Me.NumEcospaceScenarios)
        ReDim Me.EcospaceScenarioAuthor(Me.NumEcospaceScenarios)
        ReDim Me.EcospaceScenarioContact(Me.NumEcospaceScenarios)
        ReDim Me.EcospaceScenarioLastSaved(Me.NumEcospaceScenarios)

        Me.ActiveEcospaceScenario = cCore.NULL_VALUE

    End Sub

    Public Sub RedimEcotracerScenarios()

        ReDim Me.EcotracerScenarioName(Me.NumEcotracerScenarios)
        ReDim Me.EcotracerScenarioDBID(Me.NumEcotracerScenarios)
        ReDim Me.EcotracerScenarioDescription(Me.NumEcotracerScenarios)
        ReDim Me.EcotracerScenarioAuthor(Me.NumEcotracerScenarios)
        ReDim Me.EcotracerScenarioContact(Me.NumEcotracerScenarios)
        ReDim Me.EcotracerScenarioLastSaved(Me.NumEcotracerScenarios)

        Me.ActiveEcotracerScenario = cCore.NULL_VALUE

    End Sub

    Public Sub RedimPedigree()

        ReDim Me.PedigreeLevelDBID(Me.NumPedigreeLevels)
        ReDim Me.PedigreeLevelName(Me.NumPedigreeLevels)
        ReDim Me.PedigreeLevelColor(Me.NumPedigreeLevels)
        ReDim Me.PedigreeLevelDescription(Me.NumPedigreeLevels)
        ReDim Me.PedigreeLevelVarName(Me.NumPedigreeLevels)
        ReDim Me.PedigreeLevelIndexValue(Me.NumPedigreeLevels)
        ReDim Me.PedigreeLevelConfidence(Me.NumPedigreeLevels)
        ReDim Me.PedigreeLevelEstimated(Me.NumPedigreeLevels)
        ReDim Me.PedigreeEcopathGroupLevel(Me.NumGroups, Me.NumPedigreeVariables)
        ReDim Me.PedigreeEcopathGroupCV(Me.NumGroups, Me.NumPedigreeVariables)

    End Sub

    Public Sub Clear()

        Me.NumGroups = 0
        Me.NumFleet = 0
        Me.NumLiving = 0
        Me.NumDetrit = 0
        Me.NumEcosimScenarios = 0
        Me.NumEcospaceScenarios = 0
        Me.NumEcotracerScenarios = 0

    End Sub

#End Region

#Region "Computed Variables/Stats"

    ''' <summary>
    ''' Central handler for computing anything after an Ecopath model run.
    ''' </summary>
    ''' <returns></returns>
    Public Function onPostEcopathRun(fn As cEcoFunctions) As Boolean

        Try

            UpdateBH()
            Compute_M2_Resp_and_Stats()
            ComputeFisheriesStats()
            Compute_M2_Resp_and_Stats()
            ComputeMoreStats(fn)
            ComputeProfit()
            ComputePedigree()

            Return True

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".PostEcopathUpdate() Error: " & ex.Message)
            Return False
        End Try

    End Function


    ''' <summary>
    '''     Computes 
    '''CatchSum: sum of catch.
    '''GEff: Gross efficiency catch/net p.p..
    '''TLcatch: Mean trophic level of the catch.
    '''Run after the parameters have been estimated.
    ''' </summary>
    ''' <remarks>
    ''' This code was originally at the bottom of ParamEstimate1.
    ''' </remarks>
    Private Sub ComputeFisheriesStats()
        Dim Kount As Single, Total As Single, Mean As Single, IMPT As Single, Consu As Single, TruPut As Single
        Dim prod As Single
        Dim i As Integer, ii As Integer

        'Kount = 0
        'Total = 0
        'Mean = 0
        For i = 1 To NumGroups
            If TTLX(i) <> 0 And B(i) <> 0 Then
                Total = Total + BQB(i) * B(i)
                Mean = Mean + TTLX(i) * B(i)
                Kount = Kount + B(i)
            End If
        Next i

        CatchSum = 0
        IMPT = 0
        Mean = 0
        Consu = 0
        TruPut = 0

        For i = 1 To NumGroups
            CatchSum = CatchSum + Landing(0, i) + Discard(0, i) 'Catch(i)
            If PP(i) = 2 Then              'A detritus box
                IMPT = IMPT + DtImp(i)
            Else
                IMPT = IMPT + DC(i, 0) * QB(i) * B(i)
            End If
            prod = 0
            If QB(i) >= 0 Then
                prod = B(i) * PB(i) * EE(i)
                Consu = Consu + B(i) * QB(i)
            End If
            If PP(i) = 2 Then
                Consu = Consu + Dt
                For ii = 1 To NumGroups
                    prod = prod + B(ii) * QB(ii) * DC(ii, NumGroups)
                Next ii
            End If
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            '            'MOD: VC/ELI 012397
            '            If i > NumLiving And prod < 0 Then GoTo SkipTr
            '            'END MOD
            '            TruPut = TruPut + prod
            '            If QB(i) = 0 Then Mean = Mean + B(i) * PB(i)
            'SkipTr:
            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            'jb Modified to not use the goto statment
            'the original intent was to NOT sum "prod" for non living groups that had negative "prod"
            'so 'TruPut' is the sum of all positive 'prod'
            If (i > NumLiving And prod < 0) = False Then 'GoTo SkipTr
                TruPut = TruPut + prod
                If QB(i) = 0 Then Mean = Mean + B(i) * PB(i)
            End If

        Next i

        'If NumGroups > NumLiving And EX(NumGroups) > 0 Then TruPut = TruPut + EX(NumGroups) '+ BA(NumGroups)
        For i = NumLiving + 1 To NumGroups
            TruPut = TruPut + Ex(i)
        Next
        If Totpp > 0 Then
            GEff = CatchSum / Totpp
        ElseIf PProd > 0 Then
            GEff = CatchSum / PProd
        Else
            GEff = 0
        End If

        If GEff <> 0 Then
            ' TLcatch gives trophic level of the fishery
            Kount = 0 : Total = 0
            For i = 1 To NumGroups
                Kount = Kount + fCatch(i)
                Total = Total + TTLX(i) * fCatch(i)
            Next i
            If Kount > 0 Then
                TLcatch = Total / Kount
            Else
                TLcatch = 0
            End If
        End If

    End Sub

    '''<summary>
    '''Computes the following:
    '''M2(): Predator mortality for group i.
    '''Resp(i): Respiration for group i.
    '''RTZ: sum resp.  
    '''ConSum: sum of consumption.
    '''SumBio: sum of biomass.
    '''min_B_QB: minimum B*QB.
    ''' </summary>
    ''' <param name="bQuiet">Flag to suppress any system warning messages that this method may produce.</param>
    ''' <returns>True if there were all respiration is valied</returns>
    ''' <remarks>
    ''' Was Public Sub ParamEstimate2() in original code
    ''' </remarks>
    Friend Function Compute_M2_Resp_and_Stats(Optional bQuiet As Boolean = False) As Boolean
        Dim Prod As Single
        Dim Consump As Single, UnAssimConsump As Single
        Dim M2Sum As Single
        Dim strMsg As String
        Dim i As Integer, j As Integer
        Dim bRespOK As Boolean = True

        RTZ = 0
        Consum = 0
        SumBio = 0

        For i = 1 To NumGroups
            If i <= NumLiving Then
                SumBio = SumBio + B(i)
                For j = 1 To NumLiving
                    If DC(j, i) > 0 And B(i) > 0 Then M2Sum = M2Sum + B(j) * QB(j) * DC(j, i) / B(i)
                Next j
            End If
            M2(i) = M2Sum
            M2Sum = 0

            If i <= NumLiving Then
                If QB(i) > 0 Then

                    Prod = B(i) * PB(i)
                    Consump = B(i) * QB(i)
                    UnAssimConsump = GS(i) * Consump

                    'sum consumption across all the groups for Ecopath Stats
                    Consum += Consump

                    Resp(i) = Consump - Prod - UnAssimConsump
                    'Respiration = zero if the units are nutrients
                    If Me.areUnitCurrencyNutrients() Then
                        Resp(i) = 0.0F 'Nutrient    
                    End If

                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    'jb 4-Apr-2013 Change to account for Unassimilated consumption
                    'Consum = Consum + B(i) * QB(i)
                    'Prod = EE(i) * B(i) * PB(i) + FlowToDet(i)

                    '' FlowToDet(i) is the total flow to Detritus
                    'If Me.areUnitCurrencyNutrients() Then
                    '    Resp(i) = 0 'Nutrient       B(i) * QB(i) - prod
                    'ElseIf PP(i) < 1 Then
                    '    Resp(i) = B(i) * QB(i) - (1 - PP(i)) * Prod
                    'Else
                    '    Resp(i) = B(i) * QB(i) - Prod
                    'End If
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                Else
                    'Primary producers
                    'vc resp of pp OK  RESP(i) = 0
                    Resp(i) = 0.0F
                End If
            Else
                'Detritus
                'vc resp of detritus OK RESP(i) = 0
                Resp(i) = 0.0F
            End If

            'Sum of respiration across all the groups
            RTZ += Resp(i)
            If Resp(i) < 0 Then bRespOK = False 'pt = 2

        Next i

        'jb min_B_QB was called min
        min_B_QB = 0
        For i = 1 To NumGroups
            If QB(i) > 0 Then
                If min_B_QB = 0 Then min_B_QB = B(i) * QB(i)
                If min_B_QB > B(i) * QB(i) Then min_B_QB = B(i) * QB(i)
            End If
        Next i

        If (bRespOK = False) Then
            If (bQuiet = False) Then
                strMsg = My.Resources.CoreMessages.ECOPATH_NEGATIVE_RESPIR_WARNING
                Me.m_messages.AddMessage(New cMessage(strMsg, eMessageType.ErrorEncountered, eCoreComponentType.EcoPath, eMessageImportance.Warning))
            Else
                Console.WriteLine(My.Resources.CoreMessages.ECOPATH_NEGATIVE_RESPIR_WARNING)
            End If
        End If

        Return bRespOK

    End Function

    ''' <summary>
    ''' Compute
    ''' Conn: Connectance Index.
    ''' SumEx: sum of export.
    ''' SumP: Sum of all production.
    ''' SysOm: System Omnivory Index.
    ''' Shannon: ShannonDiversity  ---- either Shannon
    ''' Kempton: KemptonsQ         ---- or Kempton will be shown
    ''' </summary>
    Private Sub ComputeMoreStats(fn As cEcoFunctions)
        Dim i As Integer, j As Integer, SysOmDen As Single

        For i = 1 To NumLiving
            For j = 1 To NumGroups
                If DC(i, j) > 0 Then Conn = Conn + 1
            Next j
        Next i
        Conn = Conn / (NumLiving) ^ 2  'with detritus

        'system omnivory index
        SysOm = 0
        SysOmDen = 0
        'jb min_B_QB was min 
        'it is set in Compute_M2_Resp_and_Stats()
        For i = 1 To NumLiving
            If B(i) * QB(i) / min_B_QB > 0 Then    ' *** CONSUMERS ONLY
                SysOm = SysOm + Math.Log(B(i) * QB(i) / min_B_QB) * BQB(i)
                SysOmDen = SysOmDen + Math.Log(B(i) * QB(i) / min_B_QB)
            End If
        Next i

        If SysOmDen > 0 Then SysOm = SysOm / SysOmDen

        SumEx = 0
        SumP = 0
        For i = 1 To NumGroups
            SumEx = SumEx + Ex(i)
            If PB(i) > 0 And B(i) > 0 Then SumP = SumP + PB(i) * B(i)
        Next i

        If (fn IsNot Nothing) Then
            Me.KemptonsQ = fn.KemptonsQ(NumLiving, TTLX, B, 0.25)
            Me.Shannon = fn.ShannonDiversityIndex(NumLiving, B)
        End If

    End Sub

    Private Sub ComputeProfit()
        Dim Gear As Integer
        Dim Grp As Integer
        Dim value As Single

        LandingValue = 0
        ShadowValue = 0

        For Grp = 1 To NumGroups
            For Gear = 1 To NumFleet
                value = Landing(Gear, Grp) * Market(Gear, Grp)
                If value > 0 Then LandingValue = LandingValue + value
            Next
            value = Shadow(Grp) * B(Grp)
            If value > 0 Then ShadowValue = ShadowValue + value
        Next

        Fixed = 0
        Variab = 0
        For Gear = 1 To NumFleet
            Fixed = Fixed + cost(Gear, eCostIndex.Fixed)
            Variab = Variab + cost(Gear, eCostIndex.CUPE) + cost(Gear, eCostIndex.Sail)
        Next

    End Sub

    Private Sub ComputePedigree()

        Dim iLevel As Integer = 0
        Dim sTotalIndex As Single = 0
        Dim sTotalCV As Single = 0
        Dim iNumIndex As Integer = 0
        Dim iNumCV As Integer = 0
        Dim group As cEcoPathGroupInput = Nothing
        Dim var As eVarNameFlags = eVarNameFlags.NotSet
        Dim bPedigreeComplete As Boolean = (Me.NumPedigreeLevels > 0)

        For iGroup As Integer = 1 To Me.NumGroups
            ' For all vars
            For iVariable As Integer = 1 To Me.NumPedigreeVariables

                var = Me.PedigreeVariables(iVariable)

                If Me.PP(iGroup) = 1 And (var = eVarNameFlags.PBInput Or var = eVarNameFlags.QBInput) Then
                    'Skip qb for producers
                ElseIf Me.PP(iGroup) = 2 Then
                    'do nothing
                Else
                    Try
                        Dim cv As Integer = Me.PedigreeEcopathGroupCV(iGroup, iVariable)

                        If (cv = 0) Then
                            iLevel = Me.PedigreeEcopathGroupLevel(iGroup, iVariable)
                            sTotalIndex += Me.PedigreeLevelIndexValue(iLevel)
                            iNumIndex += 1
                        Else
                            Dim iBestLevel As Integer = -1
                            Dim iBestCV As Integer = 100

                            For iLevel = 1 To Me.NumPedigreeLevels
                                If Me.PedigreeLevelVarName(iLevel) = var Then
                                    If Me.PedigreeLevelConfidence(iLevel) >= cv Then
                                        If (Me.PedigreeLevelConfidence(iLevel) < iBestCV) Then
                                            iBestCV = Me.PedigreeLevelConfidence(iLevel)
                                            iBestLevel = iLevel
                                        End If
                                    End If
                                End If
                            Next iLevel

                            If iBestLevel < 0 Then
                                bPedigreeComplete = False
                            Else
                                sTotalIndex += Me.PedigreeLevelIndexValue(iBestLevel)
                                iNumIndex += 1
                            End If

                            sTotalCV += cv
                            iNumCV += 1

                        End If

                    Catch ex As Exception

                    End Try
                End If

            Next iVariable
        Next iGroup

        If (iNumIndex = 0 Or Not bPedigreeComplete) Then
            Me.PedigreeStatsModelIndex = cCore.NULL_VALUE
            Me.PedigreeStatsTStar = cCore.NULL_VALUE
        Else
            Dim sVar As Single = sTotalIndex / iNumIndex
            Me.PedigreeStatsModelIndex = sVar
            Me.PedigreeStatsTStar = CSng(sVar * Math.Sqrt(Me.NumLiving - 2) / Math.Sqrt(1 - sVar ^ 2))
        End If

        If (iNumCV = 0 Or sTotalCV = 0) Then
            Me.PedigreeStatsModelCV = cCore.NULL_VALUE
        Else
            Me.PedigreeStatsModelCV = sTotalCV / iNumCV
        End If

    End Sub

    Public Sub DietWasChanged(ByVal pred As Integer, ByVal prey As Integer)
        Dim j As Integer, K As Integer
        Dim FoundPredPrey As Boolean

        If (DietChanged Is Nothing) Then ReDim DietChanged(1, 0)

        ' j = UBound(DietChanged, 2)
        j = DietChanged.GetUpperBound(1)
        For K = 0 To j
            If DietChanged(0, K) = pred And DietChanged(1, K) = prey Then
                FoundPredPrey = True
                Exit For
            End If
        Next

        If FoundPredPrey = False Then
            ReDim Preserve DietChanged(1, j + 1)
            DietChanged(0, j + 1) = pred
            DietChanged(1, j + 1) = prey
        End If

    End Sub

    ''' <summary>
    ''' Copy the Input arrays into the arrays that are used for modeling and model output.
    ''' </summary>
    ''' <returns>True if all the values were copied successfully.</returns>
    ''' <remarks>This is call at the start of an Ecopath model run to copy the input data into the arrays that are used
    ''' for model computations and output. I.e. copies EEinput(NumGroups) into EE(NumGroups). In EwE5 this is called MakeUnknownUnknown </remarks>
    Public Function CopyInputToModelArrays() As Boolean

        'Warning EwE5 also included input variables for BA, Immig, and Emigration 
        'See modEcosSense.MakeUnknownUnknown
        Try
            Binput.CopyTo(B, 0)
            BHinput.CopyTo(BH, 0)
            PBinput.CopyTo(PB, 0)
            QBinput.CopyTo(QB, 0)
            GEinput.CopyTo(GE, 0)
            BAInput.CopyTo(BA, 0)

            ' deal with EE and other mort (1-EE)
            'EEinput.CopyTo(EE, 0)
            For i As Integer = 0 To Me.NumGroups
                If Me.OtherMortinput(i) > 0 Then
                    EE(i) = 1 - Me.OtherMortinput(i)
                Else
                    EE(i) = EEinput(i)
                End If
            Next

            ' copy dc
            For i As Integer = 0 To Me.NumGroups
                For j As Integer = 0 To Me.NumGroups
                    DC(i, j) = DCInput(i, j)
                Next
            Next
            Return True
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Compute missing <see cref="BH">BH</see> (Biomass/Area) values.
    ''' </summary>
    ''' <returns>True if successfully.</returns>
    ''' <remarks>
    ''' EwE5 performed differently here; BH() value was left at its NULL input value,
    ''' and was computed in the interface for display. I hope this doesn't mess anything up.
    ''' </remarks>
    Private Function UpdateBH() As Boolean
        For i As Integer = 1 To NumGroups
            If BH(i) < 0 And B(i) > 0 And Area(i) > 0 Then
                BH(i) = B(i) / Area(i)
            End If
        Next
        Return True
    End Function

    ''' <summary>
    ''' Sums a <see cref="DC">Diet Composition</see> matrix to one. 
    ''' </summary>
    Public Sub SumDCToOne()

        ' For each potential predator
        For iPred As Integer = 1 To NumLiving
            ' Is a consumer?
            If PP(iPred) < 1 Then
                ' #Yes: calc sum
                Dim sDCSum As Single = 0.0
                ' For each of potential prey
                ' ** NOTE THAT THE LOWER BOUND USED HERE IS 0 INSTEAD OF 1! This is to include
                ' ** DC Impoprt in the calculations - which is stored at index 0.
                For iPrey As Integer = 0 To Me.NumGroups
                    ' Add consumption to sum
                    sDCSum += Me.DCInput(iPred, iPrey)
                Next iPrey

                ' Is there predation with a need to recalc?
                If (sDCSum > 0) And (sDCSum <> 1.0) Then
                    ' For each prey
                    ' JS 28Aug15: Rescale imports too!!!
                    For iPrey As Integer = 0 To Me.NumGroups
                        ' Rescale consumption
                        Me.DCInput(iPred, iPrey) = Me.DCInput(iPred, iPrey) / sDCSum
                    Next iPrey
                End If
            End If ' PP < 1
        Next iPred

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether Ecopath is set to use Nutrient-based currency units.
    ''' </summary>
    ''' <returns>True if Ecopath is set to use Nutrient-based currency units.</returns>
    ''' -----------------------------------------------------------------------
    Public Function areUnitCurrencyNutrients() As Boolean
        Return Me.ModelUnitCurrency = eUnitCurrencyType.Nitrogen Or
               Me.ModelUnitCurrency = eUnitCurrencyType.Phosporous Or
               Me.ModelUnitCurrency = eUnitCurrencyType.CustomNutrient
    End Function

#End Region

    ''' <summary>
    ''' Run any post initialization validation
    ''' </summary>
    ''' <remarks>This should only be called from the datasouce once it has populated the Ecopath variables.
    ''' It should not be called by the core in response to an edit because it can alter values in an unknown number of places. 
    ''' The core would need to reload all it's Ecopath data after the call.
    ''' If other logic is need that the core can have access to it should be put in a separate routine and called here. 
    ''' The core can then access the logic via a different interface.
    '''  </remarks>
    Public Sub onPostInitialization()
        Dim igrp As Integer
        Dim bGSWarning As Boolean = False
        'GS = zero if group is Primary producer
        For igrp = 1 To NumGroups
            If PP(igrp) = 1 Then
                GS(igrp) = 0
            End If

            'Constrain GS to a percentage
            'This should not happen in EwE6 
            'This check was in a bunch of places in EwE5 it is centralized here
            'In EwE6 GS() should be constrained by the interface or the importer
            If GS(igrp) > 1 Then
                GS(igrp) = GS(igrp) / 100
                bGSWarning = True
            End If

        Next 'For iGroup As Integer = 1 To NumGroups

        'Make backup copy of GS() 
        'that can be used to swap GS if currUnitIndex is changed
        ReDim Me.GSEng(Me.NumGroups)
        If Me.areUnitCurrencyNutrients Then
            'Model Currency is Nurtient
            'set GSEng(0) to default values
            For igrp = 1 To NumLiving
                GSEng(igrp) = 0.2
                If PP(igrp) = 1 Then
                    GS(igrp) = 0
                End If
            Next

        Else
            'Energy Currency Units
            'Make a copy of the original GS values 
            Array.Copy(GS, GSEng, GS.Length)

        End If

        Try
            If bGSWarning Then
                Dim strmsg As String = My.Resources.CoreMessages.ECOPATH_GS_WARNING
                Me.m_messages.AddMessage(New cMessage(strmsg, eMessageType.ErrorEncountered,
                                                eCoreComponentType.EcoPath, eMessageImportance.Warning))
            End If
        Catch ex As Exception

        End Try

    End Sub

    Friend Sub copyTo(ByRef dest As cEcopathDataStructures, Optional ByVal bRedim As Boolean = True)
        Try
            'variables needed to redim
            dest.NumGroups = NumGroups
            dest.NumFleet = NumFleet
            dest.NumDetrit = NumDetrit
            dest.NumLiving = NumLiving

            If bRedim Then
                dest.redimGroups()
            End If

            dest.bInitialized = bInitialized

            GroupName.CopyTo(dest.GroupName, 0)    'was Specie()
            'GroupDBID.CopyTo(dest.GroupDBID, 0)        'Do not copy IDs!

            dest.NumEcosimScenarios = NumEcosimScenarios
            'EcosimScenarioName.CopyTo(dest.EcosimScenarioName, 0)
            'EcosimScenarioDBID.CopyTo(dest.EcosimScenarioDBID, 0)
            'EcosimScenarioDescription.CopyTo(dest.EcosimScenarioDescription, 0)
            dest.ActiveEcosimScenario = ActiveEcosimScenario

            NumEcospaceScenarios = dest.NumEcospaceScenarios
            'EcospaceScenarioName.CopyTo(dest.EcospaceScenarioName, 0)
            'EcospaceScenarioDBID.CopyTo(dest.EcospaceScenarioDBID, 0)
            'EcospaceScenarioDescription.CopyTo(dest.EcospaceScenarioDescription, 0)
            'ActiveEcospaceScenario = cCore.NULL_VALUE

            B.CopyTo(dest.B, 0)
            BH.CopyTo(dest.BH, 0)
            BA.CopyTo(dest.BA, 0)
            BAInput.CopyTo(dest.BAInput, 0)
            BaBi.CopyTo(dest.BaBi, 0)
            PB.CopyTo(dest.PB, 0)
            QB.CopyTo(dest.QB, 0)
            EE.CopyTo(dest.EE, 0)
            GE.CopyTo(dest.GE, 0)
            GS.CopyTo(dest.GS, 0)
            EEinput.CopyTo(dest.EEinput, 0)
            OtherMortinput.CopyTo(dest.OtherMortinput, 0)
            PBinput.CopyTo(dest.PBinput, 0)
            QBinput.CopyTo(dest.QBinput, 0)
            GEinput.CopyTo(dest.GEinput, 0)

            Binput.CopyTo(dest.Binput, 0)

            BHinput.CopyTo(dest.BHinput, 0)

            'min_B_QB = dest.min_B_QB 'minimum B*QB
            dest.DCInput = DCInput.Clone
            dest.DC = DC.Clone

            'dest.currUnitName = currUnitName
            dest.ModelUnitCurrency = ModelUnitCurrency
            dest.TimeUnitName = TimeUnitName
            dest.TimeUnitIndex = TimeUnitIndex
            dest.DietsModified = DietsModified
            dest.PProd = PProd

            ''''DietChanged.CopyTo(dest.DietChanged, 0)

            Ex.CopyTo(dest.Ex, 0)

            fCatch.CopyTo(dest.fCatch, 0) 'was called Catch but this causes a naming conflict with Try Catch blocks
            Array.Copy(DCInput, dest.DCInput, DCInput.Length)
            dest.DCInput = DCInput.Clone
            dest.DC = DC.Clone
            dest.DF = DF.Clone
            Area.CopyTo(dest.Area, 0)
            dest.DCChanged = DCChanged.Clone

            BQB.CopyTo(dest.BQB, 0)
            Resp.CopyTo(dest.Resp, 0)
            PP.CopyTo(dest.PP, 0)           'TM Trophic Mode
            dest.det = det.Clone
            dest.DCDet = DCDet.Clone                 'Diet Composition of Detritus  for fishery            DetEaten.CopyTo(dest.DetEaten, 0)                 ' For multiple detritus
            DetPassedOn.CopyTo(dest.DetPassedOn, 0)              ' For multiple detritus
            DetPassedProp.CopyTo(dest.DetPassedProp, 0)              ' For multiple detritus
            FlowToDet.CopyTo(dest.FlowToDet, 0)
            InputToDet.CopyTo(dest.InputToDet, 0)
            'JS 08Jan09: SumDC was a global scratch variable, changed to local scope
            'SumDC.CopyTo(dest.SumDC, 0)

            Immig.CopyTo(dest.Immig, 0)
            Emigration.CopyTo(dest.Emigration, 0)
            Emig.CopyTo(dest.Emig, 0)    'relative to biomass, used in Ecosim
            Shadow.CopyTo(dest.Shadow, 0)
            GroupIsFish.CopyTo(dest.GroupIsFish, 0)
            GroupIsInvert.CopyTo(dest.GroupIsInvert, 0)

            dest.NumCatchCodes = NumCatchCodes
            dest.PropLanded = PropLanded.Clone
            TTLX.CopyTo(dest.TTLX, 0)
            'JS 08Jan09: LHS was a global scratch variable, changed to local scope
            'dest.LHS = LHS.Clone
            StanzaGroup.CopyTo(dest.StanzaGroup, 0)
            dest.CatchCode = CatchCode.Clone
            dest.CVpar = CVpar.Clone
            M0.CopyTo(dest.M0, 0)
            M2.CopyTo(dest.M2, 0)
            dest.Path = Path.Clone
            dest.LastComp = LastComp.Clone
            DtImp.CopyTo(dest.DtImp, 0)

            ''fishing(variables)
            dest.NoGearData = NoGearData
            dest.cost = cost.Clone
            dest.CostPct = CostPct.Clone
            dest.Discard = Discard.Clone
            dest.DiscardFate = DiscardFate.Clone
            FleetName.CopyTo(dest.FleetName, 0)
            'FleetDBID.CopyTo(dest.FleetDBID, 0) ' Do NOT copy DBIDs
            dest.Landing = Landing.Clone
            dest.Market = Market.Clone
            dest.PropDiscard = PropDiscard.Clone

            dest.RTZ = RTZ
            dest.Consum = Consum
            dest.SumBio = SumBio
            dest.CatchSum = CatchSum
            dest.GEff = GEff
            dest.Totpp = Totpp
            dest.TLcatch = TLcatch
            dest.Dt = Dt
            dest.SumEx = SumEx
            dest.SumP = SumP
            dest.Conn = Conn
            dest.SysOm = SysOm

            vbK.CopyTo(dest.vbK, 0)
            dest.Hlap = Hlap.Clone
            dest.Plap = Plap.Clone
            GroupColor.CopyTo(dest.GroupColor, 0)
            FleetColor.CopyTo(dest.FleetColor, 0)
            dest.Host = Host.Clone
            mis.CopyTo(dest.mis, 0)

            ' Copy model data
            dest.ModelArea = Me.ModelArea
            dest.ModelAuthor = Me.ModelAuthor
            dest.ModelContact = Me.ModelContact
            dest.ModelDescription = Me.ModelDescription
            dest.ModelEast = Me.ModelEast
            dest.ModelGroupDigits = Me.ModelGroupDigits
            dest.ModelName = ModelName
            dest.ModelNorth = ModelNorth
            dest.ModelNumDigits = Me.ModelNumDigits
            dest.ModelSouth = Me.ModelSouth
            dest.ModelUnitCurrency = Me.ModelUnitCurrency
            dest.ModelUnitCurrencyCustom = Me.ModelUnitCurrencyCustom
            dest.ModelUnitMonetary = Me.ModelUnitMonetary
            dest.ModelUnitTime = Me.ModelUnitTime
            dest.ModelUnitTimeCustom = Me.ModelUnitTimeCustom
            dest.ModelWest = Me.ModelWest
            dest.FirstYear = Me.FirstYear

        Catch ex2 As Exception
            Debug.Assert(False, ex2.Message)
        End Try

    End Sub

    Public Sub New(ByVal CoreMessagePublisher As cMessagePublisher)
        Me.m_messages = CoreMessagePublisher

        'No External coupling of Ecospace by default
        Me.isEcospaceModelCoupled = False

    End Sub

End Class
