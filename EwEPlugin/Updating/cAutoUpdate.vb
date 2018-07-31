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
Imports System.Net
Imports System.IO
Imports System.Reflection
Imports System.Diagnostics
Imports EwEUtils.Utilities
Imports System.Security.Cryptography
Imports System.Windows.Forms
Imports EwEUtils.Core

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Helper class to update a plug-in assembly from the EwE web service.
''' </summary>
''' <remarks>
''' <para>The cAutoUpdate class should be used as follows:</para>
''' <code>
''' 
''' </code>
''' </remarks>
''' ===========================================================================
Friend Class cAutoUpdate

#Region " Private vars "

    ''' <summary>The update service.</summary>
    Private m_service As EwEAutoUpdateRef.UpdateService = Nothing
    ''' <summary>Update session cookies.</summary>
    Private m_cookiejar As CookieContainer = Nothing

    ''' <summary>Attached file name.</summary>
    Private m_strFile As String = ""

    ''' <summary>Attached core version.</summary>
    Private m_verCore As Version = Nothing
    ''' <summary>Attached plug-in version.</summary>
    Private m_verPlugin As Version = Nothing
    ''' <summary>Attached plug-in short file name.</summary>
    Private m_strPluginName As String = ""
    ''' <summary>Attached plug-in public hash key token.</summary>
    ''' <remarks>For strong-named assemblies only.</remarks>
    Private m_strPluginToken As String = ""

#End Region ' Private vars

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new cAutoUpdate instance.
    ''' </summary>
    ''' <param name="core">The core assembly to download updates for.</param>
    ''' <param name="iTimeOut">Number of miliseconds to wait before timing out.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As Object, iTimeOut As Integer)

        Me.m_verCore = Me.CoreVersion(core)
        Me.m_cookiejar = New CookieContainer()
        Me.m_service = New EwEAutoUpdateRef.UpdateService()
        Me.m_service.CookieContainer = Me.m_cookiejar
        Me.m_service.Timeout = iTimeOut

    End Sub

#End Region ' Constructor

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Attach a file to the updater.
    ''' </summary>
    ''' <param name="strFile"></param>
    ''' <returns>True if this is a valid assembly.</returns>
    ''' -----------------------------------------------------------------------
    Public Function AttachAssembly(ByVal strFile As String) As Boolean

        Dim assemPlugin As AssemblyName = Nothing

        ' Reset
        Me.m_strFile = ""

        Try
            assemPlugin = AssemblyName.GetAssemblyName(strFile)
        Catch ex As Exception
            assemPlugin = Nothing
        End Try

        If (assemPlugin Is Nothing) Then
            Return False
        End If

        Try
            ' Grab details
            Me.m_strPluginName = cAssemblyUtils.GetName(assemPlugin)
            Me.m_strPluginToken = cAssemblyUtils.GetToken(assemPlugin)
            Me.m_verPlugin = cAssemblyUtils.GetVersion(assemPlugin)
        Catch e As Exception
            Return False
        End Try

        ' Remember file
        Me.m_strFile = strFile

        ' Ok
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Check for updates on the attached assembly.
    ''' </summary>
    ''' <para>An <see cref="eAutoUpdateResultTypes">update status</see>flag, which 
    ''' is to be interpreted as follows:</para>
    ''' <list type="table">
    ''' <listheader><term>Flag</term><description>Description</description></listheader>
    ''' <item>
    ''' <term><see cref="eAutoUpdateResultTypes.Success_NoActionRequired"/></term>
    ''' <description>Server was contacted successfully and no action is required.</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="eAutoUpdateResultTypes.Success_Updated"/></term>
    ''' <description>Server was contacted successfully and no action is required.</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="eAutoUpdateResultTypes.Info_CanMigrate"/></term>
    ''' <description>A migration from a weak-named to a strong-named assembly is available.</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="eAutoUpdateResultTypes.Info_CanUpdate"/></term>
    ''' <description>An update is available.</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="eAutoUpdateResultTypes.Error_Connection"/></term>
    ''' <description>Connection to update server could not be established.</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="eAutoUpdateResultTypes.Error_Generic"/></term>
    ''' <description>Something else went wrong.</description>
    ''' </item>
    ''' </list>
    ''' -----------------------------------------------------------------------
    Public Function CheckForUpdate() As eAutoUpdateResultTypes

        If String.IsNullOrEmpty(Me.m_strPluginName) Then
            Return eAutoUpdateResultTypes.Error_Generic
        End If

        ' Perform local version check first
        If Me.m_verPlugin.CompareTo(Me.m_verCore) >= 0 Then
            Return eAutoUpdateResultTypes.Success_NoActionRequired
        End If

        ' For weak-named assemblies check for a likely migration
        If String.IsNullOrEmpty(Me.m_strPluginToken) Then
            Return Me.HasMigration()
        End If

        ' Return whether an update is available
        Return Me.HasUpdate()

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Download an update for a file.
    ''' </summary>
    ''' <returns>
    ''' <para>An <see cref="eAutoUpdateResultTypes">update result indicator</see>,
    ''' which are to be interpreted as follows:</para>
    ''' <list type="table">
    ''' <listheader><term>Flag</term><description>Description</description></listheader>
    ''' <item>
    ''' <term><see cref="eAutoUpdateResultTypes.Success_Updated"/></term>
    ''' <description>Update was downloaded and copied succesfully.</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="eAutoUpdateResultTypes.Error_Download"/></term>
    ''' <description>Failed to correctly download the update.</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="eAutoUpdateResultTypes.Error_Replace"/></term>
    ''' <description>Failed to replace the local plug-in file with the downloaded file.</description>
    ''' </item>
    ''' </list>
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Function DownloadUpdate() As eAutoUpdateResultTypes

        Dim abPlugin() As Byte = Nothing
        Dim fsPlugin As FileStream = Nothing
        Dim strTemp As String = Path.GetTempFileName()
        Dim md5Hash As MD5 = MD5.Create()
        Dim strHashLocal As String = ""

        If String.IsNullOrEmpty(Me.m_strPluginName) Then
            Return eAutoUpdateResultTypes.Error_Generic
        End If

        Try
            ' Download to a temp location
            abPlugin = Me.m_service.DownloadPlugin()
            fsPlugin = New FileStream(strTemp, FileMode.Create)
            fsPlugin.Write(abPlugin, 0, abPlugin.Length)
            fsPlugin.Close()
            fsPlugin = Nothing
        Catch ex As Exception
            ' Error downloading update
            Return eAutoUpdateResultTypes.Error_Download
        End Try

        Try
            ' Calculate local checksum
            strHashLocal = cStringUtils.ToHexString(md5Hash.ComputeHash(abPlugin))

            ' Does checksum match the service checksum?
            If Not String.Compare(strHashLocal, Me.m_service.GetPluginHash(), True) = 0 Then
                ' #No: download failed
                Return eAutoUpdateResultTypes.Error_Download
            End If
        Catch ex As Exception
            ' Error downloading hash
            Return eAutoUpdateResultTypes.Error_Download
        End Try

        Try
            ' Replace plug-in file
            File.Copy(strTemp, Me.m_strFile, True)
        Catch ex As Exception
            ' Unable to overwrite plug-in dll, maybe it's in use?
            Return eAutoUpdateResultTypes.Error_Replace
        End Try

        Try
            ' Delete temp file
            File.Delete(strTemp)
        Catch ex As Exception
            ' Hmm, ok, allow this
        End Try

        ' Yippee
        Return eAutoUpdateResultTypes.Success_Updated

    End Function

#End Region ' Public access

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, get the version of the core assembly.
    ''' </summary>
    ''' <param name="core">The core object to query the assembly for.</param>
    ''' -----------------------------------------------------------------------
    Private ReadOnly Property CoreVersion(ByVal core As Object) As Version
        Get
            Dim anCore As AssemblyName = cAssemblyUtils.GetAssemblyName(core.GetType())
            Return cAssemblyUtils.GetVersion(anCore)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, states whether a migration is available for the attached 
    ''' file.
    ''' </summary>
    ''' <returns>
    ''' <para>This method will return one of the following values:</para>
    ''' <list type="table">
    ''' <item><term><see cref="eAutoUpdateResultTypes.Error_Connection"/></term><description>Server could not be connected.</description></item>
    ''' <item><term><see cref="eAutoUpdateResultTypes.Info_CanUpdate"/></term><description>Server was contacted and a migration is available.</description></item>
    ''' <item><term><see cref="eAutoUpdateResultTypes.Success_NoActionRequired"/></term><description>Server was contacted but no migration is available.</description></item>
    ''' </list>
    ''' </returns>
    ''' <remarks>
    ''' Note that this check should only be performed on weak-named assemblies.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Function HasMigration() As eAutoUpdateResultTypes

        Debug.Assert(String.IsNullOrEmpty(Me.m_strPluginToken), "Assembly is not weak-named")
        Debug.Assert(Me.m_verCore IsNot Nothing, "Something is VERY wrong")
        Debug.Assert(Me.m_verPlugin IsNot Nothing, "Something is VERY wrong")

        Try
            Me.m_strPluginToken = Me.m_service.GetPluginMigrationToken(Me.m_verCore.ToString, Me.m_strPluginName, Me.m_verPlugin.ToString)

            If Not String.IsNullOrEmpty(Me.m_strPluginToken) Then
                Return eAutoUpdateResultTypes.Info_CanMigrate
            End If

            Return eAutoUpdateResultTypes.Success_NoActionRequired

        Catch ex As Exception
            ' Unable to connect to server
        End Try

        Return eAutoUpdateResultTypes.Error_Connection

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, states if an update is available for a given assembly.
    ''' </summary>
    ''' <returns>
    ''' <para>This method will return one of the following values:</para>
    ''' <list type="table">
    ''' <item><term><see cref="eAutoUpdateResultTypes.Error_Connection"/></term><description>Server could not be connected.</description></item>
    ''' <item><term><see cref="eAutoUpdateResultTypes.Info_CanUpdate"/></term><description>Server was contacted and an update is available.</description></item>
    ''' <item><term><see cref="eAutoUpdateResultTypes.Success_NoActionRequired"/></term><description>Server was contacted but no update is available.</description></item>
    ''' </list>
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Private Function HasUpdate() As eAutoUpdateResultTypes

        Debug.Assert(Me.m_verCore IsNot Nothing, "Something is VERY wrong")
        Debug.Assert(Me.m_verPlugin IsNot Nothing, "Something is VERY wrong")

        Dim result As eAutoUpdateResultTypes = eAutoUpdateResultTypes.Success_NoActionRequired
        Try
            If Me.m_service.CheckPluginUpdate(Me.m_verCore.ToString, Me.m_strPluginName, Me.m_strPluginToken, Me.m_verPlugin.ToString) Then
                result = eAutoUpdateResultTypes.Info_CanUpdate
            End If
        Catch ex As WebException
            ' Unable to connect to server
            result = eAutoUpdateResultTypes.Error_Connection
        Catch ex As Exception
            ' Something was strange, but not to worry
            result = eAutoUpdateResultTypes.Success_NoEwEComponent
        End Try
        Return result

    End Function

#End Region ' Internals

End Class
