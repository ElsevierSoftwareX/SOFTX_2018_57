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
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Ecopath.Tools

    ''' <summary>
    ''' Form implementing the pedigree assignment interface.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class frmPedigree

#Region " Helper classes "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Item for showing a pedigree level in the pedigree level listbox.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cPedigreeLevelListboxItem

            Private m_level As cPedigreeLevel = Nothing

            Public Sub New(ByVal level As cPedigreeLevel)
                Me.m_level = level
            End Sub

            Public ReadOnly Property Level() As cPedigreeLevel
                Get
                    Return Me.m_level
                End Get
            End Property

            Public Overrides Function ToString() As String
                If (Me.m_level Is Nothing) Then
                    Return SharedResources.GENERIC_VALUE_NONE
                End If
                Return Me.m_level.Name
            End Function

        End Class

#End Region ' Helper classes

#Region " Private vars "

        ''' <summary>Varname currently 'selected' in the grid.</summary>
        Private m_varname As eVarNameFlags = eVarNameFlags.NotSet
        Private m_psg As cPedigreeStyleGuide = Nothing

#End Region ' Private vars

        Public Sub New()
            Me.InitializeComponent()
            Me.Grid = Me.m_grid
        End Sub

#Region " Form overloads "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            Dim varName As eVarNameFlags = eVarNameFlags.NotSet
            Dim bLevelsMissing As Boolean = False
            Dim desc As New cVarnameTypeFormatter()

            If (Me.UIContext Is Nothing) Then Return

            Me.m_psg = New cPedigreeStyleGuide(Me.UIContext)
            Me.m_grid.PedigreeStyleGuide = Me.m_psg

            For iVariable As Integer = 1 To Me.Core.nPedigreeVariables
                Dim var As eVarNameFlags = Me.Core.PedigreeVariable(iVariable)
                Me.m_cmbCategory.Items.Add(desc.GetDescriptor(var, eDescriptorTypes.Description))
            Next

            AddHandler Me.m_psg.OnRenderStyleChanged, AddressOf OnRenderStyleChanged
            AddHandler Me.m_grid.OnVariableChanged, AddressOf OnGridVariableChanged
            AddHandler Me.m_grid.OnSelectionChanged, AddressOf OnGridSelectionChanged

            Dim cmd As cEditPedigreeCommand = DirectCast(Me.UIContext.CommandHandler.GetCommand(cEditPedigreeCommand.cCOMMAND_NAME), cEditPedigreeCommand)
            If (cmd IsNot Nothing) Then cmd.AddControl(Me.m_tsbnEditPedigree)

            Me.SelectedVariable = Me.Core.PedigreeVariable(1)

            Me.UpdateControls()

            For iVariable As Integer = 1 To Me.Core.nPedigreeVariables

                ' Get manager
                varName = Me.Core.PedigreeVariable(iVariable)
                If (Me.Core.GetPedigreeManager(varName).NumLevels = 0) Then
                    bLevelsMissing = True
                    Exit For
                End If
            Next

            If bLevelsMissing And (cmd IsNot Nothing) Then
                Dim fmsg As New cFeedbackMessage(My.Resources.PROMPT_DEFINE_PEDIGREE, _
                                                 eCoreComponentType.EcoPath, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
                fmsg.Reply = eMessageReply.YES
                fmsg.Suppressable = True
                Core.Messages.SendMessage(fmsg)

                If fmsg.Reply = eMessageReply.YES Then
                    Try
                        cmd.Invoke(varName)
                    Catch ex As Exception
                        ' Hmm
                    End Try
                End If
            End If

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            RemoveHandler Me.m_psg.OnRenderStyleChanged, AddressOf OnRenderStyleChanged
            RemoveHandler Me.m_grid.OnVariableChanged, AddressOf OnGridVariableChanged
            RemoveHandler Me.m_grid.OnSelectionChanged, AddressOf OnGridSelectionChanged

            Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("EditPedigree")
            If (cmd IsNot Nothing) Then cmd.RemoveControl(Me.m_tsbnEditPedigree)

            ' Clean up
            Me.SelectedVariable = eVarNameFlags.NotSet

            ' Done
            MyBase.OnFormClosed(e)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to respond to changes in the number of, or details of,
        ''' pedigree levels.
        ''' </summary>
        ''' <param name="msg">The <see cref="EwECore.cMessage">core message</see> 
        ''' to respond to.</param>
        ''' -----------------------------------------------------------------------
        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            MyBase.OnCoreMessage(msg)

            If (msg.Source = eCoreComponentType.EcoPath) Then
                ' #Levels added?
                If (msg.Type = eMessageType.DataAddedOrRemoved) Then
                    ' #Yes: repopulate controls reflecting pedigree levels, just in case
                    Me.UpdatePedigreeLevelsListbox()
                Else
                    ' #No: just blink, this will reflect any changes
                    Me.Invalidate(True)
                End If
            End If
        End Sub

#End Region ' Form overloads

#Region " Events "

        Private Sub OnViewAsChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbViewAs.SelectedIndexChanged

            Dim iIndex As Integer = Me.m_cmbViewAs.SelectedIndex
            If (iIndex < 0) Then Return

            Me.SelectedRenderStyle = DirectCast(Me.m_cmbViewAs.SelectedIndex + 1, cPedigreeStyleGuide.eRenderStyleTypes)

        End Sub

        Private Sub OnCategoryChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_cmbCategory.SelectedIndexChanged
            Dim iIndex As Integer = Me.m_cmbCategory.SelectedIndex
            Dim var As eVarNameFlags = Me.Core.PedigreeVariable(iIndex + 1)
            Me.SelectedVariable = var
        End Sub

        Private Sub OnRenderStyleChanged(ByVal viz As cPedigreeStyleGuide)

            Me.m_lbLevels.Invalidate()
            Me.m_grid.Invalidate()

            Me.UpdateControls()

        End Sub

        Private Sub OnDrawPedigreeListboxItem(ByVal sender As Object, ByVal e As DrawItemEventArgs) _
            Handles m_lbLevels.DrawItem

            ' Sanity checks
            If (Me.UIContext Is Nothing) Then Return
            If (e.Index < 0) Then Return

            Dim item As cPedigreeLevelListboxItem = DirectCast(Me.m_lbLevels.Items(e.Index), cPedigreeLevelListboxItem)

            ' Render default background 
            e.DrawBackground()

            ' Render default text, bumped to the right by 22 pixels
            Using br As New SolidBrush(e.ForeColor)
                e.Graphics.DrawString(item.ToString(), e.Font, br, e.Bounds.X + 22, e.Bounds.Y)
            End Using

            ' Has level?
            If (item.Level IsNot Nothing) Then
                ' #Yes: Render colour box
                Using br As New SolidBrush(Me.m_psg.BackgroundColor(Me.BackColor, item.Level, cPedigreeStyleGuide.eRenderStyleTypes.Colors))
                    e.Graphics.FillRectangle(br, New Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 18, e.Bounds.Height - 4))
                End Using
            End If

            ' Render default focus rectangle
            e.DrawFocusRectangle()

        End Sub

        Private Sub OnLevelClick(ByVal sender As Object, ByVal e As MouseEventArgs) _
            Handles m_lbLevels.MouseClick

            Me.m_grid.SetLevel(Me.m_lbLevels.SelectedIndex)

        End Sub

        Protected Sub OnGridVariableChanged(ByVal sender As Object, ByVal vn As eVarNameFlags)
            Me.SelectedVariable = vn
        End Sub

        Protected Overrides Sub OnStyleGuideChanged(ByVal ct As cStyleGuide.eChangeType)
            If (ct And cStyleGuide.eChangeType.Colours) > 0 Then
                Me.Invalidate()
            End If
        End Sub

        Protected Sub OnGridSelectionChanged()

            Dim iSelectedCV As Integer = Me.m_grid.SelectedCV
            Dim iSelectedLevel As Integer = Me.m_grid.SelectedLevel
            Dim iSelection As Integer = -1

            If iSelectedCV = 0 Then
                iSelection = iSelectedLevel
            End If
            Me.m_lbLevels.SelectedIndex = iSelection

        End Sub

#End Region ' Events

#Region " Internal implementation "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the selected <see cref="eVarNameFlags">variable</see> in the grid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Property SelectedVariable() As eVarNameFlags
            Get
                Return Me.m_varname
            End Get
            Set(ByVal value As eVarNameFlags)

                ' Sanity checks and optimizations
                If (Me.UIContext Is Nothing) Then Return
                If (value = Me.m_varname) Then Return

                ' Clean up
                If (Me.m_varname <> eVarNameFlags.NotSet) Then
                    ' Me.DestroyPedigreeControls()
                End If

                ' Remember
                Me.m_varname = value
                Me.UpdatePedigreeLevelsListbox()

                ' Update
                If (Me.m_varname <> eVarNameFlags.NotSet) Then
                    Debug.Assert(Me.Core.IsPedigreeVariableSupported(value), "Pedigree not supported for variable " & Me.m_varname.ToString)
                    Me.m_cmbCategory.SelectedIndex = Me.Core.PedigreeVariableIndex(Me.m_varname) - 1
                    Me.m_grid.SelectedVariable = Me.m_varname
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the selected <see cref="cPedigreeLevel">pedigree level</see> in
        ''' the listbox with available levels.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Property SelectedLevel() As cPedigreeLevel
            Get
                If (Me.m_lbLevels.SelectedItem Is Nothing) Then Return Nothing
                Return DirectCast(Me.m_lbLevels.SelectedItem, cPedigreeLevelListboxItem).Level
            End Get
            Set(ByVal value As cPedigreeLevel)
                For i As Integer = 0 To Me.m_lbLevels.Items.Count - 1
                    Dim item As cPedigreeLevelListboxItem = DirectCast(Me.m_lbLevels.Items(i), cPedigreeLevelListboxItem)
                    If ReferenceEquals(item.Level, value) Then
                        Me.m_lbLevels.SelectedIndex = i
                        Return
                    End If
                Next
                Me.m_lbLevels.SelectedIndex = -1
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the selected <see cref="cPedigreeStyleGuide.eRenderStyleTypes">render style</see> in
        ''' the entire interface.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Property SelectedRenderStyle() As cPedigreeStyleGuide.eRenderStyleTypes
            Get
                Return Me.m_psg.RenderStyle
            End Get
            Set(ByVal value As cPedigreeStyleGuide.eRenderStyleTypes)
                If (value = cPedigreeStyleGuide.eRenderStyleTypes.NotSet) Then Return
                Me.m_psg.RenderStyle = value
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Popluate controls reflecting available pedigree levels.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub UpdatePedigreeLevelsListbox()

            Dim man As cPedigreeManager = Me.Core.GetPedigreeManager(Me.SelectedVariable)
            Dim lvl As cPedigreeLevel = Nothing

            Me.m_lbLevels.Items.Clear()

            If (man Is Nothing) Then Return

            ' Add 'None' item
            Me.m_lbLevels.Items.Add(New cPedigreeLevelListboxItem(Nothing))
            For iLevel As Integer = 1 To man.NumLevels
                lvl = man.Level(iLevel)
                Me.m_lbLevels.Items.Add(New cPedigreeLevelListboxItem(lvl))
            Next iLevel

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Update the UI.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub UpdateControls()

            If (Me.SelectedRenderStyle <> cPedigreeStyleGuide.eRenderStyleTypes.NotSet) Then
                Me.m_cmbViewAs.SelectedIndex = CInt(Me.SelectedRenderStyle) - 1
            End If

        End Sub

#End Region ' Internal implementation

    End Class

End Namespace
