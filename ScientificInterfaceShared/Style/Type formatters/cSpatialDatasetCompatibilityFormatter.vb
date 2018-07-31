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
Imports EwECore.SpatialData
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="cDatasetCompatilibity"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cSpatialDatasetCompatibilityFormatter
        Implements ITypeFormatter

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(cDatasetCompatilibity)
        End Function

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            Dim comp As cDatasetCompatilibity = Nothing
            Dim strBit As String = ""


            Try
                comp = DirectCast(value, cDatasetCompatilibity)

                Dim val As cDatasetCompatilibity.eCompatibilityTypes = comp.Compatibility
                Dim strDescr As String = cResourceUtils.LoadString("COMPATIBILITY_" & val.ToString().ToUpper, My.Resources.ResourceManager)
                Dim astrBits As String() = Nothing
                Dim iNumBits As Integer = 0

                If (strDescr IsNot Nothing) Then
                    astrBits = strDescr.Split("|"c)
                    iNumBits = astrBits.Length
                Else
                    Return ""
                End If

                For i As Integer = 0 To descriptor

                    ' Is first part?
                    If (i = 0) Then
                        ' #Yes: remember default
                        strBit = comp.Compatibility.ToString
                    End If

                    If i < iNumBits Then
                        ' Has a part?
                        If Not String.IsNullOrEmpty(astrBits(i)) Then
                            ' #Yes: update bit
                            strBit = astrBits(i).Trim
                        End If
                    End If

                Next

                ' Special cases
                If (val = cDatasetCompatilibity.eCompatibilityTypes.Errors) Then
                    strBit = cStringUtils.Localize(strBit, comp.NumError)
                End If
            Catch ex As Exception
                Debug.Assert(False, "Malformed resource")
                Return ""
            End Try

            Return strBit

        End Function

        ''' <summary>
        ''' Get a compatibility summary.
        ''' </summary>
        ''' <param name="comp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Summary(comp As cDatasetCompatilibity) As String

            Dim iNumTS As Integer = comp.NumAssessedTimeSteps
            Dim iNumOverlap As Integer = comp.NumOverlappingTimeSteps
            Dim iNumPartial As Integer = comp.NumPartialSpatialOverlap
            Dim iNumFull As Integer = comp.NumFullSpatialOverlap

            Select Case comp.Compatibility

                Case cDatasetCompatilibity.eCompatibilityTypes.NotSet, _
                     cDatasetCompatilibity.eCompatibilityTypes.Errors, _
                     cDatasetCompatilibity.eCompatibilityTypes.NoTemporal, _
                     cDatasetCompatilibity.eCompatibilityTypes.NoSpatial
                    Return Me.GetDescriptor(comp, eDescriptorTypes.Description)

                Case cDatasetCompatilibity.eCompatibilityTypes.TemporalNotIndexed
                    Return cStringUtils.Localize(My.Resources.COMPATIBILITY_SUMMARY_NOINDEX, _
                                                 CInt(Math.Ceiling(100 * iNumOverlap / Math.Max(1, iNumTS))))

            End Select

            Return cStringUtils.Localize(My.Resources.COMPATIBILITY_SUMMARY, _
                                         CInt(Math.Ceiling(100 * iNumOverlap / Math.Max(1, iNumTS))), _
                                         CInt(Math.Ceiling(100 * iNumPartial / Math.Max(1, iNumOverlap))), _
                                         CInt(Math.Ceiling(100 * iNumFull / Math.Max(1, iNumOverlap))))
        End Function

    End Class

End Namespace
