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
Imports System
Imports System.Collections.Generic
Imports EwEUtils.SystemUtilities.cSystemUtils

#End Region ' Imports

Namespace Utilities

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Utility class for searching data in collections.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cFuzzySearch

        Public Sub New(Optional options As eSearchOptions = eSearchOptions.Default, _
                       Optional strSplitChars As String = " ,", _
                       Optional strQualifierChars As String = """()[]{}")
            Me.Options = options
            Me.SplitCharacters = strSplitChars
            Me.QualifierChars = strQualifierChars
        End Sub

        Public Enum eSearchOptions
            ''' <summary>Default search options.</summary>
            [Default] = 0
            ''' <summary>Eases the search by ignoring case character casing.</summary>
            IngoreCase = 1
            ''' <summary>Refines the search by splitting a search term in segments, and performs an additional matching on each segment.</summary>
            SearchSegments = 2
            ''' <summary>The full shebang.</summary>
            All = &HFF
        End Enum

        Public Property SplitCharacters As String
        Public Property QualifierChars As String
        Public Property IgnoredWords As String() = New String() {}
        Public Property MinWordLength As Integer = 3
        Public Property Options As eSearchOptions = eSearchOptions.Default

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Utility method for seaching a string in a string array by likelyhood.
        ''' </summary>
        ''' <param name="data">The collection to explore.</param>
        ''' <param name="strSearchTerm">The search term to look for.</param>
        ''' <param name="iThreshold">Search difference tolerance threshold [1, <see cref="Integer.MaxValue"/>>.</param>
        ''' <returns>An array of <see cref="cBaseSearchResult">fuzzy search results</see>.</returns>
        ''' -------------------------------------------------------------------
        Public Function Find(ByVal strSearchTerm As String, _
                             ByVal data As String(), _
                             ByVal iThreshold As Integer) As cArraySearchResult()

            ' Analyze options
            Dim bOptNoCase As Boolean = ((Me.Options And eSearchOptions.IngoreCase) = eSearchOptions.IngoreCase)
            Dim bOptSegmnt As Boolean = ((Me.Options And eSearchOptions.SearchSegments) = eSearchOptions.SearchSegments)

            Dim lSearchTerms As New List(Of String)
            Dim lArrayTerms As New List(Of String)
            Dim iOffset As Integer = 0
            Dim iDirectHit As Integer = -1
            Dim strTerm As String
            Dim results As New List(Of cArraySearchResult)

            Dim lIgnored As New List(Of String)
            If (IgnoredWords IsNot Nothing) Then
                If (bOptNoCase = True) Then
                    For Each strTerm In IgnoredWords : lIgnored.Add(strTerm.ToLower) : Next
                Else
                    lIgnored.AddRange(IgnoredWords)
                End If
            End If

            If bOptNoCase Then strSearchTerm = strSearchTerm.ToLower()

            lSearchTerms.Add(strSearchTerm)
            If (bOptSegmnt = True) Then
                lSearchTerms.AddRange(cStringUtils.SplitQualified(strSearchTerm, Me.SplitCharacters, Me.QualifierChars))
            End If

            ' For all source terms
            For iSearchTerm As Integer = 0 To lSearchTerms.Count - 1
                strTerm = lSearchTerms(iSearchTerm)

                If (Not String.IsNullOrWhiteSpace(strTerm)) And (Not lIgnored.Contains(strTerm)) And (strTerm.Length >= Me.MinWordLength) Then

                    ' Full-string match preferred
                    iOffset = CInt(If(iSearchTerm = 0, 0, 1))

                    ' Is a direct hit?
                    iDirectHit = Array.IndexOf(data, strTerm)
                    If (iDirectHit >= 0) Then
                        results.Add(New cArraySearchResult(iDirectHit, iOffset, data(iDirectHit)))
                    Else
                        ' Make term lowercase if string casing is irrelevant
                        If (bOptNoCase = True) Then strTerm = strTerm.ToLower

                        ' For all items in the data array
                        For i As Integer = 0 To data.Length - 1

                            If (Not String.IsNullOrWhiteSpace(data(i))) Then

                                ' Get next value from the array
                                lArrayTerms.Clear()
                                lArrayTerms.Add(data(i).Trim)
                                ' Split if need be
                                If (bOptSegmnt = True) Then
                                    lArrayTerms.AddRange(cStringUtils.SplitQualified(data(i), Me.SplitCharacters, Me.QualifierChars))
                                End If

                                For iArrayTerm As Integer = 0 To lArrayTerms.Count - 1
                                    Dim strArrayTerm As String = lArrayTerms(iArrayTerm)
                                    iOffset = CInt(If(iSearchTerm = 0 And iArrayTerm = 0, 0, 1))

                                    If (Not String.IsNullOrWhiteSpace(strArrayTerm)) And _
                                        (Not lIgnored.Contains(strArrayTerm)) And _
                                        (strArrayTerm.Length >= Me.MinWordLength) Then

                                        ' Make value lowercase if string casing is irrelevant
                                        If (Not bOptNoCase) Then strArrayTerm = strArrayTerm.ToLower
                                        ' Do strings equal within the specified treshold?
                                        If cStringUtils.DamerauLevenshteinDistance(strTerm, strArrayTerm, iThreshold) <= iThreshold Then
                                            ' #Yes: add the result to the result list
                                            results.Add(New cArraySearchResult(i, iThreshold, strArrayTerm))
                                        End If

                                    End If
                                Next
                            End If
                        Next
                    End If
                End If
            Next

            'sw.Stop()
            'Console.WriteLine("> Fuzzy: found " & results.Count & " hits for " & strSearchTerm & " in " & sw.Elapsed.Milliseconds & " ms.")

            ' Squeeze array: indices that are hit more than once will get their similarity boosted
            Dim n, m As Integer
            n = 0
            While (n < results.Count)
                m = n + 1
                While (m < results.Count)
                    If results(n).Index = results(m).Index Then
                        ' Boost similarity
                        results(n).Similarity = Math.Max(Math.Min(results(n).Similarity, results(m).Similarity) - 1, 0)
                        results(n).Term = CStr(results(n).Term) & "+" & CStr(results(m).Term)
                        results.RemoveAt(m)
                    Else
                        m += 1
                    End If
                End While
                n += 1
            End While

            ' Done
            results.Sort(New cSearchResultComparer())
            Return results.ToArray()

        End Function

#Region " Results "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' A single fuzzy search result, presenting the index of a search term 
        ''' in the array that was searched, and an indicator that quantifies how
        ''' well the item at the array index matched the original search term.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public MustInherit Class cBaseSearchResult

            Private m_iSimilarity As Integer = Integer.MaxValue
            Private m_term As Object

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Constructor
            ''' </summary>
            ''' <param name="iSimilarity">Similarity to the original search term, 
            ''' where 0 denotes an identical match.</param>
            ''' <param name="term">The matching term.</param>
            ''' -------------------------------------------------------------------
            Public Sub New(ByVal iSimilarity As Integer, ByVal term As Object)
                Me.m_iSimilarity = iSimilarity
                Me.m_term = term
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Returns a measure how closely the search result matches the original 
            ''' search term, where 1 denotes an identical match, 
            ''' and <see cref="Integer.MaxValue"/> denotes a total mismatch.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public Property Similarity As Integer
                Get
                    Return Me.m_iSimilarity
                End Get
                Protected Friend Set(iSimilarity As Integer)
                    Me.m_iSimilarity = iSimilarity
                End Set
            End Property

            Public Property Term As Object
                Get
                    Return Me.m_term
                End Get
                Protected Friend Set(term As Object)
                    Me.m_term = term
                End Set
            End Property

        End Class

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' A single fuzzy search result, presenting the index of a search term 
        ''' in the array that was searched, and an indicator that quantifies how
        ''' well the item at the array index matched the original search term.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Class cArraySearchResult
            Inherits cBaseSearchResult

            Private m_iIndex As Integer = -1

            ''' -------------------------------------------------------------------
            ''' <inheritdocs cref="cBaseSearchResult"/>.
            ''' <param name="iIndex">The array index of the search result.</param>
            ''' -------------------------------------------------------------------
            Protected Friend Sub New(ByVal iIndex As Integer, ByVal iSimilarity As Integer, term As Object)
                MyBase.New(iSimilarity, term)
                Me.m_iIndex = iIndex
            End Sub

            ''' -------------------------------------------------------------------
            ''' <summary>
            ''' Returns the array index of the search result.
            ''' </summary>
            ''' -------------------------------------------------------------------
            Public ReadOnly Property Index As Integer
                Get
                    Return Me.m_iIndex
                End Get
            End Property

        End Class

#End Region ' Results

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper class for sorting <see cref="cBaseSearchResult">search results</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Class cSearchResultComparer
            Implements IComparer(Of cBaseSearchResult)

            Public Function Compare(ByVal x As cBaseSearchResult, ByVal y As cBaseSearchResult) As Integer _
                Implements System.Collections.Generic.IComparer(Of cBaseSearchResult).Compare
                If x.Similarity < y.Similarity Then Return -1
                If x.Similarity = y.Similarity Then Return 0
                Return 1
            End Function

        End Class

#End Region ' Internals

    End Class

End Namespace

