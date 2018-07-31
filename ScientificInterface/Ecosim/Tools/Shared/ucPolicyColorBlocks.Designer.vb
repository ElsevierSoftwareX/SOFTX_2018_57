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

Namespace Ecosim

    Partial Class ucPolicyColorBlocks
        Inherits System.Windows.Forms.UserControl


        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        '<System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.m_pbFishingBlocks = New System.Windows.Forms.PictureBox()
            Me.m_lblBlockHeader = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_nudSeqEndYear = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblEndYear = New System.Windows.Forms.Label()
            Me.m_nudSeqStartYear = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblStartYear = New System.Windows.Forms.Label()
            Me.m_nudNumYearsPerBlock = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_btnSetGear = New System.Windows.Forms.Button()
            Me.m_tlpMain = New System.Windows.Forms.TableLayoutPanel()
            Me.m_hdrControls = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_pnlControls = New System.Windows.Forms.Panel()
            Me.m_lblYear = New System.Windows.Forms.Label()
            Me.m_plBlocks = New System.Windows.Forms.Panel()
            Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plScroll = New System.Windows.Forms.Panel()
            CType(Me.m_pbFishingBlocks, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudSeqEndYear, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudSeqStartYear, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudNumYearsPerBlock, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tlpMain.SuspendLayout()
            Me.m_pnlControls.SuspendLayout()
            Me.m_plScroll.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_pbFishingBlocks
            '
            Me.m_pbFishingBlocks.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_pbFishingBlocks.Location = New System.Drawing.Point(0, 0)
            Me.m_pbFishingBlocks.Margin = New System.Windows.Forms.Padding(0)
            Me.m_pbFishingBlocks.Name = "m_pbFishingBlocks"
            Me.m_pbFishingBlocks.Size = New System.Drawing.Size(866, 512)
            Me.m_pbFishingBlocks.TabIndex = 1
            Me.m_pbFishingBlocks.TabStop = False
            '
            'm_lblBlockHeader
            '
            Me.m_lblBlockHeader.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_lblBlockHeader.CanCollapseParent = False
            Me.m_lblBlockHeader.CollapsedParentHeight = 0
            Me.m_lblBlockHeader.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_lblBlockHeader.IsCollapsed = False
            Me.m_lblBlockHeader.Location = New System.Drawing.Point(0, 0)
            Me.m_lblBlockHeader.Margin = New System.Windows.Forms.Padding(0)
            Me.m_lblBlockHeader.Name = "m_lblBlockHeader"
            Me.m_lblBlockHeader.Size = New System.Drawing.Size(686, 17)
            Me.m_lblBlockHeader.TabIndex = 0
            Me.m_lblBlockHeader.Text = "Blocks"
            Me.m_lblBlockHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_nudSeqEndYear
            '
            Me.m_nudSeqEndYear.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudSeqEndYear.Location = New System.Drawing.Point(128, 29)
            Me.m_nudSeqEndYear.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
            Me.m_nudSeqEndYear.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudSeqEndYear.Name = "m_nudSeqEndYear"
            Me.m_nudSeqEndYear.Size = New System.Drawing.Size(50, 20)
            Me.m_nudSeqEndYear.TabIndex = 6
            Me.m_nudSeqEndYear.Value = New Decimal(New Integer() {2, 0, 0, 0})
            '
            'm_lblEndYear
            '
            Me.m_lblEndYear.AutoSize = True
            Me.m_lblEndYear.Location = New System.Drawing.Point(93, 31)
            Me.m_lblEndYear.Name = "m_lblEndYear"
            Me.m_lblEndYear.Size = New System.Drawing.Size(29, 13)
            Me.m_lblEndYear.TabIndex = 5
            Me.m_lblEndYear.Text = "End:"
            '
            'm_nudSeqStartYear
            '
            Me.m_nudSeqStartYear.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudSeqStartYear.Location = New System.Drawing.Point(38, 29)
            Me.m_nudSeqStartYear.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
            Me.m_nudSeqStartYear.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudSeqStartYear.Name = "m_nudSeqStartYear"
            Me.m_nudSeqStartYear.Size = New System.Drawing.Size(50, 20)
            Me.m_nudSeqStartYear.TabIndex = 4
            Me.m_nudSeqStartYear.Value = New Decimal(New Integer() {2, 0, 0, 0})
            '
            'm_lblStartYear
            '
            Me.m_lblStartYear.AutoSize = True
            Me.m_lblStartYear.Location = New System.Drawing.Point(0, 31)
            Me.m_lblStartYear.Name = "m_lblStartYear"
            Me.m_lblStartYear.Size = New System.Drawing.Size(32, 13)
            Me.m_lblStartYear.TabIndex = 3
            Me.m_lblStartYear.Text = "Start:"
            '
            'm_nudNumYearsPerBlock
            '
            Me.m_nudNumYearsPerBlock.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudNumYearsPerBlock.Location = New System.Drawing.Point(38, 3)
            Me.m_nudNumYearsPerBlock.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
            Me.m_nudNumYearsPerBlock.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudNumYearsPerBlock.Name = "m_nudNumYearsPerBlock"
            Me.m_nudNumYearsPerBlock.Size = New System.Drawing.Size(50, 20)
            Me.m_nudNumYearsPerBlock.TabIndex = 1
            Me.m_nudNumYearsPerBlock.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_btnSetGear
            '
            Me.m_btnSetGear.Location = New System.Drawing.Point(96, 3)
            Me.m_btnSetGear.Name = "m_btnSetGear"
            Me.m_btnSetGear.Size = New System.Drawing.Size(82, 20)
            Me.m_btnSetGear.TabIndex = 2
            Me.m_btnSetGear.Text = "&Set"
            Me.m_btnSetGear.UseVisualStyleBackColor = True
            '
            'm_tlpMain
            '
            Me.m_tlpMain.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_tlpMain.ColumnCount = 2
            Me.m_tlpMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.m_tlpMain.Controls.Add(Me.m_hdrControls, 1, 0)
            Me.m_tlpMain.Controls.Add(Me.m_pnlControls, 1, 1)
            Me.m_tlpMain.Controls.Add(Me.m_lblBlockHeader, 0, 0)
            Me.m_tlpMain.Controls.Add(Me.m_plBlocks, 0, 1)
            Me.m_tlpMain.Location = New System.Drawing.Point(0, 0)
            Me.m_tlpMain.Margin = New System.Windows.Forms.Padding(0)
            Me.m_tlpMain.Name = "m_tlpMain"
            Me.m_tlpMain.RowCount = 2
            Me.m_tlpMain.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.m_tlpMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpMain.Size = New System.Drawing.Size(870, 69)
            Me.m_tlpMain.TabIndex = 0
            '
            'm_hdrControls
            '
            Me.m_hdrControls.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrControls.CanCollapseParent = False
            Me.m_hdrControls.CollapsedParentHeight = 0
            Me.m_hdrControls.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_hdrControls.IsCollapsed = False
            Me.m_hdrControls.Location = New System.Drawing.Point(689, 0)
            Me.m_hdrControls.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
            Me.m_hdrControls.Name = "m_hdrControls"
            Me.m_hdrControls.Size = New System.Drawing.Size(181, 17)
            Me.m_hdrControls.TabIndex = 2
            Me.m_hdrControls.Text = "Set block years and sequence"
            Me.m_hdrControls.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_pnlControls
            '
            Me.m_pnlControls.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_pnlControls.Controls.Add(Me.m_nudSeqEndYear)
            Me.m_pnlControls.Controls.Add(Me.m_lblEndYear)
            Me.m_pnlControls.Controls.Add(Me.m_nudSeqStartYear)
            Me.m_pnlControls.Controls.Add(Me.m_btnSetGear)
            Me.m_pnlControls.Controls.Add(Me.m_lblYear)
            Me.m_pnlControls.Controls.Add(Me.m_lblStartYear)
            Me.m_pnlControls.Controls.Add(Me.m_nudNumYearsPerBlock)
            Me.m_pnlControls.Location = New System.Drawing.Point(689, 17)
            Me.m_pnlControls.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
            Me.m_pnlControls.Name = "m_pnlControls"
            Me.m_pnlControls.Size = New System.Drawing.Size(181, 52)
            Me.m_pnlControls.TabIndex = 3
            '
            'm_lblYear
            '
            Me.m_lblYear.AutoSize = True
            Me.m_lblYear.Location = New System.Drawing.Point(0, 5)
            Me.m_lblYear.Name = "m_lblYear"
            Me.m_lblYear.Size = New System.Drawing.Size(32, 13)
            Me.m_lblYear.TabIndex = 0
            Me.m_lblYear.Text = "&Year:"
            '
            'm_plBlocks
            '
            Me.m_plBlocks.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_plBlocks.Location = New System.Drawing.Point(0, 17)
            Me.m_plBlocks.Margin = New System.Windows.Forms.Padding(0)
            Me.m_plBlocks.Name = "m_plBlocks"
            Me.m_plBlocks.Size = New System.Drawing.Size(686, 52)
            Me.m_plBlocks.TabIndex = 1
            '
            'TableLayoutPanel3
            '
            Me.TableLayoutPanel3.ColumnCount = 4
            Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
            Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
            Me.TableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TableLayoutPanel3.Location = New System.Drawing.Point(0, 0)
            Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
            Me.TableLayoutPanel3.RowCount = 1
            Me.TableLayoutPanel3.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
            Me.TableLayoutPanel3.Size = New System.Drawing.Size(200, 100)
            Me.TableLayoutPanel3.TabIndex = 0
            '
            'm_plScroll
            '
            Me.m_plScroll.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_plScroll.AutoScroll = True
            Me.m_plScroll.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_plScroll.Controls.Add(Me.m_pbFishingBlocks)
            Me.m_plScroll.Location = New System.Drawing.Point(0, 72)
            Me.m_plScroll.Margin = New System.Windows.Forms.Padding(0)
            Me.m_plScroll.Name = "m_plScroll"
            Me.m_plScroll.Size = New System.Drawing.Size(870, 516)
            Me.m_plScroll.TabIndex = 2
            '
            'ucPolicyColorBlocks
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_plScroll)
            Me.Controls.Add(Me.m_tlpMain)
            Me.Name = "ucPolicyColorBlocks"
            Me.Size = New System.Drawing.Size(870, 588)
            CType(Me.m_pbFishingBlocks, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudSeqEndYear, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudSeqStartYear, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudNumYearsPerBlock, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tlpMain.ResumeLayout(False)
            Me.m_pnlControls.ResumeLayout(False)
            Me.m_pnlControls.PerformLayout()
            Me.m_plScroll.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_pbFishingBlocks As System.Windows.Forms.PictureBox
        Private WithEvents m_btnSetGear As System.Windows.Forms.Button
        Private WithEvents m_lblEndYear As System.Windows.Forms.Label
        Private WithEvents m_lblStartYear As System.Windows.Forms.Label
        Private WithEvents m_lblBlockHeader As cEwEHeaderLabel
        Private WithEvents TableLayoutPanel3 As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_hdrControls As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_tlpMain As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_pnlControls As System.Windows.Forms.Panel
        Private WithEvents m_plBlocks As System.Windows.Forms.Panel
        Private WithEvents m_lblYear As System.Windows.Forms.Label
        Private WithEvents m_plScroll As System.Windows.Forms.Panel
        Private WithEvents m_nudNumYearsPerBlock As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudSeqEndYear As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudSeqStartYear As ScientificInterfaceShared.Controls.cEwENumericUpDown

    End Class

End Namespace