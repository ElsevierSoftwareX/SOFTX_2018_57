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

Namespace Ecospace.Basemap

    Partial Class dlgExportLayerDataXYZ
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgExportLayerDataXYZ))
            Me.m_lblTarget = New System.Windows.Forms.Label()
            Me.m_tbTarget = New System.Windows.Forms.TextBox()
            Me.m_btnBrowseTarget = New System.Windows.Forms.Button()
            Me.m_lblMappings = New System.Windows.Forms.Label()
            Me.m_tlpOkCancel = New System.Windows.Forms.TableLayoutPanel()
            Me.m_bntOK = New System.Windows.Forms.Button()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_grid = New ScientificInterface.Ecospace.Basemap.gridExportLayerMappings()
            Me.m_lblRow = New System.Windows.Forms.Label()
            Me.m_lblCol = New System.Windows.Forms.Label()
            Me.m_tbRow = New System.Windows.Forms.TextBox()
            Me.m_tbCol = New System.Windows.Forms.TextBox()
            Me.m_tlpOkCancel.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_lblTarget
            '
            resources.ApplyResources(Me.m_lblTarget, "m_lblTarget")
            Me.m_lblTarget.Name = "m_lblTarget"
            '
            'm_tbTarget
            '
            resources.ApplyResources(Me.m_tbTarget, "m_tbTarget")
            Me.m_tbTarget.Name = "m_tbTarget"
            '
            'm_btnBrowseTarget
            '
            resources.ApplyResources(Me.m_btnBrowseTarget, "m_btnBrowseTarget")
            Me.m_btnBrowseTarget.Name = "m_btnBrowseTarget"
            Me.m_btnBrowseTarget.UseVisualStyleBackColor = True
            '
            'm_lblMappings
            '
            resources.ApplyResources(Me.m_lblMappings, "m_lblMappings")
            Me.m_lblMappings.Name = "m_lblMappings"
            '
            'm_tlpOkCancel
            '
            resources.ApplyResources(Me.m_tlpOkCancel, "m_tlpOkCancel")
            Me.m_tlpOkCancel.Controls.Add(Me.m_bntOK, 0, 0)
            Me.m_tlpOkCancel.Controls.Add(Me.m_btnCancel, 1, 0)
            Me.m_tlpOkCancel.Name = "m_tlpOkCancel"
            '
            'm_bntOK
            '
            resources.ApplyResources(Me.m_bntOK, "m_bntOK")
            Me.m_bntOK.Name = "m_bntOK"
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            '
            'm_grid
            '
            Me.m_grid.AllowBlockSelect = True
            resources.ApplyResources(Me.m_grid, "m_grid")
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
            Me.m_grid.FixedColumnWidths = False
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = True
            Me.m_grid.Layers = Nothing
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
            'm_lblRow
            '
            resources.ApplyResources(Me.m_lblRow, "m_lblRow")
            Me.m_lblRow.Name = "m_lblRow"
            '
            'm_lblCol
            '
            resources.ApplyResources(Me.m_lblCol, "m_lblCol")
            Me.m_lblCol.Name = "m_lblCol"
            '
            'm_tbRow
            '
            resources.ApplyResources(Me.m_tbRow, "m_tbRow")
            Me.m_tbRow.Name = "m_tbRow"
            '
            'm_tbCol
            '
            resources.ApplyResources(Me.m_tbCol, "m_tbCol")
            Me.m_tbCol.Name = "m_tbCol"
            '
            'dlgExportLayerDataXYZ
            '
            Me.AcceptButton = Me.m_bntOK
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnCancel
            Me.ControlBox = False
            Me.Controls.Add(Me.m_tbCol)
            Me.Controls.Add(Me.m_tbRow)
            Me.Controls.Add(Me.m_lblCol)
            Me.Controls.Add(Me.m_lblRow)
            Me.Controls.Add(Me.m_tlpOkCancel)
            Me.Controls.Add(Me.m_grid)
            Me.Controls.Add(Me.m_lblMappings)
            Me.Controls.Add(Me.m_tbTarget)
            Me.Controls.Add(Me.m_btnBrowseTarget)
            Me.Controls.Add(Me.m_lblTarget)
            Me.Name = "dlgExportLayerDataXYZ"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.m_tlpOkCancel.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private WithEvents m_lblTarget As System.Windows.Forms.Label
        Private WithEvents m_tbTarget As System.Windows.Forms.TextBox
        Private WithEvents m_btnBrowseTarget As System.Windows.Forms.Button
        Private WithEvents m_lblMappings As System.Windows.Forms.Label
        Private WithEvents m_grid As gridExportLayerMappings
        Private WithEvents m_tlpOkCancel As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_bntOK As System.Windows.Forms.Button
        Private WithEvents m_btnCancel As System.Windows.Forms.Button
        Private WithEvents m_lblRow As System.Windows.Forms.Label
        Private WithEvents m_lblCol As System.Windows.Forms.Label
        Private WithEvents m_tbRow As System.Windows.Forms.TextBox
        Private WithEvents m_tbCol As System.Windows.Forms.TextBox

    End Class

End Namespace