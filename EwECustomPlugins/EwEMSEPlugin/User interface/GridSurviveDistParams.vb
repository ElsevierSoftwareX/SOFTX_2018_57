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
Imports ScientificInterfaceShared.Style
Imports ScientificInterfaceShared.Properties

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Grid to allow species quota interaction.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class gridSurviveDistParameters
    Inherits EwEGrid

#Region " Internal defs "

    Private Enum eColumnTypes As Integer
        Index = 0
        FleetNumber
        FleetName
        GroupNumber
        GroupName
        Alpha
        Beta
    End Enum

    ''' <summary>The cMSE Plugin that contains the data.</summary>
    Private mMSEPlugin As cMSE
    Private m_data As List(Of cSurvivability.cSurvivabilityDistributonParam) = Nothing

#End Region ' Internal defs

#Region " Constructor "

    Public Sub New()
        MyBase.New()
    End Sub

#End Region ' Constructor

#Region " Public access "

    Public Sub Init(Plugin As cMSE, Survivability As cSurvivability)
        mMSEPlugin = Plugin
        m_data = Survivability.ListofSurvDistParams
    End Sub

    Public Property Data As List(Of cSurvivability.cSurvivabilityDistributonParam)
        Get
            Return Me.m_data
        End Get
        Set(value As List(Of cSurvivability.cSurvivabilityDistributonParam))
            Me.m_data = value
            Me.FillData()
        End Set

    End Property

    Public Event onEdited()

#End Region ' Public access

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Dim iNumCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length
        Me.Redim(1, iNumCols)

        Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.FleetNumber) = New EwEColumnHeaderCell(My.Resources.HEADER_FLEETNO)
        Me(0, eColumnTypes.FleetName) = New EwEColumnHeaderCell(SharedResources.HEADER_FLEETNAME)
        Me(0, eColumnTypes.GroupNumber) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNUM)
        Me(0, eColumnTypes.GroupName) = New EwEColumnHeaderCell(SharedResources.HEADER_NAME)
        Me(0, eColumnTypes.Alpha) = New EwEColumnHeaderCell(My.Resources.HEADER_ALPHA)
        Me(0, eColumnTypes.Beta) = New EwEColumnHeaderCell(My.Resources.HEADER_BETA)

        Me.FixedColumns = 5
        Me.FixedColumnWidths = False
        Me.AllowBlockSelect = False

    End Sub

    Protected Overrides Sub FillData()

        If (Me.m_data Is Nothing) Then Return
        If (Me.UIContext Is Nothing) Then Return

        Dim core As cCore = Me.UIContext.Core
        Dim pm As cPropertyManager = Me.UIContext.PropertyManager
        Dim iRow As Integer = -1
        Dim cell As EwECell = Nothing

        'Dim lstOptions As New List(Of cMSE.DistributionType)
        'lstOptions.AddRange(DirectCast([Enum].GetValues(GetType(cMSE.DistributionType)), IEnumerable(Of cMSE.DistributionType)))
        'Dim cb As EwEComboBoxCellEditor = New EwEComboBoxCellEditor(New cDistributionTypeFormatter(), lstOptions)

        Me.RowsCount = 1

        For i As Integer = 0 To Me.m_data.Count - 1

            iRow = Me.AddRow()
            Dim data As cSurvivability.cSurvivabilityDistributonParam = DirectCast(Me.m_data(i), cSurvivability.cSurvivabilityDistributonParam)

            Me(iRow, eColumnTypes.Index) = New EwERowHeaderCell(CStr(data.Index))
            Me(iRow, eColumnTypes.FleetNumber) = New EwERowHeaderCell(CStr(data.FleetNo))
            ' To Mark: property cells automatically keep track of changing variable values
            Me(iRow, eColumnTypes.FleetName) = New PropertyRowHeaderCell(pm, core.EcopathFleetInputs(data.FleetNo), eVarNameFlags.Name)
            Me(iRow, eColumnTypes.GroupNumber) = New EwERowHeaderCell(CStr(data.GroupNo))
            ' To Mark: property cells automatically keep track of changing variable values
            Me(iRow, eColumnTypes.GroupName) = New PropertyRowHeaderCell(pm, core.EcoPathGroupInputs(data.GroupNo), eVarNameFlags.Name)
            Me(iRow, eColumnTypes.Alpha) = DataCell(data.Alpha)
            Me(iRow, eColumnTypes.Beta) = DataCell(data.Beta)
            Me.Rows(iRow).Tag = data
        Next

        Me.Columns(eColumnTypes.Index).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.FleetNumber).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.FleetName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.GroupNumber).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.GroupName).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        'Me.AutoSizeColumn(eSurviveColumnTypes.Index, 20)
        'Me.AutoSizeColumn(eSurviveColumnTypes.FleetNumber, 50)
        'Me.AutoSizeColumn(eSurviveColumnTypes.FleetName, 50)
        'Me.AutoSizeColumn(eSurviveColumnTypes.GroupNumber, 50)
        'Me.AutoSizeColumn(eSurviveColumnTypes.GroupName, 50)
        'Me.AutoSizeColumn(eSurviveColumnTypes.Alpha, 50)
        'Me.AutoSizeColumn(eSurviveColumnTypes.Beta, 50)


        'Me.AutoSizeColumn(eSurviveColumnTypes.Name, 150)

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            ' To Mark: no need to respond to any core data changes
            Return eCoreComponentType.NotSet
        End Get
    End Property

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

    Protected Overrides Function OnCellEdited(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        Dim tag As Object = Me.Rows(p.Row).Tag
        If (tag Is Nothing) Then Return False

        Debug.Assert(TypeOf tag Is cSurvivability.cSurvivabilityDistributonParam)

        Dim data As cSurvivability.cSurvivabilityDistributonParam = DirectCast(tag, cSurvivability.cSurvivabilityDistributonParam)

        Select Case DirectCast(p.Column, eColumnTypes)
            Case eColumnTypes.Alpha
                data.Alpha = CDbl(cell.GetValue(p))
            Case eColumnTypes.Beta
                data.Beta = CDbl(cell.GetValue(p))
            Case Else
                ' NOP
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


