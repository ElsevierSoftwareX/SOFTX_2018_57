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

Namespace Controls


    Partial Class dlgChangeShape
        Inherits System.Windows.Forms.Form

        'UserControl overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
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
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgChangeShape))
            Me.m_btnOk = New System.Windows.Forms.Button()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_plPreview = New System.Windows.Forms.Panel()
            Me.m_btDefaults = New System.Windows.Forms.Button()
            Me.m_tbxName = New System.Windows.Forms.TextBox()
            Me.m_lblName = New System.Windows.Forms.Label()
            Me.m_scMain = New System.Windows.Forms.SplitContainer()
            Me.m_controlpanel = New ScientificInterfaceShared.Controls.ucChangeShapeType()
            Me.m_cbShowExtraData = New System.Windows.Forms.CheckBox()
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scMain.Panel1.SuspendLayout()
            Me.m_scMain.Panel2.SuspendLayout()
            Me.m_scMain.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_btnOk
            '
            resources.ApplyResources(Me.m_btnOk, "m_btnOk")
            Me.m_btnOk.Name = "m_btnOk"
            Me.m_btnOk.UseVisualStyleBackColor = True
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            Me.m_btnCancel.UseVisualStyleBackColor = True
            '
            'm_plPreview
            '
            Me.m_plPreview.BackColor = System.Drawing.SystemColors.Window
            Me.m_plPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            resources.ApplyResources(Me.m_plPreview, "m_plPreview")
            Me.m_plPreview.Name = "m_plPreview"
            '
            'm_btDefaults
            '
            resources.ApplyResources(Me.m_btDefaults, "m_btDefaults")
            Me.m_btDefaults.Name = "m_btDefaults"
            Me.m_btDefaults.UseVisualStyleBackColor = True
            '
            'm_tbxName
            '
            resources.ApplyResources(Me.m_tbxName, "m_tbxName")
            Me.m_tbxName.Name = "m_tbxName"
            '
            'm_lblName
            '
            resources.ApplyResources(Me.m_lblName, "m_lblName")
            Me.m_lblName.Name = "m_lblName"
            '
            'm_scMain
            '
            resources.ApplyResources(Me.m_scMain, "m_scMain")
            Me.m_scMain.Name = "m_scMain"
            '
            'm_scMain.Panel1
            '
            Me.m_scMain.Panel1.Controls.Add(Me.m_controlpanel)
            Me.m_scMain.Panel1.Controls.Add(Me.m_lblName)
            Me.m_scMain.Panel1.Controls.Add(Me.m_tbxName)
            '
            'm_scMain.Panel2
            '
            Me.m_scMain.Panel2.Controls.Add(Me.m_plPreview)
            '
            'm_controlpanel
            '
            resources.ApplyResources(Me.m_controlpanel, "m_controlpanel")
            Me.m_controlpanel.Name = "m_controlpanel"
            Me.m_controlpanel.Shape = Nothing
            Me.m_controlpanel.UIContext = Nothing
            '
            'm_cbShowExtraData
            '
            resources.ApplyResources(Me.m_cbShowExtraData, "m_cbShowExtraData")
            Me.m_cbShowExtraData.Name = "m_cbShowExtraData"
            Me.m_cbShowExtraData.UseVisualStyleBackColor = True
            '
            'dlgChangeShape
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnCancel
            Me.ControlBox = False
            Me.Controls.Add(Me.m_cbShowExtraData)
            Me.Controls.Add(Me.m_scMain)
            Me.Controls.Add(Me.m_btDefaults)
            Me.Controls.Add(Me.m_btnOk)
            Me.Controls.Add(Me.m_btnCancel)
            Me.Name = "dlgChangeShape"
            Me.ShowInTaskbar = False
            Me.m_scMain.Panel1.ResumeLayout(False)
            Me.m_scMain.Panel1.PerformLayout()
            Me.m_scMain.Panel2.ResumeLayout(False)
            CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scMain.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private WithEvents m_btnOk As System.Windows.Forms.Button
        Private WithEvents m_btnCancel As System.Windows.Forms.Button
        Private WithEvents m_plPreview As System.Windows.Forms.Panel
        Private WithEvents m_btDefaults As System.Windows.Forms.Button
        Private WithEvents m_tbxName As System.Windows.Forms.TextBox
        Private WithEvents m_lblName As System.Windows.Forms.Label
        Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
        Private WithEvents m_controlpanel As ucChangeShapeType
        Private WithEvents m_cbShowExtraData As CheckBox
    End Class

End Namespace

