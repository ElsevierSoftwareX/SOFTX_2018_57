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
Imports ScientificInterfaceShared.Controls

Namespace Ecotracer

    Partial Class frmEcotracerInput
        Inherits frmEwEGrid

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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcotracerInput))
            Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_plAaargh = New System.Windows.Forms.Panel()
            Me.m_tlpGroups = New System.Windows.Forms.TableLayoutPanel()
            Me.m_grid = New ScientificInterface.Ecotracer.gridEcotracerInput()
            Me.m_tlp = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lbCZeroEnv = New System.Windows.Forms.Label()
            Me.m_lbCDecayRateEnv = New System.Windows.Forms.Label()
            Me.m_cmbEnvInflowFF = New System.Windows.Forms.ComboBox()
            Me.m_hdrGroups = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lbFFEnv = New System.Windows.Forms.Label()
            Me.m_lblCInflowEnv = New System.Windows.Forms.Label()
            Me.m_lblCDecay = New System.Windows.Forms.Label()
            Me.m_tbCDecayRateEnv = New System.Windows.Forms.TextBox()
            Me.m_tbCInflowEnv = New System.Windows.Forms.TextBox()
            Me.m_tbCLossEnv = New System.Windows.Forms.TextBox()
            Me.m_tbCZeroEnv = New System.Windows.Forms.TextBox()
            Me.m_lbMaxTS = New System.Windows.Forms.Label()
            Me.m_tbMaxTS = New System.Windows.Forms.TextBox()
            Me.m_lbConcentration = New System.Windows.Forms.Label()
            Me.m_btSelectFile = New System.Windows.Forms.Button()
            Me.m_tlpDriverFile = New System.Windows.Forms.TableLayoutPanel()
            Me.m_lbConcentrationFile = New System.Windows.Forms.Label()
            Me.m_btClearFile = New System.Windows.Forms.Button()
            Me.m_hdrInit = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plAaargh.SuspendLayout()
            Me.m_tlpGroups.SuspendLayout()
            Me.m_tlp.SuspendLayout()
            Me.m_tlpDriverFile.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_tsMain
            '
            resources.ApplyResources(Me.m_tsMain, "m_tsMain")
            Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsMain.Name = "m_tsMain"
            Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_plAaargh
            '
            Me.m_plAaargh.Controls.Add(Me.m_tlpGroups)
            Me.m_plAaargh.Controls.Add(Me.m_tlp)
            Me.m_plAaargh.Controls.Add(Me.m_hdrInit)
            resources.ApplyResources(Me.m_plAaargh, "m_plAaargh")
            Me.m_plAaargh.Name = "m_plAaargh"
            '
            'm_tlpGroups
            '
            resources.ApplyResources(Me.m_tlpGroups, "m_tlpGroups")
            Me.m_tlpGroups.Controls.Add(Me.m_tsMain, 0, 0)
            Me.m_tlpGroups.Controls.Add(Me.m_grid, 0, 1)
            Me.m_tlpGroups.Name = "m_tlpGroups"
            '
            'm_grid
            '
            Me.m_grid.AllowBlockSelect = True
            Me.m_grid.AutoSizeMinHeight = 10
            Me.m_grid.AutoSizeMinWidth = 10
            Me.m_grid.AutoStretchColumnsToFitWidth = False
            Me.m_grid.AutoStretchRowsToFitHeight = False
            Me.m_grid.BackColor = System.Drawing.Color.White
            Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
            Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
            Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_grid.CustomSort = False
            Me.m_grid.DataName = "grid content"
            resources.ApplyResources(Me.m_grid, "m_grid")
            Me.m_grid.FixedColumnWidths = False
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = True
            Me.m_grid.Name = "m_grid"
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.UIContext = Nothing
            '
            'm_tlp
            '
            resources.ApplyResources(Me.m_tlp, "m_tlp")
            Me.m_tlp.Controls.Add(Me.m_lbCZeroEnv, 0, 0)
            Me.m_tlp.Controls.Add(Me.m_lbCDecayRateEnv, 0, 1)
            Me.m_tlp.Controls.Add(Me.m_cmbEnvInflowFF, 1, 2)
            Me.m_tlp.Controls.Add(Me.m_hdrGroups, 0, 4)
            Me.m_tlp.Controls.Add(Me.m_lbFFEnv, 0, 2)
            Me.m_tlp.Controls.Add(Me.m_lblCInflowEnv, 3, 0)
            Me.m_tlp.Controls.Add(Me.m_lblCDecay, 3, 1)
            Me.m_tlp.Controls.Add(Me.m_tbCDecayRateEnv, 1, 1)
            Me.m_tlp.Controls.Add(Me.m_tbCInflowEnv, 4, 0)
            Me.m_tlp.Controls.Add(Me.m_tbCLossEnv, 4, 1)
            Me.m_tlp.Controls.Add(Me.m_tbCZeroEnv, 1, 0)
            Me.m_tlp.Controls.Add(Me.m_lbMaxTS, 3, 2)
            Me.m_tlp.Controls.Add(Me.m_tbMaxTS, 4, 2)
            Me.m_tlp.Controls.Add(Me.m_lbConcentration, 0, 3)
            Me.m_tlp.Controls.Add(Me.m_btSelectFile, 1, 3)
            Me.m_tlp.Controls.Add(Me.m_tlpDriverFile, 3, 3)
            Me.m_tlp.Name = "m_tlp"
            '
            'm_lbCZeroEnv
            '
            resources.ApplyResources(Me.m_lbCZeroEnv, "m_lbCZeroEnv")
            Me.m_lbCZeroEnv.Name = "m_lbCZeroEnv"
            '
            'm_lbCDecayRateEnv
            '
            resources.ApplyResources(Me.m_lbCDecayRateEnv, "m_lbCDecayRateEnv")
            Me.m_lbCDecayRateEnv.Name = "m_lbCDecayRateEnv"
            '
            'm_cmbEnvInflowFF
            '
            resources.ApplyResources(Me.m_cmbEnvInflowFF, "m_cmbEnvInflowFF")
            Me.m_cmbEnvInflowFF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbEnvInflowFF.FormattingEnabled = True
            Me.m_cmbEnvInflowFF.Name = "m_cmbEnvInflowFF"
            '
            'm_hdrGroups
            '
            resources.ApplyResources(Me.m_hdrGroups, "m_hdrGroups")
            Me.m_hdrGroups.CanCollapseParent = False
            Me.m_hdrGroups.CollapsedParentHeight = 0
            Me.m_tlp.SetColumnSpan(Me.m_hdrGroups, 5)
            Me.m_hdrGroups.IsCollapsed = False
            Me.m_hdrGroups.Name = "m_hdrGroups"
            '
            'm_lbFFEnv
            '
            resources.ApplyResources(Me.m_lbFFEnv, "m_lbFFEnv")
            Me.m_lbFFEnv.Name = "m_lbFFEnv"
            '
            'm_lblCInflowEnv
            '
            resources.ApplyResources(Me.m_lblCInflowEnv, "m_lblCInflowEnv")
            Me.m_lblCInflowEnv.Name = "m_lblCInflowEnv"
            '
            'm_lblCDecay
            '
            resources.ApplyResources(Me.m_lblCDecay, "m_lblCDecay")
            Me.m_lblCDecay.Name = "m_lblCDecay"
            '
            'm_tbCDecayRateEnv
            '
            resources.ApplyResources(Me.m_tbCDecayRateEnv, "m_tbCDecayRateEnv")
            Me.m_tbCDecayRateEnv.Name = "m_tbCDecayRateEnv"
            '
            'm_tbCInflowEnv
            '
            resources.ApplyResources(Me.m_tbCInflowEnv, "m_tbCInflowEnv")
            Me.m_tbCInflowEnv.Name = "m_tbCInflowEnv"
            '
            'm_tbCLossEnv
            '
            resources.ApplyResources(Me.m_tbCLossEnv, "m_tbCLossEnv")
            Me.m_tbCLossEnv.Name = "m_tbCLossEnv"
            '
            'm_tbCZeroEnv
            '
            resources.ApplyResources(Me.m_tbCZeroEnv, "m_tbCZeroEnv")
            Me.m_tbCZeroEnv.Name = "m_tbCZeroEnv"
            '
            'm_lbMaxTS
            '
            resources.ApplyResources(Me.m_lbMaxTS, "m_lbMaxTS")
            Me.m_lbMaxTS.Name = "m_lbMaxTS"
            '
            'm_tbMaxTS
            '
            resources.ApplyResources(Me.m_tbMaxTS, "m_tbMaxTS")
            Me.m_tbMaxTS.Name = "m_tbMaxTS"
            '
            'm_lbConcentration
            '
            resources.ApplyResources(Me.m_lbConcentration, "m_lbConcentration")
            Me.m_lbConcentration.Name = "m_lbConcentration"
            '
            'm_btSelectFile
            '
            resources.ApplyResources(Me.m_btSelectFile, "m_btSelectFile")
            Me.m_btSelectFile.Name = "m_btSelectFile"
            Me.m_btSelectFile.UseVisualStyleBackColor = True
            '
            'm_tlpDriverFile
            '
            resources.ApplyResources(Me.m_tlpDriverFile, "m_tlpDriverFile")
            Me.m_tlp.SetColumnSpan(Me.m_tlpDriverFile, 2)
            Me.m_tlpDriverFile.Controls.Add(Me.m_lbConcentrationFile, 0, 0)
            Me.m_tlpDriverFile.Controls.Add(Me.m_btClearFile, 1, 0)
            Me.m_tlpDriverFile.Name = "m_tlpDriverFile"
            '
            'm_lbConcentrationFile
            '
            resources.ApplyResources(Me.m_lbConcentrationFile, "m_lbConcentrationFile")
            Me.m_lbConcentrationFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_lbConcentrationFile.Name = "m_lbConcentrationFile"
            '
            'm_btClearFile
            '
            resources.ApplyResources(Me.m_btClearFile, "m_btClearFile")
            Me.m_btClearFile.Name = "m_btClearFile"
            Me.m_btClearFile.UseVisualStyleBackColor = True
            '
            'm_hdrInit
            '
            resources.ApplyResources(Me.m_hdrInit, "m_hdrInit")
            Me.m_hdrInit.CanCollapseParent = False
            Me.m_hdrInit.CollapsedParentHeight = 0
            Me.m_hdrInit.IsCollapsed = False
            Me.m_hdrInit.Name = "m_hdrInit"
            '
            'frmEcotracerInput
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_plAaargh)
            Me.Name = "frmEcotracerInput"
            Me.TabText = ""
            Me.m_plAaargh.ResumeLayout(False)
            Me.m_tlpGroups.ResumeLayout(False)
            Me.m_tlp.ResumeLayout(False)
            Me.m_tlp.PerformLayout()
            Me.m_tlpDriverFile.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_tsMain As ScientificInterfaceShared.Controls.cEwEToolstrip
        Private WithEvents m_plAaargh As System.Windows.Forms.Panel
        Private WithEvents m_hdrGroups As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_lbFFEnv As System.Windows.Forms.Label
        Private WithEvents m_cmbEnvInflowFF As System.Windows.Forms.ComboBox
        Private WithEvents m_tlp As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_lbCZeroEnv As System.Windows.Forms.Label
        Private WithEvents m_lbCDecayRateEnv As System.Windows.Forms.Label
        Private WithEvents m_lblCInflowEnv As System.Windows.Forms.Label
        Private WithEvents m_lblCDecay As System.Windows.Forms.Label
        Private WithEvents m_tbCDecayRateEnv As System.Windows.Forms.TextBox
        Private WithEvents m_tbCInflowEnv As System.Windows.Forms.TextBox
        Private WithEvents m_tbCLossEnv As System.Windows.Forms.TextBox
        Private WithEvents m_tbCZeroEnv As System.Windows.Forms.TextBox
        Private WithEvents m_grid As ScientificInterface.Ecotracer.gridEcotracerInput
        Private WithEvents m_hdrInit As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_tlpGroups As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents m_lbMaxTS As Label
        Friend WithEvents m_tbMaxTS As TextBox
        Friend WithEvents m_lbConcentration As Label
        Friend WithEvents m_btSelectFile As Button
        Friend WithEvents m_tlpDriverFile As TableLayoutPanel
        Friend WithEvents m_lbConcentrationFile As Label
        Friend WithEvents m_btClearFile As Button
    End Class

End Namespace
