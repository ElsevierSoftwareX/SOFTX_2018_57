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

Imports System.Windows.Forms
Imports Microsoft.Win32
Imports System
Imports System.IO
Imports Microsoft.VisualBasic

#End Region ' Imports

Namespace Utilities

    ''' =======================================================================
    ''' <summary>
    ''' Helper class offering miscellaneous sound-related functionalities.
    ''' </summary>
    ''' =======================================================================
    Public Class cSoundUtilities

#Region " Private vars "

        Private Shared s_sounds As cSystemSounds = Nothing

        Private Class cSystemSounds
            ''' <summary>SystemAsterisk</summary>
            Public Property Asterisk As String
            ''' <summary>SystemExclamation</summary>
            Public Property Exclamation As String
            ''' <summary>SystemHand</summary>
            Public Property Hand As String
            ''' <summary>SystemNotification</summary>
            Public Property Notification As String
            ''' <summary>SystemQuestion</summary>
            Public Property Question As String
            ''' <summary>A default sound</summary>
            Public Property [Default] As String
        End Class

#End Region ' Private vars

#Region " Public access "


        Public Shared Sub PlaySound(ByVal icon As MessageBoxIcon)

            cSoundUtilities.InitSounds()

            Dim strFileName As String = s_sounds.[Default]

            Select Case icon
                Case MessageBoxIcon.Asterisk
                    strFileName = s_sounds.Asterisk
                Case MessageBoxIcon.Exclamation
                    strFileName = s_sounds.Exclamation
                Case MessageBoxIcon.Hand,
                     MessageBoxIcon.Stop
                    strFileName = s_sounds.Hand
                Case MessageBoxIcon.Information
                    strFileName = s_sounds.Notification
                Case MessageBoxIcon.Question
                    strFileName = s_sounds.Question
                Case MessageBoxIcon.Warning
                    strFileName = s_sounds.[Default]
            End Select

            PlaySound(strFileName)

        End Sub

        Public Shared Sub PlaySound(ByVal strFileName As String)

            If String.IsNullOrWhiteSpace(strFileName) Then Return
            Try
                My.Computer.Audio.Play(strFileName, AudioPlayMode.Background)
            Catch ex As Exception
                ' Whoah
            End Try

        End Sub

        Public Shared Sub PlaySound(ByVal stream As Stream)

            Try
                My.Computer.Audio.Play(stream, AudioPlayMode.Background)
            Catch ex As Exception
                ' Whoah
            End Try

        End Sub

#End Region ' Public access

#Region " Internals "

        Private Shared Sub InitSounds()

            If (cSoundUtilities.s_sounds Is Nothing) Then
                cSoundUtilities.s_sounds = New cSystemSounds() With {
                    .Asterisk = cRegistryUtils.ReadKey(Registry.CurrentUser, "AppEvents\Schemes\Apps\.Default\SystemAsterisk\.Current", ""),
                    .Exclamation = cRegistryUtils.ReadKey(Registry.CurrentUser, "AppEvents\Schemes\Apps\.Default\SystemExclamation\.Current", ""),
                    .Hand = cRegistryUtils.ReadKey(Registry.CurrentUser, "AppEvents\Schemes\Apps\.Default\SystemHand\.Current", ""),
                    .Notification = cRegistryUtils.ReadKey(Registry.CurrentUser, "AppEvents\Schemes\Apps\.Default\SystemNotification\.Current", ""),
                    .Question = cRegistryUtils.ReadKey(Registry.CurrentUser, "AppEvents\Schemes\Apps\.Default\SystemQuestion\.Current", ""),
                    .[Default] = cRegistryUtils.ReadKey(Registry.CurrentUser, "AppEvents\Schemes\Apps\.Default\.Default\.Current", "")
                }
            End If
        End Sub

#End Region ' Internals

    End Class

End Namespace
