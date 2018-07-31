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
Imports ScientificInterface.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Core

#End Region ' Imports

Namespace Ecotracer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Form implementing the main input interface for contaminant tracing.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class frmEcotracerInput

#Region " Private vars "

        Private m_fpCZeroEnv As cEwEFormatProvider = Nothing
        Private m_fpCInflowEnv As cEwEFormatProvider = Nothing
        Private m_fpCOutflowEnv As cEwEFormatProvider = Nothing
        Private m_fpCDecayEnv As cEwEFormatProvider = Nothing
        Private m_fpInflowForceNumberEnv As cEwEFormatProvider = Nothing

        Private m_fpMAxTS As cEwEFormatProvider = Nothing

#End Region ' Private vars

#Region " Constructors "

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
            Me.Grid = Me.m_grid
        End Sub

#End Region ' Constructors

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            Debug.Assert(Me.UIContext IsNot Nothing)

            Dim ecotracerModelParams As cEcotracerModelParameters = Me.UIContext.Core.EcotracerModelParameters()

            Me.m_fpInflowForceNumberEnv = New cPropertyFormatProvider(Me.UIContext, Me.m_cmbEnvInflowFF, ecotracerModelParams, eVarNameFlags.ConForceNumber)
            Me.m_fpCZeroEnv = New cPropertyFormatProvider(Me.UIContext, Me.m_tbCZeroEnv, ecotracerModelParams, eVarNameFlags.CZero)
            Me.m_fpCDecayEnv = New cPropertyFormatProvider(Me.UIContext, Me.m_tbCDecayRateEnv, ecotracerModelParams, eVarNameFlags.CPhysicalDecayRate)
            Me.m_fpCInflowEnv = New cPropertyFormatProvider(Me.UIContext, Me.m_tbCInflowEnv, ecotracerModelParams, eVarNameFlags.CInflow)
            Me.m_fpCOutflowEnv = New cPropertyFormatProvider(Me.UIContext, Me.m_tbCLossEnv, ecotracerModelParams, eVarNameFlags.COutflow)
            Me.m_fpMAxTS = New cPropertyFormatProvider(Me.UIContext, Me.m_tbMaxTS, ecotracerModelParams, eVarNameFlags.ConMaxTimeSteps)

            Me.m_grid.UIContext = Me.UIContext

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.ShapesManager}
            Me.UpdateFFFormatProviders()

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            Me.m_fpCDecayEnv.Release()
            Me.m_fpCInflowEnv.Release()
            Me.m_fpCOutflowEnv.Release()
            Me.m_fpCZeroEnv.Release()
            Me.m_fpInflowForceNumberEnv.Release()
            Me.m_fpMAxTS.Release()

            Me.m_grid.UIContext = Nothing

            MyBase.OnFormClosed(e)

        End Sub

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            Me.UpdateFFFormatProviders()
        End Sub

#End Region ' Events

#Region " Internals "

        Private Sub UpdateFFFormatProviders()
            ' Assemble list of FFs
            Dim ffm As cForcingFunctionShapeManager = Me.UIContext.Core.ForcingShapeManager()
            Dim aItems(ffm.Count) As Object

            aItems(0) = SHaredResources.GENERIC_VALUE_NONE
            For iFF As Integer = 0 To ffm.Count - 1
                aItems(iFF + 1) = ffm(iFF)
            Next
            Me.m_fpInflowForceNumberEnv.Items = aItems
        End Sub

        Private Sub m_btSelectFile_Click(sender As Object, e As EventArgs) Handles m_btSelectFile.Click
            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmdFO As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)

            cmdFO.Invoke(SharedResources.FILEFILTER_CSV & "|" & SharedResources.FILEFILTER_XYZ & "|" & SharedResources.FILEFILTER_TEXT)
            If cmdFO.Result = System.Windows.Forms.DialogResult.OK Then
                Dim manager As EcospaceTimeSeries.cEcospaceTimeSeriesManager = Me.Core.EcospaceTimeSeriesManager
                Dim InputFile As String = cmdFO.FileNames(0)
                If manager.Load(InputFile, "", eVarNameFlags.Concentration) Then
                    Me.m_lbConcentrationFile.Text = manager.ContaminantInputFileName
                End If ' Load with default output file name
            End If
        End Sub

        Private Sub m_btClearFile_Click(sender As Object, e As EventArgs) Handles m_btClearFile.Click
            Me.Core.EcospaceTimeSeriesManager.Clear()
            Me.m_lbConcentrationFile.Text = ""
        End Sub

#End Region ' Internals

    End Class

End Namespace
