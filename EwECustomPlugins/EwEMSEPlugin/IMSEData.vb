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
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwECore

#End Region ' Imports

''' <summary>
''' Foundation interface for user-supplied data in the MSE plug-in.
''' </summary>
Public Interface IMSEData

    ''' <summary>
    ''' Load data from a file.
    ''' </summary>
    ''' <param name="strFilename">Optional file name to load data from.</param>
    ''' <returns>True if successful.</returns>
    Function Load(Optional msg As cMessage = Nothing, _
                  Optional strFilename As String = "") As Boolean

    ''' <summary>
    ''' Load data to a file.
    ''' </summary>
    ''' <param name="strFilename">Optional file name to save data to.</param>
    ''' <returns>True if successful.</returns>
    Function Save(Optional strFilename As String = "") As Boolean

    ''' <summary>
    ''' Returns whether the data has been changed since the last time it was loaded.
    ''' </summary>
    ''' <returns>True if the data has been changed since the last time it was loaded.</returns>
    Function IsChanged() As Boolean

    ''' <summary>
    ''' Set the data to default values.
    ''' </summary>
    Sub Defaults()

    ''' <summary>
    ''' Returns whether the file(s) for the data exist
    ''' </summary>
    ''' <param name="strFilename"></param>
    ''' <returns></returns>
    Function FileExists(Optional strFilename As String = "") As Boolean

End Interface
