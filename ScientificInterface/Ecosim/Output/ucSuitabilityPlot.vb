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

Option Explicit On
Option Strict On

Imports EwECore
Imports EwEUtils.Utilities
Imports ZedGraph

#End Region ' Imports

Public Class ucSuitabilityPlot
    Implements IUIElement

#Region " Private variables "

    Private m_uic As cUIContext = Nothing
    Private m_zgh As cZedGraphHelper = Nothing
    Private m_plottype As ePlotTypes = ePlotTypes.Electivity

    Private Enum ePlotTypes As Integer
        Electivity = 0
        Suitability
        FunctionalResponse
    End Enum

#End Region ' Private variables

#Region " Constructor "

    Public Sub New()
        Me.InitializeComponent()
    End Sub

#End Region ' Constructor

#Region " IUIElement implementation "

    Public Property UIContext() As cUIContext _
        Implements IUIElement.UIContext
        Get
            Return Me.m_uic
        End Get
        Set(ByVal value As cUIContext)

            If Me.m_uic IsNot Nothing Then
                RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                If Me.m_zgh IsNot Nothing Then
                    Me.m_zgh.Detach()
                    Me.m_zgh = Nothing
                End If
                Me.m_lbGroups.Detach()
            End If

            Me.m_uic = value

            If Me.m_uic IsNot Nothing Then
                AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                Me.m_zgh = New cZedGraphHelper()
                Me.m_zgh.Attach(Me.UIContext, Me.m_graph)

                Me.m_lbGroups.Attach(Me.m_uic)

                Me.m_rbElectivity.Checked = True

                Me.UpdatePredatorList()
                Me.UpdateGraph()
            End If

        End Set
    End Property

#End Region ' IUIElement implementation

#Region " Events "

    Private Sub OnStyleGuideChanged(ByVal changeType As cStyleGuide.eChangeType)
        Try
            Me.UpdateGraph()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub OnPlotSuitability(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbSuitability.CheckedChanged
        Try
            Me.PlotType = ePlotTypes.Suitability
            Me.UpdateGraph()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub OnPlotFunctionalResponse(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbFunctionalResponse.CheckedChanged
        Try
            Me.PlotType = ePlotTypes.FunctionalResponse
            Me.UpdateGraph()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub OnPlotElectivity(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_rbElectivity.CheckedChanged
        Try
            Me.PlotType = ePlotTypes.Electivity
            Me.UpdateGraph()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub OnSelectPredator(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_lbGroups.SelectedIndexChanged
        Try
            Me.UpdateGraph()
        Catch ex As Exception
        End Try
    End Sub

#End Region ' Events

#Region " Internals "

    Private Property PlotType() As ePlotTypes
        Get
            Return Me.m_plottype
        End Get
        Set(ByVal value As ePlotTypes)
            Me.m_plottype = value
        End Set
    End Property

    Private Function SelectedPredator() As cEcoPathGroupInput
        Return Me.m_lbGroups.SelectedGroup
    End Function

    Private Sub UpdatePredatorList()

        Dim group As cEcoPathGroupInput = Nothing
        Dim liGroups As New List(Of Integer)

        Me.m_lbGroups.Items.Clear()
        For iGroup As Integer = 1 To Me.UIContext.Core.nGroups
            group = Me.UIContext.Core.EcoPathGroupInputs(iGroup)
            If group.IsConsumer Then liGroups.Add(group.Index)
        Next
        Me.m_lbGroups.Populate(liGroups.ToArray())
        Me.m_lbGroups.SelectedIndex = 0

    End Sub

    Private Sub UpdateGraph()

        Dim predIn As cEcoPathGroupInput = Me.SelectedPredator()
        Dim predOut As cEcosimGroupOutput = Nothing
        Dim preyEcopath As cEcoPathGroupOutput = Nothing
        Dim preyOut As cEcosimGroupOutput = Nothing
        Dim asX(Me.UIContext.Core.nEcosimTimeSteps - 1) As Double
        Dim asY(Me.UIContext.Core.nEcosimTimeSteps - 1) As Double
        Dim Xmax As Double
        Dim Ymax As Double
        Dim strTitle As String = ""
        Dim strXAxisTitle As String = ""
        Dim strYAxisTitle As String = ""
        Dim gp As GraphPane = Me.m_graph.GraphPane

        ' Clear
        gp.Title.Text = ""
        gp.CurveList.Clear()

        If (predIn IsNot Nothing) Then

            Select Case Me.m_plottype

                Case ePlotTypes.Electivity
                    strTitle = My.Resources.ECOSIM_SUITABILITY_PLOT_ELECTIVITY & cStringUtils.vbCrLf & _
                               My.Resources.ECOSIM_SUITABILITY_PLOT_PRED & predIn.Name
                    strXAxisTitle = ""
                    strYAxisTitle = My.Resources.ECOSIM_SUITABILITY_PLOT_CHESSON_ELECTIVITY

                Case ePlotTypes.FunctionalResponse
                    strTitle = My.Resources.ECOSIM_SUITABILITY_PLOT_FUNCT_RESP & cStringUtils.vbCrLf & _
                               My.Resources.ECOSIM_SUITABILITY_PLOT_PRED & predIn.Name
                    strXAxisTitle = My.Resources.ECOSIM_SUITABILITY_PLOT_PREYBIOM_ECOPATHBIOM
                    strYAxisTitle = My.Resources.ECOSIM_SUITABILITY_PLOT_QPREY_BPRED

                Case ePlotTypes.Suitability
                    strTitle = My.Resources.ECOSIM_SUITABILITY_PLOT_SUITABILITY & cStringUtils.vbCrLf & _
                               My.Resources.ECOSIM_SUITABILITY_PLOT_PRED & predIn.Name
                    strXAxisTitle = My.Resources.ECOSIM_SUITABILITY_PLOT_PREYBIOM_ECOPATHBIOM
                    strYAxisTitle = My.Resources.ECOSIM_SUITABILITY_PLOT_SUITABILITY

            End Select

            Me.m_zgh.ConfigurePane(strTitle, strXAxisTitle, strYAxisTitle, False)

            For iPrey As Integer = 1 To Me.UIContext.Core.nGroups
                preyEcopath = Me.UIContext.Core.EcoPathGroupOutputs(iPrey)
                predOut = Me.UIContext.Core.EcoSimGroupOutputs(predIn.Index)
                preyOut = Me.UIContext.Core.EcoSimGroupOutputs(iPrey)

                If (predIn.DietComp(iPrey) > 0.0!) And (preyEcopath.Biomass > 0.0) Then

                    Array.Clear(asX, 0, asX.Length)
                    Array.Clear(asY, 0, asY.Length)

                    For iTime As Integer = 1 To Me.UIContext.Core.nEcosimTimeSteps

                        Select Case Me.m_plottype

                            Case ePlotTypes.Electivity
                                asX(iTime - 1) = iTime
                                asY(iTime - 1) = predOut.Electivity(preyOut.Index, iTime)

                            Case ePlotTypes.FunctionalResponse
                                'x = Bprey(T)/Bprey(0)
                                'y = Qpred(prey,t)/Bpred(t)
                                asX(iTime - 1) = preyOut.Biomass(iTime) / preyEcopath.Biomass
                                asY(iTime - 1) = preyOut.Consumption(predIn.Index, iTime) / predOut.Biomass(iTime)

                            Case ePlotTypes.Suitability
                                'x = Bprey(T)/Bprey(0)
                                'y = Elect(pred,prey)
                                asX(iTime - 1) = preyOut.Biomass(iTime) / preyEcopath.Biomass
                                asY(iTime - 1) = predOut.Electivity(preyOut.Index, iTime)

                        End Select

                        Xmax = Math.Max(asX(iTime - 1), Xmax)
                        Ymax = Math.Max(asY(iTime - 1), Ymax)
                    Next

                    ' Add curve
                    gp.AddCurve(preyEcopath.Name, _
                                asX, asY, _
                                Me.m_uic.StyleGuide.GroupColor(Me.UIContext.Core, iPrey), _
                                ZedGraph.SymbolType.None)
                End If
            Next

            ' Fix axis scales
            gp.XAxis.Scale.Max = Xmax

            ' JS: this should not be necessary
            gp.YAxis.Scale.Max = Ymax * Me.m_zgh.YScaleGrace

        End If

        Me.m_graph.AxisChange()
        Me.m_graph.Invalidate()

    End Sub

#End Region ' Internals

End Class
