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

Imports System.IO
Imports System.Text
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

#Region "Must inherit Base class"

''' <summary>
''' Base class for data source objects used by the <see cref="cEcospaceRegionAvgResultsWriter">cEcospaceAvgModelAreaResultsWriter</see>
''' to write averaged Ecospace results to a csv file. 
''' </summary>
''' <remarks></remarks>
Public MustInherit Class cEcospaceResultsWriterDataSourceBase
    Protected m_core As cCore
    Protected m_spaceData As cEcospaceDataStructures

    Sub New(Core As cCore, EcospaceData As cEcospaceDataStructures)
        Me.m_core = Core
        Me.m_spaceData = EcospaceData
    End Sub

    ''' <summary>
    ''' Number of results in the data source. This can be ngroups, nfleets, ngroups * nfleets depending on the data.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    MustOverride ReadOnly Property nResults As Integer

    ''' <summary>
    ''' File identifier use to build the file name
    ''' </summary>
    MustOverride ReadOnly Property FilenameIdentifier As String

    ''' <summary>
    ''' Description of the data used in the header of the file
    ''' </summary>
    MustOverride ReadOnly Property DataDescriptor As String

    ''' <summary>
    ''' Description of the area that is covered by the data. This can be the total area or a region
    ''' </summary>
    MustOverride ReadOnly Property AreaDescriptor As String

    ''' <summary>
    ''' Number of water cells in the area
    ''' </summary>
    ''' <value></value>
    MustOverride ReadOnly Property nWaterCells As Integer

    ''' <summary>
    ''' Init the data source
    ''' </summary>
    MustOverride Sub Init(Optional ByVal OptionalIndex As Integer = 0)

    ''' <summary>
    ''' Return the result for this index and time step
    ''' </summary>
    ''' <param name="OneBasedIndex">One based index of the result to return</param>
    ''' <param name="TimeIndex">One based time step of the result</param>
    MustOverride Function getResult(OneBasedIndex As Integer, TimeIndex As Integer) As Single

    ''' <summary>
    ''' Name of the result field. This can be a group name, fleet name, or a combo of both
    ''' </summary>
    MustOverride Function FieldName(OneBasedIndex As Integer) As String


    ''' <summary>
    ''' Four character abbreviation of Variable and Area
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    MustOverride Function FileNameAbbreviation() As String

    ''' <summary>
    ''' Index of the Region for this datasource
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    MustOverride ReadOnly Property AreaIndex As Integer

End Class

#End Region

#Region "Total modeled area"

#Region "Biomass over model area"

''' <summary>
''' Implementation of <see cref="cEcospaceResultsWriterDataSourceBase">cResultsDataSourceBase</see> for biomass averaged over the total modeled area.
''' </summary>
Public Class cBiomassResultsDataSource
    Inherits cEcospaceResultsWriterDataSourceBase

    Sub New(Core As cCore, EcospaceData As cEcospaceDataStructures)
        MyBase.New(Core, EcospaceData)
    End Sub

    Public Overrides Function getResult(OneBasedIndex As Integer, TimeIndex As Integer) As Single
        Return Me.m_spaceData.ResultsByGroup(EwECore.eSpaceResultsGroups.Biomass, OneBasedIndex, TimeIndex)
    End Function

    Public Overrides Sub Init(Optional ByVal OptionalIndex As Integer = 0)

    End Sub

    Public Overrides ReadOnly Property nResults As Integer
        Get
            Return Me.m_core.nGroups
        End Get
    End Property

    Public Overrides Function FieldName(OneBasedIndex As Integer) As String
        Return Me.m_core.m_EcoPathData.GroupName(OneBasedIndex)
    End Function

    Public Overrides ReadOnly Property FilenameIdentifier As String
        Get
            Return "Biomass"
        End Get
    End Property

    Public Overrides ReadOnly Property DataDescriptor As String
        Get
            Return "Average biomass across modeled area"
        End Get
    End Property

    Public Overrides ReadOnly Property AreaDescriptor As String
        Get
            Return "Modeled area km2"
        End Get
    End Property

    Public Overrides ReadOnly Property nWaterCells As Integer
        Get
            Return Me.m_spaceData.nWaterCells
        End Get
    End Property

    Public Overrides Function FileNameAbbreviation() As String
        'Biomass total model area
        Return "BMFL"
    End Function

    Public Overrides ReadOnly Property AreaIndex As Integer
        Get
            Return 0
        End Get
    End Property
End Class

#End Region

#Region "Catch over modeled area"

''' <summary>
''' Implementation of <see cref="cEcospaceResultsWriterDataSourceBase">cResultsDataSourceBase</see> for catch averaged over the total modeled area.
''' </summary>
''' <remarks></remarks>
Public Class cCatchResultsDataSource
    Inherits cEcospaceResultsWriterDataSourceBase

    ''' <summary>
    ''' Local helper class for remembering bits of a landing record.
    ''' </summary>
    Private Class cCatch

        Public Sub New(f As cEcopathFleetInput, g As cCoreGroupBase)
            Me.FleetName = f.Name
            Me.FleetIndex = f.Index
            Me.GroupName = g.Name
            Me.GroupIndex = g.Index
        End Sub

        Public Property FleetName As String
        Public Property FleetIndex As Integer
        Public Property GroupName As String
        Public Property GroupIndex As Integer

    End Class

    Private m_lstCatch As List(Of cCatch)

    Sub New(Core As cCore, EcospaceData As cEcospaceDataStructures)
        MyBase.New(Core, EcospaceData)
    End Sub

    Public Overrides Function getResult(OneBasedIndex As Integer, TimeIndex As Integer) As Single
        Try
            Dim catchOb As cCatch = Me.m_lstCatch.Item(OneBasedIndex - 1)
            Return Me.m_spaceData.ResultsByFleetGroup(eSpaceResultsFleetsGroups.CatchBio, catchOb.FleetIndex, catchOb.GroupIndex, TimeIndex)
        Catch ex As Exception
            Debug.Assert(False, "Exception writting Ecospace results. " + ex.Message)
        End Try

        Return 0.0

    End Function


    Public Overrides Sub Init(Optional ByVal OptionalIndex As Integer = 0)

        m_lstCatch = New List(Of cCatch)
        Dim fleet As cEcopathFleetInput = Nothing
        Dim group As cCoreGroupBase = Nothing

        For iFleet As Integer = 1 To Me.m_core.nFleets
            fleet = Me.m_core.EcopathFleetInputs(iFleet)
            For iGroup As Integer = 1 To Me.m_core.nGroups
                group = Me.m_core.EcoPathGroupInputs(iGroup)
                If (fleet.Landings(iGroup) + fleet.Discards(iGroup)) > 0 Then
                    'Save the Fleet and group indexes
                    m_lstCatch.Add(New cCatch(fleet, group))
                End If
            Next iGroup
        Next iFleet

    End Sub

    Public Overrides ReadOnly Property nResults As Integer
        Get
            Return Me.m_lstCatch.Count
        End Get
    End Property

    Public Overrides Function FieldName(OneBasedIndex As Integer) As String

        Try
            Dim catchOb As cCatch = Me.m_lstCatch.Item(OneBasedIndex - 1)
            Return catchOb.FleetName + " | " + catchOb.GroupName
        Catch ex As Exception
            Debug.Assert(False, "Exception writting Ecospace results. " + ex.Message)
        End Try

        Return ""
    End Function

    Public Overrides ReadOnly Property FilenameIdentifier As String
        Get
            Return "Catch"
        End Get
    End Property


    Public Overrides ReadOnly Property DataDescriptor As String
        Get
            Return "Catch by Fleet of a Group"
        End Get
    End Property

    Public Overrides ReadOnly Property nWaterCells As Integer
        Get
            Return Me.m_core.m_EcoSpaceData.nWaterCells
        End Get
    End Property

    Public Overrides ReadOnly Property AreaDescriptor As String
        Get
            Return "Modeled area km2"
        End Get
    End Property

    Public Overrides Function FileNameAbbreviation() As String
        'Catch total model area
        Return "CTFL"
    End Function

    Public Overrides ReadOnly Property AreaIndex As Integer
        Get
            Return 0
        End Get
    End Property
End Class

#End Region

#End Region

#Region "By Region"

#Region "Biomass by region"

''' <summary>
''' Implementation of <see cref="cEcospaceResultsWriterDataSourceBase">cResultsDataSourceBase</see> for averaged biomass by region.
''' </summary>
''' <remarks></remarks>
Public Class cRegionBiomassResultsDataSource
    Inherits cEcospaceResultsWriterDataSourceBase

    ''' <summary>
    ''' Local helper class for remembering bits of a landing record.
    ''' </summary>
    Private Class cRegion

        Public Sub New(g As cCoreGroupBase, RegionIndex As Integer)
            Me.GroupName = g.Name
            Me.GroupIndex = g.Index
            Me.iRegionIndex = RegionIndex
        End Sub

        Public Property GroupName As String
        Public Property GroupIndex As Integer
        Public Property iRegionIndex As Integer

    End Class

    Private m_lstRegions As List(Of cRegion)
    Private m_iRegionIndex As Integer

    Sub New(Core As cCore, EcospaceData As cEcospaceDataStructures)
        MyBase.New(Core, EcospaceData)
    End Sub

    Public Overrides Function getResult(OneBasedIndex As Integer, TimeIndex As Integer) As Single
        Try
            Dim RegionOb As cRegion = Me.m_lstRegions.Item(OneBasedIndex - 1)
            Return Me.m_spaceData.ResultsRegionGroup(RegionOb.iRegionIndex, RegionOb.GroupIndex, TimeIndex)
        Catch ex As Exception
            Debug.Assert(False, "Exception writting Ecospace results. " + ex.Message)
        End Try

        Return 0.0

    End Function


    Public Overrides Sub Init(Optional ByVal OptionalIndex As Integer = 0)

        m_iRegionIndex = OptionalIndex
        m_lstRegions = New List(Of cRegion)
        For iGroup As Integer = 1 To Me.m_core.nGroups
            m_lstRegions.Add(New cRegion(Me.m_core.EcoPathGroupInputs(iGroup), OptionalIndex))
        Next iGroup

    End Sub

    Public Overrides ReadOnly Property nResults As Integer
        Get
            Return Me.m_lstRegions.Count
        End Get
    End Property

    Public Overrides Function FieldName(OneBasedIndex As Integer) As String

        Try
            Return Me.m_lstRegions.Item(OneBasedIndex - 1).GroupName
        Catch ex As Exception
            Debug.Assert(False, "Exception writting Ecospace results. " + ex.Message)
        End Try

        Return ""
    End Function

    Public Overrides ReadOnly Property FilenameIdentifier As String
        Get
            Return "Region_" + Me.m_iRegionIndex.ToString + "_Biomass"
        End Get
    End Property

    Public Overrides ReadOnly Property AreaDescriptor As String
        Get
            Return "Region " + Me.m_iRegionIndex.ToString + " area km2"
        End Get
    End Property

    Public Overrides ReadOnly Property DataDescriptor As String
        Get
            Return "Average biomass by region"
        End Get
    End Property

    Public Overrides ReadOnly Property nWaterCells As Integer
        Get
            Return Me.m_core.m_EcoSpaceData.nCellsInRegion(Me.m_iRegionIndex)
        End Get
    End Property

    Public Overrides Function FileNameAbbreviation() As String
        'Biomass for region
        Dim ReturnStr As String
        Dim RegStr As String = Me.m_iRegionIndex.ToString
        If RegStr.Length = 1 Then
            RegStr = "0" + RegStr
        End If
        ReturnStr = "BR" + RegStr
        Debug.Assert(ReturnStr.Length = 4, "WOW " + Me.ToString + ".FileNameAbbreviation() not the correct length for ICM abbreviation.")
        Return ReturnStr
    End Function

    Public Overrides ReadOnly Property AreaIndex As Integer
        Get
            Return Me.m_iRegionIndex
        End Get
    End Property
End Class

#End Region

#Region "Catch by region"


''' <summary>
''' Implementation of <see cref="cEcospaceResultsWriterDataSourceBase">cResultsDataSourceBase</see> for averaged catch by region.
''' </summary>
''' <remarks></remarks>
Public Class cRegionCatchResultsDataSource
    Inherits cEcospaceResultsWriterDataSourceBase

    ''' <summary>
    ''' Local helper class for remembering region and group/fleet info
    ''' </summary>
    Private Class cRegion

        Public Sub New(f As cEcopathFleetInput, g As cCoreGroupBase, RegionIndex As Integer)
            Me.FleetName = f.Name
            Me.FleetIndex = f.Index
            Me.GroupName = g.Name
            Me.GroupIndex = g.Index
            Me.RegionIndex = RegionIndex
        End Sub

        Public Property GroupName As String
        Public Property GroupIndex As Integer
        Public Property FleetName As String
        Public Property FleetIndex As Integer
        Public Property RegionIndex As Integer

    End Class

    Private m_lstRegions As List(Of cRegion)
    Private m_iRegionIndex As Integer

    Sub New(Core As cCore, EcospaceData As cEcospaceDataStructures)
        MyBase.New(Core, EcospaceData)
    End Sub

    Public Overrides Function getResult(OneBasedIndex As Integer, TimeIndex As Integer) As Single
        Try
            Dim RegionOb As cRegion = Me.m_lstRegions.Item(OneBasedIndex - 1)
            Return Me.m_spaceData.ResultsCatchRegionGearGroup(RegionOb.RegionIndex, RegionOb.FleetIndex, RegionOb.GroupIndex, TimeIndex)
        Catch ex As Exception
            Debug.Assert(False, "Exception writting Ecospace results. " + ex.Message)
        End Try

        Return 0.0

    End Function


    Public Overrides Sub Init(Optional ByVal OptionalIndex As Integer = 0)

        m_iRegionIndex = OptionalIndex
        m_lstRegions = New List(Of cRegion)

        Dim fleet As cEcopathFleetInput = Nothing
        Dim group As cCoreGroupBase = Nothing

        For iFleet As Integer = 1 To Me.m_core.nFleets
            fleet = Me.m_core.EcopathFleetInputs(iFleet)
            For iGroup As Integer = 1 To Me.m_core.nGroups
                group = Me.m_core.EcoPathGroupInputs(iGroup)
                If (fleet.Landings(iGroup) + fleet.Discards(iGroup)) > 0 Then
                    'Save the Fleet and group indexes
                    m_lstRegions.Add(New cRegion(fleet, group, m_iRegionIndex))
                End If
            Next iGroup
        Next iFleet

    End Sub

    Public Overrides ReadOnly Property nResults As Integer
        Get
            Return Me.m_lstRegions.Count
        End Get
    End Property

    Public Overrides Function FieldName(OneBasedIndex As Integer) As String

        Try
            Dim region As cRegion = Me.m_lstRegions.Item(OneBasedIndex - 1)
            Return region.FleetName + " | " + region.GroupName
        Catch ex As Exception
            Debug.Assert(False, "Exception writting Ecospace results. " + ex.Message)
        End Try

        Return ""
    End Function

    Public Overrides ReadOnly Property FilenameIdentifier As String
        Get
            Return "Region_" + Me.m_iRegionIndex.ToString + "_Catch"
        End Get
    End Property

    Public Overrides ReadOnly Property AreaDescriptor As String
        Get
            Return "Region " + Me.m_iRegionIndex.ToString + " area km2"
        End Get
    End Property

    Public Overrides ReadOnly Property DataDescriptor As String
        Get
            Return "Average catch in region by fleet and group"
        End Get
    End Property

    Public Overrides ReadOnly Property nWaterCells As Integer
        Get
            Return Me.m_core.m_EcoSpaceData.nCellsInRegion(Me.m_iRegionIndex)
        End Get
    End Property

    Public Overrides Function FileNameAbbreviation() As String
        'Biomass for region
        Dim ReturnStr As String
        Dim RegStr As String = Me.m_iRegionIndex.ToString
        If RegStr.Length = 1 Then
            RegStr = "0" + RegStr
        End If
        ReturnStr = "CR" + RegStr
        Debug.Assert(ReturnStr.Length = 4, "WOW " + Me.ToString + ".FileNameAbbreviation() not the correct length for ICM abbreviation.")
        Return ReturnStr
    End Function


    Public Overrides ReadOnly Property AreaIndex As Integer
        Get
            Return Me.m_iRegionIndex
        End Get
    End Property

End Class

#End Region

#End Region




