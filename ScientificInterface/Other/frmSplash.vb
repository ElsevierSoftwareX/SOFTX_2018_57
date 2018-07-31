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
Imports System.Drawing.Text
Imports EwEUtils.Utilities

#End Region ' Imports

Friend Class frmSplash

    Private Const CS_DROPSHADOW As Integer = &H20000
    Private m_img As Bitmap = Nothing
    Private Shared g_instance As frmSplash

    Public Sub New()
        Me.InitializeComponent()
        frmSplash.g_instance = Me
    End Sub

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_img = cDrawingUtils.BitmapFromIcon(cEwEIcon.Current(), Me.ClientRectangle.Size)
        Me.TransparencyKey = Me.BackColor

        Me.CenterToScreen()
        Me.TopMost = True
        Me.Visible = (Me.m_img IsNot Nothing)

    End Sub

    Public Overloads Sub Close()
        If (Me.InvokeRequired) Then
            Me.BeginInvoke(New MethodInvoker(AddressOf MyBase.Close))
        Else
            Me.Close()
        End If
    End Sub

    Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        frmSplash.g_instance = Nothing
        Me.m_img.Dispose()
    End Sub

    Protected Overrides Sub OnPaint(e As System.Windows.Forms.PaintEventArgs)
        ' NOP
    End Sub

    Protected Overrides Sub OnPaintBackground(e As System.Windows.Forms.PaintEventArgs)

        Dim g As Graphics = e.Graphics

        g.InterpolationMode = InterpolationMode.High
        g.SmoothingMode = SmoothingMode.HighQuality
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit
        g.CompositingQuality = CompositingQuality.HighQuality

        g.DrawImage(Me.m_img, Me.ClientRectangle)

        'Using p As New GraphicsPath()
        '    Dim strBitApp As String = If(cSystemUtils.Is64BitProcess, SharedResources.ABOUT_64BIT, SharedResources.ABOUT_32BIT)
        '    p.AddString(cStringUtils.Localize(My.Resources.ABOUT_VERSION, cCore.Version(True), strBitApp), FontFamily.GenericSansSerif, FontStyle.Regular, g.DpiY * 12 / 72, New Point(0, 0), New StringFormat())
        '    g.DrawPath(Pens.Black, p)
        '    g.FillPath(Brushes.White, p)
        'End Using

    End Sub

    Public Shared Function GetInstance() As frmSplash
        Return frmSplash.g_instance
    End Function

End Class