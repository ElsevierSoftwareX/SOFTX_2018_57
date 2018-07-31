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
' Copyright 1991- Ecopath International Initiative, Barcelona, Spain and
'                 Joint Reseach Centre, Ispra, Italy.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwEPlugin
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports System.Windows.Forms

#End Region ' Imports

Public Class cPluginPoint
    Implements IMenuItemPlugin
    Implements IUIContextPlugin
    Implements IHelpPlugin

    Private m_uic As cUIContext = Nothing

    Public Sub UIContext(uic As Object) _
        Implements EwEPlugin.IUIContextPlugin.UIContext
        Me.m_uic = DirectCast(uic, cUIContext)
    End Sub

    Public ReadOnly Property ControlImage As System.Drawing.Image _
        Implements IGUIPlugin.ControlImage
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property ControlText As String _
        Implements IGUIPlugin.ControlText
        Get
            Return "Import Aquamaps HSPEN Species Envelopes"
        End Get
    End Property

    Public ReadOnly Property ControlTooltipText As String _
        Implements IGUIPlugin.ControlTooltipText
        Get
            Return "Utility that imports species distribution envelopes from Aquamaps into capacity response functions"
        End Get
    End Property

    Public ReadOnly Property EnabledState As eCoreExecutionState _
        Implements IGUIPlugin.EnabledState
        Get
            Return eCoreExecutionState.EcosimLoaded
        End Get
    End Property

    Public Sub OnControlClick(sender As Object, e As System.EventArgs, ByRef frmPlugin As System.Windows.Forms.Form) _
        Implements IGUIPlugin.OnControlClick
        Try
            Me.m_uic.FormMain.BeginInvoke(New MethodInvoker(AddressOf LaunchUI))
        Catch ex As Exception
            ' Oof
        End Try
    End Sub

    Public ReadOnly Property MenuItemLocation As String Implements IMenuItemPlugin.MenuItemLocation
        Get
            Return "MenuTools"
        End Get
    End Property

    Public ReadOnly Property Author As String Implements IPlugin.Author
        Get
            Return "Jeroen Steenbeek, Chiara Piroddi"
        End Get
    End Property

    Public ReadOnly Property Contact As String Implements IPlugin.Contact
        Get
            Return "mailto:ewedevteam@gmail.com"
        End Get
    End Property

    Public ReadOnly Property Description As String Implements IPlugin.Description
        Get
            Return ""
        End Get
    End Property

    Public Sub Initialize(core As Object) Implements IPlugin.Initialize
        ' NOP
    End Sub

    Public ReadOnly Property Name As String Implements IPlugin.Name
        Get
            Return "ndAquamapsImport"
        End Get
    End Property

#Region " Help plug-in "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IHelpPlugin.HelpTopic"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property HelpTopic As String _
        Implements IHelpPlugin.HelpTopic
        Get
            Return ".\UserGuide\Using aquamaps environmental responses.pdf"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IHelpPlugin.HelpURL"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property HelpURL As String _
        Implements IHelpPlugin.HelpURL
        Get
            Return Me.HelpTopic
        End Get
    End Property

#End Region ' Help plug-in

    Private Sub LaunchUI()
        ' Open UI as dialog box
        Dim frm As New frmImport(Me.m_uic)
        frm.ShowDialog(Me.m_uic.FormMain)
    End Sub

End Class
