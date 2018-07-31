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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class dlgEditTransect
    Inherits System.Windows.Forms.Form

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgEditTransect))
        Me.m_lblName = New System.Windows.Forms.Label()
        Me.m_tbxName = New System.Windows.Forms.TextBox()
        Me.m_dgvPos = New System.Windows.Forms.DataGridView()
        Me.m_btnOK = New System.Windows.Forms.Button()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.m_colCellIndex = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.m_colCoordinate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.m_dgvPos, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_lblName
        '
        resources.ApplyResources(Me.m_lblName, "m_lblName")
        Me.m_lblName.Name = "m_lblName"
        '
        'm_tbxName
        '
        resources.ApplyResources(Me.m_tbxName, "m_tbxName")
        Me.m_tbxName.Name = "m_tbxName"
        '
        'm_dgvPos
        '
        Me.m_dgvPos.AllowUserToAddRows = False
        Me.m_dgvPos.AllowUserToDeleteRows = False
        Me.m_dgvPos.AllowUserToResizeRows = False
        resources.ApplyResources(Me.m_dgvPos, "m_dgvPos")
        Me.m_dgvPos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.m_dgvPos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.m_dgvPos.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.m_colName, Me.m_colCellIndex, Me.m_colCoordinate})
        Me.m_dgvPos.Name = "m_dgvPos"
        Me.m_dgvPos.RowHeadersVisible = False
        Me.m_dgvPos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        '
        'm_btnOK
        '
        resources.ApplyResources(Me.m_btnOK, "m_btnOK")
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
        'm_colName
        '
        resources.ApplyResources(Me.m_colName, "m_colName")
        Me.m_colName.Name = "m_colName"
        Me.m_colName.ReadOnly = True
        '
        'm_colCellIndex
        '
        resources.ApplyResources(Me.m_colCellIndex, "m_colCellIndex")
        Me.m_colCellIndex.Name = "m_colCellIndex"
        '
        'm_colCoordinate
        '
        resources.ApplyResources(Me.m_colCoordinate, "m_colCoordinate")
        Me.m_colCoordinate.Name = "m_colCoordinate"
        '
        'dlgEditTransect
        '
        Me.AcceptButton = Me.m_btnOK
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_btnOK)
        Me.Controls.Add(Me.m_dgvPos)
        Me.Controls.Add(Me.m_tbxName)
        Me.Controls.Add(Me.m_lblName)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgEditTransect"
        Me.ShowInTaskbar = False
        CType(Me.m_dgvPos, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents m_tbxName As System.Windows.Forms.TextBox
    Private WithEvents m_lblName As System.Windows.Forms.Label
    Private WithEvents m_dgvPos As System.Windows.Forms.DataGridView
    Friend WithEvents m_btnOK As System.Windows.Forms.Button
    Friend WithEvents m_btnCancel As System.Windows.Forms.Button
    Friend WithEvents m_colName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents m_colCellIndex As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents m_colCoordinate As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
