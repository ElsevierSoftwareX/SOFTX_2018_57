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
Imports EwECore
Imports EwECore.Style
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Supported property sort modes.
''' </summary>
''' ---------------------------------------------------------------------------
Friend Enum ePropertySortOrderTypes As Integer
    ''' <summary>Sort by <see cref="cProperty.Source"/>.</summary>
    Source
    ''' <summary>Sort by <see cref="cProperty.SourceSec"/>.</summary>
    SourceSec
    ''' <summary>Sort by <see cref="cProperty.VarName"/>.</summary>
    VarName
End Enum

''' ---------------------------------------------------------------------------
''' <summary>
''' Sort class for cProperty instances.
''' </summary>
''' ---------------------------------------------------------------------------
Friend Class cPropertySorter
    Implements IComparer(Of cProperty)

#Region " Private vars "

    Private m_sortorder As ePropertySortOrderTypes = ePropertySortOrderTypes.Source
    Private m_fmt As New cVarnameTypeFormatter()

#End Region ' Private vars

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="sortorder">The <see cref="ePropertySortOrderTypes"/> to sort by.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal sortorder As ePropertySortOrderTypes)
        Me.m_sortorder = sortorder
    End Sub

#End Region ' Constructor

#Region " IComparer implementation "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the comparison of two <see cref="cProperty"/> instances.
    ''' </summary>
    ''' <param name="x">Property x to compare.</param>
    ''' <param name="y">Property y to compare.</param>
    ''' <returns>-1 if x less than y, 0 if x equals y, 1 if x greater than y.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Compare(x As cProperty, y As cProperty) As Integer _
        Implements IComparer(Of cProperty).Compare

        Dim sortBits As ePropertySortOrderTypes() = Nothing
        Dim iBit As Integer = 0
        Dim iResult As Integer = 0

        ' Determine sort order of elements
        Select Case Me.m_sortorder
            Case ePropertySortOrderTypes.Source
                ' When sorted by SOURCE, properties are evaluated in the order prim, sec, var
                sortBits = New ePropertySortOrderTypes() {ePropertySortOrderTypes.Source, ePropertySortOrderTypes.SourceSec, ePropertySortOrderTypes.VarName}
            Case ePropertySortOrderTypes.SourceSec
                ' When sorted by SOURCESEC, properties are evaluated in the order sec, var, prim
                sortBits = New ePropertySortOrderTypes() {ePropertySortOrderTypes.SourceSec, ePropertySortOrderTypes.VarName, ePropertySortOrderTypes.Source}
            Case ePropertySortOrderTypes.VarName
                ' When sorted by VARIABLE, properties are evaluated in the order var, prim, sec
                sortBits = New ePropertySortOrderTypes() {ePropertySortOrderTypes.VarName, ePropertySortOrderTypes.Source, ePropertySortOrderTypes.SourceSec}
        End Select

        ' Sort cascaded
        While iResult = 0 And iBit < sortBits.Length
            iResult = Me.Compare(x, y, sortBits(iBit))
            iBit += 1
        End While

        ' Return result
        Return iResult

    End Function

#End Region ' IComparer implementation

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the comparison of two <see cref="cProperty"/> objects.
    ''' </summary>
    ''' <param name="x"><see cref="cProperty"/> x to compare.</param>
    ''' <param name="y"><see cref="cProperty"/> y to compare.</param>
    ''' <param name="order">The <see cref="ePropertySortOrderTypes">order to sort by</see></param>
    ''' <returns>-1 if x less than y, 0 if x equals y, 1 if x greater than y.</returns>
    ''' -----------------------------------------------------------------------
    Private Function Compare(x As cProperty, y As cProperty, order As ePropertySortOrderTypes) As Integer

        Select Case order

            Case ePropertySortOrderTypes.Source
                ' Compare prim sources
                Return Me.Compare(x.Source, y.Source)

            Case ePropertySortOrderTypes.SourceSec
                ' Compare sec sources
                Return Me.Compare(x.SourceSec, y.SourceSec)

            Case ePropertySortOrderTypes.VarName
                ' Compare variables by name
                Return String.Compare(Me.m_fmt.GetDescriptor(x.VarName), Me.m_fmt.GetDescriptor(y.VarName))

        End Select

        Return 0

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the comparison of two <see cref="cCoreInputOutputBase"/> objects.
    ''' </summary>
    ''' <param name="x"><see cref="cCoreInputOutputBase"/> x to compare.</param>
    ''' <param name="y"><see cref="cCoreInputOutputBase"/> y to compare.</param>
    ''' <returns>-1 if x less than y, 0 if x equals y, 1 if x greater than y.</returns>
    ''' -----------------------------------------------------------------------
    Private Function Compare(ByVal x As cCoreInputOutputBase, ByVal y As cCoreInputOutputBase) As Integer

        ' Handle missing data
        If (x Is Nothing) And (y Is Nothing) Then Return 0
        If (x Is Nothing) Then Return 1
        If (y Is Nothing) Then Return -1

        ' Sort by data type (which is rather blunt)
        If (x.DataType < y.DataType) Then Return -1
        If (x.DataType > y.DataType) Then Return 1

        ' If data types match, sort by index
        If (x.Index < y.Index) Then Return -1
        If (x.Index > y.Index) Then Return 1

        ' If indices match the objects are considered equal
        Return 0

    End Function

#End Region ' Internals

End Class
