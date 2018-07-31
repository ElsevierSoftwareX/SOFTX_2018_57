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

Imports System.ComponentModel
Imports EwECore
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Definitions
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' ListView-based control with icons representing EwE
    ''' <see cref="cShapeData">shapes</see>.
    ''' </summary>
    ''' ------------------------------------------------------------------
    <CLSCompliant(True)> _
    Public Class ucShapeToolbox
        Implements IUIElement

#Region " Variables "

        Private m_uic As cUIContext = Nothing
        Private m_handler As cShapeGUIHandler = Nothing
        Private m_lShapes As New List(Of cShapeData)
        Private m_iMaxXScale As Integer = cCore.NULL_VALUE
        Private m_sketchDrawMode As eSketchDrawModeTypes = eSketchDrawModeTypes.Fill
        Private m_selectionDelayed As cShapeData() = Nothing

        ''' <summary>Helper flag to prevent selection loops.</summary>
        Private m_bInUpdate As Boolean = False

#End Region ' Variables

#Region " Construction / destruction "

        Public Sub New()
            Me.InitializeComponent()
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        End Sub

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)
            Me.UpdateThumbnails(m_selectionDelayed)

        End Sub

        Protected Overrides Sub Dispose(ByVal bDisposing As Boolean)

            If (bDisposing) Then

                Dim cmd As cCommand = Nothing

                If (Me.m_uic IsNot Nothing) Then

                    cmd = Me.m_uic.CommandHandler.GetCommand("WeightTimeSeries")
                    If cmd IsNot Nothing Then
                        cmd.RemoveControl(Me.ApplyToolStripMenuItem)
                    End If

                    Me.UIContext = Nothing
                End If

                If Me.m_lvShapes.LargeImageList IsNot Nothing Then
                    Me.m_lvShapes.LargeImageList.Dispose()
                    Me.m_lvShapes.LargeImageList = Nothing
                End If

                If components IsNot Nothing Then
                    components.Dispose()
                End If
            End If

            MyBase.Dispose(bDisposing)
        End Sub

#End Region ' Construction / destruction

#Region " Properties "

        ''' ------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cShapeGUIHandler">Shape GUI Handler</see>
        ''' maintaining this toolbox.
        ''' </summary>
        ''' ------------------------------------------------------------------
        Public Property Handler() As cShapeGUIHandler
            Get
                Return Me.m_handler
            End Get
            Set(ByVal handler As cShapeGUIHandler)
                Me.m_handler = handler
                Me.UpdateControls()
            End Set
        End Property

        ''' ------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the colour that should be used to render the shapes.
        ''' </summary>
        ''' ------------------------------------------------------------------
        Public Property Color() As Color

        ''' ------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the min Y-scale value for rendering thumbnails.
        ''' </summary>
        ''' ------------------------------------------------------------------
        Public Property YAxisMinValue() As Single

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the max X value for the graph.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Sketchpad"), _
         Description("State the max X value for the graph.")> _
        Public Overridable Property XAxisMaxValue() As Integer
            Get
                Return Me.m_iMaxXScale
            End Get
            Set(ByVal iValue As Integer)
                Me.m_iMaxXScale = iValue
                Me.UpdateThumbnails(Me.Selection)
                Me.Invalidate()
            End Set
        End Property

        ''' ------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether item icons should be accompanied by check boxes.
        ''' </summary>
        ''' ------------------------------------------------------------------
        Public Property AllowCheckboxes() As Boolean
            Get
                Return Me.m_lvShapes.CheckBoxes
            End Get
            Set(ByVal value As Boolean)
                Me.m_lvShapes.CheckBoxes = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the line style used to render the graph.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Category("Thumbnails"), _
         Description("The line style used to render the graph")> _
        Public Property SketchDrawMode() As eSketchDrawModeTypes
            Get
                Return Me.m_sketchDrawMode
            End Get
            Set(ByVal value As eSketchDrawModeTypes)
                Me.m_sketchDrawMode = value
                Me.Invalidate()
            End Set
        End Property

        ''' ------------------------------------------------------------------
        ''' <summary>
        ''' Update the thumbnail image for a given shape.
        ''' </summary>
        ''' <param name="shape">The shape to update the image for.</param>
        ''' ------------------------------------------------------------------
        Public Sub UpdateThumbnail(ByVal shape As cShapeData)
            If Me.m_bInUpdate Then Return
            Me.UpdateThumbnails(New cShapeData() {shape})
        End Sub

        ''' ------------------------------------------------------------------
        ''' <summary>
        ''' Sets the list of shapes to display in the toolbox, and an optional
        ''' list of shapes to select.
        ''' </summary>
        ''' <param name="lShapes"></param>
        ''' <param name="ashapeSelect"></param>
        ''' ------------------------------------------------------------------
        Public Sub SetShapes(ByVal lShapes As List(Of cShapeData), ByVal ashapeSelect As cShapeData())

            Dim shape As cShapeData = Nothing

            Me.m_lShapes.Clear()
            If (lShapes IsNot Nothing) Then
                For i As Integer = 0 To lShapes.Count - 1
                    shape = lShapes(i)
                    Me.m_lShapes.Add(shape)
                Next
            End If

            ' Update selection when redoing thumbnails
            Me.UpdateThumbnails(ashapeSelect)

        End Sub

        ''' ------------------------------------------------------------------
        ''' <summary>
        ''' Public event, notifying that the selection of shapes in the toolbox
        ''' has changed.
        ''' </summary>
        ''' <param name="ashapes">The list of selected shapes.</param>
        ''' ------------------------------------------------------------------
        Public Event OnSelectionChanged(ByVal ashapes As cShapeData())

        ''' ------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the list of selected shapes in the tool box.
        ''' </summary>
        ''' ------------------------------------------------------------------
        <Browsable(False)> _
        Public Property Selection() As cShapeData()
            Get
                Dim lShapes As New List(Of cShapeData)
                For Each item As ListViewItem In Me.m_lvShapes.SelectedItems
                    lShapes.Add(DirectCast(item.Tag, cShapeData))
                Next
                Return lShapes.ToArray()
            End Get

            Set(ByVal ashapes As cShapeData())

                If (Not Me.Created) Then
                    Me.m_selectionDelayed = ashapes
                    Return
                End If

                Dim selection As New List(Of cShapeData)
                Dim ids As New List(Of Integer)

                Me.m_lvShapes.SuspendLayout()

                Try
                    If (ashapes Is Nothing) Then
                        ' Clear all selections
                        For Each item As ListViewItem In Me.m_lvShapes.Items
                            item.Selected = False
                        Next
                    Else
                        ' JS 26Aug14: better than object comparison
                        For Each shp As cShapeData In ashapes
                            If (shp IsNot Nothing) Then
                                ids.Add(shp.DBID)
                            End If
                        Next

                        For Each item As ListViewItem In Me.m_lvShapes.Items
                            ' Get item shape
                            Dim shape As cShapeData = DirectCast(item.Tag, cShapeData)
                            ' Exists in selection?
                            If (ids.Contains(shape.DBID)) Then
                                ' #Yes: select the item
                                item.Selected = True
                                item.EnsureVisible()
                                ' Shape still exists: add to selection to broadcast
                                selection.Add(shape)
                            Else
                                item.Selected = False
                            End If
                        Next
                    End If
                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                End Try

                Me.m_lvShapes.ResumeLayout()

                If Not Me.m_bInUpdate Then

                    Me.m_bInUpdate = True
                    Me.UpdateControls()

                    Try
                        ' JS 10May11: event try/caught
                        RaiseEvent OnSelectionChanged(selection.ToArray())
                    Catch ex As Exception
                        Debug.Assert(False, ex.Message)
                    End Try

                    Me.m_bInUpdate = False
                End If

            End Set
        End Property

#End Region ' Properties

#Region " IUIElement implementation "

        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                If (Me.m_uic IsNot Nothing) Then
                    RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If
                Me.m_uic = value
                If (Me.m_uic IsNot Nothing) Then
                    AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If
            End Set
        End Property

#End Region ' IUIElement implementation

#Region " Helper methods "

        ''' <summary>
        ''' Create a thumbnail image for a shape
        ''' </summary>
        ''' <param name="shape"></param>
        Private Function GetThumbnail(ByVal shape As cShapeData) As Image

            ' Determine whether to show enabled tick
            Dim bShowWarning As Boolean = False
            Dim xMax As Integer = shape.nPoints

            If TypeOf shape Is cTimeSeries Then
                Dim ts As cTimeSeries = DirectCast(shape, cTimeSeries)
                bShowWarning = Not (ts.ValidationStatus = eStatusFlags.OK)
            End If

            Return cShapeImage.IconImage(Me.m_uic, shape, Me.Color, Me.SketchDrawMode, Me.XAxisMaxValue, Math.Max(Me.YAxisMinValue, shape.YMax), bShowWarning)

        End Function

        Private Sub UpdateControls()

            If (Me.m_handler Is Nothing) Then Return

            Me.AddToolStripMenuItem.Visible = Me.CanShowButton(cShapeGUIHandler.eShapeCommandTypes.Add)
            Me.AddToolStripMenuItem.Enabled = Me.CanEnableButton(cShapeGUIHandler.eShapeCommandTypes.Add)

            Me.ApplyToolStripMenuItem.Visible = Me.CanShowButton(cShapeGUIHandler.eShapeCommandTypes.Weight)
            Me.ApplyToolStripMenuItem.Enabled = Me.CanEnableButton(cShapeGUIHandler.eShapeCommandTypes.Weight)

            Me.DuplicateToolStripMenuItem.Visible = Me.CanShowButton(cShapeGUIHandler.eShapeCommandTypes.Duplicate)
            Me.DuplicateToolStripMenuItem.Enabled = Me.CanEnableButton(cShapeGUIHandler.eShapeCommandTypes.Duplicate)

            Me.ImportToolStripMenuItem.Visible = Me.CanShowButton(cShapeGUIHandler.eShapeCommandTypes.Import)
            Me.ImportToolStripMenuItem.Enabled = Me.CanEnableButton(cShapeGUIHandler.eShapeCommandTypes.Import)

            Me.ExportToolStripMenuItem.Visible = Me.CanShowButton(cShapeGUIHandler.eShapeCommandTypes.Export)
            Me.ExportToolStripMenuItem.Enabled = Me.CanEnableButton(cShapeGUIHandler.eShapeCommandTypes.Export)

            Me.RemoveToolStripMenuItem.Visible = Me.CanShowButton(cShapeGUIHandler.eShapeCommandTypes.Remove)
            Me.RemoveToolStripMenuItem.Enabled = Me.CanEnableButton(cShapeGUIHandler.eShapeCommandTypes.Remove)

            Me.ResetToolStripMenuItem.Visible = Me.CanShowButton(cShapeGUIHandler.eShapeCommandTypes.Reset)
            Me.ResetToolStripMenuItem.Enabled = Me.CanEnableButton(cShapeGUIHandler.eShapeCommandTypes.Reset)

            Me.ValuesToolStripMenuItem.Visible = Me.CanShowButton(cShapeGUIHandler.eShapeCommandTypes.Modify)
            Me.ValuesToolStripMenuItem.Enabled = Me.CanEnableButton(cShapeGUIHandler.eShapeCommandTypes.Modify)

            Me.ChangeShapeToolStripMenuItem.Visible = Me.CanShowButton(cShapeGUIHandler.eShapeCommandTypes.ChangeShape)
            Me.ChangeShapeToolStripMenuItem.Enabled = Me.CanEnableButton(cShapeGUIHandler.eShapeCommandTypes.ChangeShape)

            Me.RenameToolStripMenuItem.Visible = False
            Me.RenameToolStripMenuItem.Enabled = False

        End Sub

        Private Function CanShowButton(ByVal cmd As cShapeGUIHandler.eShapeCommandTypes) As Boolean
            If (Me.m_handler IsNot Nothing) Then
                Return Me.m_handler.SupportCommand(cmd)
            Else
                Return False
            End If
        End Function

        Private Function CanEnableButton(ByVal cmd As cShapeGUIHandler.eShapeCommandTypes) As Boolean
            If (Me.m_handler IsNot Nothing) Then
                Return Me.m_handler.EnableCommand(cmd)
            Else
                Return False
            End If
        End Function

        Private m_bUpdateRequested As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Order an update of thumbnails and list view items in the toolbox.
        ''' </summary>
        ''' <remarks>
        ''' This call is buffered to handle shape updates only once. A single
        ''' shape change results in a shape change message for every shape it
        ''' a manager. This is highly unpractical, but the core messaging 
        ''' system never got updated to either send one message for all shapes,
        ''' or preserve info which shape was changed. Hence this work-around.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Private Sub UpdateThumbnails(selection As cShapeData())

            Me.m_selectionDelayed = selection

            If Not Me.Created Then Return
            If Me.m_bUpdateRequested Then Return
            Me.m_bUpdateRequested = True

            Try
                Me.BeginInvoke(New MethodInvoker(AddressOf DelayUpdateThumbnails))
            Catch ex As Exception
                ' Whoah!
            End Try
        End Sub

        Private Sub DelayUpdateThumbnails()

            ' UIC may disappear in response to manager commands
            If (Me.m_uic Is Nothing) Then Return

            If (Me.m_bUpdateRequested = False) Then Return
            Me.m_bUpdateRequested = False

            Dim iThumbSize As Integer = Me.m_uic.StyleGuide.ThumbnailSize
            Dim largeImageList As New ImageList
            Dim item As ListViewItem = Nothing
            Dim shape As cShapeData = Nothing
            Dim bShowApplyTick As Boolean = False
            Dim bCanEnable As Boolean = False

            Me.m_lvShapes.SuspendLayout()
            Me.m_bInUpdate = True

            'Clear the thumbnail list
            Me.m_lvShapes.Items.Clear()

            ' Clean up
            If Me.m_lvShapes.LargeImageList IsNot Nothing Then
                Me.m_lvShapes.LargeImageList.Dispose()
                Me.m_lvShapes.LargeImageList = Nothing
            End If

            'Set up the thumbnail image size
            largeImageList.ImageSize = New System.Drawing.Size(iThumbSize, iThumbSize)

            If Me.m_lShapes.Count > 0 Then

                For i As Integer = 0 To Me.m_lShapes.Count - 1

                    shape = Me.m_lShapes(i)
                    ' Add thumbnail image
                    largeImageList.Images.Add(Me.GetThumbnail(shape))
                    ' Create list view item
                    item = New ListViewItem(String.Format(My.Resources.GENERIC_LABEL_INDEXED, shape.Index, shape.Name))
                    item.ImageIndex = i
                    item.Tag = shape
                    If TypeOf shape Is cTimeSeries Then
                        ' Determine the checked state of the listview check box
                        item.Checked = DirectCast(shape, cTimeSeries).Enabled
                    End If
                    ' Add list view item
                    Me.m_lvShapes.Items.Add(item)
                Next

                ' Apply the lot
                Me.m_lvShapes.View = View.LargeIcon
                Me.m_lvShapes.LargeImageList = largeImageList

            End If

            m_lvShapes.ResumeLayout()
            Me.m_bInUpdate = False

            ' Update selection
            Me.Selection = Me.m_selectionDelayed
            Me.m_selectionDelayed = Nothing

        End Sub

#End Region ' Helper methods

#Region " Event handlers "

        'Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

        '    MyBase.OnLoad(e)

        '    If (Me.m_uic Is Nothing) Then Return

        '    Me.m_bInUpdate = True
        '    Me.Selection = Nothing
        '    Me.UpdateThumbnails(Nothing)
        '    Me.m_bInUpdate = False

        'End Sub

        ''' <summary>
        ''' Modify shape data.
        ''' </summary>
        Private Sub lvShapes_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) _
                    Handles m_lvShapes.ItemActivate
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.ChangeShape, Me.Selection)
        End Sub

        Private Sub lvShapes_ItemChecked(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckedEventArgs) _
            Handles m_lvShapes.ItemChecked

            Dim ts As cTimeSeries = Nothing

            ' Sanity check
            If (e.Item Is Nothing) Then Return
            ' Fixed #1095: itemchecked event was fired spontaneously when docking / undocking panels
            If (Me.m_bInUpdate Or Not Me.m_lvShapes.Focused) Then Return

            Dim shape As cShapeData = DirectCast(e.Item.Tag, cShapeData)

            If (TypeOf shape Is cTimeSeries) Then

                ts = DirectCast(shape, cTimeSeries)
                If (ts.Enabled <> e.Item.Checked) Then

                    If ts.ValidationStatus <> eStatusFlags.OK Then
                        e.Item.Checked = False
                        Me.m_uic.Core.Messages.SendMessage(New cMessage(String.Format(My.Resources.PROMPT_TIMESERIES_NOTUSABLE, ts.Name, ts.ValidationMessage), _
                                                                        eMessageType.DataValidation, EwEUtils.Core.eCoreComponentType.TimeSeries, eMessageImportance.Warning))
                        Return
                    End If

                    ' Update enabled state
                    ts.Enabled = e.Item.Checked
                    ' HACK!!!
                    If (m_bInUpdate = False) Then
                        Me.m_uic.Core.UpdateTimeSeries()
                    End If
                End If

            End If

        End Sub

        ''' <summary>
        ''' The event handler when the selected thumbnail changes in the listview.
        ''' </summary>
        Private Sub lvShapes_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_lvShapes.SelectedIndexChanged

            If Me.m_bInUpdate Then Return
            ' Haha
            Me.Selection = Me.Selection

        End Sub

        ''' <summary>
        ''' Duplicate a shape data.
        ''' </summary>
        Private Sub DuplicateShape_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles DuplicateToolStripMenuItem.Click
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Duplicate, Me.Selection)
        End Sub

        ''' <summary>
        ''' Remove a shape data.
        ''' </summary>
        Private Sub RemoveShape_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles RemoveToolStripMenuItem.Click
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Remove, Me.Selection)
        End Sub

        ''' <summary>
        ''' Add a shape data.
        ''' </summary>
        Private Sub AddShape_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles AddToolStripMenuItem.Click
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Add)
        End Sub

        Private Sub ChangeShape_Click(sender As Object, e As EventArgs) Handles ChangeShapeToolStripMenuItem.Click
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.ChangeShape)
        End Sub

        Private Sub ValuesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ValuesToolStripMenuItem.Click
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Modify)
        End Sub

        Private Sub ResetToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
            Handles ResetToolStripMenuItem.Click
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Reset)
        End Sub

        ''' <summary>
        ''' Import a time series dataset.
        ''' </summary>
        Private Sub tsBtnImport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles ImportToolStripMenuItem.Click
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Import)
        End Sub

        ''' <summary>
        ''' Export a time series dataset.
        ''' </summary>
        Private Sub tsBtnExport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles ExportToolStripMenuItem.Click
            Me.m_handler.ExecuteCommand(cShapeGUIHandler.eShapeCommandTypes.Export)
        End Sub

        ''' <summary>
        ''' Styleguide change event.
        ''' </summary>
        Private Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
            If (ct And cStyleGuide.eChangeType.Thumbnails) > 0 Then
                Me.UpdateThumbnails(Me.Selection)
            End If
        End Sub

        Private Sub m_lvContextMenuStrip_Opening(sender As Object, e As CancelEventArgs) Handles m_lvContextMenuStrip.Opening
            Me.UpdateControls()
        End Sub

#End Region ' Event handlers

    End Class

End Namespace

