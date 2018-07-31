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

Imports EwECore.ValueWrapper
Imports EwEUtils.Core

''' <summary>
''' Results, over all the time steps, at the end of an ecospace model run
''' </summary>
Public Class cEcospaceGroupOutput
    Inherits cCoreGroupBase

    Private m_spaceData As cEcospaceDataStructures
    Private m_CoreData As New Dictionary(Of eVarNameFlags, IResultsWrapper)

#Region "Constructor"

    Public Sub New(ByRef TheCore As cCore, ByVal EcoSpaceData As cEcospaceDataStructures, ByVal iGroup As Integer)
        MyBase.New(TheCore)

        Dim val As cValue = Nothing

        Me.DBID = iGroup '????
        Me.Index = iGroup
        Me.m_dataType = eDataTypes.EcospaceGroupOuput

        m_spaceData = EcoSpaceData

        'Data is loaded in Init
        'no validators
        'no validators
        val = New cValue(0, eVarNameFlags.EcospaceGroupBiomassStart, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(0, eVarNameFlags.EcospaceGroupBiomassEnd, eStatusFlags.OK, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        'no validators
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcospaceGroupCatchEnd, eStatusFlags.OK, eCoreCounterTypes.nFleets, AddressOf TheCore.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcospaceGroupCatchStart, eStatusFlags.OK, eCoreCounterTypes.nFleets, AddressOf TheCore.GetCoreCounter)
        m_values.Add(val.varName, val)

        'no validators
        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcospaceGroupValueStart, eStatusFlags.OK, eCoreCounterTypes.nFleets, AddressOf TheCore.GetCoreCounter)
        m_values.Add(val.varName, val)

        val = New cValueArray(eValueTypes.SingleArray, eVarNameFlags.EcospaceGroupValueEnd, eStatusFlags.OK, eCoreCounterTypes.nFleets, AddressOf TheCore.GetCoreCounter)
        m_values.Add(val.varName, val)

    End Sub


    Public Sub Init()

        m_CoreData.Clear()

        'jb 15-Nov-2010 Force the garbage collection on the memory that was released above
        GC.Collect()

        m_CoreData.Add(eVarNameFlags.EcospaceBiomassOverTime, New c3DResultsWrapper2Fixed(m_spaceData.ResultsByGroup, eSpaceResultsGroups.Biomass, Me.Index))
        m_CoreData.Add(eVarNameFlags.EcospaceRelativeBiomassOverTime, New c3DResultsWrapper2Fixed(m_spaceData.ResultsByGroup, eSpaceResultsGroups.RelativeBiomass, Me.Index))
        m_CoreData.Add(eVarNameFlags.EcospaceGroupValueOverTime, New c3DResultsWrapper2Fixed(m_spaceData.ResultsByGroup, eSpaceResultsGroups.Value, Me.Index))
        m_CoreData.Add(eVarNameFlags.EcospaceGroupCatchOverTime, New c3DResultsWrapper2Fixed(m_spaceData.ResultsByGroup, eSpaceResultsGroups.CatchBio, Me.Index))

    End Sub

#End Region

#Region "Data Validation and Status flag setting"
    'Status of ouput should be set to eStatusFlags.NotEditable Or eStatusFlags.Null for all timesteps that are not computed 
    'Once the data has be populate with the results from the last model run status should be set to eStatusFlags.NotEditable Or eStatusFlags.ValueComputed
    'This allows an interface to tell if the data at a timestep has been populated by the model run


    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
        Dim i As Integer

        Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
        Dim value As cValue
        For Each keyvalue In m_values
            Try
                value = keyvalue.Value

                Select Case value.varType
                    Case eValueTypes.SingleArray
                        For i = 1 To value.Length
                            value.Status(i) = eStatusFlags.NotEditable Or eStatusFlags.ValueComputed
                        Next i

                    Case eValueTypes.Str

                        If CStr(value.Value) = "" Then
                            value.Status = eStatusFlags.NotEditable Or eStatusFlags.Null
                        Else
                            value.Status = eStatusFlags.NotEditable Or eStatusFlags.OK
                        End If

                    Case Else
                        value.Status = eStatusFlags.NotEditable Or eStatusFlags.ValueComputed
                End Select

            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return False
            End Try
        Next keyvalue
        Return True

    End Function

    Public Overrides Function GetVariable(ByVal VarName As EwEUtils.Core.eVarNameFlags, Optional ByVal iIndex1 As Integer = -9999, Optional ByVal iIndex2 As Integer = -9999, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object

        If Not m_CoreData.ContainsKey(VarName) Then
            'NOT in list of sim vars so get the value from the base class GetVariable(...)
            Return MyBase.GetVariable(VarName, iIndex1, iIndex2)
        Else
            'Varname is access directly via the core data
            Return m_CoreData.Item(VarName).Value(iIndex1, iIndex2)
        End If

    End Function


    Public Overrides Function GetStatus(ByVal VarName As EwEUtils.Core.eVarNameFlags, Optional ByVal iIndex As Integer = -9999, Optional ByVal iThirdIndex As Integer = -9999) As eStatusFlags

        If Not m_CoreData.ContainsKey(VarName) Then
            'NOT in list of sim vars so get the value from the base class GetStatus(...)
            Return MyBase.GetStatus(VarName, iIndex)
        Else
            'all data managed by cEcospaceGroupOutput are read only outputs 
            Return eStatusFlags.NotEditable Or eStatusFlags.OK
        End If

    End Function


#End Region

#Region "Properties via dot '.' operator"

    Public ReadOnly Property Biomass(ByVal iTime As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceBiomassOverTime, iTime))
        End Get

    End Property

    Public ReadOnly Property RelativeBiomass(ByVal iTime As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceRelativeBiomassOverTime, iTime))
        End Get

    End Property

    Public ReadOnly Property CatchBiomass(ByVal iFleet As Integer, ByVal iTime As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceGroupCatchOverTime, iFleet, iTime))
        End Get

    End Property

    Public ReadOnly Property Value(ByVal iFleet As Integer, ByVal iTime As Integer) As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceGroupValueOverTime, iFleet, iTime))
        End Get

    End Property


#End Region

#Region "Summary values"

#Region "Properties via dot '.' operator"

    Public Property BiomassStart() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceGroupBiomassStart))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcospaceGroupBiomassStart, value)
        End Set
    End Property

    Public Property BiomassEnd() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceGroupBiomassEnd))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcospaceGroupBiomassEnd, value)
        End Set
    End Property


    Public Property CatchStart(ByVal iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceGroupCatchStart, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcospaceGroupCatchStart, value, iFleet)
        End Set
    End Property


    Public Property CatchEnd(ByVal iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceGroupCatchEnd, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcospaceGroupCatchEnd, value, iFleet)
        End Set
    End Property


    Public Property ValueStart(ByVal iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceGroupValueStart, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcospaceGroupValueStart, value, iFleet)
        End Set
    End Property

    Public Property ValueEnd(ByVal iFleet As Integer) As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.EcospaceGroupValueEnd, iFleet))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.EcospaceGroupValueEnd, value, iFleet)
        End Set
    End Property


#End Region

#Region "Status via dot '.' operator"

    Public Property BiomassStartStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceGroupBiomassStart)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceGroupBiomassStart, value)
        End Set
    End Property

    Public Property BiomassEndStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceGroupBiomassEnd)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceGroupBiomassEnd, value)
        End Set
    End Property


    Public Property CatchStartBiomassStatus(ByVal IFleet As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceGroupCatchStart, IFleet)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceGroupCatchStart, value, IFleet)
        End Set
    End Property


    Public Property CatchEndBiomassStatus(ByVal IFleet As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceGroupCatchEnd, IFleet)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceGroupCatchEnd, value, IFleet)
        End Set
    End Property


    Public Property ValueStartStatus(ByVal IFleet As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceGroupValueStart, IFleet)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceGroupValueStart, value, IFleet)
        End Set
    End Property

    Public Property ValueEndStatus(ByVal IFleet As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.EcospaceGroupValueEnd, IFleet)
        End Get

        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.EcospaceGroupValueEnd, value, IFleet)
        End Set
    End Property

#End Region

#End Region

End Class
