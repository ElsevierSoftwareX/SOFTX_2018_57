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
''' Statistics for the last Ecospace run.
''' </summary>
''' <remarks>One object for all the groups and stats</remarks>
Public Class cEcospaceStats
    Inherits cCoreInputOutputBase

    Sub New(ByRef theCore As cCore, ByVal DBID As Integer)
        MyBase.New(theCore)

        Me.DBID = DBID
        m_dataType = eDataTypes.EcospaceStatistics
        m_coreComponent = eCoreComponentType.EcoSpace

        Dim val As cValue

        Try

            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            'SS
            val = New cValue(New Single, eVarNameFlags.EcospaceSS, eStatusFlags.NotEditable, eValueTypes.Sng)
            m_values.Add(val.varName, val)


            val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcospaceSSGroup, eStatusFlags.NotEditable, eCoreCounterTypes.nGroups, AddressOf theCore.GetCoreCounter)
            m_values.Add(val.varName, val)

            val = New cValue(New Boolean, eVarNameFlags.EcospaceSSCalculated, eStatusFlags.NotEditable, eValueTypes.Bool)
            m_values.Add(val.varName, val)



            ''Region SS
            'val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcospaceRegionSS, eStatusFlags.NotEditable, eCoreCounterTypes.nRegions, _
            '             AddressOf m_core.GetCoreCounter)
            'm_values.Add(val.varName, val)

            'set status flags to their default values
            ResetStatusFlags()

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcospaceStats.")
            cLog.Write(Me.ToString & ".New(nGroups) Error creating new cEcospaceStats. Error: " & ex.Message)
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

    ''' <summary>
    ''' SS sumed across all groups and variables
    ''' </summary>
    ''' <returns>sumof(log(observed/predicted))</returns>
    Public Property SS() As Double
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceSS))
        End Get
        Set(ByVal value As Double)
            SetVariable(eVarNameFlags.EcospaceSS, value)
        End Set
    End Property



    Public Property SSStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceSS)
        End Get
        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceSS, value)
        End Set
    End Property


    ''' <summary>
    ''' SS by group
    ''' </summary>
    ''' <param name="iGrp"></param>
    ''' <returns>sumof(log(observed(igroup)/predicted(igroup))</returns>
    Public Property SSGroup(iGrp As Integer) As Double
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceSSGroup, iGrp))
        End Get
        Set(ByVal value As Double)
            SetVariable(eVarNameFlags.EcospaceSSGroup, value, iGrp)
        End Set
    End Property



    Public Property SSGroupStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceSSGroup)
        End Get
        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceSSGroup, value)
        End Set
    End Property

    Public Property isSSCalculated As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.EcospaceSSCalculated))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.EcospaceSSCalculated, value)
        End Set
    End Property

    Public ReadOnly Property isSSCalculatedStatus As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceSSCalculated)
        End Get
    End Property

    'EcospaceSSCalculated

#Region "No longer implemented"

#If DEADCODE Then
    
    Public Property RegionSS(ByVal iRegion As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceRegionSS, iRegion))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcospaceRegionSS, value, iRegion)
        End Set
    End Property



    Public Property RegionSSStatus(ByVal iRegion As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceRegionSS, iRegion)
        End Get
        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceRegionSS, value, iRegion)
        End Set
    End Property
        
#End If

#End Region



End Class
