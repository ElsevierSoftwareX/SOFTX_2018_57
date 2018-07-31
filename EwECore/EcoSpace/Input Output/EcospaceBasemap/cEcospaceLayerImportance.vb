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
Imports EwEUtils.Utilities

#End Region ' Imports 

''' <summary>
''' Layer providing access to Ecospace importance data.
''' </summary>
Public Class cEcospaceLayerImportance
    Inherits cEcospaceLayerSingle

#Region " Constructor "

    Sub New(ByRef theCore As cCore, ByVal idBID As Integer, ByRef manager As cEcospaceBasemap, ByVal iIndex As Integer)

        ' Importance layers are user-defined, and will have user-provided names
        MyBase.New(theCore, manager, "", eVarNameFlags.LayerImportance, iIndex)

        Dim val As cValue
        Dim desc As Char()

        Me.AllowValidation = False

        Try
            m_dataType = eDataTypes.EcospaceLayerImportance
            m_coreComponent = eCoreComponentType.EcoSpace

            Me.m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            ' Weight
            val = New cValue(0, eVarNameFlags.ImportanceWeight, eStatusFlags.Null, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' Description
            val = New cValue(New String(desc), eVarNameFlags.Description, eStatusFlags.NotEditable Or eStatusFlags.Null, eValueTypes.Str)
            m_values.Add(val.varName, val)

            'set status flags to default values
            ResetStatusFlags()

            Me.AllowValidation = True

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cEcospaceBasemap.")
            cLog.Write(Me.ToString & ".New(..) Error creating new cEcospaceBasemap. Error: " & ex.Message)
        End Try

    End Sub

#End Region ' Constructor

#Region " Overrides "

    Protected Overrides Function DefaultName() As String
        Return cStringUtils.Localize(My.Resources.CoreDefaults.CORE_DEFAULT_IMPORTANCE, Me.Index)
    End Function

    Public Overrides Property Cell(ByVal iRow As Integer, ByVal iCol As Integer, Optional iIndexSec As Integer = cCore.NULL_VALUE) As Object
        Get
            If Me.ValidateCellPosition(iRow, iCol) Then
                Return DirectCast(Me.Data, Single()(,))(Me.Index)(iRow, iCol)
            End If
            Return cCore.NULL_VALUE
        End Get
        Set(ByVal value As Object)
            If Me.ValidateCellPosition(iRow, iCol) Then
                DirectCast(Me.Data, Single()(,))(Me.Index)(iRow, iCol) = CSng(value)
            End If
        End Set
    End Property

#End Region ' Overrides

#Region " Properties by dot (.) operator "

    Public Property Weight() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.ImportanceWeight))
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.ImportanceWeight, value)
        End Set
    End Property

#End Region ' Properties by dot (.) operator

End Class
