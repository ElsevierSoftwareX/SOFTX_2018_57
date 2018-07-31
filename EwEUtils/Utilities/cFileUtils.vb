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
Imports System.Collections.Generic
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Linq
Imports System.Security.AccessControl
Imports System.Text

#End Region ' Imports

Namespace Utilities

    ''' =======================================================================
    ''' <summary>
    ''' Helper class offering miscellaneous file-related functionalities.
    ''' </summary>
    ''' =======================================================================
    Public Class cFileUtils

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a text into a string that would be accepted by the OS as a
        ''' valid file name.
        ''' </summary>
        ''' <param name="strText">Text to convert into a file name.</param>
        ''' <param name="bProtectPath">Flag stating whether any path information
        ''' included in <paramref name="strText">strText</paramref> should be
        ''' preserved. If False, an path information is stripped off.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToValidFileName(ByVal strText As String, ByVal bProtectPath As Boolean) As String

            Dim strPath As String = ""
            Dim strFile As String = ""

            If String.IsNullOrEmpty(strText) Then Return ""

            ' 1. Strip off path part
            If bProtectPath Then

                Try
                    ' Find path\file separator position
                    Dim iLastSep As Integer = strText.LastIndexOf("\"c)
                    If iLastSep = -1 Then iLastSep = strText.LastIndexOf("/"c)
                    strPath = strText.Substring(0, iLastSep + 1)
                    strFile = strText.Substring(iLastSep + 1)
                Catch ex As Exception
                    strPath = ""
                    strFile = strText
                End Try

                bProtectPath = Not String.IsNullOrEmpty(strPath)
            Else
                strFile = strText
            End If

            ' Clean up
            'strFile = strText.Replace(" ", "_") ' Spaces are definitely allowed under 32 bit ;-)
            strFile = strFile.Replace("\", "-")
            strFile = strFile.Replace("/", "-")

            ' Replace invalid file name chars with hyphens
            For Each c As Char In Path.GetInvalidPathChars
                If strPath.IndexOf(c) > -1 Then
                    strPath = strPath.Replace(c, "")
                End If
            Next

            ' Replace invalid file name chars with hyphens
            For Each c As Char In Path.GetInvalidFileNameChars
                If strFile.IndexOf(c) > -1 Then
                    strFile = strFile.Replace(c, "-"c)
                End If
            Next

            If bProtectPath Then
                strText = Path.Combine(strPath, strFile)
                ' Replace all accidental 'double dots'
                'removed ".." replacement so ToValidFileName can resolve relative paths
            Else
                strText = strFile
            End If

            Return strText.Trim

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a text into a valid file extension.
        ''' </summary>
        ''' <param name="strText">Text to convert into a file extension.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToValidFileExt(ByVal strText As String, strDefault As String) As String

            If (String.IsNullOrWhiteSpace(strText)) Then strText = strDefault
            If (String.IsNullOrWhiteSpace(strText)) Then Return ""

            If strText(0) <> "."c Then Return "." & strText
            Return strText

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Find a file in a directory.
        ''' </summary>
        ''' <param name="strFile">Name of the file to locate.</param>
        ''' <param name="strPath">Directory to search.</param>
        ''' <param name="bRecursive">Flag stating if subdirectories should be searched recursively.</param>
        ''' <returns>The full path to the file if found, or an empty string if the file could not be located.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FindFile(ByVal strFile As String,
                                        ByVal strPath As String,
                                        Optional ByVal bRecursive As Boolean = False) As String

            Dim strFullPath As String = Path.Combine(strPath, strFile)
            Dim fsec As FileSecurity = Nothing

            Try
                ' Try to be nice
                If File.Exists(strFullPath) Then Return strFullPath
                ' Ok, maybe the file is hidden. Let's be less nice.
                fsec = File.GetAccessControl(strFullPath, AccessControlSections.Group)
                If fsec IsNot Nothing Then
                    Return strFullPath
                End If
            Catch ex As FileNotFoundException
                ' Woops
            End Try

            If bRecursive Then
                For Each strDirectory As String In Directory.GetDirectories(strPath)
                    strFullPath = FindFile(strFile, strDirectory, bRecursive)
                    If Not String.IsNullOrEmpty(strFullPath) Then Return strFullPath
                Next
            End If
            Return ""

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get all files that match a given <see cref="System.Windows.Forms.FileDialog.Filter">dialog filter</see>.
        ''' </summary>
        ''' <param name="astrFiles">The array of files to filter.</param>
        ''' <param name="astrExtensions">Array of file extensions to text.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FilesByDialogFilter(astrFiles() As String, astrExtensions() As String) As String()

            If (astrExtensions Is Nothing) Then Return astrFiles

            Dim hash As New HashSet(Of String)
            Dim lstrFiles As New List(Of String)

            For i As Integer = 0 To astrExtensions.Length - 1
                If Not hash.Contains(astrExtensions(i)) Then hash.Add(astrExtensions(i))
                If astrExtensions(i).Contains(".*") Then Return astrFiles
            Next

            For i As Integer = 0 To astrFiles.Length - 1
                If hash.Contains("*" & Path.GetExtension(astrFiles(i)).ToLower()) Then
                    lstrFiles.Add(astrFiles(i))
                End If
            Next

            Return lstrFiles.ToArray()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Cleans up a string of separated file extensions, eliminating duplicates
        ''' and optionally sorting them.
        ''' </summary>
        ''' <param name="strExt">The <paramref name="cSeparator"/>-separated string of extensions to clean.</param>
        ''' <param name="bIgnoreCase">Flag, indicating whether upper/lower case should be ignored.</param>
        ''' <param name="cSeparator">The character that separates extentions.</param>
        ''' <returns>A merged </returns>
        ''' -------------------------------------------------------------------
        Public Shared Function CleanupExtensions(ByVal strExt As String,
                                                 Optional ByVal bIgnoreCase As Boolean = True,
                                                 Optional ByVal cSeparator As Char = ";"c,
                                                 Optional ByVal bSort As Boolean = True) As String

            Dim sb As New StringBuilder()
            Dim lstrBits As New List(Of String)
            Dim lstrFinal As New List(Of String)

            lstrBits.AddRange(strExt.Split(cSeparator))

            ' Add only unique new bits
            For Each strNew As String In lstrBits
                Dim bUnique As Boolean = True
                For Each strOrg As String In lstrFinal
                    bUnique = bUnique And (String.Compare(strNew, strOrg, bIgnoreCase) <> 0)
                Next
                If (bUnique = True) Then lstrFinal.Add(strNew)
            Next

            ' Sort if needed
            If bSort Then lstrFinal.Sort()

            ' Concoct final list
            For i As Integer = 0 To lstrFinal.Count - 1
                If Not String.IsNullOrWhiteSpace(lstrFinal(i)) Then
                    If (sb.Length > 0) Then sb.Append(cSeparator)
                    sb.Append(lstrFinal(i))
                End If
            Next

            ' Done
            Return sb.ToString

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create a backup copy of a file.
        ''' </summary>
        ''' <param name="strSrc">Source file to copy.</param>
        ''' <param name="strDest">Destination to copy file to. Leave this destination empty 
        ''' to backup to a default location. This parameter will return the backup 
        ''' destination file name.</param>
        ''' <param name="attributes"><see cref="FileAttributes">Attributes</see> to
        ''' assign to the backup file.</param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' If <paramref name="strDest"/> is left empty, a backup file name will be
        ''' created that looks like '[original name].[original ext].[short date]'.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function CreateBackup(ByVal strSrc As String,
                                            ByRef strDest As String,
                                            Optional ByVal attributes As FileAttributes = FileAttributes.Archive Or FileAttributes.NotContentIndexed) As Boolean

            If String.IsNullOrWhiteSpace(strDest) Then
                strDest = strSrc & ".backup_" & ToValidFileName(Date.Now.ToShortDateString, False)
            End If

            Dim strPath As String = (Path.GetDirectoryName(strDest))
            If (Not String.IsNullOrWhiteSpace(strPath)) Then
                If Not cFileUtils.IsDirectoryAvailable(Path.GetDirectoryName(strDest), True) Then
                    Return False
                End If
            End If

            Try
                ' Create backup copy
                File.Copy(strSrc, strDest, True)
                ' Apply attributes
                File.SetAttributes(strDest, attributes)
                Return True
            Catch ex As Exception
                ' Whoah!
            End Try
            Return False

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' List of temporary files generated by <see cref="MakeTempFile"/>.
        ''' <seealso cref="MakeTempFile"/>
        ''' <seealso cref="PurgeTempFile"/>
        ''' <seealso cref="PurgeTempFiles"/>
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Shared g_tempfiles As New List(Of String)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create a random file in the %TEMP% folder, and return the path to the file.
        ''' <seealso cref="PurgeTempFile"/>
        ''' <seealso cref="PurgeTempFiles"/>
        ''' </summary>
        ''' <param name="strExt">An optional file extension to use.</param>
        ''' <returns>The full path to the file.</returns>
        ''' <remarks>
        ''' Individual temporary files should be removed when no longer needed via 
        ''' <see cref="PurgeTempFile"/>. It also does not hurt to call <see cref="PurgeTempFiles"/>
        ''' when your application shuts down.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function MakeTempFile(Optional ByVal strExt As String = "") As String

            ' TODO: Check if file is writeable!!!

            Dim strFileName As String = Path.GetRandomFileName() & strExt
            Dim strPath As String = Path.Combine(System.IO.Path.GetTempPath(), "EwE")
            If Not cFileUtils.IsDirectoryAvailable(strPath, True) Then
                strPath = System.IO.Path.GetTempPath()
            End If
            Dim strFile As String = Path.Combine(strPath, strFileName)
            ' Add to temp file registry
            If Not cFileUtils.g_tempfiles.Contains(strFile) Then cFileUtils.g_tempfiles.Add(strFile)
            ' Done
            Return strFile

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Purge all files created by <see cref="MakeTempFile"/>.
        ''' <seealso cref="MakeTempFile"/>
        ''' <seealso cref="PurgeTempFile"/>
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Shared Sub PurgeTempFiles()
            Dim astrFiles As String() = cFileUtils.g_tempfiles.ToArray
            For Each strTempFile As String In astrFiles
                cFileUtils.PurgeTempFile(strTempFile)
            Next
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Purge a single file created by <see cref="MakeTempFile"/>
        ''' <seealso cref="MakeTempFile"/>
        ''' <seealso cref="PurgeTempFiles"/>
        ''' </summary>
        ''' <param name="strTempFile"></param>
        ''' -----------------------------------------------------------------------
        Public Shared Sub PurgeTempFile(ByVal strTempFile As String)
            Try
                If File.Exists(strTempFile) Then File.Delete(strTempFile)
                cFileUtils.g_tempfiles.Remove(strTempFile)
                ' Console.WriteLine("Purged temp file " & strTempFile)
            Catch ex As Exception
                ' Hmm
            End Try
        End Sub

        Private Const cCHARS_NUMBER As String = "-0123456789E."
        Private Const cCHARS_STRING As String = "-0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_$."
        Private cSeparator As Char = CChar(" ")

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Read a number from a <see cref="TextReader"/> and advances the read pointer.
        ''' </summary>
        ''' <param name="reader">The reader to read the number from.</param>
        ''' <returns>The read number in the form of a <see cref="Single"/></returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function ReadNumber(ByRef reader As TextReader) As Single
            Dim ch(255) As Char ' Should be enough to hold one single number
            Dim readCh(1) As Char
            Dim nChar As Integer = 0

            ' Read leading spaces
            Do
                reader.Read(readCh, 0, 1)
            Loop Until (cCHARS_NUMBER.IndexOfAny(readCh) > -1) Or (reader.Peek() < 0)

            If (reader.Peek() = -1) Then Throw New Exception("Unexpected end of file found while reading body")

            ' Read digits
            Do
                ch(nChar) = readCh(0)
                nChar += 1
                reader.Read(readCh, 0, 1)
            Loop Until (cCHARS_NUMBER.IndexOfAny(readCh) = -1) Or (reader.Peek() < 0)

            Return Single.Parse(ch)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Checks if a directory is available, and optionally tries to create the directory if missing.
        ''' </summary>
        ''' <param name="strDirectory">The directory to check.</param>
        ''' <param name="bCreate">Optional flag, stating whether the directory 
        ''' should be created if it does not exist yet.</param>
        ''' <returns>True if the directory is available.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsDirectoryAvailable(ByVal strDirectory As String,
                                                    Optional ByVal bCreate As Boolean = False) As Boolean

            ' Test if already exists as a file
            If File.Exists(strDirectory) Then Return False

            Dim bExists As Boolean = Directory.Exists(strDirectory)

            If Not bExists Then
                Try
                    If bCreate Then bExists = (Directory.CreateDirectory(strDirectory) IsNot Nothing)
                Catch ex As Exception
                    ' Whoah
                End Try
            End If

            Return bExists

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether a directory does not have any files in it.
        ''' </summary>
        ''' <param name="strDirectory">The directory to check.</param>
        ''' <returns>True if the indicated directory does not have any files in it/</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsDirectoryEmpty(ByVal strDirectory As String) As Boolean
            Return Not Directory.EnumerateFileSystemEntries(strDirectory).Any()
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Convert an absolute path to a relative path.
        ''' </summary>
        ''' <param name="strRoot">The root path to translate the absolute path to.</param>
        ''' <param name="strAbs">The absolute path to translate.</param>
        ''' <returns>A path relative to <paramref name="strRoot"/></returns>
        ''' -----------------------------------------------------------------------
        Shared Function RelativePath(ByVal strRoot As String, ByVal strAbs As String) As String

            Dim astrRoot As String() = NormalizePath(strRoot).Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar)
            Dim astrAbs As String() = NormalizePath(strAbs).Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar)

            Dim nShared As Integer = 0
            For i As Integer = 0 To Math.Min(astrRoot.Length, astrAbs.Length) - 1
                If String.Compare(astrRoot(i), astrAbs(i), True) = 0 Then
                    nShared += 1
                Else
                    Exit For
                End If
            Next i

            If nShared = 0 Then Return strAbs

            Dim sbPathRel As New StringBuilder()
            For i As Integer = nShared To astrRoot.Length - 1
                If (i > nShared) Then sbPathRel.Append(Path.DirectorySeparatorChar)
                sbPathRel.Append("..")
            Next
            'If sbPathRel.Length = 0 Then
            '    sbPathRel.Append(".")
            'End If
            For i As Integer = nShared To astrAbs.Length - 1
                If (sbPathRel.Length > 0) Then sbPathRel.Append(Path.DirectorySeparatorChar)
                sbPathRel.Append(astrAbs(i))
            Next
            Return sbPathRel.ToString

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Recursively delete a directory and everything in it. Dangerous!
        ''' </summary>
        ''' <param name="strPath">The folder to delete.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function DeleteDirectory(strPath As String) As Boolean

            ' Path indicates a file: abort
            If File.Exists(strPath) Then Return False
            If Not Directory.Exists(strPath) Then Return True

            Dim bSucces As Boolean = True

            ' Recursively get rid off all subfolders
            For Each strSubFolder As String In Directory.GetDirectories(strPath)
                bSucces = bSucces And cFileUtils.DeleteDirectory(strSubFolder)
            Next strSubFolder

            ' Now trash the content of this directory
            For Each strFile As String In Directory.GetFiles(strPath)
                Try
                    File.Delete(strFile)
                Catch ex As Exception
                    ' File in use?
                    bSucces = False
                End Try
            Next strFile

            Try
                ' Lastly trash directory itself
                Directory.Delete(strPath)
            Catch ex As Exception
                ' Directory in use?
                bSucces = False
            End Try

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generate a ESRI world file name to accompany a given file name.
        ''' </summary>
        ''' <param name="strFileName"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToWorldFileName(strFileName As String) As String

            ' Use new convention
            ' https://en.wikipedia.org/wiki/World_file
            ' http://webhelp.esri.com/arcgisdesktop/9.3/index.cfm?id=3121&pid=3109&topicname=World%20files%20for%20raster%20datasets&
            Return Path.ChangeExtension(strFileName, Path.GetExtension(strFileName) & "w")

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a DOS path to a UNIX path.
        ''' </summary>
        ''' <param name="strUnixPath"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function UnixToDos(strUnixPath As String) As String

            If String.IsNullOrWhiteSpace(strUnixPath) Then Return String.Empty
            Dim chunks As String() = strUnixPath.Split(New Char() {"/"c}, StringSplitOptions.RemoveEmptyEntries)
            If (chunks.Length > 0) Then
                If chunks(0).Length = 1 Then 'Single character root, assume drive letter.
                    Return String.Join("\", chunks).Insert(1, ":")
                Else
                    Return "\\" & String.Join("\", chunks)
                End If
            Else
                Return IO.Path.DirectorySeparatorChar
            End If

        End Function


        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a DOS path to a UNIX path.
        ''' </summary>
        ''' <param name="strDosPath"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function DosToUnix(strDosPath As String) As String

            If String.IsNullOrWhiteSpace(strDosPath) Then Return String.Empty
            Return strDosPath.Replace("\\", "/").Replace("\", "/")

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Normalize a path to a full path.
        ''' </summary>
        ''' <param name="strPath"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' After http://stackoverflow.com/questions/2281531/how-can-i-compare-directory-paths-in-c
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function NormalizePath(ByVal strPath As String) As String

            ' Sanity checks
            If (String.IsNullOrWhiteSpace(strPath)) Then Return String.Empty
            If Not (strPath.Contains(Path.DirectorySeparatorChar) Or strPath.Contains(Path.AltDirectorySeparatorChar)) Then Return String.Empty
            If Not (Directory.Exists(strPath)) Then Return strPath

            ' Validate paths and folders
            Return Path.GetFullPath(New Uri(strPath).LocalPath)
            ' .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) _
            ' .ToUpperInvariant()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether two paths refer to the same location.
        ''' </summary>
        ''' <param name="strPath1">The first path to compare.</param>
        ''' <param name="strPath2">The second path to compare.</param>
        ''' <param name="bIgnoreCase">Flag, stating comparison can exclude letter casing.</param>
        ''' <returns>True if the two paths refer to the same location.</returns>
        ''' <remarks>
        ''' http://stackoverflow.com/questions/2281531/how-can-i-compare-directory-paths-in-c
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Shadows Function Equals(ByVal strPath1 As String,
                                              ByVal strPath2 As String,
                                              Optional bIgnoreCase As Boolean = True) As Boolean
            Return String.Compare(NormalizePath(strPath1), NormalizePath(strPath2), bIgnoreCase) = 0
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the <see cref="System.Drawing.Imaging.ImageFormat"/> for a file,
        ''' or Nothing if the file extension did not resolve to a known image format.
        ''' </summary>
        ''' <param name="strFileName">Name of file to convert.</param>
        ''' <returns>A imageformat, or nothing if the file extension was not
        ''' recognized as a known format.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ImageFormat(strFileName As String) As ImageFormat

            Dim strExt As String = Path.GetExtension(strFileName)
            Select Case strExt.ToLower()
                Case ".bmp" : Return ImageFormat.Bmp
                Case ".emf" : Return ImageFormat.Emf
                Case ".exit" : Return ImageFormat.Exif
                Case ".gif" : Return ImageFormat.Gif
                Case ".icon", ".ico" : Return ImageFormat.Icon
                Case ".jpg", ".jpeg", ".jp2" : Return ImageFormat.Jpeg
                Case ".png" : Return ImageFormat.Png
                Case ".tiff", ".tif" : Return ImageFormat.Tiff
                Case ".wmf" : Return ImageFormat.Wmf
            End Select
            Return Nothing

        End Function
    End Class

End Namespace
