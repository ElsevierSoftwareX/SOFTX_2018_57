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
Imports System.Diagnostics
Imports System.Drawing
Imports System.Globalization
Imports System.Security
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities

#End Region ' Imports

Namespace Utilities

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Class offering string utilities.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cStringUtils

        ''' <summary><para>If true, CSV formatting is more restrictive than usual.
        ''' <list type="bullet"><item>headers will 
        ''' only be allowed to contain characters, numbers and underscores. All 
        ''' characters not matching this criteria will be replaced by underscores. 
        ''' Tools such as ArcGIS require this type of CSV formatting.</item>
        ''' </list>
        ''' </para>
        ''' </summary>
        Public Shared Property StrictCSVFormatting As Boolean = False

        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Split function that supports text qualifiers. Code adapted from Larry Steinly,
        ''' http://www.codeproject.com/Articles/15361/Split-Function-that-Supports-Text-Qualifiers.
        ''' </summary>
        ''' <param name="strExpression">String to split.</param>
        ''' <param name="strDelimiter">Delimiting character to split by.</param>
        ''' <param name="strQualifier">String qualifier, such as single or double quotes. Qualified string
        ''' segments will not be subdivided by delimiting characters.</param>
        ''' <returns>An array of strings.</returns>
        ''' ---------------------------------------------------------------------------
        Public Shared Function SplitQualified(ByVal strExpression As String,
                                              ByVal strDelimiter As String,
                                              Optional ByVal strQualifier As String = """") As String()

            ' Sanity check
            If String.IsNullOrEmpty(strExpression) Then Return New String() {String.Empty}

            ' Ensure defaults. A whitespace delimiter is allowed!
            If String.IsNullOrEmpty(strDelimiter) Then strDelimiter = ","
            If String.IsNullOrWhiteSpace(strQualifier) Then strQualifier = """"

            Dim bQualifier As Boolean = False
            Dim iStart As Integer = 0
            Dim lValues As New List(Of String)
            Dim iQL As Integer = strQualifier.Length
            Dim iDL As Integer = strDelimiter.Length
            Dim strVal As String = ""

            For iChar As Integer = 0 To strExpression.Length - 1
                If String.Compare(strExpression.Substring(iChar, iQL), strQualifier, True) = 0 Then
                    bQualifier = Not bQualifier
                ElseIf Not bQualifier And String.Compare(strExpression.Substring(iChar, strDelimiter.Length), strDelimiter, True) = 0 Then
                    ' Crop leading and trainling delimiter
                    strVal = strExpression.Substring(iStart, iChar - iStart)
                    If strVal.StartsWith(strQualifier) Then strVal = strVal.Substring(iQL)
                    If strVal.EndsWith(strQualifier) Then strVal = strVal.Substring(0, strVal.Length - iQL)
                    lValues.Add(strVal)
                    iStart = iChar + 1
                End If
            Next

            If (iStart < strExpression.Length) Then
                ' Crop leading and trainling delimiter
                strVal = strExpression.Substring(iStart)
                If strVal.StartsWith(strQualifier) Then strVal = strVal.Substring(iQL)
                If strVal.EndsWith(strQualifier) Then strVal = strVal.Substring(0, strVal.Length - iQL)
                lValues.Add(strVal)
            End If

            Return lValues.ToArray()

        End Function


        ''' ---------------------------------------------------------------------------
        ''' <summary>
        ''' Split function that supports text qualifiers.
        ''' </summary>
        ''' <param name="strExpression">String to split.</param>
        ''' <param name="cDelimiter">Delimiting character to split by.</param>
        ''' <param name="cQualifier">String qualifier, such as single or double quotes. Qualified string
        ''' segments will not be subdivided by delimiting characters.</param>
        ''' <returns>An array of strings.</returns>
        ''' <remarks>
        ''' <para>REgEx splitting is too slow. Replaced by a self-written, much faster method.</para>
        ''' <para>Support for "" to indicate " is needed!</para>
        ''' </remarks>
        ''' ---------------------------------------------------------------------------
        Public Shared Function SplitQualified(ByVal strExpression As String,
                                              ByVal cDelimiter As Char,
                                              Optional ByVal cQualifier As Char = """"c) As String()
            Return cStringUtils.SplitQualified(strExpression, CStr(cDelimiter), CStr(cQualifier))
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the first &lt;a href=&quot;..&gt; hyperlink within a string.
        ''' </summary>
        ''' <param name="strIn">The string to scan for hyperlinks.</param>
        ''' <param name="strOut">The input string with first hyperlink removed if
        ''' <paramref name="bStripLink"/> is set to True.</param>
        ''' <param name="iStart">The start position of the hyperlink in <paramref name="strOut"/>.</param>
        ''' <param name="iEnd">The end position of the hyperlink in <paramref name="strOut"/>.</param>
        ''' <returns>An hyperlink, or an empty string if no such link was found.</returns>
        ''' <remarks>This code is very simple, and does not use regular expressions 
        ''' for performance reasons. Detection is limited to the direct sequence 'a href=' only.</remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function Hyperlink(ByVal strIn As String,
                                         ByRef strOut As String, ByRef iStart As Integer, ByRef iEnd As Integer,
                                         Optional ByVal bStripLink As Boolean = True) As String

            Dim strLink As String = ""
            Dim i, j As Integer
            Dim quotes As Char() = New Char() {""""c, "'"c}
            iStart = -1
            iEnd = -1

            i = strIn.IndexOf("<a href=", StringComparison.CurrentCultureIgnoreCase)
            If i > -1 Then
                Dim sbOut As New StringBuilder()
                If (i > 0) Then sbOut.Append(strIn.Substring(0, i))

                If bStripLink Then iStart = i Else iStart = j

                i = strIn.IndexOfAny(quotes, i + 8)
                j = strIn.IndexOfAny(quotes, i + 1)
                If (i > 0 And j > i) Then strLink = strIn.Substring(i + 1, j - i - 1)

                i = strIn.IndexOf(">"c, j)
                j = strIn.IndexOf("</a>", i + 1, StringComparison.CurrentCultureIgnoreCase)
                If (i > 0 And j > i) Then
                    sbOut.Append(strIn.Substring(i + 1, j - i - 1))
                    If bStripLink Then iEnd = sbOut.Length Else iEnd = j + 4
                    sbOut.Append(strIn.Substring(j + 4))
                End If
                strOut = sbOut.ToString
            End If

            If String.IsNullOrWhiteSpace(strLink) Or (iEnd = -1) Then
                iStart = -1
                iEnd = -1
                strOut = strIn
            End If

            If Not bStripLink Then
                strOut = strIn
            End If

            Return strLink

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the number that exceeds the highest number in a range of 
        ''' existing autonumbered strings by one.
        ''' </summary>
        ''' <param name="astrItems">Existing autonumbered strings.</param>
        ''' <param name="strMask">Mask used to create the autonumbered strings.</param>
        ''' <param name="strMaskNumberPlaceholder">Placeholder for the number in 
        ''' the <paramref name="strMask">mask</paramref>.</param>
        ''' <returns>An integer value.</returns>
        ''' <remarks type="sidenote">
        ''' I found that using regular expressions did not really pay off as an
        ''' alternative to this maybe clumsy mothodology. Feel free to improve.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function GetNextNumber(ByVal astrItems() As String, ByVal strMask As String,
                Optional ByVal strMaskNumberPlaceholder As String = "{0}") As Integer

            ' Sanity checks
            Debug.Assert(Not String.IsNullOrEmpty(strMask), "Mask cannot be emtpy")
            Debug.Assert(Not String.IsNullOrEmpty(strMaskNumberPlaceholder), "Number placeholder cannot be emtpy")
            Debug.Assert(strMask.IndexOf(strMaskNumberPlaceholder) > -1, "Mask must contain number placeholder")

            Dim iMaskLength As Integer = 0 ' Number of chars in the mask
            Dim iMaskLeft As Integer = 0 ' Number of mask chars to the left side of the number placeholder
            Dim iMaskRight As Integer = 0 ' Number of mask chars to the right side of the number placeholder
            Dim strItem As String = "" ' Item string to analyze
            Dim iItemLength As Integer = 0 '  Number of chars in the item string
            Dim bAssessItem As Boolean = True ' States whether a given item is likely to be created with the format mask
            Dim strNumber As String = "" ' Number string extracted from items
            Dim iMax As Integer = 0 ' The max number found

            If (astrItems IsNot Nothing) Then

                ' Give this a sensible start value
                iMax = astrItems.Length

                ' Analyze mask for number placeholder
                iMaskLength = strMask.Length
                iMaskLeft = strMask.IndexOf(strMaskNumberPlaceholder)
                iMaskRight = iMaskLength - (iMaskLeft + strMaskNumberPlaceholder.Length)

                ' Try to determine the max number in each of the provided strings
                For iItem As Integer = 0 To astrItems.Length - 1
                    ' Get next string
                    strItem = astrItems(iItem)
                    ' Determine its length
                    iItemLength = strItem.Length

                    ' Assess if this item could have been generated with the format mask
                    ' - Does the item have sufficient length?
                    bAssessItem = (iItemLength > (iMaskLeft + iMaskRight))

                    ' Does the item contain all mask characters other than the number placeholder chars?
                    ' - Compare characters to the left of the likely location of the number
                    If ((bAssessItem = True) And (iMaskLeft > 0)) Then
                        ' Accept the item when it contains exactly the same chars as the mask, case independent
                        bAssessItem = strItem.StartsWith(strMask.Substring(0, iMaskLeft), StringComparison.CurrentCultureIgnoreCase)
                    End If
                    ' - Compare characters to the right of the likely location of the number
                    If (bAssessItem And iMaskRight > 0) Then
                        ' Accept the item when it contains exactly the same chars as the mask, case independent
                        bAssessItem = strItem.EndsWith(strMask.Substring(iMaskLength - iMaskRight), StringComparison.CurrentCultureIgnoreCase)
                    End If

                    ' Is this still likely to be a string generated with the mask?
                    If (bAssessItem) Then
                        ' #Yes: Attempt to extract a number
                        strNumber = astrItems(iItem).Substring(iMaskLeft, iItemLength - (iMaskLeft + iMaskRight))
                        Try
                            ' Conversion to Int may cause arithmic overflows etc so let's wear proper protection
                            iMax = Math.Max(iMax, Integer.Parse(strNumber))
                        Catch ex As Exception
                            ' Kaboom! Whoah, ignore this string, it's trouble.
                        End Try
                    End If
                Next iItem
            End If

            ' And yes, it COULD crash here if the iMax happened to hold Integer.MaxValue....
            Return (iMax + 1)

        End Function

        Public Shared Function BeginsWithOneOf(ByVal strSrc As String, ByVal astrCompareTo() As String, Optional ByVal bIgnoreCase As Boolean = True) As Boolean
            For Each strCompareTo As String In astrCompareTo
                If BeginsWith(strSrc, strCompareTo, bIgnoreCase) Then Return True
            Next
            Return False
        End Function

        Public Shared Function BeginsWith(ByVal strSrc As String, ByVal strCompareTo As String, Optional ByVal bIgnoreCase As Boolean = True) As Boolean
            If (strSrc.Length < strCompareTo.Length) Then Return False
            Dim iLen As Integer = Math.Min(strSrc.Length, strCompareTo.Length)

            strSrc = strSrc.Substring(0, iLen)
            strCompareTo = strCompareTo.Substring(0, iLen)
            Return String.Compare(strSrc, strCompareTo, bIgnoreCase) = 0

        End Function

        Public Shared Function EndsWith(ByVal strSrc As String, ByVal strCompareTo As String, Optional ByVal bIgnoreCase As Boolean = True) As Boolean
            Dim iLen As Integer = Math.Min(strSrc.Length, strCompareTo.Length)

            strSrc = strSrc.Substring(strSrc.Length - iLen, iLen)
            strCompareTo = strCompareTo.Substring(0, iLen)
            Return String.Compare(strSrc, strCompareTo, bIgnoreCase) = 0

        End Function

        Public Shared Function Shift(ByVal strIn As String) As String
            Dim strOut As String = ""
            For Each c As Char In strIn.ToCharArray
                strOut += Convert.ToChar(Convert.ToByte(c) - 1)
            Next
            Return strOut
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Truncate a string to make sure that it does not exceed a given number
        ''' of characters.
        ''' </summary>
        ''' <param name="strIn">The string to truncate.</param>
        ''' <param name="iMaxLength">The maximum length of the output string.</param>
        ''' <returns>A string of no more than <paramref name="iMaxLength"/> 
        ''' characters in length.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function MaxLength(ByVal strIn As String, iMaxLength As Integer) As String
            Return strIn.Substring(0, Math.Min(strIn.Length, iMaxLength))
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Converts an arabic value into a roman representation.
        ''' </summary>
        ''' <param name="nArabicValue">The value to convert.</param>
        ''' <returns>A number in roman format, in upper case.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToRoman(ByVal nArabicValue As Integer) As String

            Dim nThousands As Integer
            Dim nFiveHundreds As Integer
            Dim nHundreds As Integer
            Dim nFifties As Integer
            Dim nTens As Integer
            Dim nFives As Integer
            Dim nOnes As Integer
            Dim sbNumber As New StringBuilder()

            'take the value passed and split it out
            'to values representing the number of
            'ones, tens, hundreds, etc
            nOnes = nArabicValue
            nThousands = nOnes \ 1000
            nOnes = nOnes - nThousands * 1000
            nFiveHundreds = nOnes \ 500
            nOnes = nOnes - nFiveHundreds * 500
            nHundreds = nOnes \ 100
            nOnes = nOnes - nHundreds * 100
            nFifties = nOnes \ 50
            nOnes = nOnes - nFifties * 50
            nTens = nOnes \ 10
            nOnes = nOnes - nTens * 10
            nFives = nOnes \ 5
            nOnes = nOnes - nFives * 5

            'using VB's String function, create
            'a series of strings representing
            'the number of each respective denomination
            sbNumber.Append(New String("M"c, nThousands))

            'handle those cases where the denominator
            'value is on either side of a roman numeral
            If nHundreds = 4 Then
                If nFiveHundreds = 1 Then
                    sbNumber.Append("CM")
                Else
                    sbNumber.Append("CD")
                End If
            Else
                'not a 4, so create the string
                sbNumber.Append(New String("D"c, nFiveHundreds))
                sbNumber.Append(New String("C"c, nHundreds))
            End If

            If nTens = 4 Then
                If nFifties = 1 Then
                    sbNumber.Append("XC")
                Else
                    sbNumber.Append("XL")
                End If
            Else
                sbNumber.Append(New String("L"c, nFifties))
                sbNumber.Append(New String("X"c, nTens))
            End If

            If nOnes = 4 Then
                If nFives = 1 Then
                    sbNumber.Append("IX")
                Else
                    sbNumber.Append("IV")
                End If
            Else
                sbNumber.Append(New String("V"c, nFives))
                sbNumber.Append(New String("I"c, nOnes))
            End If

            Return sbNumber.ToString()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts a string into an targeted type using
        ''' the fixed EwE number format of decimal points and NO thousands separator.
        ''' </summary>
        ''' <param name="strNumber">The number to convert.</param>
        ''' <param name="typeTarget">The target type.</param>
        ''' <param name="strDecimalSeparator">Separator for decimals.</param>
        ''' <param name="strThousandsSeparator">Separator for thousands (a.k.a digit grouping separator)</param>
        ''' <param name="objNullValue">Value to return in case parse failed.</param>
        ''' <returns>An number.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ConvertToNumber(ByVal strNumber As String,
                                                ByVal typeTarget As Type,
                                                Optional ByVal objNullValue As Object = -9999,
                                                Optional ByVal strDecimalSeparator As String = ".",
                                                Optional ByVal strThousandsSeparator As String = "") As Object
            If typeTarget Is GetType(Single) Then
                Return ConvertToSingle(strNumber, CSng(objNullValue), strDecimalSeparator, strThousandsSeparator)
            ElseIf typeTarget Is GetType(Double) Then
                Return ConvertToDouble(strNumber, CDbl(objNullValue), strDecimalSeparator, strThousandsSeparator)
            ElseIf typeTarget Is GetType(Decimal) Then
                Return ConvertToDecimal(strNumber, CDec(objNullValue), strDecimalSeparator, strThousandsSeparator)
            End If
            Return ConvertToInteger(strNumber, CInt(objNullValue), strDecimalSeparator, strThousandsSeparator)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts a string into an integer value using
        ''' the fixed EwE number format of decimal points and NO thousands separator.
        ''' </summary>
        ''' <param name="strNumber">The number to convert.</param>
        ''' <param name="strDecimalSeparator">Separator for decimals.</param>
        ''' <param name="strThousandsSeparator">Separator for thousands (a.k.a digit grouping separator)</param>
        ''' <param name="iNullValue">Value to return in case parse failed.</param>
        ''' <returns>An integer value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ConvertToInteger(ByVal strNumber As String,
                                                Optional ByVal iNullValue As Integer = -9999,
                                                Optional ByVal strDecimalSeparator As String = ".",
                                                Optional ByVal strThousandsSeparator As String = "") As Integer

            Select Case strNumber.Trim
                Case "-", "_" : strNumber = ""
            End Select

            If Not String.IsNullOrEmpty(strNumber) Then

                Try

                    Dim ci As CultureInfo = System.Globalization.CultureInfo.CurrentCulture
                    Dim ni As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)
                    Dim iValue As Integer = iNullValue

                    ni.NumberDecimalSeparator = strDecimalSeparator
                    ni.NumberGroupSeparator = strThousandsSeparator

                    If Integer.TryParse(strNumber, NumberStyles.Any, ni, iValue) Then
                        Return iValue
                    End If

                Catch ex As Exception

                End Try

            End If

            Return iNullValue

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts a string into a single value using
        ''' the fixed EwE number format of decimal points and NO thousands separator.
        ''' </summary>
        ''' <param name="strNumber">The number to convert.</param>
        ''' <param name="strDecimalSeparator">Separator for decimals.</param>
        ''' <param name="strThousandsSeparator">Separator for thousands (a.k.a digit grouping separator)</param>
        ''' <param name="sNullValue">Value to return in case parse failed.</param>
        ''' <returns>A single value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ConvertToSingle(ByVal strNumber As String,
                                               Optional ByVal sNullValue As Single = -9999.0!,
                                               Optional ByVal strDecimalSeparator As String = ".",
                                               Optional ByVal strThousandsSeparator As String = "") As Single

            Select Case strNumber.Trim
                Case "-", "_" : strNumber = ""
            End Select

            If Not String.IsNullOrEmpty(strNumber) Then

                Try
                    Dim ci As CultureInfo = System.Globalization.CultureInfo.CurrentCulture
                    Dim ni As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)
                    Dim sValue As Single = sNullValue

                    ni.NumberDecimalSeparator = strDecimalSeparator
                    ni.NumberGroupSeparator = strThousandsSeparator

                    If Single.TryParse(strNumber, NumberStyles.Any, ni, sValue) Then
                        Return sValue
                    End If

                Catch ex As Exception
                    ' Whoah!
                End Try

            End If

            Return sNullValue

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts a string into a single value using
        ''' the fixed EwE number format of decimal points and NO thousands separator.
        ''' </summary>
        ''' <param name="strNumber">The number to convert.</param>
        ''' <param name="strDecimalSeparator">Separator for decimals.</param>
        ''' <param name="strThousandsSeparator">Separator for thousands (a.k.a digit grouping separator)</param>
        ''' <param name="dNullValue">Value to return in case parse failed.</param>
        ''' <returns>A double value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ConvertToDouble(ByVal strNumber As String,
                                               Optional ByVal dNullValue As Double = -9999.0#,
                                               Optional ByVal strDecimalSeparator As String = ".",
                                               Optional ByVal strThousandsSeparator As String = "") As Double

            Select Case strNumber.Trim
                Case "-", "_" : strNumber = ""
            End Select

            If Not String.IsNullOrEmpty(strNumber) Then

                Try

                    Dim ci As CultureInfo = System.Globalization.CultureInfo.CurrentCulture
                    Dim ni As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)
                    Dim dValue As Double = dNullValue

                    ni.NumberDecimalSeparator = strDecimalSeparator
                    ni.NumberGroupSeparator = strThousandsSeparator

                    If Double.TryParse(strNumber, NumberStyles.Any, ni, dValue) Then
                        Return dValue
                    End If
                Catch ex As Exception

                End Try

            End If
            Return dNullValue

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts a string into a single value using
        ''' the fixed EwE number format of decimal points and NO thousands separator.
        ''' </summary>
        ''' <param name="strNumber">The number to convert.</param>
        ''' <param name="strDecimalSeparator">Separator for decimals.</param>
        ''' <param name="strThousandsSeparator">Separator for thousands (a.k.a digit grouping separator)</param>
        ''' <param name="dNullValue">Value to return in case parse failed.</param>
        ''' <returns>A decimal value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ConvertToDecimal(ByVal strNumber As String,
                                               Optional ByVal dNullValue As Decimal = -9999D,
                                               Optional ByVal strDecimalSeparator As String = ".",
                                               Optional ByVal strThousandsSeparator As String = "") As Decimal

            Select Case strNumber.Trim
                Case "-", "_" : strNumber = ""
            End Select

            If Not String.IsNullOrEmpty(strNumber) Then

                Try

                    Dim ci As CultureInfo = System.Globalization.CultureInfo.CurrentCulture
                    Dim ni As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)
                    Dim dValue As Decimal = dNullValue

                    ni.NumberDecimalSeparator = strDecimalSeparator
                    ni.NumberGroupSeparator = strThousandsSeparator

                    If Decimal.TryParse(strNumber, NumberStyles.Any, ni, dValue) Then
                        Return dValue
                    End If
                Catch ex As Exception

                End Try

            End If
            Return dNullValue

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts a decimal value into a string with
        ''' a given number of releveant decimal digits, and custom decimal and
        ''' thousand separators.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FormatNumber(ByVal value As Object,
                                            Optional ByVal strDecimalSeparator As String = ".",
                                            Optional ByVal strThousandsSeparator As String = "",
                                            Optional ByVal iNumDigits As Integer = -9999) As String

            If (Convert.IsDBNull(value)) Then Return ""

            If TypeOf value Is Single Then
                Return FormatSingle(CSng(value), strDecimalSeparator, strThousandsSeparator, iNumDigits)
            ElseIf TypeOf value Is Double Then
                Return FormatDouble(CDbl(value), strDecimalSeparator, strThousandsSeparator, iNumDigits)
            ElseIf TypeOf value Is Decimal Then
                Return FormatDecimal(CDec(value), strDecimalSeparator, strThousandsSeparator, iNumDigits)
            End If
            Return FormatInteger(CInt(value), strDecimalSeparator, strThousandsSeparator)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts an integer value into a string using
        ''' the fixed EwE number format of decimal points, using custom decimal and
        ''' thousands separators.
        ''' </summary>
        ''' <param name="iValue">The integer to format into a string.</param>
        ''' <param name="strDecimalSeparator">Decimal separator to use. Default is 
        ''' a point.</param>
        ''' <param name="strThousandsSeparator">Thousands separator to use. By default
        ''' this separator is not used.</param>
        ''' <returns>A formatted value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FormatInteger(ByVal iValue As Integer,
                                             Optional ByVal strDecimalSeparator As String = ".",
                                             Optional ByVal strThousandsSeparator As String = "") As String

            Dim ci As CultureInfo = System.Globalization.CultureInfo.CurrentCulture
            Dim ni As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)

            ni.NumberDecimalSeparator = strDecimalSeparator
            ni.NumberGroupSeparator = strThousandsSeparator

            Return Convert.ToString(iValue, ni)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts a decimal value into a string with
        ''' a given number of releveant decimal digits, and custom decimal and
        ''' thousand separators.
        ''' <seealso cref="FormatSingle"/>
        ''' <seealso cref="FormatDouble"/>
        ''' <seealso cref="FormatNumber"/>
        ''' </summary>
        ''' <param name="decValue">The decimal to format into a string.</param>
        ''' <param name="strDecimalSeparator">Decimal separator to use. Default is 
        ''' a point.</param>
        ''' <param name="strThousandsSeparator">Thousands separator to use. By default
        ''' this separator is not used.</param>
        ''' <param name="iNumDigits">Number of decimal digits to use, or zero if
        ''' formatting should show as many digits as needed.</param>
        ''' <returns>A formatted value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FormatDecimal(ByVal decValue As Decimal,
                                             Optional ByVal strDecimalSeparator As String = ".",
                                             Optional ByVal strThousandsSeparator As String = "",
                                             Optional ByVal iNumDigits As Integer = -9999) As String

            Dim ci As CultureInfo = System.Globalization.CultureInfo.CurrentCulture
            Dim ni As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)

            ni.NumberDecimalSeparator = strDecimalSeparator
            ni.NumberGroupSeparator = strThousandsSeparator

            If (iNumDigits > 0) Then ni.NumberDecimalDigits = iNumDigits

            ' PLEASE DO NOT USE Convert.Format below!!! Convert.ToString will use ni.NumberDecimalDigits
            ' to determine the number of relevant digits (which is what we want) while Decimal.Format 
            ' rounds to ni.NumberDecimalDigits (which is what we DO NOT want)
            Return Convert.ToString(decValue, ni)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts a decimal value into a string with
        ''' a given number of releveant decimal digits, and custom decimal and
        ''' thousand separators.
        ''' <seealso cref="FormatDecimal"/>
        ''' <seealso cref="FormatDouble"/>
        ''' <seealso cref="FormatNumber"/>
        ''' </summary>
        ''' <param name="sValue">The single to format into a string.</param>
        ''' <param name="strDecimalSeparator">Decimal separator to use. Default is 
        ''' a point.</param>
        ''' <param name="strThousandsSeparator">Thousands separator to use. By default
        ''' this separator is not used.</param>
        ''' <param name="iNumDigits">Number of decimal digits to use, or zero if
        ''' formatting should show as many digits as needed.</param>
        ''' <returns>A formatted value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FormatSingle(ByVal sValue As Single,
                                            Optional ByVal strDecimalSeparator As String = ".",
                                            Optional ByVal strThousandsSeparator As String = "",
                                            Optional ByVal iNumDigits As Integer = -9999) As String

            Dim ci As CultureInfo = System.Globalization.CultureInfo.CurrentCulture
            Dim ni As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)

            ni.NumberDecimalSeparator = strDecimalSeparator
            ni.NumberGroupSeparator = strThousandsSeparator

            If (iNumDigits > 0) Then ni.NumberDecimalDigits = iNumDigits

            ' PLEASE DO NOT USE Convert.Format below!!! Convert.ToString will use ni.NumberDecimalDigits
            ' to determine the number of relevant digits (which is what we want) while Single.Format 
            ' rounds to ni.NumberDecimalDigits (which is what we DO NOT want)
            Return Convert.ToString(sValue, ni)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Generic conversion helper, converts a double value into a string with
        ''' a given number of releveant decimal digits, and custom decimal and
        ''' thousand separators.
        ''' <seealso cref="FormatDecimal"/>
        ''' <seealso cref="FormatSingle"/>
        ''' <seealso cref="FormatNumber"/>
        ''' </summary>
        ''' <param name="dValue">The double to format into a string.</param>
        ''' <param name="strDecimalSeparator">Decimal separator to use. Default is 
        ''' a point.</param>
        ''' <param name="strThousandsSeparator">Thousands separator to use. By default
        ''' this separator is not used.</param>
        ''' <param name="iNumDigits">Number of decimal digits to use, or zero if
        ''' formatting should show as many digits as needed.</param>
        ''' <seealso cref="cNumberUtils.NumRelevantDecimals"/>
        ''' <returns>A formatted value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function FormatDouble(ByVal dValue As Double,
                                            Optional ByVal strDecimalSeparator As String = ".",
                                            Optional ByVal strThousandsSeparator As String = "",
                                            Optional ByVal iNumDigits As Integer = -9999) As String

            Dim ci As CultureInfo = System.Globalization.CultureInfo.CurrentCulture
            Dim ni As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)

            ni.NumberDecimalSeparator = strDecimalSeparator
            ni.NumberGroupSeparator = strThousandsSeparator

            If (iNumDigits >= 0) Then ni.NumberDecimalDigits = iNumDigits

            ' PLEASE DO NOT USE Convert.Format below!!! Convert.ToString will use ni.NumberDecimalDigits
            ' to determine the number of relevant digits (which is what we want) while Double.Format 
            ' rounds to ni.NumberDecimalDigits (which is what we DO NOT want)
            Return Convert.ToString(dValue, ni)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a number to decimal degree notation (hours, minutes and seconds).
        ''' </summary>
        ''' <param name="dValue">The value to convert.</param>
        ''' <returns>The number in a decimal degree notation.</returns>
        ''' <remarks>
        ''' http://www.freevbcode.com/ShowCode.asp?ID=8179
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function FormatDegrees(ByVal dValue As Double,
                                             Optional strDegreeSymbol As String = "°",
                                             Optional strMinuteSymbol As String = "’",
                                             Optional strSeconds As String = """") As String
            dValue = Math.Abs(dValue)

            Dim dMinutes As Double = (dValue - Math.Truncate(dValue)) * 60
            Dim dSeconds As Double = (dMinutes - Math.Truncate(dMinutes)) * 60
            Dim sbResult As New StringBuilder()

            sbResult.Append(Math.Truncate(dValue).ToString())
            sbResult.Append(strDegreeSymbol)
            sbResult.Append(Math.Truncate(dMinutes).ToString())
            sbResult.Append(strMinuteSymbol)
            sbResult.Append(String.Format("{0:##.0000}", dSeconds))
            sbResult.Append(strSeconds)

            Return sbResult.ToString()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Format a date for persistent storage.
        ''' </summary>
        ''' <param name="dtValue">The date to format.</param>
        ''' <param name="strFormat">Optional date formatting flag (http://msdn.microsoft.com/en-us/library/zdtaw1bw%28v=vs.110%29.aspx)</param>
        ''' <returns>A formatted date.</returns>
        ''' <remarks>
        ''' http://www.w3.org/TR/NOTE-datetime
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function FormatDate(ByVal dtValue As DateTime,
                                          Optional ByVal strFormat As String = "yyyy-MM-dd") As String
            Return dtValue.ToString(strFormat)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Read a date from a formatted string.
        ''' </summary>
        ''' <param name="strDate">The date to read.</param>
        ''' <param name="strFormat">Optional date formatting flag (http://msdn.microsoft.com/en-us/library/zdtaw1bw%28v=vs.110%29.aspx)</param>
        ''' <returns>The date, of Date.MinValue if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ConvertToDate(ByVal strDate As String,
                                             Optional ByVal strFormat As String = "yyyy-MM-dd") As DateTime
            Dim dt As DateTime
            If (DateTime.TryParseExact(strDate, strFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, dt)) Then
                Return dt
            End If
            Return Date.MinValue
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Check if a string represents a valid email address.
        ''' </summary>
        ''' <param name="strEmail">Email address to validate</param>
        ''' <returns>True is valid, false if not valid</returns>
        ''' <remarks>
        ''' Uses regular expressions in this check, as it is a more thorough
        ''' way of checking an address provided.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function IsEmail(ByVal strEmail As String) As Boolean
            'regular expression pattern for valid email
            'addresses, allows for the following domains:
            'com,edu,info,gov,int,mil,net,org,biz,name,museum,coop,aero,pro,tv
            Dim strPattern As String = "^[-a-zA-Z0-9][-.a-zA-Z0-9]*@[-.a-zA-Z0-9]+(\.[-.a-zA-Z0-9]+)*\." & _
                                       "(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|xxx|[a-zA-Z]{2})$"
            Dim regexCheck As New Regex(strPattern, RegexOptions.IgnorePatternWhitespace)
            Dim bIsEmailAddress As Boolean = False

            If Not String.IsNullOrEmpty(strEmail) Then
                bIsEmailAddress = regexCheck.IsMatch(strEmail)
            End If

            Return bIsEmailAddress
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Check if a string represents a number.
        ''' </summary>
        ''' <param name="strValue">The string to check.</param>
        ''' <returns>True if the string could be parsed into a double value.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function IsNumber(ByVal strValue As String) As Boolean
            Dim dDummy As Double = 0
            Return Double.TryParse(strValue, dDummy)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a byte array to a string of hexadecimal numbers.
        ''' </summary>
        ''' <param name="bytes"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToHexString(ByVal bytes() As Byte) As String
            Dim sbHex As New StringBuilder()
            If (bytes IsNot Nothing) Then
                ' Convert public token to string
                For i As Integer = 0 To bytes.GetLength(0) - 1
                    sbHex.Append(String.Format("{0:x2}", bytes(i)))
                Next
            End If
            Return sbHex.ToString
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Convert a string into a base64 MD5 hash.
        ''' </summary>
        ''' <param name="strSrc">The string to hash.</param>
        ''' <returns>A base64 MD5 hash.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function GenerateHash(ByVal strSrc As String) As String
            ' Create an encoding object to ensure the encoding standard for the source text
            Dim enc As New UnicodeEncoding
            ' Retrieve a byte array based on the source text
            Dim abData() As Byte = enc.GetBytes(strSrc)
            ' Instantiate an MD5 Provider object
            Dim Md5 As New MD5CryptoServiceProvider()
            ' Compute the hash value from the source
            Dim abHash() As Byte = Md5.ComputeHash(abData)
            ' Return string representation
            Return Convert.ToBase64String(abHash)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' String truncation method, blatantly copied from 
        ''' http://www.codeproject.com/KB/vb/NewPathCompactPath.aspx
        ''' </summary>
        ''' <param name="strSrc">The string to truncate with path ellipses.</param>
        ''' <param name="iWidth">Allowed width of the string in pixels.</param>
        ''' <param name="ft">The font to measure the string with.</param>
        ''' <param name="tfFlags">Optional string format flags</param>
        ''' <returns>A truncated string.</returns>
        ''' <remarks>Note that this method does not modify the original string.</remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function CompactString(ByVal strSrc As String,
                                             ByVal iWidth As Integer,
                                             ByVal ft As Font,
                                             Optional ByVal tfFlags As TextFormatFlags = TextFormatFlags.SingleLine Or TextFormatFlags.PathEllipsis Or TextFormatFlags.ModifyString) As String

            If (String.IsNullOrWhiteSpace(strSrc)) Then Return ""

            Dim strResult As String = String.Copy(strSrc)
            TextRenderer.MeasureText(strResult, ft, New Size(iWidth, 0), tfFlags Or TextFormatFlags.ModifyString)
            Return strResult

        End Function

        Private Shared CSV_SEPARATORCHARS As Char() = New Char() {","c, " "c}

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Format a value for use in a CSV file.
        ''' </summary>
        ''' <param name="objValue">The value to format.</param>
        ''' <param name="cQuote">Optional quote character to use for wrapping the value.</param>
        ''' <param name="iNumDigits">Optional number of decimal digits to limit formatting to.</param>
        ''' <returns>A field fit for display in a CSV file.</returns>
        ''' <remarks>
        ''' <para>Numbers will be en-US formatted.</para>
        ''' <para>Double quotes will be removed.</para>
        ''' <para>Values containing potential CSV separator characters will be encapsulated in double quotes.</para>
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function ToCSVField(ByVal objValue As Object,
                                          Optional ByVal cQuote As Char = """"c,
                                          Optional iNumDigits As Integer = -9999) As String

            Dim strValue As String = ""

            If (objValue Is Nothing) Then Return strValue
            If (Convert.IsDBNull(objValue)) Then Return strValue

            If (TypeOf (objValue) Is String) Then
                strValue = CStr(objValue)
                If (cStringUtils.StrictCSVFormatting) Then
                    Dim sb As New StringBuilder()
                    For i As Integer = 0 To strValue.Length - 1
                        Dim c As Char = strValue(i)
                        If (Not Char.IsNumber(c)) And Not (Char.IsLetter(c)) And (Not c = "_"c) Then
                            sb.Append("_"c)
                        Else
                            sb.Append(c)
                        End If
                    Next
                    strValue = sb.ToString()
                End If
            ElseIf (TypeOf (objValue) Is DateTime) Then
                strValue = cStringUtils.FormatDate(DirectCast(objValue, DateTime))
            Else
                strValue = cStringUtils.FormatNumber(objValue, iNumDigits:=iNumDigits)
            End If

            If strValue.IndexOf(""""c) > 0 Then
                strValue = strValue.Replace("""", "")
            End If
            If strValue.IndexOfAny(CSV_SEPARATORCHARS) > 0 Then
                strValue = cQuote & strValue & cQuote
            End If

            Return strValue

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Converts an incoming string to UTF-8 encoding.
        ''' </summary>
        ''' <param name="strIn">The string to convert.</param>
        ''' <param name="encIn">The current encoding of <paramref name="strIn"/>.</param>
        ''' <returns>A UTF-8 encoded version of the string.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToUTF8(ByVal strIn As String,
                                      ByVal encIn As Encoding) As String
            ' Special cases
            strIn = strIn.Replace("²"c, "2"c)
            strIn = strIn.Replace("³"c, "3"c)
            ' Shazaam
            Dim data() As Byte = encIn.GetBytes(strIn)
            Return Encoding.UTF8.GetString(data)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Converts an incoming string to UTF-8 encoding, assuming that the
        ''' incoming string encoded as ASCII (.NET default).
        ''' </summary>
        ''' <param name="strIn">The string to convert.</param>
        ''' <returns>A UTF-8 encoded version of the string.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToUTF8(ByVal strIn As String) As String
            Return cStringUtils.ToUTF8(strIn, Encoding.ASCII)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a column number to an Excel-style column name. The resulting 
        ''' column name will always be upper case.
        ''' </summary>
        ''' <param name="iColumn">The one-based column number to convert.</param>
        ''' <returns>A character-based, Excel-style column name.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToExcelColumnName(iColumn As Integer) As String

            Debug.Assert(iColumn >= 1)

            Dim iDiv As Integer = iColumn
            Dim iMod As Integer
            Dim sb As New StringBuilder()

            While iDiv > 0
                iMod = (iDiv - 1) Mod 26
                sb.Insert(0, Convert.ToChar(65 + iMod))
                iDiv = CInt((iDiv - iMod) / 26)
            End While

            Return sb.ToString()

        End Function

        ''' <summary>Default string split delimiters, in order of decreasing relevance.</summary>
        Public Shared c_DELIMITERS As Char() = New Char() {Convert.ToChar(Keys.Tab), ";"c, Convert.ToChar(Keys.Space), ","c}

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the most likely delimiter character in a string.
        ''' </summary>
        ''' <param name="strIn">The string to explore.</param>
        ''' <param name="cQualifier">Qualifier character for enveloping non-splittable strings.</param>
        ''' <param name="candidates">An array of possible delimiter characters. If 
        ''' an empty array is provided or this parameter is omitted, the default 
        ''' array <see cref="c_DELIMITERS"/> is used.</param>
        ''' <returns>The most likely character used to split a string. If no
        ''' candidate can be found the default comma (,) is returned.</returns>
        ''' <remarks><para>This method splits <paramref name="strIn"/> by each 
        ''' delimiter character in <paramref name="candidates"/> in order. If a 
        ''' split returns more than one sub-string the split character is returned.
        ''' If no split was possible the default comma character is returned.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function FindStringDelimiter(ByVal strIn As String,
                                                   Optional ByVal cQualifier As Char = """"c,
                                                   Optional ByVal candidates As Char() = Nothing) As Char

            ' Ensure that there are candidate delimiters
            If (candidates Is Nothing) Then
                candidates = c_DELIMITERS
            End If

            If candidates.Length = 0 Then
                candidates = c_DELIMITERS
            End If

            ' Did receive any data to split? 
            ' NB: Do NOT use IsNullOrWhitespace here; all whitespace lines may contain valid split chars
            If Not String.IsNullOrEmpty(strIn) Then
                ' #Yes: find most relevant split character
                For Each c As Char In candidates
                    ' Does candidate occur in string?
                    If strIn.IndexOf(c) >= 0 Then
                        ' #Yes: Does split yield more than one substring?
                        If (cStringUtils.SplitQualified(strIn, c, cQualifier).Length > 1) Then
                            ' #Yes: return this character
                            Return c
                        End If
                    End If
                Next
            End If

            ' Return default
            Return ","c

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the current time as a string to be used in file names.
        ''' </summary>
        ''' <remarks>The time stamp is formatted as 'year-month-day hour-minute-second'.</remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function Now() As String
            Return cStringUtils.FormatDate(Date.Now, "yyyy-MM-dd HH-mm-ss")
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <para>Computes the Damerau-Levenshtein Distance between two strings. This method
        ''' Includes an optional threshold which can be used to indicate the maximum 
        ''' allowable distance between the two strings.</para>
        ''' <para>http://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance</para>
        ''' </summary>
        ''' <param name="strSrc">The first string to compare.</param>
        ''' <param name="strTarget">The second string to compare.</param>
        ''' <param name="iThreshold">Maximum allowable distance</param>
        ''' <returns>Integer.MaxValue if the threshhold is exceeded; otherwise the Damerau-Leveshteim 
        ''' distance between the strings.</returns>
        ''' <remarks>
        ''' Converted from a frigtheningly smart piece of code by http://stackoverflow.com/users/842685/jmh-gr
        ''' http://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings/9454016#9454016
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function DamerauLevenshteinDistance(ByVal strSrc As String,
                                                          ByVal strTarget As String,
                                                          Optional ByVal iThreshold As Integer = Integer.MaxValue) As Integer

            Dim length1 As Integer = strSrc.Length
            Dim length2 As Integer = strTarget.Length

            ' Return trivial case - difference in string lengths exceeds threshhold
            If (Math.Abs(length1 - length2) > iThreshold) Then Return Integer.MaxValue

            ' Ensure arrays [i] / length1 use shorter length 
            If (length1 > length2) Then
                Dim str As String = strTarget : strTarget = strSrc : strSrc = str
                Dim i As Integer = length1 : length1 = length2 : length2 = i
            End If

            Dim maxi As Integer = length1
            Dim maxj As Integer = length2

            Dim dCurrent(maxi + 1) As Integer
            Dim dMinus1(maxi + 1) As Integer
            Dim dMinus2(maxi + 1) As Integer
            Dim dSwap() As Integer = Nothing

            For i As Integer = 0 To maxi : dCurrent(i) = i : Next

            Dim jm1 As Integer = 0
            Dim im1 As Integer = 0
            Dim im2 As Integer = -1

            For j As Integer = 1 To maxj

                ' Rotate
                dSwap = dMinus2
                dMinus2 = dMinus1
                dMinus1 = dCurrent
                dCurrent = dSwap

                ' Initialize
                Dim minDistance As Integer = Integer.MaxValue
                dCurrent(0) = j
                im1 = 0
                im2 = -1

                For i As Integer = 1 To maxi

                    Dim cost As Integer = 1
                    If (strSrc(im1) = strTarget(jm1)) Then cost = 0

                    Dim del As Integer = dCurrent(im1) + 1
                    Dim ins As Integer = dMinus1(i) + 1
                    Dim [sub] As Integer = dMinus1(im1) + cost

                    Dim min As Integer = 0
                    If (del > ins) Then
                        If (ins > [sub]) Then
                            min = [sub]
                        Else
                            min = ins
                        End If
                    Else
                        If (del > [sub]) Then
                            min = [sub]
                        Else
                            min = del
                        End If
                    End If

                    If (i > 1 And j > 1) Then
                        If (strSrc(im2) = strTarget(jm1) And strSrc(im1) = strTarget(j - 2)) Then
                            min = Math.Min(min, dMinus2(im2) + cost)
                        End If
                    End If

                    dCurrent(i) = min
                    If (min < minDistance) Then minDistance = min
                    im1 += 1
                    im2 += 1
                Next i
                jm1 += 1
                If (minDistance > iThreshold) Then Return Integer.MaxValue
            Next j

            If dCurrent(maxi) > iThreshold Then Return Integer.MaxValue
            Return dCurrent(maxi)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Converts an exception with all its nested inner exceptions into a 
        ''' single string.
        ''' </summary>
        ''' <param name="ex"></param>
        ''' <returns>A string</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function UnravelException(ByVal ex As Exception) As String

            Dim sb As New StringBuilder()
            Dim trace As StackTrace = New StackTrace(ex, True)

            Try
                sb.AppendLine(ex.Message)
                Do While ex IsNot Nothing
                    sb.AppendLine(ex.Message)
                    ex = ex.InnerException
                Loop

                ' Stack trace
                For Each frame As StackFrame In trace.GetFrames
                    Dim strTrace As String = "At " & frame.GetMethod.Name & " (" & frame.GetFileLineNumber & ")"
                    Dim strName As String = System.IO.Path.GetFileName(frame.GetFileName)
                    If Not String.IsNullOrWhiteSpace(strName) Then strTrace &= " in " & strName
                    sb.AppendLine(strTrace)
                Next

            Catch exKaboom As Exception
                'oooppps
                Debug.Assert(False, exKaboom.Message)
            End Try
            Return sb.ToString

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Converts a string to proper title case, capitalizing every word in 
        ''' a sentence and turning all other characters to lower case. Note that
        ''' all-caps words are unaffected. This method takes text reading order 
        ''' and sentence breaks (periods) into account.
        ''' </summary>
        ''' <param name="strExpression">The string to convert.</param>
        ''' <returns>A string in proper title case.</returns>
        ''' <remarks>This version fixes some issues with <see cref="TextInfo.ToTitleCase"/>.
        ''' Note that substrings are split by ". " (left to right reading order) or 
        ''' " ." (l2r reading order). There is no support (yet) for custom
        ''' sentence seaprators.</remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function ToTitleCase(ByVal strExpression As String) As String

            Dim bR2L As Boolean = cSystemUtils.IsRightToLeft()
            Dim astrSentences() As String = Nothing
            Dim astrBits() As String = Nothing
            Dim sbOUt As New StringBuilder()

            If bR2L Then
                astrSentences = strExpression.Split(New String() {" ."}, System.StringSplitOptions.RemoveEmptyEntries)
            Else
                astrSentences = strExpression.Split(New String() {". "}, System.StringSplitOptions.RemoveEmptyEntries)
            End If

            For j As Integer = 0 To astrSentences.Length - 1

                ' Split sentence into words
                astrBits = astrSentences(j).Trim.Split(New String() {" "}, System.StringSplitOptions.RemoveEmptyEntries)

                ' Protect all words that are pure upper case. The rest will be turned to lower case
                For i As Integer = 0 To astrBits.Length - 1
                    If (String.Compare(astrBits(i), astrBits(i).ToUpper, False) <> 0) Then
                        astrBits(i) = astrBits(i).ToLower()
                    End If
                Next

                ' Combine sentences
                If (j > 0) Then
                    If bR2L Then
                        sbOUt.Append(" .")
                    Else
                        sbOUt.Append(". ")
                    End If
                End If

                ' Capitalize all words within a sentence
                For i As Integer = 0 To astrBits.Length - 1
                    astrBits(i) = astrBits(i).Trim
                    If Not String.IsNullOrWhiteSpace(astrBits(i)) Then
                        Dim c As Char() = astrBits(i).Trim.ToCharArray
                        If bR2L Then
                            c(c.Length - 1) = Char.ToUpper(c(c.Length - 1))
                        Else
                            c(0) = Char.ToUpper(c(0))
                        End If
                        If (i > 0) Then sbOUt.Append(" ")
                        sbOUt.Append(c)
                    End If
                Next

            Next

            Return sbOUt.ToString()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Converts a string to proper sentence case, capitalizing only the first
        ''' (or last, depending on reading order) word in a sentence and turning 
        ''' all other characters to lower case. 
        ''' Note that words with multiple caps are unaffected. Sentences are detected from periods.
        ''' </summary>
        ''' <param name="strExpression">The string to convert.</param>
        ''' <returns>A string in proper sentence case.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToSentenceCase(ByVal strExpression As String) As String

            Dim bR2L As Boolean = cSystemUtils.IsRightToLeft()
            Dim astrSentences() As String = Nothing
            Dim astrBits() As String = Nothing
            Dim sbOUt As New StringBuilder()

            If bR2L Then
                astrSentences = strExpression.Split(New String() {" ."}, System.StringSplitOptions.RemoveEmptyEntries)
            Else
                astrSentences = strExpression.Split(New String() {". "}, System.StringSplitOptions.RemoveEmptyEntries)
            End If

            For j As Integer = 0 To astrSentences.Length - 1

                ' Split sentence into words
                astrBits = astrSentences(j).Trim.Split(New String() {" "}, System.StringSplitOptions.RemoveEmptyEntries)

                ' Protect all words that are pure upper case. The rest will be turned to lower case
                For i As Integer = 0 To astrBits.Length - 1
                    Dim nCaps As Integer = 0
                    Dim nChars As Integer = 0
                    For Each c As Char In astrBits(i)
                        If Char.IsLetter(c) Then
                            If CStr(c) = CStr(c).ToUpper() Then nCaps += 1
                            nChars += 1
                        End If
                    Next
                    If (nCaps = 1) And (nChars > 1) Then astrBits(i) = astrBits(i).ToLower()
                Next

                ' Combine sentences
                If (j > 0) Then
                    If bR2L Then
                        sbOUt.Append(" .")
                    Else
                        sbOUt.Append(". ")
                    End If
                End If

                ' Capitalize only the first (or last) word of a sentence
                For i As Integer = 0 To astrBits.Length - 1
                    astrBits(i) = astrBits(i).Trim
                    If Not String.IsNullOrWhiteSpace(astrBits(i)) Then
                        Dim c As Char() = astrBits(i).Trim.ToCharArray
                        If bR2L Then
                            If (i = astrBits.Length - 1) Then
                                c(c.Length - 1) = Char.ToUpper(c(c.Length - 1))
                            End If
                        Else
                            If (i = 0) Then
                                c(0) = Char.ToUpper(c(0))
                            End If
                        End If
                        If (i > 0) Then sbOUt.Append(" ")
                        sbOUt.Append(c)
                    End If
                Next

            Next

            Return sbOUt.ToString()

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Convert text into a paragraph that wraps at <paramref name="iNumChars"/>.
        ''' </summary>
        ''' <param name="str">The string to split.</param>
        ''' <param name="iNumChars">The max number of characters on each text line.</param>
        ''' <returns>This is rather blunt logic but hey... Ideally, this should be 
        ''' outsourced to more dedicated logic that performs Locale aware hyphenation
        ''' etc. Perhaps NHunSpell? Ugh, that is for later.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function Wrap(ByVal str As String, Optional iNumChars As Integer = 100) As String

            If (String.IsNullOrWhiteSpace(str)) Then Return ""

            ' Clean up
            str = str.Replace(vbCrLf, " "c)
            str = str.Replace(vbCr, " "c)
            str = str.Replace(vbLf, " "c)
            While str.Contains("  ")
                str = str.Replace("  ", " ")
            End While

            Dim split As String() = str.Split(" "c)
            Dim sbLine As New System.Text.StringBuilder()
            Dim sbBlock As New System.Text.StringBuilder()

            For i As Integer = 0 To split.Length - 1
                Dim strTerm As String = split(i)
                If sbLine.Length >= iNumChars Then
                    If (sbBlock.Length > 0) Then sbBlock.Append(EwEUtils.Utilities.cStringUtils.vbCrLf)
                    sbBlock.Append(sbLine.ToString())
                    sbLine.Clear()
                ElseIf (sbLine.Length > 0) Then
                    sbLine.Append(" ")
                End If
                sbLine.Append(strTerm)
            Next

            If (sbBlock.Length > 0) Then sbBlock.Append(EwEUtils.Utilities.cStringUtils.vbCrLf)
            sbBlock.Append(sbLine.ToString())

            Return sbBlock.ToString()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Removes all single line breaks from a piece of text. Double line breaks
        ''' are preserved as they are interpreted as paragraph separators.
        ''' </summary>
        ''' <param name="strText"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function Unwrap(strText As String) As String

            Dim pars As String() = strText.Split(New String() {cStringUtils.vbCrLf & cStringUtils.vbCrLf, cStringUtils.vbCr & cStringUtils.vbCr, cStringUtils.vbLf & cStringUtils.vbLf}, StringSplitOptions.None)
            Dim sb As New StringBuilder()

            Dim i As Integer = 0
            For Each par As String In pars
                If (i > 0) Then
                    sb.AppendLine()
                    sb.AppendLine()
                End If
                For Each sentence As String In par.Split(New String() {cStringUtils.vbCrLf, cStringUtils.vbCr, cStringUtils.vbLf}, StringSplitOptions.None)
                    sb.Append(sentence)
                Next
                i += 1
            Next
            Return sb.ToString

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Concatenates a collection of items into a natural language list of terms.
        ''' </summary>
        ''' <param name="items">The textual items to concatenate.</param>
        ''' <param name="bAnd">States if the last item is added by 'and' or by 'or'</param>
        ''' <returns></returns>
        ''' <example>
        '''     Dim items As New String() {"Ecopath", "Ecosim", "Ecospace"}
        '''     FormatList(items, False) 
        '''     
        ''' ' Output: "Ecopath, Ecosim or Ecospace"
        ''' </example>
        ''' -------------------------------------------------------------------
        Public Shared Function FormatList(items As IEnumerable(Of String), Optional bAnd As Boolean = True) As String

            Dim sb As New StringBuilder()
            Dim n As Integer = 0

            For Each item As String In items
                If (Not String.IsNullOrWhiteSpace(item)) Then n += 1
            Next

            If (n > 0) Then
                Dim t As Integer = 0
                For Each item As String In items
                    If (Not String.IsNullOrWhiteSpace(item)) Then
                        t += 1
                        If cSystemUtils.IsRightToLeft Then
                            If (t > 1) Then sb.Insert(0, If(t < n, " ,", " " & If(bAnd, My.Resources.SEP_AND, My.Resources.SEP_OR) & " "))
                            sb.Insert(0, item)
                        Else
                            If (t > 1) Then sb.Append(If(t < n, ", ", " " & If(bAnd, My.Resources.SEP_AND, My.Resources.SEP_OR) & " "))
                            sb.Append(item)
                        End If
                    End If
                Next
            End If
            Return sb.ToString()

        End Function

#Region " Date parsing "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Try to locate and parse a numerical date from a string. The data can be
        ''' specified as YYYY{sep}MM or MM{sep}YYYY.
        ''' </summary>
        ''' <param name="strText">The text to parse.</param>
        ''' <param name="dt">The parsed date, if any.</param>
        ''' <returns>True if either date format was found.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetDate(strText As String, ByRef dt As DateTime) As Boolean

            Dim y, m As Integer
            Dim bSuccess As Boolean = False
            Dim exp1 As New Regex("[^a-z0-9+]\d{4}.\d{2}[^a-z0-9+]")
            Dim res1 As Match = exp1.Match(strText)
            Dim exp2 As New Regex("[^a-z0-9+]\d{2}.\d{4}[^a-z0-9+]")
            Dim res2 As Match = exp2.Match(strText)

            If (res1.Length = 9) Then
                bSuccess = Integer.TryParse(res1.Value.Substring(1, 4), y) And Integer.TryParse(res1.Value.Substring(6, 2), m)
            ElseIf (res2.Length = 9) Then
                bSuccess = Integer.TryParse(res2.Value.Substring(1, 2), m) And Integer.TryParse(res2.Value.Substring(4, 4), y)
            End If

            If bSuccess Then
                dt = New DateTime(y, m, 1)
            End If
            Return bSuccess

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Use a brute force strategy to try to locate and parse a numerical date 
        ''' from a string. This method tries several orders of year, month and
        ''' optional day fields, with different separators, to find a match. 
        ''' </summary>
        ''' <param name="strText">The text to parse.</param>
        ''' <param name="dt">The parsed date, if any.</param>
        ''' <param name="strFormat">The format mask that returned a result, if any.</param>
        ''' <param name="iMatchPos">The index in the string where the result was found.</param>
        ''' <returns>True if either date format was found.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetDateBruteForce(ByVal strText As String,
                                                 ByRef dt As DateTime,
                                                 Optional ByRef strFormat As String = "",
                                                 Optional ByRef iMatchPos As Integer = -1) As Boolean

            ' Possible date formats, with and without day specified. {0}:separator, {1}:month, {2}:day
            Dim fmts() As String = New String() {"yyyy{0}{1}", "{1}{0}yyyy", "yyyy{0}{1}{0}{2}", "{2}{0}{1}{0}yyyy", "{1}{0}{2}{0}yyyy"}
            ' Possible separator characters
            Dim seps() As String = New String() {"", "-", "\", "/", ".", " "}
            ' Possible month specifiers (one or two digits only)
            Dim months() As String = New String() {"MM", "M"}
            ' Possible day specifiers (one or two digits only)
            Dim days() As String = New String() {"dd", "d"}

            Dim x As Integer = strText.Length
            Dim i As Integer = Math.Max(iMatchPos, 0)
            Dim bDone As Boolean = (i >= x)

            While Not bDone
                ' Found a digit?
                If Char.IsDigit(strText(i)) Then
                    ' No format prescribed?
                    If String.IsNullOrWhiteSpace(strFormat) Then
                        ' #Yes: for all possible formats
                        For Each fmt As String In fmts
                            ' Remember if this format needs a day specifier
                            Dim bIncludeDay As Boolean = fmt.Contains("{2}")
                            ' For all possible separators
                            For Each sep As String In seps
                                ' For all possible month formats
                                For Each month As String In months
                                    ' For all possible day formats
                                    For Each day As String In days
                                        ' Build format mask
                                        Dim mask As String = ""
                                        If bIncludeDay Then
                                            mask = String.Format(fmt, sep, month, day)
                                        Else
                                            mask = String.Format(fmt, sep, month)
                                        End If
                                        Dim len As Integer = mask.Length
                                        ' Enough characters left to try this mask?
                                        If (i + len < x) Then
                                            ' #Yes: extract substring and try to parse 
                                            Dim part As String = strText.Substring(i, len)
                                            ' Parse ok?
                                            If DateTime.TryParseExact(part, mask, Nothing, Globalization.DateTimeStyles.None, dt) Then
                                                ' #Yes: expose successful format and position where this format was found
                                                strFormat = mask
                                                iMatchPos = i
                                                ' Done
                                                Return True
                                            End If
                                        End If
                                    Next ' Days
                                Next ' Months
                            Next ' Separators
                        Next ' Formats
                    Else
                        ' Use prescribed format
                        Dim len As Integer = strFormat.Length
                        ' Enough characters left to try this mask?
                        If (i + len < x) Then
                            ' #Yes: extract substring and try to parse 
                            Dim part As String = strText.Substring(i, len)
                            If DateTime.TryParseExact(part, strFormat, Nothing, Globalization.DateTimeStyles.None, dt) Then
                                ' OK. No need to return the format mask ;)
                                Return True
                            End If
                        End If
                    End If

                    ' Skip further digits
                    While (i < x - 1 And Char.IsDigit(strText(i)))
                        i += 1
                    End While
                End If

                ' Next!
                i += 1
                bDone = (i >= x)

            End While

            ' No luck
            Return False

        End Function

#End Region ' Date parsing

#Region " Replace "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Replace all occurrences of a pattern in a source string with a replacement.
        ''' </summary>
        ''' <param name="strSrc">Source string the replace all instances into.</param>
        ''' <param name="strPattern">The search pattern to replace.</param>
        ''' <param name="strReplacement">The search pattern replacement string.</param>
        ''' <returns>An amphetamine-addicted monk seal.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function ReplaceAll(ByVal strSrc As String,
                                          ByVal strPattern As String,
                                          ByVal strReplacement As String) As String

            ' Rerouted
            Return cStringUtils.Replace(strSrc, strPattern, strReplacement, StringComparison.CurrentCulture)

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <see cref="[String].Replace"/> alternative that offers comparison
        ''' options. This method is significantly faster than RegEx equivalents.
        ''' Implementation adapted from http://www.codeproject.com/Articles/10890/Fastest-C-Case-Insenstive-String-Replace.
        ''' </summary>
        ''' <param name="strSrc">Source string the replace all instances into.</param>
        ''' <param name="strPattern">The search pattern to replace.</param>
        ''' <param name="strReplacement">The search pattern replacement string.</param>
        ''' <param name="comparisonType">The <see cref="StringComparison"/> option to use.</param>
        ''' <returns>A string.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function Replace(ByVal strSrc As String, ByVal strPattern As String,
                                       ByVal strReplacement As String, ByVal comparisonType As StringComparison) As String

            If String.IsNullOrWhiteSpace(strSrc) Then Return String.Empty

            Dim posCurrent As Integer = 0
            Dim lenPattern As Integer = strPattern.Length
            Dim idxNext As Integer = strSrc.IndexOf(strPattern, comparisonType)
            Dim result As New StringBuilder()

            While idxNext >= 0
                result.Append(strSrc, posCurrent, idxNext - posCurrent)
                result.Append(strReplacement)

                posCurrent = idxNext + lenPattern

                idxNext = strSrc.IndexOf(strPattern, posCurrent, comparisonType)
            End While

            result.Append(strSrc, posCurrent, strSrc.Length - posCurrent)

            Return result.ToString()

        End Function

#End Region ' Replace

#Region " Map array conversions "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Convert a 2-dimensional (map) array to a string for database storage.
        ''' </summary>
        ''' <param name="data">Data to write to the string.</param>
        ''' <param name="dataDepth">Optional depth data mask to apply. If provided, only 
        ''' water cells or land cells are stored based on the value of <paramref name="bWaterOnly"/>.</param>
        ''' <param name="bWaterOnly">Flag, stating whether only values should be written
        ''' for water cells (true) or land cells (false), as indicated by parameter <paramref name="dataDepth"/>.</param>
        ''' <param name="valueFilter">Value to find in the data and to write to the string,
        ''' or Nothing if any value from the data must be written to the string.</param>
        ''' <param name="valueSet">Value to set, if any.</param>
        ''' <returns>The resulting converted string.</returns>
        ''' <remarks>This code is optimized to include as few characters as possible
        ''' in the output string without having to revert to run-length encoding.
        ''' Rows without any values will be left empty and are only marked by a semi-colon.</remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function ArrayToString(ByVal data As Array,
                                             ByVal InRow As Integer,
                                             ByVal InCol As Integer,
                                             Optional ByVal dataDepth As Single(,) = Nothing,
                                             Optional ByVal bWaterOnly As Boolean = True,
                                             Optional ByVal valueFilter As Object = Nothing,
                                             Optional ByVal valueSet As Object = Nothing) As String

            ' Can only handle 2-dimensional arrays
            Debug.Assert(data.Rank = 2)

            Dim sb As New StringBuilder()
            Dim sbRow As New StringBuilder()
            Dim bUseCell As Boolean = False
            Dim bHasRowValues As Boolean = False
            Dim value As Object = Nothing
            Dim tData As Type = data.GetType().GetElementType

            ' For all rows
            For i As Integer = 1 To InRow

                ' Start of new rowg
                bHasRowValues = False
                sbRow.Length = 0
                bUseCell = False

                ' For all cols
                For j As Integer = 1 To InCol

                    ' Append separator after last value
                    If bUseCell Then sbRow.Append(","c)

                    ' Ignore land filter?
                    If (dataDepth Is Nothing) Then
                        ' #Yes: use cell
                        bUseCell = True
                    Else
                        ' #No: only use cell if land or water (depeding on bWaterOnly)
                        bUseCell = If(dataDepth(i, j) > 0, bWaterOnly, Not bWaterOnly)
                    End If

                    If (bUseCell) Then
                        ' Get value
                        value = data.GetValue(i, j)
                        ' Append value in correct type 
                        If tData Is GetType(Boolean) Then
                            ' #Boolean values are stored as 1 (true) and 0 (false)
                            sbRow.Append(If(CBool(value), "1", "0"))
                            bHasRowValues = bHasRowValues Or (CBool(value))
                        Else
                            ' Is an allowed value?
                            If ((value.Equals(valueFilter) Or (valueFilter Is Nothing))) Then
                                ' #Yes: convert value to a fixed en-US representation text
                                If (valueSet IsNot Nothing) Then value = valueSet
                                sbRow.Append(cStringUtils.FormatNumber(value))
                                bHasRowValues = True
                            End If
                        End If
                    End If
                Next j

                ' Add row if not empty
                If bHasRowValues Then sb.Append(sbRow.ToString)
                ' Add row delimiter
                sb.Append(";"c)

            Next i

            ' Done
            Return sb.ToString

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Read a map from a string, and poulate a 2-dimensional array with this data.
        ''' </summary>
        ''' <param name="strData">The string containing the map.</param>
        ''' <param name="data">The 2-dimensional array to populate.</param>
        ''' <param name="InRow">Number of rows in the map.</param>
        ''' <param name="InCol">Number of columns in the map.</param>
        ''' <param name="land">Optional land layer to use.</param>
        ''' <param name="bWaterOnly">States whether only water cells (true) or land cells (false) should be written.</param>
        ''' <param name="valueFilter">Optional value to filter map values by. If specified, only map values equalling this
        ''' filter value will be copied to the data array.</param>
        ''' <param name="valueSet">Value to set, if any.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function StringToArray(ByVal strData As String, ByVal data As Array,
                                             ByVal InRow As Integer, ByVal InCol As Integer,
                                             Optional ByVal land As Single(,) = Nothing,
                                             Optional ByVal bWaterOnly As Boolean = True,
                                             Optional ByVal valueFilter As Object = Nothing,
                                             Optional ByVal valueSet As Object = Nothing) As Boolean

            ' Need 2 dim array
            Debug.Assert(data.Rank = 2)

            Dim lines As String() = strData.Replace("""", "").Split(";"c)
            Dim values As String() = Nothing
            Dim iColumn As Integer = 0
            Dim bUseCell As Boolean = False
            Dim value As Object = Nothing
            Dim tData As Type = data.GetType().GetElementType

            ' For all rows
            For i As Integer = 1 To InRow
                ' Still row data left?
                If (i < lines.Length) Then

                    ' #Yes: split row into values
                    values = lines(i - 1).Split(","c)
                    ' For all cols
                    For j As Integer = 1 To InCol
                        ' Ignore land filter?
                        If (land Is Nothing) Then
                            ' #Yes: use cell
                            bUseCell = True
                        Else
                            ' #No: only use cell if land or water (depeding on bWaterOnly)
                            bUseCell = If(land(i, j) > 0, bWaterOnly, Not bWaterOnly)
                        End If

                        ' Use cell and there is cell data?
                        If bUseCell And (iColumn < values.Length) Then
                            ' #Yes: is there really, really cell data?
                            If Not String.IsNullOrEmpty(values(iColumn)) Then
                                Try
                                    ' #Yes: get value
                                    If tData Is GetType(Boolean) Then
                                        value = (values(iColumn) = "1")
                                    Else
                                        value = cStringUtils.ConvertToNumber(values(iColumn), tData)
                                    End If

                                    ' Does this value match the value to get if provided?
                                    If (value.Equals(valueFilter) Or (valueFilter Is Nothing)) Then
                                        ' #Yes: update array
                                        If (valueSet IsNot Nothing) Then value = valueSet
                                        data.SetValue(value, i, j)
                                    End If
                                Catch ex As Exception
                                    Return False
                                End Try
                            End If
                            ' Next column
                            iColumn += 1
                        End If
                    Next j
                    ' Reset column count
                    iColumn = 0
                End If
            Next i

            ' Done
            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enumerated type, stating where to find the data filter in a 3D map array.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eFilterIndexTypes As Integer
            FirstIndex = 0
            LastIndex
        End Enum

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Convert a 3-dimensional (map) array to a string for database storage.
        ''' </summary>
        ''' <param name="data">Data to write to the string.</param>
        ''' <param name="dataDepth">Optional depth data mask to apply. If provided, only 
        ''' water cells or land cells are stored based on the value of <paramref name="bWaterOnly"/>.</param>
        ''' <param name="bWaterOnly">Flag, stating whether only values should be written
        ''' for water cells (true) or land cells (false), as indicated by parameter <paramref name="dataDepth"/>.</param>
        ''' <param name="valueSet">Value to find in the data and to write to the string,
        ''' or Nothing if any value from the data must be written to the string.</param>
        ''' <returns>The resulting converted string.</returns>
        ''' <remarks>This code is optimized to include as few characters as possible
        ''' in the output string without having to revert to run-length encoding.
        ''' Rows without any values will be left empty and are only marked by a semi-colon.</remarks>
        ''' -----------------------------------------------------------------------
        Public Shared Function ArrayToString(ByVal data As Array,
                                             ByVal iFilter As Integer,
                                             ByVal filterIndex As eFilterIndexTypes,
                                             ByVal InRow As Integer,
                                             ByVal InCol As Integer,
                                             Optional ByVal dataDepth As Single(,) = Nothing,
                                             Optional ByVal bWaterOnly As Boolean = True,
                                             Optional ByVal valueSet As Object = Nothing) As String

            ' Need 3 dim array
            Debug.Assert(data.Rank = 3)

            Dim sb As New StringBuilder()
            Dim sbRow As New StringBuilder()
            Dim bHasRowValues As Boolean = False
            Dim bUseCell As Boolean = False
            Dim value As Object = Nothing
            Dim tData As Type = data.GetType().GetElementType

            Select Case filterIndex
                Case eFilterIndexTypes.FirstIndex
                    InRow = Math.Min(InRow, data.GetUpperBound(1))
                    InCol = Math.Min(InCol, data.GetUpperBound(2))
                Case eFilterIndexTypes.LastIndex
                    InRow = Math.Min(InRow, data.GetUpperBound(0))
                    InCol = Math.Min(InCol, data.GetUpperBound(1))
            End Select

            ' For all rows
            For i As Integer = 1 To InRow

                ' Start new line
                bHasRowValues = False
                sbRow.Length = 0
                bUseCell = False

                ' For all cols
                For j As Integer = 1 To InCol

                    ' Append separator if already has values on this row
                    If bUseCell Then sbRow.Append(","c)

                    ' Ignore land filter?
                    If (dataDepth Is Nothing) Then
                        ' #Yes: use cell
                        bUseCell = True
                    Else
                        ' #No: only use cell if land or water (depeding on bWaterOnly)
                        bUseCell = If(dataDepth(i, j) > 0, bWaterOnly, Not bWaterOnly)
                    End If

                    If bUseCell Then

                        ' Get actual cell value
                        Select Case filterIndex
                            Case eFilterIndexTypes.FirstIndex : value = data.GetValue(iFilter, i, j)
                            Case eFilterIndexTypes.LastIndex : value = data.GetValue(i, j, iFilter)
                        End Select


                        If tData Is GetType(Boolean) Then
                            sbRow.Append(If(CBool(value), "1", "0"))
                            bHasRowValues = bHasRowValues Or (CBool(value))
                        Else
                            ' Is not an allowed value?
                            If ((value.Equals(valueSet) Or (valueSet Is Nothing))) Then
                                sbRow.Append(cStringUtils.FormatNumber(value))
                                bHasRowValues = True
                            End If
                        End If
                    End If
                Next j

                If bHasRowValues Then sb.Append(sbRow.ToString())
                sb.Append(";"c)
            Next i

            ' Done
            Return sb.ToString

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Read a map from a string, and poulate a 3-dimensional array with this data.
        ''' </summary>
        ''' <param name="strData">The string containing the map.</param>
        ''' <param name="iFilter">Index of third dimension to set.</param>
        ''' <param name="filterIndex">Position of third dimension in the array (first, e.g. (#,,) or last (,,#))</param>
        ''' <param name="data">The 2-dimensional array to populate.</param>
        ''' <param name="land">Optional land layer to use.</param>
        ''' <param name="bWaterOnly">States whether only water cells (true) or land cells (false) should be written.</param>
        ''' <param name="valueGet">Optional value to filter map values by. If specified, only map values equalling this
        ''' filter value will be copied to the data array.</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Public Shared Function StringToArray(ByVal strData As String,
                                            ByVal iFilter As Integer,
                                            ByVal filterIndex As eFilterIndexTypes,
                                            ByVal data As Array,
                                            ByVal InRow As Integer,
                                            ByVal InCol As Integer,
                                            Optional ByVal land As Single(,) = Nothing,
                                            Optional ByVal bWaterOnly As Boolean = True,
                                            Optional ByVal valueGet As Object = Nothing,
                                            Optional ByVal sMax As Single = Single.MaxValue) As Boolean

            ' Need 3 dim array
            Debug.Assert(data.Rank = 3)

            Dim astrLines As String() = strData.Replace("""", "").Split(";"c)
            Dim astrValues As String() = Nothing
            Dim iColumn As Integer = 0
            Dim value As Object = Nothing
            Dim tData As Type = data.GetType().GetElementType
            Dim bUseValue As Boolean = False

            Select Case filterIndex
                Case eFilterIndexTypes.FirstIndex
                    InRow = Math.Min(InRow, data.GetUpperBound(1))
                    InCol = Math.Min(InCol, data.GetUpperBound(2))
                Case eFilterIndexTypes.LastIndex
                    InRow = Math.Min(InRow, data.GetUpperBound(0))
                    InCol = Math.Min(InCol, data.GetUpperBound(1))
            End Select

            ' For all rows
            For i As Integer = 1 To InRow
                ' Still row data left?
                If (i < astrLines.Length) Then

                    ' #Yes: split row into values
                    astrValues = astrLines(i - 1).Split(","c)
                    ' For all cols
                    For j As Integer = 1 To InCol
                        ' Ignore land filter?
                        If (land Is Nothing) Then
                            ' #Yes: use cell
                            bUseValue = True
                        Else
                            ' #No: only use cell if land or water (depeding on bWaterOnly)
                            bUseValue = If(land(i, j) > 0, bWaterOnly, Not bWaterOnly)
                        End If

                        ' Use cell and there is cell data?
                        If bUseValue And (iColumn < astrValues.Length) Then
                            ' #Yes: is there really, really cell data?
                            If Not String.IsNullOrEmpty(astrValues(iColumn)) Then
                                Try
                                    ' #Yes: get value
                                    If tData Is GetType(Boolean) Then
                                        value = (astrValues(iColumn) = "1")
                                    Else
                                        value = Math.Min(sMax, CSng(cStringUtils.ConvertToNumber(astrValues(iColumn), tData)))
                                    End If
                                    ' Does this value match the value to get if provided?
                                    If (value.Equals(valueGet) Or (valueGet Is Nothing)) Then
                                        ' #Yes: update array
                                        Select Case filterIndex
                                            Case eFilterIndexTypes.FirstIndex : data.SetValue(value, iFilter, i, j)
                                            Case eFilterIndexTypes.LastIndex : data.SetValue(value, i, j, iFilter)
                                        End Select
                                    End If
                                Catch ex As Exception

                                End Try
                            End If
                            ' Next column
                            iColumn += 1
                        End If
                    Next j
                    ' Reset column count
                    iColumn = 0
                End If
            Next i

            ' Done
            Return True

        End Function

#End Region ' Map array conversions

#Region " ParamArray conversions "

        Public Shared Function ParamArrayToString(asValues As Single(), Optional nParams As Integer = Integer.MaxValue) As String

            If (asValues Is Nothing) Then Return ""

            Dim sb As New StringBuilder()
            For i As Integer = 0 To Math.Min(asValues.Length, nParams) - 1
                If (i > 0) Then sb.Append(" ")
                sb.Append(cStringUtils.FormatSingle(asValues(i)))
            Next
            Return sb.ToString()

        End Function

        Public Shared Function StringToParamArray(strValues As String) As Single()

            Dim bits As String() = strValues.Split(" "c)
            Dim lVals As New List(Of Single)
            For i As Integer = 0 To bits.Length - 1
                lVals.Add(cStringUtils.ConvertToSingle(bits(i)))
            Next
            Return lVals.ToArray()

        End Function

#End Region ' ParamArray conversions

#Region " Shape data conversions "

        Public Shared Function StringToShape(ByVal strMemo As String,
                                             ByVal nItems As Integer,
                                             ByVal sDefault As Single,
                                             ByVal sData As Single(,),
                                             ByVal iIndex As Integer) As Boolean

            Dim astrBits As String() = Nothing
            Dim iPts As Integer = 1

            If (Not String.IsNullOrWhiteSpace(strMemo)) Then
                astrBits = strMemo.Trim.Split(CChar(" "))
                iPts = astrBits.Length
                For j As Integer = 1 To Math.Min(nItems, iPts)
                    sData(iIndex, j) = cStringUtils.ConvertToSingle(astrBits(j - 1), sDefault)
                Next
            Else
                sData(iIndex, iPts) = sDefault
            End If

            For j As Integer = iPts + 1 To nItems
                sData(iIndex, j) = sData(iIndex, iPts)
            Next
            Return True

        End Function

#End Region ' Shape data conversions

#Region " Microsoft.VisualBasic alternatives "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a former vbCharNull.
        ''' </summary>
        ''' <remarks>
        ''' The Microsoft.VisualBasic assembly is known to cause problems under Mono.
        ''' For Mono compliance this definition should be used instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared ReadOnly Property vbCharNull As Char
            Get
                Return Convert.ToChar(0)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a Tab character.
        ''' </summary>
        ''' <remarks>
        ''' The Microsoft.VisualBasic assembly is known to cause problems under Mono.
        ''' For Mono compliance this definition should be used instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared ReadOnly Property vbTab As String
            Get
                Return Convert.ToChar(9).ToString
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a Newline character.
        ''' </summary>
        ''' <remarks>
        ''' The Microsoft.VisualBasic assembly is known to cause problems under Mono.
        ''' For Mono compliance this definition should be used instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared ReadOnly Property vbNewline As String
            Get
                Return cStringUtils.vbCr
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a carriage return character.
        ''' </summary>
        ''' <remarks>
        ''' The Microsoft.VisualBasic assembly is known to cause problems under Mono.
        ''' For Mono compliance this definition should be used instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared ReadOnly Property vbCr As String
            Get
                Return Convert.ToChar(13).ToString
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a line feed character.
        ''' </summary>
        ''' <remarks>
        ''' The Microsoft.VisualBasic assembly is known to cause problems under Mono.
        ''' For Mono compliance this definition should be used instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared ReadOnly Property vbLf As String
            Get
                Return Convert.ToChar(10).ToString
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a carriage return + line feed character.
        ''' </summary>
        ''' <remarks>
        ''' The Microsoft.VisualBasic assembly is known to cause problems under Mono.
        ''' For Mono compliance this definition should be used instead.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared ReadOnly Property vbCrLf As String
            Get
                Return Environment.NewLine
            End Get
        End Property

#End Region ' Microsoft.VisualBasic alternatives

#Region " Localization "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' <para>Safe string formatting to substitute value(s) into a <see cref="String.Format">string format 
        ''' mask</see>.</para>
        ''' <para>If the formatting fails, the original mask is returned and the 
        ''' failure is recorded in the EwE error log.</para>
        ''' </summary>
        ''' <param name="strMask">The string mask to use for formatting, containing
        ''' only one placeholder {0}.</param>
        ''' <param name="vals">The value(s) to substitute into the mask.</param>
        ''' <returns>The formatted string, or the original mask if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function Localize(ByVal strMask As String, ByVal ParamArray vals As Object()) As String
            Try
                Return String.Format(strMask, vals)
            Catch ex As Exception
                Debug.Assert(False, "Localization error on " & strMask)
                cLog.Write(ex, "Localization error on " & strMask)
            End Try
            Return strMask
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' <para>Safe string formatting to substitute value(s) into a <see cref="String.Format">string format 
        ''' mask</see>.</para>
        ''' <para>If the formatting fails, the original mask is returned and the 
        ''' failure is recorded in the EwE error log.</para>
        ''' </summary>
        ''' <param name="strMask">The string mask to use for formatting, containing
        ''' only one placeholder {0}.</param>
        ''' <param name="vals">The value(s) to substitute into the mask.</param>
        ''' <returns>The formatted string, or the original mask if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function LocalizeSentence(ByVal strMask As String, ByVal ParamArray vals As Object()) As String
            Try
                Return cStringUtils.ToSentenceCase(String.Format(strMask, vals))
            Catch ex As Exception
                Debug.Assert(False, "Localization error on " & strMask)
                cLog.Write(ex, "Localization error on " & strMask)
            End Try
            Return strMask
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Interprets a text pattern to describe a range of integer values.
        ''' </summary>
        ''' <param name="strValue">The text pattern to analyze.</param>
        ''' <returns>An array of integer values.</returns>
        ''' <remarks>
        ''' <para>Supported value expressions are:</para>
        ''' <list>
        ''' <item><term>A-B</term><description>Range from A to B</description></item>
        ''' <item><term>A-B@C</term><description>Range from A to B with step size C</description></item>
        ''' </list>
        ''' <para>If value B equals * then B is set to the value of <paramref name="iMax"/></para>.
        ''' <para>Expressions are separated by commas.</para>
        ''' <para>If <paramref name="strValue"> is left empty and empty array is returned.</paramref>.</para>
        ''' <para>If <paramref name="strValue"> equals * then all values from 1 to iMax are returned.</paramref>.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function Range(ByVal strValue As String,
                                     Optional iMax As Integer = -1) As Integer()

            ' RegEx are too confusing for users.

            Dim lValues As New List(Of Integer)
            Dim temp As String() = strValue.Split("-"c)
            Dim iFrom As Integer = -9999
            Dim iTo As Integer = -9999
            Dim iIncr As Integer = 1
            Dim bSuccess As Boolean = True

            ' Special case
            If (strValue.Trim = "*") Then
                For i As Integer = 1 To iMax
                    lValues.Add(i)
                Next
                Return lValues.ToArray()
            End If

            For i As Integer = 0 To temp.Length - 1
                Dim nums As String() = temp(i).Split(","c)
                If (iFrom <> -9999) Then
                    If (nums(0).Contains("@")) Then
                        Dim its As String() = nums(0).Split("@"c)
                        bSuccess = bSuccess And Integer.TryParse(its(0), iTo)
                        bSuccess = bSuccess And Integer.TryParse(its(1), iIncr)
                    Else
                        If (nums(0).Trim = "*") Then
                            iTo = iMax
                        Else
                            bSuccess = bSuccess And Integer.TryParse(nums(0), iTo)
                        End If
                        iIncr = 1
                    End If
                    For j As Integer = iFrom To iTo - 1 Step iIncr
                        If (Not lValues.Contains(j)) Then
                            lValues.Add(j)
                        End If
                    Next
                End If

                For j As Integer = If(iFrom <> -9999, 1, 0) To nums.Length - 1
                    bSuccess = bSuccess And Integer.TryParse(nums(j), iFrom)
                    If (Not lValues.Contains(iFrom)) Then
                        lValues.Add(iFrom)
                    End If
                Next
            Next

            If Not bSuccess Then lValues.Clear()

            lValues.Sort()
            Return lValues.ToArray()

        End Function

#End Region ' Localization

#Region " Encryption "

        ' From http://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file

        ''' <summary>Let's generate some controlled noise.</summary>
        Private Shared entropy As Byte() = System.Text.Encoding.Unicode.GetBytes("Secret Is Not A Password")

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Encrypt a <see cref="SecureString">secure string</see> for persistent storage.
        ''' </summary>
        ''' <param name="strsIn">The <see cref="SecureString">secure string</see> to encrypt.</param>
        ''' <returns>A <see cref="String">regular string</see>.</returns>
        ''' <remarks><example>AppSettings.Password = EncryptString(ToSecureString(PasswordTextBox.Password));</example></remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function EncryptString(strsIn As SecureString) As String
            Dim data As Byte() = ProtectedData.Protect(Encoding.Unicode.GetBytes(ToInsecureString(strsIn)), entropy, DataProtectionScope.CurrentUser)
            Return Convert.ToBase64String(data)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Decrypt a <see cref="SecureString">secure string</see> from persistent storage.
        ''' </summary>
        ''' <param name="strsIn">The <see cref="SecureString">secure string</see> to decrypt.</param>
        ''' <returns>A <see cref="SecureString">secure string</see>.</returns>
        ''' <remarks><example>SecureString password = DecryptString(AppSettings.Password);</example></remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function DecryptString(strsIn As String) As SecureString
            Try
                Dim data As Byte() = ProtectedData.Unprotect(Convert.FromBase64String(strsIn), entropy, DataProtectionScope.CurrentUser)
                Return ToSecureString(System.Text.Encoding.Unicode.GetString(data))
            Catch
                Return New SecureString()
            End Try
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a <see cref="String">regular string</see> to a <see cref="SecureString">secure string</see>.
        ''' </summary>
        ''' <param name="strIn">The string to convert.</param>
        ''' <returns>A <see cref="SecureString">secure string</see>.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToSecureString(strIn As String) As SecureString
            Dim strsOut As New SecureString()
            For Each c As Char In strIn
                strsOut.AppendChar(c)
            Next
            strsOut.MakeReadOnly()
            Return strsOut
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a <see cref="SecureString">secure string</see> to a <see cref="String">regular string</see>.
        ''' </summary>
        ''' <param name="strsIn">The <see cref="SecureString">secure string</see> to convert.</param>
        ''' <returns>A <see cref="String">regular string</see>.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToInsecureString(strsIn As SecureString) As String
            Dim strResult As String = String.Empty
            Dim ptr As IntPtr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(strsIn)
            Try
                strResult = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr)
            Finally
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr)
            End Try
            Return strResult
        End Function

#End Region ' Encryption

#Region " Private classes "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' String comparer to perform natural sorting of strings.
        ''' </summary>
        ''' <remarks>
        ''' Converted from http://www.dotnetperls.com/alphanumeric-sorting-vbnet
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Class cAlphanumComparer
            Implements IComparer(Of String)

            Public Function Compare(ByVal s1 As String, ByVal s2 As String) As Integer _
                Implements System.Collections.Generic.IComparer(Of String).Compare

                ' [1] Validate the arguments
                If (String.IsNullOrWhiteSpace(s1)) Then Return 0
                If (String.IsNullOrWhiteSpace(s2)) Then Return 0

                Dim len1 As Integer = s1.Length
                Dim len2 As Integer = s2.Length
                Dim marker1 As Integer = 0
                Dim marker2 As Integer = 0

                ' [2] Loop over both Strings.
                While marker1 < len1 And marker2 < len2

                    ' [3] Get Chars.
                    Dim ch1 As Char = s1(marker1)
                    Dim ch2 As Char = s2(marker2)

                    Dim space1(len1) As Char
                    Dim loc1 As Integer = 0
                    Dim space2(len2) As Char
                    Dim loc2 As Integer = 0

                    ' [4] Collect digits for String one.
                    Do
                        space1(loc1) = ch1
                        loc1 += 1
                        marker1 += 1

                        If marker1 < len1 Then
                            ch1 = s1(marker1)
                        Else
                            Exit Do
                        End If
                    Loop While Char.IsDigit(ch1) = Char.IsDigit(space1(0))

                    ' [5] Collect digits for String two.
                    Do
                        space2(loc2) = ch2
                        loc2 += 1
                        marker2 += 1

                        If marker2 < len2 Then
                            ch2 = s2(marker2)
                        Else
                            Exit Do
                        End If
                    Loop While Char.IsDigit(ch2) = Char.IsDigit(space2(0))

                    ' [6] Convert to Strings.
                    Dim str1 As New String(space1)
                    Dim str2 As New String(space2)

                    ' [7] Parse Strings into Integers.
                    Dim result As Integer
                    If Char.IsDigit(space1(0)) And Char.IsDigit(space2(0)) Then
                        Dim thisNumericChunk As Integer = Integer.Parse(str1)
                        Dim thatNumericChunk As Integer = Integer.Parse(str2)
                        result = thisNumericChunk.CompareTo(thatNumericChunk)
                    Else
                        result = str1.CompareTo(str2)
                    End If

                    ' [8] Return result if not equal.
                    If Not result = 0 Then
                        Return result
                    End If
                End While

                ' [9] Compare lengths.
                Return len1 - len2
            End Function

        End Class

#End Region ' Private classes

    End Class

End Namespace ' Utilities