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
Imports System.ComponentModel
Imports EwEUtils.Core

#End Region ' Imports

Namespace Ecosim

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class, implements the actual display of an EwE flow diagram using
    ''' a simple layout of nodes connected via arched lines.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cEcosimTreeFlowDiagramRenderer
        Inherits cTreeFlowDiagramRenderer

#Region " Private vars "

        Private m_tsShowBiomassLegend As Boolean = False
        Private m_tsShowFlowRateLegend As Boolean = False

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal data As IFlowDiagramData)
            MyBase.New(data)
        End Sub

#End Region ' Constructor

#Region " Properties "

        Public Event OnBiomassLegendChanged(ByVal sender As cTreeFlowDiagramRenderer)
        Public Event OnFlowRateLegendChanged(ByVal sender As cTreeFlowDiagramRenderer)

        <Browsable(True),
            Category("Appearance"),
            cLocalizedDisplayName("GENERIC_SHOW_BIOMASS_LEGEND"),
            DefaultValue(False)>
        Public Property ShowBiomassLegend As Boolean
            Get
                Return Me.m_tsShowBiomassLegend
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_tsShowBiomassLegend) Then
                    Me.m_tsShowBiomassLegend = value
                    RaiseEvent OnBiomassLegendChanged(Me)
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            Category("Appearance"),
            cLocalizedDisplayName("GENERIC_SHOW_FLOW_RATE_LEGEND"),
            DefaultValue(TriState.False)>
        Public Property ShowFlowRateLegend As Boolean
            Get
                Return Me.m_tsShowFlowRateLegend
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_tsShowFlowRateLegend) Then
                    Me.m_tsShowFlowRateLegend = value
                    RaiseEvent OnFlowRateLegendChanged(Me)
                    Me.Update()
                End If
            End Set
        End Property

#End Region ' Properties

    End Class

End Namespace