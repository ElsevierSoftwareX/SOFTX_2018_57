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

Namespace Ecospace

    Partial Class dlgDefineRegions
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
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
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgDefineRegions))
            Me.m_nudNoRegions = New System.Windows.Forms.NumericUpDown()
            Me.m_btnOK = New System.Windows.Forms.Button()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_rbCustomMax = New System.Windows.Forms.RadioButton()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.m_rbFromMPAs = New System.Windows.Forms.RadioButton()
            Me.m_rbFromHabitats = New System.Windows.Forms.RadioButton()
            CType(Me.m_nudNoRegions, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_nudNoRegions
            '
            resources.ApplyResources(Me.m_nudNoRegions, "m_nudNoRegions")
            Me.m_nudNoRegions.Name = "m_nudNoRegions"
            '
            'm_btnOK
            '
            resources.ApplyResources(Me.m_btnOK, "m_btnOK")
            Me.m_btnOK.Name = "m_btnOK"
            Me.m_btnOK.UseVisualStyleBackColor = True
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            Me.m_btnCancel.UseVisualStyleBackColor = True
            '
            'm_rbCustomMax
            '
            resources.ApplyResources(Me.m_rbCustomMax, "m_rbCustomMax")
            Me.m_rbCustomMax.Name = "m_rbCustomMax"
            Me.m_rbCustomMax.TabStop = True
            Me.m_rbCustomMax.UseVisualStyleBackColor = True
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'm_rbFromMPAs
            '
            resources.ApplyResources(Me.m_rbFromMPAs, "m_rbFromMPAs")
            Me.m_rbFromMPAs.Name = "m_rbFromMPAs"
            Me.m_rbFromMPAs.TabStop = True
            Me.m_rbFromMPAs.UseVisualStyleBackColor = True
            '
            'm_rbFromHabitats
            '
            resources.ApplyResources(Me.m_rbFromHabitats, "m_rbFromHabitats")
            Me.m_rbFromHabitats.Name = "m_rbFromHabitats"
            Me.m_rbFromHabitats.TabStop = True
            Me.m_rbFromHabitats.UseVisualStyleBackColor = True
            '
            'dlgDefineRegions
            '
            Me.AcceptButton = Me.m_btnOK
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnCancel
            Me.ControlBox = False
            Me.Controls.Add(Me.m_rbFromHabitats)
            Me.Controls.Add(Me.m_rbFromMPAs)
            Me.Controls.Add(Me.m_rbCustomMax)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.m_btnCancel)
            Me.Controls.Add(Me.m_btnOK)
            Me.Controls.Add(Me.m_nudNoRegions)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgDefineRegions"
            Me.ShowInTaskbar = False
            CType(Me.m_nudNoRegions, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_nudNoRegions As System.Windows.Forms.NumericUpDown
        Private WithEvents m_btnOK As System.Windows.Forms.Button
        Private WithEvents m_btnCancel As System.Windows.Forms.Button
        Private WithEvents m_rbCustomMax As System.Windows.Forms.RadioButton
        Private WithEvents Label1 As System.Windows.Forms.Label
        Private WithEvents m_rbFromMPAs As System.Windows.Forms.RadioButton
        Private WithEvents m_rbFromHabitats As System.Windows.Forms.RadioButton
    End Class

End Namespace
