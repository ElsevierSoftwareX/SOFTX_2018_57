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

Imports EwEUtils.Core

#End Region ' Imports

Public Class cEcospaceEnviroResponseManager
    Inherits cCoreInputOutputBase

#Region "Private data"

    Private m_maps As List(Of IEnviroInputMap)
    Private m_SpaceData As cEcospaceDataStructures
    Private m_MedData As cMediationDataStructures

#End Region

#Region " Constructor "

    Public Sub New(ByVal core As cCore)
        MyBase.New(core)
        Me.m_coreComponent = eCoreComponentType.MapResponseInteractionManager
        Me.m_dataType = eDataTypes.EcospaceMapResponse
    End Sub

#End Region ' Constructor

#Region " Public Methods and Properties "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Gets the number of maps managed by the manager.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property nMaps() As Integer
        Get
            Return Me.m_maps.Count
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the map at a given index [1, <see cref="nMaps"/>].
    ''' </summary>
    ''' <param name="MapIndex">The one-based index of the map to return.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Map(ByVal MapIndex As Integer) As IEnviroInputMap
        Get
            If MapIndex > 0 And MapIndex <= Me.m_maps.Count Then
                Return Me.m_maps(MapIndex - 1)
            End If
            Return Nothing
        End Get

    End Property

    Public ReadOnly Property Map(ByVal LayerName As String) As IEnviroInputMap
        Get

            For Each envMap As cEnviroInputMap In Me.m_maps
                If String.Compare(envMap.Layer.Name, LayerName, True) = 0 Then
                    Return envMap
                End If
            Next

            Return Nothing
        End Get

    End Property

    Public ReadOnly Property Map(ByVal layer As cEcospaceLayer) As IEnviroInputMap
        Get

            For Each envMap As cEnviroInputMap In Me.m_maps
                If String.Compare(envMap.Layer.getID, layer.getID) = 0 Then
                    Return envMap
                End If
            Next

            Return Nothing
        End Get

    End Property

#End Region ' Public Methods and Properties

#Region " Friend interfaces "

    Friend Sub Init(ByVal spaceData As cEcospaceDataStructures, ByVal MediationData As cMediationDataStructures)
        Me.m_SpaceData = spaceData
        Me.m_MedData = MediationData
        Me.m_maps = New List(Of IEnviroInputMap)
    End Sub

    Friend Function Load() As Boolean

        Dim map As IEnviroInputMap = Nothing
        Dim layer As cEcospaceLayer = Nothing
        Dim bSuccess As Boolean = True

        Me.AllowValidation = False
        Try

            Me.m_maps.Clear()

            ' Hard-code the depth map at position 0
            layer = Me.m_core.EcospaceBasemap.LayerDepth()
            map = New cEnviroInputMap(Me.m_core.CapacityMapInteractionManager, layer)

            ' Bad hack: disable updates from the layer
            map.setManager(Nothing)
            For iGroup As Integer = 1 To Me.m_SpaceData.NGroups
                map.ResponseIndexForGroup(iGroup) = Me.m_SpaceData.CapMapFunctions(0, iGroup)
            Next
            map.setManager(Me.m_core.CapacityMapInteractionManager)
            Me.m_maps.Add(map)

            'populate the list of IEnviroInputMap objects that the user will interact with 
            'to change region related parameters from the interface
            For iMap As Integer = 1 To Me.m_SpaceData.nEnvironmentalDriverLayers
                Try

                    layer = Me.m_core.EcospaceBasemap.LayerDriver(iMap)
                    map = New cEnviroInputMap(Me.m_core.CapacityMapInteractionManager, layer, iMap)
                    ' Bad hack: disable updates from the layer
                    map.setManager(Nothing)
                    For iGroup As Integer = 1 To Me.m_SpaceData.NGroups
                        map.ResponseIndexForGroup(iGroup) = Me.m_SpaceData.CapMapFunctions(iMap, iGroup)
                    Next
                    map.setManager(Me.m_core.CapacityMapInteractionManager)
                    Me.m_maps.Add(map)

                Catch ex As Exception
                    Debug.Assert(False, "InitAndLoadCapacityMaps Error: " & ex.Message)
                    bSuccess = False
                End Try

            Next iMap

            Me.m_SpaceData.CapMaps = Me.m_maps.ToArray

            'update the maps with the newly loaded data
            Me.Update()

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Load() Exception: " & ex.Message)
            bSuccess = False
        End Try
        Me.AllowValidation = True

        Return bSuccess

    End Function

    Friend Function onChanged() As Boolean

        Try

            For iMap As Integer = 1 To Me.m_maps.Count
                For iGroup As Integer = 1 To Me.m_SpaceData.NGroups
                    Me.m_SpaceData.CapMapFunctions(iMap - 1, iGroup) = Me.Map(iMap).ResponseIndexForGroup(iGroup)
                Next
            Next

            If Me.AllowValidation Then
                Me.m_core.onChanged(Me, eMessageType.DataModified)
            End If

        Catch ex As Exception
            Return False
        End Try
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update all the maps in the manager.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub Update()
        Try

            For Each map As IEnviroInputMap In Me.m_maps
                map.Update()
            Next

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Update() Exception: " & ex.Message)
        End Try

    End Sub

    Public Overrides Sub Clear()
        Me.m_maps.Clear()
    End Sub

    Friend ReadOnly Property MediationData() As cMediationDataStructures
        Get
            Return Me.m_MedData
        End Get
    End Property

    Friend ReadOnly Property SpaceData() As cEcospaceDataStructures
        Get
            Return Me.m_core.m_EcoSpaceData
        End Get
    End Property

#End Region ' Friend interfaces

End Class
