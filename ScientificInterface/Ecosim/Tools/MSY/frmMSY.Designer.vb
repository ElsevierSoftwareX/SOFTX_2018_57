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
Imports ZedGraph

Namespace Ecosim

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
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMSY))
            Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnShowHide = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnSaveOutput = New System.Windows.Forms.ToolStripButton()
            Me.m_graph = New ZedGraph.ZedGraphControl()
            Me.m_sc = New System.Windows.Forms.SplitContainer()
            Me.m_tlp = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plRunSel = New System.Windows.Forms.Panel()
            Me.m_cmbTarget = New System.Windows.Forms.ComboBox()
            Me.m_rbFleet = New System.Windows.Forms.RadioButton()
            Me.m_rbGroup = New System.Windows.Forms.RadioButton()
            Me.m_hdrRun = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plAssessment = New System.Windows.Forms.Panel()
            Me.m_rbBoth = New System.Windows.Forms.RadioButton()
            Me.m_rbStationary = New System.Windows.Forms.RadioButton()
            Me.m_rbFull = New System.Windows.Forms.RadioButton()
            Me.m_hdrAssessment = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plData = New System.Windows.Forms.Panel()
            Me.m_hdr = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_rbBiomassAndCatch = New System.Windows.Forms.RadioButton()
            Me.m_rbValue = New System.Windows.Forms.RadioButton()
            Me.m_rbCatch = New System.Windows.Forms.RadioButton()
            Me.m_rbBiomass = New System.Windows.Forms.RadioButton()
            Me.m_plMisc = New System.Windows.Forms.Panel()
            Me.m_hdrTools = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_btnRunFMSY = New System.Windows.Forms.Button()
            Me.m_btnTest = New System.Windows.Forms.Button()
            Me.m_plRun = New System.Windows.Forms.Panel()
            Me.m_nudNumSteps = New System.Windows.Forms.NumericUpDown()
            Me.m_nudNumTrialYears = New System.Windows.Forms.NumericUpDown()
            Me.m_lblNumTrialYears = New System.Windows.Forms.Label()
            Me.m_nudMaxF = New System.Windows.Forms.NumericUpDown()
            Me.m_btnRun = New System.Windows.Forms.Button()
            Me.m_lblMaxRelF = New System.Windows.Forms.Label()
            Me.m_lblNumSteps = New System.Windows.Forms.Label()
            Me.m_ts.SuspendLayout()
            CType(Me.m_sc, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_sc.Panel1.SuspendLayout()
            Me.m_sc.Panel2.SuspendLayout()
            Me.m_sc.SuspendLayout()
            Me.m_tlp.SuspendLayout()
            Me.m_plRunSel.SuspendLayout()
            Me.m_plAssessment.SuspendLayout()
            Me.m_plData.SuspendLayout()
            Me.m_plMisc.SuspendLayout()
            Me.m_plRun.SuspendLayout()
            CType(Me.m_nudNumSteps, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudNumTrialYears, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudMaxF, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_ts
            '
            Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnShowHide, Me.m_tsbnSaveOutput})
            resources.ApplyResources(Me.m_ts, "m_ts")
            Me.m_ts.Name = "m_ts"
            Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnShowHide
            '
            Me.m_tsbnShowHide.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnShowHide, "m_tsbnShowHide")
            Me.m_tsbnShowHide.Name = "m_tsbnShowHide"
            '
            'm_tsbnSaveOutput
            '
            Me.m_tsbnSaveOutput.CheckOnClick = True
            Me.m_tsbnSaveOutput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnSaveOutput, "m_tsbnSaveOutput")
            Me.m_tsbnSaveOutput.Name = "m_tsbnSaveOutput"
            '
            'm_graph
            '
            Me.m_graph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            resources.ApplyResources(Me.m_graph, "m_graph")
            Me.m_graph.Name = "m_graph"
            Me.m_graph.ScrollGrace = 0.0R
            Me.m_graph.ScrollMaxX = 0.0R
            Me.m_graph.ScrollMaxY = 0.0R
            Me.m_graph.ScrollMaxY2 = 0.0R
            Me.m_graph.ScrollMinX = 0.0R
            Me.m_graph.ScrollMinY = 0.0R
            Me.m_graph.ScrollMinY2 = 0.0R
            '
            'm_sc
            '
            resources.ApplyResources(Me.m_sc, "m_sc")
            Me.m_sc.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
            Me.m_sc.Name = "m_sc"
            '
            'm_sc.Panel1
            '
            Me.m_sc.Panel1.Controls.Add(Me.m_tlp)
            '
            'm_sc.Panel2
            '
            Me.m_sc.Panel2.Controls.Add(Me.m_graph)
            '
            'm_tlp
            '
            resources.ApplyResources(Me.m_tlp, "m_tlp")
            Me.m_tlp.Controls.Add(Me.m_plRunSel, 0, 0)
            Me.m_tlp.Controls.Add(Me.m_plAssessment, 0, 2)
            Me.m_tlp.Controls.Add(Me.m_plData, 0, 3)
            Me.m_tlp.Controls.Add(Me.m_plMisc, 0, 4)
            Me.m_tlp.Controls.Add(Me.m_plRun, 0, 1)
            Me.m_tlp.Name = "m_tlp"
            '
            'm_plRunSel
            '
            Me.m_plRunSel.Controls.Add(Me.m_cmbTarget)
            Me.m_plRunSel.Controls.Add(Me.m_rbFleet)
            Me.m_plRunSel.Controls.Add(Me.m_rbGroup)
            Me.m_plRunSel.Controls.Add(Me.m_hdrRun)
            resources.ApplyResources(Me.m_plRunSel, "m_plRunSel")
            Me.m_plRunSel.Name = "m_plRunSel"
            '
            'm_cmbTarget
            '
            resources.ApplyResources(Me.m_cmbTarget, "m_cmbTarget")
            Me.m_cmbTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbTarget.FormattingEnabled = True
            Me.m_cmbTarget.Name = "m_cmbTarget"
            '
            'm_rbFleet
            '
            resources.ApplyResources(Me.m_rbFleet, "m_rbFleet")
            Me.m_rbFleet.Name = "m_rbFleet"
            Me.m_rbFleet.UseVisualStyleBackColor = True
            '
            'm_rbGroup
            '
            resources.ApplyResources(Me.m_rbGroup, "m_rbGroup")
            Me.m_rbGroup.Checked = True
            Me.m_rbGroup.Name = "m_rbGroup"
            Me.m_rbGroup.TabStop = True
            Me.m_rbGroup.UseVisualStyleBackColor = True
            '
            'm_hdrRun
            '
            resources.ApplyResources(Me.m_hdrRun, "m_hdrRun")
            Me.m_hdrRun.CanCollapseParent = False
            Me.m_hdrRun.CollapsedParentHeight = 0
            Me.m_hdrRun.IsCollapsed = False
            Me.m_hdrRun.Name = "m_hdrRun"
            '
            'm_plAssessment
            '
            Me.m_plAssessment.Controls.Add(Me.m_rbBoth)
            Me.m_plAssessment.Controls.Add(Me.m_rbStationary)
            Me.m_plAssessment.Controls.Add(Me.m_rbFull)
            Me.m_plAssessment.Controls.Add(Me.m_hdrAssessment)
            resources.ApplyResources(Me.m_plAssessment, "m_plAssessment")
            Me.m_plAssessment.Name = "m_plAssessment"
            '
            'm_rbBoth
            '
            resources.ApplyResources(Me.m_rbBoth, "m_rbBoth")
            Me.m_rbBoth.Checked = True
            Me.m_rbBoth.Name = "m_rbBoth"
            Me.m_rbBoth.TabStop = True
            Me.m_rbBoth.UseVisualStyleBackColor = True
            '
            'm_rbStationary
            '
            resources.ApplyResources(Me.m_rbStationary, "m_rbStationary")
            Me.m_rbStationary.Name = "m_rbStationary"
            Me.m_rbStationary.UseVisualStyleBackColor = True
            '
            'm_rbFull
            '
            resources.ApplyResources(Me.m_rbFull, "m_rbFull")
            Me.m_rbFull.Name = "m_rbFull"
            Me.m_rbFull.UseVisualStyleBackColor = True
            '
            'm_hdrAssessment
            '
            resources.ApplyResources(Me.m_hdrAssessment, "m_hdrAssessment")
            Me.m_hdrAssessment.CanCollapseParent = True
            Me.m_hdrAssessment.CollapsedParentHeight = 0
            Me.m_hdrAssessment.IsCollapsed = False
            Me.m_hdrAssessment.Name = "m_hdrAssessment"
            '
            'm_plData
            '
            Me.m_plData.Controls.Add(Me.m_hdr)
            Me.m_plData.Controls.Add(Me.m_rbBiomassAndCatch)
            Me.m_plData.Controls.Add(Me.m_rbValue)
            Me.m_plData.Controls.Add(Me.m_rbCatch)
            Me.m_plData.Controls.Add(Me.m_rbBiomass)
            resources.ApplyResources(Me.m_plData, "m_plData")
            Me.m_plData.Name = "m_plData"
            '
            'm_hdr
            '
            resources.ApplyResources(Me.m_hdr, "m_hdr")
            Me.m_hdr.CanCollapseParent = True
            Me.m_hdr.CollapsedParentHeight = 0
            Me.m_hdr.IsCollapsed = False
            Me.m_hdr.Name = "m_hdr"
            '
            'm_rbBiomassAndCatch
            '
            resources.ApplyResources(Me.m_rbBiomassAndCatch, "m_rbBiomassAndCatch")
            Me.m_rbBiomassAndCatch.Name = "m_rbBiomassAndCatch"
            Me.m_rbBiomassAndCatch.UseVisualStyleBackColor = True
            '
            'm_rbValue
            '
            resources.ApplyResources(Me.m_rbValue, "m_rbValue")
            Me.m_rbValue.Name = "m_rbValue"
            Me.m_rbValue.UseVisualStyleBackColor = True
            '
            'm_rbCatch
            '
            resources.ApplyResources(Me.m_rbCatch, "m_rbCatch")
            Me.m_rbCatch.Name = "m_rbCatch"
            Me.m_rbCatch.UseVisualStyleBackColor = True
            '
            'm_rbBiomass
            '
            resources.ApplyResources(Me.m_rbBiomass, "m_rbBiomass")
            Me.m_rbBiomass.Checked = True
            Me.m_rbBiomass.Name = "m_rbBiomass"
            Me.m_rbBiomass.TabStop = True
            Me.m_rbBiomass.UseVisualStyleBackColor = True
            '
            'm_plMisc
            '
            Me.m_plMisc.Controls.Add(Me.m_hdrTools)
            Me.m_plMisc.Controls.Add(Me.m_btnRunFMSY)
            Me.m_plMisc.Controls.Add(Me.m_btnTest)
            resources.ApplyResources(Me.m_plMisc, "m_plMisc")
            Me.m_plMisc.Name = "m_plMisc"
            '
            'm_hdrTools
            '
            resources.ApplyResources(Me.m_hdrTools, "m_hdrTools")
            Me.m_hdrTools.CanCollapseParent = False
            Me.m_hdrTools.CollapsedParentHeight = 0
            Me.m_hdrTools.IsCollapsed = False
            Me.m_hdrTools.Name = "m_hdrTools"
            '
            'm_btnRunFMSY
            '
            resources.ApplyResources(Me.m_btnRunFMSY, "m_btnRunFMSY")
            Me.m_btnRunFMSY.Name = "m_btnRunFMSY"
            Me.m_btnRunFMSY.UseVisualStyleBackColor = True
            '
            'm_btnTest
            '
            resources.ApplyResources(Me.m_btnTest, "m_btnTest")
            Me.m_btnTest.Name = "m_btnTest"
            Me.m_btnTest.UseVisualStyleBackColor = True
            '
            'm_plRun
            '
            Me.m_plRun.Controls.Add(Me.m_nudNumSteps)
            Me.m_plRun.Controls.Add(Me.m_nudNumTrialYears)
            Me.m_plRun.Controls.Add(Me.m_lblNumTrialYears)
            Me.m_plRun.Controls.Add(Me.m_nudMaxF)
            Me.m_plRun.Controls.Add(Me.m_btnRun)
            Me.m_plRun.Controls.Add(Me.m_lblMaxRelF)
            Me.m_plRun.Controls.Add(Me.m_lblNumSteps)
            resources.ApplyResources(Me.m_plRun, "m_plRun")
            Me.m_plRun.Name = "m_plRun"
            '
            'm_nudNumSteps
            '
            resources.ApplyResources(Me.m_nudNumSteps, "m_nudNumSteps")
            Me.m_nudNumSteps.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
            Me.m_nudNumSteps.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
            Me.m_nudNumSteps.Name = "m_nudNumSteps"
            Me.m_nudNumSteps.Value = New Decimal(New Integer() {30, 0, 0, 0})
            '
            'm_nudNumTrialYears
            '
            resources.ApplyResources(Me.m_nudNumTrialYears, "m_nudNumTrialYears")
            Me.m_nudNumTrialYears.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudNumTrialYears.Name = "m_nudNumTrialYears"
            Me.m_nudNumTrialYears.Value = New Decimal(New Integer() {40, 0, 0, 0})
            '
            'm_lblNumTrialYears
            '
            resources.ApplyResources(Me.m_lblNumTrialYears, "m_lblNumTrialYears")
            Me.m_lblNumTrialYears.Name = "m_lblNumTrialYears"
            '
            'm_nudMaxF
            '
            resources.ApplyResources(Me.m_nudMaxF, "m_nudMaxF")
            Me.m_nudMaxF.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
            Me.m_nudMaxF.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudMaxF.Name = "m_nudMaxF"
            Me.m_nudMaxF.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_btnRun
            '
            resources.ApplyResources(Me.m_btnRun, "m_btnRun")
            Me.m_btnRun.Name = "m_btnRun"
            Me.m_btnRun.UseVisualStyleBackColor = True
            '
            'm_lblMaxRelF
            '
            resources.ApplyResources(Me.m_lblMaxRelF, "m_lblMaxRelF")
            Me.m_lblMaxRelF.Name = "m_lblMaxRelF"
            '
            'm_lblNumSteps
            '
            resources.ApplyResources(Me.m_lblNumSteps, "m_lblNumSteps")
            Me.m_lblNumSteps.Name = "m_lblNumSteps"
            '
            'frmMSY
            '
            Me.AcceptButton = Me.m_btnRun
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.ControlBox = False
            Me.Controls.Add(Me.m_sc)
            Me.Controls.Add(Me.m_ts)
            Me.Name = "frmMSY"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.TabText = ""
            Me.m_ts.ResumeLayout(False)
            Me.m_ts.PerformLayout()
            Me.m_sc.Panel1.ResumeLayout(False)
            Me.m_sc.Panel2.ResumeLayout(False)
            CType(Me.m_sc, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_sc.ResumeLayout(False)
            Me.m_tlp.ResumeLayout(False)
            Me.m_plRunSel.ResumeLayout(False)
            Me.m_plRunSel.PerformLayout()
            Me.m_plAssessment.ResumeLayout(False)
            Me.m_plAssessment.PerformLayout()
            Me.m_plData.ResumeLayout(False)
            Me.m_plData.PerformLayout()
            Me.m_plMisc.ResumeLayout(False)
            Me.m_plRun.ResumeLayout(False)
            Me.m_plRun.PerformLayout()
            CType(Me.m_nudNumSteps, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudNumTrialYears, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudMaxF, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_ts As cEwEToolstrip
        Private WithEvents m_tsbnShowHide As System.Windows.Forms.ToolStripButton
        Private WithEvents m_graph As ZedGraphControl
        Private WithEvents m_tsbnSaveOutput As System.Windows.Forms.ToolStripButton
        Private WithEvents m_sc As System.Windows.Forms.SplitContainer
        Private WithEvents m_tlp As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_plData As System.Windows.Forms.Panel
        Private WithEvents m_hdr As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_rbBiomass As System.Windows.Forms.RadioButton
        Private WithEvents m_plAssessment As System.Windows.Forms.Panel
        Private WithEvents m_btnRun As System.Windows.Forms.Button
        Private WithEvents m_plRunSel As System.Windows.Forms.Panel
        Private WithEvents m_hdrRun As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_cmbTarget As System.Windows.Forms.ComboBox
        Private WithEvents m_rbFleet As System.Windows.Forms.RadioButton
        Private WithEvents m_rbGroup As System.Windows.Forms.RadioButton
        Private WithEvents m_hdrAssessment As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_rbValue As System.Windows.Forms.RadioButton
        Private WithEvents m_rbBoth As System.Windows.Forms.RadioButton
        Private WithEvents m_rbStationary As System.Windows.Forms.RadioButton
        Private WithEvents m_rbFull As System.Windows.Forms.RadioButton
        Private WithEvents m_nudMaxF As System.Windows.Forms.NumericUpDown
        Private WithEvents m_lblNumSteps As System.Windows.Forms.Label
        Private WithEvents m_rbCatch As System.Windows.Forms.RadioButton
        Private WithEvents m_plMisc As System.Windows.Forms.Panel
        Private WithEvents m_btnTest As System.Windows.Forms.Button
        Private WithEvents m_rbBiomassAndCatch As System.Windows.Forms.RadioButton
        Private WithEvents m_hdrTools As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_btnRunFMSY As System.Windows.Forms.Button
        Private WithEvents m_nudNumTrialYears As System.Windows.Forms.NumericUpDown
        Private WithEvents m_lblNumTrialYears As System.Windows.Forms.Label
        Private WithEvents m_lblMaxRelF As System.Windows.Forms.Label
        Private WithEvents m_plRun As System.Windows.Forms.Panel
        Private WithEvents m_nudNumSteps As System.Windows.Forms.NumericUpDown
    End Class

End Namespace
