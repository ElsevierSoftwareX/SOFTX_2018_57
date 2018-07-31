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
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SourceGrid2
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecospace

    <CLSCompliant(False)> _
    Public Class gridDefineEnvDriverMaps
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
            LayerIsCapacityEnabled
            LayerUnits
            LayerDescription
            LayerStatus
        End Enum

#Region " Helper classes "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Administrative unit representing a <see cref="cEcospaceLayerDriver"/>
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

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Constructor, initializes a new instanze of this class.
            ''' </summary>
            ''' <param name="Layer">The <see cref="cEcospaceLayerDriver"/> to
            ''' initialize this instance from. If set, this instance represents a
            ''' Layer currently active in the EwE model.</param>
            ''' <param name="bEditable">States if the layer can be edited.</param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal Layer As cEcospaceLayer, Optional bEditable As Boolean = True)
                Debug.Assert(Layer IsNot Nothing)
                Me.Layer = Layer
                Me.Name = Layer.Name
                If (TypeOf Layer Is cEcospaceLayerDriver) Then
                    Me.Description = DirectCast(Layer, cEcospaceLayerDriver).Description
                    Me.Units = DirectCast(Layer, cEcospaceLayerDriver).Units
                Else
                    ' Fixed description
                    Dim fmt As New cVarnameTypeFormatter()
                    Me.Description = fmt.GetDescriptor(Layer.VarName, eDescriptorTypes.Description)
                    ' Standard units
                    Me.Units = cVariableMetaData.Get(Layer.VarName).Units
                End If
                Me.Status = eItemStatusTypes.Original
                Me.IsCapacityEnabled = True
                Me.IsEditable = bEditable
                Me.LayerID = Layer.DBID
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Constructor, initializes a new instanze of this class.
            ''' </summary>
            ''' <param name="strName">Name to assign to this administrative unit.</param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal strName As String, ByVal strDescription As String, ByVal strUnits As String)
                Me.Layer = Nothing
                Me.Name = strName
                Me.Description = strDescription
                Me.Units = strUnits
                Me.Status = eItemStatusTypes.Added
                Me.IsCapacityEnabled = True
                Me.IsEditable = True
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the name of this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Name() As String

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the units of this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Units() As String

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the description of this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Description() As String

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="cEcospaceBasemap">EwE Layer</see> associated
            ''' with this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property Layer() As cEcospaceLayer = Nothing

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="eItemStatusTypes">item status</see>
            ''' for the layer object.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Status() As eItemStatusTypes

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set whether the user has confirmed an action on this object.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Confirmed() As Boolean

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
                If (TypeOf Layer Is cEcospaceLayerDriver) Then
                    If (DirectCast(Layer, cEcospaceLayerDriver).Units <> Me.Units) Then
                        Return False
                    End If
                End If
                Return (Me.Layer.Name <> Me.Name) Or
                       (Me.Layer.Description <> Me.Description)
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
                Return (Me.Layer Is Nothing)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set whether this layer is flagged for deletion. Toggling this flag
            ''' will update the <see cref="Status">Status</see> of the item.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property FlaggedForDeletion() As Boolean
                Get
                    Return (Me.Status = eItemStatusTypes.Removed)
                End Get
                Set(ByVal bDelete As Boolean)
                    If (Me.Layer IsNot Nothing) Then
                        If bDelete Then
                            Me.Status = eItemStatusTypes.Removed
                        Else
                            Me.Status = eItemStatusTypes.Original
                        End If
                    Else
                        If bDelete Then
                            Me.Status = eItemStatusTypes.Invalid
                        Else
                            Me.Status = eItemStatusTypes.Added
                        End If
                    End If
                End Set
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get whether the layer can be modified.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property IsEditable As Boolean

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set whether the layer is active for capacity calculations.
            ''' </summary>
            ''' <remarks>
            ''' This logic really belongs in a dedicated interface.
            ''' </remarks>
            ''' -------------------------------------------------------------------
            Public Property IsCapacityEnabled() As Boolean

            Public Property LayerID As Integer

        End Class

#End Region ' Helper classes

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create the grid
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()

            MyBase.New()

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
            Me.AllowBlockSelect = False

            ' JS 15Apr07: there will be no context menu item until we have a better idea
            Me.ContextMenu = Nothing

            ' Redim columns
            Me.Redim(1, System.Enum.GetValues(GetType(eColumnTypes)).Length)

            ' Layer index cell
            Me(0, eColumnTypes.LayerIndex) = New EwEColumnHeaderCell()
            Me(0, eColumnTypes.LayerName) = New EwEColumnHeaderCell(SharedResources.HEADER_NAME)
            Me(0, eColumnTypes.LayerUnits) = New EwEColumnHeaderCell(SharedResources.HEADER_UNITS)
            Me(0, eColumnTypes.LayerDescription) = New EwEColumnHeaderCell(SharedResources.HEADER_DESCRIPTION)
            Me(0, eColumnTypes.LayerIsCapacityEnabled) = New EwEColumnHeaderCell(SharedResources.HEADER_ENABLED_CAPACITY)

            ' Layer index cell
            Me(0, eColumnTypes.LayerStatus) = New EwEColumnHeaderCell(SharedResources.HEADER_STATUS)

            ' Fix index column only; Layer name column cannot be fixed because it must be editable
            Me.FixedColumns = 1
            Me.FixedColumnWidths = False

            Me.Columns(eColumnTypes.LayerIndex).AutoSizeMode = SourceGrid2.AutoSizeMode.None
            Me.Columns(eColumnTypes.LayerName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.Columns(eColumnTypes.LayerUnits).AutoSizeMode = SourceGrid2.AutoSizeMode.None
            Me.Columns(eColumnTypes.LayerDescription).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            Me.Columns(eColumnTypes.LayerIsCapacityEnabled).AutoSizeMode = SourceGrid2.AutoSizeMode.None
            Me.Columns(eColumnTypes.LayerStatus).AutoSizeMode = SourceGrid2.AutoSizeMode.None
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

            Dim layer As cEcospaceLayerDriver = Nothing
            Dim li As cLayerInfo = Nothing

            ' Populate local administration from a snapshot of the live data

            Dim depth As cEcospaceLayer = Me.Core.EcospaceBasemap.LayerDepth
            'Depth layer cannot be deleted
            li = New cLayerInfo(depth, bEditable:=False)
            li.IsCapacityEnabled = depth.IsActive
            Me.m_alLayers.Add(li)

            ' Make snapshot of Layer configuration
            For iLayer As Integer = 1 To Me.Core.nEnvironmentalDriverLayers
                layer = Me.Core.EcospaceBasemap.LayerDriver(iLayer)
                li = New cLayerInfo(layer)
                li.IsCapacityEnabled = layer.IsActive
                Me.m_alLayers.Add(li)
            Next

            ' Brute-force update grid
            UpdateGrid()

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()

            'Me.Columns(eColumnTypes.LayerIndex).Width = 40
            'Me.Columns(eColumnTypes.LayerName).Width = 120
            'Me.Columns(eColumnTypes.LayerDescription).Width = 200
            'Me.Columns(eColumnTypes.LayerIsCApacityEnabled).Width = 50

            Me.AutoSizeColumnRange(eColumnTypes.LayerIndex, eColumnTypes.LayerStatus, 0, 0)

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
            Dim style As cStyleGuide.eStyleFlags

            ' Create missing rows
            For iRow As Integer = Me.Rows.Count To Me.m_alLayers.Count
                Me.AddRow()

                li = DirectCast(Me.m_alLayers(iRow - iFIRSTDATAROW), cLayerInfo)

                If (li.IsEditable) Then
                    style = cStyleGuide.eStyleFlags.OK
                Else
                    style = cStyleGuide.eStyleFlags.NotEditable
                End If

                ewec = New EwECell(0, GetType(Integer))
                ewec.Style = cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable
                Me(iRow, eColumnTypes.LayerIndex) = ewec

                Me(iRow, eColumnTypes.LayerName) = New EwECell("", GetType(String), style)
                If li.IsEditable Then Me(iRow, eColumnTypes.LayerName).Behaviors.Add(Me.EwEEditHandler)

                Me(iRow, eColumnTypes.LayerUnits) = New EwECell("", GetType(String), style)
                If li.IsEditable Then Me(iRow, eColumnTypes.LayerUnits).Behaviors.Add(Me.EwEEditHandler)

                Me(iRow, eColumnTypes.LayerDescription) = New EwECell("", GetType(String), style)
                If li.IsEditable Then Me(iRow, eColumnTypes.LayerDescription).Behaviors.Add(Me.EwEEditHandler)

                Me(iRow, eColumnTypes.LayerIsCapacityEnabled) = New Cells.Real.CheckBox(False)
                Me(iRow, eColumnTypes.LayerIsCapacityEnabled).Behaviors.Add(Me.EwEEditHandler)

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
            Dim cell As EwECell = Nothing
            Dim pos As SourceGrid2.Position = Nothing
            Dim u As New cUnits(Me.Core)

            Me.AllowUpdates = False

            li = DirectCast(Me.m_alLayers(iRow - iFIRSTDATAROW), cLayerInfo)
            ri = Me.Rows(iRow)

            ri.Tag = li
            aCells = ri.GetCells()

            Dim bEditable As Boolean = li.IsEditable

            pos = New Position(iRow, eColumnTypes.LayerIndex)
            aCells(eColumnTypes.LayerIndex).SetValue(pos, iRow)

            pos = New Position(iRow, eColumnTypes.LayerName)
            aCells(eColumnTypes.LayerName).SetValue(pos, li.Name)

            pos = New Position(iRow, eColumnTypes.LayerUnits)
            ' Show units for 'Depth' layer as formatted
            aCells(eColumnTypes.LayerUnits).SetValue(pos, If(iRow = 1, u.ToString(li.Units), li.Units))

            pos = New Position(iRow, eColumnTypes.LayerDescription)
            aCells(eColumnTypes.LayerDescription).SetValue(pos, li.Description)

            pos = New Position(iRow, eColumnTypes.LayerIsCapacityEnabled)
            aCells(eColumnTypes.LayerIsCapacityEnabled).SetValue(pos, li.IsCapacityEnabled)

            pos = New Position(iRow, eColumnTypes.LayerStatus)
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

            'Depth row can't be edited
            If Not li.IsEditable Then Return True

            Try

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

                    Case eColumnTypes.LayerUnits
                        Dim val As Object = cell.GetValue(p)
                        If (val Is Nothing) Then val = ""
                        li.Units = CStr(val)

                    Case eColumnTypes.LayerDescription
                        Dim val As Object = cell.GetValue(p)
                        If (val Is Nothing) Then val = ""
                        li.Description = CStr(val)

                    Case Else
                        ' NOP

                End Select
            Catch ex As Exception

            End Try

            Return True

        End Function

        Protected Overrides Function OnCellValueChanged(ByVal p As SourceGrid2.Position, ByVal cell As SourceGrid2.Cells.ICellVirtual) As Boolean

            If Not Me.AllowUpdates Then Return True

            Try
                Dim li As cLayerInfo = DirectCast(Me.m_alLayers(p.Row - 1), cLayerInfo)
                Select Case DirectCast(p.Column, eColumnTypes)
                    Case eColumnTypes.LayerIsCapacityEnabled
                        li.IsCapacityEnabled = CBool(cell.GetValue(p))
                    Case Else
                        ' NOP
                End Select
            Catch ex As Exception

            End Try
            Return MyBase.OnCellValueChanged(p, cell)

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
        ''' -----------------------------------------------------------------------
        Public Sub ToggleDeleteRow()

            For Each row As RowInfo In Me.Selection.SelectedRows

                Dim iRow As Integer = row.Index
                Dim iLayer As Integer = iRow - iFIRSTDATAROW
                Dim li As cLayerInfo = Nothing
                Dim strPrompt As String = ""

                If (iLayer >= 0) Then
                    li = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)

                    ' Depth row cannot be deleted
                    If (li.IsEditable) Then

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
                    End If
                End If
            Next

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

        Public Function CanRemoveRow(Optional ByVal iRow As Integer = -1) As Boolean
            If (iRow <= 0) Then iRow = Me.SelectedRow()
            If (iRow <= 0) Then Return False
            Return Me.m_alLayers(iRow - 1).IsEditable
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
        ''' Create a new layer.
        ''' </summary>
        Private Sub CreateLayer(iRow As Integer)
            Dim iLayer As Integer = -1
            Dim li As cLayerInfo = Nothing
            Dim lstrLayers As New List(Of String)

            ' Make fit
            iRow = Math.Min(Math.Max(iRow, iFIRSTDATAROW), Me.RowsCount)
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

            li = New cLayerInfo(strName, "", "")
            Me.m_alLayers.Insert(iLayer, li)

            Me.UpdateGrid()
            Me.SelectRow(li)
        End Sub

        Public Sub AppendRow()
            Me.CreateLayer(Me.RowsCount)
        End Sub

        Public Sub MoveRowUp(Optional ByVal iRow As Integer = -1)
            Dim bMoveSelection As Boolean = (iRow = -1)

            If iRow = -1 Then iRow = Me.SelectedRow()
            If Not CanMoveRowUp(iRow) Then Return
            Me.MoveRow(iRow, iRow - 1)

            If bMoveSelection Then
                Me.SelectRow(iRow - 1)
            End If
        End Sub

        Public Function CanMoveRowUp(Optional ByVal iRow As Integer = -1) As Boolean

            If iRow = -1 Then iRow = Me.SelectedRow()
            If iRow < (iFIRSTDATAROW + 1) Then Return False
            If (Me.RowsCount <= (iFIRSTDATAROW + 1)) Then Return False
            If (iRow >= Me.RowsCount) Then Return False
            Dim li1 As cLayerInfo = Me.m_alLayers(iRow - iFIRSTDATAROW)
            Dim li2 As cLayerInfo = Me.m_alLayers(iRow - 1 - iFIRSTDATAROW)
            Return li1.IsEditable And li2.IsEditable

        End Function

        Public Sub MoveRowDown(Optional ByVal iRow As Integer = -1)
            Dim bMoveSelection As Boolean = (iRow = -1)

            If iRow = -1 Then iRow = Me.SelectedRow()
            If Not CanMoveRowDown(iRow) Then Return
            Me.MoveRow(iRow, iRow + 1)

            If bMoveSelection Then
                Me.SelectRow(iRow + 1)
            End If
        End Sub

        Public Function CanMoveRowDown(Optional ByVal iRow As Integer = -1) As Boolean
            If iRow = -1 Then iRow = Me.SelectedRow()
            If iRow < iFIRSTDATAROW Then Return False
            If (Me.RowsCount <= (iFIRSTDATAROW + 1)) Then Return False
            If (iRow >= Me.RowsCount - 1) Then Return False
            Dim li1 As cLayerInfo = Me.m_alLayers(iRow - iFIRSTDATAROW)
            Dim li2 As cLayerInfo = Me.m_alLayers(iRow + 1 - iFIRSTDATAROW)
            Return li1.IsEditable And li2.IsEditable
        End Function

        Private Sub MoveRow(ByVal iFromRow As Integer, ByVal iToRow As Integer)

            Dim objTemp As cLayerInfo = Nothing
            Dim iStep As Integer = 1
            Dim iFrom As Integer = iFromRow - iFIRSTDATAROW
            Dim iTo As Integer = iToRow - iFIRSTDATAROW

            ' Truncate
            iFrom = Math.Max(0, Math.Min(Me.m_alLayers.Count - 1, iFrom))
            iTo = Math.Max(0, Math.Min(Me.m_alLayers.Count - 1, iTo))

            ' Nothing to do? abort
            If iFrom = iTo Then Return
            ' Determine direction of movement
            If iFrom < iTo Then iStep = 1 Else iStep = -1

            ' Swap rows (but do not swap the row at iTo because then we've gone 1 too far)
            For iGroup As Integer = iFrom To iTo - iStep Step iStep
                objTemp = Me.m_alLayers(iGroup + iStep)
                Me.m_alLayers(iGroup + iStep) = Me.m_alLayers(iGroup)
                Me.m_alLayers(iGroup) = objTemp
                Me.UpdateRow(iGroup + iFIRSTDATAROW)
                Me.UpdateRow(iGroup + iFIRSTDATAROW + iStep)
            Next iGroup

        End Sub
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
            Dim Layer As cEcospaceLayerDriver = Nothing
            Dim iLayer As Integer = 0
            Dim bSuccess As Boolean = True

            ' Validate content of the grid
            If Not Me.ValidateContent() Then Return False

            ' Assess Layer changes
            For iLayer = 0 To Me.m_alLayers.Count - 1
                li = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)
                ' Check if this layer is newly added
                bConfigurationChanged = bConfigurationChanged Or li.IsNew()
                If Not li.IsNew Then
                    ' Check if this layer has been modified
                    bLayersChanged = bLayersChanged Or li.IsChanged()
                    ' This test got screwed up because the depth layer - which is no environmental driver layer - was inserted
                    'bConfigurationChanged = bConfigurationChanged Or (li.Layer.Index <> (iLayer + 1))
                    bConfigurationChanged = bConfigurationChanged Or ((li.Layer.VarName = eVarNameFlags.LayerDriver) And (li.Layer.Index <> iLayer))
                End If
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

                iLayer = 0
                ' Add and move Layers
                While (bSuccess = True) And (iLayer < Me.m_alLayers.Count)
                    li = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)
                    If (li.IsNew()) Then
                        bSuccess = bSuccess And Me.Core.AddEcospaceDriverLayer(li.Name, li.Description, li.Units, li.LayerID)
                    Else
                        If ((iLayer + 1) <> li.Layer.Index) Then
                            If Not Me.Core.MoveDriverLayer(li.Layer.Index, iLayer + 1) Then
                                bSuccess = False
                            End If
                        End If
                    End If
                    iLayer += 1
                End While

                ' Remove deleted (and confirmed) Layers
                Dim iLayerRemove As Integer = 0
                For iLayer = 0 To Me.m_alLayersRemoved.Count - 1
                    li = DirectCast(Me.m_alLayersRemoved(iLayerRemove), cLayerInfo)

                    ' Sanity check
                    Debug.Assert(Not li.IsNew())

                    If (li.Confirmed()) Then
                        If (Me.Core.RemoveEcospaceDriverLayer(li.Layer.DBID)) Then
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
                Debug.Assert(Me.m_alLayers.Count - 1 = Me.Core.nEnvironmentalDriverLayers, ">> Internal panic: Dialog and core out of sync on Layers")
            End If

            ' Update core objects
            If (bLayersChanged) Then

                If Not Me.Core.SetBatchLock(cCore.eBatchLockType.Update) Then Return False

                Try

                    ' For each local layer admin unit
                    'Skip the Depth layer in the first index
                    'It's not in the EcospaceBasemap.LayerDriver(iLayTest) list
                    For iLayer = 1 To Me.m_alLayers.Count - 1
                        ' Get local admin unit
                        li = DirectCast(Me.m_alLayers(iLayer), cLayerInfo)
                        ' Find core layer with same BDID (cannot use cached cEcospaceBasemap instances since the core has reloaded)
                        Dim bFound As Boolean = False
                        ' For every core layer instance (and yes, this array is one-based)
                        For iLayTest As Integer = 1 To Me.Core.nEnvironmentalDriverLayers
                            ' Get core layer instance
                            Dim layTest As cEcospaceLayerDriver = Me.Core.EcospaceBasemap.LayerDriver(iLayTest)
                            ' Is this 'our' layer?
                            If (layTest.DBID = li.LayerID) Then
                                ' Has it changed?
                                If (li.IsChanged()) Then
                                    layTest.Name = li.Name
                                    layTest.Description = li.Description
                                    layTest.Units = li.Units
                                End If
                                ' Set enabled state
                                layTest.IsActive = li.IsCapacityEnabled
                                Core.onChanged(layTest, eMessageType.DataModified)
                                bFound = True
                            End If
                        Next
                        ' All went well?
                        If Not bFound Then
                            ' #No?! Uh oh...
                            Debug.Assert(False, ">> Internal panic: Unable to apply changes to layer id " & li.Layer.getID)
                        End If
                    Next
                Catch ex As Exception

                End Try
                Return Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecospace)
            End If

            Return bSuccess

        End Function

#End Region ' Apply changes

    End Class

End Namespace


