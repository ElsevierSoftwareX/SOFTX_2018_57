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

Namespace Ecospace

    Partial Class frmEcospaceResults
        Inherits frmEwE

        'Form overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEcospaceResults))
            Me.m_plResultsGrid = New System.Windows.Forms.Panel()
            Me.m_nudSumLength = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            Me.m_tbSumEndTime = New System.Windows.Forms.TextBox()
            Me.m_tbSumStartTime = New System.Windows.Forms.TextBox()
            Me.m_lblSumStartTime = New System.Windows.Forms.Label()
            Me.m_lblSumEndTime = New System.Windows.Forms.Label()
            Me.m_cmbRegions = New System.Windows.Forms.ComboBox()
            Me.m_cmbGears = New System.Windows.Forms.ComboBox()
            Me.m_rbGroup = New System.Windows.Forms.RadioButton()
            Me.m_rbRegion = New System.Windows.Forms.RadioButton()
            Me.m_rbFleet = New System.Windows.Forms.RadioButton()
            Me.m_hdrSummary = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrShow = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lblNumSteps = New System.Windows.Forms.Label()
            CType(Me.m_nudSumLength, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_plResultsGrid
            '
            resources.ApplyResources(Me.m_plResultsGrid, "m_plResultsGrid")
            Me.m_plResultsGrid.Name = "m_plResultsGrid"
            '
            'm_nudSumLength
            '
            Me.m_nudSumLength.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudSumLength, "m_nudSumLength")
            Me.m_nudSumLength.Name = "m_nudSumLength"
            '
            'm_tbSumEndTime
            '
            resources.ApplyResources(Me.m_tbSumEndTime, "m_tbSumEndTime")
            Me.m_tbSumEndTime.Name = "m_tbSumEndTime"
            '
            'm_tbSumStartTime
            '
            resources.ApplyResources(Me.m_tbSumStartTime, "m_tbSumStartTime")
            Me.m_tbSumStartTime.Name = "m_tbSumStartTime"
            '
            'm_lblSumStartTime
            '
            resources.ApplyResources(Me.m_lblSumStartTime, "m_lblSumStartTime")
            Me.m_lblSumStartTime.Name = "m_lblSumStartTime"
            '
            'm_lblSumEndTime
            '
            resources.ApplyResources(Me.m_lblSumEndTime, "m_lblSumEndTime")
            Me.m_lblSumEndTime.Name = "m_lblSumEndTime"
            '
            'm_cmbRegions
            '
            Me.m_cmbRegions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbRegions.FormattingEnabled = True
            resources.ApplyResources(Me.m_cmbRegions, "m_cmbRegions")
            Me.m_cmbRegions.Name = "m_cmbRegions"
            '
            'm_cmbGears
            '
            Me.m_cmbGears.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbGears.FormattingEnabled = True
            resources.ApplyResources(Me.m_cmbGears, "m_cmbGears")
            Me.m_cmbGears.Name = "m_cmbGears"
            '
            'm_rbGroup
            '
            resources.ApplyResources(Me.m_rbGroup, "m_rbGroup")
            Me.m_rbGroup.Name = "m_rbGroup"
            Me.m_rbGroup.TabStop = True
            Me.m_rbGroup.UseVisualStyleBackColor = True
            '
            'm_rbRegion
            '
            resources.ApplyResources(Me.m_rbRegion, "m_rbRegion")
            Me.m_rbRegion.Name = "m_rbRegion"
            Me.m_rbRegion.TabStop = True
            Me.m_rbRegion.UseVisualStyleBackColor = True
            '
            'm_rbFleet
            '
            resources.ApplyResources(Me.m_rbFleet, "m_rbFleet")
            Me.m_rbFleet.Name = "m_rbFleet"
            Me.m_rbFleet.TabStop = True
            Me.m_rbFleet.UseVisualStyleBackColor = True
            '
            'm_hdrSummary
            '
            Me.m_hdrSummary.CanCollapseParent = False
            Me.m_hdrSummary.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrSummary, "m_hdrSummary")
            Me.m_hdrSummary.IsCollapsed = False
            Me.m_hdrSummary.Name = "m_hdrSummary"
            '
            'm_hdrShow
            '
            resources.ApplyResources(Me.m_hdrShow, "m_hdrShow")
            Me.m_hdrShow.CanCollapseParent = False
            Me.m_hdrShow.CollapsedParentHeight = 0
            Me.m_hdrShow.IsCollapsed = False
            Me.m_hdrShow.Name = "m_hdrShow"
            '
            'm_lblNumSteps
            '
            resources.ApplyResources(Me.m_lblNumSteps, "m_lblNumSteps")
            Me.m_lblNumSteps.Name = "m_lblNumSteps"
            '
            'frmEcospaceResults
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_lblNumSteps)
            Me.Controls.Add(Me.m_hdrShow)
            Me.Controls.Add(Me.m_hdrSummary)
            Me.Controls.Add(Me.m_cmbRegions)
            Me.Controls.Add(Me.m_cmbGears)
            Me.Controls.Add(Me.m_rbGroup)
            Me.Controls.Add(Me.m_rbRegion)
            Me.Controls.Add(Me.m_rbFleet)
            Me.Controls.Add(Me.m_nudSumLength)
            Me.Controls.Add(Me.m_tbSumEndTime)
            Me.Controls.Add(Me.m_tbSumStartTime)
            Me.Controls.Add(Me.m_lblSumStartTime)
            Me.Controls.Add(Me.m_lblSumEndTime)
            Me.Controls.Add(Me.m_plResultsGrid)
            Me.Name = "frmEcospaceResults"
            Me.TabText = ""
            CType(Me.m_nudSumLength, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_plResultsGrid As System.Windows.Forms.Panel
        Private WithEvents m_tbSumEndTime As System.Windows.Forms.TextBox
        Private WithEvents m_lblSumStartTime As System.Windows.Forms.Label
        Private WithEvents m_lblSumEndTime As System.Windows.Forms.Label
        Private WithEvents m_cmbRegions As System.Windows.Forms.ComboBox
        Private WithEvents m_cmbGears As System.Windows.Forms.ComboBox
        Private WithEvents m_rbGroup As System.Windows.Forms.RadioButton
        Private WithEvents m_hdrSummary As cEwEHeaderLabel
        Private WithEvents m_hdrShow As cEwEHeaderLabel
        Private WithEvents m_lblNumSteps As System.Windows.Forms.Label
        Private WithEvents m_rbFleet As System.Windows.Forms.RadioButton
        Private WithEvents m_tbSumStartTime As System.Windows.Forms.TextBox
        Private WithEvents m_rbRegion As System.Windows.Forms.RadioButton
        Private WithEvents m_nudSumLength As ScientificInterfaceShared.Controls.cEwENumericUpDown
    End Class

End Namespace

