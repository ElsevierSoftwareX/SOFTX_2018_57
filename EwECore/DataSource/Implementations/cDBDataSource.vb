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

Imports System.Text
Imports EwECore.Auxiliary
Imports EwECore.MSE
Imports EwECore.SpatialData
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace DataSources

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="IEwEDataSource">EwE data source</see> implementation for reading
    ''' and writing Ecopath, Ecosim and Ecospace data from a database.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    <CLSCompliant(False)>
    Public Class cDBDataSource
        Implements IEwEDataSource
        Implements IEcopathDataSource
        Implements IEcosimDatasource
        Implements IEcospaceDatasource
        Implements IEcotracerDatasource
        Implements IEwEDatasourceMetadata
        Implements IEcopathSampleDataSource

#Region " Internal definitions "

        ''' <summary>Core components stored with Ecopath.</summary>
        Private Shared s_EcopathComponents() As eCoreComponentType = {eCoreComponentType.Core, eCoreComponentType.DataSource, eCoreComponentType.EcoPath, eCoreComponentType.EcopathSample}
        ''' <summary>Core components stored with Ecosim.</summary>
        Private Shared s_EcosimComponents() As eCoreComponentType = {eCoreComponentType.EcoSim, eCoreComponentType.ShapesManager, eCoreComponentType.TimeSeries,
                                                                     eCoreComponentType.EcoSimFitToTimeSeries, eCoreComponentType.EcoSimMonteCarlo,
                                                                     eCoreComponentType.MediatedInteractionManager, eCoreComponentType.FishingPolicySearch,
                                                                     eCoreComponentType.MSE, eCoreComponentType.SearchObjective, eCoreComponentType.EcosimResponseInteractionManager}
        ''' <summary>Core components stored with Ecospace.</summary>
        Private Shared s_EcospaceComponents() As eCoreComponentType = {eCoreComponentType.EcoSpace, eCoreComponentType.MPAOptimization, eCoreComponentType.EcospaceResponseInteractionManager}
        ''' <summary>Core components stored with Ecotracer.</summary>
        Private Shared s_EcotracerComponents() As eCoreComponentType = {eCoreComponentType.Ecotracer}

#End Region ' Internal definitions

#Region " Private vars "

        ''' <summary>The <see cref="cEwEDatabase">Database</see> connected to this data source.</summary>
        Private m_db As cEwEDatabase = Nothing
        ''' <summary>The <see cref="cCore">core</see> connected to this data source.</summary>
        Private m_core As cCore = Nothing
        ''' <summary>Datasource name</summary>
        Private m_strName As String = ""

#End Region ' Private vars

#Region " Generic "

        Public Sub New(ByVal db As cEwEDatabase)

            ' Pre
            Debug.Assert(db IsNot Nothing)
            ' Store ref to DB
            Me.m_db = db

        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            If Me.IsOpen Then Me.Close()
            Me.m_db = Nothing
            GC.SuppressFinalize(Me)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the local OS supports connecting to a data source
        ''' of a given type.
        ''' </summary>
        ''' <param name="dst"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function IsOSSupported(ByVal dst As eDataSourceTypes) As Boolean _
            Implements DataSources.IEwEDataSource.IsOSSupported
            Return Me.m_db.CanConnect(dst)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Open an existing DB.
        ''' </summary>
        ''' <param name="strName">Name of the DB database to open.</param>
        ''' <param name="core"><see cref="cCore">Core instance</see> that holds the 
        ''' data structures to read to, and write from.</param>
        ''' <param name="datasourceType">Type of database to open; specify
        ''' <see cref="eDataSourceTypes.NotSet"/> to automatically determine the
        ''' type of database.</param>
        ''' <param name="bReadOnly">Flag stating whether a data source should be
        ''' opened as read-only.</param>
        ''' <returns>True if opened successfully.</returns>
        ''' -------------------------------------------------------------------
        Public Function Open(ByVal strName As String,
                             ByVal core As cCore,
                             Optional ByVal datasourceType As eDataSourceTypes = eDataSourceTypes.NotSet,
                             Optional ByVal bReadOnly As Boolean = False) As eDatasourceAccessType _
                             Implements DataSources.IEwEDataSource.Open

            ' Attempt to open existing
            Dim atResult As eDatasourceAccessType = Me.m_db.Open(strName, datasourceType, bReadOnly)
            ' Any luck?
            If atResult = eDatasourceAccessType.Success Then
                ' Store core
                Me.m_core = core
                Me.m_strName = strName
            End If
            ' Report success
            Return atResult

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether a data source is already open.
        ''' </summary>
        ''' <returns>True if the data source is open.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsOpen() As Boolean _
                 Implements IEwEDataSource.IsOpen
            Return Me.m_db.IsConnected
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new DB, overwriting an existing file.
        ''' </summary>
        ''' <param name="strName">Name of the data source to create.</param>
        ''' <param name="strModelName">Name to assign to the model.</param>
        ''' <param name="core"><see cref="cCore">Core instance</see> that holds the 
        ''' data structures to read to, and write from.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Create(ByVal strName As String, ByVal strModelName As String, ByVal core As cCore) As eDatasourceAccessType _
                 Implements DataSources.IEwEDataSource.Create

            ' Create new db
            Dim atResult As eDatasourceAccessType = Me.m_db.Create(strName, strModelName, True)

            If atResult = eDatasourceAccessType.Success Then
                atResult = Me.Open(strName, core)
            End If

            Return atResult

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Close the DB.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Close() As Boolean _
                Implements IEwEDataSource.Close

            ' Clear changed admin
            Me.ClearChanged()
            ' Close current db
            Me.m_db.Close()
            ' Forget identity
            Me.m_strName = ""
            Me.m_core = Nothing
            ' Clear version
            Me.m_sVersion = cDATABASE_NOVERSION

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the data source is connected.
        ''' </summary>
        ''' <returns>True if connected.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsConnected() As Boolean
            Return Me.m_db.IsConnected()
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the connection to the <see cref="cEwEDatabase">database</see>
        ''' that this data source operates on.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Connection() As Object _
                Implements DataSources.IEwEDataSource.Connection
            Get
                Return Me.m_db
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a string representation of the data source.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Function ToString() As String _
                Implements IEwEDataSource.ToString
            If Me.m_db Is Nothing Then Return ""
            Return Me.m_db.Name
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IEwEDataSource.Directory"/>
        ''' -------------------------------------------------------------------
        Public Function Directory() As String _
            Implements IEwEDataSource.Directory
            Return Me.m_db.Directory
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IEwEDataSource.FileName"/>
        ''' -------------------------------------------------------------------
        Public Function FileName() As String _
            Implements IEwEDataSource.FileName
            Return Me.m_db.FileName
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IEwEDataSource.Extension"/>
        ''' -------------------------------------------------------------------
        Public Function Extension() As String _
            Implements IEwEDataSource.Extension
            Return Me.m_db.Extension
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Switch an open data source to a new database of the same type.
        ''' </summary>
        ''' <param name="strFileName">New FN to copy the DB to</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>This will open the new database if successful.</remarks>
        ''' -------------------------------------------------------------------
        Public Function SaveAs(ByVal strFileName As String, ByVal strModelName As String) As eDatasourceAccessType
            Return Me.m_db.SaveAs(strFileName, strModelName, True)
        End Function

        ''' <summary>Unknown version.</summary>
        Public Const cDATABASE_NOVERSION As Single = -1.0!
        ''' <summary>Database version number.</summary>
        Private m_sVersion As Single = cDATABASE_NOVERSION

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the version of the data source.
        ''' </summary>
        ''' <returns>A version number, or <see cref="cDATABASE_NOVERSION">cDATABASE_NOVERSION</see> 
        ''' if the database is not connected.</returns>
        ''' -------------------------------------------------------------------
        Public Function Version() As Single Implements IEwEDataSource.Version
            If (Me.IsConnected = True) Then
                If (Me.m_sVersion = -1.0!) Then
                    Me.m_sVersion = Me.m_db.GetVersion()
                End If
                Return Me.m_sVersion
            End If
            Return cDATABASE_NOVERSION
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="IEwEDataSource.BeginTransaction" />
        ''' -------------------------------------------------------------------
        Public Function BeginTransaction() As Boolean _
            Implements DataSources.IEwEDataSource.BeginTransaction
            Return Me.m_db.BeginTransaction()
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="IEwEDataSource.EndTransaction" />
        ''' -------------------------------------------------------------------
        Public Function EndTransaction(ByVal bCommit As Boolean) As Boolean _
            Implements DataSources.IEwEDataSource.EndTransaction
            If bCommit Then
                Return Me.m_db.CommitTransaction()
            Else
                Return Me.m_db.RollbackTransaction
            End If
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="IEwEDataSource.IsReadOnly" />
        ''' -------------------------------------------------------------------
        Public Function IsReadOnly() As Boolean _
            Implements IEwEDataSource.IsReadOnly
            Return Me.m_db.IsReadOnly()
        End Function

#Region " Helper methods "

        ''' <inheritdoc cref="IEcopathDataSource.CopyTo" />
        ''' <remarks>Method not implemented.</remarks>
        Private Overloads Function CopyEcopathTo(ByVal ds As DataSources.IEcopathDataSource) As Boolean _
            Implements DataSources.IEcopathDataSource.CopyTo
            Return False
        End Function

        ''' <inheritdoc cref="IEcosimDatasource.CopyTo" />
        ''' <remarks>Method not implemented.</remarks>
        Private Overloads Function CopyEcosimTo(ByVal ds As DataSources.IEcosimDatasource) As Boolean _
            Implements DataSources.IEcosimDatasource.CopyTo
            Return False
        End Function

        ''' <inheritdoc cref="IEcospaceDatasource.CopyTo" />
        ''' <remarks>Method not implemented.</remarks>
        Private Overloads Function CopyEcospaceTo(ByVal ds As DataSources.IEcospaceDatasource) As Boolean _
            Implements DataSources.IEcospaceDatasource.CopyTo
            Return False
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <para>Helper method, splits a string of numbers into an array of strings,
        ''' each string representing a number. This method assumes that numbers are
        ''' separated by a ASCII character 32, a single space.</para>
        ''' </summary>
        ''' <param name="strNumberString">A comma-seoarated string of numbers to split.</param>
        ''' <returns>
        ''' An array of strings, each representing a number in the string.
        ''' </returns>
        ''' <remarks>
        ''' <para>This method tries to resolve number formatting issues, introduced
        ''' in models written by systems with different locale settings.</para>
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Function SplitNumberString(ByRef strNumberString As String) As String()
            Dim charSeparators() As Char = {" "c}
            If strNumberString.IndexOf(CChar(",")) > -1 Then strNumberString = strNumberString.Replace(CChar(","), CChar("."))
            Return strNumberString.Trim().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries)
        End Function

#End Region ' Helper methods

#End Region ' Generic

#Region " Change management "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the data source has unsaved changes that do not relate
        ''' to any of the supported sub-models.
        ''' </summary>
        ''' <returns>True if the data source has pending changes.</returns>
        ''' -------------------------------------------------------------------
        Friend Function IsChanged() As Boolean Implements DataSources.IEwEDataSource.IsModified
            If Not Me.IsConnected() Then Return False
            Return Me.IsChanged(Nothing)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clears all changed information for either a given data type or for 
        ''' the entire data source.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Friend Sub ClearChanged() Implements IEwEDataSource.ClearChanged
            Me.ClearChanged(Nothing)
        End Sub

        ''' <summary>Dictionary of changed core components.</summary>
        Private m_dictChangedComponents As New Dictionary(Of eCoreComponentType, Boolean)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Flag a core object as changed in the data source.
        ''' </summary>
        ''' <param name="cc">The <see cref="eDataTypes">Type</see> of the object that changed.</param>
        ''' -------------------------------------------------------------------
        Public Sub SetChanged(ByVal cc As eCoreComponentType) _
                Implements IEwEDataSource.SetChanged
            ' Ignore invalid set commands. Could be due to sloppy usage
            If (cc = eCoreComponentType.NotSet) Then Return
            ' Ignore external dirtying
            If (cc = eCoreComponentType.External) Then Return
            Me.m_dictChangedComponents.Item(cc) = True
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; states whether there are pending changes for a particular
        ''' <see cref="eCoreComponentType">EwE component</see>.
        ''' </summary>
        ''' <param name="acomponents">The EwE components to check.</param>
        ''' <returns>True if there are any pending changes for any datatype that
        ''' belongs to this EwE component.</returns>
        ''' -------------------------------------------------------------------
        Private Function IsChanged(ByVal acomponents As eCoreComponentType()) As Boolean
            Dim bIsChanged As Boolean = False
            If (acomponents Is Nothing) Then
                Return (Me.m_dictChangedComponents.Count > 0)
            Else
                For Each component As eCoreComponentType In acomponents
                    bIsChanged = bIsChanged Or Me.m_dictChangedComponents.ContainsKey(component)
                Next
            End If
            Return bIsChanged
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clears the changed administration for all datatypes that belong to
        ''' a given <see cref="eCoreComponentType">EwE component</see>.
        ''' </summary>
        ''' <param name="acomponents">The EwE components to clear the changed
        ''' adminsitration for.</param>
        ''' -------------------------------------------------------------------
        Private Sub ClearChanged(ByVal acomponents As eCoreComponentType())

            If (acomponents Is Nothing) Then
                Me.m_dictChangedComponents.Clear()
            Else
                For Each component As eCoreComponentType In acomponents
                    If Me.m_dictChangedComponents.ContainsKey(component) Then
                        Me.m_dictChangedComponents.Remove(component)
                    End If
                Next component
            End If
        End Sub

#End Region ' Change management

#Region " Private helper bits "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper class, maintains a list of database ID mappings per datatype. Use this class
        ''' when duplicating objects in the database. Via the mappings, newly created objects
        ''' (with new DBID values) can be saved using content of their originals (with old
        ''' DBID values)
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cIDMappings

            ''' <summary>Array of ID mappings, per datatype.</summary>
            Private m_dictMappings() As Dictionary(Of Integer, Integer)

            Public Sub New()
                Me.Initialize()
            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Initialize the ID mapper by allocating space for the lookup tables.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Private Sub Initialize()
                ' Allocate space
                Dim nNumDatatypes As Integer = System.Enum.GetValues(GetType(eDataTypes)).Length
                ReDim Me.m_dictMappings(nNumDatatypes)
                For i As Integer = 0 To nNumDatatypes
                    Me.m_dictMappings(i) = New Dictionary(Of Integer, Integer)
                Next
            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Add an ID mapping for a specific object.
            ''' </summary>
            ''' <param name="dt">The <see cref="eDataTypes">Data Type</see> of the object to map.</param>
            ''' <param name="iIDOrg">The original database ID of the object. This is the value
            ''' under which the object is stored in the current database, and how it is currently
            ''' known in the core database ID arrays.</param>
            ''' <param name="iIDNew">The mapped database ID of the object. This is the value that
            ''' has been assigned by the data source when creating a new instance of the object
            ''' in the database.</param>
            ''' -----------------------------------------------------------------------
            Public Sub Add(ByVal dt As eDataTypes, ByVal iIDOrg As Integer, ByVal iIDNew As Integer)
                ' Only add useful mappings, please!
                If iIDOrg = iIDNew Then Return

                Try
                    Dim d As Dictionary(Of Integer, Integer) = Me.m_dictMappings(CInt(dt))

                    ' Development-time sanity checks.
                    Debug.Assert(d IsNot Nothing, String.Format("cIDMappings.Add: no dictionary for datatype {0} ({1}), something is very wrong!", dt.ToString, CInt(dt)))
                    Debug.Assert(Not d.ContainsKey(iIDOrg), String.Format("cIDMappings: DBID {0} is already used to define a mapping", iIDOrg))
                    Debug.Assert(Not d.ContainsValue(iIDNew), String.Format("cIDMappings: DBID {0} already mapped to", iIDNew))

                    d.Add(iIDOrg, iIDNew)

                Catch ex As Exception
                    ' Development-time panic event.
                    Debug.Assert(False, String.Format("cIDMappings.Add: ID Mapping failed '{0}'", ex.Message))
                End Try
            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Returns a mapped ID for a specific core object. If no mapping exists, the
            ''' original ID is returned.
            ''' </summary>
            ''' <param name="dt">The <see cref="eDataTypes">Data Type</see> of the object
            ''' to retrieve the mapping for.</param>
            ''' <param name="iIDOrg">The original database ID of the object.</param>
            ''' <returns>A mapped ID if present, or the original ID if no mapping was found.</returns>
            ''' -----------------------------------------------------------------------
            Public Function GetID(ByVal dt As eDataTypes, ByVal iIDOrg As Integer) As Integer
                Try
                    Dim d As Dictionary(Of Integer, Integer) = Me.m_dictMappings(CInt(dt))
                    If d.ContainsKey(iIDOrg) Then
                        Return d.Item(iIDOrg)
                    End If
                Catch ex As Exception
                    ' Woops
                End Try
                Return iIDOrg
            End Function

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Returns whether a mapping exists for a core object.
            ''' </summary>
            ''' <param name="dt">The <see cref="eDataTypes">Data Type</see> of the object to
            ''' test a mapping for.</param>
            ''' <param name="iIDOrg">The original database ID of the object to test.</param>
            ''' <returns>True if a mapping exists.</returns>
            ''' -----------------------------------------------------------------------
            Public Function HasMapping(ByVal dt As eDataTypes, ByVal iIDOrg As Integer) As Boolean
                Dim d As Dictionary(Of Integer, Integer) = Me.m_dictMappings(CInt(dt))
                Return d.ContainsKey(iIDOrg)
            End Function

        End Class

#End Region ' Private helper bits

#Region " Messages "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Logs a message to the application log.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub LogMessage(ByVal strMessage As String,
                Optional ByVal msgType As eMessageType = eMessageType.DataModified,
                Optional ByVal msgImportance As eMessageImportance = eMessageImportance.Information)

            If (Me.m_core IsNot Nothing) Then
                Me.m_core.m_publisher.AddMessage(New cMessage(strMessage, msgType, eCoreComponentType.DataSource, msgImportance))
            End If
            'Console.WriteLine(strMessage)

        End Sub

#End Region ' Messages

#Region " Generic data source "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Compact the data in the data source. Please ensure that this operation
        ''' is possible via <see cref="CanCompact">CanCompact</see>.
        ''' </summary>
        ''' <param name="strTarget">The destination to compact the data source to.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Public Function Compact(ByVal strTarget As String) As eDatasourceAccessType _
            Implements DataSources.IEwEDataSource.Compact
            Return Me.m_db.Compact(strTarget, strTarget)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the data underlying the data source can be compacted.
        ''' </summary>
        ''' <param name="strTarget">The destination to test whether the data source 
        ''' can compact to.</param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Function CanCompact(ByVal strTarget As String) As Boolean _
            Implements IEwEDataSource.CanCompact
            Return Me.m_db.CanCompact(strTarget, strTarget)
        End Function

#End Region ' Generic data source

#Region " EwEModel "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initiates a full load of an EwE model.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function LoadModel() As Boolean _
             Implements IEcopathDataSource.LoadModel

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim bSucces As Boolean = True

            ' Clear changed admin
            Me.ClearChanged()

            bSucces = Me.LoadModelInfo()
            If bSucces = False Then Return False

            bSucces = bSucces And Me.LoadEcopathGroups()
            bSucces = bSucces And Me.LoadEcopathTaxon()
            bSucces = bSucces And Me.LoadEcopathFleetInfo()
            bSucces = bSucces And Me.LoadParticleSizeDistribution()
            bSucces = bSucces And Me.LoadPedigreeLevels()
            bSucces = bSucces And Me.LoadPedigreeAssignments()
            bSucces = bSucces And Me.LoadEcopathSamples()
            bSucces = bSucces And Me.LoadAuxillaryData()

            ecopathDS.bInitialized = bSucces
            ecopathDS.onPostInitialization()

            bSucces = bSucces And Me.LoadEcosimScenarioDefinitions()
            bSucces = bSucces And Me.LoadEcospaceScenarioDefinitions()
            bSucces = bSucces And Me.LoadEcotracerScenarioDefinitions()
            bSucces = bSucces And Me.LoadTimeSeriesDatasets()

            ' Clear changed admin
            Me.ClearChanged(s_EcopathComponents)

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initiates a save of an EwE model
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function SaveModel() As Boolean _
                 Implements IEcopathDataSource.SaveModel

            Dim bSucces As Boolean = Me.m_db.BeginTransaction()

            ' Start saving
            bSucces = Me.SaveModelInfo()
            bSucces = bSucces And Me.SaveEcopathGroups()
            bSucces = bSucces And Me.SaveEcopathTaxon()
            bSucces = bSucces And Me.SaveEcopathFleetInfo()
            bSucces = bSucces And Me.SaveParticleSizeDistribution()
            bSucces = bSucces And Me.SaveEcosimScenarioDefinitions()
            bSucces = bSucces And Me.SaveEcospaceScenarioDefinitions()
            bSucces = bSucces And Me.SaveEcotracerScenarioDefinitions()
            bSucces = bSucces And Me.SavePedigreeLevels()
            bSucces = bSucces And Me.SavePedigreeAssignments()
            bSucces = bSucces And Me.SaveAuxillaryData()
            bSucces = bSucces And Me.SaveEcopathSamples()

            If bSucces Then
                bSucces = Me.m_db.CommitTransaction()
            Else
                Me.m_db.RollbackTransaction()
            End If

            ' Save successful?
            If bSucces Then
                ' #Yes: Clear ecopath changed flags
                Me.ClearChanged(s_EcopathComponents)
            End If

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, loads model info for the current model.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function LoadModelInfo() As Boolean

            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathModel")
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim sVal1 As Single = 0.0!
            Dim sVal2 As Single = 0.0!
            Dim bSucces As Boolean = True

            ' Crash prevention check
            If reader Is Nothing Then
                'Debug.Assert(False, "Failed to access table EcopathModel")
                Return False
            End If

            Try
                ' There is only one model in an EwE6 database
                reader.Read()

                ecopathDS.ModelDBID = CInt(reader("ModelID"))
                ecopathDS.ModelName = CStr(reader("Name"))
                ecopathDS.ModelDescription = CStr(reader("Description"))
                ecopathDS.ModelAuthor = CStr(Me.m_db.ReadSafe(reader, "Author", ""))
                ecopathDS.ModelContact = CStr(Me.m_db.ReadSafe(reader, "Contact", ""))
                ecopathDS.ModelArea = CSng(Me.m_db.ReadSafe(reader, "Area", 1.0))
                ecopathDS.ModelNumDigits = CInt(reader("NumDigits"))
                ecopathDS.ModelGroupDigits = (CInt(Me.m_db.ReadSafe(reader, "GroupDigits", False)) <> 0)
                ecopathDS.ModelUnitCurrency = DirectCast(Me.m_db.ReadSafe(reader, "UnitCurrency", eUnitCurrencyType.WetWeight), eUnitCurrencyType)
                ecopathDS.ModelUnitCurrencyCustom = CStr(Me.m_db.ReadSafe(reader, "UnitCurrencyCustom", ""))
                ecopathDS.ModelUnitTime = DirectCast(Me.m_db.ReadSafe(reader, "UnitTime", eUnitTimeType.Year), eUnitTimeType)
                ecopathDS.ModelUnitTimeCustom = CStr(Me.m_db.ReadSafe(reader, "UnitTimeCustom", ""))
                ecopathDS.ModelUnitMonetary = DirectCast(Me.m_db.ReadSafe(reader, "UnitMonetary", "EUR"), String)
                ecopathDS.FirstYear = CInt(Me.m_db.ReadSafe(reader, "FirstYear", 0))
                ecopathDS.NumYears = CInt(Me.m_db.ReadSafe(reader, "NumYears", 1))
                ecopathDS.ModelCountry = CStr(Me.m_db.ReadSafe(reader, "Country", ""))
                ecopathDS.ModelEcosystemType = CStr(Me.m_db.ReadSafe(reader, "EcosystemType", ""))
                ecopathDS.ModelEcobaseCode = CStr(Me.m_db.ReadSafe(reader, "CodeEcobase", ""))
                ecopathDS.ModelPublicationDOI = CStr(Me.m_db.ReadSafe(reader, "PublicationDOI", ""))
                ecopathDS.ModelPublicationURI = CStr(Me.m_db.ReadSafe(reader, "PublicationURI", ""))
                ecopathDS.ModelPublicationRef = CStr(Me.m_db.ReadSafe(reader, "PublicationRef", ""))

                Dim sLat1 As Single = CSng(Me.m_db.ReadSafe(reader, "MaxLat", 0))
                Dim sLat2 As Single = CSng(Me.m_db.ReadSafe(reader, "MinLat", 0))
                ecopathDS.ModelNorth = Math.Max(sLat1, sLat2)
                ecopathDS.ModelSouth = Math.Min(sLat1, sLat2)

                ecopathDS.ModelWest = CSng(Me.m_db.ReadSafe(reader, "MinLon", 0))
                ecopathDS.ModelEast = CSng(Me.m_db.ReadSafe(reader, "MaxLon", 0))

                ecopathDS.ModelLastSaved = CDbl(Me.m_db.ReadSafe(reader, "LastSaved", 0))

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading EcopathModel", ex.Message))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)
            reader = Nothing

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Updates model info into the database.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function SaveModelInfo() As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim bSucces As Boolean = True

            Try
                ' This will no longer work because of tables linking to ModelID
                'Me.m_db.Execute("DELETE * FROM EcopathModel")
                writer = Me.m_db.GetWriter("EcopathModel")
                dt = writer.GetDataTable()

                drow = dt.Rows.Find(ecopathDS.ModelDBID)

                bNewRow = (drow Is Nothing)
                If bNewRow Then
                    drow = writer.NewRow()
                Else
                    drow.BeginEdit()
                End If

                drow("Name") = ecopathDS.ModelName
                drow("Description") = ecopathDS.ModelDescription
                drow("Author") = ecopathDS.ModelAuthor
                drow("Contact") = ecopathDS.ModelContact
                drow("Area") = ecopathDS.ModelArea
                drow("NumDigits") = ecopathDS.ModelNumDigits
                drow("GroupDigits") = ecopathDS.ModelGroupDigits
                drow("UnitCurrency") = ecopathDS.ModelUnitCurrency
                drow("UnitCurrencyCustom") = ecopathDS.ModelUnitCurrencyCustom
                drow("UnitTime") = ecopathDS.ModelUnitTime
                drow("UnitTimeCustom") = ecopathDS.ModelUnitTimeCustom
                drow("UnitMonetary") = ecopathDS.ModelUnitMonetary
                'drow("UnitArea") = ecopathDS.ModelUnitArea
                'drow("UnitAreaCustom") = ecopathDS.ModelUnitAreaCustom
                drow("FirstYear") = ecopathDS.FirstYear
                drow("NumYears") = ecopathDS.NumYears
                drow("MinLat") = ecopathDS.ModelSouth
                drow("MaxLat") = ecopathDS.ModelNorth
                drow("MinLon") = ecopathDS.ModelWest
                drow("MaxLon") = ecopathDS.ModelEast
                drow("Country") = ecopathDS.ModelCountry
                drow("EcosystemType") = ecopathDS.ModelEcosystemType
                drow("CodeEcobase") = ecopathDS.ModelEcobaseCode
                drow("PublicationDOI") = ecopathDS.ModelPublicationDOI
                drow("PublicationURI") = ecopathDS.ModelPublicationURI
                drow("PublicationRef") = ecopathDS.ModelPublicationRef

                ' ------------------------------------------
                drow("LastSaved") = cDateUtils.DateToJulian()
                drow("LastSavedVersion") = cAssemblyUtils.GetVersion().ToString

                If bNewRow Then
                    writer.AddRow(drow)
                Else
                    drow.EndEdit()
                End If

                writer.Commit()

            Catch ex As Exception
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, True)

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Load the list of available Ecosim scenarios.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will NOT load any actual Ecosim scenario. Scenario definitions 
        ''' merely provide a preview of available Ecosim scenarios in the database.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Function LoadEcosimScenarioDefinitions() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcosimScenario ORDER BY ScenarioID ASC")
            Dim iScenario As Integer = 1
            Dim bSucces As Boolean = True

            ecopathDS.NumEcosimScenarios = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM EcoSimScenario", 0))
            ecopathDS.RedimEcosimScenarios()

            If ecopathDS.NumEcosimScenarios = 0 Then Return bSucces

            Try
                While reader.Read()
                    ecopathDS.EcosimScenarioDBID(iScenario) = CInt(reader("ScenarioID"))
                    ecopathDS.EcosimScenarioName(iScenario) = CStr(reader("ScenarioName"))
                    ecopathDS.EcosimScenarioDescription(iScenario) = CStr(reader("Description"))
                    ecopathDS.EcosimScenarioAuthor(iScenario) = CStr(Me.m_db.ReadSafe(reader, "Author", ""))
                    ecopathDS.EcosimScenarioContact(iScenario) = CStr(Me.m_db.ReadSafe(reader, "Contact", ""))
                    ecopathDS.EcosimScenarioLastSaved(iScenario) = CDbl(Me.m_db.ReadSafe(reader, "LastSaved", 0))
                    iScenario += 1
                End While
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading ecosim scenario definition {1}", ex.Message, iScenario))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)
            reader = Nothing

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Saves the list of available Ecosim scenarios.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will NOT save any actual Ecosim scenario. Here, only the
        ''' Ecosim scenario preview information is updated.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Function SaveEcosimScenarioDefinitions() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenario As Integer = 0
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("EcosimScenario")
                dt = writer.GetDataTable()

                For iScenario = 1 To ecopathDS.NumEcosimScenarios

                    drow = dt.Rows.Find(ecopathDS.EcosimScenarioDBID(iScenario))
                    Debug.Assert(drow IsNot Nothing, String.Format("Cannot find existing row for ecosim scenario ID {0}", ecopathDS.EcosimScenarioDBID(iScenario)))

                    drow.BeginEdit()
                    drow("ScenarioName") = ecopathDS.EcosimScenarioName(iScenario)
                    drow("Description") = ecopathDS.EcosimScenarioDescription(iScenario)
                    drow("Author") = ecopathDS.EcosimScenarioAuthor(iScenario)
                    drow("Contact") = ecopathDS.EcosimScenarioContact(iScenario)
                    drow("LastSaved") = ecopathDS.EcosimScenarioLastSaved(iScenario)
                    drow.EndEdit()

                Next

            Catch ex As Exception
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, True)

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Load the list of available Ecospace scenarios.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will NOT load any actual Ecospace scenario. Scenario definitions 
        ''' merely provide a preview of available Ecospace scenarios in the database.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Function LoadEcospaceScenarioDefinitions() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcospaceScenario ORDER BY ScenarioID ASC")
            Dim iScenario As Integer = 1
            Dim bSucces As Boolean = True

            ecopathDS.NumEcospaceScenarios = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM EcospaceScenario", 0))
            ecopathDS.RedimEcospaceScenarios()

            If ecopathDS.NumEcospaceScenarios = 0 Then Return bSucces

            Try
                While reader.Read()
                    ecopathDS.EcospaceScenarioDBID(iScenario) = CInt(reader("ScenarioID"))
                    ecopathDS.EcospaceScenarioName(iScenario) = CStr(reader("ScenarioName"))
                    ecopathDS.EcospaceScenarioDescription(iScenario) = CStr(reader("Description"))
                    ecopathDS.EcospaceScenarioAuthor(iScenario) = CStr(Me.m_db.ReadSafe(reader, "Author", ""))
                    ecopathDS.EcospaceScenarioContact(iScenario) = CStr(Me.m_db.ReadSafe(reader, "Contact", ""))
                    ecopathDS.EcospaceScenarioLastSaved(iScenario) = CDbl(Me.m_db.ReadSafe(reader, "LastSaved", 0))
                    iScenario += 1
                End While
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading ecospace scenario definition {1}", ex.Message, iScenario))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Saves the list of available Ecospace scenarios.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will NOT save any actual Ecospace scenario. Here, only the
        ''' Ecospace scenario preview information is updated.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Function SaveEcospaceScenarioDefinitions() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenario As Integer = 0
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("EcospaceScenario")
                dt = writer.GetDataTable()

                For iScenario = 1 To ecopathDS.NumEcospaceScenarios

                    drow = dt.Rows.Find(ecopathDS.EcospaceScenarioDBID(iScenario))
                    Debug.Assert(drow IsNot Nothing, String.Format("Cannot find existing row for ecospace scenario ID {0}", ecopathDS.EcospaceScenarioDBID(iScenario)))

                    drow.BeginEdit()
                    drow("ScenarioName") = ecopathDS.EcospaceScenarioName(iScenario)
                    drow("Description") = ecopathDS.EcospaceScenarioDescription(iScenario)
                    drow("Author") = ecopathDS.EcospaceScenarioAuthor(iScenario)
                    drow("Contact") = ecopathDS.EcospaceScenarioContact(iScenario)
                    drow("LastSaved") = ecopathDS.EcospaceScenarioLastSaved(iScenario)
                    drow.EndEdit()

                Next

            Catch ex As Exception
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, True)

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Load the list of available Ecotracer scenarios.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will NOT load any actual Ecotracer scenario. Scenario definitions 
        ''' merely provide a preview of available Ecotracer scenarios in the database.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Function LoadEcotracerScenarioDefinitions() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcotracerScenario ORDER BY ScenarioID ASC")
            Dim iScenario As Integer = 1
            Dim bSucces As Boolean = True

            ecopathDS.NumEcotracerScenarios = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM EcotracerScenario", 0))
            ecopathDS.RedimEcotracerScenarios()

            If ecopathDS.NumEcotracerScenarios = 0 Then Return bSucces

            Try
                While reader.Read()
                    ecopathDS.EcotracerScenarioDBID(iScenario) = CInt(reader("ScenarioID"))
                    ecopathDS.EcotracerScenarioName(iScenario) = CStr(reader("ScenarioName"))
                    ecopathDS.EcotracerScenarioDescription(iScenario) = CStr(reader("Description"))
                    ecopathDS.EcotracerScenarioAuthor(iScenario) = CStr(Me.m_db.ReadSafe(reader, "Author", ""))
                    ecopathDS.EcotracerScenarioContact(iScenario) = CStr(Me.m_db.ReadSafe(reader, "Contact", ""))
                    ecopathDS.EcotracerScenarioLastSaved(iScenario) = CDbl(Me.m_db.ReadSafe(reader, "LastSaved", 0))
                    iScenario += 1
                End While
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading ecospace scenario definition {1}", ex.Message, iScenario))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Saves the list of available Ecotracer scenarios.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will NOT save any actual Ecotracer scenario. Here, only the
        ''' Ecotracer scenario preview information is updated.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Function SaveEcotracerScenarioDefinitions() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenario As Integer = 0
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("EcotracerScenario")
                dt = writer.GetDataTable()

                For iScenario = 1 To ecopathDS.NumEcotracerScenarios

                    drow = dt.Rows.Find(ecopathDS.EcotracerScenarioDBID(iScenario))
                    Debug.Assert(drow IsNot Nothing, String.Format("Cannot find existing row for ecotracer scenario ID {0}", ecopathDS.EcotracerScenarioDBID(iScenario)))

                    drow.BeginEdit()
                    drow("ScenarioName") = ecopathDS.EcotracerScenarioName(iScenario)
                    drow("Description") = ecopathDS.EcotracerScenarioDescription(iScenario)
                    drow("Author") = ecopathDS.EcotracerScenarioAuthor(iScenario)
                    drow("Contact") = ecopathDS.EcotracerScenarioContact(iScenario)
                    drow("LastSaved") = ecopathDS.EcotracerScenarioLastSaved(iScenario)
                    drow.EndEdit()

                Next

            Catch ex As Exception
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, True)

            Return bSucces
        End Function

#Region " Pedigree "

#Region " Load "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Load the pedigree level definitions.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function LoadPedigreeLevels() As Boolean

            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM Pedigree ORDER BY Sequence ASC")
            Dim iLevel As Integer = 1
            Dim bSucces As Boolean = True

            ' Init data structure
            ecopathDS.NumPedigreeLevels = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM Pedigree", 0))

            ' Allocate space
            ecopathDS.RedimPedigree()

            While reader.Read()

                Try
                    ecopathDS.PedigreeLevelDBID(iLevel) = CInt(reader("LevelID"))
                    ecopathDS.PedigreeLevelName(iLevel) = CStr(reader("LevelName"))
                    ecopathDS.PedigreeLevelDescription(iLevel) = CStr(reader("Description"))

                    Dim var As eVarNameFlags = cin.GetVarName(CStr(reader("VarName")))
                    ' fudge, no need to issue a database update
                    If var = eVarNameFlags.Biomass Then var = eVarNameFlags.BiomassAreaInput
                    ecopathDS.PedigreeLevelVarName(iLevel) = var

                    ecopathDS.PedigreeLevelIndexValue(iLevel) = CSng(reader("IndexValue"))
                    ecopathDS.PedigreeLevelConfidence(iLevel) = CInt(reader("Confidence"))
                    ecopathDS.PedigreeLevelColor(iLevel) = CInt(Me.m_db.ReadSafe(reader, "LevelColor", 0))

                Catch ex As Exception
                    Me.LogMessage(String.Format("Error {0} occurred while reading pedigree level {1}", ex.Message, iLevel))
                    bSucces = False
                End Try

                iLevel += 1

            End While

            ' Sanity check
            Debug.Assert(iLevel - 1 = ecopathDS.NumPedigreeLevels)

            Me.m_db.ReleaseReader(reader)
            reader = Nothing

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Load the pedigree level assignments.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function LoadPedigreeAssignments() As Boolean

            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathGroupPedigree")
            Dim iGroup As Integer
            Dim iVariable As Integer
            Dim iLevel As Integer
            Dim iConfidence As Integer = 1
            Dim bSucces As Boolean = True

            While reader.Read()

                Try
                    iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("GroupID")))
                    iVariable = Array.IndexOf(ecopathDS.PedigreeVariables, cin.GetVarName(CStr(reader("VarName"))))
                    iLevel = Array.IndexOf(ecopathDS.PedigreeLevelDBID, CInt(Me.m_db.ReadSafe(reader, "LevelID", 0)))
                    iConfidence = CInt(Me.m_db.ReadSafe(reader, "Confidence", 0))

                    If (iGroup >= 1) And (iVariable >= 1) And ((iConfidence >= 0) Or (iLevel > 0)) Then
                        ecopathDS.PedigreeEcopathGroupLevel(iGroup, iVariable) = iLevel
                        ecopathDS.PedigreeEcopathGroupCV(iGroup, iVariable) = iConfidence
                    Else
                        ' NOP... log message?
                    End If

                Catch ex As Exception
                    Me.LogMessage(String.Format("Error {0} occurred while reading pedigree assignment {1}{2}", ex.Message, iGroup, iVariable))
                    bSucces = False
                End Try

            End While

            Me.m_db.ReleaseReader(reader)
            reader = Nothing

            Return bSucces
        End Function

#End Region ' Load

#Region " Save "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Save pedigree level definitions.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Public Function SavePedigreeLevels() As Boolean

            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim iLevel As Integer = 0
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("Pedigree")
                dt = writer.GetDataTable()

                For iLevel = 1 To ecopathDS.NumPedigreeLevels

                    ' Find existing row
                    drow = dt.Rows.Find(ecopathDS.PedigreeLevelDBID(iLevel))
                    Debug.Assert(drow IsNot Nothing, String.Format("Cannot find existing row for pedigree level {0}", ecopathDS.PedigreeLevelDBID(iLevel)))

                    drow.BeginEdit()
                    drow("Sequence") = iLevel
                    drow("LevelName") = ecopathDS.PedigreeLevelName(iLevel)
                    drow("Description") = ecopathDS.PedigreeLevelDescription(iLevel)
                    drow("VarName") = CStr(cin.GetVarName(ecopathDS.PedigreeLevelVarName(iLevel)))
                    drow("IndexValue") = ecopathDS.PedigreeLevelIndexValue(iLevel)
                    drow("Confidence") = ecopathDS.PedigreeLevelConfidence(iLevel)

                    drow.EndEdit()

                Next iLevel

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving pedigree level", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Return bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

        End Function

        Public Function SavePedigreeAssignments() As Boolean

            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iGroup As Integer = 0
            Dim iVariable As Integer = 0
            Dim iLevel As Integer = 0
            Dim iConfidence As Integer = 0
            Dim bSucces As Boolean = True

            Try
                bSucces = Me.m_db.Execute("DELETE * FROM EcopathGroupPedigree")
                writer = Me.m_db.GetWriter("EcopathGroupPedigree")

                For iGroup = 1 To ecopathDS.NumGroups
                    For iVariable = 1 To ecopathDS.NumPedigreeVariables
                        iLevel = ecopathDS.PedigreeEcopathGroupLevel(iGroup, iVariable)
                        iConfidence = ecopathDS.PedigreeEcopathGroupCV(iGroup, iVariable)
                        If (iConfidence > 0) Or (iLevel > 0) Then
                            drow = writer.NewRow()
                            drow("GroupID") = ecopathDS.GroupDBID(iGroup)
                            drow("VarName") = cin.GetVarName(ecopathDS.PedigreeVariables(iVariable))
                            drow("LevelID") = ecopathDS.PedigreeLevelDBID(iLevel)
                            drow("Confidence") = iConfidence
                            writer.AddRow(drow)
                        End If
                    Next
                Next

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving pedigree assignments", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Return bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

        End Function

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IEcopathDataSource.AddPedigreeLevel"/>
        ''' -------------------------------------------------------------------
        Public Function AddPedigreeLevel(ByVal iPosition As Integer,
                                         ByVal strName As String,
                                         ByVal iColor As Integer,
                                         ByVal strDescription As String,
                                         ByVal varName As eVarNameFlags,
                                         ByVal sIndexValue As Single,
                                         ByVal sConfidence As Single,
                                         ByRef iPedigreeLevelID As Integer) As Boolean _
                Implements IEcopathDataSource.AddPedigreeLevel

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            iPedigreeLevelID = CInt(Me.m_db.GetValue("SELECT MAX(LevelID) FROM Pedigree", 0)) + 1

            Try
                ' Start writing, protect sequence
                writer = Me.m_db.GetWriter("Pedigree")

                ' Get new row to add
                drow = writer.NewRow()
                drow("LevelID") = iPedigreeLevelID
                drow("LevelName") = strName
                drow("LevelColor") = iColor
                drow("Description") = strDescription
                drow("VarName") = varName.ToString
                drow("IndexValue") = sIndexValue
                drow("Confidence") = sConfidence
                drow("Sequence") = iPosition

                ' Commit to db
                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            'bSucces = bSucces And Me.LoadPedigreeLevels()

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Move a pedigree level to a different position in the level sequence.
        ''' </summary>
        ''' <param name="iPedigreeLevelID">Database ID of the pedigree level to move.</param>
        ''' <param name="iPosition">The new position of the pedigree level in the 
        ''' level sequence.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function MovePedigreeLevel(ByVal iPedigreeLevelID As Integer,
                                          ByVal iPosition As Integer) As Boolean _
                Implements IEcopathDataSource.MovePedigreeLevel

            Dim bSucces As Boolean = True
            Try
                Me.m_db.Execute(String.Format("UPDATE Pedigree SET Sequence={1} WHERE (LevelID={0})", iPedigreeLevelID, iPosition))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while moving PedigreeLevel {1}", ex.Message, iPedigreeLevelID))
                bSucces = False
            End Try
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a pedigree level from the data source.
        ''' </summary>
        ''' <param name="iPedigreeLevelID">Database ID of the pedigree level to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function RemovePedigreeLevel(ByVal iPedigreeLevelID As Integer) As Boolean _
                  Implements IEcopathDataSource.RemovePedigreeLevel

            Dim bSucces As Boolean = True

            Try
                ' Per Jul 2018, Pedigree levels are only no longer stored in other tables
                Me.m_db.Execute(String.Format("DELETE FROM Pedigree WHERE (LevelID={0})", iPedigreeLevelID))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing PedigreeLevel {1}", ex.Message, iPedigreeLevelID))
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Modify

#End Region ' Pedigree

#Region " PSD "

#Region " Load "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load Particle Size Distribution data for Ecopath.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function LoadParticleSizeDistribution() As Boolean

            Dim psdDS As cPSDDatastructures = Me.m_core.m_PSDData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathPSD")
            Dim bSucces As Boolean = True

            If (reader IsNot Nothing) Then
                If reader.Read() Then
                    Try
                        psdDS.NAgeSteps = CInt(Me.m_db.ReadSafe(reader, "NumAgeSteps", 101))
                        psdDS.MortalityType = CType(CInt(Me.m_db.ReadSafe(reader, "MortalityType", 0)), ePSDMortalityTypes)
                        psdDS.NWeightClasses = CInt(Me.m_db.ReadSafe(reader, "NumWeightClasses", 25))
                        psdDS.FirstWeightClass = CSng(Me.m_db.ReadSafe(reader, "FirstWeightClass", 0.125))
                        psdDS.ClimateType = CType(CInt(Me.m_db.ReadSafe(reader, "ClimateType", eClimateTypes.Temperate)), eClimateTypes)
                    Catch ex As Exception
                        Me.LogMessage(String.Format("Error {0} occurred while reading EcopathPSD", ex.Message))
                        bSucces = False
                    End Try
                End If

                Me.m_db.ReleaseReader(reader)
                reader = Nothing

            End If

            Return bSucces
        End Function

#End Region ' Load

#Region " Save "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save Particle Size Distribution data for Ecopath.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function SaveParticleSizeDistribution() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim psdDS As cPSDDatastructures = Me.m_core.m_PSDData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("EcopathPSD")
                dt = writer.GetDataTable()

                ' Find existing row
                drow = dt.Rows.Find(ecopathDS.ModelDBID)
                bNewRow = (drow Is Nothing)

                If bNewRow Then
                    drow = dt.NewRow()
                    drow("ModelID") = ecopathDS.ModelDBID
                Else
                    drow.BeginEdit()
                End If

                drow("NumAgeSteps") = psdDS.NAgeSteps
                drow("MortalityType") = psdDS.MortalityType
                drow("NumWeightClasses") = psdDS.NWeightClasses
                drow("FirstWeightClass") = psdDS.FirstWeightClass
                drow("ClimateType") = psdDS.ClimateType

                If bNewRow Then
                    writer.AddRow(drow)
                Else
                    drow.EndEdit()
                End If

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving PSD", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, True)

            Return bSucces
        End Function

#End Region ' Save

#End Region ' PSD

#End Region ' EwEModel

#Region " Stanza "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Load all model-generic stanza information.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function LoadStanza() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim rdStanza As IDataReader = Nothing
            Dim rdLifeStage As IDataReader = Nothing
            Dim iStanza As Integer = 0
            Dim iLifeStage As Integer = 0
            Dim iGroup As Integer = 0
            Dim sTemp As Single = 0.0
            Dim bSucces As Boolean = True

            ' Count the number of rows in StanzaInfo; this is the number of split groups that we're going to work with
            stanzaDS.Nsplit = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM Stanza", 0))
            ' Get max no of stanza
            stanzaDS.MaxStanza = 0

            ' First read Stanza
            ' JS 05Nov11: SQL Server does not allow readers for StanzaLifeStage to be opened after the master table Stanza has been opened
            '             This is unfortunate and will require some serious refactoring throughout this class. Basically, child table readers
            '             will need to be opened before master tables, or readers will have to operate on joined select statements. Not fun.
            rdStanza = Me.m_db.GetReader("SELECT * FROM Stanza ORDER BY StanzaID ASC")

            ' JS 18Jun13: Only allocate stanza data when reader successful
            If (rdStanza IsNot Nothing) Then

                If (stanzaDS.Nsplit > 0) Then
                    ' Get the highest number of groups in all split groups. Note that the sequence value field is not used here.
                    ' JS 50Nov2011: appended 'AS X' for SQL server and the likes. Works with MS Access.
                    stanzaDS.MaxStanza = CInt(Me.m_db.GetValue("SELECT MAX(NumGroups) FROM (SELECT COUNT(*) AS NumGroups FROM StanzaLifeStage GROUP BY StanzaID) AS X", 0))
                End If

                ' Get the number of groups from ecopath
                stanzaDS.nGroups = ecopathDS.NumGroups

                If stanzaDS.MaxAgeSplit < cCore.MAX_AGE Then
                    'VILLY: NEED TO REPLACE THIS WITH DYNAMIC CALCULATION ALLOWING FOR CHANGES IN K DURING EXECUTION
                    stanzaDS.MaxAgeSplit = cCore.MAX_AGE
                End If

                stanzaDS.redimStanza()
                iStanza = 0

                While rdStanza.Read()

                    ' JS 11May2010: Stanza configs without stanza groups are now loaded.
                    '               This *could* screw up the core calculations, but in a way
                    '               it already did by allowing empty stanza groups to be defined
                    '               in the system by allowing stanzaDS.nGroups to be non-zero,
                    '               even if stanzaDS.MaxStanza were 0. Based on this behaviour
                    '               there seems little harm by allowing the empty stanza group
                    '               names to be available in the core and to an interface.

                    ' Read this stanza
                    iStanza += 1

                    Try

                        stanzaDS.StanzaDBID(iStanza) = CInt(rdStanza("StanzaID"))
                        ' JS 20jun06: StanzaName array 1-dimensional. GroupNames only seem to matter to the EwE5 GUI.
                        '             EwE6 will resolve stanza group names via ICoreInputOutput objects to keep track of 'live' changes.
                        stanzaDS.StanzaName(iStanza) = CStr(rdStanza("StanzaName"))

                        stanzaDS.RecPowerSplit(iStanza) = CSng(rdStanza("RecPower"))
                        stanzaDS.BABsplit(iStanza) = CSng(rdStanza("BabSplit"))
                        stanzaDS.WmatWinf(iStanza) = CSng(rdStanza("WMatWinf"))
                        ' stanzaDS.HatchCode(iStanza) = CInt(rdStanza("HatchCode"))
                        stanzaDS.FixedFecundity(iStanza) = (CInt(rdStanza("FixedFecundity")) <> 0)
                        stanzaDS.EggAtSpawn(iStanza) = (CInt(Me.m_db.ReadSafe(rdStanza, "EggAtSpawn", True)) <> 0)

                        ' JS 23apr07: Leading B and QB groups are calculated at runtime, no longer stored in DB
                        ' JS 23nov10: Hah, three and a half years later these values are stored again
                        stanzaDS.BaseStanza(iStanza) = CInt(Me.m_db.ReadSafe(rdStanza, "LeadingLifeStage", cCore.NULL_VALUE))
                        ' JS 14jun12: Leading CB separated from leading B (default to LeadingLifeStage)
                        stanzaDS.BaseStanzaCB(iStanza) = CInt(Me.m_db.ReadSafe(rdStanza, "LeadingCB", stanzaDS.BaseStanza(iStanza)))

                    Catch ex As Exception
                        Me.LogMessage(String.Format("Error {0} occurred while reading Stanza {1}", ex.Message, stanzaDS.StanzaName(iStanza)))
                        bSucces = False
                    End Try

                    rdLifeStage = Me.m_db.GetReader(String.Format("SELECT * FROM StanzaLifeStage WHERE (StanzaID={0}) ORDER BY AgeStart ASC", stanzaDS.StanzaDBID(iStanza)))
                    iLifeStage = 0
                    While rdLifeStage.Read()

                        ' Next life stage in this stanza
                        iLifeStage += 1

                        ' Store Stanza configuration
                        Try

                            ' Resolve group index
                            iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(rdLifeStage("GroupID")))
                            ' JS 20jun06: Disabled (see comment above)
                            ' ecosimDS.StanzaName(nStanza, nGroup) = ecopathDS.GroupName(iGroup)
                            stanzaDS.EcopathCode(iStanza, iLifeStage) = iGroup
                            stanzaDS.Stanza_Z(iStanza, iLifeStage) = CSng(rdLifeStage("Mortality"))
                            stanzaDS.SpeciesCode(iGroup, 0) = iStanza
                            stanzaDS.Age1(iStanza, iLifeStage) = CInt(rdLifeStage("AgeStart"))

                        Catch ex As Exception
                            Me.LogMessage(String.Format("Error {0} occurred while reading StanzaLifeStage {1}", ex.Message, stanzaDS.StanzaName(iStanza), ecopathDS.GroupName(iGroup)))
                            bSucces = False
                        End Try

                        ' Inform Ecopath
                        ecopathDS.StanzaGroup(iGroup) = True

                    End While

                    Me.m_db.ReleaseReader(rdLifeStage)

                    ' Update number of groups in this stanza
                    stanzaDS.Nstanza(iStanza) = iLifeStage

                End While

                Me.m_db.ReleaseReader(rdStanza)
                rdStanza = Nothing

            End If
            stanzaDS.OnPostInitialization()

            Return bSucces
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Updates a stanza group in the DB.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function SaveStanza() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim iGroupID As Integer = 0
            Dim iGroup As Integer = 1

            Try
                '' This will delete Ecosim stanza shape assignments
                'Me.m_db.Execute("DELETE * FROM Stanza")

                writer = Me.m_db.GetWriter("Stanza")
                dt = writer.GetDataTable()

                For iStanza As Integer = 1 To stanzaDS.Nsplit

                    ' Sanity check: has life stages?
                    If (stanzaDS.Nstanza(iStanza) > 0) Then

                        drow = dt.Rows.Find(stanzaDS.StanzaDBID(iStanza))
                        bNewRow = (drow Is Nothing)

                        If bNewRow Then
                            drow = writer.NewRow()
                            drow("StanzaID") = stanzaDS.StanzaDBID(iStanza)
                        Else
                            drow.BeginEdit()
                        End If

                        drow("StanzaName") = stanzaDS.StanzaName(iStanza)
                        drow("RecPower") = stanzaDS.RecPowerSplit(iStanza)
                        drow("BabSplit") = stanzaDS.BABsplit(iStanza)
                        drow("WMatWinf") = stanzaDS.WmatWinf(iStanza)
                        drow("FixedFecundity") = stanzaDS.FixedFecundity(iStanza)
                        drow("EggAtSpawn") = stanzaDS.EggAtSpawn(iStanza)

                        ' JS 23apr07: Leading B and QB groups are calculated at runtime, no longer stored in DB
                        ' JS 23nov10: Leading B stored again
                        drow("LeadingLifeStage") = stanzaDS.BaseStanza(iStanza)
                        ' JS 14jun12: Leading CB separated from leading B, stored again
                        drow("LeadingCB") = stanzaDS.BaseStanzaCB(iStanza)

                        If bNewRow Then
                            writer.AddRow(drow)
                        Else
                            drow.EndEdit()
                        End If
                    Else
                        ' Hmm, something is very wrong here. This stanza group should not have existed!
                        Debug.Assert(False)
                    End If
                Next
                Me.m_db.ReleaseWriter(writer)
            Catch ex As Exception
                Return False
            End Try

            Try
                ' This is ok since no other objects link to the life stages
                Me.m_db.Execute("DELETE * FROM StanzaLifeStage")

                writer = Me.m_db.GetWriter("StanzaLifeStage")
                For iStanza As Integer = 1 To stanzaDS.Nsplit
                    For iLifeStage As Integer = 1 To stanzaDS.MaxStanza
                        iGroupID = ecopathDS.GroupDBID(stanzaDS.EcopathCode(iStanza, iLifeStage))
                        If (iGroupID > 0) Then
                            iGroup = stanzaDS.EcopathCode(iStanza, iLifeStage)
                            drow = writer.NewRow()
                            drow("StanzaID") = stanzaDS.StanzaDBID(iStanza)
                            drow("GroupID") = ecopathDS.GroupDBID(iGroup)
                            drow("Sequence") = iLifeStage
                            drow("AgeStart") = stanzaDS.Age1(iStanza, iLifeStage)
                            drow("Mortality") = stanzaDS.Stanza_Z(iStanza, iLifeStage)
                            'drow("vbK") = ecopathDS.vbKInput(iGroup)
                            writer.AddRow(drow)
                        End If
                    Next iLifeStage
                Next iStanza
                Me.m_db.ReleaseWriter(writer)
            Catch ex As Exception
                Return False
            End Try

            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a stanza group to the DB.
        ''' </summary>
        ''' <param name="strStanzaName">Name to assign to new stanza group.</param>
        ''' <param name="aiGroupID">Zero-based array of <see cref="cEcoPathGroupInput">Ecopath group</see>
        ''' IDs to assign to this multi-stanza configuration.</param>
        ''' <param name="iGroupAges">Zero-based array of start ages, corresponding
        ''' to the <paramref name="aiGroupID">group ID list</paramref>.</param>
        ''' <param name="iStanzaID">Database ID assigned to the new stanza group.</param>
        ''' <returns>Always false.</returns>
        ''' -------------------------------------------------------------------
        Friend Function AppendStanza(ByVal strStanzaName As String,
                                     ByVal aiGroupID() As Integer,
                                     ByVal iGroupAges() As Integer,
                                     ByRef iStanzaID As Integer) As Boolean _
                Implements IEcopathDataSource.AppendStanza

            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True
            Dim iMaxAge As Integer = 0
            Dim iMaxAgeGroup As Integer = 0

            ' Need to get a balanced set of values
            If aiGroupID.Length <> iGroupAges.Length Then
                Return False
            End If

            ' Process inputs
            For i As Integer = 0 To aiGroupID.Length - 1
                ' Test if groups exist
                If CInt(Me.m_db.GetValue(String.Format("SELECT COUNT(*) FROM EcopathGroup WHERE GroupID={0}", aiGroupID(i)), 0)) = 0 Then
                    Debug.Assert(False, String.Format("Invalid group ID {0} specified", aiGroupID(i)))
                    Return False
                End If
                ' Find max age
                If iGroupAges(i) > iMaxAge Then iMaxAge = iGroupAges(i) : iMaxAgeGroup = i
            Next i

            Try
                iStanzaID = CInt(Me.m_db.GetValue("SELECT MAX(StanzaID) FROM Stanza", 0)) + 1

                writer = Me.m_db.GetWriter("Stanza")

                drow = writer.NewRow()
                drow("StanzaID") = iStanzaID
                drow("StanzaName") = strStanzaName
                writer.AddRow(drow)

                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Try
                writer = Me.m_db.GetWriter("StanzaLifeStage")
                For i As Integer = 0 To aiGroupID.Length - 1
                    ' Start new row
                    drow = writer.NewRow()
                    drow("StanzaID") = iStanzaID
                    drow("GroupID") = aiGroupID(i)
                    drow("AgeStart") = iGroupAges(i)
                    drow("Sequence") = (i + 1)
                    'drow("vbK") = 0.3
                    writer.AddRow(drow)
                Next
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a stanza group from the DB.
        ''' </summary>
        ''' <param name="iStanzaID">Database ID of the stanza group to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Friend Function RemoveStanza(ByVal iStanzaID As Integer) As Boolean _
                Implements IEcopathDataSource.RemoveStanza
            Try
                Dim bSuccess As Boolean = True

                ' Cannot have orphaned taxa
                Dim reader As IDataReader = Me.m_db.GetReader(String.Format("SELECT TaxonID FROM EcopathStanzaTaxon WHERE (StanzaID={0})", iStanzaID))
                Dim ids As New List(Of Integer)

                While reader.Read
                    ids.Add(CInt(reader("TaxonID")))
                End While
                Me.m_db.ReleaseReader(reader)

                ' Delete these taxa
                For Each id As Integer In ids
                    bSuccess = bSuccess And Me.RemoveTaxon(id)
                Next

                Return bSuccess And Me.m_db.Execute(String.Format("DELETE FROM Stanza WHERE (StanzaID={0})", iStanzaID))
            Catch ex As Exception
                ' Kaboom
            End Try
            Return False

        End Function


        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a life stage to an existing stanza configuration.
        ''' </summary>
        ''' <param name="iStanzaDBID">Database ID of the stanza group to add the life stage to.</param>
        ''' <param name="iGroupDBID">Group to add as a life stage.</param>
        ''' <param name="iStartAge">Start age of this life stage.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function AddStanzaLifestage(ByVal iStanzaDBID As Integer, ByVal iGroupDBID As Integer,
                                           ByVal iStartAge As Integer) As Boolean _
                Implements DataSources.IEcopathDataSource.AddStanzaLifestage

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("StanzaLifeStage")

                ' Start new row
                drow = writer.NewRow()
                drow("StanzaID") = iStanzaDBID
                drow("GroupID") = iGroupDBID
                drow("AgeStart") = iStartAge
                'drow("vbK") = sVBK
                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a life stage from an existing stanza configuration.
        ''' </summary>
        ''' <param name="iStanzaDBID">Database ID of the stanza group to remove the life stage from.</param>
        ''' <param name="iGroupDBID">Group to remove as the life stage.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function RemoveStanzaLifestage(ByVal iStanzaDBID As Integer, ByVal iGroupDBID As Integer) As Boolean Implements DataSources.IEcopathDataSource.RemoveStanzaLifestage

            Dim bSucces As Boolean = True
            Try
                Me.m_db.Execute(String.Format("DELETE FROM StanzaLifeStage WHERE (StanzaID={0}) AND (GroupID={1})", iStanzaDBID, iGroupDBID))
            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Stanza

#Region " Ecopath "

#Region " Diagnostics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if the data source has unsaved changes for Ecopath.
        ''' </summary>
        ''' <returns>True if the data source has pending changes for Ecopath.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsEcopathModified() As Boolean Implements DataSources.IEcopathDataSource.IsEcopathModified

            If Not Me.IsConnected() Then Return False
            Return Me.IsChanged(s_EcopathComponents)

        End Function

#End Region ' Diagnostics

#Region " Groups "

#Region " Load "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads Ecopath Group information.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function LoadEcopathGroups() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim psdDS As cPSDDatastructures = Me.m_core.m_PSDData

            ' Init data structure
            ecopathDS.NumGroups = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM EcopathGroup", 0))
            ecopathDS.NumLiving = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM EcopathGroup WHERE (TYPE <= 1)", 0))
            ecopathDS.NumDetrit = ecopathDS.NumGroups - ecopathDS.NumLiving

            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathGroup ORDER BY Sequence ASC")
            Dim iGroup As Integer = 1
            Dim sTemp As Single = 0.0
            Dim strTemp As String = ""
            Dim bSucces As Boolean = True

            ' Allocate space
            If (Not ecopathDS.redimGroupVariables() Or Not psdDS.redimGroupVariables()) Then
                ' It would be quite remarkable to fail here... log message?
                Return False
            End If

            While reader.Read()

                Try
                    ecopathDS.GroupDBID(iGroup) = CInt(reader("GroupID"))
                    ecopathDS.GroupName(iGroup) = CStr(reader("GroupName"))
                    ecopathDS.PP(iGroup) = CSng(reader("Type"))
                    ecopathDS.Area(iGroup) = CSng(reader("Area"))
                    ecopathDS.BH(iGroup) = ecopathDS.B(iGroup) / ecopathDS.Area(iGroup)
                    ecopathDS.BAInput(iGroup) = CSng(reader("BiomAcc"))
                    ' VERIFY_JS: Check default value for BiomAccRate. 0 is assumed
                    ecopathDS.BaBi(iGroup) = CSng(reader("BiomAccRate"))
                    ecopathDS.GS(iGroup) = CSng(reader("Unassim"))
                    ecopathDS.DtImp(iGroup) = CSng(reader("DtImports"))
                    ecopathDS.Ex(iGroup) = CSng(reader("Export"))
                    ecopathDS.fCatch(iGroup) = CSng(reader("Catch"))
                    ecopathDS.DCInput(iGroup, 0) = CSng(reader("ImpVar"))
                    ecopathDS.GroupIsFish(iGroup) = (CInt(reader("GroupIsFish")) <> 0)
                    ecopathDS.GroupIsInvert(iGroup) = (CInt(reader("GroupIsInvert")) <> 0)
                    ecopathDS.Shadow(iGroup) = CSng(reader("NonMarketValue"))
                    ecopathDS.Resp(iGroup) = CSng(reader("Respiration"))
                    ecopathDS.Immig(iGroup) = CSng(reader("Immigration"))
                    ecopathDS.Emigration(iGroup) = CSng(reader("Emigration"))
                    ecopathDS.Emig(iGroup) = CSng(Me.m_db.ReadSafe(reader, "EmigRate", 0.0!))

                    ' PSD
                    ecopathDS.vbK(iGroup) = CSng(Me.m_db.ReadSafe(reader, "VBK", -1))
                    psdDS.AinLWInput(iGroup) = CSng(reader("AinLW"))
                    psdDS.BinLWInput(iGroup) = CSng(reader("BinLW"))
                    psdDS.LooInput(iGroup) = CSng(reader("Loo"))
                    psdDS.WinfInput(iGroup) = CSng(reader("Winf"))
                    psdDS.t0Input(iGroup) = CSng(reader("t0"))
                    psdDS.TcatchInput(iGroup) = CSng(reader("Tcatch"))
                    psdDS.TmaxInput(iGroup) = CSng(reader("Tmax"))

                    'variables with input output pairs
                    ecopathDS.EEinput(iGroup) = CSng(reader("EcoEfficiency"))
                    ecopathDS.OtherMortinput(iGroup) = CSng(Me.m_db.ReadSafe(reader, "OtherMort", cCore.NULL_VALUE))
                    ecopathDS.PBinput(iGroup) = CSng(reader("ProdBiom"))
                    ecopathDS.QBinput(iGroup) = CSng(reader("ConsBiom"))
                    ecopathDS.GEinput(iGroup) = CSng(reader("ProdCons"))
                    ecopathDS.Binput(iGroup) = CSng(reader("Biomass"))
                    ecopathDS.BHinput(iGroup) = ecopathDS.Binput(iGroup) / ecopathDS.Area(iGroup)

                    ecopathDS.GroupColor(iGroup) = Integer.Parse(CStr(reader("PoolColor")), Globalization.NumberStyles.HexNumber)

                Catch ex As Exception
                    Me.LogMessage(String.Format("Error {0} occurred while reading group {1}", ex.Message, ecopathDS.GroupName(iGroup)))
                    bSucces = False
                End Try

                iGroup += 1

            End While

            Debug.Assert(iGroup - 1 = ecopathDS.NumGroups)

            Me.m_db.ReleaseReader(reader)
            reader = Nothing

            bSucces = bSucces And Me.LoadEcopathDietComp()
            bSucces = bSucces And Me.LoadStanza()

            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update group info in the data source.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function SaveEcopathGroups() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim psdDS As cPSDDatastructures = Me.m_core.m_PSDData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim iGroup As Integer = 0
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("EcopathGroup")
                dt = writer.GetDataTable()

                For iGroup = 1 To ecopathDS.NumGroups

                    ' Find existing row
                    drow = dt.Rows.Find(ecopathDS.GroupDBID(iGroup))
                    Debug.Assert(drow IsNot Nothing, String.Format("Cannot find existing row for group {0}", ecopathDS.GroupDBID(iGroup)))

                    drow.BeginEdit()
                    drow("GroupID") = ecopathDS.GroupDBID(iGroup)
                    drow("Sequence") = iGroup
                    drow("GroupName") = ecopathDS.GroupName(iGroup)
                    drow("Type") = ecopathDS.PP(iGroup)
                    drow("Area") = ecopathDS.Area(iGroup)
                    drow("BiomAcc") = ecopathDS.BAInput(iGroup)
                    drow("BiomAccRate") = ecopathDS.BaBi(iGroup)
                    drow("Unassim") = ecopathDS.GS(iGroup)
                    drow("DtImports") = ecopathDS.DtImp(iGroup)
                    drow("Export") = ecopathDS.Ex(iGroup)
                    drow("Catch") = ecopathDS.fCatch(iGroup)
                    drow("ImpVar") = ecopathDS.DCInput(iGroup, 0)
                    drow("GroupIsFish") = ecopathDS.GroupIsFish(iGroup)
                    drow("GroupIsInvert") = ecopathDS.GroupIsInvert(iGroup)
                    drow("NonMarketValue") = ecopathDS.Shadow(iGroup)
                    drow("Respiration") = ecopathDS.Resp(iGroup)

                    'variable with input/output pair only the input gets saved
                    drow("EcoEfficiency") = ecopathDS.EEinput(iGroup)
                    drow("OtherMort") = ecopathDS.OtherMortinput(iGroup)
                    drow("ProdBiom") = ecopathDS.PBinput(iGroup)
                    drow("ConsBiom") = ecopathDS.QBinput(iGroup)
                    drow("ProdCons") = ecopathDS.GEinput(iGroup)
                    drow("Biomass") = ecopathDS.Binput(iGroup)
                    ' Should not really be here, should it? 
                    ' ecopathDS.BHinput(iGroup) = ecopathDS.Binput(iGroup) / ecopathDS.Area(iGroup)

                    drow("Immigration") = ecopathDS.Immig(iGroup)
                    drow("Emigration") = ecopathDS.Emigration(iGroup)
                    drow("EmigRate") = ecopathDS.Emig(iGroup)
                    drow("PoolColor") = String.Format("{0:x8}", ecopathDS.GroupColor(iGroup))

                    'PSD
                    drow("VBK") = ecopathDS.vbK(iGroup)
                    drow("Tcatch") = psdDS.Tcatch(iGroup)
                    drow("AinLW") = psdDS.AinLWInput(iGroup)
                    drow("BinLW") = psdDS.BinLWInput(iGroup)
                    drow("Loo") = psdDS.LooInput(iGroup)
                    drow("Winf") = psdDS.WinfInput(iGroup)
                    drow("t0") = psdDS.t0Input(iGroup)
                    drow("Tmax") = psdDS.TmaxInput(iGroup)

                    drow.EndEdit()

                Next iGroup

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving EcopathGroup", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, True)

            bSucces = bSucces And Me.SaveEcopathDietComp()
            bSucces = bSucces And Me.SaveStanza()

            Return bSucces

        End Function

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a record for a new Ecopath group in the data source.
        ''' </summary>
        ''' <param name="strGroupName">The name of the group to create.</param>
        ''' <param name="sPP">The type of the new group; 0=consumer, 1=producer, 2=detritus, or a cons/prod ratio.</param>
        ''' <param name="sVBK">The vbK value to pass to the group.</param>
        ''' <param name="iPosition">The position of the new group in the group sequence.</param>
        ''' <param name="iGroupID">Database ID assigned to the new Group.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will not adjust the data arrays. Due to the complex organization of the
        ''' core a full data reload is required after a group is created.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Function AddGroup(ByVal strGroupName As String,
                                 ByVal sPP As Single,
                                 ByVal sVBK As Single,
                                 ByVal iPosition As Integer,
                                 ByRef iGroupID As Integer) As Boolean _
                Implements IEcopathDataSource.AddGroup

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                iGroupID = CInt(Me.m_db.GetValue("SELECT MAX(GroupID) FROM EcopathGroup", 0)) + 1

                ' Start writing, protect sequence
                writer = Me.m_db.GetWriter("EcopathGroup")

                ' Get new row to add
                drow = writer.NewRow()
                ' Database will take care of defaults, only take care of the bare necessities
                drow("GroupID") = iGroupID
                drow("GroupName") = strGroupName
                drow("Type") = sPP
                drow("vbK") = sVBK
                drow("t0") = -9999 ' Fix default
                drow("Sequence") = iPosition

                ' Commit to db
                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            ' Set initial diet data for this group
            If sPP < 2 Then
                Try
                    ' Start writing
                    writer = Me.m_db.GetWriter("EcopathDietComp")

                    ' For all detritus groups
                    For iPrey As Integer = ecopathDS.NumLiving + 1 To ecopathDS.NumGroups
                        ' Get new row to add
                        drow = writer.NewRow()
                        ' Database will take care of defaults, only take care of the bare necessities
                        drow("PredID") = iGroupID
                        drow("PreyID") = ecopathDS.GroupDBID(iPrey)
                        ' Commit to db
                        writer.AddRow(drow)
                    Next iPrey

                    Me.m_db.ReleaseWriter(writer, True)

                Catch ex As Exception
                    bSucces = False
                End Try
            End If

            ' Create this group for each ecosim scenario
            bSucces = bSucces And Me.AddEcosimGroupToAllScenarios(iGroupID)
            ' Create this group for each ecospace scenario
            bSucces = bSucces And Me.AddEcospaceGroupToAllScenarios(iGroupID, (sPP = 2.0))
            ' Create this group for each ecotracer scenario
            bSucces = bSucces And Me.AddEcotracerGroupToAllScenarios(iGroupID)

            Return bSucces

        End Function

        Private Function AddCatchDataForGroup(ByVal iGroupID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim iFleetID As Integer = 0
            Dim bSucces As Boolean = True

            For iFleet As Integer = 1 To ecopathDS.NumFleet
                iFleetID = ecopathDS.FleetDBID(iFleet)
                bSucces = bSucces And Me.AddCatch(iGroupID, iFleetID)
                bSucces = bSucces And Me.AddDiscardFate(iGroupID, iFleetID)
            Next
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a group from the data source.
        ''' </summary>
        ''' <param name="iEcopathGroupID">DBID of the Ecopath group to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that this will not adjust the data arrays. Due to the complex organization of the
        ''' core a full data reload is required after a group is removed.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Function RemoveGroup(ByVal iEcopathGroupID As Integer) As Boolean _
                 Implements IEcopathDataSource.RemoveGroup

            Dim bSucces As Boolean = True

            Try

                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimTimeSeriesFleet WHERE (GroupID={0})", iEcopathGroupID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimTimeSeriesGroup WHERE (GroupID={0})", iEcopathGroupID))

                ' Remove all Ecosim groups related to this Ecopath group
                Dim reader As IDataReader = Me.m_db.GetReader(String.Format("SELECT GroupID FROM EcosimScenarioGroup WHERE EcopathGroupID={0}", iEcopathGroupID))
                If (reader IsNot Nothing) Then
                    While reader.Read()
                        bSucces = bSucces And Me.RemoveEcosimGroup(CInt(reader("GroupID")))
                    End While
                End If
                Me.m_db.ReleaseReader(reader)

                reader = Me.m_db.GetReader(String.Format("SELECT GroupID FROM EcospaceScenarioGroup WHERE EcopathGroupID={0}", iEcopathGroupID))
                If (reader IsNot Nothing) Then
                    While reader.Read()
                        bSucces = bSucces And Me.RemoveEcospaceGroup(CInt(reader("GroupID")), iEcopathGroupID)
                    End While
                End If
                Me.m_db.ReleaseReader(reader)

                ' Now Ecosim and Ecospace are clean, delete the group from Ecopath
                ' Need manual deletion from all tables that were added through database updates

                ' Taxa
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcopathGroupTaxon WHERE (EcopathGroupID={0})", iEcopathGroupID))
                ' Samples
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcopathGroupCatchSample WHERE (GroupID={0})", iEcopathGroupID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcopathDietCompSample WHERE (PredID={0} OR PreyID={0})", iEcopathGroupID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcopathGroupSample WHERE (GroupID={0})", iEcopathGroupID))
                ' Ecopath itself
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcopathGroup WHERE (GroupID={0})", iEcopathGroupID))

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing group {1}", ex.Message, iEcopathGroupID))
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Move an Ecopath group to a different position in the group sequence.
        ''' </summary>
        ''' <param name="iGroupID">Database ID of the group to move.</param>
        ''' <param name="iPosition">The new position of the group in the group sequence.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' This method will directly modify the entry in the database
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Function MoveGroup(ByVal iGroupID As Integer, ByVal iPosition As Integer) As Boolean _
                 Implements IEcopathDataSource.MoveGroup

            Dim bSucces As Boolean = True
            Try
                Me.m_db.Execute(String.Format("UPDATE EcopathGroup SET Sequence={1} WHERE (GroupID={0})", iGroupID, iPosition))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while moving group {1}", ex.Message, iGroupID))
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Modify

#Region " DietComp "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads ecopath diet composition information.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function LoadEcopathDietComp() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Nothing
            Dim iPred As Integer = 0
            Dim iPrey As Integer = 0
            Dim sDiet As Single = 0.0!
            Dim bSucces As Boolean = True

            Try
                reader = Me.m_db.GetReader("SELECT * FROM EcopathDietComp")
                While reader.Read()

                    iPred = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("PredID")))
                    iPrey = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("PreyID")))

                    Debug.Assert(iPred >= 0 And iPrey >= 0)
                    sDiet = CSng(reader("Diet"))

                    ' Set diet to 0 for non-living groups (fixes #878)
                    If (sDiet > 0) And (iPred > ecopathDS.NumLiving) Then sDiet = 0
                    ecopathDS.DCInput(iPred, iPrey) = sDiet

                    If iPrey > ecopathDS.NumLiving Then
                        ecopathDS.DF(iPred, iPrey - ecopathDS.NumLiving) = CSng(reader("DetritusFate"))
                    End If

                    ' 060528JS: ASSERT on "diet leftovers" from previous incarnations, including 041020VC fix for carbon groups
                    ' The actual data fix is performed once during EwE5 import, and should not reoccur when running EwE6.
                    If ecopathDS.PP(iPred) = 1 And ecopathDS.QB(iPred) <= 0 Then
                        If (ecopathDS.DCInput(iPred, iPrey) <> 0) Then
                            cLog.Write(String.Format("Database error on DCInput({0},{1})={2}, expected 0", iPred, iPrey, ecopathDS.DCInput(iPred, iPrey)))
                        End If
                    End If

                    ' VERIFY_JS: check mapping for MTI with JB
                    ' ecopathDS.??(nPred, nPrey) = CSng(reader("MTI"))
                    ' VERIFY_JS: check mapping for Electivity with JB
                    ' ecopathDS.??(nPred, nPrey) = CSng(reader("Electivity"))
                End While
                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading EcopathDietComp {1}, {2}", ex.Message, ecopathDS.GroupName(iPred), ecopathDS.GroupName(iPrey)))
                bSucces = False
            End Try

            ' Read 'Import'
            reader = Me.m_db.GetReader("SELECT * FROM EcopathGroup ORDER BY Sequence ASC")
            iPred = 1
            While reader.Read()
                If CSng(reader("ImpVar")) > 0 Then ecopathDS.DCInput(iPred, 0) = CSng(reader("ImpVar"))
                iPred += 1
            End While
            Me.m_db.ReleaseReader(reader)

            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Writes the DietComp information to the database.
        ''' </summary>
        ''' <returns>True if successful</returns>
        ''' -------------------------------------------------------------------
        Private Function SaveEcopathDietComp() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim idPred As Integer = 0
            Dim iPred As Integer = 0
            Dim idPrey As Integer = 0
            Dim iPrey As Integer = 0

            Dim bSucces As Boolean = True

            Try
                ' No incremental save for now
                Me.m_db.Execute("DELETE * FROM EcopathDietComp")

                writer = Me.m_db.GetWriter("EcopathDietComp")
                ' DietComp is stored in EwE as an indexed list per predator
                For iPred = 1 To ecopathDS.NumGroups

                    ' Get DBID for predator to update
                    idPred = ecopathDS.GroupDBID(iPred)

                    For iPrey = 1 To ecopathDS.NumGroups

                        ' Get DBID for prey to update
                        idPrey = ecopathDS.GroupDBID(iPrey)

                        drow = writer.NewRow()
                        drow("PredID") = idPred
                        drow("PreyID") = idPrey
                        drow("Diet") = ecopathDS.DCInput(iPred, iPrey)
                        If iPrey > ecopathDS.NumLiving Then
                            drow("DetritusFate") = ecopathDS.DF(iPred, iPrey - ecopathDS.NumLiving)
                        Else
                            drow("DetritusFate") = 0
                        End If

                        ' VERIFY_JS: check mapping for MTI with JB
                        ' drow("MTI") = ??
                        ' VERIFY_JS: check mapping for Electivity with JB
                        ' drow("Electivity") = ??

                        writer.AddRow(drow)

                    Next iPrey
                Next iPred

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces
        End Function

#End Region ' DietComp

#End Region ' Groups

#Region " Fleets "

#Region " Helper methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if there is catch for at least one group.
        ''' </summary>
        ''' <returns>True if catch was found.</returns>
        ''' -------------------------------------------------------------------
        Private Function IsFishing() As Boolean
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim bIsFishing As Boolean = False
            Dim iGroup As Integer = 1

            While iGroup < ecopathDS.NumGroups And Not bIsFishing
                bIsFishing = (ecopathDS.fCatch(iGroup) > 0.0)
                iGroup += 1
            End While

            Return bIsFishing
        End Function

        Private Function AddCatch(ByVal iGroupID As Integer, ByVal iFleetID As Integer) As Boolean
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("EcopathCatch")
                drow = writer.NewRow()
                drow("GroupID") = iGroupID
                drow("FleetID") = iFleetID
                ' All other values will receive defaults
                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer)
            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

        Private Function AddDiscardFate(ByVal iGroupID As Integer, ByVal iFleetID As Integer) As Boolean
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iGroup As Integer = Array.IndexOf(ecopathDS.GroupDBID, iGroupID)
            Dim bSucces As Boolean = True

            If (iGroup <= ecopathDS.NumLiving) Then Return True

            Try
                writer = Me.m_db.GetWriter("EcopathDiscardFate")
                drow = writer.NewRow()
                drow("GroupID") = iGroupID
                drow("FleetID") = iFleetID
                ' Set default database value
                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer)
            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Helper methods

#Region " Load "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads all fleet-related data.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' If there is <see cref="IsFishing">no fishing</see>, the fleet data will not be loaded.
        ''' This check is inherited from EwE5.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Function LoadEcopathFleetInfo() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim bSucces As Boolean = True

            ecopathDS.NoGearData = Not IsFishing()

            ecopathDS.NumFleet = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM EcopathFleet"))

            ' This will be necessary when reading Gear tables. Can only call this after groups are read.
            If Not ecopathDS.RedimFleetVariables(True) Then
                Return False
            End If

            bSucces = LoadEcopathFleets()
            bSucces = bSucces And LoadEcopathCatch()
            bSucces = bSucces And LoadEcopathDiscardFate()

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads all Ecopath fleets.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function LoadEcopathFleets() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Nothing
            Dim iFleet As Integer = 1
            Dim bSucces As Boolean = True

            Try
                reader = Me.m_db.GetReader("SELECT * FROM EcopathFleet ORDER BY Sequence ASC")
                While reader.Read()

                    ecopathDS.FleetDBID(iFleet) = CInt(reader("FleetID"))
                    ecopathDS.FleetName(iFleet) = CStr(reader("FleetName"))
                    ecopathDS.CostPct(iFleet, eCostIndex.Fixed) = CSng(reader("FixedCost"))
                    ecopathDS.CostPct(iFleet, eCostIndex.Sail) = CSng(reader("SailingCost"))
                    ecopathDS.CostPct(iFleet, eCostIndex.CUPE) = CSng(reader("variableCost"))
                    ecopathDS.FleetColor(iFleet) = Integer.Parse(CStr(reader("PoolColor")), Globalization.NumberStyles.HexNumber)
                    iFleet += 1

                End While

                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading EcopathFleet {1}", ex.Message, iFleet))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function LoadEcopathCatch() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Nothing
            Dim iFleet As Integer = 0
            Dim iGroup As Integer = 0
            Dim bSucces As Boolean = True

            Try

                reader = Me.m_db.GetReader("SELECT * FROM EcopathCatch")
                While reader.Read()

                    iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("GroupID")))
                    iFleet = Array.IndexOf(ecopathDS.FleetDBID, CInt(reader("FleetID")))

                    ' JS270707: no need to assert any longer
                    'Debug.Assert(iGroup >= 0 And iFleet >= 0)

                    If (iGroup >= 1 And iFleet >= 1) Then
                        ecopathDS.Landing(iFleet, iGroup) = CSng(reader("Landing"))
                        ecopathDS.Discard(iFleet, iGroup) = CSng(reader("discards"))
                        ecopathDS.Market(iFleet, iGroup) = CSng(reader("price"))
                        ecopathDS.PropDiscardMort(iFleet, iGroup) = CSng(Me.m_db.ReadSafe(reader, "DiscardMortality", 0.0!))
                    Else
                        Me.LogMessage(String.Format("Error {0} occurred while appending loading catch for group {0}, fleet {1}", iGroup, iFleet))
                        bSucces = False
                    End If

                End While

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading catch {1}, {2}", ex.Message, iGroup, iFleet))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)

            Return bSucces

        End Function

        Private Function LoadEcopathDiscardFate() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Nothing
            Dim iFleet As Integer = 0
            Dim iGroup As Integer = 0
            Dim bSucces As Boolean = True

            Try
                reader = Me.m_db.GetReader("SELECT * FROM EcopathDiscardFate")
                If reader IsNot Nothing Then

                    While reader.Read()

                        iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("GroupID")))
                        iFleet = Array.IndexOf(ecopathDS.FleetDBID, CInt(reader("FleetID")))

                        If (iGroup > ecopathDS.NumLiving) Then
                            ecopathDS.DiscardFate(iFleet, iGroup - ecopathDS.NumLiving) = CSng(reader("DiscardFate"))
                        End If

                    End While
                    Me.m_db.ReleaseReader(reader)

                End If

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading DiscardFate {1}, {2}", ex.Message, iGroup, iFleet))
                bSucces = False
            End Try

            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Saves all fleet-related data to the data source.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function SaveEcopathFleetInfo() As Boolean

            Dim bSucces As Boolean = True

            bSucces = SaveEcopathFleets()
            bSucces = bSucces And SaveCatch()
            bSucces = bSucces And SaveDiscardFate()

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Saves all fleets.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function SaveEcopathFleets() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim iFleet As Integer = 0
            Dim bAddNewRow As Boolean = False
            Dim bSucces As Boolean = True

            Try

                writer = Me.m_db.GetWriter("EcopathFleet")
                dt = writer.GetDataTable()

                For iFleet = 1 To ecopathDS.NumFleet

                    ' Get existing row, or create new row if a fleet does not yet exist in the DB. This can
                    ' happen when a fleet is added to the models without properly adding it to the database;
                    ' this code needs to be prepared for that eventuality.
                    drow = dt.Rows.Find(ecopathDS.FleetDBID(iFleet))
                    bAddNewRow = (drow Is Nothing)

                    If bAddNewRow Then drow = writer.NewRow()
                    Debug.Assert(drow IsNot Nothing, String.Format("No existing row for fleet {0}", ecopathDS.FleetDBID(iFleet)))

                    drow("Sequence") = iFleet
                    If bAddNewRow Then drow("FleetID") = ecopathDS.FleetDBID(iFleet)
                    drow("FleetName") = ecopathDS.FleetName(iFleet)
                    drow("FixedCost") = ecopathDS.CostPct(iFleet, eCostIndex.Fixed)
                    drow("SailingCost") = ecopathDS.CostPct(iFleet, eCostIndex.Sail)
                    drow("variableCost") = ecopathDS.CostPct(iFleet, eCostIndex.CUPE)
                    drow("PoolColor") = String.Format("{0:x8}", ecopathDS.FleetColor(iFleet))

                    If bAddNewRow Then writer.AddRow(drow)
                Next iFleet
                ' Save changes
                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving EcopathFleet", ex.Message))
                bSucces = False
            End Try

            Return bSucces
        End Function

        Private Function SaveCatch() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iFleet As Integer = 0
            Dim iGroup As Integer = 0
            Dim bSucces As Boolean = True

            Try
                Me.m_db.Execute("DELETE * FROM EcopathCatch")

                writer = Me.m_db.GetWriter("EcopathCatch")

                For iFleet = 1 To ecopathDS.NumFleet
                    For iGroup = 1 To ecopathDS.NumGroups

                        ' JS 04aug08: only save rows with data
                        If (ecopathDS.Landing(iFleet, iGroup) > 0.0!) Or
                           (ecopathDS.Discard(iFleet, iGroup) > 0.0!) Or
                           ((ecopathDS.Market(iFleet, iGroup) > 0.0!) And (ecopathDS.Market(iFleet, iGroup) < 1.0!)) Or
                           (ecopathDS.PropDiscardMort(iFleet, iGroup) > 0.0!) Then

                            drow = writer.NewRow()
                            drow("FleetID") = ecopathDS.FleetDBID(iFleet)
                            drow("GroupID") = ecopathDS.GroupDBID(iGroup)
                            drow("Landing") = ecopathDS.Landing(iFleet, iGroup)
                            drow("Discards") = ecopathDS.Discard(iFleet, iGroup)
                            drow("Price") = ecopathDS.Market(iFleet, iGroup)
                            drow("DiscardMortality") = ecopathDS.PropDiscardMort(iFleet, iGroup)
                            writer.AddRow(drow)

                        End If

                    Next iGroup
                Next iFleet

                ' Save changes
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving catch", ex.Message))
                bSucces = False
            End Try

            Return bSucces
        End Function

        Private Function SaveDiscardFate() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iFleet As Integer = 0
            Dim iGroup As Integer = 0
            Dim bSucces As Boolean = True

            Try
                Me.m_db.Execute("DELETE * FROM EcopathDiscardFate")

                writer = Me.m_db.GetWriter("EcopathDiscardFate")

                For iFleet = 1 To ecopathDS.NumFleet
                    For iGroup = 1 To ecopathDS.NumGroups - ecopathDS.NumLiving

                        drow = writer.NewRow()
                        drow("FleetID") = ecopathDS.FleetDBID(iFleet)
                        drow("GroupID") = ecopathDS.GroupDBID(iGroup + ecopathDS.NumLiving)
                        drow("DiscardFate") = ecopathDS.DiscardFate(iFleet, iGroup)
                        writer.AddRow(drow)

                    Next iGroup
                Next iFleet

                ' Save changes
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving DiscardFate {1}, {2}", ex.Message, iGroup, iFleet))
                bSucces = False
            End Try

            Return bSucces

        End Function

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a fleet to the data source.
        ''' </summary>
        ''' <param name="strFleetName">Name of the new fleet.</param>
        ''' <param name="iFleetID">Database ID assigned to the new fleet.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function AddFleet(ByVal strFleetName As String,
                                 ByVal iPosition As Integer,
                                 ByRef iFleetID As Integer) As Boolean _
                Implements IEcopathDataSource.AddFleet

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            iFleetID = CInt(Me.m_db.GetValue("SELECT MAX(FleetID) FROM EcopathFleet", 0)) + 1

            Try
                ' Start writing, protect sequence
                writer = Me.m_db.GetWriter("EcopathFleet")
                drow = writer.NewRow()
                drow("FleetID") = iFleetID
                drow("FleetName") = strFleetName
                drow("Sequence") = iPosition
                drow("PoolColor") = "00000000"
                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while adding fleet {1}", ex.Message, strFleetName))
                bSucces = False
            End Try

            ' Add Catch
            bSucces = bSucces And Me.AddCatchDataForFleet(iFleetID)
            ' Create ecosim fleet forcing bits

            ' bSucces = bSucces And Me.AddCatchabilityFleet(iFleetID)

            ' Create fleet objects though
            bSucces = bSucces And Me.AddEcosimFleetToAllScenarios(iFleetID)
            bSucces = bSucces And Me.AddEcospaceFleetToAllScenarios(iFleetID)

            Return bSucces

        End Function

        Private Function AddCatchDataForFleet(ByVal iFleetID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim iGroupID As Integer = 0
            Dim bSucces As Boolean = True

            ' JS 01Mar12: Store catches for all groups, not only living groups
            For iGroup As Integer = 1 To ecopathDS.NumGroups
                iGroupID = ecopathDS.GroupDBID(iGroup)
                bSucces = bSucces And Me.AddCatch(iGroupID, iFleetID)
            Next

            ' JS 21oct09: Send all detritus to only the LAST detritus group (bug 460)
            '             This code assumes that detritus groups are at the end of the group list
            bSucces = bSucces And Me.AddDiscardFate(ecopathDS.GroupDBID(ecopathDS.NumGroups), iFleetID)

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a fleet from the data source.
        ''' </summary>
        ''' <param name="iFleetID">Ecopath ID of the fleet to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function RemoveFleet(ByVal iFleetID As Integer) As Boolean _
                Implements IEcopathDataSource.RemoveFleet

            Dim bSucces As Boolean = True
            Try
                bSucces = bSucces And Me.RemoveEcospaceFleet(iFleetID)
                bSucces = bSucces And Me.RemoveEcosimFleet(iFleetID)
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcopathFleet WHERE (FleetID={0})", iFleetID))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing fleet {1}", ex.Message, iFleetID))
                bSucces = False
            End Try
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Move an Ecopath fleet to a different position in the fleet sequence.
        ''' </summary>
        ''' <param name="iFleetID">Database ID of the fleet to move.</param>
        ''' <param name="iPosition">The new position of the fleet in the fleet sequence.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function MoveFleet(ByVal iFleetID As Integer, ByVal iPosition As Integer) As Boolean _
                Implements DataSources.IEcopathDataSource.MoveFleet

            Dim bSucces As Boolean = True
            Try
                Me.m_db.Execute(String.Format("UPDATE EcopathFleet SET Sequence={1} WHERE (FleetID={0})", iFleetID, iPosition))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while moving fleet {1}", ex.Message, iFleetID))
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region '  Modify

#End Region ' Fleets

#Region " Datasets "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load all time series dataset definitions for Ecopath.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>Yeah, this is odd; time series can only be used with Ecosim
        ''' but this logic just reads which time series will be available for Ecosim
        ''' later on; it is convenient to know which data sets are provided with
        ''' the model, just as it is convenient to know which scenarios are
        ''' before they are loaded ;)</remarks>
        ''' -------------------------------------------------------------------
        Private Function LoadTimeSeriesDatasets() As Boolean

            Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData
            Dim reader As IDataReader = Nothing
            Dim iDataset As Integer = 1
            Dim bSucces As Boolean = True

            Try
                tsDS.nDatasets = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM EcosimTimeSeriesDataset"))
            Catch ex As Exception
                tsDS.nDatasets = 0
            End Try

            tsDS.RedimTimeSeriesDatasets()

            reader = Me.m_db.GetReader("SELECT * FROM EcosimTimeSeriesDataset ORDER BY DatasetID ASC")
            If reader IsNot Nothing Then
                Try
                    While reader.Read()
                        tsDS.iDatasetDBID(iDataset) = CInt(reader("DatasetID"))
                        tsDS.strDatasetNames(iDataset) = CStr(reader("DatasetName"))
                        tsDS.strDatasetDescription(iDataset) = CStr(Me.m_db.ReadSafe(reader, "Description", ""))
                        tsDS.strDatasetAuthor(iDataset) = CStr(Me.m_db.ReadSafe(reader, "Author", ""))
                        tsDS.strDatasetContact(iDataset) = CStr(Me.m_db.ReadSafe(reader, "Contact", ""))
                        tsDS.nDatasetFirstYear(iDataset) = CInt(reader("FirstYear"))
                        tsDS.nDatasetNumPoints(iDataset) = CInt(reader("NumPoints"))
                        tsDS.DataSetIntervals(iDataset) = CType(CInt(Me.m_db.ReadSafe(reader, "DataInterval", eTSDataSetInterval.Annual)), eTSDataSetInterval)

                        tsDS.nDatasetNumTimeSeries(iDataset) = CInt(Me.m_db.GetValue(String.Format("SELECT COUNT(*) FROM EcosimTimeSeries WHERE (DatasetID={0})", CInt(reader("DatasetID")))))
                        iDataset += 1
                    End While
                Catch ex As Exception
                    bSucces = False
                End Try
                Me.m_db.ReleaseReader(reader)
            End If

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an time series dataset to the data source.
        ''' </summary>
        ''' <param name="strDatasetName">Name to assign to new dataset.</param>
        ''' <param name="strDescription">Description to assign to new dataset.</param>
        ''' <param name="strAuthor">Author to assign to the new dataset.</param>
        ''' <param name="strContact">Contact info to assign to the new dataset.</param>
        ''' <param name="iFirstYear">First year of the dataset.</param>
        ''' <param name="iNumPoints">Number of data points in the dataset.</param>
        ''' <param name="interval"><see cref="eTSDataSetInterval">Interval</see> between
        ''' to points in the dataset.</param>
        ''' <param name="iDatasetID">Database ID assigned to the new dataset.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function AppendTimeSeriesDataset(ByVal strDatasetName As String, ByVal strDescription As String,
                ByVal strAuthor As String, ByVal strContact As String,
                ByVal iFirstYear As Integer, ByVal iNumPoints As Integer, interval As eTSDataSetInterval,
                ByRef iDatasetID As Integer) As Boolean _
            Implements DataSources.IEcosimDatasource.AppendTimeSeriesDataset

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim idm As New cIDMappings()
            Dim bSucces As Boolean = True

            Try
                ' Delete existing dataset with same name, if any
                Dim reader As IDataReader = Me.m_db.GetReader(String.Format("SELECT DatasetID FROM EcosimTimeSeriesDataset WHERE DatasetName='{0}'", strDatasetName))
                Dim lDatasetID As New List(Of Integer)
                While reader.Read
                    lDatasetID.Add(CInt(reader("DatasetID")))
                End While
                Me.m_db.ReleaseReader(reader)

                ' Delete dataset(s)
                For Each iDatasetIDTemp As Integer In lDatasetID
                    bSucces = bSucces And Me.RemoveTimeSeriesDatasetID(iDatasetIDTemp)
                Next

                ' Still looking good?
                If bSucces Then

                    iDatasetID = CInt(Me.m_db.GetValue("SELECT MAX(DatasetID) FROM EcosimTimeSeriesDataset", 0)) + 1

                    writer = Me.m_db.GetWriter("EcosimTimeSeriesDataset")

                    drow = writer.NewRow()
                    drow("DatasetID") = iDatasetID
                    drow("DatasetName") = strDatasetName
                    drow("Description") = strDescription
                    drow("Author") = strAuthor
                    drow("Contact") = strContact
                    drow("FirstYear") = iFirstYear
                    drow("NumPoints") = iNumPoints
                    drow("DataInterval") = CInt(interval)
                    'drow("LastSaved") = cDateUtils.GetJulianDate()
                    writer.AddRow(drow)

                    Me.m_db.ReleaseWriter(writer)

                    ' Reload time series dataset
                    If bSucces Then Me.LoadTimeSeriesDatasets()

                End If

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while appending dataset {1}", ex.Message, strDatasetName))
                bSucces = False
            End Try

            Return bSucces
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes all time series belonging to a specific dataset from the data source.
        ''' </summary>
        ''' <param name="iDataset">Index of the dataset to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function RemoveTimeSeriesDataset(ByVal iDataset As Integer) As Boolean _
                Implements DataSources.IEcosimDatasource.RemoveTimeSeriesDataset
            Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData
            Return Me.RemoveTimeSeriesDatasetID(tsDS.iDatasetDBID(iDataset))
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes all time series belonging to a specific dataset from the data source.
        ''' </summary>
        ''' <param name="iDatasetID">Database ID of the dataset to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function RemoveTimeSeriesDatasetID(ByVal iDatasetID As Integer) As Boolean

            Dim bSucces As Boolean = True
            Try
                ' Cascading delete may fail due to 'weak' relations set by updates. Aargh, how I dislike Access!!!
                ' Solution: manually delete all dataset links
                Me.m_db.Execute(String.Format("DELETE FROM EcosimTimeSeries WHERE (DatasetID={0})", iDatasetID))
                Me.m_db.Execute(String.Format("DELETE FROM EcosimTimeSeriesDataset WHERE (DatasetID={0})", iDatasetID))
            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Datasets

#Region " Taxa "

#Region " Load "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads Ecopath taxonomy information.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function LoadEcopathTaxon() As Boolean

            Dim taxonDS As cTaxonDataStructures = Me.m_core.m_TaxonData
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            taxonDS.NumTaxon = CInt(Me.m_db.GetValue("SELECT COUNT(*) FROM EcopathTaxon"))
            taxonDS.RedimTaxon()

            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathTaxon ORDER BY TaxonID ASC")
            Dim iTaxon As Integer = 1
            Dim bSucces As Boolean = True

            Try
                While reader.Read()
                    taxonDS.TaxonDBID(iTaxon) = CInt(reader("TaxonID"))
                    taxonDS.TaxonCodeSAUP(iTaxon) = CLng(Me.m_db.ReadSafe(reader, "CodeSAUP", cCore.NULL_VALUE))
                    taxonDS.TaxonCodeFB(iTaxon) = CLng(Me.m_db.ReadSafe(reader, "CodeFB", cCore.NULL_VALUE))
                    taxonDS.TaxonCodeSLB(iTaxon) = CLng(Me.m_db.ReadSafe(reader, "CodeSLB", cCore.NULL_VALUE))
                    taxonDS.TaxonCodeFAO(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "CodeFAO", ""))
                    taxonDS.TaxonCodeLSID(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "CodeLCID", ""))
                    taxonDS.TaxonClass(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "ClassName", ""))
                    taxonDS.TaxonOrder(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "OrderName", ""))
                    taxonDS.TaxonFamily(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "FamilyName", ""))
                    taxonDS.TaxonGenus(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "GenusName", ""))
                    taxonDS.TaxonSpecies(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "SpeciesName", ""))
                    taxonDS.TaxonName(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "CommonName", ""))
                    taxonDS.TaxonSource(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "SourceName", ""))
                    taxonDS.TaxonSourceKey(iTaxon) = CStr(Me.m_db.ReadSafe(reader, "SourceKey", ""))
                    taxonDS.TaxonEcologyType(iTaxon) = DirectCast(Me.m_db.ReadSafe(reader, "EcologyType", eEcologyTypes.NotSet), eEcologyTypes)
                    taxonDS.TaxonOrganismType(iTaxon) = DirectCast(Me.m_db.ReadSafe(reader, "OrganismType", eOrganismTypes.NotSet), eOrganismTypes)
                    taxonDS.TaxonIUCNConservationStatus(iTaxon) = DirectCast(Me.m_db.ReadSafe(reader, "ConservationStatus", eIUCNConservationStatusTypes.NotSet), eIUCNConservationStatusTypes)
                    taxonDS.TaxonExploitationStatus(iTaxon) = DirectCast(CInt(CByte(Me.m_db.ReadSafe(reader, "Exploited", eExploitationTypes.NotSet))), eExploitationTypes)
                    taxonDS.TaxonOccurrenceStatus(iTaxon) = DirectCast(Me.m_db.ReadSafe(reader, "OccurrenceStatus", eOccurrenceStatusTypes.NotSet), eOccurrenceStatusTypes)
                    taxonDS.TaxonMeanWeight(iTaxon) = CSng(Me.m_db.ReadSafe(reader, "MeanWeight", cCore.NULL_VALUE))
                    taxonDS.TaxonMeanLength(iTaxon) = CSng(Me.m_db.ReadSafe(reader, "MeanLength", cCore.NULL_VALUE))
                    taxonDS.TaxonMaxLength(iTaxon) = CSng(Me.m_db.ReadSafe(reader, "MaxLength", cCore.NULL_VALUE))
                    taxonDS.TaxonWinf(iTaxon) = CSng(Me.m_db.ReadSafe(reader, "Winf", cCore.NULL_VALUE))
                    taxonDS.TaxonK(iTaxon) = CSng(Me.m_db.ReadSafe(reader, "vbgfK", cCore.NULL_VALUE))
                    taxonDS.TaxonMaxLength(iTaxon) = CSng(Me.m_db.ReadSafe(reader, "MaxLength", cCore.NULL_VALUE))
                    taxonDS.TaxonMeanLifeSpan(iTaxon) = CSng(Me.m_db.ReadSafe(reader, "MeanLifeSpan", cCore.NULL_VALUE))
                    taxonDS.TaxonVulnerabilityIndex(iTaxon) = CInt(Me.m_db.ReadSafe(reader, "VulnerabiltyIndex", cCore.NULL_VALUE))
                    taxonDS.TaxonLastUpdated(iTaxon) = CDbl(Me.m_db.ReadSafe(reader, "LastUpdated", -1))
                    iTaxon += 1
                End While

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading taxon {1}", ex.Message, iTaxon))
                bSucces = False
            End Try

            Debug.Assert(iTaxon - 1 = taxonDS.NumTaxon)

            Me.m_db.ReleaseReader(reader)
            reader = Nothing

            ' Read taxa assignments
            bSucces = bSucces And Me.LoadEcopathGroupTaxon()
            bSucces = bSucces And Me.LoadEcopathStanzaTaxon()

            Return bSucces

        End Function

        Private Function LoadEcopathGroupTaxon() As Boolean

            Dim taxonDS As cTaxonDataStructures = Me.m_core.m_TaxonData
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathGroupTaxon")
            Dim iTaxon As Integer = 1
            Dim iGroup As Integer = 1
            Dim bSucces As Boolean = True

            Try
                While reader.Read()
                    iTaxon = Array.IndexOf(taxonDS.TaxonDBID, CInt(reader("TaxonID")))
                    iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("EcopathGroupID")))

                    If (iTaxon > 0 And iGroup > 0) Then
                        taxonDS.TaxonTarget(iTaxon) = iGroup
                        taxonDS.IsTaxonStanza(iTaxon) = False
                        taxonDS.TaxonPropBiomass(iTaxon) = CSng(reader("Proportion"))
                        taxonDS.TaxonPropCatch(iTaxon) = CSng(Me.m_db.ReadSafe(reader, "PropCatch", 0))
                    End If
                End While

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading taxon {1}, group {2}", ex.Message, iTaxon, iGroup))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)
            Return bSucces

        End Function

        Private Function LoadEcopathStanzaTaxon() As Boolean

            Dim taxonDS As cTaxonDataStructures = Me.m_core.m_TaxonData
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathStanzaTaxon")
            Dim iTaxon As Integer = 1
            Dim iStanza As Integer = 1
            Dim bSucces As Boolean = True

            Try
                While reader.Read()
                    iTaxon = Array.IndexOf(taxonDS.TaxonDBID, CInt(reader("TaxonID")))
                    iStanza = Array.IndexOf(stanzaDS.StanzaDBID, CInt(reader("StanzaID")))

                    If (iTaxon > 0 And iStanza > 0) Then
                        taxonDS.TaxonTarget(iTaxon) = iStanza
                        taxonDS.IsTaxonStanza(iTaxon) = True
                        taxonDS.TaxonPropBiomass(iTaxon) = 1
                        taxonDS.TaxonPropCatch(iTaxon) = 1
                    End If
                End While

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading taxon {1}, stanza {2}", ex.Message, iTaxon, iStanza))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)
            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update group info in the data source.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function SaveEcopathTaxon() As Boolean

            Dim taxonDS As cTaxonDataStructures = Me.m_core.m_TaxonData
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim nTaxonSaved As Integer = 0
            Dim bSucces As Boolean = True

            bSucces = Me.m_db.Execute("DELETE FROM EcopathStanzaTaxon")
            bSucces = bSucces And Me.m_db.Execute("DELETE FROM EcopathGroupTaxon")
            bSucces = bSucces And Me.m_db.Execute("DELETE FROM EcopathTaxon")

            If (bSucces) Then
                writer = Me.m_db.GetWriter("EcopathTaxon")

                Try
                    For iTaxon As Integer = 1 To taxonDS.NumTaxon

                        drow = writer.NewRow()
                        drow("TaxonID") = taxonDS.TaxonDBID(iTaxon)
                        drow("CodeSAUP") = taxonDS.TaxonCodeSAUP(iTaxon)
                        drow("CodeFB") = taxonDS.TaxonCodeFB(iTaxon)
                        drow("CodeSLB") = taxonDS.TaxonCodeSLB(iTaxon)
                        drow("CodeFAO") = taxonDS.TaxonCodeFAO(iTaxon)
                        drow("CodeLCID") = taxonDS.TaxonCodeLSID(iTaxon)
                        drow("ClassName") = taxonDS.TaxonClass(iTaxon)
                        drow("OrderName") = taxonDS.TaxonOrder(iTaxon)
                        drow("FamilyName") = taxonDS.TaxonFamily(iTaxon)
                        drow("GenusName") = taxonDS.TaxonGenus(iTaxon)
                        drow("SpeciesName") = taxonDS.TaxonSpecies(iTaxon)
                        drow("CommonName") = taxonDS.TaxonName(iTaxon)
                        drow("SourceName") = taxonDS.TaxonSource(iTaxon)
                        drow("SourceKey") = taxonDS.TaxonSourceKey(iTaxon)
                        drow("EcologyType") = taxonDS.TaxonEcologyType(iTaxon)
                        drow("OrganismType") = taxonDS.TaxonOrganismType(iTaxon)
                        drow("ConservationStatus") = taxonDS.TaxonIUCNConservationStatus(iTaxon)
                        drow("Exploited") = taxonDS.TaxonExploitationStatus(iTaxon)
                        drow("OccurrenceStatus") = taxonDS.TaxonOccurrenceStatus(iTaxon)
                        drow("MeanWeight") = taxonDS.TaxonMeanWeight(iTaxon)
                        drow("MeanLength") = taxonDS.TaxonMeanLength(iTaxon)
                        drow("MaxLength") = taxonDS.TaxonMaxLength(iTaxon)
                        drow("Winf") = taxonDS.TaxonWinf(iTaxon)
                        drow("vbgfK") = taxonDS.TaxonK(iTaxon)
                        drow("MeanLifeSpan") = taxonDS.TaxonMeanLifeSpan(iTaxon)
                        drow("VulnerabiltyIndex") = taxonDS.TaxonVulnerabilityIndex(iTaxon)
                        drow("LastUpdated") = taxonDS.TaxonLastUpdated(iTaxon)
                        writer.AddRow(drow)

                        nTaxonSaved += 1
                    Next iTaxon

                Catch ex As Exception
                    Me.LogMessage(String.Format("Error {0} occurred while saving EcopathTaxa", ex.Message))
                    bSucces = False
                End Try
            End If

            Debug.Assert(nTaxonSaved = taxonDS.NumTaxon)

            Me.m_db.ReleaseWriter(writer, bSucces)

            bSucces = bSucces And Me.SaveEcopathGroupTaxon()
            bSucces = bSucces And Me.SaveEcopathStanzaTaxon()

            Return bSucces

        End Function

        Private Function SaveEcopathGroupTaxon() As Boolean

            Dim taxonDS As cTaxonDataStructures = Me.m_core.m_TaxonData
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            bSucces = Me.m_db.Execute("DELETE FROM EcopathGroupTaxon")
            writer = Me.m_db.GetWriter("EcopathGroupTaxon")

            Try
                For iTaxon As Integer = 1 To taxonDS.NumTaxon
                    If Not taxonDS.IsTaxonStanza(iTaxon) Then
                        drow = writer.NewRow()
                        drow("TaxonID") = taxonDS.TaxonDBID(iTaxon)
                        drow("EcopathGroupID") = ecopathDS.GroupDBID(taxonDS.TaxonTarget(iTaxon))
                        drow("Proportion") = taxonDS.TaxonPropBiomass(iTaxon)
                        drow("PropCatch") = taxonDS.TaxonPropCatch(iTaxon)
                        writer.AddRow(drow)
                    End If
                Next iTaxon

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving EcopathGroupTaxon", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, bSucces)

            Return bSucces
        End Function

        Private Function SaveEcopathStanzaTaxon() As Boolean

            Dim taxonDS As cTaxonDataStructures = Me.m_core.m_TaxonData
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            bSucces = Me.m_db.Execute("DELETE FROM EcopathStanzaTaxon")
            writer = Me.m_db.GetWriter("EcopathStanzaTaxon")

            Try
                For iTaxon As Integer = 1 To taxonDS.NumTaxon
                    If taxonDS.IsTaxonStanza(iTaxon) Then
                        drow = writer.NewRow()
                        drow("TaxonID") = taxonDS.TaxonDBID(iTaxon)
                        drow("StanzaID") = stanzaDS.StanzaDBID(taxonDS.TaxonTarget(iTaxon))
                        writer.AddRow(drow)
                    End If
                Next iTaxon

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving EcopathStanzaTaxon", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, bSucces)

            Return bSucces
        End Function

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="IEcopathDataSource.AddTaxon" />
        ''' -------------------------------------------------------------------
        Public Function AddTaxon(ByVal iTargetDBID As Integer,
                                 ByVal bIsStanza As Boolean,
                                 ByVal data As ITaxonSearchData,
                                 ByVal sProportion As Single,
                                 ByRef iDBID As Integer) As Boolean _
            Implements IEcopathDataSource.AddTaxon

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' TaxonID unique for all scenarios
            iDBID = CInt(Me.m_db.GetValue("SELECT MAX(TaxonID) FROM EcopathTaxon", 0)) + 1

            writer = Me.m_db.GetWriter("EcopathTaxon")

            Try
                drow = writer.NewRow()

                drow("TaxonID") = iDBID
                drow("CodeFAO") = ""
                drow("CodeLCID") = ""
                drow("ClassName") = ""
                drow("OrderName") = ""
                drow("FamilyName") = ""
                drow("GenusName") = ""
                drow("SpeciesName") = ""
                drow("CommonName") = ""
                drow("SourceName") = ""
                drow("SourceKey") = ""
                drow("Winf") = 0
                drow("vbgfK") = 0

                If (data IsNot Nothing) Then
                    drow("CodeSAUP") = data.CodeSAUP
                    drow("CodeSLB") = data.CodeSLB
                    drow("CodeFB") = data.CodeFB
                    drow("CodeFAO") = data.CodeFAO
                    drow("CodeLCID") = data.CodeLSID
                    drow("ClassName") = data.Class
                    drow("OrderName") = data.Order
                    drow("FamilyName") = data.Family
                    drow("GenusName") = data.Genus
                    drow("SpeciesName") = data.Species
                    drow("CommonName") = data.Common
                    drow("SourceName") = data.Source
                    drow("SourceKey") = data.SourceKey

                    ' Add bonus data if available
                    If TypeOf (data) Is ITaxonDetailsData Then
                        Dim dataDetails As ITaxonDetailsData = DirectCast(data, ITaxonDetailsData)
                        drow("EcologyType") = dataDetails.EcologyType
                        drow("OrganismType") = dataDetails.OrganismType
                        drow("ConservationStatus") = dataDetails.IUCNConservationStatus
                        drow("OccurrenceStatus") = dataDetails.OccurrenceStatus
                        drow("MeanWeight") = dataDetails.MeanWeight
                        drow("MeanLength") = dataDetails.MeanLength
                        drow("MaxLength") = dataDetails.MaxLength
                        drow("MeanLifeSpan") = dataDetails.MeanLifespan
                        drow("Winf") = dataDetails.Winf
                        drow("vbgfK") = dataDetails.vbgfK
                        drow("VulnerabiltyIndex") = dataDetails.VulnerabilityIndex
                    End If
                End If
                drow("LastUpdated") = cDateUtils.DateToJulian()

                writer.AddRow(drow)
                writer.Commit()
                bSucces = Me.m_db.ReleaseWriter(writer, bSucces)
            Catch ex As Exception
                bSucces = False
                Me.LogMessage(String.Format("Error {0} occurred while adding taxon", ex.Message))
            End Try

            Try

                If Not bIsStanza Then
                    writer = Me.m_db.GetWriter("EcopathGroupTaxon")
                    drow = writer.NewRow()
                    drow("TaxonID") = iDBID
                    drow("EcopathGroupID") = iTargetDBID
                    drow("Proportion") = sProportion
                    drow("PropCatch") = sProportion
                    writer.AddRow(drow)
                    bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)
                Else
                    writer = Me.m_db.GetWriter("EcopathStanzaTaxon")
                    drow = writer.NewRow()
                    drow("TaxonID") = iDBID
                    drow("StanzaID") = iTargetDBID
                    writer.AddRow(drow)
                    writer.Commit()
                    bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)
                End If
            Catch ex As Exception
                bSucces = False
                Me.LogMessage(String.Format("Error {0} occurred while adding taxon", ex.Message))
            End Try

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="IEcopathDataSource.RemoveTaxon" />
        ''' -------------------------------------------------------------------
        Public Function RemoveTaxon(ByVal iTaxonID As Integer) As Boolean _
            Implements IEcopathDataSource.RemoveTaxon

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim bSucces As Boolean = True

            Try
                Me.m_db.Execute(String.Format("DELETE FROM EcopathGroupTaxon WHERE (TaxonID={0})", iTaxonID))
                Me.m_db.Execute(String.Format("DELETE FROM EcopathStanzaTaxon WHERE (TaxonID={0})", iTaxonID))
                Me.m_db.Execute(String.Format("DELETE FROM EcopathTaxon WHERE (TaxonID={0})", iTaxonID))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing taxon {1}", ex.Message, iTaxonID))
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Modify

#End Region ' Taxa

#End Region ' Ecopath

#Region " EcoSim "

#Region " Diagnostics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if the data source has unsaved changes for Ecosim.
        ''' </summary>
        ''' <returns>True if the data source has pending changes for Ecosim.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsEcosimModified() As Boolean Implements DataSources.IEcosimDatasource.IsEcosimModified

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData

            ' Hmm, maybe the data source should have a better way to 'remember' whether a sim scenario has been loaded.
            If Not Me.IsConnected() Then Return False
            If ecopathDS.ActiveEcosimScenario < 0 Then Return False
            Return Me.IsChanged(s_EcosimComponents)

        End Function

#End Region ' Diagnostics

#Region " Model "

        Private Function LoadEcosimModel() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcosimModel")
            Dim bSuccess As Boolean = True

            If (reader IsNot Nothing) Then
                Try
                    reader.Read()
                    ecosimDS.ForcePoints = CInt(Me.m_db.ReadSafe(reader, "ForcePoints", cEcosimDatastructures.DEFAULT_N_FORCINGPOINTS))
                Catch ex As Exception
                    bSuccess = False
                End Try

                ecosimDS.nGroups = ecopathDS.NumGroups

                Me.m_db.ReleaseReader(reader)
            End If
            Return bSuccess

        End Function

        Private Function SaveEcosimModel() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bSuccess As Boolean = True

            ' Abort if there is no active scenario
            If ecopathDS.ActiveEcosimScenario <= 0 Then Return False

            writer = Me.m_db.GetWriter("EcosimModel")
            dt = writer.GetDataTable()
            Try
                drow = dt.Rows(0)
                drow.BeginEdit()
                drow("ForcePoints") = ecosimDS.ForcePoints
                drow.EndEdit()
            Catch ex As Exception
                bSuccess = False
            End Try

            Me.m_db.ReleaseWriter(writer)
            Return bSuccess

        End Function

#End Region ' Model

#Region " Scenarios "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads an Ecosim scenario from the DB.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID of the scenario to load.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Friend Function LoadEcosimScenario(ByVal iScenarioID As Integer) As Boolean _
                Implements IEcosimDatasource.LoadEcosimScenario

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True

            bSucces = Me.LoadEcosimModel()

            ecosimDS.RedimVars()
            ecosimDS.SetDefaultParameters()
            mseDS.RedimVars()
            mseDS.setDefaultRegValues()

            'jb 11-Oct-2012 Add MSY data
            Me.m_core.m_MSYData.RedimVars()

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenario WHERE (ScenarioID={0})", iScenarioID))
            Try
                ' Read the one record
                reader.Read()

                ecosimDS.NumYears = CInt(reader("TotalTime"))
                ecosimDS.StepSize = CSng(reader("StepSize"))
                ecosimDS.EquilibriumStepSize = CSng(reader("EquilibriumStepSize"))
                ecosimDS.EquilScaleMax = CSng(reader("EquilScaleMax"))
                ecosimDS.SorWt = CSng(reader("sorwt"))
                ecosimDS.SystemRecovery = CSng(reader("SystemRecovery"))
                ecosimDS.Discount = CSng(reader("Discount"))

                'ecosimDS.NudgeStart = CSng(reader("NudgeStart"))
                'ecosimDS.NudgeEnd = CSng(reader("NudgeEnd"))
                'ecosimDS.NudgeFactor = CSng(reader("NudgeFactor"))
                'ecosimDS.DoInteg = CSng(reader("DoInteg"))
                'ecosimDS.chkNudge = CBool(reader("UseNudge"))

                'drow("NMed") = Me.FixValue(reader("NMed"))                        ' DISCONTINUED
                'drow("NMedPoints") = Me.FixValue(reader("NMedPoints"))            ' DISCONTINUED

                ecosimDS.NutBaseFreeProp = CSng(reader("NutBaseFreeProp"))
                ecosimDS.NutPBmax = CSng(reader("NutPBmax"))

                'ecosimDS.UseVarPQ = CBool(reader("UseVarPQ"))
                'VC090403: the var P/Q was being set to true by default, It shouldn't be, this should be done in interface only
                ecosimDS.UseVarPQ = False

                ecosimDS.ForagingTimeLowerLimit = CSng(Me.m_db.ReadSafe(reader, "ForagingTimeLowerLimit", 0.01))

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Scenario {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)

            'jb added to redim time variables in ecosim data structures
            ecosimDS.RedimTime()

            Me.m_core.m_MSEData.redimTime()

            Me.m_core.m_EcoSim.setDefaultCatchabilities()

            ' Set active scenario
            ecopathDS.ActiveEcosimScenario = Array.IndexOf(ecopathDS.EcosimScenarioDBID, iScenarioID)

            bSucces = bSucces And Me.LoadEcosimGroups(iScenarioID)
            bSucces = bSucces And Me.LoadEcosimFleets(iScenarioID)
            bSucces = bSucces And Me.LoadEcosimVulnerabilities()
            bSucces = bSucces And Me.LoadShapes()
            bSucces = bSucces And Me.LoadEcosimMSE(iScenarioID)
            bSucces = bSucces And Me.LoadEcosimCatchabilities(iScenarioID)
            bSucces = bSucces And Me.LoadAuxillaryData()

            Me.ClearChanged(s_EcosimComponents)

            Return bSucces
        End Function

        Friend Function SaveEcosimScenarioAs(ByVal strScenarioName As String, ByVal strDescription As String,
          ByVal strAuthor As String, ByVal strContact As String, ByRef iScenarioID As Integer) As Boolean _
                 Implements IEcosimDatasource.SaveEcosimScenarioAs

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Delete existing scenario
            Me.m_db.Execute(String.Format("DELETE FROM EcosimScenario WHERE ScenarioName='{0}'", strScenarioName))

            iScenarioID = CInt(Me.m_db.GetValue("SELECT MAX(ScenarioID) FROM EcosimScenario", 0)) + 1

            Try
                writer = Me.m_db.GetWriter("EcosimScenario")
                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("ScenarioName") = strScenarioName
                drow("Description") = strDescription
                drow("Author") = strAuthor
                drow("Contact") = strContact
                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer)
            Catch ex As Exception
                bSucces = False
            End Try

            Return (bSucces And Me.SaveEcosimScenario(iScenarioID))

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the current active Ecosim scenario in the data source under
        ''' a given database ID.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID to save the current scenario to.
        ''' If this parameter is left blank, the current scenario is saved
        ''' under its own database ID.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Friend Function SaveEcosimScenario(ByVal iScenarioID As Integer) As Boolean _
                Implements IEcosimDatasource.SaveEcosimScenario

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData

            If Not Me.SaveEcosimModel() Then Return False

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenario As Integer = Array.IndexOf(ecopathDS.EcosimScenarioDBID, iScenarioID)
            Dim iActiveScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim bDuplicating As Boolean = False
            Dim bSucces As Boolean = True
            Dim idm As New cIDMappings()

            If (iScenarioID = 0) Then iScenarioID = iActiveScenarioID

            ' Duplicating a scenario?
            If (iScenarioID <> iActiveScenarioID) Then
                ' #Yes: add ID mapping to allow copying of scenario content
                idm.Add(eDataTypes.EcoSimScenario, iActiveScenarioID, iScenarioID)
                bDuplicating = True
            End If

            bSucces = Me.m_db.BeginTransaction()

            Try

                writer = Me.m_db.GetWriter("EcosimScenario")
                dt = writer.GetDataTable()
                drow = dt.Rows.Find(iScenarioID)

                drow("TotalTime") = ecosimDS.NumYears
                drow("StepSize") = ecosimDS.StepSize
                drow("EquilibriumStepSize") = ecosimDS.EquilibriumStepSize
                drow("EquilScaleMax") = ecosimDS.EquilScaleMax
                drow("sorwt") = ecosimDS.SorWt
                drow("SystemRecovery") = ecosimDS.SystemRecovery
                drow("Discount") = ecosimDS.Discount

                'drow("NudgeStart") = ecosimDS.NudgeStart
                'drow("NudgeEnd") = ecosimDS.NudgeEnd 
                'drow("NudgeFactor") = ecosimDS.NudgeFactor
                'drow("DoInteg") = ecosimDS.DoInteg 
                'drow("UseNudge") = ecosimDS.chkNudge

                drow("NutBaseFreeProp") = ecosimDS.NutBaseFreeProp
                drow("NutForcingShapeID") = ecosimDS.ForcingDBIDs(ecosimDS.NutForceNumber)
                drow("NutPBmax") = ecosimDS.NutPBmax
                'drow("UseVarPQ") = ecosimDS.UseVarPQ
                ' ------------------------------------------
                drow("LastSaved") = cDateUtils.DateToJulian()
                drow("LastSavedVersion") = cAssemblyUtils.GetVersion().ToString

                drow("ForagingTimeLowerLimit") = ecosimDS.ForagingTimeLowerLimit

                ' Save changes
                Me.m_db.ReleaseWriter(writer)

                ' Duplicate aux. data
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcoSimScenario, iActiveScenarioID)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcoSimModelParameter, iActiveScenarioID)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving Scenario {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try

            bSucces = bSucces And Me.SaveEcosimGroups(idm)
            bSucces = bSucces And Me.SaveEcosimFleets(idm)
            bSucces = bSucces And Me.SaveEcosimVulnerabilities(idm)

            If bDuplicating Or Me.IsChanged(New eCoreComponentType() {eCoreComponentType.ShapesManager}) Or Me.IsChanged(New eCoreComponentType() {eCoreComponentType.EcoSim}) Then
                bSucces = bSucces And Me.SaveShapes(idm)
                bSucces = bSucces And Me.SaveShapeAssignments(idm)
            End If

            If bDuplicating Or Me.IsChanged(New eCoreComponentType() {eCoreComponentType.TimeSeries}) Then
                bSucces = bSucces And Me.SaveTimeSeries(idm)
            End If
            If bDuplicating Or Me.IsChanged(New eCoreComponentType() {eCoreComponentType.MSE}) Then
                bSucces = bSucces And Me.SaveEcosimMSE(idm)
            End If

            ' ToDo: only save these when modified or duplicating
            bSucces = bSucces And Me.SaveEcosimCapacityDrivers(idm)
            bSucces = bSucces And SaveEcosimCatchabilities(idm)

            If bSucces Then
                ' Commit save
                bSucces = Me.m_db.CommitTransaction(True)
            Else
                Me.m_db.RollbackTransaction()
            End If

            If (bSucces) Then
                ' Clear changed admin
                Me.ClearChanged(s_EcosimComponents)
                ' Reload ecosim scenario definitions 
                Me.LoadEcosimScenarioDefinitions()
            End If

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a scenario to the DB.
        ''' </summary>
        ''' <param name="strScenarioName">Name to assign to new scenario.</param>
        ''' <param name="strDescription">Description to assign to new scenario.</param>
        ''' <param name="strAuthor">Author to assign to the new scenario.</param>
        ''' <param name="strContact">Contact info to assign to the new scenario.</param>
        ''' <param name="iScenarioID">Database ID assigned to the new scenario.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Friend Function AppendEcosimScenario(ByVal strScenarioName As String, ByVal strDescription As String,
                ByVal strAuthor As String, ByVal strContact As String, ByRef iScenarioID As Integer) As Boolean _
                Implements IEcosimDatasource.AppendEcosimScenario

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim idm As New cIDMappings()
            Dim bSucces As Boolean = True

            Try
                iScenarioID = CInt(Me.m_db.GetValue("SELECT MAX(ScenarioID) FROM EcosimScenario", 0)) + 1

                writer = Me.m_db.GetWriter("EcosimScenario")

                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("ScenarioName") = strScenarioName
                drow("Description") = strDescription
                drow("Author") = strAuthor
                drow("Contact") = strContact
                drow("LastSaved") = cDateUtils.DateToJulian()
                writer.AddRow(drow)

                Me.m_db.ReleaseWriter(writer)

                ' Create ecosim groups for the new scenario
                For i As Integer = 1 To ecopathDS.GroupDBID.Length - 1
                    bSucces = bSucces And Me.CreateRepairEcosimGroup(ecopathDS.GroupDBID(i), iScenarioID)
                Next
                ' Create ecosim fleets for the new scenario
                For i As Integer = 1 To ecopathDS.FleetDBID.Length - 1
                    ' Sanity check to skip the 'all' fleet
                    If ecopathDS.FleetDBID(i) > 0 Then
                        bSucces = bSucces And Me.CreateRepairEcosimFleet(ecopathDS.FleetDBID(i), iScenarioID)
                    End If
                Next

                ' Reload scenario definitions
                bSucces = bSucces And Me.LoadEcosimScenarioDefinitions()

                Me.ClearChanged(s_EcosimComponents)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while appending Scenario {1}", ex.Message, strScenarioName))
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a scenario from the DB.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID of the scenario to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Friend Function RemoveEcosimScenario(ByVal iScenarioID As Integer) As Boolean _
                Implements IEcosimDatasource.RemoveEcosimScenario

            Dim bSucces As Boolean = True

            Try
                ' Delete 'soft links': database links forged by database updates
                '    DB update 6.04022
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioQuota WHERE (ScenarioID={0})", iScenarioID))
                '    DB update 6.07001
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioMSE WHERE (ScenarioID={0})", iScenarioID))
                '    DB probably an even older database update, hmm
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioFleetYear WHERE (ScenarioID={0})", iScenarioID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioGroupYear WHERE (ScenarioID={0})", iScenarioID))
                ' Delete actual scenario
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenario WHERE (ScenarioID={0})", iScenarioID))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing Ecosim scenarioID {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try

            ' Reload scenario definitions
            bSucces = bSucces And Me.LoadEcosimScenarioDefinitions()

            Return bSucces
        End Function

#End Region ' Scenarios

#Region " Groups, fleets "

#Region " Modify "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create or fixes a group in each ecosim scenario
        ''' </summary>
        ''' <param name="iEcopathGroupID">Ecopath Group DBID</param>
        ''' -----------------------------------------------------------------------
        Private Function AddEcosimGroupToAllScenarios(ByVal iEcopathGroupID As Integer) As Boolean

            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True

            Try
                reader = Me.m_db.GetReader("SELECT ScenarioID FROM EcoSimScenario")
                While reader.Read()
                    bSucces = bSucces And CreateRepairEcosimGroup(iEcopathGroupID, CInt(reader("ScenarioID")))
                End While
                Me.m_db.ReleaseReader(reader)
            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create or repair an ecosim group in a given scenario.
        ''' </summary>
        ''' <param name="iEcopathGroupID">Ecopath Group DBID.</param>
        ''' <param name="iScenarioID">Scenario ID to add the group to.</param>
        ''' -----------------------------------------------------------------------
        Private Function CreateRepairEcosimGroup(ByVal iEcopathGroupID As Integer, ByVal iScenarioID As Integer) As Boolean

            Dim readerGroup As IDataReader = Nothing
            Dim writerShape As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerGroup As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerAssgn As cEwEDatabase.cEwEDbWriter = Nothing
            Dim bValueFound As Boolean = False
            Dim bGroupFound As Boolean = False
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True
            Dim iGroupID As Integer = 1
            Dim iFishMortShapeID As Integer = -1

            readerGroup = Me.m_db.GetReader(String.Format("SELECT GroupID, FishMortShapeID FROM EcosimScenarioGroup WHERE (EcopathGroupID={0}) AND (ScenarioID={1})", iEcopathGroupID, iScenarioID))
            If (readerGroup IsNot Nothing) Then

                iGroupID = -1
                iFishMortShapeID = -1
                bGroupFound = False

                Try
                    If readerGroup.Read() Then
                        ' Try to find existing Sim group ID
                        iGroupID = CInt(readerGroup(0))
                        ' Try to find existing Fish mort shape ID
                        iFishMortShapeID = CInt(readerGroup(1))
                        ' It this did not fail we have found a group, whoot! whoot!
                        bGroupFound = True
                    End If
                Catch ex As InvalidOperationException
                    ' Kaboom
                End Try
                Me.m_db.ReleaseReader(readerGroup)
            End If

            ' Resolve group ID
            If (iGroupID <= 0) Then
                iGroupID = CInt(Me.m_db.GetValue("SELECT MAX(GroupID) FROM EcosimScenarioGroup", 0)) + 1
            End If

            ' Resolve Fish mort ID
            If (iFishMortShapeID <= 0) Then
                iFishMortShapeID = CInt(Me.m_db.GetValue("SELECT MAX(ShapeID) FROM EcosimShape", 0)) + 1
            End If

            ' *** Next: Critical bits, create missing entries in DB ***

            ' Already exists in EcosimShape?
            bValueFound = (CInt(Me.m_db.GetValue(String.Format("SELECT ShapeID FROM EcosimShape WHERE (ShapeID={0})", iFishMortShapeID))) > 0)
            If Not bValueFound Then
                Try
                    writerShape = Me.m_db.GetWriter("EcosimShape")
                    drow = writerShape.NewRow()
                    drow("ShapeID") = iFishMortShapeID
                    drow("ShapeType") = eDataTypes.FishMort
                    drow("IsSeasonal") = False
                    writerShape.AddRow(drow)
                    Me.m_db.ReleaseWriter(writerShape)

                    ' Log repair state
                    Me.LogMessage(String.Format("Added missing shape definition {0} for Ecosim group {1}", iFishMortShapeID, iGroupID))

                Catch ex As Exception
                    bSucces = False
                    ' Log failure
                    Me.LogMessage(String.Format("Failed to add shape definition {0} for Ecosim group {1}", iFishMortShapeID, iGroupID), eMessageType.NotSet, eMessageImportance.Critical)
                End Try
            End If

            ' Already exists in EcosimShapeFishMort?
            bValueFound = (CInt(Me.m_db.GetValue(String.Format("SELECT ShapeID FROM EcosimShapeFishMort WHERE (ShapeID={0})", iFishMortShapeID))) > 0)
            If Not bValueFound Then
                Try
                    writerAssgn = Me.m_db.GetWriter("EcosimShapeFishMort")
                    drow = writerAssgn.NewRow()
                    drow("ShapeID") = iFishMortShapeID
                    drow("Title") = String.Format(My.Resources.CoreDefaults.CORE_DEFAULT_FISHMORTSHAPE, iFishMortShapeID)
                    drow("Zscale") = "0"
                    writerAssgn.AddRow(drow)
                    Me.m_db.ReleaseWriter(writerAssgn)

                    ' Log repair state
                    Me.LogMessage(String.Format("Added missing fishing mortality shape {0} for Ecosim group {1}", iFishMortShapeID, iGroupID))

                Catch ex As Exception
                    bSucces = False
                    ' Log failure
                    Me.LogMessage(String.Format("Failed to add fishing mortality shape {0} for Ecosim group {1}", iFishMortShapeID, iGroupID), eMessageType.NotSet, eMessageImportance.Critical)
                End Try
            End If

            If Not bGroupFound Then
                Try
                    writerGroup = Me.m_db.GetWriter("EcosimScenarioGroup")
                    drow = writerGroup.NewRow()
                    drow("ScenarioID") = iScenarioID
                    drow("GroupID") = iGroupID
                    drow("EcopathGroupID") = iEcopathGroupID
                    drow("FishMortShapeID") = iFishMortShapeID
                    writerGroup.AddRow(drow)
                    Me.m_db.ReleaseWriter(writerGroup)

                    ' Log repair state
                    Me.LogMessage(String.Format("Added missing Ecosim group {0}", iGroupID))

                Catch ex As Exception
                    bSucces = False
                    ' Log failure
                    Me.LogMessage(String.Format("Failed to add Ecosim group {0}", iGroupID), eMessageType.NotSet, eMessageImportance.Critical)
                End Try
            End If

            Return bSucces

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create or fixes a fleet in each ecosim scenario
        ''' </summary>
        ''' <param name="iEcopathFleetID">Ecopath fleet DBID</param>
        ''' -----------------------------------------------------------------------
        Private Function AddEcosimFleetToAllScenarios(ByVal iEcopathFleetID As Integer) As Boolean

            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True

            Try
                reader = Me.m_db.GetReader("SELECT ScenarioID FROM EcoSimScenario")
                While reader.Read()
                    bSucces = bSucces And CreateRepairEcosimFleet(iEcopathFleetID, CInt(reader("ScenarioID")))
                End While
                Me.m_db.ReleaseReader(reader)
            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create or repair an ecosim group in a given scenario.
        ''' </summary>
        ''' <param name="iEcopathFleetID">Ecopath Group DBID.</param>
        ''' <param name="iScenarioID">Scenario ID to add the group to.</param>
        ''' -----------------------------------------------------------------------
        Private Function CreateRepairEcosimFleet(ByVal iEcopathFleetID As Integer, ByVal iScenarioID As Integer) As Boolean

            Dim readerFleet As IDataReader = Nothing
            Dim writerFleet As cEwEDatabase.cEwEDbWriter = Nothing
            Dim bFleetFound As Boolean = False
            Dim drow As DataRow = Nothing
            Dim iNextFleetID As Integer = 0
            Dim bSucces As Boolean = True

            iNextFleetID = CInt(Me.m_db.GetValue("SELECT MAX(FleetID) FROM EcosimScenarioFleet", 0)) + 1

            readerFleet = Me.m_db.GetReader(String.Format("SELECT EcopathFleetID FROM EcoSimScenarioFleet WHERE (EcopathFleetID={0}) AND (ScenarioID={1})", iEcopathFleetID, iScenarioID))
            If (readerFleet IsNot Nothing) Then
                bFleetFound = readerFleet.Read()
                Me.m_db.ReleaseReader(readerFleet)
            End If

            ' *** Next: Critical bits, create missing entries in DB ***

            If Not bFleetFound Then
                Try
                    writerFleet = Me.m_db.GetWriter("EcosimScenarioFleet")
                    drow = writerFleet.NewRow()
                    drow("ScenarioID") = iScenarioID
                    drow("EcopathFleetID") = iEcopathFleetID
                    drow("FleetID") = iNextFleetID
                    writerFleet.AddRow(drow)

                    bSucces = bSucces And Me.m_db.ReleaseWriter(writerFleet, True)

                    ' Log repair state
                    Me.LogMessage(String.Format("Added missing Ecosim fleet {0}", iEcopathFleetID))

                Catch ex As Exception
                    bSucces = False
                    ' Log failure
                    Me.LogMessage(String.Format("Failed to add Ecosim fleet {0}", iEcopathFleetID), eMessageType.NotSet, eMessageImportance.Critical)
                End Try
            End If

            Return bSucces

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <para>
        ''' *Sigh*
        ''' </para>
        ''' <para>
        ''' Due to the limited capabilities of Microzork Access SQL, database 
        ''' update-generated foreign keys to fleets and groups cannot cacading 
        ''' delete. Hence, we need to eradicate linked groups and fleets via code.
        ''' </para> 
        ''' </summary>
        ''' <param name="iEcopathFleetID"></param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function RemoveEcosimFleet(ByVal iEcopathFleetID As Integer) As Boolean

            Dim reader As IDataReader = Me.m_db.GetReader(String.Format("SELECT FleetID FROM EcosimScenarioFleet WHERE (EcopathFleetID={0})", iEcopathFleetID))
            Dim iFleetID As Integer = 0
            Dim bSucces As Boolean = True

            ' Year info stored by sim fleet ID, aargh
            Try
                While reader.Read
                    iFleetID = CInt(reader("FleetID"))
                    bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcoSimScenarioFleetYear WHERE FleetID={0}", iFleetID))
                End While
            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)
            reader = Nothing

            bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioQuota WHERE FleetID={0}", iEcopathFleetID))
            bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimTimeSeriesFleet WHERE (FleetID={0})", iEcopathFleetID))
            bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioFleetGroupCatchability WHERE FleetID={0}", iFleetID))
            bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioFleet WHERE EcopathFleetID={0}", iEcopathFleetID))

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Due to the limited capabilities of Microzork Access SQL, database 
        ''' update-generated foreign keys to fleets and groups cannot cascading 
        ''' delete. Hence, we need to eradicate linked groups and fleets via code.
        ''' </summary>
        ''' <param name="iGroupID">DBID of the Ecosim group to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function RemoveEcosimGroup(ByVal iGroupID As Integer) As Boolean
            Dim bSucces As Boolean = True
            Try
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioQuota WHERE EcosimGroupID={0}", iGroupID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioGroupYear WHERE GroupID={0}", iGroupID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioCapacityDrivers WHERE GroupID={0}", iGroupID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioFleetGroupCatchability WHERE GroupID={0}", iGroupID))
                ' Last
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioGroup WHERE GroupID={0}", iGroupID))

            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Modify

#Region " Load "

        Private Function LoadEcosimGroups(ByVal iScenarioID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True
            Dim iEcopathGroup As Integer = 0

            For igroup As Integer = 1 To ecosimDS.nGroups

                ' Me.CreateRepairEcosimGroup(ecopathDS.GroupDBID(j), iScenarioID, True)

                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcoSimScenarioGroup WHERE (ScenarioID={0}) AND (EcopathGroupID={1})", iScenarioID, ecopathDS.GroupDBID(igroup)))

                Try
                    reader.Read()

                    ' Find ecopath group index to store matching ecosim group data at
                    iEcopathGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("EcopathGroupID")))

                    ' Read fields
                    ecosimDS.GroupDBID(iEcopathGroup) = CInt(reader("GroupID"))
                    ecosimDS.PBmaxs(iEcopathGroup) = CSng(reader("pbmaxs"))
                    ecosimDS.FtimeMax(iEcopathGroup) = CSng(reader("FtimeMax"))
                    ecosimDS.FtimeAdjust(iEcopathGroup) = CSng(reader("FtimeAdjust"))
                    ecosimDS.MoPred(iEcopathGroup) = CSng(reader("MoPred"))
                    ecosimDS.FishRateMax(iEcopathGroup) = CSng(reader("FishRateMax"))
                    ' ecosimDS.ShowGroup(i) = CBool(reader("Show"))

                    ecosimDS.RiskTime(iEcopathGroup) = CSng(reader("RiskTime"))
                    ecosimDS.QmQo(iEcopathGroup) = CSng(reader("QmQo"))
                    ecosimDS.CmCo(iEcopathGroup) = CSng(reader("CmCo"))
                    ecosimDS.SwitchPower(iEcopathGroup) = CSng(reader("SwitchPower"))
                    ecosimDS.GroupFishRateNoDBID(iEcopathGroup) = CInt(reader("FishMortShapeID"))

                    mseDS.Blim(iEcopathGroup) = CSng(Me.m_db.ReadSafe(reader, "Blim", mseDS.Blim(iEcopathGroup), cCore.NULL_VALUE))
                    mseDS.Bbase(iEcopathGroup) = CSng(Me.m_db.ReadSafe(reader, "Bbase", mseDS.Bbase(iEcopathGroup), cCore.NULL_VALUE))
                    mseDS.Fopt(iEcopathGroup) = CSng(Me.m_db.ReadSafe(reader, "Fopt", mseDS.Fopt(iEcopathGroup), cCore.NULL_VALUE))
                    mseDS.FixedEscapement(iEcopathGroup) = CSng(Me.m_db.ReadSafe(reader, "FixedEscapement", 0.0!, cCore.NULL_VALUE))
                    mseDS.FixedF(iEcopathGroup) = CSng(Me.m_db.ReadSafe(reader, "FixedF", 0.0!, cCore.NULL_VALUE))

                    mseDS.CVbiomEst(iEcopathGroup) = CSng(Me.m_db.ReadSafe(reader, "BiomassCV", mseDS.CVbiomEst(iEcopathGroup), cCore.NULL_VALUE))
                    mseDS.BioRiskValue(iEcopathGroup, 0) = CSng(Me.m_db.ReadSafe(reader, "LowerRisk", mseDS.BioRiskValue(iEcopathGroup, 0), cCore.NULL_VALUE))
                    mseDS.BioRiskValue(iEcopathGroup, 1) = CSng(Me.m_db.ReadSafe(reader, "UpperRisk", mseDS.BioRiskValue(iEcopathGroup, 1), cCore.NULL_VALUE))

                    mseDS.DefaultBioBounds(iEcopathGroup)
                    mseDS.BioBounds(iEcopathGroup).Lower = CSng(Me.m_db.ReadSafe(reader, "BiomassRefLower", mseDS.BioBounds(iEcopathGroup).Lower, cCore.NULL_VALUE))
                    mseDS.BioBounds(iEcopathGroup).Upper = CSng(Me.m_db.ReadSafe(reader, "BiomassRefUpper", mseDS.BioBounds(iEcopathGroup).Upper, cCore.NULL_VALUE))

                    mseDS.DefaultCatchBoundsGroup(iEcopathGroup)
                    mseDS.CatchGroupBounds(iEcopathGroup).Lower = CSng(Me.m_db.ReadSafe(reader, "CatchRefLower", mseDS.CatchGroupBounds(iEcopathGroup).Lower, cCore.NULL_VALUE))
                    mseDS.CatchGroupBounds(iEcopathGroup).Upper = CSng(Me.m_db.ReadSafe(reader, "CatchRefUpper", mseDS.CatchGroupBounds(iEcopathGroup).Upper, cCore.NULL_VALUE))

                    mseDS.RstockRatio(iEcopathGroup) = CSng(Me.m_db.ReadSafe(reader, "RStockRatio", mseDS.RstockRatio(igroup), cCore.NULL_VALUE))
                    mseDS.RHalfB0Ratio(iEcopathGroup) = CSng(Me.m_db.ReadSafe(reader, "RHalfB0Ratio", mseDS.RHalfB0Ratio(igroup), cCore.NULL_VALUE))
                    mseDS.cvRec(iEcopathGroup) = CSng(Me.m_db.ReadSafe(reader, "RecruitmentCV", mseDS.cvRec(iEcopathGroup), cCore.NULL_VALUE))

                    ' Me.LoadFishMortShape(CInt(reader("FishMortShapeID")), iEcopathGroup)

                Catch ex As Exception
                    Me.LogMessage(String.Format("Error {0} occurred while reading EcoSim group info for group {1}", ex.Message, iEcopathGroup))
                    bSucces = False
                End Try

                Me.m_db.ReleaseReader(reader)
                reader = Nothing
            Next

            bSucces = bSucces And Me.LoadEcosimGroupYear(iScenarioID)

            Return bSucces

        End Function

        Private Function LoadEcosimGroupYear(ByVal iScenarioID As Integer) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim reader As IDataReader = Nothing
            Dim iGroupID As Integer = -1
            Dim iGroup As Integer = -1
            Dim iYear As Integer = -1
            Dim bSucces As Boolean = True

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenarioGroupYear WHERE (ScenarioID={0})", iScenarioID))

            Try
                While reader.Read()
                    iGroupID = CInt(reader("GroupID"))
                    iGroup = Array.IndexOf(ecosimDS.GroupDBID, iGroupID)
                    iYear = CInt(reader("TimeYear"))
                    If (iGroup > 0) And (iGroup <= ecosimDS.nGroups) And
                       (iYear > 0) And (iYear <= mseDS.nYears) Then
                        mseDS.CVBiomT(iGroup, iYear) = CSng(reader("CVBiom"))
                    End If
                End While

            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)

            Return bSucces

        End Function

        Private Function LoadEcosimVulnerabilities() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim reader As IDataReader = Nothing
            Dim iPredator As Integer = 0
            Dim iPrey As Integer = 0
            Dim bSucces As Boolean = True

            For iPredator = 1 To Me.m_core.nGroups
                For iPrey = 1 To Me.m_core.nGroups
                    ecosimDS.VulMult(iPrey, iPredator) = 2.0!
                Next iPrey
            Next iPredator

            Try
                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenarioForcingMatrix WHERE (ScenarioID={0})", iScenarioID))
                While reader.Read()

                    ' Find iPredator
                    iPredator = Array.IndexOf(ecosimDS.GroupDBID, CInt(reader("PredID")))
                    ' Find iPrey
                    iPrey = Array.IndexOf(ecosimDS.GroupDBID, CInt(reader("PreyID")))

                    If (iPredator > -1 And iPrey > -1) Then
                        ecosimDS.VulMult(iPrey, iPredator) = CSng(reader("vulnerability"))
                    End If

                End While
                Me.m_db.ReleaseReader(reader)
                reader = Nothing

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading ForcingMatrix", ex.Message))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function LoadEcosimFleets(ByVal iScenarioID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim reader As IDataReader = Nothing
            Dim iEcopathFleetID As Integer = -1
            Dim iFleetID As Integer = -1
            Dim iShapeID As Integer = -1
            Dim bSucces As Boolean = True
            Dim effort(ecosimDS.NTimes) As Single

            Dim dtNewFleetShapes As New Dictionary(Of Integer, Integer)

            For t As Integer = 0 To ecosimDS.NTimes : effort(t) = 1.0 : Next

            ' For each fleet
            For iFleet As Integer = 1 To ecosimDS.nGear
                Try
                    ' Read shape for this fleet
                    iEcopathFleetID = ecopathDS.FleetDBID(iFleet)
                    reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcoSimScenarioFleet WHERE (ScenarioID={0}) AND (EcopathFleetID={1})", iScenarioID, iEcopathFleetID))
                    reader.Read()
                    iShapeID = CInt(Me.m_db.ReadSafe(reader, "FishRateShapeID", -1))
                    iFleetID = CInt(reader("FleetID"))
                Catch ex As Exception
                    ' A different error occurred: abort!
                    bSucces = False
                End Try

                If iShapeID <= 0 Then
                    ' Define a new effort shape for this fleet
                    Me.AppendShapeImpl(ecopathDS.FleetName(iFleet), eDataTypes.FishingEffort, iShapeID, effort, eShapeFunctionType.NotSet, Nothing)
                    dtNewFleetShapes.Add(iEcopathFleetID, iShapeID)
                End If

                Try
                    ecosimDS.FleetDBID(iFleet) = iFleetID
                    ecosimDS.Epower(iFleet) = CSng(Me.m_db.ReadSafe(reader, "Epower", 3))
                    ecosimDS.PcapBase(iFleet) = CSng(Me.m_db.ReadSafe(reader, "PCapBase", 0.5))
                    ecosimDS.CapDepreciate(iFleet) = CSng(Me.m_db.ReadSafe(reader, "CapDepreciate", 0.06))
                    ecosimDS.CapBaseGrowth(iFleet) = CSng(Me.m_db.ReadSafe(reader, "CapBaseGrowth", 0.2))
                    ecosimDS.EffortConversionFactor(iFleet) = CSng(Me.m_db.ReadSafe(reader, "EffortConversionFactor", 1.0!))

                    mseDS.MaxEffort(iFleet) = CSng(Me.m_db.ReadSafe(reader, "MaxEffort", cCore.NULL_VALUE))
                    mseDS.QuotaType(iFleet) = DirectCast(CInt(Me.m_db.ReadSafe(reader, "QuotaType", 0)), eQuotaTypes)
                    mseDS.CVFest(iFleet) = CSng(Me.m_db.ReadSafe(reader, "CV", mseDS.CVFest(iFleet)))
                    mseDS.Qgrow(iFleet) = CSng(Me.m_db.ReadSafe(reader, "QIncrease", mseDS.Qgrow(iFleet)))

                    mseDS.DefaultCatchBoundsFleet(iFleet)
                    mseDS.CatchFleetBounds(iFleet).Lower = CSng(Me.m_db.ReadSafe(reader, "CatchRefLower", mseDS.CatchFleetBounds(iFleet).Lower))
                    mseDS.CatchFleetBounds(iFleet).Upper = CSng(Me.m_db.ReadSafe(reader, "CatchRefUpper", mseDS.CatchFleetBounds(iFleet).Upper))
                    mseDS.EffortFleetBounds(iFleet).Lower = CSng(Me.m_db.ReadSafe(reader, "EffortRefLower", mseDS.EffortFleetBounds(iFleet).Lower))
                    mseDS.EffortFleetBounds(iFleet).Upper = CSng(Me.m_db.ReadSafe(reader, "EffortRefUpper", mseDS.EffortFleetBounds(iFleet).Upper))
                    'mseDS.MSYEvaluateFleet(iFleet) = (CInt(Me.m_db.ReadSafe(reader, "MSYEvaluateFleet", True)) = 1)

                    LoadFishingRateShape(iShapeID, iFleet)

                Catch ex As Exception
                    bSucces = False
                End Try

                Me.m_db.ReleaseReader(reader)
            Next

            ' Store new shape links
            Dim writer As cEwEDatabase.cEwEDbWriter = Me.m_db.GetWriter("EcosimScenarioFleet")
            Dim dt As DataTable = writer.GetDataTable()
            Dim objKeys() As Object = {iScenarioID, Nothing}
            Dim drow As DataRow = Nothing

            ' Store new IDs
            For Each iEcopathFleetID In dtNewFleetShapes.Keys
                iShapeID = dtNewFleetShapes(iEcopathFleetID)
                objKeys(1) = iEcopathFleetID
                drow = dt.Rows.Find(objKeys)
                ' Check wheter a new row or an existing row
                Debug.Assert(drow IsNot Nothing)
                Try
                    drow.BeginEdit()
                    drow("FishRateShapeID") = iShapeID
                    drow.EndEdit()
                Catch ex As Exception
                    bSucces = False
                End Try
            Next

            Me.m_db.ReleaseWriter(writer)

            bSucces = bSucces And Me.LoadEcosimFleetYear(iScenarioID)
            bSucces = bSucces And Me.LoadEcosimQuota(iScenarioID)

            Return bSucces

        End Function

        Private Function LoadEcosimFleetYear(ByVal iScenarioID As Integer) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim reader As IDataReader = Nothing
            Dim iFleetID As Integer = -1
            Dim iFleet As Integer = -1
            Dim iYear As Integer = -1
            Dim bSucces As Boolean = True

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcoSimScenarioFleetYear WHERE (ScenarioID={0})", iScenarioID))

            Try
                While reader.Read()
                    iFleetID = CInt(reader("FleetID"))
                    iFleet = Array.IndexOf(ecosimDS.FleetDBID, iFleetID)
                    iYear = CInt(reader("TimeYear"))
                    If (iFleet > 0) And (iFleet <= ecosimDS.nGear) And
                       (iYear > 0) And (iYear <= mseDS.nYears) Then
                        mseDS.CVFT(iFleet, iYear) = CSng(reader("CV"))
                    End If
                End While

            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)

            Return bSucces

        End Function

        Private Function LoadEcosimQuota(ByVal iScenarioID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim reader As IDataReader = Nothing
            Dim iFleetID As Integer = -1
            Dim iFleet As Integer = -1
            Dim iGroupID As Integer = -1
            Dim iGroup As Integer = -1
            Dim bSucces As Boolean = True

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcoSimScenarioQuota WHERE (ScenarioID={0})", iScenarioID))

            Try
                While reader.Read()
                    iFleetID = CInt(reader("FleetID"))
                    iFleet = Array.IndexOf(ecopathDS.FleetDBID, iFleetID)

                    iGroupID = CInt(reader("EcosimGroupID"))
                    iGroup = Array.IndexOf(ecosimDS.GroupDBID, iGroupID)

                    If (iFleet > 0) And (iGroup > 0) Then
                        mseDS.Quotashare(iFleet, iGroup) = CSng(Me.m_db.ReadSafe(reader, "QuotaShare", mseDS.Quotashare(iFleet, iGroup)))
                        mseDS.Fweight(iFleet, iGroup) = CSng(Me.m_db.ReadSafe(reader, "FWeight", 1.0))
                    End If
                End While

            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)

            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        Private Function SaveEcosimGroups(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim iScenarioID As Integer = 0
            Dim iNextGroupID As Integer = 0
            Dim iGroupID As Integer = 0
            Dim bSucces As Boolean = True
            Dim objKeys() As Object = {Nothing, Nothing}

            Dim iActiveEcosimScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim bDuplicating As Boolean = (idm.GetID(eDataTypes.EcoSimScenario, iActiveEcosimScenarioID) <> iActiveEcosimScenarioID)
            Dim iNextShapeID As Integer = 0
            Dim iShapeID As Integer = 0

            ' Obtain mapped scenario ID
            iScenarioID = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))

            ' Get next available shape ID
            iNextShapeID = CInt(Me.m_db.GetValue("SELECT MAX(ShapeID) FROM EcoSimShape", 0)) + 1
            ' Get next available group ID
            iNextGroupID = CInt(Me.m_db.GetValue("SELECT MAX(GroupID) FROM EcosimScenarioGroup", 0)) + 1

            ' JS 28may07: Change of strategy. The primary key in table EcosimScenarioGroup has been changed from
            '             (ScenarioID, SimGroupID) to (ScenarioID, PathGroupID) for the simple reason that when
            '             overwriting an existing scenario the new SimGroupIDs are unknown, while PathGroupIDs
            '             are always known. 
            '             This change in primary keys will not jeopardize performance or referential integrity.
            objKeys(0) = iScenarioID

            Try
                writer = Me.m_db.GetWriter("EcosimScenarioGroup")
                dt = writer.GetDataTable()
                For i As Integer = 1 To ecosimDS.nGroups

                    ' Find row for scenario and ecopath ID
                    objKeys(1) = ecopathDS.GroupDBID(i)
                    drow = dt.Rows.Find(objKeys)

                    bNewRow = (drow Is Nothing)
                    If bNewRow Then
                        drow = writer.NewRow()
                        drow("ScenarioID") = iScenarioID
                        drow("EcopathGroupID") = ecopathDS.GroupDBID(i)
                        drow("GroupID") = iNextGroupID
                        iNextGroupID += 1
                    Else
                        drow.BeginEdit()
                    End If

                    iGroupID = CInt(drow("GroupID"))

                    ' Store ecosim group ID mapping now we know it
                    ' JS 12Jul09: group mapping is stored by ECOPATH group ID since this is the only constant
                    '             factor while appending Ecosim scenarios. Above, CreateRepairEcosimGroup is
                    '             called to complement missing Ecosim groups, which will create the groups
                    '             in the database for a given Ecosim scenario but this will not update the
                    '             ecosim datastructures. This caused the ID mapping context to be populated
                    '             with Ecosim IDs for groups from the previous scenario, NOT the new scenario,
                    '             thus creating Ecosim scenarios what were bugged right from the start.
                    idm.Add(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(i), iGroupID)

                    drow("pbmaxs") = ecosimDS.PBmaxs(i)
                    drow("FtimeMax") = ecosimDS.FtimeMax(i)
                    drow("FtimeAdjust") = ecosimDS.FtimeAdjust(i)
                    drow("MoPred") = ecosimDS.MoPred(i)
                    drow("FishRateMax") = ecosimDS.FishRateMax(i)
                    ' drow("Show") = ecosimDS.ShowGroup(i)
                    drow("RiskTime") = ecosimDS.RiskTime(i)
                    drow("QmQo") = ecosimDS.QmQo(i)
                    drow("CmCo") = ecosimDS.CmCo(i)
                    drow("SwitchPower") = ecosimDS.SwitchPower(i)

                    ' JS 01Jan09: mort shapes unique per scenario
                    If bDuplicating Then
                        idm.Add(eDataTypes.FishMort, ecosimDS.GroupFishRateNoDBID(i), iNextShapeID)
                        iNextShapeID += 1
                    End If
                    drow("FishMortShapeID") = idm.GetID(eDataTypes.FishMort, ecosimDS.GroupFishRateNoDBID(i))

                    drow("Blim") = mseDS.Blim(i)
                    drow("Bbase") = mseDS.Bbase(i)
                    drow("Fopt") = mseDS.Fopt(i)
                    drow("FixedEscapement") = mseDS.FixedEscapement(i)
                    drow("FixedF") = mseDS.FixedF(i)

                    drow("BiomassCV") = mseDS.CVbiomEst(i)
                    drow("LowerRisk") = mseDS.BioRiskValue(i, 0)
                    drow("UpperRisk") = mseDS.BioRiskValue(i, 1)
                    drow("BiomassRefLower") = mseDS.BioBounds(i).Lower
                    drow("BiomassRefUpper") = mseDS.BioBounds(i).Upper
                    drow("CatchRefLower") = mseDS.CatchGroupBounds(i).Lower
                    drow("CatchRefUpper") = mseDS.CatchGroupBounds(i).Upper
                    drow("RStockRatio") = mseDS.RstockRatio(i)
                    drow("RHalfB0Ratio") = mseDS.RHalfB0Ratio(i)
                    drow("RecruitmentCV") = mseDS.cvRec(i)

                    If bNewRow Then
                        writer.AddRow(drow)
                    Else
                        drow.EndEdit()
                    End If

                    ' Duplicate aux. data
                    bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(i))
                    bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcoSimGroupOutput, ecopathDS.GroupDBID(i))

                Next i

            Catch ex As Exception
                bSucces = False
            End Try

            bSucces = bSucces And Me.m_db.ReleaseWriter(writer)
            bSucces = bSucces And Me.SaveEcosimGroupYear(idm)

            Return bSucces

        End Function

        Private Function SaveEcosimGroupYear(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim strSQL As String = ""
            Dim iScenarioID As Integer = 0
            Dim bSucces As Boolean = True

            ' Obtain mapped scenario ID
            iScenarioID = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))

            strSQL = String.Format("DELETE FROM EcosimScenarioGroupYear WHERE (ScenarioID={0})", iScenarioID)
            bSucces = Me.m_db.Execute(strSQL)

            Try
                writer = Me.m_db.GetWriter("EcosimScenarioGroupYear")
                For iGroup As Integer = 1 To ecopathDS.NumGroups
                    For iYear As Integer = 1 To mseDS.nYears
                        ' Conjure row
                        drow = writer.NewRow()
                        ' Populate key
                        drow("ScenarioID") = iScenarioID
                        drow("GroupID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iGroup))
                        drow("TimeYear") = iYear
                        ' Write dynamic bit
                        drow("CVBiom") = mseDS.CVBiomT(iGroup, iYear)
                        ' Add new row to the writer
                        writer.AddRow(drow)
                    Next iYear
                Next iGroup
                ' Done
                bSucces = bSucces And Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function SaveEcosimVulnerabilities(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iScenarioID As Integer = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iPredator As Integer = 0
            Dim iPrey As Integer = 0
            Dim iShapeID As Integer = 0
            Dim bSucces As Boolean = True

            Try
                Me.m_db.Execute(String.Format("DELETE FROM EcoSimScenarioForcingMatrix WHERE (ScenarioID={0})", iScenarioID))
                writer = Me.m_db.GetWriter("EcoSimScenarioForcingMatrix")

                For iPredator = 1 To ecosimDS.nGroups
                    For iPrey = 1 To ecosimDS.nGroups

                        If (ecosimDS.SimDC(iPredator, iPrey) > 0) Then
                            drow = writer.NewRow()
                            drow("ScenarioID") = iScenarioID
                            drow("PredID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iPredator))
                            drow("PreyID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iPrey))
                            drow("vulnerability") = ecosimDS.VulMult(iPrey, iPredator)
                            writer.AddRow(drow)
                        End If

                    Next iPrey
                Next iPredator

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces
        End Function

        Private Function SaveEcosimFleets(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim iScenarioID As Integer = 0
            Dim bSucces As Boolean = True
            Dim objKeys() As Object = {Nothing, Nothing}

            Dim iActiveEcosimScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim bDuplicating As Boolean = (idm.GetID(eDataTypes.EcoSimScenario, iActiveEcosimScenarioID) <> iActiveEcosimScenarioID)
            Dim iNextFleetID As Integer = 0
            Dim iFleetID As Integer = 0
            Dim iNextShapeID As Integer = 0
            Dim iShapeID As Integer = 0

            ' Obtain mapped scenario ID
            iScenarioID = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))
            objKeys(0) = iScenarioID

            ' Get next available shape ID
            iNextShapeID = CInt(Me.m_db.GetValue("SELECT MAX(ShapeID) FROM EcoSimShape", 0)) + 1
            ' Get next available fleet ID
            iNextFleetID = CInt(Me.m_db.GetValue("SELECT MAX(FleetID) FROM EcosimScenarioFleet", 0)) + 1

            Try
                writer = Me.m_db.GetWriter("EcosimScenarioFleet")
                dt = writer.GetDataTable()
                For iFleet As Integer = 1 To ecopathDS.NumFleet

                    objKeys(1) = idm.GetID(eDataTypes.FleetInput, ecopathDS.FleetDBID(iFleet))
                    drow = dt.Rows.Find(objKeys)
                    ' Check wheter a new row or an existing row
                    bNewRow = drow Is Nothing
                    ' New row?
                    If bNewRow Then
                        ' #Yes: create new row
                        drow = writer.NewRow()
                        ' Populate PK
                        drow("ScenarioID") = objKeys(0)
                        drow("EcopathFleetID") = objKeys(1)
                        drow("FleetID") = iNextFleetID
                        iNextFleetID += 1
                    Else
                        ' #No: edit the row
                        drow.BeginEdit()
                    End If

                    iFleetID = CInt(drow("FleetID"))
                    idm.Add(eDataTypes.FleetInput, ecopathDS.FleetDBID(iFleet), iFleetID)

                    If bDuplicating Then
                        iShapeID = iNextShapeID
                        iNextShapeID += 1
                    Else
                        iShapeID = CInt(drow("FishRateShapeID"))
                    End If
                    idm.Add(eDataTypes.FishingEffort, ecosimDS.FishRateGearDBID(iFleet), iShapeID)

                    ' Write dynamic bit
                    drow("FishRateShapeID") = iShapeID
                    drow("MaxEffort") = mseDS.MaxEffort(iFleet)
                    drow("QuotaType") = CInt(mseDS.QuotaType(iFleet))
                    drow("Epower") = ecosimDS.Epower(iFleet)
                    drow("PCapBase") = ecosimDS.PcapBase(iFleet)
                    drow("CapDepreciate") = ecosimDS.CapDepreciate(iFleet)
                    drow("CapBaseGrowth") = ecosimDS.CapBaseGrowth(iFleet)
                    drow("EffortConversionFactor") = ecosimDS.EffortConversionFactor(iFleet)

                    drow("CV") = mseDS.CVFest(iFleet)
                    drow("QIncrease") = mseDS.Qgrow(iFleet)
                    drow("CatchRefLower") = mseDS.CatchFleetBounds(iFleet).Lower
                    drow("CatchRefUpper") = mseDS.CatchFleetBounds(iFleet).Upper
                    drow("EffortRefLower") = mseDS.EffortFleetBounds(iFleet).Lower
                    drow("EffortRefUpper") = mseDS.EffortFleetBounds(iFleet).Upper
                    'drow("MSYEvaluateFleet") = CInt(if(mseDS.MSYEvaluateFleet(iFleet), 1, 0))

                    ' Wrap up: was this a new row?
                    If bNewRow Then
                        ' #Yes: add it to the writer
                        writer.AddRow(drow)
                    Else
                        ' #No: done editing
                        drow.EndEdit()
                    End If

                    ' Duplicate aux. data
                    bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcosimFleetInput, ecopathDS.FleetDBID(iFleet))
                    bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcosimFleetOutput, ecopathDS.FleetDBID(iFleet))

                Next iFleet
                ' Done
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            bSucces = bSucces And Me.SaveEcosimFleetYear(idm)
            bSucces = bSucces And Me.SaveEcosimQuota(idm)

            Return bSucces

        End Function

        Private Function SaveEcosimFleetYear(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim strSQL As String = ""
            Dim iScenarioID As Integer = 0
            Dim bSucces As Boolean = True

            ' Obtain mapped scenario ID
            iScenarioID = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))

            strSQL = String.Format("DELETE FROM EcosimScenarioFleetYear WHERE (ScenarioID={0})", iScenarioID)
            bSucces = Me.m_db.Execute(strSQL)

            Try
                writer = Me.m_db.GetWriter("EcosimScenarioFleetYear")
                For iFleet As Integer = 1 To ecopathDS.NumFleet
                    For iYear As Integer = 1 To mseDS.nYears
                        If (mseDS.CVFT(iFleet, iYear) >= 0) Then

                            ' Conjure row
                            drow = writer.NewRow()
                            ' Populate key
                            drow("ScenarioID") = iScenarioID
                            drow("FleetID") = idm.GetID(eDataTypes.FleetInput, ecopathDS.FleetDBID(iFleet))
                            drow("TimeYear") = iYear
                            ' Write dynamic bit
                            drow("CV") = mseDS.CVFT(iFleet, iYear)
                            ' Add new row to the writer
                            writer.AddRow(drow)
                        End If
                    Next iYear
                Next iFleet
                ' Done
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function SaveEcosimQuota(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim strSQL As String = ""
            Dim iScenarioID As Integer = 0
            Dim bSucces As Boolean = True

            ' Obtain mapped scenario ID
            iScenarioID = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))

            strSQL = String.Format("DELETE FROM EcosimScenarioQuota WHERE (ScenarioID={0})", iScenarioID)
            bSucces = Me.m_db.Execute(strSQL)

            Try
                writer = Me.m_db.GetWriter("EcosimScenarioQuota")
                For iFleet As Integer = 1 To ecopathDS.NumFleet
                    For iGroup As Integer = 1 To ecopathDS.NumGroups
                        ' Conjure row
                        drow = writer.NewRow()
                        ' Populate key
                        drow("ScenarioID") = iScenarioID
                        drow("FleetID") = ecopathDS.FleetDBID(iFleet)
                        drow("EcosimGroupID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iGroup))
                        ' Write dynamic bit
                        drow("QuotaShare") = mseDS.Quotashare(iFleet, iGroup)
                        drow("Fweight") = mseDS.Fweight(iFleet, iGroup)
                        ' Add new row to the writer
                        writer.AddRow(drow)
                    Next iGroup
                Next iFleet
                ' Done
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

#End Region ' Save

#End Region ' Groups, fleets

#Region " Forcing and Mediaton shapes "

        Private Function LoadShapes() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim PredPreyMedDS As cMediationDataStructures = Me.m_core.m_EcoSimData.BioMedData
            Dim LandingsMedDS As cMediationDataStructures = Me.m_core.m_EcoSimData.PriceMedData
            Dim CapEnvResMedDS As cMediationDataStructures = Me.m_core.m_EcoSimData.CapEnvResData
            Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim reader As IDataReader = Nothing
            Dim iShapeID As Integer = 0
            Dim shapeDataType As eDataTypes = eDataTypes.NotSet
            Dim iForcingShape As Integer = 0
            Dim iPredPreyMediationShape As Integer = 0
            Dim iLandingsMediationShape As Integer = 0
            Dim iCapEnvResMediationShape As Integer = 0
            Dim iFishingMortShape As Integer = 0
            Dim iFishRateShape As Integer = 0
            Dim bSucces As Boolean = True

            Dim strQuery As String = ""

            strQuery = String.Format("SELECT COUNT(*) FROM EcosimShape WHERE (ShapeType={0} OR ShapeType={1})", CInt(eDataTypes.EggProd), CInt(eDataTypes.Forcing))
            ecosimDS.NumForcingShapes = CInt(Me.m_db.GetValue(strQuery, 0))

            strQuery = String.Format("SELECT COUNT(*) FROM EcosimShape WHERE (ShapeType={0})", CInt(eDataTypes.Mediation))
            PredPreyMedDS.MediationShapes = CInt(Me.m_db.GetValue(strQuery, 0))

            strQuery = String.Format("SELECT COUNT(*) FROM EcosimShape WHERE (ShapeType={0})", CInt(eDataTypes.PriceMediation))
            LandingsMedDS.MediationShapes = CInt(Me.m_db.GetValue(strQuery, 0))

            strQuery = String.Format("SELECT COUNT(*) FROM EcosimShape WHERE (ShapeType={0})", CInt(eDataTypes.CapacityMediation))
            CapEnvResMedDS.MediationShapes = CInt(Me.m_db.GetValue(strQuery, 0))

            ecosimDS.DimForcingShapes()
            ecosimDS.InitForcingShapes()
            PredPreyMedDS.ReDimMediation(ecosimDS.nGroups, ecosimDS.nGear)
            LandingsMedDS.ReDimMediation(ecosimDS.nGroups, ecosimDS.nGear)
            CapEnvResMedDS.ReDimMediation(ecosimDS.nGroups, ecosimDS.nGear)

            Try

                reader = Me.m_db.GetReader("SELECT * FROM EcosimShape ORDER BY ShapeID ASC")
                While reader.Read()

                    iShapeID = CInt(reader("ShapeID"))
                    shapeDataType = DirectCast(reader("ShapeType"), eDataTypes)

                    Select Case shapeDataType

                        Case eDataTypes.EggProd
                            iForcingShape += 1
                            bSucces = bSucces And Me.LoadEggShape(iShapeID, iForcingShape, CInt(reader("IsSeasonal")) <> 0)

                        Case eDataTypes.Forcing
                            iForcingShape += 1
                            bSucces = bSucces And Me.LoadTimeShape(iShapeID, iForcingShape, CInt(reader("IsSeasonal")) <> 0)

                        Case eDataTypes.Mediation
                            iPredPreyMediationShape += 1
                            bSucces = bSucces And Me.LoadMediationShape(iShapeID, iPredPreyMediationShape, PredPreyMedDS)

                        Case eDataTypes.PriceMediation
                            iLandingsMediationShape += 1
                            bSucces = bSucces And Me.LoadMediationShape(iShapeID, iLandingsMediationShape, LandingsMedDS)

                        Case eDataTypes.CapacityMediation
                            iCapEnvResMediationShape += 1
                            bSucces = bSucces And Me.LoadMediationShape(iShapeID, iCapEnvResMediationShape, CapEnvResMedDS)

                        Case eDataTypes.FishingEffort
                            ' Shape type loaded from LoadEcosimFleets(); do not handle here

                        Case eDataTypes.FishMort
                            ' Shape type loaded from LoadEcosimGroups(); do not handle here

                        Case eDataTypes.FleetGroupCatchability
                            ' Shape type loaded elsewhere

                        Case Else
                            Debug.Assert(False, String.Format("Cannot load invalid shapetype {0} for shape ID {1}", shapeDataType, iShapeID))

                    End Select

                End While
                Me.m_db.ReleaseReader(reader)
                reader = Nothing

            Catch ex As Exception
                bSucces = False
            End Try

            '' Sanity checks discontinued since core may arbitrarily set ecosimDS.MediationShapes to 9
            'Debug.Assert(ecosimDS.MediationShapes = iMediationShape)

            Try
                ' Read and assign scenario forcing shape number(s)
                reader = Me.m_db.GetReader(String.Format("SELECT NutForcingShapeID FROM EcosimScenario WHERE (ScenarioID={0})", iScenarioID))
                reader.Read()
                iForcingShape = CInt(Me.m_db.ReadSafe(reader, "NutForcingShapeID", 0))
                ecosimDS.NutForceNumber = Math.Max(0, Array.IndexOf(ecosimDS.ForcingDBIDs, iForcingShape))
                Me.m_db.ReleaseReader(reader)
                reader = Nothing
            Catch ex As Exception
                bSucces = False
            End Try

            bSucces = bSucces And Me.LoadPredPreyInteractions()
            bSucces = bSucces And Me.LoadLandingInteractions()
            bSucces = bSucces And Me.LoadMediationWeights()
            bSucces = bSucces And Me.LoadStanzaShapeAssignments()
            bSucces = bSucces And Me.LoadEcosimCapacityDrivers()

            Return bSucces

        End Function

#Region " Shape load helpers "

        Private Function LoadEggShape(ByVal iShapeID As Integer, ByVal iForcingShape As Integer,
                Optional ByVal bIsSeasonal As Boolean = False) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim shapeParms As New cEcosimDatastructures.ShapeParameters()
            Dim readerShape As IDataReader = Nothing
            Dim astrZScale() As String
            Dim bSucces As Boolean = True

            Try

                readerShape = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimShapeEggProd WHERE (ShapeID={0})", iShapeID))
                readerShape.Read()

                shapeParms.ShapeFunctionType = CLng(Me.m_db.ReadSafe(readerShape, "FunctionType", 0))
                shapeParms.ShapeFunctionParams = cStringUtils.StringToParamArray(CStr(Me.m_db.ReadSafe(readerShape, "FunctionParams", "")))

                ' Read z-scale
                astrZScale = Me.SplitNumberString(CStr(readerShape("Zscale")))
                For ipt As Integer = 1 To Math.Min(ecosimDS.ForcePoints, astrZScale.Length)
                    ecosimDS.zscale(ipt, iForcingShape) = cStringUtils.ConvertToSingle(astrZScale(ipt - 1), 0)
                Next ipt
                For ipt As Integer = Math.Min(ecosimDS.ForcePoints, astrZScale.Length) + 1 To ecosimDS.ForcePoints
                    ecosimDS.zscale(ipt, iForcingShape) = ecosimDS.zscale(ipt Mod cCore.N_MONTHS, iForcingShape)
                Next

                ecosimDS.ForcingShapeParams(iForcingShape) = shapeParms
                ecosimDS.ForcingDBIDs(iForcingShape) = iShapeID
                ecosimDS.ForcingTitles(iForcingShape) = CStr(readerShape("Title")).Trim()
                ecosimDS.ForcingShapeType(iForcingShape) = eDataTypes.EggProd
                ecosimDS.isSeasonal(iForcingShape) = bIsSeasonal

                Me.m_db.ReleaseReader(readerShape)
                readerShape = Nothing

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading EggShape {1}", ex.Message, iShapeID))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function LoadTimeShape(ByVal iShapeID As Integer,
                                       ByVal iForcingShape As Integer,
                                       Optional ByVal bIsSeasonal As Boolean = False) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim shapeParms As New cEcosimDatastructures.ShapeParameters()
            Dim readerShape As IDataReader = Nothing
            Dim astrZScale() As String
            Dim bSucces As Boolean = True

            Try
                readerShape = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimShapeTime WHERE (ShapeID={0})", iShapeID))
                readerShape.Read()

                shapeParms.ShapeFunctionType = CLng(Me.m_db.ReadSafe(readerShape, "FunctionType", 0))
                shapeParms.ShapeFunctionParams = cStringUtils.StringToParamArray(CStr(Me.m_db.ReadSafe(readerShape, "FunctionParams", "")))

                ' Read z-scale
                Dim sLast As Single = 1.0!
                astrZScale = Me.SplitNumberString(CStr(readerShape("Zscale")))
                For ipt As Integer = 1 To Math.Min(ecosimDS.ForcePoints, astrZScale.Length)
                    sLast = cStringUtils.ConvertToSingle(astrZScale(ipt - 1), 0)
                    ecosimDS.zscale(ipt, iForcingShape) = sLast
                Next ipt
                For ipt As Integer = Math.Min(ecosimDS.ForcePoints, astrZScale.Length) + 1 To ecosimDS.ForcePoints
                    ecosimDS.zscale(ipt, iForcingShape) = sLast
                Next

                ecosimDS.ForcingShapeParams(iForcingShape) = shapeParms
                ecosimDS.ForcingDBIDs(iForcingShape) = iShapeID
                ecosimDS.ForcingTitles(iForcingShape) = CStr(readerShape("Title")).Trim()
                ecosimDS.ForcingShapeType(iForcingShape) = eDataTypes.Forcing
                ecosimDS.isSeasonal(iForcingShape) = bIsSeasonal

                Me.m_db.ReleaseReader(readerShape)
                readerShape = Nothing

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading TimeShape {1}", ex.Message, iShapeID))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function LoadMediationShape(ByVal iShapeID As Integer,
                                            ByVal iMediationShape As Integer,
                                            ByVal medData As cMediationDataStructures) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim shapeParms As New cEcosimDatastructures.ShapeParameters()
            Dim readerShape As IDataReader = Nothing
            Dim astrZScale() As String
            Dim bSucces As Boolean = True

            Try

                readerShape = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimShapeMediation WHERE (ShapeID={0})", iShapeID))
                readerShape.Read()

                shapeParms.ShapeFunctionType = CLng(Me.m_db.ReadSafe(readerShape, "FunctionType", 0))
                shapeParms.ShapeFunctionParams = cStringUtils.StringToParamArray(CStr(Me.m_db.ReadSafe(readerShape, "FunctionParams", "")))

                ' Read z-scale
                astrZScale = Me.SplitNumberString(CStr(readerShape("Zscale")))
                ' Write points
                For ipt As Integer = 1 To Math.Min(medData.NMedPoints, astrZScale.Length)
                    medData.Medpoints(ipt, iMediationShape) = cStringUtils.ConvertToSingle(astrZScale(ipt - 1), 0)
                Next ipt
                For ipt As Integer = Math.Min(medData.NMedPoints, astrZScale.Length) + 1 To medData.NMedPoints
                    medData.Medpoints(ipt, iMediationShape) = 1.0
                Next

                medData.MediationShapeParams(iMediationShape) = shapeParms
                medData.MediationDBIDs(iMediationShape) = iShapeID
                medData.MediationTitles(iMediationShape) = CStr(readerShape("Title")).Trim()
                medData.IMedBase(iMediationShape) = CInt(Me.m_db.ReadSafe(readerShape, "IMedBase", 1200 / 3))
                medData.XAxisMin(iMediationShape) = CSng(Me.m_db.ReadSafe(readerShape, "XAxisMin", 0))
                medData.XAxisMax(iMediationShape) = CSng(Me.m_db.ReadSafe(readerShape, "XAxisMax", 1))
                Me.m_db.ReleaseReader(readerShape)
                readerShape = Nothing

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading MediationShape {1}", ex.Message, iShapeID))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function LoadPredPreyInteractions() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim reader As IDataReader = Nothing
            Dim iPredator As Integer = 0
            Dim iPrey As Integer = 0
            Dim iShapeID As Integer = 0
            Dim iShape As Integer = 0
            Dim bSucces As Boolean = True
            Dim iFNo(ecosimDS.nGroups, ecosimDS.nGroups) As Integer

            Try

                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenarioPredPreyShape WHERE (ScenarioID={0})", iScenarioID))
                While reader.Read()

                    ' Find iPredator
                    iPredator = Array.IndexOf(ecosimDS.GroupDBID, CInt(reader("PredID")))
                    ' Find iPrey
                    iPrey = Array.IndexOf(ecosimDS.GroupDBID, CInt(reader("PreyID")))
                    ' Next shape
                    iFNo(iPrey, iPredator) += 1
                    ' Protect from data overflow
                    If (iFNo(iPrey, iPredator) <= cMediationDataStructures.MAXFUNCTIONS) Then
                        ' Resolve shape ID
                        iShapeID = CInt(reader("ShapeID"))
                        ' Determine shape type
                        iShape = Array.IndexOf(ecosimDS.BioMedData.MediationDBIDs, iShapeID)
                        ' Is a mediation shape?
                        If iShape <> -1 Then
                            ' #Yes: flag as mediation shape
                            ecosimDS.BioMedData.IsMedFunction(iPrey, iPredator, iFNo(iPrey, iPredator)) = True
                        Else
                            ' #No: flag as other shape
                            ecosimDS.BioMedData.IsMedFunction(iPrey, iPredator, iFNo(iPrey, iPredator)) = False
                            ' Obtain forcing index
                            iShape = Array.IndexOf(ecosimDS.ForcingDBIDs, iShapeID)
                        End If

                        If iShape <> -1 Then
                            ' Update sim fields
                            ecosimDS.BioMedData.FunctionNumber(iPrey, iPredator, iFNo(iPrey, iPredator)) = iShape
                            Dim appl As eForcingFunctionApplication = CType(CInt(reader("FunctionType")), eForcingFunctionApplication)
                            ' Minor correction which does not warrant a database update.
                            If appl = eForcingFunctionApplication.SearchRate Then
                                If ecopathDS.PP(iPredator) = 1.0 Then
                                    appl = eForcingFunctionApplication.ProductionRate
                                ElseIf ecopathDS.PP(iPredator) = 2.0 Then
                                    appl = eForcingFunctionApplication.Import
                                End If
                            End If
                            ecosimDS.BioMedData.ApplicationType(iPrey, iPredator, iFNo(iPrey, iPredator)) = appl
                        Else
                            Me.LogMessage(String.Format("Shape {0} cannot be used for pred/prey interactions; assignment discarded", iShapeID))
                        End If
                    End If

                End While

                Me.m_db.ReleaseReader(reader)
                reader = Nothing

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading PredPreyInteraction", ex.Message))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function LoadLandingInteractions() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim reader As IDataReader = Nothing
            Dim iFleet As Integer = 0
            Dim iGroup As Integer = 0
            Dim iShapeID As Integer = 0
            Dim iShape As Integer = 0
            Dim bSucces As Boolean = True
            Dim iFNo(ecosimDS.nGroups, ecosimDS.nGear) As Integer

            Try

                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenarioLandingsShape WHERE (ScenarioID={0})", iScenarioID))
                While reader.Read()

                    ' Find iFleet
                    iFleet = Array.IndexOf(ecosimDS.FleetDBID, CInt(reader("FleetID")))
                    ' Find iGroup
                    iGroup = Array.IndexOf(ecosimDS.GroupDBID, CInt(reader("GroupID")))
                    ' Next shape
                    iFNo(iGroup, iFleet) += 1
                    ' Resolve shape ID
                    iShapeID = CInt(reader("ShapeID"))
                    ' Resolve iShape
                    iShape = Array.IndexOf(ecosimDS.PriceMedData.MediationDBIDs, iShapeID)

                    If iShape > -1 Then
                        ecosimDS.PriceMedData.PriceMedFuncNum(iGroup, iFleet, iFNo(iGroup, iFleet)) = iShape
                    Else
                        Me.LogMessage(String.Format("Shape {0} cannot be used for landings interactions; assignment discarded", iShapeID))
                    End If

                End While

                Me.m_db.ReleaseReader(reader)
                reader = Nothing

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Landing interaction", ex.Message))
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Load the mediation weights for the active scenario.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function LoadMediationWeights() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim medData As cMediationDataStructures = Nothing
            Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim readerGroup As IDataReader = Nothing
            Dim readerFleet As IDataReader = Nothing
            Dim iGroup As Integer = 0
            Dim iFleet As Integer = 0
            Dim iShape As Integer = 0
            Dim bSucces As Boolean = True

            ' === Pred/prey mediations ===
            medData = ecosimDS.BioMedData
            Try
                readerGroup = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenarioShapeMedWeightsGroup WHERE (ScenarioID={0})", iScenarioID))
                If (readerGroup IsNot Nothing) Then
                    While readerGroup.Read()
                        iShape = Array.IndexOf(medData.MediationDBIDs, readerGroup("ShapeID"))
                        iGroup = Array.IndexOf(ecosimDS.GroupDBID, readerGroup("GroupID"))
                        If (iGroup <> -1 And iShape <> -1) Then
                            medData.MedWeights(iGroup, iShape) = CSng(readerGroup("MedWeights"))
                        End If
                    End While
                    Me.m_db.ReleaseReader(readerGroup)
                    readerGroup = Nothing
                End If
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading group MediationWeights", ex.Message))
                bSucces = False
            End Try

            Try
                readerFleet = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenarioShapeMedWeightsFleet WHERE (ScenarioID={0})", iScenarioID))
                If (readerFleet IsNot Nothing) Then
                    While readerFleet.Read()
                        iShape = Array.IndexOf(medData.MediationDBIDs, readerFleet("ShapeID"))
                        ' Unfortunate legacy: fleet refers to Ecopath fleet, not Ecosim as it should have
                        iFleet = Array.IndexOf(ecopathDS.FleetDBID, readerFleet("FleetID"))
                        If (iFleet <> -1 And iShape <> -1) Then
                            medData.MedWeights(iFleet + ecosimDS.nGroups, iShape) = CSng(readerFleet("MedWeights"))
                        End If
                    End While
                    Me.m_db.ReleaseReader(readerFleet)
                    readerFleet = Nothing
                End If
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading fleet MediationWeights", ex.Message))
                bSucces = False
            End Try

            ' === Landings mediations === 
            medData = ecosimDS.PriceMedData
            Try
                readerGroup = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenarioshapeMedWeightsLandings WHERE (ScenarioID={0})", iScenarioID))
                If (readerGroup IsNot Nothing) Then
                    While readerGroup.Read()
                        iShape = Array.IndexOf(medData.MediationDBIDs, readerGroup("ShapeID"))
                        iGroup = Array.IndexOf(ecosimDS.GroupDBID, readerGroup("GroupID"))
                        iFleet = Array.IndexOf(ecosimDS.FleetDBID, readerGroup("FleetID"))
                        If (iGroup > 0 And iShape > 0) Then
                            iFleet = Math.Max(0, iFleet)
                            medData.MedPriceWeights(iGroup, iFleet, iShape) = CSng(readerGroup("MedWeights"))
                        End If
                    End While
                    Me.m_db.ReleaseReader(readerGroup)
                    readerGroup = Nothing
                End If
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading group MediationWeights", ex.Message))
                bSucces = False
            End Try

            Return True

        End Function

        Private Function LoadStanzaShapeAssignments() As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim reader As IDataReader = Nothing
            Dim iStanza As Integer = 0
            Dim iShape As Integer = 0
            Dim bSucces As Boolean = True

            Try
                reader = Me.m_db.GetReader("SELECT * FROM EcosimStanzaShape")
                While reader.Read()
                    ' Get iStanza 
                    iStanza = Array.IndexOf(stanzaDS.StanzaDBID, CInt(reader("StanzaID")))
                    ' Is valid stanza?
                    If (iStanza > 0) Then
                        ' #Yes: has egg production shape?
                        If Not Convert.IsDBNull(reader("EggprodShapeID")) Then
                            ' #Yes: resolve shape index iShape
                            iShape = Array.IndexOf(ecosimDS.ForcingDBIDs, CInt(reader("EggprodShapeID")))
                            ' Is a valid shape index?
                            If (iShape > 0) Then
                                ' #Yes: assign
                                stanzaDS.EggProdShapeSplit(iStanza) = iShape
                            End If
                        End If
                        ' #Yes: has hatch code forcing shape?
                        If Not Convert.IsDBNull(reader("HatchCodeShapeID")) Then
                            ' #Yes: resolve shape index iShape
                            iShape = Array.IndexOf(ecosimDS.ForcingDBIDs, CInt(reader("HatchCodeShapeID")))
                            ' Is a valid shape index?
                            If (iShape > 0) Then
                                ' #Yes: assign
                                stanzaDS.HatchCode(iStanza) = iShape
                            End If
                        End If
                    End If ' Is valid stanza
                End While

                Me.m_db.ReleaseReader(reader)
                reader = Nothing

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading stanza shape assignments", ex.Message))
                bSucces = False
            End Try
            Return bSucces
        End Function

        Private Function LoadFishingRateShape(ByVal iShapeID As Integer, ByVal iFishingRateShape As Integer) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim readerShape As IDataReader = Nothing
            Dim strMemo As String = ""
            Dim astrMemoBits() As String
            Dim bSucces As Boolean = True

            If iShapeID = 0 Then Return bSucces

            Try

                readerShape = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimShapeFishRate WHERE (ShapeID={0})", iShapeID))
                If readerShape.Read() Then
                    ecosimDS.FishRateGearTitle(iFishingRateShape) = CStr(Me.m_db.ReadSafe(readerShape, "Title", "")).Trim()
                    strMemo = CStr(readerShape("zScale"))
                    astrMemoBits = strMemo.Trim.Split(CChar(" "))
                    For j As Integer = 1 To Math.Min(ecosimDS.NTimes, astrMemoBits.Length)
                        ecosimDS.FishRateGear(iFishingRateShape, j) = cStringUtils.ConvertToSingle(astrMemoBits(j - 1), 1)
                    Next
                    ecosimDS.FishRateGearDBID(iFishingRateShape) = iShapeID
                End If

                Me.m_db.ReleaseReader(readerShape)
                readerShape = Nothing

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading FishingRate {1}", ex.Message, iShapeID))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function LoadFishMortShape(ByVal iShapeID As Integer, ByVal iForcingShape As Integer) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim readerShape As IDataReader = Nothing
            Dim strMemo As String = ""
            Dim astrMemoBits() As String
            Dim bSucces As Boolean = True

            If iShapeID = 0 Then Return bSucces

            Try

                readerShape = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimShapeFishMort WHERE (ShapeID={0})", iShapeID))
                readerShape.Read()
                ' Store ID
                ecosimDS.FishRateNoDBID(iForcingShape) = iShapeID
                ' Store title
                ecosimDS.FishRateNoTitle(iForcingShape) = CStr(readerShape("Title")).Trim()
                ' Store points
                strMemo = CStr(readerShape("zScale"))
                ' Got points?
                If Not String.IsNullOrEmpty(strMemo) Then
                    ' #Yes: split and process
                    astrMemoBits = strMemo.Trim.Split(CChar(" "))
                    For j As Integer = 1 To Math.Min(ecosimDS.NTimes, astrMemoBits.Length)
                        ecosimDS.FishRateNo(iForcingShape, j) = cStringUtils.ConvertToSingle(astrMemoBits(j - 1), 0)
                    Next
                End If

                Me.m_db.ReleaseReader(readerShape)
                readerShape = Nothing

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading fish mortality shape {1}", ex.Message, iShapeID))
                bSucces = False
            End Try

            Return bSucces

        End Function

#End Region ' Shape load helpers

        Private Function SaveShapes(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim iShape As Integer = 0
            Dim iShapeID As Integer = 0
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim bSucces As Boolean = True

            Try
                ' Start writing
                writer = Me.m_db.GetWriter("EcoSimShape")
                dt = writer.GetDataTable()

                For iShape = 1 To ecosimDS.NumForcingShapes
                    If (ecosimDS.ForcingDBIDs(iShape) > 0) Then
                        drow = dt.Rows.Find(ecosimDS.ForcingDBIDs(iShape))
                        bNewRow = (drow Is Nothing)

                        If bNewRow Then
                            drow = writer.NewRow()
                            drow("ShapeID") = ecosimDS.ForcingDBIDs(iShape)
                            drow("ShapeType") = ecosimDS.ForcingShapeType(iShape)
                        Else
                            drow.BeginEdit()
                        End If

                        ' Shape type does not change
                        drow("IsSeasonal") = ecosimDS.isSeasonal(iShape)

                        If bNewRow Then
                            writer.AddRow(drow)
                        Else
                            drow.EndEdit()
                        End If
                        writer.Commit()

                        Select Case ecosimDS.ForcingShapeType(iShape)
                            Case eDataTypes.EggProd
                                bSucces = bSucces And SaveEggShape(iShape)
                            Case eDataTypes.Forcing
                                bSucces = bSucces And SaveTimeShape(iShape)
                            Case Else
                                Debug.Assert(False)
                        End Select

                    End If
                Next iShape

                For Each shapeDataType As eDataTypes In New eDataTypes() {eDataTypes.Mediation, eDataTypes.PriceMediation, eDataTypes.CapacityMediation}
                    Dim medData As cMediationDataStructures = Nothing
                    Select Case shapeDataType
                        Case eDataTypes.Mediation : medData = ecosimDS.BioMedData
                        Case eDataTypes.PriceMediation : medData = ecosimDS.PriceMedData
                        Case eDataTypes.CapacityMediation : medData = ecosimDS.CapEnvResData
                    End Select

                    For iShape = 1 To medData.MediationShapes
                        If (medData.MediationDBIDs(iShape) > 0) Then
                            drow = dt.Rows.Find(medData.MediationDBIDs(iShape))
                            bNewRow = (drow Is Nothing)

                            If bNewRow Then
                                drow = writer.NewRow()
                                drow("ShapeID") = medData.MediationDBIDs(iShape)
                            Else
                                drow.BeginEdit()
                            End If

                            drow("ShapeType") = shapeDataType

                            If bNewRow Then
                                writer.AddRow(drow)
                            Else
                                drow.EndEdit()
                            End If
                            writer.Commit()
                            bSucces = bSucces And SaveMediationShape(iShape, medData)
                        End If
                    Next iShape
                Next

                For iShape = 1 To ecosimDS.FishRateGearDBID.Length - 1
                    iShapeID = idm.GetID(eDataTypes.FishingEffort, ecosimDS.FishRateGearDBID(iShape))
                    If (iShapeID > 0) Then

                        drow = dt.Rows.Find(iShapeID)
                        If (drow Is Nothing) Then
                            drow = writer.NewRow()
                            drow("ShapeID") = iShapeID
                            drow("ShapeType") = eDataTypes.FishingEffort
                            writer.AddRow(drow)
                            writer.Commit()
                        End If

                        bSucces = bSucces And Me.SaveFishingRateShape(iShape, idm)
                        bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.FishingEffort, ecosimDS.FishRateGearDBID(iShape))

                    End If
                Next iShape

                For iShape = 1 To ecosimDS.FishRateNoDBID.Length - 1
                    iShapeID = idm.GetID(eDataTypes.FishMort, ecosimDS.FishRateNoDBID(iShape))
                    If (iShapeID > 0) Then

                        drow = dt.Rows.Find(iShapeID)
                        If (drow Is Nothing) Then
                            drow = writer.NewRow()
                            drow("ShapeID") = iShapeID
                            drow("ShapeType") = eDataTypes.FishMort
                            writer.AddRow(drow)
                            writer.Commit()
                        End If
                        'bSucces = bSucces And Me.SaveFishMortShape(iShape, idm)
                    End If
                Next iShape

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving forcing shapes", ex.Message))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function SaveShapeAssignments(ByVal idm As cIDMappings) As Boolean

            Dim bSucces As Boolean = True
            bSucces = bSucces And SavePredPreyInteractions(idm)
            bSucces = bSucces And SaveLandingsInteractions(idm)
            bSucces = bSucces And SaveMediationWeights(idm)
            bSucces = bSucces And SaveStanzaShapeAssignments(idm)
            Return bSucces

        End Function

#Region " Shape save helpers "

        Private Function SaveEggShape(ByVal iShape As Integer) As Boolean

            ' ToDo: see if passing in an adapter and a datatable may speed up the save process significantly

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iShapeID As Integer = ecosimDS.ForcingDBIDs(iShape)
            Dim shapeParms As cEcosimDatastructures.ShapeParameters = ecosimDS.ForcingShapeParams(iShape)
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim sbZScale As New StringBuilder()
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim bSucces As Boolean = True

            ' Sanity check
            Debug.Assert(ecosimDS.ForcingShapeType(iShape) = eDataTypes.EggProd)

            Try
                writer = Me.m_db.GetWriter("EcosimShapeEggProd")
                dt = writer.GetDataTable()
                drow = dt.Rows.Find(iShapeID)
                bNewRow = (drow Is Nothing)

                If bNewRow Then
                    drow = writer.NewRow()
                    drow("ShapeID") = iShapeID
                Else
                    drow.BeginEdit()
                End If

                drow("Title") = ecosimDS.ForcingTitles(iShape)
                drow("FunctionType") = shapeParms.ShapeFunctionType
                drow("FunctionParams") = cStringUtils.ParamArrayToString(shapeParms.ShapeFunctionParams)

                ' Assemble Zscale
                For ipt As Integer = 1 To ecosimDS.ForcePoints
                    If (ipt > 1) Then sbZScale.Append(" ")
                    sbZScale.Append(cStringUtils.FormatSingle(ecosimDS.zscale(ipt, iShape)))
                Next
                drow("Zscale") = sbZScale.ToString()

                If bNewRow Then
                    writer.AddRow(drow)
                Else
                    drow.EndEdit()
                End If

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function SaveTimeShape(ByVal iShape As Integer) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iShapeID As Integer = ecosimDS.ForcingDBIDs(iShape)
            Dim shapeParms As cEcosimDatastructures.ShapeParameters = ecosimDS.ForcingShapeParams(iShape)
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim sbZScale As New StringBuilder()
            Dim adrows() As DataRow = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Sanity check
            Debug.Assert(ecosimDS.ForcingShapeType(iShape) = eDataTypes.Forcing)

            Try
                writer = Me.m_db.GetWriter("EcosimShapeTime")
                dt = writer.GetDataTable()
                adrows = dt.Select(String.Format("ShapeID={0}", iShapeID))
                If adrows.Length = 1 Then
                    drow = adrows(0)
                    drow.BeginEdit()
                Else
                    drow = writer.NewRow()
                    drow("ShapeID") = iShapeID
                End If

                drow("Title") = ecosimDS.ForcingTitles(iShape)
                drow("FunctionType") = shapeParms.ShapeFunctionType
                drow("FunctionParams") = cStringUtils.ParamArrayToString(shapeParms.ShapeFunctionParams)

                Dim iTrackBack As Integer = ecosimDS.ForcePoints
                Dim sLastVal As Single = ecosimDS.zscale(iTrackBack, iShape)

                ' Minor optimization: do not save repeated values at the end of a shape
                While (iTrackBack > 1) And (ecosimDS.zscale(iTrackBack - 1, iShape) = sLastVal)
                    iTrackBack -= 1
                End While

                ' Assemble Zscale
                For ipt As Integer = 1 To iTrackBack
                    If (ipt > 1) Then sbZScale.Append(" ")
                    sbZScale.Append(cStringUtils.FormatSingle(ecosimDS.zscale(ipt, iShape)))
                Next
                drow("Zscale") = sbZScale.ToString()

                If adrows.Length = 1 Then
                    drow.EndEdit()
                Else
                    writer.AddRow(drow)
                End If
                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function SaveMediationShape(ByVal iShape As Integer, ByVal medData As cMediationDataStructures) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim sbZScale As New StringBuilder()
            Dim adrows() As DataRow = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Dim iShapeID As Integer = medData.MediationDBIDs(iShape)
            Dim shapeParms As cEcosimDatastructures.ShapeParameters = medData.MediationShapeParams(iShape)


            Try
                writer = Me.m_db.GetWriter("EcosimShapeMediation")
                dt = writer.GetDataTable()
                adrows = dt.Select(String.Format("ShapeID={0}", iShapeID))
                If adrows.Length = 1 Then
                    drow = adrows(0)
                    drow.BeginEdit()
                Else
                    drow = writer.NewRow()
                    drow("ShapeID") = iShapeID
                End If

                drow("Title") = medData.MediationTitles(iShape)
                drow("IMedBase") = medData.IMedBase(iShape)
                drow("FunctionType") = shapeParms.ShapeFunctionType
                drow("FunctionParams") = cStringUtils.ParamArrayToString(shapeParms.ShapeFunctionParams)
                drow("XAxisMin") = medData.XAxisMin(iShape)
                drow("XAxisMax") = medData.XAxisMax(iShape)

                ' Assemble Zscale
                For ipt As Integer = 1 To medData.NMedPoints
                    If (ipt > 1) Then sbZScale.Append(" ")
                    sbZScale.Append(cStringUtils.FormatSingle(medData.Medpoints(ipt, iShape)))
                Next
                drow("Zscale") = sbZScale.ToString()

                If adrows.Length = 1 Then
                    drow.EndEdit()
                Else
                    writer.AddRow(drow)
                End If
                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function SavePredPreyInteractions(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim medData As cMediationDataStructures = ecosimDS.BioMedData
            Dim iScenarioID As Integer = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iShape As Integer = 0
            Dim bSucces As Boolean = True

            Try

                Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioPredPreyShape WHERE (ScenarioID={0})", iScenarioID))
                writer = Me.m_db.GetWriter("EcosimScenarioPredPreyShape")

                For iPredator As Integer = 1 To ecosimDS.nGroups
                    For iPrey As Integer = 1 To ecosimDS.nGroups
                        For iShapeNo As Integer = 1 To cMediationDataStructures.MAXFUNCTIONS

                            Try

                                ' Get shape assignment
                                iShape = ecosimDS.BioMedData.FunctionNumber(iPrey, iPredator, iShapeNo)
                                ' Is an assignment?
                                If (iShape > 0) Then
                                    ' Save assignment
                                    drow = writer.NewRow()
                                    drow("ScenarioID") = iScenarioID
                                    drow("PredID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iPredator))
                                    drow("PreyID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iPrey))
                                    If (ecosimDS.BioMedData.IsMedFunction(iPrey, iPredator, iShapeNo)) Then
                                        drow("ShapeID") = medData.MediationDBIDs(iShape)
                                    Else
                                        drow("ShapeID") = ecosimDS.ForcingDBIDs(iShape)
                                    End If
                                    drow("FunctionType") = ecosimDS.BioMedData.ApplicationType(iPrey, iPredator, iShapeNo)
                                    writer.AddRow(drow)
                                End If
                            Catch ex As Exception
                                'Debug.Assert(False, String.format("Index error on pred {0}, prey {1}, shape {2}", iPredator, iPrey, iShape))
                            End Try

                        Next iShapeNo
                    Next iPrey
                Next iPredator

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving PredPreyInteraction", ex.Message))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function SaveLandingsInteractions(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim medData As cMediationDataStructures = ecosimDS.PriceMedData
            Dim iScenarioID As Integer = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iShape As Integer = 0
            Dim bSucces As Boolean = True

            Try

                Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioLandingsShape WHERE (ScenarioID={0})", iScenarioID))
                writer = Me.m_db.GetWriter("EcosimScenarioLandingsShape")

                For iFleet As Integer = 1 To ecosimDS.nGear
                    For iGroup As Integer = 1 To ecosimDS.nGroups
                        For iShapeNo As Integer = 1 To cMediationDataStructures.MAXFUNCTIONS - 1

                            Try
                                ' Get shape assignment
                                iShape = ecosimDS.PriceMedData.PriceMedFuncNum(iGroup, iFleet, iShapeNo)
                                ' Is an assignment?
                                If (iShape > 0) Then
                                    ' Save assignment
                                    drow = writer.NewRow()
                                    drow("ScenarioID") = iScenarioID
                                    drow("FleetID") = idm.GetID(eDataTypes.EcosimFleetInput, ecopathDS.FleetDBID(iFleet))
                                    drow("GroupID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iGroup))
                                    drow("ShapeID") = medData.MediationDBIDs(iShape)
                                    drow("FunctionType") = CInt(eForcingFunctionApplication.OffVesselPrice)
                                    writer.AddRow(drow)
                                End If
                            Catch ex As Exception
                            End Try

                        Next iShapeNo
                    Next iGroup
                Next iFleet

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving landings interaction", ex.Message))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function SaveMediationWeights(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim medData As cMediationDataStructures = Nothing
            Dim iScenarioID As Integer = 0
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Obtain mapped scenario ID
            iScenarioID = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))

            ' === Pred/prey mediations ===
            Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioshapeMedWeightsGroup WHERE (ScenarioID={0})", iScenarioID))
            Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioShapeMedWeightsFleet WHERE (ScenarioID={0})", iScenarioID))

            medData = ecosimDS.BioMedData
            Try
                writer = Me.m_db.GetWriter("EcosimScenarioshapeMedWeightsGroup")
                For iGroup As Integer = 1 To ecosimDS.nGroups
                    For iShape As Integer = 1 To medData.MediationShapes
                        If medData.MedWeights(iGroup, iShape) > 0 Then
                            drow = writer.NewRow()
                            drow("ScenarioID") = iScenarioID
                            ' Ecosim groups unique per scenario: map this
                            drow("GroupID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iGroup))
                            drow("ShapeID") = medData.MediationDBIDs(iShape)
                            drow("MedWeights") = medData.MedWeights(iGroup, iShape)
                            writer.AddRow(drow)
                        End If
                    Next iShape
                Next iGroup
                Me.m_db.ReleaseWriter(writer, True)

                writer = Me.m_db.GetWriter("EcosimScenarioShapeMedWeightsFleet")
                For iFleet As Integer = 1 To ecosimDS.nGear
                    For iShape As Integer = 1 To medData.MediationShapes
                        If medData.MedWeights(iFleet + ecosimDS.nGroups, iShape) > 0 Then
                            drow = writer.NewRow()
                            drow("ScenarioID") = iScenarioID
                            ' Unfortunate legacy: FleetID refers to Ecopath fleet, NOT Ecosim as it should have
                            drow("FleetID") = ecopathDS.FleetDBID(iFleet)
                            drow("ShapeID") = medData.MediationDBIDs(iShape)
                            drow("MedWeights") = medData.MedWeights(iFleet + ecosimDS.nGroups, iShape)
                            writer.AddRow(drow)
                        End If
                    Next iShape
                Next iFleet
                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            ' === Landings mediations === 
            Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioshapeMedWeightsLandings WHERE (ScenarioID={0})", iScenarioID))
            medData = ecosimDS.PriceMedData
            Try
                writer = Me.m_db.GetWriter("EcosimScenarioshapeMedWeightsLandings")
                For iGroup As Integer = 1 To ecosimDS.nGroups
                    For iFleet As Integer = 0 To ecosimDS.nGear
                        For iShape As Integer = 1 To medData.MediationShapes
                            If medData.MedPriceWeights(iGroup, iFleet, iShape) > 0 Then
                                drow = writer.NewRow()
                                drow("ScenarioID") = iScenarioID
                                drow("GroupID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iGroup))
                                drow("FleetID") = Math.Max(0, idm.GetID(eDataTypes.EcosimFleetInput, ecopathDS.FleetDBID(iFleet)))
                                drow("ShapeID") = medData.MediationDBIDs(iShape)
                                drow("MedWeights") = medData.MedPriceWeights(iGroup, iFleet, iShape)
                                writer.AddRow(drow)
                            End If
                        Next iShape
                    Next iFleet
                Next iGroup
                bSucces = Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces
        End Function

        Private Function SaveStanzaShapeAssignments(ByVal idm As cIDMappings) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                ' Erase all
                Me.m_db.Execute("DELETE * FROM EcosimStanzaShape")
                ' Get writer
                writer = Me.m_db.GetWriter("EcosimStanzaShape")

                ' For every stanza
                For iStanza As Integer = 1 To stanzaDS.Nsplit
                    ' Has any shape assignment?
                    If (stanzaDS.EggProdShapeSplit(iStanza) > 0) Or (stanzaDS.HatchCode(iStanza) > 0) Then
                        ' #Yes: start new row
                        drow = writer.NewRow()
                        ' Set PK
                        drow("StanzaID") = stanzaDS.StanzaDBID(iStanza)

                        ' EggProdShapeID identifies the egg prod shape assigned. Do not specify anything
                        ' to leave the field at DBNull
                        If (stanzaDS.EggProdShapeSplit(iStanza) > 0) Then
                            drow("EggprodShapeID") = ecosimDS.ForcingDBIDs(CInt(stanzaDS.EggProdShapeSplit(iStanza)))
                        Else
                            ' For missing shape this value MUST BE set to DBNull (not 0)
                        End If

                        ' HatchCodeShapeID identifies the egg prod shape assigned. Do not specify anything
                        ' to leave the field at DBNull
                        If (stanzaDS.HatchCode(iStanza) > 0) Then
                            drow("HatchCodeShapeID") = ecosimDS.ForcingDBIDs(CInt(stanzaDS.HatchCode(iStanza)))
                        Else
                            ' For missing shape this value MUST BE set to DBNull (not 0)
                        End If

                        ' Done
                        writer.AddRow(drow)
                    End If
                Next
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces
        End Function

        Private Function SaveFishingRateShape(ByVal iShape As Integer, ByVal idm As cIDMappings) As Boolean

            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iShapeID As Integer = idm.GetID(eDataTypes.FishingEffort, ecosimDS.FishRateGearDBID(iShape))
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim sbZScale As New StringBuilder()
            Dim adrows() As DataRow = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Debug.Assert(iShapeID > 0, String.Format("Invalid ID for FishingRate shape {0}", iShapeID))

            Try
                writer = Me.m_db.GetWriter("EcosimShapeFishRate")
                dt = writer.GetDataTable()
                adrows = dt.Select(String.Format("ShapeID={0}", iShapeID))
                If adrows.Length = 1 Then
                    drow = adrows(0)
                    drow.BeginEdit()
                Else
                    drow = writer.NewRow()
                    drow("ShapeID") = iShapeID
                End If

                drow("ShapeID") = iShapeID
                drow("Title") = ecosimDS.FishRateGearTitle(iShape)
                For ipt As Integer = 1 To ecosimDS.NTimes
                    If (ipt > 1) Then sbZScale.Append(" ")
                    sbZScale.Append(cStringUtils.FormatSingle(ecosimDS.FishRateGear(iShape, ipt)))
                Next
                drow("Zscale") = sbZScale.ToString()

                If adrows.Length = 1 Then
                    drow.EndEdit()
                Else
                    writer.AddRow(drow)
                End If
                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        'Private Function SaveFishMortShape(ByVal iShape As Integer, ByVal idm As cIDMappings) As Boolean

        '    Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
        '    Dim iDBID As Integer = idm.GetID(eDataTypes.FishMort, ecosimDS.FishRateNoDBID(iShape))
        '    Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
        '    Dim dt As DataTable = Nothing
        '    Dim sbZScale As New StringBuilder()
        '    Dim adrows() As DataRow = Nothing
        '    Dim drow As DataRow = Nothing
        '    Dim bSucces As Boolean = True

        '    Debug.Assert(iDBID > 0, String.format("Invalid ID for FishMortShape shape {0}", iDBID))

        '    Try
        '        writer = Me.m_db.GetWriter("EcosimShapeFishMort")
        '        dt = writer.GetDataTable()
        '        adrows = dt.Select(String.format("ShapeID={0}", iDBID))
        '        If adrows.Length = 1 Then
        '            drow = adrows(0)
        '            drow.BeginEdit()
        '        Else
        '            drow = writer.NewRow()
        '            drow("ShapeID") = iDBID
        '        End If

        '        drow("Title") = ecosimDS.FishRateNoTitle(iShape)
        '        For ipt As Integer = 1 To ecosimDS.NTimes
        '            If (ipt > 1) Then sbZScale.Append(" ")
        '            sbZScale.Append(cStringUtils.FormatSingle(ecosimDS.FishRateNo(iShape, ipt)))
        '        Next
        '        drow("Zscale") = sbZScale.ToString()

        '        If adrows.Length = 1 Then
        '            drow.EndEdit()
        '        Else
        '            writer.AddRow(drow)
        '        End If
        '        Me.m_db.ReleaseWriter(writer, True)

        '    Catch ex As Exception
        '        bSucces = False
        '    End Try

        '    Return bSucces

        'End Function

#End Region ' Shape save helpers

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Appends a forcing shape to the data source.
        ''' </summary>
        ''' <param name="strShapeName">Name to assign to new shape.</param>
        ''' <param name="shapeType"><see cref="eDataTypes">Type of the shape</see> to add.</param>
        ''' <param name="iShapeID">Database ID assigned to the new shape.</param>
        ''' <param name="asData">Shape point data.</param>
        ''' <param name="functionType">Primitive function type shape was created from.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Friend Function AppendShape(ByVal strShapeName As String,
                                    ByVal shapeType As eDataTypes,
                                    ByRef iShapeID As Integer,
                                    ByVal asData As Single(),
                                    ByVal functionType As Long,
                                    ByVal params As Single()) As Boolean _
                Implements IEcosimDatasource.AppendShape

            If Me.AppendShapeImpl(strShapeName, shapeType, iShapeID, asData, functionType, params) Then
                ' #Yes: reload
                'jb the number of shapes has changed in the database so we need to reload all the shape data in memory
                Return Me.LoadShapes()
            End If

            Return False

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Append a shape to the database, internal implementation.
        ''' </summary>
        ''' <param name="strShapeName"></param>
        ''' <param name="shapeType"></param>
        ''' <param name="iShapeID"></param>
        ''' <param name="points"></param>
        ''' <param name="functionType"></param>
        ''' <param name="params"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function AppendShapeImpl(ByVal strShapeName As String,
                                         ByVal shapeType As eDataTypes,
                                         ByRef iShapeID As Integer,
                                         ByVal points As Single(),
                                         ByVal functionType As Long,
                                         ByVal params As Single()) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim writerID As cEwEDatabase.cEwEDbWriter = Me.m_db.GetWriter("EcoSimShape")
            Dim writerShape As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                iShapeID = CInt(Me.m_db.GetValue("SELECT MAX(ShapeID) FROM EcoSimShape", 0)) + 1

                drow = writerID.NewRow()
                drow("ShapeID") = iShapeID
                drow("ShapeType") = shapeType
                drow("IsSeasonal") = (shapeType = eDataTypes.EggProd)
                writerID.AddRow(drow)
                writerID.Commit()

                ' Write sub-shape row
                Select Case shapeType

                    Case eDataTypes.EggProd
                        writerShape = Me.m_db.GetWriter("EcosimShapeEggProd")

                    Case eDataTypes.Forcing
                        writerShape = Me.m_db.GetWriter("EcosimShapeTime")

                    Case eDataTypes.Mediation, eDataTypes.PriceMediation, eDataTypes.CapacityMediation
                        writerShape = Me.m_db.GetWriter("EcosimShapeMediation")

                    Case eDataTypes.FishingEffort
                        writerShape = Me.m_db.GetWriter("EcosimShapeFishRate")

                    Case eDataTypes.FishMort
                        writerShape = Me.m_db.GetWriter("EcosimShapeFishMort")

                    Case eDataTypes.NotSet
                        Debug.Assert(False, String.Format("Cannot load invalid shapetype for shape ID {0}", iShapeID))
                        Return False

                End Select

                ' Sanity check
                Debug.Assert(writerShape IsNot Nothing)

                drow = writerShape.NewRow()
                drow("ShapeID") = iShapeID
                drow("Title") = strShapeName.Substring(0, Math.Min(strShapeName.Length, 50))

                If points Is Nothing Then
                    drow("zScale") = ""
                Else
                    Dim sbZScale As New StringBuilder()
                    ' Assemble Zscale
                    For ipt As Integer = 1 To Math.Min(ecosimDS.ForcePoints, points.Length - 1)
                        If (ipt > 1) Then sbZScale.Append(" ")
                        sbZScale.Append(cStringUtils.FormatSingle(points(ipt)))
                    Next
                    drow("zScale") = sbZScale.ToString()
                End If

                ' Specific bits
                If (shapeType <> eDataTypes.FishMort) And (shapeType <> eDataTypes.FishingEffort) Then

                    drow("FunctionType") = CInt(functionType)
                    drow("FunctionParams") = cStringUtils.ParamArrayToString(params)

                    If (shapeType = eDataTypes.Mediation) Or
                       (shapeType = eDataTypes.PriceMediation) Or
                       (shapeType = eDataTypes.CapacityMediation) Then
                        drow("IMedBase") = 1200 / 3
                    End If

                End If

                writerShape.AddRow(drow)

                Me.m_db.ReleaseWriter(writerShape, True)
                Me.m_db.ReleaseWriter(writerID)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while appending shape {1}, {2}", ex.Message, strShapeName, shapeType.ToString()))
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Deletes a forcing shape from the DB.
        ''' </summary>
        ''' <param name="iShapeID">Database ID of the shape to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>The number of shapes has changed in the database so all the
        ''' shape data is reloaded in memory.</remarks>
        ''' -------------------------------------------------------------------
        Friend Function RemoveShape(ByVal iShapeID As Integer) As Boolean _
                Implements IEcosimDatasource.RemoveShape

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim bSucces As Boolean = True

            Try

                ' Manually set 'soft' shape links to 0
                Me.m_db.Execute(String.Format("UPDATE EcoSimStanzaShape Set EggProdShapeID=0 WHERE (EggProdShapeID={0})", iShapeID))
                Me.m_db.Execute(String.Format("UPDATE EcoSimStanzaShape Set HatchCodeShapeID=0 WHERE (HatchCodeShapeID={0})", iShapeID))
                Me.m_db.Execute("DELETE FROM EcoSimStanzaShape WHERE ((HatchCodeShapeID=0) AND (EggProdShapeID=0))")

                ' Fix Ecosim environmental nutrient forcing
                Me.m_db.Execute(String.Format("UPDATE EcoSimScenario Set NutForcingShapeID=0 WHERE (NutForcingShapeID={0})", iShapeID))

                ' Delete Ecosim environmental responses and drivers
                Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioCapacityDrivers WHERE (ResponseID={0})", iShapeID))
                Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioCapacityDrivers WHERE (DriverID={0})", iShapeID))

                ' Delete Ecospace environmental responses
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioCapacityDrivers WHERE (ShapeID={0})", iShapeID))

                ' Delete Ecosim mediation
                Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioshapeMedWeightsGroup WHERE (ShapeID={0})", iShapeID))
                Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioShapeMedWeightsFleet WHERE (ShapeID={0})", iShapeID))
                Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioshapeMedWeightsLandings WHERE (ShapeID={0})", iShapeID))

                ' Delete Ecosim pred/prey interactions
                Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioPredPreyShape WHERE (ShapeID={0})", iShapeID))

                ' Delete catchability shapes
                Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioFleetGroupCatchability WHERE (ShapeID={0})", iShapeID))
                Me.m_db.Execute(String.Format("DELETE FROM EcosimShapeCatchability WHERE (ShapeID={0})", iShapeID))

                ' Destroy the given shape
                Me.m_db.Execute(String.Format("DELETE FROM EcoSimShape WHERE (ShapeID={0})", iShapeID))
                ' Reload shapes data
                bSucces = Me.LoadShapes()

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while deleting shape {1}", ex.Message, iShapeID))
                bSucces = False
            End Try

            Return bSucces

        End Function

#End Region ' Forcing and Mediaton shapes

#Region " Time series "

#Region " Import "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Import a <see cref="cTimeSeriesImport">cTimeSeriesImport</see> instance into the data source.
        ''' </summary>
        ''' <param name="ts">The time series data to import.</param>
        ''' <param name="iDataset">Index of the dataset to add the time series to.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function ImportTimeSeries(ByVal ts As cTimeSeriesImport, ByVal iDataset As Integer) As Boolean _
              Implements IEcosimDatasource.ImportTimeSeries

            Select Case cTimeSeriesFactory.TimeSeriesCategory(ts.TimeSeriesType)

                Case eTimeSeriesCategoryType.Group,
                     eTimeSeriesCategoryType.Fleet,
                     eTimeSeriesCategoryType.FleetGroup
                    Return Me.AddAsTimeSeries(ts, iDataset)

                Case eTimeSeriesCategoryType.Forcing
                    Return Me.AddAsForcingFunction(ts)

                Case eTimeSeriesCategoryType.NotSet
                    Debug.Assert(False)
                    Return False
            End Select

        End Function

#Region " Import helpers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ts"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function AddAsForcingFunction(ByVal ts As cTimeSeriesImport) As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim bNewShape As Boolean = True
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True
            Dim sbZScale As New StringBuilder()
            Dim iRepetitions As Integer = 1
            Dim iShapeID As Integer = 0

            ' Find shape ID for a FF with the same name as the TS to import
            iShapeID = CInt(Me.m_db.GetValue("SELECT ShapeID FROM EcosimShapeTime WHERE Title='" & ts.Name & "'", 0))

            ' Did FF already exist?
            If (iShapeID > 0) Then
                ' #Yes: Not creating a new shape
                bNewShape = False
            Else
                ' #No: creating a new shape
                ' Determine next shape ID
                iShapeID = CInt(Me.m_db.GetValue("SELECT MAX(ShapeID) FROM EcoSimShape", 0)) + 1
                bNewShape = True
            End If

            Try
                ' Start writing
                writer = Me.m_db.GetWriter("EcoSimShape")
                If bNewShape Then
                    drow = writer.NewRow()
                    drow("ShapeID") = iShapeID
                Else
                    dt = writer.GetDataTable()
                    drow = dt.Rows.Find(iShapeID)
                    drow.BeginEdit()
                End If

                drow("ShapeType") = eDataTypes.Forcing

                If bNewShape Then
                    writer.AddRow(drow)
                Else
                    drow.EndEdit()
                End If
                writer.Commit()
                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception

            End Try

            Try
                writer = Me.m_db.GetWriter("EcosimShapeTime")

                If bNewShape Then
                    drow = writer.NewRow()
                    drow("ShapeID") = iShapeID
                    drow("Title") = ts.Name
                Else
                    dt = writer.GetDataTable()
                    drow = dt.Rows.Find(iShapeID)
                    drow.BeginEdit()
                End If

                drow("FunctionType") = eShapeFunctionType.NotSet
                drow("FunctionParams") = ""

                ' Assemble Zscale. 
                ' JS 04april09: Time Series are most likely ANNUAL, FFs are MONTHLY
                Select Case ts.Interval
                    Case eTSDataSetInterval.Annual
                        iRepetitions = CInt(Me.m_core.nEcosimTimeSteps / Me.m_core.nEcosimYears)
                    Case eTSDataSetInterval.TimeStep
                        iRepetitions = 1
                    Case Else
                        Debug.Assert(False)
                End Select

                For iYear As Integer = 0 To ts.nPoints - 1
                    For iMonth As Integer = 1 To iRepetitions
                        If sbZScale.Length > 0 Then sbZScale.Append(" ")
                        sbZScale.Append(cStringUtils.FormatSingle(ts.ShapeData(iYear)))
                    Next
                Next

                drow("Zscale") = sbZScale.ToString()

                If (bNewShape) Then
                    writer.AddRow(drow)
                Else
                    drow.EndEdit()
                End If

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ts"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function AddAsTimeSeries(ByVal ts As cTimeSeriesImport, ByVal iDataset As Integer) As Boolean

            Dim tsds As cTimeSeriesDataStructures = Me.m_core.m_TSData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iTimeSeriesID As Integer = 0
            Dim sbValues As New StringBuilder()
            Dim bSucces As Boolean = True

            iTimeSeriesID = CInt(Me.m_db.GetValue("SELECT MAX(TimeSeriesID) FROM EcosimTimeSeries", 0)) + 1

            ' Time series are scenario-independant
            writer = Me.m_db.GetWriter("EcosimTimeSeries")

            drow = writer.NewRow()
            drow("TimeSeriesID") = iTimeSeriesID
            drow("Sequence") = iTimeSeriesID
            drow("DatName") = ts.Name
            drow("DatType") = ts.TimeSeriesType
            drow("WtType") = ts.WtType
            drow("CV") = ts.CV
            drow("DatasetID") = tsds.iDatasetDBID(iDataset)

            ' Concoct time series memo
            For iYear As Integer = 0 To ts.nPoints - 1
                If (iYear > 0) Then sbValues.Append(" ")
                sbValues.Append(cStringUtils.FormatSingle(ts.ShapeData(iYear)))
            Next
            drow("TimeValues") = sbValues.ToString()

            writer.AddRow(drow)
            Me.m_db.ReleaseWriter(writer, True)

            Select Case cTimeSeriesFactory.TimeSeriesCategory(ts.TimeSeriesType)
                Case eTimeSeriesCategoryType.Group
                    bSucces = bSucces And Me.AddGroupTimeSeries(ts, iTimeSeriesID)
                Case eTimeSeriesCategoryType.Fleet, eTimeSeriesCategoryType.FleetGroup
                    bSucces = bSucces And Me.AddFleetTimeSeries(ts, iTimeSeriesID)
            End Select

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ts"></param>
        ''' <param name="iTimeSeriesID"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function AddGroupTimeSeries(ByVal ts As cTimeSeriesImport, ByVal iTimeSeriesID As Integer) As Boolean

            Dim writerGroup As cEwEDatabase.cEwEDbWriter = Nothing
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Validate DatPool
            If (ts.DatPool < 1) Or (ts.DatPool >= ecopathDS.GroupDBID.Length) Then
                ' No group for this pool ID
                Return False
            End If

            Try
                writerGroup = Me.m_db.GetWriter("EcosimTimeSeriesGroup")
                drow = writerGroup.NewRow()
                drow("TimeSeriesID") = iTimeSeriesID
                drow("GroupID") = ecopathDS.GroupDBID(ts.DatPool)
                writerGroup.AddRow(drow)
                Me.m_db.ReleaseWriter(writerGroup, True)
            Catch ex As Exception
                ' Woops
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ts"></param>
        ''' <param name="iTimeSeriesID"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function AddFleetTimeSeries(ByVal ts As cTimeSeriesImport, ByVal iTimeSeriesID As Integer) As Boolean

            Dim writerFleet As cEwEDatabase.cEwEDbWriter = Nothing
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Validate DatPool
            If (ts.DatPool < 1) Or (ts.DatPool >= ecopathDS.FleetDBID.Length) Then
                ' No fleet for this pool ID
                Return False
            End If

            Try
                writerFleet = Me.m_db.GetWriter("EcosimTimeSeriesFleet")
                drow = writerFleet.NewRow()
                drow("TimeSeriesID") = iTimeSeriesID
                drow("FleetID") = ecopathDS.FleetDBID(ts.DatPool)
                If (ts.DatPoolSec >= 1) And (ts.DatPool < ecopathDS.GroupDBID.Length) Then
                    drow("GroupID") = ecopathDS.GroupDBID(ts.DatPoolSec)
                Else
                    drow("GroupID") = 0
                End If
                writerFleet.AddRow(drow)
            Catch ex As Exception
                ' Woops
                bSucces = False
            End Try

            bSucces = bSucces And Me.m_db.ReleaseWriter(writerFleet, True)

            Return bSucces

        End Function

#End Region ' Import helpers

#End Region ' Import

#Region " Load "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load all time series for a given dataset.
        ''' </summary>
        ''' <param name="iDataset">Index of dataset to load.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function LoadTimeSeriesDataset(ByVal iDataset As Integer) As Boolean _
                  Implements DataSources.IEcosimDatasource.LoadTimeSeriesDataset

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData
            Dim strSQL As String = ""
            Dim reader As IDataReader = Nothing
            Dim readerSub As IDataReader = Nothing
            Dim astrTimeValues() As String
            Dim iTimeSeriesID As Integer = 0
            Dim iSeries As Integer = 1
            Dim iPoint As Integer = 0
            Dim bSucces As Boolean = True

            tsDS.ClearTimeSeries()
            tsDS.ActiveDatasetIndex = iDataset
            tsDS.nMaxYears = tsDS.nDatasetNumPoints(iDataset)
            tsDS.DataSetInterval = tsDS.DataSetIntervals(iDataset)

            ' JS 20oct07: data source should NOT do this; is responsibility of core logic
            tsDS.nGroups = ecopathDS.NumGroups

            ' JS 02Nov12: The database structure cannot cascadingly delete time series when
            ' a pool code target is deleted. This is not a big problem, as PoolCode (indexes)
            ' are translated to database IDs (persistent) upon import. However, lingering 
            ' time series create a bit of a mess.

            If (iDataset > 0) Then

                Try
                    tsDS.nTimeSeries = CInt(Me.m_db.GetValue(String.Format("SELECT COUNT(*) FROM EcosimTimeSeries WHERE (DatasetID={0})", tsDS.iDatasetDBID(iDataset))))
                Catch ex As Exception
                    tsDS.nTimeSeries = 0
                End Try

            End If

            tsDS.RedimTimeSeries()
            tsDS.RedimEnabledTimeSeries()

            If tsDS.nTimeSeries = 0 Then Return bSucces

            strSQL = String.Format("SELECT * FROM EcosimTimeSeries WHERE (DatasetID={0}) ORDER BY Sequence ASC", tsDS.iDatasetDBID(iDataset))
            reader = Me.m_db.GetReader(strSQL)
            Try
                While reader.Read()

                    tsDS.iTimeSeriesDBID(iSeries) = CInt(reader("TimeSeriesID"))
                    tsDS.strName(iSeries) = CStr(reader("DatName"))
                    tsDS.TimeSeriesType(iSeries) = DirectCast(CInt(reader("DatType")), eTimeSeriesType)
                    tsDS.sWeight(iSeries) = CSng(reader("WtType"))
                    tsDS.sCV(iSeries) = CSng(Me.m_db.ReadSafe(reader, "CV", 0.0!))

                    Select Case cTimeSeriesFactory.TimeSeriesCategory(CType(tsDS.TimeSeriesType(iSeries), eTimeSeriesType))

                        Case eTimeSeriesCategoryType.Group
                            Dim iIndex As Integer = 0
                            readerSub = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimTimeSeriesGroup WHERE (TimeSeriesID={0})", reader("TimeSeriesID")))
                            Try
                                readerSub.Read()
                                iIndex = Array.IndexOf(ecopathDS.GroupDBID, CInt(readerSub("GroupID")))
                            Catch ex As Exception
                                iIndex = -1
                            End Try
                            Me.m_db.ReleaseReader(readerSub)
                            readerSub = Nothing
                            tsDS.iPool(iSeries) = iIndex

                        Case eTimeSeriesCategoryType.Fleet,
                             eTimeSeriesCategoryType.FleetGroup
                            Dim iIndex As Integer = 0
                            Dim iIndexSec As Integer = 0
                            readerSub = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimTimeSeriesFleet WHERE (TimeSeriesID={0})", reader("TimeSeriesID")))
                            Try
                                readerSub.Read()
                                iIndex = Array.IndexOf(ecopathDS.FleetDBID, CInt(readerSub("FleetID")))
                                iIndexSec = Array.IndexOf(ecopathDS.GroupDBID, CInt(m_db.ReadSafe(readerSub, "GroupID", 0)))
                            Catch ex As Exception
                                iIndex = -1
                                iIndexSec = -1
                            End Try
                            Me.m_db.ReleaseReader(readerSub)
                            readerSub = Nothing
                            tsDS.iPool(iSeries) = iIndex
                            tsDS.iPoolSec(iSeries) = iIndexSec

                        Case eTimeSeriesCategoryType.Forcing
                            Debug.Assert(False, String.Format("Time series {0} should have been imported as a forcing function", reader("TimeSeriesID")))
                            bSucces = False

                        Case eTimeSeriesCategoryType.NotSet
                            Debug.Assert(False, String.Format("Time series {0} is of an unknown type", reader("TimeSeriesID")))
                            bSucces = False

                    End Select


                    astrTimeValues = CStr(reader("TimeValues")).Split(CChar(" "))

                    For iPoint = 1 To Math.Min(tsDS.nDatasetNumPoints(iDataset), astrTimeValues.Length)
                        Try
                            tsDS.sValues(iPoint, iSeries) = cStringUtils.ConvertToSingle(astrTimeValues(iPoint - 1))
                        Catch ex As Exception
                            ' Woops
                        End Try
                    Next

                    iSeries += 1
                End While

                Me.m_db.ReleaseReader(reader)
            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        Private Function SaveTimeSeries(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerGroups As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerFleets As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim dtFleets As DataTable = Nothing
            Dim dtGroups As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bHasRow As Boolean = False
            Dim sbValues As New StringBuilder()
            Dim iPoolID As Integer = 0
            Dim bSucces As Boolean = True

            Try

                ' Time series are scenario-independent
                writer = Me.m_db.GetWriter("EcosimTimeSeries")
                dt = writer.GetDataTable()

                writerGroups = Me.m_db.GetWriter("EcosimTimeSeriesGroup")
                dtGroups = writerGroups.GetDataTable()

                writerFleets = Me.m_db.GetWriter("EcosimTimeSeriesFleet")
                dtFleets = writerFleets.GetDataTable()

                For iTS As Integer = 1 To tsDS.nTimeSeries

                    drow = dt.Rows.Find(tsDS.iTimeSeriesDBID(iTS))
                    Debug.Assert(drow IsNot Nothing, String.Format("Cannot find time series {0}", tsDS.iTimeSeriesDBID(iTS)))

                    drow.BeginEdit()
                    drow("DatName") = tsDS.strName(iTS)
                    drow("DatType") = tsDS.TimeSeriesType(iTS)
                    drow("WtType") = tsDS.sWeight(iTS)
                    drow("CV") = tsDS.sCV(iTS)

                    ' Concoct time series memo
                    sbValues.Length = 0
                    For iPoint As Integer = 1 To tsDS.nDatasetNumPoints(tsDS.ActiveDatasetIndex)
                        If (iPoint > 1) Then sbValues.Append(" ")
                        sbValues.Append(cStringUtils.FormatSingle(tsDS.sValues(iPoint, iTS)))
                    Next
                    drow("TimeValues") = sbValues.ToString()

                    drow.EndEdit()

                    Select Case cTimeSeriesFactory.TimeSeriesCategory(tsDS.TimeSeriesType(iTS))

                        Case eTimeSeriesCategoryType.Fleet,
                             eTimeSeriesCategoryType.FleetGroup

                            drow = dtFleets.Rows.Find(tsDS.iTimeSeriesDBID(iTS))
                            bHasRow = (drow Is Nothing = False)

                            If bHasRow Then drow.BeginEdit() Else drow = writerFleets.NewRow() : drow("TimeSeriesID") = tsDS.iTimeSeriesDBID(iTS)

                            If (tsDS.iPool(iTS) > 0) Then
                                iPoolID = ecopathDS.FleetDBID(tsDS.iPool(iTS))
                            Else
                                iPoolID = 0
                            End If
                            drow("FleetID") = iPoolID

                            If (tsDS.iPoolSec(iTS) > 0) Then
                                iPoolID = ecopathDS.GroupDBID(tsDS.iPoolSec(iTS))
                            Else
                                iPoolID = 0
                            End If
                            drow("GroupID") = iPoolID

                            If bHasRow Then drow.EndEdit() Else writerFleets.AddRow(drow)

                        Case eTimeSeriesCategoryType.Group

                            drow = dtGroups.Rows.Find(tsDS.iTimeSeriesDBID(iTS))
                            bHasRow = (drow Is Nothing = False)

                            If bHasRow Then drow.BeginEdit() Else drow = writerGroups.NewRow() : drow("TimeSeriesID") = tsDS.iTimeSeriesDBID(iTS)

                            If (tsDS.iPool(iTS) > 0) Then
                                iPoolID = ecopathDS.GroupDBID(tsDS.iPool(iTS))
                            Else
                                iPoolID = 0
                            End If

                            drow("GroupID") = iPoolID
                            If bHasRow Then drow.EndEdit() Else writerGroups.AddRow(drow)

                        Case eTimeSeriesCategoryType.Forcing, eTimeSeriesCategoryType.NotSet
                            Debug.Assert(False)

                    End Select

                Next iTS

                Me.m_db.ReleaseWriter(writerGroups)
                Me.m_db.ReleaseWriter(writerFleets)
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception

                bSucces = False

            End Try
            Return bSucces

        End Function

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds a time series to the data source.
        ''' </summary>
        ''' <param name="strName">Name of the new Time Series to add.</param>
        ''' <param name="timeSeriesType"><see cref="eTimeSeriesType">Type</see> of the time series.</param>
        ''' <param name="iPool">Group/fleet code to assign to TS.</param>
        ''' <param name="sWeight">Relative weight of TS.</param>
        ''' <param name="asValues">Initial values to set in the TS.</param>
        ''' <param name="iShapeID">Database ID assigned to the new TS.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function AppendTimeSeries(ByVal strName As String,
                                         ByVal iPool As Integer,
                                         ByVal iPoolSec As Integer,
                                         ByVal timeSeriesType As eTimeSeriesType,
                                         ByVal sWeight As Single,
                                         ByVal asValues() As Single,
                                         ByRef iShapeID As Integer) As Boolean _
            Implements IEcosimDatasource.AppendTimeSeries

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim tsDS As cTimeSeriesDataStructures = Me.m_core.m_TSData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim writerSub As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iPosition As Integer = 0
            Dim drowSub As DataRow = Nothing
            Dim bSucces As Boolean = True
            Dim sbValues As New StringBuilder

            If tsDS.ActiveDatasetIndex < 0 Then
                'Console.WriteLine("No dataset loaded, cannot add time series")
                Return False
            End If

            iShapeID = CInt(Me.m_db.GetValue("SELECT MAX(TimeSeriesID) FROM EcosimTimeSeries", 0)) + 1
            iPosition = CInt(Me.m_db.GetValue("SELECT MAX(Sequence) FROM EcosimTimeSeries", 0)) + 1

            Try
                ' Start writing, protect sequence
                writer = Me.m_db.GetWriter("EcosimTimeSeries")
                drow = writer.NewRow()
                drow("TimeSeriesID") = iShapeID
                drow("DatasetID") = tsDS.iDatasetDBID(tsDS.ActiveDatasetIndex)
                drow("DatName") = strName
                drow("DatType") = timeSeriesType
                drow("Sequence") = iPosition
                drow("WtType") = sWeight

                ' Concoct time series memo
                For iYear As Integer = 0 To asValues.Length - 1
                    If (iYear > 0) Then sbValues.Append(" ")
                    sbValues.Append(cStringUtils.FormatSingle(asValues(iYear)))
                Next
                drow("TimeValues") = sbValues.ToString()
                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer)

                Select Case cTimeSeriesFactory.TimeSeriesCategory(timeSeriesType)

                    Case eTimeSeriesCategoryType.Fleet,
                         eTimeSeriesCategoryType.FleetGroup

                        writerSub = Me.m_db.GetWriter("EcosimTimeSeriesFleet")
                        drowSub = writerSub.NewRow()
                        drowSub("TimeSeriesID") = iShapeID
                        drowSub("FleetID") = ecopathDS.FleetDBID(iPool)
                        drowSub("GroupID") = If(iPoolSec > 0, ecopathDS.GroupDBID(iPoolSec), 0)
                        writerSub.AddRow(drowSub)
                        Me.m_db.ReleaseWriter(writerSub)

                    Case eTimeSeriesCategoryType.Group
                        writerSub = Me.m_db.GetWriter("EcosimTimeSeriesGroup")
                        drowSub = writerSub.NewRow()
                        drowSub("TimeSeriesID") = iShapeID
                        drowSub("GroupID") = ecopathDS.GroupDBID(iPool)
                        writerSub.AddRow(drowSub)
                        Me.m_db.ReleaseWriter(writerSub)

                    Case Else
                        Debug.Assert(False)

                End Select

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while appending time series {1}", ex.Message, strName))
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes a time series from the data source.
        ''' </summary>
        ''' <param name="iTimeSeriesID">Database ID of the time series to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Friend Function RemoveTimeSeries(ByVal iTimeSeriesID As Integer) As Boolean _
                Implements DataSources.IEcosimDatasource.RemoveTimeSeries

            Dim bSucces As Boolean = True
            Try
                Me.m_db.Execute(String.Format("DELETE FROM EcosimTimeSeries WHERE (TimeSeriesID = {0})", iTimeSeriesID))
            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Modify

#End Region ' Time series

#Region " Environmental drivers "

        Private Function LoadEcosimCapacityDrivers() As Boolean

            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iScenarioID As Integer = ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario)
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenarioCapacityDrivers WHERE (ScenarioID={0})", iScenarioID))
            Try

                While reader.Read()
                    Dim iGroup As Integer = Array.IndexOf(ecosimDS.GroupDBID, CInt(reader("GroupID")))
                    Dim iShapeDriver As Integer = Array.IndexOf(ecosimDS.ForcingDBIDs, CInt(reader("DriverID")))
                    Dim iShapeResponse As Integer = Array.IndexOf(Me.m_core.CapacityMapInteractionManager.MediationData.MediationDBIDs, CInt(reader("ResponseID")))

                    If (iGroup > 0) And (iShapeDriver > 0) And (iShapeResponse > 0) Then
                        ecosimDS.EnvRespFuncIndex(iShapeDriver, iGroup) = iShapeResponse
                    End If
                End While
            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)

            Return bSucces

        End Function

        Private Function SaveEcosimCapacityDrivers(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim medDS As cMediationDataStructures = Me.m_core.CapacityMapInteractionManager.MediationData
            Dim iScenarioID As Integer = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Clear
            Me.m_db.Execute(String.Format("DELETE FROM EcosimScenarioCapacityDrivers WHERE (ScenarioID={0})", iScenarioID))
            writer = Me.m_db.GetWriter("EcosimScenarioCapacityDrivers")

            Try
                For iShapeDriver As Integer = 1 To ecosimDS.NumForcingShapes
                    For iGroup As Integer = 1 To ecopathDS.NumGroups
                        If (ecosimDS.EnvRespFuncIndex(iShapeDriver, iGroup) > 0) Then
                            drow = writer.NewRow()
                            drow("ScenarioID") = iScenarioID
                            drow("GroupID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iGroup))
                            drow("ResponseID") = medDS.MediationDBIDs(ecosimDS.EnvRespFuncIndex(iShapeDriver, iGroup))
                            drow("DriverID") = ecosimDS.ForcingDBIDs(iShapeDriver)
                            writer.AddRow(drow)
                        End If
                    Next iGroup
                Next iShapeDriver
            Catch ex As Exception
                bSucces = False
            End Try

            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

            Return bSucces

        End Function

#End Region ' Environmental drivers

#Region " MSE "

#Region " Load "

        Private Function LoadEcosimMSE(ByVal iScenarioID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim reader As IDataReader = Me.m_db.GetReader(String.Format("SELECT * FROM EcoSimScenarioMSE WHERE (ScenarioID={0})", iScenarioID))
            Dim bSucces As Boolean = True

            If (reader IsNot Nothing) Then
                If (reader.Read()) Then
                    Try
                        mseDS.AssessMethod = DirectCast(Me.m_db.ReadSafe(reader, "AssessMethod", eAssessmentMethods.CatchEstmBio), eAssessmentMethods)
                        mseDS.AssessPower = CSng(Me.m_db.ReadSafe(reader, "AssessPower", 1))
                        mseDS.NTrials = CInt(Me.m_db.ReadSafe(reader, "NTrials", 10))
                        mseDS.MSYStartTimeIndex = CInt(Me.m_db.ReadSafe(reader, "StartIndex", 2))
                        mseDS.MSEMaxEffort = CSng(Me.m_db.ReadSafe(reader, "MaxEffort", cMSEDataStructures.MSE_DEFAULT_MAXEFFORT))
                    Catch ex As Exception
                        Me.LogMessage(String.Format("Error {0} occurred while reading EcopathPSD", ex.Message))
                        bSucces = False
                    End Try
                End If

                Me.m_db.ReleaseReader(reader)
                reader = Nothing

            End If

            Return bSucces
        End Function

#End Region ' Load

#Region " Save "

        Private Function SaveEcosimMSE(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim mseDS As cMSEDataStructures = Me.m_core.m_MSEData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim strSQL As String = ""
            Dim iScenarioID As Integer = 0
            Dim bSucces As Boolean = True

            ' Obtain mapped scenario ID
            iScenarioID = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))

            strSQL = String.Format("DELETE FROM EcosimScenarioMSE WHERE (ScenarioID={0})", iScenarioID)
            bSucces = Me.m_db.Execute(strSQL)

            Try
                writer = Me.m_db.GetWriter("EcosimScenarioMSE")
                drow = writer.NewRow()

                drow("ScenarioID") = iScenarioID
                drow("AssessMethod") = mseDS.AssessMethod
                drow("AssessPower") = mseDS.AssessPower
                drow("MaxEffort") = mseDS.MSEMaxEffort
                drow("Ntrials") = mseDS.NTrials
                drow("StartIndex") = mseDS.MSYStartTimeIndex

                writer.AddRow(drow)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving MSE", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, True)

            Return bSucces

        End Function

#End Region ' Save

#End Region ' MSE

#Region " Catchabilities "

        Private Function LoadEcosimCatchabilities(iScenarioID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim reader As IDataReader = Nothing
            Dim iFleetID, iFleet As Integer
            Dim iGroupID, iGroup As Integer
            Dim zScale As String = ""
            Dim astrMemoBits() As String
            Dim bSucces As Boolean = True

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcosimScenarioFleetGroupCatchability WHERE (ScenarioID={0})", iScenarioID))
            While reader.Read
                iFleetID = CInt(Me.m_db.ReadSafe(reader, "FleetID", -1))
                iFleet = Array.IndexOf(ecosimDS.FleetDBID, iFleetID)

                iGroupID = CInt(Me.m_db.ReadSafe(reader, "GroupID", -1))
                iGroup = Array.IndexOf(ecosimDS.GroupDBID, iGroupID)

                If (iFleet > 0 And iGroup > 0) Then
                    zScale = CStr(Me.m_db.ReadSafe(reader, "zScale", ""))
                    ' Store points
                    If Not String.IsNullOrWhiteSpace(zScale) Then
                        ' #Yes: split and process
                        astrMemoBits = zScale.Trim.Split(CChar(" "))
                        For j As Integer = 1 To Math.Min(ecosimDS.NTimes, astrMemoBits.Length)
                            ecosimDS.relQt(iFleet, iGroup, j) = cStringUtils.ConvertToSingle(astrMemoBits(j - 1), 0)
                        Next

                        If (ecopathDS.Landing(iFleet, iGroup) + ecopathDS.Discard(iFleet, iGroup)) > 0 Then
                            If ecosimDS.relQt(iFleet, iGroup, 1) = cCore.NULL_VALUE Then
                                Me.m_core.setDefaultCatchabilities(iFleet, iGroup)
                            End If 'ecosimDS.relQt(iFleet, iGroup, 1) = cCore.NULL_VALUE
                        End If 'ecopathDS.Landing(iFleet, iGroup) + ecopathDS.Discard(iFleet, iGroup)) > 0
                    End If ' Not String.IsNullOrWhiteSpace(zScale)
                End If '(iFleet > 0 And iGroup > 0)

            End While
            Me.m_db.ReleaseReader(reader)

            Return bSucces

        End Function

        Private Function SaveEcosimCatchabilities(idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim iScenarioID As Integer = idm.GetID(eDataTypes.EcoSimScenario, ecopathDS.EcosimScenarioDBID(ecopathDS.ActiveEcosimScenario))

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim sbZScale As StringBuilder
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Me.m_db.Execute("DELETE * FROM EcosimScenarioFleetGroupCatchability WHERE ScenarioID=" & iScenarioID)

            writer = Me.m_db.GetWriter("EcosimScenarioFleetGroupCatchability")
            dt = writer.GetDataTable()
            Try
                For iFleet As Integer = 1 To ecopathDS.NumFleet
                    For iGroup As Integer = 1 To ecopathDS.NumGroups

                        If (ecopathDS.Landing(iFleet, iGroup) + ecopathDS.Discard(iFleet, iGroup) > 0) Then

                            drow = writer.NewRow()

                            drow("ScenarioID") = iScenarioID
                            ' Generic fleet DBID mapped to Ecopath fleet ID, see SaveEcosimFleets
                            drow("FleetID") = idm.GetID(eDataTypes.FleetInput, ecopathDS.FleetDBID(iFleet))
                            ' Ecosim fleet ID mapped to Ecopath fleet ID, see SaveEcosimGroups
                            drow("GroupID") = idm.GetID(eDataTypes.EcoSimGroupInput, ecopathDS.GroupDBID(iGroup))

                            'This fleet/group has catch but the catchabilities have not been set
                            'Set the default catchability before saving
                            If ecosimDS.relQt(iFleet, iGroup, 1) = cCore.NULL_VALUE Then
                                Me.m_core.setDefaultCatchabilities(iFleet, iGroup)
                            End If

                            sbZScale = New StringBuilder()
                            ' Todo: Check upper limit
                            For ipt As Integer = 1 To ecosimDS.NTimes
                                If (ipt > 1) Then sbZScale.Append(" ")
                                sbZScale.Append(cStringUtils.FormatSingle(ecosimDS.relQt(iFleet, iGroup, ipt)))
                            Next
                            drow("Zscale") = sbZScale.ToString()

                            writer.AddRow(drow)

                        End If
                    Next
                Next

            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_db.ReleaseWriter(writer, True)
            Return bSucces

        End Function

#End Region ' Catchabilities

#End Region ' EcoSim

#Region " Ecospace "

#Region " Diagnostics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if the data source has unsaved changes for Ecospace.
        ''' </summary>
        ''' <returns>True if the data source has pending changes for Ecospace.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsEcospaceModified() As Boolean Implements DataSources.IEcospaceDatasource.IsEcospaceModified

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData

            ' Hmm, maybe the data source should have a better way to 'remember' whether a space scenario has been loaded.
            If Not Me.IsConnected() Then Return False
            If ecopathDS.ActiveEcospaceScenario < 0 Then Return False

            Return Me.IsChanged(s_EcospaceComponents)

        End Function

#End Region ' Diagnostics

#Region " Scenarios "

#Region " Load "

        Public Function LoadEcospaceScenario(ByVal iScenarioID As Integer) As Boolean _
                Implements DataSources.IEcospaceDatasource.LoadEcospaceScenario

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim spatialDS As cSpatialDataStructures = Me.m_core.m_SpatialData
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True

            'jb Jan-17-07 moved SetDefaults to run before any data has been loaded
            'this will load the default values into Ecospace before anything else is loaded
            ecospaceDS.NGroups = ecopathDS.NumGroups
            ecospaceDS.nFleets = ecopathDS.NumFleet
            ecospaceDS.nLiving = ecopathDS.NumLiving
            ecospaceDS.nImportanceLayers = CInt(Me.m_db.GetValue(String.Format("SELECT COUNT(*) FROM EcospaceScenarioWeightLayer WHERE ScenarioID={0}", iScenarioID), 0))
            ecospaceDS.nEnvironmentalDriverLayers = CInt(Me.m_db.GetValue(String.Format("SELECT COUNT(*) FROM EcospaceScenarioDriverLayer WHERE ScenarioID={0}", iScenarioID), 0))

            ' Next is a dangerous solution that may need to be revamped. It is assumed that
            ' SetDefaults properly redimensions the ecospaceDS group variables, which
            ' may wreck havoc if the implementation of SetDefaults were to change.
            ecospaceDS.SetDefaults()

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenario WHERE (ScenarioID={0})", iScenarioID))
            Try
                ' Read the one record
                reader.Read()
                ' Remember link with Ecosim scenario, if any
                ecospaceDS.EcosimScenarioDBID = CInt(Me.m_db.ReadSafe(reader, "EcosimScenarioID", cCore.NULL_VALUE))
                ecospaceDS.InRow = CInt(reader("Inrow"))
                ecospaceDS.InCol = CInt(reader("Incol"))
                ecospaceDS.CellLength = CSng(reader("CellLength"))
                ecospaceDS.Lat1 = CSng(Me.m_db.ReadSafe(reader, "MinLat", 0))
                ecospaceDS.Lon1 = CSng(Me.m_db.ReadSafe(reader, "MinLon", 0))
                ecospaceDS.TimeStep = CSng(Me.m_db.ReadSafe(reader, "TimeStep", 0))
                ecospaceDS.PredictEffort = (CInt(Me.m_db.ReadSafe(reader, "PredictEffort", True)) <> 0)
                ecospaceDS.AssumeSquareCells = (CInt(Me.m_db.ReadSafe(reader, "AssumeSquareCells", True)) <> 0)
                ecospaceDS.ProjectionString = CStr(Me.m_db.ReadSafe(reader, "CoordinateSystemWKT", cEcospaceDataStructures.DEFAULT_COORDINATESYSTEM))

                ' JS 05apr08: pragmatic fix to prevent mayhem
                If ecospaceDS.TimeStep <= 0 Then ecospaceDS.TimeStep = 1.0! / cCore.N_MONTHS

                ecospaceDS.TotalTime = CSng(reader("TotalTime"))
                ecospaceDS.IFDPower = CSng(reader("IFDPower"))
                ecospaceDS.nSpaceSolverThreads = CInt(reader("NumThreads"))
                ecospaceDS.nGridSolverThreads = CInt(reader("NumThreads"))
                ecospaceDS.nEffortDistThreads = CInt(reader("NumThreads"))
                ecospaceDS.nRegions = CInt(Me.m_db.ReadSafe(reader, "NumRegions", 0))
                ecospaceDS.AdjustSpace = (CInt(reader("AdjustSpace")) <> 0)
                ecospaceDS.UseExact = (CInt(reader("UseExact")) <> 0)
                ecospaceDS.Tol = CSng(Me.m_db.ReadSafe(reader, "Tolerance", 0.01!))
                ecospaceDS.bUseEffortDistThreshold = CInt(Me.m_db.ReadSafe(reader, "UseEffortDistrThreshold", 0)) = 1
                ecospaceDS.EffortDistThreshold = CSng(Me.m_db.ReadSafe(reader, "EffortDistrThreshold", 10000))

                stanzaDS.NPacketsMultiplier = CSng(reader("NumPacketsMultiplier"))

                Select Case CInt(reader("ModelType"))
                    Case 0
                        ecospaceDS.NewMultiStanza = False
                        ecospaceDS.UseIBM = False
                    Case 1
                        ecospaceDS.UseIBM = True
                        ecospaceDS.NewMultiStanza = False
                    Case Else
                        ecospaceDS.UseIBM = False
                        ecospaceDS.NewMultiStanza = True
                End Select

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Ecospace Scenario {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)

            ' JS 08Jul14: redimForRun is called (too) many times?

            'set the size of the variables that hold the map data to InRow and InCol
            'Call cEcospace.redimForRun() First because it allocates bigger blocks of memory
            'this should help Out of Memory exceptions caused by heap fragmentation by doing the big stuff first
            Me.m_core.m_Ecospace.redimForRun()
            ecospaceDS.ReDimMapDims()

            ' Set active scenario
            ecopathDS.ActiveEcospaceScenario = Array.IndexOf(ecopathDS.EcospaceScenarioDBID, iScenarioID)

            ' Load base map first
            bSucces = bSucces And Me.LoadEcospaceMap(iScenarioID)
            bSucces = bSucces And Me.LoadEcospaceHabitats(iScenarioID)
            bSucces = bSucces And Me.LoadEcospaceMPAs(iScenarioID)
            bSucces = bSucces And Me.LoadEcospaceGroups(iScenarioID)
            bSucces = bSucces And Me.LoadEcospaceFleets(iScenarioID)
            bSucces = bSucces And Me.LoadEcospaceMonthlyMaps(iScenarioID)
            bSucces = bSucces And Me.LoadEcospaceWeightLayers(iScenarioID)
            bSucces = bSucces And Me.LoadEcospaceDriverLayers(iScenarioID)
            bSucces = bSucces And Me.LoadEcospaceDataConnections(iScenarioID)
            bSucces = bSucces And Me.LoadAuxillaryData()

            Me.ClearChanged(s_EcospaceComponents)

            Return bSucces
        End Function

#End Region ' Load

#Region " Save "

        Public Function SaveEcospaceScenarioAs(ByVal strScenarioName As String, ByVal strDescription As String,
             ByVal strAuthor As String, ByVal strContact As String, ByRef iScenarioID As Integer) As Boolean _
                    Implements IEcospaceDatasource.SaveEcospaceScenarioAs

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Delete existing scenario
            Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenario WHERE ScenarioName='{0}'", strScenarioName))
            iScenarioID = CInt(Me.m_db.GetValue("SELECT MAX(ScenarioID) FROM EcospaceScenario", 0)) + 1

            Try
                writer = Me.m_db.GetWriter("EcospaceScenario")
                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("ScenarioName") = strScenarioName
                drow("Description") = strDescription
                drow("Author") = strAuthor
                drow("Contact") = strContact
                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer)
            Catch ex As Exception
                bSucces = False
            End Try

            Return (bSucces And Me.SaveEcospaceScenario(iScenarioID))

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Updates the active ecospace scenario under the given ID in the data source.
        ''' This method is the one external interface to save an Ecospace scenario
        ''' and everything under it.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID of the scenario to update. This
        ''' parameter is optional; if left to zero the active scenario will be saved.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Friend Function SaveEcospaceScenario(ByVal iScenarioID As Integer) As Boolean _
                Implements DataSources.IEcospaceDatasource.SaveEcospaceScenario

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData

            ' Abort if there is no active scenario
            If ecopathDS.ActiveEcospaceScenario <= 0 Then Return False

            Dim iScenario As Integer = Array.IndexOf(ecopathDS.EcospaceScenarioDBID, iScenarioID)
            Dim iActiveScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim idm As New cIDMappings()
            Dim bSucces As Boolean = True

            If iScenarioID = 0 Then iScenarioID = iActiveScenarioID

            ' Duplicating a scenario?
            If iScenarioID <> iActiveScenarioID Then
                ' #Yes: add ID mapping to allow copying of scenario content
                idm.Add(eDataTypes.EcoSpaceScenario, iActiveScenarioID, iScenarioID)
            End If

            ' Start transaction
            bSucces = Me.m_db.BeginTransaction()

            Try
                ' Save scenario
                bSucces = bSucces And Me.SaveEcospaceScenario(idm)
            Catch ex As Exception
                bSucces = False
            End Try

            ' Commit transaction
            If bSucces Then
                bSucces = Me.m_db.CommitTransaction(True)
            Else
                Me.m_db.RollbackTransaction()
            End If

            If (bSucces) Then
                ' Clear changed admin
                Me.ClearChanged(s_EcospaceComponents)
                ' Reload ecospace scenario definitions 
                Me.LoadEcospaceScenarioDefinitions()
            End If

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Internal method; updates the active ecospace scenario in the data source,
        ''' optionally saving to a different scenario.
        ''' </summary>
        ''' <param name="idm"><see cref="cIDMappings">ID mapping</see> providing
        ''' ID mappings when saving to a different scenario ID.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function SaveEcospaceScenario(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenario As Integer = ecopathDS.ActiveEcospaceScenario
            Dim iScenarioID As Integer = 0
            Dim bDuplicating As Boolean = False
            Dim bSucces As Boolean = True

            iScenarioID = idm.GetID(eDataTypes.EcoSpaceScenario, ecopathDS.EcospaceScenarioDBID(iScenario))
            bDuplicating = ((iScenarioID) <> ecopathDS.EcospaceScenarioDBID(iScenario))

            Try

                writer = Me.m_db.GetWriter("EcospaceScenario")
                dt = writer.GetDataTable()
                drow = dt.Rows.Find(iScenarioID)

                drow.BeginEdit()
                drow("Inrow") = ecospaceDS.InRow
                drow("Incol") = ecospaceDS.InCol
                drow("CellLength") = ecospaceDS.CellLength
                drow("MinLon") = ecospaceDS.Lon1
                drow("MinLat") = ecospaceDS.Lat1
                drow("TimeStep") = ecospaceDS.TimeStep
                drow("PredictEffort") = ecospaceDS.PredictEffort
                drow("AssumeSquareCells") = ecospaceDS.AssumeSquareCells
                drow("CoordinateSystemWKT") = ecospaceDS.ProjectionString

                drow("TotalTime") = ecospaceDS.TotalTime
                drow("IFDPower") = ecospaceDS.IFDPower
                drow("NumThreads") = ecospaceDS.nSpaceSolverThreads
                drow("NumRegions") = ecospaceDS.nRegions
                drow("NumPacketsMultiplier") = stanzaDS.NPacketsMultiplier
                drow("NumRegions") = ecospaceDS.nRegions

                drow("ModelType") = 0
                If ecospaceDS.UseIBM Then drow("ModelType") = 1
                If ecospaceDS.NewMultiStanza Then drow("ModelType") = 2

                drow("AdjustSpace") = ecospaceDS.AdjustSpace
                drow("UseExact") = ecospaceDS.UseExact
                drow("UseEffortDistrThreshold") = If(ecospaceDS.bUseEffortDistThreshold, 1, 0)
                drow("EffortDistrThreshold") = ecospaceDS.EffortDistThreshold

                If Me.Version >= 6.01 Then
                    drow("Tolerance") = ecospaceDS.Tol
                End If
                ' ------------------------------------------
                drow("LastSaved") = cDateUtils.DateToJulian()
                drow("LastSavedVersion") = cAssemblyUtils.GetVersion().ToString

                drow.EndEdit()

                ' Save changes
                bSucces = bSucces And Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving ecospace scenario {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try

            bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcoSpaceScenario, ecopathDS.EcospaceScenarioDBID(iScenario))
            bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceStatistics, ecopathDS.EcospaceScenarioDBID(iScenario))

            bSucces = bSucces And Me.SaveEcospaceMap(idm)
            bSucces = bSucces And Me.SaveEcospaceHabitats(idm)
            bSucces = bSucces And Me.SaveEcospaceMPAs(idm)
            bSucces = bSucces And Me.SaveEcospaceGroups(idm)
            bSucces = bSucces And Me.SaveEcospaceFleets(idm)
            bSucces = bSucces And Me.SaveEcospaceMonthlyMaps(idm)
            bSucces = bSucces And Me.SaveEcospaceWeightLayers(idm)
            bSucces = bSucces And Me.SaveEcospaceCapacityMaps(idm)
            bSucces = bSucces And Me.SaveEcospaceDataConnections(idm)

            Return bSucces

        End Function

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an ecospace scenario to the data source.
        ''' </summary>
        ''' <param name="strScenarioName">Name to assign to new scenario.</param>
        ''' <param name="strDescription">Description to assign to new scenario.</param>
        ''' <param name="strAuthor">Author to assign to the new scenario.</param>
        ''' <param name="strContact">Contact info to assign to the new scenario.</param>
        ''' <param name="iScenarioID">Database ID assigned to the new scenario.</param>
        ''' <param name="sCellLength">Length of cells, in km.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function AppendEcospaceScenario(ByVal strScenarioName As String, ByVal strDescription As String,
                 ByVal strAuthor As String, ByVal strContact As String,
                 ByVal InRow As Integer, ByVal InCol As Integer,
                 ByVal sOriginLat As Single, ByVal sOriginLon As Single, ByVal sCellLength As Single,
                 ByRef iScenarioID As Integer) As Boolean _
                 Implements DataSources.IEcospaceDatasource.AppendEcospaceScenario

            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iIDtmp As Integer = 0
            Dim aiNewMap(InRow, InCol) As Integer
            Dim asNewMap(InRow, InCol) As Single
            Dim bSucces As Boolean = True

            ' Init maps
            For iRow As Integer = 1 To InRow
                For iCol As Integer = 1 To InCol
                    aiNewMap(iRow, iCol) = 1
                    asNewMap(iRow, iCol) = 1.0!
                Next
            Next

            Try
                iScenarioID = CInt(Me.m_db.GetValue("SELECT MAX(ScenarioID) FROM EcospaceScenario", 0)) + 1
                writer = Me.m_db.GetWriter("EcospaceScenario")

                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("ScenarioName") = strScenarioName
                drow("Description") = strDescription
                drow("Author") = strAuthor
                drow("Contact") = strContact
                drow("LastSaved") = cDateUtils.DateToJulian()
                drow("InRow") = InRow
                drow("InCol") = InCol
                drow("CellLength") = sCellLength
                drow("MinLat") = sOriginLat
                drow("MinLon") = sOriginLon
                drow("ModelType") = 2
                drow("DepthMap") = cStringUtils.ArrayToString(aiNewMap, InRow, InCol)
                drow("DepthAMap") = cStringUtils.ArrayToString(aiNewMap, InRow, InCol)
                drow("RelPPMap") = cStringUtils.ArrayToString(asNewMap, InRow, InCol)
                drow("RelCinMap") = cStringUtils.ArrayToString(asNewMap, InRow, InCol)
                drow("RegionMap") = "" ' Region map empty for new scenario
                drow("ExclusionMap") = "" ' Exclusion map empty for new scenario
                drow("PredictEffort") = True
                writer.AddRow(drow)

                Me.m_db.ReleaseWriter(writer)

                ' First duplicate all Ecospace 'objects'
                For i As Integer = 1 To ecopathDS.NumGroups
                    ' Add group to the new scenario
                    bSucces = bSucces And Me.AddEcospaceGroup(ecopathDS.GroupDBID(i), iScenarioID,
                                                              (ecopathDS.PP(i) = 2.0), iIDtmp)
                Next

                For i As Integer = 1 To ecopathDS.NumFleet
                    ' Add fleet to the new scenario
                    bSucces = bSucces And Me.AddEcospaceFleet(ecopathDS.FleetDBID(i), iScenarioID, iIDtmp)
                Next

                ' Add default 'All' habitat
                bSucces = bSucces And Me.AddEcospaceHabitat("All", iScenarioID, iIDtmp)

                ' Reload scenario definitions
                bSucces = bSucces And Me.LoadEcospaceScenarioDefinitions()

                Me.ClearChanged(s_EcospaceComponents)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while appending Scenario {1}", ex.Message, strScenarioName))
                bSucces = False
            End Try

            Return bSucces

        End Function

        Public Function RemoveEcospaceScenario(ByVal iScenarioID As Integer) As Boolean _
                Implements DataSources.IEcospaceDatasource.RemoveEcospaceScenario

            Dim bSucces As Boolean = True

            If Me.Version < 6.120001 Then
                Try
                    ' Delete 'soft links' present from 6.04005 to 6.120001
                    bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioWeightLayerCell WHERE (ScenarioID={0})", iScenarioID))
                    bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioFleetMap WHERE (ScenarioID={0})", iScenarioID))
                Catch ex As Exception
                    Me.LogMessage(String.Format("Error {0} occurred while removing Ecospace scenarioID {1}", ex.Message, iScenarioID))
                    bSucces = False
                End Try
            End If

            Try
                ' Delete tables not necessarily linked by cascading rules
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioDataConnection WHERE (ScenarioID={0})", iScenarioID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioWeightLayer WHERE (ScenarioID={0})", iScenarioID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioGroupHabitat WHERE (ScenarioID={0})", iScenarioID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioCapacityDrivers WHERE (ScenarioID={0})", iScenarioID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioDriverLayer WHERE (ScenarioID={0})", iScenarioID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioGroupMigration WHERE (ScenarioID={0})", iScenarioID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioGroup WHERE (ScenarioID={0})", iScenarioID))
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioMonth WHERE (ScenarioID={0})", iScenarioID))
                ' Delete scenario
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenario WHERE (ScenarioID={0})", iScenarioID))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing Ecospace scenarioID {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try

            ' Reload scenario definitions
            bSucces = bSucces And Me.LoadEcospaceScenarioDefinitions()

            Return bSucces

        End Function

#End Region ' Modify

#End Region ' Scenarios

#Region " Map "

#Region " Resizing "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Resizes the basemap in an Ecospace scenario.
        ''' </summary>
        ''' <param name="InRow">New number of rows to assign to the basemap.</param>
        ''' <param name="InCol">New number of columns to assign to the basemap.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function ResizeEcospaceBasemap(ByVal InRow As Integer, ByVal InCol As Integer) As Boolean _
                 Implements IEcospaceDatasource.ResizeEcospaceBasemap

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("EcospaceScenario")
                dt = writer.GetDataTable()
                drow = dt.Rows.Find(iScenarioID)

                drow.BeginEdit()
                drow("Inrow") = InRow
                drow("Incol") = InCol
                drow.EndEdit()

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Resizing

#Region " Load "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Load the spatial data associated with an Ecospace scenario.
        ''' </summary>
        ''' <param name="iScenarioID">The scenario to load the data for.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function LoadEcospaceMap(ByVal iScenarioID As Integer) As Boolean
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True
            Dim iID As Integer = 0

            Try
                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenario WHERE (ScenarioID={0})", iScenarioID))
                While reader.Read()

                    bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "DepthMap", "")), ecospaceDS.DepthInput, ecospaceDS.InRow, ecospaceDS.InCol)
                    bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "RelPPMap", "")), ecospaceDS.RelPP, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                    bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "RelCinMap", "")), ecospaceDS.RelCin, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                    bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "FlowMap", "")), ecospaceDS.flow, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                    bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "DepthAMap", "")), ecospaceDS.DepthA, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                    bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "RegionMap", "")), ecospaceDS.Region, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                    bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "ExclusionMap", "")), ecospaceDS.Excluded, ecospaceDS.InRow, ecospaceDS.InCol)

                End While

                Me.m_db.ReleaseReader(reader)
                reader = Nothing

            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Load the spatial data associated with an Ecospace scenario.
        ''' </summary>
        ''' <param name="iScenarioID">The scenario to load the data for.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function LoadEcospaceMonthlyMaps(ByVal iScenarioID As Integer) As Boolean

            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True
            Dim iMonth As Integer = 0

            Try
                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioMonth WHERE (ScenarioID={0}) ORDER BY MonthID ASC", iScenarioID))
                While reader.Read()

                    iMonth = CInt(Me.m_db.ReadSafe(reader, "MonthID", 0))
                    If (1 <= iMonth And iMonth <= cCore.N_MONTHS) Then
                        bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "WindXVelMap", "")), iMonth, cStringUtils.eFilterIndexTypes.LastIndex, ecospaceDS.Xv, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                        bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "WindYVelMap", "")), iMonth, cStringUtils.eFilterIndexTypes.LastIndex, ecospaceDS.Yv, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                        bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "AdvectionXVelMap", "")), ecospaceDS.MonthlyXvel(iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                        bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "AdvectionYVelMap", "")), ecospaceDS.MonthlyYvel(iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                        bSucces = bSucces And cStringUtils.StringToArray(CStr(Me.m_db.ReadSafe(reader, "UpwellingMap", "")), ecospaceDS.MonthlyUpWell(iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                    End If

                End While

                Me.m_db.ReleaseReader(reader)
                reader = Nothing

            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Save the spatial data associated with an Ecospace scenario.
        ''' </summary>
        ''' <param name="idm">The scenario to save the data for.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function SaveEcospaceMap(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim iActiveScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim iScenarioID As Integer = 0
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' Get ID of scenario to save to
            iScenarioID = idm.GetID(eDataTypes.EcoSpaceScenario, iActiveScenarioID)

            Try
                writer = Me.m_db.GetWriter("EcospaceScenario")
                dt = writer.GetDataTable()
                drow = dt.Rows.Find(iScenarioID)

                drow.BeginEdit()
                drow("DepthMap") = cStringUtils.ArrayToString(ecospaceDS.DepthInput, ecospaceDS.InRow, ecospaceDS.InCol)
                drow("RelPPMap") = cStringUtils.ArrayToString(ecospaceDS.RelPP, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                drow("RelCinMap") = cStringUtils.ArrayToString(ecospaceDS.RelCin, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                drow("FlowMap") = cStringUtils.ArrayToString(ecospaceDS.flow, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                drow("DepthAMap") = cStringUtils.ArrayToString(ecospaceDS.DepthA, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                drow("RegionMap") = cStringUtils.ArrayToString(ecospaceDS.Region, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                drow("ExclusionMap") = cStringUtils.ArrayToString(ecospaceDS.Excluded, ecospaceDS.InRow, ecospaceDS.InCol)
                drow.EndEdit()

                bSucces = bSucces And Me.m_db.ReleaseWriter(writer)

                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceBasemap, iActiveScenarioID)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerDepth, iActiveScenarioID)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerRegion, iActiveScenarioID)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerRelPP, iActiveScenarioID)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerRelCin, iActiveScenarioID)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerMigration, iActiveScenarioID)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerAdvection, iActiveScenarioID)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerPort, iActiveScenarioID)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerSail, iActiveScenarioID)

            Catch ex As Exception
                ' Don't be alarmed..
                Debug.Assert(False, String.Format("Error saving basemap: '{0}'", ex.Message))
                '..be very, very afraid
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Save the spatial data associated with an Ecospace scenario.
        ''' </summary>
        ''' <param name="idm">The scenario to save the data for.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function SaveEcospaceMonthlyMaps(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iActiveScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim iScenarioID As Integer = 0
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim keys() As Object = New Object() {0, 0}
            Dim bNewRow As Boolean = True
            Dim bSucces As Boolean = True

            ' Get ID of scenario to save to
            iScenarioID = idm.GetID(eDataTypes.EcoSpaceScenario, iActiveScenarioID)

            Try
                writer = Me.m_db.GetWriter("EcospaceScenarioMonth")
                dt = writer.GetDataTable()
                For iMonth As Integer = 1 To cCore.N_MONTHS
                    keys(0) = iScenarioID
                    keys(1) = iMonth
                    drow = dt.Rows.Find(keys)
                    bNewRow = (drow Is Nothing)

                    If (bNewRow) Then
                        drow = writer.NewRow()
                        drow("ScenarioID") = iScenarioID
                        drow("MonthID") = iMonth
                    Else
                        drow.BeginEdit()
                    End If

                    drow("WindXVelMap") = cStringUtils.ArrayToString(ecospaceDS.Xv, iMonth, cStringUtils.eFilterIndexTypes.LastIndex, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                    drow("WindYVelMap") = cStringUtils.ArrayToString(ecospaceDS.Yv, iMonth, cStringUtils.eFilterIndexTypes.LastIndex, ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                    drow("AdvectionXVelMap") = cStringUtils.ArrayToString(ecospaceDS.MonthlyXvel(iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                    drow("AdvectionYVelMap") = cStringUtils.ArrayToString(ecospaceDS.MonthlyYvel(iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                    drow("UpwellingMap") = cStringUtils.ArrayToString(ecospaceDS.MonthlyUpWell(iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)

                    If (bNewRow) Then
                        writer.AddRow(drow)
                    Else
                        drow.EndEdit()
                    End If
                Next

                bSucces = bSucces And Me.m_db.ReleaseWriter(writer)
                bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerWind, iActiveScenarioID)

            Catch ex As Exception
                ' Don't be alarmed..
                Debug.Assert(False, String.Format("Error saving month maps: '{0}'", ex.Message))
                '..be very, very afraid
                bSucces = False
            End Try

            Return bSucces

        End Function

#End Region ' Save

#End Region ' Map

#Region " Habitats "

#Region " Load "

        Private Function LoadEcospaceHabitats(ByVal iScenarioID As Integer) As Boolean

            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim strMap As String = ""
            Dim bSucces As Boolean = True
            Dim i As Integer = 0

            ' Start loading
            Try
                ' Allocate space for habitat data
                ecospaceDS.NoHabitats = CInt(Me.m_db.GetValue(String.Format("SELECT COUNT(*) FROM EcospaceScenarioHabitat WHERE ScenarioID={0}", iScenarioID), 0))
                ecospaceDS.RedimHabitatVariables(False)

                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioHabitat WHERE (ScenarioID={0}) ORDER BY Sequence ASC", iScenarioID))
                While reader.Read()
                    ecospaceDS.HabitatDBID(i) = CInt(reader("HabitatID"))
                    ecospaceDS.HabitatText(i) = CStr(reader("HabitatName"))

                    strMap = CStr(Me.m_db.ReadSafe(reader, "HabitatMap", ""))
                    ' Read only water cells with values for this habitat index
                    cStringUtils.StringToArray(strMap, ecospaceDS.PHabType(i), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                    i += 1
                End While

                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Ecospace habitat for habitat {1}", ex.Message, i))
                bSucces = False
            End Try

            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        Private Function SaveEcospaceHabitats(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioIDSrc As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim iScenarioIDDest As Integer = 0
            Dim iHabID As Integer = 0
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True
            Dim bNewRow As Boolean = True
            Dim objKeys() As Object = {Nothing, Nothing}

            iScenarioIDDest = idm.GetID(eDataTypes.EcoSpaceScenario, iScenarioIDSrc)
            objKeys(0) = iScenarioIDSrc

            iHabID = CInt(Me.m_db.GetValue("SELECT MAX(HabitatID) FROM EcospaceScenarioHabitat", 0)) + 1

            Try
                writer = Me.m_db.GetWriter("EcospaceScenarioHabitat")
                dt = writer.GetDataTable()
                For iHabitat As Integer = 0 To ecospaceDS.NoHabitats - 1

                    bNewRow = (iScenarioIDDest <> iScenarioIDSrc)

                    If bNewRow Then
                        drow = writer.NewRow()
                        drow("ScenarioID") = iScenarioIDDest
                        drow("HabitatID") = iHabID
                        idm.Add(eDataTypes.EcospaceHabitat, ecospaceDS.HabitatDBID(iHabitat), iHabID)
                    Else
                        objKeys(1) = idm.GetID(eDataTypes.EcospaceHabitat, ecospaceDS.HabitatDBID(iHabitat))
                        drow = dt.Rows.Find(objKeys)
                        drow.BeginEdit()
                    End If

                    drow("HabitatName") = ecospaceDS.HabitatText(iHabitat)
                    drow("Sequence") = iHabitat
                    drow("HabitatMap") = cStringUtils.ArrayToString(ecospaceDS.PHabType(iHabitat), ecospaceDS.InRow, ecospaceDS.InCol,
                                                                    ecospaceDS.DepthInput, True)

                    If bNewRow Then
                        writer.AddRow(drow)
                        iHabID += 1
                    Else
                        drow.EndEdit()
                    End If

                    bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceHabitat, ecospaceDS.HabitatDBID(iHabitat))
                    bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceLayerHabitat, ecospaceDS.HabitatDBID(iHabitat))

                Next iHabitat
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Append an habitat to the current ecospace scenario
        ''' </summary>
        ''' <param name="strHabitatName"></param>
        ''' <param name="iHabitatID"></param>
        ''' -------------------------------------------------------------------
        Public Function AddEcospaceHabitat(ByVal strHabitatName As String,
                                           ByRef iHabitatID As Integer) As Boolean _
                Implements DataSources.IEcospaceDatasource.AddEcospaceHabitat

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)

            Return Me.AddEcospaceHabitat(strHabitatName, iScenarioID, iHabitatID)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Append an habitat to a given ecospace scenario
        ''' </summary>
        ''' <param name="strHabitatName"></param>
        ''' <param name="iHabitatID"></param>
        ''' <param name="iScenarioID">Ecospace scenario ID to add the habitat to.</param>
        ''' -------------------------------------------------------------------
        Private Function AddEcospaceHabitat(ByVal strHabitatName As String,
                                            ByVal iScenarioID As Integer,
                                            ByRef iHabitatID As Integer) As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True
            Dim iPosition As Integer = 1

            iHabitatID = CInt(Me.m_db.GetValue("SELECT MAX(HabitatID) FROM EcospaceScenarioHabitat", 0)) + 1
            iPosition = CInt(Me.m_db.GetValue("SELECT Count(*) FROM EcospaceScenarioHabitat", 0)) + 1

            ' The writer needed here will maintain row sequence for the given scenario only
            writer = Me.m_db.GetWriter("EcospaceScenarioHabitat")

            drow = writer.NewRow()
            drow("ScenarioID") = iScenarioID
            drow("HabitatID") = iHabitatID
            drow("HabitatName") = strHabitatName
            drow("Sequence") = iPosition
            drow("HabitatMap") = ""
            writer.AddRow(drow)

            Me.m_db.ReleaseWriter(writer)

            Return bSucces
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove an ecospace habitat from the current scenario
        ''' </summary>
        ''' <param name="iHabitatID"></param>
        ''' -------------------------------------------------------------------
        Public Function RemoveHabitat(ByVal iHabitatID As Integer) As Boolean _
                Implements DataSources.IEcospaceDatasource.RemoveEcospaceHabitat

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim bSucces As Boolean = True

            Try
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioHabitat WHERE (ScenarioID={0}) AND (HabitatID={1})", iScenarioID, iHabitatID))
                '' This could have far-fetched consequences throughout the scenario; the entire scenario should be reloaded.
                'bSucces = Me.LoadEcospaceScenario(iScenarioID)
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing Ecospace habitatID {1}", ex.Message, iHabitatID))
                bSucces = False
            End Try
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Move an Ecospace habitat to a different position in the habitat sequence.
        ''' </summary>
        ''' <param name="iHabitatID">Database ID of the habitat to move.</param>
        ''' <param name="iPosition">The new position of the habitat in the habitat sequence.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function MoveEcospaceHabitat(iHabitatID As Integer, iPosition As Integer) As Boolean _
            Implements IEcospaceDatasource.MoveHabitat

            Dim bSucces As Boolean = True
            Try
                Me.m_db.Execute(String.Format("UPDATE EcospaceScenarioHabitat SET Sequence={1} WHERE (HabitatID={0})", iHabitatID, iPosition))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while moving fleet {1}", ex.Message, iHabitatID))
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Modify

#End Region ' Habitats

#Region " Groups "

#Region " Load "

        Private Function LoadEcospaceGroups(ByVal iScenarioID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True
            Dim astrSplit As String() = Nothing
            Dim strMap As String = ""
            Dim iGroup As Integer = 0

            ' Group redimensioning is handled from the LoadScenario call
            'ecospaceDS.RedimGroupVariables(False)

            ' Clear
            For iGroup = 1 To Me.m_core.nGroups
                For iRow As Integer = 0 To ecospaceDS.InRow
                    For iCol As Integer = 0 To ecospaceDS.InCol
                        ecospaceDS.HabCapInput(iGroup)(iRow, iCol) = 1.0
                    Next iCol
                Next iRow
            Next iGroup

            ' Read the data
            Try
                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioGroup WHERE (ScenarioID={0})", iScenarioID))
                While reader.Read()

                    ' Resolve group index
                    iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("EcopathGroupID")))
                    ' Sanity check
                    Debug.Assert(iGroup > -1)
                    ' Load the data
                    ecospaceDS.GroupDBID(iGroup) = CInt(reader("GroupID"))
                    ecospaceDS.EcopathGroupDBID(iGroup) = CInt(reader("EcopathGroupID"))
                    ecospaceDS.Mvel(iGroup) = CSng(reader("Mvel"))
                    ecospaceDS.RelMoveBad(iGroup) = CSng(reader("RelMoveBad"))
                    ecospaceDS.RelVulBad(iGroup) = CSng(reader("RelVulBad"))
                    ecospaceDS.EatEffBad(iGroup) = 1 'CSng(reader("EatEffBad"))
                    ' VERIFY_JS: RiskSens imported but not used in EwE5
                    ' ecospaceDS.RiskSens(i) = CSng(reader("RiskSens"))
                    ecospaceDS.IsAdvected(iGroup) = (CInt(reader("IsAdvected")) <> 0)
                    ecospaceDS.IsMigratory(iGroup) = (CInt(reader("IsMigratory")) <> 0)

                    ecospaceDS.barrierAvoidanceWeight(iGroup) = CSng(Me.m_db.ReadSafe(reader, "BarrierAvoidanceWeight", ecospaceDS.barrierAvoidanceWeight(iGroup)))
                    ecospaceDS.CapCalType(iGroup) = DirectCast(CInt(Me.m_db.ReadSafe(reader, "CapacityCalType", eEcospaceCapacityCalType.Habitat)), eEcospaceCapacityCalType)

                    strMap = CStr(Me.m_db.ReadSafe(reader, "CapacityMap", ""))
                    cStringUtils.StringToArray(strMap, ecospaceDS.HabCapInput(iGroup), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)

                End While
                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Ecospace group {1}", ex.Message, iGroup))
                bSucces = False
            End Try

            ' Load habitat preferences
            bSucces = bSucces And Me.LoadEcospaceGroupHabitats(iScenarioID)
            ' Load migration maps
            bSucces = bSucces And Me.LoadEcospaceGroupMigration(iScenarioID)
            Return bSucces

        End Function

        Private Function LoadEcospaceGroupHabitats(ByVal iScenarioID As Integer) As Boolean

            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim iGroupID As Integer = 0
            Dim iGroup As Integer = -1
            Dim iHabitatID As Integer = 0
            Dim iHabitat As Integer = -1
            Dim sPreference As Single = 0.0!
            Dim bSucces As Boolean = True

            Try
                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioGroupHabitat WHERE (ScenarioID={0})", iScenarioID))
                While reader.Read()

                    ' Get group index
                    iGroupID = CInt(reader("GroupID"))
                    iGroup = Array.IndexOf(ecospaceDS.GroupDBID, iGroupID)
                    ' Get habitat index
                    iHabitatID = CInt(reader("HabitatID"))
                    iHabitat = Array.IndexOf(ecospaceDS.HabitatDBID, iHabitatID)
                    sPreference = CSng(Me.m_db.ReadSafe(reader, "Preference", 1.0))
                    ' Sanity check
                    If (iGroup = -1) Or (iHabitat = -1) Then
                        If (iGroup = -1) Then Me.LogMessage(String.Format("LoadEcospaceGroupHabitats: Group ID {0} no longer exist", iGroupID))
                        If (iHabitat = -1) Then Me.LogMessage(String.Format("LoadEcospaceGroupHabitats: Habitat ID {0} no longer exist", iHabitatID))
                    Else
                        ' Flag as preferred
                        ecospaceDS.PrefHab(iGroup, 0) = 0
                        ecospaceDS.PrefHab(iGroup, iHabitat) = sPreference
                    End If

                End While
                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Ecospace group preferred habitats", ex.Message))
                bSucces = False
            End Try

            Return bSucces
        End Function

        Private Function LoadEcospaceGroupMigration(ByVal iScenarioID As Integer) As Boolean

            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim iGroupID As Integer = 0
            Dim iGroup As Integer = -1
            Dim iMonth As Integer = 0
            Dim bSucces As Boolean = True

            ecospaceDS.RedimMigrationMaps(bClearExisting:=True)

            Try
                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioGroupMigration WHERE (ScenarioID={0})", iScenarioID))
                While reader.Read()

                    ' Get group index
                    iGroupID = CInt(reader("GroupID"))
                    iGroup = Array.IndexOf(ecospaceDS.GroupDBID, iGroupID)
                    ' Get habitat index
                    iMonth = CInt(reader("MonthID"))
                    ' Sanity check
                    If (iGroup = -1) Then
                        Me.LogMessage(String.Format("LoadEcospaceGroupHabitats: Group ID {0} no longer exist", iGroupID))
                    Else
                        Debug.Assert(ecospaceDS.IsMigratory(iGroup))

                        ' Read only water cells 
                        Dim strMap As String = CStr(Me.m_db.ReadSafe(reader, "Map", ""))
                        cStringUtils.StringToArray(strMap, ecospaceDS.MigMaps(iGroup, iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                    End If

                End While
                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Ecospace group preferred habitats", ex.Message))
                bSucces = False
            End Try

            Return bSucces
        End Function


#End Region ' Load

#Region " Save "

        Private Function SaveEcospaceGroups(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim sbTemp As New StringBuilder
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim iNextGroupID As Integer = 0
            Dim iGroupID As Integer = 0
            Dim bSucces As Boolean = True
            Dim objKeys() As Object = {iScenarioID, 0}

            ' Get mapped scenario ID, in case saving to a different scenario
            iScenarioID = idm.GetID(eDataTypes.EcoSpaceScenario, iScenarioID)
            objKeys(0) = iScenarioID

            ' Get next available group ID
            iNextGroupID = CInt(Me.m_db.GetValue("SELECT MAX(GroupID) FROM EcospaceScenarioGroup", 0)) + 1

            Try
                writer = Me.m_db.GetWriter("EcospaceScenarioGroup")
                dt = writer.GetDataTable()
                For iGroup As Integer = 1 To ecopathDS.NumGroups

                    objKeys(1) = ecopathDS.GroupDBID(iGroup)
                    drow = dt.Rows.Find(objKeys)

                    bNewRow = (drow Is Nothing)
                    If bNewRow Then
                        drow = writer.NewRow()
                        drow("ScenarioID") = iScenarioID
                        drow("EcopathGroupID") = ecopathDS.GroupDBID(iGroup)
                        drow("GroupID") = iNextGroupID
                        iNextGroupID += 1
                    Else
                        drow.BeginEdit()
                    End If

                    iGroupID = CInt(drow("GroupID"))

                    ' Store ecospace group ID mapping now we know it
                    ' JS 04Jul11: group mapping is stored by ECOPATH group ID since this is the only constant
                    '             factor while appending Ecospace scenarios. This follows the structure that
                    '             Ecosim adapted 2 years ago
                    idm.Add(eDataTypes.EcospaceGroup, ecopathDS.GroupDBID(iGroup), iGroupID)

                    drow("Mvel") = ecospaceDS.Mvel(iGroup)
                    drow("RelMoveBad") = ecospaceDS.RelMoveBad(iGroup)
                    drow("RelVulBad") = ecospaceDS.RelVulBad(iGroup)
                    'drow("EatEffBad") = ecospaceDS.EatEffBad(iGroup)
                    drow("IsAdvected") = ecospaceDS.IsAdvected(iGroup)
                    drow("IsMigratory") = ecospaceDS.IsMigratory(iGroup)

                    drow("BarrierAvoidanceWeight") = ecospaceDS.barrierAvoidanceWeight(iGroup)
                    drow("CapacityCalType") = ecospaceDS.CapCalType(iGroup)

                    drow("CapacityMap") = cStringUtils.ArrayToString(ecospaceDS.HabCapInput(iGroup), ecospaceDS.InRow, ecospaceDS.InCol,
                                                                     ecospaceDS.DepthInput, True)

                    If bNewRow Then
                        writer.AddRow(drow)
                    Else
                        drow.EndEdit()
                    End If

                    bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceGroup, ecospaceDS.GroupDBID(iGroup))
                    bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceGroupOuput, ecospaceDS.GroupDBID(iGroup))

                Next iGroup

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving EcospaceGroup", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, True)
            bSucces = bSucces And Me.SaveEcospaceGroupHabitats(idm)
            bSucces = bSucces And Me.SaveEcospaceGroupMigration(idm)

            Return bSucces

        End Function

        Private Function SaveEcospaceGroupHabitats(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = idm.GetID(eDataTypes.EcoSpaceScenario, ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario))
            Dim iGroupID As Integer = 0
            Dim iGroup As Integer = 0
            Dim iHabitatID As Integer = 0
            Dim iHabitat As Integer = 0

            Dim bSucces As Boolean = True

            Try
                ' No incremental save for now
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioGroupHabitat WHERE ScenarioID={0}", iScenarioID))

                writer = Me.m_db.GetWriter("EcospaceScenarioGroupHabitat")
                For iGroup = 1 To ecopathDS.NumGroups
                    iGroupID = idm.GetID(eDataTypes.EcospaceGroup, ecopathDS.GroupDBID(iGroup))

                    For iHabitat = 0 To ecospaceDS.NoHabitats
                        iHabitatID = idm.GetID(eDataTypes.EcospaceHabitat, ecospaceDS.HabitatDBID(iHabitat))

                        If (ecospaceDS.PrefHab(iGroup, iHabitat) > 0) Then

                            drow = writer.NewRow()
                            drow("ScenarioID") = iScenarioID
                            drow("GroupID") = iGroupID
                            drow("HabitatID") = iHabitatID
                            drow("Preference") = ecospaceDS.PrefHab(iGroup, iHabitat)
                            writer.AddRow(drow)

                        End If

                    Next iHabitat
                Next iGroup

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        Private Function SaveEcospaceGroupMigration(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = idm.GetID(eDataTypes.EcoSpaceScenario, ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario))
            Dim iGroupID As Integer = 0
            Dim iGroup As Integer = 0
            Dim iHabitat As Integer = 0

            Dim bSucces As Boolean = True

            ' No incremental save for now
            Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioGroupMigration WHERE ScenarioID={0}", iScenarioID))

            writer = Me.m_db.GetWriter("EcospaceScenarioGroupMigration")
            Try
                For iGroup = 1 To ecopathDS.NumGroups
                    ' We can decide to save all maps that have once been defined...
                    If ecospaceDS.IsMigratory(iGroup) Then
                        iGroupID = idm.GetID(eDataTypes.EcospaceGroup, ecopathDS.GroupDBID(iGroup))
                        For iMonth As Integer = 1 To 12
                            drow = writer.NewRow()
                            drow("ScenarioID") = iScenarioID
                            drow("GroupID") = iGroupID
                            drow("MonthID") = iMonth
                            drow("Map") = cStringUtils.ArrayToString(ecospaceDS.MigMaps(iGroup, iMonth), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)
                            writer.AddRow(drow)
                        Next iMonth
                    End If
                Next iGroup

            Catch ex As Exception
                bSucces = False
            End Try

            Me.m_db.ReleaseWriter(writer, bSucces)
            Return bSucces

        End Function

#End Region ' Save

#Region " Modify "

        ''' <summary>
        ''' Create a group for each ecospace scenario
        ''' </summary>
        ''' <param name="iEcopathGroupID">Ecopath Group DBID</param>
        Private Function AddEcospaceGroupToAllScenarios(ByVal iEcopathGroupID As Integer, ByVal bIsDetritus As Boolean) As Boolean

            Dim reader As IDataReader = Nothing
            Dim iID As Integer = 0
            Dim bSucces As Boolean = True

            Try
                reader = Me.m_db.GetReader("SELECT ScenarioID FROM EcoSpaceScenario")
                While reader.Read()
                    bSucces = bSucces And AddEcospaceGroup(iEcopathGroupID, CInt(reader("ScenarioID")), bIsDetritus, iID)
                End While
                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' <summary>
        ''' Add a group to a given Ecospace scenario.
        ''' </summary>
        ''' <param name="iEcopathGroupID"><see cref="cEcoPathGroupInput.DBID">Ecopath ID</see> of this group</param>
        ''' <param name="iScenarioID"><see cref="cEcospaceScenario.DBID">Ecospace scenario ID</see> of the scenario to add the group to.</param>
        ''' <returns>True if successful.</returns>
        Private Function AddEcospaceGroup(ByVal iEcopathGroupID As Integer, ByVal iScenarioID As Integer,
                                          ByVal bIsDetritus As Boolean, ByRef iGroupID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim iGroup As Integer = 0
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            iGroupID = CInt(Me.m_db.GetValue("SELECT MAX(GroupID) FROM EcospaceScenarioGroup", 0)) + 1

            Try
                ' Is this a detritus group?
                iGroup = Array.IndexOf(ecopathDS.GroupDBID, iEcopathGroupID)

                ' Add group
                writer = Me.m_db.GetWriter("EcospaceScenarioGroup")

                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("EcopathGroupID") = iEcopathGroupID
                drow("GroupID") = iGroupID
                ' Detritus default of 10, non-detritus 300
                drow("MVel") = If(bIsDetritus, 10.0!, 300.0!)
                drow("CapacityMap") = ""
                writer.AddRow(drow)

                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <para>
        ''' Due to the limited capabilities of Microzork Access SQL, database 
        ''' update-generated foreign keys to fleets and groups cannot cacading 
        ''' delete. Hence, we need to eradicate linked groups and fleets via code.
        ''' </para> 
        ''' </summary>
        ''' <param name="iEcospaceGroupID">DBID of the Ecospace group to remove.</param>
        ''' <param name="iEcopathGroupID">DBID of the Ecopath group.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Private Function RemoveEcospaceGroup(ByVal iEcospaceGroupID As Integer, ByVal iEcopathGroupID As Integer) As Boolean
            Dim bSucces As Boolean = True
            Try
                ' Cannot have orphaned taxa
                Dim reader As IDataReader = Me.m_db.GetReader(String.Format("SELECT TaxonID FROM EcopathGroupTaxon WHERE (GroupID={0})", iEcopathGroupID))
                Dim ids As New List(Of Integer)
                While reader.Read
                    ids.Add(CInt(reader("TaxonID")))
                End While
                Me.m_db.ReleaseReader(reader)
                For Each id As Integer In ids
                    bSucces = bSucces And Me.RemoveTaxon(id)
                Next

                ' Delete habitat assignments
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioGroupHabitat WHERE GroupID={0}", iEcospaceGroupID))

                ' Do not worry about explicitly deleting layer data connections; there are no
                ' referential integrity links between maps and their connections. The loading logic
                ' implemented in this class will deal with missing map links.

                ' Delete capacity drivers
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioCapacityDrivers WHERE GroupID={0}", iEcospaceGroupID))
                ' Delete migration maps
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioGroupMigration WHERE GroupID={0}", iEcospaceGroupID))
                ' Finally delete group
                bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioGroup WHERE GroupID={0}", iEcospaceGroupID))

            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces
        End Function

#End Region ' Modify

#End Region ' Groups

#Region " Fleets "

#Region " Load "

        Private Function LoadEcospaceFleets(ByVal iScenarioID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.EcopathDataStructures
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim strMap As String = ""
            Dim bSucces As Boolean = True
            Dim iFleet As Integer = 0

            ecospaceDS.ReDimFleets()
            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioFleet WHERE (ScenarioID={0})", iScenarioID))

            Try
                While reader.Read()

                    iFleet = Array.IndexOf(ecopathDS.FleetDBID, CInt(reader("EcopathFleetID")))
                    Debug.Assert(iFleet >= 1)
                    ecospaceDS.FleetDBID(iFleet) = CInt(reader("FleetID"))
                    ecospaceDS.EcopathFleetDBID(iFleet) = CInt(reader("EcopathFleetID"))
                    ecospaceDS.EffPower(iFleet) = CSng(reader("EffPower"))
                    ecospaceDS.SEmult(iFleet) = CSng(Me.m_db.ReadSafe(reader, "SEMult", 1.0))

                    ' Read port map for a given fleet and land cells only
                    strMap = CStr(Me.m_db.ReadSafe(reader, "PortMap", ""))
                    bSucces = bSucces And cStringUtils.StringToArray(strMap, ecospaceDS.Port(iFleet), ecospaceDS.InRow, ecospaceDS.InCol)

                    ' Read sailing cost map for a given fleet and water cells only
                    strMap = CStr(Me.m_db.ReadSafe(reader, "SailCostMap", ""))
                    bSucces = bSucces And cStringUtils.StringToArray(strMap, ecospaceDS.Sail(iFleet), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)

                End While

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Ecospace fleet {1}", ex.Message, iFleet))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)

            ' Read habitat fishery
            bSucces = bSucces And Me.LoadEcospaceHabitatFishery(iScenarioID)
            ' Read MPA fishery
            bSucces = bSucces And Me.LoadEcospaceMPAFishery(iScenarioID)
            ' There
            Return bSucces

        End Function

        Private Function LoadEcospaceHabitatFishery(ByVal iScenarioID As Integer) As Boolean
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim iFleet As Integer = 0
            Dim iHabitat As Integer = 0
            Dim bSucces As Boolean = True

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioHabitatFishery WHERE (ScenarioID={0})", iScenarioID))
            Try
                While reader.Read()
                    iFleet = Array.IndexOf(ecospaceDS.FleetDBID, CInt(reader("FleetID")))
                    iHabitat = Array.IndexOf(ecospaceDS.HabitatDBID, CInt(reader("HabitatID")))
                    'jb habitats and fleets both use the zero index
                    If (iFleet >= 0 And iHabitat >= 0) Then
                        ' Clear default 'all' habitat assignment
                        ecospaceDS.GearHab(iFleet, 0) = False
                        ecospaceDS.GearHab(iFleet, iHabitat) = True
                    End If
                End While
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading EcospaceScenarioHabitatFishery for iFleet {1}, iHabitat {2}", ex.Message, iFleet, iHabitat))
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)

            Return bSucces
        End Function

        Private Function LoadEcospaceMPAFishery(ByVal iScenarioID As Integer) As Boolean
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim iFleet As Integer = 0
            Dim iMPA As Integer = 0
            Dim bSucces As Boolean = True

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioMPAFishery WHERE (ScenarioID={0})", iScenarioID))
            Try
                While reader.Read()
                    iFleet = Array.IndexOf(ecospaceDS.FleetDBID, CInt(reader("FleetID")))
                    iMPA = Array.IndexOf(ecospaceDS.MPADBID, CInt(reader("MPAID")))
                    ' Crash prevention, should not be necessary but hey
                    If (iFleet >= 0 And iMPA > 0) Then
                        ' Clear default 'all' habitat assignment
                        ecospaceDS.MPAfishery(iFleet, 0) = False
                        ecospaceDS.MPAfishery(iFleet, iMPA) = True
                    End If
                End While
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading ReadEcospaceMPAFishery for iFleet {1}, iMPA {2}", ex.Message, iFleet, iMPA))
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)

            Return bSucces
        End Function

#End Region ' Load

#Region " Save "

        Private Function SaveEcospaceFleets(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim iScenarioID As Integer = 0
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim iFleet As Integer = 0
            Dim iNextFleetID As Integer = 0
            Dim iFleetID As Integer = 0
            Dim objKeys() As Object = {Nothing, Nothing} ' Composite key to find group per scenario
            Dim bSucces As Boolean = True

            ' Obtain mapped scenario ID
            iScenarioID = idm.GetID(eDataTypes.EcoSpaceScenario, ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario))
            objKeys(0) = iScenarioID

            ' Get next available fleet ID
            iNextFleetID = CInt(Me.m_db.GetValue("SELECT MAX(FleetID) FROM EcospaceScenarioFleet", 0)) + 1

            Try
                writer = Me.m_db.GetWriter("EcospaceScenarioFleet")
                dt = writer.GetDataTable()

                For iFleet = 1 To ecospaceDS.nFleets

                    objKeys(1) = ecopathDS.FleetDBID(iFleet)

                    ' Find existing row
                    drow = dt.Rows.Find(objKeys)
                    ' Check wheter a new row or an existing row
                    bNewRow = drow Is Nothing
                    ' New row?
                    If bNewRow Then
                        ' #Yes: create new row
                        drow = writer.NewRow()
                        ' Populate PK
                        drow("ScenarioID") = objKeys(0)
                        drow("EcopathFleetID") = objKeys(1)
                        drow("FleetID") = iNextFleetID
                        iNextFleetID += 1
                    Else
                        ' #No: edit the row
                        drow.BeginEdit()
                    End If

                    iFleetID = CInt(drow("FleetID"))
                    idm.Add(eDataTypes.EcospaceFleet, ecopathDS.FleetDBID(iFleet), iFleetID)

                    ' Update fleet vars
                    drow("EffPower") = ecospaceDS.EffPower(iFleet)
                    drow("SEMult") = ecospaceDS.SEmult(iFleet)
                    drow("PortMap") = cStringUtils.ArrayToString(ecospaceDS.Port(iFleet), ecospaceDS.InRow, ecospaceDS.InCol)
                    drow("SailCostMap") = cStringUtils.ArrayToString(ecospaceDS.Sail(iFleet), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)

                    ' Wrap up: was this a new row?
                    If bNewRow Then
                        ' #Yes: add it to the writer
                        writer.AddRow(drow)
                    Else
                        ' #No: done editing
                        drow.EndEdit()
                    End If

                Next iFleet

                ' Save changes
                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving Ecospace Fleet", ex.Message))
                bSucces = False
            End Try

            ' Save habitat fishery
            bSucces = bSucces And Me.SaveEcospaceHabitatFishery(idm)
            ' Save MPA fishery
            bSucces = bSucces And Me.SaveEcospaceMPAFishery(idm)
            ' There
            Return bSucces

        End Function

        Private Function SaveEcospaceHabitatFishery(ByVal idm As cIDMappings) As Boolean
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim iFleet As Integer = 0
            Dim iHabitat As Integer = 0
            Dim bSucces As Boolean = True

            iScenarioID = idm.GetID(eDataTypes.EcoSpaceScenario, iScenarioID)

            Try
                ' Erase
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioHabitatFishery WHERE ScenarioID={0}", iScenarioID))
                writer = Me.m_db.GetWriter("EcospaceScenarioHabitatFishery")

                For iFleet = 1 To ecospaceDS.nFleets
                    For iHabitat = 0 To ecospaceDS.NoHabitats

                        If (ecospaceDS.GearHab(iFleet, iHabitat) = True) Then

                            drow = writer.NewRow()
                            drow("ScenarioID") = iScenarioID
                            drow("FleetID") = idm.GetID(eDataTypes.EcospaceFleet, ecopathDS.FleetDBID(iFleet))
                            drow("HabitatID") = idm.GetID(eDataTypes.EcospaceHabitat, ecospaceDS.HabitatDBID(iHabitat))
                            writer.AddRow(drow)

                        End If
                    Next iHabitat
                Next iFleet

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving EcospaceScenarioHabitatFishery", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Return bSucces
        End Function

        Private Function SaveEcospaceMPAFishery(ByVal idm As cIDMappings) As Boolean
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim iFleet As Integer = 0
            Dim iMPA As Integer = 0
            Dim bSucces As Boolean = True

            iScenarioID = idm.GetID(eDataTypes.EcoSpaceScenario, iScenarioID)

            Try
                ' Erase
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioMPAFishery WHERE ScenarioID={0}", iScenarioID))
                writer = Me.m_db.GetWriter("EcospaceScenarioMPAFishery")

                For iFleet = 1 To ecospaceDS.nFleets
                    For iMPA = 1 To ecospaceDS.MPAno

                        If (ecospaceDS.MPAfishery(iFleet, iMPA) = True) Then

                            drow = writer.NewRow()
                            drow("ScenarioID") = iScenarioID
                            drow("FleetID") = idm.GetID(eDataTypes.EcospaceFleet, ecospaceDS.FleetDBID(iFleet))
                            drow("MPAID") = idm.GetID(eDataTypes.EcospaceMPA, ecospaceDS.MPADBID(iMPA))
                            writer.AddRow(drow)

                        End If
                    Next iMPA
                Next iFleet

                Me.m_db.ReleaseWriter(writer, True)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving EcospaceScenarioMPAFishery", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Return bSucces
        End Function

#End Region ' Save

#Region " Modify "

        ''' <summary>
        ''' Create a fleet for each ecospace scenario
        ''' </summary>
        ''' <param name="iEcopathFleetID">Ecopath Fleet DBID</param>
        Private Function AddEcospaceFleetToAllScenarios(ByVal iEcopathFleetID As Integer) As Boolean

            Dim reader As IDataReader = Nothing
            Dim iID As Integer = 0
            Dim bSucces As Boolean = True

            Try

                reader = Me.m_db.GetReader("SELECT ScenarioID FROM EcospaceScenario")
                While reader.Read()
                    bSucces = bSucces And AddEcospaceFleet(iEcopathFleetID, CInt(reader("ScenarioID")), iID)
                End While
                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' <summary>
        ''' Add an ecospace fleet to a given ecospace scenario.
        ''' </summary>
        ''' <param name="iEcopathFleetID">Ecopath Fleet DBID.</param>
        ''' <param name="iScenarioID">Scenario ID to add the fleet to.</param>
        Private Function AddEcospaceFleet(ByVal iEcopathFleetID As Integer, ByVal iScenarioID As Integer, ByRef iFleetID As Integer) As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            iFleetID = CInt(Me.m_db.GetValue("SELECT MAX(FleetID) FROM EcospaceScenarioFleet", 0)) + 1

            Try
                ' Add fleet
                writer = Me.m_db.GetWriter("EcospaceScenarioFleet")
                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("FleetID") = iFleetID
                drow("EcopathFleetID") = iEcopathFleetID
                drow("EffPower") = 1
                drow("SailCostMap") = ""
                drow("PortMap") = ""

                writer.AddRow(drow)
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' <summary>
        ''' Remove an ecospace fleet.
        ''' </summary>
        ''' <param name="iEcopathFleetID">Ecopath Fleet DBID.</param>
        Private Function RemoveEcospaceFleet(ByVal iEcopathFleetID As Integer) As Boolean

            Dim bSucces As Boolean = True
            Dim reader As IDataReader = Nothing
            Dim iFleetID As Integer = 0

            reader = Me.m_db.GetReader(String.Format("SELECT FleetID FROM EcospaceScenarioFleet WHERE (EcopathFleetID={0})", iEcopathFleetID))
            Try
                While reader.Read()
                    iFleetID = CInt(reader("FleetID"))
                    bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioHabitatFishery WHERE (FleetID={0})", iFleetID))
                    bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioMPAFishery WHERE (FleetID={0})", iFleetID))
                End While
            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)
            bSucces = bSucces And Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioFleet WHERE (EcopathFleetID={0})", iEcopathFleetID))

            Return bSucces

        End Function

#End Region ' Modify

#End Region ' Fleets

#Region " MPAs "

#Region " Load "

        Private Function LoadEcospaceMPAs(ByVal iScenarioID As Integer) As Boolean

            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim strMPAMonth As String = ""
            Dim strMPAMap As String = ""
            Dim bSucces As Boolean = True
            Dim iMPA As Integer = 0

            ' Allocate space for MPA data
            ecospaceDS.MPAno = CInt(Me.m_db.GetValue(String.Format("SELECT COUNT(*) FROM EcospaceScenarioMPA WHERE ScenarioID={0}", iScenarioID)))
            ecospaceDS.RedimMPAVariables()

            ' Load the data
            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioMPA WHERE (ScenarioID={0}) ORDER BY Sequence ASC", iScenarioID))

            Try
                While reader.Read()

                    ' Read fields
                    iMPA += 1
                    ' Get the data
                    ecospaceDS.MPADBID(iMPA) = CInt(reader("MPAID"))
                    ecospaceDS.MPAname(iMPA) = CStr(reader("MPAName"))

                    ' Read month '0' or '1' pattern (yeah yeah, could have been done with 12-bit bitflags LONG value)
                    strMPAMonth = CStr(reader("MPAMonth"))
                    For iMonth As Integer = 0 To Math.Min(cCore.N_MONTHS, strMPAMonth.Length) - 1
                        ' MPAmonth is an array of boolean flags depicting wheter an MPA is open for fishing,
                        ' where closed months are stored as 0, and open months are stored as 1
                        ' EcospaceDS.MPAmonth: False if closed, True if open
                        ecospaceDS.MPAmonth(iMonth + 1, iMPA) = (strMPAMonth.Substring(iMonth, 1) = "1")
                    Next iMonth

                    ' Read map
                    strMPAMap = CStr(Me.m_db.ReadSafe(reader, "MPAMap", ""))
                    bSucces = bSucces And cStringUtils.StringToArray(strMPAMap, ecospaceDS.MPA(iMPA), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True)

                End While

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading EcospaceScenarioMPA {1}", ex.Message, iMPA))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)

            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        Private Function SaveEcospaceMPAs(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim iScenarioIDSrc As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim iScenarioIDDest As Integer = 0
            Dim iID As Integer = 0
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = True
            Dim sbMPAMonth As New StringBuilder
            Dim objKeys() As Object = {Nothing, Nothing} ' Composite key to find MPA per scenario
            Dim bSucces As Boolean = True

            iID = CInt(Me.m_db.GetValue("SELECT MAX(MPAID) FROM EcospaceScenarioMPA", 0)) + 1
            iScenarioIDDest = idm.GetID(eDataTypes.EcoSpaceScenario, iScenarioIDSrc)
            objKeys(0) = iScenarioIDDest

            Try
                writer = Me.m_db.GetWriter("EcospaceScenarioMPA")
                dt = writer.GetDataTable()

                For iMPA As Integer = 1 To ecospaceDS.MPAno

                    ' Try to find row
                    objKeys(1) = ecospaceDS.MPADBID(iMPA)
                    drow = dt.Rows.Find(objKeys)

                    bNewRow = (iScenarioIDSrc <> iScenarioIDDest) Or (drow Is Nothing)

                    If bNewRow Then
                        drow = writer.NewRow()
                        drow("ScenarioID") = iScenarioIDDest
                        drow("MPAID") = iID
                        idm.Add(eDataTypes.EcospaceMPA, ecospaceDS.MPADBID(iMPA), iID)
                    Else
                        drow.BeginEdit()
                    End If

                    drow("MPAName") = ecospaceDS.MPAname(iMPA)
                    drow("MPAMap") = cStringUtils.ArrayToString(ecospaceDS.MPA(iMPA), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput, True, True)

                    ' Create MPA month bit pattern
                    sbMPAMonth.Length = 0
                    For iMonth As Integer = 1 To cCore.N_MONTHS
                        ' Closed for fishing: store as 0, open: store as 1
                        sbMPAMonth.Append(If(ecospaceDS.MPAmonth(iMonth, iMPA), "1", "0"))
                    Next iMonth
                    drow("MPAMonth") = sbMPAMonth.ToString()

                    If bNewRow Then
                        writer.AddRow(drow)
                        iID += 1
                    Else
                        drow.EndEdit()
                    End If

                    bSucces = bSucces And Me.DuplicateAuxillaryData(idm, eDataTypes.EcospaceMPA, ecospaceDS.MPADBID(iMPA))

                Next iMPA

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving Ecospace MPA", ex.Message))
                bSucces = False
            Finally
                Me.m_db.ReleaseWriter(writer)
                writer = Nothing
            End Try

            Return bSucces
        End Function

#End Region ' Save

#Region " Modify "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Add a MPA to the active Ecospace scenario
        ''' </summary>
        ''' <param name="strMPAName"></param>
        ''' <param name="iMPAID"></param>
        ''' <param name="bMPAMonths">One-based series of flags that indicate when the 
        ''' MPA is OPEN for fishing.</param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Function AppendEcospaceMPA(ByVal strMPAName As String,
                                          ByVal bMPAMonths() As Boolean,
                                          ByRef iMPAID As Integer) As Boolean _
                Implements DataSources.IEcospaceDatasource.AppendEcospaceMPA

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)

            Return Me.AddEcospaceMPA(strMPAName, iScenarioID, bMPAMonths, iMPAID)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Add a MPA to a given scenario.
        ''' </summary>
        ''' <param name="strMPAName"></param>
        ''' <param name="iScenarioID"></param>
        ''' <param name="bMPAMonths">One-based series of flags that indicate when the 
        ''' MPA is OPEN for fishing.</param>
        ''' <param name="iMPAID"></param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Private Function AddEcospaceMPA(ByVal strMPAName As String,
                                        ByVal iScenarioID As Integer,
                                        ByVal bMPAMonths() As Boolean,
                                        ByRef iMPAID As Integer) As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True
            Dim sbMPAMonth As New StringBuilder

            iMPAID = CInt(Me.m_db.GetValue("SELECT MAX(MPAID) FROM EcospaceScenarioMPA", 0)) + 1
            writer = Me.m_db.GetWriter("EcospaceScenarioMPA")

            drow = writer.NewRow()
            drow("ScenarioID") = iScenarioID
            drow("MPAID") = iMPAID
            drow("MPAName") = strMPAName
            drow("MPAMap") = ""
            drow("Sequence") = iMPAID

            sbMPAMonth.Length = 0
            For iMonth As Integer = 1 To Math.Min(cCore.N_MONTHS, bMPAMonths.Length - 1)
                ' Closed for fishing: store as 0, open: store as 1
                sbMPAMonth.Append(If(bMPAMonths(iMonth), "1", "0"))
            Next iMonth
            drow("MPAMonth") = sbMPAMonth.ToString()

            writer.AddRow(drow)

            Me.m_db.ReleaseWriter(writer)

            Return bSucces
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Remove a MPA from the active Ecospace scenario.
        ''' </summary>
        ''' <param name="iMPAID">Database ID of the MPA to remove.</param>
        ''' <returns>True if you have been good last year.</returns>
        ''' -----------------------------------------------------------------------
        Public Function RemoveEcospaceMPA(ByVal iMPAID As Integer) As Boolean _
                 Implements DataSources.IEcospaceDatasource.RemoveEcospaceMPA

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim bSucces As Boolean = True

            Try
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioMPA WHERE (ScenarioID={0}) AND (MPAID={1})", iScenarioID, iMPAID))
                '' This could have far-fetched consequences throughout the scenario; the entire scenario should be reloaded.
                'bSucces = Me.LoadEcospaceScenario(iScenarioID)
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing Ecospace MPAID {1}", ex.Message, iMPAID))
                bSucces = False
            End Try
            Return bSucces
        End Function

#End Region ' Modify

#End Region ' MPAs

#Region " Weight layers "

#Region " Load "

        Private Function LoadEcospaceWeightLayers(ByVal iScenarioID As Integer) As Boolean

            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim readerLayer As IDataReader = Nothing
            Dim bSucces As Boolean = True
            Dim iRow As Integer = 0
            Dim iCol As Integer = 0
            Dim iLayer As Integer = 0

            Try
                readerLayer = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioWeightLayer WHERE (ScenarioID={0})", iScenarioID))
                While readerLayer.Read()

                    iLayer += 1
                    ' Populate it
                    ecospaceDS.ImportanceLayerDBID(iLayer) = CInt(readerLayer("LayerID"))
                    ecospaceDS.ImportanceLayerName(iLayer) = CStr(readerLayer("Name"))
                    ecospaceDS.ImportanceLayerDescription(iLayer) = CStr(readerLayer("Description"))
                    ecospaceDS.ImportanceLayerWeight(iLayer) = CSng(readerLayer("Weight"))

                    Dim strMap As String = CStr(Me.m_db.ReadSafe(readerLayer, "LayerMap", ""))
                    bSucces = bSucces And cStringUtils.StringToArray(strMap, ecospaceDS.ImportanceLayerMap(iLayer),
                                                                     ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                End While

            Catch ex As Exception
                bSucces = False
            Finally
                Me.m_db.ReleaseReader(readerLayer)
                readerLayer = Nothing
            End Try

            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        Private Function SaveEcospaceWeightLayers(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioIDSrc As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim iScenarioIDdest As Integer = 0
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim lID As Integer = 0
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim bSucces As Boolean = True
            Dim objKeys() As Object = {Nothing, Nothing}

            ' Get ID of scenario to save to
            iScenarioIDdest = idm.GetID(eDataTypes.EcoSpaceScenario, iScenarioIDSrc)
            objKeys(0) = iScenarioIDdest

            lID = CInt(Me.m_db.GetValue("SELECT MAX(LayerID) FROM EcospaceScenarioWeightLayer", 0)) + 1

            Try
                writer = Me.m_db.GetWriter("EcospaceScenarioWeightLayer")
                dt = writer.GetDataTable()

                For iLayer As Integer = 1 To ecospaceDS.nImportanceLayers

                    ' Try to find existing row
                    objKeys(1) = idm.GetID(eDataTypes.EcospaceLayerImportance, ecospaceDS.ImportanceLayerDBID(iLayer))
                    drow = dt.Rows.Find(objKeys)

                    bNewRow = (iScenarioIDSrc <> iScenarioIDdest) Or (drow Is Nothing)

                    If bNewRow Then
                        drow = writer.NewRow()
                        drow("ScenarioID") = iScenarioIDdest
                        drow("LayerID") = lID
                        idm.Add(eDataTypes.EcospaceLayerImportance, ecospaceDS.ImportanceLayerDBID(iLayer), lID)
                        lID += 1
                    Else
                        drow.BeginEdit()
                    End If

                    drow("Name") = ecospaceDS.ImportanceLayerName(iLayer)
                    drow("Description") = ecospaceDS.ImportanceLayerDescription(iLayer)
                    drow("Weight") = ecospaceDS.ImportanceLayerWeight(iLayer)
                    drow("Sequence") = iLayer
                    drow("LayerMap") = cStringUtils.ArrayToString(ecospaceDS.ImportanceLayerMap(iLayer), ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)

                    If bNewRow Then
                        writer.AddRow(drow)
                    Else
                        drow.EndEdit()
                    End If

                Next iLayer

            Catch ex As Exception
                bSucces = False
            Finally
                Me.m_db.ReleaseWriter(writer)
                writer = Nothing
            End Try

            Return bSucces

        End Function

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an ecospace Importance Layer to the active scenario in the
        ''' data source.
        ''' </summary>
        ''' <param name="strName"></param>
        ''' <param name="strDescription"></param>
        ''' <param name="sWeight"></param>
        ''' <param name="iLayerID">Database ID assigned to the new layer.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function AppendEcospaceImportanceLayer(ByVal strName As String,
                                                      ByVal strDescription As String,
                                                      ByVal sWeight As Single,
                                                      ByRef iLayerID As Integer) As Boolean _
                Implements DataSources.IEcospaceDatasource.AppendEcospaceImportanceLayer

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)

            Return Me.AddEcospaceImportanceLayer(strName, iScenarioID, strDescription, sWeight, iLayerID)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Add a ImportanceLayer to a given scenario.
        ''' </summary>
        ''' <param name="strName"></param>
        ''' <param name="strDescription"></param>
        ''' <param name="sWeight"></param>
        ''' <param name="iLayerID"></param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Private Function AddEcospaceImportanceLayer(ByVal strName As String,
                                                    ByVal iScenarioID As Integer,
                                                    ByVal strDescription As String,
                                                    ByVal sWeight As Single,
                                                    ByRef iLayerID As Integer) As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            ' MPAID unique for all scenarios
            iLayerID = CInt(Me.m_db.GetValue("SELECT MAX(LayerID) FROM EcospaceScenarioWeightLayer", 0)) + 1

            writer = Me.m_db.GetWriter("EcospaceScenarioWeightLayer")

            drow = writer.NewRow()
            drow("ScenarioID") = iScenarioID
            drow("LayerID") = iLayerID
            drow("Name") = strName
            drow("Sequence") = iLayerID
            drow("Description") = strDescription
            drow("Weight") = sWeight
            drow("LayerMap") = ""
            writer.AddRow(drow)

            Me.m_db.ReleaseWriter(writer)

            Return bSucces
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an ecospace Importance Layer from the active scenario in the
        ''' data source.
        ''' </summary>
        ''' <param name="iLayerID">Database ID of the layer to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function RemoveEcospaceImportanceLayer(ByVal iLayerID As Integer) As Boolean _
                Implements IEcospaceDatasource.RemoveEcospaceImportanceLayer

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim bSucces As Boolean = True

            Try
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioWeightLayer WHERE (ScenarioID={0}) AND (LayerID={1})", iScenarioID, iLayerID))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing Ecospace Importance Layer {1}", ex.Message, iLayerID))
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Modify

#End Region ' Weight layers

#Region " Driver layers "

#Region " Load "

        Private Function LoadEcospaceDriverLayers(ByVal iScenarioID As Integer) As Boolean

            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim readerLayer As IDataReader = Nothing
            Dim bSucces As Boolean = True
            Dim iRow As Integer = 0
            Dim iCol As Integer = 0
            Dim iLayer As Integer = 0

            Try
                readerLayer = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioDriverLayer WHERE (ScenarioID={0}) ORDER BY Sequence ASC", iScenarioID))
                While readerLayer.Read()

                    iLayer += 1
                    ecospaceDS.EnvironmentalLayerDBID(iLayer) = CInt(readerLayer("LayerID"))
                    ecospaceDS.EnvironmentalLayerName(iLayer) = CStr(readerLayer("LayerName"))
                    ecospaceDS.EnvironmentalLayerDescription(iLayer) = CStr(readerLayer("LayerDescription"))
                    ecospaceDS.EnvironmentalLayerUnits(iLayer) = CStr(Me.m_db.ReadSafe(readerLayer, "LayerUnits", ""))

                    Dim strMap As String = CStr(Me.m_db.ReadSafe(readerLayer, "LayerMap", ""))
                    bSucces = bSucces And cStringUtils.StringToArray(strMap, ecospaceDS.EnvironmentalLayerMap(iLayer),
                                                                     ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)
                End While

            Catch ex As Exception
                bSucces = False
            Finally
                Me.m_db.ReleaseReader(readerLayer)
                readerLayer = Nothing
            End Try

            Return bSucces And Me.LoadEcospaceCapacityDrivers(iScenarioID)

        End Function

        Private Function LoadEcospaceCapacityDrivers(ByVal iScenarioID As Integer) As Boolean

            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioCapacityDrivers WHERE (ScenarioID={0})", iScenarioID))
            Try

                While reader.Read()
                    Dim iGroup As Integer = Array.IndexOf(ecospaceDS.GroupDBID, CInt(reader("GroupID")))
                    Dim iShape As Integer = Array.IndexOf(Me.m_core.CapacityMapInteractionManager.MediationData.MediationDBIDs, CInt(reader("ShapeID")))
                    Dim iMap As Integer = Array.IndexOf(ecospaceDS.EnvironmentalLayerDBID, CInt(reader("VarDBID")))

                    If (iGroup > 0) And (iShape > 0) Then
                        ' Map pos 0 indicates Depth, any other ID indicates a Driver map
                        ecospaceDS.CapMapFunctions(Math.Max(0, iMap), iGroup) = iShape
                    End If
                End While
            Catch ex As Exception
                bSucces = False
            End Try
            Me.m_db.ReleaseReader(reader)

            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        Private Function SaveEcospaceCapacityMaps(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioIDSrc As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim iScenarioIDdest As Integer = 0
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim lID As Integer = 0
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim bSucces As Boolean = True
            Dim objKeys() As Object = {Nothing, Nothing}

            ' Get ID of scenario to save to
            iScenarioIDdest = idm.GetID(eDataTypes.EcoSpaceScenario, iScenarioIDSrc)
            objKeys(0) = iScenarioIDdest

            lID = CInt(Me.m_db.GetValue("SELECT MAX(LayerID) FROM EcospaceScenarioDriverLayer", 0)) + 1

            Try
                writer = Me.m_db.GetWriter("EcospaceScenarioDriverLayer")
                dt = writer.GetDataTable()

                For iLayer As Integer = 1 To ecospaceDS.nEnvironmentalDriverLayers

                    ' Try to find existing row
                    objKeys(1) = idm.GetID(eDataTypes.EcospaceLayerDriver, ecospaceDS.EnvironmentalLayerDBID(iLayer))
                    drow = dt.Rows.Find(objKeys)

                    bNewRow = (iScenarioIDSrc <> iScenarioIDdest) Or (drow Is Nothing)

                    If bNewRow Then
                        drow = writer.NewRow()
                        drow("ScenarioID") = iScenarioIDdest
                        drow("LayerID") = lID
                        idm.Add(eDataTypes.EcospaceLayerDriver, ecospaceDS.EnvironmentalLayerDBID(iLayer), lID)
                        lID += 1
                    Else
                        drow.BeginEdit()
                    End If

                    drow("Sequence") = iLayer
                    drow("LayerName") = ecospaceDS.EnvironmentalLayerName(iLayer)
                    drow("LayerDescription") = ecospaceDS.EnvironmentalLayerDescription(iLayer)
                    drow("LayerUnits") = ecospaceDS.EnvironmentalLayerUnits(iLayer)
                    drow("LayerMap") = cStringUtils.ArrayToString(ecospaceDS.EnvironmentalLayerMap(iLayer),
                                                                  ecospaceDS.InRow, ecospaceDS.InCol, ecospaceDS.DepthInput)

                    If bNewRow Then
                        writer.AddRow(drow)
                    Else
                        drow.EndEdit()
                    End If

                Next iLayer

            Catch ex As Exception
                bSucces = False
            End Try

            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)
            writer = Nothing

            Return bSucces And SaveEcospaceCapacityDrivers(idm)

        End Function

        Private Function SaveEcospaceCapacityDrivers(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim medDS As cMediationDataStructures = Me.m_core.CapacityMapInteractionManager.MediationData
            Dim iScenarioID As Integer = idm.GetID(eDataTypes.EcoSpaceScenario, ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario))
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True
            ' Clear
            Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioCapacityDrivers WHERE (ScenarioID={0})", iScenarioID))
            writer = Me.m_db.GetWriter("EcospaceScenarioCapacityDrivers")

            Try
                For iMap As Integer = 0 To ecospaceDS.nEnvironmentalDriverLayers
                    For iGroup As Integer = 1 To ecopathDS.NumGroups
                        If (ecospaceDS.CapMapFunctions(iMap, iGroup) > 0) Then
                            drow = writer.NewRow()
                            drow("ScenarioID") = iScenarioID
                            ' Referenced to Ecospace group DBIDs
                            drow("GroupID") = idm.GetID(eDataTypes.EcospaceGroup, ecopathDS.GroupDBID(iGroup))
                            drow("ShapeID") = medDS.MediationDBIDs(ecospaceDS.CapMapFunctions(iMap, iGroup))
                            drow("VarDBID") = If(iMap = 0, 0, idm.GetID(eDataTypes.EcospaceLayerDriver, ecospaceDS.EnvironmentalLayerDBID(iMap)))
                            writer.AddRow(drow)
                        End If
                    Next iGroup
                Next iMap
            Catch ex As Exception
                bSucces = False
            End Try

            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

            Return bSucces

        End Function

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IEcospaceDatasource.AddEcospaceDriverLayer" />"
        ''' -------------------------------------------------------------------
        Public Function AddEcospaceDriverLayer(ByVal strName As String, ByVal strDescription As String, strUnits As String, ByRef iDBID As Integer) As Boolean _
            Implements IEcospaceDatasource.AddEcospaceDriverLayer

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            iDBID = CInt(Me.m_db.GetValue("SELECT MAX(LayerID) FROM EcospaceScenarioDriverLayer", 0)) + 1

            ' The writer needed here will maintain row sequence for the given scenario only
            writer = Me.m_db.GetWriter("EcospaceScenarioDriverLayer")

            drow = writer.NewRow()
            drow("ScenarioID") = iScenarioID
            drow("LayerID") = iDBID
            drow("Sequence") = iDBID
            drow("LayerName") = strName
            drow("LayerDescription") = strDescription
            drow("LayerUnits") = strUnits
            drow("LayerMap") = ""
            writer.AddRow(drow)

            Me.m_db.ReleaseWriter(writer)

            Return bSucces

        End Function

        Public Function RemoveEcospaceDriverLayer(ByVal iDBID As Integer) As Boolean _
            Implements IEcospaceDatasource.RemoveEcospaceDriverLayer

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim bSucces As Boolean = True

            Try
                ' Cascading delete any data assignments
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioDataConnection WHERE (ScenarioID={0}) AND (LayerID={1}) AND (VarName='{2}')",
                                              iScenarioID, iDBID, cin.GetVarName(eVarNameFlags.LayerDriver)))
                ' Delete the layer
                Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioDriverLayer WHERE (ScenarioID={0}) AND (LayerID={1})", iScenarioID, iDBID))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing Ecospace driver layer ID {1}", ex.Message, iDBID))
                bSucces = False
            End Try
            Return bSucces

        End Function

        Public Function MoveEcospaceDriverLayer(iDBID As Integer, iPosition As Integer) As Boolean _
            Implements IEcospaceDatasource.MoveEcospaceDriverLayer
            Dim bSucces As Boolean = True
            Try
                Me.m_db.Execute(String.Format("UPDATE EcospaceScenarioDriverLayer SET Sequence={1} WHERE (LayerID={0})", iDBID, iPosition))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while moving ecospace driver layer {1}", ex.Message, iDBID))
                bSucces = False
            End Try
            Return bSucces
        End Function

#End Region ' Modify

#End Region ' Driver layers

#Region " Data connections "

        Private Function LoadEcospaceDataConnections(iScenarioID As Integer) As Boolean

            Dim spaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim spatialDS As cSpatialDataStructures = Me.m_core.m_SpatialData
            Dim man As cSpatialDataSetManager = Me.m_core.SpatialDataConnectionManager.DatasetManager
            Dim reader As IDataReader = Me.m_db.GetReader(String.Format("SELECT * FROM EcospaceScenarioDataConnection WHERE (ScenarioID={0}) ORDER BY Sequence ASC", iScenarioID))
            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim bSucces As Boolean = True

            If (reader Is Nothing) Then Return Me.IsReadOnly

            ' Spatial datastructures may use core counters that are defined only in Ecospace 
            ' Therefore, spatialDS can only be properly initialized now, right before loading the actual connection data
            spatialDS.SetDefaults()

            While (reader.Read())
                Try
                    Dim var As eVarNameFlags = cin.GetVarName(CStr(reader("VarName")))
                    Dim iLayerID As Integer = CInt(Me.m_db.ReadSafe(reader, "LayerID", 1))
                    Dim iLayer As Integer = Array.IndexOf(spaceDS.getLayerIDs(var), iLayerID)
                    Dim iConn As Integer = -1

                    ' May link to unknown layer
                    If (iLayer > 0) Then
                        ' Find next available connection slot
                        For i As Integer = 1 To cSpatialDataStructures.cMAX_CONN
                            Dim item As cSpatialDataStructures.cAdapaterConfiguration = spatialDS.Item(var, iLayer, i)
                            If (item IsNot Nothing) And (iConn = -1) Then
                                If String.IsNullOrWhiteSpace(item.DatasetGUID) Then
                                    iConn = i
                                End If
                            End If
                        Next

                        If (iConn > 0) Then
                            Dim item As cSpatialDataStructures.cAdapaterConfiguration = spatialDS.Item(var, iLayer, iConn)
                            item.DatasetGUID = CStr(Me.m_db.ReadSafe(reader, "DatasetGUID", ""))
                            item.DatasetTypeName = CStr(Me.m_db.ReadSafe(reader, "DatasetTypeName", ""))
                            item.DatasetConfig = CStr(Me.m_db.ReadSafe(reader, "DatasetCfg", ""))
                            item.ConverterTypeName = CStr(Me.m_db.ReadSafe(reader, "ConverterTypeName", ""))
                            item.ConverterConfig = CStr(Me.m_db.ReadSafe(reader, "ConverterCfg", ""))
                            item.Scale = CSng(Me.m_db.ReadSafe(reader, "Scale", 1.0!))
                            item.ScaleType = CType(Me.m_db.ReadSafe(reader, "ScaleType", cSpatialScalarDataAdapterBase.eScaleType.Relative), cSpatialScalarDataAdapterBase.eScaleType)
                        End If
                    End If

                Catch ex As Exception
                    bSucces = False
                    cLog.Write(ex, "DBDataSource::LoadEcospaceDataConnections")
                End Try
            End While

            Me.m_db.ReleaseReader(reader)
            Return bSucces

        End Function

        Private Function SaveEcospaceDataConnections(idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecospaceDS As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData
            Dim spatialDS As cSpatialDataStructures = Me.m_core.m_SpatialData
            Dim iScenarioID As Integer = ecopathDS.EcospaceScenarioDBID(ecopathDS.ActiveEcospaceScenario)
            Dim iLayerID As Integer = -1
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True
            Dim cfg As cSpatialDataStructures.cAdapaterConfiguration = Nothing
            Dim iSequence As Integer = 1

            ' Get mapped scenario ID, in case saving to a different scenario
            iScenarioID = idm.GetID(eDataTypes.EcoSpaceScenario, iScenarioID)

            ' Kaboom
            Me.m_db.Execute(String.Format("DELETE FROM EcospaceScenarioDataConnection WHERE (ScenarioID={0})", iScenarioID))

            Try
                writer = Me.m_db.GetWriter("EcospaceScenarioDataConnection")
                dt = writer.GetDataTable()

                For Each adt As cSpatialDataAdapter In spatialDS.DataAdapters
                    For i As Integer = 1 To adt.MaxLength
                        For j As Integer = 1 To cSpatialDataStructures.cMAX_CONN
                            cfg = spatialDS.Item(adt.VarName, i, j)
                            If (cfg IsNot Nothing) Then
                                Dim strDataset As String = cfg.DatasetGUID
                                If Not String.IsNullOrWhiteSpace(strDataset) Then
                                    drow = writer.NewRow()
                                    drow("ScenarioID") = iScenarioID
                                    drow("VarName") = cin.GetVarName(adt.VarName)
                                    iLayerID = ecospaceDS.getLayerID(adt.VarName, i)

                                    ' ID linkages
                                    Select Case adt.VarName
                                        Case eVarNameFlags.LayerPort, eVarNameFlags.SailCost
                                            ' Map id-ed by fleet
                                            ' iLayerID is an Ecospace fleet. However, ID mapping is based on Ecopath fleets.
                                            Dim iEcopathFleetID As Integer = ecopathDS.FleetDBID(i)
                                            iLayerID = idm.GetID(eDataTypes.EcospaceFleet, iEcopathFleetID)

                                        Case eVarNameFlags.LayerBiomassForcing, eVarNameFlags.LayerBiomassRelativeForcing,
                                             eVarNameFlags.LayerHabitatCapacity, eVarNameFlags.LayerHabitatCapacityInput,
                                             eVarNameFlags.LayerMigration
                                            ' Map id-ed by group
                                            ' iLayerID is an Ecospace group. However, ID mapping is based on Ecopath groups.
                                            Dim iEcopathGroupID As Integer = ecopathDS.GroupDBID(i)
                                            iLayerID = idm.GetID(eDataTypes.EcospaceGroup, iEcopathGroupID)

                                        Case eVarNameFlags.LayerDriver
                                            ' Map id-ed uniquely
                                            iLayerID = idm.GetID(eDataTypes.EcospaceLayerDriver, iLayerID)

                                        Case eVarNameFlags.LayerHabitat
                                            ' Map id-ed by habitat
                                            iLayerID = idm.GetID(eDataTypes.EcospaceHabitat, iLayerID)

                                        Case eVarNameFlags.LayerImportance
                                            ' Map id-ed uniquely
                                            iLayerID = idm.GetID(eDataTypes.EcospaceLayerImportance, iLayerID)

                                        Case eVarNameFlags.LayerMPA
                                            ' Map id-ed with mpa
                                            iLayerID = idm.GetID(eDataTypes.EcospaceLayerMPA, iLayerID)

                                    End Select

                                    drow("LayerID") = iLayerID
                                    drow("Sequence") = iSequence
                                    drow("DatasetGUID") = strDataset
                                    drow("DatasetTypeName") = cfg.DatasetTypeName
                                    drow("DatasetCfg") = cfg.DatasetConfig
                                    drow("ConverterTypeName") = cfg.ConverterTypeName
                                    drow("ConverterCfg") = cfg.ConverterConfig
                                    drow("Scale") = cfg.Scale
                                    drow("ScaleType") = cfg.ScaleType
                                    writer.AddRow(drow)

                                    iSequence += 1
                                End If
                            End If
                        Next j
                    Next i
                Next adt

            Catch ex As Exception
                bSucces = False
            End Try

            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)
            writer = Nothing

            Return bSucces

        End Function

#End Region ' Data adapters

#End Region ' Ecospace

#Region " Ecotracer "

#Region " Diagnostics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States if the data source has unsaved changes for Ecotracer.
        ''' </summary>
        ''' <returns>True if the data source has pending changes for Ecotracer.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsEcotracerModified() As Boolean _
                 Implements DataSources.IEcotracerDatasource.IsEcotracerModified

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData

            ' Hmm, maybe the data source should have a better way to 'remember' whether a tracer scenario has been loaded.
            If Not Me.IsConnected() Then Return False
            If ecopathDS.ActiveEcotracerScenario < 0 Then Return False

            Return Me.IsChanged(s_EcotracerComponents)

        End Function

#End Region ' Diagnostics

#Region " Load "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Loads an Ecotracer scenario from the data source.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID of the scenario to load.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>An implementing class should ensure that this load will cascade to
        ''' load all information pertaining to a scenario.</remarks>
        ''' -------------------------------------------------------------------
        Public Function LoadEcotracerScenario(ByVal iScenarioID As Integer) As Boolean _
            Implements DataSources.IEcotracerDatasource.LoadEcotracerScenario

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim tracerDS As cContaminantTracerDataStructures = Me.m_core.m_tracerData
            Dim iConForceNumber As Integer = 0
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True

            ' JS08Dec07: Ideally, this should happen here but both Ecosim and Ecospace
            '            assume that the tracer data has already been dimensioned to the
            '            number of groups long before a tracer scenario has been loaded.
            '            This needs to change!
            tracerDS.RedimByNGroups(ecopathDS.NumGroups)

            reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcotracerScenario WHERE (ScenarioID={0})", iScenarioID))
            Try
                ' Read the one record
                reader.Read()
                tracerDS.Czero(0) = CSng(reader("Czero"))
                tracerDS.Cinflow(0) = CSng(reader("Cinflow"))
                tracerDS.CoutFlow(0) = CSng(reader("Coutflow"))
                tracerDS.cdecay(0) = CSng(reader("Cdecay"))
                'iConForceNumber = CInt(Me.m_db.ReadSafe(reader, "ConForcingShapeID", 0))
                'tracerDS.ConForceNumber = Math.Max(0, Array.IndexOf(ecosimDS.ForcingDBIDs, iConForceNumber))

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Ecotracer Scenario {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)

            ' Set active tracer scenario
            ecopathDS.ActiveEcotracerScenario = Array.IndexOf(ecopathDS.EcotracerScenarioDBID, iScenarioID)

            ' Load additional data
            bSucces = bSucces And Me.LoadEcotracerGroups(iScenarioID)

            Me.ClearChanged(s_EcotracerComponents)

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load Ecotracer groups from the data source.
        ''' </summary>
        ''' <param name="iScenarioID">The Ecotracer scenario to load groups for.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function LoadEcotracerGroups(ByVal iScenarioID As Integer) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim tracerDS As cContaminantTracerDataStructures = Me.m_core.m_tracerData
            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True
            Dim iGroup As Integer = 0

            ' Read the data
            Try
                reader = Me.m_db.GetReader(String.Format("SELECT * FROM EcotracerScenarioGroup WHERE (ScenarioID={0})", iScenarioID))
                While reader.Read()

                    ' Resolve group index
                    iGroup = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("EcopathGroupID")))
                    ' Sanity check
                    Debug.Assert(iGroup > -1)
                    ' Load the data
                    tracerDS.Czero(iGroup) = CSng(reader("Czero"))
                    tracerDS.Cimmig(iGroup) = CSng(reader("Cimmig"))
                    tracerDS.Cenv(iGroup) = CSng(reader("Cenv"))
                    tracerDS.cdecay(iGroup) = CSng(reader("Cdecay"))
                    tracerDS.CassimProp(iGroup) = CSng(Me.m_db.ReadSafe(reader, "CassimProp", 0.1))
                    tracerDS.CmetabolismRate(iGroup) = CSng(Me.m_db.ReadSafe(reader, "CmetabolismRate", 1.0!))

                End While
                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading Ecotracer group {1}", ex.Message, iGroup))
                bSucces = False
            End Try
            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the current active Ecotracer scenario in the data source under
        ''' a given database ID.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID to save the current scenario to.
        ''' If this parameter is left blank, the current scenario is saved
        ''' under its own database ID.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function SaveEcotracerScenario(ByVal iScenarioID As Integer) As Boolean _
             Implements DataSources.IEcotracerDatasource.SaveEcotracerScenario

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim tracerDS As cContaminantTracerDataStructures = Me.m_core.m_tracerData
            Dim iActiveScenarioID As Integer = ecopathDS.EcotracerScenarioDBID(ecopathDS.ActiveEcotracerScenario)
            Dim idm As cIDMappings = Nothing
            Dim bSucces As Boolean = True

            ' Abort if there is no active scenario
            If iActiveScenarioID = 0 Then Return False

            ' Prepare for saving
            idm = New cIDMappings()
            If iScenarioID = 0 Then iScenarioID = iActiveScenarioID

            ' Duplicating a scenario?
            If iScenarioID <> iActiveScenarioID Then
                ' #Yes: add ID mapping to allow copying of scenario content
                idm.Add(eDataTypes.EcotracerScenario, iActiveScenarioID, iScenarioID)
            End If

            ' Start transaction
            bSucces = Me.m_db.BeginTransaction()
            ' Save scenario
            bSucces = bSucces And Me.SaveEcotracerScenario(idm)
            ' Commit transaction
            If bSucces Then
                bSucces = Me.m_db.CommitTransaction(True)
            Else
                Me.m_db.RollbackTransaction()
            End If

            If bSucces Then
                ' Reload ecotracer scenario definitions
                Me.LoadEcotracerScenarioDefinitions()
                ' Clear changed admin
                Me.ClearChanged(s_EcotracerComponents)
            End If

            Return bSucces
        End Function

#Region " Internals "

        Private Function SaveEcotracerScenario(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecosimDS As cEcosimDatastructures = Me.m_core.m_EcoSimData
            Dim tracerDS As cContaminantTracerDataStructures = Me.m_core.m_tracerData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenario As Integer = ecopathDS.ActiveEcotracerScenario
            Dim iScenarioID As Integer = 0
            Dim bSucces As Boolean = True

            iScenarioID = idm.GetID(eDataTypes.EcotracerScenario, ecopathDS.EcotracerScenarioDBID(iScenario))

            Try

                writer = Me.m_db.GetWriter("EcotracerScenario")
                dt = writer.GetDataTable()
                drow = dt.Rows.Find(iScenarioID)

                drow.BeginEdit()
                drow("Czero") = tracerDS.Czero(0)
                drow("Cinflow") = tracerDS.Cinflow(0)
                drow("Coutflow") = tracerDS.CoutFlow(0)
                drow("Cdecay") = tracerDS.cdecay(0)
                drow("ConForcingShapeID") = ecosimDS.ForcingDBIDs(tracerDS.ConForceNumber)
                ' ------------------------------------------
                drow("LastSaved") = cDateUtils.DateToJulian()
                drow("LastSavedVersion") = cAssemblyUtils.GetVersion().ToString
                drow.EndEdit()

                ' Save changes
                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving ecotracer scenario {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try

            bSucces = bSucces And Me.SaveEcotracerGroups(idm)

            Return bSucces

        End Function

        Private Function SaveEcotracerGroups(ByVal idm As cIDMappings) As Boolean

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim tracerDS As cContaminantTracerDataStructures = Me.m_core.m_tracerData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim iScenarioID As Integer = ecopathDS.EcotracerScenarioDBID(ecopathDS.ActiveEcotracerScenario)
            Dim drow As DataRow = Nothing
            Dim iGroup As Integer = 0
            Dim objKeys() As Object = {Nothing, Nothing} ' Composite key to find group per scenario
            Dim bSucces As Boolean = True

            ' Get mapped scenario ID, in case saving to a different scenario
            iScenarioID = idm.GetID(eDataTypes.EcotracerScenario, iScenarioID)
            objKeys(0) = iScenarioID

            Try
                writer = Me.m_db.GetWriter("EcotracerScenarioGroup")
                dt = writer.GetDataTable()

                For iGroup = 1 To ecopathDS.NumGroups

                    ' Find group ID, it may be mapped to a different ID when saving to a new scenario
                    objKeys(1) = ecopathDS.GroupDBID(iGroup)

                    ' Find existing row
                    drow = dt.Rows.Find(objKeys)
                    Debug.Assert(drow IsNot Nothing, String.Format("Cannot find existing row for group {0}", ecopathDS.GroupDBID(iGroup)))

                    drow("CZero") = tracerDS.Czero(iGroup)
                    drow("Cimmig") = tracerDS.Cimmig(iGroup)
                    drow("Cenv") = tracerDS.Cenv(iGroup)
                    drow("Cdecay") = tracerDS.cdecay(iGroup)
                    drow("CassimProp") = tracerDS.CassimProp(iGroup)
                    drow("CmetabolismRate") = tracerDS.CmetabolismRate(iGroup)

                Next iGroup

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while saving EcotracerGroup", ex.Message))
                bSucces = False
            End Try

            ' Save changes
            Me.m_db.ReleaseWriter(writer, True)

            Return bSucces

        End Function

#End Region ' Internals

#End Region ' Save

#Region " Modify "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Adds an Ecotracer scenario to the data source.
        ''' </summary>
        ''' <param name="strScenarioName">Name to assign to new scenario.</param>
        ''' <param name="strDescription">Description to assign to new scenario.</param>
        ''' <param name="strAuthor">Author to assign to the new scenario.</param>
        ''' <param name="strContact">Contact info to assign to the new scenario.</param>
        ''' <param name="iScenarioID">Database ID assigned to the new scenario.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function AppendEcotracerScenario(ByVal strScenarioName As String, ByVal strDescription As String, ByVal strAuthor As String, ByVal strContact As String, ByRef iScenarioID As Integer) As Boolean _
                 Implements DataSources.IEcotracerDatasource.AppendEcotracerScenario

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim tracerDS As cContaminantTracerDataStructures = Me.m_core.m_tracerData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                iScenarioID = CInt(Me.m_db.GetValue("SELECT MAX(ScenarioID) FROM EcotracerScenario", 0)) + 1

                Me.m_db.BeginTransaction()

                writer = Me.m_db.GetWriter("EcotracerScenario")

                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("ScenarioName") = strScenarioName
                drow("Description") = strDescription
                drow("Author") = strAuthor
                drow("Contact") = strContact
                drow("LastSaved") = cDateUtils.DateToJulian()
                writer.AddRow(drow)

                Me.m_db.ReleaseWriter(writer)

                ' ------
                ' Generate Ecopath objects for new Ecotracer scenario
                ' ------

                ' First duplicate all Ecospace 'objects'
                For i As Integer = 1 To ecopathDS.NumGroups
                    ' Add group to the new scenario
                    bSucces = bSucces And Me.AddEcotracerGroup(ecopathDS.GroupDBID(i), iScenarioID)
                Next

                If bSucces Then
                    bSucces = Me.m_db.CommitTransaction(True)
                Else
                    Me.m_db.RollbackTransaction()
                End If

                ' Reload scenario definitions
                bSucces = bSucces And Me.LoadEcotracerScenarioDefinitions()

                Me.ClearChanged(s_EcotracerComponents)

            Catch ex As Exception
                Me.m_db.RollbackTransaction()
                Me.LogMessage(String.Format("Error {0} occurred while appending Ecotracer scenario {1}", ex.Message, strScenarioName))
                bSucces = False
            End Try

            Return bSucces
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes an Ecotracer scenario from the data source.
        ''' </summary>
        ''' <param name="iScenarioID">Database ID of the scenario to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function RemoveEcotracerScenario(ByVal iScenarioID As Integer) As Boolean _
                 Implements DataSources.IEcotracerDatasource.RemoveEcotracerScenario

            Dim bSucces As Boolean = True

            Try
                ' Delete 'soft links'
                '    DB update 6.036!
                Me.m_db.Execute(String.Format("DELETE FROM EcotracerScenarioGroup WHERE (ScenarioID={0})", iScenarioID))
                ' Delete scenario
                Me.m_db.Execute(String.Format("DELETE FROM EcotracerScenario WHERE (ScenarioID={0})", iScenarioID))
            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while removing Ecotracer scenarioID {1}", ex.Message, iScenarioID))
                bSucces = False
            End Try

            ' Reload scenario definitions
            bSucces = bSucces And Me.LoadEcotracerScenarioDefinitions()

            Return bSucces

        End Function

        ''' <summary>
        ''' Create a group for each Ecotracer scenario
        ''' </summary>
        ''' <param name="iEcopathGroupID">Ecopath Group DBID</param>
        Private Function AddEcotracerGroupToAllScenarios(ByVal iEcopathGroupID As Integer) As Boolean

            Dim reader As IDataReader = Nothing
            Dim bSucces As Boolean = True

            Try
                reader = Me.m_db.GetReader("SELECT ScenarioID FROM EcotracerScenario")
                While reader.Read()
                    bSucces = bSucces And AddEcotracerGroup(iEcopathGroupID, CInt(reader("ScenarioID")))
                End While
                Me.m_db.ReleaseReader(reader)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' <summary>
        ''' Add a group to a given Ecotracer scenario.
        ''' </summary>
        ''' <param name="iEcopathGroupID"><see cref="cEcoPathGroupInput.DBID">Ecopath ID</see> of this group</param>
        ''' <param name="iScenarioID"><see cref="cEcotracerScenario.DBID">Ecotracer scenario ID</see> of the scenario to add the group to.</param>
        ''' <returns>True if successful.</returns>
        Private Function AddEcotracerGroup(ByVal iEcopathGroupID As Integer, ByVal iScenarioID As Integer) As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                ' Add group
                writer = Me.m_db.GetWriter("EcotracerScenarioGroup")

                drow = writer.NewRow()
                drow("ScenarioID") = iScenarioID
                drow("EcopathGroupID") = iEcopathGroupID
                writer.AddRow(drow)

                Me.m_db.ReleaseWriter(writer)

            Catch ex As Exception
                bSucces = False
            End Try

            Return bSucces
        End Function

#End Region ' Modify

#End Region ' Ecotracer

#Region " Auxillary data "

        Private Function LoadAuxillaryData() As Boolean

            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM Auxillary")
            Dim strValueID As String = ""
            Dim strRemark As String = ""
            Dim strVisualStyle As String = ""
            Dim ad As cAuxiliaryData = Nothing
            Dim bSucces As Boolean = True

            Me.m_core.m_dtAuxiliaryData.Clear()

            Try
                While reader.Read()

                    strValueID = CStr(reader("ValueID"))
                    strRemark = CStr(Me.m_db.ReadSafe(reader, "Remark", ""))
                    strVisualStyle = CStr(Me.m_db.ReadSafe(reader, "VisualStyle", ""))

                    ad = Me.m_core.AuxillaryData(strValueID)
                    ad.AllowValidation = False

                    ad.DBID = CInt(reader("DBID"))
                    ad.Remark = strRemark
                    ad.VisualStyle = cVisualStyleReader.StringToStyle(strVisualStyle)
                    ad.Settings.Load(CStr(Me.m_db.ReadSafe(reader, "Settings", "")))

                    ad.AllowValidation = True

                End While

            Catch ex As Exception
                Me.LogMessage(String.Format("Error {0} occurred while reading AuxillaryData", ex.Message))
                bSucces = False
            End Try

            Me.m_db.ReleaseReader(reader)
            Return bSucces

        End Function

        Private Function SaveAuxillaryData() As Boolean

            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim ad As cAuxiliaryData = Nothing
            Dim iDBID As Integer = 0
            Dim bSucces As Boolean = True

            iDBID = CInt(Me.m_db.GetValue("SELECT MAX(DBID) FROM Auxillary", 0)) + 1

            Me.m_db.Execute("DELETE * FROM Auxillary")
            writer = Me.m_db.GetWriter("Auxillary")
            Try

                For Each strValueID As String In Me.m_core.m_dtAuxiliaryData.Keys

                    ' Get actual remark instance
                    ad = m_core.m_dtAuxiliaryData(strValueID)
                    ' Has anything to save?
                    If (Not ad.IsEmpty()) Then

                        ' Make row
                        drow = writer.NewRow()

                        If ad.DBID <= 0 Then ad.DBID = iDBID : iDBID += 1

                        drow("DBID") = ad.DBID
                        drow("ValueID") = strValueID
                        drow("Remark") = ad.Remark
                        drow("VisualStyle") = cVisualStyleReader.StyleToString(ad.VisualStyle)
                        drow("Settings") = ad.Settings.ToString()

                        writer.AddRow(drow)
                    End If
                Next
            Catch ex As Exception
                bSucces = False
            End Try

            ' Save changes
            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

            Return bSucces

        End Function

        Private Function DuplicateAuxillaryData(ByVal idm As cIDMappings,
                                                ByVal dt As eDataTypes,
                                                ByVal iDBID As Integer) As Boolean

            ' Do not bother if no such mappings exist
            If idm.GetID(dt, iDBID) = iDBID Then Return True

            ' Blunt way first
            Dim writer As cEwEDatabase.cEwEDbWriter = Me.m_db.GetWriter("Auxillary")
            Dim drow As DataRow = Nothing
            Dim iAdDBID As Integer = 0
            Dim ad As cAuxiliaryData = Nothing
            Dim key As cValueID = Nothing
            Dim bSucces As Boolean = True

            iAdDBID = CInt(Me.m_db.GetValue("SELECT MAX(DBID) FROM Auxillary", 0)) + 1

            For Each ad In Me.m_core.m_dtAuxiliaryData.Values

                ' Is one intended for the requested source?
                If ((ad.Key.DataTypePrim = dt) And (ad.Key.DBIDPrim) = iDBID) Then
                    ' #Yes: does this one have data?
                    If Not ad.IsEmpty Then
                        ' #Yes: Save it
                        Try

                            ' Make new key
                            key = ad.Key
                            key = New cValueID(dt, idm.GetID(dt, iDBID),
                                               key.VarNameText,
                                               key.DataTypeSec, idm.GetID(key.DataTypeSec, ad.Key.DBIDSec))

                            ' Start new row
                            drow = writer.NewRow()
                            ' Config row
                            drow("DBID") = iAdDBID
                            drow("ValueID") = key.ToString
                            drow("Remark") = ad.Remark
                            drow("VisualStyle") = cVisualStyleReader.StyleToString(ad.VisualStyle)
                            ' Add
                            writer.AddRow(drow)
                            ' Next
                            iAdDBID += 1

                        Catch ex As Exception
                            bSucces = False
                        End Try

                    End If

                End If

            Next ad

            ' Commit
            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

            Return bSucces

        End Function

#End Region ' Auxillary data

#Region " Meta data "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IEwEDatasourceMetadata.GetDescription"/>
        ''' -------------------------------------------------------------------
        Public Function GetDescription(ByVal dt As EwEUtils.Core.eDataTypes, ByVal iDBID As Integer) As String _
            Implements IEwEDatasourceMetadata.GetDescription

            Dim strTable As String = ""
            Dim strDBIDField As String = ""
            Dim strNameField As String = ""

            Select Case dt

                Case eDataTypes.EwEModel
                    strTable = "EcopathModel"
                    strNameField = "Name"

                Case eDataTypes.EcoPathGroupInput, eDataTypes.EcoPathGroupOutput
                    strTable = "EcopathGroup"
                    strDBIDField = "GroupID"
                    strNameField = "GroupName"

                Case eDataTypes.EcosimFleetInput
                    strTable = "EcopathFleet"
                    strDBIDField = "FleetID"
                    strNameField = "FleetName"

                Case eDataTypes.EcoSimScenario
                    strTable = "EcosimScenario"
                    strDBIDField = "ScenarioID"
                    strNameField = "ScenarioName"

                Case eDataTypes.EcoSimGroupInput, eDataTypes.EcoSimGroupOutput
                    ' Link to Ecopath group
                    Return Me.GetDescription(eDataTypes.EcoPathGroupInput, CInt(Me.m_db.GetValue("SELECT EcopathGroupID FROM EcoSimScenarioGroup WHERE GroupID=" & iDBID)))

                Case eDataTypes.EcosimFleetInput, eDataTypes.EcosimFleetOutput
                    ' Link to Ecopath fleet
                    Return Me.GetDescription(eDataTypes.EcoPathGroupInput, CInt(Me.m_db.GetValue("SELECT EcopathFleetID FROM EcoSimScenarioFleet WHERE FleetID=" & iDBID)))

                Case eDataTypes.EcoSpaceScenario
                    strTable = "EcospaceScenario"
                    strDBIDField = "ScenarioID"
                    strNameField = "ScenarioName"

                Case eDataTypes.EcospaceGroup, eDataTypes.EcospaceGroupOuput
                    ' Link to Ecopath group
                    Return Me.GetDescription(eDataTypes.EcoPathGroupInput, CInt(Me.m_db.GetValue("SELECT EcopathGroupID FROM EcospaceScenarioGroup WHERE GroupID=" & iDBID)))

                Case eDataTypes.EcospaceFleet, eDataTypes.EcospaceFleetOuput
                    ' Link to Ecopath fleet
                    Return Me.GetDescription(eDataTypes.EcoPathGroupInput, CInt(Me.m_db.GetValue("SELECT EcopathFleetID FROM EcospaceScenarioFleet WHERE FleetID=" & iDBID)))

                Case eDataTypes.EcotracerScenario
                    strTable = "EcospaceScenario"
                    strDBIDField = "ScenarioID"
                    strNameField = "ScenarioName"

                Case Else
                    Return ""

            End Select

            Dim strSQL As String = "SELECT " & strNameField & " FROM " & strTable
            If Not String.IsNullOrEmpty(strDBIDField) Then strSQL &= " WHERE " & strDBIDField & "=" & iDBID

            Return CStr(Me.m_db.GetValue(strSQL))

        End Function

#End Region ' Meta data

#Region " Ecopath samples "

#Region " Load "

        Private Function LoadEcopathSamples() As Boolean _
            Implements IEcopathSampleDataSource.LoadSamples

            Dim ds As Samples.cEcopathSampleDatastructures = Me.m_core.m_SampleData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathSample")
            Dim iSeq As Integer = 0
            Dim bSucces As Boolean = True

            ds.m_samples.Clear()

            While reader.Read()
                iSeq += 1
                Try
                    Dim sample As New Samples.cEcopathSample(Me.m_core, CInt(reader("SampleID")), iSeq)
                    sample.AllowValidation = False
                    sample.Hash = CStr(reader("Hash"))
                    sample.Source = CStr(reader("Source"))
                    sample.Rating = CInt(reader("Rating"))
                    sample.Generated = cDateUtils.JulianToDate(CDbl(reader("Generated")))
                    sample.AllowValidation = True

                    ds.m_samples.Add(sample)
                Catch ex As Exception
                    bSucces = False
                End Try
            End While

            bSucces = bSucces And Me.LoadGroupSamples() And
                                  Me.LoadDietSamples() And
                                  Me.LoadGroupCatchSamples()

            If Not bSucces Then ds.m_samples.Clear()


            Return bSucces

        End Function

        Private Function LoadGroupSamples() As Boolean

            Dim ds As Samples.cEcopathSampleDatastructures = Me.m_core.m_SampleData
            Dim dt As New Dictionary(Of Long, Samples.cEcopathSample)
            For Each s As Samples.cEcopathSample In ds.m_samples
                dt(s.DBID) = s
            Next

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathGroupSample")
            Dim bSucces As Boolean = True

            While reader.Read()
                Try
                    Dim DBID As Integer = CInt(reader("SampleID"))
                    Dim iGroup As Integer = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("GroupID")))

                    If dt.ContainsKey(DBID) And iGroup > 0 Then
                        Dim sample As Samples.cEcopathSample = dt(DBID)
                        sample.B(iGroup) = CSng(reader("Biomass"))
                        sample.PB(iGroup) = CSng(reader("ProdBiom"))
                        sample.QB(iGroup) = CSng(reader("ConsBiom"))
                        sample.EE(iGroup) = CSng(reader("EcoEfficiency"))
                        sample.BA(iGroup) = CSng(reader("BiomAcc"))
                        sample.BaBi(iGroup) = CSng(Me.m_db.ReadSafe(reader, "BiomAccRate", 0))
                        If (iGroup <= Me.m_core.nLivingGroups) Then
                            sample.DC(iGroup, 0) = CSng(Me.m_db.ReadSafe(reader, "ImpVar", 0))
                        End If
                    End If
                Catch ex As Exception
                    Me.LogMessage(String.Format("Error {0} occurred while reading EcopathGroupSample", ex.Message))
                    bSucces = False
                End Try
            End While
            Return bSucces

        End Function

        Private Function LoadDietSamples() As Boolean

            Dim ds As Samples.cEcopathSampleDatastructures = Me.m_core.m_SampleData
            Dim dt As New Dictionary(Of Integer, Samples.cEcopathSample)
            For Each s As Samples.cEcopathSample In ds.m_samples
                dt(s.DBID) = s
            Next

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathDietCompSample")
            Dim bSucces As Boolean = True

            While reader.Read()
                Try
                    Dim DBID As Integer = CInt(reader("SampleID"))
                    Dim iPred As Integer = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("PredID")))
                    Dim iPrey As Integer = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("PreyID")))

                    If dt.ContainsKey(DBID) And (iPred >= 0) And (iPrey >= 0) Then
                        Dim sample As Samples.cEcopathSample = dt(DBID)
                        Dim sDiet As Single = CSng(reader("Diet"))
                        If (sDiet > cCore.NULL_VALUE) Then
                            sample.DC(iPred, iPrey) = sDiet
                        End If
                    End If
                Catch ex As Exception
                    Me.LogMessage(String.Format("Error {0} occurred while reading EcopathDietCompSample", ex.Message))
                    bSucces = False
                End Try
            End While
            Return bSucces

        End Function

        Private Function LoadGroupCatchSamples() As Boolean

            Dim ds As Samples.cEcopathSampleDatastructures = Me.m_core.m_SampleData
            Dim dt As New Dictionary(Of Integer, Samples.cEcopathSample)
            For Each sample As Samples.cEcopathSample In ds.m_samples
                dt(sample.DBID) = sample
            Next

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim reader As IDataReader = Me.m_db.GetReader("SELECT * FROM EcopathGroupCatchSample")
            Dim bSucces As Boolean = True

            While reader.Read()
                Try
                    Dim DBID As Integer = CInt(reader("SampleID"))
                    Dim iGroup As Integer = Array.IndexOf(ecopathDS.GroupDBID, CInt(reader("GroupID")))
                    Dim iFleet As Integer = Array.IndexOf(ecopathDS.FleetDBID, CInt(reader("FleetID")))

                    If dt.ContainsKey(DBID) And iGroup > 0 And iFleet > 0 Then
                        Dim sample As Samples.cEcopathSample = dt(DBID)
                        sample.Landing(iFleet, iGroup) = CSng(reader("Landing"))
                        sample.Discard(iFleet, iGroup) = CSng(reader("Discards"))
                    End If
                Catch ex As Exception
                    Me.LogMessage(String.Format("Error {0} occurred while reading EcopathGroupCatchSample", ex.Message))
                    bSucces = False
                End Try
            End While
            Return bSucces

        End Function

#End Region ' Load

#Region " Save "

        Private Function SaveEcopathSamples() As Boolean _
            Implements IEcopathSampleDataSource.SaveEcopathSamples

            ' Only save rating, the other data is fixed when the sample is added

            Dim ds As EwECore.Samples.cEcopathSampleDatastructures = Me.m_core.m_SampleData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bSucces As Boolean = True

            Try
                writer = Me.m_db.GetWriter("EcopathSample")
                dt = writer.GetDataTable()

                For Each sample As Samples.cEcopathSample In ds.m_samples

                    drow = dt.Rows.Find(sample.DBID)
                    Debug.Assert(drow IsNot Nothing)
                    drow.BeginEdit()
                    drow("Rating") = sample.Rating
                    drow.EndEdit()
                Next

                writer.Commit()

            Catch ex As Exception
                bSucces = False
            End Try

            Return Me.m_db.ReleaseWriter(writer, bSucces) And bSucces

        End Function

#End Region ' Save

#Region " Modify "

        Public Function AddSample(sample As Samples.cEcopathSample) As Boolean _
            Implements IEcopathSampleDataSource.AddSample

            Dim ecopathDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim writer As cEwEDatabase.cEwEDbWriter = Nothing
            Dim drow As DataRow = Nothing
            Dim iScenario As Integer = 0
            Dim bSucces As Boolean = True

            sample.DBID = CInt(Me.m_db.GetValue("SELECT MAX(SampleID) FROM EcopathSample", 0)) + 1
            writer = Me.m_db.GetWriter("EcopathSample")
            Try
                drow = writer.NewRow()
                drow("SampleID") = sample.DBID
                drow("Hash") = sample.Hash
                drow("Source") = sample.Source
                drow("Rating") = sample.Rating
                drow("Generated") = cDateUtils.DateToJulian(sample.Generated)
                writer.AddRow(drow)
            Catch ex As Exception
                bSucces = False
            End Try
            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

            writer = Me.m_db.GetWriter("EcopathGroupSample")
            Try
                For iGroup As Integer = 1 To ecopathDS.NumGroups
                    drow = writer.NewRow()
                    drow("SampleID") = sample.DBID
                    drow("GroupID") = ecopathDS.GroupDBID(iGroup)
                    drow("Biomass") = sample.B(iGroup)
                    drow("ProdBiom") = sample.PB(iGroup)
                    drow("ConsBiom") = sample.QB(iGroup)
                    drow("EcoEfficiency") = sample.EE(iGroup)
                    drow("BiomAcc") = sample.BA(iGroup)
                    drow("BiomAccRate") = sample.BaBi(iGroup)
                    If (iGroup <= Me.m_core.nLivingGroups) Then
                        drow("ImpVar") = sample.DC(iGroup, 0)
                    Else
                        drow("ImpVar") = cCore.NULL_VALUE
                    End If
                    writer.AddRow(drow)
                Next
            Catch ex As Exception
                bSucces = False
            End Try
            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

            writer = Me.m_db.GetWriter("EcopathDietCompSample")
            Try
                For iPred As Integer = 1 To ecopathDS.NumLiving
                    For iPrey As Integer = 1 To ecopathDS.NumGroups
                        If (sample.DC(iPred, iPrey) > 0) Then
                            drow = writer.NewRow()
                            drow("SampleID") = sample.DBID
                            drow("PredID") = ecopathDS.GroupDBID(iPred)
                            drow("PreyID") = ecopathDS.GroupDBID(iPrey)
                            drow("Diet") = sample.DC(iPred, iPrey)
                            writer.AddRow(drow)
                        End If
                    Next iPrey
                Next iPred
            Catch ex As Exception
                bSucces = False
            End Try
            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

            writer = Me.m_db.GetWriter("EcopathGroupCatchSample")
            Try
                For iFleet As Integer = 1 To ecopathDS.NumFleet
                    For iGroup As Integer = 1 To ecopathDS.NumGroups
                        If ((sample.Landing(iFleet, iGroup) + sample.Discard(iFleet, iGroup)) > 0) Then
                            drow = writer.NewRow()
                            drow("SampleID") = sample.DBID
                            drow("FleetID") = ecopathDS.FleetDBID(iFleet)
                            drow("GroupID") = ecopathDS.GroupDBID(iGroup)
                            drow("Landing") = sample.Landing(iFleet, iGroup)
                            drow("Discards") = sample.Discard(iFleet, iGroup)
                            writer.AddRow(drow)
                        End If
                    Next iGroup
                Next iFleet
            Catch ex As Exception
                bSucces = False
            End Try
            bSucces = bSucces And Me.m_db.ReleaseWriter(writer, bSucces)

            Return bSucces

        End Function

        Public Function RemoveSample(sample As Samples.cEcopathSample) As Boolean _
            Implements IEcopathSampleDataSource.RemoveSample

            If (sample Is Nothing) Then Return False
            Return Me.m_db.Execute("DELETE * FROM EcopathGroupCatchSample WHERE SampleID=" & sample.DBID) And
                   Me.m_db.Execute("DELETE * FROM EcopathDietCompSample WHERE SampleID=" & sample.DBID) And
                   Me.m_db.Execute("DELETE * FROM EcopathGroupSample WHERE SampleID=" & sample.DBID) And
                   Me.m_db.Execute("DELETE * FROM EcopathSample WHERE SampleID=" & sample.DBID)

        End Function

#End Region ' Modify

        Public Overloads Function CopyTo(ds As IEcopathSampleDataSource) As Boolean _
            Implements IEcopathSampleDataSource.CopyTo
            Return False
        End Function

#End Region ' Ecopath samples

    End Class

End Namespace
