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

Public Class cEcospaceFleetInput
    Inherits cCoreInputOutputBase

#Region " Constructor "

    Sub New(ByVal theCore As cCore, ByVal iDBID As Integer)
        MyBase.New(theCore)

        Dim val As cValue = Nothing

        Try

            Me.m_dataType = eDataTypes.EcospaceFleet
            Me.m_coreComponent = eCoreComponentType.EcoSpace
            Me.DBID = iDBID

            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            ' EffectivePower
            val = New cValue(New Single, eVarNameFlags.EffectivePower, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' SEmult
            val = New cValue(New Single, eVarNameFlags.SEmult, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            'Array variables

            ' HabitatFishery
            val = New cValueArray(eValueTypes.BoolArray, eVarNameFlags.HabitatFishery, eStatusFlags.Null, eCoreCounterTypes.nHabitats, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            ' MPAFishery
            val = New cValueArray(eValueTypes.BoolArray, eVarNameFlags.MPAFishery, eStatusFlags.Null, eCoreCounterTypes.nMPAs, AddressOf m_core.GetCoreCounter)
            m_values.Add(val.varName, val)

            ResetStatusFlags()

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcospaceFleet.")
            cLog.Write(Me.ToString & ".New(nGroups) Error creating new cEcospaceFleet. Error: " & ex.Message)
        End Try

    End Sub

#End Region ' Constructor

#Region " Properties by dot (.) operator "

    ''' <summary>
    ''' Get/set the effort concentration factor for this fleet.
    ''' </summary>
    ''' <returns></returns>
    Public Property EffectivePower() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EffectivePower))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EffectivePower, value)
        End Set
    End Property

    ''' <summary>
    ''' Get/set whether this fleet is allowed to fish in a given habitat
    ''' </summary>
    ''' <param name="iHabitat"></param>
    Public Property HabitatFishery(ByVal iHabitat As Integer) As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.HabitatFishery, iHabitat))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.HabitatFishery, value, iHabitat)
        End Set
    End Property

    ''' <summary>
    ''' Get/set whether a fleet is allowed to fish in an mpa
    ''' </summary>
    ''' <value>
    '''   <c>true</c> if [mpa fishery]; otherwise, <c>false</c>.
    ''' </value>
    Public Property MPAFishery(ByVal iMPA As Integer) As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.MPAFishery, iMPA))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.MPAFishery, value, iMPA)
        End Set
    End Property

    Public Property TotalEffMultiplier() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.SEmult))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.SEmult, value)
        End Set
    End Property


#End Region ' Properties by dot (.) operator

#Region " Status by dot (.) operator "

    Public Property EffectivePowerStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EffectivePower)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EffectivePower, value)
        End Set
    End Property

    Public Property HabitatFisheryStatus(ByVal iHabitat As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.HabitatFishery, iHabitat)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.HabitatFishery, value, iHabitat)
        End Set
    End Property

    Public Property MPAFisheryStatus(ByVal iMPA As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.MPAFishery, iMPA)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MPAFishery, value, iMPA)
        End Set
    End Property



#End Region ' Status by dot (.) operator 

End Class

<Obsolete("Please use cEcospaceFleetInput instead")>
Public Class cEcospaceFleet
    Inherits cEcospaceFleetInput

    Sub New(ByVal theCore As cCore, ByVal iDBID As Integer)
        MyBase.New(theCore, iDBID)
    End Sub

End Class