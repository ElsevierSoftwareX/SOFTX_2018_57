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

Imports EwEUtils.Utilities

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
' Copyright 1991- UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'
#Region " Imports "

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Helper class, parses a setting string into ampersand parameter/value pairs.
''' The original string may not contain parameter info, and is maintained as long
''' as no parameters are set.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cSettingsParser

#Region " Private vars "

    Private m_strBuffer As String = ""
    Private m_dtParams As New Dictionary(Of String, String)

#End Region ' Private vars

#Region " Construction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="strBuffer">The initial values to parse, if any.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(Optional strBuffer As String = "")
        Me.Buffer = strBuffer
    End Sub

#End Region ' Construction

#Region " Public bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Cast an instance of this class to a string.
    ''' </summary>
    ''' <param name="parser">The instance to cast.</param>
    ''' <returns>The internal <see cref="Buffer"/> within the parser.</returns>
    ''' -----------------------------------------------------------------------
    Public Shared Widening Operator CType(ByVal parser As cSettingsParser) As String
        Return parser.Buffer
    End Operator

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Cast a buffer string to a populated parser.
    ''' </summary>
    ''' <param name="strBuffer">The string to cast.</param>
    ''' <returns>A populated parser that has interpreted the contents of the 
    ''' string into paramater/value pairs.</returns>
    ''' -----------------------------------------------------------------------
    Public Shared Narrowing Operator CType(ByVal strBuffer As String) As cSettingsParser
        Return New cSettingsParser(strBuffer)
    End Operator

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the settings string.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Buffer() As String
        Get
            Return Me.m_strBuffer
        End Get
        Set(value As String)
            Me.m_strBuffer = value
            Me.Decode()
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set a parameter in the settings.
    ''' </summary>
    ''' <param name="strName">The name of the parameter to acces. Names are
    ''' case-sensitive.</param>
    ''' <remarks>
    ''' Note that setting a parameter will replace the content of the original 
    ''' <see cref="Buffer"/> with a paremeterized version of the data.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Default Public Property Parameter(strName As String, Optional strDefault As String = "") As String
        Get
            strName = cFileUtils.ToValidFileName(strName, False).ToLower
            If (Me.m_dtParams.ContainsKey(strName)) Then
                Return Me.m_dtParams(strName)
            End If
            Return strDefault
        End Get
        Set(value As String)
            strName = cFileUtils.ToValidFileName(strName, False).ToLower
            Me.m_dtParams(strName) = value
            Me.Encode()
        End Set
    End Property

#End Region ' Public bits

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Decode the <see cref="Buffer"/> for parameter/value pairs.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub Decode()

        Me.m_dtParams.Clear()
        Dim astrBits As String() = Me.m_strBuffer.Split("&"c)
        For Each strBit As String In astrBits
            Dim astrvals As String() = strBit.Split("="c)
            If (astrvals.Length = 2) Then
                Me.m_dtParams(astrvals(0).ToLower) = astrvals(1)
            End If
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Convert the parameter/pair disctionary into a single string.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Sub Encode()

        Dim sb As New System.Text.StringBuilder()
        For Each strParam As String In Me.m_dtParams.Keys
            If (sb.Length > 0) Then sb.Append("&")
            sb.Append(strParam)
            sb.Append("=")
            sb.Append(Me.m_dtParams(strParam))
        Next
        Me.m_strBuffer = sb.ToString

    End Sub

#End Region ' Internals

End Class