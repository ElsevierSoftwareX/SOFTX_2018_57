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

Imports ScientificInterfaceShared.Controls.Map

Namespace Ecospace.Basemap.Layers

    Partial Class dlgEditLayer
        Inherits System.Windows.Forms.Form

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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgEditLayer))
            Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
            Me.OK_Button = New System.Windows.Forms.Button()
            Me.Cancel_Button = New System.Windows.Forms.Button()
            Me.Apply_Button = New System.Windows.Forms.Button()
            Me.m_lblName = New System.Windows.Forms.Label()
            Me.m_tbNameValue = New System.Windows.Forms.TextBox()
            Me.m_lblRemarks = New System.Windows.Forms.Label()
            Me.m_tbRemarks = New System.Windows.Forms.TextBox()
            Me.m_plAppearance = New System.Windows.Forms.Panel()
            Me.m_tcLayerView = New System.Windows.Forms.TabControl()
            Me.m_tpData = New System.Windows.Forms.TabPage()
            Me.m_grid = New ScientificInterface.gridLayerData()
            Me.m_tsGrid = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsddImport = New System.Windows.Forms.ToolStripDropDownButton()
            Me.m_tsmiImportCSV = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiImportXYZ = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiImportAsc = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsddExport = New System.Windows.Forms.ToolStripDropDownButton()
            Me.m_tsmiExportCSV = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiExportXYZ = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiExportAsc = New System.Windows.Forms.ToolStripMenuItem()
            Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tscmbVectorData = New System.Windows.Forms.ToolStripComboBox()
            Me.m_tpAppearance = New System.Windows.Forms.TabPage()
            Me.m_scAppearance = New System.Windows.Forms.SplitContainer()
            Me.m_zoommap = New ScientificInterfaceShared.Controls.Map.ucMapZoom()
            Me.m_tlpBits = New System.Windows.Forms.TableLayoutPanel()
            Me.m_hdrDescription = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tlpDetails = New System.Windows.Forms.TableLayoutPanel()
            Me.m_tbDescription = New System.Windows.Forms.TextBox()
            Me.m_lblWeight = New System.Windows.Forms.Label()
            Me.m_lblDescription = New System.Windows.Forms.Label()
            Me.m_nudWeight = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_hdrAppearance = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.TableLayoutPanel1.SuspendLayout()
            Me.m_tcLayerView.SuspendLayout()
            Me.m_tpData.SuspendLayout()
            Me.m_tsGrid.SuspendLayout()
            Me.m_tpAppearance.SuspendLayout()
            CType(Me.m_scAppearance, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scAppearance.Panel1.SuspendLayout()
            Me.m_scAppearance.Panel2.SuspendLayout()
            Me.m_scAppearance.SuspendLayout()
            Me.m_tlpBits.SuspendLayout()
            Me.m_tlpDetails.SuspendLayout()
            CType(Me.m_nudWeight, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'TableLayoutPanel1
            '
            resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
            Me.TableLayoutPanel1.Controls.Add(Me.OK_Button, 0, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.Cancel_Button, 1, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.Apply_Button, 2, 0)
            Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
            '
            'OK_Button
            '
            resources.ApplyResources(Me.OK_Button, "OK_Button")
            Me.OK_Button.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.OK_Button.Name = "OK_Button"
            '
            'Cancel_Button
            '
            resources.ApplyResources(Me.Cancel_Button, "Cancel_Button")
            Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Cancel_Button.Name = "Cancel_Button"
            '
            'Apply_Button
            '
            resources.ApplyResources(Me.Apply_Button, "Apply_Button")
            Me.Apply_Button.Name = "Apply_Button"
            '
            'm_lblName
            '
            resources.ApplyResources(Me.m_lblName, "m_lblName")
            Me.m_lblName.Name = "m_lblName"
            '
            'm_tbNameValue
            '
            resources.ApplyResources(Me.m_tbNameValue, "m_tbNameValue")
            Me.m_tbNameValue.Name = "m_tbNameValue"
            '
            'm_lblRemarks
            '
            resources.ApplyResources(Me.m_lblRemarks, "m_lblRemarks")
            Me.m_lblRemarks.Name = "m_lblRemarks"
            '
            'm_tbRemarks
            '
            resources.ApplyResources(Me.m_tbRemarks, "m_tbRemarks")
            Me.m_tbRemarks.Name = "m_tbRemarks"
            '
            'm_plAppearance
            '
            resources.ApplyResources(Me.m_plAppearance, "m_plAppearance")
            Me.m_plAppearance.Name = "m_plAppearance"
            '
            'm_tcLayerView
            '
            resources.ApplyResources(Me.m_tcLayerView, "m_tcLayerView")
            Me.m_tcLayerView.Controls.Add(Me.m_tpData)
            Me.m_tcLayerView.Controls.Add(Me.m_tpAppearance)
            Me.m_tcLayerView.Name = "m_tcLayerView"
            Me.m_tcLayerView.SelectedIndex = 0
            '
            'm_tpData
            '
            Me.m_tpData.Controls.Add(Me.m_grid)
            Me.m_tpData.Controls.Add(Me.m_tsGrid)
            resources.ApplyResources(Me.m_tpData, "m_tpData")
            Me.m_tpData.Name = "m_tpData"
            Me.m_tpData.UseVisualStyleBackColor = True
            '
            'm_grid
            '
            Me.m_grid.AllowBlockSelect = True
            resources.ApplyResources(Me.m_grid, "m_grid")
            Me.m_grid.AutoSizeMinHeight = 10
            Me.m_grid.AutoSizeMinWidth = 10
            Me.m_grid.AutoStretchColumnsToFitWidth = False
            Me.m_grid.AutoStretchRowsToFitHeight = False
            Me.m_grid.ContextMenuStyle = SourceGrid2.ContextMenuStyle.None
            Me.m_grid.CustomSort = False
            Me.m_grid.DataName = "grid content"
            Me.m_grid.FixedColumnWidths = True
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = False
            Me.m_grid.Layer = Nothing
            Me.m_grid.Name = "m_grid"
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.TrackPropertySelection = False
            Me.m_grid.UIContext = Nothing
            Me.m_grid.VectorFieldIndex = 0
            '
            'm_tsGrid
            '
            Me.m_tsGrid.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsGrid.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsddImport, Me.m_tsddExport, Me.ToolStripSeparator1, Me.m_tscmbVectorData})
            resources.ApplyResources(Me.m_tsGrid, "m_tsGrid")
            Me.m_tsGrid.Name = "m_tsGrid"
            Me.m_tsGrid.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsddImport
            '
            Me.m_tsddImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            Me.m_tsddImport.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiImportCSV, Me.m_tsmiImportXYZ, Me.m_tsmiImportAsc})
            resources.ApplyResources(Me.m_tsddImport, "m_tsddImport")
            Me.m_tsddImport.Name = "m_tsddImport"
            '
            'm_tsmiImportCSV
            '
            Me.m_tsmiImportCSV.Name = "m_tsmiImportCSV"
            resources.ApplyResources(Me.m_tsmiImportCSV, "m_tsmiImportCSV")
            '
            'm_tsmiImportXYZ
            '
            Me.m_tsmiImportXYZ.Name = "m_tsmiImportXYZ"
            resources.ApplyResources(Me.m_tsmiImportXYZ, "m_tsmiImportXYZ")
            '
            'm_tsmiImportAsc
            '
            Me.m_tsmiImportAsc.Name = "m_tsmiImportAsc"
            resources.ApplyResources(Me.m_tsmiImportAsc, "m_tsmiImportAsc")
            '
            'm_tsddExport
            '
            Me.m_tsddExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            Me.m_tsddExport.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiExportCSV, Me.m_tsmiExportXYZ, Me.m_tsmiExportAsc})
            resources.ApplyResources(Me.m_tsddExport, "m_tsddExport")
            Me.m_tsddExport.Name = "m_tsddExport"
            '
            'm_tsmiExportCSV
            '
            Me.m_tsmiExportCSV.Name = "m_tsmiExportCSV"
            resources.ApplyResources(Me.m_tsmiExportCSV, "m_tsmiExportCSV")
            '
            'm_tsmiExportXYZ
            '
            Me.m_tsmiExportXYZ.Name = "m_tsmiExportXYZ"
            resources.ApplyResources(Me.m_tsmiExportXYZ, "m_tsmiExportXYZ")
            '
            'm_tsmiExportAsc
            '
            Me.m_tsmiExportAsc.Name = "m_tsmiExportAsc"
            resources.ApplyResources(Me.m_tsmiExportAsc, "m_tsmiExportAsc")
            '
            'ToolStripSeparator1
            '
            Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
            resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
            '
            'm_tscmbVectorData
            '
            Me.m_tscmbVectorData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_tscmbVectorData.Items.AddRange(New Object() {resources.GetString("m_tscmbVectorData.Items"), resources.GetString("m_tscmbVectorData.Items1")})
            Me.m_tscmbVectorData.Name = "m_tscmbVectorData"
            resources.ApplyResources(Me.m_tscmbVectorData, "m_tscmbVectorData")
            '
            'm_tpAppearance
            '
            Me.m_tpAppearance.BackColor = System.Drawing.Color.Transparent
            Me.m_tpAppearance.Controls.Add(Me.m_scAppearance)
            resources.ApplyResources(Me.m_tpAppearance, "m_tpAppearance")
            Me.m_tpAppearance.Name = "m_tpAppearance"
            '
            'm_scAppearance
            '
            resources.ApplyResources(Me.m_scAppearance, "m_scAppearance")
            Me.m_scAppearance.Name = "m_scAppearance"
            '
            'm_scAppearance.Panel1
            '
            Me.m_scAppearance.Panel1.Controls.Add(Me.m_zoommap)
            '
            'm_scAppearance.Panel2
            '
            Me.m_scAppearance.Panel2.Controls.Add(Me.m_tlpBits)
            '
            'm_zoommap
            '
            resources.ApplyResources(Me.m_zoommap, "m_zoommap")
            Me.m_zoommap.Name = "m_zoommap"
            Me.m_zoommap.UIContext = Nothing
            '
            'm_tlpBits
            '
            resources.ApplyResources(Me.m_tlpBits, "m_tlpBits")
            Me.m_tlpBits.Controls.Add(Me.m_hdrDescription, 0, 0)
            Me.m_tlpBits.Controls.Add(Me.m_tlpDetails, 0, 1)
            Me.m_tlpBits.Controls.Add(Me.m_plAppearance, 0, 3)
            Me.m_tlpBits.Controls.Add(Me.m_hdrAppearance, 0, 2)
            Me.m_tlpBits.Name = "m_tlpBits"
            '
            'm_hdrDescription
            '
            Me.m_hdrDescription.CanCollapseParent = False
            Me.m_hdrDescription.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrDescription, "m_hdrDescription")
            Me.m_hdrDescription.IsCollapsed = False
            Me.m_hdrDescription.Name = "m_hdrDescription"
            '
            'm_tlpDetails
            '
            resources.ApplyResources(Me.m_tlpDetails, "m_tlpDetails")
            Me.m_tlpDetails.Controls.Add(Me.m_lblName, 0, 0)
            Me.m_tlpDetails.Controls.Add(Me.m_tbNameValue, 1, 0)
            Me.m_tlpDetails.Controls.Add(Me.m_tbRemarks, 1, 3)
            Me.m_tlpDetails.Controls.Add(Me.m_tbDescription, 1, 2)
            Me.m_tlpDetails.Controls.Add(Me.m_lblRemarks, 0, 3)
            Me.m_tlpDetails.Controls.Add(Me.m_lblWeight, 0, 1)
            Me.m_tlpDetails.Controls.Add(Me.m_lblDescription, 0, 2)
            Me.m_tlpDetails.Controls.Add(Me.m_nudWeight, 1, 1)
            Me.m_tlpDetails.Name = "m_tlpDetails"
            '
            'm_tbDescription
            '
            resources.ApplyResources(Me.m_tbDescription, "m_tbDescription")
            Me.m_tbDescription.Name = "m_tbDescription"
            '
            'm_lblWeight
            '
            resources.ApplyResources(Me.m_lblWeight, "m_lblWeight")
            Me.m_lblWeight.Name = "m_lblWeight"
            '
            'm_lblDescription
            '
            resources.ApplyResources(Me.m_lblDescription, "m_lblDescription")
            Me.m_lblDescription.Name = "m_lblDescription"
            '
            'm_nudWeight
            '
            Me.m_nudWeight.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudWeight, "m_nudWeight")
            Me.m_nudWeight.Name = "m_nudWeight"
            '
            'm_hdrAppearance
            '
            Me.m_hdrAppearance.CanCollapseParent = False
            Me.m_hdrAppearance.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrAppearance, "m_hdrAppearance")
            Me.m_hdrAppearance.IsCollapsed = False
            Me.m_hdrAppearance.Name = "m_hdrAppearance"
            '
            'dlgEditLayer
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.Cancel_Button
            Me.Controls.Add(Me.m_tcLayerView)
            Me.Controls.Add(Me.TableLayoutPanel1)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgEditLayer"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
            Me.TableLayoutPanel1.ResumeLayout(False)
            Me.m_tcLayerView.ResumeLayout(False)
            Me.m_tpData.ResumeLayout(False)
            Me.m_tpData.PerformLayout()
            Me.m_tsGrid.ResumeLayout(False)
            Me.m_tsGrid.PerformLayout()
            Me.m_tpAppearance.ResumeLayout(False)
            Me.m_scAppearance.Panel1.ResumeLayout(False)
            Me.m_scAppearance.Panel2.ResumeLayout(False)
            CType(Me.m_scAppearance, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scAppearance.ResumeLayout(False)
            Me.m_tlpBits.ResumeLayout(False)
            Me.m_tlpDetails.ResumeLayout(False)
            Me.m_tlpDetails.PerformLayout()
            CType(Me.m_nudWeight, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
        Private WithEvents OK_Button As System.Windows.Forms.Button
        Private WithEvents Cancel_Button As System.Windows.Forms.Button
        Private WithEvents m_lblName As System.Windows.Forms.Label
        Private WithEvents m_lblRemarks As System.Windows.Forms.Label
        Private WithEvents m_tbRemarks As System.Windows.Forms.TextBox
        Private WithEvents Apply_Button As System.Windows.Forms.Button
        Private WithEvents m_plAppearance As System.Windows.Forms.Panel
        Private WithEvents m_tbNameValue As System.Windows.Forms.TextBox
        Private WithEvents m_tcLayerView As System.Windows.Forms.TabControl
        Private WithEvents m_tpAppearance As System.Windows.Forms.TabPage
        Private WithEvents m_tpData As System.Windows.Forms.TabPage
        Private WithEvents m_hdrAppearance As cEwEHeaderLabel
        Private WithEvents m_hdrDescription As cEwEHeaderLabel
        Private WithEvents m_tlpDetails As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_tbDescription As System.Windows.Forms.TextBox
        Private WithEvents m_lblDescription As System.Windows.Forms.Label
        Private WithEvents m_lblWeight As System.Windows.Forms.Label
        Private WithEvents m_zoommap As ucMapZoom
        Private WithEvents m_tsGrid As cEwEToolstrip
        Private WithEvents m_nudWeight As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_scAppearance As System.Windows.Forms.SplitContainer
        Private WithEvents m_tlpBits As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_grid As ScientificInterface.gridLayerData
        Private WithEvents m_tsddImport As System.Windows.Forms.ToolStripDropDownButton
        Private WithEvents m_tsmiImportCSV As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiImportXYZ As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiImportAsc As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsddExport As System.Windows.Forms.ToolStripDropDownButton
        Private WithEvents m_tsmiExportCSV As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiExportXYZ As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiExportAsc As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_tscmbVectorData As System.Windows.Forms.ToolStripComboBox

    End Class
End Namespace