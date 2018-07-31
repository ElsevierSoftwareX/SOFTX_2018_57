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
Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Collections.Specialized
Imports EwECore
Imports EwEPlugin
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports System.Reflection

#End Region

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control; implements the Options > Plug-in options interface.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucOptionsPlugins
        Implements IUIElement
        Implements IOptionsPage

#Region " Helper classes "

        Const cIMAGE_CORE As Integer = 0
        Const cIMAGE_ENABLEDPLUGIN As Integer = 1
        Const cIMAGE_ANYPLUGINPOINT As Integer = 2
        Const cIMAGE_DISABLED As Integer = 3
        Const cIMAGE_CONFLICT As Integer = 4

        Private Class cPluginAssemblyInfo

            Public Sub New(ByVal pa As cPluginAssembly)
                Me.PluginAssembly = pa
                Me.Enabled = pa.Enabled
            End Sub

            Public ReadOnly Property PluginAssembly() As cPluginAssembly = Nothing

            Public Property Enabled() As Boolean

            Public ReadOnly Property AlwaysEnabled() As Boolean
                Get
                    Return Me.PluginAssembly.AlwaysEnabled
                End Get
            End Property

            Public ReadOnly Property Compatible() As Boolean
                Get
                    Return Me.PluginAssembly.IsCompatible
                End Get
            End Property

        End Class

#End Region ' Helper classes

#Region " Private variables "

        ''' <summary></summary>
        Private m_pm As cPluginManager = Nothing
        ''' <summary></summary>
        Private m_dictPluginAssemblyInfo As New Dictionary(Of cPluginAssembly, cPluginAssemblyInfo)

#End Region ' Private variables

#Region " Constructor "

        Public Sub New(ByVal uic As cUIContext)
            Me.UIContext = uic
            Me.m_pm = Me.UIContext.Core.PluginManager
            Me.InitializeComponent()
        End Sub

#End Region ' Constructor

#Region " Interface implementation "


#End Region ' Interface implementation

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext() As cUIContext _
                  Implements IUIElement.UIContext

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanApply"/>
        ''' -------------------------------------------------------------------
        Public Function CanApply() As Boolean _
            Implements IOptionsPage.CanApply
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.OnChanged"/>
        ''' -------------------------------------------------------------------
        Public Event OnOptionsPluginsChanged(sender As IOptionsPage, args As System.EventArgs) _
            Implements IOptionsPage.OnChanged

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.Apply"/>
        ''' -------------------------------------------------------------------
        Public Function Apply() As IOptionsPage.eApplyResultType _
            Implements IOptionsPage.Apply

            Dim result As IOptionsPage.eApplyResultType = IOptionsPage.eApplyResultType.Success
            If (Me.m_pm Is Nothing) Then Return result

            Dim bChanged As Boolean = False

            ' Build list of plugins to disable
            For Each info As cPluginAssemblyInfo In Me.m_dictPluginAssemblyInfo.Values
                Dim key As String = info.PluginAssembly.Filename
                If (Me.m_pm.IsPluginEnabled(key) <> info.Enabled) Then result = IOptionsPage.eApplyResultType.Success_restart
                Me.m_pm.IsPluginEnabled(key) = info.Enabled
            Next

            Return result

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.SetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Sub SetDefaults() _
            Implements IOptionsPage.SetDefaults
            ' NOP
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanSetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Function CanSetDefaults() As Boolean _
            Implements IOptionsPage.CanSetDefaults
            Return False
        End Function

#End Region ' Public interfaces

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            If (Me.m_pm Is Nothing) Then Return

            Dim collPA As ICollection(Of cPluginAssembly) = Nothing
            Dim info As cPluginAssemblyInfo = Nothing
            Dim pa As cPluginAssembly = Nothing
            Dim tnPA As TreeNode = Nothing
            Dim p As IPlugin = Nothing
            Dim tnP As TreeNode = Nothing
            Dim asmCore As Assembly = Assembly.GetAssembly(GetType(cCore))

            ' Prepare image list
            Me.m_ilPlugins.Images.Add(SharedResources.nav_ecopath)
            Me.m_ilPlugins.Images.Add(SharedResources.plugin)
            Me.m_ilPlugins.Images.Add(SharedResources.pluginpoint)
            Me.m_ilPlugins.Images.Add(SharedResources.Cancel)
            Me.m_ilPlugins.Images.Add(SharedResources.Warning)

            collPA = Me.m_pm.PluginAssemblies
            For Each pa In collPA
                ' Do not list EwE core here
                If (String.Compare(asmCore.FullName, pa.Assembly.FullName, True) <> 0) Then

                    info = New cPluginAssemblyInfo(pa)

                    tnPA = New TreeNode(Path.GetFileNameWithoutExtension(pa.Filename))
                    tnPA.Tag = pa
                    tnPA.ImageIndex = Me.GetPluginAssemblyImageIndex(info)
                    tnPA.SelectedImageIndex = tnPA.ImageIndex
                    Me.m_dictPluginAssemblyInfo(pa) = info

                    For Each p In pa.Plugins(Nothing, True)

                        ' Name plug-ins by rich text if possible
                        If (TypeOf p Is IGUIPlugin) Then
                            tnP = New TreeNode(DirectCast(p, IGUIPlugin).ControlText)
                        Else
                            tnP = New TreeNode(p.Name)
                        End If
                        tnP.Tag = p
                        tnP.ImageIndex = cIMAGE_ANYPLUGINPOINT
                        tnP.SelectedImageIndex = cIMAGE_ANYPLUGINPOINT

                        tnPA.Nodes.Add(tnP)
                    Next
                    Me.m_tvPlugins.Nodes.Add(tnPA)
                End If

            Next pa

            If pa IsNot Nothing Then
                Me.m_tvPlugins.SelectedNode = Me.m_tvPlugins.Nodes(0)
                Me.UpdateDetails()
            End If
            Me.UpdateControls()

            MyBase.OnLoad(e)

        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing Then
                    For Each pa As cPluginAssembly In Me.m_dictPluginAssemblyInfo.Keys
                        '' Stop listening to plugin assembly
                        'RemoveHandler pa.AssemblyEnabled, AddressOf OnHandlePluginAssemblyEnabled
                        ' Restore enabled state
                        pa.Enabled = Me.m_dictPluginAssemblyInfo(pa).Enabled
                    Next

                    Me.m_dictPluginAssemblyInfo.Clear()

                    If components IsNot Nothing Then
                        components.Dispose()
                    End If
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        Private Sub OnAfterSelectNode(ByVal sender As System.Object, ByVal e As TreeViewEventArgs) _
            Handles m_tvPlugins.AfterSelect
            Me.UpdateDetails()
        End Sub

        Private Sub OnEnableCheckChanged(ByVal sender As Object, ByVal e As EventArgs) _
            Handles m_cbEnablePlugin.CheckedChanged

            Dim pa As cPluginAssembly = Me.SelectedPluginAssembly
            Dim info As cPluginAssemblyInfo = Me.m_dictPluginAssemblyInfo(pa)
            If (pa IsNot Nothing) Then
                info.Enabled = Me.m_cbEnablePlugin.Checked
                Me.UpdatePluginImage(info)
            End If

        End Sub

        Private Sub OnEnableDisableAll(sender As Object, e As EventArgs) _
            Handles m_cbEnableDisableAll.CheckedChanged

            Me.SuspendLayout()
            For Each info As cPluginAssemblyInfo In Me.m_dictPluginAssemblyInfo.Values
                info.Enabled = Me.m_cbEnableDisableAll.Checked
                Me.UpdatePluginImage(info)
            Next
            Me.ResumeLayout()

        End Sub

#End Region ' Events

#Region " Private implementations "

        Private Function FindPluginAssemblyNode(ByVal pa As cPluginAssembly) As TreeNode
            If pa Is Nothing Then Return Nothing
            For Each tn As TreeNode In Me.m_tvPlugins.Nodes
                If (TypeOf tn.Tag Is cPluginAssembly) Then
                    If Object.ReferenceEquals(DirectCast(tn.Tag, cPluginAssembly), pa) Then
                        Return tn
                    End If
                End If
            Next
            Return Nothing
        End Function

        Private ReadOnly Property SelectedPluginAssembly() As cPluginAssembly
            Get
                Dim tn As TreeNode = Me.m_tvPlugins.SelectedNode
                If (tn Is Nothing) Then Return Nothing ' May have none
                If (TypeOf tn.Tag Is cPluginAssembly) Then
                    Return DirectCast(tn.Tag, cPluginAssembly)
                ElseIf (TypeOf tn.Tag Is IPlugin) Then
                    Return DirectCast(tn.Parent.Tag, cPluginAssembly)
                End If
                Return Nothing
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the image index to reflect a plug-in assembly.
        ''' </summary>
        ''' <param name="info">The plug-in assembly info to return the image for.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function GetPluginAssemblyImageIndex(ByVal info As cPluginAssemblyInfo) As Integer
            If (info.Enabled = False) Then Return cIMAGE_DISABLED
            If (info.Compatible = False) Then Return cIMAGE_CONFLICT
            If (info.AlwaysEnabled = True) Then Return cIMAGE_CORE
            Return cIMAGE_ENABLEDPLUGIN
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateDetails()

            Dim tn As TreeNode = Me.m_tvPlugins.SelectedNode
            Dim ctrl As UserControl = Nothing

            If (TypeOf tn.Tag Is cPluginAssembly) Then
                ctrl = New ucOptionsPluginAssemblyDetails(DirectCast(tn.Tag, cPluginAssembly))
            ElseIf (TypeOf tn.Tag Is IPlugin) Then
                ' Hackerdihack
                ctrl = New ucOptionsPluginDetails(Me.UIContext,
                                              DirectCast(tn.Tag, IPlugin),
                                              DirectCast(tn.Parent.Tag, cPluginAssembly))
            End If

            Me.m_split.SuspendLayout()

            Me.m_split.Panel2.Controls.Clear()
            If ctrl IsNot Nothing Then
                ctrl.Dock = DockStyle.Fill
                Me.m_split.Panel2.Controls.Add(ctrl)
            End If

            Me.m_split.ResumeLayout()
            Me.UpdateControls()

        End Sub

        Private Sub UpdatePluginImage(ByVal info As cPluginAssemblyInfo)
            Dim tn As TreeNode = Me.FindPluginAssemblyNode(info.PluginAssembly)
            Dim iIndex As Integer = Me.GetPluginAssemblyImageIndex(info)

            If tn IsNot Nothing Then
                tn.ImageIndex = iIndex
                tn.SelectedImageIndex = iIndex
            End If
        End Sub

        Private Sub UpdateControls()

            Dim pa As cPluginAssembly = Me.SelectedPluginAssembly
            Dim bHasSuppressedPrompts As Boolean = (Not String.IsNullOrEmpty(My.Settings.SuppressedOverwritePrompts))

            Dim bEnabled As Boolean = False
            Dim bCanDisable As Boolean = False

            If (pa IsNot Nothing) Then
                bEnabled = pa.Enabled
                bCanDisable = (pa.AlwaysEnabled = False)
            End If

            Me.m_cbEnablePlugin.Enabled = bCanDisable
            Me.m_cbEnablePlugin.Checked = bEnabled

        End Sub

#End Region ' Private implementations

    End Class

End Namespace ' Other
