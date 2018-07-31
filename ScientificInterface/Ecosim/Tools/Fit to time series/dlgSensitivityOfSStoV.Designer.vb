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

Partial Class dlgSensitivityOfSStoV
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgSensitivityOfSStoV))
        Me.m_progress = New System.Windows.Forms.ProgressBar()
        Me.m_btnSearch = New System.Windows.Forms.Button()
        Me.m_rbSearchPredPrey = New System.Windows.Forms.RadioButton()
        Me.m_rbSearchPred = New System.Windows.Forms.RadioButton()
        Me.m_lblNumCategories = New System.Windows.Forms.Label()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_btnOk = New System.Windows.Forms.Button()
        Me.m_btnSaveToCSV = New System.Windows.Forms.Button()
        Me.m_nudNumBlocks = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_hdrTransfer = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrSearch = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_legend = New ScientificInterfaceShared.Controls.ucLegendBar()
        Me.m_ucVulBlocks = New ScientificInterface.Ecosim.ucVulnerabiltyBlocks()
        CType(Me.m_nudNumBlocks, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_progress
        '
        resources.ApplyResources(Me.m_progress, "m_progress")
        Me.m_progress.Name = "m_progress"
        Me.m_progress.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        '
        'm_btnSearch
        '
        resources.ApplyResources(Me.m_btnSearch, "m_btnSearch")
        Me.m_btnSearch.Name = "m_btnSearch"
        Me.m_btnSearch.UseVisualStyleBackColor = True
        '
        'm_rbSearchPredPrey
        '
        resources.ApplyResources(Me.m_rbSearchPredPrey, "m_rbSearchPredPrey")
        Me.m_rbSearchPredPrey.Name = "m_rbSearchPredPrey"
        Me.m_rbSearchPredPrey.UseVisualStyleBackColor = True
        '
        'm_rbSearchPred
        '
        resources.ApplyResources(Me.m_rbSearchPred, "m_rbSearchPred")
        Me.m_rbSearchPred.Checked = True
        Me.m_rbSearchPred.Name = "m_rbSearchPred"
        Me.m_rbSearchPred.TabStop = True
        Me.m_rbSearchPred.UseVisualStyleBackColor = True
        '
        'm_lblNumCategories
        '
        resources.ApplyResources(Me.m_lblNumCategories, "m_lblNumCategories")
        Me.m_lblNumCategories.Name = "m_lblNumCategories"
        '
        'm_btnCancel
        '
        resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
        Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'm_btnOk
        '
        resources.ApplyResources(Me.m_btnOk, "m_btnOk")
        Me.m_btnOk.Name = "m_btnOk"
        Me.m_btnOk.UseVisualStyleBackColor = True
        '
        'm_btnSaveToCSV
        '
        resources.ApplyResources(Me.m_btnSaveToCSV, "m_btnSaveToCSV")
        Me.m_btnSaveToCSV.Name = "m_btnSaveToCSV"
        Me.m_btnSaveToCSV.UseVisualStyleBackColor = True
        '
        'm_nudNumBlocks
        '
        Me.m_nudNumBlocks.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        resources.ApplyResources(Me.m_nudNumBlocks, "m_nudNumBlocks")
        Me.m_nudNumBlocks.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudNumBlocks.Name = "m_nudNumBlocks"
        Me.m_nudNumBlocks.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_hdrTransfer
        '
        Me.m_hdrTransfer.CanCollapseParent = False
        Me.m_hdrTransfer.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrTransfer, "m_hdrTransfer")
        Me.m_hdrTransfer.IsCollapsed = False
        Me.m_hdrTransfer.Name = "m_hdrTransfer"
        '
        'm_hdrSearch
        '
        Me.m_hdrSearch.CanCollapseParent = False
        Me.m_hdrSearch.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrSearch, "m_hdrSearch")
        Me.m_hdrSearch.IsCollapsed = False
        Me.m_hdrSearch.Name = "m_hdrSearch"
        '
        'm_legend
        '
        resources.ApplyResources(Me.m_legend, "m_legend")
        Me.m_legend.BarWidthPercentage = 60
        Me.m_legend.ColorHigh = System.Drawing.SystemColors.ControlText
        Me.m_legend.ColorLow = System.Drawing.SystemColors.ControlText
        Me.m_legend.Colors = CType(resources.GetObject("m_legend.Colors"), System.Collections.Generic.List(Of System.Drawing.Color))
        Me.m_legend.LabelHigh = "Most"
        Me.m_legend.LabelLow = "Least"
        Me.m_legend.Name = "m_legend"
        Me.m_legend.UIContext = Nothing
        '
        'm_ucVulBlocks
        '
        resources.ApplyResources(Me.m_ucVulBlocks, "m_ucVulBlocks")
        Me.m_ucVulBlocks.BlockColors = Nothing
        Me.m_ucVulBlocks.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.m_ucVulBlocks.Name = "m_ucVulBlocks"
        Me.m_ucVulBlocks.SelectedBlockNum = 0
        Me.m_ucVulBlocks.UIContext = Nothing
        '
        'dlgSensitivityOfSStoV
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_legend)
        Me.Controls.Add(Me.m_btnSaveToCSV)
        Me.Controls.Add(Me.m_nudNumBlocks)
        Me.Controls.Add(Me.m_lblNumCategories)
        Me.Controls.Add(Me.m_btnSearch)
        Me.Controls.Add(Me.m_rbSearchPredPrey)
        Me.Controls.Add(Me.m_hdrTransfer)
        Me.Controls.Add(Me.m_rbSearchPred)
        Me.Controls.Add(Me.m_hdrSearch)
        Me.Controls.Add(Me.m_progress)
        Me.Controls.Add(Me.m_ucVulBlocks)
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_btnOk)
        Me.MinimizeBox = False
        Me.Name = "dlgSensitivityOfSStoV"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        CType(Me.m_nudNumBlocks, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents m_rbSearchPredPrey As System.Windows.Forms.RadioButton
    Friend WithEvents m_rbSearchPred As System.Windows.Forms.RadioButton
    Friend WithEvents m_btnCancel As System.Windows.Forms.Button
    Friend WithEvents m_btnOk As System.Windows.Forms.Button
    Private WithEvents m_hdrSearch As cEwEHeaderLabel
    Private WithEvents m_hdrTransfer As cEwEHeaderLabel
    Private WithEvents m_ucVulBlocks As ScientificInterface.Ecosim.ucVulnerabiltyBlocks
    Private WithEvents m_btnSearch As System.Windows.Forms.Button
    Private WithEvents m_progress As System.Windows.Forms.ProgressBar
    Private WithEvents m_lblNumCategories As System.Windows.Forms.Label
    Friend WithEvents m_nudNumBlocks As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_btnSaveToCSV As System.Windows.Forms.Button
    Private WithEvents m_legend As ScientificInterfaceShared.Controls.ucLegendBar
End Class
