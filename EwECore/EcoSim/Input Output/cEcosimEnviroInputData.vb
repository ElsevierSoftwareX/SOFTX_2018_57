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

#Region " Imports "

Option Strict On
Imports EwEUtils.Core
Imports EwECore.ValueWrapper

#End Region ' Imports

Public Class cEcosimEnviroInputData
    Implements IEnviroInputData

#Region " Private vars "

    Private m_source As cForcingFunction = Nothing
    Private m_GrpToShape() As Integer
    Private m_MedData As cMediationDataStructures
    Private m_EcosimData As cEcosimDatastructures
    Private m_min As Single
    Private m_max As Single
    Private m_mean As Single
    Private m_binWidth As Single
    Private m_manager As IEnvironmentalResponseManager
    Private m_iTSIndex As Integer

#End Region ' Private vars

#Region "Construction Initialization"

    Public Sub New(ByVal EcosimResponseManager As IEnvironmentalResponseManager, ByVal shape As cForcingFunction)
        Me.m_manager = EcosimResponseManager
        Me.m_source = shape
        Me.m_iTSIndex = shape.Index
    End Sub

    Public Function Init(MediationData As cMediationDataStructures, EcosimData As cEcosimDatastructures) As Boolean Implements IEnviroInputData.Init
        Me.m_MedData = MediationData
        Me.m_EcosimData = EcosimData

        ReDim Me.m_GrpToShape(Me.nGroups)
    End Function

#End Region

#Region "Public functions"

    ''' <inheritdocs cref="IEnviroInputData.Update"/>
    Public Function Update() As Boolean Implements IEnviroInputData.Update
        Dim bReturn As Boolean = False
        Try
            Me.computeMinMax()
            bReturn = True
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".Update() Exception: " & ex.Message)
        End Try
        Return bReturn

    End Function

    ''' <inheritdocs cref="IEnviroInputData.setManager"/>
    Friend Sub setManager(ByVal theManager As IEnvironmentalResponseManager) _
        Implements IEnviroInputData.SetManager
        '  Me.m_manager = theManager
    End Sub

    ''' <inheritdocs cref="IEnviroInputData.Histogram"/>
    Public Function Histogram() As Drawing.PointF() Implements IEnviroInputData.Histogram

        Dim ipt As Integer ', maxPts As Integer
        Dim nBins As Integer = 100
        Dim pts() As Drawing.PointF
        'Dim nPts As Integer
        ReDim pts(nBins)
        Me.computeMinMax()
        Dim range As Single = Me.Max - Me.Min
        'Make sure there is data in the map
        If range > 0 Then
            Me.m_binWidth = range / nBins
        Else
            'No data in the map so just set a default binwidth 
            'this will dump all the data into the zero bin
            Me.m_binWidth = 1.0F / CSng(nBins)
        End If

        Try
            Dim nFpts As Integer = Math.Min(Me.m_EcosimData.NTimes, Me.m_EcosimData.ForcePoints)

            For ifp As Integer = 1 To nFpts
                Dim value As Single = Me.m_EcosimData.zscale(ifp, Me.m_iTSIndex)
                ipt = CInt(Math.Truncate((value - Me.Min) / m_binWidth)) + 1
                If ipt >= nBins Then ipt = nBins
                If ipt <= 0 Then ipt = 1
                pts(ipt).Y += 1
                ' nPts += 1
            Next

            If nFpts = 0 Then nFpts = 1

            'Normalize the histogram
            '29-Sept-2011 make it the percentage instead
            For i As Integer = 1 To nBins
                pts(i).X = CSng(Me.Min + m_binWidth * i)
                'normalize the data
                'pts(i).Y = pts(i).Y / maxPts
                pts(i).Y = pts(i).Y / nFpts
            Next

        Catch ex As Exception

        End Try

        Return pts

    End Function

#End Region

#Region " Properties "

    Public ReadOnly Property Index As Integer
        Get
            Return Me.m_iTSIndex
        End Get
    End Property

    Public ReadOnly Property nGroups() As Integer
        Get
            ' ToDo: remove, obtain from core
            Return Me.m_EcosimData.nGroups
        End Get
    End Property

    Public ReadOnly Property nFleets() As Integer
        Get
            ' ToDo: remove, obtain from core
            Return Me.m_EcosimData.nGear
        End Get
    End Property

    ''' <inheritdocs cref="IEnviroInputData.Max"/>
    Public ReadOnly Property Max() As Single _
        Implements IEnviroInputData.Max
        Get
            Return Me.m_max
        End Get
    End Property

    ''' <inheritdocs cref="IEnviroInputData.Mean"/>
    Public ReadOnly Property Mean() As Single _
        Implements IEnviroInputData.Mean
        Get
            Return Me.m_mean
        End Get
    End Property

    ''' <inheritdocs cref="IEnviroInputData.Min"/>
    Public ReadOnly Property Min() As Single _
        Implements IEnviroInputData.Min
        Get
            Return Me.m_min
        End Get
    End Property

    ''' <summary>
    ''' The basemap layer that provides the data that this map operates onto.
    ''' </summary>
    Public ReadOnly Property Layer As cForcingFunction
        Get
            Return Me.m_source
        End Get
    End Property

    Public ReadOnly Property HistogramBinWidth As Single Implements IEnviroInputData.HistogramBinWidth
        Get
            Return Me.m_binWidth
        End Get
    End Property

    Public Function ResponseFunction(iGroup As Integer, iTimeStep As Integer) As Single Implements IEnviroInputData.ResponseFunction

        Dim iShp As Integer = 0

        Try
            iShp = Me.ResponseIndexForGroup(iGroup)
            'Response(shape) index of -9999 means there is no shape set for this Map/Group
            If iShp <= 0 Then
                'No shape has been set for this group
                'need to decide what the null response should be
                Return 0
            End If

            Return Me.m_MedData.getEnviroResponse(iShp, Me.m_EcosimData.zscale(iTimeStep, Me.m_iTSIndex))

        Catch ex As Exception
            Debug.Assert(False)
        End Try
    End Function

    ''' <summary>
    ''' Sets or gets the response(mediation) function index to use from the current cMediationDataStructures load during the Init(...)
    ''' </summary>
    ''' <param name="GrpIndex">One-based group index for the response function.</param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>The Index of the ResponseFunction must exist in the underlying mediation data.</remarks>
    Public Property ResponseIndexForGroup(ByVal GrpIndex As Integer, Optional ByVal bUpdateMaps As Boolean = True) As Integer _
        Implements IEnviroInputData.ResponseIndexForGroup
        Get
            Return Me.m_GrpToShape(GrpIndex)
        End Get

        Set(ByVal ResponseShapeIndex As Integer)
            If ResponseShapeIndex <= Me.m_MedData.MediationShapes And GrpIndex <= Me.nGroups Then
                ''Response index(shape index) of -9999 NULL_VALUE means there is no response set for this Map/Group
                Me.m_GrpToShape(GrpIndex) = ResponseShapeIndex

                'If the manager is nothing the response index was set during initialization
                'The manager is not initialized until an Ecospace scenario is loaded
                If (Not Me.m_manager Is Nothing And bUpdateMaps) Then
                    Me.m_manager.onChanged()
                End If

            End If
        End Set
    End Property

#End Region ' Properties

#Region "Private Functions"

    Private Sub computeMinMax()

        m_min = Single.MaxValue
        m_max = Single.MinValue

        Try

            For ipt As Integer = 1 To Me.m_EcosimData.ForcePoints
                Dim value As Single = Me.m_EcosimData.zscale(ipt, Me.m_iTSIndex)
                Me.m_min = Math.Min(value, Me.m_min)
                Me.m_max = Math.Max(value, Me.m_max)
            Next

        Catch ex As Exception
            ' Argh
        End Try

        Me.m_mean = (Me.m_min + Me.m_max) * 0.5F

    End Sub

    Public ReadOnly Property Name As String _
        Implements IEnviroInputData.Name
        Get
            Return Me.m_source.Name
        End Get
    End Property

#End Region

    Public Property IsDriverActive As Boolean = True _
        Implements IEnviroInputData.IsDriverActive

#Region "Overloaded Methods not implemented by cEcosimEnviroInputData"


    Friend Function Init(ByVal EnviroMediationData As cMediationDataStructures, ByVal SpaceData As cEcospaceDataStructures) As Boolean _
        Implements IEnviroInputData.Init


        Debug.Assert(False, Me.ToString + ".Init(cMediationDataStructures,cEcospaceDataStructures) not implemented by " + Me.ToString)

    End Function


    ''' <summary>
    ''' Return a value for a cell in the input map base on the the response function for a group.
    ''' </summary>
    ''' <param name="igrp">Group index for the response function</param>
    ''' <param name="iMapRow">Row of the input map</param>
    ''' <param name="iMapCol">Col of the input map</param>
    ''' <returns>Y = F(x)</returns>
    Public Function ResponseFunction(ByVal igrp As Integer, ByVal iMapRow As Integer, ByVal iMapCol As Integer) As Single _
        Implements IEnviroInputData.ResponseFunction

        Debug.Assert(False, Me.ToString + ".ResponseFunction(igrp,irow,icol) not implemented for " + Me.ToString)


    End Function




#End Region

End Class

