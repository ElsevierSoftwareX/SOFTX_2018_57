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
Imports ScientificInterface.Controls

Namespace Ecosim

    Partial Class frmEcosimParameters
        : Inherits frmEwE

        'UserControl overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcosimParameters))
            Me.m_nudNutBaseFreeProp = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_nudNumberYears = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_cmbNutForcing = New System.Windows.Forms.ComboBox()
            Me.m_chkPredictEffort = New System.Windows.Forms.CheckBox()
            Me.m_chkConTracing = New System.Windows.Forms.CheckBox()
            Me.m_lblNutForcing = New System.Windows.Forms.Label()
            Me.m_lblNutBaseFreeProp = New System.Windows.Forms.Label()
            Me.m_lblNumberYears = New System.Windows.Forms.Label()
            Me.m_hdrInitialization = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrScenario = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tbContact = New System.Windows.Forms.TextBox()
            Me.m_tbAuthor = New System.Windows.Forms.TextBox()
            Me.m_lbContact = New System.Windows.Forms.Label()
            Me.m_lbAuthor = New System.Windows.Forms.Label()
            Me.m_tbName = New System.Windows.Forms.TextBox()
            Me.m_tbDescription = New System.Windows.Forms.TextBox()
            Me.m_lblDescription = New System.Windows.Forms.Label()
            Me.m_lbScenarioName = New System.Windows.Forms.Label()
            Me.m_chkUseVarPQ = New System.Windows.Forms.CheckBox()
            Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plDescription = New System.Windows.Forms.Panel()
            Me.m_plInit = New System.Windows.Forms.Panel()
            Me.m_tbxMinFeedingRateAdjustment = New System.Windows.Forms.TextBox()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.m_txSORwt = New System.Windows.Forms.TextBox()
            CType(Me.m_nudNutBaseFreeProp, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudNumberYears, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.TableLayoutPanel1.SuspendLayout()
            Me.m_plDescription.SuspendLayout()
            Me.m_plInit.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_nudNutBaseFreeProp
            '
            Me.m_nudNutBaseFreeProp.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
            Me.m_nudNutBaseFreeProp.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudNutBaseFreeProp, "m_nudNutBaseFreeProp")
            Me.m_nudNutBaseFreeProp.Maximum = New Decimal(New Integer() {999, 0, 0, 196608})
            Me.m_nudNutBaseFreeProp.Minimum = New Decimal(New Integer() {1, 0, 0, 65536})
            Me.m_nudNutBaseFreeProp.Name = "m_nudNutBaseFreeProp"
            Me.m_nudNutBaseFreeProp.Value = New Decimal(New Integer() {1, 0, 0, 65536})
            '
            'm_nudNumberYears
            '
            Me.m_nudNumberYears.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudNumberYears, "m_nudNumberYears")
            Me.m_nudNumberYears.Name = "m_nudNumberYears"
            '
            'm_cmbNutForcing
            '
            Me.m_cmbNutForcing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbNutForcing.FormattingEnabled = True
            resources.ApplyResources(Me.m_cmbNutForcing, "m_cmbNutForcing")
            Me.m_cmbNutForcing.Name = "m_cmbNutForcing"
            '
            'm_chkPredictEffort
            '
            resources.ApplyResources(Me.m_chkPredictEffort, "m_chkPredictEffort")
            Me.m_chkPredictEffort.Name = "m_chkPredictEffort"
            Me.m_chkPredictEffort.UseVisualStyleBackColor = True
            '
            'm_chkConTracing
            '
            resources.ApplyResources(Me.m_chkConTracing, "m_chkConTracing")
            Me.m_chkConTracing.Name = "m_chkConTracing"
            Me.m_chkConTracing.UseVisualStyleBackColor = True
            '
            'm_lblNutForcing
            '
            resources.ApplyResources(Me.m_lblNutForcing, "m_lblNutForcing")
            Me.m_lblNutForcing.Name = "m_lblNutForcing"
            '
            'm_lblNutBaseFreeProp
            '
            resources.ApplyResources(Me.m_lblNutBaseFreeProp, "m_lblNutBaseFreeProp")
            Me.m_lblNutBaseFreeProp.Name = "m_lblNutBaseFreeProp"
            '
            'm_lblNumberYears
            '
            resources.ApplyResources(Me.m_lblNumberYears, "m_lblNumberYears")
            Me.m_lblNumberYears.Name = "m_lblNumberYears"
            '
            'm_hdrInitialization
            '
            Me.m_hdrInitialization.CanCollapseParent = True
            Me.m_hdrInitialization.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrInitialization, "m_hdrInitialization")
            Me.m_hdrInitialization.IsCollapsed = False
            Me.m_hdrInitialization.Name = "m_hdrInitialization"
            '
            'm_hdrScenario
            '
            Me.m_hdrScenario.CanCollapseParent = True
            Me.m_hdrScenario.CollapsedParentHeight = 92
            resources.ApplyResources(Me.m_hdrScenario, "m_hdrScenario")
            Me.m_hdrScenario.IsCollapsed = False
            Me.m_hdrScenario.Name = "m_hdrScenario"
            '
            'm_tbContact
            '
            resources.ApplyResources(Me.m_tbContact, "m_tbContact")
            Me.m_tbContact.Name = "m_tbContact"
            '
            'm_tbAuthor
            '
            resources.ApplyResources(Me.m_tbAuthor, "m_tbAuthor")
            Me.m_tbAuthor.Name = "m_tbAuthor"
            '
            'm_lbContact
            '
            resources.ApplyResources(Me.m_lbContact, "m_lbContact")
            Me.m_lbContact.Name = "m_lbContact"
            '
            'm_lbAuthor
            '
            resources.ApplyResources(Me.m_lbAuthor, "m_lbAuthor")
            Me.m_lbAuthor.Name = "m_lbAuthor"
            '
            'm_tbName
            '
            resources.ApplyResources(Me.m_tbName, "m_tbName")
            Me.m_tbName.Name = "m_tbName"
            '
            'm_tbDescription
            '
            resources.ApplyResources(Me.m_tbDescription, "m_tbDescription")
            Me.m_tbDescription.Name = "m_tbDescription"
            '
            'm_lblDescription
            '
            resources.ApplyResources(Me.m_lblDescription, "m_lblDescription")
            Me.m_lblDescription.Name = "m_lblDescription"
            '
            'm_lbScenarioName
            '
            resources.ApplyResources(Me.m_lbScenarioName, "m_lbScenarioName")
            Me.m_lbScenarioName.Name = "m_lbScenarioName"
            '
            'm_chkUseVarPQ
            '
            resources.ApplyResources(Me.m_chkUseVarPQ, "m_chkUseVarPQ")
            Me.m_chkUseVarPQ.Name = "m_chkUseVarPQ"
            Me.m_chkUseVarPQ.UseVisualStyleBackColor = True
            '
            'TableLayoutPanel1
            '
            resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
            Me.TableLayoutPanel1.Controls.Add(Me.m_plDescription, 0, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.m_plInit, 0, 1)
            Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
            '
            'm_plDescription
            '
            Me.m_plDescription.Controls.Add(Me.m_hdrScenario)
            Me.m_plDescription.Controls.Add(Me.m_lbScenarioName)
            Me.m_plDescription.Controls.Add(Me.m_tbName)
            Me.m_plDescription.Controls.Add(Me.m_lblDescription)
            Me.m_plDescription.Controls.Add(Me.m_tbContact)
            Me.m_plDescription.Controls.Add(Me.m_tbDescription)
            Me.m_plDescription.Controls.Add(Me.m_lbContact)
            Me.m_plDescription.Controls.Add(Me.m_lbAuthor)
            Me.m_plDescription.Controls.Add(Me.m_tbAuthor)
            resources.ApplyResources(Me.m_plDescription, "m_plDescription")
            Me.m_plDescription.Name = "m_plDescription"
            '
            'm_plInit
            '
            Me.m_plInit.Controls.Add(Me.m_txSORwt)
            Me.m_plInit.Controls.Add(Me.Label2)
            Me.m_plInit.Controls.Add(Me.m_tbxMinFeedingRateAdjustment)
            Me.m_plInit.Controls.Add(Me.m_hdrInitialization)
            Me.m_plInit.Controls.Add(Me.Label1)
            Me.m_plInit.Controls.Add(Me.m_lblNumberYears)
            Me.m_plInit.Controls.Add(Me.m_lblNutBaseFreeProp)
            Me.m_plInit.Controls.Add(Me.m_nudNutBaseFreeProp)
            Me.m_plInit.Controls.Add(Me.m_lblNutForcing)
            Me.m_plInit.Controls.Add(Me.m_nudNumberYears)
            Me.m_plInit.Controls.Add(Me.m_chkConTracing)
            Me.m_plInit.Controls.Add(Me.m_cmbNutForcing)
            Me.m_plInit.Controls.Add(Me.m_chkUseVarPQ)
            Me.m_plInit.Controls.Add(Me.m_chkPredictEffort)
            resources.ApplyResources(Me.m_plInit, "m_plInit")
            Me.m_plInit.Name = "m_plInit"
            '
            'm_tbxMinFeedingRateAdjustment
            '
            resources.ApplyResources(Me.m_tbxMinFeedingRateAdjustment, "m_tbxMinFeedingRateAdjustment")
            Me.m_tbxMinFeedingRateAdjustment.Name = "m_tbxMinFeedingRateAdjustment"
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'Label2
            '
            resources.ApplyResources(Me.Label2, "Label2")
            Me.Label2.Name = "Label2"
            '
            'm_txSORwt
            '
            resources.ApplyResources(Me.m_txSORwt, "m_txSORwt")
            Me.m_txSORwt.Name = "m_txSORwt"
            '
            'frmEcosimParameters
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.TableLayoutPanel1)
            Me.Name = "frmEcosimParameters"
            Me.TabText = ""
            CType(Me.m_nudNutBaseFreeProp, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudNumberYears, System.ComponentModel.ISupportInitialize).EndInit()
            Me.TableLayoutPanel1.ResumeLayout(False)
            Me.m_plDescription.ResumeLayout(False)
            Me.m_plDescription.PerformLayout()
            Me.m_plInit.ResumeLayout(False)
            Me.m_plInit.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_tbDescription As System.Windows.Forms.TextBox
        Private WithEvents m_tbName As System.Windows.Forms.TextBox
        Private WithEvents m_tbContact As System.Windows.Forms.TextBox
        Private WithEvents m_tbAuthor As System.Windows.Forms.TextBox
        Private WithEvents m_chkUseVarPQ As System.Windows.Forms.CheckBox
        Private WithEvents m_lbScenarioName As System.Windows.Forms.Label
        Private WithEvents m_lbContact As System.Windows.Forms.Label
        Private WithEvents m_lbAuthor As System.Windows.Forms.Label
        Private WithEvents m_lblDescription As System.Windows.Forms.Label
        Private WithEvents m_lblNumberYears As System.Windows.Forms.Label
        Private WithEvents m_lblNutBaseFreeProp As System.Windows.Forms.Label
        Private WithEvents m_cmbNutForcing As System.Windows.Forms.ComboBox
        Private WithEvents m_lblNutForcing As System.Windows.Forms.Label
        Private WithEvents m_chkPredictEffort As System.Windows.Forms.CheckBox
        Private WithEvents m_chkConTracing As System.Windows.Forms.CheckBox
        Private WithEvents m_hdrScenario As cEwEHeaderLabel
        Private WithEvents m_hdrInitialization As cEwEHeaderLabel
        Private WithEvents m_nudNutBaseFreeProp As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudNumberYears As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_plDescription As System.Windows.Forms.Panel
        Private WithEvents m_plInit As System.Windows.Forms.Panel
        Private WithEvents Label1 As System.Windows.Forms.Label
        Friend WithEvents m_tbxMinFeedingRateAdjustment As System.Windows.Forms.TextBox
        Friend WithEvents m_txSORwt As TextBox
        Friend WithEvents Label2 As Label
    End Class
End Namespace

