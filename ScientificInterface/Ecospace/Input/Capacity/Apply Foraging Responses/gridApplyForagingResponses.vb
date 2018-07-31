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

Namespace Ecospace

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Grid to apply environmental response functions to capacity maps.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(False)>
    Public Class gridApplyForagingResponses
        Inherits gridApplyShapeBase

#Region " Private vars "

        Private m_lProps As New List(Of cProperty)

#End Region ' Private vars

#Region " Overrides "

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            If (Me.UIContext Is Nothing) Then Return

            Dim group As cCoreGroupBase = Nothing
            Dim mapManager As IEnvironmentalResponseManager = Core.CapacityMapInteractionManager
            Dim map As IEnviroInputData = Nothing
            Dim fmt As New cCoreInterfaceFormatter()

            ' Define grid dimensions
            Me.Redim(Core.nGroups + 1, mapManager.nEnviroData + 2)

            For iMap As Integer = 1 To mapManager.nEnviroData

                map = mapManager.EnviroData(iMap)
                Me(0, 1 + iMap) = New PropertyColumnHeaderCell(Me.PropertyManager, DirectCast(map, cEnviroInputMap).Layer, eVarNameFlags.Name)
                Me(0, 1 + iMap).Behaviors.Add(Me.m_bmRowCol)

            Next iMap

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
                Dim Manager As IEnvironmentalResponseManager = Core.CapacityMapInteractionManager
                Dim ShapeManager As cEnviroResponseShapeManager = Me.Core.EnviroResponseShapeManager
                Dim ff As cForcingFunction
                Dim strLabel As String

                For igrp As Integer = 1 To Core.nGroups
                    Dim grp As cEcospaceGroupInput = Me.Core.EcospaceGroupInputs(igrp)
                    For imap As Integer = 1 To Manager.nEnviroData
                        Dim map As IEnviroInputData = Manager.EnviroData(imap)
                        strLabel = ""
                        Dim ishp As Integer = map.ResponseIndexForGroup(igrp)
                        If ishp > 0 Then
                            ff = ShapeManager.Item(ishp - 1)
                            strLabel = String.Format(SharedResources.GENERIC_LABEL_INDEXED, ff.Index, ff.Name)
                        End If

                        Me(igrp, imap + 1) = New EwECell(strLabel, GetType(String))
                        Me(igrp, imap + 1).DataModel = Me.m_editor
                        Me(igrp, imap + 1).Behaviors.Add(Me.m_bmCell)
                    Next

                    Dim prop As cProperty = Me.PropertyManager.GetProperty(grp, eVarNameFlags.EcospaceCapCalType)
                    Me.m_lProps.Add(prop)
                    AddHandler prop.PropertyChanged, AddressOf OnPropertyChanged

                    Me.UpdateRow(grp)

                Next
            Catch ex As Exception

            End Try

        End Sub

        Protected Overrides Sub ClearData()
            For Each prop As cProperty In Me.m_lProps
                RemoveHandler prop.PropertyChanged, AddressOf OnPropertyChanged
            Next
            MyBase.ClearData()
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

        Protected Overrides Sub OnRowColClicked(ByVal sender As Object, ByVal e As SourceGrid2.PositionEventArgs)
            Try

                Dim igrp As Integer = e.Position.Row
                Dim iDriver As Integer = e.Position.Column - 1

                ' Can no longer invoke UI for one group, multiple drivers. This makes no sense
                If (iDriver < 0) Then Return

                'just assume it is the column that the user has selected!!!
                Dim selectionType As eEnvironmentalResponseSelectionType = eEnvironmentalResponseSelectionType.Driver

                Me.ShowSelectionDialog(selectionType, igrp, iDriver)

            Catch ex As Exception

            End Try

        End Sub

        Private Sub OnPropertyChanged(prop As cProperty, cf As cProperty.eChangeFlags)
            Me.UpdateRow(DirectCast(prop.Source, cEcospaceGroupInput))
        End Sub

        Private Sub ShowSelectionDialog(ByVal SelectionType As eEnvironmentalResponseSelectionType, ByVal iGrp As Integer, ByVal iDriver As Integer)
            Try
                Dim MapManager As IEnvironmentalResponseManager = Core.CapacityMapInteractionManager
                Dim ShapeManager As cBaseShapeManager = Core.EnviroResponseShapeManager

                Dim dlg As New dlgSelectEnvironmentalResponse(Me.UIContext, ShapeManager, MapManager, iDriver, iGrp, SelectionType)
                dlg.ShowDialog()
                If dlg.DialogResult = DialogResult.OK Then
                    'the dialogue will update the CapacitMapInteractionManager with the selected Shapes
                    'update the interface from the CapacitMapInteractionManager data
                    Me.FillData()
                End If

            Catch ex As Exception

            End Try
        End Sub

        Private Sub UpdateRow(grp As cEcospaceGroupInput)

            Dim iGroup As Integer = grp.Index
            Dim style As cStyleGuide.eStyleFlags
            Dim mapManager As IEnvironmentalResponseManager = Core.CapacityMapInteractionManager

            If ((grp.CapacityCalculationType And eEcospaceCapacityCalType.EnvResponses) = eEcospaceCapacityCalType.EnvResponses) Then
                style = cStyleGuide.eStyleFlags.OK
            Else
                style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null
            End If

            For iMap As Integer = 1 To mapManager.nEnviroData
                Dim cell As EwECell = CType(Me(iGroup, 1 + iMap), EwECell)
                Dim map As IEnviroInputData = mapManager.EnviroData(iMap)
                ' Reflect
                cell.Style = style Or If(map.IsDriverActive, cStyleGuide.eStyleFlags.OK, cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null)
                Me.InvalidateCell(cell)
            Next

        End Sub

#End Region ' Internals

    End Class

End Namespace
