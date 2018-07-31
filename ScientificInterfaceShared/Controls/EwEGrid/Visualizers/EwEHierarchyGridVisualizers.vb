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

Option Strict On

Namespace Controls.EwEGrid

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' EwEParentRowHeaderVisualizer implements a EwERowHeaderVisualizer visualizer
    ''' for rendering EwE hierarchical parent row header cells
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)> _
     Public Class cVisualizerEwECollapseExpandRowHeader
        : Inherits cEwEGridRowHeaderVisualizer

        Public Enum eCollapsedState As Integer
            NoChildren = 0
            Collapsed
            Expanded
        End Enum

        Public Sub New()
            MyBase.new()
            Me.ImageAlignment = ContentAlignment.MiddleCenter
        End Sub

        Public Sub SetCollapsedState(ByVal state As eCollapsedState)
            Select Case state
                Case eCollapsedState.Collapsed
                    Me.Image = My.Resources.Collapsed
                Case eCollapsedState.Expanded
                    Me.Image = My.Resources.Expanded
                Case eCollapsedState.NoChildren
                    Me.Image = Nothing
            End Select
        End Sub

    End Class

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' EwEParentRowHeaderVisualizer implements a EwERowHeaderVisualizer visualizer
    ''' for rendering EwE hierarchical parent row header cells
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)> _
     Public Class cVisualizerEwEParentRowHeader
        : Inherits cEwEGridRowHeaderVisualizer

        Public Sub New()
            MyBase.new()
        End Sub

    End Class

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' EwEChildRowHeaderVisualizer implements a EwERowHeaderVisualizer visualizer
    ''' for rendering EwE hierarchical child row header cells
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)> _
     Public Class cVisualizerEwEChildRowHeader
        : Inherits cEwEGridRowHeaderVisualizer

        ''' <summary>Size of label indentation</summary>
        Private Const cINDENT_SIZE As Integer = 20

        Public Sub New()
            MyBase.new()
            Me.Indentation = cVisualizerEwEChildRowHeader.cINDENT_SIZE
        End Sub

    End Class

End Namespace
