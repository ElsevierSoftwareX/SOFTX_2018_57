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
#Region " cLandingsMediationFunction "

''' <summary>
''' A derived mediation function, dedicated to modelling fleet-group interactions
''' based on landings.
''' </summary>
''' <remarks></remarks>
Public Class cLandingsMediationFunction
    Inherits cMediationBaseFunction

    Private m_groups As New List(Of cLandingsMediatingGroup)

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

            Me.m_datatype = eDataTypes.PriceMediation
            Dim iShape As Integer = m_iEcoSimIndex 'just for clarity

            m_manager = Manager 'keep a reference to the manager for this shape

            Dim grp As cLandingsMediatingGroup = Nothing

            ' Groups: if this mediation shape has any weights applied to it then load the weight and group into an object
            For iGrp As Integer = 1 To m_data.nGroups
                For iflt As Integer = 0 To Me.m_data.nGear
                    If m_medData.MedPriceWeights(iGrp, iflt, iShape) > 0 Then
                        grp = New cLandingsMediatingGroup(iGrp, iflt, m_medData.MedPriceWeights(iGrp, iflt, iShape))
                        m_groups.Add(grp)
                    End If
                Next
            Next

            m_bInInit = False
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".New() Error: " & ex.Message)
            Throw New ApplicationException(Me.ToString & ".New() Error: " & ex.Message, ex)
        End Try

    End Sub


#End Region ' Constructors

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
        For Each grp As cLandingsMediatingGroup In m_groups
            nused += 1
            m_medData.IMedFltUsed(grp.iFleetIndex, m_iEcoSimIndex) = grp.iFleetIndex
            m_medData.IMedUsed(grp.iGroupIndex, m_iEcoSimIndex) = grp.iGroupIndex
            m_medData.MedPriceWeights(grp.iGroupIndex, grp.iFleetIndex, m_iEcoSimIndex) = grp.Weight
        Next grp

        'nused = 0

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

            For Each grp As cLandingsMediatingGroup In m_groups
                m_medData.IMedFltUsed(grp.iFleetIndex, m_iEcoSimIndex) = 0
                m_medData.IMedUsed(grp.iGroupIndex, m_iEcoSimIndex) = 0
                m_medData.MedPriceWeights(grp.iGroupIndex, grp.iFleetIndex, m_iEcoSimIndex) = 0
            Next grp

        Catch ex As Exception
            Debug.Assert(False)
        End Try
    End Sub

#End Region ' Updating

#Region " Implementation of Must Override properties "

    ''' <inheritdocs cref="cMediationBaseFunction.AddGroup"/>
    ''' <param name="iFleet"></param>
    Public Overloads Overrides Function AddGroup(ByVal iGroup As Integer, ByVal weight As Single, Optional ByVal iFleet As Integer = cCore.NULL_VALUE) As Boolean
        'ToDo: data validation
        Debug.Assert(iFleet >= 0, Me.ToString & ".AddGroup() Invalid Fleet index")
        m_groups.Add(New cLandingsMediatingGroup(iGroup, iFleet, weight))
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
            Try
                Dim grp As cLandingsMediatingGroup = DirectCast(value, cLandingsMediatingGroup)
                m_groups.Item(iGroup) = grp
                Update()
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".Group() Failed to add group Invalid group type!")
            End Try
        End Set

    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="iIndex">Zero-based index [0, <see cref="NumGroups"/>-1] of the 
    ''' mediating group to remove.</param>
    ''' <returns></returns>
    Public Function RemoveGroup(ByVal iIndex As Integer) As Boolean

        Try
            'clear the ecosim data
            clearMedWeights()
            'remove the group from the list
            m_groups.RemoveAt(iIndex)
            'update the ecosim data with the remaining group(s)
            Update()

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function RemoveGroup(ByRef group As cLandingsMediatingGroup) As Boolean

        Try
            'clear the ecosim data
            clearMedWeights()
            'remove the group from the list
            m_groups.Remove(group)
            'update the ecosim data with the remaining group(s)
            Update()
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    ''' <inheritdocs cref="cForcingFunction.Clear"/>
    Public Overrides Sub Clear()

        Try
            'clear the ecosim data
            clearMedWeights()
            m_groups.Clear()

            MyBase.Clear()

        Catch ex As Exception
            Debug.Assert(False)
        End Try

    End Sub

    Public Overrides Function AddFleet(ByVal iFleet As Integer, ByVal weight As Single) As Boolean
        Debug.Assert(False, Me.ToString & ".AddFleet() Property not supported.")
        Return False
    End Function

    Public Overrides ReadOnly Property NumFleet() As Integer
        Get
            ' Debug.Assert(False, Me.ToString & ".CountFleet() Property not supported.")
            Return 0
        End Get
    End Property

    Public Overrides Property Fleet(ByVal iGroup As Integer) As cMediatingFleet
        Get
            Return Nothing
        End Get
        Set(ByVal value As cMediatingFleet)
            Debug.Assert(False, Me.ToString & ".Fleet() Property not supported.")
        End Set
    End Property

#End Region '  List Interfaces

End Class

#End Region

