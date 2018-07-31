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

Partial Class dlgEditBasemap
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgEditBasemap))
        Me.m_tlpButtons = New System.Windows.Forms.TableLayoutPanel()
        Me.m_btnOk = New System.Windows.Forms.Button()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_NumRows = New System.Windows.Forms.Label()
        Me.m_lblNumCols = New System.Windows.Forms.Label()
        Me.m_nudRowCount = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudColCount = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_lblCellLength = New System.Windows.Forms.Label()
        Me.m_pbLink = New System.Windows.Forms.PictureBox()
        Me.m_lblLonTL = New System.Windows.Forms.Label()
        Me.m_nudCellSize = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudCellLength = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudLonTL = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudLatTL = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_lblLatTL = New System.Windows.Forms.Label()
        Me.m_lblCellSize = New System.Windows.Forms.Label()
        Me.m_hdrDimensions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrSpatialReference = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_cbAssumeSquareCells = New System.Windows.Forms.CheckBox()
        Me.m_lblUnitLat = New System.Windows.Forms.Label()
        Me.m_lblUnitLon = New System.Windows.Forms.Label()
        Me.m_lblUnitCellLen = New System.Windows.Forms.Label()
        Me.m_lblUnitCellSize = New System.Windows.Forms.Label()
        Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tsbnImport = New System.Windows.Forms.ToolStripDropDownButton()
        Me.m_tsmiImportFromASCII = New System.Windows.Forms.ToolStripMenuItem()
        Me.m_tlpButtons.SuspendLayout()
        CType(Me.m_nudRowCount, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudColCount, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbLink, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudCellSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudCellLength, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudLonTL, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudLatTL, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_ts.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_tlpButtons
        '
        resources.ApplyResources(Me.m_tlpButtons, "m_tlpButtons")
        Me.m_tlpButtons.Controls.Add(Me.m_btnOk, 0, 0)
        Me.m_tlpButtons.Controls.Add(Me.m_btnCancel, 1, 0)
        Me.m_tlpButtons.Name = "m_tlpButtons"
        '
        'm_btnOk
        '
        resources.ApplyResources(Me.m_btnOk, "m_btnOk")
        Me.m_btnOk.Name = "m_btnOk"
        '
        'm_btnCancel
        '
        resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
        Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.m_btnCancel.Name = "m_btnCancel"
        '
        'm_NumRows
        '
        resources.ApplyResources(Me.m_NumRows, "m_NumRows")
        Me.m_NumRows.Name = "m_NumRows"
        '
        'm_lblNumCols
        '
        resources.ApplyResources(Me.m_lblNumCols, "m_lblNumCols")
        Me.m_lblNumCols.Name = "m_lblNumCols"
        '
        'm_nudRowCount
        '
        resources.ApplyResources(Me.m_nudRowCount, "m_nudRowCount")
        Me.m_nudRowCount.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudRowCount.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudRowCount.Name = "m_nudRowCount"
        Me.m_nudRowCount.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_nudColCount
        '
        resources.ApplyResources(Me.m_nudColCount, "m_nudColCount")
        Me.m_nudColCount.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudColCount.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudColCount.Name = "m_nudColCount"
        Me.m_nudColCount.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_lblCellLength
        '
        resources.ApplyResources(Me.m_lblCellLength, "m_lblCellLength")
        Me.m_lblCellLength.Name = "m_lblCellLength"
        '
        'm_pbLink
        '
        resources.ApplyResources(Me.m_pbLink, "m_pbLink")
        Me.m_pbLink.Name = "m_pbLink"
        Me.m_pbLink.TabStop = False
        '
        'm_lblLonTL
        '
        resources.ApplyResources(Me.m_lblLonTL, "m_lblLonTL")
        Me.m_lblLonTL.Name = "m_lblLonTL"
        '
        'm_nudCellSize
        '
        resources.ApplyResources(Me.m_nudCellSize, "m_nudCellSize")
        Me.m_nudCellSize.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudCellSize.Minimum = New Decimal(New Integer() {1, 0, 0, 196608})
        Me.m_nudCellSize.Name = "m_nudCellSize"
        Me.m_nudCellSize.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_nudCellLength
        '
        resources.ApplyResources(Me.m_nudCellLength, "m_nudCellLength")
        Me.m_nudCellLength.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudCellLength.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudCellLength.Name = "m_nudCellLength"
        Me.m_nudCellLength.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_nudLonTL
        '
        resources.ApplyResources(Me.m_nudLonTL, "m_nudLonTL")
        Me.m_nudLonTL.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudLonTL.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudLonTL.Name = "m_nudLonTL"
        Me.m_nudLonTL.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_nudLatTL
        '
        resources.ApplyResources(Me.m_nudLatTL, "m_nudLatTL")
        Me.m_nudLatTL.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudLatTL.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudLatTL.Name = "m_nudLatTL"
        Me.m_nudLatTL.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'm_lblLatTL
        '
        resources.ApplyResources(Me.m_lblLatTL, "m_lblLatTL")
        Me.m_lblLatTL.Name = "m_lblLatTL"
        '
        'm_lblCellSize
        '
        resources.ApplyResources(Me.m_lblCellSize, "m_lblCellSize")
        Me.m_lblCellSize.Name = "m_lblCellSize"
        '
        'm_hdrDimensions
        '
        resources.ApplyResources(Me.m_hdrDimensions, "m_hdrDimensions")
        Me.m_hdrDimensions.CanCollapseParent = False
        Me.m_hdrDimensions.CollapsedParentHeight = 0
        Me.m_hdrDimensions.IsCollapsed = False
        Me.m_hdrDimensions.Name = "m_hdrDimensions"
        '
        'm_hdrSpatialReference
        '
        resources.ApplyResources(Me.m_hdrSpatialReference, "m_hdrSpatialReference")
        Me.m_hdrSpatialReference.CanCollapseParent = False
        Me.m_hdrSpatialReference.CollapsedParentHeight = 0
        Me.m_hdrSpatialReference.IsCollapsed = False
        Me.m_hdrSpatialReference.Name = "m_hdrSpatialReference"
        '
        'm_cbAssumeSquareCells
        '
        resources.ApplyResources(Me.m_cbAssumeSquareCells, "m_cbAssumeSquareCells")
        Me.m_cbAssumeSquareCells.Name = "m_cbAssumeSquareCells"
        Me.m_cbAssumeSquareCells.UseVisualStyleBackColor = True
        '
        'm_lblUnitLat
        '
        resources.ApplyResources(Me.m_lblUnitLat, "m_lblUnitLat")
        Me.m_lblUnitLat.Name = "m_lblUnitLat"
        '
        'm_lblUnitLon
        '
        resources.ApplyResources(Me.m_lblUnitLon, "m_lblUnitLon")
        Me.m_lblUnitLon.Name = "m_lblUnitLon"
        '
        'm_lblUnitCellLen
        '
        resources.ApplyResources(Me.m_lblUnitCellLen, "m_lblUnitCellLen")
        Me.m_lblUnitCellLen.Name = "m_lblUnitCellLen"
        '
        'm_lblUnitCellSize
        '
        resources.ApplyResources(Me.m_lblUnitCellSize, "m_lblUnitCellSize")
        Me.m_lblUnitCellSize.Name = "m_lblUnitCellSize"
        '
        'm_ts
        '
        Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnImport})
        resources.ApplyResources(Me.m_ts, "m_ts")
        Me.m_ts.Name = "m_ts"
        Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tsbnImport
        '
        Me.m_tsbnImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.m_tsbnImport.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiImportFromASCII})
        resources.ApplyResources(Me.m_tsbnImport, "m_tsbnImport")
        Me.m_tsbnImport.Name = "m_tsbnImport"
        '
        'm_tsmiImportFromASCII
        '
        Me.m_tsmiImportFromASCII.Name = "m_tsmiImportFromASCII"
        resources.ApplyResources(Me.m_tsmiImportFromASCII, "m_tsmiImportFromASCII")
        '
        'dlgEditBasemap
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_ts)
        Me.Controls.Add(Me.m_lblUnitCellSize)
        Me.Controls.Add(Me.m_lblUnitCellLen)
        Me.Controls.Add(Me.m_lblUnitLon)
        Me.Controls.Add(Me.m_lblUnitLat)
        Me.Controls.Add(Me.m_cbAssumeSquareCells)
        Me.Controls.Add(Me.m_pbLink)
        Me.Controls.Add(Me.m_hdrSpatialReference)
        Me.Controls.Add(Me.m_lblLonTL)
        Me.Controls.Add(Me.m_hdrDimensions)
        Me.Controls.Add(Me.m_nudCellSize)
        Me.Controls.Add(Me.m_NumRows)
        Me.Controls.Add(Me.m_nudCellLength)
        Me.Controls.Add(Me.m_nudLonTL)
        Me.Controls.Add(Me.m_nudRowCount)
        Me.Controls.Add(Me.m_nudLatTL)
        Me.Controls.Add(Me.m_lblLatTL)
        Me.Controls.Add(Me.m_lblNumCols)
        Me.Controls.Add(Me.m_lblCellSize)
        Me.Controls.Add(Me.m_tlpButtons)
        Me.Controls.Add(Me.m_lblCellLength)
        Me.Controls.Add(Me.m_nudColCount)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgEditBasemap"
        Me.ShowInTaskbar = False
        Me.m_tlpButtons.ResumeLayout(False)
        CType(Me.m_nudRowCount, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudColCount, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbLink, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudCellSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudCellLength, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudLonTL, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudLatTL, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_ts.ResumeLayout(False)
        Me.m_ts.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_btnOk As System.Windows.Forms.Button
    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_NumRows As System.Windows.Forms.Label
    Private WithEvents m_lblNumCols As System.Windows.Forms.Label
    Private WithEvents m_lblCellLength As System.Windows.Forms.Label
    Private WithEvents m_lblLonTL As System.Windows.Forms.Label
    Private WithEvents m_lblLatTL As System.Windows.Forms.Label
    Private WithEvents m_lblCellSize As System.Windows.Forms.Label
    Private WithEvents m_pbLink As System.Windows.Forms.PictureBox
    Private WithEvents m_hdrDimensions As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_hdrSpatialReference As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_tlpButtons As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_nudRowCount As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudColCount As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudCellLength As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudLonTL As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudLatTL As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudCellSize As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_cbAssumeSquareCells As System.Windows.Forms.CheckBox
    Private WithEvents m_lblUnitLat As System.Windows.Forms.Label
    Private WithEvents m_lblUnitLon As System.Windows.Forms.Label
    Private WithEvents m_lblUnitCellLen As System.Windows.Forms.Label
    Private WithEvents m_lblUnitCellSize As System.Windows.Forms.Label
    Private WithEvents m_ts As ScientificInterfaceShared.Controls.cEwEToolstrip
    Private WithEvents m_tsbnImport As System.Windows.Forms.ToolStripDropDownButton
    Private WithEvents m_tsmiImportFromASCII As System.Windows.Forms.ToolStripMenuItem

End Class
