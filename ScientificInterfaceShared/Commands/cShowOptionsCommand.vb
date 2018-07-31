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
Imports ScientificInterfaceShared.Definitions

#End Region ' Imports

Namespace Commands

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Command to launch the 'options' interface in EwE.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cShowOptionsCommand
        Inherits cCommand

#Region " Private vars "

        Private m_option As eApplicationOptionTypes = eApplicationOptionTypes.General

#End Region ' Private vars

#Region " Public interfaces "

        ''' -----------------------------------------------------------------------
        ''' <summary>The name of this command.</summary>
        ''' <example>
        ''' The folowing shows how to retrieve the one instance of the cShowOptionsCommand
        ''' from anywhere in the EwE6 user interface:
        ''' <code>
        ''' ' Get the one and only command 
        ''' Dim cdmH As cCommandHandler = cCommandHandler.GetInstance()
        ''' ' Get the one and only ecosim save data command
        ''' Dim cmd As cShowOptionsCommand = DirectCast(GetCommand(cShowOptionsCommand.COMMAND_NAME), cShowOptionsCommand)
        ''' ' Invoke the command
        ''' ...
        ''' </code>
        ''' </example>
        ''' -----------------------------------------------------------------------
        Public Const cCOMMAND_NAME As String = "~showoptions~"

        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, cCOMMAND_NAME)
        End Sub

        Public Overloads Sub Invoke(Optional ByVal [option] As eApplicationOptionTypes = eApplicationOptionTypes.General)
            ' Set option
            Me.m_option = [option]
            MyBase.Invoke()
            Me.m_option = eApplicationOptionTypes.General
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eApplicationOptionTypes">application option</see> 
        ''' that this command was invoked for.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property [Option] As eApplicationOptionTypes
            Get
                Return Me.m_option
            End Get
        End Property

#End Region ' Public interfaces

    End Class

End Namespace
