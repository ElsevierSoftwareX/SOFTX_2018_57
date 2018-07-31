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

Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecopath.Output

    <CLSCompliant(False)> _
    Public Class gridElectivity
        : Inherits EwEGrid

#Region " Helper classes "

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' A <see cref="cProperty">cProperty</see>-driven cell that reflects the 
        ''' property value by varying the colour intensity of the cell background.
        ''' </summary>
        ''' <remarks>This is a Hack'n'slash solution; no value range testing is 
        ''' performed when calculating the background colour.</remarks>
        ''' ---------------------------------------------------------------------------
        Public Class ElectivityGridCell
            : Inherits PropertyCell

#Region " Private visualizer "

            ''' ---------------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' ---------------------------------------------------------------------------
            Private Class ElectivityGridCellVisualizer
                : Inherits cEwEGridVisualizerBase

                Protected Overrides Sub DrawCell_Background(ByVal p_Cell As SourceGrid2.Cells.ICellVirtual, _
                        ByVal p_CellPosition As SourceGrid2.Position, _
                        ByVal e As System.Windows.Forms.PaintEventArgs, _
                        ByVal p_ClientRectangle As System.Drawing.Rectangle, _
                        ByVal p_Status As SourceGrid2.DrawCellStatus)

                    If (p_Status = SourceGrid2.DrawCellStatus.Normal) Then

                        If (TypeOf p_Cell Is ElectivityGridCell) Then
                            ' #Yes: obtain rich info
                            Dim cell As ElectivityGridCell = DirectCast(p_Cell, ElectivityGridCell)
                            ' Get the property
                            Dim prop As cProperty = cell.GetProperty()
                            ' Is this a property with a numerical value?
                            If TypeOf prop Is cSingleProperty Then
                                ' #Yes: get its value
                                Dim sValue As Single = CSng(prop.GetValue())
                                ' Calc back colour
                                Dim rgbColor As Integer = CInt(Math.Max(0, Math.Min(255, 255 * (1 - (1 + sValue) / 4))))
                                Dim clrBack As Color = Color.FromArgb(255, 255, rgbColor, rgbColor)
                                ' Render back colour
                                ' Draw the background
                                Using br As New SolidBrush(clrBack)
                                    e.Graphics.FillRectangle(br, p_ClientRectangle)
                                End Using
                                ' Done
                                Return
                            End If
                        End If
                    End If

                    ' Rever to default
                    MyBase.DrawCell_Background(p_Cell, p_CellPosition, e, p_ClientRectangle, p_Status)
                End Sub
            End Class

#End Region ' Private visualizer

            ''' <summary>Default visualizer for EwECells.</summary>
            Private Shared g_visualizer As New ElectivityGridCellVisualizer()

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Constructor.
            ''' </summary>
            ''' <param name="Source">The <see cref="cCoreInputOutputBase">cCoreInputOutputBase</see> data source.</param>
            ''' <param name="VarName">The <see cref="eVarNameFlags">VarName flag</see> that defines which aspect of the Source to acces.</param>
            ''' <param name="SourceSec">An optional secundary index in the VarName, or Nothing when irrelevant.</param>
            ''' -----------------------------------------------------------------------
            Public Sub New(ByVal pm As cPropertyManager, _
                           ByVal Source As cCoreInputOutputBase, _
                           ByVal VarName As eVarNameFlags, _
                           Optional ByVal SourceSec As cCoreInputOutputBase = Nothing)
                MyBase.new(pm, Source, VarName, SourceSec)
                ' Set shared visualizer
                Me.VisualModel = g_visualizer
            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Constructor.
            ''' </summary>
            ''' <param name="prop">The property to assign to the cell.</param>
            ''' -----------------------------------------------------------------------
            Public Sub New(ByVal prop As cProperty)
                ' Call baseclass constructor
                MyBase.New(prop)
                ' Set shared visualizer
                Me.VisualModel = g_visualizer
            End Sub

        End Class

#End Region ' Helper classes

        Public Sub New()
            MyBase.new()
            Me.FixedColumnWidths = False
        End Sub

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            ' Test for UI context to prevent core from being accessed
            If (Me.UIContext Is Nothing) Then Return

            Dim source As cCoreGroupBase = Nothing

            Me.Redim(core.nGroups + 1, 2)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_PREYPREDATOR)

            Dim columnIndex As Integer = 2

            For i As Integer = 1 To core.nGroups
                ' Column displays mixed consumer/producer groups ( PP < 1)
                source = core.EcoPathGroupOutputs(i)
                ' Group index header cell
                Me(i, 0) = New EwERowHeaderCell(CStr(i))
                ' # Group name row header cells
                Me(i, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)

                If source.PP < 1 Then
                    Me.Columns.Insert(columnIndex)
                    Me(0, columnIndex) = New PropertyColumnHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
                    columnIndex = columnIndex + 1
                End If

            Next

        End Sub

        Protected Overrides Sub FillData()

            Dim source As cCoreGroupBase = Nothing
            Dim sourceSec As cCoreGroupBase = Nothing
            Dim cell As PropertyCell = Nothing
            Dim columnIndex As Integer = 2

            ' For each column
            For groupIndex As Integer = 1 To core.nGroups
                ' Get the group
                source = core.EcoPathGroupOutputs(groupIndex)
                If source.PP < 1 Then
                    ' For each row
                    For rowIndex As Integer = 1 To core.nGroups
                        ' Get index group
                        sourceSec = core.EcoPathGroupOutputs(rowIndex)
                        ' Create cell
                        cell = New ElectivityGridCell(Me.PropertyManager, source, eVarNameFlags.Alpha, sourceSec)
                        ' Cells suppress zeroes to increase legibility of the grid
                        cell.SuppressZero(-1) = True
                        ' Activate the cell
                        Me(rowIndex, columnIndex) = cell
                    Next rowIndex
                    columnIndex = columnIndex + 1
                End If
            Next groupIndex

        End Sub

        Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
            Get
                Return eCoreComponentType.EcoPath
            End Get
        End Property

    End Class

End Namespace
