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

Namespace SearchObjectives

#Region "ISearchObjective definition "

    ''' <summary>
    ''' Interface for shared search variables
    ''' </summary>
    ''' <remarks>The Fishing Policy Search, Ecoseed and MSE all share some base variables. This interface provides a consistance interface for accessing these variables.</remarks>
    Public Interface ISearchObjective

        Function Init(ByRef theCore As cCore) As Boolean
        Function Update(ByVal DataType As eDataTypes) As Boolean
        Function Load() As Boolean
        Sub Clear()

        '''' <summary>
        '''' Stop a running process
        '''' </summary>
        '''' <param name="WaitTimeinMillSec">Length of time in milliseconds to wait for the process to complete, -1 wait indefinitely.  </param>
        '''' <returns>True if the process was stop within the wait time, False if it timed out.</returns>
        '''' <remarks></remarks>
        'Function StopRun(Optional ByVal WaitTimeinMillSec As Integer = -1) As Boolean

        ReadOnly Property ValueWeights() As cSearchObjectiveWeights
        ReadOnly Property GroupObjectives(ByVal iGroup As Integer) As cSearchObjectiveGroupInput
        ReadOnly Property FleetObjectives(ByVal iGroup As Integer) As cSearchObjectiveFleetInput
        ReadOnly Property ObjectiveParameters() As cSearchObjectiveParameters

    End Interface

#End Region

#Region "cSearchObjective implementation"

    ''' <summary>
    ''' Implementation of ISearchObjective that is used by all classes that implement the ISearchObjective interface.
    ''' </summary>
    ''' <remarks>
    ''' An single instance of this class is created by the core and made available via cCore.SearchObjective(). 
    ''' All objects that implement the ISearchObjective interface do so by sharing the same instance of cCore.SearchObjective(). 
    ''' This allows the data to be synced between different objects at all times.
    ''' </remarks>
    Public Class cSearchObjective
        Implements ISearchObjective

        Protected m_core As cCore
        Protected m_valWeights As cSearchObjectiveWeights
        Protected m_parameters As cSearchObjectiveParameters
        Private m_lstGroups As New cCoreInputOutputList(Of cSearchObjectiveGroupInput)(eDataTypes.SearchObjectiveGroupInput, 1)
        Private m_lstFleets As New cCoreInputOutputList(Of cSearchObjectiveFleetInput)(eDataTypes.SearchObjectiveFleetInput, 1)

        ''' <summary>
        ''' Build interface objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Overridable Function Init(ByRef theCore As cCore) As Boolean Implements ISearchObjective.Init
            Try

                m_core = theCore

                m_valWeights = New cSearchObjectiveWeights(m_core)
                m_parameters = New cSearchObjectiveParameters(m_core)

                'Init the search data
                Dim search As cSearchDatastructures = m_core.m_SearchData

                'sets BGoalValue() as a function of PB from last ecopath run
                search.setDefaultBGoal(m_core.m_EcoPathData.PB)
                'discount factor, FLimit, Default F rates
                search.setDefaultOptimizationValues()

                'default weights
                search.ValWeight(eValueWeightTypes.NetEcomValue) = 1

                m_lstGroups.Clear()
                Dim grp As cSearchObjectiveGroupInput
                For igrp As Integer = 1 To m_core.nGroups
                    'use the database ID for the Ecopath Groups
                    grp = New cSearchObjectiveGroupInput(m_core, m_core.m_EcoPathData.GroupDBID(igrp))
                    m_lstGroups.Add(grp)
                Next


                m_lstFleets.Clear()
                Dim flt As cSearchObjectiveFleetInput
                For iflt As Integer = 1 To m_core.nFleets
                    'use the database ID for the Fleets
                    flt = New cSearchObjectiveFleetInput(m_core, m_core.m_EcoPathData.FleetDBID(iflt))
                    m_lstFleets.Add(flt)
                Next

                'set the search back to false 
                search.SearchMode = eSearchModes.NotInSearch

                Return True

            Catch ex As Exception
                cLog.Write(ex)
                Return False
            End Try

        End Function

        Friend Overridable Function Load() As Boolean Implements ISearchObjective.Load

            Try
                Dim igrp As Integer
                Dim iflt As Integer

                Dim coreData As cSearchDatastructures = m_core.m_SearchData

                'values weights
                m_valWeights.AllowValidation = False
                m_valWeights.EconomicWeight = coreData.ValWeight(eValueWeightTypes.NetEcomValue)
                m_valWeights.EcoSystemWeight = coreData.ValWeight(eValueWeightTypes.EcoStructure)
                m_valWeights.MandatedRebuildingWeight = coreData.ValWeight(eValueWeightTypes.MandatedRebuilding)
                m_valWeights.SocialWeight = coreData.ValWeight(eValueWeightTypes.SocialValue)

                m_valWeights.BiomassDiversityWeight = coreData.ValWeight(eValueWeightTypes.BiomassDiversity)

                m_valWeights.PredictionVariance = coreData.ValWeight(eValueWeightTypes.PredictionVariance)
                m_valWeights.ExistenceValue = coreData.ValWeight(eValueWeightTypes.ExistenceValue)

                m_valWeights.AllowValidation = True

                m_valWeights.ResetStatusFlags()

                m_parameters.AllowValidation = False
                m_parameters.BaseYear = coreData.BaseYear
                m_parameters.GenDiscRate = coreData.GenDiscountFactor
                m_parameters.DiscountRate = coreData.DiscountFactor
                m_parameters.FishingMortalityPenalty = coreData.bUseFishingMortalityPenality

                m_parameters.PrevCostEarning = coreData.UseCostPenalty

                m_parameters.ResetStatusFlags()

                m_parameters.AllowValidation = True

                For Each grp As cSearchObjectiveGroupInput In m_lstGroups
                    grp.AllowValidation = False
                    igrp = Array.IndexOf(m_core.m_EcoPathData.GroupDBID, grp.DBID)
                    grp.Index = igrp
                    grp.Name = m_core.m_EcoPathData.GroupName(igrp)

                    grp.MandRelBiom = coreData.MGoalValue(grp.Index)
                    grp.StrucRelWeight = coreData.BGoalValue(grp.Index)
                    grp.FishingLimit = coreData.FLimit(grp.Index)

                    grp.AllowValidation = True

                Next

                For Each flt As cSearchObjectiveFleetInput In m_lstFleets
                    flt.AllowValidation = False

                    iflt = Array.IndexOf(m_core.m_EcoPathData.FleetDBID, flt.DBID)
                    flt.Index = iflt

                    flt.Resize()
                    flt.Name = m_core.m_EcoPathData.FleetName(iflt)
                    'pop variables.....

                    flt.JobCatchValue = coreData.Jobs(flt.Index)
                    flt.TargetProfitability = coreData.TargetProfitability(flt.Index)

                    'For it As Integer = 1 To m_core.nEcosimYears
                    '    flt.SearchBlocks(it) = coreData.FblockCode(iflt, it)
                    'Next it

                    flt.AllowValidation = True

                Next

                Return True

            Catch ex As Exception
                Return False
            End Try

        End Function

        Public Sub Clear() Implements ISearchObjective.Clear
            Me.m_lstFleets.Clear()
            Me.m_lstGroups.Clear()
        End Sub

        Public Overridable Function Update(ByVal DataType As eDataTypes) As Boolean Implements ISearchObjective.Update
            Dim coreData As cSearchDatastructures = m_core.m_SearchData

            Select Case DataType

                Case eDataTypes.SearchObjectiveParameters

                    coreData.UseCostPenalty = m_parameters.PrevCostEarning
                    coreData.BaseYear = m_parameters.BaseYear
                    coreData.GenDiscountFactor = m_parameters.GenDiscRate
                    coreData.DiscountFactor = m_parameters.DiscountRate
                    coreData.bUseFishingMortalityPenality = m_parameters.FishingMortalityPenalty

                Case eDataTypes.SearchObjectiveFleetInput

                    'load the code blocks
                    For Each flt As cSearchObjectiveFleetInput In m_lstFleets

                        coreData.Jobs(flt.Index) = flt.JobCatchValue
                        coreData.TargetProfitability(flt.Index) = flt.TargetProfitability

                        'For it As Integer = 1 To m_core.nEcosimYears
                        '    coreData.FblockCode(flt.Index, it) = flt.SearchBlocks(it)
                        'Next it
                    Next

                Case eDataTypes.SearchObjectiveGroupInput

                    For Each grp As cSearchObjectiveGroupInput In Me.m_lstGroups

                        coreData.MGoalValue(grp.Index) = grp.MandRelBiom
                        coreData.BGoalValue(grp.Index) = grp.StrucRelWeight
                        coreData.FLimit(grp.Index) = grp.FishingLimit

                    Next grp

                Case eDataTypes.SearchObjectiveWeights
                    'Value Weights
                    coreData.ValWeight(eValueWeightTypes.NetEcomValue) = m_valWeights.EconomicWeight
                    coreData.ValWeight(eValueWeightTypes.EcoStructure) = m_valWeights.EcoSystemWeight

                    coreData.ValWeight(eValueWeightTypes.BiomassDiversity) = m_valWeights.BiomassDiversityWeight

                    'ValWeight() shares indexes for different values based on the search.PortFolio flag
                    'SocialValue = 2
                    'PredictionVariance = 2
                    'MandatedRebuilding = 3
                    'ExistenceValue = 3
                    If coreData.PortFolio Then
                        coreData.ValWeight(eValueWeightTypes.PredictionVariance) = m_valWeights.PredictionVariance
                        coreData.ValWeight(eValueWeightTypes.ExistenceValue) = m_valWeights.ExistenceValue
                    Else
                        coreData.ValWeight(eValueWeightTypes.MandatedRebuilding) = m_valWeights.MandatedRebuildingWeight
                        coreData.ValWeight(eValueWeightTypes.SocialValue) = m_valWeights.SocialWeight
                    End If

            End Select

            Return True

        End Function


        Public ReadOnly Property ValueWeights() As cSearchObjectiveWeights Implements ISearchObjective.ValueWeights
            Get
                Return Me.m_valWeights
            End Get
        End Property

        Public ReadOnly Property GroupObjectives(ByVal iGroup As Integer) As cSearchObjectiveGroupInput Implements ISearchObjective.GroupObjectives
            Get
                Return m_lstGroups(iGroup)
            End Get
        End Property

        Public ReadOnly Property FleetObjectives(ByVal iGroup As Integer) As cSearchObjectiveFleetInput Implements ISearchObjective.FleetObjectives
            Get
                Return m_lstFleets(iGroup)
            End Get
        End Property

        Public ReadOnly Property ObjectiveParameters() As cSearchObjectiveParameters Implements ISearchObjective.ObjectiveParameters
            Get
                Return m_parameters
            End Get
        End Property


    End Class

#End Region

End Namespace





