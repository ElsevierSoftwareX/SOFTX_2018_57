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

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Plugin interface that defines all functionality required to add a menu
''' item to the EwE main menu.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IMenuItemPlugin
    Inherits IGUIPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <para>
    ''' Implement this point to specify the menu item location for this plugin.
    ''' </para>
    ''' <para>A location is a '\' separated series of menu item names, starting 
    ''' at the root node of the menu that the plug-in is nested into.</para>
    ''' <para>Use of the '|' character to separate menu item names is deprecated.</para>
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property MenuItemLocation() As String

End Interface
