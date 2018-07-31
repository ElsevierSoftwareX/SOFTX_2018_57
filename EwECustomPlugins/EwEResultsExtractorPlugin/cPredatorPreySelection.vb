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

Imports EwECore

Public Class cPredatorPreySelection

#Region "Private fields"

    Private m_Predator As String
    Private m_Prey As List(Of String)
    Private m_core As cCore

#End Region

#Region "Constructor(s)"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Predator"></param>
    ''' <param name="Core"></param>
    ''' <remarks>
    ''' JS 01Mar11: Core must be provided as a parameter.
    ''' </remarks>
    Public Sub New(ByRef Predator As String, ByVal Core As cCore)
        Me.m_core = Core
        m_Predator = Predator
        m_Prey = New List(Of String)
    End Sub

#End Region

#Region "Properties"

    Public Property PredatorName() As String
        Get
            Return m_Predator
        End Get
        Set(ByVal value As String)
            m_Predator = value
        End Set
    End Property

    Public Property PreyName(ByVal i As Integer) As String
        Get
            Return m_Prey(i)
        End Get
        Set(ByVal value As String)
            m_Prey(i) = value
        End Set
    End Property

#End Region

#Region "Subroutines"

    Public Sub AddPrey(ByVal PreyName As String)
        m_Prey.Add(PreyName)
    End Sub

    Public Sub RemovePrey(ByVal i As Integer)
        m_Prey.RemoveAt(i)
    End Sub

#End Region

#Region "Functions"

    Public Function CountPrey() As Integer
        Return m_Prey.Count
    End Function

    Public Function GetIndexPredatorForEcoSim() As Integer
        Dim PredIndexEcosim As Integer = 1

        While m_core.EcoSimGroupOutputs(PredIndexEcosim).Name <> m_Predator
            PredIndexEcosim += 1
        End While
        Return PredIndexEcosim

    End Function

    Public Function GetIndexPreyForEcoSim(ByVal i As Integer) As Integer
        Dim PreyIndexEcosim As Integer = 1

        While m_core.EcoSimGroupOutputs(PreyIndexEcosim).Name <> m_Prey(i)
            PreyIndexEcosim += 1
        End While
        Return PreyIndexEcosim

    End Function

#End Region


End Class

