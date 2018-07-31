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

Imports System.ComponentModel
Imports System.IO

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Label-derived class for receiving dropped files
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cFileDropLabel
        Inherits Label

        Private m_bDragOver As Boolean = False

        Public Sub New()
            Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.AutoSize = False
            Me.Font = New Font(Me.Font.Name, 14.25, FontStyle.Bold)
            Me.TextAlign = ContentAlignment.MiddleCenter
            Me.AllowDrop = True
        End Sub

        Public Class EwEFileDropArgs
            Public Sub New(afiles As String())
                Me.Files = afiles
            End Sub
            Public Property Files As String()
            Public Property Accept As Boolean = True
        End Class

        Public Event OnAcceptFiles(sender As Object, e As EwEFileDropArgs)
        Public Event OnFilesDropped(sender As Object, astrFiles As String())

        <Browsable(True)>
        <Description("Maximum number of files that can be dropped simultaenously. Set this value to 0 if there are no restrictions.")>
        <Category("Drag and drop")>
        Public Property MaxFiles As Integer = 0

        <Browsable(True)>
        <Description("File extensions that can be dropped (semi-colon separated)")>
        <Category("Drag and drop")>
        Public Property FileExtensions As String = ""

        Protected Overrides Sub InitLayout()
            MyBase.InitLayout()
            Me.UpdateControls()
        End Sub

        Protected Overrides Sub OnDragDrop(e As System.Windows.Forms.DragEventArgs)
            Try
                If Not Me.m_bDragOver Then Return
                RaiseEvent OnFilesDropped(Me, CType(e.Data.GetData(DataFormats.FileDrop), String()))
            Catch ex As Exception
            End Try
            Me.m_bDragOver = False
            Me.UpdateControls()
        End Sub

        Protected Overrides Sub OnDragEnter(e As System.Windows.Forms.DragEventArgs)
            Try
                If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
                    Dim astrFiles As String() = CType(e.Data.GetData(DataFormats.FileDrop), String())
                    If (Me.AcceptFiles(astrFiles)) Then
                        e.Effect = DragDropEffects.All
                        Me.m_bDragOver = True
                    End If
                End If
            Catch ex As Exception
                Me.m_bDragOver = False
            End Try
            Me.UpdateControls()
        End Sub

        Protected Overrides Sub OnDragLeave(e As System.EventArgs)
            Try
                Me.m_bDragOver = False
            Catch ex As Exception

            End Try
            Me.UpdateControls()
        End Sub

        Protected Overridable Sub UpdateControls()
            If Me.m_bDragOver Then
                Me.BackColor = SystemColors.Highlight
                Me.ForeColor = SystemColors.HighlightText
            Else
                Me.BackColor = Drawing.Color.Transparent
                Me.ForeColor = SystemColors.ButtonShadow
            End If
        End Sub

        Private Function AcceptFiles(astrFiles As String()) As Boolean

            Dim bAccept As Boolean = True

            If (astrFiles Is Nothing) Then Return False
            If (astrFiles.Length = 0) Then Return False
            If (Me.MaxFiles > 0) Then
                If (astrFiles.Length > Me.MaxFiles) Then Return False
            End If
            If (String.IsNullOrWhiteSpace(Me.FileExtensions)) Then Return True
            Dim htExt As New HashSet(Of String)
            For Each strExt As String In Me.FileExtensions.Split(";"c)
                If (Not String.IsNullOrWhiteSpace(strExt)) Then
                    htExt.Add(strExt.ToLower)
                End If
            Next
            For Each strFile As String In astrFiles
                If Not htExt.Contains(Path.GetExtension(strFile).ToLower) Then
                    Return False
                End If
            Next

            Try
                Dim hmpf As New EwEFileDropArgs(astrFiles)
                RaiseEvent OnAcceptFiles(Me, hmpf)
                bAccept = hmpf.Accept
            Catch ex As Exception

            End Try
            Return bAccept

        End Function

    End Class

End Namespace

