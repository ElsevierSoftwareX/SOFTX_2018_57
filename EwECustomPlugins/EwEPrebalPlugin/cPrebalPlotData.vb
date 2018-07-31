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

Option Explicit On
Option Strict On

Imports EwEUtils.Core

#End Region

Public Class cPrebalPlotData

#Region " Private vars "

    Private m_nGroups As Integer = 0
    Private m_iGroup As Integer()
    Private m_data As Single()
    Private m_status As eStatusFlags()
    Private m_result As cPrebalModel.eResultTypes = cPrebalModel.eResultTypes.NotSet

#End Region ' Private vars

    Public Sub New(result As cPrebalModel.eResultTypes)
        Me.m_result = result
    End Sub

    Public Sub Resize(nGroups As Integer)
        ReDim Me.m_data(nGroups)
        ReDim Me.m_status(nGroups)
        ReDim Me.m_iGroup(nGroups)
        Me.m_nGroups = nGroups
    End Sub

    Public ReadOnly Property Result As cPrebalModel.eResultTypes
        Get
            Return Me.m_result
        End Get
    End Property

    ''' <summary>
    ''' Array of data values. Indexes are one-based.
    ''' </summary>
    Public ReadOnly Property Data As Single()
        Get
            Return Me.m_data
        End Get
    End Property

    Public ReadOnly Property Status As eStatusFlags()
        Get
            Return Me.m_status
        End Get
    End Property

    Public ReadOnly Property nGroups As Integer
        Get
            Return Me.m_nGroups
        End Get
    End Property

    Public ReadOnly Property EcopathGroupIndexes As Integer()
        Get
            Return Me.m_iGroup
        End Get
    End Property

End Class

