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

    Partial Class frmEcotracerParameters
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcotracerParameters))
            Me.m_tbContact = New System.Windows.Forms.TextBox()
            Me.m_tbAuthor = New System.Windows.Forms.TextBox()
            Me.m_lbContact = New System.Windows.Forms.Label()
            Me.m_lbAuthor = New System.Windows.Forms.Label()
            Me.m_tbName = New System.Windows.Forms.TextBox()
            Me.m_tbDescription = New System.Windows.Forms.TextBox()
            Me.m_lblDescription = New System.Windows.Forms.Label()
            Me.m_lbScenario = New System.Windows.Forms.Label()
            Me.m_hdrScenario = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrSponsors = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tlpSponsors = New System.Windows.Forms.TableLayoutPanel()
            Me.m_pbFMIR = New System.Windows.Forms.PictureBox()
            Me.m_pbEU = New System.Windows.Forms.PictureBox()
            Me.m_pbLenfest = New System.Windows.Forms.PictureBox()
            Me.m_pbSAUP = New System.Windows.Forms.PictureBox()
            Me.m_hdrInitialization = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_rbSpace = New System.Windows.Forms.RadioButton()
            Me.m_rbSim = New System.Windows.Forms.RadioButton()
            Me.m_rbDisabled = New System.Windows.Forms.RadioButton()
            Me.m_tlpSponsors.SuspendLayout()
            CType(Me.m_pbFMIR, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_pbEU, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_pbLenfest, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_pbSAUP, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
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
            'm_lbScenario
            '
            resources.ApplyResources(Me.m_lbScenario, "m_lbScenario")
            Me.m_lbScenario.Name = "m_lbScenario"
            '
            'm_hdrScenario
            '
            resources.ApplyResources(Me.m_hdrScenario, "m_hdrScenario")
            Me.m_hdrScenario.CanCollapseParent = False
            Me.m_hdrScenario.CollapsedParentHeight = 0
            Me.m_hdrScenario.IsCollapsed = False
            Me.m_hdrScenario.Name = "m_hdrScenario"
            '
            'm_hdrSponsors
            '
            resources.ApplyResources(Me.m_hdrSponsors, "m_hdrSponsors")
            Me.m_hdrSponsors.CanCollapseParent = False
            Me.m_hdrSponsors.CollapsedParentHeight = 0
            Me.m_hdrSponsors.IsCollapsed = False
            Me.m_hdrSponsors.Name = "m_hdrSponsors"
            '
            'm_tlpSponsors
            '
            resources.ApplyResources(Me.m_tlpSponsors, "m_tlpSponsors")
            Me.m_tlpSponsors.BackColor = System.Drawing.Color.White
            Me.m_tlpSponsors.Controls.Add(Me.m_pbFMIR, 0, 0)
            Me.m_tlpSponsors.Controls.Add(Me.m_pbEU, 1, 0)
            Me.m_tlpSponsors.Controls.Add(Me.m_pbLenfest, 2, 0)
            Me.m_tlpSponsors.Controls.Add(Me.m_pbSAUP, 3, 0)
            Me.m_tlpSponsors.Name = "m_tlpSponsors"
            '
            'm_pbFMIR
            '
            resources.ApplyResources(Me.m_pbFMIR, "m_pbFMIR")
            Me.m_pbFMIR.Image = Global.ScientificInterface.My.Resources.Resources.logo_FIMR
            Me.m_pbFMIR.Name = "m_pbFMIR"
            Me.m_pbFMIR.TabStop = False
            '
            'm_pbEU
            '
            resources.ApplyResources(Me.m_pbEU, "m_pbEU")
            Me.m_pbEU.Image = Global.ScientificInterface.My.Resources.Resources.logo_EU
            Me.m_pbEU.Name = "m_pbEU"
            Me.m_pbEU.TabStop = False
            '
            'm_pbLenfest
            '
            Me.m_pbLenfest.BackgroundImage = Global.ScientificInterface.My.Resources.Resources.logo_LENFEST
            resources.ApplyResources(Me.m_pbLenfest, "m_pbLenfest")
            Me.m_pbLenfest.Name = "m_pbLenfest"
            Me.m_pbLenfest.TabStop = False
            '
            'm_pbSAUP
            '
            resources.ApplyResources(Me.m_pbSAUP, "m_pbSAUP")
            Me.m_pbSAUP.Image = Global.ScientificInterface.My.Resources.Resources.logo_SAUP
            Me.m_pbSAUP.Name = "m_pbSAUP"
            Me.m_pbSAUP.TabStop = False
            '
            'm_hdrInitialization
            '
            resources.ApplyResources(Me.m_hdrInitialization, "m_hdrInitialization")
            Me.m_hdrInitialization.CanCollapseParent = False
            Me.m_hdrInitialization.CollapsedParentHeight = 0
            Me.m_hdrInitialization.IsCollapsed = False
            Me.m_hdrInitialization.Name = "m_hdrInitialization"
            '
            'm_rbSpace
            '
            resources.ApplyResources(Me.m_rbSpace, "m_rbSpace")
            Me.m_rbSpace.Name = "m_rbSpace"
            Me.m_rbSpace.TabStop = True
            Me.m_rbSpace.UseVisualStyleBackColor = True
            '
            'm_rbSim
            '
            resources.ApplyResources(Me.m_rbSim, "m_rbSim")
            Me.m_rbSim.Name = "m_rbSim"
            Me.m_rbSim.TabStop = True
            Me.m_rbSim.UseVisualStyleBackColor = True
            '
            'm_rbDisabled
            '
            resources.ApplyResources(Me.m_rbDisabled, "m_rbDisabled")
            Me.m_rbDisabled.Checked = True
            Me.m_rbDisabled.Name = "m_rbDisabled"
            Me.m_rbDisabled.TabStop = True
            Me.m_rbDisabled.UseVisualStyleBackColor = True
            '
            'frmEcotracerParameters
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.ControlBox = False
            Me.Controls.Add(Me.m_rbSpace)
            Me.Controls.Add(Me.m_tbContact)
            Me.Controls.Add(Me.m_rbSim)
            Me.Controls.Add(Me.m_tlpSponsors)
            Me.Controls.Add(Me.m_rbDisabled)
            Me.Controls.Add(Me.m_tbAuthor)
            Me.Controls.Add(Me.m_lbContact)
            Me.Controls.Add(Me.m_hdrInitialization)
            Me.Controls.Add(Me.m_lbAuthor)
            Me.Controls.Add(Me.m_hdrSponsors)
            Me.Controls.Add(Me.m_tbName)
            Me.Controls.Add(Me.m_tbDescription)
            Me.Controls.Add(Me.m_lblDescription)
            Me.Controls.Add(Me.m_hdrScenario)
            Me.Controls.Add(Me.m_lbScenario)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "frmEcotracerParameters"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.TabText = ""
            Me.m_tlpSponsors.ResumeLayout(False)
            CType(Me.m_pbFMIR, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_pbEU, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_pbLenfest, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_pbSAUP, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_tbContact As System.Windows.Forms.TextBox
        Private WithEvents m_tbAuthor As System.Windows.Forms.TextBox
        Private WithEvents m_lbContact As System.Windows.Forms.Label
        Private WithEvents m_lbAuthor As System.Windows.Forms.Label
        Private WithEvents m_tbName As System.Windows.Forms.TextBox
        Private WithEvents m_tbDescription As System.Windows.Forms.TextBox
        Private WithEvents m_lblDescription As System.Windows.Forms.Label
        Private WithEvents m_lbScenario As System.Windows.Forms.Label
        Private WithEvents m_hdrScenario As cEwEHeaderLabel
        Private WithEvents m_hdrSponsors As cEwEHeaderLabel
        Private WithEvents m_tlpSponsors As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_pbFMIR As System.Windows.Forms.PictureBox
        Private WithEvents m_pbEU As System.Windows.Forms.PictureBox
        Private WithEvents m_pbLenfest As System.Windows.Forms.PictureBox
        Private WithEvents m_pbSAUP As System.Windows.Forms.PictureBox
        Private WithEvents m_hdrInitialization As cEwEHeaderLabel
        Private WithEvents m_rbDisabled As System.Windows.Forms.RadioButton
        Private WithEvents m_rbSpace As System.Windows.Forms.RadioButton
        Private WithEvents m_rbSim As System.Windows.Forms.RadioButton
    End Class

End Namespace
