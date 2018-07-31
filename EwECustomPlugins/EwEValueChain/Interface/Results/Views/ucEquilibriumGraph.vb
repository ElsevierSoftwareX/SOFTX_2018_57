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
Imports ScientificInterfaceShared.Controls
Imports ZedGraph

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' 
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class ucEquilibriumGraph
    Inherits ZedGraphControl
    Implements IResultView
    Implements IGraphView

    Private m_zgh As cZedGraphHelper = Nothing
    Private m_aVars() As cResults.eVariableType = Nothing

    Public Sub New(ByVal uic As cUIContext)
        Me.m_zgh = New cZedGraphHelper()
        Me.m_zgh.Attach(uic, Me)
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
        Dim line As LineItem = Nothing
        Dim lLines As New List(Of LineItem)
        Dim aobjSnapshots As Object() = result.Snapshots()
        Dim sEffort As Single = 0.0!
        Dim sMin As Single = 0.0!
        Dim sMax As Single = 1.0!

        For Each var As cResults.eVariableType In Me.m_aVars

            line = New LineItem(var.ToString())
            line.Color = cr.NextColor()
            line.Symbol = New ZedGraph.Symbol(SymbolType.None, line.Color)

            For iSnapshot As Integer = 0 To aobjSnapshots.Length - 1
                sEffort = CSng(aobjSnapshots(iSnapshot))
                If iSnapshot = 0 Then
                    sMin = sEffort : sMax = sEffort
                Else
                    sMin = Math.Min(sMin, sEffort)
                    sMax = Math.Max(sMax, sEffort)
                End If
                line.AddPoint(CDbl(sEffort), result.GetSnapshotTotal(var, sEffort, lUnits))
            Next

            lLines.Add(line)

        Next var

        ' Fix scale
        Me.MasterPane.PaneList(0).XAxis.Scale.Min = sMin
        Me.MasterPane.PaneList(0).XAxis.Scale.Max = sMax

        Me.m_zgh.PlotLines(lLines.ToArray)

    End Sub

    Public Sub SetData(ByVal strGraphTitle As String, ByVal strXAxisLabel As String,
                       ByVal strYAxisLabel As String, ByVal aVars() As cResults.eVariableType) Implements IGraphView.SetData

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

        Me.m_zgh.ShowPointValue = True
        Me.m_zgh.AutoscalePane = True

    End Sub

#End Region ' Internals

End Class
