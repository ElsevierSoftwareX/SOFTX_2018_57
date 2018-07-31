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

Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Forms

Imports EwECore.MSE
Imports ZedGraph


Public Class frmMSERunBatch

    ' ToDo: Add XML comments

    Private m_BatchManager As EwECore.MSEBatchManager.cMSEBatchManager
    Private m_MSE As EwECore.MSE.cMSEManager
    Private m_zgh As cZedGraphHelper

    Public Sub New()
        MyBase.New()
        Me.InitializeComponent()
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        m_BatchManager = Me.UIContext.Core.MSEBatchManager
        m_MSE = Me.UIContext.Core.MSEManager
        m_zgh = New cZedGraphHelper()
        m_zgh.Attach(Me.UIContext, Me.m_ZedGraph)

        Me.m_BatchManager.onMessageDelegate = AddressOf Me.onMSEBatchMessage

    End Sub


    Private Sub btRunBatch_Click(sender As Object, e As System.EventArgs) Handles btRunBatch.Click

        Me.lstMsgs.Items.Clear()

        If Not Me.m_zgh.IsAttached Then
            Me.m_zgh.Attach(Me.UIContext, Me.m_ZedGraph)
        End If

        Me.m_zgh.GetPane(1).CurveList.Clear()

        ' Me.m_BatchManager.setDefaults()
        Me.m_BatchManager.Connect(AddressOf Me.onProgress)
        Me.m_BatchManager.Run()
        Me.m_BatchManager.DisConnect()

    End Sub

    Private Sub onProgress(ByVal ProgressEnum As EwECore.MSEBatchManager.eMSEBatchProgress)

        Try

            Select Case ProgressEnum

                Case EwECore.MSEBatchManager.eMSEBatchProgress.MSEIteration
                    Me.plotMean(Me.m_MSE.BiomassStats(4), 1)

                Case EwECore.MSEBatchManager.eMSEBatchProgress.RunStarted
                    Me.m_btStop.Enabled = True
                    Me.btRunBatch.Enabled = False

                Case EwECore.MSEBatchManager.eMSEBatchProgress.RunCompleted
                    Me.m_btStop.Enabled = False
                    Me.btRunBatch.Enabled = True


            End Select
        Catch ex As Exception

        End Try



        ' Next

    End Sub

    Private Sub plotMean(ByVal StatsData As cMSEStats, ByVal ipane As Integer)
        Dim x As Double, dx As Double
        Dim ppl As PointPairList = Nothing
        Dim li As LineItem = Nothing

        'time varing mean
        ppl = New PointPairList()
        x = Me.UIContext.Core.EcosimFirstYear
        dx = 1 / StatsData.nStepsPerYear
        For iTime As Integer = 1 To StatsData.nTimeSteps
            ppl.Add(x, StatsData.Mean(iTime))
            x += dx
        Next
        li = Me.m_zgh.CreateLineItem("", eSketchDrawModeTypes.NotSet, Color.Blue, ppl)
        li.Line.Width = 1

        Me.m_zgh.GetPane(ipane).CurveList.Add(li)
        Me.m_zgh.RescaleAndRedraw()

    End Sub

    Private Sub onMSEBatchMessage(msg As String)
        Me.lstMsgs.Items.Add(msg)
    End Sub

    Private Sub frmMSERunBatch_Activated(sender As Object, e As System.EventArgs) Handles Me.Activated
        If Not Me.m_zgh.IsAttached Then
            ' Me.m_zgh.Attach(Me.UIContext, Me.m_ZedGraph)
        End If
    End Sub

    Private Sub m_btStop_Click(sender As System.Object, e As System.EventArgs) Handles m_btStop.Click
        Me.m_BatchManager.StopRun()
    End Sub
End Class