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
Imports System.Security.Cryptography
Imports System.Text

#End Region ' Imports

Namespace Utilities

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Class providing encryption utility methods.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cEncryptionUtilities

        Public Shared Function MD5(ByVal strToHash As String) As String

            Return cEncryptionUtilities.MD5(System.Text.Encoding.ASCII.GetBytes(strToHash))

        End Function

        Public Shared Function MD5(ByVal data As Byte()) As String

            If (data Is Nothing) Then Return ""

            Dim md5Obj As New System.Security.Cryptography.MD5CryptoServiceProvider()
            Dim hash() As Byte = md5Obj.ComputeHash(data)
            Dim sbHash As New StringBuilder()

            For Each b As Byte In hash
                sbHash.Append(b.ToString("x2"))
            Next

            Return sbHash.ToString

        End Function

    End Class

End Namespace
