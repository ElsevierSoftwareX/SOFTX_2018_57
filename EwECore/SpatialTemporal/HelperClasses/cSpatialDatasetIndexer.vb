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
Imports System.Drawing
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace SpatialData

    ''' <summary>
    ''' Helper class that performs the task of indexing one spatial dataset at the time.
    ''' The indexer has a queue of datasets to index.
    ''' </summary>
    Friend Class cSpatialDatasetIndexer
        Inherits cThreadWaitBase

#Region " Private vars "

        ''' <summary>Synchronization lock</summary>
        Private m_sync As New Object()
        ''' <summary>The core to operate on.</summary>
        Private m_core As cCore = Nothing
        Private m_manSets As cSpatialDataSetManager = Nothing

        Private m_bEnabled As Boolean = True

        ''' <summary>Queue of datasets to index.</summary>
        Private m_queue As New List(Of ISpatialDataSet)

        ''' <summary>THe worker thread to perform the indexing.</summary>
        Private m_threadIndex As Threading.Thread = Nothing

#End Region ' Private vars

#Region " Public bits "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="core">The core to index against.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(core As cCore, man As cSpatialDataSetManager)
            Me.m_core = core
            Me.m_manSets = man
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Prioritize a dataset for indexing.
        ''' </summary>
        ''' <param name="ds">The <see cref="ISpatialDataSet"/> to index, or
        ''' nothing to stop indexing.</param>
        ''' -------------------------------------------------------------------
        Public Sub Prioritize(ds As ISpatialDataSet)

            If (ds IsNot Nothing) Then
                ' Do not change anything if data set is already indexed
                Dim comp As cDatasetCompatilibity = Me.m_manSets.Compatibility(ds)
                If (comp.NumIndexed = comp.NumOverlappingTimeSteps) Then Return

                SyncLock Me.m_sync
                    ' Move dataset to the head of the queue
                    Me.m_queue.Remove(ds)
                    Me.m_queue.Insert(0, ds)
                End SyncLock
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Stop indexing.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub [Stop]()
            Me.StopRun(500)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the <see cref="ISpatialDataSet"/> currently being indexed.
        ''' </summary>
        ''' <returns>The <see cref="ISpatialDataSet"/> currently being indexed.</returns>
        ''' -------------------------------------------------------------------
        Public Function Current() As ISpatialDataSet
            If (Me.m_queue.Count = 0) Then Return Nothing
            Return Me.m_queue(0)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether a dataset is being indexed.
        ''' </summary>
        ''' <param name="ds">The <see cref="ISpatialDataSet"/> to check, or 
        ''' nothing to check if any dataset is being indexed.</param>
        ''' <returns>True if a dataset is being indexed.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsIndexing(ds As ISpatialDataSet) As Boolean

            If (Me.m_queue.Count = 0) Then Return False

            If (ds Is Nothing) Then Return True
            Return ReferenceEquals(Me.m_queue(0), ds)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enable/disable indexing. This state is managed by the data set manager
        ''' who knows he bigger picture.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Friend Property Enabled As Boolean
            Get
                Return Me.m_bEnabled
            End Get
            Set(value As Boolean)

                Me.m_bEnabled = value

                If (value = True) Then
                    SyncLock Me.m_sync

                        ' Augment the indexer queue with content from the dataset manager
                        For Each ds As ISpatialDataSet In Me.m_manSets
                            If Not Me.m_queue.Contains(ds) Then
                                Dim comp As cDatasetCompatilibity = Me.m_manSets.Compatibility(ds)
                                ' Is not yet fully indexed?
                                If (comp.NumIndexed < comp.NumOverlappingTimeSteps) Then
                                    ' #Yes: add to queue
                                    ' Do not mess up priority dataset, but process applied datasets quickly
                                    Dim conn As cSpatialDataConnectionManager = Me.m_core.SpatialDataConnectionManager
                                    If conn.IsApplied(ds) And (Me.m_queue.Count > 0) Then
                                        Me.m_queue.Insert(1, ds)
                                    Else
                                        Me.m_queue.Add(ds)
                                    End If
                                End If
                            End If
                        Next

                        ' Clear queued data sets that are no longer there
                        For i As Integer = 0 To Me.m_queue.Count - 1
                            If Not m_manSets.Contains(Me.m_queue(i)) Then
                                Me.m_queue(i) = Nothing
                            End If
                        Next

                    End SyncLock

                    If (Me.m_threadIndex Is Nothing) And (Me.m_queue.Count > 0) Then
                        Me.m_threadIndex = New Threading.Thread(AddressOf IndexDatasetThread)
                        Me.m_threadIndex.IsBackground = True
                        Me.m_threadIndex.Start()
                    End If
                Else
                    Me.m_queue.Clear()
                    Me.StopRun(500)
                End If
            End Set
        End Property

        Public Overrides Function StopRun(Optional WaitTimeInMillSec As Integer = -1) As Boolean
            Dim result As Boolean = True
            Try
                Me.m_queue.Clear()
                result = Me.Wait(WaitTimeInMillSec)
                If (Me.m_threadIndex IsNot Nothing) Then
                    If Me.m_threadIndex.IsAlive Then
                        Try
                            ' Me.m_threadIndex.Abort()
                            Me.m_threadIndex = Nothing
                        Catch ex As Exception
                            ' You asked for it
                        End Try
                    End If
                End If
            Catch ex As Exception
                result = False
            End Try
            Return result
        End Function

#End Region ' Public bits

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Private indexing thread.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub IndexDatasetThread()

            While (Me.m_queue.Count > 0)

                Dim ds As ISpatialDataSet = Me.m_queue(0)
                Dim iTS As Integer = 1
                Dim nTS As Integer = Me.m_core.nEcospaceTimeSteps
                Dim dt As DateTime
                Dim ptfTL As New PointF(-180, 90)
                Dim ptfBR As New PointF(180, -90)
                Dim c As ISpatialDataCache = Nothing
                Dim strMessage As String = ""
                Dim bDone As Boolean = False

                If (ds IsNot Nothing) Then
                    If (ds.IsConfigured) Then

                        c = ds.Cache

                        Try
                            strMessage = cStringUtils.Localize(My.Resources.CoreMessages.STATUS_INDEXING_DATASET, ds.DisplayName)
                            Me.OnSpatialIndexUpdated(strMessage, eProgressState.Start, 0)

                            While Not bDone

                                dt = Me.m_core.EcospaceTimestepToAbsoluteTime(iTS)
                                If (ds.HasDataAtT(dt)) Then
                                    If (ds.IndexStatusAtT(dt) = ISpatialDataSet.eIndexStatus.NotIndexed) Then
                                        ' ToDo: Every dataset call should be subject to a timeout
                                        ds.UpdateIndexAtT(dt)
                                        Dim comp As cDatasetCompatilibity = Me.m_manSets.Compatibility(ds)
                                        comp.Refresh()
                                        Me.OnSpatialIndexUpdated(strMessage, eProgressState.Running, CSng(comp.NumIndexed / comp.NumOverlappingTimeSteps))
                                    End If
                                End If

                                ' Next
                                iTS += 1

                                ' Queue may have been cleared, current data set may have been removed... be ready to stop
                                bDone = (iTS > Me.m_core.nEcospaceTimeSteps) Or (Not Me.m_queue.Contains(ds))
                            End While

                            ' Done (send just in case)
                            Me.OnSpatialIndexUpdated("", eProgressState.Finished, 1.0!)

                        Catch ex As Threading.ThreadAbortException
                            ' NOP
                        Catch ex As Exception
                            cLog.Write(ex, "cSpatialDatasetIndexer::IndexDatasetThread(" & ds.DisplayName & ")")
                            'Console.WriteLine(ex.Message)
                        Finally
                            ' Cleanup: restore cache
                            ds.Cache = c
                            ' Remove dataset from the queue
                            Me.m_queue.Remove(ds)
                        End Try
                    End If
                Else
                    Me.m_queue.RemoveAt(0)
                End If
            End While

            ' Done threading
            Me.m_threadIndex = Nothing

        End Sub

        Private Sub OnSpatialIndexUpdated(ByVal strMessage As String, _
                                          ByVal state As eProgressState, _
                                          ByVal sProgress As Single)

            If (Me.m_core IsNot Nothing) Then
                Try
                    Dim msg As New cProgressMessage(state, 1, sProgress, strMessage, eMessageType.Progress, eDataTypes.EcospaceSpatialDataConnection)
                    msg.Source = eCoreComponentType.EcoSpace
                    Me.m_core.Messages.SendMessage(msg)

                Catch ex As Exception
                    ' Hmm
                    Debug.Assert(False, ex.Message)
                End Try
            End If
        End Sub

#End Region ' Internals

    End Class

End Namespace
