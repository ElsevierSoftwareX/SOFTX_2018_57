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

Option Explicit On
Option Strict On

Imports ScientificInterfaceShared.Definitions

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Dialog that allows a user to modify how a <see cref="ucSketchPad">sketch pad</see>
    ''' displays a <see cref="EwECore.cShapeData">shape</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class dlgGraphDisplayOptions

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        ''' <summary></summary>
        Private m_sketchpad As ucSketchPad = Nothing
        ''' <summary></summary>
        Private m_fbYMax As cEwEFormatProvider = Nothing

#End Region ' Private vars

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this dialog.
        ''' </summary>
        ''' <param name="sketchPad"><see cref="ucSketchPad">sketch pad</see> to modify.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal sketchPad As ucSketchPad)

            Me.InitializeComponent()

            Me.m_uic = uic
            Me.m_sketchpad = sketchPad
            Me.CenterToParent()

        End Sub

#End Region ' Constructor

#Region " Internal implementation "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Apply the contents of the dialog to the connected <see cref="ucSketchPad">sketch pad</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub Apply()

            ' Show marks or not
            Me.m_sketchpad.DisplayAxis = Me.m_cbShowScaleAndTitle.Checked

            ' Do we need auto scale? 
            If Me.m_cbAutoScale.Checked Then
                Me.m_sketchpad.YAxisAutoScaleMode = eAxisAutoScaleModeTypes.Auto
            Else
                Me.m_sketchpad.YAxisAutoScaleMode = eAxisAutoScaleModeTypes.Fixed
            End If

            '' Do we want mouse right click auto scale?
            'If Me.cbRightClickAutoScale.Checked Then
            '    Me.m_SketchPad.RightClickAutoScaleMode = eRightClickAutoScaleModeTypes.Auto
            'Else
            '    Me.m_SketchPad.RightClickAutoScaleMode = eRightClickAutoScaleModeTypes.Fixed
            'End If

            ' Set display mode
            If Me.m_rbFill.Checked Then Me.m_sketchpad.SketchDrawMode = eSketchDrawModeTypes.Fill
            If Me.m_rbLine.Checked Then Me.m_sketchpad.SketchDrawMode = eSketchDrawModeTypes.Line
            If Me.m_rbTSDriver.Checked Then Me.m_sketchpad.SketchDrawMode = eSketchDrawModeTypes.TimeSeriesDriver
            If Me.m_rbTSRefAbs.Checked Then Me.m_sketchpad.SketchDrawMode = eSketchDrawModeTypes.TimeSeriesRefAbs
            If Me.m_rbTSRefRel.Checked Then Me.m_sketchpad.SketchDrawMode = eSketchDrawModeTypes.TimeSeriesRefRel

            ' The Y scale
            Me.m_sketchpad.YAxisMaxValue = CSng(Me.m_fbYMax.Value)

        End Sub

#End Region ' Internal implementation

#Region " Events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; populates the dialog for use by plundering its 
        ''' settings from the attached <see cref="ucSketchPad">sketch pad</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            'Initialization of the interface controls
            Me.m_fbYMax = New cEwEFormatProvider(Me.m_uic, Me.m_nudMax, GetType(Single))

            Me.m_cbShowScaleAndTitle.Checked = Me.m_sketchpad.DisplayAxis

            ' Rendering method
            Select Case Me.m_sketchpad.SketchDrawMode
                Case eSketchDrawModeTypes.Fill
                    Me.m_rbFill.Checked = True
                Case eSketchDrawModeTypes.Line
                    Me.m_rbLine.Checked = True
                Case eSketchDrawModeTypes.TimeSeriesDriver
                    Me.m_rbTSDriver.Checked = True
                Case eSketchDrawModeTypes.TimeSeriesRefAbs
                    Me.m_rbTSRefAbs.Checked = True
                Case eSketchDrawModeTypes.TimeSeriesRefRel
                    Me.m_rbTSRefRel.Checked = True
            End Select

            ' Is mediation sketch pad?
            If (Me.m_sketchpad.ShapeCategory = eShapeCategoryTypes.Mediation) Then
                ' #Yes: not allowed to rescale
                Me.m_cbAutoScale.Enabled = False
                'Me.cbRightClickAutoScale.Enabled = False
                Me.m_fbYMax.Enabled = False
            Else
                ' #No: scale ahead, Wanda!
                Me.m_cbAutoScale.Checked = (Me.m_sketchpad.YAxisAutoScaleMode = eAxisAutoScaleModeTypes.Auto)
                'Me.cbRightClickAutoScale.Checked = (m_SketchPad.RightClickAutoScaleMode = eRightClickAutoScaleModeTypes.Auto)
            End If

        End Sub

        Private Sub dlgGraphDisplayOptions_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) _
            Handles Me.FormClosing

            Me.m_fbYMax.Release()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; okidokionizes the dialog.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
            Me.Apply()
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; cancels the dialog.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        End Sub

#End Region ' Events

    End Class

End Namespace
