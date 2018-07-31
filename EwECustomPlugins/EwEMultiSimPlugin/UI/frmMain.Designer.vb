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
Partial Class frmMain
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.m_scMain = New System.Windows.Forms.SplitContainer()
        Me.m_cbEggProduction = New System.Windows.Forms.CheckBox()
        Me.m_cbMort = New System.Windows.Forms.CheckBox()
        Me.m_cbEffort = New System.Windows.Forms.CheckBox()
        Me.m_cbFF = New System.Windows.Forms.CheckBox()
        Me.m_lblFilesSrc = New System.Windows.Forms.Label()
        Me.m_clbFilesSrc = New System.Windows.Forms.CheckedListBox()
        Me.m_hdrIn = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lblSource = New System.Windows.Forms.Label()
        Me.m_btnGenerateSample = New System.Windows.Forms.Button()
        Me.m_btnValidate = New System.Windows.Forms.Button()
        Me.m_btnAllSrc = New System.Windows.Forms.Button()
        Me.m_rbAnnual = New System.Windows.Forms.RadioButton()
        Me.m_tbxSource = New System.Windows.Forms.TextBox()
        Me.m_rbMonthly = New System.Windows.Forms.RadioButton()
        Me.m_btnChooseSrc = New System.Windows.Forms.Button()
        Me.m_lblApply = New System.Windows.Forms.Label()
        Me.m_lblReadAs = New System.Windows.Forms.Label()
        Me.m_cbCreateRunFolder = New System.Windows.Forms.CheckBox()
        Me.m_hrdOut = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tbxDest = New System.Windows.Forms.TextBox()
        Me.m_btnRun = New System.Windows.Forms.Button()
        Me.m_clbValues = New System.Windows.Forms.CheckedListBox()
        Me.m_lblDest = New System.Windows.Forms.Label()
        Me.m_btnAllVars = New System.Windows.Forms.Button()
        Me.m_btnChooseOut = New System.Windows.Forms.Button()
        Me.m_lblVars = New System.Windows.Forms.Label()
        Me.m_pbLogoDFO = New System.Windows.Forms.PictureBox()
        Me.m_tlpCredits = New System.Windows.Forms.TableLayoutPanel()
        Me.m_pbLogoSU = New System.Windows.Forms.PictureBox()
        Me.m_tlpMain = New System.Windows.Forms.TableLayoutPanel()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scMain.Panel1.SuspendLayout()
        Me.m_scMain.Panel2.SuspendLayout()
        Me.m_scMain.SuspendLayout()
        CType(Me.m_pbLogoDFO, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tlpCredits.SuspendLayout()
        CType(Me.m_pbLogoSU, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tlpMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_scMain
        '
        resources.ApplyResources(Me.m_scMain, "m_scMain")
        Me.m_scMain.Name = "m_scMain"
        '
        'm_scMain.Panel1
        '
        Me.m_scMain.Panel1.Controls.Add(Me.m_cbEggProduction)
        Me.m_scMain.Panel1.Controls.Add(Me.m_cbMort)
        Me.m_scMain.Panel1.Controls.Add(Me.m_cbEffort)
        Me.m_scMain.Panel1.Controls.Add(Me.m_cbFF)
        Me.m_scMain.Panel1.Controls.Add(Me.m_lblFilesSrc)
        Me.m_scMain.Panel1.Controls.Add(Me.m_clbFilesSrc)
        Me.m_scMain.Panel1.Controls.Add(Me.m_hdrIn)
        Me.m_scMain.Panel1.Controls.Add(Me.m_lblSource)
        Me.m_scMain.Panel1.Controls.Add(Me.m_btnGenerateSample)
        Me.m_scMain.Panel1.Controls.Add(Me.m_btnValidate)
        Me.m_scMain.Panel1.Controls.Add(Me.m_btnAllSrc)
        Me.m_scMain.Panel1.Controls.Add(Me.m_rbAnnual)
        Me.m_scMain.Panel1.Controls.Add(Me.m_tbxSource)
        Me.m_scMain.Panel1.Controls.Add(Me.m_rbMonthly)
        Me.m_scMain.Panel1.Controls.Add(Me.m_btnChooseSrc)
        Me.m_scMain.Panel1.Controls.Add(Me.m_lblApply)
        Me.m_scMain.Panel1.Controls.Add(Me.m_lblReadAs)
        '
        'm_scMain.Panel2
        '
        Me.m_scMain.Panel2.Controls.Add(Me.m_cbCreateRunFolder)
        Me.m_scMain.Panel2.Controls.Add(Me.m_hrdOut)
        Me.m_scMain.Panel2.Controls.Add(Me.m_tbxDest)
        Me.m_scMain.Panel2.Controls.Add(Me.m_btnRun)
        Me.m_scMain.Panel2.Controls.Add(Me.m_clbValues)
        Me.m_scMain.Panel2.Controls.Add(Me.m_lblDest)
        Me.m_scMain.Panel2.Controls.Add(Me.m_btnAllVars)
        Me.m_scMain.Panel2.Controls.Add(Me.m_btnChooseOut)
        Me.m_scMain.Panel2.Controls.Add(Me.m_lblVars)
        '
        'm_cbEggProduction
        '
        resources.ApplyResources(Me.m_cbEggProduction, "m_cbEggProduction")
        Me.m_cbEggProduction.Name = "m_cbEggProduction"
        Me.m_cbEggProduction.UseVisualStyleBackColor = True
        '
        'm_cbMort
        '
        resources.ApplyResources(Me.m_cbMort, "m_cbMort")
        Me.m_cbMort.Name = "m_cbMort"
        Me.m_cbMort.UseVisualStyleBackColor = True
        '
        'm_cbEffort
        '
        resources.ApplyResources(Me.m_cbEffort, "m_cbEffort")
        Me.m_cbEffort.Name = "m_cbEffort"
        Me.m_cbEffort.UseVisualStyleBackColor = True
        '
        'm_cbFF
        '
        resources.ApplyResources(Me.m_cbFF, "m_cbFF")
        Me.m_cbFF.Checked = True
        Me.m_cbFF.CheckState = System.Windows.Forms.CheckState.Checked
        Me.m_cbFF.Name = "m_cbFF"
        Me.m_cbFF.UseVisualStyleBackColor = True
        '
        'm_lblFilesSrc
        '
        resources.ApplyResources(Me.m_lblFilesSrc, "m_lblFilesSrc")
        Me.m_lblFilesSrc.Name = "m_lblFilesSrc"
        '
        'm_clbFilesSrc
        '
        resources.ApplyResources(Me.m_clbFilesSrc, "m_clbFilesSrc")
        Me.m_clbFilesSrc.CheckOnClick = True
        Me.m_clbFilesSrc.FormattingEnabled = True
        Me.m_clbFilesSrc.Name = "m_clbFilesSrc"
        '
        'm_hdrIn
        '
        resources.ApplyResources(Me.m_hdrIn, "m_hdrIn")
        Me.m_hdrIn.CanCollapseParent = False
        Me.m_hdrIn.CollapsedParentHeight = 0
        Me.m_hdrIn.IsCollapsed = False
        Me.m_hdrIn.Name = "m_hdrIn"
        '
        'm_lblSource
        '
        resources.ApplyResources(Me.m_lblSource, "m_lblSource")
        Me.m_lblSource.Name = "m_lblSource"
        '
        'm_btnGenerateSample
        '
        resources.ApplyResources(Me.m_btnGenerateSample, "m_btnGenerateSample")
        Me.m_btnGenerateSample.Name = "m_btnGenerateSample"
        Me.m_btnGenerateSample.UseVisualStyleBackColor = True
        '
        'm_btnValidate
        '
        resources.ApplyResources(Me.m_btnValidate, "m_btnValidate")
        Me.m_btnValidate.Name = "m_btnValidate"
        Me.m_btnValidate.UseVisualStyleBackColor = True
        '
        'm_btnAllSrc
        '
        resources.ApplyResources(Me.m_btnAllSrc, "m_btnAllSrc")
        Me.m_btnAllSrc.Name = "m_btnAllSrc"
        Me.m_btnAllSrc.UseVisualStyleBackColor = True
        '
        'm_rbAnnual
        '
        resources.ApplyResources(Me.m_rbAnnual, "m_rbAnnual")
        Me.m_rbAnnual.Name = "m_rbAnnual"
        Me.m_rbAnnual.TabStop = True
        Me.m_rbAnnual.UseVisualStyleBackColor = True
        '
        'm_tbxSource
        '
        resources.ApplyResources(Me.m_tbxSource, "m_tbxSource")
        Me.m_tbxSource.Name = "m_tbxSource"
        Me.m_tbxSource.ReadOnly = True
        '
        'm_rbMonthly
        '
        resources.ApplyResources(Me.m_rbMonthly, "m_rbMonthly")
        Me.m_rbMonthly.Name = "m_rbMonthly"
        Me.m_rbMonthly.TabStop = True
        Me.m_rbMonthly.UseVisualStyleBackColor = True
        '
        'm_btnChooseSrc
        '
        resources.ApplyResources(Me.m_btnChooseSrc, "m_btnChooseSrc")
        Me.m_btnChooseSrc.Name = "m_btnChooseSrc"
        Me.m_btnChooseSrc.UseVisualStyleBackColor = True
        '
        'm_lblApply
        '
        resources.ApplyResources(Me.m_lblApply, "m_lblApply")
        Me.m_lblApply.Name = "m_lblApply"
        '
        'm_lblReadAs
        '
        resources.ApplyResources(Me.m_lblReadAs, "m_lblReadAs")
        Me.m_lblReadAs.Name = "m_lblReadAs"
        '
        'm_cbCreateRunFolder
        '
        resources.ApplyResources(Me.m_cbCreateRunFolder, "m_cbCreateRunFolder")
        Me.m_cbCreateRunFolder.Name = "m_cbCreateRunFolder"
        Me.m_cbCreateRunFolder.UseVisualStyleBackColor = True
        '
        'm_hrdOut
        '
        resources.ApplyResources(Me.m_hrdOut, "m_hrdOut")
        Me.m_hrdOut.CanCollapseParent = False
        Me.m_hrdOut.CollapsedParentHeight = 0
        Me.m_hrdOut.IsCollapsed = False
        Me.m_hrdOut.Name = "m_hrdOut"
        '
        'm_tbxDest
        '
        resources.ApplyResources(Me.m_tbxDest, "m_tbxDest")
        Me.m_tbxDest.Name = "m_tbxDest"
        Me.m_tbxDest.ReadOnly = True
        '
        'm_btnRun
        '
        resources.ApplyResources(Me.m_btnRun, "m_btnRun")
        Me.m_btnRun.Name = "m_btnRun"
        Me.m_btnRun.UseVisualStyleBackColor = True
        '
        'm_clbValues
        '
        resources.ApplyResources(Me.m_clbValues, "m_clbValues")
        Me.m_clbValues.CheckOnClick = True
        Me.m_clbValues.FormattingEnabled = True
        Me.m_clbValues.MultiColumn = True
        Me.m_clbValues.Name = "m_clbValues"
        Me.m_clbValues.Sorted = True
        '
        'm_lblDest
        '
        resources.ApplyResources(Me.m_lblDest, "m_lblDest")
        Me.m_lblDest.Name = "m_lblDest"
        '
        'm_btnAllVars
        '
        resources.ApplyResources(Me.m_btnAllVars, "m_btnAllVars")
        Me.m_btnAllVars.Name = "m_btnAllVars"
        Me.m_btnAllVars.UseVisualStyleBackColor = True
        '
        'm_btnChooseOut
        '
        resources.ApplyResources(Me.m_btnChooseOut, "m_btnChooseOut")
        Me.m_btnChooseOut.Name = "m_btnChooseOut"
        Me.m_btnChooseOut.UseVisualStyleBackColor = True
        '
        'm_lblVars
        '
        resources.ApplyResources(Me.m_lblVars, "m_lblVars")
        Me.m_lblVars.Name = "m_lblVars"
        '
        'm_pbLogoDFO
        '
        Me.m_pbLogoDFO.BackColor = System.Drawing.Color.White
        Me.m_pbLogoDFO.BackgroundImage = Global.EwEMultiSimPlugin.My.Resources.Resources.logo_canada_large
        resources.ApplyResources(Me.m_pbLogoDFO, "m_pbLogoDFO")
        Me.m_pbLogoDFO.Name = "m_pbLogoDFO"
        Me.m_pbLogoDFO.TabStop = False
        '
        'm_tlpCredits
        '
        Me.m_tlpCredits.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.m_tlpCredits, "m_tlpCredits")
        Me.m_tlpCredits.Controls.Add(Me.m_pbLogoSU, 3, 0)
        Me.m_tlpCredits.Controls.Add(Me.m_pbLogoDFO, 1, 0)
        Me.m_tlpCredits.Name = "m_tlpCredits"
        '
        'm_pbLogoSU
        '
        Me.m_pbLogoSU.BackColor = System.Drawing.Color.White
        Me.m_pbLogoSU.BackgroundImage = Global.EwEMultiSimPlugin.My.Resources.Resources.ÖCengsvart_1
        resources.ApplyResources(Me.m_pbLogoSU, "m_pbLogoSU")
        Me.m_pbLogoSU.Name = "m_pbLogoSU"
        Me.m_pbLogoSU.TabStop = False
        '
        'm_tlpMain
        '
        resources.ApplyResources(Me.m_tlpMain, "m_tlpMain")
        Me.m_tlpMain.Controls.Add(Me.m_tlpCredits, 0, 1)
        Me.m_tlpMain.Controls.Add(Me.m_scMain, 0, 0)
        Me.m_tlpMain.Name = "m_tlpMain"
        '
        'frmMain
        '
        Me.AcceptButton = Me.m_btnRun
        Me.AllowDrop = True
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_tlpMain)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "frmMain"
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.m_scMain.Panel1.ResumeLayout(False)
        Me.m_scMain.Panel1.PerformLayout()
        Me.m_scMain.Panel2.ResumeLayout(False)
        Me.m_scMain.Panel2.PerformLayout()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scMain.ResumeLayout(False)
        CType(Me.m_pbLogoDFO, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tlpCredits.ResumeLayout(False)
        CType(Me.m_pbLogoSU, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tlpMain.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_clbFilesSrc As System.Windows.Forms.CheckedListBox
    Private WithEvents m_tbxDest As System.Windows.Forms.TextBox
    Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
    Friend WithEvents m_lblFilesSrc As System.Windows.Forms.Label
    Private WithEvents m_hdrIn As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_lblSource As System.Windows.Forms.Label
    Private WithEvents m_btnAllSrc As System.Windows.Forms.Button
    Private WithEvents m_rbAnnual As System.Windows.Forms.RadioButton
    Private WithEvents m_tbxSource As System.Windows.Forms.TextBox
    Private WithEvents m_rbMonthly As System.Windows.Forms.RadioButton
    Private WithEvents m_btnChooseSrc As System.Windows.Forms.Button
    Private WithEvents m_lblReadAs As System.Windows.Forms.Label
    Private WithEvents m_hrdOut As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_clbValues As System.Windows.Forms.CheckedListBox
    Private WithEvents m_lblDest As System.Windows.Forms.Label
    Private WithEvents m_btnAllVars As System.Windows.Forms.Button
    Private WithEvents m_btnChooseOut As System.Windows.Forms.Button
    Private WithEvents m_lblVars As System.Windows.Forms.Label
    Private WithEvents m_pbLogoDFO As System.Windows.Forms.PictureBox
    Private WithEvents m_btnRun As System.Windows.Forms.Button
    Private WithEvents m_cbCreateRunFolder As System.Windows.Forms.CheckBox
    Private WithEvents m_btnValidate As System.Windows.Forms.Button
    Private WithEvents m_cbMort As System.Windows.Forms.CheckBox
    Private WithEvents m_cbEffort As System.Windows.Forms.CheckBox
    Private WithEvents m_cbFF As System.Windows.Forms.CheckBox
    Private WithEvents m_lblApply As System.Windows.Forms.Label
    Private WithEvents m_cbEggProduction As System.Windows.Forms.CheckBox
    Private WithEvents m_btnGenerateSample As System.Windows.Forms.Button
    Private WithEvents m_tlpCredits As TableLayoutPanel
    Friend WithEvents m_tlpMain As TableLayoutPanel
    Private WithEvents m_pbLogoSU As PictureBox
End Class
