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
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports ScientificInterfaceShared.Style
Imports EwEUtils.Utilities

#End Region 'Imports

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer renderer that draws cells as a wind indicator.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerRendererUpwelling
        Inherits cRasterLayerRenderer

        Public Sub New(ByVal vs As cVisualStyle)
            MyBase.New(vs, cVisualStyle.eVisualStyleTypes.ForeColor)
        End Sub

        Public Overrides Sub RenderCell(ByVal g As Graphics, _
                                        ByVal rc As Rectangle, _
                                        ByVal layer As cEcospaceLayer, _
                                        ByVal value As Object, _
                                        ByVal style As ScientificInterfaceShared.Style.cStyleGuide.eStyleFlags)

            'Cl2 = 0.01 / CellLength ' ^ 2
            'UpVel(i, j) = UpLoc  'Added for this model  SM.
            'Up.Circle (j + 0.5, i + 0.5 - UpLoc / UpMax), 0.1
            'Up.Line (j + 0.5, i + 0.5)-Step(0, -UpLoc / UpMax)

            Dim ptfCenter As PointF = Nothing
            Dim sHalfArrow As Single = Nothing
            Dim sValue As Single = 0.0!
            Dim sMax As Single = 1.0!
            Dim iR As Integer = 0
            Dim iG As Integer = 0
            Dim iB As Integer = 0

            If (layer IsNot Nothing) Then
                If (layer.MaxValue > 0.0!) Then sMax = layer.MaxValue
            End If

            If TypeOf value Is Single Then
                ' Get value to render
                sValue = -CSng(value)

                ' Calc cell center
                ptfCenter = New PointF(CSng(rc.X + rc.Width / 2), CSng(rc.Y + rc.Height / 2))
                ' Calc arrow size
                sHalfArrow = rc.Height * sValue / (2 * sMax)

                ' Has a value to draw?
                If (sValue <> 0.0!) Then
                    ' #Yes: render a Green (up) or Blue (down) upwelling arrow
                    iG = If(sValue > 0, 150, 0)
                    iB = If(sValue > 0, 0, 150)
                    Using p As New Pen(Color.FromArgb(255, iR, iG, iB), 0.001!)
                        g.DrawLine(p, _
                                   ptfCenter.X, ptfCenter.Y - sHalfArrow, _
                                   ptfCenter.X, ptfCenter.Y + sHalfArrow)
                        g.DrawEllipse(p, _
                                      ptfCenter.X - rc.Width / 8.0!, _
                                      ptfCenter.Y + sHalfArrow - rc.Height / 8.0!, _
                                      rc.Width / 4.0!, rc.Height / 4.0!)
                    End Using
                Else
                    Using p As New Pen(Me.VisualStyle.ForeColour, 0.001!)
                        g.DrawLine(p, _
                                   ptfCenter.X - rc.Width / 4.0!, ptfCenter.Y, _
                                   ptfCenter.X + rc.Width / 4.0!, ptfCenter.Y)
                    End Using
                End If



            End If

        End Sub

        Public Overrides Sub RenderPreview(ByVal g As Graphics,
                                           ByVal rc As Rectangle,
                                           Optional ByVal iSymbol As Integer = 0)

            If Me.IsStyleValid Then
                Me.RenderCell(g, rc, Nothing, 1.0!, cStyleGuide.eStyleFlags.OK)
            Else
                Me.RenderError(g, rc)
            End If

        End Sub

        Public Overrides Function GetDisplayText(value As Object) As String
            If (CSng(value) = cCore.NULL_VALUE) Then Return ""
            Return cStringUtils.FormatNumber(value)
        End Function

    End Class

End Namespace
