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
Imports EwEUtils.Core

''' ===========================================================================
''' <summary>
''' Interface for implementing plug-ins that may prevent the core from saving or
''' discarding data.
''' </summary>
''' ===========================================================================
Public Interface ISaveFilterPlugin
    Inherits ICorePlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point called when the core is about to save changes.
    ''' </summary>
    ''' <param name="bCancel">Setting this to False will abort the save attempt.</param>
    ''' -----------------------------------------------------------------------
    Function SaveChanges(ByRef bCancel As Boolean) As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point called when the core is about to save changes.
    ''' </summary>
    ''' <param name="bCancel">Setting this to False will abort the save attempt.</param>
    ''' -----------------------------------------------------------------------
    Function DiscardChanges(ByRef bCancel As Boolean) As Boolean

End Interface
