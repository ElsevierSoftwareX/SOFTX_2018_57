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
Option Explicit On

Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Content manager base class for invoking NA pyramid interfaces.
''' </summary>
''' ---------------------------------------------------------------------------
<CLSCompliant(False)> _
Public MustInherit Class cPyramid
    Inherits cContentManager

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Overridden to set up the pyramid file, outsource writing the file and
    ''' invoke the pyramid app.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Sub DisplayData()

        Dim model As cEwEModel = Me.NetworkManager.Core.EwEModel
        Dim core As cCore = Me.NetworkManager.Core
        Dim ciEnUSLocale As New CultureInfo("en-US")
        Dim sw As StreamWriter = Nothing
        Dim strError As String = ""
        Dim strOutputFile As String = ""
        Dim strOutputFile83 As String = Space(255)
        Dim bSucces As Boolean = False

        strOutputFile = modUtility.PyramidTempFile(model.Name, Me.PyramidType, ".txt")

        Try
            sw = New StreamWriter(strOutputFile, False, New System.Text.UTF8Encoding())
            bSucces = Me.WritePyramidFile(core, sw, ciEnUSLocale)
            strError = "Cannot write temp pyramid file to " & strOutputFile
            sw.Close()
        Catch ex As Exception
            strError = ex.Message
            bSucces = False
        End Try

        If Not bSucces Then
            Me.SendMessage(String.Format(My.Resources.PROMPT_FILECREATE_FAILED, "pyramid.exe", strError))
            Return
        End If

        GetShortPathName(strOutputFile, strOutputFile83, 255)

        If Not cSystemUtils.AppExec("pyramid.exe", strOutputFile83, strError, "") Then
            Dim msg As New cMessage(String.Format(My.Resources.PROMPT_APPLAUNCH_FAILED, "pyramid.exe", strError), _
                                    eMessageType.Any, eCoreComponentType.External, eMessageImportance.Critical)
            Me.NetworkManager.Core.Messages.SendMessage(msg)
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Override this to state the type of pyramid file to write.
    ''' </summary>
    ''' <returns>A <see cref="ePyramidTypes">pyramid type</see> indicator.</returns>
    ''' -----------------------------------------------------------------------
    MustOverride Function PyramidType() As ePyramidTypes

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Override this to write the pyramid data file.
    ''' </summary>
    ''' <param name="core">Core to use.</param>
    ''' <param name="sw">Stream writer to use.</param>
    ''' <param name="ci">Number format to use.</param>
    ''' <returns>True if all cool and froody.</returns>
    ''' -----------------------------------------------------------------------
    MustOverride Function WritePyramidFile(ByVal core As cCore, _
                                           ByVal sw As StreamWriter, _
                                           ByVal ci As CultureInfo) As Boolean

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)> _
    <Obsolete("Kernel32 call should be eliminated for full MONO compliance")> _
    Public Shared Function GetShortPathName(ByVal strLongPath As String, <MarshalAs(UnmanagedType.LPTStr)> ByVal strShortPath As String, <MarshalAs(UnmanagedType.U4)> ByVal bufferSize As Integer) As Integer
    End Function

End Class
