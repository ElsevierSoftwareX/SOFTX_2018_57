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
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Ecosim

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Helper class to write Ecosim results to a .csv file.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cEcosimResultWriter

#Region " Private vars "

        Private m_core As cCore = Nothing

        Public Enum eResultTypes As Integer
            Biomass = 0
            ConsumptionBiomass
            PredationMortality
            Mortality
            FeedingTime
            Prey
            [Catch]
            Value
            AvgWeightOrProdCons
            TL
            TLC
            KemptonsQ
            ShannonDiversity
            FIB
            TotalCatch
            CatchFleetGroup
            MortFleetGroup
            ValueFleetGroup
            DiscardFleetGroup
            DiscardMortalityFleetGroup
            DiscardSurvivalFleetGroup
            Landings
        End Enum

#End Region ' Private vars

#Region " Public interfaces "

        Public Sub New(ByVal core As cCore)
            Me.m_core = core
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Save all available Ecosim results to a .csv file.
        ''' </summary>
        ''' <param name="strPath">The path to write to. If not specified, output is
        ''' written to <see cref="cCore.OutputPath">the core output path</see>.</param>
        ''' <param name="results">The results to write, or nothing to write all results.</param>
        ''' <param name="tsMonthly">Flag stating how values are aggragated. Possible
        ''' values are:
        ''' <list type="bullet">
        ''' <item><term><see cref="TriState.[True]"/></term><description>Values are only written as monthly values.</description></item>
        ''' <item><term><see cref="TriState.[False]"/></term><description>Values are only written as annual values.</description></item>
        ''' <item><term><see cref="TriState.UseDefault"/></term><description>Values are written as both annual and monthly values.</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="bQuiet">Flag stating if messages must be suppressed.</param>
        ''' <returns>True if saved successfully.</returns>
        ''' -----------------------------------------------------------------------
        Public Function WriteResults(Optional ByVal strPath As String = "",
                                     Optional ByVal results As eResultTypes() = Nothing,
                                     Optional ByVal tsMonthly As TriState = TriState.UseDefault,
                                     Optional ByVal bQuiet As Boolean = False) As Boolean

            If (Not Me.m_core.StateMonitor.HasEcosimRan) Then Return False
            Return Me.WriteResultsDirect(strPath, results, tsMonthly, bQuiet)

        End Function


        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Save all available Ecosim results to a .csv file without checking if
        ''' Ecosim has ran. Dangerous!
        ''' </summary>
        ''' <param name="strPath">The path to write to. If not specified, output is
        ''' written to <see cref="cCore.OutputPath">the core output path</see>.</param>
        ''' <param name="results">The results to write, or nothing to write all results.</param>
        ''' <param name="tsMonthly">Flag stating how values are aggragated. Possible
        ''' values are:
        ''' <list type="bullet">
        ''' <item><term><see cref="TriState.[True]"/></term><description>Values are only written as monthly values.</description></item>
        ''' <item><term><see cref="TriState.[False]"/></term><description>Values are only written as annual values.</description></item>
        ''' <item><term><see cref="TriState.UseDefault"/></term><description>Values are written as both annual and monthly values.</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="bQuiet">Flag stating if messages must be suppressed.</param>
        ''' <returns>True if saved successfully.</returns>
        ''' -----------------------------------------------------------------------
        Friend Function WriteResultsDirect(ByVal strPath As String,
                                           ByVal results As eResultTypes(),
                                           ByVal tsMonthly As TriState,
                                           ByVal bQuiet As Boolean) As Boolean

            Dim msg As cMessage = Nothing
            Dim bSucces As Boolean = True

            If String.IsNullOrEmpty(strPath) Then
                strPath = Me.m_core.DefaultOutputPath(eAutosaveTypes.EcosimResults)
            End If

            ' Try to make sure that the output path is there
            If Not cFileUtils.IsDirectoryAvailable(strPath, True) Then
                msg = New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSIM_SAVE_FAILED, strPath, My.Resources.CoreMessages.OUTPUT_DIRECTORY_MISSING),
                                   eMessageType.DataExport, eCoreComponentType.EcoSim, eMessageImportance.Information)
                If (Not bQuiet) Then
                    Me.m_core.Messages.SendMessage(msg)
                Else
                    cLog.Write(msg)
                End If
                Return False
            End If

            For Each outputtype As cEcosimResultWriter.eResultTypes In [Enum].GetValues(GetType(eResultTypes))
                If Me.ShouldWriteResult(results, outputtype) Then
                    Try
                        If (tsMonthly <> TriState.False) Then bSucces = bSucces And Me.WriteResults(strPath, outputtype, True)
                        If (tsMonthly <> TriState.True) Then bSucces = bSucces And Me.WriteResults(strPath, outputtype, False)

                        If Not bSucces Then
                            msg = New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSIM_RESULTS_SAVE_FAILED, strPath, outputtype.ToString),
                                               eMessageType.DataExport, eCoreComponentType.EcoSim, eMessageImportance.Warning)
                            If (Not bQuiet) Then
                                Me.m_core.Messages.SendMessage(msg)
                            Else
                                cLog.Write(msg)
                            End If
                        End If

                    Catch ex As Exception
                        bSucces = False
                        cLog.Write(ex, "cEcosimResultWriter::WriteResults " & outputtype.ToString())
                    End Try
                End If
            Next

            If bSucces Then
                msg = New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.ECOSIM_RESULTS_SAVE_SUCCESS, strPath),
                                   eMessageType.DataExport, eCoreComponentType.EcoSim, eMessageImportance.Information)
                ' Provide hyperlink to the directory with the files
                msg.Hyperlink = strPath
                If (Not bQuiet) Then
                    Me.m_core.Messages.SendMessage(msg)
                Else
                    cLog.Write(msg)
                End If
            End If
            Return bSucces

        End Function

#End Region ' Public interfaces

#Region " Internal helpers "

        Private Function ShouldWriteResult(ByVal aResults As eResultTypes(), ByVal result As eResultTypes) As Boolean

            If (aResults Is Nothing) Then Return True
            If (aResults.Length = 0) Then Return True
            Return (Array.IndexOf(aResults, result) > -1)

        End Function

        Private Function WriteResults(ByVal strPath As String,
                                      ByVal resulttype As eResultTypes,
                                      ByVal bSaveAnnual As Boolean) As Boolean

            Dim strModelDetails As String = Me.GetModelDetails()
            Dim strDataDetails As String = ""
            Dim astrGroupNames As String = Me.GetAllGroupIdentifiers()
            Dim group As cEcoPathGroupInput = Nothing
            Dim bSuccess As Boolean = True

            Select Case resulttype

                Case eResultTypes.Biomass,
                     eResultTypes.Mortality,
                     eResultTypes.Catch,
                     eResultTypes.ConsumptionBiomass,
                     eResultTypes.FeedingTime,
                     eResultTypes.AvgWeightOrProdCons,
                     eResultTypes.TL,
                     eResultTypes.Value

                    Dim data(m_core.nGroups, m_core.nEcosimTimeSteps) As Single
                    For i As Integer = 1 To m_core.nGroups
                        group = m_core.EcoPathGroupInputs(i)
                        For j As Integer = 1 To m_core.nEcosimTimeSteps
                            Select Case resulttype
                                Case eResultTypes.Biomass
                                    data(i, j) = m_core.m_EcoSimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Biomass, i, j)
                                Case eResultTypes.Mortality
                                    data(i, j) = m_core.m_EcoSimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.TotalMort, i, j)
                                Case eResultTypes.Catch
                                    data(i, j) = m_core.m_EcoSimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.Yield, i, j)
                                Case eResultTypes.ConsumptionBiomass
                                    data(i, j) = m_core.m_EcoSimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.ConsumpBiomass, i, j)
                                Case eResultTypes.FeedingTime
                                    data(i, j) = m_core.m_EcoSimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.FeedingTime, i, j)
                                Case eResultTypes.AvgWeightOrProdCons
                                    If group.IsMultiStanza Then
                                        data(i, j) = m_core.m_EcoSimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.AvgWeight, i, j)
                                    Else
                                        data(i, j) = m_core.m_EcoSimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.ProdConsump, i, j)
                                    End If
                                Case eResultTypes.TL
                                    data(i, j) = m_core.m_EcoSimData.ResultsOverTime(cEcosimDatastructures.eEcosimResults.TL, i, j)
                                Case eResultTypes.Value
                                    ' For all fleets
                                    data(i, j) = m_core.m_EcoSimData.ResultsSumValueByGroupGear(i, 0, j)
                            End Select
                        Next

                    Next
                    strDataDetails = "Data," & resulttype.ToString
                    bSuccess = Me.SaveDataToFile(Me.GetOutputFileName(strPath, bSaveAnnual, resulttype),
                                                 bSaveAnnual, data,
                                                 strModelDetails, strDataDetails, astrGroupNames)

                Case eResultTypes.PredationMortality

                    For iGroup As Integer = 1 To Me.m_core.nGroups

                        group = m_core.EcoPathGroupInputs(iGroup)

                        Dim iNumPred As Integer = 0
                        Dim predNames As New StringBuilder()

                        For i As Integer = 1 To m_core.nLivingGroups
                            If group.IsPred(i) Then
                                iNumPred += 1
                                predNames.Append(cStringUtils.ToCSVField(Me.m_core.EcoSimGroupOutputs(i).Name))
                                predNames.Append(",")
                            End If
                        Next

                        If (predNames.Length > 0) Then

                            Dim predData(iNumPred, m_core.nEcosimTimeSteps) As Single
                            iNumPred = 1

                            For i As Integer = 1 To m_core.nLivingGroups
                                If group.IsPred(i) Then
                                    For j As Integer = 1 To m_core.nEcosimTimeSteps
                                        predData(iNumPred, j) = m_core.m_EcoSimData.PredPreyResultsOverTime(cEcosimDatastructures.eEcosimPreyPredResults.Pred, iGroup, i, j)
                                    Next
                                    iNumPred += 1
                                End If
                            Next
                            strDataDetails = "Data," & cStringUtils.ToCSVField(resulttype.ToString & " of " & group.Name)

                            bSuccess = bSuccess And Me.SaveDataToFile(Me.GetOutputFileName(strPath, bSaveAnnual, resulttype, group.Name),
                                                                      bSaveAnnual, predData,
                                                                      strModelDetails, strDataDetails, predNames.ToString)
                        End If
                    Next

                Case eResultTypes.Prey

                    ' For all predators
                    For iGroup As Integer = 1 To Me.m_core.nLivingGroups

                        group = m_core.EcoPathGroupInputs(iGroup)

                        Dim iNumPrey As Integer = 0
                        Dim preyNames As New StringBuilder

                        For i As Integer = 1 To m_core.nGroups
                            If group.IsPrey(i) Then
                                iNumPrey += 1
                                preyNames.Append(cStringUtils.ToCSVField(Me.m_core.EcoSimGroupOutputs(i).Name))
                                preyNames.Append(",")
                            End If
                        Next

                        If (preyNames.Length > 0) Then

                            Dim preyData(iNumPrey, m_core.nEcosimTimeSteps) As Single
                            iNumPrey = 0

                            For i As Integer = 1 To m_core.nGroups
                                If group.IsPrey(i) Then
                                    iNumPrey += 1
                                    For j As Integer = 1 To m_core.nEcosimTimeSteps
                                        preyData(iNumPrey, j) = m_core.m_EcoSimData.PredPreyResultsOverTime(cEcosimDatastructures.eEcosimPreyPredResults.Prey, iGroup, i, j)
                                    Next
                                End If
                            Next

                            strDataDetails = "Data," & cStringUtils.ToCSVField(resulttype.ToString & " of " & group.Name)
                            bSuccess = bSuccess And Me.SaveDataToFile(Me.GetOutputFileName(strPath, bSaveAnnual, resulttype, group.Name),
                                                  bSaveAnnual, preyData,
                                                  strModelDetails, strDataDetails, preyNames.ToString)
                        End If
                    Next

                Case eResultTypes.KemptonsQ,
                     eResultTypes.ShannonDiversity,
                    eResultTypes.TLC,
                    eResultTypes.FIB,
                    eResultTypes.TotalCatch

                    Dim data(m_core.nEcosimTimeSteps) As Single
                    For i As Integer = 1 To m_core.nEcosimTimeSteps
                        Select Case resulttype
                            Case eResultTypes.TLC
                                data(i) = Me.m_core.EcosimOutputs.TLCatch(i)
                            Case eResultTypes.KemptonsQ
                                data(i) = Me.m_core.EcosimOutputs.KemptonsQ(i)
                            Case eResultTypes.ShannonDiversity
                                data(i) = Me.m_core.EcosimOutputs.ShannonDiversity(i)
                            Case eResultTypes.FIB
                                data(i) = Me.m_core.EcosimOutputs.FIB(i)
                            Case eResultTypes.TotalCatch
                                data(i) = Me.m_core.EcosimOutputs.TotalCatch(i)
                        End Select
                    Next i

                    strDataDetails = "Data," & resulttype.ToString
                    bSuccess = Me.SaveDataToFile(Me.GetOutputFileName(strPath, bSaveAnnual, resulttype),
                                                 bSaveAnnual, data,
                                                 strModelDetails, strDataDetails)

                Case eResultTypes.CatchFleetGroup,
                     eResultTypes.MortFleetGroup,
                     eResultTypes.ValueFleetGroup,
                     eResultTypes.DiscardMortalityFleetGroup,
                     eResultTypes.DiscardSurvivalFleetGroup,
                     eResultTypes.Landings

                    Dim data(,,) As Single = Nothing

                    Select Case resulttype
                        Case eResultTypes.CatchFleetGroup
                            data = Me.m_core.m_EcoSimData.ResultsSumCatchByGroupGear
                        Case eResultTypes.MortFleetGroup
                            data = Me.m_core.m_EcoSimData.ResultsSumFMortByGroupGear
                        Case eResultTypes.ValueFleetGroup
                            data = Me.m_core.m_EcoSimData.ResultsSumValueByGroupGear
                        Case eResultTypes.DiscardFleetGroup
                            data = Me.m_core.m_EcoSimData.ResultsTimeDiscardsGroupGear
                        Case eResultTypes.DiscardMortalityFleetGroup
                            data = Me.m_core.m_EcoSimData.ResultsTimeDiscardsMortGroupGear
                        Case eResultTypes.DiscardSurvivalFleetGroup
                            data = Me.m_core.m_EcoSimData.ResultsTimeDiscardsSurvivedGroupGear
                        Case eResultTypes.Landings
                            data = Me.m_core.m_EcoSimData.ResultsTimeLandingsGroupGear
                    End Select

                    strDataDetails = "Data," & resulttype.ToString
                    bSuccess = Me.SaveDataToFile(Me.GetOutputFileName(strPath, bSaveAnnual, resulttype),
                                                 bSaveAnnual, data,
                                                 strModelDetails, strDataDetails)

            End Select

            Return bSuccess

        End Function

        Private Function GetOutputFileName(ByVal strPath As String,
                                           ByVal bSaveAnnual As Boolean,
                                           ByVal outputtype As eResultTypes,
                                           Optional ByVal strGroupName As String = "") As String

            Dim strFileName As String = ""
            Dim strExt As String = ".csv"


            Select Case outputtype
                Case eResultTypes.Biomass
                    strFileName = "biomass"
                Case eResultTypes.Mortality
                    strFileName = "mortality"
                Case eResultTypes.Catch
                    strFileName = "catch"
                Case eResultTypes.ConsumptionBiomass
                    strFileName = "consumption-biomass"
                Case eResultTypes.FeedingTime
                    strFileName = "feedingtime"
                Case eResultTypes.AvgWeightOrProdCons
                    strFileName = "weight"
                Case eResultTypes.PredationMortality
                    strFileName = "predation_" & strGroupName
                Case eResultTypes.Prey
                    strFileName = "prey_" & strGroupName
                Case eResultTypes.TL
                    strFileName = "tl"
                Case eResultTypes.Value
                    strFileName = "value"
                Case eResultTypes.FIB
                    strFileName = "fib"
                Case eResultTypes.KemptonsQ
                    strFileName = "kemptonsq"
                Case eResultTypes.ShannonDiversity
                    strFileName = "shannondiversity"
                Case eResultTypes.TLC
                    strFileName = "tlc"
                Case eResultTypes.TotalCatch
                    strFileName = "totalcatch"
                Case eResultTypes.CatchFleetGroup
                    strFileName = "catch-fleet-group"
                Case eResultTypes.MortFleetGroup
                    strFileName = "mort-fleet-group"
                Case eResultTypes.ValueFleetGroup
                    strFileName = "value-fleet-group"
            End Select

            strFileName = strFileName & "_" & If(bSaveAnnual, "annual", "monthly")

            Dim strFullPath As String = Path.Combine(strPath, cFileUtils.ToValidFileName(strFileName, False) & strExt)
            If Not EwEUtils.Utilities.cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFullPath), True) Then Return ""
            Return strFullPath

        End Function

        Private Function SaveDataToFile(ByVal strFileName As String,
                                        ByVal bAnnual As Boolean,
                                        ByVal data As Single(,),
                                        ByVal strModelDetails As String,
                                        ByVal strDataDetails As String,
                                        ByVal strGroups As String) As Boolean

            If Not cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFileName)) Then Return False
            Try
                ' Overwrite the file
                Using sw As StreamWriter = New StreamWriter(strFileName, False)

                    If Me.m_core.SaveWithFileHeader Then
                        sw.WriteLine(strModelDetails)
                        sw.WriteLine(strDataDetails)
                        sw.WriteLine()
                    End If

                    If bAnnual Then
                        Dim simYears As Integer = CInt(Math.Floor((data.GetLength(1) - 1) / cCore.N_MONTHS))
                        Dim nGroups As Integer = data.GetLength(0) - 1
                        Dim sum(nGroups) As Single
                        sw.WriteLine(cStringUtils.ToCSVField("year\group") & "," & strGroups)
                        For j As Integer = 1 To simYears
                            sw.Write(Me.m_core.EcosimFirstYear - 1 + j)
                            For i As Integer = 1 To nGroups
                                For k As Integer = 1 To cCore.N_MONTHS
                                    If (k = 1) Then sum(i) = 0
                                    sum(i) += data(i, (j - 1) * cCore.N_MONTHS + k)
                                Next
                                sw.Write(",")
                                sw.Write(cStringUtils.FormatSingle(sum(i) / cCore.N_MONTHS))
                            Next
                            sw.WriteLine()
                        Next
                    Else
                        sw.WriteLine(cStringUtils.ToCSVField("timestep\group") & "," & strGroups)
                        'Each time steps
                        For j As Integer = 1 To data.GetLength(1) - 1
                            sw.Write(j)
                            'For every group
                            For i As Integer = 1 To data.GetLength(0) - 1
                                sw.Write(",")
                                sw.Write(cStringUtils.FormatSingle(data(i, j)))
                            Next
                            sw.WriteLine()
                        Next
                    End If
                    sw.Close()

                End Using

            Catch ex As Exception
                Return False
            End Try
            Return True

        End Function

        Private Function SaveDataToFile(ByVal strFileName As String,
                                 ByVal bAnnual As Boolean,
                                 ByVal data As Single(),
                                 ByVal strModelDetails As String,
                                 ByVal strDataDetails As String) As Boolean

            Try
                ' Overwrite the file
                Using sw As StreamWriter = New StreamWriter(strFileName, False)

                    If Me.m_core.SaveWithFileHeader Then
                        sw.WriteLine(strModelDetails)
                        sw.WriteLine(strDataDetails)
                        sw.WriteLine()
                    End If

                    If bAnnual Then
                        sw.WriteLine("year,value")

                        Dim simYears As Integer = CInt((data.Length - 1) / cCore.N_MONTHS)
                        Dim sum As Single
                        For j As Integer = 1 To simYears
                            sum = 0
                            For k As Integer = 1 To cCore.N_MONTHS
                                sum += data((j - 1) * cCore.N_MONTHS + k)
                            Next
                            sw.WriteLine(Me.m_core.EcosimFirstYear - 1 + j & "," & cStringUtils.FormatSingle(sum / cCore.N_MONTHS))
                        Next
                    Else
                        sw.WriteLine("timestep,value")
                        'Each time steps
                        For j As Integer = 1 To data.Length - 1
                            sw.WriteLine(j & "," & cStringUtils.FormatSingle(data(j)))
                        Next
                    End If
                    sw.Close()

                End Using

            Catch ex As Exception
                Return False
            End Try
            Return True

        End Function

        ''' <summary>
        ''' Save data by timestep, fleet, group 
        ''' </summary>
        ''' <param name="strFileName"></param>
        ''' <param name="bAnnual"></param>
        ''' <param name="data"></param>
        ''' <param name="strModelDetails"></param>
        ''' <param name="strDataDetails"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SaveDataToFile(ByVal strFileName As String,
                                 ByVal bAnnual As Boolean,
                                 ByVal data As Single(,,),
                                 ByVal strModelDetails As String,
                                 ByVal strDataDetails As String) As Boolean

            If Not cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFileName)) Then Return False
            Try
                ' Overwrite the file
                Using sw As StreamWriter = New StreamWriter(strFileName, False)

                    If Me.m_core.SaveWithFileHeader Then
                        sw.WriteLine(strModelDetails)
                        sw.WriteLine(strDataDetails)
                        sw.WriteLine()
                    End If

                    If bAnnual Then

                        Dim simYears As Integer = Me.m_core.nEcosimYears
                        sw.WriteLine("year,fleet,group,value")
                        For y As Integer = 1 To simYears
                            For i As Integer = 1 To Me.m_core.nFleets
                                For j As Integer = 1 To Me.m_core.nGroups
                                    Dim sum As Single = 0
                                    For k As Integer = 1 To cCore.N_MONTHS
                                        sum += data(j, i, (y - 1) * cCore.N_MONTHS + k)
                                    Next k
                                    If (sum > 0) Then
                                        sw.WriteLine("{0},{1},{2},{3}", Me.m_core.EcosimFirstYear - 1 + y, i, j, cStringUtils.ToCSVField(sum / cCore.N_MONTHS))
                                    End If
                                Next
                            Next
                        Next
                    Else
                        sw.WriteLine("timestep,fleet,group,value")
                        'Each time steps
                        For t As Integer = 1 To Me.m_core.nEcosimTimeSteps
                            For i As Integer = 1 To Me.m_core.nFleets
                                For j As Integer = 1 To Me.m_core.nGroups
                                    If (data(j, i, t) > 0) Then
                                        sw.WriteLine("{0},{1},{2},{3}", t, i, j, cStringUtils.ToCSVField(data(j, i, t) / cCore.N_MONTHS))
                                    End If
                                Next
                            Next
                        Next
                    End If
                    sw.Close()

                End Using

            Catch ex As Exception
                Return False
            End Try
            Return True

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get default model details to report in output file.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Function GetModelDetails() As String
            Return Me.m_core.DefaultFileHeader(eAutosaveTypes.EcosimResults)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the Ecosim result writer should show group names (true) 
        ''' or indexes (false). False by default.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property ShowGroupNames As Boolean = False

        Private Function GetAllGroupIdentifiers() As String

            Dim str As New StringBuilder()
            For i As Integer = 1 To Me.m_core.nGroups
                If (i > 1) Then str.Append(","c)
                str.Append(cStringUtils.ToCSVField(If(ShowGroupNames,
                                                                    Me.m_core.EcoPathGroupInputs(i).Name, CStr(Me.m_core.EcoPathGroupInputs(i).Index))))
            Next i
            Return str.ToString()

        End Function

#End Region ' Internal helpers

    End Class

End Namespace
