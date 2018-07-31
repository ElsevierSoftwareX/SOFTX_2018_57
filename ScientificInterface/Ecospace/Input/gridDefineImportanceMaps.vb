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
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2

#End Region

Namespace Ecospace

    <CLSCompliant(False)> _
    Public Class gridDefineImportanceMaps
        Inherits EwEGrid

        ''' <summary>A number representing the row that contains the first Layer</summary>
        Private Const iFIRSTDATAROW As Integer = 1

        ''' <summary>List of active Layers.</summary>
        Private m_alLayers As New List(Of cLayerInfo)
        ''' <summary>List of removed Layers.</summary>
        Private m_alLayersRemoved As New List(Of cLayerInfo)
        ''' <summary>Update lock, used to distinguish between code updates and
        ''' user updates of grid cells. When grid cells are updated from within
        ''' the code, an update lock should be active to prevent edit/update recursion.</summary>
        Private m_iUpdateLock As Integer = 0

        ''' <summary>Enumerated type defining the columns in this grid.</summary>
        Private Enum eColumnTypes As Integer
            LayerIndex = 0
            LayerName
            LayerWeight
            LayerDescription
            LayerStatus
        End Enum

#Region " Helper classes "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Administrative unit representing a <see cref="cEcospaceBasemap">Importance layer</see>
        ''' in the EwE model.
        ''' </summary>
        ''' <remarks>
        ''' This class can represent existing and new Layers. If this class has its
        ''' <see cref="cLayerInfo.Layer">Layer</see> parameter set, a real live
        ''' Layer is represented. If this parameter is not set, a new Layer is
        ''' represented.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Class cLayerInfo

            ''' <summary><see cref="cEcospaceBasemap">cEcospaceBasemap</see> associated with this Layer, if any.</summary>
            Private m_Layer As cEcospaceLayerImportance = Nothing
            ''' <summary>Name for this Layer.</summary>
            Private m_strName As String = ""
            ''' <summary>Description for this Layer.</summary>
            Private m_strDescription As String = ""
            ''' <summary>Weight for this Layer.</summary>
            Private m_sWeight As Single = 0.0
            ''' <summary>Flag stating whether a user action is confirmed</summary>
            Private m_bConfirmed As Boolean = True
            ''' <summary>The status of a Layer in the interface.</summary>
            Private m_status As eItemStatusTypes = eItemStatusTypes.Original

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Constructor, initializes a new instanze of this class.
            ''' </summary>
            ''' <param name="Layer">The <see cref="cEcospaceBasemap">cEcospaceBasemap</see> to
            ''' initialize this instance from. If set, this instance represents a
            ''' Layer currently active in the EwE model.</param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal Layer As cEcospaceLayerImportance)
                Debug.Assert(Layer IsNot Nothing)
                Me.m_Layer = Layer
                Me.m_strName = Layer.Name
                Me.m_strDescription = Layer.Description
                Me.m_sWeight = Layer.Weight
                Me.m_status = eItemStatusTypes.Original
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Constructor, initializes a new instanze of this class.
            ''' </summary>
            ''' <param name="strName">Name to assign to this administrative unit.</param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal strName As String, ByVal strDescription As String, ByVal sWeight As Single)
                Me.m_Layer = Nothing
                Me.m_strName = strName
                Me.m_strDescription = strDescription
                Me.m_sWeight = sWeight
                Me.m_status = eItemStatusTypes.Added
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the name of this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Name() As String
                Get
                    Return Me.m_strName
                End Get
                Set(ByVal value As String)
                    Me.m_strName = value
                End Set
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the description of this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Description() As String
                Get
                    Return Me.m_strDescription
                End Get
                Set(ByVal value As String)
                    Me.m_strDescription = value
                End Set
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the weight of this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Weight() As Single
                Get
                    Return Me.m_sWeight
                End Get
                Set(ByVal value As Single)
                    Me.m_sWeight = value
                End Set
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="cEcospaceBasemap">EwE Layer</see> associated
            ''' with this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property Layer() As cEcospaceLayerImportance
                Get
                    Return Me.m_Layer
                End Get
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="eItemStatusTypes">item status</see>
            ''' for the layer object.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property Status() As eItemStatusTypes
                Get
                    Return Me.m_status
                End Get
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set whether the user has confirmed an action on this object.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Confirmed() As Boolean
                Get
                    Return Me.m_bConfirmed
                End Get
                Set(ByVal value As Boolean)
                    Me.m_bConfirmed = value
                End Set
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' States whether the Layer has changed.
            ''' </summary>
            ''' <returns>
            ''' True when Layer <see cref="Name">Name</see> value has changed.
            ''' </returns>
            ''' -------------------------------------------------------------------
            Public Function IsChanged() As Boolean
                If (Me.IsNew()) Then Return False
                Return (Me.m_Layer.Name <> Me.m_strName) Or _
                       (Me.Layer.Description <> Me.m_strDescription) Or _
                       (Me.Layer.Weight <> Me.m_sWeight)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' States whether the Layer is to be created.
            ''' </summary>
            ''' <returns>
            ''' True when Layer <see cref="Name">Name</see> value has changed.
            ''' </returns>
            ''' -------------------------------------------------------------------
            Public Function IsNew() As Boolean
                Return (Me.m_Layer Is Nothing)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set whether this layer is flagged for deletion. Toggling this flag
            ''' will update the <see cref="Status">Status</see> of the item.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property FlaggedForDeletion() As Boolean
                Get
                    Return Me.m_status = eItemStatusTypes.Removed
                End Get
                Set(ByVal bDelete As Boolean)
                    If Me.m_Layer IsNot Nothing Then
                        If bDelete Then
                            Me.m_status = eItemStatusTypes.Removed
                        Else
                            Me.m_status = eItemStatusTypes.Original
                        End If
                    Else
                        If bDelete Then
                            Me.m_status = eItemStatusTypes.Invalid
                        Else
                            Me.m_status = eItemStatusTypes.Added
                        End If
                    End If
                End Set
            End Property

        End Class

#End Region ' Helper classes

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create the grid
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()

            MyBase.New()
            Me.FixedColumnWidths = False

        End Sub

#Region " Grid interaction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the grid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            Me.Selection.SelectionMode = GridSelectionMode.Row
            Me.Selection.EnableMultiSelection = False

            ' JS 15Apr07: there will be no context menu item until we have a better idea
            Me.ContextMenu = Nothing

            ' Redim columns
            Me.Redim(1, System.Enum.GetValues(GetType(eColumnTypes)).Length)

            ' Layer index cell
            Me(0, eColumnTypes.LayerIndex) = New EwEColumnHeaderCell()
            ' Layer name cell, editable this time
            Me(0, eColumnTypes.LayerName) = New EwEColumnHeaderCell(SharedResources.HEADER_NAME)
            Me(0, eColumnTypes.LayerWeight) = New EwEColumnHeaderCell(SharedResources.HEADER_WEIGHT)
            Me(0, eColumnTypes.LayerDescription) = New EwEColumnHeaderCell(SharedResources.HEADER_DESCRIPTION)

            ' Layer index cell
            Me(0, eColumnTypes.LayerStatus) = New EwEColumnHeaderCell(SharedResources.HEADER_STATUS)

            ' Fix index column only; Layer name column cannot be fixed because it must be editable
            Me.FixedColumns = 1

            Me.Columns(eColumnTypes.LayerIndex).AutoSizeMode = SourceGrid2.AutoSizeMode.None
            Me.Columns(eColumnTypes.LayerWeight).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.Columns(eColumnTypes.LayerName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.Columns(eColumnTypes.LayerDescription).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            Me.Columns(eColumnTypes.LayerStatus).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.AutoStretchColumnsToFitWidth = True

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to first create a snapshot of the Layer/stanza configuration
        ''' in the current EwE model. The grid will be populated from this local
        ''' administration.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub FillData()

            Dim Layer As cEcospaceLayerImportance = Nothing
            Dim li As cLayerInfo = Nothing

            ' Populate local administration from a snapshot of the live data

            ' Make snapshot of Layer configuration
            For iLayer As Integer = 1 To Me.Core.nImportanceLayers
                Layer = Me.Core.EcospaceBasemap.LayerImportance(iLayer)
                li = New cLayerInfo(Layer)
                Me.m_alLayers.Add(li)
            Next

            ' Brute-force update grid
            UpdateGrid()

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()

            Me.Columns(eColumnTypes.LayerIndex).Width = 40
            Me.Columns(eColumnTypes.LayerName).Width = 120
            Me.Columns(eColumnTypes.LayerWeight).Width = 60
            Me.Columns(eColumnTypes.LayerDescription).Width = 278

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Brute-force resize the gird if necessary, and repopulate with data from 
        ''' the local administration.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub UpdateGrid()

            Dim li As cLayerInfo = Nothing
            Dim ri As RowInfo = Nothing
            Dim cells() As Cells.ICellVirtual = Nothing
            Dim pos As SourceGrid2.Position = Nothing
            Dim ewec As EwECell = Nothing

            ' Create missing rows
            For iRow As Integer = Me.Rows.Count To Me.m_alLayers.Count
                Me.AddRow()

                ewec = New EwECell(0, GetType(Integer))
                ewec.Style = cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable
                Me(iRow, eColumnTypes.LayerIndex) = ewec

                Me(iRow, eColumnTypes.LayerName) = New Cells.Real.Cell("", GetType(String))
                Me(iRow, eColumnTypes.LayerName).Behaviors.Add(Me.EwEEditHandler)

                Me(iRow, eColumnTypes.LayerDescription) = New Cells.Real.Cell("", GetType(String))
                Me(iRow, eColumnTypes.LayerDescription).Behaviors.Add(Me.EwEEditHandler)

                Me(iRow, eColumnTypes.LayerWeight) = New Cells.Real.Cell(0.0!, GetType(Single))
                Me(iRow, eColumnTypes.LayerWeight).Behaviors.Add(Me.EwEEditHandler)

                Me(iRow, eColumnTypes.LayerStatus) = New EwEStatusCell(eItemStatusTypes.Original)
            Next

            ' Delete obsolete rows
            While Me.Rows.Count > Me.m_alLayers.Count + 1
                Me.Rows.Remove(Me.Rows.Count - iFIRSTDATAROW)
            End While

            ' Sanity check whether grid can accomodate all Layers + header
            Debug.Assert(Me.Rows.Count = Me.m_alLayers.Count + 1)

            ' Populate rows
            For iRow As Integer = 1 To Me.m_alLayers.Count
                UpdateRow(iRow)
            Next iRow

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the content of the Row with the given index.
        ''' </summary>
        ''' <param name="iRow">The index of the row to refresh.</param>
        ''' -----------------------------------------------------------------------
        Private Sub UpdateRow(ByVal iRow As Integer)

            Dim li As cLayerInfo = Nothing
            Dim ri As RowInfo = Nothing
            Dim aCells() As Cells.ICellVirtual = Nothing
            Dim pos As SourceGrid2.Position = Nothing

            Me.AllowUpdates = False

            li = DirectCast(Me.m_alLayers(iRow - iFIRSTDATAROW), cLayerInfo)
            ri = Me.Rows(iRow)

            ri.Tag = li
            aCells = ri.GetCells()

            pos = New Position(iRow, eColumnTypes.LayerIndex)
            aCells(eColumnTypes.LayerIndex).SetValue(pos, CInt(iRow))

            pos = New Position(iRow, eColumnTypes.LayerName)
            aCells(eColumnTypes.LayerName).SetValue(pos, CStr(li.Name))

            pos = New Position(iRow, eColumnTypes.LayerDescription)
            aCells(eColumnTypes.LayerDescription).SetValue(pos, CStr(li.Description))

            pos = New Position(iRow, eColumnTypes.LayerWeight)
            aCells(eColumnTypes.LayerWeight).SetValue(pos, CSng(li.Weight))

            aCells(eColumnTypes.LayerStatus).SetValue(pos, li.Status)

            Me.AllowUpdates = True

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Called when the user has finished editing a cell. Handled to update 
        ''' local admin based on cell value changes.
        ''' </summary>
        ''' <returns>
        ''' True if the edit operation is allowed, False to cancel the edit operation.
        ''' </returns>
        ''' <remarks>
        ''' This method differs from OnCellValueChanged; at the end of an edit
        ''' operation it is once again safe to alter the value of the cell that was
        ''' just edited for text and combo box controls. *sigh*
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function OnCellEdited(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean

            If Not Me.AllowUpdates Then Return True

            Dim li As cLayerInfo = DirectCast(Me.m_alLayers(p.Row - 1), cLayerInfo)

            Select Case DirectCast(p.Column, eColumnTypes)
                Case eColumnTypes.LayerIndex
                    ' Not possible

                Case eColumnTypes.LayerName
                    Dim strName As String = CStr(cell.GetValue(p))
                    ' Check if name is unique
                    For iLayer As Integer = 0 To Me.m_alLayers.Count - 1
                        Dim giTemp As cLayerInfo = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)
                        ' Does name already exist?
                        If (Not ReferenceEquals(giTemp, li)) And (String.Compare(strName, giTemp.Name, True) = 0) Then
                            ' Change is not allowed
                            Me.UpdateRow(p.Row)
                            ' Report failure
                            Return False
                        End If
                    Next
                    ' Allow name change
                    li.Name = strName

                Case eColumnTypes.LayerDescription
                    li.Description = CStr(cell.GetValue(p))

                Case eColumnTypes.LayerWeight
                    Dim sWeight As Single = CSng(cell.GetValue(p))
                    If sWeight < 0 Then Me.UpdateRow(p.Row) : Return False
                    li.Weight = sWeight

            End Select

            Return True

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Cell click handler, called in response to clicking button-like cells.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnCellClicked(ByVal p As Position, ByVal cell As Cells.ICellVirtual)

            Select Case DirectCast(p.Column, eColumnTypes)
            End Select

        End Sub

#End Region ' Grid interaction

#Region " Row manipulation "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Delete a row from the grid
        ''' </summary>
        ''' <param name="iRow">The index of the row to delete.</param>
        ''' -----------------------------------------------------------------------
        Public Sub ToggleDeleteRow(Optional ByVal iRow As Integer = -1)

            If iRow = -1 Then iRow = Me.SelectedRow

            Dim iLayer As Integer = iRow - iFIRSTDATAROW
            Dim li As cLayerInfo = Nothing
            Dim strPrompt As String = ""

            ' Validate
            If iLayer < 0 Then Return

            li = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)
            ' Toggle 'flagged for deletion' flag
            li.FlaggedForDeletion = Not li.FlaggedForDeletion

            ' Check to see what is to happen to the Layer now
            Select Case li.Status

                Case eItemStatusTypes.Original
                    ' Clear removed status of the Layer
                    Me.m_alLayersRemoved.Remove(Me.m_alLayers(iLayer))

                Case eItemStatusTypes.Added
                    ' Clear removed status of the Layer
                    Me.m_alLayersRemoved.Remove(Me.m_alLayers(iLayer))

                Case eItemStatusTypes.Removed
                    ' Set removed status
                    Me.m_alLayersRemoved.Add(Me.m_alLayers(iLayer))

                Case eItemStatusTypes.Invalid
                    ' Set removed status
                    Me.m_alLayers.RemoveAt(iLayer)

            End Select

            Me.UpdateGrid()

        End Sub

        ''' <summary>
        ''' States whether a row holds a layer.
        ''' </summary>
        ''' <param name="iRow"></param>
        ''' <returns></returns>
        Public Function IsLayerRow(Optional ByVal iRow As Integer = -1) As Boolean
            If iRow = -1 Then iRow = Me.SelectedRow()
            Return (iRow >= iFIRSTDATAROW) And (iRow < Me.RowsCount)
        End Function

        ''' <summary>
        ''' States whether the layer on a row is flagged for deletion.
        ''' </summary>
        Public Function IsFlaggedForDeletionRow(Optional ByVal iRow As Integer = -1) As Boolean
            If iRow = -1 Then iRow = Me.SelectedRow()
            If Not IsLayerRow(iRow) Then Return False

            Dim iLayer As Integer = iRow - iFIRSTDATAROW
            Dim li As cLayerInfo = Nothing
            Dim strPrompt As String = ""

            li = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)
            Return li.FlaggedForDeletion
        End Function

        ''' <summary>
        ''' Add a row by creating a new layer.
        ''' </summary>
        Public Sub InsertRow()
            If Not Me.CanAddRow() Then Return
            Me.CreateLayer()
        End Sub

        ''' <summary>
        ''' Create a new layer.
        ''' </summary>
        Private Sub CreateLayer()
            Dim iRow As Integer = -1
            Dim iLayer As Integer = -1
            Dim li As cLayerInfo = Nothing
            Dim lstrLayers As New List(Of String)

            ' Make fit
            iRow = Math.Max(iFIRSTDATAROW, Me.RowsCount)
            iLayer = iRow - iFIRSTDATAROW

            ' Validate
            If iLayer < 0 Then Return

            ' Collect all current layer names
            For Each li In Me.m_alLayers
                lstrLayers.Add(li.Name)
            Next

            ' Format new layer with an autonumber value based on existing names
            Dim iNextNum As Integer = cStringUtils.GetNextNumber(lstrLayers.ToArray(), SharedResources.DEFAULT_NEWLAYER_NUM)
            Dim strName As String = cStringUtils.Localize(SharedResources.DEFAULT_NEWLAYER_NUM, iNextNum)

            li = New cLayerInfo(strName, "", 1.0!)
            Me.m_alLayers.Insert(iLayer, li)

            Me.UpdateGrid()
            Me.SelectRow(li)
        End Sub

        ''' <summary>
        ''' States whether a row can be inserted at the indicated position.
        ''' </summary>
        Public Function CanAddRow() As Boolean
            Return True
        End Function

#End Region ' Row manipulation 

#Region " Admin "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Update lock, should be set when modifying cell values from the code
        ''' to prevent recursive update/notification loops.
        ''' </summary>
        ''' <returns>True when no update lock is active.</returns>
        ''' <remarks>
        ''' Update locks are cumulative: setting this lock twice will require 
        ''' clearing it twice to allow updates to happen.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Property AllowUpdates() As Boolean
            Get
                Return (Me.m_iUpdateLock = 0)
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    Me.m_iUpdateLock += 1
                Else
                    Me.m_iUpdateLock -= 1
                End If
            End Set
        End Property

#Region " Selection extension "

        Private Overloads Sub SelectRow(ByVal li As cLayerInfo)
            For iLayer As Integer = 0 To Me.m_alLayers.Count - 1
                If ReferenceEquals(Me.m_alLayers(iLayer), li) Then
                    Me.SelectRow(iLayer + iFIRSTDATAROW)
                End If
            Next
        End Sub

#End Region ' Selection extension

#End Region ' Admin

#Region " Validation "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; validates the content of the grid.
        ''' </summary>
        ''' <returns>True when the content of the grid depicts a valid
        ''' Layer configuration for a model.</returns>
        ''' -----------------------------------------------------------------------
        Public Function ValidateContent() As Boolean
            Return True
        End Function

#End Region ' Validation

#Region " Apply changes "

        Public Function Apply() As Boolean

            Dim strPrompt As String = ""
            Dim bConfigurationChanged As Boolean = False
            Dim bLayersChanged As Boolean = False
            Dim li As cLayerInfo = Nothing
            Dim iDBID As Integer = Nothing
            Dim Layer As cEcospaceLayerImportance = Nothing
            Dim iLayer As Integer = 0
            Dim bSuccess As Boolean = True

            ' Validate content of the grid
            If Not Me.ValidateContent() Then Return False

            ' Assess Layer changes
            For iLayer = 0 To Me.m_alLayers.Count - 1
                li = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)
                ' Check if this layer is newly added
                bConfigurationChanged = bConfigurationChanged Or li.IsNew()
                ' Check if this layer has been modified
                bLayersChanged = bLayersChanged Or li.IsChanged()
            Next iLayer

            If Me.m_alLayersRemoved.Count > 5 Then

                strPrompt = cStringUtils.Localize(My.Resources.ECOSPACE_EDITLAYER_CONFIRMDELETENUM_PROMPT, Me.m_alLayersRemoved.Count)
                Dim fmsg As New cFeedbackMessage(strPrompt, eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO_CANCEL)
                Me.UIContext.Core.Messages.SendMessage(fmsg)

                Select Case fmsg.Reply
                    Case eMessageReply.CANCEL
                        ' Abort Apply process
                        Return False
                    Case eMessageReply.YES
                        ' Confirm all regions
                        For Each li In Me.m_alLayersRemoved
                            li.Confirmed = True
                        Next
                        bConfigurationChanged = True
                    Case eMessageReply.NO
                        ' NOP
                    Case Else
                        ' Unexpected anwer: assert
                        Debug.Assert(False)
                End Select

            Else
                ' Assess Layers to remove
                For iLayer = 0 To Me.m_alLayersRemoved.Count - 1
                    li = DirectCast(Me.m_alLayersRemoved(iLayer), cLayerInfo)
                    If (Not li.IsNew()) Then

                        strPrompt = cStringUtils.Localize(My.Resources.ECOSPACE_EDITLAYER_CONFIRMDELETE_PROMPT, li.Name)
                        Dim fmsg As New cFeedbackMessage(strPrompt, eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO_CANCEL)
                        Me.UIContext.Core.Messages.SendMessage(fmsg)

                        Select Case fmsg.Reply
                            Case eMessageReply.CANCEL
                                ' Abort Apply process
                                Return False
                            Case eMessageReply.NO
                                ' Do not delete this Layer
                                li.Confirmed = False
                            Case eMessageReply.YES
                                ' Delete this Layer
                                li.Confirmed = True
                                bConfigurationChanged = True
                            Case Else
                                ' Unexpected anwer: assert
                                Debug.Assert(False)
                        End Select

                    End If
                Next iLayer
            End If

            ' Handle added and removed items
            If (bConfigurationChanged) Then

                If Not Me.Core.SetBatchLock(cCore.eBatchLockType.Restructure) Then Return False
                cApplicationStatusNotifier.StartProgress(Me.Core, SharedResources.GENERIC_STATUS_APPLYCHANGES)

                ' Add new Layers
                For iLayer = 0 To Me.m_alLayers.Count - 1
                    li = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)
                    If (li.IsNew()) Then
                        bSuccess = bSuccess And Me.Core.AddEcospaceImportanceLayer(li.Name, li.Description, li.Weight, iDBID)
                    End If
                Next

                ' Remove deleted (and confirmed) Layers
                Dim iLayerRemove As Integer = 0
                For iLayer = 0 To Me.m_alLayersRemoved.Count - 1
                    li = DirectCast(Me.m_alLayersRemoved(iLayerRemove), cLayerInfo)

                    ' Sanity check
                    Debug.Assert(Not li.IsNew())

                    If (li.Confirmed()) Then
                        If (Me.Core.RemoveEcospaceImportanceLayer(li.Layer)) Then
                            Me.m_alLayers.Remove(li)
                            Me.m_alLayersRemoved.Remove(li)
                        Else
                            bSuccess = False
                            iLayerRemove += 1
                        End If
                    End If
                Next

                ' The core will reload now
                Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecospace)
                cApplicationStatusNotifier.EndProgress(Me.Core)

                ' Test whether new Layers were loaded correctly 
                Debug.Assert(Me.m_alLayers.Count = Me.Core.nImportanceLayers, ">> Internal panic: Dialog and core out of sync on Layers")
            End If

            ' Update core objects
            If (bLayersChanged) Then
                ' For each local layer admin unit
                For iLayer = 0 To Me.m_alLayers.Count - 1
                    ' Get local admin unit
                    li = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)
                    ' Has it changed?
                    If (li.IsChanged()) Then
                        ' Find core layer with same BDID (cannot use cached cEcospaceBasemap instances since the core has reloaded)
                        Dim bFound As Boolean = False
                        ' For every core layer instance (and yes, this array is one-based)
                        For iLayTest As Integer = 1 To Me.Core.nImportanceLayers
                            ' Get core layer instance
                            Dim layTest As cEcospaceLayerImportance = Me.Core.EcospaceBasemap.LayerImportance(iLayTest)
                            ' Has matching ID?
                            If (layTest.getID = li.Layer.getID) Then
                                ' #Yes: Update
                                layTest.Name = li.Name
                                layTest.Description = li.Description
                                layTest.Weight = li.Weight
                                ' Are we relieved or what!
                                bFound = True
                            End If
                        Next
                        ' All went well?
                        If Not bFound Then
                            ' #No?! Uh oh...
                            Debug.Assert(False, ">> Internal panic: Unable to apply changes to layer id " & li.Layer.getID)
                        End If
                    End If
                Next
            End If

            Return bSuccess

        End Function

#End Region ' Apply changes

    End Class

End Namespace


