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
Imports ScientificInterfaceShared.Forms

Partial Class frmMSERunBatch
    Inherits frmEwE

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
        Me.components = New System.ComponentModel.Container()
        Me.btRunBatch = New System.Windows.Forms.Button()
        Me.lstMsgs = New System.Windows.Forms.ListBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.m_pnlZedGraphContainer = New System.Windows.Forms.Panel()
        Me.m_ZedGraph = New ZedGraph.ZedGraphControl()
        Me.m_btStop = New System.Windows.Forms.Button()
        Me.m_pnlZedGraphContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'btRunBatch
        '
        Me.btRunBatch.Location = New System.Drawing.Point(14, 12)
        Me.btRunBatch.Name = "btRunBatch"
        Me.btRunBatch.Size = New System.Drawing.Size(99, 25)
        Me.btRunBatch.TabIndex = 0
        Me.btRunBatch.Text = "Run Batch"
        Me.btRunBatch.UseVisualStyleBackColor = True
        '
        'lstMsgs
        '
        Me.lstMsgs.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstMsgs.FormattingEnabled = True
        Me.lstMsgs.Location = New System.Drawing.Point(14, 63)
        Me.lstMsgs.Name = "lstMsgs"
        Me.lstMsgs.Size = New System.Drawing.Size(838, 69)
        Me.lstMsgs.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(14, 47)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(55, 13)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Messages"
        '
        'm_pnlZedGraphContainer
        '
        Me.m_pnlZedGraphContainer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_pnlZedGraphContainer.Controls.Add(Me.m_ZedGraph)
        Me.m_pnlZedGraphContainer.Location = New System.Drawing.Point(14, 151)
        Me.m_pnlZedGraphContainer.Name = "m_pnlZedGraphContainer"
        Me.m_pnlZedGraphContainer.Size = New System.Drawing.Size(838, 324)
        Me.m_pnlZedGraphContainer.TabIndex = 6
        '
        'm_ZedGraph
        '
        Me.m_ZedGraph.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_ZedGraph.Location = New System.Drawing.Point(0, 0)
        Me.m_ZedGraph.Name = "m_ZedGraph"
        Me.m_ZedGraph.ScrollGrace = 0.0R
        Me.m_ZedGraph.ScrollMaxX = 0.0R
        Me.m_ZedGraph.ScrollMaxY = 0.0R
        Me.m_ZedGraph.ScrollMaxY2 = 0.0R
        Me.m_ZedGraph.ScrollMinX = 0.0R
        Me.m_ZedGraph.ScrollMinY = 0.0R
        Me.m_ZedGraph.ScrollMinY2 = 0.0R
        Me.m_ZedGraph.Size = New System.Drawing.Size(838, 324)
        Me.m_ZedGraph.TabIndex = 6
        '
        'm_btStop
        '
        Me.m_btStop.Location = New System.Drawing.Point(131, 12)
        Me.m_btStop.Name = "m_btStop"
        Me.m_btStop.Size = New System.Drawing.Size(91, 24)
        Me.m_btStop.TabIndex = 7
        Me.m_btStop.Text = "Stop run"
        Me.m_btStop.UseVisualStyleBackColor = True
        '
        'frmMSERunBatch
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(864, 487)
        Me.Controls.Add(Me.m_btStop)
        Me.Controls.Add(Me.m_pnlZedGraphContainer)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lstMsgs)
        Me.Controls.Add(Me.btRunBatch)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "frmMSERunBatch"
        Me.TabText = ""
        Me.Text = "MSE batch run"
        Me.m_pnlZedGraphContainer.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btRunBatch As System.Windows.Forms.Button
    Friend WithEvents lstMsgs As System.Windows.Forms.ListBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents m_pnlZedGraphContainer As System.Windows.Forms.Panel
    Friend WithEvents m_ZedGraph As ZedGraph.ZedGraphControl
    Friend WithEvents m_btStop As System.Windows.Forms.Button
End Class
