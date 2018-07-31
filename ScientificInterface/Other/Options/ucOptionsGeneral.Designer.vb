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

    Partial Class ucOptionsGeneral
        Inherits System.Windows.Forms.UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucOptionsGeneral))
            Me.m_btnClearMRU = New System.Windows.Forms.Button()
            Me.m_lblMRU = New System.Windows.Forms.Label()
            Me.m_cbStatusShowTime = New System.Windows.Forms.CheckBox()
            Me.m_lblMaxNumMessages = New System.Windows.Forms.Label()
            Me.m_cbShowHost = New System.Windows.Forms.CheckBox()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.m_cmbLogLevel = New System.Windows.Forms.ComboBox()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.m_tbxAuthor = New System.Windows.Forms.TextBox()
            Me.Label3 = New System.Windows.Forms.Label()
            Me.m_tbxContact = New System.Windows.Forms.TextBox()
            Me.m_nudMaxNumMessages = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_hdrAuthor = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrStatusPanel = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_nudMRU = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_hdrCaption = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_btnViewLogFileDir = New System.Windows.Forms.Button()
            Me.m_cbStatusShowVariableValidations = New System.Windows.Forms.CheckBox()
            Me.m_cbUseExternalBrowser = New System.Windows.Forms.CheckBox()
            Me.m_cbStatusShowNewestFirst = New System.Windows.Forms.CheckBox()
            Me.m_cbStatusAutoPopup = New System.Windows.Forms.CheckBox()
            Me.m_cbShowSplashScreen = New System.Windows.Forms.CheckBox()
            CType(Me.m_nudMaxNumMessages, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.m_nudMRU, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_btnClearMRU
            '
            resources.ApplyResources(Me.m_btnClearMRU, "m_btnClearMRU")
            Me.m_btnClearMRU.Name = "m_btnClearMRU"
            Me.m_btnClearMRU.UseVisualStyleBackColor = True
            '
            'm_lblMRU
            '
            resources.ApplyResources(Me.m_lblMRU, "m_lblMRU")
            Me.m_lblMRU.Name = "m_lblMRU"
            '
            'm_cbStatusShowTime
            '
            resources.ApplyResources(Me.m_cbStatusShowTime, "m_cbStatusShowTime")
            Me.m_cbStatusShowTime.Name = "m_cbStatusShowTime"
            Me.m_cbStatusShowTime.UseVisualStyleBackColor = True
            '
            'm_lblMaxNumMessages
            '
            resources.ApplyResources(Me.m_lblMaxNumMessages, "m_lblMaxNumMessages")
            Me.m_lblMaxNumMessages.Name = "m_lblMaxNumMessages"
            '
            'm_cbShowHost
            '
            resources.ApplyResources(Me.m_cbShowHost, "m_cbShowHost")
            Me.m_cbShowHost.Name = "m_cbShowHost"
            Me.m_cbShowHost.UseVisualStyleBackColor = True
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'm_cmbLogLevel
            '
            Me.m_cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbLogLevel.FormattingEnabled = True
            resources.ApplyResources(Me.m_cmbLogLevel, "m_cmbLogLevel")
            Me.m_cmbLogLevel.Name = "m_cmbLogLevel"
            '
            'Label2
            '
            resources.ApplyResources(Me.Label2, "Label2")
            Me.Label2.Name = "Label2"
            '
            'm_tbxAuthor
            '
            resources.ApplyResources(Me.m_tbxAuthor, "m_tbxAuthor")
            Me.m_tbxAuthor.Name = "m_tbxAuthor"
            '
            'Label3
            '
            resources.ApplyResources(Me.Label3, "Label3")
            Me.Label3.Name = "Label3"
            '
            'm_tbxContact
            '
            resources.ApplyResources(Me.m_tbxContact, "m_tbxContact")
            Me.m_tbxContact.Name = "m_tbxContact"
            '
            'm_nudMaxNumMessages
            '
            Me.m_nudMaxNumMessages.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudMaxNumMessages, "m_nudMaxNumMessages")
            Me.m_nudMaxNumMessages.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
            Me.m_nudMaxNumMessages.Name = "m_nudMaxNumMessages"
            Me.m_nudMaxNumMessages.Value = New Decimal(New Integer() {10, 0, 0, 0})
            '
            'm_hdrAuthor
            '
            resources.ApplyResources(Me.m_hdrAuthor, "m_hdrAuthor")
            Me.m_hdrAuthor.CanCollapseParent = False
            Me.m_hdrAuthor.CollapsedParentHeight = 0
            Me.m_hdrAuthor.IsCollapsed = False
            Me.m_hdrAuthor.Name = "m_hdrAuthor"
            '
            'm_hdrStatusPanel
            '
            resources.ApplyResources(Me.m_hdrStatusPanel, "m_hdrStatusPanel")
            Me.m_hdrStatusPanel.CanCollapseParent = False
            Me.m_hdrStatusPanel.CollapsedParentHeight = 0
            Me.m_hdrStatusPanel.IsCollapsed = False
            Me.m_hdrStatusPanel.Name = "m_hdrStatusPanel"
            '
            'm_nudMRU
            '
            Me.m_nudMRU.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudMRU, "m_nudMRU")
            Me.m_nudMRU.Maximum = New Decimal(New Integer() {42, 0, 0, 0})
            Me.m_nudMRU.Name = "m_nudMRU"
            Me.m_nudMRU.Value = New Decimal(New Integer() {10, 0, 0, 0})
            '
            'm_hdrCaption
            '
            Me.m_hdrCaption.CanCollapseParent = False
            Me.m_hdrCaption.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrCaption, "m_hdrCaption")
            Me.m_hdrCaption.IsCollapsed = False
            Me.m_hdrCaption.Name = "m_hdrCaption"
            '
            'm_btnViewLogFileDir
            '
            resources.ApplyResources(Me.m_btnViewLogFileDir, "m_btnViewLogFileDir")
            Me.m_btnViewLogFileDir.Name = "m_btnViewLogFileDir"
            Me.m_btnViewLogFileDir.UseVisualStyleBackColor = True
            '
            'm_cbStatusShowVariableValidations
            '
            resources.ApplyResources(Me.m_cbStatusShowVariableValidations, "m_cbStatusShowVariableValidations")
            Me.m_cbStatusShowVariableValidations.Name = "m_cbStatusShowVariableValidations"
            Me.m_cbStatusShowVariableValidations.UseVisualStyleBackColor = True
            '
            'm_cbUseExternalBrowser
            '
            resources.ApplyResources(Me.m_cbUseExternalBrowser, "m_cbUseExternalBrowser")
            Me.m_cbUseExternalBrowser.Name = "m_cbUseExternalBrowser"
            Me.m_cbUseExternalBrowser.UseVisualStyleBackColor = True
            '
            'm_cbStatusShowNewestFirst
            '
            resources.ApplyResources(Me.m_cbStatusShowNewestFirst, "m_cbStatusShowNewestFirst")
            Me.m_cbStatusShowNewestFirst.Name = "m_cbStatusShowNewestFirst"
            Me.m_cbStatusShowNewestFirst.UseVisualStyleBackColor = True
            '
            'm_cbStatusAutoPopup
            '
            resources.ApplyResources(Me.m_cbStatusAutoPopup, "m_cbStatusAutoPopup")
            Me.m_cbStatusAutoPopup.Name = "m_cbStatusAutoPopup"
            Me.m_cbStatusAutoPopup.UseVisualStyleBackColor = True
            '
            'm_cbShowSplashScreen
            '
            resources.ApplyResources(Me.m_cbShowSplashScreen, "m_cbShowSplashScreen")
            Me.m_cbShowSplashScreen.Name = "m_cbShowSplashScreen"
            Me.m_cbShowSplashScreen.UseVisualStyleBackColor = True
            '
            'ucOptionsGeneral
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_tbxContact)
            Me.Controls.Add(Me.m_tbxAuthor)
            Me.Controls.Add(Me.m_cmbLogLevel)
            Me.Controls.Add(Me.m_nudMaxNumMessages)
            Me.Controls.Add(Me.m_cbStatusShowVariableValidations)
            Me.Controls.Add(Me.m_cbStatusShowNewestFirst)
            Me.Controls.Add(Me.m_cbStatusAutoPopup)
            Me.Controls.Add(Me.m_cbStatusShowTime)
            Me.Controls.Add(Me.m_lblMaxNumMessages)
            Me.Controls.Add(Me.m_hdrAuthor)
            Me.Controls.Add(Me.m_hdrStatusPanel)
            Me.Controls.Add(Me.m_cbShowSplashScreen)
            Me.Controls.Add(Me.m_cbUseExternalBrowser)
            Me.Controls.Add(Me.m_cbShowHost)
            Me.Controls.Add(Me.m_btnViewLogFileDir)
            Me.Controls.Add(Me.m_btnClearMRU)
            Me.Controls.Add(Me.m_nudMRU)
            Me.Controls.Add(Me.Label3)
            Me.Controls.Add(Me.m_hdrCaption)
            Me.Controls.Add(Me.Label2)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.m_lblMRU)
            Me.Name = "ucOptionsGeneral"
            CType(Me.m_nudMaxNumMessages, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.m_nudMRU, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_lblMRU As System.Windows.Forms.Label
        Private WithEvents m_lblMaxNumMessages As System.Windows.Forms.Label
        Private WithEvents m_btnClearMRU As System.Windows.Forms.Button
        Private WithEvents m_hdrCaption As cEwEHeaderLabel
        Private WithEvents m_cbStatusShowTime As System.Windows.Forms.CheckBox
        Private WithEvents m_nudMaxNumMessages As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_nudMRU As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_cbShowHost As System.Windows.Forms.CheckBox
        Private WithEvents m_hdrStatusPanel As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents Label1 As System.Windows.Forms.Label
        Private WithEvents Label2 As System.Windows.Forms.Label
        Private WithEvents m_hdrAuthor As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents Label3 As System.Windows.Forms.Label
        Private WithEvents m_cmbLogLevel As System.Windows.Forms.ComboBox
        Private WithEvents m_tbxContact As System.Windows.Forms.TextBox
        Private WithEvents m_tbxAuthor As System.Windows.Forms.TextBox
        Private WithEvents m_btnViewLogFileDir As System.Windows.Forms.Button
        Private WithEvents m_cbStatusShowVariableValidations As System.Windows.Forms.CheckBox
        Private WithEvents m_cbUseExternalBrowser As CheckBox
        Private WithEvents m_cbStatusShowNewestFirst As CheckBox
        Private WithEvents m_cbStatusAutoPopup As CheckBox
        Private WithEvents m_cbShowSplashScreen As CheckBox
    End Class

End Namespace

