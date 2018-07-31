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

Namespace Ecosim

    Partial Class dlgScaleVs
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgScaleVs))
            Me.m_lblLow = New System.Windows.Forms.Label()
            Me.m_tbxLow = New System.Windows.Forms.TextBox()
            Me.m_lblHigh = New System.Windows.Forms.Label()
            Me.m_tbxHigh = New System.Windows.Forms.TextBox()
            Me.m_btnOK = New System.Windows.Forms.Button()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.SuspendLayout()
            '
            'm_lblLow
            '
            resources.ApplyResources(Me.m_lblLow, "m_lblLow")
            Me.m_lblLow.Name = "m_lblLow"
            '
            'm_tbxLow
            '
            resources.ApplyResources(Me.m_tbxLow, "m_tbxLow")
            Me.m_tbxLow.Name = "m_tbxLow"
            '
            'm_lblHigh
            '
            resources.ApplyResources(Me.m_lblHigh, "m_lblHigh")
            Me.m_lblHigh.Name = "m_lblHigh"
            '
            'm_tbxHigh
            '
            resources.ApplyResources(Me.m_tbxHigh, "m_tbxHigh")
            Me.m_tbxHigh.Name = "m_tbxHigh"
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
            Me.m_btnCancel.Name = "m_btnCancel"
            Me.m_btnCancel.UseVisualStyleBackColor = True
            '
            'dlgScaleVs
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ControlBox = False
            Me.Controls.Add(Me.m_btnCancel)
            Me.Controls.Add(Me.m_btnOK)
            Me.Controls.Add(Me.m_tbxHigh)
            Me.Controls.Add(Me.m_lblHigh)
            Me.Controls.Add(Me.m_tbxLow)
            Me.Controls.Add(Me.m_lblLow)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.Name = "dlgScaleVs"
            Me.ShowInTaskbar = False
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_btnOK As Button
        Private WithEvents m_btnCancel As Button
        Private WithEvents m_lblLow As Label
        Private WithEvents m_tbxLow As TextBox
        Private WithEvents m_tbxHigh As TextBox
        Private WithEvents m_lblHigh As Label
    End Class

End Namespace