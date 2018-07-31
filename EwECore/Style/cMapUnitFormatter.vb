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
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="eUnitAreaType"/> objects.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cMapUnitFormatter
        Implements ITypeFormatter

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new type <see cref="cMapUnitFormatter"/>formatter.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New()
        End Sub

        Public Function GetDescribedType() As System.Type Implements ITypeFormatter.GetDescribedType
            Return GetType(eUnitMapRefType)
        End Function

        Public Function GetDescriptor(ByVal value As Object,
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                  Implements ITypeFormatter.GetDescriptor

            Dim unit As eUnitMapRefType = DirectCast(value, eUnitMapRefType)

            Select Case unit
                Case eUnitMapRefType.m
                    Return My.Resources.CoreDefaults.UNIT_METER
                Case eUnitMapRefType.km
                    Return My.Resources.CoreDefaults.UNIT_KILOMETER
                Case eUnitMapRefType.dd
                    Return My.Resources.CoreDefaults.UNIT_DECIMALDEGREE
            End Select

            Return String.Empty
        End Function

    End Class
End Namespace