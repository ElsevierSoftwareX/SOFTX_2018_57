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
Imports EwEPlugin
Imports EwEUtils.Core
Imports System.Reflection
#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Factory class for creating an <see cref="IEcospaceResultsWriter"/>
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cEcospaceResultWriterFactory

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get all the Ecospace result writers provided by the EwE core and plug-ins
    ''' </summary>
    ''' <param name="pm">The plug-in manager instance to consult, if any.</param>
    ''' <returns>An array of all avaliable result writers.</returns>
    ''' -----------------------------------------------------------------------
    Friend Shared Function GetWriters(ByVal pm As cPluginManager) As IEcospaceResultsWriter()

        Dim writers As New List(Of IEcospaceResultsWriter)

        Try

            For Each t As Type In Assembly.GetAssembly(GetType(cCore)).GetTypes()

                If (GetType(IEcospaceResultsWriter).IsAssignableFrom(t) And Not t.IsAbstract()) Then
                    Try
                        writers.Add(CType(Activator.CreateInstance(t), IEcospaceResultsWriter))
                    Catch ex As Exception
                        cLog.Write(ex, "cEcospaceResultWriterFactory.GetWriters() Failed to create instance of IEcospaceResultsWriter")
                    End Try
                End If
            Next

            ' Plug-in manager provided?
            If (pm IsNot Nothing) Then
                Try
                    ' #Yes: see if a plug-in based writer supports the requested format
                    For Each ip As IEcospaceResultWriterPlugin In pm.GetPlugins(GetType(IEcospaceResultWriterPlugin))
                        writers.Add(ip)
                    Next
                Catch ex As Exception
                    cLog.Write(ex, "cEcospaceResultWriterFactory.GetWriters() Failed to create instance of IEcospaceResultWriterPlugin")
                End Try
            End If

        Catch ex As Exception
            cLog.Write(ex, "cEcospaceResultWriterFactory.GetWriters()")
        End Try

        Return writers.ToArray()

    End Function


End Class
