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

Imports ScientificInterfaceShared.Controls.Map

Namespace Ecospace.Advection

    Partial Class frmAdvection
        Inherits frmEwE

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
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_scMaps = New System.Windows.Forms.SplitContainer()
            Me.m_ucWind = New ScientificInterface.Ecospace.Advection.ucWind()
            Me.m_scOutputMaps = New System.Windows.Forms.SplitContainer()
            Me.m_ucAdvection = New ScientificInterface.Ecospace.Advection.ucMap()
            Me.m_ucUpwelling = New ScientificInterface.Ecospace.Advection.ucUpwelling()
            Me.m_tlpControls = New System.Windows.Forms.TableLayoutPanel()
            Me.m_tlpComputeControls = New System.Windows.Forms.TableLayoutPanel()
            Me.m_btnStart = New System.Windows.Forms.Button()
            Me.m_btnStop = New System.Windows.Forms.Button()
            Me.m_lblWIndEditorPlaceholder = New System.Windows.Forms.Label()
            Me.m_hdrCompute = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrParams = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lblUpwellingThreshold = New System.Windows.Forms.Label()
            Me.m_txtUpwelling = New System.Windows.Forms.TextBox()
            Me.m_lblUpwellingMult = New System.Windows.Forms.Label()
            Me.m_txtPPMult = New System.Windows.Forms.TextBox()
            Me.m_ucZoomToolbar = New ScientificInterfaceShared.Controls.Map.ucMapZoomToolbar()
            Me.m_tsAdvection = New System.Windows.Forms.ToolStrip()
            Me.m_tslView = New System.Windows.Forms.ToolStripLabel()
            Me.m_tscmbViewMap = New System.Windows.Forms.ToolStripComboBox()
            Me.m_tscmbViewMonth = New System.Windows.Forms.ToolStripComboBox()
            Me.m_sep = New System.Windows.Forms.ToolStripSeparator()
            Me.m_plParameters = New System.Windows.Forms.Panel()
            Me.m_plCompute = New System.Windows.Forms.Panel()
            Me.m_tlpMain = New System.Windows.Forms.TableLayoutPanel()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            CType(Me.m_scMaps, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMaps.Panel1.SuspendLayout()
            Me.m_scMaps.Panel2.SuspendLayout()
            Me.m_scMaps.SuspendLayout()
            CType(Me.m_scOutputMaps, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scOutputMaps.Panel1.SuspendLayout()
            Me.m_scOutputMaps.Panel2.SuspendLayout()
            Me.m_scOutputMaps.SuspendLayout()
            Me.m_tlpControls.SuspendLayout()
            Me.m_tlpComputeControls.SuspendLayout()
            Me.m_tsAdvection.SuspendLayout()
            Me.m_plParameters.SuspendLayout()
            Me.m_plCompute.SuspendLayout()
            Me.m_tlpMain.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_scMain
            '
            Me.m_scMain.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scMain.Location = New System.Drawing.Point(0, 31)
            Me.m_scMain.Margin = New System.Windows.Forms.Padding(0)
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            Me.m_scMain.Panel1.Controls.Add(Me.m_scMaps)
            Me.m_scMain.Panel1MinSize = 190
            '
            'm_scMain.Panel2
            '
            Me.m_scMain.Panel2.Controls.Add(Me.m_tlpControls)
            Me.m_scMain.Size = New System.Drawing.Size(722, 509)
            Me.m_scMain.SplitterDistance = 486
            Me.m_scMain.TabIndex = 1
            '
            'm_scMaps
            '
            Me.m_scMaps.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scMaps.Location = New System.Drawing.Point(0, 0)
            Me.m_scMaps.Name = "m_scMaps"
            Me.m_scMaps.Orientation = System.Windows.Forms.Orientation.Horizontal
            '
            'm_scMaps.Panel1
            '
            Me.m_scMaps.Panel1.Controls.Add(Me.m_ucWind)
            '
            'm_scMaps.Panel2
            '
            Me.m_scMaps.Panel2.Controls.Add(Me.m_scOutputMaps)
            Me.m_scMaps.Size = New System.Drawing.Size(486, 509)
            Me.m_scMaps.SplitterDistance = 335
            Me.m_scMaps.TabIndex = 0
            '
            'm_ucWind
            '
            Me.m_ucWind.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_ucWind.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_ucWind.Location = New System.Drawing.Point(0, 0)
            Me.m_ucWind.Margin = New System.Windows.Forms.Padding(3, 0, 0, 3)
            Me.m_ucWind.Name = "m_ucWind"
            Me.m_ucWind.Size = New System.Drawing.Size(486, 335)
            Me.m_ucWind.TabIndex = 0
            Me.m_ucWind.UIContext = Nothing
            '
            'm_scOutputMaps
            '
            Me.m_scOutputMaps.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_scOutputMaps.Location = New System.Drawing.Point(0, 0)
            Me.m_scOutputMaps.Name = "m_scOutputMaps"
            '
            'm_scOutputMaps.Panel1
            '
            Me.m_scOutputMaps.Panel1.Controls.Add(Me.m_ucAdvection)
            '
            'm_scOutputMaps.Panel2
            '
            Me.m_scOutputMaps.Panel2.Controls.Add(Me.m_ucUpwelling)
            Me.m_scOutputMaps.Size = New System.Drawing.Size(486, 170)
            Me.m_scOutputMaps.SplitterDistance = 244
            Me.m_scOutputMaps.TabIndex = 0
            '
            'm_ucAdvection
            '
            Me.m_ucAdvection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_ucAdvection.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_ucAdvection.Location = New System.Drawing.Point(0, 0)
            Me.m_ucAdvection.Margin = New System.Windows.Forms.Padding(0, 0, 3, 3)
            Me.m_ucAdvection.Name = "m_ucAdvection"
            Me.m_ucAdvection.Size = New System.Drawing.Size(244, 170)
            Me.m_ucAdvection.TabIndex = 0
            Me.m_ucAdvection.UIContext = Nothing
            '
            'm_ucUpwelling
            '
            Me.m_ucUpwelling.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_ucUpwelling.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_ucUpwelling.Location = New System.Drawing.Point(0, 0)
            Me.m_ucUpwelling.Margin = New System.Windows.Forms.Padding(3, 3, 0, 0)
            Me.m_ucUpwelling.Name = "m_ucUpwelling"
            Me.m_ucUpwelling.Size = New System.Drawing.Size(238, 170)
            Me.m_ucUpwelling.TabIndex = 0
            Me.m_ucUpwelling.UIContext = Nothing
            '
            'm_tlpControls
            '
            Me.m_tlpControls.AutoSize = True
            Me.m_tlpControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_tlpControls.ColumnCount = 1
            Me.m_tlpControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpControls.Controls.Add(Me.m_plCompute, 0, 2)
            Me.m_tlpControls.Controls.Add(Me.m_plParameters, 0, 1)
            Me.m_tlpControls.Controls.Add(Me.m_tsAdvection, 0, 3)
            Me.m_tlpControls.Controls.Add(Me.m_lblWIndEditorPlaceholder, 0, 0)
            Me.m_tlpControls.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_tlpControls.Location = New System.Drawing.Point(0, 0)
            Me.m_tlpControls.Name = "m_tlpControls"
            Me.m_tlpControls.RowCount = 4
            Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
            Me.m_tlpControls.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
            Me.m_tlpControls.Size = New System.Drawing.Size(232, 509)
            Me.m_tlpControls.TabIndex = 0
            '
            'm_tlpComputeControls
            '
            Me.m_tlpComputeControls.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_tlpComputeControls.ColumnCount = 2
            Me.m_tlpComputeControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpComputeControls.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpComputeControls.Controls.Add(Me.m_btnStop, 1, 0)
            Me.m_tlpComputeControls.Controls.Add(Me.m_btnStart, 0, 0)
            Me.m_tlpComputeControls.Location = New System.Drawing.Point(0, 21)
            Me.m_tlpComputeControls.Name = "m_tlpComputeControls"
            Me.m_tlpComputeControls.RowCount = 1
            Me.m_tlpComputeControls.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpComputeControls.Size = New System.Drawing.Size(232, 27)
            Me.m_tlpComputeControls.TabIndex = 1
            '
            'm_btnStart
            '
            Me.m_btnStart.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnStart.Location = New System.Drawing.Point(0, 0)
            Me.m_btnStart.Margin = New System.Windows.Forms.Padding(0, 0, 3, 0)
            Me.m_btnStart.Name = "m_btnStart"
            Me.m_btnStart.Size = New System.Drawing.Size(113, 23)
            Me.m_btnStart.TabIndex = 0
            Me.m_btnStart.Text = "&Compute"
            Me.m_btnStart.UseVisualStyleBackColor = True
            '
            'm_btnStop
            '
            Me.m_btnStop.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnStop.Location = New System.Drawing.Point(119, 0)
            Me.m_btnStop.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
            Me.m_btnStop.Name = "m_btnStop"
            Me.m_btnStop.Size = New System.Drawing.Size(113, 23)
            Me.m_btnStop.TabIndex = 1
            Me.m_btnStop.Text = "&Stop"
            Me.m_btnStop.UseVisualStyleBackColor = True
            '
            'm_lblWIndEditorPlaceholder
            '
            Me.m_lblWIndEditorPlaceholder.AutoSize = True
            Me.m_lblWIndEditorPlaceholder.Location = New System.Drawing.Point(3, 0)
            Me.m_lblWIndEditorPlaceholder.Name = "m_lblWIndEditorPlaceholder"
            Me.m_lblWIndEditorPlaceholder.Size = New System.Drawing.Size(148, 13)
            Me.m_lblWIndEditorPlaceholder.TabIndex = 1
            Me.m_lblWIndEditorPlaceholder.Text = "<wind edit panel placeholder>"
            '
            'm_hdrCompute
            '
            Me.m_hdrCompute.CanCollapseParent = False
            Me.m_hdrCompute.CollapsedParentHeight = 0
            Me.m_hdrCompute.Dock = System.Windows.Forms.DockStyle.Top
            Me.m_hdrCompute.IsCollapsed = False
            Me.m_hdrCompute.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrCompute.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrCompute.Name = "m_hdrCompute"
            Me.m_hdrCompute.Size = New System.Drawing.Size(232, 18)
            Me.m_hdrCompute.TabIndex = 0
            Me.m_hdrCompute.Text = "Compute advection velocities"
            Me.m_hdrCompute.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_hdrParams
            '
            Me.m_hdrParams.CanCollapseParent = False
            Me.m_hdrParams.CollapsedParentHeight = 0
            Me.m_hdrParams.Dock = System.Windows.Forms.DockStyle.Top
            Me.m_hdrParams.IsCollapsed = False
            Me.m_hdrParams.Location = New System.Drawing.Point(0, 0)
            Me.m_hdrParams.Name = "m_hdrParams"
            Me.m_hdrParams.Size = New System.Drawing.Size(232, 20)
            Me.m_hdrParams.TabIndex = 0
            Me.m_hdrParams.Text = "Model parameters"
            Me.m_hdrParams.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_lblUpwellingThreshold
            '
            Me.m_lblUpwellingThreshold.AutoSize = True
            Me.m_lblUpwellingThreshold.Location = New System.Drawing.Point(0, 23)
            Me.m_lblUpwellingThreshold.Margin = New System.Windows.Forms.Padding(3)
            Me.m_lblUpwellingThreshold.Name = "m_lblUpwellingThreshold"
            Me.m_lblUpwellingThreshold.Size = New System.Drawing.Size(102, 13)
            Me.m_lblUpwellingThreshold.TabIndex = 1
            Me.m_lblUpwellingThreshold.Text = "&Upwelling threshold:"
            Me.m_lblUpwellingThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_txtUpwelling
            '
            Me.m_txtUpwelling.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_txtUpwelling.Location = New System.Drawing.Point(120, 20)
            Me.m_txtUpwelling.Margin = New System.Windows.Forms.Padding(3, 3, 0, 3)
            Me.m_txtUpwelling.Name = "m_txtUpwelling"
            Me.m_txtUpwelling.Size = New System.Drawing.Size(112, 20)
            Me.m_txtUpwelling.TabIndex = 2
            '
            'm_lblUpwellingMult
            '
            Me.m_lblUpwellingMult.AutoSize = True
            Me.m_lblUpwellingMult.Location = New System.Drawing.Point(0, 49)
            Me.m_lblUpwellingMult.Margin = New System.Windows.Forms.Padding(3)
            Me.m_lblUpwellingMult.Name = "m_lblUpwellingMult"
            Me.m_lblUpwellingMult.Size = New System.Drawing.Size(114, 13)
            Me.m_lblUpwellingMult.TabIndex = 3
            Me.m_lblUpwellingMult.Text = "&PP upwelling multiplier:"
            Me.m_lblUpwellingMult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_txtPPMult
            '
            Me.m_txtPPMult.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_txtPPMult.Location = New System.Drawing.Point(120, 46)
            Me.m_txtPPMult.Margin = New System.Windows.Forms.Padding(3, 3, 0, 3)
            Me.m_txtPPMult.Name = "m_txtPPMult"
            Me.m_txtPPMult.Size = New System.Drawing.Size(112, 20)
            Me.m_txtPPMult.TabIndex = 4
            '
            'm_ucZoomToolbar
            '
            Me.m_ucZoomToolbar.AutoSize = True
            Me.m_ucZoomToolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.m_ucZoomToolbar.Dock = System.Windows.Forms.DockStyle.Top
            Me.m_ucZoomToolbar.Location = New System.Drawing.Point(3, 3)
            Me.m_ucZoomToolbar.MinimumSize = New System.Drawing.Size(100, 25)
            Me.m_ucZoomToolbar.Name = "m_ucZoomToolbar"
            Me.m_ucZoomToolbar.Size = New System.Drawing.Size(716, 25)
            Me.m_ucZoomToolbar.TabIndex = 0
            Me.m_ucZoomToolbar.UIContext = Nothing
            '
            'm_tsAdvection
            '
            Me.m_tsAdvection.Dock = System.Windows.Forms.DockStyle.None
            Me.m_tsAdvection.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tslView, Me.m_tscmbViewMap, Me.m_tscmbViewMonth, Me.m_sep})
            Me.m_tsAdvection.Location = New System.Drawing.Point(0, 152)
            Me.m_tsAdvection.Name = "m_tsAdvection"
            Me.m_tsAdvection.Size = New System.Drawing.Size(232, 27)
            Me.m_tsAdvection.TabIndex = 1
            Me.m_tsAdvection.Text = "ToolStrip1"
            '
            'm_tslView
            '
            Me.m_tslView.Name = "m_tslView"
            Me.m_tslView.Size = New System.Drawing.Size(35, 24)
            Me.m_tslView.Text = "View:"
            '
            'm_tscmbViewMap
            '
            Me.m_tscmbViewMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_tscmbViewMap.Name = "m_tscmbViewMap"
            Me.m_tscmbViewMap.Size = New System.Drawing.Size(121, 27)
            '
            'm_tscmbViewMonth
            '
            Me.m_tscmbViewMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_tscmbViewMonth.Name = "m_tscmbViewMonth"
            Me.m_tscmbViewMonth.Size = New System.Drawing.Size(121, 23)
            '
            'm_sep
            '
            Me.m_sep.Name = "m_sep"
            Me.m_sep.Size = New System.Drawing.Size(6, 25)
            '
            'm_plParameters
            '
            Me.m_plParameters.Controls.Add(Me.m_txtPPMult)
            Me.m_plParameters.Controls.Add(Me.m_txtUpwelling)
            Me.m_plParameters.Controls.Add(Me.m_lblUpwellingThreshold)
            Me.m_plParameters.Controls.Add(Me.m_lblUpwellingMult)
            Me.m_plParameters.Controls.Add(Me.m_hdrParams)
            Me.m_plParameters.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_plParameters.Location = New System.Drawing.Point(0, 23)
            Me.m_plParameters.Margin = New System.Windows.Forms.Padding(0, 10, 0, 0)
            Me.m_plParameters.Name = "m_plParameters"
            Me.m_plParameters.Size = New System.Drawing.Size(232, 70)
            Me.m_plParameters.TabIndex = 0
            '
            'm_plCompute
            '
            Me.m_plCompute.Controls.Add(Me.m_hdrCompute)
            Me.m_plCompute.Controls.Add(Me.m_tlpComputeControls)
            Me.m_plCompute.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_plCompute.Location = New System.Drawing.Point(0, 103)
            Me.m_plCompute.Margin = New System.Windows.Forms.Padding(0, 10, 0, 0)
            Me.m_plCompute.Name = "m_plCompute"
            Me.m_plCompute.Size = New System.Drawing.Size(232, 49)
            Me.m_plCompute.TabIndex = 1
            '
            'm_tlpMain
            '
            Me.m_tlpMain.ColumnCount = 1
            Me.m_tlpMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpMain.Controls.Add(Me.m_ucZoomToolbar, 0, 0)
            Me.m_tlpMain.Controls.Add(Me.m_scMain, 0, 1)
            Me.m_tlpMain.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_tlpMain.Location = New System.Drawing.Point(3, 3)
            Me.m_tlpMain.Margin = New System.Windows.Forms.Padding(0)
            Me.m_tlpMain.Name = "m_tlpMain"
            Me.m_tlpMain.RowCount = 2
            Me.m_tlpMain.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.m_tlpMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.m_tlpMain.Size = New System.Drawing.Size(722, 540)
            Me.m_tlpMain.TabIndex = 2
            '
            'frmAdvection
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.ClientSize = New System.Drawing.Size(728, 546)
            Me.Controls.Add(Me.m_tlpMain)
            Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "frmAdvection"
            Me.Padding = New System.Windows.Forms.Padding(3)
            Me.ShowInTaskbar = False
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            Me.TabText = ""
            Me.Text = "Advection"
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel2.ResumeLayout(False)
            Me.m_scMain.Panel2.PerformLayout()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            Me.m_scMaps.Panel1.ResumeLayout(False)
            Me.m_scMaps.Panel2.ResumeLayout(False)
            CType(Me.m_scMaps, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMaps.ResumeLayout(False)
            Me.m_scOutputMaps.Panel1.ResumeLayout(False)
            Me.m_scOutputMaps.Panel2.ResumeLayout(False)
            CType(Me.m_scOutputMaps, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scOutputMaps.ResumeLayout(False)
            Me.m_tlpControls.ResumeLayout(False)
            Me.m_tlpControls.PerformLayout()
            Me.m_tlpComputeControls.ResumeLayout(False)
            Me.m_tsAdvection.ResumeLayout(False)
            Me.m_tsAdvection.PerformLayout()
            Me.m_plParameters.ResumeLayout(False)
            Me.m_plParameters.PerformLayout()
            Me.m_plCompute.ResumeLayout(False)
            Me.m_tlpMain.ResumeLayout(False)
            Me.m_tlpMain.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_scMain As SplitContainer
        Private WithEvents m_ucUpwelling As ucUpwelling
        Private WithEvents m_ucWind As ucWind
        Private WithEvents m_ucAdvection As ucMap
        Private WithEvents m_ucZoomToolbar As ucMapZoomToolbar
        Private WithEvents m_scMaps As SplitContainer
        Private WithEvents m_scOutputMaps As SplitContainer
        Private WithEvents m_tlpControls As TableLayoutPanel
        Private WithEvents m_tlpComputeControls As TableLayoutPanel
        Private WithEvents m_btnStart As Button
        Private WithEvents m_btnStop As Button
        Private WithEvents m_lblWIndEditorPlaceholder As Label
        Private WithEvents m_hdrCompute As cEwEHeaderLabel
        Private WithEvents m_hdrParams As cEwEHeaderLabel
        Private WithEvents m_lblUpwellingThreshold As Label
        Private WithEvents m_txtUpwelling As TextBox
        Private WithEvents m_lblUpwellingMult As Label
        Private WithEvents m_txtPPMult As TextBox
        Private WithEvents m_tsAdvection As ToolStrip
        Private WithEvents m_tslView As ToolStripLabel
        Private WithEvents m_tscmbViewMap As ToolStripComboBox
        Private WithEvents m_tscmbViewMonth As ToolStripComboBox
        Private WithEvents m_sep As ToolStripSeparator
        Private WithEvents m_plCompute As Panel
        Private WithEvents m_plParameters As Panel
        Private WithEvents m_tlpMain As TableLayoutPanel
    End Class

End Namespace
