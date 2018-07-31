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

Namespace Controls


    Partial Class ucMediationAssignments
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucMediationAssignments))
            Me.m_zedgraph = New ZedGraph.ZedGraphControl()
            Me.SuspendLayout()
            '
            'm_zedgraph
            '
            resources.ApplyResources(Me.m_zedgraph, "m_zedgraph")
            Me.m_zedgraph.Name = "m_zedgraph"
            Me.m_zedgraph.ScrollGrace = 0.0R
            Me.m_zedgraph.ScrollMaxX = 0.0R
            Me.m_zedgraph.ScrollMaxY = 0.0R
            Me.m_zedgraph.ScrollMaxY2 = 0.0R
            Me.m_zedgraph.ScrollMinX = 0.0R
            Me.m_zedgraph.ScrollMinY = 0.0R
            Me.m_zedgraph.ScrollMinY2 = 0.0R
            '
            'ucMediationAssignments
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.m_zedgraph)
            Me.Name = "ucMediationAssignments"
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents m_zedgraph As ZedGraph.ZedGraphControl

    End Class

End Namespace



