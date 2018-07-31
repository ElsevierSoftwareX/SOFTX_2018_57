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
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Integration

    ''' ===========================================================================
    ''' <summary>
    ''' Helper class, launches all Auto-launchable plug-ins.
    ''' </summary>
    ''' ===========================================================================
    Public Class cPluginAutolaunchHandler
        Inherits cPluginGUIHandler

#Region " Construction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of a cPluginAutolaunchHandler.
        ''' </summary>
        ''' <param name="pm"><see cref="cPluginManager">Plugin manager</see>
        ''' that holds the plugins to launch.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal pm As cPluginManager, ByVal cmdh As cCommandHandler)
            MyBase.New(pm, cmdh)
            Me.LaunchPlugins()
        End Sub

#End Region ' Construction 

#Region " Overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden with emtpy method to comply to base class.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub EnablePlugin(ByVal ip As IGUIPlugin, ByVal bEnable As Boolean)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden with emtpy method to comply to base class.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub PlacePlugin(ByVal ip As IGUIPlugin, ByVal bPlace As Boolean)
        End Sub

#End Region ' Overrides 

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Launch all <see cref="IAutolaunchPlugin">Auto-launchable plug-ins.</see>
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub LaunchPlugins()
            Dim collPlugins As ICollection(Of cPluginManager.cPluginContext) = Me.PluginManager.GetPluginDefs(GetType(IAutolaunchPlugin))
            For Each ipc As cPluginManager.cPluginContext In collPlugins
                Dim ip As IAutolaunchPlugin = DirectCast(ipc.Plugin, IAutolaunchPlugin)
                If ip.Autolaunch Then
                    Me.RunPlugin(ip, Nothing, Nothing)
                End If
            Next
        End Sub

#End Region ' Internals

    End Class

End Namespace
