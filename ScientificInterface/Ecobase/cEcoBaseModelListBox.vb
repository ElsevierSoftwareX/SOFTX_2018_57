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
Imports EwECore.WebServices.Ecobase
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Public Class cEcoBaseModelListBox
    Inherits ScientificInterfaceShared.Controls.cFlickerFreeListBox
    Implements IUIElement

    Protected Overrides Sub OnDrawItem(e As System.Windows.Forms.DrawItemEventArgs)

        ' Sanity checks
        If (Me.UIContext Is Nothing) Then Return
        If (Me.Items.Count = 0) Then Return

        Dim item As Object = Me.Items(e.Index)
        Dim strText As String = e.ToString()

        ' Render default background 
        e.DrawBackground()

        ' Attempt to get item 
        If (TypeOf item Is cModelData) Then
            ' Get item fleet
            Dim model As cModelData = DirectCast(item, cModelData)
            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            If (Not model.AllowDissemination And e.State <> DrawItemState.Selected) Then
                Using br As New SolidBrush(sg.ApplicationColor(cStyleGuide.eApplicationColorType.READONLY_BACKGROUND))
                    e.Graphics.FillRectangle(br, e.Bounds)
                End Using
            End If
            strText = Me.ModelItemText(model)
        End If

        e.Graphics.DrawString(strText, e.Font, SystemBrushes.ControlText, e.Bounds.X, e.Bounds.Y)

        ' Render default focus rectangle
        e.DrawFocusRectangle()
    End Sub

    Public Property UIContext As ScientificInterfaceShared.Controls.cUIContext _
        Implements ScientificInterfaceShared.Controls.IUIElement.UIContext

    Public Property ShowYear As Boolean = True
    Public Property ShowAuthor As Boolean = True

    Private Function ModelItemText(model As cModelData) As String

        Dim strModelName As String = model.Name.Trim

        If (Me.ShowYear Or Me.ShowAuthor) Then
            Dim strDetail As String = ""
            Dim strAuthor As String = model.Author
            Dim strYear As String = CStr(model.FirstYear)

            If (Me.ShowYear = Me.ShowAuthor) Then
                strDetail = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DOUBLE, strYear, strAuthor)
            ElseIf Me.ShowYear Then
                strDetail = strYear
            Else
                strDetail = strAuthor
            End If
            Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, strModelName, strDetail)
        End If

        Return strModelName

    End Function

    Protected Overrides Sub OnFormat(e As System.Windows.Forms.ListControlConvertEventArgs)
        If (TypeOf e.ListItem Is cModelData) Then
            e.Value = Me.ModelItemText(DirectCast(e.ListItem, cModelData))
        End If
        MyBase.OnFormat(e)
    End Sub

End Class
