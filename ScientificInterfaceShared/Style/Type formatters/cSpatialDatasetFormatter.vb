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
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class for providing a textual description of <see cref="ISpatialDataSet"/>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cSpatialDatasetFormatter
        Implements ITypeFormatter

        Public Function GetDescribedType() As System.Type _
            Implements ITypeFormatter.GetDescribedType
            Return GetType(ISpatialDataSet)
        End Function

        Public Function GetDescriptor(ByVal value As Object, _
                                      Optional ByVal descriptor As eDescriptorTypes = eDescriptorTypes.Name) As String _
                                      Implements ITypeFormatter.GetDescriptor

            Dim strResult As String = ""

            Try
                If (value IsNot Nothing) Then

                    Dim obj As ISpatialDataSet = DirectCast(value, ISpatialDataSet)
                    strResult = obj.DisplayName

                    Select Case descriptor

                        Case eDescriptorTypes.Name
                            ' Has configuration?
                            If (obj.IsConfigured) Then
                                ' #Yes: filter by lower time limit
                                Select Case obj.TimeStart

                                    Case DateTime.MinValue
                                        ' Has upper range set?
                                        If (obj.TimeEnd < DateTime.MaxValue) Then
                                            ' #Yes: Return <inf,upper] range
                                            strResult = String.Format(My.Resources.LABEL_VALUE_UPTO, obj.DisplayName, obj.TimeEnd.ToShortDateString)
                                        End If

                                    Case DateTime.MaxValue
                                        ' Something is screwed
                                        Debug.Assert(False)

                                    Case obj.TimeEnd
                                        ' Start = end, and neither is inf: show a single date
                                        strResult = String.Format(My.Resources.LABEL_VALUE_AT, obj.DisplayName, obj.TimeStart.ToShortDateString)

                                    Case Else
                                        ' Has upper range set?
                                        If (obj.TimeEnd < DateTime.MaxValue) Then
                                            ' #Yes: show [lower, upper] range
                                            strResult = String.Format(My.Resources.LABEL_VALUE_RANGE, obj.DisplayName, obj.TimeStart.ToShortDateString, obj.TimeEnd.ToShortDateString)
                                        Else
                                            ' #No: show [lower, inf> range
                                            strResult = String.Format(My.Resources.LABEL_VALUE_FROM, obj.DisplayName, obj.TimeStart.ToShortDateString)
                                        End If

                                End Select
                            End If

                        Case eDescriptorTypes.Description
                            strResult = obj.DataDescription
                    End Select
                Else
                    strResult = My.Resources.GENERIC_VALUE_NONE
                End If

            Catch ex As Exception
                Debug.Assert(False)
            End Try

            Return strResult

        End Function

    End Class

End Namespace
