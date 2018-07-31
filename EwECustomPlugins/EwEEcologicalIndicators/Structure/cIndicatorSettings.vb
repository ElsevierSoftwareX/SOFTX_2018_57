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

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Central indicator definitions.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cIndicatorSettings

#Region " Private variables "

    Private m_lIndicatorGroups As New List(Of cIndicatorInfoGroup)

#End Region ' Private variables

#Region " Constructor "

    Public Sub New()
        Me.Populate()
    End Sub

#End Region ' Constructor

#Region " Public fields "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a new group to the settings.
    ''' </summary>
    ''' <param name="strName">Name to assign to the group.</param>
    ''' <param name="strDescription">Optional description to assign to the group.</param>
    ''' <returns>The new group.</returns>
    ''' -----------------------------------------------------------------------
    Public Function AddGroup(ByVal strName As String, _
                             Optional ByVal strDescription As String = "") As cIndicatorInfoGroup
        Dim grp As New cIndicatorInfoGroup(strName, strDescription)
        Me.m_lIndicatorGroups.Add(grp)
        Return grp
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the number of <see cref="cIndicatorInfoGroup"/>s in the settings.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property NumIndicatorGroups As Integer
        Get
            Return Me.m_lIndicatorGroups.Count
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="cIndicatorInfoGroup"/>s at a given index in the settings.
    ''' </summary>
    ''' <param name="index">The index to obtain the <see cref="cIndicatorInfoGroup"/> for.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IndicatorGroup(ByVal index As Integer) As cIndicatorInfoGroup
        Get
            Return Me.m_lIndicatorGroups(index)
        End Get
    End Property

#End Region ' Public fields

#Region " Internals "

    Private Sub Populate()

        Dim grp As cIndicatorInfoGroup = Nothing
        Dim ind As cIndicatorInfo = Nothing

        ' Note that the name an indicator as specified below must match the name of the public function used by
        ' cIndicator to expose the value for that indicator. The function is lookup up at runtime via reflection.
        Dim strUnitCurrency As String = "[currency]"
        Dim strUnitCatch As String = "[currency]/[time]"

        ' 1 biomass-based indicators
        grp = Me.AddGroup(My.Resources.GROUP_BIOMASS, My.Resources.GROUP_BIOMASS_DESC)
        grp.Add("TotalB", My.Resources.IND_TOTALB, My.Resources.IND_TOTALB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("CommercialB", My.Resources.IND_COMMB, My.Resources.IND_COMMB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("FishB", My.Resources.IND_FISHB, My.Resources.IND_FISHB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("InveB", My.Resources.IND_INVEB, My.Resources.IND_INVEB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("InveFishB", My.Resources.IND_INVFISHB_RATIO, My.Resources.IND_INVFISHB_RATIO_DESC, My.Resources.IND_VALUE_B_RATIO)
        grp.Add("DemB", My.Resources.IND_DEMB, My.Resources.IND_DEMB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("PelB", My.Resources.IND_PELB, My.Resources.IND_PELB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("DemPelB", My.Resources.IND_DEMPELB_RATIO, My.Resources.IND_DEMPELB_RATIO_DESC, My.Resources.IND_VALUE_B_RATIO)
        grp.Add("PredB", My.Resources.IND_PREDB, My.Resources.IND_PREDB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("KemptonsQ", My.Resources.IND_KEMPTONSQ, My.Resources.IND_KEMPTONQ_DESC, My.Resources.IND_KEMPTONSQ)
        grp.Add("ShannonDiversity", My.Resources.IND_SHANNON, My.Resources.IND_SHANNON_DESC, My.Resources.IND_SHANNON)

        ' 2 catch-based indicators
        grp = Me.AddGroup(My.Resources.GROUP_CATCH, My.Resources.GROUP_CATCH_DESC)
        grp.Add("Ctotal", My.Resources.IND_TOTALC, My.Resources.IND_TOTALC_DESC, My.Resources.IND_VALUE_C, strUnitCatch)
        grp.Add("FishC", My.Resources.IND_FISHC, My.Resources.IND_FISHC_DESC, My.Resources.IND_VALUE_C, strUnitCatch)
        grp.Add("InveC", My.Resources.IND_INVC, My.Resources.IND_INVC_DESC, My.Resources.IND_VALUE_C, strUnitCatch)
        grp.Add("InveFishC", My.Resources.IND_INVFISHC, My.Resources.IND_INVFISHC_DESC, My.Resources.IND_VALUE_C_RATIO)
        grp.Add("DemC", My.Resources.IND_DEMC, My.Resources.IND_DEMC_DESC, My.Resources.IND_VALUE_C, strUnitCatch)
        grp.Add("PelC", My.Resources.IND_PELC, My.Resources.IND_PELC_DESC, My.Resources.IND_VALUE_C, strUnitCatch)
        grp.Add("DemPelC", My.Resources.IND_DEMPELC_RATIO, My.Resources.IND_DEMPELC_RATIO_DESC, My.Resources.IND_VALUE_C_RATIO)
        grp.Add("sC4", My.Resources.IND_PREDC, My.Resources.IND_PREDC_DESC, My.Resources.IND_VALUE_C, strUnitCatch)
        grp.Add("DT", My.Resources.IND_DIS, My.Resources.IND_DIS_DESC, My.Resources.IND_VALUE_DISCARDS, strUnitCatch)

        ' 3 trophic-based indicators
        grp = Me.AddGroup(My.Resources.GROUP_TROPHIC, My.Resources.GROUP_TROPHIC_DESC)
        grp.Add("TLC", My.Resources.IND_TLC, My.Resources.IND_TLC_DESC, My.Resources.IND_VALUE_TL)
        grp.Add("MTI", My.Resources.IND_MTI, String.Format(My.Resources.IND_MTIX_DESC, 3.25), My.Resources.IND_VALUE_TL)
        grp.Add("TLco", My.Resources.IND_TLCo, My.Resources.IND_TLCo_DESC, My.Resources.IND_VALUE_TL)
        grp.Add("TLco2", String.Format(My.Resources.IND_TLCoX, 2), String.Format(My.Resources.IND_TLCoX_DESC, 2), My.Resources.IND_VALUE_TL)
        grp.Add("TLco325", String.Format(My.Resources.IND_TLCoX, 3.25), String.Format(My.Resources.IND_TLCoX_DESC, 3.25), My.Resources.IND_VALUE_TL)
        grp.Add("TLco4", String.Format(My.Resources.IND_TLCoX, 4), String.Format(My.Resources.IND_TLCoX_DESC, 4), My.Resources.IND_VALUE_TL)

        ' 4 species-based indicators
        grp = Me.AddGroup(My.Resources.GROUP_SPECIES, My.Resources.GROUP_SPECIES_DESC)
        grp.Add("IVIC", My.Resources.IND_IVIC, My.Resources.IND_IVIC_DESC, My.Resources.IND_VALUE_INTR_VUL_INDEX)
        grp.Add("EndemicB", My.Resources.IND_ENDB, My.Resources.IND_ENDB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("EndemicC", My.Resources.IND_ENDC, My.Resources.IND_ENDC_DESC, My.Resources.IND_VALUE_C, strUnitCatch)
        grp.Add("IUCNB", My.Resources.IND_IUCNB, My.Resources.IND_IUCNB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("IUCNC", My.Resources.IND_IUCNC, My.Resources.IND_IUCNC_DESC, My.Resources.IND_VALUE_C, strUnitCatch)
        grp.Add("MSRB", My.Resources.IND_MSRB, My.Resources.IND_MSRB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        grp.Add("MSRC", My.Resources.IND_MSRC, My.Resources.IND_MSRC_DESC, My.Resources.IND_VALUE_C, strUnitCatch)

        ' 5 size-based indicators
        grp = Me.AddGroup(My.Resources.GROUP_SIZE, My.Resources.GROUP_SIZE_DESC)
        grp.Add("MLengthB", My.Resources.IND_MLB, My.Resources.IND_MLB_DESC, My.Resources.IND_VALUE_ML, My.Resources.UNIT_LENGTH_CM)
        grp.Add("MLengthC", My.Resources.IND_MLC, My.Resources.IND_MLC_DESC, My.Resources.IND_VALUE_ML, My.Resources.UNIT_LENGTH_CM)
        grp.Add("MWeightB", My.Resources.IND_MWB, My.Resources.IND_MWB_DESC, My.Resources.IND_VALUE_MW, My.Resources.UNIT_WEIGHT_G)
        grp.Add("MWeightC", My.Resources.IND_MWC, My.Resources.IND_MWC_DESC, My.Resources.IND_VALUE_MW, My.Resources.UNIT_WEIGHT_G)
        grp.Add("MLifeSpanB", My.Resources.IND_MLSC, My.Resources.IND_MLSC_DESC, My.Resources.IND_VALUE_AGE, My.Resources.UNIT_TIME_YEAR)
        grp.Add("MLifeSpanC", My.Resources.IND_MLSB, My.Resources.IND_MLSB_DESC, My.Resources.IND_VALUE_AGE, My.Resources.UNIT_TIME_YEAR)

        '' 6 MSFD
        'grp = Me.AddGroup(My.Resources.GROUP_MSDF, My.Resources.GROUP_MSDF_DESC)
        'grp.Add("TotalB", My.Resources.IND_TOTALB, My.Resources.IND_TOTALB_DESC, My.Resources.IND_VALUE_B, strUnitCurrency)
        'grp.Add("Ctotal", My.Resources.IND_TOTALC, My.Resources.IND_TOTALC_DESC, My.Resources.IND_VALUE_C, aunitCatch)

    End Sub

#End Region ' Internals

End Class
