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
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Definitions
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.Map

    ''' <summary>
    ''' User control to interact with a single layer in a Ecospace layer stack.
    ''' </summary>
    Partial Public Class ucLayer

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        ''' <summary>Layer</summary>
        Private m_layer As cDisplayLayer = Nothing
          ''' <summary>States whether the mouse is hovering over the control.</summary>
        Private m_bHovering As Boolean = False

        ' Images cache for faster rendering
        Protected Shared g_imgEye0 As Image = My.Resources.Eye_open
        Protected Shared g_imgEye1 As Image = My.Resources.Eye_closed
        Protected Shared g_imgPen0 As Image = My.Resources.Editable
        Protected Shared g_imgPen1 As Image = My.Resources.NotEditable
        Protected Shared g_imgLock As Image = My.Resources.ProtectFormHS
        Protected Shared g_imgData As Image = My.Resources.Database
        Protected Shared g_imgDataDisabled As Image = My.Resources.database_NA

#End Region ' Private vars

#Region " Constructor / destructor "

        Public Sub New(ByVal uic As cUIContext, ByVal l As cDisplayLayer)

            Me.InitializeComponent()

            'Enable double buffering
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.ResizeRedraw, True)
            Me.SetStyle(ControlStyles.UserPaint, True)

            Me.m_uic = uic
            Me.m_layer = l

            AddHandler Me.m_layer.LayerChanged, AddressOf OnLayerChanged

            ' Kick off
            Me.OnLayerChanged(l, cDisplayLayer.eChangeFlags.Descriptive)

            Dim p As cProperty = Me.m_layer.GetNameProperty()
            If (p IsNot Nothing) Then
                AddHandler p.PropertyChanged, AddressOf OnLayerPropertyChanged
                OnLayerPropertyChanged(p, cProperty.eChangeFlags.All)
            End If

        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then

                ' Remove from event handler
                RemoveHandler m_layer.LayerChanged, AddressOf OnLayerChanged

                Dim p As cProperty = Me.m_layer.GetNameProperty()
                If (p IsNot Nothing) Then
                    RemoveHandler p.PropertyChanged, AddressOf OnLayerPropertyChanged
                End If

                Me.LayerGroup = Nothing
                Me.m_layer = Nothing

                If components IsNot Nothing Then
                    components.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

#End Region ' Constructor / destructor

#Region " Properties "

        Public ReadOnly Property Layer() As cDisplayLayer
            Get
                Return Me.m_layer
            End Get
        End Property

        Public Property LayerGroup() As ucLayerGroup

#End Region ' Properties

#Region " Internal implementation "

        Private Sub OnLayerChanged(ByVal l As cDisplayLayer, ByVal updateType As cDisplayLayer.eChangeFlags)

            If (updateType = cDisplayLayer.eChangeFlags.Map) Then
                Me.Invalidate()
                Return
            End If

            If ((updateType And cDisplayLayer.eChangeFlags.Selected) = cDisplayLayer.eChangeFlags.Selected) Then
                ' Provide instant feedback
                Me.Refresh()
            Else
                ' Just redraw whenever there is time
                Me.Invalidate()
            End If

        End Sub

        Private Sub OnLayerPropertyChanged(prop As cProperty, cf As cProperty.eChangeFlags)
            Me.Invalidate()
            cToolTipShared.GetInstance().SetToolTip(Me, prop.GetRemark)
        End Sub

        Public Sub EditLayer(ByVal edittype As eLayerEditTypes)
            If (TypeOf Me.Layer Is cDisplayLayerRaster) Then
                Try
                    Dim rl As cDisplayLayerRaster = DirectCast(Me.Layer, cDisplayLayerRaster)
                    Dim cmd As cEditLayerCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cEditLayerCommand.cCOMMAND_NAME), cEditLayerCommand)
                    cmd.Invoke(rl, Nothing, edittype)
                Catch ex As Exception
                    cLog.Write(ex, eVerboseLevel.Detailed, "ucLayer::EditLayer " & Me.Layer.Name & "(" & edittype.ToString & ")")
                End Try
            End If
        End Sub

        Public Sub EditLayerConnection()
            If (TypeOf Me.Layer Is cDisplayLayerRaster) Then
                Try
                    Dim rl As cDisplayLayerRaster = DirectCast(Me.Layer, cDisplayLayerRaster)
                    Dim cmd As cEcospaceConfigureConnectionCommand = DirectCast(Me.m_uic.CommandHandler.GetCommand(cEcospaceConfigureConnectionCommand.cCOMMAND_NAME), cEcospaceConfigureConnectionCommand)
                    cmd.Invoke(rl.Data)
                Catch ex As Exception
                    cLog.Write(ex, eVerboseLevel.Detailed, "ucLayer::EditLayerConnection " & Me.Layer.Name)
                End Try
            End If
        End Sub

        ''' <summary>
        ''' Enum to identify areas in the control
        ''' </summary>
        ''' <remarks></remarks>
        Private Enum eAreaTypes As Byte
            ''' <summary>Area not in this control.</summary>
            None
            ''' <summary>Background area of this control.</summary>
            Background
            ''' <summary>Editable area of this control.</summary>
            Editable
            ''' <summary>Visible area of this control.</summary>
            Visible
            ''' <summary>Label area of this control.</summary>
            Label
            ''' <summary>Preview area of this control.</summary>
            Preview
        End Enum

        Private Sub GetRectangles(ByVal rcControl As Rectangle, ByRef rcEditable As Rectangle, ByRef rcVisible As Rectangle, ByRef rcLabel As Rectangle, ByRef rcPreview As Rectangle)

            Dim iAvgPad As Integer = 3

            If (cSystemUtils.IsRightToLeft) Then
                ' [ [prev][label    ][vis][edt] ]
                rcEditable.X = rcControl.Width - iAvgPad - 16
                rcEditable.Y = CInt((rcControl.Height - 16) / 2)
                rcEditable.Width = 16
                rcEditable.Height = 16

                rcVisible.X = rcEditable.X - rcEditable.Width - iAvgPad
                rcVisible.Y = rcEditable.Y
                rcVisible.Width = 16
                rcVisible.Height = 16

                rcPreview.X = 2
                rcPreview.Y = 2
                rcPreview.Width = 24
                rcPreview.Height = rcControl.Height - 4

                rcLabel.X = rcPreview.X + rcPreview.Width + iAvgPad
                rcLabel.Y = 0
                rcLabel.Width = rcVisible.X - rcLabel.X - iAvgPad
                rcLabel.Height = rcControl.Height
            Else
                ' [ [edt][vis][label    ][prev] ]
                rcEditable.X = iAvgPad
                rcEditable.Y = CInt((rcControl.Height - 16) / 2)
                rcEditable.Width = 16
                rcEditable.Height = 16

                rcVisible.X = rcEditable.X + rcEditable.Width + iAvgPad
                rcVisible.Y = rcEditable.Y
                rcVisible.Width = 16
                rcVisible.Height = 16

                rcPreview.X = rcControl.Width - 2 - 24
                rcPreview.Y = 2
                rcPreview.Width = 24
                rcPreview.Height = rcControl.Height - 4

                rcLabel.X = rcVisible.X + rcVisible.Width + iAvgPad
                rcLabel.Y = 0
                rcLabel.Width = rcPreview.X - rcLabel.X - iAvgPad
                rcLabel.Height = rcControl.Height
            End If

        End Sub

        Private Function GetArea(ByVal pt As Point) As eAreaTypes
            Dim rcControl As Rectangle = New Rectangle(0, 0, Me.Width, Me.Height)
            Dim rcEditable As Rectangle = Nothing
            Dim rcVisible As Rectangle = Nothing
            Dim rcLabel As Rectangle = Nothing
            Dim rcPreview As Rectangle = Nothing

            Me.GetRectangles(rcControl, rcEditable, rcVisible, rcLabel, rcPreview)

            If rcEditable.Contains(pt) Then Return eAreaTypes.Editable
            If rcVisible.Contains(pt) Then Return eAreaTypes.Visible
            If rcLabel.Contains(pt) Then Return eAreaTypes.Label
            If rcPreview.Contains(pt) Then Return eAreaTypes.Preview
            If rcControl.Contains(pt) Then Return eAreaTypes.Background
            Return eAreaTypes.None

        End Function

#End Region ' Internal implementation

#Region " Events "

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)

            If Me.m_uic Is Nothing Then Return

            Dim rcControl As Rectangle = New Rectangle(0, 0, Me.Width, Me.Height)
            Dim rcEditable As Rectangle = Nothing
            Dim rcVisible As Rectangle = Nothing
            Dim rcLabel As Rectangle = Nothing
            Dim rcPreview As Rectangle = Nothing
            Dim prop As cProperty = Me.m_layer.GetNameProperty()
            Dim img As Image = Nothing
            Dim fmt As New StringFormat()

            Me.GetRectangles(rcControl, rcEditable, rcVisible, rcLabel, rcPreview)

            ' Paint background
            If m_layer.IsSelected Then
                e.Graphics.FillRectangle(SystemBrushes.Highlight, rcControl)
            Else
                e.Graphics.FillRectangle(SystemBrushes.Control, rcControl)
            End If

            If (TypeOf Me.m_layer Is cDisplayLayerRaster) Then
                Dim rl As cDisplayLayerRaster = DirectCast(Me.m_layer, cDisplayLayerRaster)
                ' Draw editable indicator (only when selected or hovering)
                If (rl.IsExternal) Then
                    If (rl.IsExternalEnabled) Then
                        img = g_imgData
                    Else
                        img = g_imgDataDisabled
                    End If
                ElseIf (rl.Editor.IsReadOnly) Then
                    img = g_imgLock
                Else
                    If Me.m_bHovering Or Me.m_layer.IsSelected Then
                        If rl.Editor.IsEditable Then
                            img = g_imgPen0
                        Else
                            img = g_imgPen1
                        End If
                    End If
                End If
                ' Extract property
            Else
                img = g_imgLock
            End If

            If (img IsNot Nothing) Then e.Graphics.DrawImage(img, rcEditable)

            ' Draw visible indicator
            If Me.Layer.Renderer.IsVisible Then
                img = g_imgEye0
            Else
                img = g_imgEye1
            End If
            e.Graphics.DrawImage(img, rcVisible)

            ' Draw label
            fmt.LineAlignment = StringAlignment.Center
            fmt.Alignment = StringAlignment.Near
            fmt.FormatFlags = StringFormatFlags.NoWrap
            fmt.Trimming = StringTrimming.EllipsisPath

            If Me.m_layer.IsSelected Then
                e.Graphics.DrawString(Me.Layer.DisplayText, Me.Font, SystemBrushes.HighlightText, rcLabel, fmt)
            Else
                e.Graphics.DrawString(Me.Layer.DisplayText, Me.Font, SystemBrushes.ControlText, rcLabel, fmt)
            End If

            ' Draw preview
            ' - Render representation
            e.Graphics.FillRectangle(Brushes.White, rcPreview)
            Me.m_layer.Renderer.RenderPreview(e.Graphics, rcPreview)

            If (prop IsNot Nothing) Then
                ' - Render remarks indicator
                Dim sg As cStyleGuide = Me.m_uic.StyleGuide
                If (prop.HasRemark()) Then
                    cRemarksIndicator.Paint(sg.ApplicationColor(cStyleGuide.eApplicationColorType.REMARKS_BACKGROUND), rcPreview, e.Graphics, True, cSystemUtils.IsRightToLeft)
                End If
            End If

            ' - Render border
            ControlPaint.DrawBorder3D(e.Graphics, rcPreview, Border3DStyle.Sunken,
                Border3DSide.Bottom Or Border3DSide.Left Or Border3DSide.Top Or Border3DSide.Right)

            ' Draw button borders only when hovering
            If Me.m_bHovering Then
                ControlPaint.DrawBorder(e.Graphics, rcEditable, SystemColors.ControlDark, ButtonBorderStyle.Solid)
                ControlPaint.DrawBorder(e.Graphics, rcVisible, SystemColors.ControlDark, ButtonBorderStyle.Solid)
            End If

        End Sub

        Protected Overrides Sub OnMouseClick(e As MouseEventArgs)

            Dim flag As cDisplayLayer.eChangeFlags = 0

            ' Select layer first
            If Not m_layer.IsSelected Then
                Me.m_layer.IsSelected = True
                flag = flag Or cDisplayLayer.eChangeFlags.Selected
            End If

            ' After selecting, determine hit area and process further
            Select Case Me.GetArea(e.Location)

                Case eAreaTypes.Preview
                    Me.m_layer.Update(flag) : flag = 0
                    Me.EditLayer(eLayerEditTypes.EditVisuals)

                Case eAreaTypes.Editable
                    If (TypeOf Me.m_layer Is cDisplayLayerRaster) Then
                        Dim edt As cLayerEditor = DirectCast(Me.m_layer, cDisplayLayerRaster).Editor
                        edt.IsEditable = Not edt.IsEditable
                        flag = flag Or cDisplayLayer.eChangeFlags.Editable
                    End If

                Case eAreaTypes.Label
                Case eAreaTypes.Background

                Case eAreaTypes.Visible
                    Me.m_layer.Renderer.IsVisible = Not Me.m_layer.Renderer.IsVisible
                    flag = flag Or cDisplayLayer.eChangeFlags.Visibility

            End Select

            If flag <> 0 Then
                Me.m_layer.Update(flag)
            End If

        End Sub

        Protected Overrides Sub OnDoubleClick(e As EventArgs)

            Select Case Me.GetArea(Me.PointToClient(MousePosition))
                Case eAreaTypes.Editable
                    Me.EditLayerConnection()
                Case eAreaTypes.Preview
                    Me.EditLayer(eLayerEditTypes.EditVisuals)
                Case Else
                    Me.EditLayer(eLayerEditTypes.EditData)
            End Select

        End Sub

        Private Sub ucLayer_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
            ' Determine hit area
            Select Case Me.GetArea(e.Location)
                Case eAreaTypes.Preview, eAreaTypes.Visible ', eAreaTypes.Editable 
                    ' Use hand cursor
                    Me.Cursor = Cursors.Hand
                Case Else
                    ' Use default
                    Me.Cursor = Cursors.Default
            End Select
        End Sub

        ''' <summary>
        ''' Start hovering
        ''' </summary>
        Private Sub ucLayer_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseEnter
            Me.m_bHovering = True
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Stop hovering
        ''' </summary>
        Private Sub ucLayer_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseLeave
            Me.m_bHovering = False
            Me.Invalidate()
        End Sub

#End Region ' All events 

    End Class

End Namespace