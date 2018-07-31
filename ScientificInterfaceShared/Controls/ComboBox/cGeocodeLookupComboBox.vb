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

#Region " Imports "

Imports System.ComponentModel
Imports System.Threading
Imports ScientificInterfaceShared.GeoCode

#End Region ' Imports

Namespace Controls

    ''' ===========================================================================
    ''' <summary>
    ''' Multi-threaded combo box that attempts to find geocoded locations based
    ''' on the combo box text.
    ''' </summary>
    ''' ===========================================================================
    Public Class cGeocodeLookupComboBox
        Inherits ComboBox

#Region " Private variables "

        Private m_lookup As IGeoCodeLookup = Nothing
        Private m_searchThread As Thread = Nothing
        Private m_bIsSearching As Boolean = False
        Private m_bOkToAutoComplete As Boolean = False
        Private m_strLastSearch As String = ""

#End Region ' Private variables

#Region " Construction and destruction "

        ''' <summary>
        ''' 
        ''' </summary>
        Public Sub New()
            Me.m_lookup = New cMarineRegionsLookup()
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            ' Abort any search
            Me.Search("")
            Me.m_lookup = Nothing
            MyBase.Dispose(disposing)
        End Sub

#End Region ' Construction and destruction

#Region " Public interfacs "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the geocode location selected in the control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property SelectedLocation() As cGeoCodeLocation
            Get
                Return DirectCast(Me.SelectedItem, cGeoCodeLocation)
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>Event to notify whether a search is in progress.</summary>
        ''' <param name="sender"></param>
        ''' <param name="bSearching"></param>
        ''' -----------------------------------------------------------------------
        Public Event OnSeaching(ByVal sender As cGeocodeLookupComboBox, ByVal bSearching As Boolean)

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get whether a search is in progress.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property IsSearching() As Boolean
            Get
                Return Me.m_bIsSearching
            End Get
        End Property

        Public Property AutoSearch As Boolean = True

        <Browsable(False)>
        Public Property LookupEngine() As IGeoCodeLookup
            Get
                Return Me.m_lookup
            End Get
            Set(ByVal value As IGeoCodeLookup)
                Me.Search("")
                Me.m_lookup = value
            End Set
        End Property

        Public Sub Search()
            Me.Search(Me.Text)
        End Sub

#End Region ' Public interfacs

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Keypress handler to initiate a geolocation search.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnKeyDown(e As System.Windows.Forms.KeyEventArgs)

            Try
                If e.KeyCode = Keys.Down Or e.KeyCode = Keys.Up Or e.KeyCode = Keys.Enter Then
                    Me.m_bOkToAutoComplete = False
                Else
                    Me.m_bOkToAutoComplete = True
                End If
                MyBase.OnKeyDown(e)
            Catch ex As Exception

            End Try

        End Sub

        Protected Overrides Sub OnTextChanged(e As System.EventArgs)

            If Not Me.AutoSearch Then Return

            If Me.Text.Length <= 4 Then
                Me.Search("")
            ElseIf Me.m_bOkToAutoComplete Then
                Me.Search(Me.Text)
            End If
            Me.m_bOkToAutoComplete = False

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Abort an active search, and initiate a new geolocation search if a 
        ''' search criterion is provided.
        ''' </summary>
        ''' <param name="strFindStr">Text to find geolocatios for.</param>
        ''' -----------------------------------------------------------------------
        Private Sub Search(ByVal strFindStr As String)

            ' Let's be easy on the designer
            If Me.DesignMode Then Return

            Me.m_strLastSearch = strFindStr
            Me.DataSource = Nothing ' Forget forget forget

            If Me.m_searchThread IsNot Nothing Then
                Me.m_searchThread.Abort()
                Me.FireSearchingEvent(False)
            End If

            If String.IsNullOrEmpty(strFindStr) Or (Me.m_lookup Is Nothing) Then
                Return
            End If

            Me.FireSearchingEvent(True)

            Me.m_searchThread = New Thread(AddressOf SearchThread)
            Me.m_searchThread.Start(strFindStr)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Geocode lookup thread procedure.
        ''' </summary>
        ''' <param name="strFindStr"></param>
        ''' -----------------------------------------------------------------------
        Private Sub SearchThread(ByVal strFindStr As Object)

            Debug.Assert(Me.m_lookup IsNot Nothing)

            Dim aLocations As cGeoCodeLocation() = Me.m_lookup.FindPlaces(CStr(strFindStr))
            Me.BeginInvoke(New OnSearchResultsDelegate(AddressOf OnSearchResults), New Object() {aLocations})

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Delegate for passing geocode search results back to control thread.
        ''' </summary>
        ''' <param name="aLocations"></param>
        ''' -----------------------------------------------------------------------
        Private Delegate Sub OnSearchResultsDelegate(ByVal aLocations As cGeoCodeLocation())

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Apply geocode search results to combo box
        ''' </summary>
        ''' <param name="aLocations"></param>
        ''' -----------------------------------------------------------------------
        Private Sub OnSearchResults(ByVal aLocations As cGeoCodeLocation())

            If Me.m_strLastSearch.Length <= 4 Then Return

            Me.BindingContext = New BindingContext()
            Me.DataSource = aLocations
            Me.SuspendLayout()
            Me.BeginInvoke(New MethodInvoker(AddressOf PostProcessSearch), Nothing)

            ' Let UI process new datasource items

        End Sub

        Private Sub PostProcessSearch()

            Me.DroppedDown = True
            Me.Text = Me.m_strLastSearch
            Me.SelectionStart = Me.Text.Length
            Me.ResumeLayout()

            Me.m_strLastSearch = ""

            ' Notify world
            Me.FireSearchingEvent(False)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Notify the world that a search is in progress.
        ''' </summary>
        ''' <param name="bSearching">Flag inidicating if a search is active.</param>
        ''' -----------------------------------------------------------------------
        Private Sub FireSearchingEvent(ByVal bSearching As Boolean)
            Try
                RaiseEvent OnSeaching(Me, bSearching)
            Catch ex As Exception
                ' NOP
            End Try
        End Sub

#End Region ' Internals

    End Class

End Namespace
