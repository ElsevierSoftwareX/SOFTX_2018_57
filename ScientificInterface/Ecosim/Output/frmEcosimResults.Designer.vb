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
Imports ScientificInterfaceShared.Controls

Namespace Ecosim

    Partial Class frmEcosimResults
        Inherits frmEwE

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcosimResults))
            Me.m_lblNumTimeSteps = New System.Windows.Forms.Label()
            Me.udNumTimeSteps = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudSumEnd = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudSumStart = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblBegin = New System.Windows.Forms.Label()
            Me.m_lblEnd = New System.Windows.Forms.Label()
            Me.m_cmbFleets = New System.Windows.Forms.ComboBox()
            Me.m_rbGroup = New System.Windows.Forms.RadioButton()
            Me.m_rbIndices = New System.Windows.Forms.RadioButton()
            Me.m_rbGear = New System.Windows.Forms.RadioButton()
            Me.m_plResultsGrid = New System.Windows.Forms.Panel()
            Me.m_hdrSummary = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrShow = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tlpControls = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plContent = New System.Windows.Forms.Panel()
            Me.m_plSummary = New System.Windows.Forms.Panel()
            CType(Me.udNumTimeSteps, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudSumEnd, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudSumStart, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tlpControls.SuspendLayout()
            Me.m_plContent.SuspendLayout()
            Me.m_plSummary.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_lblNumTimeSteps
            '
            resources.ApplyResources(Me.m_lblNumTimeSteps, "m_lblNumTimeSteps")
            Me.m_lblNumTimeSteps.Name = "m_lblNumTimeSteps"
            '
            'udNumTimeSteps
            '
            resources.ApplyResources(Me.udNumTimeSteps, "udNumTimeSteps")
            Me.udNumTimeSteps.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.udNumTimeSteps.Name = "udNumTimeSteps"
            '
            'm_nudSumEnd
            '
            resources.ApplyResources(Me.m_nudSumEnd, "m_nudSumEnd")
            Me.m_nudSumEnd.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudSumEnd.Name = "m_nudSumEnd"
            '
            'm_nudSumStart
            '
            resources.ApplyResources(Me.m_nudSumStart, "m_nudSumStart")
            Me.m_nudSumStart.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudSumStart.Name = "m_nudSumStart"
            '
            'm_lblBegin
            '
            resources.ApplyResources(Me.m_lblBegin, "m_lblBegin")
            Me.m_lblBegin.Name = "m_lblBegin"
            '
            'm_lblEnd
            '
            resources.ApplyResources(Me.m_lblEnd, "m_lblEnd")
            Me.m_lblEnd.Name = "m_lblEnd"
            '
            'm_cmbFleets
            '
            Me.m_cmbFleets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbFleets.FormattingEnabled = True
            resources.ApplyResources(Me.m_cmbFleets, "m_cmbFleets")
            Me.m_cmbFleets.Name = "m_cmbFleets"
            '
            'm_rbGroup
            '
            resources.ApplyResources(Me.m_rbGroup, "m_rbGroup")
            Me.m_rbGroup.Name = "m_rbGroup"
            Me.m_rbGroup.TabStop = True
            Me.m_rbGroup.UseVisualStyleBackColor = True
            '
            'm_rbIndices
            '
            resources.ApplyResources(Me.m_rbIndices, "m_rbIndices")
            Me.m_rbIndices.Name = "m_rbIndices"
            Me.m_rbIndices.TabStop = True
            Me.m_rbIndices.UseVisualStyleBackColor = True
            '
            'm_rbGear
            '
            resources.ApplyResources(Me.m_rbGear, "m_rbGear")
            Me.m_rbGear.Name = "m_rbGear"
            Me.m_rbGear.TabStop = True
            Me.m_rbGear.UseVisualStyleBackColor = True
            '
            'm_plResultsGrid
            '
            resources.ApplyResources(Me.m_plResultsGrid, "m_plResultsGrid")
            Me.m_plResultsGrid.Name = "m_plResultsGrid"
            '
            'm_hdrSummary
            '
            resources.ApplyResources(Me.m_hdrSummary, "m_hdrSummary")
            Me.m_hdrSummary.CanCollapseParent = False
            Me.m_hdrSummary.CollapsedParentHeight = 0
            Me.m_hdrSummary.IsCollapsed = False
            Me.m_hdrSummary.Name = "m_hdrSummary"
            '
            'm_hdrShow
            '
            resources.ApplyResources(Me.m_hdrShow, "m_hdrShow")
            Me.m_hdrShow.CanCollapseParent = False
            Me.m_hdrShow.CollapsedParentHeight = 0
            Me.m_hdrShow.IsCollapsed = False
            Me.m_hdrShow.Name = "m_hdrShow"
            '
            'm_tlpControls
            '
            resources.ApplyResources(Me.m_tlpControls, "m_tlpControls")
            Me.m_tlpControls.Controls.Add(Me.m_plContent, 1, 1)
            Me.m_tlpControls.Controls.Add(Me.m_plSummary, 0, 1)
            Me.m_tlpControls.Name = "m_tlpControls"
            '
            'm_plContent
            '
            Me.m_plContent.Controls.Add(Me.m_hdrShow)
            Me.m_plContent.Controls.Add(Me.m_rbGear)
            Me.m_plContent.Controls.Add(Me.m_rbIndices)
            Me.m_plContent.Controls.Add(Me.m_cmbFleets)
            Me.m_plContent.Controls.Add(Me.m_rbGroup)
            resources.ApplyResources(Me.m_plContent, "m_plContent")
            Me.m_plContent.Name = "m_plContent"
            '
            'm_plSummary
            '
            Me.m_plSummary.Controls.Add(Me.udNumTimeSteps)
            Me.m_plSummary.Controls.Add(Me.m_hdrSummary)
            Me.m_plSummary.Controls.Add(Me.m_nudSumStart)
            Me.m_plSummary.Controls.Add(Me.m_lblNumTimeSteps)
            Me.m_plSummary.Controls.Add(Me.m_lblBegin)
            Me.m_plSummary.Controls.Add(Me.m_lblEnd)
            Me.m_plSummary.Controls.Add(Me.m_nudSumEnd)
            resources.ApplyResources(Me.m_plSummary, "m_plSummary")
            Me.m_plSummary.Name = "m_plSummary"
            '
            'frmEcosimResults
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_tlpControls)
            Me.Controls.Add(Me.m_plResultsGrid)
            Me.Name = "frmEcosimResults"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.TabText = ""
            CType(Me.udNumTimeSteps, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudSumEnd, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudSumStart, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tlpControls.ResumeLayout(False)
            Me.m_plContent.ResumeLayout(False)
            Me.m_plContent.PerformLayout()
            Me.m_plSummary.ResumeLayout(False)
            Me.m_plSummary.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_lblBegin As System.Windows.Forms.Label
        Private WithEvents m_lblEnd As System.Windows.Forms.Label
        Private WithEvents m_rbGroup As System.Windows.Forms.RadioButton
        Private WithEvents m_rbIndices As System.Windows.Forms.RadioButton
        Private WithEvents m_rbGear As System.Windows.Forms.RadioButton
        Private WithEvents m_lblNumTimeSteps As System.Windows.Forms.Label
        Private WithEvents m_cmbFleets As System.Windows.Forms.ComboBox
        Private WithEvents m_hdrSummary As cEwEHeaderLabel
        Private WithEvents m_hdrShow As cEwEHeaderLabel
        Protected WithEvents m_plResultsGrid As System.Windows.Forms.Panel
        Private WithEvents m_tlpControls As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents m_plSummary As System.Windows.Forms.Panel
        Private WithEvents m_plContent As System.Windows.Forms.Panel
        Private WithEvents m_nudSumEnd As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudSumStart As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents udNumTimeSteps As ScientificInterfaceShared.Controls.cEwENumericUpDown
    End Class

End Namespace

