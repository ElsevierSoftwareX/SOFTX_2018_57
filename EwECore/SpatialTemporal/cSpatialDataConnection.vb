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

Imports EwECore.SpatialData.cSpatialScalarDataAdapterBase
Imports EwEUtils.SpatialData

Namespace SpatialData

    ' TODO: inherit from cCoreInputOutputBase
    ' Add variables
    ' Variable statuses: scale may be read-only

    Public Class cSpatialDataConnection

        Public Property Dataset As ISpatialDataSet = Nothing
        Public Property Converter As ISpatialDataConverter = Nothing
        Public Property Scale As Single = 1
        Public Property ScaleType As eScaleType

        Public Property Adapter As cSpatialDataAdapter = Nothing
        Public Property iLayer As Integer = 1

        Public Sub New()
        End Sub

        Public Overridable Function IsConfigured() As Boolean

            Dim bIsConfigured As Boolean = False

            If (Me.Dataset IsNot Nothing) Then
                If (Me.Dataset.IsConfigured()) Then
                    If Not String.IsNullOrWhiteSpace(Me.Dataset.ConversionFormat) Then
                        If (Me.Converter IsNot Nothing) Then
                            bIsConfigured = bIsConfigured Or Me.Converter.IsConfigured()
                        End If
                    Else
                        bIsConfigured = True
                    End If
                End If
            End If
            Return bIsConfigured

        End Function

        'ToDo: add diagnostics? Compatibility?

    End Class

End Namespace
