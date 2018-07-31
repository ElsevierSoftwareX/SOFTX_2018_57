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

    Partial Class ucSketchPadToolbar
        Inherits System.Windows.Forms.UserControl

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
            Dim ts1 As System.Windows.Forms.ToolStripSeparator
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucSketchPadToolbar))
            Dim ts4 As System.Windows.Forms.ToolStripSeparator
            Dim ts2 As System.Windows.Forms.ToolStripSeparator
            Me.m_tsMenus = New ScientificInterfaceShared.Controls.cEwEToolstrip()
            Me.m_tsbnSaveAsImage = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnLongTerm = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnSeasonal = New System.Windows.Forms.ToolStripButton()
            Me.m_tslMaxValue = New System.Windows.Forms.ToolStripLabel()
            Me.m_tstbMaxValue = New System.Windows.Forms.ToolStripTextBox()
            Me.m_tslWeight = New System.Windows.Forms.ToolStripLabel()
            Me.m_tstbWeight = New System.Windows.Forms.ToolStripTextBox()
            Me.m_tsbnValues = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnChangeShape = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnReset = New System.Windows.Forms.ToolStripButton()
            Me.m_tsbnOptions = New System.Windows.Forms.ToolStripButton()
            ts1 = New System.Windows.Forms.ToolStripSeparator()
            ts4 = New System.Windows.Forms.ToolStripSeparator()
            ts2 = New System.Windows.Forms.ToolStripSeparator()
            Me.m_tsMenus.SuspendLayout()
            Me.SuspendLayout()
            '
            'ts1
            '
            ts1.Name = "ts1"
            resources.ApplyResources(ts1, "ts1")
            '
            'ts4
            '
            ts4.Name = "ts4"
            resources.ApplyResources(ts4, "ts4")
            '
            'ts2
            '
            ts2.Name = "ts2"
            resources.ApplyResources(ts2, "ts2")
            '
            'm_tsMenus
            '
            resources.ApplyResources(Me.m_tsMenus, "m_tsMenus")
            Me.m_tsMenus.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
            Me.m_tsMenus.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnSaveAsImage, ts1, Me.m_tsbnLongTerm, Me.m_tsbnSeasonal, ts2, Me.m_tslMaxValue, Me.m_tstbMaxValue, Me.m_tslWeight, Me.m_tstbWeight, Me.m_tsbnValues, Me.m_tsbnChangeShape, Me.m_tsbnReset, ts4, Me.m_tsbnOptions})
            Me.m_tsMenus.Name = "m_tsMenus"
            Me.m_tsMenus.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
            '
            'm_tsbnSaveAsImage
            '
            Me.m_tsbnSaveAsImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnSaveAsImage, "m_tsbnSaveAsImage")
            Me.m_tsbnSaveAsImage.Name = "m_tsbnSaveAsImage"
            '
            'm_tsbnLongTerm
            '
            resources.ApplyResources(Me.m_tsbnLongTerm, "m_tsbnLongTerm")
            Me.m_tsbnLongTerm.Name = "m_tsbnLongTerm"
            '
            'm_tsbnSeasonal
            '
            resources.ApplyResources(Me.m_tsbnSeasonal, "m_tsbnSeasonal")
            Me.m_tsbnSeasonal.Name = "m_tsbnSeasonal"
            '
            'm_tslMaxValue
            '
            Me.m_tslMaxValue.Name = "m_tslMaxValue"
            resources.ApplyResources(Me.m_tslMaxValue, "m_tslMaxValue")
            '
            'm_tstbMaxValue
            '
            Me.m_tstbMaxValue.Name = "m_tstbMaxValue"
            resources.ApplyResources(Me.m_tstbMaxValue, "m_tstbMaxValue")
            '
            'm_tslWeight
            '
            Me.m_tslWeight.Name = "m_tslWeight"
            resources.ApplyResources(Me.m_tslWeight, "m_tslWeight")
            '
            'm_tstbWeight
            '
            Me.m_tstbWeight.AcceptsReturn = True
            resources.ApplyResources(Me.m_tstbWeight, "m_tstbWeight")
            Me.m_tstbWeight.Name = "m_tstbWeight"
            '
            'm_tsbnValues
            '
            Me.m_tsbnValues.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnValues, "m_tsbnValues")
            Me.m_tsbnValues.Name = "m_tsbnValues"
            '
            'm_tsbnChangeShape
            '
            Me.m_tsbnChangeShape.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnChangeShape, "m_tsbnChangeShape")
            Me.m_tsbnChangeShape.Name = "m_tsbnChangeShape"
            '
            'm_tsbnReset
            '
            Me.m_tsbnReset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnReset, "m_tsbnReset")
            Me.m_tsbnReset.Name = "m_tsbnReset"
            '
            'm_tsbnOptions
            '
            Me.m_tsbnOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
            resources.ApplyResources(Me.m_tsbnOptions, "m_tsbnOptions")
            Me.m_tsbnOptions.Name = "m_tsbnOptions"
            '
            'ucSketchPadToolbar
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.BackColor = System.Drawing.SystemColors.Control
            Me.Controls.Add(Me.m_tsMenus)
            Me.Name = "ucSketchPadToolbar"
            Me.m_tsMenus.ResumeLayout(False)
            Me.m_tsMenus.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_tsMenus As cEwEToolstrip
        Private WithEvents m_tsbnReset As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnValues As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnSaveAsImage As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnOptions As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnChangeShape As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tslWeight As System.Windows.Forms.ToolStripLabel
        Private WithEvents m_tstbWeight As System.Windows.Forms.ToolStripTextBox
        Private WithEvents m_tsbnLongTerm As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tsbnSeasonal As System.Windows.Forms.ToolStripButton
        Private WithEvents m_tslMaxValue As System.Windows.Forms.ToolStripLabel
        Private WithEvents m_tstbMaxValue As System.Windows.Forms.ToolStripTextBox

    End Class

End Namespace
