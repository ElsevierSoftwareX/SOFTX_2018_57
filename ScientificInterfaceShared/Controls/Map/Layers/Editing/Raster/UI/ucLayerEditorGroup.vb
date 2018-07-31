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
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.Map.Layers

    Public Class ucLayerEditorGroup
        Inherits ucLayerEditorDefault

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Not Me.IsAttached) Then Return

            ' Initialize group combo 
            Dim core As cCore = Me.UIContext.Core
            Dim grp As cCoreGroupBase = Nothing

            Me.m_cmbGroup.Items.Clear()
            For iGroup As Integer = 1 To core.nGroups
                grp = core.EcoPathGroupInputs(iGroup)
                Me.m_cmbGroup.Items.Add(grp)
            Next iGroup

            ' Update control
            Me.m_cmbGroup.SelectedIndex = Me.GroupIndex - 1

        End Sub

        Public Overrides Sub UpdateContent(ByVal editor As cLayerEditorRaster)
            MyBase.UpdateContent(editor)

            Me.m_cmbGroup.Enabled = Me.IsAttached

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

        Private Sub OnSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbGroup.SelectedIndexChanged
            Me.GroupIndex = Me.m_cmbGroup.SelectedIndex + 1
        End Sub

        Private Sub OnFormatItemText(sender As Object, e As System.Windows.Forms.ListControlConvertEventArgs) _
            Handles m_cmbGroup.Format
            Dim io As cCoreInputOutputBase = DirectCast(e.ListItem, cCoreInputOutputBase)
            Dim fmt As New cCoreInterfaceFormatter()
            e.Value = fmt.GetDescriptor(io)
        End Sub

    End Class

End Namespace

