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

Imports EwECore
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Controls.EwEGrid

#End Region

Namespace Forms

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' EwEForm that contains an <see cref="EwEGrid">EwEGrid</see>.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class frmEwEGrid
        Inherits frmEwE

#Region " Variables "

        ''' <summary>The grid in this form.</summary>
        Private m_grid As EwEGrid = Nothing
        ''' <summary><see cref="QuickEditHandler">Quick Edit Handler</see> for this form.</summary>
        Private m_qeHandler As cQuickEditHandler = Nothing

#End Region ' Variables

#Region " Constructors "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Parameterless constructor.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()
            MyBase.New()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        ''' <param name="grid">Grid to attach to this form.</param>
        ''' -----------------------------------------------------------------------
        <CLSCompliant(False)> _
        Public Sub New(ByVal grid As EwEGrid)

            MyBase.New()

            ' Store grid
            Me.Grid = grid

            ' Grid added via constructor - perform special make-up
            ' .. fill
            Me.Grid.Dock = DockStyle.Fill
            ' .. add to controls
            Me.Controls.Add(Me.Grid)

        End Sub

        Public Overrides Property UIContext() As cUIContext
            Get
                Return MyBase.UIContext
            End Get
            Set(ByVal value As cUIContext)
                MyBase.UIContext = value
                If (Me.Grid IsNot Nothing) Then
                    Me.Grid.UIContext = value
                End If
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get a reference to the Grid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <CLSCompliant(False)> _
        Public Property Grid() As EwEGrid
            Get
                Return m_grid
            End Get
            Set(ByVal grid As EwEGrid)

                If (Me.m_grid IsNot Nothing) Then
                    Me.m_grid.UIContext = Nothing
                End If

                Me.m_grid = grid

                If (Me.m_grid IsNot Nothing) Then
                    Me.m_grid.UIContext = Me.UIContext
                    Me.m_grid.IsOutputGrid = frmEwE.IsOutputForm(Me.CoreExecutionState) Or Me.IsRunForm
                End If

            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns a persistent name for instances of this class. Instances are 
        ''' identified by the class name of the attached grid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function GetPersistString() As String

            ' Has a grid?
            If Me.m_grid IsNot Nothing Then
                ' #Yes: return grid class name
                Return Me.m_grid.GetType().ToString()
            Else
                ' #No: return the default persistent string
                Return MyBase.GetPersistString()
            End If

        End Function

#End Region ' Constructors

#Region " Obligatory overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to pass the message to the grid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overrides Sub OnCoreMessage(ByVal msg As cMessage)
            Me.m_grid.OnCoreMessage(msg)
        End Sub

#End Region ' Obligatory overrides

#Region " Form overrides "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; handles the Load event to finalize this form for usage.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnLoad(ByVal e As EventArgs)

            MyBase.OnLoad(e)

            ' Sanity check
            If (Me.Grid Is Nothing Or Me.DesignMode = True) Then Return

            ' JS 05Sep09: QEbar was Input grid only. Now, CSV interaction is available for all grids
            Me.SetQuickEditHandler(True)
            Me.Grid.DataName = Me.Text

            Me.CoreComponents = Me.Grid.CoreComponents

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler; handles the Disposed event to clear this form after usage.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            ' Release any quick edit handler
            Me.SetQuickEditHandler(False)
            ' Clear any message source links
            Me.CoreComponents = Nothing
            ' Kill the grid
            Me.Grid = Nothing

            MyBase.OnFormClosed(e)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Return the content of the grid as an image.
        ''' </summary>
        ''' <param name="rcPrint">The print area. Ignored in this method.</param>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function GetPrintContent(ByVal rcPrint As Rectangle) As Image

            Dim rc As New Rectangle(0, 0, Grid.Width, Grid.Height)
            Dim bmp As New Bitmap(rc.Width, rc.Height, Imaging.PixelFormat.Format32bppArgb)
            bmp.SetResolution(Me.StyleGuide.PreferredDPI, Me.StyleGuide.PreferredDPI)
            Me.Grid.DrawToBitmap(bmp, rc)
            Return bmp

        End Function

#End Region ' Form overrides

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get a reference to the on-board <see cref="cQuickEditHandler">Quick edit handler</see>.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        <CLSCompliant(False)> _
        Protected ReadOnly Property QuickEditHandler() As cQuickEditHandler
            Get
                Return Me.m_qeHandler
            End Get
        End Property

        Private m_tsCreated As ToolStrip = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Set or removes a grid quick edit handler.
        ''' </summary>
        ''' <param name="bSet">Flag stating whether the q.e.handler should be set
        ''' (true) or released (false).</param>
        ''' <remarks>
        ''' This code is pretty robust, do not worry about calling it too much.
        ''' Note that it's important to release all event handlers when a form 
        ''' gets destroyed.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Sub SetQuickEditHandler(ByVal bSet As Boolean)
            If bSet Then
                If (Me.m_qeHandler Is Nothing) Then

                    Dim ts As ToolStrip = Me.FindToolstripRecursive(Me.Controls)
                    ' Not found?
                    If (ts Is Nothing) Then
                        ' #Yes: create toolstrip
                        ts = New cEwEToolstrip()
                        ts.Name = "tsQuickEdit"
                        Me.Controls.Add(ts)
                        Me.m_tsCreated = ts
                    End If

                    Me.m_qeHandler = New cQuickEditHandler()
                    Me.m_qeHandler.Attach(Me.Grid, Me.UIContext, ts, Me.Grid.IsOutputGrid)

                End If
            Else
                If (Me.m_qeHandler IsNot Nothing) Then
                    Me.m_qeHandler.Detach()
                    Me.m_qeHandler = Nothing

                    If (Me.m_tsCreated IsNot Nothing) Then
                        Me.Controls.Remove(Me.m_tsCreated)
                        Me.m_tsCreated.Dispose()
                        Me.m_tsCreated = Nothing
                    End If
                End If
            End If

            Me.PerformAutoScale()

        End Sub

        Private Function FindToolstripRecursive(ByVal controls As Control.ControlCollection) As ToolStrip
            If controls IsNot Nothing Then
                For Each c As Control In controls
                    If TypeOf c Is ToolStrip Then Return DirectCast(c, ToolStrip)
                    Dim ts As ToolStrip = Me.FindToolstripRecursive(c.Controls)
                    If ts IsNot Nothing Then Return ts
                Next
            End If
            Return Nothing
        End Function

#End Region ' Internals

    End Class

End Namespace
