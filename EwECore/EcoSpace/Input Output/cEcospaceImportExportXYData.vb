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
Imports System.Text
Imports EwEUtils.Utilities
Imports EwEUtils.SpatialData
Imports EwEUtils.Core

#End Region ' Imports

' ToDo_JS: merge with EcospaceCSVResultWriter

' There is a high degree of overlap in the read/write logic here and in cEcospaceCSVResultWriter. That is silly.
' Moreover, it would be really nice if the logic presented here would be available to the spatial assets plugin via datasets.
' This is probably best accomplished by making this class and the EcospaceResultsWriter both use an cEcospaceLayer to provide access to their data.
' The plugin can then wrap this class as a IDataProvider to perform its import and export magic.

''' -----------------------------------------------------------------------
''' <summary>
''' Helper class for importing and exporting XY data from text files.
''' </summary>
''' -----------------------------------------------------------------------
Public Class cEcospaceImportExportXYData
    Implements IEcospaceImportExport

#Region " Private vars "

    Public Shared cMAPPING_IMPLICIT As String = My.Resources.CoreDefaults.CORE_DEFAULT

    Private m_core As cCore = Nothing
    Private m_bm As cEcospaceBasemap = Nothing

    ''' <summary>Buffer that holds the data to read or write.</summary>
    ''' <remarks>To save on memory we allow the use of value callbacks per field as an alternative to the buffer.</remarks>
    Private m_buffer As New Dictionary(Of String, Object())
    ''' <summary>All defined data fields.</summary>
    Private m_astrFields As String() = Nothing

    Private m_bRowColImplicit As Boolean = False

#End Region ' Private vars

#Region " Construction "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Construct a new instance of this class.
    ''' </summary>
    ''' <param name="core">The EwE Core to itneract with.</param>
    ''' <param name="astrFields">An optional array of field names.</param>
    ''' -------------------------------------------------------------------
    Public Sub New(core As cCore, _
                   Optional ByVal astrFields() As String = Nothing)

        Debug.Assert(core IsNot Nothing)
        Debug.Assert(core.EcospaceBasemap IsNot Nothing)

        Me.m_core = core
        Me.m_bm = core.EcospaceBasemap
        Me.Fields = astrFields

    End Sub

#End Region ' Construction

#Region " Read & Write "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns all duplicate <see cref="Fields">field names defined in the import/export data</see>.
    ''' </summary>
    ''' <returns>An array with duplicate <see cref="Fields">field names</see>.</returns>
    ''' -----------------------------------------------------------------------
    Public Function DuplicateFields() As String()

        Dim htNames As New HashSet(Of String)
        Dim lstrDuplicates As New List(Of String)
        Dim strField As String = ""

        For Each strField In Me.m_astrFields
            If htNames.Contains(strField) Then
                If Not lstrDuplicates.Contains(strField) Then
                    lstrDuplicates.Add(strField)
                End If
            Else
                htNames.Add(strField)
            End If
        Next

        lstrDuplicates.Sort()
        Return lstrDuplicates.ToArray

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write data to a XY text file. The format of the file is
    ''' 'col,row[,<see cref="Fields"/>]*', with a configurable the separator character.
    ''' Field names encountered in the file can be found in <see cref="Fields"/>.
    ''' </summary>
    ''' <param name="strFile">The name of the file to write.</param>
    ''' <param name="separator">The separator character to use. By default, CSV
    ''' values are separated by commas.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function ReadXYFields(ByVal strFile As String, _
                                 Optional ByVal separator As Char = ","c) As Boolean

        Dim tr As TextReader = Nothing
        Dim strLine As String = ""
        Dim astrFields As String() = Nothing
        Dim bSuccess As Boolean = True

        Try
            tr = New StreamReader(strFile)
        Catch ex As Exception
            Return False
        End Try

        Try
            ' Read fields line
            strLine = tr.ReadLine()
            astrFields = cStringUtils.SplitQualified(strLine, separator)

            ' Clean up
            For i As Integer = 0 To astrFields.Length - 1
                astrFields(i) = astrFields(i).Trim
            Next

            Me.Fields = astrFields

        Catch ex As Exception
            bSuccess = False
        End Try

        tr.Close()
        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write data to a XY text file. The format of the file is
    ''' 'col,row[,<see cref="Fields"/>]*', with a configurable the separator character.
    ''' Field names encountered in the file can be found in <see cref="Fields"/>.
    ''' </summary>
    ''' <param name="strFile">The name of the file to write.</param>
    ''' <param name="separator">The separator character to use. By default, CSV
    ''' values are separated by commas.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function ReadXYFile(ByVal strFile As String, _
                               ByVal strRowField As String, _
                               ByVal strColField As String, _
                               Optional ByVal separator As Char = ","c) As Boolean

        If Me.m_astrFields.Length = 0 Then
            If Not Me.ReadXYFields(strFile) Then Return False
        End If

        Dim tr As TextReader = Nothing
        Dim strLine As String = ""
        Dim astrFields As String() = Me.Fields
        Dim astrValues As String() = Nothing
        Dim iField As Integer
        Dim sValue As Single = 0.0!
        Dim iColField As Integer = -1
        Dim iRowField As Integer = -1
        Dim bSuccess As Boolean = True

        Try
            tr = New StreamReader(strFile)
        Catch ex As Exception
            Return False
        End Try

        Try
            ' Read fields line
            strLine = tr.ReadLine()

            iColField = Array.IndexOf(astrFields, strColField)
            iRowField = Array.IndexOf(astrFields, strRowField)

            If (iColField = -1 Or iRowField = -1) Then Return False

            While (tr.Peek() <> -1)
                strLine = tr.ReadLine()
                astrValues = strLine.Split(separator)

                For iField = 0 To astrFields.Length - 1
                    If (iField <> iRowField) And (iField <> iColField) Then
                        Me.Value(CInt(astrValues(iRowField)), CInt(astrValues(iColField)), astrFields(iField)) = astrValues(iField)
                    End If
                Next
            End While

        Catch ex As Exception
            bSuccess = False
        End Try

        tr.Close()
        Return bSuccess

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Write data to a XY text file. The format of the file is
    ''' '<paramref name="strColField"/>,<paramref name="strRowField"/>[,<see cref="Fields"/>]*'
    ''' </summary>
    ''' <param name="strFile">The file to write to.</param>
    ''' <param name="strColField">CSV header for 'col' field</param>
    ''' <param name="strRowField">CSV header for 'row' field</param>
    ''' <param name="bWaterCellsOnly">If true, only water cell data is written to the file.</param>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Public Function WriteXYFile(ByVal strFile As String, _
                                ByVal strColField As String, _
                                ByVal strRowField As String, _
                                Optional bWaterCellsOnly As Boolean = True) As Boolean

        If (Not cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFile), True)) Then
            Return False
        End If

        Dim strm As StreamWriter = Nothing
        Dim lstrFields As New List(Of String)
        Dim depth As cEcospaceLayerDepth = Me.m_bm.LayerDepth

        Try
            strm = New StreamWriter(strFile)
        Catch ex As Exception
            Return False
        End Try

        lstrFields.AddRange(Me.m_astrFields)
        lstrFields.Remove(strRowField)
        lstrFields.Remove(strColField)

        ' Write header line
        strm.Write(cStringUtils.ToCSVField(strColField))
        strm.Write(",")
        strm.Write(cStringUtils.ToCSVField(strRowField))
        strm.Write(",Lon,Lat")
        For iField As Integer = 0 To lstrFields.Count - 1
            strm.Write(",")
            strm.Write(cStringUtils.ToCSVField(Me.Fields(iField).Trim))
        Next
        strm.WriteLine()

        ' Write content
        For iRow As Integer = 1 To Me.m_bm.InRow
            For iCol As Integer = 1 To Me.m_bm.InCol

                ' Water cell filter
                If depth.IsWaterCell(iRow, iCol) Or Not bWaterCellsOnly Then
                    strm.Write(cStringUtils.FormatNumber(iCol))
                    strm.Write(",")
                    strm.Write(cStringUtils.FormatNumber(iRow))
                    strm.Write(",")
                    strm.Write(cStringUtils.FormatNumber(Me.m_bm.ColToLon(iCol)))
                    strm.Write(",")
                    strm.Write(cStringUtils.FormatNumber(Me.m_bm.RowToLat(iRow)))
                    For iField As Integer = 0 To Me.Fields.Length - 1
                        strm.Write(",")
                        Dim val As Object = Me.Value(iRow, iCol, Me.Fields(iField))
                        If (val IsNot Nothing) Then
                            Select Case val.GetType
                                Case GetType(Single), GetType(Double), GetType(Integer)
                                    strm.Write(cStringUtils.FormatNumber(val))
                                Case GetType(Boolean), GetType(String)
                                    strm.Write(cStringUtils.ToCSVField(CStr(val)))
                                Case Else
                            End Select
                        End If
                    Next iField
                    strm.WriteLine()
                End If
            Next iCol
        Next iRow

        strm.Flush()
        strm.Close()

        Return True

    End Function

#End Region ' Read & Write

#Region " Properties "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the fields that data is associated with.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Property Fields() As String()
        Get
            Return Me.m_astrFields
        End Get
        Set(ByVal value As String())

            If (value Is Nothing) Then
                Me.m_bRowColImplicit = True
            Else
                Me.m_bRowColImplicit = (value.Length = 0)
            End If

            If (Me.m_bRowColImplicit) Then
                Me.m_astrFields = New String() {cEcospaceImportExportXYData.cMAPPING_IMPLICIT}
            Else
                Dim lFields As New List(Of String)
                For Each strField As String In value
                    If Not String.IsNullOrWhiteSpace(strField) Then
                        lFields.Add(strField.Trim)
                    End If
                Next
                Me.m_astrFields = lFields.ToArray
            End If

            ' Clear
            Me.m_buffer.Clear()

            ' Create storage
            For Each strField As String In Me.Fields
                Dim asCells(Me.NumCells) As Object
                Me.m_buffer.Add(strField, asCells)
            Next

        End Set
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get/set a value in this class.
    ''' </summary>
    ''' <param name="iRow">One-based row index to access a value for.</param>
    ''' <param name="iCol">One-based column index to access a value for.</param>
    ''' <param name="strField">Optional field to access a value for.</param>
    ''' -------------------------------------------------------------------
    Public Property Value(ByVal iRow As Integer, ByVal iCol As Integer, Optional ByVal strField As String = "") As Object _
        Implements IEcospaceImportExport.Value

        Get
            Return Me.Value(Me.Seq(iRow, iCol), strField)
        End Get
        Set(ByVal value As Object)
            Me.Value(Me.Seq(iRow, iCol), strField) = value
        End Set
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get/set a value in this class.
    ''' </summary>
    ''' <param name="iCell">The one-based cell sequential index to access
    ''' a value for.</param>
    ''' <param name="strField">Optional field to access a value for.</param>
    ''' -------------------------------------------------------------------
    Public Property Value(ByVal iCell As Integer, Optional ByVal strField As String = "") As Object
        Get
            If String.IsNullOrEmpty(strField) Then
                strField = cEcospaceImportExportXYData.cMAPPING_IMPLICIT
            End If
            Return Me.m_buffer(strField)(iCell)
        End Get
        Set(ByVal value As Object)
            If String.IsNullOrWhiteSpace(strField) Then
                strField = cEcospaceImportExportXYData.cMAPPING_IMPLICIT
            End If
            Me.m_buffer(strField)(iCell) = value
        End Set
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get a cell sequential number from a (row, col) pair.
    ''' </summary>
    ''' <param name="iRow">One-based row index to get a cell for.</param>
    ''' <param name="iCol">One-based column index to get a cell for.</param>
    ''' <returns>A one-based sequence number for a cell.</returns>
    ''' -------------------------------------------------------------------
    Public Function Seq(ByVal iRow As Integer, ByVal iCol As Integer) As Integer
        If (Me.m_bm Is Nothing) Then Return 0
        'Zero base Cell
        Return (iRow - 1) * Me.m_bm.InCol + (iCol - 1)
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Returns the number of cells in this data.
    ''' </summary>
    ''' <returns>The number of cells in this data.</returns>
    ''' -------------------------------------------------------------------
    Public Function NumCells() As Integer
        If (Me.m_bm Is Nothing) Then Return 0
        Return Me.m_bm.InCol * Me.m_bm.InRow
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Returns true if no row and column fields have been defined.
    ''' </summary>
    ''' <returns>True if no row and column fields have been defined.</returns>
    ''' -------------------------------------------------------------------
    Public Function IsRowColImplicit() As Boolean
        Return Me.m_bRowColImplicit
    End Function

    ''' -------------------------------------------------------------------
    ''' <inheritdocs cref="IEcospaceImportExport.ToRaster"/>.
    ''' -------------------------------------------------------------------
    Public Function ToRaster(Optional ByVal strField As String = "") As ISpatialRaster _
        Implements IEcospaceImportExport.ToRaster
        Return New cEcospaceImportExportRaster(Me, strField)
    End Function

#End Region ' Properties

    Public ReadOnly Property CellSize As Double _
        Implements EwEUtils.Core.IEcospaceImportExport.CellSize
        Get
            Return Me.m_bm.CellSize
        End Get
    End Property

    Public ReadOnly Property InCol As Integer _
        Implements EwEUtils.Core.IEcospaceImportExport.InCol
        Get
            Return Me.m_bm.InCol
        End Get
    End Property

    Public ReadOnly Property InRow As Integer _
        Implements EwEUtils.Core.IEcospaceImportExport.InRow
        Get
            Return Me.m_bm.InRow
        End Get
    End Property

    Public ReadOnly Property NoDataValue As Double _
        Implements EwEUtils.Core.IEcospaceImportExport.NoDataValue
        Get
            Return cCore.NULL_VALUE
        End Get
    End Property

    Public ReadOnly Property ProjectionString As String _
        Implements EwEUtils.Core.IEcospaceImportExport.ProjectionString
        Get
            Return Me.m_bm.ProjectionString
        End Get
    End Property

    Public ReadOnly Property TopLeft As System.Drawing.PointF _
        Implements EwEUtils.Core.IEcospaceImportExport.PosTopLeft
        Get
            Return Me.m_bm.PosTopLeft
        End Get
    End Property

End Class