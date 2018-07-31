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
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Text
Imports System
Imports System.Data
Imports System.Diagnostics

#End Region ' Imports

Namespace Database

    ''' -----------------------------------------------------------------------
    ''' <summary>Helper class, assists in the assembly of SQL queries by maintaining
    ''' dynamic parameters for WHERE, GROUP BY and ORDER clauses. 
    ''' <para>The basis for the QueryBuilder is a pre-formatted SQL query, containing 
    ''' placeholders where dynamic parts will be substituted into.</para>
    ''' <para>Reserved placeholders are [WHERE], [GROUP], [ORDER], which are substitution points 
    ''' for SQL WHERE, GROUP BY and ORDER BY clauses, which can be dynamically configured 
    ''' via the QueryBuilder.</para>
    ''' <para>A developer can define optional placeholders referred to as Fields. 
    ''' Typical Fields are table names, which may vary during use and thus need to 
    ''' be kept dynamic without having to rewrite an entire SQL query.</para>
    ''' <para>
    ''' For examples on how to use tis class, refer to the following methods:
    ''' <list>
    ''' <item><description><see cref="cQueryBuilder">QueryBuilder constructor</see></description></item>
    ''' <item><description><see cref="cQueryBuilder.AddClause">AddClause</see></description></item>
    ''' </list>
    ''' </para>
    ''' </summary>
    ''' -------------------------------------------------------
    Public Class cQueryBuilder

#Region " Attributes "

        ''' <summary>The Query that is to be formatted/extended.</summary>
        Private m_strQuery As String = ""
        ''' <summary>WHERE clauses</summary>
        Private m_lstrClauses As New List(Of String)
        ''' <summary>GROUP BY clauses</summary>
        Private m_lstrGroupBy As New List(Of String)
        ''' <summary>ORDER BY clauses</summary>
        Private m_lstrOrderBy As New List(Of String)
        ''' <summary>Fields that will be substituted dynamically when using the query</summary>
        Private m_dtFields As New Dictionary(Of String, String)

#End Region ' Attributes

#Region " Public methods "

        ''' -------------------------------------------------------
        ''' <summary>Intializes a new instance of the QueryBuilder class
        ''' </summary>
        ''' <param name="strQuery">The SQL query to maintain/extend</param>
        ''' <param name="fields">A string-to-string key value pair list
        ''' with values to substitute in dynamic fields upon query execution.</param>
        ''' <remarks>
        ''' An example:
        ''' <code>
        ''' ' A global query, defined for public use
        ''' Public Const sPERSONEL_QUERY As String = "SELECT [NAME_FIELD], [ADDRESS_FIELD] FROM [PERSON_TABLE] [WHERE]"
        ''' 
        ''' ...
        ''' 
        ''' ' In this example, the QueryBuilder will use the public query. It will
        ''' ' substitute placeholders in this query with real values, pertaining to the
        ''' ' context of the implementation. Just presume it's necessary, ok?
        ''' 
        ''' ' Create list of dynamic fields
        ''' Dim htFields as New Dictionary(of String, String)
        ''' htFields.Add("[NAME_FIELD]", "LastName")
        ''' htFields.Add("[ADDRESS_FIELD]", "Adress")
        ''' htFields.Add("[PERSON_TABLE]", "PERSONNEL")
        ''' 
        ''' ' Create query builder
        ''' Dim qb As New QueryBuilder(sPERSONEL_QUERY, htFields)
        ''' ' Add where clause
        ''' qb.AddClause("WHERE [PERSON_TABLE].[NAME_FIELD] LIKE 'A%'")
        ''' ' Get db
        ''' Dim sqlConn as SqlConnection = ....
        ''' 
        ''' ' Execute query
        ''' Dim sqlR as SqlDataReader = qb.GetReader(sqlConn)
        ''' ' Use the data
        ''' 
        ''' ..
        ''' ..
        ''' 
        ''' ' Cleanup
        ''' qb.ReleaseReader(sqlR)
        ''' </code>
        ''' </remarks>
        ''' -------------------------------------------------------
        Public Sub New(ByVal strQuery As String, _
                       Optional ByVal fields As KeyValuePair(Of String, String)() = Nothing)

            Me.m_strQuery = strQuery
            Me.CopyFields(fields)

        End Sub

        ''' -------------------------------------------------------
        ''' <summary>Produces the final SQL query. Order of processing is as follows:
        ''' <para>First, the reserved placeholders are substituted to generate the layout of the SQL statement.</para>
        ''' <para>Second, specified fields are substituted query-wide to resolve issues such as dynamic table and table column names.</para>
        ''' </summary>
        ''' <returns>The formatted SQL query</returns>
        ''' -------------------------------------------------------
        Public Overrides Function ToString() As String

            Dim dtParts As New Dictionary(Of String, String)

            dtParts("[WHERE]") = ConcatSegments("WHERE", m_lstrClauses.ToArray)
            dtParts("[GROUP]") = ConcatSegments("GROUP BY", m_lstrGroupBy.ToArray)
            dtParts("[ORDER]") = ConcatSegments("ORDER BY", m_lstrOrderBy.ToArray)

            ' Substitute parts, and substitute query-wide fields
            Return Subst(Subst(Me.m_strQuery, dtParts), Me.m_dtFields)

        End Function

        ''' -------------------------------------------------------
        ''' <summary>Add a SQL segment to the WHERE clause ([WHERE] placeholder)
        ''' </summary>
        ''' <param name="strSegment">The segment to add.</param>
        ''' -------------------------------------------------------
        Public Sub AddClause(ByVal strSegment As String)
            If (String.IsNullOrWhiteSpace(strSegment)) Then Return
            Me.m_lstrClauses.Add(strSegment)
        End Sub

        ''' -------------------------------------------------------
        ''' <summary>Add a SQL segment to the WHERE clause ([WHERE] placeholder) with a range of values for a single variable.
        ''' </summary>
        ''' <param name="strSegment">The segment to add.</param>
        ''' <param name="strField">The variable name that will be filtered.</param>
        ''' <param name="astrValues">An array of values that will be validated upon query execution.</param>
        ''' <returns>The number of values encountered in <paramref name="astrValues">astrValues</paramref></returns>
        ''' <remarks>
        ''' Examples:
        ''' <code>
        ''' ' Make querybuilder. Note that the query contains a placeholder for
        ''' ' clauses ([WHERE] field) without implementing a WHERE clause. When the QueryBuilder
        ''' ' is instructed to build its query, this field will be substituted away if no 
        ''' ' where clauses are defined. Without the [WHERE] placeholder the QueryBuilder will 
        ''' ' not know where to insert the WHERE clauses. It's not THAT smart, ok!
        ''' Dim qb As New QueryBuilder("SELECT * FROM PERSONNEL [WHERE]")
        ''' 
        ''' ' Define names filter
        ''' Dim alNames As New ArrrayList
        ''' alNames.Add("Lai")
        ''' alNames.Add("Steenbeek")
        ''' alNames.Add("Buszowski")
        ''' qb.AddClause("LastName in ([NAME])", "[NAME]", alNames)
        ''' 
        ''' ' Define addresses filter
        ''' Dim strAddresses = "4th ave, 16th ave, west broadway")
        ''' qb.AddClause("Address in ({ADDRESS})", "[ADDRESS]", strAddresses)
        ''' 
        ''' Dim strQuery As String = qb.ToString()
        ''' </code>
        ''' </remarks>
        ''' -------------------------------------------------------
        Public Function AddClause(ByVal strSegment As String, _
                                  ByVal strField As String, _
                                  ByVal astrValues() As String) As Integer

            Dim sbValues As New StringBuilder()
            Dim nValueCount As Integer = 0

            nValueCount = astrValues.Length
            For i As Integer = 0 To nValueCount - 1
                sbValues.Append(astrValues(i))
                If (i > 0) Then sbValues.Append(",")
            Next

            Dim strFormatted As String = Me.ToClauseString(strSegment, strField, sbValues.ToString())
            If (String.IsNullOrWhiteSpace(strFormatted)) Then Return 0

            Me.m_lstrClauses.Add(strFormatted)
            Return nValueCount

        End Function

        ''' -------------------------------------------------------
        ''' <summary>Add a SQL segment to the ORDER BY clause ([ORDER] placeholder).
        ''' </summary>
        ''' <param name="strOrder">The segment to add.</param>
        ''' <param name="fields">Hash table with fields to substitute (optional).</param>
        ''' -------------------------------------------------------
        Public Sub AddOrder(ByVal strOrder As String, _
                            Optional ByVal fields As KeyValuePair(Of String, String)() = Nothing)

            If (String.IsNullOrWhiteSpace(strOrder)) Then Return

            Me.m_lstrOrderBy.Add(strOrder)
            Me.CopyFields(fields)

        End Sub

        ''' -------------------------------------------------------
        ''' <summary>Add a SQL segment to the GROUP BY clause ([GROUP] placeholder).
        ''' </summary>
        ''' <param name="strGroup">The segment to add.</param>
        ''' <param name="fields">Value fields to substitute (optional).</param>
        ''' -------------------------------------------------------
        Public Sub AddGroup(ByVal strGroup As String, _
                            Optional ByVal fields As KeyValuePair(Of String, String)() = Nothing)

            If (String.IsNullOrWhiteSpace(strGroup)) Then Return

            Me.m_lstrGroupBy.Add(strGroup)
            Me.CopyFields(fields)

        End Sub

        ''' -------------------------------------------------------
        ''' <summary>Adds a generic field to substitute query-wide.
        ''' </summary>
        ''' <param name="strField">The field placeholder to look for.</param>
        ''' <param name="strReplace">The field value to replace this placeholder with.</param>
        ''' -------------------------------------------------------
        Public Sub AddField(ByVal strField As String, ByVal strReplace As String)

            If (String.IsNullOrWhiteSpace(strField)) Then Return

            If (Not Me.m_dtFields.ContainsKey(strField)) Then
                Me.m_dtFields.Add(strField, strReplace)
            End If

        End Sub

#End Region ' Public methods

#Region " Private methods "

        ''' -------------------------------------------------------
        ''' <summary>
        ''' Add specified fields to our series of keywords
        ''' </summary>
        ''' <param name="fields"></param>
        ''' -------------------------------------------------------
        Private Sub CopyFields(ByVal fields As KeyValuePair(Of String, String)())

            ' Copy fields. These will be substituted when the query is used
            If (Not fields Is Nothing) Then
                For Each field As KeyValuePair(Of String, String) In fields
                    Me.AddField(field.Key, field.Value)
                Next field
            End If
        End Sub

        ''' -------------------------------------------------------
        ''' <summary>
        ''' Concatenate filter and clauses into a SQL segment
        ''' </summary>
        ''' <param name="strType"></param>
        ''' <param name="astrClauses">Array of clauses</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------
        Private Function ConcatSegments(ByVal strType As String, ByVal astrClauses() As String) As String

            Dim nLen As Integer = 0
            Dim n As Integer = 0
            Dim sbClauses As New StringBuilder()

            ' Concat
            If (Not astrClauses Is Nothing) Then
                For n = 0 To astrClauses.Length - 1
                    If (n = 0) Then
                        sbClauses.Append(strType)
                        sbClauses.Append(" ")
                    Else
                        sbClauses.Append(" AND ")
                    End If
                    sbClauses.Append("(" & astrClauses(n) & ")")
                Next
            End If

            Return sbClauses.ToString()

        End Function

        ''' -------------------------------------------------------
        ''' <summary>
        ''' Substitute a series of fields from a hash table into a given string.
        ''' </summary>
        ''' <param name="strIn">The string to substitute values into</param>
        ''' <param name="dtSubst">Dictionary with key/value pairs to substitute</param>
        ''' <returns>The reworked string</returns>
        ''' -------------------------------------------------------
        Private Function Subst(ByVal strIn As String, _
                               Optional ByVal dtSubst As Dictionary(Of String, String) = Nothing) As String

            Dim strOut As String = strIn

            If (Not dtSubst Is Nothing) Then
                For Each strKey As String In dtSubst.Keys
                    strOut = strOut.Replace(strKey, dtSubst(strKey))
                Next
            End If
            Return strOut

        End Function

        Private Function ToClauseString(ByVal strSegment As String, _
                                        ByVal strField As String, _
                                        ByVal strValues As String) As String

            Dim strOut As String = ""

            ' Any values specified?
            If (strField.Length = 0) Then
                ' # No: just return the segment
                strOut = strSegment
            Else
                ' # Yes: determine method of formatting
                If (strSegment.ToLower().IndexOf(" in (") > 0) Then
                    ' Simple in: where var in (values)
                    strOut = strSegment.Replace(strField, strValues)
                Else
                    ' Elaborate formatting: repeat segment for every value
                    Dim astrValues As String() = strValues.Split(CChar(","))
                    Dim nCount As Integer = astrValues.Length

                    For i As Integer = 0 To nCount - 1
                        If (i > 0) Then strOut = strOut & " OR "
                        strOut &= strSegment.Replace(strField, astrValues(i))
                    Next i
                End If
            End If
            Return strOut

        End Function

#End Region ' Private methods

    End Class

End Namespace ' Misc.Database
