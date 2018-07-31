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

Imports System.IO
Imports EwEUtils.Core
Imports EwEUtils.Utilities

'ToDo: complete intellisense XML code comments

Namespace EcospaceTimeSeries

    Public Class cEcospaceTimeSeriesXYZReader

        Private m_Manager As cEcospaceTimeSeriesManager
        Private m_StartDate As Date
        Private m_EndDate As Date

        Private m_MaxRow As Integer
        Private m_MaxCol As Integer

        Private m_dctFailedRecs As Dictionary(Of eTimeSeriesRecValidations, Integer)

        Public Sub New(TimeSeriesFile As String, TSManager As cEcospaceTimeSeriesManager)
            Me.FileName = TimeSeriesFile
            Me.m_Manager = TSManager
            Me.TimeStampFormatString = Me.m_Manager.TimeStepFormatString
        End Sub

        Public Function Read(VarName As eVarNameFlags) As Boolean

            Me.Init()

            Try

                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                'Throw New Exception("Opps Testing exceptions: " + Me.FileName)
                'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                Dim strm As New StreamReader(Me.FileName)
                Dim header As String
                Dim RecBuffer As String
                Dim rec As cEcospaceTimeSeriesRec

                header = strm.ReadLine()
                'do something with the header???
                'figure out the data format???

                Do While Not strm.EndOfStream
                    RecBuffer = strm.ReadLine()
                    rec = cEcospaceTimeSeriesRec.FromString(RecBuffer, Me.TimeStampFormatString, VarName)
                    If rec.ReadValidation = eTimeSeriesRecValidations.isReadValid Then
                        Me.getMinMaxDates(rec)
                        Me.getExtent(rec)
                        Me.m_Manager.Add(rec)
                    Else
                        Me.AddFailedRec(rec)
                    End If
                Loop

                strm.Close()

            Catch ex As Exception

                EwEUtils.Core.cLog.Write(ex, Me.ToString + ".Read() failed to read time series file '" + Me.FileName + "'")
                Throw New Exception(ex.Message)

            End Try

            Me.m_Manager.Debug_DumpDataTableRows()

            Me.dumpFailedRecs()

            Return True
        End Function

        Private Sub Init()
            m_StartDate = New Date(6666, 6, 6)
            m_EndDate = New Date(1, 1, 1)

            Me.m_MaxRow = 0
            Me.m_MaxCol = 0

            Me.m_dctFailedRecs = New Dictionary(Of eTimeSeriesRecValidations, Integer)
        End Sub

        Private Sub getMinMaxDates(rec As cEcospaceTimeSeriesRec)

            If Date.Compare(m_StartDate, rec.TimeStamp) > 0 Then
                m_StartDate = rec.TimeStamp
            End If


            If Date.Compare(m_EndDate, rec.TimeStamp) < 0 Then
                m_EndDate = rec.TimeStamp
            End If

        End Sub

        Private Sub AddFailedRec(rec As cEcospaceTimeSeriesRec)
            If Not Me.m_dctFailedRecs.ContainsKey(rec.ReadValidation) Then
                Me.m_dctFailedRecs.Add(rec.ReadValidation, 0)
            End If
            Me.m_dctFailedRecs.Item(rec.ReadValidation) += 1
        End Sub

        Private Sub dumpFailedRecs()
            If Me.m_dctFailedRecs.Count = 0 Then
                Return
            End If

            Dim msg As New Text.StringBuilder

            msg.Append(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_READ_FAIL_MESSAGE)
            For Each pair As KeyValuePair(Of eTimeSeriesRecValidations, Integer) In Me.m_dctFailedRecs
                System.Console.WriteLine(pair.Key.ToString + " " + pair.Value.ToString)
                msg.Append(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_TIMESERIES_READ_FAIL_REASON, pair.Value, pair.Key))
            Next
            If msg.Length > 0 Then
                Me.m_Manager.Core.Messages.AddMessage(New cMessage(msg.ToString, eMessageType.DataValidation,
                                                                   eCoreComponentType.EcoSpace, eMessageImportance.Information))
            End If
        End Sub


        Private Sub getExtent(rec As cEcospaceTimeSeriesRec)
            Me.m_MaxRow = Math.Max(rec.Row, Me.m_MaxRow)
            Me.m_MaxCol = Math.Max(rec.Col, Me.m_MaxCol)
        End Sub


        Public Property FileName As String

        Public Property TimeStampFormatString As String

        Public ReadOnly Property StartDate As Date
            Get
                Return Me.m_StartDate
            End Get
        End Property

        Public ReadOnly Property EndDate As Date
            Get
                Return Me.m_EndDate
            End Get
        End Property

        Public ReadOnly Property MaxRow As Integer
            Get
                Return Me.m_MaxRow
            End Get
        End Property


        Public ReadOnly Property MaxCol As Integer
            Get
                Return Me.m_MaxCol
            End Get
        End Property

    End Class

End Namespace
