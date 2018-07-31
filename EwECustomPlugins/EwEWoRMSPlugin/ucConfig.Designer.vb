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
Partial Class ucConfig
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucConfig))
        Me.m_pbWoRMS = New System.Windows.Forms.PictureBox()
        Me.m_lblConnTO = New System.Windows.Forms.Label()
        Me.m_nudConnTO = New System.Windows.Forms.NumericUpDown()
        Me.m_lblSecs1 = New System.Windows.Forms.Label()
        Me.m_lblReplyTO = New System.Windows.Forms.Label()
        Me.m_lblSecs2 = New System.Windows.Forms.Label()
        Me.m_nudReplyTO = New System.Windows.Forms.NumericUpDown()
        Me.m_pbBlueBridge = New System.Windows.Forms.PictureBox()
        CType(Me.m_pbWoRMS, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudConnTO, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudReplyTO, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbBlueBridge, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_pbWoRMS
        '
        resources.ApplyResources(Me.m_pbWoRMS, "m_pbWoRMS")
        Me.m_pbWoRMS.BackColor = System.Drawing.Color.White
        Me.m_pbWoRMS.Name = "m_pbWoRMS"
        Me.m_pbWoRMS.TabStop = False
        '
        'm_lblConnTO
        '
        resources.ApplyResources(Me.m_lblConnTO, "m_lblConnTO")
        Me.m_lblConnTO.Name = "m_lblConnTO"
        '
        'm_nudConnTO
        '
        resources.ApplyResources(Me.m_nudConnTO, "m_nudConnTO")
        Me.m_nudConnTO.Maximum = New Decimal(New Integer() {300, 0, 0, 0})
        Me.m_nudConnTO.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.m_nudConnTO.Name = "m_nudConnTO"
        Me.m_nudConnTO.Value = New Decimal(New Integer() {60, 0, 0, 0})
        '
        'm_lblSecs1
        '
        resources.ApplyResources(Me.m_lblSecs1, "m_lblSecs1")
        Me.m_lblSecs1.Name = "m_lblSecs1"
        '
        'm_lblReplyTO
        '
        resources.ApplyResources(Me.m_lblReplyTO, "m_lblReplyTO")
        Me.m_lblReplyTO.Name = "m_lblReplyTO"
        '
        'm_lblSecs2
        '
        resources.ApplyResources(Me.m_lblSecs2, "m_lblSecs2")
        Me.m_lblSecs2.Name = "m_lblSecs2"
        '
        'm_nudReplyTO
        '
        resources.ApplyResources(Me.m_nudReplyTO, "m_nudReplyTO")
        Me.m_nudReplyTO.Maximum = New Decimal(New Integer() {600, 0, 0, 0})
        Me.m_nudReplyTO.Minimum = New Decimal(New Integer() {30, 0, 0, 0})
        Me.m_nudReplyTO.Name = "m_nudReplyTO"
        Me.m_nudReplyTO.Value = New Decimal(New Integer() {300, 0, 0, 0})
        '
        'm_pbBlueBridge
        '
        resources.ApplyResources(Me.m_pbBlueBridge, "m_pbBlueBridge")
        Me.m_pbBlueBridge.BackgroundImage = Global.EwEWoRMSPlugin.My.Resources.Resources.BlueBridge_xparent
        Me.m_pbBlueBridge.Name = "m_pbBlueBridge"
        Me.m_pbBlueBridge.TabStop = False
        '
        'ucConfig
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_pbBlueBridge)
        Me.Controls.Add(Me.m_nudReplyTO)
        Me.Controls.Add(Me.m_lblSecs2)
        Me.Controls.Add(Me.m_nudConnTO)
        Me.Controls.Add(Me.m_lblReplyTO)
        Me.Controls.Add(Me.m_lblSecs1)
        Me.Controls.Add(Me.m_lblConnTO)
        Me.Controls.Add(Me.m_pbWoRMS)
        Me.Name = "ucConfig"
        CType(Me.m_pbWoRMS, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudConnTO, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudReplyTO, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbBlueBridge, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_pbWoRMS As System.Windows.Forms.PictureBox
    Private WithEvents m_lblConnTO As System.Windows.Forms.Label
    Private WithEvents m_nudConnTO As System.Windows.Forms.NumericUpDown
    Private WithEvents m_lblSecs1 As System.Windows.Forms.Label
    Private WithEvents m_lblReplyTO As System.Windows.Forms.Label
    Private WithEvents m_lblSecs2 As System.Windows.Forms.Label
    Private WithEvents m_nudReplyTO As System.Windows.Forms.NumericUpDown
    Private WithEvents m_pbBlueBridge As System.Windows.Forms.PictureBox
End Class
