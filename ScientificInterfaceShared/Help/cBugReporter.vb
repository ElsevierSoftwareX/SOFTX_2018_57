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
Imports System.Text
Imports System.Reflection
Imports EwEPlugin
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls
Imports System.IO
Imports System.Net.Mail

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Helper class for generating a bug report to be sent via the shell.
''' </summary>
''' ===========================================================================
Public Class cBugReporter

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns a bug report.
    ''' </summary>
    ''' <param name="strAppName">Name of the application.</param>
    ''' <param name="strMailTo">Target email address.</param>
    ''' <param name="pm">Plug-in manager to extract components from.</param>
    ''' -----------------------------------------------------------------------
    Public Shared Function BugReport(ByVal strAppName As String, _
                                     ByVal strMailTo As String, _
                                     Optional ByVal pm As cPluginManager = Nothing) As String

        Dim an As AssemblyName = Nothing
        Dim ub As New cUriBuilder("mailto:" & strMailTo)
        Dim sbBody As New System.Text.StringBuilder
        Dim strURL As String = ""

        ub.QueryString("subject") = strAppName & " incident report"

        sbBody.AppendLine("I experienced the following issue:")
        sbBody.AppendLine("")
        sbBody.AppendLine("(Please provide a detailed description of the issue, and steps to reproduce if possible. If required, please zip up and attach your model)")
        sbBody.AppendLine("")
        sbBody.AppendLine("")
        sbBody.AppendLine("")
        sbBody.AppendLine("")
        sbBody.AppendLine("------------------------------")
        sbBody.AppendLine("Configuration (do not modify):")
        sbBody.AppendLine(cSysConfig.OSVersion())
        sbBody.AppendLine(cSysConfig.NETVersion())
        For Each an In cAssemblyUtils.GetSummary(cAssemblyUtils.eSummaryFlags.EwECore)
            sbBody.AppendLine(String.Format("* {0}={2},{1}", _
                                            an.Name, cStringUtils.ToHexString(an.GetPublicKeyToken), an.Version))
        Next

        If (pm IsNot Nothing) Then
            For Each pa As cPluginAssembly In pm.PluginAssemblies
                an = pa.AssemblyName
                sbBody.AppendLine(String.Format("- {0}={2},{1}", _
                                                an.Name, cStringUtils.ToHexString(an.GetPublicKeyToken), an.Version))
            Next
        End If

        ub.QueryString("body") = sbBody.ToString()

        Return ub.ToString

    End Function

    'Private Shared Function SendAttachment(ByVal strAppName As String, _
    '                                ByVal strAddress As String, _
    '                                ByVal pm As cPluginManager) As Boolean

    '    Dim an As AssemblyName = Nothing
    '    Dim oMsg As New MailMessage()
    '    Dim sbBody As New System.Text.StringBuilder

    '    sbBody.AppendLine("I experienced the following issue with " & strAppName & ":")
    '    sbBody.AppendLine("")
    '    sbBody.AppendLine("(Please provide a detailed description of the issue, and steps to reproduce the error if possible. If required, please zip up and attach your model)")

    '    Dim strFile As String = Path.Combine(System.IO.Path.GetTempPath(), "EwE_config.txt")
    '    Dim swTemp As New StreamWriter(strFile)
    '    swTemp.WriteLine("EwE configuration (do not modify):")
    '    swTemp.WriteLine()
    '    swTemp.WriteLine(cSysConfig.OSVersion())
    '    swTemp.WriteLine(cSysConfig.NETVersion())
    '    swTemp.WriteLine()
    '    swTemp.WriteLine("EwE modules:")
    '    For Each an In cAssemblyUtils.GetSummary(cAssemblyUtils.eSummaryFlags.EwECore)
    '        swTemp.WriteLine(String.Format("* {0}={2},{1}", _
    '                                        an.Name, cStringUtils.ToHexString(an.GetPublicKeyToken), an.Version))
    '    Next

    '    If (pm IsNot Nothing) Then
    '        swTemp.WriteLine()
    '        swTemp.WriteLine("EwE plug-ins:")
    '        For Each pa As cPluginAssembly In pm.PluginAssemblies
    '            an = pa.AssemblyName
    '            swTemp.WriteLine(String.Format("- {0}={2},{1}", _
    '                                            an.Name, cStringUtils.ToHexString(an.GetPublicKeyToken), an.Version))
    '        Next
    '    End If
    '    swTemp.WriteLine("---------------------------------------------------")
    '    swTemp.Flush()
    '    swTemp.Close()

    '    'oMsg.From =
    '    oMsg.From = New Net.Mail.MailAddress("user@mail.com")
    '    oMsg.To.Add(New Net.Mail.MailAddress(strAddress))
    '    oMsg.Subject = strAppName & " incident report"
    '    oMsg.Body = sbBody.ToString()

    '    Dim oAttch As New Net.Mail.Attachment(strFile)
    '    oMsg.Attachments.Add(oAttch)

    '    Dim cl As New SmtpClient("127.0.0.1")
    '    Try
    '        cl.Send(oMsg)
    '    Catch ex As Exception
    '        Return False
    '    End Try

    '    Return True
    'End Function

End Class
