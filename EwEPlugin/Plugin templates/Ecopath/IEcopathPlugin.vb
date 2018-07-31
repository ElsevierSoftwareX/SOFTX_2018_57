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

''' ---------------------------------------------------------------------------
''' <summary>
''' Interface for implementing a plugin point that is invoked whenever an EwE
''' Ecopath model has been loaded or has been saved, but before the datasource is
''' closed.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcopathPlugin
    Inherits IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when an Ecopath model has been loaded, 
    ''' exposing the data source that the Ecopath model was loaded from.
    ''' </summary>
    ''' <param name="dataSource">A reference to the EwE data source from which
    ''' data is being loaded.</param>
    ''' <remarks>This plug-in point is non-exclusive, meaning that multiple
    ''' plug-ins can respond to this event.</remarks>
    ''' <returns>True if loaded successful.</returns>
    ''' -----------------------------------------------------------------------
    Function LoadModel(ByVal dataSource As Object) As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when an Ecopath model has been saved, 
    ''' exposing the data source that the Ecopath model was loaded from.
    ''' </summary>
    ''' <param name="dataSource">A reference to the EwE data source to which
    ''' data is being saved.</param>
    ''' <remarks>This plug-in point is non-exclusive, meaning that multiple
    ''' plug-ins can respond to this event.</remarks>
    ''' -----------------------------------------------------------------------
    Function SaveModel(ByVal dataSource As Object) As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when an Ecopath model has been closed.
    ''' </summary>
    ''' <returns>True if closed successful.</returns>
    ''' -----------------------------------------------------------------------
    Function CloseModel() As Boolean

End Interface
