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

Namespace Ecospace.Controls

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class dlgExportSpatialData
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgExportSpatialData))
            Me.m_clbDatsets = New System.Windows.Forms.CheckedListBox()
            Me.m_btnAll = New System.Windows.Forms.Button()
            Me.m_btnNone = New System.Windows.Forms.Button()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_btnExport = New System.Windows.Forms.Button()
            Me.m_lblName = New System.Windows.Forms.Label()
            Me.m_tbxName = New System.Windows.Forms.TextBox()
            Me.m_btnUsed = New System.Windows.Forms.Button()
            Me.m_lblFolderPreview = New System.Windows.Forms.Label()
            Me.m_lblFolder = New System.Windows.Forms.Label()
            Me.m_lblDescription = New System.Windows.Forms.Label()
            Me.m_tbxDescription = New System.Windows.Forms.TextBox()
            Me.m_btnChoose = New System.Windows.Forms.Button()
            Me.m_tlpContent = New System.Windows.Forms.TableLayoutPanel()
            Me.m_plDestination = New System.Windows.Forms.Panel()
            Me.m_hdrDestination = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_plMetadata = New System.Windows.Forms.Panel()
            Me.CEwEHeaderLabel1 = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.m_lblAuthor = New System.Windows.Forms.Label()
            Me.m_tbxContact = New System.Windows.Forms.TextBox()
            Me.m_tbxAuthor = New System.Windows.Forms.TextBox()
            Me.m_plData = New System.Windows.Forms.Panel()
            Me.m_hdrLabel = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_cbIncludeData = New System.Windows.Forms.CheckBox()
            Me.m_tlpContent.SuspendLayout()
            Me.m_plDestination.SuspendLayout()
            Me.m_plMetadata.SuspendLayout()
            Me.m_plData.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_clbDatsets
            '
            resources.ApplyResources(Me.m_clbDatsets, "m_clbDatsets")
            Me.m_clbDatsets.CheckOnClick = True
            Me.m_clbDatsets.FormattingEnabled = True
            Me.m_clbDatsets.Name = "m_clbDatsets"
            Me.m_clbDatsets.ThreeDCheckBoxes = True
            '
            'm_btnAll
            '
            resources.ApplyResources(Me.m_btnAll, "m_btnAll")
            Me.m_btnAll.Name = "m_btnAll"
            Me.m_btnAll.UseVisualStyleBackColor = True
            '
            'm_btnNone
            '
            resources.ApplyResources(Me.m_btnNone, "m_btnNone")
            Me.m_btnNone.Name = "m_btnNone"
            Me.m_btnNone.UseVisualStyleBackColor = True
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            Me.m_btnCancel.UseVisualStyleBackColor = True
            '
            'm_btnExport
            '
            resources.ApplyResources(Me.m_btnExport, "m_btnExport")
            Me.m_btnExport.Name = "m_btnExport"
            Me.m_btnExport.UseVisualStyleBackColor = True
            '
            'm_lblName
            '
            resources.ApplyResources(Me.m_lblName, "m_lblName")
            Me.m_lblName.Name = "m_lblName"
            '
            'm_tbxName
            '
            resources.ApplyResources(Me.m_tbxName, "m_tbxName")
            Me.m_tbxName.Name = "m_tbxName"
            '
            'm_btnUsed
            '
            resources.ApplyResources(Me.m_btnUsed, "m_btnUsed")
            Me.m_btnUsed.Name = "m_btnUsed"
            Me.m_btnUsed.UseVisualStyleBackColor = True
            '
            'm_lblFolderPreview
            '
            resources.ApplyResources(Me.m_lblFolderPreview, "m_lblFolderPreview")
            Me.m_lblFolderPreview.Name = "m_lblFolderPreview"
            '
            'm_lblFolder
            '
            resources.ApplyResources(Me.m_lblFolder, "m_lblFolder")
            Me.m_lblFolder.Name = "m_lblFolder"
            '
            'm_lblDescription
            '
            resources.ApplyResources(Me.m_lblDescription, "m_lblDescription")
            Me.m_lblDescription.Name = "m_lblDescription"
            '
            'm_tbxDescription
            '
            resources.ApplyResources(Me.m_tbxDescription, "m_tbxDescription")
            Me.m_tbxDescription.Name = "m_tbxDescription"
            '
            'm_btnChoose
            '
            resources.ApplyResources(Me.m_btnChoose, "m_btnChoose")
            Me.m_btnChoose.Name = "m_btnChoose"
            Me.m_btnChoose.UseVisualStyleBackColor = True
            '
            'm_tlpContent
            '
            resources.ApplyResources(Me.m_tlpContent, "m_tlpContent")
            Me.m_tlpContent.Controls.Add(Me.m_plDestination, 0, 0)
            Me.m_tlpContent.Controls.Add(Me.m_plMetadata, 0, 1)
            Me.m_tlpContent.Controls.Add(Me.m_plData, 0, 2)
            Me.m_tlpContent.Name = "m_tlpContent"
            '
            'm_plDestination
            '
            Me.m_plDestination.Controls.Add(Me.m_hdrDestination)
            Me.m_plDestination.Controls.Add(Me.m_lblFolderPreview)
            Me.m_plDestination.Controls.Add(Me.m_lblFolder)
            Me.m_plDestination.Controls.Add(Me.m_btnChoose)
            Me.m_plDestination.Controls.Add(Me.m_lblName)
            Me.m_plDestination.Controls.Add(Me.m_tbxName)
            resources.ApplyResources(Me.m_plDestination, "m_plDestination")
            Me.m_plDestination.Name = "m_plDestination"
            '
            'm_hdrDestination
            '
            resources.ApplyResources(Me.m_hdrDestination, "m_hdrDestination")
            Me.m_hdrDestination.CanCollapseParent = False
            Me.m_hdrDestination.CollapsedParentHeight = 0
            Me.m_hdrDestination.IsCollapsed = False
            Me.m_hdrDestination.Name = "m_hdrDestination"
            '
            'm_plMetadata
            '
            Me.m_plMetadata.Controls.Add(Me.CEwEHeaderLabel1)
            Me.m_plMetadata.Controls.Add(Me.m_tbxDescription)
            Me.m_plMetadata.Controls.Add(Me.Label1)
            Me.m_plMetadata.Controls.Add(Me.m_lblAuthor)
            Me.m_plMetadata.Controls.Add(Me.m_lblDescription)
            Me.m_plMetadata.Controls.Add(Me.m_tbxContact)
            Me.m_plMetadata.Controls.Add(Me.m_tbxAuthor)
            resources.ApplyResources(Me.m_plMetadata, "m_plMetadata")
            Me.m_plMetadata.Name = "m_plMetadata"
            '
            'CEwEHeaderLabel1
            '
            resources.ApplyResources(Me.CEwEHeaderLabel1, "CEwEHeaderLabel1")
            Me.CEwEHeaderLabel1.CanCollapseParent = True
            Me.CEwEHeaderLabel1.CollapsedParentHeight = 114
            Me.CEwEHeaderLabel1.IsCollapsed = False
            Me.CEwEHeaderLabel1.Name = "CEwEHeaderLabel1"
            '
            'Label1
            '
            resources.ApplyResources(Me.Label1, "Label1")
            Me.Label1.Name = "Label1"
            '
            'm_lblAuthor
            '
            resources.ApplyResources(Me.m_lblAuthor, "m_lblAuthor")
            Me.m_lblAuthor.Name = "m_lblAuthor"
            '
            'm_tbxContact
            '
            resources.ApplyResources(Me.m_tbxContact, "m_tbxContact")
            Me.m_tbxContact.Name = "m_tbxContact"
            '
            'm_tbxAuthor
            '
            resources.ApplyResources(Me.m_tbxAuthor, "m_tbxAuthor")
            Me.m_tbxAuthor.Name = "m_tbxAuthor"
            '
            'm_plData
            '
            Me.m_plData.Controls.Add(Me.m_hdrLabel)
            Me.m_plData.Controls.Add(Me.m_clbDatsets)
            Me.m_plData.Controls.Add(Me.m_btnAll)
            Me.m_plData.Controls.Add(Me.m_btnNone)
            Me.m_plData.Controls.Add(Me.m_btnUsed)
            resources.ApplyResources(Me.m_plData, "m_plData")
            Me.m_plData.Name = "m_plData"
            '
            'm_hdrLabel
            '
            resources.ApplyResources(Me.m_hdrLabel, "m_hdrLabel")
            Me.m_hdrLabel.CanCollapseParent = False
            Me.m_hdrLabel.CollapsedParentHeight = 0
            Me.m_hdrLabel.IsCollapsed = False
            Me.m_hdrLabel.Name = "m_hdrLabel"
            '
            'm_cbIncludeData
            '
            resources.ApplyResources(Me.m_cbIncludeData, "m_cbIncludeData")
            Me.m_cbIncludeData.Checked = True
            Me.m_cbIncludeData.CheckState = System.Windows.Forms.CheckState.Checked
            Me.m_cbIncludeData.Name = "m_cbIncludeData"
            Me.m_cbIncludeData.UseVisualStyleBackColor = True
            '
            'dlgExportSpatialData
            '
            Me.AcceptButton = Me.m_btnExport
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnCancel
            Me.ControlBox = False
            Me.Controls.Add(Me.m_cbIncludeData)
            Me.Controls.Add(Me.m_tlpContent)
            Me.Controls.Add(Me.m_btnExport)
            Me.Controls.Add(Me.m_btnCancel)
            Me.Name = "dlgExportSpatialData"
            Me.ShowInTaskbar = False
            Me.m_tlpContent.ResumeLayout(False)
            Me.m_plDestination.ResumeLayout(False)
            Me.m_plDestination.PerformLayout()
            Me.m_plMetadata.ResumeLayout(False)
            Me.m_plMetadata.PerformLayout()
            Me.m_plData.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_btnExport As System.Windows.Forms.Button
        Private WithEvents m_btnCancel As System.Windows.Forms.Button
        Private WithEvents m_btnNone As System.Windows.Forms.Button
        Private WithEvents m_btnAll As System.Windows.Forms.Button
        Private WithEvents m_clbDatsets As System.Windows.Forms.CheckedListBox
        Private WithEvents m_hdrLabel As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_hdrDestination As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Friend WithEvents m_lblName As System.Windows.Forms.Label
        Private WithEvents m_tbxName As System.Windows.Forms.TextBox
        Private WithEvents m_btnUsed As System.Windows.Forms.Button
        Private WithEvents m_lblFolderPreview As System.Windows.Forms.Label
        Friend WithEvents m_lblFolder As System.Windows.Forms.Label
        Private WithEvents m_tbxDescription As System.Windows.Forms.TextBox
        Private WithEvents m_btnChoose As System.Windows.Forms.Button
        Private WithEvents CEwEHeaderLabel1 As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_lblDescription As System.Windows.Forms.Label
        Private WithEvents m_plDestination As System.Windows.Forms.Panel
        Private WithEvents Label1 As System.Windows.Forms.Label
        Private WithEvents m_lblAuthor As System.Windows.Forms.Label
        Private WithEvents m_tbxContact As System.Windows.Forms.TextBox
        Private WithEvents m_tbxAuthor As System.Windows.Forms.TextBox
        Private WithEvents m_plMetadata As System.Windows.Forms.Panel
        Private WithEvents m_plData As System.Windows.Forms.Panel
        Private WithEvents m_tlpContent As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_cbIncludeData As System.Windows.Forms.CheckBox
    End Class

End Namespace
