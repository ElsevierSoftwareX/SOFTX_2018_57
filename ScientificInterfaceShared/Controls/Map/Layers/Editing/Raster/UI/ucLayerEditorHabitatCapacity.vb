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

Imports EwECore
Imports ScientificInterfaceShared.Style

Namespace Controls.Map.Layers

    Public Class ucLayerEditorHabitatCapacity

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Not Me.IsAttached) Then Return

            ' Initialize group combo 
            Dim core As cCore = Me.UIContext.Core
            Dim group As cCoreGroupBase = Nothing

            Me.m_cmbGroups.Items.Clear()

            For iGroup As Integer = 1 To core.nGroups
                group = core.EcoPathGroupInputs(iGroup)
                Me.m_cmbGroups.Items.Add(group)
            Next iGroup

            ' Update control
            Me.m_cmbGroups.SelectedIndex = Me.GroupIndex - 1

        End Sub

        Public Overrides Sub UpdateContent(ByVal editor As cLayerEditorRaster)
            MyBase.UpdateContent(editor)

            Me.m_cmbGroups.Enabled = Me.IsAttached

        End Sub

        Protected Overloads Property Editor() As cLayerEditorGroup
            Get
                Return DirectCast(MyBase.Editor, cLayerEditorGroup)
            End Get
            Set(ByVal editor As cLayerEditorGroup)
                ' Sanity check
                Debug.Assert(TypeOf editor Is cLayerEditorGroup, "ucLayerEditorGroup connected to wrong editor class")
                ' Configure editor
                editor.CellValue = 0
                ' Set
                MyBase.Editor = editor
            End Set
        End Property

        Protected Property GroupIndex() As Integer
            Get
                If (Not Me.IsAttached) Then Return cCore.NULL_VALUE
                Return Me.Editor.Group
            End Get
            Set(ByVal value As Integer)
                If (Me.IsAttached) Then
                    If (Me.Editor.Group <> value) Then
                        Me.Editor.Group = value
                    End If
                End If
            End Set
        End Property

#Region " Events "

        Private Sub OnGroupSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbGroups.SelectedIndexChanged
            Me.GroupIndex = Me.m_cmbGroups.SelectedIndex + 1
        End Sub

        Private Sub OnFormatItemText(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
            Handles m_cmbGroups.Format
            Dim io As cCoreInputOutputBase = DirectCast(e.ListItem, cCoreInputOutputBase)
            Dim fmt As New cCoreInterfaceFormatter()
            e.Value = fmt.GetDescriptor(io)
        End Sub

        Private Sub OnSetDefaultAllClick(sender As System.Object, e As System.EventArgs) _
            Handles m_btnAllDefault.Click

            Dim ngrps As Integer = Me.UIContext.Core.nGroups

            Try
                Dim capManager As EwECore.cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
                Dim map As cEcospaceLayerHabitatCapacity
                Dim nrows As Integer = capManager.InRow
                Dim ncols As Integer = capManager.InCol
                For igrp As Integer = 1 To ngrps
                    map = capManager.LayerHabitatCapacityInput(igrp)
                    Me.SetDefault(map, nrows, ncols)
                Next igrp

                Me.UpdateCore()
            Catch ex As Exception

            End Try

        End Sub

        Private Sub OnSetDefaultThis(sender As System.Object, e As System.EventArgs) Handles m_btnLayerDefault.Click
            Try
                Dim capManager As EwECore.cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
                Me.SetDefault(capManager.LayerHabitatCapacityInput(Me.GroupIndex), _
                                  capManager.InRow, capManager.InCol)
                Me.UpdateCore()
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Events

#Region " Internals "

        Private Sub SetDefault(map As cEcospaceLayerHabitatCapacity, nrows As Integer, ncols As Integer)
            For ir As Integer = 1 To nrows
                For ic As Integer = 1 To ncols
                    ' The user interface should really not assume this. It's a core thing
                    map.Cell(ir, ic) = 1.0
                Next
            Next
            map.Invalidate()
        End Sub

        Private Sub UpdateCore()
            Me.UIContext.Core.onChanged(Me.UIContext.Core.EcospaceBasemap.LayerHabitatCapacityInput(1))
        End Sub

#End Region ' Internals

    End Class

End Namespace
