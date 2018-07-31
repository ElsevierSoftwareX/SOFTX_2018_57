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
Imports EwECore.MSE
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

''' <summary>
''' Helper class for writing MSE interations to file.
''' </summary>
Friend Class cMSECSVOutputWriter
    Implements IMSEOutputWriter

    Private m_core As cCore
    Private m_MSEdata As cMSEDataStructures

    Public Sub New(ByVal theCore As cCore, ByVal MSEData As cMSEDataStructures)
        Me.m_core = theCore
        Me.m_MSEdata = MSEData
    End Sub

    Public Function getOutputFileName(ByVal strDataType As String, ByVal strDataName As String) As String
        Dim strOutputFileName As String = Path.Combine(Me.DataDir, cFileUtils.ToValidFileName(strDataType & " " & strDataName & ".csv", False))
        cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strOutputFileName), True)
        Return strOutputFileName
    End Function

    Public ReadOnly Property DataDir() As String
        Get
            Return Me.m_core.DefaultOutputPath(eAutosaveTypes.MSE)
        End Get
    End Property

    Public Sub saveIteration(ByVal ListOfData As Dictionary(Of cMSE.eResultsData, Single(,))) Implements IMSEOutputWriter.saveIteration

        If Not Me.m_core.Autosave(eAutosaveTypes.MSE) Then Return

        Dim buff As New StringBuilder()
        Dim strm As StreamWriter = Nothing
        Dim esData As cEcosimDatastructures = Me.m_core.m_EcoSimData
        Dim epData As cEcopathDataStructures = Me.m_core.m_EcoPathData
        Dim strFile As String

        Try
            'We could set this up so each type had a seperate flag for dumping

            'Biomass
            For igrp As Integer = 1 To Me.m_MSEdata.NGroups
                Try
                    strFile = getOutputFileName(cMSE.BIOMASS_DATA, epData.GroupName(igrp))
                    buff.Length = 0
                    For its As Integer = 1 To Me.m_core.GetCoreCounter(eCoreCounterTypes.nEcosimTimeSteps)
                        If (its > 1) Then buff.Append(", ")
                        buff.Append(cStringUtils.FormatSingle(esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, igrp, its)))
                    Next

                    strm = New StreamWriter(strFile, True)
                    strm.WriteLine(buff)
                    strm.Close()
                Catch ex As Exception
                    ' Debug.Assert(False, Me.ToString & " Exception saving results to file " & getFilename(BIOMASS_DATA, epdata.GroupName(igrp)))
                    System.Console.WriteLine(Me.ToString & " Failed to write data to file " & strFile & " Exception: " & ex.Message)
                    cLog.Write(ex, "MSECSVOutputWriter::BIOMASS_DATA_" & igrp)
                End Try
            Next

            'Catch by group
            For igrp As Integer = 1 To Me.m_MSEdata.NGroups
                Try
                    If epData.fCatch(igrp) > 0 Then
                        strFile = getOutputFileName(cMSE.CATCH_DATA, epData.GroupName(igrp))
                        buff.Length = 0
                        For its As Integer = 1 To Me.m_core.GetCoreCounter(eCoreCounterTypes.nEcosimTimeSteps)
                            If (its > 1) Then buff.Append(", ")
                            buff.Append(cStringUtils.FormatSingle(esData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Yield, igrp, its)))
                        Next

                        strm = New StreamWriter(strFile, True)
                        strm.WriteLine(buff)
                        strm.Close()
                    End If
                Catch ex As Exception
                    ' Debug.Assert(False, Me.ToString & " Exception saving results to file " & getFilename(BIOMASS_DATA, epdata.GroupName(igrp)))
                    System.Console.WriteLine(Me.ToString & " Failed to write data to file " & strFile & " Exception: " & ex.Message)
                    cLog.Write(ex, "MSECSVOutputWriter::CATCH_DATA_" & igrp)
                End Try
            Next

            'Quota by group
            For igrp As Integer = 1 To Me.m_MSEdata.NGroups
                Try
                    Dim data(,) As Single = ListOfData.Item(cMSE.eResultsData.GroupQuota)
                    If epData.fCatch(igrp) > 0 Then
                        strFile = getOutputFileName(cMSE.QUOTAGROUP_DATA, epData.GroupName(igrp))
                        buff.Length = 0
                        For its As Integer = 1 To Me.m_core.GetCoreCounter(eCoreCounterTypes.nEcosimTimeSteps)
                            If (its > 1) Then buff.Append(", ")
                            buff.Append(cStringUtils.FormatSingle(data(igrp, its)))
                        Next

                        strm = New StreamWriter(strFile, True)
                        strm.WriteLine(buff)
                        strm.Close()
                    End If
                Catch ex As Exception
                    ' Debug.Assert(False, Me.ToString & " Exception saving results to file " & getFilename(BIOMASS_DATA, epdata.GroupName(igrp)))
                    System.Console.WriteLine(Me.ToString & " Failed to write data to file " & strFile & " Exception: " & ex.Message)
                    cLog.Write(ex, "MSECSVOutputWriter::QUOTAGROUP_DATA_" & igrp)
                End Try
            Next

            'Catch by fleet
            For iflt As Integer = 1 To Me.m_MSEdata.nFleets
                Try
                    strFile = getOutputFileName(cMSE.FLEETCATCH_DATA, epData.FleetName(iflt))
                    buff.Length = 0
                    For its As Integer = 1 To Me.m_core.GetCoreCounter(eCoreCounterTypes.nEcosimTimeSteps)
                        If (its > 1) Then buff.Append(", ")
                        buff.Append(cStringUtils.FormatSingle(esData.ResultsSumCatchByGear(iflt, its)))
                    Next

                    strm = New StreamWriter(strFile, True)
                    strm.WriteLine(buff)
                    strm.Close()
                Catch ex As Exception
                    'Debug.Assert(False, Me.ToString & " Exception saving results to file " & getFilename(CATCH_DATA, epdata.FleetName(iflt)))
                    System.Console.WriteLine(Me.ToString & " Failed to write data to file " & getOutputFileName(cMSE.FLEETCATCH_DATA, epData.FleetName(iflt)) & " Exception: " & ex.Message)
                    cLog.Write(ex, "MSECSVOutputWriter::FLEETCATCH_DATA_" & iflt)
                End Try
            Next

            'Effort by fleet
            For iflt As Integer = 1 To Me.m_MSEdata.nFleets
                Try
                    strFile = getOutputFileName(cMSE.EFFORT_DATA, epData.FleetName(iflt))
                    buff.Length = 0
                    For its As Integer = 1 To Me.m_core.GetCoreCounter(eCoreCounterTypes.nEcosimTimeSteps)
                        If its > 1 Then buff.Append(", ")
                        buff.Append(cStringUtils.FormatSingle(esData.ResultsEffort(iflt, its)))
                    Next

                    strm = New StreamWriter(strFile, True)
                    strm.WriteLine(buff)
                    strm.Close()
  
                Catch ex As Exception
                    ' Debug.Assert(False, Me.ToString & " Exception saving results to file " & getFilename(EFFORT_DATA, epdata.GroupName(iflt)))
                    System.Console.WriteLine(Me.ToString & " Failed to write data to file " & strFile & " Exception: " & ex.Message)
                    cLog.Write(ex, "MSECSVOutputWriter::EFFORT_DATA_" & iflt)
                End Try
            Next

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".SaveIteration() Exception: " & ex.Message)
        End Try

        buff.Length = 0
        buff = Nothing

        ' ToDo: add core message stating save successes and errors

    End Sub

    Private Sub WriteOutputHeader(ByVal DataDescription As String, ByVal GroupFleet As String, ByVal DataFileName As String)

        Try
            If Not Me.m_core.Autosave(eAutosaveTypes.MSE) Then Return

            Dim header As StringBuilder
            Dim strm As StreamWriter

            header = New StringBuilder()
            Dim d As DateTime = Date.Now

            If Me.m_core.SaveWithFileHeader Then
                header.AppendLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.MSE))
                header.AppendLine("Rows = MSE Run, Columns = Time")
            End If

            For it As Integer = 1 To Me.m_core.nEcosimTimeSteps
                If it > 1 Then header.Append(", ")
                header.Append(cStringUtils.FormatInteger(it))
            Next

            strm = New StreamWriter(Me.getOutputFileName(DataFileName, GroupFleet), True)
            strm.WriteLine(header)
            strm.Close()

        Catch ex As Exception

        End Try

    End Sub

    Public Sub Init() Implements IMSEOutputWriter.Init

        If Not Me.m_core.Autosave(eAutosaveTypes.MSE) Then Return

        Try
            Dim epData As cEcopathDataStructures = Me.m_core.m_EcoPathData

            If Not cFileUtils.IsDirectoryAvailable(Me.DataDir, True) Then Exit Sub

            'clear out any existing data files
            For igrp As Integer = 1 To Me.m_MSEdata.NGroups
                Try
                    File.Delete(Me.getOutputFileName(cMSE.BIOMASS_DATA, epData.GroupName(igrp)))
                    File.Delete(Me.getOutputFileName(cMSE.CATCH_DATA, epData.GroupName(igrp)))
                    File.Delete(Me.getOutputFileName(cMSE.QUOTAGROUP_DATA, epData.GroupName(igrp)))
                Catch ex As Exception
                    System.Console.WriteLine(ex.Message)
                End Try
            Next igrp

            For iflt As Integer = 1 To Me.m_MSEdata.nFleets
                Try
                    File.Delete(Me.getOutputFileName(cMSE.FLEETCATCH_DATA, epData.FleetName(iflt)))
                    File.Delete(Me.getOutputFileName(cMSE.EFFORT_DATA, epData.FleetName(iflt)))
                Catch ex As Exception
                    System.Console.WriteLine()
                End Try
            Next iflt

            'Write output file headers

            For igrp As Integer = 1 To Me.m_MSEdata.NGroups
                Me.WriteOutputHeader("Biomass", epData.GroupName(igrp), cMSE.BIOMASS_DATA)
                If epData.fCatch(igrp) > 0 Then
                    Me.WriteOutputHeader("Catch by Group", epData.GroupName(igrp), cMSE.CATCH_DATA)
                    Me.WriteOutputHeader("Quota by Group", epData.GroupName(igrp), cMSE.QUOTAGROUP_DATA)
                End If
            Next

            For iflt As Integer = 1 To Me.m_MSEdata.nFleets
                Me.WriteOutputHeader("Catch by Fleet", epData.FleetName(iflt), cMSE.FLEETCATCH_DATA)
                Me.WriteOutputHeader("Effort by Fleet", epData.FleetName(iflt), cMSE.EFFORT_DATA)
            Next iflt

        Catch ex As Exception

        End Try
    End Sub

End Class
