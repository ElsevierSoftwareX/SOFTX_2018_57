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
''' Content manager derived class to invoke the NA biomass pyramid interface.
''' </summary>
''' ---------------------------------------------------------------------------
<CLSCompliant(False)> _
Public Class cBiomassPyramid
    Inherits cPyramid

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="cPyramid.PyramidType"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Function PyramidType() As modUtility.ePyramidTypes
        Return ePyramidTypes.Biomass
    End Function

    Public Overrides Function PageTitle() As String
        Return "Biomass pyramid (external)"
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="cPyramid.WritePyramidFile"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Function WritePyramidFile(ByVal core As cCore, _
                                               ByVal sw As StreamWriter, _
                                               ByVal ci As CultureInfo) As Boolean

        Dim intTemp As Integer
        Dim iMaxTL As Integer
        Dim sTotalBiomass As Single
        Dim iFlag As Integer
        Dim bSucces As Boolean = True

        Dim asB() As Single
        Dim asBRel() As Single
        Try
            iFlag = 2
            sw.Write(Format(iFlag, "0"))

            iMaxTL = CInt(IIf(NetworkManager.nTrophicLevels > 9, 9, NetworkManager.nTrophicLevels))
            ReDim asB(iMaxTL)
            ReDim asBRel(iMaxTL)

            sw.WriteLine(Format(iMaxTL, "0"))
            sw.WriteLine("t/km²")

            sTotalBiomass = 0
            If NetworkManager.nTrophicLevels < core.nLivingGroups Then
                intTemp = NetworkManager.nTrophicLevels
            Else
                intTemp = core.nLivingGroups
            End If
            For i As Integer = 1 To intTemp
                sTotalBiomass = sTotalBiomass + CSng(IIf(NetworkManager.BiomassByTrophicLevel(i) > 0.001, _
                    NetworkManager.BiomassByTrophicLevel(i), 0))
            Next
            sw.WriteLine(sTotalBiomass.ToString("00000000.000", ci))

            For i As Integer = 1 To iMaxTL
                Dim sngTemp As Single
                'row = i '(MaxTL - i) + 1
                sngTemp = NetworkManager.BiomassByTrophicLevel(i)
                If Math.Abs(sngTemp) > 0.001 Then
                    sw.Write(sngTemp.ToString("00000000.000", ci))
                Else
                    sw.Write(0.ToString("00000000.000", ci))
                End If
                asB(i) = sngTemp

                If i < iMaxTL And sngTemp > 0 Then
                    sngTemp = NetworkManager.BiomassByTrophicLevel(i) / sngTemp
                Else
                    sngTemp = 0
                End If
                sw.WriteLine(sngTemp.ToString("00000000.000", ci))
                asBRel(i) = sngTemp

            Next i

            '' Future
            'modUtility.WritePyramidFile(Me.NetworkManager.Core.EwEModel.Name, _
            '                Me.PyramidType, "t/km²", _
            '                iMaxTL, sTotalBiomass, _
            '                asB, asBRel)

        Catch ex As Exception
            bSucces = False
        End Try

        Return bSucces

    End Function

End Class
