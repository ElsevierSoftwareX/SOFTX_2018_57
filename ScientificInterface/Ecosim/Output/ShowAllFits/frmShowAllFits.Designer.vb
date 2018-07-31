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

Imports ScientificInterfaceShared.Forms
Imports SharedResources = ScientificInterfaceShared.My.Resources

Namespace Ecosim

    Partial Class frmShowAllFits
        Inherits frmEwE

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmShowAllFits))
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_tlpControl = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plContent = New System.Windows.Forms.Panel()
            Me.m_hdrDisplayOptions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_chkShowB = New System.Windows.Forms.CheckBox()
            Me.m_chkShowZ = New System.Windows.Forms.CheckBox()
            Me.m_chkShowLandings = New System.Windows.Forms.CheckBox()
            Me.m_chkShowDiscards = New System.Windows.Forms.CheckBox()
            Me.m_chkShowCatch = New System.Windows.Forms.CheckBox()
            Me.m_plFormatting = New System.Windows.Forms.Panel()
            Me.m_hdrGeneral = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_chkShowSS = New System.Windows.Forms.CheckBox()
            Me.m_lblRowNum = New System.Windows.Forms.Label()
            Me.m_chkShowYear = New System.Windows.Forms.CheckBox()
            Me.m_lblDotSize = New System.Windows.Forms.Label()
            Me.m_cbShowGroupNo = New System.Windows.Forms.CheckBox()
            Me.m_chkShowWeight = New System.Windows.Forms.CheckBox()
            Me.m_lblLineWidth = New System.Windows.Forms.Label()
            Me.m_chkScaleForPrinter = New System.Windows.Forms.CheckBox()
            Me.m_nudMarginTB = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblMarginLR = New System.Windows.Forms.Label()
            Me.m_nudMarginLR = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblTBMargin = New System.Windows.Forms.Label()
            Me.m_nudDotSize = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudRowNum = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudLineWidth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_pbPlots = New System.Windows.Forms.PictureBox()
            Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsmiOptions = New System.Windows.Forms.ToolStripButton()
            Me.m_sep1 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsmiChoosePlots = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnScale = New System.Windows.Forms.ToolStripButton()
            Me.m_sep2 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsddSave = New System.Windows.Forms.ToolStripDropDownButton()
            Me.m_tsmiSaveAsImage = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiSaveAsCSV = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsddPrint = New System.Windows.Forms.ToolStripDropDownButton()
            Me.m_tsmiPrint = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_tsmiPrintPreview = New System.Windows.Forms.ToolStripMenuItem()
            Me.m_printdocAllFits = New System.Drawing.Printing.PrintDocument()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            Me.m_tlpControl.SuspendLayout()
            Me.m_plContent.SuspendLayout()
            Me.m_plFormatting.SuspendLayout()
            CType(Me.m_nudMarginTB, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudMarginLR, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudDotSize, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudRowNum, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudLineWidth, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_pbPlots, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tsMain.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_scMain
            '
            Me.m_scMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            resources.ApplyResources(Me.m_scMain, "m_scMain")
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            resources.ApplyResources(Me.m_scMain.Panel1, "m_scMain.Panel1")
            Me.m_scMain.Panel1.Controls.Add(Me.m_tlpControl)
            '
            'm_scMain.Panel2
            '
            resources.ApplyResources(Me.m_scMain.Panel2, "m_scMain.Panel2")
            Me.m_scMain.Panel2.Controls.Add(Me.m_pbPlots)
            Me.m_scMain.Panel2.Controls.Add(Me.m_tsMain)
            '
            'm_tlpControl
            '
            resources.ApplyResources(Me.m_tlpControl, "m_tlpControl")
            Me.m_tlpControl.Controls.Add(Me.m_plContent, 0, 0)
            Me.m_tlpControl.Controls.Add(Me.m_plFormatting, 0, 1)
            Me.m_tlpControl.Name = "m_tlpControl"
            '
            'm_plContent
            '
            Me.m_plContent.Controls.Add(Me.m_hdrDisplayOptions)
            Me.m_plContent.Controls.Add(Me.m_chkShowB)
            Me.m_plContent.Controls.Add(Me.m_chkShowZ)
            Me.m_plContent.Controls.Add(Me.m_chkShowLandings)
            Me.m_plContent.Controls.Add(Me.m_chkShowDiscards)
            Me.m_plContent.Controls.Add(Me.m_chkShowCatch)
            resources.ApplyResources(Me.m_plContent, "m_plContent")
            Me.m_plContent.Name = "m_plContent"
            '
            'm_hdrDisplayOptions
            '
            Me.m_hdrDisplayOptions.CanCollapseParent = False
            Me.m_hdrDisplayOptions.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrDisplayOptions, "m_hdrDisplayOptions")
            Me.m_hdrDisplayOptions.IsCollapsed = False
            Me.m_hdrDisplayOptions.Name = "m_hdrDisplayOptions"
            '
            'm_chkShowB
            '
            resources.ApplyResources(Me.m_chkShowB, "m_chkShowB")
            Me.m_chkShowB.Checked = True
            Me.m_chkShowB.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_chkShowB.Name = "m_chkShowB"
            Me.m_chkShowB.UseVisualStyleBackColor = True
            '
            'm_chkShowZ
            '
            resources.ApplyResources(Me.m_chkShowZ, "m_chkShowZ")
            Me.m_chkShowZ.Checked = True
            Me.m_chkShowZ.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_chkShowZ.Name = "m_chkShowZ"
            Me.m_chkShowZ.UseVisualStyleBackColor = True
            '
            'm_chkShowLandings
            '
            resources.ApplyResources(Me.m_chkShowLandings, "m_chkShowLandings")
            Me.m_chkShowLandings.Checked = True
            Me.m_chkShowLandings.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_chkShowLandings.Name = "m_chkShowLandings"
            Me.m_chkShowLandings.UseVisualStyleBackColor = True
            '
            'm_chkShowDiscards
            '
            resources.ApplyResources(Me.m_chkShowDiscards, "m_chkShowDiscards")
            Me.m_chkShowDiscards.Checked = True
            Me.m_chkShowDiscards.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_chkShowDiscards.Name = "m_chkShowDiscards"
            Me.m_chkShowDiscards.UseVisualStyleBackColor = True
            '
            'm_chkShowCatch
            '
            resources.ApplyResources(Me.m_chkShowCatch, "m_chkShowCatch")
            Me.m_chkShowCatch.Checked = True
            Me.m_chkShowCatch.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_chkShowCatch.Name = "m_chkShowCatch"
            Me.m_chkShowCatch.UseVisualStyleBackColor = True
            '
            'm_plFormatting
            '
            Me.m_plFormatting.Controls.Add(Me.m_hdrGeneral)
            Me.m_plFormatting.Controls.Add(Me.m_chkShowSS)
            Me.m_plFormatting.Controls.Add(Me.m_lblRowNum)
            Me.m_plFormatting.Controls.Add(Me.m_chkShowYear)
            Me.m_plFormatting.Controls.Add(Me.m_lblDotSize)
            Me.m_plFormatting.Controls.Add(Me.m_cbShowGroupNo)
            Me.m_plFormatting.Controls.Add(Me.m_chkShowWeight)
            Me.m_plFormatting.Controls.Add(Me.m_lblLineWidth)
            Me.m_plFormatting.Controls.Add(Me.m_chkScaleForPrinter)
            Me.m_plFormatting.Controls.Add(Me.m_nudMarginTB)
            Me.m_plFormatting.Controls.Add(Me.m_lblMarginLR)
            Me.m_plFormatting.Controls.Add(Me.m_nudMarginLR)
            Me.m_plFormatting.Controls.Add(Me.m_lblTBMargin)
            Me.m_plFormatting.Controls.Add(Me.m_nudDotSize)
            Me.m_plFormatting.Controls.Add(Me.m_nudRowNum)
            Me.m_plFormatting.Controls.Add(Me.m_nudLineWidth)
            resources.ApplyResources(Me.m_plFormatting, "m_plFormatting")
            Me.m_plFormatting.Name = "m_plFormatting"
            '
            'm_hdrGeneral
            '
            Me.m_hdrGeneral.CanCollapseParent = True
            Me.m_hdrGeneral.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrGeneral, "m_hdrGeneral")
            Me.m_hdrGeneral.IsCollapsed = False
            Me.m_hdrGeneral.Name = "m_hdrGeneral"
            '
            'm_chkShowSS
            '
            resources.ApplyResources(Me.m_chkShowSS, "m_chkShowSS")
            Me.m_chkShowSS.Checked = True
            Me.m_chkShowSS.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_chkShowSS.Name = "m_chkShowSS"
            Me.m_chkShowSS.UseVisualStyleBackColor = True
            '
            'm_lblRowNum
            '
            resources.ApplyResources(Me.m_lblRowNum, "m_lblRowNum")
            Me.m_lblRowNum.Name = "m_lblRowNum"
            '
            'm_chkShowYear
            '
            resources.ApplyResources(Me.m_chkShowYear, "m_chkShowYear")
            Me.m_chkShowYear.Checked = True
            Me.m_chkShowYear.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_chkShowYear.Name = "m_chkShowYear"
            Me.m_chkShowYear.UseVisualStyleBackColor = True
            '
            'm_lblDotSize
            '
            resources.ApplyResources(Me.m_lblDotSize, "m_lblDotSize")
            Me.m_lblDotSize.Name = "m_lblDotSize"
            '
            'm_cbShowGroupNo
            '
            resources.ApplyResources(Me.m_cbShowGroupNo, "m_cbShowGroupNo")
            Me.m_cbShowGroupNo.Name = "m_cbShowGroupNo"
            Me.m_cbShowGroupNo.UseVisualStyleBackColor = True
            '
            'm_chkShowWeight
            '
            resources.ApplyResources(Me.m_chkShowWeight, "m_chkShowWeight")
            Me.m_chkShowWeight.Checked = True
            Me.m_chkShowWeight.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_chkShowWeight.Name = "m_chkShowWeight"
            Me.m_chkShowWeight.UseVisualStyleBackColor = True
            '
            'm_lblLineWidth
            '
            resources.ApplyResources(Me.m_lblLineWidth, "m_lblLineWidth")
            Me.m_lblLineWidth.Name = "m_lblLineWidth"
            '
            'm_chkScaleForPrinter
            '
            resources.ApplyResources(Me.m_chkScaleForPrinter, "m_chkScaleForPrinter")
            Me.m_chkScaleForPrinter.Name = "m_chkScaleForPrinter"
            Me.m_chkScaleForPrinter.UseVisualStyleBackColor = True
            '
            'm_nudMarginTB
            '
            resources.ApplyResources(Me.m_nudMarginTB, "m_nudMarginTB")
            Me.m_nudMarginTB.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudMarginTB.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
            Me.m_nudMarginTB.Name = "m_nudMarginTB"
            '
            'm_lblMarginLR
            '
            resources.ApplyResources(Me.m_lblMarginLR, "m_lblMarginLR")
            Me.m_lblMarginLR.Name = "m_lblMarginLR"
            '
            'm_nudMarginLR
            '
            resources.ApplyResources(Me.m_nudMarginLR, "m_nudMarginLR")
            Me.m_nudMarginLR.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudMarginLR.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
            Me.m_nudMarginLR.Name = "m_nudMarginLR"
            '
            'm_lblTBMargin
            '
            resources.ApplyResources(Me.m_lblTBMargin, "m_lblTBMargin")
            Me.m_lblTBMargin.Name = "m_lblTBMargin"
            '
            'm_nudDotSize
            '
            resources.ApplyResources(Me.m_nudDotSize, "m_nudDotSize")
            Me.m_nudDotSize.DecimalPlaces = 2
            Me.m_nudDotSize.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudDotSize.Name = "m_nudDotSize"
            Me.m_nudDotSize.Value = New Decimal(New Integer() {3, 0, 0, 0})
            '
            'm_nudRowNum
            '
            resources.ApplyResources(Me.m_nudRowNum, "m_nudRowNum")
            Me.m_nudRowNum.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudRowNum.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
            Me.m_nudRowNum.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
            Me.m_nudRowNum.Name = "m_nudRowNum"
            Me.m_nudRowNum.Value = New Decimal(New Integer() {1, 0, 0, 0})
            '
            'm_nudLineWidth
            '
            resources.ApplyResources(Me.m_nudLineWidth, "m_nudLineWidth")
            Me.m_nudLineWidth.DecimalPlaces = 1
            Me.m_nudLineWidth.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
            Me.m_nudLineWidth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudLineWidth.Maximum = New Decimal(New Integer() {5, 0, 0, 0})
            Me.m_nudLineWidth.Minimum = New Decimal(New Integer() {1, 0, 0, 131072})
            Me.m_nudLineWidth.Name = "m_nudLineWidth"
            Me.m_nudLineWidth.Value = New Decimal(New Integer() {1, 0, 0, 131072})
            '
            'm_pbPlots
            '
            Me.m_pbPlots.BackColor = System.Drawing.Color.White
            resources.ApplyResources(Me.m_pbPlots, "m_pbPlots")
            Me.m_pbPlots.Name = "m_pbPlots"
            Me.m_pbPlots.TabStop = False
            '
            'm_tsMain
            '
            Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiOptions, Me.m_sep1, Me.m_tsmiChoosePlots, Me.m_tsbnScale, Me.m_sep2, Me.m_tsddSave, Me.m_tsddPrint})
            resources.ApplyResources(Me.m_tsMain, "m_tsMain")
            Me.m_tsMain.Name = "m_tsMain"
            Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsmiOptions
            '
            Me.m_tsmiOptions.Checked = True
            Me.m_tsmiOptions.CheckOnClick = True
            Me.m_tsmiOptions.CheckState = System.Windows.Forms.CheckState.Checked
            resources.ApplyResources(Me.m_tsmiOptions, "m_tsmiOptions")
            Me.m_tsmiOptions.Name = "m_tsmiOptions"
            '
            'm_sep1
            '
            Me.m_sep1.Name = "m_sep1"
            resources.ApplyResources(Me.m_sep1, "m_sep1")
            '
            'm_tsmiChoosePlots
            '
            resources.ApplyResources(Me.m_tsmiChoosePlots, "m_tsmiChoosePlots")
            Me.m_tsmiChoosePlots.Name = "m_tsmiChoosePlots"
            '
            'm_tsbnScale
            '
            Me.m_tsbnScale.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnScale, "m_tsbnScale")
            Me.m_tsbnScale.Name = "m_tsbnScale"
            '
            'm_sep2
            '
            Me.m_sep2.Name = "m_sep2"
            resources.ApplyResources(Me.m_sep2, "m_sep2")
            '
            'm_tsddSave
            '
            Me.m_tsddSave.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiSaveAsImage, Me.m_tsmiSaveAsCSV})
            resources.ApplyResources(Me.m_tsddSave, "m_tsddSave")
            Me.m_tsddSave.Name = "m_tsddSave"
            '
            'm_tsmiSaveAsImage
            '
            resources.ApplyResources(Me.m_tsmiSaveAsImage, "m_tsmiSaveAsImage")
            Me.m_tsmiSaveAsImage.Name = "m_tsmiSaveAsImage"
            '
            'm_tsmiSaveAsCSV
            '
            resources.ApplyResources(Me.m_tsmiSaveAsCSV, "m_tsmiSaveAsCSV")
            Me.m_tsmiSaveAsCSV.Name = "m_tsmiSaveAsCSV"
            '
            'm_tsddPrint
            '
            Me.m_tsddPrint.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsmiPrint, Me.m_tsmiPrintPreview})
            resources.ApplyResources(Me.m_tsddPrint, "m_tsddPrint")
            Me.m_tsddPrint.Name = "m_tsddPrint"
            '
            'm_tsmiPrint
            '
            resources.ApplyResources(Me.m_tsmiPrint, "m_tsmiPrint")
            Me.m_tsmiPrint.Name = "m_tsmiPrint"
            '
            'm_tsmiPrintPreview
            '
            resources.ApplyResources(Me.m_tsmiPrintPreview, "m_tsmiPrintPreview")
            Me.m_tsmiPrintPreview.Name = "m_tsmiPrintPreview"
            '
            'm_printdocAllFits
            '
            '
            'frmShowAllFits
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_scMain)
            Me.Name = "frmShowAllFits"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.TabText = ""
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel2.ResumeLayout(False)
            Me.m_scMain.Panel2.PerformLayout()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            Me.m_tlpControl.ResumeLayout(False)
            Me.m_plContent.ResumeLayout(False)
            Me.m_plContent.PerformLayout()
            Me.m_plFormatting.ResumeLayout(False)
            Me.m_plFormatting.PerformLayout()
            CType(Me.m_nudMarginTB, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudMarginLR, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudDotSize, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudRowNum, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudLineWidth, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_pbPlots, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tsMain.ResumeLayout(False)
            Me.m_tsMain.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_lblTBMargin As System.Windows.Forms.Label
        Private WithEvents m_lblMarginLR As System.Windows.Forms.Label
        Private WithEvents m_lblDotSize As System.Windows.Forms.Label
        Private WithEvents m_lblLineWidth As System.Windows.Forms.Label
        Private WithEvents m_sep2 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_printdocAllFits As System.Drawing.Printing.PrintDocument
        Private WithEvents m_hdrDisplayOptions As cEwEHeaderLabel
        Private WithEvents m_hdrGeneral As cEwEHeaderLabel
        Private WithEvents m_tsMain As cEwEToolstrip
        Private WithEvents m_tsmiChoosePlots As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnScale As System.Windows.Forms.ToolStripButton
        Private WithEvents m_lblRowNum As System.Windows.Forms.Label
        Private WithEvents m_chkScaleForPrinter As System.Windows.Forms.CheckBox
        Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
        Private WithEvents m_tsddSave As System.Windows.Forms.ToolStripDropDownButton
        Private WithEvents m_tsmiSaveAsImage As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiSaveAsCSV As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsddPrint As System.Windows.Forms.ToolStripDropDownButton
        Private WithEvents m_tsmiPrint As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiPrintPreview As System.Windows.Forms.ToolStripMenuItem
        Private WithEvents m_tsmiOptions As System.Windows.Forms.ToolStripButton
        Private WithEvents m_sep1 As System.Windows.Forms.ToolStripSeparator
        Private WithEvents m_chkShowYear As System.Windows.Forms.CheckBox
        Private WithEvents m_chkShowWeight As System.Windows.Forms.CheckBox
        Private WithEvents m_chkShowCatch As System.Windows.Forms.CheckBox
        Private WithEvents m_chkShowZ As System.Windows.Forms.CheckBox
        Private WithEvents m_chkShowB As System.Windows.Forms.CheckBox
        Private WithEvents m_pbPlots As System.Windows.Forms.PictureBox
        Private WithEvents m_chkShowSS As System.Windows.Forms.CheckBox
        Private WithEvents m_nudRowNum As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudLineWidth As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudDotSize As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudMarginLR As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudMarginTB As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_cbShowGroupNo As System.Windows.Forms.CheckBox
        Private WithEvents m_tlpControl As TableLayoutPanel
        Private WithEvents m_plContent As Panel
        Private WithEvents m_chkShowLandings As CheckBox
        Private WithEvents m_chkShowDiscards As CheckBox
        Private WithEvents m_plFormatting As Panel
    End Class

End Namespace

