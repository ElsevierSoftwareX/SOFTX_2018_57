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
Imports EwEPlugin
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports System.Windows.Forms

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Command enabling centralized launching of GUI plug-ins.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cPluginGUICommand
    Inherits cCommand

#Region " Private vars "

    Private m_ip As IGUIPlugin = Nothing
    Private m_sender As Object = Nothing
    Private m_e As EventArgs = Nothing

    Private m_bHasRun As Boolean = False

    ' - Help -
    Private m_strHelpURL As String = ""
    Private m_strHelpTopic As String = ""

#End Region ' Private vars

    Public Sub New(ByVal cmdh As cCommandHandler)
        MyBase.New(cmdh, cPluginGUICommand.COMMAND_NAME)
    End Sub

    Public Shared COMMAND_NAME As String = "~launchguiplugin"

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get(/set) the form created by the plug-in recently invoked.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Form() As Form

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get(/set) the dock state requested by the plug-in. The dockstate is
    ''' defined by WeiFen.Luo's DockPanel suite.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DockState() As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the URL to the help file for this plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property HelpURL As String
        Get
            Return Me.m_strHelpURL
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the URL to the help topic for this plug-in.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property HelpTopic As String
        Get
            Return Me.m_strHelpTopic
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the EwE core execution state that is required to run the plug-in.
    ''' This is known BEFORE the plug-in has executed, obviously...
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property CoreExecutionState() As eCoreExecutionState
        Get
            If (Me.m_ip Is Nothing) Then Return eCoreExecutionState.Idle
            Return Me.m_ip.EnabledState
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run the plug-in in response to <see cref="Invoke"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub RunPlugin()

        If (Me.m_ip Is Nothing) Then Return
        If Me.m_bHasRun Then Return

        ' Get dockstate, if possible
        If TypeOf Me.m_ip Is IDockStatePlugin Then
            Me.DockState = DirectCast(Me.m_ip, IDockStatePlugin).DockState
        End If

        ' Pick fields from the plug-in
        If (TypeOf Me.m_ip Is IHelpPlugin) Then
            Me.m_strHelpURL = DirectCast(Me.m_ip, IHelpPlugin).HelpURL
            Me.m_strHelpTopic = DirectCast(Me.m_ip, IHelpPlugin).HelpTopic
        End If

        Try
            Me.m_ip.OnControlClick(Me.m_sender, Me.m_e, Me.Form)
        Catch ex As Exception
            Debug.Assert(False, String.Format("Error {0} occurred while running plugin {1}", ex.Message, Me.m_ip.Name))
        Finally
            Me.m_bHasRun = True
        End Try

    End Sub

    Friend Overloads Sub Invoke(ByVal ip As IGUIPlugin, ByVal sender As Object, ByVal e As EventArgs)

        ' Reset fields
        Me.m_bHasRun = False
        Me.Form = Nothing
        Me.DockState = 0

        Me.m_ip = ip
        Me.m_sender = sender
        Me.m_e = e

        ' Try to launch plugin via command structure first
        MyBase.Invoke()
        ' Try to run the plug-in manually
        Me.RunPlugin()

    End Sub

End Class

