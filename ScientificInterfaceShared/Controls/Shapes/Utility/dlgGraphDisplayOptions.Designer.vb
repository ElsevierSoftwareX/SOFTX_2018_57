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


    Partial Class dlgGraphDisplayOptions
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgGraphDisplayOptions))
            Me.m_rbLine = New System.Windows.Forms.RadioButton()
            Me.m_rbFill = New System.Windows.Forms.RadioButton()
            Me.m_cbRightClickAutoScale = New System.Windows.Forms.CheckBox()
            Me.m_nudMax = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_cbAutoScale = New System.Windows.Forms.CheckBox()
            Me.m_lblYMax = New System.Windows.Forms.Label()
            Me.m_cbShowScaleAndTitle = New System.Windows.Forms.CheckBox()
            Me.m_hdrShow = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.OK_Button = New System.Windows.Forms.Button()
            Me.Cancel_Button = New System.Windows.Forms.Button()
            Me.m_hdrDrawAs = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrScaling = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_rbTSDriver = New System.Windows.Forms.RadioButton()
            Me.m_rbTSRefAbs = New System.Windows.Forms.RadioButton()
            Me.m_rbTSRefRel = New System.Windows.Forms.RadioButton()
            CType(Me.m_nudMax, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_rbLine
            '
            resources.ApplyResources(Me.m_rbLine, "m_rbLine")
            Me.m_rbLine.Name = "m_rbLine"
            Me.m_rbLine.TabStop = True
            Me.m_rbLine.UseVisualStyleBackColor = True
            '
            'm_rbFill
            '
            resources.ApplyResources(Me.m_rbFill, "m_rbFill")
            Me.m_rbFill.Name = "m_rbFill"
            Me.m_rbFill.TabStop = True
            Me.m_rbFill.UseVisualStyleBackColor = True
            '
            'm_cbRightClickAutoScale
            '
            resources.ApplyResources(Me.m_cbRightClickAutoScale, "m_cbRightClickAutoScale")
            Me.m_cbRightClickAutoScale.Name = "m_cbRightClickAutoScale"
            Me.m_cbRightClickAutoScale.UseVisualStyleBackColor = True
            '
            'm_nudMax
            '
            Me.m_nudMax.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudMax, "m_nudMax")
            Me.m_nudMax.Name = "m_nudMax"
            '
            'm_cbAutoScale
            '
            resources.ApplyResources(Me.m_cbAutoScale, "m_cbAutoScale")
            Me.m_cbAutoScale.Name = "m_cbAutoScale"
            Me.m_cbAutoScale.UseVisualStyleBackColor = True
            '
            'm_lblYMax
            '
            resources.ApplyResources(Me.m_lblYMax, "m_lblYMax")
            Me.m_lblYMax.Name = "m_lblYMax"
            '
            'm_cbShowScaleAndTitle
            '
            resources.ApplyResources(Me.m_cbShowScaleAndTitle, "m_cbShowScaleAndTitle")
            Me.m_cbShowScaleAndTitle.Name = "m_cbShowScaleAndTitle"
            Me.m_cbShowScaleAndTitle.UseVisualStyleBackColor = True
            '
            'm_hdrShow
            '
            resources.ApplyResources(Me.m_hdrShow, "m_hdrShow")
            Me.m_hdrShow.CanCollapseParent = False
            Me.m_hdrShow.CollapsedParentHeight = 0
            Me.m_hdrShow.IsCollapsed = False
            Me.m_hdrShow.Name = "m_hdrShow"
            '
            'OK_Button
            '
            resources.ApplyResources(Me.OK_Button, "OK_Button")
            Me.OK_Button.Name = "OK_Button"
            '
            'Cancel_Button
            '
            resources.ApplyResources(Me.Cancel_Button, "Cancel_Button")
            Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Cancel_Button.Name = "Cancel_Button"
            '
            'm_hdrDrawAs
            '
            resources.ApplyResources(Me.m_hdrDrawAs, "m_hdrDrawAs")
            Me.m_hdrDrawAs.CanCollapseParent = False
            Me.m_hdrDrawAs.CollapsedParentHeight = 0
            Me.m_hdrDrawAs.IsCollapsed = False
            Me.m_hdrDrawAs.Name = "m_hdrDrawAs"
            '
            'm_hdrScaling
            '
            resources.ApplyResources(Me.m_hdrScaling, "m_hdrScaling")
            Me.m_hdrScaling.CanCollapseParent = False
            Me.m_hdrScaling.CollapsedParentHeight = 0
            Me.m_hdrScaling.IsCollapsed = False
            Me.m_hdrScaling.Name = "m_hdrScaling"
            '
            'm_rbTSDriver
            '
            resources.ApplyResources(Me.m_rbTSDriver, "m_rbTSDriver")
            Me.m_rbTSDriver.Name = "m_rbTSDriver"
            Me.m_rbTSDriver.TabStop = True
            Me.m_rbTSDriver.UseVisualStyleBackColor = True
            '
            'm_rbTSRefAbs
            '
            resources.ApplyResources(Me.m_rbTSRefAbs, "m_rbTSRefAbs")
            Me.m_rbTSRefAbs.Name = "m_rbTSRefAbs"
            Me.m_rbTSRefAbs.TabStop = True
            Me.m_rbTSRefAbs.UseVisualStyleBackColor = True
            '
            'm_rbTSRefRel
            '
            resources.ApplyResources(Me.m_rbTSRefRel, "m_rbTSRefRel")
            Me.m_rbTSRefRel.Name = "m_rbTSRefRel"
            Me.m_rbTSRefRel.TabStop = True
            Me.m_rbTSRefRel.UseVisualStyleBackColor = True
            '
            'dlgGraphDisplayOptions
            '
            Me.AcceptButton = Me.OK_Button
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.Cancel_Button
            Me.Controls.Add(Me.Cancel_Button)
            Me.Controls.Add(Me.OK_Button)
            Me.Controls.Add(Me.m_hdrScaling)
            Me.Controls.Add(Me.m_hdrDrawAs)
            Me.Controls.Add(Me.m_hdrShow)
            Me.Controls.Add(Me.m_nudMax)
            Me.Controls.Add(Me.m_lblYMax)
            Me.Controls.Add(Me.m_cbRightClickAutoScale)
            Me.Controls.Add(Me.m_cbAutoScale)
            Me.Controls.Add(Me.m_rbTSRefRel)
            Me.Controls.Add(Me.m_rbTSRefAbs)
            Me.Controls.Add(Me.m_rbTSDriver)
            Me.Controls.Add(Me.m_rbLine)
            Me.Controls.Add(Me.m_rbFill)
            Me.Controls.Add(Me.m_cbShowScaleAndTitle)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgGraphDisplayOptions"
            CType(Me.m_nudMax, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_rbLine As System.Windows.Forms.RadioButton
        Private WithEvents m_rbFill As System.Windows.Forms.RadioButton
        Private WithEvents m_cbShowScaleAndTitle As System.Windows.Forms.CheckBox
        Private WithEvents m_hdrShow As cEwEHeaderLabel
        Private WithEvents OK_Button As System.Windows.Forms.Button
        Private WithEvents Cancel_Button As System.Windows.Forms.Button
        Private WithEvents m_rbTSDriver As System.Windows.Forms.RadioButton
        Private WithEvents m_hdrDrawAs As cEwEHeaderLabel
        Private WithEvents m_hdrScaling As cEwEHeaderLabel
        Private WithEvents m_cbAutoScale As System.Windows.Forms.CheckBox
        Private WithEvents m_cbRightClickAutoScale As System.Windows.Forms.CheckBox
        Private WithEvents m_lblYMax As System.Windows.Forms.Label
        Private WithEvents m_nudMax As ScientificInterfaceShared.Controls.cEwENumericUpDown
        Private WithEvents m_rbTSRefAbs As RadioButton
        Private WithEvents m_rbTSRefRel As RadioButton
    End Class

End Namespace

