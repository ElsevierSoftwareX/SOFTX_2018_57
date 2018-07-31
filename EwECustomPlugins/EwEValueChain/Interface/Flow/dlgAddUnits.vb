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
Imports System.Windows.Forms

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Dialog for adding existing units to a flow diagram.
''' </summary>
''' ===========================================================================
Public Class dlgAddUnits

#Region " Private class "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class to list a single unit in the unit list box
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Class cUnitListBoxItem

        Private m_unit As cUnit = Nothing

        Public Sub New(ByVal unit As cUnit)
            Me.m_unit = unit
        End Sub

        Public Overrides Function ToString() As String
            Return Me.m_unit.Name
        End Function

        ReadOnly Property Unit() As cUnit
            Get
                Return Me.m_unit
            End Get
        End Property

    End Class

#End Region ' Private class

#Region " Privates "

    ''' <summary>Main data.</summary>
    Private m_data As cData = Nothing
    ''' <summary>The diagram to add units to.</summary>
    Private m_diagram As cFlowDiagram = Nothing

#End Region ' Privates

#Region " Construction / destruction "

    Public Sub New(ByVal data As cData, ByVal diagram As cFlowDiagram)
        Me.InitializeComponent()
        Me.m_data = data
        Me.m_diagram = diagram
    End Sub

#End Region ' Construction / destruction

#Region " Control events "

    Private Sub dlgSelectUnits_Load(ByVal sender As Object, ByVal e As System.EventArgs) _
        Handles Me.Load

        Dim unit As cUnit = Nothing
        Dim afp As cFlowPosition() = Me.m_data.FlowPositions(Me.m_diagram) ' All flow positions for the current diagram
        Dim bFound As Boolean = False

        ' For all available units
        For iUnit As Integer = 0 To Me.m_data.UnitCount - 1

            ' Get unit
            unit = Me.m_data.Unit(iUnit)
            ' Determine if this unit is part of the diagram already
            bFound = False
            ' Is part of this diagram?
            For Each fp As cFlowPosition In afp
                If ReferenceEquals(fp.Unit, unit) Then bFound = True : Exit For
            Next

            ' Unit not used in current diagram yet?
            If (bFound = False) Then
                ' #Yes: add it to the list box
                Me.m_clbUnits.Items.Add(New cUnitListBoxItem(unit))
            End If

        Next iUnit

    End Sub

    Private Sub OnSelectAll(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnAll.Click
        For iItem As Integer = 0 To Me.m_clbUnits.Items.Count - 1
            Me.m_clbUnits.SetItemChecked(iItem, True)
        Next
    End Sub

    Private Sub OnSelectNone(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnSelNone.Click
        For iItem As Integer = 0 To Me.m_clbUnits.Items.Count - 1
            Me.m_clbUnits.SetItemChecked(iItem, False)
        Next
    End Sub

    Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles OK_BUTTON.Click

        Dim unit As cUnit = Nothing

        ' For all items
        For iItem As Integer = 0 To Me.m_clbUnits.Items.Count - 1
            ' Is checked?
            If Me.m_clbUnits.GetItemChecked(iItem) Then
                ' #Yes: get underlying unit
                unit = DirectCast(Me.m_clbUnits.Items(iItem), cUnitListBoxItem).Unit
                ' Generate a flow position for this unit
                Me.m_data.CreateFlowPosition(unit, Me.m_diagram)
            End If
        Next

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles CANCEL_BUTTON.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

#End Region ' Control events

End Class