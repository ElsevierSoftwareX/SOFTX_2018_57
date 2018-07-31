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

Public Class cMonteCarloGroup
    Inherits cCoreGroupBase

#Region " Constructor "

    Sub New(ByRef theCore As cCore, ByVal DBID As Integer)
        MyBase.New(theCore)


        m_dataType = eDataTypes.MonteCarlo
        m_coreComponent = eCoreComponentType.EcoSim
        Me.AllowValidation = False
        Me.DBID = DBID

        'default OK status used for setVariable
        'see comment setVariable(...)
        m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        Dim val As cValue

        'biomass
        val = New cValue(New Single, eVarNameFlags.mcB, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'PB
        val = New cValue(New Single, eVarNameFlags.mcPB, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'ba
        val = New cValue(New Single, eVarNameFlags.mcBA, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'BaBi
        val = New cValue(New Single, eVarNameFlags.mcBaBi, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'QB
        val = New cValue(New Single, eVarNameFlags.mcQB, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'EE
        val = New cValue(New Single, eVarNameFlags.mcEE, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'VU
        val = New cValue(New Single, eVarNameFlags.mcVU, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'DC
        val = New cValue(New Single, eVarNameFlags.mcDC, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'biomassLower
        val = New cValue(New Single, eVarNameFlags.mcBLower, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'PBLower
        val = New cValue(New Single, eVarNameFlags.mcPBLower, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'baLower
        val = New cValue(New Single, eVarNameFlags.mcBALower, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'babiLower
        val = New cValue(New Single, eVarNameFlags.mcBaBiLower, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'QBLower
        val = New cValue(New Single, eVarNameFlags.mcQBLower, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'EELower
        val = New cValue(New Single, eVarNameFlags.mcEELower, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'VULower
        val = New cValue(New Single, eVarNameFlags.mcVULower, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Best fit
        'biomassBF
        val = New cValue(New Single, eVarNameFlags.mcBbf, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'PBBF
        val = New cValue(New Single, eVarNameFlags.mcPBbf, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'baBF
        val = New cValue(New Single, eVarNameFlags.mcBAbf, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'baBF
        val = New cValue(New Single, eVarNameFlags.mcBaBibf, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'QBBF
        val = New cValue(New Single, eVarNameFlags.mcQBbf, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'EEBF
        val = New cValue(New Single, eVarNameFlags.mcEEbf, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'VUBF
        val = New cValue(New Single, eVarNameFlags.mcVUbf, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'DCBf
        val = New cValue(New Single, eVarNameFlags.mcDCbf, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Upper

        'biomassUpper
        val = New cValue(New Single, eVarNameFlags.mcBUpper, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'PBUpper
        val = New cValue(New Single, eVarNameFlags.mcPBUpper, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'baUpper
        val = New cValue(New Single, eVarNameFlags.mcBAUpper, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'babiUpper
        val = New cValue(New Single, eVarNameFlags.mcBaBiUpper, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'QBUpper
        val = New cValue(New Single, eVarNameFlags.mcQBUpper, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'EEUpper
        val = New cValue(New Single, eVarNameFlags.mcEEUpper, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'VUUpper
        val = New cValue(New Single, eVarNameFlags.mcVUUpper, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'cv

        'biomasscv
        val = New cValue(New Single, eVarNameFlags.mcBcv, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'PBcv
        val = New cValue(New Single, eVarNameFlags.mcPBcv, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'bacv
        val = New cValue(New Single, eVarNameFlags.mcBAcv, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'babicv
        val = New cValue(New Single, eVarNameFlags.mcBaBicv, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'QBcv
        val = New cValue(New Single, eVarNameFlags.mcQBcv, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'EEcv
        val = New cValue(New Single, eVarNameFlags.mcEEcv, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'DCcv
        val = New cValue(New Single, eVarNameFlags.mcDCcv, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'VUcv
        val = New cValue(New Single, eVarNameFlags.mcVUcv, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'DietMult
        val = New cValue(New Single, eVarNameFlags.mcDietMult, eStatusFlags.Null, eValueTypes.Sng)
        val.Stored = False
        m_values.Add(val.varName, val)

        'arrayed values
        'landings
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcLandings, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

        'discards
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcDiscards, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

        'LandingsLower
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcLandingsLower, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

        'DiscardsLower
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcDiscardsLower, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

        'LandingsBF
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcLandingsbf, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

        'DiscardsBF
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcDiscardsbf, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

        'LandingsUpper
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcLandingsUpper, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

        'DiscardsUpper
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcDiscardsUpper, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

        'Landingscv
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcLandingscv, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

        'Discardscv
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.mcDiscardscv, eStatusFlags.Null, eCoreCounterTypes.nFleets, AddressOf m_core.GetCoreCounter)
        val.Stored = False
        m_values.Add(val.varName, val)

    End Sub


    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean

        Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
        Dim value As cValue
        For Each keyvalue In m_values
            Try
                value = keyvalue.Value

                Select Case value.varType

                    Case eValueTypes.Sng
                        value.Status = eStatusFlags.OK

                        If (value.varName = eVarNameFlags.mcB Or value.varName = eVarNameFlags.mcBbf) Or
                           (value.varName = eVarNameFlags.mcBA Or value.varName = eVarNameFlags.mcBAbf) Or
                           (value.varName = eVarNameFlags.mcBaBi Or value.varName = eVarNameFlags.mcBaBibf) Or
                           (value.varName = eVarNameFlags.mcEE Or value.varName = eVarNameFlags.mcEEbf) Or
                           (value.varName = eVarNameFlags.mcPB Or value.varName = eVarNameFlags.mcPBbf) Or
                           (value.varName = eVarNameFlags.mcQB Or value.varName = eVarNameFlags.mcQBbf) Or
                           (value.varName = eVarNameFlags.mcDC Or value.varName = eVarNameFlags.mcDCbf) Then

                            value.Status = eStatusFlags.NotEditable

                        End If

                    Case eValueTypes.SingleArray, eValueTypes.IntArray, eValueTypes.BoolArray

                        For i As Integer = 1 To value.Length
                            If (value.varName = eVarNameFlags.mcLandings Or value.varName = eVarNameFlags.mcLandingsbf) Or
                               (value.varName = eVarNameFlags.mcDiscards Or value.varName = eVarNameFlags.mcDiscardsbf) Then
                                value.Status(i) = eStatusFlags.NotEditable
                            Else
                                value.Status(i) = eStatusFlags.OK
                            End If
                        Next

                    Case Else
                        'name and other variables
                        value.Status = eStatusFlags.NotEditable

                End Select
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return False
            End Try
        Next keyvalue
        Return True

    End Function

#End Region

#Region " Dot (.) operators "

    Public Property B() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcB))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcB, value)
        End Set
    End Property

    Public Property BA() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBA))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBA, value)
        End Set
    End Property

    Public Property BaBi() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBaBi))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBaBi, value)
        End Set
    End Property

    Public Property PB() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcPB))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcPB, value)
        End Set
    End Property

    Public Property QB() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcQB))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcQB, value)
        End Set
    End Property

    Public Property EE() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcEE))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcEE, value)
        End Set
    End Property

    'VU = vulnerability (or VulMult) by predator
    Public Property VU() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcVU))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcVU, value)
        End Set
    End Property

    Public Property Landings(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcLandings, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcLandings, value, iFleet)
        End Set
    End Property

    Public Property Discards(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcDiscards, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcDiscards, value, iFleet)
        End Set
    End Property

    Public Property Diets(ByVal iIndex As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcDC, iIndex))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcDC, value, iIndex)
        End Set
    End Property

    Public Property BLower() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBLower))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBLower, value)
        End Set
    End Property

    Public Property BALower() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBALower))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBALower, value)
        End Set
    End Property

    Public Property BaBiLower() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBaBiLower))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBaBiLower, value)
        End Set
    End Property

    Public Property PBLower() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcPBLower))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcPBLower, value)
        End Set
    End Property

    Public Property QBLower() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcQBLower))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcQBLower, value)
        End Set
    End Property

    Public Property EELower() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcEELower))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcEELower, value)
        End Set
    End Property

    Public Property VULower() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcVULower))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcVULower, value)
        End Set
    End Property

    Public Property LandingsLower(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcLandingsLower, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcLandingsLower, value, iFleet)
        End Set
    End Property

    Public Property DiscardsLower(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcDiscardsLower, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcDiscardsLower, value, iFleet)
        End Set
    End Property

    Public Property BUpper() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBUpper))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBUpper, value)
        End Set
    End Property

    Public Property BAUpper() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBAUpper))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBAUpper, value)
        End Set
    End Property

    Public Property BaBiUpper() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBaBiUpper))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBaBiUpper, value)
        End Set
    End Property

    Public Property PBUpper() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcPBUpper))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcPBUpper, value)
        End Set
    End Property

    Public Property QBUpper() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcQBUpper))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcQBUpper, value)
        End Set
    End Property

    Public Property EEUpper() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcEEUpper))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcEEUpper, value)
        End Set
    End Property

    Public Property VUUpper() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcVUUpper))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcVUUpper, value)
        End Set
    End Property

    Public Property LandingsUpper(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcLandingsUpper, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcLandingsUpper, value, iFleet)
        End Set
    End Property

    Public Property DiscardsUpper(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcDiscardsUpper, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcDiscardsUpper, value, iFleet)
        End Set
    End Property

    Public Property Bcv() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBcv))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBcv, value)
        End Set
    End Property

    Public Property BAcv() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBAcv))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBAcv, value)
        End Set
    End Property

    Public Property BaBicv() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBaBicv))
        End Get
        Set(value As Single)
            SetVariable(eVarNameFlags.mcBaBicv, value)
        End Set
    End Property

    Public Property PBcv() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcPBcv))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcPBcv, value)
        End Set
    End Property

    Public Property QBcv() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcQBcv))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcQBcv, value)
        End Set
    End Property

    Public Property EEcv() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcEEcv))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcEEcv, value)
        End Set
    End Property

    Public Property Dietcv() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcDCcv))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcDCcv, value)
        End Set
    End Property

    Public Property VUcv() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcVUcv))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcVUcv, value)
        End Set
    End Property

    Public Property Landingscv(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcLandingscv, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcLandingscv, value, iFleet)
        End Set
    End Property

    Public Property Discardscv(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcDiscardscv, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcDiscardscv, value, iFleet)
        End Set
    End Property

    Public Property DietMultiplier() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcDietMult))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcDietMult, value)
        End Set
    End Property

    Public Property Bbf() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBbf))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBbf, value)
        End Set
    End Property

    Public Property BAbf() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBAbf))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBAbf, value)
        End Set
    End Property

    Public Property BaBibf() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcBaBibf))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcBaBibf, value)
        End Set
    End Property

    Public Property PBbf() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcPBbf))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcPBbf, value)
        End Set
    End Property

    Public Property QBbf() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcQBbf))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcQBbf, value)
        End Set
    End Property

    Public Property EEbf() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcEEbf))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcEEbf, value)
        End Set
    End Property

    Public Property VUbf() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcVUbf))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcVUbf, value)
        End Set
    End Property

    Public Property Landingsbf(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcLandingsbf, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcLandingsbf, value, iFleet)
        End Set
    End Property

    Public Property Discardsbf(iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcDiscardsbf, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcDiscardsbf, value, iFleet)
        End Set
    End Property

    Public Property Dietsbf() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.mcDCbf))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.mcDCbf, value)
        End Set
    End Property

#End Region

End Class
