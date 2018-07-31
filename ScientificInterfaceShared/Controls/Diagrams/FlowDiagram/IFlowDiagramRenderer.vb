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

Imports ScientificInterfaceShared.Definitions

Namespace Controls

    ' ToDo_JS: document this interface

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base interface for drawing a specific type of flow diagram.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface IFlowDiagramRenderer

        Enum eFDHighlightType As Integer
            None
            Selected
            LinkIn
            LinkOut
            GrayedOut
            Invisible
        End Enum

        ''' <summary>Draw the background of the flow diagram. Here trophic level lines etc. should be rendered.</summary>
        ''' <param name="g"></param>
        ''' <param name="rc"></param>
        Sub DrawBackground(ByVal g As Graphics, ByVal rc As Rectangle)
        Sub DrawTitle(ByVal g As Graphics, ByVal rc As Rectangle)
        Sub DrawNode(ByVal g As Graphics, ByVal rc As Rectangle, ByVal iGroup As Integer, ByVal highlight As eFDHighlightType)
        Sub DrawConnection(ByVal g As Graphics, ByVal rc As Rectangle, ByVal iPred As Integer, ByVal iPrey As Integer, ByVal highlight As eFDHighlightType)
        Sub DrawLegend(ByVal g As Graphics, ByVal ptTopLeft As Point)

        Function RenderFont() As Font
        Function TextColor() As Color
        Function InLinkColor() As Color
        Function OutLinkColor() As Color
        Function HighlightColor() As Color
        Function FormatLabelText(iGroup As Integer) As String
        Property NodeLocation(ByVal i As Integer, ByVal rc As Rectangle) As PointF
        Property LabelLocation(ByVal i As Integer, ByVal rc As Rectangle) As PointF
        Property ShowHiddenNodes As eFDShowHiddenType

        Sub MoveNode(ByVal rc As Rectangle, ByVal ptNew As PointF, ByVal iNode As Integer)
        Sub MoveLabel(ByVal rc As Rectangle, ByVal ptNew As PointF, ByVal iNode As Integer)
        Function IsNodeAtPoint(ByVal rc As Rectangle, ByVal ptfTest As PointF, ByVal i As Integer, ByVal sValue As Single) As Boolean
        Function IsLabelAtPoint(ByVal rc As Rectangle, ByVal ptfTest As PointF, ByVal i As Integer, ByVal strLabel As String, ByVal g As Graphics, ByVal font As Font) As Boolean

    End Interface

End Namespace