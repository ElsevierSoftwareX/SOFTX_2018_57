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
Imports System.ComponentModel
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a localized description to a method in a class. This localized
    ''' discription will show up in smart controls such as <see cref="PropertyGrid"/>.
    ''' </summary>
    ''' <example>
    ''' <para>This example shows you how to use a localized <see cref="cLocalizedDisplayNameAttribute"/>, where
    ''' "res_myprop_name1" and "res_myprop_name2" are string resources defined in the resources.</para>
    ''' <para>Note that the class can be redirected to the resources of another assembly.</para>
    ''' <code>
    ''' Class TestClass
    '''     
    '''     &gt;cLocalizedDisplayNameAttribute("res_myprop1_name")&lt; _
    '''     Public Property MyProp1 As String
    '''
    '''     &gt;cLocalizedDisplayNameAttribute("res_myprop2_name", GetType(ScientificInterfaceShared.My.Resources))&lt; _
    '''     Public Property MyProp2 As String
    '''
    ''' 
    ''' End Class
    ''' </code>
    ''' </example>
    ''' -----------------------------------------------------------------------
    Public Class cLocalizedDisplayNameAttribute
        Inherits DisplayNameAttribute

#Region " Private vars "

        ''' <summary>Name of the resource string to find.</summary>
        Private m_strResName As String = ""
        ''' <summary>Default string to return if no suitable resource string could be found.</summary>
        Private m_strDefault As String = ""
        ''' <summary>Assembly that contains the resource string.</summary>
        Private m_typeAssem As Type = Nothing

#End Region ' Private vars

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new instance of this class.
        ''' </summary>
        ''' <param name="strResName">Name of the resource string to find.</param>
        ''' <param name="typeAssem">Assembly that contains the resource string.</param>
        ''' <param name="strDefault">Default string to return if no suitable resource string could be found.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal strResName As String, _
                       ByVal typeAssem As Type, _
                       Optional strDefault As String = "")
            MyBase.New()

            Me.m_strResName = strResName
            Me.m_typeAssem = typeAssem
            Me.m_strDefault = strDefault

            If String.IsNullOrWhiteSpace(Me.m_strDefault) Then
                Me.m_strDefault = strResName
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new instance of this class.
        ''' </summary>
        ''' <param name="strResName">Name of the resource string to find.</param>
        ''' <param name="strDefault">Default string to return if no suitable resource string could be found.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(strResName As String, _
                       Optional strDefault As String = "")
            Me.New(strResName, GetType(cLocalizedDescriptionAttribute), strDefault)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the localized display name, or the default string if
        ''' a localized string could not be found.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides ReadOnly Property DisplayName As String
            Get
                Dim strName As String = cResourceUtils.LoadString(Me.m_strResName, Me.m_typeAssem)
                If Not String.IsNullOrWhiteSpace(strName) Then Return strName
                Return Me.m_strDefault
            End Get
        End Property

    End Class

End Namespace
