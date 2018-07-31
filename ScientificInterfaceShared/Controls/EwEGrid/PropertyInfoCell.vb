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
Imports System.Reflection
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style
Imports SourceGrid2

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' ===========================================================================
    ''' <summary>
    ''' Cell that manages a single value in an object via a PropertyInfo instance.
    ''' </summary>
    ''' ===========================================================================
    <CLSCompliant(False)> _
    Public Class cPropertyInfoCell
        : Inherits EwECell

#Region " Privates "

        ''' <summary>Object instance to manage the property value for.</summary>
        Private m_obj As Object = Nothing
        ''' <summary>PropertyInfo instance to manage the value for.</summary>
        Private m_pi As PropertyInfo = Nothing

#End Region ' Privates

#Region " Constructor "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="obj">The object instance to manage the property value for.</param>
        ''' <param name="pi">The PropertyInfo instance to manage the value for.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal obj As Object, ByVal pi As PropertyInfo)

            ' Set the cell value to the intial property value and type
            MyBase.New(pi.GetValue(obj, Nothing),
                       pi.PropertyType,
                       If(pi.CanWrite, cStyleGuide.eStyleFlags.OK, cStyleGuide.eStyleFlags.NotEditable))

            ' Sanity checks
            Debug.Assert(obj IsNot Nothing)
            Debug.Assert(pi IsNot Nothing)

            ' Store refs
            Me.m_obj = obj
            Me.m_pi = pi

            Me.SuppressZero = True

            ' ToDo: respond to property changes by refreshing the cell value

        End Sub

#End Region ' Constructor

#Region " Overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Set the value in the underlying cell and PropertyInfo.
        ''' </summary>
        ''' <param name="pos"></param>
        ''' <param name="objValue"></param>
        ''' -----------------------------------------------------------------------
        Public Overrides Sub SetValue(ByVal pos As SourceGrid2.Position, ByVal objValue As Object)

            ' Update the cell value
            MyBase.SetValue(pos, objValue)

            ' Has attached object and property?
            If (Me.m_obj IsNot Nothing) And (Me.m_pi IsNot Nothing) Then

                ' #Yes: use the (hopefully adjusted) cell value to update the 
                '       property in the underlying object.

                Try
                    If (Me.m_pi.PropertyType Is GetType(Single)) Then
                        If (Me.Value Is Nothing) Then Me.Value = 0
                        Me.m_pi.SetValue(Me.m_obj, CSng(Val(Me.Value)), Nothing)
                    ElseIf (Me.m_pi.PropertyType Is GetType(String)) Then
                        If (Me.Value Is Nothing) Then Me.Value = ""
                        Me.m_pi.SetValue(Me.m_obj, CStr(Me.Value), Nothing)
                    ElseIf (Me.m_pi.PropertyType Is GetType(Integer)) Then
                        If (Me.Value Is Nothing) Then Me.Value = 0
                        Me.m_pi.SetValue(Me.m_obj, CInt(Val(Me.Value)), Nothing)
                    ElseIf (Me.m_pi.PropertyType Is GetType(Double)) Then
                        If (Me.Value Is Nothing) Then Me.Value = 0
                        Me.m_pi.SetValue(Me.m_obj, CDbl(Val(Me.Value)), Nothing)
                    ElseIf (Me.m_pi.PropertyType Is GetType(Boolean)) Then
                        If (Me.Value Is Nothing) Then Me.Value = False
                        Me.m_pi.SetValue(Me.m_obj, Convert.ToBoolean(Me.Value), Nothing)
                    Else
                        Debug.Assert(False, String.Format("Value type '{0}' not supported yet in PICell", Me.m_pi.PropertyType))
                    End If
                Catch ex As Exception
                    ' Kaboom
                    Debug.Assert(False, ex.Message)
                End Try

            End If
        End Sub

#End Region ' Overrides

    End Class

End Namespace
