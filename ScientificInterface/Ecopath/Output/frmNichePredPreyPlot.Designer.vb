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
Imports SharedResources = ScientificInterfaceShared.My.Resources

Namespace Ecopath.Output

    Partial Class frmNichePredPreyPlot
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmNichePredPreyPlot))
            Me.m_graph = New ZedGraph.ZedGraphControl()
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_cbLabels = New System.Windows.Forms.CheckBox()
            Me.m_lblColours = New System.Windows.Forms.Label()
            Me.m_btnShowHideGroups = New System.Windows.Forms.Button()
            Me.m_rbNone = New System.Windows.Forms.RadioButton()
            Me.m_rbOverlap = New System.Windows.Forms.RadioButton()
            Me.m_rbPrey = New System.Windows.Forms.RadioButton()
            Me.m_rbPredator = New System.Windows.Forms.RadioButton()
            Me.m_nudCutOff = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_hdrDisplay = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lblCutOff = New System.Windows.Forms.Label()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            CType(Me.m_nudCutOff, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_graph
            '
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
            'm_scMain
            '
            Me.m_scMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            resources.ApplyResources(Me.m_scMain, "m_scMain")
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            Me.m_scMain.Panel1.Controls.Add(Me.m_cbLabels)
            Me.m_scMain.Panel1.Controls.Add(Me.m_lblColours)
            Me.m_scMain.Panel1.Controls.Add(Me.m_btnShowHideGroups)
            Me.m_scMain.Panel1.Controls.Add(Me.m_rbNone)
            Me.m_scMain.Panel1.Controls.Add(Me.m_rbOverlap)
            Me.m_scMain.Panel1.Controls.Add(Me.m_rbPrey)
            Me.m_scMain.Panel1.Controls.Add(Me.m_rbPredator)
            Me.m_scMain.Panel1.Controls.Add(Me.m_nudCutOff)
            Me.m_scMain.Panel1.Controls.Add(Me.m_hdrDisplay)
            Me.m_scMain.Panel1.Controls.Add(Me.m_lblCutOff)
            '
            'm_scMain.Panel2
            '
            Me.m_scMain.Panel2.Controls.Add(Me.m_graph)
            '
            'm_cbLabels
            '
            resources.ApplyResources(Me.m_cbLabels, "m_cbLabels")
            Me.m_cbLabels.Name = "m_cbLabels"
            Me.m_cbLabels.UseVisualStyleBackColor = True
            '
            'm_lblColours
            '
            resources.ApplyResources(Me.m_lblColours, "m_lblColours")
            Me.m_lblColours.Name = "m_lblColours"
            '
            'm_btnShowHideGroups
            '
            resources.ApplyResources(Me.m_btnShowHideGroups, "m_btnShowHideGroups")
            Me.m_btnShowHideGroups.Name = "m_btnShowHideGroups"
            Me.m_btnShowHideGroups.UseVisualStyleBackColor = True
            '
            'm_rbNone
            '
            resources.ApplyResources(Me.m_rbNone, "m_rbNone")
            Me.m_rbNone.Name = "m_rbNone"
            Me.m_rbNone.TabStop = True
            Me.m_rbNone.UseVisualStyleBackColor = True
            '
            'm_rbOverlap
            '
            resources.ApplyResources(Me.m_rbOverlap, "m_rbOverlap")
            Me.m_rbOverlap.Name = "m_rbOverlap"
            Me.m_rbOverlap.TabStop = True
            Me.m_rbOverlap.UseVisualStyleBackColor = True
            '
            'm_rbPrey
            '
            resources.ApplyResources(Me.m_rbPrey, "m_rbPrey")
            Me.m_rbPrey.Name = "m_rbPrey"
            Me.m_rbPrey.TabStop = True
            Me.m_rbPrey.UseVisualStyleBackColor = True
            '
            'm_rbPredator
            '
            resources.ApplyResources(Me.m_rbPredator, "m_rbPredator")
            Me.m_rbPredator.Name = "m_rbPredator"
            Me.m_rbPredator.TabStop = True
            Me.m_rbPredator.UseVisualStyleBackColor = True
            '
            'm_nudCutOff
            '
            Me.m_nudCutOff.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudCutOff, "m_nudCutOff")
            Me.m_nudCutOff.Name = "m_nudCutOff"
            '
            'm_hdrDisplay
            '
            resources.ApplyResources(Me.m_hdrDisplay, "m_hdrDisplay")
            Me.m_hdrDisplay.CanCollapseParent = False
            Me.m_hdrDisplay.CollapsedParentHeight = 0
            Me.m_hdrDisplay.IsCollapsed = False
            Me.m_hdrDisplay.Name = "m_hdrDisplay"
            '
            'm_lblCutOff
            '
            resources.ApplyResources(Me.m_lblCutOff, "m_lblCutOff")
            Me.m_lblCutOff.Name = "m_lblCutOff"
            '
            'frmNichePredPreyPlot
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_scMain)
            Me.Name = "frmNichePredPreyPlot"
            Me.TabText = ""
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel1.PerformLayout()
            Me.m_scMain.Panel2.ResumeLayout(False)
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            CType(Me.m_nudCutOff, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_graph As ZedGraph.ZedGraphControl
        Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
        Private WithEvents m_lblCutOff As System.Windows.Forms.Label
        Private WithEvents m_rbOverlap As System.Windows.Forms.RadioButton
        Private WithEvents m_rbPrey As System.Windows.Forms.RadioButton
        Private WithEvents m_rbPredator As System.Windows.Forms.RadioButton
        Private WithEvents m_rbNone As System.Windows.Forms.RadioButton
        Private WithEvents m_lblColours As System.Windows.Forms.Label
        Private WithEvents m_btnShowHideGroups As System.Windows.Forms.Button
        Private WithEvents m_hdrDisplay As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_cbLabels As System.Windows.Forms.CheckBox
        Private WithEvents m_nudCutOff As ScientificInterfaceShared.Controls.cEwENumericUpDown
    End Class

End Namespace
