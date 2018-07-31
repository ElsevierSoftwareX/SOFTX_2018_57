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
Imports EwECore

#End Region ' Imports

Friend Class cKeystone3

    Private Class cGroup
        Public Sub New(i As Integer, b As Single)
            Me.Index = i
            Me.Biomass = b
        End Sub
        Public Index As Integer
        Public Biomass As Single
        Public Epsilon As Single
        Public BC As Integer
        Public K3 As Single

        Public Overrides Function ToString() As String
            Return "cGroup " & Me.Index & ", " & Me.Biomass
        End Function

    End Class

    Private Class cGroupComparer
        Implements IComparer(Of cGroup)

        Public Function Compare(x As cGroup, y As cGroup) As Integer Implements System.Collections.Generic.IComparer(Of cGroup).Compare
            If (x.Biomass < y.Biomass) Then Return -1
            If (x.Biomass > y.Biomass) Then Return 1
            If (x.Index < y.Index) Then Return -1
            Return 1
        End Function

    End Class

    Public Shared Sub Calculate(data As cEcopathDataStructures, network As cEcoNetwork)

        Dim lGroups As New List(Of cGroup)
        Dim g As cGroup = Nothing
        Dim dSum As Double = 0

        For i As Integer = 1 To data.NumLiving
            g = New cGroup(i, data.B(i))

            dSum = 0
            For j As Integer = 1 To data.NumLiving
                If (i <> j) Then
                    dSum += (network.MTI(i, j) * network.MTI(i, j))
                End If
            Next
            g.Epsilon = CSng(Math.Sqrt(dSum))

            lGroups.Add(g)
        Next

        lGroups.Sort(New cGroupComparer())

        For i As Integer = 0 To data.NumLiving - 1
            g = lGroups(i)
            g.BC = data.NumLiving - i
            g.K3 = g.Epsilon * g.BC
            network.KeystoneIndex3(g.Index) = Math.Log10(g.K3)
        Next

    End Sub

End Class
