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

Namespace Controls

    Partial Class dlgImportShapes
        Inherits System.Windows.Forms.Form

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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgImportShapes))
            Me.m_hdrSource = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_tbImportSeparator = New ScientificInterfaceShared.Controls.ucCharacterTextBox()
            Me.m_tbImportDelimiter = New ScientificInterfaceShared.Controls.ucCharacterTextBox()
            Me.m_tbImportFileName = New System.Windows.Forms.TextBox()
            Me.m_lblImportDecimalSeparator = New System.Windows.Forms.Label()
            Me.m_lblImportDelimiter = New System.Windows.Forms.Label()
            Me.m_btnImportBrowse = New System.Windows.Forms.Button()
            Me.m_hdrPreview = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_btnOk = New System.Windows.Forms.Button()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_lblTextFile = New System.Windows.Forms.Label()
            Me.m_grid = New ScientificInterfaceShared.Controls.gridImportShapes()
            Me.m_pbHelp = New System.Windows.Forms.PictureBox()
            CType(Me.m_pbHelp, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_hdrSource
            '
            resources.ApplyResources(Me.m_hdrSource, "m_hdrSource")
            Me.m_hdrSource.CanCollapseParent = False
            Me.m_hdrSource.CollapsedParentHeight = 0
            Me.m_hdrSource.IsCollapsed = False
            Me.m_hdrSource.Name = "m_hdrSource"
            '
            'm_tbImportSeparator
            '
            Me.m_tbImportSeparator.AcceptsReturn = True
            Me.m_tbImportSeparator.AcceptsTab = True
            resources.ApplyResources(Me.m_tbImportSeparator, "m_tbImportSeparator")
            Me.m_tbImportSeparator.Character = Global.Microsoft.VisualBasic.ChrW(46)
            Me.m_tbImportSeparator.CharacterMask = Global.ScientificInterfaceShared.My.Resources.Resources.STYLEFLAGS_OK
            Me.m_tbImportSeparator.CharCode = 46
            Me.m_tbImportSeparator.MaskInclusive = False
            Me.m_tbImportSeparator.Name = "m_tbImportSeparator"
            Me.m_tbImportSeparator.ShortcutsEnabled = False
            '
            'm_tbImportDelimiter
            '
            Me.m_tbImportDelimiter.AcceptsReturn = True
            Me.m_tbImportDelimiter.AcceptsTab = True
            Me.m_tbImportDelimiter.Character = Global.Microsoft.VisualBasic.ChrW(44)
            Me.m_tbImportDelimiter.CharacterMask = Global.ScientificInterfaceShared.My.Resources.Resources.STYLEFLAGS_OK
            Me.m_tbImportDelimiter.CharCode = 44
            resources.ApplyResources(Me.m_tbImportDelimiter, "m_tbImportDelimiter")
            Me.m_tbImportDelimiter.MaskInclusive = False
            Me.m_tbImportDelimiter.Name = "m_tbImportDelimiter"
            Me.m_tbImportDelimiter.ShortcutsEnabled = False
            '
            'm_tbImportFileName
            '
            resources.ApplyResources(Me.m_tbImportFileName, "m_tbImportFileName")
            Me.m_tbImportFileName.Name = "m_tbImportFileName"
            Me.m_tbImportFileName.ReadOnly = True
            '
            'm_lblImportDecimalSeparator
            '
            resources.ApplyResources(Me.m_lblImportDecimalSeparator, "m_lblImportDecimalSeparator")
            Me.m_lblImportDecimalSeparator.Name = "m_lblImportDecimalSeparator"
            '
            'm_lblImportDelimiter
            '
            resources.ApplyResources(Me.m_lblImportDelimiter, "m_lblImportDelimiter")
            Me.m_lblImportDelimiter.Name = "m_lblImportDelimiter"
            '
            'm_btnImportBrowse
            '
            resources.ApplyResources(Me.m_btnImportBrowse, "m_btnImportBrowse")
            Me.m_btnImportBrowse.Name = "m_btnImportBrowse"
            Me.m_btnImportBrowse.UseVisualStyleBackColor = True
            '
            'm_hdrPreview
            '
            resources.ApplyResources(Me.m_hdrPreview, "m_hdrPreview")
            Me.m_hdrPreview.CanCollapseParent = False
            Me.m_hdrPreview.CollapsedParentHeight = 0
            Me.m_hdrPreview.IsCollapsed = False
            Me.m_hdrPreview.Name = "m_hdrPreview"
            '
            'm_btnOk
            '
            resources.ApplyResources(Me.m_btnOk, "m_btnOk")
            Me.m_btnOk.Name = "m_btnOk"
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            '
            'm_lblTextFile
            '
            resources.ApplyResources(Me.m_lblTextFile, "m_lblTextFile")
            Me.m_lblTextFile.Name = "m_lblTextFile"
            '
            'm_grid
            '
            Me.m_grid.AllowBlockSelect = False
            resources.ApplyResources(Me.m_grid, "m_grid")
            Me.m_grid.AutoSizeMinHeight = 10
            Me.m_grid.AutoSizeMinWidth = 10
            Me.m_grid.AutoStretchColumnsToFitWidth = True
            Me.m_grid.AutoStretchRowsToFitHeight = False
            Me.m_grid.BackColor = System.Drawing.Color.White
            Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_grid.CustomSort = False
            Me.m_grid.DataName = "grid content"
            Me.m_grid.FixedColumnWidths = False
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.Functions = Nothing
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = True
            Me.m_grid.Name = "m_grid"
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.UIContext = Nothing
            '
            'm_pbHelp
            '
            resources.ApplyResources(Me.m_pbHelp, "m_pbHelp")
            Me.m_pbHelp.Name = "m_pbHelp"
            Me.m_pbHelp.TabStop = False
            '
            'dlgImportShapes
            '
            Me.AllowDrop = True
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnCancel
            Me.ControlBox = False
            Me.Controls.Add(Me.m_pbHelp)
            Me.Controls.Add(Me.m_grid)
            Me.Controls.Add(Me.m_lblTextFile)
            Me.Controls.Add(Me.m_btnOk)
            Me.Controls.Add(Me.m_btnCancel)
            Me.Controls.Add(Me.m_hdrPreview)
            Me.Controls.Add(Me.m_hdrSource)
            Me.Controls.Add(Me.m_tbImportSeparator)
            Me.Controls.Add(Me.m_tbImportDelimiter)
            Me.Controls.Add(Me.m_tbImportFileName)
            Me.Controls.Add(Me.m_lblImportDecimalSeparator)
            Me.Controls.Add(Me.m_lblImportDelimiter)
            Me.Controls.Add(Me.m_btnImportBrowse)
            Me.Name = "dlgImportShapes"
            Me.ShowInTaskbar = False
            CType(Me.m_pbHelp, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private WithEvents m_hdrSource As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_tbImportSeparator As ScientificInterfaceShared.Controls.ucCharacterTextBox
        Private WithEvents m_tbImportDelimiter As ScientificInterfaceShared.Controls.ucCharacterTextBox
        Private WithEvents m_tbImportFileName As System.Windows.Forms.TextBox
        Private WithEvents m_lblImportDecimalSeparator As System.Windows.Forms.Label
        Private WithEvents m_lblImportDelimiter As System.Windows.Forms.Label
        Private WithEvents m_btnImportBrowse As System.Windows.Forms.Button
        Private WithEvents m_hdrPreview As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_btnOk As System.Windows.Forms.Button
        Private WithEvents m_btnCancel As System.Windows.Forms.Button
        Private WithEvents m_lblTextFile As System.Windows.Forms.Label
        Private WithEvents m_grid As gridImportShapes
        Private WithEvents m_pbHelp As System.Windows.Forms.PictureBox

    End Class

End Namespace
