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
Imports EwEUtils.Core

#End Region ' Imports 

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer editor that supports selections of groups.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerEditorGroup
        Inherits cLayerEditorRange
        Implements IGroupFilter

#Region " Construction "

        Public Sub New()
            Me.New(GetType(ucLayerEditorGroup))
        End Sub

        Public Sub New(ByVal t As Type)
            MyBase.New(t)
            Me.CellValue = 1
        End Sub

#End Region ' Construction

#Region " Public interfaces "

        Public Event OnFilterChanged(sender As IContentFilter) Implements IGroupFilter.FilterChanged

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the index of the Ecopath group to filter by.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Group() As Integer _
            Implements IGroupFilter.Group
            Get
                Dim layerCore As cDisplayLayerRasterBundle = DirectCast(Me.Layer, cDisplayLayerRasterBundle)
                Return layerCore.iLayer
            End Get
            Set(ByVal value As Integer)
                Dim layerCore As cDisplayLayerRasterBundle = DirectCast(Me.Layer, cDisplayLayerRasterBundle)
                ' Will Group index change?
                If (value <> layerCore.iLayer) Then
                    ' #Yes: update Group index in the underlying Ecospace layer
                    layerCore.iLayer = value
                    ' Force map update
                    Me.Layer.Update(cDisplayLayer.eChangeFlags.Map, False)

                    Try
                        RaiseEvent OnFilterChanged(Me)
                    Catch ex As Exception
                        ' NOP
                    End Try
                End If
            End Set
        End Property

#End Region ' Public interfaces

    End Class

End Namespace