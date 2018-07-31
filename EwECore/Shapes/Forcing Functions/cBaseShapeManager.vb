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
Imports System.ComponentModel

''' <summary>
''' Base class for a Shape Manager. Provides implementation to make a derived Shape Manager behave like a List (For Each). 
''' </summary>
''' <remarks>This provides For Each functionality to a Shape Manager by implementing the IEnumerable.GetEnumerator() interface. 
''' Behaviour specific to a Shape Manager must be implemented in a derived class.
''' </remarks>
Public MustInherit Class cBaseShapeManager
    Implements Collections.IEnumerable
    Implements ICoreInterface
    Implements IDisposable

#Region "Protected Variables"

    ''' <summary>underlying <see cref="cEcosimDatastructures">EcoSim data</see></summary>
    Protected m_SimData As cEcosimDatastructures
    ''' <summary>List of shapes owned by this manager.</summary>
    Protected m_shapes As New List(Of cForcingFunction)
    ''' <summary>Reference to the <see cref="cCore">core</see>.</summary>
    Protected m_core As cCore = Nothing
    ''' <summary><see cref="eDataTypes">Type of shape</see> this manager operates on.</summary>
    Protected m_DataType As eDataTypes = eDataTypes.NotSet

#End Region

#Region " Obligatory overrides "

    ''' <summary>
    ''' Initialize/build all the shapes that belong to this shape manager
    ''' </summary>
    ''' <returns>True if successful.</returns>
    Friend MustOverride Function Init() As Boolean

    ''' <summary>
    ''' Shapes can not be created outside the Shape Manager; they must be created by a ShapeManager.
    ''' </summary>
    ''' <returns>A valid shape if successfull. Otherwise Nothing</returns>
    Public MustOverride Function CreateNewShape(strName As String, points As Single(),
                                                Optional shapeType As Long = eShapeFunctionType.NotSet,
                                                Optional shapeParams As Single() = Nothing) As cForcingFunction

    ''' <summary>
    ''' Number of points in the data for this type of shape. This is specific to a ShapeManger implementation.
    ''' </summary>
    Public MustOverride ReadOnly Property NPoints() As Integer

#End Region ' Obligatory overrides

#Region " Constructor "

    ''' <summary>
    ''' Creates a new ShapeManager from the EcoSim data
    ''' </summary>
    ''' <param name="EcoSimData">EcoSim data used to populate the Shapes</param>
    ''' <remarks>New ShapeMangers can only be created by the Core so this is Declares as a Friend. Derived class should override the Init() function to initialize the Shapes. </remarks>
    Friend Sub New(ByRef EcoSimData As cEcosimDatastructures, ByRef theCore As cCore, DataType As eDataTypes)
        m_SimData = EcoSimData
        m_core = theCore
        m_DataType = DataType
    End Sub

    ''' <inheritdocs cref="IDisposable.Dispose"/>
    Friend Sub Dispose() _
        Implements IDisposable.Dispose
        Me.Clear()
        Me.m_core = Nothing
        Me.m_SimData = Nothing
        GC.SuppressFinalize(Me)
    End Sub

#End Region ' Constructor

#Region " Interface for List of shapes "

    ''' <summary>
    ''' Add a <see cref="cForcingFunction">shape</see> to the manager
    ''' </summary>
    ''' <param name="ForcingFunction">cForcingFunction or derived object to add to the ShapeManager and the underlying EcoSim data.</param>
    ''' <returns>True if Successfull</returns>
    ''' <remarks>Override this in a derived class to add the data in the cForcingFunction to the underlying EcoSim data. 
    ''' This will also work for cMediationFunction Objects as they use cForcingFunction as a base class.</remarks>
    Protected Overridable Overloads Function Add(ForcingFunction As cForcingFunction) As Boolean
        Try
            Me.m_shapes.Add(ForcingFunction)
            Me.UpdateIDs()
            Return True
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Get a <see cref="cForcingFunction">shape</see> from the manager.
    ''' </summary>
    ''' <param name="ItemIndex">The zero-based index of the shape to obtain.</param>
    Default Public Overridable ReadOnly Property Item(ItemIndex As Integer) As cForcingFunction
        Get
            Try
                Return m_shapes.Item(ItemIndex)
            Catch ex As Exception
                cLog.Write(Me.ToString & ".Item() Error: " & ex.Message)
                Return Nothing
            End Try

        End Get

    End Property

    ''' <summary>
    ''' Use a Core one based index to retrieve an item
    ''' </summary>
    ''' <param name="CoreOneBasedIndex">One based index to the item</param>
    Public Overridable ReadOnly Property CoreItem(CoreOneBasedIndex As Integer) As cForcingFunction
        Get
            Try
                'convert core one based index to zero base for list
                Return m_shapes.Item(CoreOneBasedIndex - 1)
            Catch ex As Exception
                cLog.Write(Me.ToString & ".CoreIndex() Error: " & ex.Message)
                Return Nothing
            End Try

        End Get
    End Property

    ''' <summary>
    ''' Number of Items(shapes) in this Shape Manager
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>The collection is zero based(0). So Count is one more then the last index i.e. ShapeManager.Item(ShapeManager.Count - 1) Will return the last Item in the list.  </remarks>
    Public ReadOnly Property Count() As Integer
        Get
            Return m_shapes.Count
        End Get
    End Property

    ''' <summary>
    ''' Implementation of IEnumerable.GetEnumerator provides access to the For Each statment
    ''' </summary>
    ''' <returns>The Enumerator of the List used by this object</returns>
    ''' <remarks></remarks>
    Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return m_shapes.GetEnumerator
    End Function

    ''' <summary>
    ''' Does this ShapeManager contain this cForcingFunction
    ''' </summary>
    ''' <param name="ForcingFunction">A cForcingFunction or cMediation object</param>
    ''' <returns>True if this cForcingFunction is in the Manager. False otherwise.</returns>
    ''' <remarks></remarks>
    Public Function Contains(ByRef ForcingFunction As cForcingFunction) As Boolean
        Try
            Return m_shapes.Contains(ForcingFunction)
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Remove a shape from the Manager and the underlying EcoSim Data
    ''' </summary>
    ''' <param name="ShapeToRemove">Valid shape to remove</param>
    ''' <returns>True if successful</returns>
    ''' <remarks></remarks>
    Public Overloads Function Remove(ByRef ShapeToRemove As cForcingFunction) As Boolean
        'ToDo_jb cForcingFunctionManager.Remove() Do I need to check if the shape exists before I try to remove it?????????
        Try

            'Remove all references to ShapeToRemove from Databse, EcoSim data arrays and All Shape Managers
            'this will remove this record from the database and re-load all EcoSim Data Arrays that are related to the shapes
            If Not m_core.RemoveShape(ShapeToRemove.DBID) Then Return False

            'remove the shape from the shape managers memory
            Me.m_shapes.Remove(ShapeToRemove)

            Me.UpdateIDs()

            'The structure of the underlying EcoSim data has changed because it was re-loaded above
            'So re-init both Forcing and Eggprod shape managers from the underlying EcoSim Data
            'it is not good enough to just init this manager as other shape managers were affected by changing the data
            m_core.onChanged(Me, eMessageType.DataAddedOrRemoved)

            Return True

        Catch ex As Exception
            Debug.Assert(False)
        End Try

    End Function

    Protected Sub UpdateIDs()
        Dim shape As cForcingFunction = Nothing
        For iShape As Integer = 0 To Me.Count - 1
            shape = Me.m_shapes(iShape)
            shape.ID = iShape
        Next iShape
    End Sub

#End Region ' Interface for List of shapes

#Region " Saving, loading and updating "

    ''' <summary>
    ''' Clear shapes from memory.
    ''' </summary>
    Friend Overridable Sub Clear()

        For Each shp As cForcingFunction In Me.m_shapes
            shp.Dispose()
        Next
        Me.m_shapes.Clear()

    End Sub

    ''' <summary>
    ''' Called by a shape to tell the manager that it has changed data. 
    ''' </summary>
    ''' <remarks>Tell the core that a shape has changed. </remarks>
    Friend Overridable Sub ShapeChanged(Optional shape As cShapeData = Nothing)
        m_core.onChanged(Me, eMessageType.DataModified)

        ' Send a shape changed message
        'Me.m_core.Messages.SendMessage()
    End Sub

    ''' <summary>
    ''' Populate the underlying EcoSim data structures with the forcing function data
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>This is dumb it will clear all the existing data and repopulte all the data. It has no idea what has changed </remarks>
    Public Overridable Function Update(Optional bUpdateAll As Boolean = True) As Boolean

        Try
            'have each shape will update the underlying EcoSim data
            For Each shape As cForcingFunction In Me
                If Not shape.Update() Then
                    cLog.Write(Me.ToString & ".Update() Shape failed to update DBID=" & shape.DBID.ToString)
                    Debug.Assert(False, Me.ToString & ".Update() Shape failed to update DBID=" & shape.DBID.ToString)
                    'this will keep trying to update the rest of the data
                    'even if there was a problem with one of the shapes
                End If
            Next shape

            Return True
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Update() Error: " & ex.Message)
        End Try

    End Function


    ''' <summary>
    ''' Load the existing shape with the underlying Ecosim data
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function Load() As Boolean
        Try
            'loop over all the shapes that belong to this shape manager and ask it to load
            'the shapes were constructed with a database ID and the underlying ecosim data
            For Each shape As cForcingFunction In Me
                If Not shape.Load() Then
                    cLog.Write(Me.ToString & ".Load() Shape failed to load DBID=" & shape.DBID.ToString)
                    Debug.Assert(False, Me.ToString & ".Load() Shape failed to load DBID=" & shape.DBID.ToString)
                    'keep loading the other shapes??????
                    'Return False
                End If

            Next

            Return True
        Catch ex As Exception
            Return False
            Debug.Assert(False, Me.ToString & ".Load() Error: " & ex.Message)
        End Try

    End Function

#End Region ' Saving, loading and updating

#Region " Protected methods "

    ''' <summary>
    ''' Convert an array index from the underlying data in EcoSim into the Forcing function that is stored in the list
    ''' </summary>
    ''' <param name="iEcoSimIndex"></param>
    ''' <param name="theForcingShape"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function getShapeForEcoSimArrayIndex(iEcoSimIndex As Integer, ByRef theForcingShape As cForcingFunction) As Boolean
        Dim ff As cForcingFunction

        'Hack loop over each forcing function until one is found with a matching iEcoSimIndex
        'iEcoSimIndex was populated in init() with the array index of this forcing function
        'return the actual forcing shape in the argument theForcingShape
        For Each ff In Me
            If ff.Index = iEcoSimIndex Then
                theForcingShape = ff
                Return True
            End If
        Next ff

        'ToDo something better then this. Failed to find forcing function in the list
        'Debug.Assert(False, "Failed to find forcing Function for " & iEcoSimIndex.ToString)
        'cLog.Write(Me.ToString & ".getShapeForEcoSimArrayIndex() Failed to find forcing Function for " & iEcoSimIndex.ToString)
        theForcingShape = Nothing
        Return False

    End Function


#End Region ' Protected methods

#Region " Public Properties "

    Public ReadOnly Property Core() As cCore
        Get
            Return Me.m_core
        End Get
    End Property

#End Region

#Region " ICoreInterface Implementation "

    ''' <inheritdocs cref="ICoreInterface.DataType"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public ReadOnly Property DataType() As eDataTypes Implements ICoreInterface.DataType
        Get
            Return m_DataType
        End Get
    End Property

    ''' <inheritdocs cref="ICoreInterface.CoreComponent"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public ReadOnly Property CoreComponent() As eCoreComponentType Implements ICoreInterface.CoreComponent
        Get
            Return eCoreComponentType.ShapesManager
        End Get
    End Property

    ''' <inheritdocs cref="ICoreInterface.DBID"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public Property DBID() As Integer Implements ICoreInterface.DBID
        Get
            Return cCore.NULL_VALUE
        End Get
        Set(value As Integer)
            Debug.Assert(False, "Not Implemented")
        End Set
    End Property

    ''' <inheritdocs cref="ICoreInterface.GetID"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public Function GetID() As String Implements ICoreInterface.GetID
        Dim id As Integer = CType(m_DataType, Integer)
        Return cValueID.GetDataTypeID(m_DataType, id)
    End Function

    ''' <inheritdocs cref="ICoreInterface.Index"/>
    Public Property Index() As Integer Implements ICoreInterface.Index
        Get
            Return cCore.NULL_VALUE
        End Get
        Set(value As Integer)
            Debug.Assert(False, "Not Implemented")
        End Set
    End Property

    ''' <inheritdocs cref="ICoreInterface.Name"/>
    Public Property Name() As String Implements ICoreInterface.Name
        Get
            Return Me.ToString
        End Get
        Set(value As String)
            Debug.Assert(False, "Not Implemented")
        End Set
    End Property

#End Region ' ICoreInterface Implementation

#Region " Deprecated "

    ''' <summary>
    ''' Shapes can not be created outside the Shape Manager; they must be created by a ShapeManager.
    ''' </summary>
    ''' <returns>A valid shape if successfull. Otherwise Nothing</returns>
    <Obsolete("Use CreateNewShape(name, data, shapetype, parms) instead")>
    Public Function CreateNewShape(strName As String, points As Single(),
                                   sYZero As Single, sYBase As Single,
                                   sYEnd As Single, sSteep As Single,
                                   Optional shapeType As Long = eShapeFunctionType.NotSet) As cForcingFunction
        Return Me.CreateNewShape(strName, points, shapeType, New Single() {sYZero, sYBase, sYEnd, sSteep})
    End Function

#End Region ' Deprecated

End Class

' ''' <summary>
' ''' Implemenation of the Base class for capacity shapes
' ''' </summary>
'Public Class cEcosimResponseShapeManager
'    Inherits cBaseShapeManager

'    Private m_medData As cMediationDataStructures

'    Friend Sub New(ByRef EcoSimData As cEcosimDatastructures, ByRef theCore As cCore, DataType As eDataTypes)
'        MyBase.New(EcoSimData, theCore, DataType)

'        Me.Init()

'    End Sub


'    Public Overrides ReadOnly Property NPoints() As Integer
'        Get
'            Return m_medData.NMedPoints
'        End Get
'    End Property

'    ''' <summary>
'    ''' Create a new Mediation shape
'    ''' </summary>
'    Public Overrides Function CreateNewShape(strName As String, asData As Single(), _
'            Optional sYZero As Single = 0, Optional sYBase As Single = 0, _
'            Optional sYEnd As Single = 0, Optional sSteep As Single = 0, _
'            Optional shapeType As Long = eShapeFunctionType.NotSet) As cForcingFunction

'        Dim dbID As Integer

'        If m_core.AddShape(strName, eDataTypes.CapacityMediation, dbID, asData, sYZero, sYBase, sYEnd, sSteep, shapeType) Then

'            Dim medFunct As cEnviroResponseFunction

'            'create a new shape that is hooked up to the underlying ecosim data
'            medFunct = New cEnviroResponseFunction(m_SimData, Me, Me.m_medData, dbID, m_DataType)
'            medFunct.ID = m_shapes.Count
'            medFunct.Load()

'            medFunct.ShapeFunctionType = shapeType

'            'Add the new shape to the list 
'            MyBase.Add(medFunct)

'            m_core.onChanged(Me, eMessageType.DataAddedOrRemoved)

'            Return medFunct

'        End If

'        Return Nothing

'    End Function

'    Friend Overrides Function Init() As Boolean
'        Dim medFunct As cEnviroResponseFunction

'        'get the Enviromental response function for Capacity 
'        m_medData = Me.m_SimData.CapEnvResData

'        'clear out any existing data
'        m_shapes.Clear()

'        For imed As Integer = 1 To m_medData.MediationShapes
'            'All mediation shapes from the core will have an object 
'            medFunct = New cEnviroResponseFunction(m_SimData, Me, Me.m_medData, m_medData.MediationDBIDs(imed), Me.m_DataType)
'            medFunct.ID = m_shapes.Count
'            medFunct.Load()
'            m_shapes.Add(medFunct)

'        Next imed
'        Me.Load()

'    End Function

'End Class
