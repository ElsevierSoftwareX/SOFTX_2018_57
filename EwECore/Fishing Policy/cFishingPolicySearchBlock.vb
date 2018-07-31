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

Imports EwECore.ValueWrapper
Imports EwEUtils.Core

''' <summary>
''' Blocks for Fishing Policy Search
''' </summary>
''' <remarks>This provides the CodeBlocks(iTimeIndex) interface</remarks>
Public Class cFishingPolicySearchBlock
    Inherits cCoreGroupBase

    Public Sub New(ByVal theCore As cCore, ByVal DBID As Integer)
        MyBase.New(theCore)

        Dim val As cValue

        m_dataType = eDataTypes.FishingPolicySearchBlocks
        m_coreComponent = eCoreComponentType.FishingPolicySearch
        Me.AllowValidation = False
        Me.DBID = DBID

        'default OK status used for setVariable
        'see comment setVariable(...)
        m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

        val = New cValueArray(eValueTypes.IntArray, eVarNameFlags.SearchBlock, eStatusFlags.Null, eCoreCounterTypes.nEcosimYears, AddressOf m_core.GetCoreCounter)
        m_values.Add(val.varName, val)

        Me.AllowValidation = True

    End Sub


    Public Property SearchBlocks(ByVal iTimeIndex As Integer) As Integer

        Get
            Return CInt(GetVariable(eVarNameFlags.SearchBlock, iTimeIndex))
        End Get

        Set(ByVal value As Integer)
            SetVariable(eVarNameFlags.SearchBlock, value, iTimeIndex)
        End Set

    End Property

    ''' <summary>
    ''' Edit the SearchBlocks in batch mode no messages are sent out when BatchEdit = True when BatchEdit is toggled to False then the core is notified.
    ''' </summary>
    ''' <remarks>This turns off the AllowValidation flag which stops the object from calling core.OnValidate() vastly speeding up the editing</remarks>
    Public Property BatchEdit() As Boolean
        Get
            Return Not Me.AllowValidation
        End Get

        Set(ByVal value As Boolean)

            'if turning the BatchEdit On after it has been OFF tell the core that the values has been edited
            'this will allow the core to update the underlying data and send out a datamodified message
            If Me.BatchEdit = True And value = False Then
                Me.m_core.OnValidated(m_values.Item(eVarNameFlags.SearchBlock), Me)
            End If
            Me.AllowValidation = Not value

        End Set

    End Property


End Class
