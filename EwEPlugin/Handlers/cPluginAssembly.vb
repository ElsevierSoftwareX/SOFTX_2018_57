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

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports EwEUtils.Utilities

''' ---------------------------------------------------------------------------
''' <summary>
''' Holds information on a particular plugin assembly (author, version, copyright, etc)
''' as well as a list of <see cref="IPlugin">plug-ins</see> found in the assembly.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cPluginAssembly

#Region " Private helper classes "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' IComparer that sorts plug-ins by name, ascending.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Class cPluginComparer
        Implements IComparer(Of IPlugin)

        Public Function Compare(ByVal x As IPlugin, ByVal y As IPlugin) As Integer _
            Implements IComparer(Of IPlugin).Compare
            Return String.Compare(x.Name, y.Name)
        End Function

    End Class

#End Region ' Private helper classes

#Region " Private parts "

    Private m_ass As Assembly = Nothing
    ''' <summary>All available plugins in this assembly.</summary>
    Private m_dictPlugins As New Dictionary(Of String, IPlugin)
    ''' <summary>Assembly enable state.</summary>
    Private m_bEnabled As Boolean = True
    ''' <summary>Assembly enabled state at startup.</summary>
    Private m_bEnabledInitially As Boolean = True
    ''' <summary>Assembly compatibility state.</summary>
    Private m_compatibility As ePluginCompatibilityTypes = ePluginCompatibilityTypes.VersionCompatible
    ''' <summary>Name of the plug-in sandbox, if any.</summary>
    Private m_strSandbox As String = ""

#End Region ' Private parts

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new plugin assembly wrapper.
    ''' </summary>
    ''' <param name="ass">The wrapped <see cref="Assembly"/>.</param>
    ''' <param name="bEnabled">Flag stating that the plug-in assembly is allowed to load.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal ass As Assembly, ByVal bEnabled As Boolean, strSandbox As String)
        Me.m_ass = ass
        Me.m_strSandbox = strSandbox
        Me.m_bEnabledInitially = bEnabled
        Me.m_bEnabled = bEnabled
    End Sub

#End Region ' Constructor

#Region " Plugin interfaces "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set a named <see cref="IPlugin">plugin</see>.
    ''' </summary>
    ''' <param name="strName">The <see cref="IPlugin.Name">name</see>
    ''' of the plugin.</param>
    ''' <param name="bAllowDisabled">Flag stating if plug-ins from disabled 
    ''' assemblies can be aquired as well.</param>
    ''' <remarks>An exception will be thrown when adding a plugin
    ''' with a duplicate name.</remarks>
    ''' -----------------------------------------------------------------------
    Public Property Plugin(ByVal strName As String, Optional ByVal bAllowDisabled As Boolean = False) As IPlugin
        Get
            Dim ip As IPlugin = Nothing

            strName = strName.ToLower()
            If (Me.CanRun Or bAllowDisabled) Then
                If Me.m_dictPlugins.ContainsKey(strName) Then
                    ip = Me.m_dictPlugins(strName)
                End If
            End If
            Return ip
        End Get
        Set(ByVal ip As IPlugin)
            strName = strName.ToLower()
            If Me.m_dictPlugins.ContainsKey(strName) Then
                Throw New cPluginException(Me, String.Format(My.Resources.PLUGIN_EXCEPTION_DUPLICATE, Me.Filename, strName), Nothing)
            Else
                Me.m_dictPlugins.Add(strName, ip)
            End If
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Gets a collection of <see cref="IPlugin">plugins</see> in this assembly.
    ''' </summary>
    ''' <param name="t">The <see cref="Type">Type</see> of the plugins to retrieve,
    ''' or Nothing to return all plugins in this Assembly.</param>
    ''' <param name="bAllowDisabled">Flag stating if plug-ins from disabled 
    ''' assemblies can be aquired as well.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Plugins(Optional ByVal t As Type = Nothing, _
                                     Optional ByVal bAllowDisabled As Boolean = False) As ICollection(Of IPlugin)
        Get
            Dim collPlugins As New List(Of IPlugin)

            If (Me.CanRun Or bAllowDisabled) Then
                If t Is Nothing Then
                    collPlugins.AddRange(Me.m_dictPlugins.Values)
                Else
                    For Each ip As IPlugin In Me.m_dictPlugins.Values
                        If t.IsInstanceOfType(ip) Then
                            collPlugins.Add(ip)
                        End If
                    Next
                End If
            End If

            ' Sort plug-ins
            collPlugins.Sort(New cPluginComparer())
            ' Done
            Return collPlugins

        End Get
    End Property

#End Region ' Plugin interfaces

#Region " Enabling/disabling "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether this assembly is allowed to be accessed for invoking 
    ''' plug-ins.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property CanRun() As Boolean
        Get
            Return (Me.Enabled And Me.SessionEnabled) Or Me.AlwaysEnabled
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/Set assembly enabled state.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Enabled() As Boolean
        Get
            Return Me.m_bEnabled Or Me.AlwaysEnabled()
        End Get
        Set(ByVal bEnabled As Boolean)
            ' Abort when enabled state will not change
            If (Me.m_bEnabled = bEnabled) Then Return
            ' Abort when trying to disable an AlwaysEnabled plugin
            If (Me.AlwaysEnabled() And bEnabled = False) Then Return
            ' Update enabled state
            Me.m_bEnabled = bEnabled
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get whether this assembly is enable for a session. This flag can only
    ''' be set at plugin assembly load time to ensure that a plug-in assembly
    ''' enabled state does not change thoughtout a session.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property SessionEnabled() As Boolean
        Get
            Return Me.m_bEnabledInitially
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get whether this assembly canot be disabled.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property AlwaysEnabled() As Boolean
        Get
            ' Core plugins are always enabled
            Return cStringUtils.EndsWith(Me.Filename, "ewecore.dll", True)
        End Get
    End Property

#End Region ' Enabling/disabling

#Region " Compatibility "

    Public Enum ePluginCompatibilityTypes As Integer
        ''' <summary>Versions are fully compatible.</summary>
        VersionCompatible = 0
        ''' <summary>Versions may be compatible.</summary>
        VersionCompatibleCaution
        ''' <summary>Major revision version incompatibility detected.</summary>
        VersionIncompatible
        ''' <summary>Unable to determine level of incompatibility.</summary>
        IncompatibleUndetermined
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set plugin compatibility state.
    ''' </summary>
    ''' <remarks>
    ''' States whether a plug-in is compatible with the set of assemblies that
    ''' the main application relies on.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property Compatibility() As ePluginCompatibilityTypes
        Get
            Return Me.m_compatibility
        End Get
        Friend Set(ByVal value As ePluginCompatibilityTypes)
            Me.m_compatibility = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether a plugin assembly is compatible enough to run with EwE.
    ''' </summary>
    ''' <returns>True if compatible to run, false otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsCompatibleToRun() As Boolean
        ' Minor version revisions should not matter
        Return (Me.Compatibility = ePluginCompatibilityTypes.VersionCompatible) Or _
               (Me.Compatibility = ePluginCompatibilityTypes.VersionCompatibleCaution)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether a plugin assembly is compatible with all EwE assemblies.
    ''' </summary>
    ''' <returns>True if compatible to run, false otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsCompatible() As Boolean
        Return (Me.Compatibility = ePluginCompatibilityTypes.VersionCompatible)
    End Function

#End Region ' Compatibility

#Region " Assembly metadata "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set assembly company name.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Company() As String
       
    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set assembly version.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Version() As String
       
    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set assembly description.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Description() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set assembly copyright.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Copyright() As String
     
    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set assembly file name.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Filename() As String
       
    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="AssemblyName">AssemblyName</see> associated with this
    ''' plug-in assembly.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property AssemblyName() As AssemblyName
        Get
            Return Me.m_ass.GetName()
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the actual <see cref="Assembly">Assembly</see> of the plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Assembly() As Assembly
        Get
            Return Me.m_ass
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the name of the sandbox the plug-in was loaded in, if any.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Sandbox As String
        Get
            Return Me.m_strSandbox
        End Get
    End Property

#End Region ' Assembly metadata

#Region " Overrides "

    Public Overrides Function ToString() As String
        Return System.IO.Path.GetFileNameWithoutExtension(Me.Filename) & " " & If(Me.Enabled, "", "(disabled)")
    End Function

#End Region ' Overrides

End Class
