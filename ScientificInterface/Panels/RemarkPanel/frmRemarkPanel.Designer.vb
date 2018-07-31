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

Partial Class frmRemarkPanel
    Inherits frmEwEDockContent

    Private components As System.ComponentModel.IContainer

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmRemarkPanel))
        Me.m_tsRemarks = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tsbnInfo = New System.Windows.Forms.ToolStripButton()
        Me.m_lblVarName = New System.Windows.Forms.ToolStripLabel()
        Me.m_tbxRemark = New System.Windows.Forms.TextBox()
        Me.m_scMain = New System.Windows.Forms.SplitContainer()
        Me.m_tlpInfo = New System.Windows.Forms.TableLayoutPanel()
        Me.m_lblDomain = New System.Windows.Forms.Label()
        Me.m_lblDescription = New System.Windows.Forms.Label()
        Me.m_lblStatus = New System.Windows.Forms.Label()
        Me.m_hdrInfo = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tlpRemarks = New System.Windows.Forms.TableLayoutPanel()
        Me.m_btnApply = New System.Windows.Forms.Button()
        Me.m_tsRemarks.SuspendLayout()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scMain.Panel1.SuspendLayout()
        Me.m_scMain.Panel2.SuspendLayout()
        Me.m_scMain.SuspendLayout()
        Me.m_tlpInfo.SuspendLayout()
        Me.m_tlpRemarks.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_tsRemarks
        '
        Me.m_tsRemarks.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsRemarks.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnInfo, Me.m_lblVarName})
        resources.ApplyResources(Me.m_tsRemarks, "m_tsRemarks")
        Me.m_tsRemarks.Name = "m_tsRemarks"
        Me.m_tsRemarks.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tsbnInfo
        '
        Me.m_tsbnInfo.CheckOnClick = True
        Me.m_tsbnInfo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnInfo, "m_tsbnInfo")
        Me.m_tsbnInfo.Name = "m_tsbnInfo"
        '
        'm_lblVarName
        '
        Me.m_lblVarName.Name = "m_lblVarName"
        resources.ApplyResources(Me.m_lblVarName, "m_lblVarName")
        '
        'm_tbxRemark
        '
        Me.m_tbxRemark.AcceptsReturn = True
        resources.ApplyResources(Me.m_tbxRemark, "m_tbxRemark")
        Me.m_tbxRemark.Name = "m_tbxRemark"
        '
        'm_scMain
        '
        resources.ApplyResources(Me.m_scMain, "m_scMain")
        Me.m_scMain.Name = "m_scMain"
        '
        'm_scMain.Panel1
        '
        Me.m_scMain.Panel1.Controls.Add(Me.m_tlpInfo)
        '
        'm_scMain.Panel2
        '
        Me.m_scMain.Panel2.Controls.Add(Me.m_tlpRemarks)
        Me.m_scMain.TabStop = False
        '
        'm_tlpInfo
        '
        resources.ApplyResources(Me.m_tlpInfo, "m_tlpInfo")
        Me.m_tlpInfo.Controls.Add(Me.m_lblDomain, 0, 1)
        Me.m_tlpInfo.Controls.Add(Me.m_lblDescription, 0, 2)
        Me.m_tlpInfo.Controls.Add(Me.m_lblStatus, 0, 3)
        Me.m_tlpInfo.Controls.Add(Me.m_hdrInfo, 0, 0)
        Me.m_tlpInfo.Name = "m_tlpInfo"
        '
        'm_lblDomain
        '
        resources.ApplyResources(Me.m_lblDomain, "m_lblDomain")
        Me.m_lblDomain.Name = "m_lblDomain"
        '
        'm_lblDescription
        '
        resources.ApplyResources(Me.m_lblDescription, "m_lblDescription")
        Me.m_lblDescription.Name = "m_lblDescription"
        '
        'm_lblStatus
        '
        resources.ApplyResources(Me.m_lblStatus, "m_lblStatus")
        Me.m_lblStatus.Name = "m_lblStatus"
        '
        'm_hdrInfo
        '
        Me.m_hdrInfo.CanCollapseParent = False
        Me.m_hdrInfo.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrInfo, "m_hdrInfo")
        Me.m_hdrInfo.IsCollapsed = False
        Me.m_hdrInfo.Name = "m_hdrInfo"
        '
        'm_tlpRemarks
        '
        resources.ApplyResources(Me.m_tlpRemarks, "m_tlpRemarks")
        Me.m_tlpRemarks.Controls.Add(Me.m_tbxRemark, 0, 0)
        Me.m_tlpRemarks.Controls.Add(Me.m_btnApply, 1, 0)
        Me.m_tlpRemarks.Name = "m_tlpRemarks"
        '
        'm_btnApply
        '
        resources.ApplyResources(Me.m_btnApply, "m_btnApply")
        Me.m_btnApply.Name = "m_btnApply"
        Me.m_btnApply.UseVisualStyleBackColor = True
        '
        'frmRemarkPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CloseButtonVisible = False
        Me.Controls.Add(Me.m_scMain)
        Me.Controls.Add(Me.m_tsRemarks)
        Me.DockAreas = CType((((WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) _
            Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) _
            Or WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom), WeifenLuo.WinFormsUI.Docking.DockAreas)
        Me.HideOnClose = True
        Me.Name = "frmRemarkPanel"
        Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRightAutoHide
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.m_tsRemarks.ResumeLayout(False)
        Me.m_tsRemarks.PerformLayout()
        Me.m_scMain.Panel1.ResumeLayout(False)
        Me.m_scMain.Panel2.ResumeLayout(False)
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scMain.ResumeLayout(False)
        Me.m_tlpInfo.ResumeLayout(False)
        Me.m_tlpInfo.PerformLayout()
        Me.m_tlpRemarks.ResumeLayout(False)
        Me.m_tlpRemarks.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_tsRemarks As cEwEToolstrip
    Private WithEvents m_tbxRemark As System.Windows.Forms.TextBox
    Private WithEvents m_tlpInfo As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents m_lblStatus As System.Windows.Forms.Label
    Private WithEvents m_lblVarName As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
    Private WithEvents m_lblDomain As System.Windows.Forms.Label
    Private WithEvents m_lblDescription As System.Windows.Forms.Label
    Private WithEvents m_tsbnInfo As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tlpRemarks As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_btnApply As System.Windows.Forms.Button
    Private WithEvents m_hdrInfo As cEwEHeaderLabel
End Class
