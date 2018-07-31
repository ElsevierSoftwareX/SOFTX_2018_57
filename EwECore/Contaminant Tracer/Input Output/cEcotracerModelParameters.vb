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

'==============================================================================
'
' $Log: cEcotracerModelParameters.vb,v $
' Revision 1.2  2009/01/16 18:30:25  jeroens
' eMessageSource renamed to eCoreComponentTypes
'
' Revision 1.1  2008/09/26 07:30:10  sherman
' --== DELETED HISTORY ==--
'
' Revision 1.5  2008/05/29 22:22:46  jeroens
' Moved eVarNameFlags to EwEUtils
'
' Revision 1.4  2008/01/08 11:29:04  jeroens
' Woops
'
' Revision 1.3  2008/01/06 11:00:46  jeroens
' * Inflow and outflow locked for input
'
' Revision 1.2  2007/12/05 03:48:45  jeroens
' * Added forcing no support
'
' Revision 1.1  2007/11/26 02:06:48  jeroens
' Initial version
'
'==============================================================================

Option Strict On
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

''' ---------------------------------------------------------------------------
''' <summary>
''' 
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcotracerModelParameters
    Inherits cCoreInputOutputBase

#Region " Constructor "

    Sub New(ByRef theCore As cCore)
        MyBase.New(theCore)

        Dim val As cValue

        Try

            m_dataType = eDataTypes.EcotracerModelParameters
            m_coreComponent = eCoreComponentType.Ecotracer

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            ' CZero
            val = New cValue(New Single, eVarNameFlags.CZero, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' CInflow
            val = New cValue(New Single, eVarNameFlags.CInflow, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' COutflow
            val = New cValue(New Single, eVarNameFlags.COutflow, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' CDecay
            val = New cValue(New Single, eVarNameFlags.CPhysicalDecayRate, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'ConForceNumber
            val = New cValue(New Integer, eVarNameFlags.ConForceNumber, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            'Max number of time steps
            val = New cValue(New Integer, eVarNameFlags.ConMaxTimeSteps, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            'set status flags to default values
            ResetStatusFlags()
            Me.AllowValidation = True

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcotracerScenario.")
            cLog.Write(Me.ToString & ".New Error creating new cEcotracerScenario. Error: " & ex.Message)
        End Try

    End Sub

#End Region ' Constructor

#Region " Overrides "

    'Friend Overrides Function ResetStatusFlags() As Boolean

    '    If Not MyBase.ResetStatusFlags() Then Return False

    '    Me.SetStatusFlags(eVarNameFlags.CInflow, eStatusFlags.NotEditable, 0)
    '    Me.SetStatusFlags(eVarNameFlags.COutflow, eStatusFlags.NotEditable, 0)

    '    Return False

    'End Function

#End Region ' Overrides

#Region " Variable via dot(.) operator"

    Public Property CZero() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.CZero))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CZero, value)
        End Set
    End Property

    Public Property CInflow() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.CInflow))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CInflow, value)
        End Set
    End Property

    Public Property COutflow() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.COutflow))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.COutflow, value)
        End Set
    End Property

    Public Property CDecay() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.CPhysicalDecayRate))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CPhysicalDecayRate, value)
        End Set
    End Property

    Public Property ConForceNumber() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.ConForceNumber))
        End Get
        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.ConForceNumber, value)
        End Set
    End Property

    'ConMaxTimeSteps


    Public Property MaxTimeSteps() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.ConMaxTimeSteps))
        End Get
        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.ConMaxTimeSteps, value)
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

    Public Property CInflowStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.CInflow)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CInflow, value)
        End Set

    End Property

    Public Property COutflowStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.COutflow)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.COutflow, value)
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

    Public Property MaxTimeStepsStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.ConMaxTimeSteps)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.ConMaxTimeSteps, value)
        End Set

    End Property

#End Region ' Status Flags via dot(.) operator

End Class
