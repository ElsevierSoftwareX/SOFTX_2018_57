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

#End Region ' Imports

' ToDo: Change col labels to lower, upper and midpoint
' ToDo: Midpoint should be enabled when distr = Triangular 

''' ===========================================================================
''' <summary>
''' Grid to allow species quota interaction.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class gridDistributionParameters
    Inherits EwEGrid

#Region " Internal defs "

    Private Class cDistributionTypeFormatter
        Implements ITypeFormatter

        Public Function GetDescribedType() As System.Type Implements ITypeFormatter.GetDescribedType
            Return GetType(cMSE.DistributionType)
        End Function

        Public Function GetDescriptor(value As Object, Optional descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
            Implements ITypeFormatter.GetDescriptor
            Select Case (DirectCast(value, cMSE.DistributionType))
                Case cMSE.DistributionType.NotSet
                    Return SharedResources.GENERIC_VALUE_NOTUSED
                Case cMSE.DistributionType.Triangular
                    Return My.Resources.DISTR_TYPE_TRIANGULAR
                Case cMSE.DistributionType.Uniform
                    Return My.Resources.DISTR_TYPE_UNIFORM
                Case Else
                    Debug.Assert(False)
            End Select
            Return "?"
        End Function

    End Class

    Private Enum eEcopathColumnTypes As Integer
        Index = 0
        Name
        Mean
        CV
        Upper
        Lower
    End Enum

    Private Enum eEcosimColumnTypes As Integer
        Index = 0
        Name
        DistrType
        Lower
        Upper
        MidPoint
    End Enum

#End Region ' Internal defs

    ''' <summary>The cMSE Plugin that contains the data.</summary>
    Private MSEPlugin As cMSE
    Private m_data As IDistributionParamsData() = Nothing
    Private m_mode As frmDistributionParameters.eParameterSet

#Region " Constructor "

    Public Sub New()
        MyBase.New()
    End Sub

#End Region ' Constructor

#Region " Public access "

    Public Sub Init(Plugin As cMSE)
        MSEPlugin = Plugin
    End Sub

    Public Property Mode As frmDistributionParameters.eParameterSet
        Get
            Return Me.m_mode
        End Get
        Set(value As frmDistributionParameters.eParameterSet)
            If (Me.m_mode <> value) Then
                Me.m_mode = value
                Me.m_data = Nothing
                Me.RefreshContent()
            End If
        End Set
    End Property

    Public Property Data As IDistributionParamsData()
        Get
            Return Me.m_data
        End Get
        Set(value As IDistributionParamsData())
            Me.m_data = value
            Me.FillData()
        End Set
    End Property

    Public Event onEdited()

#End Region ' Public access

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Select Case Me.m_mode
            Case frmDistributionParameters.eParameterSet.Ecopath
                Dim iNumCols As Integer = [Enum].GetValues(GetType(eEcopathColumnTypes)).Length
                Me.Redim(1, iNumCols)

                Me(0, eEcopathColumnTypes.Index) = New EwEColumnHeaderCell("")
                Me(0, eEcopathColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_NAME)
                Me(0, eEcopathColumnTypes.Mean) = New EwEColumnHeaderCell(SharedResources.HEADER_MEAN)
                Me(0, eEcopathColumnTypes.CV) = New EwEColumnHeaderCell(SharedResources.HEADER_CV)
                Me(0, eEcopathColumnTypes.Lower) = New EwEColumnHeaderCell(My.Resources.HEADER_BOUND_LOWER)
                Me(0, eEcopathColumnTypes.Upper) = New EwEColumnHeaderCell(My.Resources.HEADER_BOUND_UPPER)

            Case frmDistributionParameters.eParameterSet.Ecosim
                Dim iNumCols As Integer = [Enum].GetValues(GetType(eEcosimColumnTypes)).Length
                Me.Redim(1, iNumCols)

                Me(0, eEcosimColumnTypes.Index) = New EwEColumnHeaderCell("")
                Me(0, eEcosimColumnTypes.Name) = New EwEColumnHeaderCell(SharedResources.HEADER_NAME)
                Me(0, eEcosimColumnTypes.DistrType) = New EwEColumnHeaderCell(My.Resources.HEADER_DISTRIBUTIONTYPE)
                Me(0, eEcosimColumnTypes.Lower) = New EwEColumnHeaderCell(My.Resources.HEADER_BOUND_LOWER)
                Me(0, eEcosimColumnTypes.Upper) = New EwEColumnHeaderCell(My.Resources.HEADER_BOUND_UPPER)
                Me(0, eEcosimColumnTypes.MidPoint) = New EwEColumnHeaderCell(My.Resources.HEADER_MIDPOINT)

        End Select

        Me.FixedColumns = 2
        Me.FixedColumnWidths = False
        Me.AllowBlockSelect = False

    End Sub

    Protected Overrides Sub FillData()

        If (Me.m_data Is Nothing) Then Return

        Dim iRow As Integer = -1
        Dim cell As EwECell = Nothing
        Dim comboEditor As EwEComboBoxCellEditor = New EwEComboBoxCellEditor(New cDistributionTypeFormatter())

        Me.RowsCount = 1

        For i As Integer = 0 To Me.m_data.Length - 1
            iRow = Me.AddRow()
            Select Case Me.m_mode
                Case frmDistributionParameters.eParameterSet.Ecopath

                    Dim data As cEcopathDistributionParamsData = DirectCast(Me.m_data(i), cEcopathDistributionParamsData)
                    Dim group As cEcoPathGroupInput = Me.Core.EcoPathGroupInputs(data.GroupNo)
                    Dim sg As cStanzaGroup = Nothing
                    Dim bUse As Boolean = True

                    If group.IsMultiStanza Then
                        sg = Me.Core.StanzaGroups(group.iStanza)
                        bUse = (sg.iGroups(sg.LeadingB) = data.GroupNo)
                    End If

                    Me(iRow, eEcopathColumnTypes.Index) = New EwERowHeaderCell(CStr(data.GroupNo))
                    Me(iRow, eEcopathColumnTypes.Name) = New EwERowHeaderCell(CStr(data.GroupName))

                    If (bUse) Then
                        Me(iRow, eEcopathColumnTypes.CV) = DataCell(data.CV)
                        Me(iRow, eEcopathColumnTypes.Mean) = DataCell(data.Mean, cStyleGuide.eStyleFlags.NotEditable)
                        Me(iRow, eEcopathColumnTypes.Lower) = DataCell(data.LowerBound)
                        Me(iRow, eEcopathColumnTypes.Upper) = DataCell(data.UpperBound)
                    Else
                        Me(iRow, eEcopathColumnTypes.CV) = New EwECell(cCore.NULL_VALUE, GetType(Single), cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable)
                        Me(iRow, eEcopathColumnTypes.Mean) = New EwECell(cCore.NULL_VALUE, GetType(Single), cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable)
                        Me(iRow, eEcopathColumnTypes.Lower) = New EwECell(cCore.NULL_VALUE, GetType(Single), cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable)
                        Me(iRow, eEcopathColumnTypes.Upper) = New EwECell(cCore.NULL_VALUE, GetType(Single), cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable)
                    End If

                    Me.Rows(iRow).Tag = data

                Case frmDistributionParameters.eParameterSet.Ecosim
                    Dim data As cEcosimDistributionParamsData = DirectCast(Me.m_data(i), cEcosimDistributionParamsData)

                    Me(iRow, eEcosimColumnTypes.Index) = New EwERowHeaderCell(CStr(data.GroupNo))
                    Me(iRow, eEcosimColumnTypes.Name) = New EwERowHeaderCell(CStr(data.GroupName))

                    Dim cbCell As ICell = New SourceGrid2.Cells.Real.Cell(data.DistributionType, comboEditor)
                    Me(iRow, eEcosimColumnTypes.DistrType) = cbCell
                    cbCell.Behaviors.Add(Me.EwEEditHandler)

                    'If data.DistributionType = cMSE.DistributionType.NotSet Then
                    '    cbCell.DataModel.EnableEdit = False
                    'End If

                    Me(iRow, eEcosimColumnTypes.Lower) = DataCell(data.LowerBound)
                    Me(iRow, eEcosimColumnTypes.Upper) = DataCell(data.UpperBound)
                    Me(iRow, eEcosimColumnTypes.MidPoint) = DataCell(data.MidPoint)
                    Me.Rows(iRow).Tag = data

            End Select
        Next

        Select Case Me.m_mode
            Case frmDistributionParameters.eParameterSet.Ecopath
                Me.Columns(eEcopathColumnTypes.Index).AutoSizeMode = SourceGrid2.AutoSizeMode.None
                Me.Columns(eEcopathColumnTypes.Name).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize Or SourceGrid2.AutoSizeMode.EnableStretch
                Me.AutoSizeColumn(eEcopathColumnTypes.Name, 150)
            Case frmDistributionParameters.eParameterSet.Ecosim
                Me.Columns(eEcosimColumnTypes.Index).AutoSizeMode = SourceGrid2.AutoSizeMode.None
                Me.Columns(eEcosimColumnTypes.Name).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize Or SourceGrid2.AutoSizeMode.EnableStretch
                Me.AutoSizeColumn(eEcosimColumnTypes.Name, 150)
        End Select

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.EcoSim
        End Get
    End Property

    Private Function DataCell(dValue As Double, Optional style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK) As EwECell

        Dim cell As EwECell = Nothing

        If (dValue = cCore.NULL_VALUE) Then
            style = cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Null
        End If

        cell = New EwECell(CSng(dValue), GetType(Single), style)
        cell.Behaviors.Add(Me.EwEEditHandler)
        Return cell

    End Function

    Protected Overrides Function OnCellEdited(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean

        Select Case Me.Mode

            Case frmDistributionParameters.eParameterSet.Ecopath

                Dim tag As Object = Me.Rows(p.Row).Tag
                If (tag Is Nothing) Then Return False

                Debug.Assert(TypeOf tag Is cEcopathDistributionParamsData)

                Dim data As cEcopathDistributionParamsData = DirectCast(tag, cEcopathDistributionParamsData)

                Select Case DirectCast(p.Column, eEcopathColumnTypes)
                    Case eEcopathColumnTypes.CV
                        data.CV = CDbl(cell.GetValue(p))
                    Case eEcopathColumnTypes.Lower
                        data.LowerBound = CDbl(cell.GetValue(p))
                    Case eEcopathColumnTypes.Upper
                        data.UpperBound = CDbl(cell.GetValue(p))
                    Case eEcopathColumnTypes.Mean
                        data.Mean = CDbl(cell.GetValue(p))
                    Case Else
                        ' NOP
                End Select

            Case frmDistributionParameters.eParameterSet.Ecosim

                Dim tag As Object = Me.Rows(p.Row).Tag
                If (tag Is Nothing) Then Return False

                Debug.Assert(TypeOf tag Is cEcosimDistributionParamsData)

                Dim data As cEcosimDistributionParamsData = DirectCast(tag, cEcosimDistributionParamsData)

                Select Case DirectCast(p.Column, eEcosimColumnTypes)
                    Case eEcosimColumnTypes.DistrType
                        data.DistributionType = DirectCast(cell.GetValue(p), cMSE.DistributionType)
                    Case eEcosimColumnTypes.Lower
                        data.LowerBound = CDbl(cell.GetValue(p))
                    Case eEcosimColumnTypes.Upper
                        data.UpperBound = CDbl(cell.GetValue(p))
                    Case eEcosimColumnTypes.MidPoint
                        data.MidPoint = CDbl(cell.GetValue(p))
                    Case Else
                        ' NOP
                End Select
        End Select

        Me.RaiseDataChangeEvent()
        Return MyBase.OnCellEdited(p, cell)

    End Function

    Private Sub RaiseDataChangeEvent()
        Try
            RaiseEvent onEdited()
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Overrides

End Class


