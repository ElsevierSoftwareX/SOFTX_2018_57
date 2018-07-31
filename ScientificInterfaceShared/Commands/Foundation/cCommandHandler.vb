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
Imports System.Collections.Generic
Imports System.Diagnostics
Imports EwECore
Imports EwEPlugin

#End Region ' Imports

Namespace Commands

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The CommandHandler is the central repository for storing and retrieving
    ''' <see cref="cCommand">Commands</see> in a User Interface. Additionally, this
    ''' class serves as a central registry point for <see cref="cControlHandler">cControlHandlers</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cCommandHandler

#Region " Construction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this class.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()
            ' Create storages
            Me.m_dictCommands = New Dictionary(Of String, cCommand)
            Me.m_dictHandlerTypes = New Dictionary(Of String, Type)

            ' Register predefined command handler types
            Me.AddControlHandlerType("System.Windows.Forms.Button", GetType(cButtonControlHandler))
            Me.AddControlHandlerType("System.Windows.Forms.PictureBox", GetType(cButtonControlHandler))
            Me.AddControlHandlerType("System.Windows.Forms.ToolStripMenuItem", GetType(cToolStripMenuItemControlHandler))
            Me.AddControlHandlerType("System.Windows.Forms.ToolStripButton", GetType(cToolStripButtonControlHandler))
            Me.AddControlHandlerType("System.Windows.Forms.ToolStripDropDownButton", GetType(ToolStripButtonDropDownControlHandler))
            Me.AddControlHandlerType("System.Windows.Forms.ToolStripSplitButton", GetType(cToolStripSplitButtonHandler))
            Me.AddControlHandlerType("System.Windows.Forms.RichTextBox", GetType(cRichTextBoxControlHandler))
        End Sub

#End Region ' Construction

#Region " Public access "

        Public Property PluginManager As cPluginManager = Nothing

#End Region ' Public access

#Region " Command administration "

        ''' <summary>Dictionary of <see cref="cCommand">Commands</see>.</summary>
        Private m_dictCommands As Dictionary(Of String, cCommand) = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Add a <see cref="cCommand">Command</see> to the handler.
        ''' </summary>
        ''' <param name="c">The command to add.</param>
        ''' -----------------------------------------------------------------------
        Friend Sub Add(ByVal c As cCommand)
            Try
                Me.m_dictCommands.Add(c.Name.ToLower(), c)
            Catch ex As Exception
                ' Kaboom
                Debug.Assert(False, "Unable to add command")
            End Try
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Remove a <see cref="cCommand">Command</see> from the handler.
        ''' </summary>
        ''' <param name="c">The command to remove.</param>
        ''' -----------------------------------------------------------------------
        Public Sub Remove(ByVal c As cCommand)
            Try
                If (c IsNot Nothing) Then
                    c.Clear()
                    Me.m_dictCommands.Remove(c.Name.ToLower())
                End If
            Catch ex As Exception
                ' Kaboom
                Debug.Assert(False, "Unable to remove command")
            End Try
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Retrieve a <see cref="cCommand">Command</see> by its name.
        ''' </summary>
        ''' <param name="strName">The name of the Command to find.</param>
        ''' <returns>
        ''' A <see cref="cCommand">Command</see>, or Nothing if the command could not 
        ''' be found.
        ''' </returns>
        ''' -----------------------------------------------------------------------
        Public Function GetCommand(ByVal strName As String) As cCommand
            strName = strName.ToLower
            If Me.m_dictCommands.ContainsKey(strName) Then
                Return Me.m_dictCommands(strName.ToLower())
            End If
            Return Nothing
        End Function

        Public Sub Clear()
            Try
                Dim lcmds As New List(Of cCommand)
                For Each cmd As cCommand In Me.m_dictCommands.Values : lcmds.Add(cmd) : Next
                For Each cmd As cCommand In lcmds : Me.Remove(cmd) : Next
                lcmds.Clear()
            Catch ex As Exception
                ' Kaploof
                Debug.Assert(False, "Unable to clear command handler")
            End Try
            Debug.Assert(Me.m_dictCommands.Count = 0)
        End Sub

#End Region ' Command administration 

#Region " Command idle time updating "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Application idle time handler, makes sure that every registered command 
        ''' is updated.
        ''' </summary>
        ''' <remarks>
        ''' This method should be invoked in response to the .NET Idle event.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Sub OnIdle(ByVal sender As Object, ByVal e As EventArgs)
            For Each cmd As cCommand In Me.m_dictCommands.Values
                cmd.Update()
            Next
        End Sub

#End Region ' Command idle time updating 

#Region " ControlHandler administration "

        ''' <summary>Dictionary of registered GUI control handler types.</summary>
        Private m_dictHandlerTypes As Dictionary(Of String, Type) = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Register a GUI control handler type.
        ''' </summary>
        ''' <param name="obj">An instance of a GUI object to add a handler type for.</param>
        ''' <param name="t">The handler type that will handle events for GUI objects
        ''' of the same type.</param>
        ''' -----------------------------------------------------------------------
        Public Sub AddControlHandlerType(ByVal obj As Object, ByVal t As Type)
            Try
                Me.AddControlHandlerType(obj.GetType().ToString(), t)
            Catch ex As Exception
                Debug.Assert(False, "Unable to get control handler for this control type")
                ' Kaboom
            End Try
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Register a GUI control handler type.
        ''' </summary>
        ''' <param name="str">A Type indicator of a GUI object to add a handler type for.</param>
        ''' <param name="t">The handler type that will handle events for GUI objects
        ''' of the same type.</param>
        ''' -----------------------------------------------------------------------
        Public Sub AddControlHandlerType(ByVal str As String, ByVal t As Type)
            Try
                Me.m_dictHandlerTypes.Add(str, t)
            Catch ex As Exception
                Debug.Assert(False, "Unable to get control handler for this control type")
                ' Kaboom
            End Try
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns a registered GUI control handler type.
        ''' </summary>
        ''' <param name="obj">An instance of a GUI object to return the handler type for.</param>
        ''' -----------------------------------------------------------------------
        Public Function GetControlHandlerType(ByVal obj As Object) As Type
            Try
                Debug.Assert(obj IsNot Nothing)
                Return GetControlHandlerType(obj.GetType().ToString())
            Catch ex As Exception
                ' Kaboom
                Debug.Assert(False, "Unable to get control handler for this control type")
                Return Nothing
            End Try
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns a registered GUI control handler type.
        ''' </summary>
        ''' <param name="str">A Type indicator of a GUI object to return 
        ''' the handler type for.</param>
        ''' -----------------------------------------------------------------------
        Public Function GetControlHandlerType(ByVal str As String) As Type
            Try
                Return Me.m_dictHandlerTypes(str)
            Catch ex As Exception
                ' Kaboom
                Debug.Assert(False, "Unable to get control handler for this control type")
                Return Nothing
            End Try
        End Function

#End Region ' ControlHandler administration

    End Class

End Namespace
