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
' Copyright 1991- Ecopath International Initiative, Barcelona, Spain and
'                 Joint Reseach Centre, Ispra, Italy.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports System.Text
Imports EwEUtils.Utilities

#End Region ' Imports

''' <summary>
''' Aquamaps file reader.
''' </summary>
Public Class cFileReader

    Private m_core As cCore = Nothing

    Public Sub New(core As cCore)
        Me.m_core = core
    End Sub

    Public Function ReadEnvelopeData(strFile As String, data As cImportData) As Boolean

        Dim reader As StreamReader = Nothing
        Dim fd As cImportData.cFileData = Nothing
        Dim strLine As String = ""
        Dim strSpecies As String = ""
        Dim nUsed As Integer = 0
        Dim bSuccess As Boolean = True

        Try
            reader = New StreamReader(strFile, Encoding.GetEncoding("iso-8859-1"))
        Catch ex As Exception
            Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_FILE_NOT_FOUND, strFile), eMessageImportance.Warning)
            Return False
        End Try

        strLine = reader.ReadLine().Split(","c)(0)
        bSuccess = (strLine.IndexOf("Mapping parameters for ", 0, System.StringComparison.InvariantCultureIgnoreCase) = 0)

        If (bSuccess) Then
            strSpecies = strLine.Substring("Mapping parameters for ".Length).Trim
            bSuccess = Not String.IsNullOrWhiteSpace(strSpecies)
        End If

        If bSuccess Then
            fd = data.AddFile(strSpecies)
            Dim bFoundEnvHeader As Boolean = False
            Dim bDone As Boolean = False

            bSuccess = Not reader.EndOfStream
            While Not bFoundEnvHeader And bSuccess
                strLine = reader.ReadLine
                bFoundEnvHeader = (strLine.IndexOf("Species Envelope (HSPEN):") = 0)
                bSuccess = Not reader.EndOfStream
            End While

            If (bFoundEnvHeader) Then
                reader.ReadLine()
                strLine = reader.ReadLine()

                bSuccess = Not reader.EndOfStream
                While bSuccess And Not bDone

                    ' Patch line formatting
                    If (strLine.Contains(";")) Then
                        strLine = strLine.Replace(".", "") ' Remove thousands separators
                        strLine = strLine.Replace(",", ".") ' Replace decimal commas with decimal points
                        strLine = strLine.Replace(";", ",") ' Replace semi-colons with commas to separate numbers
                    End If

                    Dim astrBits As String() = strLine.Split(","c)
                    Dim strName As String = astrBits(0).Trim
                    Dim sMin, sMax, sMinPref, sMaxPref As Single
                    Dim bUsed As Boolean = False

                    If Not String.IsNullOrWhiteSpace(strName) Then
                        Try
                            If (astrBits(1).StartsWith("1")) And bSuccess Then
                                bSuccess = bSuccess And _
                                    Single.TryParse(astrBits(2), sMin) And _
                                    Single.TryParse(astrBits(3), sMinPref) And _
                                    Single.TryParse(astrBits(4), sMaxPref) And _
                                    Single.TryParse(astrBits(5), sMax)
                                bUsed = bSuccess

                                If (bUsed = False) Then
                                    Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_FILE_INVALID, Path.GetFileName(strFile)), eMessageImportance.Warning, _
                                                   cStringUtils.Localize(My.Resources.PROMPT_IMPORT_DETAIL_LINEERROR, strLine))
                                End If
                            End If

                        Catch ex As Exception
                            Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_FILE_INVALID, Path.GetFileName(strFile)), eMessageImportance.Warning)
                            bSuccess = False
                        End Try
                    Else
                        bDone = True
                        bUsed = False
                    End If

                    If bSuccess And bUsed Then
                        fd.AddFunction(strName, sMin, sMinPref, sMaxPref, sMax)
                        nUsed += 1
                    End If

                    strLine = reader.ReadLine()
                    bDone = reader.EndOfStream Or strLine.Contains(":")
                End While
            End If
        End If

        reader.Close()

        If (Not bSuccess) Then
            data.Clear()
        End If

        If (nUsed = 0 And bSuccess) Then
            Me.SendMessage(cStringUtils.Localize(My.Resources.PROMPT_FILE_NOCONTENT, strFile), eMessageImportance.Warning)
        End If

        Return bSuccess

    End Function

    Private Sub SendMessage(ByVal strMessage As String, importance As eMessageImportance,
                            Optional ByVal strDetails As String = "")
        Dim msg As New cMessage(strMessage, EwEUtils.Core.eMessageType.DataImport, EwEUtils.Core.eCoreComponentType.External, importance)

        If (Not String.IsNullOrWhiteSpace(strDetails)) Then
            Dim vs As New cVariableStatus(eStatusFlags.OK, strDetails, eVarNameFlags.NotSet, eDataTypes.External, eCoreComponentType.External, 0)
            msg.AddVariable(vs)
        End If

        Me.m_core.Messages.SendMessage(msg)
    End Sub

End Class
