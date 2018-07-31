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
Imports EwECore.Ecosim
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="cEcosimResultWriter.eResultTypes"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cEcosimResultTypeFormatter
        Implements ITypeFormatter

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            Dim fmt As New cVarnameTypeFormatter()

            Select Case DirectCast(value, cEcosimResultWriter.eResultTypes)
                Case cEcosimResultWriter.eResultTypes.AvgWeightOrProdCons : Return My.Resources.HEADER_PRODCONS
                Case cEcosimResultWriter.eResultTypes.Biomass : Return My.Resources.HEADER_BIOMASS
                Case cEcosimResultWriter.eResultTypes.ConsumptionBiomass : Return My.Resources.HEADER_CONSUMPTION_OVER_BIOMASS
                Case cEcosimResultWriter.eResultTypes.FeedingTime : Return My.Resources.HEADER_FEEDINGTIME
                Case cEcosimResultWriter.eResultTypes.Mortality : Return My.Resources.HEADER_MORTALITY
                Case cEcosimResultWriter.eResultTypes.PredationMortality : Return My.Resources.HEADER_PREDMORT
                Case cEcosimResultWriter.eResultTypes.Prey : Return My.Resources.HEADER_PREY_PERCENTAGE
                Case cEcosimResultWriter.eResultTypes.Value : Return My.Resources.HEADER_VALUE
                Case cEcosimResultWriter.eResultTypes.Catch : Return My.Resources.HEADER_CATCH
                Case cEcosimResultWriter.eResultTypes.TL : Return fmt.GetDescriptor(eVarNameFlags.TTLX, eDescriptorTypes.Name)
                Case cEcosimResultWriter.eResultTypes.FIB : Return My.Resources.HEADER_FIB
                Case cEcosimResultWriter.eResultTypes.KemptonsQ : Return My.Resources.HEADER_KEMPTONSQ
                Case cEcosimResultWriter.eResultTypes.ShannonDiversity : Return My.Resources.HEADER_SHANNONDIVERSITY
                Case cEcosimResultWriter.eResultTypes.TLC : Return My.Resources.HEADER_TLC
                Case cEcosimResultWriter.eResultTypes.TotalCatch : Return My.Resources.HEADER_TOTALCATCH
                Case cEcosimResultWriter.eResultTypes.CatchFleetGroup : Return My.Resources.HEADER_CATCH_BREAKDOWN
                Case cEcosimResultWriter.eResultTypes.MortFleetGroup : Return My.Resources.HEADER_FMORT_BREAKDOWN
                Case cEcosimResultWriter.eResultTypes.ValueFleetGroup : Return My.Resources.HEADER_VALUE_BREAKDOWN
                Case cEcosimResultWriter.eResultTypes.DiscardMortalityFleetGroup : Return My.Resources.HEADER_DISCARD_MORTALITY_BREAKDOWN
                Case cEcosimResultWriter.eResultTypes.DiscardSurvivalFleetGroup : Return My.Resources.HEADER_DISCARD_SURVIVAL_BREAKDOWN
                Case cEcosimResultWriter.eResultTypes.Landings : Return "Landings (group x fleet)"
                Case cEcosimResultWriter.eResultTypes.DiscardFleetGroup : Return "Discards (group x fleet)"
                Case Else
                    Debug.Assert(False, "Result type not supported")
                    Return DirectCast(value, cEcosimResultWriter.eResultTypes).ToString
            End Select

            Return ""

        End Function

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(cEcosimResultWriter.eResultTypes)
        End Function

    End Class

End Namespace ' Style
