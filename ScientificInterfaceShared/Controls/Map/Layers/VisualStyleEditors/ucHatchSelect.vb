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
Imports System.Drawing.Drawing2D

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Combo box dropdown control that allows the user to pick a hatch pattern
    ''' from a range of provided system hatch brushes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucHatchSelect

#Region " Privates "

        Private m_dtBrushes As New Dictionary(Of HatchStyle, ucHatch)
        Private m_hbsSelected As HatchStyle = Drawing2D.HatchStyle.Cross
        Private m_bHasFocus As Boolean = False
        Private m_parent As ucEditHatch = Nothing

#End Region ' Privates

        Public Sub New(ByVal parent As ucEditHatch)

            ' This call is required by the Windows Form Designer.
            InitializeComponent()

            Me.m_parent = parent

            Me.Dock = DockStyle.Fill
        End Sub

#Region " Public interfaces "

        Public Property SelectedHatchStyle() As HatchStyle
            Get
                Return Me.m_hbsSelected
            End Get
            Set(ByVal value As HatchStyle)

                If value <> Me.m_hbsSelected Then
                    Me.m_dtBrushes(Me.m_hbsSelected).Selected = False
                    Me.m_hbsSelected = value
                    Me.m_dtBrushes(Me.m_hbsSelected).Selected = True
                    ' Update parent
                    Me.m_parent.SelectedHatchStyle = value
                End If

            End Set
        End Property

        Public Sub Colours(ByVal clrFore As Color, ByVal clrBack As Color)
            For Each uc As ucHatch In Me.m_dtBrushes.Values
                uc.Colours(clrFore, clrBack)
            Next
        End Sub

#End Region ' Public interfaces

#Region " Events "

        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)

            Me.SuspendLayout()

            ' Plunder HatchStyle enum and generate an image for each
            For Each hbs As HatchStyle In [Enum].GetValues(GetType(HatchStyle))
                If Not Me.m_dtBrushes.ContainsKey(hbs) Then
                    Dim uc As New ucHatch(Me, hbs)
                    Me.flpItems.Controls.Add(uc)
                    Me.m_dtBrushes.Add(hbs, uc)

                    AddHandler uc.Click, AddressOf OnHatchClick
                    AddHandler uc.DoubleClick, AddressOf OnHatchDoubleClick
                End If
            Next

            Me.ResumeLayout()
        End Sub

        Private Sub ucHatchSelect_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed

            ' Clean-up
            For Each ctrl As Control In Me.flpItems.Controls
                If TypeOf ctrl Is ucHatch Then
                    Dim uc As ucHatch = DirectCast(ctrl, ucHatch)

                    RemoveHandler uc.Click, AddressOf OnHatchClick
                    RemoveHandler uc.DoubleClick, AddressOf OnHatchDoubleClick
                End If
            Next
            Me.flpItems.Controls.Clear()

        End Sub

#End Region ' Events

#Region " Internal implementation "

        Private Sub OnHatchClick(ByVal sender As Object, ByVal e As EventArgs)
            Debug.Assert(TypeOf sender Is ucHatch)
            Me.SelectedHatchStyle = DirectCast(sender, ucHatch).HatchStyle
        End Sub

        Private Sub OnHatchDoubleClick(ByVal sender As Object, ByVal e As EventArgs)
            Me.m_parent.HideDropdown()
        End Sub

#End Region ' Internal implementation

    End Class

End Namespace
