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
Option Explicit On

Imports System.Threading
Imports EwECore
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.Map

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Blunt class for rendering map data onto a graphics area.
    ''' </summary>
    ''' <remarks>
    ''' This class needs some thorough revisioning!
    ''' - Cannot draw fleet information yet. Suggest to split cMapDrawer in cGroupMapDrawer, cFleetMapDrawer; make cMapDrawer abstract class
    ''' - Drawer class should be cleaned up
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Public MustInherit Class cMapDrawerBase

        Public Enum eMapType As Integer
            RelBiomass = 0
            RelCatch
            FishingMortRate
            RelContam
            ContamRate
            Discards
        End Enum

#Region " Private vars "

        Protected m_SignalState As New ManualResetEvent(True)
        Protected Const MAX_FISH_MORT As Single = 2
        Protected m_core As cCore = Nothing
        Protected m_sg As cStyleGuide = Nothing
        Protected m_lItems As New List(Of Integer)
        Protected m_lLocations As New List(Of Integer)

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal core As cCore, ByVal sg As cStyleGuide)
            Me.m_core = core
            Me.m_sg = sg
            Me.AllowedToRun = True
            Me.ShowLand = True
            Me.ShowBorder = True
            Me.ShowExcluded = False
        End Sub

#End Region ' Constructor

#Region " Public properties "

        Public Property AllowedToRun() As Boolean
        Public Property ShowLand() As Boolean
        Public Property ShowBorder() As Boolean
        Public Property ShowExcluded() As Boolean
        Public Property Map() As Single(,,)
        Public Property StanzaDS() As cStanzaDatastructures
        Public Property InRow() As Integer
        Public Property InCol() As Integer
        Public Property Month() As Integer
        Public Property Colors() As List(Of Color)
        Public Property OriginList() As List(Of PointF)
        Public Property RectList() As List(Of Rectangle)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the graphics to render onto.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Graphics() As Graphics

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Clear all items associated with this map drawer.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub ClearItems()
            Me.m_lItems.Clear()
            Me.m_lLocations.Clear()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Add a <see cref="cCoreInputOutputBase">item</see> to draw.
        ''' </summary>
        ''' <param name="item">The <see cref="cCoreInputOutputBase"/> to add.</param>
        ''' <param name="iLocation">The location to show this item at.</param>
        ''' -----------------------------------------------------------------------
        Public Sub AddItem(ByVal item As cCoreInputOutputBase, ByVal iLocation As Integer)
            Me.AddItem(item.Index, iLocation)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Add a <see cref="cCoreInputOutputBase">item</see> to draw.
        ''' </summary>
        ''' <param name="iIndex">The index of the item to add.</param>
        ''' <param name="iLocation">The location to show this item at.</param>
        ''' -----------------------------------------------------------------------
        Public Sub AddItem(ByVal iIndex As Integer, ByVal iLocation As Integer)
            If Not Me.m_lItems.Contains(iIndex) Then
                Me.m_lItems.Add(iIndex)
                Me.m_lLocations.Add(iLocation)
            End If
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the date of the current time step.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property [Date]() As String

        Public ReadOnly Property SignalState As ManualResetEvent
            Get
                Return Me.m_SignalState
            End Get
        End Property

#End Region ' Public properties

#Region " Public access "

        Public Overridable Sub Draw(ByVal obParam As Object)

            Me.AllowedToRun = False
            Dim args As cMapDrawerArgs = DirectCast(obParam, cMapDrawerArgs)
            Try
                Dim i As Integer
                Dim iIndex As Integer
                Dim iLocation As Integer
                For i = 0 To Me.m_lItems.Count - 1
                    iIndex = Me.m_lItems(i)
                    iLocation = Me.m_lLocations(i)
                    Try
                        DrawMap(iIndex, Me.RectList(iLocation), args)
                    Catch ex As Exception

                    End Try
                Next

                Me.AllowedToRun = True
                SignalState.Set()

            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                SignalState.Set()
            End Try

        End Sub

        ''' <summary>
        ''' Draw the map. The base class implementation just renderers MPA data. 
        ''' </summary>
        ''' <param name="iItem"></param>
        ''' <param name="rcPos"></param>
        ''' <param name="Args"></param>
        Public Overridable Sub DrawMap(ByVal iItem As Integer, ByVal rcPos As Rectangle, ByVal Args As cMapDrawerArgs)

            If (Me.Map Is Nothing) Then Return

            Dim rcfCell As RectangleF = Nothing
            Dim brCell As Brush = Nothing
            Dim excl As cEcospaceLayerExclusion = Me.m_core.EcospaceBasemap.LayerExclusion

            Try
                If Me.m_sg.ShowMapsMPAs Then
                    For i As Integer = 1 To Me.InRow
                        For j As Integer = 1 To Me.InCol
                            For k As Integer = 1 To Me.m_core.nMPAs
                                Dim mpa As cEcospaceLayerMPA = m_core.EcospaceBasemap.LayerMPA(k)
                                If (CBool(mpa.Cell(i, j))) Then
                                    rcfCell = New RectangleF(CSng(rcPos.Left + (j - 1) * rcPos.Width() / Me.InCol),
                                                                 CSng(rcPos.Top + (i - 1) * rcPos.Height() / Me.InRow),
                                                                 CSng(rcPos.Width() / Me.InCol),
                                                                 CSng(rcPos.Height() / Me.InRow))
                                    If Me.m_core.EcospaceMPAs(k).MPAMonth(Me.Month) Then
                                        brCell = New Drawing2D.HatchBrush(Drawing2D.HatchStyle.DiagonalCross, Color.LightGray, Color.Transparent)
                                    Else
                                        brCell = New Drawing2D.HatchBrush(Drawing2D.HatchStyle.DiagonalCross, Color.Black, Color.Transparent)
                                    End If
                                    Me.Graphics.FillRectangle(brCell, rcfCell)
                                    brCell.Dispose()
                                    'End If
                                End If
                            Next k
                        Next
                    Next
                End If
            Catch ex As Exception
                'Debug.Assert(False, ex.Message)
                Exit Sub
            End Try

            If (Me.ShowBorder) Then
                ' Draw the frame of base map
                Me.Graphics.DrawRectangle(Pens.LightGray, rcPos)
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Calculate the best layout of maps on a drawing canvas, considering
        ''' the requested number of maps on the output client rectangle and map
        ''' dimensions (row x col).
        ''' </summary>
        ''' <param name="rcClient">The client area to draw to.</param>
        ''' <param name="iNumMaps">The number of maps that will be drawn.</param>
        ''' <param name="iInRow">The number of rows in the Ecospace base map.</param>
        ''' <param name="iInCol">The number of columns in the Ecospace base map.</param>
        ''' <param name="iNumHorz">The calculated number of plots horizontally onto <paramref name="rcClient"/>.</param>
        ''' <param name="iNumVert">The calculated number of plots vertically onto <paramref name="rcClient"/>.</param>
        ''' <param name="lOrigins">A list to receive the map origins onto <paramref name="rcClient"/>.</param>
        ''' <param name="lMaps">A list to receive the map rectangles onto <paramref name="rcClient"/>.</param>
        ''' -------------------------------------------------------------------
        Public Shared Sub CalcMapAreas(ByVal rcClient As Rectangle, ByVal iNumMaps As Integer, _
                                       ByVal iInRow As Integer, ByVal iInCol As Integer, _
                                       ByRef iNumHorz As Integer, ByRef iNumVert As Integer, _
                                       ByVal lOrigins As List(Of PointF), _
                                       ByVal lMaps As List(Of Rectangle))

            lOrigins.Clear()
            lMaps.Clear()
            iNumHorz = 0
            iNumVert = 0

            If (iNumMaps = 0) Then Return

            iNumHorz = CInt(Math.Ceiling(Math.Sqrt(iNumMaps) * iInRow / iInCol * rcClient.Width / rcClient.Height))
            iNumVert = CInt(Math.Ceiling(iNumMaps / Math.Max(1, iNumHorz)))

            Dim xScale As Double = iNumHorz * (iInCol + 1) + 1
            Dim yScale As Double = iNumVert * (iInRow + 1) + 1

            If xScale > 0 Then xScale = rcClient.Width / xScale
            If yScale > 0 Then yScale = rcClient.Height / yScale

            For i As Integer = 0 To iNumVert - 1
                For j As Integer = 0 To iNumHorz - 1
                    Dim iRect As Integer = i * iNumHorz + j
                    If iRect < iNumMaps Then
                        Dim origin As PointF = New PointF((iInCol + 1) * j + 1, i * (iInRow + 1) + 1)
                        Dim rect As Rectangle = New Rectangle(CInt(origin.X * xScale), _
                                                                CInt(origin.Y * yScale), _
                                                                CInt(iInCol * xScale), _
                                                                CInt(iInRow * yScale))
                        lOrigins.Add(origin)
                        lMaps.Add(rect)
                    End If
                Next
            Next

        End Sub

#End Region ' Public access

    End Class

End Namespace
