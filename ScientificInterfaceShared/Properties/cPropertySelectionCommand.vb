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
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports SourceGrid2

#End Region ' Imports

Namespace Properties

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' This class implements a selection change <see cref="cCommand">Command</see>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class cPropertySelectionCommand
        Inherits cCommand

        ''' <summary>Public available name for this command</summary>
        Public Shared COMMAND_NAME As String = "~SelectedProperties"

        ''' <summary>The properties broadcasted by this command</summary>
        Private m_lprop As New List(Of cProperty)
        ''' <summary>The event that occurred.</summary>
        Private m_event As SelectionChangeEventType = SelectionChangeEventType.Clear

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes and names an instance of this class.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, COMMAND_NAME)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="cCommand.Invoke">Invokes</see> the command, broadcasting a
        ''' data selection throughout the GUI.
        ''' </summary>
        ''' <param name="source">The <see cref="cCoreInputOutputBase">cCoreInputOutput</see> 
        ''' object that was selected.</param>
        ''' <param name="varName">The <see cref="eVarNameFlags">VarName</see> of
        ''' the field that was selected.</param>
        ''' <param name="sourceSec">The <see cref="cCoreInputOutputBase">cCoreInputOutput</see> 
        ''' object that acts as secundary index to the selection.</param>
        ''' <param name="strStatus">Optional status message to include.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal pm As cPropertyManager, _
                                    ByVal source As cCoreInputOutputBase, _
                                    ByVal varName As eVarNameFlags, _
                                    Optional ByVal sourceSec As cCoreInputOutputBase = Nothing, _
                                    Optional ByVal strStatus As String = "")

            Dim prop As cProperty = Nothing

            If source IsNot Nothing Then
                ' Get property
                prop = pm.GetProperty(source, varName, sourceSec)
            End If

            Me.Invoke(prop, strStatus)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="cCommand.Invoke">Invokes</see> the command, broadcasting a
        ''' data selection throughout the GUI.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke()
            ' Clear list of props
            Me.m_lprop.Clear()
            Me.Status = ""
            ' Fire the command
            MyBase.Invoke()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="cCommand.Invoke">Invokes</see> the command, broadcasting a
        ''' data selection throughout the GUI.
        ''' </summary>
        ''' <param name="prop">The <see cref="cProperty">cProperty</see> that 
        ''' was selected.</param>
        ''' <param name="strStatus">Optional status message to include.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal prop As cProperty, _
                                    Optional strStatus As String = "")
            ' Clear list of props
            Me.m_lprop.Clear()
            Me.m_lprop.Add(prop)
            Me.Status = strStatus
            ' Fire the command
            MyBase.Invoke()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="cCommand.Invoke">Invokes</see> the command, broadcasting a
        ''' data selection throughout the GUI.
        ''' </summary>
        ''' <param name="aprop">Array of <see cref="cProperty">cProperty</see> 
        ''' instances that were selected.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal aprop() As cProperty, _
                                    Optional strStatus As String = "")
            ' Clear list of props
            Me.m_lprop.Clear()
            Me.m_lprop.AddRange(aprop)
            Me.Status = strStatus
            ' Fire the command
            MyBase.Invoke()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="cCommand.Invoke">Invokes</see> the command, broadcasting a
        ''' data selection throughout the GUI.
        ''' </summary>
        ''' <param name="lprop">List of <see cref="cProperty">cProperty</see> 
        ''' instances that were selected.</param>
        ''' <param name="event">The <see cref="SelectionChangeEventType">event</see> that fired this command.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal lprop As List(Of cProperty), _
                                    ByVal [event] As SelectionChangeEventType, _
                                    Optional ByVal strStatus As String = "")
            ' Clear list of props
            Me.m_lprop.Clear()
            ' Store prop
            Me.m_lprop.AddRange(lprop)
            Me.m_event = [event]
            Me.Status = strStatus
            ' Fire the command
            MyBase.Invoke()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get and array of currently selected <see cref="cProperty">cProperty</see> 
        ''' instances.
        ''' </summary>
        ''' <remarks>
        ''' Note that only properties are returned that have not been <see cref="cProperty.IsDisposed">disposed</see> yet.
        ''' The EwE6 UI does not know disposal events, and adding this type of event
        ''' would be beneficial to the application but requires some thorough rethinking.
        ''' The Disposal test is a simple work-around for bug #1105.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Selection() As cProperty()
            Get
                Dim lSelection As New List(Of cProperty)

                For Each prop As cProperty In Me.m_lprop
                    Dim bValid As Boolean = True

                    If (prop.IsDisposed) Then bValid = False
                    If (prop.Source IsNot Nothing) Then bValid = Not prop.Source.Disposed

                    If bValid Then
                        lSelection.Add(prop)
                    End If
                Next
                Return lSelection.ToArray()
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="SelectionChangeEventType">event</see> that fired this command.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property EventType() As SelectionChangeEventType
            Get
                Return Me.m_event
            End Get
        End Property

    End Class

End Namespace ' Properties
