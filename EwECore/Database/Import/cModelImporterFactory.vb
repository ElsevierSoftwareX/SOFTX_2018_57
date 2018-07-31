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
Imports EwECore.Database
Imports EwECore.DataSources
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.Database

#End Region ' Imports 

''' ===========================================================================
''' <summary>
''' Factory class; builds a <see cref="cEwE5ModelImporter">EwE5 model importer</see>.
''' </summary>
''' ===========================================================================
Public Class cModelImporterFactory

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Factory method; builds a <see cref="cEwE5ModelImporter">EwE5 model importer</see>
    ''' from a path to an EwE5 source document. 
    ''' </summary>
    ''' <param name="core">The core to associate the importer with.</param>
    ''' <param name="strSource">Path to data source to build the importer for.</param>
    ''' <returns>A <see cref="cEwE5ModelImporter">EwE5 model importer</see>, if
    ''' all went well, or Nothing otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function GetModelImporter(ByVal core As cCore, _
                                            ByVal strSource As String, _
                                            ByVal pm As cPluginManager) As IModelImporter

        If (strSource.ToLower().StartsWith("ewe-ecobase:")) Then
            Return New cEcobaseImporter(core)
        End If

        Select Case cDataSourceFactory.GetSupportedType(strSource)

            Case eDataSourceTypes.Access2007, eDataSourceTypes.Access2003
                Return New cEwE5DatabaseImporter(core)

            Case eDataSourceTypes.EII
                Return New cEwE5EIIImporter(core)

        End Select

        ' Explore if a plug-in is provided that can do this too
        If (pm IsNot Nothing) Then
            For Each pi As IPlugin In pm.GetPlugins(GetType(EwEPlugin.Data.IModelImportPlugin))
                Dim imp As EwEPlugin.Data.IModelImportPlugin = DirectCast(pi, EwEPlugin.Data.IModelImportPlugin)
                If imp.CanImportFrom(strSource) Then
                    Return imp
                End If
            Next
        End If

        Return Nothing

    End Function

End Class
