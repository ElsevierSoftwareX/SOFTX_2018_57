﻿' ===============================================================================
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
    ''' Base type for data shared by plugins.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Interface IPluginData

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Name of the <see cref="IPlugin">type name</see> of the plug-in that 
        ''' exposed this data.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        ReadOnly Property PluginName() As String

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' The <see cref="IRunType">run type</see> that this data was produced with.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        ReadOnly Property RunType() As IRunType

    End Interface

End Namespace
