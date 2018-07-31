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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFLEMReader
    Inherits ScientificInterfaceShared.Forms.frmEwE

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmFLEMReader))
        Me.m_chkForcePP = New System.Windows.Forms.CheckBox()
        Me.m_btnChooseFile = New System.Windows.Forms.Button()
        Me.m_chkForceHabCap = New System.Windows.Forms.CheckBox()
        Me.m_cmbHabCap = New System.Windows.Forms.ComboBox()
        Me.m_lblGroup = New System.Windows.Forms.Label()
        Me.m_lblForcingFile = New System.Windows.Forms.Label()
        Me.m_tbxForceFile = New System.Windows.Forms.TextBox()
        Me.m_btnOK = New System.Windows.Forms.Button()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_chkUsePPMod = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'm_chkForcePP
        '
        resources.ApplyResources(Me.m_chkForcePP, "m_chkForcePP")
        Me.m_chkForcePP.Name = "m_chkForcePP"
        Me.m_chkForcePP.UseVisualStyleBackColor = True
        '
        'm_btnChooseFile
        '
        resources.ApplyResources(Me.m_btnChooseFile, "m_btnChooseFile")
        Me.m_btnChooseFile.Name = "m_btnChooseFile"
        Me.m_btnChooseFile.UseVisualStyleBackColor = True
        '
        'm_chkForceHabCap
        '
        resources.ApplyResources(Me.m_chkForceHabCap, "m_chkForceHabCap")
        Me.m_chkForceHabCap.Name = "m_chkForceHabCap"
        Me.m_chkForceHabCap.UseVisualStyleBackColor = True
        '
        'm_cmbHabCap
        '
        resources.ApplyResources(Me.m_cmbHabCap, "m_cmbHabCap")
        Me.m_cmbHabCap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbHabCap.FormattingEnabled = True
        Me.m_cmbHabCap.Name = "m_cmbHabCap"
        '
        'm_lblGroup
        '
        resources.ApplyResources(Me.m_lblGroup, "m_lblGroup")
        Me.m_lblGroup.Name = "m_lblGroup"
        '
        'm_lblForcingFile
        '
        resources.ApplyResources(Me.m_lblForcingFile, "m_lblForcingFile")
        Me.m_lblForcingFile.Name = "m_lblForcingFile"
        '
        'm_tbxForceFile
        '
        resources.ApplyResources(Me.m_tbxForceFile, "m_tbxForceFile")
        Me.m_tbxForceFile.Name = "m_tbxForceFile"
        Me.m_tbxForceFile.ReadOnly = True
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
        'm_chkUsePPMod
        '
        resources.ApplyResources(Me.m_chkUsePPMod, "m_chkUsePPMod")
        Me.m_chkUsePPMod.Name = "m_chkUsePPMod"
        Me.m_chkUsePPMod.UseVisualStyleBackColor = True
        '
        'frmFLEMReader
        '
        Me.AcceptButton = Me.m_btnOK
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_chkUsePPMod)
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_btnOK)
        Me.Controls.Add(Me.m_tbxForceFile)
        Me.Controls.Add(Me.m_lblForcingFile)
        Me.Controls.Add(Me.m_lblGroup)
        Me.Controls.Add(Me.m_cmbHabCap)
        Me.Controls.Add(Me.m_chkForceHabCap)
        Me.Controls.Add(Me.m_btnChooseFile)
        Me.Controls.Add(Me.m_chkForcePP)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmFLEMReader"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_chkForcePP As System.Windows.Forms.CheckBox
    Private WithEvents m_chkForceHabCap As System.Windows.Forms.CheckBox
    Private WithEvents m_lblGroup As System.Windows.Forms.Label
    Private WithEvents m_lblForcingFile As System.Windows.Forms.Label
    Private WithEvents m_tbxForceFile As System.Windows.Forms.TextBox
    Private WithEvents m_btnOK As System.Windows.Forms.Button
    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_cmbHabCap As System.Windows.Forms.ComboBox
    Private WithEvents m_btnChooseFile As System.Windows.Forms.Button
    Friend WithEvents m_chkUsePPMod As System.Windows.Forms.CheckBox
End Class
