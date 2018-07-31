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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgHarvestControlRule
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgHarvestControlRule))
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.m_cmbBiomassGroups = New System.Windows.Forms.ComboBox()
        Me.m_cmbFMortGroups = New System.Windows.Forms.ComboBox()
        Me.m_lblBiomassGroup = New System.Windows.Forms.Label()
        Me.m_lblFMortGroup = New System.Windows.Forms.Label()
        Me.m_tbxRule = New System.Windows.Forms.TextBox()
        Me.m_cmbTarg_Or_Cons = New System.Windows.Forms.ComboBox()
        Me.m_lblTarg_Or_Cons = New System.Windows.Forms.Label()
        Me.m_lblBiomassGroupInfo = New System.Windows.Forms.Label()
        Me.m_lblInfoFMortGroup = New System.Windows.Forms.Label()
        Me.m_lblHCRTypeInfo = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'OK_Button
        '
        resources.ApplyResources(Me.OK_Button, "OK_Button")
        Me.OK_Button.Name = "OK_Button"
        '
        'Cancel_Button
        '
        resources.ApplyResources(Me.Cancel_Button, "Cancel_Button")
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Name = "Cancel_Button"
        '
        'm_cmbBiomassGroups
        '
        Me.m_cmbBiomassGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbBiomassGroups.FormattingEnabled = True
        resources.ApplyResources(Me.m_cmbBiomassGroups, "m_cmbBiomassGroups")
        Me.m_cmbBiomassGroups.Name = "m_cmbBiomassGroups"
        '
        'm_cmbFMortGroups
        '
        Me.m_cmbFMortGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbFMortGroups.FormattingEnabled = True
        resources.ApplyResources(Me.m_cmbFMortGroups, "m_cmbFMortGroups")
        Me.m_cmbFMortGroups.Name = "m_cmbFMortGroups"
        '
        'm_lblBiomassGroup
        '
        resources.ApplyResources(Me.m_lblBiomassGroup, "m_lblBiomassGroup")
        Me.m_lblBiomassGroup.Name = "m_lblBiomassGroup"
        '
        'm_lblFMortGroup
        '
        resources.ApplyResources(Me.m_lblFMortGroup, "m_lblFMortGroup")
        Me.m_lblFMortGroup.Name = "m_lblFMortGroup"
        '
        'm_tbxRule
        '
        resources.ApplyResources(Me.m_tbxRule, "m_tbxRule")
        Me.m_tbxRule.Name = "m_tbxRule"
        '
        'm_cmbTarg_Or_Cons
        '
        Me.m_cmbTarg_Or_Cons.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbTarg_Or_Cons.FormattingEnabled = True
        resources.ApplyResources(Me.m_cmbTarg_Or_Cons, "m_cmbTarg_Or_Cons")
        Me.m_cmbTarg_Or_Cons.Name = "m_cmbTarg_Or_Cons"
        '
        'm_lblTarg_Or_Cons
        '
        resources.ApplyResources(Me.m_lblTarg_Or_Cons, "m_lblTarg_Or_Cons")
        Me.m_lblTarg_Or_Cons.Name = "m_lblTarg_Or_Cons"
        '
        'm_lblBiomassGroupInfo
        '
        resources.ApplyResources(Me.m_lblBiomassGroupInfo, "m_lblBiomassGroupInfo")
        Me.m_lblBiomassGroupInfo.Name = "m_lblBiomassGroupInfo"
        '
        'm_lblInfoFMortGroup
        '
        resources.ApplyResources(Me.m_lblInfoFMortGroup, "m_lblInfoFMortGroup")
        Me.m_lblInfoFMortGroup.Name = "m_lblInfoFMortGroup"
        '
        'm_lblHCRTypeInfo
        '
        resources.ApplyResources(Me.m_lblHCRTypeInfo, "m_lblHCRTypeInfo")
        Me.m_lblHCRTypeInfo.Name = "m_lblHCRTypeInfo"
        '
        'dlgHarvestControlRule
        '
        Me.AcceptButton = Me.OK_Button
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.Controls.Add(Me.Cancel_Button)
        Me.Controls.Add(Me.OK_Button)
        Me.Controls.Add(Me.m_lblHCRTypeInfo)
        Me.Controls.Add(Me.m_lblInfoFMortGroup)
        Me.Controls.Add(Me.m_lblBiomassGroupInfo)
        Me.Controls.Add(Me.m_lblTarg_Or_Cons)
        Me.Controls.Add(Me.m_cmbTarg_Or_Cons)
        Me.Controls.Add(Me.m_tbxRule)
        Me.Controls.Add(Me.m_lblFMortGroup)
        Me.Controls.Add(Me.m_lblBiomassGroup)
        Me.Controls.Add(Me.m_cmbFMortGroups)
        Me.Controls.Add(Me.m_cmbBiomassGroups)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MinimizeBox = False
        Me.Name = "dlgHarvestControlRule"
        Me.ShowInTaskbar = False
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_lblBiomassGroup As System.Windows.Forms.Label
    Private WithEvents m_lblFMortGroup As System.Windows.Forms.Label
    Private WithEvents m_lblInfoFMortGroup As System.Windows.Forms.Label
    Private WithEvents m_lblBiomassGroupInfo As System.Windows.Forms.Label
    Private WithEvents m_lblHCRTypeInfo As System.Windows.Forms.Label
    Private WithEvents m_lblTarg_Or_Cons As System.Windows.Forms.Label
    Private WithEvents m_cmbTarg_Or_Cons As System.Windows.Forms.ComboBox
    Private WithEvents m_cmbBiomassGroups As System.Windows.Forms.ComboBox
    Private WithEvents m_cmbFMortGroups As System.Windows.Forms.ComboBox
    Private WithEvents m_tbxRule As System.Windows.Forms.TextBox
    Private WithEvents OK_Button As System.Windows.Forms.Button
    Private WithEvents Cancel_Button As System.Windows.Forms.Button

End Class
