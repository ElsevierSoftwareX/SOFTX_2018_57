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

#End Region ' Imports

Namespace Controls

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Override for the .NET Color dialog, overridden to manage custom colours.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cEwEColorDialog
        Inherits ColorDialog

        Private Shared s_lCustomColors As New List(Of Integer)

        Public Sub New()
            MyBase.New()
            Me.AllowFullOpen = True
            Me.FullOpen = True
            Me.CustomColors = s_lCustomColors.ToArray
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            s_lCustomColors.Clear()
            s_lCustomColors.AddRange(Me.CustomColors)
            MyBase.Dispose(disposing)
        End Sub

    End Class

End Namespace ' Controls
