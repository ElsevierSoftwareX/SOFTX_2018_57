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
Option Explicit On

Imports EwECore
Imports EwECore.Style
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecopath.Output

    ''' =======================================================================
    ''' <summary>
    ''' Grid clas, showing Ecopath statistics values.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)>
    Public Class gridEcopathStatistics
        Inherits EwEGrid

        Private Enum eColumnTypes As Byte
            Header = 0
            Value
            Units
        End Enum

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()
            Me.Redim(1, 3)
            Me(0, 0) = New EwEColumnHeaderCell(SharedResources.HEADER_PARAMETER)
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_VALUE)
            Me(0, 2) = New EwEColumnHeaderCell(SharedResources.HEADER_UNITS)

            Me.FixedColumns = 1
            Me.FixedColumnWidths = False

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cEcoPathStats = Core.EcopathStats

            Me.AddRow(SharedResources.HEADER_SUM_CONSUMPTION, source, eVarNameFlags.EcopathStatsTotalConsumption)
            Me.AddRow(SharedResources.HEADER_SUM_EXPORTS, source, eVarNameFlags.EcopathStatsTotalExports)
            Me.AddRow(SharedResources.HEADER_SUM_FLOW_RESP, source, eVarNameFlags.EcopathStatsTotalRespFlow)
            Me.AddRow(SharedResources.HEADER_SUM_FLOW_DET, source, eVarNameFlags.EcopathStatsTotalFlowDetritus)
            Me.AddRow(SharedResources.HEADER_SUM_THROUGHPUT, source, eVarNameFlags.EcopathStatsTotalThroughput)
            Me.AddRow(SharedResources.HEADER_SUM_PROD, source, eVarNameFlags.EcopathStatsTotalProduction)
            Me.AddRow(SharedResources.HEADER_MEAN_CATCH_TL, source, eVarNameFlags.EcopathStatsMeanTrophicLevelCatch)
            Me.AddRow(SharedResources.HEADER_GROSS_EFFICIENCY, source, eVarNameFlags.EcopathStatsGrossEfficiency)
            Me.AddRow(SharedResources.HEADER_SUM_NET_PP, source, eVarNameFlags.EcopathStatsTotalNetPP)
            Me.AddRow(SharedResources.HEADER_SUM_PP_RESP, source, eVarNameFlags.EcopathStatsTotalPResp)
            Me.AddRow(SharedResources.HEADER_NET_PROD, source, eVarNameFlags.EcopathStatsNetSystemProduction)
            Me.AddRow(SharedResources.HEADER_SUM_PPB, source, eVarNameFlags.EcopathStatsTotalPB)
            Me.AddRow(SharedResources.HEADER_SUM_BT, source, eVarNameFlags.EcopathStatsTotalBT)
            Me.AddRow(SharedResources.HEADER_SUM_BnonDET, source, eVarNameFlags.EcopathStatsTotalBNonDet)
            Me.AddRow(SharedResources.HEADER_SUM_CATCH, source, eVarNameFlags.EcopathStatsTotalCatch)
            Me.AddRow(SharedResources.HEADER_INDEX_CONNECTANCE, source, eVarNameFlags.EcopathStatsConnectanceIndex)
            Me.AddRow(SharedResources.HEADER_INDEX_ONMIVORY, source, eVarNameFlags.EcopathStatsOmnivIndex)
            Me.AddRow(SharedResources.HEADER_SUM_VALUE_MARKET, source, eVarNameFlags.EcopathStatsTotalMarketValue)
            Me.AddRow(SharedResources.HEADER_SUM_VALUE_SHADOW, source, eVarNameFlags.EcopathStatsTotalShadowValue)
            Me.AddRow(SharedResources.HEADER_SUM_VALUE, source, eVarNameFlags.EcopathStatsTotalValue)
            Me.AddRow(SharedResources.HEADER_SUM_COST_FIXED, source, eVarNameFlags.EcopathStatsTotalFixedCost)
            Me.AddRow(SharedResources.HEADER_SUM_COST_VARIABLE, source, eVarNameFlags.EcopathStatsTotalVarCost)
            Me.AddRow(SharedResources.HEADER_SUM_COST, source, eVarNameFlags.EcopathStatsTotalCost)
            Me.AddRow(SharedResources.HEADER_SUM_PROFIT, source, eVarNameFlags.EcopathStatsProfit)
            Me.AddRow(SharedResources.HEADER_ECOPATH_PEDIGREE_INDEX, source, eVarNameFlags.EcopathStatsPedigreeIndex)
            Me.AddRow(SharedResources.HEADER_ECOPATH_PEDIGREE_CV, source, eVarNameFlags.EcopathStatsPedigreeCV)
            Me.AddRow(SharedResources.HEADER_MEASUREOFFIT, source, eVarNameFlags.EcopathStatsMeasureOfFit)

            Dim model As cEwEModel = Me.Core.EwEModel
            Dim fmt As New cDiversityIndexTypeFormatter()
            Me.AddRow(fmt.GetDescriptor(model.DiversityIndexType), source, eVarNameFlags.EcopathStatsDiversity)

        End Sub

        Protected Overrides Sub FinishStyle()
            Me.Columns(eColumnTypes.Header).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            Me.Columns(eColumnTypes.Units).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            Me.Columns(eColumnTypes.Value).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            MyBase.FinishStyle()
        End Sub

        Private Overloads Sub AddRow(ByVal strHeader As String, ByVal source As cEcoPathStats, ByVal vnf As eVarNameFlags)

            Dim iRow As Integer = Me.AddRow()
            Dim md As cVariableMetaData = source.GetVariableMetadata(vnf)

            Me(iRow, eColumnTypes.Header) = New EwERowHeaderCell(strHeader)
            Me(iRow, eColumnTypes.Value) = New PropertyCell(Me.PropertyManager, source, vnf)
            Me(iRow, eColumnTypes.Units) = New EwEUnitCell(md.Units)

        End Sub

    End Class

End Namespace
