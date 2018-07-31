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
Imports EwECore

#End Region

Namespace Controls

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' <para>ComboBox-derived class that drops down any custom control.</para>
    ''' <para>This class was based on the Custom ComboBox by Jaredpar,
    ''' http://blogs.msdn.com/jaredpar/archive/2006/10/13/custom-combobox.aspx</para>
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    <CLSCompliant(True)> _
    Public Class cCustomToolstripComboBox

        ''' <summary>Form to display the control.</summary>
        Private m_form As Form = Nothing
        ''' <summary>Original drop down height, preserved.</summary>
        Private m_iDropDownHeight As Integer = 0
        ''' <summary>The actual drop down control.</summary>
        Private m_control As Control = Nothing

        Public Sub New()

            Me.InitializeComponent()

            ' Setup the form to display the control
            Me.m_form = New Form()
            Me.m_form.StartPosition = FormStartPosition.Manual
            Me.m_form.FormBorderStyle = FormBorderStyle.None
            Me.m_form.Hide()
            Me.m_form.ShowInTaskbar = False

            ' Default Control
            Me.DropdownControl = New Control()
            Me.m_iDropDownHeight = Me.DropDownHeight
            ' Prevent the original dropdown from showing
            Me.DropDownHeight = 1
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                ' Clean up
                Me.DropdownControl = Nothing
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        Protected Overrides Sub OnDropDown(ByVal e As System.EventArgs)
            MyBase.OnDropDown(e)

            If Not Me.m_form.Visible Then
                Me.DisplayControl()
            End If

            Me.DroppedDown = False
        End Sub

        Private Sub DisplayControl()
            Dim loc As Point = Me.Control.PointToScreen(Point.Empty)
            loc.Y += Me.Height

            Me.m_form.Location = loc
            Me.m_form.Width = Me.DropDownWidth
            Me.m_form.Height = Me.m_iDropDownHeight
            Me.m_form.Show()
        End Sub

        Public Property DropdownControl() As Control
            Get
                Return Me.m_control
            End Get
            Set(ByVal value As Control)
                'Remove the existing control
                If (Not Me.m_control Is Nothing) Then
                    Me.m_form.Controls.Remove(Me.m_control)
                    RemoveHandler m_control.LostFocus, AddressOf Me.OnControlLostFocus
                    RemoveHandler m_control.DoubleClick, AddressOf Me.OnControlDoubleClick
                End If

                Me.m_control = value

                If (Not Me.m_control Is Nothing) Then
                    'Setup the new control 
                    Me.m_control.Dock = DockStyle.Fill
                    AddHandler Me.m_control.LostFocus, AddressOf Me.OnControlLostFocus
                    AddHandler Me.m_control.DoubleClick, AddressOf Me.OnControlDoubleClick
                    Me.m_form.Controls.Add(Me.m_control)
                End If

            End Set
        End Property

        Private Sub OnControlLostFocus(ByVal sender As Object, ByVal e As EventArgs)
            Me.m_form.Hide()
        End Sub

        Private Sub OnControlDoubleClick(ByVal sender As Object, ByVal e As EventArgs)
            Me.m_form.Hide()
        End Sub

        Private Sub CustomToolstripComboBox_LocationChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles Me.LocationChanged
            Me.m_form.Hide()
        End Sub

        Private Sub cCustomToolstripComboBox_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles Me.TextChanged
            Me.m_form.Hide()
        End Sub

    End Class

End Namespace ' Controls

