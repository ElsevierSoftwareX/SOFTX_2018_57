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

''' ---------------------------------------------------------------------------
''' <summary>
''' 
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcotracerGroupInput
    Inherits cCoreGroupBase

#Region " Constructor "

    Friend Sub New(ByRef theCore As cCore, ByVal iDBID As Integer)
        MyBase.New(theCore)

        Dim val As cValue

        Try

            Me.DBID = iDBID
            Me.m_dataType = eDataTypes.EcotracerGroupInput
            Me.m_coreComponent = eCoreComponentType.Ecotracer

            'default OK status used for setVariable
            'see comment setVariable(...)
            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            ' CZero
            val = New cValue(New Single, eVarNameFlags.CZero, eStatusFlags.Null, eValueTypes.Sng)
            Me.m_values.Add(val.varName, val)

            ' CImmig
            val = New cValue(New Single, eVarNameFlags.CImmig, eStatusFlags.Null, eValueTypes.Sng)
            Me.m_values.Add(val.varName, val)

            ' CEnvironment
            val = New cValue(New Single, eVarNameFlags.CEnvironment, eStatusFlags.Null, eValueTypes.Sng)
            Me.m_values.Add(val.varName, val)

            ' CDecay
            val = New cValue(New Single, eVarNameFlags.CPhysicalDecayRate, eStatusFlags.Null, eValueTypes.Sng)
            Me.m_values.Add(val.varName, val)

            ' CAssimilationProp
            val = New cValue(New Single, eVarNameFlags.CAssimilationProp, eStatusFlags.Null, eValueTypes.Sng)
            Me.m_values.Add(val.varName, val)

            ' CMetablismRate
            val = New cValue(New Single, eVarNameFlags.CMetablismRate, eStatusFlags.Null, eValueTypes.Sng)
            Me.m_values.Add(val.varName, val)

            'set status flags to default values
            ResetStatusFlags()
            Me.AllowValidation = True

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcotracerScenarioGroup.")
            cLog.Write(Me.ToString & ".New Error creating new cEcotracerScenarioGroup. Error: " & ex.Message)
        End Try

    End Sub

#End Region ' Constructor

#Region " Variable via dot(.) operator"

    Public Property CZero() As Single
        Get
            Return CType(GetVariable(eVarNameFlags.CZero), Single)
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CZero, value)
        End Set
    End Property

    Public Property CImmig() As Single
        Get
            Return CType(GetVariable(eVarNameFlags.CImmig), Single)
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CImmig, value)
        End Set
    End Property

    Public Property CEnvironment() As Single
        Get
            Return CType(GetVariable(eVarNameFlags.CEnvironment), Single)
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CEnvironment, value)
        End Set
    End Property

    Public Property CDecay() As Single
        Get
            Return CType(GetVariable(eVarNameFlags.CPhysicalDecayRate), Single)
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CPhysicalDecayRate, value)
        End Set
    End Property

    Public Property CAssimilationProp() As Single
        Get
            Return CType(GetVariable(eVarNameFlags.CAssimilationProp), Single)
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CAssimilationProp, value)
        End Set
    End Property

    Public Property CMetablismRate() As Single
        Get
            Return CType(GetVariable(eVarNameFlags.CMetablismRate), Single)
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CMetablismRate, value)
        End Set
    End Property

#End Region ' Variable via dot(.) operator

#Region " Status Flags via dot(.) operator"

    Public Property CZeroStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.CZero)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CZero, value)
        End Set

    End Property

    Public Property CImmigStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.CImmig)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CImmig, value)
        End Set

    End Property

    Public Property CEnvironmentStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.CEnvironment)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CEnvironment, value)
        End Set

    End Property

    Public Property CDecayStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.CPhysicalDecayRate)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CPhysicalDecayRate, value)
        End Set

    End Property

    Public Property CExcretionRateStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.CAssimilationProp)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CAssimilationProp, value)
        End Set

    End Property

#End Region ' Status Flags via dot(.) operator

End Class
