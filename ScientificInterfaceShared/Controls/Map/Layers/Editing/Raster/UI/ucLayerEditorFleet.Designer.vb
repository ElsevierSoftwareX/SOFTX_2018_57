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
' Copyright 1991-2012 UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'
Namespace Controls.Map.Layers

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class ucLayerEditorFleet
        Inherits ucLayerEditor

        'UserControl overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucLayerEditorFleet))
            Me.m_lblFleet = New System.Windows.Forms.Label
            Me.m_cmbFleet = New System.Windows.Forms.ComboBox
            Me.SuspendLayout()
            '
            'm_lblFleet
            '
            resources.ApplyResources(Me.m_lblFleet, "m_lblFleet")
            Me.m_lblFleet.Name = "m_lblFleet"
            '
            'm_cmbFleet
            '
            resources.ApplyResources(Me.m_cmbFleet, "m_cmbFleet")
            Me.m_cmbFleet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbFleet.FormattingEnabled = True
            Me.m_cmbFleet.Name = "m_cmbFleet"
            '
            'ucLayerEditorFleet
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.m_cmbFleet)
            Me.Controls.Add(Me.m_lblFleet)
            Me.Name = "ucLayerEditorFleet"
            Me.Controls.SetChildIndex(Me.m_lblFleet, 0)
            Me.Controls.SetChildIndex(Me.m_cmbFleet, 0)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents m_lblFleet As System.Windows.Forms.Label
        Private WithEvents m_cmbFleet As System.Windows.Forms.ComboBox

    End Class

End Namespace
