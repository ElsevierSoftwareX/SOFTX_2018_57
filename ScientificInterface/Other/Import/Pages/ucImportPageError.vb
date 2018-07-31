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
Imports ScientificInterfaceShared.Controls.Wizard

#End Region ' Imports

Namespace Import

    ''' =======================================================================
    ''' <summary>
    ''' Import wizard error page.
    ''' </summary>
    ''' =======================================================================
    Public Class ucImportPageError
        Implements IWizardPage

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the welcome page.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Init(ByVal wizard As cWizard, ByVal uic As cUIContext) _
            Implements IWizardPage.Init
            ' NOP
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Close the welcome page.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Close() _
             Implements IWizardPage.Close
            ' NOP
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the welcome page is busy.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function IsBusy() As Boolean _
              Implements IWizardPage.IsBusy
            ' Page does not have a busy state
            Return False
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the welcom page allows the wizard to navigate backward.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function AllowNavBack() As Boolean _
            Implements IWizardPage.AllowNavBack
            ' Page does not restrict backward navigation
            Return False
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether the welcom page allows the wizard to navigate backward.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function AllowNavForward() As Boolean _
            Implements IWizardPage.AllowNavForward
            ' Page does not restrict forward navigation
            Return False
        End Function

    End Class

End Namespace ' Import
