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
Imports ScientificInterfaceShared.Commands
Imports System.IO

#End Region ' Imports

Namespace Commands

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Generic command to launch an interface to select a 'save file' location.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cFileSaveCommand
        Inherits cCommand

        ''' -----------------------------------------------------------------------
        ''' <summary>The name of this command.</summary>
        ''' -----------------------------------------------------------------------
        Public Shared COMMAND_NAME As String = "~savefile"

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of the NavigationCommand class.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, COMMAND_NAME)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Invoke the file save command with default parameters.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overrides Sub Invoke()
            Me.Result = DialogResult.Cancel
            MyBase.Invoke()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Invoke the file save user interface with a given file name, file filter,
        ''' and dialog title box.
        ''' selection 
        ''' </summary>
        ''' <param name="strFileName"></param>
        ''' <param name="strFileFilter"></param>
        ''' <param name="iFilter"></param>
        ''' <param name="strTitle">
        ''' Optional dialog title. If left empty, a system default is used.
        ''' </param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal strFileName As String, _
                                    ByVal strFileFilter As String, _
                                    Optional ByVal iFilter As Integer = 0, _
                                    Optional ByVal strTitle As String = "")

            Dim strPath As String = ""

            Me.Title = strTitle
            Me.FileName = strFileName
            Me.Filters = strFileFilter
            Me.FilterIndex = iFilter

            Try
                ' Only update directory if a directory has been specified
                If Not String.IsNullOrWhiteSpace(strFileName) Then
                    strPath = Path.GetDirectoryName(strFileName)
                End If
                If Not String.IsNullOrEmpty(strPath) Then
                    Me.Directory = strPath
                End If
            Catch ex As Exception
            End Try

            Me.Invoke()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <param name="strFileFilter"></param>
        ''' <param name="iFilter"></param>
        ''' <param name="strTitle">
        ''' Optional dialog title. If left empty, the Visual Studio default is used.
        ''' </param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ByVal strFileFilter As String, _
                                    Optional ByVal iFilter As Integer = 0, _
                                    Optional ByVal strTitle As String = "")

            Me.Invoke("", strFileFilter, iFilter, strTitle)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the title to display in the dialog. If left emtpy, the .NET
        ''' framework will use the default file open title.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Title() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set the file name to show in the dialog. Once invoked and closed
        ''' with the result <see cref="DialogResult.OK">OK</see>, this
        ''' property will contain the full path to the file selected in the 
        ''' dialog.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property FileName() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the directory that the command was invoked with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Directory() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' The result that the dialog closed with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Result() As DialogResult
            
        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the file filters that the dialog was invoked with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Filters() As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the file filter index that the dialog was invoked with.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property FilterIndex() As Integer

    End Class

End Namespace
