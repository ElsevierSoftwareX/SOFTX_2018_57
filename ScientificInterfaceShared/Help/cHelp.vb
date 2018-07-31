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

Imports System.IO
Imports System.Windows.Forms

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Light-weight implementation of F1-driven application-wide help support.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cHelp
    Implements IMessageFilter

#Region " Private bits "

    Private Class cHelpTopic

        Private m_strTopic As String = ""
        Private m_strAltURL As String = ""

        Public Sub New(ByVal strTopic As String, ByVal strAltURL As String)
            Me.m_strTopic = strTopic
            Me.m_strAltURL = strAltURL
        End Sub

        Public ReadOnly Property Topic() As String
            Get
                Return Me.m_strTopic
            End Get
        End Property

        Public ReadOnly Property AltURL() As String
            Get
                Return Me.m_strAltURL
            End Get
        End Property

    End Class

    ''' <summary>The owner of the app help.</summary>
    Private m_ctlOwner As Control = Nothing
    ''' <summary>Local help file.</summary>
    Private m_strHelpFile As String = ""
    ''' <summary>Help URL to invoke for a control without help text set.</summary>
    Private m_strDefaultHelpURL As String = ""
    ''' <summary>Subdirectory for content pages within the help file.</summary>
    Private m_strHelpRoot As String = ""
    ''' <summary>Control that currently has the help focus.</summary>
    Private m_ctlContext As Control = Nothing
    ''' <summary>Dictionary of help topics.</summary>
    Private m_dtHelpTopics As New Dictionary(Of Control, cHelpTopic)

#End Region ' Private bits

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Private constructor to enforce Singleton
    ''' </summary>
    ''' <param name="strHelpFile">Path to the help file to use.</param>
    ''' <param name="strDefaultHelpURL">Default help page URL.</param>
    ''' <param name="strHelpRoot">In-help subdirectory for help content pages.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal ctlOwner As Control, _
                   ByVal strHelpFile As String, _
                   Optional ByVal strDefaultHelpURL As String = "", _
                   Optional ByVal strHelpRoot As String = "")

        ' Remember owner
        Me.m_ctlOwner = ctlOwner
        ' Set help file
        Me.m_strHelpFile = strHelpFile
        ' Set default help url
        Me.m_strDefaultHelpURL = strDefaultHelpURL
        ' Set help root
        Me.m_strHelpRoot = strHelpRoot

        ' Start listening for 'F1' key presses
        Application.AddMessageFilter(Me)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Set the help URL to display for a particular control.
    ''' </summary>
    ''' <param name="ctl">The control to set the help URL for.</param>
    ''' <param name="strAltURL">Alternate master help file URL, if any.</param>
    ''' <remarks>Note that this method does NOT capture the help focus.</remarks>
    ''' -----------------------------------------------------------------------
    Public WriteOnly Property HelpTopic(ByVal ctl As Control, _
                                        Optional ByVal strAltURL As String = "") As String
        Set(ByVal strURL As String)
            ' Safety check
            If (ctl Is Nothing) Then Return
            ' Clear
            'If Me.m_dtHelpTopics.ContainsKey(ctl) Then Me.m_dtHelpTopics.Remove(ctl)
            'If Object.ReferenceEquals(ctl, Me.ActiveHelpControl) Then Me.ActiveHelpControl = Nothing
            If Not String.IsNullOrEmpty(strURL) Then Me.m_dtHelpTopics.Add(ctl, New cHelpTopic(strURL, strAltURL))
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the control that is currently active for displaying help.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property ActiveHelpControl() As Control
        Get
            Return Me.m_ctlContext
        End Get
        Set(ByVal ctl As Control)
            Me.m_ctlContext = ctl
            Me.RemoveDeadWood()
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Show help!
    ''' </summary>
    ''' <param name="navType"></param>
    ''' -----------------------------------------------------------------------
    Public Sub ShowHelp(ByVal navType As HelpNavigator)

        Dim ctl As Control = Me.m_ctlContext
        Dim topic As cHelpTopic = Nothing

        If ctl Is Nothing Then ctl = Me.m_ctlOwner

        Select Case navType

            Case HelpNavigator.Topic
                If ctl Is Nothing Then Return
                If Me.m_dtHelpTopics.ContainsKey(ctl) Then
                    topic = Me.m_dtHelpTopics(ctl)
                    If String.IsNullOrWhiteSpace(topic.AltURL) Then
                        Help.ShowHelp(ctl, Me.m_strHelpFile, Path.Combine(Me.m_strHelpRoot, topic.Topic))
                    Else
                        Help.ShowHelp(ctl, topic.AltURL, topic.Topic)
                    End If
                Else
                    Help.ShowHelp(ctl, Me.m_strHelpFile, Path.Combine(Me.m_strHelpRoot, Me.m_strDefaultHelpURL))
                End If

            Case HelpNavigator.Find
                Help.ShowHelp(ctl, Me.m_strHelpFile, HelpNavigator.Find, ctl.Text)

            Case HelpNavigator.KeywordIndex
                Help.ShowHelpIndex(ctl, Me.m_strHelpFile)

            Case HelpNavigator.TableOfContents
                Help.ShowHelp(ctl, Me.m_strHelpFile, HelpNavigator.TableOfContents, ctl.Text)

            Case Else
                Debug.Assert(False, String.Format("Help mode {0} not supported", navType))

        End Select

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Detach help from UI components.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Clear()
        Me.m_dtHelpTopics.Clear()
        Me.m_ctlContext = Nothing
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="message"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function PreFilterMessage(ByRef message As Message) As Boolean _
        Implements IMessageFilter.PreFilterMessage

        Select Case CInt(message.Msg)

            Case &H100 ' Key down
                If CInt(message.WParam) = CInt(Keys.F1) Then
                    Me.ShowHelp(HelpNavigator.Topic)
                End If

        End Select

    End Function

    Private Sub RemoveDeadWood()
        Dim lDeadWood As New List(Of Control)
        For Each ctrl As Control In Me.m_dtHelpTopics.Keys()
            If (ctrl.IsDisposed) Then lDeadWood.Add(ctrl)
        Next
        For Each ctrl As Control In lDeadWood
            Me.m_dtHelpTopics.Remove(ctrl)
        Next
    End Sub

End Class
