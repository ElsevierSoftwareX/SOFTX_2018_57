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
    ''' Class for writing Fmsy search results.
    ''' </summary>
    Public Class cMSYResultWriterFMSY
        Inherits cMSYResultWriterBase

#Region " Construction "

        Public Sub New(core As cCore)
            MyBase.new(core)
        End Sub

#End Region ' Construction

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Write FMSY search results to a CSV file.
        ''' </summary>
        ''' <param name="strPath"></param>
        ''' <param name="ass"></param>
        ''' <param name="result"></param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function WriteCSV(ByVal strPath As String, _
                                 ByVal ass As eMSYAssessmentTypes, _
                                 ByVal result As cFMSYResults) As Boolean

            If (result Is Nothing) Then Return False

            Dim epdata As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim strFile As String = ""
            Dim sw As StreamWriter = Nothing
            Dim bSuccess As Boolean = True

            strFile = Path.Combine(strPath, "FMSY_" & ass.ToString & ".csv")
            sw = Me.OpenWriter(strFile)

            If (sw IsNot Nothing) Then

                ' Write file header
                Me.WriteHeader(sw, ass, "Fmsy")
                sw.WriteLine()
                ' Write data header
                sw.WriteLine("Group, TL, Fbase, Cbase, Vbase, FmsyFound, Fmsy, Cmsy, Vmsy, CmsyAll, VmsyAll")
                ' Write data
                For i As Integer = 1 To Me.m_core.nGroups
                    ' Only write MSY values if an actual max has been detected
                    sw.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}",
                                 cStringUtils.ToCSVField(epdata.GroupName(i)),
                                 cStringUtils.FormatSingle(epdata.TTLX(i)),
                                 cStringUtils.FormatSingle(result.FBase(i)),
                                 cStringUtils.FormatSingle(result.CMSYBase(i)),
                                 cStringUtils.FormatSingle(result.ValueBase(i)),
                                 If(result.IsFopt(i), 1, 0),
                                 cStringUtils.FormatSingle(result.FMSY(i)),
                                 cStringUtils.FormatSingle(result.CMSY(i)),
                                 cStringUtils.FormatSingle(result.Value(i)),
                                 cStringUtils.FormatSingle(result.CatchAtFMSY(i)),
                                 cStringUtils.FormatSingle(result.ValueAtFMSY(i)))
                Next i

#If 0 Then

                If Check1.value > 0 And UBound(CmsyWS, 2) > 0 Then
                    For i = 1 To NumGroups
                        For j = 1 To NumGroups - 1
                    Print #Ifileno, CmsyWS(i, j); ",";
                        Next
                Print #Ifileno, CmsyWS(i, NumGroups)
                    Next
                End If
            End If
#End If
                bSuccess = bSuccess And Me.CloseWriter(sw, strFile)
            End If

            Return bSuccess

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cMSYResultWriterBase.ErrorMessage"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Function ErrorMessage(strPath As String, strReason As String) As cMessage
            Return New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.FMSY_RESULTS_SAVE_FAILED, strPath, strReason), _
                                eMessageType.DataExport, eCoreComponentType.MSY, eMessageImportance.Information)
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cMSYResultWriterBase.SuccessMessage"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Function SuccessMessage(strPath As String) As cMessage
            Dim msg As cMessage = New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.FMSY_RESULTS_SAVE_SUCCESS, strPath), _
                                               eMessageType.DataExport, eCoreComponentType.MSY, eMessageImportance.Information)
            msg.Hyperlink = Path.GetDirectoryName(strPath)
            Return msg
        End Function

    End Class

End Namespace
