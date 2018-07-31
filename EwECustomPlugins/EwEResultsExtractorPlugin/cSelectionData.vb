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

Public Class cSelectionData

    Private m_UnSelected As List(Of String)
    Private m_Selected As List(Of cCreatedObjects)
    Private m_SelectionType As String
    Private m_ParentChildsSelected As cCreatedObjects

    Public Sub New(ByVal SelectionType As String, ByVal Input() As String)
        m_SelectionType = SelectionType
        m_UnSelected = New List(Of String)
        m_Selected = New List(Of cCreatedObjects)
        For Each x In Input 'Load unselected with an inputted array of strings
            m_UnSelected.Add(x)
        Next
    End Sub

    Public Sub Add(ByVal i As String)
        m_Selected.Add(New cCreatedObjects(i))
        m_UnSelected.Remove(i)
    End Sub

    Public Sub Remove(ByVal i As String)
        Dim p As Integer = 0
        m_UnSelected.Add(i)
        While p < m_Selected.Count
            If m_Selected(p).ParentName = i Then
                m_Selected.RemoveAt(p)
            End If
            p += 1
        End While

    End Sub

    Public Sub RemoveAll()
        For i = 0 To m_Selected.Count - 1
            Me.Remove(m_Selected(0).ParentName)
        Next
    End Sub

    Public ReadOnly Property GetSelectedItem(ByVal i As Integer) As cCreatedObjects
        Get
            Return m_Selected(i)
        End Get
    End Property

    Public ReadOnly Property UnSelectedNames() As List(Of String)
        Get
            Return m_UnSelected
        End Get
    End Property

    Public ReadOnly Property SelectedNames() As List(Of String)
        Get
            Dim ListOfNames As New List(Of String)
            For Each i In m_Selected
                ListOfNames.Add(i.ParentName)
            Next
            Return ListOfNames
        End Get
    End Property

    Public ReadOnly Property CountSelected() As Integer
        Get
            Return m_Selected.Count
        End Get
    End Property

    Public ReadOnly Property GetSelected() As List(Of cCreatedObjects)
        Get
            Return m_Selected
        End Get
    End Property

    Public ReadOnly Property GetParentChild(ByVal i As String) As cCreatedObjects
        Get
            For Each x In m_Selected
                If x.ParentName = i Then
                    Return x
                End If
            Next
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property GetAttached(ByVal SelectedItem As String) As List(Of String)
        Get
            For Each i In m_Selected
                If i.ParentName = SelectedItem Then
                    Return i.ChildNames
                End If
            Next
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property CountSelectedChild() As Integer
        Get
            Dim Count As Integer = 0
            For Each i In m_Selected
                Count += i.CountChild
            Next
            Return Count
        End Get
    End Property

    Public ReadOnly Property GetFocus() As cCreatedObjects
        Get
            Return m_ParentChildsSelected
        End Get
    End Property

    Public WriteOnly Property SetFocus() As String
        Set(ByVal value As String)
            If value = Nothing Then
                m_ParentChildsSelected = Nothing
                Exit Property
            End If
            For Each i In m_Selected
                If i.ParentName = value Then
                    m_ParentChildsSelected = i
                End If
            Next
        End Set
    End Property


End Class