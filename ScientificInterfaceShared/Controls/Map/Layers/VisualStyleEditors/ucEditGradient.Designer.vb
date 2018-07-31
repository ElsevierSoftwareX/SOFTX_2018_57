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

Namespace Controls

    Partial Class ucEditGradient
        Inherits ucEditVisualStyle

        'UserControl overrides dispose to clean up the component list.
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
            Me.m_nudAlpha = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudBlue = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudGreen = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudRed = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_slAlpha = New ScientificInterfaceShared.Controls.ucSlider()
            Me.m_slBlue = New ScientificInterfaceShared.Controls.ucSlider()
            Me.m_slGreen = New ScientificInterfaceShared.Controls.ucSlider()
            Me.m_slRed = New ScientificInterfaceShared.Controls.ucSlider()
            Me.m_plGradient = New System.Windows.Forms.Panel()
            Me.m_pbCurrentColor = New System.Windows.Forms.PictureBox()
            Me.m_lbAlpha = New System.Windows.Forms.Label()
            Me.m_lbBlue = New System.Windows.Forms.Label()
            Me.m_lbGreen = New System.Windows.Forms.Label()
            Me.m_lbRed = New System.Windows.Forms.Label()
            Me.m_rbDefaultGradient = New System.Windows.Forms.RadioButton()
            Me.m_rbCustomGradient = New System.Windows.Forms.RadioButton()
            Me.m_cmbGradient = New System.Windows.Forms.ComboBox()
            Me.m_btnAdd = New System.Windows.Forms.Button()
            Me.m_btnRemove = New System.Windows.Forms.Button()
            Me.m_slGradient = New ScientificInterfaceShared.Controls.ucSlider()
            Me.m_btnFlip = New System.Windows.Forms.Button()
            CType(Me.m_nudAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudBlue, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudGreen, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudRed, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_pbCurrentColor, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_nudAlpha
            '
            Me.m_nudAlpha.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_nudAlpha.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudAlpha.Location = New System.Drawing.Point(282, 204)
            Me.m_nudAlpha.Maximum = New Decimal(New Integer() {255, 0, 0, 0})
            Me.m_nudAlpha.Name = "m_nudAlpha"
            Me.m_nudAlpha.Size = New System.Drawing.Size(54, 20)
            Me.m_nudAlpha.TabIndex = 19
            '
            'm_nudBlue
            '
            Me.m_nudBlue.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_nudBlue.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudBlue.Location = New System.Drawing.Point(282, 181)
            Me.m_nudBlue.Maximum = New Decimal(New Integer() {255, 0, 0, 0})
            Me.m_nudBlue.Name = "m_nudBlue"
            Me.m_nudBlue.Size = New System.Drawing.Size(54, 20)
            Me.m_nudBlue.TabIndex = 16
            '
            'm_nudGreen
            '
            Me.m_nudGreen.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_nudGreen.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudGreen.Location = New System.Drawing.Point(282, 158)
            Me.m_nudGreen.Maximum = New Decimal(New Integer() {255, 0, 0, 0})
            Me.m_nudGreen.Name = "m_nudGreen"
            Me.m_nudGreen.Size = New System.Drawing.Size(54, 20)
            Me.m_nudGreen.TabIndex = 13
            '
            'm_nudRed
            '
            Me.m_nudRed.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_nudRed.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudRed.Location = New System.Drawing.Point(282, 134)
            Me.m_nudRed.Maximum = New Decimal(New Integer() {255, 0, 0, 0})
            Me.m_nudRed.Name = "m_nudRed"
            Me.m_nudRed.Size = New System.Drawing.Size(54, 20)
            Me.m_nudRed.TabIndex = 10
            '
            'm_slAlpha
            '
            Me.m_slAlpha.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_slAlpha.CurrentKnob = 0
            Me.m_slAlpha.Location = New System.Drawing.Point(105, 204)
            Me.m_slAlpha.Maximum = 255
            Me.m_slAlpha.Minimum = 0
            Me.m_slAlpha.Name = "m_slAlpha"
            Me.m_slAlpha.NumKnobs = 1
            Me.m_slAlpha.Size = New System.Drawing.Size(171, 20)
            Me.m_slAlpha.TabIndex = 18
            '
            'm_slBlue
            '
            Me.m_slBlue.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_slBlue.CurrentKnob = 0
            Me.m_slBlue.Location = New System.Drawing.Point(105, 181)
            Me.m_slBlue.Maximum = 255
            Me.m_slBlue.Minimum = 0
            Me.m_slBlue.Name = "m_slBlue"
            Me.m_slBlue.NumKnobs = 1
            Me.m_slBlue.Size = New System.Drawing.Size(171, 20)
            Me.m_slBlue.TabIndex = 15
            '
            'm_slGreen
            '
            Me.m_slGreen.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_slGreen.CurrentKnob = 0
            Me.m_slGreen.Location = New System.Drawing.Point(105, 158)
            Me.m_slGreen.Maximum = 255
            Me.m_slGreen.Minimum = 0
            Me.m_slGreen.Name = "m_slGreen"
            Me.m_slGreen.NumKnobs = 1
            Me.m_slGreen.Size = New System.Drawing.Size(171, 20)
            Me.m_slGreen.TabIndex = 12
            '
            'm_slRed
            '
            Me.m_slRed.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_slRed.CurrentKnob = 0
            Me.m_slRed.Location = New System.Drawing.Point(105, 134)
            Me.m_slRed.Maximum = 255
            Me.m_slRed.Minimum = 0
            Me.m_slRed.Name = "m_slRed"
            Me.m_slRed.NumKnobs = 1
            Me.m_slRed.Size = New System.Drawing.Size(171, 23)
            Me.m_slRed.TabIndex = 9
            '
            'm_plGradient
            '
            Me.m_plGradient.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_plGradient.BackColor = System.Drawing.SystemColors.Control
            Me.m_plGradient.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_plGradient.Cursor = System.Windows.Forms.Cursors.Hand
            Me.m_plGradient.Location = New System.Drawing.Point(25, 77)
            Me.m_plGradient.Name = "m_plGradient"
            Me.m_plGradient.Size = New System.Drawing.Size(252, 22)
            Me.m_plGradient.TabIndex = 3
            Me.m_plGradient.TabStop = True
            '
            'm_pbCurrentColor
            '
            Me.m_pbCurrentColor.BackColor = System.Drawing.Color.PaleGreen
            Me.m_pbCurrentColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_pbCurrentColor.Cursor = System.Windows.Forms.Cursors.Hand
            Me.m_pbCurrentColor.Location = New System.Drawing.Point(24, 134)
            Me.m_pbCurrentColor.Name = "m_pbCurrentColor"
            Me.m_pbCurrentColor.Size = New System.Drawing.Size(23, 23)
            Me.m_pbCurrentColor.TabIndex = 7
            Me.m_pbCurrentColor.TabStop = False
            '
            'm_lbAlpha
            '
            Me.m_lbAlpha.AutoSize = True
            Me.m_lbAlpha.Location = New System.Drawing.Point(59, 206)
            Me.m_lbAlpha.Name = "m_lbAlpha"
            Me.m_lbAlpha.Size = New System.Drawing.Size(37, 13)
            Me.m_lbAlpha.TabIndex = 17
            Me.m_lbAlpha.Text = "&Alpha:"
            '
            'm_lbBlue
            '
            Me.m_lbBlue.AutoSize = True
            Me.m_lbBlue.Location = New System.Drawing.Point(60, 183)
            Me.m_lbBlue.Name = "m_lbBlue"
            Me.m_lbBlue.Size = New System.Drawing.Size(31, 13)
            Me.m_lbBlue.TabIndex = 14
            Me.m_lbBlue.Text = "&Blue:"
            '
            'm_lbGreen
            '
            Me.m_lbGreen.AutoSize = True
            Me.m_lbGreen.Location = New System.Drawing.Point(60, 160)
            Me.m_lbGreen.Name = "m_lbGreen"
            Me.m_lbGreen.Size = New System.Drawing.Size(39, 13)
            Me.m_lbGreen.TabIndex = 11
            Me.m_lbGreen.Text = "&Green:"
            '
            'm_lbRed
            '
            Me.m_lbRed.AutoSize = True
            Me.m_lbRed.Location = New System.Drawing.Point(59, 137)
            Me.m_lbRed.Name = "m_lbRed"
            Me.m_lbRed.Size = New System.Drawing.Size(30, 13)
            Me.m_lbRed.TabIndex = 8
            Me.m_lbRed.Text = "&Red:"
            '
            'm_rbDefaultGradient
            '
            Me.m_rbDefaultGradient.AutoSize = True
            Me.m_rbDefaultGradient.Location = New System.Drawing.Point(4, 4)
            Me.m_rbDefaultGradient.Name = "m_rbDefaultGradient"
            Me.m_rbDefaultGradient.Size = New System.Drawing.Size(125, 17)
            Me.m_rbDefaultGradient.TabIndex = 0
            Me.m_rbDefaultGradient.TabStop = True
            Me.m_rbDefaultGradient.Text = "&Default EwE gradient"
            Me.m_rbDefaultGradient.UseVisualStyleBackColor = True
            '
            'm_rbCustomGradient
            '
            Me.m_rbCustomGradient.AutoSize = True
            Me.m_rbCustomGradient.Location = New System.Drawing.Point(4, 27)
            Me.m_rbCustomGradient.Name = "m_rbCustomGradient"
            Me.m_rbCustomGradient.Size = New System.Drawing.Size(101, 17)
            Me.m_rbCustomGradient.TabIndex = 1
            Me.m_rbCustomGradient.TabStop = True
            Me.m_rbCustomGradient.Text = "&Custom gradient"
            Me.m_rbCustomGradient.UseVisualStyleBackColor = True
            '
            'm_cmbGradient
            '
            Me.m_cmbGradient.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_cmbGradient.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
            Me.m_cmbGradient.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbGradient.FormattingEnabled = True
            Me.m_cmbGradient.Location = New System.Drawing.Point(24, 50)
            Me.m_cmbGradient.Name = "m_cmbGradient"
            Me.m_cmbGradient.Size = New System.Drawing.Size(252, 21)
            Me.m_cmbGradient.TabIndex = 2
            '
            'm_btnAdd
            '
            Me.m_btnAdd.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnAdd.Location = New System.Drawing.Point(283, 105)
            Me.m_btnAdd.Name = "m_btnAdd"
            Me.m_btnAdd.Size = New System.Drawing.Size(23, 23)
            Me.m_btnAdd.TabIndex = 4
            Me.m_btnAdd.Text = "+"
            Me.m_btnAdd.UseVisualStyleBackColor = True
            '
            'm_btnRemove
            '
            Me.m_btnRemove.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnRemove.Location = New System.Drawing.Point(312, 105)
            Me.m_btnRemove.Name = "m_btnRemove"
            Me.m_btnRemove.Size = New System.Drawing.Size(23, 23)
            Me.m_btnRemove.TabIndex = 5
            Me.m_btnRemove.Text = "-"
            Me.m_btnRemove.UseVisualStyleBackColor = True
            '
            'm_slGradient
            '
            Me.m_slGradient.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_slGradient.CurrentKnob = 0
            Me.m_slGradient.Location = New System.Drawing.Point(24, 105)
            Me.m_slGradient.Maximum = 100
            Me.m_slGradient.Minimum = 0
            Me.m_slGradient.Name = "m_slGradient"
            Me.m_slGradient.NumKnobs = 1
            Me.m_slGradient.Size = New System.Drawing.Size(253, 23)
            Me.m_slGradient.TabIndex = 6
            '
            'm_btnFlip
            '
            Me.m_btnFlip.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnFlip.Location = New System.Drawing.Point(283, 76)
            Me.m_btnFlip.Name = "m_btnFlip"
            Me.m_btnFlip.Size = New System.Drawing.Size(52, 23)
            Me.m_btnFlip.TabIndex = 20
            Me.m_btnFlip.Text = "Flip"
            Me.m_btnFlip.UseVisualStyleBackColor = True
            '
            'ucEditGradient
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_btnFlip)
            Me.Controls.Add(Me.m_btnRemove)
            Me.Controls.Add(Me.m_btnAdd)
            Me.Controls.Add(Me.m_cmbGradient)
            Me.Controls.Add(Me.m_rbCustomGradient)
            Me.Controls.Add(Me.m_rbDefaultGradient)
            Me.Controls.Add(Me.m_nudAlpha)
            Me.Controls.Add(Me.m_nudBlue)
            Me.Controls.Add(Me.m_nudGreen)
            Me.Controls.Add(Me.m_nudRed)
            Me.Controls.Add(Me.m_slAlpha)
            Me.Controls.Add(Me.m_slBlue)
            Me.Controls.Add(Me.m_slGreen)
            Me.Controls.Add(Me.m_slGradient)
            Me.Controls.Add(Me.m_slRed)
            Me.Controls.Add(Me.m_plGradient)
            Me.Controls.Add(Me.m_pbCurrentColor)
            Me.Controls.Add(Me.m_lbAlpha)
            Me.Controls.Add(Me.m_lbBlue)
            Me.Controls.Add(Me.m_lbGreen)
            Me.Controls.Add(Me.m_lbRed)
            Me.Name = "ucEditGradient"
            Me.Size = New System.Drawing.Size(340, 231)
            CType(Me.m_nudAlpha, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudBlue, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudGreen, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudRed, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_pbCurrentColor, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_slAlpha As ucSlider
        Private WithEvents m_slBlue As ucSlider
        Private WithEvents m_slGreen As ucSlider
        Private WithEvents m_slRed As ucSlider
        Private WithEvents m_lbAlpha As System.Windows.Forms.Label
        Private WithEvents m_lbBlue As System.Windows.Forms.Label
        Private WithEvents m_lbGreen As System.Windows.Forms.Label
        Private WithEvents m_lbRed As System.Windows.Forms.Label
        Private WithEvents m_nudAlpha As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudBlue As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudGreen As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudRed As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_pbCurrentColor As System.Windows.Forms.PictureBox
        Private WithEvents m_plGradient As System.Windows.Forms.Panel
        Private WithEvents m_rbDefaultGradient As System.Windows.Forms.RadioButton
        Private WithEvents m_rbCustomGradient As System.Windows.Forms.RadioButton
        Private WithEvents m_cmbGradient As System.Windows.Forms.ComboBox
        Private WithEvents m_btnAdd As System.Windows.Forms.Button
        Private WithEvents m_btnRemove As System.Windows.Forms.Button
        Private WithEvents m_slGradient As ScientificInterfaceShared.Controls.ucSlider
        Private WithEvents m_btnFlip As System.Windows.Forms.Button

    End Class

End Namespace
