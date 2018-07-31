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

Imports EwECore
Imports EwEPlugin
Imports EwEUtils.Core

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Plug-in point implementaiton for the EwE6 flow diagram plug-in.
''' </summary>
''' ===========================================================================
Public Class cEwEFlowDiagramPlugin
    Implements EwEPlugin.IEcopathRunCompletedPlugin
    Implements EwEPlugin.INavigationTreeItemPlugin

#Region " Private vars "

    ''' <summary>The core to use.</summary>
    Private m_core As EwECore.cCore = Nothing
    ''' <summary>The core data to use.</summary>
    Private m_data As cEcopathDataStructures = Nothing
    ''' <summary>The UI to use.</summary>
    Private m_ui As frmFlowDiagramPlugin = Nothing
    ''' <summary>Initialization OK flag.</summary>
    Private m_bInitOK As Boolean = False

#End Region ' Private vars

#Region " Core "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialize the plugin. This method is called only once when the plugin is loaded.
    ''' </summary>
    ''' <param name="core">The core that the plugin can use.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Initialize(ByVal core As Object) _
        Implements EwEPlugin.IPlugin.Initialize

        ' Sanity checks
        Debug.Assert(TypeOf core Is EwECore.cCore, Me.ToString & ".Initialize() argument core is not a cCore object.")

        ' Get ready for initialization
        Me.m_bInitOK = False

        Try
            If (TypeOf core Is EwECore.cCore) Then
                ' Store core reference for this plugin
                Me.Core = DirectCast(core, EwECore.cCore)
            Else
                Return
            End If
        Catch ex As Exception
            cLog.Write("FlowDiagram init: " & ex.Message)
            Debug.Assert(False, ex.Message)
            Return
        End Try
        Me.m_bInitOK = True

    End Sub

#End Region ' Core

#Region " GUI "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.ControlImage"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ControlImage() As System.Drawing.Image _
        Implements EwEPlugin.IGUIPlugin.ControlImage
        Get
            Return Nothing
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.ControlText"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ControlText() As String _
        Implements EwEPlugin.IGUIPlugin.ControlText
        Get
            Return "Flow diagram - EwE5 style"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.OnControlClick"/>
    ''' -----------------------------------------------------------------------
    Public Sub OnControlClick(ByVal sender As Object, _
                              ByVal e As System.EventArgs, _
                              ByRef f As Windows.Forms.Form) _
                              Implements EwEPlugin.IGUIPlugin.OnControlClick

        If (Not Me.m_bInitOK) Then Return

        If Not Me.HasUI Then
            Me.m_ui = New frmFlowDiagramPlugin(Me.ControlText, Me)
        End If

        ' Pass form reference back to calling app
        f = Me.m_ui

        ' No need to show form; this will be handled by the EwE framework
        'Me.m_ui.Show()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.ControlTooltipText"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ControlTooltipText() As String _
           Implements EwEPlugin.IGUIPlugin.ControlTooltipText
        Get
            Return Me.ControlText
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.EnabledState"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property EnabledState() As EwEUtils.Core.eCoreExecutionState _
        Implements EwEPlugin.IGUIPlugin.EnabledState
        Get
            Return EwEUtils.Core.eCoreExecutionState.EcopathCompleted
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="INavigationTreeItemPlugin.NavigationTreeItemLocation"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property NavigationTreeItemLocation() As String _
        Implements EwEPlugin.INavigationTreeItemPlugin.NavigationTreeItemLocation
        Get
            'this will put the navigation item at the end of the tree as top level node 
            'Not the best place there should be a Plugins node and all plugins should go under it
            Return "ndParameterization|ndEcopathOutputTools"
        End Get
    End Property

#End Region ' GUI

#Region " Ecopath "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IEcopathRunCompletedPlugin.EcopathRunCompleted"/>
    ''' -----------------------------------------------------------------------
    Public Sub EcopathRunCompleted(ByRef data As Object) _
        Implements EwEPlugin.IEcopathRunCompletedPlugin.EcopathRunCompleted

        Debug.Assert(TypeOf data Is EwECore.cEcopathDataStructures, Me.ToString & _
                            ".EcopathRan() argument EcopathDataStructure is not a cEcopathDataStructures object.")
        Try
            If (TypeOf data Is EwECore.cEcopathDataStructures) Then
                Me.EcopathDatastructures = DirectCast(data, cEcopathDataStructures)
            Else
                ' This is a programming error!
                Debug.Assert(False, "Plugin incorrectly used!")
            End If
        Catch ex As Exception
            cLog.Write("FlowDiagram plug-in EcopathRunCompleted: " & ex.Message)
        End Try

    End Sub

#End Region ' Ecopath

#Region " Plugin Properties "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IPlugin.Name"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Name() As String _
        Implements EwEPlugin.IPlugin.Name
        Get
            Return Me.ControlText()
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IPlugin.Description"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Description() As String _
        Implements EwEPlugin.IPlugin.Description
        Get
            Return "Flow Diagram plug-in for Ecopath with Ecosim version 6"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IPlugin.Author"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Author() As String Implements EwEPlugin.IPlugin.Author
        Get
            Return "UBC Institute for the Oceans and Fisheries"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IPlugin.Contact"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Contact() As String Implements EwEPlugin.IPlugin.Contact
        Get
            Return "mailto:ewedevteam@gmail.com"
        End Get
    End Property

#End Region ' Plugin Properties

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Reference the Ecopath datastructures allowing the plugin to access 
    ''' ecopath data to generate the flw file for the fd.exe application.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Property EcopathDatastructures() As cEcopathDataStructures
        Get
            Return m_data
        End Get
        Private Set(ByVal data As cEcopathDataStructures)
            Me.m_data = data
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Reference the EwE Core allowing the plugin to access ecopath data to 
    ''' generate the flw file for the fd.exe application.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Property Core() As cCore
        Get
            Return Me.m_core
        End Get
        Private Set(ByVal core As cCore)
            Me.m_core = core
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, states if the plugin has an interface.
    ''' </summary>
    ''' <returns>True if the plugin has an interface.</returns>
    ''' -----------------------------------------------------------------------
    Private Function HasUI() As Boolean
        If (Me.m_ui Is Nothing) Then Return False
        Return Not Me.m_ui.IsDisposed
    End Function

#End Region ' Internals

End Class
