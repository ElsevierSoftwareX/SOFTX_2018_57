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
Imports System.Reflection
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports EwEUtils.SystemUtilities

#End Region ' Imports

' ToDo_JS: Load and save converter configuration

Namespace SpatialData

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Manages the connections between available <see cref="cSpatialDataAdapter"/>s 
    ''' and <see cref="ISpatialDataSet"/>s
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cSpatialDataConnectionManager
        Implements IDisposable
        Implements ICoreInterface

#Region " Private helper classes "

        Private Class cAdapterComparer
            Implements IComparer(Of cSpatialDataAdapter)

            Public Function Compare(x As cSpatialDataAdapter, y As cSpatialDataAdapter) As Integer _
                Implements System.Collections.Generic.IComparer(Of cSpatialDataAdapter).Compare

                If (x.VarName = y.VarName) Then Return 0
                Return CInt(If(x.VarName < y.VarName, -1, 1))

            End Function

        End Class

        Private Class cDatasetComparer
            Implements IComparer(Of ISpatialDataSet)

            Public Function Compare(x As ISpatialDataSet, y As ISpatialDataSet) As Integer _
                Implements IComparer(Of ISpatialDataSet).Compare

                Dim strX As String = x.DisplayName
                Dim strY As String = y.DisplayName

                If (TypeOf x Is IPlugin) Then strX = DirectCast(x, IPlugin).Name
                If (TypeOf y Is IPlugin) Then strY = DirectCast(y, IPlugin).Name

                Return String.Compare(strX, strY, True)
            End Function

        End Class

        Private Class cConverterComparer
            Implements IComparer(Of ISpatialDataConverter)

            Public Function Compare(x As ISpatialDataConverter, y As ISpatialDataConverter) As Integer _
                Implements IComparer(Of ISpatialDataConverter).Compare

                Dim strX As String = x.DisplayName
                Dim strY As String = y.DisplayName

                If (TypeOf x Is IPlugin) Then strX = DirectCast(x, IPlugin).Name
                If (TypeOf y Is IPlugin) Then strY = DirectCast(y, IPlugin).Name

                Return String.Compare(strX, strY, True)
            End Function

        End Class

#End Region ' Private helper classes

#Region " Variables "

        ''' <summary>Manager of active data sets.</summary>
        Private m_datasetManager As cSpatialDataSetManager = Nothing
        Private m_core As cCore = Nothing
        Private m_data As cSpatialDataStructures = Nothing

#End Region ' Variables

#Region " Construction/ destruction "

        Friend Sub New()
        End Sub

        Friend Sub Init(ByVal core As cCore, ByVal data As cSpatialDataStructures)

            Me.m_core = core
            Me.m_data = data
            Me.m_datasetManager = New cSpatialDataSetManager(core)

            Me.CreateAdapters()

        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If (Me.m_core IsNot Nothing) Then
                Me.Clear()
                Me.m_datasetManager.Dispose()
                Me.m_datasetManager = Nothing
                Me.m_core = Nothing
            End If
            GC.SuppressFinalize(Me)
        End Sub

#End Region ' Construction/ destruction

#Region " Generic information "

        Public Sub Load()

            Dim ds As ISpatialDataSet = Nothing
            Dim cv As ISpatialDataConverter = Nothing
            Dim cfg As cSpatialDataStructures.cAdapaterConfiguration = Nothing
            Dim conn As cSpatialDataConnection = Nothing
            Dim t As Type = Nothing

            For Each adt As cSpatialDataAdapter In Me.Adapters
                For i As Integer = 1 To adt.MaxLength
                    For j As Integer = 1 To cSpatialDataStructures.cMAX_CONN

                        cfg = Me.m_data.Item(adt.VarName, i, j)
                        Debug.Assert(cfg IsNot Nothing)

                        ds = Me.m_datasetManager.CreateDataset(cfg)
                        cv = Me.m_datasetManager.CreateConverter(cfg)
                        If (ds IsNot Nothing) Then
                            conn = adt.AddConnection(i)
                            conn.Dataset = ds
                            conn.Converter = cv
                            conn.Scale = cfg.Scale
                            conn.ScaleType = DirectCast(cfg.ScaleType, cSpatialScalarDataAdapterBase.eScaleType)
                        End If
                    Next j
                Next i
            Next adt

            Me.NotifyCore(eMessageType.DataAddedOrRemoved)

        End Sub

        ''' <summary>
        ''' Apply spatial configration details to the underlying spatial data structures.
        ''' </summary>
        ''' <param name="adt">The adapter to update. If left empty, all adapters will be updated.</param>
        Public Sub Update(Optional adt As cSpatialDataAdapter = Nothing, Optional ForceUpdate As Boolean = False)

            'If Not Me.m_core.StateMonitor.HasEcospaceLoaded Then Return

            Dim ds As ISpatialDataSet = Nothing
            Dim cv As ISpatialDataConverter = Nothing
            Dim cfg As cSpatialDataStructures.cAdapaterConfiguration = Nothing

            Dim adapters As New List(Of cSpatialDataAdapter)
            If (adt IsNot Nothing) Then
                adapters.Add(adt)
            Else
                adapters.AddRange(Me.Adapters)
            End If

            For Each adt In adapters
                For i As Integer = 1 To adt.MaxLength
                    Dim connections As cSpatialDataConnection() = adt.Connections(i)
                    Dim conn As cSpatialDataConnection = Nothing
                    For j As Integer = 1 To cSpatialDataStructures.cMAX_CONN

                        ' Get connection
                        If (j <= connections.Length) Then
                            conn = connections(j - 1)
                        Else
                            conn = Nothing
                        End If

                        ' Get configuration
                        cfg = Me.m_data.Item(adt.VarName, i, j)

                        Debug.Assert(cfg IsNot Nothing)

                        If (conn IsNot Nothing) Then
                            Me.m_datasetManager.UpdateDataset(conn.Dataset, cfg)
                            Me.m_datasetManager.UpdateConverter(conn.Converter, cfg)
                            cfg.Scale = conn.Scale
                            cfg.ScaleType = CByte(conn.ScaleType)
                        Else
                            cfg.Clear()
                        End If
                    Next j
                Next i
            Next adt

            Me.NotifyCore(eMessageType.DataAddedOrRemoved, ForceUpdate)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the number of <see cref="cSpatialDataAdapter.IsConnected">live data connections</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property NumConnectedAdapters As Integer
            Get
                Dim n As Integer = 0
                For Each adt As cSpatialDataAdapter In Me.Adapters
                    For i As Integer = 1 To adt.MaxLength
                        If adt.IsEnabled(i) Then
                            For Each conn As cSpatialDataConnection In adt.Connections(i)
                                If conn.IsConfigured() Then n += 1
                            Next
                        End If
                    Next
                Next
                Return n
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns an array of all <see cref="ISpatialDataset">data sets</see>
        ''' that are known to be missing data.
        ''' </summary>
        ''' <param name="bEnabledOnly">If true, only <see cref="ISpatialDataset">data sets</see>
        ''' that are <see cref="cSpatialDataAdapter.IsEnabled">enabled</see> are
        ''' checked. If false, all <see cref="ISpatialDataset">data sets</see> are checked.</param>
        ''' <returns>An array of all <see cref="cSpatialDataConnection">connections</see>
        ''' that are known to be missing data.</returns>
        ''' -------------------------------------------------------------------
        Public Function InvalidConnections(bEnabledOnly As Boolean) As ISpatialDataSet()

            Dim problems As New List(Of ISpatialDataSet)

            For Each adt As cSpatialDataAdapter In Me.Adapters
                For Each conn As cSpatialDataConnection In adt.Connections(bEnabledOnly:=bEnabledOnly)
                    If conn.IsConfigured() Then
                        Dim comp As cDatasetCompatilibity = Me.m_datasetManager.Compatibility(conn.Dataset)
                        If (comp.NumError > 0) And (Not problems.Contains(conn.Dataset)) Then
                            problems.Add(conn.Dataset)
                        End If
                    End If
                Next
            Next
            Return problems.ToArray()

        End Function

#End Region ' Generic information

#Region " Adapters "

        Public ReadOnly Property Adapter(ByVal varname As eVarNameFlags) As cSpatialDataAdapter
            Get
                For Each adt As cSpatialDataAdapter In Me.m_data.DataAdapters
                    If (adt.VarName = varname) Then
                        Return adt
                    End If
                Next
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property nAdapters As Integer
            Get
                Return Me.m_data.DataAdapters.Count
            End Get
        End Property

        Public ReadOnly Property Adapters() As cSpatialDataAdapter()
            Get
                Dim adt() As cSpatialDataAdapter = Me.m_data.DataAdapters.ToArray
                Array.Sort(adt, New cAdapterComparer())
                Return adt
            End Get
        End Property

        Public Sub AddAdapter(adapter As cSpatialDataAdapter)
            Me.m_data.DataAdapters.Add(adapter)
        End Sub

#End Region ' Adapters

#Region " Data sets "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns an array of dataset templates from the core assemly and 
        ''' from loaded plug-ins.
        ''' </summary>
        ''' <param name="vn">The varname to filter by, if any.</param>
        ''' -------------------------------------------------------------------
        Public Function DatasetTemplates(Optional vn As eVarNameFlags = eVarNameFlags.NotSet) As ISpatialDataSet()

            Dim lDatasets As New List(Of ISpatialDataSet)

            For Each t As Type In Assembly.GetAssembly(GetType(cCore)).GetTypes()
                If t.IsAssignableFrom(GetType(ISpatialDataSet)) And t.IsPublic Then
                    Dim ds As ISpatialDataSet = DirectCast(Activator.CreateInstance(t), ISpatialDataSet)
                    If (ds.VarName = vn Or ds.VarName = eVarNameFlags.NotSet Or vn = eVarNameFlags.NotSet) Then
                        lDatasets.Add(ds)
                    End If
                End If
            Next

            Dim pm As cPluginManager = Me.m_core.PluginManager

            If (pm IsNot Nothing) Then
                For Each ip As IPlugin In pm.GetPlugins(GetType(ISpatialDataSetPlugin))
                    If (TypeOf ip Is ISpatialDataSet) Then
                        Dim ds As ISpatialDataSet = DirectCast(ip, ISpatialDataSet)
                        If (ds.VarName = vn Or ds.VarName = eVarNameFlags.NotSet Or vn = eVarNameFlags.NotSet) Then
                            lDatasets.Add(ds)
                        End If
                    End If
                Next
            End If

            lDatasets.Sort(New cDatasetComparer())
            Return lDatasets.ToArray

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a reference to the embedded dataset manager.
        ''' </summary>
        ''' <returns>A reference to the embedded dataset manager.</returns>
        ''' -------------------------------------------------------------------
        Public Function DatasetManager() As cSpatialDataSetManager
            Return Me.m_datasetManager
        End Function

        Public Sub Update(ByVal ds As ISpatialDataSet, Optional ForceUpdate As Boolean = False)
            Me.m_datasetManager.Compatibility(ds).Invalidate()
            Me.Update()
            ' Only send out event if this dataset is used in a spat/temp configuration
            If (Me.IsApplied(ds)) Then
                Me.NotifyCore(eMessageType.DataModified, ForceUpdate)
            End If
        End Sub

        Public Sub Update(ByVal cv As ISpatialDataConverter, Optional ForceUpdate As Boolean = False)
            ' ToDo: implement selective update
            Me.Update()
            ' ToDo: Only send out event this converter is used in a spat/temp configuration
            Me.NotifyCore(eMessageType.DataModified, ForceUpdate)
        End Sub

        ''' <summary>
        ''' Notify the core that data has changed. If there are no configured external data connections
        ''' this notification will be cancelled. To bypass this check and force the notification, set
        ''' <paramref name="ForceUpdate"/> to True.
        ''' </summary>
        ''' <param name="importance"></param>
        ''' <param name="ForceUpdate">True to force the notification</param>
        Public Sub NotifyCore(importance As eMessageType, Optional ForceUpdate As Boolean = False)
            Try
                ' Assume that this has affected currently configured adapters, because 
                ' this is very likely. This check can be improved.
                If (Me.NumConnectedAdapters > 0) Or (ForceUpdate = True) Then
                    Me.m_core.onChanged(Me, importance)
                End If
            Catch ex As Exception
                'Ouch
            End Try
        End Sub

        Public Function IsApplied(ds As ISpatialDataSet, Optional bEnabledOnly As Boolean = False) As Boolean
            For Each adt As cSpatialDataAdapter In Me.Adapters
                For i As Integer = 1 To adt.MaxLength
                    If adt.IsEnabled(i) Or bEnabledOnly = False Then
                        For Each conn As cSpatialDataConnection In adt.Connections(i)
                            If ReferenceEquals(conn.Dataset, ds) Then Return True
                        Next
                    End If
                Next
            Next
            Return False
        End Function

#End Region ' Data sets

#Region " Converters "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns an array of data converter templates compatible with a <see cref="ISpatialDataSet"/>.
        ''' </summary>
        ''' <returns>An array of compatible <see cref="ISpatialDataConverter">converters</see>.</returns>
        ''' -------------------------------------------------------------------
        Public Function ConverterTemplates(ds As ISpatialDataSet) As ISpatialDataConverter()

            Dim lConverters As New List(Of ISpatialDataConverter)
            Dim pm As cPluginManager = Me.m_core.PluginManager

            If (pm IsNot Nothing And ds IsNot Nothing) Then
                For Each ip As IPlugin In pm.GetPlugins(GetType(ISpatialDataConverterPlugin))
                    If (TypeOf ip Is ISpatialDataConverter) Then
                        Dim conv As ISpatialDataConverter = DirectCast(ip, ISpatialDataConverter)
                        If (conv.IsCompatible(ds)) Then
                            lConverters.Add(conv)
                        End If
                    End If
                Next
            End If

            lConverters.Sort(New cConverterComparer())
            Return lConverters.ToArray

        End Function


        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns an array of data converter templates compatible with a <see cref="ISpatialDataSet"/>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function ConverterTemplates() As ISpatialDataConverter()

            Dim lConverters As New List(Of ISpatialDataConverter)
            Dim pm As cPluginManager = Me.m_core.PluginManager

            If (pm IsNot Nothing) Then
                For Each ip As IPlugin In pm.GetPlugins(GetType(ISpatialDataConverterPlugin))
                    If (TypeOf ip Is ISpatialDataConverter) Then
                        lConverters.Add(DirectCast(ip, ISpatialDataConverter))
                    End If
                Next
            End If

            lConverters.Sort(New cConverterComparer())
            Return lConverters.ToArray

        End Function

#End Region ' Converters

#Region " Dataset manager callback "

        Friend Sub OnDatasetRemoved(ds As ISpatialDataSet)

            If (ds Is Nothing) Then Return

            Dim bConnectionsRemoved As Boolean = False

            For Each adt As cSpatialDataAdapter In Me.Adapters
                For i As Integer = 1 To adt.MaxLength
                    Dim connections As cSpatialDataConnection() = adt.Connections(i)
                    For j As Integer = 1 To cSpatialDataStructures.cMAX_CONN

                        ' Get connection
                        Dim conn As cSpatialDataConnection = Nothing

                        If (j <= connections.Length) Then
                            conn = connections(j - 1)
                        Else
                            conn = Nothing
                        End If

                        If (conn IsNot Nothing) Then
                            If (conn.Dataset IsNot Nothing) Then
                                If (conn.Dataset.GUID.Equals(ds.GUID)) Then
                                    ' ToDo: this can probably be done more elegantly
                                    adt.RemoveConnection(i, conn)
                                    ' Remember to inform the world
                                    bConnectionsRemoved = True
                                End If
                            End If
                        End If
                    Next j
                Next i
            Next adt

            If bConnectionsRemoved Then
                ' Commit changes to underlying data structures
                Me.Update()
                ' Explicitly let the core know that data has changed and that the datasource is now dirty
                Me.NotifyCore(eMessageType.DataModified, True)
            End If

        End Sub

#End Region ' Dataset manager callback

#Region " ICoreInterface implementation "

        Public ReadOnly Property CoreComponent As EwEUtils.Core.eCoreComponentType Implements ICoreInterface.CoreComponent
            Get
                Return eCoreComponentType.EcoSpace
            End Get
        End Property

        Public ReadOnly Property DataType As EwEUtils.Core.eDataTypes Implements ICoreInterface.DataType
            Get
                Return eDataTypes.EcospaceSpatialDataConnection
            End Get
        End Property

        Public Property DBID As Integer Implements ICoreInterface.DBID
            Get
                Return -1
            End Get
            Set(value As Integer)
                ' NOP
            End Set
        End Property

        Public Function GetID() As String Implements ICoreInterface.GetID
            Return ""
        End Function

        Public Property Index As Integer Implements ICoreInterface.Index
            Get
                Return -1
            End Get
            Set(value As Integer)
                ' NOP
            End Set
        End Property

        Public Property Name As String Implements ICoreInterface.Name
            Get
                Return "SpatialDataConnectionManager"
            End Get
            Set(value As String)
                ' NOP
            End Set
        End Property

#End Region ' ICoreInterface implementation

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create fixed data adapters for ecospace data layers.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub CreateAdapters()

            Me.Clear()

            Me.AddAdapter(New cRelPPDataAdapter(Me.m_core, eVarNameFlags.LayerRelPP, eCoreCounterTypes.NotSet))
            Me.AddAdapter(New cSpatialScalarDataAdapter(Me.m_core, eVarNameFlags.LayerRelCin, eCoreCounterTypes.NotSet))
            Me.AddAdapter(New cCapacityDataAdapter(Me.m_core, eVarNameFlags.LayerHabitatCapacityInput, eCoreCounterTypes.nGroups))
            Me.AddAdapter(New cCapacityDataAdapter(Me.m_core, eVarNameFlags.LayerDriver, eCoreCounterTypes.nEnvironmentalDriverLayers))
            Me.AddAdapter(New cBiomassForcingAdapter(Me.m_core, eVarNameFlags.LayerBiomassForcing, eCoreCounterTypes.nGroups))
            Me.AddAdapter(New cBiomassRelativeAdapter(Me.m_core, eVarNameFlags.LayerBiomassRelativeForcing, eCoreCounterTypes.nGroups))
            Me.AddAdapter(New cSpatialDataAdapter(Me.m_core, eVarNameFlags.LayerHabitat, eCoreCounterTypes.nHabitats))
            Me.AddAdapter(New cSpatialDataAdapter(Me.m_core, eVarNameFlags.LayerMPA, eCoreCounterTypes.nMPAs))
            Me.AddAdapter(New cAdvectionAdapter(Me.m_core, eVarNameFlags.LayerAdvection, eCoreCounterTypes.nVectorFields))
            Me.AddAdapter(New cSpatialDataAdapter(Me.m_core, eVarNameFlags.LayerSail, eCoreCounterTypes.nFleets))

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clear fixed data adapters.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub Clear()
            Me.m_data.DataAdapters.Clear()
        End Sub

#End Region ' Internals

    End Class

End Namespace
