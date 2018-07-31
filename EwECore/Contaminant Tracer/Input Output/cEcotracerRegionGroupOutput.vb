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
' $Log: cEcotracerRegionGroupOutput.vb,v $
' Revision 1.2  2009/01/16 18:30:25  jeroens
' eMessageSource renamed to eCoreComponentTypes
'
' Revision 1.1  2008/09/26 07:30:10  sherman
' --== DELETED HISTORY ==--
'
' Revision 1.4  2008/05/29 22:22:46  jeroens
' Moved eVarNameFlags to EwEUtils
'
' Revision 1.3  2008/03/26 21:00:56  joeb
' Added CBEnvironment
'
' Revision 1.2  2007/12/08 00:55:50  jeroens
' + Added time dimension
'
' Revision 1.1  2007/12/07 21:44:37  jeroens
' Initial version
'
'==============================================================================

Option Strict On
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

Public Class cEcotracerRegionGroupOutput
    Inherits cCoreInputOutputBase

    Private m_TracerData As cContaminantTracerDataStructures
    Private m_nRegions As Integer = 0
    Private m_nGroups As Integer = 0
    Private m_nTimeSteps As Integer = 0

#Region "Constructor"

    Public Sub New(ByRef TheCore As cCore, ByVal TracerData As cContaminantTracerDataStructures)
        MyBase.New(TheCore)

        Me.m_dataType = eDataTypes.EcotracerSimOutput
        Me.m_coreComponent = eCoreComponentType.Ecotracer

        Me.DBID = 1
        Me.Index = 1

        Me.m_TracerData = TracerData

    End Sub

#End Region

#Region "Implementation of GetVariable() GetVariable() GetStatus() SetStatus()"

    Public Overloads Function GetVariable(ByVal varName As eVarNameFlags, ByVal iRegion As Integer, ByVal iGroup As Integer, ByVal iTimeStep As Integer) As Single

        Try
            Select Case varName
                Case eVarNameFlags.Concentration
                    Return Me.m_TracerData.TracerConcByRegion(iRegion, iGroup, iTimeStep)
                Case eVarNameFlags.CEnvironment
                    Return Me.m_TracerData.TracerConcByRegion(iRegion, 0, iTimeStep)
                Case eVarNameFlags.CSum
                    Return Me.m_TracerData.TracerConcByRegion(iRegion, Me.m_nGroups + 1, iTimeStep)
                Case eVarNameFlags.ConcBio
                    Return Me.m_TracerData.TracerCBRegion(iRegion, iGroup, iTimeStep)
                Case eVarNameFlags.CBEnvironment
                    Return Me.m_TracerData.TracerCBRegion(iRegion, 0, iTimeStep)

            End Select

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        Return cCore.NULL_VALUE

    End Function

    Public Overloads Function SetVariable(ByVal varName As eVarNameFlags, ByVal newValue As Single, ByVal iRegion As Integer, ByVal iGroup As Integer, ByVal iTimeStep As Integer) As Boolean

        Try
            Debug.Assert(False, "cEcotracerRegionGroupOutput.setVaraible() not supported at this time.")
            Return False
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            Return False
        End Try

    End Function

    Public Overloads Function GetStatus(ByVal varName As eVarNameFlags, ByVal iRegion As Integer, ByVal iGroup As Integer, ByVal iTimeStep As Integer) As eStatusFlags
        Return eStatusFlags.OK Or eStatusFlags.NotEditable
    End Function

    Public Overloads Function SetStatus(ByVal varName As eVarNameFlags, ByVal newValue As eStatusFlags, ByVal iRegion As Integer, ByVal iGroup As Integer, ByVal iTimeStep As Integer) As Boolean
        Debug.Assert(False, "Not implemented yet.")
    End Function
#End Region

#Region "Variable via dot '.' operator"

    Public Property Concentration(ByVal iRegion As Integer, ByVal iGroup As Integer, ByVal iTimeStep As Integer) As Single
        Get
            Try
                Return GetVariable(eVarNameFlags.Concentration, iRegion, iGroup, iTimeStep)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try

        End Get

        Set(ByVal value As Single)
            Try
                SetVariable(eVarNameFlags.Concentration, value, iRegion, iGroup, iTimeStep)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set
    End Property


    Public Property CB(ByVal iRegion As Integer, ByVal iGroup As Integer, ByVal iTimeStep As Integer) As Single
        Get
            Try
                Return GetVariable(eVarNameFlags.ConcBio, iRegion, iGroup, iTimeStep)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try

        End Get

        Set(ByVal value As Single)
            Try
                SetVariable(eVarNameFlags.ConcBio, value, iRegion, iGroup, iTimeStep)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set
    End Property

    Public Property CEnvironment(ByVal iRegion As Integer, ByVal iTimeStep As Integer) As Single
        Get
            Try
                Return GetVariable(eVarNameFlags.CEnvironment, iRegion, 0, iTimeStep)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try

        End Get

        Set(ByVal value As Single)
            Try
                SetVariable(eVarNameFlags.CEnvironment, value, iRegion, 0, iTimeStep)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set
    End Property


    Public Property CBEnvironment(ByVal iRegion As Integer, ByVal iTimeStep As Integer) As Single

        Get
            Try
                Return GetVariable(eVarNameFlags.CBEnvironment, iRegion, 0, iTimeStep)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try
        End Get

        Set(ByVal value As Single)
            Try
                SetVariable(eVarNameFlags.CBEnvironment, value, iRegion, 0, iTimeStep)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set

    End Property

#End Region

#Region "Status Flags via dot '.' operator"

    Public Property ConcentrationStatus(ByVal iRegion As Integer, ByVal iGroup As Integer, ByVal iTimeStep As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.Concentration, iRegion, iGroup, iTimeStep)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Concentration, value, iRegion, iGroup, iTimeStep)
        End Set
    End Property

    Public Property CEnvironmentStatus(ByVal iRegion As Integer, ByVal iTimeStep As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.CEnvironment, iRegion, 0, iTimeStep)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CEnvironment, value, iRegion, 0, iTimeStep)
        End Set
    End Property

    Public Property CSumStatus(ByVal iRegion As Integer, ByVal iTimeStep As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.CSum, iRegion, Me.m_nGroups + 1, iTimeStep)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.CSum, value, iRegion, Me.m_nGroups + 1, iTimeStep)
        End Set
    End Property

#End Region

End Class
