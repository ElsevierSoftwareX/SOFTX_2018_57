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
''' Interface for implementing plug-ins that extend value validation events with 
''' the EwE Core. Whenever a user modifies a value, this value is passed to the 
''' core for validation against allowed value ranges, against other existing 
''' values, etc. Users can decide to extend this process by adding custom tests.
''' </summary>
''' ===========================================================================
Public Interface IDataValidatedPlugin
    Inherits ICorePlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point called when the core has succesfully validated a variable.
    ''' </summary>
    ''' <param name="varname">The eVarname flag identifying the variable that 
    ''' passed Core validation.</param>
    ''' <param name="dt">The eDataTypes flag identifying the core source of the
    ''' variable.</param>
    ''' -----------------------------------------------------------------------
    Sub DataValidated(ByVal varname As eVarNameFlags, ByVal dt As eDataTypes)

End Interface
