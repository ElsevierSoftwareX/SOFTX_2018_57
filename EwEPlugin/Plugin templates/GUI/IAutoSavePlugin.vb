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

''' ---------------------------------------------------------------------------
''' <summary>
''' Interface for implementing a plugin point that automatically saves its
''' data. Note that this plug-in point just serves to identify the auto-save
''' setting in the user interface. The plug-in is responsible for triggering and
''' implementing the auto-save behaviour.
''' </summary>
''' <remarks>
''' <para>The EwE framework expects an AutoSave plug-in to store its files in a
''' location that is determined as follows:</para>
''' <code>Dim strPath as string = Path.Combine(core.DefaultOutputPath(Me.AutoSaveType), Me.AutoSaveSubPath)</code>
''' <para>The EwE auto-save options interface will display this storage location
''' for auto-save plug-ins. Developers are responsible to follow this folder
''' convention when implementing auto-save behaviour.</para>
''' </remarks>
''' ---------------------------------------------------------------------------
Public Interface IAutoSavePlugin
    : Inherits IPlugin

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether this plug-in is allowed to auto-save data.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Property AutoSave As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Name to identify the auto-save setting.
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Function AutoSaveName() As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the <see cref="eAutosaveTypes"/> core autosave type that defines the
    ''' output path that this plug-in writes to.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Function AutoSaveType() As eAutosaveTypes

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the output path to save to. A plug-in is responsible for ensuting
    ''' that the default output path is nested under the EwE location for the 
    ''' provided <see cref="AutoSaveType"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Function AutoSaveOutputPath() As String

End Interface

