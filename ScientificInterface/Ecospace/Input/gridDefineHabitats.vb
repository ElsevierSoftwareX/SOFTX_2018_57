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
Imports EwEUtils.Utilities
Imports SourceGrid2
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwEUtils.Core

#End Region

Namespace Ecospace

    ''' <summary>
    ''' Grid catered to defining <see cref="cEcospaceHabitat">habitats</see>.
    ''' </summary>
    <CLSCompliant(False)>
    Public Class gridEditHabitats
        Inherits EwEGrid

        ''' <summary>A number representing the row that contains the first Habitat</summary>
        Private Const iFIRSTHABITATROW As Integer = 1

        ''' <summary>List of active Habitats.</summary>
        Private m_alHabitats As New List(Of cHabitatInfo)
        ''' <summary>List of removed Habitats.</summary>
        Private m_alHabitatsRemoved As New List(Of cHabitatInfo)

        ''' <summary>Update lock, used to distinguish between code updates and
        ''' user updates of grid cells. When grid cells are updated from within
        ''' the code, an update lock should be active to prevent edit/update recursion.</summary>
        Private m_iUpdateLock As Integer = 0

        ''' <summary>Enumerated type defining the columns in this grid.</summary>
        Private Enum eColumnTypes
            HabitatIndex = 0
            HabitatName
            HabitatStatus
        End Enum

#Region " Helper classes "


        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Administrative unit representing a <see cref="cEcospaceHabitat">Habitat</see>
        ''' in the EwE model.
        ''' </summary>
        ''' <remarks>
        ''' This class can represent existing and new Habitats.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Class cHabitatInfo

            ''' <summary>The status of a Habitat in the interface.</summary>
            Private m_status As eItemStatusTypes = eItemStatusTypes.Original

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Constructor, initializes a new instance of this class.
            ''' </summary>
            ''' <param name="Habitat">The <see cref="cEcospaceHabitat">cEcospaceHabitat</see> to
            ''' initialize this instance from. If set, this instance represents a
            ''' Habitat currently active in the EwE model.</param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal habitat As cEcospaceHabitat)
                Debug.Assert(habitat IsNot Nothing)
                Me.HabitatDBID = habitat.DBID
                Me.HabitatIndex = habitat.Index
                Me.Name = habitat.Name
                Me.m_status = eItemStatusTypes.Original
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Constructor, initializes a new instance of this class.
            ''' </summary>
            ''' <param name="strName">Name to assign to this administrative unit.</param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal strName As String)
                Me.Name = strName
                Me.m_status = eItemStatusTypes.Added
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set the name of this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Name() As String = ""

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="cEcospaceHabitat.DBID"/> of the habitat associated
            ''' with this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property HabitatDBID() As Integer

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="cEcospaceHabitat.Index"/> of the habitat associated
            ''' with this administrative unit.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property HabitatIndex() As Integer

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="eItemStatusTypes">item status</see>
            ''' for the habitat object.
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
            Public Property Confirmed() As Boolean = False

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' States whether the Habitat has changed.
            ''' </summary>
            ''' <returns>
            ''' True when Habitat <see cref="Name">Name</see> value has changed.
            ''' </returns>
            ''' -------------------------------------------------------------------
            Public Function IsChanged(ByVal habitat As cEcospaceHabitat) As Boolean
                If (Me.IsNew()) Then Return False
                If (habitat.DBID <> Me.HabitatDBID) Then Return False
                Return (habitat.Name <> Me.Name)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' States whether the Habitat is to be created.
            ''' </summary>
            ''' <returns>
            ''' True when Habitat <see cref="Name">Name</see> value has changed.
            ''' </returns>
            ''' -------------------------------------------------------------------
            Public Function IsNew() As Boolean
                Return (Me.HabitatDBID <= 0)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set whether this habitat is flagged for deletion. Toggling this flag
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

            ' Habitat index cell
            Me(0, eColumnTypes.HabitatIndex) = New EwEColumnHeaderCell()
            ' Habitat name cell, editable this time
            Me(0, eColumnTypes.HabitatName) = New EwEColumnHeaderCell(SharedResources.HEADER_HABITAT)

            ' Habitat index cell
            Me(0, eColumnTypes.HabitatStatus) = New EwEColumnHeaderCell(SharedResources.HEADER_STATUS)

            ' Fix index column only; Habitat name column cannot be fixed because it must be editable
            Me.FixedColumns = 1

            Me.Columns(eColumnTypes.HabitatIndex).AutoSizeMode = SourceGrid2.AutoSizeMode.None
            Me.Columns(eColumnTypes.HabitatName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.Columns(eColumnTypes.HabitatStatus).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            Me.AutoStretchColumnsToFitWidth = True

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to first create a snapshot of the Habitat/stanza configuration
        ''' in the current EwE model. The grid will be populated from this local
        ''' administration.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub FillData()

            ' Get the core reference
            Dim Habitat As cEcospaceHabitat = Nothing
            Dim hi As cHabitatInfo = Nothing

            ' Populate local administration from a snapshot of the live data

            ' Make snapshot of Habitat configuration
            ' SKIP ALL HABITAT HERE!
            For iHabitat As Integer = 1 To Me.Core.nHabitats - 1
                Habitat = Me.Core.EcospaceHabitats(iHabitat)
                hi = New cHabitatInfo(Habitat)
                Me.m_alHabitats.Add(hi)
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

            Dim hi As cHabitatInfo = Nothing
            Dim ri As RowInfo = Nothing
            Dim cells() As Cells.ICellVirtual = Nothing
            Dim pos As SourceGrid2.Position = Nothing
            Dim ewec As EwECell = Nothing

            ' Create missing rows
            For iRow As Integer = Me.Rows.Count To Me.m_alHabitats.Count
                Me.AddRow()

                ewec = New EwECell(0, GetType(Integer))
                ewec.Style = cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable
                Me(iRow, eColumnTypes.HabitatIndex) = ewec

                Me(iRow, eColumnTypes.HabitatName) = New Cells.Real.Cell("", GetType(String))
                Me(iRow, eColumnTypes.HabitatName).Behaviors.Add(Me.EwEEditHandler)

                Me(iRow, eColumnTypes.HabitatStatus) = New EwEStatusCell(eItemStatusTypes.Original)
            Next

            ' Delete obsolete rows
            While Me.Rows.Count > Me.m_alHabitats.Count + 1
                Me.Rows.Remove(Me.Rows.Count - iFIRSTHABITATROW)
            End While

            ' Sanity check whether grid can accommodate all Habitats + header
            Debug.Assert(Me.Rows.Count = Me.m_alHabitats.Count + 1)

            ' Populate rows
            For iRow As Integer = 1 To Me.m_alHabitats.Count
                UpdateRow(iRow)
            Next iRow

            'Me.AutoSizeColumn(eColumnTypes.HabitatName, 200)
            Me.StretchColumnsToFitWidth()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the content of the Row with the given index.
        ''' </summary>
        ''' <param name="iRow">The index of the row to refresh.</param>
        ''' -----------------------------------------------------------------------
        Private Sub UpdateRow(ByVal iRow As Integer)

            Dim hi As cHabitatInfo = Nothing
            Dim ri As RowInfo = Nothing
            Dim aCells() As Cells.ICellVirtual = Nothing
            Dim pos As SourceGrid2.Position = Nothing

            Me.AllowUpdates = False

            hi = DirectCast(Me.m_alHabitats(iRow - iFIRSTHABITATROW), cHabitatInfo)
            ri = Me.Rows(iRow)

            ri.Tag = hi
            aCells = ri.GetCells()

            pos = New Position(iRow, eColumnTypes.HabitatIndex)
            aCells(eColumnTypes.HabitatIndex).SetValue(pos, CInt(iRow))

            pos = New Position(iRow, eColumnTypes.HabitatName)
            aCells(eColumnTypes.HabitatName).SetValue(pos, CStr(hi.Name))

            aCells(eColumnTypes.HabitatStatus).SetValue(pos, hi.Status)

            Me.AllowUpdates = True

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Called when the user has finished editing a cell. Handled to update 
        ''' local administration based on cell value changes.
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

            Dim hi As cHabitatInfo = DirectCast(Me.m_alHabitats(p.Row - 1), cHabitatInfo)

            Select Case DirectCast(p.Column, eColumnTypes)
                Case eColumnTypes.HabitatIndex
                    ' Not possible

                Case eColumnTypes.HabitatName
                    Dim strName As String = CStr(cell.GetValue(p))
                    ' Check if name is unique
                    For iHabitat As Integer = 0 To Me.m_alHabitats.Count - 1
                        Dim giTemp As cHabitatInfo = DirectCast(Me.m_alHabitats(iHabitat), cHabitatInfo)
                        ' Does name already exist?
                        If (Not ReferenceEquals(giTemp, hi)) And (String.Compare(strName, giTemp.Name, True) = 0) Then
                            ' Change is not allowed
                            Me.UpdateRow(p.Row)
                            ' Report failure
                            Return False
                        End If
                    Next
                    ' Allow name change
                    hi.Name = strName

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

            Dim iHabitat As Integer = iRow - iFIRSTHABITATROW
            Dim hi As cHabitatInfo = Nothing
            Dim strPrompt As String = ""

            ' Validate
            If iHabitat < 0 Then Return

            hi = DirectCast(Me.m_alHabitats(iHabitat), cHabitatInfo)
            ' Toggle 'flagged for deletion' flag
            hi.FlaggedForDeletion = Not hi.FlaggedForDeletion

            ' Check to see what is to happen to the Habitat now
            Select Case hi.Status

                Case eItemStatusTypes.Original
                    ' Clear removed status of the Habitat
                    Me.m_alHabitatsRemoved.Remove(Me.m_alHabitats(iHabitat))

                Case eItemStatusTypes.Added
                    ' Clear removed status of the Habitat
                    Me.m_alHabitatsRemoved.Remove(Me.m_alHabitats(iHabitat))

                Case eItemStatusTypes.Removed
                    ' Set removed status
                    Me.m_alHabitatsRemoved.Add(Me.m_alHabitats(iHabitat))

                Case eItemStatusTypes.Invalid
                    ' Set removed status
                    Me.m_alHabitats.RemoveAt(iHabitat)

            End Select

            Me.UpdateGrid()

        End Sub

        ''' <summary>
        ''' States whether a row holds a habitat.
        ''' </summary>
        ''' <param name="iRow"></param>
        ''' <returns></returns>
        Public Function IsHabitatRow(Optional ByVal iRow As Integer = -1) As Boolean
            If iRow = -1 Then iRow = Me.SelectedRow()
            Return (iRow >= iFIRSTHABITATROW) And (iRow < Me.RowsCount)
        End Function

        ''' <summary>
        ''' States whether the habitat on a row is flagged for deletion.
        ''' </summary>
        Public Function IsFlaggedForDeletionRow(Optional ByVal iRow As Integer = -1) As Boolean
            If iRow = -1 Then iRow = Me.SelectedRow()
            If Not IsHabitatRow(iRow) Then Return False

            Dim iHabitat As Integer = iRow - iFIRSTHABITATROW
            Dim hi As cHabitatInfo = Nothing
            Dim strPrompt As String = ""

            hi = DirectCast(Me.m_alHabitats(iHabitat), cHabitatInfo)
            Return hi.FlaggedForDeletion
        End Function

        ''' <summary>
        ''' Add a row by creating a new habitat.
        ''' </summary>
        Public Sub InsertRow()
            If Not Me.CanAddRow() Then Return
            Me.CreateHabitat()
        End Sub

        ''' <summary>
        ''' Create a new habitat.
        ''' </summary>
        Private Sub CreateHabitat()
            Dim iRow As Integer = -1
            Dim iHabitat As Integer = -1
            Dim hi As cHabitatInfo = Nothing
            Dim lstrHabitats As New List(Of String)

            ' Make fit
            iRow = Math.Max(iFIRSTHABITATROW, Me.RowsCount)
            iHabitat = iRow - iFIRSTHABITATROW

            ' Validate
            If iHabitat < 0 Then Return

            ' Collect all current habitat names
            For Each hi In Me.m_alHabitats
                lstrHabitats.Add(hi.Name)
            Next

            ' Format new habitat with an auto-number value based on existing names
            hi = New cHabitatInfo(cStringUtils.Localize(SharedResources.DEFAULT_NEWHABITAT_NUM, _
                    cStringUtils.GetNextNumber(lstrHabitats.ToArray(), SharedResources.DEFAULT_NEWHABITAT_NUM)))
            Me.m_alHabitats.Insert(iHabitat, hi)

            Me.UpdateGrid()
            Me.SelectRow(hi)
        End Sub

        ''' <summary>
        ''' States whether a row can be inserted at the indicated position.
        ''' </summary>
        Public Function CanAddRow() As Boolean
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
            Return (Me.RowsCount > (iFIRSTHABITATROW + 1)) And (iRow > iFIRSTHABITATROW)
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
            Return (Me.RowsCount > (iFIRSTHABITATROW + 1)) And (iRow >= iFIRSTHABITATROW) And (iRow < Me.RowsCount - 1)
        End Function

        ''' <summary>
        ''' Move one row to another position.
        ''' </summary>
        Private Sub MoveRow(ByVal iFromRow As Integer, ByVal iToRow As Integer)

            Dim objTemp As cHabitatInfo = Nothing
            Dim iStep As Integer = 1
            Dim iFromHab As Integer = iFromRow - iFIRSTHABITATROW
            Dim iToHab As Integer = iToRow - iFIRSTHABITATROW

            ' Truncate
            iFromHab = Math.Max(0, Math.Min(Me.m_alHabitats.Count - 1, iFromHab))
            iToHab = Math.Max(0, Math.Min(Me.m_alHabitats.Count - 1, iToHab))

            ' Nothing to do? abort
            If iFromHab = iToHab Then Return
            ' Determine direction of movement
            If iFromHab < iToHab Then iStep = 1 Else iStep = -1

            ' Swap Fleets (but do not swap the Fleet at iTo because then we've gone 1 too far)
            For iHab As Integer = iFromHab To iToHab - iStep Step iStep
                objTemp = Me.m_alHabitats(iHab + iStep)
                Me.m_alHabitats(iHab + iStep) = Me.m_alHabitats(iHab)
                Me.m_alHabitats(iHab) = objTemp
                Me.UpdateRow(iHab + iFIRSTHABITATROW)
                Me.UpdateRow(iHab + iFIRSTHABITATROW + iStep)
            Next iHab

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

        Private Overloads Sub SelectRow(ByVal hi As cHabitatInfo)
            For iHabitat As Integer = 0 To Me.m_alHabitats.Count - 1
                If ReferenceEquals(Me.m_alHabitats(iHabitat), hi) Then
                    Me.SelectRow(iHabitat + iFIRSTHABITATROW)
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
        ''' Habitat configuration for a model.</returns>
        ''' -----------------------------------------------------------------------
        Public Function ValidateContent() As Boolean

            Return Me.ValidateNames

        End Function

        Private Function ValidateNames() As Boolean

            Dim fmsg As New cFeedbackMessage(My.Resources.PROMPT_DUPLICATE_NAMES, eCoreComponentType.External, eMessageType.DataValidation, eMessageImportance.Question, eMessageReplyStyle.YES_NO, eDataTypes.NotSet, eMessageReply.NO)
            Dim bHasDuplicates As Boolean = False
            Dim bHasBlank As Boolean = False
            Dim lstrHandled As New List(Of String)

            For Each hi As cHabitatInfo In Me.m_alHabitats
                If String.IsNullOrEmpty(hi.Name) Then
                    bHasBlank = True
                ElseIf Not Me.IsNameUnique(hi.Name, hi) Then
                    If Not lstrHandled.Contains(hi.Name) Then
                        fmsg.AddVariable(New cVariableStatus(eStatusFlags.FailedValidation, _
                                                             cStringUtils.Localize(My.Resources.PROMPT_DUPLICATE_NAME, hi.Name), _
                                                             eVarNameFlags.NotSet, eDataTypes.NotSet, eCoreComponentType.External, cCore.NULL_VALUE))
                        lstrHandled.Add(hi.Name)
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

        Private Function IsNameUnique(ByVal strName As String, ByVal info As cHabitatInfo) As Boolean

            ' Check if name is unique
            For i As Integer = 0 To Me.m_alHabitats.Count - 1
                Dim infoTmp As cHabitatInfo = DirectCast(Me.m_alHabitats(i), cHabitatInfo)
                ' Only compare new items
                If (infoTmp.Status <> eItemStatusTypes.Removed And info.Status <> eItemStatusTypes.Removed) Then
                    ' Does name already exist?
                    If (Not ReferenceEquals(infoTmp, info)) And (String.Compare(strName, infoTmp.Name, True) = 0) Then
                        ' Report failure
                        Return False
                    End If
                End If
            Next
            Return True

        End Function

#End Region ' Validation

#Region " Apply changes "

        Public Function Apply() As Boolean

            Dim strPrompt As String = ""
            Dim bConfigurationChanged As Boolean = False
            Dim bHabitatsChanged As Boolean = False
            Dim hi As cHabitatInfo = Nothing
            Dim iDBID As Integer = Nothing
            Dim hab As cEcospaceHabitat = Nothing
            Dim iHabitat As Integer = 0
            Dim bSuccess As Boolean = True

            ' Validate content of the grid
            If Not Me.ValidateContent() Then Return False

            ' Assess Habitat changes
            For iHabitat = 0 To Me.m_alHabitats.Count - 1
                hi = DirectCast(Me.m_alHabitats(iHabitat), cHabitatInfo)

                If hi.IsNew Then
                    bConfigurationChanged = True
                Else
                    ' Check if this habitat has been modified
                    If ((iHabitat + 1) <> hi.HabitatIndex) Then bConfigurationChanged = True
                    bHabitatsChanged = bHabitatsChanged Or hi.IsChanged(Me.Core.EcospaceHabitats(hi.HabitatIndex))
                End If
            Next iHabitat

            If (Me.m_alHabitatsRemoved.Count > 5) Then

                strPrompt = cStringUtils.Localize(My.Resources.ECOSPACE_EDITHABITAT_CONFIRMDELETENUM_PROMPT, Me.m_alHabitatsRemoved.Count)
                Dim fmsg As New cFeedbackMessage(strPrompt, eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO_CANCEL)
                Me.UIContext.Core.Messages.SendMessage(fmsg)

                Select Case fmsg.Reply
                    Case eMessageReply.CANCEL
                        ' Abort Apply process
                        Return False
                    Case eMessageReply.YES
                        ' Confirm all regions
                        For Each hi In Me.m_alHabitatsRemoved
                            hi.Confirmed = True
                        Next
                        bConfigurationChanged = True
                    Case eMessageReply.NO
                        ' NOP
                    Case Else
                        ' Unexpected answer: assert
                        Debug.Assert(False)
                End Select

            Else
                ' Assess Habitats to remove
                For iHabitat = 0 To Me.m_alHabitatsRemoved.Count - 1
                    hi = DirectCast(Me.m_alHabitatsRemoved(iHabitat), cHabitatInfo)
                    If (Not hi.IsNew()) Then

                        strPrompt = cStringUtils.Localize(My.Resources.ECOSPACE_EDITHABITAT_CONFIRMDELETE_PROMPT, hi.Name)
                        Dim fmsg As New cFeedbackMessage(strPrompt, eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO_CANCEL)
                        Me.UIContext.Core.Messages.SendMessage(fmsg)

                        Select Case fmsg.Reply
                            Case eMessageReply.CANCEL
                                ' Abort Apply process
                                Return False
                            Case eMessageReply.NO
                                ' Do not delete this Habitat
                                hi.Confirmed = False
                            Case eMessageReply.YES
                                ' Delete this Habitat
                                hi.Confirmed = True
                                bConfigurationChanged = True
                            Case Else
                                ' Unexpected answer: assert
                                Debug.Assert(False)
                        End Select

                    End If
                Next iHabitat
            End If

            ' Handle added and removed items
            If (bConfigurationChanged) Then

                If Not Me.Core.SetBatchLock(cCore.eBatchLockType.Restructure) Then Return False

                cApplicationStatusNotifier.StartProgress(Me.Core, SharedResources.GENERIC_STATUS_APPLYCHANGES)

                ' Add new Habitats
                For iHabitat = 0 To Me.m_alHabitats.Count - 1
                    hi = DirectCast(Me.m_alHabitats(iHabitat), cHabitatInfo)
                    If (hi.IsNew()) Then
                        bSuccess = bSuccess And Me.Core.AddEcospaceHabitat(hi.Name, iDBID)
                    Else
                        If ((iHabitat + 1) <> hi.HabitatIndex) Then
                            bSuccess = bSuccess And Me.Core.MoveEcospaceHabitat(hi.HabitatDBID, iHabitat + 1)
                        End If
                    End If
                Next

                ' Remove deleted (and confirmed) Habitats
                Dim iHabitatRemove As Integer = 0
                For iHabitat = 0 To Me.m_alHabitatsRemoved.Count - 1
                    hi = DirectCast(Me.m_alHabitatsRemoved(iHabitatRemove), cHabitatInfo)

                    ' Sanity check
                    Debug.Assert(Not hi.IsNew())

                    If (hi.Confirmed()) Then
                        If (Me.Core.RemoveEcospaceHabitat(hi.HabitatDBID)) Then
                            Me.m_alHabitats.Remove(hi)
                            Me.m_alHabitatsRemoved.Remove(hi)
                        Else
                            bSuccess = False
                            iHabitatRemove += 1
                        End If
                    End If
                Next

                ' The core will reload now
                Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecospace)
                cApplicationStatusNotifier.EndProgress(Me.Core)

                ' Test whether new Habitats were loaded correctly 
                ' !! taking into account that this dialog does NOT contain the All habitat, hence the '-1'
                Debug.Assert(Me.m_alHabitats.Count = (Me.Core.nHabitats - 1), ">> Internal panic: Dialog and core out of sync on Habitats")
            End If

            ' Update core objects
            If (bHabitatsChanged) Then

                ' Build quick habitat lookup
                Dim dtHabs As New Dictionary(Of Integer, cEcospaceHabitat)
                For iHabitat = 1 To Me.Core.nHabitats - 1
                    hab = Me.Core.EcospaceHabitats(iHabitat)
                    dtHabs(hab.DBID) = hab
                Next

                ' For each local habitat admin unit
                For iHabitat = 0 To Me.m_alHabitats.Count - 1
                    ' Get local admin unit
                    hi = DirectCast(Me.m_alHabitats(iHabitat), cHabitatInfo)
                    If Not hi.IsNew() Then
                        hab = dtHabs(hi.HabitatDBID)
                        ' Has it changed?
                        If (hi.IsChanged(hab)) Then
                            ' #Yes: Update
                            hab.Name = hi.Name
                        End If
                    End If
                Next
            End If

            Return bSuccess

        End Function

#End Region ' Apply changes

    End Class

End Namespace


