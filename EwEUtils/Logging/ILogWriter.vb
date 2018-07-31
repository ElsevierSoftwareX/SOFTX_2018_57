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
Imports EwEUtils.Core

#End Region ' Imports

''' <summary>
''' Definitions for implementing a EwE log writer.
''' </summary>
Public Interface ILogWriter

    Function InitLog(strModelPath As String) As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Opens the log writer for writing.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Function Open() As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Closes the log writer for writing.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Sub Close()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the location where the log writer writes to. This can be a file
    ''' for file-based log writers, a URL for net-based writers, or any other
    ''' location really.
    ''' </summary>
    ''' <returns>The textual description of the log location.</returns>
    ''' -----------------------------------------------------------------------
    Function Location() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Starts the session by writing log opening information.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Sub StartSession()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write an exception to the log.
    ''' </summary>
    ''' <param name="theException">The exception to write</param>
    ''' <param name="strDetails">A detail providing context to the exception.</param>
    ''' -----------------------------------------------------------------------
    Sub Write(ByVal theException As Exception, ByVal strDetails As String)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write a <see cref="IMessage"/> to the log.
    ''' </summary>
    ''' <param name="message">The message to write</param>
    ''' <param name="strDetails">A detail providing context to the message.</param>
    ''' -----------------------------------------------------------------------
    Sub Write(ByVal message As IMessage, ByVal strDetails As String)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write a string to the log.
    ''' </summary>
    ''' <param name="msg">Message string to write.</param>
    ''' -----------------------------------------------------------------------
    Sub Write(ByVal msg As String)

End Interface
