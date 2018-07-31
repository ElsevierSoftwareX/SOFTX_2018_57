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

Option Explicit On
Option Strict On

Imports System.Collections.Specialized
Imports System.IO
Imports System.Net
Imports System.Web
Imports EwECore
Imports EwECore.WebServices
Imports EwECore.WebServices.Ecobase
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

''' ---------------------------------------------------------------------------
''' <summary>
''' Dialog to allow users to submit a model to Ecobase
''' </summary>
''' ---------------------------------------------------------------------------
Public Class dlgEcobaseExport

#Region " Private vars "

    Private m_ecobase As cEcoBaseWDSL = Nothing
    Private m_models As New List(Of cModelData)
    Private m_strAuthorAgreement As String = ""

    Private m_fpFirstYear As cEwEFormatProvider = Nothing
    Private m_fpLastYear As cEwEFormatProvider = Nothing
    Private m_fpArea As cEwEFormatProvider = Nothing

    Private m_fpNorth As cEwEFormatProvider = Nothing
    Private m_fpEast As cEwEFormatProvider = Nothing
    Private m_fpWest As cEwEFormatProvider = Nothing
    Private m_fpSouth As cEwEFormatProvider = Nothing

    Private m_fpDmin As cEwEFormatProvider = Nothing
    Private m_fpDmean As cEwEFormatProvider = Nothing
    Private m_fpDmax As cEwEFormatProvider = Nothing

    Private m_fpTmin As cEwEFormatProvider = Nothing
    Private m_fpTmean As cEwEFormatProvider = Nothing
    Private m_fpTmax As cEwEFormatProvider = Nothing

#End Region ' Private vars

#Region " Construction "

    Public Sub New(uic As cUIContext)
        MyBase.New()
        Me.UIContext = uic
        Me.InitializeComponent()
    End Sub

#End Region ' Construction

#Region " Overrides "

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        Dim core As cCore = Me.Core
        Dim model As cEwEModel = core.EwEModel

        Me.m_bInUpdate = True

        Dim il As New ImageList()
        il.Images.Add(SharedResources.OK)
        il.Images.Add(SharedResources.Warning)
        il.Images.Add(SharedResources.Critical)
        Me.m_tcExport.ImageList = il

        ' -- Model page --
        Me.m_tbxModelName.Text = model.Name
        Me.m_tbxModelDescription.Text = model.Description
        Me.m_tbxModelAuthor.Text = If(String.IsNullOrWhiteSpace(model.Author), core.DefaultAuthor, model.Author)
        Me.m_tbxModelEmail.Text = If(String.IsNullOrWhiteSpace(model.Contact), core.DefaultContact, model.Contact)
        Me.m_fpFirstYear = New cEwEFormatProvider(Me.UIContext, Me.m_tbxModelFirstYear, GetType(Integer), model.GetVariableMetadata(eVarNameFlags.EcopathFirstYear))
        Me.m_fpFirstYear.Value = model.FirstYear
        Me.m_fpLastYear = New cEwEFormatProvider(Me.UIContext, Me.m_tbxModelLastYear, GetType(Integer), model.GetVariableMetadata(eVarNameFlags.EcopathNumYears))
        Me.m_fpLastYear.Value = Math.Max(model.NumYears - 1 + model.FirstYear, model.FirstYear)
        Me.m_fpArea = New cEwEFormatProvider(Me.UIContext, Me.m_tbxModelArea, GetType(Single), model.GetVariableMetadata(eVarNameFlags.Area))
        Me.m_fpArea.Value = model.Area

        ' -- Publication page --
        Me.m_tbxHyperlink.Text = model.PublicationURI
        Me.m_tbxDOI.Text = model.PublicationDOI
        Me.m_tbxReference.Text = model.PublicationReference

        ' -- Classification page --
        Me.FillCombo(Me.m_cmbCountry, Me.UIContext.StyleGuide.EcoBaseFields(cStyleGuide.eEcobaseFieldType.CountryName))
        Me.FillCombo(Me.m_cmbEcoType, Me.UIContext.StyleGuide.EcoBaseFields(cStyleGuide.eEcobaseFieldType.EcosystemType))

        Me.m_cmbCountry.Text = model.Country
        Me.m_cmbEcoType.Text = model.EcosystemType

        Me.m_fpNorth = New cEwEFormatProvider(Me.UIContext, Me.m_nudNorth, GetType(Single), model.GetVariableMetadata(eVarNameFlags.North))
        Me.m_fpNorth.Value = model.North
        Me.m_fpEast = New cEwEFormatProvider(Me.UIContext, Me.m_nudEast, GetType(Single), model.GetVariableMetadata(eVarNameFlags.East))
        Me.m_fpEast.Value = model.East
        Me.m_fpWest = New cEwEFormatProvider(Me.UIContext, Me.m_nudWest, GetType(Single), model.GetVariableMetadata(eVarNameFlags.West))
        Me.m_fpWest.Value = model.West
        Me.m_fpSouth = New cEwEFormatProvider(Me.UIContext, Me.m_nudSouth, GetType(Single), model.GetVariableMetadata(eVarNameFlags.South))
        Me.m_fpSouth.Value = model.South

        Dim mdDepth As cVariableMetaData = cVariableMetaData.Get(eVarNameFlags.LayerDepth)
        Me.m_fpDmin = New cEwEFormatProvider(Me.UIContext, Me.m_tbxDepthMin, GetType(Single), cVariableMetaData.Get(eVarNameFlags.LayerDepth))
        Me.m_fpDmin.Value = 0
        Me.m_fpDmean = New cEwEFormatProvider(Me.UIContext, Me.m_tbxDepthMean, GetType(Single), mdDepth)
        Me.m_fpDmean.Value = 0
        Me.m_fpDmax = New cEwEFormatProvider(Me.UIContext, Me.m_tbxDepthMax, GetType(Single), mdDepth)
        Me.m_fpDmax.Value = 0

        Dim mdTemp As New cVariableMetaData(-8, 104, cOperatorManager.getOperator(eOperators.GreaterThanOrEqualTo), cOperatorManager.getOperator(eOperators.LessThanOrEqualTo))
        Me.m_fpTmin = New cEwEFormatProvider(Me.UIContext, Me.m_tbxTempMin, GetType(Single), mdTemp)
        Me.m_fpTmin.Value = 0
        Me.m_fpTmean = New cEwEFormatProvider(Me.UIContext, Me.m_tbxTempMean, GetType(Single), mdTemp)
        Me.m_fpTmean.Value = 0
        Me.m_fpTmax = New cEwEFormatProvider(Me.UIContext, Me.m_tbxTempMax, GetType(Single), mdTemp)
        Me.m_fpTmax.Value = 0

        Me.m_rbSubmNew.Enabled = (String.IsNullOrWhiteSpace(model.EcobaseCode))

        ' -- Objectives page --
        Me.m_tbxObjectives.Text = ""

        Me.m_bInUpdate = False

        Me.m_ecobase = New cEcoBaseWDSL()

        Me.m_wrkGetAuthorAgreement.WorkerSupportsCancellation = True
        Me.m_wrkGetAuthorAgreement.RunWorkerAsync(Nothing)

        Me.m_wrkGetModels.WorkerSupportsCancellation = True
        Me.m_wrkGetModels.RunWorkerAsync(Nothing)

        Me.CenterToParent()
        Me.UpdateControls()

    End Sub

#End Region ' Overrides

#Region " Event handlers "

    Private Sub OnContentChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_cbAuthorAgreement.CheckedChanged,
                m_tbxModelAuthor.TextChanged, m_tbxModelName.TextChanged, m_tbxModelEmail.TextChanged, m_tbxModelDescription.TextChanged, m_tbxObjectives.TextChanged,
                m_tbxDOI.TextChanged, m_tbxHyperlink.TextChanged, m_tbxReference.TextChanged,
                m_cbModelIsAuthor.CheckedChanged, m_cbConfirmDessiminate.CheckedChanged,
                m_tbxModelFirstYear.TextChanged, m_tbxModelLastYear.TextChanged, m_tbxModelArea.TextChanged,
                m_cbEcosimUsed.CheckedChanged, m_cbFittedToTimeSeries.CheckedChanged, m_cbEcospaceUsed.CheckedChanged,
                m_tbxDepthMin.TextChanged, m_tbxDepthMean.TextChanged, m_tbxDepthMax.TextChanged,
                m_tbxTempMin.TextChanged, m_tbxTempMean.TextChanged, m_tbxTempMax.TextChanged,
                m_cmbCountry.TextChanged, m_cmbEcoType.TextChanged,
                m_nudNorth.ValueChanged, m_nudEast.ValueChanged, m_nudWest.ValueChanged, m_nudSouth.ValueChanged,
                m_cbPubMatchesPaper.CheckedChanged, m_tbxDifference.TextChanged,
                m_cbObjectiveAquaculture.CheckedChanged, m_cbObjectiveFisheries.CheckedChanged, m_cbObjectiveEcosystemFunctioning.CheckedChanged,
                m_cbObjectiveEnvironmentalVariability.CheckedChanged, m_cbObjectiveOtherImpactAssessment.CheckedChanged, m_cbObjectivePollution.CheckedChanged, m_cbObjectiveMarineProtection.CheckedChanged,
                m_rbSubmNew.CheckedChanged, m_rbSubmDerived.CheckedChanged, m_rbSubmUpdate.CheckedChanged, m_tbxSubmModifications.TextChanged, m_cmbSubmEcobaseModel.SelectedIndexChanged,
                m_tbxPermissionComments.TextChanged, m_cbObjectiveMarineProtection.CheckedChanged

        Try
            Me.UpdateControls()
        Catch ex As Exception
            cLog.Write(ex, "dlgEcobaseExport.OnContentChanged")
        End Try

    End Sub

    Private Sub OnViewPublication(sender As System.Object, e As System.EventArgs) _
        Handles m_llViewPublication.Click

        Dim strDOI As String = Me.m_tbxDOI.Text

        Try

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            Debug.Assert(cmd IsNot Nothing)

            Dim strURL As String = ""
            If strDOI.StartsWith("http://", StringComparison.OrdinalIgnoreCase) Then
                strURL = strDOI
            Else
                strURL = "http://doi.org/" & HttpUtility.UrlEncode(strDOI)
            End If
            cmd.Invoke(strURL)

        Catch ex As Exception
            cLog.Write(ex, "dlgEcobaseExport.OnViewDOIOnline(" & strDOI & ")")
        End Try

    End Sub

    Private Sub OnFormatModel(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_cmbSubmEcobaseModel.Format

        Dim m As cModelData = DirectCast(e.ListItem, cModelData)
        e.Value = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, m.Name, m.FirstYear)

    End Sub

    Private Sub OnSubmit(sender As System.Object, e As System.EventArgs) _
        Handles m_btnSubmit.Click

        Try

            If Not Me.UpdateModelParameters() Then Return
            If Not Me.SubmitToEcobase() Then Return

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnCancel(sender As System.Object, e As System.EventArgs) _
        Handles m_btnCancel.Click

        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()

    End Sub

#End Region ' Event handlers

#Region " Internals "

    Private m_bInUpdate As Boolean = False

    Protected Overrides Sub UpdateControls()

        If (Me.UIContext Is Nothing) Then Return
        If (Me.m_bInUpdate) Then Return

        Me.m_bInUpdate = True

        Dim core As cCore = Me.UIContext.Core

        ' -- Ecobase page --
        Dim bAgreementOK As Boolean = Me.m_cbAuthorAgreement.Checked

        '--Change text to rtf to enable some text formatting user agreemnt Jerome 25/04/2016
        Me.m_rtfAuthorAgreement.Rtf = Me.m_strAuthorAgreement


        Me.m_pbModelName.BackgroundImage = If(bAgreementOK, SharedResources.OK, SharedResources.Critical)
        Me.m_tpEcoBase.ImageIndex = If(bAgreementOK, 0, 2)

        ' -- Model page --
        Dim bHasModelName As Boolean = Not String.IsNullOrWhiteSpace(Me.m_tbxModelName.Text)
        Dim bHasDescription As Boolean = Not String.IsNullOrWhiteSpace(Me.m_tbxModelDescription.Text)
        Dim bHasAuthor As Boolean = Not String.IsNullOrWhiteSpace(Me.m_tbxModelAuthor.Text)
        Dim bHasContact As Boolean = cStringUtils.IsEmail(Me.m_tbxModelEmail.Text)
        Dim bIsAuthor As Boolean = (Me.m_cbModelIsAuthor.Checked = True)
        Dim bHasYears As Boolean = (CInt(Me.m_fpFirstYear.Value) <= CInt(Me.m_fpLastYear.Value))
        Dim bHasArea As Boolean = (CInt(Me.m_fpArea.Value) > 0)
        Dim bModelOK As Boolean = bHasModelName And bHasDescription And bHasAuthor And bIsAuthor And bHasYears And bHasArea

        Me.m_pbModelName.BackgroundImage = If(bHasModelName, SharedResources.OK, SharedResources.Critical)
        Me.m_pbModelDescription.BackgroundImage = If(bHasDescription, SharedResources.OK, SharedResources.Critical)
        Me.m_pbModelAuthorEmail.BackgroundImage = If(bHasAuthor And bHasContact, SharedResources.OK, SharedResources.Critical)
        Me.m_pbModelYear.BackgroundImage = If(bHasYears, SharedResources.OK, SharedResources.Critical)
        Me.m_pbArea.BackgroundImage = If(bHasArea, SharedResources.OK, SharedResources.Critical)
        Me.m_pbModelIsAuthor.BackgroundImage = If(bIsAuthor, SharedResources.OK, SharedResources.Critical)

        Me.m_cbEcosimUsed.Enabled = (core.nEcosimScenarios > 0)
        If (Not Me.m_cbEcosimUsed.Enabled) Then Me.m_cbFittedToTimeSeries.Checked = False
        Me.m_cbFittedToTimeSeries.Enabled = (core.nTimeSeriesDatasets > 0) And Me.m_cbEcosimUsed.Checked
        Me.m_cbEcospaceUsed.Enabled = (core.nEcospaceScenarios > 0)

        Me.m_tpModel.ImageIndex = If(bModelOK, 0, 2)

        ' -- Publication page --
        Dim bHasPublication As Boolean = Not String.IsNullOrWhiteSpace(Me.m_tbxDOI.Text) Or Not String.IsNullOrWhiteSpace(Me.m_tbxHyperlink.Text)
        Dim bHasReference As Boolean = Not String.IsNullOrWhiteSpace(Me.m_tbxReference.Text)
        Dim bHasDifferences As Boolean = Not String.IsNullOrWhiteSpace(Me.m_tbxDifference.Text)
        Dim bPubsOK As Boolean = bHasPublication Or bHasReference

        If (Me.m_cbPubMatchesPaper.Checked) Then
            Me.m_tbxDifference.Enabled = False
            Me.m_pbDifference.Image = SharedResources.OK
        Else
            Me.m_tbxDifference.Enabled = True
            Me.m_pbDifference.Image = If(bHasDifferences, SharedResources.OK, SharedResources.Critical)
        End If

        Me.m_pbPublication.BackgroundImage = If(bHasPublication, SharedResources.OK, SharedResources.Critical)
        Me.m_pbRef.BackgroundImage = If(bHasReference, SharedResources.OK, SharedResources.Critical)

        Me.m_llViewPublication.Enabled = bHasPublication

        Me.m_tpPublication.ImageIndex = If(bPubsOK, 0, 2)

        ' -- Classification page --
        Dim bHasCountry As Boolean = (Not String.IsNullOrWhiteSpace(Me.m_cmbCountry.Text))
        Dim bHasBoundingBox As Boolean = (CSng(Me.m_fpNorth.Value) <> CSng(Me.m_fpSouth.Value)) And (CSng(Me.m_fpWest.Value) <> CSng(Me.m_fpEast.Value))
        Dim bHasEcosystem As Boolean = (Not String.IsNullOrWhiteSpace(Me.m_cmbEcoType.Text))
        Dim bHasEnv As Boolean = (CSng(Me.m_fpDmean.Value) > 0) And (CSng(Me.m_fpDmax.Value) > 0)
        Dim bClassOK As Boolean = bHasCountry And bHasBoundingBox And bHasEcosystem

        Me.m_pbAreaName.BackgroundImage = If(bHasCountry, SharedResources.OK, SharedResources.Critical)
        Me.m_pbBoundingBox.BackgroundImage = If(bHasBoundingBox, SharedResources.OK, SharedResources.Critical)
        Me.m_pbEcosystem.BackgroundImage = If(bHasEcosystem, SharedResources.OK, SharedResources.Critical)
        Me.m_pbEnvVars.BackgroundImage = If(bHasEnv, SharedResources.OK, SharedResources.Warning)

        If (bHasCountry And bHasBoundingBox And bHasBoundingBox) Then
            Me.m_tpClassification.ImageIndex = If(bHasEnv, 0, 1)
        Else
            Me.m_tpClassification.ImageIndex = 2
        End If

        Me.m_tpClassification.ImageIndex = If(bClassOK, 0, 2)

        ' -- Objectives --
        Dim bHasObjOptions As Boolean = (Me.m_cbObjectiveFisheries.Checked Or Me.m_cbObjectiveAquaculture.Checked Or
                                         Me.m_cbObjectiveEcosystemFunctioning.Checked Or Me.m_cbObjectiveEnvironmentalVariability.Checked Or
                                         Me.m_cbObjectivePollution.Checked Or m_cbObjectiveMarineProtection.Checked)
        Dim bHasObjOther As Boolean = Me.m_cbObjectiveOtherImpactAssessment.Checked
        Dim bHasObjOtherText As Boolean = Not String.IsNullOrWhiteSpace(Me.m_tbxObjectives.Text)
        Dim bObjectivesOK As Boolean = bHasObjOptions

        If bHasObjOther Then bObjectivesOK = bObjectivesOK And bHasObjOtherText

        If (bHasObjOptions) Then
            Me.m_pbOtherNeeded.BackgroundImage = Nothing
        Else
            Me.m_pbOtherNeeded.BackgroundImage = If(bHasObjOther, SharedResources.OK, SharedResources.Critical)
        End If
        If (bHasObjOther) Then
            Me.m_pbOtherText.BackgroundImage = If(bHasObjOtherText, SharedResources.OK, SharedResources.Critical)
        Else
            Me.m_pbOtherText.BackgroundImage = Nothing
        End If

        Me.m_pbObjectives.BackgroundImage = If(bObjectivesOK, SharedResources.OK, SharedResources.Critical)
        Me.m_tpObjectives.ImageIndex = If(bObjectivesOK, 0, 2)

        ' -- Open access --
        Dim bAccessOK As Boolean = False
        If (Me.m_cbConfirmDessiminate.Checked) Then
            bAccessOK = True
            Me.m_lblPermissionComments.Enabled = False
            Me.m_tbxPermissionComments.Enabled = False
            Me.m_pbPermissionComment.Image = SharedResources.OK
        Else
            bAccessOK = (Not String.IsNullOrWhiteSpace(Me.m_tbxPermissionComments.Text))
            Me.m_lblPermissionComments.Enabled = True
            Me.m_tbxPermissionComments.Enabled = True
            Me.m_pbPermissionComment.Image = If(bAccessOK, SharedResources.OK, SharedResources.Critical)
        End If
        Me.m_tpAccess.ImageIndex = If(bAccessOK, 0, 2)

        ' -- Submission --
        Dim bIsModification As Boolean = Me.m_rbSubmUpdate.Checked Or Me.m_rbSubmDerived.Checked
        Dim bIsNew As Boolean = Me.m_rbSubmNew.Checked
        Dim bHasModelLink As Boolean = (Me.m_cmbSubmEcobaseModel.SelectedItem IsNot Nothing)
        Dim bHasModificationText As Boolean = (Not String.IsNullOrWhiteSpace(Me.m_tbxSubmModifications.Text))
        Dim bHasModDetails As Boolean = bHasModelLink And bHasModificationText
        Dim bSubmissionOk As Boolean = bIsNew Or bHasModDetails

        Me.m_tbxSubmModifications.Enabled = bIsModification
        Me.m_cmbSubmEcobaseModel.Enabled = bIsModification

        If bIsModification Then
            Me.m_pbSubmExistingModel.Image = If(bHasModelLink, SharedResources.OK, SharedResources.Critical)
            Me.m_pbSubmModifications.Image = If(bHasModificationText, SharedResources.OK, SharedResources.Critical)
        Else
            Me.m_pbSubmModifications.Image = Nothing
            Me.m_pbSubmExistingModel.Image = Nothing
        End If

        Me.m_tpSubmission.ImageIndex = If(bSubmissionOk, 0, 2)

        ' -- SUBMIT --
        Me.m_btnSubmit.Enabled = bAgreementOK And bModelOK And bPubsOK And bClassOK And bAccessOK And bObjectivesOK And bSubmissionOk

        Me.m_bInUpdate = False

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Store updated user input into the EwE model and save the changes.
    ''' </summary>
    ''' <returns>
    ''' True if successful.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Private Function UpdateModelParameters() As Boolean

        Dim strName As String = Me.m_tbxModelName.Text
        Dim strDescr As String = Me.m_tbxModelDescription.Text
        Dim strAuthor As String = Me.m_tbxModelAuthor.Text
        Dim strContact As String = Me.m_tbxModelEmail.Text
        Dim strCountry As String = Me.m_cmbCountry.Text
        Dim strEcoType As String = Me.m_cmbEcoType.Text

        Dim iYear As Integer = CInt(Me.m_fpFirstYear.Value)
        Dim iYears As Integer = CInt(Me.m_fpLastYear.Value) - iYear + 1
        Dim sArea As Single = CSng(Me.m_fpArea.Value)

        Dim strDOI As String = Me.m_tbxDOI.Text
        Dim strURI As String = Me.m_tbxHyperlink.Text
        Dim strRef As String = Me.m_tbxReference.Text
        Dim sNorth As Single = CSng(Me.m_fpNorth.Value)
        Dim sEast As Single = CSng(Me.m_fpEast.Value)
        Dim sWest As Single = CSng(Me.m_fpWest.Value)
        Dim sSouth As Single = CSng(Me.m_fpSouth.Value)

        Dim core As cCore = Me.UIContext.Core
        Dim model As cEwEModel = core.EwEModel
        Dim bSucces As Boolean = True

        Dim bChange As Boolean = (String.Compare(strName, model.Name) <> 0) Or _
                                 (String.Compare(strAuthor, model.Author) <> 0) Or _
                                 (String.Compare(strContact, model.Contact) <> 0) Or _
                                 (String.Compare(strDescr, model.Description) <> 0) Or _
                                 (String.Compare(strDOI, model.PublicationDOI) <> 0) Or _
                                 (String.Compare(strURI, model.PublicationURI) <> 0) Or _
                                 (String.Compare(strRef, model.PublicationReference) <> 0) Or _
                                 (String.Compare(strCountry, model.Country) <> 0) Or _
                                 (String.Compare(strEcoType, model.EcosystemType) <> 0) Or _
                                 (model.Area <> sArea) Or _
                                 (model.FirstYear <> iYear) Or _
                                 (model.NumYears <> iYears)

        bChange = bChange Or (model.North <> sNorth) Or _
                             (model.East <> sEast) Or _
                             (model.West <> sWest) Or _
                             (model.South <> sSouth)

        If bChange Then

            model.Name = strName
            model.Description = strDescr
            model.Author = strAuthor
            model.Contact = strContact

            model.FirstYear = iYear
            model.NumYears = iYears
            model.Area = sArea

            model.PublicationDOI = strDOI
            model.PublicationURI = strURI
            model.PublicationReference = strRef

            model.Country = strCountry
            model.EcosystemType = strEcoType

            model.North = sNorth
            model.East = sEast
            model.West = sWest
            model.South = sSouth

            bSucces = core.SaveChanges(True, cCore.eBatchChangeLevelFlags.Ecopath)

        End If

        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Store a valid Ecobase model number into the model.
    ''' </summary>
    ''' <param name="strNumber">The model number to store.</param>
    ''' <returns>
    ''' True if successful.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Private Function UpdateModelNumber(ByVal strNumber As String) As Boolean

        Dim core As cCore = Me.UIContext.Core
        Dim model As cEwEModel = core.EwEModel

        If (String.IsNullOrWhiteSpace(strNumber)) Then Return False
        If (String.Compare(strNumber, model.EcobaseCode) <> 0) Then
            model.EcobaseCode = strNumber
            Return core.SaveChanges(True, cCore.eBatchChangeLevelFlags.Ecopath)
        End If

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Sends the current model to Ecobase.
    ''' </summary>
    ''' <returns>
    ''' True if successful.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Private Function SubmitToEcobase() As Boolean

        Dim msg As cMessage = Nothing
        Dim wdsl As New cEcoBaseWDSL()
        Dim bSucces As Boolean = True

        ' Sanity checks
        Debug.Assert(Me.Core.IsModelBalanced)

        ' Prepare data to send to Ecobase
        Dim data As New WebServices.Ecobase.cEcobaseModelParameters(core)
        Dim md As Ecobase.cModelData = data.Model

        ' Update values not stored in the model
        If (Me.m_rbSubmNew.Checked) Then
            md.SubmissionType = cModelData.eSubmissionType.New
            md.SubmissionLink = ""
            md.SubmissionComments = ""
        ElseIf (Me.m_rbSubmDerived.Checked) Then
            md.SubmissionType = cModelData.eSubmissionType.Derived
            md.SubmissionLink = DirectCast(Me.m_cmbSubmEcobaseModel.SelectedItem, cModelData).EcobaseCode
            md.SubmissionComments = Me.m_tbxSubmModifications.Text
        ElseIf (Me.m_rbSubmUpdate.Checked) Then
            md.SubmissionType = cModelData.eSubmissionType.Replacement
            md.SubmissionLink = DirectCast(Me.m_cmbSubmEcobaseModel.SelectedItem, cModelData).EcobaseCode
            md.SubmissionComments = Me.m_tbxSubmModifications.Text
        End If

        md.EcosimUsed = Me.m_cbEcosimUsed.Checked
        md.EcospaceUsed = Me.m_cbEcospaceUsed.Checked
        md.IsFittedToTimeSeries = Me.m_cbFittedToTimeSeries.Checked

        md.DepthMin = CSng(Me.m_fpDmin.Value)
        md.DepthMean = CSng(Me.m_fpDmean.Value)
        md.DepthMax = CSng(Me.m_fpDmax.Value)

        md.TempMin = CSng(Me.m_fpTmin.Value)
        md.TempMean = CSng(Me.m_fpTmean.Value)
        md.TempMax = CSng(Me.m_fpTmax.Value)

        md.AllowDissemination = Me.m_cbConfirmDessiminate.Checked
        md.CommentsAccess = Me.m_tbxPermissionComments.Text.Trim()

        md.ModelMatchesPaper = Me.m_cbPubMatchesPaper.Checked
        md.CommentsDifference = Me.m_tbxDifference.Text.Trim()

        md.ObjectiveFisheries = Me.m_cbObjectiveFisheries.Checked
        md.ObjectiveAquaculture = Me.m_cbObjectiveAquaculture.Checked
        md.ObjectiveEcosystemFunctioning = Me.m_cbObjectiveEcosystemFunctioning.Checked
        md.ObjectiveEnvironmentalVariability = Me.m_cbObjectiveEnvironmentalVariability.Checked
        md.ObjectivePollution = Me.m_cbObjectivePollution.Checked
        md.ObjectiveMarineProtection = Me.m_cbObjectiveMarineProtection.Checked
        md.ObjectiveOtherImpactAssessment = Me.m_cbObjectiveOtherImpactAssessment.Checked
        md.Objectives = Me.m_tbxObjectives.Text

        ' Obtain XML
        Dim strXML As String = WebServices.Ecobase.cEcobaseModelParameters.ToXML(data)

#If DEBUG Then
        ' Store outgoing XML for debugging purposes
        Dim strFile As String = Path.GetFullPath(".\Ecobase_export.xml")
        Dim writer As New StreamWriter(strFile)
        writer.Write(strXML)
        writer.Close()

        msg = New cMessage("Ecobase export XML saved to " & strFile, eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
        msg.Hyperlink = Path.GetDirectoryName(strFile)
        core.Messages.SendMessage(msg)
        msg = Nothing
#End If

        Try
            strXML = wdsl.Upload_Model(1, strXML)

            ' Analyse result
            Dim results As Ecobase.cEcobaseSubmission = Ecobase.cEcobaseSubmission.FromXML(strXML)

            Select Case results.ResultType
                Case Ecobase.cEcobaseSubmission.eSubmisssionResultTypes.NotInEcobase
                    msg = New cMessage(My.Resources.ECOBASE_SUBMIT_DENIED, eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)

                Case Ecobase.cEcobaseSubmission.eSubmisssionResultTypes.Pending
                    msg = New cFeedbackMessage(My.Resources.ECOBASE_SUBMIT_REVIEW, eCoreComponentType.External, eMessageType.DataExport, eMessageImportance.Information, eMessageReplyStyle.OK)

                Case Ecobase.cEcobaseSubmission.eSubmisssionResultTypes.Accepted
                    msg = New cFeedbackMessage(My.Resources.ECOBASE_SUBMIT_ACCEPTED, eCoreComponentType.External, eMessageType.DataExport, eMessageImportance.Information, eMessageReplyStyle.OK)

            End Select

            Me.UpdateModelNumber(results.ModelNumber)

        Catch ex As WebException
            bSucces = False
            msg = New cMessage(My.Resources.ECOBASE_ERROR_NOCONNECTION, _
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
        Catch ex As Exception
            bSucces = False
            msg = New cMessage(cStringUtils.Localize(My.Resources.ECOBASE_ERROR_COMMUNICATION, ex.Message), _
                               eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Critical)
        End Try

        If (msg IsNot Nothing) Then
            core.Messages.SendMessage(msg)
        End If

        Return bSucces

    End Function

    Private Sub FillCombo(cmb As ComboBox, values As StringCollection)

        cmb.Items.Clear()
        For Each str As String In values
            cmb.Items.Add(str)
        Next

    End Sub

    Private Sub FillModelCombo()

        Me.m_cmbSubmEcobaseModel.Items.Clear()
        For Each m As cModelData In Me.m_models
            Me.m_cmbSubmEcobaseModel.Items.Add(m)
        Next

    End Sub

#End Region ' Internals

#Region " Background workers "

    Private Sub OnGetModels(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs) _
        Handles m_wrkGetModels.DoWork

        Dim msg As cMessage = Nothing

        Me.m_models.Clear()

        Try
            Dim strModels As String = Me.m_ecobase.list_models("", Nothing)
            Dim data As cEcobaseModelList = cEcobaseModelList.FromXML(strModels)
            Me.m_models.AddRange(data.Models)

            e.Cancel = Me.m_wrkGetModels.CancellationPending

        Catch exWeb As Net.WebException
            msg = New cMessage(My.Resources.ECOBASE_ERROR_NOCONNECTION, eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Critical)
        Catch ex As Exception
            msg = New cMessage(String.Format(My.Resources.ECOBASE_ERROR_COMMUNICATION, ex.Message), _
                                    eMessageType.DataImport, eCoreComponentType.External, eMessageImportance.Critical)
        End Try

        If (msg IsNot Nothing) Then
            Me.Core.Messages.SendMessage(msg)
        End If

    End Sub

    Private Sub OnGetModelsCompleted(sender As Object, _
                                     e As System.ComponentModel.RunWorkerCompletedEventArgs) _
        Handles m_wrkGetModels.RunWorkerCompleted

        Try
            If e.Cancelled Then Return
            Me.FillModelCombo()
            Me.UpdateControls()
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnGetAuthorAgreement(sender As Object, e As System.ComponentModel.DoWorkEventArgs) _
        Handles m_wrkGetAuthorAgreement.DoWork

        Try
            Dim wdsl As New cEcoBaseWDSL()
            Dim strAgreement As String = wdsl.getModel("agreement", -1)
            Dim data As cEcobaseDataAccessAgreement = cEcobaseDataAccessAgreement.FromXML(strAgreement)

            Me.m_strAuthorAgreement = data.AuthorAgreement
            e.Cancel = Me.m_wrkGetAuthorAgreement.CancellationPending

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnGetAuthorAgreementComplete(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) _
        Handles m_wrkGetAuthorAgreement.RunWorkerCompleted

        Try
            If (e.Cancelled) Then Return
            Me.UpdateControls()
        Catch ex As Exception

        End Try

    End Sub

#End Region

    Private Sub dlgEcobaseExport_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub m_rtfAuthorAgreement_TextChanged(sender As System.Object, e As System.EventArgs) Handles m_rtfAuthorAgreement.TextChanged

    End Sub
End Class