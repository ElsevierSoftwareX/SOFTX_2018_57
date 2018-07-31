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
Imports EwECore
Imports EwEUtils.Core

Public Class cComputeLandingPortions

    Private m_ProducerReference(,) As Integer
    Private m_sumCatch() As Single
    Private m_sumValue() As Single

    Public Sub New(ByVal data As cData, ByVal iTimeStep As Integer, ByVal runType As cModel.eRunTypes)
        Compute(data, iTimeStep, runType)
    End Sub

    Private Function Compute(ByVal data As cData, ByVal iTimeStep As Integer, ByVal runType As cModel.eRunTypes) As Boolean

        Dim EwECore As cCore = data.Core
        Dim RunOK As Boolean

        'check if there's an ecosim scenario loaded:
        If EwECore.ActiveEcosimScenarioIndex > 0 Then
            'Dim res As New cResults(edEcostData)
            Dim model As cEwEModel = data.Core.EwEModel
            Dim mu As cProducerUnit = Nothing
            Dim input As cInput = Nothing
            Dim sArea As Single = model.Area
            'Dim sLandings As Single = 0.0
            'Dim sLandingPrice As Single = 0.0

            'nGroups = EwECore.nGroups
            'nFleets = EwECore.nFleets
            'nYears = EwECore.nTimeSeriesYears
            Dim NoProducers As Integer
            '=??? edEcostData.GetUnits(cUnitFactory.cUnitFormatter.Producer)
            'not sure if this number can be read so instead summing up below

            ' For each Metier unit
            For Each unit As cUnit In data.GetUnits(cUnitFactory.eUnitType.Producer)
                'sum up the number of producers:  
                NoProducers += 1  'or get the unit number for the current unit?

                ' Get actual metier unit
                mu = DirectCast(unit, cProducerUnit)

                If (mu.Fleet IsNot Nothing) Then
                    ' Has a group?
                    ' JS: Revise this with introduction of group links
                    'If (mu.Group IsNot Nothing) Then
                    '    'ProducerReference(group, Fleet) = NoProducers
                    'Else
                    '    ' #No: aggregate over all groups which are caught by fleet
                    '    For iGroup As Integer = 1 To EwECore.nGroups
                    '        'set the producerreference

                    '    Next iGroup
                    'End If
                End If

            Next

            'Store the catch from the timeseries if it is a catch TS:
            Dim iTSCatch(EwECore.nGroups, EwECore.nTimeSeriesYears) As Single
            Dim iSpTimeSeriesCatch(EwECore.nGroups) As Boolean

            'only do the next if there's time series, if so EwECore.nTimeSeriesYears > 0
            If EwECore.nTimeSeriesYears > 0 Then
                For iTS As Integer = 1 To EwECore.nTimeSeries
                    Dim ts As cTimeSeries = EwECore.EcosimTimeSeries(iTS)

                    If TypeOf (ts) Is cGroupTimeSeries Then
                        Dim grpTS As cGroupTimeSeries = DirectCast(ts, cGroupTimeSeries)

                        If ts.TimeSeriesType = eTimeSeriesType.Catches Or _
                           ts.TimeSeriesType = eTimeSeriesType.CatchesRel Or _
                           ts.TimeSeriesType = eTimeSeriesType.CatchesForcing Then
                            ' The group that the TS is applied to
                            Dim iSp As Integer = grpTS.GroupIndex

                            iSpTimeSeriesCatch(iSp) = True
                            For iYr As Integer = 1 To EwECore.nTimeSeriesYears
                                'read the timeseries data into the itscatch matrix
                                iTSCatch(iSp, iYr) = grpTS.DatVal(iYr)
                            Next
                        End If

                    End If
                Next
            End If

            'From the catch TS we only have the total catch, not the catch by fleet, 
            'hence we need to assume:
            '
            'The distribution between species is the same as in the Ecopath model
            'Note though that once we have better effort data, we may be able to 
            'get catch series from SAUP by fleet, and if so, we should change the 
            'ecosim catch TS so as to be by fleet. 
            'Eventually, this also calls for the Ecosim plots to show catches by fleet 
            '(as well as discards -- overall or by fleet if available) 
            'summed on top of each other

            Dim ProportionOfLanding(EwECore.nGroups, EwECore.nFleets) As Single
            For iSp As Integer = 1 To EwECore.nGroups
                If iSpTimeSeriesCatch(iSp) Then
                    Dim sumCatch As Single = 0
                    For iFt As Integer = 1 To EwECore.nFleets
                        If EwECore.EcopathFleetInputs(iFt).Landings(iSp) > 0 Then
                            sumCatch += EwECore.EcopathFleetInputs(iFt).Landings(iSp)
                        End If
                    Next
                    If sumCatch > 0 Then
                        For iFt As Integer = 1 To EwECore.nFleets
                            If EwECore.EcopathFleetInputs(iFt).Landings(iSp) > 0 Then
                                ProportionOfLanding(iSp, iFt) = EwECore.EcopathFleetInputs(iFt).Landings(iSp) / sumCatch
                            End If
                        Next
                    End If
                End If
            Next


            'Now we must have a way to relate the catches by species and fleet to producers
            'I presume here (for lack of knowledge of how it has been implemented)
            'that there is a matrix, which stores 'producer-association', somewhat like this

            ReDim m_ProducerReference(EwECore.nGroups, EwECore.nGroups)
            'then assume that when the flow is set up we read the producer ID into the ProducerReference
            'and that we thus can cycle through species/groups and read producer number

            'not sure this is needed, but what
            'RunOK = EwECore.RunEcoPath()

            'RunOK = RunOK And EwECore.RunEcoSim()


            'the timeseries catches are annual values, so sum up for every year only
            If iTimeStep Mod 12 = 0 Then 'another year gone by
                Dim iYr As Integer = CInt(iTimeStep / 12)

                'if there is a timeseries catch for a species-fleet combination then use it
                'I presume this may have to be done by updating the catches 
                'then reading the catches to the producer 
                '(which may be a fleet, or a fleet/group combination)
                'cycle through each producer, and get the catches:

                ReDim m_sumCatch(NoProducers)
                ReDim m_sumValue(NoProducers)

                For iSp As Integer = 1 To EwECore.nGroups
                    For iFt As Integer = 1 To EwECore.nFleets
                        'Is this species/fleet combination associated with a producer?
                        If m_ProducerReference(iSp, iFt) > 0 Then
                            'If there is a TS catch then use it
                            If iSpTimeSeriesCatch(iSp) Then
                                m_sumCatch(m_ProducerReference(iSp, iFt)) += _
                                    iTSCatch(iSp, iYr) * ProportionOfLanding(iSp, iFt) * sArea
                                'sum value = landing x marketprice (which is really landingprice
                                m_sumValue(m_ProducerReference(iSp, iFt)) += _
                                    iTSCatch(iSp, iYr) * ProportionOfLanding(iSp, iFt) _
                                    * EwECore.EcopathFleetInputs(iFt).OffVesselValue(iSp)
                            Else        'if not then use the Ecosim landing
                                m_sumCatch(m_ProducerReference(iSp, iFt)) += _
                                    EwECore.EcoSimGroupOutputs(iSp).Biomass(iTimeStep) * EwECore.EcoSimGroupOutputs(iSp).FishMort(iTimeStep) _
                                    * sArea
                                'sum value = landing x marketprice (which is really landingprice
                                m_sumValue(m_ProducerReference(iSp, iFt)) += _
                                    EwECore.EcoSimGroupOutputs(iSp).Biomass(iTimeStep) * EwECore.EcoSimGroupOutputs(iSp).FishMort(iTimeStep) _
                                    * EwECore.EcopathFleetInputs(iFt).OffVesselValue(iSp) * sArea
                            End If
                        End If
                    Next 'fleet
                Next 'species
                'now we have the catches and the values 
                'the landing price may change for each timestep based on what species are caught
                'so calculate: AvPrice = SumValue / Sumcatch 

                'Now it's time to run the economic module


                'better store the results for this year
                'a modified results.dump I presume

            End If 'year

        End If

        Return RunOK

        Return True
    End Function

#Region " Properties "
    Public ReadOnly Property ProducerValue(ByVal spc As Integer, ByVal flt As Integer) As Integer
        Get
            Return Me.m_ProducerReference(spc, flt)
        End Get
    End Property
#End Region

End Class