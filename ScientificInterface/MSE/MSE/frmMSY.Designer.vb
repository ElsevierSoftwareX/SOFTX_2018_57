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

Imports ScientificInterfaceShared.Forms

Partial Class frmMSY
    Inherits frmEwE

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMSY))
        Me.m_btnRunMSY = New System.Windows.Forms.Button()
        Me.m_btnStop = New System.Windows.Forms.Button()
        Me.txtMSYresults = New System.Windows.Forms.TextBox()
        Me.m_btnFleetTradeoffs = New System.Windows.Forms.Button()
        Me.m_rbValue = New System.Windows.Forms.RadioButton()
        Me.rbCatch = New System.Windows.Forms.RadioButton()
        Me.m_lblMSY = New System.Windows.Forms.Label()
        Me.m_hdrOptions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lblBase = New System.Windows.Forms.Label()
        Me.m_hdrRun = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lbFleet = New System.Windows.Forms.Label()
        Me.m_lblIter = New System.Windows.Forms.Label()
        Me.m_lblEffort = New System.Windows.Forms.Label()
        Me.SuspendLayout
        '
        'm_btnRunMSY
        '
        resources.ApplyResources(Me.m_btnRunMSY, "m_btnRunMSY")
        Me.m_btnRunMSY.Name = "m_btnRunMSY"
        Me.m_btnRunMSY.UseVisualStyleBackColor = true
        '
        'm_btnStop
        '
        Me.m_btnStop.DialogResult = System.Windows.Forms.DialogResult.Cancel
        resources.ApplyResources(Me.m_btnStop, "m_btnStop")
        Me.m_btnStop.Name = "m_btnStop"
        Me.m_btnStop.UseVisualStyleBackColor = true
        '
        'txtMSYresults
        '
        resources.ApplyResources(Me.txtMSYresults, "txtMSYresults")
        Me.txtMSYresults.Name = "txtMSYresults"
        '
        'm_btnFleetTradeoffs
        '
        resources.ApplyResources(Me.m_btnFleetTradeoffs, "m_btnFleetTradeoffs")
        Me.m_btnFleetTradeoffs.Name = "m_btnFleetTradeoffs"
        Me.m_btnFleetTradeoffs.UseVisualStyleBackColor = true
        '
        'm_rbValue
        '
        resources.ApplyResources(Me.m_rbValue, "m_rbValue")
        Me.m_rbValue.Name = "m_rbValue"
        Me.m_rbValue.TabStop = true
        Me.m_rbValue.UseVisualStyleBackColor = true
        '
        'rbCatch
        '
        resources.ApplyResources(Me.rbCatch, "rbCatch")
        Me.rbCatch.Name = "rbCatch"
        Me.rbCatch.TabStop = true
        Me.rbCatch.UseVisualStyleBackColor = true
        '
        'm_lblMSY
        '
        resources.ApplyResources(Me.m_lblMSY, "m_lblMSY")
        Me.m_lblMSY.Name = "m_lblMSY"
        '
        'm_hdrOptions
        '
        Me.m_hdrOptions.CanCollapseParent = false
        Me.m_hdrOptions.CollapsedParentHeight = 0
        Me.m_hdrOptions.IsCollapsed = false
        resources.ApplyResources(Me.m_hdrOptions, "m_hdrOptions")
        Me.m_hdrOptions.Name = "m_hdrOptions"
        '
        'm_lblBase
        '
        resources.ApplyResources(Me.m_lblBase, "m_lblBase")
        Me.m_lblBase.Name = "m_lblBase"
        '
        'm_hdrRun
        '
        Me.m_hdrRun.CanCollapseParent = false
        Me.m_hdrRun.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrRun, "m_hdrRun")
        Me.m_hdrRun.IsCollapsed = false
        Me.m_hdrRun.Name = "m_hdrRun"
        '
        'm_lbFleet
        '
        resources.ApplyResources(Me.m_lbFleet, "m_lbFleet")
        Me.m_lbFleet.Name = "m_lbFleet"
        '
        'm_lblIter
        '
        resources.ApplyResources(Me.m_lblIter, "m_lblIter")
        Me.m_lblIter.Name = "m_lblIter"
        '
        'm_lblEffort
        '
        resources.ApplyResources(Me.m_lblEffort, "m_lblEffort")
        Me.m_lblEffort.Name = "m_lblEffort"
        '
        'frmMSY
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.m_btnStop
        Me.Controls.Add(Me.m_lblEffort)
        Me.Controls.Add(Me.m_lblIter)
        Me.Controls.Add(Me.m_lbFleet)
        Me.Controls.Add(Me.m_lblBase)
        Me.Controls.Add(Me.m_hdrRun)
        Me.Controls.Add(Me.m_hdrOptions)
        Me.Controls.Add(Me.m_lblMSY)
        Me.Controls.Add(Me.rbCatch)
        Me.Controls.Add(Me.m_rbValue)
        Me.Controls.Add(Me.m_btnFleetTradeoffs)
        Me.Controls.Add(Me.txtMSYresults)
        Me.Controls.Add(Me.m_btnStop)
        Me.Controls.Add(Me.m_btnRunMSY)
        Me.CoreExecutionState = EwEUtils.Core.eCoreExecutionState.EcosimLoaded
        Me.Name = "frmMSY"
        Me.TabText = ""
        Me.ResumeLayout(False)
        Me.PerformLayout

End Sub
    Friend WithEvents txtMSYresults As System.Windows.Forms.TextBox
    Friend WithEvents rbCatch As System.Windows.Forms.RadioButton
    Private WithEvents m_lblMSY As System.Windows.Forms.Label
    Private WithEvents m_hdrOptions As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_rbValue As System.Windows.Forms.RadioButton
    Private WithEvents m_lblBase As System.Windows.Forms.Label
    Private WithEvents m_btnRunMSY As System.Windows.Forms.Button
    Private WithEvents m_btnStop As System.Windows.Forms.Button
    Private WithEvents m_hdrRun As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_btnFleetTradeoffs As System.Windows.Forms.Button
    Private WithEvents m_lbFleet As System.Windows.Forms.Label
    Private WithEvents m_lblIter As System.Windows.Forms.Label
    Private WithEvents m_lblEffort As System.Windows.Forms.Label
End Class
