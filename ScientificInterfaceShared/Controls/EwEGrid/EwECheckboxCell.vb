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

Imports ScientificInterfaceShared.Style
Imports SourceGrid2
Imports EwECore
Imports ScientificInterfaceShared.Properties

Namespace Controls.EwEGrid

    <CLSCompliant(False)> _
    Public Class EwECheckboxCell
        Inherits SourceGrid2.Cells.Real.CheckBox
        Implements IEwECell
        Implements IDisposable

        ''' <summary>Default visualizer for EwECells</summary>
        Private Shared g_visualizer As New cEwECheckBoxVisualizer()
        ''' <summary>Behaviour model to catch [ENTER] key presses.</summary>
        Private m_bmCatchEnter As BehaviorModels.IBehaviorModel = Nothing
        Private m_uic As cUIContext = Nothing

        Public Sub New(ByVal bChecked As Boolean, _
                       Optional ByVal style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK)

            MyBase.New(bChecked)
            ' Set shared visualizer
            Me.VisualModel = g_visualizer
            ' Set style
            Me.Style = style
        End Sub

        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            If UIContext IsNot Nothing Then

                Me.UIContext = Nothing

                ' Remove all bahaviour models
                Me.Behaviors.Remove(Me.m_bmCatchEnter)
                Me.m_bmCatchEnter = Nothing

                ' Release any editors
                Me.DataModel.EnableEdit = False
                Me.DataModel.EditableMode = SourceGrid2.EditableMode.None
                Me.DataModel = Nothing

            End If
            GC.SuppressFinalize(Me)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Custom cell style
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private m_style As cStyleGuide.eStyleFlags = 0

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Allows to set a custom <see cref="cStyleGuide.eStyleFlags">style</see>,
        ''' triggering EwE colour feedback and EwE cell edit behaviour.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property Style() As cStyleGuide.eStyleFlags _
            Implements IEwECell.Style

            Get
                Return Me.m_style
            End Get

            Set(ByVal s As cStyleGuide.eStyleFlags)
                Me.m_style = s
                If ((s And cStyleGuide.eStyleFlags.NotEditable) = 0) Then
                    Me.DataModel.EnableEdit = True
                    Me.DataModel.EditableMode = SourceGrid2.EditableMode.Default
                Else
                    Me.DataModel.EnableEdit = False
                    Me.DataModel.EditableMode = SourceGrid2.EditableMode.None
                End If
            End Set

        End Property

#Region " Updates (StyleGuide)"

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' StyleGuide change event handler; makes sure cells are redrawn
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overridable Sub OnStyleGuideChanged(ByVal changeType As cStyleGuide.eChangeType)
            Me.Invalidate()
        End Sub

#End Region ' Updated (StyleGuide)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Connect a cell to a grid. Overridden to hook up to an existing 
        ''' <see cref="cUIContext">UI context</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnAddToGrid(ByVal e As System.EventArgs)
            MyBase.OnAddToGrid(e)
            If (TypeOf Me.Grid Is IUIElement) Then
                ' Grab UI context from parent grid
                Me.UIContext = DirectCast(Me.Grid, IUIElement).UIContext
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Remove a cell from a grid. Overridden to disconnect from the current
        ''' <see cref="cUIContext">UI context</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overrides Sub OnRemoveToGrid(ByVal e As System.EventArgs)
            MyBase.OnRemoveToGrid(e)
            Me.UIContext = Nothing
        End Sub

        Protected ReadOnly Property Core() As cCore _
            Implements IEwECell.Core
            Get
                If (Me.UIContext Is Nothing) Then Return Nothing
                Return Me.UIContext.Core
            End Get
        End Property

        Protected ReadOnly Property PropertyManager() As cPropertyManager _
            Implements IEwECell.PropertyManager
            Get
                If (Me.UIContext Is Nothing) Then Return Nothing
                Return Me.UIContext.PropertyManager
            End Get
        End Property

        Protected ReadOnly Property StyleGuide() As cStyleGuide _
            Implements IEwECell.StyleGuide
            Get
                If (Me.UIContext Is Nothing) Then Return Nothing
                Return Me.UIContext.StyleGuide
            End Get
        End Property

        Public Overridable Property UIContext() As cUIContext _
            Implements IEwECell.UIContext
            Get
                Return Me.m_uic
            End Get
            Protected Set(ByVal value As cUIContext)

                If (Me.m_uic IsNot Nothing) Then
                    ' Clean up
                    ' Release style guide event handler
                    RemoveHandler Me.StyleGuide.StyleGuideChanged, AddressOf Me.OnStyleGuideChanged
                End If

                Me.m_uic = value

                If (Me.m_uic IsNot Nothing) Then
                    ' Release style guide event handler
                    AddHandler Me.StyleGuide.StyleGuideChanged, AddressOf Me.OnStyleGuideChanged
                End If

            End Set

        End Property

    End Class

End Namespace
