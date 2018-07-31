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
Option Explicit On

Imports EwECore
Imports EwECore.MSE
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.Cells
Imports SourceGrid2.Cells.Real

#End Region

<CLSCompliant(False)> _
Public Class gridMSEBatchTFMIter
    Inherits EwEGrid

    ' ToDo: Globalize this class 
    ' ToDo: Add XML comments

    Private m_iSelGroup As Integer

#Region " Internal defs "

    Private Enum eColumnTypes As Integer
        Index = 0
        Name
        BLim
        BBase
        FOpt
    End Enum

#End Region ' Internal defs


    Public Sub New()
        MyBase.new()
        Me.m_iSelGroup = 1
    End Sub

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Dim iNumCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length

        Me.Redim(1, iNumCols)

        Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME & "-iteration# ")
        Me(0, eColumnTypes.BLim) = New EwEColumnHeaderCell("Biomass limit") 'B lim(-)
        Me(0, eColumnTypes.BBase) = New EwEColumnHeaderCell("Biomass base")
        Me(0, eColumnTypes.FOpt) = New EwEColumnHeaderCell("F max.")

        Me.FixedColumns = 2
        Me.FixedColumnWidths = False

    End Sub

    Protected Overrides Sub FillData()

        Dim group As MSE.cMSEBatchTFMGroup = Nothing

        ' For each group
        For iParIter As Integer = 1 To Core.MSEBatchManager.Parameters.nTFMIteration

            'Get the group info
            group = Core.MSEBatchManager.TFMGroups(iSelGroup)

            Me.AddRow()

            Me(iParIter, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iParIter))
            ' Me(iParIter, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)
            Me(iParIter, eColumnTypes.Name) = New EwECell(group.Name & " " & iParIter.ToString, GetType(String), cStyleGuide.eStyleFlags.Names)

            Me(iParIter, eColumnTypes.BLim) = New EwECell(group.BLimValue(iParIter), GetType(Single))
            Me(iParIter, eColumnTypes.BLim).Behaviors.Add(Me.EwEEditHandler)

            Me(iParIter, eColumnTypes.BBase) = New EwECell(group.BBaseValue(iParIter), GetType(Single))
            Me(iParIter, eColumnTypes.BBase).Behaviors.Add(Me.EwEEditHandler)

            Me(iParIter, eColumnTypes.FOpt) = New EwECell(group.FMaxValue(iParIter), GetType(Single))
            Me(iParIter, eColumnTypes.FOpt).Behaviors.Add(Me.EwEEditHandler)

        Next iParIter

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        Me.Selection.SelectionMode = GridSelectionMode.Row
    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.MSE
        End Get
    End Property


    Public Property iSelGroup As Integer
        Get
            Return m_iSelGroup
        End Get

        Set(value As Integer)

            If Me.UIContext IsNot Nothing Then
                If value <= Me.UIContext.Core.nGroups Then
                    m_iSelGroup = value
                    Me.RefreshContent()
                End If
            End If

        End Set

    End Property




    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Called when the user has finished editing a cell. Handled to update 
    ''' local admin based on cell value changes.
    ''' </summary>
    ''' <returns>
    ''' True if the edit operation is allowed, False to cancel the edit operation.
    ''' </returns>
    ''' <remarks>
    ''' This method differs from OnCellValueChanged; at the end of an edit
    ''' operation it is once again safe to alter the value of the cell that was
    ''' just edited for text and combo box controls. *sigh*
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function OnCellEdited(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean
        Try

            Dim val As Object = Me(p.Row, p.Column).Value
            Dim iIter As Integer = p.Row
            Dim igrp As Integer = Me.iSelGroup

            Dim group As MSE.cMSEBatchTFMGroup = Core.MSEBatchManager.TFMGroups(iSelGroup)

            Select Case CType(p.Column, eColumnTypes)

                Case eColumnTypes.BLim
                    group.BLimValue(iIter) = CSng(val)

                Case eColumnTypes.BBase
                    group.BBaseValue(iIter) = CSng(val)

                Case eColumnTypes.FOpt
                    group.FMaxValue(iIter) = CSng(val)

            End Select

        Catch ex As Exception

        End Try

        Return True

    End Function


#End Region ' Overrides


End Class
