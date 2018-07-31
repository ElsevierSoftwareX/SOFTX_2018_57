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
Imports System.Windows.Forms

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Plug-in interface that defines all functionality required to add a menu item
''' with a keyboard shortcut to the EwE user interface.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IMenuItemKeyboardShortcutPlugin
    Inherits IMenuItemPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Implement this to specify the shortcut key combination to invoke the 
    ''' <see cref="IMenuItemPlugin"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property ShortcutKeys() As Keys

End Interface
