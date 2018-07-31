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

    ''' =======================================================================
    ''' <summary>
    ''' Public accessible but shared tooltip instance for homogenous application
    ''' behaviour and styling. Yeah.
    ''' </summary>
    ''' =======================================================================
    Public Class cToolTipShared
        Inherits ToolTip

#Region " Privates "

        ''' <summary>Singleton instance.</summary>
        Private Shared __inst__ As cToolTipShared

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Singleton enforced constructor
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub New()
            ' Yoho
        End Sub

#End Region ' Privates

#Region " Public interfaces "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Zhe van einzterfeiz to get zhe tuhltipp.
        ''' </summary>
        ''' <returns>Zhe tuhltipp inschtanz.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetInstance() As cToolTipShared
            If cToolTipShared.__inst__ Is Nothing Then
                cToolTipShared.__inst__ = New cToolTipShared
                cToolTipShared.__inst__.Active = True
            End If
            Return cToolTipShared.__inst__
        End Function

#End Region ' Public interfaces

#Region " Doomed interfaces "

        <Obsolete("Please use ToolTip.SetToolTip instead")> _
        Public Overloads Sub Show(ByVal text As String, ByVal wnd As IWin32Window)
            Debug.Assert(False)
        End Sub

        <Obsolete("Please use ToolTip.SetToolTip instead")> _
        Public Overloads Sub Show(ByVal text As String, ByVal wnd As IWin32Window, ByVal iTimeout As Integer)
            Debug.Assert(False)
        End Sub

        <Obsolete("Please use ToolTip.SetToolTip instead")> _
        Public Overloads Sub Show(ByVal text As String, ByVal wnd As IWin32Window, ByVal pt As System.Drawing.Point)
            Debug.Assert(False)
        End Sub

        <Obsolete("Please use ToolTip.SetToolTip instead")> _
        Public Overloads Sub Show(ByVal text As String, ByVal wnd As IWin32Window, ByVal pt As System.Drawing.Point, ByVal iTimeout As Integer)
            Debug.Assert(False)
        End Sub

        <Obsolete("Please use ToolTip.SetToolTip instead")> _
        Public Overloads Sub Show(ByVal text As String, ByVal wnd As IWin32Window, ByVal x As Integer, ByVal y As Integer)
            Debug.Assert(False)
        End Sub

        <Obsolete("Please use ToolTip.SetToolTip instead")> _
        Public Overloads Sub Show(ByVal text As String, ByVal wnd As IWin32Window, ByVal x As Integer, ByVal y As Integer, ByVal iTimeout As Integer)
            Debug.Assert(False)
        End Sub

#End Region ' Doomed interfaces

    End Class

End Namespace ' Controls
