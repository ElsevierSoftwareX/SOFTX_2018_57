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
Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic

#End Region ' Imports

<HideModuleNameAttribute()>
Public Module Extensions

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Extension method; fill an array with a given value.
    ''' </summary>
    ''' <typeparam name="T">Value type.</typeparam>
    ''' <param name="arr">The array to fill.</param>
    ''' <param name="value">The value to fill the array with.</param>
    ''' -----------------------------------------------------------------------
    <Extension()>
    Public Sub Fill(Of T)(ByRef arr As T(), ByVal value As T)
        If (arr Is Nothing) Then Return
        For i As Integer = 0 To arr.Length - 1
            arr(i) = value
        Next
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Extension method; fill an array with a given value.
    ''' </summary>
    ''' <typeparam name="T">Value type.</typeparam>
    ''' <param name="arr">The array to fill.</param>
    ''' <param name="value">The value to fill the array with.</param>
    ''' -----------------------------------------------------------------------
    <Extension()>
    Public Sub Fill(Of T)(ByRef arr As T(,), ByVal value As T)
        If (arr Is Nothing) Then Return
        For i As Integer = 0 To arr.GetUpperBound(0)
            For j As Integer = 0 To arr.GetUpperBound(1)
                arr(i, j) = value
            Next j
        Next i
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Extension method; fill an array with a given value.
    ''' </summary>
    ''' <typeparam name="T">Value type.</typeparam>
    ''' <param name="arr">The array to fill.</param>
    ''' <param name="value">The value to fill the array with.</param>
    ''' -----------------------------------------------------------------------
    <Extension()>
    Public Sub Fill(Of T)(ByRef arr As T(,,), ByVal value As T)
        If (arr Is Nothing) Then Return
        For i As Integer = 0 To arr.GetUpperBound(0)
            For j As Integer = 0 To arr.GetUpperBound(1)
                For k As Integer = 0 To arr.GetUpperBound(2)
                    arr(i, j, k) = value
                Next k
            Next j
        Next i
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Extension method; extract a sub-array.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="values"></param>
    ''' <param name="iStart"></param>
    ''' <param name="iEnd"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    <Extension()>
    Public Function SubArray(Of T)(ByVal values() As T, ByVal iStart As Integer, ByVal iEnd As Integer) As T()
        Dim n As Integer = iEnd - iStart + 1
        Dim result(n - 1) As T
        Array.Copy(values, iStart, result, 0, n)
        Return result
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Extension method; shuffles the specified list.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list">The list to shuffle.</param>
    ''' -----------------------------------------------------------------------
    <Extension()>
    Public Sub Shuffle(Of T)(list As IList(Of T))
        Dim r As Random = New Random()
        For i As Integer = 0 To list.Count - 1
            Dim index As Integer = r.Next(i, list.Count)
            If i <> index Then
                Dim temp As T = list(i)
                list(i) = list(index)
                list(index) = temp
            End If
        Next
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Extension method; checks if two arrays are equal.
    ''' </summary>
    ''' <typeparam name="T">Value type.</typeparam>
    ''' <param name="arr">The array to compare.</param>
    ''' <param name="arr2">The other array to compare to.</param>
    ''' -----------------------------------------------------------------------
    <Extension()>
    Public Function EqualsArray(Of T)(ByRef arr As T(), ByVal arr2 As T()) As Boolean
        If (arr Is Nothing) And (arr2 Is Nothing) Then Return True
        If (arr Is Nothing) Or (arr2 Is Nothing) Then Return False
        If (arr.Length <> arr2.Length) Then Return False
        For i As Integer = 0 To arr.Length - 1
            If (Array.IndexOf(arr2, arr(i)) = -1) Then Return False
        Next
        Return True
    End Function

    <Extension()>
    Public Function Approximates(s1 As Single, s2 As Single, Optional sThreshold As Single = 0.00001) As Boolean
        Return EwEUtils.Utilities.cNumberUtils.Approximates(s1, s2, sThreshold)
    End Function

    <Extension()>
    Public Function Approximates(s1 As Double, s2 As Double, Optional sThreshold As Single = 0.00001) As Boolean
        Return EwEUtils.Utilities.cNumberUtils.Approximates(s1, s2, sThreshold)
    End Function

End Module

