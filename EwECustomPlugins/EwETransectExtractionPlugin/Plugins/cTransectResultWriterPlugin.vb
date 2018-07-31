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
Imports System.Drawing
Imports System.IO
Imports EwECore
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Plug-in point that implements a transect summary Ecospace result writer.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTransectResultWriterPlugin
    Inherits cEcospaceBaseResultsWriter
    Implements IEcospaceResultWriterPlugin

    Private m_data As cTransectDatastructures = Nothing

    Public Sub New()
        MyBase.New()
    End Sub

    Public Overrides Sub Init(theCore As Object) Implements IPlugin.Initialize
        MyBase.Init(theCore)
        Me.m_core = DirectCast(theCore, cCore)
        Me.m_data = cTransectDatastructures.Instance(Me.m_core)
    End Sub

    Public Overrides ReadOnly Property DisplayName As String
        Get
            Return "Transect CSV summary"
        End Get
    End Property

    Public ReadOnly Property Name As String Implements IPlugin.Name
        Get
            Return "Transect result writer"
        End Get
    End Property

    Public ReadOnly Property Description As String Implements IPlugin.Description
        Get
            Return "Provides discoverable Ecospace result writer to store transect data"
        End Get
    End Property

    Public ReadOnly Property Author As String Implements IPlugin.Author
        Get
            Return "Jeroen Steenbeek"
        End Get
    End Property

    Public ReadOnly Property Contact As String Implements IPlugin.Contact
        Get
            Return "ewedevteam@gmail.com"
        End Get
    End Property

    Public Overrides Sub StartWrite()
        ' Transect Initialization is already performed by cTransectRecorderPlugin
        'For Each t As cTransect In Me.m_data.Transects
        '    t.InitRun(Me.m_core)
        'Next
    End Sub

    Public Overrides Sub WriteResults(SpaceTimeStepResults As Object)
        ' Transect Recording is already performed by cTransectRecorderPlugin
        'For Each t As cTransect In Me.m_data.Transects
        '    t.Record(DirectCast(SpaceTimeStepResults, cEcospaceTimestep))
        'Next
    End Sub

    Public Overrides Sub EndWrite()

        ' ToDo: globalize this method

        Dim msg As cMessage = Nothing

        Try
            ' Create output dir
            Me.CreateOutputDir()
            ' Write it all
            Me.WriteResult()

            ' Notify user
            msg = New cMessage("Ecospace results across transects have been saved to '" & Me.OutputDirectory & "'",
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
            msg.Hyperlink = Me.OutputDirectory
        Catch ex As Exception
            ' Notify user of error
            msg = New cMessage("Ecospace results across transects could not be saved to '" & Me.OutputDirectory & "'. " & ex.Message,
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
        End Try
        ' Done
        Me.m_core.Messages.SendMessage(msg)

    End Sub

    Protected Overrides Function FileExtension() As String
        Return ".csv"
    End Function

#Region " Internals "

    Private Sub WriteResult()

        Dim sw As StreamWriter = Nothing
        Dim strName As String = ""
        Dim strFile As String = ""
        Dim strDescriptor As String = ""
        Dim sValue As Single = 0

        For Each t As cTransect In Me.m_data.Transects

            Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
            Dim cells As Point() = t.Cells(bm)
            If (t.NumCells > 0) Then

                strFile = Me.FileName(t)
                Try
                    ' Start writing
                    sw = New StreamWriter(Path.Combine(Me.OutputDirectory, strFile))
                    If Me.m_core.SaveWithFileHeader Then
                        sw.WriteLine(Me.m_core.DefaultFileHeader(eAutosaveTypes.Ecospace))
                        sw.WriteLine(cStringUtils.ToCSVField("Transect") & "," & cStringUtils.ToCSVField(t.Name))
                        sw.WriteLine(cStringUtils.ToCSVField("x0") + "," & cells(0).X)
                        sw.WriteLine(cStringUtils.ToCSVField("y0") + "," & cells(0).Y)
                        sw.WriteLine(cStringUtils.ToCSVField("x1") + "," & cells(t.NumCells - 1).X)
                        sw.WriteLine(cStringUtils.ToCSVField("y1") + "," & cells(t.NumCells - 1).Y)
                        sw.WriteLine(cStringUtils.ToCSVField("Number of cells") & "," & t.NumCells)
                        sw.WriteLine()
                    End If

                    Me.WriteData(sw, t)

                    ' Clean up
                    sw.Flush()
                    sw.Close()
                    sw.Dispose()

                Catch ex As Exception
                    cLog.Write(ex, "Failed to write Ecospace average biomass to file for data " + strDescriptor)
                End Try
            End If

        Next

    End Sub

    Private Function FileName(t As cTransect) As String
        Return cFileUtils.ToValidFileName("Ecospace_" & t.Name & ".csv", False)
    End Function

    Private Sub WriteData(sw As StreamWriter, t As cTransect)

        If (t Is Nothing) Then Return

        Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
        Dim cells As Point() = t.Cells(bm)
        Dim IsDataCell(cells.Count) As Boolean

        ' Assess once per transect
        For i As Integer = 0 To t.NumCells - 1
            Dim pt As Point = cells(i)
            IsDataCell(i) = bm.IsModelledCell(pt.Y, pt.X)
        Next

        ' Write data header
        sw.WriteLine("Timestep,row,col,group,biomass,catch")

        Dim vars() As cTransect.eSummaryType = CType([Enum].GetValues(GetType(cTransect.eSummaryType)), cTransect.eSummaryType())

        For iTime As Integer = Me.FirstOutputTimeStep To Me.m_core.nEcospaceTimeSteps
            For iGroup As Integer = 1 To Me.m_core.nGroups
                For iCell As Integer = 0 To cells.Count - 1
                    If (IsDataCell(iCell)) Then
                        Dim cell As Point = cells(iCell)
                        Dim vals(vars.Count - 1) As Single
                        Dim bWrite As Boolean = False
                        For Each var As cTransect.eSummaryType In vars
                            Dim s As cTransectSummary = t.Summary(iTime, iGroup, var)
                            If (s IsNot Nothing) Then
                                bWrite = True
                                vals(var) = s.Value(iCell)
                            End If
                        Next var
                        If bWrite Then
                            sw.WriteLine("{0},{1},{2},{3},{4},{5}", iTime, cell.Y, cell.X, iGroup, cStringUtils.FormatNumber(vals(0)), cStringUtils.FormatNumber(vals(1)))
                        End If
                    End If
                Next iCell
            Next iGroup
        Next iTime

    End Sub

#End Region ' Internals

End Class
