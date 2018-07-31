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

Imports ScientificInterfaceShared.Style
Imports EwEUtils.Core

Namespace Controls.Map.Layers

    Partial Class ucLayerEditorHabitat
        Inherits ucLayerEditorRange

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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucLayerEditorHabitat))
            Me.m_cbUseHabitatAreaCorrection = New System.Windows.Forms.CheckBox()
            Me.SuspendLayout()
            '
            'm_cbUseHabitatAreaCorrection
            '
            resources.ApplyResources(Me.m_cbUseHabitatAreaCorrection, "m_cbUseHabitatAreaCorrection")
            Me.m_cbUseHabitatAreaCorrection.Name = "m_cbUseHabitatAreaCorrection"
            Me.m_cbUseHabitatAreaCorrection.UseVisualStyleBackColor = True
            '
            'ucLayerEditorHabitat
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_cbUseHabitatAreaCorrection)
            Me.Name = "ucLayerEditorHabitat"
            Me.Controls.SetChildIndex(Me.m_cbUseHabitatAreaCorrection, 0)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private WithEvents m_cbUseHabitatAreaCorrection As CheckBox
    End Class

End Namespace
