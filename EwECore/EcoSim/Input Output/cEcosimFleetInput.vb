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

Public Class cEcosimFleetInput
    Inherits cCoreInputOutputBase


    ' Private m_coreData As New Dictionary(Of eVarNameFlags, IResultsWrapper)

    Public Sub New(ByRef TheCore As cCore, ByVal iFleet As Integer)
        MyBase.New(TheCore)

        Dim val As cValue
        Dim simdata As cEcosimDatastructures = Me.m_core.m_EcoSimData

        AllowValidation = False

        Me.m_dataType = eDataTypes.EcosimFleetInput
        Me.m_coreComponent = eCoreComponentType.EcoSim

        Me.AllowValidation = False

        Me.Index = iFleet
        Me.DBID = TheCore.m_EcoSimData.FleetDBID(iFleet)
        Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        'EPower
        val = New cValue(New Single, eVarNameFlags.EPower, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'PcapBase
        val = New cValue(New Single, eVarNameFlags.PcapBase, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'CapDepreciate
        val = New cValue(New Single, eVarNameFlags.CapDepreciate, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'CapBaseGrowth
        val = New cValue(New Single, eVarNameFlags.CapBaseGrowth, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        ' FleetEffortConversion
        val = New cValue(New Single, eVarNameFlags.FleetEffortConversion, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)


        val = New cValueArrayTripleIndex(eValueTypes.SingleArray, eVarNameFlags.RelQt, eStatusFlags.Null,
                                         eCoreCounterTypes.nGroups, eCoreCounterTypes.nEcosimTimeSteps, AddressOf m_core.GetCoreCounter, Me.Index, eDataTypes.EcosimFleetInput)
        m_values.Add(val.varName, val)



        ''arrayed values
        ''RelQ at T
        'val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.RelQ, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        'm_values.Add(val.varName, val)

        AllowValidation = True

    End Sub

#Region " Variable via dot '.' operator "

    ''' <summary>
    ''' Effort response pow.fi
    ''' </summary>
    Public Property EPower() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EPower))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EPower, value)
        End Set

    End Property

    ''' <summary>
    ''' capital depreciation rate
    ''' </summary>
    Public Property CapDepreciateRate() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.CapDepreciate))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CapDepreciate, value)
        End Set

    End Property

    ''' <summary>
    ''' Initial effort / capital capacity
    ''' </summary>
    Public Property PcapBase() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.PcapBase))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.PcapBase, value)
        End Set

    End Property

    ''' <summary>
    ''' initial capitial growth
    ''' </summary>
    Public Property CapBaseGrowth() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.CapBaseGrowth))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CapBaseGrowth, value)
        End Set

    End Property


    ''' <summary>
    ''' Effort conversion factor used for summing maps into a single Effort value
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property EffortConversionFactor() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.FleetEffortConversion))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.FleetEffortConversion, value)
        End Set
    End Property


    Public Property RelQt(ByVal iGroup As Integer, iTimestep As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.RelQt, iGroup, iTimestep))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.RelQt, value, iGroup, iTimestep)
        End Set
    End Property

#End Region ' Variable via dot '.' operator

#Region " Status via dot '.' operator "


    Public Property RelQtStatus(ByVal iGroup As Integer, iTimestep As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.RelQt, iGroup, iTimestep)
        End Get
        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.RelQt, value, iGroup, iTimestep)
        End Set
    End Property


    Public Property CapBaseGrowthStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.CapBaseGrowth)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CapBaseGrowth, value)
        End Set

    End Property

    Public Property CapDepreciateRateStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.CapDepreciate)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CapDepreciate, value)
        End Set

    End Property

    Public Property PcapBaseStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.PcapBase)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.PcapBase, value)
        End Set

    End Property

    Public Property EPowerStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EPower)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EPower, value)
        End Set
    End Property

    Public Property EffortConversionFactorStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.FleetEffortConversion)
        End Get
        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.FleetEffortConversion, value)
        End Set
    End Property

#End Region ' Status via dot '.' operator


#Region "Overridden base class methods"



#End Region

End Class
