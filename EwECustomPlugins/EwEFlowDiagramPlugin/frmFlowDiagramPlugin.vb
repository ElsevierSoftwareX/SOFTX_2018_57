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
Option Explicit On

Imports System
Imports System.IO
Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports System.Globalization

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Form for the EwE flow diagram plug-in.
''' </summary>
''' ===========================================================================
Public Class frmFlowDiagramPlugin

#Region " Privates "

    Private m_EcopathDs As cEcopathDataStructures
    Private m_plugin As cEwEFlowDiagramPlugin

#End Region ' Privates

    Public Sub New(ByVal strText As String, ByVal plugin As cEwEFlowDiagramPlugin)

        Me.InitializeComponent()
        Me.Text = strText
        Me.TabText = strText

        Me.m_plugin = plugin

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Launch the EwE flow diagram showing current EwE5 data.
    ''' </summary>
    ''' <param name="strFileName">Name of the EwE5 flow diagram data file to use.</param>
    ''' <param name="bCreateFile">Flag stating whether the EwE5 flow diagram 
    ''' data file should be written (true) or if an existing data file should be 
    ''' used (false).</param>
    ''' -----------------------------------------------------------------------
    Public Sub LaunchFlowDiagram(ByVal strFileName As String, _
                                 Optional ByVal bCreateFile As Boolean = True)

        Dim core As cCore = Me.m_plugin.Core
        Dim data As cEcopathDataStructures = Me.m_plugin.EcopathDatastructures
        Dim writer As TextWriter = Nothing
        Dim groupIn As cEcoPathGroupInput = Nothing
        Dim groupOut As cEcoPathGroupOutput = Nothing
        Dim strGroup As String = ""
        Dim impVal As Single
        Dim fi(core.nGroups, core.nGroups) As Single
        Dim strError As String = ""
        Dim sValue As Single = 0.0!

        ' Check if the extra necessery information is avaiable
        If (data Is Nothing) Then Return

        If bCreateFile Then

            ' Compute food index
            For i As Integer = 1 To core.nGroups
                For j As Integer = 1 To core.nGroups
                    fi(i, j) = core.EcoPathGroupOutputs(i).Biomass * core.EcoPathGroupOutputs(i).QBOutput * core.EcoPathGroupInputs(i).DietComp(j)
                Next j
            Next i

            Try
                writer = New StreamWriter(strFileName)
            Catch ex As Exception
                cLog.Write("FlowDiagram: cannot write data file, " & ex.Message)
                Return
            End Try

            ' Write number of groups, formatted to two decimals
            writer.WriteLine(Format$(core.nGroups, "00"))
            ' Write number of living groups, formatted to two decimals
            writer.WriteLine(Format$(core.nLivingGroups, "00"))

            For i As Integer = 1 To core.nGroups 'To 1 Step -1

                ' Get core groups
                groupIn = core.EcoPathGroupInputs(i)
                groupOut = core.EcoPathGroupOutputs(i)

                ' Group name is written as a string of 15 characters max in a 20 character buffer
                strGroup = New String(" "c, 20)
                Mid$(strGroup, 1, 15) = Trim(groupIn.Name) 'Specie(i)

                writer.Write(strGroup)
                writer.Write(Me.FormatNumber(Math.Abs(core.EcoPathGroupOutputs(i).TTLX)))
                writer.Write(Me.FormatNumber(groupOut.Biomass))
                If groupIn.IsConsumer Then
                    writer.Write(Me.FormatNumber(Math.Abs(groupOut.Biomass * groupOut.PBOutput)))
                Else
                    writer.Write(Me.FormatNumber(0.0!))
                End If
                writer.Write(Me.FormatNumber(data.fCatch(i)))
                writer.Write(Me.FormatNumber(data.Ex(i)))
                writer.Write(Me.FormatNumber(groupOut.FlowToDet))
                writer.Write(Me.FormatNumber(groupOut.Respiration))
                writer.Write(Me.FormatNumber(fi(i, i)))

                If i > core.nLivingGroups Then
                    impVal = groupIn.DetImport
                Else
                    impVal = groupOut.Biomass * groupOut.QBOutput * groupIn.DietComp(0)
                End If

                writer.WriteLine(Me.FormatNumber(impVal))
            Next i

            ' Save food index => nt% = 21
            For i As Integer = 1 To core.nGroups
                writer.Write("                    ")
                For j As Integer = 1 To core.nGroups
                    writer.Write(Me.FormatNumber(Math.Abs(fi(i, j))))
                Next j
                writer.WriteLine("")
            Next i

            ' Save the Det() matrix for multiple det 121895 eli.
            For i As Integer = 1 To core.nGroups
                writer.Write("                    ")
                groupIn = core.EcoPathGroupInputs(i)
                For j As Integer = 1 To core.nDetritusGroups '+ 1 To m_core.nGroups
                    writer.WriteLine(Me.FormatNumber(Math.Abs(groupIn.DetritusFate(j))))
                Next j
            Next i
            writer.Close()
        End If

        ' Todo_JS: Reprogram old FD logic as a .NET control, and get rid of the 16 bit apps once and for all.

        'Execute the external application through the general function on EwEUtils
        If Not EwEUtils.SystemUtilities.cSystemUtils.AppExec("fd.exe", strFileName, strError, "") Then
            ' ToDo: globalize this
            Dim msg As New cMessage("Unable to run application 'fd.exe': " & strError, _
                                    eMessageType.Any, eCoreComponentType.External, eMessageImportance.Critical)
            core.Messages.SendMessage(msg)
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Format a number to a string, using en-US notation, padded to a specified
    ''' number of digits and decimals.
    ''' </summary>
    ''' <param name="sValue">Value to format.</param>
    ''' <param name="iNumDigits">Number of digits before the decimal point.</param>
    ''' <param name="iNumDec">Number of decmals after the decimal point.</param>
    ''' <returns></returns>
    ''' <remarks><para>Room for the minus sign, if any, will be included in 
    ''' <paramref name="iNumDigits"/></para></remarks>
    ''' -----------------------------------------------------------------------
    Private Function FormatNumber(ByVal sValue As Single, _
                                  Optional ByVal iNumDigits As Integer = 8, _
                                  Optional ByVal iNumDec As Integer = 3) As String

        ' This code aint pretty: a format mask is generated on the fly to
        ' match the number of digits, decimals and a negative sign if needed.

        Dim ci As New CultureInfo("en-US")
        Dim strMask As String = "{0:"

        ' Account for negative sign. String formatting will place a negative sign 
        ' outside the number of digits, thus screwing up the precise number 
        ' formatting needed in this data file.
        If (Math.Sign(sValue) = -1) Then iNumDigits -= 1

        ' Add zero padding character for every digit (eek)
        For i As Integer = 0 To iNumDigits - 1
            strMask &= "0"
        Next
        ' Add decimal point format character
        strMask &= "."

        ' Add zero padding character for every decimal (aargh)
        For i As Integer = 0 To iNumDec - 1
            strMask &= "0"
        Next
        ' Close mask
        strMask &= "}"

        Return String.Format(ci, strMask, sValue)

    End Function

    Private Sub OnClickLaunch(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles SDF_btn.Click

        Try
            Dim strFileName As String = cFileUtils.ToValidFileName(Me.m_plugin.Core.EwEModel.Name, False) & ".flw"
            Dim strTempFile As String = cFileUtils.MakeTempFile(strFileName)
            Dim bCreateFile As Boolean = True

            If System.IO.File.Exists(strTempFile) Then

                Dim fmsg As New cFeedbackMessage("Flow diagram file already exist, would you like to load it? " & vbCrLf & "Yes to load the file, No to make a new file", _
                                                 eCoreComponentType.External, eMessageType.Any, eMessageImportance.Information, eMessageReplyStyle.YES_NO, eDataTypes.NotSet, eMessageReply.YES)
                Me.m_plugin.Core.Messages.SendMessage(fmsg)
                bCreateFile = (fmsg.Reply = eMessageReply.NO)
            End If

            Me.LaunchFlowDiagram(strTempFile, bCreateFile)

        Catch ex As Exception
            Throw New Exception(ex.ToString)
        End Try

    End Sub

End Class