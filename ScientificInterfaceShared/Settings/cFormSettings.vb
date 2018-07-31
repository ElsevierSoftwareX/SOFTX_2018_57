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
Imports System.Xml
Imports EwECore
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Forms
Imports WeifenLuo.WinFormsUI
Imports EwEUtils.Core

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' <para>Handy-dandy class that maintains and applies form information such as 
''' position, dock state, min/max state and a miscellaneous string of arbitrary
''' settings proprietary to individual forms classes.</para>
''' <para>The EwE framework makes this information persistent using the Application 
''' settings.</para>
''' </summary>
''' ===========================================================================
Public Class cFormSettings
    Implements IXMLDocSettings

#Region " Helper classes "

    ''' =======================================================================
    ''' <summary>
    ''' Helper class, holds and applies settings information for a single form.
    ''' </summary>
    ''' =======================================================================
    Private Class cFormSetting

#Region " Private vars "

        Private m_strName As String = ""
        Private m_iPosX As Integer = 0
        Private m_iPosY As Integer = 0
        Private m_iWidth As Integer = 0
        Private m_iHeight As Integer = 0
        Private m_dockWin As DockStyle = DockStyle.None
        Private m_dockWeifenLuo As Docking.DockState = Docking.DockState.Unknown
        Private m_formState As FormWindowState = FormWindowState.Normal
        Private m_strMisc As String = ""

#End Region ' Private vars

#Region " Constructor "

        Public Sub New(ByVal strFormName As String)
            Me.m_strName = strFormName
        End Sub

#End Region ' Constructor

#Region " Public bits "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Configure the form according to this setting.
        ''' </summary>
        ''' <param name="frm">The form to position.</param>
        ''' -------------------------------------------------------------------
        Public Sub Apply(ByVal frm As Form)

            frm.SuspendLayout()

            If frm.Parent Is Nothing Then

                Dim ptTL As New System.Drawing.Point(Me.m_iPosX, Me.m_iPosY)
                Dim ptBR As New System.Drawing.Point(Me.m_iPosX + Me.m_iWidth, Me.m_iPosY + Me.m_iHeight)
                Dim scTL As Screen = Nothing
                Dim scBR As Screen = Nothing

                For Each sc As Screen In Screen.AllScreens
                    If sc.WorkingArea.Contains(ptTL) Then scTL = sc
                    If sc.WorkingArea.Contains(ptBR) Then scBR = sc
                Next sc

                ' Position window ONLY when both screens are valid
                If scTL IsNot Nothing And scBR IsNot Nothing Then
                    frm.DesktopBounds = New Rectangle(Me.m_iPosX, Me.m_iPosY, Me.m_iWidth, Me.m_iHeight)
                End If
            Else
                frm.Location = New System.Drawing.Point(Me.m_iPosX, Me.m_iPosY)
                frm.Width = Me.m_iWidth
                frm.Height = Me.m_iHeight
            End If

            frm.WindowState = Me.m_formState

            If (TypeOf frm Is frmEwE) Then
                Dim fx As frmEwE = DirectCast(frm, frmEwE)
                fx.DockState = Me.m_dockWeifenLuo
                Try
                    fx.Settings = Me.m_strMisc
                Catch ex As Exception
                    cLog.Write(ex, "cFormSettings::Apply on " & frm.Text)
                End Try
            Else
                frm.Dock = Me.m_dockWin
            End If

            frm.ResumeLayout()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize by reading position information from a given form.
        ''' </summary>
        ''' <param name="frm">The form to read.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function Store(ByVal frm As Form) As Boolean
            Dim rc As Rectangle = Nothing

            If (frm Is Nothing) Then Return False

            If (frm.Parent Is Nothing) Then
                rc = frm.DesktopBounds
            Else
                rc = frm.RestoreBounds
            End If
            Me.m_iPosX = rc.X
            Me.m_iPosY = rc.Y
            Me.m_iWidth = rc.Width
            Me.m_iHeight = rc.Height

            If TypeOf frm Is frmEwE Then
                Me.m_dockWeifenLuo = DirectCast(frm, frmEwE).DockState
                Try
                    Me.m_strMisc = DirectCast(frm, frmEwE).Settings
                Catch ex As Exception
                    cLog.Write(ex, "cFormSettings::store on EwE form " & frm.Text)
                End Try
            Else
                Me.m_dockWin = frm.Dock
            End If
            Me.m_formState = frm.WindowState

            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize by reading a settings string.
        ''' </summary>
        ''' <param name="nodeSetting">The xml node to read.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function FromXML(ByVal nodeSetting As XmlNode) As Boolean
            Try
                Dim node As XmlNode = Nothing

                node = nodeSetting("location")
                If (node IsNot Nothing) Then
                    With node
                        Me.m_iPosX = cStringUtils.ConvertToInteger(.Attributes("x").InnerText)
                        Me.m_iPosY = cStringUtils.ConvertToInteger(.Attributes("y").InnerText)
                        Me.m_iWidth = cStringUtils.ConvertToInteger(.Attributes("w").InnerText)
                        Me.m_iHeight = cStringUtils.ConvertToInteger(.Attributes("h").InnerText)
                    End With
                End If

                node = nodeSetting("state")
                If (node IsNot Nothing) Then
                    With node
                        Me.m_dockWin = CType(Val(.Attributes("dockWin").InnerText), DockStyle)
                        Me.m_dockWeifenLuo = CType(Val(.Attributes("dockWFL").InnerText), Docking.DockState)
                        Me.m_formState = CType(Val(.Attributes("formpos").InnerText), FormWindowState)
                    End With
                End If

                node = nodeSetting("localsettings")
                If (node IsNot Nothing) Then
                    With node
                        Me.m_strMisc = .InnerText
                    End With
                End If

            Catch ex As Exception
                cLog.Write(ex, "cFormSettings::Initialize")
                Return False
            End Try
            Return True
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Produce a form settings XML node.
        ''' </summary>
        ''' <returns>A settings string.</returns>
        ''' -------------------------------------------------------------------
        Public Function ToXML(ByVal doc As XmlDocument) As XmlNode

            Dim node As XmlNode = doc.CreateElement("formsetting")
            Dim nodeChild As XmlNode = Nothing
            Dim attr As XmlAttribute = Nothing

            attr = doc.CreateAttribute("name")
            attr.InnerText = Me.m_strName
            node.Attributes.Append(attr)

            nodeChild = doc.CreateElement("location")
            attr = doc.CreateAttribute("x") : attr.InnerText = cStringUtils.FormatInteger(Me.m_iPosX) : nodeChild.Attributes.Append(attr)
            attr = doc.CreateAttribute("y") : attr.InnerText = cStringUtils.FormatInteger(Me.m_iPosY) : nodeChild.Attributes.Append(attr)
            attr = doc.CreateAttribute("w") : attr.InnerText = cStringUtils.FormatInteger(Me.m_iWidth) : nodeChild.Attributes.Append(attr)
            attr = doc.CreateAttribute("h") : attr.InnerText = cStringUtils.FormatInteger(Me.m_iHeight) : nodeChild.Attributes.Append(attr)
            node.AppendChild(nodeChild)

            nodeChild = doc.CreateElement("state")
            attr = doc.CreateAttribute("dockWin") : attr.InnerText = CStr(CInt(Me.m_dockWin)) : nodeChild.Attributes.Append(attr)
            attr = doc.CreateAttribute("dockWFL") : attr.InnerText = CStr(CInt(Me.m_dockWeifenLuo)) : nodeChild.Attributes.Append(attr)
            attr = doc.CreateAttribute("formpos") : attr.InnerText = CStr(CInt(Me.m_formState)) : nodeChild.Attributes.Append(attr)
            node.AppendChild(nodeChild)

            nodeChild = doc.CreateElement("localsettings")
            nodeChild.InnerText = Me.m_strMisc
            node.AppendChild(nodeChild)

            Return node

        End Function

        Public ReadOnly Property Name() As String
            Get
                Return Me.m_strName
            End Get
        End Property

#End Region ' Public bits

    End Class

#End Region ' Helper classes

#Region " Private vars "

    ''' <summary>All maintained form positions.</summary>
    Private m_dictFormSettings As New Dictionary(Of String, cFormSetting)

#End Region ' Private vars

#Region " Constructor "

    ''' -----------------------------------------------------------------------
    ''' <summary>Welcome, naive human.</summary>
    ''' -----------------------------------------------------------------------
    Public Sub New()
    End Sub

#End Region ' Constructor

#Region " Public interfaces "

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="IXMLDocSettings"/>
    ''' -----------------------------------------------------------------------
    Public Property Setting() As XmlDocument _
        Implements IXMLDocSettings.Settings
        Get
            Return Me.Content()
        End Get
        Set(ByVal value As XmlDocument)
            Me.Initialize(value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Store a forms' position.
    ''' </summary>
    ''' <param name="frm">The form to store the position for.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Store(ByVal frm As Form, Optional ByVal bIncludeFormText As Boolean = True)

        Dim fs As cFormSetting = Nothing
        Dim strFormType As String = ""

        ' Sanity check
        If frm Is Nothing Then Return
        strFormType = FormTypeString(frm, bIncludeFormText)

        ' Already has it?
        If Me.m_dictFormSettings.ContainsKey(strFormType) Then
            ' Obliterate
            Me.m_dictFormSettings.Remove(strFormType)
        End If

        ' Create form state
        fs = New cFormSetting(strFormType)
        ' Able to read from form?
        If fs.Store(frm) Then
            ' #Yes: store it
            Me.m_dictFormSettings(strFormType) = fs
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Update a forms' position from the position information held in this class.
    ''' </summary>
    ''' <param name="frm">The form to reposition.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Apply(ByVal frm As Form, Optional ByVal bIncludeFormText As Boolean = True)

        Dim strFormType As String = ""
        ' Sanity check
        If frm Is Nothing Then Return
        strFormType = FormTypeString(frm, bIncludeFormText)
        ' Get info
        If Me.m_dictFormSettings.ContainsKey(strFormType) Then
            ' Apply
            Me.m_dictFormSettings(strFormType).Apply(frm)
        End If
    End Sub

#End Region ' Public interfaces

#Region " Private bits "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load this class from application settings.
    ''' </summary>
    ''' <param name="settings">The settings to analyze.</param>
    ''' -----------------------------------------------------------------------
    Private Sub Initialize(ByVal settings As XmlDocument)

        Dim fp As cFormSetting = Nothing
        Dim node As XmlNode = Nothing

        ' Clear!
        Me.m_dictFormSettings.Clear()

        ' Sanity checks
        If (settings Is Nothing) Then Return
        If (settings.ChildNodes.Count = 0) Then Return

        ' For every form setting
        For Each node In settings.SelectNodes("/formsettings/formsetting")
            ' Is valid?
            Try
                fp = New cFormSetting(node.Attributes("name").InnerText)
                ' Can read form position data?
                If fp.FromXML(node) Then
                    ' #Yes: store in local admin!
                    Me.m_dictFormSettings(fp.Name) = fp
                End If
            Catch ex As Exception
                ' Woops - ignore malformed setting
            End Try
        Next

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Generate XML document from local data.
    ''' </summary>
    ''' <returns>A penguin. Really.</returns>
    ''' -----------------------------------------------------------------------
    Private Overloads Function Content() As XmlDocument

        Dim node As XmlNode = Nothing
        Dim doc As XmlDocument = cXMLUtils.NewDoc("formsettings", node, "utf-16")

        For Each strFormName As String In Me.m_dictFormSettings.Keys
            Try
                Dim ndForm As XmlNode = Me.m_dictFormSettings(strFormName).ToXML(doc)
                node.AppendChild(ndForm)
            Catch ex As Exception

            End Try
        Next
        Return doc

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, builds a string to 'uniquely' identify a form instance.
    ''' </summary>
    ''' <param name="frm">The form to identify.</param>
    ''' <returns>A string uniquely identifying a form instance.</returns>
    ''' -----------------------------------------------------------------------
    Private Function FormTypeString(ByVal frm As Form, ByVal bIncludeFormText As Boolean) As String
        If bIncludeFormText Then
            Return frm.GetType().FullName & "_" & frm.Text
        Else
            Return frm.GetType().FullName
        End If
    End Function

#End Region ' Private bits

End Class