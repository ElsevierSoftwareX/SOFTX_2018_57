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
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization
Imports System.Reflection
Imports System.Collections.Generic

#End Region ' Imports

' Levels of feedback:
'   0: no console feedback
'   1: Main info, main failures
'   2: Status updates
'   3: Data details (for the hardcore debuggers)
#Const VERBOSE_LEVEL = 1

Namespace NetUtilities

    ''' =======================================================================
    ''' <summary>
    ''' This class wraps the communication of data back and forth across a
    ''' .NET Socket.
    ''' </summary>
    ''' <remarks>
    ''' <para>Every new chunk of data sent through the wrapped socket are 
    ''' preceded by a 4-byte integer indicating the size of the data. Since
    ''' data sent through a socket is split into packages, the receiving
    ''' socket will keep combining packages until the original data is
    ''' reassembled, which is then made available.</para>
    ''' </remarks>
    ''' =======================================================================
    Public Class cSocketWrapper

#Region " Private helper classes "

        ''' ===================================================================
        ''' <summary>
        ''' Class used to help server and client recognize each other when
        ''' setting up a connection.
        ''' </summary>
        ''' ===================================================================
        <Serializable()> _
        Private Class cHandShake
            Inherits cSerializableObject

            Private m_iHandshake As Int32 = 0
            Private m_bRelayed As Boolean = False
            Private m_strClientMachineName As String = ""

            Public Sub New(ByVal iHandshake As Int32)
                MyBase.New()
                Me.m_iHandshake = iHandshake
            End Sub

            Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
                MyBase.New(info, context)
                Try
                    Me.m_iHandshake = info.GetInt32("m_iHandshake")
                    Me.m_bRelayed = info.GetBoolean("m_bRelayed")
                    Me.m_strClientMachineName = info.GetString("m_strClientMachineName")
                Catch ex As Exception
                    Debug.Assert(False, String.Format("Exception '{0}' while deserializing cHandShake", ex.Message))
                End Try
            End Sub

            Protected Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)
                MyBase.GetObjectData(info, context)
                info.AddValue("m_iHandshake", Me.m_iHandshake, GetType(Int32))
                info.AddValue("m_bRelayed", Me.m_bRelayed, GetType(Boolean))
                info.AddValue("m_strClientMachineName", Me.m_strClientMachineName, GetType(String))
            End Sub

            Public ReadOnly Property Relayed() As Boolean
                Get
                    Return Me.m_bRelayed
                End Get
            End Property

            Public Function Relay(ByVal strClientMachineName As String) As Boolean
                If Me.m_bRelayed Then Return False
                Me.m_strClientMachineName = strClientMachineName
                Me.m_bRelayed = True
                Return True
            End Function

            Public Function HandshakeID() As Int32
                Return Me.m_iHandshake
            End Function

            Public Overrides ReadOnly Property ID() As String
                Get
                    Return String.Format("cHandshake_{0}", Me.m_iHandshake)
                End Get
            End Property

            Public ReadOnly Property ClientMachineName() As String
                Get
                    Return Me.m_strClientMachineName
                End Get
            End Property
        End Class

#End Region ' Private helper classes

#Region " Privates "

        ''' <summary>
        ''' Socket wrapper read states
        ''' </summary>
        Private Enum eReadStates
            ''' <summary>Not reading.</summary>
            NotReading
            ''' <summary>Reading data size bytes.</summary>
            ReadingSize
            ''' <summary>Reading data bytes.</summary>
            ReadingObject
            ''' <summary>Data has been read.</summary>
            ObjectRead
        End Enum

        ''' <summary>Size of buffer for receiving data.</summary>
        Private Const cBUFFER_SIZE As Integer = 1024
        ''' <summary>The one buffer for receiving data.</summary>
        Private m_abBuffer(cSocketWrapper.cBUFFER_SIZE) As Byte
        ''' <summary>The wrapped socket.</summary>
        Private m_socket As Socket = Nothing
        ''' <summary>Size, in number of bytes, of the most recent data package.</summary>
        Private m_iDataSize As Integer = 0
        ''' <summary>Number of bytes read of the most recent data package.</summary>
        Private m_iDataRead As Integer = 0
        ''' <summary>States whether or not the socket connection is authorized for communication.</summary>
        Private m_bAuthorized As Boolean = False

        ''' <summary>Received bytes buffer.</summary>
        Private m_abReceived() As Byte
        ''' <summary>Buffer for object size </summary>
        Private m_abSizeBuff(4) As Byte

        ''' <summary> Number of bytes in the size buffer m_abSizeBuff </summary>
        Private m_iSizeRead As Integer
        Private m_iQueue As Integer = 0

        ''' <summary>Client ID.</summary>
        Private m_iID As Int32 = 0
        ''' <summary>Name of the remote machine.</summary>
        Private m_strRemoteMachineName As String = ""

        '''' <summary>Semaphore to lock sending of data </summary>
        '   Private m_SendLock As New System.Threading.Semaphore(1, 1)
        Private m_readState As eReadStates = eReadStates.ReadingSize

        Private m_readlock As New System.Threading.Semaphore(1, 1)
        Private m_iNumPendingSend As Integer = 0
        Private m_syncObject As SynchronizationContext = Nothing

#End Region ' Privates

#Region " Constructor "

        Private Shared Function CreateSocket() As Socket

            Dim s As Socket = Nothing

            ' Configure socket for use with IPv6
            If Socket.OSSupportsIPv6 Then
                s = New Socket(AddressFamily.InterNetwork, _
                               SocketType.Stream, _
                               ProtocolType.Tcp)

                ' After http://forum.soft32.com/windows/Socket-problem-migrating-Vista-ftopict363802.html
                s.SetSocketOption(SocketOptionLevel.IPv6, _
                                  DirectCast(27, SocketOptionName), _
                                  0)
            Else
                s = New Socket(AddressFamily.InterNetwork, _
                               SocketType.Stream, _
                               ProtocolType.Tcp)
            End If
            Return s

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new socket wrapper.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New()
            Me.New(0, cSocketWrapper.CreateSocket())
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Wrap an existing socket by a new socket wrapper.
        ''' </summary>
        ''' <param name="s">The <see cref="Socket">socket</see> to wrap.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal iID As Integer, ByVal s As Socket)

            ' Sanity checks (googoogoojoob)
            Debug.Assert(s IsNot Nothing, "Need valid socket")

            Me.m_iID = iID
            Me.m_socket = s

            ' Get sync object
            Me.m_syncObject = SynchronizationContext.Current
            If (Me.m_syncObject Is Nothing) Then
                Me.m_syncObject = New SynchronizationContext()
            End If

            If Me.Connected Then
                Me.StartListening()
            End If

        End Sub

#End Region ' Constructor

#Region " Events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Status flags passed in the <see cref="OnStatus">OnStatus</see> event.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eStatusTypes As Byte
            ''' <summary>The socket is disconnected.</summary>
            Disconnected = 0
            ''' <summary>The socket is connected.</summary>
            Connected
            ''' <summary>The socket is authorized.</summary>
            Authorized
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public event reporting status changes in the socket.
        ''' </summary>
        ''' <param name="sw">The <see cref="cSocketWrapper">socket wrapper</see>
        ''' instance that sent the event.</param>
        ''' <param name="status">The <see cref="eStatusTypes">status</see>
        ''' of the socket wrapper that sent the event.</param>
        ''' <param name="strData">The data that accompanies the event.</param>
        ''' -------------------------------------------------------------------
        Public Event OnStatus(ByVal sw As cSocketWrapper, ByVal status As eStatusTypes, ByVal strData As String)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public event reporting that data has been received across the socket.
        ''' </summary>
        ''' <param name="sw">The <see cref="cSocketWrapper">socket wrapper</see>
        ''' instance that received the data.</param>
        ''' <param name="data">The <see cref="cSerializableObject">data</see>
        ''' that was received.</param>
        ''' -------------------------------------------------------------------
        Public Event OnData(ByVal sw As cSocketWrapper, ByVal data As cSerializableObject)

        Private Class cEventInfo

            Public Enum eEventType As Integer
                Status
                Data
            End Enum

            Public Sub New(ByVal eventType As eEventType, ByVal data As Object)
                Me.EventType = eventType
                Me.Data = data
            End Sub

            Public EventType As eEventType = eEventType.Data
            Public Data As Object = Nothing

        End Class

        Private Sub SendEvent(ByVal obj As Object)

            Dim info As cEventInfo = DirectCast(obj, cEventInfo)

            Select Case info.EventType

                Case cEventInfo.eEventType.Data
                    RaiseEvent OnData(Me, DirectCast(info.Data, cSerializableObject))

                Case cEventInfo.eEventType.Status
                    RaiseEvent OnStatus(Me, DirectCast(info.Data, eStatusTypes), "")

            End Select

        End Sub

#End Region ' Events 

#Region " Public access "

#Region " Connection "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Attempt to connect the socket to a URI, port combination.
        ''' </summary>
        ''' <param name="strURI">The URI to connect to.</param>
        ''' <param name="iPort">The IP port to connect to.</param>
        ''' <returns>True if connected succesfully.</returns>
        ''' -------------------------------------------------------------------
        Public Function Connect(ByVal strURI As String, ByVal iPort As Integer) As Boolean
            Dim aIP As IPAddress() = Nothing
            Try
                Dim ipEntry As IPHostEntry = Dns.GetHostEntry(strURI)
                aIP = ipEntry.AddressList
            Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                Console.Write("sw {0}: exception '{1}' occurred while attempting to connect to {2}:{3}", _
                              Me.ToString(), ex.Message, strURI, iPort)
#End If
            End Try

            If (aIP Is Nothing) Then Return False
            If (aIP.Length = 0) Then Return False

            ' Attempt to any host entry
            For Each ip As IPAddress In aIP
                If Me.Connect(ip, iPort) Then Return True
            Next
            Return False

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Attempt to connect the socket to an IP address, port combination.
        ''' </summary>
        ''' <param name="ip">The IP address to connect to.</param>
        ''' <param name="iPort">The IP port to connect to.</param>
        ''' <returns>True if connected succesfully.</returns>
        ''' -------------------------------------------------------------------
        Public Function Connect(ByVal ip As IPAddress, ByVal iPort As Integer) As Boolean

            Try
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine("sw {0} attempting to connect to {1}:{2}", Me.ToString(), ip.ToString(), iPort)
#End If
                ' Try to connect
                Me.m_socket.Connect(ip, iPort)

            Catch ex As SocketException
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("sw {0} exception '{1}' while attempting to connect to {2}:{3}", Me.ToString(), ex.Message, ip.ToString(), iPort)
#End If
            End Try

            ' No luck?
            If Not Me.Connected Then
                ' #Ouch! Raise event
                Console.WriteLine("sw {0} failed to connect to {1}:{2}", Me.ToString(), ip.ToString(), iPort)
                Me.m_syncObject.Send(New SendOrPostCallback(AddressOf SendEvent), _
                                     New cEventInfo(cEventInfo.eEventType.Status, eStatusTypes.Disconnected))
                Return False
            End If

            ' Connected: raise event
#If VERBOSE_LEVEL >= 1 Then
            Console.WriteLine("sw {0} connected", Me.ToString())
#End If
            Me.m_syncObject.Send(New SendOrPostCallback(AddressOf SendEvent), _
                                 New cEventInfo(cEventInfo.eEventType.Status, eStatusTypes.Connected))
            Me.StartListening()

            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the socket is connected.
        ''' </summary>
        ''' <returns>True if connected.</returns>
        ''' -------------------------------------------------------------------
        Public Function Connected() As Boolean
            Return Me.m_socket.Connected
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Disconnects the socket from a server.
        ''' </summary>
        ''' <returns>True if disconnected succesfully.</returns>
        ''' -------------------------------------------------------------------
        Public Function Disconnect() As Boolean
            Try
                If Me.m_socket.Connected() Then
                    Me.m_socket.Disconnect(True)
                End If
            Catch ex As Exception
                ' Whoopy
            End Try
            ' Create new socket
            Me.m_socket = Nothing
            Me.m_socket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            Return Not Me.m_socket.Connected()
        End Function

#End Region ' Connection

#Region " Authorization "

        Public ReadOnly Property ID() As Int32
            Get
                Return Me.m_iID
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the machine name of a connected, authorized client.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property RemoteMachineName() As String
            Get
                Return Me.m_strRemoteMachineName
            End Get
            Protected Set(ByVal strRemoteMachineName As String)
                Me.m_strRemoteMachineName = strRemoteMachineName
            End Set
        End Property

        Public Sub Authorize()

            Me.m_iHandshake = Me.m_iID
            Me.Authorized = False

            If (Me.m_iHandshake <> 0) Then
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine("sw {0} awaiting authorization", Me.ToString())
#End If
                ' Send handshake
                Me.SendHandshake()
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the socket is authorized to send data for its 
        ''' intended purpose.
        ''' </summary>
        ''' <returns>True if authorized.</returns>
        ''' -------------------------------------------------------------------
        Public Property Authorized() As Boolean
            Get
                Return Me.m_bAuthorized
            End Get
            Set(ByVal value As Boolean)
                Me.m_bAuthorized = value
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine("sw {0} authorized {1}", Me.ToString(), CStr(value))
#End If
                If value = True Then
                    ' No longer wait for authorization
                    Me.m_iHandshake = 0
                End If
            End Set
        End Property

        Public Function WaitingForAutorization() As Boolean
            Return (Me.m_iHandshake <> 0)
        End Function

#End Region ' Authorization

#Region " Send "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Send <see cref="cSerializableObject">a serializable object</see> across the socket.
        ''' </summary>
        ''' <param name="obj">The object to send.</param>
        ''' <param name="bRequiresAuthorization">Flag indicating whether the socket
        ''' needs to be <see cref="Authorized">Authorized</see> to send this data.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Public Function Send(ByVal obj As cSerializableObject, _
            Optional ByVal bRequiresAuthorization As Boolean = True, _
            Optional ByVal bSendImmediately As Boolean = False) As Boolean

            ' Sanity check(s)
            If (obj Is Nothing) Then Return False

            ' Debug.Assert(m_pendingSend = 0, "sw send not completed!")

            'Me.m_SendLock.WaitOne()
            'Debug.Assert(m_pendingSend = 0, "sw send not completed!")
            Dim bf As New BinaryFormatter()
            Dim ms As New MemoryStream()
            Dim bSucces As Boolean = False

            'Block all other calls to Send until this Send has finished (SendCallback)

            m_iNumPendingSend += 1
            bf.Serialize(ms, obj)
            ' Reset position in stream
            ms.Seek(0, 0)

            Try
#If VERBOSE_LEVEL >= 3 Then
                Console.WriteLine("sw {0} sending {1}", Me.ToString(), obj.ToString())
#End If
                bSucces = SendBinary(ms.GetBuffer(), bRequiresAuthorization, bSendImmediately)
            Catch ex As Exception

                bSucces = False
            End Try
            m_iNumPendingSend -= 1
            ' Cleanup
            ms.Close()
            ms = Nothing
            bf = Nothing
            '  Me.m_SendLock.Release()

            Return bSucces
        End Function

#End Region ' Send

#End Region ' Public access

#Region " Internals "

#Region " Authorization "

        Private m_iHandshake As Int32 = 0

        Private Function IsHandshake(ByVal iHandshake As Int32) As Boolean
            ' External handshake?
            If (Me.m_iID = 0) Then Return True
            Return (Me.m_iHandshake = iHandshake)
        End Function

        Private Function HandshakeID() As Int64
            Return Me.m_iHandshake
        End Function

        Private Sub SendHandshake()

            Dim hs As New cHandShake(Me.m_iHandshake)

            If Me.Send(hs, False) Then
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine("sw {0} sent handshake {1}", Me.ToString(), hs.HandshakeID)
#End If
            Else
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine("sw {0} failed to send handshake {1}", Me.ToString(), hs.HandshakeID)
#End If
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Send handshake message back to the sender
        ''' </summary>
        ''' <param name="hs"></param>
        ''' <remarks>Messages can be relayed only once.</remarks>
        ''' -------------------------------------------------------------------
        Private Sub RelayHandshake(ByVal hs As cHandShake)

            Try
                If hs.Relayed = False Then
                    hs.Relay(Environment.MachineName)

                    If Me.Send(hs, False, True) Then
#If VERBOSE_LEVEL >= 3 Then
                        Console.WriteLine("sw {0} handshake {1} relayed", Me.ToString(), hs.HandshakeID)
#End If
                    Else
#If VERBOSE_LEVEL >= 3 Then
                        Console.WriteLine("sw {0} handshake {1} failed to relay", Me.ToString(), hs.HandshakeID)
#End If
                    End If
                Else
#If VERBOSE_LEVEL >= 3 Then
                    Console.WriteLine("sw {0} handshake {1} completed", Me.ToString(), hs.HandshakeID)
#End If
                End If
            Catch ex As Exception
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine("sw {0} exception {1} while relaying handshake {2}", Me.ToString(), ex.Message, hs.ID)
#End If
            End Try

        End Sub

#End Region ' Authorization

#Region " READ "

        Private Sub StartListening()
            ' Start waiting for data
            Me.m_socket.BeginReceive(Me.m_abBuffer, 0, cSocketWrapper.cBUFFER_SIZE, SocketFlags.None, AddressOf ReceiveCallBack, Me)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Read the most recently received chunk of data from the local socket
        ''' <see cref="m_abBuffer">buffer</see>.
        ''' </summary>
        ''' <param name="iNumBytes">The number of bytes that were received.</param>
        ''' -------------------------------------------------------------------
        Private Function ReadBuffer(ByVal iNumBytes As Integer) As List(Of cSerializableObject)

            Dim objData As cSerializableObject = Nothing
            Dim bf As BinaryFormatter = Nothing
            Dim ms As MemoryStream = Nothing
            Dim iOffset As Integer = 0
            Dim iChunkSize As Integer = 0
            Dim nSizeCopy As Integer
            Dim lstObs As New List(Of cSerializableObject)

            Try

#If VERBOSE_LEVEL >= 3 Then
                Console.WriteLine("sw {0} read buffer {1} bytes ", Me.ToString(), iNumBytes)
#End If

                ' Has data?
                While (iOffset < iNumBytes)
                    ' Is this a new message?
                    If Me.m_readState = eReadStates.ReadingSize Then
                        ' #Yes: Start new message
                        ' Extract new message length

                        'how many byte should we read from the buffer
                        nSizeCopy = Math.Min(4 - Me.m_iSizeRead, iNumBytes - iOffset)
                        Array.Copy(Me.m_abBuffer, iOffset, Me.m_abSizeBuff, m_iSizeRead, nSizeCopy)

                        m_iSizeRead += nSizeCopy
                        iOffset += nSizeCopy

#If VERBOSE_LEVEL >= 3 Then
                        Console.WriteLine("sw {0} read size buffer {1} bytes total of {2}", Me.ToString(), nSizeCopy, m_iSizeRead)
#End If

                        'is the size buffer full
                        If Me.m_iSizeRead = 4 Then
                            'yes we have all the bytes for the size buffer

                            'how big is this object
                            Me.m_iDataSize = CInt(Me.m_abSizeBuff(0)) + _
                                             CInt(Me.m_abSizeBuff(1) * 2 ^ 8) + _
                                             CInt(Me.m_abSizeBuff(2) * 2 ^ 16) + _
                                             CInt(Me.m_abSizeBuff(3) * 2 ^ 24)

                            ' Allocate size
                            ReDim Me.m_abReceived(Me.m_iDataSize)

                            'clear out the data size counters
                            Me.m_iSizeRead = 0
                            nSizeCopy = 0
                            'change the read state
                            Me.m_readState = eReadStates.ReadingObject

                        End If ' If Me.m_nSizeRead = 4 Then
                    End If ' If Me.m_readState = eReadStates.ReadingSize Then

                    If Me.m_readState = eReadStates.ReadingObject Then
                        ' Determine number of bytes to read from this block

                        iChunkSize = Math.Min(Me.m_iDataSize - Me.m_iDataRead, iNumBytes - iOffset)
                        ' Copy bytes
                        Array.Copy(Me.m_abBuffer, iOffset, Me.m_abReceived, Me.m_iDataRead, iChunkSize)

                        'the number of byte read for this object
                        Me.m_iDataRead += iChunkSize
                        ' Update offset
                        iOffset += iChunkSize

#If VERBOSE_LEVEL >= 3 Then
                        Console.WriteLine("sw {0} read {1} bytes (buffer {2}, read {3} of {4})", Me.ToString(), iChunkSize, iNumBytes, Me.m_iDataRead, Me.m_iDataSize)
#End If

                        ' Have we read all the bytes for this object?
                        If (Me.m_iDataSize = Me.m_iDataRead) Then
                            ' #Yes: change the readstate
                            Me.m_readState = eReadStates.ObjectRead
                        End If

                    End If 'If Me.m_readState = eReadStates.ReadingObject Then

                    ' Is entire message read?
                    If Me.m_readState = eReadStates.ObjectRead Then
                        ' #Yes: extract transferred binary data
                        bf = New BinaryFormatter()
                        ms = New MemoryStream(Me.m_abReceived)

#If VERBOSE_LEVEL >= 2 Then
                        Console.WriteLine("sw {0} data read, deserializing object", Me.ToString())
#End If

                        Try
                            ' Reconstruct data
                            lstObs.Add(CType(bf.Deserialize(ms), cSerializableObject))
                        Catch ex As Exception
                            Debug.Assert(False, ex.Message)
                        End Try

                        ms.Close()
                        ms = Nothing
                        bf = Nothing

                        ' Reset read buffer
                        Me.m_iDataSize = 0
                        Me.m_iDataRead = 0
                        Me.m_abReceived = Nothing
                        ' Start reading the next bytes in the buffer, which are the size bytes
                        Me.m_readState = eReadStates.ReadingSize
                    End If

                End While

            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Throw New Exception("sw ReadBuffer() Error: " & ex.Message, ex)
            End Try

            Return lstObs

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Callback for asynchronous socket data reading.
        ''' </summary>
        ''' <param name="ar"></param>
        ''' -----------------------------------------------------------------------
        Private Sub ReceiveCallBack(ByVal ar As IAsyncResult)

            ' Retrieve SocketData
            Dim sw As cSocketWrapper = CType(ar.AsyncState, cSocketWrapper)
            Dim iNumBytes As Integer = -1
            Dim bSendAuthorization As Boolean = False

            'list of objects assembled from the buffer in ReadBuffer()
            Dim ObjectsInStream As List(Of cSerializableObject)

            ' Read incoming bytes
            Try
                ' Get block from socket
                iNumBytes = sw.m_socket.EndReceive(ar)

            Catch ex As SocketException
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("sw {0} socket exception '{1}'", Me.ToString(), ex.Message)
#End If
                ' Screw this, I'm out of here
                iNumBytes = 0
            Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("sw {0} exception '{1}'", Me.ToString(), ex.Message)
#End If
            End Try

            ' Disconnected message?
            If iNumBytes = 0 Then
                ' #Yes: disconnect the socket
                sw.Disconnect()

#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("sw {0} disconnected", Me.ToString())
#End If
                Try
                    RaiseEvent OnStatus(Me, eStatusTypes.Disconnected, "")
                Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                    Console.WriteLine(">> sw {0} OnStatus(Disconnected) exception '{1}'", Me.ToString(), ex.Message)
#End If
                End Try
                Return
            End If

            ' get a list of cSerializableObject objects from the buffer
            ObjectsInStream = sw.ReadBuffer(iNumBytes)

            ' 
            For Each objRead As cSerializableObject In ObjectsInStream

                ' Is handshake?
                If (TypeOf objRead Is cHandShake) Then

                    'process the handshake
                    Dim hs As cHandShake = DirectCast(objRead, cHandShake)
                    Me.ReceiveHandShake(sw, hs)
                    ' Immediately forget object because it should not be sent out in the OnData event
                    objRead = Nothing

                Else ' If (TypeOf objRead Is cHandShake) Then

                    ' #No: not a handshake but a valid object

                    ' Is this connection Authorized?
                    If sw.Authorized Then
                        ' #Yes: connection is Authorized, broadcast new data
                        Try
                            ' Send event
                            Me.m_syncObject.Send(New SendOrPostCallback(AddressOf SendEvent), _
                                                 New cEventInfo(cEventInfo.eEventType.Data, objRead))

                        Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                            Console.WriteLine(">> sw {0} OnData() exception '{1}' ", Me.ToString(), ex.Message)
#End If
                        End Try

#If VERBOSE_LEVEL >= 2 Then
                        Console.WriteLine("sw {0} received data '{1}'", Me.ToString(), objRead.ToString())
#End If
                    Else
                        'NOT Authorized
#If VERBOSE_LEVEL >= 1 Then
                        Console.WriteLine("sw {0} not authorized to receive data", Me.ToString())
#End If
                    End If ' If sw.Authorized Then
                End If

            Next objRead

            ' After all data is handled prepare for receiving next chunk
            If iNumBytes > 1 Then
                Try
                    sw.m_socket.BeginReceive(sw.m_abBuffer, 0, cSocketWrapper.cBUFFER_SIZE, SocketFlags.None, AddressOf ReceiveCallBack, sw)
                Catch ex As Exception
                    ' TODO: Likely need better feedback, maybe send an event.
                    Debug.Assert(False, String.Format("Failed to recieve object.  May be attributed to client or server disconnecting during transfer. {0}", ex.ToString()))
                End Try
            End If

        End Sub

        ''' <summary>
        ''' Process a Handshake object received by the socket callback ReceiveCallBack(IAsyncResult)
        ''' </summary>
        ''' <param name="sw">SocketWrapper object the handshake was recieved on</param>
        ''' <param name="handshake">cHandshake to process</param>
        ''' <remarks></remarks>
        Private Sub ReceiveHandShake(ByVal sw As cSocketWrapper, ByVal handshake As cHandShake)
            Dim bSendAuthorization As Boolean = False

#If VERBOSE_LEVEL >= 2 Then
            Console.WriteLine("sw {0} handling handshake {1}", Me.ToString(), handshake.ToString())
#End If
            ' Is the socket wrapper not authorized?
            If (sw.Authorized = False) Then
                ' #Yes: is the incoming handshake designated for the socket wrapper?
                If sw.IsHandshake(handshake.HandshakeID) Then
                    ' #Yes: Authorized
                    sw.Authorized = True
                    ' Remember client ID
                    sw.m_iID = handshake.HandshakeID

                    If (handshake.Relayed = True) Then
                        sw.RemoteMachineName = handshake.ClientMachineName
                    End If
                    bSendAuthorization = True
#If VERBOSE_LEVEL >= 1 Then
                    Console.WriteLine("sw {0} authorized for client machine {1}", Me.ToString(), handshake.ClientMachineName)
#End If
                Else
#If VERBOSE_LEVEL >= 1 Then
                    Console.WriteLine(">> sw {0} received handshake {1}, expecting {2}", Me.ToString(), handshake.HandshakeID, sw.m_iHandshake)
#End If
                End If
            Else
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine(">> sw {0} already authorized!", Me.ToString())
#End If
            End If
            ' First relay handshake
            sw.RelayHandshake(handshake)
            ' Then raise authorization event
            If bSendAuthorization Then
                Try
                    RaiseEvent OnStatus(Me, eStatusTypes.Authorized, "")
                Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                    Console.WriteLine(">> sw {0} OnStatus(Authorized) exception '{1}' ", Me.ToString(), ex.Message)
#End If
                End Try
            End If


        End Sub

#End Region ' READ

#Region " SEND "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Send a message through the socket. This call is blocking (for now)
        ''' </summary>
        ''' <param name="byMessage">The message bytes to send</param>
        ''' <param name="bRequiresAuthorization">Flag indicating whether the socket
        ''' needs to be <see cref="Authorized">Authorized</see> to send this data.</param>
        ''' <remarks>
        ''' This method will prepend the message with 4 bytes stating the
        ''' length of the original message. This will allow the receiving
        ''' end to deduct whether all packets for incoming data have arrived.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Function SendBinary(ByVal byMessage As Byte(), _
            Optional ByVal bRequiresAuthorization As Boolean = True, _
            Optional ByVal bSendImmedately As Boolean = False) As Boolean

            ' Sanity checks
            If (Not Me.Connected) Then Return False
            If (bRequiresAuthorization And Not Me.Authorized) Then Return False
            If (byMessage Is Nothing) Then Return False

            Dim byData() As Byte
            Dim iLength As Integer = 0
            iLength = byMessage.Length()

            ReDim byData(4 + iLength - 1)
            byData(0) = CByte(iLength And &HFF)
            byData(1) = CByte((iLength >> 8) And &HFF)
            byData(2) = CByte((iLength >> 16) And &HFF)
            byData(3) = CByte((iLength >> 24) And &HFF)

            Array.Copy(byMessage, 0, byData, 4, iLength)

            Try
#If VERBOSE_LEVEL >= 3 Then
                Me.m_iQueue += (iLength + 4)
                Console.WriteLine("sw BeginSend() {0} sending {1} bytes (queue size {2})", Me.ToString(), (iLength + 4), Me.m_iQueue)
#End If

                'If bSendImmedately Then
                '    Me.m_socket.Send(byData, 0, byData.Length, SocketFlags.None)
                '    'Me.m_readlock.Release()
                'Else
                Me.m_socket.BeginSend(byData, 0, byData.Length, SocketFlags.None, AddressOf Me.SendCallback, Me.m_socket)
                'End If

            Catch ex As SocketException
                ' Socket has been closed, just fail this attempt and hope the failure is handled well elsewhere
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine("sw BeginSend() {0} failed: {1}", Me.ToString(), ex.Message)
#End If
                Return False
            Catch ex As Exception
                ' TODO: Likely need better feedback, maybe send an event.
                Debug.Assert(False, "Cannot send message.  Failed at sending Binary" + ex.ToString())
                Return False
            End Try
            Return True

        End Function

        Private Sub SendCallback(ByVal ar As IAsyncResult)
            ' Dim sw As cSocketWrapper = DirectCast(ar, Socket)
            '
            Dim s As Socket = CType(ar.AsyncState, Socket)
            Dim nb As Integer

            Try
                ' Sanity checks
                If s Is Nothing Then Return
                If Not s.Connected Then Return
                nb = s.EndSend(ar)
                m_iNumPendingSend -= 1

#If VERBOSE_LEVEL >= 3 Then
                Me.m_iQueue -= (nb)
                Console.WriteLine("sw SendCallback() {0} received {1} bytes (queue size {2})", Me.ToString(), nb, Me.m_iQueue)
#End If
            Catch ex As Exception
                ' Woops!
                Debug.Assert(False, ex.Message)
            Finally
                'release the semaphore

            End Try

            '   Me.m_SendLock.Release()

        End Sub

#End Region ' SEND

#End Region ' Internals

#Region " Overrides "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a string representation of a cSocketWrapper instance.
        ''' </summary>
        ''' <returns>
        ''' A string representing the Remote End Point that the wrapped socket 
        ''' is connected to.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function ToString() As String
            If Me.m_socket.RemoteEndPoint Is Nothing Then Return "(not connected)"
            Return Me.m_socket.RemoteEndPoint.ToString
        End Function

#End Region ' Overrides 

    End Class

End Namespace ' NetUtilities
