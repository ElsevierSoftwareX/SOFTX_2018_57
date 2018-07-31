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

Namespace Controls

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Interface for implementing an configuration page for integration in a 
    ''' settings dialog
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Interface IOptionsPage

        Event OnChanged(ByVal sender As IOptionsPage, ByVal args As EventArgs)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Enumerated type stating all possible results when applying the content
        ''' of an options page.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Enum eApplyResultType As Integer
            ''' <summary>Application was successful.</summary>
            Success
            ''' <summary>Application was successful but requires a restart.</summary>
            Success_restart
            ''' <summary>Application successful, but need administrator privileges to work.</summary>
            Success_administrator
            ''' <summary>Application failed.</summary>
            Failed
        End Enum

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Check whether the options page can be applied.
        ''' </summary>
        ''' <returns>True if the options page can be applied.</returns>
        ''' -----------------------------------------------------------------------
        Function CanApply() As Boolean

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Apply the content of an options page to the 'system'.
        ''' </summary>
        ''' <returns>An <see cref="eApplyResultType">apply result</see>.</returns>
        ''' -----------------------------------------------------------------------
        Function Apply() As eApplyResultType

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Revert the current page to default values
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Sub SetDefaults()

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether this options page can set defaults.
        ''' </summary>
        ''' <returns>True if the options page can set defaults.</returns>
        ''' -----------------------------------------------------------------------
        Function CanSetDefaults() As Boolean

    End Interface

End Namespace
