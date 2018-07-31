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
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Properties

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' A numerical expression that does not change value
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Class cStaticOperand
        : Inherits cExpression

        ''' <summary>The constant value of this expression</summary>
        Private m_sValue As Single

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="s">The value of this expression</param>
        ''' ---------------------------------------------------------------
        Public Sub New(ByVal s As Single)
            Me.m_sValue = s
        End Sub

        Protected Overrides Sub Dispose(ByVal bDisposing As Boolean)
            ' NOP
        End Sub

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the value of this expression
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Overrides Function GetValue() As Single
            Return Me.m_sValue
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the static <see cref="cStyleGuide.eStyleFlags">style</see>
        ''' of this expression.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Overrides Function GetStyle() As cStyleGuide.eStyleFlags
            Return cStyleGuide.eStyleFlags.OK
        End Function

    End Class

End Namespace
