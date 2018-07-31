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
Imports System.Xml
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' <para>Helper class that loads and saves core autosave settings from a XML 
''' document for persistent storage.</para>
''' </summary>
''' ===========================================================================
Friend Class cAutosaveSettingsManager
    Implements IDisposable

#Region " Private vars "

    Private m_formats As New Dictionary(Of eAutosaveTypes, String)
    Private m_core As cCore = Nothing

#End Region ' Private vars

    Public Sub New(core As cCore)
        Me.m_core = core
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If (Me.m_core IsNot Nothing) Then
            Me.m_formats.Clear()
            Me.m_core = Nothing
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set autosave settings as a XML document for persistent storage.
    ''' </summary>
    ''' <returns>A penguin. Really.</returns>
    ''' -----------------------------------------------------------------------
    Public Property Settings() As XmlDocument
        Get
            ' Just in case
            GatherSettings()

            Dim doc As New XmlDocument()
            Dim node As XmlNode = Nothing
            Dim nodeChild As XmlNode = Nothing
            Dim att As XmlAttribute = Nothing

            node = doc.CreateXmlDeclaration("1.0", "utf-16", Nothing)
            doc.AppendChild(node)

            node = doc.CreateElement("autosavesettings")
            doc.AppendChild(node)

            ' For every autosave type
            For Each t As eAutosaveTypes In [Enum].GetValues(GetType(eAutosaveTypes))
                Try
                    nodeChild = doc.CreateElement(t.ToString)

                    att = doc.CreateAttribute("Enabled")
                    att.InnerText = m_core.Autosave(t).ToString
                    nodeChild.Attributes.Append(att)

                    Select Case t
                        Case eAutosaveTypes.EcospaceResults
                            att = doc.CreateAttribute("Format")
                            If (Me.m_formats.ContainsKey(t)) Then
                                att.InnerText = Me.m_formats(t)
                            End If
                            nodeChild.Attributes.Append(att)
                        Case Else
                            ' NOP
                    End Select

                    node.AppendChild(nodeChild)

                Catch ex As Exception

                End Try
            Next
            Return doc
        End Get
        Set(settings As XmlDocument)

            Dim node As XmlNode = Nothing
            Dim att As XmlAttribute = Nothing

            ' Sanity checks
            If (settings Is Nothing) Then Return
            If (settings.ChildNodes.Count = 0) Then Return

            ' For every autosave type
            For Each t As eAutosaveTypes In [Enum].GetValues(GetType(eAutosaveTypes))
                Try
                    ' Find node
                    node = settings.SelectSingleNode("/autosavesettings/" & t.ToString)
                    ' Is valid?
                    If (node IsNot Nothing) Then
                        ' #Yes: plunder content
                        att = node.Attributes("Enabled")
                        If (att IsNot Nothing) Then
                            Me.m_core.Autosave(t) = Boolean.Parse(att.InnerText)
                        Else
                            Me.m_core.Autosave(t) = False
                        End If

                        att = node.Attributes("Format")
                        If (att IsNot Nothing) Then
                            Me.m_formats(t) = att.InnerText
                        Else
                            Me.m_formats(t) = ""
                        End If
                    End If
                Catch ex As Exception
                    cLog.Write(ex, "cAutosaveSettingsHelper.LoadFromSettings(" & t.ToString & ")")
                End Try
            Next
        End Set
    End Property

    ''' <summary>
    ''' Ensure that autosave writers are configured, and that modules have proper defaults.
    ''' </summary>
    Public Sub ApplySettingsAndEnsureDefaults()

        ' Ecosim
        If (Me.m_core.ActiveEcosimScenarioIndex > 0) And (Me.m_core.Autosave(eAutosaveTypes.MonteCarlo) = True) Then
            Dim man As cMonteCarloManager = m_core.EcosimMonteCarlo
            Dim strFormat As String = m_formats(eAutosaveTypes.MonteCarlo)
            For n As Integer = 1 To man.nResultWriters
                Dim writer As IMonteCarloResultsWriter = man.ResultWriter(n)
                If (strFormat = cTypeUtils.TypeToString(writer.GetType())) Or
                   (String.IsNullOrWhiteSpace(strFormat) And man.ActiveResultWriter Is Nothing) Then
                    man.ActiveResultWriter = writer
                End If
            Next
        End If

        ' Ecospace
        If (m_core.ActiveEcospaceScenarioIndex > 0) And (m_core.Autosave(eAutosaveTypes.EcospaceResults) = True) Then
            Dim parms As cEcospaceModelParameters = m_core.EcospaceModelParameters
            Dim strFormat As String = m_formats(eAutosaveTypes.EcospaceResults)
            Dim bits As String() = strFormat.Split(";"c)
            For n As Integer = 1 To parms.nResultWriters
                Dim writer As IEcospaceResultsWriter = parms.ResultWriter(n)
                writer.Enabled = bits.Contains(cTypeUtils.TypeToString(writer.GetType())) Or
                                 (String.IsNullOrWhiteSpace(strFormat) And TypeOf (writer) Is cEcospaceASCMapBiomassWriter)
            Next
        End If

    End Sub

    Public Sub GatherSettings()

        ' Ecosim
        If (Me.m_core.ActiveEcosimScenarioIndex > 0) Then
            Dim man As cMonteCarloManager = Me.m_core.EcosimMonteCarlo
            Dim strFormat As String = ""
            If (man.ActiveResultWriter IsNot Nothing) Then
                strFormat = cTypeUtils.TypeToString(man.ActiveResultWriter.GetType())
            End If
            Me.m_formats(eAutosaveTypes.MonteCarlo) = strFormat
        End If

        ' Ecospace
        If (Me.m_core.ActiveEcospaceScenarioIndex > 0) Then
            Dim parms As cEcospaceModelParameters = Me.m_core.EcospaceModelParameters
            Dim strFormat As String = ""
            For n As Integer = 1 To parms.nResultWriters
                Dim writer As IEcospaceResultsWriter = parms.ResultWriter(n)
                If writer.Enabled Then
                    If Not String.IsNullOrWhiteSpace(strFormat) Then strFormat &= ";"
                    strFormat &= cTypeUtils.TypeToString(writer.GetType())
                End If
            Next
            Me.m_formats(eAutosaveTypes.EcospaceResults) = strFormat
        End If

    End Sub

End Class