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

    <CLSCompliant(False)> _
    Public Class gridEditMPA
        : Inherits EwEGrid

        ''' <summary>A number representing the row that contains the first MPA</summary>
        Private Const iFIRSTMPAROW As Integer = 1

        ''' <summary>List of active MPAs.</summary>
        Private m_alMPAs As New List(Of cMPAInfo)
        ''' <summary>List of removed MPAs.</summary>
        Private m_alMPAsRemoved As New List(Of cMPAInfo)
        ''' <summary>Update lock, used to distinguish between code updates and
        ''' user updates of grid cells. When grid cells are updated from within
        ''' the code, an update lock should be active to prevent edit/update recursion.</summary>
        Private m_iUpdateLock As Integer = 0

        ''' <summary>Enumerated type defining the columns in this grid.</summary>
        Private Enum eColumnTypes
            MPAIndex = 0
            MPAName
            MPAStatus
        End Enum

#Region " Helper classes "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Administrative unit representing a <see cref="cEcospaceMPA">MPA</see>
        ''' in the EwE model.
        ''' </summary>
        ''' <remarks>
        ''' This class can represent existing and new MPAs.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Class cMPAInfo

            Private m_iMPADBID As Integer = cCore.NULL_VALUE
            Private m_iMPAIndex As Integer = cCore.NULL_VALUE

            ''' <summary>Name for this MPA.</summary>
            Private m_strName As String = ""
            ''' <summary>The status of a MPA in the interface.</summary>
            Private m_status As eItemStatusTypes = eItemStatusTypes.Original

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Constructor, initializes a new instanze of this class.
            ''' </summary>
            ''' <param name="MPA">The <see cref="cEcospaceMPA">cEcospaceMPA</see> to
            ''' initialize this instance from. If set, this instance represents a
            ''' MPA currently active in the EwE model.</param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal MPA As cEcospaceMPA)
                Debug.Assert(MPA IsNot Nothing)
                Me.m_iMPADBID = MPA.DBID
                Me.m_iMPAIndex = MPA.Index
                Me.m_strName = MPA.Name
                Me.m_status = eItemStatusTypes.Original
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Constructor, initializes a new instanze of this class.
            ''' </summary>
            ''' <param name="strName">Name to assign to this administrative unit.</param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal strName As String)
                Me.m_strName = strName
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
            ''' Get the <see cref="cEcospaceMPA.DBID"/> of an associated MPA, if any.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property MPADBID() As Integer
                Get
                    Return Me.m_iMPADBID
                End Get
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="cEcospaceMPA.Index"/> of an associated MPA, if any.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property MPAIndex() As Integer
                Get
                    Return Me.m_iMPAIndex
                End Get
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get the <see cref="eItemStatusTypes">item status</see> for the MPA 
            ''' object.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property Status() As eItemStatusTypes
                Get
                    Return Me.m_status
                End Get
            End Property

            Public Function IsNew() As Boolean
                Return (Me.m_iMPADBID = cCore.NULL_VALUE)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' States whether the MPA has changed.
            ''' </summary>
            ''' <returns>
            ''' True when MPA <see cref="Name">Name</see> value has changed.
            ''' </returns>
            ''' -------------------------------------------------------------------
            Public Function IsChanged(ByVal mpa As cEcospaceMPA) As Boolean
                If Me.IsNew Then Return False
                Return (mpa.Name <> Me.m_strName)
            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Get/set whether this MPA is flagged for deletion. Toggling this flag
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

            ' MPA index cell
            Me(0, eColumnTypes.MPAIndex) = New EwEColumnHeaderCell()
            ' MPA name cell, editable this time
            Me(0, eColumnTypes.MPAName) = New EwEColumnHeaderCell(SharedResources.HEADER_MPA)
            ' MPA index cell
            Me(0, eColumnTypes.MPAStatus) = New EwEColumnHeaderCell(SharedResources.HEADER_STATUS)

            ' Fix index column only; MPA name column cannot be fixed because it must be editable
            Me.FixedColumns = 1

            Me.Columns(eColumnTypes.MPAIndex).AutoSizeMode = SourceGrid2.AutoSizeMode.None
            Me.Columns(eColumnTypes.MPAName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch
            Me.Columns(eColumnTypes.MPAStatus).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
            Me.AutoStretchColumnsToFitWidth = True

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to first create a snapshot of the MPA/stanza configuration
        ''' in the current EwE model. The grid will be populated from this local
        ''' administration.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub FillData()

            ' Get the core reference
            Dim MPA As cEcospaceMPA = Nothing
            Dim mi As cMPAInfo = Nothing

            ' Populate local administration from a snapshot of the live data

            ' Make snapshot of MPA configuration 
            For iMPA As Integer = 1 To Me.Core.nMPAs
                MPA = Me.Core.EcospaceMPAs(iMPA)
                mi = New cMPAInfo(MPA)
                Me.m_alMPAs.Add(mi)
            Next

            ' Brute-force update grid
            UpdateGrid()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Finish the style
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.AutoSizeColumn(eColumnTypes.MPAIndex, 40)
            Me.AutoSizeColumn(eColumnTypes.MPAStatus, 80)
            Me.StretchColumnsToFitWidth()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Brute-force resize the gird if necessary, and repopulate with data from 
        ''' the local administration.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub UpdateGrid()

            Dim mi As cMPAInfo = Nothing
            Dim ri As RowInfo = Nothing
            Dim cells() As Cells.ICellVirtual = Nothing
            Dim pos As SourceGrid2.Position = Nothing
            Dim ewec As EwECell = Nothing

            ' Create missing rows
            For iRow As Integer = Me.Rows.Count To Me.m_alMPAs.Count
                Me.AddRow()

                ewec = New EwECell(0, GetType(Integer))
                ewec.Style = cStyleGuide.eStyleFlags.Names Or cStyleGuide.eStyleFlags.NotEditable
                Me(iRow, eColumnTypes.MPAIndex) = ewec

                Me(iRow, eColumnTypes.MPAName) = New Cells.Real.Cell("", GetType(String))
                Me(iRow, eColumnTypes.MPAName).Behaviors.Add(Me.EwEEditHandler)

                Me(iRow, eColumnTypes.MPAStatus) = New EwEStatusCell(eItemStatusTypes.Original)
            Next

            ' Delete obsolete rows
            While Me.Rows.Count > Me.m_alMPAs.Count + 1
                Me.Rows.Remove(Me.Rows.Count - iFIRSTMPAROW)
            End While

            ' Sanity check whether grid can accomodate all MPAs + header
            Debug.Assert(Me.Rows.Count = Me.m_alMPAs.Count + 1)

            ' Populate rows
            For iRow As Integer = 1 To Me.m_alMPAs.Count
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

            Dim mi As cMPAInfo = Nothing
            Dim ri As RowInfo = Nothing
            Dim aCells() As Cells.ICellVirtual = Nothing
            Dim pos As SourceGrid2.Position = Nothing
            Dim vm As VisualModels.IVisualModel = Nothing
            Dim strText As String = ""
            Dim iNumOpen As Integer = 0

            Me.AllowUpdates = False

            mi = DirectCast(Me.m_alMPAs(iRow - iFIRSTMPAROW), cMPAInfo)
            ri = Me.Rows(iRow)

            ri.Tag = mi
            aCells = ri.GetCells()

            ' Set index
            pos = New Position(iRow, eColumnTypes.MPAIndex)
            aCells(eColumnTypes.MPAIndex).SetValue(pos, CInt(iRow))

            ' Set name
            pos = New Position(iRow, eColumnTypes.MPAName)
            aCells(eColumnTypes.MPAName).SetValue(pos, CStr(mi.Name))

            aCells(eColumnTypes.MPAStatus).SetValue(pos, mi.Status)

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

            Dim mi As cMPAInfo = DirectCast(Me.m_alMPAs(p.Row - 1), cMPAInfo)

            Select Case DirectCast(p.Column, eColumnTypes)
                Case eColumnTypes.MPAIndex
                    ' Not possible

                Case eColumnTypes.MPAName
                    Dim strName As String = CStr(cell.GetValue(p))
                    ' Check if name is unique
                    For iMPA As Integer = 0 To Me.m_alMPAs.Count - 1
                        Dim giTemp As cMPAInfo = DirectCast(Me.m_alMPAs(iMPA), cMPAInfo)
                        ' Does name already exist?
                        If (Not ReferenceEquals(giTemp, mi)) And (String.Compare(strName, giTemp.Name, True) = 0) Then
                            ' Change is not allowed
                            Me.UpdateRow(p.Row)
                            ' Report failure
                            Return False
                        End If
                    Next
                    ' Allow name change
                    mi.Name = strName

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

            Dim iMPA As Integer = iRow - iFIRSTMPAROW
            Dim mi As cMPAInfo = Nothing

            ' Validate
            If iMPA < 0 Then Return

            mi = DirectCast(Me.m_alMPAs(iMPA), cMPAInfo)
            ' Toggle 'flagged for deletion' flag
            mi.FlaggedForDeletion = Not mi.FlaggedForDeletion

            ' Check to see what is to happen to the MPA now
            Select Case mi.Status

                Case eItemStatusTypes.Original
                    ' Clear removed status of the MPA
                    Me.m_alMPAsRemoved.Remove(Me.m_alMPAs(iMPA))

                Case eItemStatusTypes.Added
                    ' Clear removed status of the MPA
                    Me.m_alMPAsRemoved.Remove(Me.m_alMPAs(iMPA))

                Case eItemStatusTypes.Removed
                    ' Set removed status
                    Me.m_alMPAsRemoved.Add(Me.m_alMPAs(iMPA))

                Case eItemStatusTypes.Invalid
                    ' Set removed status
                    Me.m_alMPAs.RemoveAt(iMPA)

            End Select

            Me.UpdateGrid()

        End Sub

        ''' <summary>
        ''' States whether a row holds a MPA.
        ''' </summary>
        ''' <param name="iRow"></param>
        ''' <returns></returns>
        Public Function IsMPARow(Optional ByVal iRow As Integer = -1) As Boolean
            If iRow = -1 Then iRow = Me.SelectedRow()
            Return (iRow >= iFIRSTMPAROW) And (iRow < Me.RowsCount)
        End Function

        ''' <summary>
        ''' States whether the MPA on a row is flagged for deletion.
        ''' </summary>
        Public Function IsFlaggedForDeletionRow(Optional ByVal iRow As Integer = -1) As Boolean
            If iRow = -1 Then iRow = Me.SelectedRow()
            If Not IsMPARow(iRow) Then Return False

            Dim iMPA As Integer = iRow - iFIRSTMPAROW
            Dim mi As cMPAInfo = DirectCast(Me.m_alMPAs(iMPA), cMPAInfo)

            Return mi.FlaggedForDeletion
        End Function

        ''' <summary>
        ''' Add a row by creating a new MPA.
        ''' </summary>
        Public Sub InsertRow()
            If Not Me.CanAddRow() Then Return
            Me.CreateMPA()
        End Sub

        ''' <summary>
        ''' Create a new MPA.
        ''' </summary>
        Private Sub CreateMPA()
            Dim iRow As Integer = -1
            Dim iMPA As Integer = -1
            Dim mi As cMPAInfo = Nothing
            Dim lstrMPAs As New List(Of String)

            ' Make fit
            iRow = Math.Max(iFIRSTMPAROW, Me.RowsCount)
            iMPA = iRow - iFIRSTMPAROW

            ' Validate
            If iMPA < 0 Then Return

            ' Collect all current MPA names
            For Each mi In Me.m_alMPAs
                lstrMPAs.Add(mi.Name)
            Next

            mi = New cMPAInfo(cStringUtils.Localize(SharedResources.DEFAULT_NEWMPA_NUM, _
                    cStringUtils.GetNextNumber(lstrMPAs.ToArray(), SharedResources.DEFAULT_NEWMPA_NUM)))
            Me.m_alMPAs.Insert(iMPA, mi)

            Me.UpdateGrid()
            Me.SelectRow(mi)
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

        Private Overloads Sub SelectRow(ByVal mi As cMPAInfo)
            For iMPA As Integer = 0 To Me.m_alMPAs.Count - 1
                If ReferenceEquals(Me.m_alMPAs(iMPA), mi) Then
                    Me.SelectRow(iMPA + iFIRSTMPAROW)
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

            For Each hi As cMPAInfo In Me.m_alMPAs
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

        Private Function IsNameUnique(ByVal strName As String, ByVal info As cMPAInfo) As Boolean

            ' Check if name is unique
            For i As Integer = 0 To Me.m_alMPAs.Count - 1
                Dim infoTmp As cMPAInfo = DirectCast(Me.m_alMPAs(i), cMPAInfo)
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
            Dim bMPAsChanged As Boolean = False
            Dim mi As cMPAInfo = Nothing
            Dim iDBID As Integer = Nothing
            Dim mpa As cEcospaceMPA = Nothing
            Dim iMPA As Integer = 0
            Dim iDeleteCount As Integer = 0
            Dim MPAMonths(cCore.N_MONTHS) As Boolean
            Dim bSuccess As Boolean = True

            For i As Integer = 1 To cCore.N_MONTHS
                MPAMonths(i) = False
            Next

            ' Validate content of the grid
            If Not Me.ValidateContent() Then Return False

            ' Assess MPA changes
            For iMPA = 0 To Me.m_alMPAs.Count - 1
                mi = DirectCast(Me.m_alMPAs(iMPA), cMPAInfo)
                If mi.IsNew() Then
                    bConfigurationChanged = True
                Else
                    bMPAsChanged = bMPAsChanged Or mi.IsChanged(Me.Core.EcospaceMPAs(mi.MPAIndex))
                End If
            Next iMPA

            ' Assess MPAs to remove
            iDeleteCount = 0
            For iMPA = 0 To Me.m_alMPAsRemoved.Count - 1
                mi = DirectCast(Me.m_alMPAsRemoved(iMPA), cMPAInfo)
                If (Not mi.IsNew()) Then iDeleteCount += 1
            Next iMPA

            If (iDeleteCount > 0) Then

                strPrompt = cStringUtils.Localize(My.Resources.ECOSPACE_EDITMPA_CONFIRMDELETE_PROMPT, iDeleteCount)
                Dim fmsg As New cFeedbackMessage(strPrompt, eCoreComponentType.Core, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO_CANCEL)
                Me.UIContext.Core.Messages.SendMessage(fmsg)

                Select Case fmsg.Reply
                    Case eMessageReply.CANCEL
                        ' Abort Apply process
                        Return False
                    Case eMessageReply.NO
                        ' Do not delete MPAs
                        iDeleteCount = 0
                    Case eMessageReply.YES
                        ' Delete MPAs
                        bConfigurationChanged = True
                    Case Else
                        ' Unexpected anwer: assert
                        Debug.Assert(False)
                End Select

            End If

            ' Handle added and removed items
            If (bConfigurationChanged) Then

                If Not Me.Core.SetBatchLock(cCore.eBatchLockType.Restructure) Then Return False
                cApplicationStatusNotifier.StartProgress(Me.Core, SharedResources.GENERIC_STATUS_APPLYCHANGES)

                ' Add new MPAs
                For iMPA = 0 To Me.m_alMPAs.Count - 1
                    mi = DirectCast(Me.m_alMPAs(iMPA), cMPAInfo)
                    If (mi.IsNew()) Then
                        bSuccess = bSuccess And Me.Core.AddEcospaceMPA(mi.Name, MPAMonths, iDBID)
                    End If
                Next

                ' Remove MPAs
                If iDeleteCount > 0 Then
                    For iMPA = 0 To Me.m_alMPAsRemoved.Count - 1
                        mi = DirectCast(Me.m_alMPAsRemoved(iMPA), cMPAInfo)
                        If (Not mi.IsNew()) Then
                            If (Me.Core.RemoveEcospaceMPA(mi.MPADBID)) Then
                                Me.m_alMPAs.Remove(mi)
                            Else
                                bSuccess = False
                            End If
                        End If
                    Next iMPA
                    If bSuccess Then Me.m_alMPAsRemoved.Clear()
                End If

                ' The core will reload now
                If bSuccess Then
                    bSuccess = Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecospace, True)
                    ' Test whether new MPAs were loaded correctly
                    Debug.Assert(Me.m_alMPAs.Count = Me.Core.nMPAs, ">> Internal panic: Dialog and core out of sync on MPAs")
                Else
                    Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecospace)
                End If
                cApplicationStatusNotifier.EndProgress(Me.Core)
            End If

            ' Update core objects
            If (bMPAsChanged) Then

                ' Build quick local lookup for locating MPAs by DBID
                Dim dtMPAs As New Dictionary(Of Integer, cEcospaceMPA)
                For iMPA = 1 To Me.Core.nMPAs
                    mpa = Me.Core.EcospaceMPAs(iMPA)
                    dtMPAs(mpa.DBID) = mpa
                Next

                ' For each local MPA admin unit
                For iMPA = 0 To Me.m_alMPAs.Count - 1
                    ' Get local admin unit
                    mi = DirectCast(Me.m_alMPAs(iMPA), cMPAInfo)
                    ' Is associated w existing MPA, e.g. could be changed?
                    If Not mi.IsNew Then
                        ' Get MPA
                        mpa = dtMPAs(mi.MPADBID)
                        ' Has user changed the MPA?
                        If mi.IsChanged(mpa) Then
                            ' #Yes: update MPA
                            mpa.Name = mi.Name
                        End If
                    End If
                Next
            End If

            Return bSuccess

        End Function


#End Region ' Apply changes

    End Class

End Namespace
