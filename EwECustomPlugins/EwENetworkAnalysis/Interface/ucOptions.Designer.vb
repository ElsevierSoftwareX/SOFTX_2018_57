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
Partial Class ucOptions
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucOptions))
        Me.m_hdrOptions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_cbUseTimeout = New System.Windows.Forms.CheckBox()
        Me.m_lblTimeout = New System.Windows.Forms.Label()
        Me.m_nudTimeOut = New System.Windows.Forms.NumericUpDown()
        Me.m_lblTimeOutUnit = New System.Windows.Forms.Label()
        Me.m_hdrAutosave = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_cbAutosaveRoot = New System.Windows.Forms.CheckBox()
        Me.m_cbAutosaveEcopath = New System.Windows.Forms.CheckBox()
        Me.m_cbAutosaveEcosimWoPPR = New System.Windows.Forms.CheckBox()
        Me.m_cbAutosaveEcosimWithPPR = New System.Windows.Forms.CheckBox()
        CType(Me.m_nudTimeOut, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_hdrOptions
        '
        resources.ApplyResources(Me.m_hdrOptions, "m_hdrOptions")
        Me.m_hdrOptions.CanCollapseParent = False
        Me.m_hdrOptions.CollapsedParentHeight = 0
        Me.m_hdrOptions.IsCollapsed = False
        Me.m_hdrOptions.Name = "m_hdrOptions"
        '
        'm_cbUseTimeout
        '
        resources.ApplyResources(Me.m_cbUseTimeout, "m_cbUseTimeout")
        Me.m_cbUseTimeout.Name = "m_cbUseTimeout"
        Me.m_cbUseTimeout.UseVisualStyleBackColor = True
        '
        'm_lblTimeout
        '
        resources.ApplyResources(Me.m_lblTimeout, "m_lblTimeout")
        Me.m_lblTimeout.Name = "m_lblTimeout"
        '
        'm_nudTimeOut
        '
        resources.ApplyResources(Me.m_nudTimeOut, "m_nudTimeOut")
        Me.m_nudTimeOut.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.m_nudTimeOut.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.m_nudTimeOut.Name = "m_nudTimeOut"
        Me.m_nudTimeOut.Value = New Decimal(New Integer() {30, 0, 0, 0})
        '
        'm_lblTimeOutUnit
        '
        resources.ApplyResources(Me.m_lblTimeOutUnit, "m_lblTimeOutUnit")
        Me.m_lblTimeOutUnit.Name = "m_lblTimeOutUnit"
        '
        'm_hdrAutosave
        '
        resources.ApplyResources(Me.m_hdrAutosave, "m_hdrAutosave")
        Me.m_hdrAutosave.CanCollapseParent = False
        Me.m_hdrAutosave.CollapsedParentHeight = 0
        Me.m_hdrAutosave.IsCollapsed = False
        Me.m_hdrAutosave.Name = "m_hdrAutosave"
        '
        'm_cbAutosaveRoot
        '
        resources.ApplyResources(Me.m_cbAutosaveRoot, "m_cbAutosaveRoot")
        Me.m_cbAutosaveRoot.Name = "m_cbAutosaveRoot"
        Me.m_cbAutosaveRoot.UseVisualStyleBackColor = True
        '
        'm_cbAutosaveEcopath
        '
        resources.ApplyResources(Me.m_cbAutosaveEcopath, "m_cbAutosaveEcopath")
        Me.m_cbAutosaveEcopath.Name = "m_cbAutosaveEcopath"
        Me.m_cbAutosaveEcopath.UseVisualStyleBackColor = True
        '
        'm_cbAutosaveEcosimWoPPR
        '
        resources.ApplyResources(Me.m_cbAutosaveEcosimWoPPR, "m_cbAutosaveEcosimWoPPR")
        Me.m_cbAutosaveEcosimWoPPR.Name = "m_cbAutosaveEcosimWoPPR"
        Me.m_cbAutosaveEcosimWoPPR.UseVisualStyleBackColor = True
        '
        'm_cbAutosaveEcosimWithPPR
        '
        resources.ApplyResources(Me.m_cbAutosaveEcosimWithPPR, "m_cbAutosaveEcosimWithPPR")
        Me.m_cbAutosaveEcosimWithPPR.Name = "m_cbAutosaveEcosimWithPPR"
        Me.m_cbAutosaveEcosimWithPPR.UseVisualStyleBackColor = True
        '
        'ucOptions
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_cbAutosaveEcosimWithPPR)
        Me.Controls.Add(Me.m_cbAutosaveEcosimWoPPR)
        Me.Controls.Add(Me.m_cbAutosaveEcopath)
        Me.Controls.Add(Me.m_cbAutosaveRoot)
        Me.Controls.Add(Me.m_hdrAutosave)
        Me.Controls.Add(Me.m_lblTimeOutUnit)
        Me.Controls.Add(Me.m_nudTimeOut)
        Me.Controls.Add(Me.m_lblTimeout)
        Me.Controls.Add(Me.m_cbUseTimeout)
        Me.Controls.Add(Me.m_hdrOptions)
        Me.Name = "ucOptions"
        CType(Me.m_nudTimeOut, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_hdrOptions As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_cbUseTimeout As System.Windows.Forms.CheckBox
    Private WithEvents m_lblTimeout As System.Windows.Forms.Label
    Private WithEvents m_nudTimeOut As System.Windows.Forms.NumericUpDown
    Private WithEvents m_lblTimeOutUnit As System.Windows.Forms.Label
    Private WithEvents m_hdrAutosave As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_cbAutosaveRoot As System.Windows.Forms.CheckBox
    Private WithEvents m_cbAutosaveEcopath As System.Windows.Forms.CheckBox
    Private WithEvents m_cbAutosaveEcosimWoPPR As System.Windows.Forms.CheckBox
    Private WithEvents m_cbAutosaveEcosimWithPPR As System.Windows.Forms.CheckBox
End Class
