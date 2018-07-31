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

    Partial Class ucHoverMenu
        Inherits UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.m_ts = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.SuspendLayout()
            '
            'm_ts
            '
            Me.m_ts.BackColor = System.Drawing.SystemColors.Control
            Me.m_ts.CanOverflow = False
            Me.m_ts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_ts.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow
            Me.m_ts.Location = New System.Drawing.Point(0, 0)
            Me.m_ts.Name = "m_ts"
            Me.m_ts.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            Me.m_ts.Size = New System.Drawing.Size(800, 19)
            Me.m_ts.TabIndex = 0
            '
            'ucHoverMenu
            '
            Me.AutoSize = True
            Me.BackColor = System.Drawing.SystemColors.ButtonFace
            Me.Controls.Add(Me.m_ts)
            Me.Name = "ucHoverMenu"
            Me.Size = New System.Drawing.Size(800, 23)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_ts As cEwEToolstrip
    End Class

End Namespace ' Controls