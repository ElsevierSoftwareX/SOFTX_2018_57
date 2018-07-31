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

Public MustInherit Class cEwEScenario
    Inherits cCoreInputOutputBase

#Region " Constructor "

    Sub New(ByRef theCore As cCore)
        MyBase.New(theCore)

        Dim val As cValue
        Dim meta As cVariableMetaData
        Dim desc() As Char

        Try

            Me.m_dataType = eDataTypes.NotSet
            ' Scenario definition changes do not affect the running state of the model
            Me.m_coreComponent = eCoreComponentType.DataSource
            'default OK status used for setVariable
            'see comment setVariable(...)
            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            ' Description
            meta = New cVariableMetaData(60000)
            val = New cValue(New String(desc), eVarNameFlags.Description, eStatusFlags.OK, eValueTypes.Str)
            m_values.Add(val.varName, val)

            ' Author
            val = New cValue(New String(desc), eVarNameFlags.Author, eStatusFlags.OK, eValueTypes.Str)
            m_values.Add(val.varName, val)

            ' Contact
            val = New cValue(New String(desc), eVarNameFlags.Contact, eStatusFlags.OK, eValueTypes.Str)
            m_values.Add(val.varName, val)

            ' Last saved julian date
            val = New cValue(New Single, eVarNameFlags.LastSaved, eStatusFlags.OK, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            'set status flags to their default values
            ResetStatusFlags()
            Me.AllowValidation = True

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEwEScenario.")
            cLog.Write(Me.ToString & ".New(nGroups) Error creating new cEwEScenario. Error: " & ex.Message)
        End Try

    End Sub

#End Region ' Constructor

#Region " Public access "

    Public MustOverride Function IsLoaded() As Boolean

#End Region ' Public access

#Region " Variable via dot(.) operator"

    Public Property Description() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Description))
        End Get

        Set(ByVal str As String)
            SetVariable(eVarNameFlags.Description, str)
        End Set
    End Property

    Public Property Author() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Author))
        End Get

        Set(ByVal str As String)
            SetVariable(eVarNameFlags.Author, str)
        End Set
    End Property

    Public Property Contact() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Contact))
        End Get

        Set(ByVal str As String)
            SetVariable(eVarNameFlags.Contact, str)
        End Set
    End Property

    ''' <summary>
    ''' Get/set the Julian date the scenario was last saved.
    ''' </summary>
    Public Property LastSaved() As Double
        Get
            Return CDbl(GetVariable(eVarNameFlags.LastSaved))
        End Get

        Set(ByVal value As Double)
            SetVariable(eVarNameFlags.LastSaved, value)
        End Set
    End Property

#End Region ' Variable via dot(.) operator

#Region " Status Flags via dot(.) operator"

    Public Property DescriptionStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Description)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Description, value)
        End Set

    End Property

#End Region ' Status Flags via dot(.) operator

End Class
