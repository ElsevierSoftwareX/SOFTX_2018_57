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

Imports SharedResources = ScientificInterfaceShared.My.Resources

Partial Class frmUpdateComponents
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmUpdateComponents))
        Me.m_lblInfo = New System.Windows.Forms.Label()
        Me.m_pbProgress = New System.Windows.Forms.ProgressBar()
        Me.m_btnAbort = New System.Windows.Forms.Button()
        Me.m_tlpButtons = New System.Windows.Forms.TableLayoutPanel()
        Me.m_pbLogo = New System.Windows.Forms.PictureBox()
        Me.m_cbAutoUpdatePlugins = New System.Windows.Forms.CheckBox()
        Me.m_tlpButtons.SuspendLayout()
        CType(Me.m_pbLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_lblInfo
        '
        resources.ApplyResources(Me.m_lblInfo, "m_lblInfo")
        Me.m_lblInfo.Name = "m_lblInfo"
        '
        'm_pbProgress
        '
        resources.ApplyResources(Me.m_pbProgress, "m_pbProgress")
        Me.m_pbProgress.Name = "m_pbProgress"
        Me.m_pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        '
        'm_btnAbort
        '
        resources.ApplyResources(Me.m_btnAbort, "m_btnAbort")
        Me.m_btnAbort.Name = "m_btnAbort"
        Me.m_btnAbort.UseVisualStyleBackColor = True
        '
        'm_tlpButtons
        '
        resources.ApplyResources(Me.m_tlpButtons, "m_tlpButtons")
        Me.m_tlpButtons.Controls.Add(Me.m_btnAbort, 1, 0)
        Me.m_tlpButtons.Name = "m_tlpButtons"
        '
        'm_pbLogo
        '
        Me.m_pbLogo.BackgroundImage = Global.ScientificInterface.My.Resources.Resources.Ecopath_install
        resources.ApplyResources(Me.m_pbLogo, "m_pbLogo")
        Me.m_pbLogo.InitialImage = Global.ScientificInterface.My.Resources.Resources.Ecopath_install
        Me.m_pbLogo.Name = "m_pbLogo"
        Me.m_pbLogo.TabStop = False
        '
        'm_cbAutoUpdatePlugins
        '
        resources.ApplyResources(Me.m_cbAutoUpdatePlugins, "m_cbAutoUpdatePlugins")
        Me.m_cbAutoUpdatePlugins.Name = "m_cbAutoUpdatePlugins"
        Me.m_cbAutoUpdatePlugins.UseVisualStyleBackColor = True
        '
        'frmUpdateComponents
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.BackColor = System.Drawing.Color.White
        Me.ControlBox = False
        Me.Controls.Add(Me.m_cbAutoUpdatePlugins)
        Me.Controls.Add(Me.m_pbLogo)
        Me.Controls.Add(Me.m_tlpButtons)
        Me.Controls.Add(Me.m_pbProgress)
        Me.Controls.Add(Me.m_lblInfo)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Name = "frmUpdateComponents"
        Me.m_tlpButtons.ResumeLayout(False)
        CType(Me.m_pbLogo, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_pbProgress As System.Windows.Forms.ProgressBar
    Private WithEvents m_lblInfo As System.Windows.Forms.Label
    Private WithEvents m_btnAbort As System.Windows.Forms.Button
    Private WithEvents m_tlpButtons As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_pbLogo As System.Windows.Forms.PictureBox
    Private WithEvents m_cbAutoUpdatePlugins As System.Windows.Forms.CheckBox
End Class
