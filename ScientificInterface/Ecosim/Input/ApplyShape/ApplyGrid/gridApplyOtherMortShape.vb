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

    ''' <summary>
    ''' Grid to apply other mortality forcing
    ''' </summary>
    ''' <seealso cref="ScientificInterface.gridApplyShapeBase" />
    <CLSCompliant(False)>
    Public Class gridApplyOtherMortShape
        Inherits gridApplyShapeBase

#Region " Private vars "

#End Region ' Private vars

        Public Sub New()
            MyBase.New()
        End Sub

#Region " Public access "

        Public Overrides Sub ClearAllPairs()
            ' ToDo: implement this
        End Sub

        Public Overrides Sub SetAllPairs()
            Dim dlg As New dlgApplyGroupShape(Me.UIContext)
            dlg.ShowDialog()
        End Sub

#End Region ' Public properties

#Region " Overrides "

        Protected Overrides Sub InitStyle()

            MyBase.InitStyle()

            If (Me.UIContext Is Nothing) Then Return

            Dim source As cCoreGroupBase = Nothing

            ' Define grid dimensions
            Me.Redim(Core.nLivingGroups + 1, 3)

            ' Set header cells
            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUP)
            Me(0, 2) = New EwEColumnHeaderCell(My.Resources.HEADER_FORCINGNUMBER)
            Me(0, 2).Behaviors.Add(m_bmRowCol)

            Dim iCol As Integer = 2

            For i As Integer = 1 To Core.nLivingGroups
                source = Core.EcoPathGroupInputs(i)
                ' # Group name row header cells
                Me(i, 0) = New EwERowHeaderCell(CStr(i))
                Me(i, 0).Behaviors.Add(m_bmRowCol)

                ' # Group name row header cells
                Me(i, 1) = New PropertyRowHeaderCell(Me.PropertyManager, source, eVarNameFlags.Name)
                Me(i, 1).Behaviors.Add(m_bmRowCol)

            Next

        End Sub

        Protected Overrides Sub FillData()

            Dim cellDefault As EwECell = Nothing
            Dim ff As cForcingFunction = Nothing

            If (Me.m_interactionManager Is Nothing) Then Return

            Dim iCol As Integer = 2
            For iRow As Integer = 1 To Me.Rows.Count - 1
                Dim iGrp As Integer = iRow

                Dim PPI As cMediatedInteraction = m_interactionManager.GroupInteraction(iGrp)
                Dim shape As cForcingFunction = Nothing
                Dim aplType As eForcingFunctionApplication
                Dim sb As New StringBuilder()

                If PPI IsNot Nothing Then
                    For i As Integer = 1 To PPI.nAppliedShapes
                        PPI.getShape(i, shape, aplType)

                        If sb.Length > 0 Then sb.Append(" ")
                        sb.Append(cStringUtils.Localize(My.Resources.ECOSIM_APPLYFF_FFTYPE_FORCING, shape.Index))

                    Next
                Else
                    ' This should NOT occur; this indicates that the PPI manager is not up to date!
                    sb.Append("X")
                End If

                Me(iRow, iCol) = New Cells.Real.Cell(sb.ToString)
                Me(iRow, iCol).DataModel = m_editor
                Me(iRow, iCol).Behaviors.Add(m_bmCell)

            Next iRow

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.FixedColumnWidths = False
        End Sub

#End Region ' Overrides 

#Region " Internals "

        Protected Overrides Sub CellClick(ByVal sender As Object, ByVal e As PositionEventArgs)

            Dim dlg As New dlgApplyGroupShape(Me.UIContext, e.Position.Row)
            dlg.ShowDialog()

        End Sub

        Protected Overrides Sub OnRowColClicked(ByVal sender As Object, ByVal e As SourceGrid2.PositionEventArgs)
            If (e.Position.Row = 0) Then
                If (e.Position.Column = 2) Then
                    Me.SetAllPairs()
                End If
            Else
                Dim dlg As New dlgApplyGroupShape(Me.UIContext, e.Position.Row)
                dlg.ShowDialog()
            End If
        End Sub

#End Region ' Internals

    End Class

End Namespace