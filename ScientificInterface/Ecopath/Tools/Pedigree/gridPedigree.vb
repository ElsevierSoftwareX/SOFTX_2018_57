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
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SourceGrid2
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region 'Imports

Namespace Ecopath.Tools

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Grid for displaying Pedigree assignments.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Friend Class gridPedigree
        Inherits EwEGrid

#Region " Helper classes "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Visualizer for rendering pedigree cells in the lovely
        ''' <see cref="gridPedigree">pedigree grid</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Class cPedigreeCellVisualizer
            Inherits cEwEGridVisualizerBase

            Private m_psg As cPedigreeStyleGuide = Nothing

            Public Sub New(ByVal psg As cPedigreeStyleGuide)
                Me.m_psg = psg
            End Sub

            ''' <summary>
            ''' Helper method, returns a pedigree level for a given cell.
            ''' </summary>
            ''' <param name="cell">The cell to obtain pedigree info for.</param>
            ''' <param name="pos">The position to obtain pedigree info for.</param>
            ''' <returns>A <see cref="cPedigreeLevel">pedigree level</see>, or
            ''' Nothing if something went wrong.</returns>
            Private Function GetLevel(ByVal cell As SourceGrid2.Cells.ICellVirtual,
                                      ByVal pos As SourceGrid2.Position) As cPedigreeLevel

                Try

                    ' Sanity checks
                    If (cell Is Nothing) Then Return Nothing

                    Dim value As Object = cell.GetValue(pos)
                    ' Need an integer value representing a level index
                    If Not (TypeOf value Is Integer) Then Return Nothing

                    Dim core As cCore = Me.Core(cell)
                    Dim var As eVarNameFlags = core.PedigreeVariable(pos.Column - 1)
                    Dim man As cPedigreeManager = core.GetPedigreeManager(var)
                    Dim iCV As Integer = CInt(value)
                    Dim iLevel As Integer = man.PedigreeGroupLevel(pos.Row)

                    If (iCV > 0) Then Return Nothing
                    If (iLevel > 0) Then Return man.Level(iLevel)
                    Return Nothing

                Catch ex As Exception
                    ' Whoah
                End Try

                Return Nothing

            End Function

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Overidden to draw pedigree cell content background bits.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Protected Overrides Sub DrawCell_Background( _
                    ByVal cell As SourceGrid2.Cells.ICellVirtual, _
                    ByVal pos As SourceGrid2.Position, _
                    ByVal e As System.Windows.Forms.PaintEventArgs, _
                    ByVal rc As System.Drawing.Rectangle, _
                    ByVal status As SourceGrid2.DrawCellStatus)

                MyBase.DrawCell_Background(cell, pos, e, rc, status)

                Dim level As cPedigreeLevel = Me.GetLevel(cell, pos)
                If (level Is Nothing) Then Return

                Using br As New SolidBrush(Me.m_psg.BackgroundColor(Color.Transparent, level))
                    e.Graphics.FillRectangle(br, New Rectangle(rc.Left + 4, rc.Top + 3, rc.Width - 8, rc.Height - 6))
                End Using

            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Overidden to draw pedigree cell content text.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Protected Overrides Sub DrawCell_ImageAndText( _
                    ByVal cell As SourceGrid2.Cells.ICellVirtual, _
                    ByVal pos As SourceGrid2.Position, _
                    ByVal e As System.Windows.Forms.PaintEventArgs, _
                    ByVal rc As System.Drawing.Rectangle, _
                    ByVal status As SourceGrid2.DrawCellStatus)

                Dim level As cPedigreeLevel = Me.GetLevel(cell, pos)

                Dim style As cStyleGuide.eStyleFlags = 0
                Dim clrFore As Color = Me.ForeColor
                Dim clrBack As Color = Nothing ' Not used here
                Dim rcBorder As RectangleBorder = Me.Border
                Dim fontCell As Font = Me.GetCellFont()
                Dim sg As cStyleGuide = Me.StyleGuide(cell)
                Dim fmt As StringFormat = Me.StringFormat
                Dim iValueAlt As Integer = cCore.NULL_VALUE
                Dim strText As String = ""

                Dim val As Object = cell.GetValue(pos)
                If (val IsNot Nothing) Then
                    If (TypeOf val Is Integer) Then
                        iValueAlt = CInt(val)
                    End If
                End If
                strText = Me.m_psg.DisplayText(level, iValueAlt)

                ' Rendering a cell with an associated property?
                If (TypeOf cell Is EwECellBase) Then
                    ' #Yes: obtain cell style
                    style = DirectCast(cell, EwECellBase).Style()
                    If (sg IsNot Nothing) Then
                        ' Get SG colours for this style
                        sg.GetStyleColors(style, clrFore, clrBack)
                    End If
                End If

                fmt.Alignment = StringAlignment.Center
                fmt.LineAlignment = StringAlignment.Center

                ' Render Image and Text in determined fore colour and text
                Utility.PaintImageAndText(e.Graphics, rc, _
                    Me.Image, Me.ImageAlignment, Me.ImageStretch, _
                    strText, fmt, _
                    Me.AlignTextToImage, Me.Border, _
                    clrFore, Me.GetCellFont())

            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Borrow core reference from parent cell, if possible.
            ''' </summary>
            ''' <param name="cell">Cell to borrow core from.</param>
            ''' -------------------------------------------------------------------
            Protected ReadOnly Property Core(ByVal cell As SourceGrid2.Cells.ICellVirtual) As cCore
                Get
                    If (TypeOf cell Is IUIElement) Then
                        Dim uic As cUIContext = DirectCast(cell, IUIElement).UIContext
                        If (uic IsNot Nothing) Then
                            Return uic.Core
                        End If
                    End If
                    Return Nothing
                End Get
            End Property

        End Class

#End Region ' Helper classes

#Region " Private vars "

        ''' <summary>The local style guide that determines how cells are reflected.</summary>
        Private m_psg As cPedigreeStyleGuide = Nothing
        ''' <summary>The cell visualizer that renders cells in the grid.</summary>
        Private m_pcv As cPedigreeCellVisualizer = Nothing
        ''' <summary>Varname currently selected in the pedigree interface.</summary>
        Private m_varName As eVarNameFlags = eVarNameFlags.NotSet
        Private m_varCol As Integer = -1

#End Region ' Private vars

#Region " Construction "

        Public Sub New()
            MyBase.New()
            Me.FixedColumnWidths = False
        End Sub

#End Region ' Construction

#Region " Grid configuration "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cPedigreeStyleGuide">pedigree style guide</see>
        ''' to use for rendering cells.
        ''' </summary>
        ''' <remarks>
        ''' This monster has to be provided by the master UI. Only once occurrence
        ''' is used to trigger display changes throughout the pedigree interface.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property PedigreeStyleGuide() As cPedigreeStyleGuide
            Get
                Return Me.m_psg
            End Get
            Set(ByVal value As cPedigreeStyleGuide)
                If (Me.m_psg IsNot Nothing) Then
                    RemoveHandler Me.m_psg.OnRenderStyleChanged, AddressOf OnRenderStyleChanged
                End If

                Me.m_psg = value

                If (Me.m_psg IsNot Nothing) Then
                    AddHandler Me.m_psg.OnRenderStyleChanged, AddressOf OnRenderStyleChanged
                    Me.m_pcv = New cPedigreeCellVisualizer(Me.m_psg)
                    Me.RefreshContent()
                End If
            End Set
        End Property

        Public Event OnVariableChanged(ByVal sender As Object, ByVal vn As eVarNameFlags)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the variable to show. This will make only the cells for
        ''' this variable editable.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property SelectedVariable() As eVarNameFlags
            Get
                Return Me.m_varName
            End Get
            Set(ByVal value As eVarNameFlags)
                If (value <> Me.m_varName) Then
                    Me.m_varName = value
                    Me.FillData()
                    RaiseEvent OnVariableChanged(Me, Me.m_varName)
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set a value to all selected cells.
        ''' </summary>
        ''' <param name="iLevel"></param>
        ''' -------------------------------------------------------------------
        Public Sub SetLevel(ByVal iLevel As Integer)

            ' Get grid selection
            Dim sel As SourceGrid2.Selection = Me.Selection
            Dim core As cCore = Me.Core

            ' To stop a flood of updates, and to halt any conflicting operations 
            ' while we're at it.
            If Not core.SetBatchLock(cCore.eBatchLockType.Update) Then Return

            ' ToDo: can limit column iterations to selected variable
            For iCol As Integer = 2 To Me.ColumnsCount - 1
                Dim var As eVarNameFlags = Me.Core.PedigreeVariable(iCol - 1)
                Dim man As cPedigreeManager = Me.Core.GetPedigreeManager(var)
                For iRow As Integer = 1 To Me.RowsCount - 1
                    Dim pos As New SourceGrid2.Position(iRow, iCol)
                    Dim cell As SourceGrid2.Cells.ICell = Me(iRow, iCol)
                    If (cell IsNot Nothing) And Me.Selection.Contains(pos) Then
                        If TypeOf cell Is PropertyCell Then
                            Dim pcell As PropertyCell = DirectCast(cell, PropertyCell)
                            If (pcell.Style And cStyleGuide.eStyleFlags.NotEditable) = 0 Then
                                Dim prop As cProperty = pcell.GetProperty()
                                Dim bChanged As Boolean = (CInt(prop.GetValue()) <> 0) Or (man.PedigreeGroupLevel(iRow) <> iLevel)
                                man.PedigreeGroupLevel(iRow) = iLevel
                                ' Force update
                                prop.SetValue(0, If(bChanged, TriState.True, TriState.UseDefault))
                            End If
                        End If

                    End If
                Next
            Next

            core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.NotSet)

        End Sub

        Public ReadOnly Property SelectedLevel As Integer
            Get
                If (Me.m_varCol <= 0) Then Return 0

                Dim sel As SourceGrid2.Selection = Me.Selection
                Dim iValueSel As Integer = 0
                Dim iValue As Integer
                Dim bValid As Boolean = True
                Dim man As cPedigreeManager = Me.Core.GetPedigreeManager(Me.m_varName)

                For iRow As Integer = 1 To Me.RowsCount - 1
                    Dim pos As New SourceGrid2.Position(iRow, Me.m_varCol)
                    Dim cell As SourceGrid2.Cells.ICell = Me(iRow, Me.m_varCol)
                    If (cell IsNot Nothing) And Me.Selection.Contains(pos) Then
                        If TypeOf cell Is PropertyCell Then
                            Dim pcell As PropertyCell = DirectCast(cell, PropertyCell)
                            If (pcell.Style And cStyleGuide.eStyleFlags.NotEditable) = 0 Then
                                iValue = man.PedigreeGroupLevel(iRow)
                                If (iValue > 0) Then
                                    bValid = bValid And ((iValueSel = 0) Or (iValueSel = iValue))
                                    iValueSel = iValue
                                End If
                            End If
                        End If
                    End If
                Next iRow

                If bValid Then Return iValueSel Else Return 0
            End Get
        End Property

        Public ReadOnly Property SelectedCV As Integer
            Get
                If (Me.m_varCol <= 0) Then Return 0

                Dim sel As SourceGrid2.Selection = Me.Selection
                Dim iValueSel As Integer = 0
                Dim iValue As Integer
                Dim bValid As Boolean = True
                Dim man As cPedigreeManager = Me.Core.GetPedigreeManager(Me.m_varName)

                For iRow As Integer = 1 To Me.RowsCount - 1
                    Dim pos As New SourceGrid2.Position(iRow, Me.m_varCol)
                    Dim cell As SourceGrid2.Cells.ICell = Me(iRow, Me.m_varCol)
                    If (cell IsNot Nothing) And Me.Selection.Contains(pos) Then
                        If TypeOf cell Is PropertyCell Then
                            Dim pcell As PropertyCell = DirectCast(cell, PropertyCell)
                            If (pcell.Style And cStyleGuide.eStyleFlags.NotEditable) = 0 Then
                                iValue = CInt(pcell.GetProperty().GetValue)
                                If (iValue > 0) Then
                                    bValid = bValid And ((iValueSel = 0) Or (iValueSel = iValue))
                                    iValueSel = iValue
                                End If
                            End If
                        End If
                    End If
                Next iRow

                If bValid Then Return iValueSel Else Return 0
            End Get
        End Property

#End Region ' Grid configuration

#Region " Grid overrides "

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="EwEGrid.InitLayout"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub InitLayout()
            If (Me.m_psg Is Nothing) Then Return
            MyBase.InitLayout()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="EwEGrid.InitStyle"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim group As cCoreGroupBase = Nothing
            Dim cell As EwECellBase = Nothing
            Dim var As eVarNameFlags = eVarNameFlags.NotSet
            Dim descr As New cVarnameTypeFormatter()

            Me.Redim(Me.Core.nGroups + 1, Me.Core.nPedigreeVariables + 2)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)

            For iVariable As Integer = 1 To Me.Core.nPedigreeVariables
                ' Get variable
                var = Me.Core.PedigreeVariable(iVariable)
                ' Create and configure cell
                cell = New EwEColumnHeaderCell(descr.GetDescriptor(var, eDescriptorTypes.Name))
                cell.ToolTipText = descr.GetDescriptor(var, eDescriptorTypes.Description)
                cell.Behaviors.Add(Me.EwEEditHandler)
                ' Add it
                Me(0, iVariable + 1) = cell
            Next iVariable

            For iGroup As Integer = 1 To Core.nGroups
                group = Me.Core.EcoPathGroupInputs(iGroup)
                Me(iGroup, 0) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Index)
                Me(iGroup, 1) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
            Next iGroup

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="EwEGrid.FillData"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub FillData()

            Dim group As cCoreGroupBase = Nothing
            Dim man As cPedigreeManager = Nothing
            Dim prop As cProperty = Nothing
            Dim cell As PropertyCell = Nothing
            Dim style As cStyleGuide.eStyleFlags = (cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.ValueComputed)
            Dim iSelectedVar As Integer = Me.Core.PedigreeVariableIndex(Me.SelectedVariable)
            Dim varname As eVarNameFlags = eVarNameFlags.NotSet

            Me.m_varCol = -1

            ' For all pedigree variables
            For iVariable As Integer = 1 To Me.Core.nPedigreeVariables

                ' Get manager
                varname = Me.Core.PedigreeVariable(iVariable)
                man = Me.Core.GetPedigreeManager(varname)

                ' For all groups
                For iGroup As Integer = 1 To Core.nGroups
                    ' Get group
                    group = Me.Core.EcoPathGroupInputs(iGroup)

                    ' Get property
                    prop = Me.PropertyManager.GetProperty(man, eVarNameFlags.ConfidenceInterval, group)
                    ' Prepare cell
                    cell = New PropertyCell(prop)
                    ' Add EditHandler to track column selection changes
                    cell.Behaviors.Add(Me.EwEEditHandler)
                    ' Connect special pedigree cell visualizer that handles different display styles
                    cell.VisualModel = Me.m_pcv
                    ' Merge cell and property styles
                    cell.JoinStyles = True
                    cell.SuppressZero(0) = True

                    ' Apply selected variable to show only specific cells as editable
                    If iSelectedVar <> iVariable Then
                        cell.Style = cell.Style Or cStyleGuide.eStyleFlags.NotEditable
                    Else
                        Me.m_varCol = 1 + iVariable
                    End If

                    ' Store cell
                    Me(iGroup, 1 + iVariable) = cell
                Next
            Next
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="EwEGrid.FinishStyle"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()

            Me.FixedColumns = 2
            Me.FixedColumnWidths = False
            Me.SizeColumnsEqually(2)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="EwEGrid.MessageSource"/>
        ''' <summary>
        ''' Overridden to track variable changes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnCellClicked(ByVal p As SourceGrid2.Position, _
                                              ByVal cell As SourceGrid2.Cells.ICellVirtual)
            MyBase.OnCellClicked(p, cell)
            Dim iVarNew As Integer = p.Column - 1
            Me.SelectedVariable = Me.Core.PedigreeVariable(iVarNew)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="EwEGrid.MessageSource"/>
        ''' -------------------------------------------------------------------
        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Callback to redraw the grid when pedigree style guide has changed.
        ''' </summary>
        ''' <param name="psg">Maurice and his trained rodents.</param>
        ''' -------------------------------------------------------------------
        Protected Sub OnRenderStyleChanged(ByVal psg As cPedigreeStyleGuide)
            Me.FillData()
        End Sub

#End Region ' Grid overrides

    End Class

End Namespace

