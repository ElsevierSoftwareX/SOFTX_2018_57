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
Imports EwECore
Imports System.Drawing
Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Style

#End Region ' Imports

' ToDo: globalize this class
' ToDo: comment class and methods
' ToDo: comment code

Public Class gridEcopath
    Inherits EwEGrid

#Region " Private variables "

    ''' <summary>Settings to use in the grid.</summary>
    Private m_settings As cIndicatorSettings = Nothing
    ''' <summary>Computed indicators.</summary>
    Private m_indicators As cIndicators = Nothing

    ''' <summary>The columns shown in the grid</summary>
    Private Enum eColumnTypes As Integer
        ''' <summary>Column containing hierachy cells (+/-).</summary>
        Index
        ''' <summary>Column containing names.</summary>
        Name
        ''' <summary>Column containing values.</summary>
        Value
        ''' <summary>Column containing units.</summary>
        Units
        ''' <summary>Column containing descriptions</summary>
        Description
    End Enum

#End Region ' Private variables

#Region " Public methods "

    Public Sub Attach(ByVal settings As cIndicatorSettings)
        Me.m_settings = settings
    End Sub

    Public Sub Detach()
        Me.m_settings = Nothing
        Me.m_indicators = Nothing
    End Sub

#End Region ' Public methods

#Region " Grid overrides "

    Public Shadows Sub RefreshContent(indicators As cIndicators)
        Me.m_indicators = indicators
        MyBase.RefreshContent()
    End Sub

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        Me.Redim(1, [Enum].GetValues(GetType(eColumnTypes)).Length)

        Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell("Indicator")
        Me(0, eColumnTypes.Value) = New EwEColumnHeaderCell("Value")
        Me(0, eColumnTypes.Units) = New EwEColumnHeaderCell("Units")
        Me(0, eColumnTypes.Description) = New EwEColumnHeaderCell("Description")

        Me.FixedColumnWidths = True

    End Sub

    Protected Overrides Sub FillData()

        If (Me.m_settings Is Nothing) Then Return
        If (Me.m_indicators Is Nothing) Then Return

        Dim grp As cIndicatorInfoGroup = Nothing
        Dim ind As cIndicatorInfo = Nothing
        Dim hgcGrp As EwEHierarchyGridCell = Nothing
        Dim cellValue As EwECell = Nothing
        Dim iRow As Integer = 0

        Try

            ' For now show all groups, is easiest
            For iGrp As Integer = 0 To Me.m_settings.NumIndicatorGroups - 1
                ' Get group
                grp = Me.m_settings.IndicatorGroup(iGrp)
                iRow = Me.AddRow()

                ' Create hierarchy cell
                hgcGrp = New EwEHierarchyGridCell()

                Me(iRow, eColumnTypes.Index) = hgcGrp
                Me(iRow, eColumnTypes.Name) = New EwERowHeaderCell(grp.Name)
                Me(iRow, eColumnTypes.Value) = New EwERowHeaderCell("")
                Me(iRow, eColumnTypes.Units) = New EwERowHeaderCell("")
                Me(iRow, eColumnTypes.Description) = New EwERowHeaderCell("")

                For iInd As Integer = 0 To grp.NumIndicators - 1
                    ' Get indicator
                    ind = grp.Indicator(iInd)
                    iRow = Me.AddRow()

                    Me(iRow, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iInd + 1))
                    Me(iRow, eColumnTypes.Name) = New EwERowHeaderCell(ind.Name)
                    Me(iRow, eColumnTypes.Description) = New EwECell(ind.Description, GetType(String), cStyleGuide.eStyleFlags.NotEditable)
                    Me(iRow, eColumnTypes.Description).VisualModel.TextAlignment = ContentAlignment.MiddleLeft

                    ' Value cell is special case: suppresses cCore.NULL_VALUE
                    cellValue = New EwECell(ind.GetValue(Me.m_indicators), GetType(Single), cStyleGuide.eStyleFlags.NotEditable)
                    cellValue.SuppressZero(cCore.NULL_VALUE) = True
                    Me(iRow, eColumnTypes.Value) = cellValue
                    Me(iRow, eColumnTypes.Units) = New EwEUnitCell(ind.Units)

                    hgcGrp.AddChildRow(iRow)

                Next iInd

            Next iGrp

        Catch ex As Exception
            ' Whoah!
        End Try

    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()

        Me.Columns(eColumnTypes.Index).AutoSizeMode = SourceGrid2.AutoSizeMode.None
        Me.Columns(eColumnTypes.Name).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.Value).AutoSizeMode = SourceGrid2.AutoSizeMode.None
        Me.Columns(eColumnTypes.Units).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableAutoSize
        Me.Columns(eColumnTypes.Description).AutoSizeMode = SourceGrid2.AutoSizeMode.EnableStretch Or SourceGrid2.AutoSizeMode.EnableAutoSize

        Me.AutoSize = True
        Me.StretchColumnsToFitWidth()

    End Sub

#End Region ' Grid overrides

End Class
