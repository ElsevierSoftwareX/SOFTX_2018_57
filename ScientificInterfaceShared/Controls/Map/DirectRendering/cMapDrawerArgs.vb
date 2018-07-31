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

Namespace Controls.Map

    Public Class cMapDrawerArgs

        Private m_maptype As cMapDrawerBase.eMapType
        Private m_relscaler() As Single
        Private m_sMaxLegendF As Single

        Public Sub New(ByVal maptype As cMapDrawerBase.eMapType,
                       ByVal theRelScaler() As Single,
                       ByVal MaxLegendF As Single)

            Dim data As Single() = Nothing

            If (theRelScaler IsNot Nothing) Then
                ReDim data(theRelScaler.Length)
                theRelScaler.CopyTo(data, 0)
            End If

            Me.m_maptype = maptype
            Me.m_relscaler = data
            Me.m_sMaxLegendF = MaxLegendF
        End Sub

        Public ReadOnly Property MapType As cMapDrawerBase.eMapType
            Get
                Return Me.m_maptype
            End Get
        End Property

        Public ReadOnly Property RelMapScaler As Single()
            Get
                Return Me.m_relscaler
            End Get
        End Property

        Public ReadOnly Property FishingMortLegendMax As Single
            Get
                Return Me.m_sMaxLegendF
            End Get
        End Property

    End Class

End Namespace
