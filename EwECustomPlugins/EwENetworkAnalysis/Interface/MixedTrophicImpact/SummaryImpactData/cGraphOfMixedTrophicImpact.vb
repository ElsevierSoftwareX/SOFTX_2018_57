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
Imports System.Windows.Forms
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Style
Imports ZedGraph

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' MTI graph with bars.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class cGraphOfMixedTrophicImpact
    Inherits cContentManager

    Public Sub New()
        ' NOP
    End Sub

    Public Overrides Function PageTitle() As String
        Return "Mixed tropic level impacts (external)"
    End Function

    Public Overrides Function Attach(ByVal manager As cNetworkManager, _
                                     ByVal datagrid As DataGridView, _
                                     ByVal graph As ZedGraphControl, _
                                     ByVal plot As ucPlot, _
                                     ByVal toolstrip As ToolStrip, _
                                     ByVal uic As cUIContext) As Boolean
        Return MyBase.Attach(manager, datagrid, graph, plot, toolstrip, uic)
    End Function

    Public Overrides Sub DisplayData()

        Dim strOutputFileDir As String
        Dim strOutputFileName As String
        Dim FileNumber As Integer
        Dim ZeroString As String
        Dim NoDisplay As Integer
        Dim EnUSLocale As New CultureInfo("en-US")
        Dim sg As cStyleGuide = Me.StyleGuide
        Dim bShowItem As Boolean = True

        'Write data to file
        strOutputFileDir = System.IO.Path.GetTempPath
        strOutputFileName = "MTI.txt"
        If Dir(strOutputFileDir + "\") = "" Then MkDir(strOutputFileDir)
        FileNumber = FreeFile()
        FileOpen(FileNumber, strOutputFileDir & "\" & strOutputFileName, OpenMode.Output)

        NoDisplay = 0
        For i As Integer = 1 To Me.NetworkManager.nGroups
            ' Only show visible groups
            bShowItem = sg.GroupVisible(i)
            If bShowItem Then NoDisplay += 1
        Next
        For i As Integer = 1 To Me.NetworkManager.nFleets
            ' Only show visible fleets
            bShowItem = sg.FleetVisible(i)
            If bShowItem Then NoDisplay += 1
        Next
        PrintLine(FileNumber, Format(NoDisplay, "00"))

        For i As Integer = 1 To Me.NetworkManager.nGroups + Me.NetworkManager.nFleets
            If i <= Me.NetworkManager.nGroups Then
                ' Only show visible groups
                bShowItem = sg.GroupVisible(i)
            Else
                ' Only show visible fleets
                bShowItem = sg.FleetVisible(i - Me.NetworkManager.nGroups)
            End If
            If bShowItem Then
                ZeroString = "                    "
                If i <= Me.NetworkManager.nGroups Then
                    Mid$(ZeroString, 1) = Me.NetworkManager.GroupName(i)
                Else
                    Mid(ZeroString, 1) = Me.NetworkManager.FleetName(i - Me.NetworkManager.nGroups)
                End If
                Print(FileNumber, ZeroString)

                For j As Integer = 1 To Me.NetworkManager.nGroups + Me.NetworkManager.nFleets
                    If i <= Me.NetworkManager.nGroups Then
                        ' Only show visible groups
                        bShowItem = sg.GroupVisible(i)
                    Else
                        ' Only show visible fleets
                        bShowItem = sg.FleetVisible(i - Me.NetworkManager.nGroups)
                    End If
                    If bShowItem Then
                        If Me.NetworkManager.MixedTrophicImpacts(i, j) >= 0.0 Then
                            Print(FileNumber, Me.NetworkManager.MixedTrophicImpacts(i, j).ToString("000.00", EnUSLocale))
                        Else
                            Dim TmpString As String
                            TmpString = Me.NetworkManager.MixedTrophicImpacts(i, j).ToString("00.00", EnUSLocale)
                            If TmpString = "00.00" Then TmpString = "000.00"
                            Print(FileNumber, TmpString)
                        End If
                    End If
                Next j

                PrintLine(FileNumber, "")
            End If
        Next i
        FileClose(FileNumber)

        ' Todo_JS: Reprogram impacts logic as a .NET control, and get rid of the 16 bit apps once and for all.

        ' Convert text file location to a 8.3 file
        Dim strPath As String = Path.Combine(strOutputFileDir, strOutputFileName)
        Dim strPath83 As String = Space(255)
        Dim strError As String = ""

        GetShortPathName(strPath, strPath83, 255)

        'Execute the external application through the general function on EwEUtils
        If Not cSystemUtils.AppExec("impacts.exe", strPath83, strError) Then
            Dim msg As New cMessage(String.Format(My.Resources.PROMPT_APPLAUNCH_FAILED, "impacts.exe", strError), _
                                    eMessageType.Any, eCoreComponentType.External, eMessageImportance.Critical)
            Me.NetworkManager.Core.Messages.SendMessage(msg)
        End If
    End Sub

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)> _
    <Obsolete("Kernel32 call should be eliminated for full MONO compliance")> _
    Public Shared Function GetShortPathName(ByVal strLongPath As String, <MarshalAs(UnmanagedType.LPTStr)> ByVal strShortPath As String, <MarshalAs(UnmanagedType.U4)> ByVal bufferSize As Integer) As Integer
    End Function

End Class
