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

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization
Imports System.Reflection
Imports EwEUtils

#End Region ' Imports 

Namespace Auxiliary

    <Serializable()> _
    Public NotInheritable Class cVisualStyle

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper class, stores custom visualization information for data entities.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Enum eVisualStyleTypes As Integer
            NotSet = 0
            ForeColor = 1
            BackColor = 2
            Hatch = 4
            Image = 8
            Font = 16
            Gradient = 32
        End Enum

        Private m_hatchStyle As HatchStyle = Drawing2D.HatchStyle.DiagonalCross
        Private m_clrFore As Color = Color.Black
        Private m_clrBack As Color = Color.Transparent
        Private m_img As Image = Nothing
        Private m_strFontName As String = "Arial"
        Private m_sFontSize As Single = 8.0!
        Private m_fontstyle As FontStyle = FontStyle.Regular
        Private m_gradientColors As Color() = Nothing
        Private m_gradientBreaks As Double() = Nothing
        <NonSerialized()> _
        Private m_container As cAuxiliaryData = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()
        End Sub

        Public Sub New(ByVal container As cAuxiliaryData, Optional ByVal bUpdate As Boolean = False)
            Me.m_container = container
            container.AllowValidation = bUpdate
            container.VisualStyle = Me
            container.AllowValidation = True
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Create a clone of a Visual Style instance.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Function Clone() As cVisualStyle

            Dim vs As New cVisualStyle()
            SyncLock GetType(cVisualStyle)

                vs.ForeColour = Me.ForeColour
                vs.BackColour = Me.BackColour
                vs.HatchStyle = Me.HatchStyle
                vs.FontName = Me.FontName
                vs.FontSize = Me.FontSize
                vs.FontStyle = Me.FontStyle
                If Me.Image IsNot Nothing Then
                    vs.Image = DirectCast(Me.Image.Clone(), Image)
                Else
                    vs.Image = Nothing
                End If
                vs.GradientBreaks = Me.GradientBreaks
                vs.GradientColors = Me.GradientColors

            End SyncLock

            Return vs

        End Function

        Public Sub Read(vs As cVisualStyle)
            If (vs Is Nothing) Then Return
            Me.ForeColour = vs.ForeColour
            Me.BackColour = vs.BackColour
            Me.HatchStyle = vs.HatchStyle
            Me.FontName = vs.FontName
            Me.FontStyle = vs.FontStyle
            If vs.Image IsNot Nothing Then
                Me.Image = DirectCast(vs.Image.Clone(), Image)
            Else
                Me.Image = Nothing
            End If
            Me.GradientBreaks = vs.GradientBreaks
            Me.GradientColors = vs.GradientColors
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="Color">foreground colour</see> for a visual style, if any.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property ForeColour() As Color
            Get
                Return Me.m_clrFore
            End Get
            Set(ByVal value As Color)
                If (value <> Me.m_clrFore) Then
                    Me.m_clrFore = value
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="Color">background colour</see> for a visual style, if any.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property BackColour() As Color
            Get
                Return Me.m_clrBack
            End Get
            Set(ByVal value As Color)
                If (value <> Me.m_clrBack) Then
                    Me.m_clrBack = value
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the image for a visual style, if any.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property Image() As Image
            Get
                Return Me.m_img
            End Get
            Set(ByVal value As Image)
                If Not Equals(value, Me.m_img) Then
                    Me.m_img = value
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="HatchStyle">hatch style</see> for a visual style, if any.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property HatchStyle() As HatchStyle
            Get
                Return Me.m_hatchStyle
            End Get
            Set(ByVal value As HatchStyle)
                If (value <> Me.m_hatchStyle) Then
                    Me.m_hatchStyle = value
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="Font.Name">font name</see> for a visual style, if any.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property FontName() As String
            Get
                Return Me.m_strFontName
            End Get
            Set(ByVal value As String)
                If (value <> Me.m_strFontName) Then
                    Me.m_strFontName = value
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="Font.Size">font size</see> for a visual style, if any.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property FontSize() As Single
            Get
                Return Me.m_sFontSize
            End Get
            Set(ByVal value As Single)
                If (value <> Me.m_sFontSize) Then
                    Me.m_sFontSize = value
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="Font.Style">font style</see> for a visual style, if any.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property FontStyle() As FontStyle
            Get
                Return Me.m_fontstyle
            End Get
            Set(ByVal value As FontStyle)
                If (value <> Me.m_fontstyle) Then
                    Me.m_fontstyle = value
                    Me.Update()
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the break values for a gradient.
        ''' </summary>
        ''' <remarks>
        ''' The number of gradient breaks should match the number of <see cref="GradientColors">
        ''' gradient colours</see>.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Property GradientBreaks As Double()
            Get
                Return Me.m_gradientBreaks
            End Get
            Set(ByVal value As Double())
                Me.m_gradientBreaks = value
                Me.Update()
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the color values for a gradient.
        ''' </summary>
        ''' <remarks>
        ''' The number of gradient colours should match the number of <see cref="GradientBreaks">
        ''' gradient breaks</see>.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Property GradientColors As Color()
            Get
                Return Me.m_gradientColors
            End Get
            Set(ByVal value As Color())
                Me.m_gradientColors = value
                Me.Update()
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' States whether a style equals another.
        ''' </summary>
        ''' <param name="obj">The visual style to compare to.</param>
        ''' -----------------------------------------------------------------------
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If Not (TypeOf obj Is cVisualStyle) Then Return False

            Dim vs As cVisualStyle = DirectCast(obj, cVisualStyle)
            If Me.ForeColour <> vs.ForeColour Then Return False
            If Me.BackColour <> vs.BackColour Then Return False
            If Me.HatchStyle <> vs.HatchStyle Then Return False
            If String.Compare(Me.FontName, vs.FontName, True) <> 0 Then Return False
            If Me.FontSize <> vs.FontSize Then Return False
            If Me.FontStyle <> vs.FontStyle Then Return False
            If Me.Image IsNot Nothing Or vs.Image IsNot Nothing Then
                If Me.Image Is Nothing Then Return False
                If vs.Image Is Nothing Then Return False
                Return Me.Image.Equals(vs.Image)
            End If
            If Me.GradientColors IsNot Nothing Then
                If Not Me.GradientColors.EqualsArray(vs.GradientColors) Then Return False
            End If
            If Me.GradientBreaks IsNot Nothing Then
                If Not Me.GradientBreaks.EqualsArray(vs.GradientBreaks) Then Return False
            End If
            Return True
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the Auxillary data that contains this visual style.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Friend Property Container() As cAuxiliaryData
            Get
                Return Me.m_container
            End Get
            Set(ByVal value As cAuxiliaryData)
                Me.m_container = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Update visual style content change to the core.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub Update()
            If Me.m_container IsNot Nothing Then
                Me.m_container.Update()
            End If
        End Sub

    End Class ' cVisualStyle

    ''' ===========================================================================
    ''' <summary>
    ''' Helper class for serializing a visual style to text.
    ''' </summary>
    ''' ===========================================================================
    Public Class cVisualStyleReader

        Public Shared Function StyleToString(ByVal vs As cVisualStyle) As String

            Dim strResult As String = String.Empty
            Dim bf As New Binary.BinaryFormatter()
            Dim ms As New MemoryStream()

            If (vs Is Nothing) Then Return ""

            ' Write object to mem stream
            bf.AssemblyFormat = FormatterAssemblyStyle.Simple
            bf.Serialize(ms, vs)
            strResult = System.Convert.ToBase64String(ms.ToArray(), Base64FormattingOptions.None)

            ms.Close()
            ms = Nothing

            Return strResult

        End Function

        Private Class cVisualStyleNamespaceMapper
            Inherits SerializationBinder

            Public Overrides Function BindToType(assemblyName As String, strType As String) As System.Type

                If (strType.Contains("EwECore")) Then
                    Select Case strType
                        Case "EwECore.cVisualStyle"
                            strType = GetType(cVisualStyle).ToString
                    End Select
                End If

                For Each ass As Assembly In AppDomain.CurrentDomain.GetAssemblies()
                    Dim t As Type = ass.GetType(strType, False, True)
                    If (t IsNot Nothing) Then
                        Return t
                    End If
                Next
                Return Nothing

            End Function

        End Class

        Public Shared Function StringToStyle(ByVal str As String) As cVisualStyle

            Dim vsResult As cVisualStyle = Nothing

            If String.IsNullOrEmpty(str) Then Return vsResult

            Dim bf As New BinaryFormatter()
            Dim ms As MemoryStream = Nothing
            Dim ab As Byte() = Nothing

            ' Ignore assembly version differences
            bf.AssemblyFormat = FormatterAssemblyStyle.Simple
            ' Perform type mapping
            bf.Binder = New cVisualStyleNamespaceMapper()

            Try
                ab = System.Convert.FromBase64String(str)
                ms = New MemoryStream(ab)
                vsResult = CType(bf.Deserialize(ms), cVisualStyle)
            Catch ex As Exception

            End Try
            Return vsResult

        End Function

    End Class

End Namespace ' Auxillary
