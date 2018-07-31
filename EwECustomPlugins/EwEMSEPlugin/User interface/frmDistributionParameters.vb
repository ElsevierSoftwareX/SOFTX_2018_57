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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Public Class frmDistributionParameters
    Implements IDisposable

    Private m_ecopathdist As cEcopathDistributionParams = Nothing
    Private m_ecosimdist As cEcosimDistributionParams = Nothing

    Public Enum eParameterSet As Integer
        Ecopath = 0
        Ecosim
    End Enum

    Private Class ParamComboItem
        Public Sub New(paramname As cDistributionParams.eDistrParamName, text As String, ByRef data As List(Of IDistributionParamsData))
            Me.ParamName = paramname
            Me.Text = text
            Me.Data = data
        End Sub
        Public Property ParamName As cDistributionParams.eDistrParamName
        Public Property Text As String
        Public Property Data As List(Of IDistributionParamsData)
        Public Overrides Function ToString() As String
            Return Me.Text
        End Function
    End Class

    Private m_plugin As cMSEPluginPoint = Nothing

    Private nPPers As Integer
    Private m_bIsDirty As Boolean

    Public Sub New()
        Me.InitializeComponent()
    End Sub

    Public Sub Init(ByVal uic As cUIContext, ByVal Plugin As cMSEPluginPoint)

        Me.m_grid.UIContext = uic
        Me.UIContext = uic
        Me.m_plugin = Plugin

        For i As Integer = 1 To Me.Core.nGroups
            If Me.Core.EcoPathGroupInputs(i).IsProducer Then nPPers += 1
        Next

        ' JS: Item indexes should obviously correspond to eParameterSet enum values
        Me.m_tscmPathOrSim.Items.Add(SharedResources.HEADER_ECOPATH)
        Me.m_tscmPathOrSim.Items.Add(SharedResources.HEADER_ECOSIM)

    End Sub

    Private ReadOnly Property MSE As cMSE
        Get
            Return Me.m_plugin.MSE
        End Get
    End Property

#Region " Overrides "

    Protected Overrides Sub OnLoad(e As System.EventArgs)

        ' JS 30Sep13: globalized this method
        MyBase.OnLoad(e)

        AddHandler Me.m_grid.onEdited, AddressOf OnGridEdited

        Me.m_ecopathdist = New cEcopathDistributionParams(Me.MSE, Me.Core)
        Me.m_ecosimdist = New cEcosimDistributionParams(Me.MSE, Me.Core)

        ' ToDo: use proper message
        If Me.m_ecopathdist.Load(Nothing) = False Then
            Me.m_plugin.InformUser(My.Resources.ERROR_DISTRPAR_LOAD_ECOPATH, eMessageImportance.Warning)
        End If

        If Me.m_ecosimdist.Load(Nothing) = False Then
            Me.m_plugin.InformUser(My.Resources.ERROR_DISTRPAR_LOAD_ECOSIM, eMessageImportance.Warning)
        End If

        'initialises the dropdown box to the Ecopath parameters
        Me.m_tscmPathOrSim.SelectedIndex = eParameterSet.Ecopath

        Me.m_bIsDirty = False
        Me.UpdateControls()

    End Sub

    Protected Overrides Sub OnFormClosing(e As System.Windows.Forms.FormClosingEventArgs)

        If (Me.m_bIsDirty = True) Then
            ' JS 02Oct13: globalized this method
            ' JS 02Oct13: replaced MsgBox with cFeedbackMessage
            Dim fmsg As New cFeedbackMessage(My.Resources.PROMPT_UNSAVED_CHANGES, _
                                 eCoreComponentType.External, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
            fmsg.Reply = eMessageReply.YES
            Me.Core.Messages.SendMessage(fmsg)
            e.Cancel = (fmsg.Reply <> eMessageReply.YES)
        End If

        MyBase.OnFormClosing(e)

    End Sub

    Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

        RemoveHandler Me.m_grid.onEdited, AddressOf OnGridEdited
        Me.m_grid.UIContext = Nothing
        MyBase.OnFormClosed(e)

    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()
        ' Me.m_btnOK.Enabled = Me.m_bIsDirty
    End Sub

#End Region ' Overrides

#Region " Internals "

    Private Sub UpdateGrid(data As IDistributionParamsData(), strName As String)
        Me.m_grid.Data = data
        Me.m_grid.DataName = String.Format(SharedResources.GENERIC_LABEL_DOUBLE, My.Resources.CAPTION, strName)
    End Sub

#End Region ' Internals

#Region " Control events "

    ''' <summary>
    ''' Everytime the user changes the parameter type combobox from Ecopath 
    ''' Parameters to Ecosim Parameters and vice versa. This gets called to 
    ''' change all the options in the combobox used to specify the parameter 
    ''' name.
    ''' </summary>
    Private Sub OnModelSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tscmPathOrSim.SelectedIndexChanged

        ' JS 02Oct13: preserve unsaved changes flag
        Dim bSave As Boolean = Me.m_bIsDirty

        ' JS 02Oct13: globalized this method
        ' JS 02Oct13: used a class to encapsulate param instead of relying on item text

        If m_tscmPathOrSim.SelectedIndex = eParameterSet.Ecopath Then
            Me.m_grid.Mode = eParameterSet.Ecopath
            Me.m_tscmParamName.Items.Clear()
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.B, SharedResources.HEADER_BIOMASS, Me.m_ecopathdist.B))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.BA, SharedResources.HEADER_BIOMACCUM_ABBR, Me.m_ecopathdist.BA))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.QB, SharedResources.HEADER_CONSUMPTION_OVER_BIOMASS, Me.m_ecopathdist.QB))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.PB, SharedResources.HEADER_PRODUCTION_OVER_BIOMASS, Me.m_ecopathdist.PB))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.EE, "EE", Me.m_ecopathdist.EE))
            Me.m_tscmParamName.SelectedIndex = 0
        ElseIf m_tscmPathOrSim.SelectedIndex = eParameterSet.Ecosim Then
            Me.m_grid.Mode = eParameterSet.Ecosim
            Me.m_tscmParamName.Items.Clear()
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.DenDepCatchability, SharedResources.HEADER_DENDEPCATCHABILITY_ABBR, Me.m_ecosimdist.DenDepCatchability))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.SwitchingPower, SharedResources.HEADER_SWITCHINGPOWER, Me.m_ecosimdist.SwitchingPower))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.QBMaxxQBio, My.Resources.HEADER_QBMAX_X_PBMAX, Me.m_ecosimdist.QBMaxxQBio))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.PredEffectFeedingTime, My.Resources.HEADER_PREDEFFECTFEEDINGTIME, Me.m_ecosimdist.PredEffectFeedingTime))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.OtherMortFeedingTime, My.Resources.HEADER_OTHERMORTFEEDTIME, Me.m_ecosimdist.OtherMortFeedingTime))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.MaxRelFeedingTime, My.Resources.HEADER_MAXRELFEEDTIME, Me.m_ecosimdist.MaxRelFeedingTime))
            Me.m_tscmParamName.Items.Add(New ParamComboItem(cDistributionParams.eDistrParamName.FeedingTimeAdjustRate, My.Resources.HEADER_FEEDTIMEADJUSTRATE, Me.m_ecosimdist.FeedingTimeAdjustRate))
            Me.m_tscmParamName.SelectedIndex = 0

        End If

        Me.m_bIsDirty = True

    End Sub

    Private Sub OnGridEdited()
        Me.m_bIsDirty = True
        Me.Invoke(New MethodInvoker(AddressOf UpdateControls))
    End Sub

    Private Sub OnParamSelected(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_tscmParamName.SelectedIndexChanged

        Try
            Dim item As ParamComboItem = DirectCast(Me.m_tscmParamName.SelectedItem, ParamComboItem)
            Me.UpdateGrid(item.Data.ToArray, item.Text)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnCancel.Click

        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()

    End Sub

    Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSave.Click

        Dim lstrSubMessages As New List(Of String)
        Dim strFolder As String = cMSEUtils.MSEFolder(Me.MSE.DataPath, cMSEUtils.eMSEPaths.DistrParams)

        If Not Me.MSE.ResolveMSEPathConflicts(False) Then
            Return
        End If

        Me.m_ecopathdist.Save()
        Me.m_ecosimdist.Save()

        Me.m_bIsDirty = False

        Me.m_plugin.InformUser(String.Format(My.Resources.STATUS_SAVED_DISTPARMS, My.Resources.CAPTION, strFolder), _
                                 eMessageImportance.Information, strFolder, lstrSubMessages.ToArray())

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()

    End Sub

#End Region ' Control events



End Class