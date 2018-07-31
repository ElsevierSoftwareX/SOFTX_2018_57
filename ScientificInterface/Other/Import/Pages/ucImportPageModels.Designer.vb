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

Namespace Import

    Partial Class ucImportPageModels
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
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
            Me.m_btnBrowse = New System.Windows.Forms.Button()
            Me.m_tbxOutputFolder = New System.Windows.Forms.TextBox()
            Me.m_lblOutputFolder = New System.Windows.Forms.Label()
            Me.m_hdrModels = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_lblFormat = New System.Windows.Forms.Label()
            Me.m_cmbDatabaseFormat = New System.Windows.Forms.ComboBox()
            Me.m_scModels = New System.Windows.Forms.SplitContainer()
            Me.m_grid = New ScientificInterface.Import.cImportGrid()
            Me.m_lblComments = New System.Windows.Forms.Label()
            CType(Me.m_scModels, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.m_scModels.Panel1.SuspendLayout()
            Me.m_scModels.Panel2.SuspendLayout()
            Me.m_scModels.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_btnBrowse
            '
            Me.m_btnBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Popup
            Me.m_btnBrowse.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_btnBrowse.Location = New System.Drawing.Point(446, 251)
            Me.m_btnBrowse.Name = "m_btnBrowse"
            Me.m_btnBrowse.Size = New System.Drawing.Size(64, 23)
            Me.m_btnBrowse.TabIndex = 5
            Me.m_btnBrowse.Text = "&Browse..."
            Me.m_btnBrowse.UseVisualStyleBackColor = True
            '
            'm_tbxOutputFolder
            '
            Me.m_tbxOutputFolder.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_tbxOutputFolder.Location = New System.Drawing.Point(104, 253)
            Me.m_tbxOutputFolder.Name = "m_tbxOutputFolder"
            Me.m_tbxOutputFolder.Size = New System.Drawing.Size(336, 20)
            Me.m_tbxOutputFolder.TabIndex = 4
            '
            'm_lblOutputFolder
            '
            Me.m_lblOutputFolder.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.m_lblOutputFolder.AutoSize = True
            Me.m_lblOutputFolder.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
            Me.m_lblOutputFolder.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_lblOutputFolder.Location = New System.Drawing.Point(3, 256)
            Me.m_lblOutputFolder.Name = "m_lblOutputFolder"
            Me.m_lblOutputFolder.Size = New System.Drawing.Size(92, 13)
            Me.m_lblOutputFolder.TabIndex = 3
            Me.m_lblOutputFolder.Text = "&Destination folder:"
            '
            'm_hdrModels
            '
            Me.m_hdrModels.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_hdrModels.CanCollapseParent = False
            Me.m_hdrModels.CollapsedParentHeight = 0
            Me.m_hdrModels.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.m_hdrModels.IsCollapsed = False
            Me.m_hdrModels.Location = New System.Drawing.Point(3, 0)
            Me.m_hdrModels.Margin = New System.Windows.Forms.Padding(0)
            Me.m_hdrModels.Name = "m_hdrModels"
            Me.m_hdrModels.Size = New System.Drawing.Size(507, 18)
            Me.m_hdrModels.TabIndex = 1
            Me.m_hdrModels.Text = "Select model(s) to import"
            Me.m_hdrModels.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'm_lblFormat
            '
            Me.m_lblFormat.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.m_lblFormat.AutoSize = True
            Me.m_lblFormat.Location = New System.Drawing.Point(3, 282)
            Me.m_lblFormat.Name = "m_lblFormat"
            Me.m_lblFormat.Size = New System.Drawing.Size(95, 13)
            Me.m_lblFormat.TabIndex = 6
            Me.m_lblFormat.Text = "&Destination format:"
            '
            'm_cmbDatabaseFormat
            '
            Me.m_cmbDatabaseFormat.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_cmbDatabaseFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbDatabaseFormat.FormattingEnabled = True
            Me.m_cmbDatabaseFormat.Location = New System.Drawing.Point(104, 279)
            Me.m_cmbDatabaseFormat.Name = "m_cmbDatabaseFormat"
            Me.m_cmbDatabaseFormat.Size = New System.Drawing.Size(336, 21)
            Me.m_cmbDatabaseFormat.TabIndex = 7
            '
            'm_scModels
            '
            Me.m_scModels.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_scModels.Location = New System.Drawing.Point(0, 22)
            Me.m_scModels.Margin = New System.Windows.Forms.Padding(0)
            Me.m_scModels.Name = "m_scModels"
            Me.m_scModels.Orientation = System.Windows.Forms.Orientation.Horizontal
            '
            'm_scModels.Panel1
            '
            Me.m_scModels.Panel1.Controls.Add(Me.m_grid)
            '
            'm_scModels.Panel2
            '
            Me.m_scModels.Panel2.Controls.Add(Me.m_lblComments)
            Me.m_scModels.Size = New System.Drawing.Size(510, 227)
            Me.m_scModels.SplitterDistance = 169
            Me.m_scModels.TabIndex = 8
            '
            'm_grid
            '
            Me.m_grid.AllowBlockSelect = True
            Me.m_grid.AutoSizeMinHeight = 10
            Me.m_grid.AutoSizeMinWidth = 10
            Me.m_grid.AutoStretchColumnsToFitWidth = False
            Me.m_grid.AutoStretchRowsToFitHeight = False
            Me.m_grid.BackColor = System.Drawing.Color.White
            Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_grid.CustomSort = False
            Me.m_grid.DataName = "grid content"
            Me.m_grid.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_grid.FixedColumnWidths = False
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = True
            Me.m_grid.Location = New System.Drawing.Point(0, 0)
            Me.m_grid.Margin = New System.Windows.Forms.Padding(0)
            Me.m_grid.Name = "m_grid"
            Me.m_grid.Size = New System.Drawing.Size(510, 169)
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.TabIndex = 2
            Me.m_grid.TrackPropertySelection = False
            Me.m_grid.UIContext = Nothing
            '
            'm_lblComments
            '
            Me.m_lblComments.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_lblComments.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_lblComments.Location = New System.Drawing.Point(0, 0)
            Me.m_lblComments.Name = "m_lblComments"
            Me.m_lblComments.Size = New System.Drawing.Size(510, 54)
            Me.m_lblComments.TabIndex = 0
            Me.m_lblComments.Text = "<comments>"
            '
            'ucImportPageModels
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Controls.Add(Me.m_scModels)
            Me.Controls.Add(Me.m_cmbDatabaseFormat)
            Me.Controls.Add(Me.m_lblFormat)
            Me.Controls.Add(Me.m_btnBrowse)
            Me.Controls.Add(Me.m_tbxOutputFolder)
            Me.Controls.Add(Me.m_lblOutputFolder)
            Me.Controls.Add(Me.m_hdrModels)
            Me.Name = "ucImportPageModels"
            Me.Size = New System.Drawing.Size(510, 300)
            Me.m_scModels.Panel1.ResumeLayout(False)
            Me.m_scModels.Panel2.ResumeLayout(False)
            CType(Me.m_scModels, System.ComponentModel.ISupportInitialize).EndInit()
            Me.m_scModels.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_grid As cImportGrid
        Private WithEvents m_btnBrowse As System.Windows.Forms.Button
        Private WithEvents m_tbxOutputFolder As System.Windows.Forms.TextBox
        Private WithEvents m_lblOutputFolder As System.Windows.Forms.Label
        Private WithEvents m_hdrModels As cEwEHeaderLabel
        Private WithEvents m_lblFormat As System.Windows.Forms.Label
        Private WithEvents m_scModels As System.Windows.Forms.SplitContainer
        Private WithEvents m_lblComments As System.Windows.Forms.Label
        Private WithEvents m_cmbDatabaseFormat As System.Windows.Forms.ComboBox

    End Class

End Namespace
