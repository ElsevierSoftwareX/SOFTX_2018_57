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
Imports EwECore.Auxiliary
Imports EwECore.Core
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.Map

#End Region ' Imports

Namespace Style

    ''' <summary>
    ''' Utility class to import / export visual styles from Ecospace layers to file.
    ''' </summary>
    Public Class cImportExportStyle

#Region " Internal admin "

        Public Class cStyleEntry
            Public Sub New(name As String, style As cVisualStyle, varname As eVarNameFlags, index As Integer)
                Me.Name = name
                Me.VisualStyle = style
                Me.VarName = varname
                Me.Index = index
            End Sub
            Public ReadOnly Property Name As String = ""
            Public ReadOnly Property VisualStyle As cVisualStyle = Nothing
            Public ReadOnly Property VarName As eVarNameFlags = eVarNameFlags.NotSet
            Public ReadOnly Property Index As Integer = 0
        End Class

#End Region ' Internal admin

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_dtEntries As New Dictionary(Of String, cStyleEntry)
        Private m_fact As New cLayerFactoryBase()

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(uic As cUIContext)
            Me.m_uic = uic
        End Sub

#End Region ' Constructor

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <vent>pfff</vent>
        ''' -------------------------------------------------------------------
        Public Function Entries() As cStyleEntry()
            Dim lEntries As New List(Of cStyleEntry)
            lEntries.AddRange(Me.m_dtEntries.Values)
            Return lEntries.ToArray()
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Bulk add all layers from a given <see cref="IEcospaceLayerManager">layer manager</see>.
        ''' </summary>
        ''' <param name="man">The layer manager to obtain layers from.</param>
        ''' <param name="vn">Optional variable name to filter by.</param>
        ''' -------------------------------------------------------------------
        Public Sub Add(man As IEcospaceLayerManager, Optional vn As eVarNameFlags = eVarNameFlags.NotSet)
            For Each l As cEcospaceLayer In man.Layers(vn)
                Add(l)
            Next
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a single layer.
        ''' </summary>
        ''' <param name="l"></param>
        ''' -------------------------------------------------------------------
        Public Sub Add(l As cEcospaceLayer)

            If (l Is Nothing) Then Return

            Dim ad As cAuxiliaryData = Me.m_fact.GetAuxillaryData(Me.m_uic.Core, l)
            If (ad Is Nothing) Then Return

            Dim vs As cVisualStyle = ad.VisualStyle
            If (vs Is Nothing) Then Return

            Add(l.Name, vs, l.VarName, l.Index)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Manually add a style definition.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="vs"></param>
        ''' <param name="vn"></param>
        ''' <param name="index"></param>
        ''' -------------------------------------------------------------------
        Public Sub Add(name As String, vs As cVisualStyle, vn As eVarNameFlags, index As Integer)
            Me.m_dtEntries(name) = New cStyleEntry(name, vs, vn, index)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Bulk remove all styles.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub RemoveAll()
            Me.m_dtEntries.Clear()
        End Sub


        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a single layer style.
        ''' </summary>
        ''' <param name="l"></param>
        ''' -------------------------------------------------------------------
        Public Sub Remove(l As cEcospaceLayer)

            If (l Is Nothing) Then Return
            Remove(l.Name)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a single style with a given name.
        ''' </summary>
        ''' <param name="name"></param>
        ''' -------------------------------------------------------------------
        Public Sub Remove(name As String)
            If Me.m_dtEntries.ContainsKey(name) Then Me.m_dtEntries.Remove(name)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load styles from file.
        ''' </summary>
        ''' <param name="file"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function Load(file As String) As Boolean

            ' ToDo: add a whack of error handling
            ' ToDo: serialize as XML

            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim doc As New XmlDocument()

            doc.Load(file)

            For Each cn As XmlElement In doc.GetElementsByTagName("style")
                Dim name As String = cn.GetAttribute("name")
                Dim vn As eVarNameFlags = cin.GetVarName(cn.GetAttribute("varname"))
                Dim index As Integer = CInt(cn.GetAttribute("index"))
                Dim value As String = cn.GetAttribute("data")
                Me.Add(name, cVisualStyleReader.StringToStyle(value), vn, index)
            Next
            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save layers to file.
        ''' </summary>
        ''' <param name="file"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function Save(file As String) As Boolean

            ' ToDo: add a whack of error handling
            ' ToDo: serialize as XML

            If (Me.m_uic Is Nothing) Then Return False

            Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()
            Dim xnRoot As XmlNode = Nothing
            Dim xn As XmlElement = Nothing
            Dim xa As XmlAttribute = Nothing
            Dim doc As XmlDocument = cXMLUtils.NewDoc("styles", xnRoot)

            Dim msg As cMessage = Nothing
            Dim bSuccess As Boolean = True

            For Each name As String In Me.m_dtEntries.Keys

                Dim data As cStyleEntry = Me.m_dtEntries(name)

                xn = doc.CreateElement("style")

                xa = doc.CreateAttribute("name")
                xa.InnerText = name
                xn.Attributes.Append(xa)

                xa = doc.CreateAttribute("varname")
                xa.InnerText = cin.GetVarName(data.VarName)
                xn.Attributes.Append(xa)

                xa = doc.CreateAttribute("index")
                xa.InnerText = CStr(data.Index)
                xn.Attributes.Append(xa)

                xa = doc.CreateAttribute("data")
                xa.InnerText = cVisualStyleReader.StyleToString(data.VisualStyle)
                xn.Attributes.Append(xa)

                xnRoot.AppendChild(xn)
            Next

            Try
                doc.Save(file)
                msg = New cMessage(cStringUtils.Localize(My.Resources.PROMPT_STYLE_SAVE_SUCCESS, file), eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
                msg.Hyperlink = System.IO.Path.GetDirectoryName(file)
            Catch ex As Exception
                msg = New cMessage(cStringUtils.Localize(My.Resources.PROMPT_STYLE_SAVE_FAILED, file, ex.Message), eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
                bSuccess = False
            End Try

            Me.m_uic.Core.Messages.SendMessage(msg)
            Return bSuccess

        End Function

        Public Function MergeToLayers() As Boolean

            Dim bIsChanged As Boolean = False

            '' First create missing layers
            'Me.m_core.SetBatchLock(cCore.eBatchLockType.Restructure)
            'For Each entry As cImportExportStyle.cLayerEntry In io.Entries
            '    Select Case entry.VarName
            '        Case eVarNameFlags.LayerHabitat
            '        Case eVarNameFlags.LayerMPA
            '        Case eVarNameFlags.LayerDriver
            '    End Select
            'Next

            '' This may obliterate me?!
            'Me.m_core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecospace, bIsChanged)

            ' Restyle existing layers
            For Each entry As cImportExportStyle.cStyleEntry In Me.Entries
                Dim layers As cEcospaceLayer() = Me.m_uic.Core.EcospaceBasemap.Layers(entry.VarName)
                For Each l As cEcospaceLayer In layers
                    If (String.Compare(l.Name, entry.Name, True) = 0) And (entry.VisualStyle IsNot Nothing) Then

                        ' ToDo: Throw warning if indexes do not match?

                        Dim ad As cAuxiliaryData = Me.m_fact.GetAuxillaryData(Me.m_uic.Core, l)
                        If (ad.VisualStyle Is Nothing) Then
                            ad.VisualStyle = entry.VisualStyle.Clone()
                        Else
                            ad.AllowValidation = False
                            ad.VisualStyle.Read(entry.VisualStyle)
                            ad.AllowValidation = True
                            ad.Update()
                        End If
                        bIsChanged = True
                        ' Trigger refresh
                        l.Invalidate()
                    End If
                Next
            Next

            Return bIsChanged

        End Function

#End Region ' Public access

    End Class

End Namespace
