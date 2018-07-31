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

Namespace Other

    Partial Class ucOptionsSpatialTemporal
        Inherits System.Windows.Forms.UserControl

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucOptionsSpatialTemporal))
            Me.m_btnAdd = New System.Windows.Forms.Button()
            Me.m_btnViewCache = New System.Windows.Forms.Button()
            Me.m_btnClearCache = New System.Windows.Forms.Button()
            Me.m_lblCacheSize = New System.Windows.Forms.Label()
            Me.m_lblCacheSizeValue = New System.Windows.Forms.Label()
            Me.m_lblCacheLocation = New System.Windows.Forms.Label()
            Me.m_lblCacheLocationValue = New System.Windows.Forms.Label()
            Me.m_cbAllowIndexing = New System.Windows.Forms.CheckBox()
            Me.m_btnRemove = New System.Windows.Forms.Button()
            Me.m_lvDatasets = New System.Windows.Forms.ListView()
            Me.m_chdrName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.m_chdrCurrent = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.m_chdrAuthor = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.m_chdrContact = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.m_chdrLOcation = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.m_btnNew = New System.Windows.Forms.Button()
            Me.m_btnSelect = New System.Windows.Forms.Button()
            Me.m_ilLoaded = New System.Windows.Forms.ImageList(Me.components)
            Me.m_lblAvailable = New System.Windows.Forms.Label()
            Me.m_btnExport = New System.Windows.Forms.Button()
            Me.m_hdrIndexing = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrCache = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrCaption = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_btnViewConfig = New System.Windows.Forms.Button()
            Me.SuspendLayout()
            '
            'm_btnAdd
            '
            resources.ApplyResources(Me.m_btnAdd, "m_btnAdd")
            Me.m_btnAdd.Name = "m_btnAdd"
            Me.m_btnAdd.UseVisualStyleBackColor = True
            '
            'm_btnViewCache
            '
            resources.ApplyResources(Me.m_btnViewCache, "m_btnViewCache")
            Me.m_btnViewCache.Name = "m_btnViewCache"
            Me.m_btnViewCache.UseVisualStyleBackColor = True
            '
            'm_btnClearCache
            '
            resources.ApplyResources(Me.m_btnClearCache, "m_btnClearCache")
            Me.m_btnClearCache.Name = "m_btnClearCache"
            Me.m_btnClearCache.UseVisualStyleBackColor = True
            '
            'm_lblCacheSize
            '
            resources.ApplyResources(Me.m_lblCacheSize, "m_lblCacheSize")
            Me.m_lblCacheSize.Name = "m_lblCacheSize"
            '
            'm_lblCacheSizeValue
            '
            resources.ApplyResources(Me.m_lblCacheSizeValue, "m_lblCacheSizeValue")
            Me.m_lblCacheSizeValue.Name = "m_lblCacheSizeValue"
            '
            'm_lblCacheLocation
            '
            resources.ApplyResources(Me.m_lblCacheLocation, "m_lblCacheLocation")
            Me.m_lblCacheLocation.Name = "m_lblCacheLocation"
            '
            'm_lblCacheLocationValue
            '
            resources.ApplyResources(Me.m_lblCacheLocationValue, "m_lblCacheLocationValue")
            Me.m_lblCacheLocationValue.Name = "m_lblCacheLocationValue"
            '
            'm_cbAllowIndexing
            '
            resources.ApplyResources(Me.m_cbAllowIndexing, "m_cbAllowIndexing")
            Me.m_cbAllowIndexing.Name = "m_cbAllowIndexing"
            Me.m_cbAllowIndexing.UseVisualStyleBackColor = True
            '
            'm_btnRemove
            '
            resources.ApplyResources(Me.m_btnRemove, "m_btnRemove")
            Me.m_btnRemove.Name = "m_btnRemove"
            Me.m_btnRemove.UseVisualStyleBackColor = True
            '
            'm_lvDatasets
            '
            resources.ApplyResources(Me.m_lvDatasets, "m_lvDatasets")
            Me.m_lvDatasets.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.m_chdrName, Me.m_chdrCurrent, Me.m_chdrAuthor, Me.m_chdrContact, Me.m_chdrLOcation})
            Me.m_lvDatasets.FullRowSelect = True
            Me.m_lvDatasets.HideSelection = False
            Me.m_lvDatasets.MultiSelect = False
            Me.m_lvDatasets.Name = "m_lvDatasets"
            Me.m_lvDatasets.UseCompatibleStateImageBehavior = False
            Me.m_lvDatasets.View = System.Windows.Forms.View.Details
            '
            'm_chdrName
            '
            resources.ApplyResources(Me.m_chdrName, "m_chdrName")
            '
            'm_chdrCurrent
            '
            resources.ApplyResources(Me.m_chdrCurrent, "m_chdrCurrent")
            '
            'm_chdrAuthor
            '
            resources.ApplyResources(Me.m_chdrAuthor, "m_chdrAuthor")
            '
            'm_chdrContact
            '
            resources.ApplyResources(Me.m_chdrContact, "m_chdrContact")
            '
            'm_chdrLOcation
            '
            resources.ApplyResources(Me.m_chdrLOcation, "m_chdrLOcation")
            '
            'm_btnNew
            '
            resources.ApplyResources(Me.m_btnNew, "m_btnNew")
            Me.m_btnNew.Name = "m_btnNew"
            Me.m_btnNew.UseVisualStyleBackColor = False
            '
            'm_btnSelect
            '
            resources.ApplyResources(Me.m_btnSelect, "m_btnSelect")
            Me.m_btnSelect.Name = "m_btnSelect"
            Me.m_btnSelect.UseVisualStyleBackColor = True
            '
            'm_ilLoaded
            '
            Me.m_ilLoaded.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
            resources.ApplyResources(Me.m_ilLoaded, "m_ilLoaded")
            Me.m_ilLoaded.TransparentColor = System.Drawing.Color.Transparent
            '
            'm_lblAvailable
            '
            resources.ApplyResources(Me.m_lblAvailable, "m_lblAvailable")
            Me.m_lblAvailable.Name = "m_lblAvailable"
            '
            'm_btnExport
            '
            resources.ApplyResources(Me.m_btnExport, "m_btnExport")
            Me.m_btnExport.Name = "m_btnExport"
            Me.m_btnExport.UseVisualStyleBackColor = True
            '
            'm_hdrIndexing
            '
            resources.ApplyResources(Me.m_hdrIndexing, "m_hdrIndexing")
            Me.m_hdrIndexing.CanCollapseParent = False
            Me.m_hdrIndexing.CollapsedParentHeight = 0
            Me.m_hdrIndexing.IsCollapsed = False
            Me.m_hdrIndexing.Name = "m_hdrIndexing"
            '
            'm_hdrCache
            '
            resources.ApplyResources(Me.m_hdrCache, "m_hdrCache")
            Me.m_hdrCache.CanCollapseParent = False
            Me.m_hdrCache.CollapsedParentHeight = 0
            Me.m_hdrCache.IsCollapsed = False
            Me.m_hdrCache.Name = "m_hdrCache"
            '
            'm_hdrCaption
            '
            Me.m_hdrCaption.CanCollapseParent = False
            Me.m_hdrCaption.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrCaption, "m_hdrCaption")
            Me.m_hdrCaption.IsCollapsed = False
            Me.m_hdrCaption.Name = "m_hdrCaption"
            '
            'm_btnViewConfig
            '
            resources.ApplyResources(Me.m_btnViewConfig, "m_btnViewConfig")
            Me.m_btnViewConfig.Name = "m_btnViewConfig"
            Me.m_btnViewConfig.UseVisualStyleBackColor = True
            '
            'ucOptionsSpatialTemporal
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_btnViewConfig)
            Me.Controls.Add(Me.m_lblAvailable)
            Me.Controls.Add(Me.m_lvDatasets)
            Me.Controls.Add(Me.m_cbAllowIndexing)
            Me.Controls.Add(Me.m_lblCacheSizeValue)
            Me.Controls.Add(Me.m_lblCacheLocation)
            Me.Controls.Add(Me.m_lblCacheSize)
            Me.Controls.Add(Me.m_hdrIndexing)
            Me.Controls.Add(Me.m_hdrCache)
            Me.Controls.Add(Me.m_lblCacheLocationValue)
            Me.Controls.Add(Me.m_btnClearCache)
            Me.Controls.Add(Me.m_btnRemove)
            Me.Controls.Add(Me.m_btnExport)
            Me.Controls.Add(Me.m_btnSelect)
            Me.Controls.Add(Me.m_btnNew)
            Me.Controls.Add(Me.m_btnAdd)
            Me.Controls.Add(Me.m_btnViewCache)
            Me.Controls.Add(Me.m_hdrCaption)
            Me.Name = "ucOptionsSpatialTemporal"
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private m_hdrCaption As cEwEHeaderLabel
        Private WithEvents m_btnAdd As System.Windows.Forms.Button
        Private WithEvents m_hdrCache As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_btnViewCache As System.Windows.Forms.Button
        Private WithEvents m_btnClearCache As System.Windows.Forms.Button
        Private WithEvents m_lblCacheSize As System.Windows.Forms.Label
        Private WithEvents m_lblCacheSizeValue As System.Windows.Forms.Label
        Private WithEvents m_lblCacheLocation As System.Windows.Forms.Label
        Private WithEvents m_lblCacheLocationValue As System.Windows.Forms.Label
        Private WithEvents m_hdrIndexing As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_cbAllowIndexing As System.Windows.Forms.CheckBox
        Private WithEvents m_btnRemove As System.Windows.Forms.Button
        Private WithEvents m_lvDatasets As System.Windows.Forms.ListView
        Private WithEvents m_chdrName As System.Windows.Forms.ColumnHeader
        Private WithEvents m_chdrAuthor As System.Windows.Forms.ColumnHeader
        Private WithEvents m_btnNew As System.Windows.Forms.Button
        Private WithEvents m_chdrContact As System.Windows.Forms.ColumnHeader
        Private WithEvents m_chdrLOcation As System.Windows.Forms.ColumnHeader
        Private WithEvents m_btnSelect As System.Windows.Forms.Button
        Private WithEvents m_chdrCurrent As System.Windows.Forms.ColumnHeader
        Private WithEvents m_ilLoaded As System.Windows.Forms.ImageList
        Private WithEvents m_btnExport As System.Windows.Forms.Button
        Private WithEvents m_lblAvailable As System.Windows.Forms.Label
        Private WithEvents m_btnViewConfig As Button
    End Class
End Namespace

