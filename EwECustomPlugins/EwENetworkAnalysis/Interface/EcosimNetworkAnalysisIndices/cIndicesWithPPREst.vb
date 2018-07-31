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

Imports ZedGraph
Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports System.Text
Imports EwEUtils
Imports EwECore
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

<CLSCompliant(False)> _
Public Class cIndicesWithPPREst
    Inherits cContentManager

    Private m_zgh As cZedGraphHelper = Nothing

    Public Overrides Function PageTitle() As String
        Return "Ecosim indices with PPR estimated"
    End Function

    Public Overrides Function Attach(ByVal manager As cNetworkManager, _
                                     ByVal datagrid As DataGridView, _
                                     ByVal graph As ZedGraphControl, _
                                     ByVal plot As ucPlot, _
                                     ByVal toolstrip As ToolStrip, _
                                     ByVal uic As cUIContext) As Boolean

        Dim bSucces As Boolean = MyBase.Attach(manager, datagrid, graph, plot, toolstrip, uic)

        ' PPR not on yet?
        If (Me.NetworkManager.EcosimPPROn = False) Then
            ' #Yes: prompt user if need to run
            bSucces = bSucces And (MsgBox(My.Resources.PROMPT_ESTIMATE_PPR, MsgBoxStyle.YesNo, My.Resources.CAPTION) = MsgBoxResult.Yes)
        End If

        ' Need to run?
        If bSucces Then
            ' #Yes: run std PP
            Me.NetworkManager.RunRequiredPrimaryProd()
            ' Switch on PPR in Ecosim
            Me.NetworkManager.UseEcosimNetwork = True
            Me.NetworkManager.EcosimPPROn = True
            ' Ecosim NA run successful?
            bSucces = bSucces And Me.NetworkManager.RunEcosimNetwork()
            Me.NetworkManager.UseEcosimNetwork = False
        Else
            bSucces = False
        End If

        Me.m_zgh = New cZedGraphHelper()
        Me.m_zgh.Attach(uic, Me.Graph, 2)
        Me.m_zgh.ShowPointValue = True

        Me.Graph.Visible = bSucces
        Me.Toolstrip.Visible = bSucces
        Me.ToolstripShowOptionCSV()

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

        'Pane1
        pane = Me.m_zgh.ConfigurePane("", My.Resources.LBL_MONTHS, My.Resources.LBL_NA_INDIC, True, LegendPos.TopCenter, 1)
        'Add curves
        pane.CurveList.Clear()
        'FIB
        AddCurve(My.Resources.LBL_FIB_INDX, Me.NetworkManager.FIB, pane, Color.Green)
        'Relative sum of catch
        AddCurve(My.Resources.LBL_TOTAL_CATCH, Me.NetworkManager.RelativeSumOfCatch, pane, Color.Red)
        'Relative Kemptons
        AddCurve(My.Resources.LBL_KEMPTONS_Q, Me.NetworkManager.RelativeDiversity, pane, Color.Blue)
        'TL catch
        AddCurve(My.Resources.LBL_TL_CATCH, Me.NetworkManager.TLCatch, pane, Color.Black)
        'FCI
        AddCurve(My.Resources.LBL_FCI, Me.NetworkManager.FCIEcosim, pane, Color.Brown)
        'Catch PPR
        AddCurve(My.Resources.LBL_CATCH_PPR, Me.NetworkManager.RelativeCatchPPR, pane, Color.Violet)
        'Catch detritus required
        AddCurve(My.Resources.LBL_CATCH_DET_REQ, Me.NetworkManager.RelativeDetritusReq, pane, Color.Orange)
        'L-index
        AddCurve(My.Resources.LBL_LINDEX_REL, Me.NetworkManager.LIndexPlot, pane, Color.DarkKhaki)
        'Psust
        AddCurve("Psust", Me.NetworkManager.PsustPlot, pane, Color.DarkMagenta)

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


