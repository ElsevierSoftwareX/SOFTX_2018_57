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

Option Strict On
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

''' <summary>
''' The definition of a Marine Protected Area in Ecospace.
''' </summary>
''' <seealso cref="EwECore.cCoreInputOutputBase" />
''' <seealso cref="EwECore.cEcospaceBasemap.LayerMPA(Integer)"/>
Public Class cEcospaceMPA
    Inherits cCoreInputOutputBase

#Region "Constructor"

    Sub New(ByRef theCore As cCore, ByVal iDBID As Integer)
        MyBase.New(theCore)

        Dim val As cValue = Nothing

        Try

            Me.m_dataType = eDataTypes.EcospaceMPA
            Me.m_coreComponent = eCoreComponentType.EcoSpace
            Me.DBID = iDBID

            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            Me.ResetStatusFlags()

            ' MPAMonth
            val = New cValueArray(eValueTypes.BoolArray, eVarNameFlags.MPAMonth, eStatusFlags.OK, eCoreCounterTypes.nMonths, AddressOf m_core.GetCoreCounter)
            Me.m_values.Add(val.varName, val)

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcospaceMPA.")
            cLog.Write(Me.ToString & ".New(nGroups) Error creating new cEcospaceMPA. Error: " & ex.Message)
        End Try

    End Sub

#End Region

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the number of cells in a MPA.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property NumCells() As Integer
        Get
            Dim bm As cEcospaceBasemap = Me.m_core.EcospaceBasemap
            Dim l As cEcospaceLayerMPA = bm.LayerMPA(Me.Index)
            Dim iIndex As Integer = Me.Index
            Dim iNumCells As Integer = 0

            For iRow As Integer = 1 To bm.InRow
                For iCol As Integer = 1 To bm.InCol
                    ' Only include modelled cells in this count
                    If (CInt(l.Cell(iRow, iCol)) > 0) And (bm.IsModelledCell(iRow, iCol)) Then
                        iNumCells += 1
                    End If
                Next
            Next
            Return iNumCells

        End Get
    End Property

    Public ReadOnly Property IsActive() As Boolean
        Get
            Dim bIsClosed As Boolean = False
            Dim bIsApplied As Boolean = False
            For i As Integer = 1 To cCore.N_MONTHS : bIsClosed = bIsClosed Or Me.IsClosed(i) : Next
            For i As Integer = 1 To Me.m_core.nFleets : bIsApplied = bIsApplied Or (Me.m_core.EcospaceFleetInputs(i).MPAFishery(Me.Index) = False) : Next
            Return bIsClosed And bIsApplied
        End Get
    End Property

#Region " Variables by dot '.' operator "

    ''' <summary>
    ''' Get/set if an MPA is OPEN for fishing for a given month.
    ''' </summary>
    ''' <param name="iMonth">The one-based month index to access the 
    ''' MPA open state for.</param>
    Public Property MPAMonth(ByVal iMonth As Integer) As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.MPAMonth, iMonth))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.MPAMonth, value, iMonth)
        End Set
    End Property

#End Region ' Variables by dot '.' operator

#Region " Status by dot (.) operator "

    Public Property MPAMonthStatus(ByVal iMonth As Integer) As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.MPAMonth, iMonth)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.MPAMonth, value, iMonth)
        End Set
    End Property

#End Region ' Status by dot (.) operator

#Region " Quick accessors "

    Public Property IsOpen(ByVal iMonth As Integer) As Boolean
        Get
            Return Me.MPAMonth(iMonth) = True
        End Get
        Set(value As Boolean)
            Me.MPAMonth(iMonth) = (value = True)
        End Set
    End Property

    Public Property IsClosed(ByVal iMonth As Integer) As Boolean
        Get
            Return Me.MPAMonth(iMonth) = False
        End Get
        Set(value As Boolean)
            Me.MPAMonth(iMonth) = (value = False)
        End Set
    End Property

#End Region ' Quick accessors

End Class
