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
Imports System.Drawing

#End Region ' Imports

' ToDo: bring in ASCII reader and writer logic from SpatialAssets Plugin

''' -----------------------------------------------------------------------
''' <summary>
''' Helper class for importing and exporting data from ASCII grid files
''' directly to and from Ecospace, without GIS intervention.
''' </summary>
''' -----------------------------------------------------------------------
Public Class cEcospaceImportExportASCIIData
    Implements IEcospaceImportExport

    ''' <summary>
    ''' Cell sequence number to value
    ''' </summary>
    Private m_buffer As New Dictionary(Of Integer, Object)

    Private m_nRows As Integer = 0
    Private m_nCols As Integer = 0
    Private m_dCellSize As Double = 0
    Private m_dNoData As Double = cCore.NULL_VALUE
    Private m_dXLLpos As Double
    Private m_dYLLpos As Double
    Private m_strProjectionString As String = ""

    Private m_bm As cEcospaceBasemap = Nothing

#Region " Construction "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Construct a new instance of this class.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Sub New(core As cCore)

        Me.m_bm = core.EcospaceBasemap
        Me.m_nCols = Me.m_bm.InCol
        Me.m_nRows = Me.m_bm.InRow
        Me.m_dCellSize = Me.m_bm.CellSize
        Me.m_dNoData = cCore.NULL_VALUE
        Me.m_strProjectionString = Me.m_bm.ProjectionString
        Me.m_dXLLpos = Me.m_bm.PosTopLeft.X
        Me.m_dYLLpos = Me.m_bm.PosBottomRight.Y
    End Sub

    Public Sub New()

    End Sub

#End Region ' Construction

    Public Function Read(l As cEcospaceLayer) As Boolean
        Me.m_buffer.Clear()
        If (l Is Nothing) Then Return False
        For ir As Integer = 1 To Me.m_nRows
            For ic As Integer = 1 To Me.m_nCols
                Me.Value(ir, ic) = l.Cell(ir, ic)
            Next
        Next
        Return True
    End Function

    Public Function Read(strFile As String) As Boolean

        Dim rd As StreamReader = Nothing
        Dim bSuccess As Boolean = True

        Dim strFileProj As String = Path.ChangeExtension(strFile, ".prj")
        If File.Exists(strFileProj) Then
            Try
                rd = New StreamReader(strFileProj)
                Me.m_strProjectionString = rd.ReadToEnd()
                rd.Close()
            Catch ex As Exception
                bSuccess = False
            End Try
        End If

        Try
            rd = New StreamReader(strFile)
            bSuccess = bSuccess And Me.ReadHeader(rd) And Me.ReadBody(rd)
            rd.Close()

        Catch ex As Exception
            ' Kaboom
            bSuccess = False
        End Try

        Return bSuccess

    End Function

    Public Function Save(strFile As String) As Boolean

        If Not cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFile), True) Then Return False

        Try
            Using wr As New StreamWriter(strFile)
                Me.WriteASCIIHeader(wr)
                Me.WriteASCIIBody(wr)
                wr.Close()
            End Using

            Using wr As New StreamWriter(Path.ChangeExtension(strFile, ".prj"))
                wr.WriteLine(Me.m_strProjectionString)
                wr.Close()
            End Using

        Catch ex As Exception
            Return False
        End Try
        Return True

    End Function

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
            Return Me.Value(Me.Seq(iRow, iCol))
        End Get
        Set(ByVal value As Object)
            Me.Value(Me.Seq(iRow, iCol)) = value
        End Set
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get/set a value in this class.
    ''' </summary>
    ''' <param name="iCell">The one-based cell sequential index to access
    ''' a value for.</param>
    ''' -------------------------------------------------------------------
    Private Property Value(ByVal iCell As Integer) As Object
        Get
            If Not Me.m_buffer.ContainsKey(iCell) Then Return Me.m_dNoData
            Return Me.m_buffer(iCell)
        End Get
        Set(ByVal value As Object)
            Me.m_buffer(iCell) = value
        End Set
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Returns data in the form of a <see cref="ISpatialRaster"/>>
    ''' </summary>
    ''' <returns>A raster.</returns>
    ''' -------------------------------------------------------------------
    Public Function ToRaster(Optional ByVal strField As String = "") As ISpatialRaster _
        Implements IEcospaceImportExport.ToRaster
        Return New cEcospaceImportExportRaster(Me, strField)
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get a cell sequential number from a (row, col) pair.
    ''' </summary>
    ''' <param name="iRow">One-based row index to get a cell for.</param>
    ''' <param name="iCol">One-based column index to get a cell for.</param>
    ''' <returns>A one-based sequence number for a cell.</returns>
    ''' -------------------------------------------------------------------
    Private Function Seq(ByVal iRow As Integer, ByVal iCol As Integer) As Integer
        Return (iRow - 1) * Me.m_nCols + (iCol - 1)
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Returns the number of cells in this data.
    ''' </summary>
    ''' <returns>The number of cells in this data.</returns>
    ''' -------------------------------------------------------------------
    Public Function NumCells() As Integer
        Return Me.m_nCols * Me.m_nRows
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Read the ASCII header from a text reader.
    ''' </summary>
    ''' <param name="reader">The open stream reader to read from.</param>
    ''' <returns>True if successful.</returns>
    ''' <remarks>
    ''' This method aims to read a complete raster header as described in
    ''' http://resources.esri.com/help/9.3/arcgisengine/com_cpp/GP_ToolRef/Spatial_Analyst_Tools/esri_ascii_raster_format.htm.
    ''' If any of the header fields is missing the method will fail.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Protected Function ReadHeader(ByVal reader As StreamReader) As Boolean

        Dim nCols As Integer = 0
        Dim nRows As Integer = 0
        Dim dXLLpos As Double = 0.0
        Dim bIsCenterX As Boolean = False
        Dim dYLLpos As Double = 0.0
        Dim bIsCenterY As Boolean = False
        Dim dCellSize As Double = 0.0
        Dim dNoData As Double = -9999

        Dim strField As String = ""
        Dim strValue As String = ""
        Dim strLine As String
        Dim bSuccess As Boolean = True
        Dim checksum As Byte = 0

        ' Read the file until EOF or all header fields are read without any errors
        While (Not reader.EndOfStream) And (bSuccess) And (checksum < &H2F)

            ' Read a line
            strLine = reader.ReadLine()

            ' Be nice
            If Not String.IsNullOrWhiteSpace(strLine) Then

                ' Remove all double spaces
                While strLine.IndexOf("  ") > 0
                    strLine = strLine.Replace("  ", " ")
                End While

                ' Split by space
                Dim astrBits() As String = strLine.Split(" "c)
                strField = astrBits(0)
                strValue = astrBits(1)

                ' Check header field (eliminating tabs etc, lower case)
                Select Case strField.Trim().ToLower

                    Case "ncols"
                        bSuccess = bSuccess And Integer.TryParse(strValue, nCols)
                        bSuccess = bSuccess And (nCols > 0)
                        checksum = CByte(checksum Or &H1)

                    Case "nrows"
                        bSuccess = bSuccess And Integer.TryParse(strValue, nRows)
                        bSuccess = bSuccess And (nRows > 0)
                        checksum = CByte(checksum Or &H2)

                    Case "xllcorner"
                        bSuccess = bSuccess And Double.TryParse(strValue, dXLLpos)
                        bIsCenterX = False
                        checksum = CByte(checksum Or &H4)

                    Case "xllcenter"
                        bSuccess = bSuccess And Double.TryParse(strValue, dXLLpos)
                        bIsCenterX = True
                        checksum = CByte(checksum Or &H4)

                    Case "yllcorner"
                        bSuccess = bSuccess And Double.TryParse(strValue, dYLLpos)
                        bIsCenterY = False
                        checksum = CByte(checksum Or &H8)

                    Case "yllcenter"
                        bSuccess = bSuccess And Double.TryParse(strValue, dYLLpos)
                        bIsCenterY = True
                        checksum = CByte(checksum Or &H8)

                    Case "cellsize"
                        bSuccess = bSuccess And Double.TryParse(strValue, dCellSize)
                        bSuccess = bSuccess And (dCellSize > 0)
                        checksum = CByte(checksum Or &H10)

                    Case "nodatavalue", "nodata_value"
                        bSuccess = bSuccess And Double.TryParse(strValue, dNoData)
                        checksum = CByte(checksum Or &H20)

                    Case Else
                        ' Unexpected field name
                        bSuccess = False

                End Select
            End If

        End While

        ' All good?
        If (bSuccess) Then
            ' #Yes: offset header positions if need be
            If (bIsCenterX) Then
                dXLLpos -= dCellSize / 2
                dYLLpos -= dCellSize / 2
            End If

            If (Me.m_bm Is Nothing) Then
                Me.m_nCols = nCols
                Me.m_nRows = nRows
                Me.m_dCellSize = dCellSize
                Me.m_dNoData = dNoData
                Me.m_dXLLpos = dXLLpos
                Me.m_dYLLpos = dYLLpos

            Else
                Dim sz As Single = Me.m_bm.CellSize
                bSuccess = (nCols = Me.m_bm.InCol) And
                           (nRows = Me.m_bm.InRow)
                'bSuccess = bSuccess and cNumberUtils.Approximates(dXLLpos, Me.m_bm.PosBottomRight.X, sz / 100) And
                '           cNumberUtils.Approximates(dYLLpos, Me.m_bm.PosBottomRight.Y, sz / 100) And
                '           cNumberUtils.Approximates(dCellSize, Me.m_bm.CellSize, sz / 100) And
            End If

        End If

        ' Done
        Return bSuccess

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Read the ASCII body from a text reader.
    ''' </summary>
    ''' <param name="reader">The open stream reader to read from.</param>
    ''' <returns>True if successful.</returns>
    ''' -------------------------------------------------------------------
    Protected Function ReadBody(ByVal reader As StreamReader) As Boolean

        Dim value As Double = 0
        Dim strValue As String = ""
        Dim bSuccess As Boolean = True

        Try
            For ir As Integer = 1 To Me.m_nRows
                If (Not reader.EndOfStream) Then
                    'ASC files written by GDAL contain a space at the start of the line so strip it off
                    'this should not affect other ASC file reading
                    Dim strLine As String = reader.ReadLine.Trim()
                    Dim astrBits() As String = strLine.Split(" "c)
                    For ic As Integer = 1 To Math.Min(Me.m_nCols, astrBits.Length)
                        If Not Double.TryParse(astrBits(ic - 1), value) Then
                            value = Me.m_dNoData
                        End If
                        Me.Value(Me.Seq(ir, ic)) = value
                    Next
                Else
                    bSuccess = False
                End If
            Next
        Catch ex As Exception
            bSuccess = False
        End Try
        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write ESRI ASCII header block.
    ''' </summary>
    ''' <param name="writer">The <see cref="StreamWriter"/> to write to.</param>
    ''' -----------------------------------------------------------------------
    Protected Sub WriteASCIIHeader(ByVal writer As StreamWriter)

        writer.WriteLine("ncols         " & Me.m_nCols)
        writer.WriteLine("nrows         " & Me.m_nRows)
        writer.WriteLine("xllcorner     " & cStringUtils.FormatNumber(Me.m_dXLLpos))
        writer.WriteLine("yllcorner     " & cStringUtils.FormatNumber(Me.m_dYLLpos))
        writer.WriteLine("cellsize      " & cStringUtils.FormatNumber(Me.m_dCellSize))
        writer.WriteLine("NODATA_value  " & cStringUtils.FormatNumber(Me.m_dNoData))

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write ESRI ASCII body block.
    ''' </summary>
    ''' <param name="writer">The <see cref="StreamWriter"/> to write to.</param>
    ''' -----------------------------------------------------------------------
    Protected Sub WriteASCIIBody(ByVal writer As StreamWriter)

        Dim value As Object = 0
        Dim strValue As String = ""

        For ir As Integer = 1 To Me.m_nRows
            For ic As Integer = 1 To Me.m_nCols
                If ic > 1 Then writer.Write(" ")
                value = Me.Value(Me.Seq(ir, ic))
                ' Fix #1321 - always make sure the first cell value is written as floating point
                strValue = cStringUtils.FormatNumber(value)
                If (ir = 1 And ic = 1) Then
                    If (strValue.IndexOf("."c) = -1) Then
                        strValue = strValue + ".0"
                    End If
                End If
                writer.Write(strValue)
            Next
            writer.WriteLine("")
        Next

    End Sub

    Public ReadOnly Property CellSize As Double _
        Implements IEcospaceImportExport.CellSize
        Get
            Return Me.m_dCellSize
        End Get
    End Property

    Public ReadOnly Property InCol As Integer _
        Implements IEcospaceImportExport.InCol
        Get
            Return Me.m_nCols
        End Get
    End Property

    Public ReadOnly Property InRow As Integer _
        Implements IEcospaceImportExport.InRow
        Get
            Return Me.m_nRows
        End Get
    End Property

    Public ReadOnly Property NoDataValue As Double _
        Implements IEcospaceImportExport.NoDataValue
        Get
            Return Me.m_dNoData
        End Get
    End Property

    Public ReadOnly Property ProjectionString As String _
        Implements IEcospaceImportExport.ProjectionString
        Get
            Return Me.m_strProjectionString
        End Get
    End Property

    Public ReadOnly Property TopLeft As PointF _
        Implements IEcospaceImportExport.PosTopLeft
        Get
            Return New PointF(CSng(Me.m_dXLLpos), CSng(Me.m_dYLLpos + Me.m_nRows * Me.m_dCellSize))
        End Get
    End Property

End Class
