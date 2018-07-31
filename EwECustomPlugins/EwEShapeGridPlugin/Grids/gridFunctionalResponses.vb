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
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Grid for showing mediation shapes that interact on habitat capacity.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class gridFunctionalResponses
    Inherits gridMediation

    Private m_handler As cCapacityShapeGUIHandler = Nothing

    ''' <summary>Rows in the grid</summary>
    Protected Shadows Enum eRowType As Integer
        Header = 0
        Thumbnail
        Name
        LimLeft
        LimRight
        'LimMean
        FirstTime
    End Enum

    Public Sub New()
        MyBase.New()
    End Sub

    Public Overrides ReadOnly Property Handler() As ScientificInterfaceShared.Controls.cShapeGUIHandler
        Get
            If (Me.m_handler Is Nothing) Then
                Me.m_handler = New cCapacityShapeGUIHandler(Me.UIContext)
            End If
            Return Me.m_handler
        End Get
    End Property

    Public Overrides ReadOnly Property Manager() As System.Collections.IEnumerable
        Get
            Return Me.Core.EnviroResponseShapeManager
        End Get
    End Property

    Protected Overrides Sub FillData()

        If (Me.UIContext Is Nothing) Then Return
        If (Me.Handler.UIContext Is Nothing) Then Return

        Dim shapes As cShapeData() = Me.Shapes
        Dim iNumShapes As Integer = shapes.Length
        Dim iNumPoints As Integer = 0
        Dim iNumHeaders As Integer = 0
        Dim cell As SourceGrid2.Cells.ICell = Nothing
        Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

        iNumPoints = Me.XAxisMax
        Me.Redim(iNumPoints + [Enum].GetValues(GetType(eRowType)).Length, iNumShapes + 1)

        cApplicationStatusNotifier.StartProgress(Me.UIContext.Core, SharedResources.STATUS_UPDATING)

        ' Create row headers
        Me(eRowType.Header, 0) = New EwEColumnHeaderCell(SharedResources.HEADER_INDEX)
        Me(eRowType.Thumbnail, 0) = New EwERowHeaderCell(SharedResources.HEADER_IMAGE)
        Me(eRowType.Name, 0) = New EwERowHeaderCell(SharedResources.HEADER_NAME)

        Me(eRowType.LimLeft, 0) = New EwERowHeaderCell(SharedResources.HEADER_LEFT_LIMIT)
        Me(eRowType.LimRight, 0) = New EwERowHeaderCell(SharedResources.HEADER_RIGHT_LIMIT)
        'Me(eRowType.LimMean, 0) = New EwERowHeaderCell(SharedResources.HEADER_MEAN_LIMIT)

        ' Create row header cells
        For i As Integer = 0 To iNumPoints - 1
            cell = New EwERowHeaderCell(CStr(i + 1))
            Me(eRowType.FirstTime + i, 0) = cell
        Next

        ' Populate shape columns
        For i As Integer = 0 To iNumShapes - 1

            Dim env As cEnviroResponseFunction = DirectCast(shapes(i), cEnviroResponseFunction)
            style = If(Me.Handler.CanEditPoints(shapes(i)), cStyleGuide.eStyleFlags.OK, cStyleGuide.eStyleFlags.NotEditable)

            Me.Shape(i + 1) = env
            Me(eRowType.Header, i + 1) = New EwEColumnHeaderCell(CStr(shapes(i).Index))

            cell = New SourceGrid2.Cells.Real.Cell
            cell.Value = env
            cell.VisualModel = New cVisualModelThumbnail(Me.Handler)
            Me(eRowType.Thumbnail, i + 1) = cell

            cell = New EwECell(env.Name, GetType(String))
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(eRowType.Name, i + 1) = cell

            ' JS 10Jun13: added
            ' JS 07Sep14: limits must be editable
            cell = New EwECell(env.ResponseLeftLimit, GetType(Double))
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(eRowType.LimLeft, i + 1) = cell

            cell = New EwECell(env.ResponseRightLimit, GetType(Double))
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(eRowType.LimRight, i + 1) = cell

            'JS 07Sep14: removed mean cells because this calculation can only be performed after shapes have been updated. Too complicated to sync well and not really necessary
            'cell = New EwECell(env.ResponseMean, GetType(Double), cStyleGuide.eStyleFlags.NotEditable)
            'Me(eRowType.LimMean, i + 1) = cell

            For j As Integer = 0 To Math.Min(iNumPoints, env.nPoints) - 1
                cell = New EwECell(env.ShapeData(j + 1), GetType(Single), style)
                cell.Behaviors.Add(Me.EwEEditHandler)
                Me(eRowType.FirstTime + j, i + 1) = cell
            Next
            For j As Integer = env.nPoints To iNumPoints - 1
                cell = New EwECell(0, GetType(Integer), cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null)
                Me(eRowType.FirstTime + j, i + 1) = cell
            Next
        Next

        cApplicationStatusNotifier.EndProgress(Me.UIContext.Core)

    End Sub

    Protected Overrides Function SafeCellEdit(p As SourceGrid2.Position, _
                                              cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        Dim shape As cEnviroResponseFunction = DirectCast(Me.Shape(p.Column), cEnviroResponseFunction)

        Select Case p.Row
            Case eRowType.Name
                shape.Name = CStr(cell.GetValue(p))
            Case eRowType.LimLeft
                shape.ResponseLeftLimit = CSng(cell.GetValue(p))
            Case eRowType.LimRight
                shape.ResponseRightLimit = CSng(cell.GetValue(p))
            Case Else
                Dim iTime As Integer = p.Row - eRowType.FirstTime + 1
                shape.ShapeData(iTime) = CSng(cell.GetValue(p))
        End Select
        Return True

    End Function

End Class
