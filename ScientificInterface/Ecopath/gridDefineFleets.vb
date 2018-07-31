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
Imports EwEUtils.Utilities
Imports ScientificInterface.Other
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.VisualModels
Imports EwEUtils.Core

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' Grid class for the Edit Fleets interface.
''' </summary>
''' -----------------------------------------------------------------------
<CLSCompliant(False)> _
   Public Class gridDefineFleets
    : Inherits EwEGrid

#Region " Private vars "

    ''' <summary>A number representing the row that contains the first Fleet</summary>
    Private Const iFIRSTFLEETROW As Integer = 1

    ''' <summary>List of active Fleets.</summary>
    Private m_lfiFleets As New List(Of cFleetInfo)
    ''' <summary>List of removed Fleets.</summary>
    Private m_lfiFleetsRemoved As New List(Of cFleetInfo)
    ''' <summary>Update lock, used to distinguish between code updates and
    ''' user updates of grid cells. When grid cells are updated from within
    ''' the code, an update lock should be active to prevent edit/update recursion.</summary>
    Private m_iUpdateLock As Integer = 0

    ''' <summary>Enumerated type defining the columns in this grid.</summary>
    Private Enum eColumnTypes
        FleetIndex = 0
        FleetName
        FleetColor
        FleetStatus
    End Enum

#End Region ' Private vars

#Region " Helper classes "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Administrative unit representing a <see cref="cEcopathFleetInput">Fleet</see>
    ''' in the EwE model.
    ''' </summary>
    ''' <remarks>
    ''' This class can represent existing and new Fleets.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Class cFleetInfo

        Private m_iFleetIndex As Integer = -1
        Private m_iFleetDBID As Integer = cCore.NULL_VALUE

        ''' <summary>The status of a Fleet in the interface.</summary>
        Private m_status As eItemStatusTypes = eItemStatusTypes.Original

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instanze of this class.
        ''' </summary>
        ''' <param name="Fleet">The <see cref="cEcopathFleetInput">cEcopathFleetInput</see> to
        ''' initialize this instance from. If set, this instance represents a
        ''' Fleet currently active in the EwE model.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal fleet As cEcopathFleetInput)
            Debug.Assert(fleet IsNot Nothing)
            Me.m_iFleetDBID = fleet.DBID
            Me.m_iFleetIndex = fleet.Index
            Me.Name = fleet.Name
            Me.PoolColor = fleet.PoolColor
            Me.m_status = eItemStatusTypes.Original
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instanze of this class.
        ''' </summary>
        ''' <param name="strName">Name to assign to this administrative unit.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal strName As String)
            Me.Name = strName
            Me.PoolColor = 0
            Me.m_status = eItemStatusTypes.Added
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the name of this administrative unit.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Name() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cEcopathDataStructures.FleetColor">Color</see> value of
        ''' this administrative unit.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property PoolColor() As Integer

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCoreInputOutputBase.Index"/> of the fleet associated
        ''' with this administrative unit.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property FleetIndex() As Integer
            Get
                Return Me.m_iFleetIndex
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCoreInputOutputBase.DBID"/> of the fleet associated
        ''' with this administrative unit.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property FleetDBID() As Integer
            Get
                Return Me.m_iFleetDBID
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eItemStatusTypes">item status</see> for the fleet 
        ''' object.
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

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the underlying fleet has been changed.
        ''' </summary>
        ''' <returns>
        ''' True if the underlying fleet has been changed.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function IsChanged(ByVal fleet As cEcopathFleetInput) As Boolean
            If (Me.m_iFleetDBID <> fleet.DBID) Then Return False
            Return (fleet.Name <> Me.Name) Or
                   (fleet.PoolColor <> Me.PoolColor)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether this info is not associated with an existing fleet.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function IsNew() As Boolean
            Return Me.m_iFleetDBID = cCore.NULL_VALUE
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether this fleet is flagged for deletion. Toggling this flag
        ''' will update the <see cref="Status">Status</see> of the item.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property FlaggedForDeletion() As Boolean
            Get
                Return Me.m_status = eItemStatusTypes.Removed
            End Get
            Set(ByVal bDelete As Boolean)
                If Not Me.IsNew() Then
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

        ' Fleet index cell
        Me(0, eColumnTypes.FleetIndex) = New EwEColumnHeaderCell()
        ' Fleet name cell, editable this time
        Me(0, eColumnTypes.FleetName) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEETNAME)
        ' Color
        Me(0, eColumnTypes.FleetColor) = New EwEColumnHeaderCell(SharedResources.HEADER_COLOR)

        ' Fleet index cell
        Me(0, eColumnTypes.FleetStatus) = New EwEColumnHeaderCell(SharedResources.HEADER_STATUS)

        ' Fix index column only; Fleet name column cannot be fixed because it must be editable
        Me.FixedColumns = 1

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Overridden to first create a snapshot of the Fleet/stanza configuration
    ''' in the current EwE model. The grid will be populated from this local
    ''' administration.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected Overrides Sub FillData()

        Dim Fleet As cEcopathFleetInput = Nothing
        Dim fi As cFleetInfo = Nothing

        ' Populate local administration from a snapshot of the live data
        Me.m_lfiFleets.Clear()

        ' Make snapshot of Fleet configuration
        For iFleet As Integer = 1 To Me.Core.nFleets
            Fleet = Core.EcopathFleetInputs(iFleet)
            fi = New cFleetInfo(Fleet)
            Me.m_lfiFleets.Add(fi)
        Next

        ' Brute-force update grid
        UpdateGrid()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Brute-force resize the gird if necessary, and repopulate with data from 
    ''' the local administration.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub UpdateGrid()

        Dim fi As cFleetInfo = Nothing
        Dim ri As RowInfo = Nothing
        Dim cells() As Cells.ICellVirtual = Nothing
        Dim pos As SourceGrid2.Position = Nothing
        Dim ewec As EwECell = Nothing

        ' Create missing rows
        For iRow As Integer = Me.Rows.Count To Me.m_lfiFleets.Count
            Me.AddRow()

            ewec = New EwECell(0, GetType(Integer))
            ewec.Style = cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable
            Me(iRow, eColumnTypes.FleetIndex) = ewec

            Me(iRow, eColumnTypes.FleetName) = New Cells.Real.Cell("", GetType(String))
            Me(iRow, eColumnTypes.FleetName).Behaviors.Add(Me.EwEEditHandler)

            Me(iRow, eColumnTypes.FleetColor) = New Cells.Real.Cell()
            Me(iRow, eColumnTypes.FleetColor).VisualModel = New cEwEGridColorVisualizer()
            Me(iRow, eColumnTypes.FleetColor).Behaviors.Add(Me.EwEEditHandler)

            Me(iRow, eColumnTypes.FleetStatus) = New EwEStatusCell(eItemStatusTypes.Original)
        Next

        ' Delete obsolete rows
        While Me.Rows.Count > Me.m_lfiFleets.Count + 1
            Me.Rows.Remove(Me.Rows.Count - iFIRSTFLEETROW)
        End While

        ' Sanity check whether grid can accomodate all Fleets + header
        Debug.Assert(Me.Rows.Count = Me.m_lfiFleets.Count + 1)

        ' Populate rows
        For iRow As Integer = 1 To Me.m_lfiFleets.Count
            UpdateRow(iRow)
        Next iRow

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        ' Should size to fit header
        Me.Columns(eColumnTypes.FleetColor).Width = 80
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Refresh the content of the Row with the given index.
    ''' </summary>
    ''' <param name="iRow">The index of the row to refresh.</param>
    ''' -----------------------------------------------------------------------
    Private Sub UpdateRow(ByVal iRow As Integer)

        Dim fi As cFleetInfo = Nothing
        Dim ri As RowInfo = Nothing
        Dim aCells() As Cells.ICellVirtual = Nothing
        Dim pos As SourceGrid2.Position = Nothing
        Dim vm As IVisualModel = Nothing
        Dim strText As String = ""

        Me.AllowUpdates = False

        fi = DirectCast(Me.m_lfiFleets(iRow - iFIRSTFLEETROW), cFleetInfo)
        ri = Me.Rows(iRow)

        ri.Tag = fi
        aCells = ri.GetCells()

        pos = New Position(iRow, eColumnTypes.FleetIndex)
        aCells(eColumnTypes.FleetIndex).SetValue(pos, CInt(iRow))

        pos = New Position(iRow, eColumnTypes.FleetName)
        aCells(eColumnTypes.FleetName).SetValue(pos, CStr(fi.Name))

        pos = New Position(iRow, eColumnTypes.FleetColor)
        Dim clr As Color = cColorUtils.IntToColor(fi.PoolColor)
        If clr.A = 0 Then clr = Me.StyleGuide.FleetColorDefault(iRow, Me.m_lfiFleets.Count)
        aCells(eColumnTypes.FleetColor).SetValue(pos, clr)

        aCells(eColumnTypes.FleetStatus).SetValue(pos, fi.Status)

        Me.AllowUpdates = True

    End Sub

    Private Sub UpdateColorColumn()

        Dim fi As cFleetInfo = Nothing
        Dim clr As Color = Color.Transparent

        Me.AllowUpdates = False
        For iRow As Integer = iFIRSTFLEETROW To Me.RowsCount - 1
            fi = DirectCast(Me.m_lfiFleets(iRow - iFIRSTFLEETROW), cFleetInfo)
            clr = cColorUtils.IntToColor(fi.PoolColor)
            If clr.A = 0 Then
                clr = Me.StyleGuide.FleetColorDefault(iRow - iFIRSTFLEETROW + 1, Me.m_lfiFleets.Count)
            End If
            Me(iRow, eColumnTypes.FleetColor).Value = clr
        Next iRow
        Me.AllowUpdates = True

        Me.Invalidate()

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

        Dim fi As cFleetInfo = DirectCast(Me.m_lfiFleets(p.Row - 1), cFleetInfo)

        Select Case DirectCast(p.Column, eColumnTypes)
            Case eColumnTypes.FleetIndex
                ' Not possible

            Case eColumnTypes.FleetName
                fi.Name = CStr(cell.GetValue(p))

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
            Case eColumnTypes.FleetColor
                Me.SelectCustomFleetColor(p.Row)
        End Select

    End Sub

#End Region ' Grid interaction

#Region " Row manipulation "

    Public Sub SelectFleet(ByVal fleet As cEcopathFleetInput)

        Dim fi As cFleetInfo = Nothing

        If (fleet Is Nothing) Then Return

        For iRow As Integer = iFIRSTFLEETROW To Me.RowsCount - 1
            fi = DirectCast(Me.m_lfiFleets(iRow - iFIRSTFLEETROW), cFleetInfo)
            If (ReferenceEquals(fi.FleetIndex, fleet.Index)) Then
                Me.SelectRow(iRow)
                Return
            End If
        Next iRow

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Delete a row from the grid
    ''' </summary>
    ''' <param name="iRow">The index of the row to delete.</param>
    ''' -----------------------------------------------------------------------
    Public Sub ToggleDeleteRow(Optional ByVal iRow As Integer = -1)

        If iRow = -1 Then iRow = Me.SelectedRow

        Dim iFleet As Integer = iRow - iFIRSTFLEETROW
        Dim fi As cFleetInfo = Nothing
        Dim strPrompt As String = ""

        ' Validate
        If iFleet < 0 Then Return

        fi = DirectCast(Me.m_lfiFleets(iFleet), cFleetInfo)
        ' Toggle 'flagged for deletion' flag
        fi.FlaggedForDeletion = Not fi.FlaggedForDeletion

        ' Check to see what is to happen to the Fleet now
        Select Case fi.Status

            Case eItemStatusTypes.Original
                ' Clear removed status of the Fleet
                Me.m_lfiFleetsRemoved.Remove(Me.m_lfiFleets(iFleet))

            Case eItemStatusTypes.Added
                ' Clear removed status of the Fleet
                Me.m_lfiFleetsRemoved.Remove(Me.m_lfiFleets(iFleet))

            Case eItemStatusTypes.Removed
                ' Set removed status
                Me.m_lfiFleetsRemoved.Add(Me.m_lfiFleets(iFleet))

            Case eItemStatusTypes.Invalid
                ' Set removed status
                Me.m_lfiFleets.RemoveAt(iFleet)

        End Select

        Me.UpdateGrid()

    End Sub

    ''' <summary>
    ''' States whether a row holds a fleet.
    ''' </summary>
    ''' <param name="iRow"></param>
    ''' <returns></returns>
    Public Function IsFleetRow(Optional ByVal iRow As Integer = -1) As Boolean
        If iRow = -1 Then iRow = Me.SelectedRow()
        Return (iRow >= iFIRSTFLEETROW) And (iRow < Me.RowsCount)
    End Function

    ''' <summary>
    ''' States whether the fleet on a row is flagged for deletion.
    ''' </summary>
    Public Function IsFlaggedForDeletionRow(Optional ByVal iRow As Integer = -1) As Boolean
        If iRow = -1 Then iRow = Me.SelectedRow()
        If Not IsFleetRow(iRow) Then Return False

        Dim iFleet As Integer = iRow - iFIRSTFLEETROW
        Dim fi As cFleetInfo = Nothing
        Dim strPrompt As String = ""

        fi = DirectCast(Me.m_lfiFleets(iFleet), cFleetInfo)
        Return fi.FlaggedForDeletion
    End Function

    ''' <summary>
    ''' Insert a row by creating a new fleet.
    ''' </summary>
    Public Sub InsertRow(Optional ByVal iRow As Integer = -1)
        If iRow = -1 Then iRow = Me.SelectedRow()
        If iRow = -1 Then iRow = Math.Max(iFIRSTFLEETROW, Me.RowsCount)
        If Not Me.CanInsertRow(iRow) Then Return
        Me.CreateFleet(iRow)
    End Sub

    ''' <summary>
    ''' Create a new fleet.
    ''' </summary>
    Private Sub CreateFleet(ByVal iRow As Integer)

        Dim iFleet As Integer = -1
        Dim fi As cFleetInfo = Nothing
        Dim lstrFleetNames As New List(Of String)

        ' Make fit
        iRow = Math.Max(iFIRSTFLEETROW, iRow)
        iFleet = iRow - iFIRSTFLEETROW

        ' Validate
        If iFleet < 0 Then Return

        ' Gather fleet names for generating new number
        For i As Integer = 0 To Me.m_lfiFleets.Count - 1
            lstrFleetNames.Add(Me.m_lfiFleets(i).Name)
        Next i

        fi = New cFleetInfo(cStringUtils.Localize(SharedResources.DEFAULT_NEWFLEET_NUM,
                                                  cStringUtils.GetNextNumber(lstrFleetNames.ToArray, SharedResources.DEFAULT_NEWFLEET_NUM)))
        Me.m_lfiFleets.Insert(iFleet, fi)

        Me.UpdateGrid()
        Me.SelectRow(fi)
    End Sub

    ''' <summary>
    ''' States whether a row can be inserted at the indicated position.
    ''' </summary>
    Public Function CanInsertRow(Optional ByVal iRow As Integer = -1) As Boolean
        Return True
    End Function

    ''' <summary>
    ''' Move row up, switching positions with the row above it.
    ''' </summary>
    Public Sub MoveRowUp(Optional ByVal iRow As Integer = -1)
        Dim bMoveSelection As Boolean = (iRow = -1)

        If iRow = -1 Then iRow = Me.SelectedRow()
        If Not CanMoveRowUp(iRow) Then Return
        Me.MoveRow(iRow, iRow - 1)

        If bMoveSelection Then
            Me.SelectRow(iRow - 1)
        End If
    End Sub

    ''' <summary>
    ''' States whether a row can be moved up.
    ''' </summary>
    Public Function CanMoveRowUp(Optional ByVal iRow As Integer = -1) As Boolean
        If iRow = -1 Then iRow = Me.SelectedRow()
        Return (Me.RowsCount > (iFIRSTFLEETROW + 1)) And (iRow > iFIRSTFLEETROW)
    End Function

    ''' <summary>
    ''' Move row down, switching positions with the row below it.
    ''' </summary>
    Public Sub MoveRowDown(Optional ByVal iRow As Integer = -1)
        Dim bMoveSelection As Boolean = (iRow = -1)

        If iRow = -1 Then iRow = Me.SelectedRow()
        If Not CanMoveRowDown(iRow) Then Return
        Me.MoveRow(iRow, iRow + 1)

        If bMoveSelection Then
            Me.SelectRow(iRow + 1)
        End If
    End Sub

    ''' <summary>
    ''' States whether a row can be moved down.
    ''' </summary>
    Public Function CanMoveRowDown(Optional ByVal iRow As Integer = -1) As Boolean
        If iRow = -1 Then iRow = Me.SelectedRow()
        Return (Me.RowsCount > (iFIRSTFLEETROW + 1)) And (iRow >= iFIRSTFLEETROW) And (iRow < Me.RowsCount - 1)
    End Function

    ''' <summary>
    ''' Move one row to another position.
    ''' </summary>
    Private Sub MoveRow(ByVal iFromRow As Integer, ByVal iToRow As Integer)

        Dim objTemp As cFleetInfo = Nothing
        Dim iStep As Integer = 1
        Dim iFromFleet As Integer = iFromRow - iFIRSTFLEETROW
        Dim iToFleet As Integer = iToRow - iFIRSTFLEETROW

        ' Truncate
        iFromFleet = Math.Max(0, Math.Min(Me.m_lfiFleets.Count - 1, iFromFleet))
        iToFleet = Math.Max(0, Math.Min(Me.m_lfiFleets.Count - 1, iToFleet))

        ' Nothing to do? abort
        If iFromFleet = iToFleet Then Return
        ' Determine direction of movement
        If iFromFleet < iToFleet Then iStep = 1 Else iStep = -1

        ' Swap Fleets (but do not swap the Fleet at iTo because then we've gone 1 too far)
        For iFleet As Integer = iFromFleet To iToFleet - iStep Step iStep
            objTemp = Me.m_lfiFleets(iFleet + iStep)
            Me.m_lfiFleets(iFleet + iStep) = Me.m_lfiFleets(iFleet)
            Me.m_lfiFleets(iFleet) = objTemp
            Me.UpdateRow(iFleet + iFIRSTFLEETROW)
            Me.UpdateRow(iFleet + iFIRSTFLEETROW + iStep)
        Next iFleet

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

    Private Overloads Sub SelectRow(ByVal fi As cFleetInfo)
        For iFleet As Integer = 0 To Me.m_lfiFleets.Count - 1
            If ReferenceEquals(Me.m_lfiFleets(iFleet), fi) Then
                Me.SelectRow(iFleet + iFIRSTFLEETROW)
            End If
        Next
    End Sub

#End Region ' Selection extension

#End Region ' Admin

#Region " Colors "

    Public Sub SetDefaultFleetColors()
        For iRow As Integer = iFIRSTFLEETROW To Me.RowsCount - 1
            Me.SetDefaultFleetColor(iRow)
        Next
    End Sub

    Public Sub SetDefaultFleetColor(ByVal iRow As Integer)
        Me.m_lfiFleets(iRow - iFIRSTFLEETROW).PoolColor = 0 'cStyleGuide.ColorToInt(Me.StyleGuide.GroupColorDefault(iRow - iFIRSTFLEETROW + 1, Me.m_lfiFleets.Count))
        Me.UpdateRow(iRow)
    End Sub

    Public Sub SelectCustomFleetColor(Optional ByVal iRow As Integer = -1)

        Dim fi As cFleetInfo = Nothing
        Dim dlgColor As cEwEColorDialog = Nothing

        If iRow = -1 Then iRow = Me.SelectedRow

        If Not Me.IsFleetRow(iRow) Then Return

        fi = Me.m_lfiFleets(iRow - iFIRSTFLEETROW)

        dlgColor = New cEwEColorDialog()
        dlgColor.Color = cColorUtils.IntToColor(fi.PoolColor)
        If dlgColor.ShowDialog() = DialogResult.OK Then
            fi.PoolColor = cColorUtils.ColorToInt(dlgColor.Color)
            Me.UpdateRow(iRow)
        End If

    End Sub

#End Region ' Colors

#Region " Validation "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method; validates the content of the grid.
    ''' </summary>
    ''' <returns>True when the content of the grid depicts a valid
    ''' Fleet configuration for a model.</returns>
    ''' -----------------------------------------------------------------------
    Public Function ValidateContent() As Boolean

        Return Me.ValidateNames

    End Function

    Private Function ValidateNames() As Boolean

        Dim fmsg As New cFeedbackMessage(My.Resources.PROMPT_DUPLICATE_NAMES, eCoreComponentType.External, eMessageType.DataValidation, eMessageImportance.Question, eMessageReplyStyle.YES_NO, eDataTypes.NotSet, eMessageReply.NO)
        Dim bHasDuplicates As Boolean = False
        Dim bHasBlank As Boolean = False
        Dim lstrHandled As New List(Of String)

        For Each fi As cFleetInfo In Me.m_lfiFleets
            If String.IsNullOrEmpty(fi.Name) Then
                bHasBlank = True
            ElseIf Not Me.IsNameUnique(fi.Name, fi) Then
                If Not lstrHandled.Contains(fi.Name) Then
                    fmsg.AddVariable(New cVariableStatus(eStatusFlags.FailedValidation,
                                                         cStringUtils.Localize(My.Resources.PROMPT_DUPLICATE_NAME, fi.Name),
                                                         eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, cCore.NULL_VALUE))
                    lstrHandled.Add(fi.Name)
                End If
                bHasDuplicates = True
            End If
        Next

        If bHasBlank Then
            Me.Core.Messages.SendMessage(New cMessage(My.Resources.PROMPT_EMPTY_NAMES, eMessageType.DataValidation, eCoreComponentType.External, eMessageImportance.Warning))
            Return False
        End If

        If bHasDuplicates Then
            Me.Core.Messages.SendMessage(fmsg)
            Return fmsg.Reply = eMessageReply.YES
        End If

        Return True

    End Function

    Private Function IsNameUnique(ByVal strName As String, ByVal fi As cFleetInfo) As Boolean

        ' Check if name is unique
        For i As Integer = 0 To Me.m_lfiFleets.Count - 1
            Dim fiTmp As cFleetInfo = DirectCast(Me.m_lfiFleets(i), cFleetInfo)
            ' Does name already exist?
            If (Not ReferenceEquals(fiTmp, fi)) And (String.Compare(strName, fiTmp.Name, True) = 0) Then
                ' Report failure
                Return False
            End If
        Next
        Return True

    End Function

#End Region ' Validation

#Region " Apply changes "

    Public Function Apply() As Boolean

        Dim strPrompt As String = ""
        Dim bConfigurationChanged As Boolean = False
        Dim bFleetsChanged As Boolean = False
        Dim fi As cFleetInfo = Nothing
        Dim fleet As cEcopathFleetInput = Nothing
        Dim iFleet As Integer = 0
        Dim bColorsChanged As Boolean = False
        Dim bSuccess As Boolean = True

        ' Validate content of the grid
        If Not Me.ValidateContent() Then Return False

        ' Assess Fleet changes
        For iFleet = 0 To Me.m_lfiFleets.Count - 1
            fi = DirectCast(Me.m_lfiFleets(iFleet), cFleetInfo)
            ' Check this Fleet is newly added
            If fi.IsNew Then
                bConfigurationChanged = True
            Else
                If ((iFleet + 1) <> fi.FleetIndex) Then bConfigurationChanged = True
                bFleetsChanged = bFleetsChanged Or fi.IsChanged(Me.Core.EcopathFleetInputs(fi.FleetIndex))
            End If
        Next iFleet

        ' Assess Fleets to remove
        strPrompt = ""
        For iFleet = 0 To Me.m_lfiFleetsRemoved.Count - 1
            fi = DirectCast(Me.m_lfiFleetsRemoved(iFleet), cFleetInfo)
            If (Not fi.IsNew()) Then

                strPrompt = cStringUtils.Localize(My.Resources.ECOPATH_EDITFLEET_CONFIRMDELETE_PROMPT, fi.Name)

                Dim fmsg As New cFeedbackMessage(strPrompt, eCoreComponentType.EcoPath, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO_CANCEL)
                Me.UIContext.Core.Messages.SendMessage(fmsg)

                Select Case fmsg.Reply
                    Case eMessageReply.CANCEL
                        ' Abort Apply process
                        Return False
                    Case eMessageReply.NO
                        ' Do not delete this Fleet
                        fi.Confirmed = False
                    Case eMessageReply.YES
                        ' Delete this Fleet
                        fi.Confirmed = True
                        bConfigurationChanged = True
                    Case Else
                        ' Unexpected anwer: assert
                        Debug.Assert(False)
                End Select

            End If
        Next iFleet

        ' Handle added and removed items
        If (bConfigurationChanged) Then

            If Not Me.Core.SetBatchLock(cCore.eBatchLockType.Restructure) Then Return False

            cApplicationStatusNotifier.StartProgress(Me.Core, SharedResources.GENERIC_STATUS_APPLYCHANGES)

            Dim htFleetID As New Dictionary(Of cFleetInfo, Integer)
            Dim iDBID As Integer = Nothing

            ' Add new Fleets
            For iFleet = 0 To Me.m_lfiFleets.Count - 1

                fi = DirectCast(Me.m_lfiFleets(iFleet), cFleetInfo)
                If (fi.IsNew()) Then
                    Dim igt As Integer = iFleet + 1
                    bSuccess = bSuccess And Me.Core.AddFleet(fi.Name, igt, iDBID)
                    ' Map this new ID during update
                    htFleetID.Add(fi, iDBID)
                Else
                    If ((iFleet + 1) <> fi.FleetIndex) Then
                        bSuccess = bSuccess And Me.Core.MoveFleet(fi.FleetIndex, iFleet + 1)
                    End If
                End If
            Next

            ' Remove deleted (and confirmed) Fleets
            Dim iFleetRemove As Integer = 0
            For iFleet = 0 To Me.m_lfiFleetsRemoved.Count - 1
                fi = DirectCast(Me.m_lfiFleetsRemoved(iFleetRemove), cFleetInfo)
                If (Not fi.IsNew) And (fi.Confirmed = True) Then
                    If (Me.Core.RemoveFleet(fi.FleetIndex)) Then
                        Me.m_lfiFleets.Remove(fi)
                        Me.m_lfiFleetsRemoved.Remove(fi)
                    Else
                        bSuccess = False
                        iFleetRemove += 1
                    End If
                End If
            Next

            ' The core will reload now
            Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecopath)
            cApplicationStatusNotifier.EndProgress(Me.Core)

            ' Test whether new Fleets were loaded correctly
            Debug.Assert(Me.m_lfiFleets.Count = Me.Core.nFleets, "Dialog and core out of sync on Fleets")
        End If

        ' Update core objects
        If (bFleetsChanged) Then

            ' Get (possibly changed) core fleets
            Dim dtFleets As New Dictionary(Of Integer, cEcopathFleetInput)
            For iFleet = 1 To Core.nFleets
                fleet = Me.Core.EcopathFleetInputs(iFleet)
                dtFleets(fleet.DBID) = fleet
            Next

            For iFleet = 0 To Me.m_lfiFleets.Count - 1
                fi = DirectCast(Me.m_lfiFleets(iFleet), cFleetInfo)
                If Not fi.IsNew Then
                    fleet = dtFleets(fi.FleetDBID)
                    If fi.IsChanged(fleet) Then
                        If fleet.Name <> fi.Name Then fleet.Name = fi.Name
                        If fleet.PoolColor <> fi.PoolColor Then
                            ' Is gi.poolcolor the default color? 
                            If fi.PoolColor = cColorUtils.ColorToInt(Me.StyleGuide.FleetColorDefault(fleet.Index, Me.m_lfiFleets.Count)) Then
                                ' #Yes: Set color to transparent to allow group to show up as true default colour
                                fleet.PoolColor = 0
                            Else
                                ' #No: Assign new color
                                fleet.PoolColor = fi.PoolColor
                            End If
                            bColorsChanged = True
                        End If
                    End If
                End If
            Next
            If bColorsChanged Then Me.StyleGuide.ColorsChanged()
        End If

        Return bSuccess

    End Function

#End Region ' Apply changes

End Class
