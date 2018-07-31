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

Imports System.IO
Imports WeifenLuo.WinFormsUI
Imports EwECore
Imports EwEUtils.Core

#End Region

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control; implements the Options > Presentations settings interface
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucOptionsPresentation
        Implements IOptionsPage
        Implements IUIElement

        Private m_fpW As cEwEFormatProvider = Nothing
        Private m_fpH As cEwEFormatProvider = Nothing
        Private m_szFrame As Size
        Private m_bInUpdate As Boolean = False

#Region " Constructors "

        Public Sub New(ByVal uic As cUIContext)

            Me.UIContext = uic
            Me.InitializeComponent()

        End Sub

#End Region ' Constructors

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            Me.m_cbHideModelBar.Checked = My.Settings.PresentationModeHideModelBar
            Me.m_cbHideMainMenu.Checked = My.Settings.PresentationModeHideMainMenu
            Me.m_cbHideStatusBar.Checked = My.Settings.PresentationModeHideStatusBar
            Me.m_cbCollapseNavPanel.Checked = My.Settings.PresentationModeCollapseNavPanel

            Dim frm As Form = Me.UIContext.FormMain
            Dim szOut As Size = frm.Size
            Dim szIn As Size = frm.ClientRectangle.Size

            Me.m_szFrame = New Size(szOut.Width - szIn.Width, szOut.Height - szIn.Height)

            Me.m_fpW = New cEwEFormatProvider(Me.UIContext, Me.m_tbxW, GetType(Integer))
            Me.m_fpH = New cEwEFormatProvider(Me.UIContext, Me.m_tbxH, GetType(Integer))
            Me.m_fpW.Value = szOut.Width
            Me.m_fpH.Value = szOut.Height

            Me.m_bInUpdate = True
            Me.m_rbOut.Checked = True
            Me.m_bInUpdate = False

            Me.UpdateControls()

        End Sub

#End Region ' Overrides

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext As cUIContext _
             Implements IUIElement.UIContext

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanApply"/>
        ''' -------------------------------------------------------------------
        Public Function CanApply() As Boolean _
              Implements IOptionsPage.CanApply
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.OnChanged"/>
        ''' -------------------------------------------------------------------
        Public Event OnOptionsPresentationChanged(sender As IOptionsPage, args As System.EventArgs) _
               Implements IOptionsPage.OnChanged

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.Apply"/>
        ''' -------------------------------------------------------------------
        Public Function Apply() As IOptionsPage.eApplyResultType _
             Implements IOptionsPage.Apply

            If Not Me.CanApply Then Return IOptionsPage.eApplyResultType.Failed

            My.Settings.PresentationModeHideModelBar = Me.m_cbHideModelBar.Checked
            My.Settings.PresentationModeHideMainMenu = Me.m_cbHideMainMenu.Checked
            My.Settings.PresentationModeHideStatusBar = Me.m_cbHideStatusBar.Checked
            My.Settings.PresentationModeCollapseNavPanel = Me.m_cbCollapseNavPanel.Checked

            Me.UIContext.FormMain.Size = Me.OuterSize

            Return IOptionsPage.eApplyResultType.Success

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.SetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Sub SetDefaults() _
                 Implements IOptionsPage.SetDefaults

            Try
                Me.m_cbHideModelBar.Checked = CBool(My.Settings.GetDefaultValue("PresentationModeHideModelBar"))
                Me.m_cbHideMainMenu.Checked = CBool(My.Settings.GetDefaultValue("PresentationModeHideMainMenu"))
                Me.m_cbHideStatusBar.Checked = CBool(My.Settings.GetDefaultValue("PresentationModeHideStatusBar"))
                Me.m_cbCollapseNavPanel.Checked = CBool(My.Settings.GetDefaultValue("PresentationModeCollapseNavPanel"))
                Me.m_rbIn.Checked = True
            Catch ex As Exception
                cLog.Write(ex, "ucOptionsPresentation::SetDefaults")
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanSetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Function CanSetDefaults() As Boolean _
            Implements IOptionsPage.CanSetDefaults
            Return True
        End Function

#End Region ' Public access

#Region " Internals "

        Private Sub UpdateControls()
            ' NOP
        End Sub

        Private Function OuterSize() As Size

            Dim w As Integer = CInt(Me.m_fpW.Value)
            Dim h As Integer = CInt(Me.m_fpH.Value)

            If (Me.m_rbIn.Checked) Then
                w += Me.m_szFrame.Width
                h += Me.m_szFrame.Height
            End If

            Return New Size(w, h)

        End Function

        Private Sub OnSizeModeToggled(sender As System.Object, e As System.EventArgs) _
            Handles m_rbOut.CheckedChanged

            If (Me.m_bInUpdate) Then Return

            Dim w As Integer = CInt(Me.m_fpW.Value)
            Dim h As Integer = CInt(Me.m_fpH.Value)

            If Me.m_rbOut.Checked Then
                w += Me.m_szFrame.Width
                h += Me.m_szFrame.Height
            Else
                w -= Me.m_szFrame.Width
                h -= Me.m_szFrame.Height
            End If

            Me.m_fpW.Value = w
            Me.m_fpH.Value = h

        End Sub

#End Region ' Internals

    End Class

End Namespace
