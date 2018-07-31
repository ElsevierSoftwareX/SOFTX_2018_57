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
Imports System.ComponentModel
Imports ScientificInterfaceShared.Style

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Diagram element to wrap and reflect all existing links between two units in
''' the value chain.
''' </summary>
''' ===========================================================================
Public Class LinkWrapper

    Private Shared _iColorSeed_ As Integer = 0

    Private m_strName As String
    Private m_lLinks As New List(Of cLink)
    Private m_color As Color

    Public Sub New(ByVal link As cLink)
        Me.m_lLinks.Add(link)
        Me.m_color = Me.GetNextColor()
    End Sub

    Public Sub AddLink(ByVal link As cLink)
        ' Sanity checks
        Debug.Assert(Me.HasLink(link))
        Debug.Assert(Not Me.m_lLinks.Contains(link))

        Me.m_lLinks.Add(link)
    End Sub

    Public Sub RemoveLink(ByVal link As cLink)
        ' Sanity checks
        Debug.Assert(Me.HasLink(link))
        Debug.Assert(Me.m_lLinks.Contains(link))

        Me.m_lLinks.Remove(link)
    End Sub

    Public ReadOnly Property Links() As cLink()
        Get
            Return Me.m_lLinks.ToArray
        End Get
    End Property

    Public ReadOnly Property LinkCount() As Integer
        Get
            Return Me.m_lLinks.Count
        End Get
    End Property

    Public ReadOnly Property Color() As Color
        Get
            Return Me.m_color
        End Get
    End Property

    Public ReadOnly Property Source() As cUnit
        Get
            If Me.LinkCount = 0 Then Return Nothing
            Return Me.m_lLinks(0).Source
        End Get
    End Property

    Public ReadOnly Property Target() As cUnit
        Get
            If Me.LinkCount = 0 Then Return Nothing
            Return Me.m_lLinks(0).Target
        End Get
    End Property

    Public ReadOnly Property Width() As Single
        Get
            Dim w As Single = 0
            For Each l As cLink In Me.m_lLinks
                w = Math.Max(w, l.BiomassRatio)
            Next
            Return w
        End Get
    End Property

    Public ReadOnly Property Style() As cStyleGuide.eStyleFlags
        Get
            Dim s As cStyleGuide.eStyleFlags = 0
            For Each l As cLink In Me.m_lLinks
                s = s Or l.Style
            Next
            Return s
        End Get
    End Property

    Public ReadOnly Property External() As Boolean
        Get
            Dim bExt As Boolean = False
            For Each l As cLink In Me.m_lLinks
                bExt = bExt Or l.External
            Next
            Return bExt
        End Get
    End Property

    Public Function HasLink(ByVal obj As Object) As Boolean
        If TypeOf obj Is cLink Then
            Return ReferenceEquals(DirectCast(obj, cLink).Source, Me.Source) And _
                   ReferenceEquals(DirectCast(obj, cLink).Target, Me.Target)
        End If
        Return False
    End Function

#Region " Internals "

    Private Function GetNextColor() As Color
        _iColorSeed_ = (_iColorSeed_ + 1) Mod (255 * 255 * 255 - 1)

        Dim iRed As Byte = CByte((_iColorSeed_ >> 16) And &HFF)
        Dim iGreen As Byte = CByte((_iColorSeed_ >> 8) And &HFF)
        Dim iBlue As Byte = CByte(_iColorSeed_ And &HFF)

        Return Color.FromArgb(255, iRed, iGreen, iBlue)
    End Function

#End Region ' Internals

End Class
