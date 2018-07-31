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

Imports ScientificInterfaceShared.Controls

Namespace Other

    Partial Class ucOptionsMap
        Inherits System.Windows.Forms.UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucOptionsMap))
            Me.m_lblNorth = New System.Windows.Forms.Label()
            Me.m_lblWest = New System.Windows.Forms.Label()
            Me.m_lblEast = New System.Windows.Forms.Label()
            Me.m_lblSouth = New System.Windows.Forms.Label()
            Me.m_lblFile = New System.Windows.Forms.Label()
            Me.m_tbxFile = New System.Windows.Forms.TextBox()
            Me.m_btnChoose = New System.Windows.Forms.Button()
            Me.m_plPreview = New System.Windows.Forms.Panel()
            Me.m_cbShowExcludedCells = New System.Windows.Forms.CheckBox()
            Me.m_nudEast = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudSouth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudWest = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudNorth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_hdrCaption = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            CType(Me.m_nudEast, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudSouth, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudWest, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudNorth, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_lblNorth
            '
            resources.ApplyResources(Me.m_lblNorth, "m_lblNorth")
            Me.m_lblNorth.Name = "m_lblNorth"
            '
            'm_lblWest
            '
            resources.ApplyResources(Me.m_lblWest, "m_lblWest")
            Me.m_lblWest.Name = "m_lblWest"
            '
            'm_lblEast
            '
            resources.ApplyResources(Me.m_lblEast, "m_lblEast")
            Me.m_lblEast.Name = "m_lblEast"
            '
            'm_lblSouth
            '
            resources.ApplyResources(Me.m_lblSouth, "m_lblSouth")
            Me.m_lblSouth.Name = "m_lblSouth"
            '
            'm_lblFile
            '
            resources.ApplyResources(Me.m_lblFile, "m_lblFile")
            Me.m_lblFile.Name = "m_lblFile"
            '
            'm_tbxFile
            '
            resources.ApplyResources(Me.m_tbxFile, "m_tbxFile")
            Me.m_tbxFile.Name = "m_tbxFile"
            '
            'm_btnChoose
            '
            resources.ApplyResources(Me.m_btnChoose, "m_btnChoose")
            Me.m_btnChoose.Name = "m_btnChoose"
            Me.m_btnChoose.UseVisualStyleBackColor = True
            '
            'm_plPreview
            '
            resources.ApplyResources(Me.m_plPreview, "m_plPreview")
            Me.m_plPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_plPreview.Name = "m_plPreview"
            '
            'm_cbShowExcludedCells
            '
            resources.ApplyResources(Me.m_cbShowExcludedCells, "m_cbShowExcludedCells")
            Me.m_cbShowExcludedCells.Name = "m_cbShowExcludedCells"
            Me.m_cbShowExcludedCells.UseVisualStyleBackColor = True
            '
            'm_nudEast
            '
            resources.ApplyResources(Me.m_nudEast, "m_nudEast")
            Me.m_nudEast.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudEast.Maximum = New Decimal(New Integer() {180, 0, 0, 0})
            Me.m_nudEast.Minimum = New Decimal(New Integer() {180, 0, 0, -2147483648})
            Me.m_nudEast.Name = "m_nudEast"
            Me.m_nudEast.Value = New Decimal(New Integer() {180, 0, 0, 0})
            '
            'm_nudSouth
            '
            resources.ApplyResources(Me.m_nudSouth, "m_nudSouth")
            Me.m_nudSouth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudSouth.Maximum = New Decimal(New Integer() {90, 0, 0, 0})
            Me.m_nudSouth.Minimum = New Decimal(New Integer() {90, 0, 0, -2147483648})
            Me.m_nudSouth.Name = "m_nudSouth"
            Me.m_nudSouth.Value = New Decimal(New Integer() {90, 0, 0, -2147483648})
            '
            'm_nudWest
            '
            resources.ApplyResources(Me.m_nudWest, "m_nudWest")
            Me.m_nudWest.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudWest.Maximum = New Decimal(New Integer() {180, 0, 0, 0})
            Me.m_nudWest.Minimum = New Decimal(New Integer() {180, 0, 0, -2147483648})
            Me.m_nudWest.Name = "m_nudWest"
            Me.m_nudWest.Value = New Decimal(New Integer() {180, 0, 0, -2147483648})
            '
            'm_nudNorth
            '
            resources.ApplyResources(Me.m_nudNorth, "m_nudNorth")
            Me.m_nudNorth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudNorth.Maximum = New Decimal(New Integer() {90, 0, 0, 0})
            Me.m_nudNorth.Minimum = New Decimal(New Integer() {90, 0, 0, -2147483648})
            Me.m_nudNorth.Name = "m_nudNorth"
            Me.m_nudNorth.Value = New Decimal(New Integer() {90, 0, 0, 0})
            '
            'm_hdrCaption
            '
            Me.m_hdrCaption.CanCollapseParent = False
            Me.m_hdrCaption.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrCaption, "m_hdrCaption")
            Me.m_hdrCaption.IsCollapsed = False
            Me.m_hdrCaption.Name = "m_hdrCaption"
            '
            'ucOptionsMap
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_cbShowExcludedCells)
            Me.Controls.Add(Me.m_plPreview)
            Me.Controls.Add(Me.m_btnChoose)
            Me.Controls.Add(Me.m_tbxFile)
            Me.Controls.Add(Me.m_lblFile)
            Me.Controls.Add(Me.m_nudEast)
            Me.Controls.Add(Me.m_nudSouth)
            Me.Controls.Add(Me.m_nudWest)
            Me.Controls.Add(Me.m_nudNorth)
            Me.Controls.Add(Me.m_lblNorth)
            Me.Controls.Add(Me.m_lblWest)
            Me.Controls.Add(Me.m_lblEast)
            Me.Controls.Add(Me.m_lblSouth)
            Me.Controls.Add(Me.m_hdrCaption)
            Me.Name = "ucOptionsMap"
            CType(Me.m_nudEast, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudSouth, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudWest, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudNorth, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private m_hdrCaption As cEwEHeaderLabel
        Private WithEvents m_nudEast As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudSouth As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudWest As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudNorth As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_lblNorth As System.Windows.Forms.Label
        Private WithEvents m_lblWest As System.Windows.Forms.Label
        Private WithEvents m_lblEast As System.Windows.Forms.Label
        Private WithEvents m_lblSouth As System.Windows.Forms.Label
        Private WithEvents m_lblFile As System.Windows.Forms.Label
        Private WithEvents m_btnChoose As System.Windows.Forms.Button
        Private WithEvents m_tbxFile As System.Windows.Forms.TextBox
        Private WithEvents m_plPreview As System.Windows.Forms.Panel
        Private WithEvents m_cbShowExcludedCells As System.Windows.Forms.CheckBox

    End Class
End Namespace

