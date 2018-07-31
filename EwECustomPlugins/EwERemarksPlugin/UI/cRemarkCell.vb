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

Imports ScientificInterfaceShared.Controls.EwEGrid
Imports ScientificInterfaceShared.Properties
Imports SourceGrid2.VisualModels

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Cell that provides edit capabilities of the remark text of a <see cref="cProperty"/>, 
''' and refreshes its content when the remark is changed externally.
''' </summary>
''' ---------------------------------------------------------------------------
Friend Class cRemarkCell
    Inherits EwECell

#Region " Private vars "

    Private Shared m_vmRemarks As New ScientificInterfaceShared.Controls.EwEGrid.cEwECellVisualizer(Drawing.ContentAlignment.MiddleLeft)

    Private m_prop As cProperty
    Private m_bInUpdate As Boolean = False

#End Region ' Private vars

#Region " Construction and destruction "

    Public Sub New(ByVal prop As cProperty)
        MyBase.New(prop.GetRemark(), GetType(String))

        Me.VisualModel = m_vmRemarks
        Me.m_prop = prop
        AddHandler Me.m_prop.PropertyChanged, AddressOf OnPropertyChanged

    End Sub

    Public Overrides Sub Dispose()

        If (Me.m_prop IsNot Nothing) Then
            RemoveHandler Me.m_prop.PropertyChanged, AddressOf OnPropertyChanged
            Me.m_prop = Nothing
        End If
        MyBase.Dispose()

    End Sub

#End Region ' Construction and destruction

#Region " Monitoring "

    Private Sub OnPropertyChanged(ByVal prop As cProperty, ct As cProperty.eChangeFlags)
        If Me.m_bInUpdate Then Return
        ' Is a remark change?
        If ((ct And cProperty.eChangeFlags.Remarks) > 0) Then
            ' #Yes: update remark
            Me.Value = prop.GetRemark()
        End If
    End Sub

#End Region ' Monitoring

#Region " Overrides "

    Public Overrides Sub OnEditEnded(e As SourceGrid2.PositionCancelEventArgs)
        MyBase.OnEditEnded(e)
        If e.Cancel = False Then
            Me.m_bInUpdate = True
            Me.m_prop.SetRemark(CStr(Me.Value))
            Me.m_bInUpdate = False
        End If
    End Sub

    ''' <summary>
    ''' For quick editor access
    ''' </summary>
    ''' <param name="p_Position"></param>
    ''' <param name="p_Value"></param>
    Public Overrides Sub SetValue(p_Position As SourceGrid2.Position, p_Value As Object)
        MyBase.SetValue(p_Position, p_Value)
        Me.m_bInUpdate = True
        Me.m_prop.SetRemark(CStr(Me.Value))
        Me.m_bInUpdate = False
    End Sub

#End Region ' Overrides

End Class
