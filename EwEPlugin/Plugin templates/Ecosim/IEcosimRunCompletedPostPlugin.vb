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

''' ---------------------------------------------------------------------------
''' <summary>
''' Interface for implementing plugin points that are invoked when Ecosim finishes
''' a run, and all <see cref="IEcosimRunCompletedPlugin"/> points have been called.
''' <seealso cref="IEcosimRunCompletedPlugin"/>
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface IEcosimRunCompletedPostPlugin
    Inherits IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Plug-in point that is called when Ecosim has finished running, and all
    ''' <see cref="IEcosimRunCompletedPlugin"/> points have been called.
    ''' <seealso cref="IEcosimRunCompletedPlugin"/>
    ''' </summary>
    ''' <param name="EcosimDatastructures">Ecosim data structires.</param>
    ''' -----------------------------------------------------------------------
    Sub EcosimRunCompletedPost(ByVal EcosimDatastructures As Object)

End Interface
