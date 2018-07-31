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
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Storage class for system-wide, model independent EwE core settings.
''' </summary>
''' ---------------------------------------------------------------------------
Friend Class cCoreSettings

#Region " Private vars "

    ''' <summary>Autosave flags</summary>
    Private m_bAutosave() As Boolean

#End Region ' Private vars

#Region " Constructor "

    Public Sub New()
        ReDim m_bAutosave([Enum].GetValues(GetType(eAutosaveTypes)).Length)
    End Sub

#End Region ' Constructor

#Region " Accessors "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether a component is allowed to auto-save.
    ''' </summary>
    ''' <param name="t">The <see cref="eAutosaveTypes">auto-save enabled component</see>
    ''' to enable or disable.</param>
    ''' -----------------------------------------------------------------------
    Public Property Autosave(t As eAutosaveTypes) As Boolean
        Get
            Return Me.m_bAutosave(t)
        End Get
        Set(value As Boolean)
            Me.m_bAutosave(t) = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether a component is allowed to auto-save.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property AutosaveHeaders() As Boolean = True

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the core output path.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property OutputPath As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the model backup path mask.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property BackupFileMask As String = ""

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the default EwE author.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Author As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the default EwE author contact.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Contact As String

#End Region ' Accessors

End Class
