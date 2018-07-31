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
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Base class for computing biodiversity indicators.
''' </summary>
''' ---------------------------------------------------------------------------
Public MustInherit Class cIndicators

#Region " Private variables "

    ' --- Core data ---
    ''' <summary>Zhe core</summary>
    Private m_core As cCore = Nothing
    ''' <summary>Ecopath data structures</summary>
    Private m_ecopathDS As cEcopathDataStructures = Nothing
    ''' <summary>Taxonomy data structures</summary>
    Private m_taxonDS As cTaxonDataStructures = Nothing
    ''' <summary>Stanza data structures</summary>
    Private m_stanzaDS As cStanzaDatastructures = Nothing

    ' --- Trophic based ---
    ''' <summary>Tropic level of the catch indicator</summary>
    Private m_sTLC As Single
    ''' <summary>Tropic level of the community indicator</summary>
    Private m_sTLCo As Single
    ''' <summary>Tropic level of the community >= TL2 indicator</summary>
    Private m_sTLCo2 As Single
    ''' <summary>Tropic level of the community > TL3.25 indicator</summary>
    Private m_sTLCo325 As Single
    ''' <summary>MTI indicator</summary>
    Private m_sMTI As Single
    ''' <summary>Tropic level of the community > TL4 indicator</summary>
    Private m_sTLCo4 As Single

    ' --- Biomass ---
    ''' <summary>Total biomass indicator</summary>
    Private m_sTotalB As Single
    ''' <summary>Total commercial biomass indicator</summary>
    Private m_sCommB As Single
    ''' <summary>Total fish biomass indicator</summary>
    Private m_sFishB As Single
    ''' <summary>Total invertebrate biomass indicator</summary>
    Private m_sInveB As Single
    ''' <summary>Total invertebrate over fish biomass indicator</summary>
    Private m_sInveFishB As Single
    ''' <summary>Total predatory biomass > TL4 in the community indicator</summary>
    Private m_sTotalB4 As Single
    ''' <summary>mammals, seabirds and reptiles biomass indicator</summary>
    Private m_sMSRB As Single
    ''' <summary>demersal biomass indicator</summary>
    Private m_sDemB As Single
    ''' <summary>pelagic biomass indicator</summary>
    Private m_sPelB As Single
    ''' <summary>demersal over pelagic biomass indicator</summary>
    Private m_sDemPelB As Single
    ''' <summary>Kempton's Q</summary>
    Private m_sKemptonsQ As Single
    ''' <summary>Shannon Diversity</summary>
    Private m_sShannonDiversity As Single

    ' --- Catch ---
    ''' <summary>Total Catch indicator</summary>
    Private m_sCT As Single
    ''' <summary>Fish Catch indicator</summary>
    Private m_sFishC As Single
    ''' <summary>Invertebrate Catch indicator</summary>
    Private m_sInveC As Single
    ''' <summary>Invertebrate over fish catch indicator</summary>
    Private m_sInveFishC As Single
    ''' <summary>Total cach of predatory organisms > TL4</summary>
    Private m_sC4 As Single
    ''' <summary>mammals, seabirds and reptiles catch indicator</summary>
    Private m_sMSRC As Single
    ''' <summary>Total Discards indicator</summary>
    Private m_sDT As Single
    ''' <summary>demersal catch indicator</summary>
    Private m_sDemC As Single
    ''' <summary>pelagic catch indicator</summary>
    Private m_sPelC As Single
    ''' <summary>demersal over pelagic catch indicator</summary>
    Private m_sDemPelC As Single

    ' --- Species ---
    ''' <summary>Endemic species in the catch</summary>
    Private m_sEndemicC As Single
    ''' <summary>Biomass of endemic species in the community</summary>
    Private m_sEndemicB As Single
    ''' <summary>IUCN species in the catch</summary>
    Private m_sIUCNC As Single
    ''' <summary>IUCN species biomass in the community</summary>
    Private m_sIUCNB As Single
    ''' <summary>Intrinsic vulnerability Index of the catch</summary>
    Private m_sIVIc As Single

    ' --- Size-based  ---
    ''' <summary>Mean life span of fish in the catch </summary>
    Private m_sMLifeSc As Single
    ''' <summary>Mean life span of fish in the community </summary>
    Private m_sMLifeSb As Single
    ''' <summary>Mean lengh of fish the catch </summary>
    Private m_sMLengthc As Single
    ''' <summary>Mean lengh of fish on the community </summary>
    Private m_sMLengthb As Single
    ''' <summary>Mean weight of fish the catch </summary>
    Private m_sMWeightc As Single
    ''' <summary>Mean weight of fish in the community </summary>
    Private m_sMWeightb As Single

#End Region ' Private variables

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of this class.
    ''' </summary>
    ''' <param name="core">The <see cref="cCore">Core</see> to operate onto.</param>
    ''' <param name="ecopathDS">The <see cref="cEcopathDataStructures">Ecopath data structures</see> to operate onto.</param>
    ''' <param name="stanzaDS">The <see cref="cStanzaDatastructures">Stanza data structures</see> to operate onto.</param>
    ''' <param name="taxonDS">The <see cref="cTaxonDataStructures">Taxonomy data structures</see> to operate onto.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore,
                   ByVal ecopathDS As cEcopathDataStructures,
                   ByVal stanzaDS As cStanzaDatastructures,
                   ByVal taxonDS As cTaxonDataStructures,
                   ByVal lookup As cTaxonAnalysis)

        ' Sanity checks
        Debug.Assert(core IsNot Nothing, "aargh")
        Debug.Assert(ecopathDS IsNot Nothing, "aargh")
        Debug.Assert(stanzaDS IsNot Nothing, "aargh")
        Debug.Assert(taxonDS IsNot Nothing, "aargh")

        ' Store refs
        Me.m_core = core
        Me.m_ecopathDS = ecopathDS
        Me.m_stanzaDS = stanzaDS
        Me.m_taxonDS = taxonDS

        ' Start off clean
        Me.Clear()

    End Sub

#End Region ' Constructor

#Region " Core data access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper function to access the associated <see cref="cCore">Core</see>.
    ''' </summary>
    ''' <returns>The associated <see cref="cCore">Core</see>.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function Core() As cCore
        Return Me.m_core
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper function to access the associated <see cref="cEcopathDataStructures">Ecopath data structures</see>.
    ''' </summary>
    ''' <returns>The associated <see cref="cEcopathDataStructures">Ecopath data structures</see>.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function EcopathDS() As cEcopathDataStructures
        Return Me.m_ecopathDS
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper function to access the associated <see cref="cTaxonDataStructures">Taxonomy data structures</see>.
    ''' </summary>
    ''' <returns>The associated <see cref="cTaxonDataStructures">Taxonomy data structures</see>.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function TaxonDS() As cTaxonDataStructures
        Return Me.m_taxonDS
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper function to access the associated <see cref="cStanzaDatastructures">Stanza data structures</see>.
    ''' </summary>
    ''' <returns>The associated <see cref="cStanzaDatastructures">Stanza data structures</see>.</returns>
    ''' -----------------------------------------------------------------------
    Protected Function StanzaDS() As cStanzaDatastructures
        Return Me.m_stanzaDS
    End Function

#End Region ' Core data access

#Region " Inputs "

    ' These are the computation units that are available from cEcopathIndicators, 
    ' cEcosimIndicators and cEcospaceIndicators

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Override to obtain the biomass for a given group, as computed by the 
    ''' underlying model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected MustOverride Function ModelBiomass(ByVal iGroup As Integer) As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Override to return the Trophic Level for a given group, as computed by 
    ''' the underlying model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected MustOverride Function ModelTL(ByVal iGroup As Integer) As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Override to return the Trophic Level of the Catch, a computed by the 
    ''' underlying model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected MustOverride Function ModelTLCatch() As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Override to return the Catch of a given group, as computed by the 
    ''' underlying model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected MustOverride Function ModelCatch(ByVal iGroup As Integer) As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Override to return the Discards of a given group, as computed by the 
    ''' underlying model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected MustOverride Function ModelDiscards(ByVal iGroup As Integer) As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Override to return the Kemptons' Q of a given group, as computed by the 
    ''' underlying model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected MustOverride Function ModelKemptionsQ() As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Override to return the Shannon Diversity as computed by the 
    ''' underlying model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected MustOverride Function ModelShannonDiversity() As Single

#End Region ' Inputs 

#Region " Computations "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Computations
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function Compute() As Boolean

        Dim group As cEcoPathGroupInput = Nothing
        Dim taxon As cTaxon = Nothing
        Dim ta As cTaxonAnalysis = Me.m_core.TaxonAnalysis

        ' Biomass computations
        Dim sTotalB As Single = 0
        Dim sTotalB2 As Single = 0
        Dim sTotalB325 As Single = 0
        Dim sTotalB4 As Single = 0
        Dim sCommB As Single = 0
        Dim sFishB As Single = 0
        Dim sInveB As Single = 0
        Dim sMSRB As Single = 0
        Dim sInveFishB As Single = 0
        Dim sDemB As Single = 0
        Dim sPelB As Single = 0

        ' Catch computations
        Dim sC325 As Single = 0
        Dim sCT As Single = 0
        Dim sDT As Single = 0
        Dim bIsLanded As Boolean = False
        Dim sFishC As Single = 0
        Dim sInveC As Single = 0
        Dim sInveFishC As Single = 0
        Dim sC4 As Single = 0
        Dim sMSRC As Single = 0
        Dim sDemC As Single = 0
        Dim sPelC As Single = 0

        ' Trophic level computations
        Dim sTLiBi As Single = 0
        Dim sTLi2Bi As Single = 0
        Dim sTLi325Bi As Single = 0
        Dim sTLi4Bi As Single = 0
        Dim sMTI As Single = 0
        Dim sTLCo325 As Single = 0

        ' Species indicators
        Dim sEndemicC As Single = 0
        Dim sEndemicB As Single = 0
        Dim sIUCNC As Single = 0
        Dim sIUCNB As Single = 0
        Dim sIVIc As Single = 0
        Dim sIVIiCi As Single = 0

        'Size-based indicators
        Dim sMLifeSSC As Single = 0
        Dim sMLifeSC As Single = 0
        Dim sMLifeSSB As Single = 0
        Dim sMLifeSB As Single = 0
        Dim sMLengthSC As Single = 0
        Dim sMLengthC As Single = 0
        Dim sMLengthSB As Single = 0
        Dim sMLengthB As Single = 0
        Dim sMWeightSC As Single = 0
        Dim sMWeightC As Single = 0
        Dim sMWeightSB As Single = 0
        Dim sMWeightB As Single = 0

        ' Make sure there are no left-overs from previous calculations
        Me.Clear()

        ' Gather information by group
        For iGroup As Integer = 1 To Me.m_ecopathDS.NumGroups

            group = Me.m_core.EcoPathGroupInputs(iGroup)

            Dim sC As Single = Me.ModelCatch(iGroup)
            Dim sB As Single = Me.ModelBiomass(iGroup)
            Dim sTL As Single = Me.ModelTL(iGroup)
            Dim sD As Single = Me.ModelDiscards(iGroup)

            sTotalB = sTotalB + sB
            sTLiBi = sTLiBi + (sB * sTL)
            sCT = sCT + sC
            sDT = sDT + sD

            If sTL >= 2.0 Then
                sTotalB2 = sTotalB2 + sB
                sTLi2Bi = sTLi2Bi + (sB * sTL)
            End If

            If sTL >= 3.25 Then
                sTotalB325 = sTotalB325 + sB
                sTLi325Bi = sTLi325Bi + (sB * sTL)
                sC325 = sC325 + sC
                sMTI = sMTI + (sTL * sC)
            End If

            If sTL >= 4 Then
                sTotalB4 = sTotalB4 + sB
                sTLi4Bi = sTLi4Bi + (sB * sTL)
                sC4 = sC4 + sC

            End If

            sInveB = sInveB + (sB * ta.GroupBiomassProportion(iGroup, eOrganismTypes.Invertebrates))
            sFishB = sFishB + (sB * ta.GroupBiomassProportion(iGroup, eOrganismTypes.Fishes))
            sFishC = sFishC + (sC * ta.GroupCatchProportion(iGroup, eOrganismTypes.Fishes))
            sInveC = sInveC + (sC * ta.GroupCatchProportion(iGroup, eOrganismTypes.Invertebrates))
            sEndemicB = sEndemicB + (sB * ta.GroupBiomassProportion(iGroup, eOccurrenceStatusTypes.Endemic))
            sEndemicC = sEndemicC + (sC * ta.GroupCatchProportion(iGroup, eOccurrenceStatusTypes.Endemic))
            sIUCNB = sIUCNB + (sB * ta.GroupBiomassProportion(iGroup, eIUCNConservationStatusTypes.NearThreatened, eOperators.GreaterThanOrEqualTo))
            sIUCNC = sIUCNC + (sC * ta.GroupCatchProportion(iGroup, eIUCNConservationStatusTypes.NearThreatened, eOperators.GreaterThanOrEqualTo))
            sMSRB = sMSRB + ((sB * ta.GroupBiomassProportion(iGroup, eOrganismTypes.Birds)) +
                             (sB * ta.GroupBiomassProportion(iGroup, eOrganismTypes.Mammals)) +
                             (sB * ta.GroupBiomassProportion(iGroup, eOrganismTypes.Reptiles)))
            sMSRC = sMSRC + ((sC * ta.GroupCatchProportion(iGroup, eOrganismTypes.Birds)) +
                             (sB * ta.GroupCatchProportion(iGroup, eOrganismTypes.Mammals)) +
                             (sB * ta.GroupCatchProportion(iGroup, eOrganismTypes.Reptiles)))
            sDemB = sDemB + (sB * ta.GroupCatchProportion(iGroup, eEcologyTypes.Demersal)) +
                (sB * ta.GroupCatchProportion(iGroup, eEcologyTypes.Bethic)) +
                (sB * ta.GroupCatchProportion(iGroup, eEcologyTypes.BathyDemersal))
            sPelB = sPelB + (sB * ta.GroupCatchProportion(iGroup, eEcologyTypes.Pelagic)) +
                (sB * ta.GroupCatchProportion(iGroup, eEcologyTypes.BathyPelagic)) +
                (sB * ta.GroupCatchProportion(iGroup, eEcologyTypes.BenthoPelagic)) +
                (sB * ta.GroupCatchProportion(iGroup, eEcologyTypes.PelagicNeritic)) +
                (sB * ta.GroupCatchProportion(iGroup, eEcologyTypes.PelagicOceanic))

            sDemC = sDemC + (sC * ta.GroupCatchProportion(iGroup, eEcologyTypes.Demersal)) +
                (sC * ta.GroupCatchProportion(iGroup, eEcologyTypes.Bethic)) +
                (sC * ta.GroupCatchProportion(iGroup, eEcologyTypes.BathyDemersal))

            sPelC = sPelC + (sC * ta.GroupCatchProportion(iGroup, eEcologyTypes.Pelagic)) +
                (sC * ta.GroupCatchProportion(iGroup, eEcologyTypes.BathyPelagic)) +
                (sC * ta.GroupCatchProportion(iGroup, eEcologyTypes.BenthoPelagic)) +
                (sC * ta.GroupCatchProportion(iGroup, eEcologyTypes.PelagicNeritic)) +
                (sC * ta.GroupCatchProportion(iGroup, eEcologyTypes.PelagicOceanic))

            For i As Integer = 1 To group.NTaxon
                Dim iTaxon As Integer = group.iTaxon(i)
                taxon = Me.Core.Taxon(iTaxon)

                If taxon.OrganismType = eOrganismTypes.Fishes Then
                    sIVIiCi = sIVIiCi + (taxon.VulnerabilityIndex * taxon.ProportionCatch * sC)

                    If (taxon.MeanLifespan > 0) Then
                        sMLifeSSC = sMLifeSSC + (taxon.MeanLifespan * taxon.ProportionCatch * sC)
                        sMLifeSC = sMLifeSC + (taxon.ProportionCatch * sC)
                    End If

                    If (taxon.MeanLifespan > 0) Then
                        sMLifeSSB = sMLifeSSB + (taxon.MeanLifespan * taxon.Proportion * sB)
                        sMLifeSB = sMLifeSB + (taxon.Proportion * sB)
                    End If

                    If (taxon.MeanWeight > 0) Then
                        sMWeightSC = sMWeightSC + (taxon.MeanWeight * taxon.ProportionCatch * sC)
                        sMWeightC = sMWeightC + (taxon.ProportionCatch * sC)
                    End If

                    If (taxon.MeanWeight > 0) Then
                        sMWeightSB = sMWeightSB + (taxon.MeanWeight * taxon.Proportion * sB)
                        sMWeightB = sMWeightB + (taxon.Proportion * sB)
                    End If

                    If (taxon.MeanLength > 0) Then
                        sMLengthSC = sMLengthSC + (taxon.MeanLength * taxon.ProportionCatch * sC)
                        sMLengthC = sMLengthC + (taxon.ProportionCatch * sC)
                    End If

                    If (taxon.MeanLength > 0) Then
                        sMLengthSB = sMLengthSB + (taxon.MeanLength * taxon.Proportion * sB)
                        sMLengthB = sMLengthB + (taxon.Proportion * sB)
                    End If

                End If ' fishes

            Next i

            ' Is group landed?
            bIsLanded = False
            For iFleet As Integer = 1 To Me.m_ecopathDS.NumFleet
                bIsLanded = bIsLanded Or (Me.m_ecopathDS.Landing(iFleet, iGroup) > 0)
            Next

            If (bIsLanded) Then
                ' Sum biomass for all landed groups
                sCommB = sCommB + sB
            End If

        Next iGroup

        ' Trouble shooting
        Debug.Assert(sInveB = 0 Or sDemB = 0 Or sInveB <> sDemB)
        Debug.Assert(sFishC = sFishC Or sCT = 0 Or sFishC <> sCT)

        ' Store indicators

        ' Biomass indicators
        Me.m_sTotalB = sTotalB
        Me.m_sCommB = sCommB
        Me.m_sFishB = sFishB
        Me.m_sInveB = sInveB
        Me.m_sTotalB4 = sTotalB4
        Me.m_sMSRB = sMSRB
        Me.m_sInveFishB = CSng(If(sFishB = 0, 0, sInveB / sFishB))
        Me.m_sDemB = sDemB
        Me.m_sPelB = sPelB
        Me.m_sDemPelB = CSng(If(sPelB = 0, 0, sDemB / sPelB))
        Me.m_sKemptonsQ = Me.ModelKemptionsQ()
        Me.m_sShannonDiversity = Me.ModelShannonDiversity()

        ' Catch indicators
        Me.m_sCT = sCT
        Me.m_sFishC = sFishC
        Me.m_sInveC = sInveC
        Me.m_sInveFishC = CSng(If(sFishC = 0, 0, sInveC / sFishC))
        Me.m_sC4 = sC4
        Me.m_sMSRC = sMSRC
        Me.m_sDT = sDT
        Me.m_sDemC = sDemC
        Me.m_sPelC = sPelC
        Me.m_sDemPelC = CSng(If(sPelC = 0, 0, sDemC / sPelC))

        ' Trophic indicators
        Me.m_sTLC = Me.ModelTLCatch ' Ha!
        Me.m_sMTI = CSng(If(sC325 = 0, 0, sMTI / sC325))
        Me.m_sTLCo = sTLiBi / Me.m_sTotalB
        Me.m_sTLCo2 = CSng(If(sTotalB2 = 0, 0, sTLi2Bi / sTotalB2))
        Me.m_sTLCo325 = CSng(If(sTotalB325 = 0, 0, sTLi325Bi / sTotalB325))
        Me.m_sTLCo4 = CSng(If(sTotalB4 = 0, 0, sTLi4Bi / sTotalB4))

        ' Species indicators
        Me.m_sEndemicC = sEndemicC
        Me.m_sEndemicB = sEndemicB
        Me.m_sIUCNC = sIUCNC
        Me.m_sIUCNB = sIUCNB
        Me.m_sIVIc = CSng(If(sFishC = 0, 0, sIVIiCi / sFishC))

        'Size-base indicators
        Me.m_sMLifeSc = CSng(If(sMLifeSC = 0, 0, sMLifeSSC / sMLifeSC))
        Me.m_sMLifeSb = CSng(If(sMLifeSB = 0, 0, sMLifeSSB / sMLifeSB))
        Me.m_sMLengthc = CSng(If(sMLengthC = 0, 0, sMLengthSC / sMLengthC))
        Me.m_sMLengthb = CSng(If(sMLengthB = 0, 0, sMLengthSB / sMLengthB))
        Me.m_sMWeightc = CSng(If(sMWeightC = 0, 0, sMWeightSC / sMWeightC))
        Me.m_sMWeightb = CSng(If(sMWeightB = 0, 0, sMWeightSB / sMWeightB))

        Me.IsComputed = True

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Reset calculated variables
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub Clear()

        Me.IsComputed = False

        ' --- Trophic based ---
        Me.m_sTLC = 0
        Me.m_sTLCo = 0
        Me.m_sTLCo2 = 0
        Me.m_sTLCo325 = 0
        Me.m_sMTI = 0
        Me.m_sTLCo4 = 0

        ' --- Biomass ---
        Me.m_sTotalB = 0
        Me.m_sCommB = 0
        Me.m_sFishB = 0
        Me.m_sInveB = 0
        Me.m_sInveFishB = 0
        Me.m_sTotalB4 = 0
        Me.m_sMSRB = 0
        Me.m_sDemB = 0
        Me.m_sPelB = 0
        Me.m_sDemPelB = 0
        Me.m_sKemptonsQ = 0
        Me.m_sShannonDiversity = 0

        ' --- Catch ---
        Me.m_sCT = 0
        Me.m_sFishC = 0
        Me.m_sInveC = 0
        Me.m_sInveFishC = 0
        Me.m_sC4 = 0
        Me.m_sMSRC = 0
        Me.m_sDT = 0
        Me.m_sDemC = 0
        Me.m_sPelC = 0
        Me.m_sDemPelC = 0

        ' --- Species ---
        Me.m_sEndemicC = 0
        Me.m_sEndemicB = 0
        Me.m_sIUCNC = 0
        Me.m_sIUCNB = 0
        Me.m_sIVIc = 0

        ' --- Size-based  ---
        Me.m_sMLifeSc = 0
        Me.m_sMLifeSb = 0
        Me.m_sMLengthc = 0
        Me.m_sMLengthb = 0
        Me.m_sMWeightc = 0
        Me.m_sMWeightb = 0

    End Sub

#End Region ' Computations

#Region " Outputs "

    ' Functions that expose the private variables. Do NOT rename these functions;
    ' they are invoked via Reflection.

#Region " Trophic indicators "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'tropic level of the catch' indicator.
    ''' </summary>
    ''' <returns>The 'tropic level of the catch' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function TLC() As Single
        Return Me.m_sTLC
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'MTI' indicator.
    ''' </summary>
    ''' <returns>The 'MTI' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function MTI() As Single
        Return Me.m_sMTI
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'tropic level of the community' indicator.
    ''' </summary>
    ''' <returns>The 'tropic level of the community' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function TLco() As Single
        Return Me.m_sTLCo
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'tropic level of the community where tropic level >= 2' indicator.
    ''' </summary>
    ''' <returns>The 'tropic level of the community where tropic level >= 2' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function TLco2() As Single
        Return Me.m_sTLCo2
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' returns the 'tropic level of the community where tropic level > 3.25' indicator.
    ''' </summary>
    ''' <returns>the 'tropic level of the community where tropic level > 3.25' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function TLco325() As Single
        Return Me.m_sTLCo325
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Trophic level of the community where trophic level > 4 indicator.
    ''' </summary>
    ''' <returns>The 'Trophic level of the community where trophic level > 4 indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function TLco4() As Single
        Return Me.m_sTLCo4
    End Function

#End Region ' Trophic indicators

#Region " Biomass indicators "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'total biomass' indicator.
    ''' </summary>
    ''' <returns>The 'total biomass' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function TotalB() As Single
        Return Me.m_sTotalB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'total commercial biomass' indicator.
    ''' </summary>
    ''' <returns>The 'total commercial biomass' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function CommercialB() As Single
        Return Me.m_sCommB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'total fish biomass' indicator.
    ''' </summary>
    ''' <returns>The 'total fish biomass' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function FishB() As Single
        Return Me.m_sFishB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'total invertebrate biomass' indicator.
    ''' </summary>
    ''' <returns>The 'total invertebrate biomass' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function InveB() As Single
        Return Me.m_sInveB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'biomass of invertebrates over fish' indicator.
    ''' </summary>
    ''' <returns>The 'biomass of invertebrates over fish' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function InveFishB() As Single
        Return Me.m_sInveFishB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'total fish of predaotry organisms >TL4' indicator.
    ''' </summary>
    ''' <returns>The 'total fish of predaotry organisms >TL4' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function PredB() As Single
        Return Me.m_sTotalB4
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'total biomass of marine mammals, seabirds and turtles' indicator.
    ''' </summary>
    ''' <returns>The 'total biomass of marine mammals, seabirds and turtles.</returns>
    ''' -----------------------------------------------------------------------
    Public Function MSRB() As Single
        Return Me.m_sMSRB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'demersal biomass' indicator.
    ''' </summary>
    ''' <returns>The 'demersal biomass.</returns>
    ''' -----------------------------------------------------------------------
    Public Function DemB() As Single
        Return Me.m_sDemB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'demersal over pelagic biomass' indicator.
    ''' </summary>
    ''' <returns>The 'demersal over pelagic biomass.</returns>
    ''' -----------------------------------------------------------------------
    Public Function DemPelB() As Single
        Return Me.m_sDemPelB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'demersal biomass' indicator.
    ''' </summary>
    ''' <returns>The 'demersal biomass.</returns>
    ''' -----------------------------------------------------------------------
    Public Function PelB() As Single
        Return Me.m_sPelB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'modified kemptons Q diversity index.
    ''' </summary>
    ''' <returns>The 'demersal biomass.</returns>
    ''' -----------------------------------------------------------------------
    Public Function KemptonsQ() As Single
        Return Me.m_sKemptonsQ
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the Shannon diversity index.
    ''' </summary>
    ''' <returns>The Shannon Diversity Index.</returns>
    ''' -----------------------------------------------------------------------
    Public Function ShannonDiversity() As Single
        Return Me.m_sShannonDiversity
    End Function

#End Region ' Biomass indicators

#Region " Catch indicators "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'total catch' indicator.
    ''' </summary>
    ''' <returns>The 'total catch' indicator.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Ctotal() As Single
        Return Me.m_sCT
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'fish catch' indicator.
    ''' </summary>
    ''' <returns>The 'fish catch' indicator.</returns>
    Public Function FishC() As Single
        Return Me.m_sFishC
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Invertebrate catch' indicator.
    ''' </summary>
    ''' <returns>The 'Invertebrate catch' indicator.</returns>
    Public Function InveC() As Single
        Return Me.m_sInveC
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Invertebrate over fish catch' indicator.
    ''' </summary>
    ''' <returns>The 'Invertebrate over fish catch' indicator.</returns>
    Public Function InveFishC() As Single
        Return Me.m_sInveFishC
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Catch of predatory organisms with TL >= 4.
    ''' </summary>
    ''' <returns>The 'Catch of predatory organisms with TL >= 4.</returns>
    Public Function sC4() As Single
        Return Me.m_sC4
    End Function

    '' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'total catch of marine mammals, seabirds and turtles.
    ''' </summary>
    ''' <returns>The 'total catch of marine mammals, seabirds and turtles.</returns>
    ''' -----------------------------------------------------------------------
    Public Function MSRC() As Single
        Return Me.m_sMSRC
    End Function

    '' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'total discards.
    ''' </summary>
    ''' <returns>The 'total discards.</returns>
    ''' -----------------------------------------------------------------------
    Public Function DT() As Single
        Return Me.m_sDT
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'demersal catch.
    ''' </summary>
    ''' <returns>The 'demersal catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function DemC() As Single
        Return Me.m_sDemC
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'demersal over pelagic catch.
    ''' </summary>
    ''' <returns>The 'demersal over pelagic catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function DemPelC() As Single
        Return Me.m_sDemPelC
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'demersal catch.
    ''' </summary>
    ''' <returns>The 'demersal catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function PelC() As Single
        Return Me.m_sPelC
    End Function

#End Region ' Catch indicators

#Region " Species indicators "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Endemic species in the catch.
    ''' </summary>
    ''' <returns>The 'Endemic species in the catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function EndemicC() As Single
        Return Me.m_sEndemicC
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Biomass of endemic species in the community.
    ''' </summary>
    ''' <returns>The 'Biomass of endemic species in the community.</returns>
    ''' -----------------------------------------------------------------------
    Public Function EndemicB() As Single
        Return Me.m_sEndemicB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'IUCN species in the catch'.
    ''' </summary>
    ''' <returns>The 'IUCN species in the catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IUCNC() As Single
        Return Me.m_sIUCNC
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'IUCN species biomass in the community'.
    ''' </summary>
    ''' <returns>The 'IUCN species biomass in the community.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IUCNB() As Single
        Return Me.m_sIUCNB
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Intrinsic Vulnerability Index of the catch'.
    ''' </summary>
    ''' <returns>The 'Intrinsic Vulnerability Index of the catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IVIC() As Single
        Return Me.m_sIVIc
    End Function


#End Region ' Species indicators

#Region " Size-based indicators "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Mean life span of fish in the catch.
    ''' </summary>
    ''' <returns>The 'Mean life span of the catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function MLifeSpanC() As Single
        Return m_sMLifeSc
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Mean life span of fish in the community.
    ''' </summary>
    ''' <returns>The 'Mean life span of the community.</returns>
    ''' -----------------------------------------------------------------------
    Public Function MLifeSpanB() As Single
        Return m_sMLifeSb
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Mean lenght of fish in the catch.
    ''' </summary>
    ''' <returns>The 'Mean lenght span of the catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function MLengthC() As Single
        Return m_sMLengthc
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Mean lenght of fish in the community.
    ''' </summary>
    ''' <returns>The 'Mean lenght span of the community.</returns>
    ''' -----------------------------------------------------------------------
    Public Function MLengthB() As Single
        Return m_sMLengthb
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Mean Weight of fish in the catch.
    ''' </summary>
    ''' <returns>The 'Mean Weight of fish in the catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function MWeightC() As Single
        Return m_sMWeightc
    End Function

    '' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the 'Mean Weight of fish in the community.
    ''' </summary>
    ''' <returns>The 'Mean Weight of fish in the community.</returns>
    ''' -----------------------------------------------------------------------
    Public Function MWeightB() As Single
        Return m_sMWeightb
    End Function

#End Region ' Size-based indicators

#End Region ' Outputs

#Region " Diagnostics "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether these indicators have been computed.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property IsComputed As Boolean = False

#End Region ' Diagnostics

End Class
