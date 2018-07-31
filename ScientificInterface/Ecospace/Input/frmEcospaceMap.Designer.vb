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
Imports ScientificInterfaceShared.Controls.Map
Imports SharedResources = ScientificInterfaceShared.My.Resources

Namespace Ecospace.Basemap

    Partial Class frmEcospaceMap
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
        '<System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcospaceMap))
            Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
            Me.m_zoomToolbar = New ScientificInterfaceShared.Controls.Map.ucMapZoomToolbar()
            Me.m_zoomContainer = New ScientificInterfaceShared.Controls.Map.ucMapZoom()
            Me.m_tlpControls = New System.Windows.Forms.TableLayoutPanel()
            Me.m_tsEditBasemapThingies = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.tsbEditBasemap = New System.Windows.Forms.ToolStripButton()
            Me.m_ucLayers = New ScientificInterfaceShared.Controls.Map.ucLayersControl()
            Me.m_plEditor = New System.Windows.Forms.Panel()
            CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SplitContainer1.Panel1.SuspendLayout()
            Me.SplitContainer1.Panel2.SuspendLayout()
            Me.SplitContainer1.SuspendLayout()
            Me.m_tlpControls.SuspendLayout()
            Me.m_tsEditBasemapThingies.SuspendLayout()
            Me.SuspendLayout()
            '
            'SplitContainer1
            '
            resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
            Me.SplitContainer1.Name = "SplitContainer1"
            '
            'SplitContainer1.Panel1
            '
            Me.SplitContainer1.Panel1.Controls.Add(Me.m_zoomToolbar)
            Me.SplitContainer1.Panel1.Controls.Add(Me.m_zoomContainer)
            '
            'SplitContainer1.Panel2
            '
            Me.SplitContainer1.Panel2.Controls.Add(Me.m_tlpControls)
            '
            'm_zoomToolbar
            '
            resources.ApplyResources(Me.m_zoomToolbar, "m_zoomToolbar")
            Me.m_zoomToolbar.Name = "m_zoomToolbar"
            Me.m_zoomToolbar.UIContext = Nothing
            '
            'm_zoomContainer
            '
            resources.ApplyResources(Me.m_zoomContainer, "m_zoomContainer")
            Me.m_zoomContainer.BackColor = System.Drawing.SystemColors.ButtonShadow
            Me.m_zoomContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_zoomContainer.Name = "m_zoomContainer"
            '
            'm_tlpControls
            '
            resources.ApplyResources(Me.m_tlpControls, "m_tlpControls")
            Me.m_tlpControls.Controls.Add(Me.m_tsEditBasemapThingies, 0, 0)
            Me.m_tlpControls.Controls.Add(Me.m_ucLayers, 0, 1)
            Me.m_tlpControls.Controls.Add(Me.m_plEditor, 0, 2)
            Me.m_tlpControls.Name = "m_tlpControls"
            '
            'm_tsEditBasemapThingies
            '
            resources.ApplyResources(Me.m_tsEditBasemapThingies, "m_tsEditBasemapThingies")
            Me.m_tsEditBasemapThingies.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsEditBasemapThingies.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsbEditBasemap})
            Me.m_tsEditBasemapThingies.Name = "m_tsEditBasemapThingies"
            Me.m_tsEditBasemapThingies.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'tsbEditBasemap
            '
            resources.ApplyResources(Me.tsbEditBasemap, "tsbEditBasemap")
            Me.tsbEditBasemap.Name = "tsbEditBasemap"
            '
            'm_ucLayers
            '
            resources.ApplyResources(Me.m_ucLayers, "m_ucLayers")
            Me.m_ucLayers.BackColor = System.Drawing.SystemColors.Control
            Me.m_ucLayers.Name = "m_ucLayers"
            Me.m_ucLayers.UIContext = Nothing
            '
            'm_plEditor
            '
            Me.m_plEditor.BackColor = System.Drawing.SystemColors.Window
            Me.m_plEditor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            resources.ApplyResources(Me.m_plEditor, "m_plEditor")
            Me.m_plEditor.Name = "m_plEditor"
            '
            'frmEcospaceMap
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.SplitContainer1)
            Me.Name = "frmEcospaceMap"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.TabText = "Define habitats"
            Me.SplitContainer1.Panel1.ResumeLayout(False)
            Me.SplitContainer1.Panel1.PerformLayout()
            Me.SplitContainer1.Panel2.ResumeLayout(False)
            CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
            Me.SplitContainer1.ResumeLayout(False)
            Me.m_tlpControls.ResumeLayout(False)
            Me.m_tlpControls.PerformLayout()
            Me.m_tsEditBasemapThingies.ResumeLayout(False)
            Me.m_tsEditBasemapThingies.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
        Private WithEvents m_tlpControls As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_tsEditBasemapThingies As cEwEToolstrip
        Private WithEvents tsbEditBasemap As System.Windows.Forms.ToolStripButton
        Private WithEvents m_plEditor As System.Windows.Forms.Panel
        Private WithEvents m_zoomContainer As ucMapZoom
        Private WithEvents m_zoomToolbar As ucMapZoomToolbar
        Private WithEvents m_ucLayers As ScientificInterfaceShared.Controls.Map.ucLayersControl

    End Class

End Namespace

