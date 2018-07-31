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
Imports EwECore.Style
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    Public Class cEwEUnitLabel
        Inherits Label
        Implements IUIElement

        Private m_strTextOrg As String = ""
        Private m_uic As cUIContext = Nothing

        Public Sub New()
            MyBase.New()
        End Sub

        Public Property UIContext As cUIContext Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(uic As cUIContext)
                If (Me.m_uic IsNot Nothing) Then
                    RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If
                Me.m_uic = uic
                If (Me.m_uic IsNot Nothing) Then
                    AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If
            End Set
        End Property

        Protected Overrides Sub Dispose(disposing As Boolean)
            MyBase.Dispose(disposing)
            Me.UIContext = Nothing
        End Sub

        Public Overrides Property Text As String
            Get
                If Me.DesignMode Or Me.UIContext Is Nothing Then
                    If (Me.m_strTextOrg Is Nothing) Then Return Me.GetType().ToString
                    Return Me.m_strTextOrg
                End If
                Dim unit As New cUnits(Me.UIContext.Core)
                Return unit.ToString(Me.m_strTextOrg)
            End Get
            Set(strText As String)

                Me.m_strTextOrg = strText
                MyBase.Text = strText

            End Set
        End Property

        Private Sub OnStyleGuideChanged(changeType As cStyleGuide.eChangeType)
            Me.Invalidate()
        End Sub

    End Class

End Namespace
