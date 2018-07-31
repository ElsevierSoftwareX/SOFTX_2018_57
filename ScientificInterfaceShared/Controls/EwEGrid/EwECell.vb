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

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' A standard EwE grid cell for static values.
    ''' </summary>
    ''' -------------------------------------------------------------------
    <CLSCompliant(False)> _
    Public Class EwECell
        : Inherits EwECellBase

#Region " Construction "

        Public Sub New(ByVal objVal As Object, ByVal t As Type, _
                       Optional ByVal style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK)
            MyBase.New(objVal, t)
            ' Set value
            If objVal IsNot Nothing Then Me.Value = objVal
            ' Set style
            Me.Style = style
        End Sub

        Public Sub New(ByVal objVal As Object, Optional ByVal style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK)
            Me.New(objVal, objVal.GetType(), style)
        End Sub

        Public Overrides Sub Dispose()
            ' JS 13Dec10: Memory leaks were discovered on tooltips. Perhaps explicitly 
            '             clearing the grid tooltip text wil fix this.
            Me.ToolTipText = ""
            MyBase.Dispose()
        End Sub

#End Region ' Construction 

#Region " Data "

        ''' -------------------------------------------------------------------
        ''' <summary>Locally maintained value.</summary>
        ''' -------------------------------------------------------------------
        Private m_objValue As Object = Nothing

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Commonly called in response to end edit.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub SetValue(ByVal p_Position As SourceGrid2.Position, ByVal p_Value As Object)
            ' JS Jun 2011: Override this method to allow behaviour models to intercept a cell edit.
            For Each bm As BehaviorModels.IBehaviorModel In Me.Behaviors
                Dim args As New PositionCancelEventArgs(p_Position, Me)
                Try
                    bm.OnEditEnded(args)
                Catch ex As Exception
                End Try
                If args.Cancel Then Return
            Next
            Me.Value = p_Value
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the locally maintained value.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Property Value() As Object
            Get
                Return Me.m_objValue
            End Get
            Set(ByVal objValue As Object)
                If Not Object.Equals(objValue, Me.m_objValue) Then
                    Me.m_objValue = objValue
                    Dim pos As New Position(Me.Row, Me.Column)
                    For Each bh As SourceGrid2.BehaviorModels.IBehaviorModel In Me.Behaviors
                        Try
                            bh.OnValueChanged(New SourceGrid2.PositionEventArgs(pos, Me))
                        Catch ex As Exception
                        End Try
                    Next
                End If
            End Set
        End Property

#End Region ' Data

    End Class

End Namespace
