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
Imports EwECore.DataSources

#End Region ' Imports

Namespace Database

    ''' =======================================================================
    ''' <summary>
    ''' Helper class, returns a database compact engine for compacting a 
    ''' database for an underlying database engine.
    ''' </summary>
    ''' =======================================================================
    Public Class cDatabaseCompactFactory

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get <see cref="IDatabaseCompact">database compact engine</see> for
        ''' a given database.
        ''' </summary>
        ''' <param name="strFileName">The complete path to the database to compact.</param>
        ''' <returns>An instance of a <see cref="IDatabaseCompact">database compact engine</see>,
        ''' or Null / Nothing if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Shared Function GetDatabaseCompact(ByVal strFileName As String) As IDatabaseCompact

            Select Case cDataSourceFactory.GetSupportedType(strFileName)
                Case eDataSourceTypes.Access2003
                    ' MDB databases compacted via JRO
                    Return New cCompactJRO()
                Case eDataSourceTypes.Access2007
                    ' ACCDB databases compacted via DAO
                    Return New cCompactDAO()
                Case Else
                    ' Not supported
            End Select

            Return Nothing

        End Function

    End Class

End Namespace ' Database
