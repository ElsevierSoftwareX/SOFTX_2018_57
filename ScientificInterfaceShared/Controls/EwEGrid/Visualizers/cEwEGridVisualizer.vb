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

#Region " Imports "

Option Strict On
Imports System.Windows.Forms
Imports System.Drawing
Imports EwECore
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports SourceGrid2.Cells.Real
Imports SourceGrid2.VisualModels

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' A EwE Grid base visualizer that aligns its content.
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class cEwECellVisualizer
        : Inherits cEwEGridVisualizerBase

        ''' <summary>
        ''' Create a new visualizer.
        ''' </summary>
        ''' <param name="alignment">Alignment to choose. If not specified content will
        ''' be aligned <see cref="ContentAlignment.MiddleCenter"/>.</param>
        Public Sub New(Optional ByVal alignment As ContentAlignment = ContentAlignment.MiddleCenter)
            MyBase.New()
            Me.TextAlignment = alignment
            Me.AlignTextToImage = True
            Me.WordWrap = False
            Me.AlignTextToImage = True
        End Sub

    End Class

End Namespace