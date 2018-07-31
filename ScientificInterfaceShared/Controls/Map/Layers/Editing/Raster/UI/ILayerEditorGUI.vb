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

Namespace Controls.Map.Layers

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Interface for implementing a user interface for editing raster layers.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Interface ILayerEditorGUI

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize an editor GUI to a raster layer.
        ''' </summary>
        ''' <param name="editor"></param>
        ''' -------------------------------------------------------------------
        Sub Initialize(ByVal editor As cLayerEditorRaster)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Inform the editor GUI that the user has started editing the 
        ''' raster layer.
        ''' </summary>
        ''' <param name="editor"></param>
        ''' -------------------------------------------------------------------
        Sub StartEdit(ByVal editor As cLayerEditorRaster)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Inform the editor GUI that the user has finished editing the raster 
        ''' layer.
        ''' </summary>
        ''' <param name="editor"></param>
        ''' -------------------------------------------------------------------
        Sub EndEdit(ByVal editor As cLayerEditorRaster)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update the content of the editor GUI because something in the
        ''' underlying <see cref="cDisplayLayer">display layer</see> or
        ''' <see cref="cLayerEditorRaster">raster layer editor</see> has changed.
        ''' </summary>
        ''' <param name="editor"></param>
        ''' -------------------------------------------------------------------
        Sub UpdateContent(ByVal editor As cLayerEditorRaster)

    End Interface

End Namespace
