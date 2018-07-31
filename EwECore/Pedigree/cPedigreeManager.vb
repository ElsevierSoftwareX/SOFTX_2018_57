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

#Region " Imports "

Option Strict On
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
Imports System.Collections.Generic

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Class that contains and distributes <see cref="cPedigreeLevel">pedigree levels</see>,
''' and maintains group <see cref="cPedigreeManager.PedigreeGroupCV">pedigree assignments</see>.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cPedigreeManager
    Inherits cCoreInputOutputBase

#Region " Private classes "

    Private Class cPedigreeLevelSort
        Implements IComparer(Of cPedigreeLevel)

        Public Function Compare(x As cPedigreeLevel, y As cPedigreeLevel) As Integer Implements IComparer(Of cPedigreeLevel).Compare
            If (x Is Nothing) Or (y Is Nothing) Then Return 0
            If x.ConfidenceInterval < y.ConfidenceInterval Then Return 1
            If x.ConfidenceInterval > y.ConfidenceInterval Then Return -1
            Return 0
        End Function

    End Class
#End Region ' Private classes

#Region " Private vars "

    ''' <summary>The variable that this manager is responsible for.</summary>
    Private m_varName As eVarNameFlags = eVarNameFlags.NotSet
    ''' <summary>The pedigree levels that belong to the variable of this manager.</summary>
    Private m_levels As New cCoreInputOutputList(Of cPedigreeLevel)(eDataTypes.PedigreeLevel, 1)
    ''' <summary>Mapping of Core level index to local level ID.</summary>
    Private m_dictID As New Dictionary(Of Integer, Integer)

#End Region ' Private vars

#Region " Constructor and Cleanup "

    Friend Sub New(ByVal core As cCore, ByVal varName As eVarNameFlags, ByVal iDBID As Integer)
        MyBase.New(core)

        Dim val As cValue = Nothing

        Me.m_dataType = eDataTypes.PedigreeManager
        Me.m_coreComponent = eCoreComponentType.EcoPath
        Me.m_varName = varName
        Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        Me.AllowValidation = False

        Me.DBID = iDBID

        'Pedigree levels
        val = New cValueArray(eValueTypes.IntArray, eVarNameFlags.PedigreeLevel, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        Me.m_values.Add(val.varName, val)

        'Pedigree confidence intervals
        val = New cValueArray(eValueTypes.IntArray, eVarNameFlags.ConfidenceInterval, eStatusFlags.Null, eCoreCounterTypes.nGroups, AddressOf m_core.GetCoreCounter)
        Me.m_values.Add(val.varName, val)

        Me.AllowValidation = True

    End Sub

    Public Overrides Sub Clear()
        MyBase.Clear()

        For Each ped As cPedigreeLevel In Me.m_levels
            ped.Clear()
        Next
        Me.m_levels.Clear()

        Me.m_dictID.Clear()

    End Sub

#End Region ' Constructor

#Region " Properties "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the pedigree level index for a given variable. 
    ''' </summary>
    ''' <param name="iGroup">One-based index of the group.</param>
    ''' -----------------------------------------------------------------------
    Public Property PedigreeGroupLevel(ByVal iGroup As Integer) As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.PedigreeLevel, iGroup))
        End Get
        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.PedigreeLevel, value, iGroup)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the pedigree level index status for a given variable. 
    ''' </summary>
    ''' <param name="iVariable">One-based index of the variable for which to access the status.</param>
    ''' -----------------------------------------------------------------------
    Public Property PedigreeGroupLevelStatus(ByVal iVariable As Integer) As eStatusFlags
        Get
            Return Me.GetStatus(eVarNameFlags.PedigreeLevel, iVariable)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            Me.SetStatus(eVarNameFlags.PedigreeLevel, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the pedigree index for a given variable. 
    ''' </summary>
    ''' <param name="iGroup">One-based index of the group.</param>
    ''' -----------------------------------------------------------------------
    Public Property PedigreeGroupCV(ByVal iGroup As Integer) As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.ConfidenceInterval, iGroup))
        End Get
        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.ConfidenceInterval, value, iGroup)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the pedigree index status for a given variable. 
    ''' </summary>
    ''' <param name="iVariable">One-based index of the variable for which to access the status.</param>
    ''' -----------------------------------------------------------------------
    Public Property PedigreeGroupCVStatus(ByVal iVariable As Integer) As eStatusFlags
        Get
            Return Me.GetStatus(eVarNameFlags.ConfidenceInterval, iVariable)
        End Get
        Friend Set(ByVal value As eStatusFlags)
            Me.SetStatus(eVarNameFlags.ConfidenceInterval, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the number of pedigree levels in the manager.
    ''' </summary>
    ''' <remarks>
    ''' Level indexing is one-base. It's just so that you know it. Really. ONE
    ''' based; let there be no confusion. At least as little confusion as
    ''' possibly possible. Right. There you go. I hope.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property NumLevels() As Integer
        Get
            Return Me.m_levels.Count
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get a pedigree level from the manager.
    ''' </summary>
    ''' <param name="iLevel">The one-based index of the level to obtain.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Level(ByVal iLevel As Integer) As cPedigreeLevel
        Get
            If (iLevel <= 0) Then Return Nothing
            Return Me.m_levels(iLevel)
        End Get
    End Property

#End Region ' Properties

#Region " Public update interfaces "

    Private m_bInUpdate As Boolean = False

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Commit pedigree levels to the EwE core.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function UpdatePedigreeLevels() As Boolean

        Dim data As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim level As cPedigreeLevel = Nothing

        Me.m_bInUpdate = True

        Try
            For iLevel As Integer = 1 To Me.NumLevels

                Try

                    level = Me.m_levels(iLevel)
                    data.PedigreeLevelName(level.Index) = level.Name
                    data.PedigreeLevelDescription(level.Index) = level.Description
                    data.PedigreeLevelColor(level.Index) = level.PoolColor
                    data.PedigreeLevelIndexValue(level.Index) = level.IndexValue
                    data.PedigreeLevelConfidence(level.Index) = level.ConfidenceInterval

                    ' Issue #796: in some daily build databases the description field cannot be empty
                    If String.IsNullOrEmpty(data.PedigreeLevelDescription(level.Index)) Then data.PedigreeLevelDescription(level.Index) = " "

                    Me.m_core.onChanged(level, eMessageType.DataModified)

                Catch ex As Exception
                    cLog.Write(Me.ToString & ".Update() level failed to update DBID=" & level.DBID)
                    Debug.Assert(False, Me.ToString & ".Update() level failed to update DBID=" & level.DBID)
                End Try

            Next iLevel

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Update() Error: " & ex.Message)
            Return False
        End Try

        Me.m_bInUpdate = False

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Commit pedigree assignments to the EwE core.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function UpdatePedigreeAssignments() As Boolean

        Dim data As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim iVariable As Integer = Me.m_core.PedigreeVariableIndex(Me.m_varName)

        ' Store CV values
        For iGroup As Integer = 1 To Me.m_core.nGroups
            Try
                ' Store
                data.PedigreeEcopathGroupCV(iGroup, iVariable) = Me.PedigreeGroupCV(iGroup)
                data.PedigreeEcopathGroupLevel(iGroup, iVariable) = Me.PedigreeGroupLevel(iGroup)
            Catch ex As Exception
                cLog.Write(Me.ToString & ".UpdatePedigree() group failed to update DBID=" & iGroup)
                Debug.Assert(False, Me.ToString & ".UpdatePedigree() group failed to update DBID=" & iGroup)
            End Try
        Next
        Return True

    End Function

#End Region ' Public update interfaces

#Region " Configuration "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create pedigree levels.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub Init()

        Dim level As cPedigreeLevel = Nothing
        Dim data As cEcopathDataStructures = Me.m_core.m_EcoPathData

        Me.m_levels.Clear()
        Me.m_dictID.Clear()

        For iLevel As Integer = 1 To data.NumPedigreeLevels
            If data.PedigreeLevelVarName(iLevel) = Me.m_varName Then

                level = New cPedigreeLevel(Me.m_core, Me, data.PedigreeLevelDBID(iLevel))

                ' Config level
                level.AllowValidation = False
                level.Sequence = Me.m_levels.Count + 1 ' one based
                level.Index = iLevel
                level.AllowValidation = True

                ' Update local admin
                Me.m_levels.Add(level)
                Me.m_dictID(iLevel) = level.Sequence

            End If
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load pedigree levels.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function LoadPedigreeLevels() As Boolean

        Dim bSucces As Boolean = True

        If (Me.m_bInUpdate) Then Return bSucces

        Try
            Dim level As cPedigreeLevel = Nothing
            Dim data As cEcopathDataStructures = Me.m_core.m_EcoPathData

            For iLevel As Integer = 1 To Me.NumLevels

                level = Me.m_levels(iLevel)

                level.AllowValidation = False
                level.Name = data.PedigreeLevelName(level.Index)
                level.Description = data.PedigreeLevelDescription(level.Index)
                level.PoolColor = data.PedigreeLevelColor(level.Index)
                level.IndexValue = data.PedigreeLevelIndexValue(level.Index)
                level.ConfidenceInterval = data.PedigreeLevelConfidence(level.Index)
                level.VariableName = Me.m_varName
                level.IsEstimated = data.PedigreeLevelEstimated(level.Index)
                level.ResetStatusFlags()
                level.AllowValidation = True

            Next

        Catch ex As Exception
            bSucces = False
            Debug.Assert(False, Me.ToString & ".Load() Error: " & ex.Message)
        End Try

        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load or update assigned pedigree CV values.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function LoadPedigreeAssignments() As Boolean

        Dim data As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim iVariable As Integer = Me.m_core.PedigreeVariableIndex(Me.m_varName)

        ' Sanity check
        Debug.Assert(data.PedigreeEcopathGroupCV IsNot Nothing, "Pedigree data not dimensioned")

        Me.AllowValidation = False

        ' Map core level indexes to local manager indexes
        For iGroup As Integer = 1 To Me.m_core.nGroups
            ' No assignment = 0
            Me.PedigreeGroupLevel(iGroup) = data.PedigreeEcopathGroupLevel(iGroup, iVariable)
            Me.PedigreeGroupCV(iGroup) = data.PedigreeEcopathGroupCV(iGroup, iVariable)
        Next
        Me.ResetStatusFlags()
        Me.AllowValidation = True

        Return True

    End Function

    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean

        MyBase.ResetStatusFlags(bForceReset)
        For iGroup As Integer = 1 To Me.m_core.nGroups
            Me.Set_Pedigree_Flags(Me.m_core.EcoPathGroupInputs(iGroup))
        Next
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Set the status flags of pedigree.
    ''' </summary>
    ''' <param name="group">The group to update.</param>
    ''' -----------------------------------------------------------------------
    Friend Sub Set_Pedigree_Flags(ByVal group As cEcoPathGroupInput)

        Dim epdata As cEcopathDataStructures = Me.m_core.m_EcoPathData

        ' Borrow status flags from groups
        Me.AllowValidation = False

        ' JS 13Nov13: Addressed issue #1301 (VC email "I was doing the pedigree 
        '             for a model and noted that the table does not allow entry
        '             of P/B for producers (should have B and P/B) and of B for
        '             detritus (should only have B))
        Select Case Me.m_varName

            Case eVarNameFlags.BiomassAreaInput
                Me.ClearStatusFlags(eVarNameFlags.ConfidenceInterval, eStatusFlags.NotEditable Or eStatusFlags.Null, group.Index)

            Case eVarNameFlags.PBInput
                If (group.IsDetritus()) Then
                    Me.SetStatusFlags(eVarNameFlags.ConfidenceInterval, eStatusFlags.NotEditable Or eStatusFlags.Null, group.Index)
                Else
                    Me.ClearStatusFlags(eVarNameFlags.ConfidenceInterval, eStatusFlags.NotEditable, group.Index)
                End If

            Case eVarNameFlags.QBInput
                If (group.IsDetritus() Or group.IsProducer()) Then
                    Me.SetStatusFlags(eVarNameFlags.ConfidenceInterval, eStatusFlags.NotEditable Or eStatusFlags.Null, group.Index)
                Else
                    Me.ClearStatusFlags(eVarNameFlags.ConfidenceInterval, eStatusFlags.NotEditable, group.Index)
                End If

            Case eVarNameFlags.DietComp
                If (group.IsDetritus() Or group.IsProducer()) Then
                    Me.SetStatusFlags(eVarNameFlags.ConfidenceInterval, eStatusFlags.NotEditable, group.Index)
                Else
                    Me.ClearStatusFlags(eVarNameFlags.ConfidenceInterval, eStatusFlags.NotEditable, group.Index)
                End If

            Case eVarNameFlags.TCatchInput
                If epdata.fCatch(group.Index) > 0 Then
                    Me.ClearStatusFlags(eVarNameFlags.ConfidenceInterval, eStatusFlags.NotEditable, group.Index)
                Else
                    Me.SetStatusFlags(eVarNameFlags.ConfidenceInterval, eStatusFlags.NotEditable, group.Index)
                End If

        End Select

        Me.AllowValidation = True

    End Sub

#End Region ' Configuration

End Class