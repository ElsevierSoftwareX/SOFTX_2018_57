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

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Reads one or more time series from a CSV file.
''' </summary>
''' <remarks>
''' For a description of the CSV file layout, refer to 
''' <see cref="cTimeSeriesTextReader">cTimeSeriesTextReader</see>.
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class cTimeSeriesCSVReader
    Inherits cTimeSeriesTextReader

    ''' <summary>Path to the CSV file that was read.</summary>
    Private m_strFileName As String = ""
    Private m_stream As FileStream = Nothing

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new instance of this class.
    ''' </summary>
    ''' <param name="core">A reference to the <see cref="cCore">Core</see> that
    ''' this reader belongs to.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore)
        MyBase.New(core)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Reads any number of Time Series data from a text source. The
    ''' Time Series are exposed by this collection as <see cref="cTimeSeries">cTimeSeries</see>
    ''' objects.
    ''' </summary>
    ''' <param name="strFileName">Path to the CSV file to read.</param>
    ''' <param name="strDelimiter">
    ''' String delimiting character to use when splitting the text into different columns.
    ''' </param>
    ''' <param name="strDecimalSeparator">
    ''' Decimal separator to use when interpreting floating point values in the text.
    ''' </param>
    ''' <returns>True when successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overloads Function Read(ByVal strFileName As String, _
                                   ByVal strDelimiter As String, _
                                   ByVal strDecimalSeparator As String, _
                                   ByVal interval As eTSDataSetInterval) As Boolean
        ' Store file name
        Me.m_strFileName = strFileName
        ' Let the baseclass do the work
        Return MyBase.Read(strDelimiter, strDecimalSeparator, interval)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Access the content of the CSV file via a <see cref="TextReader">TextReader</see>.
    ''' </summary>
    ''' <returns>A TextReader connected to the CSV file.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function GetReader() As TextReader

        ' Sanity checks
        If String.IsNullOrEmpty(Me.m_strFileName) Then Return Nothing
        If Not File.Exists(Me.m_strFileName) Then Return Nothing

        Me.m_stream = New FileStream(Me.m_strFileName, _
                                     FileMode.Open, _
                                     FileAccess.Read, _
                                     FileShare.ReadWrite Or FileShare.Delete Or FileShare.Inheritable)
        Return New StreamReader(Me.m_stream)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The one entry point to release a text reader obtained via
    ''' <see cref="GetReader">GetReader</see>.
    ''' </summary>
    ''' <returns>A TextReader if the connection could be made, or
    ''' Nothing if an error occurred.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ReleaseReader(ByVal reader As TextReader) As Boolean
        Try
            reader.Close()
            Me.m_stream.Close()
        Catch ex As Exception
            ' Yippee
        End Try
        Return True
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get a description of the CSV file.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property Dataset() As String
        Get
            Return Path.GetFileNameWithoutExtension(Me.m_strFileName)
        End Get
    End Property

End Class
