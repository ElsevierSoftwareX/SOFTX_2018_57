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
Imports EwECore.Ecosim
Imports ScientificInterfaceShared.Commands
Imports System.Drawing.Printing

#End Region ' Imports

Namespace Commands

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' The cEcosimSaveDataCommand class implements a <see cref="cCommand">Command</see>
    ''' that is used in EwE6 to save Ecosim data to csv file.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cEcosimSaveDataCommand
        Inherits cCommand

        Private m_results As cEcosimResultWriter.eResultTypes() = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>The name of this command.</summary>
        ''' <example>
        ''' The folowing shows how to retrieve the one instance of the cEcosimSaveDataCommand
        ''' from anywhere in the EwE6 user interface:
        ''' <code>
        ''' ' Get the one and only command 
        ''' Dim cdmH As cCommandHandler = cCommandHandler.GetInstance()
        ''' ' Get the one and only ecosim save data command
        ''' Dim cmd As cEcosimSaveDataCommand = DirectCast(GetCommand(cEcosimSaveDataCommand.COMMAND_NAME), cEcosimSaveDataCommand)
        ''' ' Invoke the command
        ''' ...
        ''' </code>
        ''' </example>
        ''' -----------------------------------------------------------------------
        Public Shared COMMAND_NAME As String = "ExportEcosimResultsToCSV"

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this class.
        ''' </summary>
        ''' <param name="cmdh">The <see cref="cCommandHandler"/> to associate this command with.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, COMMAND_NAME)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <inheritdocs cref="cCommand.Invoke"/>
        ''' <param name="results">Optional array of results to output.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(Optional ByVal results As cEcosimResultWriter.eResultTypes() = Nothing)
            Me.m_results = results
            MyBase.Invoke()
            Me.m_results = Nothing
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' The array of results to output, ordered when the command was 
        ''' <see cref="Invoke">invoked</see>.
        ''' </summary>
        ''' <remarks>
        ''' Note that the value returned here is only valid while the command is invoking.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Results() As cEcosimResultWriter.eResultTypes()
            Get
                Return Me.m_results
            End Get
        End Property

    End Class

End Namespace
