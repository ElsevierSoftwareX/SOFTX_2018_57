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

Public Class cEcosimStats
    Inherits cCoreInputOutputBase

    Sub New(ByRef theCore As cCore)
        MyBase.New(theCore)

        Dim val As cValue = Nothing

        Me.DBID = cCore.NULL_VALUE
        Me.m_dataType = eDataTypes.EcoSimStatistics
        Me.m_coreComponent = eCoreComponentType.EcoSim

        Try

            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            'SS
            val = New cValue(New Single, eVarNameFlags.EcosimSS, eStatusFlags.NotEditable Or eStatusFlags.ValueComputed, eValueTypes.Sng)
            Me.m_values.Add(val.varName, val)

            'SSGroup
            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcosimSSGroup, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
            Me.m_values.Add(val.varName, val)

            'set status flags to their default values
            Me.ResetStatusFlags()

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcosimStats.")
            cLog.Write(Me.ToString & ".New(nGroups) Error creating new cEcosimStats. Error: " & ex.Message)
        End Try

    End Sub

    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
        Dim i As Integer

        'tell the base class to do the default values
        MyBase.ResetStatusFlags(bForceReset)

        Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
        Dim value As cValue
        For Each keyvalue In m_values
            Try
                value = keyvalue.Value

                Select Case value.varType
                    Case eValueTypes.SingleArray, eValueTypes.IntArray, eValueTypes.BoolArray
                        For i = 0 To value.Length
                            value.Status(i) = eStatusFlags.NotEditable Or eStatusFlags.ValueComputed
                        Next i

                    Case eValueTypes.Sng, eValueTypes.Int
                        value.Status = eStatusFlags.NotEditable Or eStatusFlags.ValueComputed

                End Select
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return False
            End Try
        Next keyvalue
        Return True

    End Function


    Public Property SS() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimSS))
        End Get
        Friend Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimSS, value)
        End Set
    End Property


    Public Property SSGroup(ByVal iGroup As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcosimSSGroup, iGroup))
        End Get
        Friend Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcosimSSGroup, value, iGroup)
        End Set
    End Property

    Public Property SSStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcosimSS)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimSS, value)
        End Set
    End Property

    Public Property SSGroupStatus(ByVal iGroup As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcosimSS, iGroup)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcosimSS, value, iGroup)
        End Set
    End Property

End Class
