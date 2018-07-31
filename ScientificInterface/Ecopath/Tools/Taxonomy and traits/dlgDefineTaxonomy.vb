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
Imports System.Windows.Forms
Imports EwECore
Imports EwECore.ExternalData
Imports EwEUtils.Core
Imports EwEPlugin
Imports EwEPlugin.Data
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Dialog class implementing the Edit Group Taxon interface.
''' </summary>
''' <remarks>
''' <para>Note that this class breaks the EwE/Core/Plugin convention that all 
''' interaction with plug-ins should happen via the plug-in manager!</para>
''' <para>This class directly interfaces wit the plug-in manager to find taxonomy 
''' data producing search engines. These plug-ins are directly called to execute 
''' searches and to scoop up search results. This is all very neat, but this 
''' means that the core will not be able to use this behaviour at all.</para>
''' <para>This crucial behavour should probably be contained within a core class 
''' called 'cDataSearchManager(Of T)'. This will yield generic behaviour that can
''' be used for other purposes at core level.</para>
''' </remarks>
''' ===========================================================================
Public Class dlgDefineTaxonomy

#Region " Private vars "

    ''' <summary>UI context to connect to.</summary>
    Private m_uic As cUIContext = Nothing
    ''' <summary>Datasource delivering taxonomy data.</summary>
    Private m_tds As cTaxonDataSource = Nothing
    ''' <summary>Looped update prevention flag.</summary>
    Private m_bInUpdate As Boolean = False
    ''' <summary>Flag stating whether search engines were found.</summary>
    Private m_bHasSearchEngines As Boolean = False
    ''' <summary>Start up group.</summary>
    Private m_groupStartup As cEcoPathGroupInput = Nothing

#End Region ' Private vars

#Region " Private classes "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class for listing data producers in a combo box.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Class cDataProducerSearchItem

        Private m_prod As IDataSearchProducerPlugin = Nothing

        Public Sub New(ByVal prod As IDataSearchProducerPlugin)
            Me.m_prod = prod
        End Sub

        Public ReadOnly Property Producer() As IDataSearchProducerPlugin
            Get
                Return Me.m_prod
            End Get
        End Property

        Public Overrides Function ToString() As String
            If Me.m_prod IsNot Nothing Then Return Me.m_prod.Name
            Return SharedResources.GENERIC_VALUE_NOSEARCHENGINES
        End Function

    End Class

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class to wait for search results to be formatted and delivered.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Class cWaitForSearch

        ''' <summary>UI to notify when search complete.</summary>
        Private m_ui As dlgDefineTaxonomy = Nothing
        ''' <summary>Data producer that is searching.</summary>
        Private m_producer As IDataSearchProducerPlugin = Nothing
        ''' <summary>Search results.</summary>
        Private m_results As IDataSearchResults = Nothing

        Public Sub New(ByVal form As dlgDefineTaxonomy, ByVal prod As IDataSearchProducerPlugin, ByVal res As IDataSearchResults)
            Me.m_ui = form
            Me.m_producer = prod
            Me.m_results = res
        End Sub

        Public Sub Wait()
            While m_producer.IsSeaching
                ' NOP
            End While
            Me.m_ui.OnProcessSearchResults(Me.m_results)
        End Sub

    End Class

#End Region ' Private classes

#Region " Constructor "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance of this class.
    ''' </summary>
    ''' <param name="uic">The <see cref="cUIContext">UI context</see> to connect to.</param>
    ''' -------------------------------------------------------------------
    Public Sub New(ByVal uic As cUIContext)

        Me.InitializeComponent()
        Me.m_uic = uic

        Me.m_gridGroups.UIContext = uic
        'Me.m_gridResults.UIContext = uic
        Me.m_gridResults.Init(Me.m_uic, New gridTaxonSearchResults.IsTaxonUsedDelegate(AddressOf OnIsTaxonUsed))

    End Sub

#End Region ' Constructor

#Region " Form overrides "

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        Me.m_tds = cTaxonDataSource.GetInstance()

        Me.PopulateTaxonDataProducerControls()
        Me.UpdateControls()

        ' Connect to group grid selection changes
        AddHandler Me.m_gridGroups.OnSelectionChanged, AddressOf OnRowSelectionChanged
        ' Connect to search result changes
        AddHandler Me.m_tds.OnTaxonSearchResults, AddressOf OnProcessResults
        ' Connect to result selection changes
        AddHandler Me.m_gridResults.OnResultSelected, AddressOf OnResultSelected

        If (Me.m_groupStartup Is Nothing) Then Me.m_groupStartup = Me.m_uic.Core.EcoPathGroupInputs(1)

        Me.m_pbSearching.Image = SharedResources.ani_loader

        Me.UpdateEngineCapabilities()
        Me.UpdateControls()

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        RemoveHandler Me.m_gridGroups.OnSelectionChanged, AddressOf OnRowSelectionChanged
        RemoveHandler Me.m_tds.OnTaxonSearchResults, AddressOf OnProcessResults
        RemoveHandler Me.m_gridResults.OnResultSelected, AddressOf OnResultSelected

        MyBase.OnFormClosed(e)
    End Sub

#End Region ' Form overrides

#Region " Events "

    Private Sub OnShowCodes(sender As System.Object, e As System.EventArgs) _
        Handles m_cbShowCodes.CheckedChanged
        Me.Cursor = Cursors.WaitCursor
        Try
            Me.m_gridGroups.ShowCodes = Me.m_cbShowCodes.Checked
            Me.m_gridResults.ShowCodes = Me.m_cbShowCodes.Checked
        Catch ex As Exception

        End Try
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub OnRowSelectionChanged()
        Me.UpdateControls()
    End Sub

    Private Sub OnResultSelected(ByVal result As Object)

        If (result Is Nothing) Then Return
        If Not (TypeOf result Is ITaxonSearchData) Then Return
        Me.m_gridGroups.AddTaxon(DirectCast(result, ITaxonSearchData))

    End Sub

    Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles OK_Button.Click
        Try
            If Me.m_gridGroups.Apply Then
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()
            End If
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::OnOK")
        End Try
    End Sub

    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub OnDefineNew(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnDefine.Click
        Try
            Me.m_gridGroups.AddTaxon()
            Me.m_gridResults.OnUsedTaxaChanged()
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::m_btnAdd_Click")
        End Try
        Me.UpdateControls()
    End Sub

    Private Sub OnRemoveSelected(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnRemove.Click
        Try
            Me.m_gridGroups.DeleteRows(True)
            Me.m_gridResults.OnUsedTaxaChanged()
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::m_btnRemove_Click")
        End Try
        Me.UpdateControls()
    End Sub

    Private Sub OnKeepSelected(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnKeep.Click
        Try
            Me.m_gridGroups.DeleteRows(False)
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::m_btnKeep_Click")
        End Try
        Me.UpdateControls()
    End Sub

    Private Sub OnNormalizeProportions(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnProps.Click
        Try
            Me.m_gridGroups.NormalizeProportions()
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::m_btnProps_Click")
        End Try
    End Sub

    'Private Sub OnUpdateCurrent(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    '    Handles m_btnUpdate.Click
    '    ' Hmm
    'End Sub

    'Private Sub OnUpdateAll(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    '    Handles m_btnUpdateAll.Click
    '    ' Hmm
    'End Sub

    Private Sub OnSearchTextChanged(ByVal sender As System.Object, ByVal e As EventArgs) _
        Handles m_tbxSearchTerm.TextChanged
        Try
            Me.RefreshSearch()
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::OnSearchTextChanged")
        End Try
    End Sub

    Private Sub OnIncludeExtentChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles m_cbIncludeExtent.CheckedChanged
        Try
            Me.RefreshSearch()
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::m_cbIncludeExtent_CheckedChanged")
        End Try
    End Sub

    Private Sub OnResultSelected() _
        Handles m_gridResults.OnSelectionChanged
        Try
            Me.BeginInvoke(New MethodInvoker(AddressOf UpdateControls))
        Catch ex As Exception

        End Try
    End Sub

    Private Sub OnConnect(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnConfigure.Click
        Try
            Me.ConfigureSelectedDataProducer()
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::OnConnect")
        End Try
    End Sub

    Private Sub OnSourceChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_cmbEngine.SelectedIndexChanged

        If (Me.m_bInUpdate) Then Return
        Me.UpdateEngineCapabilities()
        Me.UpdateControls()

    End Sub

    Private m_wait As cWaitForSearch = Nothing

    Private Sub OnProcessResults(ByVal results As IDataSearchResults)

        ' Ignore search terms of different data types
        If Not TypeOf results.SearchTerm Is ITaxonSearchData Then Return

        ' Process results async
        Me.m_wait = New cWaitForSearch(Me, Me.SelectedDataProducer, results)

        ' Handle this in a separate thread to allow the search to complete without stalling
        Dim thr As New Threading.Thread(AddressOf Me.m_wait.Wait)
        thr.Start()

    End Sub

    Protected Delegate Sub OnProcessSearchResultsDelegate(ByVal results As IDataSearchResults)

    Friend Sub OnProcessSearchResults(ByVal results As IDataSearchResults)

        If Me.InvokeRequired Then
            Me.Invoke(New OnProcessSearchResultsDelegate(AddressOf ProcessSearchResults), New Object() {results})
        Else
            Me.ProcessSearchResults(results)
        End If

    End Sub

    Private Sub ProcessSearchResults(ByVal results As IDataSearchResults)

        Debug.Assert(Not Me.InvokeRequired)

        Me.Cursor = Cursors.WaitCursor
        Try
            Me.UpdateControls()
            Me.m_gridResults.AddResults(results)
        Catch ex As Exception

        End Try
        Me.Cursor = Cursors.Default

    End Sub

    Private Sub OnTaxonLevelFormat(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
        Handles m_cmbFilter.Format
        Dim fmt As New cTaxonClassificationTypeFormatter()
        e.Value = fmt.GetDescriptor(e.ListItem)
    End Sub

    Private Sub OnTaxonLevelSelected(sender As Object, e As System.EventArgs) _
        Handles m_cmbFilter.SelectedIndexChanged
        Try
            Me.RefreshSearch()
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::OnTaxonLevelSelected")
        End Try
    End Sub

    Private Sub m_btnAdd_Click(sender As Object, e As System.EventArgs) _
        Handles m_btnAdd.Click

        Try
            For Each i As Integer In Me.m_gridResults.SelectedRows
                Dim t As ITaxonSearchData = Me.m_gridResults.TaxonAtRow(i)
                If (t IsNot Nothing) Then
                    Me.m_gridGroups.AddTaxon(t)
                End If
            Next
        Catch ex As Exception
            cLog.Write(ex, "dlgDefineTaxa::m_btnAdd_Click")
        End Try

    End Sub

#End Region ' Events

#Region " Internals "

    Private Sub UpdateControls()

        Dim bCanSearch As Boolean = False
        Dim bCanConfig As Boolean = False
        Dim bIsSearching As Boolean = False
        Dim bHasResult As Boolean = (Me.m_gridResults.TaxonAtRow(-1) IsNot Nothing)

        Me.m_bInUpdate = True

        ' == Manipulation controls ==
        Try
            Me.m_btnDefine.Enabled = Me.m_gridGroups.CanAddTaxon(Nothing)
            Me.m_btnRemove.Enabled = Me.m_gridGroups.CanDeleteTaxon() And Not Me.m_gridGroups.IsFlaggedForDeletionRow()
            Me.m_btnKeep.Enabled = Me.m_gridGroups.IsFlaggedForDeletionRow()
            Me.m_btnAdd.Enabled = Me.m_gridGroups.CanAddTaxon(DirectCast(Me.m_gridResults.TaxonAtRow, ITaxonSearchData)) And bHasResult
        Catch ex As Exception

        End Try

        ' == SEARCH ==

        Me.m_scMain.Panel2Collapsed = (Not Me.m_bHasSearchEngines)

        If (Me.m_bHasSearchEngines) Then
            Dim prod As IDataProducerPlugin = Me.SelectedDataProducer
            If (prod IsNot Nothing) Then
                bCanSearch = (TypeOf prod Is IDataSearchProducerPlugin) And (prod.IsDataAvailable(GetType(ITaxonSearchData)))
                If (TypeOf prod Is IConfigurablePlugin) Then
                    Try
                        bCanSearch = bCanSearch And DirectCast(prod, IConfigurablePlugin).IsConfigured
                    Catch ex As Exception
                        Debug.Assert(False, ex.Message)
                    End Try
                End If
                bIsSearching = bIsSearching Or (DirectCast(prod, IDataSearchProducerPlugin).IsSeaching)
                bCanConfig = (TypeOf prod Is IConfigurablePlugin)
            End If
        End If

        ' Config search controls
        Me.m_cmbEngine.Enabled = Me.m_bHasSearchEngines
        Me.m_btnConfigure.Enabled = bCanConfig
        Me.m_lblSearchTerm.Enabled = bCanSearch
        Me.m_tbxSearchTerm.Enabled = bCanSearch
        Me.m_lblIn.Enabled = bCanSearch
        Me.m_gridResults.Enabled = bCanSearch

        Me.m_pbSearching.Visible = bIsSearching
        Me.m_bInUpdate = False

    End Sub

    Private Sub UpdateEngineCapabilities()

        Dim engine As IDataSearchProducerPlugin = Me.SelectedDataProducer
        Dim taxacaps As eTaxonClassificationType = eTaxonClassificationType.Latin
        Dim bSpatialCaps As Boolean = False

        If (engine IsNot Nothing) Then
            ' Does the engine report specific search capabilities?
            If (TypeOf engine Is ITaxonSearchCapabilities) Then
                ' #Yes: Report capabilities
                Dim caps As ITaxonSearchCapabilities = CType(engine, ITaxonSearchCapabilities)
                taxacaps = caps.TaxonSearchCapabilities
                bSpatialCaps = caps.HasSpatialSearchCapabilities
            End If

            Me.m_cmbFilter.Items.Clear()
            For Each test As eTaxonClassificationType In [Enum].GetValues(GetType(eTaxonClassificationType))
                If (taxacaps And test) = test Then
                    Me.m_cmbFilter.Items.Add(test)
                End If
            Next
            Me.m_cmbFilter.SelectedIndex = 0
        End If

        Me.m_cbIncludeExtent.Enabled = bSpatialCaps

    End Sub

    Private Sub PopulateTaxonDataProducerControls()

        Dim pm As cPluginManager = Me.m_uic.Core.PluginManager
        Dim pi As IPlugin = Nothing
        Dim dpi As IDataSearchProducerPlugin = Nothing
        Dim coll As ICollection(Of IPlugin) = Nothing

        Me.m_cmbEngine.Items.Clear()
        Me.m_bHasSearchEngines = False

        If (pm Is Nothing) Then Return

        coll = pm.GetPlugins(GetType(IDataSearchProducerPlugin))

        ' Only show data producers that provide taxon data
        For Each pi In coll
            Try
                dpi = DirectCast(pi, IDataSearchProducerPlugin)
                If (dpi.IsDataAvailable(GetType(ITaxonSearchData))) Then
                    Me.m_cmbEngine.Items.Add(New cDataProducerSearchItem(dpi))
                    Me.m_bHasSearchEngines = True
                End If
            Catch ex As Exception

            End Try
        Next

        If Not Me.m_bHasSearchEngines Then
            Me.m_cmbEngine.Items.Add(New cDataProducerSearchItem(Nothing))
        End If

        Me.m_cmbEngine.SelectedIndex = 0

    End Sub

    Private ReadOnly Property SelectedDataProducer() As IDataSearchProducerPlugin
        Get
            Dim item As cDataProducerSearchItem = DirectCast(Me.m_cmbEngine.SelectedItem, cDataProducerSearchItem)
            If item Is Nothing Then Return Nothing
            Return item.Producer
        End Get
    End Property

    Private Sub ConfigureSelectedDataProducer()

        Dim prod As IDataSearchProducerPlugin = Me.SelectedDataProducer
        Dim ui As Control = Nothing
        If (prod Is Nothing) Then Return
        If Not (TypeOf prod Is IConfigurablePlugin) Then Return

        Try
            ui = DirectCast(prod, IConfigurablePlugin).GetConfigUI()
        Catch ex As Exception
            ui = Nothing
        End Try

        If (ui Is Nothing) Then Return

        Dim dlg As New dlgConfig(Me.m_uic)
        dlg.ShowDialog(cStringUtils.Localize("Configuring {0}", ui.Text), ui)

        Me.UpdateControls()

    End Sub

    Private Sub ApplyTaxon(ByVal taxon As ITaxonSearchData)
        Me.m_gridGroups.UpdateSelectedTaxon(taxon)
        Me.m_gridGroups.UpdateSelectedTaxonRow()
        Me.UpdateControls()
    End Sub

    Private Sub RefreshSearch()
        Me.Search(Me.m_tbxSearchTerm.Text)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Search for a textual search term.
    ''' </summary>
    ''' <param name="strTerm">The text to search for.</param>
    ''' -----------------------------------------------------------------------
    Private Sub Search(ByVal strTerm As String)

        ' No term? Abort
        If (String.IsNullOrWhiteSpace(strTerm)) Then Return

        ' Prune term
        strTerm = strTerm.Trim()

        ' Term less than 4 chars? Abort
        If (strTerm.Length < 4) Then Return

        ' Make search term
        Dim objTerm As Object = Me.SelectedDataProducer.CreateSearchTerm()
        ' No valid term? Abort
        If Not (TypeOf objTerm Is ITaxonSearchData) Then Return

        ' Create search term
        Dim searchterm As ITaxonSearchData = DirectCast(objTerm, ITaxonSearchData)
        ' Successful?
        If searchterm IsNot Nothing Then
            '#Yes: populate term
            searchterm.SearchFields = DirectCast(Me.m_cmbFilter.SelectedItem, eTaxonClassificationType)
            searchterm.Common = strTerm
            ' Go Jimmy
            Me.Search(searchterm)
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Search for a <see cref="ITaxonSearchData">taxonomoy data</see> search term.
    ''' </summary>
    ''' <param name="term">The <see cref="ITaxonSearchData">taxonomoy data</see> 
    ''' search term to search for.</param>
    ''' -----------------------------------------------------------------------
    Private Sub Search(ByVal term As ITaxonSearchData)

        If (Me.SelectedDataProducer Is Nothing) Then Return

        Try
            Dim taxonSearch As ITaxonSearchData = Me.m_gridGroups.GetSearchTerm(term)

            ' Clear search key to initiate a full search
            taxonSearch.SourceKey = ""

            ' Set bounding box if necessary
            If Me.m_cbIncludeExtent.Checked Then
                Dim model As cEwEModel = Me.m_uic.Core.EwEModel
                taxonSearch.North = model.North
                taxonSearch.South = model.South
                taxonSearch.East = model.East
                taxonSearch.West = model.West
            Else
                taxonSearch.North = cCore.NULL_VALUE
                taxonSearch.South = cCore.NULL_VALUE
                taxonSearch.East = cCore.NULL_VALUE
                taxonSearch.West = cCore.NULL_VALUE
            End If

            ' Start searching
            Me.SelectedDataProducer.StartSearch(taxonSearch, 100)
        Catch ex As Exception

        End Try

        Me.UpdateControls()

    End Sub

    Private Sub UpdateRecord(ByVal term As ITaxonSearchData)

        If (Me.SelectedDataProducer Is Nothing) Then Return

        Try
            ' Has a search key for this specific producer?
            If (Not String.IsNullOrEmpty(term.SourceKey)) And _
               (String.Compare(term.Source, Me.SelectedDataProducer.Name, True) = 0) Then
                ' #Yes: Start searching (expected to return only one result)
                Me.SelectedDataProducer.StartSearch(Me.m_gridGroups.GetSearchTerm(term), 100)
            End If
        Catch ex As Exception
            ' Woops
        End Try

    End Sub

    Private Function OnIsTaxonUsed(ti As ITaxonSearchData) As Boolean
        Debug.Assert(ti IsNot Nothing)
        Return Me.m_gridGroups.IsTaxonUsed(ti)
    End Function

#End Region ' Internals

#Region " Public bits "


#End Region ' Public bits

End Class
