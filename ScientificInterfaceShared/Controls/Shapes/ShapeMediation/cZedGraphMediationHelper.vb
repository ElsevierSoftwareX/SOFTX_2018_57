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

Option Explicit On
Option Strict On

Imports EwECore
Imports ZedGraph
Imports System.ComponentModel
Imports ScientificInterfaceShared.Style

#End Region

Namespace Controls

    ''' <summary>
    ''' Derived Zedgraph helper class that just overrides the ToolTip formating for the EnvironmentalResponse graphs
    ''' </summary>
    ''' <remarks></remarks>
    <CLSCompliant(False)>
    Public Class cZedGraphMediationHelper
        Inherits cZedGraphHelper

        Public Enum eEnvResponseLineType As Integer
            Response
            Histogram
        End Enum

        Public Shadows Function CreateLineItem(ByVal strName As String, ByVal ppl As ZedGraph.PointPairList, ByVal lineType As eEnvResponseLineType) As ZedGraph.LineItem
            Dim clr As Color
            Select Case lineType
                Case eEnvResponseLineType.Histogram : clr = Color.Gray
                Case eEnvResponseLineType.Response : clr = Color.SandyBrown
                Case Else : Debug.Assert(False)
            End Select
            Return MyBase.CreateLineItem(strName, Definitions.eSketchDrawModeTypes.Line, clr, ppl, lineType)
        End Function

        Protected Overrides Function FormatTooltip(ByVal pane As ZedGraph.GraphPane, ByVal curve As ZedGraph.CurveItem, ByVal iPoint As Integer) As String

            ' ToDo: localize this

            'This is not a very good way to do this 
            'It may be better to not use a tool tip at all 
            'instead pass out the X and Y Axis value(s) and let the container figure out how to show the data
            Try

                Dim bUseBase As Boolean = True

                If curve.Tag IsNot Nothing Then
                    If TypeOf curve.Tag Is cCurveInfo Then
                        Dim ci As cCurveInfo = DirectCast(curve.Tag, cCurveInfo)
                        Dim tag As eEnvResponseLineType = DirectCast(ci.Tag, eEnvResponseLineType)

                        Select Case tag
                            Case eEnvResponseLineType.Response
                                bUseBase = False
                            Case eEnvResponseLineType.Histogram
                                Return ""
                            Case Else
                                Debug.Assert(False, "Unsupported line type")
                        End Select
                    End If ' If TypeOf curve.Tag Is cCurveInfo Then
                End If ' If curve.Tag IsNot Nothing Then

                If bUseBase Then
                    Return MyBase.FormatTooltip(pane, curve, iPoint)
                End If

                Debug.Assert(curve.IsLine, "ToolTip wrong line type.")

                ' ToDo: localize this
                Dim sb As New System.Text.StringBuilder()
                sb.AppendLine("Capacity for Map input.")

                Dim pp As PointPair = curve(iPoint)
                sb.AppendLine("Map input " & Me.StyleGuide.FormatNumber(pp.X))
                sb.AppendLine("Capacity multiplier" & Me.StyleGuide.FormatNumber(pp.Y))
                Return sb.ToString
            Catch ex As Exception

            End Try
            Return ""

        End Function

    End Class

End Namespace
