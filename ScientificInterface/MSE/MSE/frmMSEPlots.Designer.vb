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

Partial Class frmMSEPlots
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMSEPlots))
        Me.m_plPlot = New System.Windows.Forms.Panel()
        Me.m_rbTotFleetValue = New System.Windows.Forms.RadioButton()
        Me.m_rbFComparison = New System.Windows.Forms.RadioButton()
        Me.m_rbBioEst = New System.Windows.Forms.RadioButton()
        Me.m_btnShowHide = New System.Windows.Forms.Button()
        Me.m_hdrPlots = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_rbEffort = New System.Windows.Forms.RadioButton()
        Me.m_rbFleetValue = New System.Windows.Forms.RadioButton()
        Me.m_rbGroupCatch = New System.Windows.Forms.RadioButton()
        Me.m_rbGroupBiomass = New System.Windows.Forms.RadioButton()
        Me.m_plPlotType = New System.Windows.Forms.Panel()
        Me.m_hdrType = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_rbValues = New System.Windows.Forms.RadioButton()
        Me.m_rbHisto = New System.Windows.Forms.RadioButton()
        Me.pnlGraph = New System.Windows.Forms.Panel()
        Me.m_graph = New ZedGraph.ZedGraphControl()
        Me.m_plPlot.SuspendLayout()
        Me.m_plPlotType.SuspendLayout()
        Me.pnlGraph.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_plPlot
        '
        resources.ApplyResources(Me.m_plPlot, "m_plPlot")
        Me.m_plPlot.Controls.Add(Me.m_rbTotFleetValue)
        Me.m_plPlot.Controls.Add(Me.m_rbFComparison)
        Me.m_plPlot.Controls.Add(Me.m_rbBioEst)
        Me.m_plPlot.Controls.Add(Me.m_btnShowHide)
        Me.m_plPlot.Controls.Add(Me.m_hdrPlots)
        Me.m_plPlot.Controls.Add(Me.m_rbEffort)
        Me.m_plPlot.Controls.Add(Me.m_rbFleetValue)
        Me.m_plPlot.Controls.Add(Me.m_rbGroupCatch)
        Me.m_plPlot.Controls.Add(Me.m_rbGroupBiomass)
        Me.m_plPlot.Name = "m_plPlot"
        '
        'm_rbTotFleetValue
        '
        resources.ApplyResources(Me.m_rbTotFleetValue, "m_rbTotFleetValue")
        Me.m_rbTotFleetValue.Name = "m_rbTotFleetValue"
        Me.m_rbTotFleetValue.UseVisualStyleBackColor = True
        '
        'm_rbFComparison
        '
        resources.ApplyResources(Me.m_rbFComparison, "m_rbFComparison")
        Me.m_rbFComparison.Name = "m_rbFComparison"
        Me.m_rbFComparison.TabStop = True
        Me.m_rbFComparison.UseVisualStyleBackColor = True
        '
        'm_rbBioEst
        '
        resources.ApplyResources(Me.m_rbBioEst, "m_rbBioEst")
        Me.m_rbBioEst.Name = "m_rbBioEst"
        Me.m_rbBioEst.TabStop = True
        Me.m_rbBioEst.UseVisualStyleBackColor = True
        '
        'm_btnShowHide
        '
        resources.ApplyResources(Me.m_btnShowHide, "m_btnShowHide")
        Me.m_btnShowHide.Name = "m_btnShowHide"
        Me.m_btnShowHide.UseVisualStyleBackColor = True
        '
        'm_hdrPlots
        '
        resources.ApplyResources(Me.m_hdrPlots, "m_hdrPlots")
        Me.m_hdrPlots.CanCollapseParent = False
        Me.m_hdrPlots.CollapsedParentHeight = 0
        Me.m_hdrPlots.IsCollapsed = False
        Me.m_hdrPlots.Name = "m_hdrPlots"
        '
        'm_rbEffort
        '
        resources.ApplyResources(Me.m_rbEffort, "m_rbEffort")
        Me.m_rbEffort.Name = "m_rbEffort"
        Me.m_rbEffort.UseVisualStyleBackColor = True
        '
        'm_rbFleetValue
        '
        resources.ApplyResources(Me.m_rbFleetValue, "m_rbFleetValue")
        Me.m_rbFleetValue.Name = "m_rbFleetValue"
        Me.m_rbFleetValue.UseVisualStyleBackColor = True
        '
        'm_rbGroupCatch
        '
        resources.ApplyResources(Me.m_rbGroupCatch, "m_rbGroupCatch")
        Me.m_rbGroupCatch.Name = "m_rbGroupCatch"
        Me.m_rbGroupCatch.UseVisualStyleBackColor = True
        '
        'm_rbGroupBiomass
        '
        resources.ApplyResources(Me.m_rbGroupBiomass, "m_rbGroupBiomass")
        Me.m_rbGroupBiomass.Checked = True
        Me.m_rbGroupBiomass.Name = "m_rbGroupBiomass"
        Me.m_rbGroupBiomass.TabStop = True
        Me.m_rbGroupBiomass.UseVisualStyleBackColor = True
        '
        'm_plPlotType
        '
        Me.m_plPlotType.Controls.Add(Me.m_hdrType)
        Me.m_plPlotType.Controls.Add(Me.m_rbValues)
        Me.m_plPlotType.Controls.Add(Me.m_rbHisto)
        resources.ApplyResources(Me.m_plPlotType, "m_plPlotType")
        Me.m_plPlotType.Name = "m_plPlotType"
        '
        'm_hdrType
        '
        Me.m_hdrType.CanCollapseParent = False
        Me.m_hdrType.CollapsedParentHeight = 0
        Me.m_hdrType.IsCollapsed = False
        resources.ApplyResources(Me.m_hdrType, "m_hdrType")
        Me.m_hdrType.Name = "m_hdrType"
        '
        'm_rbValues
        '
        resources.ApplyResources(Me.m_rbValues, "m_rbValues")
        Me.m_rbValues.Name = "m_rbValues"
        Me.m_rbValues.Tag = ""
        Me.m_rbValues.UseVisualStyleBackColor = True
        '
        'm_rbHisto
        '
        resources.ApplyResources(Me.m_rbHisto, "m_rbHisto")
        Me.m_rbHisto.Checked = True
        Me.m_rbHisto.Name = "m_rbHisto"
        Me.m_rbHisto.TabStop = True
        Me.m_rbHisto.Tag = ""
        Me.m_rbHisto.UseVisualStyleBackColor = True
        '
        'pnlGraph
        '
        resources.ApplyResources(Me.pnlGraph, "pnlGraph")
        Me.pnlGraph.Controls.Add(Me.m_graph)
        Me.pnlGraph.Name = "pnlGraph"
        '
        'm_graph
        '
        resources.ApplyResources(Me.m_graph, "m_graph")
        Me.m_graph.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.m_graph.Name = "m_graph"
        Me.m_graph.ScrollGrace = 0.0R
        Me.m_graph.ScrollMaxX = 0.0R
        Me.m_graph.ScrollMaxY = 0.0R
        Me.m_graph.ScrollMaxY2 = 0.0R
        Me.m_graph.ScrollMinX = 0.0R
        Me.m_graph.ScrollMinY = 0.0R
        Me.m_graph.ScrollMinY2 = 0.0R
        '
        'frmMSEPlots
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.pnlGraph)
        Me.Controls.Add(Me.m_plPlotType)
        Me.Controls.Add(Me.m_plPlot)
        Me.Name = "frmMSEPlots"
        Me.TabText = ""
        Me.m_plPlot.ResumeLayout(False)
        Me.m_plPlot.PerformLayout()
        Me.m_plPlotType.ResumeLayout(False)
        Me.m_plPlotType.PerformLayout()
        Me.pnlGraph.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_plPlot As System.Windows.Forms.Panel
    Private WithEvents m_rbEffort As System.Windows.Forms.RadioButton
    Private WithEvents m_rbFleetValue As System.Windows.Forms.RadioButton
    Private WithEvents m_rbGroupCatch As System.Windows.Forms.RadioButton
    Private WithEvents m_rbGroupBiomass As System.Windows.Forms.RadioButton
    Private WithEvents m_plPlotType As System.Windows.Forms.Panel
    Private WithEvents m_rbValues As System.Windows.Forms.RadioButton
    Private WithEvents m_rbHisto As System.Windows.Forms.RadioButton
    Private WithEvents m_hdrType As cEwEHeaderLabel
    Private WithEvents m_btnShowHide As System.Windows.Forms.Button
    Private WithEvents m_rbBioEst As System.Windows.Forms.RadioButton
    Private WithEvents m_hdrPlots As cEwEHeaderLabel
    Friend WithEvents pnlGraph As System.Windows.Forms.Panel
    Private WithEvents m_graph As ZedGraph.ZedGraphControl
    Private WithEvents m_rbTotFleetValue As System.Windows.Forms.RadioButton
    Private WithEvents m_rbFComparison As System.Windows.Forms.RadioButton
End Class
