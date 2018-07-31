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

Imports System.Configuration
Imports System.IO
Imports EwEUtils

''' <summary>
''' Settings class that uses a custom <see cref="SettingsProvider"/>.
''' </summary>
''' <remarks>
''' For details about the overridden settings behaviour refer to <see cref="cEwESettingsProvider"/>.
''' </remarks>
Partial Friend NotInheritable Class Settings

    ''' <summary>Custom <see cref="cEwESettingsProvider">settings provider</see>.</summary>
    Private m_provider As cEwESettingsProvider = Nothing

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub New()

        MyBase.New()

        Me.m_provider = New cEwESettingsProvider(Path.GetFileNameWithoutExtension(Application.ExecutablePath), Me)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the default value for a given settings property name.
    ''' </summary>
    ''' <param name="strName">The name of the property to access. This name is not case-sensitive.</param>
    ''' <returns>A value, or Nothing if a property by this name does not exist.</returns>
    ''' -----------------------------------------------------------------------
    Public Function GetDefaultValue(ByVal strName As String) As Object
        Dim prop As SettingsProperty = Me.Properties(strName)
        If prop IsNot Nothing Then Return prop.DefaultValue
        Return Nothing
    End Function

End Class
