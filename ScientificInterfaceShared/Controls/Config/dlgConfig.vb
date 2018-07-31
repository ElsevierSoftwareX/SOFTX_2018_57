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

Option Strict On
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls

Namespace Controls

    ''' <summary>
    ''' Generic configuration dialog for EwE.
    ''' </summary>
    Public Class dlgConfig
        Implements IUIElement

        Private m_ctrl As Control = Nothing

        Public Sub New(uic As cUIContext)
            Me.InitializeComponent()
            Me.UIContext = uic
        End Sub

        Public Property UIContext As ScientificInterfaceShared.Controls.cUIContext _
            Implements ScientificInterfaceShared.Controls.IUIElement.UIContext

        Public Shadows Function ShowDialog(owner As IWin32Window, strTitle As String, ctrl As Control) As DialogResult

            ' Set window text
            Me.Text = strTitle
            ' Store control
            Me.m_ctrl = ctrl
            ' Configure control
            If TypeOf ctrl Is IUIElement Then
                DirectCast(ctrl, IUIElement).UIContext = Me.UIContext
            End If

            ' Base, do your work
            Return MyBase.ShowDialog(owner)

        End Function

        Public Shadows Function ShowDialog(strTitle As String, ctrl As Control) As DialogResult
            Return Me.ShowDialog(Nothing, strTitle, ctrl)
        End Function

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            ' Resize to page control size
            Dim szPanel As Size = Me.m_plContent.Size
            Dim szPage As Size = Me.m_ctrl.Size

            Me.Size = New Size(Me.Width + szPage.Width - szPanel.Width, _
                               Me.Height + szPage.Height - szPanel.Height)

            Me.MinimumSize = Size

            Me.m_ctrl.Dock = DockStyle.Fill
            Me.m_plContent.Controls.Add(Me.m_ctrl)

            If (TypeOf Me.m_ctrl Is IOptionsPage) Then
                Dim opts As IOptionsPage = DirectCast(Me.m_ctrl, IOptionsPage)
                AddHandler opts.OnChanged, AddressOf OnOptionsPageChanged
                Me.OnOptionsPageChanged(opts, New EventArgs)
            End If

            Me.UpdateControls()
            Me.CenterToParent()

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)
            Me.m_plContent.Controls.Remove(Me.m_ctrl)
            Me.m_ctrl.Dispose()

            If (TypeOf Me.m_ctrl Is IOptionsPage) Then
                RemoveHandler DirectCast(Me.m_ctrl, IOptionsPage).OnChanged, AddressOf OnOptionsPageChanged
            End If

            Me.UIContext = Nothing
            MyBase.OnFormClosed(e)
        End Sub

        Private Sub OnOK(sender As System.Object, e As System.EventArgs) _
            Handles m_btnOK.Click

            Dim uic As cUIContext = Me.UIContext

            ' Status
            If (uic IsNot Nothing) Then
                cApplicationStatusNotifier.StartProgress(uic.Core, ScientificInterfaceShared.My.Resources.STATUS_APPLYVALUES)
            End If

            Try
                If (TypeOf Me.m_ctrl Is IOptionsPage) Then
                    DirectCast(Me.m_ctrl, IOptionsPage).Apply()
                End If
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()
            Catch ex As Exception
                cLog.Write(ex, "dlgConfig::OnOK")
            End Try

            ' Status
            If (uic IsNot Nothing) Then
                cApplicationStatusNotifier.EndProgress(uic.Core)
            End If

        End Sub

        Private Sub OnCancel(sender As System.Object, e As System.EventArgs) _
         Handles m_btnCancel.Click

            Try
                Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
                Me.Close()
            Catch ex As Exception
                cLog.Write(ex, "dlgConfig::OnOK")
            End Try

        End Sub

        Private Sub OnOptionsPageChanged(ByVal sender As IOptionsPage, ByVal args As EventArgs)
            'Lazy reflect state
            BeginInvoke(New MethodInvoker(AddressOf UpdateControls))
        End Sub

        Private Sub OnSetDefaults(sender As System.Object, e As System.EventArgs) _
            Handles m_btnDefaults.Click

            Try
                If (TypeOf Me.m_ctrl Is IOptionsPage) Then
                    DirectCast(Me.m_ctrl, IOptionsPage).SetDefaults()
                End If
            Catch ex As Exception
                cLog.Write(ex, "dlgConfig::OnSetDefaults")
            End Try

        End Sub

        Protected Overridable Sub UpdateControls()

            Dim bIsConfigPage As Boolean = TypeOf Me.m_ctrl Is IOptionsPage

            If bIsConfigPage Then
                Dim page As IOptionsPage = DirectCast(Me.m_ctrl, IOptionsPage)
                Me.m_btnOK.Enabled = page.CanApply
                Me.m_btnDefaults.Enabled = page.CanSetDefaults
            Else
                Me.m_btnOK.Enabled = True
                Me.m_btnCancel.Visible = False
                Me.m_btnDefaults.Visible = False
                Me.m_btnOK.Location = Me.m_btnCancel.Location
            End If

        End Sub

    End Class

End Namespace
