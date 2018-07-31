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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmDefineTransects
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDefineTransects))
        Me.m_scMain = New System.Windows.Forms.SplitContainer()
        Me.m_mapzoom = New ScientificInterfaceShared.Controls.Map.ucMapZoom()
        Me.m_toolstrip = New ScientificInterfaceShared.Controls.Map.ucMapZoomToolbar()
        Me.m_btnDeleteTransect = New System.Windows.Forms.Button()
        Me.m_lbxTransects = New System.Windows.Forms.ListBox()
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_scMain.Panel1.SuspendLayout()
        Me.m_scMain.Panel2.SuspendLayout()
        Me.m_scMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_scMain
        '
        resources.ApplyResources(Me.m_scMain, "m_scMain")
        Me.m_scMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.m_scMain.Name = "m_scMain"
        '
        'm_scMain.Panel1
        '
        Me.m_scMain.Panel1.Controls.Add(Me.m_mapzoom)
        Me.m_scMain.Panel1.Controls.Add(Me.m_toolstrip)
        '
        'm_scMain.Panel2
        '
        Me.m_scMain.Panel2.Controls.Add(Me.m_btnDeleteTransect)
        Me.m_scMain.Panel2.Controls.Add(Me.m_lbxTransects)
        '
        'm_mapzoom
        '
        resources.ApplyResources(Me.m_mapzoom, "m_mapzoom")
        Me.m_mapzoom.Name = "m_mapzoom"
        Me.m_mapzoom.UIContext = Nothing
        '
        'm_toolstrip
        '
        resources.ApplyResources(Me.m_toolstrip, "m_toolstrip")
        Me.m_toolstrip.Name = "m_toolstrip"
        Me.m_toolstrip.UIContext = Nothing
        '
        'm_btnDeleteTransect
        '
        resources.ApplyResources(Me.m_btnDeleteTransect, "m_btnDeleteTransect")
        Me.m_btnDeleteTransect.Name = "m_btnDeleteTransect"
        Me.m_btnDeleteTransect.UseVisualStyleBackColor = True
        '
        'm_lbxTransects
        '
        resources.ApplyResources(Me.m_lbxTransects, "m_lbxTransects")
        Me.m_lbxTransects.FormattingEnabled = True
        Me.m_lbxTransects.Name = "m_lbxTransects"
        '
        'frmDefineTransects
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.m_scMain)
        Me.Name = "frmDefineTransects"
        Me.TabText = ""
        Me.m_scMain.Panel1.ResumeLayout(False)
        Me.m_scMain.Panel1.PerformLayout()
        Me.m_scMain.Panel2.ResumeLayout(False)
        CType(Me.m_scMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_scMain.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_scMain As System.Windows.Forms.SplitContainer
    Private WithEvents m_mapzoom As ScientificInterfaceShared.Controls.Map.ucMapZoom
    Private WithEvents m_toolstrip As ScientificInterfaceShared.Controls.Map.ucMapZoomToolbar
    Friend WithEvents m_lbxTransects As System.Windows.Forms.ListBox
    Private WithEvents m_btnDeleteTransect As System.Windows.Forms.Button
End Class
