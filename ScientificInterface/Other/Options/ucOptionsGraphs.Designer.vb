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

Imports ScientificInterfaceShared

Namespace Other

    Partial Class ucOptionsGraphs
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucOptionsGraphs))
            Me.m_hdr1 = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lblThumbnailSize = New System.Windows.Forms.Label()
            Me.m_nudThumbnailSize = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_rbLegendAlways = New System.Windows.Forms.RadioButton()
            Me.m_rbLegendSelective = New System.Windows.Forms.RadioButton()
            Me.m_lblThumbnailUnit = New System.Windows.Forms.Label()
            Me.m_nudFontSize = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblFontSize = New System.Windows.Forms.Label()
            Me.m_lblExample = New System.Windows.Forms.Label()
            Me.m_hdr2 = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_cbFontStyle = New System.Windows.Forms.ComboBox()
            Me.m_lblItemFontStyle = New System.Windows.Forms.Label()
            Me.m_cbFontFamily = New System.Windows.Forms.ComboBox()
            Me.lblItemForeColor = New System.Windows.Forms.Label()
            Me.m_lbFontTypes = New System.Windows.Forms.ListBox()
            Me.Label1 = New System.Windows.Forms.Label()
            CType(Me.m_nudThumbnailSize, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudFontSize, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_hdr1
            '
            Me.m_hdr1.CanCollapseParent = False
            Me.m_hdr1.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdr1, "m_hdr1")
            Me.m_hdr1.IsCollapsed = False
            Me.m_hdr1.Name = "m_hdr1"
            '
            'm_lblThumbnailSize
            '
            resources.ApplyResources(Me.m_lblThumbnailSize, "m_lblThumbnailSize")
            Me.m_lblThumbnailSize.Name = "m_lblThumbnailSize"
            '
            'm_nudThumbnailSize
            '
            Me.m_nudThumbnailSize.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudThumbnailSize, "m_nudThumbnailSize")
            Me.m_nudThumbnailSize.Maximum = New Decimal(New Integer() {240, 0, 0, 0})
            Me.m_nudThumbnailSize.Minimum = New Decimal(New Integer() {32, 0, 0, 0})
            Me.m_nudThumbnailSize.Name = "m_nudThumbnailSize"
            Me.m_nudThumbnailSize.Value = New Decimal(New Integer() {32, 0, 0, 0})
            '
            'm_rbLegendAlways
            '
            resources.ApplyResources(Me.m_rbLegendAlways, "m_rbLegendAlways")
            Me.m_rbLegendAlways.Name = "m_rbLegendAlways"
            Me.m_rbLegendAlways.TabStop = True
            Me.m_rbLegendAlways.UseVisualStyleBackColor = True
            '
            'm_rbLegendSelective
            '
            resources.ApplyResources(Me.m_rbLegendSelective, "m_rbLegendSelective")
            Me.m_rbLegendSelective.Name = "m_rbLegendSelective"
            Me.m_rbLegendSelective.TabStop = True
            Me.m_rbLegendSelective.UseVisualStyleBackColor = True
            '
            'm_lblThumbnailUnit
            '
            resources.ApplyResources(Me.m_lblThumbnailUnit, "m_lblThumbnailUnit")
            Me.m_lblThumbnailUnit.Name = "m_lblThumbnailUnit"
            '
            'm_nudFontSize
            '
            resources.ApplyResources(Me.m_nudFontSize, "m_nudFontSize")
            Me.m_nudFontSize.DecimalPlaces = 2
            Me.m_nudFontSize.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudFontSize.Maximum = New Decimal(New Integer() {24, 0, 0, 0})
            Me.m_nudFontSize.Minimum = New Decimal(New Integer() {4, 0, 0, 0})
            Me.m_nudFontSize.Name = "m_nudFontSize"
            Me.m_nudFontSize.Value = New Decimal(New Integer() {825, 0, 0, 131072})
            '
            'm_lblFontSize
            '
            resources.ApplyResources(Me.m_lblFontSize, "m_lblFontSize")
            Me.m_lblFontSize.Name = "m_lblFontSize"
            '
            'm_lblExample
            '
            resources.ApplyResources(Me.m_lblExample, "m_lblExample")
            Me.m_lblExample.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_lblExample.Name = "m_lblExample"
            '
            'm_hdr2
            '
            resources.ApplyResources(Me.m_hdr2, "m_hdr2")
            Me.m_hdr2.CanCollapseParent = False
            Me.m_hdr2.CollapsedParentHeight = 0
            Me.m_hdr2.IsCollapsed = False
            Me.m_hdr2.Name = "m_hdr2"
            '
            'm_cbFontStyle
            '
            resources.ApplyResources(Me.m_cbFontStyle, "m_cbFontStyle")
            Me.m_cbFontStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cbFontStyle.FormattingEnabled = True
            Me.m_cbFontStyle.Items.AddRange(New Object() {resources.GetString("m_cbFontStyle.Items"), resources.GetString("m_cbFontStyle.Items1"), resources.GetString("m_cbFontStyle.Items2"), resources.GetString("m_cbFontStyle.Items3")})
            Me.m_cbFontStyle.Name = "m_cbFontStyle"
            '
            'm_lblItemFontStyle
            '
            resources.ApplyResources(Me.m_lblItemFontStyle, "m_lblItemFontStyle")
            Me.m_lblItemFontStyle.Name = "m_lblItemFontStyle"
            '
            'm_cbFontFamily
            '
            resources.ApplyResources(Me.m_cbFontFamily, "m_cbFontFamily")
            Me.m_cbFontFamily.BackColor = System.Drawing.Color.White
            Me.m_cbFontFamily.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
            Me.m_cbFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cbFontFamily.FormattingEnabled = True
            Me.m_cbFontFamily.Name = "m_cbFontFamily"
            '
            'lblItemForeColor
            '
            resources.ApplyResources(Me.lblItemForeColor, "lblItemForeColor")
            Me.lblItemForeColor.Name = "lblItemForeColor"
            '
            'm_lbFontTypes
            '
            resources.ApplyResources(Me.m_lbFontTypes, "m_lbFontTypes")
            Me.m_lbFontTypes.Name = "m_lbFontTypes"
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'ucOptionsGraphs
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_lblExample)
            Me.Controls.Add(Me.m_lblThumbnailUnit)
            Me.Controls.Add(Me.m_rbLegendAlways)
            Me.Controls.Add(Me.m_nudThumbnailSize)
            Me.Controls.Add(Me.m_lblThumbnailSize)
            Me.Controls.Add(Me.m_nudFontSize)
            Me.Controls.Add(Me.m_rbLegendSelective)
            Me.Controls.Add(Me.m_lblFontSize)
            Me.Controls.Add(Me.m_hdr2)
            Me.Controls.Add(Me.m_cbFontStyle)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.m_lblItemFontStyle)
            Me.Controls.Add(Me.m_cbFontFamily)
            Me.Controls.Add(Me.lblItemForeColor)
            Me.Controls.Add(Me.m_lbFontTypes)
            Me.Controls.Add(Me.m_hdr1)
            Me.Name = "ucOptionsGraphs"
            CType(Me.m_nudThumbnailSize, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudFontSize, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_hdr1 As cEwEHeaderLabel
        Private WithEvents m_lblThumbnailSize As System.Windows.Forms.Label
        Private WithEvents m_rbLegendAlways As System.Windows.Forms.RadioButton
        Private WithEvents m_rbLegendSelective As System.Windows.Forms.RadioButton
        Private WithEvents m_lblThumbnailUnit As System.Windows.Forms.Label
        Private WithEvents m_lblFontSize As System.Windows.Forms.Label
        Private WithEvents m_lblExample As System.Windows.Forms.Label
        Private WithEvents m_hdr2 As cEwEHeaderLabel
        Private WithEvents m_lblItemFontStyle As System.Windows.Forms.Label
        Private WithEvents m_cbFontFamily As System.Windows.Forms.ComboBox
        Private WithEvents lblItemForeColor As System.Windows.Forms.Label
        Private WithEvents m_lbFontTypes As System.Windows.Forms.ListBox
        Private WithEvents m_cbFontStyle As System.Windows.Forms.ComboBox
        Private WithEvents Label1 As System.Windows.Forms.Label
        Private WithEvents m_nudThumbnailSize As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudFontSize As ScientificInterfaceShared.Controls.cEwENumericUpDown

    End Class
End Namespace

