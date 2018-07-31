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

Namespace Data

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Interface for implementing a plugin point that is able to receive broadcasted
    ''' data.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Interface IDataConsumerPlugin
        : Inherits IPlugin

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Interface to receive data originating from 
        ''' <see cref="IDataBroadcaster.BroadcastData">IDataBroadcaster.BroadcastData</see>.
        ''' </summary>
        ''' <param name="strDataName">Name of the data that is being broadcasted.</param>
        ''' <param name="data">The <see cref="IPluginData">data</see> that is being 
        ''' broadcasted.</param>
        ''' -----------------------------------------------------------------------
        Function ReceiveData(ByVal strDataName As String, ByVal data As IPluginData) As Boolean

    End Interface

End Namespace
