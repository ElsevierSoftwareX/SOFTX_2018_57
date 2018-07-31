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
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Controls.Wizard

    ''' =======================================================================
    ''' <summary>
    ''' Base class for implementing a GUI-driven wizard.
    ''' </summary>
    ''' <remarks>
    ''' Note that this class can be severely improved. For one, it does
    ''' not support branches in the logic. Pages be connected in a parent/child
    ''' tree structure, etc. For now I have not bothered.
    ''' </remarks>
    ''' =======================================================================
    Public Class ucWizardNavigation
        Implements IWizardNavigation

#Region " Private vars "

        ''' <summary>The attached wizard.</summary>
        Private m_wizard As cWizard = Nothing

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()

            Me.InitializeComponent()
            Me.UpdateNavigation()

        End Sub

#End Region ' Constructor

#Region " IWizardNavigation implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Attach a wizard to a navigation structure
        ''' </summary>
        ''' <param name="wizard">The wizard to attach.</param>
        ''' -------------------------------------------------------------------
        Public Sub Attach(ByVal wizard As cWizard) _
              Implements IWizardNavigation.Attach
            Me.m_wizard = wizard
            Me.UpdateNavigation()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Detach the current wizard from a navigation structure
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Detach() _
              Implements IWizardNavigation.Detach
            Me.m_wizard = Nothing
            Me.UpdateNavigation()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Wizard callback, informing the navigation structure to update
        ''' itself.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub UpdateNavigation() _
              Implements IWizardNavigation.UpdateNavigation

            Dim bCanNavBack As Boolean = False
            Dim bCanNavForward As Boolean = False
            Dim bCanClose As Boolean = False
            Dim bCanFinish As Boolean = False

            If (Me.m_wizard IsNot Nothing) Then
                bCanNavBack = Me.m_wizard.CanNavBack()
                bCanNavForward = Me.m_wizard.CanNavForward()
                bCanClose = Me.m_wizard.CanClose()
                bCanFinish = Me.m_wizard.CanFinish()
            End If

            Me.m_btnBack.Enabled = bCanNavBack
            Me.m_btnNext.Enabled = bCanNavForward

            If bCanFinish Then
                Me.m_btnClose.Text = My.Resources.BUTTON_OK
            Else
                Me.m_btnClose.Text = My.Resources.BUTTON_CANCEL
            End If
            Me.m_btnClose.Enabled = bCanClose

        End Sub

#End Region ' IWizardNavigation implementation

#Region " Control events "

        Private Sub OnBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnBack.Click
            Me.m_wizard.NavigateBack()
        End Sub

        Private Sub OnNext_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnNext.Click
            Me.m_wizard.NavigateNext()
        End Sub

        Private Sub OnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnClose.Click

            Dim bCanFinish As Boolean = False

            If Me.m_wizard.CanFinish() Then
                Me.m_wizard.Close(DialogResult.OK)
            Else
                Me.m_wizard.Close(DialogResult.Cancel)
            End If

        End Sub

#End Region ' Control events

    End Class

End Namespace ' Controls.Wizard
