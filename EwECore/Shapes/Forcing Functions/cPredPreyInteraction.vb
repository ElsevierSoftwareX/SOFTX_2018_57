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

Option Explicit On
Imports EwEUtils.Core
Imports System.ComponentModel

''' <summary>
''' Class to wrap the shape and function type modifiers for a pred/prey interaction
''' </summary>
''' <remarks>This will populate a list of five(MaxFunctions) shapes/functiontype pairs on construction. 
''' A user calls setShape(,,,) or getShape(,,,) to modify the shape or functiontype for this Pred Prey interaction. </remarks>
Public Class cPredPreyInteraction
    Inherits cMediatedInteraction

#Region "Private Data"

    Private m_pred As Integer
    Private m_prey As Integer
    Private m_bIsProd As Boolean

#End Region

#Region "Construction and Initialization"

    ''' <summary>
    ''' Create a new interaction.
    ''' </summary>
    ''' <param name="PredIndex"><see cref="cCoreGroupBase.Index">Predator index</see>.</param>
    ''' <param name="PreyIndex"><see cref="cCoreGroupBase.Index">Prey index</see>.</param>
    ''' <param name="manager"><see cref="cMediatedInteractionManager">Mediated interaction manager</see>.</param>
    Sub New(ByVal PredIndex As Integer, ByVal PreyIndex As Integer,
            ByVal manager As cMediatedInteractionManager, ApplicationTypes As List(Of eForcingFunctionApplication))

        Me.m_dbid = cCore.NULL_VALUE '???

        Me.m_pred = PredIndex
        Me.m_prey = PreyIndex
        Me.m_manager = manager

        Me.m_lstAppTypes = ApplicationTypes

        ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ''jb 4-Apr-2017 Allow more than one forcing or mediation function on a primary producer
        ''This allows for both forcing and mediation fuctions on a primary producer
        ''this logic comes from EwE5 frmAddFunction.Form_Load()
        'If (Me.m_pred = Me.m_prey) And (Me.m_manager.getEcoPathData.PP(Me.m_prey) = 1) Then
        '    Debug.Assert(False, Me.ToString + ".New()")
        '    Me.m_bIsProd = True
        'End If
        ''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        'initialize the list of shape/functiontype pairs with the number of function modifiers from Ecosim
        'Modifiers that are not used will have a NULL shape in the cShapeFunctionTypePair object
        For i As Integer = 1 To Me.MaxNumShapes
            Me.m_SFPairs.Add(New cShapeFunctionTypePair())
        Next

    End Sub

    ''' <summary>
    ''' Build the list of shapes used by this interaction from the underlying Ecosim data.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    Friend Overrides Function Load() As Boolean

        Dim esdata As cEcosimDatastructures = m_manager.getEcoSimData
        Dim SFPair As cShapeFunctionTypePair
        Dim bSucces As Boolean = True

        For i As Integer = 1 To cMediationDataStructures.MAXFUNCTIONS

            If esdata.BioMedData.FunctionNumber(m_prey, m_pred, i) = 0 Then Exit For

            If Me.m_lstAppTypes.Contains(esdata.BioMedData.ApplicationType(m_prey, m_pred, i)) Then

                'get the cShapeFunctionTypePair object for this index
                SFPair = m_SFPairs.Item(i - 1) 'Ecosim indexes are one based, m_SFPairs is zero based
                SFPair.FunctionType = esdata.BioMedData.ApplicationType(m_prey, m_pred, i)

                ' Retrieve shape
                If esdata.BioMedData.IsMedFunction(m_prey, m_pred, i) Then
                    SFPair.Shape = Me.getShapeFromEcosimIndex(Me.m_manager.getCore.MediationShapeManager, esdata.BioMedData.FunctionNumber(Me.m_prey, Me.m_pred, i))
                Else
                    SFPair.Shape = Me.getShapeFromEcosimIndex(Me.m_manager.getCore.ForcingShapeManager, esdata.BioMedData.FunctionNumber(Me.m_prey, Me.m_pred, i))
                End If

            End If

        Next i
        Return bSucces

    End Function

#End Region

#Region "Public Properties"

    ''' <summary>
    ''' Get the <see cref="cCoreGroupBase.Index">index</see> of the predator
    ''' for this interaction.
    ''' </summary>
    Public ReadOnly Property PredIndex() As Integer
        Get
            Return m_pred
        End Get
    End Property

    ''' <summary>
    ''' Get the <see cref="cCoreGroupBase.Index">index</see> of the prey for
    ''' this interaction.
    ''' </summary>
    Public ReadOnly Property PreyIndex() As Integer
        Get
            Return m_prey
        End Get
    End Property

    ''' <summary>
    ''' Get whether this interaction denotes a production rate interaction.
    ''' </summary>
    Public ReadOnly Property isProdRate() As Boolean
        Get
            Return m_bIsProd
        End Get
    End Property

    Public Overrides ReadOnly Property MaxNumShapes As Integer
        Get
            ' Can only apply one PP shape
            If Me.m_bIsProd Then Return 1

            Return Me.m_manager.MaxNShapes
        End Get
    End Property

#End Region

#Region "Editing and Updating"

    ''' <summary>
    ''' Update the underlying Ecosim data with the values in this pred prey interaction
    ''' </summary>
    ''' <remarks>The update does not communicate the update with the core that is done by what/who ever called the method. 
    ''' This allows a manager to update all the data then tell the core. </remarks>
    Friend Overrides Sub Update()
        Dim ishp As Integer
        Dim esdata As cEcosimDatastructures = m_manager.getEcoSimData

        If LockUpdates Then Return

        Try

            'this only need to set FunctionNumber(), FunctionType() and IsMedFunction() 
            'Ecosim will set MedIsUsed() in InitializeMedFunctions() based on FunctionNumber()
            For Each sfPair As cShapeFunctionTypePair In m_SFPairs
                ishp += 1
                If sfPair.Shape IsNot Nothing Then
                    esdata.BioMedData.FunctionNumber(m_prey, m_pred, ishp) = sfPair.Shape.Index 'Index to data arrays in Ecosim zscale()
                    esdata.BioMedData.ApplicationType(m_prey, m_pred, ishp) = sfPair.FunctionType
                    If TypeOf sfPair.Shape Is cMediationFunction Then
                        esdata.BioMedData.IsMedFunction(m_prey, m_pred, ishp) = True
                    Else
                        esdata.BioMedData.IsMedFunction(m_prey, m_pred, ishp) = False
                    End If
                Else
                    esdata.BioMedData.FunctionNumber(m_prey, m_pred, ishp) = 0
                    esdata.BioMedData.ApplicationType(m_prey, m_pred, ishp) = eForcingFunctionApplication.NotSet
                    esdata.BioMedData.IsMedFunction(m_prey, m_pred, ishp) = False 'this probable doesn't matter
                End If

            Next

            Me.m_manager.getCore.onChanged(Me)

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Update() " & ex.Message)
        End Try

        'End If

    End Sub

#End Region

#Region "ICoreInterface implementation"

    ''' <inheritdocs cref="ICoreInterface.DataType"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Overrides ReadOnly Property DataType() As eDataTypes
        Get
            Return eDataTypes.PredPreyInteraction
        End Get
    End Property

    ''' <inheritdocs cref="ICoreInterface.GetID"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Overrides Function GetID() As String
        Return cValueID.GetDataTypeID(Me.DataType, CInt(m_pred * 1000 + m_prey))
    End Function

#End Region

End Class
