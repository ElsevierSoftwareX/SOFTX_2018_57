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

Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Drawing

#End Region ' Imports

Namespace Controls

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Extended <see cref="TabPage"/> that enables properties hidden by the WinForms 
    ''' base class.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class ucTabPageEx
        Inherits TabPage

        Public Overloads Property Enabled As Boolean
            Get
                Return MyBase.Enabled
            End Get
            Set(value As Boolean)
                MyBase.Enabled = value
                If (Me.Parent IsNot Nothing) Then
                    Me.Parent.Invalidate()
                End If
            End Set
        End Property

    End Class

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Extended <see cref="TabControl"/> that supports for horizontal tab tags
    ''' to the left or right of the tab area.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class ucTabControlEx
        Inherits TabControl

        ''' <summary>Text orientation.</summary>
        Private m_orientation As Orientation = Orientation.Horizontal
        Private m_admin As New Dictionary(Of TabPage, Boolean)
        Private m_bInUpdate As Boolean = False

        Public Sub New()
            Me.InitializeComponent()
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the enabled state of the entire tab control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overloads Property Enabled As Boolean
            Get
                Return MyBase.Enabled
            End Get
            Set(value As Boolean)
                MyBase.Enabled = value
                Me.Invalidate()
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the orientation of tab text.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property TextOrientation As Orientation
            Get
                Return Me.m_orientation
            End Get
            Set(value As Orientation)
                Me.m_orientation = value
                Me.Invalidate()
            End Set
        End Property

        Public Property IsVisible(tab As TabPage) As Boolean
            Get
                If (Me.m_admin.ContainsKey(tab)) Then Return Me.m_admin(tab)
                Return True
            End Get
            Set(value As Boolean)
                Me.m_admin(tab) = value
                Me.UpdateTabVisibility()
            End Set
        End Property

#Region " Internals "

        Protected Overrides Sub OnControlAdded(e As System.Windows.Forms.ControlEventArgs)
            MyBase.OnControlAdded(e)
            If (TypeOf e.Control Is TabPage And Not Me.m_bInUpdate) Then
                Dim tab As TabPage = DirectCast(e.Control, TabPage)
                If (Not Me.m_admin.ContainsKey(tab)) Then Me.m_admin(tab) = True
            End If
        End Sub

        Protected Overrides Sub OnControlRemoved(e As System.Windows.Forms.ControlEventArgs)
            If (TypeOf e.Control Is TabPage And Not Me.m_bInUpdate) Then
                Dim tab As TabPage = DirectCast(e.Control, TabPage)
                If (Me.m_admin.ContainsKey(tab)) Then Me.m_admin.Remove(tab)
            End If
            MyBase.OnControlRemoved(e)
        End Sub

        Protected Overrides Sub OnDrawItem(e As System.Windows.Forms.DrawItemEventArgs)

            Dim pg As TabPage = Me.TabPages(e.Index)
            Dim strText As String = pg.Text
            Dim szText As SizeF
            Dim rc As New RectangleF(e.Bounds.X, e.Bounds.Y + 2, e.Bounds.Width, e.Bounds.Height - 2)
            Dim bEnabled As Boolean = Me.Enabled
            Dim fmt As New StringFormat()

            fmt.LineAlignment = StringAlignment.Center

            If Me.TextOrientation = Orientation.Vertical Then
                fmt.FormatFlags = fmt.FormatFlags Or StringFormatFlags.DirectionVertical
            End If

            If Me.RightToLeft = System.Windows.Forms.RightToLeft.Yes Then
                fmt.Alignment = StringAlignment.Far
            Else
                fmt.Alignment = StringAlignment.Near
            End If

            szText = e.Graphics.MeasureString(strText, Me.Font)

            ' Draw back color
            If (Me.SelectedIndex = e.Index) Then
                e.Graphics.FillRectangle(SystemBrushes.ControlLightLight, e.Bounds)
            End If

            rc.X += Me.Padding.X
            rc.Width -= 2 * Me.Padding.X

            ' ToDo: include image rendering

            ' Draw text
            If (TypeOf pg Is ucTabPageEx) Then
                bEnabled = bEnabled And DirectCast(pg, ucTabPageEx).Enabled
            End If

            If bEnabled Then
                e.Graphics.DrawString(strText, Me.Font, SystemBrushes.ControlText, rc, fmt)
            Else
                e.Graphics.DrawString(strText, Me.Font, SystemBrushes.GrayText, rc, fmt)
            End If

        End Sub

        Protected Overrides Sub OnPaintBackground(e As System.Windows.Forms.PaintEventArgs)
            e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle)
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso m_components IsNot Nothing Then
                    m_components.Dispose()
                End If
                ' Manually dispose of all orphaned (e.g., invisible) tab pages
                For Each key As TabPage In Me.m_admin.Keys
                    If (Not Me.m_admin(key)) Then key.Dispose()
                Next
                Me.m_admin.Clear()
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private m_components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        Private Sub InitializeComponent()
            Me.m_components = New System.ComponentModel.Container()
        End Sub

        Private Sub UpdateTabVisibility()

            Dim tabSel As TabPage = Me.SelectedTab
            Dim bSelOK As Boolean = False
            Me.SuspendLayout()
            Me.m_bInUpdate = True
            Me.TabPages.Clear()
            For Each tab As TabPage In Me.m_admin.Keys
                If (Me.m_admin(tab)) Then
                    Me.TabPages.Add(tab)
                    bSelOK = bSelOK Or (ReferenceEquals(tab, tabSel))
                End If
            Next
            Me.m_bInUpdate = False
            If bSelOK Then
                Me.SelectedTab = tabSel
            End If
            Me.ResumeLayout()

        End Sub

#End Region ' Internals

    End Class

End Namespace ' Controls
