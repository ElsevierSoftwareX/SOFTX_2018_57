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
Imports EwECore.Core
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' The heart of the Ecospace map interfaces. The basemap manages all foundation
''' Ecospace map layers.
''' </summary>
''' ===========================================================================
Public Class cEcospaceBasemap
    Inherits cCoreInputOutputBase
    Implements IEcospaceLayerManager

    ''' <summary>The layers maintained in a basemap.</summary>
    Private m_dictLayers As New Dictionary(Of eVarNameFlags, cEcospaceLayer())

#Region " Constructor "

    Sub New(ByRef theCore As cCore)

        MyBase.New(theCore)

        Dim data As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
        Dim val As cValue = Nothing

        Me.AllowValidation = False

        Try
            Me.DBID = DBID
            m_dataType = eDataTypes.EcospaceBasemap
            m_coreComponent = eCoreComponentType.EcoSpace

            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            ' InRow
            val = New cValue(0, eVarNameFlags.InRow, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            ' InCol
            val = New cValue(0, eVarNameFlags.InCol, eStatusFlags.Null, eValueTypes.Int)
            m_values.Add(val.varName, val)

            ' CellLength
            val = New cValue(1, eVarNameFlags.CellLength, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' CellSize
            val = New cValue(1, eVarNameFlags.CellSize, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' Latitude (top-left coord of layer)
            val = New cValue(0, eVarNameFlags.Latitude, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' Longitude (top-left coord of layer)
            val = New cValue(0, eVarNameFlags.Longitude, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' Assume square cells
            val = New cValue(New Boolean, eVarNameFlags.AssumeSquareCells, eStatusFlags.Null, eValueTypes.Bool)
            m_values.Add(val.varName, val)

            ' CoordinateSystem
            val = New cValue("", eVarNameFlags.ProjectionString, eStatusFlags.NotEditable Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' ************************************************************************************************* '
            ' Variables for layers, providing metadata and an anchor point for remarks, visual styles, metadata '
            ' ************************************************************************************************* '

            ' LayerRelPP
            val = New cValue(0, eVarNameFlags.LayerRelPP, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerRelCin
            val = New cValue(0, eVarNameFlags.LayerRelCin, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerDepth
            val = New cValue(0, eVarNameFlags.LayerDepth, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerHabitat
            val = New cValue(0, eVarNameFlags.LayerHabitat, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerHabitatCapacity
            val = New cValue(0, eVarNameFlags.LayerHabitatCapacity, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerHabitatCapacityInput
            val = New cValue(0, eVarNameFlags.LayerHabitatCapacityInput, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerMPA
            val = New cValue(0, eVarNameFlags.LayerMPA, eStatusFlags.Null, eValueTypes.Bool)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerRegion
            val = New cValue(0, eVarNameFlags.LayerRegion, eStatusFlags.Null, eValueTypes.Int)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerMigration
            val = New cValue(0, eVarNameFlags.LayerMigration, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' MPASeed
            val = New cValue(0, eVarNameFlags.LayerMPASeed, eStatusFlags.Null, eValueTypes.Int)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerPort
            val = New cValue(0, eVarNameFlags.LayerPort, eStatusFlags.Null, eValueTypes.Bool)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerSail
            val = New cValue(0, eVarNameFlags.LayerSail, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' Advection interface
            ' LayerAdvection
            val = New cValue(0, eVarNameFlags.LayerAdvection, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerWind
            val = New cValue(0, eVarNameFlags.LayerWind, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerUpwelling
            val = New cValue(0, eVarNameFlags.LayerUpwelling, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerDriver
            val = New cValue(0, eVarNameFlags.LayerDriver, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' BiomassForcing
            val = New cValue(0, eVarNameFlags.LayerBiomassForcing, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' RelativeBiomassForcing
            val = New cValue(0, eVarNameFlags.LayerBiomassRelativeForcing, eStatusFlags.Null, eValueTypes.Sng)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' LayerExclusion
            val = New cValue(0, eVarNameFlags.LayerExclusion, eStatusFlags.Null, eValueTypes.Bool)
            val.Stored = False
            m_values.Add(val.varName, val)

            ' ----------------
            ' Init layers
            ' ----------------
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim llayers As New List(Of cEcospaceLayer)

            ' Depth layer
            Me.m_dictLayers(eVarNameFlags.LayerDepth) = New cEcospaceLayer() {New cEcospaceLayerDepth(theCore, Me, 1)}

            ' Habitats
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.NoHabitats - 1
                llayers.Add(New cEcospaceLayerHabitat(theCore, Me, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerHabitat) = llayers.ToArray

            ' Habitat capacity input layer
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.NGroups
                llayers.Add(New cEcospaceLayerHabitatCapacity(theCore, Me, eDataTypes.EcospaceLayerHabitatCapacityInput, eVarNameFlags.LayerHabitatCapacityInput, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerHabitatCapacityInput) = llayers.ToArray

            ' Habitat capacity output layer
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.NGroups
                llayers.Add(New cEcospaceLayerHabitatCapacity(theCore, Me, eDataTypes.EcospaceLayerHabitatCapacityInput, eVarNameFlags.LayerHabitatCapacity, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerHabitatCapacity) = llayers.ToArray

            ' Biomass Forcing
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.NGroups
                llayers.Add(New cEcospaceLayerBiomassForcing(theCore, Me, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerBiomassForcing) = llayers.ToArray

            ' Relative Biomass Forcing
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.NGroups
                llayers.Add(New cEcospaceLayerBiomassRelativeForcing(theCore, Me, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerBiomassRelativeForcing) = llayers.ToArray

            ' MPA layer
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.MPAno
                llayers.Add(New cEcospaceLayerMPA(theCore, Me, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerMPA) = llayers.ToArray

            ' Region layer
            Me.m_dictLayers(eVarNameFlags.LayerRegion) = New cEcospaceLayer() {New cEcospaceLayerRegion(theCore, Me)}

            ' RelPP layer
            Me.m_dictLayers(eVarNameFlags.LayerRelPP) = New cEcospaceLayer() {New cEcospaceLayerRelPP(theCore, Me)}

            ' RelCin layer
            Me.m_dictLayers(eVarNameFlags.LayerRelCin) = New cEcospaceLayer() {New cEcospaceLayerRelCin(theCore, Me)}

            ' MPA Seed
            Me.m_dictLayers(eVarNameFlags.LayerMPASeed) = New cEcospaceLayer() {New cEcospaceLayerMPASeed(theCore, Me)}

            ' Importance
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.nImportanceLayers
                llayers.Add(New cEcospaceLayerImportance(Me.m_core, ecospaceDS.ImportanceLayerDBID(i), Me, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerImportance) = llayers.ToArray()

            ' Driver
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.nEnvironmentalDriverLayers
                llayers.Add(New cEcospaceLayerDriver(Me.m_core, Me, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerDriver) = llayers.ToArray()

            ' Exclusion
            Me.m_dictLayers(eVarNameFlags.LayerExclusion) = New cEcospaceLayer() {New cEcospaceLayerExclusion(theCore, Me)}

            ' Migration
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.NGroups
                llayers.Add(New cEcospaceLayerMigration(theCore, Me, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerMigration) = llayers.ToArray()

            ' Port
            llayers.Clear()
            For i As Integer = 0 To ecospaceDS.nFleets
                llayers.Add(New cEcospaceLayerPort(theCore, Me, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerPort) = llayers.ToArray()

            ' Sailing cost
            llayers.Clear()
            For i As Integer = 1 To ecospaceDS.nFleets
                llayers.Add(New cEcospaceLayerSail(theCore, Me, i))
            Next
            Me.m_dictLayers(eVarNameFlags.LayerSail) = llayers.ToArray()

            ' Advection
            llayers.Clear()
            llayers.Add(New cEcospaceLayerAdvection(theCore, Me, 1))
            llayers.Add(New cEcospaceLayerAdvection(theCore, Me, 2))
            Me.m_dictLayers(eVarNameFlags.LayerAdvection) = llayers.ToArray()

            ' Wind
            llayers.Clear()
            llayers.Add(New cEcospaceLayerWind(theCore, Me, 1))
            llayers.Add(New cEcospaceLayerWind(theCore, Me, 2))
            Me.m_dictLayers(eVarNameFlags.LayerWind) = llayers.ToArray()

            ' Upwelling
            Me.m_dictLayers(eVarNameFlags.LayerUpwelling) = New cEcospaceLayer() {New cEcospaceLayerUpwelling(theCore, Me)}

            '' MLD
            'Me.m_dictLayers(eVarNameFlags.LayerMLD) = New cEcospaceLayer() {New cEcospaceLayerMLD(theCore, Me)}

            'set status flags to default values
            ResetStatusFlags()

            Me.AllowValidation = True

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcospaceBasemap.")
            cLog.Write(Me.ToString & ".New(..) Error creating new cEcospaceBasemap. Error: " & ex.Message)
        End Try

    End Sub

#End Region ' Constructor

#Region " Overrides "

    Public Overrides Function GetVariable(VarName As eVarNameFlags,
                                          Optional iIndex As Integer = -9999,
                                          Optional iIndex2 As Integer = -9999,
                                          Optional iIndex3 As Integer = -9999) As Object

        ' JS 07Jul14: cell size is now a derived value
        If (VarName = eVarNameFlags.CellSize) Then
            Return Me.ToCellSize(CSng(Me.GetVariable(eVarNameFlags.CellLength)), Me.AssumeSquareCells)
        End If
        Return MyBase.GetVariable(VarName, iIndex, iIndex2, iIndex3)
    End Function

    Public Overrides Function SetVariable(VarName As eVarNameFlags,
                                          newValue As Object,
                                          Optional iSecondaryIndex As Integer = -9999, Optional ByVal iThirdIndex As Integer = -9999) As Boolean

        ' JS 07Jul14: cell size is now a derived value
        If (VarName = eVarNameFlags.CellSize) Then
            Return SetVariable(eVarNameFlags.CellLength, Me.ToCellLength(CSng(newValue), Me.AssumeSquareCells))
        End If
        Return MyBase.SetVariable(VarName, newValue, iSecondaryIndex)

    End Function

#End Region ' Overrides

#Region " Variables by dot (.) operator "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcospaceDataStructures.Inrow">InRow</see>
    ''' value for this scenario
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property InRow() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.InRow))
        End Get
        Friend Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.InRow, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcospaceDataStructures.Incol">InCol</see>
    ''' value for this scenario
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property InCol() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.InCol))
        End Get
        Friend Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.InCol, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcospaceDataStructures.CellLength">CellLength</see>
    ''' value for this scenario in kilometers.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property CellLength() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.CellLength))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CellLength, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cEcospaceDataStructures.CellLength">CellLength</see>
    ''' value for this scenario in map units. The value returned here depends 
    ''' on the setting of the <see cref="AssumeSquareCells"/> flag. If set to false,
    ''' Ecospace returns the cell size in decimal degrees. If set to true, Ecospace 
    ''' returns the cell size in meters.
    ''' </summary>
    ''' <remarks>
    ''' This conversion should be explicitly driven by map projections, of course...
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property CellSize() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.CellSize))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.CellSize, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the TopLeft latitude value for the map.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Latitude() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.Latitude))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.Latitude, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the TopLeft longitude value for the map.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Longitude() As Single

        Get
            Return CSng(GetVariable(eVarNameFlags.Longitude))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.Longitude, value)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the top-left (NW) extent of the map, expressed in degrees (lon, lat)
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property PosTopLeft() As Drawing.PointF

        Get
            Return New Drawing.PointF(CSng(GetVariable(eVarNameFlags.Longitude)), CSng(GetVariable(eVarNameFlags.Latitude)))
        End Get

        Set(ByVal value As Drawing.PointF)
            SetVariable(eVarNameFlags.Longitude, value.X)
            SetVariable(eVarNameFlags.Latitude, value.Y)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the bottom-right (SE) extent of the map, expressed in degrees (lon, lat)
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property PosBottomRight() As Drawing.PointF

        Get
            Return New Drawing.PointF(CSng(GetVariable(eVarNameFlags.Longitude)) + Me.CellSize * Me.InCol,
                                      CSng(GetVariable(eVarNameFlags.Latitude)) - Me.CellSize * Me.InRow)
        End Get

        Set(ByVal value As Drawing.PointF)
            SetVariable(eVarNameFlags.Longitude, value.X - Me.CellSize * Me.InCol)
            SetVariable(eVarNameFlags.Latitude, value.Y - Me.CellSize * Me.InRow)
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <para>Get/set whether to assume square cells without latitude tapering correction. 
    ''' Square cells can be assumed on relatively small areas in UTM projections.</para>
    ''' <para>As an additional bonus Ecospace assumes meters as map units when this flag is 
    ''' set; if this flag is cleared map units are expected to be decimal degrees.</para>
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property AssumeSquareCells As Boolean
        Get
            Return CBool(Me.GetVariable(eVarNameFlags.AssumeSquareCells))
        End Get
        Set(value As Boolean)
            Me.SetVariable(eVarNameFlags.AssumeSquareCells, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <para>Get/set the Proj4 string for the Ecospace map.</para>
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ProjectionString As String
        Get
            Return CStr(Me.GetVariable(eVarNameFlags.ProjectionString))
        End Get
        Set(value As String)
            Me.SetVariable(eVarNameFlags.ProjectionString, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether a cell is modelled for Ecosystem dynamics.
    ''' </summary>
    ''' <param name="iRow">One-based row index.</param>
    ''' <param name="iCol">One-based column index.</param>
    ''' <returns>
    ''' A cell is modelled when it represent water in the included cell range.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Function IsModelledCell(ByVal iRow As Integer, ByVal iCol As Integer) As Boolean
        Return Me.m_core.m_EcoSpaceData.Depth(iRow, iCol) > 0
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether an intended cell index falls within the map bounds.
    ''' </summary>
    ''' <param name="iRow">One-based row index to validate.</param>
    ''' <param name="iCol">One-based column index to validate.</param>
    ''' <returns>True if the intended cell index falls within the map bounds,
    ''' False otherwise. No shades of grey here, let alone fifty of 'em. Ugh.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsValidCellPosition(ByVal iRow As Integer, iCol As Integer) As Boolean
        If (iRow < 1) Then Return False
        If (iCol < 1) Then Return False
        If (iRow > Me.InRow) Then Return False
        If (iCol > Me.InCol) Then Return False
        Return True
    End Function

#End Region ' Variables by dot (.) operator

#Region " Layer interface "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a LayerImportance
    ''' </summary>
    ''' <param name="index">Index from 1 to nLayerImportance</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerImportance(ByVal index As Integer) As cEcospaceLayerImportance
        Get
            Try
                Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerImportance)(index - 1), cEcospaceLayerImportance)
            Catch ex As Exception
                cLog.Write(Me.ToString & ".New(..) Unable to access importance layer of index:" & index & ". Error: " & ex.Message)
                ' ToDo: globalize this
                m_core.Messages.AddMessage(New cMessage("Unable to access importance layer with index" & index, eMessageType.DataValidation, eCoreComponentType.EcoSpace, eMessageImportance.Critical, eDataTypes.EcospaceBasemap))
                Return Nothing
            End Try
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns an external driver layer
    ''' </summary>
    ''' <param name="index">Index from 1 to nLayerDriver</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerDriver(ByVal index As Integer) As cEcospaceLayerDriver
        Get
            Try
                Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerDriver)(index - 1), cEcospaceLayerDriver)
            Catch ex As Exception
                cLog.Write(Me.ToString & ".New(..) Unable to access driver layer of index:" & index & ". Error: " & ex.Message)
                ' ToDo: globalize this
                m_core.Messages.AddMessage(New cMessage("Unable to access driver layer of index " & index, eMessageType.DataValidation, eCoreComponentType.EcoSpace, eMessageImportance.Critical, eDataTypes.EcospaceBasemap))
                Return Nothing
            End Try
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Ecospace Depth layer.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerDepth() As cEcospaceLayerDepth
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerDepth)(0), cEcospaceLayerDepth)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Ecospace port layer for a given fleet.
    ''' </summary>
    ''' <param name="iFleet">Zero-based fleet index to get the layer for. Fleet
    ''' index 0 will return the ports for All fleets.
    ''' </param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerPort(iFleet As Integer) As cEcospaceLayerPort
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerPort)(iFleet), cEcospaceLayerPort)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Ecospace sailing cost layer for a given fleet.
    ''' </summary>
    ''' <param name="iFleet">One-based Fleet index to get the layer for.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerSailingCost(iFleet As Integer) As cEcospaceLayerSail
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerSail)(iFleet - 1), cEcospaceLayerSail)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Ecospace Habitat layer.
    ''' </summary>
    ''' <param name="iHabitat">One-based habitat index</param>
    ''' <remarks>
    ''' This layer provides access to the one and only array that holds all
    ''' Habitats in Ecospace. At the moment (Nov '08), Habitats cannot overlap
    ''' and are stored in one two-dimensional array.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerHabitat(ByVal iHabitat As Integer) As cEcospaceLayerHabitat
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerHabitat)(iHabitat - 1), cEcospaceLayerHabitat)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="iGroup"></param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerHabitatCapacityInput(iGroup As Integer) As cEcospaceLayerHabitatCapacity
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerHabitatCapacityInput)(iGroup - 1), cEcospaceLayerHabitatCapacity)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="iGroup"></param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerHabitatCapacity(iGroup As Integer) As cEcospaceLayerHabitatCapacity
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerHabitatCapacity)(iGroup - 1), cEcospaceLayerHabitatCapacity)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Ecospace MPA layer.
    ''' </summary>
    ''' <param name="iMPA">One-based MPA index</param>
    ''' <remarks>
    ''' This layer provides access to the one and only array that holds all
    ''' MPAs in Ecospace.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerMPA(iMPA As Integer) As cEcospaceLayerMPA
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerMPA)(iMPA - 1), cEcospaceLayerMPA)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Ecospace Region layer.
    ''' </summary>
    ''' <remarks>
    ''' This layer provides access to the one and only array that holds all
    ''' Regions in Ecospace. At the moment (Nov '08), Regions cannot overlap
    ''' and are stored in one two-dimensional array.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerRegion() As cEcospaceLayerRegion
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerRegion)(0), cEcospaceLayerRegion)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Relative Primary Production layer in Ecospace.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerRelPP() As cEcospaceLayerRelPP
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerRelPP)(0), cEcospaceLayerRelPP)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Advection layers in Ecospace.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerAdvection() As cEcospaceLayer()
        Get
            Return Me.Layers(eVarNameFlags.LayerAdvection)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Ecospace wind layers.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerWind() As cEcospaceLayer()
        Get
            Return Me.Layers(eVarNameFlags.LayerWind)
        End Get
    End Property

    '''' -----------------------------------------------------------------------
    '''' <summary>
    '''' Get the Ecospace Mixed Layer Depths layer.
    '''' </summary>
    '''' -----------------------------------------------------------------------
    'Public ReadOnly Property LayerMixedLayerDepths() As cEcospaceLayerSingle
    '    Get
    '        Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerMLD)(0), cEcospaceLayerSingle)
    '    End Get
    'End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the flow layer in Ecospace for the current month.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerUpwelling() As cEcospaceLayerSingle
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerUpwelling)(0), cEcospaceLayerSingle)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the migration layer for a given group.
    ''' </summary>
    ''' <param name="iGroup">One-based group index.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerMigration(iGroup As Integer) As cEcospaceLayerMigration
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerMigration)(iGroup - 1), cEcospaceLayerMigration)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerRelCin() As cEcospaceLayerRelCin
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerRelCin)(0), cEcospaceLayerRelCin)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerMPASeed() As cEcospaceLayerMPASeed
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerMPASeed)(0), cEcospaceLayerMPASeed)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Ecospace exclusion layer.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property LayerExclusion() As cEcospaceLayerExclusion
        Get
            Return DirectCast(Me.m_dictLayers(eVarNameFlags.LayerExclusion)(0), cEcospaceLayerExclusion)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IEcospaceLayerManager.Layers"/>
    ''' -----------------------------------------------------------------------
    Public Function Layers(Optional ByVal varName As eVarNameFlags = eVarNameFlags.NotSet) As cEcospaceLayer() _
        Implements IEcospaceLayerManager.Layers

        Select Case varName
            Case eVarNameFlags.NotSet
                Dim l As New List(Of cEcospaceLayer)
                For Each vn As eVarNameFlags In Me.m_dictLayers.Keys
                    Select Case vn
                        Case eVarNameFlags.LayerMigration
                            Dim tmp As cEcospaceLayer() = Me.m_dictLayers(vn)
                            For i As Integer = 1 To Me.m_core.nGroups
                                If Me.m_core.EcospaceGroupInputs(i).IsMigratory Then
                                    l.Add(tmp(i - 1))
                                End If
                            Next
                        Case Else
                            l.AddRange(Me.m_dictLayers(vn))
                    End Select
                Next
                Return l.ToArray
            Case Else
                Return Me.m_dictLayers(varName)
        End Select

    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IEcospaceLayerManager.Layer"/>
    ''' -----------------------------------------------------------------------
    Public Function Layer(ByVal varName As eVarNameFlags, Optional ByVal iIndex As Integer = cCore.NULL_VALUE) As cEcospaceLayer _
        Implements IEcospaceLayerManager.Layer

        iIndex = Math.Max(iIndex, 1)
        For Each l As cEcospaceLayer In Me.Layers(varName)
            If (l.Index = iIndex) Then Return l
        Next
        Return Nothing

    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IEcospaceLayerManager.LayerData"/>
    ''' -----------------------------------------------------------------------
    Friend Function LayerData(ByVal varName As eVarNameFlags, iIndex As Integer) As Object _
        Implements IEcospaceLayerManager.LayerData

        Select Case varName
            Case eVarNameFlags.LayerDepth
                Return Me.m_core.m_EcoSpaceData.DepthInput
            Case eVarNameFlags.LayerHabitat
                Return Me.m_core.m_EcoSpaceData.PHabType
            Case eVarNameFlags.LayerHabitatCapacity
                ' Just in case, may not be needed
                Me.m_core.m_Ecospace.SetHabCap()
                Return Me.m_core.m_EcoSpaceData.HabCap
            Case eVarNameFlags.LayerHabitatCapacityInput
                Return Me.m_core.m_EcoSpaceData.HabCapInput
            Case eVarNameFlags.LayerMPA
                Return Me.m_core.m_EcoSpaceData.MPA
            Case eVarNameFlags.LayerRegion
                Return Me.m_core.m_EcoSpaceData.Region
            Case eVarNameFlags.LayerRelPP
                Return Me.m_core.m_EcoSpaceData.RelPP
            Case eVarNameFlags.LayerRelCin
                Return Me.m_core.m_EcoSpaceData.RelCin
            Case eVarNameFlags.LayerMPASeed
                Return Me.m_core.MPAOptData.MPASeed
            Case eVarNameFlags.LayerAdvection
                Return If(iIndex = 1, Me.m_core.m_EcoSpaceData.MonthlyXvel, Me.m_core.m_EcoSpaceData.MonthlyYvel)
            Case eVarNameFlags.LayerMigration
                Return Me.m_core.m_EcoSpaceData.MigMaps
            Case eVarNameFlags.LayerWind
                Return If(iIndex = 1, Me.m_core.m_EcoSpaceData.Xv, Me.m_core.m_EcoSpaceData.Yv)
            Case eVarNameFlags.LayerUpwelling
                Return Me.m_core.m_EcoSpaceData.MonthlyUpWell
            'Case eVarNameFlags.LayerMLD
            '    Return Me.m_core.m_EcoSpaceData.DepthA
            Case eVarNameFlags.LayerImportance
                Return Me.m_core.m_EcoSpaceData.ImportanceLayerMap
            Case eVarNameFlags.LayerDriver
                Return Me.m_core.m_EcoSpaceData.EnvironmentalLayerMap
            Case eVarNameFlags.LayerPort
                Return Me.m_core.m_EcoSpaceData.Port
            Case eVarNameFlags.LayerSail
                Return Me.m_core.m_EcoSpaceData.Sail
            Case eVarNameFlags.LayerBiomassForcing
                Return Me.m_core.m_EcoSpaceData.Bcell
            Case eVarNameFlags.LayerBiomassRelativeForcing
                Return Me.m_core.m_EcoSpaceData.Bcell
            Case eVarNameFlags.LayerExclusion
                Return Me.m_core.m_EcoSpaceData.Excluded
        End Select
        Return Nothing
    End Function

#End Region ' Layer interface

#Region " Cell position calculations "

    Public Function ToCellSize(ByVal sCellLength As Single, ByVal bAssumeSquareCells As Boolean) As Single
        Return cEcospaceDataStructures.ToCellSize(sCellLength, bAssumeSquareCells)
    End Function

    Public Function ToCellLength(ByVal sCellSize As Single, ByVal bAssumeSquareCells As Boolean) As Single
        Return cEcospaceDataStructures.ToCellLength(sCellSize, bAssumeSquareCells)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the top-left latitude position of the given row.
    ''' </summary>
    ''' <param name="iRow"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function RowToLat(ByVal iRow As Integer) As Single
        Return Me.Latitude - (iRow - 1) * Me.CellSize
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the one-based index of the row that contains a given latitude value.
    ''' </summary>
    ''' <param name="sLat"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function LatToRow(ByVal sLat As Single) As Integer
        Return CInt(Math.Floor((Me.Latitude - sLat) / Me.CellSize)) + 1
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the top-left longitude position of the given row.
    ''' </summary>
    ''' <param name="iCol"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function ColToLon(ByVal iCol As Integer) As Single
        Return Me.Longitude + (iCol - 1) * Me.CellSize
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the one-based index of the column that contains a given longitude 
    ''' value.
    ''' </summary>
    ''' <param name="sLon"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function LonToCol(ByVal sLon As Single) As Integer
        Return CInt(Math.Floor((sLon - Me.Longitude) / Me.CellSize)) + 1
    End Function

#End Region ' Cell calculations

End Class
