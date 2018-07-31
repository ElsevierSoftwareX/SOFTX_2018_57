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
Imports EwEUtils.Utilities

#End Region 'Imports

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer renderer that draws cells displaying the actual cell value number,
    ''' and scaling the cell background colour across a colour gradient based
    ''' on the cell value in relation to the layer min/max value range.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerRendererDepth
        Inherits cLayerRendererValue

        Public Sub New(ByVal vs As cVisualStyle)
            MyBase.New(vs)
        End Sub

        'Public Overrides Sub RenderPreview(ByVal g As Graphics, _
        '                                   ByVal rc As Rectangle)
        '    MyBase.RenderPreview(g, rc)
        '    If Me.ForeBrush Is Nothing Then Me.Update()
        '    g.DrawString("#", Me.Font, Me.ForeBrush, rc)
        'End Sub

        Public Overrides Sub RenderCell(ByVal g As System.Drawing.Graphics, _
                                        ByVal rc As System.Drawing.Rectangle, _
                                        ByVal layer As cEcospaceLayer, _
                                        ByVal value As Object, _
                                        ByVal style As cStyleGuide.eStyleFlags)

            Try
                Dim sValue As Single = CSng(value)
                Dim sValueMax As Single = layer.MaxValue
                Dim sValueMin As Single = layer.MinValue

                ' Is non-water cell?
                If (sValue <= 0) Then
                    ' #Yes: draw in black
                    g.FillRectangle(Brushes.Gray, rc)
                Else
                    ' #No: only draw colours when highlighted

                    ' Highlighted? draw values in colour + value on top
                    If ((style And cStyleGuide.eStyleFlags.Highlight) = cStyleGuide.eStyleFlags.Highlight) Then

                        If (Me.ForeBrush Is Nothing) Then Me.Update()

                        If (value IsNot Nothing) And (Me.Font IsNot Nothing) Then
                            ' Calculate the cell color based on the cell value RELATIVE TO [0, sValueMax),
                            Using br As New SolidBrush(Me.ColorRamp.GetColor(sValue - sValueMin, sValueMax - sValueMin))
                                g.FillRectangle(br, rc)
                            End Using
                        End If
                    End If
                End If

            Catch ex As Exception
                ' Boom
            End Try
        End Sub

        Public Overrides Function GetDisplayText(value As Object) As String
            Return cStringUtils.FormatNumber(value)
        End Function

    End Class

End Namespace
