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
Imports System
Imports EwEUtils.Core

#End Region ' Imports

Namespace Data

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Interface for implementing a plugin point that can broadcast data.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Interface IDataProducerPlugin
        : Inherits IPlugin

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Initialization interface to inform the plug-in where to send its data
        ''' to once ready.
        ''' </summary>
        ''' <param name="broadcaster">The <see cref="IDataBroadcaster">IDataBroadcaster</see> 
        ''' to send data to.</param>
        ''' <remarks>
        ''' The plug-in should call <see cref="IDataBroadcaster.BroadcastData">IDataBroadcaster.BroadcastData</see>,
        ''' from where any <see cref="IDataConsumerPlugin">IDataConsumerPlugin</see>
        ''' -derived class gets a chance to consume the data by implementing
        ''' <see cref="IDataConsumerPlugin.ReceiveData">ReceiveData</see>.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Sub Broadcaster(ByVal broadcaster As IDataBroadcaster)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Requests whether data with a given <see cref="Type">Type</see> and
        ''' <see cref="IRunType">run type</see> is provided by this plug-in.
        ''' </summary>
        ''' <param name="typeData">
        ''' <see cref="Type">Type</see> of the data to request.
        ''' </param>
        ''' <param name="runType">
        ''' <see cref="IRunType">Run type</see> of the data to request.
        ''' </param>
        ''' -----------------------------------------------------------------------
        Function IsDataAvailable(ByVal typeData As Type, Optional ByVal runType As IRunType = Nothing) As Boolean

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Request data from this plug-in for a data with a specific
        ''' <see cref="Type">Type</see>.
        ''' </summary>
        ''' <param name="typeData"><see cref="Type">Type</see> of the data to request.</param>
        ''' <param name="data">The <see cref="IPluginData">data</see> offered by 
        ''' the plug-in.</param>
        ''' <returns>True if requested data is available.</returns>
        ''' -----------------------------------------------------------------------
        Function GetDataByType(ByVal typeData As Type, ByRef data As IPluginData) As Boolean

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get whether a data producer is allowed to distribute data.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Function IsEnabled() As Boolean

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Set whether a data producer is allowed to distribute data.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Function SetEnabled(ByVal bEnable As Boolean) As Boolean

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Set whether a plug-in distributes data for a given run type.
        ''' </summary>
        ''' <param name="typeData"><see cref="Type">Type</see> of the data to enable.</param>
        ''' <param name="runType">
        ''' <see cref="IRunType">Run type</see> of the data to enable or disable.
        ''' </param>
        ''' -----------------------------------------------------------------------
        Sub SetEnabled(ByVal typeData As Type, ByVal runType As IRunType, ByVal bEnable As Boolean)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get whether a plug-in distributes data for a given run type.
        ''' </summary>
        ''' <param name="typeData"><see cref="Type">Type</see> of the data to request 
        ''' enabled state for.</param>
        ''' <param name="runType">
        ''' <see cref="IRunType">Run type</see> of the data to enable or disable.
        ''' </param>
        ''' -----------------------------------------------------------------------
        Function IsEnabled(ByVal typeData As Type, ByVal runType As IRunType) As Boolean

    End Interface

End Namespace
