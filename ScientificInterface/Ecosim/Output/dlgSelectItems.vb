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

Imports EwEUtils.Utilities

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
' Copyright 1991- UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'

Public Class dlgSelectItems
    Inherits Form

#Region " Private helper class "

    Private Class cItem

        Private m_strText As String = ""
        Private m_data As Integer = Nothing

        Public Sub New(ByVal strText As String, ByVal data As Integer)
            Me.m_strText = strText
            Me.m_data = data
        End Sub

        Public Overrides Function ToString() As String
            Return Me.m_strText
        End Function

        ReadOnly Property Data() As Integer
            Get
                Return Me.m_data
            End Get
        End Property

    End Class

#End Region ' Private helper class

#Region " Private vars "

    Private m_tdata As Type = Nothing
    Private m_fmt As ITypeFormatter = Nothing
    Private m_aobjDefaults As Integer() = Nothing
    Private m_aobjSelection As Integer() = Nothing

#End Region ' Private vars

#Region " Constructor "

    Public Sub New(ByVal tdata As Type, ByVal fmt As ITypeFormatter)

        Me.InitializeComponent()

        Me.m_tdata = tdata
        Me.m_fmt = fmt

        Debug.Assert(tdata.IsEnum, "Dialog only accepts enumerated types")
        Debug.Assert(fmt.GetDescribedType().IsAssignableFrom(tdata), "Type formatter does not match provided enumerated type")

    End Sub

#End Region ' Constructor

#Region " Form overrides "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Selected">Initial selection to show. Provide nothing to select all items in the provided enumerator.</param>
    ''' <param name="Defaults">Default selection to use when the user pressed 'Default'. Provide nothing to omit the defaults option.</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Overloads Function ShowDialog(ByVal owner As IWin32Window, _
                                         Optional ByVal Selected As Integer() = Nothing, _
                                         Optional ByVal Defaults As Integer() = Nothing) As DialogResult

        Me.m_aobjSelection = Selected
        Me.m_aobjDefaults = Defaults

        Return MyBase.ShowDialog(owner)
    End Function

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        For Each obj As Integer In [Enum].GetValues(Me.m_tdata)

            Dim iItem As Integer = Me.m_clbItems.Items.Add(New cItem(Me.m_fmt.GetDescriptor(obj), obj))
            Dim bSelect As Boolean = True

            If (Me.m_aobjSelection IsNot Nothing) Then
                bSelect = (Array.IndexOf(Me.m_aobjSelection, obj) > -1)
            End If

            Me.m_clbItems.SetItemChecked(iItem, bSelect)
        Next

        Me.m_btnDefaults.Visible = (Me.m_aobjDefaults IsNot Nothing)

    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

        Dim lSel As New List(Of Integer)
        For Each obj As Object In Me.m_clbItems.CheckedItems
            lSel.Add(DirectCast(obj, cItem).Data)
        Next
        Me.m_aobjSelection = lSel.ToArray

        MyBase.OnFormClosed(e)
    End Sub

#End Region ' Overrides

#Region " Properties "

    Public ReadOnly Property Selection() As Integer()
        Get
            Return Me.m_aobjSelection
        End Get
    End Property

#End Region ' Properties

#Region " Forms designer bits "

    Private WithEvents m_btnCancel As System.Windows.Forms.Button
    Private WithEvents m_btnOK As System.Windows.Forms.Button
    Private WithEvents m_btnDefaults As System.Windows.Forms.Button
    Private WithEvents m_btnNone As System.Windows.Forms.Button
    Private WithEvents m_btnAll As System.Windows.Forms.Button
    Private WithEvents m_clbItems As System.Windows.Forms.CheckedListBox
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgSelectItems))
        Me.m_clbItems = New System.Windows.Forms.CheckedListBox()
        Me.m_btnAll = New System.Windows.Forms.Button()
        Me.m_btnNone = New System.Windows.Forms.Button()
        Me.m_btnDefaults = New System.Windows.Forms.Button()
        Me.m_btnOK = New System.Windows.Forms.Button()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'm_clbItems
        '
        resources.ApplyResources(Me.m_clbItems, "m_clbItems")
        Me.m_clbItems.CheckOnClick = True
        Me.m_clbItems.FormattingEnabled = True
        Me.m_clbItems.Name = "m_clbItems"
        '
        'm_btnAll
        '
        resources.ApplyResources(Me.m_btnAll, "m_btnAll")
        Me.m_btnAll.Name = "m_btnAll"
        Me.m_btnAll.UseVisualStyleBackColor = True
        '
        'm_btnNone
        '
        resources.ApplyResources(Me.m_btnNone, "m_btnNone")
        Me.m_btnNone.Name = "m_btnNone"
        Me.m_btnNone.UseVisualStyleBackColor = True
        '
        'm_btnDefaults
        '
        resources.ApplyResources(Me.m_btnDefaults, "m_btnDefaults")
        Me.m_btnDefaults.Name = "m_btnDefaults"
        Me.m_btnDefaults.UseVisualStyleBackColor = True
        '
        'm_btnOK
        '
        resources.ApplyResources(Me.m_btnOK, "m_btnOK")
        Me.m_btnOK.Name = "m_btnOK"
        Me.m_btnOK.UseVisualStyleBackColor = True
        '
        'm_btnCancel
        '
        resources.ApplyResources(Me.m_btnCancel, "m_btnCancel")
        Me.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'dlgSelectItems
        '
        Me.AcceptButton = Me.m_btnOK
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.m_btnCancel
        Me.Controls.Add(Me.m_btnCancel)
        Me.Controls.Add(Me.m_btnOK)
        Me.Controls.Add(Me.m_btnDefaults)
        Me.Controls.Add(Me.m_btnNone)
        Me.Controls.Add(Me.m_btnAll)
        Me.Controls.Add(Me.m_clbItems)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgSelectItems"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.ResumeLayout(False)

    End Sub

#End Region ' Forms designer bits

#Region " Events "

    Private Sub OnOK(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnOK.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub OnCancel(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnCancel.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
        Me.m_aobjSelection = New Integer() {}
    End Sub

    Private Sub OnAll(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnAll.Click
        For i As Integer = 0 To Me.m_clbItems.Items.Count - 1
            Me.m_clbItems.SetItemChecked(i, True)
        Next
    End Sub

    Private Sub OnNone(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnNone.Click
        For i As Integer = 0 To Me.m_clbItems.Items.Count - 1
            Me.m_clbItems.SetItemChecked(i, False)
        Next
    End Sub

    Private Sub OnDefaults(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles m_btnDefaults.Click
        For i As Integer = 0 To Me.m_clbItems.Items.Count - 1
            Dim obj As cItem = DirectCast(Me.m_clbItems.Items(i), cItem)
            Dim bSelect As Boolean = True

            If (Me.m_aobjDefaults IsNot Nothing) Then
                bSelect = (Array.IndexOf(Me.m_aobjDefaults, obj.Data) > -1)
            End If
            Me.m_clbItems.SetItemChecked(i, bSelect)
        Next
    End Sub

#End Region ' Events

End Class
