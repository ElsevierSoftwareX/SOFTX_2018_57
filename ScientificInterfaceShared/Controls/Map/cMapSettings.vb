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
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports System.Text

#End Region ' Imports

Namespace Controls.Map

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Utility class to preserve and restore layer visibility states of a map.
    ''' Ideally, connect this class to <see cref="ScientificInterfaceShared.Forms.frmEwE.Settings"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cMapSettings

        ''' <summary>
        ''' Hide all layers with the same name that were hidden last. This logic
        ''' does not take model + scenario name into account (as it should)
        ''' </summary>
        ''' <param name="strSettings"></param>
        ''' <param name="map"></param>
        Public Shared Sub Load(strSettings As String, map As ucMap)

            Dim ht As New HashSet(Of String)
            For Each bit As String In strSettings.Split("|"c)
                If Not String.IsNullOrWhiteSpace(bit) Then ht.Add(bit)
            Next
            For Each l As cDisplayLayer In map.Layers
                If l.Renderer IsNot Nothing Then
                    l.Renderer.IsVisible = (Not ht.Contains(l.Name))
                End If
            Next
        End Sub

        ''' <summary>
        ''' Hide all layers with the same name that were hidden last. This logic
        ''' does not take model + scenario name into account (as it should)
        ''' </summary>
        ''' <param name="map"></param>
        Public Shared Function Save(map As ucMap) As String
            Dim sb As New StringBuilder()
            For Each l As cDisplayLayer In map.Layers
                If l.Renderer IsNot Nothing Then
                    If (Not l.Renderer.IsVisible) Then
                        If (sb.Length > 0) Then sb.Append("|"c)
                        sb.Append(l.Name)
                    End If
                End If
            Next
            Return sb.ToString()
        End Function

#If 0 Then

        Public Shared Sub Load(strSettings As String, strScenario As String, map As ucMap)

            Dim data As Dictionary(Of String, HashSet(Of String)) = cMapSettings.Unwrap(strSettings)
            If data.ContainsKey(strScenario) Then
                Dim ht As HashSet(Of String) = data(strScenario)
                For Each l As cDisplayLayer In map.Layers
                    If l.Renderer IsNot Nothing Then
                        l.Renderer.IsVisible = (Not ht.Contains(l.Name))
                    End If
                Next
            End If

        End Sub

        Public Shared Function Save(strSettings As String, strScenario As String, map As ucMap) As String

            Dim data As Dictionary(Of String, HashSet(Of String)) = cMapSettings.Unwrap(strSettings)
            Dim ht As New HashSet(Of String)
            For Each l As cDisplayLayer In map.Layers
                If l.Renderer IsNot Nothing Then
                    If (Not l.Renderer.IsVisible) Then ht.Add(l.Name)
                End If
            Next
            data(strScenario) = ht
            Return cMapSettings.Wrap(data)

        End Function

#Region " Internals "

        Private Shared Function Unwrap(strSettings As String) As Dictionary(Of String, HashSet(Of String))

            Dim groups As String() = strSettings.Split(New String() {"|@|"}, StringSplitOptions.RemoveEmptyEntries)
            Dim lOut As New Dictionary(Of String, HashSet(Of String))

            For Each group As String In groups
                Dim lHidden As New HashSet(Of String)
                Dim strScenario As String = ""
                Dim i As Integer = 0
                For Each strBit As String In group.Split("|"c)
                    If i = 0 Then
                        strScenario = strBit
                    ElseIf (Not lHidden.Contains(strBit)) Then
                        lHidden.Add(strBit)
                    End If
                    i += 1
                Next
                If Not String.IsNullOrWhiteSpace(strScenario) Then
                    lOut(strScenario) = lHidden
                End If
            Next
            Return lOut
        End Function

        Private Shared Function Wrap(data As Dictionary(Of String, HashSet(Of String))) As String
            Dim sb As New StringBuilder()
            Dim lData As HashSet(Of String)
            For Each strScenario As String In data.Keys
                lData = data(strScenario)
                If (lData.Count > 0) Then
                    If (sb.Length > 0) Then sb.Append("|@|")
                    sb.Append(strScenario)
                    For Each strLayer As String In lData
                        If (Not String.IsNullOrWhiteSpace(strLayer)) Then
                            sb.Append("|")
                            sb.Append(strLayer)
                        End If
                    Next
                End If
            Next
            Return sb.ToString
        End Function

#End Region ' Internals

#End If

    End Class

End Namespace
