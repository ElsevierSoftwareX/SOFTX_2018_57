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
Option Explicit On

Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Properties
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Ecopath.Output

    ''' =======================================================================
    ''' <summary>
    ''' Implementation of the EwE5 niche pred/prey overlap plot.
    ''' </summary>
    ''' =======================================================================
    Public Class frmNichePredPreyPlot

#Region " Private vars "

        ''' <summary>
        ''' Emerated type for controlling how users want to see the graph coloured.
        ''' </summary>
        Private Enum eColourType As Integer
            ''' <summary>Do not use colours.</summary>
            None
            ''' <summary>Colour all graph dots by predator.</summary>
            ByPredator
            ''' <summary>Colour all graph dots by prey.</summary>
            ByPrey
            ''' <summary>Colour all graph dots by overlap ratio.</summary>
            ByOverlap
        End Enum

        ''' <summary>Handler for styling and controlling the graph.</summary>
        Private m_zgh As cZedGraphHelper = Nothing

        ''' <summary>User-provided cut-off value for filtering content [0, 1].</summary>
        Private m_sCutOff As Single = 0.1!
        ''' <summary>Format provider for making cut-off UI control look good.</summary>
        Private m_fpm_sCutOff As cEwEFormatProvider = Nothing

        ''' <summary>User-provided <see cref="eColourType">colour</see> behaviour.</summary>
        Private m_colourType As eColourType = eColourType.ByPredator
        ''' <summary>Colour ramp for rendering a <see cref="eColourType.ByOverlap"/> graph.</summary>
        Private m_crColor As New cARGBColorRamp(New Color() {Color.White, Color.Gray, Color.Black}, New Double() {0, 0.6, 0.4})

        ''' <summary>Flag stating whether value labels should be shown on the plot.</summary>
        Private m_bShowLabels As Boolean = True

#End Region ' Private vars

#Region " Form overrides "

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Form is about to appear; hook up to all dynamic bits, ect.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If Me.UIContext Is Nothing Then Return

            Me.m_zgh = New cZedGraphHelper()
            Me.m_zgh.Attach(Me.UIContext, Me.m_graph)
            Me.m_zgh.ShowPointValue = True

            Me.m_fpm_sCutOff = New cEwEFormatProvider(Me.UIContext, Me.m_nudCutOff, GetType(Single))
            Me.m_fpm_sCutOff.Value = Me.m_sCutOff
            Me.m_nudCutOff.Maximum = CDec(1)
            Me.m_nudCutOff.Minimum = CDec(0.0)
            Me.m_nudCutOff.Increment = CDec(0.1)

            Dim cmd As cCommand = Me.CommandHandler.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            If (cmd IsNot Nothing) Then
                cmd.AddControl(Me.m_btnShowHideGroups)
            End If

            Me.CoreExecutionState = eCoreExecutionState.EcopathCompleted

            Me.UpdateControls()
            Me.UpdatePlot()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Form is about to close; clean up. Detach from all dynamic bits, etc.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            If Me.UIContext Is Nothing Then Return

            Dim cmd As cCommand = Me.CommandHandler.GetCommand(cDisplayGroupsCommand.cCOMMAND_NAME)
            If (cmd IsNot Nothing) Then
                cmd.RemoveControl(Me.m_btnShowHideGroups)
            End If

            Me.m_fpm_sCutOff.Release()
            Me.m_fpm_sCutOff = Nothing

            Me.m_zgh.Detach()
            Me.m_zgh = Nothing

            MyBase.OnFormClosed(e)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="ScientificInterfaceShared.Forms.frmEwE.UpdateControls"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub UpdateControls()
            MyBase.UpdateControls()
            Select Case Me.m_colourType
                Case eColourType.None : Me.m_rbNone.Checked = True
                Case eColourType.ByPredator : Me.m_rbPredator.Checked = True
                Case eColourType.ByPrey : Me.m_rbPrey.Checked = True
                Case eColourType.ByOverlap : Me.m_rbOverlap.Checked = True
            End Select
            Me.m_cbLabels.Checked = Me.m_bShowLabels
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="ScientificInterfaceShared.Forms.frmEwE.OnStyleGuideChanged"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnStyleGuideChanged(ByVal ct As ScientificInterfaceShared.Style.cStyleGuide.eChangeType)
            MyBase.OnStyleGuideChanged(ct)
            ' Update graph whenever any style guide aspect has changed. Could have 
            ' anything really: graph bits, fonts, colours, group visiblity... Yes,
            ' those eventualities could be tested for.
            Me.UpdatePlot()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="ScientificInterfaceShared.Forms.frmEwE.OnStyleGuideChanged"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Function GetPrintContent(rc As System.Drawing.Rectangle) As System.Drawing.Image
            Return Me.m_graph.GetImage()
        End Function

#End Region ' Form overrides

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the content of the plot.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdatePlot()

            Dim prey As cEcoPathGroupOutput = Nothing
            Dim pred As cEcoPathGroupOutput = Nothing
            Dim ppl As PointPairList = Nothing
            Dim pane As GraphPane = Nothing
            Dim li As LineItem = Nothing
            Dim label As ZedGraph.TextObj = Nothing

            ' Configure graph pane
            pane = Me.m_zgh.ConfigurePane(My.Resources.CAPTION_NICHEPLOT_HEADER, My.Resources.CAPTION_NICHEPLOT_XAXIS_LABEL, My.Resources.CAPTION_NICHEPLOT_YAXIS_LABEL, False)
            pane.XAxis.Scale.Max = 1.1!
            pane.YAxis.Scale.Max = 1.1!

            ' Clear any existing curves
            pane.CurveList.Clear()
            ' Clear any exiting text objects
            pane.GraphObjList.Clear()

            ' For all predators
            For iPred As Integer = 1 To Me.Core.nGroups
                ' Get predator
                pred = Me.Core.EcoPathGroupOutputs(iPred)
                ' For all prey
                For iPrey As Integer = 1 To Me.Core.nGroups
                    ' Get prey
                    prey = Me.Core.EcoPathGroupOutputs(iPrey)
                    ' Are both predator and prey visible?
                    If Me.StyleGuide.GroupVisible(iPrey) And Me.StyleGuide.GroupVisible(iPred) Then
                        ' #Yes: Is there overlap, and did we not already process this pred/prey combination?
                        '       (n.b. Hlap and Plap are populated bi-directionally)
                        ' JS: test set to EXCEED the cut-off (>) rather than include cut-off (>=) to avoid 0 cut-off values.
                        If (prey.Hlap(iPred) > Me.m_sCutOff) And _
                           (prey.Plap(iPred) > Me.m_sCutOff) And _
                           (iPrey > iPred) Then

                            ' #Yes: create a new line item for this pred/prey combo
                            ppl = New PointPairList()
                            ppl.Add(prey.Hlap(iPred), prey.Plap(iPred))

                            ' Style line
                            li = New LineItem(String.Format(SharedResources.GENERIC_LABEL_INDEXED, pred.Name, prey.Name), ppl, Color.Black, SymbolType.Circle)
                            li.Line.Color = Color.Transparent
                            ' ToDo: make dot size configurable
                            li.Symbol.Size = CSng(10)

                            Select Case Me.m_colourType
                                Case eColourType.None
                                    li.Symbol.Fill.IsVisible = False
                                Case eColourType.ByPredator
                                    li.Symbol.Fill = New Fill(Me.StyleGuide.GroupColor(Me.Core, pred.Index))
                                Case eColourType.ByPrey
                                    li.Symbol.Fill = New Fill(Me.StyleGuide.GroupColor(Me.Core, prey.Index))
                                Case eColourType.ByOverlap
                                    li.Symbol.Fill = New Fill(Me.m_crColor.GetColor(prey.Hlap(iPred) + prey.Plap(iPred) / 2, 1.0))
                            End Select

                            ' Need to show labels?
                            If (Me.m_bShowLabels) Then
                                ' #Yes: create text label
                                label = New TextObj(String.Format("{0}, {1}", pred.Index, prey.Index), _
                                                    prey.Hlap(iPred), prey.Plap(iPred), CoordType.AxisXYScale, AlignH.Left, AlignV.Top)
                                ' Style text label
                                label.FontSpec.Border.IsVisible = False
                                label.FontSpec.Fill.IsVisible = False
                                ' Add text label
                                pane.GraphObjList.Add(label)
                            End If

                            ' Add line last
                            pane.CurveList.Add(li)

                        End If
                    End If
                Next
            Next

            ' Done; rescale graph entirely
            Me.m_zgh.RescaleAndRedraw()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler to trap user colour behaviour changes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnColourOptionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_rbPredator.CheckedChanged, _
                    m_rbPrey.CheckedChanged, _
                    m_rbOverlap.CheckedChanged, _
                    m_rbNone.CheckedChanged

            If (Me.UIContext Is Nothing) Then Return

            ' Determine colour type from UI control settings
            If Me.m_rbNone.Checked Then
                Me.m_colourType = eColourType.None
            ElseIf Me.m_rbPredator.Checked Then
                Me.m_colourType = eColourType.ByPredator
            ElseIf Me.m_rbPrey.Checked Then
                Me.m_colourType = eColourType.ByPrey
            Else
                Me.m_colourType = eColourType.ByOverlap
            End If
            ' Refresh plot
            Me.UpdatePlot()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler to trap user cut-off value changes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnCutOffValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_nudCutOff.ValueChanged
            If (Me.UIContext Is Nothing) Then Return
            Me.m_sCutOff = CSng(Me.m_nudCutOff.Value)
            Me.UpdatePlot()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler to trap user label display changes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnShowLabelsChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
           Handles m_cbLabels.CheckedChanged
            Me.m_bShowLabels = Me.m_cbLabels.Checked
            Me.UpdatePlot()
        End Sub

#End Region ' Internals

    End Class

End Namespace
