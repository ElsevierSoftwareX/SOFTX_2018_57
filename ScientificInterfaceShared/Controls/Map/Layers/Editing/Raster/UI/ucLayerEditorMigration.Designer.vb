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

    Partial Class ucLayerEditorMigration
        Inherits ucLayerEditorRange

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucLayerEditorMigration))
            Me.m_lbMonth = New System.Windows.Forms.Label()
            Me.m_cmbMonth = New System.Windows.Forms.ComboBox()
            Me.m_btnNext = New System.Windows.Forms.Button()
            Me.SuspendLayout()
            '
            'm_lbMonth
            '
            resources.ApplyResources(Me.m_lbMonth, "m_lbMonth")
            Me.m_lbMonth.Name = "m_lbMonth"
            '
            'm_cmbMonth
            '
            resources.ApplyResources(Me.m_cmbMonth, "m_cmbMonth")
            Me.m_cmbMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbMonth.FormattingEnabled = True
            Me.m_cmbMonth.Items.AddRange(New Object() {resources.GetString("m_cmbMonth.Items"), resources.GetString("m_cmbMonth.Items1"), resources.GetString("m_cmbMonth.Items2"), resources.GetString("m_cmbMonth.Items3"), resources.GetString("m_cmbMonth.Items4"), resources.GetString("m_cmbMonth.Items5"), resources.GetString("m_cmbMonth.Items6"), resources.GetString("m_cmbMonth.Items7"), resources.GetString("m_cmbMonth.Items8"), resources.GetString("m_cmbMonth.Items9"), resources.GetString("m_cmbMonth.Items10"), resources.GetString("m_cmbMonth.Items11")})
            Me.m_cmbMonth.Name = "m_cmbMonth"
            '
            'm_btnNext
            '
            resources.ApplyResources(Me.m_btnNext, "m_btnNext")
            Me.m_btnNext.Name = "m_btnNext"
            Me.m_btnNext.UseVisualStyleBackColor = True
            '
            'ucLayerEditorMigration
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_btnNext)
            Me.Controls.Add(Me.m_cmbMonth)
            Me.Controls.Add(Me.m_lbMonth)
            Me.Name = "ucLayerEditorMigration"
            Me.Controls.SetChildIndex(Me.m_lbMonth, 0)
            Me.Controls.SetChildIndex(Me.m_cmbMonth, 0)
            Me.Controls.SetChildIndex(Me.m_btnNext, 0)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_lbMonth As System.Windows.Forms.Label
        Friend WithEvents m_cmbMonth As System.Windows.Forms.ComboBox
        Private WithEvents m_btnNext As System.Windows.Forms.Button

    End Class

End Namespace
