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

Imports System.Drawing
Imports System.Windows.Forms
Imports EwECore
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports ZedGraph

#End Region ' Imports

<CLSCompliant(False)>
Public Class cIndicesWithoutPPREst
    Inherits cContentManager

    Private m_zgh As cZedGraphHelper = Nothing

    Public Sub New()
        '
    End Sub

    Public Overrides Function PageTitle() As String
        Return "Ecosim indices without PPR estimated"
    End Function

    Public Overrides Function Attach(ByVal manager As cNetworkManager,
                                     ByVal datagrid As DataGridView,
                                     ByVal graph As ZedGraphControl,
                                     ByVal plot As ucPlot,
                                     ByVal toolstrip As ToolStrip,
                                     ByVal uic As cUIContext) As Boolean

        Dim bSucces As Boolean = MyBase.Attach(manager, datagrid, graph, plot, toolstrip, uic)

        Me.NetworkManager.UseEcosimNetwork = True
        Me.NetworkManager.EcosimPPROn = False
        bSucces = bSucces And Me.NetworkManager.RunEcosimNetwork()
        Me.NetworkManager.UseEcosimNetwork = False

        Me.Graph.Visible = bSucces
        Me.Toolstrip.Visible = bSucces
        Me.ToolstripShowOptionCSV()

        Me.m_zgh = New cZedGraphHelper()
        Me.m_zgh.Attach(uic, Me.Graph, 2)
        Me.m_zgh.ShowPointValue = True

        Return bSucces

    End Function

    Public Overrides Sub Detach()

        Me.m_zgh.Detach()
        Me.m_zgh = Nothing

        MyBase.Detach()

    End Sub

    Public Overrides ReadOnly Property IsDataOverTime() As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property UsesEcosim() As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides Sub DisplayData()

        Dim paneMaster As MasterPane = Me.Graph.MasterPane
        Dim pane As GraphPane = Nothing
        Dim g As Graphics = Nothing

        Dim fmt As New cDiversityIndexTypeFormatter()
        Dim model As cEwEModel = Me.UIContext.Core.EwEModel

        'Pane1
        pane = Me.m_zgh.ConfigurePane("", My.Resources.LBL_MONTHS, My.Resources.LBL_NA_INDIC, True, LegendPos.TopCenter, 1)
        'Add curves
        pane.CurveList.Clear()
        'FIB
        AddCurve(My.Resources.LBL_FIB_INDX, Me.NetworkManager.FIB, pane, Color.Green)
        'Relative sum of catch
        AddCurve(My.Resources.LBL_TOTAL_CATCH, Me.NetworkManager.RelativeSumOfCatch, pane, Color.Red)
        'Relative diversity
        AddCurve(fmt.GetDescriptor(model.DiversityIndexType), Me.NetworkManager.RelativeDiversity, pane, Color.Blue)
        'TL catch
        AddCurve(My.Resources.LBL_TL_CATCH, Me.NetworkManager.TLCatch, pane, Color.Black)
        'FCI
        AddCurve(My.Resources.LBL_FCI, Me.NetworkManager.FCIEcosim, pane, Color.Brown)

        'Pane2
        pane = Me.m_zgh.ConfigurePane("", My.Resources.LBL_MONTHS, My.Resources.LBL_NA_INDIC, True, LegendPos.TopCenter, 2)
        'Add curves
        pane.CurveList.Clear()
        'Ascendency on flow
        AddCurve(My.Resources.LBL_ASCEND_FLOW, Me.NetworkManager.AscendFlowEcosim, pane, Color.Gold)

        Me.m_zgh.RescaleAndRedraw()

        g = Me.Graph.Parent.CreateGraphics
        paneMaster.AxisChange(g)
        paneMaster.SetLayout(g, PaneLayout.SingleColumn)

    End Sub

End Class



