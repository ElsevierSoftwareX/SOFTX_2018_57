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

''' <summary>
''' Form to implement shape grids.
''' </summary>
Partial Class frmShapes
    Inherits frmEwEGrid

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmShapes))
        Me.m_plGrid = New System.Windows.Forms.Panel()
        Me.m_tsMain = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tsbnTimeSeries = New System.Windows.Forms.ToolStripButton()
        Me.m_tsSep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tsbnLongTerm = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnSeasonal = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tsbnShowAllData = New System.Windows.Forms.ToolStripButton()
        Me.m_tsMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_plGrid
        '
        resources.ApplyResources(Me.m_plGrid, "m_plGrid")
        Me.m_plGrid.Name = "m_plGrid"
        '
        'm_tsMain
        '
        Me.m_tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnTimeSeries, Me.m_tsSep1, Me.m_tsbnLongTerm, Me.m_tsbnSeasonal, Me.ToolStripSeparator1, Me.m_tsbnShowAllData})
        resources.ApplyResources(Me.m_tsMain, "m_tsMain")
        Me.m_tsMain.Name = "m_tsMain"
        Me.m_tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tsbnTimeSeries
        '
        Me.m_tsbnTimeSeries.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnTimeSeries, "m_tsbnTimeSeries")
        Me.m_tsbnTimeSeries.Name = "m_tsbnTimeSeries"
        '
        'm_tsSep1
        '
        Me.m_tsSep1.Name = "m_tsSep1"
        resources.ApplyResources(Me.m_tsSep1, "m_tsSep1")
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
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        '
        'm_tsbnShowAllData
        '
        Me.m_tsbnShowAllData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        resources.ApplyResources(Me.m_tsbnShowAllData, "m_tsbnShowAllData")
        Me.m_tsbnShowAllData.Name = "m_tsbnShowAllData"
        '
        'frmShapes
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_plGrid)
        Me.Controls.Add(Me.m_tsMain)
        Me.Name = "frmShapes"
        Me.TabText = ""
        Me.m_tsMain.ResumeLayout(False)
        Me.m_tsMain.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private WithEvents m_tsMain As ScientificInterfaceShared.Controls.cEwEToolstrip
    Private WithEvents m_plGrid As System.Windows.Forms.Panel
    Private WithEvents m_tsbnTimeSeries As System.Windows.Forms.ToolStripButton
    Private m_tsSep1 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsbnLongTerm As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnSeasonal As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsbnShowAllData As System.Windows.Forms.ToolStripButton

End Class
