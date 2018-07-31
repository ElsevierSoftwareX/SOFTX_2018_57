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
Imports System.Drawing.Drawing2D
Imports EwECore
Imports EwECore.Auxiliary
Imports ScientificInterfaceShared.Style

#End Region 'Imports

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer renderer that draws cells interpreting the cell value as index 
    ''' to the .NET provided <see cref="HatchStyle">hatch patterns</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerRendererHatch
        Inherits cRasterLayerRenderer

        Public Sub New(ByVal vs As cVisualStyle)
            MyBase.New(vs, cVisualStyle.eVisualStyleTypes.ForeColor Or cVisualStyle.eVisualStyleTypes.BackColor Or cVisualStyle.eVisualStyleTypes.Hatch)
        End Sub

        Public Overrides Sub RenderPreview(ByVal g As Graphics, _
                                           ByVal rc As Rectangle,
                                           Optional ByVal iSymbol As Integer = 0)
            If Me.IsStyleValid Then
                Using br As New HatchBrush(Me.VisualStyle.HatchStyle, Me.VisualStyle.ForeColour, Me.VisualStyle.BackColour)
                    g.FillRectangle(br, rc)
                End Using
            Else
                Me.RenderError(g, rc)
            End If
        End Sub

        Public Overrides Sub RenderCell(ByVal g As System.Drawing.Graphics, _
                                        ByVal rc As System.Drawing.Rectangle, _
                                        ByVal layer As cEcospaceLayer, _
                                        ByVal value As Object, _
                                        ByVal style As cStyleGuide.eStyleFlags)
            Me.RenderPreview(g, rc)
        End Sub

        Protected Overrides Function IsStyleValid() As Boolean
            Return ((Me.VisualStyle.HatchStyle > 0) And (CInt(Me.VisualStyle.HatchStyle) < [Enum].GetValues(GetType(System.Drawing.Drawing2D.HatchStyle)).Length))
        End Function

        Public Overrides Function GetDisplayText(value As Object) As String
            Return ""
        End Function

    End Class

End Namespace
