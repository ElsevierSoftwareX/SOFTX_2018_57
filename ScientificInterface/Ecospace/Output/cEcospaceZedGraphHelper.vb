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
Imports System
Imports EwECore
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports ScientificInterface.Other
Imports ScientificInterfaceShared.Style
Imports ZedGraph
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Ecospace

    ''' =======================================================================
    ''' <summary>
    ''' <see cref="cZedGraphHelper">ZedGraph helper</see>-derived class to
    ''' make Ecospace plots look a lot more pretty.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
    Public Class cEcospaceZedGraphHelper
        Inherits cZedGraphHelper

        Private m_nTotalSteps As Integer = 0
        Private m_iFirstYear As Integer = 0
        Private m_sNumStepsPerYear As Single = 0.0!
        Private m_nGroups As Integer = 0
        Private m_pane As GraphPane = Nothing

        Private m_showitemMode As frmRunEcospace.eShowItemType = frmRunEcospace.eShowItemType.ShowAll
        Private m_iItemToShow As Integer = cCore.NULL_VALUE


        Private m_useLogScale As Boolean = True

        Public Overrides Sub Attach(ByVal uic As ScientificInterfaceShared.Controls.cUIContext, ByVal zgc As ZedGraph.ZedGraphControl, Optional ByVal iNumPanels As Integer = 1)
            MyBase.Attach(uic, zgc, iNumPanels)
            For i As Integer = 0 To Me.NumPanes - 1
                AddHandler Me.GetPane(i + 1).XAxis.ScaleFormatEvent, AddressOf XScaleFormatEvent
                AddHandler Me.GetPane(i + 1).YAxis.ScaleFormatEvent, AddressOf YScaleFormatEvent
            Next
        End Sub

        Public Overrides Sub Detach()
            For i As Integer = 0 To Me.NumPanes - 1
                RemoveHandler Me.GetPane(i + 1).XAxis.ScaleFormatEvent, AddressOf XScaleFormatEvent
                RemoveHandler Me.GetPane(i + 1).YAxis.ScaleFormatEvent, AddressOf YScaleFormatEvent
            Next
            MyBase.Detach()
        End Sub

        Public Sub Reset(ByVal strTitle As String, _
                         ByVal strYAxisLabel As String, _
                         ByVal nGroups As Integer, _
                         ByVal nTotalSteps As Integer, _
                         ByVal iFirstYear As Integer, _
                         ByVal sNumStepsPerYear As Single)

            Dim li As LineItem = Nothing
            Dim YMin As Single, YMax As Single
            If Me.m_useLogScale Then
                YMin = -1
                YMax = 1
            End If

            Me.m_pane = Me.ConfigurePane(strTitle, _
                                         ScientificInterfaceShared.My.Resources.HEADER_YEAR, _
                                         0, nTotalSteps, strYAxisLabel, YMin, YMax, False)
            'Auto Scale the Y Axis if not using a log scale
            Me.m_pane.YAxis.Scale.MaxAuto = (Not Me.m_useLogScale)

            Me.m_nGroups = nGroups
            Me.m_nTotalSteps = nTotalSteps
            Me.m_iFirstYear = iFirstYear
            Me.m_sNumStepsPerYear = sNumStepsPerYear

            Me.m_pane.CurveList.Clear()
            For iGroup As Integer = 1 To nGroups
                li = Me.CreateLineItem(Me.Core.EcoPathGroupInputs(iGroup), New PointPairList())
                Me.m_pane.CurveList.Add(li)
            Next

            Me.RescaleAndRedraw(1)

        End Sub

        Public Sub Overlay(ByVal nGroups As Integer)
            'For igroup As Integer = 1 To nGroups
            '    Me.m_agpLines(igroup).StartFigure()
            'Next
        End Sub

        Public Sub AddValue(ByVal iGroup As Integer, ByVal iTimeStep As Integer, ByVal sValue As Single)

            If Not cNumberUtils.IsFinite(sValue) Then
                cNumberUtils.FixValue(sValue)
#If DEBUG Then
                If 2 = 3 Then
                    Debug.Assert(False, "Point contains invalid values")
                End If
#End If
            End If
            Try
                Dim li As CurveItem = Me.m_pane.CurveList(iGroup - 1)
                li.AddPoint(iTimeStep, sValue)
            Catch ex As Exception

            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the group or fleet show mode. Note that this will not refresh the graph;
        ''' the calling process will have to invoke <see cref="UpdateCurveVisibility">UpdateCurveVisibility</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ItemShowMode() As frmRunEcospace.eShowItemType
            Get
                Return Me.m_showitemMode
            End Get
            Set(ByVal value As frmRunEcospace.eShowItemType)
                Me.m_showitemMode = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the group to show. Note that this will not refresh the graph;
        ''' the calling process will have to invoke <see cref="UpdateCurveVisibility">UpdateCurveVisibility</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ItemToShow() As Integer
            Get
                Return Me.m_iItemToShow
            End Get
            Set(ByVal value As Integer)
                Me.m_iItemToShow = value
            End Set
        End Property

        Public Property LogScale() As Boolean
            Get
                Return Me.m_useLogScale
            End Get
            Set(ByVal value As Boolean)
                Me.m_useLogScale = value
            End Set
        End Property


        Protected Overrides Function IsCurveVisible(ByVal ci As ZedGraph.CurveItem) As Boolean

            Dim info As cCurveInfo = Me.CurveInfo(ci)

            Select Case Me.ItemShowMode
                Case frmRunEcospace.eShowItemType.ShowAll
                    Return True
                Case frmRunEcospace.eShowItemType.ShowCustom
                    Return MyBase.IsCurveVisible(ci)
                Case frmRunEcospace.eShowItemType.ShowSingle
                    Return (info.Index = Me.m_iItemToShow)
            End Select

            Return True

        End Function

        Private Function XScaleFormatEvent(ByVal pane As GraphPane, _
                                           ByVal axis As Axis, _
                                           ByVal dValue As Double, _
                                           ByVal iIndex As Integer) As String
            Dim sNumStepsPerYear As Single = Me.m_sNumStepsPerYear
            Dim sYear As Single = 0.0!

            If (sNumStepsPerYear <= 0.0!) Then sNumStepsPerYear = 1.0!
            sYear = Me.m_iFirstYear + CSng(dValue / sNumStepsPerYear)
            Return sYear.ToString

        End Function

        Private Function YScaleFormatEvent(ByVal pane As GraphPane, _
                                           ByVal axis As Axis, _
                                           ByVal dValue As Double, _
                                           ByVal iIndex As Integer) As String
            If Me.LogScale Then
                Return Me.StyleGuide.FormatNumber(Math.Pow(10, dValue))
            Else
                Return Me.StyleGuide.FormatNumber(dValue)
            End If
        End Function

    End Class

End Namespace ' Ecospace
