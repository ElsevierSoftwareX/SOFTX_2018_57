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

Option Strict On

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Control that draws a horizontal or vertical line.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <ToolboxBitmap(GetType(ucFormSeparator), "ucFormSeparator.ico")> _
    Public Class ucFormSeparator

        Public Sub New()
            Me.InitializeComponent()
            Me.Horizontal = True
        End Sub

        Public Property Horizontal As Boolean

        Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)

            Using br As New SolidBrush(Me.BackColor)
                e.Graphics.FillRectangle(br, 0, 0, Width, Height)
            End Using

            If Me.Horizontal Then
                Using p As New Pen(System.Drawing.SystemColors.ControlDark, 1)
                    e.Graphics.DrawLine(p, 0, 0, Me.Width, 0)
                End Using
                Using p As New Pen(System.Drawing.SystemColors.ControlLightLight, 1)
                    e.Graphics.DrawLine(p, 0, 1, Me.Width, 1)
                End Using
            Else
                Using p As New Pen(System.Drawing.SystemColors.ControlDark, 1)
                    e.Graphics.DrawLine(p, 0, 0, 0, Me.Height)
                End Using
                Using p As New Pen(System.Drawing.SystemColors.ControlLightLight, 1)
                    e.Graphics.DrawLine(p, 1, Me.Height, 1, Me.Height)
                End Using
            End If

        End Sub

    End Class

End Namespace
