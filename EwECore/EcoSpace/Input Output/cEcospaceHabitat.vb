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

Public Class cEcospaceHabitat
    Inherits cCoreInputOutputBase

#Region "Constructor"

    Sub New(ByRef theCore As cCore, ByVal DBID As Integer)
        MyBase.New(theCore)

        Me.DBID = DBID
        m_dataType = eDataTypes.EcospaceHabitat
        m_coreComponent = eCoreComponentType.EcoSpace

        Dim val As cValue

        Try

            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            ' HabAreaProportion
            val = New cValue(New Single, eVarNameFlags.HabAreaProportion, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ResetStatusFlags()

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcospaceHabitat.")
            cLog.Write(Me.ToString & ".New(nGroups) Error creating new cEcospaceHabitat. Error: " & ex.Message)
        End Try

    End Sub

#End Region

#Region "Properties by dot (.) operator "

    Public Property HabAreaProportion() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.HabAreaProportion))
        End Get

        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.HabAreaProportion, value)
        End Set
    End Property

#End Region

#Region "Status by dot (.) operator"

    Public Property HabAreaProportionStatus() As eStatusFlags
        Get
            Return GetStatus(eVarNameFlags.HabAreaProportion)
        End Get

        Friend Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.HabAreaProportion, value)
        End Set
    End Property

#End Region

End Class
