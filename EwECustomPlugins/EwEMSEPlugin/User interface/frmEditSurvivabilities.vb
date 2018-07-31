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

Option Strict On
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources


Public Class frmEditSurvivabilities
    Implements IDisposable

    Private m_mse As cMSE = Nothing
    Private m_survivability As cSurvivability
    Private m_bIsDirty As Boolean

    Public Sub New(MSE As cMSE)
        Me.m_mse = MSE
        Me.InitializeComponent()
        Me.Grid = Me.m_grid
    End Sub

    Public Sub Init(ByVal uic As cUIContext)
        Me.UIContext = uic
        Me.m_grid.UIContext = uic
        Me.m_grid.Init(Me.m_mse, Me.m_mse.Survivability)
        Me.m_survivability = New cSurvivability(Me.m_mse, uic.Core, Me.m_mse.Survivability.EcosimData, Me.m_mse.Survivability.EcopathData)
        Me.m_survivability.Load()
        Me.UpdateGrid(Me.m_survivability.ListofSurvDistParams, My.Resources.HEADER_SURVIVABILITIES)
    End Sub

    Protected Overrides Sub OnLoad(e As System.EventArgs)

        MyBase.OnLoad(e)

        Me.QuickEditHandler.ShowImportExport = False
        Me.QuickEditHandler.Attach(Me.m_grid, Me.UIContext, Me.m_ts)

        AddHandler Me.m_grid.onEdited, AddressOf OnGridEdited

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

        Me.QuickEditHandler.Detach()
        RemoveHandler Me.m_grid.onEdited, AddressOf OnGridEdited
        Me.m_grid.UIContext = Nothing

        MyBase.OnFormClosed(e)

    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()
        ' Me.m_btnOK.Enabled = Me.m_bIsDirty
    End Sub

    Private Sub UpdateGrid(data As List(Of cSurvivability.cSurvivabilityDistributonParam), strName As String)
        Me.m_grid.Data = data
        Me.m_grid.DataName = String.Format(SharedResources.GENERIC_LABEL_DOUBLE, My.Resources.CAPTION, strName)
    End Sub

    Private Sub m_btnSave_Click(sender As System.Object, e As System.EventArgs) Handles m_btnSave.Click
        Dim lstrSubMessages As New List(Of String)
        Dim strFolder As String = cMSEUtils.MSEFolder(Me.m_mse.DataPath, cMSEUtils.eMSEPaths.DistrParams)

        'Saves all the parameters to csv when user clicks to save
        If m_survivability.Save() Then lstrSubMessages.Add(String.Format(My.Resources.STATUS_SAVED_DETAIL, "Survivabilities_dist.csv"))

        Me.m_bIsDirty = False

        Me.m_mse.InformUser(String.Format(My.Resources.STATUS_SAVED_DISTPARMS, My.Resources.CAPTION, strFolder), _
                                 eMessageImportance.Information, strFolder, lstrSubMessages.ToArray())

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub OnGridEdited()
        Me.m_bIsDirty = True
        Me.Invoke(New MethodInvoker(AddressOf UpdateControls))
    End Sub

    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnCancel.Click

        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()

    End Sub

End Class