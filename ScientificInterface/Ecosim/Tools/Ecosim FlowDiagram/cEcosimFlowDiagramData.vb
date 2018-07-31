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
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Ecosim

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Data for rendering the Ecosim groups and trophic links as a flow diagram.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cEcosimFlowDiagramData
        Implements IFlowDiagramData

#Region " Internals "

        Private m_sDietMin As Single = 0
        Private m_sDietMax As Single = 0
        Private m_sBiomassMin As Single = 0
        Private m_sBiomassMax As Single = 0

        Private m_bInvalid As Boolean = True
        Private m_core As cCore = Nothing
        Private m_sg As cStyleGuide = Nothing

#End Region ' Internals

#Region " Constructor "

        Public Sub New(ByVal uic As cUIContext)
            Me.UIContext = uic
            Me.m_core = uic.Core
            Me.m_sg = uic.StyleGuide
            Me.DataTitle = ScientificInterfaceShared.My.Resources.HEADER_BIOMASS
        End Sub

#End Region ' Constructor

#Region " Properties "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.UIContext"/>
        ''' -------------------------------------------------------------------
        Friend Property UIContext() As cUIContext _
            Implements IFlowDiagramData.UIContext

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.Refresh"/>
        ''' -------------------------------------------------------------------
        Public Sub Refresh() _
            Implements IFlowDiagramData.Refresh
            Me.m_bInvalid = True
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.NumItems"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property NumItems() As Integer _
              Implements IFlowDiagramData.NumItems
            Get
                Return Me.m_core.nGroups
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.NumLivingItems"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property NumLivingitems() As Integer _
                Implements IFlowDiagramData.NumLivingItems
            Get
                Return Me.m_core.nLivingGroups
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.Value"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Value(ByVal iIndex As Integer) As Single _
               Implements IFlowDiagramData.Value
            Get
                Return Me.m_core.EcoSimGroupOutputs(iIndex).Biomass(Me.TimeStep)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.ValueLabel"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ValueLabel(ByVal sBiomass As Single) As String _
              Implements IFlowDiagramData.ValueLabel
            Get
                Return cStringUtils.Localize(My.Resources.FLOWDIAGRAM_LABEL_BIOMASS, Me.m_sg.FormatNumber(sBiomass, cStyleGuide.eStyleFlags.OK))
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.ItemName"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ItemName(ByVal iIndex As Integer) As String _
                Implements IFlowDiagramData.ItemName
            Get
                Return Me.m_core.EcoPathGroupInputs(iIndex).Name
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.ItemColor"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ItemColor(ByVal iGroup As Integer) As Color _
                Implements IFlowDiagramData.ItemColor
            Get
                Return Me.m_sg.GroupColor(Me.UIContext.Core, iGroup)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.IsItemVisible"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property IsItemVisible(ByVal iGroup As Integer) As Boolean _
                Implements IFlowDiagramData.IsItemVisible
            Get
                Return Me.m_sg.GroupVisible(iGroup)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.LinkValue"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property LinkValue(ByVal iPred As Integer, ByVal iPrey As Integer) As Single _
               Implements IFlowDiagramData.LinkValue
            Get
                ' ToDo: obtain this from Ecosim
                Dim group As cEcoPathGroupInput = Me.m_core.EcoPathGroupInputs(iPred)
                Return group.DietComp(iPrey)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.TrophicLevel"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property TrophicLevel(ByVal iIndex As Integer) As Single _
                Implements IFlowDiagramData.TrophicLevel
            Get
                Return Me.m_core.EcoPathGroupOutputs(iIndex).TTLX
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.ValueMax"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ValueMax() As Single _
                Implements IFlowDiagramData.ValueMax
            Get
                Me.Recalc()
                Return Me.m_sBiomassMax
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.ValueMin"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ValueMin() As Single _
               Implements IFlowDiagramData.ValueMin
            Get
                Me.Recalc()
                Return Me.m_sBiomassMin
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.LinkValueMin"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property LinkValueMin() As Single _
                 Implements IFlowDiagramData.LinkValueMin
            Get
                Me.Recalc()
                Return Me.m_sDietMin
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.LinkValueMax"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property LinkValueMax() As Single _
                  Implements IFlowDiagramData.LinkValueMax
            Get
                Me.Recalc()
                Return Me.m_sDietMax
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.Title"/>
        ''' -------------------------------------------------------------------
        Public Property Title As String _
            Implements IFlowDiagramData.Title

        ''' -------------------------------------------------------------------
        ''' <summary>Get/set the time step in Ecosim.</summary>
        ''' -------------------------------------------------------------------
        Public Property TimeStep As Integer

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IFlowDiagramData.DataTitle"/>
        ''' -------------------------------------------------------------------
        Public Property DataTitle As String _
            Implements IFlowDiagramData.DataTitle

#End Region ' Properties

#Region " Internals "

        Private Sub Recalc()

            If Not Me.m_bInvalid Then Return

            Me.m_sBiomassMax = 0
            Me.m_sBiomassMin = Single.MaxValue
            Me.m_sDietMax = 0
            Me.m_sDietMin = Single.MaxValue

            For i As Integer = 1 To Me.NumItems
                For j As Integer = 1 To Me.NumItems
                    Dim sDiet As Single = Me.LinkValue(i, j)
                    Me.m_sDietMax = Math.Max(Me.m_sDietMax, sDiet)
                    Me.m_sDietMin = Math.Min(Me.m_sDietMin, sDiet)

                Next j

                Dim sB As Single = Me.Value(i)
                Me.m_sBiomassMax = Math.Max(Me.m_sBiomassMax, sB)
                Me.m_sBiomassMin = Math.Min(Me.m_sBiomassMin, sB)

            Next i

            Me.m_bInvalid = False

        End Sub

#End Region ' Interals

    End Class

End Namespace
