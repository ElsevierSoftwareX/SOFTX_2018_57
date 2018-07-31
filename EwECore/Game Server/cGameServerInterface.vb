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

Imports EwEUtils.Core

''' <summary>
''' Core data collector for the OceanViz game server.
''' </summary>
Public Class cGameServerInterface

    Private m_core As cCore
    ' Private m_dctDataTypes As Dictionary(Of EwEUtils.Core.eDataTypes, Object)
    Private m_dctCoreListData As Dictionary(Of EwEUtils.Core.eDataTypes, cCoreInputOutputList(Of EwECore.cCoreInputOutputBase))

    ' JS 14Jan01: Core data objects no longer cached due to new core cleanup dynamics
    '             More vigilant cleanup code requires these objects to be created only when needed
    Private m_dctCoreData As Dictionary(Of EwEUtils.Core.eDataTypes, GetCoreIOObjectDelegate)

    Public Sub New(ByRef theCore As cCore)
        m_core = theCore
    End Sub

    Public Delegate Function GetCoreIOObjectDelegate() As cCoreInputOutputBase

    Friend Sub Init()

        m_dctCoreListData = New Dictionary(Of EwEUtils.Core.eDataTypes, cCoreInputOutputList(Of EwECore.cCoreInputOutputBase))
        m_dctCoreData = New Dictionary(Of EwEUtils.Core.eDataTypes, GetCoreIOObjectDelegate)
        'ecopath
        m_dctCoreListData.Add(eDataTypes.EcoPathGroupInput, m_core.m_EcoPathInputs)
        m_dctCoreListData.Add(eDataTypes.EcoPathGroupOutput, m_core.m_EcoPathOutputs)

        m_dctCoreListData.Add(eDataTypes.FleetInput, m_core.m_EcopathFleetsInput)

        'ecosim
        m_dctCoreListData.Add(eDataTypes.EcoSimGroupOutput, m_core.m_EcoSimGroupOutputs)
        m_dctCoreListData.Add(eDataTypes.EcosimFleetOutput, m_core.m_EcosimFleetOutputs)
        m_dctCoreListData.Add(eDataTypes.EcoSimScenario, m_core.m_EcoSimScenarios)
        m_dctCoreListData.Add(eDataTypes.MSEFleetInput, m_core.MSEManager.EcopathFleetInputs)
        m_dctCoreListData.Add(eDataTypes.EcoSimGroupInput, m_core.m_EcoSimGroups)

        'EcoSpace
        m_dctCoreListData.Add(eDataTypes.EcospaceRegionResults, m_core.m_EcospaceRegionSummaries)
        m_dctCoreListData.Add(eDataTypes.EcospaceGroupOuput, m_core.m_EcospaceGroupOuputs)
        m_dctCoreListData.Add(eDataTypes.EcospaceFleetOuput, m_core.m_EcospaceFleetOutputs)
        m_dctCoreListData.Add(eDataTypes.EcospaceMPA, m_core.m_EcospaceMPAs)
        m_dctCoreListData.Add(eDataTypes.EcospaceHabitat, m_core.m_EcospaceHabitats)

        'MSE 
        m_dctCoreListData.Add(eDataTypes.MSEGroupOutputs, Me.m_core.MSEManager.GroupOutputs)
        m_dctCoreListData.Add(eDataTypes.MSEBiomassStats, Me.m_core.MSEManager.BiomassStats)

        m_dctCoreListData.Add(eDataTypes.MSEGroupInput, Me.m_core.MSEManager.GroupInputs)

        m_dctCoreData.Add(eDataTypes.MSEOutput, New GetCoreIOObjectDelegate(AddressOf Me.m_core.MSEManager.Output))
        m_dctCoreData.Add(eDataTypes.EcosimOutput, New GetCoreIOObjectDelegate(AddressOf Me.m_core.EcosimOutputs))

    End Sub

    Public ReadOnly Property CoreDataList(ByVal DataType As EwEUtils.Core.eDataTypes) As cCoreInputOutputList(Of EwECore.cCoreInputOutputBase)
        Get
            Dim data As cCoreInputOutputList(Of EwECore.cCoreInputOutputBase)
            If Me.m_dctCoreListData.ContainsKey(DataType) Then
                data = m_dctCoreListData.Item(DataType)
            End If
            Return data
        End Get
    End Property

    Public ReadOnly Property CoreData(ByVal DataType As EwEUtils.Core.eDataTypes) As EwECore.cCoreInputOutputBase
        Get
            Dim data As EwECore.cCoreInputOutputBase
            If Me.m_dctCoreData.ContainsKey(DataType) Then
                data = m_dctCoreData.Item(DataType).Invoke
            End If
            Debug.Assert(data IsNot Nothing, Me.ToString & ".CoreData( " & DataType.ToString & " ) not found in core data!")
            Return data
        End Get
    End Property

    Public ReadOnly Property CoreData(ByVal DataType As EwEUtils.Core.eDataTypes, ByVal Index As Integer) As EwECore.cCoreInputOutputBase
        Get
            Dim data As EwECore.cCoreInputOutputBase
            If Me.m_dctCoreListData.ContainsKey(DataType) Then
                data = m_dctCoreListData.Item(DataType).Item(Index)
            End If
            Debug.Assert(data IsNot Nothing, Me.ToString & ".CoreData( " & DataType.ToString & ", " & Index.ToString & " ) not found in core data!")
            Return data
        End Get
    End Property

    Public Function ContainKey(ByVal DataType As EwEUtils.Core.eDataTypes) As Boolean

        If Me.m_dctCoreListData.ContainsKey(DataType) Or Me.m_dctCoreData.ContainsKey(DataType) Then
            Return True
        End If
        Return False

    End Function

    ''' <summary>
    ''' Clear all game server data structures.
    ''' </summary>
    Public Sub Clear()

        Try
            ' Server may not have been initialized!
            If (Me.m_dctCoreListData IsNot Nothing) Then
                Me.m_dctCoreListData.Clear()
            End If
            If (Me.m_dctCoreData IsNot Nothing) Then
                Me.m_dctCoreData.Clear()
            End If
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Clear() Exception: " & ex.Message)
        End Try

    End Sub

End Class
