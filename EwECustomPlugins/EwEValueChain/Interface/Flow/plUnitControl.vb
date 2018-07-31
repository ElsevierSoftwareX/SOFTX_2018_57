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
Imports System.Drawing
Imports System.Windows.Forms
Imports EwEUtils.SystemUtilities
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports EwEUtils.Database.cEwEDatabase
Imports EwEUtils.Utilities

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Panel to interact with a single unit in a flow.
''' </summary>
''' ===========================================================================
Public Class plUnitControl
    Inherits Panel

#Region " Private vars "

    Private components As System.ComponentModel.IContainer = Nothing
    Private m_uic As cUIContext = Nothing
    Private m_fp As cFlowPosition = Nothing
    Private m_bInUpdate As Boolean = False
    Private m_sScale As Single = 1.0

#End Region ' Private vars

#Region " Constructors "

    Public Sub New(ByVal uic As cUIContext, ByVal fp As cFlowPosition)

        Debug.Assert(fp IsNot Nothing)

        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.AllowDrop = True

        Me.m_fp = fp
        Me.Name = Me.m_fp.Unit.Name
        Me.m_uic = uic

        ' Auto-repos
        Me.OnPositionChanged(Me.m_fp)

        AddHandler Me.m_fp.OnChanged, AddressOf OnPositionChanged
        AddHandler Me.m_fp.Unit.OnChanged, AddressOf OnDataChanged
        AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleguideChanged

    End Sub

#End Region ' Constructors

#Region " Events "

    Private Sub OnDataChanged(ByVal obj As cOOPStorable)
        Me.Invalidate()
    End Sub

    Private Sub OnPositionChanged(ByVal obj As cOOPStorable)

        If Me.m_bInUpdate Then Return
        Me.m_bInUpdate = True

        Debug.Assert(TypeOf obj Is cFlowPosition)
        Me.Location = New Point(CInt(Me.m_fp.Xpos * Me.m_sScale), CInt(Me.m_fp.Ypos * Me.m_sScale))
        Me.Size = New Size(CInt(Me.m_fp.Width * Me.m_sScale), CInt(Me.m_fp.Height * Me.m_sScale))

        Me.m_bInUpdate = False

    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)

        If (Me.m_fp IsNot Nothing) Then

            RemoveHandler Me.m_fp.OnChanged, AddressOf OnPositionChanged
            RemoveHandler Me.m_fp.Unit.OnChanged, AddressOf OnDataChanged
            RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleguideChanged

            Me.m_fp = Nothing
            Me.m_uic = Nothing

        End If
        MyBase.Dispose(disposing)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler to process a mouse click.
    ''' </summary>
    ''' <remarks>
    ''' Handling is outsourced to the master panel which will process the click 
    ''' based on the current interaction mode.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnMouseDown(e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseDown(e)
        Debug.Assert(Me.FlowPanel IsNot Nothing)
        Me.FlowPanel.OnUnitMouseDown(Me)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler to start a mouse hover event.
    ''' </summary>
    ''' <remarks>
    ''' Handling is outsourced to the master panel which will process the hover
    ''' based on the current interaction mode.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnMouseEnter(e As System.EventArgs)
        MyBase.OnMouseEnter(e)
        If (Me.FlowPanel IsNot Nothing) Then
            Me.FlowPanel.OnUnitMouseHover(Me, True)
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler to start a mouse hover event.
    ''' </summary>
    ''' <remarks>
    ''' Handling is outsourced to the master panel which will process the hover
    ''' based on the current interaction mode.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnMouseLeave(e As System.EventArgs)
        MyBase.OnMouseLeave(e)
        ' Could be due to deletion
        If (Me.FlowPanel IsNot Nothing) Then
            Me.FlowPanel.OnUnitMouseHover(Me, False)
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler to update the underlying flow position instance whenever 
    ''' this control has been repositioned.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub OnLocationChanged(ByVal e As System.EventArgs)
        MyBase.OnLocationChanged(e)
        Me.m_fp.Xpos = CInt(Me.Location.X / Me.m_sScale)
        Me.m_fp.Ypos = CInt(Me.Location.Y / Me.m_sScale)
    End Sub

    Protected Overrides Sub OnSizeChanged(ByVal e As System.EventArgs)
        MyBase.OnSizeChanged(e)
        Me.m_fp.Width = CInt(Me.Width / Me.m_sScale)
        Me.m_fp.Height = CInt(Me.Height / Me.m_sScale)
    End Sub

    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        MyBase.OnPaint(e)

        Dim sg As cStyleGuide = Me.m_uic.StyleGuide
        Dim rc As Rectangle = Me.ClientRectangle
        Dim clrBackground As Color = Color.Black
        Dim clrBorder As Color = Color.Black
        Dim clrText As Color = Color.Black
        Dim img As Image = Nothing

        ' Adjust rect
        rc.Width -= 1
        rc.Height -= 1

        ' Get style colors
        sg.GetStyleColors(Me.Unit.Style, clrText, clrBackground)

        If Me.Selected Then
            clrBackground = sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)
        ElseIf Not Me.Unit.CanCompute Then
            clrBackground = sg.ApplicationColor(cStyleGuide.eApplicationColorType.MISSINGPARAMETER_BACKGROUND)
#If DEBUG Then
        Else
            If Me.Unit.IsRunError Then
                clrBackground = Color.Red
            End If
#End If
        End If


        ' Clear background
        Using br As New SolidBrush(clrBackground)
            e.Graphics.FillRectangle(br, rc)
        End Using

        ' Draw unit image
        img = cUnitImageFactory.GetImage(Me.Unit.UnitType)
        If (img IsNot Nothing) Then
            Dim rcImage As Rectangle = New Rectangle(0, 0, CInt(16 * Me.ZoomFactor), CInt(16 * Me.ZoomFactor))
            If cSystemUtils.IsRightToLeft Then
                rcImage.Offset(2, Me.Height - rcImage.Height - 2)
            Else
                rcImage.Offset(Me.Width - rcImage.Width - 2, Me.Height - rcImage.Height - 2)
            End If
            e.Graphics.DrawImage(img, rcImage, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel)
        End If
        ' Color background with slight transparency to keep image visible
        clrBackground = Color.FromArgb(200, clrBackground.R, clrBackground.G, clrBackground.B)
        Using br As New SolidBrush(clrBackground)
            e.Graphics.FillRectangle(br, rc)
        End Using

        ' Paint unit name or alternate name
        Using ft As Font = sg.Font(cStyleGuide.eApplicationFontType.Scale)
            Dim strName As String = ""
            If My.Settings.ShowAltNames Then strName = Me.Unit.NameLocal
            If String.IsNullOrWhiteSpace(strName) Then strName = Me.Unit.Name

            If cColorUtils.IsDark(clrText) And cColorUtils.IsDark(clrBackground) Then
                clrText = cColorUtils.Inverse(clrText)
            End If

            Using br As New SolidBrush(clrText)
                e.Graphics.DrawString(strName, ft, br, rc)
            End Using
        End Using

        e.Graphics.DrawRectangle(Pens.Black, rc)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Copy/paste handling.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub tsmCopy_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Clipboard.SetDataObject(Me.Unit)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Copy/paste handling.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub tsmPaste_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim data As IDataObject = Clipboard.GetDataObject()
        If data.GetDataPresent(GetType(cOOPStorable)) Then
            Me.Unit.CopyFrom(DirectCast(data.GetData(GetType(cOOPStorable)), cOOPStorable))
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' cStyleGuide changed handler, caught to redraw whenever sg colours have
    ''' been modified.
    ''' </summary>
    ''' <param name="changeFlags"></param>
    ''' -----------------------------------------------------------------------
    Private Sub OnStyleguideChanged(ByVal changeFlags As cStyleGuide.eChangeType)
        ' Redraw on color or font changes
        If ((changeFlags And (cStyleGuide.eChangeType.Colours Or cStyleGuide.eChangeType.Fonts)) > 0) Then
            Me.Invalidate(True)
        End If
    End Sub

#End Region ' Events

#Region " Public interfaces "

    Public Function Center() As Point
        Dim pt As Point = Me.Location
        Dim sz As Size = Me.Size
        Return New Point(CInt((pt.X + sz.Width / 2)), CInt((pt.Y + sz.Height / 2)))
    End Function

    Public Property ZoomFactor() As Single
        Get
            Return Me.m_sScale
        End Get
        Set(ByVal value As Single)
            Me.m_sScale = value
            Me.OnPositionChanged(Me.m_fp)
        End Set
    End Property

#End Region ' Public interfaces

#Region " Selection "

    Private m_bSelected As Boolean

    Public Property Selected() As Boolean
        Get
            Return Me.m_bSelected
        End Get
        Set(ByVal value As Boolean)
            Me.m_bSelected = value
            Me.Refresh()
        End Set
    End Property

#End Region ' Selection

#Region " Public properties "

    Public ReadOnly Property Unit() As cUnit
        Get
            Return Me.m_fp.Unit
        End Get
    End Property

    Public ReadOnly Property FlowPos() As cFlowPosition
        Get
            Return Me.m_fp
        End Get
    End Property

#End Region ' Public properties

#Region " Internals "

    Private Function FlowPanel() As plFlow
        Return DirectCast(Me.Parent, plFlow)
    End Function

#End Region ' Internals

#Region " VS "

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'UnitControl
        '
        Me.ResumeLayout(False)

    End Sub

#End Region ' VS

End Class
