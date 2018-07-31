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
Option Explicit On

Imports EwEUtils.Core

''' <summary>
''' Manages all mediated shape interactions.
''' </summary>
Public Class cMediatedInteractionManager

    Private m_core As cCore
    Private m_interactionsPredPrey As New Dictionary(Of String, cPredPreyInteraction)
    Private m_interactionsLandings As New Dictionary(Of String, cLandingsInteraction)
    'm_interactionGroup is used for Mortality Forcing data which is stored by group 
    Private m_interactionGroup As New Dictionary(Of String, cPredPreyInteraction)
    Private m_EPData As cEcopathDataStructures
    Private m_ESData As cEcosimDatastructures

#Region "Private functions"

    Friend Function getHashKey(ByVal iIndex1 As Integer, ByVal iIndex2 As Integer) As String
        Return iIndex1.ToString & "_" & iIndex2.ToString
    End Function

#End Region

#Region "Construction and Initialization"

    Friend Sub New(ByRef EcoPathData As cEcopathDataStructures, ByRef EcoSimData As cEcosimDatastructures, ByRef theCore As cCore)

        m_EPData = EcoPathData
        m_ESData = EcoSimData
        m_core = theCore

        Init()
        Load()

    End Sub

    Public Function Init() As Boolean

        Me.m_interactionsPredPrey.Clear()
        Me.m_interactionsLandings.Clear()
        Me.m_interactionGroup.Clear()

        Dim lstPredPreyApTypes As New List(Of eForcingFunctionApplication)({eForcingFunctionApplication.Import,
                                                                           eForcingFunctionApplication.ProductionRate, eForcingFunctionApplication.SearchRate,
                                                                           eForcingFunctionApplication.VulAndArea, eForcingFunctionApplication.Vulnerability, eForcingFunctionApplication.ArenaArea})

        For ipred As Integer = 1 To m_EPData.NumGroups
            For iprey As Integer = 1 To m_EPData.NumGroups

                If Me.isPredPrey(ipred, iprey) Then
                    Dim interaction As New cPredPreyInteraction(ipred, iprey, Me, lstPredPreyApTypes)
                    Me.m_interactionsPredPrey.Add(getHashKey(ipred, iprey), interaction)
                End If

            Next iprey
        Next ipred


        Dim lstLandingApTypes As New List(Of eForcingFunctionApplication)({eForcingFunctionApplication.OffVesselPrice})
        For iFleet As Integer = 1 To m_EPData.NumFleet
            For iGroup As Integer = 1 To Me.m_EPData.NumGroups

                If Me.isLandings(iFleet, iGroup) Then
                    Dim interaction As New cLandingsInteraction(iFleet, iGroup, Me, lstLandingApTypes)
                    Me.m_interactionsLandings.Add(getHashKey(iFleet, iGroup), interaction)
                End If

            Next
        Next

        Dim lstMortApTypes As New List(Of eForcingFunctionApplication)({eForcingFunctionApplication.MortOther})
        For ipred As Integer = 1 To m_EPData.NumLiving

            'Stored Group x Group
            Dim interaction As New cPredPreyInteraction(ipred, ipred, Me, lstMortApTypes)
            Me.m_interactionGroup.Add(getHashKey(ipred, ipred), interaction)

        Next ipred


    End Function

    Public Function Load() As Boolean
        For Each interaction As cMediatedInteraction In Me.m_interactionsPredPrey.Values
            interaction.Load()
        Next
        For Each interaction As cMediatedInteraction In Me.m_interactionsLandings.Values
            interaction.Load()
        Next

        For Each interaction As cMediatedInteraction In Me.m_interactionGroup.Values
            interaction.Load()
        Next

    End Function

    Public Sub Clear()

        Try
            For Each interaction As cMediatedInteraction In Me.m_interactionsPredPrey.Values
                interaction.Clear()
            Next
            For Each interaction As cMediatedInteraction In Me.m_interactionsLandings.Values
                interaction.Clear()
            Next
            For Each interaction As cMediatedInteraction In Me.m_interactionGroup.Values
                interaction.Clear()
            Next

            Me.m_interactionsPredPrey.Clear()
            Me.m_interactionsLandings.Clear()
            Me.m_interactionGroup.Clear()

        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

#End Region

#Region "Public Properties"

    Public ReadOnly Property isPredPrey(ByVal PredIndex As Integer, ByVal PreyIndex As Integer) As Boolean
        Get
            Try
                'True for primary producer pairs
                If (PredIndex = PreyIndex) And m_EPData.PP(PreyIndex) = 1 Then
                    Return True
                End If

                'Detritus Groups
                If (PredIndex > m_EPData.NumLiving) Then
                    If PredIndex = PreyIndex Then
                        'this is the detritus diagonal that appears in the grids
                        Return True
                    End If
                    'the DC() matrix will contain values for detrius as a predator for all consumer groups
                    'So this needs to return false for all detritus groups that are not Pred = Prey (diagonal in the matrix)
                    Return False
                End If

                ' Return whether there is a predation interaction
                Return (m_EPData.DC(PredIndex, PreyIndex) > 0)

            Catch ex As Exception
                Return False
            End Try

        End Get
    End Property

    Public ReadOnly Property PredPreyInteraction(ByVal PredIndex As Integer, ByVal PreyIndex As Integer) As cPredPreyInteraction
        Get
            Try
                Dim key As String = getHashKey(PredIndex, PreyIndex)
                If m_interactionsPredPrey.ContainsKey(key) Then
                    Return Me.m_interactionsPredPrey.Item(key)
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".Item() Failed to find cPredPreyInteraction().")
                Return Nothing
            End Try
        End Get
    End Property




    Public ReadOnly Property GroupInteraction(ByVal PredIndex As Integer) As cPredPreyInteraction
        Get
            Try
                Dim key As String = getHashKey(PredIndex, PredIndex)
                If Me.m_interactionGroup.ContainsKey(key) Then
                    Return Me.m_interactionGroup.Item(key)
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".Item() GroupInteraction() Failed to find cPredPreyInteraction().")
                Return Nothing
            End Try
        End Get
    End Property


    Public ReadOnly Property isLandings(ByVal iFleet As Integer, ByVal iGroup As Integer) As Boolean
        Get
            Try
                Return (Me.m_EPData.Landing(iFleet, iGroup) > 0)
            Catch ex As Exception
                Return False
            End Try
        End Get
    End Property

    Public ReadOnly Property LandingInteraction(ByVal iFleet As Integer, ByVal iGroup As Integer) As cLandingsInteraction
        Get
            Try
                Dim key As String = getHashKey(iFleet, iGroup)
                If m_interactionsLandings.ContainsKey(key) Then
                    Return Me.m_interactionsLandings.Item(key)
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".Item() Failed to find cPriceElasticityInteraction().")
                Return Nothing
            End Try
        End Get
    End Property



    ''' <summary>
    ''' Get the maximum number of shapes that can be assigned to an interaction.
    ''' </summary>
    Public ReadOnly Property MaxNShapes() As Integer
        Get
            Return cMediationDataStructures.MAXFUNCTIONS
        End Get
    End Property

#End Region

#Region "Friend functions "

    Friend Function getEcoPathData() As cEcopathDataStructures
        Return m_EPData
    End Function

    Friend Function getEcoSimData() As cEcosimDatastructures
        Return m_ESData
    End Function

    Friend Function getCore() As cCore
        Return m_core
    End Function

#End Region

End Class
