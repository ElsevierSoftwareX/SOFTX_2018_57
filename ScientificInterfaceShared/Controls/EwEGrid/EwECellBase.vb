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
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports SourceGrid2.Cells
Imports SourceGrid2.Cells.Real
Imports SourceGrid2.VisualModels

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Base class for EwE SourceGrid2 Grid cells, implementing EwE GUI 
    ''' feedback.
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)>
    Public MustInherit Class EwECellBase
        Inherits Cell
        Implements IEwECell
        Implements IDisposable

#Region " Private helper class "

        Class cCatchEnterPressBehaviour
            Inherits BehaviorModels.Common

            Public Overrides Sub OnKeyUp(ByVal e As SourceGrid2.PositionKeyEventArgs)
                If (e.KeyEventArgs.KeyCode = Keys.Enter) Then

                    Dim dm As DataModels.IDataModel = e.Cell.DataModel
                    Dim bAdvance As Boolean = True

                    ' Check if last edit was successful. In EwE, this can be done checking the
                    ' style of a cell: focus is not allowed to progress on a failed validation.
                    If (dm.EditableMode <> SourceGrid2.EditableMode.None) Then
                        If (TypeOf e.Cell Is EwECellBase) Then
                            Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
                            style = DirectCast(e.Cell, EwECellBase).Style
                            ' Do not advance if validation failed
                            bAdvance = (style And cStyleGuide.eStyleFlags.FailedValidation) = 0
                        End If
                    End If

                    ' Allowed to advance?
                    If bAdvance Then
                        ' #Yes: advance to next editable row
                        Dim grid As GridVirtual = e.Grid
                        Dim iRow As Integer = e.Position.Row
                        Dim p As Position = Nothing
                        Dim cell As ICellVirtual = Nothing
                        Dim bFound As Boolean = (grid.RowsCount <= grid.FixedRows)

                        While Not bFound
                            ' Next
                            iRow += 1
                            ' Past last row?
                            If iRow > grid.RowsCount Then
                                ' #Yes: switch to the first top row that is not frozen (e.g. skip headers)
                                iRow = grid.FixedRows
                            End If
                            ' Perform Mod operation as a safety catch in case the number of frozen rows is misconfigured.
                            p = New Position(iRow Mod grid.RowsCount, e.Position.Column)
                            ' Get cell
                            cell = grid.GetCell(p)
                            ' Abort if grid has insufficient cells to rotate
                            If (cell Is Nothing) Then Return

                            ' Stop searching if cell is editable OR back at start position
                            bFound = (iRow = e.Position.Row) Or (cell.DataModel.EnableEdit = True)
                        End While

                        ' Advance focus
                        grid.SetFocusCell(p)
                    End If
                End If
            End Sub

        End Class

#End Region ' Private helper class

#Region " Construction and destruction"

        ''' <summary>UI context to operate onto.</summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>Custom cell style.</summary>
        Private m_style As cStyleGuide.eStyleFlags = 0
        ''' <summary>Number of decimal digits to display</summary>
        Private m_iNumDigits As Integer = -1
        ''' <summary>Group digits by thousands.</summary>
        Private m_tsGroupDigits As TriState = TriState.UseDefault
        ''' <summary>If true, the cell will not show numerical '0' values.</summary>
        Private m_bSuppressZero As Boolean = False
        ''' <summary>Behaviour model to catch [ENTER] key presses.</summary>
        Private m_bmCatchEnter As BehaviorModels.IBehaviorModel = Nothing
        ''' <summary>Behaviour model to catch cell resize events.</summary>
        Private m_bmResize As BehaviorModels.IBehaviorModel = Nothing

        Public Sub New(ByVal objVal As Object, ByVal t As Type)
            MyBase.New(Nothing, t)

            ' Set shared visualizer
            Me.VisualModel = New cEwECellVisualizer()

            ' Configure data model, if any
            If (DataModel IsNot Nothing) Then
                Me.DataModel.AllowNull = True
                Me.DataModel.DefaultValue = cCore.NULL_VALUE
            End If

            ' Catch ENTER presses
            Me.m_bmCatchEnter = New cCatchEnterPressBehaviour()
            Me.Behaviors.Add(Me.m_bmCatchEnter)

            ' Only resize width, not height of cells
            Me.m_bmResize = New SourceGrid2.BehaviorModels.Resize(CellResizeMode.Width)
            Me.Behaviors.Add(Me.m_bmResize)

        End Sub

        Public Overridable Sub Dispose() Implements IDisposable.Dispose

            If (Me.m_uic IsNot Nothing) Then
                Me.UIContext = Nothing
            End If

            If (Me.m_bmCatchEnter IsNot Nothing) Then
                ' Remove all bahaviour models
                Me.Behaviors.Remove(Me.m_bmCatchEnter)
                Me.m_bmCatchEnter = Nothing

                Me.Behaviors.Remove(Me.m_bmResize)
                Me.m_bmResize = Nothing
            End If

            If (Me.DataModel IsNot Nothing) Then
                ' Release any editors
                Me.DataModel.EnableEdit = False
                Me.DataModel.EditableMode = SourceGrid2.EditableMode.None
                Me.DataModel = Nothing
            End If
            GC.SuppressFinalize(Me)

        End Sub

#End Region ' Construction

#Region " Data (value, style, image, pedigree) "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Allows to set a custom <see cref="cStyleGuide.eStyleFlags">style</see>,
        ''' triggering EwE colour feedback and EwE cell edit behaviour.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property Style() As cStyleGuide.eStyleFlags _
            Implements IEwECell.Style

            Get
                Return Me.m_style
            End Get

            Set(ByVal s As cStyleGuide.eStyleFlags)
                Me.m_style = s
                If (Me.DataModel IsNot Nothing) Then
                    If ((s And cStyleGuide.eStyleFlags.NotEditable) = 0) Then
                        Me.DataModel.EnableEdit = True
                        Me.DataModel.EditableMode = SourceGrid2.EditableMode.Default
                    Else
                        Me.DataModel.EnableEdit = False
                        Me.DataModel.EditableMode = SourceGrid2.EditableMode.None
                    End If
                End If
            End Set

        End Property

        Public Overrides Sub OnEditStarting(ByVal e As SourceGrid2.PositionCancelEventArgs)
            ' JS, 26aug08: Bug fix 502
            ' Safety catch, this method should be obsolete but *apparently* a double-click on
            ' disabled cells (EndableEdit and EditableMode locked down) still
            ' results into EditStarting!
            If ((Me.Style And cStyleGuide.eStyleFlags.NotEditable) = cStyleGuide.eStyleFlags.NotEditable) Then
                e.Cancel = True
            End If
            MyBase.OnEditStarting(e)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' When set to True, cells will not show numerical '0' values
        ''' </summary>
        ''' <param name="sZeroValue">A custom zero value, if applicable.</param>
        ''' -------------------------------------------------------------------
        Public Property SuppressZero(Optional ByVal sZeroValue As Single = 0.0) As Boolean
            Get
                Return Me.m_bSuppressZero
            End Get
            Set(ByVal bSuppress As Boolean)
                If (bSuppress <> Me.m_bSuppressZero) And (Me.DataModel IsNot Nothing) Then
                    Me.m_bSuppressZero = bSuppress
                    Me.DataModel.DefaultValue = sZeroValue
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the number of decimal digits to display when formatting
        ''' numeric values. Set this value to a negative number to use the 
        ''' system-wide <see cref="cStyleGuide.NumDigits">NumDigits</see> setting.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property NumDigits() As Integer
            Get
                Return Me.m_iNumDigits
            End Get
            Set(ByVal iNumDigits As Integer)
                If (iNumDigits <> Me.m_iNumDigits) Then
                    Me.m_iNumDigits = iNumDigits
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether numbers should be grouped by thousands. Set this value 
        ''' to a <see cref="TriState.UseDefault"/> to use the default dictated by
        ''' the <see cref="cStyleGuide.GroupDigits">style guide</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property GroupDigits As TriState
            Get
                Return Me.m_tsGroupDigits
            End Get
            Set(value As TriState)
                If (Me.m_tsGroupDigits <> value) Then
                    Me.m_tsGroupDigits = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the text to display in the cell.
        ''' </summary>
        ''' <returns>The formatted value of the cell.</returns>
        ''' <remarks>
        ''' Real values will be formatted according
        ''' to the <see cref="cStyleGuide.NumDigits">number of digits</see>
        ''' setting specified in the EwE <see cref="cStyleGuide">StyleGuide</see>,
        ''' or via the local <see cref="NumDigits">NumDigits</see> override
        ''' if provided.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overrides ReadOnly Property DisplayText() As String
            Get
                If (Me.DataModel Is Nothing) Then
                    Return "" ' Could smartly resolve this via type formatters etc... ugh 
                End If

                Dim objValue As Object = Me.Value
                Dim tValue As Type = Me.DataModel.ValueType

                If ((Me.Style And cStyleGuide.eStyleFlags.Null) > 0) Then
                    Return ""
                End If

                ' Is this a single?
                If (tValue Is GetType(Single)) Then
                    Dim sValue As Single = 0

                    ' JS 27Mar17: Avoid costly exception
                    If (objValue Is Nothing) Then objValue = Me.DataModel.DefaultValue

                    Try
                        ' #Yes: apply format
                        sValue = CSng(Val(objValue))
                    Catch ex As Exception

                    End Try

                    If Single.IsNaN(sValue) Then
                        Return My.Resources.GENERIC_VALUE_ERROR
                    End If

                    ' Must suppress true zero?
                    If (Me.SuppressZero And (sValue = CSng(Me.DataModel.DefaultValue))) Then
                        ' #Yes: return empty cell
                        Return ""
                    End If

                    If (Me.StyleGuide Is Nothing) Then Return My.Resources.GENERIC_VALUE_ERROR

                    Return Me.StyleGuide.FormatNumber(sValue, Me.Style, Me.m_iNumDigits, Me.m_tsGroupDigits)
                End If

                ' Is this a double?
                If (tValue Is GetType(Double)) Then
                    ' #Yes: apply format
                    Dim dValue As Double = 0

                    ' JS 27Mar17: Avoid costly exception
                    If (objValue Is Nothing) Then objValue = Me.DataModel.DefaultValue

                    Try
                        dValue = CDbl(Val(objValue))
                    Catch ex As Exception

                    End Try
                    ' Must suppress true zero?
                    If (Me.SuppressZero And (dValue = CDbl(Me.DataModel.DefaultValue))) Then
                        ' #Yes: return empty cell
                        Return ""
                    End If

                    If (Me.StyleGuide Is Nothing) Then Return "#.##"

                    Return Me.StyleGuide.FormatNumber(dValue, Me.Style, Me.m_iNumDigits, Me.m_tsGroupDigits)
                End If

                ' Is this an integer?
                If (tValue Is GetType(Integer) Or tValue Is GetType(Long)) Then
                    ' #Yes: apply format
                    Dim iValue As Integer = 0
                    Try
                        iValue = CInt(Val(objValue))
                    Catch ex As Exception

                    End Try
                    ' Must suppress true zero?
                    If (Me.SuppressZero And (iValue = CInt(Me.DataModel.DefaultValue))) Then
                        ' #Yes: return empty cell
                        Return ""
                    End If
                    Return Me.StyleGuide.FormatNumber(iValue, Me.Style, 0, Me.m_tsGroupDigits)
                End If

                ' Return value as-is
                Return CStr(objValue)

            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure a cell by examining <see cref="cVariableMetaData">variable meta data</see>.
        ''' </summary>
        ''' <param name="md">The <see cref="cVariableMetaData">variable meta data</see> to examine.</param>
        ''' -------------------------------------------------------------------
        Public Sub ConfigureCell(ByVal md As cVariableMetaData)

            ' Sanity checks
            If (md Is Nothing) Then Return
            If (Me.DataModel Is Nothing) Then Return

            ' Set default val
            Me.DataModel.DefaultValue = md.NullValue

            ' Do not set min and max; the default may be out of range and this will massively confuse the grid engine
            'Me.DataModel.MinimumValue = md.Min
            'Me.DataModel.MaximumValue = md.Max

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the image to display in the cell
        ''' </summary>
        ''' <remarks>This method provides a shortcut to the image in the 
        ''' <see cref="VisualModel"/> of the cell.</remarks>
        ''' -------------------------------------------------------------------
        Public Property Image() As Image
            Get
                If TypeOf Me.VisualModel Is VisualModels.Common Then
                    Return DirectCast(Me.VisualModel, VisualModels.Common).Image
                End If
                Return Nothing
            End Get
            Set(value As Image)
                If TypeOf Me.VisualModel Is VisualModels.Common Then
                    DirectCast(Me.VisualModel, VisualModels.Common).Image = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        Public Overridable Property Pedigree As Integer

#End Region ' Data (value, style, image, pedigree)

#Region " UIContext connection "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Connect a cell to a grid. Overridden to hook up to an existing 
        ''' <see cref="cUIContext">UI context</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnAddToGrid(ByVal e As System.EventArgs)
            MyBase.OnAddToGrid(e)
            If TypeOf Me.Grid Is IUIElement Then
                Me.UIContext = DirectCast(Me.Grid, IUIElement).UIContext
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a cell from a grid. Overridden to disconnect from the current
        ''' <see cref="cUIContext">UI context</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnRemoveToGrid(ByVal e As System.EventArgs)
            MyBase.OnRemoveToGrid(e)
            Me.UIContext = Nothing
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the UI Context to operate onto.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property UIContext() As cUIContext _
            Implements IEwECell.UIContext
            Get
                Return Me.m_uic
            End Get
            Protected Set(ByVal value As cUIContext)

                ' JS 29Nov11: dramatically improved grid performace by handling styleguide updates 
                '             in the grid instead of in the individual cells. StyleGuide changes
                '             are observed only once per grid, issuing a total invalidate which
                '             offers no loss in performance in rendering, yet tremendously reduces
                '             the amount of time needed to register and unregister change handlers
                '             per cell. Yippee.

                If (Me.m_uic IsNot Nothing) Then
                    ' Release style guide event handler
                    'RemoveHandler Me.StyleGuide.StyleGuideChanged, AddressOf Me.OnStyleGuideChanged
                End If

                Me.m_uic = value

                If (Me.m_uic IsNot Nothing) Then
                    ' Set style guide event handler
                    'AddHandler Me.StyleGuide.StyleGuideChanged, AddressOf Me.OnStyleGuideChanged
                End If

            End Set

        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a reference to the <see cref="cCore"/>, attached to the cell.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property Core() As cCore _
            Implements IEwECell.Core
            Get
                If (Me.UIContext Is Nothing) Then Return Nothing
                Return Me.UIContext.Core
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a reference to the <see cref="cPropertyManager"/>, attached to the cell.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property PropertyManager() As cPropertyManager _
            Implements IEwECell.PropertyManager
            Get
                If (Me.UIContext Is Nothing) Then Return Nothing
                Return Me.UIContext.PropertyManager
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get a reference to the <see cref="cStyleGuide"/>, attached to the cell.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property StyleGuide() As cStyleGuide _
            Implements IEwECell.StyleGuide
            Get
                If (Me.UIContext Is Nothing) Then Return Nothing
                Return Me.UIContext.StyleGuide
            End Get
        End Property

#End Region ' UIContext connection

    End Class

End Namespace