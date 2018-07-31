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
Imports EwECore.Core

''' <summary>
''' Results of the current Ecospace time step
''' </summary>
Public Class cEcospaceTimestep
    Implements ICoreInterface
    Implements IEcospaceLayerManager

#Region "Private data"

    Private m_core As cCore
    Private m_dbid As Integer
    Private m_name As String

    Private m_ConMax() As Single

    Private m_biomass() As Single 'biomass by group
    Private m_relativebiomass() As Single 'biomass relative to start biomass by group
    Private m_biomassByRegion(,) As Single 'biomass by group region
    Private m_sumEffortMap(,) As Single 'map of effort over all fleets

    Private m_F() As Single
    Private m_pred() As Single
    Private m_consum() As Single
    Private m_catch() As Single

    Private m_spaceData As cEcospaceDataStructures
    Private m_simData As cEcosimDatastructures
    Private m_stanzaData As cStanzaDatastructures

#End Region

#Region "Private classes"

    ''' <summary>
    ''' Data wrapper for a (row, col) formatted Ecospace result.
    ''' </summary>
    Friend Class cTimestepLayer
        Inherits cEcospaceLayerSingle

        Public Sub New(core As cCore, manager As cEcospaceTimestep, varName As eVarNameFlags)
            MyBase.New(core, manager, "", varName)
        End Sub

    End Class

    ''' <summary>
    ''' Data wrapper for a (row, col, group) formatted Ecospace result.
    ''' </summary>
    Friend Class cTimestepLayerGroup
        Inherits cEcospaceLayerSingle

        Public Sub New(core As cCore, manager As cEcospaceTimestep, varName As eVarNameFlags, iGroup As Integer)
            MyBase.New(core, manager, "", varName)
            Me.m_iGroup = iGroup
        End Sub

        Private m_iGroup As Integer = 0

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the value of a cell.
        ''' </summary>
        ''' <param name="iRow">Row index of the cell to access.</param>
        ''' <param name="iCol">Column index of the cell to access.</param>
        ''' -----------------------------------------------------------------------
        Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object
            Get
                If (TypeOf Me.Data Is Single(,,)) Then
                    Dim d As Single(,,) = DirectCast(Me.Data, Single(,,))
                    Return d(iRow, iCol, Me.m_iGroup)
                ElseIf (TypeOf Me.Data Is Single()(,)) Then
                    Dim d As Single()(,) = DirectCast(Me.Data, Single()(,))
                    Return d(Me.m_iGroup)(iRow, iCol)
                ElseIf (TypeOf Me.Data Is Single(,)) Then
                    Dim d As Single(,) = DirectCast(Me.Data, Single(,))
                    Return d(iRow, iCol)
                End If
                Return cCore.NULL_VALUE
            End Get
            Set(ByVal value As Object)
                ' NOP
            End Set
        End Property

    End Class

    ''' <summary>
    ''' Data wrapper for a (fleet, row, col) formatted Ecospace result.
    ''' </summary>
    Friend Class cTimestepLayerFleet
        Inherits cEcospaceLayerSingle

        Public Sub New(core As cCore, manager As cEcospaceTimestep, varName As eVarNameFlags, iFleet As Integer)
            MyBase.New(core, manager, "", varName)
            Me.m_iFleet = iFleet
        End Sub

        Private m_iFleet As Integer = 0

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the value of a cell.
        ''' </summary>
        ''' <param name="iRow">Row index of the cell to access.</param>
        ''' <param name="iCol">Column index of the cell to access.</param>
        ''' -----------------------------------------------------------------------
        Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional ByVal iIndexSec As Integer = cCore.NULL_VALUE) As Object
            Get
                Dim data As Single(,,) = DirectCast(Me.Data, Single(,,))
                Return data(Me.m_iFleet, iRow, iCol)
            End Get
            Set(ByVal value As Object)
                ' NOP
            End Set
        End Property

    End Class

#End Region

#Region "Constructor & Initialization"

    Public Sub New(theCore As cCore, ByVal EcoSimData As cEcosimDatastructures, ByVal EcoSpaceData As cEcospaceDataStructures, ByVal StanzaData As cStanzaDatastructures)

        m_core = theCore
        m_dbid = cCore.NULL_VALUE
        m_name = eDataTypes.EcospaceTimestepResults.ToString
        Me.m_simData = EcoSimData
        Me.m_spaceData = EcoSpaceData
        Me.m_stanzaData = StanzaData

        Debug.Assert(Me.m_simData IsNot Nothing, Me.ToString & ".New() Ecosim data cannot be null!")
        Debug.Assert(Me.m_spaceData IsNot Nothing, Me.ToString & ".New() Ecospace data cannot be null!")

        Try
            ReDim m_biomass(Me.m_spaceData.NGroups)
            ReDim m_relativebiomass(Me.m_spaceData.NGroups)
            ReDim m_ConMax(Me.m_spaceData.NGroups)
            ReDim m_biomassByRegion(Me.m_spaceData.NGroups, Me.m_spaceData.nRegions)
            ReDim m_sumEffortMap(Me.m_spaceData.InRow, Me.m_spaceData.InCol)


            ReDim m_F(Me.m_spaceData.NGroups)
            ReDim m_pred(Me.m_spaceData.NGroups)
            ReDim m_consum(Me.m_spaceData.NGroups)
            ReDim m_catch(Me.m_spaceData.NGroups)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".New() Error: " & ex.Message)
        End Try

    End Sub

    Friend Sub ComputeSumEffortMap()

        Array.Clear(m_sumEffortMap, 0, m_sumEffortMap.Length)
        'ReDim m_sumEffortMap(Me.m_spaceData.InRow, Me.m_spaceData.InCol)

        For iRow As Integer = 1 To Me.m_spaceData.InRow
            For iCol As Integer = 1 To Me.m_spaceData.InCol
                Dim sTotal As Single = 0
                If Me.m_spaceData.Depth(iRow, iCol) > 0 Then
                    For iFleet As Integer = 1 To Me.m_simData.nGear
                        sTotal += Me.FishingEffortMap(iFleet, iRow, iCol) * Me.m_simData.EffortConversionFactor(iFleet)
                    Next iFleet
                End If
                Me.m_sumEffortMap(iRow, iCol) = sTotal
            Next iCol
        Next iRow

    End Sub

#End Region

#Region "Public Properties"

    ''' <summary>
    ''' Cumulative timestep counter for the current results. 
    ''' </summary>
    ''' <remarks>
    ''' This is the number of timesteps computed. It is not necessarily the number of months completed. 
    ''' See <see cref="TimeStepinYears">TimeStepinYears</see> for the length of the run.
    ''' </remarks>
    Public Property iTimeStep() As Integer

    ''' <summary>
    ''' Lenght of the run in Years
    ''' </summary>
    Public Property TimeStepinYears() As Single

    ''' <summary>
    ''' Biomass map dimensioned by Row, Col, Group
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>BiomassMap(row,col,group) and FishingEffortMap(fleet,row,col) are both map variables but they are not indexed the same</remarks>
    Public ReadOnly Property BiomassMap() As Single(,,)
        Get
            Return Me.m_spaceData.Bcell
        End Get
    End Property

    ''' <summary>
    ''' Catch map dimensioned by Row, Col, Group
    ''' </summary>
    Public ReadOnly Property CatchMap() As Single(,,)
        Get
            Return Me.m_spaceData.CatchMap
        End Get
    End Property

    ''' <summary>
    ''' Catch map dimensioned by Row, Col, Fleet
    ''' </summary>
    Public ReadOnly Property CatchFleetMap() As Single(,,)
        Get
            Return Me.m_spaceData.CatchFleetMap
        End Get
    End Property

    ''' <summary>
    ''' Fishing Effort dimensioned by Fleet, Row, Col
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>BiomassMap(row,col,group) and FishingEffortMap(fleet,row,col) are both map variables but they are not indexed the same</remarks>
    Public ReadOnly Property FishingEffortMap() As Single(,,)
        Get
            Return Me.m_spaceData.EffortSpace
        End Get
    End Property

    ''' <summary>
    ''' Contaminant concentrations dimensioned by Row, Col and Group
    ''' </summary>
    ''' <value></value>
    ''' <returns>Matrix of contaminant concentrations at this timestep</returns>
    Public ReadOnly Property ContaminantMap() As Single(,,)
        Get
            Return Me.m_spaceData.Ccell
        End Get
    End Property

    ''' <summary>
    ''' Discard mortality dimensioned by Row, Col and Group
    ''' </summary>
    ''' <value></value>
    ''' <returns>Matrix of discard mortality at this timestep</returns>
    Public ReadOnly Property DiscardMortalityMap() As Single(,,)
        Get
            Return Me.m_spaceData.DiscardsMap
        End Get
    End Property

    ''' <summary>
    ''' KemptonsQ map dimensioned by Row, Col
    ''' </summary>
    Public ReadOnly Property KemptonsQMap() As Single(,)
        Get
            Return Me.m_spaceData.KemptonsQ
        End Get
    End Property

    ''' <summary>
    ''' Shannon diversity map dimensioned by Row, Col
    ''' </summary>
    Public ReadOnly Property ShannonDiversityMap() As Single(,)
        Get
            Return Me.m_spaceData.ShannonDiversity
        End Get
    End Property

    Public Property FishingMort(ByVal iGroup As Single) As Single

        Get
            Try
                Return m_F(iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

        Friend Set(ByVal value As Single)
            Try
                m_F(iGroup) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set

    End Property

    Public Property PredMortRate(ByVal iGroup As Single) As Single

        Get
            Try
                Return m_pred(iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

        Friend Set(ByVal value As Single)
            Try
                m_pred(iGroup) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set

    End Property


    Public Property [Catch](ByVal iGroup As Single) As Single

        Get
            Try
                Return m_catch(iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

        Friend Set(ByVal value As Single)
            Try
                m_catch(iGroup) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set

    End Property

    Public Property ConsumptRate(ByVal iGroup As Single) As Single

        Get
            Try
                Return m_consum(iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

        Friend Set(ByVal value As Single)
            Try
                m_consum(iGroup) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set

    End Property

    ''' <summary>
    ''' Average Biomass by group
    ''' </summary>
    ''' <param name="iGroup">Group index</param>
    Public Property Biomass(ByVal iGroup As Single) As Single

        Get
            Try
                Return m_biomass(iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

        Friend Set(ByVal value As Single)
            Try
                m_biomass(iGroup) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set

    End Property

    ''' <summary>
    ''' Max concentration of contaminant at the current time step by group
    ''' </summary>
    Public Property ConcMax(ByVal iGroup As Single) As Single

        Get
            Try
                Return Me.m_ConMax(iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

        Friend Set(ByVal value As Single)
            Try
                Me.m_ConMax(iGroup) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set

    End Property


    ''' <summary>
    ''' Average Biomass relative to the base by group (Bt/B0)
    ''' </summary>
    ''' <param name="iGroup">Group index</param>
    Public Property RelativeBiomass(ByVal iGroup As Single) As Single

        Get
            Try
                Return m_relativebiomass(iGroup)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

        Friend Set(ByVal value As Single)
            Try
                m_relativebiomass(iGroup) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set

    End Property


    ''' <summary>
    ''' Number of rows in the map
    ''' </summary>
    Public ReadOnly Property inRows() As Integer
        Get
            Return Me.m_spaceData.InRow
        End Get
    End Property


    ''' <summary>
    ''' Number of columns in the map
    ''' </summary>
    Public ReadOnly Property inCols() As Integer
        Get
            Return Me.m_spaceData.InCol
        End Get
    End Property

    ''' <summary>
    ''' Average Biomass by group, region
    ''' </summary>
    ''' <param name="iGroup">Group index</param>
    Public Property BiomassByRegion(ByVal iGroup As Integer, ByVal iRegion As Integer) As Single

        Get
            Try
                Return m_biomassByRegion(iGroup, iRegion)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

        Friend Set(ByVal value As Single)
            Try
                m_biomassByRegion(iGroup, iRegion) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Set

    End Property

    ''' <summary>
    ''' Number of Prey/Pred linkages
    ''' </summary>
    ''' <remarks>Number of links is set in cEcoSimModel.CalcEatenOfBy()</remarks>
    Public ReadOnly Property nPreyPredLinks() As Integer
        Get
            Return Me.m_simData.inlinks
        End Get
    End Property

    ''' <summary>
    ''' Gets the group index for the Prey of this Prey/Pred link
    ''' </summary>
    ''' <param name="iPreyPredIndex">Index of the Prey/Pred link (1 to nPreyPredLinks)</param>
    ''' <remarks> </remarks>
    Public ReadOnly Property iPreyIndex(ByVal iPreyPredIndex As Integer) As Integer

        Get
            Debug.Assert(iPreyPredIndex <= Me.m_simData.inlinks, Me.ToString & ".iPreyIndex(iPreyPredIndex) iPreyPredIndex out of bounds!")
            Try
                Return Me.m_simData.ilink(iPreyPredIndex)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

    End Property

    ''' <summary>
    ''' Gets the group index for the Predator of this Prey/Pred link
    ''' </summary>
    ''' <param name="iPreyPredIndex">Index of the Prey/Pred link (1 to nPreyPredLinks)</param>
    ''' <remarks> </remarks>
    Public ReadOnly Property iPredIndex(ByVal iPreyPredIndex As Integer) As Integer

        Get
            Debug.Assert(iPreyPredIndex <= Me.m_simData.inlinks, Me.ToString & ".iPredIndex(iPreyPredIndex) iPreyPredIndex out of bounds!")
            Try
                Return Me.m_simData.jlink(iPreyPredIndex)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Get

    End Property

    ''' <summary>
    ''' Mortality rate map due to predation by Row, Col, Prey/Pred linkage <see cref="nPreyPredLinks">nPreyPredLinks</see>
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>MortPredRate = [prey biomass eaten] / [prey biomass]</remarks>
    Public ReadOnly Property MortPredRate() As Single(,,)
        Get
            Return Me.m_spaceData.MPred
        End Get
    End Property

    ''' <summary>
    ''' Detritus by group
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Total biomass a group contributes to detritus</remarks>
    Public ReadOnly Property GroupDetritus() As Single(,,)
        Get
            Return Me.m_spaceData.GroupDetritus
        End Get
    End Property

    Public ReadOnly Property StanzaDS() As cStanzaDatastructures
        Get
            Return Me.m_stanzaData
        End Get
    End Property

#End Region

#Region " ICoreInterface implementation "

    Public ReadOnly Property DataType() As eDataTypes Implements ICoreInterface.DataType
        Get
            Return eDataTypes.EcospaceTimestepResults
        End Get
    End Property

    Public ReadOnly Property CoreComponent() As eCoreComponentType Implements ICoreInterface.CoreComponent
        Get
            Return eCoreComponentType.EcoSpace
        End Get
    End Property

    Public Property DBID() As Integer Implements ICoreInterface.DBID
        Get
            Return m_dbid
        End Get
        Set(ByVal value As Integer)
            m_dbid = value
        End Set
    End Property

    Public Function GetID() As String Implements ICoreInterface.GetID
        Return cValueID.GetDataTypeID(Me.DataType, Me.DBID)
    End Function

    Public Property Index() As Integer Implements ICoreInterface.Index
        Get
            Return cCore.NULL_VALUE
        End Get
        Set(ByVal value As Integer)

        End Set
    End Property

    Public Property Name() As String Implements ICoreInterface.Name
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            m_name = value
        End Set
    End Property

#End Region

#Region " IEcospaceLayerManager implementation "

    ''' <inheritdocs cref="IEcospaceLayerManager.Layer"/>
    Public Function Layer(varName As eVarNameFlags, Optional iIndex As Integer = -9999) As cEcospaceLayer Implements Core.IEcospaceLayerManager.Layer
        Dim lLayers As cEcospaceLayer() = Me.Layers(varName)
        If (lLayers.Length = 0) Then Return Nothing
        If (iIndex < 0) Then
            Return lLayers(0)
        Else
            If (iIndex < lLayers.Length) Then
                Return lLayers(iIndex)
            End If
        End If
        Return Nothing
    End Function

    ''' <inheritdocs cref="IEcospaceLayerManager.Layers"/>
    Public Function Layers(Optional varName As eVarNameFlags = eVarNameFlags.NotSet) As cEcospaceLayer() Implements Core.IEcospaceLayerManager.Layers

        Dim lLayers As New List(Of cEcospaceLayer)
        Select Case varName
            Case eVarNameFlags.EcospaceMapBiomass,
                 eVarNameFlags.EcospaceMapCatch,
                 eVarNameFlags.LayerHabitatCapacity,
                 eVarNameFlags.EcospaceMapDiscards,
                 eVarNameFlags.LayerHabitatCapacity
                lLayers.Add(Nothing) ' Add 0-item emptyness
                For igroup As Integer = 1 To Me.m_core.nGroups
                    lLayers.Add(New cTimestepLayerGroup(Me.m_core, Me, varName, igroup))
                Next

            Case eVarNameFlags.Concentration
                For igroup As Integer = 0 To Me.m_core.nGroups
                    lLayers.Add(New cTimestepLayerGroup(Me.m_core, Me, varName, igroup))
                Next

            Case eVarNameFlags.EcospaceMapSumEffort
                lLayers.Add(New cTimestepLayer(Me.m_core, Me, varName))

            Case eVarNameFlags.EcospaceMapEffort
                lLayers.Add(Nothing) ' Add 0-item emptyness
                For iFleet As Integer = 1 To Me.m_core.nFleets
                    lLayers.Add(New cTimestepLayerFleet(Me.m_core, Me, varName, iFleet))
                Next

        End Select
        Return lLayers.ToArray

    End Function

    ''' <inheritdocs cref="IEcospaceLayerManager.LayerData"/>
    Public Function LayerData(varName As EwEUtils.Core.eVarNameFlags, iIndex As Integer) As Object _
        Implements Core.IEcospaceLayerManager.LayerData

        Select Case varName

            Case eVarNameFlags.LayerHabitatCapacity
                Return Me.m_spaceData.HabCap
            Case eVarNameFlags.EcospaceMapBiomass
                Return Me.BiomassMap
            Case eVarNameFlags.EcospaceMapCatch
                Return Me.CatchMap
            Case eVarNameFlags.EcospaceMapEffort
                Return Me.FishingEffortMap
            Case eVarNameFlags.EcospaceMapSumEffort
                Return Me.m_sumEffortMap

            Case eVarNameFlags.EcospaceMapDiscards
                Return Me.DiscardMortalityMap
            Case eVarNameFlags.Concentration
                Return Me.ContaminantMap
            Case eVarNameFlags.EcospaceMapShannonDiversity
                Return Me.ShannonDiversityMap
            Case eVarNameFlags.EcospaceMapKemptonsQ
                Return Me.KemptonsQMap
        End Select
        Return Nothing

    End Function

#End Region

End Class


