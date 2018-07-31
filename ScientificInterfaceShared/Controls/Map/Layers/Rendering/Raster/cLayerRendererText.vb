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
    ''' Layer renderer that draws cells values directly as text. The cell 
    ''' background colour is obtained from the visual style.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerRendererText
        Inherits cRasterLayerRenderer

        Private m_brFore As Brush = Nothing
        Private m_ft As Font = Nothing

        Public Sub New(ByVal vs As cVisualStyle)
            MyBase.New(vs, cVisualStyle.eVisualStyleTypes.ForeColor Or _
                    cVisualStyle.eVisualStyleTypes.Font Or _
                    cVisualStyle.eVisualStyleTypes.Gradient)
        End Sub

        Public Overrides Sub RenderPreview(ByVal g As Graphics,
                                           ByVal rc As Rectangle,
                                           Optional iSymbol As Integer = 0)

            If Me.m_brFore Is Nothing Then Me.Update()

            If Me.IsStyleValid Then
                g.FillRectangle(Brushes.White, rc)
                g.DrawString("Aa", Me.m_ft, Me.m_brFore, rc)
            Else
                Me.RenderError(g, rc)
            End If

        End Sub

        Public Overrides Sub RenderCell(ByVal g As System.Drawing.Graphics, _
                                        ByVal rc As System.Drawing.Rectangle, _
                                        ByVal layer As cEcospaceLayer, _
                                        ByVal value As Object, _
                                        ByVal style As cStyleGuide.eStyleFlags)

            Try
                If Me.m_brFore Is Nothing Then Me.Update()

                ' Draw background
                ' Render value on top for highlighted layers
                If ((style And cStyleGuide.eStyleFlags.Highlight) = cStyleGuide.eStyleFlags.Highlight) Then

                    If (value IsNot Nothing) And (Me.m_ft IsNot Nothing) Then
                        Dim strValue As String = CStr(value)
                        Using br As New SolidBrush(Me.VisualStyle.BackColour)
                            g.FillRectangle(br, rc)
                        End Using
                        ' Draw value
                        g.DrawString(strValue, Me.m_ft, Me.m_brFore, rc)

                    End If
                End If
            Catch ex As Exception
                ' Boom
            End Try
        End Sub

        Public Overrides Sub Update()
            If Me.VisualStyle Is Nothing Then
                Me.m_brFore = cRasterLayerRenderer.brDEFAULT
            Else
                Me.m_brFore = New SolidBrush(Me.VisualStyle.ForeColour)
                Me.m_ft = New Font(Me.VisualStyle.FontName, Me.VisualStyle.FontSize, Me.VisualStyle.FontStyle)
            End If
        End Sub

        Protected Overrides Function IsStyleValid() As Boolean
            If Not MyBase.IsStyleValid() Then Return False
            Return (Not String.IsNullOrEmpty(Me.VisualStyle.FontName) Or (Me.VisualStyle.FontSize > 1))
        End Function

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)
            Me.m_ft.Dispose()
            Me.m_ft = Nothing
            Me.m_brFore.Dispose()
            Me.m_brFore = Nothing
        End Sub

        Public Overrides Function GetDisplayText(value As Object) As String
            If String.IsNullOrWhiteSpace(CStr(value)) Then Return ""
            Return CStr(value)
        End Function

    End Class

End Namespace