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
Imports System
Imports System.Windows.Forms
Imports System.Diagnostics
Imports System.Reflection

#End Region ' Imports

' ToDo_JS: Make menu items toggle automatically

Namespace Commands

#Region " cControlHandler - base class "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Base class for connecting a Command to a User Interface Control
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public MustInherit Class cControlHandler
        Implements IDisposable

        ''' <summary>The associated Command.</summary>
        Private m_cmd As cCommand = Nothing
        ''' <summary>Optional command invocation function parameters.</summary>
        Private m_objParams As Object() = Nothing

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of a ControlHandler. Connects a
        ''' Command instance to a User Interface control.
        ''' </summary>
        ''' <param name="objCmd">The Command instance to attach.</param>
        ''' <param name="objGUI">The User Interface instance to attach.</param>
        ''' ---------------------------------------------------------------------------
        Public Sub New(ByVal objCmd As Object, ByVal objGUI As Object, ByVal fnparms As Object())
            Debug.Assert(TypeOf objCmd Is cCommand)
            Me.m_cmd = DirectCast(objCmd, cCommand)
            Me.m_objParams = fnparms
        End Sub

        ''' ---------------------------------------------------------------------------
        ''' <inheritdocs cref="IDisposable.Dispose"/>
        ''' ---------------------------------------------------------------------------
        Public Overridable Sub Dispose() _
            Implements IDisposable.Dispose

            Me.m_cmd = Nothing
            Me.m_objParams = Nothing
            GC.SuppressFinalize(Me)

        End Sub

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; exposes the attached cCommand to derived classes.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Protected ReadOnly Property Command() As cCommand
            Get
                Return Me.m_cmd
            End Get
        End Property

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; exposes an attached launch parameter to derived classes.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Protected ReadOnly Property Params As Object()
            Get
                Return Me.m_objParams
            End Get
        End Property

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Invoke the underlying command.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Protected Sub Invoke(Optional p As Object() = Nothing)

            If (p Is Nothing) Then p = Me.Params
            Dim t As Type = Me.m_cmd.GetType
            t.InvokeMember("Invoke", BindingFlags.InvokeMethod, Type.DefaultBinder, Me.m_cmd, p)

        End Sub

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Override this to implement how the User Interface control will reflect
        ''' the Command state.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Public MustOverride Sub Update()

    End Class

#End Region ' cControlHandler 

#Region " cToolStripMenuItemControlHandler "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Implementation of a connecting between a <see cref="cCommand"/> and a 
    ''' <see cref="ToolStripMenuItem"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cToolStripMenuItemControlHandler
        Inherits cControlHandler

        Private WithEvents m_tsi As ToolStripMenuItem = Nothing

        Public Sub New(ByVal objCmd As Object, ByVal objGUI As Object, ByVal fnparms As Object())
            MyBase.New(objCmd, objGUI, fnparms)
            Debug.Assert(TypeOf objGUI Is ToolStripMenuItem)
            Me.m_tsi = DirectCast(objGUI, ToolStripMenuItem)
        End Sub

        Public Overrides Sub Dispose()
            Me.m_tsi = Nothing
            MyBase.Dispose()
        End Sub

        Public Overrides Sub Update()
            Me.m_tsi.Available = Me.Command.IsAvailable
            Me.m_tsi.Enabled = Me.Command.Enabled
            Me.m_tsi.Checked = Me.Command.Checked
            If (String.IsNullOrWhiteSpace(Me.m_tsi.ToolTipText) Or (String.Compare(Me.m_tsi.Text, Me.m_tsi.ToolTipText, False) = 0)) Then
                Me.m_tsi.ToolTipText = Me.Command.Description
            End If
        End Sub

        Private Sub OnClick(ByVal sender As Object, ByVal e As EventArgs) Handles m_tsi.Click
            Try
                Me.Invoke()
            Catch ex As Exception
                ' Kaboom
            End Try
        End Sub

    End Class

#End Region ' ToolStripMenuItemControlHandler

#Region " cToolStripButtonControlHandler "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Implementation of a connecting between a <see cref="cCommand"/> and a 
    ''' <see cref="ToolStripButton"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cToolStripButtonControlHandler
        Inherits cControlHandler

        Private WithEvents m_tsb As ToolStripButton = Nothing

        Public Sub New(ByVal objCmd As Object, ByVal objGUI As Object, ByVal fnparms As Object())
            MyBase.New(objCmd, objGUI, fnparms)
            Debug.Assert(TypeOf objGUI Is ToolStripButton)
            Me.m_tsb = DirectCast(objGUI, ToolStripButton)
        End Sub

        Public Overrides Sub Dispose()
            Me.m_tsb = Nothing
            MyBase.Dispose()
        End Sub

        Public Overrides Sub Update()
            Me.m_tsb.Available = Me.Command.IsAvailable
            Me.m_tsb.Enabled = Me.Command.Enabled
            Me.m_tsb.Checked = Me.Command.Checked
            If (String.IsNullOrWhiteSpace(Me.m_tsb.ToolTipText) Or (String.Compare(Me.m_tsb.Text, Me.m_tsb.ToolTipText, False) = 0)) Then
                Me.m_tsb.ToolTipText = Me.Command.Description
            End If
        End Sub

        Private Sub OnClick(ByVal sender As Object, ByVal e As EventArgs) Handles m_tsb.Click
            Try
                Me.Invoke()
            Catch ex As Exception
                ' Kaboom
            End Try
        End Sub

    End Class

#End Region ' cToolStripButtonControlHandler

#Region " cToolStripButtonDropDownControlHandler "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Implementation of a connecting between a <see cref="cCommand"/> and a 
    ''' <see cref="ToolStripButton"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class ToolStripButtonDropDownControlHandler
        Inherits cControlHandler

        Private WithEvents m_tsb As ToolStripDropDownButton = Nothing

        Public Sub New(ByVal objCmd As Object, ByVal objGUI As Object, ByVal fnparms As Object())
            MyBase.New(objCmd, objGUI, fnparms)
            Debug.Assert(TypeOf objGUI Is ToolStripDropDownButton)
            Me.m_tsb = DirectCast(objGUI, ToolStripDropDownButton)
        End Sub

        Public Overrides Sub Dispose()
            Me.m_tsb = Nothing
            MyBase.Dispose()
        End Sub

        Public Overrides Sub Update()
            Me.m_tsb.Available = Me.Command.IsAvailable
            Me.m_tsb.Enabled = Me.Command.Enabled
            If (String.IsNullOrWhiteSpace(Me.m_tsb.ToolTipText) Or (String.Compare(Me.m_tsb.Text, Me.m_tsb.ToolTipText, False) = 0)) Then
                Me.m_tsb.ToolTipText = Me.Command.Description
            End If
        End Sub

        Private Sub OnClick(ByVal sender As Object, ByVal e As EventArgs) Handles m_tsb.Click
            Try
                Me.Invoke()
            Catch ex As Exception
                ' Kaboom
            End Try
        End Sub

    End Class

#End Region ' cToolStripButtonDropDownControlHandler

#Region " ToolStripSplitButtonHandler "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Implementation of a connecting between a <see cref="cCommand"/> and a 
    ''' <see cref="ToolStripSplitButton"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cToolStripSplitButtonHandler
        Inherits cControlHandler

        Private WithEvents m_tsb As ToolStripSplitButton = Nothing

        Public Sub New(ByVal objCmd As Object, ByVal objGUI As Object, ByVal fnparms As Object())
            MyBase.New(objCmd, objGUI, fnparms)
            Debug.Assert(TypeOf objGUI Is ToolStripSplitButton)
            Me.m_tsb = DirectCast(objGUI, ToolStripSplitButton)
        End Sub

        Public Overrides Sub Dispose()
            Me.m_tsb = Nothing
            MyBase.Dispose()
        End Sub

        Public Overrides Sub Update()
            Me.m_tsb.Available = Me.Command.IsAvailable
            Me.m_tsb.Enabled = Me.Command.Enabled
            If (String.IsNullOrWhiteSpace(Me.m_tsb.ToolTipText) Or (String.Compare(Me.m_tsb.Text, Me.m_tsb.ToolTipText, False) = 0)) Then
                Me.m_tsb.ToolTipText = Me.Command.Description
            End If
        End Sub

        Private Sub OnClick(ByVal sender As Object, ByVal e As EventArgs) Handles m_tsb.ButtonClick
            Try
                Me.Invoke()
            Catch ex As Exception
                ' Kaboom
            End Try
        End Sub

    End Class

#End Region ' ToolStripSplitButtonHandler

#Region " cButtonControlHandler "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Implementation of a connecting between a <see cref="cCommand"/> and a <see cref="Button"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cButtonControlHandler
        Inherits cControlHandler

        Private WithEvents m_ctrl As Control = Nothing

        Public Sub New(ByVal objCmd As Object, ByVal objGUI As Object, ByVal fnparms As Object())
            MyBase.New(objCmd, objGUI, fnparms)
            Debug.Assert(TypeOf objGUI Is Control)
            Me.m_ctrl = DirectCast(objGUI, Control)
        End Sub

        Public Overrides Sub Dispose()
            Me.m_ctrl = Nothing
            MyBase.Dispose()
        End Sub

        Public Overrides Sub Update()
            Me.m_ctrl.Enabled = Me.Command.Enabled
            ' Buttons do not have tooltips, you silly
            'Me.m_btn.ToolTipText = Me.Command.Description
        End Sub

        Private Sub OnClick(ByVal sender As Object, ByVal e As EventArgs) Handles m_ctrl.Click
            Try
                Me.Invoke()
            Catch ex As Exception
                ' Kaboom
            End Try
        End Sub

    End Class

#End Region ' cButtonControlHandler

#Region " cRichTextBoxControlHandler "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Implementation of a connecting between a <see cref="cCommand"/> and a <see cref="RichTextBox"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cRichTextBoxControlHandler
        Inherits cControlHandler

        Private WithEvents m_ctrl As RichTextBox = Nothing

        Public Sub New(ByVal objCmd As Object, ByVal objGUI As Object, ByVal fnparms As Object())
            MyBase.New(objCmd, objGUI, fnparms)
            Debug.Assert(TypeOf objGUI Is RichTextBox)
            Me.m_ctrl = DirectCast(objGUI, RichTextBox)
        End Sub

        Public Overrides Sub Dispose()
            Me.m_ctrl = Nothing
            MyBase.Dispose()
        End Sub

        Public Overrides Sub Update()
            Me.m_ctrl.Enabled = Me.Command.Enabled
        End Sub

        Private Sub OnClick(sender As Object, e As System.EventArgs) Handles m_ctrl.Click
            Try
                If (Me.Params IsNot Nothing) Then
                    Me.Invoke()
                End If
            Catch ex As Exception
                ' Kaboom
            End Try
        End Sub

        Private Sub OnLinkClicked(sender As Object, e As LinkClickedEventArgs) Handles m_ctrl.LinkClicked
            Try
                Me.Invoke(New Object() {e.LinkText})
            Catch ex As Exception
                ' Kaboom
            End Try
        End Sub

    End Class

#End Region ' cRichTextBoxControlHandler

End Namespace
