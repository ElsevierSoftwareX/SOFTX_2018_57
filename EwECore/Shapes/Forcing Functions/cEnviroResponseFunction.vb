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


Public Class cEnviroResponseFunction
    Inherits cMediationBaseFunction

    Friend Sub New(ByVal EcoSimData As cEcosimDatastructures, ByVal Manager As cBaseShapeManager,
                   ByVal data As cMediationDataStructures, ByVal DBID As Integer, ByVal DataType As eDataTypes)
        MyBase.New(EcoSimData, Manager, data, DBID, DataType)

    End Sub


    Public Overrides Function Update() As Boolean
        MyBase.Update()

        'do not update during initialization
        If m_bInInit Then
            Return False
        End If

        'tell the manager that a shape has changed its data
        ShapeChanged()
        Return True

    End Function

#Region " Response function "

    ''' <summary>
    ''' Minimum value of the input map that the response will be computed for. 
    ''' All values less than this will return the first value of the response function. 
    ''' </summary>
    ''' <remarks>
    ''' Left margin of the X Axis considered to be inbounds of the response function.
    ''' Updates <see cref="cMediationDataStructures.XAxisMin">cMediationDataStructures.XAxisMin</see>
    ''' </remarks>
    Public Property ResponseLeftLimit() As Single
        Get
            Return m_medData.XAxisMin(Me.Index)
        End Get
        Set(ByVal value As Single)
            m_medData.XAxisMin(Me.Index) = value
            'tell the manager that a shape has changed its data
            ShapeChanged()
        End Set

    End Property

    ''' <summary>
    ''' Maximum value of the input map that the response will be computed for. 
    ''' All values greater than this will return the last value of the response function. 
    ''' </summary>
    ''' <remarks>
    ''' Right margin of the X Axis considered to be inbounds of the response function. 
    ''' Updates <see cref="cMediationDataStructures.XAxisMax">cMediationDataStructures.XAxisMax</see>
    ''' </remarks>
    Public Property ResponseRightLimit() As Single
        Get
            Return m_medData.XAxisMax(Me.Index)
        End Get
        Set(ByVal value As Single)
            m_medData.XAxisMax(Me.Index) = value
            'tell the manager that a shape has changed its data
            ShapeChanged()
        End Set

    End Property

    Public ReadOnly Property ResponseMean() As Single
        Get
            Return (m_medData.XAxisMin(Me.Index) + m_medData.XAxisMax(Me.Index)) * 0.5F
        End Get
    End Property

#End Region ' Response function

#Region "Groups and Fleets interfaces not used by a cEnviroResponseFunction "

    Public Overrides Function AddFleet(ByVal iFleet As Integer, ByVal weight As Single) As Boolean
        Debug.Assert(False, "Not implemented by cEnviroResponseFunction.")
        Return False
    End Function

    Public Overrides Function AddGroup(ByVal iGroup As Integer, ByVal weight As Single, Optional ByVal iFleetIndex As Integer = -9999) As Boolean
        Debug.Assert(False, "Not implemented by cEnviroResponseFunction.")
        Return False
    End Function

    Public Overrides Property Fleet(ByVal iIndex As Integer) As cMediatingFleet
        Get
            Debug.Assert(False, "Not implemented by cEnviroResponseFunction.")
            Return Nothing
        End Get
        Set(ByVal value As cMediatingFleet)
            Debug.Assert(False, "Not implemented by cEnviroResponseFunction.")
        End Set
    End Property

    Public Overrides Property Group(ByVal iIndex As Integer) As cMediatingGroup
        Get
            Debug.Assert(False, "Not implemented by cEnviroResponseFunction.")
            Return Nothing
        End Get
        Set(ByVal value As cMediatingGroup)
            Debug.Assert(False, "Not implemented by cEnviroResponseFunction.")
        End Set
    End Property

    Public Overrides ReadOnly Property NumFleet() As Integer
        Get
            'Debug.Assert(False, "Not implemented by cEnviroResponseFunction.")
            Return 0
        End Get
    End Property

    Public Overrides ReadOnly Property NumGroups() As Integer
        Get
            'Debug.Assert(False, "Not implemented by cEnviroResponseFunction.")
            Return 0
        End Get
    End Property

#End Region

End Class ' Response Function


