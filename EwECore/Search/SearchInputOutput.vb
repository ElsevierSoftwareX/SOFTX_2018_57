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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

#End Region ' Imports

Namespace SearchObjectives

    'ValWeights uses the same array indexes to store different values
    'what these indexes hold depends on the eOptimizeOptions.MaxPortUtil
    Public Enum eValueWeightTypes
        NetEcomValue = 1
        SocialValue = 2
        PredictionVariance = 2
        MandatedRebuilding = 3
        ExistenceValue = 3
        EcoStructure = 4
        BiomassDiversity = 5
    End Enum

#Region "Fleets "

    Public Class cSearchObjectiveFleetInput
        Inherits cCoreGroupBase

        Public Sub New(ByRef theCore As cCore, ByVal DBID As Integer)
            MyBase.New(theCore)

            Me.AllowValidation = False

            Dim val As cValue

            Me.m_dataType = eDataTypes.SearchObjectiveFleetInput
            Me.m_coreComponent = eCoreComponentType.SearchObjective
            Me.DBID = DBID

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)


            val = New cValue(New Single, eVarNameFlags.FPSFleetJobCatchValue, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'FPSFleetTargetProfit
            val = New cValue(New Single, eVarNameFlags.FPSFleetTargetProfit, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)


            ResetStatusFlags()
            Me.AllowValidation = True

        End Sub

        Public Property JobCatchValue() As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.FPSFleetJobCatchValue))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSFleetJobCatchValue, value)
            End Set
        End Property


        Public Property TargetProfitability() As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.FPSFleetTargetProfit))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSFleetTargetProfit, value)
            End Set
        End Property

    End Class

#End Region

#Region "Value Weights"

    Public Class cSearchObjectiveWeights
        Inherits cCoreInputOutputBase

        Public Sub New(ByRef theCore As cCore)
            MyBase.New(theCore)

            Me.AllowValidation = False

            Dim val As cValue

            Me.m_dataType = eDataTypes.SearchObjectiveWeights
            Me.m_coreComponent = eCoreComponentType.SearchObjective

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            'FPSEcoSystemWeight
            val = New cValue(New Single, eVarNameFlags.FPSEconomicWeight, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'FPSSocialWeight
            val = New cValue(New Single, eVarNameFlags.FPSSocialWeight, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'FPSBiomassDiversityWeight
            val = New cValue(New Single, eVarNameFlags.FPSBiomassDiversityWeight, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'FPSMandatedRebuildingWeight
            val = New cValue(New Single, eVarNameFlags.FPSMandatedRebuildingWeight, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'FPSEcoSystemWeight
            val = New cValue(New Single, eVarNameFlags.FPSEcoSystemWeight, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'FPSPredictionVariance
            val = New cValue(New Single, eVarNameFlags.FPSPredictionVariance, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'FPSExistenceValue
            val = New cValue(New Single, eVarNameFlags.FPSExistenceValue, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ResetStatusFlags()
            Me.AllowValidation = True

        End Sub

        Public Property EconomicWeight() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSEconomicWeight), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSEconomicWeight, value)
            End Set
        End Property

        Public Property SocialWeight() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSSocialWeight), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSSocialWeight, value)
            End Set
        End Property

        Public Property BiomassDiversityWeight() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSBiomassDiversityWeight), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSBiomassDiversityWeight, value)
            End Set
        End Property

        Public Property MandatedRebuildingWeight() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSMandatedRebuildingWeight), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSMandatedRebuildingWeight, value)
            End Set
        End Property

        Public Property EcoSystemWeight() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSEcoSystemWeight), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSEcoSystemWeight, value)
            End Set
        End Property

        Public Property PredictionVariance() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSPredictionVariance), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSPredictionVariance, value)
            End Set
        End Property

        Public Property ExistenceValue() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSExistenceValue), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSExistenceValue, value)
            End Set
        End Property

    End Class

#End Region

#Region "Groups "


    Public Class cSearchObjectiveGroupInput
        Inherits cCoreGroupBase


        Public Sub New(ByRef theCore As cCore, ByVal DBID As Integer)
            MyBase.New(theCore)
            Me.AllowValidation = False
            Me.DBID = DBID

            Me.m_dataType = eDataTypes.SearchObjectiveGroupInput
            Me.m_coreComponent = eCoreComponentType.SearchObjective

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            Dim val As cValue

            'FPSGroupMandRelBiom
            val = New cValue(New Single, eVarNameFlags.FPSGroupMandRelBiom, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'StrucRelWeight
            val = New cValue(New Single, eVarNameFlags.FPSGroupStrucRelWeight, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'Fishing Limit
            val = New cValue(New Single, eVarNameFlags.FPSFishingLimit, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ResetStatusFlags()
            Me.AllowValidation = True

        End Sub


        Public Property MandRelBiom() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSGroupMandRelBiom), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSGroupMandRelBiom, value)
            End Set
        End Property

        Public Property StrucRelWeight() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSGroupStrucRelWeight), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSGroupStrucRelWeight, value)
            End Set
        End Property


        Public Property FishingLimit() As Single
            Get
                Return CType(GetVariable(eVarNameFlags.FPSFishingLimit), Single)
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.FPSFishingLimit, value)
            End Set
        End Property

    End Class

#End Region

#Region "Parameters"

    Public Class cSearchObjectiveParameters
        Inherits cCoreInputOutputBase

        Public Sub New(ByRef theCore As cCore)
            MyBase.New(theCore)

            Me.AllowValidation = False
            Me.DBID = cCore.NULL_VALUE
            Me.m_dataType = eDataTypes.SearchObjectiveParameters
            Me.m_coreComponent = eCoreComponentType.SearchObjective
            AllowValidation = False

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            Dim val As cValue

            'SearchDiscountRate
            val = New cValue(New Single, eVarNameFlags.SearchDiscountRate, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'SearchGenDiscRate
            val = New cValue(New Single, eVarNameFlags.SearchGenDiscRate, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            'SearchBaseYear
            val = New cValue(New Integer, eVarNameFlags.SearchBaseYear, eStatusFlags.Null, eValueTypes.Int)
            val.Stored = False
            m_values.Add(val.varName, val)

            'SearchPrevCostEarning
            val = New cValue(New Boolean, eVarNameFlags.SearchPrevCostEarning, eStatusFlags.Null, eValueTypes.Bool)
            val.Stored = False
            m_values.Add(val.varName, val)

            'SearchFishingMortalityPenalty
            val = New cValue(New Boolean, eVarNameFlags.SearchFishingMortalityPenalty, eStatusFlags.Null, eValueTypes.Bool)
            val.Stored = False
            m_values.Add(val.varName, val)

            ResetStatusFlags()
            AllowValidation = True

        End Sub


        Public Property DiscountRate() As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.SearchDiscountRate))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.SearchDiscountRate, value)
            End Set
        End Property


        Public Property GenDiscRate() As Single
            Get
                Return CSng(GetVariable(eVarNameFlags.SearchGenDiscRate))
            End Get

            Set(ByVal value As Single)
                SetVariable(eVarNameFlags.SearchGenDiscRate, value)
            End Set
        End Property


        Public Property BaseYear() As Integer
            Get
                Return CInt(GetVariable(eVarNameFlags.SearchBaseYear))
            End Get

            Set(ByVal value As Integer)
                SetVariable(eVarNameFlags.SearchBaseYear, value)
            End Set
        End Property


        Public Property PrevCostEarning() As Boolean
            Get
                Return CBool(GetVariable(eVarNameFlags.SearchPrevCostEarning))
            End Get

            Set(ByVal value As Boolean)
                SetVariable(eVarNameFlags.SearchPrevCostEarning, value)
            End Set
        End Property


        Public Property FishingMortalityPenalty() As Boolean
            Get
                Return CBool(GetVariable(eVarNameFlags.SearchFishingMortalityPenalty))
            End Get

            Set(ByVal value As Boolean)
                SetVariable(eVarNameFlags.SearchFishingMortalityPenalty, value)
            End Set
        End Property

    End Class

#End Region

End Namespace
