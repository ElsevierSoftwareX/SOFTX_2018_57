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
Imports EwECore.SpatialData
Imports EwEPlugin
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

' ToDo: add check on delete if dataset is applied in this model. Could also be applied to other models, have no idea
' ToDo: add indexing overview
' ToDo: add cache overview

Namespace Ecospace.Controls

    ''' <summary>
    ''' Dialog for defining Ecospace spatial temporal datasets
    ''' </summary>
    Public Class dlgDefineExternalSpatialData

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_man As cSpatialDataConnectionManager = Nothing
        Private m_manSets As cSpatialDataSetManager = Nothing
        Private m_bHasDatasetTemplates As Boolean = False

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#End Region ' Constructor

#Region " Form overrides "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Overrides Property UIContext As cUIContext
            Get
                Return Me.m_uic
            End Get
            Set(uic As cUIContext)

                If (Me.m_uic IsNot Nothing) Then
                    ' Disconnect from data objects first; we do not want disconnecting UI elements from screwing up the last configuration
                    Me.m_gridDatasets.UIContext = Nothing
                    Me.m_manSets = Nothing
                    Me.m_man = Nothing
                End If

                Me.m_uic = uic

                If (Me.m_uic IsNot Nothing) Then
                    ' Set new
                    Me.m_man = Me.m_uic.Core.SpatialDataConnectionManager
                    Me.m_manSets = Me.m_man.DatasetManager
                    Me.m_gridDatasets.UIContext = Me.m_uic
                End If
            End Set
        End Property

        Protected Overrides Sub OnLoad(e As System.EventArgs)

            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_btnExport.Image = ScientificInterfaceShared.My.Resources.ExportHS

            If (Me.UIContext Is Nothing) Then Return

            Me.m_cbEnableIndexing.Checked = Me.m_manSets.IsIndexingEnabled

            AddHandler Me.m_gridDatasets.OnSelectionChanged, AddressOf OnGridSelectionChanged

            Me.CenterToParent()
            Me.Reload()
            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)
            RemoveHandler Me.m_gridDatasets.OnSelectionChanged, AddressOf OnGridSelectionChanged
            Me.UIContext = Nothing
            MyBase.OnFormClosed(e)
        End Sub

        Private Sub Reload()
            Me.FillTemplateDatasetDropdown()
            Me.m_gridDatasets.Fill()
        End Sub

        Protected Overrides Sub UpdateControls()
            MyBase.UpdateControls()

            Dim ds As ISpatialDataSet = Me.m_gridDatasets.SelectedDataset

            Dim bHasTemplate As Boolean = Me.m_bHasDatasetTemplates
            Dim bHasDS As Boolean = (Me.m_gridDatasets.RowsCount > 1)
            Dim bHasSelection As Boolean = (ds IsNot Nothing)
            Dim bCanConfig As Boolean = (TypeOf ds Is IConfigurable)

            Me.m_cmbTemplates.Enabled = bHasTemplate
            Me.m_btnCreate.Enabled = bHasTemplate
            Me.m_btnConfigure.Enabled = bHasSelection And bCanConfig
            Me.m_btnDelete.Enabled = bHasSelection
            Me.m_btnExport.Enabled = bHasDS

            Me.m_lblConfigValue.Text = cStringUtils.CompactString(Me.m_manSets.CurrentConfigFile, Me.m_lblConfigValue.ClientSize.Width, Me.Font)

        End Sub

        Protected Overrides Sub OnResizeEnd(e As System.EventArgs)
            MyBase.OnResizeEnd(e)
            Me.UpdateControls()
        End Sub

#End Region ' Form overrides

#Region " Event handlers "

        Private Sub OnCreateDataset(sender As System.Object, e As System.EventArgs) _
            Handles m_btnCreate.Click

            Me.Cursor = Cursors.WaitCursor
            Try
                Dim ds As ISpatialDataSet = DirectCast(Me.m_cmbTemplates.SelectedItem, ISpatialDataSet)
                Me.CreateDS(ds)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
            Me.Cursor = Cursors.Default
            Me.UpdateControls()
        End Sub

        Private Sub OnDeleteDS(sender As System.Object, e As System.EventArgs) _
            Handles m_btnDelete.Click
            Try
                Me.Delete(Me.m_gridDatasets.SelectedDatasets)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
            Me.UpdateControls()
        End Sub

        Private Sub OnExport(sender As System.Object, e As System.EventArgs) _
            Handles m_btnExport.Click
            Try
                Dim dlg As New dlgExportSpatialData(Me.UIContext)
                dlg.ShowDialog(Me)
            Catch ex As Exception
                cLog.Write(ex, "dlgDefineExternalSpatialData::OnExportSelected")
            End Try
        End Sub

        ''' <summary>
        ''' User wants to configure the currently selected dataset.
        ''' </summary>
        Private Sub OnConfigDS(sender As System.Object, e As System.EventArgs) _
            Handles m_btnConfigure.Click

            Me.Cursor = Cursors.WaitCursor
            Try
                Me.ConfigDS(Me.SelectedDataset)
                Me.m_gridDatasets.Fill(Me.SelectedDataset)
                Me.m_manSets.IndexDataset = Me.SelectedDataset
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                cLog.Write(ex, "dlgDefineExternalSpatialData::OnConfigureDS")
            End Try
            Me.Cursor = Cursors.Default

        End Sub

        Private Sub OnGridSelectionChanged()
            Try
                Me.m_manSets.IndexDataset = Me.SelectedDataset
                Me.UpdateControls()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnEnableIndexingChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_cbEnableIndexing.CheckedChanged
            Try
                Me.m_manSets.IsIndexingEnabled = Me.m_cbEnableIndexing.Checked
                Me.m_manSets.IndexDataset = Me.SelectedDataset
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnOK(sender As System.Object, e As System.EventArgs) _
            Handles m_btnOK.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.m_manSets.Save()
            Me.Close()
        End Sub

        Private Sub OnManageConfigurations(sender As System.Object, e As System.EventArgs) _
            Handles m_btnManageConfigurations.Click
            Try
                Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("ManageSpatialDatasetConfigurations")
                cmd.Invoke()
                Me.Reload()
            Catch ex As Exception
                cLog.Write(ex, "dlgDefineExternalSpatialData.OnSwitchConfig")
            End Try
        End Sub

#End Region ' Event handlers 

#Region " Internals "

        Private ReadOnly Property Varname As eVarNameFlags
            Get
                Return eVarNameFlags.NotSet
            End Get
        End Property

        Private Property SelectedDataset As ISpatialDataSet
            Get
                Return Me.m_gridDatasets.SelectedDataset
            End Get
            Set(dataset As ISpatialDataSet)
                Me.m_gridDatasets.SelectedDataset = dataset
                Me.UpdateControls()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Fill UI with available dataset templates
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub FillTemplateDatasetDropdown()

            Me.m_bHasDatasetTemplates = False

            Me.m_cmbTemplates.Items.Clear()
            For Each ds As ISpatialDataSet In Me.m_man.DatasetTemplates
                Me.m_cmbTemplates.Items.Add(ds)
                Me.m_bHasDatasetTemplates = True
            Next

            If (Not Me.m_bHasDatasetTemplates) Then
                Me.m_cmbTemplates.Items.Add(My.Resources.VALUE_NOTEMPLATES)
            End If
            Me.m_cmbTemplates.SelectedIndex = 0

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a new spatial data set
        ''' </summary>
        ''' <param name="dsTemplate">The template to create a new dataset from.</param>
        ''' -------------------------------------------------------------------
        Private Sub CreateDS(dsTemplate As ISpatialDataSet)

            Dim dsNew As ISpatialDataSet = Nothing

            If (dsTemplate Is Nothing) Then Return

            dsNew = CType(Activator.CreateInstance(dsTemplate.GetType()), ISpatialDataSet)
            If (dsNew Is Nothing) Then Return

            If (TypeOf dsNew Is IPlugin) Then
                DirectCast(dsNew, IPlugin).Initialize(Me.m_uic.Core)
            End If

            Try
                dsNew.VarName = Me.Varname
                If Me.ConfigDS(dsNew) Then
                    Me.m_manSets.Add(dsNew)
                    Me.m_gridDatasets.Fill(dsNew)
                    Me.m_manSets.IndexDataset = dsNew
                End If
            Catch ex As Exception
                cLog.Write(ex, "dlgDefineExternalSpatialData::CreateDS")
            End Try

        End Sub

        ''' <summary>
        ''' Configure a dataset.
        ''' </summary>
        ''' <param name="ds"></param>
        ''' <returns>True if successful.</returns>
        Private Function ConfigDS(ds As ISpatialDataSet) As Boolean

            If (ds Is Nothing) Then Return True
            If (Not TypeOf ds Is IConfigurable) Then Return True

            If (TypeOf ds Is IPlugin) Then
                DirectCast(ds, IPlugin).Initialize(Me.m_uic.Core)
            End If

            Dim dsConf As IConfigurable = DirectCast(ds, IConfigurable)
            Dim ctrl As Control = dsConf.GetConfigUI()

            If (ctrl Is Nothing) Then Return dsConf.IsConfigured

            Dim dlg As New dlgConfig(Me.UIContext)
            If (dlg.ShowDialog(Me.FindForm, My.Resources.CAPTION_EXTERNAL_DATASET_CONFIGURE, ctrl) = System.Windows.Forms.DialogResult.OK) Then
                Me.m_man.Update(ds)
            End If
            Return (dsConf.IsConfigured)

        End Function

        Private Function Delete(sets As ISpatialDataSet()) As Boolean

            If (sets Is Nothing) Then Return False
            If (sets.Length = 0) Then Return False

            ' ToDo: globalize this
            Dim fmsg As New cFeedbackMessage("This operation cannot be undone. Are you sure?", _
                                             eCoreComponentType.EcoSpace, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
            fmsg.Reply = eMessageReply.NO
            Me.m_uic.Core.Messages.SendMessage(fmsg)
            If (fmsg.Reply = eMessageReply.NO) Then Return False

            Me.m_manSets.AllowValidation = False
            For Each ds As ISpatialDataSet In sets
                Me.m_manSets.Remove(ds)
            Next
            Me.m_manSets.AllowValidation = True

            Me.SelectedDataset = Nothing

            Me.m_gridDatasets.Fill()
            Me.UpdateControls()

        End Function

#End Region ' Internals 

    End Class

End Namespace
