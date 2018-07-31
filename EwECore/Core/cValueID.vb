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
Imports EwEUtils.Core

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' <para>Helper class; assists in uniquely identifying EwE values.</para>
''' </summary>
''' -----------------------------------------------------------------------
Public Class cValueID

#Region " Private vars "

    Private m_dtPrim As eDataTypes = eDataTypes.NotSet
    Private m_iDBIDPrim As Integer = cCore.NULL_VALUE
    Private m_strVarName As String = ""
    Private m_varname As eVarNameFlags = eVarNameFlags.NotSet
    Private m_dtSec As eDataTypes = eDataTypes.NotSet
    Private m_iDBIDSec As Integer = cCore.NULL_VALUE
    Private m_strCached As String = ""

    Private Shared s_cni As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()

#End Region ' Private vars

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="dtPrim"></param>
    ''' <param name="iDBIDPrim"></param>
    ''' <param name="strVarName"></param>
    ''' <param name="dtSec"></param>
    ''' <param name="iDBIDSec"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal dtPrim As eDataTypes,
                   ByVal iDBIDPrim As Integer,
                   ByVal strVarName As String,
                   Optional ByVal dtSec As eDataTypes = eDataTypes.NotSet,
                   Optional ByVal iDBIDSec As Integer = -1)

        ' Pre
        If (dtSec <> eDataTypes.NotSet) Then
            Debug.Assert(iDBIDPrim > 0, "Unique indexed object must have a primary ID")
        End If

        Me.m_dtPrim = dtPrim
        Me.m_iDBIDPrim = iDBIDPrim
        Me.m_strVarName = strVarName
        Me.m_dtSec = dtSec
        Me.m_iDBIDSec = iDBIDSec

    End Sub

    Public Sub New(ByVal dtPrim As eDataTypes,
                   ByVal iDBIDPrim As Integer,
                   ByVal varname As eVarNameFlags,
                   Optional ByVal dtSec As eDataTypes = eDataTypes.NotSet,
                   Optional ByVal iDBIDSec As Integer = -1)

        Me.New(dtPrim, iDBIDPrim, s_cni.GetVarName(varname), dtSec, iDBIDSec)

    End Sub

    Public Sub New(ByVal obj As ICoreInterface,
                   ByVal strVarName As String,
                   Optional ByVal objSec As ICoreInterface = Nothing)

        MyBase.New()

        ' Pre
        If (obj.Index > 0) Then
            Debug.Assert(obj.DBID > 0, "Unique indexed object must have a primary ID")
        End If

        Me.m_dtPrim = obj.DataType
        Me.m_iDBIDPrim = obj.DBID
        Me.m_strVarName = strVarName

        If (objSec IsNot Nothing) Then

            Me.m_dtSec = objSec.DataType
            Me.m_iDBIDSec = objSec.DBID

        End If
    End Sub

    Public Sub New(ByVal obj As ICoreInterface,
                   ByVal varname As eVarNameFlags,
                   Optional ByVal objSec As ICoreInterface = Nothing)

        Me.New(obj, s_cni.GetVarName(varname), objSec)

    End Sub

    Private Sub New()
    End Sub

    Public Overrides Function ToString() As String
        If Me.m_strCached.Length = 0 Then
            If Me.m_dtSec = eDataTypes.NotSet Then
                Me.m_strCached = s_cni.GetDataTypeName(Me.m_dtPrim) & ":" & Me.m_iDBIDPrim & ":" & Me.m_strVarName
            Else
                Me.m_strCached = s_cni.GetDataTypeName(Me.m_dtPrim) & ":" & Me.m_iDBIDPrim & ":" & Me.m_strVarName & ":" & s_cni.GetDataTypeName(Me.m_dtSec) & ":" & Me.m_iDBIDSec
            End If
        End If
        Return Me.m_strCached
    End Function

    Public Property DataTypePrim() As eDataTypes
        Get
            Return Me.m_dtPrim
        End Get
        Friend Set(ByVal dt As eDataTypes)
            Me.m_dtPrim = dt
            Me.m_strCached = ""
        End Set
    End Property

    Public Property DataTypeSec() As eDataTypes
        Get
            Return Me.m_dtSec
        End Get
        Friend Set(ByVal dt As eDataTypes)
            Me.m_dtSec = dt
            Me.m_strCached = ""
        End Set
    End Property

    Public Property DBIDPrim() As Integer
        Get
            Return Me.m_iDBIDPrim
        End Get
        Friend Set(ByVal iDBID As Integer)
            Me.m_iDBIDPrim = iDBID
            Me.m_strCached = ""
        End Set
    End Property

    Public Property DBIDSec() As Integer
        Get
            Return Me.m_iDBIDSec
        End Get
        Friend Set(ByVal iDBID As Integer)
            Me.m_iDBIDSec = iDBID
            Me.m_strCached = ""
        End Set
    End Property

    Public Property VarNameText() As String
        Get
            Return Me.m_strVarName
        End Get
        Friend Set(ByVal strVarName As String)
            Me.m_strVarName = strVarName
            Me.m_strCached = ""
        End Set
    End Property

    Public Property VarName() As eVarNameFlags
        Get
            Return Me.m_varname
        End Get
        Friend Set(ByVal vn As eVarNameFlags)
            Me.m_varname = vn
            Me.m_strCached = ""
        End Set
    End Property

    Public Shared Function FromString(ByVal strAuxKey As String) As cValueID

        Dim asBits() As String = strAuxKey.Split(":"c)
        Dim key As New cValueID()
        Dim cni As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance

        For i As Integer = 0 To asBits.Length - 1
            Select Case i
                Case 0 : key.m_dtPrim = s_cni.GetDataType(asBits(0))
                Case 1 : key.m_iDBIDPrim = CInt(asBits(1))
                Case 2 : key.m_strVarName = asBits(2) : key.m_varname = cni.GetVarName(asBits(2))
                Case 3 : key.m_dtSec = s_cni.GetDataType(asBits(3))
                Case 4 : key.m_iDBIDSec = CInt(asBits(4))
            End Select
        Next

        Return key

    End Function

    Public Shared Function FromValueID(ByVal val As cValueID) As cValueID
        Return New cValueID(val.DataTypePrim, val.DBIDPrim, val.VarName, val.DataTypeSec, val.DBIDSec)
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Generates an ID for a core data type and ID
    ''' </summary>
    ''' <param name="dataType">The <see cref="eDataTypes">Core data type</see> representing the object to generate the ID for</param>
    ''' <param name="nID">The unique ID of the object to generate the ID for</param>
    ''' <returns>A computer ID</returns>
    ''' -------------------------------------------------------------------
    Public Shared Function GetDataTypeID(ByVal dataType As eDataTypes, ByVal nID As Integer) As String
        ' JS 06jul07: profiled, m3 runs the the fastest
        'Return String.Concat(cCoreEnumNamesIndex.GetInstance().GetDataTypeName(dataType), CChar("_"), CStr(nID)) ' m1
        'Return cCoreEnumNamesIndex.GetInstance().GetDataTypeName(dataType) & CChar("_") & CStr(nID)              ' m2
        Return cCoreEnumNamesIndex.GetInstance().GetDataTypeName(dataType) & "_" & nID                            ' m3
    End Function

End Class

