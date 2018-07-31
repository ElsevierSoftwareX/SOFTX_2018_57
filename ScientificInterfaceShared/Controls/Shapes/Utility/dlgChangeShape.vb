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

Imports System.Drawing.Drawing2D
Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style

#End Region ' Imports

' To consider (after VC comment 21 May 18): 
'   Draw shapes With axis information. This logic Is now stored In SketchPad, And can perhaps be moved To the shape handlers
'   using a shetchpad is not an option, as this only operates on live shapes with core connections. For this we'd need to
'   rework the shapes so we can use them detached from the core. A lot of work for little gain.

Namespace Controls

    ''' <summary>
    ''' Interface to set the contour of a given shape to a 'common' primitive
    ''' </summary>
    ''' <remarks>
    ''' This code is decreasingly based on frmShaper.vb in EwE5.
    ''' </remarks>
    Public Class dlgChangeShape

#Region " Private vars "

        ''' <summary></summary>
        Private m_uic As cUIContext = Nothing
        ''' <summary></summary>
        Private m_shape As cForcingFunction = Nothing
        ''' <summary></summary>
        Private m_handler As cShapeGUIHandler = Nothing
 
#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal uic As cUIContext, ByVal shape As cForcingFunction)

            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)

            Me.InitializeComponent()

            ' Sanity checks
            Debug.Assert(uic IsNot Nothing)
            Debug.Assert(shape IsNot Nothing)

            ' Init
            Me.m_uic = uic
            Me.m_controlpanel.UIContext = uic

            Me.m_shape = shape
            Me.m_controlpanel.Shape = shape

            Me.m_handler = cShapeGUIHandler.GetShapeUIHandler(shape, uic)

            'Debug.Print("Change Shape " + Me.m_shape.ToCSVString())

        End Sub

#End Region ' Constructor

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            ' Add shape name 
            Me.m_tbxName.Text = Me.m_shape.Name
            Me.m_cbShowExtraData.Checked = False ' Me.m_uic.StyleGuide.ShowExtraShapeData
            Me.UpdatePreview()
            Me.UpdateControls()
            Me.CenterToScreen()

        End Sub

        Private Sub OnToggleShowExtraData(sender As Object, e As EventArgs) _
            Handles m_cbShowExtraData.CheckedChanged
            Me.UpdatePreview()
        End Sub

        Private Sub OnDefaults(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btDefaults.Click

            Me.m_controlpanel.Defaults()
            Me.UpdatePreview()

        End Sub

        Private Sub OnOk(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnOk.Click

            Dim fs As IShapeFunction = Me.m_controlpanel.SelectedShapeFunction()
            If (fs Is Nothing) Then Return

            Me.m_shape.Name = Me.m_tbxName.Text
            fs.Apply(Me.m_shape)

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()

        End Sub

        Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnCancel.Click
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Close()
        End Sub

        Private Sub OnNameChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_tbxName.TextChanged

            Me.UpdateControls()

        End Sub

        Private Sub OnShapeFunctionChanged() Handles m_controlpanel.OnShapeFunctionChanged
            Me.UpdatePreview()
            Debug.Print("OnShapeFunctionChanged " + Me.m_shape.ToCSVString())
        End Sub

        Private Sub OnPaintPreview(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) _
            Handles m_plPreview.Paint

            Try

                Dim fs As IShapeFunction = Me.m_controlpanel.SelectedShapeFunction()
                If (fs Is Nothing) Then Return

                Dim sDataMax As Single = 0.0
                Dim g As Graphics = e.Graphics
                Dim rc As Rectangle = Me.m_plPreview.ClientRectangle
                Dim data As Single() = fs.Shape(Me.nDataPoints)
                Dim iNumPoints As Integer = data.Length - 1

                For Each s As Single In data
                    sDataMax = Math.Max(s, sDataMax)
                Next

                Using br As New SolidBrush(Me.m_plPreview.BackColor)
                    g.FillRectangle(br, rc)
                End Using

                cShapeImage.DrawShapeDirect(Me.m_uic,
                                            data, Me.nDisplayPoints, Me.m_shape.IsSeasonal,
                                            Me.m_plPreview.ClientRectangle, e.Graphics, Me.m_handler.Color,
                                            Me.m_handler.SketchDrawMode(Me.m_shape),
                                            sDataMax / 0.8!, cCore.NULL_VALUE, cCore.NULL_VALUE)

                Using br As New HatchBrush(HatchStyle.SmallConfetti, Color.FromArgb(100, 0, 0, 0), Color.Transparent)
                    Dim x As Integer = CInt(Math.Ceiling(rc.Width * Me.nDataPoints / Me.nDisplayPoints))
                    g.FillRectangle(br, New Rectangle(x, 0, rc.Width, rc.Height))
                End Using

            Catch ex As Exception

            End Try

        End Sub

#End Region ' Events

#Region " Internals "

        Private Sub UpdateControls()

            Dim bHasName As Boolean = Not String.IsNullOrWhiteSpace(Me.m_tbxName.Text)
            Me.m_btnOk.Enabled = bHasName

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Redraw the shape
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdatePreview()
            ' Me.m_plPreview.Invalidate()
            Me.m_plPreview.Refresh()
        End Sub

        Private Function nDisplayPoints() As Integer
            If (Not Me.m_cbShowExtraData.Checked) Then Return Me.nDataPoints
            If Me.m_shape.IsSeasonal Then Return cCore.N_MONTHS
            Return Me.m_shape.ShapeData.Length - 1
        End Function

        Private Function nDataPoints() As Integer
            If Me.m_shape.IsSeasonal Then Return cCore.N_MONTHS
            If Me.m_shape.DataType = EwEUtils.Core.eDataTypes.Forcing Then
                Return Me.m_uic.Core.nEcosimYears * cCore.N_MONTHS
            End If
            Return Me.m_shape.nPoints
        End Function

#End Region ' Internals

    End Class

End Namespace

