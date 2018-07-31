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
Imports System.Globalization
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="RegionInfo"/> 
    ''' currency information.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cMonetaryTypeFormatter
        Implements ITypeFormatter

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(RegionInfo)
        End Function

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            Dim ri As RegionInfo = DirectCast(value, RegionInfo)
            Dim strDescr As String = ""

            Select Case descriptor
                Case eDescriptorTypes.Symbol
                    strDescr = ri.CurrencySymbol

                Case eDescriptorTypes.Abbreviation
                    strDescr = ri.ISOCurrencySymbol

                Case eDescriptorTypes.Name
                    strDescr = ri.CurrencyEnglishName()

                Case eDescriptorTypes.Description
                    strDescr = ri.CurrencyNativeName

            End Select
            Return strDescr

        End Function

    End Class

End Namespace
