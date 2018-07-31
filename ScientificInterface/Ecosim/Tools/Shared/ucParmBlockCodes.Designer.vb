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

    Partial Class ucParmBlockCodes
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
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
        '<System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.m_pbxBlockCodes = New System.Windows.Forms.PictureBox()
            Me.m_nudNumBlockCodes = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudSelectedBlockCode = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblNumBlocks = New System.Windows.Forms.Label()
            Me.m_lblSelectedBlock = New System.Windows.Forms.Label()
            Me.m_slSelectedBlockCode = New ScientificInterfaceShared.Controls.ucSlider()
            CType(Me.m_pbxBlockCodes, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudNumBlockCodes, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudSelectedBlockCode, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_pbxBlockCodes
            '
            Me.m_pbxBlockCodes.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_pbxBlockCodes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_pbxBlockCodes.Location = New System.Drawing.Point(133, 3)
            Me.m_pbxBlockCodes.Margin = New System.Windows.Forms.Padding(0)
            Me.m_pbxBlockCodes.Name = "m_pbxBlockCodes"
            Me.m_pbxBlockCodes.Size = New System.Drawing.Size(380, 20)
            Me.m_pbxBlockCodes.TabIndex = 0
            Me.m_pbxBlockCodes.TabStop = False
            '
            'm_nudNumBlockCodes
            '
            Me.m_nudNumBlockCodes.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudNumBlockCodes.Location = New System.Drawing.Point(76, 3)
            Me.m_nudNumBlockCodes.Maximum = New Decimal(New Integer() {9999, 0, 0, 0})
            Me.m_nudNumBlockCodes.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudNumBlockCodes.Name = "m_nudNumBlockCodes"
            Me.m_nudNumBlockCodes.Size = New System.Drawing.Size(51, 20)
            Me.m_nudNumBlockCodes.TabIndex = 1
            Me.m_nudNumBlockCodes.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_nudSelectedBlockCode
            '
            Me.m_nudSelectedBlockCode.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudSelectedBlockCode.Location = New System.Drawing.Point(76, 29)
            Me.m_nudSelectedBlockCode.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
            Me.m_nudSelectedBlockCode.Name = "m_nudSelectedBlockCode"
            Me.m_nudSelectedBlockCode.Size = New System.Drawing.Size(51, 20)
            Me.m_nudSelectedBlockCode.TabIndex = 3
            Me.m_nudSelectedBlockCode.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_lblNumBlocks
            '
            Me.m_lblNumBlocks.AutoSize = True
            Me.m_lblNumBlocks.Location = New System.Drawing.Point(0, 5)
            Me.m_lblNumBlocks.Margin = New System.Windows.Forms.Padding(0)
            Me.m_lblNumBlocks.Name = "m_lblNumBlocks"
            Me.m_lblNumBlocks.Size = New System.Drawing.Size(73, 13)
            Me.m_lblNumBlocks.TabIndex = 0
            Me.m_lblNumBlocks.Text = "&No. of blocks:"
            '
            'm_lblSelectedBlock
            '
            Me.m_lblSelectedBlock.AutoSize = True
            Me.m_lblSelectedBlock.Location = New System.Drawing.Point(0, 31)
            Me.m_lblSelectedBlock.Margin = New System.Windows.Forms.Padding(0)
            Me.m_lblSelectedBlock.Name = "m_lblSelectedBlock"
            Me.m_lblSelectedBlock.Size = New System.Drawing.Size(52, 13)
            Me.m_lblSelectedBlock.TabIndex = 2
            Me.m_lblSelectedBlock.Text = "&Selected:"
            '
            'm_slSelectedBlockCode
            '
            Me.m_slSelectedBlockCode.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_slSelectedBlockCode.CurrentKnob = 0
            Me.m_slSelectedBlockCode.Location = New System.Drawing.Point(136, 27)
            Me.m_slSelectedBlockCode.Margin = New System.Windows.Forms.Padding(0)
            Me.m_slSelectedBlockCode.Maximum = 100
            Me.m_slSelectedBlockCode.Minimum = 0
            Me.m_slSelectedBlockCode.Name = "m_slSelectedBlockCode"
            Me.m_slSelectedBlockCode.NumKnobs = 1
            Me.m_slSelectedBlockCode.Size = New System.Drawing.Size(377, 23)
            Me.m_slSelectedBlockCode.TabIndex = 4
            '
            'ucParmBlockCodes
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_lblSelectedBlock)
            Me.Controls.Add(Me.m_lblNumBlocks)
            Me.Controls.Add(Me.m_nudSelectedBlockCode)
            Me.Controls.Add(Me.m_nudNumBlockCodes)
            Me.Controls.Add(Me.m_pbxBlockCodes)
            Me.Controls.Add(Me.m_slSelectedBlockCode)
            Me.Margin = New System.Windows.Forms.Padding(0)
            Me.Name = "ucParmBlockCodes"
            Me.Size = New System.Drawing.Size(513, 55)
            CType(Me.m_pbxBlockCodes, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudNumBlockCodes, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudSelectedBlockCode, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_lblNumBlocks As System.Windows.Forms.Label
        Private WithEvents m_pbxBlockCodes As System.Windows.Forms.PictureBox
        Private WithEvents m_lblSelectedBlock As System.Windows.Forms.Label
        Private WithEvents m_slSelectedBlockCode As ScientificInterfaceShared.Controls.ucSlider
        Private WithEvents m_nudNumBlockCodes As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudSelectedBlockCode As ScientificInterfaceShared.Controls.cEwENumericUpDown

    End Class

End Namespace
