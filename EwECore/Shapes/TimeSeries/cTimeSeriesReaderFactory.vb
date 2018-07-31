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

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class; creates a <see cref="cTimeSeriesTextReader">Time series reader</see>
''' for a given <see cref="cTimeSeriesReaderFactory.eTimeSeriesReaderTypes">type of Time series input source</see>.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTimeSeriesReaderFactory

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type stating all supported time series input formats.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eTimeSeriesReaderTypes
        ''' <summary>Indicates a reader that can read Time Series data from a comma-separated file.</summary>
        CSV
        ''' <summary>Indicates a reader that can read Time Series data from the clipboard.</summary>
        Clipboard
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return a <see cref="cTimeSeriesTextReader">Time series text reader</see>
    ''' for a given <see cref="cTimeSeriesReaderFactory.eTimeSeriesReaderTypes">type of Time series input source</see>.
    ''' </summary>
    ''' <param name="core">The <see cref="cCore">Core</see> instance to obtain the reader for.</param>
    ''' <param name="readerType">The <see cref="cTimeSeriesReaderFactory.eTimeSeriesReaderTypes">type of Time series input source</see>
    ''' to rad from.</param>
    ''' <returns>
    ''' A <see cref="cTimeSeriesTextReader">Time series text reader</see> if successful, 
    ''' or Nothing/Null/Nada/Zip if an error occurred.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function GetTimeSeriesReader(ByVal core As cCore, ByVal readerType As eTimeSeriesReaderTypes) As cTimeSeriesTextReader
        Dim reader As cTimeSeriesTextReader = Nothing
        Select Case readerType
            Case eTimeSeriesReaderTypes.CSV
                reader = New cTimeSeriesCSVReader(core)
            Case eTimeSeriesReaderTypes.Clipboard
                reader = New cTimeSeriesClipboardReader(core)
            Case Else
                ' Wtf
                Debug.Assert(False, String.Format("Unable to create Time series text reader for input source {0}", readerType))
        End Select
        Return reader
    End Function

End Class
