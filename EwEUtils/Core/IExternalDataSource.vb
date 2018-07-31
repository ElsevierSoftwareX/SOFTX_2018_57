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
Imports EwEUtils.Core

#End Region ' Imports

Namespace Core

    ''' ===========================================================================
    ''' <summary>
    ''' Interface for implementing external data sources.
    ''' </summary>
    ''' ===========================================================================
    Public Interface IExternalDataSource

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' States whether a plug-in capable of delivering external data is available.
        ''' </summary>
        ''' <param name="runtype">The core run type to check availability for.</param>
        ''' <returns>True if available.</returns>
        ''' -----------------------------------------------------------------------
        Function IsDataAvailable(ByVal runtype As IRunType) As Boolean

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether data delivering plug-ins are supposed to be enabled or
        ''' disabled for a certain core run type.
        ''' </summary>
        ''' <param name="runtype">The core run type to check availability for.</param>
        ''' <returns>True if available.</returns>
        ''' -----------------------------------------------------------------------
        Property EnableData(ByVal runtype As IRunType) As Boolean

    End Interface

End Namespace ' Core
