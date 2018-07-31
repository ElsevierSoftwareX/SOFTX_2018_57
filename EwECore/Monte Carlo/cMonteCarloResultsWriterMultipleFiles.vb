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
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' EwE 6.6 Monte Carlo result writer, using separate files for baseline, 
''' iterations, and Ecosim output.
''' </summary>
''' <remarks>
''' This writer has been added after several complaints about the inaccessible
''' format of <see cref="cMonteCarloResultsWriterOneFile"/>. 
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class cMonteCarloResultsWriterMultipleFiles
    Implements IMonteCarloResultsWriter

#Region " Private vars "

    Private m_MC As cEcosimMonteCarlo
    Private m_core As cCore
    Private m_msgStatus As cMessage = Nothing
    Private m_bSaveError As Boolean = False
    Private m_bCalcExtrasOld As Boolean = False

#End Region ' Private vars

    Public Sub New(ByVal MonteCarlo As cEcosimMonteCarlo, ByVal theCore As cCore)

        Me.m_MC = MonteCarlo
        Me.m_core = theCore

    End Sub

    Public Sub Init() Implements IMonteCarloResultsWriter.Init

        ' Reset error flag
        Me.m_bSaveError = False

        If (Not Me.IsSaving) Then Return

        Me.m_msgStatus = New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.MONTECARLO_RESULTS_SAVED_SUCCESS, Me.DataDir),
                                          eMessageType.DataExport, eCoreComponentType.EcoSimMonteCarlo, eMessageImportance.Information)
        Me.m_msgStatus.Hyperlink = Me.DataDir
        Me.m_bCalcExtrasOld = Me.m_core.m_EcoSimData.bAlwaysCalcTLc

        If cFileUtils.IsDirectoryAvailable(Me.DataDir, True) Then
            Me.Save(cCore.NULL_VALUE)
            Me.m_core.m_EcoSimData.bAlwaysCalcTLc = True
        Else
            Me.ReportSaveError("Directory unavailable")
        End If

    End Sub

    ''' <summary>
    ''' Save data to file.
    ''' </summary>
    Public Sub Save(ByVal iTrial As Integer) Implements IMonteCarloResultsWriter.Save

        If Not Me.IsSaving() Then Return

        Dim strPathInput As String = Path.Combine(Me.DataDir, "mc_input")
        Dim strPathOutput As String = Path.Combine(Me.DataDir, String.Format("mc_output_trial{0:D4}", iTrial))

        Try
            If (iTrial <= 0) Then

                For Each par As eMCParams In [Enum].GetValues(GetType(eMCParams))
                    If (Me.m_MC.IsEnabled(par)) Then
                        Dim sw As StreamWriter = Nothing
                        Try
                            cFileUtils.IsDirectoryAvailable(strPathInput, True)
                            sw = New StreamWriter(Path.Combine(strPathInput, String.Format("mc_baseline_{0}.csv", par.ToString())))
                            Me.WriteHeader(sw, iTrial)
                            Me.WriteBaselineBody(sw, par)

                            sw.Flush()
                            sw.Close()
                            sw.Dispose()
                        Catch ex As Exception
                            Me.ReportSaveError(ex.Message)
                        End Try

                    End If
                Next
            ElseIf (iTrial < Integer.MaxValue) Then
                For Each par As eMCParams In [Enum].GetValues(GetType(eMCParams))
                    If (Me.m_MC.IsEnabled(par)) Then
                        Dim sw As StreamWriter = Nothing
                        Try
                            cFileUtils.IsDirectoryAvailable(strPathInput, True)
                            sw = New StreamWriter(Path.Combine(strPathInput, String.Format("mc_trial{0:D5}_{1}.csv", iTrial, par.ToString())))
                            Me.WriteHeader(sw, iTrial)
                            Me.WriteTrialBody(sw, par)

                            sw.Flush()
                            sw.Close()
                            sw.Dispose()
                        Catch ex As Exception
                            Me.ReportSaveError(ex.Message)
                        End Try

                    End If
                Next

                ' Write Ecosim output
                Dim writerSim As New Ecosim.cEcosimResultWriter(Me.Core)

                If Not writerSim.WriteResultsDirect(strPathOutput, Nothing, TriState.UseDefault, True) Then
                    Me.ReportSaveError("Unable to save Ecosim results to " & strPathOutput)
                End If

            Else
                ' ToDo: write Best Fit summary
            End If

        Catch ex As Exception
            ' ToDo: log error
            cLog.Write(ex, "cMonteCarloResultsWriterMultipleFiles.Save(" & iTrial & ")")
            Debug.Assert(False)
        End Try

    End Sub

    Public Sub Finish() Implements IMonteCarloResultsWriter.Finish

        ' Write save notification message
        If (Me.m_msgStatus IsNot Nothing) Then
            Me.m_core.Messages.SendMessage(Me.m_msgStatus)
            Me.m_msgStatus = Nothing
        End If
        Me.m_bSaveError = False

        Me.m_core.m_EcoSimData.bAlwaysCalcTLc = Me.m_bCalcExtrasOld

    End Sub

    Public Function DataName() As String Implements IMonteCarloResultsWriter.DataName
        Return "mcMultFile"
    End Function

    Public Function DsiplayName() As String Implements IMonteCarloResultsWriter.DisplayName
        Return My.Resources.CoreDefaults.MONTECARLO_WRITER_MULTIPLE
    End Function

#Region " Internals "

    Private Function DataDir() As String
        Return Me.Core.DefaultOutputPath(eAutosaveTypes.MonteCarlo)
    End Function

    Private ReadOnly Property ModelName() As String
        Get
            Return Me.Core.DataSource.FileName
        End Get
    End Property

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

    Private Sub WriteHeader(sw As StreamWriter, iTrial As Integer)
        Try
            If Me.m_core.SaveWithFileHeader Then
                sw.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.MonteCarlo))
                sw.WriteLine(cStringUtils.ToCSVField("Num. groups") & "," & Me.m_core.nGroups)
                sw.WriteLine(cStringUtils.ToCSVField("Num. trials") & "," & Me.m_MC.Ntrials)
                sw.WriteLine(cStringUtils.ToCSVField("Trial") & "," & If(iTrial <= 0, "baseline", CStr(iTrial)))
                sw.WriteLine(cStringUtils.ToCSVField("SS") & "," & If(iTrial <= 0, cStringUtils.ToCSVField(Me.MC.SSorg), cStringUtils.ToCSVField(Me.MC.SSCurrent)))
            End If
        Catch ex As Exception
            Me.ReportSaveError(ex.Message)
            Debug.Assert(False, Me.ToString & ".WriteHeader() Exception: " & ex.Message)
        End Try

    End Sub

    Private Sub WriteBaselineBody(sw As StreamWriter, par As eMCParams)

        Select Case par
            Case eMCParams.Landings, eMCParams.Discards
                sw.WriteLine("group,fleet,{0}_cv,{0}_lower,{0}_upper", par.ToString.ToLower)
                For iGroup As Integer = 1 To Core.nGroups
                    Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)
                    For iFleet As Integer = 1 To Core.nFleets
                        Dim fleet As cEcopathFleetInput = Me.Core.EcopathFleetInputs(iFleet)
                        If (fleet.Landings(iGroup) > 0) Or (fleet.Discards(iGroup) > 0) Then
                            sw.Write("{0},{1}", iGroup, iFleet)
                            Select Case par
                                Case eMCParams.Landings
                                    sw.WriteLine(",{0},{1},{2}", Me.MC.CVparLanding(iFleet, iGroup), Me.MC.ParLimitLanding(0, iFleet, iGroup), Me.MC.ParLimitLanding(1, iFleet, iGroup))
                                Case eMCParams.Discards
                                    sw.WriteLine(",{0},{1},{2}", Me.MC.CVparDiscard(iFleet, iGroup), Me.MC.ParLimitDiscard(0, iFleet, iGroup), Me.MC.ParLimitDiscard(1, iFleet, iGroup))
                            End Select
                        End If
                    Next iFleet
                Next iGroup

            Case eMCParams.Diets
                sw.WriteLine("predator,{0}_multiplier", par.ToString.ToLower)
                For iGroup As Integer = 1 To Core.nGroups
                    sw.Write("{0},{1}", iGroup, Me.MC.CVpar(par, iGroup))
                Next

            Case Else
                sw.WriteLine("group,{0}_cv,{0}_lower,{0}_upper", par.ToString.ToLower)
                For iGroup As Integer = 1 To Core.nGroups
                    sw.Write("{0}", iGroup)
                    sw.WriteLine(",{0},{1},{2}", Me.MC.CVpar(par, iGroup), Me.MC.ParLimit(0, par, iGroup), Me.MC.ParLimit(1, par, iGroup))
                Next
        End Select

    End Sub

    Private Sub WriteTrialBody(sw As StreamWriter, par As eMCParams)

        Select Case par
            Case eMCParams.Landings, eMCParams.Discards
                sw.WriteLine("group,fleet,{0}", par.ToString.ToLower)
                For iGroup As Integer = 1 To Core.nGroups
                    Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iGroup)
                    For iFleet As Integer = 1 To Core.nFleets
                        Dim fleet As cEcopathFleetInput = Me.Core.EcopathFleetInputs(iFleet)
                        If (fleet.Landings(iGroup) > 0) Or (fleet.Discards(iGroup) > 0) Then
                            sw.Write("{0},{1}", group.Index, fleet.Index, cStringUtils.ToCSVField(fleet.Name))
                            Dim val As Single = 0.0
                            Select Case par
                                Case eMCParams.Landings
                                    val = Me.Core.m_EcoPathData.Landing(iFleet, iGroup)
                                Case eMCParams.Discards
                                    val = Me.Core.m_EcoPathData.Discard(iFleet, iGroup)
                            End Select
                            sw.Write(",{0}", If(val > 0, cStringUtils.ToCSVField(val), ""))
                        End If
                    Next iFleet
                Next iGroup

            Case eMCParams.Diets
                sw.Write("predator")
                For iPrey As Integer = 1 To Me.Core.nGroups
                    Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(iPrey)
                    sw.Write("," & group.Index)
                Next
                sw.WriteLine()
                For iPred As Integer = 1 To Core.nGroups
                    sw.Write("{0}", iPred)
                    For iPrey As Integer = 1 To Me.Core.nGroups
                        Dim val As Single = Me.m_core.m_EcoPathData.DC(iPred, iPrey)
                        sw.Write(",{0}", If(val > 0, cStringUtils.ToCSVField(val), ""))
                    Next
                    sw.WriteLine()
                Next

            Case Else
                sw.WriteLine("group,{0}", par.ToString)
                For iGroup As Integer = 1 To Core.nGroups
                    Dim group As cEcoPathGroupOutput = Me.Core.EcoPathGroupOutputs(iGroup)
                    sw.Write("{0}", iGroup)
                    Dim val As Single = 0
                    Select Case par
                        Case eMCParams.Biomass : val = Me.Core.m_EcoPathData.B(iGroup)
                        Case eMCParams.BA : val = Me.Core.m_EcoPathData.BA(iGroup)
                        Case eMCParams.BaBi : val = Me.Core.m_EcoPathData.BaBi(iGroup)
                        Case eMCParams.PB : val = Me.Core.m_EcoPathData.PB(iGroup)
                        Case eMCParams.QB : val = Me.Core.m_EcoPathData.QB(iGroup)
                        Case eMCParams.EE : val = Me.Core.m_EcoPathData.EE(iGroup)
                        Case Else
                            Debug.Assert(False, "Yo! Variable not supported yet!")
                    End Select
                    sw.WriteLine(",{0}", If(val > 0, cStringUtils.ToCSVField(val), ""))
                Next
        End Select

    End Sub

    Private Sub ReportSaveError(strMessage As String)

        If (Me.m_bSaveError = False) Then
            Me.m_msgStatus = New cMessage(String.Format(My.Resources.CoreMessages.MONTECARLO_RESULTS_SAVED_ERROR, Me.DataDir),
                                          eMessageType.ErrorEncountered, eCoreComponentType.EcoSimMonteCarlo, eMessageImportance.Warning, eDataTypes.MonteCarlo)
        End If

        Dim vs As New cVariableStatus(eStatusFlags.ErrorEncountered, strMessage, eVarNameFlags.NotSet, eDataTypes.Auxillary, eCoreComponentType.EcoSimMonteCarlo, 0)
        Me.m_msgStatus.AddVariable(vs)
        Me.m_bSaveError = True

    End Sub

#End Region ' Internals

End Class
