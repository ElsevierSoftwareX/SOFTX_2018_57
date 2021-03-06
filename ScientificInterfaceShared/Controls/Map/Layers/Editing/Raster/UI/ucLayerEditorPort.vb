﻿' ===============================================================================
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
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.Map.Layers

    Public Class ucLayerEditorPort

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Not Me.IsAttached) Then Return

            ' Initialize group combo 
            Dim core As cCore = Me.UIContext.Core
            Dim fleet As cEcopathFleetInput = Nothing

            Me.m_cmbFleet.Items.Clear()

            Me.m_cmbFleet.Items.Add(My.Resources.GENERIC_VALUE_ALLFLEETS)
            For i As Integer = 1 To core.nFleets
                fleet = core.EcopathFleetInputs(i)
                Me.m_cmbFleet.Items.Add(fleet)
            Next i

            ' Update control
            Me.m_cmbFleet.SelectedIndex = Me.FleetIndex

        End Sub

        Public Overrides Sub UpdateContent(ByVal editor As cLayerEditorRaster)
            MyBase.UpdateContent(editor)

            Me.m_btnClear.Enabled = (Me.IsAttached)
            Me.m_btnSet.Enabled = (Me.IsAttached)

        End Sub

        Protected Overloads Property Editor() As cLayerEditorPorts
            Get
                Return DirectCast(MyBase.Editor, cLayerEditorPorts)
            End Get
            Set(ByVal editor As cLayerEditorPorts)
                ' Sanity check
                Debug.Assert(TypeOf editor Is cLayerEditorPorts, "ucLayerEditorFleet connected to wrong editor class")
                ' Configure editor
                editor.CellValue = 0
                ' Set
                MyBase.Editor = editor
            End Set
        End Property

        Protected Property FleetIndex() As Integer
            Get
                If (Not Me.IsAttached) Then Return cCore.NULL_VALUE
                Return Me.Editor.Fleet
            End Get
            Set(ByVal value As Integer)
                If (Me.IsAttached) Then
                    If (Me.Editor.Fleet <> value) Then
                        Me.Editor.Fleet = value
                    End If
                End If
            End Set
        End Property

        Private Sub OnFormatItemText(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
            Handles m_cmbFleet.Format

            If (TypeOf e.ListItem Is String) Then
                e.Value = CStr(e.ListItem)
            Else
                Dim io As cCoreInputOutputBase = DirectCast(e.ListItem, cCoreInputOutputBase)
                Dim fmt As New cCoreInterfaceFormatter()
                e.Value = fmt.GetDescriptor(io)
            End If
        End Sub

        Private Sub OnFleetSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbFleet.SelectedIndexChanged
            Dim item As Object = Me.m_cmbFleet.SelectedItem
            If (TypeOf item Is cCoreInputOutputBase) Then
                Me.FleetIndex = DirectCast(item, cCoreInputOutputBase).Index
            Else
                Me.FleetIndex = 0
            End If
        End Sub

        Private Sub OnClear(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnClear.Click
            Me.UIContext.Core.ClearEcospacePort(Me.FleetIndex)
        End Sub

        Private Sub OnSet(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnSet.Click
            Me.UIContext.Core.SetEcospaceAllCoastToPort(Me.FleetIndex)
        End Sub


    End Class

End Namespace
