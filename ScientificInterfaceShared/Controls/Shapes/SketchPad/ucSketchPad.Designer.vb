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

    Partial Class ucSketchPad
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
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim m_tss1 As System.Windows.Forms.ToolStripSeparator
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucSketchPad))
            Me.m_spContextMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.m_tsmiDrawMode = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiFill = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiLine = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiTSDriver = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiShowMarks = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiAutoScaleYAxis = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiReset = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiValue = New System.Windows.Forms.ToolStripMenuItem()
            Me.LoadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiSave = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tss2 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsmiOptions = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiTSRefAbs = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiTSRefRel = New System.Windows.Forms.ToolStripMenuItem()
            m_tss1 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_spContextMenuStrip.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_tss1
            '
            m_tss1.Name = "m_tss1"
            resources.ApplyResources(m_tss1, "m_tss1")
            '
            'm_spContextMenuStrip
            '
            Me.m_spContextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiDrawMode, Me.m_tsmiShowMarks, Me.m_tsmiAutoScaleYAxis, m_tss1, Me.m_tsmiReset, Me.m_tsmiValue, Me.LoadToolStripMenuItem, Me.m_tsmiSave, Me.m_tss2, Me.m_tsmiOptions})
            Me.m_spContextMenuStrip.Name = "ContextMenuStrip1"
            resources.ApplyResources(Me.m_spContextMenuStrip, "m_spContextMenuStrip")
            '
            'm_tsmiDrawMode
            '
            Me.m_tsmiDrawMode.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiFill, Me.m_tsmiLine, Me.m_tsmiTSDriver, Me.m_tsmiTSRefAbs, Me.m_tsmiTSRefRel})
            Me.m_tsmiDrawMode.Name = "m_tsmiDrawMode"
            resources.ApplyResources(Me.m_tsmiDrawMode, "m_tsmiDrawMode")
            '
            'm_tsmiFill
            '
            Me.m_tsmiFill.Name = "m_tsmiFill"
            resources.ApplyResources(Me.m_tsmiFill, "m_tsmiFill")
            '
            'm_tsmiLine
            '
            Me.m_tsmiLine.Name = "m_tsmiLine"
            resources.ApplyResources(Me.m_tsmiLine, "m_tsmiLine")
            '
            'm_tsmiTSDriver
            '
            Me.m_tsmiTSDriver.Name = "m_tsmiTSDriver"
            resources.ApplyResources(Me.m_tsmiTSDriver, "m_tsmiTSDriver")
            '
            'm_tsmiShowMarks
            '
            Me.m_tsmiShowMarks.Name = "m_tsmiShowMarks"
            resources.ApplyResources(Me.m_tsmiShowMarks, "m_tsmiShowMarks")
            '
            'm_tsmiAutoScaleYAxis
            '
            Me.m_tsmiAutoScaleYAxis.Name = "m_tsmiAutoScaleYAxis"
            resources.ApplyResources(Me.m_tsmiAutoScaleYAxis, "m_tsmiAutoScaleYAxis")
            '
            'm_tsmiReset
            '
            Me.m_tsmiReset.Name = "m_tsmiReset"
            resources.ApplyResources(Me.m_tsmiReset, "m_tsmiReset")
            '
            'm_tsmiValue
            '
            Me.m_tsmiValue.Name = "m_tsmiValue"
            resources.ApplyResources(Me.m_tsmiValue, "m_tsmiValue")
            '
            'LoadToolStripMenuItem
            '
            resources.ApplyResources(Me.LoadToolStripMenuItem, "LoadToolStripMenuItem")
            Me.LoadToolStripMenuItem.Name = "LoadToolStripMenuItem"
            '
            'm_tsmiSave
            '
            Me.m_tsmiSave.Name = "m_tsmiSave"
            resources.ApplyResources(Me.m_tsmiSave, "m_tsmiSave")
            '
            'm_tss2
            '
            Me.m_tss2.Name = "m_tss2"
            resources.ApplyResources(Me.m_tss2, "m_tss2")
            '
            'm_tsmiOptions
            '
            Me.m_tsmiOptions.Name = "m_tsmiOptions"
            resources.ApplyResources(Me.m_tsmiOptions, "m_tsmiOptions")
            '
            'm_tsmiTSRefAbs
            '
            Me.m_tsmiTSRefAbs.Name = "m_tsmiTSRefAbs"
            resources.ApplyResources(Me.m_tsmiTSRefAbs, "m_tsmiTSRefAbs")
            '
            'm_tsmiTSRefRel
            '
            Me.m_tsmiTSRefRel.Name = "m_tsmiTSRefRel"
            resources.ApplyResources(Me.m_tsmiTSRefRel, "m_tsmiTSRefRel")
            '
            'ucSketchPad
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.BackColor = System.Drawing.SystemColors.Window
            Me.ContextMenuStrip = Me.m_spContextMenuStrip
            Me.Cursor = System.Windows.Forms.Cursors.Cross
            Me.Name = "ucSketchPad"
            Me.m_spContextMenuStrip.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents LoadToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_spContextMenuStrip As System.Windows.Forms.ContextMenuStrip
        Private WithEvents m_tsmiDrawMode As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiFill As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiLine As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiTSDriver As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiAutoScaleYAxis As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiReset As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiValue As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiSave As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tss2 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_tsmiOptions As System.Windows.Forms.ToolStripMenuItem
        Protected WithEvents m_tsmiShowMarks As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiTSRefAbs As ToolStripMenuItem
        Private WithEvents m_tsmiTSRefRel As ToolStripMenuItem
    End Class

End Namespace
