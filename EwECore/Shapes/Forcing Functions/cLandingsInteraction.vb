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

Option Explicit On
Imports System.ComponentModel
Imports EwEUtils.Core

''' <summary>
''' Class to wrap the shape and function type modifiers for a price elasticity interaction
''' </summary>
Public Class cLandingsInteraction
    Inherits cMediatedInteraction

#Region "Private Data"

    Private m_iFleet As Integer
    Private m_iGroup As Integer

#End Region

#Region "Construction and Initialization"

    ''' <summary>
    ''' Create a new interaction.
    ''' </summary>
    ''' <param name="iFleet"><see cref="cCoreGroupBase.Index">Fleet index</see>.</param>
    ''' <param name="iGroup"><see cref="cCoreGroupBase.Index">Group index</see>.</param>
    ''' <param name="manager"><see cref="cMediatedInteractionManager">Mediated interaction manager</see>.</param>
    Sub New(ByVal iFleet As Integer, ByVal iGroup As Integer, ByVal manager As cMediatedInteractionManager, ApplicationTypes As List(Of eForcingFunctionApplication))

        Me.m_dbid = cCore.NULL_VALUE '???

        Me.m_iFleet = iFleet
        Me.m_iGroup = iGroup
        Me.m_manager = manager

        'initialize the list of shape/functiontype pairs with the number of function modifiers from Ecosim
        'Modifiers that are not used will have a NULL shape in the cShapeFunctionTypePair object
        For i As Integer = 1 To Me.MaxNumShapes
            Me.m_SFPairs.Add(New cShapeFunctionTypePair())
        Next

    End Sub

    ''' <summary>
    ''' Build the list of shapes used by this interaction from the underlying Ecosim data.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    Friend Overrides Function Load() As Boolean

        Dim esdata As cEcosimDatastructures = m_manager.getEcoSimData
        Dim SFPair As cShapeFunctionTypePair
        Dim man As cLandingsMediationShapeManager = Me.m_manager.getCore.LandingsShapeManager
        Dim bSucces As Boolean = True

        For i As Integer = 1 To cMediationDataStructures.MAXFUNCTIONS
            If esdata.PriceMedData.PriceMedFuncNum(Me.m_iGroup, Me.m_iFleet, i) = 0 Then Exit For

            SFPair = m_SFPairs.Item(i - 1) 'Ecosim indexes are one based, m_SFPairs is zero based
            SFPair.FunctionType = eForcingFunctionApplication.OffVesselPrice
            SFPair.Shape = Me.getShapeFromEcosimIndex(man, esdata.PriceMedData.PriceMedFuncNum(Me.m_iGroup, Me.m_iFleet, i))
        Next i

        Return bSucces

    End Function

#End Region

#Region " Public Properties "

    ''' <summary>
    ''' Get the <see cref="cCoreGroupBase.Index">index</see> of the fleet
    ''' for this interaction.
    ''' </summary>
    Public ReadOnly Property FleetIndex() As Integer
        Get
            Return Me.m_iFleet
        End Get
    End Property

    ''' <summary>
    ''' Get the <see cref="cCoreGroupBase.Index">index</see> of the group
    ''' for this interaction.
    ''' </summary>
    Public ReadOnly Property GroupIndex() As Integer
        Get
            Return Me.m_iGroup
        End Get
    End Property

    Public Overrides ReadOnly Property MaxNumShapes As Integer
        Get
            Return Me.m_manager.MaxNShapes
        End Get
    End Property

#End Region

#Region " Editing and Updating "

    ''' <summary>
    ''' Update the underlying Ecosim data with the values in this landings interaction
    ''' </summary>
    Friend Overrides Sub Update()
        Dim ishp As Integer
        Dim esdata As cEcosimDatastructures = m_manager.getEcoSimData

        If LockUpdates Then Return

        Try
            For Each sfPair As cShapeFunctionTypePair In m_SFPairs
                ishp += 1
                If sfPair.Shape IsNot Nothing Then
                    esdata.PriceMedData.PriceMedFuncNum(Me.m_iGroup, Me.m_iFleet, ishp) = sfPair.Shape.Index
                Else
                    esdata.PriceMedData.PriceMedFuncNum(Me.m_iGroup, Me.m_iFleet, ishp) = 0
                End If
            Next
            Me.m_manager.getCore.onChanged(Me)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Update() " & ex.Message)
        End Try

        'End If

    End Sub

#End Region

#Region "ICoreInterface implementation"

    ''' <inheritdocs cref="ICoreInterface.DataType"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Overrides ReadOnly Property DataType() As eDataTypes
        Get
            Return eDataTypes.LandingInteraction
        End Get
    End Property

    ''' <inheritdocs cref="ICoreInterface.GetID"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Overrides Function GetID() As String
        Return cValueID.GetDataTypeID(Me.DataType, CInt(m_iFleet * 1000 + m_iGroup))
    End Function

#End Region

End Class
