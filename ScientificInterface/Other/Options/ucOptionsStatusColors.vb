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

Imports EwEUtils.SystemUtilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' User control; implements the interface to change value status feedback colors.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class ucOptionsStatusColors
        Implements IOptionsPage
        Implements IUIElement

#Region " Helper classes "

        Private Class cColorItem

            Private m_strName As String = ""
            Private m_strDescription As String = ""
            Private m_ctFore As cStyleGuide.eApplicationColorType
            Private m_clrFore As Color = Nothing
            Private m_ctBack As cStyleGuide.eApplicationColorType
            Private m_clrBack As Color = Nothing

            Public Sub New(ByVal strText As String, ByVal ctFore As cStyleGuide.eApplicationColorType, ByVal ctBack As cStyleGuide.eApplicationColorType, ByVal sg As cStyleGuide)

                Dim astrBits As String() = strText.Split("|"c)
                Me.m_strName = astrBits(0)
                If astrBits.Length = 2 Then
                    Me.m_strDescription = astrBits(1)
                Else
                    Me.m_strDescription = ""
                End If
                Me.m_ctFore = ctFore
                Me.m_ctBack = ctBack

                Me.ForeColor = sg.ApplicationColor(Me.m_ctFore)
                Me.BackColor = sg.ApplicationColor(Me.m_ctBack)
            End Sub

            Public ReadOnly Property Description() As String
                Get
                    Return Me.m_strDescription
                End Get
            End Property

            Public ReadOnly Property Name() As String
                Get
                    Return Me.m_strName
                End Get
            End Property

            Public ReadOnly Property ForeColorType() As cStyleGuide.eApplicationColorType
                Get
                    Return Me.m_ctFore
                End Get
            End Property

            Public ReadOnly Property BackColorType() As cStyleGuide.eApplicationColorType
                Get
                    Return Me.m_ctBack
                End Get
            End Property

            Public Property ForeColor() As Color
                Get
                    Return Me.m_clrFore
                End Get
                Set(ByVal value As Color)
                    If Me.m_ctFore <> cStyleGuide.eApplicationColorType.NotSet Then Me.m_clrFore = value
                End Set
            End Property

            Public Property BackColor() As Color
                Get
                    Return Me.m_clrBack
                End Get
                Set(ByVal value As Color)
                    If Me.m_ctBack <> cStyleGuide.eApplicationColorType.NotSet Then Me.m_clrBack = value
                End Set
            End Property

            Public Overrides Function ToString() As String
                Return Me.m_strName
            End Function

        End Class

        Private Class cKnownColorItem

            Private m_strName As String = ""
            Private m_clr As Color

            Public Sub New(ByVal strName As String, ByVal clr As Color)
                Me.m_strName = strName
                Me.m_clr = clr
            End Sub

            Public ReadOnly Property Name() As String
                Get
                    Return Me.m_strName
                End Get
            End Property

            Public Property Color() As Color
                Get
                    Return Me.m_clr
                End Get
                Set(ByVal value As Color)
                    Me.m_clr = value
                End Set
            End Property

            Public Overrides Function ToString() As String
                Return Me.m_strName
            End Function

        End Class

#End Region ' Helper classes

#Region " Variables "

        ''' <summary>List of known colours.</summary>
        Private m_lciKnownColors As New List(Of cKnownColorItem)

#End Region ' Variables

#Region " Constructors "

        Public Sub New(ByVal uic As cUIContext)

            Me.UIContext = uic
            Me.InitializeComponent()
            Me.InitKnownColors()

        End Sub

#End Region ' Constructors

#Region " Helper methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' The helper methods define the common used colors to choosing color from for easy access 
        ''' </summary>
        ''' <remarks>Define all known colours</remarks>
        ''' -------------------------------------------------------------------
        Private Sub InitKnownColors()

            Dim astrNames() As String = [Enum].GetNames(GetType(KnownColor))
            Dim kcColor As KnownColor = Nothing

            m_lciKnownColors.Clear()

            ' Iterate through each known color name
            For Each strName As String In astrNames
                ' Cast the color name into a KnownColor
                kcColor = DirectCast([Enum].Parse(GetType(KnownColor), strName), KnownColor)
                ' Check if this is a System color (system color names have no ARGB values)
                If (kcColor > KnownColor.Transparent) Then
                    ' Add it to the internal list of colours
                    m_lciKnownColors.Add(New cKnownColorItem(strName, Color.FromName(strName)))
                End If
            Next strName

            FillColourComboBox(Me.m_cmbItemForeground, m_lciKnownColors)
            FillColourComboBox(Me.m_cmbItemBackground, m_lciKnownColors)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to find the known color name for a given color.
        ''' </summary>
        ''' <param name="clr">the color to find.</param>
        ''' <returns>The name of a known color, or and empty string if no match was not found.</returns>
        ''' -------------------------------------------------------------------
        Private Function GetColorName(ByVal clr As Color) As String

            Dim ciTest As cKnownColorItem = Nothing
            For iKnown As Integer = 0 To Me.m_lciKnownColors.Count - 1
                ciTest = Me.m_lciKnownColors(iKnown)
                If clr.R = ciTest.Color.R And clr.G = ciTest.Color.G And clr.B = ciTest.Color.B Then
                    Return ciTest.Name
                End If
            Next
            Return ""

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to load color items into the listbox. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub FillColorItemsList()

            Me.m_lvItems.Items.Clear()

            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_DEFAULT, cStyleGuide.eApplicationColorType.DEFAULT_TEXT, cStyleGuide.eApplicationColorType.DEFAULT_BACKGROUND)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_NAMES, cStyleGuide.eApplicationColorType.NAMES_TEXT, cStyleGuide.eApplicationColorType.NAMES_BACKGROUND)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_MODEL_FAILEDRESULT, cStyleGuide.eApplicationColorType.INVALIDMODELRESULT_TEXT)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_MODEL_FAILEDVALIDATION, cStyleGuide.eApplicationColorType.FAILEDVALIDATION_TEXT)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_ERROR, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.GENERICERROR_BACKGROUND)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_COMPUTED, cStyleGuide.eApplicationColorType.COMPUTED_TEXT)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_REMARKS, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.REMARKS_BACKGROUND)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_PEDIGREE, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.PEDIGREE)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_SUM, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.SUM_BACKGROUND)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_READONLY, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.READONLY_BACKGROUND)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_MODEL_MISSINGPARAM, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.MISSINGPARAMETER_BACKGROUND)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_CHECKED, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.CHECKED_BACKGROUND)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_HIGHLIGHT, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.HIGHLIGHT)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_PREDATOR, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.PREDATOR)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_PROMPT_PREY, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.PREY)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_ECOSIM_PLOTS_BACKGROUND_COLOR, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.PLOT_BACKGROUND)
            Me.AddColorTypeItem(My.Resources.OPTIONS_COLORDLG_ECOSPACE_MAPLOT_BACKGROUND_COLOR, cStyleGuide.eApplicationColorType.NotSet, cStyleGuide.eApplicationColorType.MAP_BACKGROUND)

            ' Kick off
            Me.m_lvItems.Items(0).Selected = True
            Me.m_lvItems.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent)

        End Sub

        Private Sub AddColorTypeItem(ByVal strName As String, ByVal ctFore As cStyleGuide.eApplicationColorType, _
                Optional ByVal ctBack As cStyleGuide.eApplicationColorType = cStyleGuide.eApplicationColorType.NotSet)
            Dim ci As New cColorItem(strName, ctFore, ctBack, Me.UIContext.StyleGuide)
            Dim lvi As New ListViewItem(ci.Name)
            lvi.SubItems.Add(ci.Description)
            lvi.Tag = ci
            Me.m_lvItems.Items.Add(lvi)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to add an array of items into combobox control. 
        ''' </summary>
        ''' <param name="cb">The combobox reference</param>
        ''' <param name="lColors">The list of <see cref="cKnownColorItem">color items</see> 
        ''' to be added into the combobox</param>
        ''' -------------------------------------------------------------------
        Private Sub FillColourComboBox(ByVal cb As ComboBox, ByVal lColors As List(Of cKnownColorItem))

            cb.Items.Clear()

            ' Add intial 'custom' item
            cb.Items.Add(New cKnownColorItem(SharedResources.GENERIC_VALUE_CUSTOM, Color.Black))

            ' Add all known colours
            For i As Integer = 0 To lColors.Count - 1
                cb.Items.Add(lColors(i))
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to select an item in the combo box by name. If the item
        ''' to update was not found, the custom colour item is selected and updated.
        ''' </summary>
        ''' <param name="cb">The comboxbox to update.</param>
        ''' <param name="clr">The color to update.</param>
        ''' -------------------------------------------------------------------
        Private Sub UpdateColorComboboxItem(ByVal cb As ComboBox, ByVal clr As Color)

            Dim ciTest As cKnownColorItem = Nothing

            For i As Integer = 1 To cb.Items.Count - 1
                ciTest = DirectCast(cb.Items(i), cKnownColorItem)
                If (ciTest.Color = clr) Then
                    cb.SelectedIndex = i
                    Return
                End If
            Next

            ' Update item 0: the Custom item
            ciTest = DirectCast(cb.Items(0), cKnownColorItem)
            ciTest.Color = clr
            cb.SelectedIndex = 0

        End Sub

        Private Function SelectedColor() As cColorItem
            If (Me.m_lvItems.SelectedItems.Count <> 1) Then Return Nothing
            Dim lvi As ListViewItem = Me.m_lvItems.SelectedItems(0)
            Return DirectCast(lvi.Tag, cColorItem)
        End Function

        Private m_bInUpdate As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to enable and update UI controls.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateControls()

            Dim item As cColorItem = Me.SelectedColor()
            Dim bShowForeground As Boolean = False
            Dim bShowBackground As Boolean = False
            Dim strName As String = ""
            Dim strDescription As String = ""
            Dim sg As cStyleGuide = Me.UIContext.StyleGuide

            Me.m_bInUpdate = True

            If (item IsNot Nothing) Then
                bShowForeground = (item.ForeColorType <> cStyleGuide.eApplicationColorType.NotSet)
                bShowBackground = (item.BackColorType <> cStyleGuide.eApplicationColorType.NotSet)
                strName = item.Name
                strDescription = item.Description
            End If

            'Update the selection in combobox
            If (bShowForeground) Then
                Me.UpdateColorComboboxItem(Me.m_cmbItemForeground, item.ForeColor) ' sg.ApplicationColor(item.ForeColorType))
            End If

            If (bShowBackground) Then
                Me.UpdateColorComboboxItem(Me.m_cmbItemBackground, item.BackColor) ' sg.ApplicationColor(item.BackColorType))
            End If

            ' Enable/disable foreground color related controls
            Me.m_lblItemForeColor.Enabled = bShowForeground
            Me.m_cmbItemForeground.Enabled = bShowForeground
            Me.m_btnCustomForeColor.Enabled = bShowForeground

            ' Avoid confusion by blanking out the fore color combo if no fore color should be shown
            If Not bShowForeground Then Me.m_cmbItemForeground.SelectedIndex = -1

            ' Enable/disable background color related controls
            Me.m_lblItemBackColor.Enabled = bShowBackground
            Me.m_cmbItemBackground.Enabled = bShowBackground
            Me.m_btnCustomBackColor.Enabled = bShowBackground

            ' Avoid confusion by blanking out the back color combo if no back color should be shown
            If Not bShowBackground Then Me.m_cmbItemBackground.SelectedIndex = -1

            Me.m_bInUpdate = False

            ' Invalidate preview
            Me.m_plPreview.Invalidate()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to update the color in an item. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub UpdateForeColor(ByVal ci As cColorItem, ByVal clr As Color)

            ' Sanity check
            If ci Is Nothing Then Return

            ' Update the color in the data structure
            ci.ForeColor = clr
            ' Invalidate preview
            Me.m_plPreview.Invalidate()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to update the color in an item. 
        ''' </summary>
        ''' <param name="clr">The item reference whose color gets updated.</param>
        ''' -------------------------------------------------------------------
        Private Sub UpdateBackColor(ByVal ci As cColorItem, ByVal clr As Color)

            ' Sanity check
            If (ci Is Nothing) Then Return

            ' Update the color in the data structure
            ci.BackColor = clr
            ' Invalidate preview
            Me.m_plPreview.Invalidate()

        End Sub


        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper methods to draw a custom listcontrol item 
        ''' </summary>
        ''' <param name="e">DrawItemEventArgs sent by DrawItem event handler</param>
        ''' <param name="clr">The colorbox's color</param>
        ''' <param name="txt">The text beside the colorbox</param>
        ''' <remarks>This method is called by both Listbox and Combobox drawItem event handlers</remarks>
        ''' -------------------------------------------------------------------
        Private Sub DrawCustomItem(ByVal e As System.Windows.Forms.DrawItemEventArgs, _
                                   ByVal clr As Color, _
                                   ByVal txt As String, _
                                   ByVal rect As Rectangle)


            ' Do nothing if there is no data
            If e.Index = -1 Then Return

            'If the item is selected, draw the correct background color
            e.DrawBackground()
            e.DrawFocusRectangle()

            'Get the listbox's graphics object
            Dim g As Graphics = e.Graphics

            'Draw color box
            g.FillRectangle(New SolidBrush(clr), rect)
            g.DrawRectangle(Pens.Black, rect)
            'Draw text 
            g.DrawString(txt, e.Font, New SolidBrush(e.ForeColor), _
                            New RectangleF(e.Bounds.X + rect.Width + 4, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height))


        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper methods to draw a custom listcontrol item 
        ''' </summary>
        ''' <param name="e">DrawItemEventArgs sent by DrawItem event handler</param>
        ''' <param name="txt">The text beside the colorbox</param>
        ''' <remarks>This method is called by both Listbox drawItem event handlers</remarks>
        ''' -------------------------------------------------------------------
        Private Sub DrawCustomText(ByVal e As System.Windows.Forms.DrawItemEventArgs, _
                                   ByVal txt As String, _
                                   ByVal rect As Rectangle)
            ' Do nothing if there is no data
            If e.Index = -1 Then Return

            'If the item is selected, draw the correct background color
            e.DrawBackground()
            e.DrawFocusRectangle()

            'Get the listbox's graphics object
            Dim g As Graphics = e.Graphics
            'Draw text 
            g.DrawString(txt, e.Font, New SolidBrush(e.ForeColor), rect)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to show a colorDialog and lets user to select a color.  
        ''' </summary>
        ''' <returns>The chosen color, nothing if no color was chosen.</returns>
        ''' -------------------------------------------------------------------
        Private Function SelectColorByDialog(ByVal clr As Color) As Color

            Dim dlg As New ColorDialog
            Dim iCustomColor As Integer = 0

            ' Pass in the current color
            dlg.Color = clr
            dlg.AllowFullOpen = True
            dlg.AnyColor = True
            ' Work-around for known Color-to-Colorref conversion bug in .NET ColorDialog (ARGB vs XBGR)
            ' http://groups.google.com/group/microsoft.public.dotnet.framework.windowsforms/browse_frm/thread/58cbe7edf7402584
            iCustomColor = clr.R + (clr.G * 256) + (clr.B * 65536)
            dlg.CustomColors() = New Integer() {iCustomColor}

            If dlg.ShowDialog() = DialogResult.OK Then clr = dlg.Color

            Return clr

        End Function

#End Region ' Helper methods

#Region " Event handlers "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Control's load event which gets called every time the control gets loaded. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            Me.FillColorItemsList()

        End Sub

        Private Sub OnColorSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_lvItems.SelectedIndexChanged
            Me.UpdateControls()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Combobox drawItem method getting called when the drawMode is either OwnerDrawFixed or OwnerDrawVariable
        ''' </summary>
        ''' <remarks>To customize drawing so we can draw colorbox next to text</remarks>
        ''' -------------------------------------------------------------------
        Private Sub cbItemForeground_DrawItem(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DrawItemEventArgs) _
            Handles m_cmbItemForeground.DrawItem, m_cmbItemBackground.DrawItem

            Dim cmb As ComboBox = DirectCast(sender, ComboBox)
            If cmb Is Nothing Then Return

            If (e.Index = -1) Then Return

            Try
                'Get the current drawn item
                Dim item As cKnownColorItem = DirectCast(cmb.Items(e.Index), cKnownColorItem)
                'The rectangle to draw the color box
                Dim rect As Rectangle = New Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, e.Bounds.Height, e.Bounds.Height - 4)

                Me.DrawCustomItem(e, item.Color, item.Name, rect)
            Catch ex As Exception
                Return
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' button's click event handler. It provides the functionality for use to select a color from 
        ''' system-defined color dialog. 
        ''' </summary>
        ''' <remarks>For foreground color</remarks>
        ''' -------------------------------------------------------------------
        Private Sub btnCustomForeColor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnCustomForeColor.Click

            Dim ci As cColorItem = Me.SelectedColor()
            Dim clrSelected As Color = Nothing

            If (ci IsNot Nothing) Then
                clrSelected = SelectColorByDialog(ci.ForeColor)
                Me.UpdateColorComboboxItem(m_cmbItemForeground, clrSelected)
                Me.UpdateForeColor(ci, clrSelected)
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' button's click event handler. It provides the functionality for use to select a color from 
        ''' system-defined color dialog. 
        ''' </summary>
        ''' <remarks>For background color</remarks>
        ''' -------------------------------------------------------------------
        Private Sub btnCustomBackColor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnCustomBackColor.Click

            Dim ci As cColorItem = Me.SelectedColor()
            Dim clrSelected As Color = Nothing

            If (ci IsNot Nothing) Then
                clrSelected = Me.SelectColorByDialog(ci.BackColor)
                Me.UpdateColorComboboxItem(m_cmbItemBackground, clrSelected)
                Me.UpdateBackColor(ci, clrSelected)
            End If

        End Sub

        ' ''' -------------------------------------------------------------------
        ' ''' <summary>
        ' ''' Event handler to set the color preference to default colors. 
        ' ''' </summary>
        ' ''' -------------------------------------------------------------------
        'Private Sub btnUseDefault_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        '    Dim sel As ListView.SelectedIndexCollection = Me.m_lvItems.SelectedIndices
        '    Me.UIContext.StyleGuide.ResetApplicationColors()
        '    Me.FillColorItemsList()
        '    For Each i As Integer In sel
        '        Me.m_lvItems.Items(i).Selected = True
        '    Next

        'End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler to set the new color for an item. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub cbItemForeground_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbItemForeground.SelectedIndexChanged

            If (Me.m_bInUpdate) Then Return

            Dim ci As cColorItem = Me.SelectedColor()
            Dim selClr As cKnownColorItem = DirectCast(Me.m_cmbItemForeground.SelectedItem, cKnownColorItem)

            If ci Is Nothing Then Return

            If ci.ForeColorType <> cStyleGuide.eApplicationColorType.NotSet Then
                Me.UpdateForeColor(ci, selClr.Color)
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler to set the new color for an item. 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub cbItemBackground_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbItemBackground.SelectedIndexChanged

            If (Me.m_bInUpdate) Then Return

            Dim ci As cColorItem = Me.SelectedColor()
            Dim selClr As cKnownColorItem = DirectCast(Me.m_cmbItemBackground.SelectedItem, cKnownColorItem)

            If (ci Is Nothing) Then Return

            If ci.BackColorType <> cStyleGuide.eApplicationColorType.NotSet Then
                Me.UpdateBackColor(ci, selClr.Color)
            End If
        End Sub

        Private Sub OnPaintPreview(sender As Object, e As System.Windows.Forms.PaintEventArgs) _
            Handles m_plPreview.Paint

            Dim ci As cColorItem = Me.SelectedColor()
            Dim fmt As New StringFormat()

            fmt.Alignment = StringAlignment.Center
            fmt.LineAlignment = StringAlignment.Center
            fmt.FormatFlags = StringFormatFlags.NoWrap

            If (ci IsNot Nothing) Then

                Dim clrFore As Color = SystemColors.ControlText
                Dim clrBack As Color = SystemColors.Window
                Dim bIsRemark As Boolean = (ci.BackColorType = cStyleGuide.eApplicationColorType.REMARKS_BACKGROUND)

                If (ci.BackColorType <> cStyleGuide.eApplicationColorType.NotSet And Not bIsRemark) Then
                    clrBack = ci.BackColor
                End If
                If (ci.ForeColorType <> cStyleGuide.eApplicationColorType.NotSet And Not bIsRemark) Then
                    clrFore = ci.ForeColor
                End If

                Using br As New SolidBrush(clrBack)
                    e.Graphics.FillRectangle(br, e.ClipRectangle)
                End Using
                Using br As New SolidBrush(clrFore)
                    e.Graphics.DrawString(My.Resources.VALUE_PREVIEW, Me.Font, br, e.ClipRectangle, fmt)
                End Using

                If bIsRemark Then
                    cRemarksIndicator.Paint(ci.BackColor, Me.m_plPreview.ClientRectangle, e.Graphics, True, cSystemUtils.IsRightToLeft)
                End If
            End If

        End Sub

#End Region ' Event handlers

#Region " Public methods "

        Public Property UIContext As cUIContext _
            Implements IUIElement.UIContext

        Public Function CanApply() As Boolean _
            Implements IOptionsPage.CanApply
            Return True
        End Function

        Public Event OnOptionsColorsChanged(sender As IOptionsPage, args As System.EventArgs) _
            Implements IOptionsPage.OnChanged

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save colour selections back to the style guide.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function Apply() As IOptionsPage.eApplyResultType _
            Implements IOptionsPage.Apply

            Dim ci As cColorItem = Nothing
            Dim sg As cStyleGuide = Me.UIContext.StyleGuide

            ' Apply colors to the style guide
            sg.SuspendEvents()

            For Each lvi As ListViewItem In Me.m_lvItems.Items
                ci = DirectCast(lvi.Tag, cColorItem)
                If ci.ForeColorType <> cStyleGuide.eApplicationColorType.NotSet Then
                    sg.ApplicationColor(ci.ForeColorType) = ci.ForeColor
                End If
                If ci.BackColorType <> cStyleGuide.eApplicationColorType.NotSet Then
                    sg.ApplicationColor(ci.BackColorType) = ci.BackColor
                End If
            Next

            sg.ResumeEvents()
            Return IOptionsPage.eApplyResultType.Success

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Reset all colours
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub SetDefaults() _
            Implements IOptionsPage.SetDefaults

            Dim sel As ListView.SelectedIndexCollection = Me.m_lvItems.SelectedIndices
            Me.UIContext.StyleGuide.ResetApplicationColors()
            Me.FillColorItemsList()
            For Each i As Integer In sel
                Me.m_lvItems.Items(i).Selected = True
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IOptionsPage.CanSetDefaults"/>
        ''' -------------------------------------------------------------------
        Public Function CanSetDefaults() As Boolean _
            Implements IOptionsPage.CanSetDefaults
            Return True
        End Function

#End Region ' Public methods

    End Class

End Namespace


