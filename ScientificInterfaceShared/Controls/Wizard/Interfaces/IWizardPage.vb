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
    ''' Interface for building a wizard page.
    ''' </summary>
    ''' =======================================================================
    Public Interface IWizardPage

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize a page with the wizard content.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Sub Init(ByVal wizard As cWizard, ByVal uic As cUIContext)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Close a wizard page. The wizard is most likely navigating away
        ''' from this page. The page would do well to save its content to the
        ''' wizard at this point.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Sub Close()

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether a page allows its parent wizard to navigate away to 
        ''' a previous page in the page chain.
        ''' </summary>
        ''' <remarks>
        ''' Note that pages should not try to make assumptions about their 
        ''' placement in the page chain. The parent wizard is responsible for 
        ''' handling beginning and end of page chains. This flag merely states
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Function AllowNavBack() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether a page allows its parent wizard to navigate forward
        ''' to a next page in the page chain.
        ''' </summary>
        ''' <remarks>
        ''' Note that pages should not try to make assumptions about their 
        ''' placement in the page chain. The parent wizard is responsible for 
        ''' handling beginning and end of page chains. This flag merely states
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Function AllowNavForward() As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether a page is busy.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Function IsBusy() As Boolean

    End Interface

End Namespace ' Controls.Wizard
