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

Imports ScientificInterfaceShared.Forms

Namespace Ecopath.Output

    Partial Class SizeWeightPlot
        Inherits frmEwE

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
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SizeWeightPlot))
            Me.zgcZedGraphCntl = New ZedGraph.ZedGraphControl()
            Me.SuspendLayout()
            '
            'zgcZedGraphCntl
            '
            resources.ApplyResources(Me.zgcZedGraphCntl, "zgcZedGraphCntl")
            Me.zgcZedGraphCntl.Name = "zgcZedGraphCntl"
            Me.zgcZedGraphCntl.ScrollGrace = 0.0R
            Me.zgcZedGraphCntl.ScrollMaxX = 0.0R
            Me.zgcZedGraphCntl.ScrollMaxY = 0.0R
            Me.zgcZedGraphCntl.ScrollMaxY2 = 0.0R
            Me.zgcZedGraphCntl.ScrollMinX = 0.0R
            Me.zgcZedGraphCntl.ScrollMinY = 0.0R
            Me.zgcZedGraphCntl.ScrollMinY2 = 0.0R
            '
            'SizeWeightPlot
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.Controls.Add(Me.zgcZedGraphCntl)
            Me.Name = "SizeWeightPlot"
            Me.TabText = ""
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents zgcZedGraphCntl As ZedGraph.ZedGraphControl
    End Class

End Namespace
