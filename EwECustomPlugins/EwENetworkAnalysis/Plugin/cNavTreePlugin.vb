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
Imports EwECore
Imports EwEPlugin
Imports EwEUtils.Core
Imports System.Text
Imports EwEUtils.SystemUtilities
Imports System.Reflection

#End Region ' Imports

Public MustInherit Class cNavTreeControlPlugin
    Implements INavigationTreeItemPlugin

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IPlugin.Name"/>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property Name() As String Implements EwEPlugin.IPlugin.Name

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IGUIPlugin.ControlImage"/>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property ControlImage() As System.Drawing.Image _
        Implements EwEPlugin.IGUIPlugin.ControlImage

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IGUIPlugin.ControlText"/>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property ControlText() As String _
        Implements EwEPlugin.IGUIPlugin.ControlText

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IGUIPlugin.ControlTooltipText"/>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property ControlTooltipText() As String _
        Implements EwEPlugin.IGUIPlugin.ControlTooltipText
        Get
            Return My.Resources.GENERIC_TOOLTIP
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IGUIPlugin.EnabledState"/>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property EnabledState() As EwEUtils.Core.eCoreExecutionState _
        Implements EwEPlugin.IGUIPlugin.EnabledState
        Get
            Return eCoreExecutionState.EcopathCompleted
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IGUIPlugin.OnControlClick"/>
    ''' -----------------------------------------------------------------------
    Public Sub OnControlClick(ByVal sender As Object, ByVal e As System.EventArgs, ByRef frmPlugin As System.Windows.Forms.Form) _
        Implements EwEPlugin.IGUIPlugin.OnControlClick
        frmPlugin = cEwENetworkAnalysisPlugin.SwitchForm(Me.FormPage)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.INavigationTreeItemPlugin.NavigationTreeItemLocation"/>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property NavigationTreeItemLocation() As String _
        Implements EwEPlugin.INavigationTreeItemPlugin.NavigationTreeItemLocation

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IPlugin.Author"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Author() As String Implements EwEPlugin.IPlugin.Author
        Get
            Return "UBC Institute for the Oceans and Fisheries"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IPlugin.Contact"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Contact() As String Implements EwEPlugin.IPlugin.Contact
        Get
            Return "mailto:ewedevteam@gmail.com"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IPlugin.Description"/>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property Description() As String Implements EwEPlugin.IPlugin.Description
        Get
            ' ToDo: globalize this
            Return "Network Analysis plug-in for Ecopath with Ecosim"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="EwEPlugin.IPlugin.Initialize"/>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub Initialize(ByVal core As Object) Implements EwEPlugin.IPlugin.Initialize
        ' NOP
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Must override to define the name of the <see cref="frmNetworkAnalysis.ShowPage"></see>
    ''' network analysis page that a navigation item opens.
    ''' </summary>
    ''' <returns>The page to navigate to when this plug-in point is activated.</returns>
    ''' -----------------------------------------------------------------------
    Public MustOverride Function FormPage() As frmNetworkAnalysis.eNetworkAnalysisPageTypes

    Protected Function NavTreeNodeRoot() As String
        Return "ndParameterization|ndEcopathOutput|ndEcopathOutputTools"
    End Function

End Class
