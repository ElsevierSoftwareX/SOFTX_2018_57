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

    Partial Class dlgSelectAllFitsPlots
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgSelectAllFitsPlots))
            Me.OK_Button = New System.Windows.Forms.Button()
            Me.Cancel_Button = New System.Windows.Forms.Button()
            Me.clbAllPlots = New System.Windows.Forms.CheckedListBox()
            Me.btnCheckAll = New System.Windows.Forms.Button()
            Me.btnUnCheckAll = New System.Windows.Forms.Button()
            Me.SuspendLayout()
            '
            'OK_Button
            '
            resources.ApplyResources(Me.OK_Button, "OK_Button")
            Me.OK_Button.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.OK_Button.Name = "OK_Button"
            '
            'Cancel_Button
            '
            resources.ApplyResources(Me.Cancel_Button, "Cancel_Button")
            Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Cancel_Button.Name = "Cancel_Button"
            '
            'clbAllPlots
            '
            resources.ApplyResources(Me.clbAllPlots, "clbAllPlots")
            Me.clbAllPlots.CheckOnClick = True
            Me.clbAllPlots.FormattingEnabled = True
            Me.clbAllPlots.Name = "clbAllPlots"
            '
            'btnCheckAll
            '
            resources.ApplyResources(Me.btnCheckAll, "btnCheckAll")
            Me.btnCheckAll.Name = "btnCheckAll"
            Me.btnCheckAll.UseVisualStyleBackColor = True
            '
            'btnUnCheckAll
            '
            resources.ApplyResources(Me.btnUnCheckAll, "btnUnCheckAll")
            Me.btnUnCheckAll.Name = "btnUnCheckAll"
            Me.btnUnCheckAll.UseVisualStyleBackColor = True
            '
            'dlgSelectAllFitsPlots
            '
            Me.AcceptButton = Me.OK_Button
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.clbAllPlots)
            Me.Controls.Add(Me.Cancel_Button)
            Me.Controls.Add(Me.OK_Button)
            Me.Controls.Add(Me.btnUnCheckAll)
            Me.Controls.Add(Me.btnCheckAll)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgSelectAllFitsPlots"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.ResumeLayout(False)

        End Sub

        Friend WithEvents OK_Button As System.Windows.Forms.Button
        Friend WithEvents Cancel_Button As System.Windows.Forms.Button
        Friend WithEvents clbAllPlots As System.Windows.Forms.CheckedListBox
        Friend WithEvents btnCheckAll As System.Windows.Forms.Button
        Friend WithEvents btnUnCheckAll As System.Windows.Forms.Button
    End Class

End Namespace


