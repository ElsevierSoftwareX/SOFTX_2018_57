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

Option Explicit On
Imports System
Imports System.IO
Imports System.Threading
Imports System.Data
Imports System.Diagnostics
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Core

    ''' <summary>
    ''' Class encapsulating writing of messages to a log file.
    ''' </summary>
    Public Class cLog

#Region " Private Data "

        Private Shared m_lock As New ReaderWriterLock()
        Private Shared m_logwriter As cXMLLogWriter = Nothing
        Private Shared m_typeLogWriter As Type = GetType(cXMLLogWriter)
        Private Shared m_strModelPath As String = ""

#End Region

#Region " Log methods "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the level of message detail that the log should register.
        ''' </summary>
        ''' <remarks>Several log events can be tagged with a <see cref="eVerboseLevel">verbose level</see>,
        ''' which is measured against the level of detail that a user wants to
        ''' include in the log to determine whether event will be written to the log.</remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Property VerboseLevel As eVerboseLevel = eVerboseLevel.Standard

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of the current log file.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Shared ReadOnly Property LogFile As String
            Get
                Return GetWriter().Location
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Start a new log file with the model name as part of the log file name
        ''' </summary>
        ''' <param name="strModelPath">File path to the model to create a log file for.</param>
        ''' <param name="typeLogWriter">Optional type, inherited from <see cref="ILogWriter"/>,
        ''' of the type of log writer to use. If nothing is specified the <see cref="cXMLLogWriter">default 
        ''' XML log file writer</see> will be used.</param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub InitLog(ByVal strModelPath As String, Optional typeLogWriter As Type = Nothing)

            cLog.m_logwriter = Nothing
            cLog.m_strModelPath = strModelPath

            If (typeLogWriter IsNot Nothing) Then
                If (typeLogWriter.IsAssignableFrom(GetType(ILogWriter))) Then
                    cLog.m_typeLogWriter = typeLogWriter
                End If
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Write an <see cref="Exception"/> to the log
        ''' </summary>
        ''' <param name="theException">Exception to write to the log.</param>
        ''' <param name="level"><see cref="eVerboseLevel">Verbose level</see>.</param>
        ''' <param name="strMsg">Optional text to add.</param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub Write(ByVal theException As Exception,
                                ByVal level As eVerboseLevel,
                                Optional ByVal strMsg As String = "")
            If (level > cLog.VerboseLevel) Then Return
            cLog.Write(theException, strMsg)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Write an <see cref="Exception"/> to the log at 
        ''' <see cref="eVerboseLevel.Standard">standard verbose level</see>.
        ''' </summary>
        ''' <param name="theException">Exception to write to the log.</param>
        ''' <param name="strDetail">Optional text to add.</param>
        ''' <remarks>
        ''' This will log the exception text and all nested exceptions.
        '''</remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Sub Write(ByVal theException As Exception,
                                Optional ByVal strDetail As String = "")

            If Not AcquireWriterLock() Then Return
            Try
                GetWriter().Write(theException, strDetail)
            Catch ex As Exception

            End Try
            ReleaseWriterLock()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Write a <see cref="IMessage"/> to the log.
        ''' </summary>
        ''' <param name="message">The <see cref="IMessage"/> to write.</param>
        ''' <param name="level"><see cref="eVerboseLevel">Verbose level</see>.</param>
        ''' <param name="strDetail">Optional text to add.</param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub Write(ByVal message As IMessage,
                                ByVal level As eVerboseLevel,
                                Optional ByVal strDetail As String = "")
            If (level > cLog.VerboseLevel) Then Return
            cLog.Write(message, strDetail)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Write a <see cref="IMessage"/> to the log at <see cref="eVerboseLevel.Standard"/> level.
        ''' </summary>
        ''' <param name="message">The <see cref="IMessage"/> to write.</param>
        ''' <param name="strDetail">Optional text to add.</param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub Write(ByVal message As IMessage, Optional ByVal strDetail As String = "")

            If Not AcquireWriterLock() Then Return
            Try
                GetWriter().Write(message, strDetail)
            Catch ex As Exception

            End Try
            ReleaseWriterLock()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Write a string to the application log.
        ''' </summary>
        ''' <param name="msg">Message string to write.</param>
        ''' <param name="level"><see cref="eVerboseLevel">Verbose level</see>.</param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub Write(ByVal msg As String, ByVal level As eVerboseLevel)
            If (level > cLog.VerboseLevel) Then Return
            cLog.Write(msg)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Write a string to the application log.
        ''' </summary>
        ''' <param name="msg">Message string to write.</param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub Write(ByVal msg As String)

            If Not AcquireWriterLock() Then Return
            Try
                GetWriter.Write(msg)
            Catch ex As Exception

            End Try
            ReleaseWriterLock()

        End Sub

#End Region ' Log methods

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Singleton interface for creating a <see cref="ILogWriter"/> instance
        ''' </summary>
        ''' <returns>A ILogWriter instance that can be opened and written to.</returns>
        ''' <remarks>The log writer will write information for the model specified
        ''' in <see cref="InitLog(String, Type)"/>.</remarks>
        ''' -----------------------------------------------------------------------
        Private Shared Function GetWriter() As ILogWriter

            If (cLog.m_logwriter Is Nothing) Then
                Try
                    cLog.m_logwriter = CType(Activator.CreateInstance(cLog.m_typeLogWriter), cXMLLogWriter)
                Catch ex As Exception
                    ' Fallback
                    Debug.Assert(False, ex.Message)
                    cLog.m_logwriter = New cXMLLogWriter()
                End Try
                cLog.m_logwriter.InitLog(cLog.m_strModelPath)
                cLog.m_logwriter.StartSession()
            End If
            Return cLog.m_logwriter

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Aquire a critical section writer lock to the log file to initiate a
        ''' write operation.
        ''' </summary>
        ''' <returns>True if a lock was acquired.</returns>
        ''' <remarks>
        ''' <para>ReaderWriterLock.AcquireWriterLock() will throw an exception if it 
        ''' times out! This keeps the exception handling out of the main code.</para>
        ''' <para>The writer lock must be released using <see cref="ReleaseWriterLock"/>.</para>
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Shared Function AcquireWriterLock() As Boolean
            Try
                'Wait 10 seconds for a lock
                cLog.m_lock.AcquireWriterLock(10000)
                Return True
            Catch ex As Exception
                Console.WriteLine("Error trying to lock the Log file for writting! " & ex.Message)
                Return False
            End Try
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Release a critical section writer lock to the log file previously 
        ''' obtained via <see cref="AcquireWriterLock"/>.
        ''' write operation.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Shared Sub ReleaseWriterLock()
            Try
                cLog.m_lock.ReleaseWriterLock()
            Catch ex As Exception
                Console.WriteLine("Error trying to unlock the Log file after writting! " & ex.Message)
            End Try
        End Sub

#End Region ' Internals

#Region "Interfaces for writting Debugging files"

        ''' <summary>
        ''' Writes a load of text to a text file, obliterating anything in it's way.
        ''' </summary>
        ''' <param name="strFilename"></param>
        ''' <param name="sb"></param>
        ''' <param name="strHeader"></param>
        ''' <remarks></remarks>
        Public Shared Sub WriteTextToFile(ByVal strFilename As String, ByVal sb As Text.StringBuilder, _
                Optional ByVal bAppend As Boolean = False, Optional ByVal strHeader As String = "")

            Dim strm As System.IO.StreamWriter = Nothing

            Try
                Dim strTarget As String = FixDirectory(strFilename)
                If bAppend Then
                    strm = System.IO.File.AppendText(strTarget)
                Else
                    strm = System.IO.File.CreateText(strTarget)
                End If

                If Not String.IsNullOrWhiteSpace(strHeader) Then
                    strm.WriteLine(strHeader)
                    strm.WriteLine()
                End If
                strm.WriteLine(sb.ToString())
                strm.Close()

            Catch ex As Exception
                cLog.Write("Error in WriteFile(...) Error: " + ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Append the contents of a singly dimensioned array to a csv file. Each call is a new line in the file.
        ''' </summary>
        ''' <param name="strFilename">Name of the file to append</param>
        ''' <param name="array">Array whose contents get written to new line in the file</param>
        ''' <remarks>Used for debugging to test the contents of an array against the original code
        ''' the data is appended so that it can be written to multiple time each call is a new line
        ''' </remarks>
        Public Shared Sub WriteArrayToFile(ByVal strFilename As String, ByVal array() As Single, Optional ByVal strHeader As String = "")
            Dim strm As System.IO.StreamWriter
            Dim n As Integer = array.GetLength(0)
            Dim i As Integer

            Try
                Dim strTarget As String = FixDirectory(strFilename)
                strm = System.IO.File.AppendText(strFilename)
                If strHeader <> "" Then
                    strm.Write(strHeader)
                    strm.Write(", ")
                End If

                For i = 0 To n - 1
                    strm.Write(array(i).ToString("###0.00000##"))
                    '       strm.Write(array(i))
                    If i < n - 1 Then
                        strm.Write(", ")
                    End If
                Next i
                strm.Write(Environment.NewLine)
                strm.Close()

            Catch ex As Exception
                cLog.Write("Error in WriteArrayToFile(...) Error: " + ex.Message)
            End Try

        End Sub


        ''' <summary>
        ''' Append the contents of a 3 "map" array to file. The data will be written row, col, group. Each call is a new block in the file.
        ''' </summary>
        ''' <param name="strFilename">Name of the file to append</param>
        ''' <param name="array">Array whose contents get written to new line in the file</param>
        ''' <remarks>Used for debugging to test the contents of an array against the original code
        ''' the data is appended so that it can be written to multiple time each call is a new block
        ''' </remarks>
        Public Shared Sub WriteGroupMapToFile(ByVal strFilename As String, ByVal array(,,) As Single, Optional ByVal strHeader As String = "")
            Dim strm As System.IO.StreamWriter
            Dim n1 As Integer = array.GetUpperBound(0)
            Dim n2 As Integer = array.GetUpperBound(1)
            Dim n3 As Integer = array.GetUpperBound(2)
            Dim i As Integer, j As Integer, igrp As Integer

            Try
                Dim strTarget As String = FixDirectory(strFilename)
                strm = System.IO.File.AppendText(strFilename)

                If strHeader <> "" Then
                    strm.WriteLine(strHeader)
                End If

                For igrp = 1 To n3
                    For i = 1 To n1
                        For j = 1 To n2
                            strm.Write(array(i, j, igrp).ToString())
                            strm.Write(",")
                        Next j
                        strm.WriteLine("")
                    Next i
                    strm.WriteLine("")
                    strm.WriteLine(igrp.ToString)
                Next igrp
                strm.Close()

            Catch ex As Exception
                cLog.Write("Error in WriteArrayToFile(...) Error: " + ex.Message)
            End Try


        End Sub

        ''' <summary>
        ''' Append the contents of a singly dimensioned array to a csv file. Each call is a new line in the file.
        ''' </summary>
        ''' <param name="strFilename">Name of the file to append</param>
        ''' <param name="array">Array whose contents get written to new line in the file</param>
        ''' <remarks>Used for debugging to test the contents of an array against the original code
        ''' the data is appended so that it can be written to multiple time each call is a new line
        ''' </remarks>
        Public Shared Sub WriteArrayToFile(ByVal strFilename As String, ByVal array() As Double, Optional ByVal strHeader As String = "")
            Dim strm As System.IO.StreamWriter
            Dim n As Integer = array.GetLength(0)
            Dim i As Integer

            Try
                Dim strTarget As String = FixDirectory(strFilename)
                strm = System.IO.File.AppendText(strFilename)
                If strHeader <> "" Then
                    strm.Write(strHeader)
                    strm.Write(", ")
                End If
                For i = 0 To n - 1
                    'CStr(Format(GetType(Integer), i, "00")
                    strm.Write(array(i).ToString("###0.00000##"))
                    If i < n - 1 Then
                        strm.Write(", ")
                    End If
                Next i
                strm.Write(Environment.NewLine)
                strm.Close()

            Catch ex As Exception
                cLog.Write("Error in WriteArrayToFile(...) Error: " + ex.Message)
            End Try

        End Sub


        ''' <summary>
        ''' Append the contents of a matrix array to a csv file. Each call is a new block in the file.
        ''' </summary>
        ''' <param name="strFilename">Name of the file to append</param>
        ''' <param name="array">Array whose contents get written to new line in the file</param>
        ''' <remarks>Used for debugging to test the contents of an array against the original code
        ''' the data is appended so that it can be written to multiple time each call is a new block
        ''' </remarks>
        Public Shared Sub WriteMatrixToFile(ByVal strFilename As String, ByVal array(,) As Single, Optional ByVal strHeader As String = "")
            Dim strm As System.IO.StreamWriter
            Dim n1 As Integer = array.GetUpperBound(0)
            Dim n2 As Integer = array.GetUpperBound(1)
            Dim i As Integer, j As Integer

            Try
                Dim strTarget As String = FixDirectory(strFilename)
                strm = System.IO.File.AppendText(strFilename)

                If strHeader <> "" Then
                    strm.WriteLine(strHeader)
                End If

                For i = 0 To n1
                    For j = 0 To n2
                        '  strm.Write(Format(array(i, j), "###0.00000##"))
                        strm.Write(array(i, j).ToString("###0.00000##"))
                        If j < n2 Then
                            strm.Write(", ")
                        End If

                    Next j
                    strm.Write(Environment.NewLine)
                Next i
                strm.Close()

            Catch ex As Exception
                cLog.Write("Error in WriteArrayToFile(...) Error: " + ex.Message)
            End Try

        End Sub


        ''' <summary>
        ''' Append the contents of a 3 dimensional array to a csv file. Each call is a new block in the file.
        ''' </summary>
        ''' <param name="strFilename">Name of the file to append</param>
        ''' <param name="array">Array whose contents get written to new line in the file</param>
        ''' <remarks>Used for debugging to test the contents of an array against the original code
        ''' the data is appended so that it can be written to multiple time each call is a new block
        ''' </remarks>
        <CLSCompliant(False)>
        Public Shared Sub WriteMatrixToFile(ByVal strFilename As String, ByVal array(,,) As Single, Optional ByVal strHeader As String = "")
            Dim strm As System.IO.StreamWriter
            Dim n1 As Integer = array.GetUpperBound(0)
            Dim n2 As Integer = array.GetUpperBound(1)
            Dim n3 As Integer = array.GetUpperBound(2)
            Dim i As Integer, j As Integer, k As Integer

            Try
                Dim strTarget As String = FixDirectory(strFilename)
                strm = System.IO.File.AppendText(strFilename)

                If strHeader <> "" Then
                    strm.WriteLine(strHeader)
                End If

                For i = 0 To n1
                    For j = 0 To n2
                        For k = 0 To n3
                            '  strm.Write(Format(array(i, j), "###0.00000##"))
                            strm.Write(array(i, j, k).ToString("###0.00000##"))
                            If k < n3 Then
                                strm.Write(", ")
                            End If
                        Next k
                        strm.WriteLine("i=" & i & " j=" & j)
                    Next j
                    '    strm.Write(ControlChars.NewLine)
                    strm.WriteLine("")
                Next i
                strm.Close()

            Catch ex As Exception
                cLog.Write("Error in WriteArrayToFile(...) Error: " + ex.Message)
            End Try


        End Sub

        Private Shared Function FixDirectory(strFileName As String) As String

            Dim strDir As String = Path.GetDirectoryName(strFileName)
            If String.IsNullOrWhiteSpace(strDir) Then
                strDir = System.AppDomain.CurrentDomain.BaseDirectory()
                strFileName = Path.Combine(strDir, strFileName)
            End If

            If Not Directory.Exists(strDir) Then
                Directory.CreateDirectory(strDir)
            End If

            Return strFileName

        End Function

#End Region

    End Class

End Namespace
