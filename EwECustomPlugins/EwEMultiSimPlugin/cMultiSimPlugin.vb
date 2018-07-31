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
Imports EwECore
Imports ScientificInterfaceShared.Controls
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' The plug-in point for the Multi-Sim plug-in.
''' </summary>
''' <remarks>
''' Did you know that this plug-in was briefly called 'Multi-Runs'? Tee hee hee.
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class cMultiSimPlugin
    Implements EwEPlugin.IUIContextPlugin
    Implements EwEPlugin.IMenuItemPlugin
    Implements EwEPlugin.INavigationTreeItemPlugin
    Implements EwEPlugin.IHelpPlugin

#Region " Private vars "

    Private m_frmUI As frmMain = Nothing
    Private m_uic As cUIContext = Nothing

#End Region ' Private vars

#Region " Plug-in points "

#Region " IPlugin "

    Public Sub Initialize(core As Object) _
        Implements EwEPlugin.IPlugin.Initialize
        ' Ignore
    End Sub

    Public ReadOnly Property Name As String _
        Implements EwEPlugin.IPlugin.Name
        Get
            Return "ndDFO_MultiSim"
        End Get
    End Property

    Public ReadOnly Property Author As String _
        Implements EwEPlugin.IPlugin.Author
        Get
            Return "Fisheries and Oceans Canada"
        End Get
    End Property

    Public ReadOnly Property Contact As String _
        Implements EwEPlugin.IPlugin.Contact
        Get
            Return "Sylvie Guenette, Carie Hoover, Dave Preikshot"
        End Get
    End Property

    Public ReadOnly Property Description As String _
        Implements EwEPlugin.IPlugin.Description
        Get
            Return My.Resources.GENERIC_DESCRIPTION
        End Get
    End Property

#End Region ' IPlugin

#Region " UI Context "

    Public Sub UIContext(uic As Object) _
        Implements EwEPlugin.IUIContextPlugin.UIContext
        Me.m_uic = DirectCast(uic, cUIContext)
    End Sub

#End Region ' UI Context

#Region " GUI integration "

    Public ReadOnly Property ControlImage As System.Drawing.Image _
        Implements EwEPlugin.IGUIPlugin.ControlImage
        Get
            Return Nothing ' My.Resources.logo_canada
        End Get
    End Property

    Public Sub OnControlClick(sender As Object, e As System.EventArgs, ByRef frmPlugin As System.Windows.Forms.Form) _
        Implements EwEPlugin.IGUIPlugin.OnControlClick
        frmPlugin = Me.UI
    End Sub

    Public ReadOnly Property ControlText As String _
        Implements EwEPlugin.IGUIPlugin.ControlText
        Get
            Return My.Resources.GENERIC_NAME
        End Get
    End Property

    Public ReadOnly Property ControlTooltipText As String _
        Implements EwEPlugin.IGUIPlugin.ControlTooltipText
        Get
            Return Me.Description
        End Get
    End Property

    Public ReadOnly Property EnabledState As eCoreExecutionState _
        Implements EwEPlugin.IGUIPlugin.EnabledState
        Get
            Return EwEUtils.Core.eCoreExecutionState.EcosimLoaded
        End Get
    End Property

#End Region ' GUI integration

#Region " Menu item "

    Public ReadOnly Property MenuItemLocation As String _
        Implements EwEPlugin.IMenuItemPlugin.MenuItemLocation
        Get
            Return "MenuTools"
        End Get
    End Property

#End Region ' Menu item

#Region " Navigation tree "

    Public ReadOnly Property NavigationTreeItemLocation As String _
        Implements EwEPlugin.INavigationTreeItemPlugin.NavigationTreeItemLocation
        Get
            Return "ndTimeDynamic\ndEcosimTools"
        End Get
    End Property

#End Region ' Navigation tree

#Region " Help! "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IHelpPlugin.HelpTopic"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property HelpTopic As String _
        Implements EwEPlugin.IHelpPlugin.HelpTopic
        Get
            Return ".\UserGuide\EwEMultiSimPlugin.pdf"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IHelpPlugin.HelpURL"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property HelpURL As String _
        Implements EwEPlugin.IHelpPlugin.HelpURL
        Get
            Return Me.HelpTopic
        End Get
    End Property

#End Region ' Help!

#End Region ' Plug-in point

#Region " Private helper methods "

    Private Function UI() As frmMain
        Dim bHasUI As Boolean = False

        If (Me.m_frmUI IsNot Nothing) Then
            bHasUI = Not Me.m_frmUI.IsDisposed
        End If

        If Not bHasUI Then
            Me.m_frmUI = New frmMain()
            Me.m_frmUI.UIContext = Me.m_uic
            Me.m_frmUI.Text = Me.ControlText
        End If

        Return Me.m_frmUI

    End Function

#End Region ' Private helper methods

End Class
