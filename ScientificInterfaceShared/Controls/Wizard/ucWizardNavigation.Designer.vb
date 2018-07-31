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

Namespace Controls.Wizard


    Partial Class ucWizardNavigation
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
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
            Me.m_tlpButtons = New System.Windows.Forms.TableLayoutPanel()
            Me.m_btnBack = New System.Windows.Forms.Button()
            Me.m_btnClose = New System.Windows.Forms.Button()
            Me.m_btnNext = New System.Windows.Forms.Button()
            Me.m_tlpButtons.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_tlpButtons
            '
            Me.m_tlpButtons.ColumnCount = 4
            Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.m_tlpButtons.Controls.Add(Me.m_btnBack, 0, 0)
            Me.m_tlpButtons.Controls.Add(Me.m_btnClose, 3, 0)
            Me.m_tlpButtons.Controls.Add(Me.m_btnNext, 2, 0)
            Me.m_tlpButtons.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_tlpButtons.Location = New System.Drawing.Point(0, 0)
            Me.m_tlpButtons.Margin = New System.Windows.Forms.Padding(0)
            Me.m_tlpButtons.MaximumSize = New System.Drawing.Size(5000, 23)
            Me.m_tlpButtons.MinimumSize = New System.Drawing.Size(100, 23)
            Me.m_tlpButtons.Name = "m_tlpButtons"
            Me.m_tlpButtons.RowCount = 1
            Me.m_tlpButtons.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpButtons.Size = New System.Drawing.Size(406, 23)
            Me.m_tlpButtons.TabIndex = 0
            '
            'm_btnBack
            '
            Me.m_btnBack.Location = New System.Drawing.Point(0, 0)
            Me.m_btnBack.Margin = New System.Windows.Forms.Padding(0, 0, 3, 0)
            Me.m_btnBack.Name = "m_btnBack"
            Me.m_btnBack.Size = New System.Drawing.Size(75, 23)
            Me.m_btnBack.TabIndex = 0
            Me.m_btnBack.Text = "&Back"
            Me.m_btnBack.UseVisualStyleBackColor = True
            '
            'm_btnClose
            '
            Me.m_btnClose.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_btnClose.Location = New System.Drawing.Point(331, 0)
            Me.m_btnClose.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
            Me.m_btnClose.Name = "m_btnClose"
            Me.m_btnClose.Size = New System.Drawing.Size(75, 23)
            Me.m_btnClose.TabIndex = 3
            Me.m_btnClose.Text = "&Cancel"
            Me.m_btnClose.UseVisualStyleBackColor = True
            '
            'm_btnNext
            '
            Me.m_btnNext.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_btnNext.Location = New System.Drawing.Point(253, 0)
            Me.m_btnNext.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
            Me.m_btnNext.Name = "m_btnNext"
            Me.m_btnNext.Size = New System.Drawing.Size(75, 23)
            Me.m_btnNext.TabIndex = 1
            Me.m_btnNext.Text = "&Next"
            Me.m_btnNext.UseVisualStyleBackColor = True
            '
            'ucWizardNavigation
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_tlpButtons)
            Me.Name = "ucWizardNavigation"
            Me.Size = New System.Drawing.Size(406, 23)
            Me.m_tlpButtons.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub

        Private WithEvents m_btnNext As System.Windows.Forms.Button
        Private WithEvents m_btnBack As System.Windows.Forms.Button
        Private WithEvents m_btnClose As System.Windows.Forms.Button
        Private WithEvents m_tlpButtons As System.Windows.Forms.TableLayoutPanel

    End Class

End Namespace
