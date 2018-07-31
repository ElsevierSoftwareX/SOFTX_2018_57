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

Namespace Database

    ''' =======================================================================
    ''' <summary>
    ''' Interface for implementing database compact capabilities.
    ''' </summary>
    ''' =======================================================================
    Public Interface IDatabaseCompact

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the OS supports compacting of databases of the type
        ''' that this engine was created for.
        ''' </summary>
        ''' <returns>True if the OS supports compacting of a database.</returns>
        ''' -------------------------------------------------------------------
        Function CanCompact() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Compact a database.
        ''' </summary>
        ''' <param name="strFileFrom">Source database location.</param>
        ''' <param name="strConnectionFrom">Source database connection string.</param>
        ''' <param name="strFileTo">Target database location.</param>
        ''' <param name="strConnectionTo">Target database connection string.</param>
        ''' <returns>A <see cref="eDatasourceAccessType">database access
        ''' result code</see>.</returns>
        ''' -------------------------------------------------------------------
        Function Compact(ByVal strFileFrom As String, _
                         ByVal strConnectionFrom As String, _
                         ByVal strFileTo As String, _
                         ByVal strConnectionTo As String) As eDatasourceAccessType

    End Interface

End Namespace ' Database
