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
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Resources
Imports System.Windows.Forms

#End Region ' Imports

Namespace Utilities

    Public Class cResourceUtils

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Saves an embedded resource to a file
        ''' </summary>
        ''' <param name="strResourceName">The name of the resource, including  in the current assembly, current namespace.</param>
        ''' <param name="strFileName">The name of the file to save the resource to</param>
        ''' <param name="bOverwrite">States whether an existing file is allowed to be overwritten</param>
        ''' <param name="ass">The assembly to obtain the resource from.</param>
        ''' <param name="strNamespace">The namespace to obtain the resource from.</param>
        ''' <returns>True if successful</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function SaveResourceToFile(ByVal strResourceName As String, _
                                                  ByVal strFileName As String, _
                                                  Optional ByVal bOverwrite As Boolean = False, _
                                                  Optional ByVal ass As Assembly = Nothing, _
                                                  Optional ByVal strNamespace As String = "") As Boolean

            Dim sResource As Stream = Nothing
            Dim sFile As FileStream = Nothing
            Dim nBufLen As Integer = 256
            Dim byBuffer(nBufLen) As Byte
            Dim nBytesRead As Integer = 0

            If ass Is Nothing Then
                ass = Assembly.GetExecutingAssembly()
            End If

            If String.IsNullOrWhiteSpace(strNamespace) Then
                strNamespace = ass.GetName().Name.ToString()
            End If

            sResource = ass.GetManifestResourceStream(strNamespace & "." & strResourceName)

            ' Pre
            Debug.Assert(Not String.IsNullOrEmpty(strFileName), "Required target file name missing")
            Debug.Assert(sResource IsNot Nothing, String.Format("Resource {0} not found in {1}", strResourceName, strNamespace))

            ' Work with full path
            strFileName = Path.GetFullPath(strFileName)

            ' Make sure directory is available
            If Not cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strFileName), True) Then
                Return False
            End If

            Try
                If (bOverwrite) Then
                    ' Create the file, overwriting any existing file with the same path
                    sFile = New FileStream(strFileName, FileMode.Create, FileAccess.Write)
                Else
                    ' Create the file but do not overwrite
                    sFile = New FileStream(strFileName, FileMode.CreateNew, FileAccess.Write)
                End If
            Catch ex As Exception
                ' Just so you know
                Debug.Print("Unable to create or overwrite file {0}", strFileName)
                ' Report failure
                Return False
            End Try

            ' Copy embedded resource to file
            nBytesRead = sResource.Read(byBuffer, 0, nBufLen)
            While (nBytesRead > 0)
                sFile.Write(byBuffer, 0, nBytesRead)
                nBytesRead = sResource.Read(byBuffer, 0, nBufLen)
            End While
            ' Done
            sFile.Close()
            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the system icon for a <see cref="System.Windows.Forms.MessageBoxIcon">message box 
        ''' icon</see> identifier.
        ''' </summary>
        ''' <param name="mbi"><see cref="System.Windows.Forms.MessageBoxIcon">message box icon</see>
        ''' identifier to get the system icon for.</param>
        ''' <returns>An <see cref="Icon">Icon</see>, or Nothing if the icon
        ''' could not be found.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetMessageBoxIcon(ByVal mbi As MessageBoxIcon) As Icon

            Dim objIcon As Icon = Nothing

            Select Case mbi
                Case MessageBoxIcon.Asterisk
                    objIcon = SystemIcons.Asterisk
                Case MessageBoxIcon.Error
                    objIcon = SystemIcons.Error
                Case MessageBoxIcon.Exclamation
                    objIcon = SystemIcons.Exclamation
                Case MessageBoxIcon.Hand, _
                     MessageBoxIcon.Stop
                    objIcon = SystemIcons.Hand
                Case MessageBoxIcon.Information
                    objIcon = SystemIcons.Information
                Case MessageBoxIcon.Question
                    objIcon = SystemIcons.Question
                Case MessageBoxIcon.Warning
                    objIcon = SystemIcons.Warning
                Case Else
                    ' NOP
            End Select

            Return objIcon

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load a resource string from a .NET assembly.
        ''' <seealso cref="LoadString(String, ResourceManager, CultureInfo)"/>
        ''' <seealso cref="LoadString(String, Assembly, String, CultureInfo)"/>
        ''' </summary>
        ''' <param name="strName">The name of the string resource.</param>
        ''' <param name="typeAssembly">The type for which to find the assembly.</param>
        ''' <param name="strNamespace">The namespace within the assembly, if any.</param>
        ''' <param name="culture">The culture info, if any.</param>
        ''' <returns>A string, or <paramref name="strName"/> if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Shared Function LoadString(ByVal strName As String, _
                                   ByVal typeAssembly As Type, _
                                   Optional ByVal strNamespace As String = "", _
                                   Optional ByVal culture As CultureInfo = Nothing) As String

            Dim ass As Assembly = Assembly.GetAssembly(typeAssembly)
            Return LoadString(strName, ass, strNamespace, culture)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load a resource string from a .NET assembly.
        ''' <seealso cref="LoadString(String, ResourceManager, CultureInfo)"/>
        ''' <seealso cref="LoadString(String, Type, String, CultureInfo)"/>
        ''' </summary>
        ''' <param name="strName">The name of the string resource.</param>
        ''' <param name="ass">The assembly to load the resource from.</param>
        ''' <param name="strNamespace">The namespace within the assembly, if any.</param>
        ''' <param name="culture">The culture info, if any. If not specified, the
        ''' executing culture is used.</param>
        ''' <returns>A string, or <paramref name="strName"/> if an error occurred.</returns>
        ''' <remarks>
        ''' <para>This method will attempt to construct a resource manager for the given
        ''' assembly and namespace, and will then attempt to load the string for the 
        ''' indicated culture.</para>
        ''' <para>If the resource manager to obtain the resource from is know, please
        ''' use the much faster method <see cref="LoadString(String, ResourceManager, CultureInfo)"/> 
        ''' instead.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function LoadString(ByVal strName As String,
                                          Optional ByVal ass As Assembly = Nothing,
                                          Optional ByVal strNamespace As String = "",
                                          Optional ByVal culture As CultureInfo = Nothing) As String

            Dim rm As ResourceManager = Nothing
            Dim strRes As String = ""

            ' Provide defaults
            If (ass Is Nothing) Then ass = Assembly.GetExecutingAssembly()
            If (String.IsNullOrEmpty(strNamespace)) Then strNamespace = ass.GetName.Name & ".resources"

            Return LoadString(strName, New ResourceManager(strNamespace, ass), culture)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load a resource string from a given resource manager.
        ''' </summary>
        ''' <param name="strName">The name of the string resource.</param>
        ''' <param name="rm">The resource manager to load the string from.</param>
        ''' <param name="culture">The culture info, if any.</param>
        ''' <returns>A string, or <paramref name="strName"/> if an error occurred.</returns>
        ''' -------------------------------------------------------------------     
        Public Shared Function LoadString(ByVal strName As String, rm As ResourceManager,
                                          Optional ByVal culture As CultureInfo = Nothing) As String
            Dim strRes As String = ""
            If (culture Is Nothing) Then culture = Threading.Thread.CurrentThread.CurrentUICulture
            Try
                strRes = rm.GetString(strName, culture)
            Catch ex As Exception
                ' Whoah!
            End Try
            Return strRes

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns all the resources for the current culture.
        ''' </summary>
        ''' <param name="ass">The assembly to load the resources from. 
        ''' If left empty the current executing assembly is used.</param>
        ''' <param name="strNamespace">The namespace within the assembly, if any.
        ''' If left empty the name of the <paramref name="ass">provided assembly</paramref> 
        ''' is used.</param>
        ''' <param name="culture">The culture info, if any. If left empty the
        ''' current loaded culture is used.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetResources(Optional ByVal ass As Assembly = Nothing, _
                                            Optional ByVal strNamespace As String = "", _
                                            Optional ByVal culture As CultureInfo = Nothing) As Dictionary(Of String, Object)

            Dim dic As New Dictionary(Of String, Object)
            Dim e As System.Collections.IDictionaryEnumerator = Nothing
            Dim rm As ResourceManager = Nothing
            Dim strRes As String = ""

            ' Provide defaults
            If (ass Is Nothing) Then ass = Assembly.GetExecutingAssembly()
            If (culture Is Nothing) Then culture = Threading.Thread.CurrentThread.CurrentUICulture
            If (String.IsNullOrEmpty(strNamespace)) Then strNamespace = ass.GetName.Name

            rm = New ResourceManager(strNamespace & ".resources", ass)

            Using recset As Resources.ResourceSet = rm.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, False, True)
                For Each entry As DictionaryEntry In recset
                    Dim strName As String = entry.Key.ToString
                    dic.Add(strName, My.Resources.ResourceManager.GetObject(strName))
                Next
            End Using

            Return dic

        End Function

    End Class

End Namespace
