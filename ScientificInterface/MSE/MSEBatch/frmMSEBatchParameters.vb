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
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core

#End Region ' Imports


Public Class frmMSEBatchParameters

    ' ToDo: Add XML comments

    ' Properties to monitor for setting radio button check states
    Private WithEvents m_fpBiomass As cPropertyFormatProvider = Nothing
    Private WithEvents m_fpCatch As cPropertyFormatProvider = Nothing
    Private WithEvents m_fpQB As cPropertyFormatProvider = Nothing
    Private WithEvents m_fpF As cPropertyFormatProvider = Nothing
    Private WithEvents m_fpPred As cPropertyFormatProvider = Nothing
    Private WithEvents m_fpFeeding As cPropertyFormatProvider = Nothing

    Private m_batchManager As EwECore.MSEBatchManager.cMSEBatchManager

    Public Sub New()
        MyBase.New()
        Me.InitializeComponent()
    End Sub

    Private Sub EcospaceParameters_Load(ByVal sender As Object, ByVal e As System.EventArgs) _
          Handles Me.Load
     

        Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.MSE}

        Me.m_batchManager = Me.UIContext.Core.MSEBatchManager

        Dim pm As cPropertyManager = Me.PropertyManager

        'Biomass()
        'QB() 'consumption/biomass
        'FeedingTime()
        'FishingMortRate()
        'PredRate()
        'CatchByGroup()
        Me.m_fpBiomass = New cPropertyFormatProvider(Me.UIContext, Me.chkSaveBiomass, Me.m_batchManager.Parameters, eVarNameFlags.MSEBatchOutputBiomass)
        Me.m_fpCatch = New cPropertyFormatProvider(Me.UIContext, Me.chkCatch, Me.m_batchManager.Parameters, eVarNameFlags.MSEBatchOutputCatch)
        Me.m_fpF = New cPropertyFormatProvider(Me.UIContext, Me.chkFishingMort, Me.m_batchManager.Parameters, eVarNameFlags.MSEBatchOutputFishingMortRate)
        Me.m_fpPred = New cPropertyFormatProvider(Me.UIContext, Me.chkPredMort, Me.m_batchManager.Parameters, eVarNameFlags.MSEBatchOutputPredRate)
        Me.m_fpQB = New cPropertyFormatProvider(Me.UIContext, Me.chkQB, Me.m_batchManager.Parameters, eVarNameFlags.MSEBatchOutputConBio)
        Me.m_fpFeeding = New cPropertyFormatProvider(Me.UIContext, Me.chkFeedingTime, Me.m_batchManager.Parameters, eVarNameFlags.MSEBatchOutputFeedingTime)

        Me.m_lbOutputDir.Text = m_batchManager.Parameters.OutputDir

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        Me.m_fpBiomass = Nothing


        MyBase.OnFormClosed(e)
    End Sub

End Class