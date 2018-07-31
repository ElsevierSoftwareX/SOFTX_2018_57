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
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports System.IO
Imports EwEUtils.Core
Imports EwECore.SpatialData
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Utilities
Imports EwEUtils.SystemUtilities

#End Region

' ToDo: make interface work like proper options page (Cancel on Cancel, etc)

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control; implements the Options > Spatail temporal data interface.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucOptionsSpatialTemporal
        Implements IOptionsPage
        Implements IUIElement

#Region " Private vars "

        Private m_bDragOver As Boolean = False

#End Region ' Private vars

#Region " Constructors "

        Public Sub New(ByVal uic As cUIContext)

            Me.UIContext = uic
            Me.InitializeComponent()

        End Sub

#End Region ' Constructors

#Region " Event handlers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Control's load event which gets called every time the control gets loaded. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Dim core As cCore = Me.UIContext.Core
            Dim man As cSpatialDataSetManager = core.SpatialDataConnectionManager.DatasetManager

            Me.m_cbAllowIndexing.Checked = man.IsIndexingEnabled
            Me.AllowDrop = True

            Me.UpdateConfigFileList()

        End Sub

        Private Sub OnSelectDataset(sender As System.Object, e As System.EventArgs) _
            Handles m_btnSelect.Click, m_lvDatasets.DoubleClick

            If (Me.m_lvDatasets.SelectedItems.Count <> 1) Then Return

            Dim core As cCore = Me.UIContext.Core
            Dim man As cSpatialDataSetManager = core.SpatialDataConnectionManager.DatasetManager
            Dim cfg As cSpatialDataConfigFile = CType(Me.m_lvDatasets.SelectedItems(0).Tag, cSpatialDataConfigFile)
            Dim bSuccess As Boolean = False
            Dim strFileName As String = ""

            If (cfg IsNot Nothing) Then
                strFileName = cfg.FileName
            End If

            Try
                Me.UIContext.Core.SetBatchLock(cCore.eBatchLockType.Restructure)
                Try
                    bSuccess = man.Load(strFileName, True)
                Catch ex As Exception
                    bSuccess = False
                End Try
                core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecospace, bSuccess)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                cLog.Write(ex, "ucOptionsSpatialTemporal::Apply")
            End Try
            Me.UpdateConfigFileList()

        End Sub

        Private Sub OnAddFile(sender As System.Object, e As System.EventArgs) _
            Handles m_btnAdd.Click

            ' ToDo: globalize this

            If (Me.UIContext Is Nothing) Then Return

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmdFO As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)
            Dim core As cCore = Me.UIContext.Core
            Dim man As cSpatialDataSetManager = core.SpatialDataConnectionManager.DatasetManager

            cmdFO.Title = "Select data set file to add"
            cmdFO.Invoke(SharedResources.FILEFILTER_SPATTEMPCONFIG, 0)

            If (cmdFO.Result = DialogResult.OK) Then
                man.AddConfigFile(cmdFO.FileName)
                Me.UpdateConfigFileList()
            End If

        End Sub

        Private Sub OnRemoveFile(sender As System.Object, e As System.EventArgs) _
            Handles m_btnRemove.Click

            If (Me.m_lvDatasets.SelectedItems.Count <> 1) Then Return
            Dim cfg As cSpatialDataConfigFile = CType(Me.m_lvDatasets.SelectedItems(0).Tag, cSpatialDataConfigFile)
            Dim core As cCore = Me.UIContext.Core
            Dim man As cSpatialDataSetManager = core.SpatialDataConnectionManager.DatasetManager

            man.ConfigFileDefinitions.Remove(cfg)
            Me.UpdateConfigFileList()

        End Sub

        Private Sub OnNewFile(sender As System.Object, e As System.EventArgs) _
            Handles m_btnNew.Click

            ' ToDo: globalize this

            If (Me.UIContext Is Nothing) Then Return

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmdFS As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
            Dim core As cCore = Me.UIContext.Core
            Dim man As cSpatialDataSetManager = core.SpatialDataConnectionManager.DatasetManager

            cmdFS.Title = "Select data set file to create"
            cmdFS.Invoke(SharedResources.FILEFILTER_SPATTEMPCONFIG, 0)

            If (cmdFS.Result = DialogResult.OK) Then
                man.CreateConfigFile(cmdFS.FileName, Path.GetFileNameWithoutExtension(cmdFS.FileName), "")
                Me.UpdateConfigFileList()
            End If

        End Sub

        Private Sub OnViewConfigFile(sender As Object, e As EventArgs) _
            Handles m_btnViewConfig.Click
            Try
                If (Me.UIContext Is Nothing) Then Return
                If (Me.m_lvDatasets.SelectedItems.Count <> 1) Then Return

                Dim strFile As String = cSpatialDataSetManager.DefaultConfigFile
                Dim cfg As cSpatialDataConfigFile = CType(Me.m_lvDatasets.SelectedItems(0).Tag, cSpatialDataConfigFile)
                If (cfg IsNot Nothing) Then
                    strFile = cfg.FileName
                End If

                Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
                Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
                cmd.Invoke(Path.GetDirectoryName(strFile))

            Catch ex As Exception
                'ToDo: log this
            End Try
        End Sub

        Private Sub OnExportFile(sender As System.Object, e As System.EventArgs) _
            Handles m_btnExport.Click
            Try
                If (Me.UIContext Is Nothing) Then Return

                Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
                Dim cmd As cCommand = cmdh.GetCommand("ExportSpatialDatasets")
                cmd.Invoke()
                Me.UpdateConfigFileList()
            Catch ex As Exception
                'ToDo: log this
            End Try

        End Sub

        Protected Overrides Sub OnDragEnter(e As System.Windows.Forms.DragEventArgs)
            Try
                If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
                    Dim astrFiles As String() = CType(e.Data.GetData(DataFormats.FileDrop), String())
                    If (astrFiles.Length = 1) Then
                        e.Effect = DragDropEffects.All
                        Me.m_bDragOver = True
                    End If
                End If
            Catch ex As Exception
                Me.m_bDragOver = False
            End Try
        End Sub

        Protected Overrides Sub OnDragLeave(e As System.EventArgs)
            Me.m_bDragOver = False
            MyBase.OnDragLeave(e)
        End Sub

        Protected Overrides Sub OnDragDrop(e As System.Windows.Forms.DragEventArgs)
            Try
                If Not Me.m_bDragOver Then Return
                Dim core As cCore = Me.UIContext.Core
                Dim man As cSpatialDataSetManager = core.SpatialDataConnectionManager.DatasetManager
                For Each strFile As String In CType(e.Data.GetData(DataFormats.FileDrop), String())
                    man.AddConfigFile(strFile)
                Next
                Me.UpdateConfigFileList()
            Catch ex As Exception
            End Try
            Me.m_bDragOver = False
            Me.UpdateControls()
        End Sub

        Private Sub OnDatasetSelectionChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_lvDatasets.SelectedIndexChanged
            Me.UpdateControls()
        End Sub

        Private Sub OnViewCache(sender As System.Object, e As System.EventArgs) _
            Handles m_btnViewCache.Click

            If (Me.UIContext IsNot Nothing) Then
                Try
                    Dim cache As cSpatialDataCache = cSpatialDataCache.DefaultDataCache
                    Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
                    Dim cmd As cBrowserCommand = DirectCast(cmdh.GetCommand(cBrowserCommand.COMMAND_NAME), cBrowserCommand)
                    cmd.Invoke(cache.RootFolder)
                Catch ex As Exception
                    cLog.Write(ex, "ucOptionsSpatialTemporal::OnViewCache")
                End Try
            End If

        End Sub

        Private Sub OnClearCache(sender As System.Object, e As System.EventArgs) _
            Handles m_btnClearCache.Click

            If (Me.UIContext IsNot Nothing) Then
                Try
                    Dim man As cSpatialDataSetManager = Me.UIContext.Core.SpatialDataConnectionManager.DatasetManager
                    Dim cache As cSpatialDataCache = cSpatialDataCache.DefaultDataCache
                    Dim sg As cStyleGuide = Me.UIContext.StyleGuide
                    Dim core As cCore = Me.UIContext.Core
                    Dim lSizeBefore As Long = cache.GetSize()
                    Dim lSizeUnused As Long = cache.GetUnusedSize(man)
                    Dim strPrompt As String = My.Resources.PROMPT_CACHE_CLEAR

                    If (lSizeUnused > 0) Then
                        Dim fmsg As New cFeedbackMessage(cStringUtils.Localize(strPrompt, sg.FormatMemory(lSizeBefore), sg.FormatMemory(lSizeUnused)), _
                                                         eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO_CANCEL)
                        core.Messages.SendMessage(fmsg)

                        Select Case fmsg.Reply
                            Case eMessageReply.YES
                                cache.Clear(man)
                            Case eMessageReply.NO
                                cache.Clear()
                            Case eMessageReply.CANCEL
                                Return
                        End Select
                    Else
                        cache.Clear()
                    End If

                    Dim msg As New cMessage(cStringUtils.Localize(My.Resources.STATUS_CACHECLEARED, sg.FormatMemory(lSizeBefore - cache.GetSize())), _
                         eMessageType.Any, EwEUtils.Core.eCoreComponentType.External, eMessageImportance.Information)
                    core.Messages.SendMessage(msg)

                    Me.UpdateControls()
                Catch ex As Exception
                    cLog.Write(ex, "ucOptionsSpatialTemporal::OnClearCache")
                End Try
            End If

        End Sub

        Protected Overrides Sub OnResize(e As System.EventArgs)
            MyBase.OnResize(e)
            Me.UpdateControls()
        End Sub

#End Region ' Event handlers

#Region " Public methods "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext As cUIContext _
                 Implements IUIElement.UIContext

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanApply"/>
        ''' -------------------------------------------------------------------
        Public Function CanApply() As Boolean _
              Implements IOptionsPage.CanApply
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.OnChanged"/>
        ''' -------------------------------------------------------------------
        Public Event OnOptionsPedigreeChanged(sender As IOptionsPage, args As System.EventArgs) _
              Implements IOptionsPage.OnChanged

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.Apply"/>
        ''' -------------------------------------------------------------------
        Public Function Apply() As IOptionsPage.eApplyResultType _
            Implements IOptionsPage.Apply

            Dim core As cCore = Me.UIContext.Core
            Dim man As cSpatialDataSetManager = core.SpatialDataConnectionManager.DatasetManager
            Dim strFile As String = ""
            Dim bSuccess As Boolean = True

            If Not Me.CanApply Then Return IOptionsPage.eApplyResultType.Failed

            'Try

            'Me.UIContext.Core.SetBatchLock(cCore.eBatchLockType.Restructure)
            'Try
            '    bSuccess = man.Load(strFile, True)
            'Catch ex As Exception
            '    bSuccess = False
            'End Try
            'core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecospace, bSuccess)
            'Catch ex As Exception
            '    Debug.Assert(False, ex.Message)
            '    cLog.Write(ex, "ucOptionsSpatialTemporal::Apply")
            'End Try

            man.IsIndexingEnabled = Me.m_cbAllowIndexing.Checked

            If bSuccess Then Return IOptionsPage.eApplyResultType.Success
            Return IOptionsPage.eApplyResultType.Failed

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.SetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Sub SetDefaults() _
                Implements IOptionsPage.SetDefaults

            Try
                Me.m_cbAllowIndexing.Checked = False
            Catch ex As Exception

            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanSetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Function CanSetDefaults() As Boolean _
            Implements IOptionsPage.CanSetDefaults
            Return True
        End Function

#End Region ' Public methods

#Region " Internals "

        Private Sub UpdateConfigFileList()

            ' ToDo: replace listview by SourceGrid

            Dim lvi As ListViewItem = Nothing
            Dim core As cCore = Me.UIContext.Core
            Dim man As cSpatialDataSetManager = core.SpatialDataConnectionManager.DatasetManager
            Dim strItem As String = ""

            Me.m_lvDatasets.Items.Clear()
            lvi = Me.m_lvDatasets.Items.Add(SharedResources.GENERIC_VALUE_DEFAULT)
            If (man.CurrentConfigFile = cSpatialDataSetManager.DefaultConfigFile) Then
                lvi.SubItems.Add(SharedResources.GENERIC_VALUE_YES)
            Else
                lvi.SubItems.Add("")
            End If
            lvi.SubItems.Add(SharedResources.GENERIC_VALUE_YOU)
            lvi.SubItems.Add("")
            lvi.SubItems.Add(cSpatialDataSetManager.DefaultConfigFile)
            lvi.Tag = Nothing

            For Each cfg As cSpatialDataConfigFile In man.ConfigFileDefinitions
                lvi = Me.m_lvDatasets.Items.Add(Me.ToDefaultString(cfg.DatasetName))
                If (man.CurrentConfigFile = cfg.FileName) Then
                    lvi.SubItems.Add(SharedResources.GENERIC_VALUE_YES)
                Else
                    lvi.SubItems.Add("")
                End If
                lvi.SubItems.Add(Me.ToDefaultString(cfg.Author, cfg.Station))
                lvi.SubItems.Add(Me.ToDefaultString(cfg.Contact))
                lvi.SubItems.Add(cfg.FileName)
                lvi.Tag = cfg
            Next

            Me.m_lvDatasets.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent)

            Me.UpdateControls()

        End Sub

        Private Function ToDefaultString(ByVal strIn As String) As String
            If String.IsNullOrWhiteSpace(strIn) Then Return SharedResources.GENERIC_VALUE_NOTSET
            Return strIn
        End Function

        Private Function ToDefaultString(strA As String, strB As String) As String
            If Not String.IsNullOrWhiteSpace(strA) And Not String.IsNullOrWhiteSpace(strB) Then
                Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_A_AT_B, strA, strB)
            End If
            If Not String.IsNullOrWhiteSpace(strA) Then Return strA
            If Not String.IsNullOrWhiteSpace(strB) Then Return strB
            Return SharedResources.GENERIC_VALUE_NOTSET
        End Function

        Private Sub UpdateControls()

            If (Me.UIContext Is Nothing) Then Return

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Dim cache As cSpatialDataCache = cSpatialDataCache.DefaultDataCache
            Dim man As cSpatialDataSetManager = Me.UIContext.Core.SpatialDataConnectionManager.DatasetManager
            Dim bHasSelection As Boolean = False
            Dim bIsCurrent As Boolean = False
            Dim bHasCustomSelection As Boolean = False

            If (Me.m_lvDatasets.SelectedIndices.Count = 1) Then
                bHasSelection = True
                bHasCustomSelection = (Me.m_lvDatasets.SelectedIndices(0) > 0)

                Dim item As ListViewItem = Me.m_lvDatasets.SelectedItems(0)

                If (item.Tag Is Nothing) Then
                    bIsCurrent = (man.CurrentConfigFile = cSpatialDataSetManager.DefaultConfigFile)
                Else
                    Dim cfg As cSpatialDataConfigFile = CType(item.Tag, cSpatialDataConfigFile)
                    bIsCurrent = (man.CurrentConfigFile = cfg.FileName)
                End If
            End If

            Me.m_btnSelect.Enabled = bHasSelection And Not bIsCurrent
            Me.m_btnRemove.Enabled = bHasCustomSelection
            Me.m_btnExport.Enabled = (man.Datasets.Count > 0)
            Me.m_btnViewConfig.Enabled = bHasSelection

            Me.m_lblCacheLocationValue.Text = cStringUtils.CompactString(cache.RootFolder, Me.m_lblCacheLocationValue.ClientSize.Width, Me.Font)
            Me.m_lblCacheSizeValue.Text = cStringUtils.Localize(My.Resources.GENERIC_VALUE_CACHEMEMORY, _
                                                        sg.FormatMemory(cache.GetSize()), _
                                                        sg.FormatMemory(cache.GetUnusedSize(man)))

            Me.m_btnViewCache.Enabled = Directory.Exists(cache.RootFolder)
            Me.m_btnClearCache.Enabled = (cache.GetSize() > 0)

        End Sub

#End Region ' Internals

    End Class

End Namespace


