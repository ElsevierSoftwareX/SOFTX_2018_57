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
Imports EwECore.SearchObjectives
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Grid allowing setting of search objective weights.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(False)> _
   Public Class gridSearchObjectivesWeight
        : Inherits EwEGrid

#Region " Private vars "

        Private m_manager As ISearchObjective = Nothing
        Private m_bIsBatchRun As Boolean = False
        Private m_bShowMaxPortUtil As Boolean = False
        Private m_bShowMPAOptParams As Boolean = False

#End Region ' Private vars

        Public Sub New()
            MyBase.New()
        End Sub

#Region " Properties "

        Public Property Manager() As ISearchObjective
            Get
                Return Me.m_manager
            End Get
            Set(ByVal value As ISearchObjective)
                Me.m_manager = value
                Me.RefreshContent()
            End Set
        End Property

        Public Property ShowMaxPortUtil() As Boolean
            Get
                Return Me.m_bShowMaxPortUtil
            End Get
            Set(ByVal value As Boolean)
                Me.m_bShowMaxPortUtil = value
                Me.RefreshContent()
            End Set
        End Property

        Public Property ShowMPAOptParams() As Boolean
            Get
                Return Me.m_bShowMPAOptParams
            End Get
            Set(ByVal value As Boolean)
                Me.m_bShowMPAOptParams = value
                Me.RefreshContent()
            End Set
        End Property

        ' JS 12Nov08: not implemented yet
        'Public Property IsBatchRun() As Boolean
        '    Get
        '        Return Me.m_bIsBatchRun
        '    End Get
        '    Set(ByVal value As Boolean)
        '        Me.m_bIsBatchRun = value
        '        Me.RefreshContent
        '    End Set
        'End Property

#End Region ' Properties

#Region " Overrides "

        Protected Overrides Sub InitStyle()
            MyBase.InitStyle()

            Dim iCol As Integer = 0
            ' Resize grid
            Me.Redim(1, Me.NumCols)

            ' == Add columns (for details refer to NumCols) ==

            ' Standard cols
            Me(0, iCol) = New EwEColumnHeaderCell(SharedResources.HEADER_OBJECTIVE) : iCol += 1
            ' Batch run specific cols
            If Me.m_bIsBatchRun Then
                Me(0, iCol) = New EwEColumnHeaderCell(SharedResources.HEADER_MINWEIGHT) : iCol += 1
                Me(0, iCol) = New EwEColumnHeaderCell(SharedResources.HEADER_MAXWEIGHT) : iCol += 1
                Me(0, iCol) = New EwEColumnHeaderCell(SharedResources.GENERIC_LABEL_STEP_SIZE) : iCol += 1
            Else
                Me(0, iCol) = New EwEColumnHeaderCell(SharedResources.HEADER_RELATIVEWEIGHT_ABBR) : iCol += 1
            End If

        End Sub

        Protected Overrides Sub FillData()

            If (Me.Manager Is Nothing) Then Return
            If (Me.UIContext Is Nothing) Then Return

            Dim source As cCoreInputOutputBase = m_manager.ValueWeights
            Dim iRow As Integer = 0

            ' == POPULATE ROWS (for details refer to NumRows) ==
            ' JS 12Nov08: this code does not account for Batch run columns yet

            ' Standard rows
            iRow = Me.AddRow()
            Me(iRow, 0) = New EwERowHeaderCell(SharedResources.HEADER_NET_ECONOMIC_VALUE)
            Me(iRow, 1) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSEconomicWeight)

            ' MaxPortUtil rows
            If m_bShowMaxPortUtil Then
                iRow = Me.AddRow()
                Me(iRow, 0) = New EwERowHeaderCell(SharedResources.HEADER_PREDICTIONVARIANCE)
                Me(iRow, 1) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSPredictionVariance)

                iRow = Me.AddRow()
                Me(iRow, 0) = New EwERowHeaderCell(SharedResources.HEADER_EXISTENCE_VALUE)
                Me(iRow, 1) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSExistenceValue)

            Else
                iRow = Me.AddRow()
                Me(iRow, 0) = New EwERowHeaderCell(SharedResources.HEADER_SOCIAL_VALUE_EMPLOYMENT)
                Me(iRow, 1) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSSocialWeight)

                iRow = Me.AddRow()
                Me(iRow, 0) = New EwERowHeaderCell(SharedResources.HEADER_MANDATED_REBUILDING)
                Me(iRow, 1) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSMandatedRebuildingWeight)

                iRow = Me.AddRow()
                Me(iRow, 0) = New EwERowHeaderCell(SharedResources.HEADER_ECOSYSTEM_STRUCTURE)
                Me(iRow, 1) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSEcoSystemWeight)
            End If

            iRow = Me.AddRow()
            Me(iRow, 0) = New EwERowHeaderCell(SharedResources.HEADER_BIODIVERSITY)
            Me(iRow, 1) = New PropertyCell(Me.PropertyManager, source, eVarNameFlags.FPSBiomassDiversityWeight)

            If Me.m_bShowMPAOptParams Then
                ' HACK
                iRow = Me.AddRow()
                Me(iRow, 0) = New EwERowHeaderCell(SharedResources.HEADER_BOUNDARYWEIGHT)
                Me(iRow, 1) = New PropertyCell(Me.PropertyManager, _
                                               Me.Core.MPAOptimizationManager.MPAOptimizationParamters, _
                                               eVarNameFlags.MPAOptBoundaryWeight)
            Else

            End If

        End Sub

        Protected Overrides Sub FinishStyle()
            MyBase.FinishStyle()
            Me.FixedColumns = 1
            Me.FixedColumnWidths = False
        End Sub

#End Region ' Overrides

#Region " Internal implementation "

        Private Function NumCols() As Integer

            ' Fixed col: ValueComponent
            Dim iNumCols As Integer = 1

            ' Batch run?
            If Me.m_bIsBatchRun Then
                ' #Yes: MinWeight, MaxWeight, Stepsize
                iNumCols += 3
            Else
                ' #No: RelWeight
                iNumCols += 1
            End If

            Return iNumCols

        End Function

        'Private Function NumRows() As Integer

        '    ' Fixed rows: Header, NetEconValue, BiomassDiversity
        '    Dim iNumRows As Integer = 3

        '    ' MaxPortUtil?
        '    If Me.m_bShowMaxPortUtil Then
        '        ' #Yes: add PredictionVariance, ExistenceValue rows
        '        iNumRows += 2
        '    Else
        '        ' #No: add SocialValue, MandatedRebuilding, EcosystemStructure
        '        iNumRows += 3
        '    End If

        '    ' MPAOpt?
        '    If Me.m_bShowMPAOptParams Then
        '        ' #Yes: add BoundaryWeight
        '        iNumRows += 1
        '    Else
        '        ' #No: NOP
        '    End If

        '    Return iNumRows

        'End Function

#End Region ' Internal implementation

    End Class

End Namespace

