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
Imports EwEUtils.Core

#End Region ' Imports

Namespace Controls.Map.Layers

    Public Class ucLayerEditorMigration

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_btnNext.Image = My.Resources.PlayStepHS
            Me.m_btnNext.Text = ""

            Me.UpdateContent(Me.Editor)
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try

        End Sub

        Public Overrides Sub UpdateContent(ByVal editor As cLayerEditorRaster)
            MyBase.UpdateContent(editor)

            If (Me.UIContext Is Nothing) Then Return
            If (Me.m_cmbMonth Is Nothing) Then Return

            If (editor IsNot Nothing) Then
                Me.m_bInUpdate = True
                Try
                    Dim layer As cEcospaceLayer = Me.Editor.Layer.Data
                    Dim grp As cEcospaceGroupInput = Me.UIContext.Core.EcospaceGroupInputs(layer.Index)

                    Me.m_cmbMonth.SelectedIndex = Math.Max(0, Me.Editor.Month - 1)
                Catch ex As Exception

                End Try
                Me.m_bInUpdate = False
            End If

        End Sub

        Protected Overloads Property Editor() As cLayerEditorMigration
            Get
                Return DirectCast(MyBase.Editor, cLayerEditorMigration)
            End Get
            Set(ByVal editor As cLayerEditorMigration)
                ' Sanity check
                Debug.Assert(TypeOf editor Is cLayerEditorMigration, "ucLayerEditorMigration connected to wrong editor class")
                ' Set
                MyBase.Editor = editor
            End Set
        End Property

#Region " Event handlers "

        Private Sub OnMonthChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbMonth.SelectedIndexChanged
            Try
                Me.Editor.Month = Me.m_cmbMonth.SelectedIndex + 1
            Catch ex As Exception
                cLog.Write(ex, "ucLayerMigration.OnMonthChanged()")
            End Try
        End Sub

        Private Sub OnNextMonth(sender As System.Object, e As System.EventArgs) _
            Handles m_btnNext.Click
            Try
                Me.Editor.Next()
                Me.UpdateContent(Me.Editor)
            Catch ex As Exception
                cLog.Write(ex, "ucLayerMigration.OnNextMonth()")
            End Try
         End Sub

#End Region ' Event handlers

    End Class

End Namespace

