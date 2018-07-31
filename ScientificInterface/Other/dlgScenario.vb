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

Imports EwECore
Imports ScientificInterface.Ecopath.Input
Imports ScientificInterface.Ecopath.Output
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Utilities
Imports EwEUtils.Core

#End Region ' Imports

Namespace Wizard

    ''' <summary>
    ''' Dialog to load, save or create an EwE scenario.
    ''' </summary>
    ''' <remarks>
    ''' <para>This dialog can be opened in four modes:</para>
    ''' <list type="bullet">
    ''' <item>
    ''' <term>
    ''' <see cref="dlgScenario.eDialogModeType.LoadScenario">LoadScenario</see>
    ''' </term>
    ''' <description>
    ''' Opens the dialog for loading an existing scenario.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <term>
    ''' <see cref="dlgScenario.eDialogModeType.CreateScenario">CreateScenario</see>
    ''' </term>
    ''' <description>
    ''' Opens the dialog for creating a new scenario.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <term>
    ''' <see cref="dlgScenario.eDialogModeType.SaveScenario">SaveScenario</see>
    ''' </term>
    ''' <description>
    ''' Opens the dialog for saving the current loaded scenario.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <term>
    ''' <see cref="dlgScenario.eDialogModeType.DeleteScenario">DeleteScenario</see>
    ''' </term>
    ''' <description>
    ''' Opens the dialog for deleting an existing scenario.
    ''' </description>
    ''' </item>
    ''' </list>
    ''' </remarks>
    Public Class dlgScenario
        Implements IUIElement

        ''' <summary>
        ''' <para>Enumerated type defining dialog interaction modes.</para>
        ''' </summary>
        Public Enum eDialogModeType
            ''' <summary>Use the dialog to support creating a new EwE scenario.</summary>
            CreateScenario
            ''' <summary>Use the dialog to support saving an EwE scenario.</summary>
            SaveScenario
            ''' <summary>Use the dialog to support loading an existing EwE scenario.</summary>
            LoadScenario
            ''' <summary>Use the dialog to deleting an EwE scenario.</summary>
            DeleteScenario
        End Enum

        Private Enum eColumnTypes
            Name
            Loaded
            LastSaved
        End Enum

#Region " Helper classes "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper class for sorting <see cref="cEwEScenario">scenario</see>
        ''' list view items for specific <see cref="eColumnTypes">column type</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Class ColumnSorter
            Implements IComparer

            ''' <summary><see cref="eColumnTypes">column type</see> to sort by.</summary>
            Private m_column As eColumnTypes

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set <see cref="eColumnTypes">column type</see> to sort by
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Column() As eColumnTypes
                Get
                    Return Me.m_column
                End Get
                Set(ByVal value As eColumnTypes)
                    Me.m_column = value
                End Set
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Perform actual sort
            ''' </summary>
            ''' <param name="x"></param>
            ''' <param name="y"></param>
            ''' <returns></returns>
            ''' -------------------------------------------------------------------
            Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer _
                    Implements IComparer.Compare

                Dim lvi1 As ListViewItem = DirectCast(x, ListViewItem)
                Dim s1 As cEwEScenario = DirectCast(lvi1.Tag, cEwEScenario)
                Dim lvi2 As ListViewItem = DirectCast(y, ListViewItem)
                Dim s2 As cEwEScenario = DirectCast(lvi2.Tag, cEwEScenario)

                ' Assume both not null
                Select Case Me.m_column
                    Case eColumnTypes.Name
                        ' Sort names ascending
                        Return String.Compare(s1.Name, s2.Name, True)
                    Case eColumnTypes.Loaded
                        ' Sort loaded scenario first
                        If s1.IsLoaded Then Return 1
                        If s2.IsLoaded Then Return -1
                        Return 0
                    Case eColumnTypes.LastSaved
                        ' Sort save dates in descending order
                        If s1.LastSaved > s2.LastSaved Then Return -1
                        If s1.LastSaved = s2.LastSaved Then Return 0
                        Return 1

                End Select
                Return 0

            End Function

        End Class

#End Region ' Helper classes

#Region " Private vars "

        ''' <summary>UI context to connect to.</summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>Dialog operation mode.</summary>
        Private m_mode As eDialogModeType = eDialogModeType.CreateScenario
        ''' <summary>Scenario that is selected in the dialog.</summary>
        Private m_scenario As cEwEScenario = Nothing
        ''' <summary>Scenario that the dialog was invoked for, if any.</summary>
        Private m_scenarioSource As cEwEScenario = Nothing
        ''' <summary>List of available scenarios.</summary>
        Private m_lScenarios As List(Of cEwEScenario)

#End Region ' Private vars

#Region " Constructor "

        ''' <summary>
        ''' Constructor, initializes a new instance of this dialog.
        ''' </summary>
        ''' <param name="uic">The UI context to connect to.</param>
        ''' <param name="mode"><see cref="eDialogModeType">Dialog interaction mode</see>.</param>
        ''' <param name="scenario">EwE scenario to save, if any.</param>
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal mode As eDialogModeType, _
                       Optional ByVal scenario As cEwEScenario = Nothing)

            Me.InitializeComponent()

            Me.UIContext = uic
            Me.m_mode = mode
            Me.m_scenario = scenario
            Me.m_scenarioSource = scenario
            Me.m_lScenarios = Me.GetAvailableScenarios()
            Me.UpdateScenarioListControls()

            ' Init create dialog
            Me.tbNameCreate.Text = Me.GetNewScenarioName()
            Me.tbDescriptionCreate.Text = cStringUtils.Localize(My.Resources.GENERIC_DEFAULT_DESCRIPTION, Date.Now().ToShortDateString(), Date.Now().ToShortTimeString())
            Me.tbAuthorCreate.Text = Me.UIContext.Core.EwEModel.Author
            Me.tbContactCreate.Text = Me.UIContext.Core.EwEModel.Contact

            Me.Icon = Me.GetIcon()
            Me.TopMost = True

        End Sub

#End Region ' Constructor

#Region " Interface implementation "

        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Private Set(ByVal value As cUIContext)
                Me.m_uic = value
            End Set
        End Property

#End Region ' Interface implementation

#Region " Overridables "

        Protected Overridable Function GetIcon() As Icon
            Return Me.Icon
        End Function

        Protected Overridable Function GetNewScenarioName() As String
            Return ""
        End Function

        Protected Overridable Function GetDialogCaption(ByVal mode As eDialogModeType, ByVal strEwEModelName As String) As String
            Return ""
        End Function

        Protected Overridable Function DeleteScenario(ByVal scenario As cEwEScenario) As Boolean
            Return False
        End Function

        Protected Overridable Function GetAvailableScenarios() As List(Of cEwEScenario)
            Return Nothing
        End Function

#End Region ' Overridables

#Region " Internal implementation "

        Private Sub SwitchMode(ByVal mode As eDialogModeType)

            For iPage As Integer = 0 To Me.tabctrlModes.TabCount - 1
                Dim tp As TabPage = Me.tabctrlModes.TabPages(iPage)
                If (CInt(tp.Tag) = mode) Then
                    Me.tabctrlModes.SelectedTab = tp
                End If
            Next

            Me.m_mode = mode
            Me.InitControls()

        End Sub

        Private Sub InitControls()

            Me.Text = Me.GetDialogCaption(Me.m_mode, Me.UIContext.Core.EwEModel.Name)

            Me.tsmCreate.Visible = (Me.m_mode = eDialogModeType.CreateScenario)
            Me.tsmLoad.Visible = (Me.m_mode = eDialogModeType.LoadScenario)
            Me.tsmDelete.Visible = (Me.m_mode = eDialogModeType.DeleteScenario)
            Me.tsmSave.Visible = (Me.m_mode = eDialogModeType.SaveScenario)
            Me.tsmRename.Visible = (Me.m_mode = eDialogModeType.CreateScenario Or Me.m_mode = eDialogModeType.DeleteScenario)

            ' JS 07may08: 'Save As' must use source scenario details
            If Me.m_scenarioSource IsNot Nothing Then
                Me.tbNameSaveAs.Text = Me.m_scenarioSource.Name
                Me.tbDescriptionSaveAs.Text = Me.m_scenarioSource.Description
                Me.tbAuthorSaveAs.Text = Me.m_scenarioSource.Author
                Me.tbContactSaveAs.Text = Me.m_scenarioSource.Contact
            End If

            Me.UpdateControls()

        End Sub

        Private Sub UpdateControls()

            Select Case Me.m_mode

                Case eDialogModeType.CreateScenario
                    Me.btnCreate.Enabled = Me.CanCreateScenario()
                    Me.tsmCreate.Enabled = Me.CanCreateScenario()
                    Me.tsmRename.Enabled = Me.CanRenameScenario()
                    Me.AcceptButton = Me.btnCreate
                    Me.CancelButton = Me.btnCancelCreate

                    ' Do not sync any of the fields

                Case eDialogModeType.LoadScenario
                    Me.btnLoad.Enabled = Me.CanLoadScenario()
                    Me.tsmLoad.Enabled = Me.CanLoadScenario()
                    Me.tsmRename.Enabled = False
                    Me.AcceptButton = Me.btnLoad
                    Me.CancelButton = Me.btnCancelLoad

                    ' Sync all with selection
                    If Me.Scenario IsNot Nothing Then
                        Me.tbDescriptionLoad.Text = Scenario.Description
                        Me.tbAuthorLoad.Text = Scenario.Author
                        Me.tbContactLoad.Text = Scenario.Contact
                    End If

                Case eDialogModeType.SaveScenario
                    Me.btnSave.Enabled = Me.CanSaveScenario()
                    Me.tsmSave.Enabled = Me.CanSaveScenario()
                    Me.tsmRename.Enabled = Me.CanRenameScenario()
                    Me.AcceptButton = Me.btnSave
                    Me.CancelButton = Me.btnCancelSave

                    ' Sync name with selection
                    If Me.Scenario IsNot Nothing Then
                        Me.tbNameSaveAs.Text = Scenario.Name
                    End If

                Case eDialogModeType.DeleteScenario
                    Me.btnDelete.Enabled = Me.CanDeleteScenario()
                    Me.tsmDelete.Enabled = Me.CanDeleteScenario()
                    Me.tsmRename.Enabled = False
                    Me.AcceptButton = Me.btnDelete
                    Me.CancelButton = Me.btnCancelDelete

                    ' Sync all with selection
                    If Me.Scenario IsNot Nothing Then
                        Me.tbDescriptionDelete.Text = Scenario.Description
                        Me.tbAuthorDelete.Text = Scenario.Author
                        Me.tbContactDelete.Text = Scenario.Contact
                    End If

            End Select

        End Sub

        Private Function GetScenarioListViewItem(ByVal scenario As cEwEScenario) As ListViewItem

            Dim lvi As ListViewItem = Nothing
            Dim astrColumns([Enum].GetValues(GetType(eColumnTypes)).Length - 1) As String

            ' Pop columns
            ' - name
            astrColumns(eColumnTypes.Name) = scenario.Name
            ' - Loaded
            If scenario.IsLoaded Then
                astrColumns(eColumnTypes.Loaded) = SharedResources.GENERIC_VALUE_YES
            Else
                astrColumns(eColumnTypes.Loaded) = ""
            End If
            ' - last saved date
            If (scenario.LastSaved > 0) Then
                astrColumns(eColumnTypes.LastSaved) = cStringUtils.Localize("{0:g}", cDateUtils.JulianToDate(scenario.LastSaved))
            Else
                astrColumns(eColumnTypes.LastSaved) = ""
            End If

            ' Prep item
            lvi = New ListViewItem(astrColumns)
            lvi.Tag = scenario

            Return lvi

        End Function

        ''' <summary>
        ''' Repopulate the scenario list boxes, preserving the selection if possible.
        ''' </summary>
        Private Sub UpdateScenarioListControls()

            ' Clear the list first
            Me.lvCreate.Items.Clear()
            Me.lvLoad.Items.Clear()
            Me.lvDelete.Items.Clear()
            Me.lvSaveAs.Items.Clear()

            ' Add the list of scenarios
            For i As Integer = 0 To Me.m_lScenarios.Count - 1

                Me.lvCreate.Items.Add(Me.GetScenarioListViewItem(m_lScenarios(i)))
                Me.lvLoad.Items.Add(Me.GetScenarioListViewItem(m_lScenarios(i)))
                Me.lvDelete.Items.Add(Me.GetScenarioListViewItem(m_lScenarios(i)))
                Me.lvSaveAs.Items.Add(Me.GetScenarioListViewItem(m_lScenarios(i)))
            Next

            ' Set the selected index
            If Me.m_lScenarios.Count > 0 Then
                Me.lvCreate.TopItem.Selected = True
                Me.lvLoad.TopItem.Selected = True
                Me.lvDelete.TopItem.Selected = True
                Me.lvSaveAs.TopItem.Selected = True
            End If

            ' Update selection
            Me.Scenario = Me.m_scenario

        End Sub

        Private Function CanCreateScenario() As Boolean
            Dim bHasUniqueName As Boolean = Me.IsUniqueScenarioName(Me.tbNameCreate.Text)
            Dim bIsCorrectMode As Boolean = (Me.m_mode = eDialogModeType.CreateScenario)
            Return bHasUniqueName And bIsCorrectMode
        End Function

        Private Function CanLoadScenario() As Boolean
            Dim bHasSelection As Boolean = (Me.Scenario IsNot Nothing)
            Dim bIsCorrectMode As Boolean = (Me.m_mode = eDialogModeType.LoadScenario)
            Return bHasSelection And bIsCorrectMode
        End Function

        Private Function CanSaveScenario() As Boolean
            Dim bHasName As Boolean = Not (String.IsNullOrEmpty(Me.tbNameSaveAs.Text))
            Dim bIsCorrectMode As Boolean = (Me.m_mode = eDialogModeType.SaveScenario)
            Return bHasName And bIsCorrectMode
        End Function

        Private Function CanDeleteScenario() As Boolean
            Dim bHasSelection As Boolean = (Me.Scenario IsNot Nothing)
            Dim bIsCorrectMode As Boolean = (Me.m_mode = eDialogModeType.DeleteScenario)
            Dim bIsLoaded As Boolean = False
            If (Me.Scenario IsNot Nothing) Then bIsLoaded = Me.Scenario.IsLoaded
            Return bHasSelection And bIsCorrectMode And Not bIsLoaded
        End Function

        Private Function CanRenameScenario() As Boolean
            Dim bHasSelection As Boolean = (Me.lvCreate.SelectedIndices.Count = 1)
            Return bHasSelection
        End Function

        Private Function FindScenarioByName(ByVal strScenarioName As String) As cEwEScenario
            For iScenario As Integer = 0 To Me.m_lScenarios.Count - 1
                If (String.Compare(Me.m_lScenarios(iScenario).Name, strScenarioName, True) = 0) Then
                    Return Me.m_lScenarios(iScenario)
                End If
            Next
            Return Nothing
        End Function

        Private Property SelectedScenario(ByVal lv As ListView) As cEwEScenario
            Get
                If (lv.SelectedItems.Count <> 1) Then Return Nothing
                Return DirectCast(lv.SelectedItems(0).Tag, cEwEScenario)
            End Get
            Set(ByVal value As cEwEScenario)
                For Each item As ListViewItem In lv.Items
                    item.Selected = ReferenceEquals(item.Tag, value)
                Next
            End Set
        End Property

        Private Function IsUniqueScenarioName(ByVal strName As String) As Boolean
            Dim bYepItIs As Boolean = Not String.IsNullOrEmpty(strName)
            For Each sc As cEwEScenario In Me.m_lScenarios
                If String.Compare(strName, sc.Name, True) = 0 Then
                    bYepItIs = False
                End If
            Next
            Return bYepItIs
        End Function

#End Region ' Implementation

#Region " Event handlers "

        ''' <summary>
        ''' Event handler when dialog is being loaded.
        ''' </summary>
        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            ' Set up possible modes correctly
            With Me.tabctrlModes
                .TabPages(0).Tag = eDialogModeType.CreateScenario
                .TabPages(1).Tag = eDialogModeType.LoadScenario
                .TabPages(2).Tag = eDialogModeType.DeleteScenario
                .TabPages(3).Tag = eDialogModeType.SaveScenario
            End With

            Select Case Mode

                Case eDialogModeType.CreateScenario
                    ' Cannot save as
                    Me.tabctrlModes.TabPages.RemoveAt(3)
                    Me.tabctrlModes.SelectedIndex = 0

                Case eDialogModeType.LoadScenario
                    ' Cannot save as
                    Me.tabctrlModes.TabPages.RemoveAt(3)
                    Me.tabctrlModes.SelectedIndex = 1

                Case eDialogModeType.SaveScenario
                    ' Cannot create, cannot load, cannot delete
                    Me.tabctrlModes.TabPages.RemoveAt(0)
                    Me.tabctrlModes.TabPages.RemoveAt(0)
                    Me.tabctrlModes.TabPages.RemoveAt(0)
                    Me.tabctrlModes.SelectedIndex = 0

                Case eDialogModeType.DeleteScenario
                    ' Cannot save as
                    Me.tabctrlModes.TabPages.RemoveAt(3)
                    Me.tabctrlModes.SelectedIndex = 2

            End Select

            Me.lvCreate.ListViewItemSorter = New ColumnSorter()
            Me.lvLoad.ListViewItemSorter = New ColumnSorter()
            Me.lvDelete.ListViewItemSorter = New ColumnSorter()
            Me.lvSaveAs.ListViewItemSorter = New ColumnSorter()

            ' Get scenarios
            Me.UpdateScenarioListControls()

            ' In load mode and nothing to load?
            If ((Me.m_mode = eDialogModeType.LoadScenario) And (Me.m_lScenarios.Count = 0)) Then
                ' #Yes: switch to create mode
                Me.m_mode = eDialogModeType.CreateScenario
            End If

            Me.SwitchMode(Me.m_mode)

        End Sub

        Private Sub tabctrlModes_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
                Handles tabctrlModes.SelectedIndexChanged

            ' Sanity check
            Debug.Assert(ReferenceEquals(sender, Me.tabctrlModes))
            Me.Mode = DirectCast(Me.tabctrlModes.TabPages(Me.tabctrlModes.SelectedIndex).Tag, eDialogModeType)

        End Sub

        Private Sub OnCreateScenario(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles btnCreate.Click, lvCreate.DoubleClick

            ' Sanity check
            Debug.Assert(Me.m_mode = eDialogModeType.CreateScenario)
            ' Validation
            If Not Me.CanCreateScenario() Then Return
            ' Acutal create does not happen here. This dialog is just the messenger
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnLoadScenario(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles btnLoad.Click, lvLoad.DoubleClick, tsmLoad.Click

            ' Sanity check
            Debug.Assert(Me.m_mode = eDialogModeType.LoadScenario)
            ' Validation
            If Not Me.CanLoadScenario() Then Return
            ' Acutal load does not happen here. This dialog is just the messenger
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnSaveScenarioAs(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles btnSave.Click, lvSaveAs.DoubleClick, tsmSave.Click

            ' Sanity check
            Debug.Assert(Me.m_mode = eDialogModeType.SaveScenario)
            ' Validation
            If Not Me.CanSaveScenario() Then Return
            ' Acutal save does not happen here. This dialog is just the messenger
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        ''' <summary>
        ''' Event handler to delete a EwE scenario.
        ''' </summary>
        Private Sub OnDeleteScenario(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles btnDelete.Click, lvDelete.DoubleClick, tsmDelete.Click

            If Not Me.CanDeleteScenario() Then Return

            Dim scenario As cEwEScenario = Me.Scenario

            ' Sanity check
            If scenario Is Nothing Then Return

            ' Ask for confirmation
            Dim strMessage As String = cStringUtils.Localize(My.Resources.SCENARIO_CONFIRMDELETE_PROMPT, scenario.Name)
            Dim fmsg As New cFeedbackMessage(strMessage, eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
            Me.UIContext.Core.Messages.SendMessage(fmsg)

            If (fmsg.Reply <> eMessageReply.YES) Then Return

            ' Remove successful?
            If Me.DeleteScenario(scenario) Then
                Me.m_scenario = Nothing
                Me.m_lScenarios = Me.GetAvailableScenarios()
                Me.UpdateScenarioListControls()
            End If

        End Sub

        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles btnCancelCreate.Click, btnCancelLoad.Click, btnCancelSave.Click, btnCancelDelete.Click

            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()

        End Sub

        ''' <summary>
        ''' Event handler...
        ''' </summary>
        Private Sub OnScenarioCreateNameChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles tbNameCreate.TextChanged

            Me.Scenario = Me.FindScenarioByName(tbNameCreate.Text)

        End Sub

        ''' <summary>
        ''' Event handler...
        ''' </summary>
        Private Sub OnScenarioSaveAsNameChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles tbNameSaveAs.TextChanged

            Me.Scenario = Me.FindScenarioByName(tbNameSaveAs.Text)

        End Sub

        Private Sub OnRenameScenario(ByVal sender As System.Object, ByVal e As System.EventArgs) _
                Handles tsmRename.Click

            Dim lv As ListView = Nothing
            Dim lvi As ListViewItem = Nothing

            Select Case Me.m_mode
                Case eDialogModeType.CreateScenario
                    lv = Me.lvCreate
                Case eDialogModeType.SaveScenario
                    lv = Me.lvSaveAs
            End Select

            If (lv IsNot Nothing) Then
                If (lv.SelectedItems.Count = 1) Then
                    lvi = lv.SelectedItems(0)
                    lvi.BeginEdit()
                End If
            End If

        End Sub

        Private Sub OnLVBeforeLabelEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.LabelEditEventArgs) _
                Handles lvCreate.BeforeLabelEdit, lvSaveAs.BeforeLabelEdit

            Dim lv As ListView = DirectCast(sender, ListView)
            Dim lvi As ListViewItem = lv.Items(e.Item)
            Dim scenario As cEwEScenario = DirectCast(lvi.Tag, cEwEScenario)

            e.CancelEdit = (scenario Is Nothing)
        End Sub

        Private Sub OnLVAfterLabelEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.LabelEditEventArgs) _
                Handles lvCreate.AfterLabelEdit, lvSaveAs.AfterLabelEdit

            ' Reject empty names
            If String.IsNullOrEmpty(e.Label) Then
                e.CancelEdit = True
                Return
            End If

            Dim lv As ListView = DirectCast(sender, ListView)
            Dim lvi As ListViewItem = lv.Items(e.Item)
            Dim scenario As cEwEScenario = DirectCast(lvi.Tag, cEwEScenario)

            If (scenario IsNot Nothing) Then
                ' Apply new scenario name
                scenario.Name = e.Label
                Me.UpdateScenarioListControls()
            End If

        End Sub

        Private Sub OnLVColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) _
                Handles lvCreate.ColumnClick, lvDelete.ColumnClick, lvLoad.ColumnClick, lvSaveAs.ColumnClick

            Dim lv As ListView = Nothing
            Dim comparer As ColumnSorter = Nothing

            lv = DirectCast(sender, ListView)
            If (TypeOf lv.ListViewItemSorter Is ColumnSorter) Then
                comparer = DirectCast(lv.ListViewItemSorter, ColumnSorter)
                comparer.Column = DirectCast(e.Column, eColumnTypes)
            End If

            lv.Sort()

        End Sub

        Private Sub OnLVSelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
                Handles lvCreate.SelectedIndexChanged, lvDelete.SelectedIndexChanged, lvLoad.SelectedIndexChanged, lvSaveAs.SelectedIndexChanged

            Dim lv As ListView = DirectCast(sender, ListView)
            Dim lvi As ListViewItem = Nothing

            If (lv.SelectedItems.Count <> 1) Then
                Me.Scenario = Nothing
            Else
                lvi = lv.SelectedItems(0)
                Me.Scenario = DirectCast(lvi.Tag, cEwEScenario)
            End If

        End Sub

#End Region ' Event handlers

#Region " Properties "

        Private m_bInUpdate As Boolean = False

        Public Overridable Property Scenario() As cEwEScenario
            Get
                Return Me.m_scenario
            End Get
            Set(ByVal scenario As cEwEScenario)
                If Me.m_bInUpdate Then Return

                ' Lock down
                Me.m_bInUpdate = True

                Me.SelectedScenario(Me.lvLoad) = scenario
                Me.SelectedScenario(Me.lvDelete) = scenario
                Me.SelectedScenario(Me.lvSaveAs) = scenario

                Me.m_scenario = scenario

                Me.UpdateControls()

                Me.m_bInUpdate = False

            End Set
        End Property

        Public ReadOnly Property ScenarioName() As String
            Get
                Select Case Me.m_mode
                    Case eDialogModeType.CreateScenario
                        Return Me.tbNameCreate.Text
                    Case eDialogModeType.SaveScenario
                        Return Me.tbNameSaveAs.Text
                End Select
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property ScenarioDescription() As String
            Get
                Select Case Me.m_mode
                    Case eDialogModeType.CreateScenario
                        Return Me.tbDescriptionCreate.Text
                    Case eDialogModeType.SaveScenario
                        Return Me.tbDescriptionSaveAs.Text
                End Select
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property ScenarioAuthor() As String
            Get
                Select Case Me.m_mode
                    Case eDialogModeType.CreateScenario
                        Return Me.tbAuthorCreate.Text
                    Case eDialogModeType.SaveScenario
                        Return Me.tbAuthorSaveAs.Text
                End Select
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property ScenarioContact() As String
            Get
                Select Case Me.m_mode
                    Case eDialogModeType.CreateScenario
                        Return Me.tbContactCreate.Text
                    Case eDialogModeType.SaveScenario
                        Return Me.tbContactSaveAs.Text
                End Select
                Return Nothing
            End Get
        End Property

        Public Property Mode() As eDialogModeType
            Get
                Return Me.m_mode
            End Get
            Set(ByVal value As eDialogModeType)
                Me.SwitchMode(value)
            End Set
        End Property

#End Region ' Properties

    End Class

End Namespace