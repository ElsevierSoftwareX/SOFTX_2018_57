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
' Copyright 1991- UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwECore
Imports EwECore.Auxiliary
Imports ScientificInterfaceShared.Style
Imports EwEUtils.Utilities

#End Region 'Imports

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer renderer that draws cells as an arrow with a specific direction
    ''' and scale. The cell value to render provided in 
    ''' <see cref="cLayerRendererArrow.RenderCell">RenderCell</see> should hold a
    ''' two-dimensional array describing these arrow properties.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerRendererArrow
        Inherits cRasterLayerRenderer

        Public Sub New(ByVal vs As cVisualStyle)
            MyBase.New(vs, cVisualStyle.eVisualStyleTypes.ForeColor)
        End Sub

        Public Overrides Sub RenderPreview(ByVal g As Graphics, _
                                           ByVal rc As Rectangle)
            If Me.IsStyleValid Then
                'g.FillRectangle(Brushes.White, rc)
                Me.RenderCell(g, rc, Nothing, 45, cStyleGuide.eStyleFlags.OK)
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
        Public Overrides Sub RenderCell(ByVal g As Graphics, _
                                        ByVal rc As Rectangle, _
                                        ByVal layer As cEcospaceLayer, _
                                        ByVal value As Object, _
                                        ByVal style As cStyleGuide.eStyleFlags)

            Dim asValues As Single() = Nothing
            Dim sAngle As Single = 0
            Dim sScale As Single = 1.0

            ' Value should be an array of two singles:
            '   value(0) = arrow angle [0, 360]
            '   value(1) = arrow scale [0, 1]

            If TypeOf value Is Single() Then
                asValues = DirectCast(value, Single())
                If asValues.Length > 0 Then
                    sAngle = asValues(0)
                    If asValues.Length > 1 Then
                        sScale = asValues(1)
                    End If
                End If
            End If

            cArrowIndicator.DrawArrow(g, Me.VisualStyle.ForeColour, rc, sAngle, sScale)
        End Sub

        Protected Overrides Function IsStyleValid() As Boolean
            Return True
        End Function

        Public Overrides Function GetDisplayText(value As Object) As String

            Dim asValues As Single() = Nothing
            Dim sAngle As Single = 0
            Dim sScale As Single = 1.0

            If TypeOf value Is Single() Then
                asValues = DirectCast(value, Single())
                If asValues.Length > 0 Then
                    sAngle = asValues(0)
                    If asValues.Length > 1 Then
                        sScale = asValues(1)
                    End If
                End If

                Return String.Format(ScientificInterfaceShared.My.Resources.GENERIC_LABEL_DOUBLE, _
                                     cStringUtils.FormatNumber(sAngle), cStringUtils.FormatNumber(sScale))
            End If
            Return ""

        End Function

    End Class

End Namespace