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
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

' ToDo: Populate dataset details panel
' ToDo: Respond to configuration / name changes
' ToDo: Enable varname hierarchy in TreeView

Namespace Ecospace.Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Configuration interface for setting up a <see cref="cSpatialDataAdapter"/>.
    ''' </summary>
    ''' <remarks>
    ''' This interface allows users to define new datasets, configure datasets, 
    ''' change dataset selections, define new converters and configure the
    ''' existing converter.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Class dlgApplyConnection
        Implements IUIElement
        Implements IDisposable

#Region " Private variables "

        Private m_uic As cUIContext = Nothing
        Private m_manConn As cSpatialDataConnectionManager = Nothing
        Private m_manSets As cSpatialDataSetManager = Nothing

        ''' <summary>Selected data adapter</summary>
        Private m_adt As cSpatialDataAdapter = Nothing
        ''' <summary>Selected layer index</summary>
        Private m_iLayer As Integer = -1

        ''' <summary>Flag to break looped layer change updates/notifications</summary>
        Private m_bInUpdate As Boolean = False
        Private m_bIsChanged As Boolean = False
        Private m_bIsScaling As Boolean = False

        Private m_fpScale As cEwEFormatProvider = Nothing

#End Region ' Private variables

#Region " Constructor "

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="uic"></param>
        ''' <param name="adt"></param>
        ''' <param name="layer"></param>
        ''' <param name="conn"></param>
        ''' <remarks></remarks>
        Public Sub New(uic As cUIContext, adt As cSpatialDataAdapter, layer As cEcospaceLayer, _
                       Optional conn As cSpatialDataConnection = Nothing)

            Me.InitializeComponent()

            Debug.Assert(adt IsNot Nothing)

            Me.m_adt = adt
            Me.m_iLayer = layer.Index
            Me.m_bIsScaling = (TypeOf Me.m_adt Is cSpatialScalarDataAdapterBase)

            Me.Text = cStringUtils.Localize(Me.Text, layer.Name)

            Me.UIContext = uic

            If (Me.m_adt IsNot Nothing) Then
                For Each conn2 As cSpatialDataConnection In Me.m_adt.Connections(Me.m_iLayer)
                    Me.m_gridConnections.AddConnection(conn2, ReferenceEquals(conn2, conn))
                Next
            End If

        End Sub

#End Region ' Constructor

#Region " Form overrides "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(uic As cUIContext)

                If (Me.m_uic IsNot Nothing) Then

                    ' Disconnect from data objects first; we do not want disconnecting UI elements from screwing up the last configuration
                    Me.m_adt = Nothing
                    Me.m_iLayer = Nothing

                    Me.m_lbSourceDatasets.UIContext = Nothing
                    Me.m_gridConnections.UIContext = Nothing

                    Me.m_manSets.Save()
                    Me.m_manSets = Nothing
                    Me.m_manConn = Nothing
                End If

                Me.m_uic = uic

                If (Me.m_uic IsNot Nothing) Then
                    ' Set new
                    Me.m_manConn = Me.m_uic.Core.SpatialDataConnectionManager
                    Me.m_manSets = Me.m_manConn.DatasetManager
                    Me.m_lbSourceDatasets.UIContext = Me.m_uic
                    Me.m_gridConnections.UIContext = Me.m_uic
                End If
            End Set
        End Property

        Protected Overrides Sub OnLoad(e As System.EventArgs)

            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_bInUpdate = True

            Me.m_tslFilter.Image = SharedResources.FilterHS
            Me.m_tsbnCaseSensitive.Image = SharedResources.CaseSensitive
            Me.m_tsbnShowIncompatibleConnections.Image = SharedResources.Warning
            ' Kick!
            Me.RefreshDatasetList()

            ' Dynamic makeup
            Me.m_tsbnDefineConnections.Image = SharedResources.Database
            Me.m_fpScale = New cEwEFormatProvider(Me.UIContext, Me.m_tbxScale, GetType(Single))

            If (Me.m_bIsScaling) Then
                AddHandler Me.m_fpScale.OnValueChanged, AddressOf OnScaleChanged
                Me.m_fpScale.Style = cStyleGuide.eStyleFlags.OK
            Else
                Me.m_fpScale.Style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null
            End If

            Me.m_cbEnabled.Checked = Me.m_adt.IsEnabled(Me.m_iLayer)

            ' Start listening to grid events
            AddHandler Me.m_gridConnections.OnSelectionChanged, AddressOf OnSelectDS

            ' ToDo: globalize this
            cToolTipShared.GetInstance().SetToolTip(Me.m_btnAdd, "Connect")
            cToolTipShared.GetInstance().SetToolTip(Me.m_btnRemove, "Disconnect")

            Me.m_bInUpdate = False
            Me.CenterToParent()

            Me.UpdateDatasetPanel()
            Me.UpdateConversionPanel()
            Me.UpdateScalingPanel()
            Me.UpdateControls()

        End Sub

        Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)

            Me.m_adt.IsEnabled(Me.m_iLayer) = Me.m_cbEnabled.Checked

            If Me.m_bIsChanged Then
                Me.m_manConn.Update(Me.m_adt)
                Me.m_bIsChanged = False
            End If

            If (Me.m_bIsScaling) Then
                RemoveHandler Me.m_fpScale.OnValueChanged, AddressOf OnScaleChanged
            End If
            Me.m_fpScale.Release()

            RemoveHandler Me.m_gridConnections.OnSelectionChanged, AddressOf OnSelectDS
            Me.UIContext = Nothing
            MyBase.OnFormClosed(e)

        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                    components = Nothing
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

#End Region ' Form overrides

#Region " Control events "

#Region " Manage datasets "

        Private Sub OnManageConnections(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnDefineConnections.Click
            Try
                Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("EditSpatialDatasets")
                If (cmd IsNot Nothing) Then
                    cmd.Invoke()
                    Me.RefreshDatasetList()
                    Me.UpdateControls()
                End If
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Manage connections 

#Region " Connections list "

        Private Sub OnNameFilterChanged(sender As Object, e As EventArgs) _
            Handles m_tstbFilter.TextChanged
            Try
                Me.m_lbSourceDatasets.TextFilter = Me.m_tstbFilter.Text
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnToggleCaseSensitive(sender As Object, e As EventArgs) _
            Handles m_tsbnCaseSensitive.CheckedChanged
            Try
                Me.m_lbSourceDatasets.IsTextFilterCaseSensitive = Me.m_tsbnCaseSensitive.Checked
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnToggleFilterByVariable(sender As System.Object, e As System.EventArgs) _
            Handles m_tsbnShowIncompatibleConnections.CheckedChanged
            Try
                If (Me.m_tsbnShowIncompatibleConnections.Checked) Or (Me.m_adt Is Nothing) Then
                    Me.m_lbSourceDatasets.VariableFilter = eVarNameFlags.NotSet
                Else
                    Me.m_lbSourceDatasets.VariableFilter = Me.m_adt.VarName
                End If
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnAddDataset(sender As System.Object, e As System.EventArgs) _
            Handles m_btnAdd.Click, m_lbSourceDatasets.DoubleClick

            cApplicationStatusNotifier.StartProgress(Me.m_uic.Core)
            Try

                Dim conn As cSpatialDataConnection = Me.m_adt.AddConnection(Me.m_iLayer)
                Dim ds As ISpatialDataSet = Me.m_lbSourceDatasets.SelectedDataset

                If (ds IsNot Nothing) And (conn IsNot Nothing) Then
                    Me.m_bInUpdate = True
                    conn.Dataset = ds
                    Me.LayerChanged()
                    Me.m_bInUpdate = False

                    Me.m_gridConnections.AddConnection(conn, True)
                End If
            Catch ex As Exception

            End Try
            cApplicationStatusNotifier.EndProgress(Me.m_uic.Core)
            Me.UpdateControls()

        End Sub

        Private Sub OnRemoveSelectedConnections(sender As System.Object, e As System.EventArgs) _
            Handles m_btnRemove.Click

            Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection

            Me.m_bInUpdate = True
            Try
                Me.m_adt.RemoveConnection(Me.m_iLayer, conn)
                Me.m_gridConnections.RemoveConnection(conn)
            Catch ex As Exception

            End Try
            Me.UpdateControls()
            Me.m_bInUpdate = False

            Me.LayerChanged()

        End Sub

        Private Sub OnDatasetSelected(sender As System.Object, e As System.EventArgs) _
            Handles m_lbSourceDatasets.SelectedIndexChanged
            Try
                Me.UpdateControls()
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Candidate connections

#Region " Selected connections "

        ''' <summary>
        ''' User has selected a dataset for the current adapter and layer.
        ''' </summary>
        Private Sub OnSelectDS()

            If Me.m_bInUpdate Then Return
            Try
                Me.m_bInUpdate = True
                ' Brilliant
                Me.SelectedDataset = Me.SelectedDataset
                Me.m_bInUpdate = False
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

#End Region ' Selected connections

#Region " Dataset config "

        ''' <summary>
        ''' User wants to configure the currently selected dataset.
        ''' </summary>
        Private Sub OnConfigDS(sender As System.Object, e As System.EventArgs) Handles m_btnConfigDS.Click

            Me.Cursor = Cursors.WaitCursor
            Try
                Dim iRow As Integer = Me.m_gridConnections.SelectedRow
                Me.ConfigDataset(Me.SelectedDataset)
                Me.m_gridConnections.RefreshContent()
                Me.m_gridConnections.SelectRow(iRow)
                'Me.m_manSets.IndexDataset = Me.SelectedDataset
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                cLog.Write(ex, "ucConficAdapter::OnConfigureDS")
            End Try
            Me.Cursor = Cursors.Default
            Me.LayerChanged()

        End Sub

#End Region ' Dataset config

#Region " Converters "

        ''' <summary>
        ''' Event handler for customizing how converters are displayed in this UI.
        ''' </summary>
        Private Sub OnFormatCV(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
            Handles m_cmbConverter.Format

            Dim fmt As New cSpatialConverterFormatter()
            If e.ListItem.Equals(String.Empty) Then
                e.Value = fmt.GetDescriptor(Nothing)
            Else
                e.Value = fmt.GetDescriptor(e.ListItem)
            End If
        End Sub

        ''' <summary>
        ''' User wants to configure the currently selected converter.
        ''' </summary>
        Private Sub OnConfigCV(sender As System.Object, e As System.EventArgs) _
            Handles m_btnConfigCV.Click, m_btnConfigDS.Click
            Try
                Me.ConfigConverter(Me.SelectedConverter)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' User has selected a converter for the current adapter and layer.
        ''' </summary>
        Private Sub OnSelectCV(sender As System.Object, e As System.EventArgs) _
            Handles m_cmbConverter.SelectedIndexChanged
            Try
                Dim obj As Object = Me.m_cmbConverter.SelectedItem
                If String.Empty.Equals(obj) Then
                    Me.SelectedConverter = Nothing
                Else
                    Me.SelectedConverter = DirectCast(obj, ISpatialDataConverter)
                End If
                Me.UpdateControls()
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Sub

#End Region ' Converters

#Region " Scaling "

        Private Sub OnScaleTextChanged(sender As Object, e As EventArgs) _
            Handles m_tbxScale.TextChanged

            Me.m_rbRelative.Checked = True

        End Sub


        Private Sub OnDatScaleTypeChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_rbAbsolute.CheckedChanged, m_rbRelative.CheckedChanged

            Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection()
            Dim iLayer As Integer = m_iLayer

            If (Me.m_bInUpdate) Then Return
            If (conn Is Nothing) Then Return

            Try
                If (Me.m_bIsScaling) Then
                    If (Me.m_rbAbsolute.Checked) Then
                        conn.ScaleType = cSpatialScalarDataAdapterBase.eScaleType.Absolute
                    Else
                        conn.ScaleType = cSpatialScalarDataAdapterBase.eScaleType.Relative
                        conn.Scale = CSng(Me.m_fpScale.Value)
                    End If

                    ' Invalidate the cached data for this dataset
                    ' ToDo_JS: Make dataset clearing more sublte. 
                    '          This statement deletes cached data for ALL scenarios a dataset is cached for. It should only
                    '          clear the cached data for the current Ecospace scenario. Oof. Ok, at least it works...
                    cSpatialDataCache.DefaultDataCache.Clear(Me.SelectedDataset)

                    Me.LayerChanged()

                End If
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

        Private Sub OnScaleChanged(sender As Object, e As System.EventArgs)

            If Me.m_bInUpdate Then Return

            Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection()
            If (conn Is Nothing) Then Return

            Debug.Assert(Me.m_bIsScaling)

            Try
                conn.Scale = CSng(Me.m_fpScale.Value)
                conn.ScaleType = cSpatialScalarDataAdapterBase.eScaleType.Relative

                ' Invalidate the cached data for this dataset
                cSpatialDataCache.DefaultDataCache.Clear(Me.SelectedDataset)

                Me.LayerChanged()
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

        Private Sub OnCalculateScale(sender As System.Object, e As System.EventArgs) _
            Handles m_btnCalculate.Click

            Me.m_bInUpdate = True

            Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection()
            If (conn Is Nothing) Then Return

            Try

                Debug.Assert(Me.m_bIsScaling)

                ' Wait for indexing to stop
                'While Me.SelectedDataset.IsIndexing()
                ' JS: at least try to stop the indexing process
                Me.m_manSets.IndexDataset = Nothing
                'End While

                Me.UpdateControls()
                Dim ssda As cSpatialScalarDataAdapterBase = DirectCast(Me.m_adt, cSpatialScalarDataAdapterBase)

                Dim iStartTimeStep As Integer = Math.Max(1, Me.m_uic.Core.AbsoluteTimeToEcospaceTimestep(Me.SelectedDataset.TimeStart))
                Dim dtStartDate As DateTime = Me.m_uic.Core.EcospaceTimestepToAbsoluteTime(iStartTimeStep)
                Dim dScale As Double = 1.0
                Dim msg As cMessage = Nothing

                ' Perform calculation
                Select Case ssda.CalculateScaleFromEcopathTimePeriod(m_iLayer, conn, iStartTimeStep, dScale)

                    Case cDatasetCompatilibity.eCompatibilityTypes.NotSet
                        msg = New cMessage(My.Resources.PROMPT_SPATIALTEMPORAL_CALC_NOINDEX,
                                           eMessageType.Any, EwEUtils.Core.eCoreComponentType.Ecotracer, eMessageImportance.Information)
                    Case cDatasetCompatilibity.eCompatibilityTypes.Errors,
                         cDatasetCompatilibity.eCompatibilityTypes.NoTemporal
                        msg = New cMessage(cStringUtils.Localize(My.Resources.PROMPT_SPATIALTEMPORAL_CALC_NODATA, dtStartDate.ToShortDateString()),
                                           eMessageType.Any, EwEUtils.Core.eCoreComponentType.Ecotracer, eMessageImportance.Warning)
                    Case cDatasetCompatilibity.eCompatibilityTypes.NoSpatial
                        msg = New cMessage(cStringUtils.Localize(My.Resources.PROMPT_SPATIALTEMPORAL_CALC_NOOVERLAP, dtStartDate.ToShortDateString()),
                                           eMessageType.Any, EwEUtils.Core.eCoreComponentType.Ecotracer, eMessageImportance.Warning)
                    Case Else
                        ' Only when ok
                        Me.m_fpScale.Value = CSng(dScale)
                        conn.Scale = CSng(dScale)
                        conn.ScaleType = cSpatialScalarDataAdapterBase.eScaleType.Relative
                        Me.LayerChanged()

                End Select

                ' Got compatibility error message?
                If (msg IsNot Nothing) Then
                    Me.m_uic.Core.Messages.SendMessage(msg)
                End If

                Me.UpdateScalingPanel()

            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try

            Me.m_bInUpdate = False
            Me.UpdateControls()

        End Sub

#End Region ' Scaling

#Region " OK "

        Private Sub OnOK(sender As Object, e As System.EventArgs) _
            Handles m_btnOK.Click

            Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection()
            If (conn IsNot Nothing) Then
                Try
                    If (Me.m_rbAbsolute.Checked) Then
                        conn.ScaleType = cSpatialScalarDataAdapterBase.eScaleType.Absolute
                        Me.m_fpScale.Enabled = False
                    Else
                        conn.ScaleType = cSpatialScalarDataAdapterBase.eScaleType.Relative
                        conn.Scale = CSng(Me.m_fpScale.Value)
                        Me.m_fpScale.Enabled = True
                    End If
                    Me.LayerChanged()
                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                End Try
            End If

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

#End Region ' Other

#End Region ' Control events

#Region " Internals "

        Private Sub RefreshDatasetList()



        End Sub

        Private Sub UpdateControls()

            Dim iLayer As Integer = m_iLayer
            Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection()
            Dim bHasConnectionSelected As Boolean = (conn IsNot Nothing)
            Dim ds As ISpatialDataSet = Nothing
            Dim cv As ISpatialDataConverter = Nothing
            Dim bCanConfigDS As Boolean = False
            Dim bCanConfigCV As Boolean = False
            Dim bIsConfigured As Boolean = False
            Dim bNeedsConverter As Boolean = False
            Dim bNeedsScaling As Boolean = False
            Dim bCanAddDS As Boolean = False
            Dim bCanRemoveDS As Boolean = False
            Dim comp As cDatasetCompatilibity.eCompatibilityTypes = cDatasetCompatilibity.eCompatibilityTypes.NotSet
            Dim bHasIncompatible As Boolean = False

            If (Me.m_adt IsNot Nothing) Then
                For Each test As ISpatialDataSet In Me.m_manSets.Datasets
                    If (test.VarName <> eVarNameFlags.NotSet) And (test.VarName <> Me.m_adt.VarName) Then
                        bHasIncompatible = True
                    End If
                Next
            End If
            Me.m_tsbnShowIncompatibleConnections.Visible = bHasIncompatible

            If (bHasConnectionSelected) Then
                ds = conn.Dataset
                cv = conn.Converter
            End If

            If (ds IsNot Nothing) Then

                bCanConfigDS = (TypeOf ds Is IConfigurable)
                bNeedsConverter = Not String.IsNullOrWhiteSpace(ds.ConversionFormat)
                bNeedsScaling = Me.m_bIsScaling
                bCanRemoveDS = True

                If (ds.IsConfigured) And (Not bNeedsConverter) Then
                    bIsConfigured = True
                Else
                    bIsConfigured = False
                    If (cv IsNot Nothing) Then
                        If cv.IsCompatible(ds) Then
                            bIsConfigured = cv.IsConfigured
                        End If
                    End If
                End If

                If (bIsConfigured And bNeedsScaling) Then
                    Dim worker As cDatasetCompatilibity = Me.m_manSets.Compatibility(ds)
                    comp = worker.Compatibility
                End If
            End If

            bCanAddDS = (Me.m_gridConnections.RowsCount < cSpatialDataStructures.cMAX_CONN) And (Me.SourceDataset IsNot Nothing)

            If (cv IsNot Nothing) Then
                bCanConfigCV = bHasConnectionSelected And (TypeOf cv Is IConfigurable)
            End If

            Me.m_btnConfigDS.Enabled = bCanConfigDS

            Me.m_plDataset.Visible = bHasConnectionSelected

            Me.m_plConversion.Enabled = bHasConnectionSelected And bNeedsConverter
            Me.m_plConversion.Visible = bNeedsConverter
            Me.m_btnConfigCV.Enabled = bCanConfigCV
            Me.m_cmbConverter.Enabled = (Me.m_cmbConverter.Items.Count > 1)

            Me.m_plScalarAdapter.Enabled = bHasConnectionSelected And bNeedsScaling
            Me.m_plScalarAdapter.Visible = bNeedsScaling

            If bNeedsScaling And bIsConfigured Then
                ' Allow calc of scaling even if spatial compatibility has not been assessed yet, for indexing may have been turned off
                Me.m_btnCalculate.Enabled = True
            Else
                Me.m_btnCalculate.Enabled = False
            End If

            Me.m_btnAdd.Enabled = bCanAddDS
            Me.m_btnRemove.Enabled = bCanRemoveDS

        End Sub

        Private Sub UpdateDatasetPanel()

            Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection()

            If (conn IsNot Nothing) Then

                Try
                    Me.m_lblDatasetInfo.Text = conn.Dataset.DisplayName

                    Dim comp As cDatasetCompatilibity = Me.m_manSets.Compatibility(conn.Dataset)
                    Dim fmt As New cSpatialDatasetCompatibilityFormatter()
                    Me.m_lblCompatibility.Text = fmt.Summary(comp)

                    Select Case comp.Compatibility
                        Case cDatasetCompatilibity.eCompatibilityTypes.TemporalNotIndexed, _
                             cDatasetCompatilibity.eCompatibilityTypes.NotSet
                            Me.m_pbCompat.Image = SharedResources.Question
                        Case cDatasetCompatilibity.eCompatibilityTypes.Errors
                            Me.m_pbCompat.Image = SharedResources.Critical
                        Case cDatasetCompatilibity.eCompatibilityTypes.PartialSpatial
                            Me.m_pbCompat.Image = SharedResources.Warning
                        Case Else
                            Me.m_pbCompat.Image = SharedResources.OK
                    End Select

                Catch ex As Exception

                End Try

            Else
                Me.m_lblDatasetInfo.Text = ""
                Me.m_lblCompatibility.Text = ""
                Me.m_pbCompat.Image = Nothing
            End If

        End Sub

        ''' <summary>
        ''' Fill UI with converters compatible with the selected dataset.
        ''' </summary>
        Private Sub UpdateConversionPanel()

            Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection()

            Me.m_cmbConverter.Items.Clear()

            If (conn IsNot Nothing) Then
                Dim ds As ISpatialDataSet = conn.Dataset
                Dim cv As ISpatialDataConverter = conn.Converter
                For Each cvTest As ISpatialDataConverter In Me.m_manConn.ConverterTemplates(ds)
                    If (cv Is Nothing) Then conn.Converter = cvTest
                    Me.m_cmbConverter.Items.Add(cvTest)
                Next
                Me.SelectConverter(conn.Converter)
            End If

        End Sub

        ''' <summary>
        ''' Get the dataset selected that can be added to the current layer
        ''' </summary>
        Private ReadOnly Property SourceDataset As ISpatialDataSet
            Get
                Return Me.m_lbSourceDatasets.SelectedDataset
            End Get
        End Property

        Private Function ConfigConverter(cv As ISpatialDataConverter) As Boolean

            ' ToDo: globalize this

            If (cv Is Nothing) Then Return False
            If (Not TypeOf cv Is IConfigurable) Then Return True

            If (TypeOf cv Is IPlugin) Then
                DirectCast(cv, IPlugin).Initialize(Me.m_uic.Core)
            End If

            Dim cvConf As IConfigurable = DirectCast(cv, IConfigurable)
            Dim ctrl As Control = cvConf.GetConfigUI()

            If (ctrl Is Nothing) Then Return cvConf.IsConfigured

            Dim dlg As New dlgConfig(Me.UIContext)

            If (dlg.ShowDialog(Me.FindForm, "Configure conversion", ctrl) = System.Windows.Forms.DialogResult.OK) Then
                Me.m_manConn.Update(cv)
            End If
            Return (cvConf.IsConfigured)

        End Function

        Private Property SelectedDataset As ISpatialDataSet
            Get
                Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection
                If (conn Is Nothing) Then Return Nothing
                Return conn.Dataset
            End Get
            Set(dataset As ISpatialDataSet)

                ' Apply
                Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection
                If (conn IsNot Nothing) Then
                    conn.Dataset = dataset
                    Me.m_manSets.IndexDataset = dataset
                End If

                Me.UpdateDatasetPanel()
                Me.UpdateConversionPanel()
                Me.UpdateScalingPanel()
                Me.UpdateControls()

            End Set
        End Property

        Private Sub SelectConverter(converter As ISpatialDataConverter)
            ' Update selection
            Dim iIndex As Integer = 0
            If (converter IsNot Nothing) Then
                For Each item As Object In Me.m_cmbConverter.Items
                    If converter.GetType().Equals(item.GetType()) Then
                        Me.m_cmbConverter.SelectedItem = item
                        Return
                    End If
                Next
            End If
            Me.m_cmbConverter.SelectedItem = Nothing
        End Sub

        Private Property SelectedConverter As ISpatialDataConverter
            Get
                Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection
                If (conn Is Nothing) Then Return Nothing
                Return conn.Converter
            End Get
            Set(converter As ISpatialDataConverter)

                Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection
                If (conn IsNot Nothing) Then
                    ' Apply
                    If (Not ReferenceEquals(conn.Converter, converter)) Then
                        conn.Converter = converter
                        Me.LayerChanged()
                    End If
                End If

            End Set
        End Property

        Private Function ConfigDataset(ds As ISpatialDataSet) As Boolean

            Dim cmd As cEditSpatialDatasetCommand = CType(Me.UIContext.CommandHandler.GetCommand(cEditSpatialDatasetCommand.COMMAND_NAME), cEditSpatialDatasetCommand)
            cmd.Invoke(ds)
            Return (ds.IsConfigured)

        End Function

        Private Sub LayerChanged()
            Me.m_bIsChanged = True
        End Sub

#Region " Scalar data adapter "

        Private Sub UpdateScalingPanel()

            If (Not Me.m_bIsScaling) Then Return

            Dim conn As cSpatialDataConnection = Me.m_gridConnections.SelectedConnection
            If (conn Is Nothing) Then Return

            Dim bInUpdate As Boolean = Me.m_bInUpdate
            Me.m_bInUpdate = True

            Select Case conn.ScaleType
                Case cSpatialScalarDataAdapterBase.eScaleType.Absolute
                    Me.m_rbAbsolute.Checked = True
                Case cSpatialScalarDataAdapterBase.eScaleType.Relative
                    Me.m_rbRelative.Checked = True
            End Select
            Me.m_fpScale.Value = conn.Scale

            Me.m_bInUpdate = bInUpdate

        End Sub

#End Region ' Scalar data adapter

#End Region ' Internals

#Region " Disabled bits that may come in handy again "

#If 0 Then

        Private Sub OnSaveStats(sender As System.Object, e As System.EventArgs)

            ' This is very deliberately hidden functionality!
            Dim ds As ISpatialDataSet = Me.SelectedDataset
            Dim cv As ISpatialDataConverter = Me.SelectedConverter
            Dim sw As StreamWriter = Nothing
            Dim core As cCore = Me.m_uic.Core
            Dim bm As cEcospaceBasemap = core.EcospaceBasemap
            Dim strFile As String = ""

            If (ds Is Nothing) Or (cv Is Nothing) Then Return

            strFile = Path.Combine(Me.m_uic.Core.OutputPath(), cFileUtils.ToValidFileName(ds.DisplayName & "_stats.csv", False))
            If Not cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFile), True) Then
                Return
            End If

            Try
                sw = New StreamWriter(strFile)
            Catch ex As Exception
                Return
            End Try

            sw.WriteLine("timestep,date,min,max,mean,stdev")
            Dim iStart As Integer = Math.Max(0, core.AbsoluteTimeToEcospaceTimestep(ds.TimeStart))
            Dim iEnd As Integer = Math.Min(core.AbsoluteTimeToEcospaceTimestep(ds.TimeEnd), core.nEcospaceTimeSteps)
            Dim t As Date = Nothing
            Dim rs As ISpatialRaster = Nothing

            Try

                For i As Integer = iStart To iEnd
                    t = core.EcospaceTimestepToAbsoluteTime(i)
                    If ds.HasDataAtT(t) Then
                        Console.WriteLine("Getting stats for time step " & i & " [" & iStart & ", " & iEnd & "]")
                        If (ds.LockDataAtT(t, bm.CellSize, bm.PosTopLeft, bm.PosBottomRight)) Then
                            rs = ds.GetRaster(cv, "")
                            sw.WriteLine("{0},{1},{2},{3},{4},{5}", _
                                              i, cStringUtils.ToCSVField(t.ToShortDateString()), _
                                              cStringUtils.FormatNumber(rs.Min), _
                                              cStringUtils.FormatNumber(rs.Max), _
                                              cStringUtils.FormatNumber(rs.Mean), _
                                              cStringUtils.FormatNumber(rs.StandardDeviation))
                            ds.Unlock()
                        End If
                    End If
                Next
            Catch ex As Exception
                sw.WriteLine(cStringUtils.ToCSVField(ex.Message))
            End Try
            sw.Flush()
            sw.Close()

            Try
                Dim msg As New cMessage("External data statistics saved to " & strFile, eMessageType.DataExport, EwEUtils.Core.eCoreComponentType.External, eMessageImportance.Information)
                msg.Hyperlink = Path.GetDirectoryName(strFile)
                core.Messages.SendMessage(msg)
            Catch ex As Exception

            End Try

            ' Update
            Me.UpdateControls()

        End Sub
#End If ' 0

#End Region ' Disabled bits that may come in handy again

    End Class

End Namespace ' Ecospace.Controls
