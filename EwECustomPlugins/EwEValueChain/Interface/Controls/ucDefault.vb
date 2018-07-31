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
Imports System.Windows.Forms
Imports EwEUtils.Database.cEwEDatabase
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style

Public Class ucDefault
    Inherits UserControl
    Implements IUIElement

    Private m_bSelected As Boolean = False
    Private m_obj As cOOPStorable = Nothing
    Private m_uic As cUIContext = Nothing

    Public Property Selected() As Boolean
        Get
            Return Me.m_bSelected
        End Get
        Set(ByVal value As Boolean)
            Me.m_bSelected = value
            Me.Invalidate()
        End Set
    End Property

    Public Property ObjDefault() As cOOPStorable
        Get
            Return Me.m_obj
        End Get
        Set(ByVal value As cOOPStorable)
            Me.m_obj = value
            Me.Invalidate()
        End Set
    End Property

    Public Property UIContext() As cUIContext _
        Implements IUIElement.UIContext
        Get
            Return Me.m_uic
        End Get
        Set(ByVal value As cUIContext)
            If (Me.m_uic IsNot Nothing) Then
                RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
            End If
            Me.m_uic = value
            If (Me.m_uic IsNot Nothing) Then
                AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
            End If
        End Set
    End Property

    Protected ReadOnly Property StyleGuide() As cStyleGuide
        Get
            Return Me.m_uic.StyleGuide
        End Get
    End Property

    Protected Overridable Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
        ' NOP
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'ucDefault
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Name = "ucDefault"
        Me.ResumeLayout(False)

    End Sub
End Class
