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
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Ecopath

    Public Class cEcopathSplitGroup
        Inherits cEcopathMergeSplitGroups

        Public Sub New(core As cCore)
            Me.m_core = core
        End Sub

#Region " Public access "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the current Ecopath model is ready to split a group to a 
        ''' candidate name.
        ''' </summary>
        ''' <param name="iGroup">The one-based index of the group to split.</param>
        ''' <param name="strName">A suggested name for the split groups.</param>
        ''' <returns>True if the proposed split can be executed.</returns>
        ''' -----------------------------------------------------------------------
        Public Function CanSplitGroups(iGroup As Integer, strName As String) As Boolean

            Dim epdata As cEcopathDataStructures = Me.m_core.m_EcoPathData

            Return (Me.CanMergeSplitGroups(False) = True) And
               (-1 = Array.IndexOf(epdata.GroupName, strName.Trim())) And
               (Not String.IsNullOrWhiteSpace(strName))

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Split a non-stanza group.
        ''' </summary>
        ''' <param name="iSrc"><see cref="cCoreGroupBase.Index">index</see> of 
        ''' the group split.</param>
        ''' <param name="strNameSplit">Name for the new split group.</param>
        ''' <param name="sBiomassSource"></param>
        ''' <param name="sBiomassSplit"></param>
        ''' <param name="taxaSplit"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function SplitGroup(iSrc As Integer, strNameSplit As String,
                                   sBiomassSource As Single, sBiomassSplit As Single,
                                   taxaSplit As cTaxon()) As Boolean

            If (Me.m_core.getStanzaIndexForGroup(iSrc) >= 0) Then
                Debug.Assert(False, "Cannot split stanza group with non-stanza split method")
                Return False
            End If

            Dim epdata As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim txdata As cTaxonDataStructures = Me.m_core.m_TaxonData
            Dim bSuccess As Boolean = True

            Dim iSrcNew As Integer = 0
            Dim iTgtNew As Integer = 0

            bSuccess = Me.SplitAndCopy(iSrc, strNameSplit, iSrcNew, iTgtNew)

            If (bSuccess) Then

                ' Set biomasses
                epdata.Binput(iSrcNew) = sBiomassSource
                epdata.Binput(iTgtNew) = sBiomassSplit

                ' Reallocate taxa
                For Each taxon As cTaxon In taxaSplit
                    Debug.Assert(txdata.TaxonTarget(taxon.Index) = iSrcNew)
                    txdata.TaxonTarget(taxon.Index) = iTgtNew
                Next

                Me.m_core.DataSource.SetChanged(eCoreComponentType.EcoPath)
                Me.m_core.StateMonitor.UpdateDataState(Me.m_core.DataSource)
                Me.m_core.SaveChanges(True)

                Me.m_core.LoadModel(Me.m_core.DataSource)

            End If

            If bSuccess Then
                Dim strName As String = epdata.GroupName(iSrcNew)
                Me.SendMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_GROUPSPLIT_SUCCESS, strName, strName, strNameSplit), True)
            Else
                ' Report failure?
            End If

            Return bSuccess

        End Function

        ''' <summary>
        ''' Split a stanza life stage.
        ''' </summary>
        ''' <param name="iSrc"></param>
        ''' <param name="strNameSplit"></param>
        ''' <param name="iStartAgeSplit"></param>
        ''' <returns></returns>
        Public Function SplitLifeStage(iSrc As Integer, strNameSplit As String, iStartAgeSplit As Integer) As Boolean

            If (Me.m_core.getStanzaIndexForGroup(iSrc) < 0) Then
                Debug.Assert(False, "Cannot split regular group with stanza split method")
                Return False
            End If

            Dim epdata As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim bSuccess As Boolean = True

            Dim iSrcNew As Integer = 0
            Dim iTgtNew As Integer = 0

            bSuccess = Me.SplitAndCopy(iSrc, strNameSplit, iSrcNew, iTgtNew)

            If (bSuccess) Then

                Dim iGroupDBID As Integer = epdata.GroupDBID(iTgtNew)
                Dim iStanza As Integer = Me.m_core.getStanzaIndexForGroup(iSrcNew)
                bSuccess = Me.m_core.AddStanzaLifestage(iStanza, iGroupDBID, iStartAgeSplit)

            End If

            If bSuccess Then
                Dim strName As String = epdata.GroupName(iSrcNew)
                Me.SendMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOPATH_GROUPSPLIT_SUCCESS, strName, strName, strNameSplit), True)
                Me.m_core.SaveChanges(True)
            Else
                ' Report failure?
            End If

            Return bSuccess

        End Function

#End Region ' Public access

#Region " Internals "

        Private Function SplitAndCopy(iSrc As Integer, strNameSplit As String, ByRef iSrcNew As Integer, ByRef iTgtNew As Integer) As Boolean

            Dim epdata As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim psdata As cPSDDatastructures = Me.m_core.m_PSDData

            Dim iDBIDsrc As Integer = epdata.GroupDBID(iSrc)
            Dim iDBIDtgt As Integer = -1
            Dim iTgt As Integer = iSrc

            If Not Me.m_core.SaveChanges(False) Then Return False
            If Not Me.m_core.AddGroup(strNameSplit, epdata.PP(iSrc), epdata.vbK(iSrc), iTgt, iDBIDtgt) Then Return False

            iSrc = Array.IndexOf(epdata.GroupDBID, iDBIDsrc)
            iTgt = Array.IndexOf(epdata.GroupDBID, iDBIDtgt)

            epdata.EEinput(iTgt) = epdata.EEinput(iSrc)
            epdata.OtherMortinput(iTgt) = epdata.OtherMortinput(iSrc)
            epdata.QBinput(iTgt) = epdata.QBinput(iSrc)
            epdata.PBinput(iTgt) = epdata.PBinput(iSrc)
            epdata.GEinput(iTgt) = epdata.GEinput(iSrc)

            epdata.Area(iTgt) = epdata.Area(iSrc)
            epdata.GS(iTgt) = epdata.GS(iSrc)
            epdata.DtImp(iTgt) = epdata.DtImp(iSrc)
            epdata.BaBi(iTgt) = epdata.BaBi(iSrc)
            If (epdata.Immig(iSrc) > 0) Then epdata.Immig(iSrc) /= 2 : epdata.Immig(iTgt) = epdata.Immig(iSrc)
            epdata.Emig(iTgt) = epdata.Emig(iSrc)
            If (epdata.BAInput(iTgt) > 0) Then epdata.BAInput(iSrc) /= 2 : epdata.BAInput(iTgt) = epdata.BAInput(iSrc)
            If (epdata.Emigration(iSrc) > 0) Then epdata.Emigration(iSrc) /= 2 : epdata.Emigration(iTgt) = epdata.Emigration(iSrc)
            epdata.Shadow(iTgt) = epdata.Shadow(iSrc)

            ' Diet
            For iPrey As Integer = 1 To epdata.NumGroups
                epdata.DCInput(iTgt, iPrey) = epdata.DCInput(iSrc, iPrey)
            Next
            For iPred As Integer = 1 To epdata.NumLiving
                If epdata.DCInput(iPred, iSrc) > 0 Then epdata.DCInput(iPred, iSrc) /= 2 : epdata.DCInput(iPred, iTgt) = epdata.DCInput(iPred, iSrc)
            Next
            epdata.DCInput(iTgt, 0) = epdata.DCInput(iSrc, 0)

            ' Detritus fate
            If (iSrc > epdata.NumLiving) Then
                For i As Integer = 1 To epdata.NumDetrit
                    epdata.DF(iTgt - epdata.NumLiving, i) = epdata.DF(iSrc - epdata.NumLiving, i)
                Next
            End If

            ' PSD
            psdata.AinLWInput(iTgt) = psdata.AinLWInput(iSrc)
            psdata.BinLWInput(iTgt) = psdata.BinLWInput(iSrc)
            psdata.LooInput(iTgt) = psdata.LooInput(iSrc)
            psdata.WinfInput(iTgt) = psdata.WinfInput(iSrc)
            psdata.t0Input(iTgt) = psdata.t0Input(iSrc)
            psdata.TcatchInput(iTgt) = psdata.TcatchInput(iSrc)
            psdata.TmaxInput(iTgt) = psdata.TmaxInput(iSrc)

            ' Fishery
            For iFleet As Integer = 1 To epdata.NumFleet
                If (epdata.Landing(iFleet, iSrc) > 0) Then epdata.Landing(iFleet, iSrc) /= 2 : epdata.Landing(iFleet, iTgt) = epdata.Landing(iFleet, iSrc)
                If (epdata.Discard(iFleet, iSrc) > 0) Then epdata.Discard(iFleet, iSrc) /= 2 : epdata.Discard(iFleet, iTgt) = epdata.Discard(iFleet, iSrc)
                epdata.PropDiscard(iFleet, iTgt) = epdata.PropDiscard(iFleet, iSrc)
                epdata.PropDiscardMort(iFleet, iTgt) = epdata.PropDiscardMort(iFleet, iSrc)
                epdata.Market(iFleet, iTgt) = epdata.Market(iFleet, iSrc)
            Next

            ' Pedigree

            ' Remarks

            ' Pass the word
            iTgtNew = iTgt
            iSrcNew = iSrc

            Return True

        End Function

#End Region ' Internals

    End Class

End Namespace

