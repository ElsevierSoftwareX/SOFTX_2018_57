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
Imports System.Windows.Forms
Imports EwECore
Imports EwECore.DataSources
Imports EwEPlugin
Imports EwEPlugin.Data
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Plug-in point to manage transect persistence
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cTransectStoragePlugin
    Implements IEcospacePlugin
    Implements IEcospaceScenarioAddedOrRemovedPlugin

#Region " Private vars "

    Private m_core As cCore = Nothing
    Private m_data As cTransectDatastructures = Nothing

#End Region ' Private vars

#Region " Foundation "

    Public Sub Initialize(core As Object) Implements IPlugin.Initialize
        Me.m_core = CType(core, cCore)
        Me.m_data = cTransectDatastructures.Instance(Me.m_core)
    End Sub

    Public Sub LoadEcospaceScenario(dataSource As Object) Implements IEcospacePlugin.LoadEcospaceScenario

        Dim ds As IEcospaceDatasource = DirectCast(dataSource, IEcospaceDatasource)
        Dim scenario As cEcospaceScenario = Me.m_core.EcospaceScenarios(Me.m_core.ActiveEcospaceScenarioIndex)
        Dim strDBFileNme As String = Me.TransectFileName(ds, scenario.DBID)

        If Me.m_data.FromXML(strDBFileNme) Then
            Me.m_data.IsChanged = False
        End If

    End Sub

    Public Sub SaveEcospaceScenario(dataSource As Object) Implements IEcospacePlugin.SaveEcospaceScenario

        Dim ds As IEcospaceDatasource = DirectCast(dataSource, IEcospaceDatasource)
        Dim scenario As cEcospaceScenario = Me.m_core.EcospaceScenarios(Me.m_core.ActiveEcospaceScenarioIndex)
        Dim strDBFileNme As String = Me.TransectFileName(ds, scenario.DBID)

        If (Me.m_data.IsChanged()) Then
            If Me.m_data.ToXML(strDBFileNme) Then
                Me.m_data.IsChanged = False
            End If
        End If

    End Sub

    Public Sub CloseEcospaceScenario() Implements IEcospacePlugin.CloseEcospaceScenario
        Me.m_data.Clear()
    End Sub

    Public Sub EcospaceScenarioAdded(dataSource As Object, scenarioID As Integer) _
        Implements IEcospaceScenarioAddedOrRemovedPlugin.EcospaceScenarioAdded
        ' NOP
    End Sub

    Public Sub EcospaceScenarioRemoved(dataSource As Object, scenarioID As Integer) _
        Implements IEcospaceScenarioAddedOrRemovedPlugin.EcospaceScenarioRemoved

        Try
            Dim strFile As String = Me.TransectFileName(DirectCast(dataSource, IEcospaceDatasource), scenarioID)
            If (File.Exists(strFile)) Then File.Delete(strFile)
        Catch ex As Exception

        End Try

    End Sub

    Public ReadOnly Property Name As String Implements IPlugin.Name
        Get
            Return "Transect storage"
        End Get
    End Property

    Public ReadOnly Property Description As String Implements IPlugin.Description
        Get
            Return "This plug-in manages the life span of transect data"
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

#End Region ' Foundation

#Region " Internals "

    Private Function TransectFileName(ds As IEcospaceDatasource, iScenarioID As Integer) As String
        Dim strDB As String = ds.ToString()
        Dim strPath As String = Path.GetDirectoryName(strDB)
        Dim strFile As String = Path.GetFileNameWithoutExtension(strDB) & "_" & iScenarioID & "_transects.xml"
        Return Path.Combine(strPath, strFile)
    End Function

#End Region ' Internals

End Class
