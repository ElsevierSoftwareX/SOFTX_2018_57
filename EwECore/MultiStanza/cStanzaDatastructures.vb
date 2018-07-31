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

Imports EwEUtils.Core
Imports EwEUtils.Utilities

Public Class cStanzaDatastructures

    'this changes how many packets are used per age (default 0.5)
    '# packets = # cells * NPacketsMultiplier
    Public NPacketsMultiplier As Single

    'These are new IBM variables
    Public EggAtSpawn() As Boolean
    Public EggCell(,,) As Single 'eggs per cell and species
    Public AgeIndex1() As Integer ' index of the age one creatures of a species in Npacket and Wpacket
    Public StanzaNo(,) As Integer
    Public Npacket(,,) As Single 'number of fish in the packet (species, age, packet#)
    Public Wpacket(,,) As Single ' weight of fish in the packet
    Public IBMMovesPerMonth() As Integer
    Public IBMdistmove(,) As Single

    ''' <summary>
    ''' i (map row) position index of the packet
    ''' </summary>
    Public iPacket(,,) As Single

    ''' <summary>
    ''' i (map col) position index of the packet
    ''' </summary>
    Public jPacket(,,) As Single

    ''' <summary>
    ''' Row Index in map of cells used for a nursery. Cells that are populated by the first life stage of a multi-stanza group.
    ''' </summary>
    ''' <remarks>Populated in <see cref="cEcoSpace.InitPackets"> InitPackets</see></remarks>
    Public iNursery(,) As Integer

    ''' <summary>
    ''' Col Index in map of cells used for a nursery. Cells that are populated by the first life stage of a multi-stanza group.
    ''' </summary>
    ''' <remarks>Populated in <see cref="cEcoSpace.InitPackets"> InitPackets</see></remarks>
    Public jNursery(,) As Integer

    ''' <summary>
    ''' Total number of Nersery cell in the map by Multi-stanza groups
    ''' </summary>
    ''' <remarks>Populated in <see cref="cEcoSpace.InitPackets"> InitPackets</see></remarks>
    Public Nnursery() As Integer

    Public Zcell(,,) As Single   'mortality rate by cell and species
    Public MaxAgeSpecies() As Integer
    Public Npackets As Integer  'total # of packets per age

    ''' <summary>Max number of stanazs across all the stanza groups.</summary>
    Public MaxStanza As Integer
    ''' <summary>The number of stanza groups (split groups).</summary>
    Public Nsplit As Integer
    ''' <summary>For redimensioning SpeciesCode.</summary>
    Public nGroups As Integer '

    Public StanzaDBID() As Integer

    Public BaseStanza() As Integer
    Public BaseStanzaCB() As Integer
    Public BABsplit() As Single

    ''' <summary>Number of stanzas in each split group (one-based).</summary>
    Public Nstanza() As Integer
    ''' <summary>Group index (iGroup) for this (Nsplit, nStanza).</summary>
    Public EcopathCode(,) As Integer
    Public MaxAgeSplit As Integer
    Public NumSplit(,) As Single
    Public SplitRflow(,) As Single
    ''' <summary>Numbers at age (dynamic) for split species (set in initialstate using ecopath base array SplitNo(isp,age)).</summary>
    Public NageS(,) As Single
    ''' <summary>Weights at age (dynamic)(set in initialstate) (set in initialstate using ecopath base array SplitWage(isp,age).</summary>
    Public WageS(,) As Single
    ''' <summary>Base recruitment to age 0 for split species.</summary>
    Public RzeroS() As Single
    Public SplitAlpha(,) As Single 'growth coefficients by split spp and age (set in initialstate)
    Public RscaleSplit() As Single
    Public EggsSplit(,) As Single
    Public Age1(,) As Integer
    Public Age2(,) As Integer
    Public SplitNo(,) As Single
    Public SplitWage(,) As Single
    Public WWa(,) As Single

    Public StanzaName() As String
    Public Stanza_Z(,) As Single
    Public Stanza_Bio(,) As Single
    Public Stanza_CB(,) As Single


    Public WmatWinf() As Single ' weight at maturity/ weight at infinity (max weight) from EwE5 interface
    Public EggsStanza() As Single

    ''' <summary>Boolean flag set in an interface.</summary>
    ''' <remarks>Used by SplitUpdate(b)</remarks>
    Public FixedFecundity() As Boolean
    Public BaseEggsStanza() As Single

    Public RecPowerSplit() As Single

    Public vBM() As Single
    Public HatchCode() As Integer

    Public EggProdShapeSplit() As Integer

    ''' <summary>Egg production shape is seasonal.</summary>
    Public EggProdIsSeasonal() As Boolean

    ''' <summary></summary>
    ''' <remarks>
    ''' <list type="bullet">
    ''' <item>0: Ecopath group no for this stanza.</item>
    ''' <item>1: Ecopath no for leading B stanza.</item>
    ''' <item>2: Ecopath no for leading QB stanza.</item>
    ''' </list>
    ''' </remarks>
    Public SpeciesCode(,) As Integer

#Region " Private data "

    Private m_messages As cMessagePublisher

#End Region ' Private data

    Public Sub New(ByVal CoreMessagePublisher As cMessagePublisher)
        Me.m_messages = CoreMessagePublisher
    End Sub

    ''' <summary>
    ''' Redimension the stanza arrays
    ''' </summary>
    Public Sub redimStanza()

        ReDim StanzaDBID(Nsplit)

        ReDim RecPowerSplit(Nsplit)
        ReDim Nstanza(Nsplit) 'number of stanzas by split species (set in ecopath)
        ReDim BaseStanza(Nsplit) 'holds stanzano for which info is entered
        ReDim BaseStanzaCB(Nsplit)
        ReDim EcopathCode(Nsplit, MaxStanza) 'ecopath group# by split species, stanza (set in ecopath)
        ReDim StanzaName(Nsplit)
        ReDim Age1(Nsplit, MaxStanza) 'first month of age by species, stanza (set in ecopath)
        ReDim Age2(Nsplit, MaxStanza) 'last month of age by spp, stanza (set in ecopath)
        ReDim Stanza_Z(Nsplit, MaxStanza) 'mortality
        ReDim Stanza_Bio(Nsplit, MaxStanza) 'mortality
        ReDim Stanza_CB(Nsplit, MaxStanza) 'mortality
        ReDim RzeroS(Nsplit) 'base recruitment to age 0 for split species
        'redim PredS() 'effective predator abund for split species (set in ecosim splitpred)
        ReDim SplitAlpha(Nsplit, MaxAgeSplit) 'growth coefficients by split spp and age (set in initialstate)
        ReDim vBM(Nsplit)  'metabolic parameter 1-3*K by split species (set in ecopath)
        ' ReDim vBMann(Nsplit)
        ReDim WWa(Nsplit, MaxAgeSplit)
        ReDim SplitNo(Nsplit, MaxAgeSplit)
        ReDim SplitWage(Nsplit, MaxAgeSplit)
        ReDim HatchCode(Nsplit)
        ReDim WmatWinf(Nsplit)
        ReDim EggsStanza(Nsplit)
        ReDim FixedFecundity(Nsplit)
        ReDim BaseEggsStanza(Nsplit)
        ReDim EggProdShapeSplit(Nsplit)
        ReDim EggProdIsSeasonal(Nsplit)
        ReDim BABsplit(Nsplit)
        ReDim EggAtSpawn(Nsplit)

        'variables by nGroups
        ReDim SpeciesCode(nGroups, 2) '0: Ecopath group no for this stanza, 1: Ecopath no for leading B stanza, 2: Ecopath no for leading QB stanza

        ReDim WmatWinf(Nsplit)

    End Sub

    Public Sub Clear()
        Me.Nsplit = 0
        Me.nGroups = 0
        '  Me.redimStanza()

        Me.StanzaDBID = Nothing ' (Nsplit)

        Me.RecPowerSplit = Nothing ' (Nsplit)
        Me.Nstanza = Nothing ' (Nsplit) 'number of stanzas by split species  = nothing ' (set in ecopath)
        Me.BaseStanza = Nothing ' (Nsplit) 'holds stanzano for which info is entered
        Me.BaseStanzaCB = Nothing ' (Nsplit)
        Me.EcopathCode = Nothing ' (Nsplit, MaxStanza) 'ecopath group# by split species, stanza  = nothing ' (set in ecopath)
        Me.StanzaName = Nothing ' (Nsplit)
        Me.Age1 = Nothing ' (Nsplit, MaxStanza) 'first month of age by species, stanza  = nothing ' (set in ecopath)
        Me.Age2 = Nothing ' (Nsplit, MaxStanza) 'last month of age by spp, stanza  = nothing ' (set in ecopath)
        Me.Stanza_Z = Nothing ' (Nsplit, MaxStanza) 'mortality
        Me.Stanza_Bio = Nothing ' (Nsplit, MaxStanza) 'mortality
        Me.Stanza_CB = Nothing ' (Nsplit, MaxStanza) 'mortality
        Me.RzeroS = Nothing ' (Nsplit) 'base recruitment to age 0 for split species
        'me.PredS = nothing ' () 'effective predator abund for split species  = nothing ' (set in ecosim splitpred)
        Me.SplitAlpha = Nothing ' (Nsplit, MaxAgeSplit) 'growth coefficients by split spp and age  = nothing ' (set in initialstate)
        Me.vBM = Nothing ' (Nsplit)  'metabolic parameter 1-3*K by split species  = nothing ' (set in ecopath)
        ' me.vBMann = nothing ' (Nsplit)
        Me.WWa = Nothing ' (Nsplit, MaxAgeSplit)
        Me.SplitNo = Nothing ' (Nsplit, MaxAgeSplit)
        Me.SplitWage = Nothing ' (Nsplit, MaxAgeSplit)
        Me.HatchCode = Nothing ' (Nsplit)
        Me.WmatWinf = Nothing ' (Nsplit)
        Me.EggsStanza = Nothing ' (Nsplit)
        Me.FixedFecundity = Nothing ' (Nsplit)
        Me.BaseEggsStanza = Nothing ' (Nsplit)
        Me.EggProdShapeSplit = Nothing ' (Nsplit)
        Me.EggProdIsSeasonal = Nothing ' (Nsplit)
        Me.BABsplit = Nothing ' (Nsplit)

        'variables by nGroups
        Me.SpeciesCode = Nothing ' (nGroups, 2) '0: Ecopath group no for this stanza, 1: Ecopath no for leading B stanza, 2: Ecopath no for leading QB stanza

        Me.WmatWinf = Nothing ' (Nsplit)
        Me.NumSplit = Nothing ' (m_stanza.Nsplit, m_stanza.MaxAgeSplit)
        Me.NageS = Nothing ' (m_stanza.Nsplit, m_stanza.MaxAgeSplit)
        Me.WageS = Nothing ' (m_stanza.Nsplit, m_stanza.MaxAgeSplit)
        Me.SplitAlpha = Nothing ' (m_stanza.Nsplit, m_stanza.MaxAgeSplit)
        Me.SplitRflow = Nothing ' (m_stanza.Nsplit, m_stanza.MaxAgeSplit)
        Me.RscaleSplit = Nothing ' (m_stanza.Nsplit)
        Me.EggsSplit = Nothing ' (m_stanza.Nsplit, m_stanza.MaxAgeSplit)

    End Sub

    Public Sub copyTo(ByRef d As cStanzaDatastructures)
        Try
            d.MaxStanza = MaxStanza
            d.Nsplit = Nsplit
            d.nGroups = nGroups
            d.MaxAgeSplit = MaxAgeSplit

            d.redimStanza()

            NPacketsMultiplier = d.NPacketsMultiplier
            EggAtSpawn = d.EggAtSpawn
            Npackets = d.Npackets

            'EggCell.CopyTo(d.EggCell, 0)
            'AgeIndex1.CopyTo(d.AgeIndex1, 0)
            'StanzaNo.CopyTo(d.StanzaNo, 0)
            'Npacket.CopyTo(d.Npacket, 0)
            'Wpacket.CopyTo(d.Wpacket, 0)
            'IBMMovesPerMonth.CopyTo(d.IBMMovesPerMonth, 0)
            'IBMdistmove.CopyTo(d.IBMdistmove, 0)
            'iPacket.CopyTo(d.iPacket, 0)
            'jPacket.CopyTo(d.jPacket, 0)
            'iNursery.CopyTo(d.iNursery, 0)
            'jNursery.CopyTo(d.jNursery, 0)
            'Nnursery.CopyTo(d.Nnursery, 0)
            'Zcell.CopyTo(d.Zcell, 0)

            'MaxAgeSpecies.CopyTo(d.MaxAgeSpecies, 0)
            StanzaDBID.CopyTo(d.StanzaDBID, 0)

            BaseStanza.CopyTo(d.BaseStanza, 0)
            BaseStanzaCB.CopyTo(d.BaseStanzaCB, 0)
            BABsplit.CopyTo(d.BABsplit, 0)

            Nstanza.CopyTo(d.Nstanza, 0)
            d.EcopathCode = EcopathCode.Clone
            d.NumSplit = NumSplit.Clone
            d.SplitRflow = SplitRflow.Clone
            d.NageS = NageS.Clone
            d.WageS = WageS.Clone
            'RzeroS.CopyTo(d.RzeroS, 0)
            d.SplitAlpha = SplitAlpha.Clone
            d.RscaleSplit = RscaleSplit.Clone
            d.EggsSplit = EggsSplit.Clone
            d.Age1 = Age1.Clone
            d.Age2 = Age2.Clone
            d.SplitNo = SplitNo.Clone
            d.SplitWage = SplitWage.Clone
            d.WWa = WWa.Clone
            d.StanzaName = StanzaName.Clone
            d.Stanza_Z = Stanza_Z.Clone
            d.Stanza_Bio = Stanza_Bio.Clone
            d.Stanza_CB = Stanza_CB.Clone
            d.EggsStanza = EggsStanza.Clone


            d.FixedFecundity = FixedFecundity.Clone
            d.BaseEggsStanza = BaseEggsStanza.Clone
            d.RecPowerSplit = RecPowerSplit.Clone
            d.vBM = vBM.Clone
            d.HatchCode = HatchCode.Clone
            d.EggProdShapeSplit = EggProdShapeSplit.Clone
            d.EggProdIsSeasonal = EggProdIsSeasonal.Clone
            d.SpeciesCode = SpeciesCode.Clone

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    Public Sub OnPostInitialization()

        Dim msg As New cMessage(My.Resources.CoreMessages.STANZA_LEADING_ADJUSTED, eMessageType.DataModified, _
                                EwEUtils.Core.eCoreComponentType.EcoPath, eMessageImportance.Warning)
        Dim vs As cVariableStatus = Nothing
        Dim i As Integer = 0

        ' Fix leading B and CB if out of range
        For iStanza As Integer = 1 To Nsplit

            ' Assess B
            i = Math.Max(1, Math.Min(Nstanza(iStanza), Me.BaseStanza(iStanza)))
            If (i <> Me.BaseStanza(iStanza)) Then
                Me.BaseStanza(iStanza) = i
                vs = New cVariableStatus(eStatusFlags.MissingParameter, _
                                         cStringUtils.Localize(My.Resources.CoreMessages.STANZA_LEADINGB_ADJUSTED, Me.StanzaName(iStanza)), _
                                         EwEUtils.Core.eVarNameFlags.LeadingBiomass, EwEUtils.Core.eDataTypes.Stanza, EwEUtils.Core.eCoreComponentType.EcoPath, iStanza)
                msg.AddVariable(vs)
            End If

            ' Assess CB
            i = Math.Max(1, Math.Min(Nstanza(iStanza), Me.BaseStanzaCB(iStanza)))
            If (i <> Me.BaseStanzaCB(iStanza)) Then
                Me.BaseStanzaCB(iStanza) = i
                vs = New cVariableStatus(eStatusFlags.MissingParameter, _
                                         cStringUtils.Localize(My.Resources.CoreMessages.STANZA_LEADINGCB_ADJUSTED, Me.StanzaName(iStanza)), _
                                         EwEUtils.Core.eVarNameFlags.LeadingCB, EwEUtils.Core.eDataTypes.Stanza, EwEUtils.Core.eCoreComponentType.EcoPath, iStanza)
                msg.AddVariable(vs)
            End If

        Next

        If (msg.Variables.Count > 0) Then
            Me.m_messages.AddMessage(msg)
        End If
    End Sub

End Class


