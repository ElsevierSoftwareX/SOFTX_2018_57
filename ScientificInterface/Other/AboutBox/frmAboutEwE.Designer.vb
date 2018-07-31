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

Namespace Other

    Partial Public Class frmAboutEwE
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
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
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAboutEwE))
            Me.m_btnOK = New System.Windows.Forms.Button()
            Me.m_tlpGeneral = New System.Windows.Forms.TableLayoutPanel()
            Me.m_pbFish0 = New System.Windows.Forms.PictureBox()
            Me.m_tlpDetails = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lbCopyright = New System.Windows.Forms.TextBox()
            Me.m_rtbDisclaimer = New System.Windows.Forms.RichTextBox()
            Me.m_rtbDistribution = New System.Windows.Forms.RichTextBox()
            Me.m_lbVersion = New System.Windows.Forms.TextBox()
            Me.m_lbLicense = New System.Windows.Forms.TextBox()
            Me.m_lbTitle = New System.Windows.Forms.TextBox()
            Me.m_tcMain = New System.Windows.Forms.TabControl()
            Me.m_tpGeneral = New System.Windows.Forms.TabPage()
            Me.m_tpLicense = New System.Windows.Forms.TabPage()
            Me.m_rtbLicense = New System.Windows.Forms.RichTextBox()
            Me.m_tpTeam = New System.Windows.Forms.TabPage()
            Me.m_rtbTeam = New System.Windows.Forms.RichTextBox()
            Me.m_tpAcknowledgements = New System.Windows.Forms.TabPage()
            Me.m_rtbAcknowledgements = New System.Windows.Forms.RichTextBox()
            Me.m_tpTechnical = New System.Windows.Forms.TabPage()
            Me.m_tlpTechnicalDetails = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lblNetVersion = New System.Windows.Forms.Label()
            Me.m_gridTechnical = New ScientificInterface.gridAboutEwE()
            Me.m_tsTechnical = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnShowEwEAssembliesOnly = New System.Windows.Forms.ToolStripButton()
            Me.ToolStripLabel1 = New System.Windows.Forms.ToolStripLabel()
            Me.m_tpDatabase = New System.Windows.Forms.TabPage()
            Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lblDatabase = New System.Windows.Forms.Label()
            Me.m_gridDatabase = New ScientificInterface.gridDatabase()
            Me.m_tlpGeneral.SuspendLayout()
            CType(Me.m_pbFish0, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_tlpDetails.SuspendLayout()
            Me.m_tcMain.SuspendLayout()
            Me.m_tpGeneral.SuspendLayout()
            Me.m_tpLicense.SuspendLayout()
            Me.m_tpTeam.SuspendLayout()
            Me.m_tpAcknowledgements.SuspendLayout()
            Me.m_tpTechnical.SuspendLayout()
            Me.m_tlpTechnicalDetails.SuspendLayout()
            Me.m_tsTechnical.SuspendLayout()
            Me.m_tpDatabase.SuspendLayout()
            Me.TableLayoutPanel2.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_btnOK
            '
            resources.ApplyResources(Me.m_btnOK, "m_btnOK")
            Me.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnOK.Name = "m_btnOK"
            '
            'm_tlpGeneral
            '
            resources.ApplyResources(Me.m_tlpGeneral, "m_tlpGeneral")
            Me.m_tlpGeneral.Controls.Add(Me.m_pbFish0, 0, 0)
            Me.m_tlpGeneral.Controls.Add(Me.m_tlpDetails, 1, 0)
            Me.m_tlpGeneral.Name = "m_tlpGeneral"
            '
            'm_pbFish0
            '
            Me.m_pbFish0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_pbFish0.Image = Global.ScientificInterface.My.Resources.Resources.logo_EWE5_caption
            resources.ApplyResources(Me.m_pbFish0, "m_pbFish0")
            Me.m_pbFish0.Name = "m_pbFish0"
            Me.m_pbFish0.TabStop = False
            '
            'm_tlpDetails
            '
            resources.ApplyResources(Me.m_tlpDetails, "m_tlpDetails")
            Me.m_tlpDetails.Controls.Add(Me.m_lbCopyright, 0, 3)
            Me.m_tlpDetails.Controls.Add(Me.m_rtbDisclaimer, 0, 5)
            Me.m_tlpDetails.Controls.Add(Me.m_rtbDistribution, 0, 7)
            Me.m_tlpDetails.Controls.Add(Me.m_lbVersion, 0, 1)
            Me.m_tlpDetails.Controls.Add(Me.m_lbLicense, 0, 2)
            Me.m_tlpDetails.Controls.Add(Me.m_lbTitle, 0, 0)
            Me.m_tlpDetails.Name = "m_tlpDetails"
            '
            'm_lbCopyright
            '
            Me.m_lbCopyright.BorderStyle = System.Windows.Forms.BorderStyle.None
            resources.ApplyResources(Me.m_lbCopyright, "m_lbCopyright")
            Me.m_lbCopyright.Name = "m_lbCopyright"
            Me.m_lbCopyright.ReadOnly = True
            '
            'm_rtbDisclaimer
            '
            Me.m_rtbDisclaimer.BackColor = System.Drawing.SystemColors.Control
            Me.m_rtbDisclaimer.BorderStyle = System.Windows.Forms.BorderStyle.None
            resources.ApplyResources(Me.m_rtbDisclaimer, "m_rtbDisclaimer")
            Me.m_rtbDisclaimer.Name = "m_rtbDisclaimer"
            '
            'm_rtbDistribution
            '
            Me.m_rtbDistribution.BackColor = System.Drawing.SystemColors.Control
            Me.m_rtbDistribution.BorderStyle = System.Windows.Forms.BorderStyle.None
            resources.ApplyResources(Me.m_rtbDistribution, "m_rtbDistribution")
            Me.m_rtbDistribution.Name = "m_rtbDistribution"
            '
            'm_lbVersion
            '
            Me.m_lbVersion.BorderStyle = System.Windows.Forms.BorderStyle.None
            resources.ApplyResources(Me.m_lbVersion, "m_lbVersion")
            Me.m_lbVersion.Name = "m_lbVersion"
            Me.m_lbVersion.ReadOnly = True
            '
            'm_lbLicense
            '
            Me.m_lbLicense.BorderStyle = System.Windows.Forms.BorderStyle.None
            resources.ApplyResources(Me.m_lbLicense, "m_lbLicense")
            Me.m_lbLicense.Name = "m_lbLicense"
            Me.m_lbLicense.ReadOnly = True
            '
            'm_lbTitle
            '
            Me.m_lbTitle.BorderStyle = System.Windows.Forms.BorderStyle.None
            resources.ApplyResources(Me.m_lbTitle, "m_lbTitle")
            Me.m_lbTitle.Name = "m_lbTitle"
            Me.m_lbTitle.ReadOnly = True
            '
            'm_tcMain
            '
            resources.ApplyResources(Me.m_tcMain, "m_tcMain")
            Me.m_tcMain.Controls.Add(Me.m_tpGeneral)
            Me.m_tcMain.Controls.Add(Me.m_tpLicense)
            Me.m_tcMain.Controls.Add(Me.m_tpTeam)
            Me.m_tcMain.Controls.Add(Me.m_tpAcknowledgements)
            Me.m_tcMain.Controls.Add(Me.m_tpTechnical)
            Me.m_tcMain.Controls.Add(Me.m_tpDatabase)
            Me.m_tcMain.Name = "m_tcMain"
            Me.m_tcMain.SelectedIndex = 0
            '
            'm_tpGeneral
            '
            Me.m_tpGeneral.Controls.Add(Me.m_tlpGeneral)
            resources.ApplyResources(Me.m_tpGeneral, "m_tpGeneral")
            Me.m_tpGeneral.Name = "m_tpGeneral"
            Me.m_tpGeneral.UseVisualStyleBackColor = True
            '
            'm_tpLicense
            '
            Me.m_tpLicense.Controls.Add(Me.m_rtbLicense)
            resources.ApplyResources(Me.m_tpLicense, "m_tpLicense")
            Me.m_tpLicense.Name = "m_tpLicense"
            Me.m_tpLicense.UseVisualStyleBackColor = True
            '
            'm_rtbLicense
            '
            Me.m_rtbLicense.BackColor = System.Drawing.SystemColors.Control
            Me.m_rtbLicense.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.m_rtbLicense.Cursor = System.Windows.Forms.Cursors.Default
            resources.ApplyResources(Me.m_rtbLicense, "m_rtbLicense")
            Me.m_rtbLicense.Name = "m_rtbLicense"
            Me.m_rtbLicense.ShortcutsEnabled = False
            '
            'm_tpTeam
            '
            Me.m_tpTeam.Controls.Add(Me.m_rtbTeam)
            resources.ApplyResources(Me.m_tpTeam, "m_tpTeam")
            Me.m_tpTeam.Name = "m_tpTeam"
            Me.m_tpTeam.UseVisualStyleBackColor = True
            '
            'm_rtbTeam
            '
            Me.m_rtbTeam.BackColor = System.Drawing.SystemColors.Control
            Me.m_rtbTeam.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.m_rtbTeam.Cursor = System.Windows.Forms.Cursors.Default
            resources.ApplyResources(Me.m_rtbTeam, "m_rtbTeam")
            Me.m_rtbTeam.Name = "m_rtbTeam"
            '
            'm_tpAcknowledgements
            '
            Me.m_tpAcknowledgements.Controls.Add(Me.m_rtbAcknowledgements)
            resources.ApplyResources(Me.m_tpAcknowledgements, "m_tpAcknowledgements")
            Me.m_tpAcknowledgements.Name = "m_tpAcknowledgements"
            Me.m_tpAcknowledgements.UseVisualStyleBackColor = True
            '
            'm_rtbAcknowledgements
            '
            Me.m_rtbAcknowledgements.BackColor = System.Drawing.SystemColors.Control
            Me.m_rtbAcknowledgements.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.m_rtbAcknowledgements.Cursor = System.Windows.Forms.Cursors.Default
            resources.ApplyResources(Me.m_rtbAcknowledgements, "m_rtbAcknowledgements")
            Me.m_rtbAcknowledgements.HideSelection = False
            Me.m_rtbAcknowledgements.Name = "m_rtbAcknowledgements"
            '
            'm_tpTechnical
            '
            Me.m_tpTechnical.Controls.Add(Me.m_tlpTechnicalDetails)
            resources.ApplyResources(Me.m_tpTechnical, "m_tpTechnical")
            Me.m_tpTechnical.Name = "m_tpTechnical"
            Me.m_tpTechnical.UseVisualStyleBackColor = True
            '
            'm_tlpTechnicalDetails
            '
            resources.ApplyResources(Me.m_tlpTechnicalDetails, "m_tlpTechnicalDetails")
            Me.m_tlpTechnicalDetails.Controls.Add(Me.m_lblNetVersion, 0, 2)
            Me.m_tlpTechnicalDetails.Controls.Add(Me.m_gridTechnical, 0, 1)
            Me.m_tlpTechnicalDetails.Controls.Add(Me.m_tsTechnical, 0, 0)
            Me.m_tlpTechnicalDetails.Name = "m_tlpTechnicalDetails"
            '
            'm_lblNetVersion
            '
            resources.ApplyResources(Me.m_lblNetVersion, "m_lblNetVersion")
            Me.m_lblNetVersion.Name = "m_lblNetVersion"
            '
            'm_gridTechnical
            '
            Me.m_gridTechnical.AllowBlockSelect = False
            resources.ApplyResources(Me.m_gridTechnical, "m_gridTechnical")
            Me.m_gridTechnical.AutoSizeMinHeight = 10
            Me.m_gridTechnical.AutoSizeMinWidth = 10
            Me.m_gridTechnical.AutoStretchColumnsToFitWidth = False
            Me.m_gridTechnical.AutoStretchRowsToFitHeight = False
            Me.m_gridTechnical.BackColor = System.Drawing.Color.White
            Me.m_gridTechnical.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridTechnical.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridTechnical.CustomSort = False
            Me.m_gridTechnical.DataName = "EwE components"
            Me.m_gridTechnical.FixedColumnWidths = False
            Me.m_gridTechnical.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridTechnical.GridToolTipActive = True
            Me.m_gridTechnical.IsLayoutSuspended = False
            Me.m_gridTechnical.IsOutputGrid = True
            Me.m_gridTechnical.Name = "m_gridTechnical"
            Me.m_gridTechnical.ShowEwEComponentsOnly = True
            Me.m_gridTechnical.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridTechnical.UIContext = Nothing
            '
            'm_tsTechnical
            '
            Me.m_tsTechnical.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsTechnical.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnShowEwEAssembliesOnly, Me.ToolStripLabel1})
            resources.ApplyResources(Me.m_tsTechnical, "m_tsTechnical")
            Me.m_tsTechnical.Name = "m_tsTechnical"
            Me.m_tsTechnical.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnShowEwEAssembliesOnly
            '
            Me.m_tsbnShowEwEAssembliesOnly.CheckOnClick = True
            Me.m_tsbnShowEwEAssembliesOnly.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
            resources.ApplyResources(Me.m_tsbnShowEwEAssembliesOnly, "m_tsbnShowEwEAssembliesOnly")
            Me.m_tsbnShowEwEAssembliesOnly.Name = "m_tsbnShowEwEAssembliesOnly"
            '
            'ToolStripLabel1
            '
            Me.ToolStripLabel1.Name = "ToolStripLabel1"
            resources.ApplyResources(Me.ToolStripLabel1, "ToolStripLabel1")
            '
            'm_tpDatabase
            '
            Me.m_tpDatabase.Controls.Add(Me.TableLayoutPanel2)
            resources.ApplyResources(Me.m_tpDatabase, "m_tpDatabase")
            Me.m_tpDatabase.Name = "m_tpDatabase"
            Me.m_tpDatabase.UseVisualStyleBackColor = True
            '
            'TableLayoutPanel2
            '
            resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
            Me.TableLayoutPanel2.Controls.Add(Me.m_lblDatabase, 0, 0)
            Me.TableLayoutPanel2.Controls.Add(Me.m_gridDatabase, 0, 1)
            Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
            '
            'm_lblDatabase
            '
            resources.ApplyResources(Me.m_lblDatabase, "m_lblDatabase")
            Me.m_lblDatabase.Name = "m_lblDatabase"
            '
            'm_gridDatabase
            '
            Me.m_gridDatabase.AllowBlockSelect = False
            resources.ApplyResources(Me.m_gridDatabase, "m_gridDatabase")
            Me.m_gridDatabase.AutoSizeMinHeight = 10
            Me.m_gridDatabase.AutoSizeMinWidth = 10
            Me.m_gridDatabase.AutoStretchColumnsToFitWidth = False
            Me.m_gridDatabase.AutoStretchRowsToFitHeight = False
            Me.m_gridDatabase.BackColor = System.Drawing.Color.White
            Me.m_gridDatabase.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_gridDatabase.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_gridDatabase.CustomSort = False
            Me.m_gridDatabase.DataName = "grid content"
            Me.m_gridDatabase.FixedColumnWidths = False
            Me.m_gridDatabase.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_gridDatabase.GridToolTipActive = True
            Me.m_gridDatabase.IsLayoutSuspended = False
            Me.m_gridDatabase.IsOutputGrid = True
            Me.m_gridDatabase.Name = "m_gridDatabase"
            Me.m_gridDatabase.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_gridDatabase.UIContext = Nothing
            '
            'frmAboutEwE
            '
            Me.AcceptButton = Me.m_btnOK
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnOK
            Me.ControlBox = False
            Me.Controls.Add(Me.m_tcMain)
            Me.Controls.Add(Me.m_btnOK)
            Me.DoubleBuffered = True
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "frmAboutEwE"
            Me.ShowInTaskbar = False
            Me.m_tlpGeneral.ResumeLayout(False)
            Me.m_tlpGeneral.PerformLayout()
            CType(Me.m_pbFish0, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_tlpDetails.ResumeLayout(False)
            Me.m_tlpDetails.PerformLayout()
            Me.m_tcMain.ResumeLayout(False)
            Me.m_tpGeneral.ResumeLayout(False)
            Me.m_tpLicense.ResumeLayout(False)
            Me.m_tpTeam.ResumeLayout(False)
            Me.m_tpAcknowledgements.ResumeLayout(False)
            Me.m_tpTechnical.ResumeLayout(False)
            Me.m_tlpTechnicalDetails.ResumeLayout(False)
            Me.m_tlpTechnicalDetails.PerformLayout()
            Me.m_tsTechnical.ResumeLayout(False)
            Me.m_tsTechnical.PerformLayout()
            Me.m_tpDatabase.ResumeLayout(False)
            Me.TableLayoutPanel2.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_btnOK As System.Windows.Forms.Button
        Private WithEvents m_rtbAcknowledgements As System.Windows.Forms.RichTextBox
        Private WithEvents m_pbFish0 As System.Windows.Forms.PictureBox
        Private WithEvents m_rtbDistribution As System.Windows.Forms.RichTextBox
        Private WithEvents m_tcMain As System.Windows.Forms.TabControl
        Private WithEvents m_tlpDetails As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_tlpTechnicalDetails As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_rtbTeam As System.Windows.Forms.RichTextBox
        Private WithEvents m_tlpGeneral As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_tpGeneral As System.Windows.Forms.TabPage
        Private WithEvents m_tpTeam As System.Windows.Forms.TabPage
        Private WithEvents m_tpAcknowledgements As System.Windows.Forms.TabPage
        Private WithEvents m_tpTechnical As System.Windows.Forms.TabPage
        Private WithEvents m_gridTechnical As ScientificInterface.gridAboutEwE
        Private WithEvents m_tpDatabase As System.Windows.Forms.TabPage
        Private WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_lblDatabase As System.Windows.Forms.Label
        Private WithEvents m_gridDatabase As gridDatabase
        Private WithEvents m_tpLicense As System.Windows.Forms.TabPage
        Private WithEvents m_rtbLicense As System.Windows.Forms.RichTextBox
        Private WithEvents m_rtbDisclaimer As System.Windows.Forms.RichTextBox
        Private WithEvents m_tsTechnical As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_tsbnShowEwEAssembliesOnly As System.Windows.Forms.ToolStripButton
        Friend WithEvents ToolStripLabel1 As System.Windows.Forms.ToolStripLabel
        Private WithEvents m_lbCopyright As System.Windows.Forms.TextBox
        Private WithEvents m_lbVersion As System.Windows.Forms.TextBox
        Private WithEvents m_lbLicense As System.Windows.Forms.TextBox
        Private WithEvents m_lbTitle As System.Windows.Forms.TextBox
        Private WithEvents m_lblNetVersion As System.Windows.Forms.Label

    End Class
End Namespace

