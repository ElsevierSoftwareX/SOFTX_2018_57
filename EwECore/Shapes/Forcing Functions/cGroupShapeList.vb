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

Option Strict On

''' <summary>
''' This is a collection of cShapeGroupPair 
''' </summary>
Public Class cGroupShapeList
    Implements Collections.IEnumerable

    Private m_list As New List(Of cGroupShapePair)
    Private m_data As cEcosimDatastructures
    Private m_stanza As cStanzaDatastructures
    Private m_manager As cEggProductionShapeManager

    Friend Sub New(ByRef EcoSimData As cEcosimDatastructures, ByRef StanzaData As cStanzaDatastructures, ByRef EggProdManager As cEggProductionShapeManager)
        m_data = EcoSimData
        m_manager = EggProdManager
        m_stanza = StanzaData
    End Sub

    Friend Sub Add(ByRef shapeGroupPair As cGroupShapePair)

        'ToDo_jb cAppliesToList.Add()  Make sure the shapeGroupPair.iStanzaGroup is a valid stanza group
        m_list.Add(shapeGroupPair)

    End Sub


    Default Public Property Item(Index As Integer) As cGroupShapePair
        Get
            Try
                Return m_list.Item(Index)
            Catch ex As Exception
                Return Nothing
            End Try
        End Get
        Set(value As cGroupShapePair)
            Try
                m_list.Item(Index) = value
            Catch ex As Exception
                Return
            End Try
        End Set
    End Property


    Public Function Count() As Integer
        Return m_list.Count
    End Function

    Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return m_list.GetEnumerator
    End Function


End Class
