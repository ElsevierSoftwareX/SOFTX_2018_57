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


Partial Class frmShapeValue
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmShapeValue))
        Me.m_lblName = New System.Windows.Forms.Label()
        Me.m_lblPoolCode = New System.Windows.Forms.Label()
        Me.m_lblType = New System.Windows.Forms.Label()
        Me.m_txtName = New System.Windows.Forms.TextBox()
        Me.m_cmbType = New System.Windows.Forms.ComboBox()
        Me.m_cmbPoolCode = New System.Windows.Forms.ComboBox()
        Me.m_lblWeight = New System.Windows.Forms.Label()
        Me.m_txtWeight = New System.Windows.Forms.TextBox()
        Me.m_lblValues = New System.Windows.Forms.Label()
        Me.m_lblNoOfPoints = New System.Windows.Forms.Label()
        Me.m_lblNumPoints = New System.Windows.Forms.Label()
        Me.m_tlpAll = New System.Windows.Forms.TableLayoutPanel()
        Me.m_tlpNoOfYears = New System.Windows.Forms.TableLayoutPanel()
        Me.m_btnSetNoOfYears = New System.Windows.Forms.Button()
        Me.pnlValueGrid = New System.Windows.Forms.Panel()
        Me.m_grid = New ScientificInterfaceShared.gridShapeValue()
        Me.m_lblViewAs = New System.Windows.Forms.Label()
        Me.m_cmbViewAs = New System.Windows.Forms.ComboBox()
        Me.m_lblXBase = New System.Windows.Forms.Label()
        Me.m_txtXBase = New System.Windows.Forms.TextBox()
        Me.m_lblPoolCodeSec = New System.Windows.Forms.Label()
        Me.m_cmbPoolCodeSec = New System.Windows.Forms.ComboBox()
        Me.m_btnOK = New System.Windows.Forms.Button()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_tlpAll.SuspendLayout()
        Me.m_tlpNoOfYears.SuspendLayout()
        Me.pnlValueGrid.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_lblName
        '
        resources.ApplyResources(Me.m_lblName, "m_lblName")
        Me.m_lblName.Name = "m_lblName"
        '
        'm_lblPoolCode
        '
        resources.ApplyResources(Me.m_lblPoolCode, "m_lblPoolCode")
        Me.m_lblPoolCode.Name = "m_lblPoolCode"
        '
        'm_lblType
        '
        resources.ApplyResources(Me.m_lblType, "m_lblType")
        Me.m_lblType.Name = "m_lblType"
        '
        'm_txtName
        '
        resources.ApplyResources(Me.m_txtName, "m_txtName")
        Me.m_txtName.Name = "m_txtName"
        '
        'm_cmbType
        '
        resources.ApplyResources(Me.m_cmbType, "m_cmbType")
        Me.m_cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbType.FormattingEnabled = True
        Me.m_cmbType.Name = "m_cmbType"
        '
        'm_cmbPoolCode
        '
        resources.ApplyResources(Me.m_cmbPoolCode, "m_cmbPoolCode")
        Me.m_cmbPoolCode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbPoolCode.FormattingEnabled = True
        Me.m_cmbPoolCode.Name = "m_cmbPoolCode"
        '
        'm_lblWeight
        '
        resources.ApplyResources(Me.m_lblWeight, "m_lblWeight")
        Me.m_lblWeight.Name = "m_lblWeight"
        '
        'm_txtWeight
        '
        resources.ApplyResources(Me.m_txtWeight, "m_txtWeight")
        Me.m_txtWeight.Name = "m_txtWeight"
        '
        'm_lblValues
        '
        resources.ApplyResources(Me.m_lblValues, "m_lblValues")
        Me.m_lblValues.Name = "m_lblValues"
        '
        'm_lblNoOfPoints
        '
        resources.ApplyResources(Me.m_lblNoOfPoints, "m_lblNoOfPoints")
        Me.m_lblNoOfPoints.Name = "m_lblNoOfPoints"
        '
        'm_lblNumPoints
        '
        resources.ApplyResources(Me.m_lblNumPoints, "m_lblNumPoints")
        Me.m_lblNumPoints.Name = "m_lblNumPoints"
        '
        'm_tlpAll
        '
        resources.ApplyResources(Me.m_tlpAll, "m_tlpAll")
        Me.m_tlpAll.Controls.Add(Me.m_txtName, 1, 0)
        Me.m_tlpAll.Controls.Add(Me.m_cmbType, 1, 1)
        Me.m_tlpAll.Controls.Add(Me.m_lblValues, 0, 7)
        Me.m_tlpAll.Controls.Add(Me.m_lblType, 0, 1)
        Me.m_tlpAll.Controls.Add(Me.m_lblPoolCode, 0, 2)
        Me.m_tlpAll.Controls.Add(Me.m_cmbPoolCode, 1, 2)
        Me.m_tlpAll.Controls.Add(Me.m_txtWeight, 1, 4)
        Me.m_tlpAll.Controls.Add(Me.m_lblName, 0, 0)
        Me.m_tlpAll.Controls.Add(Me.m_lblWeight, 0, 4)
        Me.m_tlpAll.Controls.Add(Me.m_lblNoOfPoints, 0, 8)
        Me.m_tlpAll.Controls.Add(Me.m_tlpNoOfYears, 1, 8)
        Me.m_tlpAll.Controls.Add(Me.pnlValueGrid, 1, 7)
        Me.m_tlpAll.Controls.Add(Me.m_lblViewAs, 0, 6)
        Me.m_tlpAll.Controls.Add(Me.m_cmbViewAs, 1, 6)
        Me.m_tlpAll.Controls.Add(Me.m_lblXBase, 0, 5)
        Me.m_tlpAll.Controls.Add(Me.m_txtXBase, 1, 5)
        Me.m_tlpAll.Controls.Add(Me.m_lblPoolCodeSec, 0, 3)
        Me.m_tlpAll.Controls.Add(Me.m_cmbPoolCodeSec, 1, 3)
        Me.m_tlpAll.Name = "m_tlpAll"
        '
        'm_tlpNoOfYears
        '
        resources.ApplyResources(Me.m_tlpNoOfYears, "m_tlpNoOfYears")
        Me.m_tlpNoOfYears.Controls.Add(Me.m_lblNumPoints, 0, 0)
        Me.m_tlpNoOfYears.Controls.Add(Me.m_btnSetNoOfYears, 1, 0)
        Me.m_tlpNoOfYears.Name = "m_tlpNoOfYears"
        '
        'm_btnSetNoOfYears
        '
        resources.ApplyResources(Me.m_btnSetNoOfYears, "m_btnSetNoOfYears")
        Me.m_btnSetNoOfYears.Name = "m_btnSetNoOfYears"
        Me.m_btnSetNoOfYears.UseVisualStyleBackColor = True
        '
        'pnlValueGrid
        '
        resources.ApplyResources(Me.pnlValueGrid, "pnlValueGrid")
        Me.pnlValueGrid.Controls.Add(Me.m_grid)
        Me.pnlValueGrid.Name = "pnlValueGrid"
        Me.pnlValueGrid.TabStop = True
        '
        'm_grid
        '
        Me.m_grid.AllowBlockSelect = False
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
        Me.m_grid.DataName = "grid content"
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
        Me.m_grid.TrackPropertySelection = False
        Me.m_grid.UIContext = Nothing
        '
        'm_lblViewAs
        '
        resources.ApplyResources(Me.m_lblViewAs, "m_lblViewAs")
        Me.m_lblViewAs.Name = "m_lblViewAs"
        '
        'm_cmbViewAs
        '
        resources.ApplyResources(Me.m_cmbViewAs, "m_cmbViewAs")
        Me.m_cmbViewAs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbViewAs.FormattingEnabled = True
        Me.m_cmbViewAs.Items.AddRange(New Object() {resources.GetString("m_cmbViewAs.Items"), resources.GetString("m_cmbViewAs.Items1")})
        Me.m_cmbViewAs.Name = "m_cmbViewAs"
        '
        'm_lblXBase
        '
        resources.ApplyResources(Me.m_lblXBase, "m_lblXBase")
        Me.m_lblXBase.Name = "m_lblXBase"
        '
        'm_txtXBase
        '
        resources.ApplyResources(Me.m_txtXBase, "m_txtXBase")
        Me.m_txtXBase.Name = "m_txtXBase"
        '
        'm_lblPoolCodeSec
        '
        resources.ApplyResources(Me.m_lblPoolCodeSec, "m_lblPoolCodeSec")
        Me.m_lblPoolCodeSec.Name = "m_lblPoolCodeSec"
        '
        'm_cmbPoolCodeSec
        '
        resources.ApplyResources(Me.m_cmbPoolCodeSec, "m_cmbPoolCodeSec")
        Me.m_cmbPoolCodeSec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbPoolCodeSec.FormattingEnabled = True
        Me.m_cmbPoolCodeSec.Name = "m_cmbPoolCodeSec"
        '
        'm_btnOK
        '
        resources.ApplyResources(Me.m_btnOK, "m_btnOK")
        Me.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.m_btnOK.Name = "m_btnOK"
        Me.m_btnOK.UseVisualStyleBackColor = True
        '
        'm_btnCancel
        '
        resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
        Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'frmShapeValue
        '
        Me.AcceptButton = Me.m_btnOK
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_btnOK)
        Me.Controls.Add(Me.m_tlpAll)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmShapeValue"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.m_tlpAll.ResumeLayout(False)
        Me.m_tlpAll.PerformLayout()
        Me.m_tlpNoOfYears.ResumeLayout(False)
        Me.pnlValueGrid.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_lblName As System.Windows.Forms.Label
    Private WithEvents m_lblPoolCode As System.Windows.Forms.Label
    Private WithEvents m_lblType As System.Windows.Forms.Label
    Private WithEvents m_txtName As System.Windows.Forms.TextBox
    Private WithEvents m_cmbType As System.Windows.Forms.ComboBox
    Private WithEvents m_cmbPoolCode As System.Windows.Forms.ComboBox
    Private WithEvents m_lblWeight As System.Windows.Forms.Label
    Private WithEvents m_txtWeight As System.Windows.Forms.TextBox
    Private WithEvents m_lblValues As System.Windows.Forms.Label
    Private WithEvents m_lblNoOfPoints As System.Windows.Forms.Label
    Private WithEvents m_lblNumPoints As Label
    Private WithEvents m_tlpAll As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_tlpNoOfYears As System.Windows.Forms.TableLayoutPanel
    Private WithEvents pnlValueGrid As System.Windows.Forms.Panel
    Private WithEvents m_btnOK As System.Windows.Forms.Button
    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_lblViewAs As System.Windows.Forms.Label
    Private WithEvents m_cmbViewAs As System.Windows.Forms.ComboBox
    Private WithEvents m_lblXBase As System.Windows.Forms.Label
    Private WithEvents m_txtXBase As System.Windows.Forms.TextBox
    Private WithEvents m_btnSetNoOfYears As System.Windows.Forms.Button
    Private WithEvents m_grid As ScientificInterfaceShared.gridShapeValue
    Private WithEvents m_lblPoolCodeSec As Label
    Private WithEvents m_cmbPoolCodeSec As ComboBox
End Class

