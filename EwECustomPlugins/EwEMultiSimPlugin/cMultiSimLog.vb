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
Imports System.IO
Imports EwECore
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Simple log file to register multisim execution events.
''' </summary>
''' ---------------------------------------------------------------------------
Friend Class cMultiSimLog

#Region " Private vars "

    Private m_core As cCore = Nothing
    Private m_eng As cEngine = Nothing

    Private m_sw As StreamWriter = Nothing
    Private m_strLogFile As String = ""

    Private Const g_sLogFile As String = "MultiSimLog.txt"

#End Region ' Private vars

#Region " Construction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="core">The <see cref="cCore"/> to operate against.</param>
    ''' <param name="eng">The <see cref="cEngine"/> to operate against.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore, ByVal eng As cEngine)
        Me.m_core = core
        Me.m_eng = eng
    End Sub

#End Region ' Construction

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Open a new session in the log.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Open() As Boolean

        If (Me.IsOpen()) Then Me.Close()

        If (Not cFileUtils.IsDirectoryAvailable(Me.m_eng.OutputPath, True)) Then Return False

        Try
            Me.m_strLogFile = Path.Combine(Me.m_eng.OutputPath, cMultiSimLog.g_sLogFile)
            Me.m_sw = New StreamWriter(Me.m_strLogFile)
            If (Me.m_core.SaveWithFileHeader) Then
                Me.m_sw.WriteLine(Me.m_core.DefaultFileHeader(EwEUtils.Core.eAutosaveTypes.Ecosim))
            End If
        Catch ex As Exception
            ' Plok
        End Try
        Return Me.IsOpen()

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get whether a session is active in the log.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsOpen() As Boolean
        Get
            Return (Me.m_sw IsNot Nothing)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Close the current session in the log.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Close() As Boolean

        If (Not Me.IsOpen()) Then Return False
        Me.m_sw.Flush()
        Me.m_sw.Close()
        Me.m_sw.Dispose()
        Me.m_sw = Nothing
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the name of the file where the log is written to.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property FileName() As String
        Get
            Return Me.m_strLogFile
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a line of text to the log.
    ''' </summary>
    ''' <param name="strLine">The line to add.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Add(ByVal strLine As String) As Boolean
        If (Not Me.IsOpen()) Then Return False
        Dim dt As DateTime = DateTime.Now
        Me.m_sw.WriteLine(dt.ToShortDateString & " " & dt.ToShortTimeString & ": " & strLine)
        Return True
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a <see cref="cVariableStatus"/> entry to the log.
    ''' </summary>
    ''' <param name="vs">The <see cref="cVariableStatus"/> to add.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Add(ByVal vs As cVariableStatus) As Boolean
        Return Me.Add(vs.Status.ToString & cStringUtils.vbTab & cStringUtils.ToCSVField(vs.Message))
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a <see cref="cMessage"/> entry to the log with all of its <see cref="cVariableStatus">sub-messages</see>.
    ''' </summary>
    ''' <param name="msg">The <see cref="cMessage"/> to add.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Add(ByVal msg As cMessage) As Boolean
        Dim bSuccess As Boolean = Me.Add(cStringUtils.ToCSVField(msg.Message))
        For Each vs As cVariableStatus In msg.Variables
            bSuccess = bSuccess And Me.Add(vs)
        Next
        Return bSuccess
    End Function

End Class
