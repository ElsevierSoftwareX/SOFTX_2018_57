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
Imports System.ComponentModel
Imports System.Reflection
Imports System.Drawing

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Parameters that dictate the behaviour of the Value Chain plug-in.
''' </summary>
''' ===========================================================================
<Serializable()> _
Public Class cParameters
    Inherits EwEUtils.Database.cEwEDatabase.cOOPStorable

#Region " Private vars "

    Private m_bRunWithEcopath As Boolean = False
    Private m_bRunWithEcosim As Boolean = False
    Private m_bRunSearches As Boolean = False
    Private m_bResultsByFleet As Boolean = False
    Private m_liFleets As New List(Of Integer)
    Private m_bDeletePrompt As Boolean = True
    Private m_aggmode As eAggregationModeType = eAggregationModeType.FullModel

    Private m_bAutoSaveResults As Boolean = False

#End Region ' Private vars

#Region " Properties "

    Public Enum eAggregationModeType As Integer
        FullModel = 0
        ByFleet
        ByGroup
    End Enum

    Public ReadOnly Property EquilibriumFleetsToVary() As List(Of Integer)
        Get
            Return Me.m_liFleets
        End Get
    End Property

    Public Property EquilibriumEffortMin() As Single = 0.0
    Public Property EquilibriumEffortMax() As Single = 4.0!
    Public Property EquilibriumEffortIncrement() As Single = 0.25!

    Public Property RunWithEcopath() As Boolean
        Get
            Return Me.m_bRunWithEcopath
        End Get
        Set(ByVal bRunWithEcopath As Boolean)
            If (bRunWithEcopath <> Me.m_bRunWithEcopath) Then
                Me.m_bRunWithEcopath = bRunWithEcopath
                Me.SetChanged()
            End If
        End Set
    End Property

    Public Property RunWithEcosim() As Boolean
        Get
            Return Me.m_bRunWithEcosim
        End Get
        Set(ByVal bRunWithEcosim As Boolean)
            If (Me.m_bRunWithEcosim <> bRunWithEcosim) Then
                Me.m_bRunWithEcosim = bRunWithEcosim
                Me.SetChanged()
            End If
        End Set
    End Property

    Public Property RunWithSearches() As Boolean
        Get
            Return Me.m_bRunSearches
        End Get
        Set(ByVal bRunWithFishingPolicySearch As Boolean)
            If (Me.m_bRunSearches <> bRunWithFishingPolicySearch) Then
                Me.m_bRunSearches = bRunWithFishingPolicySearch
                Me.SetChanged()
            End If
        End Set
    End Property

    Public Property ZoomFactor As Single = 1.0!

    Public Property AggregationMode As eAggregationModeType
        Get
            Return Me.m_aggmode
        End Get
        Set(value As eAggregationModeType)
            Me.m_aggmode = value
            Me.SetChanged()
        End Set
    End Property

    <DefaultValue(True)> _
    Public Property DeletePrompt As Boolean
        Get
            Return Me.m_bDeletePrompt
        End Get
        Set(value As Boolean)
            If (value <> Me.m_bDeletePrompt) Then
                Me.m_bDeletePrompt = value
                Me.SetChanged()
            End If
        End Set
    End Property

#End Region ' Parameters

End Class
