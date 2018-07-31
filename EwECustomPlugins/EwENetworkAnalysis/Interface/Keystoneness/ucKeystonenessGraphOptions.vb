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
Public Class ucKeystonenessGraphOptions
    Inherits usercontrol

    Private WithEvents m_rbCircles As System.Windows.Forms.RadioButton
    Private WithEvents m_rbCircleScaled As System.Windows.Forms.RadioButton
    Private WithEvents m_tlpOptions As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_plData As System.Windows.Forms.Panel
    Private WithEvents m_hdrData As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_rbNumbers As System.Windows.Forms.RadioButton
    Private m_content As cKeystonenessGraph = Nothing

    Public Sub New(ByVal content As cKeystonenessGraph)
        Me.InitializeComponent()
        Me.m_content = content
    End Sub

    Private Sub InitializeComponent()
        Me.m_rbCircles = New System.Windows.Forms.RadioButton()
        Me.m_rbCircleScaled = New System.Windows.Forms.RadioButton()
        Me.m_tlpOptions = New System.Windows.Forms.TableLayoutPanel()
        Me.m_plData = New System.Windows.Forms.Panel()
        Me.m_hdrData = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_rbNumbers = New System.Windows.Forms.RadioButton()
        Me.m_tlpOptions.SuspendLayout()
        Me.m_plData.SuspendLayout()
        Me.SuspendLayout()
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
        'm_rbCircleScaled
        '
        Me.m_rbCircleScaled.AutoSize = True
        Me.m_rbCircleScaled.Location = New System.Drawing.Point(3, 44)
        Me.m_rbCircleScaled.Name = "m_rbCircleScaled"
        Me.m_rbCircleScaled.Size = New System.Drawing.Size(96, 17)
        Me.m_rbCircleScaled.TabIndex = 2
        Me.m_rbCircleScaled.TabStop = True
        Me.m_rbCircleScaled.Text = "Circles (&scaled)"
        Me.m_rbCircleScaled.UseVisualStyleBackColor = True
        '
        'm_tlpOptions
        '
        Me.m_tlpOptions.ColumnCount = 1
        Me.m_tlpOptions.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpOptions.Controls.Add(Me.m_plData, 0, 0)
        Me.m_tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpOptions.Location = New System.Drawing.Point(0, 0)
        Me.m_tlpOptions.Name = "m_tlpOptions"
        Me.m_tlpOptions.RowCount = 2
        Me.m_tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.m_tlpOptions.Size = New System.Drawing.Size(134, 299)
        Me.m_tlpOptions.TabIndex = 0
        '
        'm_plData
        '
        Me.m_plData.Controls.Add(Me.m_hdrData)
        Me.m_plData.Controls.Add(Me.m_rbNumbers)
        Me.m_plData.Controls.Add(Me.m_rbCircleScaled)
        Me.m_plData.Controls.Add(Me.m_rbCircles)
        Me.m_plData.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_plData.Location = New System.Drawing.Point(0, 0)
        Me.m_plData.Margin = New System.Windows.Forms.Padding(0)
        Me.m_plData.Name = "m_plData"
        Me.m_plData.Size = New System.Drawing.Size(134, 89)
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
        'm_rbNumbers
        '
        Me.m_rbNumbers.AutoSize = True
        Me.m_rbNumbers.Location = New System.Drawing.Point(2, 67)
        Me.m_rbNumbers.Name = "m_rbNumbers"
        Me.m_rbNumbers.Size = New System.Drawing.Size(67, 17)
        Me.m_rbNumbers.TabIndex = 2
        Me.m_rbNumbers.TabStop = True
        Me.m_rbNumbers.Text = "&Numbers"
        Me.m_rbNumbers.UseVisualStyleBackColor = True
        '
        'ucKeystonenessGraphOptions
        '
        Me.Controls.Add(Me.m_tlpOptions)
        Me.Name = "ucKeystonenessGraphOptions"
        Me.Size = New System.Drawing.Size(134, 299)
        Me.m_tlpOptions.ResumeLayout(False)
        Me.m_plData.ResumeLayout(False)
        Me.m_plData.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private m_bInUpdate As Boolean = False

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_bInUpdate = True

        Select Case Me.m_content.Representation
            Case cKeystonenessGraph.eRepresentationType.Circle
                Me.m_rbCircles.Checked = True
            Case cKeystonenessGraph.eRepresentationType.CircleScaled
                Me.m_rbCircleScaled.Checked = True
            Case cKeystonenessGraph.eRepresentationType.Number
                Me.m_rbNumbers.Checked = True
        End Select

        Me.m_bInUpdate = False

    End Sub

    Private Sub OnDrawModeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbCircles.CheckedChanged, m_rbCircleScaled.CheckedChanged, m_rbNumbers.CheckedChanged

        If (Me.m_content Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        If Me.m_rbCircles.Checked Then
            Me.m_content.Representation = cKeystonenessGraph.eRepresentationType.Circle
        ElseIf Me.m_rbCircleScaled.Checked Then
            Me.m_content.Representation = cKeystonenessGraph.eRepresentationType.CircleScaled
        Else
            Me.m_content.Representation = cKeystonenessGraph.eRepresentationType.Number
        End If

    End Sub

End Class
