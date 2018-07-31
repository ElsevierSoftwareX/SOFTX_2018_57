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
Imports EwECore
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities
Imports System.Text

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Form, implements the main MSY search interface.
''' </summary>
''' ===========================================================================
Public Class frmMSY

    Private m_mse As MSE.cMSEManager
    Private m_nFleets As Integer
    Private MSY() As Single

    Public Sub New()
        Me.InitializeComponent()
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_mse = Me.UIContext.Core.MSEManager
        Me.m_rbValue.Checked = True

    End Sub

    Private Sub OnRun(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnRunMSY.Click

        cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_MSE_INITIALIZING, -1)
        Try

            ' Hard-wire run state parameters...for now
            Me.m_mse.ModelParameters.MSYEvaluateValue = m_rbValue.Checked
            Me.m_mse.ModelParameters.MSYRunSilent = False
            Me.m_mse.ModelParameters.MSYStartTimeIndex = 2

            'get the number of fleets for the progress updates
            m_nFleets = Me.UIContext.Core.nFleets
            ReDim MSY(Me.UIContext.Core.nFleets)
            Me.UpdateControls(True)

            'connect and disconnect every time we run the MSY
            Me.m_mse.Connect(Nothing, AddressOf Me.OnMSYProgress)
            Me.m_mse.RunMSYSearch(True)
            Me.m_mse.Disconnect()

        Catch ex As Exception

        End Try
        cApplicationStatusNotifier.EndProgress(Me.Core)
        Me.UpdateControls(False)

    End Sub

    Private Shadows Sub UpdateControls(ByVal bRunning As Boolean)

        If bRunning Then
            Me.m_btnRunMSY.Enabled = False
            Me.m_btnStop.Enabled = True
        Else
            Me.m_btnRunMSY.Enabled = True
            Me.m_btnStop.Enabled = False

            Dim sb As New StringBuilder()
            sb.AppendLine(cStringUtils.Localize(My.Resources.MSE_ITERATION_HEADER, cStringUtils.vbTab))
            For i As Integer = 1 To Me.UIContext.Core.nFleets
                sb.AppendLine(cStringUtils.Localize(My.Resources.MSE_ITERATION_LINE, cStringUtils.vbTab, i, Me.StyleGuide.FormatNumber(MSY(i))))
            Next
            Me.txtMSYresults.Text = sb.ToString
        End If

    End Sub

    Private Sub OnMSYProgress(ByVal MSYProgress As MSE.cMSYProgressArgs)

        Try
            Me.m_lbFleet.Text = cStringUtils.Localize(SharedResources.GENERIC_VALUE_FLEET_OF_N, MSYProgress.FleetIndex, m_nFleets)
            Me.m_lblIter.Text = cStringUtils.Localize(SharedResources.GENERIC_VALUE_ITERATION, MSYProgress.Iteration)
            Me.m_lblEffort.Text = cStringUtils.Localize(My.Resources.MSE_EFFORT_VALUE, Me.StyleGuide.FormatNumber(MSYProgress.CurrentEffort))
            If MSYProgress.CurrentEffort > 0 Then MSY(MSYProgress.FleetIndex) = MSYProgress.CurrentEffort

            cApplicationStatusNotifier.UpdateProgress(Me.Core, cStringUtils.Localize(SharedResources.GENERIC_VALUE_FLEET_OF_N, MSYProgress.FleetIndex, m_nFleets), CSng(MSYProgress.FleetIndex / m_nFleets))

            'the DoEvents can be removed once the MSY is running on a thread 
            Application.DoEvents()
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnStop(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnStop.Click
        Me.m_mse.StopRun(0)
    End Sub

    Private Sub btFleetTradeoffs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnFleetTradeoffs.Click

        Try
            'get the number of fleets for the progress updates
            m_nFleets = Me.UIContext.Core.nFleets

            'connect and disconnect every time we run the MSY
            Me.m_mse.Connect(Nothing, AddressOf Me.OnMSYProgress)
            Me.m_mse.FleetTradeoffs()
            Me.m_mse.Disconnect()

            MessageBox.Show(SharedResources.GENERIC_LABEL_FINISHED)

        Catch ex As Exception

        End Try

    End Sub

End Class