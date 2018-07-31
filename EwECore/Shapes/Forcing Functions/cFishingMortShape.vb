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
Imports EwEUtils.Core

''' <summary>
''' A fishing mortality shape.
''' </summary>
Public Class cFishingMortShape
    Inherits cForcingFunction

    Friend Sub New(ByRef EcoSimData As cEcosimDatastructures, _
                   ByRef Manager As cBaseShapeManager, _
                   ByVal DBID As Integer, _
                   ByVal GroupName As String)

        MyBase.New(EcoSimData, Manager, DBID, eDataTypes.FishMort)
        m_bInInit = True

        'iEcoSimIndex is the array index in the underlying EcoSim data
        m_iEcoSimIndex = Array.IndexOf(m_data.FishRateNoDBID, Me.DBID)

        Me.Name = GroupName 'groupname is part of the Ecopath data so it can not be retrieved from the Ecosim data and must be passed in
        Me.Load()

        m_bInInit = False

    End Sub


    ''' <summary>
    ''' Initialize the propeties from the underlying EcoSim data structures at the existing array index (iEcoSimIndex)
    ''' </summary>
    ''' <returns>True if successful</returns>
    ''' <remarks>This seperates creation from initialization so that an existing object can be repopluated from its underlying data</remarks>
    Protected Friend Overrides Function Load() As Boolean

        m_bInInit = True

        Me.m_iEcoSimIndex = Array.IndexOf(m_data.FishRateNoDBID, m_iDBID)
        Debug.Assert(m_iEcoSimIndex <> -1, Me.ToString & ".Load() invalid database ID.")
        If m_iEcoSimIndex = -1 Then Return False

        Me.ResizeData(m_data.NTimes)
        For ipt As Integer = 1 To m_data.NTimes
            Me.ShapeData(ipt) = m_data.FishRateNo(Me.m_iEcoSimIndex, ipt) 'FishRateNo(nGroups,nTime)
        Next ipt

        m_nYears = m_data.NumYears

        m_bInInit = False

        Return True

    End Function

    ''' <summary>
    ''' Update the underlying EcoSim data structures
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function Update() As Boolean

        'do not update during initialization
        If m_bInInit Then
            Return False
        End If

        'can not update if there is not an index to the underlying data structures
        If (m_iEcoSimIndex = cCore.NULL_VALUE) Or (m_iEcoSimIndex > m_data.nGroups) Then
            cLog.Write(Me.ToString & ".update(m_data) index out of bounds. Data not updated.")
            Return False
        End If

        'make sure the shape data is the same size as the EcoSim Shape data
        'this is a double check as the data size was check when the forcing function was added to the Shape Manager
        'however it could have been changed be an interface at a later date
        Me.ResizeData(m_data.NTimes)

        'populate the raw shape data
        For ipt As Integer = 1 To m_data.NTimes
            m_data.FishRateNo(m_iEcoSimIndex, ipt) = Me.ShapeData(ipt) 'FishRateNo(nGroups,nTime)
        Next ipt

        'tell the manager that a shape has changed it's data
        ShapeChanged()

        Return True

    End Function

End Class

