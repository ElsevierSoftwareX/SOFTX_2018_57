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
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Ecotracer

    ''' <summary>
    ''' Form class, implements the Ecotracer generic parameters user interface.
    ''' </summary>
    Public Class frmEcotracerParameters

#Region " Private vars "

        Private m_fpScenarioName As cEwEFormatProvider = Nothing
        Private m_fpScenarioDescription As cEwEFormatProvider = Nothing
        Private m_fpAuthor As cEwEFormatProvider = Nothing
        Private m_fpContact As cEwEFormatProvider = Nothing
        Private m_propEcosimConTracing As cBooleanProperty = Nothing
        Private m_propEcospaceConTracing As cBooleanProperty = Nothing

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()
            InitializeComponent()
        End Sub

#End Region ' Constructor

#Region " Overloads and events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Dim scenarioDef As cEcotracerScenario = Me.Core.EcotracerScenarios(Me.Core.ActiveEcotracerScenarioIndex)

            Me.m_fpScenarioName = New cPropertyFormatProvider(Me.UIContext, Me.m_tbName, scenarioDef, eVarNameFlags.Name)
            Me.m_fpScenarioDescription = New cPropertyFormatProvider(Me.UIContext, Me.m_tbDescription, scenarioDef, eVarNameFlags.Description)
            Me.m_fpAuthor = New cPropertyFormatProvider(Me.UIContext, Me.m_tbAuthor, scenarioDef, eVarNameFlags.Author)
            Me.m_fpContact = New cPropertyFormatProvider(Me.UIContext, Me.m_tbContact, scenarioDef, eVarNameFlags.Contact)

            ' Try this
            Me.ConnectToEcosim(True)
            Me.ConnectToEcospace(True)

            Me.UpdateControls()

            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoSim, eCoreComponentType.EcoSpace}


        End Sub

        Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)

            Me.CoreComponents = Nothing

            Me.ConnectToEcosim(False)
            Me.ConnectToEcospace(False)

            ' Sanity checks
            Debug.Assert(Not Me.IsConnectedToEcosim())
            Debug.Assert(Not Me.IsConnectedToEcospace())

            Me.m_fpAuthor.Release()
            Me.m_fpContact.Release()
            Me.m_fpScenarioDescription.Release()
            Me.m_fpScenarioName.Release()

        End Sub

        Private Sub OnConTracingChanged(ByVal p As cProperty, ByVal cf As cProperty.eChangeFlags)
            Me.UpdateControls()
        End Sub

        Private Sub rbDisabled_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_rbDisabled.Click
            SetTracerRunMode(eTracerRunModeTypes.Disabled)
        End Sub

        Private Sub rbSim_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_rbSim.Click
            SetTracerRunMode(eTracerRunModeTypes.RunSim)
        End Sub

        Private Sub rbSpace_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_rbSpace.Click
            SetTracerRunMode(eTracerRunModeTypes.RunSpace)
        End Sub

        Private Sub OnVisitSAUP(sender As System.Object, e As System.EventArgs) _
            Handles m_pbSAUP.Click
            Me.OpenLink("http://www.seaaroundus.org/")
        End Sub

        Private Sub OnVisitFMIR(sender As System.Object, e As System.EventArgs) _
            Handles m_pbFMIR.Click
            ' ToDo: add FMIR sponsor link
            'Me.OpenLink("http://?/")
        End Sub

        Private Sub OnVisitEU(sender As System.Object, e As System.EventArgs) _
            Handles m_pbEU.Click
            ' ToDo: add EU sponsor link
            'Me.OpenLink("http://>")
        End Sub

        Private Sub OnVisitLenfest(sender As System.Object, e As System.EventArgs) _
            Handles m_pbLenfest.Click
            ' ToDo: add Lenfest sponsor link
            'Me.OpenLink("http://?/")
        End Sub

#End Region ' Overloads and events

#Region " Overrides "

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            If msg.Source = eCoreComponentType.EcoSim Then
                Me.ConnectToEcosim(Me.Core.ActiveEcosimScenarioIndex > 0)
            End If
            If msg.Source = eCoreComponentType.EcoSpace Then
                Me.ConnectToEcospace(Me.Core.ActiveEcospaceScenarioIndex > 0)
            End If
        End Sub

#End Region ' Overrides

#Region " Internals "

        Protected Overrides Sub UpdateControls()
            'UpdateControls() is called by the Ecosim and Ecospace properties PropertyChanged event handler 
            'This means anytime another interface or the core changes one of these flags this will be called 

            If Me.IsConnectedToEcosim Then
                If CBool(Me.m_propEcosimConTracing.GetValue()) = True Then
                    Me.m_rbSim.Checked = True
                    Exit Sub
                End If
            End If

            If Me.IsConnectedToEcospace Then
                If CBool(Me.m_propEcospaceConTracing.GetValue()) = True Then
                    Me.m_rbSpace.Checked = True
                    Exit Sub
                End If
            End If

            Me.m_rbDisabled.Checked = True

        End Sub

        Private Sub ConnectToEcosim(ByVal bConnect As Boolean)

            If bConnect Then

                ' Already connected? Abort
                If (Me.IsConnectedToEcosim() = True) Then Return
                If (Me.Core.ActiveEcosimScenarioIndex <= 0) Then Return

                Dim pm As cPropertyManager = Me.PropertyManager
                Dim ecosimModelParams As cEcoSimModelParameters = Me.Core.EcoSimModelParameters()

                Me.m_propEcosimConTracing = DirectCast(pm.GetProperty(ecosimModelParams, eVarNameFlags.ConSimOnEcoSim), cBooleanProperty)
                AddHandler Me.m_propEcosimConTracing.PropertyChanged, AddressOf OnConTracingChanged

            Else

                ' Already disconnected? Abort
                If Not Me.IsConnectedToEcosim() Then Return

                RemoveHandler Me.m_propEcosimConTracing.PropertyChanged, AddressOf OnConTracingChanged
                Me.m_propEcosimConTracing = Nothing

            End If

        End Sub

        Private Sub ConnectToEcospace(ByVal bConnect As Boolean)

            If bConnect Then

                ' Already connected? Abort
                If Me.IsConnectedToEcospace() Then Return
                If (Me.Core.ActiveEcospaceScenarioIndex <= 0) Then Return

                Dim pm As cPropertyManager = Me.PropertyManager
                Dim ecospaceModelParams As cEcospaceModelParameters = Me.Core.EcospaceModelParameters()

                Me.m_propEcospaceConTracing = DirectCast(pm.GetProperty(ecospaceModelParams, eVarNameFlags.ConSimOnEcoSpace), cBooleanProperty)
                AddHandler Me.m_propEcospaceConTracing.PropertyChanged, AddressOf OnConTracingChanged

            Else

                ' Already disconnected? Abort
                If Not Me.IsConnectedToEcospace() Then Return

                RemoveHandler Me.m_propEcospaceConTracing.PropertyChanged, AddressOf OnConTracingChanged
                Me.m_propEcospaceConTracing = Nothing

            End If

        End Sub

        Private Function IsConnectedToEcosim() As Boolean
            Return (Me.m_propEcosimConTracing IsNot Nothing)
        End Function

        Private Function IsConnectedToEcospace() As Boolean
            Return (Me.m_propEcospaceConTracing IsNot Nothing)
        End Function

        Private Sub SetTracerRunMode(ByVal tracerRunMode As eTracerRunModeTypes)

            Dim cmd As cCommand = Me.CommandHandler.GetCommand("EnableEcotracer")
            cmd.Tag = tracerRunMode
            cmd.Invoke()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Open an external link.
        ''' </summary>
        ''' <param name="strURL">The link to navigate to.</param>
        ''' -----------------------------------------------------------------------
        Private Sub OpenLink(strURL As String)

            Try
                Dim cmd As cBrowserCommand = DirectCast(Me.UIContext.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
                If (cmd IsNot Nothing) Then
                    cmd.Invoke(strURL)
                End If
            Catch ex As Exception
                cLog.Write(ex, "cEwEBioDivPlugin::NavigateTo(" & strURL & ")")
            End Try

        End Sub

#End Region ' Internals

    End Class

End Namespace ' Ecotracer
