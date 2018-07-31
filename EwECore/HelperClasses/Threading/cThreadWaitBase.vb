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

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Base Class to provide thread blocking for the Threaded Manager Classes
''' </summary>
''' ---------------------------------------------------------------------------
Public MustInherit Class cThreadWaitBase
    Implements IThreadedProcess

#Region " Private vars "

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Signal state flag, used by an calling routine to block its thread until the 
    ''' model has completed. Invoked by <see cref="SetWait"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Private m_SignalState As System.Threading.ManualResetEvent

    Private m_bIsRunning As Boolean

    Private m_MessagePump As cCore.MessagePumpDelegate

#End Region ' Private vars

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Sub New()
        Me.m_SignalState = New System.Threading.ManualResetEvent(True)
    End Sub

    ''' ---------------------------------------------------------------------------
    ''' <inheritdocs cref="IThreadedProcess.StopRun"/>
    ''' ---------------------------------------------------------------------------
    Public MustOverride Function StopRun(Optional ByVal WaitTimeInMillSec As Integer = -1) As Boolean _
        Implements IThreadedProcess.StopRun

    ''' ---------------------------------------------------------------------------
    ''' <inheritdocs cref="IThreadedProcess.Wait"/>
    ''' ---------------------------------------------------------------------------
    Public Overridable Function Wait(Optional ByVal WaitTimeInMillSec As Integer = -1) As Boolean Implements IThreadedProcess.Wait
        'if WaitTimeInMillSec  = -1 wait until completed(WaitOne returns True) no matter how long
        'if WaitTimeInMillSec = 0 then wait for zero time even if WaitOne returns False, process has not completed
        'WaitTimeInMillSec > 0 (any positive integer) then wait for WaitTimeInMillSec or until WaitOne returns True

        Dim result As Boolean
        Dim waitTime As Integer
        Dim totTime As Integer
        Dim processing As Boolean = True
        Dim waitForever As Boolean
        Dim stpwWaitTime As Stopwatch
        Dim n As Integer

        'Thread is not running
        'The Waithandle is in a nonsignaled state (the door is open)
        If Not Me.IsRunning Then Return True

        'No Wait Time this just checks if the thread is blocked
        If WaitTimeInMillSec = 0 Then Return Me.m_SignalState.WaitOne(0)


        If WaitTimeInMillSec = -1 Then
            waitForever = True
        Else
            waitForever = False
            'use a separate timer to figure out if the wait has timed out
            stpwWaitTime = Stopwatch.StartNew
        End If


        'Wait is in a loop because
        'm_SignalState is signaled when a thread is running
        'm_SignalState.WaitOne will block the calling thread (the interface) while the signal is set
        'If the running thread calls out to the interface there will be a deadlock, it is block by WaitOne.
        'This loop pumps any interface messages so running threads don't block waiting for the interface to process messages it sent out
        Do
            n += 1

            'Let the interface process any messages that have been sent out by the running process
            'Without this the interace will deadlock 
            'if the thread makes a call to the interface before it has a chance to exit
            Me.RunInterfaceMessagePump()

            'WaitOne() will return True if the WaitHandle has been released be the thread
            result = Me.m_SignalState.WaitOne(0)
            totTime += waitTime

            If result = True Then processing = False
            If Not waitForever Then
                If stpwWaitTime.ElapsedMilliseconds >= WaitTimeInMillSec Then
                    'Timed out result will = False
                    processing = False
                End If
            End If

        Loop While processing

        If n > 1 Then
            System.Console.WriteLine("Waiting for running process " & n.ToString & " iterations.")
        End If

        'System.Console.WriteLine("Finished waiting " & totTime.ToString & " milliseconds, " & n.ToString & " iterations")
        Return result

    End Function


    Private Sub RunInterfaceMessagePump()
        Try
            If Me.m_MessagePump IsNot Nothing Then
                Me.m_MessagePump.Invoke()
            End If
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

    ''' ---------------------------------------------------------------------------
    ''' <inheritdocs cref="IThreadedProcess.SetWait"/>
    ''' ---------------------------------------------------------------------------
    Public Overridable Sub SetWait() _
        Implements IThreadedProcess.SetWait

        'set the isRunning flag
        m_bIsRunning = True
        'puts the ManualResetEvent into a non-signaled state
        'threads calling Wait() will block until ReleaseWait() is called
        Me.m_SignalState.Reset()

    End Sub

    ''' ---------------------------------------------------------------------------
    ''' <inheritdocs cref="IThreadedProcess.ReleaseWait"/>
    ''' ---------------------------------------------------------------------------
    Public Overridable Sub ReleaseWait() _
        Implements IThreadedProcess.ReleaseWait

        m_bIsRunning = False
        'puts the ManualResetEvent into a signaled state
        'Threads that called Wait() will be signaled to proceed
        Me.m_SignalState.Set()

    End Sub

    ''' ---------------------------------------------------------------------------
    ''' <inheritdocs cref="IThreadedProcess.IsRunning"/>
    ''' ---------------------------------------------------------------------------
    Public Overridable ReadOnly Property IsRunning() As Boolean _
        Implements IThreadedProcess.IsRunning
        Get
            Return m_bIsRunning
        End Get
    End Property

    Public WriteOnly Property MessagePump As cCore.MessagePumpDelegate Implements IThreadedProcess.MessagePump
        Set(value As cCore.MessagePumpDelegate)
            m_MessagePump = value
        End Set
    End Property
End Class