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

#End Region ' Imports

Namespace Controls.Wizard

    ''' =======================================================================
    ''' <summary>
    ''' Interface for building a wizard navigation structure.
    ''' </summary>
    ''' =======================================================================
    <CLSCompliant(True)> _
    Public Interface IWizardNavigation

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Attach a wizard to a navigation structure
        ''' </summary>
        ''' <param name="wizard">The wizard to attach.</param>
        ''' -------------------------------------------------------------------
        Sub Attach(ByVal wizard As cWizard)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Detach the current wizard from a navigation structure
        ''' </summary>
        ''' -------------------------------------------------------------------
        Sub Detach()

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Wizard callback, informing the navigation structure to update
        ''' itself.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Sub UpdateNavigation()

    End Interface

End Namespace ' Controls.Wizard
