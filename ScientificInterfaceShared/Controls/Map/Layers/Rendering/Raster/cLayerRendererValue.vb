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
Imports ScientificInterfaceShared.Style

#End Region 'Imports

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer renderer that draws cells displaying the actual cell value number,
    ''' and scaling the cell background colour across a colour gradient based
    ''' on the cell value in relation to the layer min/max value range.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerRendererValue
        Inherits cRasterLayerRenderer

        Private m_bHasError As Boolean = False
        Private m_bHasNull As Boolean = False

        Private Enum eSymbolTypes As Integer
            None = 0
            [Null] = 1
            [Error] = 2
        End Enum


        Public Sub New(ByVal vs As cVisualStyle)
            MyBase.New(vs, cVisualStyle.eVisualStyleTypes.ForeColor Or
                    cVisualStyle.eVisualStyleTypes.Font Or
                    cVisualStyle.eVisualStyleTypes.Gradient)

        End Sub

        Protected Property Font As Font
        Protected Property ColorRamp As cColorRamp
        Protected Property ForeBrush As Brush

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether this attached layer should always rendered (True),
        ''' or only when the layer is <see cref="cDisplayLayer.IsSelected"/> (False).
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property DrawAlways() As Boolean

        Public Overrides Sub RenderPreview(ByVal g As Graphics,
                                           ByVal rc As Rectangle,
                                           Optional ByVal iSymbol As Integer = 0)

            If (Me.ForeBrush Is Nothing) Then Me.Update()

            Select Case DirectCast(iSymbol, eSymbolTypes)
                Case eSymbolTypes.None
                    cColorRampIndicator.DrawColorRamp(g, Me.ColorRamp, rc, False)
                Case eSymbolTypes.Null
                    Using br As New Drawing2D.HatchBrush(Drawing2D.HatchStyle.Percent25, Color.Gray, Color.Transparent)
                        g.FillRectangle(br, rc)
                    End Using
                Case eSymbolTypes.Error
                    Using br As New Drawing2D.HatchBrush(Drawing2D.HatchStyle.Percent25, Color.Red, Color.Transparent)
                        g.FillRectangle(br, rc)
                    End Using
                Case Else
                    Debug.Assert(False, "Unsupported symbol requested")
            End Select

        End Sub

        Public Overrides Sub RenderCell(ByVal g As Graphics,
                                        ByVal rc As Rectangle,
                                        ByVal layer As cEcospaceLayer,
                                        ByVal value As Object,
                                        ByVal style As cStyleGuide.eStyleFlags)

            Dim sValue As Single = CSng(value)

            ' Skip this if layer should not draw
            If ((style And cStyleGuide.eStyleFlags.Highlight) = 0) And (Me.DrawAlways = False) Then
                Return
            End If

            Me.m_bHasError = False
            Me.m_bHasNull = False

            If (Not cNumberUtils.IsFinite(sValue)) Or (sValue = cCore.NULL_VALUE) Or (sValue = 0 And Me.SuppressZero) Then
                Me.RenderPreview(g, rc, eSymbolTypes.Null)

                Me.m_bHasNull = True
                Me.InvalidateSymbols()

                Return
            End If

            Dim sValMax As Single = Me.ScaleMax
            Dim sValMin As Single = Me.ScaleMin

            If (sValMax = cCore.NULL_VALUE) Then sValMax = layer.MaxValue
            If (sValMin = cCore.NULL_VALUE) Then sValMin = layer.MinValue

            Dim bOutOfRange As Boolean = (sValue > sValMax)

            Try
                If Me.ForeBrush Is Nothing Then Me.Update()

                ' Draw background
                If (value IsNot Nothing) And (Me.Font IsNot Nothing) Then
                    If bOutOfRange Then

                        RenderPreview(g, rc, eSymbolTypes.Error)
                        Me.m_bHasError = True
                        Me.InvalidateSymbols()

                    Else

                        ' Render value on top for highlighted layers
                        Dim sValRange As Single = (sValMax - sValMin)

                        ' Has a range? draw background
                        If (sValRange > 0.0) Then
                            ' Calculate the cell color based on the cell value RELATIVE TO [sValueMin, sValueMax),
                            ' not (0, sValueMax)!!!
                            Using br As New SolidBrush(Me.ColorRamp.GetColor(sValue - sValMin, sValRange))
                                g.FillRectangle(br, rc)
                            End Using
                        Else
                            g.FillRectangle(Brushes.White, rc)
                        End If
                    End If
                End If
            Catch ex As Exception
                ' Boom
            End Try

        End Sub

        Public Overrides Sub Update()

            Dim vs As cVisualStyle = Me.VisualStyle

            If (vs Is Nothing) Then
                Me.ForeBrush = cRasterLayerRenderer.brDEFAULT
            Else
                Me.ForeBrush = New SolidBrush(vs.ForeColour)

                If (Me.Font IsNot Nothing) Then Me.Font.Dispose()
                Me.Font = New Font(vs.FontName, Me.VisualStyle.FontSize, Me.VisualStyle.FontStyle)

                If (vs.GradientBreaks IsNot Nothing) And (vs.GradientColors IsNot Nothing) Then
                    Me.ColorRamp = New cARGBColorRamp(vs.GradientColors, vs.GradientBreaks)
                Else
                    Me.ColorRamp = New cEwEColorRamp()
                End If
            End If

        End Sub

        Protected Overrides Function IsStyleValid() As Boolean
            If Not MyBase.IsStyleValid() Then Return False
            Return (Not String.IsNullOrEmpty(Me.VisualStyle.FontName) Or (Me.VisualStyle.FontSize > 1))
        End Function

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)
            Try
                Me.Font.Dispose()
                Me.Font = Nothing
                Me.ForeBrush.Dispose()
                Me.ForeBrush = Nothing
            Catch ex As Exception
                Debug.Assert(False)
            End Try
        End Sub

        Public Overrides Function GetDisplayText(value As Object) As String
            If (CSng(value) = cCore.NULL_VALUE) Then Return ""
            Return cStringUtils.FormatNumber(value)
        End Function

        Protected Overrides Sub UpdateSymbols()
            MyBase.UpdateSymbols()
            If Me.m_bHasNull Then Me.AddSymbol(My.Resources.GENERIC_VALUE_NO_DATA, eSymbolTypes.Null)
            If Me.m_bHasError Then Me.AddSymbol(My.Resources.GENERIC_VALUE_INVALID, eSymbolTypes.Error)
        End Sub

    End Class

End Namespace
