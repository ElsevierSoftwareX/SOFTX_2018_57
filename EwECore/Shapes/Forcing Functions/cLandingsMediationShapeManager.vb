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
''' Implemenation of the Base class for Mediation shapes
''' </summary>
''' <remarks>
''' </remarks>
Public Class cLandingsMediationShapeManager
    Inherits cBaseShapeManager

    Private m_medData As cMediationDataStructures


    Friend Sub New(ByRef EcoSimData As cEcosimDatastructures, ByRef theCore As cCore, DataType As eDataTypes)
        MyBase.New(EcoSimData, theCore, DataType)

        Init()

    End Sub


    Public Overrides ReadOnly Property NPoints() As Integer
        Get
            Return m_medData.NMedPoints
        End Get
    End Property

    ''' <summary>
    ''' Create a new Landings shape
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function CreateNewShape(strName As String, points As Single(),
                                             Optional shapeType As Long = eShapeFunctionType.NotSet,
                                             Optional params As Single() = Nothing) As cForcingFunction

        Dim dbID As Integer
        Dim medFunct As cLandingsMediationFunction

        If m_core.AddShape(strName, m_DataType, dbID, points, shapeType, params) Then

            'create a new shape that is hooked up to the underlying ecosim data
            medFunct = New cLandingsMediationFunction(m_SimData, Me, Me.m_medData, dbID, m_DataType)
            medFunct.ID = m_shapes.Count
            medFunct.Load()

            'Add the new shape to the list 
            MyBase.Add(medFunct)

            m_core.onChanged(Me, eMessageType.DataAddedOrRemoved)

            Return medFunct

        End If

        Return Nothing

    End Function

    Friend Overrides Function Init() As Boolean
        Dim medFunct As cLandingsMediationFunction

        Me.m_medData = Me.m_SimData.PriceMedData

        'clear out any existing data
        m_shapes.Clear()

        For imed As Integer = 1 To m_medData.MediationShapes
            'All mediation shapes from the core will have an object 
            'A mediation function may have a shape but not have any Mediating Groups or weights (MedIsUsed(iMed) = False) 
            'Mediation function objects load there own group and weight data from the ecosim data via the Load() method
            medFunct = New cLandingsMediationFunction(m_SimData, Me, Me.m_medData, Me.m_medData.MediationDBIDs(imed), Me.m_DataType)
            medFunct.ID = m_shapes.Count
            medFunct.Load()
            m_shapes.Add(medFunct)

        Next imed
        Me.Load()

    End Function

End Class
