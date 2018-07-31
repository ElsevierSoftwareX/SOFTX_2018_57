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
Imports System.Text
Imports EwECore
Imports EwECore.SpatialData
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Ecospace

    Public Class ucSpatialTimeSeriesMap
        Implements IUIElement

        ' ToDo: make multi-select

        ''' <summary>Map zoom level</summary>
        Public Enum eZoomLevel As Integer
            ''' <summary>Zoom to the extent that encompasses both the basemap and external data.</summary>
            Both = 0
            ''' <summary>Zoom to the extent of the basemap.</summary>
            Basemap
            ''' <summary>Zoom to the extent of external data.</summary>
            ExternalData
        End Enum

        Private m_ds As ISpatialDataSet = Nothing
        Private m_iTimeStep As Integer = 1
        Private m_uic As cUIContext = Nothing
        Private m_rcfEcospaceExtent As RectangleF
        Private m_lExternalDataMapExtents As New List(Of RectangleF)
        Private m_iCurrentTimeStepExtent As Integer = -1
        Private m_zoomlevel As eZoomLevel = eZoomLevel.Both
        Private m_bShowRefMap As Boolean = False
        Private m_bShowGrid As Boolean = False

        ' -- automated zoom --
        Private m_bUseBasemap As Boolean = True
        ''' <summary>Manual map display extent (min lon, max lat, width, height).</summary>
        Private m_rcfMapExtent As New RectangleF(0, 0, 0, 0)
        ''' <summary>Manual map display grid (#cols, #rows)</summary>
        Private m_szMap As New Size(10, 10)

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint, True)
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing And components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

#Region " Public access "

        <Browsable(False)> _
        Public Property UIContext As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(value As cUIContext)

                ' Cleanup
                If (Me.m_uic IsNot Nothing) Then
                    RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If

                ' Set
                Me.m_uic = value

                ' Update
                If (Me.m_uic IsNot Nothing) Then
                    AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                    Me.RefreshContent()
                End If

            End Set
        End Property

        <Browsable(False)> _
        Public Property SelectedDataset As ISpatialDataSet
            Get
                Return Me.m_ds
            End Get
            Set(value As ISpatialDataSet)
                Me.m_ds = value
                Me.RefreshContent()
                Me.Invalidate()
            End Set
        End Property

        Public Property SelectedTimeStep As Integer
            Get
                Return Me.m_iTimeStep
            End Get
            Set(value As Integer)
                Me.m_iTimeStep = value
                Me.RefreshContent()
            End Set
        End Property

        Public Property ZoomLevel As eZoomLevel
            Get
                Return Me.m_zoomlevel
            End Get
            Set(value As eZoomLevel)
                Me.m_zoomlevel = value
                Me.Invalidate()
            End Set
        End Property

        Public Property ShowReferenceMap As Boolean
            Get
                Return Me.m_bShowRefMap
            End Get
            Set(value As Boolean)
                Me.m_bShowRefMap = value
                Me.Invalidate()
            End Set
        End Property

        Public Property UseBuiltInReferenceMap As Boolean

        Public Property ShowGrid As Boolean
            Get
                Return Me.m_bShowGrid
            End Get
            Set(value As Boolean)
                Me.m_bShowGrid = value
                Me.Invalidate()
            End Set
        End Property

        Public Property ShowLabels As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the map extent and size are automatically calculated from the
        ''' Ecospace base map (true), or must be provided manually (false).
        ''' </summary>
        ''' <remarks>
        ''' The manual extent must be provided via <see cref="MapExtent"/>. The
        ''' manual map size must be provided via <see cref="MapSize"/>.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        <DefaultValue(True)> _
        <Description("Determines whether the map extent is calculated from the Ecospace scenario (true) or is manually provided (false).")> _
        Public Property UseBasemap As Boolean
            Get
                Return Me.m_bUseBasemap
            End Get
            Set(value As Boolean)
                If (Me.m_bUseBasemap <> value) Then
                    Me.m_bUseBasemap = value
                    Me.RefreshContent()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the top-left corner, width and height (in decimal degrees lon, lat) for the 
        ''' manual extent of the map to display.
        ''' </summary>
        ''' <remarks>
        ''' This value is only effective if the <see cref="UseBasemap"/>
        ''' is set to false.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property MapExtent As RectangleF
            Get
                If (Me.UIContext IsNot Nothing) And (Me.m_bUseBasemap = True) Then
                    Dim bm As cEcospaceBasemap = Me.m_uic.Core.EcospaceBasemap
                    If (bm IsNot Nothing) Then
                        Dim ptfTL As PointF = bm.PosTopLeft
                        Dim ptfBR As PointF = bm.PosBottomRight
                        Return New RectangleF(ptfTL, New SizeF(ptfBR.X - ptfTL.X, ptfBR.Y - ptfTL.Y))
                    End If
                End If
                Return Me.m_rcfMapExtent
            End Get
            Set(value As RectangleF)
                Me.m_rcfMapExtent = value
                If Not Me.UseBasemap Then Me.RefreshContent()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the number of rows and columns to display in the grid.
        ''' </summary>
        ''' <remarks>
        ''' This value is only effective if the <see cref="UseBasemap"/>
        ''' is set to false.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property MapSize As Size
            Get
                If (Me.UIContext IsNot Nothing) And (Me.m_bUseBasemap = True) Then
                    Dim bm As cEcospaceBasemap = Me.m_uic.Core.EcospaceBasemap
                    If (bm IsNot Nothing) Then
                        Return New Size(bm.InCol, bm.InRow)
                    End If
                End If
                Return Me.m_szMap
            End Get
            Set(value As Size)
                Me.m_szMap = value
                If Not Me.m_bUseBasemap Then Me.RefreshContent()
            End Set
        End Property

        Public Sub RefreshContent()

            If (Me.m_uic Is Nothing) Then Return

            Try
                Me.RecalcDisplayBits()
            Catch ex As Exception
                ' Whoah!
            End Try

        End Sub

#End Region ' Public access

#Region " Events "

        Protected Overrides Sub OnPaint(e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Me.DoPaint(e.Graphics, Me.ClientRectangle)
        End Sub

        Private Sub OnStyleGuideChanged(change As cStyleGuide.eChangeType)
            If ((change And (cStyleGuide.eChangeType.Colours Or cStyleGuide.eChangeType.Fonts Or cStyleGuide.eChangeType.Map)) > 0) Then
                Me.Invalidate()
            End If
        End Sub

#End Region ' Events

#Region " Rendering "

        Private Sub RecalcDisplayBits()

            Dim sg As cStyleGuide = Me.m_uic.StyleGuide
            Dim rcf As RectangleF = Me.MapExtent
            Dim ptfTL As PointF = rcf.Location
            Dim ptfBR As PointF = rcf.Location + rcf.Size

            Me.m_rcfEcospaceExtent = Me.ToDisplayRect(ptfTL, ptfBR)
            Me.m_lExternalDataMapExtents.Clear()

            Me.BackColor = sg.ApplicationColor(cStyleGuide.eApplicationColorType.PLOT_BACKGROUND)
            Me.Invalidate()

            If (Me.m_ds Is Nothing) Then Return

            Dim iTimeStart As Integer = 1
            Dim iTimeEnd As Integer = Me.m_uic.Core.nEcospaceTimeSteps

            If Me.m_ds.TimeStart > Date.MinValue Then
                iTimeStart = Me.m_uic.Core.AbsoluteTimeToEcospaceTimestep(Me.m_ds.TimeStart)
            End If

            If Me.m_ds.TimeEnd < Date.MaxValue Then
                iTimeEnd = Me.m_uic.Core.AbsoluteTimeToEcospaceTimestep(Me.m_ds.TimeEnd)
            End If

            Me.m_iCurrentTimeStepExtent = -1
            For iStep As Integer = iTimeStart To iTimeEnd
                If Me.m_ds.GetExtentAtT(Me.m_uic.Core.EcospaceTimestepToAbsoluteTime(iStep), ptfTL, ptfBR) Then
                    ' Is data is for current time step?
                    If (iStep = Me.m_iTimeStep) Then
                        ' #Yes: remember current time step index
                        Me.m_iCurrentTimeStepExtent = Me.m_lExternalDataMapExtents.Count
                    End If
                    ' Add map extent
                    Me.m_lExternalDataMapExtents.Add(ToDisplayRect(ptfTL, ptfBR))
                End If
            Next
        End Sub

        Private Sub DoPaint(g As Graphics, rc As Rectangle)

            If (Me.m_uic Is Nothing) Then Return

            Dim rcfFocusExtent As RectangleF = Me.m_rcfEcospaceExtent

            ' Is data zoom involved?
            If (Me.m_zoomlevel = eZoomLevel.Both) Or (Me.m_zoomlevel = eZoomLevel.ExternalData) Then
                ' #Yes: only zoom to data? (and has data to zoom to)
                If (Me.m_zoomlevel = eZoomLevel.ExternalData) And (Me.m_lExternalDataMapExtents.Count > 0) Then
                    ' #Yes: ok, then base zoom exclusively to data
                    rcfFocusExtent = New Rectangle(180, 90, -360, -180)
                End If

                ' Find biggest display rect
                For Each rcf As RectangleF In Me.m_lExternalDataMapExtents
                    Dim ptfTL As New PointF(Math.Min(rcf.Left, rcfFocusExtent.Left), Math.Min(rcf.Top, rcfFocusExtent.Top))
                    Dim ptfBR As New PointF(Math.Max(rcf.Right, rcfFocusExtent.Right), Math.Max(rcf.Bottom, rcfFocusExtent.Bottom))
                    rcfFocusExtent = New RectangleF(ptfTL.X, ptfTL.Y, ptfBR.X - ptfTL.X, ptfBR.Y - ptfTL.Y)
                Next
            End If

            ' Abort if nothing to display
            If (rcfFocusExtent.Width = 0 Or rcfFocusExtent.Height = 0) Then Return

            ' Scale to 120% of the focus area OR the focus area + 3 degrees, whatever is largest
            ' .. BUT limit extent to width 360, height 180!
            Dim sScale As Single = CSng(Math.Min(rc.Height / (Math.Min(180, Math.Max(rcfFocusExtent.Height + 3, rcfFocusExtent.Height * 1.2))), _
                                                 rc.Width / (Math.Min(360, Math.Max(rcfFocusExtent.Width + 3, rcfFocusExtent.Width * 1.2)))))
            Dim dx As Single = rc.Width / 2.0! - (rcfFocusExtent.X + rcfFocusExtent.Width / 2.0!) * sScale
            Dim dy As Single = rc.Height / 2.0! - (rcfFocusExtent.Y + rcfFocusExtent.Height / 2.0!) * sScale
            Dim rcfViewExtent As New RectangleF(rcfFocusExtent.X - rc.Width / 2.0! / sScale, rcfFocusExtent.Y - rc.Height / 2.0! / sScale, rc.Width / sScale, rc.Height / sScale)

            g.TranslateTransform(dx, dy)
            g.ScaleTransform(sScale, sScale)

            Try
                ' Draw content
                Me.DrawReferenceImage(g)
                Me.DrawDataRectangles(g)
                Me.DrawMap(g)
                Me.DrawGridLines(g, rc, rcfViewExtent)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

            g.ResetTransform()

            Try
                Me.DrawLabels(g, rc)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

        Private Sub DrawDataRectangles(g As Graphics)

            ' Only draw data rectangles if external data is available
            If (Me.m_ds Is Nothing) Then Return
            If (Me.m_lExternalDataMapExtents.Count = 0) Then Return

            Dim manSets As cSpatialDataSetManager = Me.m_uic.Core.SpatialDataConnectionManager.DatasetManager
            Dim sg As cStyleGuide = Me.m_uic.StyleGuide
            Dim comp As cDatasetCompatilibity = manSets.Compatibility(Me.m_ds)
            Dim clrFillFull As Color = Color.FromKnownColor(KnownColor.LightBlue)
            Dim clrOutlineFull As Color = Color.FromKnownColor(KnownColor.Blue)
            Dim bError As Boolean = False

            Select Case comp.Compatibility
                Case cDatasetCompatilibity.eCompatibilityTypes.Errors, _
                     cDatasetCompatilibity.eCompatibilityTypes.NoTemporal, _
                     cDatasetCompatilibity.eCompatibilityTypes.NoSpatial
                    clrFillFull = sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)
                Case cDatasetCompatilibity.eCompatibilityTypes.NotSet, _
                     cDatasetCompatilibity.eCompatibilityTypes.TemporalNotIndexed
                    clrFillFull = Color.LightGray
            End Select

            Dim clrFillLight As Color = cColorUtils.GetVariant(clrFillFull, 0.5!)
            Dim clrOutlineLight As Color = cColorUtils.GetVariant(clrOutlineFull, 0.5!)

            ' - Fills -
            If (Me.m_iCurrentTimeStepExtent >= 0) Then
                Using br As New SolidBrush(Color.FromArgb(64, Color.LightBlue))
                    g.FillRectangle(br, Me.m_lExternalDataMapExtents(Me.m_iCurrentTimeStepExtent))
                End Using
            Else
                Using br As New SolidBrush(Color.FromArgb(64, Color.LightBlue))
                    g.FillRectangles(br, Me.m_lExternalDataMapExtents.ToArray)
                End Using
            End If

            ' - Outlines -
            'Using p As New Pen(clrOutlineLight, 0.001)
            '    g.DrawRectangles(p, Me.m_lExternalDataMapExtents.ToArray)
            'End Using
            If (Me.m_iCurrentTimeStepExtent >= 0) Then
                Using p As New Pen(clrOutlineFull, 0.001)
                    g.DrawRectangles(p, New RectangleF() {Me.m_lExternalDataMapExtents(Me.m_iCurrentTimeStepExtent)})
                End Using
            End If

        End Sub

        Private Sub DrawMap(g As Graphics)

            ' ToDo: draw depth layer preview
            Using br As New SolidBrush(Color.FromArgb(64, Color.White))
                g.FillRectangles(br, New RectangleF() {Me.m_rcfEcospaceExtent})
            End Using
            Using p As New Pen(Color.FromArgb(192, Color.Green), 0.001)
                g.DrawRectangles(p, New RectangleF() {Me.m_rcfEcospaceExtent})
            End Using

        End Sub

        Private Sub DrawReferenceImage(g As Graphics)

            If (Not Me.ShowReferenceMap) Then Return

            Dim img As Image = Nothing
            Dim ptfTL As PointF
            Dim ptfBR As PointF

            If Me.UseBuiltInReferenceMap Then
                img = My.Resources.urf
                ptfTL = New PointF(-180, 90)
                ptfBR = New PointF(180, -90)
            Else
                Dim sg As cStyleGuide = Me.m_uic.StyleGuide
                img = sg.MapReferenceImage
                ptfTL = sg.MapReferenceLayerTL
                ptfBR = sg.MapReferenceLayerBR
            End If

            If (img IsNot Nothing) Then
                Try
                    g.DrawImage(img, ptfTL.X, -ptfTL.Y, (ptfBR.X - ptfTL.X), (ptfTL.Y - ptfBR.Y))
                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                End Try
            End If

        End Sub

        Private Sub DrawLabels(g As Graphics, rc As Rectangle)

            If Not Me.ShowLabels Then Return

            Dim sg As cStyleGuide = Me.m_uic.StyleGuide
            Dim tmpFont As Font = sg.Font(cStyleGuide.eApplicationFontType.Scale)
            Dim brText As New SolidBrush(Color.Black)
            Dim brBack As New SolidBrush(Color.FromArgb(164, 255, 255, 255))
            Dim sbText As New StringBuilder()
            Dim fmt As New StringFormat()

            fmt.Alignment = StringAlignment.Center
            fmt.LineAlignment = StringAlignment.Center

            If (Me.m_iTimeStep >= 0) Then
                sbText.AppendLine(cStringUtils.Localize(My.Resources.CAPTION_TIMESTEP, _
                                                Me.m_iTimeStep, Me.m_uic.Core.EcospaceTimestepToAbsoluteTime(Me.m_iTimeStep).ToShortDateString()))
            End If

            If (Me.m_ds IsNot Nothing) Then
                Dim sdf As New cSpatialDatasetFormatter()
                sbText.AppendLine(cStringUtils.Localize(My.Resources.CAPTION_DATASET, sdf.GetDescriptor(Me.m_ds)))
            End If

            Dim szfTextSize As SizeF = g.MeasureString(sbText.ToString, tmpFont, rc.Size, fmt)
            g.FillRectangle(brBack, CInt(rc.Width / 2 - szfTextSize.Width / 2) - 4, 27, CInt(szfTextSize.Width + 8), CInt(szfTextSize.Height) + 8)
            g.DrawString(sbText.ToString, tmpFont, brText, _
                         New RectangleF(rc.Width / 2.0! - szfTextSize.Width / 2.0!, 31, szfTextSize.Width, szfTextSize.Height), _
                         fmt)


            brText.Dispose()
            brBack.Dispose()

        End Sub

        ''' <summary>
        ''' Draw lat, lon grid lines
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="rc"></param>
        ''' <param name="rcfView"></param>
        ''' <remarks></remarks>
        Private Sub DrawGridLines(g As Graphics, rc As Rectangle, rcfView As RectangleF)

            If (Not Me.m_bShowGrid) Then Return

            Dim x As Integer = CInt(Math.Floor(rcfView.X))
            Dim y As Integer = CInt(Math.Floor(rcfView.Y))
            Dim dx As Integer = CInt(Math.Ceiling(rcfView.Width)) + 2
            Dim dy As Integer = CInt(Math.Ceiling(rcfView.Height)) + 2

            Dim pMinor As New Pen(Color.FromArgb(64, 0, 0, 0), 0.00001)
            Dim pMajor As New Pen(Color.FromArgb(148, 0, 0, 0), 0.001)
            Dim p As Pen = Nothing

            For i As Integer = 0 To dx
                If ((i + x) Mod 5) = 0 Then p = pMajor Else p = pMinor
                g.DrawLine(p, x + i, y, x + i, y + dy)
            Next i
            For j As Integer = 0 To dy
                If ((j + x) Mod 5) = 0 Then p = pMajor Else p = pMinor
                g.DrawLine(p, x, y + j, x + dx, y + j)
            Next j

            p = Nothing
            pMinor.Dispose()
            pMajor.Dispose()

        End Sub

#End Region ' Rendering

#Region " Helper methods "

        Private Function ToDisplayRect(ptfTL As PointF, ptfBR As PointF) As RectangleF
            Return New RectangleF(ptfTL.X, -ptfTL.Y, ptfBR.X - ptfTL.X, ptfTL.Y - ptfBR.Y)
        End Function

#End Region ' Helper methods

    End Class

End Namespace
