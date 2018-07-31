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
Option Explicit On

Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Threading
Imports EwECore
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports SourceGrid2.Cells

#End Region

Namespace Controls.EwEGrid

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This class provides a <see cref="SourceGrid2.Grid">SourceGrid2.Grid</see>
    ''' derived grid for displaying EwE6 data. Basic interaction and presentation
    ''' styles are defined, and key points of interaction must be overridden to
    ''' complete an customized grid.
    ''' </summary>
    ''' <example>
    ''' The following code illustrates how to create and populate a custom EwE6 grid:
    ''' <code>
    ''' Public Class BasicInputEwEGrid
    '''    : Inherits EwEGrid
    '''
    '''    Protected Overrides Sub InitStyle()
    '''    
    '''        MyBase.InitStyle()
    '''    
    '''        Me.Redim(1, 10)
    '''        Me(0, 0) = New EwEColumnHeaderCell("")
    '''        Me(0, 1) = New EwEColumnHeaderCell(My.Resources.ECOPATH_HEADER_GROUPNAME)
    '''        Me(0, 2) = New EwEColumnHeaderCell(My.Resources.HEADER_AREA)
    '''        Me(0, 3) = New EwEColumnHeaderCell(My.Resources.ECOPATH_HEADER_BIOMASSAREA)
    '''        Me.FixedColumns = 2
    '''    
    '''    End Sub
    '''    
    '''    Protected Overrides Sub FillData()
    '''    
    '''    Dim core As cCore = cCore.GetInstance()
    '''    Dim source As cCoreInputOutputBase = Nothing
    '''    
    '''       Me.Rows.Clear()
    ''' 
    '''       For groupIndex As Integer = 1 To core.nGroups
    '''           Me.Rows.Insert(groupIndex)
    '''    
    '''           source = core.EcoPathGroupInputs(groupIndex)
    '''    
    '''           Me(groupIndex, 0) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
    '''           Me(groupIndex, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
    '''           Me(groupIndex, 2) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.Area)
    '''           Me(groupIndex, 3) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.BiomassAreaInput)
    '''    
    '''       Next groupIndex
    '''    
    '''    End Sub
    '''    
    ''' End Class
    ''' </code>
    ''' </example>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public MustInherit Class EwEGrid
        Inherits SourceGrid2.Grid
        Implements IUIElement

#Region " Private helper classes "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Behaviour model that invokes key <see cref="EwEGrid">EwEGrid</see>
        ''' methods when specific cell actions are performed.
        ''' </summary>
        ''' <remarks>
        ''' Lo and behold! This class needs to be PUBLIC otherwise Sourcegrid2
        ''' will NOT be able to find it!
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Class cEwEGridBacklinkModel
            Implements BehaviorModels.IBehaviorModel

            ''' <summary>The attached grid.</summary>
            Private m_grid As EwEGrid = Nothing

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="grid"></param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal grid As EwEGrid)
                Me.m_grid = grid
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property CanReceiveFocus() As Boolean _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.CanReceiveFocus
                Get
                    Return True
                End Get
            End Property

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Event that is fired when a cell is clicked.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnClick(ByVal e As SourceGrid2.PositionEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnClick
                Me.m_grid.OnCellClicked(e.Position, e.Cell)
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnContextMenuPopUp(ByVal e As SourceGrid2.PositionContextMenuEventArgs) Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnContextMenuPopUp
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnDoubleClick(ByVal e As SourceGrid2.PositionEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnDoubleClick
                Me.m_grid.OnCellDoubleClicked(e.Position, e.Cell)
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Event that is fired when a cell showing a text box has been edited.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnEditEnded(ByVal e As SourceGrid2.PositionCancelEventArgs) Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnEditEnded
                e.Cancel = Not Me.m_grid.OnCellEdited(e.Position, e.Cell)
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnEditStarting(ByVal e As SourceGrid2.PositionCancelEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnEditStarting
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnFocusEntered(ByVal e As SourceGrid2.PositionEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnFocusEntered
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnFocusEntering(ByVal e As SourceGrid2.PositionCancelEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnFocusEntering
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnFocusLeaving(ByVal e As SourceGrid2.PositionCancelEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnFocusLeaving
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnFocusLeft(ByVal e As SourceGrid2.PositionEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnFocusLeft
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnKeyDown(ByVal e As SourceGrid2.PositionKeyEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnKeyDown
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnKeyPress(ByVal e As SourceGrid2.PositionKeyPressEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnKeyPress
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnKeyUp(ByVal e As SourceGrid2.PositionKeyEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnKeyUp
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnMouseDown(ByVal e As SourceGrid2.PositionMouseEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnMouseDown
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnMouseEnter(ByVal e As SourceGrid2.PositionEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnMouseEnter
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnMouseLeave(ByVal e As SourceGrid2.PositionEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnMouseLeave
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnMouseMove(ByVal e As SourceGrid2.PositionMouseEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnMouseMove
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnMouseUp(ByVal e As SourceGrid2.PositionMouseEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnMouseUp
                ' NOP
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Event that is fired when a non-numerical cell is changed, such as a
            ''' check box or a combo box.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Sub OnValueChanged(ByVal e As SourceGrid2.PositionEventArgs) _
                Implements SourceGrid2.BehaviorModels.IBehaviorModel.OnValueChanged
                Me.m_grid.OnCellValueChanged(e.Position, e.Cell)
            End Sub

        End Class

#End Region ' Public helper classes

#Region " Variables "

        ''' <summary>The UI context for this grid.</summary>
        Private m_uic As cUIContext = Nothing

        ''' <summary>Position event handler for trapping top-left cell clicks.</summary>
        Private m_pehTLCell As SourceGrid2.PositionEventHandler = Nothing
        ''' <summary>Cell click behaviour model.</summary>
        Private m_ceCellClick As New BehaviorModels.CustomEvents

        ''' <summary>Position event handler for trapping row header clicks.</summary>
        Private m_pehRowHeader As SourceGrid2.PositionEventHandler = Nothing
        ''' <summary>Row click behaviour model.</summary>
        Private m_ceRowSelect As New BehaviorModels.CustomEvents

        ''' <summary>Position event handler for trapping column header clicks.</summary>
        Private m_pehColHeader As SourceGrid2.PositionEventHandler = Nothing
        ''' <summary>Column click behaviour model.</summary>
        Private m_ceColSelect As New BehaviorModels.CustomEvents

        ''' <summary>List of selected properties in the grid, if any.</summary>
        Private m_lpropertySelected As New List(Of cProperty)

        ''' <summary>Flag stating to use fixed col widths and heights.</summary>
        Private m_bFixedColumnWidths As Boolean = False

        ''' <summary>Generic edit behaviour.</summary>
        Private m_bm As cEwEGridBacklinkModel = Nothing

        ''' <summary>Bin of local properties that will need disposing. Add locally crafted
        ''' formula properties and other non propertymanager-delivered properties here.</summary>
        Private m_lpropLocal As List(Of cProperty) = Nothing

        ''' <summary>Helper flag, states whether a batch cell edit is active</summary>
        Private m_bInBatchEdit As Boolean = False

#End Region ' Variables

#Region " Constructor / destructor "

        Public Sub New()
            MyBase.New()

            Me.m_pehTLCell = New SourceGrid2.PositionEventHandler(AddressOf OnSelectEntireGrid)
            AddHandler m_ceCellClick.Click, Me.m_pehTLCell
            Me.m_pehRowHeader = New SourceGrid2.PositionEventHandler(AddressOf OnSelectRow)
            AddHandler m_ceRowSelect.Click, Me.m_pehRowHeader
            Me.m_pehColHeader = New SourceGrid2.PositionEventHandler(AddressOf OnSelectColumn)
            AddHandler m_ceColSelect.Click, Me.m_pehColHeader

            AddHandler Me.Selection.ClipboardCopy, AddressOf OnClipboardCopy
            AddHandler Me.Selection.ClipboardCut, AddressOf OnClipboardCut
            AddHandler Me.Selection.ClipboardPaste, AddressOf OnClipboardPaste
            AddHandler Me.Selection.ClearCells, AddressOf OnClearCells
            AddHandler Me.Selection.SelectionChange, AddressOf OnSelectionChange

        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)

            ' JS 13Dec10: Memory leaks were discovered on tooltips. Perhaps explicitly 
            '             deactivating the grid tooltip will fix this. If not we have a 
            '             minor bug in SourceGrid.
            Me.GridToolTipActive = False

            If (Me.m_pehTLCell IsNot Nothing) Then

                RemoveHandler m_ceCellClick.Click, Me.m_pehTLCell
                Me.m_pehTLCell = Nothing
                RemoveHandler m_ceRowSelect.Click, Me.m_pehRowHeader
                Me.m_pehRowHeader = Nothing
                RemoveHandler m_ceColSelect.Click, Me.m_pehColHeader
                Me.m_pehColHeader = Nothing

                RemoveHandler Me.Selection.ClipboardCopy, AddressOf OnClipboardCopy
                RemoveHandler Me.Selection.ClipboardCut, AddressOf OnClipboardCut
                RemoveHandler Me.Selection.ClipboardPaste, AddressOf OnClipboardPaste
                RemoveHandler Me.Selection.ClearCells, AddressOf OnClearCells
                RemoveHandler Me.Selection.SelectionChange, AddressOf OnSelectionChange

                Me.TrackPropertySelection = False

            End If

            Me.UIContext = Nothing

            MyBase.Dispose(disposing)

        End Sub

#End Region ' Constructor / destructor

#Region " IUIElement implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether the grid is intended to only show output data,
        ''' and is not supposed to be used for input. This has mainly implications
        ''' for the behavior of any <see cref="cQuickEditHandler">quick-edit</see>
        ''' toolbar controls attached to the grid. Note that this setting
        ''' does not prevent that editable controls are added to the grid.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property IsOutputGrid As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cUIContext">UI Context</see> for this grid.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public Overridable Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)

                Try
                    ' Clean-up
                    If (Me.m_uic IsNot Nothing) Then
                        RemoveHandler Me.StyleGuide.StyleGuideChanged, AddressOf Me.OnStyleGuideChanged
                        Me.ClearData()
                    End If

                    ' Store UIC
                    Me.m_uic = value

                    ' Refresh when setting
                    If (Me.m_uic IsNot Nothing) Then
                        Me.RefreshContent()
                        AddHandler Me.StyleGuide.StyleGuideChanged, AddressOf Me.OnStyleGuideChanged
                    End If

                Catch ex As Exception

                End Try

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCore">core</see> that this grid connects to.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        Public ReadOnly Property Core() As cCore
            Get
                If Me.UIContext Is Nothing Then Return Nothing
                Return Me.UIContext.Core
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cStyleGuide">style guide</see> that this grid 
        ''' connects to.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        Public ReadOnly Property StyleGuide() As cStyleGuide
            Get
                If Me.UIContext Is Nothing Then Return Nothing
                Return Me.UIContext.StyleGuide
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cPropertyManager">property manager</see> that 
        ''' this grid can interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        Public ReadOnly Property PropertyManager() As cPropertyManager
            Get
                If Me.UIContext Is Nothing Then Return Nothing
                Return Me.UIContext.PropertyManager
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCommandHandler">command handler</see> that 
        ''' this grid can interact with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        Public ReadOnly Property CommandHandler() As cCommandHandler
            Get
                If Me.UIContext Is Nothing Then Return Nothing
                Return Me.UIContext.CommandHandler
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the model that implements typical EwE-style edit behaviour. Any
        ''' new cell should receive this edit handler.
        ''' </summary>
        ''' <remarks>
        ''' <para>Methods that are invoked by this editor upon cell edit are:</para>
        ''' <list type="bullet">
        ''' <item><term><see cref="OnCellClicked">OnCellClicked</see></term><description>A cell received focus</description></item>
        ''' <item><term><see cref="OnCellValueChanged">OnCellValueChanged</see></term><description>A cell value has changed, and the grid has a chance to reject or accept the change.</description></item>
        ''' <item><term><see cref="OnCellEdited">OnCellEdited</see></term><description>A cell value has been edited.</description></item>
        ''' </list>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        <Browsable(False)>
        Protected ReadOnly Property EwEEditHandler() As BehaviorModels.IBehaviorModel
            Get
                If (Me.m_bm Is Nothing) Then Me.m_bm = New cEwEGridBacklinkModel(Me)
                Return Me.m_bm
            End Get
        End Property

#End Region ' IUIElement implementation

#Region " EwE events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event that is raised whenever the selection in the grid is modified,
        ''' either via code of by user interaction.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Event OnSelectionChanged()

        Public Property IsLayoutSuspended As Boolean

        Public Overrides Sub SuspendLayoutGrid()
            MyBase.SuspendLayoutGrid()
            Me.IsLayoutSuspended = True
        End Sub

        Public Overrides Sub ResumeLayoutGrid()
            MyBase.ResumeLayoutGrid()
            Me.IsLayoutSuspended = False
        End Sub

        Protected Sub RaiseSelectionChangeEvent()
            If (Me.UIContext IsNot Nothing) And (Not Me.IsLayoutSuspended) Then
                Try
                    RaiseEvent OnSelectionChanged()
                Catch ex As Exception
                    cLog.Write(ex, "EwEGrid::RaiseSelectionChangeEvent(" & Me.ToString & ")")
                End Try
            End If
        End Sub

#End Region ' EwE events

#Region " Appearance "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to trigger the EwE process of <see cref="SourceGrid2.Grid.InitLayout">initializing</see>, 
        ''' <see cref="InitStyle">styling</see>, <see cref="FillData">populating</see> and
        ''' <see cref="FinishStyle">finalizing</see> the grid.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub InitLayout()
            MyBase.InitLayout()

            Dim bIsUIContent As Boolean = (Me.UIContext IsNot Nothing)
            Dim bIsDesigning As Boolean = (Me.DesignMode = True)
            Dim bIsLive As Boolean = bIsUIContent And Not bIsDesigning

            Try
                ' Clear grid of any remaining data
                Me.ClearData()
            Catch ex As Exception
                Debug.Assert(False, "Exception " & ex.Message & " in ClearData")
            End Try

            Try
                ' Style the grid only when designing OR fully live
                If bIsDesigning Or bIsLive Then
                    Me.InitStyle()
                End If
            Catch ex As Exception
                Debug.Assert(False, "Exception " & ex.Message & " in InitStyle: check if grid is using a missing UI context")
            End Try

            If (bIsLive) Then
                Try
                    Me.FillData()
                Catch ex As Exception
                    Debug.Assert(False, "Exception " & ex.Message & " in FillData")
                End Try
            End If

            Try
                ' Style the grid only when designing OR fully live
                If bIsDesigning Or bIsLive Then
                    Me.FinishStyle()
                End If
            Catch ex As Exception
                Debug.Assert(False, "Exception " & ex.Message & " in FinishStyle")
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Provides a grid with standard EwE appearances and behaviours.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub InitStyle()

            Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.BackColor = Color.White
            Me.FixedColumns = 2
            Me.FixedRows = 1
            Me.GridToolTipActive = True
            Me.ContextMenuStyle = SourceGrid2.ContextMenuStyle.CellContextMenu Or
                                  SourceGrid2.ContextMenuStyle.CopyPasteSelection Or
                                  SourceGrid2.ContextMenuStyle.ColumnResize Or
                                  SourceGrid2.ContextMenuStyle.AutoSize
            Me.AutoStretchRowsToFitHeight = False

            ' JS 05aug07: this flag controls whether selections can be made with cell nav keys and [ctrl] and/or [shift]
            '             It does not seem to work well though; when set to True it is impossible to select a range w
            '             [shift] and [ctrl] pressed. This is different from Excel and other grids. Let this be a known
            '             issue but let's not waste time on this issue right now.
            Me.Selection.EnableMultiSelection = True

            ' JS 06aug07: taking care of copy/paste ourselves
            Me.Selection.AutoCopyPaste = False

            Me.Selection.AutoClear = False

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Finalize the grid by formatting the grid header and column widths 
        ''' to indicated sizes after data has been provided.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub FinishStyle()

            Dim cell As ICell = Nothing

            Me.AutoSizeAll()

            'Add the selection of whole grid.
            If (Me.RowsCount > 0) And (Me.ColumnsCount > 0) Then

                cell = Me(0, 0)
                If (cell IsNot Nothing) Then cell.Behaviors.Add(Me.m_ceCellClick)

                'Add the selection of whole row while clicking first column
                For i As Integer = 1 To Me.RowsCount - 1
                    cell = Me(i, 0)
                    If (cell IsNot Nothing) Then cell.Behaviors.Add(Me.m_ceRowSelect)
                Next

                'Add the selection of whole column while clicking first row 
                For i As Integer = 1 To Me.ColumnsCount - 1
                    cell = Me(0, i)
                    If (cell IsNot Nothing) Then cell.Behaviors.Add(Me.m_ceColSelect)
                Next
            End If

            Me.FixedColumnWidths = Me.m_bFixedColumnWidths

            ' Sanity checks
            If (Me.FocusStyle <> SourceGrid2.FocusStyle.None) Then
                Console.WriteLine("Warning: grid {0} ({1}) focus style may cause problems", Me.Name, Me.GetType().FullName)
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="EwEEditHandler">EwEEditHandler</see> callback for responding
        ''' to user cell value edits. Override to respond to the notification or
        ''' to cancel the edit if the cell value is not allowed.
        ''' </summary>
        ''' <param name="p">Position that was affected.</param>
        ''' <param name="cell">Cell that was edited.</param>
        ''' <returns>True if the cell edit is allowed, false otherwise.</returns>
        ''' <remarks>
        ''' Note that this method will only be called if a cell has been given a 
        ''' <see cref="EwEEditHandler">EwEEditHandler</see> behaviour model.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Overridable Function OnCellEdited(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="EwEEditHandler">EwEEditHandler</see> callback for responding
        ''' to user cell value changes. Override to respond to the notification.
        ''' </summary>
        ''' <param name="p">Position that was affected.</param>
        ''' <param name="cell">Cell that has received a new value.</param>
        ''' <returns>The return value is ignored by the EwEGrid framework.</returns>
        ''' <remarks>
        ''' Note that this method will only be called if a cell has been given a 
        ''' <see cref="EwEEditHandler">EwEEditHandler</see> behaviour model.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Overridable Function OnCellValueChanged(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="EwEEditHandler">EwEEditHandler</see> callback for responding
        ''' to user clicks on a cell.
        ''' </summary>
        ''' <param name="p">Position that was affected.</param>
        ''' <param name="cell">Cell that was clicked.</param>
        ''' <remarks>
        ''' Note that this method will only be called if a cell has been given a 
        ''' <see cref="EwEEditHandler">EwEEditHandler</see> behaviour model.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub OnCellClicked(ByVal p As Position, ByVal cell As Cells.ICellVirtual)
            ' NOP
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="EwEEditHandler">EwEEditHandler</see> callback for responding
        ''' to user double-clicks on a cell.
        ''' </summary>
        ''' <param name="p">Position that was affected.</param>
        ''' <param name="cell">Cell that was double-clicked.</param>
        ''' <remarks>
        ''' Note that this method will only be called if a cell has been given a 
        ''' <see cref="EwEEditHandler">EwEEditHandler</see> behaviour model.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub OnCellDoubleClicked(ByVal p As Position, ByVal cell As Cells.ICellVirtual)
            ' NOP
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Flag, states whether columns are fixed in width and height. When True, 
        ''' the header row is set to a fixed height of 45 (shudder)
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), Description("States whether columns are fixed in width and height")>
        Public Property FixedColumnWidths() As Boolean
            Get
                Return m_bFixedColumnWidths
            End Get
            Set(ByVal bFixedColumnWidths As Boolean)
                'If (m_bFixedColumnWidths = bFixedColumnWidths) Then Return

                Me.m_bFixedColumnWidths = bFixedColumnWidths
                If (Me.RowsCount > 0) And (Me.ColumnsCount > 0) Then
                    If (Me.m_bFixedColumnWidths = True) Then
                        For i As Integer = 2 To Me.ColumnsCount - 1
                            Me.Columns(i).Width = 80
                        Next
                        Me.Rows(0).Height = 45
                        Me.AutoStretchColumnsToFitWidth = False
                    Else
                        Me.Columns(0).AutoSizeMode = SourceGrid2.AutoSizeMode.None
                        For i As Integer = 1 To Me.ColumnsCount - 1
                            Me.Columns(i).AutoSizeMode = (SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize)
                        Next
                        Me.Rows(0).AutoSizeMode = (SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize)
                        Me.AutoSizeAll()
                    End If
                End If
                Me.Invalidate()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Flag, states whether the grid will maintain a list of 
        ''' <see cref="SelectedProperties">selected properties</see>.
        ''' </summary>
        ''' <remarks>
        ''' It is advised to set this setting to False for larger grids.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        <Browsable(True),
         Description("States whether the grid maintains a list of selected cProperty instances."),
         DefaultValue(True)>
        Public Property TrackPropertySelection() As Boolean = True

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set a range of columns to equal size fitted to the max column width.
        ''' </summary>
        ''' <param name="iMinWidth">Min width to size to.</param>
        ''' <param name="iMaxWidth">Max width to size to.</param>
        ''' <param name="iColFrom">Start column for resizing.</param>
        ''' <param name="iColTo">End column for resizing.</param>
        ''' -------------------------------------------------------------------
        Public Sub SizeColumnsEqually(Optional ByVal iColFrom As Integer = 0,
                                      Optional ByVal iColTo As Integer = 10000,
                                      Optional ByVal iMinWidth As Integer = 10,
                                      Optional ByVal iMaxWidth As Integer = 10000)

            Dim iWidth As Integer = iMinWidth
            For i As Integer = Math.Max(0, iColFrom) To Math.Min(Me.ColumnsCount - 1, iColTo)
                iWidth = Math.Max(Me.Columns(i).Width, Math.Min(iMaxWidth, iWidth))
            Next
            For i As Integer = 2 To Me.ColumnsCount - 1
                Me.Columns(i).Width = iWidth
            Next

        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetType().ToString & "(" & Me.Name & ")"
        End Function

        ''' <summary>
        ''' Get/set the name of the data in the grid.
        ''' </summary>
        Overridable Property DataName As String = My.Resources.GENERIC_VALUE_GRID_CONTENT

#End Region ' Appearance

#Region " Data "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the entire content of the grid.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub RefreshContent()
            Me.SuspendLayoutGrid()
            Try
                Me.InitLayout()
            Catch ex As Exception

            End Try
            Me.ResumeLayoutGrid()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Override this to fill the grid with data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected MustOverride Sub FillData()

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Properly releases all EwE cells in the grid.
        ''' </summary>
        ''' <note_js>Method does not require UI context to be present.</note_js>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub ClearData()

            Me.Selection.Clear()

            ' Clear row content, smartly
            For iRow As Integer = 0 To Me.RowsCount - 1
                Me.ClearRow(iRow)
            Next

            ' Clear orphaned local properties
            If (Me.m_lpropLocal IsNot Nothing) Then
                For Each prop As cProperty In Me.m_lpropLocal
                    prop.Dispose()
                Next
                Me.m_lpropLocal.Clear()
                Me.m_lpropLocal = Nothing
            End If

            ' Remove all rows
            If Not Me.Disposing And Me.RowsCount > 0 Then
                Try
                    Me.RowsCount = 0
                Catch ex As Exception
                    ' Hmm
                End Try
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Properly releases all EwE cells in a row.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub ClearRow(ByVal iRow As Integer)
            Dim cell As SourceGrid2.Cells.ICell = Nothing
            For iCol As Integer = 0 To Me.ColumnsCount - 1
                cell = Me(iRow, iCol)
                If cell IsNot Nothing Then
                    If TypeOf (cell) Is EwECellBase Then
                        ' ..and get rid of it
                        Me(iRow, iCol) = Nothing
                        ' Clear the cell
                        DirectCast(cell, EwECellBase).Dispose()
                    End If
                End If
            Next
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a row to the grid.
        ''' </summary>
        ''' <param name="iRowIndex"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Protected Function AddRow(Optional ByVal iRowIndex As Integer = -1) As Integer
            If (-1 = iRowIndex) Then iRowIndex = Me.Rows.Count
            Me.Rows.Insert(iRowIndex)
            Return iRowIndex
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constant; returns the default text for 'value not available'.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Protected ReadOnly Property DataNotAvailable() As String
            Get
                Return My.Resources.GENERIC_VALUE_NOTAVAILABLE
            End Get
        End Property

        <Browsable(False)> _
        Public Overridable ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.NotSet
            End Get
        End Property

        ''' <summary>
        ''' Core components that grids can use to connect to the message flow
        ''' of encapsulating EwEForms.
        ''' </summary>
        <Browsable(False)> _
        Public Overridable ReadOnly Property CoreComponents() As eCoreComponentType()
            Get
                Return New eCoreComponentType() {Me.MessageSource}
            End Get
        End Property

        Public Overridable Sub OnCoreMessage(ByRef msg As cMessage)
            Try
                If (msg.Type = eMessageType.DataAddedOrRemoved) Then
                    Me.RefreshContent()
                End If
            Catch ex As Exception

            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a new local formula propery.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Protected Function Formula(ByVal exp As cExpression) As cFormulaProperty
            Dim fp As New cFormulaProperty(exp)
            Me.RegisterLocalProperty(fp)
            Return fp
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Register a <see cref="cProperty"/> that is to be automatically disposed 
        ''' when the grid no longer needs it.
        ''' </summary>
        ''' <param name="prop">The property to register.</param>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Protected Sub RegisterLocalProperty(ByVal prop As cProperty)
            If Me.m_lpropLocal Is Nothing Then
                Me.m_lpropLocal = New List(Of cProperty)
            End If
            Me.m_lpropLocal.Add(prop)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Method called when a mass change of cells is about to begin.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub BeginBatchEdit()
            Me.m_bInBatchEdit = True
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Method called when a mass change of cells is done.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub EndBatchEdit()
            Me.m_bInBatchEdit = False
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether a <see cref="BeginBatchEdit">Batch edit</see> is active.
        ''' </summary>
        ''' <returns>True if a batch edit is active, false otherwise.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsInBatchEdit() As Boolean
            Return Me.m_bInBatchEdit
        End Function

#End Region ' Data

#Region " Selection behavior "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the grid allows block selections, which occur when the
        ''' user clicks a row header, a column header or the (0, 0) cell to select
        ''' the entire grid.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
         Description("States whether the grid allows row, column and entire content selections.")> _
        Public Property AllowBlockSelect() As Boolean = True

        ' ToDo_JS 05aug07: fix [SHIFT]+key nav selection logic to select a range, not just select a cell

        Protected Overridable Sub OnSelectEntireGrid(ByVal sender As Object, ByVal e As SourceGrid2.PositionEventArgs)

            If Not Me.AllowBlockSelect Then Return

            ' JS 05aug07: no need to process keys here; shift and ctrl modifiers behave just fine
            ' JS 05aug07: on second thought: it doesn't. [SHIFT]+[CTRL] click should ADD to a selection, not replace it
            Me.Selection.AddRange(New Range(0, 0, Me.RowsCount - 1, Me.ColumnsCount - 1))

        End Sub

        Protected Overridable Sub OnSelectRow(ByVal sender As Object, ByVal e As SourceGrid2.PositionEventArgs)

            If Not Me.AllowBlockSelect Then Return

            ' JS 05aug 07: select range of rows if shift pressed
            Dim iFirstRow As Integer = e.Position.Row
            Dim iLastRow As Integer = e.Position.Row

            If ((Control.ModifierKeys And Keys.Shift) = Keys.Shift) Then
                iFirstRow = Math.Min(Selection.GetRange.Start.Row, iFirstRow)
                iLastRow = Math.Min(Selection.GetRange.End.Row, iLastRow)
            End If

            Me.Selection.AddRange(New Range(iFirstRow, 0, iLastRow, Me.ColumnsCount - 1))

        End Sub

        Protected Overridable Sub OnSelectColumn(ByVal sender As Object, ByVal e As SourceGrid2.PositionEventArgs)

            If Not Me.AllowBlockSelect Then Return

            ' JS 05aug07: select range of columns if shift pressed
            Dim iFirstCol As Integer = e.Position.Column
            Dim iLastCol As Integer = e.Position.Column

            If ((Control.ModifierKeys And Keys.Shift) = Keys.Shift) Then
                iFirstCol = Math.Min(Selection.GetRange.Start.Column, iFirstCol)
                iLastCol = Math.Min(Selection.GetRange.End.Column, iLastCol)
            End If

            Me.Selection.AddRange(New Range(0, iFirstCol, Me.RowsCount - 1, iLastCol))
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnClearCells(ByVal sender As Object, ByVal e As System.EventArgs)

            Dim cell As SourceGrid2.Cells.ICell = Nothing

            For Each pos As Position In Me.Selection.GetCellsPositions()
                cell = Me(pos.Row, pos.Column)
                If cell.DataModel IsNot Nothing Then
                    If cell.DataModel.EditableMode <> EditableMode.None And cell.DataModel.EnableEdit = True Then
                        cell.SetValue(pos, Nothing)
                    End If
                End If
            Next
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clipboard copy, implemented to return actual property cell values and
        ''' style-masked values in the clipboard text.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnClipboardCopy(ByVal sender As Object, ByVal e As System.EventArgs)

            Dim r As Range = Me.Selection.GetRange()
            Dim pos As Position = Nothing
            Dim prop As cProperty = Nothing
            Dim sbClipText As New StringBuilder
            Dim strValue As String = ""
            Dim bIgnoreSelection As Boolean = False
            Dim cell As Cells.ICell = Nothing
            Dim bHasRowData As Boolean
            Dim bHasColData As Boolean

            ' Empty or near-empty range?
            If (r.IsEmpty) Then
                ' Select remaining grid
                r = New Range(r.Start.Row, r.Start.Column, r.Start.Row, r.Start.Column)
                ' Ignore selected cells
                bIgnoreSelection = True
            End If

            bHasRowData = False
            For iRow As Integer = r.Start.Row To r.End.Row
                ' Only process visible rows (#1012)
                If Me.Rows(iRow).Visible Then

                    If bHasRowData Then sbClipText.AppendLine()

                    bHasColData = False
                    For iCol As Integer = r.Start.Column To r.End.Column
                        ' Only process visible columns (#1012)
                        If Me.Columns(iCol).Visible Then

                            pos = New Position(iRow, iCol)
                            strValue = ""

                            If bHasColData Then sbClipText.Append(cStringUtils.vbTab)

                            If (Me.Selection.Contains(pos) Or bIgnoreSelection) Then
                                cell = Me(iRow, iCol)
                                If cell IsNot Nothing Then
                                    If TypeOf cell Is PropertyCell Then
                                        prop = DirectCast(cell, PropertyCell).GetProperty()
                                        strValue = CStr(prop.GetValue(False))
                                    Else
                                        Try
                                            strValue = CStr(Me(iRow, iCol).GetValue(pos))
                                        Catch ex As InvalidCastException
                                            ' Cell value holds an object that cannot be converted to string - handle graciously
                                            strValue = ""
                                        Catch ex As Exception
                                            Debug.Assert(False, ex.Message)
                                        End Try
                                    End If
                                End If
                            End If

                            If String.Compare(strValue, CStr(cCore.NULL_VALUE)) = 0 Then strValue = ""

                            ' Add to clip text
                            sbClipText.Append(strValue)
                            bHasColData = True

                        End If
                    Next iCol

                    ' Next
                    bHasRowData = True

                End If
            Next iRow

            Dim dobj As New DataObject()
            dobj.SetData(DataFormats.Text, sbClipText.ToString())
            Clipboard.Clear()
            Clipboard.SetDataObject(dobj, True)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clipboard cut.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnClipboardCut(ByVal sender As Object, ByVal e As System.EventArgs)

            Me.OnClipboardCopy(sender, e)
            Me.OnClearCells(sender, e)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clipboard paste
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnClipboardPaste(ByVal sender As Object, ByVal e As System.EventArgs)

            Dim dtObj As IDataObject = Clipboard.GetDataObject()

            If dtObj.GetDataPresent(DataFormats.Text, True) = False Then Return

            Dim strData As String = CStr(dtObj.GetData(DataFormats.Text)).Replace(cStringUtils.vbCrLf, cStringUtils.vbLf)
            Dim astrLines() As String = strData.Split(New Char() {CChar(cStringUtils.vbCr), CChar(cStringUtils.vbLf)})
            Dim cSplit As Char = Convert.ToChar(Keys.Tab)
            Dim r As Range = Me.Selection.GetRange()
            Dim pos As Position = Nothing
            Dim cell As SourceGrid2.Cells.ICell = Nothing
            Dim iRowData As Integer = 0
            Dim iColData As Integer = 0
            Dim strValue As String = ""

            ' Empty or near-empty range?
            If (r.IsEmpty) Then
                ' Select remaining grid
                r = New Range(r.Start.Row, r.Start.Column, Me.RowsCount - r.Start.Row, Me.ColumnsCount - r.Start.Column)
            End If

            ' Diagnose dimensions of pasted data
            Dim iDY As Integer = astrLines.Length
            Dim iDX As Integer = 0

            ' Determine most likely delimiter used in text
            If (iDY > 0) Then cSplit = cStringUtils.FindStringDelimiter(astrLines(0))

            If (Me.Core IsNot Nothing) Then
                Dim fmt As New cCharFormatter()
                cLog.Write("Grid " & Me.ToString & "::OnClipboardPaste using " & fmt.GetDescriptor(cSplit), eVerboseLevel.Detailed)
            End If

            For Each strLine As String In astrLines
                ' JS 16dec12: use qualified split
                Dim astrBits As String() = cStringUtils.SplitQualified(strLine, cSplit)
                iDX = Math.Max(iDX, astrBits.Length)
            Next

            ' JS 28Feb12: added special paste behaviour
            ' - Rows can be repeated if the selected row(s) exactly fit the selected area
            ' - Columns can be repeated if the selected column(s) exactly fit the selected area
            Dim bRepeatRow As Boolean = (r.ColumnsCount Mod iDX = 0) And (iDX > 1)
            Dim bRepeatCol As Boolean = (r.RowsCount Mod iDY = 0) And (iDY > 1)
            Dim iRowFrom As Integer = r.Start.Row
            Dim iRowTo As Integer = Math.Min(If(bRepeatRow, r.End.Row, r.Start.Row + astrLines.Length - 1), Me.RowsCount - 1)
            ' Restrict paste operation to the selection area when repeating data and/or when pasting into a range
            Dim bRestrictToSelection As Boolean = bRepeatRow Or bRepeatCol Or (r.RowsCount > 1) Or (r.ColumnsCount > 1)

            If bRestrictToSelection Then iRowTo = Math.Min(iRowTo, r.End.Row)

            Me.BeginBatchEdit()

            For iRow As Integer = iRowFrom To iRowTo

                ' Only process visible rows (#1012)
                If Me.Rows(iRow).Visible Then

                    If Not String.IsNullOrEmpty(astrLines(iRowData)) Then

                        Dim astrCols() As String = astrLines(iRowData).Split(cSplit)
                        Dim iColFrom As Integer = r.Start.Column
                        Dim iColTo As Integer = Math.Min(If(bRepeatCol, r.End.Column, r.Start.Column + astrCols.Length - 1), Me.ColumnsCount - 1)
                        iColData = 0

                        If bRestrictToSelection Then iColTo = Math.Min(iColTo, r.End.Column)

                        For iCol As Integer = iColFrom To iColTo
                            ' Only process visible columns
                            If Me.Columns(iCol).Visible And (iColData < astrCols.Length) Then

                                pos = New Position(iRow, iCol)
                                cell = Me(iRow, iCol)

                                ' Prevent from crashing on irregular grids
                                ' Is there a cell?
                                If (cell IsNot Nothing) Then
                                    ' #Yes: does it have a datamodel?
                                    If (cell.DataModel IsNot Nothing) Then
                                        ' #Yes: is the cell enabled for editing?
                                        If (cell.DataModel.EnableEdit) Then
                                            ' #Yes: attempt to set value
                                            strValue = astrCols(iColData).Trim
                                            ' Is empty value?
                                            If (String.IsNullOrWhiteSpace(strValue)) And
                                                ((cell.DataModel.ValueType Is GetType(Single)) Or (cell.DataModel.ValueType Is GetType(Double)) Or (cell.DataModel.ValueType Is GetType(Integer))) Then
                                                ' #Yes: Convert to cell default value
                                                strValue = Convert.ToString(cell.DataModel.DefaultValue)
                                            End If

                                            ' Try to convert
                                            Dim objValue As Object = strValue
                                            Try
                                                If (cell.DataModel.ValueType Is GetType(Single)) Then
                                                    objValue = Single.Parse(strValue)
                                                ElseIf (cell.DataModel.ValueType Is GetType(Double)) Then
                                                    objValue = Double.Parse(strValue)
                                                ElseIf (cell.DataModel.ValueType Is GetType(Integer)) Then
                                                    objValue = Integer.Parse(strValue)
                                                End If
                                            Catch ex As Exception
                                                ' Whoah
                                                cLog.Write("Grid " & Me.ToString & "::OnClipboardPaste failed on data type " & cell.DataModel.ValueType.ToString,
                                                           eVerboseLevel.Detailed)
                                            End Try
                                            If cell.DataModel.IsValidValue(objValue) Then
                                                cell.SetValue(pos, objValue)
                                            End If

                                        End If
                                    End If
                                End If

                                ' Next column
                                iColData += 1
                                If bRepeatCol Then iColData = iColData Mod iDX

                            End If ' Column visible
                        Next iCol
                    End If

                    ' Next row
                    iRowData += 1
                    If bRepeatRow Then iRowData = iRowData Mod iDY

                End If ' Row visible
            Next iRow
            ' Redraw later
            Me.InvalidateCells()
            Me.EndBatchEdit()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Selection change event handler; implemented to fire a 
        ''' <see cref="cPropertySelectionCommand">property select command</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnSelectionChange(ByVal sender As Object, ByVal e As SourceGrid2.SelectionChangeEventArgs)

            If Me.TrackPropertySelection Then

                Dim cmdh As cCommandHandler = Me.CommandHandler
                If (cmdh IsNot Nothing) Then

                    Dim cmd As cCommand = cmdh.GetCommand(cPropertySelectionCommand.COMMAND_NAME)
                    Dim sc As cPropertySelectionCommand = Nothing

                    If cmd IsNot Nothing Then
                        ' Get properties from selected cells
                        Me.m_lpropertySelected.Clear()
                        For Each p As Position In Me.Selection.GetCellsPositions
                            Try
                                Dim c As SourceGrid2.Cells.ICell = Me(p.Row, p.Column)
                                If (c IsNot Nothing) Then
                                    ' Is property cell, but is not header?
                                    If (TypeOf c Is IPropertyCell) And Not (TypeOf c Is PropertyHeaderCell) Then
                                        ' #Yes: add to list of selected cells
                                        Me.m_lpropertySelected.Add(DirectCast(c, IPropertyCell).GetProperty())
                                    End If
                                End If
                            Catch ex As Exception

                            End Try
                        Next

                        If (TypeOf cmd Is cPropertySelectionCommand) Then
                            sc = DirectCast(cmd, cPropertySelectionCommand)
                            sc.Invoke(Me.m_lpropertySelected, e.EventType)
                        End If
                    End If
                End If
            End If

            Me.RaiseSelectionChangeEvent()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns an array of selected properties.
        ''' </summary>
        ''' <returns>An array of selected properties</returns>
        ''' <remarks>
        ''' Note that the grid will only track selected properties when
        ''' <see cref="TrackPropertySelection">TrackPropertySelection</see> is
        ''' set to <see cref="Boolean">True</see>.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Function SelectedProperties() As cProperty()
            Return m_lpropertySelected.ToArray()
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the index of the current selected row, or when the grid is in
        ''' multi-selection mode, the index of the first selected row.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function SelectedRow() As Integer

            Dim iSelectedRow As Integer = -1
            Dim selection As SourceGrid2.Selection = Me.Selection
            Dim arSelection As SourceGrid2.Range = Nothing

            If selection Is Nothing Then Return iSelectedRow
            If selection.Count = 0 Then Return iSelectedRow

            arSelection = selection.Item(0)
            iSelectedRow = arSelection.Start.Row
            Return iSelectedRow

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the indexes of all currently selected rows. 
        ''' </summary>
        ''' <returns>An array of row indexes.</returns>
        ''' -------------------------------------------------------------------
        Public Function SelectedRows() As Integer()

            Dim indexes As New List(Of Integer)
            Dim selection As SourceGrid2.Selection = Me.Selection
            If (selection IsNot Nothing) Then
                For Each ri As RowInfo In selection.SelectedRows
                    indexes.Add(ri.Index)
                Next
            End If
            Return indexes.ToArray()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the indexes of all currently selected columns.
        ''' </summary>
        ''' <returns>An array of column indexes.</returns>
        ''' -------------------------------------------------------------------
        Public Function SelectedColumns() As Integer()

            Dim indexes As New List(Of Integer)
            Dim selection As SourceGrid2.Selection = Me.Selection
            If (selection IsNot Nothing) Then
                For Each ci As ColumnInfo In selection.SelectedColumns
                    indexes.Add(ci.Index)
                Next
            End If
            Return indexes.ToArray()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set the current selected row to a specific row index.
        ''' </summary>
        ''' <param name="iRow">The index of the row to select.</param>
        ''' <remarks>
        ''' Note that this method only works when the grid supports multiple
        ''' selections. An assertion may occur otherwise.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Sub SelectRow(ByVal iRow As Integer)

            ' Clear current selection
            If (Me.Selection IsNot Nothing) Then
                Dim r As SourceGrid2.Range = Me.Selection.GetRange()
                If Not r.IsEmpty Then
                    Me.Selection.RemoveRange(r)
                End If
            End If

            If (iRow >= 0 And iRow < Me.RowsCount) Then
                Me.Selection.AddRange(New SourceGrid2.Range(iRow, 0, iRow, Me.ColumnsCount - 1))
                ' Make sure selected row is visible
                Me.ShowCell(New Position(iRow, 0))
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Populate the grid content from a stream reader.
        ''' </summary>
        ''' <param name="sr">The <see cref="StreamReader">stream reader</see> to 
        ''' read the grid content from.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' This method does not affect read-only cells, and attempts to convert 
        ''' values encountered in the stream reader to the proper cell value types. 
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Function ReadContent(ByVal sr As StreamReader) As Boolean

            Dim strLine As String = ""
            Dim astrCells As String()
            Dim cell As ICell = Nothing
            Dim cellValue As Object = Nothing
            Dim iRow As Integer = 0
            Dim iCol As Integer = 0
            Dim ci As CultureInfo = Thread.CurrentThread.CurrentUICulture
            Dim nfi As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)
            Dim cSplit As Char = ","c

            nfi.NumberDecimalSeparator = "."

            Try
                While Not sr.EndOfStream And iRow < Me.RowsCount
                    strLine = sr.ReadLine()
                    If (iRow = 0) Then
                        cSplit = cStringUtils.FindStringDelimiter(strLine)
                    End If
                    astrCells = strLine.Split(cSplit)
                    For iCol = 0 To Math.Min(Me.ColumnsCount, astrCells.Length) - 1
                        cell = Me(iRow, iCol)
                        If cell IsNot Nothing Then
                            If (cell.DataModel.EnableEdit = True) And _
                               (cell.DataModel.EditableMode <> SourceGrid2.EditableMode.None) Then
                                If (cell.DataModel.ValueType Is GetType(String)) Then
                                    cell.Value = astrCells(iCol)
                                Else
                                    astrCells(iCol) = astrCells(iCol).Trim()
                                    If String.IsNullOrEmpty(astrCells(iCol)) Then
                                        astrCells(iCol) = CStr(cCore.NULL_VALUE)
                                    End If
                                    If (cell.DataModel.ValueType Is GetType(Single)) Then
                                        ' Parse using UI default number formatting
                                        cell.Value = Single.Parse(astrCells(iCol), nfi)
                                    ElseIf (cell.DataModel.ValueType Is GetType(Double)) Then
                                        ' Parse using UI default number formatting
                                        cell.Value = Double.Parse(astrCells(iCol), nfi)
                                    ElseIf (cell.DataModel.ValueType Is GetType(Integer)) Then
                                        ' Parse using UI default number formatting
                                        cell.Value = Integer.Parse(astrCells(iCol), nfi)
                                    ElseIf (cell.DataModel.ValueType Is GetType(Boolean)) Then
                                        ' Booleans can occur as string or integer representations, 
                                        ' which both require separate conversion strategies
                                        Dim strVal As String = astrCells(iCol).Trim
                                        Dim iValTest As Integer
                                        If Integer.TryParse(strVal, iValTest) Then
                                            cell.Value = Convert.ToBoolean(iValTest)
                                        Else
                                            cell.Value = Boolean.Parse(strVal)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                    iRow += 1
                End While
            Catch ex As Exception
                cLog.Write(ex, "EwEGrid::ReadContent(" & Me.ToString & ")")
                Return False
            End Try
            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Write the content of the grid content to a stream writer. Note that
        ''' content will be written with north-american decimal separators.
        ''' </summary>
        ''' <param name="sw">The <see cref="StreamWriter">stream writer</see> to
        ''' write grid values to.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function WriteContent(ByVal sw As StreamWriter) As Boolean

            Dim cell As ICell = Nothing
            Dim cellValue As Object = Nothing
            Dim strValue As String = ""

            Try
                For iRow As Integer = 0 To Me.RowsCount - 1
                    If Me.Rows(iRow).Visible Then
                        For iCol As Integer = 0 To Me.ColumnsCount - 1
                            If Me.Columns(iCol).Visible Then
                                cell = Me(iRow, iCol)
                                If (cell IsNot Nothing) Then
                                    cellValue = cell.Value
                                    If (cellValue IsNot Nothing) Then
                                        Try
                                            If TypeOf (cellValue) Is String Then
                                                sw.Write(cStringUtils.ToCSVField(cell.DisplayText))
                                            Else
                                                strValue = cStringUtils.FormatNumber(cell.GetValue(New SourceGrid2.Position(iRow, iCol)))
                                                sw.Write(strValue)
                                            End If
                                        Catch ex As Exception
                                            ' Ignore value graciously
                                        End Try
                                    End If
                                End If
                                sw.Write(",")
                            End If
                        Next
                        sw.WriteLine()
                    End If
                Next

            Catch ex As Exception
                Return False
            End Try

            Return True

        End Function

#End Region ' Selection behavior

#Region " Updates (StyleGuide)"

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' StyleGuide change event handler; makes sure cells are redrawn.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overridable Sub OnStyleGuideChanged(ByVal changeType As cStyleGuide.eChangeType)
            Me.Invalidate(True)
        End Sub

#End Region ' Updated (StyleGuide)

#Region " Experimental "

        Public Function AddRow(values As Object(), styles As cStyleGuide.eStyleFlags()) As Boolean

            Dim bOK As Boolean = True

            If (values Is Nothing) Then Return False
            If (styles Is Nothing) Then Return False
            If (values.Length <> Me.ColumnsCount) Then Return False
            If (styles.Length <> values.Length) Then Return False

            Dim iRow As Integer = Me.AddRow()
            Dim cell As EwECellBase = Nothing

            Try
                For i As Integer = 0 To values.Length - 1
                    If (TypeOf (values(i)) Is cProperty) Then
                        Dim prop As cProperty = DirectCast(values(i), cProperty)
                        If (i < Me.FixedRows) Then
                            cell = New PropertyRowHeaderCell(prop)
                        Else
                            cell = New PropertyCell(prop)
                            cell.Style = styles(i)
                        End If
                    Else
                        If (i < Me.FixedRows) Then
                            If (values(i) IsNot Nothing) Then
                                If (TypeOf (values(i)) Is eVarNameFlags) Then
                                    cell = New EwEColumnHeaderCell(DirectCast(values(i), eVarNameFlags))
                                Else
                                    cell = New EwEColumnHeaderCell(CStr(values(i)))
                                End If
                            Else
                                cell = New EwEColumnHeaderCell("")
                            End If
                        Else
                            cell = New EwECell(values(i), values(i).GetType(), styles(i))
                            cell.Behaviors.Add(Me.EwEEditHandler)
                        End If
                    End If
                    Me(iRow, i) = cell
                Next
            Catch ex As Exception
                bOK = False
            End Try

            Return bOK

        End Function

#End Region ' Experimental

    End Class

End Namespace
