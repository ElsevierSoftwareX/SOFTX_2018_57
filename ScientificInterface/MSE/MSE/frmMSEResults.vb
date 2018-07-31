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

Imports EwECore
Imports EwECore.MSE
Imports EwEUtils.Core

#End Region

Public Class frmMSEResults

    Private m_EventSource As cMSEEventSource

    Public Sub New()
        MyBase.New()
        InitializeComponent()
        m_EventSource = New cMSEEventSource
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.rbFleet.Tag = ScientificInterface.gridRiskResults.eGridType.Fleet
        Me.rbGroup.Tag = ScientificInterface.gridRiskResults.eGridType.Group

        Me.m_grid.UIContext = Me.UIContext

        Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.MSE}

        AddHandler Me.m_EventSource.onRefLevelsChanged, AddressOf Me.onRefLevelsChanged
        AddHandler Me.m_EventSource.onRunCompleted, AddressOf Me.onRunCompleted

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

        RemoveHandler Me.m_EventSource.onRefLevelsChanged, AddressOf Me.onRefLevelsChanged
        RemoveHandler Me.m_EventSource.onRunCompleted, AddressOf Me.onRunCompleted
        MyBase.OnFormClosed(e)

    End Sub

    ''' <summary>
    ''' Reference levels have changed! For now just update the m_grid
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub onRefLevelsChanged()
        Try
            Me.m_grid.Update()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub onGridTypeCheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles rbGroup.CheckedChanged, rbFleet.CheckedChanged
        Try
            Dim rb As RadioButton = DirectCast(sender, RadioButton)
            If rb.Checked Then
                Me.m_grid.GridType = DirectCast(rb.Tag, ScientificInterface.gridRiskResults.eGridType)
            End If
        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' Stats data has changed. For now just update the m_grid
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub onRunCompleted()
        Try
            Me.m_grid.Update()
        Catch ex As Exception

        End Try
    End Sub

#Region " Core interactions "

    Public Overrides Sub OnCoreMessage(ByVal msg As cMessage)
        Try
            Me.m_EventSource.HandleCoreMessage(msg)
        Catch ex As Exception

        End Try
    End Sub

#End Region

End Class