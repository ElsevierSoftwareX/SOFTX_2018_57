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
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Writer to save Ecopath estimates to a CSV file.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcopathResultWriter

#Region " Private vars "

    Private m_core As cCore = Nothing
    Private m_data As cEcopathDataStructures = Nothing

#End Region ' Private vars

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="core">The core instance to write result for.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore)
        Me.m_core = core
        Me.m_data = core.m_EcoPathData
    End Sub

#End Region ' Constructor

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Write Ecopath estimates to a CSV file.
    ''' </summary>
    ''' <param name="strFN">The target file name.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function WriteCSV(strFN As String) As Boolean

        ' Extracted this logic from the Ecopath datastructures 'Dump' method
        If Not cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFN)) Then Return False

        Dim sw As StreamWriter = Nothing
        Dim bSuccess As Boolean = True

        Try
            sw = New StreamWriter(strFN)
        Catch ex As Exception
            bSuccess = False
        End Try

        If (sw IsNot Nothing) Then

            If Me.m_core.SaveWithFileHeader Then
                sw.Write(Me.m_core.DefaultFileHeader(EwEUtils.Core.eAutosaveTypes.Ecopath))
                sw.WriteLine()
            End If

            sw.WriteLine("Group,""Biomass(B)"",""Prod/Biomass(PB)"",""Cons/Biomass(QB)"",""Ecotrophic eff.(EE)"",""Prod/Consum(GE)""")
            For i As Integer = 1 To Me.m_data.NumGroups
                sw.Write(cStringUtils.ToCSVField(Me.m_data.GroupName(i)))
                sw.Write(",")
                sw.Write(cStringUtils.FormatSingle(Me.m_data.B(i)))
                sw.Write(",")
                sw.Write(cStringUtils.FormatSingle(Me.m_data.PB(i)))
                sw.Write(",")
                sw.Write(cStringUtils.FormatSingle(Me.m_data.QB(i)))
                sw.Write(",")
                sw.Write(cStringUtils.FormatSingle(Me.m_data.EE(i)))
                sw.Write(",")
                sw.Write(cStringUtils.FormatSingle(Me.m_data.GE(i)))
                sw.WriteLine()
            Next
            sw.Flush()
            sw.Close()
        Else
            bSuccess = False
            cLog.Write(Me.ToString + ".WriteCSV() failed to open file.")
        End If
        Return bSuccess

    End Function

#End Region ' Public access

End Class
