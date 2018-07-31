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
Imports EwEMSEPlugin.HCR_GroupNS
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports SourceGrid2.Cells
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Grid to allow species quota interaction.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class gridTargetFishingMortalityPolicy
    Inherits EwEGrid

#Region " Internal defs "

    Private Enum eColumnTypes As Integer
        Index = 0
        BioGroupName
        HCR_Type
        BLowerLim
        BStep
        BUpperLim
        FGroupName
        MinF
        MaxF
        Targ_Or_Cons
        TimeFrameRuleYears
    End Enum

#End Region ' Internal defs

    Public Event onEdited()
    Private m_strategy As Strategy = Nothing
    Private m_editorTarg_Or_Cons As EwEComboBoxCellEditor = Nothing
    Private m_editorHCR_Type As EwEComboBoxCellEditor = Nothing
    Private m_conversionToDisplay As eConvertTypes = eConvertTypes.ToDisplayBio
    Private m_conversionToData As eConvertTypes = eConvertTypes.ToEcopathBio

#Region " Constructor "

    Public Sub New()
        MyBase.New()
        Me.m_editorTarg_Or_Cons = New EwEComboBoxCellEditor(New cCostFunctionTypeFormatter())
        Me.m_editorHCR_Type = New EwEComboBoxCellEditor(New cHCRTypeFormatter)
    End Sub

#End Region ' Constructor

#Region " Public interfaces "

    Public Property HarvestControlRule() As HCR_Group
        Get
            Dim iRow As Integer = Me.SelectedRow
            If (iRow > 0) Then
                Return DirectCast(Me.Rows(iRow).Tag, HCR_Group)
            End If
            Return Nothing
        End Get
        Set(ByVal value As HCR_Group)
            For iRow As Integer = 1 To Me.RowsCount - 1
                If ReferenceEquals(Me.Rows(iRow).Tag, value) Then
                    Me.SelectRow(iRow)
                    Return
                End If
            Next
            Me.SelectRow(-1)
        End Set
    End Property

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.EcoSim
        End Get
    End Property

    Public Property SelectedStrategy As Strategy
        Get
            Return Me.m_strategy
        End Get
        Set(value As Strategy)
            Me.m_strategy = value
            Me.FillData()
        End Set
    End Property

    Public Property DisplayRelativeValues As Boolean
        Get
            Return (Me.m_conversionToDisplay = eConvertTypes.None)
        End Get
        Set(value As Boolean)
            Me.m_conversionToDisplay = If(value, eConvertTypes.None, eConvertTypes.ToDisplayBio)
            Me.m_conversionToData = If(value, eConvertTypes.None, eConvertTypes.ToEcopathBio)
            Me.RefreshContent()
        End Set
    End Property

#End Region ' Public interfaces

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Dim iNumCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length

        Me.Redim(1, iNumCols)

        ' Mark: you need display proper units according to the DisplayRelativeValues flag. I did this for the upper b lim to show an example

        Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.BioGroupName) = New EwEColumnHeaderCell(SharedResources.HEADER_GROUPNAME)
        Me(0, eColumnTypes.HCR_Type) = New EwEColumnHeaderCell(My.Resources.HEADER_HCR_TYPE)
        Me(0, eColumnTypes.BLowerLim) = New EwEColumnHeaderCell(If(Me.DisplayRelativeValues, My.Resources.HEADER_LIMIT_LOWER_B_T_KM2, My.Resources.HEADER_LIMIT_LOWER_B_KT))
        Me(0, eColumnTypes.BStep) = New EwEColumnHeaderCell(If(Me.DisplayRelativeValues, My.Resources.HEADER_STEP_BIOMASS_T_KM2, My.Resources.HEADER_STEP_BIOMASS_KT))
        Me(0, eColumnTypes.BUpperLim) = New EwEColumnHeaderCell(If(Me.DisplayRelativeValues, My.Resources.HEADER_LIMIT_UPPER_B_T_KM2, My.Resources.HEADER_LIMIT_UPPER_B_KT))
        Me(0, eColumnTypes.FGroupName) = New EwEColumnHeaderCell(My.Resources.HEADER_FMORT_GROUP)
        Me(0, eColumnTypes.MinF) = New EwEColumnHeaderCell(My.Resources.HEADER_F_MORT_MIN)
        Me(0, eColumnTypes.MaxF) = New EwEColumnHeaderCell(My.Resources.HEADER_F_MORT_MAX)
        Me(0, eColumnTypes.Targ_Or_Cons) = New EwEColumnHeaderCell(My.Resources.HEADER_HCR_TARG_OR_CONS)
        Me(0, eColumnTypes.TimeFrameRuleYears) = New EwEColumnHeaderCell(My.Resources.HEADER_TIMEFRAMERULES)

        Me.FixedColumns = 2
        Me.FixedColumnWidths = True
        Me.AllowBlockSelect = False

    End Sub

    Protected Overrides Sub FillData()

        If (Me.UIContext Is Nothing) Then Return
        Me.RowsCount = 1

        If (Me.m_strategy Is Nothing) Then Return

        Dim iHCR As Integer
        Dim cell As ICell

        For Each Rule As HCR_Group In Me.m_strategy
            iHCR = Me.AddRow()

            'Row Index
            Me(iHCR, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iHCR))

            'Group Name
            cell = New EwECell(Rule.GroupB.Name, cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Names)
            Me(iHCR, eColumnTypes.BioGroupName) = cell

            'HCR Type
            cell = New SourceGrid2.Cells.Real.Cell(Rule.HCR_Type, Me.m_editorHCR_Type)
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(iHCR, eColumnTypes.HCR_Type) = cell
            'Dependent on Type change change Bstep
            If Rule.HCR_Type = eHCR_Type.Traditional Then
                If Rule.BStep = cCore.NULL_VALUE Then
                    cell = New EwECell(Rule.BStep, GetType(Double), cStyleGuide.eStyleFlags.NotEditable)
                Else
                    cell = New EwECell(Units.Convert(Me.m_conversionToDisplay, Rule.BStep))
                End If
                DirectCast(cell, EwECell).Style = cStyleGuide.eStyleFlags.NotEditable
            ElseIf Rule.HCR_Type = eHCR_Type.Multilevel Then
                cell = New EwECell(Units.Convert(Me.m_conversionToDisplay, Rule.BStep))
                'End If
                DirectCast(cell, EwECell).Style = cStyleGuide.eStyleFlags.OK
            End If
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(iHCR, eColumnTypes.BStep) = cell
            ' Dependent on type change change MinF
            If Rule.HCR_Type = eHCR_Type.Traditional Then
                If Rule.MinF = cCore.NULL_VALUE Then
                    cell = New EwECell(Rule.MinF, GetType(Double), cStyleGuide.eStyleFlags.NotEditable)
                Else
                    cell = New EwECell(CDbl(Rule.MinF))
                End If
                DirectCast(cell, EwECell).Style = cStyleGuide.eStyleFlags.NotEditable
            ElseIf Rule.HCR_Type = eHCR_Type.Multilevel Then
                cell = New EwECell(CDbl(Rule.MinF))
                DirectCast(cell, EwECell).Style = cStyleGuide.eStyleFlags.OK
            End If
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(iHCR, eColumnTypes.MinF) = cell

            'Biomass lower limit
            cell = New EwECell(Units.Convert(Me.m_conversionToDisplay, Rule.LowerLimit))
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(iHCR, eColumnTypes.BLowerLim) = cell

            'Biomass upper limit
            cell = New EwECell(Units.Convert(Me.m_conversionToDisplay, Rule.UpperLimit))
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(iHCR, eColumnTypes.BUpperLim) = cell

            'GroupF Name
            cell = New EwECell(Rule.GroupF.Name, GetType(String), cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Names)
            Me(iHCR, eColumnTypes.FGroupName) = cell

            'MaxF
            cell = New EwECell(Rule.MaxF)
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(iHCR, eColumnTypes.MaxF) = cell

            'Target or Conservation
            cell = New SourceGrid2.Cells.Real.Cell(Rule.Targ_Or_Cons, Me.m_editorTarg_Or_Cons)
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(iHCR, eColumnTypes.Targ_Or_Cons) = cell

            'Time frame rules number of years
            cell = New EwECell(Rule.TimeFrameRule.NYears)
            cell.Behaviors.Add(Me.EwEEditHandler)
            Me(iHCR, eColumnTypes.TimeFrameRuleYears) = cell

            Me.Rows(iHCR).Tag = Rule
        Next

    End Sub

    Public Sub UpdateContent()
        Dim curHCR As HCR_Group = Me.HarvestControlRule

        ' Mark: please revise this. I ran into a case where curHCR was nothing. Not sure how to make this work properly
        If (curHCR Is Nothing) Then Return

        For Each row As RowInfo In Rows
            If row.Tag IsNot Nothing Then

                Dim hcr As HCR_Group = DirectCast(row.Tag, HCR_Group)
                If ReferenceEquals(hcr.GroupB, curHCR.GroupB) Then

                    DirectCast(row.GetCells(eColumnTypes.BioGroupName), EwECell).Value = hcr.GroupB.Name
                    DirectCast(row.GetCells(eColumnTypes.FGroupName), EwECell).Value = hcr.GroupF.Name
                    DirectCast(row.GetCells(eColumnTypes.BLowerLim), EwECell).Value = Units.Convert(Me.m_conversionToDisplay, hcr.LowerLimit)
                    If hcr.BStep = cCore.NULL_VALUE Then
                        DirectCast(row.GetCells(eColumnTypes.BStep), EwECell).Value = cCore.NULL_VALUE
                    Else
                        DirectCast(row.GetCells(eColumnTypes.BStep), EwECell).Value = Units.Convert(Me.m_conversionToDisplay, hcr.BStep)
                    End If

                    DirectCast(row.GetCells(eColumnTypes.BUpperLim), EwECell).Value = Units.Convert(Me.m_conversionToDisplay, hcr.UpperLimit)

                    DirectCast(row.GetCells(eColumnTypes.MinF), EwECell).Value = CDbl(hcr.MinF)

                    DirectCast(row.GetCells(eColumnTypes.MaxF), EwECell).Value = hcr.MaxF

                    DirectCast(row.GetCells(eColumnTypes.Targ_Or_Cons), ICell).Value = hcr.Targ_Or_Cons

                    DirectCast(row.GetCells(eColumnTypes.TimeFrameRuleYears), ICell).Value = hcr.TimeFrameRule.NYears

                End If
            End If
        Next
        Me.Refresh()
    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
        Me.Selection.SelectionMode = GridSelectionMode.Row
        Me.Columns(eColumnTypes.Index).Width = 20
    End Sub

    Protected Overrides Function OnCellValueChanged(ByVal p As Position, ByVal cell As Cells.ICellVirtual) As Boolean

        Try

            If Rows(p.Row).Tag Is Nothing Then
                'No HCR in this row
                Return True
            End If

            Select Case p.Column

                Case eColumnTypes.HCR_Type
                    Me.HarvestControlRule.HCR_Type = DirectCast(cell.GetValue(p), eHCR_Type)
                    If Me.HarvestControlRule.HCR_Type = eHCR_Type.Traditional Then
                        Dim bstepcell As EwECell = DirectCast(Me(p.Row, eColumnTypes.BStep), EwECell)
                        bstepcell.Style = cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable
                        Dim minfcell As EwECell = DirectCast(Me(p.Row, eColumnTypes.MinF), EwECell)
                        minfcell.Style = cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable
                    ElseIf Me.HarvestControlRule.HCR_Type = eHCR_Type.Multilevel Then
                        Dim bstepcell As EwECell = DirectCast(Me(p.Row, eColumnTypes.BStep), EwECell)
                        bstepcell.Style = cStyleGuide.eStyleFlags.OK
                        If Me.HarvestControlRule.BStep < Me.HarvestControlRule.LowerLimit Then bstepcell.Value = Me.HarvestControlRule.LowerLimit
                        Dim minfcell As EwECell = DirectCast(Me(p.Row, eColumnTypes.MinF), EwECell)
                        minfcell.Style = cStyleGuide.eStyleFlags.OK
                        If Me.HarvestControlRule.MinF < 0 Then minfcell.Value = CDbl(0.0)
                    End If

                Case eColumnTypes.BLowerLim
                    'bounds checking lower limit can not be > upper limit
                    Dim LowerLim As Double = Units.Convert(Me.m_conversionToData, CDbl(cell.GetValue(p)))
                    If LowerLim > Me.HarvestControlRule.UpperLimit Then
                        LowerLim = Me.HarvestControlRule.UpperLimit
                        cell.SetValue(p, Units.Convert(Me.m_conversionToDisplay, LowerLim))
                    End If

                    Me.HarvestControlRule.LowerLimit = CSng(LowerLim)

                Case eColumnTypes.BStep
                    'bounds checking: cannot be <Lowerlim or >UpperLim
                    Dim BStep As Double = Units.Convert(Me.m_conversionToData, CDbl(cell.GetValue(p)))

                    If BStep > Me.HarvestControlRule.UpperLimit Then
                        BStep = Me.HarvestControlRule.UpperLimit
                        cell.SetValue(p, Units.Convert(Me.m_conversionToDisplay, BStep))
                    ElseIf BStep < Me.HarvestControlRule.LowerLimit Then
                        BStep = Me.HarvestControlRule.LowerLimit
                        cell.SetValue(p, Units.Convert(Me.m_conversionToDisplay, BStep))
                    End If

                    Me.HarvestControlRule.BStep = CSng(BStep)

                Case eColumnTypes.BUpperLim
                    'bounds checking upper limit can not be < lower limit
                    Dim upperLim As Double = Units.Convert(Me.m_conversionToData, CDbl(cell.GetValue(p)))
                    If upperLim < Me.HarvestControlRule.LowerLimit Then
                        upperLim = Me.HarvestControlRule.LowerLimit
                        cell.SetValue(p, Units.Convert(Me.m_conversionToDisplay, upperLim))
                    End If

                    Me.HarvestControlRule.UpperLimit = CSng(upperLim)

                Case eColumnTypes.MinF
                    Dim minf As Single = CSng(cell.GetValue(p))
                    If minf < 0 Then minf = 0
                    If minf > HarvestControlRule.MaxF Then minf = HarvestControlRule.MaxF
                    Me.HarvestControlRule.MinF = CSng(cell.GetValue(p))

                Case eColumnTypes.MaxF
                    Me.HarvestControlRule.MaxF = CSng(cell.GetValue(p))

                Case eColumnTypes.Targ_Or_Cons
                    Me.HarvestControlRule.Targ_Or_Cons = DirectCast(cell.GetValue(p), eHCR_Targ_Or_Cons)

                Case eColumnTypes.TimeFrameRuleYears
                    Me.HarvestControlRule.TimeFrameRule.NYears = CInt(cell.GetValue(p))

            End Select

            Try
                RaiseEvent onEdited()
            Catch ex As Exception
                Debug.Assert(False, Me.ToString + " onEdited Event Exception: " + ex.Message)
            End Try

        Catch ex As Exception
            Debug.Assert(False, Me.ToString + ".OnCellValueChanged() Exception: " + ex.Message)
        End Try

        Return True
    End Function

#End Region ' Overrides

End Class
