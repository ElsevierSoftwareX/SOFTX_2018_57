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

    Partial Class ucLayerEditorRange
        Inherits ucLayerEditorDefault

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucLayerEditorRange))
            Me.m_lbValue = New System.Windows.Forms.Label()
            Me.m_nudValue = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_pbPreview = New System.Windows.Forms.PictureBox()
            Me.m_btnSmooth = New System.Windows.Forms.Button()
            Me.m_btnReset = New System.Windows.Forms.Button()
            CType(Me.m_nudValue, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_pbPreview, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_lbValue
            '
            resources.ApplyResources(Me.m_lbValue, "m_lbValue")
            Me.m_lbValue.Name = "m_lbValue"
            '
            'm_nudValue
            '
            resources.ApplyResources(Me.m_nudValue, "m_nudValue")
            Me.m_nudValue.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudValue.Name = "m_nudValue"
            '
            'm_pbPreview
            '
            resources.ApplyResources(Me.m_pbPreview, "m_pbPreview")
            Me.m_pbPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_pbPreview.Name = "m_pbPreview"
            Me.m_pbPreview.TabStop = False
            '
            'm_btnSmooth
            '
            resources.ApplyResources(Me.m_btnSmooth, "m_btnSmooth")
            Me.m_btnSmooth.Name = "m_btnSmooth"
            Me.m_btnSmooth.UseVisualStyleBackColor = True
            '
            'm_btnReset
            '
            resources.ApplyResources(Me.m_btnReset, "m_btnReset")
            Me.m_btnReset.Image = Global.ScientificInterfaceShared.My.Resources.Resources.ResetHS
            Me.m_btnReset.Name = "m_btnReset"
            Me.m_btnReset.UseVisualStyleBackColor = True
            '
            'ucLayerEditorRange
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_btnReset)
            Me.Controls.Add(Me.m_btnSmooth)
            Me.Controls.Add(Me.m_pbPreview)
            Me.Controls.Add(Me.m_nudValue)
            Me.Controls.Add(Me.m_lbValue)
            Me.Name = "ucLayerEditorRange"
            Me.Controls.SetChildIndex(Me.m_lbValue, 0)
            Me.Controls.SetChildIndex(Me.m_nudValue, 0)
            Me.Controls.SetChildIndex(Me.m_pbPreview, 0)
            Me.Controls.SetChildIndex(Me.m_btnSmooth, 0)
            Me.Controls.SetChildIndex(Me.m_btnReset, 0)
            CType(Me.m_nudValue, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_pbPreview, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_lbValue As System.Windows.Forms.Label
        Private WithEvents m_btnSmooth As System.Windows.Forms.Button
        Private WithEvents m_nudValue As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_btnReset As System.Windows.Forms.Button
        Private WithEvents m_pbPreview As System.Windows.Forms.PictureBox

    End Class

End Namespace
