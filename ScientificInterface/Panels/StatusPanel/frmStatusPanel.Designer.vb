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
Imports ScientificInterfaceShared.Forms

#End Region ' Imports

Partial Class frmStatusPanel
    Inherits frmEwEDockContent

    Private components As System.ComponentModel.IContainer

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStatusPanel))
        Me.m_tvStatus = New ScientificInterfaceShared.Controls.cNavigateTreeview()
        Me.SuspendLayout()
        '
        'm_tvStatus
        '
        resources.ApplyResources(Me.m_tvStatus, "m_tvStatus")
        Me.m_tvStatus.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText
        Me.m_tvStatus.FullRowSelect = True
        Me.m_tvStatus.HideSelection = False
        Me.m_tvStatus.Name = "m_tvStatus"
        Me.m_tvStatus.ShowImages = True
        Me.m_tvStatus.ShowLines = False
        Me.m_tvStatus.ShowTime = False
        '
        'frmStatusPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CloseButtonVisible = False
        Me.Controls.Add(Me.m_tvStatus)
        Me.DockAreas = CType((((WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) _
            Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) _
            Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom), WeifenLuo.WinFormsUI.Docking.DockAreas)
        Me.DoubleBuffered = True
        Me.HideOnClose = True
        Me.Name = "frmStatusPanel"
        Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_tvStatus As ScientificInterfaceShared.Controls.cNavigateTreeview

End Class


