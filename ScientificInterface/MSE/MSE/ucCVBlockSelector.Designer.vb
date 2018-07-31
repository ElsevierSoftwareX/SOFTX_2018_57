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

Partial Class ucCVBlockSelector
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
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.m_gridSelector = New ScientificInterface.gridSelectColorBlock()
        Me.SuspendLayout()
        '
        'm_gridSelector
        '
        Me.m_gridSelector.AllowBlockSelect = True
        Me.m_gridSelector.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_gridSelector.AutoSizeMinHeight = 10
        Me.m_gridSelector.AutoSizeMinWidth = 10
        Me.m_gridSelector.AutoStretchColumnsToFitWidth = False
        Me.m_gridSelector.AutoStretchRowsToFitHeight = False
        Me.m_gridSelector.ContextMenuStyle = SourceGrid2.ContextMenuStyle.None
        Me.m_gridSelector.CustomSort = False
        Me.m_gridSelector.DataName = "grid content"
        Me.m_gridSelector.FixedColumnWidths = True
        Me.m_gridSelector.FocusStyle = SourceGrid2.FocusStyle.None
        Me.m_gridSelector.GridToolTipActive = True
        Me.m_gridSelector.IsLayoutSuspended = False
        Me.m_gridSelector.IsOutputGrid = True
        Me.m_gridSelector.Location = New System.Drawing.Point(0, 0)
        Me.m_gridSelector.Name = "m_gridSelector"
        Me.m_gridSelector.SelectedBlock = 1
        Me.m_gridSelector.Size = New System.Drawing.Size(539, 67)
        Me.m_gridSelector.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
            Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
            Or SourceGrid2.GridSpecialKeys.Delete) _
            Or SourceGrid2.GridSpecialKeys.Arrows) _
            Or SourceGrid2.GridSpecialKeys.Tab) _
            Or SourceGrid2.GridSpecialKeys.PageDownUp) _
            Or SourceGrid2.GridSpecialKeys.Enter) _
            Or SourceGrid2.GridSpecialKeys.Escape) _
            Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
        Me.m_gridSelector.TabIndex = 4
        Me.m_gridSelector.UIContext = Nothing
        '
        'ucCVBlockSelector
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.Controls.Add(Me.m_gridSelector)
        Me.Name = "ucCVBlockSelector"
        Me.Size = New System.Drawing.Size(539, 67)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_gridSelector As ScientificInterface.gridSelectColorBlock

End Class
