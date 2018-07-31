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
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.EwEGrid

    ''' <summary>
    ''' Base interface for connecting EwE cells, visualizers and grids to share
    ''' the use of the <see cref="cStyleGuide"/>, <see cref="cCore"/> and <see cref="cPropertyManager"/>.
    ''' </summary>
    Public Interface IEwECell
        Inherits IUIElement

        ''' <summary>
        ''' Get the <see cref="cCore"/> catering to the current user interface.
        ''' </summary>
        ReadOnly Property Core As cCore

        ''' <summary>
        ''' Get the <see cref="cPropertyManager"/> catering to the current user interface.
        ''' </summary>
        ReadOnly Property PropertyManager As cPropertyManager

        ''' <summary>
        ''' Get the <see cref="cStyleGuide"/> catering to the current user interface.
        ''' </summary>
        ReadOnly Property StyleGuide As cStyleGuide

        ''' <summary>
        ''' Get the <see cref="cStyleGuide.eStyleFlags"/> cell style.
        ''' </summary>
        Property Style() As cStyleGuide.eStyleFlags

    End Interface

End Namespace
