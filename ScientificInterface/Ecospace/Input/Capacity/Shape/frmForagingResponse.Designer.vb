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


Partial Class frmForagingResponse
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmForagingResponse))
        Me.m_tlpSketchPad = New System.Windows.Forms.TableLayoutPanel()
        Me.m_sketchPadToolbar = New ScientificInterfaceShared.Controls.ucSketchPadToolbar()
        Me.m_sketchPad = New ScientificInterfaceShared.Controls.ucMediationSketchPad()
        Me.m_scMain = New System.Windows.Forms.SplitContainer()
        Me.m_scBottomBits = New System.Windows.Forms.SplitContainer()
        Me.m_tlpToolbox = New System.Windows.Forms.TableLayoutPanel()
        Me.m_shapeToolBox = New ScientificInterfaceShared.Controls.ucShapeToolbox()
        Me.m_shapeToolboxToolbar = New ScientificInterfaceShared.Controls.ucShapeToolboxToolbar()
        Me.m_tlpAssingments = New System.Windows.Forms.TableLayoutPanel()
        Me.m_assignments = New ScientificInterfaceShared.Controls.ucMediationAssignments()
        Me.m_assignmentsToolbar = New ScientificInterfaceShared.Controls.ucMediationAssignmentsToolbar()
        Me.m_tlpSketchPad.SuspendLayout()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scMain.Panel1.SuspendLayout()
        Me.m_scMain.Panel2.SuspendLayout()
        Me.m_scMain.SuspendLayout()
        CType(Me.m_scBottomBits, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scBottomBits.Panel1.SuspendLayout()
        Me.m_scBottomBits.Panel2.SuspendLayout()
        Me.m_scBottomBits.SuspendLayout()
        Me.m_tlpToolbox.SuspendLayout()
        Me.m_tlpAssingments.SuspendLayout()
        Me.SuspendLayout()
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
        resources.ApplyResources(Me.m_sketchPadToolbar, "m_sketchPadToolbar")
        Me.m_sketchPadToolbar.BackColor = System.Drawing.SystemColors.Control
        Me.m_sketchPadToolbar.Handler = Nothing
        Me.m_sketchPadToolbar.Name = "m_sketchPadToolbar"
        Me.m_sketchPadToolbar.UIContext = Nothing
        '
        'm_sketchPad
        '
        Me.m_sketchPad.AllowDragXMark = False
        resources.ApplyResources(Me.m_sketchPad, "m_sketchPad")
        Me.m_sketchPad.BackColor = System.Drawing.SystemColors.Window
        Me.m_sketchPad.CanEditPoints = True
        Me.m_sketchPad.Cursor = System.Windows.Forms.Cursors.Cross
        Me.m_sketchPad.DisplayAxis = True
        Me.m_sketchPad.Editable = True
        Me.m_sketchPad.Handler = Nothing
        Me.m_sketchPad.IsSeasonal = False
        Me.m_sketchPad.Name = "m_sketchPad"
        Me.m_sketchPad.NumDataPoints = -9999
        Me.m_sketchPad.Shape = Nothing
        Me.m_sketchPad.ShapeColor = System.Drawing.Color.AliceBlue
        Me.m_sketchPad.ShowValueTooltip = False
        Me.m_sketchPad.ShowXMark = False
        Me.m_sketchPad.ShowYMark = False
        Me.m_sketchPad.SketchDrawMode = ScientificInterfaceShared.Definitions.eSketchDrawModeTypes.Fill
        Me.m_sketchPad.UIContext = Nothing
        Me.m_sketchPad.XAxisLabel = "Input value"
        Me.m_sketchPad.XAxisMaxValue = -9999
        Me.m_sketchPad.XMarkValue = -9999.0!
        Me.m_sketchPad.YAxisAutoScaleMode = ScientificInterfaceShared.Definitions.eAxisAutoScaleModeTypes.[Auto]
        Me.m_sketchPad.YAxisMaxValue = 0!
        Me.m_sketchPad.YAxisMinValue = -9999.0!
        Me.m_sketchPad.YMarkLabel = ""
        Me.m_sketchPad.YMarkValue = -9999.0!
        '
        'm_scMain
        '
        Me.m_scMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        resources.ApplyResources(Me.m_scMain, "m_scMain")
        Me.m_scMain.Name = "m_scMain"
        '
        'm_scMain.Panel1
        '
        Me.m_scMain.Panel1.Controls.Add(Me.m_tlpSketchPad)
        '
        'm_scMain.Panel2
        '
        Me.m_scMain.Panel2.Controls.Add(Me.m_scBottomBits)
        '
        'm_scBottomBits
        '
        Me.m_scBottomBits.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        resources.ApplyResources(Me.m_scBottomBits, "m_scBottomBits")
        Me.m_scBottomBits.Name = "m_scBottomBits"
        '
        'm_scBottomBits.Panel1
        '
        Me.m_scBottomBits.Panel1.Controls.Add(Me.m_tlpToolbox)
        '
        'm_scBottomBits.Panel2
        '
        Me.m_scBottomBits.Panel2.Controls.Add(Me.m_tlpAssingments)
        '
        'm_tlpToolbox
        '
        resources.ApplyResources(Me.m_tlpToolbox, "m_tlpToolbox")
        Me.m_tlpToolbox.Controls.Add(Me.m_shapeToolBox, 0, 1)
        Me.m_tlpToolbox.Controls.Add(Me.m_shapeToolboxToolbar, 0, 0)
        Me.m_tlpToolbox.Name = "m_tlpToolbox"
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
        'm_shapeToolboxToolbar
        '
        resources.ApplyResources(Me.m_shapeToolboxToolbar, "m_shapeToolboxToolbar")
        Me.m_shapeToolboxToolbar.Handler = Nothing
        Me.m_shapeToolboxToolbar.Name = "m_shapeToolboxToolbar"
        '
        'm_tlpAssingments
        '
        resources.ApplyResources(Me.m_tlpAssingments, "m_tlpAssingments")
        Me.m_tlpAssingments.Controls.Add(Me.m_assignments, 0, 1)
        Me.m_tlpAssingments.Controls.Add(Me.m_assignmentsToolbar, 0, 0)
        Me.m_tlpAssingments.Name = "m_tlpAssingments"
        '
        'm_assignments
        '
        resources.ApplyResources(Me.m_assignments, "m_assignments")
        Me.m_assignments.Data = Nothing
        Me.m_assignments.Name = "m_assignments"
        Me.m_assignments.Shape = Nothing
        Me.m_assignments.Title = ""
        Me.m_assignments.UIContext = Nothing
        Me.m_assignments.ViewMode = ScientificInterfaceShared.Controls.ucMediationAssignments.eViewModeTypes.Line
        Me.m_assignments.XAxisLabel = "Assigned driver maps"
        Me.m_assignments.YAxisLabel = "Response"
        '
        'm_assignmentsToolbar
        '
        resources.ApplyResources(Me.m_assignmentsToolbar, "m_assignmentsToolbar")
        Me.m_assignmentsToolbar.BackColor = System.Drawing.SystemColors.Control
        Me.m_assignmentsToolbar.DefineMediationLabel = "Define foraging response...."
        Me.m_assignmentsToolbar.Handler = Nothing
        Me.m_assignmentsToolbar.IsMenuVisible = True
        Me.m_assignmentsToolbar.Name = "m_assignmentsToolbar"
        '
        'frmForagingResponse
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_scMain)
        Me.Name = "frmForagingResponse"
        Me.TabText = ""
        Me.m_tlpSketchPad.ResumeLayout(False)
        Me.m_scMain.Panel1.ResumeLayout(False)
        Me.m_scMain.Panel2.ResumeLayout(False)
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scMain.ResumeLayout(False)
        Me.m_scBottomBits.Panel1.ResumeLayout(False)
        Me.m_scBottomBits.Panel2.ResumeLayout(False)
        CType(Me.m_scBottomBits, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scBottomBits.ResumeLayout(False)
        Me.m_tlpToolbox.ResumeLayout(False)
        Me.m_tlpToolbox.PerformLayout()
        Me.m_tlpAssingments.ResumeLayout(False)
        Me.m_tlpAssingments.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
    Private WithEvents m_tlpSketchPad As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_sketchPadToolbar As ucSketchPadToolbar
    Private WithEvents m_shapeToolBox As ucShapeToolbox
    Private WithEvents m_shapeToolboxToolbar As ucShapeToolboxToolbar
    Private WithEvents m_sketchPad As ucMediationSketchPad
    Private WithEvents m_assignments As ucMediationAssignments
    Private WithEvents m_assignmentsToolbar As ucMediationAssignmentsToolbar
    Private WithEvents m_tlpAssingments As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_tlpToolbox As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_scBottomBits As System.Windows.Forms.SplitContainer

End Class


