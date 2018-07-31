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
''' Interface for implementing plugin points that provide different types of 
''' shape functions.
''' <seealso cref="EwEUtils.Core.IShapeFunction"/>
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcosimShapeFunctionPlugin
    Inherits IPlugin
    Inherits EwEUtils.Core.IShapeFunction

    ''' <summary>
    ''' Returns the new of the function which will be displayed in the EwE
    ''' user interface.
    ''' </summary>
    ReadOnly Property DisplayName() As String

End Interface
