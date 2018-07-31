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

Namespace Ecosim

    Partial Class dlgShowHideItems
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgShowHideItems))
            Me.OK_Button = New System.Windows.Forms.Button()
            Me.Cancel_Button = New System.Windows.Forms.Button()
            Me.m_clbGroups = New System.Windows.Forms.CheckedListBox()
            Me.m_btnAllGroups = New System.Windows.Forms.Button()
            Me.m_btnNoneGroups = New System.Windows.Forms.Button()
            Me.m_hdrAdd = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_btnLiving = New System.Windows.Forms.Button()
            Me.m_btnNonLiving = New System.Windows.Forms.Button()
            Me.m_btnStanza = New System.Windows.Forms.Button()
            Me.m_btnConsumers = New System.Windows.Forms.Button()
            Me.m_btnNonStanza = New System.Windows.Forms.Button()
            Me.m_btnProducers = New System.Windows.Forms.Button()
            Me.m_btnNonFished = New System.Windows.Forms.Button()
            Me.m_btnFished = New System.Windows.Forms.Button()
            Me.m_clbFleets = New System.Windows.Forms.CheckedListBox()
            Me.m_btnAllFleets = New System.Windows.Forms.Button()
            Me.m_btnNoneFleets = New System.Windows.Forms.Button()
            Me.m_tlpContent = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plGroups = New System.Windows.Forms.Panel()
            Me.m_hdrGroups = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plFleets = New System.Windows.Forms.Panel()
            Me.m_hdrFleets = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_cbSyncViaFishing = New System.Windows.Forms.CheckBox()
            Me.m_cbSyncViaPredation = New System.Windows.Forms.CheckBox()
            Me.m_lblSyncSelections = New System.Windows.Forms.Label()
            Me.m_tlpContent.SuspendLayout()
            Me.m_plGroups.SuspendLayout()
            Me.m_plFleets.SuspendLayout()
            Me.SuspendLayout()
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
            'm_clbGroups
            '
            resources.ApplyResources(Me.m_clbGroups, "m_clbGroups")
            Me.m_clbGroups.CheckOnClick = True
            Me.m_clbGroups.FormattingEnabled = True
            Me.m_clbGroups.Name = "m_clbGroups"
            '
            'm_btnAllGroups
            '
            resources.ApplyResources(Me.m_btnAllGroups, "m_btnAllGroups")
            Me.m_btnAllGroups.Name = "m_btnAllGroups"
            '
            'm_btnNoneGroups
            '
            resources.ApplyResources(Me.m_btnNoneGroups, "m_btnNoneGroups")
            Me.m_btnNoneGroups.Name = "m_btnNoneGroups"
            '
            'm_hdrAdd
            '
            resources.ApplyResources(Me.m_hdrAdd, "m_hdrAdd")
            Me.m_hdrAdd.CanCollapseParent = False
            Me.m_hdrAdd.CollapsedParentHeight = 0
            Me.m_hdrAdd.IsCollapsed = False
            Me.m_hdrAdd.Name = "m_hdrAdd"
            '
            'm_btnLiving
            '
            resources.ApplyResources(Me.m_btnLiving, "m_btnLiving")
            Me.m_btnLiving.Name = "m_btnLiving"
            '
            'm_btnNonLiving
            '
            resources.ApplyResources(Me.m_btnNonLiving, "m_btnNonLiving")
            Me.m_btnNonLiving.Name = "m_btnNonLiving"
            '
            'm_btnStanza
            '
            resources.ApplyResources(Me.m_btnStanza, "m_btnStanza")
            Me.m_btnStanza.Name = "m_btnStanza"
            '
            'm_btnConsumers
            '
            resources.ApplyResources(Me.m_btnConsumers, "m_btnConsumers")
            Me.m_btnConsumers.Name = "m_btnConsumers"
            '
            'm_btnNonStanza
            '
            resources.ApplyResources(Me.m_btnNonStanza, "m_btnNonStanza")
            Me.m_btnNonStanza.Name = "m_btnNonStanza"
            '
            'm_btnProducers
            '
            resources.ApplyResources(Me.m_btnProducers, "m_btnProducers")
            Me.m_btnProducers.Name = "m_btnProducers"
            '
            'm_btnNonFished
            '
            resources.ApplyResources(Me.m_btnNonFished, "m_btnNonFished")
            Me.m_btnNonFished.Name = "m_btnNonFished"
            '
            'm_btnFished
            '
            resources.ApplyResources(Me.m_btnFished, "m_btnFished")
            Me.m_btnFished.Name = "m_btnFished"
            '
            'm_clbFleets
            '
            resources.ApplyResources(Me.m_clbFleets, "m_clbFleets")
            Me.m_clbFleets.CheckOnClick = True
            Me.m_clbFleets.FormattingEnabled = True
            Me.m_clbFleets.Name = "m_clbFleets"
            '
            'm_btnAllFleets
            '
            resources.ApplyResources(Me.m_btnAllFleets, "m_btnAllFleets")
            Me.m_btnAllFleets.Name = "m_btnAllFleets"
            '
            'm_btnNoneFleets
            '
            resources.ApplyResources(Me.m_btnNoneFleets, "m_btnNoneFleets")
            Me.m_btnNoneFleets.Name = "m_btnNoneFleets"
            '
            'm_tlpContent
            '
            resources.ApplyResources(Me.m_tlpContent, "m_tlpContent")
            Me.m_tlpContent.Controls.Add(Me.m_plGroups, 0, 0)
            Me.m_tlpContent.Controls.Add(Me.m_plFleets, 2, 0)
            Me.m_tlpContent.Name = "m_tlpContent"
            '
            'm_plGroups
            '
            Me.m_plGroups.Controls.Add(Me.m_hdrAdd)
            Me.m_plGroups.Controls.Add(Me.m_hdrGroups)
            Me.m_plGroups.Controls.Add(Me.m_btnLiving)
            Me.m_plGroups.Controls.Add(Me.m_clbGroups)
            Me.m_plGroups.Controls.Add(Me.m_btnNonLiving)
            Me.m_plGroups.Controls.Add(Me.m_btnAllGroups)
            Me.m_plGroups.Controls.Add(Me.m_btnStanza)
            Me.m_plGroups.Controls.Add(Me.m_btnNoneGroups)
            Me.m_plGroups.Controls.Add(Me.m_btnConsumers)
            Me.m_plGroups.Controls.Add(Me.m_btnFished)
            Me.m_plGroups.Controls.Add(Me.m_btnNonStanza)
            Me.m_plGroups.Controls.Add(Me.m_btnNonFished)
            Me.m_plGroups.Controls.Add(Me.m_btnProducers)
            resources.ApplyResources(Me.m_plGroups, "m_plGroups")
            Me.m_plGroups.Name = "m_plGroups"
            '
            'm_hdrGroups
            '
            resources.ApplyResources(Me.m_hdrGroups, "m_hdrGroups")
            Me.m_hdrGroups.CanCollapseParent = False
            Me.m_hdrGroups.CollapsedParentHeight = 0
            Me.m_hdrGroups.IsCollapsed = False
            Me.m_hdrGroups.Name = "m_hdrGroups"
            '
            'm_plFleets
            '
            Me.m_plFleets.Controls.Add(Me.m_btnAllFleets)
            Me.m_plFleets.Controls.Add(Me.m_btnNoneFleets)
            Me.m_plFleets.Controls.Add(Me.m_clbFleets)
            Me.m_plFleets.Controls.Add(Me.m_hdrFleets)
            resources.ApplyResources(Me.m_plFleets, "m_plFleets")
            Me.m_plFleets.Name = "m_plFleets"
            '
            'm_hdrFleets
            '
            resources.ApplyResources(Me.m_hdrFleets, "m_hdrFleets")
            Me.m_hdrFleets.CanCollapseParent = False
            Me.m_hdrFleets.CollapsedParentHeight = 0
            Me.m_hdrFleets.IsCollapsed = False
            Me.m_hdrFleets.Name = "m_hdrFleets"
            '
            'm_cbSyncViaFishing
            '
            resources.ApplyResources(Me.m_cbSyncViaFishing, "m_cbSyncViaFishing")
            Me.m_cbSyncViaFishing.Name = "m_cbSyncViaFishing"
            Me.m_cbSyncViaFishing.UseVisualStyleBackColor = True
            '
            'm_cbSyncViaPredation
            '
            resources.ApplyResources(Me.m_cbSyncViaPredation, "m_cbSyncViaPredation")
            Me.m_cbSyncViaPredation.Name = "m_cbSyncViaPredation"
            Me.m_cbSyncViaPredation.UseVisualStyleBackColor = True
            '
            'm_lblSyncSelections
            '
            resources.ApplyResources(Me.m_lblSyncSelections, "m_lblSyncSelections")
            Me.m_lblSyncSelections.Name = "m_lblSyncSelections"
            '
            'dlgShowHideItems
            '
            Me.AcceptButton = Me.OK_Button
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.Cancel_Button
            Me.ControlBox = False
            Me.Controls.Add(Me.m_lblSyncSelections)
            Me.Controls.Add(Me.m_tlpContent)
            Me.Controls.Add(Me.m_cbSyncViaPredation)
            Me.Controls.Add(Me.m_cbSyncViaFishing)
            Me.Controls.Add(Me.OK_Button)
            Me.Controls.Add(Me.Cancel_Button)
            Me.DoubleBuffered = True
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgShowHideItems"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
            Me.m_tlpContent.ResumeLayout(False)
            Me.m_plGroups.ResumeLayout(False)
            Me.m_plFleets.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_btnAllGroups As System.Windows.Forms.Button
        Private WithEvents m_btnNoneGroups As System.Windows.Forms.Button
        Private WithEvents m_clbGroups As System.Windows.Forms.CheckedListBox
        Private WithEvents m_clbFleets As System.Windows.Forms.CheckedListBox
        Private WithEvents m_btnAllFleets As System.Windows.Forms.Button
        Private WithEvents m_btnNoneFleets As System.Windows.Forms.Button
        Private WithEvents m_hdrAdd As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_btnNonLiving As System.Windows.Forms.Button
        Private WithEvents m_btnConsumers As System.Windows.Forms.Button
        Private WithEvents m_btnProducers As System.Windows.Forms.Button
        Private WithEvents m_btnFished As System.Windows.Forms.Button
        Private WithEvents m_btnLiving As System.Windows.Forms.Button
        Private WithEvents OK_Button As System.Windows.Forms.Button
        Private WithEvents Cancel_Button As System.Windows.Forms.Button
        Private WithEvents m_btnStanza As System.Windows.Forms.Button
        Private WithEvents m_btnNonStanza As System.Windows.Forms.Button
        Private WithEvents m_btnNonFished As System.Windows.Forms.Button
        Private WithEvents m_plGroups As System.Windows.Forms.Panel
        Private WithEvents m_hdrGroups As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_plFleets As System.Windows.Forms.Panel
        Private WithEvents m_hdrFleets As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_tlpContent As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_cbSyncViaFishing As CheckBox
        Private WithEvents m_cbSyncViaPredation As CheckBox
        Private WithEvents m_lblSyncSelections As Label
    End Class

End Namespace

