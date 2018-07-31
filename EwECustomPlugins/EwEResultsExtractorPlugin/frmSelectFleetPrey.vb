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

Public Class frmSelectFleetPrey
    Inherits EwEResultsExtractor.CreateCollectionForData

    Public Event FormExited()

    Public Sub New(ByVal i As cSelectionData, ByVal p As cCore)
        MyBase.New(i, p)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.Show()

    End Sub

    Public Overrides Sub PopulateAttachedList(ByVal Fleet As String)

        Dim FleetIndex As Integer
        Dim TotalCatch As Single

        'Clear the list
        Me.chklstAttached.Items.Clear()

        If Fleet = Nothing Then 'don't do anything

            Exit Sub

        Else 'else populate list

            ' Find the index number to refer to selected fleet
            FleetIndex = 0
            Do While m_core.EcosimFleetOutput(FleetIndex).Name IsNot Fleet
                FleetIndex += 1
            Loop

            'Check which functional groups are prey to given fleet and add to prey chklist
            With Me.chklstAttached.Items
                For i As Integer = 1 To m_core.nGroups
                    TotalCatch = 0
                    For p = 0 To m_core.nEcosimTimeSteps
                        TotalCatch += m_core.EcoSimGroupOutputs(i).CatchByFleet(FleetIndex, p)
                    Next
                    If TotalCatch > 0 Then
                        .Add(m_core.EcoSimGroupOutputs(i).Name)
                    End If
                Next
            End With

        End If

    End Sub

    Private Sub frmSelectFleetPrey_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        If frmResults.FireChecked = False Then
            frmResults.NextAction()
        End If
        RaiseEvent FormExited()
    End Sub
End Class