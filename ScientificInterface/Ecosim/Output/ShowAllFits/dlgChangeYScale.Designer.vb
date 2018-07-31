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

    Partial Class dlgChangeYScale
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgChangeYScale))
            Me.m_btnOK = New System.Windows.Forms.Button()
            Me.m_btnReset = New System.Windows.Forms.Button()
            Me.m_lbAllPlots = New System.Windows.Forms.ListBox()
            Me.m_lblSelPlotName = New System.Windows.Forms.Label()
            Me.m_txbSelPlotName = New System.Windows.Forms.TextBox()
            Me.m_lblYScale = New System.Windows.Forms.Label()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_nudYScale = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
            CType(Me.m_nudYScale, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'm_btnOK
            '
            resources.ApplyResources(Me.m_btnOK, "m_btnOK")
            Me.m_btnOK.Name = "m_btnOK"
            '
            'm_btnReset
            '
            resources.ApplyResources(Me.m_btnReset, "m_btnReset")
            Me.m_btnReset.Name = "m_btnReset"
            '
            'm_lbAllPlots
            '
            resources.ApplyResources(Me.m_lbAllPlots, "m_lbAllPlots")
            Me.m_lbAllPlots.FormattingEnabled = True
            Me.m_lbAllPlots.Name = "m_lbAllPlots"
            '
            'm_lblSelPlotName
            '
            resources.ApplyResources(Me.m_lblSelPlotName, "m_lblSelPlotName")
            Me.m_lblSelPlotName.Name = "m_lblSelPlotName"
            '
            'm_txbSelPlotName
            '
            resources.ApplyResources(Me.m_txbSelPlotName, "m_txbSelPlotName")
            Me.m_txbSelPlotName.Name = "m_txbSelPlotName"
            Me.m_txbSelPlotName.ReadOnly = True
            '
            'm_lblYScale
            '
            resources.ApplyResources(Me.m_lblYScale, "m_lblYScale")
            Me.m_lblYScale.Name = "m_lblYScale"
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            '
            'm_nudYScale
            '
            Me.m_nudYScale.DecimalPlaces = 3
            Me.m_nudYScale.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
            resources.ApplyResources(Me.m_nudYScale, "m_nudYScale")
            Me.m_nudYScale.Name = "m_nudYScale"
            '
            'dlgChangeYScale
            '
            Me.AcceptButton = Me.m_btnOK
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnCancel
            Me.Controls.Add(Me.m_nudYScale)
            Me.Controls.Add(Me.m_btnCancel)
            Me.Controls.Add(Me.m_btnOK)
            Me.Controls.Add(Me.m_btnReset)
            Me.Controls.Add(Me.m_lblYScale)
            Me.Controls.Add(Me.m_txbSelPlotName)
            Me.Controls.Add(Me.m_lblSelPlotName)
            Me.Controls.Add(Me.m_lbAllPlots)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "dlgChangeYScale"
            Me.ShowInTaskbar = False
            CType(Me.m_nudYScale, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Friend WithEvents m_btnOK As System.Windows.Forms.Button
        Friend WithEvents m_btnReset As System.Windows.Forms.Button
        Friend WithEvents m_lbAllPlots As System.Windows.Forms.ListBox
        Friend WithEvents m_lblSelPlotName As System.Windows.Forms.Label
        Friend WithEvents m_txbSelPlotName As System.Windows.Forms.TextBox
        Friend WithEvents m_lblYScale As System.Windows.Forms.Label
        Friend WithEvents m_btnCancel As System.Windows.Forms.Button
        Friend WithEvents m_nudYScale As ScientificInterfaceShared.Controls.cEwENumericUpDown

    End Class

End Namespace

