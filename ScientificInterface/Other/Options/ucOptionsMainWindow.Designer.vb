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

Imports ScientificInterfaceShared

Namespace Other

    Partial Class ucOptionsPresentation
        Inherits System.Windows.Forms.UserControl

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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucOptionsPresentation))
            Me.m_hdrCaption = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lblEntering = New System.Windows.Forms.Label()
            Me.m_cbHideModelBar = New System.Windows.Forms.CheckBox()
            Me.m_cbHideStatusBar = New System.Windows.Forms.CheckBox()
            Me.m_cbHideMainMenu = New System.Windows.Forms.CheckBox()
            Me.m_cbCollapseNavPanel = New System.Windows.Forms.CheckBox()
            Me.m_lblUnit2 = New System.Windows.Forms.Label()
            Me.m_lblUnit1 = New System.Windows.Forms.Label()
            Me.m_lblWhat = New System.Windows.Forms.Label()
            Me.m_tbxH = New System.Windows.Forms.TextBox()
            Me.m_tbxW = New System.Windows.Forms.TextBox()
            Me.m_lblHeight = New System.Windows.Forms.Label()
            Me.m_lblWidth = New System.Windows.Forms.Label()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.m_rbIn = New System.Windows.Forms.RadioButton()
            Me.m_rbOut = New System.Windows.Forms.RadioButton()
            Me.CEwEHeaderLabel1 = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.SuspendLayout()
            '
            'm_hdrCaption
            '
            Me.m_hdrCaption.CanCollapseParent = False
            Me.m_hdrCaption.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrCaption, "m_hdrCaption")
            Me.m_hdrCaption.IsCollapsed = False
            Me.m_hdrCaption.Name = "m_hdrCaption"
            '
            'm_lblEntering
            '
            resources.ApplyResources(Me.m_lblEntering, "m_lblEntering")
            Me.m_lblEntering.Name = "m_lblEntering"
            '
            'm_cbHideModelBar
            '
            resources.ApplyResources(Me.m_cbHideModelBar, "m_cbHideModelBar")
            Me.m_cbHideModelBar.Name = "m_cbHideModelBar"
            Me.m_cbHideModelBar.UseVisualStyleBackColor = True
            '
            'm_cbHideStatusBar
            '
            resources.ApplyResources(Me.m_cbHideStatusBar, "m_cbHideStatusBar")
            Me.m_cbHideStatusBar.Name = "m_cbHideStatusBar"
            Me.m_cbHideStatusBar.UseVisualStyleBackColor = True
            '
            'm_cbHideMainMenu
            '
            resources.ApplyResources(Me.m_cbHideMainMenu, "m_cbHideMainMenu")
            Me.m_cbHideMainMenu.Name = "m_cbHideMainMenu"
            Me.m_cbHideMainMenu.UseVisualStyleBackColor = True
            '
            'm_cbCollapseNavPanel
            '
            resources.ApplyResources(Me.m_cbCollapseNavPanel, "m_cbCollapseNavPanel")
            Me.m_cbCollapseNavPanel.Name = "m_cbCollapseNavPanel"
            Me.m_cbCollapseNavPanel.UseVisualStyleBackColor = True
            '
            'm_lblUnit2
            '
            resources.ApplyResources(Me.m_lblUnit2, "m_lblUnit2")
            Me.m_lblUnit2.Name = "m_lblUnit2"
            '
            'm_lblUnit1
            '
            resources.ApplyResources(Me.m_lblUnit1, "m_lblUnit1")
            Me.m_lblUnit1.Name = "m_lblUnit1"
            '
            'm_lblWhat
            '
            resources.ApplyResources(Me.m_lblWhat, "m_lblWhat")
            Me.m_lblWhat.Name = "m_lblWhat"
            '
            'm_tbxH
            '
            resources.ApplyResources(Me.m_tbxH, "m_tbxH")
            Me.m_tbxH.Name = "m_tbxH"
            '
            'm_tbxW
            '
            resources.ApplyResources(Me.m_tbxW, "m_tbxW")
            Me.m_tbxW.Name = "m_tbxW"
            '
            'm_lblHeight
            '
            resources.ApplyResources(Me.m_lblHeight, "m_lblHeight")
            Me.m_lblHeight.Name = "m_lblHeight"
            '
            'm_lblWidth
            '
            resources.ApplyResources(Me.m_lblWidth, "m_lblWidth")
            Me.m_lblWidth.Name = "m_lblWidth"
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'm_rbIn
            '
            resources.ApplyResources(Me.m_rbIn, "m_rbIn")
            Me.m_rbIn.Name = "m_rbIn"
            Me.m_rbIn.TabStop = True
            Me.m_rbIn.UseVisualStyleBackColor = True
            '
            'm_rbOut
            '
            resources.ApplyResources(Me.m_rbOut, "m_rbOut")
            Me.m_rbOut.Name = "m_rbOut"
            Me.m_rbOut.TabStop = True
            Me.m_rbOut.UseVisualStyleBackColor = True
            '
            'CEwEHeaderLabel1
            '
            Me.CEwEHeaderLabel1.CanCollapseParent = False
            Me.CEwEHeaderLabel1.CollapsedParentHeight = 0
            resources.ApplyResources(Me.CEwEHeaderLabel1, "CEwEHeaderLabel1")
            Me.CEwEHeaderLabel1.IsCollapsed = False
            Me.CEwEHeaderLabel1.Name = "CEwEHeaderLabel1"
            '
            'ucOptionsPresentation
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_lblUnit2)
            Me.Controls.Add(Me.m_lblUnit1)
            Me.Controls.Add(Me.m_lblWhat)
            Me.Controls.Add(Me.m_tbxH)
            Me.Controls.Add(Me.m_tbxW)
            Me.Controls.Add(Me.m_lblHeight)
            Me.Controls.Add(Me.m_lblWidth)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.m_rbIn)
            Me.Controls.Add(Me.m_rbOut)
            Me.Controls.Add(Me.CEwEHeaderLabel1)
            Me.Controls.Add(Me.m_cbCollapseNavPanel)
            Me.Controls.Add(Me.m_cbHideMainMenu)
            Me.Controls.Add(Me.m_cbHideStatusBar)
            Me.Controls.Add(Me.m_cbHideModelBar)
            Me.Controls.Add(Me.m_lblEntering)
            Me.Controls.Add(Me.m_hdrCaption)
            Me.Name = "ucOptionsPresentation"
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_hdrCaption As cEwEHeaderLabel
        Private WithEvents m_lblEntering As System.Windows.Forms.Label
        Private WithEvents m_cbHideModelBar As System.Windows.Forms.CheckBox
        Private WithEvents m_cbHideStatusBar As System.Windows.Forms.CheckBox
        Private WithEvents m_cbHideMainMenu As System.Windows.Forms.CheckBox
        Private WithEvents m_cbCollapseNavPanel As System.Windows.Forms.CheckBox
        Private WithEvents m_lblUnit2 As Label
        Private WithEvents m_lblUnit1 As Label
        Friend WithEvents m_lblWhat As Label
        Private WithEvents m_tbxH As TextBox
        Private WithEvents m_tbxW As TextBox
        Private WithEvents m_lblHeight As Label
        Private WithEvents m_lblWidth As Label
        Friend WithEvents Label1 As Label
        Friend WithEvents m_rbIn As RadioButton
        Friend WithEvents m_rbOut As RadioButton
        Private WithEvents CEwEHeaderLabel1 As cEwEHeaderLabel
    End Class

End Namespace

