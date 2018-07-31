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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

Imports ScientificInterfaceShared.Forms
Imports ScientificInterfaceShared.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDistributionParameters
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDistributionParameters))
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_btnSave = New System.Windows.Forms.Button()
        Me.m_grid = New EwEMSEPlugin.gridDistributionParameters()
        Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tslModel = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscmPathOrSim = New System.Windows.Forms.ToolStripComboBox()
        Me.m_tslVariable = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscmParamName = New System.Windows.Forms.ToolStripComboBox()
        Me.m_tsMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_btnCancel
        '
        resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
        Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'm_btnSave
        '
        resources.ApplyResources(Me.m_btnSave, "m_btnSave")
        Me.m_btnSave.Name = "m_btnSave"
        Me.m_btnSave.UseVisualStyleBackColor = True
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = False
        resources.ApplyResources(Me.m_grid, "m_grid")
        Me.m_grid.AutoSizeMinHeight = 10
        Me.m_grid.AutoSizeMinWidth = 10
        Me.m_grid.AutoStretchColumnsToFitWidth = True
        Me.m_grid.AutoStretchRowsToFitHeight = False
        Me.m_grid.BackColor = System.Drawing.Color.White
        Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_grid.CustomSort = False
        Me.m_grid.Data = Nothing
        Me.m_grid.DataName = "grid content"
        Me.m_grid.FixedColumnWidths = False
        Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_grid.GridToolTipActive = True
        Me.m_grid.IsLayoutSuspended = False
        Me.m_grid.Mode = EwEMSEPlugin.frmDistributionParameters.eParameterSet.Ecopath
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
        resources.ApplyResources(Me.m_tsMain, "m_tsMain")
        Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tslModel, Me.m_tscmPathOrSim, Me.m_tslVariable, Me.m_tscmParamName})
        Me.m_tsMain.Name = "m_tsMain"
        Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tslModel
        '
        Me.m_tslModel.Name = "m_tslModel"
        resources.ApplyResources(Me.m_tslModel, "m_tslModel")
        '
        'm_tscmPathOrSim
        '
        Me.m_tscmPathOrSim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscmPathOrSim.Name = "m_tscmPathOrSim"
        Me.m_tscmPathOrSim.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never
        resources.ApplyResources(Me.m_tscmPathOrSim, "m_tscmPathOrSim")
        '
        'm_tslVariable
        '
        Me.m_tslVariable.Name = "m_tslVariable"
        resources.ApplyResources(Me.m_tslVariable, "m_tslVariable")
        '
        'm_tscmParamName
        '
        Me.m_tscmParamName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscmParamName.Name = "m_tscmParamName"
        Me.m_tscmParamName.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never
        resources.ApplyResources(Me.m_tscmParamName, "m_tscmParamName")
        '
        'frmDistributionParameters
        '
        Me.AcceptButton = Me.m_btnSave
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_tsMain)
        Me.Controls.Add(Me.m_grid)
        Me.Controls.Add(Me.m_btnSave)
        Me.Controls.Add(Me.m_btnCancel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MinimizeBox = False
        Me.Name = "frmDistributionParameters"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.m_tsMain.ResumeLayout(False)
        Me.m_tsMain.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_btnSave As System.Windows.Forms.Button
    Private WithEvents m_grid As gridDistributionParameters
    Private WithEvents m_tsMain As cEwEToolstrip
    Private WithEvents m_tscmPathOrSim As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_tscmParamName As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_tslModel As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tslVariable As System.Windows.Forms.ToolStripLabel

End Class
