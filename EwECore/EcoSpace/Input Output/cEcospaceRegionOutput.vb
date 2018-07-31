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

Public Class cEcospaceRegionOutput
    Inherits cCoreInputOutputBase

    Private m_spacedata As cEcospaceDataStructures
    Private m_CoreArrays As New Dictionary(Of eVarNameFlags, IResultsWrapper)
    Private m_CatchFleetGroup(,,) As Single

#Region "Constructor"

    Public Sub New(ByRef TheCore As cCore, ByVal EcospaceData As cEcospaceDataStructures, ByVal iRegion As Integer)
        MyBase.New(TheCore)

        Me.m_spacedata = EcospaceData

        Me.DBID = iRegion '????
        Me.Index = iRegion
        Me.m_dataType = eDataTypes.EcospaceRegionResults

        Dim val As cValue

        'Weirdness
        'There are three ways of managing data
        'If the data has a core array then use that directly via the m_CoreArrays dictionary
        'If no core array and the data can fit into a cValue object then use that, only one variable index
        'If no core array and the data contains more then one variable index then use a local buffer

        'cValue objects
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcospaceRegionBiomassStart, eStatusFlags.OK, eCoreCounterTypes.nGroups, AddressOf TheCore.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcospaceRegionBiomassEnd, eStatusFlags.OK, eCoreCounterTypes.nGroups, AddressOf TheCore.GetCoreCounter)
        m_values.Add(val.varName, val)

    End Sub


    Public Sub Init()

        Try
            m_CoreArrays.Clear()
            m_CoreArrays.Add(eVarNameFlags.EcospaceRegionBiomass, New c3DResultsWrapper(m_spacedata.ResultsRegionGroup, Me.Index))
            m_CoreArrays.Add(eVarNameFlags.EcospaceRegionBiomassYear, New c3DResultsWrapper(m_spacedata.ResultsRegionGroupYear, Me.Index))
            m_CoreArrays.Add(eVarNameFlags.EcospaceRegionFleetGroupCatch, New c4DResultsWrapperFirstFixed(m_spacedata.ResultsCatchRegionGearGroup, Me.Index))
            m_CoreArrays.Add(eVarNameFlags.EcospaceRegionFleetGroupCatchYear, New c4DResultsWrapperFirstFixed(m_spacedata.ResultsCatchRegionGearGroup, Me.Index))
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Init() Error: " & ex.Message)
            cLog.Write(ex)
        End Try

    End Sub

#End Region

#Region "Implementation of GetVariable() SetVariable() GetStatus() SetStatus()"

    Public Overrides Function GetVariable(ByVal varName As eVarNameFlags, Optional ByVal iFirstIndex As Integer = cCore.NULL_VALUE, Optional ByVal iSecondIndex As Integer = cCore.NULL_VALUE, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object
        Try

            If Not m_CoreArrays.ContainsKey(varName) Then
                Debug.Assert(iSecondIndex = cCore.NULL_VALUE, Me.ToString & ".GetVariable() called with optional argument iSecondIndex for variable " & varName.ToString & " this can not be handled for this variable.")
                'NOT in list of sim vars so get the value from the base class GetVariable(...)
                Return MyBase.GetVariable(varName, iFirstIndex)
            Else
                'Varname is access directly via the core data
                Return m_CoreArrays.Item(varName).Value(iFirstIndex, iSecondIndex, iIndex3)
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

        Return cCore.NULL_VALUE

    End Function


    Public Overloads Function GetStatus(ByVal varName As eVarNameFlags, ByVal iFleet As Integer, ByVal iGroup As Integer) As eStatusFlags
        Return eStatusFlags.OK 'Oh Yeah 
    End Function

    Public Overloads Function SetStatus(ByVal varName As eVarNameFlags, ByVal newValue As eStatusFlags, ByVal iFleet As Integer, ByVal iGroup As Integer) As Boolean
        Debug.Assert(False, "Not implemented yet.")
    End Function

    Friend Overrides Function Resize() As Boolean
        MyBase.Resize()

        'resize local buffer
        ReDim Me.m_CatchFleetGroup(1, Me.m_core.nFleets, Me.m_core.nGroups)
        Return True
    End Function


#End Region

#Region "Variable via dot '.' operator"

    Public Property BiomassStart(ByVal iGroup As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceRegionBiomassStart, iGroup))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcospaceRegionBiomassStart, value, iGroup)
        End Set
    End Property

    Public Property BiomassEnd(ByVal iGroup As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceRegionBiomassEnd, iGroup))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcospaceRegionBiomassEnd, value, iGroup)
        End Set
    End Property

    Public Property CatchFleetGroupStart(ByVal iFleet As Integer, ByVal iGroup As Integer) As Single
        Get
            Try
                Return Me.m_CatchFleetGroup(0, iFleet, iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try
        End Get

        Set(ByVal value As Single)
            Try
                Me.m_CatchFleetGroup(0, iFleet, iGroup) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set
    End Property


    Public Property CatchFleetGroupEnd(ByVal iFleet As Integer, ByVal iGroup As Integer) As Single
        Get
            Try
                Return Me.m_CatchFleetGroup(1, iFleet, iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try
        End Get

        Set(ByVal value As Single)
            Try
                Me.m_CatchFleetGroup(1, iFleet, iGroup) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set
    End Property

    Public ReadOnly Property BiomassByTime(ByVal iGroup As Integer, ByVal iTimeStep As Integer) As Single
        Get
            Try
                Return DirectCast(GetVariable(eVarNameFlags.EcospaceRegionBiomass, iGroup, iTimeStep), Single)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try
        End Get
    End Property

    Public ReadOnly Property BiomassByYear(ByVal iGroup As Integer, ByVal iYear As Integer) As Single
        Get
            Try
                Return DirectCast(GetVariable(eVarNameFlags.EcospaceRegionBiomassYear, iGroup, iYear), Single)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try
        End Get
    End Property

    Public ReadOnly Property CatchFleetGroupTime(ByVal iFleet As Integer, ByVal iGroup As Integer, ByVal iTimeStep As Integer) As Single
        Get
            Try
                Return DirectCast(GetVariable(eVarNameFlags.EcospaceRegionFleetGroupCatch, iFleet, iGroup, iTimeStep), Single)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try
        End Get
    End Property

    Public ReadOnly Property CatchFleetGroupYear(ByVal iFleet As Integer, ByVal iGroup As Integer, ByVal iYear As Integer) As Single
        Get
            Try
                Return DirectCast(GetVariable(eVarNameFlags.EcospaceRegionFleetGroupCatchYear, iFleet, iGroup, iYear), Single)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return cCore.NULL_VALUE
            End Try
        End Get
    End Property

#End Region

#Region "Status Flags via dot '.' operator"

    Public Property BiomassStartStatus(ByVal iGroup As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceRegionBiomassStart, iGroup)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceRegionBiomassStart, value, iGroup)
        End Set
    End Property

    Public Property BiomassEndStatus(ByVal iGroup As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceRegionBiomassEnd, iGroup)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceRegionBiomassEnd, value, iGroup)
        End Set
    End Property


    Public Property CatchFleetGroupStartStatus(ByVal iGroup As Integer, ByVal iFleet As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceRegionCatchStart, iGroup, iFleet)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceRegionCatchStart, value, iGroup, iFleet)
        End Set
    End Property


    Public Property CatchFleetGroupEndStatus(ByVal iGroup As Integer, ByVal iFleet As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceRegionCatchEnd, iGroup, iFleet)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceRegionCatchEnd, value, iGroup, iFleet)
        End Set
    End Property

#End Region

End Class
