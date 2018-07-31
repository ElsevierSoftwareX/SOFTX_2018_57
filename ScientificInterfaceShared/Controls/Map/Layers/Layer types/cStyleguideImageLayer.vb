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
Imports EwECore.Auxiliary
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Controls.Map.Layers
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls.Map.Layers

    Public Class cStyleguideImageLayer
        Inherits cDisplayLayerImage

        Private m_bStyleGuideChanged As Boolean = False

        Public Sub New(uic As cUIContext)
            MyBase.New(uic)
            AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
            Me.UpdateImage()
        End Sub

        Protected Overrides Sub Dispose(bDisposing As Boolean)
            ' Stop listening
            RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
            ' Base class cleanup
            MyBase.Dispose(bDisposing)
        End Sub

        Public Overrides Property Image As Image
            Get
                Return Me.m_uic.StyleGuide.MapReferenceImage
            End Get
            Set(value As Image)
                ' NOP
            End Set
        End Property

        Private Sub UpdateImage()
            Try
                With Me.m_uic.StyleGuide
                    Me.Image = .MapReferenceImage
                    Me.ImageTL = .MapReferenceLayerTL
                    Me.ImageBR = .MapReferenceLayerBR
                End With
            Catch ex As Exception

            End Try
            Me.m_bStyleGuideChanged = False
            Me.Update(eChangeFlags.Map, False)
        End Sub

        Private Sub OnStyleGuideChanged(ct As cStyleGuide.eChangeType)
            If (ct And cStyleGuide.eChangeType.Map) > 0 Then
                Me.UpdateImage()
            End If
        End Sub
    End Class

End Namespace
