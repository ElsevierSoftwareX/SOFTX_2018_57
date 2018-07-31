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
''' Class to wrap the shape and function type modifiers for a meditated interaction
''' </summary>
''' <remarks>This will populate a list of five(MaxFunctions) shapes/functiontype pairs on construction. 
''' A user calls setShape(,,,) or getShape(,,,) to modify the shape or functiontype for this Pred Prey interaction. </remarks>
Public MustInherit Class cMediatedInteraction
    Implements ICoreInterface

#Region "Private Data"

#Region " Protected class "

    'ToDo_jb cPredPreyInteraction needs to set the needs update

    ''' <summary>
    ''' Private class to hold the shape and function type for each possible modifier.
    ''' </summary>
    Protected Class cShapeFunctionTypePair
        Public Shape As cForcingFunction = Nothing
        Public FunctionType As eForcingFunctionApplication = eForcingFunctionApplication.NotSet
    End Class

#End Region

    Protected m_manager As cMediatedInteractionManager
    Protected m_SFPairs As New List(Of cShapeFunctionTypePair)
    Protected m_dbid As Integer

    ''' <summary>
    '''List of Application types this that this interation applies to
    '''Passed into the Constructor
    ''' </summary>
    Protected m_lstAppTypes As List(Of eForcingFunctionApplication)

#End Region

#Region "Construction and Initialization"

    ''' <summary>
    ''' Build the list of shapes used by this interaction from the underlying Ecosim data.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    Friend MustOverride Function Load() As Boolean

    Public Sub Clear()
        Try
            Me.m_SFPairs.Clear()
        Catch ex As Exception
            cLog.Write(ex)
        End Try
    End Sub

#End Region

#Region "Public Properties"

    ''' <summary>
    ''' Get the maximum number of shapes that can be assigned to a 
    ''' pred/prey interaction.
    ''' </summary>
    Public MustOverride ReadOnly Property MaxNumShapes() As Integer

    ''' <summary>
    ''' Get the number of shapes that are used by this predator/prey interaction.
    ''' </summary>
    ''' <remarks>The first shape that is Nothing marks the end of the series. 
    ''' No shapes after that will be used.</remarks>
    Public ReadOnly Property nAppliedShapes() As Integer

        Get
            Dim n As Integer
            'count the number of shapes that are used 
            'all shapes after the first null shape are not used
            For Each sfpair As cShapeFunctionTypePair In m_SFPairs
                If sfpair.Shape IsNot Nothing Then
                    n += 1
                Else
                    Exit For
                End If
            Next

            Return n

        End Get

    End Property

#End Region

#Region "Editing and Updating"

    ''' <summary>
    ''' Get a shape modifier, consisting of a <see cref="cForcingFunction">forcing funtion</see> and 
    ''' a <see cref="eForcingFunctionApplication">Type of variable</see>, defined at a given index.
    ''' </summary>
    ''' <param name="iItem">One-based index of the <see cref="cForcingFunction">shape</see> and 
    ''' <see cref="eForcingFunctionApplication">FunctionType</see> to retreive. There can 
    ''' be up to <see cref="MaxNumShapes">MaxNumShapes</see> for a pred prey interaction.</param>
    ''' <param name="Shape">A reference to the shape that is used for this pred/prey 
    ''' interaction.</param>
    ''' <param name="FunctionType"><see cref="eForcingFunctionApplication">Type of variable</see>
    ''' that this modifier applies to.</param>
    ''' <returns>True if there is a shape modifier defined at this index.</returns>
    Public Function getShape(ByVal iItem As Integer, _
                             ByRef shape As cForcingFunction, _
                             ByRef functiontype As eForcingFunctionApplication) As Boolean

        Dim esdata As cEcosimDatastructures = m_manager.getEcoSimData

        Try

            ' Sanity checks
            Debug.Assert(iItem > 0 And iItem <= cMediationDataStructures.MAXFUNCTIONS, Me.ToString & ".getShape() ItemIndex out of bounds.")

            If iItem > cMediationDataStructures.MAXFUNCTIONS Or iItem < 1 Then
                shape = Nothing
                Return False
            End If

            'm_SFPairs list is zero based
            'indexes in the interface are one based
            Dim iList As Integer = iItem - 1

            Dim pair As cShapeFunctionTypePair = m_SFPairs.Item(iList)
            shape = pair.Shape
            functiontype = pair.FunctionType

            If shape IsNot Nothing Then
                Return True
            Else
                'no shape defined for this index
                Return False
            End If

        Catch ex As Exception
            Debug.Assert(False, "Error: " & Me.ToString & ".getShape() " & ex.Message)
            shape = Nothing
            Return False
        End Try

    End Function


    ''' <summary>
    ''' Set a shape modifier, consisting of a <see cref="cForcingFunction">forcing function</see> and 
    ''' <see cref="eForcingFunctionApplication">function type</see>, for a given index.
    ''' </summary>
    ''' <param name="ItemIndex">One-base index of the shape to set. There can be 
    ''' up to <see cref="MaxNumShapes">MaxNumShapes</see> for a pred/prey interaction.</param>
    ''' <param name="Shape"><see cref="cForcingFunction">Shape</see> to use for this 
    ''' pred/prey interaction index. If the shape is Nothing/Null then no modifier will be 
    ''' used for this pred/prey interaction index.</param>
    ''' <param name="FunctionType"><see cref="eForcingFunctionApplication">Type of variable</see>
    ''' to apply this modifier to.</param>
    ''' <returns>True is the index was in bounds and the shape was set</returns>
    ''' <remarks>To clear an index set the shape to Nothing</remarks>
    Public Function setShape(ByVal ItemIndex As Integer, _
                             ByVal shape As cForcingFunction, _
                             Optional ByVal FunctionType As eForcingFunctionApplication = eForcingFunctionApplication.SearchRate) As Boolean

        Dim esdata As cEcosimDatastructures = m_manager.getEcoSimData

        Try

            If (ItemIndex > cMediationDataStructures.MAXFUNCTIONS) Or (ItemIndex < 1) Then
                shape = Nothing
                Debug.Assert(False, Me.ToString & ".setShape() ShapeIndex out of bounds.")
                Return False
            End If

            'm_SFPairs list is zero based
            'indexes in the interface are one based
            Dim iList As Integer = ItemIndex - 1

            'set the shape object and the function type
            'in the already existing cShapeFunctionTypePair object from the m_SFPairs list
            'the cShapeFunctionTypePair objects were created when this interaction object was constructed
            Dim sfPair As cShapeFunctionTypePair = m_SFPairs.Item(iList)
            sfPair.Shape = shape
            sfPair.FunctionType = FunctionType

            'update the ecosim data
            Me.Update()

            Return True

        Catch ex As Exception
            Debug.Assert(False, "Error: " & Me.ToString & ".setShape() " & ex.Message)
            shape = Nothing
            Return False
        End Try

    End Function

    Dim m_bLockUpdates As Boolean = False

    ''' <summary>
    ''' Get/set whether updates should not be sent to the core. This functionality 
    ''' is particularly useful when making a series of changes to pred/prey interactions.
    ''' </summary>
    Public Property LockUpdates() As Boolean
        Get
            Return m_bLockUpdates
        End Get
        Set(ByVal value As Boolean)
            m_bLockUpdates = value
            Me.Update()
        End Set
    End Property

    ''' <summary>
    ''' Update the underlying Ecosim data with the values in this pred prey interaction
    ''' </summary>
    ''' <remarks>The update does not communicate the update with the core that is done by what/who ever called the method. 
    ''' This allows a manager to update all the data then tell the core. </remarks>
    Friend MustOverride Sub Update()

#End Region

#Region " Internal Methods "

    Protected Function getShapeFromEcosimIndex(ByRef theManager As cBaseShapeManager, ByVal iEcosimIndex As Integer) As cForcingFunction

        'HACK find a shape with the matching Ecosim index in the theManager
        For Each shape As cForcingFunction In theManager
            If shape.Index = iEcosimIndex Then
                Return shape
            End If
        Next
        'Debug.Assert(False, Me.ToString & ".Failed to find shape.")
        Return Nothing
    End Function

#End Region

#Region "ICoreInterface implementation"

    ''' <inheritdocs cref="ICoreInterface.DataType"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public MustOverride ReadOnly Property DataType() As eDataTypes Implements ICoreInterface.DataType

    ''' <inheritdocs cref="ICoreInterface.GetID"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public MustOverride Function GetID() As String Implements ICoreInterface.GetID

    ''' <inheritdocs cref="ICoreInterface.CoreComponent"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public ReadOnly Property CoreComponent() As eCoreComponentType Implements ICoreInterface.CoreComponent
        Get
            Return eCoreComponentType.EcoSim
        End Get
    End Property

    ''' <inheritdocs cref="ICoreInterface.DBID"/>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Property DBID() As Integer Implements ICoreInterface.DBID
        Get
            Return m_dbid
        End Get
        Set(ByVal value As Integer)
            m_dbid = value
        End Set
    End Property

    ''' <inheritdocs cref="ICoreInterface.Index"/>
    Public Property Index() As Integer Implements ICoreInterface.Index
        Get
            Return cCore.NULL_VALUE
        End Get
        Set(ByVal value As Integer)
            Debug.Assert(False, "Not Implemented")
        End Set
    End Property

    ''' <inheritdocs cref="ICoreInterface.Name"/>
    Public Property Name() As String Implements ICoreInterface.Name
        Get
            Return "Predator/Prey interaction"
        End Get
        Set(ByVal value As String)
            Debug.Assert(False, "Not Implemented")
        End Set
    End Property

#End Region

End Class
