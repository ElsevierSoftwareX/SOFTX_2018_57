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
Partial Class frmEditAssessmentError
    Inherits frmEwEGrid

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEditAssessmentError))
        Me.m_btnSave = New System.Windows.Forms.Button()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tscbTypes = New System.Windows.Forms.ToolStripComboBox()
        Me.m_grdError = New EwEMSEPlugin.gridErrorCVs()
        Me.m_ts.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_btnSave
        '
        resources.ApplyResources(Me.m_btnSave, "m_btnSave")
        Me.m_btnSave.Name = "m_btnSave"
        Me.m_btnSave.UseVisualStyleBackColor = True
        '
        'm_btnCancel
        '
        resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
        Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'm_ts
        '
        resources.ApplyResources(Me.m_ts, "m_ts")
        Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tscbTypes})
        Me.m_ts.Name = "m_ts"
        Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tscbTypes
        '
        Me.m_tscbTypes.DropDownWidth = 200
        Me.m_tscbTypes.Name = "m_tscbTypes"
        resources.ApplyResources(Me.m_tscbTypes, "m_tscbTypes")
        '
        'm_grdError
        '
        Me.m_grdError.AllowBlockSelect = True
        resources.ApplyResources(Me.m_grdError, "m_grdError")
        Me.m_grdError.AutoSizeMinHeight = 10
        Me.m_grdError.AutoSizeMinWidth = 10
        Me.m_grdError.AutoStretchColumnsToFitWidth = False
        Me.m_grdError.AutoStretchRowsToFitHeight = False
        Me.m_grdError.BackColor = System.Drawing.Color.White
        Me.m_grdError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_grdError.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
        Me.m_grdError.CustomSort = False
        Me.m_grdError.DataName = "grid content"
        Me.m_grdError.ErrorDataType = EwEMSEPlugin.frmEditAssessmentError.eErrorDataType.GroupObervationError
        Me.m_grdError.FixedColumnWidths = False
        Me.m_grdError.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_grdError.GridToolTipActive = True
        Me.m_grdError.IsLayoutSuspended = False
        Me.m_grdError.Name = "m_grdError"
        Me.m_grdError.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_grdError.UIContext = Nothing
        '
        'frmEditAssessmentError
        '
        Me.AcceptButton = Me.m_btnSave
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_grdError)
        Me.Controls.Add(Me.m_ts)
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_btnSave)
        Me.MinimizeBox = False
        Me.Name = "frmEditAssessmentError"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.m_ts.ResumeLayout(False)
        Me.m_ts.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_ts As cEwEToolstrip
    Private WithEvents m_btnSave As System.Windows.Forms.Button
    Friend WithEvents m_grdError As EwEMSEPlugin.gridErrorCVs
    Friend WithEvents m_tscbTypes As System.Windows.Forms.ToolStripComboBox
End Class
