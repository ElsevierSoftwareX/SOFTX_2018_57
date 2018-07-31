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
Imports System.Reflection
Imports EwEUtils.SystemUtilities
Imports EwEPlugin
Imports EwEUtils.Utilities
Imports System.Text

#End Region ' Imports

Namespace Controls

    Public Class cSysConfig

        Public Shared Function OSVersion() As String
            Dim strOS As String = String.Format(My.Resources.GENERIC_LABEL_DOUBLE, My.Computer.Info.OSFullName, My.Computer.Info.OSVersion)
            Dim strBit As String = If(cSystemUtils.Is64BitOS, My.Resources.ABOUT_64BIT, My.Resources.ABOUT_32BIT)
            Return String.Format(My.Resources.GENERIC_LABEL_DETAILED, strOS, strBit)
        End Function

        Public Shared Function NETVersion() As String
            Return String.Format(My.Resources.ABOUT_NET_VERSION,
                                 System.Environment.Version.ToString(),
                                 If(cSystemUtils.Is64BitProcess, My.Resources.ABOUT_64BIT, My.Resources.ABOUT_32BIT))
        End Function

        Public Shared Function Modules(pm As cPluginManager) As String

            Dim aanLoaded As AssemblyName() = pm.PluginAssemblyNames()
            Dim aanPlugins As AssemblyName() = cAssemblyUtils.GetSummary()
            Dim an As AssemblyName = Nothing
            Dim sb As New StringBuilder()

            For Each an In cAssemblyUtils.GetSummary(cAssemblyUtils.eSummaryFlags.EwECore)
                sb.AppendLine(String.Format("* {0}={2},{1}", _
                                                an.Name, cStringUtils.ToHexString(an.GetPublicKeyToken), an.Version))
            Next
            If (pm IsNot Nothing) Then
                For Each pa As cPluginAssembly In pm.PluginAssemblies
                    an = pa.AssemblyName
                    sb.AppendLine(String.Format("- {0}={2},{1}", _
                                                    an.Name, cStringUtils.ToHexString(an.GetPublicKeyToken), an.Version))
                Next
            End If

            Return sb.ToString

        End Function

    End Class

End Namespace
