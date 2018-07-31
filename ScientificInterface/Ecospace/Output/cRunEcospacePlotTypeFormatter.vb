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
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Ecospace

    Public Class cRunEcospacePlotTypeFormatter
        Implements ITypeFormatter

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(frmRunEcospace.ePlotTypes)
        End Function

        Public Function GetDescriptor(value As Object, Optional descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
            Implements ITypeFormatter.GetDescriptor

            ' ToDo: globalize this properly
            Select Case DirectCast(value, frmRunEcospace.ePlotTypes)
                Case frmRunEcospace.ePlotTypes.RelB
                    Return SharedResources.HEADER_RELATIVEBIOMASS
                Case frmRunEcospace.ePlotTypes.F
                    Return SharedResources.HEADER_FISHMORT_OVER_TOTMORT
                Case frmRunEcospace.ePlotTypes.FOverB
                    Return "F over B"
                Case frmRunEcospace.ePlotTypes.Effort
                    Return SharedResources.HEADER_EFFORT
                Case frmRunEcospace.ePlotTypes.CoverB
                    Return "C over B"
                Case frmRunEcospace.ePlotTypes.Contaminant
                    Return "Rel. contaiminants"
            End Select
            Return ""

        End Function

    End Class

End Namespace
