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

Namespace Controls.Map.Layers

    Partial Class ucLayerEditorRegion
        Inherits ucLayerEditorDefault

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.m_lblRegion = New System.Windows.Forms.Label()
            Me.m_nudRegion = New System.Windows.Forms.NumericUpDown()
            CType(Me.m_nudRegion, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_lblRegion
            '
            Me.m_lblRegion.AutoSize = True
            Me.m_lblRegion.Location = New System.Drawing.Point(3, 153)
            Me.m_lblRegion.Name = "m_lblRegion"
            Me.m_lblRegion.Size = New System.Drawing.Size(44, 13)
            Me.m_lblRegion.TabIndex = 11
            Me.m_lblRegion.Text = "&Region:"
            '
            'm_nudRegion
            '
            Me.m_nudRegion.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_nudRegion.Location = New System.Drawing.Point(68, 151)
            Me.m_nudRegion.Name = "m_nudRegion"
            Me.m_nudRegion.Size = New System.Drawing.Size(129, 20)
            Me.m_nudRegion.TabIndex = 12
            '
            'ucLayerEditorRegion
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_lblRegion)
            Me.Controls.Add(Me.m_nudRegion)
            Me.Name = "ucLayerEditorRegion"
            Me.Size = New System.Drawing.Size(200, 178)
            Me.Controls.SetChildIndex(Me.m_nudRegion, 0)
            Me.Controls.SetChildIndex(Me.m_lblRegion, 0)
            CType(Me.m_nudRegion, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_lblRegion As System.Windows.Forms.Label
        Private WithEvents m_nudRegion As System.Windows.Forms.NumericUpDown

    End Class

End Namespace
