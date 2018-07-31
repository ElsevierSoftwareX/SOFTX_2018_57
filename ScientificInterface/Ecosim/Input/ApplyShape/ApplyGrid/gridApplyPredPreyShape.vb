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

Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceLibrary
Imports EwEUtils.Utilities

#End Region

Namespace Ecosim

    <CLSCompliant(False)> _
    Public Class gridApplyPredPreyShape
        Inherits gridApplyShapeBase

#Region " Private vars "

        Private m_groupfilter As eGroupFilter = eGroupFilter.Consumer
        Private m_applyShapeMode As eShapeCategoryTypes = eShapeCategoryTypes.NotSet

#End Region ' Private vars

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)
        End Sub

#Region " Public access "

        Public Property ApplyShapeMode() As eShapeCategoryTypes
            Get
                Return Me.m_applyShapeMode
            End Get
            Set(ByVal value As eShapeCategoryTypes)
                If (Me.m_applyShapeMode <> value) Then
                    Me.m_applyShapeMode = value
                    Me.RefreshContent()
                End If
            End Set
        End Property

        Public Property IsPredatorGrid() As eGroupFilter
            Get
                Return Me.m_groupfilter
            End Get
            Set(ByVal value As eGroupFilter)
                If (value <> Me.m_groupfilter) Then
                    Me.m_groupfilter = value
                    Me.RefreshContent()
                End If
            End Set
        End Property

        Public Overrides Sub ClearAllPairs()

            Dim interaction As cMediatedInteraction = Nothing
            Dim application As eForcingFunctionApplication
            Dim ff As cForcingFunction = Nothing

            cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_APPLYVALUES)
            Me.Core.SetBatchLock(cCore.eBatchLockType.Update)

            ' For each column (groupIndex - Predator)
            For iPred As Integer = 1 To Core.nLivingGroups
                ' For each row (rowIndex - Prey)
                For iPrey As Integer = 1 To Core.nGroups

                    ' Can assign FF at this spot in the matrix?
                    If m_interactionManager.isPredPrey(iPred, iPrey) Then

                        interaction = m_interactionManager.PredPreyInteraction(iPred, iPrey)
                        interaction.LockUpdates = True

                        For i As Integer = 1 To Me.m_interactionManager.MaxNShapes
                            interaction.getShape(i, ff, application)

                            ' Only delete pairs of current type
                            If (TypeOf ff Is cMediationBaseFunction) And _
                               (Me.m_applyShapeMode = eShapeCategoryTypes.Mediation) Then
                                interaction.setShape(i, Nothing)
                            End If

                            If (TypeOf ff Is cForcingFunction) And _
                               (Me.m_applyShapeMode = eShapeCategoryTypes.Forcing) Then
                                interaction.setShape(i, Nothing)
                            End If
                        Next

                        interaction.LockUpdates = False

                    End If
                Next
            Next

            Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecosim, True)
            cApplicationStatusNotifier.EndProgress(Me.Core)

        End Sub

        Public Overrides Sub SetAllPairs()

            Dim dlg As New dlgApplyPredPreyShape(Me.UIContext, Me.m_applyShapeMode, Me.m_groupfilter)
            dlg.ShowDialog()

        End Sub

#End Region ' Public properties

#Region " Overrides "

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_applyShapeMode = eShapeCategoryTypes.NotSet) Then Return

            Dim source As cCoreGroupBase = Nothing

            ' Define grid dimensions
            Me.Redim(Core.nGroups + 1, 2)

            ' Set header cells  'Prey \Predator '
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_PREYPREDATOR)

            Dim iCol As Integer = 2

            For i As Integer = 1 To Core.nGroups
                source = Core.EcoPathGroupInputs(i)
                ' # Group name row header cells
                Me(i, 0) = New EwERowHeaderCell(CStr(i))
                Me(i, 0).Behaviors.Add(m_bmRowCol)

                ' # Group name row header cells
                Me(i, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
                Me(i, 1).Behaviors.Add(m_bmRowCol)

                If ((Me.m_groupfilter = eGroupFilter.Consumer) And (source.IsConsumer)) Or _
                   ((Me.m_groupfilter = eGroupFilter.Producer) And (source.IsProducer)) Or _
                   ((Me.m_groupfilter = eGroupFilter.Detritus) And (source.IsDetritus)) Then
                    Me.AddColumn(iCol, source)
                    iCol += 1
                End If
            Next

        End Sub

        Protected Overrides Sub FillData()

            Dim cellDefault As EwECell = Nothing
            Dim ff As cForcingFunction = Nothing
            Dim PPI As cMediatedInteraction = Nothing

            If (Me.m_interactionManager Is Nothing) Then Return
            If (Me.m_applyShapeMode = eShapeCategoryTypes.NotSet) Then Return

            ' For each predator column
            For iCol As Integer = 2 To Me.Columns.Count - 1
                Dim iPred As Integer = CInt(Me.Columns(iCol).Tag)

                ' For each prey row 
                For iRow As Integer = 1 To Me.Rows.Count - 1
                    Dim iPrey As Integer = iRow

                    ' Can assign FF at this spot in the matrix?
                    If m_interactionManager.isPredPrey(iPred, iPrey) Then

                        PPI = m_interactionManager.PredPreyInteraction(iPred, iPrey)
                        Dim shape As cForcingFunction = Nothing
                        Dim aplType As eForcingFunctionApplication
                        Dim sb As New StringBuilder()

                        If PPI IsNot Nothing Then
                            For i As Integer = 1 To PPI.nAppliedShapes
                                PPI.getShape(i, shape, aplType)

                                ' Is med?
                                If (TypeOf shape Is cMediationFunction) Then
                                    If ((Me.m_applyShapeMode And eShapeCategoryTypes.Mediation) = eShapeCategoryTypes.Mediation) Then
                                        If sb.Length > 0 Then sb.Append(" ")
                                        sb.Append(cStringUtils.Localize(My.Resources.ECOSIM_APPLYFF_FFTYPE_MEDIATION, shape.Index))
                                    End If
                                Else
                                    If ((Me.m_applyShapeMode And eShapeCategoryTypes.Forcing) = eShapeCategoryTypes.Forcing) Then
                                        If sb.Length > 0 Then sb.Append(" ")
                                        sb.Append(cStringUtils.Localize(My.Resources.ECOSIM_APPLYFF_FFTYPE_FORCING, shape.Index))
                                    End If
                                End If
                            Next
                        Else
                            ' This should NOT occur; this indicates that the PPI manager is not up to date!
                            sb.Append("X")
                        End If

                        Me(iRow, iCol) = New Cells.Real.Cell(sb.ToString)
                        Me(iRow, iCol).DataModel = m_editor
                        Me(iRow, iCol).Behaviors.Add(m_bmCell)

                    Else
                        ' #No: cannot assign FF to this pred/prey combo
                        cellDefault = New EwECell(Nothing, GetType(Single))
                        '  Setup default cell
                        cellDefault.Style = (cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null)
                        ' Apply cell to the grid
                        Me(iRow, iCol) = cellDefault
                    End If

                Next iRow

            Next iCol

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.FixedColumnWidths = False
        End Sub

#End Region ' Overrides 

#Region " Internals "

        Protected Overrides Sub CellClick(ByVal sender As Object, ByVal e As PositionEventArgs)

            'Row num, column num starts from one, which is consistent with group index scheme (from one)
            Dim iPred As Integer = CInt(Me(0, e.Position.Column).Value)
            Dim dlg As New dlgApplyPredPreyShape(Me.UIContext, e.Position.Row, iPred, Me.m_applyShapeMode, Me.m_groupfilter)

            dlg.ShowDialog()

        End Sub

        Protected Overrides Sub OnRowColClicked(ByVal sender As Object, ByVal e As SourceGrid2.PositionEventArgs)

            Dim iRow As Integer = e.Position.Row
            Dim iCol As Integer = e.Position.Column
            Dim dlg As dlgApplyPredPreyShape = Nothing

            ' --------------
            ' Prepare dialog
            ' --------------

            ' Column header clicked?
            If iRow = 0 Then
                ' #Yes: Predator column clicked?
                If iCol > 1 Then
                    ' #Yes: launch dialog for all diets of this predator
                    Dim iPred As Integer = CInt(Me(0, iCol).Value)
                    dlg = New dlgApplyPredPreyShape(Me.UIContext, iPred, dlgApplyPredPreyShape.eEditMode.Predator, Me.m_applyShapeMode, Me.m_groupfilter)
                End If
            Else
                ' #No: Prey row header clicked?
                If iCol < Me.FixedColumns Then
                    ' #Yes: Prey row clicked?
                    If iRow > 0 Then
                        ' #Yes: launch dialog for all predation of this prey
                        dlg = New dlgApplyPredPreyShape(Me.UIContext, iRow, dlgApplyPredPreyShape.eEditMode.Prey, Me.m_applyShapeMode, Me.m_groupfilter)
                    End If
                End If
            End If

            ' --------------
            ' Invoke dialog
            ' --------------

            If dlg IsNot Nothing Then
                dlg.ShowDialog()
            End If

        End Sub

        Protected Sub AddColumn(ByVal iCol As Integer, ByVal source As cCoreGroupBase)
            Me.Columns.Insert(iCol)
            Me(0, iCol) = New PropertyColumnHeaderCell(Me.PropertyManager, source, eVarNameFlags.Index)
            Me(0, iCol).Behaviors.Add(m_bmRowCol)
            Me.Columns(iCol).Tag = source.Index
        End Sub

#End Region ' Internals

    End Class

End Namespace
