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

Public Enum eMPAOptimizationModels
    EcoSeed
    RandomSearch
End Enum


Public Class cMPAOptDataStructures

    Const MIN_RUN_LENGTH As Integer = 3
 
    Public CurRow As Integer
    Public CurCol As Integer
    Public bestrow As Integer
    Public bestcol As Integer
    Public StopRun As Boolean
    Public BoundaryWeight As Single
    Public MPASeed(,) As Integer
    Public SeedBlockSize2 As Integer

    'value of objective function  relative to the base value
    Public objFuncEconomicValue As Single
    Public objFuncMandatedValue As Single
    Public objFuncSocialValue As Single
    Public objFuncEcologicalValue As Single
    Public objFuncAreaBorder As Single

    Public objFuncBiodiversity As Single

    Public objFuncTotal As Single

    Public SearchType As eMPAOptimizationModels

    Public stepSize As Integer
    Public MaxArea As Integer
    Public MinArea As Integer
    Public nIterations As Integer

    Public iMPAtoUse As Integer
    Public bUseCellWeight As Boolean

    Public EcoSpaceStartYear As Integer = 3
    Public EcoSpaceEndYear As Integer

    Private m_cells As List(Of cMPACell)

    Public Sub New()

        SearchType = eMPAOptimizationModels.RandomSearch

        nIterations = 100
        stepSize = 10
        MaxArea = 20
        MinArea = 20
        iMPAtoUse = 1

        m_cells = New List(Of cMPACell)

    End Sub

    ''' <summary>
    ''' Clear out the current Ecoseed values
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Clear()
        CurRow = 0
        CurCol = 0
        bestrow = 0
        bestcol = 0

        objFuncEconomicValue = 0
        objFuncMandatedValue = 0
        objFuncSocialValue = 0
        objFuncEcologicalValue = 0
        objFuncAreaBorder = 0
        objFuncBiodiversity = 0
        objFuncTotal = 0

    End Sub

    Public Sub setObjectiveValues(ByVal SearchData As cSearchDatastructures)

    End Sub




    Public Sub AddCell(ByVal Row As Integer, ByVal col As Integer, ByVal iMPA As Integer)
        m_cells.Add(New cMPACell(Row, col, iMPA))
    End Sub

    Public Sub ClearCells()
        m_cells.Clear()
    End Sub

    Public Function Cells() As List(Of cMPACell)
        Return m_cells
    End Function

    Public ReadOnly Property MinRunLength() As Integer
        Get
            Return MIN_RUN_LENGTH
        End Get
    End Property

End Class



''' <summary>
''' MPA cell selected during a trial
''' </summary>
''' <remarks></remarks>
Public Class cMPACell
    Public Row As Integer
    Public Col As Integer
    Public iMPA As Integer

    Public Sub New(ByVal theRow As Integer, ByVal theCol As Integer, ByVal theMPAIndex As Integer)
        Row = theRow
        Col = theCol
        iMPA = theMPAIndex
    End Sub

End Class