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
Imports EwECore
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="eTaxonClassificationType"/>s.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cTaxonClassificationTypeFormatter
        Implements ITypeFormatter

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(eTaxonClassificationType)
        End Function

        Public Function GetDescriptor(ByVal value As Object, Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
            Implements ITypeFormatter.GetDescriptor

            Dim val As eTaxonClassificationType = DirectCast(value, eTaxonClassificationType)
            Dim fmt As New cVarnameTypeFormatter()

            Select Case val
                Case eTaxonClassificationType.Phylum
                    Return fmt.GetDescriptor(eVarNameFlags.Phylum, eDescriptorTypes.Name)
                Case eTaxonClassificationType.Order
                    Return fmt.GetDescriptor(eVarNameFlags.Order, eDescriptorTypes.Name)
                Case eTaxonClassificationType.Class
                    Return fmt.GetDescriptor(eVarNameFlags.Class, eDescriptorTypes.Name)
                Case eTaxonClassificationType.Family
                    Return fmt.GetDescriptor(eVarNameFlags.Family, eDescriptorTypes.Name)
                Case eTaxonClassificationType.Genus
                    Return fmt.GetDescriptor(eVarNameFlags.Genus, eDescriptorTypes.Name)
                Case eTaxonClassificationType.Species
                    Return fmt.GetDescriptor(eVarNameFlags.Species, eDescriptorTypes.Name)
                Case eTaxonClassificationType.Latin
                    Return fmt.GetDescriptor(eVarNameFlags.Name, eDescriptorTypes.Name)
                Case Else
                    Debug.Assert(False)
            End Select

            Return ""
        End Function

    End Class

End Namespace
