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
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports System.Collections.Specialized
Imports System.Web
Imports ScientificInterfaceShared.GeoCode

#End Region ' Imports

Public Class frmModelParameters

    Private m_fpName As cEwEFormatProvider = Nothing
    Private m_fpDescription As cEwEFormatProvider = Nothing
    Private m_fpAuthor As cEwEFormatProvider = Nothing
    Private m_fpContact As cEwEFormatProvider = Nothing
    Private m_fpArea As cEwEFormatProvider = Nothing
    Private m_fpNumDigits As cEwEFormatProvider = Nothing
    Private m_fpGroupDigits As cEwEFormatProvider = Nothing
    Private m_fpPSD As cEwEFormatProvider = Nothing
    Private m_fpFirstYear As cEwEFormatProvider = Nothing
    Private m_fpNumYears As cEwEFormatProvider = Nothing
    Private m_fpCountry As cEwEFormatProvider = Nothing
    Private m_fpEcosystemType As cEwEFormatProvider = Nothing
    Private m_fpSouth As cEwEFormatProvider = Nothing
    Private m_fpNorth As cEwEFormatProvider = Nothing
    Private m_fpWest As cEwEFormatProvider = Nothing
    Private m_fpEast As cEwEFormatProvider = Nothing

    Private m_fpDOI As cEwEFormatProvider = Nothing
    Private m_fpURI As cEwEFormatProvider = Nothing
    Private m_fpReference As cEwEFormatProvider = Nothing

    Private m_fpIsCoupled As cEwEFormatProvider = Nothing
    Private m_fpBiodivIndex As cEwEFormatProvider = Nothing

    ' Unit properties
    Private m_propUnitCurrency As cIntegerProperty = Nothing
    Private m_propUnitCurrencyText As cStringProperty = Nothing
    Private m_propUnitTime As cIntegerProperty = Nothing
    Private m_propUnitTimeText As cStringProperty = Nothing
    Private m_propUnitMonetary As cStringProperty = Nothing

    Public Sub New()
        Me.InitializeComponent()
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

        MyBase.OnLoad(e)

        If Me.UIContext Is Nothing Then Return

        Dim eweModel As cEwEModel = Me.UIContext.Core.EwEModel()
        Dim psdParms As cPSDParameters = Me.UIContext.Core.ParticleSizeDistributionParameters()
        Dim pm As cPropertyManager = Me.UIContext.PropertyManager

        Me.m_fpName = New cPropertyFormatProvider(Me.UIContext, Me.m_tbName, eweModel, eVarNameFlags.Name)
        Me.m_fpDescription = New cPropertyFormatProvider(Me.UIContext, Me.m_tbDescription, eweModel, eVarNameFlags.Description)
        Me.m_fpAuthor = New cPropertyFormatProvider(Me.UIContext, Me.m_tbAuthor, eweModel, eVarNameFlags.Author)
        Me.m_fpContact = New cPropertyFormatProvider(Me.UIContext, Me.m_tbContact, eweModel, eVarNameFlags.Contact)
        Me.m_fpArea = New cPropertyFormatProvider(Me.UIContext, Me.m_tbArea, eweModel, eVarNameFlags.Area)
        Me.m_fpNumDigits = New cPropertyFormatProvider(Me.UIContext, Me.m_udNumDigits, eweModel, eVarNameFlags.NumDigits)
        Me.m_fpGroupDigits = New cPropertyFormatProvider(Me.UIContext, Me.m_cbGroupDigits, eweModel, eVarNameFlags.GroupDigits)
        Me.m_fpFirstYear = New cPropertyFormatProvider(Me.UIContext, Me.m_tbxFirstYear, eweModel, eVarNameFlags.EcopathFirstYear)
        Me.m_fpNumYears = New cPropertyFormatProvider(Me.UIContext, Me.m_tbxNumYears, eweModel, eVarNameFlags.EcopathNumYears)
        Me.m_fpNorth = New cPropertyFormatProvider(Me.UIContext, Me.m_nudNorth, eweModel, eVarNameFlags.North)
        Me.m_fpSouth = New cPropertyFormatProvider(Me.UIContext, Me.m_nudSouth, eweModel, eVarNameFlags.South)
        Me.m_fpWest = New cPropertyFormatProvider(Me.UIContext, Me.m_nudWest, eweModel, eVarNameFlags.West)
        Me.m_fpEast = New cPropertyFormatProvider(Me.UIContext, Me.m_nudEast, eweModel, eVarNameFlags.East)
        Me.m_fpCountry = New cPropertyFormatProvider(Me.UIContext, Me.m_cmbCountry, eweModel, eVarNameFlags.Country)
        Me.m_fpEcosystemType = New cPropertyFormatProvider(Me.UIContext, Me.m_cmbEcoType, eweModel, eVarNameFlags.EcosystemType)
        Me.m_fpBiodivIndex = New cPropertyFormatProvider(Me.UIContext, Me.m_cbmBiodivIndex, eweModel, eVarNameFlags.DiversityIndex, Nothing, New cDiversityIndexTypeFormatter())

        Me.m_fpURI = New cPropertyFormatProvider(Me.UIContext, Me.m_tbxPublicationURL, eweModel, eVarNameFlags.PublicationURI)
        Me.m_fpDOI = New cPropertyFormatProvider(Me.UIContext, Me.m_tbxPublicationDOI, eweModel, eVarNameFlags.PublicationDOI)
        Me.m_fpReference = New cPropertyFormatProvider(Me.UIContext, Me.m_tbxReference, eweModel, eVarNameFlags.PublicationReference)

        Me.m_fpPSD = New cPropertyFormatProvider(Me.UIContext, Me.m_chkPSD, psdParms, eVarNameFlags.PSDEnabled)
        Me.m_fpIsCoupled = New cPropertyFormatProvider(Me.UIContext, Me.m_chkIsCoupled, eweModel, eVarNameFlags.IsEcospaceModelCoupled)

        Me.m_propUnitCurrency = DirectCast(pm.GetProperty(Me.UIContext.Core.EwEModel, eVarNameFlags.UnitCurrency), cIntegerProperty)
        AddHandler Me.m_propUnitCurrency.PropertyChanged, AddressOf OnUnitCurrencyChanged

        Me.m_propUnitCurrencyText = DirectCast(pm.GetProperty(Me.UIContext.Core.EwEModel, eVarNameFlags.UnitCurrencyCustomText), cStringProperty)
        AddHandler Me.m_propUnitCurrencyText.PropertyChanged, AddressOf OnUnitCurrencyTextChanged

        Me.m_propUnitTime = DirectCast(pm.GetProperty(Me.UIContext.Core.EwEModel, eVarNameFlags.UnitTime), cIntegerProperty)
        AddHandler Me.m_propUnitTime.PropertyChanged, AddressOf OnUnitTimeChanged

        Me.m_propUnitTimeText = DirectCast(pm.GetProperty(Me.UIContext.Core.EwEModel, eVarNameFlags.UnitTimeCustomText), cStringProperty)
        AddHandler Me.m_propUnitTimeText.PropertyChanged, AddressOf OnUnitTimeTextChanged

        Me.m_propUnitMonetary = DirectCast(pm.GetProperty(Me.UIContext.Core.EwEModel, eVarNameFlags.UnitMonetary), cStringProperty)
        AddHandler Me.m_propUnitMonetary.PropertyChanged, AddressOf OnUnitMonetaryChanged

        Me.m_cmbMonetaryUnit.UIContext = Me.UIContext
        ' Listen to shapes data added or removed messages
        Me.CoreComponents = Nothing

        Me.m_lblAreaUnit.UIContext = Me.UIContext
        Me.m_lblNorthUnit.UIContext = Me.UIContext
        Me.m_lblSouthUnit.UIContext = Me.UIContext
        Me.m_lblWestUnit.UIContext = Me.UIContext
        Me.m_lblEastUnit.UIContext = Me.UIContext

        Me.PatchCurrencyUnitRadioButtonText(Me.rbWetWeight, eUnitCurrencyType.WetWeight)
        Me.PatchCurrencyUnitRadioButtonText(Me.rbCalorie, eUnitCurrencyType.Calorie)
        Me.PatchCurrencyUnitRadioButtonText(Me.rbCarbon, eUnitCurrencyType.Carbon)
        Me.PatchCurrencyUnitRadioButtonText(Me.rbJoules, eUnitCurrencyType.Joules)
        Me.PatchCurrencyUnitRadioButtonText(Me.rbDryWeight, eUnitCurrencyType.DryWeight)
        Me.PatchCurrencyUnitRadioButtonText(Me.rbNitrogen, eUnitCurrencyType.Nitrogen)
        Me.PatchCurrencyUnitRadioButtonText(Me.rbPhosporus, eUnitCurrencyType.Phosporous)

        ' Kick!
        Me.OnUnitCurrencyChanged(Me.m_propUnitCurrency, cProperty.eChangeFlags.All)
        Me.OnUnitCurrencyTextChanged(Me.m_propUnitCurrencyText, cProperty.eChangeFlags.All)
        Me.OnUnitTimeChanged(Me.m_propUnitTime, cProperty.eChangeFlags.All)
        Me.OnUnitTimeTextChanged(Me.m_propUnitTimeText, cProperty.eChangeFlags.All)
        Me.OnUnitMonetaryChanged(Me.m_propUnitMonetary, cProperty.eChangeFlags.All)

        Me.FillEcoBaseCombos()

        Me.m_hdrClassification.IsCollapsed = True
        Me.m_hdrPublication.IsCollapsed = True

        Me.m_gcmbAreaName.AutoSearch = False

        Me.UpdateControls()

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        Me.m_fpName.Release() '
        Me.m_fpDescription.Release()
        Me.m_fpAuthor.Release()
        Me.m_fpContact.Release()
        Me.m_fpArea.Release()
        Me.m_fpNumDigits.Release()
        Me.m_fpGroupDigits.Release()
        Me.m_fpFirstYear.Release()
        Me.m_fpNumYears.Release()
        Me.m_fpNorth.Release()
        Me.m_fpSouth.Release()
        Me.m_fpWest.Release()
        Me.m_fpEast.Release()
        Me.m_fpPSD.Release()
        Me.m_fpIsCoupled.Release()
        Me.m_fpCountry.Release()
        Me.m_fpEcosystemType.Release()
        Me.m_fpBiodivIndex.Release()

        Me.m_fpURI.Release()
        Me.m_fpDOI.Release()
        Me.m_fpReference.Release()

        ' Clean up ( not really necessary since bas class takes care of this, but hey :) )
        Me.CoreComponents = Nothing

        RemoveHandler Me.m_propUnitCurrency.PropertyChanged, AddressOf OnUnitCurrencyChanged
        Me.m_propUnitCurrency = Nothing

        RemoveHandler Me.m_propUnitCurrencyText.PropertyChanged, AddressOf OnUnitCurrencyTextChanged
        Me.m_propUnitCurrencyText = Nothing

        RemoveHandler Me.m_propUnitTime.PropertyChanged, AddressOf OnUnitTimeChanged
        Me.m_propUnitTime = Nothing

        RemoveHandler Me.m_propUnitTimeText.PropertyChanged, AddressOf OnUnitTimeTextChanged
        Me.m_propUnitTimeText = Nothing

        RemoveHandler Me.m_propUnitMonetary.PropertyChanged, AddressOf OnUnitMonetaryChanged
        Me.m_propUnitMonetary = Nothing

        MyBase.OnFormClosed(e)

    End Sub

    Protected Overrides Sub OnStyleGuideChanged(ct As ScientificInterfaceShared.Style.cStyleGuide.eChangeType)
        MyBase.OnStyleGuideChanged(ct)

        If ((ct And cStyleGuide.eChangeType.EcobaseLists) > 0) Then
            Me.FillEcoBaseCombos()
        End If

    End Sub

#Region " Unit handling "

    Dim m_bInUpdate As Boolean = False

#Region " Currency "

    Private Sub PatchCurrencyUnitRadioButtonText(ByVal rb As RadioButton, ByVal uct As eUnitCurrencyType)
        rb.Text = cStringUtils.Localize(rb.Text, Me.StyleGuide.CurrencyUnitText(uct))
    End Sub

    Private Sub OnUnitCurrencyRadioChanged(ByVal sender As Object, ByVal eventargs As EventArgs) _
        Handles rbWetWeight.CheckedChanged, rbJoules.CheckedChanged, rbCalorie.CheckedChanged, _
                rbCarbon.CheckedChanged, rbDryWeight.CheckedChanged, rbNitrogen.CheckedChanged, rbPhosporus.CheckedChanged, _
                rbCurrencyEnergyOther.CheckedChanged, rbNutrientOther.CheckedChanged, rbCurrencyEnergyOther.CheckedChanged

        If (Me.m_propUnitCurrency Is Nothing) Then Return

        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True
        If Me.rbWetWeight.Checked Then m_propUnitCurrency.SetValue(eUnitCurrencyType.WetWeight)
        If Me.rbJoules.Checked Then m_propUnitCurrency.SetValue(eUnitCurrencyType.Joules)
        If Me.rbCalorie.Checked Then m_propUnitCurrency.SetValue(eUnitCurrencyType.Calorie)
        If Me.rbCarbon.Checked Then m_propUnitCurrency.SetValue(eUnitCurrencyType.Carbon)
        If Me.rbDryWeight.Checked Then m_propUnitCurrency.SetValue(eUnitCurrencyType.DryWeight)
        If Me.rbNitrogen.Checked Then m_propUnitCurrency.SetValue(eUnitCurrencyType.Nitrogen)
        If Me.rbPhosporus.Checked Then m_propUnitCurrency.SetValue(eUnitCurrencyType.Phosporous)
        If Me.rbCurrencyEnergyOther.Checked Then m_propUnitCurrency.SetValue(eUnitCurrencyType.CustomEnergy)
        If Me.rbNutrientOther.Checked Then m_propUnitCurrency.SetValue(eUnitCurrencyType.CustomNutrient)
        Me.m_bInUpdate = False

    End Sub

    Private Sub OnUnitCurrencyChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)

        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True

        Select Case DirectCast(prop.GetValue(), eUnitCurrencyType)
            Case eUnitCurrencyType.WetWeight : Me.rbWetWeight.Checked = True
            Case eUnitCurrencyType.Joules : Me.rbJoules.Checked = True
            Case eUnitCurrencyType.Calorie : Me.rbCalorie.Checked = True
            Case eUnitCurrencyType.Carbon : Me.rbCarbon.Checked = True
            Case eUnitCurrencyType.DryWeight : Me.rbDryWeight.Checked = True
            Case eUnitCurrencyType.Nitrogen : Me.rbNitrogen.Checked = True
            Case eUnitCurrencyType.Phosporous : Me.rbPhosporus.Checked = True
            Case eUnitCurrencyType.CustomEnergy : Me.rbCurrencyEnergyOther.Checked = True
            Case eUnitCurrencyType.CustomNutrient : Me.rbNutrientOther.Checked = True
        End Select

        Me.m_bInUpdate = False
    End Sub

    Private Sub OnUnitCurrencyTextChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)
        If Me.rbCurrencyEnergyOther.Checked Then
            Me.tbCurrencyEnergyOther.Text = CStr(prop.GetValue())
            Me.tbCurrencyNutrientOther.Text = ""
        Else
            Me.tbCurrencyNutrientOther.Text = CStr(prop.GetValue())
            Me.tbCurrencyEnergyOther.Text = ""
        End If
    End Sub

    Private Sub OnCustomEnergyTextValidated(ByVal sender As Object, ByVal eventargs As EventArgs) Handles tbCurrencyEnergyOther.Validated
        Me.m_propUnitCurrencyText.SetValue(tbCurrencyEnergyOther.Text)
    End Sub

    Private Sub OnCustomEnergySetFocus(ByVal sender As Object, ByVal eventargs As EventArgs) Handles tbCurrencyEnergyOther.GotFocus
        Me.rbCurrencyEnergyOther.Checked = True
    End Sub

    Private Sub OnCustomNutrientTextValidated(ByVal sender As Object, ByVal eventargs As EventArgs) Handles tbCurrencyNutrientOther.Validated
        Me.m_propUnitCurrencyText.SetValue(tbCurrencyNutrientOther.Text)
    End Sub

    Private Sub OnCustomNutrientSetFocus(ByVal sender As Object, ByVal eventargs As EventArgs) Handles tbCurrencyNutrientOther.GotFocus
        Me.rbNutrientOther.Checked = True
    End Sub

#End Region ' Currency

#Region " Time "

    Private Sub OnUnitTimeRadioChanged(ByVal sender As Object, ByVal eventargs As EventArgs) _
        Handles rbYear.CheckedChanged, rbDay.CheckedChanged, rbTimeOther.CheckedChanged

        If (Me.m_propUnitCurrency Is Nothing) Then Return

        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True

        If Me.rbYear.Checked Then m_propUnitTime.SetValue(eUnitTimeType.Year)
        If Me.rbDay.Checked Then m_propUnitTime.SetValue(eUnitTimeType.Day)
        If Me.rbTimeOther.Checked Then m_propUnitTime.SetValue(eUnitTimeType.Custom)

        Me.m_bInUpdate = False

    End Sub

    Private Sub OnUnitTimeChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)

        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True

        Select Case DirectCast(prop.GetValue(), eUnitTimeType)
            Case eUnitTimeType.Year : Me.rbYear.Checked = True
            Case eUnitTimeType.Day : Me.rbDay.Checked = True
            Case eUnitTimeType.Custom : Me.rbTimeOther.Checked = True
        End Select

        Me.m_bInUpdate = False
    End Sub

    Private Sub OnUnitTimeTextChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)
        Me.txbTimeOther.Text = CStr(prop.GetValue())
    End Sub

    Private Sub OnUnitTimeSetFocus(ByVal sender As Object, ByVal eventargs As EventArgs) Handles txbTimeOther.GotFocus
        Me.rbTimeOther.Checked = True
    End Sub

    Private Sub OnUnitTimeTextValidated(ByVal sender As Object, ByVal eventargs As EventArgs) Handles txbTimeOther.Validated
        Me.m_propUnitTimeText.SetValue(txbTimeOther.Text)
    End Sub

#End Region ' Time

#Region " Monetary "

    Private Sub m_cmbMonetaryUnit_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_cmbMonetaryUnit.SelectedIndexChanged
        Me.m_propUnitMonetary.SetValue(Me.m_cmbMonetaryUnit.Unit)
    End Sub

    Private Sub OnUnitMonetaryChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)
        If (Me.m_bInUpdate) Then Return
        Me.m_bInUpdate = True
        Me.m_cmbMonetaryUnit.Unit = DirectCast(prop.GetValue(), String)
        Me.m_bInUpdate = False
    End Sub

#End Region ' Monetary

#End Region ' Unit handling

#Region " Control events "

    Private Sub OnViewPublication(sender As System.Object, e As System.EventArgs) _
        Handles m_llViewPublication.Click

        Dim strDOI As String = Me.m_tbxPublicationDOI.Text

        Try

            Dim cmdh As cCommandHandler = Me.CommandHandler
            Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            Debug.Assert(cmd IsNot Nothing)

            cmd.Invoke("http://doi.org/" & HttpUtility.UrlEncode(strDOI))

        Catch ex As Exception
            cLog.Write(ex, "frmModelParameters.OnViewDOIOnline(" & strDOI & ")")
        End Try

    End Sub

    Private Sub OnSearchLocation(sender As Object, e As EventArgs) _
        Handles m_btnSearch.Click
        Me.m_gcmbAreaName.Search()
    End Sub

    Private Sub OnSearchingLocation(ByVal sender As cGeocodeLookupComboBox, ByVal bSearching As Boolean) _
        Handles m_gcmbAreaName.OnSeaching

        If bSearching Then
            Me.m_btnSearch.Image = ScientificInterfaceShared.My.Resources.ani_loader
        Else
            Me.m_btnSearch.Image = Nothing
        End If
    End Sub

    Private Sub OnLocationSelected(ByVal sender As Object, ByVal e As EventArgs) _
        Handles m_gcmbAreaName.SelectedIndexChanged

        If (Me.IsDisposed) Then Return

        Dim location As cGeoCodeLocation = Me.m_gcmbAreaName.SelectedLocation
        If location Is Nothing Then Return

        Me.m_fpNorth.Value = location.North
        Me.m_fpEast.Value = location.East
        Me.m_fpSouth.Value = location.South
        Me.m_fpWest.Value = location.West

    End Sub

#End Region ' Control events

#Region " Internals "

    Private Sub FillEcoBaseCombos()

        Me.FillCombo(Me.m_cmbCountry, Me.StyleGuide.EcoBaseFields(cStyleGuide.eEcobaseFieldType.CountryName))
        Me.FillCombo(Me.m_cmbEcoType, Me.StyleGuide.EcoBaseFields(cStyleGuide.eEcobaseFieldType.EcosystemType))

    End Sub

    Private Sub FillCombo(cmb As ComboBox, data As StringCollection)

        Dim obj As Object = cmb.SelectedItem()
        cmb.Items.Clear()
        If (data IsNot Nothing) Then
            For Each str As String In data
                cmb.Items.Add(str)
            Next
        End If
        cmb.SelectedItem = obj

    End Sub

#End Region ' Internals

End Class