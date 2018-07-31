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
Imports EwEUtils.Core


Namespace EcospaceTimeSeries

    Public Class cEcospaceTimeSeriesRec

#Region "Private Properties and Definitions"

        Private Enum eDataCols
            Row
            Col
            GroupID
            Timestamp
            Value
        End Enum

        Private m_validation As eTimeSeriesRecValidations

#End Region

#Region "Public Properties"

        Public iGroupID As Integer = cCore.NULL_VALUE
        Public Row As Integer = cCore.NULL_VALUE
        Public Col As Integer = cCore.NULL_VALUE
        Public TimeStamp As Date = New Date(1, 1, 1)
        Public CellValue As Single = cCore.NULL_VALUE

        Public PredError As Double = cCore.NULL_VALUE
        Public PredictedValue As Double = cCore.NULL_VALUE

        Public VarType As eVarNameFlags = eVarNameFlags.EcospaceMapBiomass

        Public InputTimeStepFormat As String = "yyyy-MM-dd"

#End Region

#Region "Constructors"

        Private Sub New(strRec As String, TimeStepFormatString As String, Optional DataType As eVarNameFlags = eVarNameFlags.EcospaceMapBiomass)
            Dim data() As String

            Me.InputTimeStepFormat = TimeStepFormatString
            VarType = DataType
            Me.m_validation = eTimeSeriesRecValidations.isReadValid

            Dim tempTimeStamp As Date
            Try
                If Me.PaserString(strRec, data) Then
                    Me.Row = EwEUtils.Utilities.cStringUtils.ConvertToInteger(data(eDataCols.Row))
                    Me.Col = EwEUtils.Utilities.cStringUtils.ConvertToInteger(data(eDataCols.Col))
                    Me.iGroupID = EwEUtils.Utilities.cStringUtils.ConvertToInteger(data(eDataCols.GroupID))
                    tempTimeStamp = EwEUtils.Utilities.cStringUtils.ConvertToDate(data(eDataCols.Timestamp), InputTimeStepFormat)
                    'strip the day off of the timestamp
                    Me.TimeStamp = New Date(tempTimeStamp.Year, tempTimeStamp.Month, 1)
                    Me.CellValue = EwEUtils.Utilities.cStringUtils.ConvertToSingle(data(eDataCols.Value))
                End If
            Catch ex As Exception

                'Set all the values to NULL
                Me.Row = cCore.NULL_VALUE
                Me.Col = cCore.NULL_VALUE
                Me.iGroupID = cCore.NULL_VALUE
                Me.TimeStamp = New Date(1, 1, 1)
                Me.CellValue = cCore.NULL_VALUE

            End Try

        End Sub

        Private Sub New()

        End Sub

#End Region

#Region "Public Methods"

        Shared Function FromString(strRec As String, DateFormatString As String, Optional DataType As eVarNameFlags = eVarNameFlags.EcospaceMapBiomass) As cEcospaceTimeSeriesRec
            Try
                Return New cEcospaceTimeSeriesRec(strRec, DateFormatString, DataType)
            Catch ex As Exception
                'Ok something really messed up 
                System.Console.WriteLine(ex)
            End Try
            'Failed to create a rec from the string
            'return object with default empty values
            Return New cEcospaceTimeSeriesRec()
        End Function


        Public Function ToCSVString() As String
            Dim csvStr As New System.Text.StringBuilder()
            Dim delim As String = ","

            csvStr.Append(EwEUtils.Utilities.cStringUtils.ToCSVField(Me.Row))
            csvStr.Append(delim)
            csvStr.Append(EwEUtils.Utilities.cStringUtils.ToCSVField(Me.Col))
            csvStr.Append(delim)
            csvStr.Append(EwEUtils.Utilities.cStringUtils.ToCSVField(Me.iGroupID))
            csvStr.Append(delim)
            'Output timestamp format in hard coded
            csvStr.Append(EwEUtils.Utilities.cStringUtils.ToCSVField(Me.TimeStamp.ToString("yyyy-MM-dd")))
            csvStr.Append(delim)
            csvStr.Append(EwEUtils.Utilities.cStringUtils.ToCSVField(Me.CellValue))
            csvStr.Append(delim)
            csvStr.Append(EwEUtils.Utilities.cStringUtils.ToCSVField(Me.PredictedValue))
            csvStr.Append(delim)
            csvStr.Append(EwEUtils.Utilities.cStringUtils.ToCSVField(Me.PredError))

            Return csvStr.ToString
        End Function


        Public ReadOnly Property ReadValidation As eTimeSeriesRecValidations
            Get
                Return Me.m_validation
            End Get
        End Property

        Public Sub ClearResults()
            PredError = cCore.NULL_VALUE
            PredictedValue = cCore.NULL_VALUE
        End Sub

#End Region

#Region "Private Methods"

        Private Function PaserString(recString As String, ByRef data() As String) As Boolean
            Dim bReturn As Boolean = True
            data = EwEUtils.Utilities.cStringUtils.SplitQualified(recString, ",")
            If data.Length < 5 Then
                Me.m_validation = eTimeSeriesRecValidations.MalformedString
                Return False
            End If
            For irec As Integer = 0 To 4
                If String.IsNullOrEmpty(data(irec)) Then
                    Me.m_validation = eTimeSeriesRecValidations.EmptyRec
                    Return False
                End If
            Next

            If Not data(eDataCols.Timestamp).Contains("-") Then
                Me.m_validation = eTimeSeriesRecValidations.InvalidDateFormat
                Return False
            End If

            Return bReturn
        End Function


#End Region

    End Class

End Namespace

