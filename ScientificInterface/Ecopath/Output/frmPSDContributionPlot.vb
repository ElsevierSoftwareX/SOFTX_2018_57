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
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph

#End Region ' Imports

Namespace Ecopath.Output

    Public Class PSDContributionPlot

#Region " Variables "

        Private m_zgh As cZedGraphHelper = Nothing

#End Region ' Variables

#Region " Constructor "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#End Region ' Constructor

#Region " Event handlers "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            Debug.Assert(Me.UIContext IsNot Nothing)

            Me.m_zgh = New cZedGraphHelper()
            Me.m_zgh.Attach(Me.UIContext, Me.m_graph)

            Me.m_lbGroups.Attach(Me.UIContext)
            Me.m_lbGroups.SelectedIndex = 0

        End Sub

        Private Sub llbGroups_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_lbGroups.SelectedIndexChanged
            AddCurves(CreatePane(My.Resources.CAPTION_PSD_GROUP_CONTRIB, SharedResources.HEADER_BODYWEIGHT_LOGg, _
                     SharedResources.HEADER_BIOMASS_LOGg))

            'highlight group contribution in the histogram
            UpdatePlot()
        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
            Me.m_zgh.Detach()
            Me.m_zgh = Nothing
            Me.m_lbGroups.Detach()
            MyBase.OnFormClosed(e)
        End Sub

#End Region ' Event handlers

#Region " Helper methods "

        Private Function CreatePane(ByVal strTitle As String, ByVal strXAxisTitle As String, _
                                    ByVal strYAxisTitle As String) As GraphPane
            Dim pane As GraphPane = Me.m_graph.GraphPane
            InitGraphPane(strTitle, strXAxisTitle, strYAxisTitle)
            Return pane
        End Function

        Private Sub InitGraphPane(ByVal strTitle As String, ByVal strXAxisTitle As String, ByVal strYAxisTitle As String)

            Dim psd As cPSDParameters = Me.Core.ParticleSizeDistributionParameters
            Dim gp As GraphPane = Me.m_zgh.ConfigurePane(strTitle, strXAxisTitle, strYAxisTitle, False)

            'gp.XAxis.Scale.Min = Int(Math.Log10(psd.FirstWeightClass))
            'gp.XAxis.Scale.Max = Math.Round(Math.Log10(psd.FirstWeightClass * 2 ^ (Me.Core.nWeightClasses - 1)) + 0.4, 0, MidpointRounding.AwayFromZero)
            'gp.YAxis.Scale.Min = 0

        End Sub

        Private Sub AddCurves(ByVal pane As GraphPane)

            Dim psd As cPSDParameters = Me.Core.ParticleSizeDistributionParameters
            Dim resultLists As New List(Of PointPairList)
            Dim sXValue As Single = 0
            Dim dPlotX, dPlotY As Double
            Dim sXMax, sYMax As Single
            Dim sXMin, sYMin As Single
            Dim group As cEcoPathGroupOutput = Nothing
            Dim sSystemPSD(Me.Core.nWeightClasses) As Single
            Dim curve As BarItem = Nothing
            Dim fmt As New cCoreInterfaceFormatter()
            Dim iNumSelected As Integer = Me.m_lbGroups.SelectedIndices.Count

            InitLists(resultLists, Me.Core.nLivingGroups) '3)

            'Find the system PSD by summing the group PSD
            FindSystemPSD(sSystemPSD)

            For igroup As Integer = 1 To Me.Core.nLivingGroups
                'No need to check if group is selected. Generate the result list even for the not selected group. It will have zero Y values
                'If IsGroupSelected(igroup) Then
                group = Me.Core.EcoPathGroupOutputs(igroup)
                For iWtClass As Integer = 1 To Me.Core.nWeightClasses
                    ' Calc X
                    sXValue = CSng(psd.FirstWeightClass * 2 ^ (iWtClass - 1))
                    ' Convert to plot values
                    dPlotX = Math.Log10(sXValue)
                    dPlotY = 0
                    If sSystemPSD(iWtClass) * 1000000000 > 0 Then
                        'group contribution to the system PSD is Math.Log10(sSystemPSD(iWtClass) * 1000000000) * grpOutput.PSD(iWtClass) / sSystemPSD(iWtClass)
                        '* 1000000000 for plotting purpose
                        dPlotY = Math.Log10(sSystemPSD(iWtClass) * 1000000000) * group.PSD(iWtClass) / sSystemPSD(iWtClass)
                    End If

                    ' Add point
                    resultLists(igroup - 1).Add(New PointPair(dPlotX, dPlotY))

                    ' Keep track of scale range
                    sXMin = CSng(Math.Min(sXMin, dPlotX))
                    sXMax = CSng(Math.Max(sXMax, dPlotX))
                    sYMin = CSng(Math.Min(sYMin, dPlotY))
                    sYMax = CSng(Math.Max(sYMax, dPlotY))
                Next
                'End If
            Next

            ' Clear pane
            pane.CurveList.Clear()

            For iGroup As Integer = 1 To Me.Core.nLivingGroups
                group = Me.Core.EcoPathGroupOutputs(iGroup)

                Dim clrFore As Color = Color.Black
                Dim clrBack As Color = Color.Gray

                If (Me.m_lbGroups.IsAllGroupsItemSelected) Or (Me.m_lbGroups.IsGroupSelected(iGroup)) Then
                    clrFore = Color.DarkGray
                    clrBack = Me.StyleGuide.GroupColor(Me.Core, iGroup)
                End If
                AddCurveToGraphPane(pane, fmt.GetDescriptor(group), resultLists(iGroup - 1), clrBack, clrFore)
            Next

            pane.XAxis.Scale.Min = sXMin * 1.1
            pane.XAxis.Scale.Max = sXMax / 1.1
            pane.YAxis.Scale.Min = 0
            pane.YAxis.Scale.Max = sYMax * 1.2
            pane.BarSettings.Type = BarType.Stack

        End Sub

        Private Sub InitLists(ByRef lists As List(Of PointPairList), ByVal size As Integer)
            ' Init the result lists
            For i As Integer = 1 To size
                Dim list As New PointPairList()
                lists.Add(list)
            Next
        End Sub

        Private Function AddCurveToGraphPane(ByVal pane As GraphPane, ByVal strName As String, ByVal list As PointPairList, _
                                        ByVal clrFill As Color, ByVal clrBorder As Color) As BarItem
            Dim curve As BarItem = Nothing
            curve = pane.AddBar(strName, list, clrFill)
            curve.Bar.Fill = New Fill(clrFill)
            curve.Bar.Border = New Border(clrBorder, 0.2)
            Return curve

        End Function

        Private Sub UpdatePlot()
            Me.m_graph.AxisChange()
            Me.m_graph.Refresh()
        End Sub

        Private Sub FindSystemPSD(ByVal sSystemPSD() As Single)
            Dim grpOutput As cEcoPathGroupOutput = Nothing

            'Find the system PSD by summing the group PSD
            For iGroup As Integer = 1 To Me.Core.nLivingGroups
                If Me.m_lbGroups.GroupIndex(iGroup) > -1 Then
                    grpOutput = Me.Core.EcoPathGroupOutputs(iGroup)
                    For iWtClass As Integer = 1 To Me.Core.nWeightClasses
                        sSystemPSD(iWtClass) = sSystemPSD(iWtClass) + grpOutput.PSD(iWtClass)
                    Next
                End If
            Next
        End Sub

#End Region ' Helper methods

    End Class

End Namespace