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
    Public Class cAreaUnitFormatter
        Implements ITypeFormatter

        Private m_strCustom As String = ""

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new type <see cref="eUnitCurrencyType"/>formatter.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New(strCustom As String)
            Me.m_strCustom = strCustom
        End Sub

        Public Function GetDescribedType() As System.Type Implements ITypeFormatter.GetDescribedType
            Return GetType(eUnitAreaType)
        End Function

        Public Function GetDescriptor(ByVal value As Object,
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                  Implements ITypeFormatter.GetDescriptor

            Dim unit As eUnitAreaType = DirectCast(value, eUnitAreaType)

            Select Case unit
                Case eUnitAreaType.Km2
                    Return My.Resources.CoreDefaults.UNIT_AREA_KM2
                Case eUnitAreaType.Mi2
                    Return My.Resources.CoreDefaults.UNIT_AREA_MI2
                Case eUnitAreaType.Custom
                    Return Me.m_strCustom
            End Select

            Return String.Empty
        End Function

    End Class

End Namespace