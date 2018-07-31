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

Option Explicit On
Option Strict On

Imports EwECore
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form implementing the Ecosim parameters user interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmEcosimParameters

#Region " Private vars "

        Private m_fpScenarioName As cEwEFormatProvider = Nothing
        Private m_fpScenarioDescription As cEwEFormatProvider = Nothing
        Private m_fpAuthor As cEwEFormatProvider = Nothing
        Private m_fpContact As cEwEFormatProvider = Nothing
        Private m_fpNumYears As cEwEFormatProvider = Nothing
        Private m_fpNutBaseFreeProp As cEwEFormatProvider = Nothing
        Private m_fpPredictEffort As cEwEFormatProvider = Nothing
        Private m_fpUseVarPQ As cEwEFormatProvider = Nothing
        Private m_fpForagingTimeLowerLimit As cEwEFormatProvider = Nothing
        Private m_fpSORwt As cEwEFormatProvider = Nothing

        Private m_propConTracing As cBooleanProperty = Nothing
        Private m_propPredictEffort As cBooleanProperty = Nothing

#End Region ' Private vars

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            Me.m_bInUpdate = True

            Dim parms As cEcoSimModelParameters = Core.EcoSimModelParameters()
            Dim pm As cPropertyManager = Me.PropertyManager

            Me.m_fpNumYears = New cPropertyFormatProvider(Me.UIContext, Me.m_nudNumberYears, parms, eVarNameFlags.EcoSimNYears)
            Me.m_fpNutBaseFreeProp = New cPropertyFormatProvider(Me.UIContext, Me.m_nudNutBaseFreeProp, parms, eVarNameFlags.NutBaseFreeProp)

            Me.m_fpPredictEffort = New cPropertyFormatProvider(Me.UIContext, Me.m_chkPredictEffort, parms, eVarNameFlags.PredictEffort)
            Me.m_fpUseVarPQ = New cPropertyFormatProvider(Me.UIContext, Me.m_chkUseVarPQ, parms, eVarNameFlags.UseVarPQ)
            Me.m_fpForagingTimeLowerLimit = New cPropertyFormatProvider(Me.UIContext, Me.m_tbxMinFeedingRateAdjustment, parms, eVarNameFlags.ForagingTimeLowerLimit)
            Me.m_fpSORwt = New cPropertyFormatProvider(Me.UIContext, Me.m_txSORwt, parms, eVarNameFlags.EcosimSORWt)

            Me.m_propConTracing = DirectCast(pm.GetProperty(parms, eVarNameFlags.ConSimOnEcoSim), cBooleanProperty)
            AddHandler Me.m_propConTracing.PropertyChanged, AddressOf OnConTracingChanged

            Me.m_propPredictEffort = DirectCast(pm.GetProperty(parms, eVarNameFlags.PredictEffort), cBooleanProperty)
            AddHandler Me.m_propPredictEffort.PropertyChanged, AddressOf OnPredictEffortChanged

            ' Listen to shapes data added or removed messages
            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.ShapesManager, eCoreComponentType.EcoSim}

            Me.UpdateEnvForcingControls()
            Me.RebuildScenarioFormatProviders()
            Me.UpdateControls()

            Me.m_cmbNutForcing.SelectedIndex = Math.Max(0, parms.NutForceFunctionNumber)

            Me.m_bInUpdate = False

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            Me.m_fpScenarioName.Release()
            Me.m_fpScenarioDescription.Release()
            Me.m_fpAuthor.Release()
            Me.m_fpContact.Release()
            Me.m_fpNumYears.Release()
            Me.m_fpNutBaseFreeProp.Release()
            Me.m_fpPredictEffort.Release()
            Me.m_fpUseVarPQ.Release()
            Me.m_fpForagingTimeLowerLimit.Release()
            Me.m_fpSORwt.Release()

            RemoveHandler Me.m_propConTracing.PropertyChanged, AddressOf OnConTracingChanged
            Me.m_propConTracing = Nothing

            RemoveHandler Me.m_propPredictEffort.PropertyChanged, AddressOf OnPredictEffortChanged
            Me.m_propPredictEffort = Nothing

            MyBase.OnFormClosed(e)

        End Sub

        Dim m_bInUpdate As Boolean = False

        Private Sub chkConTracing_Click(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_chkConTracing.Click, m_chkUseVarPQ.Click

            If (Me.m_bInUpdate = True) Then Return

            Me.m_bInUpdate = True

            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand("EnableEcotracer")
            If (cmd IsNot Nothing) Then
                If (Me.m_chkConTracing.Checked) Then
                    cmd.Tag = eTracerRunModeTypes.RunSim
                Else
                    cmd.Tag = eTracerRunModeTypes.Disabled
                End If
                cmd.Invoke()
                If (Me.Core.ActiveEcotracerScenarioIndex <= 0) Then
                    Me.m_chkConTracing.Checked = False
                End If
            End If

            ' If tracer scenario loaded turn this on
            Me.m_propConTracing.SetValue(Me.m_chkConTracing.Checked)

            Me.m_bInUpdate = False

        End Sub

        Private Sub OnConTracingChanged(ByVal p As cProperty, ByVal cf As cProperty.eChangeFlags)
            Me.UpdateControls()
        End Sub

        Private Sub OnPredictEffortChanged(ByVal p As cProperty, ByVal cf As cProperty.eChangeFlags)
            '   Me.m_fpRegulatoryFeedback.Value = Me.m_fpPredictEffort.Value
            Me.UpdateControls()
        End Sub

        Private Sub OnFormatFF(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
            Handles m_cmbNutForcing.Format
            Try
                Dim fmt As New cShapeDataFormatter()
                e.Value = fmt.GetDescriptor(e.ListItem)
            Catch ex As Exception
                Debug.Assert(False)
            End Try
        End Sub

        Private Sub OnFFSelectionChanged(sender As Object, e As System.EventArgs) _
            Handles m_cmbNutForcing.SelectedIndexChanged

            If Me.m_bInUpdate Then Return

            Try
                Dim parms As cEcoSimModelParameters = Core.EcoSimModelParameters()
                parms.NutForceFunctionNumber = Me.m_cmbNutForcing.SelectedIndex
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Events

#Region " Overrides "

        Public Overrides Sub OnCoreMessage(ByVal msg As cMessage)
            If ((msg.Source = eCoreComponentType.ShapesManager) And (msg.Type = eMessageType.DataAddedOrRemoved)) Then
                Me.UpdateEnvForcingControls()
            End If

            If msg.Source = eCoreComponentType.EcoSim And msg.Type = eMessageType.DataAddedOrRemoved Then
                Me.RebuildScenarioFormatProviders()
            End If
        End Sub

#End Region ' Overrides

#Region " Internals "

        Private Sub RebuildScenarioFormatProviders()

            Dim scenarioDef As cEcoSimScenario = Nothing

            If (Me.Core.ActiveEcosimScenarioIndex > 0) Then
                scenarioDef = Me.Core.EcosimScenarios(Me.Core.ActiveEcosimScenarioIndex)
            End If

            If Me.m_fpScenarioName IsNot Nothing Then Me.m_fpScenarioName.Release()
            If Me.m_fpScenarioDescription IsNot Nothing Then Me.m_fpScenarioDescription.Release()
            If Me.m_fpAuthor IsNot Nothing Then Me.m_fpAuthor.Release()
            If Me.m_fpContact IsNot Nothing Then Me.m_fpContact.Release()

            If (scenarioDef IsNot Nothing) Then
                Me.m_fpScenarioName = New cPropertyFormatProvider(Me.UIContext, Me.m_tbName, scenarioDef, eVarNameFlags.Name)
                Me.m_fpScenarioDescription = New cPropertyFormatProvider(Me.UIContext, Me.m_tbDescription, scenarioDef, eVarNameFlags.Description)
                Me.m_fpAuthor = New cPropertyFormatProvider(Me.UIContext, Me.m_tbAuthor, scenarioDef, eVarNameFlags.Author)
                Me.m_fpContact = New cPropertyFormatProvider(Me.UIContext, Me.m_tbContact, scenarioDef, eVarNameFlags.Contact)
            End If

        End Sub

        Private Sub UpdateEnvForcingControls()
            Dim ffm As cForcingFunctionShapeManager = Me.Core.ForcingShapeManager()
            Dim aItems(ffm.Count) As Object

            aItems(0) = ""
            For iFF As Integer = 0 To ffm.Count - 1
                aItems(iFF + 1) = ffm(iFF)
            Next

            Me.m_cmbNutForcing.Items.AddRange(aItems)
        End Sub

        Protected Overrides Sub UpdateControls()

            If (Me.m_propConTracing Is Nothing) Then Return
            Me.m_chkConTracing.Checked = CBool(Me.m_propConTracing.GetValue())

        End Sub

#End Region ' Internals

    End Class

End Namespace
