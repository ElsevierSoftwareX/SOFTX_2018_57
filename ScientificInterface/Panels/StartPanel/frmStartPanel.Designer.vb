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

Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ScientificInterfaceShared.Forms

Partial Class frmStartPanel
    Inherits frmEwE

    'UserControl overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStartPanel))
        Me.m_tlp = New System.Windows.Forms.TableLayoutPanel()
        Me.m_browser = New System.Windows.Forms.WebBrowser()
        Me.m_ts1 = New ScientificInterfaceShared.Controls.cEwEToolstrip()
        Me.m_tsbnStartPage = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnEcopathSite = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnFacebook = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnBugTracker = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnBetaFeedback = New System.Windows.Forms.ToolStripButton()
        Me.m_sep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tsbnBack = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnForward = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnRefresh = New System.Windows.Forms.ToolStripButton()
        Me.m_sep2 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tlp.SuspendLayout()
        Me.m_ts1.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_tlp
        '
        resources.ApplyResources(Me.m_tlp, "m_tlp")
        Me.m_tlp.Controls.Add(Me.m_browser, 0, 1)
        Me.m_tlp.Controls.Add(Me.m_ts1, 0, 0)
        Me.m_tlp.Name = "m_tlp"
        '
        'm_browser
        '
        Me.m_browser.AllowWebBrowserDrop = False
        resources.ApplyResources(Me.m_browser, "m_browser")
        Me.m_browser.Name = "m_browser"
        Me.m_browser.ScriptErrorsSuppressed = True
        '
        'm_ts1
        '
        Me.m_ts1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_ts1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnStartPage, Me.m_tsbnEcopathSite, Me.m_tsbnFacebook, Me.m_tsbnBugTracker, Me.m_tsbnBetaFeedback, Me.m_sep1, Me.m_tsbnBack, Me.m_tsbnForward, Me.m_tsbnRefresh, Me.m_sep2})
        resources.ApplyResources(Me.m_ts1, "m_ts1")
        Me.m_ts1.Name = "m_ts1"
        Me.m_ts1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'm_tsbnStartPage
        '
        Me.m_tsbnStartPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnStartPage, "m_tsbnStartPage")
        Me.m_tsbnStartPage.Name = "m_tsbnStartPage"
        '
        'm_tsbnEcopathSite
        '
        Me.m_tsbnEcopathSite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnEcopathSite, "m_tsbnEcopathSite")
        Me.m_tsbnEcopathSite.Name = "m_tsbnEcopathSite"
        '
        'm_tsbnFacebook
        '
        Me.m_tsbnFacebook.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnFacebook, "m_tsbnFacebook")
        Me.m_tsbnFacebook.Name = "m_tsbnFacebook"
        '
        'm_tsbnBugTracker
        '
        Me.m_tsbnBugTracker.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnBugTracker, "m_tsbnBugTracker")
        Me.m_tsbnBugTracker.Name = "m_tsbnBugTracker"
        '
        'm_tsbnBetaFeedback
        '
        Me.m_tsbnBetaFeedback.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnBetaFeedback, "m_tsbnBetaFeedback")
        Me.m_tsbnBetaFeedback.Name = "m_tsbnBetaFeedback"
        '
        'm_sep1
        '
        Me.m_sep1.Name = "m_sep1"
        resources.ApplyResources(Me.m_sep1, "m_sep1")
        '
        'm_tsbnBack
        '
        Me.m_tsbnBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnBack, "m_tsbnBack")
        Me.m_tsbnBack.Name = "m_tsbnBack"
        '
        'm_tsbnForward
        '
        Me.m_tsbnForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.m_tsbnForward, "m_tsbnForward")
        Me.m_tsbnForward.Name = "m_tsbnForward"
        '
        'm_tsbnRefresh
        '
        resources.ApplyResources(Me.m_tsbnRefresh, "m_tsbnRefresh")
        Me.m_tsbnRefresh.Name = "m_tsbnRefresh"
        '
        'm_sep2
        '
        Me.m_sep2.Name = "m_sep2"
        resources.ApplyResources(Me.m_sep2, "m_sep2")
        '
        'frmStartPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_tlp)
        Me.HideOnClose = True
        Me.Name = "frmStartPanel"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.TabText = ""
        Me.m_tlp.ResumeLayout(False)
        Me.m_tlp.PerformLayout()
        Me.m_ts1.ResumeLayout(False)
        Me.m_ts1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_browser As System.Windows.Forms.WebBrowser
    Private WithEvents m_ts1 As cEwEToolstrip
    Private WithEvents m_tsbnBack As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tlp As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_tsbnForward As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnRefresh As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnStartPage As System.Windows.Forms.ToolStripButton
    Private WithEvents m_sep1 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_sep2 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents m_tsbnEcopathSite As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnBugTracker As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnBetaFeedback As System.Windows.Forms.ToolStripButton
    Private WithEvents m_tsbnFacebook As System.Windows.Forms.ToolStripButton

End Class
