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
Imports System
Imports System.Collections.Generic
Imports EwEUtils.Core
Imports System.Security.Policy
Imports System.Security.Permissions

#End Region ' Imports

Namespace Utilities

    ''' =======================================================================
    ''' <summary>
    ''' Helper class offering miscellaneous Assemblyname-related functionalities.
    ''' </summary>
    ''' =======================================================================
    Public Class cAssemblyUtils

#Region " Internal helper classes "

        ''' <summary>Microsoft assembly name prefixes (not complete but hey, it's a start).</summary>
        Private Shared s_FrameworkPrefixes() As String = {"mscorlib", "system", "microsoft", "interop", "accessibility", "office", "stdole"}
        ''' <summary>EwE assembly name prefixes.</summary>
        Private Shared s_CoreNames As String() = New String() {"EwEUtils", "EwEPlugin", "EwECore", "ScientificInterfaceShared", "EwE6"}
        ''' <summary>For quick look-up</summary>
        Private Shared m_cache As New cAssemblyStateCache()

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper class to sort an assembly name list.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class AssemblyNameComparer
            Implements IComparer(Of AssemblyName)

            Public Function Compare(ByVal x As System.Reflection.AssemblyName, ByVal y As System.Reflection.AssemblyName) As Integer _
                Implements System.Collections.Generic.IComparer(Of System.Reflection.AssemblyName).Compare

                Dim i As Integer = String.Compare(x.Name, y.Name)
                If (i = 0) Then
                    i = x.Version.CompareTo(y.Version)
                End If
                Return i
            End Function
        End Class

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper class to maintain the EwE state of a single assembly.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cAssemblyState
            Public Property IsEwE As TriState = TriState.UseDefault
            Public Property IsEwECore As TriState = TriState.UseDefault
            Public Property IsEwEExt As TriState = TriState.UseDefault
            Public Property IsFramework As TriState = TriState.UseDefault
        End Class

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper class to cache the EwE state for a range of assemblies.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cAssemblyStateCache
            Private m_info As Dictionary(Of String, cAssemblyState) = Nothing

            Public Sub New()
                Me.m_info = New Dictionary(Of String, cAssemblyState)
            End Sub

            Public Function Item(an As AssemblyName) As cAssemblyState
                Dim strName As String = an.FullName
                If Not Me.m_info.ContainsKey(strName) Then
                    Me.m_info(strName) = New cAssemblyState()
                End If
                Return Me.m_info(strName)
            End Function

            Public Function IsCached(an As AssemblyName) As Boolean
                Return Me.m_info.ContainsKey(an.FullName)
            End Function

        End Class

#End Region ' Internal helper classes

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Gets the executing assembly.
        ''' </summary>
        ''' <value>The executing assembly.</value>
        ''' -----------------------------------------------------------------------
        Public Shared ReadOnly Property ExecutingAssembly() As System.Reflection.Assembly
            Get
                Return Assembly.GetExecutingAssembly()
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the short name of an assembly.
        ''' </summary>
        ''' <param name="an">The <see cref="AssemblyName">AssemblyName</see> to return
        ''' the name for.</param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function GetName(ByVal an As AssemblyName) As String
            If (an Is Nothing) Then Return String.Empty
            Return an.Name
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Extracts the public key token of an assembly and returns it as a string.
        ''' </summary>
        ''' <param name="an">The <see cref="AssemblyName">AssemblyName</see> to return
        ''' the public key token for.</param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function GetToken(ByVal an As AssemblyName) As String
            If (an Is Nothing) Then Return String.Empty
            Return cStringUtils.ToHexString(an.GetPublicKeyToken())
        End Function


        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the version number of an assembly.
        ''' </summary>
        ''' <param name="an">The <see cref="AssemblyName">AssemblyName</see> to return
        ''' the version for. If not specified, the version of the 
        ''' <see cref="Assembly.GetExecutingAssembly">executing assembly</see> is returned.</param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function GetVersion(Optional ByVal an As AssemblyName = Nothing) As Version
            If (an Is Nothing) Then
                an = ExecutingAssembly.GetName
            End If
            Return an.Version
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Gets the compile date of the <see cref="ExecutingAssembly">currently 
        ''' executing assembly</see>.
        ''' </summary>
        ''' <value>The compile date.</value>
        ''' -----------------------------------------------------------------------
        Public Shared ReadOnly Property GetCompileDate(Optional ByVal an As AssemblyName = Nothing) As System.DateTime
            Get
                Dim ass As Assembly = Nothing

                Dim strFile As String = ""
                If (an Is Nothing) Then
                    ass = ExecutingAssembly
                Else
                    ass = Assembly.Load(an)
                End If
                Dim dt As DateTime = RetrieveLinkerTimestamp(ass.Location)
                If (dt = Nothing) Then dt = New DateTime()
                Return dt
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get an assembly name for a class type.
        ''' </summary>
        ''' <param name="tclass">The class to return the defining assembly name for.</param>
        ''' <returns>An AssemblyName, or nothing if the class type could not be resolved.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function GetAssemblyName(ByVal tclass As Type) As AssemblyName

            If (tclass Is Nothing) Then Return Nothing
            Dim ass As Assembly = Assembly.GetAssembly(tclass)
            If (ass Is Nothing) Then Return Nothing
            Return ass.GetName()

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Bitwise flags for obtaining assembly information.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Enum eSummaryFlags As Byte
            ''' <summary>EwE core assemblies.</summary>
            EwECore = 1
            ''' <summary>Assemblies built on EwE, but not part of <see cref="eSummaryFlags.EwECore"/>.</summary>
            EwEExtended = 2
            ''' <summary>.NET Framework assemblies.</summary>
            Framework = 4
            ''' <summary>Referenced assemblies.</summary>
            Referenced = 8
            ''' <summary>All possible assemblies.</summary>
            All = 255
        End Enum

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Reports all <see cref="AssemblyName">assemblies</see> referenced by the
        ''' current <see cref="AppDomain">application domain</see>.
        ''' </summary>
        ''' <param name="flags">Bitwise combination of <see cref="eSummaryFlags">summary
        ''' flags</see>, stating that assemblies should be included in the summary.</param>
        ''' <remarks>
        ''' The array of assembly names will be sorted by name.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function GetSummary(Optional flags As eSummaryFlags = eSummaryFlags.EwECore Or eSummaryFlags.EwEExtended) As AssemblyName()

            Dim hsh As New HashSet(Of String)
            Dim lSummary As New List(Of AssemblyName)
            Dim strFullName As String = ""

            ' Get a summary for all loaded assemblies
            For Each ass As Assembly In AppDomain.CurrentDomain.GetAssemblies()
                For Each an As AssemblyName In GetSummary(ass, flags)
                    strFullName = an.FullName
                    If Not hsh.Contains(strFullName) Then
                        lSummary.Add(an)
                        hsh.Add(strFullName)
                    End If
                Next
            Next

            lSummary.Sort(New AssemblyNameComparer())

            Return lSummary.ToArray

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Recursively reports all <see cref="AssemblyName">assemblies</see> referenced 
        ''' by a given assembly.
        ''' </summary>
        ''' <param name="entry">The entry assembly to find the summary of referenced
        ''' assemblies for.</param>
        ''' <param name="flags">Bitwise combination of <see cref="eSummaryFlags">summary
        ''' flags</see>, stating that assemblies should be included in the summary.</param>
        ''' <remarks>
        ''' The array of assembly names will be sorted by name.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Shared Function GetSummary(ByVal entry As Assembly, _
                                           Optional flags As eSummaryFlags = eSummaryFlags.EwECore Or eSummaryFlags.EwEExtended) As AssemblyName()

            ' List to hold collected summary data
            Dim lAssemblies As New List(Of AssemblyName)

            If (entry Is Nothing) Then
                Return lAssemblies.ToArray()
            End If

            Dim an As AssemblyName = entry.GetName()
            Dim bAddAssembly As Boolean = False
            Dim bIsEwECore As Boolean = IsEwECore(an)
            Dim bIsEwEExt As Boolean = IsEwEExternal(an)
            Dim bIsFramework As Boolean = IsFramework(an)
            Dim bIsReferenced As Boolean = (Not bIsEwECore) And (Not bIsEwEExt) And (Not bIsFramework)

            If (flags And eSummaryFlags.EwECore) = eSummaryFlags.EwECore Then
                bAddAssembly = bAddAssembly Or bIsEwECore
            End If

            If (flags And eSummaryFlags.EwEExtended) = eSummaryFlags.EwEExtended Then
                bAddAssembly = bAddAssembly Or bIsEwEExt
            End If

            If (flags And eSummaryFlags.Framework) = eSummaryFlags.Framework Then
                bAddAssembly = bAddAssembly Or bIsFramework
            End If

            If (flags And eSummaryFlags.Referenced) = eSummaryFlags.Referenced Then
                bAddAssembly = bAddAssembly Or bIsReferenced
            End If

            ' Is one we're after?
            If bAddAssembly Then
                ' #Yes: add to list
                lAssemblies.Add(an)
                ' Consider all referenced assemblies too
                For Each an In entry.GetReferencedAssemblies()
                    ' Do not too much work
                    If Not m_cache.IsCached(an) Then
                        lAssemblies.AddRange(GetSummary(GetAssembly(an), flags))
                    End If
                Next
            End If

            Return lAssemblies.ToArray()

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether an <see cref="AssemblyName"/> is part of the .NET framework.
        ''' </summary>
        ''' <param name="an">The <see cref="AssemblyName"/> to test.</param>
        ''' <returns>True if the <see cref="AssemblyName"/> is part of the .NET framework.</returns>
        ''' <remarks>
        ''' After http://stackoverflow.com/questions/2066041/any-way-to-check-if-an-assembly-is-a-framework-assembly-in-net-other-than-che
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsFramework(an As AssemblyName) As Boolean

            If (an Is Nothing) Then Return False

            ' Get cached info
            Dim info As cAssemblyState = m_cache.Item(an)

            ' Not determined yet?
            If (info.IsFramework = TriState.UseDefault) Then
                ' #Yes: assume the worst
                info.IsFramework = TriState.False
                ' Assess framework state
                If an.FullName.Contains("PublicKeyToken=b77a5c561934e089") Then
                    info.IsFramework = TriState.True
                Else
                    For Each strName As String In s_FrameworkPrefixes
                        ' Does name begin with a blacklisted string?
                        If (an.FullName.ToLower().IndexOf(strName) = 0) Then
                            ' #Yes: got one
                            info.IsFramework = TriState.True
                        End If
                    Next
                End If
            End If

            Return (info.IsFramework = TriState.True)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return whether a given assembly name is built upon one of the EwE core assemblies.
        ''' </summary>
        ''' <param name="an">The assembly name to check.</param>
        ''' <returns>True if the assembly is built upon the EwE assemblies, but is not a 
        ''' part of the EwE core libraries.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function IsEwEExternal(an As AssemblyName) As Boolean

            If (an Is Nothing) Then Return False

            ' Get cached info
            Dim info As cAssemblyState = m_cache.Item(an)

            ' Not determined yet?
            If (info.IsEwEExt = TriState.UseDefault) Then
                ' #Yes: assume the worst
                info.IsEwEExt = TriState.False
                ' Is an EwE assembly?
                If IsEwE(an) Then
                    ' #Yes: is not a known name?
                    If (Array.IndexOf(s_CoreNames, an.Name) < 0) Then
                        ' Okidoki
                        info.IsEwEExt = TriState.True
                    End If
                End If
            End If

            Return (info.IsEwEExt = TriState.True)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return whether a given assembly name is one of the EwE core assemblies.
        ''' </summary>
        ''' <param name="an">The assembly name to check.</param>
        ''' <returns>True if the assembly is one of the EwE assemblies.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function IsEwECore(an As AssemblyName) As Boolean

            If (an Is Nothing) Then Return False

            ' Get cached info
            Dim info As cAssemblyState = m_cache.Item(an)

            ' Not determined yet?
            If (info.IsEwECore = TriState.UseDefault) Then
                ' #Yes: assume the worst
                info.IsEwECore = TriState.False
                ' Is an EwE assembly?
                If IsEwE(an) Then
                    ' #Yes: is not a known name?
                    If (Array.IndexOf(s_CoreNames, an.Name) >= 0) Then
                        ' Okidoki
                        info.IsEwECore = TriState.True
                    End If
                End If
            End If

            Return (info.IsEwECore = TriState.True)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a StrongName that matches a specific assembly.
        ''' </summary>
        ''' <param name="an">Assembly name to create a StrongName for.</param>
        ''' <returns>A StrongName that matches the given assembly, or Nothing
        ''' if the assembly was not strongly named.</returns>
        ''' <remarks>
        ''' Adapted from http://blogs.msdn.com/b/shawnfa/archive/2005/08/08/449050.aspx
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function GetStrongName(an As AssemblyName) As StrongName

            ' Test if assembly is present
            If (an Is Nothing) Then Return Nothing

            ' Get the public key blob
            Dim publicKey As Byte() = an.GetPublicKey()

            ' Test if assembly is strongly named
            If (publicKey Is Nothing) Then Return Nothing
            If (publicKey.Length = 0) Then Return Nothing

            ' Create the StrongName
            Dim keyBlob As New StrongNamePublicKeyBlob(publicKey)
            Return New StrongName(keyBlob, an.Name, an.Version)

        End Function

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Retrieves the linker timestamp, as written in the assembly header file
        ''' at a fixed position. This may fail one day in future .NET versions.
        ''' Ideally, the link date and time would be stored in a universal time
        ''' format in the code by the compiler.
        ''' </summary>
        ''' <param name="strAssemblyPath">Path of the assembly file to read the
        ''' build time from.</param>
        ''' <returns>The build date.</returns>
        ''' <remarks>
        ''' Taken from http://www.codinghorror.com/blog/2005/04/determining-build-date-the-hard-way.html
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Shared Function RetrieveLinkerTimestamp(strAssemblyPath As String) As System.DateTime

            Const peHeaderOffset As Integer = 60
            Const linkerTimestampOffset As Integer = 8
            Dim b(2047) As Byte
            Dim s As System.IO.FileStream = Nothing

            Try
                s = New System.IO.FileStream(strAssemblyPath, System.IO.FileMode.Open, System.IO.FileAccess.Read)
                s.Read(b, 0, 2048)
            Finally
                If s IsNot Nothing Then
                    s.Close()
                End If
            End Try
            Dim dt As New System.DateTime(1970, 1, 1, 0, 0, 0)

            dt = dt.AddSeconds(System.BitConverter.ToInt32(b, System.BitConverter.ToInt32(b, peHeaderOffset) + linkerTimestampOffset))
            Return dt.AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether a given assembly is either an EwE core assembly, or
        ''' references the EwEUtils core assembly.
        ''' </summary>
        ''' <param name="an"></param>
        ''' <returns>True if the given assembly is either an EwE core assembly, 
        ''' or references the EwEUtils core assembly.</returns>
        ''' -----------------------------------------------------------------------
        Private Shared Function IsEwE(an As AssemblyName) As Boolean

            If (an Is Nothing) Then Return False

            ' Get cached info
            Dim info As cAssemblyState = m_cache.Item(an)

            ' Not determined yet?
            If (info.IsEwE = TriState.UseDefault) Then

                ' Assume the worst
                info.IsEwE = TriState.False

                ' Get EwEUtils assembly. All EwE assemblies refer to EwEUtils
                Dim anEwEUtils As AssemblyName = GetType(cAssemblyUtils).Assembly().GetName

                ' Ok if this as = EwEUtils
                If CompareNames(anEwEUtils, an) Then
                    info.IsEwE = TriState.True
                    info.IsEwECore = TriState.True
                Else
                    ' Check if the assembly for 'an' refers to EwEUtils
                    ' For this, we'll first need to find each assembly for the given assembly name. *sigh*
                    For Each ass As Assembly In AppDomain.CurrentDomain.GetAssemblies
                        ' Got one of ours?
                        If CompareNames(ass.GetName, an) Then
                            ' #Yes: now check if this 
                            For Each anTest As AssemblyName In ass.GetReferencedAssemblies
                                If CompareNames(anTest, anEwEUtils) Then
                                    info.IsEwE = TriState.True
                                    Exit For
                                End If
                            Next
                        End If
                    Next
                End If
            End If

            Return (info.IsEwE = TriState.True)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether two assembly names can be considered equal.
        ''' </summary>
        ''' <param name="an1"></param>
        ''' <param name="an2"></param>
        ''' <returns>True if the names equal.</returns>
        ''' <remarks>
        ''' This code does not use 
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Shared Function CompareNames(ByVal an1 As AssemblyName, ByVal an2 As AssemblyName) As Boolean
            If (an1 Is Nothing) Or (an2 Is Nothing) Then Return False
            Return (String.Compare(an1.FullName, an2.FullName, True) = 0)
        End Function

        Private Shared Function GetAssembly(an As AssemblyName) As Assembly

            For Each ass As Assembly In AppDomain.CurrentDomain.GetAssemblies()
                If CompareNames(ass.GetName, an) Then
                    Return ass
                End If
            Next
            Return Nothing

        End Function

#End Region

    End Class

End Namespace
