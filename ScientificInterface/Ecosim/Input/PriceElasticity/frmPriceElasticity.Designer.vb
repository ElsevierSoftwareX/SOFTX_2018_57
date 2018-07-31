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

Namespace Ecosim

    Partial Class frmPriceElasticity
        Inherits frmEwE

        'UserControl overrides dispose to clean up the component list.
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
        '<System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPriceElasticity))
            Me.plSketchPad = New System.Windows.Forms.Panel()
            Me.m_tlpSketchPad = New System.Windows.Forms.TableLayoutPanel()
            Me.m_sketchPadToolbar = New ScientificInterfaceShared.Controls.ucSketchPadToolbar()
            Me.m_sketchPad = New ScientificInterfaceShared.Controls.ucMediationSketchPad()
            Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
            Me.SplitContainer2 = New System.Windows.Forms.SplitContainer()
            Me.m_scBottom = New System.Windows.Forms.SplitContainer()
            Me.m_tlpToolbox = New System.Windows.Forms.TableLayoutPanel()
            Me.m_shapeToolboxToolbar = New ScientificInterfaceShared.Controls.ucShapeToolboxToolbar()
            Me.m_shapeToolBox = New ScientificInterfaceShared.Controls.ucShapeToolbox()
            Me.m_assignments = New ScientificInterfaceShared.Controls.ucMediationAssignments()
            Me.m_assignmentsToolbar = New ScientificInterfaceShared.Controls.ucMediationAssignmentsToolbar()
            Me.plSketchPad.SuspendLayout()
            Me.m_tlpSketchPad.SuspendLayout()
            CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SplitContainer1.Panel1.SuspendLayout()
            Me.SplitContainer1.Panel2.SuspendLayout()
            Me.SplitContainer1.SuspendLayout()
            CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SplitContainer2.Panel1.SuspendLayout()
            Me.SplitContainer2.SuspendLayout()
            CType(Me.m_scBottom, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scBottom.Panel1.SuspendLayout()
            Me.m_scBottom.Panel2.SuspendLayout()
            Me.m_scBottom.SuspendLayout()
            Me.m_tlpToolbox.SuspendLayout()
            Me.SuspendLayout()
            '
            'plSketchPad
            '
            Me.plSketchPad.Controls.Add(Me.m_tlpSketchPad)
            resources.ApplyResources(Me.plSketchPad, "plSketchPad")
            Me.plSketchPad.Name = "plSketchPad"
            '
            'm_tlpSketchPad
            '
            resources.ApplyResources(Me.m_tlpSketchPad, "m_tlpSketchPad")
            Me.m_tlpSketchPad.Controls.Add(Me.m_sketchPadToolbar, 0, 0)
            Me.m_tlpSketchPad.Controls.Add(Me.m_sketchPad, 0, 1)
            Me.m_tlpSketchPad.Name = "m_tlpSketchPad"
            '
            'm_sketchPadToolbar
            '
            Me.m_sketchPadToolbar.BackColor = System.Drawing.SystemColors.Control
            resources.ApplyResources(Me.m_sketchPadToolbar, "m_sketchPadToolbar")
            Me.m_sketchPadToolbar.Handler = Nothing
            Me.m_sketchPadToolbar.Name = "m_sketchPadToolbar"
            Me.m_sketchPadToolbar.UIContext = Nothing
            '
            'm_sketchPad
            '
            Me.m_sketchPad.AllowDragXMark = True
            Me.m_sketchPad.BackColor = System.Drawing.SystemColors.Window
            Me.m_sketchPad.CanEditPoints = True
            Me.m_sketchPad.Cursor = System.Windows.Forms.Cursors.Cross
            Me.m_sketchPad.DisplayAxis = True
            resources.ApplyResources(Me.m_sketchPad, "m_sketchPad")
            Me.m_sketchPad.Editable = True
            Me.m_sketchPad.Handler = Nothing
            Me.m_sketchPad.IsSeasonal = False
            Me.m_sketchPad.Name = "m_sketchPad"
            Me.m_sketchPad.NumDataPoints = -9999
            Me.m_sketchPad.Shape = Nothing
            Me.m_sketchPad.ShapeColor = System.Drawing.Color.AliceBlue
            Me.m_sketchPad.ShowValueTooltip = True
            Me.m_sketchPad.ShowXMark = True
            Me.m_sketchPad.ShowYMark = True
            Me.m_sketchPad.SketchDrawMode = ScientificInterfaceShared.Definitions.eSketchDrawModeTypes.Fill
            Me.m_sketchPad.UIContext = Nothing
            Me.m_sketchPad.XAxisLabel = "Landings (weighted by value)"
            Me.m_sketchPad.XAxisMaxValue = -9999
            Me.m_sketchPad.XMarkValue = -9999.0!
            Me.m_sketchPad.YAxisAutoScaleMode = ScientificInterfaceShared.Definitions.eAxisAutoScaleModeTypes.[Auto]
            Me.m_sketchPad.YAxisMaxValue = 0.0!
            Me.m_sketchPad.YAxisMinValue = -9999.0!
            Me.m_sketchPad.YMarkLabel = ""
            Me.m_sketchPad.YMarkValue = -9999.0!
            '
            'SplitContainer1
            '
            Me.SplitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
            Me.SplitContainer1.Name = "SplitContainer1"
            '
            'SplitContainer1.Panel1
            '
            Me.SplitContainer1.Panel1.Controls.Add(Me.SplitContainer2)
            '
            'SplitContainer1.Panel2
            '
            Me.SplitContainer1.Panel2.Controls.Add(Me.m_scBottom)
            '
            'SplitContainer2
            '
            Me.SplitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            resources.ApplyResources(Me.SplitContainer2, "SplitContainer2")
            Me.SplitContainer2.Name = "SplitContainer2"
            '
            'SplitContainer2.Panel1
            '
            Me.SplitContainer2.Panel1.Controls.Add(Me.plSketchPad)
            Me.SplitContainer2.Panel2Collapsed = True
            '
            'm_scBottom
            '
            Me.m_scBottom.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            resources.ApplyResources(Me.m_scBottom, "m_scBottom")
            Me.m_scBottom.Name = "m_scBottom"
            '
            'm_scBottom.Panel1
            '
            Me.m_scBottom.Panel1.Controls.Add(Me.m_tlpToolbox)
            '
            'm_scBottom.Panel2
            '
            Me.m_scBottom.Panel2.Controls.Add(Me.m_assignments)
            Me.m_scBottom.Panel2.Controls.Add(Me.m_assignmentsToolbar)
            '
            'm_tlpToolbox
            '
            resources.ApplyResources(Me.m_tlpToolbox, "m_tlpToolbox")
            Me.m_tlpToolbox.Controls.Add(Me.m_shapeToolboxToolbar, 0, 0)
            Me.m_tlpToolbox.Controls.Add(Me.m_shapeToolBox, 0, 1)
            Me.m_tlpToolbox.Name = "m_tlpToolbox"
            '
            'm_shapeToolboxToolbar
            '
            resources.ApplyResources(Me.m_shapeToolboxToolbar, "m_shapeToolboxToolbar")
            Me.m_shapeToolboxToolbar.Handler = Nothing
            Me.m_shapeToolboxToolbar.Name = "m_shapeToolboxToolbar"
            '
            'm_shapeToolBox
            '
            Me.m_shapeToolBox.AllowCheckboxes = False
            resources.ApplyResources(Me.m_shapeToolBox, "m_shapeToolBox")
            Me.m_shapeToolBox.Color = System.Drawing.Color.Empty
            Me.m_shapeToolBox.Handler = Nothing
            Me.m_shapeToolBox.Name = "m_shapeToolBox"
            Me.m_shapeToolBox.Selection = New EwECore.cShapeData(-1) {}
            Me.m_shapeToolBox.SketchDrawMode = ScientificInterfaceShared.Definitions.eSketchDrawModeTypes.Fill
            Me.m_shapeToolBox.UIContext = Nothing
            Me.m_shapeToolBox.XAxisMaxValue = -9999
            Me.m_shapeToolBox.YAxisMinValue = -9999.0!
            '
            'm_assignments
            '
            resources.ApplyResources(Me.m_assignments, "m_assignments")
            Me.m_assignments.Data = Nothing
            Me.m_assignments.Name = "m_assignments"
            Me.m_assignments.Shape = Nothing
            Me.m_assignments.Title = ""
            Me.m_assignments.UIContext = Nothing
            Me.m_assignments.ViewMode = ScientificInterfaceShared.Controls.ucMediationAssignments.eViewModeTypes.Pie
            Me.m_assignments.XAxisLabel = "Assigned landings"
            Me.m_assignments.YAxisLabel = "Relative weight"
            '
            'm_assignmentsToolbar
            '
            resources.ApplyResources(Me.m_assignmentsToolbar, "m_assignmentsToolbar")
            Me.m_assignmentsToolbar.BackColor = System.Drawing.SystemColors.Control
            Me.m_assignmentsToolbar.DefineMediationLabel = "Define supply..."
            Me.m_assignmentsToolbar.Handler = Nothing
            Me.m_assignmentsToolbar.IsMenuVisible = True
            Me.m_assignmentsToolbar.Name = "m_assignmentsToolbar"
            '
            'frmPriceElasticity
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.SplitContainer1)
            Me.Name = "frmPriceElasticity"
            Me.TabText = ""
            Me.plSketchPad.ResumeLayout(False)
            Me.m_tlpSketchPad.ResumeLayout(False)
            Me.SplitContainer1.Panel1.ResumeLayout(False)
            Me.SplitContainer1.Panel2.ResumeLayout(False)
            CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
            Me.SplitContainer1.ResumeLayout(False)
            Me.SplitContainer2.Panel1.ResumeLayout(False)
            CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
            Me.SplitContainer2.ResumeLayout(False)
            Me.m_scBottom.Panel1.ResumeLayout(False)
            Me.m_scBottom.Panel2.ResumeLayout(False)
            Me.m_scBottom.Panel2.PerformLayout()
            CType(Me.m_scBottom, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scBottom.ResumeLayout(False)
            Me.m_tlpToolbox.ResumeLayout(False)
            Me.m_tlpToolbox.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents plSketchPad As System.Windows.Forms.Panel
        Private WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
        Private WithEvents SplitContainer2 As System.Windows.Forms.SplitContainer
        Private WithEvents m_shapeToolBox As ucShapeToolbox
        Private WithEvents m_shapeToolboxToolbar As ucShapeToolboxToolbar
        Private WithEvents m_assignmentsToolbar As ScientificInterfaceShared.Controls.ucMediationAssignmentsToolbar
        Private WithEvents m_assignments As ScientificInterfaceShared.Controls.ucMediationAssignments
        Private WithEvents m_tlpSketchPad As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_sketchPadToolbar As ScientificInterfaceShared.Controls.ucSketchPadToolbar
        Private WithEvents m_sketchPad As ScientificInterfaceShared.Controls.ucMediationSketchPad
        Private WithEvents m_scBottom As System.Windows.Forms.SplitContainer
        Private WithEvents m_tlpToolbox As System.Windows.Forms.TableLayoutPanel

    End Class
End Namespace

