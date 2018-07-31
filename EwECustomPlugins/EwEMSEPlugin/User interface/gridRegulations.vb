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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.Cells
Imports EwEMSEPlugin.HCR_GroupNS

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Grid to allow species quota interaction.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class gridRegulations
    Inherits EwEGrid

#Region " Internal defs "

    Private Enum eColumnTypes As Byte
        FleetIndex = 0
        FleetName
        Method
    End Enum

    Private Class eRegMethodFormatter
        Implements ITypeFormatter

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(cRegulations.eRegMethod)
        End Function

        Public Function GetDescriptor(value As Object, Optional descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
            Implements ITypeFormatter.GetDescriptor

            Dim rm As cRegulations.eRegMethod = cRegulations.eRegMethod.None
            Try
                rm = DirectCast(value, cRegulations.eRegMethod)
            Catch ex As Exception
                rm = cRegulations.eRegMethod.None
            End Try
            Return cResourceUtils.LoadString("REGMETHOD_" & rm.ToString().ToUpper, Me.GetType.Assembly)
  
        End Function

    End Class

#End Region ' Internal defs

    ''' <summary>Selected strategy</summary>
    Private m_strategy As Strategy = Nothing

    Private m_editorMethod As EwEComboBoxCellEditor = Nothing

#Region " Constructor "

    Public Sub New()
        MyBase.New()
        ' Prepare editor
        Me.m_editorMethod = New EwEComboBoxCellEditor(New eRegMethodFormatter())
    End Sub

#End Region ' Constructor

#Region " Public access "

    Public Event onEdited()

    Public Property SelectedStrategy As Strategy
        Get
            Return Me.m_strategy
        End Get
        Set(value As Strategy)
            Me.m_strategy = value
            Me.RefreshContent()
        End Set
    End Property

#End Region ' Public access

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Dim iNumCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length
        Me.Redim(1, iNumCols)

        Me(0, eColumnTypes.FleetIndex) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.FleetName) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEETNAME)
        Me(0, eColumnTypes.Method) = New EwEColumnHeaderCell(SharedResources.HEADER_METHOD)

        Me.FixedColumns = 2
        Me.FixedColumnWidths = False
        Me.AllowBlockSelect = False

    End Sub

    Protected Overrides Sub FillData()

        If (Me.UIContext Is Nothing) Then Return
        If (Me.m_strategy Is Nothing) Then Return

        Dim iRow As Integer = -1
        Dim cell As EwECell = Nothing
        Dim core As cCore = Me.Core
        Dim reg As cRegulations = Me.m_strategy.Regulations
        Dim bIsTargeted(Me.Core.nFleets) As Boolean

        Me.RowsCount = 1

        ' Check all fleets that apply to the HCR of the selected strategy
        For i As Integer = 1 To Me.Core.nFleets
            Dim fleet As cEcopathFleetInput = Me.Core.EcopathFleetInputs(i)
            Dim bFished As Boolean = False

            For Each hcr As HCR_Group In Me.m_strategy
                If (hcr.GroupB IsNot Nothing) Then
                    bFished = (fleet.Landings(hcr.GroupB.Index) > 0) Or (fleet.Discards(hcr.GroupB.Index) > 0)
                End If

                If (bFished) Then
                    bIsTargeted(i) = True
                    Exit For
                End If
            Next hcr
        Next i

        For i As Integer = 1 To core.nFleets
            iRow = Me.AddRow()

            Dim fleet As cEcopathFleetInput = Me.Core.EcopathFleetInputs(i)
            Me(iRow, eColumnTypes.FleetIndex) = New EwERowHeaderCell(CStr(fleet.Index))
            Me(iRow, eColumnTypes.FleetName) = New EwERowHeaderCell(CStr(fleet.Name))

            If (bIsTargeted(i)) Then
                Me(iRow, eColumnTypes.Method) = New SourceGrid2.Cells.Real.Cell(reg.Method(i), Me.m_editorMethod)
                Me(iRow, eColumnTypes.Method).Behaviors.Add(Me.EwEEditHandler)
            Else
                Me(iRow, eColumnTypes.Method) = New EwECell("", GetType(String), cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable)
            End If

        Next

        Me.Columns(eColumnTypes.FleetIndex).AutoSizeMode = SourceGrid2.AutoSizeMode.None
        Me.Columns(eColumnTypes.FleetName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.Method).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.AutoSizeColumn(eColumnTypes.Method, 150)

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        Me.Columns(eColumnTypes.FleetIndex).Width = 20
    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.NotSet
        End Get
    End Property

    Protected Overrides Function OnCellEdited(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        If (Not MyBase.OnCellEdited(p, cell)) Then Return False
        If (Me.m_strategy Is Nothing) Then Return False
        Dim reg As cRegulations = Me.m_strategy.Regulations
        If (reg Is Nothing) Then Return False

        ' Check column
        If (p.Column = eColumnTypes.Method) Then
            ' Store value
            reg.Method(p.Row) = CType(cell.GetValue(p), cRegulations.eRegMethod)
        End If

        ' Yippee
        Me.RaiseDataChangeEvent()

        ' Done
        Return True

    End Function

#End Region ' Overrides

    Private Sub RaiseDataChangeEvent()
        Try
            RaiseEvent onEdited()
        Catch ex As Exception

        End Try
    End Sub

End Class
