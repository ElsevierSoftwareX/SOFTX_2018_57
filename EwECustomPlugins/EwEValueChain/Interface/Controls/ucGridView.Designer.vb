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
Partial Class ucGridView
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tslGroup = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscmbGroup = New System.Windows.Forms.ToolStripComboBox()
        Me.m_tss1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tslFleet = New System.Windows.Forms.ToolStripLabel()
        Me.m_tscmbFleet = New System.Windows.Forms.ToolStripComboBox()
        Me.m_plGrid = New System.Windows.Forms.Panel()
        Me.m_ts.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_ts
        '
        Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tslGroup, Me.m_tscmbGroup, Me.m_tss1, Me.m_tslFleet, Me.m_tscmbFleet})
        Me.m_ts.Location = New System.Drawing.Point(0, 0)
        Me.m_ts.Name = "m_ts"
        Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.m_ts.Size = New System.Drawing.Size(695, 25)
        Me.m_ts.TabIndex = 0
        '
        'm_tslGroup
        '
        Me.m_tslGroup.Name = "m_tslGroup"
        Me.m_tslGroup.Size = New System.Drawing.Size(43, 22)
        Me.m_tslGroup.Text = "&Group:"
        '
        'm_tscmbGroup
        '
        Me.m_tscmbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscmbGroup.Name = "m_tscmbGroup"
        Me.m_tscmbGroup.Size = New System.Drawing.Size(121, 25)
        '
        'm_tss1
        '
        Me.m_tss1.Name = "m_tss1"
        Me.m_tss1.Size = New System.Drawing.Size(6, 25)
        '
        'm_tslFleet
        '
        Me.m_tslFleet.Name = "m_tslFleet"
        Me.m_tslFleet.Size = New System.Drawing.Size(35, 22)
        Me.m_tslFleet.Text = "&Fleet:"
        '
        'm_tscmbFleet
        '
        Me.m_tscmbFleet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_tscmbFleet.Name = "m_tscmbFleet"
        Me.m_tscmbFleet.Size = New System.Drawing.Size(121, 25)
        '
        'm_plGrid
        '
        Me.m_plGrid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_plGrid.Location = New System.Drawing.Point(0, 25)
        Me.m_plGrid.Name = "m_plGrid"
        Me.m_plGrid.Size = New System.Drawing.Size(695, 342)
        Me.m_plGrid.TabIndex = 1
        '
        'ucGridView
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Controls.Add(Me.m_plGrid)
        Me.Controls.Add(Me.m_ts)
        Me.Name = "ucGridView"
        Me.Size = New System.Drawing.Size(695, 367)
        Me.m_ts.ResumeLayout(False)
        Me.m_ts.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_ts As ScientificInterfaceShared.Controls.cEwEToolstrip
    Private WithEvents m_plGrid As System.Windows.Forms.Panel
    Private WithEvents m_tslGroup As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tscmbGroup As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_tslFleet As System.Windows.Forms.ToolStripLabel
    Private WithEvents m_tscmbFleet As System.Windows.Forms.ToolStripComboBox
    Private WithEvents m_tss1 As System.Windows.Forms.ToolStripSeparator

End Class
