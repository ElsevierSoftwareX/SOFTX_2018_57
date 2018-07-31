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
Imports System.Xml
Imports EwECore
Imports EwECore.Core
Imports EwECore.DataSources
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Datastructures for holding <see cref="cTransect">transects</see>.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTransectDatastructures
    Inherits cCoreInputOutputBase
    Implements IEcospaceLayerManager

#Region " Private vars "

    Private Shared Instances As New Dictionary(Of cCore, cTransectDatastructures)

    Private m_transects As New List(Of cTransect)
    Private m_selection As cTransect = Nothing

#End Region ' Private vars

#Region " Singleton "

    Public Shared Function Instance(core As cCore) As cTransectDatastructures
        If (Not Instances.ContainsKey(core)) Then
            Instances(core) = New cTransectDatastructures(core)
        End If
        Return Instances(core)
    End Function

    Protected Sub New(core As cCore)
        MyBase.New(core)
        Me.m_coreComponent = eCoreComponentType.EcoSpace
    End Sub

#End Region ' Singleton

#Region " Events "

    ''' <summary>
    ''' Event to notify that the selected transect has changed.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="transect">The newly selected transect.</param>
    Public Event OnTransectSelected(sender As cTransectDatastructures, transect As cTransect)

    ''' <summary>
    ''' Event to notify that a transect was added.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="transect">The selected that was added.</param>
    Public Event OnTransectAdded(sender As cTransectDatastructures, transect As cTransect)

    ''' <summary>
    ''' Event to notify that a transect was removed.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="transect">The selected that was removed.</param>
    Public Event OnTransectRemoved(sender As cTransectDatastructures, transect As cTransect)

    ''' <summary>
    ''' Event to notify that a transect has been modified.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="transect">The selected that was modified.</param>
    Public Event OnTransectChanged(sender As cTransectDatastructures, transect As cTransect)

#End Region ' Events

#Region " Public access "

    ''' <summary>
    ''' Erase all transects.
    ''' </summary>
    Public Overrides Sub Clear()
        Dim transects() As cTransect = Me.m_transects.ToArray()
        For Each t As cTransect In transects
            Me.Delete(t)
        Next
    End Sub

    Public Sub Add(t As cTransect)
        Me.m_transects.Add(t)
        Try
            RaiseEvent OnTransectAdded(Me, t)
        Catch ex As Exception

        End Try
        If (Me.m_selection Is Nothing) Then Me.Selection = t
    End Sub

    Public Sub Delete(t As cTransect)
        Me.m_transects.Remove(t)
        Try
            RaiseEvent OnTransectRemoved(Me, t)
        Catch ex As Exception

        End Try
        If (ReferenceEquals(Me.m_selection, t)) Then Me.Selection = Nothing
    End Sub

    Public Property Selection As cTransect
        Get
            Return Me.m_selection
        End Get
        Set(value As cTransect)
            If (Not ReferenceEquals(Me.m_selection, value)) Then
                Me.m_selection = value
                Try
                    RaiseEvent OnTransectSelected(Me, Me.m_selection)
                Catch ex As Exception

                End Try
            End If
        End Set
    End Property

    Public Sub OnChanged(t As cTransect)
        Try
            RaiseEvent OnTransectChanged(Me, t)
            Me.IsChanged = True
            Me.m_core.onChanged(Me)
        Catch ex As Exception

        End Try
    End Sub

    Public ReadOnly Property Transects As cTransect()
        Get
            Return Me.m_transects.ToArray()
        End Get
    End Property

    Public Property IsChanged As Boolean = False

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether transect summaries are automatically saved.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Autosaving As Boolean = False

#End Region ' Public access

#Region " Database access "

    Public Function FromXML(strFile As String) As Boolean

        Me.m_selection = Nothing
        Me.m_transects.Clear()

        If Not System.IO.File.Exists(strFile) Then Return True

        Try

            Dim doc As New XmlDocument()
            doc.Load(strFile)

            Me.m_transects.Clear()
            For Each xn As XmlNode In doc.SelectNodes("//transect")

                Dim strName As String = ""
                Dim x0 As Single = 0
                Dim y0 As Single = 0
                Dim x1 As Single = 0
                Dim y1 As Single = 0

                For Each xa As XmlAttribute In xn.Attributes
                    Select Case xa.Name
                        Case "name" : strName = xa.InnerText
                        Case "start_lon" : x0 = cStringUtils.ConvertToSingle(xa.InnerText)
                        Case "start_lat" : y0 = cStringUtils.ConvertToSingle(xa.InnerText)
                        Case "end_lon" : x1 = cStringUtils.ConvertToSingle(xa.InnerText)
                        Case "end_lat" : y1 = cStringUtils.ConvertToSingle(xa.InnerText)
                    End Select
                Next
                Dim t As New cTransect(strName)
                t.Start = New PointF(x0, y0)
                t.End = New PointF(x1, y1)
                Me.m_transects.Add(t)
            Next
        Catch ex As Exception
            cLog.Write(ex, "cTransectDataStructures.Load(" & strFile & ")")
            Return False
        End Try
        Return True

    End Function

    Public Function ToXML(strFile As String) As Boolean

        Try

            Dim xnRoot As XmlNode = Nothing
            Dim xnTransect As XmlNode = Nothing
            Dim xa As XmlAttribute = Nothing
            Dim doc As XmlDocument = cXMLUtils.NewDoc("transects", xnRoot)

            For Each t As cTransect In Me.Transects
                xnTransect = doc.CreateElement("transect")

                xa = doc.CreateAttribute("name")
                xa.InnerText = t.Name
                xnTransect.Attributes.Append(xa)

                xa = doc.CreateAttribute("start_lon")
                xa.InnerText = cStringUtils.FormatNumber(t.Start.X)
                xnTransect.Attributes.Append(xa)

                xa = doc.CreateAttribute("start_lat")
                xa.InnerText = cStringUtils.FormatNumber(t.Start.Y)
                xnTransect.Attributes.Append(xa)

                xa = doc.CreateAttribute("end_lon")
                xa.InnerText = cStringUtils.FormatNumber(t.End.X)
                xnTransect.Attributes.Append(xa)

                xa = doc.CreateAttribute("end_lat")
                xa.InnerText = cStringUtils.FormatNumber(t.End.Y)
                xnTransect.Attributes.Append(xa)

                xnRoot.AppendChild(xnTransect)

            Next

            doc.Save(strFile)

        Catch ex As Exception
            cLog.Write(ex, "cTransectDataStructures.Save(" & strFile & ")")
            Return False
        End Try
        Return True

    End Function

#End Region ' Database access

#Region " IEcospaceLayerManager implementation "

    Public Function Layers(Optional varName As eVarNameFlags = eVarNameFlags.NotSet) As cEcospaceLayer() Implements IEcospaceLayerManager.Layers
        Return Nothing
    End Function

    Public Function Layer(varName As eVarNameFlags, Optional iIndex As Integer = -9999) As cEcospaceLayer Implements IEcospaceLayerManager.Layer
        Return Nothing
    End Function

    Public Function LayerData(varName As eVarNameFlags, iIndex As Integer) As Object Implements IEcospaceLayerManager.LayerData
        If (Me.Selection Is Nothing) Then Return Nothing
        Return Me.Selection.Cells(Me.m_core.EcospaceBasemap)
    End Function

#End Region ' IEcospaceLayerManager implementation

End Class