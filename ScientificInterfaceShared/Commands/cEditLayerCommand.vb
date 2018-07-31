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
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Definitions

#End Region ' Imports

Namespace Commands

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Command to invoke the 'Edit Ecospace Layer' interface
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cEditLayerCommand
        Inherits cCommand

        Private m_layer As cDisplayLayerRaster = Nothing
        Private m_layerDepth As cDisplayLayerRaster = Nothing
        Private m_edittype As eLayerEditTypes

        ''' <summary>Static name for this command.</summary>
        Public Shared cCOMMAND_NAME As String = "~EditLayer"

        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, cEditLayerCommand.cCOMMAND_NAME)
        End Sub

        ''' ---------------------------------------------------------------------------
        ''' <inheritdocs cref="cCommand.Invoke"/>
        ''' <param name="layer">The layer to edit.</param>
        ''' <param name="layerDepth">Depth reference layer.</param>
        ''' <param name="edittype">Type of <see cref="eLayerEditTypes">edit operation</see>.</param>
        ''' ---------------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal layer As cDisplayLayerRaster, ByVal layerDepth As cDisplayLayerRaster, ByVal edittype As eLayerEditTypes)
            Me.m_layer = layer
            Me.m_layerDepth = layerDepth
            Me.m_edittype = edittype
            MyBase.Invoke()
        End Sub

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Get the layer to edit.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Public ReadOnly Property Layer() As cDisplayLayerRaster
            Get
                Return Me.m_layer
            End Get
        End Property

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Get the reference depth layer to help with the edit operation.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Public ReadOnly Property LayerDepth() As cDisplayLayerRaster
            Get
                Return Me.m_layer
            End Get
        End Property

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eLayerEditTypes">edit operation</see> to execute upon the
        ''' selected <see cref="Layer"/>.
        ''' </summary>
        ''' ---------------------------------------------------------------------------
        Public ReadOnly Property EditType() As eLayerEditTypes
            Get
                Return Me.m_edittype
            End Get
        End Property

    End Class

End Namespace ' Commands
