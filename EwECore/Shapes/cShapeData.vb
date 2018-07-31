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
''' Class to handle the data that makes up the shape of a forcing or mediation shape
''' </summary>
''' <remarks>This is used be all the Forcing or Mediation shapes</remarks>
Public MustInherit Class cShapeData
    Implements ICoreInterface
    Implements IDisposable

#Region " Private variables "

    'the core array index needs to be accessble by the derived classes
    Protected m_iEcoSimIndex As Integer = 0

    Protected m_datatype As eDataTypes = eDataTypes.NotSet
    Protected m_coreComponent As eCoreComponentType = eCoreComponentType.NotSet
    Protected m_iDBID As Integer = 0

    Private m_x1 As Integer
    Private m_x2 As Integer

    Private m_strName As String
    Private m_xdata() As Single
    Private m_nPoints As Integer
    Private m_bSeasonal As Boolean = False
    Protected m_timeresolution As eTSDataSetInterval = eTSDataSetInterval.TimeStep

    Public Event OnChanged(ByVal sd As cShapeData)

#End Region ' Private variables

#Region " Constructors "

    Sub New(ByVal NumberOfPoints As Integer)
        Init(NumberOfPoints)
    End Sub

    Sub New(ByVal ArrayOfData() As Single)
        Init(ArrayOfData)
    End Sub

    ''' <summary>
    ''' Destroys all data maintained by cShapeData. This will leave the
    ''' </summary>
    Friend Overridable Sub Dispose() _
            Implements IDisposable.Dispose
        Me.Clear()
        Me.m_xdata = Nothing
        GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' Clear out the data for further use
    ''' </summary>
    Public Overridable Sub Clear()
        ' NOP
    End Sub

#End Region ' Constructors

#Region " Capture "

    Private m_iLockCount As Integer = 0

    Public Sub LockUpdates()
        Me.m_iLockCount += 1
    End Sub

    Public Sub UnlockUpdates(Optional ByVal bUpdate As Boolean = True)
        Me.m_iLockCount -= 1
        If ((IsLockedUpdates() = False) And (bUpdate = True)) Then
            Me.Update()
        End If
    End Sub

    Public Function IsLockedUpdates() As Boolean
        Return Me.m_iLockCount <> 0
    End Function

#End Region ' Capture

#Region " Private methods "

    Protected Sub Init(ByVal NumberOfPoints As Integer)

        Debug.Assert(NumberOfPoints >= 0, "You can not initialize cForcingData with less than zero points.")
        m_nPoints = NumberOfPoints

        ReDim m_xdata(m_nPoints)
        Me.SetValue(1.0!)
        Me.setDefaultEditBlocks()
    End Sub

    Protected Sub Init(ByVal ArrayOfData() As Single)

        Me.m_nPoints = ArrayOfData.GetUpperBound(0)
        Debug.Assert(m_nPoints > 0, "You can not initialize cForcingData with zero points.")

        Me.m_xdata = ArrayOfData
        Me.setDefaultEditBlocks()

    End Sub

    ''' <summary>
    ''' Update the underlying EcoSim data by calling update on the CForcingFunction object that owns this data
    ''' </summary>
    ''' <remarks>This object does not know it's data is stored in the underlying EcoSim Data. 
    ''' That info is held by the CForcingFunction object that owns this data. This is because different shapes (Forcing or Mediation) store there data differently within the EcoSim data structures.
    ''' </remarks>
    Public MustOverride Function Update() As Boolean

    Public Sub SetValue(ByVal sValue As Single)
        For i As Integer = 0 To Me.m_nPoints
            Me.m_xdata(i) = sValue
        Next
        'm_Ymax = sValue
    End Sub


    Private Sub setDefaultEditBlocks()
        Me.m_x1 = 1
        Me.m_x2 = m_nPoints
    End Sub

#End Region ' Private methods

#Region " Properties "

    ''' <summary>
    ''' Returns the actual shape values. Note that his method returns a copy
    ''' of the original data array; making changes to the array returned here
    ''' will not be reflected in the original shape.
    ''' </summary>
    Public Property ShapeData() As Single()
        Get
            Return DirectCast(Me.m_xdata.Clone(), Single())
        End Get
        Set(ByVal value() As Single)
            Me.Init(value)
            If Not Me.IsLockedUpdates Then
                Me.Update()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Get/set a value in the shape for a given point.
    ''' </summary>
    ''' <param name="iPoint">The point to access.</param>
    Public Property ShapeData(ByVal iPoint As Integer) As Single
        Get
            Try
                If (iPoint < 0 Or iPoint > Me.m_nPoints) Then Return 0
                Return m_xdata(iPoint)
            Catch ex As Exception
                cLog.Write(Me.ToString & ".ShapeData(" & iPoint.ToString & ") Error: " & ex.Message)
                Debug.Assert(False, Me.ToString & ".ShapeData(" & iPoint.ToString & ") Error: " & ex.Message)
            End Try
        End Get

        Set(ByVal value As Single)
            Try
                If (iPoint < 0 Or iPoint > Me.m_nPoints) Then Return
                m_xdata(iPoint) = value

                If Not Me.IsLockedUpdates() Then Me.Update()
            Catch ex As Exception
                cLog.Write(Me.ToString & ".ShapeData(" & iPoint.ToString & ") Error: " & ex.Message)
                Debug.Assert(False, Me.ToString & ".ShapeData(" & iPoint.ToString & ") Error: " & ex.Message)
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Get the upper bound of the array. 
    ''' </summary>
    ''' <remarks>
    ''' This property cannot be used to resize the data. Call either 
    ''' <see cref="ResizeData">ResizeData</see> or build a new object
    ''' of the desired size.
    ''' </remarks>
    Public ReadOnly Property nPoints() As Integer
        Get
            Return m_nPoints
        End Get
    End Property

    ''' <summary>
    ''' Obsolete please use cShapeData.nPoints instead. 
    ''' </summary>
    <Obsolete("Property replaced by cShapeData.nPoints.")> _
    Public ReadOnly Property XMax() As Integer
        Get
            Return Me.nPoints
        End Get
    End Property

    ''' <summary>
    ''' Get the maximum value in the shape.
    ''' </summary>
    Public ReadOnly Property YMax() As Single
        Get
            Dim sYMax As Single = 0.0
            For i As Integer = 1 To Me.m_nPoints
                sYMax = Math.Max(sYMax, Me.m_xdata(i))
            Next
            Return sYMax
        End Get
    End Property

    ''' <summary>
    ''' Get/set the <see cref="eTSDataSetInterval"/> temporal resolution of a shape.
    ''' </summary>
    ''' <remarks>
    ''' This flag merely serves to direct a user interface how to reflect the data in a 
    ''' shape. The underlying shape data is still interpreted per timestep (for shapes).</remarks>
    Public Property TimeResolution() As eTSDataSetInterval
        Get
            Return Me.m_timeresolution
        End Get

        Set(ByVal timeresolution As eTSDataSetInterval)
            Me.m_timeresolution = timeresolution
            Me.Update()
        End Set
    End Property

    ''' <summary>
    ''' Get/set whether a shape is supposed to reflect a monthly pattern.
    ''' </summary>
    Public Property IsSeasonal() As Boolean
        Get
            Return Me.m_bSeasonal
        End Get

        Set(ByVal bSeasonal As Boolean)
            Me.m_bSeasonal = bSeasonal
            Me.Update()
        End Set
    End Property

    Public ReadOnly Property Mean() As Single
        Get

            If Me.m_nPoints = 0 Then Return 0

            Dim sum As Single
            For i As Integer = 1 To Me.m_nPoints
                sum += Me.m_xdata(i)
            Next
            Return sum / Me.m_nPoints
        End Get
    End Property

    ''' <summary>
    ''' First X Index of the current edit block
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>>When a user edits a shape this it the index of the first point</remarks>
    Public Property StartEditPoint() As Integer
        Get
            Return Me.m_x1
        End Get
        Set(ByVal value As Integer)
            'constrain the value
            If value < 1 Then value = 1
            If value > Me.m_nPoints Then value = Me.m_nPoints
            Me.m_x1 = value
        End Set
    End Property

    ''' <summary>
    ''' Last X Index of the current edit block
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>When a user edits a shape this it the index of the last point </remarks>
    Public Property EndEditPoint() As Integer
        Get
            Return Me.m_x2
        End Get
        Set(ByVal value As Integer)
            'constrain the value
            If value < 1 Then value = 1
            If value > Me.m_nPoints Then value = Me.m_nPoints
            Me.m_x2 = value
        End Set
    End Property

#End Region ' Properties

#Region " Friend methods "

    ''' <summary>
    ''' Resize the existing data to a new number of points this will preserve any existing data and populate new points with a value of one (1). 
    ''' New points will have no affect on the model.
    ''' </summary>
    ''' <param name="newNumberOfPoints">New number of points</param>
    ''' <returns>True if successful. False otherwise</returns>
    ''' <remarks>This is called by the Forcing or Mediation shape that owns this data (m_owner.update()) 
    ''' when it needs to update it's data or when the Shape has been added to the Manager. 
    ''' If this object has not been assigned to a Shape then this will not be called and it can hold any amount of data.
    ''' </remarks>
    Friend Function ResizeData(ByVal newNumberOfPoints As Integer) As Boolean

        Try

            Debug.Assert(newNumberOfPoints >= 0, Me.ToString & ".ResizeData() Must be greater then zero points.")

            'Does the data need resizing
            If newNumberOfPoints = m_nPoints Then
                'No
                Return False
            End If

            ReDim Preserve m_xdata(newNumberOfPoints)
            For i As Integer = m_nPoints + 1 To newNumberOfPoints
                m_xdata(i) = 1 'give all the new points the value of one this means they will have no effect on the model
            Next i

            Me.m_nPoints = newNumberOfPoints
            Me.setDefaultEditBlocks()

            Return True

        Catch ex As Exception
            cLog.Write(Me.ToString & ".ResizeData() Error: " & ex.Message)
            Return False
        End Try

    End Function

#End Region ' Friend methods

#Region " ICoreInterface implementation "


    Public Property DBID() As Integer _
        Implements ICoreInterface.DBID
        Get
            Return m_iDBID
        End Get
        Friend Set(ByVal value As Integer)
            m_iDBID = value
        End Set
    End Property

    Public Function GetID() As String _
        Implements ICoreInterface.GetID
        Return cValueID.GetDataTypeID(m_datatype, Me.DBID)
    End Function

    Public ReadOnly Property DataType() As eDataTypes _
        Implements ICoreInterface.DataType
        Get
            Return m_datatype 'datatype is set in the constructor of each class
        End Get
    End Property

    Public ReadOnly Property CoreComponent() As eCoreComponentType _
        Implements ICoreInterface.CoreComponent
        Get
            Return m_coreComponent
        End Get
    End Property

    ''' <summary>
    ''' Get/set the index of the time series in a time series dataset.
    ''' </summary>
    Public Property Index() As Integer Implements ICoreInterface.Index
        Get
            Return m_iEcoSimIndex
        End Get
        Friend Set(ByVal value As Integer)
            m_iEcoSimIndex = value
        End Set
    End Property

    ''' <summary>
    ''' Get/set the name of a time series.
    ''' </summary>
    Public Property Name() As String Implements ICoreInterface.Name
        Get
            Return Me.m_strName
        End Get
        Set(ByVal strName As String)
            m_strName = strName
        End Set
    End Property

#End Region ' ICoreInterface implementation

End Class
