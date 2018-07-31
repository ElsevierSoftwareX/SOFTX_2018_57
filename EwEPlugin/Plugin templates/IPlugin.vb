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

''' ===========================================================================
''' <summary>
''' Base interface for defining an EwE6 plug-in. Plug-ins are detected by the
''' presence of this Interface.
''' </summary>
''' ===========================================================================
Public Interface IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialize the plugin.
    ''' </summary>
    ''' <param name="core">The core this plugin is initialized for.</param>
    ''' -----------------------------------------------------------------------
    Sub Initialize(ByVal core As Object)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Uniquely identifies a plugin. This field cannot be left empty!
    ''' </summary>
    ''' <remarks>
    ''' The name field will be used to determine the order of appearance of 
    ''' user interface plug-in elements; user interface elements originating
    ''' from plug-ins will be sorted by this property in ascending order.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    ReadOnly Property Name() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Uniquely describes a plugin.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property Description() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Describes the author of the plugin.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property Author() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Provides contact information about the plugin.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property Contact() As String

End Interface
