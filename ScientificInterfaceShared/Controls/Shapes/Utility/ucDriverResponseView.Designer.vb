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
Partial Class ucDriverResponseView
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.m_lblXMin = New System.Windows.Forms.Label()
        Me.m_tbxXMin = New System.Windows.Forms.TextBox()
        Me.m_lblXMax = New System.Windows.Forms.Label()
        Me.m_tbxXMax = New System.Windows.Forms.TextBox()
        Me.m_lblMean = New System.Windows.Forms.Label()
        Me.m_tbxMean = New System.Windows.Forms.TextBox()
        Me.m_btnDefaultMinMax = New System.Windows.Forms.Button()
        Me.m_btnChangeShape = New System.Windows.Forms.Button()
        Me.m_tlpControls = New System.Windows.Forms.TableLayoutPanel()
        Me.m_graph = New ZedGraph.ZedGraphControl()
        Me.m_tlpBits = New System.Windows.Forms.TableLayoutPanel()
        Me.m_tlpControls.SuspendLayout()
        Me.m_tlpBits.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_lblXMin
        '
        Me.m_lblXMin.AutoSize = True
        Me.m_lblXMin.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lblXMin.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_lblXMin.Location = New System.Drawing.Point(3, 3)
        Me.m_lblXMin.Margin = New System.Windows.Forms.Padding(3)
        Me.m_lblXMin.Name = "m_lblXMin"
        Me.m_lblXMin.Size = New System.Drawing.Size(37, 22)
        Me.m_lblXMin.TabIndex = 0
        Me.m_lblXMin.Text = "X m&in:"
        Me.m_lblXMin.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'm_tbxXMin
        '
        Me.m_tbxXMin.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tbxXMin.Location = New System.Drawing.Point(43, 5)
        Me.m_tbxXMin.Margin = New System.Windows.Forms.Padding(0, 5, 0, 0)
        Me.m_tbxXMin.Name = "m_tbxXMin"
        Me.m_tbxXMin.Size = New System.Drawing.Size(50, 20)
        Me.m_tbxXMin.TabIndex = 1
        '
        'm_lblXMax
        '
        Me.m_lblXMax.AutoSize = True
        Me.m_lblXMax.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lblXMax.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_lblXMax.Location = New System.Drawing.Point(116, 3)
        Me.m_lblXMax.Margin = New System.Windows.Forms.Padding(3)
        Me.m_lblXMax.Name = "m_lblXMax"
        Me.m_lblXMax.Size = New System.Drawing.Size(45, 22)
        Me.m_lblXMax.TabIndex = 2
        Me.m_lblXMax.Text = "X m&ax:  "
        Me.m_lblXMax.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'm_tbxXMax
        '
        Me.m_tbxXMax.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tbxXMax.Location = New System.Drawing.Point(164, 5)
        Me.m_tbxXMax.Margin = New System.Windows.Forms.Padding(0, 5, 0, 0)
        Me.m_tbxXMax.Name = "m_tbxXMax"
        Me.m_tbxXMax.Size = New System.Drawing.Size(50, 20)
        Me.m_tbxXMax.TabIndex = 3
        '
        'm_lblMean
        '
        Me.m_lblMean.AutoSize = True
        Me.m_lblMean.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_lblMean.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_lblMean.Location = New System.Drawing.Point(3, 31)
        Me.m_lblMean.Margin = New System.Windows.Forms.Padding(3)
        Me.m_lblMean.Name = "m_lblMean"
        Me.m_lblMean.Size = New System.Drawing.Size(37, 23)
        Me.m_lblMean.TabIndex = 5
        Me.m_lblMean.Text = "Mean:"
        Me.m_lblMean.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_tbxMean
        '
        Me.m_tbxMean.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tbxMean.Location = New System.Drawing.Point(43, 33)
        Me.m_tbxMean.Margin = New System.Windows.Forms.Padding(0, 5, 0, 0)
        Me.m_tbxMean.Name = "m_tbxMean"
        Me.m_tbxMean.Size = New System.Drawing.Size(50, 20)
        Me.m_tbxMean.TabIndex = 6
        '
        'm_btnDefaultMinMax
        '
        Me.m_btnDefaultMinMax.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_btnDefaultMinMax.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_btnDefaultMinMax.Location = New System.Drawing.Point(267, 3)
        Me.m_btnDefaultMinMax.Margin = New System.Windows.Forms.Padding(0, 3, 0, 2)
        Me.m_btnDefaultMinMax.Name = "m_btnDefaultMinMax"
        Me.m_btnDefaultMinMax.Size = New System.Drawing.Size(100, 23)
        Me.m_btnDefaultMinMax.TabIndex = 4
        Me.m_btnDefaultMinMax.Text = "&Default X axis"
        Me.m_btnDefaultMinMax.UseVisualStyleBackColor = True
        '
        'm_btnChangeShape
        '
        Me.m_btnChangeShape.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_btnChangeShape.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.m_btnChangeShape.Location = New System.Drawing.Point(267, 31)
        Me.m_btnChangeShape.Margin = New System.Windows.Forms.Padding(0, 3, 0, 2)
        Me.m_btnChangeShape.Name = "m_btnChangeShape"
        Me.m_btnChangeShape.Size = New System.Drawing.Size(100, 24)
        Me.m_btnChangeShape.TabIndex = 7
        Me.m_btnChangeShape.Text = "&Change shape..."
        Me.m_btnChangeShape.UseVisualStyleBackColor = True
        '
        'm_tlpControls
        '
        Me.m_tlpControls.AutoSize = True
        Me.m_tlpControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.m_tlpControls.ColumnCount = 7
        Me.m_tlpControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.m_tlpControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.m_tlpControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.m_tlpControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.m_tlpControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.m_tlpControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.m_tlpControls.Controls.Add(Me.m_lblXMin, 0, 0)
        Me.m_tlpControls.Controls.Add(Me.m_tbxXMax, 4, 0)
        Me.m_tlpControls.Controls.Add(Me.m_btnChangeShape, 6, 1)
        Me.m_tlpControls.Controls.Add(Me.m_btnDefaultMinMax, 6, 0)
        Me.m_tlpControls.Controls.Add(Me.m_lblXMax, 3, 0)
        Me.m_tlpControls.Controls.Add(Me.m_tbxXMin, 1, 0)
        Me.m_tlpControls.Controls.Add(Me.m_lblMean, 0, 1)
        Me.m_tlpControls.Controls.Add(Me.m_tbxMean, 1, 1)
        Me.m_tlpControls.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpControls.Location = New System.Drawing.Point(0, 0)
        Me.m_tlpControls.Margin = New System.Windows.Forms.Padding(0, 0, 0, 3)
        Me.m_tlpControls.Name = "m_tlpControls"
        Me.m_tlpControls.RowCount = 2
        Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpControls.Size = New System.Drawing.Size(367, 57)
        Me.m_tlpControls.TabIndex = 0
        '
        'm_graph
        '
        Me.m_graph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_graph.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_graph.Location = New System.Drawing.Point(0, 63)
        Me.m_graph.Margin = New System.Windows.Forms.Padding(0, 3, 0, 0)
        Me.m_graph.Name = "m_graph"
        Me.m_graph.ScrollGrace = 0R
        Me.m_graph.ScrollMaxX = 0R
        Me.m_graph.ScrollMaxY = 0R
        Me.m_graph.ScrollMaxY2 = 0R
        Me.m_graph.ScrollMinX = 0R
        Me.m_graph.ScrollMinY = 0R
        Me.m_graph.ScrollMinY2 = 0R
        Me.m_graph.Size = New System.Drawing.Size(367, 272)
        Me.m_graph.TabIndex = 1
        '
        'm_tlpBits
        '
        Me.m_tlpBits.ColumnCount = 1
        Me.m_tlpBits.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpBits.Controls.Add(Me.m_graph, 0, 1)
        Me.m_tlpBits.Controls.Add(Me.m_tlpControls, 0, 0)
        Me.m_tlpBits.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpBits.Location = New System.Drawing.Point(0, 0)
        Me.m_tlpBits.Margin = New System.Windows.Forms.Padding(0)
        Me.m_tlpBits.Name = "m_tlpBits"
        Me.m_tlpBits.RowCount = 2
        Me.m_tlpBits.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpBits.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpBits.Size = New System.Drawing.Size(367, 335)
        Me.m_tlpBits.TabIndex = 0
        '
        'ucDriverResponseView
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.m_tlpBits)
        Me.Name = "ucDriverResponseView"
        Me.Size = New System.Drawing.Size(367, 335)
        Me.m_tlpControls.ResumeLayout(False)
        Me.m_tlpControls.PerformLayout()
        Me.m_tlpBits.ResumeLayout(False)
        Me.m_tlpBits.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_lblXMin As Label
    Private WithEvents m_tbxXMin As TextBox
    Private WithEvents m_lblXMax As Label
    Private WithEvents m_tbxXMax As TextBox
    Private WithEvents m_lblMean As Label
    Private WithEvents m_tbxMean As TextBox
    Private WithEvents m_btnDefaultMinMax As Button
    Private WithEvents m_btnChangeShape As Button
    Private WithEvents m_tlpControls As TableLayoutPanel
    Private WithEvents m_graph As ZedGraph.ZedGraphControl
    Private WithEvents m_tlpBits As TableLayoutPanel
End Class
