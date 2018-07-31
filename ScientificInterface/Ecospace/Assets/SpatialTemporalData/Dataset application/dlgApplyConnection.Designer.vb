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

Namespace Ecospace.Controls

    Partial Class dlgApplyConnection
        Inherits Form

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgApplyConnection))
            Me.m_tlpContent = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plConnection = New System.Windows.Forms.Panel()
            Me.m_hdrSource = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plScalarAdapter = New System.Windows.Forms.Panel()
            Me.m_btnCalculate = New System.Windows.Forms.Button()
            Me.m_lblScaling = New System.Windows.Forms.Label()
            Me.m_tbxScale = New System.Windows.Forms.TextBox()
            Me.m_rbRelative = New System.Windows.Forms.RadioButton()
            Me.m_rbAbsolute = New System.Windows.Forms.RadioButton()
            Me.m_plConversion = New System.Windows.Forms.Panel()
            Me.m_cmbConverter = New System.Windows.Forms.ComboBox()
            Me.m_lblSelectCV = New System.Windows.Forms.Label()
            Me.m_btnConfigCV = New System.Windows.Forms.Button()
            Me.m_plDataset = New System.Windows.Forms.Panel()
            Me.m_pbCompat = New System.Windows.Forms.PictureBox()
            Me.m_lblCompatibility = New System.Windows.Forms.Label()
            Me.m_lblDatasetInfo = New System.Windows.Forms.Label()
            Me.m_btnConfigDS = New System.Windows.Forms.Button()
            Me.m_hdrConfig = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrConnections = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tslFilter = New System.Windows.Forms.ToolStripLabel()
            Me.m_tstbFilter = New System.Windows.Forms.ToolStripTextBox()
            Me.m_tsbnCaseSensitive = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnShowIncompatibleConnections = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnDefineConnections = New System.Windows.Forms.ToolStripButton()
            Me.m_btnOK = New System.Windows.Forms.Button()
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_plAvailable = New System.Windows.Forms.Panel()
            Me.m_btnRemove = New System.Windows.Forms.Button()
            Me.m_btnAdd = New System.Windows.Forms.Button()
            Me.m_cbEnabled = New System.Windows.Forms.CheckBox()
            Me.m_lbSourceDatasets = New ScientificInterface.Ecospace.Controls.cSpatialDatasetListbox()
            Me.m_gridConnections = New ScientificInterface.Ecospace.Controls.gridApplyDatasets()
            Me.m_tlpContent.SuspendLayout()
            Me.m_plConnection.SuspendLayout()
            Me.m_plScalarAdapter.SuspendLayout()
            Me.m_plConversion.SuspendLayout()
            Me.m_plDataset.SuspendLayout()
            CType(Me.m_pbCompat, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tsMain.SuspendLayout()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            Me.m_plAvailable.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_tlpContent
            '
            resources.ApplyResources(Me.m_tlpContent, "m_tlpContent")
            Me.m_tlpContent.Controls.Add(Me.m_plConnection, 0, 0)
            Me.m_tlpContent.Controls.Add(Me.m_plScalarAdapter, 0, 3)
            Me.m_tlpContent.Controls.Add(Me.m_plConversion, 0, 2)
            Me.m_tlpContent.Controls.Add(Me.m_plDataset, 0, 1)
            Me.m_tlpContent.Name = "m_tlpContent"
            '
            'm_plConnection
            '
            Me.m_plConnection.Controls.Add(Me.m_gridConnections)
            Me.m_plConnection.Controls.Add(Me.m_hdrSource)
            resources.ApplyResources(Me.m_plConnection, "m_plConnection")
            Me.m_plConnection.Name = "m_plConnection"
            '
            'm_hdrSource
            '
            Me.m_hdrSource.CanCollapseParent = False
            Me.m_hdrSource.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrSource, "m_hdrSource")
            Me.m_hdrSource.IsCollapsed = False
            Me.m_hdrSource.Name = "m_hdrSource"
            '
            'm_plScalarAdapter
            '
            Me.m_plScalarAdapter.Controls.Add(Me.m_btnCalculate)
            Me.m_plScalarAdapter.Controls.Add(Me.m_lblScaling)
            Me.m_plScalarAdapter.Controls.Add(Me.m_tbxScale)
            Me.m_plScalarAdapter.Controls.Add(Me.m_rbRelative)
            Me.m_plScalarAdapter.Controls.Add(Me.m_rbAbsolute)
            resources.ApplyResources(Me.m_plScalarAdapter, "m_plScalarAdapter")
            Me.m_plScalarAdapter.Name = "m_plScalarAdapter"
            '
            'm_btnCalculate
            '
            resources.ApplyResources(Me.m_btnCalculate, "m_btnCalculate")
            Me.m_btnCalculate.Name = "m_btnCalculate"
            Me.m_btnCalculate.UseVisualStyleBackColor = True
            '
            'm_lblScaling
            '
            resources.ApplyResources(Me.m_lblScaling, "m_lblScaling")
            Me.m_lblScaling.Name = "m_lblScaling"
            '
            'm_tbxScale
            '
            resources.ApplyResources(Me.m_tbxScale, "m_tbxScale")
            Me.m_tbxScale.Name = "m_tbxScale"
            '
            'm_rbRelative
            '
            resources.ApplyResources(Me.m_rbRelative, "m_rbRelative")
            Me.m_rbRelative.Name = "m_rbRelative"
            Me.m_rbRelative.TabStop = True
            Me.m_rbRelative.UseVisualStyleBackColor = True
            '
            'm_rbAbsolute
            '
            resources.ApplyResources(Me.m_rbAbsolute, "m_rbAbsolute")
            Me.m_rbAbsolute.Name = "m_rbAbsolute"
            Me.m_rbAbsolute.TabStop = True
            Me.m_rbAbsolute.UseVisualStyleBackColor = True
            '
            'm_plConversion
            '
            Me.m_plConversion.Controls.Add(Me.m_cmbConverter)
            Me.m_plConversion.Controls.Add(Me.m_lblSelectCV)
            Me.m_plConversion.Controls.Add(Me.m_btnConfigCV)
            resources.ApplyResources(Me.m_plConversion, "m_plConversion")
            Me.m_plConversion.Name = "m_plConversion"
            '
            'm_cmbConverter
            '
            resources.ApplyResources(Me.m_cmbConverter, "m_cmbConverter")
            Me.m_cmbConverter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbConverter.FormattingEnabled = True
            Me.m_cmbConverter.Name = "m_cmbConverter"
            '
            'm_lblSelectCV
            '
            resources.ApplyResources(Me.m_lblSelectCV, "m_lblSelectCV")
            Me.m_lblSelectCV.Name = "m_lblSelectCV"
            '
            'm_btnConfigCV
            '
            resources.ApplyResources(Me.m_btnConfigCV, "m_btnConfigCV")
            Me.m_btnConfigCV.Name = "m_btnConfigCV"
            Me.m_btnConfigCV.UseVisualStyleBackColor = True
            '
            'm_plDataset
            '
            Me.m_plDataset.Controls.Add(Me.m_pbCompat)
            Me.m_plDataset.Controls.Add(Me.m_lblCompatibility)
            Me.m_plDataset.Controls.Add(Me.m_lblDatasetInfo)
            Me.m_plDataset.Controls.Add(Me.m_btnConfigDS)
            Me.m_plDataset.Controls.Add(Me.m_hdrConfig)
            resources.ApplyResources(Me.m_plDataset, "m_plDataset")
            Me.m_plDataset.Name = "m_plDataset"
            '
            'm_pbCompat
            '
            resources.ApplyResources(Me.m_pbCompat, "m_pbCompat")
            Me.m_pbCompat.Name = "m_pbCompat"
            Me.m_pbCompat.TabStop = False
            '
            'm_lblCompatibility
            '
            resources.ApplyResources(Me.m_lblCompatibility, "m_lblCompatibility")
            Me.m_lblCompatibility.AutoEllipsis = True
            Me.m_lblCompatibility.Name = "m_lblCompatibility"
            '
            'm_lblDatasetInfo
            '
            resources.ApplyResources(Me.m_lblDatasetInfo, "m_lblDatasetInfo")
            Me.m_lblDatasetInfo.Name = "m_lblDatasetInfo"
            '
            'm_btnConfigDS
            '
            resources.ApplyResources(Me.m_btnConfigDS, "m_btnConfigDS")
            Me.m_btnConfigDS.Name = "m_btnConfigDS"
            Me.m_btnConfigDS.UseVisualStyleBackColor = True
            '
            'm_hdrConfig
            '
            resources.ApplyResources(Me.m_hdrConfig, "m_hdrConfig")
            Me.m_hdrConfig.CanCollapseParent = False
            Me.m_hdrConfig.CollapsedParentHeight = 0
            Me.m_hdrConfig.IsCollapsed = False
            Me.m_hdrConfig.Name = "m_hdrConfig"
            '
            'm_hdrConnections
            '
            resources.ApplyResources(Me.m_hdrConnections, "m_hdrConnections")
            Me.m_hdrConnections.CanCollapseParent = False
            Me.m_hdrConnections.CollapsedParentHeight = 0
            Me.m_hdrConnections.IsCollapsed = False
            Me.m_hdrConnections.Name = "m_hdrConnections"
            '
            'm_tsMain
            '
            Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnShowIncompatibleConnections, Me.m_tsbnCaseSensitive, Me.m_tstbFilter, Me.m_tslFilter, Me.m_tsbnDefineConnections})
            resources.ApplyResources(Me.m_tsMain, "m_tsMain")
            Me.m_tsMain.Name = "m_tsMain"
            Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tslFilter
            '
            Me.m_tslFilter.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
            Me.m_tslFilter.Name = "m_tslFilter"
            resources.ApplyResources(Me.m_tslFilter, "m_tslFilter")
            '
            'm_tstbFilter
            '
            Me.m_tstbFilter.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
            resources.ApplyResources(Me.m_tstbFilter, "m_tstbFilter")
            Me.m_tstbFilter.Name = "m_tstbFilter"
            '
            'm_tsbnCaseSensitive
            '
            Me.m_tsbnCaseSensitive.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
            Me.m_tsbnCaseSensitive.AutoToolTip = False
            Me.m_tsbnCaseSensitive.CheckOnClick = True
            Me.m_tsbnCaseSensitive.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            resources.ApplyResources(Me.m_tsbnCaseSensitive, "m_tsbnCaseSensitive")
            Me.m_tsbnCaseSensitive.Name = "m_tsbnCaseSensitive"
            '
            'm_tsbnShowIncompatibleConnections
            '
            Me.m_tsbnShowIncompatibleConnections.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
            Me.m_tsbnShowIncompatibleConnections.CheckOnClick = True
            Me.m_tsbnShowIncompatibleConnections.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            resources.ApplyResources(Me.m_tsbnShowIncompatibleConnections, "m_tsbnShowIncompatibleConnections")
            Me.m_tsbnShowIncompatibleConnections.Name = "m_tsbnShowIncompatibleConnections"
            '
            'm_tsbnDefineConnections
            '
            resources.ApplyResources(Me.m_tsbnDefineConnections, "m_tsbnDefineConnections")
            Me.m_tsbnDefineConnections.Name = "m_tsbnDefineConnections"
            '
            'm_btnOK
            '
            resources.ApplyResources(Me.m_btnOK, "m_btnOK")
            Me.m_btnOK.Name = "m_btnOK"
            Me.m_btnOK.UseVisualStyleBackColor = True
            '
            'm_scMain
            '
            resources.ApplyResources(Me.m_scMain, "m_scMain")
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            Me.m_scMain.Panel1.Controls.Add(Me.m_plAvailable)
            '
            'm_scMain.Panel2
            '
            Me.m_scMain.Panel2.Controls.Add(Me.m_tlpContent)
            '
            'm_plAvailable
            '
            Me.m_plAvailable.Controls.Add(Me.m_lbSourceDatasets)
            Me.m_plAvailable.Controls.Add(Me.m_btnRemove)
            Me.m_plAvailable.Controls.Add(Me.m_btnAdd)
            Me.m_plAvailable.Controls.Add(Me.m_hdrConnections)
            resources.ApplyResources(Me.m_plAvailable, "m_plAvailable")
            Me.m_plAvailable.Name = "m_plAvailable"
            '
            'm_btnRemove
            '
            resources.ApplyResources(Me.m_btnRemove, "m_btnRemove")
            Me.m_btnRemove.Name = "m_btnRemove"
            Me.m_btnRemove.UseVisualStyleBackColor = True
            '
            'm_btnAdd
            '
            resources.ApplyResources(Me.m_btnAdd, "m_btnAdd")
            Me.m_btnAdd.Name = "m_btnAdd"
            Me.m_btnAdd.UseVisualStyleBackColor = True
            '
            'm_cbEnabled
            '
            resources.ApplyResources(Me.m_cbEnabled, "m_cbEnabled")
            Me.m_cbEnabled.Name = "m_cbEnabled"
            Me.m_cbEnabled.UseVisualStyleBackColor = True
            '
            'm_lbSourceDatasets
            '
            resources.ApplyResources(Me.m_lbSourceDatasets, "m_lbSourceDatasets")
            Me.m_lbSourceDatasets.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
            Me.m_lbSourceDatasets.FormattingEnabled = True
            Me.m_lbSourceDatasets.IsTextFilterCaseSensitive = False
            Me.m_lbSourceDatasets.Name = "m_lbSourceDatasets"
            Me.m_lbSourceDatasets.Sorted = True
            Me.m_lbSourceDatasets.TextFilter = ""
            Me.m_lbSourceDatasets.UIContext = Nothing
            Me.m_lbSourceDatasets.VariableFilter = EwEUtils.Core.eVarNameFlags.NotSet
            '
            'm_gridConnections
            '
            Me.m_gridConnections.AllowBlockSelect = False
            Me.m_gridConnections.AutoSizeMinHeight = 10
            Me.m_gridConnections.AutoSizeMinWidth = 10
            Me.m_gridConnections.AutoStretchColumnsToFitWidth = True
            Me.m_gridConnections.AutoStretchRowsToFitHeight = False
            Me.m_gridConnections.BackColor = System.Drawing.Color.White
            Me.m_gridConnections.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridConnections.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridConnections.CustomSort = False
            Me.m_gridConnections.DataName = "grid content"
            resources.ApplyResources(Me.m_gridConnections, "m_gridConnections")
            Me.m_gridConnections.FixedColumnWidths = False
            Me.m_gridConnections.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridConnections.GridToolTipActive = True
            Me.m_gridConnections.IsLayoutSuspended = False
            Me.m_gridConnections.IsOutputGrid = True
            Me.m_gridConnections.Name = "m_gridConnections"
            Me.m_gridConnections.SelectedConnection = Nothing
            Me.m_gridConnections.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridConnections.UIContext = Nothing
            '
            'dlgApplyConnection
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.ControlBox = False
            Me.Controls.Add(Me.m_cbEnabled)
            Me.Controls.Add(Me.m_scMain)
            Me.Controls.Add(Me.m_btnOK)
            Me.Controls.Add(Me.m_tsMain)
            Me.MinimizeBox = False
            Me.Name = "dlgApplyConnection"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.m_tlpContent.ResumeLayout(False)
            Me.m_plConnection.ResumeLayout(False)
            Me.m_plScalarAdapter.ResumeLayout(False)
            Me.m_plScalarAdapter.PerformLayout()
            Me.m_plConversion.ResumeLayout(False)
            Me.m_plConversion.PerformLayout()
            Me.m_plDataset.ResumeLayout(False)
            CType(Me.m_pbCompat, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tsMain.ResumeLayout(False)
            Me.m_tsMain.PerformLayout()
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel2.ResumeLayout(False)
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            Me.m_plAvailable.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_tlpContent As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_plConnection As System.Windows.Forms.Panel
        Private WithEvents m_cmbConverter As System.Windows.Forms.ComboBox
        Private WithEvents m_btnConfigCV As System.Windows.Forms.Button
        Private WithEvents m_lblSelectCV As System.Windows.Forms.Label
        Private WithEvents m_hdrSource As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_plScalarAdapter As System.Windows.Forms.Panel
        Private WithEvents m_btnCalculate As System.Windows.Forms.Button
        Private WithEvents m_tbxScale As System.Windows.Forms.TextBox
        Private WithEvents m_rbRelative As System.Windows.Forms.RadioButton
        Private WithEvents m_rbAbsolute As System.Windows.Forms.RadioButton
        Private WithEvents m_hdrConnections As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_lblScaling As System.Windows.Forms.Label
        Private WithEvents m_gridConnections As ScientificInterface.Ecospace.Controls.gridApplyDatasets
        Private WithEvents m_btnConfigDS As System.Windows.Forms.Button
        Private WithEvents m_btnOK As System.Windows.Forms.Button
        Private WithEvents m_tsbnDefineConnections As System.Windows.Forms.ToolStripButton
        Private WithEvents m_hdrConfig As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
        Private WithEvents m_plAvailable As System.Windows.Forms.Panel
        Private WithEvents m_tsMain As cEwEToolstrip
        Private WithEvents m_plConversion As System.Windows.Forms.Panel
        Private WithEvents m_plDataset As System.Windows.Forms.Panel
        Private WithEvents m_lblCompatibility As System.Windows.Forms.Label
        Private WithEvents m_lblDatasetInfo As System.Windows.Forms.Label
        Private WithEvents m_btnRemove As System.Windows.Forms.Button
        Private WithEvents m_btnAdd As System.Windows.Forms.Button
        Private WithEvents m_lbSourceDatasets As cSpatialDatasetListbox
        Friend WithEvents m_pbCompat As System.Windows.Forms.PictureBox
        Private WithEvents m_cbEnabled As System.Windows.Forms.CheckBox
        Private WithEvents m_tstbFilter As ToolStripTextBox
        Private WithEvents m_tsbnCaseSensitive As ToolStripButton
        Private WithEvents m_tsbnShowIncompatibleConnections As ToolStripButton
        Friend WithEvents m_tslFilter As ToolStripLabel
    End Class

End Namespace
