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
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Reads one or more time series from a text input source.
''' </summary>
''' <remarks>
''' Time series text input format is hopefully described in the latest manual.
''' </remarks>
''' ---------------------------------------------------------------------------
Public MustInherit Class cTimeSeriesTextReader
    Implements ICollection(Of cTimeSeriesImport)

#Region " Private vars "

    Private m_core As cCore = Nothing

    ''' <summary>Start year of the time series.</summary>
    Private m_iFirstYear As Integer = 0
    ''' <summary>Number of years in the time series.</summary>
    Private m_iNumPoints As Integer = 0

    ''' <summary>Internal list of read time series objects.</summary>
    Private m_ts As New List(Of cTimeSeriesImport)
    ''' <summary>A <see cref="cPreview">preview</see> how the reader has interpreted the text source, allowing a user interface to tune the read process.</summary>
    Private m_tsPreview As cPreview = Nothing
    ''' <summary>String delimiting character to use when splitting the text into different columns.</summary>
    Private m_strDelimiter As String = ""
    ''' <summary>Decimal separator to use when interpreting floating point values in the text.</summary>
    Private m_strDecimalSeparator As String = ""
    ''' <summary>Dat apoint interval.</summary>
    Private m_interval As eTSDataSetInterval = eTSDataSetInterval.Annual

#End Region ' Private vars

#Region " Preview class "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class; showing how a <see cref="cTimeSeriesTextReader">cTimeSeriesTextReader</see>
    ''' has interpreted the incoming time series data. The preview allows a user interface to
    ''' interactively adjust the reader to correctly import time series.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cPreview

        ''' <summary>Number of columns encountered in the external time series text.</summary>
        ''' <remarks>Note that this number does not per definition denote the number of time series in the external text!</remarks>
        Private m_iColumnCount As Integer
        ''' <summary>Original lines of text encountered in the time series text.</summary>
        Private m_alRows As New List(Of String)
        ''' <summary>Lines of text from the time series text, split by delimiter.</summary>
        Private m_alRowValues As New List(Of String())
        ''' <summary>Errors encountered for each line of text.</summary>
        Private m_alRowErrors As New List(Of String)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this class.
        ''' </summary>
        ''' <param name="iColumnCount">The number of columns found in the time series text.</param>
        ''' -----------------------------------------------------------------------
        Friend Sub New(ByVal iColumnCount As Integer)
            Me.m_iColumnCount = iColumnCount
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Add a row to the preview.
        ''' </summary>
        ''' <param name="strLine">The original line of text, as read from the time series text.</param>
        ''' <param name="astrValues">The line of text, as split by the requested delimiter.</param>
        ''' -----------------------------------------------------------------------
        Friend Sub AddRow(ByVal strLine As String, ByVal astrValues() As String)
            If String.IsNullOrEmpty(strLine) Then Return
            Me.m_alRows.Add(strLine)
            Me.m_alRowValues.Add(astrValues)
            Me.m_alRowErrors.Add("")
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the number of rows in the preview.
        ''' </summary>
        ''' <returns>The number of rows in the preview</returns>
        ''' -----------------------------------------------------------------------
        Public Function RowCount() As Integer
            Return Me.m_alRowValues.Count
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the number of columns in the preview.
        ''' </summary>
        ''' <returns>The number of columns in the preview</returns>
        ''' -----------------------------------------------------------------------
        Public Function ColumnCount() As Integer
            Return Me.m_iColumnCount
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns an original line of text.
        ''' </summary>
        ''' <param name="iRow">The row number to obtain the text for. Note that this
        ''' value is 1-based.</param>
        ''' <returns>An original row of text, as read from the time series text.</returns>
        ''' -----------------------------------------------------------------------
        Public Function Row(ByVal iRow As Integer) As String
            If (iRow > 0 And iRow <= Me.m_alRowErrors.Count) Then Return Me.m_alRows(iRow - 1)
            Return ""
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns an error for a line from the time series text.
        ''' </summary>
        ''' <param name="iRow">The row number to obtain the error text for. Note that this
        ''' value is 1-based.</param>
        ''' <value></value>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Property RowError(ByVal iRow As Integer) As String
            Get
                If (iRow > 0 And iRow <= Me.m_alRowErrors.Count) Then Return Me.m_alRowErrors(iRow - 1)
                Return ""
            End Get
            Friend Set(value As String)
                If (iRow > 0 And iRow <= Me.m_alRowErrors.Count) Then Me.m_alRowErrors(iRow - 1) = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns a (col, row) value as distilled from the time series text.
        ''' </summary>
        ''' <param name="iColumn">The column number to obtain the error text for. Note that this
        ''' value is 1-based.</param>
        ''' <param name="iRow">The row number to obtain the error text for. Note that this
        ''' value is 1-based.</param>
        ''' <returns>A (col, row) value as distilled from the time series text.</returns>
        ''' -----------------------------------------------------------------------
        Public Property Value(ByVal iColumn As Integer, ByVal iRow As Integer) As String
            Get
                If (iRow > 0 And iRow <= Me.m_alRowErrors.Count) Then
                    Dim astrValues As String() = Me.m_alRowValues(iRow - 1)
                    If (iColumn > 0 And iColumn <= astrValues.Length) Then
                        Return astrValues(iColumn - 1)
                    End If
                End If
                Return ("")
            End Get
            Friend Set(ByVal value As String)
                If (iRow > 0 And iRow <= Me.m_alRowErrors.Count) Then
                    Dim astrValues As String() = Me.m_alRowValues(iRow - 1)
                    If (iColumn > 0 And iColumn <= astrValues.Length) Then
                        astrValues(iColumn - 1) = value
                        Me.m_alRowValues(iRow - 1) = astrValues
                    End If
                End If
            End Set
        End Property

        Public Function HasErrors() As Boolean

            Dim bHasErrors As Boolean = (Me.RowCount <= 3) Or (Me.m_iColumnCount <= 1)
            For iRow As Integer = 1 To Me.RowCount
                bHasErrors = bHasErrors Or Not String.IsNullOrWhiteSpace(Me.RowError(iRow))
            Next
            Return bHasErrors

        End Function

    End Class

#End Region ' Preview class

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new instance of this class.
    ''' </summary>
    ''' <param name="core">A reference to the <see cref="cCore">Core</see> that
    ''' this reader belongs to.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore)
        Me.m_core = core
    End Sub

#End Region ' Constructor

#Region " Reading "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Reads any number of Time Series data from a text source. The
    ''' Time Series are exposed by this collection as <see cref="cTimeSeries">cTimeSeries</see>
    ''' objects.
    ''' </summary>
    ''' <param name="strDelimiter">
    ''' String delimiting character to use when splitting the text into different columns.
    ''' </param>
    ''' <param name="strDecimalSeparator">
    ''' Decimal separator to use when interpreting floating point values in the text.
    ''' </param>
    ''' <returns>True when successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overridable Function Read(ByVal strDelimiter As String, _
                                     ByVal strDecimalSeparator As String, _
                                     ByVal interval As eTSDataSetInterval) As Boolean

        ' Reset reader to clear any previous read results.
        Me.Reset()

        ' Sanity check
        If String.Compare(strDelimiter, strDecimalSeparator, True) = 0 Then
            Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_IDENTICALSEPARATORS)
            Return False
        End If

        ' Store delimiter and decimal separator
        Me.m_strDelimiter = strDelimiter
        Me.m_strDecimalSeparator = strDecimalSeparator
        Me.m_interval = interval

        ' Asses data validity and build preview
        If Not Me.AnalyzeData() Then Return False
        ' Read the data
        Return Me.ReadTimeSeriesFromText()

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The one entry point to access the source text reader. Override this method 
    ''' to implement a connection to the appropriate text source. Note that the
    ''' reader obtained via this method should be released by overriding
    ''' <see cref="ReleaseReader">ReleaseReader</see>.
    ''' </summary>
    ''' <returns>A TextReader if the connection could be made, or
    ''' Nothing if an error occurred.</returns>
    ''' -----------------------------------------------------------------------
    Public MustOverride Function GetReader() As TextReader

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The one entry point to release a text reader obtained via
    ''' <see cref="GetReader">GetReader</see>.
    ''' </summary>
    ''' <returns>A TextReader if the connection could be made, or
    ''' Nothing if an error occurred.</returns>
    ''' -----------------------------------------------------------------------
    Public MustOverride Function ReleaseReader(ByVal reader As TextReader) As Boolean

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Reset the reader.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub Reset()
        ' Remove read time series
        Me.m_ts.Clear()
        ' Clear preview
        Me.m_tsPreview = Nothing
        Me.m_iFirstYear = 0
        Me.m_iNumPoints = 0
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Analyze the data
    ''' </summary>
    ''' <returns>True if valid</returns>
    ''' -----------------------------------------------------------------------
    Private Function AnalyzeData() As Boolean

        Dim reader As TextReader = Me.GetReader()
        Dim strLine As String = ""
        Dim astrCols() As String
        Dim iLineNumber As Integer = 0
        Dim iYear As Integer = 0
        Dim iPrevYear As Integer = 0
        Dim bSucces As Boolean = True
        Dim iWeightFactors As Integer = 0

        Me.Reset()

        ' Sanity checks
        If (reader Is Nothing) Then Return False

        Try

            ' Count number of captions from header line
            strLine = Me.ReadLine(reader, iLineNumber)

            If String.IsNullOrEmpty(strLine) Then
                ' Init preview
                Me.m_tsPreview = New cPreview(0)
                Return True
            End If

            ' Init preview
            astrCols = Me.SplitLine(strLine)
            Me.m_tsPreview = New cPreview(astrCols.Length)

            ' Add header to preview
            Me.m_tsPreview.AddRow(strLine, astrCols)

            ' Next line
            strLine = Me.ReadLine(reader, iLineNumber)
            astrCols = Me.SplitLine(strLine)
            Me.m_tsPreview.AddRow(strLine, astrCols)

            ' Is this the weight line?
            ' 060613VC: There may be a Weight for each time series from now on
            If cStringUtils.BeginsWith(astrCols(0), "weight") Then
                If Not Me.ValidateLine(m_tsPreview.ColumnCount, astrCols) Then
                    Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_WEIGHTVALUEMISSING, iLineNumber)
                    bSucces = False
                End If

                ' Advance to next line
                strLine = Me.ReadLine(reader, iLineNumber)
                astrCols = Me.SplitLine(strLine)
                Me.m_tsPreview.AddRow(strLine, astrCols)
                iWeightFactors += 1
            End If

            If cStringUtils.BeginsWith(astrCols(0), "cv") Then
                If Not Me.ValidateLine(m_tsPreview.ColumnCount, astrCols) Then
                    Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_CVVALUEMISSING, iLineNumber)
                    bSucces = False
                End If

                ' Advance to next line
                strLine = Me.ReadLine(reader, iLineNumber)
                astrCols = Me.SplitLine(strLine)
                Me.m_tsPreview.AddRow(strLine, astrCols)
                iWeightFactors += 1
            End If

            If (iWeightFactors > 1) Then
                Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_TOOMANYWEIGHTS, iLineNumber - 2)
                Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_TOOMANYWEIGHTS, iLineNumber - 1)
                bSucces = False
            End If

            ' Pool code
            If Not cStringUtils.BeginsWithOneOf(astrCols(0), New String() {"pool", "group", "fleet"}) Then
                Me.ReportError(cStringUtils.Localize(My.Resources.CoreMessages.TIMESERIES_ERROR_POOLLINEMISSING, astrCols(0)), iLineNumber)
                bSucces = False
            End If
            If Not Me.ValidateLine(m_tsPreview.ColumnCount, astrCols) Then
                Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_POOLVALUEMISSING, iLineNumber)
                bSucces = False
            End If

            ' ToDo: validate pool code values

            ' Next line
            strLine = Me.ReadLine(reader, iLineNumber)
            astrCols = Me.SplitLine(strLine)
            Me.m_tsPreview.AddRow(strLine, astrCols)

            If astrCols(0).ToLower.Contains("sec") Then
                strLine = Me.ReadLine(reader, iLineNumber)
                astrCols = Me.SplitLine(strLine)
                Me.m_tsPreview.AddRow(strLine, astrCols)
            End If

            ' Dat type
            If Not cStringUtils.BeginsWithOneOf(astrCols(0), New String() {"type", "code", "dat"}) Then
                Me.ReportError(cStringUtils.Localize(My.Resources.CoreMessages.TIMESERIES_ERROR_TYPELINEMISSING, astrCols(0)), iLineNumber)
                bSucces = False
            End If

            If Not Me.ValidateLine(m_tsPreview.ColumnCount, astrCols) Then
                Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_TYPEVALUEMISSING, iLineNumber)
                bSucces = False
            End If

            ' ToDo: validate types, etc
            ' ToDo: convert numeric type to string representation

            ' Next
            strLine = Me.ReadLine(reader, iLineNumber)
            While Not String.IsNullOrEmpty(strLine)

                astrCols = Me.SplitLine(strLine)

                ' JS 290815: Addressed issue #1391 (skip empty value lines)
                Dim bIsLineEmpty As Boolean = True
                For Each col As String In astrCols
                    bIsLineEmpty = bIsLineEmpty And String.IsNullOrWhiteSpace(col)
                Next

                If (Not bIsLineEmpty) Then

                    Me.m_tsPreview.AddRow(strLine, astrCols)

                    Try
                        iYear = cStringUtils.ConvertToInteger(astrCols(0))
                    Catch ex As Exception
                        iYear = -9999
                    End Try

                    If (iYear = -9999) Then
                        Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_LINEMISSING, iLineNumber)
                        bSucces = False
                    Else

                        ' Fix Start year if not set
                        If (Me.m_iFirstYear = 0) Then
                            Me.m_iNumPoints = 0
                            Me.m_iFirstYear = iYear
                        End If
                        Me.m_iNumPoints += 1

                        If Not Me.ValidateLine(m_tsPreview.ColumnCount, astrCols) Then
                            Me.ReportError(cStringUtils.Localize(My.Resources.CoreMessages.TIMESERIES_ERROR_VALUEMISSING, iYear), iLineNumber)
                            bSucces = False
                        End If

                    End If
                End If

                ' Next
                strLine = Me.ReadLine(reader, iLineNumber)

            End While

        Catch ex As Exception
            ' Report generic error
            Me.ReportError(ex.Message)
            ' Abort any attempt to make sense of this
            bSucces = False
        End Try

        Me.ReleaseReader(reader)

        If (Not bSucces) Then Me.m_iNumPoints = 0

        ' Bye!
        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Reads time series in local collection of cTimeSeries objects.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function ReadTimeSeriesFromText() As Boolean

        Dim tr As TextReader = Me.GetReader()
        Dim ts As cTimeSeriesImport = Nothing
        Dim iNumSeries As Integer = m_tsPreview.ColumnCount - 1
        Dim strLine As String = ""
        Dim iLineNumber As Integer = 0
        Dim cols() As String
        Dim sValue As Single = 0.0

        ' Temp buffers for creating Time Series objects
        Dim asWeight(iNumSeries) As Single
        Dim asCV(iNumSeries) As Single
        Dim astrNames(iNumSeries) As String
        Dim datpool(iNumSeries) As Integer
        Dim datpool2(iNumSeries) As Integer
        Dim tstype(iNumSeries) As eTimeSeriesType

        Me.m_ts.Clear()

        ' Sanity checks
        If (tr Is Nothing) Then Return False

        ' Init all weights to 1 by default
        For i As Integer = 0 To iNumSeries : asWeight(i) = 1.0! : Next i

        ' Read names from columns
        cols = Me.SplitLine(Me.ReadLine(tr, iLineNumber))
        For i As Integer = 1 To iNumSeries : astrNames(i - 1) = cols(i) : Next i

        ' Read weight from columns
        cols = Me.SplitLine(Me.ReadLine(tr, iLineNumber))
        If (String.Compare(cols(0), "weight", True) = 0) Then
            Try
                For i As Integer = 1 To iNumSeries : asWeight(i - 1) = cStringUtils.ConvertToSingle(cols(i), 0, Me.m_strDecimalSeparator) : Next i
            Catch ex As Exception
                Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_WEIGHTFORMAT, iLineNumber)
            End Try
            cols = Me.SplitLine(Me.ReadLine(tr, iLineNumber))
        ElseIf (String.Compare(cols(0), "cv", True) = 0) Then
            Try
                For i As Integer = 1 To iNumSeries : asCV(i - 1) = cStringUtils.ConvertToSingle(cols(i), 0, Me.m_strDecimalSeparator) : Next i
            Catch ex As Exception
                Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_WEIGHTFORMAT, iLineNumber)
            End Try
            cols = Me.SplitLine(Me.ReadLine(tr, iLineNumber))
        End If

        ' Read pool code from columns
        Try
            For i As Integer = 1 To Math.Min(iNumSeries, cols.Length - 1)
                If Not String.IsNullOrWhiteSpace(cols(i)) Then
                    datpool(i - 1) = cStringUtils.ConvertToInteger(cols(i))
                End If
            Next i
        Catch ex As Exception
            Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_POOLFORMAT, iLineNumber)
        End Try
        cols = Me.SplitLine(Me.ReadLine(tr, iLineNumber))

        ' Read secundary pool code from columns
        If (cols.Length > 0) Then
            If (cols(0).ToLower().Contains("sec")) Or (cols(0).ToLower().Contains("2")) Then
                Try
                    For i As Integer = 1 To Math.Min(iNumSeries, cols.Length - 1)
                        If Not String.IsNullOrWhiteSpace(cols(i)) Then
                            datpool2(i - 1) = cStringUtils.ConvertToInteger(cols(i))
                        End If
                    Next i
                Catch ex As Exception
                    Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_POOLFORMAT, iLineNumber)
                End Try
                cols = Me.SplitLine(Me.ReadLine(tr, iLineNumber))
            End If
        End If

        ' Read type from columns
        Try
            For i As Integer = 1 To iNumSeries

                ' Extract time series type
                ' JS28oct09 allow strings as time series types too
                tstype(i - 1) = Me.ToTimeSeriesType(cols(i))
                If (tstype(i - 1) = eTimeSeriesType.NotSet) Then
                    tstype(i - 1) = DirectCast(cStringUtils.ConvertToInteger(cols(i)), eTimeSeriesType)
                    If (tstype(i - 1) = cCore.NULL_VALUE) Then
                        Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_TYPEFORMAT, iLineNumber)
                    End If
                End If

                ' Validate pool code(s) fits the corresponding core counter
                Select Case cTimeSeriesFactory.TimeSeriesCategory(tstype(i - 1))

                    Case eTimeSeriesCategoryType.Group
                        ' Leniency: if dp2 is entered instead of dp, use dp2
                        If (datpool(i - 1) = 0 And datpool2(i - 1) > 0) Then
                            datpool(i - 1) = datpool2(i - 1)
                            datpool2(i - 1) = 0
                        End If

                        ' Group index cannot exceed core nGroups
                        If (datpool(i - 1) < 1) Or (datpool(i - 1) > Me.m_core.GetCoreCounter(eCoreCounterTypes.nGroups)) Then
                            Me.ReportError(cStringUtils.Localize(My.Resources.CoreMessages.TIMESERIES_ERROR_INVALIDGROUP, datpool(i - 1)), iLineNumber - 1)
                        End If

                    Case eTimeSeriesCategoryType.Fleet
                        ' Leniency: if dp2 is entered instead of dp, use dp2
                        If (datpool(i - 1) = 0 And datpool2(i - 1) > 0) Then
                            datpool(i - 1) = datpool2(i - 1)
                            datpool2(i - 1) = 0
                        End If

                        'Fleet index cannot exceed core nFleets
                        If ((datpool(i - 1) < 1) Or (datpool(i - 1) > Me.m_core.GetCoreCounter(eCoreCounterTypes.nFleets))) Then
                            Me.ReportError(cStringUtils.Localize(My.Resources.CoreMessages.TIMESERIES_ERROR_INVALIDFLEET, datpool(i - 1)), iLineNumber - 1)
                        End If

                    Case eTimeSeriesCategoryType.FleetGroup

                        'Fleet index cannot exceed core nFleets
                        If ((datpool(i - 1) < 1) Or (datpool(i - 1) > Me.m_core.GetCoreCounter(eCoreCounterTypes.nFleets))) Then
                            Me.ReportError(cStringUtils.Localize(My.Resources.CoreMessages.TIMESERIES_ERROR_INVALIDFLEET, datpool(i - 1)), iLineNumber - 1)
                        End If

                        ' Group index cannot exceed core nGroups
                        If (datpool2(i - 1) < 1) Or (datpool2(i - 1) > Me.m_core.GetCoreCounter(eCoreCounterTypes.nGroups)) Then
                            Me.ReportError(cStringUtils.Localize(My.Resources.CoreMessages.TIMESERIES_ERROR_INVALIDGROUP, datpool2(i - 1)), iLineNumber - 1)
                        End If

                    Case eTimeSeriesCategoryType.Forcing
                        ' All good

                End Select
            Next i

        Catch ex As Exception
            Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_TYPEFORMAT, iLineNumber)
        End Try
        cols = Me.SplitLine(Me.ReadLine(tr, iLineNumber))

        ' Initialize time series objects
        For i As Integer = 0 To iNumSeries - 1

            ts = New cTimeSeriesImport(Me.m_iNumPoints, tstype(i))

            ' Configure time series
            With ts
                .Name = astrNames(i)
                .WtType = asWeight(i)
                .CV = asCV(i)
                .DatPool = datpool(i)
                .DatPoolSec = datpool2(i)
                .ResizeData(Me.m_iNumPoints)
            End With

            ' Add it
            Me.m_ts.Add(ts)
        Next

        ' Values
        For iRow As Integer = 1 To Me.m_iNumPoints

            For iColumn As Integer = 1 To iNumSeries

                Dim bAllowCoreNull As Boolean = Me.m_ts(iColumn - 1).SupportsNull()

                ' Reset value
                sValue = If(bAllowCoreNull, cCore.NULL_VALUE, 0)

                ' Reset preview
                Me.m_tsPreview.Value(iColumn + 1, iLineNumber) = ""

                ' Has a column value?
                If (iColumn < cols.Length) Then
                    ' #Yes: get the value  
                    Try
                        ' Big change 16Nov16: read empty cells as NULL values for supporting time series. 
                        If (Not String.IsNullOrWhiteSpace(cols(iColumn))) Then
                            ' Try to parse the value
                            sValue = cStringUtils.ConvertToSingle(cols(iColumn), 0, Me.m_strDecimalSeparator)
                            ' Add parsed value to preview
                            If (sValue <> 0) Or bAllowCoreNull Then
                                ' Write preview value
                                Me.m_tsPreview.Value(iColumn + 1, iLineNumber) = CStr(sValue)
                            End If
                        End If
                    Catch ex As Exception
                        ' JS04feb08: error parsing value
                        Me.ReportError(My.Resources.CoreMessages.TIMESERIES_ERROR_VALUEFORMAT, iLineNumber)
                        ' Add original string to preview
                        Me.m_tsPreview.Value(iColumn + 1, iLineNumber) = cols(iColumn)
                    End Try
                End If

                ' Store converted value
                Me.m_ts(iColumn - 1).ShapeData(iRow - 1) = sValue

            Next iColumn

            ' Next line
            cols = Me.SplitLine(Me.ReadLine(tr, iLineNumber))

        Next iRow

        Return True

    End Function

#Region " Helper methods "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Validates the number of columns encountered in a line of text.
    ''' </summary>
    ''' <param name="iNumCols">The number of columns that is expected.</param>
    ''' <param name="astrCols">The columns in the line of text.</param>
    ''' <returns>True if the number of columns validated succesfully.</returns>
    ''' -----------------------------------------------------------------------
    Private Function ValidateLine(ByVal iNumCols As Integer, ByVal astrCols() As String) As Boolean
        Return iNumCols >= astrCols.Length
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Reads a line of text from the time series data.
    ''' </summary>
    ''' <param name="tr">The reader to read from.</param>
    ''' <param name="iLineNumber">The line number that is being read. This number
    ''' is be incremented when a line of text is read succesfully.</param>
    ''' <returns>True when a line of text is read succesfully</returns>
    ''' -----------------------------------------------------------------------
    Private Function ReadLine(ByVal tr As TextReader, ByRef iLineNumber As Integer) As String
        Dim strLine As String = ""

        If tr.Peek() = -1 Then Return strLine
        Try
            strLine = tr.ReadLine()
            iLineNumber += 1
        Catch e As Exception
            Me.ReportError(e.Message, iLineNumber)
        End Try

        Return strLine
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, splits a line of text by the current <see cref="Delimiter">Delimiter</see>.
    ''' </summary>
    ''' <param name="strLine">The line to split.</param>
    ''' <returns>An array of strings.</returns>
    ''' -----------------------------------------------------------------------
    Private Function SplitLine(ByVal strLine As String) As String()
        Dim astrBits As String() = cStringUtils.SplitQualified(strLine, Me.m_strDelimiter)
        ' Trim spaces
        For iBit As Integer = 0 To astrBits.Length - 1
            astrBits(iBit) = astrBits(iBit).Trim
        Next
        Return astrBits
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; adds an error message to the data preview for a given line.
    ''' If the line is not specified the error message is directly sent to the EwE system.
    ''' </summary>
    ''' <param name="strError">Error text to report.</param>
    ''' <param name="iLineNumber">Text line that this error occurred at, or
    ''' <see cref="cCore.NULL_VALUE">cCore.NULL_VALUE</see> if the line number
    ''' is irrelevant.</param>
    ''' -----------------------------------------------------------------------
    Private Sub ReportError(ByVal strError As String, _
                            Optional ByVal iLineNumber As Integer = cCore.NULL_VALUE)

        ' Flag line error if possible
        If iLineNumber <> cCore.NULL_VALUE Then
            'Dim sb As New StringBuilder()
            'Dim strLineError As String = Me.m_tsPreview.RowError(iLineNumber)
            'If (Not String.IsNullOrWhiteSpace(strLineError)) Then
            '    sb.AppendLine(strLineError)
            'End If
            'sb.Append(strError)
            Me.m_tsPreview.RowError(iLineNumber) = strError
        Else
            Me.m_core.m_publisher.SendMessage(New cMessage(strError, eMessageType.DataImport, eCoreComponentType.TimeSeries, eMessageImportance.Warning))
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Convert a time series type string to a time series enumerated type.
    ''' </summary>
    ''' <param name="strTimeSeries"></param>
    ''' <returns>
    ''' A time series type value, or <see cref="eTimeSeriesType.NotSet">NotSet</see>
    ''' if the conversion failed.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Private Function ToTimeSeriesType(ByVal strTimeSeries As String) As eTimeSeriesType

        strTimeSeries = strTimeSeries.Trim

        For Each [alias] As eTimeSeriesAliases In [Enum].GetValues(GetType(eTimeSeriesAliases))
            If String.Compare([alias].ToString, strTimeSeries, True) = 0 Then
                Return DirectCast([alias], eTimeSeriesType)
            End If
        Next [alias]

        For Each [type] As eTimeSeriesType In [Enum].GetValues(GetType(eTimeSeriesType))
            If String.Compare([type].ToString, strTimeSeries, True) = 0 Then
                Return [type]
            End If
        Next [type]

        Return eTimeSeriesType.NotSet

    End Function

#End Region ' Helper methods

#End Region ' Internals

#End Region ' Reading

#Region " Collection "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add an item to the collection.
    ''' </summary>
    ''' <remarks>
    ''' This collection is read-only, only to be manipulated through the <see cref="Read">Read</see> interface.
    ''' </remarks>
    ''' <param name="item">Item NOT to add :P</param>
    ''' -----------------------------------------------------------------------
    Public Sub Add(ByVal item As cTimeSeriesImport) _
            Implements System.Collections.Generic.ICollection(Of cTimeSeriesImport).Add
        ' Read-only
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Clears the collection.
    ''' </summary>
    ''' <remarks>
    ''' This collection is read-only, only to be manipulated through the <see cref="Read">Read</see> interface.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Sub Clear() _
            Implements System.Collections.Generic.ICollection(Of cTimeSeriesImport).Clear
        ' Read-only
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the collection contains a given item.
    ''' </summary>
    ''' <param name="item">The Item to locate in the collection</param>
    ''' <returns>True if the item was found.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Contains(ByVal item As cTimeSeriesImport) As Boolean _
            Implements System.Collections.Generic.ICollection(Of cTimeSeriesImport).Contains
        Return Me.m_ts.Contains(item)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Copies the collection to a strong-typed array of <see cref="cTimeSeries">cTimeSeries</see> objects.
    ''' </summary>
    ''' <param name="array">The array to copy to.</param>
    ''' <param name="arrayIndex">The index to start the copy process at.</param>
    ''' -----------------------------------------------------------------------
    Public Sub CopyTo(ByVal array() As cTimeSeriesImport, ByVal arrayIndex As Integer) Implements _
            System.Collections.Generic.ICollection(Of cTimeSeriesImport).CopyTo
        Me.m_ts.CopyTo(array, arrayIndex)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the number of items in the collection.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Count() As Integer _
            Implements System.Collections.Generic.ICollection(Of cTimeSeriesImport).Count
        Get
            Return Me.m_ts.Count
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' States whether the collection is read-only.
    ''' </summary>
    ''' <returns>Always true.</returns>
    ''' <remarks>
    ''' This collection is always read-only, only to be manipulated through the <see cref="Read">Read</see> interface.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsReadOnly() As Boolean _
            Implements System.Collections.Generic.ICollection(Of cTimeSeriesImport).IsReadOnly
        Get
            Return True
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Removes an item to the collection.
    ''' </summary>
    ''' <remarks>
    ''' This collection is read-only, only to be manipulated through the <see cref="Read">Read</see> interface.
    ''' </remarks>
    ''' <param name="item">Item NOT to remove.</param>
    ''' <returns>Always false.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Remove(ByVal item As cTimeSeriesImport) As Boolean _
            Implements System.Collections.Generic.ICollection(Of cTimeSeriesImport).Remove
        ' Read only
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a strong-typed enumerator for this collection.
    ''' </summary>
    ''' <returns>A strong-typed enumerator for this collection.</returns>
    ''' -----------------------------------------------------------------------
    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of cTimeSeriesImport) _
            Implements System.Collections.Generic.IEnumerable(Of cTimeSeriesImport).GetEnumerator
        Return Me.m_ts.GetEnumerator()
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a weak-typed enumerator for this collection.
    ''' </summary>
    ''' <remarks>This obligatory override is silly, obsolete and therefore hidden from view.</remarks>
    ''' <returns>A weak-typed enumerator for this collection.</returns>
    ''' -----------------------------------------------------------------------
    Private Function GetEnumeratorObligatoryOverrideWhichWeDoNotNeedAtAllThankYou() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return Me.GetEnumerator()
    End Function

#End Region ' Collection

#Region " Properties "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a double indexed array of strings with a preview of read time
    ''' series data. Data is indexed by (column, row). Indexes are zero based.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function Preview() As cPreview
        Return Me.m_tsPreview
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the delimiter used by the reader.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function Delimiter() As String
        Return Me.m_strDelimiter
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the decimal separator used by the reader.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function DecimalSeparator() As String
        Return Me.m_strDecimalSeparator
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the first year of time series data found by the reader.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property FirstYear() As Integer
        Get
            Return Me.m_iFirstYear
        End Get
        Friend Set(ByVal iStartYear As Integer)
            Me.m_iFirstYear = iStartYear
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the number of years of time series data found by the reader.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property NumPoints() As Integer
        Get
            Return Me.m_iNumPoints
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the number of years of time series data found by the reader.
    ''' </summary>
    ''' <remarks>
    ''' This method has been deprecated in favour of <see cref="NumPoints"/>.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    <Obsolete("Use NumPoints instead")> _
    Public ReadOnly Property NumYears() As Integer
        Get
            Return Me.NumPoints
        End Get
    End Property

    Public MustOverride ReadOnly Property Dataset() As String

#End Region ' Properties

End Class
