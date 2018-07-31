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
Option Explicit On

Imports System.IO
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Controls.Map.Layers

#End Region

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control; implements the Options > Map settings interface.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucOptionsMap
        Implements IOptionsPage
        Implements IUIElement

#Region " Variables "

        Private m_fpSouth As cEwEFormatProvider = Nothing
        Private m_fpNorth As cEwEFormatProvider = Nothing
        Private m_fpWest As cEwEFormatProvider = Nothing
        Private m_fpEast As cEwEFormatProvider = Nothing

        Private m_layerBack As cDisplayLayerImage = Nothing
        Private m_layerPreview As cDisplayLayerImage = Nothing
        Private m_imgPreview As Image = Nothing

#End Region ' Variables

#Region " Constructors "

        Public Sub New(ByVal uic As cUIContext)

            Me.UIContext = uic
            Me.InitializeComponent()

            Me.m_fpNorth = New cEwEFormatProvider(Me.UIContext, Me.m_nudNorth, GetType(Single))
            Me.m_fpSouth = New cEwEFormatProvider(Me.UIContext, Me.m_nudSouth, GetType(Single))
            Me.m_fpEast = New cEwEFormatProvider(Me.UIContext, Me.m_nudEast, GetType(Single))
            Me.m_fpWest = New cEwEFormatProvider(Me.UIContext, Me.m_nudWest, GetType(Single))

            AddHandler Me.m_fpNorth.OnValueChanged, AddressOf OnExtentChanged
            AddHandler Me.m_fpSouth.OnValueChanged, AddressOf OnExtentChanged
            AddHandler Me.m_fpEast.OnValueChanged, AddressOf OnExtentChanged
            AddHandler Me.m_fpWest.OnValueChanged, AddressOf OnExtentChanged

            Me.m_layerBack = New cDisplayLayerImage(Me.UIContext, My.Resources.urf)
            Me.m_layerBack.ImageTL = New PointF(-180, 90)
            Me.m_layerBack.ImageBR = New PointF(180, -90)

            Me.m_layerPreview = New cDisplayLayerImage(Me.UIContext, Nothing)

        End Sub

#End Region ' Constructors

#Region " Event handlers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Control's load event which gets called every time the control gets loaded. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            Me.UpdateControls()
            Me.UpdatePreviewImage()

            Me.m_cbShowExcludedCells.Checked = Me.UIContext.StyleGuide.ShowMapsExcludedCells

        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)

            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If

            Me.m_layerBack.Dispose()
            Me.m_layerBack = Nothing

            Me.m_layerPreview.Dispose()
            Me.m_layerPreview = Nothing

            RemoveHandler Me.m_fpNorth.OnValueChanged, AddressOf OnExtentChanged
            RemoveHandler Me.m_fpSouth.OnValueChanged, AddressOf OnExtentChanged
            RemoveHandler Me.m_fpEast.OnValueChanged, AddressOf OnExtentChanged
            RemoveHandler Me.m_fpWest.OnValueChanged, AddressOf OnExtentChanged

            Me.m_fpNorth.Release()
            Me.m_fpNorth = Nothing

            Me.m_fpSouth.Release()
            Me.m_fpSouth = Nothing

            Me.m_fpEast.Release()
            Me.m_fpEast = Nothing

            Me.m_fpWest.Release()
            Me.m_nudWest = Nothing

            If (Me.m_imgPreview IsNot Nothing) Then
                Me.m_imgPreview.Dispose()
                Me.m_imgPreview = Nothing
            End If

            MyBase.Dispose(disposing)
        End Sub

        Private Sub OnChooseImage(sender As System.Object, e As System.EventArgs) _
            Handles m_btnChoose.Click

            If (Me.UIContext Is Nothing) Then Return

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim cmdFO As cFileOpenCommand = DirectCast(cmdh.GetCommand(cFileOpenCommand.COMMAND_NAME), cFileOpenCommand)

            cmdFO.Title = My.Resources.PROMPT_SELECT_REFIMAGE
            cmdFO.Invoke(Me.m_tbxFile.Text, My.Resources.FILEFILTER_IMAGE_TRANSPARENT, 0)

            If (cmdFO.Result = DialogResult.OK) Then
                Me.m_tbxFile.Text = cmdFO.FileName
                Me.UpdatePreviewImage()
            End If

        End Sub

        Private Sub OnExtentChanged(sender As Object, e As EventArgs) _
            Handles m_nudNorth.Validated, m_nudSouth.Validated, m_nudEast.Validated, m_nudWest.Validated

            Me.UpdatePreviewImageExtent()

        End Sub

        Private Sub OnPaintPreview(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles m_plPreview.Paint

            Using br As New SolidBrush(Me.BackColor)
                e.Graphics.FillRectangle(br, Me.ClientRectangle)
            End Using

            If (Me.m_layerPreview Is Nothing) Or (Me.m_layerBack Is Nothing) Then Return

            Dim iSize As Integer = Math.Min(Me.m_plPreview.Height, CInt(Me.m_plPreview.Width / 2))
            Dim rcMap As New Rectangle(CInt(Me.m_plPreview.Width / 2) - iSize, CInt((Me.m_plPreview.Height - iSize) / 2), iSize * 2, iSize)
            Dim bError As Boolean = (Me.m_imgPreview Is Nothing)

            DirectCast(Me.m_layerBack.Renderer, cImageLayerRenderer).Render(e.Graphics, Me.m_layerBack, rcMap, Me.m_layerBack.ImageTL, Me.m_layerBack.ImageBR, cStyleGuide.eStyleFlags.OK)

            Try
                DirectCast(Me.m_layerPreview.Renderer, cImageLayerRenderer).Render(e.Graphics, Me.m_layerPreview, rcMap, Me.m_layerBack.ImageTL, Me.m_layerBack.ImageBR, cStyleGuide.eStyleFlags.OK)
            Catch ex As Exception
                bError = True
            End Try

            ' Draw error indicator
            If (bError) Then
                Dim ft As New Font("Arial", iSize / 4.0!)
                Dim br As New SolidBrush(Me.UIContext.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.MISSINGPARAMETER_BACKGROUND))
                Dim fmt As New StringFormat()
                fmt.Alignment = StringAlignment.Center
                fmt.LineAlignment = StringAlignment.Center

                e.Graphics.DrawString("?", ft, br, rcMap, fmt)

                br.Dispose()
                ft.Dispose()
            End If

        End Sub

#End Region ' Event handlers

#Region " Public methods "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext As cUIContext _
             Implements IUIElement.UIContext

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanApply"/>
        ''' -------------------------------------------------------------------
        Public Function CanApply() As Boolean _
               Implements IOptionsPage.CanApply
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.OnChanged"/>
        ''' -------------------------------------------------------------------
        Public Event OnOptionsMapChanged(sender As IOptionsPage, args As System.EventArgs) _
                Implements IOptionsPage.OnChanged

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.Apply"/>
        ''' -------------------------------------------------------------------
        Public Function Apply() As IOptionsPage.eApplyResultType _
            Implements IOptionsPage.Apply

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide

            ' Apply colors to the style guide
            sg.SuspendEvents()

            Try
                sg.MapReferenceLayerFile = Me.m_tbxFile.Text
                sg.MapReferenceLayerTL = New PointF(CSng(Me.m_fpWest.Value), CSng(Me.m_fpNorth.Value))
                sg.MapReferenceLayerBR = New PointF(CSng(Me.m_fpEast.Value), CSng(Me.m_fpSouth.Value))
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                cLog.Write(ex, "ucOptionsMap::Apply")
            End Try
            sg.ShowMapsExcludedCells = Me.m_cbShowExcludedCells.Checked

            sg.ResumeEvents()
            Return IOptionsPage.eApplyResultType.Success

        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.SetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Sub SetDefaults() _
             Implements IOptionsPage.SetDefaults

            Try
                Me.m_nudNorth.Value = CDec(My.Settings.GetDefaultValue("MapLayerRefLatMax"))
                Me.m_nudSouth.Value = CDec(My.Settings.GetDefaultValue("MapLayerRefLatMin"))
                Me.m_nudEast.Value = CDec(My.Settings.GetDefaultValue("MapLayerRefLonMax"))
                Me.m_nudWest.Value = CDec(My.Settings.GetDefaultValue("MapLayerRefLonMin"))
                Me.m_tbxFile.Text = CStr(My.Settings.GetDefaultValue("MapLayerRefFile"))
            Catch ex As Exception

            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanSetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Function CanSetDefaults() As Boolean _
            Implements IOptionsPage.CanSetDefaults
            Return True
        End Function

#End Region ' Public methods

#Region " Helper methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to enable and update UI controls.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateControls()

            If (Me.UIContext Is Nothing) Then Return

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide

            Me.m_tbxFile.Text = sg.MapReferenceLayerFile
            Me.m_fpNorth.Value = sg.MapReferenceLayerTL.Y
            Me.m_fpSouth.Value = sg.MapReferenceLayerBR.Y
            Me.m_fpEast.Value = sg.MapReferenceLayerBR.X
            Me.m_fpWest.Value = sg.MapReferenceLayerTL.X

            Me.m_plPreview.Invalidate()

        End Sub

        Private Sub UpdatePreviewImage()

            If (Me.m_imgPreview IsNot Nothing) Then
                Me.m_imgPreview.Dispose()
            End If

            Dim strFile As String = Me.m_tbxFile.Text
            If (Not String.IsNullOrWhiteSpace(strFile)) Then
                If (File.Exists(strFile)) Then
                    cApplicationStatusNotifier.StartProgress(Me.UIContext.Core)
                    Try
                        Me.m_imgPreview = Image.FromFile(strFile)
                    Catch ex As Exception
                        cLog.Write(ex, "ucOptionsMap::UpdatePreviewImage(" & strFile & ")")
                    End Try
                    cApplicationStatusNotifier.EndProgress(Me.UIContext.Core)
                End If
            End If
            Me.m_layerPreview.Image = Me.m_imgPreview
            Me.m_plPreview.Invalidate()

        End Sub

        Private Sub UpdatePreviewImageExtent()

            Me.m_layerPreview.ImageTL = New PointF(CSng(Me.m_fpWest.Value), CSng(Me.m_fpNorth.Value))
            Me.m_layerPreview.ImageBR = New PointF(CSng(Me.m_fpEast.Value), CSng(Me.m_fpSouth.Value))
            Me.m_plPreview.Invalidate()

        End Sub

#End Region ' Helper methods

    End Class

End Namespace


