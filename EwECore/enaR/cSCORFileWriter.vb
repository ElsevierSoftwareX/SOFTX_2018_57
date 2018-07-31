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

Option Strict On

Imports System.IO
Imports EwECore
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports System.Globalization


Public Class cSCORFileWriter

    Private m_EPData As cEcopathDataStructures

    Public Sub New(EcopathData As cEcopathDataStructures)
        Me.m_EPData = EcopathData
    End Sub

    Public Function Write(Filename As String, ENARData As cENARDataStructures) As Boolean
        Dim igrp As Integer
        Dim bReturn As Boolean = True

        'System.Console.WriteLine("SCOR file writer: " + Filename)

        Dim orgCulture As CultureInfo = Threading.Thread.CurrentThread.CurrentCulture
        Dim culture As CultureInfo = CultureInfo.CreateSpecificCulture("en-US")
        Threading.Thread.CurrentThread.CurrentCulture = culture

        Try

            Dim strm As New StreamWriter(Filename)

            strm.WriteLine(Me.m_EPData.ModelName)

            strm.WriteLine("{0,3}{1,3}", m_EPData.NumGroups, m_EPData.NumLiving)

            For igrp = 1 To Me.m_EPData.NumGroups
                'strip spaces out of group names
                strm.WriteLine(Me.m_EPData.GroupName(igrp).Replace(" ", ""))
            Next igrp


            Dim frmStr As String = "{0,3} {1,6:F6}"

            For igrp = 1 To Me.m_EPData.NumGroups
                strm.WriteLine(frmStr, igrp, ENARData.b(igrp))
            Next

            strm.WriteLine(" -1 0.")
            For igrp = 1 To Me.m_EPData.NumGroups
                strm.WriteLine(frmStr, igrp, ENARData.Import(igrp))
            Next


            strm.WriteLine(" -1 0.")
            For igrp = 1 To Me.m_EPData.NumGroups
                strm.WriteLine(frmStr, igrp, ENARData.CatchExport(igrp))
            Next

            strm.WriteLine(" -1 0.")
            For igrp = 1 To Me.m_EPData.NumGroups
                strm.WriteLine(frmStr, igrp, ENARData.Resp(igrp))
            Next

            strm.WriteLine(" -1 0.")
            For iprey As Integer = 1 To Me.m_EPData.NumGroups
                For ipred As Integer = 1 To Me.m_EPData.NumGroups

                    If ENARData.Consumpt(iprey, ipred) > 0 Then
                        strm.WriteLine("{0,-4}{1,-4}{2,6:F6}", iprey, ipred, ENARData.Consumpt(iprey, ipred))
                    End If

                Next ipred
            Next iprey

            strm.WriteLine(" -1 0.")

            strm.Close()

        Catch ex As Exception
            cLog.Write(ex, "enaR SCOR file writer exception.")
            bReturn = False
        End Try

        Threading.Thread.CurrentThread.CurrentCulture = orgCulture

        Return bReturn

    End Function


#If 0 Then
     Private Function Write(ByVal strFileName As String) As Boolean

        'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        'Code from original attempt as a SCOR file writer
        Dim sw As StreamWriter = Nothing
        Dim line As Char() = Nothing
        Dim fmt As New cCurrencyUnitFormatter(Me.m_epData.ModelUnitCurrencyCustom)
        Dim sValue As Single = 0

        Try
            sw = New StreamWriter(strFileName)
        Catch ex As Exception
            cLog.Write(ex, "cSCORWriter '" & strFileName & "'")
            Return False
        End Try

        ' Do yer magics

        ' 1. Header line, with optional accuracy bytes
        line = NewLine(Me.m_epData.ModelName & ";" & cStringUtils.ToUTF8(fmt.GetDescriptor(Me.m_epData.ModelUnitCurrency)), 80)
        sw.WriteLine(line)

        ' 2. Compartments indices line
        sw.WriteLine("{0,3}{1,3}", m_epData.NumGroups, m_epData.NumLiving)

        ' 3. Group names
        For i As Integer = 1 To m_epData.NumGroups
            sw.WriteLine(Me.m_epData.GroupName(i))
        Next i

        ' 4. Data
        'a) Biomass()
        For i As Integer = 1 To m_epData.NumGroups
            sw.WriteLine("{0,3} {1}", i, cStringUtils.FormatNumber(Me.m_epData.B(i)))
        Next i

        ' b) Import
        sw.WriteLine("{0,3}\IMPORTS", -1)
        For i As Integer = 1 To m_epData.NumGroups
            sValue = Me.CalcImport(i)
            If (sValue > 0) Then
                sw.WriteLine("{0,3} {1}", i, cStringUtils.FormatNumber(sValue))
            End If
        Next i

        ' c) Export
        sw.WriteLine("{0,3}\EXPORTS", -1)
        For j As Integer = 1 To Me.m_epData.NumGroups
            Dim Q As Single = 0
            Dim R As Single = 0
            Dim Flow As Single = 0

            For i As Integer = 1 To Me.m_epData.NumGroups
                Q += Me.CalcConsumption(j, i)
                Flow += Me.CalcConsumption(i, j)
            Next
            Q += Me.CalcConsumption(j, 0)
            R = Me.m_epData.Resp(j)

            ' PP fudge - not correct
            If (m_epData.PP(j) = 1) Then Q = Flow

            ' Export = Qi - Ri - (Sum of flows i>j)
            sw.WriteLine("{0,3} {1}", j, cStringUtils.FormatNumber(Math.Max(0, Q - R - Flow)))
        Next j

        ' d) Respiration
        sw.WriteLine("{0,3}\RESPIRATION", -1)
        For i As Integer = 1 To m_epData.NumGroups
            sw.WriteLine("{0,3} {1}", i, cStringUtils.FormatNumber(Me.m_epData.Resp(i)))
        Next i

        'e) Diet
        sw.WriteLine("{0,3}\FLOWS", -1)
        For iPrey As Integer = 1 To m_epData.NumGroups
            For iPred As Integer = 1 To m_epData.NumGroups
                Dim cons As Single = Me.CalcConsumption(iPred, iPrey)
                If (cons > 0) Then
                    sw.WriteLine("{0,3}{1,3} {2}", iPrey, iPred, cStringUtils.FormatNumber(cons))
                End If
            Next iPred
        Next iPrey
        sw.WriteLine("{0,3}{0,3}", -1)

        sw.Flush()
        sw.Close()

        Return True

    End Function

#End If

End Class
