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
Imports EwEUtils.SystemUtilities.cSystemUtils

#End Region ' Imports

Namespace Controls

    ''' =======================================================================
    ''' <summary>
    ''' InputBox alternative for Mono compliance.
    ''' </summary>
    ''' =======================================================================
    Public Class frmInputBox

#Region " Private vars "

        ''' <summary>Value maintained in the box.</summary>
        Private m_strValue As String = ""

#End Region ' Private vars

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        Public Shadows Function Show(ByVal owner As IWin32Window, _
                                     ByVal strPrompt As String, _
                                     Optional strCaption As String = "", _
                                     Optional strInitialValue As String = "") As DialogResult
            Return Me.ShowDialog(owner, strPrompt, strCaption, strInitialValue)
        End Function

        Public Shadows Function Show(ByVal strPrompt As String, _
                                     Optional strCaption As String = "", _
                                     Optional strInitialValue As String = "") As DialogResult
            Return Me.ShowDialog(Nothing, strPrompt, strCaption, strInitialValue)
        End Function

        Public Shadows Function ShowDialog(owner As IWin32Window, _
                              ByVal strPrompt As String, _
                              Optional strCaption As String = "", _
                              Optional strInitialValue As String = "") As DialogResult
            Me.Text = strCaption
            Me.m_lblPrompt.Text = strPrompt
            Me.m_tbxValue.Text = strInitialValue
            Return MyBase.ShowDialog(owner)
        End Function

#Region " Events "

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            Me.CenterToParent()
            MyBase.OnLoad(e)
        End Sub

        Private Sub OnOK(sender As System.Object, e As System.EventArgs) _
            Handles m_btnOk.Click
            Me.m_strValue = Me.m_tbxValue.Text
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()
        End Sub

        Private Sub OnCancel(sender As System.Object, e As System.EventArgs) _
            Handles m_btnCancel.Click
            Me.m_strValue = ""
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        End Sub

        Private Sub OnInputTextChanged(sender As Object, e As System.EventArgs) _
            Handles m_tbxValue.TextChanged
            Me.UpdateControls()
        End Sub

#End Region ' Events

#Region " Public properties "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the value entered in the input box.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Value As String
            Get
                Return Me.m_strValue
            End Get
        End Property

#End Region ' Public properties

#Region " Internals "

        Private Sub UpdateControls()
            Me.m_btnOk.Enabled = (Not String.IsNullOrWhiteSpace(Me.m_tbxValue.Text))
        End Sub

#End Region ' Internals

    End Class

End Namespace
