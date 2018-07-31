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
''' Plugin interface that defines all functionality required to intercept the 
''' execution of a user interface command.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface ICommandHandlerPlugin
    Inherits IGUIPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Implement this point to intercept execution of commands.
    ''' </summary>
    ''' <param name="cmd">The cCommand taht is being executed.</param>
    ''' <returns>True if EwE should consider the command as handled, or
    ''' Fals if EwE needs to handle the command.</returns>
    ''' -----------------------------------------------------------------------
    ReadOnly Property HandleCommand(cmd As Object) As Boolean

End Interface
