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

Imports System.Drawing.Drawing2D
Imports EwECore
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Ecosim

    Public Class ApplyEP

#Region "Constructor"

        Private m_EPManager As cEggProductionShapeManager
        Private m_bInUpdate As Boolean = False

        Public Sub New()
            InitializeComponent()
        End Sub

#End Region

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            ' Config manager
            Me.m_EPManager = Me.Core.EggProdShapeManager
            ' Apply resources text to Set button
            Me.m_tsbSet.Text = My.Resources.LABEL_SET
            ' Init the form to the current data
            Me.InitForm()
            ' Hook up to baseclass refresh
            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.EcoPath, eCoreComponentType.ShapesManager}

        End Sub

        Public Overrides Property UIContext() As cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(ByVal value As cUIContext)
                MyBase.UIContext = value
                Me.m_grid.UIContext = value
            End Set
        End Property

#End Region ' Overrides

#Region " Event handlers "

        Private Sub m_tscEggProdShapes_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
            Handles m_tscEggProdShapes.SelectedIndexChanged

            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True
            Me.m_grid.SelectShapeName(Me.m_tscEggProdShapes.Text)
            Me.m_bInUpdate = False

        End Sub

        Private Sub tsbSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_tsbSet.Click

            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True
            Me.m_grid.SelectShapeName(Me.m_tscEggProdShapes.Text)
            Me.m_bInUpdate = False

        End Sub

        Private Sub m_lvShapes_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_lvShapes.SelectedIndexChanged

            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True
            If m_lvShapes.SelectedItems.Count = 1 Then
                Me.m_grid.SelectShapeName(Me.m_lvShapes.SelectedItems(0).Text)
            Else
                Me.m_grid.SelectShapeName("")
            End If
            Me.m_bInUpdate = False

        End Sub

        Private Sub m_grid_OnSelectionChanged() _
            Handles m_grid.OnSelectionChanged
            Me.UpdateSetControls()
        End Sub

        Protected Overrides Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
            ' Respond to color changes and thumbnail size changes
            If (ct And (cStyleGuide.eChangeType.Thumbnails Or cStyleGuide.eChangeType.Colours)) > 0 Then
                Me.LoadShapes()
            End If
        End Sub

#End Region ' Event handlers

#Region " Internals "

        Private Sub InitForm()

            ' JS 20sep07: Pragmatic fix, also test for available egg production shapes
            If Me.Core.nStanzas > 0 And Me.Core.EggProdShapeManager.Count > 0 Then
                Me.m_tlpContent.Visible = True
                Me.m_lblNoStanza.Visible = False
                Me.LoadShapes()
            Else
                Me.m_lblNoStanza.Visible = True
                Me.m_tlpContent.Visible = False
            End If

            Me.UpdateSetControls()

        End Sub

        Private Sub LoadShapes()

            'Set up the thumbnail image size
            Dim largeImageList As New ImageList
            Dim item As ListViewItem = Nothing
            Dim rcItem As Rectangle = New Rectangle(0, 0, Me.StyleGuide.ThumbnailSize, Me.StyleGuide.ThumbnailSize)
            Dim bmp As Bitmap = Nothing
            Dim iItemIndex As Integer = 0
            Dim astrShapeNames As String()
            Dim strSelection As String = Me.m_tscEggProdShapes.Text

            If Me.m_bInUpdate Then Return

            Me.m_bInUpdate = True

            Try

                'Clear the thumbnail list
                m_lvShapes.Items.Clear()

                largeImageList.ImageSize = rcItem.Size

                If m_EPManager.Count > 0 Then

                    For Each shapeFunc As cForcingFunction In m_EPManager

                        ' Create image
                        bmp = New Bitmap(rcItem.Width, rcItem.Height)
                        Using g As Graphics = Graphics.FromImage(bmp)
                            cShapeImage.DrawShape(Me.UIContext, shapeFunc, rcItem, g, Color.Red, eSketchDrawModeTypes.Fill, cCore.NULL_VALUE, Math.Max(2.0!, shapeFunc.YMax))
                            largeImageList.Images.Add(bmp)
                        End Using

                        ' Create label
                        item = New ListViewItem(String.Format(SharedResources.GENERIC_LABEL_INDEXED, (shapeFunc.ID + 1), shapeFunc.Name))
                        item.ImageIndex = iItemIndex
                        m_lvShapes.Items.Add(item)

                        iItemIndex += 1

                    Next shapeFunc

                    m_lvShapes.View = View.LargeIcon
                    m_lvShapes.Items(0).Selected = True
                    m_lvShapes.LargeImageList = largeImageList

                End If

                Me.m_grid.ResetData()

                Me.m_tscEggProdShapes.Items.Clear()
                astrShapeNames = Me.m_grid.GetEPShapeNames()
                For Each str As String In astrShapeNames
                    Me.m_tscEggProdShapes.Items.Add(str)
                Next

                Me.m_tscEggProdShapes.SelectedIndex = Me.m_tscEggProdShapes.FindStringExact(strSelection)

            Catch ex As Exception

            End Try

            Me.m_bInUpdate = False

        End Sub

        ''' <summary>
        ''' Update the controls in the toolbar that allow the user to set a range of cells
        ''' to a specific shape. These controls implement a local version of
        ''' EwEGridForm.QuickEditHandler
        ''' </summary>
        Private Sub UpdateSetControls()

            ' Hackittyhack: enable when the shape interface is visible and the non-empty grid selection includes the shapes column
            Dim bEnabled As Boolean = (Me.m_tlpContent.Visible = True) And
                                      (Me.m_grid.Selection.GetRange.IsEmpty = False) And
                                      (Me.m_grid.Selection.GetRange.ContainsColumn(gridApplyEP.eColumnTypes.Shape))
            Me.m_tlbSet.Enabled = bEnabled
            Me.m_tscEggProdShapes.Enabled = bEnabled
            Me.m_tsbSet.Enabled = bEnabled

        End Sub

#End Region ' Internals

#Region " Mandatory overrides "

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)

            Dim bRefreshForm As Boolean = False
            Dim bRefreshShapes As Boolean = False

            ' Check for relevant messages:
            ' * Refresh on any ShapesManager EggProd message
            If ((msg.Source = eCoreComponentType.ShapesManager) And (msg.DataType = eDataTypes.EggProd)) Then
                bRefreshShapes = (msg.Type = eMessageType.DataModified)
                bRefreshForm = (msg.Type = eMessageType.DataAddedOrRemoved)
            End If

            ' * Refresh on Ecopath stanza additions or removals
            If ((msg.Source = eCoreComponentType.EcoPath) And (msg.DataType = eDataTypes.Stanza)) Then
                bRefreshForm = (msg.Type = eMessageType.DataAddedOrRemoved)
            End If

            If bRefreshForm Then
                Me.InitForm()
            Else
                If bRefreshShapes Then Me.LoadShapes()
            End If

        End Sub

#End Region ' Mandatory overrides

    End Class

End Namespace
