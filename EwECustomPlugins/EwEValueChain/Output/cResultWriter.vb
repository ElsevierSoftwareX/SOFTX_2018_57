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
Imports EwEUtils.Utilities

#End Region ' Imports

''' <summary>
''' CSV writer for Value Chain results.
''' </summary>
Public Class cResultWriter

#Region " Variables "

    Private m_data As cData = Nothing
    Private m_results As cResults = Nothing
    Private m_msg As cMessage = Nothing

#End Region ' Variables

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Shazaam!
    ''' </summary>
    ''' <param name="data"><see cref="cData">Value chain data</see> to plunder.</param>
    ''' <param name="results"><see cref="cResults">Value chain results</see> to write.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal data As cData, ByVal results As cResults)
        Me.m_data = data
        Me.m_results = results
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write results to CSV file.
    ''' </summary>
    ''' <param name="agg">Data aggregation method in use during the run.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function WriteResults(ByVal agg As cParameters.eAggregationModeType) As Boolean
        Return Me.WriteResults(agg, 0, "")
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="agg"></param>
    ''' <param name="iItem"></param>
    ''' <param name="strItem"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function WriteResults(ByVal agg As cParameters.eAggregationModeType, _
                                 ByVal iItem As Integer, _
                                 ByVal strItem As String) As Boolean

        Dim strFile As String = Me.GetFileName(agg, strItem)
        Dim sw As StreamWriter = Nothing
        Dim vs As cVariableStatus = Nothing

        ' Sanity check
        If String.IsNullOrWhiteSpace(strFile) Then Return False

        ' Try to open file
        Try
            sw = New StreamWriter(strFile, False)
        Catch ex As Exception
            ' Waah!
            Me.m_msg = New cMessage(cStringUtils.Localize(My.Resources.PROMPT_SAVERESULTS_FAILED, Path.GetDirectoryName(strFile), ex.Message),
                                    eMessageType.DataExport, eCoreComponentType.Ecotracer, eMessageImportance.Warning)
            Return False
        End Try

        ' -------------
        ' Start write process

        ' Write EwE header
        If Me.m_data.Core.SaveWithFileHeader Then
            sw.WriteLine(Me.GetModelDetails())
            sw.WriteLine()
        End If

        ' Write data header
        sw.Write("Variable")
        For Each u As cUnit In Me.m_data.GetUnits(cUnitFactory.eUnitType.All)
            sw.Write(",")
            sw.Write(cStringUtils.ToCSVField(u.Name))
        Next
        sw.WriteLine("")

        ' Write data
        For Each v As cResults.eVariableType In [Enum].GetValues(GetType(cResults.eVariableType))
            sw.Write(cStringUtils.ToCSVField(v.ToString))
            For Each u As cUnit In Me.m_data.GetUnits(cUnitFactory.eUnitType.All)
                sw.Write(",")
                sw.Write(cStringUtils.FormatNumber(Me.m_results.GetTotal(v, New cUnit() {u}, iItem, cResults.GetVariableContributionType(v))))
            Next
            sw.WriteLine("")
        Next
        sw.Close()

        ' Already has save result message?
        If (Me.m_msg Is Nothing) Then
            ' #No: create one
            Me.m_msg = New cMessage(cStringUtils.Localize(My.Resources.PROMPT_SAVERESULTS_SUCCESS, Path.GetDirectoryName(strFile)),
                                        eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
            ' Set hyperlink
            Me.m_msg.Hyperlink = Path.GetDirectoryName(strFile)
        End If

        ' Add status to message
        vs = New cVariableStatus(eStatusFlags.OK, cStringUtils.Localize(My.Resources.PROMPT_SAVERESULT_DETAIL, strFile),
                                 eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0)
        Me.m_msg.AddVariable(vs)

        ' We're done, Jim
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="agg"></param>
    ''' <param name="strItem"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function GetFileName(ByVal agg As cParameters.eAggregationModeType,
                                 ByVal strItem As String) As String

        Dim strPath As String = ""
        Dim strFile As String = ""

        Select Case m_results.RunType
            Case cModel.eRunTypes.Ecopath
                strPath = Path.Combine(Me.m_data.Core.DefaultOutputPath(eAutosaveTypes.Ecopath), "ValueChain")
            Case cModel.eRunTypes.Ecosim
                strPath = Path.Combine(Me.m_data.Core.DefaultOutputPath(eAutosaveTypes.Ecosim), "ValueChain")
            Case cModel.eRunTypes.Equilibrium
                Return ""
                'strPath = Me.m_data.Core.DefaultOutputPath(eAutosaveTypes.Ecopath, strPrefix:="ValueChain_")
        End Select

        If Not cFileUtils.IsDirectoryAvailable(strPath, True) Then Return ""

        If String.IsNullOrWhiteSpace(strItem) Then
            strFile = cStringUtils.Localize("valuechain_{0}.csv", agg.ToString())
        Else
            strFile = cStringUtils.Localize("valuechain_{0}_{1}.csv", agg.ToString(), strItem)
        End If

        Return Path.Combine(strPath, cFileUtils.ToValidFileName(strFile, False))

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the save results message.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property Message As cMessage
        Get
            Return Me.m_msg
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get default model details to report in output file.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Function GetModelDetails() As String

        Dim sb As New StringBuilder()
        Dim core As cCore = Me.m_data.Core

        ' Append header
        If (Me.m_results.RunType = cModel.eRunTypes.Ecopath) Then
            sb.AppendLine(core.DefaultFileHeader(eAutosaveTypes.Ecopath))
        Else
            sb.AppendLine(core.DefaultFileHeader(eAutosaveTypes.Ecosim))
        End If
        ' Append value chain run type
        sb.AppendLine("RunType," & cStringUtils.ToCSVField(Me.m_results.RunType.ToString()))

        Return sb.ToString()

    End Function

End Class
