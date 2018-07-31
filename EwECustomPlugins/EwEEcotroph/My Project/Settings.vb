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

Imports System.IO
Imports System.Reflection
Imports System.Configuration
Imports EwEUtils

Namespace My

    ''' <summary>
    ''' Settings class that uses a custom <see cref="System.Configuration.SettingsProvider"/>.
    ''' </summary>
    ''' <remarks>
    ''' For details about the overridden settings behaviour refer to <see cref="cEwESettingsProvider"/>.
    ''' </remarks>
    Partial Friend NotInheritable Class MySettings

        ''' <summary>Custom <see cref="cEwESettingsProvider">settings provider</see>.</summary>
        Private m_provider As cEwESettingsProvider = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()

            MyBase.New()

            Dim asm As Assembly = Assembly.GetAssembly(GetType(MySettings))
            Me.m_provider = New cEwESettingsProvider(Path.GetFileNameWithoutExtension(asm.Location), Me)

        End Sub

    End Class

End Namespace
