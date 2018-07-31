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

#End Region ' Imports

Namespace Ecosim

    ''' <summary>
    ''' User interface to scale Vulnerabilities to Trophic Level.
    ''' </summary>
    Public Class dlgScaleVs

#Region "Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_bInUpdate As Boolean = False

        Private WithEvents m_fpLow As cEwEFormatProvider = Nothing
        Private WithEvents m_fpHigh As cEwEFormatProvider = Nothing

#End Region 'Private vars

#Region " Construction "

        Public Sub New(uic As cUIContext)
            Me.InitializeComponent()
            Me.m_uic = uic
        End Sub

#End Region 'Construction

#Region " Form overrides "

        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)

            Me.m_bInUpdate = True

            Dim metaMin As New cVariableMetaData(1, 1000000, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo), 1.2!)
            Dim metaMax As New cVariableMetaData(2.1, 1000000, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo), 1.2!)
            Me.m_fpLow = New cEwEFormatProvider(Me.m_uic, m_tbxLow, GetType(Single), metaMin)
            Me.m_fpHigh = New cEwEFormatProvider(Me.m_uic, m_tbxHigh, GetType(Single), metaMax)

            Me.m_fpLow.Value = 1.2
            Me.m_fpHigh.Value = 10

            Me.m_bInUpdate = False

            Me.CenterToParent()
            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)

            Me.m_fpLow.Release()
            Me.m_fpHigh.Release()

            MyBase.OnFormClosed(e)

        End Sub

#End Region 'Form overrides

#Region " Events "

        Private Sub OnInputChanged(sender As Object, args As EventArgs) _
            Handles m_fpLow.OnValueChanged, m_fpHigh.OnValueChanged

            If (Me.m_bInUpdate) Then Return
            Me.UpdateControls()

        End Sub

        Private Sub OnCalculate(sender As Object, e As EventArgs) _
            Handles m_btnOK.Click

            If (Me.m_uic.Core.ScaleVulnerabilitiesToTL(CSng(Me.m_fpLow.Value), CSng(Me.m_fpHigh.Value))) Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If

        End Sub

        Private Sub OnCancel(sender As Object, e As EventArgs) _
            Handles m_btnCancel.Click

            Me.DialogResult = DialogResult.Cancel
            Me.Close()

        End Sub

#End Region 'Events

#Region " Internals "

        Private Sub UpdateControls()

            Try
                Me.m_btnOK.Enabled = (CSng(Me.m_fpLow.Value) < CSng(Me.m_fpHigh.Value))
            Catch ex As Exception
                cLog.Write(ex, "dlgScaleVs.UpdateControls")
            End Try

        End Sub

#End Region 'Internals

    End Class

End Namespace
