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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

#End Region

Public Class cMSEBatchParameters
    Inherits cCoreGroupBase


    Public Sub New(ByRef theCore As cCore, ByRef MSEBatchData As MSEBatchManager.cMSEBatchDataStructures, ByVal DBID As Integer)
        MyBase.New(theCore)

        Dim val As cValue

        m_dataType = eDataTypes.MSEBatchParameters
        m_coreComponent = eCoreComponentType.MSE
        Me.AllowValidation = False
        Me.DBID = DBID

        'default OK status used for setVariable
        'see comment setVariable(...)
        m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        val = New cValue(New Integer, eVarNameFlags.MSETFMNIteration, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        val = New cValue(New Integer, eVarNameFlags.MSEBatchFNIteration, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        val = New cValue(New Integer, eVarNameFlags.MSEBatchTACNIteration, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        val = New cValue(New Integer, eVarNameFlags.MSEBatchIterCalcType, eStatusFlags.Null, eValueTypes.Int)
        m_values.Add(val.varName, val)

        val = New cValue(New Boolean, eVarNameFlags.MSEBatchOutputBiomass, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        val = New cValue(New Boolean, eVarNameFlags.MSEBatchOutputConBio, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        val = New cValue(New Boolean, eVarNameFlags.MSEBatchOutputFeedingTime, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        val = New cValue(New Boolean, eVarNameFlags.MSEBatchOutputPredRate, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        val = New cValue(New Boolean, eVarNameFlags.MSEBatchOutputCatch, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        val = New cValue(New Boolean, eVarNameFlags.MSEBatchOutputFishingMortRate, eStatusFlags.Null, eValueTypes.Bool)
        val.Stored = False
        m_values.Add(val.varName, val)

        val = New cValue("", eVarNameFlags.MSEBatchOuputDir, eStatusFlags.Null, eValueTypes.Str)
        val.Stored = False
        m_values.Add(val.varName, val)

        Me.AllowValidation = True

    End Sub

    Public Property nTFMIteration As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.MSETFMNIteration))
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.MSETFMNIteration, value)
        End Set
    End Property

    Public Property nFixedFIteration As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.MSEBatchFNIteration))
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.MSEBatchFNIteration, value)
        End Set
    End Property

    Public Property nTACIteration As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.MSEBatchTACNIteration))
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.MSEBatchTACNIteration, value)
        End Set
    End Property


    Public Property IterCalcType As Integer
        Get
            Return CType(GetVariable(eVarNameFlags.MSEBatchIterCalcType), eMSEBatchIterCalcTypes)
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.MSEBatchIterCalcType, value)
        End Set
    End Property


    Public Property bSaveBiomass As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.MSEBatchOutputBiomass))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.MSEBatchOutputBiomass, value)
        End Set
    End Property

    Public Property bSaveCatch As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.MSEBatchOutputCatch))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.MSEBatchOutputCatch, value)
        End Set
    End Property

    Public Property bSaveConsumptBio As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.MSEBatchOutputConBio))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.MSEBatchOutputConBio, value)
        End Set
    End Property

    Public Property bSaveFeedingTime As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.MSEBatchOutputFeedingTime))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.MSEBatchOutputFeedingTime, value)
        End Set
    End Property

    Public Property bSavePredRate As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.MSEBatchOutputPredRate))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.MSEBatchOutputPredRate, value)
        End Set
    End Property
    Public Property bSaveFishingMort As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.MSEBatchOutputFishingMortRate))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.MSEBatchOutputFishingMortRate, value)
        End Set
    End Property

    Public Property OutputDir As String
        Get
            Return CStr(GetVariable(eVarNameFlags.MSEBatchOuputDir))
        End Get
        Set(value As String)
            SetVariable(eVarNameFlags.MSEBatchOuputDir, value)
        End Set
    End Property


    Friend Overrides Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean
        MyBase.ResetStatusFlags(bForceReset)
        Me.AllowValidation = False
        Dim tcatch As Single

        For iflt As Integer = 1 To Me.m_core.nFleets
            Dim fleet As cEcopathFleetInput = Me.m_core.EcopathFleetInputs(iflt)
            tcatch += fleet.Landings(Me.Index) + fleet.Discards(Me.Index)
        Next

        If tcatch = 0.0! Then
            For Each var As cValue In Me.m_values.Values
                If var.varName = eVarNameFlags.MSEBatchGroupRunType Then
                    For igrp As Integer = 1 To Me.m_core.nGroups
                        Me.SetStatusFlags(var.varName, eStatusFlags.Null Or eStatusFlags.NotEditable, igrp)
                    Next
                End If
            Next
        End If
        Return True

    End Function



End Class
