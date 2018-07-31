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
Imports System.ComponentModel
Imports System.Text
Imports EwECore
Imports EwECore.WebServices
Imports EwECore.WebServices.Ecobase
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports System.Web

#End Region ' Imports

Public Class dlgEcobaseImport

#Region " Private vars "

    Private m_ecobase As cEcoBaseWDSL = Nothing
    Private m_models As New List(Of cModelData)
    Private m_model As cModelData = Nothing

    Private m_strUserAgreement As String = ""
    Private m_img As Image = Nothing

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Tags in <see cref="m_tsddValue">filter dropdown items</see> must correspond to the values in this enum.
    ''' </remarks>
    Private Enum eFilterTypes As Integer
        None = 0
        Author = 1
        Country = 2
        EcosystemType = 3
        Depth = 4
        Temperature = 5
        Reference = 6
        ModelName = 7
    End Enum

    Private m_filter As eFilterTypes = eFilterTypes.None

#End Region ' Private vars

#Region " Construction "

    Public Sub New(uic As cUIContext)
        Me.InitializeComponent()
        Me.UIContext = uic
    End Sub

#End Region ' Construction

#Region " Form overrides "

    Protected Overrides Sub OnLoad(e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.CenterToScreen()

        If (Me.UIContext Is Nothing) Then Return

        Me.m_lbxModels.UIContext = Me.UIContext

        Dim il As New ImageList()
        il.Images.Add(SharedResources.OK)
        il.Images.Add(SharedResources.Warning)
        il.Images.Add(SharedResources.Critical)
        Me.m_tcContent.ImageList = il

        Me.m_tsbnShowYear.Image = SharedResources.CalendarHS
        Me.m_tsbnShowAuthor.Image = SharedResources.PersonHS
        Me.m_tsbnShowDownloadable.Image = SharedResources.DownloadHS
        Me.m_tsddValue.Image = SharedResources.FilterHS

        ' Retrieve persistent settings
        Dim p As New cSettingsParser(Me.Settings)
        Me.m_tsbnShowYear.Checked = p.Parameter("Year", "1") = "1"
        Me.m_tsbnShowAuthor.Checked = p.Parameter("Author", "0") = "1"
        Me.m_tsbnShowDownloadable.Checked = p.Parameter("Downloadable", "1") = "1"

        Me.m_ecobase = New cEcoBaseWDSL()

        Me.m_wrkGetAgreement.WorkerSupportsCancellation = True
        Me.m_wrkGetModels.WorkerSupportsCancellation = True
        Me.m_wrkGetImage.WorkerSupportsCancellation = True

        Me.m_wrkGetAgreement.RunWorkerAsync(Nothing)
        Me.m_wrkGetModels.RunWorkerAsync(Nothing)

        Me.UpdateControls()

    End Sub

    Protected Overrides Sub OnFormClosing(e As System.Windows.Forms.FormClosingEventArgs)

        ' See if form can apply on OK
        If (Me.DialogResult = System.Windows.Forms.DialogResult.OK) Then
            If (Me.m_model Is Nothing) Then
                e.Cancel = True
            Else
                e.Cancel = (Me.m_model.AllowDissemination = False)
            End If
        End If

        ' Going to close?
        If (Not e.Cancel) Then

            ' #Yes: store persistent settings
            Dim p As New cSettingsParser()
            p.Parameter("Year") = If(Me.m_tsbnShowYear.Checked, "1", "0")
            p.Parameter("Author") = If(Me.m_tsbnShowAuthor.Checked, "1", "0")
            p.Parameter("Downloadable") = If(Me.m_tsbnShowDownloadable.Checked, "1", "0")
            Me.Settings = p.Buffer

            Try
                ' Shoot workers
                Me.m_ecobase.CancelAsync(Nothing)
                Me.m_wrkGetAgreement.CancelAsync()
                Me.m_wrkGetImage.CancelAsync()
                Me.m_wrkGetModels.CancelAsync()
            Catch ex As Exception
                ' Do not tell the union why
            End Try
        End If

        MyBase.OnFormClosing(e)

    End Sub

    ''' <summary>
    ''' Overridden to redraw models listbox that uses StyleGuide colours.
    ''' </summary>
    Protected Overrides Sub OnStyleGuideChanged(ct As ScientificInterfaceShared.Style.cStyleGuide.eChangeType)
        If ((ct And cStyleGuide.eChangeType.Colours) > 0) Then
            Me.m_lbxModels.Invalidate()
        End If
        MyBase.OnStyleGuideChanged(ct)
    End Sub

    Protected Overrides Sub OnSizeChanged(e As System.EventArgs)
        MyBase.OnSizeChanged(e)
    End Sub

    Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

        Try
            Me.m_ecobase.Dispose()
            Me.m_ecobase = Nothing
        Catch ex As Exception

        End Try
        MyBase.OnFormClosed(e)

    End Sub

    Protected Overrides Sub UpdateControls()
        MyBase.UpdateControls()

        Dim bCanDissiminate As Boolean = False
        Dim bAgreementOK As Boolean = Me.m_cbEcoBaseAgreement.Checked
        Dim bCanDownload As Boolean = False

        Me.m_tsddValue.Text = Me.FilterItemText(Me.m_filter)
        Me.m_tstbSearch.Enabled = (Me.m_filter <> eFilterTypes.None)
        '--Change text to rtf to enable some text formatting user agreemnt Jerome 25/04/2016
        Me.m_rtfAgreement.Rtf = Me.m_strUserAgreement
        Me.m_cbEcoBaseAgreement.Enabled = Not String.IsNullOrWhiteSpace(Me.m_strUserAgreement)

        ' Populate model controls
        If (Me.m_model IsNot Nothing) Then
            Dim sb As New StringBuilder()

            Me.m_lblModelNameValue.Text = cStringUtils.ToSentenceCase(Me.m_model.Name.Trim)
            Me.m_lblAreaValue.Text = cStringUtils.FormatNumber(Me.m_model.Area)
            Me.m_lblAuthorValue.Text = cStringUtils.ToTitleCase(Me.m_model.Author)
            Me.m_lblCountryValue.Text = cStringUtils.ToTitleCase(Me.m_model.Country)
            Me.m_lblEcosystemTypeValue.Text = cStringUtils.ToSentenceCase(Me.m_model.EcosystemType)
            Me.m_lblPeriodValue.Text = Me.PeriodLabel()
            Me.m_lblEcosimUsedValue.Text = If(Me.m_model.EcosimUsed, SharedResources.BUTTON_YES, SharedResources.BUTTON_NO)
            Me.m_lblFittedValue.Text = If(Me.m_model.IsFittedToTimeSeries, SharedResources.BUTTON_YES, SharedResources.BUTTON_NO)
            Me.m_lblEcospaceUsedValue.Text = If(Me.m_model.EcospaceUsed, SharedResources.BUTTON_YES, SharedResources.BUTTON_NO)

            Me.m_lblDessimAllowValue.Text = If(Me.m_model.AllowDissemination, SharedResources.BUTTON_YES, SharedResources.BUTTON_NO)
            Me.m_lblDessimAllowValue.Image = If(Me.m_model.AllowDissemination, SharedResources.OK, SharedResources.Critical)

            Me.m_lblRefValue.Text = Me.m_model.Reference()

            If (String.IsNullOrWhiteSpace(Me.m_model.DOI)) Then
                Me.m_llDOI.Visible = False
            Else
                Me.m_llDOI.Text = Me.m_model.DOI
                Me.m_llDOI.Visible = True
            End If

            If (String.IsNullOrWhiteSpace(Me.m_model.URI)) Then
                Me.m_llURL.Visible = False
            Else
                Me.m_llURL.Text = Me.m_model.URI
                Me.m_llURL.Visible = True
            End If

            If (Me.m_img Is Nothing) Then
                Me.m_pbImage.Image = SharedResources.ani_loader
                Me.m_pbImage.SizeMode = PictureBoxSizeMode.CenterImage
                Me.m_pbImage.BackColor = Color.Transparent
                'Me.m_pbImage.BackgroundImageLayout = ImageLayout.Center
            Else
                Me.m_pbImage.Image = Me.m_img
                Me.m_pbImage.SizeMode = PictureBoxSizeMode.Zoom
                Me.m_pbImage.BackColor = Me.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.MAP_BACKGROUND)
                'Me.m_pbImage.BackgroundImageLayout = ImageLayout.Zoom
            End If
            Me.m_lblLonVal.Text = Me.LonLabel()
            Me.m_lblLatVal.Text = Me.LatLabel()
            Me.m_lblDepthRangeVal.Text = Me.RangeLabel(Me.m_model.DepthMin, Me.m_model.DepthMax)
            Me.m_lblDepthMeanVal.Text = Me.MeanLabel(Me.m_model.DepthMin, Me.m_model.DepthMax, Me.m_model.DepthMean)
            Me.m_lblTempRangeVal.Text = Me.RangeLabel(Me.m_model.TempMin, Me.m_model.TempMax)
            Me.m_lblTempMeanVal.Text = Me.MeanLabel(Me.m_model.TempMin, Me.m_model.TempMax, Me.m_model.TempMean)

            bCanDissiminate = Me.m_model.AllowDissemination
        Else
            Me.m_lblModelNameValue.Text = ""
            Me.m_lblAreaValue.Text = ""
            Me.m_lblAuthorValue.Text = ""
            Me.m_lblCountryValue.Text = ""
            Me.m_lblEcosystemTypeValue.Text = ""
            Me.m_lblPeriodValue.Text = ""
            Me.m_lblEcosimUsedValue.Text = ""
            Me.m_lblFittedValue.Text = ""
            Me.m_lblEcospaceUsedValue.Text = ""
            Me.m_lblDessimAllowValue.Text = ""
            Me.m_lblDessimAllowValue.Image = Nothing
            Me.m_lblRefValue.Text = ""
            Me.m_pbImage.Image = Nothing
            Me.m_lblLonVal.Text = ""
            Me.m_lblLatVal.Text = ""
            Me.m_lblDepthRangeVal.Text = ""
            Me.m_lblDepthMeanVal.Text = ""
            Me.m_lblTempRangeVal.Text = ""
            Me.m_lblTempMeanVal.Text = ""
            Me.m_llDOI.Visible = False
            Me.m_llURL.Visible = False
        End If

        bCanDownload = bCanDissiminate And bAgreementOK

        Me.m_tpAgreement.ImageIndex = If(bAgreementOK, 0, 2)
        Me.m_tpImport.ImageIndex = If(bCanDownload, 0, 2)

        Me.m_btnOK.Enabled = bCanDownload

    End Sub

#End Region ' Form overrides

#Region " Public access "

    Public ReadOnly Property SelectedModel As cModelData
        Get
            Return Me.m_model
        End Get
    End Property

#End Region ' Public access

#Region " Control events "

    Private Sub OnShowOptionChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_tsbnShowYear.CheckedChanged, m_tsbnShowAuthor.CheckedChanged, m_tsbnShowDownloadable.CheckedChanged

        Me.UpdateModelList()

    End Sub

    Private Sub OnModelSelected(sender As System.Object, e As System.EventArgs) _
        Handles m_lbxModels.SelectedIndexChanged

        Try
            Me.m_model = Nothing
            If (Me.m_lbxModels.SelectedIndex > -1) Then

                Me.m_model = DirectCast(Me.m_lbxModels.SelectedItem, cModelData)
                Me.m_img = Nothing

                If (Me.m_wrkGetImage.IsBusy) Then
                    Me.m_wrkGetImage.CancelAsync()
                End If
                Me.m_wrkGetImage.RunWorkerAsync(Nothing)

            End If

        Catch ex As Exception

        End Try

        Me.UpdateControls()

    End Sub

    Private Sub OnAcceptAgreement(sender As System.Object, e As System.EventArgs) _
        Handles m_cbEcoBaseAgreement.CheckedChanged

        Try
            Me.UpdateControls()
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

    Private Sub OnViewDOI(sender As System.Object, e As System.EventArgs) _
        Handles m_llDOI.Click

        Dim strLink As String = Me.m_llDOI.Text

        Try

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            Debug.Assert(cmd IsNot Nothing)

            cmd.Invoke("http://doi.org/" & HttpUtility.UrlEncode(strLink))

        Catch ex As Exception
            cLog.Write(ex, "dlgEcobaseImport.OnViewDOI(" & strLink & ")")
        End Try

    End Sub

    Private Sub OnViewLink(sender As System.Object, e As System.EventArgs) _
        Handles m_llURL.Click

        Dim strLink As String = Me.m_llURL.Text

        Try

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            Debug.Assert(cmd IsNot Nothing)

            cmd.Invoke(strLink)

        Catch ex As Exception
            cLog.Write(ex, "dlgEcobaseImport.OnViewLink(" & strLink & ")")
        End Try

    End Sub

    Private Sub OnVisitEcobase(sender As System.Object, e As EventArgs) _
        Handles m_llToEcoBase.Click

        Try
            Dim link As New cWebLinks(Me.Core)
            Dim cmd As cBrowserCommand = CType(Me.CommandHandler.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
            Dim strURL As String = String.Format(link.GetURL(cWebLinks.eLinkType.EcoBaseModelInfo), Me.m_model.EcobaseCode)
            cmd.Invoke(strURL)

            'If Not My.Settings.UseExternalBrowser Then
            '    ' Close this dialog to reveal start screen
            '    Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            '    Me.Close()
            'End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnCancel(sender As System.Object, e As System.EventArgs) _
        Handles m_btnCancel.Click

        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()

    End Sub

    Private Sub OnOK(sender As System.Object, e As System.EventArgs) _
        Handles m_btnOK.Click, m_lbxModels.DoubleClick

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()

    End Sub

#End Region ' Control events

#Region " Filter events "

    Private Sub OnSearchTextChanged(sender As System.Object, e As System.EventArgs) _
        Handles m_tstbSearch.TextChanged

        Me.UpdateModelList()

    End Sub

    Private Sub OnFilterSelected(sender As System.Object, e As System.EventArgs) _
        Handles m_tsmiNone.Click, m_tsmiAuthor.Click, m_tsmiCountry.Click, m_tsmiEcoType.Click, _
                m_tsmiDepth.Click, m_tsmiTemperature.Click, m_tsmiReference.Click, m_tsmiModelName.Click

        Dim tsmi As ToolStripItem = DirectCast(sender, ToolStripItem)
        If (tsmi.Tag IsNot Nothing) Then
            Dim iVal As Integer
            If (Integer.TryParse(CStr(tsmi.Tag), iVal)) Then
                Try
                    Me.m_filter = DirectCast(iVal, eFilterTypes)
                Catch ex As Exception
                    Me.m_filter = eFilterTypes.None
                End Try
            End If
        End If

        Me.UpdateModelList()
        Me.UpdateControls()

    End Sub

    Private Function FilterItemText(filter As Integer) As String

        Dim strFilterText As String = SharedResources.GENERIC_VALUE_NONE

        For Each tsmi As ToolStripItem In Me.m_tsddValue.DropDownItems
            Dim iVal As Integer = cCore.NULL_VALUE
            If tsmi.Tag IsNot Nothing Then
                If Integer.TryParse(CStr(tsmi.Tag), iVal) Then
                    If CInt(filter) = iVal Then
                        strFilterText = tsmi.Text
                    End If
                End If
            End If
        Next
        Return strFilterText

    End Function

#End Region ' Filter events

#Region " Background workers "

    Private Sub OnGetModels(sender As System.Object, e As DoWorkEventArgs) _
        Handles m_wrkGetModels.DoWork

        Dim msg As cMessage = Nothing

        Me.m_models.Clear()

        Try
            Dim strModels As String = Me.m_ecobase.list_models("", Nothing)
            Dim data As cEcobaseModelList = cEcobaseModelList.FromXML(strModels)
            Me.m_models.AddRange(data.Models)

            If Me.m_wrkGetModels.CancellationPending Then e.Cancel = True

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

    Private Sub OnGetModelsCompleted(sender As Object, e As RunWorkerCompletedEventArgs) _
        Handles m_wrkGetModels.RunWorkerCompleted

        Try
            If e.Cancelled Then Return
            Me.UpdateEcoBaseLists()
            Me.UpdateModelList()
            Me.UpdateControls()
        Catch ex As Exception

        End Try

    End Sub


    Private Sub OnGetUserAgreement(sender As Object, e As DoWorkEventArgs) _
        Handles m_wrkGetAgreement.DoWork

        Try
            Dim wdsl As New cEcoBaseWDSL()
            Dim strAgreement As String = wdsl.getModel("agreement", -1)
            Dim data As cEcobaseDataAccessAgreement = cEcobaseDataAccessAgreement.FromXML(strAgreement)
            If Me.m_wrkGetAgreement.CancellationPending Then e.Cancel = True

            Me.m_strUserAgreement = data.UserAgreement

        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnGetAgreementComplete(sender As Object, e As RunWorkerCompletedEventArgs) _
        Handles m_wrkGetAgreement.RunWorkerCompleted

        Try
            If e.Cancelled Then Return
            Me.UpdateControls()
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnGetImage(sender As System.Object, e As DoWorkEventArgs) _
        Handles m_wrkGetImage.DoWork

        Try
            If (Me.m_model Is Nothing) Then
                Me.m_img = Nothing
                Return
            End If

            Dim MyWebClient As New System.Net.WebClient()
            Dim data() As Byte = MyWebClient.DownloadData("http://ecobase.ecopath.org/php/mapserver.php?model=" & m_model.EcobaseCode)
            Dim strm As New IO.MemoryStream(data)
            Me.m_img = New System.Drawing.Bitmap(strm)

            If Me.m_wrkGetImage.CancellationPending Then e.Cancel = True
        Catch ex As Exception

        End Try

    End Sub

    Private Sub OnGetImageCompleted(sender As Object, e As RunWorkerCompletedEventArgs) _
        Handles m_wrkGetImage.RunWorkerCompleted

        Try
            If e.Cancelled Then Return
            Me.UpdateControls()
        Catch ex As Exception

        End Try

    End Sub

#End Region ' Background workers

#Region " Internals "

    Private Sub UpdateModelList()

        Dim strFilter As String = Me.m_tstbSearch.Text
        Dim bShowDownloadsOnly As Boolean = Me.m_tsbnShowDownloadable.Checked
        Dim bUseModel As Boolean = True
        Dim bKeepSelection As Boolean = False

        Me.m_lbxModels.ShowAuthor = Me.m_tsbnShowAuthor.Checked
        Me.m_lbxModels.ShowYear = Me.m_tsbnShowYear.Checked

        Me.m_lbxModels.Sorted = True
        Me.m_lbxModels.BeginUpdate()
        Me.m_lbxModels.Items.Clear()

        For Each model As cModelData In Me.m_models
            If (model.AllowDissemination Or Not bShowDownloadsOnly) Then
                bUseModel = True

                ' Filters
                If (Not String.IsNullOrWhiteSpace(strFilter)) Then
                    Select Case Me.m_filter
                        Case eFilterTypes.None
                        Case eFilterTypes.Author : bUseModel = Me.StartsWith(strFilter, model.Author)
                        Case eFilterTypes.Country : bUseModel = Me.StartsWith(strFilter, model.Country)
                        Case eFilterTypes.EcosystemType : bUseModel = Me.StartsWith(strFilter, model.EcosystemType)
                        Case eFilterTypes.Depth : bUseModel = Me.IsInRange(strFilter, model.DepthMin, model.DepthMax)
                        Case eFilterTypes.Temperature : bUseModel = Me.IsInRange(strFilter, model.TempMean, model.TempMax)
                        Case eFilterTypes.Reference : bUseModel = (model.Reference.IndexOf(strFilter, 0, StringComparison.OrdinalIgnoreCase) >= 0)
                        Case eFilterTypes.ModelName : bUseModel = Me.StartsWith(strFilter, model.Name)
                    End Select
                End If

                If (bUseModel) Then
                    Me.m_lbxModels.Items.Add(model)
                    bKeepSelection = bKeepSelection Or (ReferenceEquals(model, Me.m_model))
                End If
            End If
        Next

        Me.m_lbxModels.EndUpdate()

        If (bKeepSelection) Then
            Me.m_lbxModels.SelectedItem = Me.m_model
        Else
            Me.m_lbxModels.SelectedIndex = -1
            Me.m_model = Nothing
        End If
        Me.UpdateControls()

    End Sub

    Private Function StartsWith(strFilter As String, strValue As String) As Boolean
        If (String.IsNullOrWhiteSpace(strValue)) Then Return False
        Return strValue.StartsWith(strFilter, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function IsInRange(strFilter As String, sMin As Single, sMax As Single) As Boolean

        Dim sVal As Single = 0
        If Not Single.TryParse(strFilter.Trim, sVal) Then Return False
        Return (sMin <= sVal) And (sVal <= sMax)

    End Function

    Private Sub UpdateEcoBaseLists()

        Dim lCountry As New List(Of String)
        Dim lEcoTyp As New List(Of String)

        For Each model As cModelData In Me.m_models
            If Not String.IsNullOrWhiteSpace(model.Country) Then lCountry.Add(model.Country)
            If Not String.IsNullOrWhiteSpace(model.EcosystemType) Then lEcoTyp.Add(model.EcosystemType)
        Next

        If (My.Settings.CountryNames IsNot Nothing) Then
            For Each str As String In My.Settings.CountryNames : lCountry.Add(str) : Next
        End If

        If (My.Settings.EcosystemTypes IsNot Nothing) Then
            For Each str As String In My.Settings.EcosystemTypes : lEcoTyp.Add(str) : Next
        End If

        Dim sgu As New cStyleGuideUpdater(Me.UIContext)
        sgu.Save()

        My.Settings.CountryNames.Clear()
        My.Settings.CountryNames.AddRange(lCountry.Distinct(StringComparer.OrdinalIgnoreCase).ToArray())

        My.Settings.EcosystemTypes.Clear()
        My.Settings.EcosystemTypes.AddRange(lEcoTyp.Distinct(StringComparer.OrdinalIgnoreCase).ToArray())

        sgu.Load()

        Me.StyleGuide.EcoBaseFieldsChanged()

    End Sub

    Private Function PeriodLabel() As String

        If (Me.m_model Is Nothing) Then Return ""
        If (Me.m_model.FirstYear = 0) Then Return "?"
        If (Me.m_model.NumYears <= 1) Then Return CStr(Me.m_model.FirstYear)
        Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_SPLIT, _
                                     Me.m_model.FirstYear, _
                                     Me.m_model.FirstYear + Math.Max(1, Me.m_model.NumYears) - 1)

    End Function

    Private Function LatLabel() As String

        ' ToDo: globalize this method

        Dim sg As cStyleGuide = Me.StyleGuide

        If (Me.m_model Is Nothing) Then Return ""
        If (Me.m_model.North = Me.m_model.South) Then Return SharedResources.GENERIC_VALUE_NONE
        Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_SPLIT, _
                                     cStringUtils.Localize("{0}N", sg.FormatNumber(Me.m_model.North)), _
                                     cStringUtils.Localize("{0}S", sg.FormatNumber(Me.m_model.South)))

    End Function

    Private Function LonLabel() As String

        ' ToDo: globalize this method

        Dim sg As cStyleGuide = Me.StyleGuide

        If (Me.m_model Is Nothing) Then Return ""
        If (Me.m_model.North = Me.m_model.South) Then Return SharedResources.GENERIC_VALUE_NONE
        Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_SPLIT, _
                                     cStringUtils.Localize("{0}W", sg.FormatNumber(Me.m_model.West)), _
                                     cStringUtils.Localize("{0}E", sg.FormatNumber(Me.m_model.East)))

    End Function

    Private Function RangeLabel(v1 As Single, v2 As Single) As String
        If (v1 = 0 And v2 = 0) Then Return "(not specified)"
        Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_SPLIT,
                                     cStringUtils.FormatNumber(v1),
                                     cStringUtils.FormatNumber(v2))
    End Function

    Private Function MeanLabel(v1 As Single, v2 As Single, vMean As Single) As String
        If (v1 = v2 And vMean = v1) Then Return "(not specified)"
        Return cStringUtils.FormatNumber(vMean)
    End Function

#End Region ' Internals

    Private Sub dlgEcobaseImport_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub m_rtfAgreement_TextChanged(sender As System.Object, e As System.EventArgs) Handles m_rtfAgreement.TextChanged

    End Sub
End Class