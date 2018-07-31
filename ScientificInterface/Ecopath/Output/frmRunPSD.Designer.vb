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

Namespace Ecopath.Output

    Partial Class RunPSD
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
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RunPSD))
            Me.m_zedgraph = New ZedGraph.ZedGraphControl()
            Me.m_scContent = New System.Windows.Forms.SplitContainer()
            Me.m_tlpInputs = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plTotalMortality = New System.Windows.Forms.Panel()
            Me.m_cmbMeanLat = New System.Windows.Forms.ComboBox()
            Me.m_lblMeanLat = New System.Windows.Forms.Label()
            Me.m_hdrTotalMort = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_rbLorenzen = New System.Windows.Forms.RadioButton()
            Me.m_rbGroupPB = New System.Windows.Forms.RadioButton()
            Me.m_plInputs = New System.Windows.Forms.Panel()
            Me.NumericUpDown1 = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudLowestWtClass = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudNoWtClasses = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_lblNoPts = New System.Windows.Forms.Label()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.m_lblNoWtClasses = New System.Windows.Forms.Label()
            Me.m_hdrInputs = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.Panel1 = New System.Windows.Forms.Panel()
            Me.m_bntShowGroups = New System.Windows.Forms.Button()
            Me.m_btnRun = New System.Windows.Forms.Button()
            Me.m_hdrRun = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            CType(Me.m_scContent, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scContent.Panel1.SuspendLayout()
            Me.m_scContent.Panel2.SuspendLayout()
            Me.m_scContent.SuspendLayout()
            Me.m_tlpInputs.SuspendLayout()
            Me.m_plTotalMortality.SuspendLayout()
            Me.m_plInputs.SuspendLayout()
            CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudLowestWtClass, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudNoWtClasses, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.Panel1.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_zedgraph
            '
            resources.ApplyResources(Me.m_zedgraph, "m_zedgraph")
            Me.m_zedgraph.Name = "m_zedgraph"
            Me.m_zedgraph.ScrollGrace = 0.0R
            Me.m_zedgraph.ScrollMaxX = 0.0R
            Me.m_zedgraph.ScrollMaxY = 0.0R
            Me.m_zedgraph.ScrollMaxY2 = 0.0R
            Me.m_zedgraph.ScrollMinX = 0.0R
            Me.m_zedgraph.ScrollMinY = 0.0R
            Me.m_zedgraph.ScrollMinY2 = 0.0R
            '
            'm_scContent
            '
            resources.ApplyResources(Me.m_scContent, "m_scContent")
            Me.m_scContent.Name = "m_scContent"
            '
            'm_scContent.Panel1
            '
            Me.m_scContent.Panel1.Controls.Add(Me.m_tlpInputs)
            '
            'm_scContent.Panel2
            '
            Me.m_scContent.Panel2.Controls.Add(Me.m_zedgraph)
            '
            'm_tlpInputs
            '
            resources.ApplyResources(Me.m_tlpInputs, "m_tlpInputs")
            Me.m_tlpInputs.Controls.Add(Me.m_plTotalMortality, 0, 0)
            Me.m_tlpInputs.Controls.Add(Me.m_plInputs, 0, 1)
            Me.m_tlpInputs.Controls.Add(Me.Panel1, 0, 2)
            Me.m_tlpInputs.Name = "m_tlpInputs"
            '
            'm_plTotalMortality
            '
            Me.m_plTotalMortality.Controls.Add(Me.m_cmbMeanLat)
            Me.m_plTotalMortality.Controls.Add(Me.m_lblMeanLat)
            Me.m_plTotalMortality.Controls.Add(Me.m_hdrTotalMort)
            Me.m_plTotalMortality.Controls.Add(Me.m_rbLorenzen)
            Me.m_plTotalMortality.Controls.Add(Me.m_rbGroupPB)
            resources.ApplyResources(Me.m_plTotalMortality, "m_plTotalMortality")
            Me.m_plTotalMortality.Name = "m_plTotalMortality"
            '
            'm_cmbMeanLat
            '
            resources.ApplyResources(Me.m_cmbMeanLat, "m_cmbMeanLat")
            Me.m_cmbMeanLat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbMeanLat.FormattingEnabled = True
            Me.m_cmbMeanLat.Name = "m_cmbMeanLat"
            '
            'm_lblMeanLat
            '
            resources.ApplyResources(Me.m_lblMeanLat, "m_lblMeanLat")
            Me.m_lblMeanLat.Name = "m_lblMeanLat"
            '
            'm_hdrTotalMort
            '
            resources.ApplyResources(Me.m_hdrTotalMort, "m_hdrTotalMort")
            Me.m_hdrTotalMort.CanCollapseParent = False
            Me.m_hdrTotalMort.CollapsedParentHeight = 0
            Me.m_hdrTotalMort.IsCollapsed = False
            Me.m_hdrTotalMort.Name = "m_hdrTotalMort"
            '
            'm_rbLorenzen
            '
            resources.ApplyResources(Me.m_rbLorenzen, "m_rbLorenzen")
            Me.m_rbLorenzen.Name = "m_rbLorenzen"
            Me.m_rbLorenzen.UseVisualStyleBackColor = True
            '
            'm_rbGroupPB
            '
            resources.ApplyResources(Me.m_rbGroupPB, "m_rbGroupPB")
            Me.m_rbGroupPB.Name = "m_rbGroupPB"
            Me.m_rbGroupPB.UseVisualStyleBackColor = True
            '
            'm_plInputs
            '
            Me.m_plInputs.Controls.Add(Me.NumericUpDown1)
            Me.m_plInputs.Controls.Add(Me.m_nudLowestWtClass)
            Me.m_plInputs.Controls.Add(Me.m_nudNoWtClasses)
            Me.m_plInputs.Controls.Add(Me.m_lblNoPts)
            Me.m_plInputs.Controls.Add(Me.Label1)
            Me.m_plInputs.Controls.Add(Me.m_lblNoWtClasses)
            Me.m_plInputs.Controls.Add(Me.m_hdrInputs)
            resources.ApplyResources(Me.m_plInputs, "m_plInputs")
            Me.m_plInputs.Name = "m_plInputs"
            '
            'NumericUpDown1
            '
            resources.ApplyResources(Me.NumericUpDown1, "NumericUpDown1")
            Me.NumericUpDown1.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.NumericUpDown1.Name = "NumericUpDown1"
            '
            'm_nudLowestWtClass
            '
            resources.ApplyResources(Me.m_nudLowestWtClass, "m_nudLowestWtClass")
            Me.m_nudLowestWtClass.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudLowestWtClass.Name = "m_nudLowestWtClass"
            '
            'm_nudNoWtClasses
            '
            resources.ApplyResources(Me.m_nudNoWtClasses, "m_nudNoWtClasses")
            Me.m_nudNoWtClasses.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            Me.m_nudNoWtClasses.Name = "m_nudNoWtClasses"
            '
            'm_lblNoPts
            '
            resources.ApplyResources(Me.m_lblNoPts, "m_lblNoPts")
            Me.m_lblNoPts.Name = "m_lblNoPts"
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'm_lblNoWtClasses
            '
            resources.ApplyResources(Me.m_lblNoWtClasses, "m_lblNoWtClasses")
            Me.m_lblNoWtClasses.Name = "m_lblNoWtClasses"
            '
            'm_hdrInputs
            '
            resources.ApplyResources(Me.m_hdrInputs, "m_hdrInputs")
            Me.m_hdrInputs.CanCollapseParent = False
            Me.m_hdrInputs.CollapsedParentHeight = 0
            Me.m_hdrInputs.IsCollapsed = False
            Me.m_hdrInputs.Name = "m_hdrInputs"
            '
            'Panel1
            '
            Me.Panel1.Controls.Add(Me.m_bntShowGroups)
            Me.Panel1.Controls.Add(Me.m_btnRun)
            Me.Panel1.Controls.Add(Me.m_hdrRun)
            resources.ApplyResources(Me.Panel1, "Panel1")
            Me.Panel1.Name = "Panel1"
            '
            'm_bntShowGroups
            '
            resources.ApplyResources(Me.m_bntShowGroups, "m_bntShowGroups")
            Me.m_bntShowGroups.Name = "m_bntShowGroups"
            Me.m_bntShowGroups.UseVisualStyleBackColor = True
            '
            'm_btnRun
            '
            resources.ApplyResources(Me.m_btnRun, "m_btnRun")
            Me.m_btnRun.Name = "m_btnRun"
            Me.m_btnRun.UseVisualStyleBackColor = True
            '
            'm_hdrRun
            '
            resources.ApplyResources(Me.m_hdrRun, "m_hdrRun")
            Me.m_hdrRun.CanCollapseParent = False
            Me.m_hdrRun.CollapsedParentHeight = 0
            Me.m_hdrRun.IsCollapsed = False
            Me.m_hdrRun.Name = "m_hdrRun"
            '
            'RunPSD
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_scContent)
            Me.Name = "RunPSD"
            Me.ShowInTaskbar = False
            Me.TabText = ""
            Me.m_scContent.Panel1.ResumeLayout(False)
            Me.m_scContent.Panel2.ResumeLayout(False)
            CType(Me.m_scContent, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scContent.ResumeLayout(False)
            Me.m_tlpInputs.ResumeLayout(False)
            Me.m_plTotalMortality.ResumeLayout(False)
            Me.m_plTotalMortality.PerformLayout()
            Me.m_plInputs.ResumeLayout(False)
            Me.m_plInputs.PerformLayout()
            CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudLowestWtClass, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudNoWtClasses, System.ComponentModel.ISupportInitialize).EndInit()
            Me.Panel1.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_zedgraph As ZedGraph.ZedGraphControl
        Private WithEvents m_scContent As System.Windows.Forms.SplitContainer
        Friend WithEvents m_tlpInputs As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents m_plTotalMortality As System.Windows.Forms.Panel
        Friend WithEvents m_rbGroupPB As System.Windows.Forms.RadioButton
        Friend WithEvents m_hdrTotalMort As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Friend WithEvents m_rbLorenzen As System.Windows.Forms.RadioButton
        Friend WithEvents m_plInputs As System.Windows.Forms.Panel
        Friend WithEvents m_lblNoWtClasses As System.Windows.Forms.Label
        Friend WithEvents m_hdrInputs As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Friend WithEvents m_lblNoPts As System.Windows.Forms.Label
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Friend WithEvents Panel1 As System.Windows.Forms.Panel
        Friend WithEvents m_btnRun As System.Windows.Forms.Button
        Friend WithEvents m_hdrRun As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Friend WithEvents m_bntShowGroups As System.Windows.Forms.Button
        Friend WithEvents m_lblMeanLat As System.Windows.Forms.Label
        Private WithEvents m_cmbMeanLat As System.Windows.Forms.ComboBox
        Friend WithEvents m_nudNoWtClasses As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Friend WithEvents NumericUpDown1 As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Friend WithEvents m_nudLowestWtClass As ScientificInterfaceShared.Controls.cEwENumericUpDown
    End Class

End Namespace
