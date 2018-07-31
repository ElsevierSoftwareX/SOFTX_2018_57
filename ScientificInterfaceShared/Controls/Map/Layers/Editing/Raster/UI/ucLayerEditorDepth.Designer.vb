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

Namespace Controls.Map.Layers

    Partial Class ucLayerEditorDepth
        Inherits ucLayerEditorDefault

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.m_rbWater = New System.Windows.Forms.RadioButton()
            Me.m_rbLand = New System.Windows.Forms.RadioButton()
            Me.m_nudDepth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_pbPreviewWater = New System.Windows.Forms.PictureBox()
            Me.m_pbPreviewLand = New System.Windows.Forms.PictureBox()
            Me.m_cbProtectCoastline = New System.Windows.Forms.CheckBox()
            Me.m_btnFill = New System.Windows.Forms.Button()
            Me.m_btnSmooth = New System.Windows.Forms.Button()
            CType(Me.m_nudDepth, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_pbPreviewWater, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_pbPreviewLand, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_rbWater
            '
            Me.m_rbWater.AutoSize = True
            Me.m_rbWater.Location = New System.Drawing.Point(7, 178)
            Me.m_rbWater.Name = "m_rbWater"
            Me.m_rbWater.Size = New System.Drawing.Size(54, 17)
            Me.m_rbWater.TabIndex = 1
            Me.m_rbWater.TabStop = True
            Me.m_rbWater.Text = "&Water"
            Me.m_rbWater.UseVisualStyleBackColor = True
            '
            'm_rbLand
            '
            Me.m_rbLand.AutoSize = True
            Me.m_rbLand.Location = New System.Drawing.Point(8, 153)
            Me.m_rbLand.Name = "m_rbLand"
            Me.m_rbLand.Size = New System.Drawing.Size(49, 17)
            Me.m_rbLand.TabIndex = 0
            Me.m_rbLand.TabStop = True
            Me.m_rbLand.Text = "&Land"
            Me.m_rbLand.UseVisualStyleBackColor = True
            '
            'm_nudDepth
            '
            Me.m_nudDepth.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_nudDepth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudDepth.Location = New System.Drawing.Point(68, 178)
            Me.m_nudDepth.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
            Me.m_nudDepth.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudDepth.Name = "m_nudDepth"
            Me.m_nudDepth.Size = New System.Drawing.Size(93, 20)
            Me.m_nudDepth.TabIndex = 2
            Me.m_nudDepth.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_pbPreviewWater
            '
            Me.m_pbPreviewWater.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_pbPreviewWater.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_pbPreviewWater.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_pbPreviewWater.Location = New System.Drawing.Point(167, 178)
            Me.m_pbPreviewWater.Name = "m_pbPreviewWater"
            Me.m_pbPreviewWater.Size = New System.Drawing.Size(27, 20)
            Me.m_pbPreviewWater.TabIndex = 5
            Me.m_pbPreviewWater.TabStop = False
            '
            'm_pbPreviewLand
            '
            Me.m_pbPreviewLand.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_pbPreviewLand.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_pbPreviewLand.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_pbPreviewLand.Location = New System.Drawing.Point(167, 151)
            Me.m_pbPreviewLand.Name = "m_pbPreviewLand"
            Me.m_pbPreviewLand.Size = New System.Drawing.Size(27, 21)
            Me.m_pbPreviewLand.TabIndex = 5
            Me.m_pbPreviewLand.TabStop = False
            '
            'm_cbProtectCoastline
            '
            Me.m_cbProtectCoastline.AutoSize = True
            Me.m_cbProtectCoastline.Checked = True
            Me.m_cbProtectCoastline.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_cbProtectCoastline.Location = New System.Drawing.Point(68, 202)
            Me.m_cbProtectCoastline.Name = "m_cbProtectCoastline"
            Me.m_cbProtectCoastline.Size = New System.Drawing.Size(120, 17)
            Me.m_cbProtectCoastline.TabIndex = 3
            Me.m_cbProtectCoastline.Text = "Only edit water cells"
            Me.m_cbProtectCoastline.UseVisualStyleBackColor = True
            '
            'm_btnFill
            '
            Me.m_btnFill.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnFill.Image = Global.ScientificInterfaceShared.My.Resources.Resources.Fill
            Me.m_btnFill.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_btnFill.Location = New System.Drawing.Point(167, 225)
            Me.m_btnFill.Name = "m_btnFill"
            Me.m_btnFill.Size = New System.Drawing.Size(27, 23)
            Me.m_btnFill.TabIndex = 8
            Me.m_btnFill.UseVisualStyleBackColor = True
            '
            'm_btnSmooth
            '
            Me.m_btnSmooth.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnSmooth.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_btnSmooth.Location = New System.Drawing.Point(68, 225)
            Me.m_btnSmooth.Name = "m_btnSmooth"
            Me.m_btnSmooth.Size = New System.Drawing.Size(93, 23)
            Me.m_btnSmooth.TabIndex = 7
            Me.m_btnSmooth.Text = "Smooth"
            Me.m_btnSmooth.UseVisualStyleBackColor = True
            '
            'ucLayerEditorDepth
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_btnFill)
            Me.Controls.Add(Me.m_btnSmooth)
            Me.Controls.Add(Me.m_rbWater)
            Me.Controls.Add(Me.m_pbPreviewWater)
            Me.Controls.Add(Me.m_pbPreviewLand)
            Me.Controls.Add(Me.m_rbLand)
            Me.Controls.Add(Me.m_nudDepth)
            Me.Controls.Add(Me.m_cbProtectCoastline)
            Me.Name = "ucLayerEditorDepth"
            Me.Size = New System.Drawing.Size(200, 257)
            Me.Controls.SetChildIndex(Me.m_cbProtectCoastline, 0)
            Me.Controls.SetChildIndex(Me.m_nudDepth, 0)
            Me.Controls.SetChildIndex(Me.m_rbLand, 0)
            Me.Controls.SetChildIndex(Me.m_pbPreviewLand, 0)
            Me.Controls.SetChildIndex(Me.m_pbPreviewWater, 0)
            Me.Controls.SetChildIndex(Me.m_rbWater, 0)
            Me.Controls.SetChildIndex(Me.m_btnSmooth, 0)
            Me.Controls.SetChildIndex(Me.m_btnFill, 0)
            CType(Me.m_nudDepth, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_pbPreviewWater, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_pbPreviewLand, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_rbWater As System.Windows.Forms.RadioButton
        Private WithEvents m_rbLand As System.Windows.Forms.RadioButton
        Protected WithEvents m_pbPreviewWater As System.Windows.Forms.PictureBox
        Protected WithEvents m_pbPreviewLand As System.Windows.Forms.PictureBox
        Private WithEvents m_cbProtectCoastline As System.Windows.Forms.CheckBox
        Private WithEvents m_nudDepth As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_btnFill As System.Windows.Forms.Button
        Private WithEvents m_btnSmooth As System.Windows.Forms.Button

    End Class

End Namespace