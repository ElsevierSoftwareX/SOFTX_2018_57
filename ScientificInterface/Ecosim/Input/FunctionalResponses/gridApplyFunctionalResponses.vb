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
Imports System.Globalization
Imports EwECore
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceLibrary

#End Region

Namespace Ecosim

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Grid to apply environmental response functions to Ecosim forcing functions.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class gridApplyEcosimEnvironmentalResponses
        Inherits gridApplyShapeBase

#Region " Private vars "

        Private m_lProps As New List(Of cProperty)
        Private m_driverManager As IEnvironmentalResponseManager = Nothing
        Private m_shapeManager As cBaseShapeManager = Nothing

#End Region ' Private vars

#Region " Overrides "

        Public Overrides Property UIContext As ScientificInterfaceShared.Controls.cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(value As ScientificInterfaceShared.Controls.cUIContext)
                If (value IsNot Nothing) Then
                    Me.m_driverManager = value.Core.EcosimEnviroResponseManager
                    Me.m_shapeManager = value.Core.EnviroResponseShapeManager
                Else
                    Me.m_driverManager = Nothing
                    Me.m_shapeManager = Nothing
                End If
                MyBase.UIContext = value
            End Set
        End Property

        Public Overrides ReadOnly Property CoreComponents As EwEUtils.Core.eCoreComponentType()
            Get
                Return New eCoreComponentType() {eCoreComponentType.EcosimResponseInteractionManager, eCoreComponentType.ShapesManager}
            End Get
        End Property

        Public Overrides Sub OnCoreMessage(ByRef msg As EwECore.cMessage)
            If (msg.Type = eMessageType.DataAddedOrRemoved) Then
                Me.RefreshContent()
            ElseIf (msg.Type = eMessageType.DataModified And msg.Source = eCoreComponentType.EcosimResponseInteractionManager) Then
                For igrp As Integer = 1 To Me.Core.nGroups
                    Me.UpdateRow(Me.Core.EcoSimGroupInputs(igrp))
                Next
            End If
        End Sub

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            If (Me.UIContext Is Nothing) Then Return

            Dim group As cCoreGroupBase = Nothing
            Dim driver As IEnviroInputData = Nothing
            Dim fmt As New cCoreInterfaceFormatter()

            ' Define grid dimensions
            Me.Redim(Core.nGroups + 1, Me.m_driverManager.nEnviroData + 2)

            For iDriver As Integer = 1 To Me.m_driverManager.nEnviroData

                driver = Me.m_driverManager.EnviroData(iDriver)
                Me(0, 1 + iDriver) = New EwEColumnHeaderCell(driver.Name)
                Me(0, 1 + iDriver).Behaviors.Add(Me.m_bmRowCol)

            Next iDriver

            Me(0, 0) = New EwEColumnHeaderCell("")
            Me(0, 1) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)

            For iGroup As Integer = 1 To Core.nGroups
                group = Core.EcoPathGroupInputs(iGroup)
                ' # Group index row header cells
                Me(iGroup, 0) = New EwERowHeaderCell(CStr(iGroup))
                Me(iGroup, 0).Behaviors.Add(Me.m_bmRowCol)

                ' # Group name row header cells
                Me(iGroup, 1) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
                Me(iGroup, 1).Behaviors.Add(Me.m_bmRowCol)
            Next

        End Sub

        Protected Overrides Sub FillData()

            Try
                For igrp As Integer = 1 To Core.nGroups
                    For iDriver As Integer = 1 To Me.m_driverManager.nEnviroData
                        Me(igrp, iDriver + 1) = New EwECell("", GetType(String))
                        Me(igrp, iDriver + 1).DataModel = Me.m_editor
                        Me(igrp, iDriver + 1).Behaviors.Add(Me.m_bmCell)
                    Next

                    Me.UpdateRow(Me.Core.EcoSimGroupInputs(igrp))

                Next
            Catch ex As Exception

            End Try

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.FixedColumnWidths = False
        End Sub

        Public Overrides Sub ClearAllPairs()
            ' NOP
        End Sub

        Public Overrides Sub SetAllPairs()
            ' NOP
        End Sub

#End Region ' Overrides

#Region " Internals "

        Protected Overrides Sub CellClick(ByVal sender As Object, ByVal e As PositionEventArgs)

            Try

                Dim iGrp As Integer = e.Position.Row
                Dim iDriver As Integer = e.Position.Column - 1
                Dim cell As EwECell = DirectCast(Me(e.Position.Row, e.Position.Column), EwECell)

                If ((cell.Style And cStyleGuide.eStyleFlags.NotEditable) = cStyleGuide.eStyleFlags.NotEditable) Then Return

                Me.ShowSelectionDialog(eEnvironmentalResponseSelectionType.DriverGroup, iGrp, iDriver)

            Catch ex As Exception
                ' Whoah
            End Try

        End Sub

        Private Sub ShowSelectionDialog(ByVal SelectionType As eEnvironmentalResponseSelectionType, ByVal iGrp As Integer, ByVal iDriver As Integer)
            Try
                Dim dlg As New dlgSelectEnvironmentalResponse(Me.UIContext, Me.m_shapeManager, Me.m_driverManager, iDriver, iGrp, SelectionType)
                dlg.ShowDialog()
                If dlg.DialogResult = DialogResult.OK Then
                    'the dialogue will update the CapacitMapInteractionManager with the selected Shapes
                    'update the interface from the CapacitMapInteractionManager data
                    Me.FillData()
                End If

            Catch ex As Exception

            End Try
        End Sub

        Protected Overrides Sub OnRowColClicked(ByVal sender As Object, ByVal e As SourceGrid2.PositionEventArgs)
            Try

                Dim igrp As Integer = e.Position.Row
                Dim iDriver As Integer = e.Position.Column - 1

                ' Can no longer invoke UI for one group, multiple drivers. This makes no sense
                If iDriver < 0 Then Return

                Me.ShowSelectionDialog(eEnvironmentalResponseSelectionType.Driver, igrp, iDriver)

            Catch ex As Exception

            End Try

        End Sub

        Private Function CanApplyGroup(iGroup As Integer) As Boolean

        End Function

        Private Sub OnPropertyChanged(prop As cProperty, cf As cProperty.eChangeFlags)
            Me.UpdateRow(DirectCast(prop.Source, cEcospaceGroupInput))
        End Sub

        Private Sub UpdateRow(grp As cCoreGroupBase)

            Try

                Dim igrp As Integer = grp.Index
                Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
                Dim fmt As New cShapeDataFormatter()

                For iDriver As Integer = 1 To Me.m_driverManager.nEnviroData
                    Dim strLabel As String = ""
                    Dim ff As cForcingFunction = Nothing
                    Dim driver As IEnviroInputData = Me.m_driverManager.EnviroData(iDriver)
                    Dim cell As EwECell = CType(Me(igrp, 1 + iDriver), EwECell)

                    Dim ishp As Integer = driver.ResponseIndexForGroup(igrp)
                    If ishp > 0 Then
                        ff = Me.m_shapeManager.Item(ishp - 1)
                        strLabel = fmt.GetDescriptor(ff)
                    End If

                    cell.Style = style
                    cell.Value = strLabel
                    Me.InvalidateCell(cell)
                Next
            Catch ex As Exception

            End Try

        End Sub

#End Region ' Internals

    End Class

End Namespace
