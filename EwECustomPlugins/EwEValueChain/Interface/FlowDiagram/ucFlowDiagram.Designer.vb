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

Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources

Partial Class ucFlowDiagram
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucFlowDiagram))
        Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tsbnSaveImage = New System.Windows.Forms.ToolStripButton()
        Me.m_sep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tslLayout = New System.Windows.Forms.ToolStripLabel()
        Me.m_tsbnSaveLayout = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnLoadLayout = New System.Windows.Forms.ToolStripButton()
        Me.m_sep2 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tslData = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscbmValue = New System.Windows.Forms.ToolStripComboBox()
        Me.m_tsbnOptions = New System.Windows.Forms.ToolStripButton()
        Me.m_scFD = New System.Windows.Forms.SplitContainer()
        Me.m_pbFlowDiagram = New ScientificInterfaceShared.Controls.ucSmoothPanel()
        Me.m_pgFD = New System.Windows.Forms.PropertyGrid()
        Me.m_tsMain.SuspendLayout()
        CType(Me.m_scFD, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scFD.Panel1.SuspendLayout()
        Me.m_scFD.Panel2.SuspendLayout()
        Me.m_scFD.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_tsMain
        '
        Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnSaveImage, Me.m_sep1, Me.m_tslLayout, Me.m_tsbnSaveLayout, Me.m_tsbnLoadLayout, Me.m_sep2, Me.m_tslData, Me.m_tscbmValue, Me.m_tsbnOptions})
        resources.ApplyResources(Me.m_tsMain, "m_tsMain")
        Me.m_tsMain.Name = "m_tsMain"
        Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tsbnSaveImage
        '
        Me.m_tsbnSaveImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnSaveImage, "m_tsbnSaveImage")
        Me.m_tsbnSaveImage.Name = "m_tsbnSaveImage"
        '
        'm_sep1
        '
        Me.m_sep1.Name = "m_sep1"
        resources.ApplyResources(Me.m_sep1, "m_sep1")
        '
        'm_tslLayout
        '
        Me.m_tslLayout.Name = "m_tslLayout"
        resources.ApplyResources(Me.m_tslLayout, "m_tslLayout")
        '
        'm_tsbnSaveLayout
        '
        Me.m_tsbnSaveLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnSaveLayout, "m_tsbnSaveLayout")
        Me.m_tsbnSaveLayout.Name = "m_tsbnSaveLayout"
        '
        'm_tsbnLoadLayout
        '
        Me.m_tsbnLoadLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnLoadLayout, "m_tsbnLoadLayout")
        Me.m_tsbnLoadLayout.Name = "m_tsbnLoadLayout"
        '
        'm_sep2
        '
        Me.m_sep2.Name = "m_sep2"
        resources.ApplyResources(Me.m_sep2, "m_sep2")
        '
        'm_tslData
        '
        Me.m_tslData.Name = "m_tslData"
        resources.ApplyResources(Me.m_tslData, "m_tslData")
        '
        'm_tscbmValue
        '
        Me.m_tscbmValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscbmValue.Name = "m_tscbmValue"
        resources.ApplyResources(Me.m_tscbmValue, "m_tscbmValue")
        '
        'm_tsbnOptions
        '
        Me.m_tsbnOptions.CheckOnClick = True
        Me.m_tsbnOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnOptions, "m_tsbnOptions")
        Me.m_tsbnOptions.Name = "m_tsbnOptions"
        '
        'm_scFD
        '
        Me.m_scFD.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        resources.ApplyResources(Me.m_scFD, "m_scFD")
        Me.m_scFD.Name = "m_scFD"
        '
        'm_scFD.Panel1
        '
        Me.m_scFD.Panel1.Controls.Add(Me.m_pbFlowDiagram)
        '
        'm_scFD.Panel2
        '
        Me.m_scFD.Panel2.Controls.Add(Me.m_pgFD)
        Me.m_scFD.Panel2Collapsed = True
        '
        'm_pbFlowDiagram
        '
        resources.ApplyResources(Me.m_pbFlowDiagram, "m_pbFlowDiagram")
        Me.m_pbFlowDiagram.Name = "m_pbFlowDiagram"
        '
        'm_pgFD
        '
        resources.ApplyResources(Me.m_pgFD, "m_pgFD")
        Me.m_pgFD.Name = "m_pgFD"
        '
        'ucFlowDiagram
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_scFD)
        Me.Controls.Add(Me.m_tsMain)
        Me.Name = "ucFlowDiagram"
        Me.m_tsMain.ResumeLayout(False)
        Me.m_tsMain.PerformLayout()
        Me.m_scFD.Panel1.ResumeLayout(False)
        Me.m_scFD.Panel2.ResumeLayout(False)
        CType(Me.m_scFD, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scFD.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private WithEvents m_tsMain As cEwEToolstrip
    Private WithEvents m_tslData As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tscbmValue As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_scFD As System.Windows.Forms.SplitContainer
    Private WithEvents m_pgFD As System.Windows.Forms.PropertyGrid
    Private WithEvents m_tsbnOptions As System.Windows.Forms.ToolStripButton
    Private WithEvents m_pbFlowDiagram As ucSmoothPanel
    Private WithEvents m_tsbnSaveLayout As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnSaveImage As System.Windows.Forms.ToolStripButton
    Friend WithEvents m_tsbnLoadLayout As System.Windows.Forms.ToolStripButton
    Private WithEvents m_sep1 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_sep2 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tslLayout As System.Windows.Forms.ToolStripLabel

End Class
