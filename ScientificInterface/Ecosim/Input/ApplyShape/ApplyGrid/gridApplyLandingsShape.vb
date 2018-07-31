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

Imports System.Globalization
Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2

#End Region

Namespace Ecosim

    <CLSCompliant(False)> _
    Public Class gridApplyLandingsShape
        Inherits gridApplyShapeBase

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)
        End Sub

#Region " Public access "

        Public Overrides Sub ClearAllPairs()

            Dim interaction As cMediatedInteraction = Nothing

            cApplicationStatusNotifier.StartProgress(Me.Core, My.Resources.STATUS_APPLYVALUES)
            Me.Core.SetBatchLock(cCore.eBatchLockType.Update)

            ' For each column (groupIndex - Fleet)
            For iFleet As Integer = 1 To Core.nFleets
                ' For each row (rowIndex - Landed group)
                For iGroup As Integer = 1 To Core.nGroups

                    ' Can assign FF at this spot in the matrix?
                    If Me.m_interactionManager.isLandings(iFleet, iGroup) Then

                        interaction = Me.m_interactionManager.LandingInteraction(iFleet, iGroup)
                        interaction.LockUpdates = True
                        For i As Integer = 1 To Me.m_interactionManager.MaxNShapes
                            interaction.setShape(i, Nothing)
                        Next
                        interaction.LockUpdates = False

                    End If
                Next
            Next

            Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecosim, True)
            cApplicationStatusNotifier.EndProgress(Me.Core)

        End Sub

        Public Overrides Sub SetAllPairs()
            ' NOP
        End Sub

#End Region ' Public properties

#Region " Overrides "

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            If (Me.UIContext Is Nothing) Then Return

            Dim fleet As cEcopathFleetInput = Nothing
            Dim group As cCoreGroupBase = Nothing

            ' Define grid dimensions
            Me.Redim(Core.nLivingGroups + 1, Core.nFleets + 2)

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)

            For iFleet As Integer = 1 To Me.Core.nFleets
                fleet = Me.Core.EcopathFleetInputs(iFleet)
                Me(0, 1 + iFleet) = New EwEColumnHeaderCell(fleet.Name)
                Me(0, 1 + iFleet).Behaviors.Add(Me.m_bmRowCol)
            Next

            For iGroup As Integer = 1 To Core.nLivingGroups
                group = Core.EcoPathGroupInputs(iGroup)
                ' # Group name row header cells
                Me(iGroup, 0) = New EwERowHeaderCell(CStr(iGroup))
                Me(iGroup, 0).Behaviors.Add(Me.m_bmRowCol)

                ' # Group name row header cells
                Me(iGroup, 1) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
                Me(iGroup, 1).Behaviors.Add(Me.m_bmRowCol)
            Next

        End Sub

        Protected Overrides Sub FillData()

            Dim ff As cForcingFunction = Nothing
            Dim interaction As cMediatedInteraction = Nothing
            Dim cellBlocked As EwECell = Nothing
            Dim fmt As New cMonetaryTypeFormatter()
            Dim ri As New RegionInfo(CultureInfo.CurrentUICulture.LCID)
            Dim strSymbol As String = fmt.GetDescriptor(ri, eDescriptorTypes.Symbol)

            If (Me.m_interactionManager Is Nothing) Then Return

            For iFleet As Integer = 1 To Me.Core.nFleets
                For iGroup As Integer = 1 To Me.Core.nLivingGroups

                    If Me.m_interactionManager.isLandings(iFleet, iGroup) Then

                        interaction = Me.m_interactionManager.LandingInteraction(iFleet, iGroup)
                        Dim shape As cForcingFunction = Nothing
                        Dim aplType As eForcingFunctionApplication
                        Dim sb As New StringBuilder()

                        If interaction IsNot Nothing Then
                            For i As Integer = 1 To interaction.nAppliedShapes
                                interaction.getShape(i, shape, aplType)
                                If shape IsNot Nothing Then
                                    If sb.Length > 0 Then sb.Append(" ")
                                    sb.Append(String.Format(My.Resources.ECOSIM_APPLYFF_FFTYPE_PRICEELASTICITY, shape.Index, strSymbol))
                                End If
                            Next
                        Else
                            ' This should NOT occur; this indicates that the interaction manager is not up to date!
                            sb.Append("X")
                        End If

                        Me(iGroup, iFleet + 1) = New Cells.Real.Cell(sb.ToString)
                        Me(iGroup, iFleet + 1).DataModel = Me.m_editor
                        Me(iGroup, iFleet + 1).Behaviors.Add(Me.m_bmCell)

                    Else
                        ' #No: cannot assign FF to this pred/prey combo
                        cellBlocked = New EwECell(Nothing, GetType(Single))
                        '  Setup default cell
                        cellBlocked.Style = (cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null)
                        ' Apply cell to the grid
                        Me(iGroup, iFleet + 1) = cellBlocked
                    End If

                Next iGroup

            Next iFleet

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            'Me.FixedColumnWidths = False
        End Sub

#End Region ' Overrides 

#Region " Internals "

        Protected Overrides Sub CellClick(ByVal sender As Object, ByVal e As PositionEventArgs)

            Try
                Dim dlg As New dlgApplyLandingShape(Me.UIContext, e.Position.Row, e.Position.Column - 1)
                dlg.ShowDialog()
            Catch ex As Exception
                ' Whoah
            End Try

        End Sub

        Protected Overrides Sub OnRowColClicked(ByVal sender As Object, ByVal e As SourceGrid2.PositionEventArgs)

            Dim iRow As Integer = e.Position.Row
            Dim iCol As Integer = e.Position.Column
            Dim dlg As dlgApplyLandingShape = Nothing

            ' --------------
            ' Prepare dialog
            ' --------------

            ' Column header clicked?
            If iRow = 0 Then
                ' #Yes: Predator column clicked?
                If iCol > 1 Then
                    ' #Yes: launch dialog for all diets of this predator
                    Dim iFleet As Integer = iCol - 1
                    dlg = New dlgApplyLandingShape(Me.UIContext, iFleet, dlgApplyLandingShape.eEditMode.Fleet)
                Else
                    dlg = New dlgApplyLandingShape(Me.UIContext)
                End If
            Else
                ' #No: Prey row header clicked?
                If iCol < Me.FixedColumns Then
                    ' #Yes: Prey row clicked?
                    If iRow > 0 Then
                        ' #Yes: launch dialog for all predation of this prey
                        dlg = New dlgApplyLandingShape(Me.UIContext, iRow, dlgApplyLandingShape.eEditMode.Group)
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

#End Region ' Internals

    End Class

End Namespace
