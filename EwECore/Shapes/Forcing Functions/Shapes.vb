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

Imports EwECore.ValueWrapper
Imports EwEUtils.Core

#Region " Forcing Shape "

''' -----------------------------------------------------------------------
''' <summary>
''' Provides access to Forcing and EggProduction shapes, and a base class 
''' for Mediation functions.
''' </summary>
''' -----------------------------------------------------------------------
Public Class cForcingFunction
    Inherits cShapeData

#Region " Protected data "

    Private m_bAllowValidation As Boolean = True

    Protected m_iIndex As Integer = 0
    Protected m_data As cEcosimDatastructures
    Protected m_manager As cBaseShapeManager

    Protected m_ID As Integer
    '   Protected m_Type As eDataTypes
    Protected m_nYears As Integer

    ' Parameters use to build a Curve
    Protected m_ShapeFunctionType As Long
    Protected m_params As Single() = New Single() {}

    Protected m_ForcingApplicationType As eForcingApplicationTypes

    'this flag is used to stop updating during initialization
    'it is more of a safe guard 
    Protected m_bInInit As Boolean

    Protected m_bLockUpdates As Boolean

#End Region ' Protected data

#Region " Public fields/properties "

    Public Property ShapeFunctionType() As Long
        Get
            Return Me.m_ShapeFunctionType
        End Get
        Set(ByVal value As Long)
            If (Me.m_ShapeFunctionType <> value) Then
                Me.m_ShapeFunctionType = value
                Dim fn As IShapeFunction = cShapeFunctionFactory.GetShapeFunction(value)
                If (fn IsNot Nothing) Then
                    ReDim Me.m_params(fn.nParameters)
                End If
            End If
            Me.Update()
        End Set
    End Property

    Public ReadOnly Property nParams As Integer
        Get
            Return Me.m_params.Count
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="iParam">One-based parameter index.</param>
    ''' <returns></returns>
    Public Property ShapeFunctionParameter(iParam As Integer) As Single
        Get
            If 1 <= iParam And iParam <= Me.nParams Then
                Return Me.m_params(iParam - 1)
            End If
            Return cCore.NULL_VALUE
        End Get
        Set(value As Single)
            If 1 <= iParam And iParam <= Me.nParams Then
                Me.m_params(iParam - 1) = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    Public ReadOnly Property ShapeFunctionParameters() As Single()
        Get
            Return Me.m_params
        End Get
    End Property

    Public Property ForcingApplicationType() As eForcingApplicationTypes
        Get
            Return Me.m_ForcingApplicationType
        End Get
        Set(ByVal value As eForcingApplicationTypes)
            Me.m_ForcingApplicationType = value
            Me.Update()
        End Set
    End Property

    ''' <summary>
    ''' Index of the shape in the list managers list of shape
    ''' </summary>
    ''' <remarks>This is a zero based index set when the shape is added to the manager (Construction of the shape) </remarks>
    Public Property ID() As Integer
        Get
            Return Me.m_ID
        End Get
        Friend Set(ByVal value As Integer)
            Me.m_ID = value
            '  Update()
        End Set
    End Property

    Public Property NYears() As Integer
        Get
            Return Me.m_nYears
        End Get
        Friend Set(ByVal value As Integer)
            Me.m_nYears = value
            Me.Update()
        End Set
    End Property

#End Region ' Public fields/properties

#Region " Constructors and Initialization "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new Forcing Function Shape from the underlying EcoSim Data.
    ''' </summary>
    ''' <param name="esData"><see cref="cEcosimDatastructures">Ecosim data structure</see>
    ''' to create the forcing function from.</param>
    ''' <param name="Manager"></param>
    ''' <param name="iDBID"></param>
    ''' <param name="DataType"></param>
    ''' <remarks>
    ''' This is used by the Manager to create forcing function from the 
    ''' underlying EcoSim data.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Friend Sub New(ByRef esData As cEcosimDatastructures, _
                   ByRef Manager As cBaseShapeManager, _
                   ByVal iDBID As Integer, _
                   ByVal DataType As eDataTypes)

        MyBase.New(esData.ForcePoints)

        m_bInInit = True
        m_data = esData

        m_datatype = DataType
        m_coreComponent = CoreComponent
        m_iDBID = iDBID

        m_manager = Manager 'keep a reference to the manager for this shape

        'Load()

        m_bInInit = False

    End Sub


    ''' <summary>
    ''' Initialize the propeties from the underlying EcoSim data structures for this shapes Database ID 
    ''' </summary>
    ''' <returns>True if successful</returns>
    ''' <remarks>This seperates creation from initialization so that an existing object can be repopluated from its underlying data</remarks>
    Protected Friend Overridable Function Load() As Boolean

        m_iEcoSimIndex = Array.IndexOf(m_data.ForcingDBIDs, m_iDBID)
        Debug.Assert(m_iEcoSimIndex <> -1, "Failed to find index for Shape.")

        If m_iEcoSimIndex = -1 Then Return False
        m_bInInit = True
        Me.LockUpdates()

        'copy the data from zscale into an array that will be used to create a forcing data object
        Me.Init(m_data.ForcePoints)
        For ipt As Integer = 1 To m_data.ForcePoints
            Me.ShapeData(ipt) = m_data.zscale(ipt, m_iEcoSimIndex)
        Next ipt

        Me.Name = m_data.ForcingTitles(m_iEcoSimIndex)
        Me.m_ForcingApplicationType = m_data.ForcingApplicationType(m_iEcoSimIndex)

        m_nYears = m_data.NumYears

        'shape parameters
        m_ShapeFunctionType = m_data.ForcingShapeParams(m_iEcoSimIndex).ShapeFunctionType
        m_params = CType(m_data.ForcingShapeParams(m_iEcoSimIndex).ShapeFunctionParams.Clone(), Single())

        Me.IsSeasonal = m_data.isSeasonal(m_iEcoSimIndex)

        Me.UnlockUpdates()
        m_bInInit = False

        Return True

    End Function

    ''' <inheritdocs cref="cShapeData.Dispose"/>
    Friend Overrides Sub Dispose()
        MyBase.Dispose()
        Me.m_data = Nothing
    End Sub

    ''' <inheritdocs cref="cShapeData.Clear"/>
    Public Overrides Sub Clear()
        MyBase.Clear()
    End Sub

#End Region ' Constructors and Initialization

#Region " Updating "

    ''' <summary>
    ''' Update the already existing underlying EcoSim data structures (m_data)
    ''' </summary>
    ''' <returns>True if succesful.</returns>
    ''' <remarks>This gets called by the cForcingData when it has been edited to update the existing EcoSim data</remarks>
    Public Overrides Function Update() As Boolean

        Try

            Debug.Assert(m_data IsNot Nothing, Me.ToString & ".Update() underlying ecosim data has not been set.")

            'do not update during initialization
            If m_bInInit Then
                'update will be call be the Forcing Data object (m_xData) when it is populated it has no way of knowing who is changing its value
                'so it has to call update on its parent
                Return False
            End If


            'turn the Database ID into an Array index using the Ecosim Data structures database ID this value should be good
            m_iEcoSimIndex = Array.IndexOf(m_data.ForcingDBIDs, m_iDBID)
            Debug.Assert(m_iEcoSimIndex >= 0, Me.ToString & ".Update() Failed to find index for Database ID " & m_iDBID)
            If (m_iEcoSimIndex = cCore.NULL_VALUE) Or (m_iEcoSimIndex > m_data.NumForcingShapes) Then
                cLog.Write(Me.ToString & ".Update() index out of bounds. Data not updated.")
                Return False
            End If

            'make sure the shape data is the same size as the EcoSim Shape data
            'this is a double check as the data size was checked when the forcing function was created by the Shape Manager
            'however it could have been changed by an interface at a later date
            Me.ResizeData(m_data.ForcePoints)

            'populate the raw shape data
            For ipt As Integer = 1 To Me.nPoints
                m_data.zscale(ipt, m_iEcoSimIndex) = Me.ShapeData(ipt)
            Next ipt
            m_data.ForcingTitles(m_iEcoSimIndex) = Me.Name

            m_data.ForcingShapeType(m_iEcoSimIndex) = m_datatype
            m_data.ForcingApplicationType(m_iEcoSimIndex) = Me.m_ForcingApplicationType

            'shape parameters
            m_data.ForcingShapeParams(m_iEcoSimIndex).ShapeFunctionType = m_ShapeFunctionType
            m_data.ForcingShapeParams(m_iEcoSimIndex).ShapeFunctionParams = CType(Me.m_params.Clone(), Single())

            m_data.isSeasonal(m_iEcoSimIndex) = Me.IsSeasonal()

            ShapeChanged()
            Return True

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".update() Error: " & ex.Message)
            cLog.Write(Me.ToString & ".update() Error: " & ex.Message)
            Return False

        End Try

    End Function

    ''' <summary>
    ''' Tell the manager that a shape has changed
    ''' </summary>
    Friend Sub ShapeChanged()

        'tell the manager that a shape has changed it's data
        If Not Me.IsLockedUpdates Then Me.m_manager.ShapeChanged(Me)

    End Sub

#End Region ' Updating

    Public Overridable Function ToCSVString() As String

        Return Me.Name '+ ", mean " + Me.m_p3.ToString + ", YZero " + Me.m_p0.ToString + ", YEnd " + Me.m_p2.ToString

    End Function

End Class ' cForcingFunction

#End Region ' Forcing Shape 

#Region " Mediation Shape "

#Region " cMediatingGroup "

''' <summary>
''' Group and Weight of a Group that make up a Mediating Group for a Mediation function. There can be more then one cMediatingGroup for a Mediation Function
''' </summary>
''' <remarks>This is the Group(s) that provide the Biomass for the X axis of a mediation function</remarks>
Public Class cMediatingGroup

    Public iGroupIndex As Integer
    Public Weight As Single

    ''' <summary>
    ''' Build a new Mediation Group
    ''' </summary>
    ''' <param name="iGroup">Index to the EcoPath/EcoSIm group this is the iGroup</param>
    ''' <param name="theWeight">Weight that is applied to this group 0-1</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal iGroup As Integer, ByVal theWeight As Single)

        iGroupIndex = iGroup
        'weight does not have to one or zero it can be any value it 
        Weight = theWeight

    End Sub

    Public Sub New()
        iGroupIndex = 0
        Weight = 0
    End Sub

    Public Overrides Function ToString() As String
        Return "Group Index=" & iGroupIndex.ToString & " Weight=" & Weight.ToString
    End Function

End Class

#End Region ' cMediatingGroup

#Region "cLandingsMediatingGroup"

''' <summary>
''' Mediation group for Price Elasticity.
''' cPriceMediatingGroup "Is A" cMediatingGroup with a fleet index that tell you what Fleet to get the Landings from
''' </summary>
''' <remarks></remarks>
Public Class cLandingsMediatingGroup
    Inherits cMediatingGroup

    Public iFleetIndex As Integer

    ''' <summary>
    ''' Build a new Mediation Group
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New(ByVal iGroup As Integer, ByVal iFleet As Integer, ByVal theWeight As Single)
        MyBase.New(iGroup, theWeight)

        iFleetIndex = iFleet

    End Sub

    Public Sub New()
        MyBase.New()
        iFleetIndex = 0
    End Sub

End Class

#End Region

#Region " cMediationFleet "

''' <summary>
''' Fleet and Weight of a Fleet that make up a Mediating Fleet for a Mediation function. There 
''' can be more then one cMediatingFleet for a Mediation Function
''' </summary>
''' <remarks>This defines the Fleet(s) that provide the Biomass for the X axis of a mediation function.</remarks>
Public Class cMediatingFleet

    Public iFleetIndex As Integer
    Public Weight As Single

    ''' <summary>
    ''' Build a new Mediation Fleet
    ''' </summary>
    ''' <param name="iFleet">Index to the EcoPath/EcoSim fleet.</param>
    ''' <param name="theWeight">Weight that is applied to this fleet [0-1]</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal iFleet As Integer, ByVal theWeight As Single)

        iFleetIndex = iFleet
        'weight does not have to one or zero it can be any value it 
        Weight = theWeight

    End Sub

    Public Sub New()
        iFleetIndex = 0
        Weight = 0
    End Sub

    Public Overrides Function ToString() As String
        Return "Fleet Index=" & iFleetIndex.ToString & " Weight=" & Weight.ToString
    End Function

End Class

#End Region ' cMediationFleet

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
        Me.m_ForcingApplicationType = eForcingApplicationTypes.NotSet

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
    ''' <param name="iFleet">The <see cref="cEcopathFleetInput.Index">ecopath index</see> of the fleet to add.</param>
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

#End Region ' cLandingsMediationFunction

#End Region ' cMediationFunction

#End Region ' Mediation Shape

#Region " Fishing Rate shape "

''' <summary>
''' A fish s
''' </summary>
''' <remarks></remarks>
Public Class cFishingRateShape
    Inherits cForcingFunction

    'Private m_ntimesteps As Integer

    Friend Sub New(ByVal EcoSimData As cEcosimDatastructures, ByVal Manager As cBaseShapeManager, ByVal DBID As Integer, ByVal strFleetName As String)

        MyBase.New(EcoSimData, Manager, DBID, eDataTypes.FishingEffort)

        m_bInInit = True
        Me.DBID = DBID

        m_data = EcoSimData
        'this can be changed to use the database id see load

        Me.Name = strFleetName ' the fleetname is only stored in the Ecopath Data so it has to be passed into the constructor

        'create a new forcing data object this only happens when the shape is created
        'if the shape is being initialized from the EcoSim the forcing object must already exist

        Load()

        m_bInInit = False

    End Sub


    ''' <summary>
    ''' Initialize the propeties from the underlying EcoSim data structures at the existing array index (iEcoSimIndex)
    ''' </summary>
    ''' <returns>True if successful</returns>
    ''' <remarks>This seperates creation from initialization so that an existing object can be repopluated from its underlying data</remarks>
    Protected Friend Overrides Function Load() As Boolean

        'copy the Fishing rate data into an array that will be used to create a forcing data object
        m_bInInit = True
        Dim m_ntimesteps As Integer = m_data.NTimes

        Debug.Assert(m_iEcoSimIndex > -1, Me.ToString & " database ID invalid.")
        If m_iEcoSimIndex = -1 Then Return False

        Me.ResizeData(m_ntimesteps)

        For ipt As Integer = 1 To m_ntimesteps
            Me.ShapeData(ipt) = m_data.FishRateGear(m_iEcoSimIndex, ipt) 'FishRateGear(NFleets,nTime)
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
        If (m_iEcoSimIndex = cCore.NULL_VALUE) Or (m_iEcoSimIndex > m_data.nGear + 1) Then
            cLog.Write(Me.ToString & ".update(m_data) index out of bounds. Data not updated.")
            Return False
        End If

        'make sure the shape data is the same size as the EcoSim Shape data
        'this is a double check as the data size was check when the forcing function was added to the Shape Manager
        'however it could have been changed be an interface at a later date
        'jb Sept-06 this can not happen for this type of object because all time dimensioning is handled by the core
        'and a new object will be created by the core with the new number of time steps changes when the time changes
        'm_Xdata.ResizeData(m_ntimesteps)

        Dim orgvalue As Single, newvalue As Single
        ' Dim bhaschanged As Boolean
        Dim isCombinedFleets As Boolean

        If m_iEcoSimIndex = m_data.nGear + 1 Then
            isCombinedFleets = True
        End If

        Debug.Assert(Me.m_data.NTimes >= Me.EndEditPoint, Me.ToString & " Warning Shape data contain more timestep then Ecosim data!")
        'we have to loop over all the time steps because we don't know what has changed
        For it As Integer = Me.StartEditPoint To Me.EndEditPoint
            orgvalue = m_data.FishRateGear(m_iEcoSimIndex, it)
            newvalue = Me.ShapeData(it)

            'update FishRateGear() with the new values 
            m_data.FishRateGear(m_iEcoSimIndex, it) = Me.ShapeData(it)

            'this shape is the combined fleets type so update all the fleets types with the new value
            If isCombinedFleets Then
                For iFlt As Integer = 1 To m_data.nGear
                    m_data.FishRateGear(iFlt, it) = m_data.FishRateGear(m_iEcoSimIndex, it) 'dont worry about overwriting the fleet we just updated
                Next
            End If

            'FishRateGear() is a multiplier that is used to change fishing effort from the base Ecopath value for a fleet
            'Now use FishRateGear/effort to update the fishing mortality for each group fished by this fleet
            For igrp As Integer = 1 To m_data.nGroups


                'don't change the fishing mortality if there is fishing mortality timeseries loaded for this group
                'JB 21-Feb-2011 Changed so Effort can be edited when F time series is loaded
                If Not isCombinedFleets Then
                    Dim cat As Single = Me.m_manager.Core.m_EcoPathData.Landing(Me.m_iEcoSimIndex, igrp) + Me.m_manager.Core.m_EcoPathData.Discard(Me.m_iEcoSimIndex, igrp)
                    If cat > 0 Then
                        'this fleet exploits this group update the F
                        m_data.FishRateNo(igrp, it) = m_data.FishRateNo(igrp, it) + m_data.FishMGear(m_iEcoSimIndex, igrp) * (m_data.FishRateGear(m_iEcoSimIndex, it) - orgvalue)
                        If m_data.FishRateNo(igrp, it) < 0 Then m_data.FishRateNo(igrp, it) = 0
                    End If

                Else
                    'combined fleet this changes all the mortality
                    m_data.FishRateNo(igrp, it) = 0
                    For iflt As Integer = 1 To m_data.nGear
                        m_data.FishRateNo(igrp, it) = m_data.FishRateNo(igrp, it) + m_data.FishMGear(iflt, igrp) * m_data.FishRateGear(iflt, it)
                        ' JS 3sept11: Fixed #1041
                        If m_data.FishRateNo(igrp, it) < 0 Then m_data.FishRateNo(igrp, it) = 0
                    Next iflt

                End If ' Not isCombinedFleets

            Next igrp
        Next it

        'tell the manager that a shape has changed its data
        ShapeChanged()

        Return True

    End Function

End Class

#End Region ' Fishing Rate Shape

#Region " Fish Mortality Shape "

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

#End Region ' Fish Mortality Shape

#Region " Response Function "

Public Class cEnviroResponseFunction
    Inherits cMediationBaseFunction

    Friend Sub New(ByVal EcoSimData As cEcosimDatastructures, ByVal Manager As cBaseShapeManager, _
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

#End Region

