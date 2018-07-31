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
Imports EwECore
Imports ScientificInterfaceShared.Controls
Imports ZedGraph

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' 
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class ucEcosimGraph
    Inherits ZedGraphControl
    Implements IResultView
    Implements IGraphView

    Private m_zgh As cZedGraphHelper = Nothing
    Private m_data As cData = Nothing
    Private m_aVars() As cResults.eVariableType = Nothing

    Public Sub New(ByVal data As cData, ByVal uic As cUIContext)
        Me.m_zgh = New cZedGraphHelper()
        Me.m_zgh.Attach(uic, Me)
        Me.m_zgh.ShowPointValue = True
        Me.m_data = data
        Me.PrepareGraph()
    End Sub

    Protected Overrides Sub Finalize()
        Me.m_zgh.Detach()
        Me.m_zgh = Nothing
        MyBase.Finalize()
    End Sub

    Public Sub ShowResults(ByVal iFleet As Integer, ByVal lUnits As cUnit(), ByVal result As cResults, _
                           ByVal iTimeStep As Integer) _
             Implements IResultView.ShowResults

        Dim cr As New ZedGraph.ColorSymbolRotator()
        Dim lLines As New List(Of LineItem)
        Dim line As LineItem = Nothing
        Dim iBaseYear As Integer = 0

        iBaseYear = Me.m_data.Core.EcosimFirstYear

        For Each vn As cResults.eVariableType In Me.m_aVars

            line = New LineItem(vn.ToString())
            line.Color = cr.NextColor()
            line.Symbol = New ZedGraph.Symbol(SymbolType.None, line.Color)

            For iTimeStep = 1 To result.NumTimeSteps
                line.AddPoint(CDbl(iBaseYear + ((iTimeStep - 1) / cCore.N_MONTHS)), _
                              result.GetTimeStepTotal(vn, iTimeStep, lUnits, iFleet, cResults.GetVariableContributionType(vn)))
            Next iTimeStep

            lLines.Add(line)

        Next vn

        ' Fix scale
        If result.NumTimeSteps > 1 Then

            Me.MasterPane.PaneList(0).XAxis.Scale.Min = iBaseYear
            Me.MasterPane.PaneList(0).XAxis.Scale.Max = iBaseYear + (result.NumTimeSteps / cCore.N_MONTHS)

        End If

        Me.m_zgh.PlotLines(lLines.ToArray)

    End Sub

    Public Sub SetData(ByVal strGraphTitle As String,
                       ByVal strXAxisLabel As String,
                       ByVal strYAxisLabel As String,
                       ByVal aVars() As cResults.eVariableType) Implements IGraphView.SetData

        Me.m_zgh.ConfigurePane(strGraphTitle, strXAxisLabel, strYAxisLabel, True)
        Me.m_aVars = aVars

    End Sub

#Region " Internals "

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'ucGraph
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.Name = "ucGraph"
        Me.Size = New System.Drawing.Size(485, 268)
        Me.ResumeLayout(False)

    End Sub

    Private Sub PrepareGraph()

        Me.m_zgh.AutoscalePane() = True

    End Sub

#End Region ' Internals

End Class
