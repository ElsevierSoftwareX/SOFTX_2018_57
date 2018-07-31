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

Imports ScientificInterfaceShared.Controls
Imports SourceGrid2

Public Class frmStrategiesOverview

    Private m_mse As cMSE = Nothing
    Private m_data As Strategies = Nothing

    Public Sub New()
        MyBase.New()
        Me.InitializeComponent()
    End Sub

    Public Sub Init(ByVal uic As cUIContext, ByVal mse As cMSE)
        Me.m_mse = mse
        Me.m_data = mse.Strategies
        'Me.m_data = New Strategies(mse, mse.Core)
        Me.m_data.Load()
        Me.Grid = m_grid
        'Me.m_grid.Init(Me.m_data)
        Me.UIContext = uic
    End Sub

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.QuickEditHandler.ShowImportExport = False
        Me.QuickEditHandler.Attach(Me.m_grid, Me.UIContext, Me.m_ts)

        Me.m_grid.Init(Me.m_data)

    End Sub

    Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)
        Me.QuickEditHandler.Detach()
        MyBase.OnFormClosed(e)
    End Sub

    Private Sub OnCancel(sender As System.Object, e As System.EventArgs) _
    Handles m_btnCancel.Click

        Try
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnOK(sender As System.Object, e As System.EventArgs) _
    Handles m_btnSave.Click

        Try
            ' Save to default location
            If Me.m_data.Save("") Then
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub m_btnCheckAll_Click(sender As Object, e As EventArgs) Handles m_btnCheckAll.Click

        m_grid.CheckAll()

    End Sub

    Private Sub m_btnCheckNone_Click(sender As Object, e As EventArgs) Handles m_btnCheckNone.Click

        m_grid.UncheckAll()

    End Sub
End Class