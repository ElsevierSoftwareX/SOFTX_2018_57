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

#Region "Imports"

Option Strict On
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style

#End Region

' ToDo: globalize this form
' ToDo: use cMessage instead of MsgBox

Public Class frmResults

#Region "Enumerator(s)"

    Private Enum eResultTypes As Integer
        Biomass = 0
        BiomassIntegrated = 1
        FishingMortality = 2
        PredationMortality = 3
        ConsumptionBiomass = 4
        PredationPerPredator = 5
        FishMortFleetToPrey = 6
        GroupCatch = 7
        DietProportions = 8
        FleetCatch = 9
        FleetValue = 10
        BasicEstimates = 11
        KeyIndices = 12

    End Enum

#End Region

#Region "Private Fields"

    Private m_PluginInterface As frmResults
    Private m_bInitOK As Boolean
    Private m_uic As cUIContext = Nothing
    Private APredPreySelection As List(Of cPredatorPreySelection)
    Private Shared m_NumberTicked As Integer
    Private PredatorPreySelection As cSelectionData
    Private FleetPreySelection As cSelectionData
    Private PreyPredatorSelection As cSelectionData
    Private ParentOnlySelection As cSelectionData
    Private FleetOnlySelection As cSelectionData
    Private m_MyCheckBoxes As CheckBox()
    Private strPath As String
    'Private FunctGroupWB As Excel.Workbook
    'Private FisheriesWB As Excel.Workbook
    'Private IndicatorsWB As Excel.Workbook
    Private nDataRows As Integer
    'Private Const FuncGroupsFileName As String = My.Resources.FUNCTIONALGROUPS
    'Private Const FishFleetsFileName As String = My.Resources.FISHERIESGROUPS
    'Private Const IndicatorsFileName As String = "Indicators"
    'Private Const DiagnosticsName As String = "Diagnostics"
    Private DataOutputter As cDataOutputer
    Private mLogDiff(,) As Single
    Private mTimeSeries As cTimeSeriesDataStructures
    Private mDataStructure As cEcosimDatastructures
    Private mEcosimModel As Ecosim.cEcoSimModel


#End Region

    'Delegate that points to next sub to be executed when key-run button clicked
    Public Delegate Sub NextActionTickAll()
    'An instance of the delegate that points to next action 
    Public Shared NextAction As NextActionTickAll

    ' The boolean that determines whether checked event for tick boxes occurs
    Public Shared FireChecked As Boolean = True

#Region "Constructor(s)"

    Public Sub New()

        Me.InitializeComponent()

    End Sub

#End Region

#Region "Overrides"

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)
        If (Me.m_uic Is Nothing) Then Return
    End Sub

#End Region

    Public Sub StartForm(ByVal sender As Object, ByVal e As System.EventArgs, ByRef frmPlugin As System.Windows.Forms.Form, ByRef log2diff(,) As Single, ByRef TimeSeries As cTimeSeriesDataStructures, ByVal EcosimModel As Ecosim.cEcoSimModel)

        Dim GroupNames As String() = Me.GetAllGroupNamesArray()
        Dim FleetNames As String() = Me.GetAllFleetNamesArray()

        mLogDiff = log2diff
        mTimeSeries = TimeSeries
        mEcosimModel = EcosimModel

        DataOutputter = New cDataOutputer

        frmPlugin = Me

        'JS 04 March 2011: Do not show yet; let form populate itself and let the framework do the showing after the plugin is fully prepared
        'Me.Show()

        nDataRows = Core.nEcosimTimeSteps

        'Get all group names for predators to create PredatorPreySelection & PreyPredSelection
        'Remember that EcoSimGroupOutputs are indexed from 1!!!
        Dim str(Me.Core.nGroups - 1) As String
        For i As Integer = 1 To Me.Core.nGroups
            str(i - 1) = Me.Core.EcoSimGroupOutputs(i).Name
        Next
        'Create PredPreySelection object
        PredatorPreySelection = New cSelectionData(My.Resources.PRED2_MANYPREY, str)
        'Create PreyPredSelection object
        PreyPredatorSelection = New cSelectionData(My.Resources.PREY2_MANYPRED, str)
        'Create Parent object
        ParentOnlySelection = New cSelectionData(My.Resources.PARENT_ONLY, str)

        'Get all groups names for fleet to create FleetPreySelection
        'Remember that EcosimFleetOutput is referenced from 0!!!
        Dim str2(Me.Core.nFleets) As String
        For i As Integer = 0 To Me.Core.nFleets
            str2(i) = Me.Core.EcosimFleetOutput(i).Name
        Next
        ' Create FleetPreySelection
        FleetPreySelection = New cSelectionData(My.Resources.FLEET2_MANYPREY, str2)
        ' Create FleetOnlySelection
        FleetOnlySelection = New cSelectionData(My.Resources.FLEET_ONLY, str2)

        ' Try to set interop to Excel
        DataOutputter.POutputType = cDataOutputer.eOutputTypes.Excel

        ' See what happened. If output type is CSV then Excel was not accessible.
        Select Case DataOutputter.POutputType
            Case cDataOutputer.eOutputTypes.Excel
                optExcel.Checked = True
            Case cDataOutputer.eOutputTypes.CSV
                ' Disable Excel option
                optCSV.Checked = True
                optExcel.Enabled = False
        End Select

    End Sub

    Public Sub Initialize(ByVal uic As cUIContext)
        m_bInitOK = False
        Try
            Me.m_uic = uic
            m_bInitOK = True
            System.Console.WriteLine(Me.ToString & ".Initialize() Successfull.")
        Catch ex As Exception
            cLog.Write(ex)
            System.Console.WriteLine(Me.ToString & ".Initialize() Error: " & ex.Message)
            Debug.Assert(False, ex.Message)
            Return
        End Try
    End Sub

#Region "Properties"

    Public ReadOnly Property Core() As cCore
        Get
            Return Me.m_uic.Core
        End Get
    End Property

    Public WriteOnly Property DataStructure() As cEcosimDatastructures
        Set(ByVal value As cEcosimDatastructures)
            mDataStructure = value
        End Set
    End Property

#End Region

#Region "Event Handlers"

    Private Sub btnSaveResults_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSaveResults.Click
        ' #1199: Made bullet proof to missing inputs
        Try
            Me.SaveResults()
        Catch ex As Exception
            Dim msg As New cMessage(My.Resources.PROMPT_INPUTS, eMessageType.TooManyMissingParameters, eCoreComponentType.EcoSim, eMessageImportance.Warning)
            Me.Core.Messages.SendMessage(msg)
        End Try
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
        ResetForm()
    End Sub

    Private Sub chkBiomass_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBiomass.CheckedChanged
        Dim a As frmSelectParentOnly
        If FireChecked = False Then Exit Sub
        If chkBiomass.Checked = True And ParentOnlySelection.CountSelected = 0 Then
            a = frmSelectParentOnly.GetInstance(ParentOnlySelection, Core)
            'Dim a As New frmSelectParentOnly(ParentOnlySelection, m_core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkBiomass.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkBiomassInteg_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBiomassInteg.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkBiomassInteg.Checked = True And ParentOnlySelection.CountSelected = 0 Then
            Dim a As New frmSelectParentOnly(ParentOnlySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkBiomassInteg.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkConsumptionBiomass_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkConsumption.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkConsumption.Checked = True And PredatorPreySelection.CountSelected = 0 Then
            Dim a As New frmSelectPredatorPrey(PredatorPreySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkConsumption.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkPredationMortality_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPredationMortality.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkPredationMortality.Checked = True And ParentOnlySelection.CountSelected = 0 Then
            Dim a As New frmSelectParentOnly(ParentOnlySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkPredationMortality.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkFishingMortality_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFishingMortality.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkFishingMortality.Checked = True And ParentOnlySelection.CountSelected = 0 Then
            Dim a As New frmSelectParentOnly(ParentOnlySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkFishingMortality.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkPredationPerPredator_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPredationPerPredator.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkPredationPerPredator.Checked = True And PreyPredatorSelection.CountSelected = 0 Then
            Dim a As New frmSelectPreyPredator(PreyPredatorSelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkPredationPerPredator.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkFishMortFleetToPrey_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFishMortFleetToPrey.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkFishMortFleetToPrey.Checked = True And FleetPreySelection.CountSelected = 0 Then
            Dim a As New frmSelectFleetPrey(FleetPreySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkFishMortFleetToPrey.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkEffort_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEffort.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkEffort.Checked = True And FleetOnlySelection.CountSelected = 0 Then
            Dim a As New frmSelectFleetOnly(FleetOnlySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkEffort.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkCatch_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCatch.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkCatch.Checked = True And ParentOnlySelection.CountSelected = 0 Then
            Dim a As New frmSelectParentOnly(ParentOnlySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkCatch.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkDietProportions_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDietProportions.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkDietProportions.Checked = True And PredatorPreySelection.CountSelected = 0 Then
            Dim a As New frmSelectPredatorPrey(PredatorPreySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkDietProportions.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkCatchFleet_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCatchFleet.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkCatchFleet.Checked = True And FleetPreySelection.CountSelected = 0 Then
            Dim a As New frmSelectFleetPrey(FleetPreySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkCatchFleet.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub chkValue_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFleetValue.CheckedChanged
        If FireChecked = False Then Exit Sub
        If chkFleetValue.Checked = True And FleetOnlySelection.CountSelected = 0 Then
            Dim a As New frmSelectFleetOnly(FleetOnlySelection, Core)
            a.Show()
            'When form is closed call this validation sub
            AddHandler a.FormExited, AddressOf ValidateObjectCreated
        End If
        If chkFleetValue.Checked = False Then DeleteObjects()
        SetSaveResultsState()
    End Sub

    Private Sub btnSetPredPrey_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPredPrey.Click
        Dim a As New frmSelectPredatorPrey(PredatorPreySelection, Core)
        AddHandler a.FormExited, AddressOf ValidateObjectCreated
    End Sub

    Private Sub btnSetFeetPrey_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim a As New frmSelectFleetPrey(FleetPreySelection, Core)
        a.Show()
        AddHandler a.FormExited, AddressOf ValidateObjectCreated
    End Sub

    Private Sub btnSetPreyPred_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPreyPred.Click
        Dim a As New frmSelectPreyPredator(PreyPredatorSelection, Core)
        a.Show()
        AddHandler a.FormExited, AddressOf ValidateObjectCreated
    End Sub

    Private Sub btnSetParentOnly_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetParentOnly.Click
        Dim a As New frmSelectParentOnly(ParentOnlySelection, Core)
        a.Show()
        AddHandler a.FormExited, AddressOf ValidateObjectCreated
    End Sub

    Private Sub btnSetCatchFleet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetFleetPrey.Click
        Dim a As New frmSelectFleetPrey(FleetPreySelection, Core)
        a.Show()
        AddHandler a.FormExited, AddressOf ValidateObjectCreated
    End Sub

    Private Sub btnSetFleetOnly_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetFleetOnly.Click
        Dim a As New frmSelectFleetOnly(FleetOnlySelection, Core)
        a.Show()
        AddHandler a.FormExited, AddressOf ValidateObjectCreated
    End Sub

    Private Sub btnTickAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAllOptions.Click
        FireChecked = False
        NextAction = New NextActionTickAll(AddressOf Me.PredatorPreyStage)

        'First stage is do parent only section
        Dim a As New frmSelectParentOnly(ParentOnlySelection, Core)
        a.Show()

    End Sub

    Private Sub chkBasicEstimates_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBasicEstimates.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkKeyIndices_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkKeyIndices.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkMortalityCoefficients_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMortalityCoefficients.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkInitPredMort_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkInitPredMort.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkInitConsumption_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkInitConsumption.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkInitFishMort_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkInitFishMort.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkRespiration_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRespiration.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkPreyOverlap_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPreyOverlap.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkPredOverlap_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPredOverlap.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkElectivity_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkElectivity.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkSearchRates_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSearchRates.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkInitFishingQuantities_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkInitFishingQuantities.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkInitFishingValues_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkInitFishingValues.CheckedChanged
        SetSaveResultsState()
    End Sub

    Private Sub chkYearly_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkYearly.CheckedChanged
        If chkYearly.Checked Then
            nDataRows = CInt(Math.Floor(Core.nEcosimTimeSteps / cCore.N_MONTHS))
        Else
            nDataRows = Core.nEcosimTimeSteps
        End If
    End Sub

    Private Sub optCSV_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optCSV.CheckedChanged
        DataOutputter.POutputType = cDataOutputer.eOutputTypes.CSV
    End Sub

    Private Sub optExcel_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optExcel.CheckedChanged
        DataOutputter.POutputType = cDataOutputer.eOutputTypes.Excel
    End Sub

    Private Sub chklog2res_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkresiduals.CheckedChanged
        If FireChecked = False Then Exit Sub
        SetSaveResultsState()
    End Sub

    Private Sub chkSS_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSS.CheckedChanged
        If FireChecked = False Then Exit Sub
        SetSaveResultsState()
    End Sub

#End Region

#Region "Functions"

    Public Function GetAllGroupNamesArray() As String()
        Dim str(Me.Core.nGroups - 1) As String

        For i As Integer = 1 To Me.Core.nGroups
            str(i - 1) = Me.Core.EcoSimGroupOutputs(i).Name
        Next
        Return str

    End Function

    Public Function GetAllFleetNamesArray() As String()
        Dim str(Me.Core.nFleets) As String

        For i As Integer = 0 To Me.Core.nFleets
            str(i) = Me.Core.EcosimFleetOutput(i).Name
        Next
        Return str

    End Function

    Private Function CreateListNames(ByVal InputStrings As List(Of String)) As String
        'Create a string of names for the list of input objects
        Dim CompiledNames As New StringBuilder()

        For i As Integer = 0 To InputStrings.Count - 2
            CompiledNames.Append("""" & InputStrings(i) & """" & ",")
        Next
        CompiledNames.Append("""" & InputStrings(InputStrings.Count - 1) & """")
        Return CompiledNames.ToString

    End Function

    Private Function GetPreyNames(ByVal PredPreyObject As cPredatorPreySelection) As String
        'Create a string of predator names for the list of prey a given predator selection
        Dim PreyNames As New StringBuilder()

        For i As Integer = 0 To PredPreyObject.CountPrey - 2
            PreyNames.Append("""" & PredPreyObject.PreyName(i) & """" & ",")
        Next
        PreyNames.Append("""" & PredPreyObject.PreyName(PredPreyObject.CountPrey - 1) & """")
        Return PreyNames.ToString

    End Function

    Private Function GetIndexGroup(ByVal Group As String) As Integer

        'Find out what the index number is for a given group in m_core.EcosimGroupOutputs
        Dim i As Integer = 1
        While i <= Core.nGroups And Core.EcoSimGroupOutputs(i).Name <> Group
            i += 1
        End While
        If i > Core.nGroups Then
            Return -1
        Else
            Return i
        End If

    End Function

    Private Function GetIndexFleet(ByVal Fleet As String) As Integer

        'Find out what the index number is for a given fleet in m_core.EcosimGroupOutputs
        Dim i As Integer = 0
        While i <= Core.nFleets
            If Core.EcosimFleetOutput(i).Name = Fleet Then
                Exit While
            End If
            i += 1
        End While
        If i > Core.nFleets Then
            Return -1
        Else
            Return i
        End If

    End Function


#End Region

#Region "Subroutines"


    Private Sub SaveResults()

        Dim NumberChecks As Integer = 0
        Dim CurrentPredator As cCreatedObjects
        Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
        Dim cmd As cCommand = Nothing
        Dim cmdDir As cDirectoryOpenCommand = Nothing

        If (cmdh Is Nothing) Then Return
        cmd = cmdh.GetCommand(cDirectoryOpenCommand.COMMAND_NAME)
        If (cmd Is Nothing) Then Return
        If (Not TypeOf cmd Is cDirectoryOpenCommand) Then Return
        cmdDir = DirectCast(cmd, cDirectoryOpenCommand)

        Dim strPath As String = Me.Core.DefaultOutputPath(eAutosaveTypes.EcosimResults)
        If Not Directory.Exists(strPath) Then
            strPath = Me.Core.OutputPath
        End If

        ' Let EwE framework do the folder browsing
        ' JS 18Nov12: Start browsing at default Sim output dir
        cmdDir.Invoke(strPath, My.Resources.PROMPT_FOLDER)

        If (cmdDir.Result = System.Windows.Forms.DialogResult.OK) Or (cmdDir.Result = System.Windows.Forms.DialogResult.Yes) Then

            DataOutputter.PPath = cmdDir.Directory

            'Count how many dataselections have been checked
            If chkBiomass.Checked Then NumberChecks += 1
            If chkBiomassInteg.Checked Then NumberChecks += 1
            If chkConsumption.Checked Then NumberChecks += 1
            If chkFishingMortality.Checked Then NumberChecks += 1
            If chkPredationMortality.Checked Then NumberChecks += 1
            If chkPredationPerPredator.Checked Then NumberChecks += 1
            If chkFishMortFleetToPrey.Checked Then NumberChecks += 1
            If chkEffort.Checked Then NumberChecks += 1
            If chkCatch.Checked Then NumberChecks += 1
            If chkDietProportions.Checked Then NumberChecks += 1
            If chkCatchFleet.Checked Then NumberChecks += 1
            If chkFleetValue.Checked Then NumberChecks += 1
            If chkBasicEstimates.Checked Then NumberChecks += 1
            If chkKeyIndices.Checked Then NumberChecks += 1
            If chkMortalityCoefficients.Checked Then NumberChecks += 1
            If chkInitPredMort.Checked Then NumberChecks += 1
            If chkInitConsumption.Checked Then NumberChecks += 1
            If chkInitFishMort.Checked Then NumberChecks += 1
            If chkRespiration.Checked Then NumberChecks += 1
            If chkPreyOverlap.Checked Then NumberChecks += 1
            If chkPredOverlap.Checked Then NumberChecks += 1
            If chkElectivity.Checked Then NumberChecks += 1
            If chkInitFishingQuantities.Checked Then NumberChecks += 1
            If chkSearchRates.Checked Then NumberChecks += 1
            If chkInitFishingValues.Checked Then NumberChecks += 1
            If chkresiduals.Checked Then NumberChecks += 1
            If chkSS.Checked Then NumberChecks += 1

            'Setup progress bar
            lblPrgInfo.Show()
            prgSave.Visible = True
            prgSave.Minimum = 0
            prgSave.Maximum = NumberChecks
            prgSave.Value = 0
            prgSave.Step = 1
            Application.DoEvents()

            If chkBiomass.Checked Then
                CreateBiomassCSV()
                prgSave.PerformStep()
            End If

            If chkBiomassInteg.Checked Then
                CreateBiomassIntegratedCSV()
                prgSave.PerformStep()
            End If

            If chkConsumption.Checked Then
                If chkConsumption.Checked Then
                    For PredatorIndex As Integer = 0 To PredatorPreySelection.CountSelected - 1
                        'Get Predator Parent-Child Object
                        CurrentPredator = PredatorPreySelection.GetSelectedItem(PredatorIndex)
                        CreateConsumptionCSV(CurrentPredator)
                    Next
                    prgSave.PerformStep()
                End If
            End If
            If chkFishingMortality.Checked Then
                CreateFishingMortalityCSV()
                prgSave.PerformStep()
            End If
            If chkPredationMortality.Checked Then
                CreatePredationMortalityCSV()
                prgSave.PerformStep()
            End If
            If chkPredationPerPredator.Checked Then
                CreatePredationMortalityEachPredatorCSV()
                prgSave.PerformStep()
            End If
            If chkFishMortFleetToPrey.Checked Then
                CreateMortalityByFleetCSV()
                prgSave.PerformStep()
            End If
            If chkEffort.Checked Then
                CreateEffort()
                prgSave.PerformStep()
            End If
            If chkCatch.Checked Then
                CreateCatchCSV()
                prgSave.PerformStep()
            End If
            If chkDietProportions.Checked Then
                'Run for each Predator object
                For PredatorIndex As Integer = 0 To PredatorPreySelection.GetSelected.Count - 1
                    'Get Predator Parent-Child Object
                    CurrentPredator = PredatorPreySelection.GetSelectedItem(PredatorIndex)
                    CreateDietCSV(CurrentPredator)
                Next

                prgSave.PerformStep()
            End If
            If chkCatchFleet.Checked Then
                CreateCatchByFleetCSV()
                CreateLandingsByFleetCSV()
                CreateDiscardsByFleetCSV()
                prgSave.PerformStep()
            End If
            If chkFleetValue.Checked Then
                CreateValueCSV()
                prgSave.PerformStep()
            End If
            If chkBasicEstimates.Checked Then
                CreateBasicEstimatesCSV()
                prgSave.PerformStep()
            End If
            If chkKeyIndices.Checked Then
                CreateKeyIndicesCSV()
                prgSave.PerformStep()
            End If
            If chkMortalityCoefficients.Checked Then
                CreateInitMortCoeffsCSV()
                prgSave.PerformStep()
            End If
            If chkInitPredMort.Checked Then
                CreateInitPredMortCSV()
                prgSave.PerformStep()
            End If
            If chkInitFishMort.Checked Then
                CreateInitFishingMortCSV()
                prgSave.PerformStep()
            End If
            If chkInitConsumption.Checked Then
                CreateInitConsumptionCSV()
                prgSave.PerformStep()
            End If
            If chkRespiration.Checked Then
                CreateRespirationCSV()
                prgSave.PerformStep()
            End If
            If chkPreyOverlap.Checked Then
                CreateOverlapPreyCSV()
                prgSave.PerformStep()
            End If
            If chkPredOverlap.Checked Then
                CreateOverlapPredCSV()
                prgSave.PerformStep()
            End If
            If chkElectivity.Checked Then
                CreateElectivityCSV()
                prgSave.PerformStep()
            End If
            If chkInitFishingQuantities.Checked Then
                CreateInitFishingQuantitiesCSV()
                prgSave.PerformStep()
            End If
            If chkSearchRates.Checked Then
                CreateSearchRatesCSV()
                prgSave.PerformStep()
            End If
            If chkInitFishingValues.Checked Then
                CreateInitFishingValuesCSV()
                prgSave.PerformStep()
            End If
            If chkresiduals.Checked Then
                CreateResiduals()
                prgSave.PerformStep()
            End If
            If chkSS.Checked Then
                CreateSS()
                prgSave.PerformStep()
            End If

            prgSave.Visible = False
            lblPrgInfo.Hide()

            ' Export all data
            Dim msg As cMessage = DataOutputter.OutputData()

            ' Send status message to the rest of the world
            If (msg IsNot Nothing) Then
                Me.Core.Messages.SendMessage(msg)
            End If

            Me.Close()

        End If

        ResetForm()

    End Sub

    Private Sub CreateBiomassCSV()

        Dim EwEIndex As Integer 'Index of group in EwE datastructure
        Dim YearlyBiomass As Single 'Holds the cumulative yearly biomass so that an average can be calced
        Dim Biomass As cDataSheet = New cDataSheet

        'Holds the array of data for all selected groups
        Dim ABiomass(,) As Object = Nothing

        If chkYearly.Checked Then
            ReDim ABiomass(ParentOnlySelection.CountSelected, Core.nEcosimYears)
        Else
            ReDim ABiomass(ParentOnlySelection.CountSelected, nDataRows)
        End If

        'Gets a list of names for the selected groups

        Dim SelectedNames As List(Of String) = ParentOnlySelection.SelectedNames
        For x = 1 To SelectedNames.Count
            ABiomass(x, 0) = ParentOnlySelection.SelectedNames(x - 1)
        Next

        'Loops for each group in selected
        For ParentIndex = 0 To SelectedNames.Count - 1

            'Finds index for group wanting to get biomass of
            EwEIndex = GetIndexGroup(SelectedNames(ParentIndex))

            'Loop through EwE datastructure getting biomass for current group at each timestep
            If chkYearly.Checked Then
                For Year As Integer = 1 To Core.nEcosimYears
                    YearlyBiomass = 0
                    For Month As Integer = 1 To cCore.N_MONTHS
                        YearlyBiomass += Core.EcoSimGroupOutputs(EwEIndex).Biomass((Year - 1) * cCore.N_MONTHS + Month)
                    Next
                    ABiomass(ParentIndex + 1, Year) = YearlyBiomass / cCore.N_MONTHS
                Next
            Else
                For TimeStep As Integer = 1 To nDataRows
                    ABiomass(ParentIndex + 1, TimeStep) = Core.EcoSimGroupOutputs(EwEIndex).Biomass(TimeStep)
                Next
            End If

        Next

        Biomass.Name = My.Resources.BIOMASS
        Biomass.Data = ABiomass

        DataOutputter.AddFunctionalGroup(Biomass)


    End Sub

    Private Sub CreateBiomassIntegratedCSV()

        'Holds the array of data for all selected groups
        Dim ABiomassInteg(ParentOnlySelection.CountSelected - 1, 1) As Object
        Dim BiomassInteg As cDataSheet = New cDataSheet
        Dim StartStepBiomass As Single
        Dim EndStepBiomass As Single
        Dim IntegStep As Single

        'Index of group in EwE datastructure
        Dim EwEIndex As Integer

        'Gets a list of names for the selected groups
        Dim SelectedNames As List(Of String) = ParentOnlySelection.SelectedNames
        For x = 1 To ParentOnlySelection.SelectedNames.Count
            ABiomassInteg(x - 1, 0) = ParentOnlySelection.SelectedNames(x - 1)
        Next

        'Loops for each group in selected
        For ParentIndex = 0 To SelectedNames.Count - 1

            'Finds index for group wanting to get biomass of
            EwEIndex = GetIndexGroup(SelectedNames(ParentIndex))

            'IntegStep holds cummulative total of integrated biomass for calculated final total integ
            IntegStep = 0

            For TimeStep As Integer = 2 To Core.nEcosimTimeSteps

                'Remember that Biomass is changed to difference from initial biomass
                StartStepBiomass = Core.EcoSimGroupOutputs(EwEIndex).Biomass(TimeStep - 1) _
                    - Core.EcoPathGroupOutputs(EwEIndex).Biomass
                EndStepBiomass = Core.EcoSimGroupOutputs(EwEIndex).Biomass(TimeStep) _
                    - Core.EcoPathGroupOutputs(EwEIndex).Biomass

                'Calc. Integ. for step
                IntegStep += (StartStepBiomass + EndStepBiomass) / (2 * cCore.N_MONTHS) 'Gives units tons*year

                'Add step to array
            Next

            ABiomassInteg(ParentIndex, 1) = IntegStep
        Next

        'REDUND.
        'SendToFileTabbed(ABiomassInteg, SelectedNames, TabName:="BiomassIntegrated", _
        '                FileName:=FuncGroupsFileName, sheet:=sheet, wb:=FunctGroupWB)

        'Setup object for datasheet and send to dataoutputer
        BiomassInteg.Name = My.Resources.BIOMASSINTEG
        BiomassInteg.Data = ABiomassInteg
        DataOutputter.AddFunctionalGroup(BiomassInteg)

    End Sub

    Private Sub CreateConsumptionCSV(ByVal CurrentPredator As cCreatedObjects)

        Dim AConsPerPrey(,) As Object
        Dim ConsPerPrey As cDataSheet = New cDataSheet
        Dim PreyNames As New StringBuilder()    'to create prey names for top .CSV file
        Dim PredatorIndexEcosim As Integer      'holds index in EwE m_core of Pred
        Dim PreyIndexEcosim As Integer          'holds index in EwE m_core of Prey
        Dim ConsumpCumul As Single              'use to calculate the total consumpt each year

        'Index of group in EwE datastructure
        Dim EwEIndex As Integer
        'Current Parent-Child Object

        'Gets a list of names for the selected objects
        Dim SelectedNames As List(Of String) = PredatorPreySelection.SelectedNames

        'Get Predator index in EcoSim
        PredatorIndexEcosim = GetIndexGroup(CurrentPredator.ParentName)

        'Runs only if prey>0
        If CurrentPredator.CountChild > 0 Then

            'Find PredatorIndexEcosim in m_core.EcoSimGroupOutputs(PredatorIndexEcosim) for PredatorIndex
            EwEIndex = GetIndexGroup(CurrentPredator.ParentName)

            'Dim array for holding consumption values for each predprey
            AConsPerPrey = Nothing
            ReDim AConsPerPrey(CurrentPredator.CountChild - 1, nDataRows + 1)

            'Setup the titles on sheet
            'AConsPerPrey(0, 0) = CurrentPredator.ParentName
            For x = 1 To CurrentPredator.ChildNames.Count
                AConsPerPrey(x - 1, 0) = CurrentPredator.ChildNames(x - 1)
            Next

            For PreyIndex As Integer = 0 To CurrentPredator.CountChild - 1

                'Find PreyIndexEcosim in m_core.EcoSimGroupOutputs(PredatorIndexEcosim) for PreyIndex
                PreyIndexEcosim = GetIndexGroup(CurrentPredator.ChildNames(PreyIndex))

                'Calculate consumption values for each prey of each predator for each year
                If chkYearly.Checked Then
                    For Year As Integer = 1 To Core.nEcosimYears
                        ConsumpCumul = 0
                        For Month As Integer = 1 To cCore.N_MONTHS
                            ConsumpCumul += Core.EcoSimGroupOutputs(PredatorIndexEcosim).PreyPercentage(PreyIndexEcosim, (Year - 1) * cCore.N_MONTHS + Month) _
                                * Core.EcoSimGroupOutputs(PredatorIndexEcosim).Biomass((Year - 1) * cCore.N_MONTHS + Month) _
                                * Core.EcoSimGroupOutputs(PredatorIndexEcosim).ConsumpBiomass((Year - 1) * cCore.N_MONTHS + Month)


                        Next
                        AConsPerPrey(PreyIndex, Year + 1) = ConsumpCumul / cCore.N_MONTHS
                    Next
                Else
                    For TimeStep As Integer = 1 To nDataRows
                        AConsPerPrey(PreyIndex, TimeStep) =
                            Core.EcoSimGroupOutputs(PredatorIndexEcosim).PreyPercentage(PreyIndexEcosim, TimeStep) _
                            * Core.EcoSimGroupOutputs(PredatorIndexEcosim).Biomass(TimeStep) _
                            * Core.EcoSimGroupOutputs(PredatorIndexEcosim).ConsumpBiomass(TimeStep)
                    Next
                End If

            Next

            'Setup object for datasheet and send to dataoutputer
            ConsPerPrey.Name = My.Resources.CONSUMPT & "_" & Mid(CurrentPredator.ParentName, 1, 22)
            ConsPerPrey.Data = AConsPerPrey
            DataOutputter.AddFunctionalGroup(ConsPerPrey)

            '            SendToFileTabbed(AConsPerPrey, CurrentPredator.ChildNames, _
            '                            TabName:="Consumpt_" & Mid(CurrentPredator.ParentName, 1, 22), _
            '                           FileName:=FuncGroupsFileName, Sheet:=sheet, wb:=FunctGroupWB)
        End If

    End Sub

    'Retrieves the F on each group
    Private Sub CreateFishingMortalityCSV()

        Dim AFishingMortality(ParentOnlySelection.CountSelected - 1, nDataRows) As Object
        Dim CumulFishingMortality As Single
        Dim FishingMortality As cDataSheet = New cDataSheet

        'Index of group in EwE datastructure
        Dim EwEIndex As Integer

        'Set sheet titles
        For x = 1 To ParentOnlySelection.CountSelected
            AFishingMortality(x - 1, 0) = ParentOnlySelection.SelectedNames(x - 1)
        Next


        For ParentIndex As Integer = 0 To ParentOnlySelection.CountSelected - 1
            'Get Index of Parent in EwE
            EwEIndex = GetIndexGroup(ParentOnlySelection.SelectedNames(ParentIndex))

            If chkYearly.Checked Then

                For Year As Integer = 1 To Core.nEcosimYears
                    CumulFishingMortality = 0
                    For Month As Integer = 1 To cCore.N_MONTHS
                        'Retrieve Fishing mortality for parent
                        CumulFishingMortality +=
                                        Core.EcoSimGroupOutputs(EwEIndex).FishMort((Year - 1) * cCore.N_MONTHS + Month) -
                                        Core.EcoSimGroupOutputs(EwEIndex).PredMort((Year - 1) * cCore.N_MONTHS + Month)
                    Next
                    AFishingMortality(ParentIndex, Year) = CumulFishingMortality / cCore.N_MONTHS
                Next
            Else
                For TimeStep As Integer = 1 To nDataRows

                    'Retrieve Fishing mortality for parent
                    AFishingMortality(ParentIndex, TimeStep) =
                                    Core.EcoSimGroupOutputs(EwEIndex).FishMort(TimeStep) -
                                    Core.EcoSimGroupOutputs(EwEIndex).PredMort(TimeStep)
                Next
            End If

        Next

        'Setup object for datasheet and send to dataoutputer
        FishingMortality.Name = My.Resources.FISHMORT_ALLFLEET
        FishingMortality.Data = AFishingMortality
        DataOutputter.AddFunctionalGroup(FishingMortality)

        'SendToFileTabbed(AFishingMortality, ParentOnlySelection.SelectedNames, _
        '            FileName:=FuncGroupsFileName, Sheet:=sheet, TabName:="FishMortAllFleet", _
        '          wb:=FunctGroupWB)

    End Sub

    Private Sub CreatePredationMortalityCSV()

        'Dim APredationMortality(APredPreySelection.Count - 1, m_core.nEcosimTimeSteps - 1) As Single
        Dim APredationMortality(ParentOnlySelection.CountSelected - 1, nDataRows) As Object
        Dim PredationMortality As cDataSheet = New cDataSheet
        Dim CumulPredationMortality As Single

        'Index of group in EwE datastructure
        Dim EwEIndex As Integer

        'Set the sheet titles
        For x = 1 To ParentOnlySelection.CountSelected
            APredationMortality(x - 1, 0) = ParentOnlySelection.SelectedNames(x - 1)
        Next

        If chkYearly.Checked Then
            For PredatorIndex As Integer = 0 To ParentOnlySelection.CountSelected - 1

                'Get Index of Parent in EwE
                EwEIndex = GetIndexGroup(ParentOnlySelection.SelectedNames(PredatorIndex))

                For Year As Integer = 1 To Core.nEcosimYears
                    CumulPredationMortality = 0
                    For Month As Integer = 1 To cCore.N_MONTHS
                        'Retrieve Predation mortality for parent
                        CumulPredationMortality +=
                                            Core.EcoSimGroupOutputs(EwEIndex).PredMort((Year - 1) * cCore.N_MONTHS + Month)

                    Next
                    APredationMortality(PredatorIndex, Year) = CumulPredationMortality / cCore.N_MONTHS
                Next
            Next
        Else
            For PredatorIndex As Integer = 0 To ParentOnlySelection.CountSelected - 1
                For TimeStep As Integer = 1 To nDataRows

                    'Get Index of Parent in EwE
                    EwEIndex = GetIndexGroup(ParentOnlySelection.SelectedNames(PredatorIndex))
                    'retrieve mortality for current predator at current timestep
                    APredationMortality(PredatorIndex, TimeStep) =
                    Core.EcoSimGroupOutputs(EwEIndex).PredMort(TimeStep)

                Next
            Next
        End If

        'Setup dataobject and add to outputter
        PredationMortality.Name = My.Resources.PREDMORT
        PredationMortality.Data = APredationMortality
        DataOutputter.AddFunctionalGroup(PredationMortality)

        'SendToFileTabbed(APredationMortality, ParentOnlySelection.SelectedNames, _
        '                 TabName:="PredMort", FileName:=FuncGroupsFileName, _
        '                 sheet:=sheet, wb:=FunctGroupWB)

    End Sub

    Private Sub CreatePredationMortalityEachPredatorCSV()

        'Count number of childs for all prey objects to dimension array holding mortalities
        Dim NumberOfChilds As Integer = 0
        For Each prey In PreyPredatorSelection.GetSelected
            NumberOfChilds += prey.CountChild
        Next
        Dim APredationMortality(NumberOfChilds - 1, nDataRows + 1) As Object
        Dim PredationMortality As cDataSheet = New cDataSheet
        Dim CumPredMort As Single

        'Index of group in EwE datastructure
        Dim EwEIndexPredator As Integer
        Dim EwEIndexPrey As Integer
        'Init column pointer
        Dim ColPointer As Integer = 0
        Dim Consumption As Single
        Dim CurrentPrey As cCreatedObjects
        Dim FileHeader As String = Nothing


        'Create Titles
        For Each prey In PreyPredatorSelection.GetSelected
            APredationMortality(ColPointer, 0) = prey.ParentName
            For Each pred In prey.ChildNames
                APredationMortality(ColPointer, 1) = pred
                ColPointer += 1
            Next
        Next

        ColPointer = 0

        For PreyIndex As Integer = 0 To PreyPredatorSelection.CountSelected - 1

            CurrentPrey = PreyPredatorSelection.GetSelected(PreyIndex)
            EwEIndexPrey = GetIndexGroup(CurrentPrey.ParentName)

            For PredatorIndex As Integer = 0 To CurrentPrey.CountChild - 1

                EwEIndexPredator = GetIndexGroup(CurrentPrey.ChildNames(PredatorIndex))

                If chkYearly.Checked Then
                    For nYear As Integer = 1 To Core.nEcosimYears
                        CumPredMort = 0
                        For nMonth As Integer = 1 To cCore.N_MONTHS
                            Consumption =
                                Core.EcoSimGroupOutputs(EwEIndexPredator).PreyPercentage(EwEIndexPrey, (nYear - 1) * cCore.N_MONTHS + nMonth) _
                                * Core.EcoSimGroupOutputs(EwEIndexPredator).Biomass((nYear - 1) * cCore.N_MONTHS + nMonth) _
                                * Core.EcoSimGroupOutputs(EwEIndexPredator).ConsumpBiomass((nYear - 1) * cCore.N_MONTHS + nMonth)
                            CumPredMort += Consumption / Core.EcoSimGroupOutputs(EwEIndexPrey).Biomass((nYear - 1) * cCore.N_MONTHS + nMonth)
                        Next
                        APredationMortality(ColPointer, nYear + 1) = CumPredMort / cCore.N_MONTHS
                    Next
                Else
                    For TimeStep As Integer = 1 To nDataRows
                        Consumption =
                            Core.EcoSimGroupOutputs(EwEIndexPredator).PreyPercentage(EwEIndexPrey, TimeStep) _
                            * Core.EcoSimGroupOutputs(EwEIndexPredator).Biomass(TimeStep) _
                            * Core.EcoSimGroupOutputs(EwEIndexPredator).ConsumpBiomass(TimeStep)

                        APredationMortality(ColPointer, TimeStep + 1) = Consumption / Core.EcoSimGroupOutputs(EwEIndexPrey).Biomass(TimeStep)
                    Next
                End If

                ColPointer += 1

            Next
        Next

        'Setup object and add to data outputter
        PredationMortality.Name = My.Resources.PREDMORT_EACH_PRED
        PredationMortality.Data = APredationMortality
        DataOutputter.AddFunctionalGroup(PredationMortality)

        'SendToFileTabbed(APredationMortality, PreyPredatorSelection.GetSelected, _
        '                TabName:="PredMortEachPred", FileName:=FuncGroupsFileName, _
        '               sheet:=sheet, wb:=FunctGroupWB)

    End Sub

    'Retrieves the partial F's on each group
    Private Sub CreateMortalityByFleetCSV()

        'Count number of childs for all prey objects to dimension array holding mortalities
        Dim NumberOfChilds As Integer = 0
        For Each prey In FleetPreySelection.GetSelected
            NumberOfChilds += prey.CountChild
        Next
        Dim AFishingMortality(NumberOfChilds - 1, nDataRows + 1) As Object
        Dim FishingMortality As cDataSheet = New cDataSheet
        Dim CumulFishingMort As Single

        'Index of group in EwE datastructure
        Dim EwEIndexFleet As Integer
        Dim EwEIndexPrey As Integer
        'Init column pointer
        Dim ColPointer As Integer = 0
        Dim FleetCatch As Single
        Dim Biomass As Single
        Dim CurrentFleet As cCreatedObjects
        Dim FileHeader As String = Nothing

        'Create sheet titles
        For Each fleet In FleetPreySelection.GetSelected
            AFishingMortality(ColPointer, 0) = fleet.ParentName
            For Each prey In fleet.ChildNames
                AFishingMortality(ColPointer, 1) = prey
                ColPointer += 1
            Next
        Next
        ColPointer = 0

        For FleetIndex As Integer = 0 To FleetPreySelection.CountSelected - 1
            CurrentFleet = FleetPreySelection.GetSelected(FleetIndex)

            'Get Index of fleet in EwE
            For i = 0 To Core.nFleets
                If Core.EcosimFleetOutput(i).Name = CurrentFleet.ParentName Then
                    EwEIndexFleet = i
                    Exit For
                End If
            Next

            For PreyIndex As Integer = 0 To CurrentFleet.CountChild - 1
                EwEIndexPrey = GetIndexGroup(CurrentFleet.ChildNames(PreyIndex))
                If chkYearly.Checked Then
                    For nYear As Integer = 1 To Core.nEcosimYears
                        CumulFishingMort = 0
                        For nMonth As Integer = 1 To cCore.N_MONTHS
                            FleetCatch = Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, (nYear - 1) * cCore.N_MONTHS + nMonth)
                            Biomass = Core.EcoSimGroupOutputs(EwEIndexPrey).Biomass((nYear - 1) * cCore.N_MONTHS + nMonth)
                            CumulFishingMort += FleetCatch / Biomass
                        Next
                        AFishingMortality(ColPointer, nYear + 1) = CumulFishingMort / cCore.N_MONTHS
                    Next
                Else
                    For TimeStep As Integer = 1 To nDataRows
                        'Get Catch Biomass
                        FleetCatch = Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, TimeStep)
                        Biomass = Core.EcoSimGroupOutputs(EwEIndexPrey).Biomass(TimeStep)
                        AFishingMortality(ColPointer, TimeStep + 1) = FleetCatch / Biomass
                    Next
                End If
                ColPointer += 1
            Next

        Next

        'Setup data sheet and send to data outputter
        FishingMortality.Name = My.Resources.FISHMORT_PER_FLEET
        FishingMortality.Data = AFishingMortality
        DataOutputter.AddFisheries(FishingMortality)

        'SendToFileTabbed(AFishingMortality, FleetPreySelection.GetSelected, _
        '                 FileName:=FishFleetsFileName, sheet:=sheet, TabName:="FishMortPerFleet", _
        '                 wb:=FisheriesWB)

    End Sub

    'Calculates effort time series for each fleet
    Private Sub CreateEffort()

        Dim AEffort(FleetOnlySelection.CountSelected - 1, nDataRows + 1) As Object
        Dim Effort As cDataSheet = New cDataSheet
        Dim PartialF As Single
        Dim InitialPartialF As Single
        Dim ColPointer As Integer = 0
        Dim CumulEffort As Single

        'Index of group in EwE datastructure
        Dim EwEIndexFleet As Integer
        Dim EwEIndexPrey As Integer

        'Setup sheet titles
        For Each Fleet In FleetOnlySelection.SelectedNames
            AEffort(ColPointer, 0) = Fleet
            ColPointer += 1
        Next

        For FleetIndex As Integer = 0 To FleetOnlySelection.CountSelected - 1

            'Get Index of fleet in EwE
            EwEIndexFleet = 0
            For i = 0 To Core.nFleets
                If Core.EcosimFleetOutput(i).Name = FleetOnlySelection.SelectedNames(FleetIndex) Then
                    EwEIndexFleet = i
                    Exit For
                End If
            Next

            If EwEIndexFleet <> 0 Then

                'Find a functional group that is caught by fleet
                EwEIndexPrey = 1
                While Core.EcopathFleetInputs(EwEIndexFleet).Landings(EwEIndexPrey) = 0 Or EwEIndexFleet > Core.nGroups
                    EwEIndexPrey += 1
                End While

                If EwEIndexFleet > Core.nGroups Then Exit Sub

                'Calculate initial partialF
                InitialPartialF = (Core.EcopathFleetInputs(EwEIndexFleet).Landings(EwEIndexPrey) + _
                                    Core.EcopathFleetInputs(EwEIndexFleet).Discards(EwEIndexPrey)) _
                                    / Core.EcoPathGroupOutputs(EwEIndexPrey).Biomass

                'Calculate efforts
                AEffort(FleetIndex, 1) = 1
                If chkYearly.Checked Then
                    For nYear As Integer = 1 To Core.nEcosimYears
                        CumulEffort = 0
                        For nMonth As Integer = 1 To cCore.N_MONTHS
                            PartialF = Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, (nYear - 1) * cCore.N_MONTHS + nMonth) /
                                        Core.EcoSimGroupOutputs(EwEIndexPrey).Biomass((nYear - 1) * cCore.N_MONTHS + nMonth)
                            CumulEffort += PartialF
                        Next
                        AEffort(FleetIndex, nYear + 1) = CumulEffort / (cCore.N_MONTHS * InitialPartialF)
                    Next
                Else
                    For TimeStep As Integer = 1 To nDataRows
                        PartialF = Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, TimeStep) /
                                    Core.EcoSimGroupOutputs(EwEIndexPrey).Biomass(TimeStep)
                        AEffort(FleetIndex, TimeStep + 1) = PartialF / InitialPartialF
                    Next
                End If

            Else
                For TimeStep As Integer = 1 To nDataRows + 1
                    AEffort(FleetIndex, TimeStep) = -9999
                Next

            End If
        Next

        'setup data sheet and send to outputter
        Effort.Name = My.Resources.FISHING_EFFORT
        Effort.Data = AEffort
        DataOutputter.AddFisheries(Effort)

        'SendToFileTabbed(AEffort, FleetOnlySelection.SelectedNames, _
        '                 FileName:=FishFleetsFileName, Sheet:=sheet, TabName:="FishingEffort", _
        '                 wb:=FisheriesWB)

    End Sub

    Private Sub CreateCatchCSV()
        Dim EwEIndex As Integer 'Index of group in EwE datastructure
        Dim ColPointer As Integer = 0
        Dim CumulCatch As Single

        'Holds the array of data for all selected groups
        Dim ACatch(ParentOnlySelection.CountSelected - 1, nDataRows) As Object
        Dim SheetCatch As cDataSheet = New cDataSheet

        'Set titles
        For Each Fleet In ParentOnlySelection.SelectedNames
            ACatch(ColPointer, 0) = Fleet
            ColPointer += 1
        Next

        'Loops for each group in selected
        For ParentIndex = 0 To ParentOnlySelection.SelectedNames.Count - 1

            'Finds index for group wanting to get biomass of
            EwEIndex = GetIndexGroup(ParentOnlySelection.SelectedNames(ParentIndex))

            'Loop through EwE datastructure getting Catch for current group at each timestep
            If chkYearly.Checked Then
                For nYear As Integer = 1 To Core.nEcosimYears
                    CumulCatch = 0
                    For nMonth = 1 To cCore.N_MONTHS
                        CumulCatch += Core.EcoSimGroupOutputs(EwEIndex).Catch((nYear - 1) * cCore.N_MONTHS + nMonth)
                    Next
                    ACatch(ParentIndex, nYear) = CumulCatch / cCore.N_MONTHS
                Next
            Else
                For TimeStep As Integer = 1 To nDataRows
                    ACatch(ParentIndex, TimeStep) = Core.EcoSimGroupOutputs(EwEIndex).Catch(TimeStep)
                Next
            End If

        Next

        'Setup datasheet and send to outputter
        SheetCatch.Name = My.Resources.CATCH_
        SheetCatch.Data = ACatch
        DataOutputter.AddFunctionalGroup(SheetCatch)

        'SendToFileTabbed(ACatch, SelectedNames, FileName:=FuncGroupsFileName, _
        '             sheet:=sheet, TabName:="Catch", wb:=FunctGroupWB)

    End Sub

    Private Sub CreateCatchByFleetCSV()
        Dim EwEIndexFleet As Integer 'Index of group in EwE datastructure
        Dim EwEIndexPrey As Integer
        Dim ColPointer As Integer = 0 'To track col in array to put data
        Dim ColTitles As String = Nothing 'Title of columns in .CSV file
        Dim CumulCatch As Single
        'Used to hold ratio to seperate catch into discards and landings (should sum to 1)

        'Holds the array of data for all selected groups
        Dim ACatchByFleet(FleetPreySelection.CountSelectedChild - 1, nDataRows + 1) As Object
        Dim CatchByFleet As cDataSheet = New cDataSheet

        'Gets a list of names for the selected groups
        Dim SelectedObjects As List(Of cCreatedObjects) = FleetPreySelection.GetSelected

        'Create sheet titles
        For Each fleet In FleetPreySelection.GetSelected
            ACatchByFleet(ColPointer, 0) = fleet.ParentName
            For Each prey In fleet.ChildNames
                ACatchByFleet(ColPointer, 1) = prey
                ColPointer += 1
            Next
        Next
        ColPointer = 0


        'Loops for each group in selected
        For FleetIndex = 0 To SelectedObjects.Count - 1

            'Finds index for group wanting to get values of
            EwEIndexFleet = GetIndexFleet(SelectedObjects(FleetIndex).ParentName)

            'Loop for each prey
            For Each Prey In SelectedObjects(FleetIndex).ChildNames
                EwEIndexPrey = GetIndexGroup(Prey)

                'Loop through EwE datastructure getting biomass for current group at each timestep
                If chkYearly.Checked Then
                    For nYear As Integer = 1 To Core.nEcosimYears
                        CumulCatch = 0
                        For nMonth = 1 To cCore.N_MONTHS
                            CumulCatch += Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, (nYear - 1) * cCore.N_MONTHS + nMonth)
                        Next
                        ACatchByFleet(ColPointer, nYear + 1) = CumulCatch / cCore.N_MONTHS
                    Next
                Else
                    For TimeStep As Integer = 1 To Core.nEcosimTimeSteps
                        ACatchByFleet(ColPointer, TimeStep + 1) = Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, TimeStep)
                    Next
                End If

                ColPointer += 1

            Next
        Next

        'Setup data sheet and send to dataoutputer
        CatchByFleet.Name = My.Resources.CATCH_PER_FLEETGROUP
        CatchByFleet.Data = ACatchByFleet
        DataOutputter.AddFisheries(CatchByFleet)

        'SendToFileTabbed(ACatchByFleet, SelectedObjects, _
        '        FileName:=FishFleetsFileName, sheet:=sheet, _
        '        TabName:="CatchPerFleetPerPrey", wb:=FisheriesWB)


    End Sub

    Private Sub CreateLandingsByFleetCSV()
        Dim EwEIndexFleet As Integer 'Index of group in EwE datastructure
        Dim EwEIndexPrey As Integer
        Dim ColPointer As Integer = 0 'To track col in array to put data
        Dim ColTitles As String = Nothing 'Title of columns in .CSV file
        'Used to hold ratio to seperate catch into discards and landings (should sum to 1)
        Dim PropLandings As Single
        Dim Landings As Single
        Dim Discards As Single

        'Holds the array of data for all selected groups
        Dim ACatchByFleet(FleetPreySelection.CountSelectedChild - 1, nDataRows - 1) As Single
        Dim ALandingsByFleet(FleetPreySelection.CountSelectedChild - 1, nDataRows + 1) As Object

        Dim SheetLandings As cDataSheet = New cDataSheet

        Dim SelectedObjects As List(Of cCreatedObjects) = FleetPreySelection.GetSelected

        'Create sheet titles
        For Each fleet In FleetPreySelection.GetSelected
            ALandingsByFleet(ColPointer, 0) = fleet.ParentName
            For Each prey In fleet.ChildNames
                ALandingsByFleet(ColPointer, 1) = prey
                ColPointer += 1
            Next
        Next
        ColPointer = 0

        'Loops for each group in selected
        For FleetIndex = 0 To SelectedObjects.Count - 1

            'Finds index for group wanting to get values of
            EwEIndexFleet = GetIndexFleet(SelectedObjects(FleetIndex).ParentName)

            'Loop for each prey
            For Each Prey In SelectedObjects(FleetIndex).ChildNames
                EwEIndexPrey = GetIndexGroup(Prey)

                'Calculate proportion of catch is landings and discards _
                'for given fleet and group
                Landings = 0
                Discards = 0
                If EwEIndexFleet = 0 Then
                    For i = 1 To Core.nFleets
                        Landings += Core.EcopathFleetInputs(i).Landings(EwEIndexPrey)
                        Discards += Core.EcopathFleetInputs(i).Discards(EwEIndexPrey)
                    Next
                Else
                    Landings = Core.EcopathFleetInputs(EwEIndexFleet).Landings(EwEIndexPrey)
                    Discards = Core.EcopathFleetInputs(EwEIndexFleet).Discards(EwEIndexPrey)
                End If
                PropLandings = Landings / (Landings + Discards)

                'Loop through EwE datastructure getting biomass for current group at each timestep
                If chkYearly.Checked Then
                    For nYear As Integer = 1 To Core.nEcosimYears
                        For nMonth As Integer = 1 To cCore.N_MONTHS
                            ACatchByFleet(ColPointer, nYear - 1) += Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, (nYear - 1) * cCore.N_MONTHS + nMonth)
                        Next
                        ALandingsByFleet(ColPointer, nYear + 1) = ACatchByFleet(ColPointer, nYear - 1) * PropLandings / cCore.N_MONTHS
                    Next
                Else
                    For TimeStep As Integer = 1 To Core.nEcosimTimeSteps
                        ACatchByFleet(ColPointer, TimeStep - 1) = Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, TimeStep)
                        ALandingsByFleet(ColPointer, TimeStep + 1) = ACatchByFleet(ColPointer, TimeStep - 1) * PropLandings
                    Next
                End If
                ColPointer += 1

            Next
        Next

        'setup sheet and send to dataoutputter
        SheetLandings.Data = ALandingsByFleet
        DataOutputter.AddFisheries(SheetLandings)

        'SendToFileTabbed(ALandingsByFleet, SelectedObjects, _
        '        FileName:=FishFleetsFileName, sheet:=sheet, _
        '        TabName:="LandingsPerFleetPerPrey", wb:=FisheriesWB)

    End Sub

    Private Sub CreateDiscardsByFleetCSV()
        Dim EwEIndexFleet As Integer 'Index of group in EwE datastructure
        Dim EwEIndexPrey As Integer
        Dim ColPointer As Integer = 0 'To track col in array to put data
        Dim ColTitles As String = Nothing 'Title of columns in .CSV file
        'Used to hold ratio to seperate catch into discards and landings (should sum to 1)
        Dim PropLandings As Single
        Dim PropDiscards As Single
        Dim Landings As Single
        Dim Discards As Single

        'Holds the array of data for all selected groups
        Dim ACatchByFleet(FleetPreySelection.CountSelectedChild - 1, nDataRows - 1) As Single
        Dim ALandingsByFleet(FleetPreySelection.CountSelectedChild - 1, nDataRows - 1) As Single
        Dim ADiscardsByFleet(FleetPreySelection.CountSelectedChild - 1, nDataRows + 1) As Object

        Dim SheetDiscards As cDataSheet = New cDataSheet

        Dim SelectedObjects As List(Of cCreatedObjects) = FleetPreySelection.GetSelected

        'Create sheet titles
        For Each fleet In FleetPreySelection.GetSelected
            ADiscardsByFleet(ColPointer, 0) = fleet.ParentName
            For Each prey In fleet.ChildNames
                ADiscardsByFleet(ColPointer, 1) = prey
                ColPointer += 1
            Next
        Next
        ColPointer = 0

        'Loops for each group in selected
        For FleetIndex = 0 To SelectedObjects.Count - 1

            'Finds index for group wanting to get values of
            EwEIndexFleet = GetIndexFleet(SelectedObjects(FleetIndex).ParentName)

            'Loop for each prey
            For Each Prey In SelectedObjects(FleetIndex).ChildNames
                EwEIndexPrey = GetIndexGroup(Prey)

                'Calculate proportion of catch is landings and discards _
                'for given fleet and group
                Landings = 0
                Discards = 0
                If EwEIndexFleet = 0 Then
                    For i = 1 To Core.nFleets
                        Landings += Core.EcopathFleetInputs(i).Landings(EwEIndexPrey)
                        Discards += Core.EcopathFleetInputs(i).Discards(EwEIndexPrey)
                    Next
                Else
                    Landings = Core.EcopathFleetInputs(EwEIndexFleet).Landings(EwEIndexPrey)
                    Discards = Core.EcopathFleetInputs(EwEIndexFleet).Discards(EwEIndexPrey)
                End If
                PropLandings = Landings / (Landings + Discards)
                PropDiscards = Discards / (Landings + Discards)

                'Loop through EwE datastructure getting discards for current group at each timestep
                If chkYearly.Checked Then
                    For nYear As Integer = 1 To Core.nEcosimYears
                        For nMonth As Integer = 1 To cCore.N_MONTHS
                            ACatchByFleet(ColPointer, nYear - 1) += Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, (nYear - 1) * cCore.N_MONTHS + nMonth)
                        Next
                        ADiscardsByFleet(ColPointer, nYear + 1) = ACatchByFleet(ColPointer, nYear - 1) * PropDiscards / cCore.N_MONTHS
                    Next
                Else
                    For TimeStep As Integer = 1 To Core.nEcosimTimeSteps
                        ACatchByFleet(ColPointer, TimeStep - 1) = Core.EcoSimGroupOutputs(EwEIndexPrey).CatchByFleet(EwEIndexFleet, TimeStep)
                        ADiscardsByFleet(ColPointer, TimeStep + 1) = ACatchByFleet(ColPointer, TimeStep - 1) * PropDiscards
                    Next
                End If

                ColPointer += 1

            Next
        Next

        'setupsheet and send to outputter
        SheetDiscards.Name = My.Resources.DISCARDS_PER_FLEETPREY
        SheetDiscards.Data = ADiscardsByFleet
        DataOutputter.AddFisheries(SheetDiscards)

        'SendToFileTabbed(ADiscardsByFleet, SelectedObjects, _
        '        FileName:=FishFleetsFileName, sheet:=sheet, _
        '        TabName:="DiscardsPerFleetPerPrey", wb:=FisheriesWB)

    End Sub

    Private Sub CreateDietCSV(ByVal CurrentPredator As cCreatedObjects)
        'Holds the diet of each prey at each time step for given predator
        Dim ADietOfPredator(,) As Object
        Dim PreyNames As New StringBuilder()    'to create prey names for top .CSV file
        Dim PredatorIndexEcosim As Integer      'holds index in EwE m_core of Pred
        Dim PreyIndexEcosim As Integer          'holds index in EwE m_core of Prey
        Dim DietOfPredator As New cDataSheet    'the datasheet to send to dataoutputer
        Dim CumulDiet As Single                 'used to total all diet values ups

        'Runs only if prey>0
        If CurrentPredator.CountChild > 0 Then

            'Get Predator index in EcoSim
            PredatorIndexEcosim = GetIndexGroup(CurrentPredator.ParentName)

            'Dim array for holding consumption values for each predprey
            ADietOfPredator = Nothing
            If chkYearly.Checked Then
                ReDim ADietOfPredator(CurrentPredator.CountChild - 1, Core.nEcosimYears)
            Else
                ReDim ADietOfPredator(CurrentPredator.CountChild - 1, nDataRows)
            End If

            'Setup titles of sheet
            For x = 1 To CurrentPredator.CountChild
                ADietOfPredator(x - 1, 0) = CurrentPredator.ChildNames(x - 1)
            Next

            For PreyIndex As Integer = 0 To CurrentPredator.CountChild - 1

                'Find PreyIndexEcosim in m_core.EcoSimGroupOutputs(PredatorIndexEcosim) for PreyIndex
                PreyIndexEcosim = GetIndexGroup(CurrentPredator.ChildNames(PreyIndex))

                'Calculate consumption values for each prey of each predator for each year
                If chkYearly.Checked Then
                    For nYear As Integer = 1 To Core.nEcosimYears
                        CumulDiet = 0
                        For nMonth As Integer = 1 To cCore.N_MONTHS
                            CumulDiet += Core.EcoSimGroupOutputs(PredatorIndexEcosim).PreyPercentage(PreyIndexEcosim, (nYear - 1) * cCore.N_MONTHS + nMonth)
                        Next
                        ADietOfPredator(PreyIndex, nYear) = CumulDiet / cCore.N_MONTHS
                    Next
                Else
                    For TimeStep As Integer = 1 To nDataRows
                        ADietOfPredator(PreyIndex, TimeStep) = Core.EcoSimGroupOutputs(PredatorIndexEcosim).PreyPercentage(PreyIndexEcosim, TimeStep)
                    Next
                End If

            Next

            'setup datasheet and send to dataoutputter
            DietOfPredator.Name = My.Resources.DIETOF & Mid(CurrentPredator.ParentName, 1, 24)
            DietOfPredator.Data = ADietOfPredator
            DataOutputter.AddFunctionalGroup(DietOfPredator)

            'SendToFileTabbed(ADietOfPredator, CurrentPredator.ChildNames, _
            '    TabName:="DietOf" & Mid(CurrentPredator.ParentName, 1, 24), _
            '    FileName:=FuncGroupsFileName, Sheet:=sheet, wb:=FunctGroupWB)

        End If


    End Sub

    'Creates .CSV for the value of each selected fleet at each timestep
    Private Sub CreateValueCSV()

        Dim EwEIndexFleet As Integer 'Index of group in EwE datastructure
        Dim CumValue As Single 'Holds cumulative value to calc total value

        'Holds the array of data for all selected Fleets
        Dim AValue(FleetOnlySelection.CountSelected - 1, nDataRows) As Object
        'Datasheet
        Dim Value As New cDataSheet

        'Gets a list of names for the selected groups
        Dim SelectedNames As List(Of String) = FleetOnlySelection.SelectedNames

        For x = 1 To FleetOnlySelection.CountSelected
            AValue(x - 1, 0) = FleetOnlySelection.SelectedNames(x - 1)
        Next

        'Loops for each group in selected
        For FleetIndex = 0 To SelectedNames.Count - 1

            'Finds index for group wanting to get biomass of
            EwEIndexFleet = GetIndexFleet(SelectedNames(FleetIndex))

            'Loop through EwE datastructure getting Value for current group at each timestep
            If chkYearly.Checked Then
                For nYear As Integer = 1 To Core.nEcosimYears
                    CumValue = 0
                    For nMonth As Integer = 1 To cCore.N_MONTHS
                        CumValue += Core.EcosimFleetOutput(EwEIndexFleet).Value((nYear - 1) * cCore.N_MONTHS + nMonth)
                    Next
                    AValue(FleetIndex, nYear) = CumValue / cCore.N_MONTHS
                Next

            Else
                For TimeStep As Integer = 1 To nDataRows
                    AValue(FleetIndex, TimeStep) = Core.EcosimFleetOutput(EwEIndexFleet).Value(TimeStep)
                Next
            End If

        Next

        'setup datasheet and send to dataoutputter
        Value.Name = My.Resources.VALUES
        Value.Data = AValue
        DataOutputter.AddFisheries(Value)

        'SendToFileTabbed(AValue, SelectedNames, TabName:="Values", _
        '    FileName:=FishFleetsFileName, Sheet:=sheet, wb:=FisheriesWB)

    End Sub

    Private Sub CreateBasicEstimatesCSV()

        Dim ABasicEstimates(10, Core.nGroups) As Object
        Dim BasicEstimates As New cDataSheet

        Dim parms As EwECore.cEwEModel = Me.Core.EwEModel
        Dim sg As cStyleGuide = Me.m_uic.StyleGuide

        'Setup titles
        ABasicEstimates(0, 0) = My.Resources.INDEX
        ABasicEstimates(1, 0) = My.Resources.GROUP_NAME
        ABasicEstimates(2, 0) = My.Resources.TROPHIC_LEVEL
        ABasicEstimates(3, 0) = My.Resources.HABITAT_AREA_UNITS
        ' I changed the resource string to hold the placeholders for receiving the units ({0}/{1}), where {0} reveives 'biomass', and {1} receives 'time'
        ABasicEstimates(4, 0) = cStringUtils.Localize(My.Resources.BIOMASS_AREA_UNITS, sg.FormatUnitString("[biomass]/[area]")) '  My.Resources.BIOMASS_AREA_UNITS
        ABasicEstimates(5, 0) = My.Resources.BIOMASS_AREA_UNITS
        ABasicEstimates(6, 0) = My.Resources.PRODUCTION_BIOMASS_UNITS
        ABasicEstimates(7, 0) = My.Resources.CONSUMPTION_BIOMASS_UNITS
        ABasicEstimates(8, 0) = My.Resources.ECOTROPHIC_EFFICIENCY
        ABasicEstimates(9, 0) = My.Resources.PRODUCTION_CONSUMPTION

        'Fill out core data
        For Row = 1 To Core.nGroups
            ABasicEstimates(0, Row) = Core.EcoPathGroupOutputs(Row).Index
            ABasicEstimates(1, Row) = Core.EcoPathGroupOutputs(Row).Name
            ABasicEstimates(2, Row) = Core.EcoPathGroupOutputs(Row).TTLX
            ABasicEstimates(3, Row) = Core.EcoPathGroupOutputs(Row).Area
            ABasicEstimates(4, Row) = Core.EcoPathGroupOutputs(Row).BiomassArea
            ABasicEstimates(5, Row) = Core.EcoPathGroupOutputs(Row).Biomass
            ABasicEstimates(6, Row) = Core.EcoPathGroupOutputs(Row).PBOutput
            ABasicEstimates(7, Row) = Core.EcoPathGroupOutputs(Row).QBOutput
            ABasicEstimates(8, Row) = Core.EcoPathGroupOutputs(Row).EEOutput
            ABasicEstimates(9, Row) = Core.EcoPathGroupOutputs(Row).GEOutput
        Next

        'Setup datasheet and send to dataoutputter
        BasicEstimates.Name = My.Resources.BASIC_ESTIMATES
        BasicEstimates.Data = ABasicEstimates
        DataOutputter.AddIndicators(BasicEstimates)

    End Sub

    Private Sub CreateKeyIndicesCSV()
        Dim AKeyIndices(7, Core.nGroups) As Object
        Dim KeyIndices As New cDataSheet

        'Setup titles
        AKeyIndices(0, 0) = My.Resources.INDEX
        AKeyIndices(1, 0) = My.Resources.GROUP_NAME
        AKeyIndices(2, 0) = My.Resources.BIOMASS_ACCUM
        AKeyIndices(3, 0) = My.Resources.BIOMASS_ACCUM_RATE
        AKeyIndices(4, 0) = My.Resources.NET_MIGRATION
        AKeyIndices(5, 0) = My.Resources.FLOW_DETRITUS
        AKeyIndices(6, 0) = My.Resources.NET_EFFICIENCY
        AKeyIndices(7, 0) = My.Resources.OMNIVORY_INDEX

        'Fill out main data
        For Row = 1 To Core.nGroups
            AKeyIndices(0, Row) = Core.EcoPathGroupOutputs(Row).Index
            AKeyIndices(1, Row) = Core.EcoPathGroupOutputs(Row).Name
            AKeyIndices(2, Row) = Core.EcoPathGroupOutputs(Row).BioAccum
            AKeyIndices(3, Row) = Core.EcoPathGroupOutputs(Row).BioAccumRatePerYear
            AKeyIndices(4, Row) = Core.EcoPathGroupOutputs(Row).NetMigration
            AKeyIndices(5, Row) = Core.EcoPathGroupOutputs(Row).FlowToDet
            AKeyIndices(6, Row) = Core.EcoPathGroupOutputs(Row).NetEfficiency
            AKeyIndices(7, Row) = Core.EcoPathGroupOutputs(Row).OmnivoryIndex
        Next

        'Setup datasheet and send to dataoutputer
        KeyIndices.Name = My.Resources.KEY_INDICES
        KeyIndices.Data = AKeyIndices
        DataOutputter.AddIndicators(KeyIndices)

    End Sub

    Private Sub CreateInitMortCoeffsCSV()
        Dim AInitMortCoef(9, Core.nLivingGroups) As Object
        Dim InitMortCoef As New cDataSheet

        'Setup titles
        AInitMortCoef(0, 0) = My.Resources.INDEX
        AInitMortCoef(1, 0) = My.Resources.GROUP_NAME
        AInitMortCoef(2, 0) = My.Resources.PROD_BIOMASS_Z
        AInitMortCoef(3, 0) = My.Resources.FISH_MORT_RATE
        AInitMortCoef(4, 0) = My.Resources.PRED_MORT_RATE
        AInitMortCoef(5, 0) = My.Resources.BIOMASS_ACCUM_RATE
        AInitMortCoef(6, 0) = My.Resources.NET_MIGRATION_RATE
        AInitMortCoef(7, 0) = My.Resources.OTHER_MORT_RATE
        AInitMortCoef(8, 0) = My.Resources.FISH_MORT_TOTAL_MORT
        AInitMortCoef(9, 0) = My.Resources.PROP_NAT_MORT

        For Row = 1 To Core.nLivingGroups
            AInitMortCoef(0, Row) = Core.EcoPathGroupOutputs(Row).Index
            AInitMortCoef(1, Row) = Core.EcoPathGroupOutputs(Row).Name
            AInitMortCoef(2, Row) = Core.EcoPathGroupOutputs(Row).PBOutput
            AInitMortCoef(3, Row) = Core.EcoPathGroupOutputs(Row).MortCoFishRate
            AInitMortCoef(4, Row) = Core.EcoPathGroupOutputs(Row).MortCoPredMort
            AInitMortCoef(5, Row) = Core.EcoPathGroupOutputs(Row).BioAccumRatePerYear
            AInitMortCoef(6, Row) = Core.EcoPathGroupOutputs(Row).MortCoNetMig
            AInitMortCoef(7, Row) = Core.EcoPathGroupOutputs(Row).MortCoOtherMort
            AInitMortCoef(8, Row) = Core.EcoPathGroupOutputs(Row).FishMortPerTotMort
            AInitMortCoef(9, Row) = Core.EcoPathGroupOutputs(Row).NatMortPerTotMort
        Next

        'setup datasheet and send to data outputter
        InitMortCoef.Name = My.Resources.INIT_MORT_COEFFS
        InitMortCoef.Data = AInitMortCoef
        DataOutputter.AddIndicators(InitMortCoef)

    End Sub

    Private Sub CreateInitPredMortCSV()

        Dim ColPoint As Integer
        Dim Pred As cCoreGroupBase
        Dim PredIndex(Core.nGroups) As Integer
        Dim AInitPredMort(Core.nGroups, Core.nLivingGroups) As Object
        Dim InitPredMort As New cDataSheet

        'Write column headings
        AInitPredMort(1, 0) = My.Resources.PREY & "\" & My.Resources.PREDATOR
        ColPoint = 3
        For x = 1 To Core.nGroups
            Pred = Core.EcoSimGroupOutputs(x)
            If Pred.PP < 1 Then
                AInitPredMort(ColPoint - 1, 0) = x
                PredIndex(ColPoint - 3) = x
                ColPoint += 1
            End If
        Next

        'Write row titles
        For y = 1 To Core.nLivingGroups
            AInitPredMort(0, y) = Core.EcoSimGroupOutputs(y).Index
            AInitPredMort(1, y) = Core.EcoSimGroupOutputs(y).Name
        Next

        'Fill out consumption values
        For x = 3 To ColPoint - 1
            For y = 1 To Core.nLivingGroups
                AInitPredMort(x - 1, y) = Core.EcoPathGroupOutputs(y).PredMort(PredIndex(x - 3))
            Next
        Next

        'setup datasheet and send to data outputter
        InitPredMort.Name = My.Resources.INITPREDMORT
        InitPredMort.Data = AInitPredMort
        DataOutputter.AddIndicators(InitPredMort)

    End Sub

    Private Sub CreateInitFishingMortCSV()
        Dim slandings As Single
        Dim sDiscards As Single
        Dim sBiomass As Single
        Dim AInitFishingMort(1 + Core.nFleets, Core.nLivingGroups) As Object
        Dim InitFishingMort As New cDataSheet

        'Fill column titles row
        AInitFishingMort(1, 0) = My.Resources.GROUP & "\" & My.Resources.FLEET
        For x = 1 To Core.nFleets
            AInitFishingMort(1 + x, 0) = Core.EcopathFleetInputs(x).Name
        Next

        'Fill main data
        For y = 1 To Core.nLivingGroups
            AInitFishingMort(0, y) = Core.EcoPathGroupOutputs(y).Index
            AInitFishingMort(1, y) = Core.EcoPathGroupOutputs(y).Name
            For x = 1 To Core.nFleets
                slandings = Core.EcopathFleetInputs(x).Landings(y)
                sDiscards = Core.EcopathFleetInputs(x).Discards(y)
                sBiomass = Core.EcoPathGroupOutputs(y).Biomass
                If sBiomass > 0 Then
                    AInitFishingMort(1 + x, y) = (slandings + sDiscards) / sBiomass
                Else
                    AInitFishingMort(1 + x, y) = 0
                End If
            Next
        Next

        'setup datasheet and send to dataouputter
        InitFishingMort.Name = My.Resources.INITFISHMORT
        InitFishingMort.Data = AInitFishingMort
        DataOutputter.AddIndicators(InitFishingMort)

    End Sub

    Private Sub CreateInitConsumptionCSV()

        Dim ColPoint As Integer
        Dim Pred As cCoreGroupBase
        Dim TotalConsumption As Single
        Dim PredIndex(Core.nGroups) As Integer
        Dim AInitCons(Core.nGroups, Core.nGroups + 2) As Object
        Dim InitCons As New cDataSheet

        'Write column headings
        AInitCons(1, 0) = My.Resources.PREY & "\" & My.Resources.PREDATOR
        ColPoint = 3
        For x = 1 To Core.nGroups
            Pred = Core.EcoSimGroupOutputs(x)
            If Pred.PP < 1 Or Pred.PP = 2 Then
                AInitCons(ColPoint - 1, 0) = x
                PredIndex(ColPoint - 3) = x
                ColPoint += 1
            End If
        Next

        'Write row headings
        For y = 1 To Core.nGroups
            AInitCons(0, y) = Core.EcoSimGroupOutputs(y).Index
            AInitCons(1, y) = Core.EcoSimGroupOutputs(y).Name
        Next
        'Add Import row
        AInitCons(0, Core.nGroups + 1) = Core.nGroups + 1
        AInitCons(1, Core.nGroups + 1) = My.Resources.IMPORT
        'Add Sum row
        AInitCons(0, Core.nGroups + 2) = Core.nGroups + 2
        AInitCons(1, Core.nGroups + 2) = My.Resources.SUM

        'Fill out consumption values
        For x = 3 To ColPoint - 1
            TotalConsumption = 0
            For y = 1 To Core.nGroups
                AInitCons(x - 1, y) = Core.EcoPathGroupOutputs(y).Consumption(PredIndex(x - 3))
                TotalConsumption += Core.EcoPathGroupOutputs(y).Consumption(PredIndex(x - 3))
            Next
            AInitCons(x - 1, Core.nGroups + 1) = Core.EcoPathGroupOutputs(PredIndex(x - 3)).ImportedConsumption
            TotalConsumption += Core.EcoPathGroupOutputs(PredIndex(x - 3)).ImportedConsumption
            AInitCons(x - 1, Core.nGroups + 2) = TotalConsumption
        Next

        'Setup datasheet and send to dataoutputter
        InitCons.Name = My.Resources.INITCONSUMPTION
        InitCons.Data = AInitCons
        DataOutputter.AddIndicators(InitCons)

    End Sub

    Private Sub CreateRespirationCSV()
        Dim ARespiration(6, Core.nGroups) As Object
        Dim Respiration As New cDataSheet

        'Set up titles
        ARespiration(1, 0) = My.Resources.GROUP_NAME
        ARespiration(2, 0) = My.Resources.RESPIRATION_UNITS
        ARespiration(3, 0) = My.Resources.ASSIMILATION_UNITS
        ARespiration(4, 0) = My.Resources.RESPIRATION_ASSIMILATION
        ARespiration(5, 0) = My.Resources.PRODUCTION_RESPIRATION
        ARespiration(6, 0) = My.Resources.RESPIRATION_BIOMASS_UNITS

        For Row = 1 To Core.nGroups
            ARespiration(0, Row) = Core.EcoPathGroupOutputs(Row).Index
            ARespiration(1, Row) = Core.EcoPathGroupOutputs(Row).Name
            ARespiration(2, Row) = Core.EcoPathGroupOutputs(Row).Respiration
            ARespiration(3, Row) = Core.EcoPathGroupOutputs(Row).Assimilation
            ARespiration(4, Row) = Core.EcoPathGroupOutputs(Row).RespAssim
            ARespiration(5, Row) = Core.EcoPathGroupOutputs(Row).ProdResp
            ARespiration(6, Row) = Core.EcoPathGroupOutputs(Row).RespBiom
        Next

        'Setup datasheet and send to data outputter
        Respiration.Name = My.Resources.RESPIRATION
        Respiration.Data = ARespiration
        DataOutputter.AddIndicators(Respiration)

    End Sub

    Private Sub CreateOverlapPreyCSV()
        Dim AOverlapPrey(Core.nLivingGroups + 1, Core.nLivingGroups) As Object
        Dim OverlapPrey As New cDataSheet

        AOverlapPrey(1, 0) = My.Resources.GROUP_NAME
        For x = 1 To Core.nLivingGroups
            AOverlapPrey(1 + x, 0) = x
        Next

        'Write body of data
        For Row = 1 To Core.nLivingGroups
            AOverlapPrey(0, Row) = Core.EcoPathGroupOutputs(Row).Index
            AOverlapPrey(1, Row) = Core.EcoPathGroupOutputs(Row).Name
            For Col = 1 To Row
                AOverlapPrey(1 + Col, Row) = Core.EcoPathGroupOutputs(Row).Plap(Col)
            Next
        Next

        'Setup datasheet and send to data outputter
        OverlapPrey.Name = My.Resources.OVERLAPPREY
        OverlapPrey.Data = AOverlapPrey
        DataOutputter.AddIndicators(OverlapPrey)

    End Sub

    Private Sub CreateOverlapPredCSV()

        Dim AOverlapPred(Core.nLivingGroups + 1, Core.nLivingGroups) As Object
        Dim OverlapPred As New cDataSheet

        'Write column headings
        AOverlapPred(1, 0) = My.Resources.GROUP_NAME
        For x = 1 To Core.nLivingGroups
            AOverlapPred(1 + x, 0) = CStr(x)
        Next

        'Write body of data
        For Row = 1 To Core.nLivingGroups
            AOverlapPred(0, Row) = Core.EcoPathGroupOutputs(Row).Index
            AOverlapPred(1, Row) = Core.EcoPathGroupOutputs(Row).Name
            For Col = 1 To Row
                AOverlapPred(1 + Col, Row) = Core.EcoPathGroupOutputs(Row).Hlap(Col)
            Next
        Next

        'Setup datasheet and send to data outputter
        OverlapPred.Name = My.Resources.OVERLAPPRED
        OverlapPred.Data = AOverlapPred
        DataOutputter.AddIndicators(OverlapPred)

    End Sub

    Private Sub CreateElectivityCSV()
        Dim AElectivity(Core.nGroups + 1, Core.nGroups) As Object
        Dim Electivity As New cDataSheet
        Dim ColPoint As Integer

        'Write column headings
        AElectivity(1, 0) = My.Resources.PREY & "\" & My.Resources.PREDATOR
        ColPoint = 1
        For x = 1 To Core.nGroups
            If Core.EcoPathGroupOutputs(x).PP < 1 Then
                AElectivity(1 + ColPoint, 0) = Core.EcoPathGroupOutputs(x).Index
                ColPoint += 1
            End If
        Next

        'Write body of data
        For Row = 1 To Core.nGroups
            AElectivity(0, Row) = Core.EcoPathGroupOutputs(Row).Index
            AElectivity(1, Row) = Core.EcoPathGroupOutputs(Row).Name

            For Col = 1 To Core.nGroups
                If Core.EcoPathGroupOutputs(Col).PP < 1 Then
                    AElectivity(1 + Col, Row) = Core.EcoPathGroupOutputs(Col).Alpha(Row)
                End If
            Next
        Next

        'Setup datasheet and send to data outputter
        Electivity.Name = My.Resources.ELECTIVITY
        Electivity.Data = AElectivity
        DataOutputter.AddIndicators(Electivity)

    End Sub

    Private Sub CreateInitFishingQuantitiesCSV()

        Dim TotalCatchGroup As Single
        Dim TotalCatchFleet(Core.nFleets - 1) As Single
        Dim TotalTotalCatch As Single = 0
        Dim TTCatch As Single = 0
        Dim RowVals(Core.nFleets - 1) As Single
        Dim RowPoint As Integer = 1
        Dim sourceGrpIntput As cCoreInputOutputBase = Nothing
        Dim sourceGrpIntputSec As cCoreInputOutputBase = Nothing
        Dim sourceGrpOutput As cCoreInputOutputBase = Nothing
        Dim propLandings As Single
        Dim propDiscards As Single
        Dim Quantities As Single
        Dim propTTLX As Single
        Dim QuantitiesTTLX As Single
        Dim FleetQuantities As Single
        Dim FleetQuantitiesTTLX As Single
        Dim AllQuantities As Single = 0
        Dim AllQuantitiesTTLX As Single = 0

        Dim AInitFishQuant(2 + Core.nFleets, Core.nGroups + 1) As Object
        Dim InitFishQuant As New cDataSheet

        'Write column headings
        AInitFishQuant(1, 0) = My.Resources.GROUP_NAME
        For x = 1 To Core.nFleets
            AInitFishQuant(1 + x, 0) = Core.EcopathFleetInputs(x).Name
        Next
        AInitFishQuant(2 + Core.nFleets, 0) = My.Resources.TOTAL_CATCH

        'Write body of data
        For xGroup = 1 To Core.nGroups
            TotalCatchGroup = 0
            For Col = 1 To Core.nFleets
                RowVals(Col - 1) = Core.EcopathFleetInputs(Col).Landings(xGroup) + Core.EcopathFleetInputs(Col).Discards(xGroup)
                TotalCatchGroup += Core.EcopathFleetInputs(Col).Landings(xGroup) + Core.EcopathFleetInputs(Col).Discards(xGroup)
                TotalCatchFleet(Col - 1) += Core.EcopathFleetInputs(Col).Landings(xGroup) + Core.EcopathFleetInputs(Col).Discards(xGroup)
            Next
            If TotalCatchGroup > 0 Then
                AInitFishQuant(0, RowPoint) = Core.EcoPathGroupOutputs(xGroup).Index
                AInitFishQuant(1, RowPoint) = Core.EcoPathGroupOutputs(xGroup).Name
                For Col = 0 To Core.nFleets - 1
                    AInitFishQuant(2 + Col, RowPoint) = RowVals(Col)
                Next
                AInitFishQuant(2 + Core.nFleets, RowPoint) = TotalCatchGroup
                RowPoint += 1
            End If

        Next

        'Write the total line on the bottom
        AInitFishQuant(1, RowPoint) = My.Resources.TOTAL_CATCH
        For Col = 0 To Core.nFleets - 1
            AInitFishQuant(2 + Col, RowPoint) = TotalCatchFleet(Col)
            TTCatch += TotalCatchFleet(Col)
        Next
        AInitFishQuant(2 + Core.nFleets, RowPoint) = TTCatch
        RowPoint += 1

        'Write the trophic level line at bottom
        AInitFishQuant(1, RowPoint) = My.Resources.TROPHIC_LEVEL

        For fleetIndex As Integer = 1 To Core.nFleets

            FleetQuantities = 0
            FleetQuantitiesTTLX = 0

            For GrpIndex As Integer = 1 To Core.nGroups

                'Reset for each row
                Quantities = 0
                QuantitiesTTLX = 0

                'Calculate Quantity for each group
                propLandings = Core.EcopathFleetInputs(fleetIndex).Landings(GrpIndex)
                propDiscards = Core.EcopathFleetInputs(fleetIndex).Discards(GrpIndex)
                Quantities = (propLandings + propDiscards)

                'Get trophic level of group and multiply by quanity
                propTTLX = Core.EcoPathGroupOutputs(GrpIndex).TTLX
                QuantitiesTTLX = Quantities * propTTLX

                'Keep running total of quanities and quantities*TTLX for each column
                FleetQuantities += Quantities
                FleetQuantitiesTTLX += QuantitiesTTLX

            Next

            AInitFishQuant(1 + fleetIndex, RowPoint) = FleetQuantitiesTTLX / FleetQuantities
            AllQuantities += FleetQuantities
            AllQuantitiesTTLX += FleetQuantitiesTTLX

        Next

        AInitFishQuant(2 + Core.nFleets, RowPoint) = AllQuantitiesTTLX / AllQuantities

        'Setup data sheet and send to data outputter
        InitFishQuant.Name = My.Resources.INITFISHQUANTS
        InitFishQuant.Data = AInitFishQuant
        DataOutputter.AddIndicators(InitFishQuant)


    End Sub

    Private Sub CreateInitFishingValuesCSV()
        Dim y As Integer = 0
        Dim AInitFishVals(4 + Core.nFleets, Core.nGroups + 3) As Object
        Dim InitFishVals As New cDataSheet

        Dim ValueFleetGroup As Single
        Dim SumFixedCPUESailCost As Single

        Dim MarketValueSum As Single
        Dim NonMarketValueSum As Single
        Dim TotalValueSum As Single

        Dim TotalValueFleet(Core.nFleets) As Single
        Dim TotalCostFleet(Core.nFleets) As Single
        Dim TotalProfitFleet As Single

        'Write column headings for fleets
        AInitFishVals(1, y) = My.Resources.GROUP_NAME
        For x = 1 To Core.nFleets
            AInitFishVals(1 + x, y) = Core.EcopathFleetInputs(x).Name
        Next

        AInitFishVals(2 + Core.nFleets, y) = My.Resources.CATCH_VALUE
        AInitFishVals(3 + Core.nFleets, y) = My.Resources.NONMARKET_VALUE & "(" & Core.EwEModel.UnitMonetary.ToString & ")"
        AInitFishVals(4 + Core.nFleets, y) = My.Resources.TOTAL_VALUE & "(" & Core.EwEModel.UnitMonetary.ToString & ")"

        'Write body of data
        For Row = 1 To Core.nGroups
            y += 1

            'Write Group Name
            AInitFishVals(0, y) = Core.EcoPathGroupOutputs(Row).Index
            AInitFishVals(1, y) = Core.EcoPathGroupOutputs(Row).Name

            'Reset totals(last 3 columns) to zero for start of each row
            MarketValueSum = 0
            NonMarketValueSum = 0
            TotalValueSum = 0

            For Col = 1 To Core.nFleets
                ValueFleetGroup = Core.EcopathFleetInputs(Col).Landings(Row) * Core.EcopathFleetInputs(Col).OffVesselValue(Row)
                AInitFishVals(1 + Col, y) = ValueFleetGroup
                MarketValueSum += ValueFleetGroup
                TotalValueFleet(Col) += ValueFleetGroup
            Next

            'Calculate the sum for all fleets of the Non-market value
            NonMarketValueSum = Core.EcoPathGroupInputs(Row).NonMarketValue * _
                Core.EcoPathGroupOutputs(Core.EcoPathGroupInputs(Row).Index).Biomass
            'Calculate the value total value for all fleets
            TotalValueSum = MarketValueSum + NonMarketValueSum

            'Fill last three columns of row
            AInitFishVals(2 + Core.nFleets, y) = MarketValueSum
            AInitFishVals(3 + Core.nFleets, y) = NonMarketValueSum
            AInitFishVals(4 + Core.nFleets, y) = TotalValueSum

        Next

        y += 1

        'Output total value for each fleet
        AInitFishVals(1, y) = My.Resources.TOTAL_VALUE & "(" & Core.EwEModel.UnitMonetary.ToString & ")"
        MarketValueSum = 0
        For col = 1 To Core.nFleets
            AInitFishVals(1 + col, y) = TotalValueFleet(col)
            MarketValueSum += TotalValueFleet(col)
        Next
        AInitFishVals(2 + Core.nFleets, y) = MarketValueSum

        y += 1

        'Output total cost for each fleet
        AInitFishVals(1, y) = My.Resources.TOTAL_COST & "(" & Core.EwEModel.UnitMonetary.ToString & ")"
        MarketValueSum = 0
        For Col = 1 To Core.nFleets
            SumFixedCPUESailCost = Core.EcopathFleetInputs(Col).FixedCost + _
                                    Core.EcopathFleetInputs(Col).CPUECost + _
                                    Core.EcopathFleetInputs(Col).SailCost
            TotalCostFleet(Col) = SumFixedCPUESailCost * TotalValueFleet(Col) * CSng(0.01)
            MarketValueSum += TotalCostFleet(Col)
            AInitFishVals(1 + Col, y) = TotalCostFleet(Col)

        Next
        AInitFishVals(2 + Core.nFleets, y) = MarketValueSum

        y += 1

        'Output profit row
        AInitFishVals(1, y) = My.Resources.TOTAL_PROFIT & "(" & Core.EwEModel.UnitMonetary.ToString & ")"
        MarketValueSum = 0
        For Col = 1 To Core.nFleets
            TotalProfitFleet = TotalValueFleet(Col) - TotalCostFleet(Col)
            MarketValueSum += TotalProfitFleet
            AInitFishVals(1 + Col, y) = TotalProfitFleet
        Next
        AInitFishVals(2 + Core.nFleets, y) = MarketValueSum

        'Setup datasheet and send to data outputter
        InitFishVals.Name = My.Resources.INITFISHVALUES
        InitFishVals.Data = AInitFishVals
        DataOutputter.AddIndicators(InitFishVals)

    End Sub

    Private Sub CreateSearchRatesCSV()
        Dim ASearchRates(1 + Core.nGroups, Core.nGroups) As Object
        Dim SearchRates As New cDataSheet
        Dim ColPointer As Integer = 1

        'Write column headings
        ASearchRates(1, 0) = My.Resources.PREY & "\" & My.Resources.PREDATOR
        For x = 1 To Core.nGroups
            If Core.EcoPathGroupOutputs(x).PP < 1 Then
                ASearchRates(ColPointer + 1, 0) = Core.EcoPathGroupOutputs(x).Index
                ColPointer += 1
            End If
        Next

        'Write body of data
        For Row = 1 To Core.nGroups
            ColPointer = 1
            ASearchRates(0, Row) = Core.EcoPathGroupOutputs(Row).Index
            ASearchRates(1, Row) = Core.EcoPathGroupOutputs(Row).Name
            For x = 1 To Core.nGroups
                If Core.EcoPathGroupOutputs(x).PP < 1 Then
                    ASearchRates(1 + ColPointer, Row) = Core.EcoPathGroupOutputs(Row).SearchRate(x)
                    ColPointer += 1
                End If
            Next
        Next

        'Setup data sheet and send to data outputter
        SearchRates.Name = My.Resources.SEARCHRATES
        SearchRates.Data = ASearchRates
        DataOutputter.AddIndicators(SearchRates)

    End Sub

    Private Sub CreateResiduals()
        Dim Residuals As New cDataSheet
        Dim AResiduals(,) As Object
        Dim YDim As Integer
        Dim XDim As Integer

        YDim = UBound(mLogDiff, 1) 'number of time series
        XDim = UBound(mLogDiff, 2) 'number of years

        ReDim AResiduals(XDim + 1, YDim)

        'Setup headings for each row
        For y = 1 To YDim
            AResiduals(0, y) = mTimeSeries.strName(y) & "(" & My.Resources.TYPE & mTimeSeries.DatType(y) & ")"
        Next
        AResiduals(XDim, 0) = My.Resources.SS

        'Create array for output with all fitting stats in
        For x = 1 To XDim
            AResiduals(x, 0) = x
            For y = 1 To YDim
                AResiduals(x, y) = mLogDiff(y, x)
            Next
        Next

        'Setup data sheet and send to data outputter
        Residuals.Name = My.Resources.RESIDUALS
        Residuals.Data = AResiduals
        DataOutputter.AddDiagnostics(Residuals)


    End Sub

    Private Sub CreateSS()
        Dim SS As New cDataSheet
        Dim ASS(1, mTimeSeries.NdatType + 1) As Object
        Dim rowindex As Integer = 0

        ASS(0, 0) = My.Resources.TOTALSS
        ASS(1, 0) = mDataStructure.SS

        For idat = 1 To mTimeSeries.nTimeSeries
            If mTimeSeries.bEnable(idat) Then
                rowindex += 1
                ASS(0, rowindex) = mTimeSeries.strName(idat)
                ASS(1, rowindex) = mTimeSeries.DatSS(rowindex)
            End If

        Next

        'Setup data sheet and send to data outputter
        SS.Name = My.Resources.SS
        SS.Data = ASS
        DataOutputter.AddDiagnostics(SS)

    End Sub

    Private Sub SetSaveResultsState()

        btnSaveResults.Enabled = False

        If ParentOnlySelection.CountSelected > 0 Then

            If chkBiomass.Checked Or chkBiomassInteg.Checked Or _
            chkPredationMortality.Checked Or chkFishingMortality.Checked Or _
            chkCatch.Checked Then
                btnSaveResults.Enabled = True
            End If

        ElseIf PredatorPreySelection.CountSelectedChild > 0 Then

            If chkConsumption.Checked Or chkDietProportions.Checked Then
                btnSaveResults.Enabled = True
            End If

        ElseIf PreyPredatorSelection.CountSelectedChild > 0 Then

            If chkPredationPerPredator.Checked Then
                btnSaveResults.Enabled = True
            End If

        ElseIf FleetPreySelection.CountSelectedChild > 0 Then

            If chkFishMortFleetToPrey.Checked Or chkCatchFleet.Checked Then
                btnSaveResults.Enabled = True
            End If

        ElseIf FleetOnlySelection.CountSelected > 0 Then

            If chkFleetValue.Checked Or chkEffort.Checked Then
                btnSaveResults.Enabled = True
            End If

        ElseIf chkBasicEstimates.Checked Or chkKeyIndices.Checked Or _
        chkMortalityCoefficients.Checked Or chkInitPredMort.Checked Or chkInitFishMort.Checked Or _
        chkInitConsumption.Checked Or chkRespiration.Checked Or _
        chkPreyOverlap.Checked Or chkPredOverlap.Checked Or _
        chkElectivity.Checked Or chkSearchRates.Checked Or _
        chkInitFishingQuantities.Checked Or chkInitFishingValues.Checked Or chkresiduals.Checked Or _
        chkSS.Checked Then

            btnSaveResults.Enabled = True

        End If

    End Sub

    Private Sub ResetForm()

        'Set all checkboxes to unchecked
        Me.chkBiomass.Checked = False
        Me.chkBiomassInteg.Checked = False
        Me.chkFishingMortality.Checked = False
        Me.chkPredationMortality.Checked = False
        Me.chkCatch.Checked = False
        Me.chkConsumption.Checked = False
        Me.chkDietProportions.Checked = False
        Me.chkPredationPerPredator.Checked = False
        Me.chkFishMortFleetToPrey.Checked = False
        Me.chkEffort.Checked = False
        Me.chkCatchFleet.Checked = False
        Me.chkFleetValue.Checked = False
        Me.chkBasicEstimates.Checked = False
        Me.chkKeyIndices.Checked = False
        Me.chkMortalityCoefficients.Checked = False
        Me.chkInitPredMort.Checked = False
        Me.chkInitFishMort.Checked = False
        Me.chkInitConsumption.Checked = False
        Me.chkRespiration.Checked = False
        Me.chkPreyOverlap.Checked = False
        Me.chkPredOverlap.Checked = False
        Me.chkElectivity.Checked = False
        Me.chkSearchRates.Checked = False
        Me.chkInitFishingQuantities.Checked = False
        Me.chkInitFishingValues.Checked = False
        Me.chkSS.Checked = False

    End Sub

    Public Sub ValidateObjectCreated()

        If ParentOnlySelection.SelectedNames.Count = 0 Then
            chkBiomass.Checked = False
            chkBiomassInteg.Checked = False
            chkFishingMortality.Checked = False
            chkPredationMortality.Checked = False
            chkCatch.Checked = False
            btnSetParentOnly.Enabled = False
        Else
            btnSetParentOnly.Enabled = True
        End If

        If PredatorPreySelection.CountSelectedChild = 0 Then
            chkConsumption.Checked = False
            chkDietProportions.Checked = False
            btnSetPredPrey.Enabled = False
        Else
            btnSetPredPrey.Enabled = True
        End If

        If PreyPredatorSelection.CountSelectedChild = 0 Then
            chkPredationPerPredator.Checked = False
            btnSetPreyPred.Enabled = False
        Else
            btnSetPreyPred.Enabled = True
        End If

        If FleetPreySelection.CountSelectedChild = 0 Then
            chkFishMortFleetToPrey.Checked = False
            chkCatchFleet.Checked = False
            btnSetFleetPrey.Enabled = False
        Else
            btnSetFleetPrey.Enabled = True
        End If

        If FleetOnlySelection.CountSelected = 0 Then
            chkFleetValue.Checked = False
            chkEffort.Checked = False
            btnSetFleetOnly.Enabled = False
        Else
            btnSetFleetOnly.Enabled = True
        End If

        SetSaveResultsState()

    End Sub

    Public Sub DeleteObjects()

        If chkBiomass.Checked = False And chkBiomassInteg.Checked = False And _
            chkFishingMortality.Checked = False And chkPredationMortality.Checked = False And _
            chkCatch.Checked = False Then
            ParentOnlySelection.RemoveAll()
            btnSetParentOnly.Enabled = False
        End If

        If chkFishMortFleetToPrey.Checked = False And chkCatchFleet.Checked = False Then
            FleetPreySelection.RemoveAll()
            btnSetFleetPrey.Enabled = False
        End If

        If chkConsumption.Checked = False And chkDietProportions.Checked = False Then
            PredatorPreySelection.RemoveAll()
            btnSetPredPrey.Enabled = False
        End If

        If chkPredationPerPredator.Checked = False Then
            PreyPredatorSelection.RemoveAll()
            btnSetPreyPred.Enabled = False
        End If

        If chkFleetValue.Checked = False Then
            FleetOnlySelection.RemoveAll()
            btnSetFleetOnly.Enabled = False
        End If

    End Sub

#Region "KeyRun"

    Private Sub PredatorPreyStage()

        'Check if previous selection performed correctly...
        If ParentOnlySelection.CountSelected = 0 Then
            If MsgBoxResult.Cancel = MsgBox(My.Resources.MSG_PREV_FORM_INVALID, MsgBoxStyle.RetryCancel, My.Resources.INVALID_SELECTION) Then
                FireChecked = True
                Exit Sub
            End If
            btnAllOptions.PerformClick()
            Exit Sub
        End If

        '...and if they have tick all the relevant checkboxes
        chkBiomass.Checked = True
        chkBiomassInteg.Checked = True
        chkFishingMortality.Checked = True
        chkPredationMortality.Checked = True
        chkCatch.Checked = True

        'set delegate to next stage and load next form
        NextAction = AddressOf Me.PreyPredStage
        Dim a As New frmSelectPredatorPrey(PredatorPreySelection, Core)
        a.Show()
        AddHandler a.FormExited, AddressOf ValidateObjectCreated

    End Sub

    Private Sub PreyPredStage()

        'Check if previous selection performed correctly...
        If PredatorPreySelection.CountSelectedChild = 0 Then
            If MsgBoxResult.Cancel = MsgBox(My.Resources.MSG_PREV_FORM_INVALID, MsgBoxStyle.RetryCancel, My.Resources.INVALID_SELECTION) Then
                FireChecked = True
                Exit Sub
            End If
            PredatorPreyStage()
            Exit Sub
        End If

        '...and if they have tick all the relevant checkboxes
        chkDietProportions.Checked = True
        chkConsumption.Checked = True

        'set delegate to next stage and load next form
        NextAction = AddressOf Me.FleetPreyStage
        Dim a As New frmSelectPreyPredator(PreyPredatorSelection, Core)
        a.Show()
        AddHandler a.FormExited, AddressOf ValidateObjectCreated


    End Sub

    Private Sub FleetPreyStage()

        'Check if previous selection performed correctly...
        If PreyPredatorSelection.CountSelectedChild = 0 Then
            If MsgBoxResult.Cancel = MsgBox(My.Resources.MSG_PREV_FORM_INVALID, MsgBoxStyle.RetryCancel, My.Resources.INVALID_SELECTION) Then
                FireChecked = True
                Exit Sub
            End If
            PreyPredStage()
            Exit Sub
        End If

        '...and if they have tick all the relevant checkboxes
        chkPredationPerPredator.Checked = True

        'set delegate to next stage and load next form
        NextAction = AddressOf Me.FleetOnlyStage
        Dim a As New frmSelectFleetPrey(FleetPreySelection, Core)
        a.Show()
        AddHandler a.FormExited, AddressOf ValidateObjectCreated


    End Sub

    Private Sub FleetOnlyStage()

        'Check if previous selection performed correctly...
        If FleetPreySelection.CountSelectedChild = 0 Then
            If MsgBoxResult.Cancel = MsgBox(My.Resources.MSG_PREV_FORM_INVALID, MsgBoxStyle.RetryCancel, My.Resources.INVALID_SELECTION) Then
                FireChecked = True
                Exit Sub
            End If
            FleetPreyStage()
            Exit Sub
        End If

        '...and if they have tick all the relevant checkboxes
        chkFishMortFleetToPrey.Checked = True
        chkCatchFleet.Checked = True

        'set delegate to next stage and load next form
        NextAction = AddressOf Me.EcoPathValuesStage
        Dim a As New frmSelectFleetOnly(FleetOnlySelection, Core)
        a.Show()
        AddHandler a.FormExited, AddressOf ValidateObjectCreated

    End Sub

    Private Sub EcoPathValuesStage()

        'Check if previous selection performed correctly...
        If FleetOnlySelection.CountSelected = 0 Then
            If MsgBoxResult.Cancel = MsgBox(My.Resources.MSG_PREV_FORM_INVALID, MsgBoxStyle.RetryCancel, My.Resources.INVALID_SELECTION) Then
                FireChecked = True
                Exit Sub
            End If
            FleetOnlyStage()
            Exit Sub
        End If

        '...and if they have tick all the relevant checkboxes
        chkFleetValue.Checked = True
        chkEffort.Checked = True

        chkBasicEstimates.Checked = True
        chkKeyIndices.Checked = True
        chkMortalityCoefficients.Checked = True
        chkInitPredMort.Checked = True
        chkInitConsumption.Checked = True
        chkRespiration.Checked = True
        chkPreyOverlap.Checked = True
        chkPredOverlap.Checked = True
        chkElectivity.Checked = True
        chkSearchRates.Checked = True
        chkInitFishingQuantities.Checked = True
        chkInitFishingValues.Checked = True
        chkInitFishMort.Checked = True
        chkresiduals.Checked = True
        chkSS.Checked = True
        FireChecked = True

    End Sub

#End Region ' Subs that are executed in sequence when key-run is clicked

#End Region

End Class


'Public Sub SendToFileTabbed(ByVal data(,) As Single, ByVal GroupNames As List(Of cCreatedObjects), _
'                  ByVal TabName As String, ByVal FileName As String, _
'                  ByVal sheet As Excel.Worksheet, ByVal wb As Excel.Workbook)

'    sheet.Name = CheckName(TabName, wb)

'    Dim simYears As Integer = CInt(m_core.nEcosimTimeSteps / cCore.N_MONTHS)
'    Dim nGroups As Integer = data.GetLength(0) - 1
'    Dim sum(nGroups) As Single

'    Dim x As Integer = 1, y As Integer = 1 'Hold coordinates of cell underfocus

'    'Create Super Headings
'    For Each SuperGroup In GroupNames
'        sheet.Cells(y, x) = SuperGroup.ParentName
'        x += SuperGroup.CountChild
'    Next

'    'Move down start of next line
'    x = 1
'    y += 1
'    For Each SuperGroup In GroupNames
'        For Each SubGroup In SuperGroup.ChildNames
'            sheet.Cells(y, x) = SubGroup
'            x += 1
'        Next
'    Next

'    For j As Integer = 0 To data.GetLength(1) - 1
'        For i As Integer = 0 To nGroups
'            sheet.Cells(j + y + 1, i + 1) = data(i, j)
'        Next
'    Next


'End Sub

'Public Sub SendToFileTabbed(ByVal data(,) As Single, ByVal strGroupNames As List(Of String), _
'                      ByVal TabName As String, ByVal FileName As String, _
'                      ByVal Sheet As Excel.Worksheet, ByVal wb As Excel.Workbook)

'    Sheet.Name = CheckName(TabName, wb)

'    Dim nGroups As Integer = data.GetLength(0) - 1

'    For i = 0 To strGroupNames.Count - 1
'        Sheet.Cells(1, i + 1) = strGroupNames(i)
'    Next

'    For j As Integer = 0 To data.GetLength(1) - 1
'        For i As Integer = 0 To nGroups
'            Sheet.Cells(j + 2, i + 1) = data(i, j)
'        Next
'    Next

'End Sub

'Dim SSgrp As Single
'Dim SS As Single
'Dim shape As cForcingFunction = Nothing
'Dim FuncType As eForcingFunctionApplication
'Dim ppi As cPredPreyInteraction = Nothing
'Dim FishMorts() As cForcingFunction


''switch off all production terms ------------------------------------------------------------------------------------------------------------
'For FunctGrp = 1 To m_core.nGroups
'    If m_core.EcoPathGroupInputs(FunctGrp).IsProducer Then

'        'Get the object that deals with the predator prey interactions for each primary producer
'        ppi = m_core.PPInteractionManager.Interaction(FunctGrp, FunctGrp)

'        'Lock updates while changing shape settings
'        ppi.LockUpdates = True

'        For i As Integer = 1 To ppi.MaxNumShapes
'            If ppi.getShape(i, shape, FuncType) Then
'                If FuncType = eForcingFunctionApplication.ProductionRate Then
'                    Exit For
'                End If
'            End If
'        Next

'        For i As Integer = 1 To ppi.MaxNumShapes
'            ppi.setShape(i, Nothing)
'        Next

'        'Unlock to allow for updates now changes have been made
'        ppi.LockUpdates = False

'    End If
'Next

''Reset vulnerabilities ---------------------------------------------------------------------------------------------------------------------------
'Dim VulnerabilityForcers(m_core.nGroups, m_core.nGroups, 5) As cForcingFunction

'For iPred As Integer = 1 To m_core.nGroups
'    For iPrey As Integer = 1 To m_core.nGroups
'        ppi = m_core.PPInteractionManager.Interaction(iPred, iPrey)
'        If ppi IsNot Nothing Then
'            ppi.LockUpdates = True
'            For i As Integer = 1 To ppi.MaxNumShapes
'                If (ppi.getShape(i, VulnerabilityForcers(iPred, iPrey, i), FuncType)) Then
'                    If FuncType = eForcingFunctionApplication.Vulnerability Then
'                        ppi.setShape(i, Nothing)
'                    End If
'                End If
'            Next
'            ppi.LockUpdates = False
'        End If
'    Next
'Next

'mEcosimModel.Run()
'SSgrp = mDataStructure.SSGroup(1)
'SS = mDataStructure.SS


''Switch off all the time series for fishing mortalities ------------------------------------------------------------------------------------------
'Dim nFishShapes As Integer = m_core.FishMortShapeManager.Count
'ReDim FishMorts(nFishShapes)

''For iShape As Integer = 1 To nFishShapes
''    FishMorts(iShape) = m_core.FishMortShapeManager.Item(iShape - 1)
''    m_core.FishMortShapeManager.Remove(FishMorts(iShape))
''Next
'For iTimeSeries As Integer = 1 To mTimeSeries.NdatType
'    If mTimeSeries.DatType(iTimeSeries) = eTimeSeriesType.FishingMortality Then
'        mTimeSeries.bEnable(iTimeSeries) = False
'    End If
'Next

'mEcosimModel.Run()
'SSgrp = mDataStructure.SSGroup(1)
'SS = mDataStructure.SS



'Dim VulnerabilityVals(m_core.nGroups, m_core.nGroups) As Single

'For iPred As Integer = 1 To m_core.nGroups
'    For iPrey As Integer = 1 To m_core.nGroups
'        VulnerabilityVals(iPred, iPrey) = m_core.EcoSimGroupInputs(iPred).VulMult(iPrey)
'        m_core.EcoSimGroupInputs(iPred).VulMult(iPrey) = 2
'    Next
'Next


'mEcosimModel.Run()
'SSgrp = mDataStructure.SSGroup(1)
'SS = mDataStructure.SS

''Set appropriate shape
'ppi.LockUpdates = True
'ppi.setShape(1, shape, eForcingFunctionApplication.ProductionRate)
'ppi.LockUpdates = False

'mEcosimModel.Run()

'SSgrp = mDataStructure.SSGroup(1)
'SS = mDataStructure.SS