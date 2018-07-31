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
Imports EwECore.Database
Imports EwEUtils.Core
Imports EwEUtils.Database
Imports EwEUtils.SystemUtilities

#End Region ' Imports

Namespace DataSources

    ''' =======================================================================
    ''' <summary>
    ''' Factory for creating data sources
    ''' </summary>
    ''' =======================================================================
    Public Class cDataSourceFactory

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns an EwE <see cref="eDataSourceTypes">datasource type</see> that
        ''' will be able to interact with the provided file name.
        ''' </summary>
        ''' <param name="strFile">Name of the file.</param>
        ''' <returns>A <see cref="eDataSourceTypes">datasource type</see>
        ''' indicating what type of EwE datasource will be able to interact with
        ''' the provided file name.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetSupportedType(ByVal strFile As String) As eDataSourceTypes

            Select Case Path.GetExtension(strFile).ToLower

                Case ".eii"
                    Return eDataSourceTypes.EII

                Case ".accdb", ".eweaccdb"
                    Return eDataSourceTypes.Access2007

                Case ".mdb", ".ewemdb"
                    If cSystemUtils.Is64BitProcess() Then
                        Return eDataSourceTypes.Access2007
                    Else
                        Return eDataSourceTypes.Access2003
                    End If

                Case ".eiixml"
                    Return eDataSourceTypes.EIIXML

            End Select

            ' Explore URL protocols
            Dim i As Integer = strFile.IndexOf(":"c)

            ' Is probably a URL protocol?
            If (i > 0) Then
                Select Case strFile.Substring(0, i)
                    Case "ewe-ecobase"
                        Return eDataSourceTypes.EcoBase
                End Select
            End If

            Return eDataSourceTypes.NotSet

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the default extension for a given <see cref="eDataSourceTypes">datasource type</see>.
        ''' </summary>
        ''' <param name="dst">The <see cref="eDataSourceTypes">datasource type</see> to query.</param>
        ''' <returns>A string providing a file extension, or an empty string if
        ''' the given datasource type is not supported.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetDefaultExtension(ByVal dst As eDataSourceTypes) As String
            Select Case dst
                Case eDataSourceTypes.Access2003 : Return ".ewemdb"
                Case eDataSourceTypes.EII : Return ".eii"
                Case eDataSourceTypes.Access2007 : Return ".eweaccdb"
                Case eDataSourceTypes.EIIXML : Return ".eiixml"
            End Select
            Return ""
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the compatibility of a given database with the current code.
        ''' </summary>
        ''' <param name="strDatabase"></param>
        ''' <param name="access">Flag that must state whether the database can be accessed.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function GetCompatibility(ByVal strDatabase As String, ByRef access As eDatasourceAccessType) As cEwEDatabase.eCompatibilityTypes

            Dim comp As cEwEDatabase.eCompatibilityTypes = cEwEDatabase.eCompatibilityTypes.Unknown
            Dim dst As eDataSourceTypes = cDataSourceFactory.GetSupportedType(strDatabase)

            ' Detect file type
            Select Case dst

                Case eDataSourceTypes.Access2007, eDataSourceTypes.Access2003

                    If File.Exists(strDatabase) Then
                        Dim db As New cEwEAccessDatabase()
                        access = db.Open(strDatabase)
                        If (access = eDatasourceAccessType.Opened) Then
                            comp = db.Compatibility
                            db.Close()
                        End If
                    Else
                        access = eDatasourceAccessType.Failed_FileNotFound
                    End If

                Case eDataSourceTypes.EII, eDataSourceTypes.EIIXML

                    If File.Exists(strDatabase) Then
                        comp = cEwEDatabase.eCompatibilityTypes.EwE6
                        access = eDatasourceAccessType.Opened
                    Else
                        access = eDatasourceAccessType.Failed_FileNotFound
                    End If

                Case eDataSourceTypes.EcoBase

                    If cSystemUtils.IsConnectedToInternet Then
                        comp = cEwEDatabase.eCompatibilityTypes.Importable
                        access = eDatasourceAccessType.Success
                    Else
                        ' ToDo: create explicit enum value Failed_NoInternet
                        access = eDatasourceAccessType.Failed_FileNotFound
                    End If

                Case eDataSourceTypes.NotSet

                    comp = cEwEDatabase.eCompatibilityTypes.Unknown
                    access = eDatasourceAccessType.Failed_Unknown

            End Select

            Return comp

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a data source onto an existing <see cref="cEwEDatabase">EwE database</see>.
        ''' </summary>
        ''' <param name="db"><see cref="cEwEDatabase">cEwEDatabase</see> to create a datasource for.</param>
        ''' <param name="ds">The newly created datasource.</param>
        ''' <returns>A <see cref="eStatusFlags">Status flag</see> that indicates the valid</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function Create(ByRef db As cEwEDatabase, ByRef ds As IEwEDataSource) As eStatusFlags

            Dim nResult As eStatusFlags = eStatusFlags.OK

            If TypeOf db Is cEwEAccessDatabase Then
                ' Create a DB datasource on a MS Access database
                ds = New cDBDataSource(db)
            End If
            Return nResult

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a data source for a given <see cref="eDataSourceTypes">type of EwE datasource</see>.
        ''' </summary>
        ''' <param name="dst"><see cref="eDataSourceTypes">Type of EwE datasource</see> to create.</param>
        ''' <returns>A <see cref="IEwEDataSource">IEwEDataSource</see> or 
        ''' Nothing if creation failed</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function Create(ByVal dst As eDataSourceTypes) As IEwEDataSource

            Dim nResult As eStatusFlags = eStatusFlags.OK

            Select Case dst

                Case eDataSourceTypes.EII
                    Return New cEIIDataSource()

                Case eDataSourceTypes.EIIXML
                    Return New cEIIXMLDataSource()

                Case eDataSourceTypes.Access2003, _
                     eDataSourceTypes.Access2007
                    ' Create a DB datasource on a MS Access database
                    Return New cDBDataSource(New cEwEAccessDatabase())

                Case Else
                    '
            End Select

            'Failure
            Return Nothing

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a data source for a given file name.
        ''' </summary>
        ''' <param name="strFileName">The file to create the data source for.</param>
        ''' <returns>A <see cref="IEwEDataSource">IEwEDataSource</see> or 
        ''' Nothing if creation failed</returns>
        ''' <remarks>The factory will attempt to decipher from the file name
        ''' which <see cref="eDataSourceTypes">type of EwE datasource</see>
        ''' is requred.</remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function Create(ByVal strFileName As String) As IEwEDataSource

            Return Create(GetSupportedType(strFileName))

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the operating system supports a given type of EwE 
        ''' <see cref="eDataSourceTypes">data source</see>.
        ''' </summary>
        ''' <param name="dst">The type of EwE <see cref="eDataSourceTypes">data source</see>
        ''' to test.</param>
        ''' <returns>True if the system appears to support the given type of
        ''' data source. The check is implemented by the actual data sources. 
        ''' Implementations can range from simple file checks to online driver 
        ''' validations.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function IsOSSupported(ByVal dst As eDataSourceTypes) As Boolean

            Dim ds As IEwEDataSource = cDataSourceFactory.Create(dst)
            If ds Is Nothing Then Return False
            Return ds.IsOSSupported(dst)

        End Function

    End Class

End Namespace ' DataSources
