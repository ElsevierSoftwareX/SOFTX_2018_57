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

Namespace Ecospace.Controls

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class dlgDefineExternalSpatialData
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgDefineExternalSpatialData))
            Me.m_btnDelete = New System.Windows.Forms.Button()
            Me.m_btnOK = New System.Windows.Forms.Button()
            Me.m_btnConfigure = New System.Windows.Forms.Button()
            Me.m_cbEnableIndexing = New System.Windows.Forms.CheckBox()
            Me.m_btnCreate = New System.Windows.Forms.Button()
            Me.m_cmbTemplates = New System.Windows.Forms.ComboBox()
            Me.m_lblNew = New System.Windows.Forms.Label()
            Me.m_hdrExisting = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_btnExport = New System.Windows.Forms.Button()
            Me.m_lblConfig = New System.Windows.Forms.Label()
            Me.m_lblConfigValue = New System.Windows.Forms.Label()
            Me.m_hdrDefineConnections = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_btnManageConfigurations = New System.Windows.Forms.Button()
            Me.m_gridDatasets = New ScientificInterface.Ecospace.Controls.gridDefineExternalSpatialData()
            Me.m_hdrSharing = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.SuspendLayout()
            '
            'm_btnDelete
            '
            resources.ApplyResources(Me.m_btnDelete, "m_btnDelete")
            Me.m_btnDelete.Name = "m_btnDelete"
            Me.m_btnDelete.UseVisualStyleBackColor = True
            '
            'm_btnOK
            '
            resources.ApplyResources(Me.m_btnOK, "m_btnOK")
            Me.m_btnOK.Name = "m_btnOK"
            Me.m_btnOK.UseVisualStyleBackColor = True
            '
            'm_btnConfigure
            '
            resources.ApplyResources(Me.m_btnConfigure, "m_btnConfigure")
            Me.m_btnConfigure.Name = "m_btnConfigure"
            Me.m_btnConfigure.UseVisualStyleBackColor = True
            '
            'm_cbEnableIndexing
            '
            resources.ApplyResources(Me.m_cbEnableIndexing, "m_cbEnableIndexing")
            Me.m_cbEnableIndexing.Name = "m_cbEnableIndexing"
            Me.m_cbEnableIndexing.UseVisualStyleBackColor = True
            '
            'm_btnCreate
            '
            resources.ApplyResources(Me.m_btnCreate, "m_btnCreate")
            Me.m_btnCreate.Name = "m_btnCreate"
            Me.m_btnCreate.UseVisualStyleBackColor = True
            '
            'm_cmbTemplates
            '
            resources.ApplyResources(Me.m_cmbTemplates, "m_cmbTemplates")
            Me.m_cmbTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbTemplates.FormattingEnabled = True
            Me.m_cmbTemplates.Name = "m_cmbTemplates"
            '
            'm_lblNew
            '
            resources.ApplyResources(Me.m_lblNew, "m_lblNew")
            Me.m_lblNew.Name = "m_lblNew"
            '
            'm_hdrExisting
            '
            resources.ApplyResources(Me.m_hdrExisting, "m_hdrExisting")
            Me.m_hdrExisting.CanCollapseParent = False
            Me.m_hdrExisting.CollapsedParentHeight = 0
            Me.m_hdrExisting.IsCollapsed = False
            Me.m_hdrExisting.Name = "m_hdrExisting"
            '
            'm_btnExport
            '
            resources.ApplyResources(Me.m_btnExport, "m_btnExport")
            Me.m_btnExport.Name = "m_btnExport"
            Me.m_btnExport.UseVisualStyleBackColor = True
            '
            'm_lblConfig
            '
            resources.ApplyResources(Me.m_lblConfig, "m_lblConfig")
            Me.m_lblConfig.Name = "m_lblConfig"
            '
            'm_lblConfigValue
            '
            resources.ApplyResources(Me.m_lblConfigValue, "m_lblConfigValue")
            Me.m_lblConfigValue.Name = "m_lblConfigValue"
            '
            'm_hdrDefineConnections
            '
            resources.ApplyResources(Me.m_hdrDefineConnections, "m_hdrDefineConnections")
            Me.m_hdrDefineConnections.CanCollapseParent = False
            Me.m_hdrDefineConnections.CollapsedParentHeight = 0
            Me.m_hdrDefineConnections.IsCollapsed = False
            Me.m_hdrDefineConnections.Name = "m_hdrDefineConnections"
            '
            'm_btnManageConfigurations
            '
            resources.ApplyResources(Me.m_btnManageConfigurations, "m_btnManageConfigurations")
            Me.m_btnManageConfigurations.Name = "m_btnManageConfigurations"
            Me.m_btnManageConfigurations.UseVisualStyleBackColor = True
            '
            'm_gridDatasets
            '
            Me.m_gridDatasets.AllowBlockSelect = False
            resources.ApplyResources(Me.m_gridDatasets, "m_gridDatasets")
            Me.m_gridDatasets.AutoSizeMinHeight = 10
            Me.m_gridDatasets.AutoSizeMinWidth = 10
            Me.m_gridDatasets.AutoStretchColumnsToFitWidth = True
            Me.m_gridDatasets.AutoStretchRowsToFitHeight = False
            Me.m_gridDatasets.BackColor = System.Drawing.Color.White
            Me.m_gridDatasets.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridDatasets.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridDatasets.CustomSort = False
            Me.m_gridDatasets.DataName = "grid content"
            Me.m_gridDatasets.FixedColumnWidths = False
            Me.m_gridDatasets.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridDatasets.GridToolTipActive = True
            Me.m_gridDatasets.IsLayoutSuspended = False
            Me.m_gridDatasets.IsOutputGrid = True
            Me.m_gridDatasets.Name = "m_gridDatasets"
            Me.m_gridDatasets.SelectedDataset = Nothing
            Me.m_gridDatasets.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridDatasets.UIContext = Nothing
            '
            'm_hdrSharing
            '
            resources.ApplyResources(Me.m_hdrSharing, "m_hdrSharing")
            Me.m_hdrSharing.CanCollapseParent = False
            Me.m_hdrSharing.CollapsedParentHeight = 0
            Me.m_hdrSharing.IsCollapsed = False
            Me.m_hdrSharing.Name = "m_hdrSharing"
            '
            'dlgDefineExternalSpatialData
            '
            Me.AcceptButton = Me.m_btnOK
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.ControlBox = False
            Me.Controls.Add(Me.m_lblConfigValue)
            Me.Controls.Add(Me.m_lblConfig)
            Me.Controls.Add(Me.m_hdrDefineConnections)
            Me.Controls.Add(Me.m_hdrSharing)
            Me.Controls.Add(Me.m_hdrExisting)
            Me.Controls.Add(Me.m_lblNew)
            Me.Controls.Add(Me.m_cmbTemplates)
            Me.Controls.Add(Me.m_cbEnableIndexing)
            Me.Controls.Add(Me.m_btnOK)
            Me.Controls.Add(Me.m_gridDatasets)
            Me.Controls.Add(Me.m_btnManageConfigurations)
            Me.Controls.Add(Me.m_btnCreate)
            Me.Controls.Add(Me.m_btnConfigure)
            Me.Controls.Add(Me.m_btnExport)
            Me.Controls.Add(Me.m_btnDelete)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgDefineExternalSpatialData"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.TabText = ""
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_btnDelete As System.Windows.Forms.Button
        Private WithEvents m_gridDatasets As gridDefineExternalSpatialData
        Private WithEvents m_btnOK As System.Windows.Forms.Button
        Private WithEvents m_btnConfigure As System.Windows.Forms.Button
        Private WithEvents m_cbEnableIndexing As System.Windows.Forms.CheckBox
        Private WithEvents m_btnCreate As System.Windows.Forms.Button
        Private WithEvents m_cmbTemplates As System.Windows.Forms.ComboBox
        Private WithEvents m_lblNew As System.Windows.Forms.Label
        Private WithEvents m_hdrExisting As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_btnExport As System.Windows.Forms.Button
        Private WithEvents m_lblConfig As System.Windows.Forms.Label
        Private WithEvents m_lblConfigValue As System.Windows.Forms.Label
        Private WithEvents m_hdrDefineConnections As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_btnManageConfigurations As System.Windows.Forms.Button
        Private WithEvents m_hdrSharing As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    End Class

End Namespace
