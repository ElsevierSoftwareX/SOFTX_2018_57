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
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Implementation of <see cref="IEcospaceResultsWriter">IEcospaceResultsWriter</see> 
''' and <see cref="cEcospaceBaseResultsWriter">cEcospaceBaseResultsWriter</see> 
''' to save Ecospace results to a series of CSV files.
''' </summary>
''' <remarks>There will be one CSV file for each group containing data for all the time steps.</remarks>
''' ---------------------------------------------------------------------------
Public Class cEcospaceXYZTResultsWriter
    Inherits cEcospaceBaseResultsWriter

#Region " Overrides "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.DisplayName"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property DisplayName As String
        Get
            Return My.Resources.CoreDefaults.ECOSPACE_WRITER_XYZT
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.StartWrite"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub StartWrite()
        Try
            If Not Me.CreateOutputDir() Then
                ' ToDo: fail
            End If
            Me.WriteGroupFileHeaders(eVarNameFlags.EcospaceMapBiomass)
            Me.WriteGroupFileHeaders(eVarNameFlags.EcospaceMapCatch)
            Me.WriteFleetFileHeaders(eVarNameFlags.EcospaceMapEffort)
        Catch ex As Exception
            Me.m_core.Messages.SendMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_SAVEMAP_FAILED, ex.Message),
                                                        eMessageType.ErrorEncountered, eCoreComponentType.EcoSpace, eMessageImportance.Warning))
        End Try
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.WriteResults"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub WriteResults(ByVal SpaceTimeStepResults As Object)

        Dim vars() As eVarNameFlags = New eVarNameFlags() {eVarNameFlags.EcospaceMapBiomass, eVarNameFlags.EcospaceMapCatch}
        Dim tsData As cEcospaceTimestep = DirectCast(SpaceTimeStepResults, cEcospaceTimestep)
        Dim strm As StreamWriter = Nothing
        Dim strFile As String = ""

        Try

            If tsData.iTimeStep < Me.FirstOutputTimeStep Then
                Return
            End If

            For Each varname As eVarNameFlags In vars
                For igrp As Integer = 1 To Me.m_core.m_EcoPathData.NumLiving
                    strFile = Me.GetGroupFileName(varname, igrp, Me.FileExtension())
                    If cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFile), True) Then
                        'Handle file exceptions on a per file basis
                        'this way only the offending file will be skipped
                        'all other files will be written 
                        Try
                            strm = New StreamWriter(strFile, True)
                            If (strm IsNot Nothing) Then
                                Me.SaveCSV(strm, tsData, igrp, varname)
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

                    End If
                Next
            Next

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".WriteResults Exception: " & ex.Message)
        End Try

        ' Space effort
        For iFlt As Integer = 1 To Me.m_core.m_EcoPathData.NumFleet
            strFile = Me.GetFleetFileName(eVarNameFlags.EcospaceMapEffort, iFlt, Me.FileExtension())
            Try
                strm = New StreamWriter(strFile, True)
                If (strm IsNot Nothing) Then
                    Me.SaveCSV(strm, tsData, iFlt, eVarNameFlags.EcospaceMapEffort)
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
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.EndWrite"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub EndWrite()
        Dim msg As New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSPACE_EXPORT_CSV_SUCCESS, Me.m_OutputPath),
                                eMessageType.DataExport, eCoreComponentType.EcoSpace, eMessageImportance.Information)
        ' Provide hyperlink to the directory with the files
        msg.Hyperlink = Me.m_OutputPath
        Me.m_core.Messages.SendMessage(msg)
    End Sub

#End Region ' Overrides

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.FileExtension"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function FileExtension() As String
        Return ".csv"
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write run info header.
    ''' </summary>
    ''' <param name="strm">The writer to write to.</param>
    ''' <param name="igrp">The group to write the header for.</param>
    ''' <param name="varname">The variable name to write the header for.</param>
    ''' -----------------------------------------------------------------------
    Private Sub WriteHeader(ByRef strm As StreamWriter, ByVal igrp As Integer, ByVal varname As eVarNameFlags, TypeLabel As String, Type As String)

        Try
            Me.WriteRunInfo(strm)
            strm.WriteLine("Variable," + varname.ToString())
            strm.WriteLine(TypeLabel + "," + Type)
            strm.WriteLine()
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Save the data to a CSV file.
    ''' </summary>
    ''' <param name="strm"></param>
    ''' <param name="timestep">Time step results providing the values to write.</param>
    ''' <param name="iIndex"></param>
    ''' <param name="varname"></param>
    ''' -----------------------------------------------------------------------
    Private Sub SaveCSV(ByVal strm As StreamWriter,
                        ByVal timestep As cEcospaceTimestep,
                        ByVal iIndex As Integer,
                        ByVal varname As eVarNameFlags)

        Dim map As cEcospaceLayer = timestep.Layer(varname, iIndex)
        Dim sbBuff As New StringBuilder()
        Dim value As Double = 0

        Debug.Assert(map IsNot Nothing)

        strm.WriteLine("Step," & timestep.iTimeStep.ToString)
        strm.WriteLine("Year," & timestep.TimeStepinYears.ToString)
        For ir As Integer = 1 To Me.EcospaceData.InRow
            For ic As Integer = 1 To Me.EcospaceData.InCol
                If ic > 1 Then sbBuff.Append(",")
                value = CDbl(map.Cell(ir, ic))
                If (value <> cCore.NULL_VALUE) Then
                    value = Me.ScaleValue(value, timestep, iIndex, varname)
                End If
                sbBuff.Append(cStringUtils.FormatNumber(value))
            Next
            strm.WriteLine(sbBuff.ToString)
            sbBuff.Length = 0
        Next
        strm.WriteLine()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Saves the data to a XYZ formatted file.
    ''' </summary>
    ''' <param name="strm"></param>
    ''' <param name="SpaceTSData"></param>
    ''' <param name="iIndex"></param>
    ''' -----------------------------------------------------------------------
    Private Sub saveXYZ(ByVal strm As StreamWriter,
                        ByVal SpaceTSData As cEcospaceTimestep,
                        ByVal iIndex As Integer,
                        ByVal varname As eVarNameFlags)

        Dim map As cEcospaceLayer = SpaceTSData.Layer(varname, iIndex)

        Debug.Assert(map IsNot Nothing)

        ' Write header
        strm.WriteLine("X,Y,Z")
        ' Write data
        For ir As Integer = 1 To Me.EcospaceData.InRow
            For ic As Integer = 1 To Me.EcospaceData.InCol
                strm.WriteLine("{0},{1},{2}", ic, ir, cStringUtils.FormatSingle(CSng(map.Cell(ir, ic))))
            Next
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write headers for the all groups for the given variable.
    ''' </summary>
    ''' <param name="varname"></param>
    ''' -----------------------------------------------------------------------
    Private Sub WriteGroupFileHeaders(ByVal varname As eVarNameFlags)

        Dim strm As StreamWriter
        Dim strFN As String

        For igrp As Integer = 1 To Me.m_core.m_EcoPathData.NumLiving
            strFN = Me.GetGroupFileName(varname, igrp, "csv")
            'Create a new file when writting the header
            'this overwrites the data in the current directory
            strm = New StreamWriter(strFN)
            If Me.m_core.SaveWithFileHeader Then
                Me.WriteHeader(strm, igrp, varname, "Group name", cStringUtils.ToCSVField(Me.EcopathData.GroupName(igrp)))
            End If
            strm.Close()
            strm = Nothing
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write headers for the all groups for the given variable.
    ''' </summary>
    ''' <param name="varname"></param>
    ''' -----------------------------------------------------------------------
    Private Sub WriteFleetFileHeaders(ByVal varname As eVarNameFlags)

        Dim strm As StreamWriter
        Dim strFN As String

        For iflt As Integer = 1 To Me.m_core.m_EcoPathData.NumFleet
            strFN = Me.GetFleetFileName(varname, iflt, "csv")
            'Create a new file when writting the header
            'this overwrites the data in the current directory
            strm = New StreamWriter(strFN)
            If Me.m_core.SaveWithFileHeader Then
                Me.WriteHeader(strm, iflt, varname, "Fleet name", cStringUtils.ToCSVField(Me.EcopathData.FleetName(iflt)))
            End If
            strm.Close()
            strm = Nothing
        Next

    End Sub

#End Region ' Internals

End Class
