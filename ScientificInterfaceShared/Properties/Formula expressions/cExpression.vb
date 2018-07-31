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
    ''' Base class for a cFormulaProperty formula
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public MustInherit Class cExpression
        Implements IDisposable

#Region " Disposal "

        ''' <summary>To detect redundant calls.</summary>
        Private m_bDisposed As Boolean = False        ' 

        Public Sub Dispose() _
            Implements IDisposable.Dispose
            If Not Me.m_bDisposed Then
                Me.Dispose(True)
                Me.m_bDisposed = True
            End If
            GC.SuppressFinalize(Me)
        End Sub

        Protected MustOverride Sub Dispose(ByVal bDisposing As Boolean)

#End Region ' Disposal

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the value of this expression.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public MustOverride Function GetValue() As Single

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the style of this expression.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public MustOverride Function GetStyle() As cStyleGuide.eStyleFlags

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Change notification event delegate.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Delegate Sub ValueChangedEventHandler(ByVal exp As cExpression)

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Change notification event that must be fired when
        ''' the value of this expression has changed.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Event OnValueChanged As ValueChangedEventHandler

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Fire the change event for this expression.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Protected Sub FireChangeNotification()
            RaiseEvent OnValueChanged(Me)
        End Sub

    End Class

End Namespace
