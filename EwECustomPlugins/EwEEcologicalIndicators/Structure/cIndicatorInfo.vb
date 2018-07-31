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
Imports System.Reflection
Imports EwECore
Imports EwEUtils.Core
Imports EwECore.Style

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' Class providing name, description and access to computed values for a single indicator.
''' </summary>
''' -----------------------------------------------------------------------
Public Class cIndicatorInfo

#Region " Private fields "

    ''' <summary>The name of the indicator</summary>
    Private m_strName As String = ""
    ''' <summary>The description of the indicator</summary>
    Private m_strDescription As String = ""
    ''' <summary>The description of the unit of the indicator (for display on axis)</summary>
    Private m_strValueDescription As String = ""
    ''' <summary>The units of the indicator</summary>
    Private m_strUnit As String = ""
    ''' <summary>The function name of the indicator in the <see cref="cIndicators">indicator</see></summary>
    Private m_strFunctionName As String = ""

#End Region ' Private fields

#Region " Construction "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Create a new instance.
    ''' </summary>
    ''' <param name="strName">Name to assign to the indicator.</param>
    ''' <param name="strFunctionName">The name of function for the indicator as exposed by the computed <see cref="cIndicators">indicator</see>.</param>
    ''' <param name="strDescription">Description to assign to the indicator.</param>
    ''' <param name="strValueDescription">Description of the value of indicator (biomass, catch, etc).</param>
    ''' <param name="strUnits">EwE <see cref="cUnits">units</see> to show for the indicator.</param>
    ''' -------------------------------------------------------------------
    Public Sub New(ByVal strFunctionName As String,
                   ByVal strName As String,
                   ByVal strDescription As String,
                   ByVal strValueDescription As String,
                   ByVal strUnits As String)

        Me.m_strName = strName
        Me.m_strFunctionName = strFunctionName
        Me.m_strValueDescription = strValueDescription
        Me.m_strUnit = strUnits
        Me.m_strDescription = strDescription

    End Sub

#End Region ' Construction

#Region " Public access "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get the name of the indicator.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public ReadOnly Property Name As String
        Get
            Return Me.m_strName
        End Get
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get the description of the indicator.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public ReadOnly Property Description As String
        Get
            Return Me.m_strDescription
        End Get
    End Property

    Public ReadOnly Property ValueDescription As String
        Get
            Return Me.m_strValueDescription
        End Get
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get the units of the indicator.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public ReadOnly Property Units As String
        Get
            Return Me.m_strUnit
        End Get
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get the value for the indicator from a computed <see cref="cIndicators">indicator</see>.
    ''' </summary>
    ''' <param name="indicators">The computed <see cref="cIndicators">indicator</see> to extract information from.</param>
    ''' <returns>A value, or <see cref="cCore.NULL_VALUE"/> if the property was not found.</returns>
    ''' -------------------------------------------------------------------
    Public Function GetValue(ByVal indicators As cIndicators) As Single

        If (indicators Is Nothing) Then Return 0

        ' Try to get property info from the indicator
        Dim mi As MethodInfo = GetType(cIndicators).GetMethod(Me.m_strFunctionName)
        ' Prepare default value
        Dim sValue As Single = cCore.NULL_VALUE
        ' Was property found?
        If (mi IsNot Nothing) Then
            ' #Yes: try to extract the value as a SINGLE precision number
            Try
                sValue = CSng(mi.Invoke(indicators, New Object() {}))
            Catch ex As Exception
                ' A failure is due to a programming error
                Debug.Assert(False, "Property " & Me.m_strFunctionName & " cannot be converted to Single")
            End Try
        End If
        ' Return value
        Return sValue

    End Function

#End Region ' Public access

End Class
