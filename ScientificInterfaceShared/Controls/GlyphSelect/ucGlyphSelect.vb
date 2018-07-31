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

'==============================================================================
'
' $Log: ucGlyphSelect.vb,v $
' Revision 1.1  2008/09/26 07:31:17  sherman
' --== DELETED HISTORY ==--
'
' Revision 1.1  2008/06/01 23:45:09  jeroens
' Separated from Scientific Interface
'
' Revision 1.3  2007/12/02 03:33:01  jeroens
' + Selection scrolled into view
'
' Revision 1.2  2007/12/01 22:07:47  jeroens
' * Added selection optimization
'
' Revision 1.1  2007/12/01 19:39:00  jeroens
' Initial version
'
'==============================================================================

Option Strict On

Imports System
imports System.Drawing
imports System.Drawing.Imaging
imports System.Security.Cryptography

Namespace Controls

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' User control that provides an interface for users to select a glyph.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class ucGlyphSelect

#Region " Private vars "

        ''' <summary>Index of selected image, if any.</summary>
        Private m_iSelectedIndex As Integer = -1
        ''' <summary>List of byte arrays (labt) containing hash codes for stored images.</summary>
        Private m_labtImageHashCodes As New List(Of Byte())
        ''' <summary>Max image dimension accepted by this control.</summary>
        Private m_szMaxImageSize As Size = New Size(100, 100)
        ''' <summary>Glyph size.</summary>
        Private m_szGlyphSize As Size = New Size(40, 25)

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()
            ' This call is required by the Windows Form Designer.
            InitializeComponent()

            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            Me.SetStyle(ControlStyles.ResizeRedraw, True)
            Me.SetStyle(ControlStyles.ContainerControl, True)
        End Sub

#End Region ' Constructor

#Region " Public interfaces "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event that this control throws whenever the selection changes.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Event OnSelectionChanged(ByVal sender As ucGlyphSelect, ByVal e As EventArgs)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the selected image in this control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property SelectedImage() As Image
            Get
                Return Me.GetImageAt(Me.SelectedIndex)
            End Get
            Set(ByVal value As Image)
                Me.SelectedIndex = Me.GetImageIndex(value)
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the index of the selected image in this control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property SelectedIndex() As Integer
            Get
                Return Me.m_iSelectedIndex
            End Get
            Set(ByVal value As Integer)
                Dim uc As ucGlyph = Nothing

                If (value = Me.m_iSelectedIndex) Then Return

                If (Me.IsValidImageIndex(Me.m_iSelectedIndex)) Then
                    uc = Me.GetGlyphControlAt(Me.m_iSelectedIndex)
                    uc.Selected = False
                End If

                Me.m_iSelectedIndex = value

                If (Me.IsValidImageIndex(Me.m_iSelectedIndex)) Then
                    uc = Me.GetGlyphControlAt(Me.m_iSelectedIndex)
                    uc.Selected = True
                    Me.ScrollControlIntoView(uc)
                End If

                RaiseEvent OnSelectionChanged(Me, New EventArgs)
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Adds an image to this control.
        ''' </summary>
        ''' <param name="img">The image to add.</param>
        ''' <returns>True if this image is valid and was not already added, 
        ''' False otherwise.</returns>
        ''' -----------------------------------------------------------------------
        Public Function AddImage(ByVal img As Image) As Boolean

            Dim uc As ucGlyph = Nothing
            Dim abtHash As Byte() = Nothing

            If Not Me.IsValidImage(img) Then Return False

            abtHash = Me.GetImageHashCode(img)
            If (Me.GetHashCodeIndex(abtHash) > -1) Then Return False

            uc = New ucGlyph(img)
            uc.Size = Me.m_szGlyphSize

            Me.m_labtImageHashCodes.Add(abtHash)
            Me.m_flpGlyphs.Controls.Add(uc)

            AddHandler uc.Click, AddressOf OnGlyphSelect

            Return True
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Removes an image from this control.
        ''' </summary>
        ''' <param name="img">The image to remove.</param>
        ''' <returns>True if the image was succesfully removed.</returns>
        ''' -----------------------------------------------------------------------
        Public Function RemoveImage(ByVal img As Image) As Boolean

            Dim iIndex As Integer = -1
            Dim abtHash As Byte() = Nothing
            Dim uc As ucGlyph = Nothing

            If Not Me.IsValidImage(img) Then Return False

            abtHash = Me.GetImageHashCode(img)
            iIndex = Me.GetHashCodeIndex(abtHash)

            If (iIndex = -1) Then Return False

            uc = Me.GetGlyphControlAt(iIndex)
            RemoveHandler uc.Click, AddressOf OnGlyphSelect
            uc = Nothing

            Me.m_flpGlyphs.Controls.RemoveAt(iIndex)
            Me.m_labtImageHashCodes.RemoveAt(iIndex)

            If (iIndex >= Me.m_iSelectedIndex) Then
                Me.SelectedIndex = Math.Max(0, iIndex - 1)
            End If

            Return True

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the index of an image in this control.
        ''' </summary>
        ''' <param name="img">Image to find.</param>
        ''' <returns>A value equal to or greater than 0 representing the image
        ''' index, or -1 if the image was not found.</returns>
        ''' -----------------------------------------------------------------------
        Public Function GetImageIndex(ByVal img As Image) As Integer
            ' Sanity check
            If (img Is Nothing) Then Return -1
            ' Get the real thing
            Return Me.GetHashCodeIndex(Me.GetImageHashCode(img))
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the number of images in this control.
        ''' </summary>
        ''' <returns>The number of images in this control.</returns>
        ''' -----------------------------------------------------------------------
        Public Function GetImageCount() As Integer
            Return Me.m_labtImageHashCodes.Count
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the size of glyphs in this control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property GlyphSize() As Size
            Get
                Return Me.m_szGlyphSize
            End Get
            Set(ByVal value As Size)
                ' Store
                Me.m_szGlyphSize = value
                ' Resize existing glyphs
                Me.SuspendLayout()
                For iIndex As Integer = 0 To Me.GetImageCount() - 1
                    Me.GetGlyphControlAt(iIndex).Size = value
                Next
                Me.ResumeLayout()
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the size of glyphs in this control.
        ''' </summary>
        ''' <remarks>
        ''' Note that this method will not affect images that have already
        ''' been added to this control.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Property MaxImageSize() As Size
            Get
                Return Me.m_szMaxImageSize
            End Get
            Set(ByVal value As Size)
                Me.m_szMaxImageSize = value
            End Set
        End Property

#End Region ' Public interfaces

#Region " Events "

        Private Sub ucGlyphSelect_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
            ' Detatch dangling event handlers
            ' This is not the correct way; the panel may be cleared while control instances may still be kept alive
            ' by the very dangling event handlers that we're trying to release. Instead, a second administration
            ' should keep track of control instances from which handlers are freed. Not important enough right now!
            For Each uc As Panel In Me.m_flpGlyphs.Controls
                RemoveHandler DirectCast(uc, ucGlyph).Click, AddressOf OnGlyphSelect
            Next
            Me.m_labtImageHashCodes.Clear()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Glyph selection state change event handler.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub OnGlyphSelect(ByVal sender As Object, ByVal e As EventArgs)
            Debug.Assert(TypeOf sender Is ucGlyph)
            Me.SelectedImage = DirectCast(sender, ucGlyph).Image
        End Sub

#End Region ' Events

#Region " Internal implementation "

        Private Function GetImageAt(ByVal iIndex As Integer) As Image
            If (Not Me.IsValidImageIndex(iIndex)) Then
                Return Nothing
            End If
            Return Me.GetGlyphControlAt(iIndex).Image
        End Function

        Private Function GetGlyphControlAt(ByVal iIndex As Integer) As ucGlyph
            If (Not Me.IsValidImageIndex(iIndex)) Then
                Return Nothing
            End If
            Return DirectCast(Me.m_flpGlyphs.Controls(iIndex), ucGlyph)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; states whether a given image is valid for storage 
        ''' in this control.
        ''' </summary>
        ''' <param name="img">The image to validate.</param>
        ''' <returns>True if valid, False otherwise.</returns>
        ''' -----------------------------------------------------------------------
        Private Function IsValidImage(ByVal img As Image) As Boolean
            If (img Is Nothing) Then Return False
            If (img.Width > Me.m_szMaxImageSize.Width) Then Return False
            If (img.Height > Me.m_szMaxImageSize.Height) Then Return False
            Return True
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; states whether a given index is valid to indicate
        ''' an image in this control.
        ''' </summary>
        ''' <param name="iIndex">The index to validate.</param>
        ''' <returns>True if valid, False otherwise.</returns>
        ''' -----------------------------------------------------------------------
        Private Function IsValidImageIndex(ByVal iIndex As Integer) As Boolean
            Return ((iIndex >= 0) And (iIndex < Me.GetImageCount()))
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the index of an image hash code in this control.
        ''' </summary>
        ''' <param name="abtHash">Image hash code, obtained from 
        ''' <see cref="GetImageHashCode">GetImageHashCode</see>.</param>
        ''' <returns>A value equal to or greater than 0 representing the hash code 
        ''' index, or -1 if the image hash code was not found.</returns>
        ''' -----------------------------------------------------------------------
        Private Function GetHashCodeIndex(ByVal abtHash As Byte()) As Integer
            Dim iIndex As Integer = 0
            For iIndex = 0 To Me.GetImageCount() - 1
                If Me.EqualsHashCodes(abtHash, Me.m_labtImageHashCodes(iIndex)) Then Return iIndex
            Next
            Return -1
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; states whether two image hash codes equal.
        ''' </summary>
        ''' <param name="abt1">First hash code to compare. Cannot be NULL.</param>
        ''' <param name="abt2">Second hash code to compare. Cannot be NULL.</param>
        ''' <returns>True if the hash codes are equal in length and content, false otherwise.</returns>
        ''' -----------------------------------------------------------------------
        Private Function EqualsHashCodes(ByVal abt1 As Byte(), ByVal abt2 As Byte()) As Boolean

            ' Sanity checks
            Debug.Assert(abt1 IsNot Nothing)
            Debug.Assert(abt2 IsNot Nothing)

            ' Compare the hash values
            Dim iByte As Integer = 0

            ' Fail if different lengths
            If (abt1.Length <> abt2.Length) Then Return False
            ' Compare actual bytes
            While (iByte < abt1.Length)
                ' Fail if different byte found
                If (abt1(iByte) <> abt2(iByte)) Then Return False
                ' Next
                iByte += 1
            End While
            ' No failures? Kewl, the hash codes are the same.
            Return True

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; build an image hash code.
        ''' </summary>
        ''' <param name="img">The image to build a hash code for.</param>
        ''' <returns>A hash code.</returns>
        ''' -----------------------------------------------------------------------
        Private Function GetImageHashCode(ByVal img As Image) As Byte()

            Dim bmp As Bitmap = New Bitmap(img)
            Dim shaM As New SHA256Managed()
            Dim ic As System.Drawing.ImageConverter = New System.Drawing.ImageConverter()
            Dim abtImage(1) As Byte

            ' Compute a hash for this image
            abtImage = DirectCast(ic.ConvertTo(bmp, abtImage.GetType()), Byte())
            Return shaM.ComputeHash(abtImage)

        End Function

#End Region ' Internal implementation

    End Class

End Namespace ' Controls
