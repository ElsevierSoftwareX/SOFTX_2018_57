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
Imports System.Diagnostics
Imports System.Collections
Imports System.Collections.Generic
Imports System.Reflection
Imports System.ComponentModel

#End Region ' Imports

Namespace Utilities

    ''' ===========================================================================
    ''' <summary>
    ''' Code taken from "Ordering Items in the Property Grid" by
    ''' Paul T (http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=126190)
    ''' url: http://www.codeproject.com/KB/cpp/orderedpropertygrid.aspx
    ''' </summary>
    ''' <remarks>
    ''' Usage:
    ''' 
    ''' [TypeConverter(TypeOf(PropertySorter))]
    ''' [DefaultProperty("Name")]
    ''' Public Class Person
    ''' {
    '''     [cPropertySorter.PropertyOrder(1)}
    '''     Public Property Test
    '''     ..
    ''' }
    ''' </remarks>
    ''' ===========================================================================
    Public Class cPropertySorter
        Inherits ExpandableObjectConverter

#Region " Helper classes "

#Region " PropertyOrderAttribute "

        <AttributeUsage(AttributeTargets.[Property])> _
        Public Class PropertyOrderAttribute
            Inherits Attribute

            ''' <summary>Simple attribute to allow the order of a property to be specified.</summary>
            Private m_iOrder As Integer = 0

            Public Sub New(ByVal iOrder As Integer)
                Me.m_iOrder = iOrder
            End Sub

            Public ReadOnly Property Order() As Integer
                Get
                    Return Me.m_iOrder
                End Get
            End Property
        End Class

#End Region ' PropertyOrderAttribute

#Region " PropertyOrderComparer "

        Private Class PropertyOrderComparer
            Implements IComparable

            Private m_strPropertyName As String = ""
            Private m_strCategory As String = ""
            Private m_strDisplayName As String = ""
            Private m_iOrder As Integer = 0

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="strPropertyName">Property name</param>
            ''' <param name="strCategory">Category attribute</param>
            ''' <param name="strDisplayName">Name attribute</param>
            ''' <param name="iOrder">Order attribute</param>
            ''' ---------------------------------------------------------------
            Public Sub New(ByVal strPropertyName As String, _
                           ByVal strCategory As String, ByVal strDisplayName As String, ByVal iOrder As Integer)
                Me.m_strPropertyName = strPropertyName
                Me.m_strCategory = strCategory
                Me.m_strDisplayName = strDisplayName
                Me.m_iOrder = iOrder
            End Sub

            Public ReadOnly Property PropertyName() As String
                Get
                    Return Me.m_strPropertyName
                End Get
            End Property

            Public ReadOnly Property Category() As String
                Get
                    Return Me.m_strCategory
                End Get
            End Property

            Public ReadOnly Property DisplayName() As String
                Get
                    Return Me.m_strDisplayName
                End Get
            End Property

            Public ReadOnly Property Order() As Integer
                Get
                    Return Me.m_iOrder
                End Get
            End Property

            Public Function CompareTo(ByVal obj As Object) As Integer _
                Implements System.IComparable.CompareTo

                ' Get object to compare to
                Dim cmp As PropertyOrderComparer = DirectCast(obj, PropertyOrderComparer)
                ' Sort by category first
                Dim iSort As Integer = String.Compare(Me.m_strCategory, cmp.Category)

                ' Categories match?
                If iSort = 0 Then
                    ' #Yes: sort by order
                    ' Orders match?
                    If cmp.Order = Me.m_iOrder Then
                        ' #Yes: sort by name 
                        iSort = String.Compare(Me.m_strDisplayName, cmp.DisplayName)
                    Else
                        ' #No: sort by order
                        If cmp.Order > m_iOrder Then
                            iSort = -1
                        Else
                            iSort = 1
                        End If
                    End If
                End If

                Return iSort

            End Function

        End Class

#End Region ' PropertyOrderComparer

#End Region ' Helper classes

        Public Overloads Overrides Function GetPropertiesSupported(ByVal context As ITypeDescriptorContext) As Boolean
            Return True
        End Function

        ''' <summary>
        ''' This override returns a list of properties in order.
        ''' </summary>
        ''' <param name="context"></param>
        ''' <param name="value"></param>
        ''' <param name="attributes"></param>
        ''' <returns></returns>
        Public Overloads Overrides Function GetProperties(ByVal context As ITypeDescriptorContext, ByVal value As Object, ByVal attributes As Attribute()) As PropertyDescriptorCollection

            Dim pdc As PropertyDescriptorCollection = TypeDescriptor.GetProperties(value, attributes)
            Dim attribute As Attribute = Nothing
            Dim poa As PropertyOrderAttribute = Nothing
            Dim strName As String = ""
            Dim alPropsOrdered As New ArrayList()
            Dim lstrNames As New List(Of String)

            For Each pd As PropertyDescriptor In pdc

                ' Get appropriate name
                If Not String.IsNullOrEmpty(pd.DisplayName) Then
                    strName = pd.DisplayName
                Else
                    strName = pd.Name
                End If

                ' Get order attribute, if any
                attribute = pd.Attributes(GetType(PropertyOrderAttribute))
                ' Has an order specifier attribute?
                If attribute IsNot Nothing Then
                    ' #Yes: create an pair object to hold it
                    poa = DirectCast(attribute, PropertyOrderAttribute)
                    alPropsOrdered.Add(New PropertyOrderComparer(pd.Name, pd.Category, strName, poa.Order))
                Else
                    ' #No: create dummy pair object with a default order of 0
                    alPropsOrdered.Add(New PropertyOrderComparer(pd.Name, pd.Category, strName, 0))
                End If
            Next

            ' Perform the actual order using the value PropertyOrderPair classes
            ' implementation of IComparable to sort
            alPropsOrdered.Sort()

            ' Build a string list of the ordered names
            For Each pop As PropertyOrderComparer In alPropsOrdered
                lstrNames.Add(pop.PropertyName)
            Next

            ' Pass in the ordered list for the PropertyDescriptorCollection to sort by
            Return pdc.Sort(lstrNames.ToArray())
        End Function

    End Class

    ''' =======================================================================
    ''' <summary>
    ''' Property conversion utility class.
    ''' </summary>
    ''' =======================================================================
    Public Class cPropertyConverter

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Find a <see cref="PropertyDescriptor">PropertyDescriptor</see> for
        ''' a given <see cref="PropertyInfo">PropertyInfo</see> instance.
        ''' </summary>
        ''' <param name="pi">The property info instance to find a 
        ''' property descriptor for.</param>
        ''' <returns>A <see cref="PropertyDescriptor">PropertyDescriptor</see>
        ''' instance, or nothing if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FindOrigPropertyDescriptor(ByVal pi As PropertyInfo) As PropertyDescriptor
            For Each pd As PropertyDescriptor In TypeDescriptor.GetProperties(pi.DeclaringType)
                If pd.Name.Equals(pi.Name) Then
                    Return pd
                End If
            Next
            Return Nothing
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Find a  for <see cref="PropertyInfo">PropertyInfo</see> a given 
        ''' <see cref="PropertyDescriptor">PropertyDescriptor</see> instance.
        ''' </summary>
        ''' <param name="t">The type to search.</param>
        ''' <param name="pd">The property descriptor instance to find a 
        ''' property descriptor for.</param>
        ''' <returns>A <see cref="PropertyInfo">PropertyInfo</see> instance,
        ''' or nothing if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FindOrigPropertyInfo(ByVal t As Type, ByVal pd As PropertyDescriptor) As PropertyInfo
            For Each pi As PropertyInfo In t.GetProperties()
                If pd.Name.Equals(pi.Name) Then
                    Return pi
                End If
            Next
            Return Nothing
        End Function

    End Class

End Namespace
