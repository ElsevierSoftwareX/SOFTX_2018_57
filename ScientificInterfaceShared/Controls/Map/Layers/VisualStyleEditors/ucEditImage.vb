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
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Style
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control for editing the image part of a <see cref="cVisualStyle"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucEditImage

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="uic">UIContext to operate onto.</param>
        ''' <param name="vs">The <see cref="cVisualStyle"/> to create the editor for.</param>
        ''' <param name="style">Aspect of the style that needs editing.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal vs As cVisualStyle, _
                       ByVal style As cVisualStyle.eVisualStyleTypes)
            MyBase.New(uic, vs, style)
            Me.InitializeComponent()
        End Sub

#End Region ' Constructor

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Dim avs As cVisualStyle() = Me.UIContext.StyleGuide.GetVisualStyles(-1, cStyleGuide.eBrushType.Glyphs)

            For Each vs As cVisualStyle In avs
                Me.m_glyphSelect.AddImage(vs.Image)
            Next
            Me.m_glyphSelect.AddImage(Me.VisualStyle.Image)
            Me.m_glyphSelect.SelectedImage = Me.VisualStyle.Image
            Me.m_glyphSelect.Enabled = True
            Me.m_btnImport.Enabled = True
        End Sub

        Private Sub OnAddImage(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_btnImport.Click

            Dim img As Image = Nothing
            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmdFO As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)

            cmdFO.Invoke(SharedResources.FILEFILTER_IMAGE)

            If (cmdFO.Result = System.Windows.Forms.DialogResult.OK) Then
                Try
                    ' Create image
                    img = Image.FromFile(cmdFO.FileName)
                    ' Add image
                    If Me.m_glyphSelect.AddImage(img) Then
                        ' Select it
                        Me.m_glyphSelect.SelectedImage = img
                    Else
                        ' Warn user
                        Dim msg As New cMessage(My.Resources.PROMPT_FILEIMPORT_INVALIDIMAGEFORGLYPH, eMessageType.DataImport, EwEUtils.Core.eCoreComponentType.External, eMessageImportance.Warning)
                        Me.UIContext.Core.Messages.SendMessage(msg)
                    End If
                Catch ex As Exception
                    ' Neh
                End Try
            End If
        End Sub

        Private Sub m_glyphSelect_OnSelectionChanged(ByVal sender As ucGlyphSelect, ByVal e As System.EventArgs) _
            Handles m_glyphSelect.OnSelectionChanged
            Me.FireStyleChangedEvent()
        End Sub

#End Region ' Events

#Region " Overridables "

        Public Overrides Function Apply(ByVal vs As cVisualStyle) As Boolean
            Dim img As Image = Me.m_glyphSelect.SelectedImage
            If (img IsNot Nothing) Then
                vs.Image = Me.m_glyphSelect.SelectedImage
            End If
            Return True
        End Function

#End Region ' Overridables

    End Class

End Namespace
