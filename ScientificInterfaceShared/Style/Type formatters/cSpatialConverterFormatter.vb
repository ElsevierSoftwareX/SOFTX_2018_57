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
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="ISpatialDataConverter"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cSpatialConverterFormatter
        Implements ITypeFormatter

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(ISpatialDataConverter)
        End Function

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            ' ToDo: globalize this

            Try
                If (value IsNot Nothing) Then
                    Dim obj As ISpatialDataConverter = DirectCast(value, ISpatialDataConverter)
                    If (descriptor = eDescriptorTypes.Description) Then Return obj.Description

                    Dim strName As String = obj.DisplayName
                    Dim strDetails As String = ""

                    If (Not String.IsNullOrWhiteSpace(obj.AttributeFilter)) Then
                        strName = String.Format("{0} where {1}", strName, obj.AttributeFilter)
                    ElseIf (Not String.IsNullOrWhiteSpace(obj.AttributeName)) Then
                        strName = String.Format("{0} by {1}", strName, obj.AttributeName)
                    End If

                    Return strName
                End If

            Catch ex As Exception
                Debug.Assert(False)
            End Try

            Return My.Resources.GENERIC_VALUE_NONE

        End Function

    End Class

End Namespace
