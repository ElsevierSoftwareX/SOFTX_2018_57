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
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Security.Principal
Imports System.Threading
Imports System.Windows.Forms
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace SystemUtilities

    ''' <summary>
    ''' Utilities for obtaining system properties.
    ''' </summary>
    Public Class cSystemUtils

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of the host machine.
        ''' </summary>
        ''' <returns>The name of the host machine.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function GetHostName() As String
            Return Dns.GetHostName()
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the IP address of the host machine.
        ''' </summary>
        ''' <param name="bIP4">True to get the IP4 address, False to get the IP6 address.</param>
        ''' <returns>The IP address of the host machine.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function GetHostIP(Optional strHost As String = "",
                                         Optional bIP4 As Boolean = True) As String
            If String.IsNullOrWhiteSpace(strHost) Then strHost = GetHostName()
            For Each ip As IPAddress In Dns.GetHostEntry(strHost).AddressList
                Dim by As Byte() = ip.GetAddressBytes
                If bIP4 Then
                    If (by.Length = 4) And (by(0) <> 169) Then
                        Return ip.ToString
                    End If
                Else
                    If (by.Length = 16) Then
                        Return ip.ToString
                    End If
                End If
            Next
            Return ""
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Checks if a given host name or IP address is local. First, it gets all 
        ''' IP addresses of the given host, then it gets all IP addresses of the 
        ''' local computer and finally it compares both lists. If any host IP equals 
        ''' to any of local IPs, the host is a local IP. It also checks whether the 
        ''' host is a loopback address (localhost / 127.0.0.1).
        ''' </summary>
        ''' <param name="strHost">Host name or IP address to check.</param>
        ''' <returns>True if the host denotes a local IP</returns>
        ''' <remarks>
        ''' Converted from http://www.csharp-examples.net/local-ip/
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsLocalIP(strHost As String) As Boolean
            Try
                ' get host IP addresses
                Dim hostIPs As IPAddress() = Dns.GetHostAddresses(strHost)
                ' get local IP addresses
                Dim localIPs As IPAddress() = Dns.GetHostAddresses(Dns.GetHostName())

                ' test if any host IP equals to any local IP or to localhost
                For Each hostIP As IPAddress In hostIPs
                    ' is localhost
                    If IPAddress.IsLoopback(hostIP) Then
                        Return True
                    End If
                    ' is local address
                    For Each localIP As IPAddress In localIPs
                        If hostIP.Equals(localIP) Then
                            Return True
                        End If
                    Next
                Next
            Catch
            End Try
            Return False
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>True if an internet connection has been detected.</returns>
        ''' <remarks>
        ''' For original article, see http://stackoverflow.com/questions/8800119/check-internet-connectivity.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsConnectedToInternet() As Boolean
            Try
                Using client As New WebClient()
                    Using stream As IDisposable = client.OpenRead("http://www.google.com")
                        Return True
                    End Using
                End Using
            Catch
                Return False
            End Try
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Function that execute external applications for all plug-ins
        ''' </summary>
        ''' <param name="strAppName">Name of the executable to execute (including extension)</param>
        ''' <param name="strPath">Application path to use. Provide an empty string
        ''' here to detect the application file in all possible locations.</param>
        ''' <param name="strOutputParameters">Arguments to pass to the executable.</param>
        ''' -----------------------------------------------------------------------
        Public Shared Function AppExec(ByVal strAppName As String,
                                       ByVal strOutputParameters As String,
                                       ByRef strError As String,
                                       Optional ByVal strPath As String = "") As Boolean

            ' Check if Directory is forced 
            If Not String.IsNullOrEmpty(strPath) Then
                Return ExecuteApplication(strPath, strAppName, strOutputParameters, strError)
            Else
                ' Loop through all the file locations to find the files
                For Each strLocation As String In ApplicationLaunchLocations()
                    ' Execute without directory parameter
                    If ExecuteApplication(strLocation, strAppName, strOutputParameters, strError) Then Return True
                Next
            End If
            Return False

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns an array of possible application locations.</summary>
        ''' <returns>
        ''' </returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function ApplicationLaunchLocations() As String()
            Return New String() {Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(7),
                                 Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) & "\Ecopath"}
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' [to document]
        ''' </summary>
        ''' <param name="strLocationDir"></param>
        ''' <param name="strAppName"></param>
        ''' <param name="strOutputParameters"></param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Private Shared Function ExecuteApplication(ByVal strLocationDir As String,
                                                   ByVal strAppName As String,
                                                   ByVal strOutputParameters As String,
                                                   ByRef strError As String) As Boolean
            Dim proc As New System.Diagnostics.Process()
            Dim bSuccess As Boolean = False
            Dim strFullAppPath As String = ""

            ' Preserve the current directory
            Dim strTemDir As String = Environment.CurrentDirectory

            Try
                Environment.CurrentDirectory = strLocationDir
                strFullAppPath = Path.Combine(strLocationDir, strAppName)
                'Check if the application EcoPath install this application or it was deleted
                If Not File.Exists(strFullAppPath) Then
                    bSuccess = False
                Else
                    'Execute external application
                    proc.EnableRaisingEvents = False
                    proc.StartInfo.FileName = strFullAppPath
                    proc.StartInfo.Arguments = strOutputParameters
                    proc.Start()

                    bSuccess = True
                End If

            Catch ex As Exception
                ' JS 19ap09 (happy 4th birthday Sascha!) do not throw exception; the calling code is not ready for this
                'Throw New Exception(String.Format("Failed to load {0} with parameters {1}.  Please check if the application exist and reinstall the application.  If the issue still persist contact your application vendor.  Error: {2}.", _
                '                                   strFullAppPath, strOutputFileName, ex.ToString))
                strError = ex.Message
                ' Fix faulty Win7 exception text
                If strError.IndexOf("%1") > -1 Then
                    strError = strError.Replace("%1", strAppName)
                End If
                bSuccess = False
            End Try

            ' Restore the current directory
            Environment.CurrentDirectory = strTemDir

            Return bSuccess
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether this application is running in 64 bit mode.
        ''' </summary>
        ''' <returns>True if running in 64 bit mode.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function Is64BitProcess() As Boolean

            ' This flag was introduced in .NET framework 4
            Return Environment.Is64BitProcess
            'Return (System.Runtime.InteropServices.Marshal.SizeOf(GetType(IntPtr)) = 8)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether this application is running under Windows.
        ''' </summary>
        ''' <returns>True if running under Windows.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsWindows() As Boolean

            Select Case cSystemUtils.Platform
                Case PlatformID.MacOSX,
                     PlatformID.Unix,
                     PlatformID.Xbox
                    Return False
                Case PlatformID.Win32NT,
                     PlatformID.Win32S,
                     PlatformID.Win32Windows,
                     PlatformID.WinCE
                    Return True
                Case Else
                    Debug.Assert(False, "Unknown platform ID " & Environment.OSVersion.Platform.ToString)
            End Select
            Return False

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the <see cref="PlatformID"/> of the running computer.
        ''' </summary>
        ''' <returns>The <see cref="PlatformID"/> of the running computer.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function Platform() As System.PlatformID
            Return Environment.OSVersion.Platform
        End Function


        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns a OS version and service pack description of the running computer.
        ''' </summary>
        ''' <returns>The OS version and service pack description of the running computer.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function OSVersion() As String
            Return Environment.OSVersion.VersionString
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether this application is running in a remote desktop session.
        ''' </summary>
        ''' <returns>True if this application was running in a remote desktop session.</returns>
        ''' <remarks>This test should also properly detect VNC. Other protocols have
        ''' not been evaluated.</remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsRDC() As Boolean

            Return System.Windows.Forms.SystemInformation.TerminalServerSession

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the current UI culture is right-to-left ordered.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function IsRightToLeft() As Boolean
            Dim ci As CultureInfo = Thread.CurrentThread.CurrentUICulture
            Return ci.TextInfo.IsRightToLeft
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the OS is 64 bit.
        ''' </summary>
        ''' <returns>True if the OS is 64 bit.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function Is64BitOS() As Boolean

            Return System.Environment.Is64BitOperatingSystem

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether this application is running with administrator privileges.
        ''' </summary>
        ''' <returns>True if running with administrator privileges.</returns>
        ''' <remarks>
        ''' http://www.codekeep.net/snippets/16758a1f-6186-47a7-98ba-30449fe74cda.aspx
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsAdministrator() As Boolean

            Dim identity As WindowsIdentity = WindowsIdentity.GetCurrent()
            Dim principal As New WindowsPrincipal(identity)
            Return principal.IsInRole(WindowsBuiltInRole.Administrator)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' States if running Win 7 or higher.
        ''' </summary>
        ''' <returns>True if running Win 7 or higher.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsRunningWin7OrHigher() As Boolean

            Dim os As System.OperatingSystem = System.Environment.OSVersion
            Dim ver As Version = os.Version

            If (os.Platform <> PlatformID.Win32NT) Then Return False
            If (ver.Major < 6) Then Return False
            Return True

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the system is running on battery power.
        ''' </summary>
        ''' <returns>True if the system is running on battery power.</returns>
        ''' <remarks>
        ''' http://stackoverflow.com/questions/241142/c-sharp-net-how-to-check-if-were-running-on-battery
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function IsBatteryPower() As Boolean
            Return (PowerLineStatus.Offline = SystemInformation.PowerStatus.PowerLineStatus)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the name of the folder to store application settings. Please note 
        ''' that this value needs to be set before an application framework loads its 
        ''' settings. If left empty, a default is obtained from the launching 
        ''' assembly information.
        ''' </summary>
        ''' <seealso cref="ApplicationSettingsPath"/>
        ''' -----------------------------------------------------------------------
        Public Shared Property ApplicationSettingsFolderName As String = ""

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the path for storing application settings.
        ''' </summary>
        ''' <param name="bPerUserSetting">States if this is a per-user setting
        ''' (True) or a setting for all users (False).</param>
        ''' <returns>The path</returns>
        ''' <seealso cref="ApplicationSettingsFolderName"/>
        ''' -------------------------------------------------------------------
        Public Shared Function ApplicationSettingsPath(Optional ByVal bPerUserSetting As Boolean = True) As String

            Dim strBaseDir As String = ""
            Dim strPath As String = ""

            If bPerUserSetting Then
                strBaseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            Else
                strBaseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            End If

            If (String.IsNullOrEmpty(ApplicationSettingsFolderName)) Then
                ' Prefer product name, but uif that does not work, use assembly name
                ApplicationSettingsFolderName = My.Application.Info.ProductName
                If (String.IsNullOrEmpty(ApplicationSettingsFolderName)) Then
                    ApplicationSettingsFolderName = My.Application.Info.AssemblyName
                End If
            End If

            strPath = Path.Combine(strBaseDir, cFileUtils.ToValidFileName(ApplicationSettingsFolderName, False))

            cFileUtils.IsDirectoryAvailable(strPath, True)

            Return strPath

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns one of two values, depending on the evaluation of an expression. 
        ''' This is a geenric-typed alternative to Microsoft.VisualBasic IIF.
        ''' </summary>
        ''' <param name="bValue">The value to test.</param>
        ''' <param name="cTrue">Value to return if <paramref name="bValue"/> resolved to True.</param>
        ''' <param name="cFalse">Value to return if <paramref name="bValue"/> resolved to False.</param>
        ''' <returns>A value.</returns>
        ''' <remarks>
        ''' The Microsoft.VisualBasic namespace is known to cause problems under Mono. If
        ''' Mono-compliance is required do not reference Microsoft.VisualBasic and use this method instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        <Obsolete("Use If(condition, truepart, falsepart) construction instead")>
        Public Shared Function IIF(Of T)(ByVal bValue As Boolean, ByVal cTrue As T, ByVal cFalse As T) As T
            Return If(bValue, cTrue, cFalse)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the numbers contained in a string as a numeric value of appropriate type.
        ''' This is a strong-typed alternative to Microsoft.VisualBasic Val.
        ''' </summary>
        ''' <param name="strValue">The string to convert.</param>
        ''' <returns>A Double value, or 0 if the conversion failed.</returns>
        ''' <remarks>
        ''' The Microsoft.VisualBasic namespace is known to cause problems under Mono. If
        ''' Mono-compliance is required do not reference Microsoft.VisualBasic and use this method instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function Val(ByVal strValue As String) As Double
            Dim dVal As Double = 0
            ' Does not handle Exponent values!
            'Dim m As Match = Regex.Match(strValue, "^-?[\d\s]*(\.[\d\s]+|[\d\s]*)")
            'If (m.Value <> "") Then
            '    Return Convert.ToDouble(Regex.Replace(m.Value, "\s+", ""))
            'End If
            Double.TryParse(strValue, dVal)
            Return dVal
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the numbers contained in an object as a numeric value of appropriate type.
        ''' This is a strong-typed alternative to Microsoft.VisualBasic Val.
        ''' </summary>
        ''' <param name="value">The object to convert.</param>
        ''' <returns>A Double value, or 0 if the conversion failed.</returns>
        ''' <remarks>
        ''' The Microsoft.VisualBasic namespace is known to cause problems under Mono. If
        ''' Mono-compliance is required do not reference Microsoft.VisualBasic and use this method instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function Val(ByVal value As Object) As Double
            Return Val(value.ToString)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the number contained in a character as a numeric value of appropriate type.
        ''' This is a strong-typed alternative to Microsoft.VisualBasic Val.
        ''' </summary>
        ''' <param name="value">The character to convert.</param>
        ''' <returns>A Double value, or 0 if the conversion failed.</returns>
        ''' <remarks>
        ''' The Microsoft.VisualBasic namespace is known to cause problems under Mono. If
        ''' Mono-compliance is required do not reference Microsoft.VisualBasic and use this method instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function Val(ByVal value As Char) As Integer
            If Char.IsDigit(value) Then
                Return Convert.ToInt32(value)
            End If
            Return 0
        End Function

#Region " Discontinued "

        <Obsolete("Please use Is64BitProcess instead")> _
        Public Shared Function Is64Bit() As Boolean
            Return cSystemUtils.Is64BitProcess
        End Function

#End Region ' Discontinued

    End Class

End Namespace ' SystemUtilities
