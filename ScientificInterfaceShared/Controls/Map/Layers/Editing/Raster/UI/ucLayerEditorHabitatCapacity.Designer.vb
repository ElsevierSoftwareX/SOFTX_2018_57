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

Namespace Controls.Map.Layers

    Partial Class ucLayerEditorHabitatCapacity
        Inherits ucLayerEditorRange

        'UserControl overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
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
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.m_cmbGroups = New System.Windows.Forms.ComboBox()
            Me.m_lblFleet = New System.Windows.Forms.Label()
            Me.m_btnAllDefault = New System.Windows.Forms.Button()
            Me.m_btnLayerDefault = New System.Windows.Forms.Button()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.m_hdDefaults = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tlpButtons = New System.Windows.Forms.TableLayoutPanel()
            Me.m_tlpButtons.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_cmbGroups
            '
            Me.m_cmbGroups.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_cmbGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbGroups.FormattingEnabled = True
            Me.m_cmbGroups.Location = New System.Drawing.Point(70, 207)
            Me.m_cmbGroups.MaxDropDownItems = 12
            Me.m_cmbGroups.Name = "m_cmbGroups"
            Me.m_cmbGroups.Size = New System.Drawing.Size(127, 21)
            Me.m_cmbGroups.TabIndex = 8
            '
            'm_lblFleet
            '
            Me.m_lblFleet.AutoSize = True
            Me.m_lblFleet.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_lblFleet.Location = New System.Drawing.Point(0, 210)
            Me.m_lblFleet.Name = "m_lblFleet"
            Me.m_lblFleet.Size = New System.Drawing.Size(39, 13)
            Me.m_lblFleet.TabIndex = 7
            Me.m_lblFleet.Text = "&Group:"
            '
            'm_btnAllDefault
            '
            Me.m_btnAllDefault.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnAllDefault.Location = New System.Drawing.Point(98, 0)
            Me.m_btnAllDefault.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
            Me.m_btnAllDefault.Name = "m_btnAllDefault"
            Me.m_btnAllDefault.Size = New System.Drawing.Size(93, 23)
            Me.m_btnAllDefault.TabIndex = 12
            Me.m_btnAllDefault.Text = "&All groups"
            Me.m_btnAllDefault.UseVisualStyleBackColor = True
            '
            'm_btnLayerDefault
            '
            Me.m_btnLayerDefault.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnLayerDefault.Location = New System.Drawing.Point(0, 0)
            Me.m_btnLayerDefault.Margin = New System.Windows.Forms.Padding(0, 0, 3, 0)
            Me.m_btnLayerDefault.Name = "m_btnLayerDefault"
            Me.m_btnLayerDefault.Size = New System.Drawing.Size(92, 23)
            Me.m_btnLayerDefault.TabIndex = 11
            Me.m_btnLayerDefault.Text = "&This group"
            Me.m_btnLayerDefault.UseVisualStyleBackColor = True
            '
            'Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Location = New System.Drawing.Point(3, 138)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(0, 13)
            Me.Label1.TabIndex = 10
            '
            'm_hdDefaults
            '
            Me.m_hdDefaults.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdDefaults.CanCollapseParent = False
            Me.m_hdDefaults.CollapsedParentHeight = 0
            Me.m_hdDefaults.IsCollapsed = False
            Me.m_hdDefaults.Location = New System.Drawing.Point(0, 232)
            Me.m_hdDefaults.Name = "m_hdDefaults"
            Me.m_hdDefaults.Size = New System.Drawing.Size(195, 18)
            Me.m_hdDefaults.TabIndex = 9
            Me.m_hdDefaults.Text = "Set default capacity"
            Me.m_hdDefaults.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_tlpButtons
            '
            Me.m_tlpButtons.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_tlpButtons.ColumnCount = 2
            Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpButtons.Controls.Add(Me.m_btnLayerDefault, 0, 0)
            Me.m_tlpButtons.Controls.Add(Me.m_btnAllDefault, 1, 0)
            Me.m_tlpButtons.Location = New System.Drawing.Point(6, 253)
            Me.m_tlpButtons.Name = "m_tlpButtons"
            Me.m_tlpButtons.RowCount = 1
            Me.m_tlpButtons.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.m_tlpButtons.Size = New System.Drawing.Size(191, 23)
            Me.m_tlpButtons.TabIndex = 15
            '
            'ucLayerEditorHabitatCapacity
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_tlpButtons)
            Me.Controls.Add(Me.m_hdDefaults)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.m_cmbGroups)
            Me.Controls.Add(Me.m_lblFleet)
            Me.Name = "ucLayerEditorHabitatCapacity"
            Me.Size = New System.Drawing.Size(203, 281)
            Me.Controls.SetChildIndex(Me.m_lblFleet, 0)
            Me.Controls.SetChildIndex(Me.m_cmbGroups, 0)
            Me.Controls.SetChildIndex(Me.Label1, 0)
            Me.Controls.SetChildIndex(Me.m_hdDefaults, 0)
            Me.Controls.SetChildIndex(Me.m_tlpButtons, 0)
            Me.m_tlpButtons.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_cmbGroups As System.Windows.Forms.ComboBox
        Private WithEvents m_lblFleet As System.Windows.Forms.Label
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Private WithEvents m_hdDefaults As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_btnAllDefault As System.Windows.Forms.Button
        Private WithEvents m_btnLayerDefault As System.Windows.Forms.Button
        Private WithEvents m_tlpButtons As TableLayoutPanel
    End Class

End Namespace
