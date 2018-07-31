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


Public Class cEcosimFleetOutput
    Inherits cCoreInputOutputBase

    'dictionary of vars and wrappers that directly access the core data
    Private m_coreData As New Dictionary(Of eVarNameFlags, IResultsWrapper)
    Private m_simData As cEcosimDatastructures


    Public Sub New(ByRef TheCore As cCore, ByVal iFleet As Integer)
        MyBase.New(TheCore)

        Dim val As cValue
        m_simData = TheCore.m_EcoSimData

        Me.m_dataType = eDataTypes.EcosimFleetOutput
        Me.Index = iFleet
        Me.DBID = TheCore.m_EcoSimData.FleetDBID(iFleet)

        'no validators
        'Catch biomass
        val = New cValue(0, eVarNameFlags.EcosimFleetCatchStart, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(0, eVarNameFlags.EcosimFleetCatchEnd, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'Value
        val = New cValue(0, eVarNameFlags.EcosimFleetValueStart, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(0, eVarNameFlags.EcosimFleetValueEnd, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'Cost
        val = New cValue(0, eVarNameFlags.EcosimFleetCostStart, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(0, eVarNameFlags.EcosimFleetCostEnd, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'Effort
        val = New cValue(0, eVarNameFlags.EcosimFleetEffort, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'Profit
        val = New cValue(0, eVarNameFlags.EcosimFleetProfit, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'Jobs
        val = New cValue(0, eVarNameFlags.EcosimFleetJobs, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)


    End Sub


    Public Sub Init()

        'the results arrays of ecosim are redim for each run
        'this means the reference to the results data is lost on each run 
        'so reset the reference
        m_coreData.Clear()

        m_coreData.Add(eVarNameFlags.EcosimFleetValueTime, New c2DResultsWrapper(m_simData.ResultsSumValueByGear, Me.Index))
        m_coreData.Add(eVarNameFlags.EcosimFleetCatchTime, New c2DResultsWrapper(m_simData.ResultsSumCatchByGear, Me.Index))
        'ResultsSumCatchByGear

    End Sub



    Public Overrides Function GetVariable(ByVal VarName As EwEUtils.Core.eVarNameFlags, Optional ByVal iIndex1 As Integer = -9999, Optional ByVal iIndex2 As Integer = -9999, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object

        If Not m_coreData.ContainsKey(VarName) Then
            'NOT in list of sim vars so get the value from the base class GetVariable(...)
            Return MyBase.GetVariable(VarName, iIndex1, iIndex2)
        Else
            'Varname is access directly via the core data
            Return m_coreData.Item(VarName).Value(iIndex1, iIndex2)
        End If

    End Function

#Region "Variable via dot '.' operator"

    Public Property ProfitSummary() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetProfit))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimFleetProfit, value)
        End Set
    End Property


    Public Property JobsSummary() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetJobs))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimFleetJobs, value)
        End Set
    End Property

    Public Property CatchStart() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetCatchStart))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimFleetCatchStart, value)
        End Set
    End Property

    Public Property CatchEnd() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetCatchEnd))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimFleetCatchEnd, value)
        End Set
    End Property


    Public Property ValueStart() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetValueStart))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimFleetValueStart, value)
        End Set
    End Property

    Public Property ValueEnd() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetValueEnd))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimFleetValueEnd, value)
        End Set
    End Property


    Public Property CostStart() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetCostStart))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimFleetCostStart, value)
        End Set
    End Property

    Public Property CostEnd() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetCostEnd))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimFleetCostEnd, value)
        End Set
    End Property

    Public Property Effort() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetEffort))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimFleetEffort, value)
        End Set
    End Property

    Public ReadOnly Property Value(ByVal Time As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetValueTime, Time))
        End Get

    End Property

    Public ReadOnly Property CatchBiomass(ByVal Time As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimFleetCatchTime, Time))
        End Get

    End Property



#End Region

#Region "Status via dot '.' operator"

    Public Property CatchStartStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcosimFleetCatchStart)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimFleetCatchStart, value)
        End Set
    End Property

    Public Property CatchEndStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcosimFleetCatchEnd)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimFleetCatchEnd, value)
        End Set
    End Property

    Public Property ValueStartStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcosimFleetValueStart)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimFleetValueStart, value)
        End Set
    End Property

    Public Property ValueEndStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcosimFleetValueEnd)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimFleetValueEnd, value)
        End Set
    End Property


    Public Property CostStartStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcosimFleetCostStart)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimFleetCostStart, value)
        End Set
    End Property

    Public Property CostEndStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcosimFleetCostEnd)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimFleetCostEnd, value)
        End Set
    End Property

    Public Property EffortStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcosimFleetEffort)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimFleetEffort, value)
        End Set
    End Property

#End Region


End Class
