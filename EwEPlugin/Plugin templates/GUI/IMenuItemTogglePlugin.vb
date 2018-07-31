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
''' item to the EwE main menu. The menu item can be checked or unchecked.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IMenuItemTogglePlugin
    Inherits IMenuItemPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Implement this to specify whether a menu item should be checked or 
    ''' unchecked at a given moment. This options should always have been part of 
    ''' <see cref="IMenuItemPlugin"/>.
    ''' </summary>
    ''' <remarks>
    ''' Note that the checked state may not show in the Windows UI if a plug-in 
    ''' has been given a <see cref="ControlImage"/>.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    ReadOnly Property IsChecked() As Boolean

End Interface
