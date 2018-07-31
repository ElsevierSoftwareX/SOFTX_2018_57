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
Imports EwECore
Imports EwECore.Auxiliary
Imports ScientificInterfaceShared.Style

#End Region 'Imports

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer renderer that draws cells as a coloured symbol, using the 
    ''' foreground colour in the attaced <see cref="cLayerRenderer.VisualStyle">visual style</see>
    ''' to fill the symbol.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerRendererSymbol
        Inherits cRasterLayerRenderer

        Public Sub New(ByVal vs As cVisualStyle)
            MyBase.New(vs, cVisualStyle.eVisualStyleTypes.ForeColor)
        End Sub

        Public Overrides Sub RenderPreview(ByVal g As Graphics,
                                            ByVal rc As Rectangle,
                                            Optional iSymbol As Integer = 0)

            Me.RenderSymbol(g, rc, Me.VisualStyle.ForeColour)

        End Sub

        Public Overrides Sub RenderCell(ByVal g As System.Drawing.Graphics,
                                        ByVal rc As System.Drawing.Rectangle,
                                        ByVal layer As cEcospaceLayer,
                                        ByVal value As Object,
                                        ByVal style As cStyleGuide.eStyleFlags)

            If (CBool(value)) Then Me.RenderPreview(g, rc)

        End Sub

        Public Overrides Function GetDisplayText(value As Object) As String
            Return ""
        End Function

        Protected Overrides Function IsStyleValid() As Boolean
            Return True
        End Function

#Region " Internals "

        Protected Sub RenderSymbol(ByVal g As Graphics,
                                   ByVal rc As Rectangle,
                                   ByVal colorFill As Color)
            If Me.IsStyleValid() Then
                rc.Inflate(CInt(-rc.Width * 0.1), CInt(-rc.Height * 0.1))

                ' JS 05Sep16: center symbol
                Dim sz As Integer = Math.Min(rc.Width, rc.Height)
                Dim rcSymbol As New Rectangle(rc.X + CInt((rc.Width - sz) / 2), rc.Y + CInt((rc.Height - sz) / 2), sz, sz)

                Using p As New Pen(Color.White, 3)
                    g.DrawEllipse(p, rcSymbol)
                End Using
                Using br As New SolidBrush(colorFill)
                    g.FillEllipse(br, rcSymbol)
                End Using
                g.DrawEllipse(Pens.Black, rcSymbol)
            Else
                Me.RenderError(g, rc)
            End If
        End Sub

#End Region ' Internals

    End Class

End Namespace