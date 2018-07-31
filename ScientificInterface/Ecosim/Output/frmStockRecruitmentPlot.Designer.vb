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

Namespace Ecosim

    Partial Class frmStockRecruitmentPlot
        Inherits frmEwE

        'Form overrides dispose to clean up the component list.
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
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStockRecruitmentPlot))
            Me.m_plot = New ZedGraph.ZedGraphControl()
            Me.m_tvGroups = New System.Windows.Forms.TreeView()
            Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
            Me.m_lblPt = New System.Windows.Forms.Label()
            Me.m_btnRun = New System.Windows.Forms.Button()
            CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SplitContainer1.Panel1.SuspendLayout()
            Me.SplitContainer1.Panel2.SuspendLayout()
            Me.SplitContainer1.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_plot
            '
            resources.ApplyResources(Me.m_plot, "m_plot")
            Me.m_plot.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_plot.Name = "m_plot"
            Me.m_plot.ScrollGrace = 0.0R
            Me.m_plot.ScrollMaxX = 0.0R
            Me.m_plot.ScrollMaxY = 0.0R
            Me.m_plot.ScrollMaxY2 = 0.0R
            Me.m_plot.ScrollMinX = 0.0R
            Me.m_plot.ScrollMinY = 0.0R
            Me.m_plot.ScrollMinY2 = 0.0R
            '
            'm_tvGroups
            '
            resources.ApplyResources(Me.m_tvGroups, "m_tvGroups")
            Me.m_tvGroups.BackColor = System.Drawing.SystemColors.Window
            Me.m_tvGroups.HideSelection = False
            Me.m_tvGroups.Name = "m_tvGroups"
            '
            'SplitContainer1
            '
            resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
            Me.SplitContainer1.Name = "SplitContainer1"
            '
            'SplitContainer1.Panel1
            '
            Me.SplitContainer1.Panel1.Controls.Add(Me.m_tvGroups)
            '
            'SplitContainer1.Panel2
            '
            Me.SplitContainer1.Panel2.Controls.Add(Me.m_lblPt)
            Me.SplitContainer1.Panel2.Controls.Add(Me.m_btnRun)
            Me.SplitContainer1.Panel2.Controls.Add(Me.m_plot)
            '
            'm_lblPt
            '
            resources.ApplyResources(Me.m_lblPt, "m_lblPt")
            Me.m_lblPt.Name = "m_lblPt"
            '
            'm_btnRun
            '
            resources.ApplyResources(Me.m_btnRun, "m_btnRun")
            Me.m_btnRun.Name = "m_btnRun"
            Me.m_btnRun.UseVisualStyleBackColor = True
            '
            'frmStockRecruitmentPlot
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.SplitContainer1)
            Me.Name = "frmStockRecruitmentPlot"
            Me.TabText = "S/R plot"
            Me.SplitContainer1.Panel1.ResumeLayout(False)
            Me.SplitContainer1.Panel2.ResumeLayout(False)
            CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
            Me.SplitContainer1.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
        Private WithEvents m_plot As ZedGraph.ZedGraphControl
        Private WithEvents m_tvGroups As System.Windows.Forms.TreeView
        Private WithEvents m_btnRun As System.Windows.Forms.Button
        Private WithEvents m_lblPt As System.Windows.Forms.Label
    End Class
End Namespace

