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
''' to write Ecospace area averaged results to csv files. This class provides 
''' the framework for writting the file. The actual data is supplied by an implementation 
''' of <see cref="cEcospaceResultsWriterDataSourceBase">cEcospaceResultsWriterDataSourceBase</see> 
''' that supplies the data in a generic format.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcospaceRegionAvgResultsWriter
    Inherits cEcospaceBaseResultsWriter

    Public Const cDATA_NAME As String = "regavg"

#Region " Private classes "

    ''' <summary>
    ''' Types of result objects
    ''' </summary>
    ''' <remarks></remarks>
    Private Enum eDataSourceTypes
        Biomass
        [Catch]
        RegionBiomass
        RegionCatch
    End Enum

#End Region ' Private classes

#Region " Constructor "

    Public Sub New()
    End Sub

#End Region ' Constructor

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.StartWrite"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub StartWrite()
        ' Do not do anything here
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.WriteResults"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub WriteResults(SpaceTimeStepResults As Object)
        ' Take no action. Ecospace results by region are populated only when Ecospace has finished running.
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cEcospaceBaseResultsWriter.EndWrite"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub EndWrite()


        Dim msg As cMessage = Nothing

        Try
            ' Create output dir
            Me.CreateOutputDir()
            ' Write it all
            Me.WriteResult()

            ' ToDo: globalize this method

            ' Notify user
            msg = New cMessage("Ecospace average results have been saved to '" & Me.OutputDirectory & "'",
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
            msg.Hyperlink = Me.OutputDirectory
        Catch ex As Exception
            ' Notify user of error
            msg = New cMessage("Ecospace average results could not be saved to '" & Me.OutputDirectory & "'. " & ex.Message,
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
        End Try
        ' Done
        Me.m_core.Messages.SendMessage(msg)

    End Sub

#End Region ' Public access

#Region " Internals "

    Protected Overrides Function FileExtension() As String
        Return ".csv"
    End Function

    ''' <summary>
    ''' Make sure output directory is defined and available.
    ''' </summary>
    Protected Overrides Function CreateOutputDir() As Boolean

        If Me.m_core.m_EcoSpaceData.UseCoreOutputDir Then
            Me.m_OutputPath = Me.m_core.DefaultOutputPath(eAutosaveTypes.Ecospace)
        Else
            If String.IsNullOrWhiteSpace(Me.EcospaceData.EcospaceAreaOutputDir) Then
                Me.m_OutputPath = Me.m_core.OutputPath
            Else
                Me.m_OutputPath = Path.Combine(Me.m_core.OutputPath, Me.EcospaceData.EcospaceAreaOutputDir)
            End If
        End If

        If (Not cFileUtils.IsDirectoryAvailable(Me.OutputDirectory, True)) Then
            Debug.Assert(False, Me.ToString & ".CreateOutputDir() cannot create directory")
            cLog.Write("cEcospaceRegionResultWriter failed to create directory " & Me.OutputDirectory)
            Return False
        End If

        Return True

    End Function

    ''' <summary>
    ''' Creates a new cResultsDataSourceBase object
    ''' </summary>
    ''' <param name="ResultType"></param>
    ''' <param name="RegionIndex"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function DataSourceFactory(ResultType As eDataSourceTypes, Optional ByVal RegionIndex As Integer = 0) As cEcospaceResultsWriterDataSourceBase
        Dim dataSource As cEcospaceResultsWriterDataSourceBase
        Select Case ResultType
            Case eDataSourceTypes.Biomass
                dataSource = New cBiomassResultsDataSource(Me.m_core, Me.m_core.m_EcoSpaceData)
            Case eDataSourceTypes.Catch
                dataSource = New cCatchResultsDataSource(Me.m_core, Me.m_core.m_EcoSpaceData)
            Case eDataSourceTypes.RegionBiomass
                dataSource = New cRegionBiomassResultsDataSource(Me.m_core, Me.m_core.m_EcoSpaceData)
            Case eDataSourceTypes.RegionCatch
                dataSource = New cRegionCatchResultsDataSource(Me.m_core, Me.m_core.m_EcoSpaceData)
        End Select
        dataSource.Init(RegionIndex)
        Return dataSource
    End Function

#Region " Write Results  "


    Private Function initDataSources() As List(Of cEcospaceResultsWriterDataSourceBase)
        Dim lstDataSources As New List(Of cEcospaceResultsWriterDataSourceBase)

        lstDataSources.Add(Me.DataSourceFactory(eDataSourceTypes.Biomass))
        lstDataSources.Add(Me.DataSourceFactory(eDataSourceTypes.Catch))

        For irgn As Integer = 1 To Me.m_core.nRegions
            lstDataSources.Add(Me.DataSourceFactory(eDataSourceTypes.RegionBiomass, irgn))
            lstDataSources.Add(Me.DataSourceFactory(eDataSourceTypes.RegionCatch, irgn))
        Next
        Return lstDataSources
    End Function

    Private Sub WriteResult()

        Dim sw As StreamWriter = Nothing
        Dim strName As String = ""
        Dim strFile As String = ""
        Dim strDescriptor As String = ""
        Dim sValue As Single = 0

        Dim lstDataSources As List(Of cEcospaceResultsWriterDataSourceBase) = Me.initDataSources()

        For Each ds As cEcospaceResultsWriterDataSourceBase In lstDataSources

            Dim eAvgs As Array
            eAvgs = System.Enum.GetValues(GetType(eEcospaceResultsAverageType))

            For Each AvgType As eEcospaceResultsAverageType In eAvgs

                strFile = Me.getFileName(AvgType, ds)

                Try

                    ' Start writing
                    sw = New StreamWriter(Path.Combine(Me.OutputDirectory, strFile))
                    If Me.m_core.SaveWithFileHeader Then
                        sw.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecospace))
                        sw.WriteLine(cStringUtils.ToCSVField("Data") & "," & cStringUtils.ToCSVField(ds.DataDescriptor))
                        sw.WriteLine(cStringUtils.ToCSVField(ds.AreaDescriptor) + "," & cStringUtils.ToCSVField(ds.nWaterCells * Me.m_core.EcospaceBasemap.CellLength() ^ 2))
                        sw.WriteLine(cStringUtils.ToCSVField("Number of cells") & "," & cStringUtils.ToCSVField(ds.nWaterCells))
                        sw.WriteLine()
                    End If

                    Me.WriteData(sw, ds, AvgType)

                    ' Clean up
                    sw.Flush()
                    sw.Close()
                    sw.Dispose()

                Catch ex As Exception
                    cLog.Write(ex, "Failed to write Ecospace average biomass to file for data " + strDescriptor)
                End Try

            Next
        Next


    End Sub


    Private Function getFileName(AverageType As eEcospaceResultsAverageType, ds As cEcospaceResultsWriterDataSourceBase) As String
        Dim fn As String

        'get the file name from a plugin
        If Me.m_core.PluginManager.EcospaceResultsModelAreaFileName(fn, ds, AverageType) Then
            Return fn
        Else
            'No plugin with the filename
            'So use the default
            Select Case AverageType
                Case eEcospaceResultsAverageType.TimeStep
                    fn = "Ecospace_Average_"
                Case eEcospaceResultsAverageType.Annual
                    fn = "Ecospace_Annual_Average_"
            End Select

            System.Console.WriteLine(ds.FileNameAbbreviation)

            Return cFileUtils.ToValidFileName(fn + ds.FilenameIdentifier + ".csv", False)
        End If

    End Function

    Private Sub WriteData(sw As StreamWriter, dataSource As cEcospaceResultsWriterDataSourceBase, AvgType As eEcospaceResultsAverageType)
        Dim nYrs As Integer = 0
        Dim spaceData As cEcospaceDataStructures = Me.m_core.m_EcoSpaceData

        nYrs = Me.m_core.nEcospaceTimeSteps

        ' Write data header
        If AvgType = eEcospaceResultsAverageType.Annual Then
            sw.Write("Year")
        Else
            sw.Write("TimeStep")
        End If

        For iGroup As Integer = 1 To dataSource.nResults
            sw.Write("," & cStringUtils.ToCSVField(dataSource.FieldName(iGroup)))
        Next iGroup
        sw.WriteLine()

        'Dim nTsYr As Integer = CInt(1.0 / spaceData.TimeStep)
        Dim value() As Single = New Single(dataSource.nResults) {}
        Dim bSave As Boolean
        Dim TSLabel As String, Year As Integer
        Dim nAvg(dataSource.nResults) As Integer

        Year = CInt(Math.Truncate((Me.FirstOutputTimeStep - 1) / Me.m_core.m_EcoSpaceData.nTimeStepsPerYear))

        'Loop over all the time steps
        'If in Annual mode then sum and average the at the end of the year
        For iTime As Integer = Me.FirstOutputTimeStep To Me.m_core.nEcospaceTimeSteps
            For iRslt As Integer = 1 To dataSource.nResults

                If AvgType = eEcospaceResultsAverageType.Annual Then
                    value(iRslt) += dataSource.getResult(iRslt, iTime)
                    nAvg(iRslt) += 1
                    If ((iTime Mod Me.m_core.m_EcoSpaceData.nTimeStepsPerYear) = 0) Then
                        'End of the year
                        'Average the results and
                        'Save to file
                        value(iRslt) /= nAvg(iRslt) 'average over the number of data points/timesteps
                        bSave = True
                        nAvg(iRslt) = 0
                    End If
                Else
                    'Save every time step
                    value(iRslt) = dataSource.getResult(iRslt, iTime)
                    bSave = True
                End If

            Next iRslt

            If bSave Then
                'Grab the label based on the Average Type
                If AvgType = eEcospaceResultsAverageType.Annual Then
                    Year += 1
                    TSLabel = CStr(Year)
                Else
                    TSLabel = CStr(iTime)
                End If

                sw.Write(TSLabel)
                For igrp As Integer = 1 To dataSource.nResults
                    sw.Write(",")
                    sw.Write(cStringUtils.FormatNumber(value(igrp)))
                    bSave = False
                    value(igrp) = 0
                Next igrp

                sw.WriteLine()
            End If

        Next iTime

    End Sub

#End Region ' Internals

#End Region

    Public Overrides ReadOnly Property DisplayName As String
        Get
            Return My.Resources.CoreDefaults.ECOSPACE_WRITER_REGAVG
        End Get
    End Property

End Class
