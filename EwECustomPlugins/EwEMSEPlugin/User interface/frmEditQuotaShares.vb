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

Public Class frmEditQuotaShares

    Private m_mse As cMSE = Nothing
    Private m_quotashares As cQuotaShares
    Private m_bIsDirty As Boolean

    Public Sub New(MSE As cMSE)

        Me.m_mse = MSE
        Me.InitializeComponent()
        Me.Grid = Me.m_grid

    End Sub

    Public Sub Init(ByVal uic As cUIContext)
        Me.UIContext = uic
        Me.m_grid.UIContext = uic
        Me.m_grid.Init(Me.m_mse, Me.m_mse.QuotaShares)
        Me.m_quotashares = New cQuotaShares(Me.m_mse, uic.Core)
        Me.m_quotashares.Load()
        Me.UpdateGrid(Me.m_quotashares.GetLstGrpShares, My.Resources.HEADER_QUOTASHARE)
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

    Private Sub UpdateGrid(data As List(Of cQuotaShares.QuotaShare), strName As String)
        Me.m_grid.Data = data
        Me.m_grid.DataName = String.Format(SharedResources.GENERIC_LABEL_DOUBLE, My.Resources.CAPTION, strName)
    End Sub

    Private Sub OutputMessageNotSum1(ByVal Groups_Not_Summing_to_1 As List(Of Integer))

        Dim strGroupsNotSum1 As String

        strGroupsNotSum1 = m_mse.Core.EcoPathGroupInputs(Groups_Not_Summing_to_1(0)).Name

        If Groups_Not_Summing_to_1.Count > 1 Then
            For i = 2 To Groups_Not_Summing_to_1.Count
                If i = Groups_Not_Summing_to_1.Count Then
                    strGroupsNotSum1 &= " and " & m_mse.Core.EcoPathGroupInputs(Groups_Not_Summing_to_1(i - 1)).Name
                Else
                    strGroupsNotSum1 &= ", " & m_mse.Core.EcoPathGroupInputs(Groups_Not_Summing_to_1(i - 1)).Name
                End If
            Next
        End If

        'flag an error
        If Groups_Not_Summing_to_1.Count = 1 Then
            MessageBox.Show("The quotashares for group " & strGroupsNotSum1 & " do not sum to 1")
        Else
            MessageBox.Show("The quotashares for groups " & strGroupsNotSum1 & " do not sum to 1")
        End If

    End Sub

    Private Sub m_btnSave_Click(sender As System.Object, e As System.EventArgs) Handles m_btnSave.Click

        Dim lstrSubMessages As New List(Of String)
        Dim strFolder As String = cMSEUtils.MSEFolder(Me.m_mse.DataPath, cMSEUtils.eMSEPaths.Fleet)
        Dim Groups_Not_Summing_to_1 As New List(Of Integer)

        If Not QuotaSharesSumTo1(Groups_Not_Summing_to_1) Then

            OutputMessageNotSum1(Groups_Not_Summing_to_1)
            Exit Sub

        End If

        'Saves all the parameters to csv when user clicks to save
        If m_quotashares.Save() Then lstrSubMessages.Add(String.Format(My.Resources.STATUS_SAVED_DETAIL, "QuotaShares.csv"))

        Me.m_bIsDirty = False

        Me.m_mse.InformUser(String.Format(My.Resources.STATUS_SAVED_QUOTASHARES, My.Resources.CAPTION, strFolder),
                                 eMessageImportance.Information, strFolder, lstrSubMessages.ToArray())

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Function QuotaSharesSumTo1(ByRef GroupsNotSum1 As List(Of Integer)) As Boolean

        Const Precision As Single = 0.0001
        Dim QShares As List(Of cQuotaShares.QuotaShare) = m_quotashares.GetLstGrpShares
        Dim SumShare(m_mse.Core.nGroups) As Single
        Dim PassTest As Boolean = True

        For Each iQuotaShare In QShares
            SumShare(iQuotaShare.mGroupNo) += iQuotaShare.mShare
        Next

        For iGrp = 1 To m_mse.Core.nGroups
            If SumShare(iGrp) <> 0 Then
                If SumShare(iGrp) < 1 - Precision Or SumShare(iGrp) > 1 + Precision Then
                    GroupsNotSum1.Add(iGrp)
                    PassTest = False
                End If
            End If
        Next

        Return PassTest

    End Function

    Private Sub OnGridEdited()
        Me.m_bIsDirty = True
        Me.Invoke(New MethodInvoker(AddressOf UpdateControls))
    End Sub

    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    Handles m_btnCancel.Click

        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()

    End Sub

    Private Sub btnSum2One_Click(sender As Object, e As EventArgs) Handles btnSum2One.Click

        Dim iStart As Integer = 0
        Dim SumQuotaShare As Single = 0


        SumQuotaShare = m_quotashares.ReadRowDist(0).mShare

        For i = 2 To m_quotashares.CountDist
            If m_quotashares.ReadRowDist(i - 1).mGroupNo <> m_quotashares.ReadRowDist(i - 2).mGroupNo Then
                For iChange = iStart To i - 2
                    m_quotashares.ReadRowDist(iChange).mShare /= SumQuotaShare
                Next
                SumQuotaShare = 0
                iStart = i - 1
            End If
            If m_quotashares.ReadRowDist(i - 1).mShare > 0 Then SumQuotaShare += m_quotashares.ReadRowDist(i - 1).mShare
        Next

        For iChange = iStart To m_quotashares.CountDist - 1
            m_quotashares.ReadRowDist(iChange).mShare /= SumQuotaShare
        Next

        Me.m_grid.Init(Me.m_mse, m_quotashares)

        Me.UpdateGrid(Me.m_quotashares.GetLstGrpShares, My.Resources.HEADER_SURVIVABILITIES)

    End Sub
End Class