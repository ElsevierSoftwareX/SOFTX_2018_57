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
Imports System.Diagnostics

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Utility class for writing entries to the Windows Event log.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEventLog

    Private m_strAppName As String = ""

    ''' <summary>
    ''' Create a n
    ''' </summary>
    ''' <param name="strAppName">Name of Client Application. Needed because before 
    ''' writing to event log, you must have a named EventLog source.</param>
    ''' <param name="strLogName">Name of Log (System, Application, Security is 
    ''' read-only) If you specify a non-existent log, the log will be created</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal strAppName As String, _
                   Optional ByVal strLogName As String = "Application")

        Me.m_strAppName = strAppName

        'Register the App as an Event Source
        If Not EventLog.SourceExists(strAppName) Then
            EventLog.CreateEventSource(strAppName, strLogName)
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write entry to the Windows Event log.
    ''' </summary>
    ''' <param name="strEntry">Value to write</param>
    ''' <param name="eventlogentry"><see cref="EventLogEntryType">Entry Type</see>.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function WriteToEventLog(ByVal strEntry As String, _
                                    Optional ByVal eventlogentry As EventLogEntryType = EventLogEntryType.Information) As Boolean

        Dim objEventLog As New EventLog()
        Dim bSucces As Boolean = True

        objEventLog.Source = Me.m_strAppName
        Try
            objEventLog.WriteEntry(strEntry, eventlogentry)
        Catch Ex As Exception
            bSucces = False
        End Try

        Try
            objEventLog.Close()
            objEventLog.Dispose()
        Catch ex As Exception

        End Try

        Return bSucces

    End Function

End Class
