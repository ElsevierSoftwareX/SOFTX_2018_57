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
Imports System.ComponentModel
Imports EwECore

#End Region ' Imports

Namespace Controls.Map

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' User control for implementing a <see cref="ucMap">EwE map</see> that
    ''' can be zoomed onto.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class ucMapZoom
        Implements IUIElement

#Region " Public enums "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Enumerated types defining zoom modes for displaying the map.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Enum eZoomTypes As Byte
            ''' <summary>Increase zoom level.</summary>
            ZoomIn
            ''' <summary>Decrease zoom level.</summary>
            ZoomOut
            ''' <summary>Resets zoom level to exactly fit the zoom area.</summary>
            ZoomReset
        End Enum

#End Region ' Public enums

#Region " Private vars "

        ''' <summary>UI context to connect to.</summary>
        Private m_uic As cUIContext = Nothing

        Private m_sZoom As Single = 1.0!
        Private m_sZoomMax As Single = 1.0!

        Private m_bInit As Boolean = False

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()
            Me.InitializeComponent()
            Me.SetStyle(ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer, True)
        End Sub

#End Region ' Constructor

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                If (Me.m_uic IsNot Nothing) Then
                    RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If
                Me.m_uic = value
                If (Me.m_map IsNot Nothing) Then
                    Me.m_map.UIContext = Me.m_uic
                End If
                If (Me.m_uic IsNot Nothing) Then
                    AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If
            End Set
        End Property

        Public ReadOnly Property Map() As ucMap
            Get
                Return Me.m_map
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the zoom percentage for displaying the map.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        Public Property ZoomScale(Optional bZoomToCursor As Boolean = False) As Single
            Get
                Return Me.m_sZoom
            End Get
            Set(ByVal value As Single)
                Me.m_bInUpdate = True

                Dim x, y As Single

                ' JS 10May18: the 'zoom to cursor' logic attempts to keep the map location under the
                ' mouse position fixed in place while zooming. AutoScrollPosition has to be 'in view', and does not work.
                ' Best solution is probably to calculate the top-left map corner to show after zooming,
                ' and move the scroll position to that location

                Me.SuspendLayout()

                If (bZoomToCursor) Then
                    ' A: Grab the map focus point under the cursor
                    Dim ptScreen As Point = Control.MousePosition
                    Dim ptMap As Point = Me.m_map.PointToClient(ptScreen)
                    x = CSng(ptMap.X / Me.m_map.Width)
                    y = CSng(ptMap.Y / Me.m_map.Width)
                End If

                Me.m_sZoom = Math.Max(1.0!, Math.Min(Me.m_sZoomMax, value))
                Me.ScaleMap()

                If (bZoomToCursor) Then

                    ' B: Convert back up
                    Dim ptMap As New Point(CInt(x * Me.m_map.Width), CInt(y * Me.m_map.Height))
                    Dim ptScreen As Point = Me.m_map.PointToScreen(ptMap)
                    ' Calculate how much the location under the cursor has moved
                    Dim ptDisplaced As Point = Me.PointToClient(ptScreen)
                    ' Find the new point under the cursor
                    ptScreen = Me.PointToClient(Control.MousePosition)
                    Dim dx As Integer = ptScreen.X - ptDisplaced.X
                    Dim dy As Integer = ptScreen.Y - ptDisplaced.Y

                    ' Offset scroll
                    Me.VerticalScroll.Value = Math.Max(0, Math.Min(Me.VerticalScroll.Maximum, Me.VerticalScroll.Value - dy))
                    Me.HorizontalScroll.Value = Math.Max(0, Math.Min(Me.HorizontalScroll.Maximum, Me.HorizontalScroll.Value - dx))

                End If

                Me.ResumeLayout()

                Me.m_bInUpdate = False
            End Set
        End Property

        Public Overrides Sub Refresh()
            'Re-evaluate map size etc
            Me.ScaleMap()
            MyBase.Refresh()
        End Sub

        Public ReadOnly Property CanZoomIn As Boolean
            Get
                Return (Me.m_sZoom < Me.m_sZoomMax)
            End Get
        End Property

        Public ReadOnly Property CanZoomOut As Boolean
            Get
                Return (Me.m_sZoom > 1)
            End Get
        End Property

#End Region ' Public access

#Region " Events "

#Region " Form events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext IsNot Nothing) Then

                ' Max zoom scale displays 10 cells width or height in the map
                Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
                Me.m_sZoomMax = Math.Max(1, Math.Max(bm.InCol / 10.0!, bm.InRow / 10.0!))

            End If

            Me.CenterMap()
            Me.m_bInit = False

        End Sub

        Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
            MyBase.OnResize(e)
            Me.ScaleMap()
            If Not Me.m_bInit Then
                Me.CenterMap()
                Me.m_bInit = True
            End If
        End Sub

        Protected Overrides Sub OnScroll(se As ScrollEventArgs)
            MyBase.OnScroll(se)

            If (Me.m_bInUpdate) Then Return
            Me.m_bInUpdate = True
            Try
                RaiseEvent OnPositionChanged(Me)
            Catch ex As Exception
                ' Plop
            End Try
            Me.m_bInUpdate = False

        End Sub

#End Region ' Form events

#Region " Style guide "

        Private Sub OnStyleGuideChanged(ct As Style.cStyleGuide.eChangeType)
            If (ct And Style.cStyleGuide.eChangeType.Colours) > 0 Then
                Me.UpdateControls()
            End If
        End Sub

#End Region ' Style guide

#End Region ' Events

#Region " Internal implementation "

        Private Sub ScaleMap()
            Dim cellsize As Single = CSng(Math.Min(Me.ClientRectangle.Width / Me.m_map.NumCols, Me.ClientRectangle.Height / Me.m_map.NumRows) * Me.m_sZoom)
            Me.m_map.Size = New Size(CInt(cellsize * Me.m_map.NumCols), CInt(cellsize * Me.m_map.NumRows))
        End Sub

        Private Sub CenterMap()
            Me.m_map.Location = New Point(CInt((Me.ClientRectangle.Width - Me.m_map.Width) / 2), CInt((Me.ClientRectangle.Height - Me.m_map.Height) / 2))
        End Sub

        Public Event OnPositionChanged(ByVal sender As ucMapZoom)
        Private m_bInUpdate As Boolean = False

        ''' <summary>
        ''' Zoom and position to the location of another map
        ''' </summary>
        ''' <param name="src"></param>
        Public Sub UpdatePosition(ByVal src As ucMapZoom)
            Me.m_bInUpdate = True

            Me.m_sZoom = src.m_sZoom
            Me.ScaleMap()
            Me.AutoScrollPosition = src.AutoScrollPosition

            Me.m_bInUpdate = False
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Update enabled- and checked states of child controls.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub UpdateControls()

            If (Me.IsDisposed) Then Return

            If (Me.m_uic IsNot Nothing) Then
                Me.BackColor = Me.m_uic.StyleGuide.ApplicationColor(Style.cStyleGuide.eApplicationColorType.MAP_BACKGROUND)
            End If

        End Sub

#End Region ' Internal implementation

    End Class

End Namespace
