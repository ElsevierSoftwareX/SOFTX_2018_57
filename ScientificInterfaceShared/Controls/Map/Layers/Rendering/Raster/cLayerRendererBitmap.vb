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
Imports System.Drawing.Imaging

#End Region 'Imports

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer renderer that draws cells as a bitmap, provided in the attached
    ''' <see cref="cLayerRenderer.VisualStyle">visual style</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerRendererBitmap
        Inherits cRasterLayerRenderer

        Public Sub New(ByVal vs As cVisualStyle)
            MyBase.New(vs, cVisualStyle.eVisualStyleTypes.Image)
        End Sub

        Public Overrides Sub RenderPreview(ByVal g As Graphics,
                                            ByVal rc As Rectangle,
                                            Optional iSymbol As Integer = 0)
            If (Me.IsStyleValid) Then
                Me.DrawImageAlpha(g, rc, Me.VisualStyle.Image, 1.0!)
            Else
                Me.RenderError(g, rc)
            End If
        End Sub

        Public Overrides Sub RenderCell(ByVal g As System.Drawing.Graphics, _
                                        ByVal rc As System.Drawing.Rectangle, _
                                        ByVal layer As cEcospaceLayer, _
                                        ByVal value As Object, _
                                        ByVal style As cStyleGuide.eStyleFlags)
            If (Me.IsStyleValid) Then
                Me.DrawImageAlpha(g, rc, Me.VisualStyle.Image, Math.Min(1, Math.Max(0, CSng(value))))
            Else
                Me.RenderError(g, rc)
            End If
        End Sub

        Protected Overrides Function IsStyleValid() As Boolean
            Return (Me.VisualStyle.Image IsNot Nothing)
        End Function

        Public Overrides Function Clone() As cRasterLayerRenderer
            Dim objClone As Object = Nothing
            Dim vs As cVisualStyle = Me.VisualStyle.Clone()

            objClone = Activator.CreateInstance(Me.GetType(), New Object() {vs})
            Return DirectCast(objClone, cRasterLayerRenderer)
        End Function

        Private Sub DrawImageAlpha(ByVal g As Graphics, ByVal rc As Rectangle, ByVal img As Image, ByVal sAlpha As Single)

            If sAlpha >= 1 Then
                Using br As New TextureBrush(img, WrapMode.Tile)
                    g.FillRectangle(br, rc)
                End Using
            Else
                Dim matrixItems As Single()() = { _
                    New Single() {1, 0, 0, 0, 0}, _
                    New Single() {0, 1, 0, 0, 0}, _
                    New Single() {0, 0, 1, 0, 0}, _
                    New Single() {0, 0, 0, sAlpha, 0}, _
                    New Single() {0, 0, 0, 0, 1}}

                Dim colorMatrix As New ColorMatrix(matrixItems)
                Dim imageAtt As New ImageAttributes()
                imageAtt.SetColorMatrix( _
                   colorMatrix, _
                   ColorMatrixFlag.Default, _
                   ColorAdjustType.Bitmap)

                Using br As New TextureBrush(img, New Rectangle(0, 0, img.Width, img.Height), imageAtt)
                    br.WrapMode = WrapMode.Tile
                    g.FillRectangle(br, rc)
                End Using
            End If

        End Sub

        Public Overrides Function GetDisplayText(value As Object) As String
            Return Convert.ToString(value)
        End Function

    End Class

End Namespace
