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

Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Forms

Partial Class frmMSEBatchParameters
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
        Me.chkSaveBiomass = New System.Windows.Forms.CheckBox()
        Me.chkCatch = New System.Windows.Forms.CheckBox()
        Me.chkFishingMort = New System.Windows.Forms.CheckBox()
        Me.chkPredMort = New System.Windows.Forms.CheckBox()
        Me.chkQB = New System.Windows.Forms.CheckBox()
        Me.chkFeedingTime = New System.Windows.Forms.CheckBox()
        Me.eweHdrSave = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lbOutputDir = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'chkSaveBiomass
        '
        Me.chkSaveBiomass.AutoSize = True
        Me.chkSaveBiomass.Location = New System.Drawing.Point(17, 62)
        Me.chkSaveBiomass.Name = "chkSaveBiomass"
        Me.chkSaveBiomass.Size = New System.Drawing.Size(65, 17)
        Me.chkSaveBiomass.TabIndex = 0
        Me.chkSaveBiomass.Text = "Biomass"
        Me.chkSaveBiomass.UseVisualStyleBackColor = True
        '
        'chkCatch
        '
        Me.chkCatch.AutoSize = True
        Me.chkCatch.Location = New System.Drawing.Point(17, 85)
        Me.chkCatch.Name = "chkCatch"
        Me.chkCatch.Size = New System.Drawing.Size(54, 17)
        Me.chkCatch.TabIndex = 1
        Me.chkCatch.Text = "Catch"
        Me.chkCatch.UseVisualStyleBackColor = True
        '
        'chkFishingMort
        '
        Me.chkFishingMort.AutoSize = True
        Me.chkFishingMort.Location = New System.Drawing.Point(17, 108)
        Me.chkFishingMort.Name = "chkFishingMort"
        Me.chkFishingMort.Size = New System.Drawing.Size(85, 17)
        Me.chkFishingMort.TabIndex = 2
        Me.chkFishingMort.Text = "Fishing mort."
        Me.chkFishingMort.UseVisualStyleBackColor = True
        '
        'chkPredMort
        '
        Me.chkPredMort.AutoSize = True
        Me.chkPredMort.Location = New System.Drawing.Point(140, 85)
        Me.chkPredMort.Name = "chkPredMort"
        Me.chkPredMort.Size = New System.Drawing.Size(97, 17)
        Me.chkPredMort.TabIndex = 3
        Me.chkPredMort.Text = "Predation mort."
        Me.chkPredMort.UseVisualStyleBackColor = True
        '
        'chkQB
        '
        Me.chkQB.AutoSize = True
        Me.chkQB.Location = New System.Drawing.Point(140, 62)
        Me.chkQB.Name = "chkQB"
        Me.chkQB.Size = New System.Drawing.Size(131, 17)
        Me.chkQB.TabIndex = 4
        Me.chkQB.Text = "Consumption/Biomass"
        Me.chkQB.UseVisualStyleBackColor = True
        '
        'chkFeedingTime
        '
        Me.chkFeedingTime.AutoSize = True
        Me.chkFeedingTime.Location = New System.Drawing.Point(140, 108)
        Me.chkFeedingTime.Name = "chkFeedingTime"
        Me.chkFeedingTime.Size = New System.Drawing.Size(86, 17)
        Me.chkFeedingTime.TabIndex = 5
        Me.chkFeedingTime.Text = "Feeding time"
        Me.chkFeedingTime.UseVisualStyleBackColor = True
        '
        'eweHdrSave
        '
        Me.eweHdrSave.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.eweHdrSave.CanCollapseParent = False
        Me.eweHdrSave.CollapsedParentHeight = 0
        Me.eweHdrSave.IsCollapsed = False
        Me.eweHdrSave.Location = New System.Drawing.Point(17, 9)
        Me.eweHdrSave.Name = "eweHdrSave"
        Me.eweHdrSave.Size = New System.Drawing.Size(446, 24)
        Me.eweHdrSave.TabIndex = 6
        Me.eweHdrSave.Text = "Save"
        Me.eweHdrSave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_lbOutputDir
        '
        Me.m_lbOutputDir.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_lbOutputDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_lbOutputDir.Location = New System.Drawing.Point(105, 37)
        Me.m_lbOutputDir.Name = "m_lbOutputDir"
        Me.m_lbOutputDir.Size = New System.Drawing.Size(358, 15)
        Me.m_lbOutputDir.TabIndex = 7
        Me.m_lbOutputDir.Text = "ouput dir"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(17, 37)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(82, 13)
        Me.Label1.TabIndex = 8
        Me.Label1.Text = "Output directory"
        '
        'frmMSEBatchParameters
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(470, 416)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.m_lbOutputDir)
        Me.Controls.Add(Me.eweHdrSave)
        Me.Controls.Add(Me.chkFeedingTime)
        Me.Controls.Add(Me.chkQB)
        Me.Controls.Add(Me.chkPredMort)
        Me.Controls.Add(Me.chkFishingMort)
        Me.Controls.Add(Me.chkCatch)
        Me.Controls.Add(Me.chkSaveBiomass)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "frmMSEBatchParameters"
        Me.TabText = ""
        Me.Text = "MSE batch parameters"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents chkSaveBiomass As System.Windows.Forms.CheckBox
    Friend WithEvents chkCatch As System.Windows.Forms.CheckBox
    Friend WithEvents chkFishingMort As System.Windows.Forms.CheckBox
    Friend WithEvents chkPredMort As System.Windows.Forms.CheckBox
    Friend WithEvents chkQB As System.Windows.Forms.CheckBox
    Friend WithEvents chkFeedingTime As System.Windows.Forms.CheckBox
    Friend WithEvents eweHdrSave As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Friend WithEvents m_lbOutputDir As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
