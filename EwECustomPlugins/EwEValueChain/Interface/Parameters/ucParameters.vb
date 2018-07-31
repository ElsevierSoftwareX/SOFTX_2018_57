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
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports EwEUtils.Database.cEwEDatabase

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Plug-in credits/parameters/info form
''' </summary>
''' ===========================================================================
Public Class ucParameters
    Implements IDisposable

    ''' <summary>The core currently linked to.</summary>
    Private m_uic As cUIContext = Nothing
    ''' <summary>The value chain parameters that this page operates on.</summary>
    Private m_params As cParameters = Nothing
    ''' <summary>Smartibits</summary>
    Private m_fpBaseYear As cEwEFormatProvider = Nothing
    Private m_fpFMin As cEwEFormatProvider = Nothing
    Private m_fpFMax As cEwEFormatProvider = Nothing
    Private m_fpIncr As cEwEFormatProvider = Nothing
    ''' <summary>Core event monitor</summary>
    Private m_mhCore As cMessageHandler = Nothing

    Private m_bInUpdate As Boolean = False

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="data">The data to paramterize.</param>
    ''' <param name="uic">UI context of EwE GUI.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal data As cData, ByVal uic As cUIContext)

        Me.InitializeComponent()

        Me.m_params = data.Parameters
        Me.m_uic = uic

        ' Start listening for core messages
        Me.m_mhCore = New cMessageHandler(AddressOf CoreMessageHandler, eCoreComponentType.Core, eMessageType.GlobalSettingsChanged, Me.m_uic.SyncObject)
#If DEBUG Then
        Me.m_mhCore.Name = "ValueChain.ucParameters.Core"
#End If
        Me.m_uic.Core.Messages.AddMessageHandler(Me.m_mhCore)
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then

                ' Stop listening for core messages
                Me.m_uic.Core.Messages.RemoveMessageHandler(Me.m_mhCore)
                Me.m_mhCore.Dispose()

                Me.ConfigureEcosimControls(False)

                If (Me.m_fpFMin IsNot Nothing) Then
                    Me.m_fpFMin.Release()
                    Me.m_fpFMin = Nothing
                End If

                If (Me.m_fpFMax IsNot Nothing) Then
                    Me.m_fpFMax.Release()
                    Me.m_fpFMax = Nothing
                End If

                If (Me.m_fpIncr IsNot Nothing) Then
                    Me.m_fpIncr.Release()
                    Me.m_fpIncr = Nothing
                End If

                If (Me.m_uic IsNot Nothing) Then

                    ' Stop listening to core state changes
                    RemoveHandler Me.m_uic.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreStateChanged
                    ' Stop listening to parameter changes
                    RemoveHandler Me.m_params.OnChanged, AddressOf OnParametersChanged

                    Dim cmd As cCommand = Me.m_uic.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME)
                    cmd.RemoveControl(Me.m_pbLenfest)
                    cmd.RemoveControl(Me.m_pbSAUP)
                    cmd.RemoveControl(Me.m_pbEcostProject)

                    ' Unplug Ecosim controls
                    Me.ConfigureEcosimControls(False)
                    Me.m_uic = Nothing

                End If

                If components IsNot Nothing Then
                    components.Dispose()
                End If
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region ' Constructor

#Region " Overrides "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load me!
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Debug.Assert(Me.m_uic IsNot Nothing)

        Me.m_fpFMin = New cEwEFormatProvider(Me.m_uic, Me.m_nudEffortMin, GetType(Single))
        Me.m_fpFMax = New cEwEFormatProvider(Me.m_uic, Me.m_nudEffortMax, GetType(Single))
        Me.m_fpIncr = New cEwEFormatProvider(Me.m_uic, Me.m_nudEffortIncr, GetType(Single))

        ' Init check boxes
        Try
            For iFleet As Integer = 1 To Me.m_uic.Core.nFleets
                Me.m_clbFleets.Items.Add(Me.m_uic.Core.EcopathFleetInputs(iFleet))
            Next iFleet
        Catch ex As Exception
            ' Aargh
        End Try

        ' Reflect parameters values in controls
        Me.UpdateControlValues()

        ' Start listening to core state changes
        AddHandler Me.m_uic.Core.StateMonitor.CoreExecutionStateEvent, AddressOf OnCoreStateChanged
        ' Start listening to parameter changes
        AddHandler Me.m_params.OnChanged, AddressOf OnParametersChanged

        Dim cmd As cCommand = Me.m_uic.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME)
        cmd.AddControl(Me.m_pbLenfest, "http://www.lenfestocean.org/")
        cmd.AddControl(Me.m_pbSAUP, "http://www.seaaroundus.org/")
        cmd.AddControl(Me.m_pbEcostProject, "http://www.ird.fr/ecostproject/doku.php")

        ' Force core state dependent initialization
        Me.OnCoreStateChanged(Me.m_uic.Core.StateMonitor)

    End Sub

#End Region ' Overrides

#Region " Events "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler; responds to core execution state changes.
    ''' </summary>
    ''' <param name="csm">Core state monitor that changes.</param>
    ''' -----------------------------------------------------------------------
    Private Sub OnCoreStateChanged(ByVal csm As cCoreStateMonitor)
        Me.ConfigureEcosimControls(csm.HasEcosimLoaded)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnRunWithEcopathCheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_chkRunWithEcopath.CheckedChanged
        If Me.m_bInUpdate Then Return
        Me.m_params.RunWithEcopath = Me.m_chkRunWithEcopath.Checked
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnRunWithEcosimCheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
          Handles m_chkRunWithEcosim.CheckedChanged
        If Me.m_bInUpdate Then Return
        Me.m_params.RunWithEcosim = Me.m_chkRunWithEcosim.Checked
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnRunWithFishingPolicySearchCheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
         Handles m_chkRunWithSearches.CheckedChanged
        If Me.m_bInUpdate Then Return
        Me.m_params.RunWithSearches = Me.m_chkRunWithSearches.Checked
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnAggregationModeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbAggNone.CheckedChanged, m_rbAggFleet.CheckedChanged, m_rbAggGroup.CheckedChanged

        If Me.m_bInUpdate Then Return
        If Me.m_rbAggNone.Checked Then
            Me.m_params.AggregationMode = cParameters.eAggregationModeType.FullModel
        ElseIf Me.m_rbAggFleet.Checked Then
            Me.m_params.AggregationMode = cParameters.eAggregationModeType.ByFleet
        Else
            Me.m_params.AggregationMode = cParameters.eAggregationModeType.ByGroup
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub OnAutoSaveChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_cbAutoSave.CheckedChanged
        If Me.m_bInUpdate Then Return
        My.Settings.AutosaveResults = Me.m_cbAutoSave.Checked
        Me.m_uic.Core.OnSettingsChanged()
    End Sub

    Private Sub m_nudEffortMin_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_nudEffortMin.ValueChanged
        If (Me.m_params Is Nothing) Then Return
        If Me.m_bInUpdate Then Return
        Me.m_params.EquilibriumEffortMin = CSng(Me.m_nudEffortMin.Value)
    End Sub

    Private Sub m_nudEffortMax_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_nudEffortMax.ValueChanged

        If (Me.m_params Is Nothing) Then Return
        If Me.m_bInUpdate Then Return

        Me.m_params.EquilibriumEffortMax = CSng(Me.m_nudEffortMax.Value)
    End Sub

    Private Sub m_nudEffortIncr_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_nudEffortIncr.ValueChanged

        If (Me.m_params Is Nothing) Then Return
        If Me.m_bInUpdate Then Return

        Me.m_params.EquilibriumEffortIncrement = CSng(Me.m_nudEffortIncr.Value)
    End Sub

    Private Sub OnFormatFleet(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_clbFleets.Format
        Dim item As ICoreInputOutput = DirectCast(e.ListItem, ICoreInputOutput)
        Dim fmt As New cCoreInterfaceFormatter()
        e.Value = fmt.GetDescriptor(item)
    End Sub

    Private Sub OnFleetSelected(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_clbFleets.SelectedIndexChanged

        If Me.m_bInUpdate Then Return

        Me.m_params.EquilibriumFleetsToVary.Clear()
        For Each iFleet As Integer In Me.m_clbFleets.CheckedIndices
            Me.m_params.EquilibriumFleetsToVary.Add(iFleet + 1)
        Next
    End Sub

    Private Sub OnParametersChanged(ByVal obj As cOOPStorable)
        Me.UpdateControlValues()
    End Sub

    Private Sub CoreMessageHandler(ByRef msg As cMessage)

        Try
            Select Case msg.Type
                Case eMessageType.GlobalSettingsChanged
                    Me.UpdateControlValues()
            End Select

        Catch ex As Exception
            cLog.Write(ex)
        End Try

    End Sub

#End Region ' Events

#Region " Internals "

    Private Sub UpdateControlValues()

        Me.m_bInUpdate = True
        Try
            Me.m_chkRunWithEcopath.Checked = Me.m_params.RunWithEcopath
            Me.m_chkRunWithEcosim.Checked = Me.m_params.RunWithEcosim
            Me.m_chkRunWithSearches.Checked = Me.m_params.RunWithSearches

            Me.m_cbAutoSave.Checked = My.Settings.AutosaveResults

            Me.m_rbAggNone.Checked = (Me.m_params.AggregationMode = cParameters.eAggregationModeType.FullModel)
            Me.m_rbAggFleet.Checked = (Me.m_params.AggregationMode = cParameters.eAggregationModeType.ByFleet)
            Me.m_rbAggGroup.Checked = (Me.m_params.AggregationMode = cParameters.eAggregationModeType.ByGroup)

            Me.m_fpFMin.Value = Me.m_params.EquilibriumEffortMin
            Me.m_fpFMax.Value = Me.m_params.EquilibriumEffortMax
            Me.m_fpIncr.Value = Me.m_params.EquilibriumEffortIncrement
        Catch ex As Exception

        End Try
        Me.m_bInUpdate = False

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Configure Ecosim-dependent parameter controls.
    ''' </summary>
    ''' <param name="bConnect">
    ''' True to connect to Ecosim, False to disconnect.
    ''' </param>
    ''' -----------------------------------------------------------------------
    Private Sub ConfigureEcosimControls(ByVal bConnect As Boolean)

        If (bConnect) Then

            If (Me.m_fpBaseYear Is Nothing) Then
                ' Create Ecosim dependent format provider(s)
                Me.m_fpBaseYear = New cPropertyFormatProvider(Me.m_uic, Me.m_nudBaseYear, Me.m_uic.Core.SearchObjective.ObjectiveParameters, eVarNameFlags.SearchBaseYear)
            End If

        Else

            If (Me.m_fpBaseYear IsNot Nothing) Then
                ' Release Ecosim dependent format provider(s)
                Me.m_fpBaseYear.Release()
                Me.m_fpBaseYear = Nothing
            End If

        End If

        ' Enable/disable Ecosim dependent controls
        If (Not Me.m_nudBaseYear.IsDisposed) Then
            Me.m_lblBaseYear.Enabled = bConnect
            Me.m_nudBaseYear.Enabled = bConnect

            Me.m_lblEffortMin.Enabled = bConnect
            Me.m_nudEffortMin.Enabled = bConnect

            Me.m_lblEffortMax.Enabled = bConnect
            Me.m_nudEffortMax.Enabled = bConnect

            Me.m_lbEffortIncr.Enabled = bConnect
            Me.m_nudEffortIncr.Enabled = bConnect

            Me.m_lblFleets.Enabled = bConnect
            Me.m_clbFleets.Enabled = bConnect
        End If

    End Sub

#End Region ' Internals

End Class
