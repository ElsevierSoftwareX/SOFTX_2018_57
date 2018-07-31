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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace SpatialData

    ''' <summary>
    ''' Base spatial data adapter for inserting external spatial/temporal raster data into
    ''' Ecospace map data structures.
    ''' </summary>
    Public Class cSpatialDataAdapter
        Inherits cCoreInputOutputBase

#Region " Private vars "

        Protected m_connections() As List(Of cSpatialDataConnection)
        Protected m_astrBackupFiles() As String
        Protected m_bIsEnabled() As Boolean

        ''' <summary>Ecospace variable to operate onto.</summary>
        Protected m_varName As eVarNameFlags = Nothing
        ''' <summary>Core counter that this adapter operates onto.</summary>
        Protected m_coreCounter As eCoreCounterTypes = eCoreCounterTypes.NotSet

#End Region ' Private vars

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new instance of this class.
        ''' </summary>
        ''' <param name="core">The core to use.</param>
        ''' <param name="varName">The ecospace layer, identified by <see cref="eVarNameFlags">varname</see>,
        ''' that this adapter will interface with.</param>
        ''' <param name="cc">The <see cref="eCoreCounterTypes">core counter</see> that states the
        ''' number of layers that this adapter will interface with.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal core As cCore, ByVal varName As eVarNameFlags, cc As eCoreCounterTypes)

            MyBase.New(core)

            Me.m_dataType = eDataTypes.EcospaceSpatialDataSource
            Me.m_coreComponent = eCoreComponentType.EcoSpace
            Me.m_coreCounter = cc
            Me.m_varName = varName
            Me.AllowSaveIntermediateResults = False
            Me.DBID = -1
            Me.AllowValidation = True

            Me.Initialize()

        End Sub

#End Region ' Constructor

#Region " Basic bits "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether intermediate rasters will be saved to disk when
        ''' obtained from an exernal data connection.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property AllowSaveIntermediateResults As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the adapter in response to an Ecospace scenario (re)load.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Initialize()

            Dim iNumItems As Integer = Math.Max(0, Me.m_core.GetCoreCounter(Me.m_coreCounter)) + 1

            ReDim Me.m_connections(iNumItems)
            For i As Integer = 0 To iNumItems
                Me.m_connections(i) = New List(Of cSpatialDataConnection)
            Next

            ReDim Me.m_astrBackupFiles(iNumItems)
            ReDim Me.m_bIsEnabled(iNumItems)

            For i As Integer = 0 To iNumItems
                Me.m_bIsEnabled(i) = True
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the maximum number of layers for this adapter.
        ''' </summary>
        ''' <returns>The number of layers for this adapter.</returns>
        ''' -------------------------------------------------------------------
        Public Function MaxLength() As Integer
            Return Math.Max(1, Me.m_core.GetCoreCounter(Me.m_coreCounter))
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return whether a layer in this adapter is connected to external data.
        ''' </summary>
        ''' <param name="iLayer">The one-based index of the layer to query.</param>
        ''' -------------------------------------------------------------------
        Public Function IsConnected(ByVal iLayer As Integer) As Boolean

            Dim bConnected As Boolean = False
            For Each conn As cSpatialDataConnection In Me.Connections(iLayer)
                bConnected = bConnected Or conn.IsConfigured()
            Next
            Return bConnected

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return whether a layer in this adapter is allowed to exchange external 
        ''' data.
        ''' </summary>
        ''' <param name="iLayer">The one-based index of the layer to query.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Property IsEnabled(ByVal iLayer As Integer) As Boolean
            Get
                Return Me.m_bIsEnabled(iLayer)
            End Get
            Set(value As Boolean)
                If (value <> Me.m_bIsEnabled(iLayer)) Then
                    Me.m_bIsEnabled(iLayer) = value
                    Me.OnChanged()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eVarNameFlags">variable name</see> for the type
        ''' of layer that this adapter operates onto.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property VarName() As eVarNameFlags
            Get
                Return Me.m_varName
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the status for the underlying layer.
        ''' </summary>
        ''' <param name="iLayer">The one-based index of the layer to query.</param>
        ''' -------------------------------------------------------------------
        Public Overridable ReadOnly Property Status(ByVal iLayer As Integer) As eStatusFlags
            Get
                ' This is hack!
                If (Me.m_varName = eVarNameFlags.LayerDriver) Then
                    Dim manager As IEnvironmentalResponseManager = Me.m_core.CapacityMapInteractionManager
                    Dim map As IEnviroInputData = Nothing

                End If
                Return eStatusFlags.OK
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Populate the core data that this adapter is responsible for.
        ''' </summary>
        ''' <param name="iTime">The one-based Ecospace time step to populate data for.</param>
        ''' <param name="dNoData">The no data value for the Ecospace layer.</param>
        ''' <param name="layer">The layers to populate. If left to null, all layers
        ''' for the implicit <see cref="VarName"/> will be populated.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function Populate(ByVal iTime As Integer, ByVal dNoData As Double, _
                                             Optional ByVal layer As cEcospaceLayer = Nothing) As Boolean

            Dim strMsg As String = ""
            Dim msg As cMessage = Nothing
            Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
            Dim dataExternal As ISpatialRaster = Nothing
            Dim dCellSize As Double = Math.Round(CDbl(bm.CellSize), 8)
            Dim layers As cEcospaceLayer() = Nothing
            Dim dt As Date
            Dim bSuccess As Boolean = True

            ' Decide which layers to update
            If (layer Is Nothing) Then
                layers = bm.Layers(Me.VarName)
            Else
                layers = New cEcospaceLayer() {layer}
            End If

            For Each layer In layers

                If layer.IsActive() And Me.IsEnabled(layer.Index) Then

                    For Each conn As cSpatialDataConnection In Me.m_connections(layer.Index)
                        ' Is ready to go?
                        If conn.IsConfigured Then

                            ' Get dataset and converter
                            Dim ds As ISpatialDataSet = conn.Dataset
                            Dim cv As ISpatialDataConverter = conn.Converter

                            ' #Yes: has data for this time step?
                            dt = Me.m_core.EcospaceTimestepToAbsoluteTime(iTime)

                            If (ds.HasDataAtT(dt)) Then

                                ' Internal log, no need to translate
                                strMsg = "cSpatialDataAdapter::Populate({0}.{1}) dataset {2} trying to load data for T{3}, ext({4},{5}) to ({6},{7})"
                                cLog.Write(cStringUtils.Localize(strMsg, Me.ToString, layer.ToString(), ds.DisplayName, iTime, bm.PosTopLeft.X, bm.PosTopLeft.Y, bm.PosBottomRight.X, bm.PosBottomRight.Y), eVerboseLevel.Detailed)

                                ' #Yes: Can lock that data?
                                If (ds.LockDataAtT(dt, dCellSize, bm.PosTopLeft, bm.PosBottomRight, bm.ProjectionString)) Then
                                    ' #Yes: start process of extracting external data

                                    ' Start logging the operations on successfully locked data
                                    Me.m_core.SpatialOperationLog.BeginLayerLog(iTime, dt, Me.VarName, layer)

                                    ' Sanity check
                                    Debug.Assert(ds.IsLocked, "Dataset is not locked - something is wrong")

                                    Try
                                        ' The raster returned here MUST have the extent and projection compatible with Ecospace
                                        dataExternal = ds.GetRaster(cv, layer.Name)
                                    Catch ex As Exception
                                        Me.m_core.SpatialOperationLog.LogOperation(cStringUtils.Localize(My.Resources.CoreMessages.STATUS_EXCEPTION, ex.Message), _
                                                                                   eStatusFlags.MissingParameter)
                                        cLog.Write(ex, "cSpatialDataAdapter::Populate(" & layer.ToString() & ")")
                                        bSuccess = False
                                    End Try

                                    If (dataExternal IsNot Nothing) Then

                                        ' Stop any validation
                                        Dim bAllow As Boolean = layer.AllowValidation
                                        layer.AllowValidation = False

                                        Me.SaveIntermediateResults(iTime, dataExternal)

                                        ' Notify world
                                        If (Me.m_core.PluginManager IsNot Nothing) Then
                                            Me.m_core.PluginManager.EcospaceBeginLayerChange(iTime, dt, layer)
                                        End If

                                        ' Integrate data
                                        Me.Adapt(bm, layer, conn, iTime, dt, dataExternal, dNoData)

                                        ' Notify world
                                        If (Me.m_core.PluginManager IsNot Nothing) Then
                                            Me.m_core.PluginManager.EcospaceEndLayerChange(iTime, dt, layer)
                                        End If

                                        ' Restore layer validation
                                        layer.AllowValidation = bAllow

                                        ' Done, clean up
                                        dataExternal.Dispose()
                                        dataExternal = Nothing

                                    Else
                                        strMsg = cStringUtils.Localize(My.Resources.CoreMessages.SPATIALTEMPORAL_POP_FAILED_LOAD, layer.ToString(), ds.DisplayName, iTime, bm.PosTopLeft.X, bm.PosTopLeft.Y, bm.PosBottomRight.X, bm.PosBottomRight.Y, dCellSize)
                                        Me.m_core.SpatialOperationLog.LogOperation(strMsg, eStatusFlags.MissingParameter)
                                        cLog.Write(strMsg)
                                        bSuccess = False
                                    End If

                                    ' Unlock dataset
                                    ds.Unlock()

                                    ' Done logging
                                    Me.m_core.SpatialOperationLog.EndLayerLog()
                                Else
                                    strMsg = cStringUtils.Localize(My.Resources.CoreMessages.SPATIALTEMPORAL_POP_FAILED_LOCK, layer.ToString(), ds.DisplayName, iTime, bm.PosTopLeft.X, bm.PosTopLeft.Y, bm.PosBottomRight.X, bm.PosBottomRight.Y, dCellSize)
                                    cLog.Write(strMsg)
                                    bSuccess = False
                                End If

                            End If
                        End If
                    Next
                End If
            Next layer

            Return bSuccess

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Perform pre-run initializations for all adapters such as preserving
        ''' layer data prior to a run. Individual adapters can perform their 
        ''' own initialization in.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub InitRun()
            Me.SaveLayerData()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Perform post-run cleanup for all adapters such as restoring
        ''' layer data after to a run, as an accompanying method to.
        ''' Individual adapters can perform their own cleanup in.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub EndRun()
            Me.RestoreLayerData()
        End Sub


        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load data from an external data raster into an Ecospace array.
        ''' </summary>
        ''' <param name="bm">The <see cref="cEcospaceBasemap"/> for the scenario to load data into.</param>
        ''' <param name="layer">The <see cref="cEcospaceLayer"/> that will receive the data.</param>
        ''' <param name="iTime">The Ecospace time step to load data for.</param>
        ''' <param name="dataExternal">The <see cref="ISpatialRaster"/> that holds the loaded external data.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>Note that this method writes values straight into the underlying data structures!</remarks>
        ''' -------------------------------------------------------------------
        Protected Friend Overridable Function Adapt(ByVal bm As cEcospaceBasemap, _
                                                    ByVal layer As cEcospaceLayer, _
                                                    ByVal conn As cSpatialDataConnection, _
                                                    ByVal iTime As Integer, _
                                                    ByVal dt As Date, _
                                                    ByVal dataExternal As ISpatialRaster, _
                                                    ByVal dNoData As Double) As Boolean

            ' To ensure proper usage by inherited classes
            Debug.Assert(bm IsNot Nothing)
            Debug.Assert(layer IsNot Nothing)
            Debug.Assert(dataExternal IsNot Nothing)

            Dim layerDepth As cEcospaceLayerDepth = bm.LayerDepth
            Dim msg As cMessage = Nothing
            Dim sValue As Double = 0
            Dim bSuccess As Boolean = True ' Think positive. Really
            Dim iNumRows As Integer = bm.InRow
            Dim iNumCols As Integer = bm.InCol
            Dim iRow As Integer
            Dim iCol As Integer

            Try
                ' For all rows
                iRow = 1
                While (iRow <= iNumRows) And (bSuccess = True)
                    ' For all columns
                    iCol = 1
                    While (iCol <= iNumCols) And (bSuccess = True)
                        sValue = dataExternal.Cell(iRow, iCol, dNoData)
                        ' Is a valid value?
                        If (sValue <> cCore.NULL_VALUE) Or (Me.m_varName = eVarNameFlags.LayerDepth) Then
                            ' #Yes: set value
                            bSuccess = bSuccess And Me.SetCell(layer, conn, iRow, iCol, sValue)
                        Else
                            bSuccess = bSuccess And Me.SetCell(layer, conn, iRow, iCol, dNoData)
                        End If
                        iCol += 1
                    End While ' iCol
                    iRow += 1
                End While ' iRow

                If bSuccess Then
                    Me.m_core.SpatialOperationLog.LogOperation(cStringUtils.Localize(My.Resources.CoreMessages.STATUS_SPATIALTEMPORAL_APPLIED, dataExternal.ToString()),
                                                               eStatusFlags.OK)
                End If

            Catch ex As Exception
                Me.m_core.SpatialOperationLog.LogOperation(cStringUtils.Localize(My.Resources.CoreMessages.STATUS_EXCEPTION, ex.Message),
                                                           eStatusFlags.FailedValidation)
                cLog.Write(ex, "cSpatialDataAdapter::Adapt(" & layer.ToString() & ")")
                bSuccess = False
            End Try

            Return bSuccess

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set a cell value into the underlying layer.
        ''' </summary>
        ''' <param name="layer">The layer to set the value into.</param>
        ''' <param name="iRow">One-based row index for setting the value.</param>
        ''' <param name="iCol">One-based column index for setting the value.</param>
        ''' <param name="sCellValueAtT">The value to set in the cell, as obtained from 
        ''' external data.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function SetCell(ByVal layer As cEcospaceLayer, _
                                               ByVal conn As cSpatialDataConnection, _
                                               ByVal iRow As Integer, _
                                               ByVal iCol As Integer, _
                                               ByVal sCellValueAtT As Double) As Boolean
            Try
                layer.Cell(iRow, iCol) = sCellValueAtT
            Catch ex As Exception

                Dim strMsg As String = "cSpatialDataAdapter::SetCell({0}) at ({1},{2})={3}: exception {4}"
                cLog.Write(ex, cStringUtils.Localize(strMsg, layer.ToString, iCol, iRow, sCellValueAtT))

                Me.m_core.SpatialOperationLog.LogOperation(cStringUtils.Localize(My.Resources.CoreMessages.STATUS_SPATIALTEMPORAL_ADAPTERROR, iRow, iCol, sCellValueAtT, ex.Message), _
                                                           eStatusFlags.MissingParameter)
                Return False
            End Try
            Return True

        End Function

        Public Sub OnChanged()
            If (Me.AllowValidation) Then
                Me.m_core.onChanged(Me, eMessageType.DataModified)
            End If
        End Sub

#End Region ' Basic bits

#Region " Layer rescue "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the content of adapter-managed layers to a temporary file.
        ''' <seealso cref="RestoreLayerData"/>
        ''' </summary>
        ''' <remarks>
        ''' Note that only the content of layers <see cref="cEcospaceLayer.IsExternalData">configured to receive external data</see>
        ''' will be preserved, and only for layers of type single, integer or boolean.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Friend Overridable Sub SaveLayerData()

            ' Wipe, just in case
            For i As Integer = 0 To Me.m_core.GetCoreCounter(Me.m_coreCounter)
                Me.m_astrBackupFiles(i) = String.Empty
            Next

            Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
            Dim iNumRow As Integer = bm.InRow
            Dim iNumCol As Integer = bm.InCol
            Dim strFileName As String = ""
            Dim sw As StreamWriter = Nothing
            Dim tData As Type = Nothing

            ' For all layers
            For Each layer As cEcospaceLayer In bm.Layers(Me.m_varName)
                ' Is driven by external data?
                If (Me.IsConnected(layer.Index)) Then
                    ' #Yes: set up a temp file and save the layer content to the file
                    Try
                        strFileName = cFileUtils.MakeTempFile(".ewetmp")
                        tData = layer.ValueType
                        sw = New StreamWriter(strFileName)
                        For iRow As Integer = 1 To iNumRow
                            For iCol As Integer = 1 To iNumCol
                                If (iCol > 1) Then sw.Write(",")
                                If tData Is GetType(Single) Or tData Is GetType(Integer) Then
                                    sw.Write(cStringUtils.FormatNumber(layer.Cell(iRow, iCol)))
                                ElseIf tData Is GetType(Boolean) Then
                                    sw.Write(layer.Cell(iRow, iCol).ToString())
                                End If
                            Next iCol
                            sw.WriteLine()
                        Next iRow

                        ' Clean up
                        sw.Flush()
                        sw.Close()
                        sw = Nothing

                        ' Store the name of the file where this layer's data was preserved
                        Me.m_astrBackupFiles(layer.Index) = strFileName
#If DEBUG Then
                        Console.WriteLine("Adapter " & Me.ToString & " saved content of layer " & layer.ToString & " to " & strFileName)
#End If
                        cLog.Write("cSpatialDataAdapter::SaveLayerData successful for " & layer.Name & " into " & strFileName, eVerboseLevel.Detailed)

                    Catch ex As Exception
                        ' Log failure, plod along
                        cLog.Write(ex, "cSpatialDataAdapter::SaveLayerData " & Me.ToString & ", layer " & layer.ToString & ", file " & strFileName)
                    End Try

                End If
            Next layer

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Restore the content of layers from a temporary file.
        ''' <seealso cref="SaveLayerData"/>
        ''' </summary>
        ''' -------------------------------------------------------------------
        Friend Overridable Sub RestoreLayerData()

            Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
            Dim iNumRow As Integer = bm.InRow
            Dim iNumCol As Integer = bm.InCol
            Dim tData As Type = Nothing
            Dim sr As StreamReader = Nothing
            Dim strFileName As String = ""

            For Each layer As cEcospaceLayer In bm.Layers(Me.m_varName)

                strFileName = Me.m_astrBackupFiles(layer.Index)
                tData = layer.ValueType

                If (Not String.IsNullOrWhiteSpace(strFileName) And Me.IsConnected(layer.Index)) Then
                    Try
#If DEBUG Then
                        Console.WriteLine("Adapter " & Me.ToString & " restoring layer " & layer.ToString & " from " & strFileName)
#End If
                        sr = New StreamReader(strFileName)
                        For iRow As Integer = 1 To iNumRow
                            Dim strLine As String = sr.ReadLine
                            Dim astrFields As String() = strLine.Split(","c)
                            For iCol As Integer = 1 To iNumCol

                                If tData Is GetType(Single) Or tData Is GetType(Integer) Then
                                    layer.Cell(iRow, iCol) = cStringUtils.ConvertToNumber(astrFields(iCol - 1), tData)
                                ElseIf tData Is GetType(Boolean) Then
                                    layer.Cell(iRow, iCol) = Boolean.Parse(astrFields(iCol - 1))
                                End If

                            Next iCol
                        Next iRow
                        sr.Close()
                        sr = Nothing
                        cLog.Write("cSpatialDataAdapter::RestoreLayerData successful for " & layer.Name & " from " & strFileName, eVerboseLevel.Detailed)
                    Catch ex As Exception
                        ' Whoah!
                        cLog.Write(ex, "cSpatialDataAdapter::RestoreLayerData " & Me.ToString & ", layer " & layer.ToString & ", file " & strFileName)
                    End Try
                    ' Remove this temp file
                    cFileUtils.PurgeTempFile(strFileName)
                End If
                Me.m_astrBackupFiles(layer.Index) = String.Empty
            Next layer

        End Sub


        Public Overridable Function RestoreForcing(SpaceData As cEcospaceDataStructures) As Boolean
            'Only forcing adapters have data to restore
            Return True

        End Function

#End Region ' Layer rescue

#Region " Debugging "

        Protected Sub SaveIntermediateResults(iTime As Integer, dataExternal As ISpatialRaster)

            If Not Me.AllowSaveIntermediateResults Then Return

            Dim strPath As String = Me.getIntermediateOutputDir()
            Dim strFile As String = Me.getIntermediateFile(strPath, iTime)

            If Not cFileUtils.IsDirectoryAvailable(strPath, True) Then Return

            dataExternal.Save(strFile)

        End Sub

#End Region ' Debugging

#Region " Intermediate output files "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the directory for storing intermedite results for debugging.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property IntermediateSubDirectory As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the file name for storing intermedite results for debugging.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property IntermediateFileName As String

        Protected Function getIntermediateOutputDir() As String
            If Not String.IsNullOrWhiteSpace(Me.IntermediateSubDirectory) Then
                Return Path.Combine(Me.m_core.DefaultOutputPath(eAutosaveTypes.EcospaceResults), Me.IntermediateSubDirectory)
            End If
            Return Path.Combine(Me.m_core.DefaultOutputPath(eAutosaveTypes.EcospaceResults), "_debug_")
        End Function

        Protected Function getIntermediateFile(ByVal thePath As String, iTime As Integer) As String
            If Not String.IsNullOrWhiteSpace(Me.IntermediateFileName) Then
                Return Path.Combine(thePath, cFileUtils.ToValidFileName(Me.IntermediateFileName + "_" + Me.m_core.EcospaceTimestepToAbsoluteTime(iTime).ToShortDateString + ".asc", False))
            End If
            Return Path.Combine(thePath, cFileUtils.ToValidFileName("in_" & Me.m_varName.ToString & "_" & Me.Index & "_" & iTime & ".asc", False))
        End Function

#End Region ' Intermediate output files

#Region " Connections "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return configured connections for this adapter.
        ''' </summary>
        ''' <param name="iLayer">The one-based index of the layer to return connections for.
        ''' If less than one, configured connections for all layers in the adapter are returned.</param>
        ''' <param name="bEnabledOnly">If true, only those layers specified by <paramref name="iLayer"/>
        ''' that are <see cref="IsEnabled">enabled</see> are examined. If this flag is False, 
        ''' all layers specified by <paramref name="iLayer"/> are examined.</param>
        ''' <returns>Configured connections for this adapter.</returns>
        ''' -------------------------------------------------------------------
        Public Function Connections(Optional iLayer As Integer = -1, _
                                    Optional bEnabledOnly As Boolean = False) As cSpatialDataConnection()

            Dim iFrom As Integer = iLayer
            Dim iTo As Integer = iLayer
            Dim lConn As New List(Of cSpatialDataConnection)

            If (iFrom <= 0) Then iFrom = 1
            If (iTo <= 0) Then iTo = Me.MaxLength
            For iLayer = iFrom To iTo
                If Me.IsEnabled(iLayer) Or (bEnabledOnly = False) Then
                    If Me.m_connections(iLayer).Count > 0 Then
                        lConn.AddRange(Me.m_connections(iLayer))
                    End If
                End If
            Next

            Return lConn.ToArray

        End Function

        Public Function AddConnection(ByVal iLayer As Integer) As cSpatialDataConnection

            Dim conn As cSpatialDataConnection = Nothing
            If (Me.m_connections(iLayer).Count < cSpatialDataStructures.cMAX_CONN) Then
                conn = Me.NewConnection()
                conn.Adapter = Me
                conn.iLayer = iLayer
                Me.m_connections(iLayer).Add(conn)
            End If
            Return conn

        End Function

        Public Function RemoveConnection(ByVal iLayer As Integer, ByVal conn As cSpatialDataConnection) As Boolean

            Me.m_connections(iLayer).Remove(conn)
            Return True

        End Function

        Protected Overridable Function NewConnection() As cSpatialDataConnection
            Return New cSpatialDataConnection()
        End Function

#End Region ' Connections

        Public Overrides Function ToString() As String
            Return Me.VarName.ToString()
        End Function

    End Class

End Namespace
