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

Imports System.Windows.Forms

<CLSCompliant(False)> _
Public Class ucPlotOfMTIOptions
    Inherits usercontrol

    Private WithEvents m_hdrOptions As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_rbCircles As System.Windows.Forms.RadioButton
    Private WithEvents m_rbRectangles As System.Windows.Forms.RadioButton
    Private WithEvents m_cbShowGrid As System.Windows.Forms.CheckBox
    Private WithEvents m_cbSlantingLabels As System.Windows.Forms.CheckBox
    Private WithEvents m_cbFitToScreen As System.Windows.Forms.CheckBox
    Private WithEvents m_tlpOptions As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_plLabels As System.Windows.Forms.Panel
    Private WithEvents m_rbShowName As System.Windows.Forms.RadioButton
    Private WithEvents m_rbShowNo As System.Windows.Forms.RadioButton
    Private WithEvents m_rbShowNameNum As System.Windows.Forms.RadioButton
    Private WithEvents m_plData As System.Windows.Forms.Panel
    Private WithEvents m_hdrData As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_plGraph As System.Windows.Forms.Panel
    Private WithEvents m_hdrPlot As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_cbShowLegend As System.Windows.Forms.CheckBox
    Private WithEvents m_rbColors As System.Windows.Forms.RadioButton
    Private m_content As cPlotOfMixedTrophicImpact = Nothing

    Public Sub New(ByVal content As cPlotOfMixedTrophicImpact)
        Me.InitializeComponent()
        Me.m_content = content
    End Sub

    Private Sub InitializeComponent()
        Me.m_hdrOptions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_rbCircles = New System.Windows.Forms.RadioButton()
        Me.m_rbRectangles = New System.Windows.Forms.RadioButton()
        Me.m_cbShowGrid = New System.Windows.Forms.CheckBox()
        Me.m_cbSlantingLabels = New System.Windows.Forms.CheckBox()
        Me.m_cbFitToScreen = New System.Windows.Forms.CheckBox()
        Me.m_tlpOptions = New System.Windows.Forms.TableLayoutPanel()
        Me.m_plLabels = New System.Windows.Forms.Panel()
        Me.m_rbShowName = New System.Windows.Forms.RadioButton()
        Me.m_rbShowNo = New System.Windows.Forms.RadioButton()
        Me.m_rbShowNameNum = New System.Windows.Forms.RadioButton()
        Me.m_plData = New System.Windows.Forms.Panel()
        Me.m_hdrData = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_plGraph = New System.Windows.Forms.Panel()
        Me.m_hdrPlot = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_cbShowLegend = New System.Windows.Forms.CheckBox()
        Me.m_rbColors = New System.Windows.Forms.RadioButton()
        Me.m_tlpOptions.SuspendLayout()
        Me.m_plLabels.SuspendLayout()
        Me.m_plData.SuspendLayout()
        Me.m_plGraph.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_hdrOptions
        '
        Me.m_hdrOptions.CanCollapseParent = False
        Me.m_hdrOptions.CollapsedParentHeight = 0
        Me.m_hdrOptions.Dock = System.Windows.Forms.DockStyle.Top
        Me.m_hdrOptions.IsCollapsed = False
        Me.m_hdrOptions.Location = New System.Drawing.Point(0, 0)
        Me.m_hdrOptions.Margin = New System.Windows.Forms.Padding(0)
        Me.m_hdrOptions.Name = "m_hdrOptions"
        Me.m_hdrOptions.Size = New System.Drawing.Size(134, 18)
        Me.m_hdrOptions.TabIndex = 0
        Me.m_hdrOptions.Text = "Labels"
        Me.m_hdrOptions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_rbCircles
        '
        Me.m_rbCircles.AutoSize = True
        Me.m_rbCircles.Checked = True
        Me.m_rbCircles.Location = New System.Drawing.Point(3, 21)
        Me.m_rbCircles.Name = "m_rbCircles"
        Me.m_rbCircles.Size = New System.Drawing.Size(56, 17)
        Me.m_rbCircles.TabIndex = 1
        Me.m_rbCircles.TabStop = True
        Me.m_rbCircles.Text = "&Circles"
        Me.m_rbCircles.UseVisualStyleBackColor = True
        '
        'm_rbRectangles
        '
        Me.m_rbRectangles.AutoSize = True
        Me.m_rbRectangles.Location = New System.Drawing.Point(3, 44)
        Me.m_rbRectangles.Name = "m_rbRectangles"
        Me.m_rbRectangles.Size = New System.Drawing.Size(79, 17)
        Me.m_rbRectangles.TabIndex = 2
        Me.m_rbRectangles.TabStop = True
        Me.m_rbRectangles.Text = "&Rectangles"
        Me.m_rbRectangles.UseVisualStyleBackColor = True
        '
        'm_cbShowGrid
        '
        Me.m_cbShowGrid.AutoSize = True
        Me.m_cbShowGrid.Location = New System.Drawing.Point(3, 21)
        Me.m_cbShowGrid.Name = "m_cbShowGrid"
        Me.m_cbShowGrid.Size = New System.Drawing.Size(95, 17)
        Me.m_cbShowGrid.TabIndex = 1
        Me.m_cbShowGrid.Text = "Draw &grid lines"
        Me.m_cbShowGrid.UseVisualStyleBackColor = True
        '
        'm_cbSlantingLabels
        '
        Me.m_cbSlantingLabels.AutoSize = True
        Me.m_cbSlantingLabels.Checked = True
        Me.m_cbSlantingLabels.CheckState = System.Windows.Forms.CheckState.Checked
        Me.m_cbSlantingLabels.Location = New System.Drawing.Point(3, 21)
        Me.m_cbSlantingLabels.Name = "m_cbSlantingLabels"
        Me.m_cbSlantingLabels.Size = New System.Drawing.Size(120, 17)
        Me.m_cbSlantingLabels.TabIndex = 1
        Me.m_cbSlantingLabels.Text = "Draw &slanting labels"
        Me.m_cbSlantingLabels.UseVisualStyleBackColor = True
        '
        'm_cbFitToScreen
        '
        Me.m_cbFitToScreen.AutoSize = True
        Me.m_cbFitToScreen.Location = New System.Drawing.Point(3, 44)
        Me.m_cbFitToScreen.Name = "m_cbFitToScreen"
        Me.m_cbFitToScreen.Size = New System.Drawing.Size(118, 17)
        Me.m_cbFitToScreen.TabIndex = 2
        Me.m_cbFitToScreen.Text = "&Fit to available area"
        Me.m_cbFitToScreen.UseVisualStyleBackColor = True
        '
        'm_tlpOptions
        '
        Me.m_tlpOptions.ColumnCount = 1
        Me.m_tlpOptions.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpOptions.Controls.Add(Me.m_plLabels, 0, 0)
        Me.m_tlpOptions.Controls.Add(Me.m_plData, 0, 1)
        Me.m_tlpOptions.Controls.Add(Me.m_plGraph, 0, 2)
        Me.m_tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpOptions.Location = New System.Drawing.Point(0, 0)
        Me.m_tlpOptions.Name = "m_tlpOptions"
        Me.m_tlpOptions.RowCount = 4
        Me.m_tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpOptions.Size = New System.Drawing.Size(134, 299)
        Me.m_tlpOptions.TabIndex = 0
        '
        'm_plLabels
        '
        Me.m_plLabels.Controls.Add(Me.m_rbShowName)
        Me.m_plLabels.Controls.Add(Me.m_rbShowNo)
        Me.m_plLabels.Controls.Add(Me.m_rbShowNameNum)
        Me.m_plLabels.Controls.Add(Me.m_hdrOptions)
        Me.m_plLabels.Controls.Add(Me.m_cbSlantingLabels)
        Me.m_plLabels.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_plLabels.Location = New System.Drawing.Point(0, 0)
        Me.m_plLabels.Margin = New System.Windows.Forms.Padding(0)
        Me.m_plLabels.Name = "m_plLabels"
        Me.m_plLabels.Size = New System.Drawing.Size(134, 112)
        Me.m_plLabels.TabIndex = 0
        '
        'm_rbShowName
        '
        Me.m_rbShowName.AutoSize = True
        Me.m_rbShowName.Location = New System.Drawing.Point(3, 90)
        Me.m_rbShowName.Name = "m_rbShowName"
        Me.m_rbShowName.Size = New System.Drawing.Size(75, 17)
        Me.m_rbShowName.TabIndex = 4
        Me.m_rbShowName.TabStop = True
        Me.m_rbShowName.Text = "Name only"
        Me.m_rbShowName.UseVisualStyleBackColor = True
        '
        'm_rbShowNo
        '
        Me.m_rbShowNo.AutoSize = True
        Me.m_rbShowNo.Location = New System.Drawing.Point(3, 67)
        Me.m_rbShowNo.Name = "m_rbShowNo"
        Me.m_rbShowNo.Size = New System.Drawing.Size(84, 17)
        Me.m_rbShowNo.TabIndex = 3
        Me.m_rbShowNo.TabStop = True
        Me.m_rbShowNo.Text = "Number only"
        Me.m_rbShowNo.UseVisualStyleBackColor = True
        '
        'm_rbShowNameNum
        '
        Me.m_rbShowNameNum.AutoSize = True
        Me.m_rbShowNameNum.Location = New System.Drawing.Point(3, 44)
        Me.m_rbShowNameNum.Name = "m_rbShowNameNum"
        Me.m_rbShowNameNum.Size = New System.Drawing.Size(92, 17)
        Me.m_rbShowNameNum.TabIndex = 2
        Me.m_rbShowNameNum.TabStop = True
        Me.m_rbShowNameNum.Text = "No. and name"
        Me.m_rbShowNameNum.UseVisualStyleBackColor = True
        '
        'm_plData
        '
        Me.m_plData.Controls.Add(Me.m_hdrData)
        Me.m_plData.Controls.Add(Me.m_rbColors)
        Me.m_plData.Controls.Add(Me.m_rbRectangles)
        Me.m_plData.Controls.Add(Me.m_rbCircles)
        Me.m_plData.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_plData.Location = New System.Drawing.Point(0, 112)
        Me.m_plData.Margin = New System.Windows.Forms.Padding(0)
        Me.m_plData.Name = "m_plData"
        Me.m_plData.Size = New System.Drawing.Size(134, 97)
        Me.m_plData.TabIndex = 1
        '
        'm_hdrData
        '
        Me.m_hdrData.CanCollapseParent = False
        Me.m_hdrData.CollapsedParentHeight = 0
        Me.m_hdrData.Dock = System.Windows.Forms.DockStyle.Top
        Me.m_hdrData.IsCollapsed = False
        Me.m_hdrData.Location = New System.Drawing.Point(0, 0)
        Me.m_hdrData.Name = "m_hdrData"
        Me.m_hdrData.Size = New System.Drawing.Size(134, 18)
        Me.m_hdrData.TabIndex = 0
        Me.m_hdrData.Text = "Data"
        Me.m_hdrData.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_plGraph
        '
        Me.m_plGraph.Controls.Add(Me.m_hdrPlot)
        Me.m_plGraph.Controls.Add(Me.m_cbShowLegend)
        Me.m_plGraph.Controls.Add(Me.m_cbFitToScreen)
        Me.m_plGraph.Controls.Add(Me.m_cbShowGrid)
        Me.m_plGraph.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_plGraph.Location = New System.Drawing.Point(0, 209)
        Me.m_plGraph.Margin = New System.Windows.Forms.Padding(0)
        Me.m_plGraph.Name = "m_plGraph"
        Me.m_plGraph.Size = New System.Drawing.Size(134, 88)
        Me.m_plGraph.TabIndex = 2
        '
        'm_hdrPlot
        '
        Me.m_hdrPlot.CanCollapseParent = False
        Me.m_hdrPlot.CollapsedParentHeight = 0
        Me.m_hdrPlot.Dock = System.Windows.Forms.DockStyle.Top
        Me.m_hdrPlot.IsCollapsed = False
        Me.m_hdrPlot.Location = New System.Drawing.Point(0, 0)
        Me.m_hdrPlot.Name = "m_hdrPlot"
        Me.m_hdrPlot.Size = New System.Drawing.Size(134, 18)
        Me.m_hdrPlot.TabIndex = 0
        Me.m_hdrPlot.Text = "Plot"
        Me.m_hdrPlot.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'm_cbShowLegend
        '
        Me.m_cbShowLegend.AutoSize = True
        Me.m_cbShowLegend.Location = New System.Drawing.Point(3, 67)
        Me.m_cbShowLegend.Name = "m_cbShowLegend"
        Me.m_cbShowLegend.Size = New System.Drawing.Size(86, 17)
        Me.m_cbShowLegend.TabIndex = 2
        Me.m_cbShowLegend.Text = "Draw &legend"
        Me.m_cbShowLegend.UseVisualStyleBackColor = True
        '
        'm_rbColors
        '
        Me.m_rbColors.AutoSize = True
        Me.m_rbColors.Location = New System.Drawing.Point(3, 67)
        Me.m_rbColors.Name = "m_rbColors"
        Me.m_rbColors.Size = New System.Drawing.Size(54, 17)
        Me.m_rbColors.TabIndex = 3
        Me.m_rbColors.TabStop = True
        Me.m_rbColors.Text = "Co&lors"
        Me.m_rbColors.UseVisualStyleBackColor = True
        '
        'ucPlotOfMTIOptions
        '
        Me.Controls.Add(Me.m_tlpOptions)
        Me.Name = "ucPlotOfMTIOptions"
        Me.Size = New System.Drawing.Size(134, 299)
        Me.m_tlpOptions.ResumeLayout(False)
        Me.m_plLabels.ResumeLayout(False)
        Me.m_plLabels.PerformLayout()
        Me.m_plData.ResumeLayout(False)
        Me.m_plData.PerformLayout()
        Me.m_plGraph.ResumeLayout(False)
        Me.m_plGraph.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private m_bInUpdate As Boolean = False

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_bInUpdate = True

        If Me.m_content.DrawCircles Then
            Me.m_rbCircles.Checked = True
        Else
            Me.m_rbRectangles.Checked = True
        End If

        Me.m_cbShowGrid.Checked = Me.m_content.DrawGrid
        Me.m_cbSlantingLabels.Checked = Me.m_content.DrawSlanted

        Select Case Me.m_content.LabelStyle
            Case cPlotOfMixedTrophicImpact.eLabelStyle.All
                Me.m_rbShowNameNum.Checked = True
            Case cPlotOfMixedTrophicImpact.eLabelStyle.Number
                Me.m_rbShowNo.Checked = True
            Case cPlotOfMixedTrophicImpact.eLabelStyle.Name
                Me.m_rbShowName.Checked = True
        End Select

        Me.m_cbShowLegend.Checked = Me.m_content.DrawLegend

        Me.m_bInUpdate = False

    End Sub

    Private Sub OnDrawModeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbCircles.CheckedChanged, m_rbRectangles.CheckedChanged, m_rbColors.CheckedChanged

        If (Me.m_content Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        If Me.m_rbCircles.Checked Then
            Me.m_content.DrawCircles = True
        ElseIf Me.m_rbRectangles.Checked Then
            Me.m_content.DrawRectangles = True
        Else
            Me.m_content.DrawColors = True
        End If

    End Sub

    Private Sub OnShowGridChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_cbShowGrid.CheckedChanged

        If (Me.m_content Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        Me.m_content.DrawGrid = Me.m_cbShowGrid.Checked

    End Sub

    Private Sub OnSlantLabelsChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_cbSlantingLabels.CheckedChanged

        If (Me.m_content Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        Me.m_content.DrawSlanted = Me.m_cbSlantingLabels.Checked

    End Sub

    Private Sub OnFitToScreenChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_cbFitToScreen.CheckedChanged

        If (Me.m_content Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        Me.m_content.FillPlotToArea = Me.m_cbFitToScreen.Checked

    End Sub

    Private Sub OnLabelDrawModeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbShowName.CheckedChanged, m_rbShowNo.CheckedChanged, m_rbShowNameNum.CheckedChanged

        If (Me.m_content Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        If Me.m_rbShowNo.Checked Then
            Me.m_content.LabelStyle = cPlotOfMixedTrophicImpact.eLabelStyle.Number
        ElseIf Me.m_rbShowName.Checked Then
            Me.m_content.LabelStyle = cPlotOfMixedTrophicImpact.eLabelStyle.Name
        Else
            Me.m_content.LabelStyle = cPlotOfMixedTrophicImpact.eLabelStyle.All
        End If
    End Sub

    Private Sub OnDrawLegendChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_cbShowLegend.CheckedChanged

        If (Me.m_content Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        Me.m_content.DrawLegend = Me.m_cbShowLegend.Checked

    End Sub
End Class
