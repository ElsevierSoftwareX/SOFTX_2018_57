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
''' Base Class for ASC Specific Writers that implement <see cref="IEcospaceResultsWriter">IEcospaceResultsWriter</see> 
''' and <see cref="cEcospaceBaseResultsWriter">cEcospaceBaseResultsWriter</see> 
''' to write Ecospace output to ESRI ASCII files. 
''' </summary>
''' <remarks>Each ASCII file will contain an Ecospace value for a given group and time step</remarks>
''' ---------------------------------------------------------------------------
Public MustInherit Class cEcospaceASCBaseResultsWriter
    Inherits cEcospaceBaseResultsWriter

#Region " Base writer overrides "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.StartWrite"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub StartWrite()
        Try
            If Not Me.CreateOutputDir() Then
                ' ToDo: fail
            End If

            If Me.m_core.SaveWithFileHeader Then
                Me.WriteRunInfoFile()
            End If
        Catch ex As Exception
            Me.m_core.Messages.SendMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_SAVEMAP_FAILED, ex.Message),
                                                        eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, eMessageImportance.Warning))
        End Try
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.WriteResults"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub WriteResults(ByVal SpaceTimeStepResults As Object)

        Try

            Dim tsData As cEcospaceTimestep = DirectCast(SpaceTimeStepResults, cEcospaceTimestep)
            Dim strm As StreamWriter = Nothing
            Dim strFile As String = ""

            If tsData.iTimeStep < Me.FirstOutputTimeStep Then
                Return
            End If

            'Varnames are implemented by derived class
            For Each varname As eVarNameFlags In vars

                'NMaps(Number of maps) = NLiving by default but can be Overriden by derived class
                For iGrp As Integer = 0 To Me.NMaps

                    'SelectedGroups() MUST be set in implementation class. Currently in .Init() via cEcospaceBaseResultsWriter.setAllGroupsSelected(), setCatchSelected()...
                    'Or(potentially) via the UI's Group Selections
                    If Me.SelectedGroups(iGrp) Then

                        'GetFileName() groups by default, can overridden by derived classes.
                        strFile = Me.GetFileName(varname, iGrp, Me.FileExtension(), tsData.iTimeStep)
                        ' Create directory any time; user may have deleted it during a run
                        If (cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFile), True)) Then
                            'Handle file exceptions on a per file basis
                            'this way only the offending file will be skipped
                            'all other files will be written 

                            Try
                                strm = New StreamWriter(strFile, False)
                                If (strm IsNot Nothing) Then
                                    Me.SaveASCFile(strm, tsData, iGrp, varname)
                                    strm.Flush()
                                    strm.Close()
                                    strm = Nothing
                                End If
                            Catch ex As IOException
                                cLog.Write(ex)
                                Dim msg As New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_EXPORT_FAILED, strFile, ex.Message),
                                                       eMessageType.DataExport, eCoreComponentType.EcoSpace, eMessageImportance.Warning)
                                Me.m_core.Messages.SendMessage(msg)
                            End Try
                        End If 'cFileUtils.IsDirectoryAvailable()
                    End If 'Me.SelectedGroups(iGrp)
                Next iGrp
            Next varname

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".WriteResults Exception: " & ex.Message)
        End Try

    End Sub


    Protected Overridable Function NMaps() As Integer
        Return Me.m_core.m_EcoPathData.NumLiving
    End Function

    Protected Overridable Function GetFileName(ByVal varname As eVarNameFlags,
                                                    ByVal iGrp As Integer,
                                                    ByVal strExt As String,
                                                    Optional ByVal iModelTimeStep As Integer = cCore.NULL_VALUE) As String
        Return MyBase.GetGroupFileName(varname, iGrp, strExt, iModelTimeStep)
    End Function


    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.EndWrite"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub EndWrite()
        Dim msg As New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_EXPORT_ASCII_SUCCESS, Me.m_OutputPath),
                                eMessageType.DataExport, eCoreComponentType.EcoSpace, eMessageImportance.Information)
        ' Provide hyperlink to the directory with the files
        msg.Hyperlink = Me.m_OutputPath
        Me.m_core.Messages.SendMessage(msg)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.FileExtension"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function FileExtension() As String
        Return ".asc"
    End Function

#End Region ' Base writer overrides

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write the run information file to accompany the run results.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub WriteRunInfoFile()

        Try
            Dim strFN As String = Path.Combine(Me.OutputDirectory, "Ecospace RunInfo.txt")
            Dim strm As New StreamWriter(strFN, False)

            strm.WriteLine("EcoSpace .asc map output")
            Me.WriteRunInfo(strm)

            strm.Flush()
            strm.Close()
            strm = Nothing

        Catch ex As Exception

        End Try
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write an entire ASCII file for a group, time step and variable.
    ''' </summary>
    ''' <param name="strm"></param>
    ''' <param name="SpaceTSData"></param>
    ''' <param name="igrp"></param>
    ''' <param name="varName"></param>
    ''' -----------------------------------------------------------------------
    Protected Sub SaveASCFile(ByVal strm As StreamWriter, ByVal SpaceTSData As cEcospaceTimestep,
                              ByVal igrp As Integer, ByVal varName As eVarNameFlags)
        Try
            Me.WriteASCIIHeader(strm)
            Me.WriteASCIIBody(strm, SpaceTSData, igrp, varName)
        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".WriteResults() Exception: " & ex.Message)
        End Try
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write ESRI ASCII header block.
    ''' </summary>
    ''' <param name="writer">The <see cref="StreamWriter"/> to write to.</param>
    ''' -----------------------------------------------------------------------
    Protected Sub WriteASCIIHeader(ByVal writer As StreamWriter)

        Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap

        writer.WriteLine("ncols         " & Me.EcospaceData.InCol)
        writer.WriteLine("nrows         " & Me.EcospaceData.InRow)
        writer.WriteLine("xllcorner     " & cStringUtils.FormatNumber(Me.EcospaceData.Lon1))
        writer.WriteLine("yllcorner     " & cStringUtils.FormatNumber(Me.EcospaceData.Lat1 - (Me.EcospaceData.InRow) * bm.CellSize()))
        writer.WriteLine("cellsize      " & cStringUtils.FormatNumber(bm.CellSize()))
        writer.WriteLine("NODATA_value  " & cCore.NULL_VALUE)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write ESRI ASCII body block.
    ''' </summary>
    ''' <param name="writer">The <see cref="StreamWriter"/> to write to.</param>
    ''' <param name="iIndex">The layer index to write the data for.</param>
    ''' <param name="SpaceTSData">The Ecospace data structures to use for spatial referencing.</param>
    ''' <param name="varname">The variable to write.</param>
    ''' -----------------------------------------------------------------------
    Protected Sub WriteASCIIBody(ByVal writer As StreamWriter,
                                 ByVal SpaceTSData As cEcospaceTimestep,
                                 ByVal iIndex As Integer,
                                 ByVal varname As eVarNameFlags)

        Dim map As cEcospaceLayer = SpaceTSData.Layer(varname, iIndex)
        Dim value As Double = 0
        Dim strValue As String = ""

        Debug.Assert(map IsNot Nothing)

        For ir As Integer = 1 To Me.EcospaceData.InRow
            For ic As Integer = 1 To Me.EcospaceData.InCol
                If ic > 1 Then writer.Write(" ")
                If Me.EcospaceData.Depth(ir, ic) > 0 Then
                    value = CSng(map.Cell(ir, ic))
                    If (value <> cCore.NULL_VALUE) Then
                        value = Me.ScaleValue(value, SpaceTSData, iIndex, varname)
                    End If
                Else
                    'land as NODATAVALUE
                    value = cCore.NULL_VALUE
                End If

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

#End Region ' Internals

End Class
