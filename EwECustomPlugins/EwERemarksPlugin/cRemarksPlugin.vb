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
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Plug-in point for the remarks plug-in panel.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cRemarksPlugin
    Implements IUIContextPlugin
    Implements IMenuItemTogglePlugin
    Implements IDisposedPlugin
    Implements IDockStatePlugin

#Region " Private variables "

    Private m_core As cCore = Nothing
    Private m_uic As cUIContext = Nothing
    Private m_frm As frmRemarkUI = Nothing

#End Region ' Private variables

#Region " Plug-in implementation "

    Public Sub Initialize(ByVal core As Object) _
        Implements IPlugin.Initialize
        Me.m_core = DirectCast(core, cCore)
    End Sub

    Public Sub Dispose() _
        Implements IDisposedPlugin.Dispose
        Me.m_core = Nothing
        If Me.HasUI Then
            Me.m_frm.Close()
            Me.m_frm.Dispose()
            Me.m_frm = Nothing
        End If
    End Sub

    Public ReadOnly Property Name() As String Implements IPlugin.Name
        Get
            ' The name will (hopefilly) sort the menu item near m_tsmiViewRemarks
            Return "m_tsmiViewRemarksCollector"
        End Get
    End Property

    Public ReadOnly Property ControlImage() As System.Drawing.Image _
     Implements IGUIPlugin.ControlImage
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property ControlText() As String _
        Implements IGUIPlugin.ControlText
        Get
            Return My.Resources.CAPTION
        End Get
    End Property

    Public ReadOnly Property ControlTooltipText() As String _
        Implements IGUIPlugin.ControlTooltipText
        Get
            ' ToDo: globalize this
            Return "Show all remarks in the model"
        End Get
    End Property

    Public ReadOnly Property EnabledState() As EwEUtils.Core.eCoreExecutionState _
        Implements IGUIPlugin.EnabledState
        Get
            Return eCoreExecutionState.Idle
        End Get
    End Property

    Public Sub UIContext(ByVal uic As Object) _
        Implements IUIContextPlugin.UIContext
        Try
            Me.m_uic = DirectCast(uic, cUIContext)
        Catch ex As Exception
        End Try
    End Sub

    Public ReadOnly Property MenuItemLocation() As String _
        Implements IMenuItemPlugin.MenuItemLocation
        Get
            Return "MenuView"
        End Get
    End Property

    Public ReadOnly Property IsChecked As Boolean _
        Implements IMenuItemTogglePlugin.IsChecked
        Get
            Return Me.HasUI
        End Get
    End Property

    Public Sub OnControlClick(ByVal sender As Object, ByVal e As System.EventArgs, ByRef frmPlugin As System.Windows.Forms.Form) _
        Implements IGUIPlugin.OnControlClick
        Try
            If (Me.m_core Is Nothing) Then Return
            If Me.HasUI Then
                Me.m_frm.Close()
                Me.m_frm.Dispose()
                Me.m_frm = Nothing
            Else
                Me.CreateUI()
            End If
            frmPlugin = Me.m_frm
        Catch ex As Exception

        End Try
    End Sub

    Public ReadOnly Property Author() As String _
        Implements IPlugin.Author
        Get
            Return "Jeroen Steenbeek"
        End Get
    End Property

    Public ReadOnly Property Contact() As String _
        Implements IPlugin.Contact
        Get
            Return "mailto:jeroensteenbeek@gmail.com"
        End Get
    End Property

    Public ReadOnly Property Description() As String _
        Implements IPlugin.Description
        Get
            ' ToDo: globalize this
            Return "Plug-in for EwE6 that shows all active remarks"
        End Get
    End Property

    Public Function DockState() As Integer Implements IDockStatePlugin.DockState
        Return WeifenLuo.WinFormsUI.Docking.DockState.DockBottomAutoHide
    End Function

#End Region ' Plug-in implementation

#Region " Internals "

    Private Function CreateUI() As Boolean
        If Not Me.HasUI Then
            Me.m_frm = New frmRemarkUI(Me.m_uic)
            Me.m_frm.Text = My.Resources.CAPTION
            Me.m_frm.TabText = My.Resources.CAPTION
        End If
        Return True
    End Function

    Private Function HasUI() As Boolean
        If Me.m_frm Is Nothing Then Return False
        Return Not Me.m_frm.IsDisposed
    End Function

#End Region ' Internals

End Class
