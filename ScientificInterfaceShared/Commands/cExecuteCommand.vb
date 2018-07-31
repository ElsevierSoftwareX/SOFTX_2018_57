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
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Commands

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Command to request remote execution of an instruction.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cExecuteCommand
        Inherits cCommand

#Region " Private vars "

        ' ToDo: use eCoreComponentType, eMessageImportance here?

        ''' <summary>Command to execute.</summary>
        Private m_strCommand As String = ""
        ''' <summary>Dictionary of execution parameters.</summary>
        Private m_dictParams As New Dictionary(Of String, String)

#End Region ' Private vars

        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, COMMAND_NAME)
        End Sub

#Region " Public interfaces "

        ''' -----------------------------------------------------------------------
        ''' <summary>The name of this command.</summary>
        ''' -----------------------------------------------------------------------
        Public Shared COMMAND_NAME As String = "~execute"

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' The command (string) to execute.
        ''' </summary>
        ''' <remarks>The command string is converted to LOWER CASE.</remarks>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Command() As String
            Get
                Return Me.m_strCommand
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set parameters in for this command.
        ''' </summary>
        ''' <param name="strName">Name of the parameter.</param>
        ''' <remarks>Parameter names and values are converted to LOWER CASE.</remarks>
        ''' -----------------------------------------------------------------------
        Public Property Parameter(ByVal strName As String) As String
            Get
                strName = strName.ToLower()
                If Me.m_dictParams.ContainsKey(strName) Then
                    Return Me.m_dictParams(strName)
                End If
                Return ""
            End Get
            Set(ByVal strValue As String)
                strName = strName.ToLower()
                If Me.m_dictParams.ContainsKey(strName) Then
                    Me.m_dictParams(strName) = strValue
                Else
                    Me.m_dictParams.Add(strName, strValue)
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Invoke the command.
        ''' </summary>
        ''' <param name="strCommand">Command string to pass to the command.</param>
        ''' -----------------------------------------------------------------------
        Public Shadows Sub Invoke(ByVal strCommand As String)

            ' Sanity check
            Debug.Assert(Not String.IsNullOrEmpty(strCommand))

            ' Store command
            Me.m_strCommand = strCommand.ToLower()
            ' Invoke!
            MyBase.Invoke()

            ' Clear command values to prepare it for next usage
            Me.m_strCommand = ""
            Me.m_dictParams.Clear()

        End Sub

#End Region ' Public interfaces

    End Class

End Namespace
