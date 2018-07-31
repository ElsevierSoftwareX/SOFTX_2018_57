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
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Definitions
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="cShapeGUIHandler">cShapeGUIHandler implementation</see> for 
    ''' handling <see cref="cTimeSeries">Time Series shapes</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(True)> _
    Public Class cTimeSeriesShapeGUIHandler
        Inherits cShapeGUIHandler

        ''' <summary>The Time Series to distribute.</summary>
        Private m_lShapes As New List(Of cShapeData)
        ''' <summary>Time series type filter</summary>
        Private m_iTSTypeFilter As Integer = 0
        ''' <summary>List of available time series types.</summary>
        Private m_types() As eTimeSeriesType = Nothing
        ''' <summary>Shape changed core message handler.</summary>
        Private m_mhShapes As cMessageHandler = Nothing

#Region " Time series type filter "

        Private Class cTypeAdmin

            Private m_strName As String
            Private m_type As eTimeSeriesType

            Public Sub New(ByVal t As eTimeSeriesType)
                Me.m_type = t
                Dim fmt As New cTimeSeriesTypeFormatter
                Me.m_strName = fmt.GetDescriptor(t, eDescriptorTypes.Abbreviation)
            End Sub

            Public Property NumShapes() As Integer

            Public ReadOnly Property TimeSeriesType() As eTimeSeriesType
                Get
                    Return Me.m_type
                End Get
            End Property

            Public ReadOnly Property Name() As String
                Get
                    Return Me.m_strName
                End Get
            End Property

        End Class

        Private Class cTypeAdminComparer
            Implements IComparer(Of cTypeAdmin)

            Public Function Compare(ByVal x As cTypeAdmin, ByVal y As cTypeAdmin) As Integer _
                Implements System.Collections.Generic.IComparer(Of cTypeAdmin).Compare
                Return String.Compare(x.Name, y.Name)
            End Function

        End Class

#End Region ' Filter

        Public Sub New(uic As cUIContext)
            MyBase.new(uic)
        End Sub

#Region " Baseclass overrides "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this handler.
        ''' </summary>
        ''' <param name="stb"><see cref="ucShapeToolbox">Shape toolbox control </see> to handle, if any.</param>
        ''' <param name="stbtb"><see cref="ucShapeToolboxToolbar">Shape toolbox toolbar control </see> to handle, if any.</param>
        ''' <param name="sp"><see cref="ucSketchPad">Shape sketch pad control </see> to handle, if any.</param>
        ''' <param name="sptb"><see cref="ucSketchPadToolbar">Shape sketch pad toolbar control </see> to handle, if any.</param>
        ''' -------------------------------------------------------------------
        Public Overloads Sub Attach(ByVal stb As ucShapeToolbox, _
                                    ByVal stbtb As ucShapeToolboxToolbar, _
                                    ByVal sp As ucSketchPad, _
                                    ByVal sptb As ucSketchPadToolbar)

            MyBase.Attach(stb, stbtb, sp, sptb)

            If Me.SketchPad IsNot Nothing Then
                ' Cannot draw onto time series shapes
                Me.SketchPad.Enabled = False
            End If

            If Me.ShapeToolBox IsNot Nothing Then
                ' Add check boxes to the toolbox
                Me.ShapeToolBox.AllowCheckboxes = True
            End If

            Me.UpdateShapeList(New cShapeData() {sp.Shape}, eAutoSelectMode.SelectFirstShape)

            Me.m_mhShapes = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.ShapesManager, eMessageType.DataModified, Me.UIContext.SyncObject)
            Me.UIContext.Core.Messages.AddMessageHandler(Me.m_mhShapes)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.Detach"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Detach()
            If (Me.m_mhShapes IsNot Nothing) Then
                Me.UIContext.Core.Messages.RemoveMessageHandler(Me.m_mhShapes)
                Me.m_mhShapes = Nothing
            End If
            MyBase.Detach()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public interface to ask whether a given command is supported by this handler.
        ''' Overridden to weed out non-Time Series commands.
        ''' </summary>
        ''' <param name="cmd">The command to test.</param>
        ''' <returns>True if command is supported.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function SupportCommand(ByVal cmd As eShapeCommandTypes) As Boolean

            Select Case cmd
                Case eShapeCommandTypes.Add
                    Return True
                Case eShapeCommandTypes.Weight
                    Return True
                Case eShapeCommandTypes.Duplicate
                    Return False
                Case eShapeCommandTypes.Import
                    Return True
                Case eShapeCommandTypes.Export
                    Return True
                Case eShapeCommandTypes.Load
                    Return True
                Case eShapeCommandTypes.Modify
                    Return True
                Case eShapeCommandTypes.Remove
                    Return True
                Case eShapeCommandTypes.Seasonal
                    Return False
                Case eShapeCommandTypes.SetWeight
                    Return True
                Case eShapeCommandTypes.SaveAsImage
                    Return True
                Case eShapeCommandTypes.ResetAll
                    Return False
                Case eShapeCommandTypes.FilterList
                    Return True
                Case eShapeCommandTypes.FilterName
                    Return True
                Case Else
                    ' Debug.Assert(False, cStringUtils.Localize("Command {0} not supported", cmd))
            End Select
            Return False

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public interface to query the enables state of a given command by this handler.
        ''' Overridden to enable commands Time Series-style, kachingg!!
        ''' </summary>
        ''' <param name="cmd">The <see cref="eShapeCommandTypes">command</see> to test.</param>
        ''' <returns>True if enabled.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function EnableCommand(ByVal cmd As cShapeGUIHandler.eShapeCommandTypes) As Boolean

            Dim bHasSelection As Boolean = (Me.SelectedShapes IsNot Nothing)
            Dim bHasSingleSelection As Boolean = (Me.SelectedShape IsNot Nothing)

            Select Case cmd

                Case cShapeGUIHandler.eShapeCommandTypes.Import, _
                     cShapeGUIHandler.eShapeCommandTypes.Load, _
                     cShapeGUIHandler.eShapeCommandTypes.FilterList, _
                     eShapeCommandTypes.FilterName
                    Return True

                Case cShapeGUIHandler.eShapeCommandTypes.SetWeight
                    ' #1079: only enable for reference series
                    If bHasSingleSelection Then
                        Return DirectCast(Me.SelectedShape, cTimeSeries).IsReference
                    End If
                    Return False

                Case cShapeGUIHandler.eShapeCommandTypes.Add, _
                     cShapeGUIHandler.eShapeCommandTypes.Weight, _
                     cShapeGUIHandler.eShapeCommandTypes.Export
                    Return Me.Core.HasTimeSeries

                Case cShapeGUIHandler.eShapeCommandTypes.Duplicate, _
                     cShapeGUIHandler.eShapeCommandTypes.Remove
                    Return bHasSelection

                Case cShapeGUIHandler.eShapeCommandTypes.Modify, _
                     cShapeGUIHandler.eShapeCommandTypes.SaveAsImage
                    Return bHasSingleSelection

            End Select

            Return False

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public interface to execute a given command by this handler. 
        ''' Overridden to implement Time Series commands.
        ''' </summary>
        ''' <param name="cmd">The <see cref="eShapeCommandTypes">command</see> to test.</param>
        ''' <param name="ashapes">The <see cref="EwECore.cShapeData">shapes</see> to apply the command to.</param>
        ''' <param name="data">Optional data to accompany the command.</param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub ExecuteCommand(ByVal cmd As eShapeCommandTypes, _
             Optional ByVal ashapes As cShapeData() = Nothing, Optional ByVal data As Object = Nothing)

            If (ashapes Is Nothing) Then ashapes = Me.SelectedShapes

            Select Case cmd
                Case eShapeCommandTypes.Add
                    Me.AddTimeSeries()

                Case eShapeCommandTypes.Duplicate
                    Me.DuplicateTimeSeries(ashapes)

                Case eShapeCommandTypes.Import
                    Me.ImportTimeSeries()

                Case eShapeCommandTypes.Export
                    Me.ExportTimeSeries()

                Case eShapeCommandTypes.Load
                    Me.LoadDatasets()

                Case eShapeCommandTypes.Remove
                    Me.RemoveTimeSeries(ashapes)

                Case eShapeCommandTypes.Modify
                    Me.ModifyTimeSeries(ashapes(0))

                Case eShapeCommandTypes.SetWeight
                    Me.SetWeight(ashapes(0), CSng(data))

                Case eShapeCommandTypes.SaveAsImage
                    Me.SaveAsImage(ashapes(0), Me.SketchPad)

                Case eShapeCommandTypes.Weight
                    Me.WeightTimeSeries()

            End Select
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overridden this to make controls respond to any kind of change in 
        ''' time series data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Refresh()
            If Me.m_bInUpdate Then Return
            Me.UpdateShapeList(Me.SelectedShapes, eAutoSelectMode.SelectCurrentShape)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Respond to local shape change.
        ''' </summary>
        ''' <param name="shape">The newly selected shape.</param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeChanged(ByVal shape As EwECore.cShapeData)
            If (Me.ShapeToolBox Is Nothing) Then Return
            If Me.m_bInUpdate Then Return

            Me.m_bInUpdate = True
            Me.ShapeToolBox.UpdateThumbnail(shape)
            Me.m_bInUpdate = False
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to kick the programmer; Time Series cannot be drawn by hand.
        ''' </summary>
        ''' <param name="shape"></param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeFinalized(ByVal shape As EwECore.cShapeData, ByVal sketchpad As ucSketchPad)
            Debug.Assert(False)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Cascade a newly selected shape to the managed controls.
        ''' </summary>
        ''' <param name="ashapes">The newly selected shapes.</param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeSelected(ByVal ashapes As EwECore.cShapeData())
            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True
            If Me.SketchPad IsNot Nothing Then
                Me.SelectedShapes = ashapes
            End If
            Me.m_bInUpdate = False
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the colour for Time Series shapes.
        ''' </summary>
        ''' <returns>The color for Time Series shapes.</returns>
        ''' -----------------------------------------------------------------------
        Public Overrides Function Color() As System.Drawing.Color
            Debug.Assert(Me.UIContext IsNot Nothing)
            Return Me.UIContext.StyleGuide.ShapeColor(eDataTypes.GroupTimeSeries)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the default sketch mode for Time Series shapes.
        ''' </summary>
        ''' <returns>The default sketch mode for Time Series shapes.</returns>
        ''' -----------------------------------------------------------------------
        Public Overrides Function SketchDrawMode(shape As cShapeData) As eSketchDrawModeTypes
            If (shape Is Nothing) Then Return eSketchDrawModeTypes.TimeSeriesDriver
            Debug.Assert(TypeOf shape Is cTimeSeries)
            Dim ts As cTimeSeries = DirectCast(shape, cTimeSeries)
            If (ts.IsDriver) Then
                Return eSketchDrawModeTypes.TimeSeriesDriver
            End If
            If (ts.IsAbsolute) Then
                Return eSketchDrawModeTypes.TimeSeriesRefAbs
            End If
            Return eSketchDrawModeTypes.TimeSeriesRefRel
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the lower limit for the sketch pad Y-axis when displaying 
        ''' Time Series data.
        ''' </summary>
        ''' <returns>The lower limit for the sketch pad Y-axis when displaying 
        ''' Time Series data.</returns>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function MinYScale() As Single
            Return 0.0!
        End Function

        Public Overrides ReadOnly Property Filters() As String()
            Get
                ' Got a bit of work to do here!

                ' 1) Iterate over all TS
                ' 2) Count no of shapes per time series type
                Dim dtAdmin As New Dictionary(Of eTimeSeriesType, cTypeAdmin)
                Dim ts As cTimeSeries = Nothing

                For i As Integer = 1 To Me.Core.nTimeSeries
                    ' Get TS
                    ts = Me.Core.EcosimTimeSeries(i)
                    If Not dtAdmin.ContainsKey(ts.TimeSeriesType) Then
                        dtAdmin(ts.TimeSeriesType) = New cTypeAdmin(ts.TimeSeriesType)
                    End If
                    dtAdmin(ts.TimeSeriesType).NumShapes += 1
                Next

                ' 3) Create a list of filters for all types with one or more TS
                Dim lAdmin As New List(Of cTypeAdmin)
                For Each ad As cTypeAdmin In dtAdmin.Values
                    If ad.NumShapes > 0 Then
                        lAdmin.Add(ad)
                    End If
                Next

                ' 4) Sort filters alphabetically
                lAdmin.Sort(New cTypeAdminComparer)

                ' 5) Maintain 2 lists: one of filter names and one of filter types
                Dim lTypes As New List(Of eTimeSeriesType)
                Dim lstrFilters As New List(Of String)

                lstrFilters.Add(My.Resources.GENERIC_VALUE_ALL)
                lTypes.Add(eTimeSeriesType.NotSet)

                For i As Integer = 0 To lAdmin.Count - 1
                    lstrFilters.Add(cStringUtils.Localize(My.Resources.GENERIC_LABEL_DETAILED, lAdmin(i).Name, lAdmin(i).NumShapes))
                    lTypes.Add(lAdmin(i).TimeSeriesType)
                Next

                dtAdmin.Clear()
                lAdmin.Clear()

                Me.m_types = lTypes.ToArray
                Return lstrFilters.ToArray
            End Get
        End Property

        Public Overrides Property FilterIndex() As Integer
            Get
                Return Me.m_iTSTypeFilter
            End Get
            Set(ByVal value As Integer)
                Dim iFilterNew As Integer = Math.Max(0, Math.Min(value, Me.m_types.Length - 1))
                If (iFilterNew <> Me.m_iTSTypeFilter) Then
                    Me.m_iTSTypeFilter = iFilterNew
                    Me.Refresh()
                End If
            End Set
        End Property

#End Region ' Baseclass overrides

#Region " Internal implementation "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Add">Add</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub AddTimeSeries()
            Dim frm As frmShapeValue = New frmShapeValue(Me.UIContext, Me)
            If (frm.ShowDialog() = DialogResult.OK) Then
                ' Ecosim will reload, which means a reload of datasets and time series
                ' As a result, this control will be told to update
                Me.Core.LoadTimeSeries(Me.Core.ActiveTimeSeriesDatasetIndex)
            End If
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Weight">Weight</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub WeightTimeSeries()
            Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("WeightTimeSeries")

            If cmd IsNot Nothing Then
                cmd.Invoke()
            End If
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Load">Load</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub LoadDatasets()
            Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("LoadTimeSeries")

            If cmd IsNot Nothing Then
                cmd.Invoke()
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Duplicate">Duplicate</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub DuplicateTimeSeries(ByVal ashapes As cShapeData())

            ' Sanity check
            Debug.Assert(ashapes IsNot Nothing, "Need valid TS")

            Dim strNewTSName As String = ""
            Dim lstrTSNames As New List(Of String)
            Dim iNextTSNumber As Integer = 0
            Dim ts As cTimeSeries = Nothing
            Dim asValues() As Single
            Dim intDBID As Integer = -1
            Dim bSucces As Boolean = True

            ' Collect all current shape names
            For Each s As cShapeData In Me.m_lShapes
                lstrTSNames.Add(s.Name)
            Next

            ' Concoct a new name based on the numbered strings that are found
            iNextTSNumber = cStringUtils.GetNextNumber(lstrTSNames.ToArray(), My.Resources.ECOSIM_DEFAULT_NEWTIMESERIES)
            strNewTSName = cStringUtils.Localize(My.Resources.ECOSIM_DEFAULT_NEWTIMESERIES, iNextTSNumber)

            ' Generate TS data
            For Each shape As cShapeData In ashapes
                ts = Me.Core.EcosimTimeSeries(shape.Index)
                ReDim asValues(ts.ShapeData.Length - 2)
                For i As Integer = 1 To ts.ShapeData.Length - 1
                    asValues(i - 1) = ts.DatVal(i)
                Next
                bSucces = bSucces And (Me.Core.AddTimeSeries(strNewTSName, ts.DatPool, ts.DatPoolSec, ts.TimeSeriesType, ts.WtType, asValues, intDBID))
            Next

            If bSucces Then
                ' Update shape to select
                Me.UpdateShapeList(Nothing, eAutoSelectMode.SelectLastShape)
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Import">Import</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub ImportTimeSeries()
            ' Launch via command!
            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand("ImportTimeSeries")
            If cmd IsNot Nothing Then cmd.Invoke()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Export">ExportTimeSeries</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub ExportTimeSeries()
            ' Launch via command!
            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmd As cCommand = cmdh.GetCommand("ExportTimeSeries")
            If cmd IsNot Nothing Then cmd.Invoke()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Remove">Remove</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub RemoveTimeSeries(ByVal ashapes As cShapeData())

            Dim fms As cFeedbackMessage = Nothing
            Dim strMessage As String = ""
            Dim bSucces As Boolean = True

            ' Sanity check
            Debug.Assert(ashapes IsNot Nothing, "Need valid TS")

            ' Prompt for confirmation
            If ashapes.Length = 1 Then
                strMessage = cStringUtils.Localize(My.Resources.PROMPT_TIMESERIES_DELETE, ashapes(0).Name)
            Else
                strMessage = cStringUtils.Localize(My.Resources.PROMPT_TIMESERIES_DELETE_MULTIPLE, ashapes.Length)
            End If

            fms = New cFeedbackMessage(strMessage, _
                                       eCoreComponentType.ShapesManager, _
                                       eMessageType.Any, _
                                       eMessageImportance.Warning, _
                                       eMessageReplyStyle.YES_NO, _
                                       eDataTypes.TimeSeriesDataset, _
                                       eMessageReply.OK)
            Me.Core.Messages.SendMessage(fms, True)
            If (fms.Reply = eMessageReply.NO) Then Return

            ' Delete
            Me.Core.SetBatchLock(cCore.eBatchLockType.Restructure)
            Try
                For Each shape As cShapeData In ashapes
                    Debug.Assert(TypeOf shape Is cTimeSeries, "Need valid TS")
                    bSucces = bSucces And Me.Core.RemoveTimeSeries(shape.DBID)
                Next
            Catch ex As Exception
                ' Whoah!
            End Try
            Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.TimeSeries, bSucces)

            ' Refresh
            Me.UpdateShapeList()

        End Sub

        Private Sub ModifyTimeSeries(ByVal shape As cShapeData)

            ' Sanity check
            Debug.Assert(shape IsNot Nothing, "Need valid TS")
            Debug.Assert(TypeOf shape Is cTimeSeries, "Need valid TS")

            Dim dlg As New frmShapeValue(Me.UIContext, shape)
            Try
                dlg.ShowDialog()
            Catch ex As Exception
                ' Whoa!
            End Try

        End Sub

        Private Sub SetWeight(ByVal shape As cShapeData, ByVal sWeight As Single)

            ' Sanity check
            Debug.Assert(shape IsNot Nothing, "Need valid TS")
            Debug.Assert(TypeOf shape Is cTimeSeries, "Need valid TS")

            Dim ts As cTimeSeries = DirectCast(shape, cTimeSeries)
            ts.WtType = sWeight
            shape.Update()

        End Sub

#End Region ' Internal implementation 

#Region " Helper methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper enum; states how to reload data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Enum eAutoSelectMode As Byte
            None = 0
            SelectFirstShape
            SelectLastShape
            SelectCurrentShape
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; updates the list of time series to manage.
        ''' </summary>
        ''' <param name="ashapeSelect">Shapes to select.</param>
        ''' <param name="selectMode">If shape cannot be selected, or no shape 
        ''' has been provided, this mode indicates how the new selection should 
        ''' be made.</param>
        ''' -------------------------------------------------------------------
        Private Sub UpdateShapeList(Optional ByVal ashapeSelect As cShapeData() = Nothing, _
                Optional ByVal selectMode As eAutoSelectMode = eAutoSelectMode.SelectCurrentShape)

            Dim ts As cTimeSeries = Nothing
            Dim bIncludeShape As Boolean = False
            Dim shapeSelectCurr As cShapeData() = Me.SelectedShapes

            Me.m_lShapes.Clear()

            ' For all TS
            For i As Integer = 1 To Me.Core.nTimeSeries
                ' Get TS
                ts = Me.Core.EcosimTimeSeries(i)
                bIncludeShape = True

                ' Need to filter?
                If Me.m_iTSTypeFilter > 0 Then
                    ' #Yes: check if type matches
                    Try
                        bIncludeShape = (ts.TimeSeriesType = Me.m_types(Me.m_iTSTypeFilter))
                    Catch ex As Exception
                        ' Aargh
                    End Try
                End If

                If (bIncludeShape And MyBase.IncludeShape(ts)) Then
                    Me.m_lShapes.Add(ts)
                End If
            Next

            ' Select a shape
            If ashapeSelect Is Nothing Then
                If Me.m_lShapes.Count > 0 Then
                    Select Case selectMode
                        Case eAutoSelectMode.None
                            ' Haha
                        Case eAutoSelectMode.SelectCurrentShape
                            ashapeSelect = shapeSelectCurr
                        Case eAutoSelectMode.SelectFirstShape
                            ashapeSelect = New cShapeData() {Me.m_lShapes(0)}
                        Case eAutoSelectMode.SelectLastShape
                            ashapeSelect = New cShapeData() {Me.m_lShapes(Me.m_lShapes.Count - 1)}
                    End Select
                End If
            Else
                ' JS 11Apr14: clear selected shape if no shapes present
                If (Me.m_lShapes.Count = 0) Then
                    ashapeSelect = Nothing
                End If
            End If

            If (Me.ShapeToolBox IsNot Nothing) Then
                Me.ShapeToolBox.SetShapes(Me.m_lShapes, ashapeSelect)
                ashapeSelect = Me.ShapeToolBox.Selection
            End If

            Me.SelectedShapes = ashapeSelect

        End Sub

#End Region ' Helper methods

        Protected Overrides Function ShapeManager() As EwECore.cBaseShapeManager
            Return Nothing
        End Function

        Protected Overrides Function Datatypes() As EwEUtils.Core.eDataTypes()
            Return New eDataTypes() {eDataTypes.GroupTimeSeries, eDataTypes.FleetTimeSeries}
        End Function

        Public Overrides Function XAxisMaxValue() As Integer
            Dim iDS As Integer = Me.Core.ActiveTimeSeriesDatasetIndex
            If (iDS <= 0) Then Return 0
            Dim ds As cTimeSeriesDataset = Me.Core.TimeSeriesDataset(iDS)
            Return ds.NumPoints
        End Function

        Public Overrides Function IsForcing() As Boolean
            Return False
        End Function

        Public Overrides Function IsMediation() As Boolean
            Return False
        End Function

        Public Overrides Function IsTimeSeries() As Boolean
            Return True
        End Function

    End Class

End Namespace
