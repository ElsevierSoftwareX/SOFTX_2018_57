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
Imports System
Imports Microsoft.Win32

#End Region ' Imports

Namespace Utilities

    ''' =======================================================================
    ''' <summary>
    ''' Helper class providing standardized access to the Windows registry.
    ''' </summary>
    ''' =======================================================================
    Public Class cRegistryUtils

        Public Shared Function ReadKey(ByVal keyParent As RegistryKey, ByVal strSubKey As String, ByVal strValueName As String) As String

            Dim Key As RegistryKey = Nothing
            Dim strValue As String = ""

            Try
                'Open the registry key.
                Key = keyParent.OpenSubKey(strSubKey, True)
                If Key Is Nothing Then 'if the key doesn't exist
                    strValue = ""
                End If

                'Get the value.
                strValue = Convert.ToString(Key.GetValue(strValueName))

            Catch e As Exception
            End Try

            Return strValue

        End Function

    End Class

End Namespace ' Utilities
