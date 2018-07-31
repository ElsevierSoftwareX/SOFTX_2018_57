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
''' Extents the base class to manage the Forcing Shapes
''' </summary>
''' <remarks>cBaseShapeManager contains the code to manage the list. This will load the Forcing data only</remarks>
Public Class cForcingFunctionShapeManager
    Inherits cBaseShapeManager

    ''' <summary>
    ''' Creates and loads a new Forcing shape manager out from the EcoSim data
    ''' </summary>
    ''' <param name="EcoSimData">EcoSim data structures to load the forcing shapes from</param>
    ''' <param name="theCore">Reference to the Core that is used for functionality that only the core can know</param>
    ''' <param name="DataType"><see cref="eDataTypes">Data type</see> of shapes to load</param>
    ''' <remarks>This will create the new manager and load the data into shapes</remarks>
    Friend Sub New(ByRef EcoSimData As cEcosimDatastructures, ByRef theCore As cCore, DataType As eDataTypes)
        MyBase.New(EcoSimData, theCore, DataType)

        Init()

    End Sub

    ''' <summary>
    ''' Number of points in the underlying Shape data
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is provided for convenience. So the number of points can be retrieved without getting a shape.</remarks>
    Public Overrides ReadOnly Property NPoints() As Integer
        Get
            Return m_SimData.ForcePoints
        End Get
    End Property

    ''' <summary>
    ''' Creates a new shape
    ''' </summary>
    ''' <returns>A shape that has been added to the Shape Manager</returns>
    ''' <remarks>A shape cannot be created on its own. It must be created by this factory so that it is hooked up to the core data on creation. </remarks>
    Public Overrides Function CreateNewShape(strName As String, points As Single(),
                                             Optional shapeType As Long = eShapeFunctionType.NotSet,
                                             Optional parms As Single() = Nothing) As cForcingFunction

        Dim dbID As Integer
        Dim shape As cForcingFunction
        Dim iEcoSimIndex As Integer
        Dim bSucces As Boolean = True

        'Add storage to the underlying data arrays and the db
        'AddShape() will NOT preserve the existing data  
        'All the data in the Ecosim data structures will be reloaded from the database
        If m_core.AddShape(strName, m_DataType, dbID, points, shapeType, parms) Then

            'get the index from the dbid for the new shape
            iEcoSimIndex = Array.IndexOf(m_SimData.ForcingDBIDs, dbID)

            'create a new shape that contains a database ID to the underlying ecosim data
            shape = New cForcingFunction(m_SimData, Me, dbID, m_DataType)

            shape.ID = m_shapes.Count

            'tell the shape to load from the ecosim data
            'the call below to onChanged() will reload all the data this is not really necessary 
            'but it makes me feel safe
            shape.Load()

            'Add the new shape to the list 
            MyBase.Add(shape)

            'When the new shape was added to the EcoSim data all the existing data in memory was erased and re-loaded when the arrays where resized
            'Now tell all the Shape Managers to re-load the Ecosim data into their existing shapes
            m_core.onChanged(Me, eMessageType.DataAddedOrRemoved)

            Return shape

        End If

        Return Nothing
    End Function

    Friend Overrides Function Init() As Boolean
        Dim forcing As cForcingFunction

        'clear out any existing data
        m_shapes.Clear()
        For isp As Integer = 1 To m_SimData.NumForcingShapes

            If m_SimData.ForcingShapeType(isp) = m_DataType Then

                forcing = New cForcingFunction(m_SimData, Me, m_SimData.ForcingDBIDs(isp), m_DataType)
                'keep the index of this forcing function in the list in the function itself
                'it will be used later to return the list item for a given EcoSim array index
                forcing.ID = m_shapes.Count
                forcing.Load()

                'now Add it to the base class list so that it does not try to Add via the overridden Add in this class
                MyBase.Add(forcing)

            End If

        Next isp

        Me.Load()

    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

End Class
