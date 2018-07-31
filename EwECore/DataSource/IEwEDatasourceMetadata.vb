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

Imports EwEUtils.Core

Namespace DataSources

    ''' =======================================================================
    ''' <summary>
    ''' Base interface for implementing metadata functionality on to a datasource.
    ''' </summary>
    ''' =======================================================================
    Public Interface IEwEDatasourceMetadata
        Inherits IEwEDataSource

        ''' <summary>
        ''' Returns a name for a given data type and DBID.
        ''' </summary>
        ''' <param name="dt"><see cref="eDataTypes"/> to obtain a description for.</param>
        ''' <param name="iDBID">Unique ID of this datatype to obtain a description for.</param>
        ''' <returns>A textual description, or an empty string if the request could not be honoured.</returns>
        Function GetDescription(ByVal dt As eDataTypes, ByVal iDBID As Integer) As String

    End Interface

End Namespace
