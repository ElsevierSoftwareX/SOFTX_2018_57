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
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Grid class for showing MPA optimizations progress information.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class gridMPAOptimizations
    : Inherits EwEGrid

    Public Enum eColumnTypes As Byte
        Variable = 0
        Value
    End Enum

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        If Me.UIContext Is Nothing Then Return

        Dim c As EwECell = Nothing

        Me.FixedColumnWidths = False

        Me.Redim(9, [Enum].GetValues(GetType(eColumnTypes)).Length)

        Me(0, eColumnTypes.Variable) = New EwEColumnHeaderCell(SharedResources.HEADER_INDICATOR)
        Me(0, eColumnTypes.Value) = New EwEColumnHeaderCell(SharedResources.HEADER_VALUE)

        Me(1, eColumnTypes.Variable) = New EwERowHeaderCell(SharedResources.HEADER_NET_ECONOMIC_VALUE)
        c = New EwECell(0.0!, GetType(Single))
        c.Style = cStyleGuide.eStyleFlags.OK Or cStyleGuide.eStyleFlags.NotEditable
        Me(1, eColumnTypes.Value) = c

        Me(2, eColumnTypes.Variable) = New EwERowHeaderCell(SharedResources.HEADER_SOCIAL_VALUE_EMPLOYMENT)
        c = New EwECell(0.0!, GetType(Single))
        c.Style = cStyleGuide.eStyleFlags.OK Or cStyleGuide.eStyleFlags.NotEditable
        Me(2, eColumnTypes.Value) = c

        Me(3, eColumnTypes.Variable) = New EwERowHeaderCell(SharedResources.HEADER_MANDATED_REBUILDING)
        c = New EwECell(0.0!, GetType(Single))
        c.Style = cStyleGuide.eStyleFlags.OK Or cStyleGuide.eStyleFlags.NotEditable
        Me(3, eColumnTypes.Value) = c

        Me(4, eColumnTypes.Variable) = New EwERowHeaderCell(SharedResources.HEADER_ECOSYSTEM_STRUCTURE)
        c = New EwECell(0.0!, GetType(Single))
        c.Style = cStyleGuide.eStyleFlags.OK Or cStyleGuide.eStyleFlags.NotEditable
        Me(4, eColumnTypes.Value) = c

        Me(5, eColumnTypes.Variable) = New EwERowHeaderCell(SharedResources.HEADER_BIODIVERSITY)
        c = New EwECell(0.0!, GetType(Single))
        c.Style = cStyleGuide.eStyleFlags.OK Or cStyleGuide.eStyleFlags.NotEditable
        Me(5, eColumnTypes.Value) = c

        Me(6, eColumnTypes.Variable) = New EwERowHeaderCell(SharedResources.HEADER_BOUNDARYWEIGHT)
        c = New EwECell(0.0!, GetType(Single))
        c.Style = cStyleGuide.eStyleFlags.OK Or cStyleGuide.eStyleFlags.NotEditable
        Me(6, eColumnTypes.Value) = c

        Me(7, eColumnTypes.Variable) = New EwERowHeaderCell(SharedResources.HEADER_TOTAL)
        c = New EwECell(0.0!, GetType(Single))
        c.Style = cStyleGuide.eStyleFlags.OK Or cStyleGuide.eStyleFlags.NotEditable
        Me(7, eColumnTypes.Value) = c

        Me(8, eColumnTypes.Variable) = New EwERowHeaderCell(SharedResources.HEADER_AREA_CLOSED)
        c = New EwECell(0.0!, GetType(Single))
        c.Style = cStyleGuide.eStyleFlags.OK Or cStyleGuide.eStyleFlags.NotEditable
        Me(8, eColumnTypes.Value) = c

    End Sub

    Protected Overrides Sub FillData()

    End Sub

    Public Sub LogResult(ByVal sEconomicValue As Single, ByVal sSocialValue As Single, _
        ByVal sMandatedValue As Single, ByVal sEcologicalValue As Single, _
        ByVal sBiomassDiversityValue As Single, ByVal sBoundaryWeightValue As Single, _
        ByVal sTotalWeighted As Single, ByVal sPercClosed As Single)

        Me(1, eColumnTypes.Value).Value = sEconomicValue
        Me(2, eColumnTypes.Value).Value = sSocialValue
        Me(3, eColumnTypes.Value).Value = sMandatedValue
        Me(4, eColumnTypes.Value).Value = sEcologicalValue
        Me(5, eColumnTypes.Value).Value = sBiomassDiversityValue
        Me(6, eColumnTypes.Value).Value = sBoundaryWeightValue
        Me(7, eColumnTypes.Value).Value = sTotalWeighted
        Me(8, eColumnTypes.Value).Value = sPercClosed

        Me.InvalidateCells()

    End Sub

End Class
