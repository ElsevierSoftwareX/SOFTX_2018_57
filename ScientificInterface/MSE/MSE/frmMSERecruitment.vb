﻿' ===============================================================================
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
Imports System.Windows.Forms
Imports EwECore
Imports EwECore.MSE
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph
Imports SourceGrid2

#End Region ' Imports

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form, implementing the Ecosim Recruitment interface.
    ''' </summary>
    ''' =======================================================================
    Public Class frmMSERecruitment

#Region " Internals "

        ''' <summary><see cref="cZedGraphHelper">Helper</see> to manipulate the graph.</summary>
        Private m_zgh As cZedGraphHelper = Nothing
        ''' <summary>Group selected in the form.</summary>
        Private m_group As cMSEGroupInput = Nothing

        Private Structure sGraphData

            Public MaxRecruitment As Single
            Public HalfRecruitmentBiomass As Single
            Public EcopathBiomass As Single
            Public Biomass() As Single
            Public Recruitment() As Single
            Public NumSteps As Integer

            Public Sub New(ByVal iStep As Integer)
                Me.NumSteps = iStep
                ReDim Me.Biomass(iStep)
                ReDim Me.Recruitment(iStep)
            End Sub

        End Structure

#End Region ' Internals

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            Dim group As cMSEGroupInput = Nothing

            If Me.UIContext Is Nothing Then Return

            Me.m_zgh = New cZedGraphHelper()
            Me.m_zgh.Attach(Me.UIContext, Me.m_graph)
            Me.m_zgh.ConfigurePane("", SharedResources.HEADER_BIOMASS, SharedResources.HEADER_RECRUITMENT, True)

            Me.m_zgh.AllowZoom = False
            Me.m_zgh.AllowPan = False
            Me.m_zgh.AllowEdit = True
            Me.m_zgh.ShowPointValue = True

            Me.m_grid.UIContext = Me.UIContext

            ' Select first group with likely values
            For iGroup As Integer = 1 To Core.nGroups
                ' Get group
                group = Me.Core.MSEManager.GroupInputs(iGroup)
                ' Has forcastgain value?
                If ((group.GetStatus(eVarNameFlags.MSEForcastGain) And eStatusFlags.Null) = 0) Then
                    ' #Yep: select group in grid (which will update this group too)
                    Me.m_grid.Group = group
                    ' Bail out
                    Exit For
                End If
            Next

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            If Me.m_zgh IsNot Nothing Then
                Me.m_zgh.Detach()
                Me.m_zgh = Nothing
            End If
            Me.Group = Nothing
            MyBase.OnFormClosed(e)

        End Sub

        Private Sub HandleGridSelectionChanged() _
            Handles m_grid.OnSelectionChanged
            ' Update group selection according to user actions in the grid
            Me.Group = Me.m_grid.Group
        End Sub

        Private Sub tsbtDefaults_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles tsbtDefaults.Click
            Try
                Me.Core.SetDefaultMSERecruitment()
            Catch ex As Exception
                ' Yippee
            End Try
        End Sub

        Private Sub HandlePropertyChanged(ByVal prop As cProperty, ByVal cf As cProperty.eChangeFlags)
            ' A relevant property has changed: redraw the graph
            Me.Redraw()
        End Sub

#End Region ' Events

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the group to select in the form.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Property Group() As cMSEGroupInput
            Get
                Return Me.m_group
            End Get
            Set(ByVal value As cMSEGroupInput)

                Dim pm As cPropertyManager = Me.PropertyManager

                ' Unregister
                If (Me.m_group IsNot Nothing) Then
                    RemoveHandler pm.GetProperty(Me.m_group, eVarNameFlags.RHalfB0Ratio).PropertyChanged, AddressOf HandlePropertyChanged
                    RemoveHandler pm.GetProperty(Me.m_group, eVarNameFlags.MSEForcastGain).PropertyChanged, AddressOf HandlePropertyChanged
                End If

                ' Update
                Me.m_group = value

                ' Register
                If (Me.m_group IsNot Nothing) Then
                    AddHandler pm.GetProperty(Me.m_group, eVarNameFlags.RHalfB0Ratio).PropertyChanged, AddressOf HandlePropertyChanged
                    AddHandler pm.GetProperty(Me.m_group, eVarNameFlags.MSEForcastGain).PropertyChanged, AddressOf HandlePropertyChanged
                End If

                ' Redraw the graph
                Me.Redraw()

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the data to render in the graph.
        ''' </summary>
        ''' <returns>A <see cref="sGraphData">graph data</see> instance with data
        ''' to render in the graph.</returns>
        ''' -------------------------------------------------------------------
        Private Function GetGraphValues() As sGraphData

            Dim data As sGraphData = Nothing

            If (Me.Group Is Nothing) Then
                data = New sGraphData(0)
                Return data
            Else  'a group has been selected

                Dim BiomassStep As Single
                Dim iGrp As Integer = Me.Group.Index

                'the 100 steps for the graphs:
                data = New sGraphData(100)

                'Calculate the recruitment as a vector with x-values.
                'the Ecopath biomass is the reference biomass:
                data.EcopathBiomass = Me.Core.EcoPathGroupOutputs(iGrp).Biomass
                'Now we can calculate the biomass where the recruitment is half of the maximum
                'with a default of 0.2 it means that the half of max recruitments is at 20% of the Ecopath biomass
                data.HalfRecruitmentBiomass = data.EcopathBiomass * Me.Group.RHalfB0Ratio

                Dim EcopathRecruitment As Single = data.EcopathBiomass * Me.Group.ForcastGain   'the ForcastGain is the Ecopath recruitment

                'Let's just scale the x-axis to default 10 times the HalfRecruitmentBiomass
                Dim maxXaxisValue As Single = 10
                If CSng(1.1 / Group.RHalfB0Ratio) > maxXaxisValue Then
                    maxXaxisValue = CSng(1.2 / Group.RHalfB0Ratio)  '1.2 is just to give some extra space on the x axis
                End If


                'the max recruitment = RecEcop*(Ratio+1)
                Dim maxYaxisValue As Single = EcopathRecruitment * (Me.Group.RHalfB0Ratio + 1)

                'Dim Recruitment(iStep) As Single
                data.MaxRecruitment = EcopathRecruitment * (Me.Group.RHalfB0Ratio + 1)    'RecEcop*(Ratio+1)
                Dim Rmax As Single = 1

                'plot the (0) array value 
                data.Recruitment(0) = 0

                'the max x value is 
                For i As Integer = 1 To data.NumSteps
                    BiomassStep = CSng(i / 100 * maxXaxisValue * data.HalfRecruitmentBiomass)
                    'Recruitment is calculated as:  R = R max * No  / (Bh + No)
                    data.Recruitment(i) = data.MaxRecruitment * BiomassStep / (data.HalfRecruitmentBiomass + BiomassStep)
                    data.Biomass(i) = BiomassStep
                    'Rec=(Rmax*C2)/(Ratio*Be+C2)
                Next


                Return data
            End If
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Redraw the recruitment curve.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub Redraw()

            If Me.m_zgh Is Nothing Then Return

            Dim lpts As New PointPairList
            Dim lLines As New List(Of LineItem)
            Dim data As sGraphData = Me.GetGraphValues()

            If (Me.Group IsNot Nothing) Then
                ' Group has data?
                If (Me.Group.GetStatus(eVarNameFlags.MSEForcastGain) And eStatusFlags.Null) = 0 Then
                    ' #Yes: plot data
                    For i As Integer = 0 To data.NumSteps - 1
                        lpts.Add(data.Biomass(i), data.Recruitment(i))
                    Next
                    lLines.Add(Me.m_zgh.CreateLineItem(Me.Group.Name, eSketchDrawModeTypes.Line, Color.DarkSlateGray, lpts))
                End If
            End If

            ' Did any lines get manufactured?
            If lLines.Count > 0 Then

                '#Yes: plot graph
                ''  - fix graph scale

                ' JS: this should not be necessary
                Me.m_zgh.YScaleMax = data.MaxRecruitment * (1 + Me.m_zgh.YScaleGrace)
                Me.m_zgh.XScaleMax = data.Biomass(data.NumSteps - 1)

                'now we need some lines:
                '  - place a horizontal, stippled?, grey line at: maxRecruitment 
                lpts = New PointPairList()
                lpts.Add(0.0!, data.MaxRecruitment) : lpts.Add(Me.m_zgh.XScaleMax, data.MaxRecruitment)
                Dim li As LineItem = Me.m_zgh.CreateLineItem(SharedResources.HEADER_MAX_RECRUITMENT,
                                                             eSketchDrawModeTypes.NotSet, Color.DarkGray, lpts)
                li.Line.Style = Drawing2D.DashStyle.DashDot
                lLines.Add(li)

                '  - place a horizontal, stippled?, grey line at: maxRecruitment / 2
                lpts = New PointPairList()
                lpts.Add(0.0!, data.MaxRecruitment / 2) : lpts.Add(data.HalfRecruitmentBiomass, data.MaxRecruitment / 2)
                li = Me.m_zgh.CreateLineItem(SharedResources.HEADER_HALF_MAX_RECRUITMENT,
                                             eSketchDrawModeTypes.NotSet, Color.DarkGray, lpts)
                li.Line.Style = Drawing2D.DashStyle.Dot
                lLines.Add(li)

                '  - place a vertical,   stippled?, grey line at: HalfRecruitment biomass
                lpts = New PointPairList()
                lpts.Add(data.HalfRecruitmentBiomass, 0.0) : lpts.Add(data.HalfRecruitmentBiomass, data.MaxRecruitment / 2)
                li = Me.m_zgh.CreateLineItem(SharedResources.HEADER_HALF_RECRUITMENT_B,
                                             eSketchDrawModeTypes.NotSet, Color.LightSalmon, lpts)
                lLines.Add(li)

                '  - place a vertical,   full, red line at: EcopathBiomass
                lpts = New PointPairList()
                lpts.Add(data.EcopathBiomass, 0.0!) : lpts.Add(data.EcopathBiomass, Me.m_zgh.YScaleMax)
                li = Me.m_zgh.CreateLineItem(SharedResources.HEADER_BIOMASS_ECOPATH,
                                             eSketchDrawModeTypes.NotSet, Color.LightSalmon, lpts)
                li.Line.Style = Drawing2D.DashStyle.Dash
                lLines.Add(li)

                ' - place the dot
                lpts = New PointPairList()
                lpts.Add(data.HalfRecruitmentBiomass, data.MaxRecruitment / 2)
                li = Me.m_zgh.CreateLineItem("", eSketchDrawModeTypes.NotSet, Color.LightSalmon, lpts)
                li.Symbol.Type = SymbolType.Circle
                li.Line.IsVisible = False
                lLines.Add(li)

                ' place lines
                Me.m_zgh.PlotLines(lLines.ToArray)
            Else
                ' Clear graph
                Me.m_zgh.PlotLines(Nothing)
            End If

        End Sub

#End Region ' Internals

    End Class

End Namespace
