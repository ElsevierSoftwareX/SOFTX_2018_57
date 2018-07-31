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
Imports System.Drawing.Drawing2D
Imports EwECore.Auxiliary

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control for editing the hatch part of a <see cref="cVisualStyle"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucEditHatch

#Region " Private parts "

        Private m_uic As cUIContext = Nothing
        Private m_hbs As HatchStyle = HatchStyle.DottedDiamond
        Private m_clrFore As Color = Color.Black
        Private m_clrBack As Color = Color.Transparent
        Private m_selectionType As eSelectionType = eSelectionType.ForeColor
        Private m_form As Form = Nothing
        Private m_control As ucHatchSelect = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Enum eSelectionType As Byte
            ForeColor
            BackColor
        End Enum

#End Region ' Private parts

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="uic">UIContext to operate onto.</param>
        ''' <param name="vs">The <see cref="cVisualStyle"/> to create the editor for.</param>
        ''' <param name="style">Aspect of the style that needs editing.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                         ByVal vs As cVisualStyle, _
                         ByVal style As cVisualStyle.eVisualStyleTypes)
            MyBase.New(uic, vs, style)

            Me.InitializeComponent()

            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            Me.SetStyle(ControlStyles.ResizeRedraw, True)

            Me.m_control = New ucHatchSelect(Me)

            Me.m_form = New Form()
            Me.m_form.StartPosition = FormStartPosition.Manual
            Me.m_form.FormBorderStyle = FormBorderStyle.None
            Me.m_form.Hide()
            Me.m_form.ShowInTaskbar = False
            Me.m_form.Controls.Add(Me.m_control)
            Me.m_form.Width = Me.m_control.Width + 24
            Me.m_form.Height = Me.m_control.Height + 10

            Me.SelectedForeColor = vs.ForeColour
            Me.SelectedBackColor = vs.BackColour
            Me.SelectedHatchStyle = vs.HatchStyle

            Me.UpdateControls()

        End Sub

#End Region ' Constructor

#Region " Overrides "

        Public Overrides Function Apply(ByVal vs As cVisualStyle) As Boolean
            vs.ForeColour = Me.SelectedForeColor
            vs.BackColour = Me.SelectedBackColor
            vs.HatchStyle = Me.SelectedHatchStyle
            Return True
        End Function

#End Region ' Overrides

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property SelectedForeColor() As Color
            Get
                Return Me.m_clrFore
            End Get
            Set(ByVal value As Color)
                Me.m_clrFore = value
                Me.UpdateControls()
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property SelectedBackColor() As Color
            Get
                Return Me.m_clrBack
            End Get
            Set(ByVal value As Color)
                Me.m_clrBack = value
                Me.UpdateControls()
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property SelectedHatchStyle() As HatchStyle
            Get
                Return Me.m_hbs
            End Get
            Set(ByVal value As HatchStyle)
                Me.m_hbs = value
                Me.UpdateColors()
            End Set
        End Property

#End Region ' Internals

#Region " Events "

        Private Sub pbBrush_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbBrush.Click
            Me.DisplayDropdown()
        End Sub

        Private Sub pbForeColor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles plForeColor.Click
            SelectCustomControl(eSelectionType.ForeColor)
        End Sub

        Private Sub pbForeColor_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles plForeColor.DoubleClick
            SelectCustomControl(eSelectionType.ForeColor)
            DisplayDropdown()
        End Sub

        Private Sub pbBackColor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles plBackColor.Click
            SelectCustomControl(eSelectionType.BackColor)
        End Sub

        Private Sub pbBackColor_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles plBackColor.Click
            SelectCustomControl(eSelectionType.BackColor)
            DisplayDropdown()
        End Sub

        Private Sub nud_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles nudRed.ValueChanged, nudGreen.ValueChanged, nudBlue.ValueChanged, nudAlpha.ValueChanged
            Dim clr As Color = Color.FromArgb(CInt(nudAlpha.Value), CInt(nudRed.Value), CInt(nudGreen.Value), CInt(nudBlue.Value))

            Select Case Me.m_selectionType
                Case eSelectionType.ForeColor
                    Me.m_clrFore = clr
                Case eSelectionType.BackColor
                    Me.m_clrBack = clr
            End Select

            Me.UpdateColors()
        End Sub

        Private Sub tb_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbRed.ValueChanged, tbBlue.ValueChanged, tbGreen.ValueChanged, tbAlpha.ValueChanged
            Dim clr As Color = Color.FromArgb(CInt(tbAlpha.Value), CInt(tbRed.Value), CInt(tbGreen.Value), CInt(tbBlue.Value))

            Select Case Me.m_selectionType
                Case eSelectionType.ForeColor
                    Me.m_clrFore = clr
                Case eSelectionType.BackColor
                    Me.m_clrBack = clr
            End Select

            Me.UpdateColors()
        End Sub

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)
            Me.UpdateControls()
        End Sub

        Private Sub pbBrush_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pbBrush.Paint

            Dim br As Brush = Nothing

            e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle)
            If ((Me.RepresentationStyles And cVisualStyle.eVisualStyleTypes.Hatch) > 0) Then
                br = New HatchBrush(Me.m_hbs, Me.m_clrFore, Me.m_clrBack)
            Else
                br = New SolidBrush(Me.m_clrFore)
            End If
            e.Graphics.FillRectangle(br, e.ClipRectangle)
            br.Dispose()
            br = Nothing

        End Sub

        Private Sub pbForeColor_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) _
            Handles plForeColor.Paint

            Dim rcOuter As New Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height)
            Dim rcInner As New Rectangle(e.ClipRectangle.X + 3, e.ClipRectangle.Y + 3, e.ClipRectangle.Width - 6, e.ClipRectangle.Height - 6)

            If plForeColor.Enabled Then
                e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle)
                Using br As New SolidBrush(Me.m_clrFore)
                    e.Graphics.FillRectangle(br, rcInner)
                End Using
                If Me.m_selectionType = eSelectionType.ForeColor Then
                    e.Graphics.DrawRectangle(Pens.Black, rcOuter)
                End If
            Else
                e.Graphics.FillRectangle(SystemBrushes.Control, e.ClipRectangle)
            End If

        End Sub

        Private Sub pbBackColor_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) _
            Handles plBackColor.Paint

            Dim rcOuter As New Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height)
            Dim rcInner As New Rectangle(e.ClipRectangle.X + 3, e.ClipRectangle.Y + 3, e.ClipRectangle.Width - 6, e.ClipRectangle.Height - 6)

            If plBackColor.Enabled Then
                e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle)
                Using br As New SolidBrush(Me.m_clrBack)
                    e.Graphics.FillRectangle(br, rcInner)
                End Using
                If Me.m_selectionType = eSelectionType.BackColor Then
                    e.Graphics.DrawRectangle(Pens.Black, rcOuter)
                End If
            Else
                e.Graphics.FillRectangle(SystemBrushes.Control, e.ClipRectangle)
            End If

        End Sub

        Private Sub OnDropDownLostFocus(ByVal sender As Object, ByVal e As EventArgs)
            HideDropdown()
        End Sub

        Private Sub OnDropDownDoubleClick(ByVal sender As Object, ByVal e As EventArgs)
            HideDropdown()
        End Sub

#End Region ' Events 

#Region " Internal implementation "

        Private Sub UpdateControls()

            ' Brush picker
            Me.pbBrush.Enabled = ((Me.RepresentationStyles And cVisualStyle.eVisualStyleTypes.Hatch) > 0)
            Me.pbBrush.BorderStyle = If(Me.pbBrush.Enabled, BorderStyle.Fixed3D, BorderStyle.FixedSingle)

            Me.plBackColor.Enabled = ((Me.RepresentationStyles And cVisualStyle.eVisualStyleTypes.BackColor) > 0)
            Me.plBackColor.BorderStyle = If(Me.plBackColor.Enabled, BorderStyle.Fixed3D, BorderStyle.FixedSingle)

            Me.plForeColor.Enabled = ((Me.RepresentationStyles And cVisualStyle.eVisualStyleTypes.ForeColor) > 0)
            Me.plForeColor.BorderStyle = If(Me.plForeColor.Enabled, BorderStyle.Fixed3D, BorderStyle.FixedSingle)

            Me.UpdateColors()
        End Sub

        ''' <summary>Loop prevention flag.</summary>
        Private m_bInUpdate As Boolean = False

        Private Sub UpdateColors()

            If m_bInUpdate Then Return

            Me.m_bInUpdate = True

            Dim clr As Color = If(Me.m_selectionType = eSelectionType.ForeColor, Me.m_clrFore, Me.m_clrBack)
            Dim bEnabled As Boolean = (Me.RepresentationStyles And (cVisualStyle.eVisualStyleTypes.BackColor Or cVisualStyle.eVisualStyleTypes.ForeColor)) > 0

            Me.tbRed.Value = clr.R
            Me.tbRed.Enabled = bEnabled
            Me.nudRed.Value = clr.R
            Me.nudRed.Enabled = bEnabled

            Me.tbGreen.Value = clr.G
            Me.tbGreen.Enabled = bEnabled
            Me.nudGreen.Value = clr.G
            Me.nudGreen.Enabled = bEnabled

            Me.tbBlue.Value = clr.B
            Me.tbBlue.Enabled = bEnabled
            Me.nudBlue.Value = clr.B
            Me.nudBlue.Enabled = bEnabled

            Me.tbAlpha.Value = clr.A
            Me.tbAlpha.Enabled = bEnabled
            Me.nudAlpha.Value = clr.A
            Me.nudAlpha.Enabled = bEnabled

            Me.plBackColor.Refresh()
            Me.plForeColor.Refresh()
            Me.pbBrush.Refresh()

            Me.FireStyleChangedEvent()

            Me.m_bInUpdate = False

        End Sub

        Private Sub SelectCustomControl(ByVal selType As eSelectionType)

            Dim dlg As New ColorDialog()

            If (Me.m_selectionType <> selType) Then
                Me.m_selectionType = selType
                Me.UpdateControls()
                Return
            End If

            Select Case selType
                Case eSelectionType.BackColor
                    dlg.Color = Me.m_clrBack

                Case eSelectionType.ForeColor
                    dlg.Color = Me.m_clrFore
            End Select

            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return

            Select Case selType
                Case eSelectionType.BackColor
                    Me.m_clrBack = dlg.Color

                Case eSelectionType.ForeColor
                    Me.m_clrFore = dlg.Color
            End Select

            Me.UpdateControls()

        End Sub

        Friend Sub DisplayDropdown()
            Dim loc As Point = Me.PointToScreen(Point.Empty)
            loc.Y += Me.pbBrush.Height + Me.pbBrush.Location.Y
            loc.X += Me.pbBrush.Location.X

            m_control.Colours(Me.m_clrFore, Me.m_clrBack)

            m_form.Location = loc
            m_form.Show()
            m_form.Focus()

            AddHandler m_control.LostFocus, AddressOf Me.OnDropDownLostFocus
            AddHandler m_control.DoubleClick, AddressOf Me.OnDropDownDoubleClick
        End Sub

        Friend Sub HideDropdown()
            RemoveHandler m_control.LostFocus, AddressOf Me.OnDropDownLostFocus
            RemoveHandler m_control.DoubleClick, AddressOf Me.OnDropDownDoubleClick
            m_form.Hide()
        End Sub

#End Region ' Internal implementation

    End Class

End Namespace
