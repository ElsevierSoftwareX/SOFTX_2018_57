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

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class to analyze taxonomy data.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTaxonAnalysis

#Region " Private vars "

    Private m_taxonDS As cTaxonDataStructures = Nothing
    Private m_dt As New Dictionary(Of String, Single)

#End Region ' Private vars

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of the taxon analysis class.
    ''' </summary>
    ''' <param name="taxonDS">The <see cref="cTaxonDataStructures">taxonomy data structures</see>
    ''' to obtain taxon data from.</param>
    ''' -----------------------------------------------------------------------
    Friend Sub New(ByVal taxonDS As cTaxonDataStructures)
        Me.m_taxonDS = taxonDS
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the biomass proportion of all taxa for a single group matching 
    ''' a specific condition.
    ''' </summary>
    ''' <param name="iGroup">The group to obtain taxa for.</param>
    ''' <param name="val">The value to test against. Supported value types are
    ''' <see cref="eOccurrenceStatusTypes"/>, <see cref="eIUCNConservationStatusTypes"/>, 
    ''' <see cref="eOrganismTypes"/> and <see cref="eEcologyTypes"/></param>
    ''' <param name="op">The <see cref="eOperators">operation</see> to perform.
    ''' If not provided <see cref="eOperators.EqualTo">'='</see> is used.</param>
    ''' <returns>The proportion of biomass.</returns>
    ''' <example lang="VB.NET">
    ''' <code>
    ''' Dim taxonanalysis As cTaxonAnalysis = Me.m_core.TaxonAnalysis
    ''' Dim Binv As Single = 0 ' Biomass of invertebrates
    ''' Dim Bnt as single = 0  ' Biomass of species listed as IUCN near-treathened or worse 
    ''' 
    ''' For iGroup As Integer = 1 To Me.m_core.NumGroups
    '''     ' Sum up the biomass for all invertebrates
    '''     Binv += Me.m_core.EcopathGroupOutput(iGroup).Biomass * taxonanalysis.GroupBiomassProportion(iGroup, eOrganismTypes.Invertebrates))
    '''     ' Sum up the biomass for all species with a IUCN status of near-threathened or worse
    '''     Bnt += Me.m_core.EcopathGroupOutput(iGroup).Biomass * taxonanalysis.GroupBiomassProportion(iGroup, eIUCNConservationStatusTypes.NearThreatened, eOperators.GreaterThanOrEqualTo))
    ''' Next iGroup
    ''' </code>
    ''' </example>
    ''' -----------------------------------------------------------------------
    Public Function GroupBiomassProportion(ByVal iGroup As Integer,
                                           ByVal val As Object,
                                           Optional ByVal op As eOperators = eOperators.EqualTo) As Single
        Return Me.GroupProportion(eComputationType.Biomass, iGroup, val, op)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the catch proportion of all taxa for a single group matching 
    ''' a specific condition.
    ''' </summary>
    ''' <param name="iGroup">The group to obtain taxa for.</param>
    ''' <param name="val">The value to test against. Supported value types are
    ''' <see cref="eOccurrenceStatusTypes"/>, <see cref="eIUCNConservationStatusTypes"/>, 
    ''' <see cref="eOrganismTypes"/> and <see cref="eEcologyTypes"/></param>
    ''' <param name="op">The <see cref="eOperators">operation</see> to perform.
    ''' If not provided <see cref="eOperators.EqualTo">'='</see> is used.</param>
    ''' <returns>The proportion of catch.</returns>
    ''' -----------------------------------------------------------------------
    Public Function GroupCatchProportion(ByVal iGroup As Integer,
                                         ByVal val As Object,
                                         Optional ByVal op As eOperators = eOperators.EqualTo) As Single
        Return Me.GroupProportion(eComputationType.Catch, iGroup, val, op)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Resets all internally cached values.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Clear()
        Me.m_dt.Clear()
    End Sub

#Region " Internals "

    Private Enum eComputationType As Integer
        Biomass
        [Catch]
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Perform the actual computation and caches the result.
    ''' </summary>
    ''' <param name="computation">The <see cref="eComputationType"/>.</param>
    ''' <param name="iGroup"></param>
    ''' <param name="value"></param>
    ''' <param name="op"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function GroupProportion(ByVal computation As eComputationType,
                                     ByVal iGroup As Integer,
                                     ByVal value As Object,
                                     ByVal op As eOperators) As Single

        Dim iTaxon As Integer = 0
        Dim sProportion As Single = 0
        Dim sPropTot As Single = 0
        Dim comp As cOperatorBase = cOperatorManager.getOperator(op)
        Dim avals As Array = Nothing
        Dim sVal As Single = CSng(value)
        Dim strKey As String = Me.Key(computation, iGroup, sVal, value.GetType(), op)

        If (Not Me.m_dt.ContainsKey(strKey)) Then

            If TypeOf (value) Is eOrganismTypes Then
                avals = Me.m_taxonDS.TaxonOrganismType
            ElseIf TypeOf (value) Is eIUCNConservationStatusTypes Then
                avals = Me.m_taxonDS.TaxonIUCNConservationStatus
            ElseIf TypeOf (value) Is eExploitationTypes Then
                avals = Me.m_taxonDS.TaxonExploitationStatus
            ElseIf TypeOf (value) Is eEcologyTypes Then
                avals = Me.m_taxonDS.TaxonEcologyType
            ElseIf TypeOf (value) Is eOccurrenceStatusTypes Then
                avals = m_taxonDS.TaxonOccurrenceStatus
            End If

            Debug.Assert(avals IsNot Nothing)

            For i As Integer = 1 To Me.m_taxonDS.NumGroupTaxa(iGroup)
                iTaxon = Me.m_taxonDS.GroupTaxa(iGroup, i)
                If (comp.Compare(CSng(avals.GetValue(iTaxon)), sVal)) Then
                    Select Case computation
                        Case eComputationType.Biomass
                            sProportion += Me.m_taxonDS.TaxonPropBiomass(iTaxon)
                        Case eComputationType.Catch
                            sProportion += Me.m_taxonDS.TaxonPropCatch(iTaxon)
                    End Select
                End If

                Select Case computation
                    Case eComputationType.Biomass
                        sPropTot += Me.m_taxonDS.TaxonPropBiomass(iTaxon)
                    Case eComputationType.Catch
                        sPropTot += Me.m_taxonDS.TaxonPropCatch(iTaxon)
                End Select
            Next

            If (sPropTot = 0) Then
                sVal = 0
            Else
                sVal = sProportion / sPropTot
            End If

            ' Cache the calculated value
            Me.m_dt(strKey) = sVal

        End If
        Return Me.m_dt(strKey)

    End Function

    Private Function Key(ByVal comp As eComputationType,
                         ByVal iGroup As Integer,
                         ByVal sVal As Single,
                         ByVal t As Type,
                         ByVal op As eOperators) As String
        Return comp.ToString & ":" & iGroup & "_" & op & "_" & t.ToString & "(" & sVal & ")"
    End Function

#End Region ' Internals

End Class
