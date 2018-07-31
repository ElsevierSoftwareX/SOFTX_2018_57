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
#Region " cMediationFunction "

''' <summary>
''' Mediation functions inherit their base functionality from cMediationBaseFunction 
''' which provide the underlying shape data and Ecopath base line properties. 
''' This implements loading and updating of the correct core data for this type of mediation function. 
''' </summary>
Public Class cMediationFunction
    Inherits cMediationBaseFunction

    Private m_groups As New List(Of cMediatingGroup)
    Private m_fleets As New List(Of cMediatingFleet)

#Region " Constructors "

    Friend Sub New(ByVal EcoSimData As cEcosimDatastructures, ByVal Manager As cBaseShapeManager, _
                   ByVal data As cMediationDataStructures, ByVal DBID As Integer, ByVal DataType As eDataTypes)
        'mediation data arrays from EcoSim
        'Public MedWeights(nGroups + nGear, MediationShapes) As Single 'defines biomass weights for med X
        'Public NMedXused() As Integer 'number of biomasses (mediation weights) in an iMediation
        'Public IMedUsed(,) As Integer 'groups used in med function X IMedUsed(nGroups + nGear, MediationShapes)
        'Public MedXbase() As Single 'ecopath base value of med function X
        'Public MedYbase() As Single 'value of med function at ecopath base X
        'Public MedIsUsed() As Boolean 'true if med function iMediation is used
        MyBase.New(EcoSimData, Manager, data, DBID, DataType)

        Try

            Dim iShape As Integer = m_iEcoSimIndex 'just for clarity

            m_manager = Manager 'keep a reference to the manager for this shape

            Dim grp As cMediatingGroup = Nothing
            Dim flt As cMediatingFleet = Nothing

            ' Groups: if this mediation shape has any weights applied to it then load the weight and group into an object
            For iGrp As Integer = 1 To m_data.nGroups
                If m_medData.MedWeights(iGrp, iShape) > 0 Then
                    grp = New cMediatingGroup
                    grp.iGroupIndex = iGrp ' m_data.IMedUsed(iGrp, iShape)
                    grp.Weight = m_medData.MedWeights(iGrp, iShape)
                    m_groups.Add(grp)
                End If
            Next

            ' Fleets: if this mediation shape has any weights applied to it then load the weight and fleet into an object
            For iFlt As Integer = 1 To m_data.nGear
                If m_medData.MedWeights(m_data.nGroups + iFlt, iShape) > 0 Then
                    flt = New cMediatingFleet
                    flt.iFleetIndex = iFlt ' m_data.IMedUsed(iGrp, iShape)
                    flt.Weight = m_medData.MedWeights(m_data.nGroups + iFlt, iShape)
                    m_fleets.Add(flt)
                End If
            Next

            m_bInInit = False
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".New() Error: " & ex.Message)
            Throw New ApplicationException(Me.ToString & ".New() Error: " & ex.Message, ex)
        End Try

    End Sub


#End Region ' Constructors

#Region "Properties"

#End Region

#Region " Updating "

    ''' <summary>
    ''' Update the underlying EcoSim data structures
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function Update() As Boolean
        MyBase.Update()

        'do not update during initialization
        If m_bInInit Then
            Return False
        End If


        Dim nused As Integer
        For Each grp As cMediatingGroup In m_groups
            nused += 1
            m_medData.IMedUsed(grp.iGroupIndex, m_iEcoSimIndex) = grp.iGroupIndex
            m_medData.MedWeights(grp.iGroupIndex, m_iEcoSimIndex) = grp.Weight
        Next grp

        nused = 0
        For Each flt As cMediatingFleet In m_fleets
            nused += 1
            m_medData.IMedUsed(m_data.nGroups + flt.iFleetIndex, m_iEcoSimIndex) = flt.iFleetIndex
            m_medData.MedWeights(m_data.nGroups + flt.iFleetIndex, m_iEcoSimIndex) = flt.Weight
        Next flt

        m_medData.NMedXused(m_iEcoSimIndex) = nused

        'tell the manager that a shape has changed it's data
        ShapeChanged()

        Return True

    End Function

    ''' <summary>
    ''' Clear all the data, in the underlying ecosim data, out of the MedWeights for this mediation shape.
    ''' </summary>
    ''' <remarks>
    ''' This is used if a mediating group is removed to clear the ecosim data before the group is removed from the list. 
    ''' This must be used in conjuction the Update() to restore the data
    ''' </remarks>
    Private Sub clearMedWeights()

        Try

            For Each grp As cMediatingGroup In m_groups
                m_medData.IMedUsed(grp.iGroupIndex, m_iEcoSimIndex) = 0
                m_medData.MedWeights(grp.iGroupIndex, m_iEcoSimIndex) = 0
            Next grp

            For Each flt As cMediatingFleet In m_fleets
                m_medData.IMedUsed(m_data.nGroups + flt.iFleetIndex, m_iEcoSimIndex) = 0
                m_medData.MedWeights(m_data.nGroups + flt.iFleetIndex, m_iEcoSimIndex) = 0
            Next flt

        Catch ex As Exception
            Debug.Assert(False)
        End Try
    End Sub

#End Region ' Updating

#Region " Implementation of Must Override properties "

    ''' <inheritdocs cref="cMediationBaseFunction.AddGroup"/>
    Public Overloads Overrides Function AddGroup(ByVal iGroup As Integer, ByVal weight As Single, Optional ByVal iFleetIndex As Integer = cCore.NULL_VALUE) As Boolean
        'ToDo: data validation
        Debug.Assert(iFleetIndex <= 0, Me.ToString & ".AddGroup() Invalid Fleet index")
        m_groups.Add(New cMediatingGroup(iGroup, weight))
        Update()
        Return True

    End Function

    ''' <inheritdocs cref="cMediationBaseFunction.NumGroups"/>
    Public Overrides ReadOnly Property NumGroups() As Integer
        Get
            Return m_groups.Count
        End Get
    End Property

    ''' <inheritdocs cref="cMediationBaseFunction.Group"/>
    Public Overrides Property Group(ByVal iGroup As Integer) As cMediatingGroup
        Get
            Return m_groups(iGroup)
        End Get

        Set(ByVal value As cMediatingGroup)
            m_groups.Item(iGroup) = value
            Update()
        End Set
    End Property

    ''' <inheritdocs cref="cForcingFunction.Clear"/>
    Public Overrides Sub Clear()

        Try
            'clear the ecosim data
            clearMedWeights()
            m_groups.Clear()
            m_fleets.Clear()

            MyBase.Clear()

        Catch ex As Exception
            Debug.Assert(False)
        End Try

    End Sub

    ''' <inheritdocs cref="cMediationBaseFunction.AddFleet"/>
    Public Overrides Function AddFleet(ByVal iFleet As Integer, ByVal weight As Single) As Boolean

        'ToDo: data validation
        m_fleets.Add(New cMediatingFleet(iFleet, weight))
        Update()
        Return True
    End Function

    ''' <inheritdocs cref="cMediationBaseFunction.Fleet"/>
    Public Overrides Property Fleet(ByVal iFleet As Integer) As cMediatingFleet

        Get
            Return m_fleets(iFleet)
        End Get

        Set(ByVal value As cMediatingFleet)
            m_fleets.Item(iFleet) = value
            Update()
        End Set

    End Property

    ''' <inheritdocs cref="cMediationBaseFunction.NumFleet"/>
    Public Overrides ReadOnly Property NumFleet() As Integer
        Get
            Return m_fleets.Count
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="iIndex">Zero-based index [0, <see cref="NumFleet"/>-1] of the 
    ''' mediating group to remove.</param>
    ''' <returns></returns>
    Public Function RemoveFleet(ByVal iIndex As Integer) As Boolean

        Try
            'clear the ecosim data
            clearMedWeights()
            'remove the fleet from the list
            m_fleets.RemoveAt(iIndex)
            'update the ecosim data with the remaining fleet(s)
            Update()

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function RemoveFleet(ByRef fleet As cMediatingFleet) As Boolean

        Try
            'clear the ecosim data
            clearMedWeights()
            'remove the fleet from the list
            m_fleets.Remove(fleet)
            'update the ecosim data with the remaining fleet(s)
            Update()
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

#End Region '  List Interfaces

End Class

#End Region

