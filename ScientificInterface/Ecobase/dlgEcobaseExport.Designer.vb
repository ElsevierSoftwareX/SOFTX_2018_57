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

Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Forms

Partial Class dlgEcobaseExport
    Inherits frmEwE

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgEcobaseExport))
        Me.m_lblModelName = New System.Windows.Forms.Label()
        Me.m_tbxModelName = New System.Windows.Forms.TextBox()
        Me.m_lblDOI = New System.Windows.Forms.Label()
        Me.m_lblModelAuthor = New System.Windows.Forms.Label()
        Me.m_tbxDOI = New System.Windows.Forms.TextBox()
        Me.m_tbxModelAuthor = New System.Windows.Forms.TextBox()
        Me.m_cbConfirmDessiminate = New System.Windows.Forms.CheckBox()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_btnSubmit = New System.Windows.Forms.Button()
        Me.m_lblModelEmail = New System.Windows.Forms.Label()
        Me.m_tbxModelEmail = New System.Windows.Forms.TextBox()
        Me.m_llViewPublication = New System.Windows.Forms.LinkLabel()
        Me.m_pbModelName = New System.Windows.Forms.PictureBox()
        Me.m_pbModelAuthorEmail = New System.Windows.Forms.PictureBox()
        Me.m_pbPublication = New System.Windows.Forms.PictureBox()
        Me.m_lblModelDescription = New System.Windows.Forms.Label()
        Me.m_tbxModelDescription = New System.Windows.Forms.TextBox()
        Me.m_pbModelDescription = New System.Windows.Forms.PictureBox()
        Me.m_tbxHyperlink = New System.Windows.Forms.TextBox()
        Me.m_lblHyperlink = New System.Windows.Forms.Label()
        Me.m_pbAreaName = New System.Windows.Forms.PictureBox()
        Me.m_lblEcoType = New System.Windows.Forms.Label()
        Me.m_lblCountry = New System.Windows.Forms.Label()
        Me.m_cmbEcoType = New System.Windows.Forms.ComboBox()
        Me.m_cmbCountry = New System.Windows.Forms.ComboBox()
        Me.m_tcExport = New System.Windows.Forms.TabControl()
        Me.m_tpEcoBase = New System.Windows.Forms.TabPage()
        Me.m_rtfAuthorAgreement = New System.Windows.Forms.RichTextBox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.m_tpModel = New System.Windows.Forms.TabPage()
        Me.m_tbxModelLastYear = New System.Windows.Forms.TextBox()
        Me.m_tbxModelFirstYear = New System.Windows.Forms.TextBox()
        Me.m_lblModelLastYear = New System.Windows.Forms.Label()
        Me.m_tbxModelArea = New System.Windows.Forms.TextBox()
        Me.m_lblModelFirstYear = New System.Windows.Forms.Label()
        Me.m_lblModelArea = New System.Windows.Forms.Label()
        Me.m_cbModelIsAuthor = New System.Windows.Forms.CheckBox()
        Me.m_pbModelIsAuthor = New System.Windows.Forms.PictureBox()
        Me.m_pbArea = New System.Windows.Forms.PictureBox()
        Me.m_pbModelYear = New System.Windows.Forms.PictureBox()
        Me.m_tpObjectives = New System.Windows.Forms.TabPage()
        Me.m_cbObjectiveOtherImpactAssessment = New System.Windows.Forms.CheckBox()
        Me.m_cbObjectiveMarineProtection = New System.Windows.Forms.CheckBox()
        Me.m_cbObjectivePollution = New System.Windows.Forms.CheckBox()
        Me.m_cbObjectiveEcosystemFunctioning = New System.Windows.Forms.CheckBox()
        Me.m_cbObjectiveEnvironmentalVariability = New System.Windows.Forms.CheckBox()
        Me.m_cbObjectiveAquaculture = New System.Windows.Forms.CheckBox()
        Me.m_cbObjectiveFisheries = New System.Windows.Forms.CheckBox()
        Me.m_lblObjectives = New System.Windows.Forms.Label()
        Me.m_tbxObjectives = New System.Windows.Forms.TextBox()
        Me.m_pbOtherText = New System.Windows.Forms.PictureBox()
        Me.m_pbOtherNeeded = New System.Windows.Forms.PictureBox()
        Me.m_pbObjectives = New System.Windows.Forms.PictureBox()
        Me.m_tpClassification = New System.Windows.Forms.TabPage()
        Me.m_lblM3 = New System.Windows.Forms.Label()
        Me.m_lblDegC3 = New System.Windows.Forms.Label()
        Me.m_lblDegC2 = New System.Windows.Forms.Label()
        Me.m_lblDegC1 = New System.Windows.Forms.Label()
        Me.m_lblM2 = New System.Windows.Forms.Label()
        Me.m_lblM1 = New System.Windows.Forms.Label()
        Me.m_cbIsEntireFoodWeb = New System.Windows.Forms.CheckBox()
        Me.m_plExtent = New System.Windows.Forms.Panel()
        Me.m_nudSouth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudNorth = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudWest = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_nudEast = New ScientificInterfaceShared.Controls.cEwENumericUpDown()
        Me.m_lblNorth = New System.Windows.Forms.Label()
        Me.m_lblWest = New System.Windows.Forms.Label()
        Me.m_lblEast = New System.Windows.Forms.Label()
        Me.m_lblSouth = New System.Windows.Forms.Label()
        Me.m_lblTempMax = New System.Windows.Forms.Label()
        Me.m_lblDepthMax = New System.Windows.Forms.Label()
        Me.m_lblTempMean = New System.Windows.Forms.Label()
        Me.m_lblDepthMean = New System.Windows.Forms.Label()
        Me.m_lblTempMin = New System.Windows.Forms.Label()
        Me.m_lblDepthMin = New System.Windows.Forms.Label()
        Me.m_pbEnvVars = New System.Windows.Forms.PictureBox()
        Me.m_pbEcosystem = New System.Windows.Forms.PictureBox()
        Me.m_pbBoundingBox = New System.Windows.Forms.PictureBox()
        Me.m_tbxTempMax = New System.Windows.Forms.TextBox()
        Me.m_tbxDepthMax = New System.Windows.Forms.TextBox()
        Me.m_tbxTempMean = New System.Windows.Forms.TextBox()
        Me.m_tbxDepthMean = New System.Windows.Forms.TextBox()
        Me.m_tbxTempMin = New System.Windows.Forms.TextBox()
        Me.m_tbxDepthMin = New System.Windows.Forms.TextBox()
        Me.m_lblClassArea = New System.Windows.Forms.Label()
        Me.m_hdrEcosystem = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrClassification = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_hdrArea = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_tpPublication = New System.Windows.Forms.TabPage()
        Me.m_lblExtraInfo = New System.Windows.Forms.Label()
        Me.m_cbPubMatchesPaper = New System.Windows.Forms.CheckBox()
        Me.m_lblExplanation = New System.Windows.Forms.Label()
        Me.m_cbEcospaceUsed = New System.Windows.Forms.CheckBox()
        Me.m_cbFittedToTimeSeries = New System.Windows.Forms.CheckBox()
        Me.m_lblReference = New System.Windows.Forms.Label()
        Me.m_cbEcosimUsed = New System.Windows.Forms.CheckBox()
        Me.m_pbDifference = New System.Windows.Forms.PictureBox()
        Me.m_pbRef = New System.Windows.Forms.PictureBox()
        Me.m_tbxDifference = New System.Windows.Forms.TextBox()
        Me.m_tbxReference = New System.Windows.Forms.TextBox()
        Me.m_tpAccess = New System.Windows.Forms.TabPage()
        Me.m_hdrAccess = New ScientificInterfaceShared.Controls.cEwEHeaderLabel()
        Me.m_pbPermissionComment = New System.Windows.Forms.PictureBox()
        Me.m_lblPermissionComments = New System.Windows.Forms.Label()
        Me.m_tbxPermissionComments = New System.Windows.Forms.TextBox()
        Me.m_tpSubmission = New System.Windows.Forms.TabPage()
        Me.m_pbSubmModifications = New System.Windows.Forms.PictureBox()
        Me.m_pbSubmExistingModel = New System.Windows.Forms.PictureBox()
        Me.m_pbSubmType = New System.Windows.Forms.PictureBox()
        Me.m_tbxSubmModifications = New System.Windows.Forms.TextBox()
        Me.m_cmbSubmEcobaseModel = New System.Windows.Forms.ComboBox()
        Me.m_lblSubmModications = New System.Windows.Forms.Label()
        Me.m_rbSubmNew = New System.Windows.Forms.RadioButton()
        Me.m_rbSubmDerived = New System.Windows.Forms.RadioButton()
        Me.m_rbSubmUpdate = New System.Windows.Forms.RadioButton()
        Me.m_lblSubmExistingModel = New System.Windows.Forms.Label()
        Me.m_lblSubmInfo = New System.Windows.Forms.Label()
        Me.m_pbEcobaseAgreement = New System.Windows.Forms.PictureBox()
        Me.m_cbAuthorAgreement = New System.Windows.Forms.CheckBox()
        Me.m_wrkGetAuthorAgreement = New System.ComponentModel.BackgroundWorker()
        Me.m_wrkGetModels = New System.ComponentModel.BackgroundWorker()
        CType(Me.m_pbModelName, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbModelAuthorEmail, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbPublication, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbModelDescription, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbAreaName, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tcExport.SuspendLayout()
        Me.m_tpEcoBase.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpModel.SuspendLayout()
        CType(Me.m_pbModelIsAuthor, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbArea, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbModelYear, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpObjectives.SuspendLayout()
        CType(Me.m_pbOtherText, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbOtherNeeded, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbObjectives, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpClassification.SuspendLayout()
        Me.m_plExtent.SuspendLayout()
        CType(Me.m_nudSouth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudNorth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudWest, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_nudEast, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbEnvVars, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbEcosystem, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbBoundingBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpPublication.SuspendLayout()
        CType(Me.m_pbDifference, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbRef, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpAccess.SuspendLayout()
        CType(Me.m_pbPermissionComment, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tpSubmission.SuspendLayout()
        CType(Me.m_pbSubmModifications, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbSubmExistingModel, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbSubmType, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_pbEcobaseAgreement, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_lblModelName
        '
        resources.ApplyResources(Me.m_lblModelName, "m_lblModelName")
        Me.m_lblModelName.Name = "m_lblModelName"
        '
        'm_tbxModelName
        '
        resources.ApplyResources(Me.m_tbxModelName, "m_tbxModelName")
        Me.m_tbxModelName.Name = "m_tbxModelName"
        '
        'm_lblDOI
        '
        resources.ApplyResources(Me.m_lblDOI, "m_lblDOI")
        Me.m_lblDOI.Name = "m_lblDOI"
        '
        'm_lblModelAuthor
        '
        resources.ApplyResources(Me.m_lblModelAuthor, "m_lblModelAuthor")
        Me.m_lblModelAuthor.Name = "m_lblModelAuthor"
        '
        'm_tbxDOI
        '
        resources.ApplyResources(Me.m_tbxDOI, "m_tbxDOI")
        Me.m_tbxDOI.Name = "m_tbxDOI"
        '
        'm_tbxModelAuthor
        '
        resources.ApplyResources(Me.m_tbxModelAuthor, "m_tbxModelAuthor")
        Me.m_tbxModelAuthor.Name = "m_tbxModelAuthor"
        '
        'm_cbConfirmDessiminate
        '
        resources.ApplyResources(Me.m_cbConfirmDessiminate, "m_cbConfirmDessiminate")
        Me.m_cbConfirmDessiminate.Name = "m_cbConfirmDessiminate"
        Me.m_cbConfirmDessiminate.UseVisualStyleBackColor = True
        '
        'm_btnCancel
        '
        resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'm_btnSubmit
        '
        resources.ApplyResources(Me.m_btnSubmit, "m_btnSubmit")
        Me.m_btnSubmit.Name = "m_btnSubmit"
        Me.m_btnSubmit.UseVisualStyleBackColor = True
        '
        'm_lblModelEmail
        '
        resources.ApplyResources(Me.m_lblModelEmail, "m_lblModelEmail")
        Me.m_lblModelEmail.Name = "m_lblModelEmail"
        '
        'm_tbxModelEmail
        '
        resources.ApplyResources(Me.m_tbxModelEmail, "m_tbxModelEmail")
        Me.m_tbxModelEmail.Name = "m_tbxModelEmail"
        '
        'm_llViewPublication
        '
        resources.ApplyResources(Me.m_llViewPublication, "m_llViewPublication")
        Me.m_llViewPublication.Name = "m_llViewPublication"
        Me.m_llViewPublication.TabStop = True
        '
        'm_pbModelName
        '
        resources.ApplyResources(Me.m_pbModelName, "m_pbModelName")
        Me.m_pbModelName.Name = "m_pbModelName"
        Me.m_pbModelName.TabStop = False
        '
        'm_pbModelAuthorEmail
        '
        resources.ApplyResources(Me.m_pbModelAuthorEmail, "m_pbModelAuthorEmail")
        Me.m_pbModelAuthorEmail.Name = "m_pbModelAuthorEmail"
        Me.m_pbModelAuthorEmail.TabStop = False
        '
        'm_pbPublication
        '
        resources.ApplyResources(Me.m_pbPublication, "m_pbPublication")
        Me.m_pbPublication.Name = "m_pbPublication"
        Me.m_pbPublication.TabStop = False
        '
        'm_lblModelDescription
        '
        resources.ApplyResources(Me.m_lblModelDescription, "m_lblModelDescription")
        Me.m_lblModelDescription.Name = "m_lblModelDescription"
        '
        'm_tbxModelDescription
        '
        resources.ApplyResources(Me.m_tbxModelDescription, "m_tbxModelDescription")
        Me.m_tbxModelDescription.Name = "m_tbxModelDescription"
        '
        'm_pbModelDescription
        '
        resources.ApplyResources(Me.m_pbModelDescription, "m_pbModelDescription")
        Me.m_pbModelDescription.Name = "m_pbModelDescription"
        Me.m_pbModelDescription.TabStop = False
        '
        'm_tbxHyperlink
        '
        resources.ApplyResources(Me.m_tbxHyperlink, "m_tbxHyperlink")
        Me.m_tbxHyperlink.Name = "m_tbxHyperlink"
        '
        'm_lblHyperlink
        '
        resources.ApplyResources(Me.m_lblHyperlink, "m_lblHyperlink")
        Me.m_lblHyperlink.Name = "m_lblHyperlink"
        '
        'm_pbAreaName
        '
        resources.ApplyResources(Me.m_pbAreaName, "m_pbAreaName")
        Me.m_pbAreaName.Name = "m_pbAreaName"
        Me.m_pbAreaName.TabStop = False
        '
        'm_lblEcoType
        '
        resources.ApplyResources(Me.m_lblEcoType, "m_lblEcoType")
        Me.m_lblEcoType.Name = "m_lblEcoType"
        '
        'm_lblCountry
        '
        resources.ApplyResources(Me.m_lblCountry, "m_lblCountry")
        Me.m_lblCountry.Name = "m_lblCountry"
        '
        'm_cmbEcoType
        '
        resources.ApplyResources(Me.m_cmbEcoType, "m_cmbEcoType")
        Me.m_cmbEcoType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbEcoType.FormattingEnabled = True
        Me.m_cmbEcoType.Name = "m_cmbEcoType"
        Me.m_cmbEcoType.Sorted = True
        '
        'm_cmbCountry
        '
        resources.ApplyResources(Me.m_cmbCountry, "m_cmbCountry")
        Me.m_cmbCountry.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
        Me.m_cmbCountry.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.m_cmbCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbCountry.FormattingEnabled = True
        Me.m_cmbCountry.Name = "m_cmbCountry"
        Me.m_cmbCountry.Sorted = True
        '
        'm_tcExport
        '
        resources.ApplyResources(Me.m_tcExport, "m_tcExport")
        Me.m_tcExport.Controls.Add(Me.m_tpEcoBase)
        Me.m_tcExport.Controls.Add(Me.m_tpModel)
        Me.m_tcExport.Controls.Add(Me.m_tpObjectives)
        Me.m_tcExport.Controls.Add(Me.m_tpClassification)
        Me.m_tcExport.Controls.Add(Me.m_tpPublication)
        Me.m_tcExport.Controls.Add(Me.m_tpAccess)
        Me.m_tcExport.Controls.Add(Me.m_tpSubmission)
        Me.m_tcExport.Name = "m_tcExport"
        Me.m_tcExport.SelectedIndex = 0
        '
        'm_tpEcoBase
        '
        Me.m_tpEcoBase.Controls.Add(Me.m_rtfAuthorAgreement)
        Me.m_tpEcoBase.Controls.Add(Me.PictureBox1)
        resources.ApplyResources(Me.m_tpEcoBase, "m_tpEcoBase")
        Me.m_tpEcoBase.Name = "m_tpEcoBase"
        Me.m_tpEcoBase.UseVisualStyleBackColor = True
        '
        'm_rtfAuthorAgreement
        '
        resources.ApplyResources(Me.m_rtfAuthorAgreement, "m_rtfAuthorAgreement")
        Me.m_rtfAuthorAgreement.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.m_rtfAuthorAgreement.Name = "m_rtfAuthorAgreement"
        Me.m_rtfAuthorAgreement.ReadOnly = True
        '
        'PictureBox1
        '
        Me.PictureBox1.BackgroundImage = Global.ScientificInterface.My.Resources.Resources.EcoBase1
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = False
        '
        'm_tpModel
        '
        Me.m_tpModel.Controls.Add(Me.m_tbxModelLastYear)
        Me.m_tpModel.Controls.Add(Me.m_tbxModelFirstYear)
        Me.m_tpModel.Controls.Add(Me.m_lblModelLastYear)
        Me.m_tpModel.Controls.Add(Me.m_tbxModelArea)
        Me.m_tpModel.Controls.Add(Me.m_lblModelFirstYear)
        Me.m_tpModel.Controls.Add(Me.m_lblModelArea)
        Me.m_tpModel.Controls.Add(Me.m_cbModelIsAuthor)
        Me.m_tpModel.Controls.Add(Me.m_lblModelName)
        Me.m_tpModel.Controls.Add(Me.m_lblModelDescription)
        Me.m_tpModel.Controls.Add(Me.m_lblModelAuthor)
        Me.m_tpModel.Controls.Add(Me.m_tbxModelName)
        Me.m_tpModel.Controls.Add(Me.m_pbModelIsAuthor)
        Me.m_tpModel.Controls.Add(Me.m_pbArea)
        Me.m_tpModel.Controls.Add(Me.m_pbModelYear)
        Me.m_tpModel.Controls.Add(Me.m_pbModelName)
        Me.m_tpModel.Controls.Add(Me.m_tbxModelDescription)
        Me.m_tpModel.Controls.Add(Me.m_pbModelDescription)
        Me.m_tpModel.Controls.Add(Me.m_tbxModelAuthor)
        Me.m_tpModel.Controls.Add(Me.m_lblModelEmail)
        Me.m_tpModel.Controls.Add(Me.m_tbxModelEmail)
        Me.m_tpModel.Controls.Add(Me.m_pbModelAuthorEmail)
        resources.ApplyResources(Me.m_tpModel, "m_tpModel")
        Me.m_tpModel.Name = "m_tpModel"
        Me.m_tpModel.UseVisualStyleBackColor = True
        '
        'm_tbxModelLastYear
        '
        resources.ApplyResources(Me.m_tbxModelLastYear, "m_tbxModelLastYear")
        Me.m_tbxModelLastYear.Name = "m_tbxModelLastYear"
        '
        'm_tbxModelFirstYear
        '
        resources.ApplyResources(Me.m_tbxModelFirstYear, "m_tbxModelFirstYear")
        Me.m_tbxModelFirstYear.Name = "m_tbxModelFirstYear"
        '
        'm_lblModelLastYear
        '
        resources.ApplyResources(Me.m_lblModelLastYear, "m_lblModelLastYear")
        Me.m_lblModelLastYear.Name = "m_lblModelLastYear"
        '
        'm_tbxModelArea
        '
        resources.ApplyResources(Me.m_tbxModelArea, "m_tbxModelArea")
        Me.m_tbxModelArea.Name = "m_tbxModelArea"
        '
        'm_lblModelFirstYear
        '
        resources.ApplyResources(Me.m_lblModelFirstYear, "m_lblModelFirstYear")
        Me.m_lblModelFirstYear.Name = "m_lblModelFirstYear"
        '
        'm_lblModelArea
        '
        resources.ApplyResources(Me.m_lblModelArea, "m_lblModelArea")
        Me.m_lblModelArea.Name = "m_lblModelArea"
        '
        'm_cbModelIsAuthor
        '
        resources.ApplyResources(Me.m_cbModelIsAuthor, "m_cbModelIsAuthor")
        Me.m_cbModelIsAuthor.Name = "m_cbModelIsAuthor"
        Me.m_cbModelIsAuthor.UseVisualStyleBackColor = True
        '
        'm_pbModelIsAuthor
        '
        resources.ApplyResources(Me.m_pbModelIsAuthor, "m_pbModelIsAuthor")
        Me.m_pbModelIsAuthor.Name = "m_pbModelIsAuthor"
        Me.m_pbModelIsAuthor.TabStop = False
        '
        'm_pbArea
        '
        resources.ApplyResources(Me.m_pbArea, "m_pbArea")
        Me.m_pbArea.Name = "m_pbArea"
        Me.m_pbArea.TabStop = False
        '
        'm_pbModelYear
        '
        resources.ApplyResources(Me.m_pbModelYear, "m_pbModelYear")
        Me.m_pbModelYear.Name = "m_pbModelYear"
        Me.m_pbModelYear.TabStop = False
        '
        'm_tpObjectives
        '
        Me.m_tpObjectives.Controls.Add(Me.m_cbObjectiveOtherImpactAssessment)
        Me.m_tpObjectives.Controls.Add(Me.m_cbObjectiveMarineProtection)
        Me.m_tpObjectives.Controls.Add(Me.m_cbObjectivePollution)
        Me.m_tpObjectives.Controls.Add(Me.m_cbObjectiveEcosystemFunctioning)
        Me.m_tpObjectives.Controls.Add(Me.m_cbObjectiveEnvironmentalVariability)
        Me.m_tpObjectives.Controls.Add(Me.m_cbObjectiveAquaculture)
        Me.m_tpObjectives.Controls.Add(Me.m_cbObjectiveFisheries)
        Me.m_tpObjectives.Controls.Add(Me.m_lblObjectives)
        Me.m_tpObjectives.Controls.Add(Me.m_tbxObjectives)
        Me.m_tpObjectives.Controls.Add(Me.m_pbOtherText)
        Me.m_tpObjectives.Controls.Add(Me.m_pbOtherNeeded)
        Me.m_tpObjectives.Controls.Add(Me.m_pbObjectives)
        resources.ApplyResources(Me.m_tpObjectives, "m_tpObjectives")
        Me.m_tpObjectives.Name = "m_tpObjectives"
        Me.m_tpObjectives.UseVisualStyleBackColor = True
        '
        'm_cbObjectiveOtherImpactAssessment
        '
        resources.ApplyResources(Me.m_cbObjectiveOtherImpactAssessment, "m_cbObjectiveOtherImpactAssessment")
        Me.m_cbObjectiveOtherImpactAssessment.Name = "m_cbObjectiveOtherImpactAssessment"
        Me.m_cbObjectiveOtherImpactAssessment.UseVisualStyleBackColor = True
        '
        'm_cbObjectiveMarineProtection
        '
        resources.ApplyResources(Me.m_cbObjectiveMarineProtection, "m_cbObjectiveMarineProtection")
        Me.m_cbObjectiveMarineProtection.Name = "m_cbObjectiveMarineProtection"
        Me.m_cbObjectiveMarineProtection.UseVisualStyleBackColor = True
        '
        'm_cbObjectivePollution
        '
        resources.ApplyResources(Me.m_cbObjectivePollution, "m_cbObjectivePollution")
        Me.m_cbObjectivePollution.Name = "m_cbObjectivePollution"
        Me.m_cbObjectivePollution.UseVisualStyleBackColor = True
        '
        'm_cbObjectiveEcosystemFunctioning
        '
        resources.ApplyResources(Me.m_cbObjectiveEcosystemFunctioning, "m_cbObjectiveEcosystemFunctioning")
        Me.m_cbObjectiveEcosystemFunctioning.Name = "m_cbObjectiveEcosystemFunctioning"
        Me.m_cbObjectiveEcosystemFunctioning.UseVisualStyleBackColor = True
        '
        'm_cbObjectiveEnvironmentalVariability
        '
        resources.ApplyResources(Me.m_cbObjectiveEnvironmentalVariability, "m_cbObjectiveEnvironmentalVariability")
        Me.m_cbObjectiveEnvironmentalVariability.Name = "m_cbObjectiveEnvironmentalVariability"
        Me.m_cbObjectiveEnvironmentalVariability.UseVisualStyleBackColor = True
        '
        'm_cbObjectiveAquaculture
        '
        resources.ApplyResources(Me.m_cbObjectiveAquaculture, "m_cbObjectiveAquaculture")
        Me.m_cbObjectiveAquaculture.Name = "m_cbObjectiveAquaculture"
        Me.m_cbObjectiveAquaculture.UseVisualStyleBackColor = True
        '
        'm_cbObjectiveFisheries
        '
        resources.ApplyResources(Me.m_cbObjectiveFisheries, "m_cbObjectiveFisheries")
        Me.m_cbObjectiveFisheries.Name = "m_cbObjectiveFisheries"
        Me.m_cbObjectiveFisheries.UseVisualStyleBackColor = True
        '
        'm_lblObjectives
        '
        resources.ApplyResources(Me.m_lblObjectives, "m_lblObjectives")
        Me.m_lblObjectives.Name = "m_lblObjectives"
        '
        'm_tbxObjectives
        '
        resources.ApplyResources(Me.m_tbxObjectives, "m_tbxObjectives")
        Me.m_tbxObjectives.Name = "m_tbxObjectives"
        '
        'm_pbOtherText
        '
        resources.ApplyResources(Me.m_pbOtherText, "m_pbOtherText")
        Me.m_pbOtherText.Name = "m_pbOtherText"
        Me.m_pbOtherText.TabStop = False
        '
        'm_pbOtherNeeded
        '
        resources.ApplyResources(Me.m_pbOtherNeeded, "m_pbOtherNeeded")
        Me.m_pbOtherNeeded.Name = "m_pbOtherNeeded"
        Me.m_pbOtherNeeded.TabStop = False
        '
        'm_pbObjectives
        '
        resources.ApplyResources(Me.m_pbObjectives, "m_pbObjectives")
        Me.m_pbObjectives.Name = "m_pbObjectives"
        Me.m_pbObjectives.TabStop = False
        '
        'm_tpClassification
        '
        Me.m_tpClassification.Controls.Add(Me.m_lblM3)
        Me.m_tpClassification.Controls.Add(Me.m_lblDegC3)
        Me.m_tpClassification.Controls.Add(Me.m_lblDegC2)
        Me.m_tpClassification.Controls.Add(Me.m_lblDegC1)
        Me.m_tpClassification.Controls.Add(Me.m_lblM2)
        Me.m_tpClassification.Controls.Add(Me.m_lblM1)
        Me.m_tpClassification.Controls.Add(Me.m_cbIsEntireFoodWeb)
        Me.m_tpClassification.Controls.Add(Me.m_plExtent)
        Me.m_tpClassification.Controls.Add(Me.m_lblTempMax)
        Me.m_tpClassification.Controls.Add(Me.m_lblDepthMax)
        Me.m_tpClassification.Controls.Add(Me.m_lblTempMean)
        Me.m_tpClassification.Controls.Add(Me.m_lblDepthMean)
        Me.m_tpClassification.Controls.Add(Me.m_lblTempMin)
        Me.m_tpClassification.Controls.Add(Me.m_lblDepthMin)
        Me.m_tpClassification.Controls.Add(Me.m_lblEcoType)
        Me.m_tpClassification.Controls.Add(Me.m_pbEnvVars)
        Me.m_tpClassification.Controls.Add(Me.m_pbEcosystem)
        Me.m_tpClassification.Controls.Add(Me.m_pbBoundingBox)
        Me.m_tpClassification.Controls.Add(Me.m_pbAreaName)
        Me.m_tpClassification.Controls.Add(Me.m_tbxTempMax)
        Me.m_tpClassification.Controls.Add(Me.m_tbxDepthMax)
        Me.m_tpClassification.Controls.Add(Me.m_tbxTempMean)
        Me.m_tpClassification.Controls.Add(Me.m_tbxDepthMean)
        Me.m_tpClassification.Controls.Add(Me.m_tbxTempMin)
        Me.m_tpClassification.Controls.Add(Me.m_tbxDepthMin)
        Me.m_tpClassification.Controls.Add(Me.m_cmbCountry)
        Me.m_tpClassification.Controls.Add(Me.m_cmbEcoType)
        Me.m_tpClassification.Controls.Add(Me.m_lblClassArea)
        Me.m_tpClassification.Controls.Add(Me.m_lblCountry)
        Me.m_tpClassification.Controls.Add(Me.m_hdrEcosystem)
        Me.m_tpClassification.Controls.Add(Me.m_hdrClassification)
        Me.m_tpClassification.Controls.Add(Me.m_hdrArea)
        resources.ApplyResources(Me.m_tpClassification, "m_tpClassification")
        Me.m_tpClassification.Name = "m_tpClassification"
        Me.m_tpClassification.UseVisualStyleBackColor = True
        '
        'm_lblM3
        '
        resources.ApplyResources(Me.m_lblM3, "m_lblM3")
        Me.m_lblM3.Name = "m_lblM3"
        '
        'm_lblDegC3
        '
        resources.ApplyResources(Me.m_lblDegC3, "m_lblDegC3")
        Me.m_lblDegC3.Name = "m_lblDegC3"
        '
        'm_lblDegC2
        '
        resources.ApplyResources(Me.m_lblDegC2, "m_lblDegC2")
        Me.m_lblDegC2.Name = "m_lblDegC2"
        '
        'm_lblDegC1
        '
        resources.ApplyResources(Me.m_lblDegC1, "m_lblDegC1")
        Me.m_lblDegC1.Name = "m_lblDegC1"
        '
        'm_lblM2
        '
        resources.ApplyResources(Me.m_lblM2, "m_lblM2")
        Me.m_lblM2.Name = "m_lblM2"
        '
        'm_lblM1
        '
        resources.ApplyResources(Me.m_lblM1, "m_lblM1")
        Me.m_lblM1.Name = "m_lblM1"
        '
        'm_cbIsEntireFoodWeb
        '
        resources.ApplyResources(Me.m_cbIsEntireFoodWeb, "m_cbIsEntireFoodWeb")
        Me.m_cbIsEntireFoodWeb.Name = "m_cbIsEntireFoodWeb"
        Me.m_cbIsEntireFoodWeb.UseVisualStyleBackColor = True
        '
        'm_plExtent
        '
        resources.ApplyResources(Me.m_plExtent, "m_plExtent")
        Me.m_plExtent.Controls.Add(Me.m_nudSouth)
        Me.m_plExtent.Controls.Add(Me.m_nudNorth)
        Me.m_plExtent.Controls.Add(Me.m_nudWest)
        Me.m_plExtent.Controls.Add(Me.m_nudEast)
        Me.m_plExtent.Controls.Add(Me.m_lblNorth)
        Me.m_plExtent.Controls.Add(Me.m_lblWest)
        Me.m_plExtent.Controls.Add(Me.m_lblEast)
        Me.m_plExtent.Controls.Add(Me.m_lblSouth)
        Me.m_plExtent.Name = "m_plExtent"
        '
        'm_nudSouth
        '
        resources.ApplyResources(Me.m_nudSouth, "m_nudSouth")
        Me.m_nudSouth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudSouth.Name = "m_nudSouth"
        '
        'm_nudNorth
        '
        resources.ApplyResources(Me.m_nudNorth, "m_nudNorth")
        Me.m_nudNorth.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudNorth.Name = "m_nudNorth"
        '
        'm_nudWest
        '
        resources.ApplyResources(Me.m_nudWest, "m_nudWest")
        Me.m_nudWest.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
        Me.m_nudWest.Name = "m_nudWest"
        '
        'm_nudEast
        '
        resources.ApplyResources(Me.m_nudEast, "m_nudEast")
        Me.m_nudEast.InterceptMouseWheel = ScientificInterfaceShared.Controls.cEwENumericUpDown.eInterceptMouseWheelType.WhenMouseOver
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
        'm_lblTempMax
        '
        resources.ApplyResources(Me.m_lblTempMax, "m_lblTempMax")
        Me.m_lblTempMax.Name = "m_lblTempMax"
        '
        'm_lblDepthMax
        '
        resources.ApplyResources(Me.m_lblDepthMax, "m_lblDepthMax")
        Me.m_lblDepthMax.Name = "m_lblDepthMax"
        '
        'm_lblTempMean
        '
        resources.ApplyResources(Me.m_lblTempMean, "m_lblTempMean")
        Me.m_lblTempMean.Name = "m_lblTempMean"
        '
        'm_lblDepthMean
        '
        resources.ApplyResources(Me.m_lblDepthMean, "m_lblDepthMean")
        Me.m_lblDepthMean.Name = "m_lblDepthMean"
        '
        'm_lblTempMin
        '
        resources.ApplyResources(Me.m_lblTempMin, "m_lblTempMin")
        Me.m_lblTempMin.Name = "m_lblTempMin"
        '
        'm_lblDepthMin
        '
        resources.ApplyResources(Me.m_lblDepthMin, "m_lblDepthMin")
        Me.m_lblDepthMin.Name = "m_lblDepthMin"
        '
        'm_pbEnvVars
        '
        resources.ApplyResources(Me.m_pbEnvVars, "m_pbEnvVars")
        Me.m_pbEnvVars.Name = "m_pbEnvVars"
        Me.m_pbEnvVars.TabStop = False
        '
        'm_pbEcosystem
        '
        resources.ApplyResources(Me.m_pbEcosystem, "m_pbEcosystem")
        Me.m_pbEcosystem.Name = "m_pbEcosystem"
        Me.m_pbEcosystem.TabStop = False
        '
        'm_pbBoundingBox
        '
        resources.ApplyResources(Me.m_pbBoundingBox, "m_pbBoundingBox")
        Me.m_pbBoundingBox.Name = "m_pbBoundingBox"
        Me.m_pbBoundingBox.TabStop = False
        '
        'm_tbxTempMax
        '
        resources.ApplyResources(Me.m_tbxTempMax, "m_tbxTempMax")
        Me.m_tbxTempMax.Name = "m_tbxTempMax"
        '
        'm_tbxDepthMax
        '
        resources.ApplyResources(Me.m_tbxDepthMax, "m_tbxDepthMax")
        Me.m_tbxDepthMax.Name = "m_tbxDepthMax"
        '
        'm_tbxTempMean
        '
        resources.ApplyResources(Me.m_tbxTempMean, "m_tbxTempMean")
        Me.m_tbxTempMean.Name = "m_tbxTempMean"
        '
        'm_tbxDepthMean
        '
        resources.ApplyResources(Me.m_tbxDepthMean, "m_tbxDepthMean")
        Me.m_tbxDepthMean.Name = "m_tbxDepthMean"
        '
        'm_tbxTempMin
        '
        resources.ApplyResources(Me.m_tbxTempMin, "m_tbxTempMin")
        Me.m_tbxTempMin.Name = "m_tbxTempMin"
        '
        'm_tbxDepthMin
        '
        resources.ApplyResources(Me.m_tbxDepthMin, "m_tbxDepthMin")
        Me.m_tbxDepthMin.Name = "m_tbxDepthMin"
        '
        'm_lblClassArea
        '
        resources.ApplyResources(Me.m_lblClassArea, "m_lblClassArea")
        Me.m_lblClassArea.Name = "m_lblClassArea"
        '
        'm_hdrEcosystem
        '
        resources.ApplyResources(Me.m_hdrEcosystem, "m_hdrEcosystem")
        Me.m_hdrEcosystem.CanCollapseParent = False
        Me.m_hdrEcosystem.CollapsedParentHeight = 0
        Me.m_hdrEcosystem.IsCollapsed = False
        Me.m_hdrEcosystem.Name = "m_hdrEcosystem"
        '
        'm_hdrClassification
        '
        Me.m_hdrClassification.CanCollapseParent = False
        Me.m_hdrClassification.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrClassification, "m_hdrClassification")
        Me.m_hdrClassification.IsCollapsed = False
        Me.m_hdrClassification.Name = "m_hdrClassification"
        '
        'm_hdrArea
        '
        Me.m_hdrArea.CanCollapseParent = False
        Me.m_hdrArea.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrArea, "m_hdrArea")
        Me.m_hdrArea.IsCollapsed = False
        Me.m_hdrArea.Name = "m_hdrArea"
        '
        'm_tpPublication
        '
        Me.m_tpPublication.Controls.Add(Me.m_lblExtraInfo)
        Me.m_tpPublication.Controls.Add(Me.m_cbPubMatchesPaper)
        Me.m_tpPublication.Controls.Add(Me.m_lblExplanation)
        Me.m_tpPublication.Controls.Add(Me.m_cbEcospaceUsed)
        Me.m_tpPublication.Controls.Add(Me.m_cbFittedToTimeSeries)
        Me.m_tpPublication.Controls.Add(Me.m_lblReference)
        Me.m_tpPublication.Controls.Add(Me.m_cbEcosimUsed)
        Me.m_tpPublication.Controls.Add(Me.m_lblHyperlink)
        Me.m_tpPublication.Controls.Add(Me.m_tbxHyperlink)
        Me.m_tpPublication.Controls.Add(Me.m_pbDifference)
        Me.m_tpPublication.Controls.Add(Me.m_pbRef)
        Me.m_tpPublication.Controls.Add(Me.m_pbPublication)
        Me.m_tpPublication.Controls.Add(Me.m_lblDOI)
        Me.m_tpPublication.Controls.Add(Me.m_llViewPublication)
        Me.m_tpPublication.Controls.Add(Me.m_tbxDifference)
        Me.m_tpPublication.Controls.Add(Me.m_tbxReference)
        Me.m_tpPublication.Controls.Add(Me.m_tbxDOI)
        resources.ApplyResources(Me.m_tpPublication, "m_tpPublication")
        Me.m_tpPublication.Name = "m_tpPublication"
        Me.m_tpPublication.UseVisualStyleBackColor = True
        '
        'm_lblExtraInfo
        '
        resources.ApplyResources(Me.m_lblExtraInfo, "m_lblExtraInfo")
        Me.m_lblExtraInfo.Name = "m_lblExtraInfo"
        '
        'm_cbPubMatchesPaper
        '
        resources.ApplyResources(Me.m_cbPubMatchesPaper, "m_cbPubMatchesPaper")
        Me.m_cbPubMatchesPaper.Name = "m_cbPubMatchesPaper"
        Me.m_cbPubMatchesPaper.UseVisualStyleBackColor = True
        '
        'm_lblExplanation
        '
        resources.ApplyResources(Me.m_lblExplanation, "m_lblExplanation")
        Me.m_lblExplanation.Name = "m_lblExplanation"
        '
        'm_cbEcospaceUsed
        '
        resources.ApplyResources(Me.m_cbEcospaceUsed, "m_cbEcospaceUsed")
        Me.m_cbEcospaceUsed.Name = "m_cbEcospaceUsed"
        Me.m_cbEcospaceUsed.UseVisualStyleBackColor = True
        '
        'm_cbFittedToTimeSeries
        '
        resources.ApplyResources(Me.m_cbFittedToTimeSeries, "m_cbFittedToTimeSeries")
        Me.m_cbFittedToTimeSeries.Name = "m_cbFittedToTimeSeries"
        Me.m_cbFittedToTimeSeries.UseVisualStyleBackColor = True
        '
        'm_lblReference
        '
        resources.ApplyResources(Me.m_lblReference, "m_lblReference")
        Me.m_lblReference.Name = "m_lblReference"
        '
        'm_cbEcosimUsed
        '
        resources.ApplyResources(Me.m_cbEcosimUsed, "m_cbEcosimUsed")
        Me.m_cbEcosimUsed.Name = "m_cbEcosimUsed"
        Me.m_cbEcosimUsed.UseVisualStyleBackColor = True
        '
        'm_pbDifference
        '
        resources.ApplyResources(Me.m_pbDifference, "m_pbDifference")
        Me.m_pbDifference.Name = "m_pbDifference"
        Me.m_pbDifference.TabStop = False
        '
        'm_pbRef
        '
        resources.ApplyResources(Me.m_pbRef, "m_pbRef")
        Me.m_pbRef.Name = "m_pbRef"
        Me.m_pbRef.TabStop = False
        '
        'm_tbxDifference
        '
        resources.ApplyResources(Me.m_tbxDifference, "m_tbxDifference")
        Me.m_tbxDifference.Name = "m_tbxDifference"
        '
        'm_tbxReference
        '
        resources.ApplyResources(Me.m_tbxReference, "m_tbxReference")
        Me.m_tbxReference.Name = "m_tbxReference"
        '
        'm_tpAccess
        '
        Me.m_tpAccess.Controls.Add(Me.m_hdrAccess)
        Me.m_tpAccess.Controls.Add(Me.m_pbPermissionComment)
        Me.m_tpAccess.Controls.Add(Me.m_lblPermissionComments)
        Me.m_tpAccess.Controls.Add(Me.m_tbxPermissionComments)
        Me.m_tpAccess.Controls.Add(Me.m_cbConfirmDessiminate)
        resources.ApplyResources(Me.m_tpAccess, "m_tpAccess")
        Me.m_tpAccess.Name = "m_tpAccess"
        Me.m_tpAccess.UseVisualStyleBackColor = True
        '
        'm_hdrAccess
        '
        Me.m_hdrAccess.CanCollapseParent = False
        Me.m_hdrAccess.CollapsedParentHeight = 0
        resources.ApplyResources(Me.m_hdrAccess, "m_hdrAccess")
        Me.m_hdrAccess.IsCollapsed = False
        Me.m_hdrAccess.Name = "m_hdrAccess"
        '
        'm_pbPermissionComment
        '
        resources.ApplyResources(Me.m_pbPermissionComment, "m_pbPermissionComment")
        Me.m_pbPermissionComment.Name = "m_pbPermissionComment"
        Me.m_pbPermissionComment.TabStop = False
        '
        'm_lblPermissionComments
        '
        resources.ApplyResources(Me.m_lblPermissionComments, "m_lblPermissionComments")
        Me.m_lblPermissionComments.Name = "m_lblPermissionComments"
        '
        'm_tbxPermissionComments
        '
        resources.ApplyResources(Me.m_tbxPermissionComments, "m_tbxPermissionComments")
        Me.m_tbxPermissionComments.Name = "m_tbxPermissionComments"
        '
        'm_tpSubmission
        '
        Me.m_tpSubmission.Controls.Add(Me.m_pbSubmModifications)
        Me.m_tpSubmission.Controls.Add(Me.m_pbSubmExistingModel)
        Me.m_tpSubmission.Controls.Add(Me.m_pbSubmType)
        Me.m_tpSubmission.Controls.Add(Me.m_tbxSubmModifications)
        Me.m_tpSubmission.Controls.Add(Me.m_cmbSubmEcobaseModel)
        Me.m_tpSubmission.Controls.Add(Me.m_lblSubmModications)
        Me.m_tpSubmission.Controls.Add(Me.m_rbSubmNew)
        Me.m_tpSubmission.Controls.Add(Me.m_rbSubmDerived)
        Me.m_tpSubmission.Controls.Add(Me.m_rbSubmUpdate)
        Me.m_tpSubmission.Controls.Add(Me.m_lblSubmExistingModel)
        Me.m_tpSubmission.Controls.Add(Me.m_lblSubmInfo)
        resources.ApplyResources(Me.m_tpSubmission, "m_tpSubmission")
        Me.m_tpSubmission.Name = "m_tpSubmission"
        Me.m_tpSubmission.UseVisualStyleBackColor = True
        '
        'm_pbSubmModifications
        '
        resources.ApplyResources(Me.m_pbSubmModifications, "m_pbSubmModifications")
        Me.m_pbSubmModifications.Name = "m_pbSubmModifications"
        Me.m_pbSubmModifications.TabStop = False
        '
        'm_pbSubmExistingModel
        '
        resources.ApplyResources(Me.m_pbSubmExistingModel, "m_pbSubmExistingModel")
        Me.m_pbSubmExistingModel.Name = "m_pbSubmExistingModel"
        Me.m_pbSubmExistingModel.TabStop = False
        '
        'm_pbSubmType
        '
        resources.ApplyResources(Me.m_pbSubmType, "m_pbSubmType")
        Me.m_pbSubmType.Name = "m_pbSubmType"
        Me.m_pbSubmType.TabStop = False
        '
        'm_tbxSubmModifications
        '
        resources.ApplyResources(Me.m_tbxSubmModifications, "m_tbxSubmModifications")
        Me.m_tbxSubmModifications.Name = "m_tbxSubmModifications"
        '
        'm_cmbSubmEcobaseModel
        '
        resources.ApplyResources(Me.m_cmbSubmEcobaseModel, "m_cmbSubmEcobaseModel")
        Me.m_cmbSubmEcobaseModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.m_cmbSubmEcobaseModel.FormattingEnabled = True
        Me.m_cmbSubmEcobaseModel.Name = "m_cmbSubmEcobaseModel"
        '
        'm_lblSubmModications
        '
        resources.ApplyResources(Me.m_lblSubmModications, "m_lblSubmModications")
        Me.m_lblSubmModications.Name = "m_lblSubmModications"
        '
        'm_rbSubmNew
        '
        resources.ApplyResources(Me.m_rbSubmNew, "m_rbSubmNew")
        Me.m_rbSubmNew.Name = "m_rbSubmNew"
        Me.m_rbSubmNew.UseVisualStyleBackColor = True
        '
        'm_rbSubmDerived
        '
        resources.ApplyResources(Me.m_rbSubmDerived, "m_rbSubmDerived")
        Me.m_rbSubmDerived.Name = "m_rbSubmDerived"
        Me.m_rbSubmDerived.UseVisualStyleBackColor = True
        '
        'm_rbSubmUpdate
        '
        resources.ApplyResources(Me.m_rbSubmUpdate, "m_rbSubmUpdate")
        Me.m_rbSubmUpdate.Name = "m_rbSubmUpdate"
        Me.m_rbSubmUpdate.UseVisualStyleBackColor = True
        '
        'm_lblSubmExistingModel
        '
        resources.ApplyResources(Me.m_lblSubmExistingModel, "m_lblSubmExistingModel")
        Me.m_lblSubmExistingModel.Name = "m_lblSubmExistingModel"
        '
        'm_lblSubmInfo
        '
        resources.ApplyResources(Me.m_lblSubmInfo, "m_lblSubmInfo")
        Me.m_lblSubmInfo.Name = "m_lblSubmInfo"
        '
        'm_pbEcobaseAgreement
        '
        resources.ApplyResources(Me.m_pbEcobaseAgreement, "m_pbEcobaseAgreement")
        Me.m_pbEcobaseAgreement.Name = "m_pbEcobaseAgreement"
        Me.m_pbEcobaseAgreement.TabStop = False
        '
        'm_cbAuthorAgreement
        '
        resources.ApplyResources(Me.m_cbAuthorAgreement, "m_cbAuthorAgreement")
        Me.m_cbAuthorAgreement.Name = "m_cbAuthorAgreement"
        Me.m_cbAuthorAgreement.UseVisualStyleBackColor = True
        '
        'm_wrkGetAuthorAgreement
        '
        '
        'm_wrkGetModels
        '
        '
        'dlgEcobaseExport
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ControlBox = False
        Me.Controls.Add(Me.m_pbEcobaseAgreement)
        Me.Controls.Add(Me.m_tcExport)
        Me.Controls.Add(Me.m_btnSubmit)
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_cbAuthorAgreement)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgEcobaseExport"
        Me.ShowInTaskbar = False
        Me.TabText = ""
        CType(Me.m_pbModelName, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbModelAuthorEmail, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbPublication, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbModelDescription, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbAreaName, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tcExport.ResumeLayout(False)
        Me.m_tpEcoBase.ResumeLayout(False)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpModel.ResumeLayout(False)
        Me.m_tpModel.PerformLayout()
        CType(Me.m_pbModelIsAuthor, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbArea, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbModelYear, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpObjectives.ResumeLayout(False)
        Me.m_tpObjectives.PerformLayout()
        CType(Me.m_pbOtherText, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbOtherNeeded, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbObjectives, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpClassification.ResumeLayout(False)
        Me.m_tpClassification.PerformLayout()
        Me.m_plExtent.ResumeLayout(False)
        Me.m_plExtent.PerformLayout()
        CType(Me.m_nudSouth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudNorth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudWest, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_nudEast, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbEnvVars, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbEcosystem, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbBoundingBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpPublication.ResumeLayout(False)
        Me.m_tpPublication.PerformLayout()
        CType(Me.m_pbDifference, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbRef, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpAccess.ResumeLayout(False)
        Me.m_tpAccess.PerformLayout()
        CType(Me.m_pbPermissionComment, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tpSubmission.ResumeLayout(False)
        Me.m_tpSubmission.PerformLayout()
        CType(Me.m_pbSubmModifications, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbSubmExistingModel, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbSubmType, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_pbEcobaseAgreement, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents m_lblModelName As System.Windows.Forms.Label
    Private WithEvents m_tbxModelName As System.Windows.Forms.TextBox
    Private WithEvents m_lblDOI As System.Windows.Forms.Label
    Private WithEvents m_lblModelAuthor As System.Windows.Forms.Label
    Private WithEvents m_tbxDOI As System.Windows.Forms.TextBox
    Private WithEvents m_tbxModelAuthor As System.Windows.Forms.TextBox
    Private WithEvents m_cbConfirmDessiminate As System.Windows.Forms.CheckBox
    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_btnSubmit As System.Windows.Forms.Button
    Private WithEvents m_lblModelEmail As System.Windows.Forms.Label
    Private WithEvents m_tbxModelEmail As System.Windows.Forms.TextBox
    Private WithEvents m_llViewPublication As System.Windows.Forms.LinkLabel
    Private WithEvents m_pbModelName As System.Windows.Forms.PictureBox
    Private WithEvents m_pbModelAuthorEmail As System.Windows.Forms.PictureBox
    Private WithEvents m_pbPublication As System.Windows.Forms.PictureBox
    Private WithEvents m_lblModelDescription As System.Windows.Forms.Label
    Private WithEvents m_tbxModelDescription As System.Windows.Forms.TextBox
    Private WithEvents m_pbModelDescription As System.Windows.Forms.PictureBox
    Private WithEvents m_tbxHyperlink As System.Windows.Forms.TextBox
    Private WithEvents m_lblHyperlink As System.Windows.Forms.Label
    Private WithEvents m_pbAreaName As System.Windows.Forms.PictureBox
    Private WithEvents m_lblEcoType As System.Windows.Forms.Label
    Private WithEvents m_lblCountry As System.Windows.Forms.Label
    Private WithEvents m_cmbEcoType As System.Windows.Forms.ComboBox
    Private WithEvents m_cmbCountry As System.Windows.Forms.ComboBox
    Private WithEvents m_tcExport As System.Windows.Forms.TabControl
    Private WithEvents m_tpModel As System.Windows.Forms.TabPage
    Private WithEvents m_tpClassification As System.Windows.Forms.TabPage
    Private WithEvents m_tpPublication As System.Windows.Forms.TabPage
    Private WithEvents m_tpAccess As System.Windows.Forms.TabPage
    Private WithEvents m_cbModelIsAuthor As System.Windows.Forms.CheckBox
    Private WithEvents m_lblReference As System.Windows.Forms.Label
    Private WithEvents m_tbxReference As System.Windows.Forms.TextBox
    Private WithEvents m_hdrArea As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_nudEast As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudSouth As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_nudWest As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_hdrClassification As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_nudNorth As ScientificInterfaceShared.Controls.cEwENumericUpDown
    Private WithEvents m_lblSouth As System.Windows.Forms.Label
    Private WithEvents m_lblEast As System.Windows.Forms.Label
    Private WithEvents m_lblWest As System.Windows.Forms.Label
    Private WithEvents m_lblNorth As System.Windows.Forms.Label
    Private WithEvents m_pbModelIsAuthor As System.Windows.Forms.PictureBox
    Private WithEvents m_pbRef As System.Windows.Forms.PictureBox
    Private WithEvents m_lblPermissionComments As System.Windows.Forms.Label
    Private WithEvents m_tbxPermissionComments As System.Windows.Forms.TextBox
    Private WithEvents m_lblTempMax As System.Windows.Forms.Label
    Private WithEvents m_lblDepthMax As System.Windows.Forms.Label
    Private WithEvents m_lblTempMean As System.Windows.Forms.Label
    Private WithEvents m_lblDepthMean As System.Windows.Forms.Label
    Private WithEvents m_lblTempMin As System.Windows.Forms.Label
    Private WithEvents m_lblDepthMin As System.Windows.Forms.Label
    Private WithEvents m_tbxTempMax As System.Windows.Forms.TextBox
    Private WithEvents m_tbxDepthMax As System.Windows.Forms.TextBox
    Private WithEvents m_tbxTempMean As System.Windows.Forms.TextBox
    Private WithEvents m_tbxDepthMean As System.Windows.Forms.TextBox
    Private WithEvents m_tbxTempMin As System.Windows.Forms.TextBox
    Private WithEvents m_tbxDepthMin As System.Windows.Forms.TextBox
    Private WithEvents m_hdrEcosystem As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents m_pbEnvVars As System.Windows.Forms.PictureBox
    Private WithEvents m_pbEcosystem As System.Windows.Forms.PictureBox
    Private WithEvents m_pbBoundingBox As System.Windows.Forms.PictureBox
    Private WithEvents m_lblObjectives As System.Windows.Forms.Label
    Private WithEvents m_pbObjectives As System.Windows.Forms.PictureBox
    Private WithEvents m_tbxObjectives As System.Windows.Forms.TextBox
    Private WithEvents m_pbPermissionComment As System.Windows.Forms.PictureBox
    Private WithEvents m_cbEcospaceUsed As System.Windows.Forms.CheckBox
    Private WithEvents m_cbFittedToTimeSeries As System.Windows.Forms.CheckBox
    Private WithEvents m_cbEcosimUsed As System.Windows.Forms.CheckBox
    Private WithEvents m_cbPubMatchesPaper As System.Windows.Forms.CheckBox
    Private WithEvents m_pbDifference As System.Windows.Forms.PictureBox
    Private WithEvents m_tbxDifference As System.Windows.Forms.TextBox
    Private WithEvents m_lblExplanation As System.Windows.Forms.Label
    Private WithEvents m_hdrAccess As ScientificInterfaceShared.Controls.cEwEHeaderLabel
    Private WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Private WithEvents m_cbAuthorAgreement As System.Windows.Forms.CheckBox
    Private WithEvents m_tpEcoBase As System.Windows.Forms.TabPage
    Private WithEvents m_pbEcobaseAgreement As System.Windows.Forms.PictureBox
    Private WithEvents m_wrkGetAuthorAgreement As System.ComponentModel.BackgroundWorker
    Private WithEvents m_rtfAuthorAgreement As System.Windows.Forms.RichTextBox
    Private WithEvents m_tbxModelLastYear As System.Windows.Forms.TextBox
    Private WithEvents m_tbxModelFirstYear As System.Windows.Forms.TextBox
    Private WithEvents m_lblModelLastYear As System.Windows.Forms.Label
    Private WithEvents m_tbxModelArea As System.Windows.Forms.TextBox
    Private WithEvents m_lblModelFirstYear As System.Windows.Forms.Label
    Private WithEvents m_lblModelArea As System.Windows.Forms.Label
    Private WithEvents m_pbArea As System.Windows.Forms.PictureBox
    Private WithEvents m_pbModelYear As System.Windows.Forms.PictureBox
    Private WithEvents m_lblExtraInfo As System.Windows.Forms.Label
    Private WithEvents m_plExtent As System.Windows.Forms.Panel
    Private WithEvents m_tpObjectives As System.Windows.Forms.TabPage
    Private WithEvents m_cbObjectiveOtherImpactAssessment As System.Windows.Forms.CheckBox
    Private WithEvents m_cbObjectivePollution As System.Windows.Forms.CheckBox
    Private WithEvents m_cbObjectiveEcosystemFunctioning As System.Windows.Forms.CheckBox
    Private WithEvents m_cbObjectiveEnvironmentalVariability As System.Windows.Forms.CheckBox
    Private WithEvents m_cbObjectiveAquaculture As System.Windows.Forms.CheckBox
    Private WithEvents m_cbObjectiveFisheries As System.Windows.Forms.CheckBox
    Private WithEvents m_tpSubmission As System.Windows.Forms.TabPage
    Private WithEvents m_lblSubmModications As System.Windows.Forms.Label
    Private WithEvents m_lblSubmInfo As System.Windows.Forms.Label
    Private WithEvents m_pbSubmExistingModel As System.Windows.Forms.PictureBox
    Private WithEvents m_tbxSubmModifications As System.Windows.Forms.TextBox
    Private WithEvents m_cmbSubmEcobaseModel As System.Windows.Forms.ComboBox
    Private WithEvents m_lblSubmExistingModel As System.Windows.Forms.Label
    Private WithEvents m_pbSubmModifications As System.Windows.Forms.PictureBox
    Private WithEvents m_rbSubmNew As System.Windows.Forms.RadioButton
    Private WithEvents m_rbSubmUpdate As System.Windows.Forms.RadioButton
    Private WithEvents m_rbSubmDerived As System.Windows.Forms.RadioButton
    Private WithEvents m_cbIsEntireFoodWeb As System.Windows.Forms.CheckBox
    Private WithEvents m_lblClassArea As System.Windows.Forms.Label
    Private WithEvents m_pbOtherText As System.Windows.Forms.PictureBox
    Private WithEvents m_pbOtherNeeded As System.Windows.Forms.PictureBox
    Private WithEvents m_wrkGetModels As System.ComponentModel.BackgroundWorker
    Private WithEvents m_pbSubmType As System.Windows.Forms.PictureBox
    Private WithEvents m_cbObjectiveMarineProtection As System.Windows.Forms.CheckBox
    Private WithEvents m_lblM3 As Label
    Private WithEvents m_lblDegC3 As Label
    Private WithEvents m_lblDegC2 As Label
    Private WithEvents m_lblDegC1 As Label
    Private WithEvents m_lblM2 As Label
    Private WithEvents m_lblM1 As Label
End Class
