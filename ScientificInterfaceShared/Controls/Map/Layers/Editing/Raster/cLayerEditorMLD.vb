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

#End Region ' Imports 

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Layer editor base class that supports manual modification of Ecospace 
    ''' layers.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cLayerEditorMLD
        Inherits cLayerEditorRange

#Region " Private vars "

        ''' <summary>The depth layer to limit MLD values against.</summary>
        Private m_layerDepth As cEcospaceLayer = Nothing

#End Region ' Private vars

#Region " Construction "

        Public Sub New()
            MyBase.New()
        End Sub

#End Region ' Construction

#Region " Overrides "

        ''' -------------------------------------------------------------------
        ''' <inheritdoc cref="cLayerEditor.Initialize"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Initialize(ByVal uic As cUIContext, ByVal layer As cDisplayLayer)
            MyBase.Initialize(uic, layer)

            Dim bm As cEcospaceBasemap = uic.Core.EcospaceBasemap
            If (bm IsNot Nothing) Then
                Me.m_layerDepth = bm.LayerDepth
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set a cell value. Overridden to limit values to the actual map depths.
        ''' </summary>
        ''' <param name="ptSet"></param>
        ''' <param name="value"></param>
        ''' <param name="e"></param>
        ''' <param name="ptClick"></param>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub SetCellValue(ByVal ptSet As System.Drawing.Point, _
                                             ByVal value As Object, _
                                             ByVal e As System.Windows.Forms.MouseEventArgs, _
                                             ByVal ptClick As System.Drawing.Point)
            ' Sanity checks
            Debug.Assert(Me.m_layerDepth IsNot Nothing)
            If (Not Me.IsEditable) Then Return

            MyBase.SetCellValue(ptSet, Math.Min(CSng(value), CSng(Me.m_layerDepth.Cell(ptSet.Y, ptSet.X))), e, ptClick)

        End Sub

#End Region ' Overrides

    End Class

End Namespace
