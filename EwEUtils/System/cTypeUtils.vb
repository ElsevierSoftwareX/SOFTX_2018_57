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

Imports System
Imports System.Reflection

Namespace Utilities

    Public Class cTypeUtils

        Public Shared Function TypeToString(ByVal t As Type) As String

            ' Include assembly short name in the type name. This enables
            ' the OOP database logic to relocate the type from its original
            ' assembly, even if similar class names exist in similar namespaces
            ' in different asssemblies. Yes, it's far fetched, but hey...
            Return t.Assembly.GetName.Name + "!" + t.FullName()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, locates the originating type from a type string.
        ''' </summary>
        ''' <param name="strType">The type name to locate the originating type
        ''' for.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The counterpart of this method, <see cref="TypeToString"/>,
        ''' can be used to create the string for a type.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function StringToType(ByVal strType As String) As Type

            ' Split assembly short name from type name
            Dim astr As String() = strType.Split(CChar("!"))
            Dim ass As Assembly = Nothing

            For Each ass In AppDomain.CurrentDomain.GetAssemblies
                If String.Compare(ass.GetName.Name, astr(0), True) = 0 Then
                    Try
                        Return ass.GetType(astr(1))
                    Catch ex As Exception

                    End Try
                End If
            Next
            Return Nothing

        End Function
    End Class

End Namespace
