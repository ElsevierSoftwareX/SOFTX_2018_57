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

Option Explicit On
Option Strict On

Imports EwEPlugin
Imports ScientificInterfaceShared.Controls
Imports EwECore
Imports EwEUtils.Core
Imports System.Windows.Forms
Imports System.Drawing

#End Region

Public Class cPluginPoint
    Implements IEcopathRunCompleted2Plugin
    Implements IUIContextPlugin
    Implements INavigationTreeItemPlugin

    Private m_uic As cUIContext = Nothing
    Private m_frm As Ecopath.frmPrebal = Nothing
    Private m_model As cPrebalModel = Nothing

#Region " Running "

    Public Sub EcopathRunCompleted(ByRef EcopathDataStructures As Object, _
                                   ByRef TaxonDataStructures As Object, _
                                   ByRef StanzaDataStructures As Object) _
        Implements IEcopathRunCompleted2Plugin.EcopathRunCompleted

        Try
            Me.m_model.Update()
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Running

#Region " Generic "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IPlugin.Author"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Author As String _
         Implements IPlugin.Author
        Get
            Return "Sheila Heymans (SAMS), Jeroen Steenbeek (EII), Marta Coll (EII)"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IPlugin.Contact"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Contact As String _
        Implements IPlugin.Contact
        Get
            Return "Ecopath development team: ewedevteam@gmail.com"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IPlugin.Description"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Description As String _
        Implements IPlugin.Description
        Get
            Return "Ecopath pre-balance diagnostics plug-in. This plug-in nests a series of graphs into the Ecopath output tools section, where users can explore biomass, production and consumption trends across the trophic levels in their foodweb. This plug-in is based on theory posed by Jason Link in 'Adding rigor to ecological network models by evaluating a set of pre-balance diagnostics: A plea for PREBAL' (http://www.sciencedirect.com/science/article/pii/S0304380010001468)"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IPlugin.Name"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Name As String _
        Implements IPlugin.Name
        Get
            Return "ndPrebal"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IPlugin.Initialize"/>
    ''' -----------------------------------------------------------------------
    Public Sub Initialize(core As Object) _
        Implements IPlugin.Initialize

        Try
            Me.m_model = New cPrebalModel(DirectCast(core, cCore))
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Generic

#Region " UI "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IUIContextPlugin.UIContext"/>
    ''' -----------------------------------------------------------------------
    Public Sub UIContext(uic As Object) _
         Implements IUIContextPlugin.UIContext
        Try
            Me.m_uic = DirectCast(uic, cUIContext)
        Catch ex As Exception

        End Try
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.ControlImage"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ControlImage As Image _
         Implements IGUIPlugin.ControlImage
        Get
            Return Nothing
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.ControlText"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ControlText As String _
        Implements IGUIPlugin.ControlText
        Get
            Return "Pre-bal"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.ControlTooltipText"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ControlTooltipText As String _
        Implements IGUIPlugin.ControlTooltipText
        Get
            Return "Pre-balance analysis"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.EnabledState"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property EnabledState As eCoreExecutionState _
        Implements IGUIPlugin.EnabledState
        Get
            Return eCoreExecutionState.EcopathCompleted
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IGUIPlugin.OnControlClick"/>
    ''' -----------------------------------------------------------------------
    Public Sub OnControlClick(sender As Object, e As System.EventArgs, ByRef frmPlugin As Form) _
        Implements IGUIPlugin.OnControlClick

        Try
            If (Not Me.HasUI()) Then
                Me.m_frm = New Ecopath.frmPrebal(Me.m_uic, Me.m_model)
            End If
            frmPlugin = Me.m_frm
        Catch ex As Exception

        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="INavigationTreeItemPlugin.NavigationTreeItemLocation"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property NavigationTreeItemLocation As String _
        Implements INavigationTreeItemPlugin.NavigationTreeItemLocation
        Get
            Return "ndParameterization\ndEcopathOutput\ndEcopathOutputTools"
        End Get
    End Property

#End Region ' UI

#Region " Internals "

    Private Function HasUI() As Boolean
        If (Me.m_frm Is Nothing) Then Return False
        Return (Not Me.m_frm.IsDisposed)
    End Function

#End Region ' Internals

End Class
