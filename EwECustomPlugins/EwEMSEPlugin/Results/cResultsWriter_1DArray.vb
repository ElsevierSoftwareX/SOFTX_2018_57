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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

Option Strict Off
Imports System.IO
Imports EwEUtils.Utilities
Imports EwECore
Imports EwEUtils.Core

Public Class cResultsWriter_1DArray
    Inherits cResultsWriter_Base

    Protected m_ResultsArray As cResultsCollector_1DArray
    Protected m_StreamWriters As List(Of StreamWriter)


    Public Overrides Sub Initialise(msgReport As EwECore.cMessage, MSE As cMSE, Results_Array As cResultsCollector_Base, FolderPath As cMSEUtils.eMSEPaths)

        Dim strFile As String
        Dim writer As StreamWriter

        m_ResultsArray = Results_Array

        m_MSE = MSE
        m_Core = MSE.Core
        m_StreamWriters = New List(Of StreamWriter)

        For iElement = 1 To m_ResultsArray.nElements
            strFile = cFileUtils.ToValidFileName(m_ResultsArray.FileNamePrefix & m_ResultsArray.ElementName(iElement) & "_" & m_ResultsArray.Dim_Name & "No" & iElement & ".csv", False)

            writer = cMSEUtils.GetWriter(cMSEUtils.MSEFile(MSE.DataPath, FolderPath, strFile))
            msgReport.AddVariable(New cVariableStatus(eStatusFlags.OK, String.Format(My.Resources.STATUS_SAVED_DETAIL, strFile), eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, 0))

            Debug.Assert(writer IsNot Nothing)

            'Setup the HCR F Targ file for igrp
            m_StreamWriters.Add(writer)
            If Me.m_Core.SaveWithFileHeader Then m_StreamWriters(iElement - 1).WriteLine(Me.m_Core.DefaultFileHeader(eAutosaveTypes.Ecosim))
            m_StreamWriters(iElement - 1).Write(m_ResultsArray.Dim_Name & "Name,ModelID,StrategyName,ResultType")
            For iTime As Integer = 1 To m_ResultsArray.NumberOfTimeRecords
                m_StreamWriters(iElement - 1).Write("," & cStringUtils.FormatNumber(iTime))
            Next
            m_StreamWriters(iElement - 1).WriteLine()

        Next

    End Sub

    Public Overrides Sub ReleaseWriters()
        For Each iStreamWriter In m_StreamWriters
            cMSEUtils.ReleaseWriter(iStreamWriter)
        Next
        m_StreamWriters.Clear()
    End Sub

    Public Overrides Sub WriteResults()

        For iElement = 1 To m_ResultsArray.nElements
            For iStrategy = 1 To m_ResultsArray.nStrategies
                If m_MSE.Strategies(iStrategy - 1).RunThisStrategy = False Then Continue For
                m_StreamWriters(iElement - 1).Write("{0},{1},{2},{3}",
                       cStringUtils.ToCSVField(m_ResultsArray.ElementName(iElement)),
                       cStringUtils.FormatNumber(m_ResultsArray.ModelID),
                       cStringUtils.ToCSVField(StrategyName(iStrategy)),
                       cStringUtils.ToCSVField(m_ResultsArray.DataName))
                For iTime = 1 To m_ResultsArray.NumberOfTimeRecords
                    m_StreamWriters(iElement - 1).Write("," & m_ResultsArray.GetValue_Formatted4CSV(iStrategy, iElement, iTime))
                Next
                m_StreamWriters(iElement - 1).WriteLine()
            Next
        Next

    End Sub
End Class
