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

Option Strict On
Option Explicit On

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Xml
Imports EwEUtils.Utilities
Imports EwEUtils.SystemUtilities

Namespace Core

    ''' <summary>
    ''' XML writer for appening data to the end of an XML log file
    ''' </summary>
    ''' <remarks>This class appends XML messages to the end of a log file. XML writters do not like to append data so it has to jump through a bunch of hoops to do this.</remarks>
    Public Class cXMLLogWriter
        Inherits XmlWriter
        Implements ILogWriter

        ''' <summary>
        ''' Max size of the log file in bytes. One megabyte
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared MAX_LOG_SIZE As Integer = CInt(1024 ^ 2)

        'Inheriting from an XmlWriter
        'most of the methods are declared as MustOverride so the inheritance is kind of bogus 
        'you don’t get a lot of functionality

        'Hack Warning This is a little strange
        'If an XmlWriter is attached to a FileStream and the FileStream calls FileStream.Seek the XmlWriter will write a bogus character at the start of the stream
        'This messes up the XML file
        'So
        'The XmlWriter is attached to a string stream (m_stringstrm As StringWriter) not to a file stream. m_XMLwriter = XmlWriter.Create(m_stringstrm, m_settings)
        'There is a TextWriter (m_textwriter As TextWriter) that is attached to the file stream.
        'When a message is written to the XmlWriter log it is buffered in the string stream (m_stringstrm)
        'then when cXMLLogWriter.Close is called the StringWriter (m_stringstrm) is written to TextWriter (m_textwriter) which writes to the file stream it is attached to.

#Region " Private data "

        'the underlying XML Writer
        'when inheriting from an XmlWriter it does not supply much through the base class so we have to encapsulte the underlying writer to get functionality
        Private m_XMLwriter As XmlWriter
        Private m_filestream As FileStream
        Private m_strLogFileName As String = ""
        Private m_strModelName As String = ""

        Private m_stringstrm As StringWriter
        Private m_textwriter As TextWriter

        Private m_endDocXmlTag As String = "</doc>" 'if you change this make sure you look at FindDocumentEnd() it is hardwired in there backwards

#End Region ' Private data

#Region " Construction "

        Public Sub New()

        End Sub

#End Region ' Construction

#Region "Overridden Methods"

        Public Function Location() As String Implements ILogWriter.Location
            Return m_strLogFileName
        End Function

        ''' <summary>
        ''' Open the XML stream
        ''' </summary>
        Public Function Open() As Boolean Implements ILogWriter.Open

            DeleteLargeLogFiles()

            Try
                Dim settings As New XmlWriterSettings()

                'If no log file exists this will create a new one
                'If the file exists it will not do anything
                CreateNew()

                m_stringstrm = New StringWriter 'memory buffer the XmlWriter will use

                m_filestream = File.Open(m_strLogFileName, FileMode.OpenOrCreate)
                m_textwriter = New StreamWriter(m_filestream)

                'create a new xml stream that can be managed by the writer
                settings.Indent = True
                settings.NewLineOnAttributes = True
                settings.NewLineHandling = NewLineHandling.Replace

                'This is so we can write the raw xml code without the stream exploding
                'this is for the closing doc tag
                settings.ConformanceLevel = ConformanceLevel.Fragment
                settings.OmitXmlDeclaration = True

                'create an XMLWriter that is attached to a sting instead of the file
                m_XMLwriter = XmlWriter.Create(m_stringstrm, settings)

                'bump backwards from the end of the file to the start of the doc tag
                'or the end of the file if no doc tag is found
                FindDocumentEnd()

            Catch ex As Exception
                Console.WriteLine("CLog.Open() Exception: " + ex.Message)
                Return False
            End Try

            Return True

        End Function

        ''' <summary>
        ''' If the log file does not exist then create a new log and write out the xml header info. Otherwise just leave the file as it is.
        ''' </summary>
        ''' <remarks>This is call any time a file is opend to make sure the file exists. 
        ''' This way if the directory that contains the log files is cleared out then a new log file will be created next time it is needed.
        ''' </remarks>
        Private Sub CreateNew()
            Dim settings As New XmlWriterSettings
            Try
                If Not File.Exists(m_strLogFileName) Then
                    m_filestream = File.Open(m_strLogFileName, FileMode.CreateNew)
                    m_textwriter = New StreamWriter(m_filestream)

                    settings.Indent = True
                    settings.NewLineOnAttributes = True
                    settings.NewLineHandling = NewLineHandling.Replace
                    settings.OmitXmlDeclaration = False
                    settings.ConformanceLevel = ConformanceLevel.Document

                    m_XMLwriter = XmlWriter.Create(m_filestream, settings)
                    m_XMLwriter.WriteStartDocument()
                    m_XMLwriter.WriteStartElement("doc")
                    m_XMLwriter.WriteElementString("Platform", cSystemUtils.OSVersion())
                    m_XMLwriter.WriteElementString("Is64BitOS", If(cSystemUtils.Is64BitOS(), "True", "False"))
                    m_XMLwriter.WriteElementString("Is64BitEwE", If(cSystemUtils.Is64BitProcess(), "True", "False"))
                    m_XMLwriter.WriteElementString("CultureOS", Threading.Thread.CurrentThread.CurrentCulture.DisplayName)
                    m_XMLwriter.WriteElementString("CultureUI", Threading.Thread.CurrentThread.CurrentUICulture.DisplayName)
                    m_XMLwriter.WriteElementString("DecimalSeparator", Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                    m_XMLwriter.WriteElementString("ModelName", m_strModelName)

                    m_XMLwriter.WriteElementString("Created", String.Format("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString()))
                    m_XMLwriter.WriteEndElement()
                    m_XMLwriter.WriteEndDocument()

                    m_XMLwriter.Flush()
                    m_XMLwriter.Close()
                    m_filestream.Flush()
                    m_filestream.Close()

                End If

            Catch ex As Exception
                Console.WriteLine("CLog.CreateNew() Exception: " + ex.Message)
                Throw New ApplicationException(Me.ToString & ".CreateNew() Error: " & ex.Message)
            End Try

        End Sub

        ''' <summary>
        ''' Find the end closing doc xml tag at the end of the log file
        ''' </summary>
        ''' <remarks>If the doc tag is found this will move the file cursor to the start of the tag. 
        ''' This erases the tag from the file. The tag is re-written when the file is closed. 
        ''' If no doc tag is found this will leave the file cursor at the end of the file for appending the new data.</remarks>
        Private Sub FindDocumentEnd()

            Dim ipos As Integer
            Dim curByte(1) As Byte
            Dim x As String = "/"
            Dim byteToFind As Byte = System.Text.Encoding.ASCII.GetBytes(x.ToCharArray(), 0, 1)(0)
            Dim tagbuff As String = ""

            Try
                'make sure the file contains some data
                'this should not happen but if it does this will return with the file cursor at the start of the file
                If m_filestream.Length < 1 Then
                    Debug.Assert(False, Me.ToString & ".FindDocumentEnd() Warning: " & "File should contain some XML Data")
                    Exit Sub
                End If

                'Wierd stream behaviour
                'Seek and Read have to be called on the FileStream directly not an attached TextReader
                'using the TextWriter's Seek and Peek causes the file cursor to skip around to strange places in the file

                Do While curByte(0) <> byteToFind
                    ipos -= 1
                    Try
                        m_filestream.Seek(ipos, SeekOrigin.End) 'seek can throw an error if it tries to seek outside the file
                    Catch ex As Exception
                        Exit Do
                    End Try
                    If m_filestream.Read(curByte, 0, 1) = -1 Then
                        'end of the file before finding the end tag??????
                        Exit Do
                    End If
                    tagbuff = tagbuff + Convert.ToChar(curByte(0))
                    '   Console.Write(Chr(curByte(0)) & ", " & m_filestream.Position & ", ")
                Loop

                'make sure the tag we found is the </doc> tag 
                'tagbuff is backwards!
                If tagbuff.ToLower.Contains(">cod/") Then
                    'If InStr(tagbuff.ToLower, ">cod/", CompareMethod.Text) > 0 Then
                    m_filestream.Seek(ipos - 1, SeekOrigin.End)
                Else
                    'not the correct tag so just append to the end of the file
                    m_filestream.Seek(0, SeekOrigin.End)
                End If

            Catch ex As Exception
                Console.WriteLine("CLog.FindDocumentEnd() Exception: " + ex.Message)
                Throw New ApplicationException(Me.ToString & ".FindDocumentEnd() Error: " & ex.Message)
            End Try

        End Sub

        ''' <summary>
        ''' Seek backwards in the filestream for a character
        ''' </summary>
        Private Function SeekBack(ByRef TagToFind As String) As Boolean
            Dim curByte(1) As Byte
            Dim bEndFound As Boolean = True
            Dim tagbuff As String
            Dim iPos As Integer
            Dim flength As Long = m_filestream.Length

            tagbuff = ""
            Do While String.Compare(tagbuff.ToLower, TagToFind, ignoreCase:=True) <= 0
                'Do While InStr(tagbuff.ToLower, TagToFind, CompareMethod.Text) <= 0
                iPos -= 1
                If iPos <= -flength Then
                    bEndFound = False
                    Exit Do
                End If
                m_filestream.Seek(iPos, SeekOrigin.End)
                If m_filestream.Read(curByte, 0, 1) = -1 Then
                    'end of the file before finding the end tag??????
                    bEndFound = False
                    Exit Do
                End If
                tagbuff = tagbuff + Convert.ToChar(curByte(0))
            Loop

            If bEndFound Then
                m_filestream.Seek(iPos - 1, SeekOrigin.End)
            Else
                'not the correct tag so just append to the end of the file
                m_filestream.Seek(0, SeekOrigin.End)
            End If

            Return True

        End Function

        Public Overrides Sub WriteEndDocument()

            m_XMLwriter.WriteRaw(Environment.NewLine)
            m_XMLwriter.WriteRaw(m_endDocXmlTag)

        End Sub

        Public Function InitLog(strModelPath As String) As Boolean Implements ILogWriter.InitLog

            If Not String.IsNullOrWhiteSpace(strModelPath) Then
                Me.m_strModelName = cFileUtils.ToValidFileName(Path.GetFileNameWithoutExtension(strModelPath), False)
                Me.m_strLogFileName = Path.Combine(Path.GetDirectoryName(strModelPath), m_strModelName & "_log.xml")
            Else
                Me.m_strModelName = ""
                Me.m_strLogFileName = Path.Combine(cSystemUtils.ApplicationSettingsPath(), "EwELog.xml")
            End If
            Return True

        End Function

        Public Overrides Sub Close() Implements ILogWriter.Close
            Try
                Me.Flush()

                m_textwriter.Write(m_stringstrm.ToString)
                m_textwriter.Flush()
                '  m_textwriter.Close

                m_XMLwriter.Close()
                m_filestream.Flush()
                m_filestream.Close()

            Catch ex As Exception
                Console.WriteLine("cXMLLogWriter.Close() Exception: " + ex.Message)
            End Try

        End Sub

        Public Sub StartSession() Implements ILogWriter.StartSession
            Try
                If Me.Open() Then
                    Me.WriteStartElement("Session_Started")
                    Me.WriteAttributeString("Date", String.Format("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString()))
                    Me.WriteElementString("Model", Me.m_strModelName)
                    Me.WriteElementString("LogFile", Me.m_strLogFileName)
                    Me.WriteEndElement() 'Session_Started
                    Me.WriteEndDocument()

                    Me.Close()
                End If
            Catch ex As Exception

            End Try

        End Sub

        Public Sub Write(theException As Exception, strMsg As String) Implements ILogWriter.Write
            Try
                If Me.Open() Then

                    Dim trace As StackTrace = New StackTrace(theException, True)

                    'now the message
                    Me.WriteStartElement("Exception_Messages")
                    Me.WriteAttributeString("Date", String.Format("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString()))
                    If Not String.IsNullOrEmpty(strMsg) Then
                        Me.WriteElementString("Detail", strMsg)
                    End If
                    Dim thisEx As Exception = theException
                    Do While thisEx IsNot Nothing
                        Me.WriteStartElement("Exception")
                        Me.WriteElementString("Type", thisEx.GetType().ToString)
                        Me.WriteElementString("Source", thisEx.Source)
                        Me.WriteElementString("Message", thisEx.Message)
                        Me.WriteEndElement() 'Exception
                        thisEx = thisEx.InnerException
                    Loop

                    ' Stack trace
                    Me.WriteStartElement("StackTrace")
                    For Each frame As StackFrame In trace.GetFrames
                        Me.WriteStartElement("Method")
                        Me.WriteElementString("Name", frame.GetMethod.Name)
                        Me.WriteElementString("Line", CStr(frame.GetFileLineNumber))
                        If Not String.IsNullOrWhiteSpace(frame.GetFileName) Then
                            Me.WriteElementString("FileName", frame.GetFileName)
                        End If
                        Me.WriteEndElement() 'Method"
                    Next
                    Me.WriteEndElement() 'StackTrace

                    Me.WriteEndElement() 'Msg
                    Me.WriteEndDocument()

                    Me.Close()

                End If

            Catch ex As Exception
                Console.WriteLine("cXMLLogWriter.Write() Exception: " + ex.Message)
                Me.Close()
            End Try

        End Sub

        Public Sub Write(message As IMessage, strMsg As String) Implements ILogWriter.Write
            Try
                If Me.Open() Then

                    Me.WriteStartElement(message.Importance.ToString & "_Message") '????
                    Me.WriteAttributeString("Date", String.Format("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString()))
                    If Not String.IsNullOrEmpty(strMsg) Then
                        Me.WriteElementString("Detail", strMsg)
                    End If
                    Me.WriteElementString("Message", message.Message)
                    Me.WriteElementString("Message_Type", message.Type.ToString)
                    Me.WriteElementString("Message_Source", message.Source.ToString)
                    Me.WriteElementString("Message_DataType", message.DataType.ToString)
                    Me.WriteEndElement() 'Msg
                    Me.WriteEndDocument()

                    Me.Close()
                End If

            Catch ex As Exception
                Console.WriteLine("cXMLLogWriter.Write() Exception: " + ex.Message)
                Me.Close()
            End Try

        End Sub

        Public Sub Write(msg As String) Implements ILogWriter.Write

            Try
                If Me.Open() Then
                    Me.WriteStartElement("Log_Message")
                    Me.WriteAttributeString("Date", String.Format("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString()))
                    Me.WriteElementString("Message", msg)
                    Me.WriteEndElement() 'Log_Message
                    Me.WriteEndDocument()
                    Me.Close()
                End If
            Catch ex As Exception
                Console.WriteLine("cXMLLogWriter.Write() Exception: " & ex.Message)
                Me.Close()
            End Try

        End Sub

        ''' <summary>
        ''' Delete log files greater than MAX_LOG_SIZE (1mb).
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub DeleteLargeLogFiles()
            Try

                Dim fn As String = Me.m_strLogFileName
                If File.Exists(cFileUtils.ToValidFileName(Me.m_strLogFileName, True)) Then
                    Dim fi As FileInfo = New FileInfo(fn)
                    If fi.Length > MAX_LOG_SIZE Then
                        Console.WriteLine("cLog.DeleteLargeLogFiles() Deleting log file " & Me.m_strLogFileName)
                        File.Delete(fn)
                    End If 'fi.Length > MAX_LOG_SIZE
                End If 'File.Exists(cFileUtils.ToValidFileName(fn, True))

            Catch ex As Exception
                Console.WriteLine("cLog.DeleteLargeLogFiles() Exception while deleting old log file: " & ex.Message)
            End Try

        End Sub

#End Region

#Region " Methods with default behavior "

        Public Overloads Sub WriteElementString(ByVal ElementName As String, ByVal value As String)
            m_XMLwriter.WriteElementString(ElementName, value)
        End Sub

        Public Overloads Sub WriteAttributeString(ByVal AttributeName As String, ByVal value As String)
            m_XMLwriter.WriteAttributeString(AttributeName, value)
        End Sub

        Public Overrides Sub Flush()
            m_XMLwriter.Flush()
        End Sub

        Public Overrides Function LookupPrefix(ByVal ns As String) As String
            Return m_XMLwriter.LookupPrefix(ns)
        End Function

        Public Overrides Sub WriteBase64(ByVal buffer() As Byte, ByVal index As Integer, ByVal count As Integer)
            m_XMLwriter.WriteBase64(buffer, index, count)
        End Sub

        Public Overrides Sub WriteCData(ByVal text As String)
            m_XMLwriter.WriteCData(text)
        End Sub

        Public Overrides Sub WriteEndAttribute()
            m_XMLwriter.WriteEndAttribute()
        End Sub

        Public Overrides Sub WriteEndElement()
            m_XMLwriter.WriteEndElement()
        End Sub

        Public Overloads Overrides Sub WriteRaw(ByVal data As String)
            m_XMLwriter.WriteRaw(data)
        End Sub

        Public Overloads Overrides Sub WriteStartAttribute(ByVal prefix As String, ByVal localName As String, ByVal ns As String)
            m_XMLwriter.WriteStartAttribute(prefix, localName, ns)
        End Sub

        Public Overloads Sub WriteStartAttribute(ByVal AtributeName As String, ByVal value As String)
            m_XMLwriter.WriteStartAttribute(AtributeName, value)
        End Sub

        Public Overloads Overrides Sub WriteStartDocument()
            m_XMLwriter.WriteStartDocument()
        End Sub

        Public Overloads Overrides Sub WriteStartDocument(ByVal standalone As Boolean)
            m_XMLwriter.WriteStartDocument(standalone)
        End Sub

        Public Overloads Sub WriteStartElement(ByVal localName As String)
            m_XMLwriter.WriteStartElement(localName)
        End Sub

        Public Overloads Overrides Sub WriteStartElement(ByVal prefix As String, ByVal localName As String, ByVal ns As String)
            m_XMLwriter.WriteStartElement(prefix, localName, ns)
        End Sub

        Public Overrides ReadOnly Property WriteState() As System.Xml.WriteState
            Get
                Debug.Assert(False, "Not implemented for this class")
            End Get
        End Property

        Public Overrides Sub WriteString(ByVal text As String)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overrides Sub WriteSurrogateCharEntity(ByVal lowChar As Char, ByVal highChar As Char)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overrides Sub WriteWhitespace(ByVal ws As String)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overrides Sub WriteEntityRef(ByVal name As String)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overrides Sub WriteFullEndElement()
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overrides Sub WriteProcessingInstruction(ByVal name As String, ByVal text As String)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overloads Overrides Sub WriteRaw(ByVal buffer() As Char, ByVal index As Integer, ByVal count As Integer)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overrides Sub WriteCharEntity(ByVal ch As Char)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overrides Sub WriteChars(ByVal buffer() As Char, ByVal index As Integer, ByVal count As Integer)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overrides Sub WriteComment(ByVal text As String)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

        Public Overrides Sub WriteDocType(ByVal name As String, ByVal pubid As String, ByVal sysid As String, ByVal subset As String)
            Debug.Assert(False, "Not implemented for this class")
        End Sub

#End Region ' Methods with default behavior

    End Class

End Namespace
