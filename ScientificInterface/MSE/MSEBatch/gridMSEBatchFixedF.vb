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
Public Class gridMSEBatchFixedF
    Inherits EwEGrid

    ' ToDo: Globalize this class 
    ' ToDo: Add XML comments

    Private m_iter As Integer

#Region " Internal defs "

    Private Enum eColumnTypes As Integer
        Index = 0
        Name
        RunType
        FixedF
        FixedFValue
        FixedFLow
        FixedFUp
    End Enum

#End Region ' Internal defs

    Public Sub New()
        MyBase.new()
        Me.m_iter = 1
    End Sub

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Dim iNumCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length

        Me.Redim(1, iNumCols)
        Dim limitStr As String = "%"
        If Me.UIContext IsNot Nothing Then
            'UIContext can be nothing in the development enviro
            If Me.UIContext.Core.MSEBatchManager.Parameters.IterCalcType = eMSEBatchIterCalcTypes.UpperLowerValues Then
                limitStr = "Value"
            End If
        End If

        Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
        Me(0, eColumnTypes.RunType) = New EwEColumnHeaderCell("Managed via Fishing Mort.")

        Me(0, eColumnTypes.FixedF) = New EwEColumnHeaderCell("Fixed F") 'B lim(-)
        Me(0, eColumnTypes.FixedFValue) = New EwEColumnHeaderCell("Iter.(" & Me.iCurIter.ToString & ")")
        Me(0, eColumnTypes.FixedFLow) = New EwEColumnHeaderCell("Lower " & limitStr) 'B lim(-)
        Me(0, eColumnTypes.FixedFUp) = New EwEColumnHeaderCell("Upper " & limitStr) 'B Lim(+)


        Me.FixedColumns = 2
        Me.FixedColumnWidths = False

    End Sub

    Protected Overrides Sub FillData()

        Dim group As MSE.cMSEBatchFGroup
        Dim RowStyle As cStyleGuide.eStyleFlags

        For iGroup As Integer = 1 To Core.nLivingGroups

            'Get the group info
            group = Core.MSEBatchManager.FixedFGroups(iGroup)

            Me.AddRow()

            RowStyle = DirectCast(group.GetStatus(eVarNameFlags.MSEFixedF), cStyleGuide.eStyleFlags)
            Me(iGroup, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iGroup))
            Me(iGroup, eColumnTypes.Name) = New PropertyRowHeaderCell(Me.PropertyManager, group, eVarNameFlags.Name)

            ' ToDo: replace by property style
            Me(iGroup, eColumnTypes.RunType) = New PropertyCheckboxCell(Me.PropertyManager, group, eVarNameFlags.MSEBatchFManaged)
            'Me(iGroup, eColumnTypes.RunType) = New EwECheckboxCell(group.isManaged, RowStyle)
            Me(iGroup, eColumnTypes.RunType).Behaviors.Add(Me.EwEEditHandler)

            Me(iGroup, eColumnTypes.FixedF) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.MSEFixedF)

            Me(iGroup, eColumnTypes.FixedFValue) = New EwECell(group.FixedFValue(iCurIter), GetType(Single), RowStyle)
            Me(iGroup, eColumnTypes.FixedFValue).Behaviors.Add(Me.EwEEditHandler)

            Me(iGroup, eColumnTypes.FixedFLow) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.MSEBatchFLower)
            Me(iGroup, eColumnTypes.FixedFUp) = New PropertyCell(Me.PropertyManager, group, eVarNameFlags.MSEBatchFUpper)


        Next iGroup

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


    Public Property iCurIter As Integer
        Get
            Return m_iter
        End Get

        Set(value As Integer)
            If Me.UIContext IsNot Nothing Then
                If value <= Me.UIContext.Core.MSEBatchManager.Parameters.nFixedFIteration Then
                    m_iter = value
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
        Dim iGrp As Integer
        Dim ColType As eColumnTypes

        Try

            Dim val As Object = Me(p.Row, p.Column).Value
            iGrp = p.Row
            ColType = CType(p.Column, eColumnTypes)

            Select Case ColType
                Case eColumnTypes.FixedFValue
                    Core.MSEBatchManager.FixedFGroups(iGrp).FixedFValue(Me.iCurIter) = CSng(val)
            End Select

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".OnCellEdited() Exception " & ex.Message)
        End Try

        Return True

    End Function

    Protected Overrides Function OnCellValueChanged(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean


    End Function

#End Region ' Overrides


End Class
