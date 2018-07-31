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

Imports System
Imports System.Diagnostics
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Data.Common
Imports System.Reflection
Imports System.ComponentModel
Imports System.Collections.Generic
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

#If VERBOSE Then
#Const VERBOSE_LEVEL = 4
#End If

Namespace Database

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Generic base class for implementing a DBMS-specific EwE database
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public MustInherit Class cEwEDatabase

#Region " Class cEwEDbWriter "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper class that eases the process of adding records to a table.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Class cEwEDbWriter
            Implements IDisposable

            ''' <summary>Database to write to</summary>
            Private m_db As cEwEDatabase = Nothing
            ''' <summary>Table in the database to write to</summary>
            Private m_strTable As String = ""
            ''' <summary>DataSet contains a mirror of the indicated table</summary>
            Private m_ds As DataSet = Nothing
            ''' <summary>DataTable that mirrors the indicated table</summary>
            Private m_dt As DataTable = Nothing
            ''' <summary>Adapter to sync table content back and forth</summary>
            Private m_apt As IDataAdapter = Nothing

            Private m_dtSchema As DataTable = Nothing
            Private m_bDisposed As Boolean = False

#If DEBUG Then
            Private m_ID As Integer = 0
            Private Shared s_IDnext As Integer = 1
#End If

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' <para>Constructor, initializes a new instance of a cEwEDbWriter.</para>
            ''' </summary>
            ''' <param name="db">The <see cref="cEwEDatabase">cEwEDatabase</see> to read from.</param>
            ''' <param name="strTable">The name of the table to link to.</param>
            ''' <remarks>
            ''' <para>This method will attempt to connect and read the table into its internal
            ''' structures. It might be prudent to validate whether the instance is connected
            ''' by calling <see cref="IsConnected">IsConnected</see> prior to using it.</para>
            ''' </remarks>
            ''' ---------------------------------------------------------------
            Public Sub New(ByRef db As cEwEDatabase, ByVal strTable As String)
#If DEBUG Then
                Me.m_ID = s_IDnext
                s_IDnext += 1
#End If
#If VERBOSE_LEVEL > 2 Then
                Debug.WriteLine("DB writer " & Me.m_ID & " created(" & strTable & ")")
#End If
                Me.Connect(db, strTable)
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Trash me!
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Sub Dispose() Implements IDisposable.Dispose
                If (Me.m_bDisposed = False) Then
                    Me.m_bDisposed = True
#If DEBUG Then
                    Debug.WriteLine("DB writer " & Me.m_ID & " disposed")
#End If
                    If Me.IsConnected Then Me.Disconnect(True)
                End If
                GC.SuppressFinalize(Me)
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Attempts to connect to the database and read the table data.
            ''' </summary>
            ''' <param name="db">The <see cref="cEwEDatabase">cEwEDatabase</see> to read from.</param>
            ''' <param name="strTable">The name of the table to link to.</param>
            ''' <returns>True if connected.</returns>
            ''' ---------------------------------------------------------------
            Public Function Connect(ByRef db As cEwEDatabase, ByVal strTable As String) As Boolean

                ' Pre
                Debug.Assert(db IsNot Nothing, "Need a valid database")
                Debug.Assert(Not String.IsNullOrEmpty(strTable), "Need a table name")

                Dim conn As IDbConnection = db.GetConnection()

                ' Remember these
                Me.m_db = db
                Me.m_strTable = strTable
                Me.m_dtSchema = Nothing

                ' OLEDB hack
                If TypeOf conn Is OleDbConnection Then
                    Me.m_dtSchema = DirectCast(conn, OleDbConnection).GetSchema("Columns", New String() {Nothing, Nothing, strTable, Nothing})
                End If

                ' Get adapter
                Me.m_apt = Me.m_db.GetAdapter(String.Format("Select * from {0}", strTable))
                ' Adapter gotten succesfully?
                If (Me.m_apt IsNot Nothing) Then
                    ' #Yes: Get dataset
                    Me.m_ds = Me.m_db.GetDataSet(Me.m_apt, strTable)
                    ' Dataset obtained succesfully?
                    If (Me.m_ds IsNot Nothing) Then
                        ' #Yes: read the data
                        Me.m_apt.Fill(Me.m_ds)
                        ' Set up DataTable for making modifications
                        Me.m_dt = Me.m_ds.Tables(0)
                    Else
                        ' #No: dataset failed, release adapter
                        Me.m_db.ReleaseAdapter(Me.m_apt)
                        Me.m_apt = Nothing
                        ' Release the rest as well, why not
                        Me.m_db = Nothing
                        Me.m_strTable = ""
                    End If
                End If
                ' Return connection state
                Return Me.IsConnected()

            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Commit all pending changes in the cEwEDBWriter without closing
            ''' the writer; the writer is left open for further database operations.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Function Commit() As Boolean

                ' Optimizations
                If Not Me.IsConnected Then Return False
                If Not Me.m_ds.HasChanges() Then Return True

                If Me.m_dtSchema IsNot Nothing Then
                    ' Fix unwanted nulls in new and modified rows
                    Dim adrows() As DataRow = Me.m_dt.Select()
                    For Each drow As DataRow In adrows
                        If drow.RowState = DataRowState.Added Or drow.RowState = DataRowState.Modified Then
                            Me.FixUnwantedDBNulls(drow)
                        End If
                    Next
                End If
                Return Me.m_db.CommitDataSet(Me.m_ds, Me.m_apt, Me.m_strTable)

            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Disconnects the cEwEDBWriter from the database.
            ''' </summary>
            ''' <param name="bSaveChanges">States whether changes need to be saved (true)
            ''' or discarded (false).</param>
            ''' ---------------------------------------------------------------
            Public Function Disconnect(Optional ByVal bSaveChanges As Boolean = True) As Boolean

                Dim bSucces As Boolean = False
                If Not Me.IsConnected Then Return bSucces

                If bSaveChanges Then
                    bSucces = Me.Commit()
                End If

                bSucces = bSucces And Me.m_db.ReleaseDataSet(Me.m_ds)
                bSucces = bSucces And Me.m_db.ReleaseAdapter(Me.m_apt)

                Me.m_dt = Nothing
                Me.m_ds = Nothing
                Me.m_apt = Nothing
                Me.m_db = Nothing
                Me.m_strTable = ""

#If VERBOSE_LEVEL > 2 Then
                Debug.WriteLine("DB writer " & Me.m_ID & " disconnected")
#End If
                Return bSucces
            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Returns whether the cEwEDBWriter is currently <see cref="Connect">connected</see>.
            ''' </summary>
            ''' <returns>True if connected.</returns>
            ''' ---------------------------------------------------------------
            Public Function IsConnected() As Boolean
                Return Me.m_apt IsNot Nothing
            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Returns an empty row for the given table to populate values into.
            ''' </summary>
            ''' <returns>An empty row</returns>
            ''' <remarks>Note that this empty row is not yet added to the table. 
            ''' If the row is populated to satisfaction, call <see cref="AddRow">AddRow</see>
            ''' to add it to the the list of rows waiting to be added to the database.</remarks>
            ''' ---------------------------------------------------------------
            Public Function NewRow() As DataRow
                Try
                    Return Me.m_dt.NewRow()
                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                    Return Nothing
                End Try
            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Adds a row previously obtained from <see cref="NewRow">NewRow</see>
            ''' to the list of rows waiting to be added to the database.
            ''' </summary>
            ''' <remarks>
            ''' <para>This method will preserve and re-align a sequence field if specified
            ''' in the <see cref="cEwEDbWriter">Constructor</see>.</para>
            ''' <para>Use <see cref="cEwEDbWriter.RemoveRow">RemoveRow</see> to protect the
            ''' row sequence during deletes.</para>
            ''' </remarks>
            ''' ---------------------------------------------------------------
            Public Sub AddRow(ByVal drow As DataRow)
                'Me.FixStringLengths(drow)
                Me.m_dt.Rows.Add(drow)
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Returns an arbitrary row maintained in the writer.
            ''' </summary>
            ''' <param name="drow">The datarow to delete.</param>
            ''' <returns>The row.</returns>
            ''' <remarks>
            ''' <para>This method will preserve and re-align a sequence field if specified
            ''' in the <see cref="cEwEDbWriter">Constructor</see>.</para>
            ''' <para>Use <see cref="cEwEDbWriter.AddRow">AddRow</see> to protect the
            ''' row sequence during additions.</para>
            ''' </remarks>
            ''' ---------------------------------------------------------------
            Public Function RemoveRow(ByVal drow As DataRow) As Boolean
                Me.m_dt.Rows.Remove(drow)
                Return True
            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Returns an arbitrary row maintained in the writer
            ''' </summary>
            ''' <param name="nRow">The row number to retrieve</param>
            ''' <returns>The row</returns>
            ''' <remarks>This method might not be necessary?</remarks>
            ''' ---------------------------------------------------------------
            Public Function GetRow(ByVal nRow As Integer) As DataRow
                Return Me.m_dt.Rows(nRow)
            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Overridden to close the cEwEDbWriter if not already closed.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Protected Overrides Sub Finalize()
#If VERBOSE_LEVEL > 2 Then
                Debug.WriteLine("DB writer " & Me.m_ID & " finalized")
#End If
                ' JS 18May16: Disconnecting upon disposal is very likely to fail,
                ' because the underlying transaction has probably already perished.
                ' Instead, users should explicitly Disconnect
                Debug.Assert(Not Me.IsConnected(), "Database adapter for " & Me.m_strTable & " not explicitly released!")

                ' Changes may get lost here, which is the consequence of not properly releasing
                If Me.IsConnected Then
                    Me.Disconnect(Me.m_db.Transaction IsNot Nothing)
                End If

                MyBase.Finalize()
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get a reference to the DataTable for the current writer
            ''' </summary>
            ''' <returns></returns>
            ''' ---------------------------------------------------------------
            Public Function GetDataTable() As DataTable
                Return Me.m_dt
            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Helper method; replaces DBNull values that are specified as not 
            ''' Nullable in the underlying Access database schema with the default 
            ''' value in the schema.
            ''' </summary>
            ''' <param name="drow">The row to fix.</param>
            ''' ---------------------------------------------------------------
            Private Sub FixUnwantedDBNulls(ByRef drow As DataRow)

                Dim bIsValueNull As Boolean = False
                Dim bIsNullable As Boolean = False
                Dim bHasDefault As Boolean = False
                Dim columnDataType As Data.OleDb.OleDbType = OleDbType.IUnknown
                Dim strColumnName As String = ""

                For Each drowSchema As DataRow In Me.m_dtSchema.Rows
                    strColumnName = CStr(drowSchema("COLUMN_NAME"))

                    bIsValueNull = drow.IsNull(strColumnName)
                    bIsNullable = CBool(drowSchema("IS_NULLABLE"))
                    bHasDefault = CBool(drowSchema("COLUMN_HASDEFAULT"))

                    If bIsValueNull And Not bIsNullable Then

                        ' Set default using common data type conversions to bypass language-specific
                        ' problems caused by misinterpreted decimal separators, etc

                        ' Get column data type
                        columnDataType = CType(drowSchema("DATA_TYPE"), Data.OleDb.OleDbType)

                        ' Convert defaults for common data types. Add others when needed.
                        Select Case columnDataType

                            Case OleDbType.WChar
                                If (bHasDefault) Then
                                    ' Get default value for this column (it's a string, regardless of column datatype. Brilliant)
                                    ' Access weirdness: fix double quotes problems
                                    drow(strColumnName) = CStr(drowSchema("COLUMN_DEFAULT")).Replace("""", "")
                                Else
                                    drow(strColumnName) = String.Empty
                                End If

                            Case OleDbType.Boolean
                                If (bHasDefault) Then
                                    ' Get default value for this column (it's a string, regardless of column datatype. Brilliant)
                                    drow(strColumnName) = Boolean.Parse(CStr(drowSchema("COLUMN_DEFAULT")))
                                Else
                                    drow(strColumnName) = False
                                End If

                            Case OleDbType.SmallInt
                                If (bHasDefault) Then
                                    drow(strColumnName) = CType(CStr(drowSchema("COLUMN_DEFAULT")), Int16)
                                Else
                                    drow(strColumnName) = 0
                                End If

                            Case OleDbType.Integer
                                If (bHasDefault) Then
                                    drow(strColumnName) = CInt(CStr(drowSchema("COLUMN_DEFAULT")))
                                Else
                                    drow(strColumnName) = 0
                                End If

                            Case OleDbType.Single
                                Try
                                    If bHasDefault Then
                                        drow(strColumnName) = cStringUtils.ConvertToSingle(CStr(drowSchema("COLUMN_DEFAULT")), 0.0!)
                                    Else
                                        drow(strColumnName) = 0.0!
                                    End If
                                Catch ex As Exception
                                    drow(strColumnName) = 0.0!
                                End Try

                            Case OleDbType.Double
                                Try
                                    If bHasDefault Then
                                        drow(strColumnName) = cStringUtils.ConvertToDouble(CStr(drowSchema("COLUMN_DEFAULT")), 0.0#)
                                    Else
                                        drow(strColumnName) = 0.0#
                                    End If
                                Catch ex As Exception
                                    Debug.Assert(False)
                                    drow(strColumnName) = 0.0#
                                End Try

                            Case OleDbType.Currency
                                ' ToDo_JS: Consider what to do here; test possible issues across locales
                                Debug.Assert(False, "Currency defaults not properly supported in the EwE database logic")

                            Case Else
                                ' Unexpected datatype encountered
#If VERBOSE_LEVEL >= 2 Then
                                    Console.WriteLine("   - Default {0} for column {1}: unexpected datatype {2}", drow(strColumnName), strColumnName, columnDataType.ToString())
#End If
                                ' Set the default and hope for the best
                                If (bHasDefault) Then
                                    drow(strColumnName) = CStr(drowSchema("COLUMN_DEFAULT"))
                                End If

                        End Select
                    End If
                Next
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Helper method; replaces DBNull values that are specified as not 
            ''' nullable in the underlying Access database schema with the default 
            ''' value in the schema.
            ''' </summary>
            ''' <param name="drow">The row to fix.</param>
            ''' ---------------------------------------------------------------
            Private Sub FixStringLengths(ByVal drow As DataRow)

                Debug.Assert(False, "This code does not work. iMaxLen will always be unknown due to a bug in Access according to MSDN")
                Dim columnDataType As Data.OleDb.OleDbType = OleDbType.IUnknown
                Dim strColumnName As String = ""
                Dim iMaxLen As Integer = 0
                Dim iLen As Integer = 0

                For Each drowSchema As DataRow In Me.m_dtSchema.Rows

                    strColumnName = CStr(drowSchema("COLUMN_NAME"))
                    columnDataType = CType(drowSchema("DATA_TYPE"), Data.OleDb.OleDbType)

                    Select Case columnDataType
                        Case OleDbType.WChar, OleDbType.VarWChar, OleDbType.LongVarChar
                            Dim strVal As String = CStr(drow(strColumnName))

                            iMaxLen = CInt(drowSchema("CHARACTER_MAXIMUM_LENGTH"))
                            iLen = strVal.Length

                            If (iLen > iMaxLen) Then
                                drow(strColumnName) = strVal.Substring(0, iMaxLen)
                            End If

                    End Select
                Next
            End Sub

        End Class

#End Region ' Class cEwEDbWriter

#Region " Private vars and constants "

        ''' <summary>Current database version.</summary>
        Private m_sVersion As Single = 0.0
        ''' <summary>Database read-only state.</summary>
        Private m_bIsReadonly As Boolean = False
        ''' <summary>Directory associated with the database.</summary>
        Private m_strDirectory As String = ""

        ''' <summary>Oldest EwE5 version number supported</summary>
        Private Const cDBVERSION_EWE5_MIN As Single = 1.6!
        ''' <summary>Newest EwE5 version number supported</summary>
        Private Const cDBVERSION_EWE5_MAX As Single = 1.73!

#End Region ' Private vars and constants

#Region " Open and close "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new database.
        ''' </summary>
        ''' <param name="strDatabase">The database to create.</param>
        ''' <param name="strModelName">Name of the new model.</param>
        ''' <param name="strAuthor">Name of the author to add. May be omitted.</param>
        ''' <param name="databaseType">Type of the database to create. May be omitted.</param>
        ''' <param name="bOverwrite">States whether an existing database may be overwritten.</param>
        ''' <returns>True of created succesfully.</returns>
        ''' <remarks>Note that this will NOT open the newly created database.</remarks>
        ''' -------------------------------------------------------------------
        Public MustOverride Function Create(ByVal strDatabase As String,
                ByVal strModelName As String,
                Optional ByVal bOverwrite As Boolean = False,
                Optional ByVal databaseType As eDataSourceTypes = eDataSourceTypes.NotSet,
                Optional strAuthor As String = "") As eDatasourceAccessType

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Open a connection to a database.
        ''' </summary>
        ''' <param name="strDatabase">The database to open.</param>
        ''' <param name="databaseType">Type to use to open the database. Set this
        ''' to 'NotSet' to auto-detect the database type.</param>
        ''' <returns>True if connected succesfully.</returns>
        ''' -------------------------------------------------------------------
        Public MustOverride Function Open(ByVal strDatabase As String,
                                          Optional ByVal databaseType As eDataSourceTypes = eDataSourceTypes.NotSet,
                                          Optional ByVal bReadOnly As Boolean = False) As eDatasourceAccessType

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Close an open connection.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Close()
            Me.m_sVersion = 0.0!
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of the connected database.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public MustOverride ReadOnly Property Name() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save a given database to a new destination, and open this new database.
        ''' </summary>
        ''' <param name="strDatabaseTo">Target database name.</param>
        ''' <param name="strModelName">New name to assign to the model.</param>
        ''' <param name="bOverwrite">States whether any model in the way will be obliterated.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public MustOverride Function SaveAs(ByVal strDatabaseTo As String,
                ByVal strModelName As String,
                Optional ByVal bOverwrite As Boolean = False,
                Optional ByVal databaseType As eDataSourceTypes = eDataSourceTypes.NotSet) As eDatasourceAccessType

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the database is read-only.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property IsReadOnly() As Boolean
            Get
                Return Me.m_bIsReadonly
            End Get
            Protected Set(ByVal value As Boolean)
                Me.m_bIsReadonly = value
            End Set
        End Property

#End Region ' Open and close

#Region " Compatibility "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enumberated type, used to indicate database compatibility modes
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eCompatibilityTypes As Integer
            ''' <summary>Combatibility unknown. Most likely the accessed file is not an EwE database.</summary>
            Unknown = 0
            ''' <summary>An older database, too old to be imported.</summary>
            TooOld
            ''' <summary>An older database that can be imported.</summary>
            Importable
            ''' <summary>EwE6 database that is supported.</summary>
            EwE6
            ''' <summary>A database that is of a newer format and is not supported.</summary>
            Future
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States the level of compatibility with the EwE code.
        ''' </summary>
        ''' <returns>A <see cref="eCompatibilityTypes">database compatibility</see>
        ''' indicator.</returns>
        ''' -------------------------------------------------------------------
        Public Function Compatibility() As eCompatibilityTypes
            Dim sVersion As Single = Me.GetVersion()
            If (sVersion = 0.0!) Then Return eCompatibilityTypes.Unknown
            If (sVersion < cDBVERSION_EWE5_MIN) Then Return eCompatibilityTypes.TooOld
            If (sVersion <= cDBVERSION_EWE5_MAX) Then Return eCompatibilityTypes.Importable
            If (sVersion <= Me.MaxDBVersion) Then Return eCompatibilityTypes.EwE6
            Return eCompatibilityTypes.Future
        End Function

        Public MustOverride Function MaxDBVersion() As Single

#End Region ' Compatibility

#Region " Maintenance "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Compact an EwE database.
        ''' </summary>
        ''' <param name="strFileFrom">Source database to compact.</param>
        ''' <param name="strFileTo">Target database to compact to. If left 
        ''' blank, the source database is replaced with a compacted version.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public MustOverride Function Compact(ByVal strFileFrom As String,
                                             ByVal strFileTo As String) As eDatasourceAccessType

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns if a compact engine is available for the underlying database.
        ''' </summary>
        ''' <param name="strConnectionFrom">Compact source.</param>
        ''' <param name="strConnectionTo">Compact target.</param>
        ''' <returns>True if a compact engine is available for the underlying 
        ''' database.</returns>
        ''' -------------------------------------------------------------------
        Public MustOverride Function CanCompact(ByVal strConnectionFrom As String,
                                                ByVal strConnectionTo As String) As Boolean

#End Region ' Maintenance

#Region " Connection "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the current database connection.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public MustOverride Function GetConnection() As IDbConnection

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether there is a database connection that is open.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function IsConnected() As Boolean
            Dim conn As IDbConnection = Me.GetConnection()

            If (conn Is Nothing) Then Return False
            Return (conn.State = ConnectionState.Open)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the database can connect to an indicated type.
        ''' </summary>
        ''' <param name="dst">The datasource type to test.</param>
        ''' <returns>True if the OS can connect to a given datasource type.</returns>
        ''' -------------------------------------------------------------------
        Public MustOverride Function CanConnect(ByVal dst As eDataSourceTypes) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a directory associated with this database.
        ''' </summary>
        ''' <seealso cref="Extension"/>
        ''' <seealso cref="FileName"/>
        ''' -------------------------------------------------------------------
        Public MustOverride ReadOnly Property Directory() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a file name associated with this database, excluding the
        ''' file extension.
        ''' </summary>
        ''' <seealso cref="Extension"/>
        ''' <seealso cref="Directory"/>
        ''' -------------------------------------------------------------------
        Public MustOverride ReadOnly Property FileName() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a file extension associated with this database.
        ''' </summary>
        ''' <seealso cref="FileName"/>
        ''' <seealso cref="Directory"/>
        ''' -------------------------------------------------------------------
        Public MustOverride ReadOnly Property Extension() As String

#End Region ' Connection

#Region " Transaction "

        ''' <summary>The current transaction, if any.</summary>
        Private m_transaction As IDbTransaction = Nothing

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Begins a transaction for the current <see cref="GetConnection">Connection</see>.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' <remarks>19may07: status experimental</remarks>
        ''' -------------------------------------------------------------------
        Public Function BeginTransaction() As Boolean
            If Not (Me.m_transaction Is Nothing) Then Return False
            Try
                Me.m_transaction = Me.GetConnection.BeginTransaction()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Commits a transaction previously initiated via <see cref="BeginTransaction">BeginTransaction</see>.
        ''' </summary>
        ''' <param name="bRollbackOnError">Flag stating whether the transaction needs
        ''' to automatically rollback when the commit process fails.</param>
        ''' <returns>True if the commit operation succeeded.</returns>
        ''' -------------------------------------------------------------------
        Public Function CommitTransaction(Optional ByVal bRollbackOnError As Boolean = True) As Boolean
            If (Me.m_transaction Is Nothing) Then Return False
            Try
                Me.m_transaction.Commit()
                Me.m_transaction = Nothing
                Return True
            Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("cEwEDatabase: Transaction commit failed: {0}", ex.Message)
#End If
                If (bRollbackOnError) Then Me.RollbackTransaction()
            End Try
            Return False
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Commits a transaction to the current <see cref="GetConnection">Connection</see>.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function RollbackTransaction() As Boolean
            Try
                Me.m_transaction.Rollback()
                Me.m_transaction = Nothing
                Return True
            Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("cEwEDatabase: Transaction rollback failed: {0}", ex.Message)
#End If
                Return False
            End Try
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; internally exposes the current active transaction.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Function Transaction() As IDbTransaction
            Return Me.m_transaction
        End Function

#End Region ' Transaction

#Region " DB helper methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns an <see cref="IDbCommand"/> for the current DBMS
        ''' </summary>
        ''' <param name="strSQL">Query to create the IDbCommand with.</param>
        ''' <returns>Nothing if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function CreateDBCommand(ByVal strSQL As String) As IDbCommand

            Dim conn As IDbConnection = Me.GetConnection()
            Dim cmd As IDbCommand = Nothing

            Try
                If TypeOf conn Is OleDbConnection Then
                    cmd = New OleDbCommand(strSQL, DirectCast(conn, OleDbConnection), DirectCast(Me.Transaction(), OleDbTransaction))
                Else
                    cmd = New SqlCommand(strSQL, DirectCast(conn, SqlConnection), DirectCast(Me.Transaction(), SqlTransaction))
                End If
                Return cmd
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return Nothing
            End Try

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a <see cref="IDataReader"/> with a collection
        ''' of readonly records from the currently open connection.
        ''' <seealso cref="ReleaseReader"/>
        ''' </summary>
        ''' <param name="strSQL">The query to obtain the records.</param>
        ''' <returns></returns>
        ''' <remarks>The obtained IDataReader should be released via <see cref="ReleaseReader"/>.</remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Function GetReader(ByVal strSQL As String) As IDataReader

            Dim reader As IDataReader = Nothing
            Try
                Using command As IDbCommand = Me.CreateDBCommand(strSQL)
                    reader = command.ExecuteReader()
                End Using
            Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("GetReader error: {0}", ex.Message)
#End If
                cLog.Write(ex, eVerboseLevel.Detailed, "cEwEDatabase.GetReader(" & strSQL & ")")
                reader = Nothing
            End Try
            Return reader

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Releases the set of readonly records previously obtained by calling
        ''' <see cref="GetReader"/>.
        ''' <seealso cref="GetReader"/>
        ''' </summary>
        ''' <param name="reader">The <see cref="IDataReader"/> to release.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function ReleaseReader(ByVal reader As IDataReader) As Boolean
            Try
                reader.Close()
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".ReleaseReader() Error: " & ex.Message)
                Return False
            End Try
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a <see cref="cEwEDbWriter"/> for
        ''' the given table in the database.
        ''' <seealso cref="ReleaseWriter"/>
        ''' </summary>
        ''' <param name="strTable">The table to connect the EwEDbWriter to.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Function GetWriter(ByVal strTable As String) As cEwEDbWriter
            Return New cEwEDbWriter(Me, strTable)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Releases a writer previously created via <see cref="GetWriter"/>.
        ''' <seealso cref="GetWriter"/>
        ''' </summary>
        ''' <param name="writer">The writer to release</param>
        ''' <param name="bSaveChanges">States whether changes should be written (true) or discarded (false).</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function ReleaseWriter(ByRef writer As cEwEDbWriter, Optional ByVal bSaveChanges As Boolean = True) As Boolean
            Dim bSuccess As Boolean = writer.Disconnect(bSaveChanges)
            writer.Dispose()
            Return bSuccess
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a scalar value from the current open connection.
        ''' </summary>
        ''' <param name="strSQL">The query to execute.</param>
        ''' <param name="objDefault">A default to return in case a value could not be returned.</param>
        ''' <returns>The scalar value returned from the query.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function GetValue(ByVal strSQL As String, Optional objDefault As Object = Nothing) As Object

            Dim value As Object = objDefault
            Try
                Using command As IDbCommand = Me.CreateDBCommand(strSQL)
                    value = command.ExecuteScalar()
                    If Convert.IsDBNull(value) Then
                        value = objDefault
                    End If
                End Using
            Catch ex As Exception
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine("** DB error '{0}' on query '{1}'", ex.Message, strSQL)
#End If
                cLog.Write(ex, eVerboseLevel.Detailed, "cEwEDatabase.GetValue(" & strSQL & ")")
            End Try
            Return value
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Obtains an <see cref="IDataAdapter"/> for the current open connection.
        ''' <seealso cref="ReleaseAdapter"/>
        ''' </summary>
        ''' <param name="strSQL">The SQL query to obtain the adaper for.</param>
        ''' <returns>An <see cref="IDataAdapter"/> if successful, or Nothing if 
        ''' an error occurred.</returns>
        ''' <remarks>
        ''' <para>The obtained IDataAdapter should be released via 
        ''' <see cref="ReleaseAdapter"/>.</para></remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Function GetAdapter(ByVal strSQL As String) As IDataAdapter

            Dim cmd As IDbCommand = Me.CreateDBCommand(strSQL)
            Try
                If TypeOf cmd Is OleDbCommand Then
                    Return New OleDbDataAdapter(DirectCast(cmd, OleDbCommand))
                Else
                    Return New SqlDataAdapter(DirectCast(cmd, SqlCommand))
                End If
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

            Return Nothing

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Releases an <see cref="IDataAdapter"/> previously obtained via
        ''' <see cref="GetAdapter"/>.
        ''' </summary>
        ''' <param name="adapter">The <see cref="IDataAdapter"/> to release.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function ReleaseAdapter(ByRef adapter As IDataAdapter) As Boolean
            ' Nothing to do
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Executes a SQL command that does not return any information.
        ''' </summary>
        ''' <param name="strSQL">The query to execute.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function Execute(ByVal strSQL As String) As Boolean

            Dim bSucces As Boolean = True
            Try
                Using command As IDbCommand = Me.CreateDBCommand(strSQL)
                    command.ExecuteNonQuery()
                End Using
            Catch ex As Exception
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine("* DB exception '{0}' on '{1}'", ex.Message, strSQL)
#End If
                cLog.Write(ex, eVerboseLevel.Detailed, "cEwEDatabase.Execute(" & strSQL & ")")
                bSucces = False
            End Try
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the name of a primary key of a table.
        ''' </summary>
        ''' <param name="strTable">The table name to obtain the primary key for.</param>
        ''' <returns>A name, or an empty string when no primary key was found.</returns>
        ''' <remarks>
        ''' After http://www.koders.com/csharp/fidE6A0EFDE719732D025C3D41E95CC26214E50188C.aspx
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Function GetPkKeyName(ByVal strTable As String) As String

            Dim conn As IDbConnection = Me.GetConnection()
            Dim dtKeys As DataTable = Nothing
            Dim strPKKey As String = ""

            ' Execute oledb variant
            If (TypeOf conn Is OleDbConnection) Then

                Dim cdb As OleDbConnection = DirectCast(conn, OleDbConnection)
                ' Get PK keys schema information for entire DB
                dtKeys = cdb.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, New Object() {Nothing, Nothing, strTable})
                ' Sanity checks, pk may not be defined
                If (dtKeys.Rows.Count = 0) Then Return strPKKey
                ' Return whatever was found
                '   JS: could be that 'COLUMN_NAME' should be used here!!
                strPKKey = CStr(dtKeys.Rows(0)("PK_NAME"))
                'strPKKey = CStr(drKeys(0)("COLUMN_NAME"))
            End If

            If (TypeOf conn Is SqlConnection) Then
                ' Not implemented yet
                Throw New NotImplementedException("cEwEDatabase.GetPkKeyName() not implemented for SqlConnections")
            End If

            Return strPKKey

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the name of a foreign key between a given column in a table to 
        ''' another table.
        ''' </summary>
        ''' <param name="strTableFrom">The table where the foreign key is defined.</param>
        ''' <param name="strColumn">The column in the <paramref name="strTableFrom">source table</paramref>.</param>
        ''' <param name="strTableTo">The table where the foreign key links to.</param>
        ''' <returns>A name, or an empty string when no foreign key was found.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function GetFkKeyName(ByVal strTableFrom As String,
                                                 ByVal strTableTo As String,
                                                 ByVal strColumn As String) As String

            Dim conn As IDbConnection = Me.GetConnection()
            Dim dtKeys As DataTable = Nothing
            Dim strFKKey As String = ""

            ' Execute oledb variant
            If (TypeOf conn Is OleDbConnection) Then

                Try
                    Dim cdb As OleDbConnection = DirectCast(conn, OleDbConnection)
                    ' Get PK keys schema information for entire DB
                    dtKeys = cdb.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, New Object() {Nothing, Nothing, strTableFrom})
                    ' Sanity checks, pk may not be defined
                    If (dtKeys.Rows.Count = 0) Then Return strFKKey
                    For Each drow As DataRow In dtKeys.Rows
                        If (String.Compare(CStr(drow("PK_TABLE_NAME")), strTableFrom) = 0) And
                           (String.Compare(CStr(drow("FK_TABLE_NAME")), strTableTo) = 0) And
                           (String.Compare(CStr(drow("FK_COLUMN_NAME")), strColumn) = 0) Then
                            strFKKey = CStr(drow("FK_NAME"))
                            Exit For
                        End If
                    Next
                Catch ex As Exception
                End Try
            End If

            If (TypeOf conn Is SqlConnection) Then
                ' Not implemented yet
                Throw New NotImplementedException("cEwEDatabase.GetFkKeyName() not implemented for SqlConnections")
            End If

            Return strFKKey

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the name of an index for a given column in a table.
        ''' </summary>
        ''' <param name="strTable"></param>
        ''' <param name="strColumn">The column to remove the index from, if any.</param>
        ''' <returns>A name, or an empty string when no idnex was found.</returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function GetIndexName(ByVal strTable As String,
                                                 ByVal strColumn As String) As String

            Dim conn As IDbConnection = Me.GetConnection()
            Dim dtIndexes As DataTable = Nothing
            Dim strIndex As String = ""

            ' Execute oledb variant
            If (TypeOf conn Is OleDbConnection) Then

                Try
                    Dim cdb As OleDbConnection = DirectCast(conn, OleDbConnection)
                    ' Get PK keys schema information for entire DB
                    dtIndexes = cdb.GetSchema("Indexes")
                    ' Sanity checks, pk may not be defined
                    If (dtIndexes.Rows.Count = 0) Then Return strIndex
                    For Each drow As DataRow In dtIndexes.Rows
                        If (String.Compare(CStr(drow("TABLE_NAME")), strTable) = 0) And
                           (String.Compare(CStr(drow("COLUMN_NAME")), strColumn) = 0) Then
                            strIndex = CStr(drow("INDEX_NAME"))
                            Exit For
                        End If
                    Next
                Catch ex As Exception
                End Try
            End If

            If (TypeOf conn Is SqlConnection) Then
                ' Not implemented yet
                Throw New NotImplementedException("cEwEDatabase.GetIndexName() not implemented for SqlConnections")
            End If

            Return strIndex

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, reads data from a column that may not exist. In that case,
        ''' an optional default value is returned
        ''' </summary>
        ''' <param name="reader">The <see cref="IDataReader">IDataReader</see> to read from.</param>
        ''' <param name="strField">The name of the DB field (column) to read.</param>
        ''' <param name="objValueDefault">A default value to return if the field could not be read.</param>
        ''' <param name="objValueIgnore">Value to interpret as 'no value. When encountered, the default value will be returned.</param>
        ''' <returns>The value of the requested column, or the provided default if an error occurred.</returns>
        ''' -----------------------------------------------------------------------
        Public Function ReadSafe(ByVal reader As IDataReader,
                                 ByVal strField As String,
                                 Optional ByVal objValueDefault As Object = Nothing,
                                 Optional ByVal objValueIgnore As Object = CSng(-9999)) As Object

            Dim objResult As Object = Nothing

            If (reader Is Nothing) Then Return objValueDefault

            Try
                If Me.HasColumn(reader, strField) Then
                    objResult = reader.Item(strField)
                End If
            Catch ex As IndexOutOfRangeException
                ' Ugh
            Catch ex As InvalidOperationException
                'Console.WriteLine("DB: field '{0}' has no value, returning provided default '{1}'", strField, objValueDefault)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Console.WriteLine("DB: Exception {2} occurred while accessing field '{0}', returning provided default '{1}'", strField, objValueDefault, ex.ToString)
            End Try

            If (objResult Is Nothing) Then
                objResult = objValueDefault
            ElseIf (objValueIgnore IsNot Nothing) _
                And Not (Convert.IsDBNull(objResult)) _
                And Not (Convert.IsDBNull(objValueIgnore)) Then

                ' Compare ignore values
                If TypeOf objResult Is String Then
                    Try
                        If (String.Compare(CStr(objResult), Convert.ToString(objValueIgnore), True) = 0) Then
                            objResult = objValueDefault
                        End If
                    Catch ex As Exception
                    End Try
                ElseIf TypeOf objResult Is Boolean Then
                    Try
                        If (CBool(objResult) = Convert.ToBoolean(objValueIgnore)) Then
                            objResult = objValueDefault
                        End If
                    Catch ex As Exception
                    End Try
                Else
                    Try
                        If (CSng(objResult) = Convert.ToSingle(objValueIgnore)) Then
                            objResult = objValueDefault
                        End If
                    Catch ex As Exception
                    End Try
                End If

            End If

            If (Convert.IsDBNull(objResult)) Then
                objResult = objValueDefault
            End If

            Return objResult
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Safely drop a column by first removing any indexes on that column.
        ''' </summary>
        ''' <param name="strTable">The table to remove the column from.</param>
        ''' <param name="strColumn">The name of the column to remove.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function DropColumn(strTable As String, strColumn As String) As Boolean

            Dim bSuccess As Boolean = True

            Dim strIndex As String = Me.GetIndexName(strTable, strColumn)
            If (Not String.IsNullOrWhiteSpace(strIndex)) Then
                bSuccess = bSuccess And Me.Execute("DROP Index " & strIndex & " ON " & strTable)
            End If
            Return bSuccess And Me.Execute("ALTER TABLE " & strTable & " DROP COLUMN " & strColumn)

        End Function

#End Region ' DB helper methods

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Obtains a <see cref="DataSet">DataSet</see> for modifying records.
        ''' </summary>
        ''' <param name="adapter">The <see cref="IDataAdapter">IDataAdapter</see> to fill the <see cref="DataSet">DataSet</see> from.</param>
        ''' <param name="strTable">The name of the table to fill the <see cref="DataSet">DataSet</see> from.</param>
        ''' <returns>The <see cref="DataSet">DataSet</see> if successful, or Nothing if an error occurred.</returns>
        ''' <remarks>The obtained <see cref="DataSet">DataSet</see> should be released via <see cref="ReleaseDataSet">ReleaseWriter</see>.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Overridable Function GetDataSet(ByVal adapter As IDataAdapter, ByVal strTable As String) As DataSet
            Dim ds As New DataSet()
            Try
                adapter.Fill(ds)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                ds = Nothing
            End Try
            Return ds
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Commits all the pending changes in the <see cref="DataSet">DataSet</see>. This will
        ''' leave the DataSet open for further operations.
        ''' </summary>
        ''' <param name="dset">The <see cref="DataSet">DataSet</see> to commit</param>
        ''' <param name="adapter">The <see cref="IDataAdapter">OleDbDataAdapter</see> to write to the database</param>
        ''' <param name="strTable">The table to update</param>
        ''' <returns>True if successful</returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function CommitDataSet(ByVal dset As DataSet, ByVal adapter As IDataAdapter, ByVal strTable As String) As Boolean
            Dim bSucces As Boolean = True

            ' Is adapter specified?
            If (adapter Is Nothing) Then
                ' #No adapter = no need to update database. Done
                Return True
            End If

            ' Table name optional, no need to Assert
            Try
                adapter.Update(dset)
            Catch ex As Exception
                ' Woops
                bSucces = False
            End Try
            ' Report result
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Releases a <see cref="DataSet">DataSet</see> previously obtained via 
        ''' <see cref="GetDataSet">GetDataSet</see>.
        ''' </summary>
        ''' <param name="dset">The writer to release.</param>
        ''' <param name="adapter">The <see cref="IDataAdapter">IDataAdapter</see>
        ''' to commit any changes to. If this parameter is left blank, any changes made to
        ''' the dataset and its data are discarded.</param>
        ''' <param name="strTable">The name of the table to update.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function ReleaseDataSet(ByVal dset As DataSet, Optional ByVal adapter As IDataAdapter = Nothing, Optional ByVal strTable As String = "") As Boolean
            Return Me.CommitDataSet(dset, adapter, strTable)
        End Function

        Protected Function HasColumn(reader As IDataReader, strColumnName As String) As Boolean
            If (reader Is Nothing) Then Return False
            reader.GetSchemaTable().DefaultView.RowFilter = "ColumnName= '" + strColumnName + "'"
            Return (reader.GetSchemaTable().DefaultView.Count > 0)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether a given table exists in the open connection.
        ''' </summary>
        ''' <param name="strTableName">The table to check.</param>
        ''' <returns>True if the table exists in the open connection.</returns>
        ''' -------------------------------------------------------------------
        Protected Function HasTable(strTableName As String) As Boolean

            If (Me.GetConnection() Is Nothing) Then Return False

            If (TypeOf Me.GetConnection() Is OleDbConnection) Then
                Dim dtSchema As DataTable = DirectCast(Me.GetConnection(), OleDbConnection).GetOleDbSchemaTable(OleDb.OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, strTableName, "TABLE"})
                Return dtSchema.Rows.Count > 0
            Else
                Throw New NotImplementedException("HasTable not implemented for SQL databases")
            End If
            Return False

        End Function

#End Region ' Internals

#Region " OOP "

#Region " OOP public interfaces "

#Region " OOP Public classes "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Base class for implementing objects that can be stored in this type
        ''' of database.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Serializable()>
        Public MustInherit Class cOOPStorable

#Region " Privates "

            ''' <summary>Unique ID of an instance of cOOPStorable</summary>
            Private m_iDBID As Integer = cDBID_INVALID ' Key not assigned yet

            ''' <summary>Flag stating whether an cOOPStorabe instance is allowed to 
            ''' broadcast <see cref="OnChanged">OnChanged</see>events.</summary>
            Private m_bAllowEvents As Boolean = True

            ''' <summary>Flag preventing looped updates.</summary>
            Private m_bInUpdate As Boolean = False

#If DEBUG Then

            ''' <summary>Flag stating whether the object is deleted from the database.</summary>
            ''' <remarks>This flag is deliberately only available at debug time.</remarks>
            Private m_bDeleted As Boolean = False

#End If

#End Region ' Privates

#Region " Constructor "

            ''' <summary>Default ID for newly created cOOPStorable instances.</summary>
            Public Const cDBID_INVALID As Integer = 0

            Public Sub New()
                Me.m_iDBID = cDBID_INVALID
            End Sub

#End Region ' Constructor

#Region " Public properties "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Event to notify that instance unit has changed
            ''' </summary>
            ''' <param name="obj">The <see cref="cOOPStorable">instance</see>
            ''' that changed</param>
            ''' ---------------------------------------------------------------
            Public Event OnChanged(ByVal obj As cOOPStorable)

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' The unique ID of any object in the database. The database
            ''' manages this property exclusively although public read 
            ''' access is allowed.
            ''' </summary>
            ''' ---------------------------------------------------------------
            <Browsable(False)>
            Public Property DBID() As Integer
                Get
                    Return Me.m_iDBID
                End Get
                Friend Set(ByVal value As Integer)
                    Me.m_iDBID = value
                End Set
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set whether this instance is allowed to send
            ''' <see cref="OnChanged">change events</see>.
            ''' </summary>
            ''' ---------------------------------------------------------------
            <Browsable(False)>
            Public Property AllowEvents() As Boolean
                Get
                    Return Me.m_bAllowEvents
                End Get
                Set(ByVal value As Boolean)
                    Me.m_bAllowEvents = value
                    If m_bAllowEvents Then Me.SetChanged()
                End Set
            End Property

#End Region ' Public properties

#Region " Public interfaces "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Copy the content of another instance of cOOPStorable.
            ''' </summary>
            ''' <param name="objSrc">The source ofject to copy content from.</param>
            ''' ---------------------------------------------------------------
            Public Overridable Sub CopyFrom(ByVal objSrc As cOOPStorable)

                Dim apiSrc As PropertyInfo() = Nothing
                Dim apiTgt As PropertyInfo() = Nothing
                Dim piSrc As PropertyInfo = Nothing
                Dim piTgt As PropertyInfo = Nothing

                If (objSrc Is Nothing) Then Return

                ' Copy all copyable properties
                apiSrc = objSrc.GetType().GetProperties()
                apiTgt = Me.GetType().GetProperties()
                For Each piSrc In apiSrc
                    If String.Compare(piSrc.Name, "DBID") <> 0 Then
                        For Each piTgt In apiTgt
                            If piSrc.Name = piTgt.Name Then
                                Try
                                    If piTgt.CanWrite() Then
                                        piTgt.SetValue(Me, piSrc.GetValue(objSrc, Nothing), Nothing)
                                    End If
                                Catch ex As Exception
#If VERBOSE_LEVEL >= 2 Then
                                ' Ok, this did not work
                                Console.WriteLine("Woops: failed to copy prop {0} : {1}", piTgt.Name, ex.Message)
#End If
                                End Try
                            End If
                        Next
                    End If
                Next
            End Sub

#End Region ' Public interfaces

#Region " Internals "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Helper method, states whether the content of an instance has
            ''' been modified externally.
            ''' </summary>
            ''' ---------------------------------------------------------------
            <Browsable(False)>
            Protected Sub SetChanged()
                If Me.m_bAllowEvents Then
                    If (Me.m_bInUpdate = False) Then
                        ' Set deadlonk prevention lock
                        Me.m_bInUpdate = True
                        ' Raise event
                        RaiseEvent OnChanged(Me)
                        ' Release deadlonk prevention lock
                        Me.m_bInUpdate = False
                    End If
                End If
            End Sub

#If DEBUG Then

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Helper method, states whether the object has been deleted from 
            ''' the database and thus cannot be saved.
            ''' </summary>
            ''' <remarks>
            ''' This method is only available at debug time.
            ''' </remarks>
            ''' ---------------------------------------------------------------
            <Browsable(False)>
            Friend Property Deleted() As Boolean
                Get
                    Return Me.m_bDeleted
                End Get
                Set(ByVal value As Boolean)
                    Me.m_bDeleted = value
                End Set
            End Property

#End If

#End Region ' Internals

        End Class

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Strong-typed list for cOOPStorable instances.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Class cOOPStorableList
            Inherits cOOPStorable
            Implements IList(Of cOOPStorable)

            Private m_list As New List(Of cOOPStorable)

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Add an <see cref="cOOPStorable">object</see> to the list.
            ''' </summary>
            ''' <param name="item">The <see cref="cOOPStorable">object</see> to add.</param>
            ''' -------------------------------------------------------------------
            Public Sub Add(ByVal item As cOOPStorable) _
                Implements ICollection(Of cOOPStorable).Add
                Debug.Assert(Not Me.Contains(item), "Item already present in list")
                Me.m_list.Add(item)
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Clear the list.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub Clear() _
                Implements ICollection(Of cOOPStorable).Clear
                Me.m_list.Clear()
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' States whether the list contains a given <see cref="cOOPStorable">object</see>.
            ''' </summary>
            ''' <param name="item">The <see cref="cOOPStorable">object</see> to find.</param>
            ''' <returns>True if the list contains this <see cref="cOOPStorable">object</see>.</returns>
            ''' -------------------------------------------------------------------
            Public Function Contains(ByVal item As cOOPStorable) As Boolean _
                Implements ICollection(Of cOOPStorable).Contains
                Return Me.m_list.Contains(item)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Copy the list to an array, starting at a given array index.
            ''' </summary>
            ''' <param name="array">The array to copy <see cref="cOOPStorable">object</see> into.</param>
            ''' <param name="arrayIndex">Index of the list item to start copying from.</param>
            ''' <remarks>Please make sure the receiving array is big enough 
            ''' to hold the list items.</remarks>
            ''' -------------------------------------------------------------------
            Public Sub CopyTo(ByVal array() As cOOPStorable, ByVal arrayIndex As Integer) _
                Implements ICollection(Of cOOPStorable).CopyTo
                Me.m_list.CopyTo(array, arrayIndex)
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the number of <see cref="cOOPStorable">objects</see> in the list.
            ''' </summary>
            ''' -------------------------------------------------------------------
            <Browsable(False)>
            Public ReadOnly Property Count() As Integer _
                Implements ICollection(Of cOOPStorable).Count
                Get
                    Return Me.m_list.Count
                End Get
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get whether the list can be modified.
            ''' </summary>
            ''' -------------------------------------------------------------------
            <Browsable(False)>
            Public ReadOnly Property IsReadOnly() As Boolean _
                Implements ICollection(Of cOOPStorable).IsReadOnly
                Get
                    Return False
                End Get
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Remove an <see cref="cOOPStorable">object</see> from the list.
            ''' </summary>
            ''' <param name="item">The <see cref="cOOPStorable">object</see> to remove.</param>
            ''' <returns>True if successful.</returns>
            ''' -------------------------------------------------------------------
            Public Function Remove(ByVal item As cOOPStorable) As Boolean _
                Implements ICollection(Of cOOPStorable).Remove
                ' ToDo: remember this to actively erase item from DB?
                Me.m_list.Remove(item)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Returns an enumerator for cartwheeling though this list.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Function GetEnumerator() As IEnumerator(Of cOOPStorable) _
                Implements IEnumerable(Of cOOPStorable).GetEnumerator
                Return Me.m_list.GetEnumerator()
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Returns the list index of a given <see cref="cOOPStorable">object</see>.
            ''' </summary>
            ''' <param name="item">The <see cref="cOOPStorable">object</see> to locate.</param>
            ''' <returns>An integer value representing the index of the 
            ''' <see cref="cOOPStorable">object</see> in the list, or -1 if this item
            ''' was not found.</returns>
            ''' -------------------------------------------------------------------
            Public Function IndexOf(ByVal item As cOOPStorable) As Integer _
                Implements IList(Of cOOPStorable).IndexOf
                Return Me.m_list.IndexOf(item)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Insert an <see cref="cOOPStorable">object</see> at a given position in the list.
            ''' </summary>
            ''' <param name="index">The list position to insert the item at.</param>
            ''' <param name="item">The <see cref="cOOPStorable">object</see> to insert.</param>
            ''' -------------------------------------------------------------------
            Public Sub Insert(ByVal index As Integer, ByVal item As cOOPStorable) _
                Implements IList(Of cOOPStorable).Insert
                Debug.Assert(Not Me.Contains(item), "Item already present in list")
                Me.m_list.Insert(index, item)
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="cOOPStorable">object</see> at a given position in the list.
            ''' </summary>
            ''' <param name="index">The list index of the <see cref="cOOPStorable">object</see> to retrieve.</param>
            ''' -------------------------------------------------------------------
            <Browsable(False)>
            Default Public Property Item(ByVal index As Integer) As cOOPStorable _
                Implements IList(Of cOOPStorable).Item
                Get
                    Return Me.m_list.Item(index)
                End Get
                Set(ByVal value As cOOPStorable)
                    Me.m_list.Item(index) = value
                End Set
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Remove an <see cref="cOOPStorable">object</see> at a given position in the list.
            ''' </summary>
            ''' <param name="index">The position of the <see cref="cOOPStorable">object</see> to remove.</param>
            ''' -------------------------------------------------------------------
            Public Sub RemoveAt(ByVal index As Integer) _
                Implements IList(Of cOOPStorable).RemoveAt
                Me.m_list.RemoveAt(index)
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Neh
            ''' </summary>
            ''' -------------------------------------------------------------------
            Private Function GetEnumaarghAarghAargh() As System.Collections.IEnumerator _
                Implements System.Collections.IEnumerable.GetEnumerator
                Return Nothing
            End Function

        End Class

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Class for querying if cOOPStorable instances are stored in the 
        ''' database.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Class cOOPKey

#Region " Privates "

            ''' <summary>The ID for an object in the database</summary>
            Private m_iDBID As Integer = cOOPStorable.cDBID_INVALID
            ''' <summary>The runtime type that was used to write the
            ''' record in the database with this ID.</summary>
            Private m_tOriginating As Type = Nothing

#End Region ' Privates

#Region " Constructor "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Construct a new cOOPKey instance.
            ''' </summary>
            ''' <param name="t">The runtime type to generate the key for.</param>
            ''' <param name="iDBID">The unique ID that this key is generated for.</param>
            ''' ---------------------------------------------------------------
            Friend Sub New(ByVal t As Type, ByVal iDBID As Integer)

                ' Sanity checks
                Debug.Assert(t IsNot Nothing)
                Debug.Assert(iDBID <> cOOPStorable.cDBID_INVALID)

                Me.m_tOriginating = t
                Me.m_iDBID = iDBID

            End Sub

#End Region ' Constructor

#Region " Properties "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get the unique ID for the key.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property DBID() As Integer
                Get
                    Return Me.m_iDBID
                End Get
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get the runtime type for the key.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property OriginatingType() As Type
                Get
                    Return Me.m_tOriginating
                End Get
            End Property

#End Region ' Properties

        End Class

#End Region ' OOP Public classes

#Region " OOP Read "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Read an cOOPKey for a ID in the database. If all goes well a 
        ''' <see cref="cOOPKey">object key</see> is returned, which links the
        ''' ID value to the originating runtime type.
        ''' </summary>
        ''' <param name="iDBID">
        ''' The ID to retrieve an <see cref="cOOPKey">object key</see> for.
        ''' </param>
        ''' <returns>
        ''' A <see cref="cOOPKey">object key</see> instance, or null if the ID 
        ''' was not present in the database, or if the originating runtime
        ''' type was not present in the current loaded set of assemblies.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Protected Function ReadObjectKey(ByVal iDBID As Integer) As cOOPKey

            ' Sanity check
            If Not Me.m_bOOPEnabled Then Return Nothing

            Dim reader As IDataReader = Nothing
            Dim strTypeName As String = ""
            Dim tOrg As Type = Nothing
            Dim strSQL As String = ""
            Dim objKey As cOOPKey = Nothing

            ' Construct SQL statement to query the runtime information from the 
            ' OOP core table 'cOOPStorable'.
            strSQL = String.Format("SELECT {0}, DBID FROM {1} WHERE DBID={2}", OOP_CLASSNAMECOL, Me.OOPGetTableName(GetType(cOOPStorable)), iDBID)
            reader = Me.GetReader(strSQL)
            Try
                ' Don't try this at home
                reader.Read()
                ' Get the original type name from the database
                strTypeName = CStr(reader(OOP_CLASSNAMECOL))
                ' Try to find originating type in the set of loaded assemblies
                tOrg = Me.OOPStringToType(strTypeName)
                ' Succes?
                If (tOrg IsNot Nothing) Then objKey = New cOOPKey(tOrg, iDBID)
                ' Done
                Me.ReleaseReader(reader)
            Catch ex As Exception

            End Try
            ' Return constructed key, if any
            Return objKey
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Read keys for all objects of a given type from the database.
        ''' </summary>
        ''' <param name="t">Type to filter by.</param>
        ''' <param name="bIncludeInherited">Flag indicating whether objects 
        ''' inherited from <paramref name="t">t</paramref> are allowed to be 
        ''' returned as well.</param>
        ''' <returns>An array of <see cref="cOOPKey">object keys</see>.</returns>
        ''' -------------------------------------------------------------------
        Protected Function ReadObjectKeys(ByVal t As Type, Optional ByVal bIncludeInherited As Boolean = True) As cOOPKey()

            ' Sanity check
            If Not Me.m_bOOPEnabled Then Return Nothing

            Dim strTypeName As String = ""
            Dim strSQL As String = ""
            Dim reader As IDataReader = Nothing
            Dim objKey As cOOPKey = Nothing
            Dim lKeys As New List(Of cOOPKey)
            Dim tOrg As Type = Nothing
            Dim bInclude As Boolean = True

            strSQL = String.Format("SELECT {0}, DBID FROM {1} ORDER BY DBID ASC", OOP_CLASSNAMECOL, Me.OOPGetTableName(GetType(cOOPStorable)))
            reader = Me.GetReader(strSQL)

            If reader IsNot Nothing Then
                Try
                    While reader.Read

                        ' Get type name
                        strTypeName = CStr(reader(OOP_CLASSNAMECOL))
                        ' Try to find orginating type in currently loaded assemblies
                        tOrg = OOPStringToType(strTypeName)

                        ' Was originating type succesfully located?
                        If (tOrg IsNot Nothing) Then
                            ' #Yes: allowed to include inherited classes?
                            If bIncludeInherited Then
                                ' #Yes: include when tOrg is inherited - or equals - from t
                                bInclude = t.IsAssignableFrom(tOrg)
                            Else
                                ' #No: include only when t equals tOrg
                                bInclude = (t Is tOrg)
                            End If
                        Else
                            ' #No: do not include
                            bInclude = False
                        End If

                        ' Include?
                        If bInclude Then
                            ' #Yes: generate key
                            objKey = New cOOPKey(tOrg, CInt(reader("DBID")))
                            ' Add new key to the list to return
                            lKeys.Add(objKey)
                        End If

                    End While
                    Me.ReleaseReader(reader)

                Catch ex As Exception
                    cLog.Write(ex, eVerboseLevel.Detailed, "cEwEDatabase.ReadObjectKeys(" & strSQL & ")")
                End Try
            End If

            ' Present results
            Return lKeys.ToArray()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Reads an object from the database.
        ''' </summary>
        ''' <param name="key"><see cref="cOOPKey">Object key</see> to read the 
        ''' object for.</param>
        ''' <returns>A <see cref="cOOPStorable">cOOPStorable</see>-derived 
        ''' instance, or null if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Function ReadObject(ByVal key As cOOPKey) As cOOPStorable
            If (key Is Nothing) Then
                Return Nothing
            End If
            Return Me.ReadObject(key.OriginatingType, key.DBID)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Reads an object from the database.
        ''' </summary>
        ''' <param name="t">The type of the object to read.</param>
        ''' <param name="iDBID">The ID of the object to read.</param>
        ''' <returns>A <see cref="cOOPStorable">cOOPStorable</see>-derived 
        ''' instance, or null if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Function ReadObject(ByVal t As Type, ByVal iDBID As Integer) As cOOPStorable
            If Not Me.m_bOOPEnabled Then Return Nothing
            Dim piKey As PropertyInfo = Me.OOPGetKeyProperty(t)
            Return OOPReadObject(t, iDBID, piKey)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Reads all objects of a given type from the database.
        ''' </summary>
        ''' <param name="t">The type to read objects for.</param>
        ''' <param name="bIncludeInherited">Flag indicating whether objects 
        ''' inherited from <paramref name="t">t</paramref> are allowed to be 
        ''' read as well.</param>
        ''' <returns>
        ''' An array of <see cref="cOOPStorable">cOOPStorable</see>-derived 
        ''' instances.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function ReadObjects(ByVal t As Type,
                                    Optional ByVal bIncludeInherited As Boolean = True) As cOOPStorable()

            Dim lObjs As New List(Of cOOPStorable)

            If (Not Me.m_bOOPEnabled) Then Return lObjs.ToArray()
            If (Not Me.HasTable(OOPGetTableName(GetType(cOOPStorable)))) Then Return lObjs.ToArray()

            Dim aKeys As cOOPKey() = Me.ReadObjectKeys(t, bIncludeInherited)
            Dim obj As cOOPStorable = Nothing
            Dim piKey As PropertyInfo = Me.OOPGetKeyProperty(t)

            For iKey As Integer = 0 To aKeys.Length - 1
                obj = Me.OOPReadObject(aKeys(iKey).OriginatingType, aKeys(iKey).DBID, piKey)
                If obj IsNot Nothing Then lObjs.Add(obj)
            Next
            Return lObjs.ToArray()

        End Function

#End Region ' OOP Read

#Region " OOP Write "

        ''' <summary>Assembly for OOP transaction.</summary>
        Private m_assTransaction As Assembly = Nothing

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Starts an OOP database transaction. In addition to a regular 
        ''' <see cref="BeginTransaction">transaction</see> this method also
        ''' opens all adapters for <see cref="cOOPStorable"/>-derived classes
        ''' in a given assembly to reduce the amount of adapter traffic while
        ''' the transaction is open.
        ''' </summary>
        ''' <param name="ass">The assembly to load types from.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function OOPBeginTransaction(ass As Assembly, Optional bCreateSchema As Boolean = False) As Boolean

            ' Sanity checks
            Debug.Assert(ass IsNot Nothing)
            Debug.Assert(Me.m_assTransaction Is Nothing)

            Dim bSuccess As Boolean = True

            If Not Me.BeginTransaction() Then Return False

            Me.m_assTransaction = ass

            For Each t As Type In Me.m_assTransaction.GetTypes()
                If t.IsSubclassOf(GetType(cOOPStorable)) Then
                    If (bCreateSchema) Then
                        bSuccess = bSuccess And OOPUpdateObjectSchema(t)
                    End If

                    Dim strTable As String = Me.OOPGetTableName(t)
                    While Not OOPIsBaseClass(t)
                        Me.OOPGetAdapter(strTable)
                        t = t.BaseType
                    End While
                End If
            Next
            Return bSuccess

        End Function

        Public Function OOPCommitTransaction(Optional bCommit As Boolean = True) As Boolean

            ' Sanity checks
            Debug.Assert(Me.m_assTransaction IsNot Nothing)

            For Each t As Type In Me.m_assTransaction.GetTypes()
                If t.IsSubclassOf(GetType(cOOPStorable)) Then
                    Dim strTable As String = Me.OOPGetTableName(t)
                    While Not OOPIsBaseClass(t)
                        Me.OOPReleaseAdapter(strTable)
                        t = t.BaseType
                    End While
                End If
            Next
            Debug.Assert(Not OOPHasOpenAdapters())

            Me.m_assTransaction = Nothing
            Return Me.CommitTransaction(bCommit)

        End Function

        Public Function OOPRollbackTransaction() As Boolean

            ' Sanity checks
            Debug.Assert(Me.m_assTransaction IsNot Nothing)

            For Each t As Type In Me.m_assTransaction.GetTypes()
                If t.IsAssignableFrom(GetType(cOOPStorable)) Then
                    Dim strTable As String = Me.OOPGetTableName(t)
                    While Not OOPIsBaseClass(t)
                        Me.OOPReleaseAdapter(strTable)
                        t = t.BaseType
                    End While
                End If
            Next
            Debug.Assert(Not OOPHasOpenAdapters())

            Me.m_assTransaction = Nothing
            Return Me.RollbackTransaction()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Write an <see cref="cOOPStorable">OOPstorable-derived</see> instance 
        ''' to the database.
        ''' </summary>
        ''' <param name="obj">The object to save.</param>
        ''' <remarks>
        ''' This method will verify - and update if  necessary - the database
        ''' schema for this class of object and its base classes. This check 
        ''' is performed only the very first time a particular object is written 
        ''' to the database; consecutive save attempts will bypass the check for
        ''' performance reasons.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Function WriteObject(ByVal obj As cOOPStorable) As Boolean

            ' Friendly sanity check
            If Not Me.m_bOOPEnabled Then Return False

#If DEBUG Then
            ' Sanity check
            Debug.Assert(Not obj.Deleted, "Object is deleted and cannot not be saved.")
#End If

            Dim t As Type = obj.GetType()
            Dim api As PropertyInfo() = Me.OOPGetStorableProperties(t)
            Dim piKey As PropertyInfo = Nothing
            Dim bSucces As Boolean = True

            '' Make sure database schema is up to date to accomodate this object
            'bSucces = Me.OOPUpdateObjectSchema(t)

            ' Everything well?
            If bSucces Then

                ' #Yes: get the property that holds the primary key (DBID)
                piKey = Me.OOPGetKeyProperty(t)

                ' Does the database already have an ID for this object? 
                ' cOOPStorable instances that are not saved yet will have an
                ' invalid database ID, which will need to be properly set
                ' before the object can be saved to the database.
                If (obj.DBID = cOOPStorable.cDBID_INVALID) Then
                    ' #Yes: invalid ID must be set
                    obj.DBID = Me.m_iNextDBID
                    ' Increment next ID for next object.
                    Me.m_iNextDBID += 1
                End If

                ' Add to saved object cache to prevent looped saving. Assume all is well
                Me.m_OOPObjectCache.AddObject(obj)
                ' Write the object
                bSucces = Me.OOPWriteObjectRecursive(t, obj, piKey)

                ' Failure?!
                If Not bSucces Then
                    ' #Yes: remove object from the saved object cache
                    Me.m_OOPObjectCache.RemoveObject(obj)
                End If
            End If

            ' Report outcome
            Return bSucces

        End Function

#End Region ' OOP Write

#Region " OOP Delete "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Delete an <see cref="cOOPStorable">cOOPStorable-derived</see> 
        ''' instance from the database.
        ''' </summary>
        ''' <param name="obj">The object to delete.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function DeleteObject(ByVal obj As cOOPStorable) As Boolean

            Dim bSucces As Boolean = True

            ' Friendly sanity check
            If Not Me.m_bOOPEnabled Then Return False
            If (obj Is Nothing) Then Return False
            If (obj.DBID = 0) Then Return True

            ' Delete object recursively
            bSucces = Me.DeleteObjectRecursive(obj, obj.GetType())

            ' All well?
            If bSucces Then
                ' #Yes: remove the object from the saved object cache
                Me.m_OOPObjectCache.RemoveObject(obj)
            End If

            ' Report outcome
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Delete an cOOPStorable-derived instance from the database, starting
        ''' at the given class level, and recursively deleting the direct
        ''' base class instance if applicable.
        ''' </summary>
        ''' <param name="obj">The object to delete.</param>
        ''' <param name="t">The class level to delete from the database.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function DeleteObjectRecursive(ByVal obj As cOOPStorable, ByVal t As Type) As Boolean

            Dim strSQL As String = String.Format("DELETE FROM {0} WHERE DBID={1}", Me.OOPGetTableName(t), obj.DBID)
            Dim bSucces As Boolean = True

            ' Able to delete object at class level 't' from the database
            If Me.Execute(strSQL) Then
                Try
                    ' #Yes: remove from local cache
                    Me.m_OOPObjectCache.RemoveObject(obj)
                Catch ex As Exception
                    ' #woops: panic
                    bSucces = False
                End Try
            Else
                ' #woops: panic
                bSucces = False
            End If

            ' Not the base class?
            If Not Me.OOPIsBaseClass(t) Then
                ' #Good, delete direct base class next
                bSucces = bSucces And Me.DeleteObjectRecursive(obj, t.BaseType)
            End If

#If DEBUG Then
            ' Set debug-mode verification flag
            obj.Deleted = bSucces
#End If

            ' Report outcome
            Return bSucces

        End Function

#End Region ' OOP Delete

#End Region ' OOP public interfaces

#Region " OOP Admin "

#Region " OOP Admin vars "

        ''' <summary>Flag stating whether the database has been prepared 
        ''' for OOP object access. To prepare the database for OOP access
        ''' refer to <see cref="OOPEnabled">OOPEnabled</see>.</summary>
        Private m_bOOPEnabled As Boolean = False
        ''' <summary>The unique ID to assigne to new cOOPStorable instances
        ''' in the database.</summary>
        Private m_iNextDBID As Integer = -1
        ''' <summary>Cache of all objects already saved into the database.
        ''' Keeping a cache is much faster than having to query the database
        ''' for every potential save.</summary>
        Private m_OOPObjectCache As cOOPObjectCache = Nothing
        ''' <summary>Cache of all object for which the database schema has
        ''' already been verifyfied since the last time the database
        ''' was opened.</summary>
        Private m_OOPObjectSchemaVerified As List(Of Type) = Nothing

#End Region ' OOP Admin vars

#Region " OOP Amin interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Turn on or off OOP capabilities
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Property OOPEnabled() As Boolean
            Get
                Return Me.m_bOOPEnabled
            End Get
            Set(ByVal bEnable As Boolean)

                ' Optimization
                If (bEnable = Me.m_bOOPEnabled) Then Return

                ' Must turn OOP capabilities on?
                If bEnable Then
                    ' #Yes: determine next unique ID
                    Dim strTable As String = Me.OOPGetTableName(GetType(cOOPStorable))

                    Me.m_iNextDBID = 1
                    If Me.HasTable(strTable) Then Me.m_iNextDBID = CInt(Me.GetValue(String.Format("SELECT MAX(DBID) FROM {0}", strTable), 0)) + 1

                    ' Create schema verification cache
                    Me.m_OOPObjectSchemaVerified = New List(Of Type)
                    ' Create saved object cache
                    Me.m_OOPObjectCache = New cOOPObjectCache()
                Else
                    ' #No: reset OOP capabilities
                    Me.m_iNextDBID = 0
                    Me.m_OOPObjectSchemaVerified = Nothing
                    Me.m_OOPObjectCache = Nothing
                End If
                ' Yo!
                Me.m_bOOPEnabled = bEnable
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Internal admin helper; clear the saved object cache.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Sub OOPFlushObjectCache()
            Me.m_OOPObjectCache.Clear()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Internal admin helper; clear the verified schema cache.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Sub OOPFlushSchemaCache()
            Me.m_OOPObjectSchemaVerified.Clear()
        End Sub

#End Region ' OOP Amin interfaces

#Region " OOP Admin internals "

#Region " OOP Object cache "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper class, maintains a dictionary of processed 
        ''' <see cref="cOOPStorable">objects</see> for reassembling item links.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Class cOOPObjectCache

            ''' <summary>The cache.</summary>
            Private m_dtObjectCache As New Dictionary(Of Integer, cOOPStorable)

            ''' ---------------------------------------------------------------
            ''' <summary>Add an object to the cache.</summary>
            ''' <param name="obj">The object to add.</param>
            ''' ---------------------------------------------------------------
            Public Sub AddObject(ByVal obj As cOOPStorable)
                If Not HasObject(obj.DBID) Then
                    Me.m_dtObjectCache(obj.DBID) = obj
                End If
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Remove an object from the cache.
            ''' </summary>
            ''' <param name="obj">The object to remove.</param>
            ''' ---------------------------------------------------------------
            Public Sub RemoveObject(ByVal obj As cOOPStorable)
                If HasObject(obj.DBID) Then
                    Me.m_dtObjectCache.Remove(obj.DBID)
                End If
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Retrieves an object from the cache.
            ''' </summary>
            ''' <param name="iDBID">The ID of the object to retrieve.</param>
            ''' <returns>An object, or nothing if the object is not present
            ''' in the cache.</returns>
            ''' ---------------------------------------------------------------
            Public Function GetObject(ByVal iDBID As Integer) As cOOPStorable
                If HasObject(iDBID) Then
                    Return Me.m_dtObjectCache(iDBID)
                Else
                    Return Nothing
                End If
            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' States whether an object with a given database ID is present
            ''' in the cache.
            ''' </summary>
            ''' <param name="iDBID">The ID of the object to find.</param>
            ''' <returns>True if the object is present in the cache.</returns>
            ''' ---------------------------------------------------------------
            Public Function HasObject(ByVal iDBID As Integer) As Boolean
                Return Me.m_dtObjectCache.ContainsKey(iDBID)
            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Clears the object cache.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Sub Clear()
                Me.m_dtObjectCache.Clear()
            End Sub

        End Class

#End Region ' OOP Object cache

#Region " OOP Schema management "

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' OOP foreign key
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Structure OOP_sFKInfo
            Public Sub New(ByVal strCol As String, ByVal strTable As String, ByVal bInherited As Boolean)
                Me.ColumnName = strCol
                Me.TableName = strTable
                Me.Inherited = bInherited
            End Sub
            Public ColumnName As String
            Public TableName As String
            Public Inherited As Boolean
        End Structure

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a table for a <see cref="cOOPStorable">cOOPStorable</see>-derived class 
        ''' </summary>
        ''' <param name="t">The <see cref="Type">type</see> to build the table for.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function OOPCreateObjectTable(ByVal t As Type) As Boolean

            ' Get all storable properties for type t
            Dim api As PropertyInfo() = Me.OOPGetStorableProperties(t)
            Dim strColumnName As String = ""
            Dim strColumnType As String = ""
            Dim strQuery As String = ""
            Dim sbClause As New Text.StringBuilder
            Dim bSucces As Boolean = False
            Dim lFK As New List(Of OOP_sFKInfo)

            ' Iterate through all 
            For Each pi As PropertyInfo In api
                strColumnName = Me.OOPGetColumnName(pi)
                strColumnType = Me.OOPGetColumnType(pi)

                If Not String.IsNullOrEmpty(strColumnName) And Not String.IsNullOrEmpty(strColumnType) Then
                    If sbClause.Length > 0 Then sbClause.Append(", ")
                    sbClause.Append("[" & strColumnName & "] " & strColumnType)
                End If

                If Me.OOPIsForeignKeyProperty(pi) Then
                    Me.OOPUpdateObjectSchema(pi.PropertyType)
                    lFK.Add(New OOP_sFKInfo(strColumnName, Me.OOPGetTableName(pi.PropertyType), False))
                End If
            Next

            If (sbClause.Length = 0) Then Return True

            ' Add class name as first column for base classes only
            If Me.OOPIsBaseClass(t) Then
                sbClause.Insert(0, OOP_CLASSNAMECOL + " TEXT(64), ")
            Else
                lFK.Insert(0, New OOP_sFKInfo("DBID", Me.OOPGetTableName(t.BaseType), True))
            End If

            ' Create table
            strQuery = String.Format("CREATE TABLE {0} ({1})", Me.OOPGetTableName(t), sbClause.ToString)
            bSucces = Me.Execute(strQuery)

            ' Create primary key for this table
            strQuery = String.Format("ALTER TABLE {0} ADD PRIMARY KEY (DBID)", Me.OOPGetTableName(t))
            bSucces = bSucces And Me.Execute(strQuery)

            ' Create all FKs
            For Each fk As OOP_sFKInfo In lFK
                strQuery = String.Format("ALTER TABLE {2} ADD FOREIGN KEY ({1}) REFERENCES {0} (DBID) ON DELETE CASCADE",
                    fk.TableName,
                    fk.ColumnName, Me.OOPGetTableName(t))
                bSucces = bSucces And Me.Execute(strQuery)
            Next

            If Not bSucces Then
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("Failed to create table scheme {0}", Me.OOPGetTableName(t))
#End If
            End If
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update column definitions for a single given cOOPStorable-derived 
        ''' type within an existing table.
        ''' </summary>
        ''' <param name="t">The type to update the database schema for.</param>
        ''' <param name="conn">The database connection to update the database
        ''' schema.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' <para>This method does not recurse; a single type is processed for a 
        ''' single table. To process an enitre object inheritance tree, use
        ''' <see cref="OOPUpdateObjectSchema">OOPUpdateObjectSchema</see>.</para>
        ''' <para>Note that this method does not handle column datatype 
        ''' conversions; only mssing columns are added.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Function OOPUpdateObjectTable(ByVal t As Type, ByVal conn As OleDbConnection) As Boolean

            Dim dt As DataTable = Nothing
            Dim api As PropertyInfo() = Me.OOPGetStorableProperties(t)
            Dim lpiMissing As New List(Of PropertyInfo)
            Dim strTable As String = Me.OOPGetTableName(t)
            Dim strName As String = ""
            Dim strType As String = ""
            Dim strSQL As String = ""
            Dim sbClauses As New System.Text.StringBuilder
            Dim bSucces As Boolean = True
            Dim bFound As Boolean = False

            ' Obtain the list of columns for the desired table
            dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, New String() {Nothing, Nothing, strTable, Nothing})

            ' Obtain a list of storable property that do not have a column in the associated schema
            For iProp As Integer = 0 To api.Length - 1
                strName = Me.OOPGetColumnName(api(iProp))
                bFound = False
                For Each drow As DataRow In dt.Rows
                    If String.Compare(CStr(drow("COLUMN_NAME")), strName, True) = 0 Then bFound = True
                Next drow
                If Not bFound Then lpiMissing.Add(api(iProp))
            Next

            ' No missing columns? #Goody: all done
            If lpiMissing.Count = 0 Then Return True

            ' Add missing columns to the database schema
            For Each pi As PropertyInfo In lpiMissing
                strName = Me.OOPGetColumnName(pi)
                strType = Me.OOPGetColumnType(pi)
                If Not String.IsNullOrEmpty(strName) And Not String.IsNullOrEmpty(strType) Then
                    If sbClauses.Length > 0 Then sbClauses.Append(", ")
                    sbClauses.Append(String.Format("{0} {1}", strName, strType))
                End If
            Next

            If (sbClauses.Length > 0) Then
                ' M$ Access does not like brackets in 'ALTER TABLE <name> ADD (<clause(s)>)'
                strSQL = String.Format("ALTER TABLE {0} ADD {1}", Me.OOPGetTableName(t), sbClauses.ToString)
                bSucces = Me.Execute(strSQL)
            End If

            If Not bSucces Then
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("Failed to update table scheme {0}", Me.OOPGetTableName(t))
#End If
            End If

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update the database schema for storing a class of a given type. The
        ''' entire inheritance tree of the given type is processed, new tables
        ''' are created and table columns are added when necessary.
        ''' </summary>
        ''' <param name="t">The type to update the database schema for.</param>
        ''' <returns>
        ''' True if successful.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Private Function OOPUpdateObjectSchema(ByVal t As Type) As Boolean

            Dim conn As OleDbConnection = DirectCast(Me.GetConnection(), OleDbConnection)
            Dim strTable As String = ""
            Dim dt As DataTable = Nothing
            Dim bIsBaseClass As Boolean = False
            Dim bSucces As Boolean = True ' Ommmm

            ' Already verified?
            If (Me.m_OOPObjectSchemaVerified.IndexOf(t) <> -1) Then Return True
            ' Immediately flag as verified to prevent self-links to cause verification loops
            Me.m_OOPObjectSchemaVerified.Add(t)

            ' Not the base class?
            If Not Me.OOPIsBaseClass(t) Then
                ' #Good, write base class first
                bSucces = bSucces And Me.OOPUpdateObjectSchema(t.BaseType)
            End If

            ' Process this class
            strTable = t.Name()
            ' Obtain the list of columns for the desired table
            dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New String() {Nothing, Nothing, strTable, "TABLE"})

            ' Does table exist?
            If (dt.Rows.Count = 0) Then
                ' #No: create it
                bSucces = bSucces And Me.OOPCreateObjectTable(t)
            Else
                ' #Yes: Update table
                bSucces = bSucces And Me.OOPUpdateObjectTable(t, conn)
            End If

            Return bSucces

        End Function

#End Region ' Schema management

#Region " OOP shared adapters "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Single adapter entry in the 
        ''' <see cref="m_dtOOPAdapterCache">adapter cache</see>.
        ''' </summary>
        ''' <remarks>
        ''' The adapter cache is meant to speed up object access during a single
        ''' database transaction while writing nested classes to the OOP database.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        Private Class cOOPAdapterCacheEntry

#Region " Privates "

            ''' <summary>Name of the table that an adapter references to.</summary>
            Private m_strTable As String
            ''' <summary>The cached adapter.</summary>
            Private m_adapter As IDataAdapter
            ''' <summary>Number of references to a cached adapater.</summary>
            Private m_iRefCount As Integer = 0

#End Region ' Privates

#Region " Constructor "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Contructs a new cache entry.
            ''' </summary>
            ''' <param name="strTable">The table the adapter was obtained for.</param>
            ''' <param name="adapter">The database adapter that was returned for
            ''' this table.</param>
            ''' ---------------------------------------------------------------
            Public Sub New(ByVal strTable As String, ByVal adapter As IDataAdapter)
                Me.m_strTable = strTable
                Me.m_adapter = adapter
                Me.m_iRefCount = 0
            End Sub

#End Region ' Constructor

#Region " Public interfaces "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Increase adapter usage count.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Sub AddRef()
                Me.m_iRefCount += 1
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Decrease adapter usage count.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Sub RemoveRef()
                Me.m_iRefCount -= 1
            End Sub

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' States if the adapter is no longer used.
            ''' </summary>
            ''' <returns>
            ''' True if released.
            ''' </returns>
            ''' ---------------------------------------------------------------
            Public Function Released() As Boolean
                Return Me.m_iRefCount = 0
            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Returns the referenced adapter.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property Adapter() As IDataAdapter
                Get
                    Return Me.m_adapter
                End Get
            End Property

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Returns the name of the referenced table.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public ReadOnly Property Table() As String
                Get
                    Return Me.m_strTable
                End Get
            End Property

#End Region ' Public interfaces

        End Class

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Cache of open database adapters.
        ''' </summary>
        ''' <remarks>
        ''' <para>When writing an OOP class structure, objects are written recursively
        ''' to the database, ensuring that baseclass information is written first. Linked
        ''' objects are written whenever a reference is encountered. Due to this unpredictable
        ''' flow, chances are that database tables need to accessed for writing several
        ''' times when writing a single object instance.</para>
        ''' <para>A database will deny multiple adapter request for writing. To overcome this
        ''' problem, the adapter cache maintains a list of open adapters available while saving
        ''' OOP data which can be reused until the entire write operation is done.</para>
        ''' <para>Adapters are obtained via <see cref="OOPGetAdapter">OOPGetAdapter</see>,
        ''' and are released via <see cref="OOPReleaseAdapter">OOPReleaseAdapter</see>.</para>
        ''' </remarks>
        ''' ------------------------------------------------------------------- 
        Private m_dtOOPAdapterCache As New Dictionary(Of String, cOOPAdapterCacheEntry)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Obtain a database adapter from the adapter cache.
        ''' </summary>
        ''' <param name="strTable">Table name to obtain the adapter for.</param>
        ''' <returns>A database adapter if successful, or Nothing if an error occurred.</returns>
        ''' <remarks>
        ''' An adapter obtained via this method must be released via 
        ''' <see cref="OOPReleaseAdapter">OOPReleaseAdapter</see>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Function OOPGetAdapter(ByVal strTable As String) As IDataAdapter
            Dim wl As cOOPAdapterCacheEntry = Nothing
            If Not m_dtOOPAdapterCache.ContainsKey(strTable) Then
                wl = New cOOPAdapterCacheEntry(strTable, Me.GetAdapter("SELECT * FROM " + strTable))
                m_dtOOPAdapterCache(strTable) = wl
            Else
                wl = m_dtOOPAdapterCache(strTable)
            End If
            wl.AddRef()
            Return wl.Adapter
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Release a database adapter from the adapter cache that was previously
        ''' obtained via <see cref="OOPGetAdapter">OOPGetAdapter</see>.
        ''' </summary>
        ''' <param name="strTable">Table name to release the adapter for.</param>
        ''' <returns>True if the adapter was released succesfully, or False 
        ''' if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Protected Function OOPReleaseAdapter(ByVal strTable As String) As Boolean
            Dim wl As cOOPAdapterCacheEntry = Nothing
            If m_dtOOPAdapterCache.ContainsKey(strTable) Then
                wl = m_dtOOPAdapterCache(strTable)
                wl.RemoveRef()
                If wl.Released() Then
                    m_dtOOPAdapterCache.Remove(strTable)
                    Return Me.ReleaseAdapter(wl.Adapter)
                End If
                Return True
            Else
                Debug.Assert(False)
            End If
            Return False
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, states if there are open adapters left in the adapter cache.
        ''' </summary>
        ''' <returns>True if there are any open adapters left in the cache.</returns>
        ''' <remarks>There should be no more open adapters left when a write
        ''' operation is complete.</remarks>
        ''' -------------------------------------------------------------------
        Protected Function OOPHasOpenAdapters() As Boolean
            Return (Me.m_dtOOPAdapterCache.Count > 0)
        End Function

#End Region ' OOP shared adapters

#End Region ' OOP Admin internals

#End Region ' Admin

#Region " OOP internals "

#Region " Helpers "

        ''' <summary>
        ''' Reserved column name for storing class names in the OOP database schema
        ''' </summary>
        Private Const OOP_CLASSNAMECOL As String = "xCLASS_NAMEx"

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, returns a SQL table name for a given class/type.
        ''' </summary>
        ''' <param name="t">The class to return the table name for.</param>
        ''' <returns>A table name for the given class.</returns>
        ''' <remarks>
        ''' This method will convert invalid namespace characters into
        ''' valid SQL table name characters.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Function OOPGetTableName(ByVal t As Type) As String
            ' Get class name (including namespaces)
            Dim strName As String = t.Name()
            ' Replace invalid SQL characters
            Return strName.Replace(".", "_").Replace("+", "_")
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, returns a SQL column name for a given property.
        ''' </summary>
        ''' <param name="pi">The property info instance to obtain a column
        ''' name for.</param>
        ''' <returns>A column name.</returns>
        ''' -------------------------------------------------------------------
        Private Function OOPGetColumnName(ByVal pi As PropertyInfo) As String
            Return pi.Name()
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, states whether a given class is inherited directly 
        ''' from Object.
        ''' </summary>
        ''' <param name="t">The class to test.</param>
        ''' <returns>True if class <paramref name="t">t</paramref> is directly
        ''' inherited from Object.</returns>
        ''' -------------------------------------------------------------------
        Private Function OOPIsBaseClass(ByVal t As Type) As Boolean
            Return t.BaseType Is GetType(Object)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the property that holds the primary key for a 
        ''' <see cref="cOOPStorable">cOOPStorable</see>-derived <see cref="Type">Type</see>.
        ''' </summary>
        ''' <param name="t">The <see cref="cOOPStorable">cOOPStorable</see>-derived 
        ''' <see cref="Type">Type</see> to get the primary key for.</param>
        ''' <returns>A <see cref="PropertyInfo">PropertyInfo</see> instance, or
        ''' nothing if the primary key property was not found. Which is not good;
        ''' this will probably only occur when the class was not properly derived.</returns>
        ''' -------------------------------------------------------------------
        Private Function OOPGetKeyProperty(ByVal t As Type) As PropertyInfo
            Return t.GetProperty("DBID")
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; returns all writable public properties that are either
        ''' directly declared by a provided <paramref name="t">class</paramref>,
        ''' or that serves as the primary key to the class structure. Only
        ''' properties of classes derived from <see cref="cOOPStorable">cOOPStorable</see>
        ''' are returned.
        ''' </summary>
        ''' <param name="t">The <see cref="cOOPStorable">cOOPStorable</see>-derived
        ''' <see cref="Type">Type</see> to find storable properties for.</param>
        ''' <returns>An array of <see cref="PropertyInfo">PropertyInfo</see> instances.</returns>
        ''' -------------------------------------------------------------------
        Private Function OOPGetStorableProperties(ByVal t As Type) As PropertyInfo()
            Dim lpi As New List(Of PropertyInfo)
            Dim bAllowed As Boolean = False

            ' ToDo: test if Type t is derived of cOOPStorable?
            If GetType(cOOPStorable).IsAssignableFrom(t) Then
                For Each pi As PropertyInfo In t.GetProperties()
                    ' Allow (props declared directly in this class) AND (the property is writable)
                    bAllowed = t.Equals(pi.DeclaringType) And (pi.CanWrite())
                    ' Also allow primary key
                    bAllowed = bAllowed Or (pi.Name = "DBID")
                    ' Allowed?
                    If (bAllowed) Then
                        ' #Yes: add it
                        lpi.Add(pi)
                    End If
                Next
            End If

            Return lpi.ToArray()
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, returns the SQL data type for a given property.
        ''' </summary>
        ''' <param name="pi">The property to obtain the SQL data type for.</param>
        ''' <returns>
        ''' An SQL data type name, or an empty string if the property value type
        ''' was not supported. The list of supported data types can easily be 
        ''' extended.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Private Function OOPGetColumnType(ByVal pi As PropertyInfo) As String
            Dim strType As String = pi.PropertyType.ToString()
            Select Case strType
                Case "System.Double"
                    Return "DOUBLE"
                Case "System.Single"
                    Return "SINGLE"
                Case "System.Int64"
                    Return "LONG" ' BIGINT?
                Case "System.Int32"
                    Return "INTEGER"
                Case "System.Int16"
                    Return "SHORT" ' SMALLINT?
                        'Case "System.Byte"
                        '    Return "SHORT"
                Case "System.Boolean"
                    ' I'm refusing to use Access 'YESNO' because it's not portable
                    Return "SHORT"
                Case "System.String"
                    ' Perform property browsable attribute length check?
                    Return "TEXT(255)"
                Case Else
                    ' Check for FK
                    If OOPIsForeignKeyProperty(pi) Then
                        ' Store DBID of FK
                        Return "INTEGER"
                    End If
                    ' This list can be greatly extended
            End Select
            Return ""
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the default value for a property.
        ''' </summary>
        ''' <param name="pi">The property info to get the default value for.</param>
        ''' <returns>
        ''' </returns>
        ''' <remarks>
        ''' JS 06Mar09: not implemented yet; need to figure out how to get to
        ''' the actual 'Default' attribute.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Function OOPGetPropertyDefaultValue(ByVal pi As PropertyInfo) As Object
            Dim pd As PropertyDescriptor = cPropertyConverter.FindOrigPropertyDescriptor(pi)
            Return Nothing
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Assembly name cache, added to optimize the process of relocating types
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private m_dtAssemblyNames As New Dictionary(Of String, Assembly)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, translats a class/type to a unique string.
        ''' </summary>
        ''' <param name="t">The class to convert.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The counterpart of this method, <see cref="OOPStringToType">OOPStringToType</see>,
        ''' can be used to find the originating type from a string.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Function OOPTypeToString(ByVal t As Type) As String

            ' Include assembly short name in the type name. This enables
            ' the OOP database logic to relocate the type from its original
            ' assembly, even if similar class names exist in similar namespaces
            ' in different asssemblies. Yes, it's far fetched, but hey...
            Return t.Assembly.GetName.Name + "!" + t.FullName()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, locates the originating type from a type string.
        ''' </summary>
        ''' <param name="strType">The type name to locate the originating type
        ''' for.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The counterpart of this method, <see cref="OOPTypeToString">OOPTypeToString</see>,
        ''' can be used to create the string for a type.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Function OOPStringToType(ByVal strType As String) As Type

            ' Split assembly short name from type name
            Dim astr As String() = strType.Split(CChar("!"))
            Dim ass As Assembly = Nothing

            ' Optimization: cache assembly names
            If m_dtAssemblyNames.Count = 0 Then
                For Each ass In AppDomain.CurrentDomain.GetAssemblies()
                    Try
                        If Not ass.FullName.StartsWith("Microsoft") Then
                            m_dtAssemblyNames.Add(ass.GetName.Name, ass)
                        End If
                    Catch ex As Exception
                        ' Ignore assembly confusion 
                    End Try
                Next
            End If

            ' Try to find type name in the named assembly 
            Try
                ass = Me.m_dtAssemblyNames(astr(0))
                ' Found assembly! Now return the contained type (fingers crossed)
                Return ass.GetType(astr(1))
            Catch ex As Exception

            End Try
            Return Nothing

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, determines if the value type of a given property 
        ''' is, or is inherited from, cOOPStorable.
        ''' </summary>
        ''' <param name="pi">The property info instance to test.</param>
        ''' <returns>
        ''' True if the value type of a given property is, or is inherited from,
        ''' cOOPStorable.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Private Function OOPIsForeignKeyProperty(ByVal pi As PropertyInfo) As Boolean
            ' Is a ref to another cOOPStorable?
            If GetType(cOOPStorable).IsAssignableFrom(pi.PropertyType) Then
                ' Is NOT an indexed prop
                Return (pi.GetIndexParameters.Length = 0)
            End If
            Return False
        End Function

#End Region ' Helpers

#Region " Read "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Read a single cOOPStorable-derived instance from the database.
        ''' </summary>
        ''' <param name="t">The class/type of the object to read.</param>
        ''' <param name="iDBID">The database ID of the object to read.</param>
        ''' <param name="piKey">The property in the <paramref name="t">given type</paramref> 
        ''' where the <paramref name="iDBID">database ID</paramref> should be stored.</param>
        ''' <returns>
        ''' A cOOPStorable-derived instance, or nothing if an error occurred.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Private Function OOPReadObject(ByVal t As Type, ByVal iDBID As Integer, ByVal piKey As PropertyInfo) As cOOPStorable

            Dim objRead As cOOPStorable = Nothing
            Try
                objRead = CType(System.Activator.CreateInstance(t), cOOPStorable)
                objRead.DBID = iDBID
            Catch ex As Exception
                Return Nothing
            End Try

            ' Able to read the object with the primary key?
            If Me.OOPReadObjectRecursive(t, objRead, piKey, iDBID) Then
                ' #Yes: remember the instance
                Me.m_OOPObjectCache.AddObject(objRead)
                ' Return the object that was successfully read
                Return objRead
            Else
                ' Report failure
                Return Nothing
            End If
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Recursively read a single cOOPStorable-derived instance from the 
        ''' database along the inheritance tree.
        ''' </summary>
        ''' <param name="t">The type in the class hierarchy of instance 
        ''' <paramref name="objRead">objRead</paramref> to read.</param>
        ''' <param name="objRead">Object to read.</param>
        ''' <param name="piKey">Property in the object that holds the database
        ''' ID.</param>
        ''' <param name="iDBID">Database ID value of the object being read.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function OOPReadObjectRecursive(ByVal t As Type, ByVal objRead As cOOPStorable, ByVal piKey As PropertyInfo, ByVal iDBID As Integer) As Boolean

            Dim api As PropertyInfo() = Me.OOPGetStorableProperties(t)
            Dim strTable As String = Me.OOPGetTableName(t)
            Dim strColumnName As String = Me.OOPGetColumnName(piKey)
            Dim strColumnType As String = Me.OOPGetColumnName(piKey)
            Dim strValue As String = ""
            Dim strSQL As String = ""
            Dim reader As IDataReader = Nothing
            Dim bIsBaseClass As Boolean = False
            Dim bSucces As Boolean = True

#If VERBOSE_LEVEL >= 2 Then
            Console.WriteLine("Reading {0}.{1}", strTable, iDBID)
#End If

            ' Not the base class?
            If Not Me.OOPIsBaseClass(t) Then
                ' #Indeed: read base class first
                bSucces = bSucces And Me.OOPReadObjectRecursive(t.BaseType, objRead, piKey, iDBID)
            End If

            ' All good so far?
            If bSucces Then
                ' #Yes: read this specific class from the database
                strSQL = String.Format("SELECT * FROM {0} WHERE DBID={1}", strTable, iDBID)
                reader = Me.GetReader(strSQL)
                Try
                    ' Grab one single record
                    reader.Read()

                    ' For all properties in the given type
                    For Each pi As PropertyInfo In api
                        ' Extract database equivalents
                        strColumnName = Me.OOPGetColumnName(pi)
                        strColumnType = Me.OOPGetColumnType(pi)

                        ' Is the data type of this particular property supported?
                        If Not String.IsNullOrEmpty(strColumnType) Then
                            ' Is this NOT the primary key?
                            If String.Compare("DBID", strColumnName) <> 0 Then
                                ' Is this a foreign key property?
                                If Me.OOPIsForeignKeyProperty(pi) Then
                                    ' #Yes: read FK
                                    Try
                                        ' Get FK ID
                                        Dim iLinkedDBID As Integer = CInt(reader(strColumnName))
                                        Dim objFK As cOOPStorable = Nothing

                                        ' Has object attached?
                                        If iLinkedDBID > 0 Then
                                            ' FK object not read yet?
                                            If Not Me.m_OOPObjectCache.HasObject(iLinkedDBID) Then
                                                ' #Yes: read object into cache
                                                If Me.ReadObject(Me.ReadObjectKey(iLinkedDBID)) Is Nothing Then
#If VERBOSE_LEVEL >= 1 Then
                                                    Console.WriteLine("Read: fk object {0} failed to load for {1}.{2}", iLinkedDBID, strColumnName, strTable)
#End If
                                                    ' Links could not be restored
                                                    Return False
                                                End If
                                            End If
                                            ' Get the object
                                            objFK = Me.m_OOPObjectCache.GetObject(iLinkedDBID)
                                        End If
                                        ' Store FK
                                        pi.SetValue(objRead, objFK, Nothing)

                                    Catch ex As Exception
                                        Console.WriteLine("Read: failed to read FK {0}.{1}: {2}", strColumnName, strTable, ex.Message)
                                        bSucces = False
                                    End Try
                                Else ' Me.OOPIsForeignKeyProperty(pi)
                                    ' #No: just read the property value
                                    Dim bRead As Boolean = False
                                    If Me.HasColumn(reader, strColumnName) Then
                                        Try
                                            ' Special cases
                                            If pi.PropertyType Is GetType(Boolean) Then
                                                pi.SetValue(objRead, Convert.ToBoolean(reader(strColumnName)), Nothing)
                                            Else
                                                pi.SetValue(objRead, Me.ReadSafe(reader, strColumnName, Nothing), Nothing)
                                            End If
                                        Catch ex As Exception
                                            bRead = False
                                        End Try
                                    End If

                                    If Not bRead Then
                                        ' ToDo: assign property default value (which can be obtained from pi.Attributes
                                        'pi.SetValue(objRead, pi.Attributes, Nothing)
                                        'Console.WriteLine("Read: skipped col {0}.{1} ({2})", strColumnName, strTable, strColumnType)
                                    End If
                                End If
                            End If
                        End If
                    Next

                Catch ex As Exception
                    Console.WriteLine("Read: error when reading {0}: {1}", strTable, ex.Message)
                    bSucces = False
                End Try

                Me.ReleaseReader(reader)
                reader = Nothing

            End If

            If GetType(cOOPStorableList).Equals(t) Then
                bSucces = bSucces And Me.OOPReadListItems(DirectCast(objRead, cOOPStorableList))
            End If

            Return bSucces
        End Function

        ''' <summary>
        ''' Helper method, write contents of list 
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function OOPReadListItems(ByVal list As cOOPStorableList) As Boolean

            Dim strTable As String = "cOOPStorableListItems"
            Dim strSQL As String = ""
            Dim reader As IDataReader = Nothing
            Dim item As cOOPStorable = Nothing
            Dim key As cOOPKey = Nothing
            Dim bSucces As Boolean = True

#If VERBOSE_LEVEL >= 2 Then
            Console.WriteLine("Reading list items {0}", list.DBID)
#End If

            strSQL = String.Format("SELECT * FROM {0} WHERE DBID={1}", strTable, list.DBID)
            reader = Me.GetReader(strSQL)

            Try
                While reader.Read
                    key = Me.ReadObjectKey(CInt(reader("item")))
                    item = Me.ReadObject(key)
                    If item IsNot Nothing Then
                        list.Add(item)
                    End If
                End While
            Catch ex As Exception
                bSucces = False
                Console.WriteLine("Error {0} reading list {1}", ex.Message, list.DBID)
            End Try
            Return bSucces
        End Function

#End Region ' Read

#Region " Write "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, write contents of a list.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' -------------------------------------------------------------------
        Private Function OOPWriteListItems(ByVal list As cOOPStorableList) As Boolean

            Dim adapter As IDataAdapter = Nothing
            Dim item As cOOPStorable = Nothing
            Dim strTable As String = "cOOPStorableListItems"
            Dim drow As DataRow = Nothing
            Dim iRow As Integer = 0
            Dim nRows As Integer = 0
            Dim ds As DataSet = Nothing
            Dim dt As DataTable = Nothing
            Dim bSucces As Boolean = True

            adapter = Me.OOPGetAdapter(strTable)

            ' Clear list from DB
            ds = Me.GetDataSet(adapter, strTable)
            dt = ds.Tables(0)
            nRows = dt.Rows.Count
            iRow = 0

            ' Remove current rows for the list
            While iRow < nRows - 1
                drow = dt.Rows(iRow)
                If CInt(drow("DBID")) = list.DBID Then
                    dt.Rows.RemoveAt(iRow) : nRows -= 1
                Else
                    iRow += 1
                End If
            End While

            ' Write new items
            For iItem As Integer = 0 To list.Count - 1
                item = list(iItem)
                bSucces = bSucces And Me.WriteObject(item)
                If bSucces Then
                    drow = dt.NewRow()
                    drow("DBID") = list.DBID
                    drow("Item") = item.DBID
                    dt.Rows.Add(drow)
                End If
            Next iItem

            Me.CommitDataSet(ds, adapter, strTable)

            Me.OOPReleaseAdapter(strTable)

            ds = Nothing
            dt = Nothing
            adapter = Nothing

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="obj"></param>
        ''' <param name="piKey"></param>
        ''' <returns></returns>
        Private Function OOPWriteObjectRecursive(ByVal t As Type, ByVal obj As cOOPStorable, ByVal piKey As PropertyInfo) As Boolean

            Dim api As PropertyInfo() = Me.OOPGetStorableProperties(t)
            Dim strTable As String = Me.OOPGetTableName(t)
            Dim strColumnName As String = ""
            Dim strColumnType As String = ""
            Dim adapter As IDataAdapter = Nothing
            Dim ds As DataSet = Nothing
            Dim dt As DataTable = Nothing
            Dim drow As DataRow = Nothing
            Dim bNewRow As Boolean = False
            Dim bIsBaseClass As Boolean = Me.OOPIsBaseClass(t)
            Dim bSucces As Boolean = True

            ' Not the base class?
            If Not bIsBaseClass Then
                bSucces = Me.OOPWriteObjectRecursive(t.BaseType, obj, piKey)
            End If

            If bSucces Then

                Try
                    adapter = Me.OOPGetAdapter(strTable)

                    ds = Me.GetDataSet(adapter, Me.OOPGetTableName(t))
                    dt = ds.Tables(0)

                    drow = dt.Rows.Find(piKey.GetValue(obj, Nothing))

                    bNewRow = (drow Is Nothing)
                    If bNewRow Then
                        drow = dt.NewRow()
                        If bIsBaseClass Then
                            ' Write baseclass class name
                            drow(OOP_CLASSNAMECOL) = Me.OOPTypeToString(obj.GetType())
                        End If
                    Else
                        drow.BeginEdit()
                    End If

                    For Each pi As PropertyInfo In api
                        strColumnName = Me.OOPGetColumnName(pi)
                        strColumnType = Me.OOPGetColumnType(pi)

                        ' Is column type supported?
                        If Not String.IsNullOrEmpty(strColumnType) Then
                            ' #Yes: is this a foreign key?
                            If Me.OOPIsForeignKeyProperty(pi) Then
                                ' #Yes: write foreign key value
                                Dim objFK As cOOPStorable = DirectCast(pi.GetValue(obj, Nothing), cOOPStorable)
                                Dim iDBIDFK As Integer = 0
                                ' Has linked object attached?
                                If (objFK IsNot Nothing) Then
                                    ' #Yes: get DBID for linked object. 
                                    '     ! Note that this ID might not yet be assigned
                                    iDBIDFK = objFK.DBID
                                    ' Test if referenced object needs to be stored first
                                    If Not Me.m_OOPObjectCache.HasObject(iDBIDFK) Then
                                        ' Write linked object
                                        If Me.WriteObject(objFK) Then
                                            ' Just in case, obtain DBID again in case WriteObject assigned this
                                            iDBIDFK = objFK.DBID
                                        Else
#If VERBOSE_LEVEL >= 1 Then
                                            Console.WriteLine("Unable to write FK object {0} when writing {1} as {2}", objFK.DBID, obj, strTable)
#End If
                                            iDBIDFK = 0
                                        End If
                                    End If
                                End If
                                ' Write FK key value
                                drow(strColumnName) = iDBIDFK
                            Else
                                ' #No: Just write supported value
                                drow(strColumnName) = pi.GetValue(obj, Nothing)
                            End If
                        Else
#If VERBOSE_LEVEL >= 2 Then
                            Console.WriteLine("Column type {0} not supported when writing {1} as {2}", strColumnName, obj, strTable)
#End If
                        End If
                    Next

                    If bNewRow Then dt.Rows.Add(drow) Else drow.EndEdit()

                Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                    Console.WriteLine("Error {0} while saving {1} as {2}", ex.Message, obj, t.Name)
#End If
                    bSucces = False
                End Try

                Me.CommitDataSet(ds, adapter, strTable)

                Me.ReleaseDataSet(ds)
                Me.OOPReleaseAdapter(strTable)

            End If

            adapter = Nothing
            ds = Nothing
            dt = Nothing

            If GetType(cOOPStorableList).Equals(t) Then
                bSucces = bSucces And Me.OOPWriteListItems(DirectCast(obj, cOOPStorableList))
            End If

            Return bSucces
        End Function

#End Region ' Write

#End Region ' OOP internal helper methods

#End Region ' OOP

#Region " EwE versioning "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the current version of the connected EwE database.
        ''' </summary>
        ''' <returns>
        ''' A Single value with the version latest version number of the connected database.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function GetVersion() As Single

            If Me.m_sVersion = 0.0 Then
                Try
                    ' Try EwE6 version first
                    Me.m_sVersion = CSng(Me.GetValue("Select Max(Version) FROM [UpdateLog]"))
                    If (Me.m_sVersion = 0.0) Then
                        ' Try EwE5 version
                        Me.m_sVersion = CSng(Me.GetValue("Select Max(Version) FROM [Database specifications]"))
                    End If
                Catch ex As Exception
                    Me.m_sVersion = 0.0
                End Try
            End If
            Return Me.m_sVersion

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Updates the version of the database
        ''' </summary>
        ''' <param name="sVersion">The version to set.</param>
        ''' <param name="strRemark">The remark to add to the update.</param>
        ''' <returns>True if successful</returns>
        ''' <remarks>This method only allows setting the version on an EwE6 database.</remarks>
        ''' -------------------------------------------------------------------
        Public Function SetVersion(ByVal sVersion As Single, ByVal strRemark As String) As Boolean

            Dim version As Version = cAssemblyUtils.GetVersion()
            Dim dtNow As Date = Date.Now()
            Dim strSQL As String = ""

            If (sVersion < 6.120003!) Then
                strSQL = String.Format("INSERT INTO UpdateLog ([Version], [Remark], [Date]) VALUES('{0}', '{1}', '{2}')",
                                                 sVersion, strRemark, dtNow.ToShortDateString())
            Else
                strSQL = String.Format("INSERT INTO UpdateLog ([Version], [Remark], [Date], [EwEVersion]) VALUES('{0}', '{1}', '{2}', '{3}')",
                                                 sVersion, strRemark, dtNow.ToShortDateString(), version.ToString())
            End If
            Dim bSucces As Boolean = True
            Try
                bSucces = Me.Execute(strSQL)
                Me.m_sVersion = sVersion
            Catch ex As Exception
                bSucces = False
            End Try
            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Extract the major version number from a given version number.
        ''' </summary>
        ''' <param name="sVersion">The version number to examine.</param>
        ''' <returns>The major version number of the given version number.</returns>
        ''' <remarks>
        ''' <para>'6.0' returns '6'</para>
        ''' <para>'2.93' returns '2'</para>
        ''' <para>'-4.4' returns '4'</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function GetMajorVersion(ByVal sVersion As Single) As Single
            Return CSng(Math.Sign(sVersion) * Math.Floor(Math.Abs(sVersion)))
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Extract the minor version number from a given version number.
        ''' </summary>
        ''' <param name="sVersion">The version number to examine.</param>
        ''' <returns>The minor version number of the given version number.</returns>
        ''' <remarks>
        ''' <para>'6.0' returns '0.0'</para>
        ''' <para>'2.93' returns '0.93'</para>
        ''' <para>'-4.4' returns '0.4'</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function GetMinorVersion(ByVal sVersion As Single) As Single
            Dim sAbsVersion As Single = Math.Abs(sVersion)
            Return CSng(sAbsVersion - Math.Floor(sAbsVersion))
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Database change log item.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Class cHistoryItem
            Private m_strVersion As String
            Private m_strComments As String
            Private m_date As DateTime

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="strVersion"></param>
            ''' <param name="strComments"></param>
            ''' <param name="strDate"></param>
            ''' <remarks></remarks>
            Friend Sub New(ByVal strVersion As String, ByVal strComments As String, ByVal strDate As String)
                Me.m_strVersion = strVersion
                Me.m_strComments = strComments
                Me.m_date = Date.Parse(strDate)
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the version number of a particular history item.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property Version() As String
                Get
                    Return Me.m_strVersion
                End Get
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the comments to a particular history item.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property Comments() As String
                Get
                    Return Me.m_strComments
                End Get
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the date of a particular history item.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property [Date]() As DateTime
                Get
                    Return Me.m_date
                End Get
            End Property

        End Class

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the change log of the database.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function GetHistory() As cHistoryItem()
            Dim lHistory As New List(Of cHistoryItem)
            Dim item As cHistoryItem = Nothing
            Dim r As IDataReader = Me.GetReader("SELECT * FROM UpdateLog ORDER BY Date ASC, Version ASC")
            While r.Read
                Try
                    item = New cHistoryItem(CStr(r("Version")), CStr(r("Remark")), CStr(r("Date")))
                    lHistory.Add(item)
                Catch ex As Exception
                    ' Whoah! Unable to parse the date?!
                End Try
            End While
            Return lHistory.ToArray
        End Function

#End Region ' EwE versioning

    End Class

End Namespace
