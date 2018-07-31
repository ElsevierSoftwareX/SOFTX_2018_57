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

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Base class for implementing a <see cref="cVisualStyle"/> editor.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucEditVisualStyle
        Inherits UserControl
        Implements IUIElement

#Region " Factory "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Factory method for visual style editors.
        ''' </summary>
        ''' <param name="uic">UIContext to operate onto.</param>
        ''' <param name="vs">The <see cref="cVisualStyle"/> to create the editor for.</param>
        ''' <param name="style">Aspect of the style that needs editing.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetEditor(ByVal uic As cUIContext, _
                                         ByVal vs As cVisualStyle, _
                                         ByVal style As cVisualStyle.eVisualStyleTypes) As ucEditVisualStyle
            ' Sanity checks
            Debug.Assert(vs IsNot Nothing)

            If ((style And cVisualStyle.eVisualStyleTypes.Image) = cVisualStyle.eVisualStyleTypes.Image) Then
                Return New ucEditImage(uic, vs, style)
            End If

            If ((style And cVisualStyle.eVisualStyleTypes.Gradient) = cVisualStyle.eVisualStyleTypes.Gradient) Then
                Return New ucEditGradient(uic, vs, style)
            End If

            Return New ucEditHatch(uic, vs, style)

        End Function

#End Region ' Factory

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_visualStyle As cVisualStyle = Nothing
        Private m_style As cVisualStyle.eVisualStyleTypes = cVisualStyle.eVisualStyleTypes.NotSet

#End Region ' Private vars

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Default constructor for Visual Studio designer only.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Obsolete("Do not use this constructor, it's for the VS editor only")>
        Public Sub New()
            If Not Me.DesignMode Then
                Debug.Assert(False, "Please do not use this constructor!")
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="uic">UIContext to operate onto.</param>
        ''' <param name="vs">The <see cref="cVisualStyle"/> to create the editor for.</param>
        ''' <param name="style">Aspect of the style that needs editing.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, ByVal vs As cVisualStyle, ByVal style As cVisualStyle.eVisualStyleTypes)

            ' Sanity checks
            Debug.Assert(uic IsNot Nothing)
            Debug.Assert(vs IsNot Nothing)
            Debug.Assert(style <> cVisualStyle.eVisualStyleTypes.NotSet)

            Me.m_uic = uic
            Me.m_visualStyle = vs
            Me.m_style = style

        End Sub

#End Region ' Constructor

#Region " Events "

        Public Event OnVisualStyleChanged(ByVal sender As ucEditVisualStyle)

        Protected Sub FireStyleChangedEvent()

            Try
                RaiseEvent OnVisualStyleChanged(Me)
            Catch ex As Exception
                Debug.Assert(False, Me.ToString & ".FireStyleChangedEvent() Exception " & ex.Message)
                System.Console.WriteLine(Me.ToString & ".FireStyleChangedEvent() Exception " & ex.Message)
            End Try

        End Sub

#End Region ' Events

#Region " Properties "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cVisualStyle"/> that is being edited.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property VisualStyle() As cVisualStyle
            Get
                Return Me.m_visualStyle
            End Get
            Set(ByVal value As cVisualStyle)
                Me.m_visualStyle = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set how this editor represents its content
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property RepresentationStyles() As cVisualStyle.eVisualStyleTypes
            Get
                Return Me.m_style
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -----------------------------------------------------------------------
        Public Property UIContext As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                Me.m_uic = value
            End Set
        End Property

#End Region ' Properties

#Region " Overridables "

        Public Overridable Function Apply(ByVal vs As cVisualStyle) As Boolean
            Return True
        End Function

#End Region ' Overridables

        Private Sub InitializeComponent()
            Me.SuspendLayout()
            '
            'ucEditVisualStyle
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
            Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.Name = "ucEditVisualStyle"
            Me.ResumeLayout(False)

        End Sub
    End Class

End Namespace
