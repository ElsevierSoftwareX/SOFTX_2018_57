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
Imports System.Collections.Specialized
Imports System.Configuration
Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms
Imports System.Xml
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' <para>Custom <see cref="SettingsProvider"/> to affect a few of the default
''' and messy .NET settings behaviours:</para>
''' <list type="bullet">
''' <item>All EwE settings are stored in the local roaming profile of the current
''' user. This class removes the distinction between application settings and user 
''' settings which traditionally end up in different directories on a system.</item>
''' <item>This class also ensures that all EwE settings are stored in one directory, 
''' in one file only. This stops the proliferation of standard .NET versioned setting
''' directories that become impossible to manage.</item>
''' </list>
''' </summary>
''' <remarks>
''' <para>The following code illustrates how to use this class to store settings
''' native to a class library:</para>
''' <code>
''' Imports System.IO
''' Imports EwEUtils
''' Imports System.Reflection
''' 
''' Namespace My
''' 
''' Partial Friend NotInheritable Class MySettings
''' 
'''     Private m_provider As cEwESettingsProvider = Nothing
''' 
'''     Public Sub New()
''' 
'''         MyBase.New()
'''         Dim asm As Assembly = Assembly.GetAssembly(GetType(MySettings))
'''         Me.m_provider = New cEwESettingsProvider(Path.GetFileNameWithoutExtension(asm.Location), Me)
''' 
'''     End Sub
''' 
''' End Class
''' 
''' End Namespace
''' </code>
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class cEwESettingsProvider
    Inherits SettingsProvider

#Region " Private parts "

    Private Const cSETTINGSROOT As String = "Settings" 'XML Root Node
    Private m_xmldocSettings As Xml.XmlDocument = Nothing
    Private m_strAssembly As String = ""
    Private m_settings As ApplicationSettingsBase = Nothing

#End Region ' Private parts

    Public Sub New(strAssembly As String, settings As ApplicationSettingsBase)

        Debug.Assert(settings IsNot Nothing)

        Me.m_strAssembly = strAssembly
        Me.m_settings = settings

        ' Eradicate existing providers
        settings.Providers.Clear()
        ' Add custom provider
        settings.Providers.Add(Me)
        ' Hijack all existing properties
        For Each sp As SettingsProperty In settings.Properties
            sp.Provider = Me
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialization. Overridden to stop .NET from trying to be too smart.
    ''' </summary>
    ''' <param name="strName"></param>
    ''' <param name="col"></param>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub Initialize(ByVal strName As String, ByVal col As NameValueCollection)
        MyBase.Initialize(Me.ApplicationName, col)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the default value for a given settings property name.
    ''' </summary>
    ''' <param name="strName">The name of the property to access. This name is not case-sensitive.</param>
    ''' <returns>A value, or Nothing if a property by this name does not exist.</returns>
    ''' -----------------------------------------------------------------------
    Public Function GetDefaultValue(ByVal strName As String) As Object
        Dim prop As SettingsProperty = Me.m_settings.Properties(strName)
        If prop IsNot Nothing Then Return prop.DefaultValue
        Return Nothing
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' I have no idea who uses this, but hey, I'll override anything you'll
    ''' tell me to.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property Name() As String
        Get
            Return "EwEProgramSettingsProvider"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Who are you?
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Property ApplicationName() As String
        Get
            Return Me.m_strAssembly
        End Get
        Set(ByVal value As String)
            'Do nothing
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Store the values of all properties.
    ''' </summary>
    ''' <param name="context"></param>
    ''' <param name="propvals"></param>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub SetPropertyValues(ByVal context As SettingsContext, ByVal propvals As SettingsPropertyValueCollection)

        ' Sanity check
        If propvals Is Nothing Then Return

        Try
            'Iterate through the settings to be stored
            'Only dirty settings are included in propvals, and only ones relevant to this provider
            For Each propval As SettingsPropertyValue In propvals
                StoreValue(propval)
            Next

            ' Save the document
            SettingsDoc.Save(IO.Path.Combine(GetAppSettingsPath, GetAppSettingsFilename))

        Catch ex As Exception
            'Ignore if can't save
        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the values of all properties.
    ''' </summary>
    ''' <param name="context"></param>
    ''' <param name="props"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function GetPropertyValues(ByVal context As SettingsContext, ByVal props As SettingsPropertyCollection) As SettingsPropertyValueCollection

        Dim values As SettingsPropertyValueCollection = New SettingsPropertyValueCollection()
        Dim value As SettingsPropertyValue = Nothing

        If props IsNot Nothing Then
            'Iterate through the settings to be retrieved
            For Each setting As SettingsProperty In props
                Try
                    value = New SettingsPropertyValue(setting)
                    value.IsDirty = False
                    value.SerializedValue = GetValue(setting)
                    values.Add(value)
                Catch ex As Exception
                    ' Yohoho
                End Try
            Next
        End If

        Return values

    End Function

#Region " Internal overridables "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get previous location of app settings file.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Protected Function GetAppSettingsPathOld() As String
        Dim fi As New System.IO.FileInfo(Application.ExecutablePath)
        Return fi.DirectoryName
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get location where to store settings file.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' .NET uses the ApplicationData structure for this. EwE6 instead stores this
    ''' value in an ApplicationData directory accessible to all versions of EwE.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Protected Overridable Function GetAppSettingsPath() As String
        Return cSystemUtils.ApplicationSettingsPath
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get name of settings file.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' .NET commonly tries to do very fancy things here, pertaining to merging
    ''' different versions of settings, and managing local and roaming settings.
    ''' EwE6 does not need any of that stuff; let's keep it simple.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Protected Overridable Function GetAppSettingsFilename() As String
        Return Me.ApplicationName & ".settings"
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the XML document to operate on.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overridable ReadOnly Property SettingsDoc() As Xml.XmlDocument
        Get

            Dim strSettingsDocPath As String = IO.Path.Combine(GetAppSettingsPath, GetAppSettingsFilename)
            Dim strSettingsDocPathOld As String = IO.Path.Combine(GetAppSettingsPathOld, GetAppSettingsFilename)
            Dim bFileRead As Boolean = False
            Dim decl As XmlDeclaration = Nothing
            Dim node As XmlNode = Nothing

            ' -----------------
            ' Settings file migration
            ' -----------------

            ' Has original (wrong) settings file?
            If File.Exists(strSettingsDocPathOld) Then
                ' #Yes: does NOT have new, correct settings file?
                If (Not File.Exists(strSettingsDocPath)) Then
                    Try
                        File.Copy(strSettingsDocPathOld, strSettingsDocPath)
                    Catch ex As Exception
                        ' Wow
                    End Try
                End If

                Try
                    File.Delete(strSettingsDocPathOld)
                Catch ex As Exception
                    ' Aargh
                End Try
            End If

            ' Is XML doc present?
            If (Me.m_xmldocSettings Is Nothing) Then
                ' #No: make one
                Me.m_xmldocSettings = New Xml.XmlDocument
                ' Does file exist?
                If File.Exists(strSettingsDocPath) Then
                    ' #Yes: try to read it
                    Try
                        ' Load file
                        Me.m_xmldocSettings.Load(strSettingsDocPath)
                        ' All good
                        bFileRead = True
                    Catch ex As Exception
                        ' Kaboom
                        bFileRead = False
                    End Try
                End If

                ' File not read yet?
                If (Not bFileRead) Then
                    ' #Yes: create new document
                    decl = Me.m_xmldocSettings.CreateXmlDeclaration("1.0", "utf-8", String.Empty)
                    Me.m_xmldocSettings.AppendChild(decl)

                    node = Me.m_xmldocSettings.CreateNode(XmlNodeType.Element, cSETTINGSROOT, "")
                    Me.m_xmldocSettings.AppendChild(node)
                End If
            End If

            ' Return prepared (and hopefully read) document
            Return Me.m_xmldocSettings

        End Get
    End Property

#End Region ' Internal overridables

#Region " Internal bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return a single value from the settings.
    ''' </summary>
    ''' <param name="sp"></param>
    ''' <returns>
    ''' A value in the form of a string, or an emtpy string if an error occurred.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Private Function GetValue(ByVal sp As SettingsProperty) As String

        Dim strValue As String = ""
        Dim node As XmlNode = Nothing

        If (sp IsNot Nothing) Then

            Try
                node = SettingsDoc.SelectSingleNode(cSETTINGSROOT & "/" & sp.Name)
                If (node IsNot Nothing) Then
                    strValue = node.InnerText
                Else
                    If (sp.DefaultValue IsNot Nothing) Then
                        strValue = sp.DefaultValue.ToString
                    End If
                End If
            Catch ex As Exception
                ' Yippee
            End Try

        End If

        Return strValue

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Store a single value into the settings.
    ''' </summary>
    ''' <param name="propVal"></param>
    ''' -----------------------------------------------------------------------
    Private Sub StoreValue(ByVal propVal As SettingsPropertyValue)

        Dim elem As Xml.XmlElement
        Dim strVal As String = ""

        If (propVal Is Nothing) Then Return
        If (propVal.SerializedValue IsNot Nothing) Then
            strVal = propVal.SerializedValue.ToString
        End If

        'Determine if the setting is roaming.
        'If roaming then the value is stored as an element under the root
        'Otherwise it is stored under a machine name node 
        Try
            elem = DirectCast(SettingsDoc.SelectSingleNode(cSETTINGSROOT & "/" & propVal.Name), XmlElement)
        Catch ex As Exception
            elem = Nothing
        End Try

        Try

            'Check to see if the node exists, if so then set its new value
            If (elem IsNot Nothing) Then
                elem.InnerText = strVal
            Else
                'Store the value as an element of the Settings Root Node
                elem = SettingsDoc.CreateElement(propVal.Name)
                elem.InnerText = strVal
                SettingsDoc.SelectSingleNode(cSETTINGSROOT).AppendChild(elem)
            End If
        Catch ex As Exception
            ' Value not set
        End Try

    End Sub

#End Region ' Internal bits

End Class

