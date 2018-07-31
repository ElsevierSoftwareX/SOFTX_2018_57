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

Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports ScientificInterfaceShared.Definitions
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This Sketchpad control class is used to render the shape and support
    ''' mouse interaction. It can be used as the base class for both forcing functions and 
    ''' mediation functions.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(True)> _
    Public Class ucSketchPad
        Implements IUIElement

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Exclusive mouse interaction modes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Enum eMouseInteractionMode As Integer
            ''' <summary>Not drawing.</summary>
            None
            ''' <summary>User is drawing the shape.</summary>
            DrawShape
            ''' <summary>User is dragging the X mark line.</summary>
            DragXMark
            ''' <summary>User is dragging the Y mark line.</summary>
            DragYMark
        End Enum

        Private Const cCLICK_TOLERANCE As Integer = 4

#Region " Variables "

        ''' <summary>The one UI context</summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>The manager of this control.</summary>
        Private m_handler As cShapeGUIHandler = Nothing
        ''' <summary>The shape shown in this control.</summary>
        Private m_shape As cShapeData = Nothing
        ''' <summary>Last known mouse location when drawing.</summary>
        Private m_ptPosPrevious As Point = Nothing

        ''' <summary></summary>
        Private m_scalemodeYAxis As eAxisAutoScaleModeTypes = eAxisAutoScaleModeTypes.Auto
        ''' <summary></summary>
        Protected m_color As Color = Drawing.Color.AliceBlue
        ''' <summary></summary>
        Protected m_bShowAxis As Boolean = True
        ''' <summary></summary>
        Protected m_sketchDrawMode As eSketchDrawModeTypes = eSketchDrawModeTypes.Fill
        ''' <summary></summary>
        Protected m_shapeType As eShapeCategoryTypes = eShapeCategoryTypes.NotSet

        ''' <summary></summary>
        Private m_iXMax As Integer = cCore.NULL_VALUE
        ''' <summary></summary>
        Private m_sYMax As Single = cCore.NULL_VALUE
        ''' <summary></summary>
        Private m_sYMin As Single = cCore.NULL_VALUE
        ''' <summary>Number of data years to show.</summary>
        Private m_iNumDataPoints As Integer = 0

        ''' <summary>Horizontal mark line.</summary>
        Private m_sYMarkValue As Single = cCore.NULL_VALUE
        ''' <summary>Flag stating if horizontal Y mark line should be shown.</summary>
        Private m_bShowYMark As Boolean = False
        ''' <summary>Horizontal mark line.</summary>
        Private m_strYMarkLabel As String = ""
        ''' <summary>Flag stating if the vertical X mark line should be shown.</summary>
        Private m_bShowXMark As Boolean = False
        ''' <summary>Vertical mark line.</summary>
        Private m_sXMarkValue As Single = cCore.NULL_VALUE
        ''' <summary>Flag stating whether the value tool tip should be shown.</summary>
        Private m_bShowTooltip As Boolean = True
        ''' <summary>Current edit mode</summary>
        Private m_editMode As eMouseInteractionMode = eMouseInteractionMode.None
        ''' <summary>Style of the control.</summary>
        Private m_style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

        ''' <summary></summary>
        Public Delegate Sub ShapeChangedDelegate(ByVal shape As cShapeData)
        ''' <summary></summary>
        Public Event ShapeChanged As ShapeChangedDelegate

        ''' <summary></summary>
        Public Delegate Sub ShapeFinalizedDelegate(ByVal shape As cShapeData, ByVal sketchpad As ucSketchPad)
        ''' <summary></summary>
        Public Event ShapeFinalized As ShapeFinalizedDelegate

#End Region ' Variables

#Region " Constructor "

        Public Sub New()

            Me.InitializeComponent()

            ' Enable double buffering
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.UserPaint, True)
            Me.SetStyle(ControlStyles.ResizeRedraw, True)

            Me.Dock = DockStyle.Fill

            ' Default rendering mode
            Me.m_sketchDrawMode = eSketchDrawModeTypes.Fill
            Me.m_scalemodeYAxis = eAxisAutoScaleModeTypes.Auto

        End Sub

#End Region ' Constructor

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the handler that manages this sketch pad.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public Property Handler() As cShapeGUIHandler
            Get
                Return Me.m_handler
            End Get
            Set(ByVal value As cShapeGUIHandler)
                Me.m_handler = value
                Me.UpdateMenuItemStates()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the shape to display in the sketch pad.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public Overridable Property Shape() As cShapeData

            Get
                Return Me.m_shape
            End Get

            Set(ByVal value As cShapeData)
                ' Store new shape ref
                Me.m_shape = value
                ' Respond to this major event
                Me.UpdateControl()
                ' Broadcast change
                Me.OnShapeChanged()
            End Set

        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether to display the shape as 12-month seasonal data or
        ''' across the full length of time.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("State whether to display the shape as 12-month seasonal data or across the full length of time.")> _
        Public Property IsSeasonal() As Boolean

            Get
                If Me.m_shape Is Nothing Then Return False
                Return Me.m_shape.IsSeasonal
            End Get

            Set(ByVal value As Boolean)
                If Me.m_shape IsNot Nothing Then
                    Me.m_shape.IsSeasonal = value
                    If Me.m_shape.IsSeasonal Then RepeatSeasonalPattern()
                    Me.OnShapeChanged()
                End If
            End Set

        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the line style used to render the graph.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("The line style used to render the graph")> _
        Public Property SketchDrawMode() As eSketchDrawModeTypes

            Get
                Return Me.m_sketchDrawMode
            End Get

            Set(ByVal value As eSketchDrawModeTypes)
                Me.m_sketchDrawMode = value
                Me.Invalidate()
            End Set

        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the sketch pad should display an X and Y axis.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("State whether the sketch pad should display an X and Y axis.")> _
        Public Property DisplayAxis() As Boolean

            Get
                Return Me.m_bShowAxis
            End Get

            Set(ByVal value As Boolean)
                Me.m_bShowAxis = value
                Me.Invalidate()
            End Set

        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the sketch pad should automatically scale the Y axis
        ''' to the range of data in the current shape.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("State whether the sketch pad should automatically scale the Y axis to the range of data in the current shape.")> _
        Public Property YAxisAutoScaleMode() As eAxisAutoScaleModeTypes

            Get
                Return Me.m_scalemodeYAxis
            End Get

            Set(ByVal value As eAxisAutoScaleModeTypes)
                Me.m_scalemodeYAxis = value
                Me.Invalidate()
            End Set

        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the max X value for the graph.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("State the max X value for the graph.")> _
        Public Overridable Property XAxisMaxValue() As Integer
            Get
                If (Me.m_iXMax <= 0) And (Me.Handler IsNot Nothing) Then Return Me.Handler.XAxisMaxValue
                Return Me.m_iXMax
            End Get
            Set(ByVal iValue As Integer)
                Me.m_iXMax = iValue
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the max Y value for the graph.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("State the max Y value for the graph.")> _
        Public Property YAxisMaxValue() As Single
            Get
                ' Locked for drawing?
                If Me.m_sYMaxLock > 0.0! Then
                    Return Me.m_sYMaxLock
                End If

                Select Case Me.YAxisAutoScaleMode
                    Case eAxisAutoScaleModeTypes.Auto
                        If Me.Shape Is Nothing Then Return 0.0
                        Return Math.Max(Me.Shape.YMax * 1.25!, Me.YAxisMinValue)
                    Case eAxisAutoScaleModeTypes.Fixed
                        Return Math.Max(0, Me.m_sYMax * 1.25!)
                End Select
            End Get
            Set(ByVal sValue As Single)
                Me.m_sYMax = sValue
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the min Y value for the graph.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
          Description("State the min Y value for the graph.")> _
        Public Property YAxisMinValue() As Single
            Get
                Return Me.m_sYMin
            End Get
            Set(ByVal value As Single)
                Me.m_sYMin = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the horizontal (Y mark) line should be shown.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("Flag stating whether the horizontal (Y mark) line should be shown.")> _
        Public Property ShowYMark() As Boolean
            Get
                Return Me.m_bShowYMark
            End Get
            Set(ByVal value As Boolean)
                Me.m_bShowYMark = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Value for horizontal (Y mark) line.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("The value for a horizontal (Y mark) line.")> _
        Public Property YMarkValue() As Single
            Get
                If Not Me.ShowYMark Then Return cCore.NULL_VALUE
                Return Me.m_sYMarkValue
            End Get
            Set(ByVal value As Single)
                Me.m_sYMarkValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Label for horizontal (Y mark) line
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("The label for a horizontal (Y mark) line.")> _
        Public Property YMarkLabel() As String
            Get
                Return Me.m_strYMarkLabel
            End Get
            Set(ByVal value As String)
                Me.m_strYMarkLabel = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the vertical (X mark) line should be shown.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("Flag stating whether the vertical (X mark) line should be shown.")> _
        Public Property ShowXMark() As Boolean
            Get
                Return Me.m_bShowXMark
            End Get
            Set(ByVal value As Boolean)
                Me.m_bShowXMark = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the vertical (X mark) line can be dragged.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("Flag stating whether the vertical (X mark) line can be dragged.")> _
        Public Property AllowDragXMark() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Value for vertical (X mark) line.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("Value for vertical (X mark) line.")> _
        Public Property XMarkValue() As Single
            Get
                If Not Me.ShowXMark Then Return cCore.NULL_VALUE
                Return Me.m_sXMarkValue
            End Get
            Set(ByVal value As Single)
                Me.m_sXMarkValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the number of X-axis points with relevant data. All data 
        ''' beyond this number will be blocked-out when drawn.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public Overridable Property NumDataPoints() As Integer
            Get
                Return Me.m_iNumDataPoints
            End Get
            Set(ByVal value As Integer)
                Me.m_iNumDataPoints = value
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Label for vertical (X mark) line.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public Overridable ReadOnly Property XMarkLabel() As String
            Get
                Return ""
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the colour used to draw the shape.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("Colour to draw the shape with.")> _
        Public Property ShapeColor() As Color

            Get
                Return Me.m_color
            End Get

            Set(ByVal value As Color)
                Me.m_color = value
                Me.Invalidate()
            End Set

        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eShapeCategoryTypes">category</see> of the shape.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public ReadOnly Property ShapeCategory() As eShapeCategoryTypes

            Get
                If Me.m_shape Is Nothing Then Return eShapeCategoryTypes.NotSet
                Select Case m_shape.DataType
                    Case eDataTypes.Forcing
                        Return eShapeCategoryTypes.Forcing
                    Case eDataTypes.EggProd
                        Return eShapeCategoryTypes.EggProduction
                    Case eDataTypes.Mediation
                        Return eShapeCategoryTypes.Mediation

                    Case eDataTypes.CapacityMediation
                        Return eShapeCategoryTypes.Mediation
                    Case Else
                        Debug.Assert(False)
                End Select
                Return m_shapeType
            End Get

        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the sketch pad should display a value tool tip.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("Category of the shape.")> _
        Public Property ShowValueTooltip() As Boolean
            Get
                Return Me.m_bShowTooltip
            End Get
            Set(ByVal value As Boolean)
                Me.m_bShowTooltip = value
                Me.UpdateTooltip(Nothing)
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save the current shape to an image file.
        ''' </summary>
        ''' <param name="shape"></param>
        ''' <param name="strFileName"></param>
        ''' <param name="imgFormat"></param>
        ''' <param name="strError"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Overridable Function SaveAsImage(ByVal shape As cShapeData, ByVal strFileName As String, _
                                                ByVal imgFormat As ImageFormat, _
                                                ByRef strError As String) As Boolean

            Dim rcClient As Rectangle = Me.ClientRectangle()
            Dim bmp As Bitmap = Me.UIContext.StyleGuide.GetImage(rcClient.Width, rcClient.Height, imgFormat, strFileName)
            Dim g As Graphics = Graphics.FromImage(bmp)
            Dim fs As IO.FileStream = Nothing
            Dim bSucces As Boolean = True

            ' Render the shape
            g.Clear(Me.BackColor)

            Try
                Me.DrawShape(shape, rcClient, g, Me.ShapeColor, True, Me.SketchDrawMode, Me.XAxisMaxValue, Me.YAxisMaxValue)
            Catch ex As Exception
                bSucces = False
            End Try

            ' Try to open the stream
            Try
                fs = New FileStream(strFileName, FileMode.Create)
                bmp.Save(fs, imgFormat)
                fs.Close()
            Catch ex As Exception
                ' An error occurred
                strError = ex.Message
                bSucces = False
            End Try
            Return bSucces

        End Function

        Private m_bEditable As Boolean = True

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the shape is editable by the user.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("States whether the shape can be edited by the user.")> _
        Public Overridable Property Editable() As Boolean
            Get
                Return Me.m_bEditable
            End Get
            Set(ByVal bEditable As Boolean)
                If (bEditable <> Me.m_bEditable) Then
                    Me.m_bEditable = bEditable
                    Me.UpdateControl()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Draws a <see cref="cForcingFunction">Forcing Function</see>.
        ''' </summary>
        ''' <param name="shape">The shape to draw.</param>
        ''' <param name="rcImage">The dimensions of the area to render the shape onto.</param>
        ''' <param name="g">The graphics to draw the image onto.</param>
        ''' <param name="clr">The colour to use rendering the image.</param>
        ''' <param name="drawMode">The <see cref="SketchDrawMode">Mode</see> to render the shape with.</param>
        ''' <param name="bDrawLabels">The max X value to draw.</param>
        ''' <param name="sYMax">The max Y value to scale the shape to.</param>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub DrawShape(ByVal shape As cShapeData, _
                                ByVal rcImage As Rectangle, _
                                ByVal g As Graphics, _
                                ByVal clr As Color, _
                                ByVal bDrawLabels As Boolean, _
                                ByVal drawMode As eSketchDrawModeTypes, _
                                ByVal iXMax As Integer, _
                                ByVal sYMax As Single)

            If (Me.Shape Is Nothing) Then Return

            ' Draw default
            cShapeImage.DrawShape(Me.UIContext, shape, rcImage, g, clr, drawMode, _
                                  iXMax, sYMax, _
                                  Me.XMarkValue, Me.YMarkValue, _
                                  Me.XMarkLabel, Me.YMarkLabel)

            ' Draw gray area to block out data past the NumDataYears, if applicable
            If (Me.NumDataPoints > 0 And Not shape.IsSeasonal) Then
                Me.DrawYearLimit(g, Me.YearToX(Me.NumDataPoints, rcImage.Width))
            End If

        End Sub

        Public Property CanEditPoints As Boolean = True

        ''' <summary>
        ''' Current mouse interaction mode.
        ''' </summary>
        <Browsable(False)> _
        Protected Property EditMode As eMouseInteractionMode
            Get
                Return Me.m_editMode
            End Get
            Set(ByVal value As eMouseInteractionMode)
                Me.m_editMode = value
                Me.UpdateCursor()
            End Set
        End Property

#End Region ' Public access

#Region " IUIElement implementation "

        <Browsable(False)> _
        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                Me.m_uic = value
                Me.UpdateControl()
            End Set
        End Property

#End Region ' IUIElement implementation

#Region " Internal Methods "

        ''' <summary>
        ''' Draw a hashed-out area past a given <see cref="XAxisMaxValue">data limit</see>.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="x"></param>
        Protected Sub DrawYearLimit(ByRef g As Graphics, ByVal x As Integer)
            Using br As New HatchBrush(HatchStyle.SmallConfetti, Color.FromArgb(100, 0, 0, 0), Color.Transparent)
                g.FillRectangle(br, New Rectangle(x, 0, Me.Width, Me.Height))
            End Using
        End Sub

        Protected Function YearToX(ByVal iYear As Integer, ByVal iWidth As Integer) As Integer
            Return CInt(Math.Round((iYear * iWidth * cCore.N_MONTHS) / Me.XAxisMaxValue))
        End Function

        Protected Function XToYear(ByVal x As Integer, ByVal iWidth As Integer) As Integer
            Dim iYear As Integer = CInt(Math.Round(x * Me.XAxisMaxValue / (cCore.N_MONTHS * iWidth)))
            Return Math.Min(Math.Max(0, iYear), CInt(Math.Floor(Me.XAxisMaxValue / cCore.N_MONTHS)))
        End Function

        Private Sub UpdateControl()

            If (Me.m_uic Is Nothing) Then Return

            If (Me.Editable = True) And (Me.m_shape IsNot Nothing) Then
                Me.BackColor = Me.m_uic.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.IMAGE_BACKGROUND)
            Else
                Me.BackColor = Me.m_uic.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.READONLY_BACKGROUND)
            End If

            Me.UpdateCursor()

        End Sub

        Private Sub UpdateCursor()

            Select Case Me.EditMode

                Case eMouseInteractionMode.DragXMark
                    Me.Cursor = Cursors.SizeWE

                Case eMouseInteractionMode.DragYMark
                    Me.Cursor = Cursors.SizeNS

                Case eMouseInteractionMode.DrawShape,
                     eMouseInteractionMode.None
                    Me.Cursor = Cursors.Default

            End Select

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, states whether a given point lies inside the drawing 
        ''' region.
        ''' </summary>
        ''' <returns>
        ''' True if the given point lies inside the drawing region.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Private Function PointInRegion(ByVal p As Point, ByVal rcImage As Rectangle) As Boolean
            Return (p.X >= rcImage.Left) And (p.X <= rcImage.Right) And _
                   (p.Y >= rcImage.Top) And (p.Y <= rcImage.Bottom)
        End Function

        Protected Overridable Sub DragXMark(ByVal ptPrev As Point, ByVal ptCur As Point)
            Dim sYMax As Single = Me.YAxisMaxValue
            Dim iXMax As Integer = If(Me.Shape.IsSeasonal, cCore.N_MONTHS, Me.XAxisMaxValue)
            Dim ptfCur As PointF = cShapeImage.ToModelPoint(ptCur, Me.ClientRectangle, iXMax, sYMax)
            Me.XMarkValue = ptfCur.X
        End Sub

        Protected Overridable Sub DragYMark(ByVal ptPrev As Point, ByVal ptCur As Point)
            Dim sYMax As Single = Me.YAxisMaxValue
            Dim iXMax As Integer = If(Me.Shape.IsSeasonal, cCore.N_MONTHS, Me.XAxisMaxValue)
            Dim ptfCur As PointF = cShapeImage.ToModelPoint(ptCur, Me.ClientRectangle, iXMax, sYMax)
            Me.YMarkValue = ptfCur.Y
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Modifies a shape between two click points.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub ModifyShapePoints(ByVal ptPrev As Point, ByVal ptCur As Point)

            Dim sYMax As Single = Me.YAxisMaxValue
            Dim iXMax As Integer = If(Me.Shape.IsSeasonal, cCore.N_MONTHS, Me.XAxisMaxValue)
            Dim ptfPrev As PointF = cShapeImage.ToModelPoint(ptPrev, Me.ClientRectangle, iXMax, sYMax)
            Dim ptfCur As PointF = cShapeImage.ToModelPoint(ptCur, Me.ClientRectangle, iXMax, sYMax)

            Dim iStart As Integer = CInt(Math.Min(ptfPrev.X, ptfCur.X))
            Dim iEnd As Integer = CInt(Math.Max(ptfPrev.X, ptfCur.X))
            Dim sY As Single = 0

            If (iStart < 0) Or (iEnd > Me.XAxisMaxValue) Then
                Return
            End If

            ' Single click?
            If iStart = iEnd Then
                sY = (ptfCur.Y + ptfPrev.Y) / 2
                Me.Shape.ShapeData(iStart) = sY
            Else
                For i As Integer = iStart To iEnd
                    sY = (ptfCur.Y - ptfPrev.Y) * (i - ptfPrev.X) / (ptfCur.X - ptfPrev.X) + ptfPrev.Y
                    Me.Shape.ShapeData(i) = sY
                Next
            End If

            'If iEnd = Math.Round((Me.ClientRectangle.Width - 1) * Me.XAxisMaxValue / Me.ClientRectangle.Width) Then
            '    For i As Integer = iEnd To Me.XAxisMaxValue - 1
            '        Me.Shape.ShapeData(i) = Me.Shape.ShapeData(iEnd)
            '    Next
            'End If

        End Sub

        Private Sub UpdateTooltip(ByVal ptCur As Point)

            If (Me.Shape Is Nothing) Then Return

            Dim strTooltip As String = ""

            If Me.m_bShowTooltip And (ptCur <> Nothing) Then
                Dim sYMax As Single = Me.YAxisMaxValue
                Dim iXMax As Integer = If(Me.Shape.IsSeasonal, cCore.N_MONTHS, Me.XAxisMaxValue)
                Dim ptfCur As PointF = cShapeImage.ToModelPoint(ptCur, Me.ClientRectangle, iXMax, sYMax)
                Dim sValue As Single = 0.0!

                If Me.IsNearValue(ptCur) Then
                    sValue = Me.Shape.ShapeData(CInt(ptfCur.X))
                    strTooltip = Me.UIContext.StyleGuide.FormatNumber(sValue)
                Else
                    ' No tip, sorry.
                End If
            End If
            cToolTipShared.GetInstance().SetToolTip(Me, strTooltip)

        End Sub

        Public Sub RepeatSeasonalPattern()

            If Not Me.m_shape.IsSeasonal Then Return

            Dim asValues(Me.Shape.nPoints - 1) As Single
            Dim j As Integer = 0

            For i As Integer = 1 To Me.Shape.nPoints - 1
                asValues(i) = Me.Shape.ShapeData(j + 1)
                j += 1
                If j = cCore.N_MONTHS Then j = 0
            Next
            Me.Shape.ShapeData = asValues

        End Sub

        Private Function IsNearXMark(ByVal sX As Single) As Boolean

            If Not m_bShowXMark Then Return False

            ' Check if x value is near x mark
            Dim sYMax As Single = Me.YAxisMaxValue
            Dim iXMax As Integer = If(Me.Shape.IsSeasonal, cCore.N_MONTHS, Me.XAxisMaxValue)
            Dim ptfMouseL As PointF = cShapeImage.ToModelPoint(New PointF(sX - cCLICK_TOLERANCE, 0), Me.ClientRectangle, iXMax, sYMax)
            Dim ptfMouseR As PointF = cShapeImage.ToModelPoint(New PointF(sX + cCLICK_TOLERANCE, 0), Me.ClientRectangle, iXMax, sYMax)

            Return (ptfMouseL.X <= Me.XMarkValue) And (ptfMouseR.X >= Me.XMarkValue)

        End Function

        Private Function IsNearYMark(ByVal sY As Single) As Boolean

            If Not m_bShowYMark Then Return False

            ' Check if y value is near y mark
            Dim sYMax As Single = Me.YAxisMaxValue
            Dim ptfMouseT As PointF = cShapeImage.ToModelPoint(New PointF(0, sY - cCLICK_TOLERANCE), Me.ClientRectangle, 0, sYMax)
            Dim ptfMouseB As PointF = cShapeImage.ToModelPoint(New PointF(0, sY + cCLICK_TOLERANCE), Me.ClientRectangle, 0, sYMax)

            Return (ptfMouseT.Y >= Me.YMarkValue) And (ptfMouseB.Y <= Me.YMarkValue)

        End Function

        Private Function IsNearValue(ByVal ptCur As Point) As Boolean
            If (Me.Shape Is Nothing) Then Return False
            If (Me.UIContext Is Nothing) Then Return False

            Dim sYMax As Single = Me.YAxisMaxValue
            Dim iXMax As Integer = If(Me.Shape.IsSeasonal, cCore.N_MONTHS, Me.XAxisMaxValue)
            Dim ptfMouseT As PointF = cShapeImage.ToModelPoint(New PointF(ptCur.X, ptCur.Y - 2 * cCLICK_TOLERANCE), Me.ClientRectangle, iXMax, sYMax)
            Dim ptfMouseB As PointF = cShapeImage.ToModelPoint(New PointF(ptCur.X, ptCur.Y + 2 * cCLICK_TOLERANCE), Me.ClientRectangle, iXMax, sYMax)
            Dim sValue As Single = Me.Shape.ShapeData(CInt(ptfMouseT.X))

            Return (ptfMouseB.Y <= sValue) And (sValue <= ptfMouseT.Y)
        End Function

#End Region ' Private Methods

#Region " Rendering "

        ''' <summary>
        ''' Locked Y scale while drawing
        ''' </summary>
        Private m_sYMaxLock As Single = 0.0!

        ''' <summary>
        ''' This method handls the Paint event and does the actual drawing routine
        ''' It only draws the graph with no other additional info like caption, axises..eg..Those will be drawn in the inherited class if needed
        ''' </summary>
        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)

            If (Me.UIContext Is Nothing) Then Return

            Dim sYMax As Single = Me.YAxisMaxValue
            ' Avoid division by zero
            If (sYMax <= 0.0!) Then sYMax = 1.0!

            ' Check for invalid values
            If (Single.IsNaN(sYMax)) Then Return
            If (Single.IsNegativeInfinity(sYMax)) Then Return
            If (Single.IsPositiveInfinity(sYMax)) Then Return

            Try
                ' Draw
                Me.DrawShape(Me.Shape, Me.ClientRectangle, e.Graphics, Me.ShapeColor, True, Me.SketchDrawMode, Me.XAxisMaxValue, sYMax)
            Catch ex As Exception
                ' Woops
            End Try

        End Sub

        Private Sub ProcessMouseInput(ByVal e As System.Windows.Forms.MouseEventArgs)

            ' Nothing to do here?
            If Me.UIContext Is Nothing Then Return

            Dim ptPosCurrent As Point = New Point(e.X, e.Y)

            If (e.Button = MouseButtons.Left) Then

                If Not Me.Editable Then Return
                If Not Me.Capture Then Return

                If (Me.m_ptPosPrevious = Nothing) Then m_ptPosPrevious = ptPosCurrent

                Select Case Me.EditMode
                    Case eMouseInteractionMode.DrawShape
                        If (Me.CanEditPoints) Then
                            Me.ModifyShapePoints(Me.m_ptPosPrevious, ptPosCurrent)
                        End If

                    Case eMouseInteractionMode.DragXMark
                        Me.DragXMark(Me.m_ptPosPrevious, ptPosCurrent)
                        Me.Refresh()

                    Case eMouseInteractionMode.DragYMark
                        Me.DragYMark(Me.m_ptPosPrevious, ptPosCurrent)
                        Me.Refresh()

                    Case eMouseInteractionMode.None

                End Select

                Me.m_ptPosPrevious = ptPosCurrent
                Me.Refresh()

                If (Me.EditMode = eMouseInteractionMode.DrawShape) And (Me.CanEditPoints) Then
                    Me.OnShapeChanged()
                End If
            Else

                Me.UpdateTooltip(ptPosCurrent)

            End If

        End Sub

#End Region ' Rendering

#Region " Event handling "

#Region " Mouse events "

        ''' <summary>
        ''' Mouse move handler; draws the shape when the mouse input is captured.
        ''' </summary>
        Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)

            If (Me.Shape Is Nothing) Then Return

            ' Determine interaction mode only when not capturing input
            If (Me.Editable = True) And (Me.Capture = False) Then

                If (Me.IsNearXMark(e.X) And (Me.AllowDragXMark)) Then
                    Me.EditMode = eMouseInteractionMode.DragXMark
                    'ElseIf (Me.IsNearYMark(e.Y) And ((Me.m_editAllowed And eMouseInteractionMode.DragYMark) > 0)) Then
                    '    Me.EditMode = eMouseInteractionMode.DragYMark
                ElseIf (Me.CanEditPoints) Then
                    Me.EditMode = eMouseInteractionMode.DrawShape
                Else
                    Me.EditMode = eMouseInteractionMode.None
                End If

                ' Update cursor to provide feedback
                Me.UpdateCursor()

            End If

            Me.ProcessMouseInput(e)


        End Sub

        ''' <summary>
        ''' Mouse click handler; starts mouse capture and initiates shape drawing.
        ''' </summary>
        Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseDown(e)

            If (Me.Shape Is Nothing) Then Return

            Dim bShiftPressed As Boolean = (Control.ModifierKeys = Keys.Shift)
            If Not Me.Editable Then Return

            Me.Capture = True

            ' If NOT Shift key pressed release the last mouse pos
            If Not bShiftPressed Then Me.m_ptPosPrevious = Nothing

            Me.Shape.LockUpdates()

            Me.m_sYMaxLock = Me.YAxisMaxValue
            If Me.m_sYMaxLock = 0 Then Me.m_sYMaxLock = 2.0

            Me.ProcessMouseInput(e)

        End Sub

        ''' <summary>
        ''' Mouse up handler; finalizes the shape when the mouse is captured.
        ''' </summary>
        Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseUp(e)

            If (Me.Shape Is Nothing) Then Return

            If Not Me.Editable Then Return
            If Not Me.Capture Then Return

            ' Clear lock
            Me.m_sYMaxLock = 0.0!
            Me.Capture = False

            ' Test auto scale
            If (Me.m_scalemodeYAxis = eAxisAutoScaleModeTypes.Auto) Then
                Dim sYNew As Single = Me.m_shape.YMax
                If (sYNew <> Me.YAxisMaxValue) Then
                    Me.YAxisMaxValue = Math.Max(Me.YAxisMinValue, sYNew)
                End If
            End If

            Me.RepeatSeasonalPattern()

            ' Unlock quietly; OnShapeFinalized will inform the world
            Me.Shape.UnlockUpdates(False)
            Me.OnShapeFinalized()

            Me.Invalidate()

        End Sub

#End Region ' Mouse events

#Region " Local events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
        End Sub

        ''' <summary>
        ''' This generated the shape changed event so its thumbnail image can synchronize with it.
        ''' </summary>
        Protected Overridable Sub OnShapeChanged()
            RaiseEvent ShapeChanged(Me.Shape)
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' This generated the shape finalized event so the underlying data can be synchronized
        ''' </summary>
        Protected Overridable Sub OnShapeFinalized()
            RaiseEvent ShapeFinalized(Me.Shape, Me)
        End Sub

#End Region ' Local events

#Region " Context menu handlers "

        Private Sub UpdateMenuItemStates()

            Me.m_tsmiLine.Checked = (Me.SketchDrawMode = eSketchDrawModeTypes.Line)
            Me.m_tsmiFill.Checked = (Me.SketchDrawMode = eSketchDrawModeTypes.Fill)
            Me.m_tsmiTSDriver.Checked = (Me.SketchDrawMode = eSketchDrawModeTypes.TimeSeriesDriver)
            Me.m_tsmiTSRefAbs.Checked = (Me.SketchDrawMode = eSketchDrawModeTypes.TimeSeriesRefAbs)
            Me.m_tsmiTSRefRel.Checked = (Me.SketchDrawMode = eSketchDrawModeTypes.TimeSeriesRefRel)

            If Me.Handler IsNot Nothing Then
                Me.m_tsmiOptions.Visible = Me.Handler.SupportCommand(cShapeGUIHandler.eShapeCommandTypes.DisplayOptions)
                Me.m_tsmiOptions.Enabled = Me.Handler.EnableCommand(cShapeGUIHandler.eShapeCommandTypes.DisplayOptions)

                Me.LoadToolStripMenuItem.Visible = Me.Handler.SupportCommand(cShapeGUIHandler.eShapeCommandTypes.Load)
                Me.LoadToolStripMenuItem.Enabled = Me.Handler.EnableCommand(cShapeGUIHandler.eShapeCommandTypes.Load)

                Me.m_tsmiValue.Visible = Me.Handler.SupportCommand(cShapeGUIHandler.eShapeCommandTypes.Modify)
                Me.m_tsmiValue.Enabled = Me.Handler.EnableCommand(cShapeGUIHandler.eShapeCommandTypes.Modify)

                Me.m_tsmiReset.Visible = Me.Handler.SupportCommand(cShapeGUIHandler.eShapeCommandTypes.Reset)
                Me.m_tsmiReset.Enabled = Me.Handler.EnableCommand(cShapeGUIHandler.eShapeCommandTypes.Reset)

                Me.m_tsmiSave.Visible = Me.Handler.SupportCommand(cShapeGUIHandler.eShapeCommandTypes.SaveAsImage)
                Me.m_tsmiSave.Enabled = Me.Handler.EnableCommand(cShapeGUIHandler.eShapeCommandTypes.SaveAsImage)
            End If

        End Sub

        Private Sub LineOnlyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiLine.Click

            Me.SketchDrawMode = eSketchDrawModeTypes.Line
            Me.UpdateMenuItemStates()

        End Sub

        Private Sub FillToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiFill.Click

            Me.SketchDrawMode = eSketchDrawModeTypes.Fill
            Me.UpdateMenuItemStates()

        End Sub

        Private Sub DriverTSItemClick(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiTSDriver.Click

            Me.SketchDrawMode = eSketchDrawModeTypes.TimeSeriesDriver
            Me.UpdateMenuItemStates()

        End Sub

        Private Sub RefAbsTSItemClick(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiTSRefAbs.Click

            Me.SketchDrawMode = eSketchDrawModeTypes.TimeSeriesRefAbs
            Me.UpdateMenuItemStates()

        End Sub

        Private Sub RefRelTSItemClick(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiTSDriver.Click

            Me.SketchDrawMode = eSketchDrawModeTypes.TimeSeriesRefRel
            Me.UpdateMenuItemStates()

        End Sub
        Private Sub AxisToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiShowMarks.Click

            Me.m_tsmiShowMarks.Checked = Not Me.m_tsmiShowMarks.Checked
            Me.m_bShowAxis = m_tsmiShowMarks.Checked

        End Sub

        Private Sub AutoScaleToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

            Me.m_tsmiAutoScaleYAxis.Checked = Not Me.m_tsmiAutoScaleYAxis.Checked
            If Me.m_tsmiAutoScaleYAxis.Checked Then
                Me.m_scalemodeYAxis = eAxisAutoScaleModeTypes.Auto
            Else
                Me.m_scalemodeYAxis = eAxisAutoScaleModeTypes.Fixed
            End If

        End Sub

        Private Sub spContextMenuStrip_Opening(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) _
            Handles m_spContextMenuStrip.Opening

            Me.m_tsmiShowMarks.Checked = Me.m_bShowAxis
            Me.m_tsmiAutoScaleYAxis.Checked = (Me.m_scalemodeYAxis = eAxisAutoScaleModeTypes.Auto)

            Me.UpdateMenuItemStates()
        End Sub

        ''' <summary>
        ''' The event handler; handles a Reset toolstrip button click.
        ''' </summary>
        Private Sub OnResetShapeClick(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiReset.Click

            If Me.Handler IsNot Nothing Then
                Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Reset)
            End If
        End Sub

        ''' <summary>
        ''' Event handler; handles a Options toolstrip menu click 
        ''' </summary>
        Private Sub OnOptionClick(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiOptions.Click

            If Me.Handler IsNot Nothing Then
                Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.DisplayOptions)
            End If
        End Sub

        ''' <summary>
        ''' Event handler; handles a Save image value toolstrip item click
        ''' </summary>
        Private Sub OnSaveImageClick(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiSave.Click

            If Me.Handler IsNot Nothing Then
                Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.SaveAsImage, _
                        New cShapeData() {Me.Shape}, Me)
            End If
        End Sub

        ''' <summary>
        ''' Event handler; handles a Shape value toolstrip item click
        ''' </summary>
        Private Sub OnShapeValueClick(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsmiValue.Click

            If Me.Handler IsNot Nothing Then
                Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Modify)
            End If
        End Sub

        ''' <summary>
        ''' Event handler; handles a Load shape toolstrip item click
        ''' </summary>
        Private Sub OnLoadShapeClick(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles LoadToolStripMenuItem.Click

            If Me.Handler IsNot Nothing Then
                Me.Handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Load)
            End If
        End Sub

#End Region ' Context menu handlers

#End Region 'Event handling

    End Class

End Namespace



