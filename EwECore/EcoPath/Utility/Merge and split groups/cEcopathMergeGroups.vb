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
Imports EwECore.Auxiliary
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Ecopath

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class to merge two groups in Ecopath.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cEcopathMergeGroups
        Inherits cEcopathMergeSplitGroups

        Public Enum eEstimate As Integer
            NotSet = 0
            Biomass
            PB
            QB
            EE
        End Enum

#Region " Private variables "

        Private m_data As cEcopathMergeGroupsDatastructures = Nothing

#End Region ' Private variables

#Region " Construction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="core">The <see cref="cCore"/> to operate on.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal core As cCore)
            Me.m_core = core
            Me.m_data = New cEcopathMergeGroupsDatastructures()
            Me.m_data.Init(core.m_EcoPathData, core.m_Stanza)
        End Sub

#End Region ' Construction

#Region " Public access "

        Public ReadOnly Property Data As cEcopathMergeGroupsDatastructures
            Get
                Return Me.m_data
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the current Ecopath model is ready to merge two specific
        ''' groups and a candidate name.
        ''' </summary>
        ''' <param name="agg1">The one-based index of the first group.</param>
        ''' <param name="agg2">The one-based index of the second group.</param>
        ''' <param name="strName">A suggested name for the aggregation of two groups.</param>
        ''' <returns>True if the proposed merge can be executed.</returns>
        ''' -----------------------------------------------------------------------
        Public Function CanMergeGroups(agg1 As Integer, agg2 As Integer, strName As String) As Boolean

            Return (Me.CanMergeSplitGroups(False) = True) And
               (Array.IndexOf(Me.CompatibleGroups(agg1), agg2) > -1) And
               (Not String.IsNullOrWhiteSpace(strName))

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns an array of <see cref="cCoreGroupBase.Index">indexes</see> of
        ''' groups that can be merged with the provided <paramref name="iGroup">group index</paramref>.
        ''' </summary>
        ''' <param name="iGroup">The group index to find compatible groups for.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <para>Producers can be merged with producers;</para>
        ''' <para>Consumers with consumers;</para>
        ''' <para>Detritus with detritus.</para>
        ''' <para>For stanza groups, only life stages within the same stanza group 
        ''' can be merged</para>
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Function CompatibleGroups(iGroup As Integer) As Integer()

            Dim groups As New List(Of Integer)
            Dim ecopathds As cEcopathDataStructures = Me.m_core.m_EcoPathData

            If (1 <= iGroup And iGroup <= Me.m_core.nGroups) Then
                If ecopathds.StanzaGroup(iGroup) Then

                    Dim stanzads As cStanzaDatastructures = Me.m_core.m_Stanza
                    Dim iStanza As Integer = stanzads.SpeciesCode(iGroup, 0)

                    ' Find stanza for this group
                    For isp As Integer = 1 To stanzads.Nsplit 'No. of split group
                        Dim bFound As Boolean = False
                        For ist As Integer = 1 To stanzads.Nstanza(isp) ' No. of stanza in a split group
                            If stanzads.EcopathCode(isp, ist) = iGroup Then
                                bFound = True
                            Else
                                groups.Add(stanzads.EcopathCode(isp, ist))
                            End If
                        Next
                        If bFound Then Return groups.ToArray()
                        groups.Clear()
                    Next
                Else

                    If (iGroup <= ecopathds.NumLiving) Then
                        Dim sPP As Single = ecopathds.PP(iGroup)
                        For i As Integer = 1 To ecopathds.NumGroups
                            ' Math.Ceiling bit added to match Producer PP fractions
                            If (Math.Ceiling(ecopathds.PP(i)) = Math.Ceiling(sPP)) And (Not ecopathds.StanzaGroup(i)) Then groups.Add(i)
                        Next
                    Else
                        For i As Integer = 1 To ecopathds.NumDetrit
                            groups.Add(i + ecopathds.NumLiving)
                        Next
                    End If

                    groups.Remove(iGroup)
                End If
            End If

            Return groups.ToArray()
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns a suggested name for the aggregation of two groups.
        ''' </summary>
        ''' <param name="agg1">The one-based index of the first group.</param>
        ''' <param name="agg2">The one-based index of the second group.</param>
        ''' <returns>A suggested name for the aggregation of two groups.</returns>
        ''' -----------------------------------------------------------------------
        Public Function GroupName(agg1 As Integer, agg2 As Integer) As String

            Dim ecopathds As cEcopathDataStructures = Me.m_core.m_EcoPathData

            If (agg1 < 1) Or (agg2 < 1) Then Return ""
            If (agg1 > ecopathds.NumGroups) Or (agg2 > ecopathds.NumGroups) Then Return ""

            Dim s1 As String = ecopathds.GroupName(agg1)
            Dim s2 As String = ecopathds.GroupName(agg2)

            If (s1.Length + s2.Length) > 47 Then
                If (s1.Length > 20) Then s1 = s1.Substring(0, 20)
                If (s2.Length > 20) Then s2 = s2.Substring(0, 20)
            End If

            Return s1 & " / " & s2

        End Function

        Public Function Calculate(agg1 As Integer, agg2 As Integer, strName As String,
                              estimate As eEstimate, bCalcEstimate As Boolean) As Boolean

            Dim ecopathds As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim stanzads As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim taxonds As cTaxonDataStructures = Me.m_core.m_TaxonData

            Me.m_data.IndexTarget = agg1
            Me.m_data.IndexMerge = agg2
            Me.m_data.Estimate = estimate
            Me.m_data.GroupName = strName
            Me.m_data.IsValid = False

            ' Sanity checks
            If (Array.IndexOf(Me.CompatibleGroups(agg1), agg2) = -1) Then Return False

            Dim Btot As Single = (ecopathds.B(agg1) + ecopathds.B(agg2))
            Dim Qtot As Single = (ecopathds.B(agg1) * ecopathds.QB(agg1) + ecopathds.B(agg2) * ecopathds.QB(agg2))
            Dim Ptot As Single = (ecopathds.B(agg1) * ecopathds.PB(agg1) + ecopathds.B(agg2) * ecopathds.PB(agg2))
            Dim Ctot As Single = ecopathds.fCatch(agg1) + ecopathds.fCatch(agg2)

            If (0 = Ctot) Then Ctot = 1 ' To prevent division by zero

            Me.m_data.PBinput = (ecopathds.PB(agg1) * ecopathds.B(agg1) + ecopathds.PB(agg2) * ecopathds.B(agg2)) / Btot
            Me.m_data.BaBi = (ecopathds.BaBi(agg1) * ecopathds.B(agg1) + ecopathds.BaBi(agg2) * ecopathds.B(agg2)) / Btot
            Me.m_data.BAInput = ecopathds.BA(agg1) + ecopathds.BA(agg2)
            Me.m_data.Immig = ecopathds.Immig(agg1) + ecopathds.Immig(agg2)
            Me.m_data.Emigration = ecopathds.Emigration(agg1) + ecopathds.Emigration(agg2)
            Me.m_data.EmigRate = (ecopathds.Emig(agg1) * ecopathds.B(agg1) + ecopathds.Emig(agg2) * ecopathds.B(agg2)) / Btot
            'Catch(Agg1) = Catch(Agg1) + Catch(Agg2)
            ' fCatch calculated on the fly from landings and discards; no need to update
            Me.m_data.Det = ecopathds.det(0, agg1) + ecopathds.det(0, agg2)

            ' Discard fate for detritus groups
            If (agg1 > ecopathds.NumLiving) Then
                For iFleet As Integer = 1 To ecopathds.NumFleet
                    If ecopathds.Discard(iFleet, agg1) + ecopathds.Discard(iFleet, agg2) > 0 Then
                        Me.m_data.DiscardFate(iFleet) = (ecopathds.Discard(iFleet, agg1) * ecopathds.DiscardFate(iFleet, agg1 - ecopathds.NumLiving) + ecopathds.Discard(iFleet, agg2) * ecopathds.DiscardFate(iFleet, agg2 - ecopathds.NumLiving)) / (ecopathds.Discard(iFleet, agg1) + ecopathds.Discard(iFleet, agg2))
                    End If
                Next
            End If

            Me.m_data.EEinput = cCore.NULL_VALUE
            Me.m_data.GS = cCore.NULL_VALUE

            ' Is living?
            If (agg1 <= ecopathds.NumLiving) Then
                ' #Yes: do living bits
                Me.m_data.EEinput = (ecopathds.EE(agg1) * ecopathds.B(agg1) + ecopathds.EE(agg2) * ecopathds.B(agg2)) / Btot

                ' Has consumption?
                If (Qtot > 0) Then

                    ' Merge amounts that other groups eat of agg1 and agg2
                    For i As Integer = 1 To ecopathds.NumGroups
                        If (i <> agg1 And i <> agg2) Then
                            Me.m_data.DCInput(i, agg1) = ecopathds.DCInput(i, agg1) + ecopathds.DCInput(i, agg2)
                        End If
                    Next i

                    Me.m_data.GS = (ecopathds.GS(agg1) * ecopathds.B(agg1) * ecopathds.QB(agg1) + ecopathds.GS(agg2) * ecopathds.B(agg2) * ecopathds.QB(agg2)) / Qtot

                    ' Fix diet of agg1
                    For iPrey As Integer = 1 To ecopathds.NumGroups
                        Dim iPreyMerge As Integer = iPrey
                        ' Special case: cannibalism
                        If (iPrey = agg2) Then iPreyMerge = agg1
                        Me.m_data.DCInput(agg1, iPreyMerge) = (ecopathds.DCInput(agg1, iPrey) * ecopathds.QB(agg1) * ecopathds.B(agg1) + ecopathds.DCInput(agg2, iPrey) * ecopathds.QB(agg2) * ecopathds.B(agg2)) / Qtot
                    Next iPrey

                    Me.m_data.DCInput(agg1, 0) = (ecopathds.DCInput(agg1, 0) * ecopathds.QB(agg1) * ecopathds.B(agg1) + ecopathds.DCInput(agg2, 0) * ecopathds.QB(agg2) * ecopathds.B(agg2)) / Qtot

                End If
                Me.m_data.QBinput = Qtot / Btot
            Else
                ' #No: detritus
                Me.m_data.DtImp = ecopathds.DtImp(agg1) + ecopathds.DtImp(agg2)
            End If

            Me.m_data.Shadow = (ecopathds.Shadow(agg1) * ecopathds.B(agg1) + ecopathds.Shadow(agg2) * ecopathds.B(agg2)) / (ecopathds.B(agg1) + ecopathds.B(agg2))

            'Landing and discards are just summed
            For i As Integer = 1 To ecopathds.NumFleet
                If ecopathds.Landing(i, agg1) + ecopathds.Landing(i, agg2) > 0 Then
                    Me.m_data.Market(i) = (ecopathds.Market(i, agg1) * ecopathds.Landing(i, agg1) + ecopathds.Market(i, agg2) * ecopathds.Landing(i, agg2)) / (ecopathds.Landing(i, agg1) + ecopathds.Landing(i, agg2))
                End If
                Me.m_data.Landing(i) = ecopathds.Landing(i, agg1) + ecopathds.Landing(i, agg2)
                Me.m_data.Discard(i) = ecopathds.Discard(i, agg1) + ecopathds.Discard(i, agg2)
            Next

            Me.m_data.Binput = ecopathds.B(agg1) + ecopathds.B(agg2)
            Me.m_data.Area = (ecopathds.Area(agg1) + ecopathds.Area(agg2)) / 2
            Me.m_data.BHinput = (ecopathds.BH(agg1) + ecopathds.BH(agg2)) / m_data.Area

            ' Need to determine estimation?
            If (bCalcEstimate) Then
                If (ecopathds.BHinput(agg1) < 0) And (ecopathds.BHinput(agg2) < 0) Then Me.m_data.Estimate = eEstimate.Biomass
                If (ecopathds.PBinput(agg1) < 0) And (ecopathds.PBinput(agg2) < 0) Then Me.m_data.Estimate = eEstimate.PB
                If (ecopathds.QBinput(agg1) < 0) And (ecopathds.QBinput(agg2) < 0) Then Me.m_data.Estimate = eEstimate.QB
                If (ecopathds.EEinput(agg1) < 0) And (ecopathds.EEinput(agg2) < 0) Then Me.m_data.Estimate = eEstimate.EE
            End If

            Select Case Me.m_data.Estimate
                Case eEstimate.Biomass : m_data.Binput = cCore.NULL_VALUE
                Case eEstimate.PB : m_data.PBinput = cCore.NULL_VALUE
                Case eEstimate.QB : m_data.QBinput = cCore.NULL_VALUE
                Case eEstimate.EE : m_data.EEinput = cCore.NULL_VALUE
            End Select

            ' Stanza
            If Me.m_core.m_EcoPathData.StanzaGroup(agg1) Then

                Dim iStanza As Integer = -1
                Dim iLifestage1 As Integer = -1
                Dim iLifestage2 As Integer = -1

                For isp As Integer = 1 To stanzads.Nsplit
                    For ist As Integer = 1 To stanzads.Nstanza(isp)
                        If stanzads.EcopathCode(isp, ist) = agg1 Then
                            iLifestage1 = ist
                            iStanza = isp
                        End If
                        If stanzads.EcopathCode(isp, ist) = agg2 Then iLifestage2 = ist
                    Next
                Next

                Debug.Assert(iStanza >= 0 And iLifestage1 >= 0 And iLifestage2 >= 0)

                If stanzads.BaseStanza(iStanza) = agg2 Then Me.m_data.BaseStanza(iStanza) = agg1
                If stanzads.BaseStanzaCB(iStanza) = agg2 Then Me.m_data.BaseStanzaCB(iStanza) = agg1

                Me.m_data.Age1(iStanza, iLifestage1) = Math.Min(stanzads.Age1(iStanza, iLifestage1), stanzads.Age1(iStanza, iLifestage2))
                Me.m_data.StanzaZ(iStanza, iLifestage1) = (stanzads.Stanza_Z(iStanza, iLifestage1) + stanzads.Stanza_Z(iStanza, iLifestage2)) / 2
                Me.m_data.iStanza = iStanza
                Me.m_data.iLifeStage = iLifestage1

            End If

            ' Taxa
            For i As Integer = 1 To taxonds.NumTaxon
                If Not (taxonds.IsTaxonStanza(i)) Then
                    Select Case taxonds.TaxonTarget(i)
                        Case Me.m_data.IndexTarget
                            Me.m_data.TaxonPropBiomass(i) = ecopathds.B(agg1) * taxonds.TaxonPropBiomass(i) / Btot
                            Me.m_data.TaxonPropCatch(i) = ecopathds.fCatch(agg1) * taxonds.TaxonPropCatch(i) / Ctot
                        Case Me.m_data.IndexMerge
                            Me.m_data.TaxonPropBiomass(i) = ecopathds.B(agg2) * taxonds.TaxonPropBiomass(i) / Btot
                            Me.m_data.TaxonPropCatch(i) = ecopathds.fCatch(agg2) * taxonds.TaxonPropCatch(i) / Ctot
                    End Select
                End If
            Next

            Me.m_data.IsValid = Not String.IsNullOrWhiteSpace(Me.m_data.GroupName)

        End Function


        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Merge two groups.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Public Function Merge() As Boolean

            ' Weakness in this implementation:
            ' Core changes are made that will get saved. If group deletion somehow fails then these saved changes cannot be undone.
            ' A proper fix would be to
            '  1) Start batch lock
            '  2) Remove group 2
            '  3) Update Ecopath data
            '  4) Release batch lock
            ' This would require all data modifications, including taxon and remarks merge, to be prepared in cMergeGroupsDataStructures prior to setting the batch lock

            ' Sanity checks
            If Not Me.m_data.IsValid Then Return False

            Dim ecopathds As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim stanzads As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim taxonds As cTaxonDataStructures = Me.m_core.m_TaxonData
            Dim agg1 As Integer = Me.m_data.IndexTarget
            Dim bSuccess As Boolean = True

            ' Grab names that are going to disappear
            Dim strName1 As String = ecopathds.GroupName(Me.m_data.IndexTarget)
            Dim strName2 As String = ecopathds.GroupName(Me.m_data.IndexMerge)

            ' Merge generic fields
            ecopathds.GroupName(agg1) = Me.m_data.GroupName
            ecopathds.Binput(agg1) = Me.m_data.Binput
            ecopathds.BHinput(agg1) = Me.m_data.BHinput
            ecopathds.PBinput(agg1) = Me.m_data.PBinput
            ecopathds.BaBi(agg1) = Me.m_data.BaBi
            ecopathds.BAInput(agg1) = Me.m_data.BAInput
            ecopathds.Immig(agg1) = Me.m_data.Immig
            ecopathds.Emigration(agg1) = Me.m_data.Emigration
            ecopathds.Emig(agg1) = Me.m_data.EmigRate
            ecopathds.det(0, agg1) = Me.m_data.Det

            If (agg1 > ecopathds.NumLiving) Then
                For i As Integer = 1 To ecopathds.NumFleet
                    ecopathds.DiscardFate(i, agg1 - ecopathds.NumLiving) = Me.m_data.DiscardFate(agg1 - ecopathds.NumLiving)
                Next
            End If

            ecopathds.EEinput(agg1) = Me.m_data.EEinput
            ecopathds.GS(agg1) = Me.m_data.GS
            ecopathds.Shadow(agg1) = Me.m_data.Shadow
            ecopathds.QBinput(agg1) = Me.m_data.QBinput

            ' Diet
            For iPred As Integer = 1 To Me.m_core.nLivingGroups
                For iPrey As Integer = 0 To Me.m_core.nGroups
                    ecopathds.DCInput(iPred, iPrey) = Me.m_data.DCInput(iPred, iPrey)
                    ' ecopathds.DietWasChanged(agg1, i) is not needed; Ecopath will reinit
                Next
            Next
            ecopathds.DtImp(agg1) = Me.m_data.DtImp

            ' Fisheries
            For i As Integer = 1 To ecopathds.NumFleet
                ecopathds.Market(i, agg1) = Me.m_data.Market(i)
                ecopathds.Landing(i, agg1) = Me.m_data.Landing(i)
                ecopathds.Discard(i, agg1) = Me.m_data.Discard(i)
            Next

            ' Stanza
            If Me.m_core.m_EcoPathData.StanzaGroup(agg1) Then
                stanzads.Age1(Me.m_data.iStanza, Me.m_data.iLifeStage) = Me.m_data.Age1(Me.m_data.iStanza, Me.m_data.iLifeStage)
                stanzads.Stanza_Z(Me.m_data.iStanza, Me.m_data.iLifeStage) = Me.m_data.StanzaZ(Me.m_data.iStanza, Me.m_data.iLifeStage)
            End If

            ' Taxa
            For Each iTaxon As Integer In Me.m_data.TaxonPropBiomass.Keys
                taxonds.TaxonTarget(iTaxon) = agg1
                taxonds.TaxonPropBiomass(iTaxon) = Me.m_data.TaxonPropBiomass(iTaxon)
                taxonds.TaxonPropCatch(iTaxon) = Me.m_data.TaxonPropCatch(iTaxon)
            Next

            ' Remark magic
            Me.MergeRemarks(eDataTypes.EcoPathGroupInput, ecopathds.GroupDBID(Me.m_data.IndexTarget), ecopathds.GroupDBID(Me.m_data.IndexMerge))
            Me.MergeRemarks(eDataTypes.EcoPathGroupOutput, ecopathds.GroupDBID(Me.m_data.IndexTarget), ecopathds.GroupDBID(Me.m_data.IndexMerge))

            Me.m_core.DataSource.SetChanged(eCoreComponentType.EcoPath)
            Me.m_core.StateMonitor.UpdateDataState(Me.m_core.DataSource)

            ' Compact bit: save changes and prepare for group removal in a batch lock.
            If Me.m_core.SetBatchLock(cCore.eBatchLockType.Restructure) Then
                bSuccess = Me.m_core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecopath, Me.m_core.RemoveGroup(Me.m_data.IndexMerge))
            End If

            If bSuccess Then
                Me.SendMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_GROUPMERGE_SUCCESS, strName1, strName2, Me.m_data.GroupName), True)
            End If

            Return bSuccess

        End Function

        Private Sub MergeRemarks(dt As eDataTypes, iDBID1 As Integer, iDBID2 As Integer)

            ' Get all remarks for the group that is going to disappear
            Dim merge As Dictionary(Of String, cAuxiliaryData) = Me.m_core.AuxillaryData(dt, iDBID2, True)
            ' For all keys
            For Each mergekey As String In merge.Keys
                ' Get data
                Dim auxMerge As cAuxiliaryData = merge(mergekey)
                ' Has remark?
                If (Not String.IsNullOrWhiteSpace(auxMerge.Remark)) Then
                    ' #Yes: deconstruct ID
                    Dim vidTarget As cValueID = cValueID.FromString(mergekey)
                    ' Reroute ID to target group
                    If (vidTarget.DataTypePrim = dt And vidTarget.DBIDPrim = iDBID2) Then vidTarget.DBIDPrim = iDBID1
                    If (vidTarget.DataTypeSec = dt And vidTarget.DBIDSec = iDBID2) Then vidTarget.DBIDSec = iDBID1
                    ' Apply to core
                    Me.m_core.AuxillaryData(vidTarget).MergeWith(auxMerge)
                End If
            Next

        End Sub

        Private Sub MergeTaxa()

            Dim taxonds As cTaxonDataStructures = Me.m_core.m_TaxonData
            Dim ecopathds As cEcopathDataStructures = Me.m_core.m_EcoPathData

            ' Taxon biomasses
            Dim dtB As New Dictionary(Of Integer, Single)
            Dim Btot As Single = ecopathds.B(Me.m_data.IndexTarget) + ecopathds.B(Me.m_data.IndexMerge)

            For i As Integer = 1 To taxonds.NumTaxon
                If Not (taxonds.IsTaxonStanza(i)) Then
                    Select Case taxonds.TaxonTarget(i)
                        Case Me.m_data.IndexTarget
                            dtB(i) = ecopathds.B(Me.m_data.IndexTarget) * taxonds.TaxonPropBiomass(i)
                        Case Me.m_data.IndexMerge
                            dtB(i) = ecopathds.B(Me.m_data.IndexMerge) * taxonds.TaxonPropBiomass(i)
                    End Select
                End If
            Next

            For Each iTaxon As Integer In dtB.Keys
                taxonds.TaxonTarget(iTaxon) = Me.m_data.IndexTarget
                taxonds.TaxonPropBiomass(iTaxon) = dtB(iTaxon) / Btot
            Next

            ' ToDo: update proportion of Catch

        End Sub

#End Region ' Public access

#Region " Original code "

#If 0 Then ' From Ecopath v5, ecoagg1.bas

Attribute VB_Name = "modAggregation"
        ' ============================================
        ' System package : ECOPATH2 (ver. 2.0)
        ' Programmers    : V. Christensen & K. Janagap
        ' Program name   : ECOAGG.BAS (Aggregate)
        ' Last revision  : 22 Nov 1991
        ' VB Version     : Jan-Feb 1995
        ' By             : Edwin de Guzmann and VC
        ' ============================================
        Option Explicit

Public Agg1 As Integer
Public Agg2 As Integer
Public NN1 As Integer
Dim K As Integer

Public Sub AggregateYourSelf()
'That is: You already named groups to be aggregated: Agg1 and Agg2
    ReDim GroupForDeletion(NumGroups) As Boolean
    'Aggregate basic parameters:
    CalcAggMan
    UpdateValuesOnInputFormAndInDataBase frmInputData.vaInput
    'Delete the Agg2 group
    GroupForDeletion(Agg2) = True
    DoDeleteGroup
    UpdateSpeciesSequence

End Sub

Sub CalcAggMan()
On Local Error Resume Next
Dim i As Integer
Dim SumCons As Single
Dim SQL As String
    SQL = "SELECT * from [Group Info] where modelName='" & lastModel & "' and groupName='" & Trim(Specie(Agg1)) & "'"
    Set g_Recordset = CCG.UpdatableRecords(SQL)
    If g_Recordset.RecordCount > 0 Then
        g_Recordset.Fields("groupName").value = Trim(Left$(Specie(Agg1), 20) + " / " + Left$(Specie(Agg2), 20))
        g_Recordset.Update
    End If
    Specie(Agg1) = Left$(Specie(Agg1), 20) + " / " + Left$(Specie(Agg2), 20)
    SetCellText frmInputData.vaInput, 1, Agg1, Specie(Agg1)

    PBi(Agg1) = (PB(Agg1) * B(Agg1) + PB(Agg2) * B(Agg2)) / (B(Agg1) + B(Agg2))
    BaBi(Agg1) = (BaBi(Agg1) * B(Agg1) + BaBi(Agg2) * B(Agg2)) / (B(Agg1) + B(Agg2))
    BAi(Agg1) = BA(Agg1) + BA(Agg2)
    Immigi(Agg1) = Immig(Agg1) + Immig(Agg2)
    Emigrationi(Agg1) = Emigrationi(Agg1) + Emigrationi(Agg2)
    Emigi(Agg1) = (Emigi(Agg1) * B(Agg1) + Emigi(Agg2) * B(Agg2)) / (B(Agg1) + B(Agg2))
    'EX(Agg1) = EX(Agg1) + EX(Agg2)
    Catch(Agg1) = Catch(Agg1) + Catch(Agg2)
    det(0, Agg1) = det(0, Agg1) + det(0, Agg2)

    'Discardfate: Weighted average; only for detritus groups
    If Agg1 > NumLiving Then
        For i = 1 To NumGear
            If Discard(i, Agg1) + Discard(i, Agg2) > 0 Then
                DiscardFate(i, Agg1 - NumLiving) = (Discard(i, Agg1) * DiscardFate(i, Agg1 - NumLiving) + Discard(i, Agg2) * DiscardFate(i, Agg2 - NumLiving)) / (Discard(i, Agg1) + Discard(i, Agg2))
            End If
        Next
    End If

    If (B(Agg1) + B(Agg2)) > 0 Then
        If Agg1 <= NumLiving Then       'Both are living
            EE(Agg1) = (EE(Agg1) * B(Agg1) + EE(Agg2) * B(Agg2)) / (B(Agg1) + B(Agg2))
            If QB(Agg1) > 0 Or QB(Agg2) > 0 Then    'Weighted after consumption
                GS(Agg1) = (GS(Agg1) * B(Agg1) * QB(Agg1) + GS(Agg2) * B(Agg2) * QB(Agg2)) / (B(Agg1) * QB(Agg1) + B(Agg2) * QB(Agg2))
            End If
        End If
        Shadow(Agg1) = (Shadow(Agg1) * B(Agg1) + Shadow(Agg2) * B(Agg2)) / (B(Agg1) + B(Agg2))
    End If

    If Agg1 <= NumLiving Then          'diet comp for living groups
        If QB(Agg1) > 0 Or QB(Agg2) > 0 Then
            SumCons = QB(Agg1) * B(Agg1) + QB(Agg2) * B(Agg2)
            For i = 1 To NumGroups          'prey
                If B(i) > 0 Then        'Exclude aggregated groups
                    If SumCons > 0 Then 'Eaten by the two groups of all prey
                        DCi(Agg1, i) = (DCi(Agg1, i) * QB(Agg1) * B(Agg1) + DCi(Agg2, i) * QB(Agg2) * B(Agg2)) / SumCons
                        If DCi(Agg1, i) > 0 Then DietWasChanged Agg1, i
                    End If
                End If
            Next i
            If SumCons > 0 Then DCi(Agg1, 0) = (DCi(Agg1, 0) * QB(Agg1) * B(Agg1) + DCi(Agg2, 0) * QB(Agg2) * B(Agg2)) / SumCons
            ' Calculate the DC(Agg1,Import)
            If (B(Agg1) + B(Agg2)) > 0 Then 'QB aggregation is only for living groups
                QBi(Agg1) = (QB(Agg1) * B(Agg1) + QB(Agg2) * B(Agg2)) / (B(Agg1) + B(Agg2))
            End If
        End If
    Else        'It's detritus do dtimp:
        DtImp(Agg1) = DtImp(Agg1) + DtImp(Agg2)
    End If

    For i = 1 To NumGroups
        DCi(i, Agg1) = DCi(i, Agg1) + DCi(i, Agg2)
        If DCi(i, Agg1) > 0 Then DietWasChanged i, Agg1
    Next i                 'Eaten by predators of the two groups

    'Landing and discards are just summed
    For i = 1 To NumGear
        If Landing(i, Agg1) + Landing(i, Agg2) > 0 Then
            Market(i, Agg1) = (Market(i, Agg1) * Landing(i, Agg1) + Market(i, Agg2) * Landing(i, Agg2)) / (Landing(i, Agg1) + Landing(i, Agg2))
        End If
        Landing(i, Agg1) = Landing(i, Agg1) + Landing(i, Agg2)
        Discard(i, Agg1) = Discard(i, Agg1) + Discard(i, Agg2)
    Next
    'Biomasses are not required for detritus groups
    'total biomass:
    Bi(Agg1) = B(Agg1) + B(Agg2)
    'Cannot know which combined area we are talking about, so use the area for the first group:
    BHi(Agg1) = Bi(Agg1) / Area(Agg1)
    Bi(Agg2) = 0
    BHi(Agg2) = 0
    'Aggregate BasicRemarks()
    For i = 2 To 10
        AggregateRemarks "[BasicParam Remarks]", "groupName", "remarks", "RefCode", "paramNum", CStr(i)
    Next
    'Aggregate Catch
    For i = 1 To NumGear
        AggregateRemarks "[Catch]", "groupName", "remarksCatch", "RefCodeCatch", "gearName", GearName(i)
        AggregateRemarks "[Catch]", "groupName", "remarksPrice", "RefCodePrice", "gearName", GearName(i)
        AggregateRemarks "[Catch]", "groupName", "remarksDiscards", "RefCodeDiscards", "gearName", GearName(i)
    Next

    If Agg1 > NumLiving Then    'Detritus group so aggregate discardfate
        For i = 1 To NumGear
            AggregateRemarks "[Discard Fate]", "groupColName", "remarksCatch", "RefCodeCatch", "gearName", GearName(i)
        Next
    End If

    'Ecosim scenario is lacking
    'Ecospace scenario is missing
    AggregateRemarks "[Output param]", "groupName", "remarks", "RefCode"

    'Remarks in species list are not needed and can be removed
    'AggregateRemarks "[Species List]", "groupName", "remarks", "RefCode"
    AggregateSpeciesList
End Sub

Public Sub DoDeleteGroup()
On Local Error Resume Next
Dim SQL As String
    For K = NumGroups To 1 Step -1
        If GroupForDeletion(K) Then 'go ahead with deletion from the mdb /vc170398
            SQL = "SELECT * from [Group Info] where modelName='" & lastModel & "' and groupName='" & Trim(Specie(K)) & "'"
            Set g_Recordset = CCG.UpdatableRecords(SQL)
            g_Recordset.Delete
            'Also delete from forms
            'DeleteGroupFromRemarks Trim(Specie(k))
            frmInputData.DeleteGroup K, K > NumLiving, True
        End If
    Next
End Sub

Private Sub UpdateValuesOnInputFormAndInDataBase(Grid As vaSpread)
'grid is frminputdata.vainput
Dim SQL As String
    'Groupname on form and database has been update already
    'SetCellValue Grid, 2, Agg1, Format(PP(Agg1), "0.0")
    'If Not NotInput(Agg1, 1) Then SetCellValue Grid, 3, Agg1, if(B(Agg1) > 0, Format(B(Agg1), GenNum), "")
    'If Not NotInput(Agg1, 2) Then SetCellValue Grid, 4, Agg1, if(PB(Agg1) > 0 And Agg1 <= NumLiving, Format(PB(Agg1), GenNum), "")
    'If Not NotInput(Agg1, 3) Then SetCellValue Grid, 5, Agg1, if(QB(Agg1) > 0 And Agg1 <= NumLiving, Format(QB(Agg1), GenNum), "")
    'If Not NotInput(Agg1, 4) Then SetCellValue Grid, 6, Agg1, if(EE(Agg1) >= 0 And Agg1 <= NumLiving, Format(EE(Agg1), GenNum), "")
    'If Not NotInput(Agg1, 0) Then SetCellValue Grid, 7, Agg1, if(GE(Agg1) > 0 And Agg1 <= NumLiving, Format(GE(Agg1), GenNum), "")
    'SetCellValue Grid, 8, Agg1, if(Agg1 <= NumLiving, Format(BA(Agg1), GenNum), "")
    'SetCellValue Grid, 9, Agg1, if(Agg1 <= NumLiving, Format(GS(Agg1), GenNum), "")
    'SetCellValue Grid, 10, Agg1, if(Agg1 <= NumLiving, "", Format(DtImp(Agg1), GenNum))
    'Also update the database   [Group Info]
    SaveGroupInfo Specie(Agg1), Agg1    ', True
    '(GrpName As String, Group As Integer, EditOnly As Boolean)
    UpdateDiet
    'SaveDietComp
    SaveDetritusFate
    SaveFisheryInfo lastModel
    SaveCatches 'True
    SaveStanza
    SaveDiscardFate
End Sub

Private Sub UpdateSpeciesSequence()
Dim i As Integer
Dim SQL As String
    SQL = "SELECT * from [Group Info] where modelName='" + lastModel + "'  ORDER BY [Group Info].Sequence"
    Set g_Recordset = CCG.UpdatableRecords(SQL)  ' g_databas.OpenRecordset(SQL)
    i = 1
    g_Recordset.MoveFirst
    Do While Not g_Recordset.EOF
        Specie(i) = g_Recordset.Fields("groupName").value
        g_Recordset.Fields("Sequence").value = i
        i = i + 1
        g_Recordset.Update
        g_Recordset.MoveNext
    Loop
End Sub

Private Sub AggregateRemarks(Table As String, groupField As String, RemField As String, RefField As String, Optional Field1 As String, Optional Name As String, Optional sceneField As String, Optional sceneName As String)
Dim SQL As String
Dim Remark1 As String
Dim Remark2 As String
Dim Ref1 As Long
Dim Ref2 As Long
Dim QRef As String
    'First Agg1:
    SQL = "SELECT * from " + Table + " where modelName='" + lastModel
    SQL = SQL + "' and " + groupField + "= '" + Specie(Agg1) + "'"
    If Field1 <> "" Then SQL = SQL + " and " + Field1 + "= '" + Name + "'"
    If sceneField <> "" Then SQL = SQL + " and " + sceneField + "= '" + sceneName + "'"
    Set g_Recordset = CCG.UpdatableRecords(SQL)  ' g_databas.OpenRecordset(SQL)
    If g_Recordset.RecordCount > 0 Then
        If IsNull(g_Recordset.Fields(RemField).value) Then
            Remark1 = ""
        Else
            Remark1 = g_Recordset.Fields(RemField).value
        End If
        If IsNull(g_Recordset.Fields(RefField).value) Then
            Ref1 = 0
        Else
            Ref1 = g_Recordset.Fields(RefField).value
        End If
    Else
        Remark1 = ""
        Ref1 = 0
    End If

    'Next Agg2:
    SQL = "SELECT * from " + Table + " where modelName='" + lastModel
    SQL = SQL + "' and " + groupField + "= '" + Specie(Agg1) + "'"
    If Field1 <> "" Then SQL = SQL + " and " + Field1 + "= '" + Name + "'"
    If sceneField <> "" Then SQL = SQL + " and " + sceneField + "= '" + sceneName + "'"
    Set x_Recordset = CCG.UpdatableRecords(SQL)  ' g_databas.OpenRecordset(SQL)
    If x_Recordset.RecordCount > 0 Then
        If IsNull(x_Recordset.Fields(RemField).value) Then
            Remark2 = ""
        Else
            Remark2 = x_Recordset.Fields(RemField).value
        End If
        If IsNull(x_Recordset.Fields(RefField).value) Then
            Ref2 = 0
        Else
            Ref2 = x_Recordset.Fields(RefField).value
        End If
    Else
        Remark2 = ""
        Ref2 = 0
    End If

    'Got the rem and ref so update the database:
    If Ref1 > 0 Or Ref2 > 0 Then
        If Ref1 = 0 Then    'Only a reference for Agg2:
            g_Recordset.Fields(RefField).value = Ref2
            Ref2 = 0
        Else    'A ref is present for Agg1
            g_Recordset.Fields(RefField).value = Ref1
        End If
        g_Recordset.Update
    End If
    QRef = ""
    If Remark1 <> "" Or Remark2 <> "" Then
        If Ref2 > 0 Then QRef = ";  " + frmReferences.GetQuickRef(Ref2)
        g_Recordset.Fields(RemField).value = Remark1 + "; " + Remark2 + QRef
        g_Recordset.Update
    End If

End Sub

Private Sub AggregateSpeciesList()
Dim SQL As String
Dim TaxCode As Long
Dim prop As Single
Dim Name As String
Dim Remark1 As String
Dim Remark2 As String
Dim Ref1 As Long
Dim Ref2 As Long
Dim QRef As String
    'First Agg1:
    'Agg2:
    SQL = "SELECT * from [Group Taxon] where modelName='" + lastModel
    SQL = SQL + "' and groupName= '" + Specie(Agg2) + "'"
    Set x_Recordset = CCG.UpdatableRecords(SQL)

    If x_Recordset.RecordCount > 0 Then
        x_Recordset.MoveFirst
        Do While Not x_Recordset.EOF
            TaxCode = x_Recordset.Fields("code").value
            Name = x_Recordset.Fields("name").value
            prop = x_Recordset.Fields("proportion").value
            SQL = "SELECT * from [Group Taxon] where modelName='" + lastModel
            SQL = SQL + "' and groupName= '" + Specie(Agg2)
            SQL = SQL + "' and code=" + CStr(TaxCode)
            Set g_Recordset = CCG.UpdatableRecords(SQL)
            If g_Recordset.RecordCount = 0 Then g_Recordset.AddNew
            g_Recordset.Fields("modelName").value = lastModel
            g_Recordset.Fields("groupName").value = Specie(Agg2)
            g_Recordset.Fields("code").value = TaxCode
            g_Recordset.Fields("name").value = Name
            g_Recordset.Fields("proportion").value = prop
            g_Recordset.Update
            x_Recordset.MoveNext
        Loop
    End If
End Sub

#End If
#End Region ' Original code

    End Class

End Namespace
