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
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="eTimeSeriesType">time series types</see>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cTimeSeriesTypeFormatter
        Implements ITypeFormatter

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            Dim ts As eTimeSeriesType = DirectCast(value, eTimeSeriesType)
            Dim strType As String = ""

            Select Case ts
                Case eTimeSeriesType.AverageWeight : strType = My.Resources.TS_TYPE_AVERAGEWEIGHT
                Case eTimeSeriesType.BiomassAbs : strType = My.Resources.TS_TYPE_BIOMASSABS
                Case eTimeSeriesType.BiomassForcing : strType = My.Resources.TS_TYPE_BIOMASSFORCING
                Case eTimeSeriesType.BiomassRel : strType = My.Resources.TS_TYPE_BIOMASSREL
                Case eTimeSeriesType.CatchesForcing : strType = My.Resources.TS_TYPE_CATCHESFORCING
                Case eTimeSeriesType.Catches : strType = My.Resources.TS_TYPE_CATCHESABS
                Case eTimeSeriesType.CatchesRel : strType = My.Resources.TS_TYPE_CATCHESREL
                Case eTimeSeriesType.ConstantTotalMortality : strType = My.Resources.TS_TYPE_CONSTTOTALMORT
                'Case eTimeSeriesType.EcotracerConcAbs : strType = My.Resources.TS_TYPE_TRACER_CONCABS
                'Case eTimeSeriesType.EcotracerConcRel : strType = My.Resources.TS_TYPE_TRACER_CONCREL
                Case eTimeSeriesType.FishingEffort : strType = My.Resources.TS_TYPE_FISHINGEFFORT
                Case eTimeSeriesType.FishingMortality : strType = My.Resources.TS_TYPE_FISHMORTABS
                Case eTimeSeriesType.TimeForcing : strType = My.Resources.TS_TYPE_TIMEFORCING
                Case eTimeSeriesType.TotalMortality : strType = My.Resources.TS_TYPE_TOTALMORT
                Case eTimeSeriesType.FishingMortalityRef : strType = My.Resources.TS_TYPE_FISHMORTREL
                Case eTimeSeriesType.DiscardMortality : strType = My.Resources.TS_TYPE_DISCARDMORT
                Case eTimeSeriesType.DiscardProportion : strType = My.Resources.TS_TYPE_DISCARDPROP
                Case eTimeSeriesType.Landings : strType = My.Resources.TS_TYPE_LANDINGS
                Case eTimeSeriesType.Discards : strType = My.Resources.TS_TYPE_DISCARDS

            End Select

            Select Case descriptor
                Case eDescriptorTypes.Name
                    Return String.Format(My.Resources.GENERIC_LABEL_DETAILED, strType, CInt(value))
            End Select
            Return strType

        End Function

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(eTimeSeriesType)
        End Function

    End Class

End Namespace
