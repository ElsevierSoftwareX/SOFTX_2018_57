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
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Style

#End Region 'Imports

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer renderer that draws cells as a wind indicator.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerRendererWindEwE5
        Inherits cLayerRendererValue

        Public Sub New(ByVal vs As cVisualStyle)
            MyBase.New(vs)
        End Sub

        Public Overrides Sub RenderPreview(ByVal g As Graphics,
                                           ByVal rc As Rectangle,
                                           Optional iSymbol As Integer = 0)
            If Me.IsStyleValid Then
                Dim sz As Integer = Math.Min(rc.Width, rc.Height)
                Dim rcSymbol As New Rectangle(rc.X + CInt((rc.Width - sz) / 2), rc.Y + CInt((rc.Height - sz) / 2), sz, sz)
                Me.RenderCell(g, rcSymbol, Nothing, New Single() {5, 5}, cStyleGuide.eStyleFlags.OK)
            Else
                Me.RenderError(g, rc)
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Draw the cell as an arrow with a given angle and scale.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="rc"></param>
        ''' <param name="value">A two-dimensional array of singles, 
        ''' holding the angle [0, 360] as the first index, and the scale
        ''' [0, 1] as the second index.</param>
        ''' <param name="style"></param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub RenderCell(ByVal g As Graphics,
                                        ByVal rc As Rectangle,
                                        ByVal layer As cEcospaceLayer,
                                        ByVal value As Object,
                                        ByVal style As cStyleGuide.eStyleFlags)

            Dim asValues As Single() = Nothing
            Dim ptfCenter As PointF = Nothing
            Dim szfHalfArrow As SizeF = Nothing
            Dim sMax As Single = 1
            Dim sScaleX As Single = 0.0!
            Dim sScaleY As Single = 0.0!

            If (layer IsNot Nothing) Then
                If (layer.MaxValue > 0) Then sMax = layer.MaxValue
            End If

            If TypeOf value Is Single() Then
                asValues = DirectCast(value, Single())
                If asValues.Length = 2 Then
                    If (asValues(0) <> cCore.NULL_VALUE And asValues(1) <> cCore.NULL_VALUE) Then

                        ' Calc display scale, rounded to two decimals between -1 and 1
                        Try
                            sScaleX = Math.Max(-1, Math.Min(cNumberUtils.FixValue(CSng(Math.Round(asValues(0) / sMax, 2))), 1))
                            sScaleY = Math.Max(-1, Math.Min(cNumberUtils.FixValue(CSng(Math.Round(asValues(1) / sMax, 2))), 1))

                            Dim sCol As Single = CSng(Math.Sqrt(sScaleX * sScaleX + sScaleY * sScaleY))
                            Dim clr As Color = Me.ColorRamp.GetColor(sCol)
                            cArrowIndicator.DrawArrowDxDy(g, clr, rc, sScaleX, sScaleY)

                        Catch ex As Exception
                            sScaleX = 0
                            sScaleY = 0
                        End Try
                    Else
                        ' Draw nothing
                    End If
                End If
            End If

        End Sub

        Protected Overrides Function IsStyleValid() As Boolean
            Return True
        End Function

        Public Overrides Function GetDisplayText(value As Object) As String

            Dim asValues As Single() = Nothing
            Dim sMax As Single = 1

            If TypeOf value Is Single() Then
                asValues = DirectCast(value, Single())
                If asValues.Length = 2 Then
                    Return String.Format(ScientificInterfaceShared.My.Resources.GENERIC_LABEL_DOUBLE,
                                         cStringUtils.FormatNumber(asValues(0)), cStringUtils.FormatNumber(asValues(1)))
                End If
            End If
            Return ""

        End Function

    End Class

End Namespace