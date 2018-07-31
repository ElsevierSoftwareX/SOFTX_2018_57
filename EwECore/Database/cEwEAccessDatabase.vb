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
Imports System.Data.OleDb
Imports System.Reflection
Imports System.Text
Imports EwECore.DataSources
Imports EwEUtils.Database
Imports EwEUtils.Utilities
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities

#End Region ' Imports

Namespace Database

    ''' =======================================================================
    ''' <summary>
    ''' Database class specialized for storing and writing EwE data to Microsoft 
    ''' Access databases.
    ''' </summary>
    ''' <remarks>
    ''' This class wraps Microsoft Access specifics such as connection
    ''' strings, deals with deficiencies in the SQL implementation of Bill's 
    ''' beast, and provides default field values from the Access DB schema.
    ''' </remarks>
    ''' =======================================================================
    Public Class cEwEAccessDatabase
        Inherits cEwEDatabase

#Region " Private vars "

        ''' <summary>A connection to an OleDb database, if any.</summary>
        Public m_conn As OleDbConnection = Nothing
        ''' <summary>The connection string to connect to a MDB database.</summary>
        Private m_strConnectionMDB As String = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0};"
        ' Download from http://www.microsoft.com/download/en/details.aspx?displaylang=en&id=13255.
        ' only works if office is 64 bit too. To detect
        ' To determine if office 2010 is 32 bit or 64 bit, we could check a registry key named 
        ' bitness. For more information, please refer to this article: http://technet.microsoft.com/en-us/library/ee681792.aspx.
        ' Also here is a resource with a same question you could refer to: 
        ' Detect whether Office 2010 is 32bit or 64bit via the registry (http://stackoverflow.com/questions/2203980/detect-whether-office-2010-is-32bit-or-64bit-via-the-registry).
        ''' <summary>The connection string to connect to a ACCDB database.</summary>
        Private m_strConnectionACCDB As String = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Persist Security Info=False;"
        ''' <summary>File name to access database.</summary>
        Private m_strFileName As String = ""

#End Region ' Private vars

#Region " Generic "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new M$ Access database.
        ''' </summary>
        ''' <param name="strDatabase">The file name of the .MDB to create.</param>
        ''' <param name="strAuthor">Name of the author to assign.</param>
        ''' <param name="strModelName">Name of the model to use.</param>
        ''' <param name="bOverwrite">States whether an existing database may be overwritten.</param>
        ''' <param name="format">Database format type to use. If not set, the 
        ''' database type is deducted from the <paramref name="strDatabase">database</paramref>.</param>
        ''' <returns>A <see cref="eDatasourceAccessType">eDatasourceAccessType</see> value</returns>
        ''' <remarks>Note that this will NOT open the newly created database.</remarks>
        ''' -------------------------------------------------------------------
        Public Overrides Function Create(ByVal strDatabase As String, _
                ByVal strModelName As String, _
                Optional ByVal bOverwrite As Boolean = False, _
                Optional ByVal format As eDataSourceTypes = eDataSourceTypes.NotSet, _
                Optional strAuthor As String = "") As eDatasourceAccessType

            Dim strSource As String = ""
            Dim datResult As eDatasourceAccessType = eDatasourceAccessType.Success

            If format = eDataSourceTypes.NotSet Then
                format = cDataSourceFactory.GetSupportedType(strDatabase)
            End If

            Select Case format
                Case eDataSourceTypes.Access2003
                    strSource = "EwE6.mdb"
                    cLog.Write("Create DB: selected MDB format")
                Case eDataSourceTypes.Access2007
                    strSource = "EwE6.accdb"
                    cLog.Write("Create DB: selected ACCDB format")
                Case Else
                    datResult = eDatasourceAccessType.Failed_UnknownType
                    cLog.Write("Create DB: cannot determine format")
            End Select

            If (datResult = eDatasourceAccessType.Success) Then

                ' Save resource file
                If cResourceUtils.SaveResourceToFile(strSource, strDatabase, bOverwrite, Assembly.GetExecutingAssembly()) Then
                    Try
                        'Try to open the database to update the model name
                        Dim db As New cEwEAccessDatabase()
                        datResult = db.Open(strDatabase, format)
                        If (datResult = eDatasourceAccessType.Opened) Then
                            db.Execute(cStringUtils.Localize("UPDATE EcopathModel SET Name='{0}', Author='{1}' WHERE ModelID=1", strModelName, strAuthor))
                            Try
                                ' Egg - over-easy but slightly obfuscated ;)
                                If strModelName.ToLower().Contains(cStringUtils.Shift("Dbsm!Xbmufst").ToLower()) Then
                                    db.Execute(cStringUtils.Localize("UPDATE EcopathGroup SET GroupName='{0}' WHERE GroupID=1", cStringUtils.Shift("Dijdlfo!tiju")))
                                    db.Execute(cStringUtils.Localize("UPDATE EcopathFleet SET FleetName='{0}' WHERE FleetID=1", cStringUtils.Shift("Tfbm!cbtifst")))
                                End If
                            Catch ex As Exception
                                ' Do not let eggs make the pot explode
                                cLog.Write("Create DB: found a rotten egg: " & ex.Message)
                            End Try
                            db.Close()
                        Else
                            cLog.Write("Create DB: Unable to open DB using required drivers, check driver installation.")
                        End If
                        db = Nothing
                    Catch ex As Exception
                        cLog.Write(ex)
                        datResult = eDatasourceAccessType.Failed_Unknown
                    End Try
                Else
                    'Unable to write to target location
                    cLog.Write("Create DB: Unable to save to target location " & strDatabase)
                    datResult = eDatasourceAccessType.Failed_CannotSave
                End If
            End If
            Return datResult

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save a given Access database to a new destination, and open this 
        ''' new database.
        ''' </summary>
        ''' <param name="strDatabaseTo">Target database name.</param>
        ''' <param name="strModelName">New name to assign to the model.</param>
        ''' <param name="bOverwrite">States whether any model in the way will be obliterated.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function SaveAs(ByVal strDatabaseTo As String, _
                ByVal strModelName As String, _
                Optional ByVal bOverwrite As Boolean = False, _
                Optional ByVal databaseType As eDataSourceTypes = eDataSourceTypes.NotSet) As eDatasourceAccessType

            Dim datResult As eDatasourceAccessType = eDatasourceAccessType.Success
            Dim strDatabaseFrom As String = Me.Name
            Dim bSucces As Boolean = True

            If databaseType = eDataSourceTypes.NotSet Then
                databaseType = cDataSourceFactory.GetSupportedType(strDatabaseTo)
            End If

            ' Databases are copied from one spot to another, not using proper database replication
            ' Therefore, check if source and target types will remain unchanged
            If databaseType <> cDataSourceFactory.GetSupportedType(strDatabaseFrom) Then
                Return eDatasourceAccessType.Failed_TransferTypes
            End If

            Me.Close()

            ' Test if we can create a new DB at the intended location
            datResult = Me.Create(strDatabaseTo, strModelName, bOverwrite)

            ' Success?
            If (datResult = eDatasourceAccessType.Success) Then

                ' #Yes: this is painful... File Copy the current DB on top of the newly created DB
                Try
                    ' Can copy database from old to new MDB?
                    System.IO.File.Copy(strDatabaseFrom, strDatabaseTo, True)
                Catch ex As Exception
                    ' #Failure
                    datResult = eDatasourceAccessType.Failed_CannotSave
                End Try

                datResult = Me.Open(strDatabaseTo, databaseType)
                'Able to open?
                If datResult = eDatasourceAccessType.Opened Then
                    ' #Yes: Fix model name after copying
                    Me.Execute(cStringUtils.Localize("UPDATE EcopathModel SET NAME='{0}' WHERE (ModelID=1)", strModelName))
                Else
                    ' #No: Open ye olde database
                    Me.Open(strDatabaseFrom)
                End If
            End If
            Return datResult

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Open a connection to a M$ Access database.
        ''' </summary>
        ''' <param name="strDatabase">The database to open.</param>
        ''' <param name="databaseType">Type to use to open the database. Set this
        ''' to 'NotSet' to auto-detect the database type.</param>
        ''' <returns>True if connected successfully.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function Open(ByVal strDatabase As String, _
                                       Optional ByVal databaseType As eDataSourceTypes = eDataSourceTypes.NotSet, _
                                       Optional ByVal bReadOnly As Boolean = False) As eDatasourceAccessType

            ' Preconditions
            Debug.Assert(Not String.IsNullOrEmpty(strDatabase), "Invalid data source specified")
            Debug.Assert(Not Me.IsConnected(), "Connection already open, close first")

            Dim datResult As eDatasourceAccessType = eDatasourceAccessType.Failed_Unknown

            ' Does file exist?
            If Not File.Exists(strDatabase) Then Return eDatasourceAccessType.Failed_FileNotFound

            ' Test read-only file attributes
            If (File.GetAttributes(strDatabase) And FileAttributes.ReadOnly) = FileAttributes.ReadOnly Then
                bReadOnly = True
                ' Return eDatasourceAccessType.Failed_ReadOnly
            End If

            ' Need to auto-detect database type?
            If databaseType = eDataSourceTypes.NotSet Then
                ' #Yes: auto-detect
                databaseType = cDataSourceFactory.GetSupportedType(strDatabase)
            End If

            Me.m_conn = New OleDbConnection()

            ' Try to assemble connection string
            Select Case databaseType
                Case eDataSourceTypes.Access2003
                    Me.m_conn.ConnectionString = cStringUtils.Localize(m_strConnectionMDB, strDatabase)
                Case eDataSourceTypes.Access2007
                    Me.m_conn.ConnectionString = cStringUtils.Localize(m_strConnectionACCDB, strDatabase)
                Case eDataSourceTypes.NotSet
                    Me.m_conn.ConnectionString = ""
                    datResult = eDatasourceAccessType.Failed_UnknownType
            End Select

            If Not String.IsNullOrEmpty(Me.m_conn.ConnectionString) Then

                ' Whoah!
                If bReadOnly Then Me.m_conn.ConnectionString &= "Mode=Read;"
                ' Remember read-only state
                Me.IsReadOnly = bReadOnly

                Try

                    ' Try to open the connection
                    Me.m_conn.Open()
                    ' Set status
                    datResult = eDatasourceAccessType.Opened
                    ' All well: store file name
                    Me.m_strFileName = strDatabase

                Catch ex As OleDbException
                    ' OleDb got into trouble
                    datResult = eDatasourceAccessType.Failed_Unknown
                    cLog.Write(cStringUtils.Localize("Open DB: OleDbException {0}, {1} when opening '{2}'", ex.Message, ex.ErrorCode, Me.m_conn.ConnectionString))

                Catch ex As InvalidOperationException
                    datResult = eDatasourceAccessType.Failed_OSUnsupported
                    cLog.Write(cStringUtils.Localize("Open DB: InvalidOperationException {0} when opening {1}", ex.Message, strDatabase))

                Catch ex As Exception
                    datResult = eDatasourceAccessType.Failed_Unknown
                    cLog.Write(cStringUtils.Localize("Open DB: Exception {0} when opening {1}", ex.Message, strDatabase))

                End Try

            End If

            Return datResult

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Close the current M$ Access connection.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Close()

            ' Preconditions
            Debug.Assert(Me.IsConnected(), "Cannot close a connection that is not open")

            Me.m_conn.Close()
            Me.m_conn.Dispose()
            Me.m_conn = Nothing

            ' Clear file name
            Me.m_strFileName = ""

            MyBase.Close()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of the connected database.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides ReadOnly Property Name() As String
            Get
                Return m_strFileName
            End Get
        End Property

#End Region ' Generic

#Region " Overrides "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Obtains and configures a <see cref="OleDbDataAdapter">OleDbDataAdapter</see>
        ''' for the current M$ Access database.
        ''' </summary>
        ''' <param name="strSQL">The SQL query to obtain the adapter for.</param>
        ''' <returns>A <see cref="OleDbDataAdapter">OleDbDataAdapter</see> if
        ''' successful, or Nothing when an error occurred.</returns>
        ''' <remarks>
        ''' <para>The returned adapter is initialized with default insert, update 
        ''' and delete commands based on the provided query.</para>
        ''' <para>The obtained OleDbDataAdapter should be released via 
        ''' <see cref="ReleaseAdapter">ReleaseAdapter</see>.</para></remarks>
        ''' -------------------------------------------------------------------
        Public Overrides Function GetAdapter(ByVal strSQL As String) As IDataAdapter

            Dim adapter As OleDbDataAdapter = DirectCast(MyBase.GetAdapter(strSQL), OleDbDataAdapter)

            ' Sanity check
            If adapter Is Nothing Then
                Return adapter
            End If

            Dim cmdBuilder As New OleDbCommandBuilder(adapter)

            Try
                adapter.ContinueUpdateOnError = False

                ' Configure adapter
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey
                adapter.InsertCommand = cmdBuilder.GetInsertCommand(True)
                adapter.UpdateCommand = cmdBuilder.GetUpdateCommand(True)
                adapter.DeleteCommand = cmdBuilder.GetDeleteCommand(True)

            Catch ex As InvalidOperationException
                cLog.Write(cStringUtils.Localize("Table in query '{0}' seems to be missing a primary key: {1}", strSQL, ex.Message))
                Debug.Assert(False, cStringUtils.Localize("Table in query '{0}' seems to be missing a primary key: {1}", strSQL, ex.Message))
                adapter = Nothing

            Catch ex As Exception
                cLog.Write(cStringUtils.Localize("Error when opening adapter for query {0}: {1}", strSQL, ex.Message))
                Debug.Assert(False, cStringUtils.Localize("Error when opening adapter for query {0}: {1}", strSQL, ex.Message))
                adapter = Nothing
            End Try

            Return adapter

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the current M$ Access database connection.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Function GetConnection() As IDbConnection
            Return Me.m_conn
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the database can connect to an indicated type.
        ''' </summary>
        ''' <param name="dst">The data source type to test.</param>
        ''' <returns>True if the OS can connect to a given data source type.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function CanConnect(ByVal dst As EwEUtils.Core.eDataSourceTypes) As Boolean

            Dim conn As OleDbConnection = New OleDbConnection()
            Dim strDatabase As String = "~doesnotexist~"
            Dim datResult As eDatasourceAccessType = eDatasourceAccessType.Opened

            ' Try to assemble connection string
            Select Case dst
                Case eDataSourceTypes.Access2003
                    conn.ConnectionString = cStringUtils.Localize(m_strConnectionMDB, strDatabase)
                Case eDataSourceTypes.Access2007
                    conn.ConnectionString = cStringUtils.Localize(m_strConnectionACCDB, strDatabase)
                Case Else
                    conn.ConnectionString = ""
                    datResult = eDatasourceAccessType.Failed_UnknownType
            End Select

            If Not String.IsNullOrWhiteSpace(conn.ConnectionString) Then
                Try
                    conn.Open()
                    conn.Close() ' Can't be, but hey
                Catch ex As InvalidOperationException
                    datResult = eDatasourceAccessType.Failed_OSUnsupported
                Catch ex As OleDbException
                    ' At least the OleDB drivers were loaded successfully...
                    datResult = eDatasourceAccessType.Success
                Catch ex As Exception
                    ' Hmm
                End Try
            End If

            Return (datResult <> eDatasourceAccessType.Failed_OSUnsupported)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns if the compact database engine is available.
        ''' </summary>
        ''' <param name="strConnectionFrom"></param>
        ''' <param name="strConnectionTo"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function CanCompact(ByVal strConnectionFrom As String, ByVal strConnectionTo As String) As Boolean

            Dim compact As IDatabaseCompact = cDatabaseCompactFactory.GetDatabaseCompact(strConnectionFrom)
            If (compact Is Nothing) Then Return False
            Return compact.CanCompact()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Compact the current M$ Access database.
        ''' </summary>
        ''' <param name="strFileFrom">Source database to compact.</param>
        ''' <param name="strFileTo">Target database to compact to. Can be left blank.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Only MDB databases can be compacted for now. Note that the database
        ''' cannot be <see cref="IsConnected">connected</see> when compacting.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overrides Function Compact(ByVal strFileFrom As String, _
                                          ByVal strFileTo As String) As eDatasourceAccessType

            '  If read-only: report only status and abort
            If (Me.IsReadOnly) Then Return eDatasourceAccessType.Failed_ReadOnly

            ' Fix parameters
            If String.IsNullOrEmpty(strFileTo) Then strFileTo = strFileFrom

            Dim bCompactToOriginal As Boolean = (String.Compare(strFileFrom, strFileTo, True) = 0)
            Dim strConnectionFrom As String = ""
            Dim strConnectionTo As String = ""
            Dim strConnection As String = ""
            Dim result As eDatasourceAccessType = eDatasourceAccessType.Success
            Dim comp As IDatabaseCompact = cDatabaseCompactFactory.GetDatabaseCompact(strFileFrom)

            Select Case cDataSourceFactory.GetSupportedType(strFileFrom)
                Case eDataSourceTypes.Access2003
                    strConnection = Me.m_strConnectionMDB
                Case eDataSourceTypes.Access2007
                    ' Accdb needs different compaction engine, no idea how to do that for now
                    strConnection = Me.m_strConnectionACCDB
                Case Else
                    ' Not supported
                    strConnection = ""
            End Select

            ' No connection string for compacting this type of database?
            If String.IsNullOrEmpty(strConnection) Then Return eDatasourceAccessType.Failed_OSUnsupported
            ' Cannot compact when connected
            Debug.Assert(Me.IsConnected = False)

            Try

                ' #Yes: try to compact
                Dim strDBToOrg As String = strFileTo
                ' Identical database specified for in and out?
                If (bCompactToOriginal) Then
                    ' #Yes: compact DB to temp location
                    strFileTo = System.IO.Path.GetTempFileName()
                End If

                If File.Exists(strFileTo) Then
                    Try
                        File.Delete(strFileTo)
                    Catch ex As Exception
                        Return eDatasourceAccessType.Failed_CannotSave
                    End Try
                End If

                ' Set up connections
                strConnectionFrom = cStringUtils.Localize(strConnection, strFileFrom)
                strConnectionTo = cStringUtils.Localize(strConnection, strFileTo)

                ' Attempt to Compact
                result = comp.Compact(strFileFrom, strConnectionFrom, strFileTo, strConnectionTo)
                ' Is successful?
                If (result <> eDatasourceAccessType.Success) Then
                    ' #No: report compact failure
                    Return result
                End If

                ' Is successfully compacted?
                If File.Exists(strFileTo) Then
                    ' #Yes: Need to copy from temp location?
                    If (bCompactToOriginal) Then
                        ' #Yes: Overwrite original db with compacted db
                        File.Copy(strFileTo, strDBToOrg, True)
                        ' Delete temp location compacted database
                        File.Delete(strFileTo)
                    End If
                End If
                Return eDatasourceAccessType.Success

            Catch ex As Exception
                ' Wow
            End Try

            Return eDatasourceAccessType.Failed_Unknown

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cEwEDatabase.MaxDBVersion"/>
        ''' -------------------------------------------------------------------
        Public Overrides Function MaxDBVersion() As Single
            Return cDatabaseUpdater.MaxSupportedVersion
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cEwEDatabase.Directory"/>
        ''' -------------------------------------------------------------------
        Public Overrides ReadOnly Property Directory() As String
            Get
                Return Path.GetDirectoryName(Me.m_strFileName)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cEwEDatabase.FileName"/>
        ''' -------------------------------------------------------------------
        Public Overrides ReadOnly Property FileName() As String
            Get
                Return Path.GetFileNameWithoutExtension(Me.m_strFileName)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cEwEDatabase.Extension"/>
        ''' -------------------------------------------------------------------
        Public Overrides ReadOnly Property Extension() As String
            Get
                Return Path.GetExtension(Me.m_strFileName)
            End Get
        End Property

#End Region ' Overrides

    End Class

End Namespace
