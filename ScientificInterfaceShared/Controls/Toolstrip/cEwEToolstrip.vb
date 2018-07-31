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

Option Strict On
Imports System.ComponentModel
Imports System.Linq
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

Imports System.Reflection

#End Region ' Imports

Namespace Controls

    ''' ===========================================================================
    ''' <summary>
    ''' Toolstrip that automagically manages the visibile state of its separators.
    ''' </summary>
    ''' ===========================================================================
    Public Class cEwEToolstrip
        Inherits ToolStrip

#Region " Private vars "

        ''' <summary>Update lock flag.</summary>
        Private m_bInUpdate As Boolean = False

#End Region ' Private vars

#Region " Constructor "

        Public Sub New()
            MyBase.New()
        End Sub

#End Region ' Constructor

#Region " Overrides "

        Private m_bIsDirty As Boolean = False

        Protected Overrides Sub OnLayoutCompleted(e As EventArgs)
            MyBase.OnLayoutCompleted(e)

            ' Set default display properties
            Me.GripStyle = ToolStripGripStyle.Hidden
            'Me.RenderMode = ToolStripRenderMode.System

            If Not Me.DesignMode And Me.IsHandleCreated Then
                Me.m_bIsDirty = True
                BeginInvoke(New MethodInvoker(AddressOf ShowHideRepeatingSeparators))
            End If

        End Sub

#End Region ' Overrides

#Region " Public bits "

        ''' <summary>
        ''' Merge items from a source toolstrip into this toolstrip.
        ''' </summary>
        ''' <param name="tsSource"></param>
        ''' <returns>True if successful.</returns>
        ''' <remarks>
        ''' Note that merged toolstrip items are still connected to the lifespan of the <paramref name="tsSource">
        ''' source toolstrip</paramref>. Event handlers are not copied over yet. This means that the
        ''' source toolstrip can only be disposed when the merged toolstrip items are no longer needed.
        ''' </remarks>
        Public Function Merge(tsSource As ToolStrip) As Boolean

            If (tsSource Is Nothing) Then Return False

            Me.SuspendLayout()
            Try
                For i As Integer = tsSource.Items.Count - 1 To 0 Step -1
                    Dim item As ToolStripItem = tsSource.Items(i)
                    ' Event list not copied over properly with Clone extension - abandon efforts for now
                    ' This means that the source toolstrip cannot be disposed after the Merge has completed;
                    ' it needs to be kept alive until the end of the life of the merge target toolstrip
                    'Me.Items.Insert(0, Me.Clone(item))
                    Me.Items.Insert(0, item)
                Next
                tsSource.Items.Clear()
            Catch ex As Exception
                Debug.Assert(False)
            End Try

            Me.ResumeLayout()
            Return True

        End Function

#End Region ' Public bits

#Region " Internals "

        ''' <summary>
        ''' Note that this method ONLY works for left-to-right toolstrips
        ''' </summary>
        Private Sub ShowHideRepeatingSeparators()

            If Me.DesignMode Then Return
            If Not Me.m_bIsDirty Then Return

            Dim tsi As ToolStripItem = Nothing
            Dim iNumVisibleControl As Integer = 0 ' Num vis controls since last separator
            Dim iLastVisibleSeparator As Integer = -1 ' Position of last visible separator

            Me.SuspendLayout()

            ' For all toolbar items
            For i As Integer = 0 To Me.Items.Count - 1
                ' Get item
                tsi = Me.Items(i)
                ' Is a separator?
                If (TypeOf tsi Is ToolStripSeparator) Then
                    ' #Yes: show this separator only if it separates visible controls AND controls do not switch left/right alignment
                    If (iNumVisibleControl > 0) Then

                        ' Peek ahead for alignment switch
                        Dim al As Integer = CInt(tsi.Alignment)
                        Dim bShow As Boolean = True

                        For j As Integer = i + 1 To Me.Items.Count - 1
                            Dim tsiTest As ToolStripItem = Me.Items(j)
                            If (Not TypeOf tsiTest Is ToolStripSeparator) And (tsiTest.Visible) Then
                                bShow = (tsiTest.Alignment = al)
                                Exit For
                            End If
                        Next

                        ' Show separator
                        tsi.Visible = bShow
                        If (bShow) Then
                            ' Remember this visible separator
                            iLastVisibleSeparator = i
                        End If
                    Else
                        tsi.Visible = False
                    End If

                    ' Reset visible control count
                    iNumVisibleControl = 0
                Else
                    ' #No: count number of visible regular controls
                    If (tsi.Visible) Then
                        iNumVisibleControl += 1
                        iLastVisibleSeparator = 0
                    End If
                End If
            Next

            ' Fishished without visible controls since the last visible separator?
            If (iNumVisibleControl = 0 And iLastVisibleSeparator >= 0) Then
                ' #Yep: hide the last separator
                Me.Items(iLastVisibleSeparator).Visible = False
            End If

            Me.ResumeLayout()
            Me.m_bIsDirty = False

        End Sub

        Private s_lNextItem As Long = 0

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' From https://www.devexpress.com/Support/Center/Question/Details/Q454839
        ''' </summary>
        ''' <param name="itemSrc"></param>
        ''' <returns>A new, cloned tool strip item.</returns>
        ''' -------------------------------------------------------------------
        Private Function Clone(itemSrc As ToolStripItem) As ToolStripItem

            Dim itemNew As ToolStripItem = CType(Activator.CreateInstance(itemSrc.GetType()), ToolStripItem)
            Dim propInfoList As IEnumerable(Of PropertyInfo) = From p In GetType(ToolStripItem).GetProperties()
                                                               Let attributes = p.GetCustomAttributes(True)
                                                               Let notBrowseable = (From a In attributes
                                                                                    Where a.[GetType]() = GetType(BrowsableAttribute)
                                                                                    Select Not TryCast(a, BrowsableAttribute).Browsable).FirstOrDefault()
                                                               Where Not notBrowseable And p.CanRead And p.CanWrite Order By p.Name
                                                               Select p

            ' Copy sub items first
            If TypeOf itemSrc Is ToolStripDropDownButton Then
                For Each item As ToolStripItem In DirectCast(itemSrc, ToolStripDropDownButton).DropDownItems
                    DirectCast(itemNew, ToolStripDropDownButton).DropDownItems.Add(Clone(item))
                Next
            ElseIf TypeOf itemSrc Is ToolStripComboBox Then
                For Each item As Object In DirectCast(itemSrc, ToolStripComboBox).Items
                    DirectCast(itemNew, ToolStripComboBox).Items.Add(item)
                Next
            End If

            ' Copy over using reflections
            For Each propertyInfo As PropertyInfo In propInfoList
                Dim propertyInfoValue As Object = propertyInfo.GetValue(itemSrc, Nothing)
                propertyInfo.SetValue(itemNew, propertyInfoValue, Nothing)
            Next

            ' Create a new menu name
            itemNew.Name = itemSrc.Name & "-" & CStr(Math.Max(Threading.Interlocked.Increment(s_lNextItem), s_lNextItem - 1))

            ' Process any other properties
            If itemSrc.ImageIndex <> -1 Then
                itemNew.ImageIndex = itemSrc.ImageIndex
            End If

            If Not String.IsNullOrWhiteSpace(itemSrc.ImageKey) Then
                itemNew.ImageKey = itemSrc.ImageKey
            End If

            ' We need to make this visible 
            itemNew.Visible = itemSrc.Visible

            ' The handler list starts empty because we created its parent via a new
            AddHandlers(itemSrc, itemNew)

            Return itemNew

        End Function

        ''' <summary>
        ''' Adds the handlers from the source component to the destination component
        ''' </summary>
        Private Sub AddHandlers(source As ToolStripItem, target As ToolStripItem)
            Dim sourceEventHandlerList As EventHandlerList = EventHandlerList(source)
            Dim destEventHandlerList As EventHandlerList = EventHandlerList(target)
            destEventHandlerList.AddHandlers(sourceEventHandlerList)
        End Sub

        ''' <summary>
        ''' Gets the event handler list from a component
        ''' </summary>
        ''' <returns>The EventHanderList or null if none</returns>
        Private Function EventHandlerList(source As ToolStripItem) As EventHandlerList
            Dim eventsInfo As PropertyInfo = source.[GetType]().GetProperty("Events", BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Static)
            Return DirectCast(eventsInfo.GetValue(source, Nothing), EventHandlerList)
        End Function

#End Region ' Internals

    End Class

End Namespace
