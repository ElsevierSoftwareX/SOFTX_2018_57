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
Imports EwEUtils.Utilities

''' <summary>
''' Results from EcoPath for a single fleet.
''' </summary>
Public Class cEcopathFleetOutput
    Inherits cCoreInputOutputBase


    'ToDo: Added comments to varname enums


    Public Sub New(ByRef TheCore As cCore, ByVal DBID As Integer, ByVal iIndex As Integer)
        MyBase.New(TheCore)

        Dim val As cValue
        Me.m_dataType = eDataTypes.EcoPathFleetOutput
        Me.Index = iIndex
        Me.DBID = DBID

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcopathCatchTotalByFleetGroup, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcopathCatchMortByFleetGroup, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcopathLandingsByFleetGroup, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcopathDiscardsByFleetGroup, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcopathDiscardsMortByFleetGroup, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcopathDiscardsSurvivalByFleetGroup, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

    End Sub


    ''' <summary>
    ''' Total Catch Landings + discards. Includes discards that survived
    ''' </summary>
    ''' <param name="iGrp"></param>
    ''' <returns></returns>
    Public Property CatchTotalByGroup(ByVal iGrp As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcopathCatchTotalByFleetGroup, iGrp))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathCatchTotalByFleetGroup, newValue, iGrp)
            End If
        End Set

    End Property

    Public Property CatchMortByGroup(ByVal iGrp As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcopathCatchMortByFleetGroup, iGrp))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathCatchMortByFleetGroup, newValue, iGrp)
            End If
        End Set

    End Property


    ''' <summary>
    ''' Landings only
    ''' </summary>
    ''' <param name="iGrp"></param>
    ''' <returns></returns>
    Public Property LandingsByGroup(ByVal iGrp As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcopathLandingsByFleetGroup, iGrp))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathLandingsByFleetGroup, newValue, iGrp)
            End If
        End Set

    End Property

    ''' <summary>
    ''' Total Discards 
    ''' </summary>
    ''' <param name="iGrp"></param>
    Public Property DiscardByGroup(ByVal iGrp As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcopathDiscardsByFleetGroup, iGrp))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathDiscardsByFleetGroup, newValue, iGrp)
            End If
        End Set

    End Property

    ''' <summary>
    ''' Discards that incurred mortality Discards * DiscardMortRate
    ''' </summary>
    ''' <param name="iGrp"></param>
    ''' <returns></returns>
    Public Property DiscardMortByGroup(ByVal iGrp As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcopathDiscardsMortByFleetGroup, iGrp))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathDiscardsMortByFleetGroup, newValue, iGrp)
            End If
        End Set

    End Property

    ''' <summary>
    ''' Discards that survived Discards * (1 - DiscardMortRate)
    ''' </summary>
    ''' <param name="iGrp"></param>
    ''' <returns></returns>
    Public Property DiscardSurvivalByGroup(ByVal iGrp As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcopathDiscardsSurvivalByFleetGroup, iGrp))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.EcopathDiscardsSurvivalByFleetGroup, newValue, iGrp)
            End If
        End Set

    End Property


End Class
