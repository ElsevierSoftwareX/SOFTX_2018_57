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

#End Region ' Imports

Namespace Controls.Map.Layers

    ''' <summary>
    ''' Visual editor for <see cref="cLayerEditorHabitat"/>
    ''' </summary>
    Public Class ucLayerEditorHabitat

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)
            Me.m_cbUseHabitatAreaCorrection.Checked = Me.UIContext.StyleGuide.UseHabitatAreaCorrection
            Me.UpdateEditor()
        End Sub

        Protected Overloads Property Editor() As cLayerEditorHabitat
            Get
                Return DirectCast(MyBase.Editor, cLayerEditorHabitat)
            End Get
            Set(ByVal editor As cLayerEditorHabitat)
                ' Sanity check
                Debug.Assert(TypeOf editor Is cLayerEditorHabitat, "ucLayerEditorHabitat connected to wrong editor class")
                ' Set
                MyBase.Editor = editor
                Me.UpdateEditor()
            End Set
        End Property

        Private Sub OnUseHabitatAreaCorrectionChanged(sender As Object, e As EventArgs) Handles m_cbUseHabitatAreaCorrection.CheckedChanged
            If (Me.IsAttached) Then
                Me.UIContext.StyleGuide.UseHabitatAreaCorrection = Me.m_cbUseHabitatAreaCorrection.Checked
                Me.UpdateEditor()
            End If
        End Sub

        Private Sub UpdateEditor()
            If (Me.IsAttached) Then
                Me.Editor.UseHabitatAreaCorrection = Me.UIContext.StyleGuide.UseHabitatAreaCorrection
            End If
        End Sub

    End Class

End Namespace
