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
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' UnitCell implements a cell that shows a dynamic unit string.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class EwEUnitCell
        : Inherits EwECell

        Protected m_strUnit As String = ""
        Protected m_strUnitMask As String = ""

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal strUnit As String)
            Me.New("{0}", strUnit)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal strUnitMask As String, strUnit As String)
            MyBase.New(Nothing, GetType(String))

            Me.m_strUnitMask = strUnitMask
            Me.m_strUnit = strUnit
        End Sub

#End Region ' Construction 

#Region " Overrides "

        Public Overrides ReadOnly Property DisplayText() As String
            Get
                Dim strDisplayText As String = ""
                If (Not String.IsNullOrWhiteSpace(Me.m_strUnit)) Then
                    strDisplayText = cStringUtils.Localize(Me.m_strUnitMask, GetUnitString(Me.m_strUnit))
                End If
                Return strDisplayText
            End Get
        End Property

        Private Function GetUnitString(ByVal strUnit As String) As String

            If (Me.StyleGuide Is Nothing) Then Return "u1"
            Return Me.StyleGuide.FormatUnitString(strUnit)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to enusre that this cell cannot be edited.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overrides Property Style() As cStyleGuide.eStyleFlags
            Get
                Return (MyBase.Style Or cStyleGuide.eStyleFlags.NotEditable)
            End Get
            Set(ByVal styleNew As cStyleGuide.eStyleFlags)
                MyBase.Style = (styleNew Or cStyleGuide.eStyleFlags.NotEditable)
            End Set
        End Property

#End Region ' Overrides

    End Class

End Namespace
