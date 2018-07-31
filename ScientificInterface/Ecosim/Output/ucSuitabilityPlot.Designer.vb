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

Partial Class ucSuitabilityPlot
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.m_graph = New ZedGraph.ZedGraphControl
        Me.m_scContent = New System.Windows.Forms.SplitContainer
        Me.m_rbSuitability = New System.Windows.Forms.RadioButton
        Me.m_rbFunctionalResponse = New System.Windows.Forms.RadioButton
        Me.m_rbElectivity = New System.Windows.Forms.RadioButton
        Me.m_hdrPredator = New cEwEHeaderLabel
        Me.m_hdrPlotType = New cEwEHeaderLabel
        Me.m_lbGroups = New ScientificInterfaceShared.Controls.cGroupListBox
        Me.m_scContent.Panel1.SuspendLayout()
        Me.m_scContent.Panel2.SuspendLayout()
        Me.m_scContent.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_graph
        '
        Me.m_graph.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_graph.IsShowPointValues = True
        Me.m_graph.Location = New System.Drawing.Point(0, 0)
        Me.m_graph.Name = "m_graph"
        Me.m_graph.ScrollGrace = 0
        Me.m_graph.ScrollMaxX = 0
        Me.m_graph.ScrollMaxY = 0
        Me.m_graph.ScrollMaxY2 = 0
        Me.m_graph.ScrollMinX = 0
        Me.m_graph.ScrollMinY = 0
        Me.m_graph.ScrollMinY2 = 0
        Me.m_graph.Size = New System.Drawing.Size(504, 532)
        Me.m_graph.TabIndex = 4
        '
        'm_scContent
        '
        Me.m_scContent.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_scContent.Location = New System.Drawing.Point(0, 0)
        Me.m_scContent.Name = "m_scContent"
        '
        'm_scContent.Panel1
        '
        Me.m_scContent.Panel1.Controls.Add(Me.m_graph)
        '
        'm_scContent.Panel2
        '
        Me.m_scContent.Panel2.Controls.Add(Me.m_rbSuitability)
        Me.m_scContent.Panel2.Controls.Add(Me.m_rbFunctionalResponse)
        Me.m_scContent.Panel2.Controls.Add(Me.m_rbElectivity)
        Me.m_scContent.Panel2.Controls.Add(Me.m_hdrPredator)
        Me.m_scContent.Panel2.Controls.Add(Me.m_hdrPlotType)
        Me.m_scContent.Panel2.Controls.Add(Me.m_lbGroups)
        Me.m_scContent.Size = New System.Drawing.Size(700, 532)
        Me.m_scContent.SplitterDistance = 504
        Me.m_scContent.TabIndex = 6
        '
        'm_rbSuitability
        '
        Me.m_rbSuitability.AutoSize = True
        Me.m_rbSuitability.Location = New System.Drawing.Point(4, 69)
        Me.m_rbSuitability.Name = "m_rbSuitability"
        Me.m_rbSuitability.Size = New System.Drawing.Size(69, 17)
        Me.m_rbSuitability.TabIndex = 2
        Me.m_rbSuitability.TabStop = True
        Me.m_rbSuitability.Tag = ""
        Me.m_rbSuitability.Text = "Suitability"
        Me.m_rbSuitability.UseVisualStyleBackColor = True
        '
        'm_rbFunctionalResponse
        '
        Me.m_rbFunctionalResponse.AutoSize = True
        Me.m_rbFunctionalResponse.Location = New System.Drawing.Point(4, 46)
        Me.m_rbFunctionalResponse.Name = "m_rbFunctionalResponse"
        Me.m_rbFunctionalResponse.Size = New System.Drawing.Size(120, 17)
        Me.m_rbFunctionalResponse.TabIndex = 2
        Me.m_rbFunctionalResponse.TabStop = True
        Me.m_rbFunctionalResponse.Tag = ""
        Me.m_rbFunctionalResponse.Text = "Functional response"
        Me.m_rbFunctionalResponse.UseVisualStyleBackColor = True
        '
        'm_rbElectivity
        '
        Me.m_rbElectivity.AutoSize = True
        Me.m_rbElectivity.Location = New System.Drawing.Point(4, 23)
        Me.m_rbElectivity.Name = "m_rbElectivity"
        Me.m_rbElectivity.Size = New System.Drawing.Size(67, 17)
        Me.m_rbElectivity.TabIndex = 2
        Me.m_rbElectivity.TabStop = True
        Me.m_rbElectivity.Tag = ""
        Me.m_rbElectivity.Text = "Electivity"
        Me.m_rbElectivity.UseVisualStyleBackColor = True
        '
        'm_hdrPredator
        '
        Me.m_hdrPredator.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_hdrPredator.BackColor = System.Drawing.SystemColors.ControlDark
        Me.m_hdrPredator.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.m_hdrPredator.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.m_hdrPredator.Location = New System.Drawing.Point(0, 105)
        Me.m_hdrPredator.Margin = New System.Windows.Forms.Padding(0)
        Me.m_hdrPredator.Name = "m_hdrPredator"
        Me.m_hdrPredator.Size = New System.Drawing.Size(196, 18)
        Me.m_hdrPredator.TabIndex = 1
        Me.m_hdrPredator.Text = "Predator"
        Me.m_hdrPredator.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_hdrPlotType
        '
        Me.m_hdrPlotType.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_hdrPlotType.BackColor = System.Drawing.SystemColors.ControlDark
        Me.m_hdrPlotType.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.m_hdrPlotType.ForeColor = System.Drawing.SystemColors.ControlLightLight
        Me.m_hdrPlotType.Location = New System.Drawing.Point(0, 0)
        Me.m_hdrPlotType.Margin = New System.Windows.Forms.Padding(0)
        Me.m_hdrPlotType.Name = "m_hdrPlotType"
        Me.m_hdrPlotType.Size = New System.Drawing.Size(196, 18)
        Me.m_hdrPlotType.TabIndex = 1
        Me.m_hdrPlotType.Text = "Plot type"
        Me.m_hdrPlotType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_lbGroups
        '
        Me.m_lbGroups.AllGroupsItemColor = System.Drawing.Color.Transparent
        Me.m_lbGroups.AllGroupsItemText = "(All)"
        Me.m_lbGroups.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_lbGroups.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.m_lbGroups.FormattingEnabled = True
        Me.m_lbGroups.GroupDisplayStyle = ScientificInterfaceShared.Controls.cGroupListBox.eGroupDisplayStyleTypes.DisplayAlways
        Me.m_lbGroups.GroupListTracking = ScientificInterfaceShared.Controls.cGroupListBox.eGroupTrackingType.LivingGroups
        Me.m_lbGroups.IntegralHeight = False
        Me.m_lbGroups.Location = New System.Drawing.Point(0, 123)
        Me.m_lbGroups.Margin = New System.Windows.Forms.Padding(0)
        Me.m_lbGroups.Name = "m_lbGroups"
        Me.m_lbGroups.SelectedGroup = Nothing
        Me.m_lbGroups.SelectedGroupIndex = -1
        Me.m_lbGroups.ShowAllGroupsItem = False
        Me.m_lbGroups.Size = New System.Drawing.Size(192, 409)
        Me.m_lbGroups.SortThreshold = -9999.0!
        Me.m_lbGroups.SortType = ScientificInterfaceShared.Controls.cGroupListBox.eSortType.GroupIndexAsc
        Me.m_lbGroups.TabIndex = 0
        '
        'ucSuitabilityPlot
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.m_scContent)
        Me.Name = "ucSuitabilityPlot"
        Me.Size = New System.Drawing.Size(700, 532)
        Me.m_scContent.Panel1.ResumeLayout(False)
        Me.m_scContent.Panel2.ResumeLayout(False)
        Me.m_scContent.Panel2.PerformLayout()
        Me.m_scContent.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_graph As ZedGraph.ZedGraphControl
    Private WithEvents m_hdrPredator As cEwEHeaderLabel
    Private WithEvents m_hdrPlotType As cEwEHeaderLabel
    Private WithEvents m_lbGroups As ScientificInterfaceShared.Controls.cGroupListBox
    Private WithEvents m_scContent As System.Windows.Forms.SplitContainer
    Private WithEvents m_rbElectivity As System.Windows.Forms.RadioButton
    Private WithEvents m_rbSuitability As System.Windows.Forms.RadioButton
    Private WithEvents m_rbFunctionalResponse As System.Windows.Forms.RadioButton

End Class
