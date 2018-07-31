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
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterface.Ecospace.Basemap.Layers
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecospace.Basemap

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class dlgImportLayerDataXYZ
        Inherits Form

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_lLayers As New List(Of cEcospaceLayer)
        Private m_data As cEcospaceImportExportXYData = Nothing

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal uic As cUIContext)
            Me.InitializeComponent()
            Me.m_uic = uic
        End Sub

#End Region ' Constructor

#Region " Public properties "

        Public Property Layers() As cEcospaceLayer()
            Get
                Return Me.m_lLayers.ToArray()
            End Get
            Set(ByVal aLayers As cEcospaceLayer())
                Me.m_lLayers.Clear()

                If aLayers Is Nothing Then Return
                If aLayers.Length = 0 Then Return

                Me.m_lLayers.AddRange(aLayers)
            End Set
        End Property

#End Region ' Public properties

#Region " Form overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.DesignMode = True) Then Return

            Debug.Assert(Me.m_uic IsNot Nothing)

            Dim f As New cLayerFactoryInternal()
            Dim bm As cEcospaceBasemap = Me.m_uic.Core.EcospaceBasemap

            If (Me.m_lLayers.Count = 0) Then
                For Each layer As cEcospaceLayer In bm.Layers(eVarNameFlags.NotSet)
                    If Not TypeOf layer Is cEcospaceLayerVelocity Then
                        Me.m_lLayers.Add(layer)
                    End If
                Next
            End If
            Me.m_grid.Layers = Me.m_lLayers.ToArray
            Me.m_grid.UIContext = Me.m_uic

            AddHandler Me.m_grid.MappingChanged, AddressOf UpdateControls

            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

            RemoveHandler Me.m_grid.MappingChanged, AddressOf UpdateControls

            Me.m_grid.Layers = Nothing
            Me.m_grid.UIContext = Nothing

            MyBase.OnFormClosed(e)

        End Sub

#End Region ' Form overrides

#Region " Events "

        Protected Overrides Sub OnDragEnter(e As System.Windows.Forms.DragEventArgs)
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effect = DragDropEffects.All
            End If
            MyBase.OnDragEnter(e)
        End Sub

        Protected Overrides Sub OnDragDrop(e As System.Windows.Forms.DragEventArgs)
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Try
                    Dim astrFiles() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
                    If astrFiles.Length > 0 Then
                        Me.m_tbInput.Text = astrFiles(0)
                        Me.ReadCSVFile()
                    End If
                Catch ex As Exception

                End Try
            End If
            MyBase.OnDragDrop(e)
        End Sub

        Private Sub OnBrowseInput(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnBrowseInput.Click

            ' Browse via EwE6 open file dialog 
            Dim cmdh As cCommandHandler = Me.m_uic.CommandHandler
            Dim foc As cFileOpenCommand = TryCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)

            ' Sanity check
            If (foc Is Nothing) Then Return

            foc.Invoke(Me.m_tbInput.Text, SharedResources.FILEFILTER_CSV, 0, Me.Text)

            If (foc.Result = System.Windows.Forms.DialogResult.OK) Then
                Me.m_tbInput.Text = foc.FileName
                Me.ReadCSVFields()
            End If

        End Sub

        Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_bntOK.Click

            If ReadCSVFile() Then
                If Not Me.LoadMappedLayers() Then Return
            End If

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnRowColFieldChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbRow.SelectedIndexChanged, m_cmbCol.SelectedIndexChanged
            Me.UpdateControls()
        End Sub

#End Region ' Events

#Region " Internals "

        Private Function ReadCSVFile() As Boolean
            Dim bSuccess As Boolean = True

            'Make sure the initialization all happened correctly
            'Me.m_data was initialized in me.ReadCSVFields()
            Debug.Assert(Me.m_data IsNot Nothing, Me.ToString + ".ReadCSVFile() cEcospaceImportExportXYData has not been initialized correctly.")

            If Not Me.m_data.ReadXYFile(Me.m_tbInput.Text, Me.RowField, Me.ColField) Then
                Dim msg As New cMessage(cStringUtils.Localize(SharedResources.FILE_LOAD_ERROR_READ, Me.m_tbInput.Text),
                                        eMessageType.Any, EwEUtils.Core.eCoreComponentType.External, eMessageImportance.Warning)
                Me.m_uic.Core.Messages.SendMessage(msg)
                bSuccess = False
            End If

            Return bSuccess

        End Function

        Private Function ReadCSVFields() As Boolean

            ' Create data buffer
            Me.m_data = New cEcospaceImportExportXYData(Me.m_uic.Core)

            Me.m_grid.Mappings.Clear()

            ' Read file
            If (Not Me.m_data.ReadXYFields(Me.m_tbInput.Text)) Then
                Return False
            End If

            ' Get fields from the XY Reader
            Dim astrFields As String() = Me.m_data.Fields

            ' Show in UI
            Me.m_cmbRow.Items.AddRange(astrFields) : Me.m_cmbRow.SelectedIndex = Me.m_cmbRow.FindString("Row")
            Me.m_cmbCol.Items.AddRange(astrFields) : Me.m_cmbCol.SelectedIndex = Me.m_cmbCol.FindString("Column")

            For Each l As cEcospaceLayer In Me.m_lLayers
                If Array.IndexOf(astrFields, l.Name) > -1 Then
                    Me.m_grid.Mappings(l) = l.Name
                End If
            Next

            ' Last apply fields
            Me.m_grid.Fields = astrFields

            Return True

        End Function

        Private Function LoadMappedLayers() As Boolean

            Dim dtMappings As Dictionary(Of cEcospaceLayer, String) = Me.m_grid.Mappings()
            Dim bm As cEcospaceBasemap = Me.m_uic.Core.EcospaceBasemap
            Dim layer As cEcospaceLayer = Nothing
            Dim md As cVariableMetaData = Nothing
            Dim strField As String = ""
            Dim iRow As Integer = 0
            Dim iCol As Integer = 0
            Dim iCell As Integer = 0
            Dim sNoData As Single = 0.0!

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core, My.Resources.STATUS_APPLYVALUES)
            Me.m_uic.Core.SetBatchLock(cCore.eBatchLockType.Update)

            Try

                ' For each mapped field
                For Each layer In dtMappings.Keys

                    strField = dtMappings(layer)
                    If Not String.IsNullOrEmpty(strField.Trim) Then

                        md = layer.GetVariableMetadata(layer.VarName)
                        If (md IsNot Nothing) Then
                            sNoData = CSng(md.NullValue)
                        End If

                        ' Clear layer
                        For iRow = 1 To bm.InRow
                            For iCol = 1 To bm.InCol
                                layer.Cell(iRow, iCol) = sNoData
                            Next
                        Next

                        ' Load layer
                        For iRow = 1 To bm.InRow
                            For iCol = 1 To bm.InCol
                                Dim val As Object = Me.m_data.Value(iRow, iCol, strField)
                                Dim sVal As Single = sNoData
                                If (val IsNot Nothing) Then
                                    sVal = cStringUtils.ConvertToSingle(CStr(val))
                                    If (sVal = cCore.NULL_VALUE) Then sVal = sNoData
                                End If
                                layer.Cell(iRow, iCol) = val
                            Next
                        Next
                    End If

                    layer.Invalidate()

                Next layer

            Catch ex As Exception

            End Try

            Me.m_uic.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecospace)
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)

            Me.m_uic.Core.onChanged(Me.m_uic.Core.EcospaceBasemap)

            Return True

        End Function

        Private Property RowField() As String
            Get
                Return Me.m_cmbRow.Text
            End Get
            Set(ByVal value As String)
                Me.m_cmbRow.Text = value
            End Set
        End Property

        Private Property ColField() As String
            Get
                Return Me.m_cmbCol.Text
            End Get
            Set(ByVal value As String)
                Me.m_cmbCol.Text = value
            End Set
        End Property

        Private Sub UpdateControls()

            Dim bHasFile As Boolean = File.Exists(Me.m_tbInput.Text)
            Dim bHasRowCol As Boolean = (Me.m_cmbCol.SelectedIndex >= 0) And (Me.m_cmbRow.SelectedIndex >= 0)
            Dim bHasMappings As Boolean = (Me.m_grid.HasMappings())

            Me.m_cmbRow.Enabled = (Me.m_cmbRow.Items.Count > 0)
            Me.m_cmbCol.Enabled = (Me.m_cmbCol.Items.Count > 0)

            Me.m_grid.Enabled = bHasFile
            Me.m_bntOK.Enabled = bHasFile And bHasRowCol And bHasMappings

        End Sub

#End Region ' Internals

#Region " DevStudio generated surprises "

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgImportLayerDataXYZ))
            Me.m_lblSource = New System.Windows.Forms.Label()
            Me.m_tbInput = New System.Windows.Forms.TextBox()
            Me.m_btnBrowseInput = New System.Windows.Forms.Button()
            Me.m_lblMappings = New System.Windows.Forms.Label()
            Me.m_tlpOkCancel = New System.Windows.Forms.TableLayoutPanel()
            Me.m_bntOK = New System.Windows.Forms.Button()
            Me.m_btnCancel = New System.Windows.Forms.Button()
            Me.m_grid = New ScientificInterface.Ecospace.Basemap.gridImportLayerMappings()
            Me.m_lblRow = New System.Windows.Forms.Label()
            Me.m_cmbRow = New System.Windows.Forms.ComboBox()
            Me.m_cmbCol = New System.Windows.Forms.ComboBox()
            Me.m_lblCol = New System.Windows.Forms.Label()
            Me.m_tlpOkCancel.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_lblSource
            '
            resources.ApplyResources(Me.m_lblSource, "m_lblSource")
            Me.m_lblSource.Name = "m_lblSource"
            '
            'm_tbInput
            '
            Me.m_tbInput.AllowDrop = True
            resources.ApplyResources(Me.m_tbInput, "m_tbInput")
            Me.m_tbInput.Name = "m_tbInput"
            Me.m_tbInput.ReadOnly = True
            Me.m_tbInput.TabStop = False
            '
            'm_btnBrowseInput
            '
            resources.ApplyResources(Me.m_btnBrowseInput, "m_btnBrowseInput")
            Me.m_btnBrowseInput.Name = "m_btnBrowseInput"
            Me.m_btnBrowseInput.UseVisualStyleBackColor = True
            '
            'm_lblMappings
            '
            resources.ApplyResources(Me.m_lblMappings, "m_lblMappings")
            Me.m_lblMappings.Name = "m_lblMappings"
            '
            'm_tlpOkCancel
            '
            resources.ApplyResources(Me.m_tlpOkCancel, "m_tlpOkCancel")
            Me.m_tlpOkCancel.Controls.Add(Me.m_bntOK, 0, 0)
            Me.m_tlpOkCancel.Controls.Add(Me.m_btnCancel, 1, 0)
            Me.m_tlpOkCancel.Name = "m_tlpOkCancel"
            '
            'm_bntOK
            '
            resources.ApplyResources(Me.m_bntOK, "m_bntOK")
            Me.m_bntOK.Name = "m_bntOK"
            '
            'm_btnCancel
            '
            resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
            Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_btnCancel.Name = "m_btnCancel"
            '
            'm_grid
            '
            Me.m_grid.AllowBlockSelect = True
            resources.ApplyResources(Me.m_grid, "m_grid")
            Me.m_grid.AutoSizeMinHeight = 10
            Me.m_grid.AutoSizeMinWidth = 10
            Me.m_grid.AutoStretchColumnsToFitWidth = False
            Me.m_grid.AutoStretchRowsToFitHeight = False
            Me.m_grid.BackColor = System.Drawing.Color.White
            Me.m_grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.m_grid.ContextMenuStyle = CType((((SourceGrid2.ContextMenuStyle.ColumnResize Or SourceGrid2.ContextMenuStyle.AutoSize) _
                Or SourceGrid2.ContextMenuStyle.CopyPasteSelection) _
                Or SourceGrid2.ContextMenuStyle.CellContextMenu), SourceGrid2.ContextMenuStyle)
            Me.m_grid.CustomSort = False
            Me.m_grid.DataName = "grid content"
            Me.m_grid.Fields = New String() {"(none)"}
            Me.m_grid.FixedColumnWidths = False
            Me.m_grid.FocusStyle = SourceGrid2.FocusStyle.None
            Me.m_grid.GridToolTipActive = True
            Me.m_grid.IsLayoutSuspended = False
            Me.m_grid.IsOutputGrid = True
            Me.m_grid.Layers = Nothing
            Me.m_grid.Name = "m_grid"
            Me.m_grid.SpecialKeys = CType((((((((((SourceGrid2.GridSpecialKeys.Ctrl_C Or SourceGrid2.GridSpecialKeys.Ctrl_V) _
                Or SourceGrid2.GridSpecialKeys.Ctrl_X) _
                Or SourceGrid2.GridSpecialKeys.Delete) _
                Or SourceGrid2.GridSpecialKeys.Arrows) _
                Or SourceGrid2.GridSpecialKeys.Tab) _
                Or SourceGrid2.GridSpecialKeys.PageDownUp) _
                Or SourceGrid2.GridSpecialKeys.Enter) _
                Or SourceGrid2.GridSpecialKeys.Escape) _
                Or SourceGrid2.GridSpecialKeys.Backspace), SourceGrid2.GridSpecialKeys)
            Me.m_grid.UIContext = Nothing
            '
            'm_lblRow
            '
            resources.ApplyResources(Me.m_lblRow, "m_lblRow")
            Me.m_lblRow.Name = "m_lblRow"
            '
            'm_cmbRow
            '
            Me.m_cmbRow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbRow.FormattingEnabled = True
            resources.ApplyResources(Me.m_cmbRow, "m_cmbRow")
            Me.m_cmbRow.Name = "m_cmbRow"
            '
            'm_cmbCol
            '
            Me.m_cmbCol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_cmbCol.FormattingEnabled = True
            resources.ApplyResources(Me.m_cmbCol, "m_cmbCol")
            Me.m_cmbCol.Name = "m_cmbCol"
            '
            'm_lblCol
            '
            resources.ApplyResources(Me.m_lblCol, "m_lblCol")
            Me.m_lblCol.Name = "m_lblCol"
            '
            'dlgImportLayerDataXYZ
            '
            Me.AcceptButton = Me.m_bntOK
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.CancelButton = Me.m_btnCancel
            Me.ControlBox = False
            Me.Controls.Add(Me.m_cmbCol)
            Me.Controls.Add(Me.m_cmbRow)
            Me.Controls.Add(Me.m_lblCol)
            Me.Controls.Add(Me.m_lblRow)
            Me.Controls.Add(Me.m_tlpOkCancel)
            Me.Controls.Add(Me.m_grid)
            Me.Controls.Add(Me.m_lblMappings)
            Me.Controls.Add(Me.m_tbInput)
            Me.Controls.Add(Me.m_btnBrowseInput)
            Me.Controls.Add(Me.m_lblSource)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.Name = "dlgImportLayerDataXYZ"
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.m_tlpOkCancel.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private WithEvents m_lblSource As System.Windows.Forms.Label
        Private WithEvents m_tbInput As System.Windows.Forms.TextBox
        Private WithEvents m_btnBrowseInput As System.Windows.Forms.Button
        Private WithEvents m_lblMappings As System.Windows.Forms.Label
        Private WithEvents m_grid As gridImportLayerMappings
        Private WithEvents m_tlpOkCancel As System.Windows.Forms.TableLayoutPanel
        Private WithEvents m_bntOK As System.Windows.Forms.Button
        Private WithEvents m_btnCancel As System.Windows.Forms.Button
        Private WithEvents m_lblRow As System.Windows.Forms.Label
        Private WithEvents m_cmbRow As System.Windows.Forms.ComboBox
        Private WithEvents m_cmbCol As System.Windows.Forms.ComboBox
        Private WithEvents m_lblCol As System.Windows.Forms.Label

#End Region ' DevStudio generated surprises

    End Class

End Namespace ' Ecospace.Basemap
