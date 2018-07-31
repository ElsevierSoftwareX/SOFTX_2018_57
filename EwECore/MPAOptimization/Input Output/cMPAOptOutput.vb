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
''' Results of the current search iteration
''' </summary>
''' <remarks></remarks>
Public Class cMPAOptOutput
    Inherits cCoreGroupBase

    'Private m_cells As List(Of cMPACell)

    Sub New(ByRef theCore As cCore)
        MyBase.New(theCore)
        Dim val As cValue = Nothing

        Me.DBID = cCore.NULL_VALUE '????
        Me.Index = cCore.NULL_VALUE
        Me.m_dataType = eDataTypes.MPAOptOuput

        ' Outputs should never send out messages
        m_coreComponent = eCoreComponentType.MPAOptimization
        'default OK status used for SetVariable
        'see comment SetVariable(...)
        m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK Or eStatusFlags.NotEditable, "", eVarNameFlags.NotSet)

        val = New cValue(New Integer, eVarNameFlags.MPAOptBestCol, eStatusFlags.NotEditable, eValueTypes.Int)
        m_values.Add(val.varName, val)

        val = New cValue(New Integer, eVarNameFlags.MPAOptBestRow, eStatusFlags.NotEditable, eValueTypes.Int)
        m_values.Add(val.varName, val)

        val = New cValue(New Integer, eVarNameFlags.MPAOptCurRow, eStatusFlags.NotEditable, eValueTypes.Int)
        m_values.Add(val.varName, val)

        val = New cValue(New Integer, eVarNameFlags.MPAOptCurCol, eStatusFlags.NotEditable, eValueTypes.Int)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.MPAOptEconomicValue, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.MPAOptEcologicalValue, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.MPAOptMandatedValue, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.MPAOptSocialValue, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.MPAOptTotalValue, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Integer, eVarNameFlags.MPAOptPercentageClosed, eStatusFlags.NotEditable, eValueTypes.Int)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.MPAOptBiomassDiversityValue, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)

        val = New cValue(New Single, eVarNameFlags.MPAOptAreaBoundary, eStatusFlags.NotEditable, eValueTypes.Sng)
        m_values.Add(val.varName, val)



    End Sub


    Friend Sub Init(ByRef mpaData As cMPAOptDataStructures, ByVal SpaceData As cEcospaceDataStructures)

        Me.BestRow = mpaData.bestrow
        Me.BestCol = mpaData.bestcol
        Me.CurRow = mpaData.CurRow
        Me.CurCol = mpaData.CurCol

        Me.EcologicalValue = mpaData.objFuncEcologicalValue
        Me.EconomicValue = mpaData.objFuncEconomicValue
        Me.MandatedValue = mpaData.objFuncMandatedValue
        Me.SocialValue = mpaData.objFuncSocialValue
        Me.TotalValue = mpaData.objFuncTotal
        Me.BiomassDiversityValue = mpaData.objFuncBiodiversity
        Me.AreaBoundaryValue = mpaData.objFuncAreaBorder

        Dim nTotCells As Integer = SpaceData.nWaterCells
        Dim nMPACells As Integer
        For ir As Integer = 1 To SpaceData.InRow
            For ic As Integer = 1 To SpaceData.InCol
                'SpaceData.MPA(ir, ic) = mpaData.iMPAtoUse
                'If SpaceData.MPA(ir, ic) = mpaData.iMPAtoUse Then
                If SpaceData.MPA(mpaData.iMPAtoUse)(ir, ic) = True Then
                    nMPACells += 1
                End If
            Next
        Next

        Me.PercentageClosed = CInt(nMPACells / nTotCells * 100)

    End Sub

    ''' <summary>
    ''' Row of the best cell for the currrent Ecoseed evaluation
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property BestRow() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.MPAOptBestRow))
        End Get

        Set(ByVal newValue As Integer)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptBestRow, newValue)
            End If
        End Set

    End Property

    ''' <summary>
    ''' Col of the best cell for the currrent Ecoseed evaluation
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property BestCol() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.MPAOptBestCol))
        End Get

        Set(ByVal newValue As Integer)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptBestCol, newValue)
            End If
        End Set

    End Property

    ''' <summary>
    ''' Row of current cell being evaluated by Ecoseed
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CurRow() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.MPAOptCurRow))
        End Get

        Set(ByVal newValue As Integer)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptCurRow, newValue)
            End If
        End Set

    End Property

    ''' <summary>
    '''  Col of current cell being evaluated by Ecoseed
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CurCol() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.MPAOptCurCol))
        End Get

        Set(ByVal newValue As Integer)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptCurCol, newValue)
            End If
        End Set

    End Property


    Public Property EconomicValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MPAOptEconomicValue))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptEconomicValue, newValue)
            End If
        End Set

    End Property

    Public Property EcologicalValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MPAOptEcologicalValue))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptEcologicalValue, newValue)
            End If
        End Set

    End Property

    Public Property MandatedValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MPAOptMandatedValue))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptMandatedValue, newValue)
            End If
        End Set

    End Property

    Public Property SocialValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MPAOptSocialValue))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptSocialValue, newValue)
            End If
        End Set

    End Property

    Public Property BiomassDiversityValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MPAOptBiomassDiversityValue))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptBiomassDiversityValue, newValue)
            End If
        End Set

    End Property

    Public Property TotalValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MPAOptTotalValue))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptTotalValue, newValue)
            End If
        End Set

    End Property


    Public Property AreaBoundaryValue() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.MPAOptAreaBoundary))
        End Get

        Set(ByVal newValue As Single)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptAreaBoundary, newValue)
            End If
        End Set

    End Property

    Public Property PercentageClosed() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.MPAOptPercentageClosed))
        End Get

        Set(ByVal newValue As Integer)
            If Not m_bReadOnly Then
                SetVariable(eVarNameFlags.MPAOptPercentageClosed, newValue)
            End If
        End Set

    End Property

End Class
