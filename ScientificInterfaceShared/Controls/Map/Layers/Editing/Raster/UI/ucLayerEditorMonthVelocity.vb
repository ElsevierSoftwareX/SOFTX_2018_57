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
Imports EwEUtils.Utilities

Namespace Controls.Map.Layers

    Public Class ucLayerEditorMonthVelocity

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)

            ' Populate month dropdown
            Me.m_cmbMonth.Items.Clear()
            For i As Integer = 1 To cCore.N_MONTHS
                Me.m_cmbMonth.Items.Add(cDateUtils.GetMonthName(i))
            Next
            Me.m_cmbMonth.SelectedIndex = Me.Month - 1

        End Sub

        Private Sub OnMonthSelected(sender As Object, e As EventArgs) _
            Handles m_cmbMonth.SelectedIndexChanged

            Me.Month = Me.m_cmbMonth.SelectedIndex + 1

        End Sub

        Private Sub OnCopyMap(sender As Object, e As EventArgs) _
            Handles m_btnCopy.Click

            Try
                Me.Editor.Duplicate(Me.Month)
            Catch ex As Exception
                ' Whoah!
                Debug.Assert(False, ex.Message)
            End Try

        End Sub

        Public Overrides Sub UpdateContent(ByVal editor As cLayerEditorRaster)
            MyBase.UpdateContent(editor)

            Me.m_cmbMonth.Enabled = Me.IsAttached
            Me.m_btnCopy.Enabled = Me.Editor.CanDuplicate()

            If (Me.m_cmbMonth.Items.Count > 0) Then
                Me.m_cmbMonth.SelectedIndex = Me.Month - 1
            End If

        End Sub

        Protected Overloads Property Editor() As cLayerEditorVelocity
            Get
                Return DirectCast(MyBase.Editor, cLayerEditorVelocity)
            End Get
            Set(ByVal editor As cLayerEditorVelocity)
                ' Sanity check
                Debug.Assert(TypeOf editor Is cLayerEditorVelocity, "ucLayerEditorGroup connected to wrong editor class")
                ' Configure editor
                editor.CellValue = 20
                ' Set
                MyBase.Editor = editor
            End Set
        End Property

        Protected Property Month() As Integer
            Get
                If (Not Me.IsAttached) Then Return cCore.NULL_VALUE
                Return Me.Editor.Month
            End Get
            Set(ByVal value As Integer)
                If (Me.IsAttached) Then
                    If (Me.Editor.Month <> value) Then
                        Me.Editor.Month = value
                    End If
                End If
            End Set
        End Property
    End Class

End Namespace

