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
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecospace

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Form from which to configure generic Ecospace parameters.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class frmEcospaceParameters

        Public Const NOTSAVEDEXT As String = "-notsaved-"

#Region " Private vars "

        ' Scenario generics
        Private m_fpScenarioName As cEwEFormatProvider = Nothing
        Private m_fpScenarioDescription As cEwEFormatProvider = Nothing
        Private m_fpAuthor As cEwEFormatProvider = Nothing
        Private m_fpContact As cEwEFormatProvider = Nothing

        ' Threading
        Private m_fpNGridThreads As cEwEFormatProvider = Nothing
        Private m_fpNBiomassThreads As cEwEFormatProvider = Nothing
        Private m_fpNEffortThreads As cEwEFormatProvider = Nothing
        Private m_fpNumPackets As cEwEFormatProvider = Nothing

        ' Spatial
        Private m_fpN As cEwEFormatProvider = Nothing
        Private m_fpW As cEwEFormatProvider = Nothing
        Private m_fpS As cEwEFormatProvider = Nothing
        Private m_fpE As cEwEFormatProvider = Nothing
        Private m_fpInCol As cEwEFormatProvider = Nothing
        Private m_fpInRow As cEwEFormatProvider = Nothing
        Private m_fpCellLength As cEwEFormatProvider = Nothing
        Private m_fpCellSize As cEwEFormatProvider = Nothing

        ' Model
        Private m_fpTotalTime As cEwEFormatProvider = Nothing
        Private m_fpNumTSpYear As cEwEFormatProvider = Nothing
        Private m_fpTolerance As cEwEFormatProvider = Nothing
        Private m_fpSOR As cEwEFormatProvider = Nothing
        Private m_fpMaxIterations As cEwEFormatProvider = Nothing
        Private m_fpUseExact As cEwEFormatProvider = Nothing
        Private m_fpAnnualOutput As cEwEFormatProvider = Nothing

        Private m_fpMovePackets As cEwEFormatProvider = Nothing
        Private WithEvents m_bpConTracing As cBooleanProperty = Nothing

        ' Ecospace time series
        Private WithEvents m_bpUseBiomassForcing As cBooleanProperty = Nothing
        Private m_fpUseBiomassForcing As cEwEFormatProvider = Nothing
        Private WithEvents m_bpUseDiscardForcing As cBooleanProperty = Nothing
        Private m_fpUseDiscardForcing As cEwEFormatProvider = Nothing

        ' Properties to monitor for setting radio button check states
        Private WithEvents m_bpUseIBM As cBooleanProperty = Nothing
        Private WithEvents m_bpUseNewStanza As cBooleanProperty = Nothing
        Private WithEvents m_bpAdjustSpace As cBooleanProperty = Nothing
        Private WithEvents m_bpEffort As cBooleanProperty = Nothing

        Private m_fpFirstOutputTimestep As cEwEFormatProvider

#End Region ' Private vars

#Region " Form events "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; called when the form is initially loaded.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnLoad(e As System.EventArgs)

            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters()
            Dim bm As cEcospaceBasemap = Me.Core.EcospaceBasemap
            Dim propMan As cPropertyManager = Me.PropertyManager

            ' Start listening to props
            Me.m_bpUseIBM = DirectCast(propMan.GetProperty(parms, eVarNameFlags.UseIBM), cBooleanProperty)
            Me.m_bpUseNewStanza = DirectCast(propMan.GetProperty(parms, eVarNameFlags.UseNewMultiStanza), cBooleanProperty)
            Me.m_bpAdjustSpace = DirectCast(propMan.GetProperty(parms, eVarNameFlags.AdjustSpace), cBooleanProperty)
            Me.m_bpEffort = DirectCast(propMan.GetProperty(parms, eVarNameFlags.PredictEffort), cBooleanProperty)

            Me.m_bpConTracing = DirectCast(propMan.GetProperty(parms, eVarNameFlags.ConSimOnEcoSpace), cBooleanProperty)

            Me.m_bpUseBiomassForcing = DirectCast(propMan.GetProperty(parms, eVarNameFlags.EcospaceUseEcosimBiomassForcing), cBooleanProperty)
            Me.m_fpUseBiomassForcing = New cPropertyFormatProvider(Me.UIContext, Me.m_cbUseEcosimBiomassForcing, Me.m_bpUseBiomassForcing)

            Me.m_bpUseDiscardForcing = DirectCast(propMan.GetProperty(parms, eVarNameFlags.EcospaceUseEcosimDiscardForcing), cBooleanProperty)
            Me.m_fpUseDiscardForcing = New cPropertyFormatProvider(Me.UIContext, Me.m_cbUseEcosimDiscardForcing, Me.m_bpUseDiscardForcing)

            Me.m_clbAutosave.Items.Clear()
            For n As Integer = 1 To parms.nResultWriters
                Dim writer As IEcospaceResultsWriter = parms.ResultWriter(n)
                Me.m_clbAutosave.Items.Add(writer, writer.Enabled)
            Next

            Me.UpdateControls()

            Me.m_fpInCol = New cEwEFormatProvider(Me.UIContext, Me.m_nudColCount, GetType(Integer), bm.GetVariableMetadata(eVarNameFlags.InCol))
            Me.m_fpInCol.Value = bm.InCol

            Me.m_fpInRow = New cEwEFormatProvider(Me.UIContext, Me.m_nudRowCount, GetType(Integer), bm.GetVariableMetadata(eVarNameFlags.InRow))
            Me.m_fpInRow.Value = bm.InRow

            Me.m_fpN = New cEwEFormatProvider(Me.UIContext, Me.m_nudNorth, GetType(Single), bm.GetVariableMetadata(eVarNameFlags.Latitude))
            Me.m_fpN.Value = bm.Latitude

            Me.m_fpW = New cEwEFormatProvider(Me.UIContext, Me.m_nudWest, GetType(Single), bm.GetVariableMetadata(eVarNameFlags.Longitude))
            Me.m_fpW.Value = bm.Longitude

            Me.m_fpCellLength = New cEwEFormatProvider(Me.UIContext, Me.m_nudCellLength, GetType(Single), bm.GetVariableMetadata(eVarNameFlags.CellLength))
            Me.m_fpCellLength.Value = bm.CellLength

            Me.m_fpCellSize = New cEwEFormatProvider(Me.UIContext, Me.m_nudCellSize, GetType(Single), bm.GetVariableMetadata(eVarNameFlags.CellSize))
            Me.m_fpCellSize.Value = bm.CellSize

            ' Hmm, connecting one control to three live properties - this could be dangerous
            Me.m_fpNGridThreads = New cPropertyFormatProvider(Me.UIContext, Me.m_nudNumThreads, parms, eVarNameFlags.nGridSolverThreads)
            Me.m_fpNBiomassThreads = New cPropertyFormatProvider(Me.UIContext, Me.m_nudNumThreads, parms, eVarNameFlags.nSpaceThreads)
            Me.m_fpNEffortThreads = New cPropertyFormatProvider(Me.UIContext, Me.m_nudNumThreads, parms, eVarNameFlags.nEffortDistThreads)
            Me.m_fpNumPackets = New cPropertyFormatProvider(Me.UIContext, Me.m_tbNumPackets, parms, eVarNameFlags.PacketsMultiplier)
            Me.m_fpFirstOutputTimestep = New cPropertyFormatProvider(Me.UIContext, Me.m_nudFirstTimeStep, parms, eVarNameFlags.EcospaceFirstOutputTimeStep)

            ' Model
            Me.m_fpTotalTime = New cPropertyFormatProvider(Me.UIContext, Me.m_tbTotalTime, parms, eVarNameFlags.TotalTime)
            Me.m_fpNumTSpYear = New cPropertyFormatProvider(Me.UIContext, Me.m_tbNumTimeStepsPerYear, parms, eVarNameFlags.NumTimeStepsPerYear)
            Me.m_fpTolerance = New cPropertyFormatProvider(Me.UIContext, Me.m_tbTolerance, parms, eVarNameFlags.Tolerance)
            Me.m_fpSOR = New cPropertyFormatProvider(Me.UIContext, Me.m_tbSOR, parms, eVarNameFlags.SOR)
            Me.m_fpMaxIterations = New cPropertyFormatProvider(Me.UIContext, Me.m_nudMaxIterations, parms, eVarNameFlags.MaxIterations)
            Me.m_fpUseExact = New cPropertyFormatProvider(Me.UIContext, Me.m_cbUseExact, parms, eVarNameFlags.UseExact)
            Me.m_fpAnnualOutput = New cPropertyFormatProvider(Me.UIContext, Me.m_cbAnnualOutput, parms, eVarNameFlags.EcospaceUseAnnualOutput)

            Me.m_fpMovePackets = New cPropertyFormatProvider(Me.UIContext, Me.m_cbMovePackets, parms, eVarNameFlags.EcospaceIBMMovePacketOnStanza)

            Me.UpdateScenarioFormatProviders()

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSpace, eCoreComponentType.Core, eCoreComponentType.TimeSeries}

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            Try

                Me.m_bpUseIBM = Nothing
                Me.m_bpUseNewStanza = Nothing
                Me.m_bpAdjustSpace = Nothing
                Me.m_bpConTracing = Nothing
                Me.m_bpEffort = Nothing

                Me.m_fpScenarioName.Release()
                Me.m_fpScenarioDescription.Release()
                Me.m_fpAuthor.Release()
                Me.m_fpContact.Release()

                Me.m_fpNGridThreads.Release()
                Me.m_fpNBiomassThreads.Release()
                Me.m_fpNEffortThreads.Release()
                Me.m_fpNumPackets.Release()
                Me.m_fpTotalTime.Release()
                Me.m_fpNumTSpYear.Release()
                Me.m_fpTolerance.Release()
                Me.m_fpSOR.Release()
                Me.m_fpMaxIterations.Release()
                Me.m_fpUseExact.Release()
                Me.m_fpMovePackets.Release()
                Me.m_fpUseBiomassForcing.Release()
                Me.m_fpUseDiscardForcing.Release()
                Me.m_fpAnnualOutput.Release()

                Me.m_fpFirstOutputTimestep.Release()

            Catch ex As Exception

            End Try

            MyBase.OnFormClosed(e)

        End Sub

#End Region ' Form events

#Region " Form content handling "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper enum, used to determine the threading model type from ecospace data flags.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Enum eThreadingModelType As Integer
            UseNewStanza
            UseIBM
            OldSchool
        End Enum

        Private m_bInUpdate As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update and enable controls that cannot be managed any other way.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub UpdateControls()

            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_bInUpdate) Then Return

            Me.m_bInUpdate = True

            Dim threadingModel As eThreadingModelType = eThreadingModelType.OldSchool
            Dim bUseIBM As Boolean = CBool(Me.m_bpUseIBM.GetValue())
            Dim bUseNewStanza As Boolean = CBool(Me.m_bpUseNewStanza.GetValue())
            Dim parms As cEcospaceModelParameters = Me.Core.EcospaceModelParameters

            If bUseIBM Then threadingModel = eThreadingModelType.UseIBM
            If bUseNewStanza Then threadingModel = eThreadingModelType.UseNewStanza

            Select Case threadingModel
                Case eThreadingModelType.OldSchool
                    Me.m_rbOldSchool.Checked = True
                Case eThreadingModelType.UseIBM
                    Me.m_rbIBM.Checked = True
                Case eThreadingModelType.UseNewStanza
                    Me.m_rbNewStanzaModel.Checked = True
            End Select

            Me.m_rbBaseBiomass.Checked = Not CBool(Me.m_bpAdjustSpace.GetValue())
            Me.m_rbAdjustedBiomass.Checked = CBool(Me.m_bpAdjustSpace.GetValue())

            Me.m_cbContaminantTracing.Checked = CBool(Me.m_bpConTracing.GetValue())

            For i As Integer = 0 To Me.m_clbAutosave.Items.Count - 1
                Dim wr As IEcospaceResultsWriter = DirectCast(Me.m_clbAutosave.Items(i), IEcospaceResultsWriter)
                Me.m_clbAutosave.SetItemChecked(i, wr.Enabled And Me.Core.Autosave(eAutosaveTypes.EcospaceResults))
            Next

            Me.m_rbPredictEffort.Checked = CBool(Me.m_bpEffort.GetValue())
            Me.m_rbEcopathEffort.Checked = Not CBool(Me.m_bpEffort.GetValue())

            ' Time series
            Dim manager As EcospaceTimeSeries.cEcospaceTimeSeriesManager = Me.Core.EcospaceTimeSeriesManager
            Me.m_tbxXYTimeSeriesFile.Text = If(String.IsNullOrWhiteSpace(manager.BiomassInputFileName), SharedResources.GENERIC_VALUE_NOTSET, manager.BiomassInputFileName)
            Me.m_tbxlOutputResidualsFile.Text = If(String.IsNullOrWhiteSpace(manager.OutputFileName), SharedResources.GENERIC_VALUE_NOTSET, manager.OutputFileName)

            ' Ecosim forcing
            Me.m_fpUseBiomassForcing.Enabled = Core.EcospaceModelParameters.IsEcosimBiomassForcingLoaded
            Me.m_cbUseEcosimDiscardForcing.Visible = True
            Me.m_fpUseDiscardForcing.Enabled = Core.EcospaceModelParameters.IsEcosimDiscardForcingLoaded

            Me.m_bInUpdate = False

        End Sub

#End Region ' Form content handling

#Region " cProperty events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; called when either of the two model state properties changes.
        ''' </summary>
        ''' <param name="prop">The property that changed.</param>
        ''' <param name="changeFlags">The extent of the change.</param>
        ''' -------------------------------------------------------------------
        Private Sub OnPropertyChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags) _
            Handles m_bpUseIBM.PropertyChanged, m_bpUseNewStanza.PropertyChanged, m_bpConTracing.PropertyChanged, _
                    m_bpUseBiomassForcing.PropertyChanged, m_bpUseDiscardForcing.PropertyChanged
            Me.UpdateControls()
        End Sub

#End Region ' cProperty events

#Region " Control events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; called when the IBM mode radio button is checked.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnRunIBM(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_rbIBM.CheckedChanged, m_rbNewStanzaModel.CheckedChanged, m_rbOldSchool.CheckedChanged

            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_bInUpdate) Then Return

            If Me.m_rbIBM.Checked Then
                Me.m_bpUseNewStanza.SetValue(False)
                Me.m_bpUseIBM.SetValue(True)
            ElseIf Me.m_rbNewStanzaModel.Checked Then
                Me.m_bpUseIBM.SetValue(False)
                Me.m_bpUseNewStanza.SetValue(True)
            ElseIf Me.m_rbOldSchool.Checked Then
                Me.m_bpUseNewStanza.SetValue(False)
                Me.m_bpUseIBM.SetValue(False)
            End If

        End Sub

        Private Sub OnBiomassOptionChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_rbBaseBiomass.CheckedChanged, m_rbAdjustedBiomass.CheckedChanged

            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_bInUpdate) Then Return

            Me.m_bpAdjustSpace.SetValue(Me.m_rbAdjustedBiomass.Checked)

        End Sub

        Private Sub OnEffortOptionChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_rbPredictEffort.CheckedChanged, m_rbEcopathEffort.CheckedChanged

            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_bInUpdate) Then Return

            Me.m_bpEffort.SetValue(Me.m_rbPredictEffort.Checked)

        End Sub

        Private Sub OnConcTracingOptionChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_cbContaminantTracing.CheckedChanged

            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_bInUpdate) Then Return

            If Me.m_cbContaminantTracing.Checked Then
                Dim cmdh As cCommandHandler = Me.CommandHandler
                Dim cmd As cCommand = cmdh.GetCommand("EnableEcotracer")

                If (cmd IsNot Nothing) Then
                    cmd.Tag = eTracerRunModeTypes.RunSpace
                    cmd.Invoke()
                    If (Me.Core.ActiveEcotracerScenarioIndex <= 0) Then
                        Me.m_cbContaminantTracing.Checked = False
                    End If
                End If
            End If

            ' If tracer scenario loaded turn this on
            Me.m_bpConTracing.SetValue(Me.m_cbContaminantTracing.Checked)

            Me.UpdateControls()

        End Sub

        Private Sub m_clbAutosave_Format(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
            Handles m_clbAutosave.Format
            Try
                e.Value = DirectCast(e.ListItem, IEcospaceResultsWriter).DisplayName
            Catch ex As Exception

            End Try
        End Sub

        Private Sub m_clbAutosave_ItemCheck(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) _
            Handles m_clbAutosave.ItemCheck

            If Me.m_bInUpdate Then Return

            ' Delay the update, because the item state has not changed yet
            Me.BeginInvoke(New MethodInvoker(AddressOf UpdateResultWriters))

        End Sub

        Private Sub UpdateResultWriters()

            Dim bAutoSaving As Boolean = False

            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True

            For i As Integer = 0 To Me.m_clbAutosave.Items.Count - 1
                Dim wr As IEcospaceResultsWriter = DirectCast(Me.m_clbAutosave.Items(i), IEcospaceResultsWriter)
                wr.Enabled = Me.m_clbAutosave.GetItemChecked(i)
                bAutoSaving = bAutoSaving Or wr.Enabled
            Next
            Me.Core.Autosave(eAutosaveTypes.EcospaceResults) = bAutoSaving

            Me.m_bInUpdate = False

        End Sub

        Private Sub OnLoadXYTimeSeries_Click(sender As Object, e As EventArgs) Handles m_btnLoadXYTimeSeries.Click

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmdFO As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)

            cmdFO.Invoke(SharedResources.FILEFILTER_CSV & "|" & SharedResources.FILEFILTER_XYZ & "|" & SharedResources.FILEFILTER_TEXT)
            If cmdFO.Result = System.Windows.Forms.DialogResult.OK Then
                Dim manager As EcospaceTimeSeries.cEcospaceTimeSeriesManager = Me.Core.EcospaceTimeSeriesManager
                Dim InputFile As String = cmdFO.FileNames(0)
                manager.Load(InputFile, "", eVarNameFlags.EcospaceMapBiomass)
            End If
        End Sub

        Private Sub OnTimeSeriesOutputFile_Click(sender As Object, e As EventArgs) Handles m_btnTimeSeriesOutputFile.Click
            Dim manager As EcospaceTimeSeries.cEcospaceTimeSeriesManager = Me.Core.EcospaceTimeSeriesManager
            Dim dlgSave As New SaveFileDialog

            dlgSave.Filter = SharedResources.FILEFILTER_CSV & "|" & SharedResources.FILEFILTER_XYZ & "|" & SharedResources.FILEFILTER_TEXT
            dlgSave.InitialDirectory = IO.Path.GetDirectoryName(manager.OutputFileName)
            dlgSave.FileName = IO.Path.GetFileName(manager.OutputFileName)
            If dlgSave.ShowDialog = System.Windows.Forms.DialogResult.OK Then
                manager.OutputFileName = dlgSave.FileName
            End If

        End Sub

#End Region ' Control events

#Region " Overrides "

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            If ((msg.Source = eCoreComponentType.Core) And (msg.Type = eMessageType.GlobalSettingsChanged)) Then
                Me.UpdateControls()
            End If

            If msg.Source = eCoreComponentType.EcoSpace And msg.Type = eMessageType.DataModified Then
                Me.UpdateControls()
            End If

        End Sub

#End Region ' Overrides

#Region " Internals "

        Private Sub UpdateScenarioFormatProviders()

            Dim scenarioDef As cEcospaceScenario = Core.EcospaceScenarios(Core.ActiveEcospaceScenarioIndex)

            ' Connect controls to core data
            Me.m_fpScenarioName = New cPropertyFormatProvider(Me.UIContext, Me.m_tbName, scenarioDef, eVarNameFlags.Name)
            Me.m_fpScenarioDescription = New cPropertyFormatProvider(Me.UIContext, Me.m_tbDescription, scenarioDef, eVarNameFlags.Description)
            Me.m_fpAuthor = New cPropertyFormatProvider(Me.UIContext, Me.m_tbAuthor, scenarioDef, eVarNameFlags.Author)
            Me.m_fpContact = New cPropertyFormatProvider(Me.UIContext, Me.m_tbContact, scenarioDef, eVarNameFlags.Contact)

        End Sub

#End Region ' Internals

    End Class

End Namespace
