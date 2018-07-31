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

Public Class cENACellData
    Inherits cRowCol

    Public nGroups As Integer
    Public ENARData As cENARDataStructures
    Public iTimeStep As Integer

    Private m_epdata As cEcopathDataStructures


    Public Sub New(ByVal theRow As Integer, ByVal theCol As Integer)
        MyBase.New(theRow, theCol)

    End Sub


    Public Sub Init(SpaceData As cEcospaceDataStructures, EcoPathData As cEcopathDataStructures)
        nGroups = SpaceData.NGroups

        Me.ENARData = New cENARDataStructures(nGroups)
        Me.m_epdata = EcoPathData

    End Sub

    Public Sub Populate(iTime As Integer, SpaceData As cEcospaceDataStructures, ByVal Biomass() As Single, Production() As Single, consumpt(,) As Single,
                        FishingMort() As Single, EatenOf() As Single, FlowToDertitus() As Single, DetritusFlowByGroup() As Single, TotfisheriesDiscards As Single)

        'System.Console.WriteLine("Populating ENA Data " + Me.Key)

        Debug.Assert(Biomass.Length >= nGroups, "enaR Populate(...) Number of groups in enaR and Core is not the same! This will cause problems!")
        Try

            Me.iTimeStep = iTime
            Array.Copy(Biomass, Me.ENARData.b, nGroups)
            Array.Copy(consumpt, Me.ENARData.Consumpt, nGroups)

            For igrp As Integer = 1 To nGroups

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'IMPORTS / PRODUCTION
                If m_epdata.PP(igrp) = 1 Then
                    Me.ENARData.Import(igrp) = Production(igrp)
                Else
                    'Imported consumption from Ecopath
                    'impConsump = CSng(m_EcoPathData.B(iGroup) * m_EcoPathData.QB(iGroup) * m_EcoPathData.DC(iGroup, 0))
                    Me.ENARData.Import(igrp) = Biomass(igrp) * m_epdata.QB(igrp) * m_epdata.DC(igrp, 0)
                End If

                If igrp = Me.m_epdata.NumLiving + 1 Then
                    Me.ENARData.Import(igrp) = TotfisheriesDiscards
                End If
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'RESPIRATION
                If m_epdata.PP(igrp) < 1 Then 'only for consumers

                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    'Respiration Calcualtion from cEcoNetwork.PrepareUlanowForCallFromEcosim()
                    'This does not result in the same respiration as Ecopath
                    'If m_epdata.GE(i) > 0 Then SimQB(i) = SimPB(i) / m_epdata.GE(i)
                    'SimIm(i) = m_epdata.DC(i, 0) * SimQB(i)
                    'SimResp(i) = BB(i) * (SimQB(i) - SimPB(i) - m_epdata.GS(i))
                    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    'Respiration calculation from Ecopath
                    Dim consump As Single = Biomass(igrp) * Me.m_epdata.QB(igrp)
                    Me.ENARData.Resp(igrp) = consump - Production(igrp) - (consump * Me.m_epdata.GS(igrp))
                    'constrain Respiration to > 0
                    If Me.ENARData.Resp(igrp) < 0 Then Me.ENARData.Resp(igrp) = 0
                End If
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'EXPORTS

                Dim sumCatch As Single
                If igrp <= Me.m_epdata.NumLiving Then
                    Me.ENARData.CatchExport(igrp) = FishingMort(igrp) * Biomass(igrp)

                    sumCatch += Me.ENARData.CatchExport(igrp) ' * Me.m_epdata.PropDiscard(iflt, igrp) * Me.m_epdata.PropDiscardMort(iflt, igrp)

                Else

                    'Export of Detritus
                    'cEcopathModel.CalcExportOfDetritus()
                    Dim ExpDet As Single = FlowToDertitus(igrp - m_epdata.NumLiving) - m_epdata.BA(igrp) - EatenOf(igrp) - Me.ENARData.Resp(igrp)
                    If ExpDet > 0 Then
                        Me.ENARData.CatchExport(igrp) = ExpDet - sumCatch
                    End If

                    If Me.ENARData.CatchExport(igrp) < 0 Then Me.ENARData.CatchExport(igrp) = 0.0

                End If
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'FLOW TO DETRITUS (CONSUMPTION)

                'Det to Det
                Dim ndet As Integer = nGroups - Me.m_epdata.NumLiving
                If igrp > Me.m_epdata.NumLiving Then

                    For iDetGrp As Integer = 1 To ndet
                        If Me.m_epdata.DF(igrp, iDetGrp) <> 0 Then

                            Me.ENARData.Consumpt(igrp, Me.m_epdata.NumLiving + iDetGrp) = FlowToDertitus(igrp - Me.m_epdata.NumLiving)

                        End If
                    Next iDetGrp

                End If 'igrp > Me.m_epdata.NumLiving

                'Flow to Detritus from a group
                If igrp <= Me.m_epdata.NumLiving Then

                    For ii As Integer = 1 To ndet
                        If Me.m_epdata.DC(Me.m_epdata.NumLiving + ii, igrp) > 0 Then
                            Me.ENARData.Consumpt(igrp, Me.m_epdata.NumLiving + ii) = DetritusFlowByGroup(igrp)
                        End If
                    Next ii

                End If 'igrp <= Me.m_epdata.NumLiving

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            Next igrp

        Catch ex As Exception
            Debug.Assert(False, "Exception in " + Me.ToString + ".Populate(): " + ex.Message)
        End Try

    End Sub

    Public Shared Function getHash(irow As Integer, icol As Integer) As String
        Return irow.ToString + "_" + icol.ToString
    End Function

    Public Function Key() As String
        Return cENACellData.getHash(Me.Row, Me.Col)
    End Function

    Public Function toFileName(theCore As cCore) As String
        Dim fname As String = theCore.EwEModel.Name + "_" + theCore.EcospaceScenarios(theCore.ActiveEcospaceScenarioIndex).Name + ".txt"
        Dim row As String = Me.toFormattedString(Me.Row, 3)
        Dim col As String = Me.toFormattedString(Me.Col, 3)

        Dim fp As String = Path.Combine(theCore.DefaultOutputPath(eAutosaveTypes.Ecospace), "ena_data", "TimeStep=" + toFormattedString(iTimeStep, 4))

        If Not Directory.Exists(fp) Then
            Directory.CreateDirectory(fp)
        End If

        Return Path.Combine(fp, row + "-" + col + "-" + fname)

    End Function

    Function toFormattedString(value As Integer, nZeros As Integer) As String
        Dim FormatedNum As String = EwEUtils.Utilities.cStringUtils.FormatInteger(value)
        Dim n As Integer = FormatedNum.Length
        For i As Integer = n To nZeros
            FormatedNum = "0" + FormatedNum
        Next
        Return FormatedNum
    End Function


#Region "ENA code from EwE Used as reference"

#If 0 Then
     
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        ''TLCatch gets computed for each time step
        ''it will need to be stored
        'Dim relCatch As Single 'relative catch 
        'Dim esData As cEcosimDatastructures = DirectCast(EcosimDatastructures, cEcosimDatastructures)

        'Try

        '    'get a local copy of the biomass computed by ecosim at this time step
        '    Array.Copy(BiomassAtTimestep, BB, BiomassAtTimestep.Length)

        '    ' If ipct = 6 Then Estimate_Taxon_indices(CInt((iTime - ipct) / 12))
        '    'EstimateTLsInEcosim(iTime, True)
        '    'EstimateTLofCatch(iTime, BB, CatchSim, TLCatch, relCatch, True)         'Orig total catch =catchsum

        '    ' JS 08Jan10: this used to be in EstimateTLofCatch
        '    If iTime = 0 Then
        '        relCatch = 0
        '    ElseIf iTime = 1 Then
        '        relCatch = 1
        '    Else
        '        relCatch = esData.CatchSim(iTime) / esData.CatchSim(1)
        '    End If

        '    PrepareUlanowForCallFromEcosim(iTime)

        '    'Summary data
        '    'see EwE5 RunModel() "If IndicesOn Then"
        '    RelativeSumOfCatch(iTime) = relCatch
        '    RelativeDiversityIndex(iTime) = Me.DiversityIndex(iTime) / OrigDiversityIndex

        '    TLCatch(iTime) = esData.TLC(iTime)

        '    For igrp As Integer = 1 To m_epdata.NumGroups
        '        TLSim(igrp, iTime) = esData.TLSim(igrp)
        '    Next igrp

        '    If PPRon Then
        '        RelativeCatchPPR(iTime) = RaiseToPP(0) / OrigPPR(0)
        '        RelativeCatchDetReq(iTime) = RaiseToDet(0) / OrigPPR(1)

        '        Dim LIndexTot As Single = 0
        '        For i As Integer = 1 To Me.m_core.nLivingGroups
        '            LIndexTot += Me.m_manager.LindexSim(i)
        '        Next

        '        AbsoluteLIndex(iTime) = LIndexTot
        '        RelativeLIndex(iTime) = AbsoluteLIndex(iTime) / AbsoluteLIndex(1)

        '        AbsolutePsust(iTime) = Me.m_manager.CalcPsust(LIndexTot)
        '        RelativePsust(iTime) = AbsolutePsust(iTime) / AbsolutePsust(1)
        '    End If

        'Catch ex As Exception
        '    cLog.Write(ex)
        '    Debug.Assert(False, ex.StackTrace)
        '    Return False
        'End Try

        'Return True

        'xxxxxxxxxxxxxxxxxxxxxxx
        'From cEcoNetwork.PrepareUlanowForCallFromEcosim()
        'For i = 1 To m_epdata.NumLiving
        '    SimB(i) = BB(i)
        '    SimPB(i) = m_esdata.loss(i) / BB(i)

        '    SimCatch(i) = BB(i) * m_esdata.FishTime(i)
        '    BEmig = m_epdata.Emig(i) * BB(i)

        '    ToDo_jb 5 - Jan - 2010 NetworkAnalysis PrepareUlanowForCallFromEcosim calculation of simEE uses fishtime() as catch
        '    m_esdata.FishTime(i) is a rate not an amount it should use SimCatch() BB(i) * m_esdata.FishTime(i)
        '     SimEE(i) = 1 - (m_esdata.loss(i) - m_esdata.Eatenof(i) - m_esdata.FishTime(i)) / (SimPB(i) * BB(i))
        '    SimEE(i) = 1 - (m_esdata.loss(i) - m_esdata.Eatenof(i) - SimCatch(i)) / (SimPB(i) * BB(i))

        '    If m_epdata.PP(i) < 1 Then 'only for consumers
        '        If m_epdata.GE(i) > 0 Then SimQB(i) = SimPB(i) / m_epdata.GE(i)
        '        SimIm(i) = m_epdata.DC(i, 0) * SimQB(i)
        '        SimResp(i) = BB(i) * (SimQB(i) - SimPB(i) - m_epdata.GS(i))
        '    End If
        '    SimEx(i) = SimCatch(i)

        '    fCatch = fCatch + SimEx(i)
        '    Biom = Biom + BB(i)
        '    Production = Production + BB(i) * SimPB(i)
        '    If m_epdata.PP(i) > 0 Then PProd = PProd + SimPB(i) * BB(i) * m_epdata.PP(i)
        'Next
        'For i = m_epdata.NumLiving + 1 To m_epdata.NumGroups
        '    SimB(i) = BB(i)
        '    If m_esdata.ToDetritus(i - m_epdata.NumLiving) > 0 Then SimEE(i) = m_esdata.loss(i) / m_esdata.ToDetritus(i - m_epdata.NumLiving)
        '    Emig and Imig removed from below, zero for detritus-
        '    SimEx(i) = (m_esdata.ToDetritus(i - m_epdata.NumLiving) - m_epdata.BA(i) - m_esdata.Eatenof(i)) / BB(i)
        'Next i

        'For ii = 1 To m_esdata.inlinks
        '    i = m_esdata.ilink(ii) : j = m_esdata.jlink(ii)
        '    If m_esdata.Eatenby(j) > 0 Then SDiet(j, i) = m_esdata.DCMean(j, i) / m_esdata.Eatenby(j)
        '    jb Consumpt() contains the same values as DCMean 
        '    DCmean is not used anywhere else so use Consumpt() instead
        '    If m_esdata.Eatenby(j) > 0 Then SDiet(j, i) = m_esdata.Consumpt(i, j) / m_esdata.Eatenby(j)
        'Next
        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
#End If

#End Region

End Class