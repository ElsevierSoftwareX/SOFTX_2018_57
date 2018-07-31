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
Imports System.ComponentModel
Imports EwEUtils.SystemUtilities

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' A <see cref="TreeView"/>-inherited user control that uses the Windows 7 visual display style.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cThemedTreeView
        Inherits TreeView

        Private m_bShowImages As Boolean = True
        Private m_il As ImageList = Nothing

        Public Sub New()
            Me.ShowImages = True
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' And here we thought to be rid of P/invoke!
        ''' </summary>
        ''' <param name="hWnd"></param>
        ''' <param name="pszSubAppName"></param>
        ''' <param name="pszSubIdList"></param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Declare Unicode Function SetWindowTheme Lib "uxtheme.dll" (ByVal hWnd As IntPtr, ByVal pszSubAppName As String, ByVal pszSubIdList As String) As Integer

        Protected Overrides Sub CreateHandle()
            MyBase.CreateHandle()
            If cSystemUtils.IsWindows And cSystemUtils.IsRunningWin7OrHigher And Not cSystemUtils.Is64BitProcess Then
                Try
                    SetWindowTheme(Me.Handle, "explorer", Nothing)
                Catch ex As Exception
                    ' Whoah!
                End Try
            End If
        End Sub

        Private Const WM_ERASEBKGND As Integer = &H14

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overriden to reduce flicker when redrawing.
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <remarks>
        ''' http://forums.codeguru.com/showthread.php?182326-TreeView-Flickering-Problem
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub WndProc(ByRef msg As System.Windows.Forms.Message)
            If (msg.Msg = WM_ERASEBKGND) Then
                msg.Result = IntPtr.Zero
                Return
            End If
            MyBase.WndProc(msg)
        End Sub

        <Category("Appearance")>
        <Browsable(True)>
        Public Property ShowImages As Boolean
            Get
                Return Me.m_bShowImages
            End Get
            Set(value As Boolean)
                Me.m_bShowImages = value
                UpdateImageVisibility()
            End Set
        End Property

        Public Overloads Property ImageList As ImageList
            Get
                Return Me.m_il
            End Get
            Set(value As ImageList)
                Me.m_il = value
                UpdateImageVisibility()
            End Set
        End Property

        Private Sub UpdateImageVisibility()

            If (Me.m_bShowImages) And (Me.m_il IsNot Nothing) Then
                MyBase.ImageList = Me.m_il
            Else
                MyBase.ImageList = Nothing
            End If
            Me.Invalidate()

        End Sub

    End Class

End Namespace
