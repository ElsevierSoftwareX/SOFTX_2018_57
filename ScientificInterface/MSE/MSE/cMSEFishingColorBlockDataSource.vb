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
Imports EwECore.MSE
Imports ScientificInterface.Ecosim
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports EwECore

#End Region ' Imports

''' <summary>
''' Implementation of IPolicyColorBlockDataSource for MSE Fleets
''' </summary>
''' <remarks></remarks>
Public Class cMSEFishingColorBlockDataSource
    Implements IPolicyColorBlockDataSource

    Private m_uic As cUIContext

    Private m_BlockCells(,) As Integer

    Private m_BlockSelector As ucCVBlockSelector
    Private m_iTotalBlocks As Integer
    Private m_batchEdit As Boolean

    Public ReadOnly Property BlockCells() As Integer(,) Implements IPolicyColorBlockDataSource.BlockCells
        Get
            Return m_BlockCells
        End Get
    End Property


    Public ReadOnly Property TotalBlocks() As Integer Implements IPolicyColorBlockDataSource.TotalBlocks
        Get
            Return Me.m_uic.Core.EcoSimModelParameters.NumberYears
        End Get
    End Property

    Public Sub New(ByVal UIContext As cUIContext)
        Me.m_uic = UIContext
    End Sub


    ''' <summary>
    ''' Attach an IBlockSelector to the data source
    ''' </summary>
    ''' <param name="BlockSelector">ucCVBlockSelector implementation of IBlockSelector</param>
    ''' <remarks>When an IBlockSelector is attached the datasource will add CV's that missing from the IBlockSelector.  </remarks>
    Public Sub Atatch(ByVal BlockSelector As IBlockSelector) Implements IPolicyColorBlockDataSource.Attach

        Debug.Assert(TypeOf BlockSelector Is ucCVBlockSelector, Me.ToString & ".Atatch() Blocks must be a ucCVBlockSelector!")
        Try
            m_BlockSelector = DirectCast(BlockSelector, ucCVBlockSelector)

            'populate the blocks with values from the data!!!!
            Dim cvs As New List(Of Single)
            cvs.Add(0) 'if adding values the first value should be zero
            Dim manager As cMSEManager = Me.m_uic.Core.MSEManager
            Dim blks() As Single = Me.m_BlockSelector.BlockValues

            For i As Integer = 1 To Me.m_uic.Core.nFleets
                Dim flt As cMSEFleetInput = manager.EcopathFleetInputs(i)
                For it As Integer = 1 To Me.m_uic.Core.nEcosimYears
                    Dim cv As Single = flt.FleetCV(it)

                    If Not blks.Contains(cv) Then
                        If Not cvs.Contains(cv) Then
                            cvs.Add(cv)
                        End If
                    End If

                Next ' Me.m_uic.Core.nEcosimYears
            Next '  Me.m_uic.Core.nFleets

            'cvs in the datasource that are not in the control
            If cvs.Count > 1 Then

                'skip the fist default value it will be zero
                For iblk As Integer = 1 To Me.m_BlockSelector.NumBlocks
                    cvs.Insert(iblk, blks(iblk))
                Next ' For iblk As Integer = 1 To Me.m_blockCodes.NumBlocks
                cvs.Sort()
                m_BlockSelector.BlockValues = cvs.ToArray
                '   m_blockCodes.Invalidate()
            End If

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
        End Try

    End Sub

    Public Sub Init() Implements IPolicyColorBlockDataSource.Init

        m_iTotalBlocks = Me.m_uic.Core.EcoSimModelParameters.NumberYears

        ReDim m_BlockCells(Me.nRows, Me.TotalBlocks)
        Dim mseData As cMSEFleetInput
        Dim sYear As Integer = Me.m_uic.Core.MSEManager.ModelParameters.MSEStartYear

        For iflt As Integer = 1 To Me.nRows
            mseData = Me.m_uic.Core.MSEManager.EcopathFleetInputs(iflt)
            For iTime As Integer = 1 To Me.TotalBlocks
                If iTime >= sYear Then
                    m_BlockCells(iflt, iTime) = Me.m_BlockSelector.ValuetoBlock(mseData.FleetCV(iTime))
                Else
                    m_BlockCells(iflt, iTime) = -1
                End If
            Next
        Next

    End Sub

    Public Sub FillBlock(ByVal iRow As Integer, ByVal iCol As Integer) Implements IPolicyColorBlockDataSource.FillBlock

        ' Sanity checks
        'If (iCol <= Me.m_uic.Core.FishingPolicyManager.ObjectiveParameters.BaseYear) Then Return

        If (iRow < 1) Then Return
        If (iRow > m_BlockCells.GetLength(0) - 1) Then Return

        If iCol < Me.m_uic.Core.MSEManager.ModelParameters.MSEStartYear Then
            'Not in bounds 
            'Don't set the value
            Return
        End If


        ' Fill single block

        Me.m_BlockCells(iRow, iCol) = Me.m_BlockSelector.SelectedBlock
        Me.m_uic.Core.MSEManager.EcopathFleetInputs(iRow).FleetCV(iCol) = Me.m_BlockSelector.BlocktoValue(Me.m_BlockSelector.SelectedBlock)

    End Sub

    Public Sub SetSeqColorCodes(ByVal startYear As Integer, ByVal endYear As Integer, ByVal yearPerBlock As Integer) Implements IPolicyColorBlockDataSource.SetSeqColorCodes

        'Sequence years not implemented for MSE fleets

    End Sub

    Public ReadOnly Property nRows() As Integer Implements IPolicyColorBlockDataSource.nRows
        Get
            Return Me.m_uic.Core.nFleets
        End Get
    End Property

    Public ReadOnly Property RowLabel(ByVal iRow As Integer) As String Implements IPolicyColorBlockDataSource.RowLabel
        Get
            Try
                Return String.Format(SharedResources.GENERIC_LABEL_INDEXED, _
                                     iRow, _
                                     Me.m_uic.Core.MSEManager.EcopathFleetInputs(iRow).Name)
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".RowLabel() Exception: " & ex.Message)
            End Try
            Return String.Empty
        End Get
    End Property

    Public Property BatchEdit() As Boolean Implements Ecosim.IPolicyColorBlockDataSource.BatchEdit
        Get
            Return Me.m_batchEdit
        End Get
        Set(ByVal value As Boolean)

            Me.m_batchEdit = value

            Dim mse As cMSEManager = Me.m_uic.Core.MSEManager
            For iflt As Integer = 1 To Me.nRows
                mse.EcopathFleetInputs(iflt).BatchEdit = Me.m_batchEdit
            Next iflt

        End Set
    End Property

    Public Sub Update() Implements Ecosim.IPolicyColorBlockDataSource.Update

        Try

            Dim mse As cMSEManager = Me.m_uic.Core.MSEManager
            For iflt As Integer = 1 To Me.nRows
                mse.EcopathFleetInputs(iflt).BatchEdit = True
                For iyr As Integer = 1 To Me.TotalBlocks
                    mse.EcopathFleetInputs(iflt).FleetCV(iyr) = Me.m_BlockSelector.BlocktoValue(m_BlockCells(iflt, iyr))
                Next
                mse.EcopathFleetInputs(iflt).BatchEdit = False
            Next iflt

        Catch ex As Exception
            System.Console.WriteLine(ex.Message)
        End Try

    End Sub

    Public ReadOnly Property isControlPanelVisible() As Boolean Implements Ecosim.IPolicyColorBlockDataSource.isControlPanelVisible
        Get
            Return False
        End Get
    End Property

    Public Function BlockToValue(ByVal iBlock As Integer) As Single Implements Ecosim.IPolicyColorBlockDataSource.BlockToValue
        Try
            Return Me.m_BlockSelector.BlocktoValue(iBlock)
        Catch ex As Exception
            Return cCore.NULL_VALUE
        End Try
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
