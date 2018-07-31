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
#Region " cMediationBase "

'''<summary>
''' Base Class for a mediation function. 
''' A mediation function "Is A" Forcing function that that contains Ecopath base values along it's X axis, Biomass, Effort or Catch
''' depending on the implementation. The Y axis contains the value of the underlying shape entered by the user. 
''' The value of the Y axis is used to mediate/modify a Group biomass (for a Mediation function) or Group/Fleet price (for a Price Elasticity function) 
''' based on how the mediaton function is applied via the cPredPreyInteraction or cLandingsInteraction that "contain" the mediation function. 
''' </summary>
Public MustInherit Class cMediationBaseFunction
    Inherits cForcingFunction

    Protected m_iMedXBase As Integer
    Protected m_medData As cMediationDataStructures

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
        MyBase.New(EcoSimData, Manager, DBID, DataType)

        Try

            'Me.m_datatype = DataType
            Me.m_coreComponent = eCoreComponentType.EcoSim
            Me.m_medData = data
            Me.m_timeresolution = eTSDataSetInterval.TimeStep

            Me.m_bInInit = True
            Me.m_data = EcoSimData
            Me.m_iDBID = DBID
            Me.m_iEcoSimIndex = Array.IndexOf(m_medData.MediationDBIDs, m_iDBID)

            Dim iShape As Integer = m_iEcoSimIndex 'just for clarity

            m_manager = Manager 'keep a reference to the manager for this shape

            m_bInInit = False
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".New() Error: " & ex.Message)
            Throw New ApplicationException(Me.ToString & ".New() Error: " & ex.Message, ex)
        End Try

    End Sub

    ''' <summary>
    ''' Initialize the propeties from the underlying EcoSim data structures at the existing array index (iEcoSimIndex)
    ''' </summary>
    ''' <returns>True if successful</returns>
    ''' <remarks>This seperates creation from initialization so that an existing object can be repopluated from its underlying data</remarks>
    Protected Friend Overrides Function Load() As Boolean

        'copy the data from zscale into an array that will be used to create a forcing data object
        m_bInInit = True
        Me.LockUpdates()

        m_iEcoSimIndex = Array.IndexOf(m_medData.MediationDBIDs, m_iDBID)
        Debug.Assert(m_iEcoSimIndex > -1, "mediation shape database ID invalid.")
        If m_iEcoSimIndex < 0 Then Return False

        Me.ResizeData(m_medData.NMedPoints)
        For ipt As Integer = 1 To m_medData.NMedPoints
            Me.ShapeData(ipt) = m_medData.Medpoints(ipt, m_iEcoSimIndex)
        Next ipt

        m_nYears = m_data.NumYears
        Me.Name = m_medData.MediationTitles(m_iEcoSimIndex)

        'shape parameters
        m_ShapeFunctionType = m_medData.MediationShapeParams(m_iEcoSimIndex).ShapeFunctionType
        m_params = CType(m_medData.MediationShapeParams(m_iEcoSimIndex).ShapeFunctionParams.Clone(), Single())

        Me.UnlockUpdates()
        m_bInInit = False
        Return True

    End Function

#End Region ' Constructors

#Region " Must override properties "

    ''' <summary>Returns the number of <see cref="cMediatingGroup">mediating groups</see> attached to this function.</summary>
    Public MustOverride ReadOnly Property NumGroups() As Integer
    ''' <summary>Retrieve a <see cref="cMediatingGroup">mediating group</see>.</summary>
    ''' <param name="iIndex">Zero-based index [0, <see cref="NumGroups"/>-1] of the mediating group to retrieve.</param>
    Public MustOverride Property Group(ByVal iIndex As Integer) As cMediatingGroup
    ''' <summary>Add a <see cref="cMediatingGroup">mediating group</see> to this function.</summary>
    ''' <param name="iGroup">The <see cref="cEcoPathGroupInput.Index">ecopath index</see> of the group to add.</param>
    ''' <param name="weight">The weight of the mediating fleet.</param>
    Public MustOverride Function AddGroup(ByVal iGroup As Integer, ByVal weight As Single, Optional ByVal iFleetIndex As Integer = cCore.NULL_VALUE) As Boolean

    ''' <summary>Returns the number of <see cref="cMediatingFleet">mediating fleets</see> attached to this function.</summary>
    Public MustOverride ReadOnly Property NumFleet() As Integer
    ''' <summary>Retrieve a <see cref="cMediatingFleet">mediating fleet</see>.</summary>
    ''' <param name="iIndex">Zero-based index [0, <see cref="NumFleet"/>-1] of the mediating fleet to retrieve.</param>
    Public MustOverride Property Fleet(ByVal iIndex As Integer) As cMediatingFleet
    ''' <summary>Add a <see cref="cMediatingFleet">mediating fleet</see> to this function.</summary>
    ''' <param name="iFleet">The <see cref="cFleetInput.Index">ecopath index</see> of the fleet to add.</param>
    ''' <param name="weight">The weight of the mediating fleet.</param>
    Public MustOverride Function AddFleet(ByVal iFleet As Integer, ByVal weight As Single) As Boolean

#End Region

#Region "Properties shared by all implementations"

    ''' <summary>
    ''' X Axis base index for biomass
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is the vertical green line in the EwE 5 mediation interface</remarks>
    Public Property XBaseIndex() As Integer
        Get
            Try
                Return m_medData.IMedBase(Array.IndexOf(m_medData.MediationDBIDs, m_iDBID))
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                cLog.Write(ex)
            End Try

        End Get
        Set(ByVal value As Integer)
            Try
                m_medData.IMedBase(Array.IndexOf(m_medData.MediationDBIDs, m_iDBID)) = value
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                cLog.Write(ex)
            End Try
        End Set
    End Property


    ''' <summary>
    ''' X Axis base value for sum of x biomass
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is the vertical green line in the EwE 5 mediation interface</remarks>
    Public ReadOnly Property XBase() As Single
        Get
            Try
                Return m_medData.MedXbase(Array.IndexOf(m_medData.MediationDBIDs, m_iDBID))
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                cLog.Write(ex)
            End Try

        End Get

    End Property

#End Region

#Region " Updating of shared properties"

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

        m_iEcoSimIndex = Array.IndexOf(m_medData.MediationDBIDs, m_iDBID)
        'can not update if there is not an index to the underlying data structures
        If (m_iEcoSimIndex = cCore.NULL_VALUE) Or (m_iEcoSimIndex > m_medData.MediationShapes) Then
            cLog.Write(Me.ToString & ".update(m_data) index out of bounds. Data not updated.")
            Return False
        End If

        'make sure the shape data is the same size as the EcoSim Shape data
        'this is a double check as the data size was check when the forcing function was added to the Shape Manager
        'however it could have been changed be an interface at a later date
        Me.ResizeData(m_medData.NMedPoints)

        'populate the raw shape data
        For ipt As Integer = 1 To Me.nPoints
            m_medData.Medpoints(ipt, m_iEcoSimIndex) = Me.ShapeData(ipt)
        Next ipt

        m_medData.MediationTitles(m_iEcoSimIndex) = Me.Name

        ' Forcing application type not applicable to mediation functions
        'm_data.ForcingApplicationType(m_iEcoSimIndex) = Me.m_ForcingApplicationType

        'shape parameters
        m_medData.MediationShapeParams(m_iEcoSimIndex).ShapeFunctionType = m_ShapeFunctionType
        m_medData.MediationShapeParams(m_iEcoSimIndex).ShapeFunctionParams = CType(m_params.Clone(), Single())

        Return True

    End Function



#End Region ' Updating

End Class

#End Region

