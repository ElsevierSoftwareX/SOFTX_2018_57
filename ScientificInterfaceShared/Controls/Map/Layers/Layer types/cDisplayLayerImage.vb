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

    Public Class cDisplayLayerImage
        Inherits cDisplayLayer

        Public Sub New(uic As cUIContext, Optional img As Image = Nothing)
            MyBase.New(uic, New cImageLayerRenderer(Nothing))
            Me.Image = img
            Me.m_editor = Nothing
        End Sub

        Public Overridable Property Image As Image = Nothing

        Public Property ImageTL As PointF = New PointF(0, 0)

        Public Property ImageBR As PointF = New PointF(0, 0)

        Public ReadOnly Property IsValid As Boolean
            Get
                If (Me.m_uic Is Nothing) Then Return False
                Return (Me.Image IsNot Nothing)
            End Get
        End Property

    End Class

End Namespace
