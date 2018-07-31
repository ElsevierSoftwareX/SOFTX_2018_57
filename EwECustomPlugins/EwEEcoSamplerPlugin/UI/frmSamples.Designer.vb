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

Partial Class frmSamples
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSamples))
        Me.m_tlpOptions = New System.Windows.Forms.TableLayoutPanel()
        Me.m_btnDelete = New System.Windows.Forms.Button()
        Me.m_btnLoad = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.m_tlpMain = New System.Windows.Forms.TableLayoutPanel()
        Me.m_grid = New gridSamples()
        Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tsbnRecord = New System.Windows.Forms.ToolStripButton()
        Me.m_tsddImport = New System.Windows.Forms.ToolStripDropDownButton()
        Me.m_tsmiImportModel = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tsmiImportCefas = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_plBatchRun = New System.Windows.Forms.Panel()
        Me.m_cbBatchRandomize = New System.Windows.Forms.CheckBox()
        Me.m_btnRun = New System.Windows.Forms.Button()
        Me.m_nudNumSamples = New System.Windows.Forms.NumericUpDown()
        Me.m_lblNumSamples = New System.Windows.Forms.Label()
        Me.m_hdrRun = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tlpOptions.SuspendLayout()
        Me.m_tlpMain.SuspendLayout()
        Me.m_tsMain.SuspendLayout()
        Me.m_plBatchRun.SuspendLayout()
        CType(Me.m_nudNumSamples, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_tlpOptions
        '
        resources.ApplyResources(Me.m_tlpOptions, "m_tlpOptions")
        Me.m_tlpOptions.Controls.Add(Me.m_btnDelete, 2, 0)
        Me.m_tlpOptions.Controls.Add(Me.m_btnLoad, 1, 0)
        Me.m_tlpOptions.Controls.Add(Me.Label1, 0, 0)
        Me.m_tlpOptions.Name = "m_tlpOptions"
        '
        'm_btnDelete
        '
        resources.ApplyResources(Me.m_btnDelete, "m_btnDelete")
        Me.m_btnDelete.Name = "m_btnDelete"
        Me.m_btnDelete.UseVisualStyleBackColor = True
        '
        'm_btnLoad
        '
        resources.ApplyResources(Me.m_btnLoad, "m_btnLoad")
        Me.m_btnLoad.Name = "m_btnLoad"
        Me.m_btnLoad.UseVisualStyleBackColor = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'm_tlpMain
        '
        resources.ApplyResources(Me.m_tlpMain, "m_tlpMain")
        Me.m_tlpMain.Controls.Add(Me.m_tlpOptions, 0, 2)
        Me.m_tlpMain.Controls.Add(Me.m_grid, 0, 1)
        Me.m_tlpMain.Controls.Add(Me.m_tsMain, 0, 0)
        Me.m_tlpMain.Controls.Add(Me.m_plBatchRun, 0, 3)
        Me.m_tlpMain.Name = "m_tlpMain"
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = False
        Me.m_grid.AutoSizeMinHeight = 10
        Me.m_grid.AutoSizeMinWidth = 10
        Me.m_grid.AutoStretchColumnsToFitWidth = False
        Me.m_grid.AutoStretchRowsToFitHeight = False
        Me.m_grid.BackColor = System.Drawing.Color.White
        Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_grid.CustomSort = False
        Me.m_grid.DataName = "EcoSampler"
        resources.ApplyResources(Me.m_grid, "m_grid")
        Me.m_grid.FixedColumnWidths = False
        Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_grid.GridToolTipActive = True
        Me.m_grid.IsLayoutSuspended = False
        Me.m_grid.IsOutputGrid = True
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
        Me.m_grid.UIContext = Nothing
        '
        'm_tsMain
        '
        Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnRecord, Me.m_tsddImport})
        resources.ApplyResources(Me.m_tsMain, "m_tsMain")
        Me.m_tsMain.Name = "m_tsMain"
        Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tsbnRecord
        '
        Me.m_tsbnRecord.CheckOnClick = True
        resources.ApplyResources(Me.m_tsbnRecord, "m_tsbnRecord")
        Me.m_tsbnRecord.Name = "m_tsbnRecord"
        '
        'm_tsddImport
        '
        Me.m_tsddImport.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.m_tsddImport.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiImportModel, Me.m_tsmiImportCefas})
        resources.ApplyResources(Me.m_tsddImport, "m_tsddImport")
        Me.m_tsddImport.Name = "m_tsddImport"
        '
        'm_tsmiImportModel
        '
        Me.m_tsmiImportModel.Name = "m_tsmiImportModel"
        resources.ApplyResources(Me.m_tsmiImportModel, "m_tsmiImportModel")
        '
        'm_tsmiImportCefas
        '
        Me.m_tsmiImportCefas.Name = "m_tsmiImportCefas"
        resources.ApplyResources(Me.m_tsmiImportCefas, "m_tsmiImportCefas")
        '
        'm_plBatchRun
        '
        Me.m_plBatchRun.Controls.Add(Me.m_cbBatchRandomize)
        Me.m_plBatchRun.Controls.Add(Me.m_btnRun)
        Me.m_plBatchRun.Controls.Add(Me.m_nudNumSamples)
        Me.m_plBatchRun.Controls.Add(Me.m_lblNumSamples)
        Me.m_plBatchRun.Controls.Add(Me.m_hdrRun)
        resources.ApplyResources(Me.m_plBatchRun, "m_plBatchRun")
        Me.m_plBatchRun.Name = "m_plBatchRun"
        '
        'm_cbBatchRandomize
        '
        resources.ApplyResources(Me.m_cbBatchRandomize, "m_cbBatchRandomize")
        Me.m_cbBatchRandomize.Name = "m_cbBatchRandomize"
        Me.m_cbBatchRandomize.UseVisualStyleBackColor = True
        '
        'm_btnRun
        '
        resources.ApplyResources(Me.m_btnRun, "m_btnRun")
        Me.m_btnRun.Name = "m_btnRun"
        Me.m_btnRun.UseVisualStyleBackColor = True
        '
        'm_nudNumSamples
        '
        resources.ApplyResources(Me.m_nudNumSamples, "m_nudNumSamples")
        Me.m_nudNumSamples.Name = "m_nudNumSamples"
        '
        'm_lblNumSamples
        '
        resources.ApplyResources(Me.m_lblNumSamples, "m_lblNumSamples")
        Me.m_lblNumSamples.Name = "m_lblNumSamples"
        '
        'm_hdrRun
        '
        Me.m_hdrRun.CanCollapseParent = False
        Me.m_hdrRun.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrRun, "m_hdrRun")
        Me.m_hdrRun.IsCollapsed = False
        Me.m_hdrRun.Name = "m_hdrRun"
        '
        'frmSamples
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.m_tlpMain)
        Me.Name = "frmSamples"
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.m_tlpOptions.ResumeLayout(False)
        Me.m_tlpOptions.PerformLayout()
        Me.m_tlpMain.ResumeLayout(False)
        Me.m_tlpMain.PerformLayout()
        Me.m_tsMain.ResumeLayout(False)
        Me.m_tsMain.PerformLayout()
        Me.m_plBatchRun.ResumeLayout(False)
        Me.m_plBatchRun.PerformLayout()
        CType(Me.m_nudNumSamples, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_tlpOptions As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_btnDelete As System.Windows.Forms.Button
    Private WithEvents m_tlpMain As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_grid As gridSamples
    Private WithEvents m_tsMain As cEwEToolstrip
    Private WithEvents m_tsbnRecord As System.Windows.Forms.ToolStripButton
    Private WithEvents m_btnLoad As System.Windows.Forms.Button
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents m_plBatchRun As System.Windows.Forms.Panel
    Private WithEvents m_btnRun As System.Windows.Forms.Button
    Private WithEvents m_nudNumSamples As System.Windows.Forms.NumericUpDown
    Private WithEvents m_lblNumSamples As System.Windows.Forms.Label
    Private WithEvents m_hdrRun As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_tsmiImportModel As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsmiImportCefas As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents m_tsddImport As System.Windows.Forms.ToolStripDropDownButton
    Private WithEvents m_cbBatchRandomize As System.Windows.Forms.CheckBox
End Class
