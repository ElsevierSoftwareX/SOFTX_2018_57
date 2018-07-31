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

    Partial Class ucLayerEditorPort
        Inherits ucLayerEditorDefault

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
            Me.m_btnClear = New System.Windows.Forms.Button()
            Me.m_btnSet = New System.Windows.Forms.Button()
            Me.m_cmbFleet = New System.Windows.Forms.ComboBox()
            Me.m_lblFleet = New System.Windows.Forms.Label()
            Me.m_tlpButtons.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_tlpButtons
            '
            Me.m_tlpButtons.ColumnCount = 2
            Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.44444!))
            Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55.55556!))
            Me.m_tlpButtons.Controls.Add(Me.m_btnClear, 0, 0)
            Me.m_tlpButtons.Controls.Add(Me.m_btnSet, 1, 0)
            Me.m_tlpButtons.Location = New System.Drawing.Point(68, 178)
            Me.m_tlpButtons.Name = "m_tlpButtons"
            Me.m_tlpButtons.RowCount = 1
            Me.m_tlpButtons.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpButtons.Size = New System.Drawing.Size(129, 23)
            Me.m_tlpButtons.TabIndex = 3
            '
            'm_btnClear
            '
            Me.m_btnClear.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_btnClear.Location = New System.Drawing.Point(0, 0)
            Me.m_btnClear.Margin = New System.Windows.Forms.Padding(0, 0, 3, 0)
            Me.m_btnClear.Name = "m_btnClear"
            Me.m_btnClear.Size = New System.Drawing.Size(54, 23)
            Me.m_btnClear.TabIndex = 0
            Me.m_btnClear.Text = "&Clear"
            Me.m_btnClear.UseVisualStyleBackColor = True
            '
            'm_btnSet
            '
            Me.m_btnSet.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_btnSet.Location = New System.Drawing.Point(60, 0)
            Me.m_btnSet.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
            Me.m_btnSet.Name = "m_btnSet"
            Me.m_btnSet.Size = New System.Drawing.Size(69, 23)
            Me.m_btnSet.TabIndex = 0
            Me.m_btnSet.Text = "&All coasts"
            Me.m_btnSet.UseVisualStyleBackColor = True
            '
            'm_cmbFleet
            '
            Me.m_cmbFleet.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_cmbFleet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbFleet.FormattingEnabled = True
            Me.m_cmbFleet.Location = New System.Drawing.Point(68, 151)
            Me.m_cmbFleet.MaxDropDownItems = 12
            Me.m_cmbFleet.Name = "m_cmbFleet"
            Me.m_cmbFleet.Size = New System.Drawing.Size(129, 21)
            Me.m_cmbFleet.TabIndex = 5
            '
            'm_lblFleet
            '
            Me.m_lblFleet.AutoSize = True
            Me.m_lblFleet.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_lblFleet.Location = New System.Drawing.Point(3, 154)
            Me.m_lblFleet.Name = "m_lblFleet"
            Me.m_lblFleet.Size = New System.Drawing.Size(33, 13)
            Me.m_lblFleet.TabIndex = 4
            Me.m_lblFleet.Text = "&Fleet:"
            '
            'ucLayerEditorPort
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_cmbFleet)
            Me.Controls.Add(Me.m_lblFleet)
            Me.Controls.Add(Me.m_tlpButtons)
            Me.Name = "ucLayerEditorPort"
            Me.Size = New System.Drawing.Size(200, 211)
            Me.Controls.SetChildIndex(Me.m_tlpButtons, 0)
            Me.Controls.SetChildIndex(Me.m_lblFleet, 0)
            Me.Controls.SetChildIndex(Me.m_cmbFleet, 0)
            Me.m_tlpButtons.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_tlpButtons As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_btnClear As System.Windows.Forms.Button
        Private WithEvents m_btnSet As System.Windows.Forms.Button
        Private WithEvents m_cmbFleet As System.Windows.Forms.ComboBox
        Private WithEvents m_lblFleet As System.Windows.Forms.Label

    End Class

End Namespace
