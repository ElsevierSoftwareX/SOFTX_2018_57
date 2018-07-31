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

Namespace Ecopath

    Partial Class EditMultiStanza
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
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(EditMultiStanza))
            Me.m_btnCalculate = New System.Windows.Forms.Button()
            Me.m_btnOK = New System.Windows.Forms.Button()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_lblStanzaGroups = New System.Windows.Forms.Label()
            Me.m_lblK = New System.Windows.Forms.Label()
            Me.m_lblRecPwr = New System.Windows.Forms.Label()
            Me.m_lblBAB = New System.Windows.Forms.Label()
            Me.m_lblWmatWinf = New System.Windows.Forms.Label()
            Me.m_lblFF = New System.Windows.Forms.Label()
            Me.m_txtK = New System.Windows.Forms.TextBox()
            Me.m_txtRecPwr = New System.Windows.Forms.TextBox()
            Me.m_txtBAB = New System.Windows.Forms.TextBox()
            Me.m_txtWmatWinf = New System.Windows.Forms.TextBox()
            Me.m_zgc = New ZedGraph.ZedGraphControl()
            Me.m_cbFFecun = New System.Windows.Forms.CheckBox()
            Me.m_cmbStanzaGroups = New System.Windows.Forms.ComboBox()
            Me.m_cmbFF = New System.Windows.Forms.ComboBox()
            Me.m_cbEggAtSpawn = New System.Windows.Forms.CheckBox()
            Me.m_hdrEcospace = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_hdrEcosim = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
            Me.m_grid = New ScientificInterface.gridEditMultiStanza()
            Me.SuspendLayout()
            '
            'm_btnCalculate
            '
            resources.ApplyResources(Me.m_btnCalculate, "m_btnCalculate")
            Me.m_btnCalculate.Name = "m_btnCalculate"
            Me.m_btnCalculate.UseVisualStyleBackColor = True
            '
            'm_btnOK
            '
            resources.ApplyResources(Me.m_btnOK, "m_btnOK")
            Me.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.m_btnOK.Name = "m_btnOK"
            Me.m_btnOK.UseVisualStyleBackColor = True
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            Me.m_btnCancel.UseVisualStyleBackColor = True
            '
            'm_lblStanzaGroups
            '
            resources.ApplyResources(Me.m_lblStanzaGroups, "m_lblStanzaGroups")
            Me.m_lblStanzaGroups.Name = "m_lblStanzaGroups"
            '
            'm_lblK
            '
            resources.ApplyResources(Me.m_lblK, "m_lblK")
            Me.m_lblK.Name = "m_lblK"
            '
            'm_lblRecPwr
            '
            resources.ApplyResources(Me.m_lblRecPwr, "m_lblRecPwr")
            Me.m_lblRecPwr.Name = "m_lblRecPwr"
            '
            'm_lblBAB
            '
            resources.ApplyResources(Me.m_lblBAB, "m_lblBAB")
            Me.m_lblBAB.Name = "m_lblBAB"
            '
            'm_lblWmatWinf
            '
            resources.ApplyResources(Me.m_lblWmatWinf, "m_lblWmatWinf")
            Me.m_lblWmatWinf.Name = "m_lblWmatWinf"
            '
            'm_lblFF
            '
            resources.ApplyResources(Me.m_lblFF, "m_lblFF")
            Me.m_lblFF.Name = "m_lblFF"
            '
            'm_txtK
            '
            resources.ApplyResources(Me.m_txtK, "m_txtK")
            Me.m_txtK.Name = "m_txtK"
            '
            'm_txtRecPwr
            '
            resources.ApplyResources(Me.m_txtRecPwr, "m_txtRecPwr")
            Me.m_txtRecPwr.Name = "m_txtRecPwr"
            '
            'm_txtBAB
            '
            resources.ApplyResources(Me.m_txtBAB, "m_txtBAB")
            Me.m_txtBAB.Name = "m_txtBAB"
            '
            'm_txtWmatWinf
            '
            resources.ApplyResources(Me.m_txtWmatWinf, "m_txtWmatWinf")
            Me.m_txtWmatWinf.Name = "m_txtWmatWinf"
            '
            'm_zgc
            '
            resources.ApplyResources(Me.m_zgc, "m_zgc")
            Me.m_zgc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
            Me.m_zgc.Name = "m_zgc"
            Me.m_zgc.ScrollGrace = 0.0R
            Me.m_zgc.ScrollMaxX = 0.0R
            Me.m_zgc.ScrollMaxY = 0.0R
            Me.m_zgc.ScrollMaxY2 = 0.0R
            Me.m_zgc.ScrollMinX = 0.0R
            Me.m_zgc.ScrollMinY = 0.0R
            Me.m_zgc.ScrollMinY2 = 0.0R
            Me.m_zgc.TabStop = False
            '
            'm_cbFFecun
            '
            resources.ApplyResources(Me.m_cbFFecun, "m_cbFFecun")
            Me.m_cbFFecun.Name = "m_cbFFecun"
            Me.m_cbFFecun.UseVisualStyleBackColor = True
            '
            'm_cmbStanzaGroups
            '
            resources.ApplyResources(Me.m_cmbStanzaGroups, "m_cmbStanzaGroups")
            Me.m_cmbStanzaGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbStanzaGroups.FormattingEnabled = True
            Me.m_cmbStanzaGroups.Name = "m_cmbStanzaGroups"
            '
            'm_cmbFF
            '
            resources.ApplyResources(Me.m_cmbFF, "m_cmbFF")
            Me.m_cmbFF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbFF.DropDownWidth = 120
            Me.m_cmbFF.FormattingEnabled = True
            Me.m_cmbFF.Name = "m_cmbFF"
            '
            'm_cbEggAtSpawn
            '
            resources.ApplyResources(Me.m_cbEggAtSpawn, "m_cbEggAtSpawn")
            Me.m_cbEggAtSpawn.Name = "m_cbEggAtSpawn"
            Me.m_cbEggAtSpawn.UseVisualStyleBackColor = True
            '
            'm_hdrEcospace
            '
            Me.m_hdrEcospace.CanCollapseParent = False
            Me.m_hdrEcospace.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrEcospace, "m_hdrEcospace")
            Me.m_hdrEcospace.IsCollapsed = False
            Me.m_hdrEcospace.Name = "m_hdrEcospace"
            '
            'm_hdrEcosim
            '
            Me.m_hdrEcosim.CanCollapseParent = False
            Me.m_hdrEcosim.CollapsedParentHeight = 0
            resources.ApplyResources(Me.m_hdrEcosim, "m_hdrEcosim")
            Me.m_hdrEcosim.IsCollapsed = False
            Me.m_hdrEcosim.Name = "m_hdrEcosim"
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
            Me.m_grid.StanzaGroup = Nothing
            Me.m_grid.TabStop = True
            Me.m_grid.UIContext = Nothing
            '
            'EditMultiStanza
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnCancel
            Me.ControlBox = False
            Me.Controls.Add(Me.m_cbEggAtSpawn)
            Me.Controls.Add(Me.m_hdrEcospace)
            Me.Controls.Add(Me.m_hdrEcosim)
            Me.Controls.Add(Me.m_cmbFF)
            Me.Controls.Add(Me.m_cmbStanzaGroups)
            Me.Controls.Add(Me.m_cbFFecun)
            Me.Controls.Add(Me.m_zgc)
            Me.Controls.Add(Me.m_txtWmatWinf)
            Me.Controls.Add(Me.m_txtBAB)
            Me.Controls.Add(Me.m_txtRecPwr)
            Me.Controls.Add(Me.m_txtK)
            Me.Controls.Add(Me.m_lblFF)
            Me.Controls.Add(Me.m_lblWmatWinf)
            Me.Controls.Add(Me.m_lblBAB)
            Me.Controls.Add(Me.m_lblRecPwr)
            Me.Controls.Add(Me.m_lblK)
            Me.Controls.Add(Me.m_lblStanzaGroups)
            Me.Controls.Add(Me.m_grid)
            Me.Controls.Add(Me.m_btnCancel)
            Me.Controls.Add(Me.m_btnOK)
            Me.Controls.Add(Me.m_btnCalculate)
            Me.Name = "EditMultiStanza"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_cmbStanzaGroups As System.Windows.Forms.ComboBox
        Private WithEvents m_txtK As System.Windows.Forms.TextBox
        Private WithEvents m_txtRecPwr As System.Windows.Forms.TextBox
        Private WithEvents m_txtBAB As System.Windows.Forms.TextBox
        Private WithEvents m_txtWmatWinf As System.Windows.Forms.TextBox
        Private WithEvents m_cmbFF As System.Windows.Forms.ComboBox
        Private WithEvents m_zgc As ZedGraph.ZedGraphControl
        Private WithEvents m_cbFFecun As System.Windows.Forms.CheckBox
        Private WithEvents m_grid As gridEditMultiStanza
        Private WithEvents m_btnCalculate As System.Windows.Forms.Button
        Private WithEvents m_btnOK As System.Windows.Forms.Button
        Private WithEvents m_btnCancel As System.Windows.Forms.Button
        Private WithEvents m_lblStanzaGroups As System.Windows.Forms.Label
        Private WithEvents m_lblK As System.Windows.Forms.Label
        Private WithEvents m_lblRecPwr As System.Windows.Forms.Label
        Private WithEvents m_lblBAB As System.Windows.Forms.Label
        Private WithEvents m_lblWmatWinf As System.Windows.Forms.Label
        Private WithEvents m_lblFF As System.Windows.Forms.Label
        Private WithEvents m_hdrEcosim As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_hdrEcospace As ScientificInterfaceShared.Controls.cEwEHeaderLabel
        Private WithEvents m_cbEggAtSpawn As System.Windows.Forms.CheckBox
    End Class

End Namespace
