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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

''' <summary>
''' Class to encapsulate a variables for a single Fishing Fleet Input
''' </summary>
''' <remarks></remarks>
Public Class cEcopathFleetInput
    Inherits cCoreInputOutputBase

#Region " Construction and Intialization "

    Public Sub New(TheCore As cCore, DBID As Integer, iIndex As Integer)
        MyBase.New(TheCore)

        'stop the data validation for now
        'ToDo_jb add data validation for Fleets
        AllowValidation = False
        m_coreComponent = eCoreComponentType.EcoPath
        m_dataType = eDataTypes.FleetInput

        m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        Me.Index = iIndex
        Me.DBID = DBID

        Dim val As cValue

        'For fisheries data validation see EwE5 frmInputData.vaInput_Change(...)

        'FixedCost
        val = New cValue(New Single, eVarNameFlags.FixedCost, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'CPUECost
        val = New cValue(New Single, eVarNameFlags.CPUECost, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'SailCost
        val = New cValue(New Single, eVarNameFlags.SailCost, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'PoolColor
        val = New cValue(New Integer, eVarNameFlags.PoolColor, eStatusFlags.Null, eValueTypes.Int)
        val.AffectsRunState = False
        m_values.Add(val.varName, val)

        'arrayed values
        'Landings
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.Landings, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        'Discards
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.Discards, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        'Off-vessel price (formerly known as MarketPrice)
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.OffVesselPrice, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        'DiscardFate
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.DiscardFate, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        'DiscardMortality
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.DiscardMortality, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

    End Sub

#End Region

#Region " Overrides "

    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
        If Not MyBase.ResetStatusFlags(bForceReset) Then Return False
        Return Me.m_core.Set_OffVesselValue_Flags(Me, False) And Me.m_core.Set_DiscardMort_Flags(Me, False)
    End Function

#End Region ' Overrides

#Region "Variables via dot (.) operator"

    Public Property FixedCost() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.FixedCost))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.FixedCost, value)
        End Set
    End Property

    Public Property SailCost() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.SailCost))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.SailCost, value)
        End Set
    End Property

    Public Property CPUECost() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.CPUECost))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CPUECost, value)
        End Set
    End Property

    Public Property Landings(ByVal iGroup As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.Landings, iGroup))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.Landings, value, iGroup)
        End Set
    End Property

    Public Property Landings() As Single()
        Get
            Return DirectCast(GetVariable(eVarNameFlags.Landings), Single())
        End Get
        Set(ByVal value() As Single)
            SetVariable(eVarNameFlags.Landings, value)
        End Set
    End Property

    Public Property PoolColor() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.PoolColor))
        End Get
        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.PoolColor, value)
        End Set
    End Property

#Region "Indexed Variables"

    Public Property OffVesselValue(ByVal iGroup As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.OffVesselPrice, iGroup))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.OffVesselPrice, value, iGroup)
        End Set

    End Property

    <Obsolete("Use OffVesselValue instead")>
    Public Property OffVesselPrice() As Single()

        Get
            Return Me.OffVesselValue()
        End Get
        Set(ByVal value() As Single)
            Me.OffVesselValue() = value
        End Set

    End Property

    Public Property OffVesselValue() As Single()

        Get
            Return DirectCast(GetVariable(eVarNameFlags.OffVesselPrice), Single())
        End Get
        Set(ByVal value() As Single)
            SetVariable(eVarNameFlags.OffVesselPrice, value)
        End Set

    End Property


    Public Property Discards(ByVal iGroup As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.Discards, iGroup))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.Discards, value, iGroup)
        End Set

    End Property

    Public Property Discards() As Single()

        Get
            Return DirectCast(GetVariable(eVarNameFlags.Discards), Single())
        End Get
        Set(ByVal value() As Single)
            SetVariable(eVarNameFlags.Discards, value)
        End Set

    End Property

    Public Property DiscardFate(ByVal iGroup As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.DiscardFate, iGroup))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.DiscardFate, value, iGroup)
        End Set

    End Property

    Public Property DiscardMortality(ByVal iGroup As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.DiscardMortality, iGroup))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.DiscardMortality, value, iGroup)
        End Set
    End Property

#End Region

    Public Property CPUECostStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.CPUECost)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CPUECost, value)
        End Set
    End Property

    Public Property DiscardFateStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.DiscardFate)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.DiscardFate, value)
        End Set
    End Property

    Public Property DiscardsStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.Discards)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Discards, value)
        End Set
    End Property

    Public Property FixedCostStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.FixedCost)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.FixedCost, value)
        End Set
    End Property

    Public Property iFleetStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.Index)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Index, value)
        End Set
    End Property

    Public Property LandingsStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.Landings)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Landings, value)
        End Set
    End Property

    <Obsolete("Use OffVesselValueStatus instead")>
    Public Property OffVesselPriceStatus() As eStatusFlags
        Get
            Return Me.OffVesselValueStatus
        End Get
        Friend Set(ByVal value As eStatusFlags)
            Me.OffVesselValueStatus = value
        End Set
    End Property

    Public Property OffVesselValueStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.OffVesselPrice)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.OffVesselPrice, value)
        End Set
    End Property

    Public Property SailCostStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.SailCost)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.SailCost, value)
        End Set
    End Property

    Public Property DiscardMortalityStatus(ByVal iGroup As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.DiscardMortality, iGroup)
        End Get
        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.DiscardMortality, value, iGroup)
        End Set
    End Property

#End Region

End Class

#Region "Deprecated"

<Obsolete("Use cEcopathFleetInput instead")>
Public Class cFleetInput
    Inherits cEcopathFleetInput

    Public Sub New(TheCore As cCore, DBID As Integer, iIndex As Integer)
        MyBase.New(TheCore, DBID, iIndex)
    End Sub

End Class

#End Region ' Deprecated
