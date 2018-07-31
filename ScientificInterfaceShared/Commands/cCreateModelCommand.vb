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
' Copyright 1991-2013 UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwEUtils.Core
Imports EwEUtils.Commands
Imports EwEUtils.Database

#End Region ' Imports

Namespace Commands

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Command to launch a hyperlink, which can be either a url or a file/folder path.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cCreateModelCommand
        Inherits cCommand

        Private m_strFileName As String = ""
        Private m_strModelName As String = ""
        Private m_format As eDataSourceTypes = eDataSourceTypes.NotSet

        ''' -----------------------------------------------------------------------
        ''' <summary>The name of this command.</summary>
        ''' <example>
        ''' The folowing shows how to retrieve the one instance of the cBrowserCommand
        ''' from anywhere in the EwE6 user interface:
        ''' <code>
        ''' ' Get the one and only command 
        ''' Dim cdmH As CommandHandler = CommandHandler.GetInstance()
        ''' ' Get the one and only navigation command
        ''' Dim cmd As cBrowserCommand = DirectCast(GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
        ''' ' Invoke the command
        ''' ...
        ''' </code>
        ''' </example>
        ''' -----------------------------------------------------------------------
        Public Shared COMMAND_NAME As String = "~createmodel"

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of the NavigationCommand class.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, COMMAND_NAME)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Invokes the command to make the EwE6 GUI navigate to user interface
        ''' element defined by this call.
        ''' </summary>
        ''' <param name="strFileName">Location of the new model.</param>
        ''' <param name="strModelName">Name to assign to the new model.</param>
        ''' <param name="format"><see cref="eDataSourceTypes">Format</see> of the new model.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal strFileName As String, ByVal strModelName As String, ByVal format As eDataSourceTypes)

            Me.m_strFileName = strFileName
            Me.m_strModelName = strModelName
            Me.m_format = format
            Me.Database = Nothing

            MyBase.Invoke()

            Me.m_strFileName = ""
            Me.m_strModelName = ""
            Me.m_format = eDataSourceTypes.NotSet

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the location of the new model.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property FileName() As String
            Get
                Return Me.m_strFileName
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the name to assign to the new model.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property ModelName() As String
            Get
                Return Me.m_strModelName
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the format of the new model.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Format() As eDataSourceTypes
            Get
                Return Me.m_format
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the most recently created database.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Database As cEwEDatabase

    End Class

End Namespace
