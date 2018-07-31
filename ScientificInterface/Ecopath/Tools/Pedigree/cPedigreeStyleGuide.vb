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

#End Region ' Imports

Namespace Ecopath.Tools

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Helper class for formatting pedigree control content.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cPedigreeStyleGuide

#Region " Private vars "

        ''' <summary>The UI context to format against.</summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary>Current active render style.</summary>
        Private m_renderstyle As eRenderStyleTypes = eRenderStyleTypes.Colors

#End Region ' Private vars

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constjuctoj.
        ''' </summary>
        ''' <param name="uic"></param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext)
            Me.m_uic = uic
        End Sub

#Region " Formatting "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the background colour for rendering pedigree information, 
        ''' considering the <paramref name="style">provided</paramref> and 
        ''' <see cref="RenderStyle">default</see> <see cref="eRenderStyleTypes">render styles</see>.
        ''' </summary>
        ''' <param name="clrBack">The default background colour to use if this method
        ''' does if no alternative is found.</param>
        ''' <param name="level">The <see cref="cPedigreeLevel">level</see> to render.</param>
        ''' <param name="style">The <see cref="eRenderStyleTypes">render style</see> to
        ''' use, or <see cref="eRenderStyleTypes.NotSet">NotSet</see> to use the
        ''' <see cref="RenderStyle">present render style</see>.</param>
        ''' <returns>A color.</returns>
        ''' -------------------------------------------------------------------
        Public Function BackgroundColor(ByVal clrBack As Color, _
                                        ByVal level As cPedigreeLevel, _
                                        Optional ByVal style As eRenderStyleTypes = eRenderStyleTypes.NotSet) As Color

            ' Fix up render style
            If (style = eRenderStyleTypes.NotSet) Then style = Me.m_renderstyle

            ' Bail out!
            If (level Is Nothing) Then Return clrBack

            ' Do colour magic
            Select Case style
                Case eRenderStyleTypes.Colors
                    ' Use colour defined in the style guide for this level.
                    Return Me.m_uic.StyleGuide.PedigreeColor(Me.m_uic.Core, level.VariableName, level.Sequence)
            End Select

            ' Return provided default
            Return clrBack

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns text that represents the given level, considering the
        ''' <paramref name="style">provided</paramref> and 
        ''' <see cref="RenderStyle">default</see> <see cref="eRenderStyleTypes">render styles</see>.
        ''' </summary>
        ''' <param name="level">The <see cref="cPedigreeLevel">level</see> to render.</param>
        ''' <param name="iValueAlt">ALternate value to display if the level does not exist.</param>
        ''' <param name="style">The <see cref="eRenderStyleTypes">render style</see> to
        ''' use, or <see cref="eRenderStyleTypes.NotSet">NotSet</see> to use the
        ''' <see cref="RenderStyle">present render style</see>.</param>
        ''' <returns>A text that represents the given level, considering the
        ''' <paramref name="style">provided</paramref> and <see cref="RenderStyle">selected</see> render styles.</returns>
        ''' -------------------------------------------------------------------
        Public Function DisplayText(ByVal level As cPedigreeLevel, ByVal iValueAlt As Integer,
                                    Optional ByVal style As eRenderStyleTypes = eRenderStyleTypes.NotSet) As String

            ' Fix up render style
            If (style = eRenderStyleTypes.NotSet) Then style = Me.m_renderstyle

            If (iValueAlt > 0) Then Return Me.m_uic.StyleGuide.FormatNumber(iValueAlt)
            If (level Is Nothing) Then Return ""

            ' Decide on string to display
            Select Case style

                Case eRenderStyleTypes.Colors
                    ' NOP

                Case eRenderStyleTypes.Index
                    ' Represent level by its index (local to its manager)
                    Dim iValue As Integer = level.Sequence
                    If (iValue < 0) Then Return ""
                    Return Me.m_uic.StyleGuide.FormatNumber(iValue)

                Case eRenderStyleTypes.IndexValue
                    ' Represent level by its IndexValue
                    Return Me.m_uic.StyleGuide.FormatNumber(level.IndexValue)

                Case eRenderStyleTypes.ConfidenceInterval
                    ' Represent level by its ConfidenceInterval
                    Dim iValue As Integer = level.ConfidenceInterval
                    If (iValue <= 0) Then Return ""
                    Return Me.m_uic.StyleGuide.FormatNumber(iValue, cStyleGuide.eStyleFlags.OK)

            End Select

            ' Return default
            Return ""

        End Function

#End Region ' Formatting

#Region " Render style "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Different render styles for pedigree information.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eRenderStyleTypes As Integer
            ''' <summary>Render style has not been provided.</summary>
            NotSet = 0
            ''' <summary>Render pedigree cells as colours.</summary>
            Colors
            ''' <summary>Render pedigree cells by <see cref="cPedigreeLevel.Sequence">level sequence</see>.</summary>
            Index
            ''' <summary>Render pedigree cells by <see cref="cPedigreeLevel.IndexValue">index value</see>.</summary>
            IndexValue
            ''' <summary>Render pedigree cells by <see cref="cPedigreeLevel.IndexValue">confidence interval percentages</see>.</summary>
            ConfidenceInterval
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event for responding to a <see cref="RenderStyle">change in default render style</see>.
        ''' </summary>
        ''' <param name="sender">The style guide sending the event.</param>
        ''' -------------------------------------------------------------------
        Public Event OnRenderStyleChanged(ByVal sender As cPedigreeStyleGuide)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the default render style to use.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property RenderStyle() As eRenderStyleTypes
            Get
                Return Me.m_renderstyle
            End Get
            Set(ByVal value As eRenderStyleTypes)
                If (value <> Me.m_renderstyle) Then
                    Me.m_renderstyle = value
                    RaiseEvent OnRenderStyleChanged(Me)
                End If
            End Set
        End Property

#End Region ' Render style

    End Class

End Namespace
