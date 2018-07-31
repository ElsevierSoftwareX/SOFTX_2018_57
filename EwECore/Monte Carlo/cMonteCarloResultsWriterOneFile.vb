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

''' <summary>
''' Pre-EwE6.6 result writer, writing all MC output to one single CSV file.
''' </summary>
Public Class cMonteCarloResultsWriterOneFile
    Implements IMonteCarloResultsWriter

    Private m_MC As cEcosimMonteCarlo
    Private m_core As cCore
    Private m_msgStatus As cMessage = Nothing

    Public Sub New(ByVal MonteCarlo As cEcosimMonteCarlo, ByVal theCore As cCore)

        Me.m_MC = MonteCarlo
        Me.m_core = theCore

    End Sub

#Region " Public access "

    Public Sub Init() Implements IMonteCarloResultsWriter.Init

        Me.m_bSaveError = False
        If Not Me.IsSaving Then Return

        Dim strFile As String = Me.OutputFilename()

        Try

            If cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFile), True) Then

                If File.Exists(strFile) Then
                    File.Delete(strFile)
                End If

                Me.m_msgStatus = New cMessage(String.Format(My.Resources.CoreMessages.MONTECARLO_RESULTS_SAVED_SUCCESS, strFile),
                                              eMessageType.DataExport, eCoreComponentType.EcoSimMonteCarlo, eMessageImportance.Information)
                Me.m_msgStatus.Hyperlink = Path.GetDirectoryName(strFile)

                Me.WriteHeader()

                'save the baseline data
                Me.Save(0)

            End If

        Catch ex As Exception
            Me.m_msgStatus = New cMessage(String.Format(My.Resources.CoreMessages.MONTECARLO_RESULTS_SAVED_SUCCESS, strFile, ex.Message),
                                          eMessageType.ErrorEncountered, eCoreComponentType.EcoSimMonteCarlo, eMessageImportance.Warning, eDataTypes.MonteCarlo)
            Me.m_bSaveError = True
            cLog.Write(ex)
        End Try
    End Sub

    Public Sub Finish() Implements IMonteCarloResultsWriter.Finish

        ' Write save notification message
        If (Me.m_msgStatus IsNot Nothing) Then
            Me.m_core.Messages.SendMessage(Me.m_msgStatus)
            Me.m_msgStatus = Nothing
        End If
        Me.m_bSaveError = False

    End Sub

    ''' <summary>
    ''' Save both iteration and baseline data to file
    ''' </summary>
    Public Sub Save(ByVal iTrial As Integer) Implements IMonteCarloResultsWriter.Save

        Dim strm As StreamWriter
        Dim isBaseLineData As Boolean = (iTrial <= 0)

        Try

            ' ToDo: pivot Ecosim data to show time as rows, data as columns

            If Not Me.IsSaving() Then Return

            strm = New StreamWriter(Me.OutputFilename, True)

            'empty line at the start of a new data block
            strm.WriteLine("")

            If isBaseLineData Then
                strm.WriteLine(Me.getParameterVariance)
            End If

            If isBaseLineData Then
                strm.WriteLine(cStringUtils.ToCSVField("Baseline data"))
            Else
                strm.WriteLine(cStringUtils.ToCSVField("Trial number") & "," & cStringUtils.ToCSVField(Me.MC.nTrialIterations))
            End If

            strm.WriteLine(cStringUtils.ToCSVField("Original SS") & "," & cStringUtils.ToCSVField(Me.MC.SSorg))
            strm.WriteLine(cStringUtils.ToCSVField("Current SS") & "," & cStringUtils.ToCSVField(Me.MC.SSCurrent))
            strm.WriteLine(cStringUtils.ToCSVField("Ecopath parameters"))

            strm.WriteLine(cStringUtils.ToCSVField("Group Name") & "," & Me.ToCSVString(Core.m_EcoPathData.GroupName))

            strm.WriteLine("Biomass," & Me.ToCSVString(Core.m_EcoPathData.B))
            strm.WriteLine("PB," & Me.ToCSVString(Core.m_EcoPathData.PB))
            strm.WriteLine("EE," & Me.ToCSVString(Core.m_EcoPathData.EE))
            strm.WriteLine("QB," & Me.ToCSVString(Core.m_EcoPathData.QB))
            strm.WriteLine("BA," & Me.ToCSVString(Core.m_EcoPathData.BA))

            strm.WriteLine("Landings")
            For iflt As Integer = 1 To Core.nFleets
                Dim landings(Core.nGroups) As Single
                For igrp As Integer = 1 To Core.nGroups
                    landings(igrp) += Core.m_EcoPathData.Landing(iflt, igrp)
                Next
                strm.WriteLine(cStringUtils.ToCSVField(Core.m_EcoPathData.FleetName(iflt) & "," & Me.ToCSVString(landings)))
            Next

            strm.WriteLine("Discards")
            For iflt As Integer = 1 To Core.nFleets
                Dim discards(Core.nGroups) As Single
                For igrp As Integer = 1 To Core.nGroups
                    discards(igrp) += Core.m_EcoPathData.Discard(iflt, igrp)
                Next
                strm.WriteLine(cStringUtils.ToCSVField(Core.m_EcoPathData.FleetName(iflt) & "," & Me.ToCSVString(discards)))
            Next

            strm.WriteLine("Diets")
            For iPrey As Integer = 1 To Core.nGroups
                strm.Write("," & cStringUtils.ToCSVField("Prey " & iPrey.ToString))
            Next
            For iPred As Integer = 1 To Core.nGroups
                strm.Write("Pred " & iPred.ToString)
                For iPrey As Integer = 1 To Core.nGroups
                    strm.Write("," & Me.ToCSVString(Core.m_EcoPathData.DC, iPred))
                Next
            Next
            strm.WriteLine(cStringUtils.ToCSVField("Ecosim biomass"))
            strm.Write(",")
            For it As Integer = 1 To Me.Core.m_EcoSimData.NTimes
                strm.Write(",")
                strm.Write(cStringUtils.ToCSVField(it))
            Next
            strm.WriteLine()
            For igrp As Integer = 1 To Me.Core.m_EcoPathData.NumGroups
                strm.Write(cStringUtils.ToCSVField(igrp) & "," & cStringUtils.ToCSVField(Core.m_EcoPathData.GroupName(igrp)) & ",")
                strm.WriteLine(Me.ToCSVString(Me.Core.m_EcoSimData.ResultsOverTime, cEcosimDatastructures.eEcosimResults.Biomass, igrp))
            Next
            strm.WriteLine(cStringUtils.ToCSVField("Ecosim catch"))
            strm.Write(",")
            For it As Integer = 1 To Me.Core.m_EcoSimData.NTimes
                strm.Write(",")
                strm.Write(cStringUtils.ToCSVField(it))
            Next
            strm.WriteLine()
            For igrp As Integer = 1 To Me.Core.m_EcoPathData.NumGroups
                strm.Write(cStringUtils.ToCSVField(igrp) & "," & cStringUtils.ToCSVField(Core.m_EcoPathData.GroupName(igrp)) & ",")
                strm.WriteLine(Me.ToCSVString(Me.Core.m_EcoSimData.ResultsOverTime, cEcosimDatastructures.eEcosimResults.Yield, igrp))
            Next
            ' ToDo: Export Landings, Discards, DiscardMort, DiscardSurv ;)
            strm.Close()
            strm = Nothing

        Catch ex As Exception
            System.Console.WriteLine(Me.ToString & ".SaveIteration(...) Exception: " & ex.Message)
        End Try

        'Make sure the stream did not get left open somehow
        Try
            If strm IsNot Nothing Then
                strm.Flush()
                strm.Close()
            End If
        Catch ex As Exception

        End Try

    End Sub

    Public Function DataName() As String Implements IMonteCarloResultsWriter.DataName
        Return "mcOneFile"
    End Function

    Public Function DsiplayName() As String Implements IMonteCarloResultsWriter.DisplayName
        Return My.Resources.CoreDefaults.MONTECARLO_WRITER_ONEFILE
    End Function

#End Region ' Public access

#Region " Internals "

    Private ReadOnly Property OutputFilename() As String
        Get
            Return Path.Combine(Me.DataDir, "MonteCarloTrials.csv")
        End Get
    End Property

    Private Function DataDir() As String
        Return Me.Core.DefaultOutputPath(eAutosaveTypes.MonteCarlo)
    End Function

    Private ReadOnly Property ModelName() As String
        Get
            Return Me.Core.DataSource.FileName
        End Get
    End Property

    Private m_bSaveError As Boolean = False

    Private Function IsSaving() As Boolean
        Return Me.MC.SaveOutput And Not Me.m_bSaveError
    End Function

    Private Function ScenarioName() As String
        Return Me.m_core.EcosimScenarios(Me.m_core.ActiveEcosimScenarioIndex).Name
    End Function

    Private ReadOnly Property MC() As cEcosimMonteCarlo
        Get
            Return Me.m_MC
        End Get
    End Property

    Private ReadOnly Property Core() As cCore
        Get
            Return Me.m_core
        End Get
    End Property

    Private Sub WriteHeader()
        Try
            If Not Me.IsSaving() Then Return

            Dim strm As New StreamWriter(Me.OutputFilename)

            If Me.m_core.SaveWithFileHeader Then
                strm.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.MonteCarlo))
            End If
            strm.Write(cStringUtils.ToCSVField("Num. groups") & "," & Me.m_core.nGroups)
            strm.Write(cStringUtils.ToCSVField("Num. trials") & "," & Me.m_MC.Ntrials)
            strm.Close()

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".WriteHeader() Exception: " & ex.Message)
        End Try

    End Sub

#End Region ' Internals

#Region " Save helper methods "

    Private Function getParameterVariance() As String
        Dim buff As New StringBuilder

        'Group name
        buff.AppendLine(cStringUtils.ToCSVField("Group Name") & "," & Me.ToCSVString(Core.m_EcoPathData.GroupName))

        'CV's
        buff.AppendLine(cStringUtils.ToCSVField("Biomass CV") & "," & Me.ToCSVString(Me.MC.CVpar, eMCParams.Biomass))
        buff.AppendLine(cStringUtils.ToCSVField("Biomass lower limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 0, eMCParams.Biomass))
        buff.AppendLine(cStringUtils.ToCSVField("Biomass upper limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 1, eMCParams.Biomass))

        buff.AppendLine(cStringUtils.ToCSVField("P/B CV") & "," & Me.ToCSVString(Me.MC.CVpar, eMCParams.PB))
        buff.AppendLine(cStringUtils.ToCSVField("P/B lower limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 0, eMCParams.PB))
        buff.AppendLine(cStringUtils.ToCSVField("P/B upper limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 1, eMCParams.PB))

        buff.AppendLine(cStringUtils.ToCSVField("QB CV") & "," & Me.ToCSVString(Me.MC.CVpar, eMCParams.QB))
        buff.AppendLine(cStringUtils.ToCSVField("QB lower limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 0, eMCParams.QB))
        buff.AppendLine(cStringUtils.ToCSVField("QB upper limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 1, eMCParams.QB))

        buff.AppendLine(cStringUtils.ToCSVField("EE CV") & "," & Me.ToCSVString(Me.MC.CVpar, eMCParams.EE))
        buff.AppendLine(cStringUtils.ToCSVField("EE lower limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 0, eMCParams.EE))
        buff.AppendLine(cStringUtils.ToCSVField("EE upper limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 1, eMCParams.EE))

        buff.AppendLine(cStringUtils.ToCSVField("BA CV") & "," & Me.ToCSVString(Me.MC.CVpar, eMCParams.BA))
        buff.AppendLine(cStringUtils.ToCSVField("BA lower limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 0, eMCParams.BA))
        buff.AppendLine(cStringUtils.ToCSVField("BA upper limit") & "," & Me.ToCSVString(Me.MC.ParLimit, 1, eMCParams.BA))

        'MP Apr 2016 Adding Diets
        buff.AppendLine(cStringUtils.ToCSVField("Diet Multiplier") & "," & Me.ToCSVString(Me.MC.CVpar, eMCParams.Diets))

        Return buff.ToString

    End Function


    Private Function ToCSVString(ByVal Values(,,) As Single, ByVal FirstFixedIndex As Integer, ByVal SecondFixedIndex As Integer) As String

        Dim buff As New StringBuilder()
        Try
            For igrp As Integer = 1 To Core.m_EcoPathData.NumGroups
                If igrp > 1 Then buff.Append(",")
                buff.Append(cStringUtils.FormatSingle(Values(FirstFixedIndex, SecondFixedIndex, igrp)))
            Next
        Catch ex As Exception
            Debug.Assert(False, "ToCSVString() Exception: " & ex.Message)
        End Try
        Return buff.ToString()

    End Function


    Private Function ToCSVString(ByVal Values(,) As Single, ByVal FixedIndex As Integer) As String

        Dim buff As New StringBuilder()
        Try
            For igrp As Integer = 1 To Core.m_EcoPathData.NumGroups
                If igrp > 1 Then buff.Append(",")
                buff.Append(cStringUtils.FormatSingle(Values(FixedIndex, igrp)))
            Next
        Catch ex As Exception
            Debug.Assert(False, "ToCSVString() Exception: " & ex.Message)
        End Try
        Return buff.ToString()

    End Function


    Private Function ToCSVString(ByVal Values(,,) As Single, ByVal Variable As cEcosimDatastructures.eEcosimResults, ByVal iGroup As Integer) As String

        Dim buff As New StringBuilder()
        Try
            For it As Integer = 1 To Me.Core.m_EcoSimData.NTimes
                If it > 1 Then buff.Append(",")
                buff.Append(cStringUtils.FormatSingle(Values(Variable, iGroup, it)))
            Next
        Catch ex As Exception
            Debug.Assert(False, "ToCSVString() Exception: " & ex.Message)
        End Try
        Return buff.ToString()

    End Function


    Private Function ToCSVString(ByVal Values() As String) As String

        Dim buff As New StringBuilder()
        Try
            For igrp As Integer = 1 To Core.m_EcoPathData.NumGroups
                If igrp > 1 Then buff.Append(",")
                buff.Append(Values(igrp))
            Next
        Catch ex As Exception
            Debug.Assert(False, "ToCSVString() Exception: " & ex.Message)
        End Try
        Return buff.ToString

    End Function


    Private Function ToCSVString(ByVal values() As Single) As String

        Dim buff As New StringBuilder()
        Try
            For igrp As Integer = 1 To Core.m_EcoPathData.NumGroups
                If igrp > 1 Then buff.Append(",")
                buff.Append(cStringUtils.FormatSingle(values(igrp)))
            Next
        Catch ex As Exception
            Debug.Assert(False, "ToCSVString() Exception: " & ex.Message)
        End Try
        Return buff.ToString()

    End Function

#End Region ' Save helper methods

End Class
