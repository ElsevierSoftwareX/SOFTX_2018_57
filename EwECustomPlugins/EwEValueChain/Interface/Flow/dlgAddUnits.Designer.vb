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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class dlgAddUnits
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.m_btnSelNone = New System.Windows.Forms.Button()
        Me.m_btnAll = New System.Windows.Forms.Button()
        Me.OK_BUTTON = New System.Windows.Forms.Button()
        Me.CANCEL_BUTTON = New System.Windows.Forms.Button()
        Me.m_clbUnits = New System.Windows.Forms.CheckedListBox()
        Me.SuspendLayout()
        '
        'm_btnSelNone
        '
        Me.m_btnSelNone.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnSelNone.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.m_btnSelNone.Location = New System.Drawing.Point(205, 41)
        Me.m_btnSelNone.Name = "m_btnSelNone"
        Me.m_btnSelNone.Size = New System.Drawing.Size(75, 23)
        Me.m_btnSelNone.TabIndex = 2
        Me.m_btnSelNone.Text = "&None"
        Me.m_btnSelNone.UseVisualStyleBackColor = True
        '
        'm_btnAll
        '
        Me.m_btnAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_btnAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.m_btnAll.Location = New System.Drawing.Point(205, 12)
        Me.m_btnAll.Name = "m_btnAll"
        Me.m_btnAll.Size = New System.Drawing.Size(75, 23)
        Me.m_btnAll.TabIndex = 1
        Me.m_btnAll.Text = "&All"
        Me.m_btnAll.UseVisualStyleBackColor = True
        '
        'OK_BUTTON
        '
        Me.OK_BUTTON.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.OK_BUTTON.Location = New System.Drawing.Point(124, 238)
        Me.OK_BUTTON.Name = "OK_BUTTON"
        Me.OK_BUTTON.Size = New System.Drawing.Size(75, 23)
        Me.OK_BUTTON.TabIndex = 3
        Me.OK_BUTTON.Text = "OK"
        Me.OK_BUTTON.UseVisualStyleBackColor = True
        '
        'CANCEL_BUTTON
        '
        Me.CANCEL_BUTTON.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CANCEL_BUTTON.Location = New System.Drawing.Point(205, 238)
        Me.CANCEL_BUTTON.Name = "CANCEL_BUTTON"
        Me.CANCEL_BUTTON.Size = New System.Drawing.Size(75, 23)
        Me.CANCEL_BUTTON.TabIndex = 4
        Me.CANCEL_BUTTON.Text = "Cancel"
        Me.CANCEL_BUTTON.UseVisualStyleBackColor = True
        '
        'm_clbUnits
        '
        Me.m_clbUnits.CheckOnClick = True
        Me.m_clbUnits.FormattingEnabled = True
        Me.m_clbUnits.IntegralHeight = False
        Me.m_clbUnits.Location = New System.Drawing.Point(12, 12)
        Me.m_clbUnits.Name = "m_clbUnits"
        Me.m_clbUnits.Size = New System.Drawing.Size(187, 220)
        Me.m_clbUnits.TabIndex = 0
        '
        'dlgAddUnits
        '
        Me.AcceptButton = Me.OK_BUTTON
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(292, 273)
        Me.ControlBox = False
        Me.Controls.Add(Me.m_clbUnits)
        Me.Controls.Add(Me.CANCEL_BUTTON)
        Me.Controls.Add(Me.OK_BUTTON)
        Me.Controls.Add(Me.m_btnAll)
        Me.Controls.Add(Me.m_btnSelNone)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgAddUnits"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.Text = "Select units to add"
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_btnSelNone As System.Windows.Forms.Button
    Private WithEvents m_btnAll As System.Windows.Forms.Button
    Private WithEvents OK_BUTTON As System.Windows.Forms.Button
    Private WithEvents CANCEL_BUTTON As System.Windows.Forms.Button
    Private WithEvents m_clbUnits As System.Windows.Forms.CheckedListBox
End Class
