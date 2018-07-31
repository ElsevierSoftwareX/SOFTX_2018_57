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

Imports System

''' ---------------------------------------------------------------------------
''' <summary>
''' Plugin exception
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cPluginException
    Inherits Exception

    Private m_assembly As cPluginAssembly = Nothing

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new instance of a cPluginException.
    ''' </summary>
    ''' <param name="strMessage"></param>
    ''' <param name="exception"></param>
    ''' <param name="assembly"></param>
    ''' ---------------------------------------------------------------------------
    Public Sub New(ByVal assembly As cPluginAssembly, ByVal strMessage As String, ByVal exception As Exception)
        MyBase.New(strMessage, exception)
        Me.m_assembly = assembly
    End Sub

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new instance of a cPluginException.
    ''' </summary>
    ''' <param name="strMessage"></param>
    ''' <param name="assembly"></param>
    ''' ---------------------------------------------------------------------------
    Public Sub New(ByVal assembly As cPluginAssembly, ByVal strMessage As String)
        Me.New(assembly, strMessage, Nothing)
    End Sub

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Constructor, initializes a new instance of a cPluginException.
    ''' </summary>
    ''' <param name="exception"></param>
    ''' <param name="assembly"></param>
    ''' ---------------------------------------------------------------------------
    Public Sub New(ByVal assembly As cPluginAssembly, ByVal exception As Exception)
        Me.New(assembly, exception.Message)
    End Sub

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Get the assembly that caused the exception.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public ReadOnly Property Assembly() As cPluginAssembly
        Get
            Return Me.m_assembly
        End Get
    End Property

End Class
