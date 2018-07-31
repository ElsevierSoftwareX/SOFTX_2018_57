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
Imports ScientificInterfaceShared.Properties

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class, monitors available <see cref="cProperty"/> instances
''' for the occurrence of Remarks. The monitor fires <see cref="cRemarkMonitor.OnRemarksListChanged">change events</see>
''' when the list of properties wuth remarks changes.
''' </summary>
''' ---------------------------------------------------------------------------
Friend Class cRemarkMonitor
    Implements IDisposable

#Region " Internals "

    Private m_pm As cPropertyManager = Nothing
    Private m_dtProps As New HashSet(Of cProperty)
    Private m_bBatch As Boolean = False

#End Region ' Internals

#Region " Construction / destruction "

    Public Sub New(pm As cPropertyManager)

        Me.m_pm = pm

        Me.m_bBatch = True
        For Each prop As cProperty In Me.m_pm.GetProperties
            OnPropertyAdded(prop)
        Next
        Me.m_bBatch = False

        AddHandler Me.m_pm.OnPropertyRemoved, AddressOf OnPropertyRemoved
        AddHandler Me.m_pm.OnPropertyAdded, AddressOf OnPropertyAdded

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose

        If (Me.m_pm IsNot Nothing) Then

            RemoveHandler Me.m_pm.OnPropertyAdded, AddressOf OnPropertyAdded
            RemoveHandler Me.m_pm.OnPropertyRemoved, AddressOf OnPropertyRemoved

            Me.m_bBatch = True
            For Each prop As cProperty In Me.m_pm.GetProperties
                OnPropertyRemoved(prop)
            Next
            Me.m_bBatch = False

            Me.m_pm = Nothing

        End If

        GC.SuppressFinalize(Me)

    End Sub

#End Region ' Construction / destruction

#Region " Public interfaces "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Notification event, fired when the list of properties that have remarks
    ''' changes.
    ''' </summary>
    ''' <param name="sender">The monitor that fired the event.</param>
    ''' -----------------------------------------------------------------------
    Public Event OnRemarksListChanged(ByRef sender As cRemarkMonitor)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns an array of all properties with remarks.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function Remarks() As cProperty()

        Dim lProps As New List(Of cProperty)
        lProps.AddRange(Me.m_dtProps)

#If DEBUG Then
        For Each prop In lProps
            Debug.Assert(Not prop.IsDisposed)
        Next
#End If
        Return lProps.ToArray()

    End Function

#End Region ' Public interfaces

#Region " Internals "

    Private Sub OnPropertyAdded(ByVal prop As cProperty)

        ' Add to internal admin, if applicable
        If prop.HasRemark() Then
            Me.m_dtProps.Add(prop)
            Me.FireListChangedEvent()
        End If

        ' Start monitoring for remark text changes
        AddHandler prop.PropertyChanged, AddressOf OnPropertyChanged

    End Sub

    Private Sub OnPropertyRemoved(ByVal prop As cProperty)

        ' Stop monitoring for remark text changes
        RemoveHandler prop.PropertyChanged, AddressOf OnPropertyChanged

        ' Remove from internal admin, if applicable
        If (Me.m_dtProps.Contains(prop)) Then
            Me.m_dtProps.Remove(prop)
            Me.FireListChangedEvent()
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, invoked when a property reports a change.
    ''' </summary>
    ''' <param name="prop">The property that changed.</param>
    ''' <param name="ct">The aspect of the property that changed.</param>
    ''' -----------------------------------------------------------------------
    Private Sub OnPropertyChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)

        Dim bListChanged As Boolean = False

        If (ct And cProperty.eChangeFlags.Remarks) > 0 Then
            Dim bHasRemark As Boolean = prop.HasRemark()
            If bHasRemark Then
                If Not Me.m_dtProps.Contains(prop) Then
                    Me.m_dtProps.Add(prop)
                    bListChanged = True
                End If
            Else
                If Me.m_dtProps.Contains(prop) Then
                    Me.m_dtProps.Remove(prop)
                    bListChanged = True
                End If
            End If
        End If

        If bListChanged Then
            Me.FireListChangedEvent()
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Let the world know that the list of properties with remarks was modified.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub FireListChangedEvent()

        ' Shut up when in batch mode
        If Me.m_bBatch Then Return

        Try
            RaiseEvent OnRemarksListChanged(Me)
        Catch ex As Exception
            ' Plop
        End Try

    End Sub

#End Region ' Internals

End Class
