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

Public Class frmSelectPreyPredator
    Inherits EwEResultsExtractor.CreateCollectionForData

    Public Event FormExited()

    Public Sub New(ByVal i As cSelectionData, ByRef p As cCore)
        MyBase.New(i, p)
        InitializeComponent()

        Me.Show()

    End Sub

    Public Overrides Sub PopulateAttachedList(ByVal Prey As String)
        Dim PreyIndex As Integer

        'Clear the list
        Me.chklstAttached.Items.Clear()

        If Prey = Nothing Then 'don't do anything

            Exit Sub

        Else 'else populate list

            ' Find the index number to refer to selected predator
            PreyIndex = 1
            Do While m_core.EcoSimGroupOutputs(PreyIndex).Name IsNot Prey
                PreyIndex += 1
            Loop

            'Check which functional groups are prey to given predator and add to prey chklist
            With Me.chklstAttached.Items
                For i As Integer = 1 To m_core.nGroups
                    If m_core.EcoPathGroupInputs(i).IsPrey(PreyIndex) Then
                        .Add(m_core.EcoSimGroupOutputs(i).Name)
                    End If
                Next
            End With

        End If

    End Sub

    Private Sub frmSelectPreyPredator_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        If frmResults.FireChecked = False Then
            frmResults.NextAction()
        End If
        RaiseEvent FormExited()
    End Sub
End Class