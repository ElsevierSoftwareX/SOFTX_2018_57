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
Imports System.IO
Imports System.Text
Imports EwECore
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Utility class; adds a toolstrip-contained text box to a form from which 
    ''' all currently selected EwE variables can be modified. Conditions apply.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class cQuickEditHandler

#Region " Private variables "

        ''' <summary>The grid whose selection is monitored.</summary>
        Private m_grid As EwEGrid = Nothing
        ''' <summary>UI context to use.</summary>
        Private m_uic As cUIContext = Nothing

        ''' <summary>The toolstrip that is managed by this handler.</summary>
        Private m_ts As ToolStrip = Nothing
        ''' <summary>The value control that is managed by this handler.</summary>
        Private m_ctrlValue As ToolStripItem = Nothing
        ''' <summary>The edit box label that is managed by this handler.</summary>
        Private m_lblSet As ToolStripLabel = Nothing
        ''' <summary>Set button that is managed by this handler.</summary>
        Private m_btnSet As ToolStripButton = Nothing
        ''' <summary>Flag stating whether handler is attached.</summary>
        Private m_bAttached As Boolean = False
        ''' <summary>Flag stating whether import/export controls can be shown.</summary>
        Private m_bShowImportExport As Boolean = True
        ''' <summary>Flag stating whether the grid is used for showing outputs only.</summary>
        Private m_bIsOutputGrid As Boolean = True

        ''' <summary>Original value.</summary>
        Private m_strValueOrg As String = Nothing

        ' Import Export
        Private m_sep As ToolStripSeparator = Nothing
        Private m_btnImport As ToolStripButton = Nothing
        Private m_btnExport As ToolStripButton = Nothing

#End Region ' Private variables

#Region " Public interfaces "

        Private m_iValuePos As Integer = -1

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' <para>Connect the QuickEditHandler to a <see cref="EwEGrid">EwE grid</see>.
        ''' Call <see cref="Detach">Detach</see> to disconnect a Quick Edit handler
        ''' from a form it was previously attached to.</para>
        ''' <para>A toolstrip is created if not available, and Quick Edit toolstrip 
        ''' items will be added to the toolstrip.</para>
        ''' </summary>
        ''' <param name="ts">The ToolStrip to connect to, if any.</param>
        ''' -------------------------------------------------------------------
        Public Sub Attach(ByVal grid As EwEGrid, _
                          ByVal uic As cUIContext, _
                          ByVal ts As ToolStrip, _
                          Optional ByVal bIsOutputGrid As Boolean = True)

            ' Sanity checks
            Debug.Assert(grid IsNot Nothing)
            Debug.Assert(uic IsNot Nothing)
            Debug.Assert(ts IsNot Nothing)

            If Me.m_bAttached Then Me.Detach()

            ' Store ref to grid
            Me.m_grid = grid
            ' Store ref to UIC
            Me.m_uic = uic
            Me.m_ts = ts

            AddHandler Me.m_grid.OnSelectionChanged, AddressOf OnGridSelectionChanged

            ' Hide grip
            Me.m_ts.GripStyle = ToolStripGripStyle.Hidden

            Me.m_sep = New ToolStripSeparator()

            ' Create quick edit label
            Me.m_lblSet = New ToolStripLabel(cStyleGuide.ToControlLabel(My.Resources.LABEL_SET))

            ' Create quick edit control
            Me.SetEditControl(eControlType.TextBox)

            ' Create quick edit set button
            Me.m_btnSet = New ToolStripButton(My.Resources.GENERIC_LABEL_APPLY)
            Me.m_btnSet.ToolTipText = My.Resources.TOOLTIP_GRID_APPLYVALUE
            AddHandler Me.m_btnSet.Click, AddressOf OnBtnSetClick

            ' Create import button (input grids only)
            If Not Me.m_bIsOutputGrid Then
                Me.m_btnImport = New ToolStripButton(My.Resources.ImportXMLHS)
                Me.m_btnImport.ToolTipText = My.Resources.TOOLTIP_LOADFROMCSV
                AddHandler Me.m_btnImport.Click, AddressOf OnImportGrid
            End If

            ' Create export button
            Me.m_btnExport = New ToolStripButton(My.Resources.ExportHS)
            Me.m_btnExport.ToolTipText = My.Resources.TOOLTIP_SAVETOCSV
            AddHandler Me.m_btnExport.Click, AddressOf OnExportGrid

            ' Add items to the toolstrip
            If (cSystemUtils.IsRightToLeft) Then
                If (Me.m_btnImport IsNot Nothing) Then
                    Me.m_btnImport.Alignment = ToolStripItemAlignment.Left
                    Me.m_ts.Items.Add(Me.m_btnImport)
                End If
                Me.m_btnExport.Alignment = ToolStripItemAlignment.Left
                Me.m_sep.Alignment = ToolStripItemAlignment.Left
                Me.m_lblSet.Alignment = ToolStripItemAlignment.Left
                Me.m_ctrlValue.Alignment = ToolStripItemAlignment.Left
                Me.m_btnSet.Alignment = ToolStripItemAlignment.Left
                Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_btnExport, Me.m_sep, Me.m_lblSet, Me.m_ctrlValue, Me.m_btnSet})
            Else
                If (Me.m_btnImport IsNot Nothing) Then
                    Me.m_btnImport.Alignment = ToolStripItemAlignment.Right
                    Me.m_ts.Items.Add(Me.m_btnImport)
                End If
                Me.m_btnExport.Alignment = ToolStripItemAlignment.Right
                Me.m_sep.Alignment = ToolStripItemAlignment.Right
                Me.m_lblSet.Alignment = ToolStripItemAlignment.Right
                Me.m_ctrlValue.Alignment = ToolStripItemAlignment.Right
                Me.m_btnSet.Alignment = ToolStripItemAlignment.Right
                Me.m_ts.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_btnExport, Me.m_sep, Me.m_btnSet, Me.m_ctrlValue, Me.m_lblSet})
            End If

            Me.m_iValuePos = Me.m_ts.Items.IndexOf(Me.m_ctrlValue)

            ' Set attached flag
            Me.m_bAttached = True

            ' Set initial state
            Me.IsOutputGrid = bIsOutputGrid
            Me.UpdateControls()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' <para>Detach the Quick Edit handler from its current form that is was
        ''' previously connected to with the <see cref="Attach">Attach</see> method.</para>
        ''' <para>This will also clean up any toolstrips and toolstrip items
        ''' that an instance created when it was attached to a form.</para>
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Detach()

            If Not m_bAttached Then Return

            Me.m_ts.Items.Remove(Me.m_ctrlValue)
            Me.m_ts.Items.Remove(Me.m_lblSet)

            If Me.m_btnImport IsNot Nothing Then
                RemoveHandler Me.m_btnImport.Click, AddressOf OnImportGrid
                Me.m_btnImport.Dispose()
                Me.m_btnImport = Nothing
            End If

            RemoveHandler Me.m_btnExport.Click, AddressOf OnExportGrid
            Me.m_btnExport.Dispose()
            Me.m_btnExport = Nothing

            Me.SetEditControl(eControlType.NotSet)

            RemoveHandler Me.m_btnSet.Click, AddressOf OnBtnSetClick
            Me.m_btnSet.Dispose()
            Me.m_btnSet = Nothing

            RemoveHandler Me.m_grid.OnSelectionChanged, AddressOf OnGridSelectionChanged
            Me.m_grid = Nothing

            Me.m_ts = Nothing

            Me.m_bAttached = False

        End Sub

        Public Property IsOutputGrid As Boolean
            Get
                Return Me.m_bIsOutputGrid
            End Get
            Set(value As Boolean)
                If (Me.m_bIsOutputGrid <> value) Then
                    Me.m_bIsOutputGrid = value
                    Me.UpdateControls()
                End If
            End Set
        End Property

        Public Property ShowImportExport As Boolean
            Get
                Return Me.m_bShowImportExport
            End Get
            Set(value As Boolean)
                If (Me.m_bShowImportExport <> value) Then
                    Me.m_bShowImportExport = value
                    Me.UpdateControls()
                End If
            End Set
        End Property

#End Region ' Public interfaces

#Region " Control events "

#Region " Text box "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; responds to an [ENTER] key press to apply entered text
        ''' to the grid selection.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnSetBoxKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
            ' Is [ENTER]?
            If e.KeyCode = Keys.Enter Then Me.ApplyValueToSelection(Me.m_ctrlValue.Text)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; called when the caret enters the 'Set' text box.
        ''' Overriden to remember the value of the text box, for testing whether
        ''' a change has occurred that needs applying later.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnSetBoxEnter(ByVal sender As Object, ByVal e As EventArgs)
            Me.m_strValueOrg = Me.m_ctrlValue.Text
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; called when the caret leaves the 'Set' text box.
        ''' Applies the content of the box to underlying fields when the
        ''' entered value has been modified.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnSetBoxLeave(ByVal sender As Object, ByVal e As EventArgs)
            If String.Compare(Me.m_strValueOrg, Me.m_ctrlValue.Text) <> 0 Then
                Me.ApplyValueToSelection(Me.m_ctrlValue.Text)
                Me.m_strValueOrg = Me.m_ctrlValue.Text
            End If
        End Sub

#End Region ' Text box

#Region " Combo box "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; called when the user changes the value of the combo box.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnSelectedValueChanged(ByVal sender As Object, ByVal e As EventArgs)
            Me.ApplyValueToSelection(DirectCast(Me.m_ctrlValue, ToolStripComboBox).SelectedItem)
        End Sub

#End Region ' Combo box

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; responds to Set button press to apply entered text
        ''' to the grid selection.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnBtnSetClick(ByVal sender As Object, ByVal e As EventArgs)
            Me.ApplyValueToSelection(Me.m_ctrlValue.Text)
            Me.m_strValueOrg = Me.m_ctrlValue.Text
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; responds to Import button press to import grid content
        ''' from a CSV file.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnImportGrid(ByVal sender As Object, ByVal e As EventArgs)
            Me.ImportGridFromCSV()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; responds to Export button press to export grid content
        ''' to a CSV file.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnExportGrid(ByVal sender As Object, ByVal e As EventArgs)
            Me.ExportGridToCSV()
        End Sub

#End Region ' Control events

#Region " Grid events "

        Private Sub OnGridSelectionChanged()
            Me.UpdateControls()
        End Sub

#End Region ' Command events

#Region " Internal implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update the state of the edit box based on the content of a grid selection.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateControls()

            ' ToDo: remove flashing of editor control

            If (Not Me.m_bAttached) Then Return

            Dim sel As SourceGrid2.Selection = Me.m_grid.Selection
            Dim bIsInputGrid As Boolean = Not Me.m_bIsOutputGrid
            Dim bHasEditableCells As Boolean = False
            ' Flag stating that the selection contains a mix of variable names
            Dim bIsMixedSelection As Boolean = False
            ' The last found variable name
            Dim vn As eVarNameFlags = eVarNameFlags.NotSet

            ' Flag stating that the selection contains different values
            Dim bIsMixedValue As Boolean = False
            Dim bIsStandardValuesExclusive As Boolean = False

            Dim objValue As Object = Nothing
            Dim items As ICollection = Nothing

            ' Iterate through cells
            ' JS 18May18: Greatly optimized logic to iterate over cells in the selection
            Dim r As SourceGrid2.Range = sel.GetRange
            ' Math.max construction prevens from looping into empty regions
            For row As Integer = Math.Max(0, r.Start.Row) To r.End.Row
                For col As Integer = Math.Max(0, r.Start.Column) To r.End.Column
                    If r.Contains(New SourceGrid2.Position(row, col)) Then
                        Dim cell As SourceGrid2.Cells.ICell = CType(Me.m_grid.GetCell(row, col), SourceGrid2.Cells.ICell)
                        If (cell IsNot Nothing) Then
                            If (cell.DataModel IsNot Nothing) Then
                                If cell.DataModel.StandardValuesExclusive Then
                                    items = cell.DataModel.StandardValues
                                    bIsStandardValuesExclusive = True
                                End If

                                ' Is this cell editable?
                                If cell.DataModel.EnableEdit Then
                                    ' #Yes: explore the variable this cell represents by checking an attached property
                                    ' Is a property cell?
                                    If TypeOf cell Is PropertyCell Then
                                        ' #Yes: get the property
                                        Dim p As cProperty = DirectCast(cell, PropertyCell).GetProperty()
                                        ' Does this property refer to a variable other than found earlier?
                                        If ((vn <> eVarNameFlags.NotSet) And (p.VarName <> vn)) Then
                                            ' #Yes: this is a mixed selection.
                                            bIsMixedSelection = True
                                        End If

                                        ' Does this property hold a value other than found earlier?
                                        If (objValue IsNot Nothing) Then
                                            If (Not objValue.Equals(p.GetValue())) Then
                                                ' #Yes: this is mixed value
                                                bIsMixedValue = True
                                            End If
                                        End If

                                        ' Update varname
                                        vn = p.VarName
                                        ' Update value
                                        objValue = p.GetValue()
                                    Else
                                        ' Does this property hold a value other than found earlier?
                                        If (objValue IsNot Nothing) Then
                                            If (Not objValue.Equals(cell.Value)) Then
                                                ' #Yes: this is mixed value
                                                bIsMixedValue = True
                                            End If
                                        End If
                                        objValue = cell.Value
                                    End If
                                    ' There was at least one editable cell
                                    bHasEditableCells = True
                                End If
                            End If
                        End If
                    End If
                Next col
            Next row

            ' Switch control type
            If (bHasEditableCells And Not bIsMixedSelection) Then
                If bIsStandardValuesExclusive Then
                    Me.SetEditControl(eControlType.ComboBox, items)
                Else
                    Me.SetEditControl(eControlType.TextBox)
                End If
            End If

            ' Enable set label if the grid has editable cells that represent only one type of variable.
            If Me.m_lblSet IsNot Nothing Then
                Me.m_lblSet.Enabled = bHasEditableCells And Not bIsMixedSelection
                Me.m_lblSet.Visible = bIsInputGrid And bHasEditableCells
            End If

            ' Enable edit control if the grid has editable cells that represent only one type of variable.
            If Me.m_ctrlValue IsNot Nothing Then
                Me.m_ctrlValue.Enabled = bHasEditableCells And Not bIsMixedSelection
                Me.m_ctrlValue.Visible = bIsInputGrid And bHasEditableCells
                Me.m_ctrlValue.Text = ""

                ' ToDo_JS: replace text box with a dynamic control that is smartly configured to
                '   - Use cell variable metadata to determine a value range
                '   - Use cell editor properties to allow entry or selections (e.g. text box or combo box) via cTypeFormatterFactory.GetTypeFormatter
                '   - Use cell editor properties to limit the combo box to discreet values
                If ((objValue IsNot Nothing) And (bIsMixedValue = False)) Then
                    Select Case Me.m_controlType
                        Case eControlType.TextBox
                            If TypeOf objValue Is String Then
                                Me.m_ctrlValue.Text = CStr(objValue)
                            ElseIf (TypeOf objValue Is Single) Or (TypeOf objValue Is Double) Then
                                Try
                                    Me.m_ctrlValue.Text = Me.m_uic.StyleGuide.FormatNumber(CSng(objValue))
                                Catch ex As Exception
                                End Try
                            ElseIf (TypeOf objValue Is Integer) Then
                                Try
                                    Me.m_ctrlValue.Text = Me.m_uic.StyleGuide.FormatNumber(CInt(objValue))
                                Catch ex As Exception
                                End Try
                            ElseIf TypeOf objValue Is Boolean Then
                                Me.m_ctrlValue.Text = If(CBool(objValue) = True, "1", "0")
                            End If

                        Case eControlType.ComboBox
                            DirectCast(Me.m_ctrlValue, ToolStripComboBox).SelectedItem = objValue
                    End Select
                End If
            End If

            ' Enable set button if the grid has editable cells that represent only one type of variable.
            If Me.m_btnSet IsNot Nothing Then
                Me.m_btnSet.Enabled = bHasEditableCells And Not bIsMixedSelection
                Me.m_btnSet.Visible = bIsInputGrid And Not bIsStandardValuesExclusive And bHasEditableCells
            End If

            ' Show import button only for input forms - and when allowed to show
            If Me.m_btnImport IsNot Nothing Then
                Me.m_btnImport.Visible = bIsInputGrid And Me.ShowImportExport
            End If

            ' Show export button only  allowed to show
            If Me.m_btnExport IsNot Nothing Then
                Me.m_btnExport.Visible = Me.ShowImportExport
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Apply a string to all selected cells that are 
        ''' <see cref="SourceGrid2.DataModels.DataModelBase.EnableEdit">Editable</see>.
        ''' </summary>
        ''' <param name="newval">The value to apply.</param>
        ''' -------------------------------------------------------------------
        Private Sub ApplyValueToSelection(ByVal newval As Object)

            ' Get grid selection
            Dim sel As SourceGrid2.Selection = Me.m_grid.Selection
            Dim objValue As Object = Nothing

            ' To stop a flood of updates, and to halt any conflicting operations 
            ' while we're at it.
            If Not Me.m_uic.Core.SetBatchLock(cCore.eBatchLockType.Update) Then Return

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_APPLYVALUES)
            Me.m_grid.BeginBatchEdit()

            ' Iterate through cells
            ' JS 18May18: Greatly optimized logic to iterate over cells in the selection
            Dim r As SourceGrid2.Range = sel.GetRange
            ' Math.max construction prevens from looping into empty regions
            For row As Integer = Math.Max(0, r.Start.Row) To r.End.Row
                For col As Integer = Math.Max(0, r.Start.Column) To r.End.Column
                    If r.Contains(New SourceGrid2.Position(row, col)) Then
                        Dim cell As SourceGrid2.Cells.ICell = CType(Me.m_grid.GetCell(row, col), SourceGrid2.Cells.ICell)
                        If (cell IsNot Nothing) Then
                            If TypeOf cell Is PropertyCell Then
                                Dim pcell As PropertyCell = DirectCast(cell, PropertyCell)
                                If (pcell.Style And cStyleGuide.eStyleFlags.NotEditable) = 0 Then
                                    pcell.GetProperty().SetValue(newval)
                                End If
                            Else
                                If cell.DataModel.EnableEdit Then
                                    If (cell.DataModel.ValueType Is GetType(String)) Then
                                        objValue = newval
                                    ElseIf ((cell.DataModel.ValueType Is GetType(Single)) Or (cell.DataModel.ValueType Is GetType(Double))) Then
                                        Try
                                            If (TypeOf newval Is String) Then
                                                objValue = cStringUtils.ConvertToSingle(CStr(newval))
                                            Else
                                                objValue = newval
                                            End If
                                        Catch ex As Exception
                                        End Try
                                    ElseIf (cell.DataModel.ValueType Is GetType(Integer)) Then
                                        Try
                                            If (TypeOf newval Is String) Then
                                                objValue = cStringUtils.ConvertToInteger(CStr(newval))
                                            Else
                                                objValue = newval
                                            End If
                                        Catch ex As Exception
                                        End Try
                                    ElseIf (cell.DataModel.ValueType Is GetType(Boolean)) Then
                                        If (TypeOf newval Is String) Then
                                            objValue = (CStr(newval) = "1")
                                        Else
                                            objValue = newval
                                        End If
                                    Else
                                        objValue = newval
                                    End If

                                    Try
                                        cell.SetValue(New SourceGrid2.Position(cell.Row, cell.Column), objValue)
                                    Catch ex As Exception
                                        ' Whoah!
                                    End Try
                                End If
                            End If
                        End If ' Not a property cell
                    End If ' Cell isnot nothing
                Next
            Next

            Me.m_grid.EndBatchEdit()
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

            Me.m_uic.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.NotSet)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a csv file name from a concatenation of the EwE model name, 
        ''' if any, and the grid dataname, if any. The resulting file name is 
        ''' made safe for use by the current OS.
        ''' </summary>
        ''' <returns>A file name for the grid data.</returns>
        ''' -------------------------------------------------------------------
        Private Function GetCSVFileName() As String

            Dim sbFileName As New StringBuilder()

            If (Me.m_uic.Core.EwEModel IsNot Nothing) Then
                sbFileName.Append(Me.m_uic.Core.EwEModel.Name)
                sbFileName.Append("-")
            End If

            If (String.IsNullOrWhiteSpace(Me.m_grid.DataName)) Then
                sbFileName.Append("grid")
            Else
                sbFileName.Append(Me.m_grid.DataName)
            End If
            sbFileName.Append(".csv")

            Return cFileUtils.ToValidFileName(sbFileName.ToString, False)

        End Function

        Public Function ImportGridFromCSV() As Boolean

            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim cmdOF As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)
            Dim msg As cMessage = Nothing
            Dim fs As Stream = Nothing
            Dim sr As StreamReader = Nothing
            Dim bSuccess As Boolean = True

            cmdOF.Invoke(Me.GetCSVFileName(), My.Resources.FILEFILTER_CSV)
            If (cmdOF.Result <> System.Windows.Forms.DialogResult.OK) Then Return bSuccess

            Try
                fs = New FileStream(cmdOF.FileName, _
                                    FileMode.Open, _
                                    FileAccess.Read, _
                                    FileShare.ReadWrite Or FileShare.Delete Or FileShare.Inheritable)
            Catch ex As Exception
                msg = New cMessage(String.Format(My.Resources.GENERIC_FILELOAD_FAILURE, Me.m_grid.DataName, cmdOF.FileName, ex.Message), _
                                  eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
                bSuccess = False
            End Try

            If (fs IsNot Nothing) Then

                sr = New StreamReader(fs)
                Me.m_grid.ReadContent(sr)
                sr.Close()
                fs.Close()

                msg = New cMessage(String.Format(My.Resources.GENERIC_FILELOAD_SUCCES, Me.m_grid.DataName, cmdOF.FileName), _
                    eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
                msg.Hyperlink = Path.GetDirectoryName(cmdOF.FileName)

            End If

            ' Log!
            Me.m_uic.Core.Messages.SendMessage(msg)
            Return bSuccess

        End Function

        Public Sub ExportGridToCSV()

            If (Me.m_uic Is Nothing) Then Return

            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim cmdSF As cFileSaveCommand = DirectCast(cmdh.GetCommand(cFileSaveCommand.COMMAND_NAME), cFileSaveCommand)
            Dim msg As cMessage = Nothing
            Dim fs As Stream = Nothing
            Dim sw As StreamWriter = Nothing

            cmdSF.Invoke(Me.GetCSVFileName(), My.Resources.FILEFILTER_CSV)
            If (cmdSF.Result <> System.Windows.Forms.DialogResult.OK) Then Return

            Try
                'Create the file
                fs = New FileStream(cmdSF.FileName, FileMode.Create, FileAccess.Write, FileShare.None)
            Catch ex As Exception
                ' Woops!
                msg = New cMessage(String.Format(My.Resources.GENERIC_FILESAVE_FAILURE, Me.m_grid.DataName, cmdSF.FileName, ex.Message), _
                                  eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Warning)
            End Try

            If (fs IsNot Nothing) Then
                sw = New StreamWriter(fs)
                Me.m_grid.WriteContent(sw)
                sw.Close()
                fs.Close()

                msg = New cMessage(String.Format(My.Resources.GENERIC_FILESAVE_SUCCES, Me.m_grid.DataName, cmdSF.FileName), _
                                   eMessageType.DataExport, eCoreComponentType.External, eMessageImportance.Information)
                msg.Hyperlink = Path.GetDirectoryName(cmdSF.FileName)
            End If

            ' Log!
            Me.m_uic.Core.Messages.SendMessage(msg)

        End Sub

        Private Enum eControlType As Byte
            NotSet = 0
            TextBox
            ComboBox
        End Enum

        Private m_controlType As eControlType = eControlType.NotSet

        Private Sub SetEditControl(controltype As eControlType, Optional items As ICollection = Nothing)

            Select Case Me.m_controlType

                Case eControlType.TextBox
                    Dim ctrl As ToolStripTextBox = DirectCast(Me.m_ctrlValue, ToolStripTextBox)
                    RemoveHandler ctrl.KeyDown, AddressOf OnSetBoxKeyDown
                    RemoveHandler ctrl.Enter, AddressOf OnSetBoxEnter
                    RemoveHandler ctrl.Leave, AddressOf OnSetBoxLeave
                    ctrl.Dispose()

                Case eControlType.ComboBox
                    Dim ctrl As ToolStripComboBox = DirectCast(Me.m_ctrlValue, ToolStripComboBox)
                    RemoveHandler ctrl.SelectedIndexChanged, AddressOf OnSelectedValueChanged
                    ctrl.Dispose()

            End Select

            If (Me.m_ctrlValue IsNot Nothing) Then
                Me.m_ts.Items.Remove(Me.m_ctrlValue)
            End If

            Me.m_ctrlValue = Nothing
            Me.m_controlType = controltype

            Select Case controltype

                Case eControlType.TextBox
                    Dim ctrl As New ToolStripTextBox("~tsqeValue")
                    ctrl.AcceptsReturn = True
                    AddHandler ctrl.Enter, AddressOf OnSetBoxEnter
                    AddHandler ctrl.Leave, AddressOf OnSetBoxLeave
                    AddHandler ctrl.KeyDown, AddressOf OnSetBoxKeyDown
                    Me.m_ctrlValue = ctrl

                Case eControlType.ComboBox
                    Dim ctrl As New ToolStripComboBox("~tsqeValue")
                    ctrl.DropDownStyle = ComboBoxStyle.DropDownList
                    AddHandler ctrl.SelectedIndexChanged, AddressOf OnSelectedValueChanged
                    Me.m_ctrlValue = ctrl
                    If (items IsNot Nothing) Then
                        For Each item As Object In items
                            ctrl.Items.Add(item)
                        Next
                    End If
            End Select

            If (Me.m_ctrlValue IsNot Nothing) Then
                If (cSystemUtils.IsRightToLeft) Then
                    Me.m_ctrlValue.Alignment = ToolStripItemAlignment.Left
                Else
                    Me.m_ctrlValue.Alignment = ToolStripItemAlignment.Right
                End If

                If (Me.m_iValuePos >= 0) Then
                    ' The first time the control is added iValuePos is determined. Do not try
                    ' to insert the item if iValuePos is not known yet
                    If (Me.m_iValuePos > Me.m_ts.Items.Count) Then
                        Me.m_ts.Items.Add(Me.m_ctrlValue)
                    Else
                        Me.m_ts.Items.Insert(Me.m_iValuePos, Me.m_ctrlValue)
                    End If
                End If
            End If
        End Sub

#End Region ' Internal implementation

    End Class

End Namespace
