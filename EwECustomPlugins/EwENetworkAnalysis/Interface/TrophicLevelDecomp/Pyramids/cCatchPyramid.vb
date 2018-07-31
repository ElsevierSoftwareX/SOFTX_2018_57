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

Imports EwECore
Imports System.IO
Imports System.Globalization
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Content manager derived class to invoke the NA catch pyramid interface.
''' </summary>
''' ---------------------------------------------------------------------------
<CLSCompliant(False)> _
Public Class cCatchPyramid
    Inherits cPyramid

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="cPyramid.PyramidType"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Function PyramidType() As modUtility.ePyramidTypes
        Return ePyramidTypes.Catch
    End Function

    Public Overrides Function PageTitle() As String
        Return "Catch pyramid (external)"
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="cPyramid.WritePyramidFile"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Function WritePyramidFile(ByVal core As cCore, _
                                               ByVal sw As System.IO.StreamWriter, _
                                               ByVal ci As CultureInfo) As Boolean

        Dim iMaxTL As Integer
        Dim iFlag As Integer
        Dim bSucces As Boolean = True

        Try

            iFlag = 1
            sw.Write(Format(iFlag, "0"))

            iMaxTL = CInt(IIf(NetworkManager.nTrophicLevels > 9, 9, NetworkManager.nTrophicLevels))
            sw.WriteLine(Format(iMaxTL, "0"))

            'If Not (currUnitIndex = 6 Or currUnitIndex = 9) Then
            'Print #fnum, Trim(currUnitName);
            'Else
            'Print #fnum, Trim(currUnitName);
            'End If
            'Print #fnum, "/";
            'Print #fnum, Trim(TimeUnitName)
            sw.WriteLine("t/km²/year")

            sw.WriteLine(NetworkManager.TotalCatch.ToString("00000000.000", ci))

            For i As Integer = 1 To iMaxTL
                Dim sngTemp As Single
                sngTemp = NetworkManager.CatchByTrophicLevel(i)
                If Math.Abs(sngTemp) > 0.001 Then
                    sw.Write(sngTemp.ToString("00000000.000", ci))
                Else
                    sw.Write(0.ToString("00000000.000", ci))
                End If

                If i < iMaxTL And sngTemp > 0 Then
                    sngTemp = NetworkManager.CatchByTrophicLevel(i) / sngTemp
                Else
                    sngTemp = 0
                End If
                sw.WriteLine(sngTemp.ToString("00000000.000", ci))
            Next i

        Catch ex As Exception
            bSucces = False
        End Try

        Return bSucces

    End Function

End Class
