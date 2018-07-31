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
Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports ScientificInterfaceShared.Definitions
Imports SourceGrid2.VisualModels

#End Region ' Imports

Namespace Controls.EwEGrid

    <CLSCompliant(False)> _
    Public Class EwEStatusCell
        Inherits EwECellBase

        Private m_strText As String = ""

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Visual model for reflecting 'Original' values.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property DefaultVisualOriginal() As VisualModels.IVisualModel
            Get
                Dim vm As VisualModels.Common = New cEwECellVisualizer()
                vm.ForeColor = Color.FromArgb(255, 0, 0, 0)
                vm.TextAlignment = ContentAlignment.MiddleCenter
                vm.MakeReadOnly()
                Return vm
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Visual model for reflecting 'Added' values.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property DefaultVisualAdd() As VisualModels.IVisualModel
            Get
                Dim vm As VisualModels.Common = New cEwECellVisualizer()
                vm.ForeColor = Color.FromArgb(255, 8, 128, 12)
                vm.TextAlignment = ContentAlignment.MiddleCenter
                vm.MakeReadOnly()
                Return vm
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Visual model for reflecting 'Removed' values.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property DefaultVisualRemove() As VisualModels.IVisualModel
            Get
                Dim vm As VisualModels.Common = New cEwECellVisualizer()
                vm.ForeColor = Color.FromArgb(255, 255, 22, 12)
                vm.TextAlignment = ContentAlignment.MiddleCenter
                vm.MakeReadOnly()
                Return vm
            End Get
        End Property

        Public Sub New(status As eItemStatusTypes)
            MyBase.New(status, GetType(Integer))
            Me.Style = cStyleGuide.eStyleFlags.NotEditable
        End Sub

        Public Overrides Sub SetValue(pos As SourceGrid2.Position, value As Object)
            MyBase.SetValue(pos, value)

            Dim status As eItemStatusTypes = eItemStatusTypes.Invalid
            Dim vm As IVisualModel = DefaultVisualOriginal
            Dim strText As String = ""

            If (TypeOf value Is eItemStatusTypes) Then
                status = DirectCast(value, eItemStatusTypes)
            End If
            Select Case status
                Case eItemStatusTypes.Original
                    ' NOP
                Case eItemStatusTypes.Added
                    strText = My.Resources.GENERIC_VALUE_CREATE_PENDING
                    vm = DefaultVisualAdd
                Case eItemStatusTypes.Removed
                    strText = My.Resources.GENERIC_VALUE_DELETE_PENDING
                    vm = DefaultVisualRemove
                Case eItemStatusTypes.Invalid
                    ' NOP
            End Select
            Me.m_strText = strText
            Me.VisualModel = vm
            MyBase.SetValue(pos, value)
        End Sub

        Public Overrides ReadOnly Property DisplayText As String
            Get
                Return Me.m_strText
            End Get
        End Property

    End Class

End Namespace
