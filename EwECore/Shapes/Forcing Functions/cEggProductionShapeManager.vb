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
'''Manager for the Egg Production Shapes
''' </summary>
''' <remarks> Egg production and Forcing shapes are stored in the same data structures in EcoSim so most of their functionality is in cForcingFunctionManager. 
''' The only real difference is in how the data is applied to groups. Egg Production can only be applied to a Stanza Group.
'''  So the manager contains a list of all the Stanza Groups that have an associated Egg Production shape.</remarks>
Public Class cEggProductionShapeManager
    Inherits cForcingFunctionShapeManager

    Private m_grplist As cGroupShapeList

    Friend Sub New(ByRef EcoSimData As cEcosimDatastructures, ByRef theCore As cCore, DataType As eDataTypes)
        MyBase.New(EcoSimData, theCore, DataType)

    End Sub

    Public Overrides Function Load() As Boolean

        Dim rv As Boolean

        rv = MyBase.Load()
        rv = rv And Me.LoadGroupShapeList()

        Return rv

    End Function

    ''' <summary>
    ''' Overrides the base class CForcingFunctionManager InitAppliesTo() to initialize the cAppliesToList with the EggProduction forcing data from EcoSim
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>EggProduction forcing data points are stored in the same data structures as Time Forcing data cEcoSimDataStructures.zscale(). 
    ''' so cEggProductionManager can use the same InitShapes routine as it's base class cForcingFunctionManager</remarks>
    Private Function LoadGroupShapeList() As Boolean
        Try
            Dim shape As cForcingFunction
            m_grplist = New cGroupShapeList(m_SimData, MyBase.m_core.m_Stanza, Me)

            'EggProdShape data is only for Stanza groups data 
            'EggProdShapeSplit(iStanza) holds the forcing function shape index for this stanza group
            For iStanza As Integer = 1 To m_core.m_Stanza.Nsplit 'nSplit is the number of stanza groups

                Me.getShapeForEcoSimArrayIndex(m_core.m_Stanza.EggProdShapeSplit(iStanza), shape)
                'make the stanza index zero based 
                m_grplist.Add(New cGroupShapePair(Me, shape, iStanza))

            Next iStanza

            Return True

        Catch ex As Exception
            cLog.Write(Me.ToString & ".InitAppliesTo() Error:" & ex.Message)
            Debug.Assert(False, Me.ToString & ".InitAppliesTo() Error:" & ex.Message)
        End Try


    End Function

    Public ReadOnly Property GroupShapeList() As cGroupShapeList
        Get
            Return m_grplist
        End Get
    End Property

    Friend Sub validationFailedMessage()
        ' ToDo: globalize this
        m_core.Messages.SendMessage(New cMessage("Validataion Failed. Egg Production no shape with this index.", eMessageType.DataValidation,
                                    eCoreComponentType.ShapesManager, eMessageImportance.Information, eDataTypes.EggProd))
    End Sub


    ''' <summary>
    ''' Tell the core that data has been changed
    ''' </summary>
    ''' <remarks>Called by a GroupShapePair when its data has changed</remarks>
    Friend Function OnChanged(ByRef GroupShapePair As cGroupShapePair) As Boolean

        Try
            'neither of these should ever happen 
            Debug.Assert(GroupShapePair.iShape <= m_core.m_EcoSimData.NumForcingShapes, Me.ToString & ".OnChanged() shape index out of bounds.")
            Debug.Assert(GroupShapePair.iCoreStanzaIndex <= m_core.m_Stanza.Nsplit, Me.ToString & ".OnChanged() stanza index out of bounds.")

            'update the cores data
            m_core.m_Stanza.EggProdShapeSplit(GroupShapePair.iCoreStanzaIndex) = GroupShapePair.iShape

            'Tell the core that this data has changed
            m_core.onChanged(Me, eMessageType.DataModified)
            Return True


        Catch ex As Exception
            cLog.Write(ex)
            Debug.Assert(False, ex.StackTrace)
        End Try

    End Function

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
