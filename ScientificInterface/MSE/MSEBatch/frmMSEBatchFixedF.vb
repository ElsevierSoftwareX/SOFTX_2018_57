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

Option Explicit On
Option Strict On

Imports EwECore
Imports EwEUtils.Core
Imports EwECore.MSEBatchManager

Public Class frmMSEBatchFixedF

    ' ToDo: Add XML comments

    Private m_BatchManager As EwECore.MSEBatchManager.cMSEBatchManager

    Public Sub New()
        MyBase.New()
        Me.InitializeComponent()
    End Sub

    Public Overrides Property UIContext As ScientificInterfaceShared.Controls.cUIContext
        Get
            Return MyBase.UIContext
        End Get
        Set(ByVal value As ScientificInterfaceShared.Controls.cUIContext)
            MyBase.UIContext = value
            Me.grdGroups.UIContext = Me.UIContext
            Me.grdIters.UIContext = Me.UIContext
        End Set
    End Property

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        If (Me.UIContext Is Nothing) Then Return

        m_BatchManager = Me.UIContext.Core.MSEBatchManager

        Me.txNFixedF.Text = NIters.ToString

        Me.rbCalcTypePercent.Tag = eMSEBatchIterCalcTypes.Percent
        Me.rbCalcTypeValue.Tag = eMSEBatchIterCalcTypes.UpperLowerValues

        UpdateControls()

        Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.MSE, eCoreComponentType.EcoSim}


    End Sub


    Private Property NIters As Integer
        Get
            Return Me.m_BatchManager.Parameters.nFixedFIteration
        End Get
        Set(value As Integer)
            Me.m_BatchManager.Parameters.nFixedFIteration = value
        End Set
    End Property


    Private Sub txNTFM_TextChanged(sender As System.Object, e As System.EventArgs) Handles txNFixedF.TextChanged

        Dim newValue As Integer = Integer.Parse(Me.txNFixedF.Text)
        If newValue > 0 And newValue <> Me.NIters Then
            Me.NIters = newValue
        End If

    End Sub

    Private Sub onCalcIterValues(sender As Object, e As System.EventArgs) Handles btCalcIters.Click

        Me.m_BatchManager.CalculateFIterationValues()

    End Sub

    Private Sub UpDwnIter_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs)

    End Sub


    Private Sub UpDwnIter_ValueChanged(sender As System.Object, e As System.EventArgs) Handles UpDwnIter.ValueChanged
        Dim iter As Integer = CInt(Me.UpDwnIter.Value)
        If Me.m_BatchManager Is Nothing Then Exit Sub
        If iter <= Me.NIters Then
            Me.grdGroups.iCurIter = iter
        End If
    End Sub


    Private Sub OnIterCalcTypeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
          Handles rbCalcTypePercent.CheckedChanged, rbCalcTypeValue.CheckedChanged

        Try

            Dim rb As RadioButton = DirectCast(sender, RadioButton)

            If rb.Tag IsNot Nothing Then

                If rb.Checked Then
                    ' Me.m_BatchManager.Parameters.IterCalcType = DirectCast(rb.Tag, EwEUtils.Core.eMSEBatchIterCalcTypes)
                    Me.m_BatchManager.Parameters.IterCalcType = DirectCast(rb.Tag, Integer)

                    Me.grdGroups.RefreshContent()
                End If
            End If
        Catch ex As Exception

        End Try

    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()

        Dim pars As cMSEBatchParameters = Me.m_BatchManager.Parameters
        Me.rbCalcTypePercent.Checked = (pars.IterCalcType = eMSEBatchIterCalcTypes.Percent)
        Me.rbCalcTypeValue.Checked = (pars.IterCalcType = eMSEBatchIterCalcTypes.UpperLowerValues)

        For igrp As Integer = 1 To Me.UIContext.Core.nGroups
            Dim grp As cEcoPathGroupInput = Me.UIContext.Core.EcoPathGroupInputs(igrp)
            If grp.IsFished Then
                Me.cbGroups.Items.Add(New cCoreInputOutputControlItem(grp))
            End If
        Next

    End Sub


    Private Sub cbGroups_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbGroups.SelectedIndexChanged
        If Me.m_BatchManager Is Nothing Then Exit Sub
        Dim grp As cCoreInputOutputBase = DirectCast(Me.cbGroups.SelectedItem, cCoreInputOutputControlItem).Source
        Me.grdIters.iSelGroup = grp.Index
    End Sub


    Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
        Dim bRefresh As Boolean
        Select Case msg.Source

            Case eCoreComponentType.MSE

                If msg.DataType = eDataTypes.MSEBatchFixedFInput Then
                    'Something has update the TFM inputs

                    If Me.grdGroups.iCurIter > Me.NIters Then
                        Me.UpDwnIter.Maximum = Me.NIters
                        Me.UpDwnIter.Value = Me.NIters
                        Me.grdGroups.iCurIter = Me.NIters
                    End If

                    If msg.Type = eMessageType.MSEBatch_IterationDataUpdated Then
                        bRefresh = True
                    End If

                    'Has one of the iteration values has been edited
                    For Each var As cVariableStatus In msg.Variables
                        If var.VarName = eVarNameFlags.MSEBatchFValues Then
                            bRefresh = True
                            Exit For
                        End If
                    Next

                    If bRefresh Then
                        Me.grdGroups.RefreshContent()
                        Me.grdIters.RefreshContent()
                    End If

                End If


        End Select

    End Sub


End Class