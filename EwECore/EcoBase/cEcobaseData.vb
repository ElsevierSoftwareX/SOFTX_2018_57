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
Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports EwEUtils.Core
Imports EwEUtils.NetUtilities
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace WebServices.Ecobase

#Region " Model "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing the data for model parameters.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cModelData

        Public Enum eSubmissionType As Integer
            [New] = 0
            Replacement = 1
            Derived = 2
        End Enum

#Region " Variables "

        ''' <summary>Ecobase ID.</summary>
        <XmlElement("model_number")> _
        Public Property EcobaseCode As String = ""

        <XmlElement("model_name")> _
        Public Property Name As String = ""

        <XmlElement("description")> _
        Public Property Description As String = ""

        <XmlElement("author")> _
        Public Property Author As String = ""

        <XmlElement("contact")> _
        Public Property Contact As String = ""

        <XmlElement("num_digits")> _
        Public Property NumDigits As Integer = 3

        <XmlElement("model_year")> _
        Public Property FirstYear As Integer = 0

        <XmlElement("model_period")> _
        Public Property NumYears As Integer = 1

        <XmlElement("country")> _
        Public Property Country As String = ""

        ''' <summary>Area size.</summary>
        <XmlElement("area")> _
        Public Property Area As Single = 0

        ''' <summary>Northern limit of the <see cref="Extent"/>.</summary>
        <XmlIgnore()> _
        Public Property North As Single = cCore.NULL_VALUE

        ''' <summary>Eastern limit of the <see cref="Extent"/>.</summary>
        <XmlIgnore()> _
        Public Property East As Single = cCore.NULL_VALUE

        ''' <summary>Western limit of the <see cref="Extent"/>.</summary>
        <XmlIgnore()> _
        Public Property West As Single = cCore.NULL_VALUE

        ''' <summary>Southern limit of the <see cref="Extent"/>.</summary>
        <XmlIgnore()> _
        Public Property South As Single = cCore.NULL_VALUE

        ''' <summary>Spatial bounding box.</summary>
        <XmlElement("geographic_extent")> _
        Public Property Extent As String
            Get
                Return "BOX(" & cStringUtils.FormatSingle(Me.West) & " " & cStringUtils.FormatSingle(Me.North) & "," & cStringUtils.FormatSingle(Me.East) & " " & cStringUtils.FormatSingle(Me.South) & ")"
            End Get
            Set(value As String)
                Dim astrBits() As String = value.ToUpper().Replace("BOX(", "").Replace(",", " ").Replace(")", "").Trim().Split(" "c)
                If (astrBits.Length > 3) Then
                    Single.TryParse(astrBits(0), Me.West)
                    Single.TryParse(astrBits(1), Me.North)
                    Single.TryParse(astrBits(2), Me.East)
                    Single.TryParse(astrBits(3), Me.South)
                End If
            End Set
        End Property

        ''' <summary>Ecosystem type.</summary>
        <XmlElement("ecosystem_type")> _
        Public Property EcosystemType As String = ""

        ''' <summary>Currency unit.</summary>
        <XmlElement("currency_units")> _
        Public Property UnitCurrency As String = ""

        ''' <summary>Flag, stating if currency unit is a custom unit.</summary>
        <XmlElement("currency_units_custom")> _
        Public Property UnitCurrencyIsCustom As Boolean

        ''' <summary>Flag, stating if Ecobase has the right to make model parameters available for download.</summary>
        <XmlElement("dissemination_allow")> _
        Public Property AllowDissemination As Boolean = False

        ''' <summary>The digitial object identifier (doi) of the publication for this model.</summary>
        <XmlElement("doi")> _
        Public Property DOI As String = ""

        ''' <summary>The URI to the publication for this model.</summary>
        <XmlElement("url")> _
        Public Property URI As String = ""

        ''' <summary>The reference of the publication for this model.</summary>
        <XmlElement("reference")> _
        Public Property Reference As String = ""

        ''' <summary>EwE version</summary>
        <XmlElement("ewe_version")> _
        Public Property EwEVersion As String = ""

        ''' <summary>Flag, stating if the model matches the paper version.</summary>
        <XmlElement("match_paper")> _
        Public Property ModelMatchesPaper As Boolean = False

        ''' <summary></summary>
        <XmlElement("temperature_mean")> _
        Public Property TempMean As Single = 0
        ''' <summary></summary>
        <XmlElement("temperature_min")> _
        Public Property TempMin As Single = 0
        ''' <summary></summary>
        <XmlElement("temperature_max")> _
        Public Property TempMax As Single = 0

        ''' <summary></summary>
        <XmlElement("depth_mean")> _
        Public Property DepthMean As Single = 0
        ''' <summary></summary>
        <XmlElement("depth_min")> _
        Public Property DepthMin As Single = 0
        ''' <summary></summary>
        <XmlElement("depth_max")> _
        Public Property DepthMax As Single = 0

        ''' <summary>Is Ecosim used?</summary>
        <XmlElement("ecosim")> _
        Public Property EcosimUsed As Boolean = False

        ''' <summary>Is Ecospace used?</summary>
        <XmlElement("ecospace")> _
        Public Property EcospaceUsed As Boolean = False

        <XmlElement("is_fitted")> _
        Public Property IsFittedToTimeSeries As Boolean = False

        ''' <summary>Is the entire foodweb accounted for?</summary>
        <XmlElement("whole_food_web")> _
        Public Property IsWholeFoodWeb As Boolean = False

        ''' <summary>Comments if there is difference between model used for the references and model upload</summary>
        <XmlElement("comments_difference")> _
        Public Property CommentsDifference As String

        ''' <summary>Comments if model is not declared as open access.</summary>
        <XmlElement("comments_access")> _
        Public Property CommentsAccess As String

        <XmlElement("fisheries")> _
        Public Property ObjectiveFisheries As Boolean
        <XmlElement("aquaculture")> _
        Public Property ObjectiveAquaculture As Boolean
        <XmlElement("environment_variability")> _
        Public Property ObjectiveEnvironmentalVariability As Boolean
        <XmlElement("ecosyst_functioning")> _
        Public Property ObjectiveEcosystemFunctioning As Boolean
        <XmlElement("pollution")> _
        Public Property ObjectivePollution As Boolean
        <XmlElement("mpa")> _
        Public Property ObjectiveMarineProtection As Boolean
        <XmlElement("other_impact_assessment")> _
        Public Property ObjectiveOtherImpactAssessment As Boolean
        ''' <summary>Description of objectives of the model.</summary>
        <XmlElement("comments_objectives")> _
        Public Property Objectives As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>Set as <see cref="eSubmissionType"/></remarks>
        <XmlElement("submission_type")> _
        Public Property SubmissionType As Integer

        ''' <summary>
        ''' Linked / updated EcoBase model
        ''' </summary>
        <XmlElement("modification_child")> _
        Public Property SubmissionLink As String

        <XmlElement("modification_comments")> _
        Public Property SubmissionComments As String

#End Region ' Variables

#Region " Construction "

        Public Sub New()
            ' NOP
        End Sub

        Public Sub New(core As cCore)

            Dim ecopathDS As cEcopathDataStructures = core.m_EcoPathData
            Dim taxonDS As cTaxonDataStructures = core.m_TaxonData

            Me.Name = ecopathDS.ModelName
            Me.Description = ecopathDS.ModelDescription
            Me.EcobaseCode = ecopathDS.ModelEcobaseCode

            Me.Author = ecopathDS.ModelAuthor
            Me.Contact = ecopathDS.ModelContact

            Me.FirstYear = ecopathDS.FirstYear
            Me.NumYears = ecopathDS.NumYears

            Me.Area = ecopathDS.ModelArea

            Me.North = ecopathDS.ModelNorth
            Me.East = ecopathDS.ModelEast
            Me.West = ecopathDS.ModelWest
            Me.South = ecopathDS.ModelSouth

            Me.DOI = ecopathDS.ModelPublicationDOI
            Me.URI = ecopathDS.ModelPublicationURI
            Me.Reference = ecopathDS.ModelPublicationRef

            Me.UnitCurrencyIsCustom = Not String.IsNullOrWhiteSpace(ecopathDS.ModelUnitCurrencyCustom)
            Me.UnitCurrency = If(Me.UnitCurrencyIsCustom,
                                               ecopathDS.ModelUnitCurrencyCustom,
                                               DirectCast(ecopathDS.ModelUnitCurrency, eUnitCurrencyType).ToString())

            Me.EcosystemType = ecopathDS.ModelEcosystemType
            Me.Country = ecopathDS.ModelCountry

            Me.DepthMin = 0
            Me.DepthMean = 0
            Me.DepthMax = 0

            Me.TempMin = 0
            Me.TempMean = 0
            Me.TempMax = 0

            Me.EwEVersion = cCore.Version

        End Sub

#End Region ' Construction "

    End Class

#End Region ' Model

#Region " Groups "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing all data for a single group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cGroupData

#Region " Variables "

        <XmlElement("group_seq")>
        Public Property Index As Integer
        <XmlElement("group_name")>
        Public Property Name() As String
        <XmlElement("group_color")>
        Public Property Color As Integer

        ' -- Basic inputs --

        <XmlElement("habitat_area")>
        Public Property Area As Single
        <XmlElement("biomass")>
        Public Property B As Single
        ''' <summary>Biomass in habitat area</summary>
        <XmlElement("biomass_habitat_area")>
        Public Property BH As Single
        <XmlElement("b_hab_area_input")>
        Public Property BHIsInput As Boolean

        <XmlElement("pb")>
        Public Property PB As Single
        <XmlElement("pb_input")>
        Public Property PBIsInput As Boolean

        <XmlElement("qb")>
        Public Property QB As Single
        <XmlElement("qb_input")>
        Public Property QBIsInput As Boolean

        <XmlElement("ee")>
        Public Property EE As Single
        <XmlElement("ee_input")>
        Public Property EEIsInput As Boolean

        <XmlElement("ge")>
        Public Property GE As Single
        <XmlElement("ge_input")>
        Public Property GEIsInput As Boolean

        <XmlElement("detritus_import")>
        Public Property DtImp As Single
        <XmlElement("export")>
        Public Property Ex As Single
        <XmlElement("pp")>
        Public Property PP As Single
        <XmlElement("gs")>
        Public Property GS As Single
        <XmlElement("biomass_accum_rate_input")>
        Public BAIsInput As Boolean
        <XmlElement("biomass_accum")>
        Public Property BA As Single
        <XmlElement("biomass_accum_rate")>
        Public Property BaBi As Single
        <XmlElement("respiration")>
        Public Property Respiration As Single
        <XmlElement("immigration")>
        Public Property Immig As Single
        <XmlElement("emigration")>
        Public Property Emig As Single
        <XmlElement("emigration_rate")>
        Public Property EmigRate As Single
        ''' <summary>Non-market price</summary>
        <XmlElement("shadow_price")>
        Public Property Shadow As Single
        <XmlElement("other_mort")>
        Public Property OtherMort As Single
        <XmlElement("other_mort_rate")>
        Public Property MortCoOtherMort As Single
        <XmlElement("vbk")>
        Public Property vbK As Single

        ' -- Fields exclusively for the benefit of Ecotroph --
        <XmlElement("tl")>
        Public Property TL As Single
        <XmlElement("oi")>
        Public Property OmnivoryIndex As Single
        <XmlElement("flow_to_det")>
        Public Property FlowToDet As Single
        <XmlElement("net_efficiency")>
        Public Property NetEfficiency As Single
        <XmlElement("fish_mort_rate")>
        Public Property MortCoFishRate As Single
        <XmlElement("pred_mort_rate")>
        Public Property MortCoPredMort As Single
        <XmlElement("net_migration_rate")>
        Public Property MortCoNetMig As Single

        ' Diets
        <XmlArray("diet_descr")>
        <XmlArrayItem("diet")>
        Public Property Diets As New List(Of cDietData)
        <XmlElement("diet_imp")>
        Public Property ImpVar As Single

        ' Pedigree
        <XmlArray("pedigree_assignment_descr")>
        <XmlArrayItem("pedigree_assignment")>
        Public Property PedigreeAssignments As New List(Of cPedigreeAssignmentData)

        ' Taon
        <XmlArray("taxon_descr")>
        <XmlArrayItem("taxon")>
        Public Property Taxa As New List(Of cTaxonData)

#End Region ' Variables

#Region " Constructor "

        Public Sub New()
            ' NOP
        End Sub

        ''' <summary>
        ''' Constructor, initializes an instance with model data for submitting to EcoBase.
        ''' </summary>
        ''' <param name="core">The core to obtain data from.</param>
        ''' <param name="iGroup"></param>
        Public Sub New(core As cCore, iGroup As Integer)

            ' Sanity checks
            Debug.Assert(core IsNot Nothing)
            Debug.Assert(core.StateMonitor.HasEcopathRan)
            Debug.Assert(iGroup <= core.nGroups)

            Dim ecopathDS As cEcopathDataStructures = core.m_EcoPathData
            Dim taxonDS As cTaxonDataStructures = core.m_TaxonData

            Me.Index = iGroup
            Me.Name = ecopathDS.GroupName(iGroup)
            Me.PP = ecopathDS.PP(iGroup)
            Me.Area = ecopathDS.Area(iGroup)
            Me.BA = ecopathDS.BAInput(iGroup)
            Me.BAIsInput = (ecopathDS.BAInput(iGroup) = ecopathDS.BA(iGroup))
            Me.BaBi = ecopathDS.BaBi(iGroup)
            Me.GS = ecopathDS.GS(iGroup)
            Me.DtImp = ecopathDS.DtImp(iGroup)
            Me.Ex = ecopathDS.Ex(iGroup)
            Me.ImpVar = ecopathDS.DCInput(iGroup, 0)
            'drow("GroupIsFish") = ecopathDS.GroupIsFish(iGroup)
            'drow("GroupIsInvert") = ecopathDS.GroupIsInvert(iGroup)
            Me.Shadow = ecopathDS.Shadow(iGroup)
            Me.Respiration = ecopathDS.Resp(iGroup)
            Me.OtherMort = ecopathDS.OtherMortinput(iGroup)

            Me.BHIsInput = (ecopathDS.Binput(iGroup) >= 0)
            Me.B = If(Me.BHIsInput, ecopathDS.Binput(iGroup), ecopathDS.B(iGroup))
            Me.BH = ecopathDS.Binput(iGroup) / ecopathDS.Area(iGroup)

            Me.EEIsInput = (ecopathDS.EEinput(iGroup) >= 0)
            Me.EE = If(Me.EEIsInput, ecopathDS.EEinput(iGroup), ecopathDS.EE(iGroup))

            Me.PBIsInput = (ecopathDS.PBinput(iGroup) >= 0)
            Me.PB = If(Me.PBIsInput, ecopathDS.PBinput(iGroup), ecopathDS.PB(iGroup))

            Me.QBIsInput = (ecopathDS.QBinput(iGroup) >= 0)
            Me.QB = If(Me.QBIsInput, ecopathDS.QBinput(iGroup), ecopathDS.QB(iGroup))

            Me.GEIsInput = (ecopathDS.GEinput(iGroup) >= 0)
            Me.GE = If(Me.GEIsInput, ecopathDS.GEinput(iGroup), ecopathDS.GE(iGroup))

            Me.Immig = ecopathDS.Immig(iGroup)
            Me.Emig = ecopathDS.Emigration(iGroup)
            Me.EmigRate = ecopathDS.Emig(iGroup)
            Me.Color = ecopathDS.GroupColor(iGroup)
            Me.vbK = ecopathDS.vbK(iGroup)

            Dim grpOut As cEcoPathGroupOutput = core.EcoPathGroupOutputs(iGroup)
            Me.TL = grpOut.TTLX
            Me.OmnivoryIndex = grpOut.OmnivoryIndex
            Me.BaBi = grpOut.BioAccumRatePerYear
            Me.FlowToDet = grpOut.FlowToDet
            Me.NetEfficiency = grpOut.NetEfficiency
            Me.MortCoFishRate = grpOut.MortCoFishRate
            Me.MortCoPredMort = grpOut.MortCoPredMort
            Me.MortCoNetMig = grpOut.MortCoNetMig
            Me.MortCoOtherMort = grpOut.MortCoOtherMort

            'PSD
            'drow("Tcatch") = psdDS.Tcatch(iGroup)
            'drow("AinLW") = psdDS.AinLWInput(iGroup)
            'drow("BinLW") = psdDS.BinLWInput(iGroup)
            'drow("Loo") = psdDS.LooInput(iGroup)
            'drow("Winf") = psdDS.WinfInput(iGroup)
            'drow("t0") = psdDS.t0Input(iGroup)
            'drow("Tmax") = psdDS.TmaxInput(iGroup)

            Me.Diets.Clear()
            For iPrey As Integer = 1 To core.nGroups
                Dim dc As Single = ecopathDS.DCInput(iGroup, iPrey)
                Dim df As Single = 0
                If (iPrey > core.nLivingGroups) Then df = ecopathDS.DF(iGroup, iPrey - core.nLivingGroups)
                If ((dc + df) > 0) Then
                    Dim diet As New cDietData(iPrey, dc, df)
                    Me.Diets.Add(diet)
                End If
            Next

            Me.Taxa.Clear()
            For iTaxon As Integer = 1 To taxonDS.NumTaxon
                If (taxonDS.IsTaxonStanza(iTaxon) = False) And (taxonDS.TaxonTarget(iTaxon) = iGroup) Then
                    Me.Taxa.Add(New cTaxonData(core, iTaxon))
                End If
            Next

            Me.PedigreeAssignments.Clear()
            For iVar As Integer = 1 To ecopathDS.NumPedigreeVariables
                If (ecopathDS.PedigreeEcopathGroupCV(iGroup, iVar) > 0) Then
                    Me.PedigreeAssignments.Add(New cPedigreeAssignmentData(ecopathDS.PedigreeVariables(iVar), ecopathDS.PedigreeEcopathGroupCV(iGroup, iVar)))
                End If
            Next

        End Sub

#End Region ' Constructor

#Region " Public properties "

        Public ReadOnly Property NumPedigreeAssignments As Integer
            Get
                Return Me.PedigreeAssignments.Count
            End Get
        End Property

#End Region ' Public properties

    End Class

#End Region ' Groups

#Region " Diets "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing a single diet for a predator.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cDietData

#Region " Variables "

        <XmlElement("prey_seq")> _
        Public Property PreyIndex As Integer

        <XmlElement("proportion")> _
        Public Property Amount As Single

        <XmlElement("detritus_fate")> _
        Public Property DetritusFate As Single

#End Region ' Variables

#Region " Constructor "

        Public Sub New()
            ' NOP
        End Sub

        Public Sub New(iPrey As Integer, amount As Single, detritusfate As Single)
            Me.New()
            Me.PreyIndex = iPrey
            Me.Amount = amount
            Me.DetritusFate = detritusfate
        End Sub

#End Region ' Constructor

    End Class

#End Region ' Diets

#Region " Fleets "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing the data for a single fleet.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cFleetData

#Region " Variables "

        ''' <summary>Sequential, one-based index of a fleet.</summary>
        <XmlElement("fleet_seq")> _
        Public Property Index As Integer = 0

        ''' <summary>Name of a fleet.</summary>
        <XmlElement("fleet_name")> _
        Public Property Name() As String

        <XmlElement("fleet_color")> _
        Public Property Color() As Integer

        <XmlElement("fixed_cost")> _
        Public Property FixedCost As Single

        <XmlElement("sailing_cost")> _
        Public Property SailCost As Single

        <XmlElement("variable_cost")> _
        Public Property VarCost As Single

        <XmlArray("catch_descr")> _
        <XmlArrayItem("catch")> _
        Public Property Catches As New List(Of cCatchData)

        <XmlArray("discard_fate_descr")> _
        <XmlArrayItem("discard_fate")> _
        Public Property DiscardFates As New List(Of cDiscardFateData)

#End Region ' Variables

#Region " Construction "

        Public Sub New()
            ' NOP
        End Sub

        Public Sub New(core As cCore, iFleet As Integer)

            Dim ecopathDS As cEcopathDataStructures = core.m_EcoPathData

            Me.Index = iFleet
            Me.Name = ecopathDS.FleetName(iFleet)
            Me.Color = ecopathDS.FleetColor(iFleet)
            Me.FixedCost = ecopathDS.CostPct(iFleet, eCostIndex.Fixed)
            Me.SailCost = ecopathDS.CostPct(iFleet, eCostIndex.Sail)
            Me.VarCost = ecopathDS.CostPct(iFleet, eCostIndex.CUPE)

            ' Catches and landings
            Me.Catches.Clear()
            ' Discard fate
            Me.DiscardFates.Clear()

            For iGroup As Integer = 1 To core.nGroups
                If (ecopathDS.Landing(iFleet, iGroup) > 0) Then
                    Me.Catches.Add(New cCatchData(iGroup, ecopathDS.Landing(iFleet, iGroup), cCatchData.eCatchType.Landing))
                End If
                If (ecopathDS.Discard(iFleet, iGroup) > 0) Then
                    Me.Catches.Add(New cCatchData(iGroup, ecopathDS.Discard(iFleet, iGroup), cCatchData.eCatchType.Discards))
                End If
                If (ecopathDS.Market(iFleet, iGroup) > 0) Then
                    Me.Catches.Add(New cCatchData(iGroup, ecopathDS.Market(iFleet, iGroup), cCatchData.eCatchType.Market))
                End If
                If (ecopathDS.PropDiscardMort(iFleet, iGroup) > 0) Then
                    Me.Catches.Add(New cCatchData(iGroup, ecopathDS.PropDiscardMort(iFleet, iGroup), cCatchData.eCatchType.PropDiscardMort))
                End If
                If (iGroup > core.nLivingGroups) Then
                    If (ecopathDS.DiscardFate(iFleet, iGroup - core.nLivingGroups) > 0) Then
                        Me.DiscardFates.Add(New cDiscardFateData(iGroup - core.nLivingGroups, ecopathDS.DiscardFate(iFleet, iGroup - core.nLivingGroups)))
                    End If
                End If
            Next

        End Sub

#End Region ' Construction

#Region " Public properties "

        Public ReadOnly Property NumCatches As Integer
            Get
                Return Me.Catches.Count
            End Get
        End Property

        Public ReadOnly Property NumDiscardFate As Integer
            Get
                Return Me.DiscardFates.Count
            End Get
        End Property

#End Region ' Public properties

    End Class

#End Region ' Fleets

#Region " Catches "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing a single catch for a group.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cCatchData

        Public Enum eCatchType As Byte
            Landing
            Discards
            Market
            PropDiscardMort
        End Enum

#Region " Variables "

        ''' <summary>Name of the fleet the catch belongs to.</summary>
        <XmlElement("group_seq")> _
        Public Property GroupIndex As Integer

        ''' <summary>Catch value.</summary>
        <XmlElement("catch_value")> _
        Public Property Amount As Single

        <XmlElement("catch_type")> _
        Public Property Type As String

        ''' <summary>Interpreted <see cref="eCatchType">value</see>.</summary>
        <XmlIgnore()> _
        Public Property CatchType As eCatchType
            Get
                Select Case Me.Type.ToLower()
                    Case "total landings" : Return eCatchType.Landing
                    Case "discards" : Return eCatchType.Discards
                    Case "market" : Return eCatchType.Market
                    Case "prop mort" : Return eCatchType.PropDiscardMort
                    Case Else
                        Debug.Assert(False, "Enumerated value " & Me.Type & " not supported")
                End Select
                Return eCatchType.Discards
            End Get
            Set(ByVal value As eCatchType)
                Select Case value
                    Case eCatchType.Discards : Me.Type = "discards"
                    Case eCatchType.Landing : Me.Type = "total landings"
                    Case eCatchType.Market : Me.Type = "market"
                    Case eCatchType.PropDiscardMort : Me.Type = "prop mort"
                    Case Else
                        Debug.Assert(False, "Enumerated value " & value & " not supported")
                End Select
            End Set
        End Property

#End Region ' Variables

#Region " Construction "

        Public Sub New()
            'NOP
        End Sub

        Public Sub New(iGroup As Integer, amount As Single, type As eCatchType)
            Me.GroupIndex = iGroup
            Me.Amount = amount
            Me.CatchType = type
        End Sub

#End Region ' Construction

    End Class

#End Region ' Catches

#Region " Discard fate "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing a discard fate for a fleet/group combination.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cDiscardFateData

#Region " Variables "

        ''' <summary>Name of the fleet the catch belongs to.</summary>
        <XmlElement("group_seq")> _
        Public Property GroupIndex As Integer
        <XmlElement("amount")> _
        Public Property Amount As Single

#End Region ' Variables

#Region " Construction "

        Public Sub New()
            'NOP
        End Sub

        Public Sub New(iGroup As Integer, amount As Single)
            Me.GroupIndex = iGroup
            Me.Amount = amount
        End Sub

#End Region ' Construction

    End Class

#End Region ' Catches

#Region " Multi-stanza "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing the data for a single fleet.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cStanzaData

#Region " Variables "

        ''' <summary>Sequential, one-based index of a stanza.</summary>
        <XmlElement("stanza_seq")> _
        Public Property Index As Integer = 0

        ''' <summary>Name of a stanza.</summary>
        <XmlElement("stanza_name")> _
        Public Property Name() As String

        <XmlArray("lifestage_descr")> _
        <XmlArrayItem("lifestage")> _
        Public Property LifeStages As New List(Of cStanzaLifeStageData)

        <XmlElement("leading_b")> _
        Public Property LeadingB As Integer
        <XmlElement("leading_qb")> _
        Public Property LeadingQB As Integer
        <XmlElement("rec_power")> _
        Public Property RecPower As Single
        <XmlElement("bab_split")> _
        Public Property BaBSplit As Single
        <XmlElement("w_mat_w_inf")> _
        Public Property WmatWinf As Single
        <XmlElement("fixed_fecundity")> _
        Public Property FixedFecundity As Boolean
        <XmlElement("egg_at_spawn")> _
        Public Property EggAtSpawn As Boolean

        ' Taxon
        <XmlArray("taxon_descr")> _
        <XmlArrayItem("taxon")> _
        Public Property Taxonomy As New List(Of cTaxonData)

#End Region ' Variables

#Region " Construction "

        Public Sub New()
            ' NOP
        End Sub

        Public Sub New(ByVal core As cCore, ByVal iStanza As Integer)

            Dim stanzads As cStanzaDatastructures = core.m_Stanza
            Dim taxonDS As cTaxonDataStructures = core.m_TaxonData

            Me.Index = iStanza
            Me.Name = stanzads.StanzaName(iStanza)

            Me.RecPower = stanzads.RecPowerSplit(iStanza)
            Me.BaBSplit = stanzads.BABsplit(iStanza)
            Me.WmatWinf = stanzads.WmatWinf(iStanza)
            Me.FixedFecundity = stanzads.FixedFecundity(iStanza)
            Me.EggAtSpawn = stanzads.EggAtSpawn(iStanza)

            Me.LeadingB = stanzads.BaseStanza(iStanza)
            Me.LeadingQB = stanzads.BaseStanzaCB(iStanza)

            Me.LifeStages.Clear()
            For iStage As Integer = 1 To stanzads.Nstanza(iStanza)
                Me.LifeStages.Add(New cStanzaLifeStageData(core, iStanza, iStage))
            Next

            Me.Taxonomy.Clear()
            For iTaxon As Integer = 1 To taxonDS.NumTaxon
                If (taxonDS.IsTaxonStanza(iTaxon) = True) And (taxonDS.TaxonTarget(iTaxon) = iStanza) Then
                    Debug.Assert(Me.Taxonomy.Count = 0)
                    Me.Taxonomy.Add(New cTaxonData(core, iTaxon))
                End If
            Next

        End Sub

#End Region ' Construction

#Region " Public properties "

        Public ReadOnly Property NumLifeStages As Integer
            Get
                Return Me.LifeStages.Count
            End Get
        End Property

#End Region ' Public properties

    End Class

#End Region ' Multi-stanza

#Region " Multi-stanza life stage "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing the data for a single multi-stanza life stage.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cStanzaLifeStageData

#Region " Variables "

        ''' <summary>Sequential, one-based index of a stanza.</summary>
        <XmlElement("stage_seq")> _
        Public Property Index As Integer = 0
        <XmlElement("group_seq")> _
        Public Property GroupIndex As Integer = 0
        <XmlElement("z")> _
        Public Property Z As Single
        <XmlElement("start_age")> _
        Public Property Age As Integer

#End Region ' Variables

#Region " Construction "

        Public Sub New()
            ' NOP
        End Sub

        Public Sub New(ByVal core As cCore, ByVal iStanza As Integer, ByVal iLifeStage As Integer)

            Dim stanzaDS As cStanzaDatastructures = core.m_Stanza

            Me.Index = iLifeStage
            Me.GroupIndex = stanzaDS.EcopathCode(iStanza, iLifeStage)
            Me.Z = stanzaDS.Stanza_Z(iStanza, iLifeStage)
            Me.Age = stanzaDS.Age1(iStanza, iLifeStage)

        End Sub

#End Region ' Construction

    End Class

#End Region ' Multi-stanza

#Region " Pedigree "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Class for containing a single pedigree level.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cPedigreeData

#Region " Variables "

        ''' <summary>Index of the pedigree level.</summary>
        <XmlElement("pedigree_seq")> _
        Public Property Index As Integer
        <XmlElement("pedigree_name")> _
        Public Property Name As String
        <XmlElement("description")> _
        Public Property Description As String
        <XmlElement("pedigree_color")> _
        Public Property Color As Integer

        <XmlElement("variable")> _
        Public Property Variable As String

        <XmlIgnore()> _
        Public Property VarName As eVarNameFlags
            Get
                Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
                Return cin.GetVarName(Me.Variable)
            End Get
            Set(value As eVarNameFlags)
                Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
                Me.Variable = cin.GetVarName(value)
            End Set
        End Property

        <XmlElement("index_value")> _
        Public Property IndexValue As Single
        <XmlElement("conf_interv")> _
        Public Property ConfidenceValue As Integer
        <XmlElement("estimated")> _
        Public Property IsEstimated As Boolean

#End Region ' Variables

#Region " Construction "

        Public Sub New()
            'NOP
        End Sub

        Public Sub New(ByVal core As cCore, iLevel As Integer)

            Dim ecopathDS As cEcopathDataStructures = core.m_EcoPathData

            Me.Index = iLevel
            Me.Name = ecopathDS.PedigreeLevelName(iLevel)
            Me.Description = ecopathDS.PedigreeLevelDescription(iLevel)
            Me.Color = ecopathDS.PedigreeLevelColor(iLevel)
            Me.VarName = ecopathDS.PedigreeLevelVarName(iLevel)
            Me.IndexValue = ecopathDS.PedigreeLevelIndexValue(iLevel)
            Me.ConfidenceValue = ecopathDS.PedigreeLevelConfidence(iLevel)
            Me.IsEstimated = ecopathDS.PedigreeLevelEstimated(iLevel)

        End Sub

#End Region ' Construction

    End Class

#End Region ' Pedigree

#Region " Pedigree assignments "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Class for containing a single pedigree level.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cPedigreeAssignmentData

#Region " Variables "

        ''' <summary>Index of the pedigree level.</summary>
        <XmlElement("pedigree_seq")> _
        Public Property LevelIndex As Integer

        <XmlElement("variable")> _
        Public Property Variable As String

        <XmlIgnore()> _
        Public Property VarName As eVarNameFlags
            Get
                Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
                Return cin.GetVarName(Me.Variable)
            End Get
            Set(value As eVarNameFlags)
                Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
                Me.Variable = cin.GetVarName(value)
            End Set
        End Property

#End Region ' Variables

#Region " Construction "

        Public Sub New()
            'NOP
        End Sub

        Public Sub New(ByVal var As eVarNameFlags, ByVal iLevel As Integer)

            Me.VarName = var
            Me.LevelIndex = iLevel

        End Sub

#End Region ' Construction

    End Class

#End Region ' Pedigree assignments

#Region " Taxa "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Class for containing a single species.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cTaxonData

#Region " Variables "

        ''' <summary>Index of the taxon.</summary>
        <XmlElement("taxon_seq")> _
        Public Property TaxonIndex As Integer
        <XmlElement("taxon_name")> _
        Public Property CommonName As String
        ''' <summary>Reserved for future use.</summary>
        <XmlElement("taxon_kingdom")> _
        Public Property Kingdom As String
        ''' <summary>Reserved for future use.</summary>
        <XmlElement("taxon_phylum")> _
        Public Property Phylum As String
        <XmlElement("taxon_class")> _
        Public Property [Class] As String
        <XmlElement("taxon_order")> _
        Public Property Order As String
        <XmlElement("taxon_family")> _
        Public Property Family As String
        <XmlElement("taxon_genus")> _
        Public Property Genus As String
        <XmlElement("taxon_species")> _
        Public Property Species As String

        ''' <summary>See <see cref="eVarNameFlags.CodeSAUP"></see></summary>
        <XmlElement("code_saup")> _
        Public Property CodeSAUP As Long
        ''' <summary>See <see cref="eVarNameFlags.CodeFB"></see></summary>
        <XmlElement("code_fishbase")> _
        Public Property CodeFB As Long
        ''' <summary>See <see cref="eVarNameFlags.CodeSLB"></see></summary>
        <XmlElement("code_sealifebase")> _
        Public Property CodeSLB As Long
        ''' <summary>See <see cref="eVarNameFlags.CodeFAO"></see></summary>
        <XmlElement("code_fao")> _
        Public Property CodeFAO As String
        ''' <summary>See <see cref="eVarNameFlags.CodeLSID"></see></summary>
        <XmlElement("code_lsid")> _
        Public Property CodeLSID As String

        <XmlElement("source")> _
        Public Property Source As String
        <XmlElement("source_key")> _
        Public Property SourceKey As String

        ''' <summary>Northern limit of the <see cref="Extent"/>.</summary>
        <XmlIgnore()> _
        Public Property North As Single
        ''' <summary>Eastern limit of the <see cref="Extent"/>.</summary>
        <XmlIgnore()> _
        Public Property East As Single
        ''' <summary>Western limit of the <see cref="Extent"/>.</summary>
        <XmlIgnore()> _
        Public Property West As Single
        ''' <summary>Southern limit of the <see cref="Extent"/>.</summary>
        <XmlIgnore()> _
        Public Property South As Single

        ''' <summary>Spatial bounding box.</summary>
        <XmlElement("geographic_extent")> _
        Public Property Extent As String
            Get
                Return "BOX(" & cStringUtils.FormatSingle(Me.West) & " " & cStringUtils.FormatSingle(Me.North) & "," & cStringUtils.FormatSingle(Me.East) & " " & cStringUtils.FormatSingle(Me.South) & ")"
            End Get
            Set(value As String)
                Dim strBits() As String = value.ToUpper().Replace("BOX(", "").Replace(",", " ").Replace(")", "").Trim().Split(" "c)
                Me.West = Single.Parse(strBits(0))
                Me.North = Single.Parse(strBits(1))
                Me.East = Single.Parse(strBits(2))
                Me.South = Single.Parse(strBits(3))
            End Set
        End Property

        <XmlElement("prop_biomass")> _
        Public Property PropBiomass As Single

        <XmlElement("prop_catch")> _
        Public Property PropCatch As Single

        ' -- Ecology type --

        <XmlElement("type_ecology")> _
        Public Property Ecology As String

        <XmlIgnore()> _
        Public Property EcologyType As eEcologyTypes
            Get
                Dim t As eEcologyTypes = eEcologyTypes.NotSet
                [Enum].TryParse(Me.Ecology, t)
                Return t
            End Get
            Set(value As eEcologyTypes)
                Me.Ecology = value.ToString
            End Set
        End Property

        ' -- Organism type --

        <XmlElement("type_organism")> _
        Public Property Organism As String

        <XmlIgnore()> _
        Public Property OrganismType As eOrganismTypes
            Get
                Dim t As eOrganismTypes = eOrganismTypes.NotSet
                [Enum].TryParse(Me.Organism, t)
                Return t
            End Get
            Set(value As eOrganismTypes)
                Me.Organism = value.ToString
            End Set
        End Property

        ' -- IUCN status --

        <XmlElement("iucn_status")> _
        Public Property IUCNConservationStatus As String

        <XmlIgnore()> _
        Public Property IUCNConservationStatusType As eIUCNConservationStatusTypes
            Get
                Dim t As eIUCNConservationStatusTypes = eIUCNConservationStatusTypes.NotSet
                [Enum].TryParse(Me.IUCNConservationStatus, t)
                Return t
            End Get
            Set(value As eIUCNConservationStatusTypes)
                Me.IUCNConservationStatus = value.ToString
            End Set
        End Property

        ' -- Exploitation type --

        <XmlElement("type_exploitation")> _
        Public Property ExploitationStatus As String

        <XmlIgnore()> _
        Public Property ExploitationStatusType As eExploitationTypes
            Get
                Dim t As eExploitationTypes = eExploitationTypes.NotSet
                [Enum].TryParse(Me.ExploitationStatus, t)
                Return t
            End Get
            Set(value As eExploitationTypes)
                Me.ExploitationStatus = value.ToString
            End Set
        End Property

        ' -- Occurrence --

        <XmlElement("type_occurrence")> _
        Public Property OccurrenceStatus As String

        <XmlIgnore()> _
        Public Property OccurrenceStatusType As eOccurrenceStatusTypes
            Get
                Dim t As eOccurrenceStatusTypes = eOccurrenceStatusTypes.NotSet
                [Enum].TryParse(Me.OccurrenceStatus, t)
                Return t
            End Get
            Set(value As eOccurrenceStatusTypes)
                Me.OccurrenceStatus = value.ToString
            End Set
        End Property

        <XmlElement("vulnerability_index")> _
        Public Property VulnerabilityIndex As Integer
        <XmlElement("weight_mean")> _
        Public Property MeanWeight As Single
        <XmlElement("length_mean")> _
        Public Property MeanLength As Single
        <XmlElement("length_max")> _
        Public Property MaxLength As Single
        <XmlElement("lifespan_mean")> _
        Public Property MeanLifeSpan As Single
        <XmlElement("weight_at_inf")> _
        Public Property Winf As Single
        <XmlElement("vbk")> _
        Public Property vbk As Single

#End Region ' Variables

#Region " Construction "

        Public Sub New()
            'NOP
        End Sub

        Public Sub New(ByVal core As cCore, iTaxon As Integer)

            Dim taxonDS As cTaxonDataStructures = core.m_TaxonData

            Me.TaxonIndex = iTaxon
            Me.CommonName = taxonDS.TaxonName(iTaxon)

            Me.Kingdom = ""
            Me.Phylum = ""
            Me.Class = taxonDS.TaxonClass(iTaxon)
            Me.Order = taxonDS.TaxonOrder(iTaxon)
            Me.Family = taxonDS.TaxonFamily(iTaxon)
            Me.Genus = taxonDS.TaxonGenus(iTaxon)
            Me.Species = taxonDS.TaxonSpecies(iTaxon)

            Me.CodeSAUP = taxonDS.TaxonCodeSAUP(iTaxon)
            Me.CodeFB = taxonDS.TaxonCodeFB(iTaxon)
            Me.CodeSLB = taxonDS.TaxonCodeSLB(iTaxon)
            Me.CodeFAO = taxonDS.TaxonCodeFAO(iTaxon)
            Me.CodeLSID = taxonDS.TaxonCodeLSID(iTaxon)

            Me.Source = taxonDS.TaxonSource(iTaxon)
            Me.SourceKey = taxonDS.TaxonSourceKey(iTaxon)
            Me.North = taxonDS.TaxonNorth(iTaxon)
            Me.West = taxonDS.TaxonWest(iTaxon)
            Me.South = taxonDS.TaxonSouth(iTaxon)
            Me.East = taxonDS.TaxonEast(iTaxon)

            Me.PropBiomass = taxonDS.TaxonPropBiomass(iTaxon)
            Me.PropCatch = taxonDS.TaxonPropCatch(iTaxon)

            Me.EcologyType = taxonDS.TaxonEcologyType(iTaxon)
            Me.OrganismType = taxonDS.TaxonOrganismType(iTaxon)
            Me.IUCNConservationStatusType = taxonDS.TaxonIUCNConservationStatus(iTaxon)
            Me.ExploitationStatusType = taxonDS.TaxonExploitationStatus(iTaxon)
            Me.OccurrenceStatusType = taxonDS.TaxonOccurrenceStatus(iTaxon)

            Me.VulnerabilityIndex = taxonDS.TaxonVulnerabilityIndex(iTaxon)
            Me.MeanWeight = taxonDS.TaxonMeanWeight(iTaxon)
            Me.MeanLength = taxonDS.TaxonMeanLength(iTaxon)
            Me.MaxLength = taxonDS.TaxonMaxLength(iTaxon)
            Me.MeanLifeSpan = taxonDS.TaxonMeanLifeSpan(iTaxon)
            Me.Winf = taxonDS.TaxonWinf(iTaxon)
            Me.vbk = taxonDS.TaxonK(iTaxon)

        End Sub

#End Region ' Construction

    End Class

#End Region ' Taxa

#Region " cEcobaseModelParameters "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing the data for a single model, as received from
    ''' EcoBase
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <XmlRoot("EcoBaseModel")> _
    Public Class cEcobaseModelParameters

#Region " Variables "

        ''' <summary>The <see cref="cModelData"/>.</summary>
        <XmlElement("model_descr")> _
        Public Property Model As cModelData

        ''' <summary>The list of <see cref="cGroupData">groups</see>.</summary>
        <XmlArray("group_descr")> _
        <XmlArrayItem("group")> _
        Public Groups As New List(Of cGroupData)

        ''' <summary>The list of <see cref="cFleetData">fleets</see>.</summary>
        <XmlArray("fleet_descr")> _
        <XmlArrayItem("fleet")> _
        Public Fleets As New List(Of cFleetData)

        ''' <summary>The list of <see cref="cStanzaData">multi-stanza groups</see>.</summary>
        <XmlArray("stanza_descr")> _
        <XmlArrayItem("stanza")> _
        Public Stanzas As New List(Of cStanzaData)

        ''' <summary>The list of <see cref="cPedigreeData">pedigree levels</see>.</summary>
        <XmlArray("pedigree_descr")> _
        <XmlArrayItem("pedigree")> _
        Public PedigreeLevels As New List(Of cPedigreeData)

#End Region ' Variables

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Default contructor
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New()
            ' NOP
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, loads an instance from the currently loaded model.
        ''' </summary>
        ''' <param name="core">The core that has the loaded model.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(core As cCore)

            ' Sanity checks
            Debug.Assert(core.StateMonitor.HasEcopathLoaded(), "Ecopath not loaded, cannot continue")
            Debug.Assert(core.IsModelBalanced(), "Ecopath not balanced, cannot continue")

            Me.Model = New cModelData(core)

            For iGroup As Integer = 1 To core.nGroups
                Me.Groups.Add(New cGroupData(core, iGroup))
            Next

            For iFleet As Integer = 1 To core.nFleets
                Me.Fleets.Add(New cFleetData(core, iFleet))
            Next

            For iStanza As Integer = 1 To core.nStanzas
                Me.Stanzas.Add(New cStanzaData(core, iStanza))
            Next

            For iPedigree As Integer = 1 To core.m_EcoPathData.NumPedigreeLevels
                Me.PedigreeLevels.Add(New cPedigreeData(core, iPedigree))
            Next

        End Sub

#End Region ' Construction

#Region " Shared access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Factory method, create a cEcobaseData instance from WSDL output.
        ''' </summary>
        ''' <param name="strModel"></param>
        ''' <returns>A cEcobaseData instance, or nothing if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FromXML(strModel As String) As cEcobaseModelParameters

            ' Clean up
            If (String.IsNullOrWhiteSpace(strModel)) Then Return Nothing

#If DEBUG Then
            ' Store original XML for debugging purposes
            Using w As New StreamWriter("EcobaseModelParameters_org.xml")
                w.Write(strModel)
                w.Close()
            End Using
#End If

            'strModel = strModel.Replace(""" & vbLF && """, "")
            strModel = strModel.Replace("ETinputtot", "cEcobaseModelParameters")

            Dim reader As New StringReader(strModel)
            Dim serializer As New XmlSerializer(GetType(cEcobaseModelParameters))
            Dim selfie As cEcobaseModelParameters = Nothing

            Try
                selfie = CType(serializer.Deserialize(reader), cEcobaseModelParameters)
            Catch ex As Exception
                ' Hmm
                cLog.Write(ex, "cEcobaseModelParameters.FromXML")
            End Try

            If (selfie.Model Is Nothing) Then Return Nothing

#If DEBUG Then
            ' Store cleaned XML for debugging purposes
            Dim doc As New Xml.XmlDocument()
            doc.LoadXml(strModel)
            doc.Save("EcobaseModelParameters_in.xml")
            ' Store processed XML for debugging purposes
            doc.LoadXml(cEcobaseModelParameters.ToXML(selfie))
            doc.Save("EcobaseModelParameters_processed.xml")
#End If
            Return selfie

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a cEcobaseData instance to a chunk of XML for submission to EcoBase
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToXML(data As cEcobaseModelParameters) As String

            Dim writerText As New cFlexibleEncodingStringWriter()
            writerText.CustomEncoding = System.Text.Encoding.UTF8
            Dim writerXML As XmlWriter = XmlWriter.Create(writerText)
            Dim serializer As New XmlSerializer(GetType(cEcobaseModelParameters))
            serializer.Serialize(writerXML, data)
            Return writerText.ToString()

        End Function

#End Region ' Shared access

#Region " Public properties "

        Public ReadOnly Property NumGroups As Integer
            Get
                Return Me.Groups.Count
            End Get
        End Property

        Public ReadOnly Property NumLiving As Integer
            Get
                Dim iNumLiving As Integer = 0
                For Each gd As cGroupData In Me.Groups
                    If gd.PP < 2 Then iNumLiving += 1
                Next
                Return iNumLiving
            End Get
        End Property

        Public ReadOnly Property NumFleets As Integer
            Get
                Return Me.Fleets.Count
            End Get
        End Property

        Public ReadOnly Property NumStanza As Integer
            Get
                Return Me.Stanzas.Count
            End Get
        End Property

        Public ReadOnly Property NumPedigree As Integer
            Get
                Return Me.PedigreeLevels.Count
            End Get
        End Property

#End Region ' Public properties

    End Class

#End Region ' cEcobaseModelParameters

#Region " cEcobaseModelList "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for containing a list of models received from EcoBase.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <XmlRoot("EcoBaseModels")> _
    Public Class cEcobaseModelList

#Region " Variables "

        ''' <summary>The list of <see cref="cModelData"/> for all models in EcoBase.</summary>
        <XmlArray("model_descr")> _
        <XmlArrayItem("model")> _
        Public Property Models As New List(Of cModelData)

#End Region ' Variables

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Deafult hidden constructor.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub New()
            ' NOP
        End Sub

#End Region ' Construction

#Region " Shared access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Factory method, create a cEcobaseData instance from WSDL output.
        ''' </summary>
        ''' <param name="strModelsList">Models list XML from EcoBase.</param>
        ''' <returns>A cEcobaseData instance, or nothing if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FromXML(strModelsList As String) As cEcobaseModelList

            ' Clean up
            If (String.IsNullOrWhiteSpace(strModelsList)) Then Return Nothing

            'strModelsList = strModelsList.Replace("<ETinputtot>", "<?xml version=""1.0"" encoding=""utf-8""?><ETinputtot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">")
            'strModelsList = strModelsList.Replace("ETinputtot", "EcoBaseModels")
            'strModelsList = strModelsList.Replace(""" & vbLF && """, "")

#If DEBUG Then
            ' Store original XML for debugging purposes
            Using w As New StreamWriter("EcobaseModelList_org.xml")
                w.Write(strModelsList)
                w.Close()
            End Using
#End If

            Dim reader As New StringReader(strModelsList)
            Dim serializer As New XmlSerializer(GetType(cEcobaseModelList))
            Dim selfie As cEcobaseModelList = Nothing

            Try
                selfie = CType(serializer.Deserialize(reader), cEcobaseModelList)
            Catch ex As Exception
                ' Hmm
                cLog.Write(ex, "cEcobaseModelList.FromXML")
            End Try

#If DEBUG Then
            ' Store cleaned XML for debugging purposes
            Dim doc As New Xml.XmlDocument()
            doc.LoadXml(strModelsList)
            doc.Save("EcobaseModelList_in.xml")
#End If
            Return selfie

        End Function

#End Region ' Shared access

#Region " Public properties "

        Public ReadOnly Property NumModels As Integer
            Get
                Return Me.Models.Count
            End Get
        End Property

#End Region ' Public properties

    End Class

#End Region ' cEcobaseModelList

End Namespace
