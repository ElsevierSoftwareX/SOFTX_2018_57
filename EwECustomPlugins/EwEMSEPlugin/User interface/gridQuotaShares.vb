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
Imports EwECore.MSE
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.Cells
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style


#End Region ' Imports



''' ===========================================================================
''' <summary>
''' Grid to allow species quota interaction.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class gridQuotaShares
    Inherits EwEGrid


#Region " Internal defs "

    Private Enum eColumnTypes As Integer
        GroupNumber = 0
        GroupName
        FleetNumber
        FleetName
        QuotaShare
    End Enum

#End Region ' Internal defs

    Private mMSEPlugin As cMSE
    Private m_data As List(Of cQuotaShares.QuotaShare) = Nothing

#Region " Constructor "

    Public Sub New()
        MyBase.new()

    End Sub

#End Region ' Constructor

#Region " Public access "

    Public Sub Init(Plugin As cMSE, data As cQuotaShares)
        mMSEPlugin = Plugin
        Me.m_data = data.GetLstGrpShares
        Me.RefreshContent()
    End Sub

    Public Event onEdited()

    Public Property Data As List(Of cQuotaShares.QuotaShare)
        Get
            Return Me.m_data
        End Get
        Set(value As List(Of cQuotaShares.QuotaShare))
            Me.m_data = value
            Me.FillData()
        End Set

    End Property

#End Region ' Public access

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Dim iNumCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length

        Me.Redim(1, iNumCols)

        Me(0, eColumnTypes.GroupNumber) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNUM)
        Me(0, eColumnTypes.GroupName) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
        Me(0, eColumnTypes.FleetNumber) = New EwEColumnHeaderCell(My.Resources.HEADER_FLEETNUMBER)
        Me(0, eColumnTypes.FleetName) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEETNAME)
        Me(0, eColumnTypes.QuotaShare) = New EwEColumnHeaderCell(My.Resources.HEADER_QUOTASHARE)

        Me.FixedColumns = 2
        Me.FixedColumnWidths = False

    End Sub

    Private Function DataCell(dValue As Double) As EwECell

        Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
        Dim cell As EwECell = Nothing

        ' To Mark: Ok, so when the data is NULL it will always be NULL and cannot be changed?! 
        '          I suspect that you still want users to be able to change the cell value away from
        '          cCore.NULL; in that case remove the 'NotEditable' style.
        If (dValue = cCore.NULL_VALUE) Then
            style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null
        End If

        cell = New EwECell(CSng(dValue), GetType(Single), style)
        cell.Behaviors.Add(Me.EwEEditHandler)
        Return cell

    End Function

    Protected Overrides Sub FillData()

        If (Me.m_data Is Nothing) Then Return
        If (Me.UIContext Is Nothing) Then Return

        Dim core As cCore = Me.UIContext.Core
        Dim pm As cPropertyManager = Me.UIContext.PropertyManager
        Dim iRow As Integer = -1
        Dim cell As EwECell = Nothing
        Dim lstShares As List(Of cQuotaShares.QuotaShare) = m_data

        'Dim lstOptions As New List(Of cMSE.DistributionType)
        'lstOptions.AddRange(DirectCast([Enum].GetValues(GetType(cMSE.DistributionType)), IEnumerable(Of cMSE.DistributionType)))
        'Dim cb As EwEComboBoxCellEditor = New EwEComboBoxCellEditor(New cDistributionTypeFormatter(), lstOptions)

        Me.RowsCount = 1

        For i As Integer = 0 To lstShares.Count - 1

            iRow = Me.AddRow()
            Dim data As cQuotaShares.QuotaShare = DirectCast(lstShares(i), cQuotaShares.QuotaShare)

            Me(iRow, eColumnTypes.GroupNumber) = New EwERowHeaderCell(CStr(data.mGroupNo))
            Me(iRow, eColumnTypes.GroupName) = New PropertyRowHeaderCell(pm, core.EcoPathGroupInputs(data.mGroupNo), eVarNameFlags.Name)
            ' To Mark: property cells automatically keep track of changing variable values
            Me(iRow, eColumnTypes.FleetNumber) = New EwERowHeaderCell(CStr(data.mFleetNo))
            Me(iRow, eColumnTypes.FleetName) = New PropertyRowHeaderCell(pm, core.EcopathFleetInputs(data.mFleetNo), eVarNameFlags.Name)
            ' To Mark: property cells automatically keep track of changing variable values
            Me(iRow, eColumnTypes.QuotaShare) = DataCell(data.mShare)
            Me.Rows(iRow).Tag = data
        Next

        Me.Columns(eColumnTypes.GroupNumber).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.GroupName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.FleetNumber).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.FleetName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.QuotaShare).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        ' JS: keep at default for quickedit handler
        'Me.Selection.SelectionMode = GridSelectionMode.Row
    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.MSE
        End Get
    End Property

    Protected Overrides Function OnCellEdited(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        Dim tag As Object = Me.Rows(p.Row).Tag
        If (tag Is Nothing) Then Return False

        Debug.Assert(TypeOf tag Is cQuotaShares.QuotaShare)

        Dim data As cQuotaShares.QuotaShare = DirectCast(tag, cQuotaShares.QuotaShare)

        Select Case DirectCast(p.Column, eColumnTypes)
            Case eColumnTypes.QuotaShare
                data.mShare = CSng(cell.GetValue(p))
        End Select

        ' To Mark: First complete the edit, then notify the world. It was the other way around
        MyBase.OnCellEdited(p, cell)

        ' To Mark: I've added a 'lazy notification' to be fired after the entire celll edit bit has completed.
        'Me.RaiseDataChangeEvent()
        Me.BeginInvoke(New MethodInvoker(AddressOf RaiseDataChangeEvent))

        Return True

    End Function

    Private Sub RaiseDataChangeEvent()
        Try
            RaiseEvent onEdited()
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Overrides

End Class


