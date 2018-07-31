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

Imports ScientificInterfaceShared.Forms
Imports ScientificInterfaceShared.Controls
Imports SharedResources = ScientificInterfaceShared.My.Resources

Namespace Ecopath.Output

    Partial Class frmSizeShiftedConnectancePlot
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
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_btnShowHideGroups = New System.Windows.Forms.Button()
            Me.m_hdrDisplay = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_graph = New ZedGraph.ZedGraphControl()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_scMain
            '
            Me.m_scMain.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scMain.Location = New System.Drawing.Point(0, 0)
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            Me.m_scMain.Panel1.Controls.Add(Me.m_btnShowHideGroups)
            Me.m_scMain.Panel1.Controls.Add(Me.m_hdrDisplay)
            '
            'm_scMain.Panel2
            '
            Me.m_scMain.Panel2.Controls.Add(Me.m_graph)
            Me.m_scMain.Size = New System.Drawing.Size(742, 506)
            Me.m_scMain.SplitterDistance = 142
            Me.m_scMain.TabIndex = 1
            '
            'm_btnShowHideGroups
            '
            Me.m_btnShowHideGroups.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnShowHideGroups.FlatStyle = System.Windows.Forms.FlatStyle.Popup
            Me.m_btnShowHideGroups.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_btnShowHideGroups.Location = New System.Drawing.Point(6, 21)
            Me.m_btnShowHideGroups.Name = "m_btnShowHideGroups"
            Me.m_btnShowHideGroups.Size = New System.Drawing.Size(133, 23)
            Me.m_btnShowHideGroups.TabIndex = 1
            Me.m_btnShowHideGroups.Text = "&Show/hide groups"
            Me.m_btnShowHideGroups.UseVisualStyleBackColor = True
            '
            'm_hdrDisplay
            '
            Me.m_hdrDisplay.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrDisplay.CanCollapseParent = False
            Me.m_hdrDisplay.CollapsedParentHeight = 0
            Me.m_hdrDisplay.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_hdrDisplay.IsCollapsed = False
            Me.m_hdrDisplay.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrDisplay.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrDisplay.Name = "m_hdrDisplay"
            Me.m_hdrDisplay.Size = New System.Drawing.Size(142, 18)
            Me.m_hdrDisplay.TabIndex = 0
            Me.m_hdrDisplay.Text = "Display"
            Me.m_hdrDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_graph
            '
            Me.m_graph.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_graph.Location = New System.Drawing.Point(0, 0)
            Me.m_graph.Margin = New System.Windows.Forms.Padding(0)
            Me.m_graph.Name = "m_graph"
            Me.m_graph.ScrollGrace = 0.0R
            Me.m_graph.ScrollMaxX = 0.0R
            Me.m_graph.ScrollMaxY = 0.0R
            Me.m_graph.ScrollMaxY2 = 0.0R
            Me.m_graph.ScrollMinX = 0.0R
            Me.m_graph.ScrollMinY = 0.0R
            Me.m_graph.ScrollMinY2 = 0.0R
            Me.m_graph.Size = New System.Drawing.Size(596, 506)
            Me.m_graph.TabIndex = 0
            '
            'frmSizeShiftedConnectancePlot
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.ClientSize = New System.Drawing.Size(742, 506)
            Me.Controls.Add(Me.m_scMain)
            Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Name = "frmSizeShiftedConnectancePlot"
            Me.TabText = ""
            Me.Text = "frmSizeShifterConnectancePlot"
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel2.ResumeLayout(False)
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
        Private WithEvents m_btnShowHideGroups As System.Windows.Forms.Button
        Private WithEvents m_hdrDisplay As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_graph As ZedGraph.ZedGraphControl
    End Class

End Namespace
