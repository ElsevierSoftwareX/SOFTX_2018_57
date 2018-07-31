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
Imports System.IO
Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Style

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class to write network analysis results to a CSV file.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cNetworkAnalysisEcosimResultWriter
    Inherits cNetworkAnalysisResultWriter

#Region " Private vars "

    Private Enum eColTypes As Integer
        YEAR = 0
        THROUGHPUT
        CAPACITY_ECOSIM
        ASCEND_IMPORT
        ASCEND_FLOW
        ASCEND_EXPORT
        ASCEND_RESP
        OVERHEAD_IMPORT
        OVERHEAD_FLOW
        OVERHEAD_EXPORT
        OVERHEAD_RESP
        PCI
        FCI
        PATH_LEN
        EXPORT
        RESP_ECOSIM
        PRIM_PROD
        PROD
        BIOMASS
        [CATCH]
        PROP_FLOW_DET
        ASCEND_TOTAL
        AMI
        ENTROPY
        TLc
        Diversity
        FIB
        DET_TE_W
        PP_TE_W
        TOT_TE_W
        PPR
        LINDEX
        PSUST
    End Enum

#End Region ' Private vars

#Region " Internal helper classes "

    Private Class cColTypeFormatter
        Implements ITypeFormatter

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(eColTypes)
        End Function

        Public Function GetDescriptor(value As Object, Optional descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
            Implements ITypeFormatter.GetDescriptor

            Dim strVar As String = value.ToString()
            Dim strCol As String = cResourceUtils.LoadString("COL_HDR_" & strVar.ToUpper, Me.GetType.Assembly)

            If String.IsNullOrWhiteSpace(strCol) Then Return strVar
            Return strCol

        End Function

    End Class

#End Region ' Internal helper classes

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Shazaam
    ''' </summary>
    ''' <param name="manager"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal manager As cNetworkManager)
        MyBase.New(manager)
    End Sub

    Public Overrides Function WriteResults(ByVal strPath As String) As Boolean

        If Not Me.Manager.IsEcosimNetworkRun Then
            Return False
        End If

        If Not cFileUtils.IsDirectoryAvailable(strPath, True) Then Return False

        If Me.Manager.EcosimPPROn Then
            Return Me.WriteIndicesWithPPR(strPath)
        Else
            Return Me.WriteIndicesWithoutPPR(strPath)
        End If

    End Function

#Region " Internals "

    Private Function WriteIndicesWithoutPPR(ByVal strPath As String) As Boolean

        Return Me.WriteFile(Me.GetNAIndicatorsFileName(strPath, False, True), Me.GetIndicesWithoutPPRData(True)) And
               Me.WriteFile(Me.GetNAIndicatorsFileName(strPath, False, False), Me.GetIndicesWithoutPPRData(False))

    End Function

    Private Function WriteIndicesWithPPR(ByVal strPath As String) As Boolean

        Return Me.WriteFile(Me.GetNAIndicatorsFileName(strPath, True, True), Me.GetIndicesWithPPRData(True)) And
               Me.WriteFile(Me.GetNAIndicatorsFileName(strPath, True, False), Me.GetIndicesWithPPRData(False))

    End Function

    Private Function GetNAIndicatorsFileName(ByVal strPath As String, ByVal bWithPPR As Boolean, ByVal bAnnual As Boolean) As String
        Dim strFile As String = "NA_" &
                                If(bAnnual, My.Resources.HEADER_ANNUAL, My.Resources.HEADER_MONTHLY) & "_" &
                                If(bWithPPR, "IndicesPPR", "IndicesWithoutPPR") &
                                ".csv"
        Return Path.Combine(strPath, strFile)
    End Function

    Private Function GetIndicesWithoutPPRData(ByVal bAnnualAverage As Boolean) As String

        Dim cols As eColTypes() = DirectCast([Enum].GetValues(GetType(eColTypes)), eColTypes())
        Dim iNumCols As Integer = cols.Length - 3 ' Exclude PPR columns
        Dim asValues(iNumCols) As Single
        Dim iMonth As Integer = 0
        Dim iYear As Integer = 0
        Dim bLineAdded As Boolean = False
        Dim sb As New StringBuilder()
        Dim fmt As New cColTypeFormatter()

        ' Header line
        For j As Integer = 0 To iNumCols - 1
            If (j > 0) Then sb.Append(",")

            Dim strField As String = ""
            Select Case cols(j)
                Case eColTypes.Diversity
                    Dim model As cEwEModel = Me.Manager.Core.EwEModel
                    Dim f As New cDiversityIndexTypeFormatter()
                    strField = f.GetDescriptor(model.DiversityIndexType)
                Case Else
                    strField = fmt.GetDescriptor(cols(j))
            End Select
            sb.Append(cStringUtils.ToCSVField(strField))
        Next
        sb.AppendLine("")

        For i As Integer = 1 To Me.Manager.nEcosimTimesteps

            ' Calc month
            iMonth = (i - 1) Mod cCore.N_MONTHS
            iYear = CInt(Me.Manager.Core.EcosimFirstYear + Math.Floor((i - 1) / cCore.N_MONTHS))

            bLineAdded = False

            ' For every var to output
            For j As Integer = 1 To iNumCols - 1

                ' Reset total when either processing monthly values OR processing January
                If (bAnnualAverage = False) Or (iMonth = 0) Then
                    asValues(j) = 0
                End If

                ' Add indicator to total
                Select Case cols(j)

                    Case eColTypes.THROUGHPUT : asValues(j) += Me.Manager.ThroughputEcosim(i)
                    Case eColTypes.CAPACITY_ECOSIM : asValues(j) += Me.Manager.CapacityEcosim(i)
                    Case eColTypes.ASCEND_IMPORT : asValues(j) += Me.Manager.AscendImportEcosim(i)
                    Case eColTypes.ASCEND_FLOW : asValues(j) += Me.Manager.AscendFlowEcosim(i)
                    Case eColTypes.ASCEND_EXPORT : asValues(j) += Me.Manager.AscendExportEcosim(i)
                    Case eColTypes.ASCEND_RESP : asValues(j) += Me.Manager.AscendRespEcosim(i)
                    Case eColTypes.OVERHEAD_IMPORT : asValues(j) += Me.Manager.OverheadImportEcosim(i)
                    Case eColTypes.OVERHEAD_FLOW : asValues(j) += Me.Manager.OverheadFlowEcosim(i)
                    Case eColTypes.OVERHEAD_EXPORT : asValues(j) += Me.Manager.OverheadExportEcosim(i)
                    Case eColTypes.OVERHEAD_RESP : asValues(j) += Me.Manager.OverheadRespEcosim(i)
                    Case eColTypes.PCI : asValues(j) += Me.Manager.PCIEcosim(i)
                    Case eColTypes.FCI : asValues(j) += Me.Manager.FCIEcosim(i)
                    Case eColTypes.PATH_LEN : asValues(j) += Me.Manager.PathLengthEcosim(i)
                    Case eColTypes.EXPORT : asValues(j) += Me.Manager.ExportEcosim(i)
                    Case eColTypes.RESP_ECOSIM : asValues(j) += Me.Manager.RespEcosim(i)
                    Case eColTypes.PRIM_PROD : asValues(j) += Me.Manager.PrimaryProdEcosim(i)
                    Case eColTypes.PROD : asValues(j) += Me.Manager.ProdEcosim(i)
                    Case eColTypes.BIOMASS : asValues(j) += Me.Manager.BiomassEcosim(i)
                    Case eColTypes.CATCH : asValues(j) += Me.Manager.CatchEcosim(i)
                    Case eColTypes.PROP_FLOW_DET : asValues(j) += Me.Manager.PropFlowDetEcosim(i)
                    Case eColTypes.ASCEND_TOTAL : asValues(j) += Me.Manager.AscendTotalEcosim(i)
                    Case eColTypes.AMI : asValues(j) += Me.Manager.AMIEcosim(i)
                    Case eColTypes.ENTROPY : asValues(j) += Me.Manager.EntropyEcosim(i)
                    Case eColTypes.TLc : asValues(j) += Me.Manager.TLCatch(i)
                    Case eColTypes.Diversity : asValues(j) += Me.Manager.RelativeDiversity(i)
                    Case eColTypes.FIB : asValues(j) += Me.Manager.FIB(i)
                    Case eColTypes.DET_TE_W : asValues(j) += Me.Manager.DetTransferEfficiencyWeighted(i)
                    Case eColTypes.PP_TE_W : asValues(j) += Me.Manager.PPTransferEfficiencyWeighted(i)
                    Case eColTypes.TOT_TE_W : asValues(j) += Me.Manager.TotTransferEfficiencyWeighted(i)

                End Select

                ' Processing annual averages?
                If (bAnnualAverage) Then
                    ' #Yes: processing december?
                    If (iMonth = (cCore.N_MONTHS - 1)) Then
                        ' #Yes: average value and add it
                        asValues(j) /= cCore.N_MONTHS
                        ' Add year label first
                        If (j = 1) Then
                            sb.Append(iYear)
                            sb.Append(", ")
                        End If
                        sb.Append(cStringUtils.FormatSingle(asValues(j)))
                        sb.Append(", ")
                        bLineAdded = True
                    End If
                Else
                    ' #No: add year label
                    If (j = 1) Then
                        sb.Append(iYear & ":" & (iMonth + 1))
                        sb.Append(", ")
                    End If
                    ' Add value
                    sb.Append(cStringUtils.FormatSingle(asValues(j)))
                    sb.Append(", ")
                    bLineAdded = True
                End If
            Next j

            ' Add newline when a line was added
            If (bLineAdded) Then
                sb.AppendLine()
            End If

        Next i

        Return sb.ToString()
    End Function

    Private Function GetIndicesWithPPRData(ByVal bAnnualAverage As Boolean) As String

        Dim cols As eColTypes() = DirectCast([Enum].GetValues(GetType(eColTypes)), eColTypes())
        Dim iNumCols As Integer = cols.Length ' Include PPR columns
        Dim asValues(iNumCols) As Single
        Dim iMonth As Integer = 0
        Dim iYear As Integer = 0
        Dim bLineAdded As Boolean = False
        Dim sb As New StringBuilder()
        Dim fmt As New cColTypeFormatter()

        ' Header line
        For j As Integer = 0 To iNumCols - 1
            If (j > 0) Then sb.Append(",")
            sb.Append(cStringUtils.ToCSVField(fmt.GetDescriptor(cols(j))))
        Next
        sb.AppendLine("")

        For i As Integer = 1 To Me.Manager.nEcosimTimesteps

            ' Calc month
            iMonth = (i - 1) Mod cCore.N_MONTHS
            iYear = CInt(Me.Manager.Core.EcosimFirstYear + Math.Floor((i - 1) / cCore.N_MONTHS))

            bLineAdded = False

            ' For every var to output
            For j As Integer = 1 To iNumCols - 1

                ' Reset total when either processing monthly values OR processing January
                If (bAnnualAverage = False) Or (iMonth = 0) Then
                    asValues(j) = 0
                End If

                ' Add indicator to total
                Select Case cols(j)

                    Case eColTypes.THROUGHPUT : asValues(j) += Me.Manager.ThroughputEcosim(i)
                    Case eColTypes.CAPACITY_ECOSIM : asValues(j) += Me.Manager.CapacityEcosim(i)
                    Case eColTypes.ASCEND_IMPORT : asValues(j) += Me.Manager.AscendImportEcosim(i)
                    Case eColTypes.ASCEND_FLOW : asValues(j) += Me.Manager.AscendFlowEcosim(i)
                    Case eColTypes.ASCEND_EXPORT : asValues(j) += Me.Manager.AscendExportEcosim(i)
                    Case eColTypes.ASCEND_RESP : asValues(j) += Me.Manager.AscendRespEcosim(i)
                    Case eColTypes.OVERHEAD_IMPORT : asValues(j) += Me.Manager.OverheadImportEcosim(i)
                    Case eColTypes.OVERHEAD_FLOW : asValues(j) += Me.Manager.OverheadFlowEcosim(i)
                    Case eColTypes.OVERHEAD_EXPORT : asValues(j) += Me.Manager.OverheadExportEcosim(i)
                    Case eColTypes.OVERHEAD_RESP : asValues(j) += Me.Manager.OverheadRespEcosim(i)
                    Case eColTypes.PCI : asValues(j) += Me.Manager.PCIEcosim(i)
                    Case eColTypes.FCI : asValues(j) += Me.Manager.FCIEcosim(i)
                    Case eColTypes.PATH_LEN : asValues(j) += Me.Manager.PathLengthEcosim(i)
                    Case eColTypes.EXPORT : asValues(j) += Me.Manager.ExportEcosim(i)
                    Case eColTypes.RESP_ECOSIM : asValues(j) += Me.Manager.RespEcosim(i)
                    Case eColTypes.PRIM_PROD : asValues(j) += Me.Manager.PrimaryProdEcosim(i)
                    Case eColTypes.PROD : asValues(j) += Me.Manager.ProdEcosim(i)
                    Case eColTypes.BIOMASS : asValues(j) += Me.Manager.BiomassEcosim(i)
                    Case eColTypes.CATCH : asValues(j) += Me.Manager.CatchEcosim(i)
                    Case eColTypes.PROP_FLOW_DET : asValues(j) += Me.Manager.PropFlowDetEcosim(i)
                    Case eColTypes.ASCEND_TOTAL : asValues(j) += Me.Manager.AscendTotalEcosim(i)
                    Case eColTypes.AMI : asValues(j) += Me.Manager.AMIEcosim(i)
                    Case eColTypes.ENTROPY : asValues(j) += Me.Manager.EntropyEcosim(i)
                    Case eColTypes.TLc : asValues(j) += Me.Manager.TLCatch(i)
                    Case eColTypes.Diversity : asValues(j) += Me.Manager.RelativeDiversity(i)
                    Case eColTypes.FIB : asValues(j) += Me.Manager.FIB(i)
                    Case eColTypes.DET_TE_W : asValues(j) += Me.Manager.DetTransferEfficiencyWeighted(i)
                    Case eColTypes.PP_TE_W : asValues(j) += Me.Manager.PPTransferEfficiencyWeighted(i)
                    Case eColTypes.TOT_TE_W : asValues(j) += Me.Manager.TotTransferEfficiencyWeighted(i)
                    Case eColTypes.PPR
                    Case eColTypes.LINDEX : asValues(j) += Me.Manager.LIndexEcosim(i)
                    Case eColTypes.PSUST : asValues(j) += Me.Manager.PsustEcosim(i)
                End Select

                ' Processing annual averages?
                If (bAnnualAverage) Then
                    ' #Yes: processing december?
                    If (iMonth = (cCore.N_MONTHS - 1)) Then
                        ' #Yes: add year label first
                        If (j = 1) Then
                            sb.Append(iYear)
                            sb.Append(", ")
                        End If
                        ' Add average value and add it
                        asValues(j) /= cCore.N_MONTHS
                        sb.Append(cStringUtils.FormatSingle(asValues(j)))
                        sb.Append(", ")
                        bLineAdded = True
                    End If
                Else
                    ' #No: 
                    ' First add year
                    If (j = 1) Then
                        sb.Append(iYear & ":" & (iMonth + 1))
                        sb.Append(", ")
                    End If
                    ' Add value
                    sb.Append(cStringUtils.FormatSingle(asValues(j)))
                    sb.Append(", ")
                    bLineAdded = True
                End If

            Next j

            ' Add newline when a line was added
            If (bLineAdded) Then
                sb.AppendLine()
            End If

        Next i

        Return sb.ToString()
    End Function

#End Region ' Internals

End Class
