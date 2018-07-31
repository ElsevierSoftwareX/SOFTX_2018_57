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

Namespace MSY

    ''' <summary>
    ''' Class for writing MSY run results.
    ''' </summary>
    Public Class cMSYResultWriterMSY
        Inherits cMSYResultWriterBase

#Region " Construction "

        Public Sub New(core As cCore)
            MyBase.New(core)
        End Sub

#End Region ' Construction

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Write MSY results to CSV file when MSY has been ran for a group.
        ''' </summary>
        ''' <param name="strPath">Output file location.</param>
        ''' <param name="iGroup">Group that MSY was ran for.</param>
        ''' <param name="ass"><see cref="eMSYAssessmentTypes">Tee hee hee</see>.</param>
        ''' <param name="FBase">Base F.</param>
        ''' <param name="results">MSY results.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function WriteGroupResults(ByVal strPath As String, _
                                          ByVal iGroup As Integer, _
                                          ByVal ass As eMSYAssessmentTypes, _
                                          ByVal FBase As Single, _
                                          ByVal results As cMSYFResult(), _
                                          ByVal optimum As cMSYOptimum) As Boolean

            Dim target As cEcoPathGroupInput = Me.m_core.EcoPathGroupInputs(iGroup)
            Dim sw As StreamWriter = Nothing
            Dim r As cMSYFResult = Nothing
            Dim strFile As String = ""
            Dim bSuccess As Boolean = True

            ' 2 Variables
            For k As Integer = 0 To 1

                strFile = Path.Combine(strPath, Me.CSVFileName(target, If(k = 0, "B", "Catch"), ass))
                sw = Me.OpenWriter(strFile)

                If (sw IsNot Nothing) Then
                    Me.WriteGroupHeader(sw, ass, target, FBase, optimum)
                    sw.WriteLine()

                    ' Data header
                    sw.Write("F")
                    For j As Integer = 1 To Me.m_core.nGroups
                        Dim grp As cEcoPathGroupInput = Me.m_core.EcoPathGroupInputs(j)
                        sw.Write(",{0}", cStringUtils.ToCSVField(grp.Name))
                    Next
                    sw.WriteLine()

                    For i As Integer = 0 To results.Length - 1
                        r = results(i)
                        sw.Write(cStringUtils.FormatSingle(r.FCur))
                        For j As Integer = 1 To Me.m_core.nGroups
                            Dim grp As cEcoPathGroupInput = Me.m_core.EcoPathGroupInputs(j)
                            sw.Write(",{0}", cStringUtils.FormatSingle(If(k = 0, r.B(j), r.Catch(j))))
                        Next
                        sw.WriteLine()
                    Next
                    bSuccess = bSuccess And Me.CloseWriter(sw, strFile)
                Else
                    bSuccess = False
                End If
            Next
            Return bSuccess And Me.WriteGroupValueResults(strPath, iGroup, ass, FBase, results, optimum)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Write MSY results to CSV file when MSY has been ran for a group.
        ''' </summary>
        ''' <param name="strPath">Output file location.</param>
        ''' <param name="iGroup">Group that MSY was ran for.</param>
        ''' <param name="ass"><see cref="eMSYAssessmentTypes">Tee hee hee</see>.</param>
        ''' <param name="FBase">Base F.</param>
        ''' <param name="results">MSY results.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function WriteGroupValueResults(ByVal strPath As String,
                                               ByVal iGroup As Integer,
                                               ByVal ass As eMSYAssessmentTypes,
                                               ByVal FBase As Single,
                                               ByVal results As cMSYFResult(),
                                               ByVal optimum As cMSYOptimum) As Boolean

            Dim target As cEcoPathGroupInput = Me.m_core.EcoPathGroupInputs(iGroup)
            Dim strFile As String = ""
            Dim sw As StreamWriter = Nothing
            Dim r As cMSYFResult = Nothing
            Dim bSuccess As Boolean = True

            strFile = Path.Combine(strPath, Me.CSVFileName(target, "value", ass))
            sw = Me.OpenWriter(strFile)

            If (sw IsNot Nothing) Then
                Me.WriteGroupHeader(sw, ass, target, FBase, optimum)
                sw.WriteLine()

                ' Data header
                sw.Write("F, TotalValue")
                sw.WriteLine()

                For i As Integer = 0 To results.Length - 1
                    r = results(i)
                    sw.Write(cStringUtils.FormatSingle(r.FCur))
                    sw.Write(",")
                    sw.Write(cStringUtils.FormatSingle(r.TotalValue))
                    sw.WriteLine()
                Next
                bSuccess = bSuccess And Me.CloseWriter(sw, strFile)
            Else
                bSuccess = False
            End If
            Return bSuccess

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Write MSY results to CSV file when MSY has been ran for a fleet.
        ''' </summary>
        ''' <param name="strPath">Output file location.</param>
        ''' <param name="iFleet">Fleet that MSY was ran for.</param>
        ''' <param name="assessment"><see cref="eMSYAssessmentTypes"/>.</param>
        ''' <param name="results">MSY results.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function WriteFleetResults(ByVal strPath As String,
                                          ByVal iFleet As Integer,
                                          ByVal assessment As eMSYAssessmentTypes,
                                          ByVal results As cMSYFResult(),
                                          ByVal optimum As cMSYOptimum) As Boolean

            Dim flt As cEcopathFleetInput = Me.m_core.EcopathFleetInputs(iFleet)
            Dim sw As StreamWriter = Nothing
            Dim r As cMSYFResult = Nothing
            Dim strFile As String = ""
            Dim bSuccess As Boolean = True

            ' 2 variables
            For k As Integer = 0 To 1

                strFile = Path.Combine(strPath, Me.CSVFileName(flt, If(k = 0, "B", "Catch"), assessment))
                sw = Me.OpenWriter(strFile)
                If (sw IsNot Nothing) Then

                    Me.WriteFleetHeader(sw, assessment, flt, optimum)
                    sw.WriteLine()

                    ' Data header
                    sw.Write("F")
                    For j As Integer = 1 To Me.m_core.nGroups
                        Dim grp As cEcoPathGroupInput = Me.m_core.EcoPathGroupInputs(j)
                        sw.Write(",{0}", cStringUtils.ToCSVField(grp.Name))
                    Next
                    sw.WriteLine()

                    For i As Integer = 0 To results.Length - 1
                        r = results(i)
                        sw.Write(cStringUtils.FormatSingle(r.FCur))
                        For j As Integer = 1 To Me.m_core.nGroups
                            Dim grp As cEcoPathGroupInput = Me.m_core.EcoPathGroupInputs(j)
                            sw.Write(",{0}", cStringUtils.FormatSingle(If(k = 0, r.B(j), r.Catch(j))))
                        Next
                        sw.WriteLine()
                    Next

                    bSuccess = bSuccess And Me.CloseWriter(sw, strFile)
                Else
                    bSuccess = False
                End If
            Next

            Return bSuccess And Me.WriteFleetValueResults(strPath, iFleet, assessment, results, optimum)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Write MSY results to CSV file when MSY has been ran for a fleet.
        ''' </summary>
        ''' <param name="strPath">Output file location.</param>
        ''' <param name="iFleet">Fleet that MSY was ran for.</param>
        ''' <param name="assessment"><see cref="eMSYAssessmentTypes"/>.</param>
        ''' <param name="results">MSY results.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function WriteFleetValueResults(ByVal strPath As String,
                                               ByVal iFleet As Integer,
                                               ByVal assessment As eMSYAssessmentTypes,
                                               ByVal results As cMSYFResult(),
                                               ByVal optimum As cMSYOptimum) As Boolean

            Dim flt As cEcopathFleetInput = Me.m_core.EcopathFleetInputs(iFleet)
            Dim sw As StreamWriter = Nothing
            Dim r As cMSYFResult = Nothing
            Dim bSuccess As Boolean = True

            strPath = Path.Combine(strPath, Me.CSVFileName(flt, "value", assessment))
            sw = Me.OpenWriter(strPath)
            If (sw IsNot Nothing) Then

                Me.WriteFleetHeader(sw, assessment, flt, optimum)
                sw.WriteLine()

                ' Data header
                sw.Write("F, TotalValue")
                sw.WriteLine()

                For i As Integer = 0 To results.Length - 1
                    r = results(i)
                    sw.Write(cStringUtils.FormatSingle(r.FCur))
                    sw.Write(",")
                    sw.Write(cStringUtils.FormatSingle(r.TotalValue))
                    sw.WriteLine()
                Next

                bSuccess = bSuccess And Me.CloseWriter(sw, strPath)
            Else
                bSuccess = False
            End If

            Return bSuccess

        End Function

#End Region ' Public access

#Region " Internals "

        Protected Sub WriteGroupHeader(ByVal sw As StreamWriter, ByVal ass As eMSYAssessmentTypes,
                                       ByVal target As cEcoPathGroupInput,
                                       ByVal fBase As Single, ByVal optimum As cMSYOptimum)
            MyBase.WriteHeader(sw, ass, "MSY")
            sw.WriteLine("Group,{0}", cStringUtils.ToCSVField(target.Name))
            sw.WriteLine("Fbase,{0}", cStringUtils.FormatSingle(fBase))
            sw.WriteLine("Fmsy,{0}", If(optimum.IsFopt(target.Index),
                                                       cStringUtils.FormatSingle(optimum.FOpt(target.Index)),
                                                       cStringUtils.ToCSVField(My.Resources.CoreMessages.FMSY_STATUS_NOTFOUND)))
        End Sub

        Protected Sub WriteFleetHeader(ByVal sw As StreamWriter, ByVal ass As eMSYAssessmentTypes,
                                       ByVal target As cEcopathFleetInput, ByVal optimum As cMSYOptimum)

            Me.WriteHeader(sw, ass)

            sw.WriteLine("Fleet,{0}", cStringUtils.ToCSVField(target.Name))
            sw.WriteLine()

            ' Fmsy found header
            sw.Write("")
            For j As Integer = 1 To Me.m_core.nGroups
                Dim grp As cEcoPathGroupInput = Me.m_core.EcoPathGroupInputs(j)
                sw.Write(",{0}", cStringUtils.ToCSVField(grp.Name))
            Next
            sw.WriteLine()
            sw.Write("FmsyFound")
            For j As Integer = 1 To Me.m_core.nGroups
                sw.Write(",{0}", If(optimum.IsFopt(j), 1, 0))
            Next
            sw.WriteLine()
        End Sub

        Protected Overloads Sub WriteHeader(sw As StreamWriter, ass As eMSYAssessmentTypes)
            MyBase.WriteHeader(sw, ass, "MSY")
        End Sub

        Protected Function CSVFileName(ByVal target As cCoreInputOutputBase, _
                                       ByVal strVar As String, _
                                       ByVal assessment As eMSYAssessmentTypes) As String
            Return cFileUtils.ToValidFileName(target.Name & "_" & strVar & "_" & assessment.ToString() & ".csv", False)

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cMSYResultWriterBase.ErrorMessage"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Function ErrorMessage(strPath As String, strReason As String) As cMessage
            Return New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.MSY_RESULTS_SAVE_FAILED, strPath, strReason), _
                                eMessageType.DataExport, eCoreComponentType.MSY, eMessageImportance.Information)
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cMSYResultWriterBase.SuccessMessage"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Function SuccessMessage(strPath As String) As cMessage
            Dim msg As cMessage = New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.MSY_RESULTS_SAVE_SUCCESS, strPath), _
                                               eMessageType.DataExport, eCoreComponentType.MSY, eMessageImportance.Information)
            msg.Hyperlink = Path.GetDirectoryName(strPath)
            Return msg
        End Function

#End Region ' Internals

    End Class

End Namespace
