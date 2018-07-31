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

Option Strict Off
Imports System.Reflection
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    Public Class cTypeFormatterFactory

        ''' <summary>
        ''' Factory method, explores the EwEUtils assembly for ITypeFormatter-derived
        ''' classes and returns a type formatter instance if this type implements
        ''' formatting of an indicated type.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <returns></returns>
        Public Shared Function GetTypeFormatter(ByVal t As Type) As ITypeFormatter

            ' For Each ass As Assembly In AppDomain.CurrentDomain.GetAssemblies()
            Dim ass As Assembly = Assembly.GetAssembly(GetType(ITypeFormatter))
            Dim tfm As Type = GetType(ITypeFormatter)

            For Each tTest As Type In ass.GetTypes
                If tfm.IsAssignableFrom(tTest) And tTest.GetConstructor(Type.EmptyTypes) IsNot Nothing Then
                    Dim f As ITypeFormatter = DirectCast(Activator.CreateInstance(tTest), ITypeFormatter)
                    If f.GetDescribedType.IsAssignableFrom(t) Then
                        Return f
                    End If
                End If
            Next
            'Next
            Return Nothing

        End Function

    End Class

End Namespace
