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

Imports ScientificInterfaceShared.Forms
Imports ScientificInterfaceShared.Controls

Partial Class frmModelParameters
    Inherits frmEwE

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmModelParameters))
        Dim CMarineRegionsLookup2 As ScientificInterfaceShared.GeoCode.cMarineRegionsLookup = New ScientificInterfaceShared.GeoCode.cMarineRegionsLookup()
        Me.m_udNumDigits = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.lbNumDigits = New System.Windows.Forms.Label()
        Me.m_lblOptions = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_lbDescription = New System.Windows.Forms.Label()
        Me.m_lbScenarioName = New System.Windows.Forms.Label()
        Me.m_lblModel = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tbName = New System.Windows.Forms.TextBox()
        Me.m_lbAuthor = New System.Windows.Forms.Label()
        Me.m_tbAuthor = New System.Windows.Forms.TextBox()
        Me.m_lbContact = New System.Windows.Forms.Label()
        Me.m_lblFirstYear = New System.Windows.Forms.Label()
        Me.m_lblArea = New System.Windows.Forms.Label()
        Me.m_tbArea = New System.Windows.Forms.TextBox()
        Me.m_lblAreaUnit = New ScientificInterfaceShared.Controls.cEwEUnitLabel()
        Me.m_tlpUnits = New System.Windows.Forms.TableLayoutPanel()
        Me.m_gbCurrencyUnit = New System.Windows.Forms.GroupBox()
        Me.tbCurrencyNutrientOther = New System.Windows.Forms.TextBox()
        Me.rbNitrogen = New System.Windows.Forms.RadioButton()
        Me.rbNutrientOther = New System.Windows.Forms.RadioButton()
        Me.rbPhosporus = New System.Windows.Forms.RadioButton()
        Me.tbCurrencyEnergyOther = New System.Windows.Forms.TextBox()
        Me.rbCurrencyEnergyOther = New System.Windows.Forms.RadioButton()
        Me.m_lblNutrientRelated = New System.Windows.Forms.Label()
        Me.m_lblEnergyRelated = New System.Windows.Forms.Label()
        Me.rbWetWeight = New System.Windows.Forms.RadioButton()
        Me.rbJoules = New System.Windows.Forms.RadioButton()
        Me.rbCalorie = New System.Windows.Forms.RadioButton()
        Me.rbCarbon = New System.Windows.Forms.RadioButton()
        Me.rbDryWeight = New System.Windows.Forms.RadioButton()
        Me.m_gbTimeUnits = New System.Windows.Forms.GroupBox()
        Me.txbTimeOther = New System.Windows.Forms.TextBox()
        Me.rbTimeOther = New System.Windows.Forms.RadioButton()
        Me.rbDay = New System.Windows.Forms.RadioButton()
        Me.rbYear = New System.Windows.Forms.RadioButton()
        Me.m_gbMonetaryUnits = New System.Windows.Forms.GroupBox()
        Me.m_lblMonetaryUnit = New System.Windows.Forms.Label()
        Me.m_cmbMonetaryUnit = New ScientificInterfaceShared.Controls.cMonetaryUnitComboBox()
        Me.m_gbNumFormatting = New System.Windows.Forms.GroupBox()
        Me.m_cbGroupDigits = New System.Windows.Forms.CheckBox()
        Me.m_chkPSD = New System.Windows.Forms.CheckBox()
        Me.m_hdrExecution = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_nudNorth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudSouth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudWest = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudEast = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_lblNorth = New System.Windows.Forms.Label()
        Me.m_lblWest = New System.Windows.Forms.Label()
        Me.m_lblEast = New System.Windows.Forms.Label()
        Me.m_lblSouth = New System.Windows.Forms.Label()
        Me.m_plDescr = New System.Windows.Forms.Panel()
        Me.m_btnSearch = New System.Windows.Forms.Button()
        Me.m_gcmbAreaName = New ScientificInterfaceShared.Controls.cGeocodeLookupComboBox()
        Me.m_lblAreaName = New System.Windows.Forms.Label()
        Me.m_tbContact = New System.Windows.Forms.TextBox()
        Me.m_tbDescription = New System.Windows.Forms.TextBox()
        Me.m_tbxNumYears = New System.Windows.Forms.TextBox()
        Me.m_tbxFirstYear = New System.Windows.Forms.TextBox()
        Me.m_lblNoYears = New System.Windows.Forms.Label()
        Me.m_lblSouthUnit = New ScientificInterfaceShared.Controls.cEwEUnitLabel()
        Me.m_lblEastUnit = New ScientificInterfaceShared.Controls.cEwEUnitLabel()
        Me.m_lblNorthUnit = New ScientificInterfaceShared.Controls.cEwEUnitLabel()
        Me.m_lblModelBounds = New System.Windows.Forms.Label()
        Me.m_lblWestUnit = New ScientificInterfaceShared.Controls.cEwEUnitLabel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.m_tlpContent = New System.Windows.Forms.TableLayoutPanel()
        Me.m_plExecution = New System.Windows.Forms.Panel()
        Me.m_lblBiodivIndex = New System.Windows.Forms.Label()
        Me.m_cbmBiodivIndex = New System.Windows.Forms.ComboBox()
        Me.m_chkIsCoupled = New System.Windows.Forms.CheckBox()
        Me.m_plMetadata = New System.Windows.Forms.Panel()
        Me.m_cmbEcoType = New System.Windows.Forms.ComboBox()
        Me.m_lblEcoType = New System.Windows.Forms.Label()
        Me.m_cmbCountry = New System.Windows.Forms.ComboBox()
        Me.m_lblCountry = New System.Windows.Forms.Label()
        Me.m_hdrClassification = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_plPublication = New System.Windows.Forms.Panel()
        Me.m_llViewPublication = New System.Windows.Forms.LinkLabel()
        Me.m_hdrPublication = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tbxReference = New System.Windows.Forms.TextBox()
        Me.m_tbxPublicationDOI = New System.Windows.Forms.TextBox()
        Me.m_tbxPublicationURL = New System.Windows.Forms.TextBox()
        Me.m_lblReference = New System.Windows.Forms.Label()
        Me.m_lblPublicationDOI = New System.Windows.Forms.Label()
        Me.m_lblPublicationURL = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        CType(Me.m_udNumDigits, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tlpUnits.SuspendLayout()
        Me.m_gbCurrencyUnit.SuspendLayout()
        Me.m_gbTimeUnits.SuspendLayout()
        Me.m_gbMonetaryUnits.SuspendLayout()
        Me.m_gbNumFormatting.SuspendLayout()
        CType(Me.m_nudNorth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudSouth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudWest, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudEast, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_plDescr.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.m_tlpContent.SuspendLayout()
        Me.m_plExecution.SuspendLayout()
        Me.m_plMetadata.SuspendLayout()
        Me.m_plPublication.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_udNumDigits
        '
        Me.m_udNumDigits.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        resources.ApplyResources(Me.m_udNumDigits, "m_udNumDigits")
        Me.m_udNumDigits.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.m_udNumDigits.Name = "m_udNumDigits"
        '
        'lbNumDigits
        '
        resources.ApplyResources(Me.lbNumDigits, "lbNumDigits")
        Me.lbNumDigits.Name = "lbNumDigits"
        '
        'm_lblOptions
        '
        Me.m_lblOptions.CanCollapseParent = True
        Me.m_lblOptions.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_lblOptions, "m_lblOptions")
        Me.m_lblOptions.IsCollapsed = False
        Me.m_lblOptions.Name = "m_lblOptions"
        '
        'm_lbDescription
        '
        resources.ApplyResources(Me.m_lbDescription, "m_lbDescription")
        Me.m_lbDescription.Name = "m_lbDescription"
        '
        'm_lbScenarioName
        '
        resources.ApplyResources(Me.m_lbScenarioName, "m_lbScenarioName")
        Me.m_lbScenarioName.Name = "m_lbScenarioName"
        '
        'm_lblModel
        '
        Me.m_lblModel.CanCollapseParent = True
        Me.m_lblModel.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_lblModel, "m_lblModel")
        Me.m_lblModel.IsCollapsed = False
        Me.m_lblModel.Name = "m_lblModel"
        '
        'm_tbName
        '
        resources.ApplyResources(Me.m_tbName, "m_tbName")
        Me.m_tbName.Name = "m_tbName"
        '
        'm_lbAuthor
        '
        resources.ApplyResources(Me.m_lbAuthor, "m_lbAuthor")
        Me.m_lbAuthor.Name = "m_lbAuthor"
        '
        'm_tbAuthor
        '
        resources.ApplyResources(Me.m_tbAuthor, "m_tbAuthor")
        Me.m_tbAuthor.Name = "m_tbAuthor"
        '
        'm_lbContact
        '
        resources.ApplyResources(Me.m_lbContact, "m_lbContact")
        Me.m_lbContact.Name = "m_lbContact"
        '
        'm_lblFirstYear
        '
        resources.ApplyResources(Me.m_lblFirstYear, "m_lblFirstYear")
        Me.m_lblFirstYear.Name = "m_lblFirstYear"
        '
        'm_lblArea
        '
        resources.ApplyResources(Me.m_lblArea, "m_lblArea")
        Me.m_lblArea.Name = "m_lblArea"
        '
        'm_tbArea
        '
        resources.ApplyResources(Me.m_tbArea, "m_tbArea")
        Me.m_tbArea.Name = "m_tbArea"
        '
        'm_lblAreaUnit
        '
        resources.ApplyResources(Me.m_lblAreaUnit, "m_lblAreaUnit")
        Me.m_lblAreaUnit.Name = "m_lblAreaUnit"
        Me.m_lblAreaUnit.UIContext = Nothing
        '
        'm_tlpUnits
        '
        resources.ApplyResources(Me.m_tlpUnits, "m_tlpUnits")
        Me.m_tlpUnits.Controls.Add(Me.m_gbCurrencyUnit, 0, 1)
        Me.m_tlpUnits.Controls.Add(Me.m_gbTimeUnits, 1, 1)
        Me.m_tlpUnits.Controls.Add(Me.m_gbMonetaryUnits, 1, 0)
        Me.m_tlpUnits.Controls.Add(Me.m_gbNumFormatting, 0, 0)
        Me.m_tlpUnits.Name = "m_tlpUnits"
        '
        'm_gbCurrencyUnit
        '
        Me.m_gbCurrencyUnit.Controls.Add(Me.tbCurrencyNutrientOther)
        Me.m_gbCurrencyUnit.Controls.Add(Me.rbNitrogen)
        Me.m_gbCurrencyUnit.Controls.Add(Me.rbNutrientOther)
        Me.m_gbCurrencyUnit.Controls.Add(Me.rbPhosporus)
        Me.m_gbCurrencyUnit.Controls.Add(Me.tbCurrencyEnergyOther)
        Me.m_gbCurrencyUnit.Controls.Add(Me.rbCurrencyEnergyOther)
        Me.m_gbCurrencyUnit.Controls.Add(Me.m_lblNutrientRelated)
        Me.m_gbCurrencyUnit.Controls.Add(Me.m_lblEnergyRelated)
        Me.m_gbCurrencyUnit.Controls.Add(Me.rbWetWeight)
        Me.m_gbCurrencyUnit.Controls.Add(Me.rbJoules)
        Me.m_gbCurrencyUnit.Controls.Add(Me.rbCalorie)
        Me.m_gbCurrencyUnit.Controls.Add(Me.rbCarbon)
        Me.m_gbCurrencyUnit.Controls.Add(Me.rbDryWeight)
        resources.ApplyResources(Me.m_gbCurrencyUnit, "m_gbCurrencyUnit")
        Me.m_gbCurrencyUnit.Name = "m_gbCurrencyUnit"
        Me.m_gbCurrencyUnit.TabStop = False
        '
        'tbCurrencyNutrientOther
        '
        resources.ApplyResources(Me.tbCurrencyNutrientOther, "tbCurrencyNutrientOther")
        Me.tbCurrencyNutrientOther.Name = "tbCurrencyNutrientOther"
        '
        'rbNitrogen
        '
        resources.ApplyResources(Me.rbNitrogen, "rbNitrogen")
        Me.rbNitrogen.Name = "rbNitrogen"
        Me.rbNitrogen.UseVisualStyleBackColor = True
        '
        'rbNutrientOther
        '
        resources.ApplyResources(Me.rbNutrientOther, "rbNutrientOther")
        Me.rbNutrientOther.Name = "rbNutrientOther"
        Me.rbNutrientOther.UseVisualStyleBackColor = True
        '
        'rbPhosporus
        '
        resources.ApplyResources(Me.rbPhosporus, "rbPhosporus")
        Me.rbPhosporus.Name = "rbPhosporus"
        Me.rbPhosporus.UseVisualStyleBackColor = True
        '
        'tbCurrencyEnergyOther
        '
        resources.ApplyResources(Me.tbCurrencyEnergyOther, "tbCurrencyEnergyOther")
        Me.tbCurrencyEnergyOther.Name = "tbCurrencyEnergyOther"
        '
        'rbCurrencyEnergyOther
        '
        resources.ApplyResources(Me.rbCurrencyEnergyOther, "rbCurrencyEnergyOther")
        Me.rbCurrencyEnergyOther.Name = "rbCurrencyEnergyOther"
        Me.rbCurrencyEnergyOther.UseVisualStyleBackColor = True
        '
        'm_lblNutrientRelated
        '
        resources.ApplyResources(Me.m_lblNutrientRelated, "m_lblNutrientRelated")
        Me.m_lblNutrientRelated.Name = "m_lblNutrientRelated"
        '
        'm_lblEnergyRelated
        '
        resources.ApplyResources(Me.m_lblEnergyRelated, "m_lblEnergyRelated")
        Me.m_lblEnergyRelated.Name = "m_lblEnergyRelated"
        '
        'rbWetWeight
        '
        resources.ApplyResources(Me.rbWetWeight, "rbWetWeight")
        Me.rbWetWeight.Checked = True
        Me.rbWetWeight.Name = "rbWetWeight"
        Me.rbWetWeight.TabStop = True
        Me.rbWetWeight.UseVisualStyleBackColor = True
        '
        'rbJoules
        '
        resources.ApplyResources(Me.rbJoules, "rbJoules")
        Me.rbJoules.Name = "rbJoules"
        Me.rbJoules.UseVisualStyleBackColor = True
        '
        'rbCalorie
        '
        resources.ApplyResources(Me.rbCalorie, "rbCalorie")
        Me.rbCalorie.Name = "rbCalorie"
        Me.rbCalorie.UseVisualStyleBackColor = True
        '
        'rbCarbon
        '
        resources.ApplyResources(Me.rbCarbon, "rbCarbon")
        Me.rbCarbon.Name = "rbCarbon"
        Me.rbCarbon.UseVisualStyleBackColor = True
        '
        'rbDryWeight
        '
        resources.ApplyResources(Me.rbDryWeight, "rbDryWeight")
        Me.rbDryWeight.Name = "rbDryWeight"
        Me.rbDryWeight.UseVisualStyleBackColor = True
        '
        'm_gbTimeUnits
        '
        Me.m_gbTimeUnits.Controls.Add(Me.txbTimeOther)
        Me.m_gbTimeUnits.Controls.Add(Me.rbTimeOther)
        Me.m_gbTimeUnits.Controls.Add(Me.rbDay)
        Me.m_gbTimeUnits.Controls.Add(Me.rbYear)
        resources.ApplyResources(Me.m_gbTimeUnits, "m_gbTimeUnits")
        Me.m_gbTimeUnits.Name = "m_gbTimeUnits"
        Me.m_gbTimeUnits.TabStop = False
        '
        'txbTimeOther
        '
        resources.ApplyResources(Me.txbTimeOther, "txbTimeOther")
        Me.txbTimeOther.Name = "txbTimeOther"
        '
        'rbTimeOther
        '
        resources.ApplyResources(Me.rbTimeOther, "rbTimeOther")
        Me.rbTimeOther.Name = "rbTimeOther"
        Me.rbTimeOther.UseVisualStyleBackColor = True
        '
        'rbDay
        '
        resources.ApplyResources(Me.rbDay, "rbDay")
        Me.rbDay.Name = "rbDay"
        Me.rbDay.UseVisualStyleBackColor = True
        '
        'rbYear
        '
        resources.ApplyResources(Me.rbYear, "rbYear")
        Me.rbYear.Checked = True
        Me.rbYear.Name = "rbYear"
        Me.rbYear.TabStop = True
        Me.rbYear.UseVisualStyleBackColor = True
        '
        'm_gbMonetaryUnits
        '
        Me.m_gbMonetaryUnits.Controls.Add(Me.m_lblMonetaryUnit)
        Me.m_gbMonetaryUnits.Controls.Add(Me.m_cmbMonetaryUnit)
        resources.ApplyResources(Me.m_gbMonetaryUnits, "m_gbMonetaryUnits")
        Me.m_gbMonetaryUnits.Name = "m_gbMonetaryUnits"
        Me.m_gbMonetaryUnits.TabStop = False
        '
        'm_lblMonetaryUnit
        '
        resources.ApplyResources(Me.m_lblMonetaryUnit, "m_lblMonetaryUnit")
        Me.m_lblMonetaryUnit.Name = "m_lblMonetaryUnit"
        '
        'm_cmbMonetaryUnit
        '
        resources.ApplyResources(Me.m_cmbMonetaryUnit, "m_cmbMonetaryUnit")
        Me.m_cmbMonetaryUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbMonetaryUnit.FormattingEnabled = True
        Me.m_cmbMonetaryUnit.Name = "m_cmbMonetaryUnit"
        Me.m_cmbMonetaryUnit.Sorted = True
        Me.m_cmbMonetaryUnit.UIContext = Nothing
        Me.m_cmbMonetaryUnit.Unit = "EUR"
        '
        'm_gbNumFormatting
        '
        Me.m_gbNumFormatting.Controls.Add(Me.m_cbGroupDigits)
        Me.m_gbNumFormatting.Controls.Add(Me.lbNumDigits)
        Me.m_gbNumFormatting.Controls.Add(Me.m_udNumDigits)
        resources.ApplyResources(Me.m_gbNumFormatting, "m_gbNumFormatting")
        Me.m_gbNumFormatting.Name = "m_gbNumFormatting"
        Me.m_gbNumFormatting.TabStop = False
        '
        'm_cbGroupDigits
        '
        resources.ApplyResources(Me.m_cbGroupDigits, "m_cbGroupDigits")
        Me.m_cbGroupDigits.Name = "m_cbGroupDigits"
        Me.m_cbGroupDigits.UseVisualStyleBackColor = True
        '
        'm_chkPSD
        '
        resources.ApplyResources(Me.m_chkPSD, "m_chkPSD")
        Me.m_chkPSD.Name = "m_chkPSD"
        Me.m_chkPSD.UseVisualStyleBackColor = True
        '
        'm_hdrExecution
        '
        Me.m_hdrExecution.CanCollapseParent = False
        Me.m_hdrExecution.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrExecution, "m_hdrExecution")
        Me.m_hdrExecution.IsCollapsed = False
        Me.m_hdrExecution.Name = "m_hdrExecution"
        '
        'm_nudNorth
        '
        Me.m_nudNorth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        resources.ApplyResources(Me.m_nudNorth, "m_nudNorth")
        Me.m_nudNorth.Name = "m_nudNorth"
        '
        'm_nudSouth
        '
        Me.m_nudSouth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        resources.ApplyResources(Me.m_nudSouth, "m_nudSouth")
        Me.m_nudSouth.Name = "m_nudSouth"
        '
        'm_nudWest
        '
        Me.m_nudWest.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        resources.ApplyResources(Me.m_nudWest, "m_nudWest")
        Me.m_nudWest.Name = "m_nudWest"
        '
        'm_nudEast
        '
        Me.m_nudEast.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        resources.ApplyResources(Me.m_nudEast, "m_nudEast")
        Me.m_nudEast.Name = "m_nudEast"
        '
        'm_lblNorth
        '
        resources.ApplyResources(Me.m_lblNorth, "m_lblNorth")
        Me.m_lblNorth.Name = "m_lblNorth"
        '
        'm_lblWest
        '
        resources.ApplyResources(Me.m_lblWest, "m_lblWest")
        Me.m_lblWest.Name = "m_lblWest"
        '
        'm_lblEast
        '
        resources.ApplyResources(Me.m_lblEast, "m_lblEast")
        Me.m_lblEast.Name = "m_lblEast"
        '
        'm_lblSouth
        '
        resources.ApplyResources(Me.m_lblSouth, "m_lblSouth")
        Me.m_lblSouth.Name = "m_lblSouth"
        '
        'm_plDescr
        '
        Me.m_plDescr.Controls.Add(Me.m_btnSearch)
        Me.m_plDescr.Controls.Add(Me.m_gcmbAreaName)
        Me.m_plDescr.Controls.Add(Me.m_lblAreaName)
        Me.m_plDescr.Controls.Add(Me.m_tbContact)
        Me.m_plDescr.Controls.Add(Me.m_tbDescription)
        Me.m_plDescr.Controls.Add(Me.m_lbAuthor)
        Me.m_plDescr.Controls.Add(Me.m_lblModel)
        Me.m_plDescr.Controls.Add(Me.m_lbScenarioName)
        Me.m_plDescr.Controls.Add(Me.m_nudEast)
        Me.m_plDescr.Controls.Add(Me.m_lbDescription)
        Me.m_plDescr.Controls.Add(Me.m_lbContact)
        Me.m_plDescr.Controls.Add(Me.m_tbName)
        Me.m_plDescr.Controls.Add(Me.m_nudSouth)
        Me.m_plDescr.Controls.Add(Me.m_tbAuthor)
        Me.m_plDescr.Controls.Add(Me.m_tbxNumYears)
        Me.m_plDescr.Controls.Add(Me.m_tbxFirstYear)
        Me.m_plDescr.Controls.Add(Me.m_nudWest)
        Me.m_plDescr.Controls.Add(Me.m_lblNoYears)
        Me.m_plDescr.Controls.Add(Me.m_tbArea)
        Me.m_plDescr.Controls.Add(Me.m_lblFirstYear)
        Me.m_plDescr.Controls.Add(Me.m_nudNorth)
        Me.m_plDescr.Controls.Add(Me.m_lblSouth)
        Me.m_plDescr.Controls.Add(Me.m_lblSouthUnit)
        Me.m_plDescr.Controls.Add(Me.m_lblEastUnit)
        Me.m_plDescr.Controls.Add(Me.m_lblNorthUnit)
        Me.m_plDescr.Controls.Add(Me.m_lblAreaUnit)
        Me.m_plDescr.Controls.Add(Me.m_lblEast)
        Me.m_plDescr.Controls.Add(Me.m_lblModelBounds)
        Me.m_plDescr.Controls.Add(Me.m_lblArea)
        Me.m_plDescr.Controls.Add(Me.m_lblWest)
        Me.m_plDescr.Controls.Add(Me.m_lblNorth)
        Me.m_plDescr.Controls.Add(Me.m_lblWestUnit)
        resources.ApplyResources(Me.m_plDescr, "m_plDescr")
        Me.m_plDescr.Name = "m_plDescr"
        '
        'm_btnSearch
        '
        resources.ApplyResources(Me.m_btnSearch, "m_btnSearch")
        Me.m_btnSearch.Name = "m_btnSearch"
        Me.m_btnSearch.UseVisualStyleBackColor = True
        '
        'm_gcmbAreaName
        '
        resources.ApplyResources(Me.m_gcmbAreaName, "m_gcmbAreaName")
        Me.m_gcmbAreaName.AutoSearch = True
        CMarineRegionsLookup2.Term = Nothing
        Me.m_gcmbAreaName.LookupEngine = CMarineRegionsLookup2
        Me.m_gcmbAreaName.Name = "m_gcmbAreaName"
        '
        'm_lblAreaName
        '
        resources.ApplyResources(Me.m_lblAreaName, "m_lblAreaName")
        Me.m_lblAreaName.Name = "m_lblAreaName"
        '
        'm_tbContact
        '
        resources.ApplyResources(Me.m_tbContact, "m_tbContact")
        Me.m_tbContact.Name = "m_tbContact"
        '
        'm_tbDescription
        '
        resources.ApplyResources(Me.m_tbDescription, "m_tbDescription")
        Me.m_tbDescription.Name = "m_tbDescription"
        '
        'm_tbxNumYears
        '
        resources.ApplyResources(Me.m_tbxNumYears, "m_tbxNumYears")
        Me.m_tbxNumYears.Name = "m_tbxNumYears"
        '
        'm_tbxFirstYear
        '
        resources.ApplyResources(Me.m_tbxFirstYear, "m_tbxFirstYear")
        Me.m_tbxFirstYear.Name = "m_tbxFirstYear"
        '
        'm_lblNoYears
        '
        resources.ApplyResources(Me.m_lblNoYears, "m_lblNoYears")
        Me.m_lblNoYears.Name = "m_lblNoYears"
        '
        'm_lblSouthUnit
        '
        resources.ApplyResources(Me.m_lblSouthUnit, "m_lblSouthUnit")
        Me.m_lblSouthUnit.Name = "m_lblSouthUnit"
        Me.m_lblSouthUnit.UIContext = Nothing
        '
        'm_lblEastUnit
        '
        resources.ApplyResources(Me.m_lblEastUnit, "m_lblEastUnit")
        Me.m_lblEastUnit.Name = "m_lblEastUnit"
        Me.m_lblEastUnit.UIContext = Nothing
        '
        'm_lblNorthUnit
        '
        resources.ApplyResources(Me.m_lblNorthUnit, "m_lblNorthUnit")
        Me.m_lblNorthUnit.Name = "m_lblNorthUnit"
        Me.m_lblNorthUnit.UIContext = Nothing
        '
        'm_lblModelBounds
        '
        resources.ApplyResources(Me.m_lblModelBounds, "m_lblModelBounds")
        Me.m_lblModelBounds.Name = "m_lblModelBounds"
        '
        'm_lblWestUnit
        '
        resources.ApplyResources(Me.m_lblWestUnit, "m_lblWestUnit")
        Me.m_lblWestUnit.Name = "m_lblWestUnit"
        Me.m_lblWestUnit.UIContext = Nothing
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.m_lblOptions)
        Me.Panel2.Controls.Add(Me.m_tlpUnits)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'm_tlpContent
        '
        resources.ApplyResources(Me.m_tlpContent, "m_tlpContent")
        Me.m_tlpContent.Controls.Add(Me.m_plExecution, 0, 4)
        Me.m_tlpContent.Controls.Add(Me.m_plDescr, 0, 0)
        Me.m_tlpContent.Controls.Add(Me.Panel2, 0, 3)
        Me.m_tlpContent.Controls.Add(Me.m_plMetadata, 0, 2)
        Me.m_tlpContent.Controls.Add(Me.m_plPublication, 0, 1)
        Me.m_tlpContent.Name = "m_tlpContent"
        '
        'm_plExecution
        '
        Me.m_plExecution.Controls.Add(Me.m_lblBiodivIndex)
        Me.m_plExecution.Controls.Add(Me.m_cbmBiodivIndex)
        Me.m_plExecution.Controls.Add(Me.m_chkIsCoupled)
        Me.m_plExecution.Controls.Add(Me.m_hdrExecution)
        Me.m_plExecution.Controls.Add(Me.m_chkPSD)
        resources.ApplyResources(Me.m_plExecution, "m_plExecution")
        Me.m_plExecution.Name = "m_plExecution"
        '
        'm_lblBiodivIndex
        '
        resources.ApplyResources(Me.m_lblBiodivIndex, "m_lblBiodivIndex")
        Me.m_lblBiodivIndex.Name = "m_lblBiodivIndex"
        '
        'm_cbmBiodivIndex
        '
        Me.m_cbmBiodivIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cbmBiodivIndex.FormattingEnabled = True
        resources.ApplyResources(Me.m_cbmBiodivIndex, "m_cbmBiodivIndex")
        Me.m_cbmBiodivIndex.Name = "m_cbmBiodivIndex"
        '
        'm_chkIsCoupled
        '
        resources.ApplyResources(Me.m_chkIsCoupled, "m_chkIsCoupled")
        Me.m_chkIsCoupled.Name = "m_chkIsCoupled"
        Me.m_chkIsCoupled.UseVisualStyleBackColor = True
        '
        'm_plMetadata
        '
        Me.m_plMetadata.Controls.Add(Me.m_cmbEcoType)
        Me.m_plMetadata.Controls.Add(Me.m_lblEcoType)
        Me.m_plMetadata.Controls.Add(Me.m_cmbCountry)
        Me.m_plMetadata.Controls.Add(Me.m_lblCountry)
        Me.m_plMetadata.Controls.Add(Me.m_hdrClassification)
        resources.ApplyResources(Me.m_plMetadata, "m_plMetadata")
        Me.m_plMetadata.Name = "m_plMetadata"
        '
        'm_cmbEcoType
        '
        Me.m_cmbEcoType.FormattingEnabled = True
        resources.ApplyResources(Me.m_cmbEcoType, "m_cmbEcoType")
        Me.m_cmbEcoType.Name = "m_cmbEcoType"
        Me.m_cmbEcoType.Sorted = True
        '
        'm_lblEcoType
        '
        resources.ApplyResources(Me.m_lblEcoType, "m_lblEcoType")
        Me.m_lblEcoType.Name = "m_lblEcoType"
        '
        'm_cmbCountry
        '
        Me.m_cmbCountry.FormattingEnabled = True
        resources.ApplyResources(Me.m_cmbCountry, "m_cmbCountry")
        Me.m_cmbCountry.Name = "m_cmbCountry"
        Me.m_cmbCountry.Sorted = True
        '
        'm_lblCountry
        '
        resources.ApplyResources(Me.m_lblCountry, "m_lblCountry")
        Me.m_lblCountry.Name = "m_lblCountry"
        '
        'm_hdrClassification
        '
        Me.m_hdrClassification.CanCollapseParent = True
        Me.m_hdrClassification.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrClassification, "m_hdrClassification")
        Me.m_hdrClassification.IsCollapsed = False
        Me.m_hdrClassification.Name = "m_hdrClassification"
        '
        'm_plPublication
        '
        Me.m_plPublication.Controls.Add(Me.m_llViewPublication)
        Me.m_plPublication.Controls.Add(Me.m_hdrPublication)
        Me.m_plPublication.Controls.Add(Me.m_tbxReference)
        Me.m_plPublication.Controls.Add(Me.m_tbxPublicationDOI)
        Me.m_plPublication.Controls.Add(Me.m_tbxPublicationURL)
        Me.m_plPublication.Controls.Add(Me.m_lblReference)
        Me.m_plPublication.Controls.Add(Me.m_lblPublicationDOI)
        Me.m_plPublication.Controls.Add(Me.m_lblPublicationURL)
        resources.ApplyResources(Me.m_plPublication, "m_plPublication")
        Me.m_plPublication.Name = "m_plPublication"
        '
        'm_llViewPublication
        '
        resources.ApplyResources(Me.m_llViewPublication, "m_llViewPublication")
        Me.m_llViewPublication.Name = "m_llViewPublication"
        Me.m_llViewPublication.TabStop = True
        '
        'm_hdrPublication
        '
        Me.m_hdrPublication.CanCollapseParent = True
        Me.m_hdrPublication.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrPublication, "m_hdrPublication")
        Me.m_hdrPublication.IsCollapsed = False
        Me.m_hdrPublication.Name = "m_hdrPublication"
        '
        'm_tbxReference
        '
        resources.ApplyResources(Me.m_tbxReference, "m_tbxReference")
        Me.m_tbxReference.Name = "m_tbxReference"
        '
        'm_tbxPublicationDOI
        '
        resources.ApplyResources(Me.m_tbxPublicationDOI, "m_tbxPublicationDOI")
        Me.m_tbxPublicationDOI.Name = "m_tbxPublicationDOI"
        '
        'm_tbxPublicationURL
        '
        resources.ApplyResources(Me.m_tbxPublicationURL, "m_tbxPublicationURL")
        Me.m_tbxPublicationURL.Name = "m_tbxPublicationURL"
        '
        'm_lblReference
        '
        resources.ApplyResources(Me.m_lblReference, "m_lblReference")
        Me.m_lblReference.Name = "m_lblReference"
        '
        'm_lblPublicationDOI
        '
        resources.ApplyResources(Me.m_lblPublicationDOI, "m_lblPublicationDOI")
        Me.m_lblPublicationDOI.Name = "m_lblPublicationDOI"
        '
        'm_lblPublicationURL
        '
        resources.ApplyResources(Me.m_lblPublicationURL, "m_lblPublicationURL")
        Me.m_lblPublicationURL.Name = "m_lblPublicationURL"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'frmModelParameters
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.m_tlpContent)
        Me.Name = "frmModelParameters"
        Me.TabText = ""
        CType(Me.m_udNumDigits, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tlpUnits.ResumeLayout(False)
        Me.m_gbCurrencyUnit.ResumeLayout(False)
        Me.m_gbCurrencyUnit.PerformLayout()
        Me.m_gbTimeUnits.ResumeLayout(False)
        Me.m_gbTimeUnits.PerformLayout()
        Me.m_gbMonetaryUnits.ResumeLayout(False)
        Me.m_gbMonetaryUnits.PerformLayout()
        Me.m_gbNumFormatting.ResumeLayout(False)
        Me.m_gbNumFormatting.PerformLayout()
        CType(Me.m_nudNorth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudSouth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudWest, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudEast, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_plDescr.ResumeLayout(False)
        Me.m_plDescr.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.m_tlpContent.ResumeLayout(False)
        Me.m_plExecution.ResumeLayout(False)
        Me.m_plExecution.PerformLayout()
        Me.m_plMetadata.ResumeLayout(False)
        Me.m_plMetadata.PerformLayout()
        Me.m_plPublication.ResumeLayout(False)
        Me.m_plPublication.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents lbNumDigits As System.Windows.Forms.Label
    Private WithEvents m_lblOptions As cEwEHeaderLabel
    Private WithEvents m_lbDescription As System.Windows.Forms.Label
    Private WithEvents m_lbScenarioName As System.Windows.Forms.Label
    Private WithEvents m_lblModel As cEwEHeaderLabel
    Private WithEvents m_tbName As System.Windows.Forms.TextBox
    Private WithEvents m_tbAuthor As System.Windows.Forms.TextBox
    Private WithEvents m_lbContact As System.Windows.Forms.Label
    Private WithEvents m_lbAuthor As System.Windows.Forms.Label
    Private WithEvents m_lblFirstYear As System.Windows.Forms.Label
    Private WithEvents m_lblArea As System.Windows.Forms.Label
    Private WithEvents m_tbArea As System.Windows.Forms.TextBox
    Private WithEvents m_lblAreaUnit As cEwEUnitLabel
    Private WithEvents m_tlpUnits As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_gbCurrencyUnit As System.Windows.Forms.GroupBox
    Private WithEvents m_lblEnergyRelated As System.Windows.Forms.Label
    Private WithEvents rbWetWeight As System.Windows.Forms.RadioButton
    Private WithEvents rbJoules As System.Windows.Forms.RadioButton
    Private WithEvents rbCalorie As System.Windows.Forms.RadioButton
    Private WithEvents rbCarbon As System.Windows.Forms.RadioButton
    Private WithEvents rbDryWeight As System.Windows.Forms.RadioButton
    Private WithEvents tbCurrencyEnergyOther As System.Windows.Forms.TextBox
    Private WithEvents rbCurrencyEnergyOther As System.Windows.Forms.RadioButton
    Private WithEvents rbNitrogen As System.Windows.Forms.RadioButton
    Private WithEvents rbPhosporus As System.Windows.Forms.RadioButton
    Private WithEvents m_lblNutrientRelated As System.Windows.Forms.Label
    Private WithEvents m_gbTimeUnits As System.Windows.Forms.GroupBox
    Private WithEvents txbTimeOther As System.Windows.Forms.TextBox
    Private WithEvents rbTimeOther As System.Windows.Forms.RadioButton
    Private WithEvents rbDay As System.Windows.Forms.RadioButton
    Private WithEvents rbYear As System.Windows.Forms.RadioButton
    Private WithEvents m_lblMonetaryUnit As System.Windows.Forms.Label
    Private WithEvents m_cmbMonetaryUnit As ScientificInterfaceShared.Controls.cMonetaryUnitComboBox
    Private WithEvents rbNutrientOther As System.Windows.Forms.RadioButton
    Private WithEvents tbCurrencyNutrientOther As System.Windows.Forms.TextBox
    Private WithEvents m_chkPSD As System.Windows.Forms.CheckBox
    Private WithEvents m_cbGroupDigits As System.Windows.Forms.CheckBox
    Private WithEvents m_gbMonetaryUnits As System.Windows.Forms.GroupBox
    Private WithEvents m_gbNumFormatting As System.Windows.Forms.GroupBox
    Private WithEvents m_hdrExecution As cEwEHeaderLabel
    Private WithEvents m_lblNorth As System.Windows.Forms.Label
    Private WithEvents m_lblWest As System.Windows.Forms.Label
    Private WithEvents m_lblEast As System.Windows.Forms.Label
    Private WithEvents m_lblSouth As System.Windows.Forms.Label
    Private WithEvents Panel2 As System.Windows.Forms.Panel
    Private WithEvents m_plDescr As System.Windows.Forms.Panel
    Private WithEvents m_plExecution As System.Windows.Forms.Panel
    Private WithEvents m_tlpContent As System.Windows.Forms.TableLayoutPanel
    Private WithEvents m_chkIsCoupled As System.Windows.Forms.CheckBox
    Private WithEvents m_udNumDigits As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudNorth As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudSouth As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudWest As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudEast As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_tbContact As System.Windows.Forms.TextBox
    Private WithEvents m_tbDescription As System.Windows.Forms.TextBox
    Private WithEvents m_tbxFirstYear As System.Windows.Forms.TextBox
    Private WithEvents m_plMetadata As System.Windows.Forms.Panel
    Private WithEvents m_cmbEcoType As System.Windows.Forms.ComboBox
    Private WithEvents m_lblEcoType As System.Windows.Forms.Label
    Private WithEvents m_cmbCountry As System.Windows.Forms.ComboBox
    Private WithEvents m_lblCountry As System.Windows.Forms.Label
    Private WithEvents m_hdrClassification As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_plPublication As System.Windows.Forms.Panel
    Private WithEvents m_hdrPublication As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_tbxPublicationURL As System.Windows.Forms.TextBox
    Private WithEvents m_lblPublicationURL As System.Windows.Forms.Label
    Private WithEvents m_tbxNumYears As System.Windows.Forms.TextBox
    Private WithEvents m_lblNoYears As System.Windows.Forms.Label
    Private WithEvents m_tbxReference As System.Windows.Forms.TextBox
    Private WithEvents m_lblReference As System.Windows.Forms.Label
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents m_tbxPublicationDOI As System.Windows.Forms.TextBox
    Private WithEvents m_lblPublicationDOI As System.Windows.Forms.Label
    Private WithEvents m_llViewPublication As System.Windows.Forms.LinkLabel
    Private WithEvents m_lblModelBounds As System.Windows.Forms.Label
    Private WithEvents m_lblBiodivIndex As Label
    Private WithEvents m_cbmBiodivIndex As ComboBox
    Private WithEvents m_lblSouthUnit As cEwEUnitLabel
    Private WithEvents m_lblEastUnit As cEwEUnitLabel
    Private WithEvents m_lblNorthUnit As cEwEUnitLabel
    Private WithEvents m_lblWestUnit As cEwEUnitLabel
    Private WithEvents m_gcmbAreaName As cGeocodeLookupComboBox
    Private WithEvents m_lblAreaName As Label
    Private WithEvents m_btnSearch As Button
End Class
