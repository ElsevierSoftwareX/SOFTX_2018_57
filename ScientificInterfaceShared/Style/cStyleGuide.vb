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

Imports System.Collections.Specialized
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Threading
Imports EwECore
Imports EwECore.Auxiliary
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.Drawing
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Style

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' The style guide provides the one and only interface to standardized user 
    ''' interface color feedback.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cStyleGuide
        Implements IDisposable

#Region " Private bits "

        Private m_core As cCore = Nothing

        ''' <summary>States the number of decimal digits to be displayed</summary>
        Private m_iNumDigits As Integer = 3
        ''' <summary>States whether numbers are formatted in groups.</summary>
        Private m_bGroupDigits As Boolean = False

        ' -- 
        ''' <summary>Default currency unit.</summary>
        Private m_unitCurrency As eUnitCurrencyType = eUnitCurrencyType.Nitrogen
        ''' <summary>Currency unit custom text.</summary>
        Private m_strUnitCurrencyCustom As String = ""
        ''' <summary>Default currency unit.</summary>
        Private m_unitTime As eUnitTimeType = eUnitTimeType.Year
        ''' <summary>Time unit custom text.</summary>
        Private m_strUnitTimeCustom As String = ""
        ''' <summary>Default monetary unit.</summary>
        Private m_unitMonetary As String = ""
        ''' <summary>Monetary unit custom text.</summary>
        Private m_strUnitMonetaryCustom As String = ""
        ''' <summary>Default area unit.</summary>
        Private m_unitArea As eUnitAreaType = eUnitAreaType.Km2
        ''' <summary>Area unit custom text.</summary>
        Private m_strUnitAreaCustom As String = ""

        ' -- internal management --
        ''' <summary>States whether the StyleGuide contains unsaved changes</summary>
        Private m_bChanged As Boolean = False
        ''' <summary>Application colour scheme.</summary>
        Private m_dtApplicationColors As New Dictionary(Of cStyleGuide.eApplicationColorType, Color)
        ''' <summary>Shape colour scheme.</summary>
        Private m_dtShapeColors As New Dictionary(Of eDataTypes, Color)
        ''' <summary>Color ramp for obtaining EwE5 group colors</summary>
        Private m_colorrampGroups As New cEwEColorRamp()
        ''' <summary>Color ramp for obtaining fleet colors</summary>
        Private m_colorrampFleets As New cARGBColorRamp(New Color() {Color.Green, Color.LightGreen, Color.LightBlue, Color.Blue, Color.DarkBlue}, New Double() {0.0#, 0.4#, 0.3#, 0.2#, 0.1#})
        ''' <summary>Color ramp for obtaining pedigree colors</summary>
        Private m_colorrampPedigree As New cEwEColorRamp()
        ''' <summary>Start offset for colour ramp.</summary>
        Private Const c_sRampOffsetStart As Single = 0.15!
        ''' <summary>End offset for colour ramp.</summary>
        Private Const c_sRampOffsetEnd As Single = 1.0!

        ' -- graphs --
        ''' <summary></summary>
        Private m_dtFontFamilyName As New Dictionary(Of eApplicationFontType, String)
        ''' <summary></summary>
        Private m_dtFontSize As New Dictionary(Of eApplicationFontType, Single)
        ''' <summary></summary>
        Private m_dtFontStye As New Dictionary(Of eApplicationFontType, FontStyle)
        ''' <summary>Usage of legends.</summary>
        ''' <remarks>UseDefault = selective, True or False</remarks>
        Private m_tsShowLegends As TriState = TriState.UseDefault
        ''' <summary>Usage of axis labels.</summary>
        ''' <remarks>UseDefault = selective, True or False</remarks>
        Private m_tsShowAxisLabels As TriState = TriState.UseDefault
        ''' <summary>Show transparent backgrounds where applicable</summary>
        Private m_bTransparentBackgrounds As Boolean = False

        ' -- group visibility --
        ''' <summary>List of indexes of groups to hide.</summary>
        Private m_lHiddenGroups As New List(Of Integer)
        ''' <summary>List of indexes of fleets to hide.</summary>
        Private m_lHiddenFleets As New List(Of Integer)
        Private m_bHideTotalCatch As Boolean = False
        Private m_bHideTotalValue As Boolean = False

        ' -- thumbnails --
        ''' <summary>Size (width and height) of thumbnails in EwE6.</summary>
        Private m_iThumbnailSize As Integer = 48

        ' -- maps --
        Private m_strMapRefLayerFile As String = ""
        Private m_ptMapRefLayerTL As PointF
        Private m_ptMapRefLayerBR As PointF
        ''' <summary>Display of excluded cells.</summary>
        Private m_bShowMapExcludedCells As Boolean = False
        ''' <summary>Display of MPAs.</summary>
        Private m_bShowMapMPAs As Boolean = False
        ''' <summary>Show labels on maps.</summary>
        Private m_bShowMapLabels As Boolean = True
        Private m_bShowMapLabelDate As Boolean = True
        Private m_bInvertMapLabelColor As Boolean = False
        Private m_posMapLabelHorz As StringAlignment = StringAlignment.Near
        Private m_posMapLabelVert As StringAlignment = StringAlignment.Near

        ' -- pedigree --
        Private m_bShowPedigree As Boolean = True

        ' -- EcoBase --
        Private m_dtEcoBaseFields As New Dictionary(Of eEcobaseFieldType, StringCollection)

        ' -- event locks --
        ''' <summary>Event lock count.</summary>
        Private m_nEventLock As Integer = 0
        ''' <summary>States whether there are events withheld and pending while an event lock is active.</summary>
        Private m_pendingChangeEventTypes As eChangeType = eChangeType.None

#End Region ' Private bits

#Region " Construction & destruction "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <remarks>Singleton enforced: constructor is only accessible locally</remarks>
        ''' -----------------------------------------------------------------------
        Public Sub New(core As cCore)

            Me.m_core = core

            ' Control how colour ramp delivers its colours
            Me.m_colorrampGroups.ColorOffsetStart = c_sRampOffsetStart
            Me.m_colorrampGroups.ColorOffsetEnd = c_sRampOffsetEnd

            Me.m_colorrampPedigree.ColorOffsetStart = c_sRampOffsetStart
            Me.m_colorrampPedigree.ColorOffsetEnd = c_sRampOffsetEnd

            ' Load up
            Me.ResetApplicationColors()

        End Sub

        Public Sub Dispose() _
            Implements IDisposable.Dispose

            Me.m_dtFontFamilyName.Clear()
            Me.m_dtFontSize.Clear()
            Me.m_dtFontStye.Clear()

            Me.m_dtApplicationColors.Clear()

            If Me.m_imgReference IsNot Nothing Then
                Me.m_imgReference.Dispose()
                Me.m_imgReference = Nothing
            End If
        End Sub

#End Region ' Construction & destruction

#Region " Public Methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Resets application colors to default values.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub ResetApplicationColors()
            'Default colors
            Me.m_dtApplicationColors.Clear()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Resets application fonts to default values.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub ResetApplicationFonts()
            Me.m_dtFontFamilyName.Clear()
            Me.m_dtFontSize.Clear()
            Me.m_dtFontStye.Clear()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Resets shape colors to default values.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub ResetShapeColors()
            Me.m_dtShapeColors.Clear()
        End Sub

#End Region ' Public interfaces

#Region " Public access "

#Region " Enums and events "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public enumerator stating the visual feedback required for rendering a value.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eStyleFlags As Integer

            '-----------------------------------------------------------------
            ' Directly mapped Core flags
            '-----------------------------------------------------------------

            ''' <summary>All well, value is OK and does not require any kind of formatting.</summary>
            OK = CInt(eStatusFlags.OK)

            ''' <summary>Flag indicating that Data Validation Failed for a value.</summary>
            FailedValidation = CInt(eStatusFlags.FailedValidation)

            ''' <summary>Flag indicating that a value was Computed, not entered.</summary>
            ValueComputed = CInt(eStatusFlags.ValueComputed)

            ''' <summary>Flag indicating that a value was Computed to an Invalid Result.</summary>
            InvalidModelResult = CInt(eStatusFlags.InvalidModelResult)

            ''' <summary>Flag indicating that a value is Not Editable, e.g. should not
            ''' be modified by user input.</summary>
            ''' <remarks>This flag is also known as ReadOnly or BlockedForInput (EwE5)</remarks>
            NotEditable = CInt(eStatusFlags.NotEditable)

            ''' <summary>Flag indicating that an Unknown Error has been encountered regarding this value.</summary>
            ErrorEncountered = CInt(eStatusFlags.ErrorEncountered)

            '''' <summary>Flag indicating that a value is a Missing Parameter for one of the EwE models.</summary>
            '''' <remarks>
            '''' This flag is different from <see cref="eStatusFlags.Null">StyleFlags.Null</see>; Null values
            '''' are model-technically not initialized while Missing Parameter values do not contain a valid
            '''' value for the model that they are used in.
            '''' </remarks>
            'MissingParameter = CInt(eStatusFlags.MissingParameter)

            ''' <summary>
            ''' Flag indicating that the core deemed a value as important for whatever reason. The
            ''' core is not able to communicate such reasons, and highlighting is therefore an
            ''' ad-hoc process on a per-case basis.
            ''' </summary>
            Checked = CInt(eStatusFlags.CoreHighlight)

            ''' <summary>Flag indicating that a value is Null; its value has not been set or has been
            ''' set to the <see cref="cCore.NULL_VALUE">Core NULL value</see>.</summary>
            Null = CInt(eStatusFlags.Null)

            ''' <summary>Bit-pattern mask to separate core statuses from GUI statuses.</summary>
            CoreStatusFlagsMask = 4095

            '-----------------------------------------------------------------
            ' GUI-specific flags
            '-----------------------------------------------------------------

            ' There is room for 3 flags (2^13, 2^14, and 2^15) here

            ''' <summary>EcoPath GUI flag; indicates whether a value has associated remarks.</summary>
            Remarks = 65536 ' 2^16

            ''' <summary>Flag indicating that a value provides a Summary of other values in the same screen.</summary>
            Sum = 131072 ' 2^17

            ''' <summary>Flag indicating that a value is Highlighted.</summary>
            Highlight = 262144 ' 2^18

            ''' <summary>Flag indicating that a value is a Name.</summary>
            Names = 524288 ' 2^19

            ''' <summary>Flag indicating that a value is an taxon code.</summary>
            Taxon = 1048576 ' 2^20

        End Enum

        Public Enum eLegendPosition As Integer

            ''' <summary>Do not show legends.</summary>
            Hidden = cCore.NULL_VALUE
            ''' <summary>Show legend to the left of graphs.</summary>
            Left = ZedGraph.LegendPos.Left
            ''' <summary>Show legend to the right of graphs.</summary>
            Right = ZedGraph.LegendPos.Right
            ''' <summary>Show legend above graphs.</summary>
            Above = ZedGraph.LegendPos.TopCenter
            ''' <summary>Show legend below graphs.</summary>
            Below = ZedGraph.LegendPos.BottomCenter

        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Types of changes that can occur in the StyleGuide.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eChangeType As Integer
            None = 0
            Colours = &H1
            NumberFormatting = &H2
            Units = &H4
            Fonts = &H8
            GroupVisibility = &H10
            FleetVisibility = &H20
            Thumbnails = &H40
            GraphStyle = &H80
            Map = &H100
            Pedigree = &H200
            EcobaseLists = &H400
            All = &HFFFFFFFF
        End Enum

        ''' <summary>Good old-fashioned (but slightly blunt) way</summary>
        Public Event StyleGuideChanged(ByVal changeType As eChangeType)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Notify listeners of StyleGuide changes
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub FireChangeEvent(ByVal changeType As eChangeType)
            ' Are events locked?
            If (Me.m_nEventLock > 0) Then
                ' #Yes: remember that an event is pending
                Me.m_pendingChangeEventTypes = Me.m_pendingChangeEventTypes Or changeType
                ' Abort, leave the event for later
                Return
            End If

            Try
                ' Broadcast change event to listeners
                RaiseEvent StyleGuideChanged(changeType)
            Catch ex As Exception
                cLog.Write(ex, "cStyleGuide.FireChangeEvent(" & changeType & ")")
            End Try

            ' No more change events pending
            Me.m_pendingChangeEventTypes = eChangeType.None
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Lock down the broadcasting of StyleGuide events.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub SuspendEvents()
            Me.m_nEventLock += 1
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Unlocks broadcasting of StyleGuide change events.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub ResumeEvents()
            Me.m_nEventLock -= 1
            ' Did this clear the event lock?
            If (Me.m_nEventLock <= 0) And (m_pendingChangeEventTypes <> eChangeType.None) Then
                ' Fire remaining event(s)
                FireChangeEvent(Me.m_pendingChangeEventTypes)
                ' Clear cache
                Me.m_pendingChangeEventTypes = eChangeType.None
            End If
        End Sub

        Public Enum eEcobaseFieldType As Integer
            CountryName
            EcosystemType
        End Enum

#End Region ' Enums and events

#Region " System settings "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the current UI culture is right-to-left ordered.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        <Obsolete("Use cSystemUtils.IsRightToLeft instead")>
        Public Shared Function IsRightToLeft() As Boolean
            Return cSystemUtils.IsRightToLeft()
        End Function

#End Region ' System settings

#Region " Number formatting "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the number of decimal digits to display.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property NumDigits() As Integer
            Get

                Return Me.m_iNumDigits

            End Get
            Set(ByVal nNumDigits As Integer)

                ' Is this a change?
                If (nNumDigits = Me.m_iNumDigits) Then
                    ' #No: abort
                    Return
                End If
                ' Update number of digits to maintain
                Me.m_iNumDigits = nNumDigits
                ' Notify listeners
                Me.FireChangeEvent(eChangeType.NumberFormatting)

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether formatted numbers are grouped via the thousands
        ''' separator.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property GroupDigits() As Boolean
            Get

                Return Me.m_bGroupDigits

            End Get
            Set(ByVal bGroupDigits As Boolean)

                ' Is this a change?
                If (bGroupDigits = Me.m_bGroupDigits) Then
                    ' #No: abort
                    Return
                End If
                ' Update 
                Me.m_bGroupDigits = bGroupDigits
                ' Notify listeners
                Me.FireChangeEvent(eChangeType.NumberFormatting)

            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <para>Format an Integer number to a string. The number will be rendered
        ''' with 0 relevant decimal digits.</para>
        ''' </summary>
        ''' <param name="iValue">The value to format.</param>
        ''' <param name="style">Optional <see cref="eStyleFlags">style flags</see> to
        ''' that may need specific formatting. Computed values for instance will
        ''' be represented with exactly the requested number of decimal digits, instead
        ''' of the </param>
        ''' <returns>A formatted value that always displays the least significant precision digit.</returns>
        ''' -----------------------------------------------------------------------
        Public Function FormatNumber(ByVal iValue As Integer,
                                     Optional ByVal style As eStyleFlags = eStyleFlags.OK) As String
            Return Me.FormatNumber(CDbl(iValue), style, 0)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <para>Format a Single number to a string. The number will be rendered
        ''' with a requested number of relevant decimal digits.</para>
        ''' </summary>
        ''' <param name="sValue">The value to format.</param>
        ''' <param name="iNumDigits">
        ''' <para>The minimum precision that should be used to format the value.</para>
        ''' <para>If left at its default value, this number is obtained from the 
        ''' <see cref="NumDigits">precision setting</see> in the StyleGuide.</para>
        ''' </param>
        ''' <param name="style">Optional <see cref="eStyleFlags">style flags</see> to
        ''' that may need specific formatting. Computed values for instance will
        ''' be represented with exactly the requested number of decimal digits, instead
        ''' of the </param>
        ''' <returns>A formatted value that always displays the least significant precision digit.</returns>
        ''' -----------------------------------------------------------------------
        Public Function FormatNumber(ByVal sValue As Single,
                                     Optional ByVal style As eStyleFlags = eStyleFlags.OK,
                                     Optional ByVal iNumDigits As Integer = -1,
                                     Optional ByVal tsGroupDigits As TriState = TriState.UseDefault) As String
            Return Me.FormatNumber(CDbl(sValue), style, iNumDigits, tsGroupDigits)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' <para>Format a Double number to a string. The number will be rendered
        ''' with a requested number of relevant decimal digits.</para>
        ''' </summary>
        ''' <param name="dValue">The value to format.</param>
        ''' <param name="iNumDigits">
        ''' <para>The minimum precision that should be used to format the value.</para>
        ''' <para>If left at its default value, this number is obtained from the 
        ''' <see cref="NumDigits">precision setting</see> in the StyleGuide.</para>
        ''' </param>
        ''' <param name="style">Optional <see cref="eStyleFlags">style flags</see> to
        ''' that may need specific formatting. Computed values for instance will
        ''' be represented with exactly the requested number of decimal digits.</param>
        ''' <returns>A formatted value that always displays the least significant precision digit.</returns>
        ''' -----------------------------------------------------------------------
        Public Function FormatNumber(ByVal dValue As Double,
                                     Optional ByVal style As eStyleFlags = eStyleFlags.OK,
                                     Optional ByVal iNumDigits As Integer = -1,
                                     Optional ByVal tsGroupDigits As TriState = TriState.UseDefault) As String

            ' Use styleguide numdigits setting if value not provided
            If (iNumDigits < 0) Then iNumDigits = Math.Max(0, Me.m_iNumDigits)

            If ((style And eStyleFlags.Null) > 0) Or (dValue = cCore.NULL_VALUE) Then
                Return ""
            End If

            If (tsGroupDigits = TriState.UseDefault) Then
                tsGroupDigits = If(Me.m_bGroupDigits, TriState.True, TriState.False)
            End If

            ' Calculated values must be formatted with a hard number of digits
            If (style And (eStyleFlags.ValueComputed Or eStyleFlags.Sum)) > 0 Then
                Return FormatNumber(dValue, iNumDigits, tsGroupDigits)
            End If

            ' Format the value with selected number of decimal digits
            Return FormatNumber(dValue, cNumberUtils.NumRelevantDecimals(CDbl(Math.Abs(dValue)), iNumDigits), tsGroupDigits)

        End Function

        Private Shared Function FormatNumber(dValue As Double, iNumDigits As Integer, tsGroupDigits As TriState) As String

            Dim nf As NumberFormatInfo = DirectCast(Thread.CurrentThread.CurrentCulture.NumberFormat.Clone, NumberFormatInfo)
            nf.NumberDecimalDigits = iNumDigits

            Select Case tsGroupDigits
                Case TriState.True
                Case TriState.False
                    nf.NumberGroupSeparator = ""
                Case TriState.UseDefault
            End Select

            Return dValue.ToString("N", nf)

        End Function

        Public Function FormatMemory(size As Long) As String

            Dim astrUnits As String() = New String() {My.Resources.UNIT_BYTE, My.Resources.UNIT_KILOBYTE, My.Resources.UNIT_MEGABYTE, My.Resources.UNIT_TERABYTE}
            Dim i As Integer = 0
            Dim dTest As Double = 1024
            Dim dValue As Double = size

            While (dValue > dTest) And (i < astrUnits.Length - 1)
                dValue /= 1024
                i += 1
            End While

            Return cStringUtils.Localize(My.Resources.GENERIC_LABEL_DOUBLE, Me.FormatNumber(CInt(size)), astrUnits(i))

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Format a date for display in the user interface.
        ''' </summary>
        ''' <param name="dt">The date to format.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function FormatDate(dt As DateTime, Optional bShowDay As Boolean = True) As String
            If bShowDay Then
                Return dt.ToString("yyyy/MM/dd")
            End If
            Return dt.ToString("yyyy/MM")
        End Function

#End Region ' Number formatting

#Region " Units "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Call this when application unit settings have changed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub UnitsChanged()
            Me.FireChangeEvent(eChangeType.Units)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Format a string from one or more units.
        ''' </summary>
        ''' <param name="strUnits">Units to format into the mask.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function FormatUnitString(ByVal strUnits As String) As String
            Dim units As New cUnits(Me.m_core)
            Return units.ToString(strUnits)
        End Function

#Region " Currency units "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to cascade currency unit changes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property CurrencyUnit() As eUnitCurrencyType
            Set(ByVal value As eUnitCurrencyType)
                If (Me.m_unitCurrency <> value) Then
                    Me.m_unitCurrency = value
                    Me.UnitsChanged()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method to cascade custom currency unit text changes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property CustomCurrencyUnitText() As String
            Set(ByVal value As String)
                If (String.Compare(Me.m_strUnitCurrencyCustom, value) <> 0) Then
                    Me.m_strUnitCurrencyCustom = value
                    Me.UnitsChanged()
                End If
            End Set
        End Property

        Public ReadOnly Property CurrencyUnitText(ByVal unit As eUnitCurrencyType) As String
            Get
                Dim fmt As New cCurrencyUnitFormatter(Me.m_strUnitCurrencyCustom)
                Return fmt.GetDescriptor(unit)
            End Get
        End Property

#End Region ' Currency units

#Region " Time units "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, get/set the time unit text.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property TimeUnit() As eUnitTimeType
            Set(ByVal value As eUnitTimeType)
                If (Me.m_unitTime <> value) Then
                    Me.m_unitTime = value
                    Me.UnitsChanged()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the textual representation of a time unit.
        ''' </summary>
        ''' <param name="unit">Time unit to represent.</param>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property TimeUnitText(ByVal unit As eUnitTimeType) As String
            Get
                Dim fmt As New cTimeUnitFormatter(Me.m_strUnitTimeCustom)
                Return fmt.GetDescriptor(unit)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the custom time unit text.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property CustomTimeUnitText() As String
            Set(ByVal value As String)
                If (String.Compare(Me.m_strUnitTimeCustom, value) <> 0) Then
                    Me.m_strUnitTimeCustom = value
                    Me.UnitsChanged()
                End If
            End Set
        End Property

#End Region ' Time units

#Region " Monetary units "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, get/set the monetary unit to show in the application.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property MonetaryUnit() As String
            Set(ByVal value As String)
                If (Me.m_unitMonetary <> value) Then
                    Me.m_unitMonetary = value
                    Me.UnitsChanged()
                End If
            End Set
        End Property

#End Region ' Monetary units

#Region " Area units "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, get/set the area unit to show in the application.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property AreaUnit() As eUnitAreaType
            Set(ByVal value As eUnitAreaType)
                If (Me.m_unitArea <> value) Then
                    Me.m_unitArea = value
                    Me.UnitsChanged()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, get the area unit text to show in the application.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property AreaUnitText(ByVal unit As eUnitAreaType) As String
            Get
                Dim fmt As New cAreaUnitFormatter(Me.m_strUnitAreaCustom)
                Return fmt.GetDescriptor(unit)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, get/set the text for the area unit text to show 
        ''' in the application.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public WriteOnly Property CustomAreaUnitText() As String
            Set(ByVal value As String)
                If (String.Compare(Me.m_strUnitAreaCustom, value) <> 0) Then
                    Me.m_strUnitAreaCustom = value
                    Me.UnitsChanged()
                End If
            End Set
        End Property

#End Region ' Area units

#Region " Nominal units "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method, get the nominal unit text to show in the application.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property NominalUnitText() As String
            Get
                Return "#"
            End Get
        End Property

#End Region ' Nominal units

#End Region ' Units

#Region " Maps and charts "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set how graphs should show legends.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowLegends() As TriState
            Get
                Return Me.m_tsShowLegends
            End Get
            Set(ByVal value As TriState)
                Me.m_tsShowLegends = value
                Me.GraphStyleChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set how graphs should show axis labels.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowAxisLabels() As TriState
            Get
                Return Me.m_tsShowAxisLabels
            End Get
            Set(ByVal value As TriState)
                Me.m_tsShowAxisLabels = value
                Me.GraphStyleChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Broadcast a <see cref="eChangeType.GraphStyle">graph style changed event</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub GraphStyleChanged()
            Me.FireChangeEvent(eChangeType.GraphStyle)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set how graphs should display backgrounds.
        ''' </summary>
        ''' <remarks>
        ''' Whenever this setting changes a <see cref="eChangeType.Colours">Colours</see>
        ''' change is broadcasted.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property UseTransparentBackgrounds() As Boolean
            Get
                Return Me.m_bTransparentBackgrounds
            End Get
            Set(ByVal value As Boolean)
                If value <> Me.m_bTransparentBackgrounds Then
                    Me.m_bTransparentBackgrounds = value
                    Me.ColorsChanged()
                End If
            End Set
        End Property

        Public Sub MapStyleChanged()
            Me.FireChangeEvent(eChangeType.Map)
        End Sub

        Public Property MapReferenceLayerFile As String
            Get
                Return Me.m_strMapRefLayerFile
            End Get
            Set(value As String)
                If (String.Compare(Me.m_strMapRefLayerFile, value) <> 0) Then
                    If (Me.m_imgReference IsNot Nothing) Then
                        Me.m_imgReference.Dispose()
                        Me.m_imgReference = Nothing
                    End If
                    Me.m_strMapRefLayerFile = value
                    Me.MapStyleChanged()
                End If
            End Set
        End Property

        Public Property MapReferenceLayerTL As PointF
            Get
                Return Me.m_ptMapRefLayerTL
            End Get
            Set(value As PointF)
                If Not Point.Equals(Me.m_ptMapRefLayerTL, value) Then
                    Me.m_ptMapRefLayerTL = value
                    MapStyleChanged()
                End If
            End Set
        End Property

        Public Property MapReferenceLayerBR As PointF
            Get
                Return Me.m_ptMapRefLayerBR
            End Get
            Set(value As PointF)
                If Not Point.Equals(Me.m_ptMapRefLayerBR, value) Then
                    Me.m_ptMapRefLayerBR = value
                    MapStyleChanged()
                End If
            End Set
        End Property

        Private m_imgReference As Image = Nothing

        Public ReadOnly Property MapReferenceImage As Image
            Get
                If (Me.m_imgReference Is Nothing) Then
                    Try
                        If (File.Exists(Me.MapReferenceLayerFile)) Then
                            Me.m_imgReference = Image.FromFile(Me.MapReferenceLayerFile)
                        End If
                    Catch ex As Exception

                    End Try
                End If
                Return Me.m_imgReference
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set if maps should show excluded cells.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowMapsExcludedCells() As Boolean
            Get
                Return Me.m_bShowMapExcludedCells
            End Get
            Set(ByVal value As Boolean)
                Me.m_bShowMapExcludedCells = value
                Me.MapStyleChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set if maps should show MPA cells.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowMapsMPAs() As Boolean
            Get
                Return Me.m_bShowMapMPAs
            End Get
            Set(ByVal value As Boolean)
                Me.m_bShowMapMPAs = value
                Me.MapStyleChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set if maps should show labels.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowMapLabels() As Boolean
            Get
                Return Me.m_bShowMapLabels
            End Get
            Set(ByVal value As Boolean)
                Me.m_bShowMapLabels = value
                Me.MapStyleChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set if maps should show datas in labels.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowMapsDateInLabels() As Boolean
            Get
                Return Me.m_bShowMapLabelDate
            End Get
            Set(ByVal value As Boolean)
                Me.m_bShowMapLabelDate = value
                Me.MapStyleChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set if maps labels should be drawn in inverted colours.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property InvertMapLabelColor() As Boolean
            Get
                Return Me.m_bInvertMapLabelColor
            End Get
            Set(ByVal value As Boolean)
                Me.m_bInvertMapLabelColor = value
                Me.MapStyleChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the horizontal map position
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property MapLabelPosHorizontal() As StringAlignment
            Get
                Return Me.m_posMapLabelHorz
            End Get
            Set(ByVal value As StringAlignment)
                Me.m_posMapLabelHorz = value
                Me.MapStyleChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the vertical map position
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property MapLabelPosVertical() As StringAlignment
            Get
                Return Me.m_posMapLabelVert
            End Get
            Set(ByVal value As StringAlignment)
                Me.m_posMapLabelVert = value
                Me.MapStyleChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set if sketching one habitat layer should allow the GUI to
        ''' auto-correct the cell ratio of other habitats in affected cells.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property UseHabitatAreaCorrection As Boolean = False

#End Region ' Maps and charts

#Region " Generic "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return the character to display for password input.
        ''' </summary>
        ''' <returns>The character to display for password input.</returns>
        ''' -------------------------------------------------------------------
        Public Function PasswordChar() As Char
            Return "●"c
        End Function

#End Region ' Generic

#Region " Color access "

#Region " Group "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the color to represent a group.
        ''' </summary>
        ''' <remarks>
        ''' Setting the Alpha component of the ARGB colour value to 0 will
        ''' trigger the style guide to issue default colours for groups.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property GroupColor(ByVal core As cCore, ByVal iGroup As Integer) As Color
            Get
                Dim clr As Color = Color.Transparent
                If (0 < iGroup) And (iGroup <= core.nGroups) Then
                    Dim grp As cEcoPathGroupInput = core.EcoPathGroupInputs(iGroup)
                    clr = cColorUtils.IntToColor(grp.PoolColor)
                End If
                If clr.A = 0 Then
                    clr = Me.GroupColorDefault(core, iGroup)
                End If
                Return clr
            End Get
            Set(ByVal value As Color)
                If (0 < iGroup) And (iGroup <= core.nGroups) Then
                    Dim grp As cEcoPathGroupInput = core.EcoPathGroupInputs(iGroup)
                    ' Optimization
                    If grp.PoolColor = cColorUtils.ColorToInt(value) Then Return
                    ' Apply
                    grp.PoolColor = cColorUtils.ColorToInt(value)
                    ' Notify the world
                    Me.ColorsChanged()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a default colour for a group.
        ''' </summary>
        ''' <param name="core">Core to operate onto.</param>
        ''' <param name="iGroup">The group index to obtain the default colour for.</param>
        ''' <returns>
        ''' Default group colours are picked from the Ecopath 5 group colour scheme.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function GroupColorDefault(ByVal core As cCore, _
                                          ByVal iGroup As Integer) As Color
            If (iGroup = 0) Then Return Color.Gray
            Return Me.GroupColorDefault(iGroup, core.nGroups)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a default colour for a group.
        ''' </summary>
        ''' <param name="iGroup">The group index to obtain the default colour for.</param>
        ''' <param name="nGroups">The number of groups to scale the colour to.</param>
        ''' <returns>
        ''' Default group colours are picked from the Ecopath 5 group colour scheme.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function GroupColorDefault(ByVal iGroup As Integer, _
                                          ByVal nGroups As Integer) As Color
            Return Me.m_colorrampGroups.GetColor(iGroup, nGroups)
        End Function

#End Region ' Group

#Region " Fleet "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the color to represent a fleet.
        ''' </summary>
        ''' <remarks>
        ''' Setting the Alpha component of the ARGB colour value to 0 will
        ''' trigger the style guide to issue default colours for fleets.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property FleetColor(ByVal core As cCore, ByVal iFleet As Integer) As Color
            Get
                Dim clr As Color = Color.Transparent
                If (0 <= iFleet) And (iFleet <= core.nFleets) Then
                    Dim flt As cEcopathFleetInput = core.EcopathFleetInputs(iFleet)
                    clr = cColorUtils.IntToColor(flt.PoolColor)
                End If
                If clr.A = 0 Then
                    clr = Me.FleetColorDefault(core, iFleet)
                End If
                Return clr
            End Get
            Set(ByVal value As Color)
                If (0 <= iFleet) And (iFleet <= core.nFleets) Then
                    Dim flt As cEcopathFleetInput = core.EcopathFleetInputs(iFleet)
                    ' Optimization
                    If flt.PoolColor = cColorUtils.ColorToInt(value) Then Return
                    ' Apply
                    flt.PoolColor = cColorUtils.ColorToInt(value)
                    ' Notify the world
                    Me.ColorsChanged()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a default colour for a fleet.
        ''' </summary>
        ''' <param name="iFleet">The fleet index to obtain the default colour for.</param>
        ''' <param name="nFleets">Number of fleets to scale colour by, or -1 to
        ''' use the max number of fleets as dictated by the core.</param>
        ''' <returns>
        ''' Default fleet colours are picked from a colour ramp that runs from
        ''' green to blue.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function FleetColorDefault(ByVal iFleet As Integer, _
                                          ByVal nFleets As Integer) As Color
            If (iFleet = 0) Then Return Color.Gray
            Return Me.m_colorrampFleets.GetColor(iFleet, nFleets)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a default colour for a fleet.
        ''' </summary>
        ''' <param name="core">Core to operate onto.</param>
        ''' <param name="iFleet">The fleet index to obtain the default colour for.</param>
        ''' <returns>
        ''' Default fleet colours are picked from a colour ramp that runs from
        ''' green to blue.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function FleetColorDefault(ByVal core As cCore, _
                                          ByVal iFleet As Integer) As Color
            Return FleetColorDefault(iFleet, core.nFleets)
        End Function

#End Region ' Fleet 

#Region " Pedigree "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Notify the world that the pedigree display style has changed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub PedigreeChanged()
            Me.FireChangeEvent(eChangeType.Pedigree)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set whether pedigree indicators are to be shown on input grids and
        ''' other UI elements.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ShowPedigree As Boolean
            Get
                Return Me.m_bShowPedigree
            End Get
            Set(value As Boolean)
                Me.m_bShowPedigree = value
                Me.PedigreeChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the color to represent a pedigree level.
        ''' </summary>
        ''' <remarks>
        ''' Setting the Alpha component of the ARGB colour value to 0 will
        ''' trigger the style guide to issue default colours for pedigree levels.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Property PedigreeColor(ByVal core As cCore, ByVal vn As eVarNameFlags, ByVal iLevel As Integer) As Color
            Get
                Dim clr As Color = Color.Transparent
                Dim man As cPedigreeManager = core.GetPedigreeManager(vn)
                If (man Is Nothing) Then Return clr
                If (0 < iLevel) And (iLevel <= man.NumLevels) Then
                    Dim lvl As cPedigreeLevel = man.Level(iLevel)
                    clr = cColorUtils.IntToColor(lvl.PoolColor)
                End If
                If clr.A = 0 Then
                    clr = Me.PedigreeColorDefault(iLevel, man.NumLevels)
                End If
                Return clr
            End Get
            Set(ByVal value As Color)
                Dim man As cPedigreeManager = core.GetPedigreeManager(vn)
                If (man IsNot Nothing) Then
                    If (0 < iLevel) And (iLevel <= man.NumLevels) Then
                        Dim lvl As cPedigreeLevel = man.Level(iLevel)
                        ' Optimization
                        If lvl.PoolColor = cColorUtils.ColorToInt(value) Then Return
                        ' Apply
                        lvl.PoolColor = cColorUtils.ColorToInt(value)
                        ' Notify the world
                        Me.ColorsChanged()
                    End If
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a default colour for a pedigree level.
        ''' </summary>
        ''' <param name="iLevel">The level index to obtain the default colour for.</param>
        ''' <param name="nLevels">Number of levels to scale colour by.</param>
        ''' <returns>
        ''' Default pedigree colours are picked from a SAUP/EwE5 colour ramp.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function PedigreeColorDefault(ByVal iLevel As Integer, _
                                             ByVal nLevels As Integer) As Color
            Return Me.m_colorrampPedigree.GetColor(iLevel - 1, nLevels)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a default colour for a pedigree level.
        ''' </summary>
        ''' <param name="core">Core to operate onto.</param>
        ''' <param name="iLevel">The level index to obtain the default colour for.</param>
        ''' <param name="vn">The variable of the level to query.</param>
        ''' <returns>
        ''' Default group colours are picked from a colour ramp that runs from
        ''' green to blue.
        ''' </returns>
        ''' -------------------------------------------------------------------
        Public Function PedigreeColorDefault(ByVal core As cCore, _
                                             ByVal iLevel As Integer, _
                                             ByVal vn As eVarNameFlags) As Color
            Debug.Assert(core.IsPedigreeVariableSupported(vn))
            Return PedigreeColorDefault(iLevel, core.GetPedigreeManager(vn).NumLevels)
        End Function

#End Region ' Pedigree

#Region " Application "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enumerated type defining the types of EwE6 user interface elements
        ''' for which custom colour coding is available.
        ''' </summary>
        ''' <remarks>
        ''' Not all styles will support both foreground and background colours.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Enum eApplicationColorType As Integer
            NotSet = 0
            DEFAULT_TEXT
            DEFAULT_BACKGROUND
            READONLY_BACKGROUND
            REMARKS_BACKGROUND
            SUM_BACKGROUND
            NAMES_TEXT
            NAMES_BACKGROUND
            CHECKED_BACKGROUND
            FAILEDVALIDATION_TEXT
            MISSINGPARAMETER_BACKGROUND
            COMPUTED_TEXT
            INVALIDMODELRESULT_TEXT
            GENERICERROR_BACKGROUND
            HIGHLIGHT
            IMAGE_BACKGROUND
            PLOT_BACKGROUND
            MAP_BACKGROUND
            PREY
            PREDATOR
            PEDIGREE
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enumerated type defining the types of EwE6 user interface elements
        ''' for which custom font options are available.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eApplicationFontType As Integer
            NotSet = 0
            ''' <summary>The font to use for graphs and charts major titles.</summary>
            Title
            ''' <summary>The font to use for graphs and charts legend text.</summary>
            Legend
            ''' <summary>The font to use for graphs and charts minor titles, 
            ''' such as subtitles, axis labels, legend titles, etc.</summary>
            SubTitle
            ''' <summary>The font to use for graph and chart axis labels.</summary>
            Scale
        End Enum

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get colours for a given combination of <see cref="eStyleFlags">styles</see>.
        ''' </summary>
        ''' <param name="eStatus">The bitwise pattern of <see cref="eStyleFlags">style</see>
        ''' to retrieve a foreground and background colour for.</param>
        ''' <param name="colorText">A foreground color that will be returned for the
        ''' given style pattern.</param>
        ''' <param name="colorBackground">A background color that will be returned for the
        ''' given style pattern.</param>
        ''' <remarks>
        ''' The algorithm that picks the colour to return analyzes that provided
        ''' style flags by order of priority. This priority is arbitrary, where
        ''' style flags indicating severe statuses will precede over lesser,
        ''' mere informational style flags.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Sub GetStyleColors(ByVal eStatus As cStyleGuide.eStyleFlags, _
                                  ByRef colorText As Color, _
                                  ByRef colorBackground As Color)

            ' Default priorities, used when the provided priorities did not yield
            ' a status to display, or when no priority sequence has been provided.
            Dim ePriorities() As cStyleGuide.eStyleFlags = { _
                    cStyleGuide.eStyleFlags.Null, _
                    cStyleGuide.eStyleFlags.InvalidModelResult, _
                    cStyleGuide.eStyleFlags.FailedValidation, _
                    cStyleGuide.eStyleFlags.ErrorEncountered, _
                    cStyleGuide.eStyleFlags.ValueComputed, _
                    cStyleGuide.eStyleFlags.Remarks, _
                    cStyleGuide.eStyleFlags.Sum, _
                    cStyleGuide.eStyleFlags.Names, _
                    cStyleGuide.eStyleFlags.Checked, _
                    cStyleGuide.eStyleFlags.NotEditable, _
                    cStyleGuide.eStyleFlags.OK}

            ' JS 02Aug08: disabled, not used at all
            'StyleGuide.eStyleFlags.FishingPressure, _
            'StyleGuide.eStyleFlags.Profit, _
            'StyleGuide.eStyleFlags.TotalCatch, _
            'StyleGuide.eStyleFlags.TrophicLink, _

            ' Set defaults
            Dim eColorText As cStyleGuide.eApplicationColorType = 0
            Dim eColorBack As cStyleGuide.eApplicationColorType = 0

            ' Variable statuses may have a text style, a background style or both.
            ' 
            ' This code can probably do with some serious optimizing.

            ' Now iterate in REVERSE ORDER (e.g. least important styles first)
            ' through the constructed priorities array. Every time a style match
            ' is encountered, available style parts are 'upgraded'
            For i As Integer = ePriorities.Length - 1 To 0 Step -1

                Select Case (eStatus And ePriorities(i))

                    Case cStyleGuide.eStyleFlags.Null
                        ' No specific colour feedback

                    Case cStyleGuide.eStyleFlags.InvalidModelResult
                        eColorText = eApplicationColorType.INVALIDMODELRESULT_TEXT

                    Case cStyleGuide.eStyleFlags.FailedValidation
                        eColorText = eApplicationColorType.FAILEDVALIDATION_TEXT

                    Case cStyleGuide.eStyleFlags.ErrorEncountered
                        eColorBack = eApplicationColorType.GENERICERROR_BACKGROUND

                    Case cStyleGuide.eStyleFlags.ValueComputed
                        eColorText = eApplicationColorType.COMPUTED_TEXT

                    Case cStyleGuide.eStyleFlags.Remarks
                        eColorBack = eApplicationColorType.REMARKS_BACKGROUND

                    Case cStyleGuide.eStyleFlags.Sum
                        eColorBack = eApplicationColorType.SUM_BACKGROUND

                        'Case StyleGuide.eStyleFlags.FishingPressure
                        '    eColorText = eApplicationColorType.FISHINGPRESSURE_TEXT

                        'Case StyleGuide.eStyleFlags.Profit
                        '    eColorText = eApplicationColorType.PROFIT_TEXT

                        'Case StyleGuide.eStyleFlags.TotalCatch
                        '    eColorText = eApplicationColorType.TOTALCATCH_TEXT

                        'Case StyleGuide.eStyleFlags.TrophicLink
                        '    eColorText = eApplicationColorType.TROPHICLINK_TEXT

                    Case eStyleFlags.Checked
                        eColorBack = eApplicationColorType.CHECKED_BACKGROUND

                    Case cStyleGuide.eStyleFlags.Names
                        eColorText = eApplicationColorType.NAMES_TEXT
                        eColorBack = eApplicationColorType.NAMES_BACKGROUND

                    Case cStyleGuide.eStyleFlags.NotEditable
                        eColorBack = eApplicationColorType.READONLY_BACKGROUND

                    Case cStyleGuide.eStyleFlags.OK
                        eColorText = eApplicationColorType.DEFAULT_TEXT
                        eColorBack = eApplicationColorType.DEFAULT_BACKGROUND

                End Select
            Next i

            ' Finally fetch the real colours
            If eColorText > 0 Then colorText = Color.FromArgb(Me.ApplicationColor(eColorText).ToArgb)
            If eColorBack > 0 Then colorBackground = Color.FromArgb(Me.ApplicationColor(eColorBack).ToArgb)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the color for a particular type of <see cref="eApplicationColorType">application feedback.</see>.
        ''' </summary>
        ''' <param name="colorType">The <see cref="eApplicationColorType">application feedback type</see>
        ''' to affect.</param>
        ''' -------------------------------------------------------------------
        Public Property ApplicationColor(ByVal colorType As cStyleGuide.eApplicationColorType) As Color
            Get
                ' Sanity check
                If (Me.m_dtApplicationColors.ContainsKey(colorType)) Then
                    Return Me.m_dtApplicationColors(colorType)
                End If
                Return DefaultColor(colorType)
            End Get
            Set(ByVal value As Color)
                ' Optimization
                If (Me.m_dtApplicationColors.ContainsKey(colorType)) Then
                    If Me.m_dtApplicationColors(colorType) = value Then Return
                End If

                ' Apply
                Me.m_dtApplicationColors(colorType) = value
                ' Notify the world
                Me.ColorsChanged()
            End Set
        End Property

#End Region ' Application

#Region " Shape "

        Public Property ShapeColor(ByVal shapetype As eDataTypes) As Color
            Get
                If (Me.m_dtShapeColors.ContainsKey(shapetype)) Then
                    Return Me.m_dtShapeColors(shapetype)
                End If
                Return DefaultShapeColor(shapetype)
            End Get
            Set(value As Color)

                ' Optimization
                If (Me.m_dtShapeColors.ContainsKey(shapetype)) Then
                    If Me.m_dtShapeColors(shapetype) = value Then Return
                End If

                ' Apply
                Me.m_dtShapeColors(shapetype) = value
                ' Notify the world
                Me.ColorsChanged()
            End Set
        End Property

#End Region ' Shape

#Region " Generics "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a list of colours, picked from the Ecopath 5 group colour
        ''' scheme.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function GetEwE5ColorRamp(ByVal iNumLevels As Integer) As List(Of Color)
            Dim lColors As New List(Of Color)
            For i As Integer = 0 To iNumLevels
                Dim clr As Color = Me.m_colorrampGroups.GetColor(i, iNumLevels)
                lColors.Add(clr)
            Next
            Return lColors
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Notify the world that colours have changed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub ColorsChanged()
            Me.FireChangeEvent(eChangeType.Colours)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a random color.
        ''' </summary>
        ''' <returns>A random color.</returns>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property NextRandomColor() As Color
            Get
                Return cColorUtils.RandomColor(Color.Gray)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Calculate a series of alternating <see cref="HSV">HSV colors</see> 
        ''' over a range.
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="iLen"></param>
        ''' <param name="iHueScale"></param>
        ''' <param name="iSaturationRange"></param>
        ''' <param name="iValueRange"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function CalculateAlternatingColors(ByVal i As Integer, _
                                                          ByVal iLen As Integer, _
                                                          Optional ByVal iHueScale As Integer = 9, _
                                                          Optional ByVal iSaturationRange As Integer = 240, _
                                                          Optional ByVal iValueRange As Integer = 200) As HSV

            Dim nCount As Integer = CInt(Math.Ceiling(Math.Sqrt(iLen / iHueScale)))
            Dim iHueTick As Integer = 255 \ iHueScale
            Dim iSaturationTick As Integer = 0
            Dim iValueTick As Integer = 0

            If nCount > 1 Then
                iSaturationTick = iSaturationRange \ nCount
                iValueTick = iValueRange \ nCount
            End If

            Dim i1 As Integer = (i - 1) Mod iHueScale
            Dim i2 As Integer = ((i - 1) \ iHueScale) Mod nCount
            Dim i3 As Integer = ((i - 1) \ (iHueScale * nCount)) Mod nCount
            Return New HSV(i1 * iHueTick, 255 - i2 * iSaturationTick, 255 - i3 * iValueTick)

        End Function

        Public Shared Function CalculateAlternatingStanzaGroupColor(ByVal hsvGroup As HSV, ByVal iLifeStage As Integer, ByVal iNumLifeStages As Integer) As HSV

            Dim sRange As Integer = 255
            Dim vRange As Integer = 100

            Dim sTick As Integer = sRange \ iNumLifeStages
            Dim vTick As Integer = vRange \ iNumLifeStages

            Return New HSV(hsvGroup.Hue, hsvGroup.Saturation - iLifeStage * sTick, hsvGroup.Value - (iNumLifeStages - iLifeStage - 1) * vTick)

        End Function

#End Region ' Generics

#End Region ' Color access

#Region " Fonts "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the font for a given application type. The font size is specified
        ''' in <see cref="GraphicsUnit.Point">points</see>. You must manually
        ''' dispose the font after use!
        ''' </summary>
        ''' <param name="ft">Font type indicator.</param>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Font(ByVal ft As eApplicationFontType) As Font
            Get
                ' ToDo: use proper DPI scaling here
                Return New Font(Me.FontFamilyName(ft), Me.FontSize(ft), Me.FontStyle(ft), GraphicsUnit.Point)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="FontFamily.Name">font family name</see> for 
        ''' a given application type.
        ''' </summary>
        ''' <param name="ft"></param>
        ''' -------------------------------------------------------------------
        Public Property FontFamilyName(ByVal ft As eApplicationFontType) As String
            Get
                If Me.m_dtFontFamilyName.ContainsKey(ft) Then
                    Dim strName As String = Me.m_dtFontFamilyName(ft)
                    If Not String.IsNullOrEmpty(strName) Then
                        Return strName
                    End If
                End If
                Return DefaultFontFamilyName(ft)
            End Get
            Set(ByVal value As String)
                Me.m_dtFontFamilyName(ft) = value
                Me.FontsChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="FontStyle">font style</see> for a given 
        ''' application type.
        ''' </summary>
        ''' <param name="ft"></param>
        ''' -------------------------------------------------------------------
        Public Property FontStyle(ByVal ft As eApplicationFontType) As FontStyle
            Get
                If Me.m_dtFontStye.ContainsKey(ft) Then
                    Return Me.m_dtFontStye(ft)
                End If
                Return DefaultFontStyle(ft)
            End Get
            Set(ByVal value As FontStyle)
                If (value < 0) Then
                    If (Me.m_dtFontStye.ContainsKey(ft)) Then Me.m_dtFontStye.Remove(ft)
                Else
                    Me.m_dtFontStye(ft) = value
                End If
                Me.FontsChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the font size for a given application type. The font size 
        ''' is specified in <see cref="GraphicsUnit.Point">points</see>.
        ''' </summary>
        ''' <param name="ft"></param>
        ''' -------------------------------------------------------------------
        Public Property FontSize(ByVal ft As eApplicationFontType) As Single
            Get
                If Me.m_dtFontSize.ContainsKey(ft) Then
                    Dim sSize As Single = Me.m_dtFontSize(ft)
                    If sSize >= 6 Then
                        Return sSize
                    End If
                End If
                Return DefaultFontSize(ft)
            End Get
            Set(ByVal value As Single)
                If (value < 0) Then
                    If (Me.m_dtFontSize.ContainsKey(ft)) Then Me.m_dtFontSize.Remove(ft)
                Else
                    Me.m_dtFontSize(ft) = value
                End If
                Me.FontsChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Broadcast a <see cref="eChangeType.Fonts">font changed event</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub FontsChanged()
            Me.FireChangeEvent(eChangeType.Fonts)
        End Sub

#End Region ' Fonts

#Region " Thumbnails "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the size of thumbnails.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property ThumbnailSize() As Integer
            Get
                If (Me.m_iThumbnailSize <= 0) Then Return 48 ' Default
                Return Math.Max(0, Math.Min(512, Me.m_iThumbnailSize))
            End Get
            Set(ByVal value As Integer)
                Me.m_iThumbnailSize = value
                Me.ThumbnailsChanged()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Broadcast a <see cref="eChangeType.Thumbnails">thumbnails changed event</see>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub ThumbnailsChanged()
            Me.FireChangeEvent(eChangeType.Thumbnails)
        End Sub

#End Region ' Thumbnails

#Region " Item visibility "

        Public Property GroupVisible(ByVal iGroup As Integer) As Boolean
            Get
                ' Return whether group is not hidden
                Return (Me.m_lHiddenGroups.IndexOf(iGroup) = -1)
            End Get
            Set(ByVal bVisible As Boolean)

                Dim bChanged As Boolean = False

                If bVisible Then
                    ' Remove group from hidden list, if applicable
                    If (Me.m_lHiddenGroups.IndexOf(iGroup) <> -1) Then
                        Me.m_lHiddenGroups.Remove(iGroup)
                        bChanged = True
                    End If
                Else
                    ' Add group to hidden list, if applicable
                    If (Me.m_lHiddenGroups.IndexOf(iGroup) = -1) Then
                        Me.m_lHiddenGroups.Add(iGroup)
                        bChanged = True
                    End If
                End If

                If bChanged Then Me.FireChangeEvent(eChangeType.GroupVisibility)
            End Set
        End Property

        Public Property FleetVisible(ByVal iFleet As Integer) As Boolean
            Get
                ' Return whether fleet is not hidden
                Return (Me.m_lHiddenFleets.IndexOf(iFleet) = -1)
            End Get
            Set(ByVal bVisible As Boolean)

                Dim bChanged As Boolean = False

                If bVisible Then
                    ' Remove fleet from hidden list, if applicable
                    If (Me.m_lHiddenFleets.IndexOf(iFleet) <> -1) Then
                        Me.m_lHiddenFleets.Remove(iFleet)
                        bChanged = True
                    End If
                Else
                    ' Add fleet to hidden list, if applicable
                    If (Me.m_lHiddenFleets.IndexOf(iFleet) = -1) Then
                        Me.m_lHiddenFleets.Add(iFleet)
                        bChanged = True
                    End If
                End If

                If bChanged Then Me.FireChangeEvent(eChangeType.FleetVisibility)
            End Set
        End Property

        Public Property TotalCatchVisible() As Boolean
            Get
                Return (Me.m_bHideTotalCatch = False)
            End Get
            Set(ByVal bShow As Boolean)
                Me.m_bHideTotalCatch = (bShow = False)
            End Set
        End Property

        Public Property TotalValueVisible() As Boolean
            Get
                Return (Me.m_bHideTotalValue = False)
            End Get
            Set(ByVal bShow As Boolean)
                Me.m_bHideTotalValue = (bShow = False)
            End Set
        End Property

        Public Sub ResetVisibleFlags(Optional ByVal bFireChangeEvent As Boolean = True)
            Me.m_lHiddenGroups.Clear()
            Me.m_lHiddenFleets.Clear()
            Me.m_bHideTotalCatch = False
            Me.m_bHideTotalValue = False

            If bFireChangeEvent Then Me.FireChangeEvent(eChangeType.GroupVisibility Or eChangeType.FleetVisibility)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Broadcast a <see cref="eChangeType.GroupVisibility">group</see> and
        ''' <see cref="eChangeType.FleetVisibility">fleet</see> visibility 
        ''' changed event.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub ItemVisibilityChanged()
            Me.FireChangeEvent(eChangeType.GroupVisibility Or eChangeType.FleetVisibility)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether any groups or fleets are hidden.
        ''' <seealso cref="GroupVisible"/>
        ''' <seealso cref="FleetVisible"/>
        ''' </summary>
        ''' <returns>True if any groups or fleets are hidden.</returns>
        ''' -------------------------------------------------------------------
        Public Function HasHiddenItems() As Boolean
            Return ((Me.m_lHiddenFleets.Count + Me.m_lHiddenGroups.Count) > 0)
        End Function

#End Region ' Item visibility

#Region " Item order "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns an array of <see cref="cCoreGroupBase">all groups</see>,
        ''' sorted by multi-stanza.
        ''' </summary>
        ''' <param name="core">The core to extract groups from.</param>
        ''' <remarks>This central method enables centralized sort options, 
        ''' such as sorting by stanza age (asc), stanza first, etc.</remarks>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Groups(core As cCore) As cCoreGroupBase()
            Get
                Dim grp As cCoreGroupBase = Nothing
                Dim grpTest As cCoreGroupBase = Nothing
                Dim bIncluded(core.nGroups) As Boolean
                Dim lGroups As New List(Of cCoreGroupBase)

                ' For all groups:
                For i As Integer = 1 To core.nGroups
                    ' Get group
                    grp = core.EcoPathGroupInputs(i)
                    ' Group not included in final list?
                    If Not bIncluded(i) Then
                        ' #Yes: add group
                        lGroups.Add(grp)
                        ' Is multi-stanza?
                        If (grp.IsMultiStanza) Then
                            ' #Yes: Add all related stanza for this group:
                            For j As Integer = i + 1 To core.nGroups
                                ' Get remaining group
                                grpTest = core.EcoPathGroupInputs(j)
                                ' Is of same stanza?
                                If (grpTest.iStanza = grp.iStanza) Then
                                    ' #Yes: add below current group
                                    lGroups.Add(grpTest)
                                    ' Remember that this group has been included already
                                    bIncluded(j) = True
                                End If
                            Next j
                            grpTest = Nothing
                        End If
                    End If
                Next i

                grp = Nothing
                Return lGroups.ToArray()

            End Get
        End Property

#End Region ' Item order

#Region " Visual Styles "

        Private m_abrDefaultGlyphs As Image() = { _
            My.Resources.glyph_blue1, _
            My.Resources.glyph_blue2, _
            My.Resources.glyph_blue3, _
            My.Resources.glyph_blue4, _
            My.Resources.glyph_blue5, _
            My.Resources.glyph_blue6, _
            My.Resources.glyph_blue7, _
            My.Resources.glyph_blue8, _
            My.Resources.glyph_blue9, _
            My.Resources.glyph_blue10, _
            My.Resources.glyph_deep_beige, _
            My.Resources.glyph_deep_blue, _
            My.Resources.glyph_deep_brown, _
            My.Resources.glyph_deep_bw, _
            My.Resources.glyph_deep_green, _
            My.Resources.glyph_muddy_blue, _
            My.Resources.glyph_muddy_brown, _
            My.Resources.glyph_muddy_bw, _
            My.Resources.glyph_muddy_green, _
            My.Resources.glyph_rubble_blue, _
            My.Resources.glyph_rubble_lightblue, _
            My.Resources.glyph_rubble_brown, _
            My.Resources.glyph_rubble_bw, _
            My.Resources.glyph_rubble_sand, _
            My.Resources.glyph_rubble_green, _
            My.Resources.glyph_seagrass_brown, _
            My.Resources.glyph_seagrass_bw, _
            My.Resources.glyph_seagrass_dark, _
            My.Resources.glyph_seagrass_red, _
            My.Resources.glyph_arrows_down, _
            My.Resources.glyph_arrows_up, _
            My.Resources.glyph_hl_fine_dblue, _
            My.Resources.glyph_hl_fine_blue, _
            My.Resources.glyph_hl_fine_lblue, _
            My.Resources.glyph_hl_fine_lgreen, _
            My.Resources.glyph_hl_fine_dgreen, _
            My.Resources.glyph_hl_fine_pink, _
            My.Resources.glyph_hl_fine_lorange, _
            My.Resources.glyph_hl_fine_orange, _
            My.Resources.glyph_hl_fine_red, _
            My.Resources.glyph_hl_med_blue, _
            My.Resources.glyph_hl_med_dblue, _
            My.Resources.glyph_vl_fine_dblue, _
            My.Resources.glyph_vl_fine_blue, _
            My.Resources.glyph_vl_fine_lblue, _
            My.Resources.glyph_vl_fine_lgreen, _
            My.Resources.glyph_vl_fine_dgreen, _
            My.Resources.glyph_vl_fine_pink, _
            My.Resources.glyph_vl_fine_lorange, _
            My.Resources.glyph_vl_fine_orange, _
            My.Resources.glyph_vl_fine_red, _
            My.Resources.glyph_vl_med_blue, _
            My.Resources.glyph_vl_med_dblue, _
            My.Resources.glyph_blocks_large, _
            My.Resources.glyph_blocks_small, _
            My.Resources.glyph_squares_large, _
            My.Resources.glyph_squares_small, _
            My.Resources.glyph_dots_large, _
            My.Resources.glyph_dots_small, _
            My.Resources.glyph_circles_large, _
            My.Resources.glyph_circles_small _
        }

        Private m_abrDefaultHatchPatterns As HatchStyle() = {HatchStyle.DiagonalCross, _
                                                  HatchStyle.Cross, _
                                                  HatchStyle.DiagonalBrick, _
                                                  HatchStyle.Divot, _
                                                  HatchStyle.LightHorizontal, _
                                                  HatchStyle.Shingle, _
                                                  HatchStyle.ZigZag, _
                                                  HatchStyle.SmallGrid, _
                                                  HatchStyle.DashedVertical, _
                                                  HatchStyle.Plaid}

        Private m_agrads As cARGBColorRamp() = {
            New cARGBColorRamp(New Color() {Color.FromArgb(250, 230, 230), Color.FromArgb(250, 24, 24)}, New Double() {0, 1}),
            New cARGBColorRamp(New Color() {Color.FromArgb(250, 250, 230), Color.FromArgb(250, 250, 24)}, New Double() {0, 1}),
            New cARGBColorRamp(New Color() {Color.FromArgb(230, 250, 230), Color.FromArgb(24, 250, 24)}, New Double() {0, 1}),
            New cARGBColorRamp(New Color() {Color.FromArgb(230, 250, 250), Color.FromArgb(24, 250, 250)}, New Double() {0, 1}),
            New cARGBColorRamp(New Color() {Color.FromArgb(230, 230, 250), Color.FromArgb(24, 24, 250)}, New Double() {0, 1}),
            New cARGBColorRamp(New Color() {Color.FromArgb(250, 230, 250), Color.FromArgb(240, 24, 250)}, New Double() {0, 1}),
            New cARGBColorRamp(New Color() {Color.FromArgb(230, 230, 230), Color.FromArgb(24, 24, 24)}, New Double() {0, 1}),
            New cARGBColorRamp(New Color() {Color.FromArgb(240, 240, 255), Color.LightBlue, Color.MediumPurple, Color.Purple}, New Double() {0, 1 / 3, 1 / 3, 1 / 3}),
            New cARGBColorRamp(New Color() {Color.FromArgb(240, 255, 240), Color.LightGreen, Color.DarkGreen}, New Double() {0, 1 / 3, 2 / 3}),
            New cARGBColorRamp(New Color() {Color.LightBlue, Color.DarkBlue}, New Double() {0, 1}),
            New cARGBColorRamp(New Color() {Color.LemonChiffon, Color.Orange, Color.SaddleBrown}, New Double() {0, 0.5, 0.5}),
            New cARGBColorRamp(New Color() {Color.LightYellow, Color.SaddleBrown}, New Double() {0, 1}),
            New cARGBColorRamp(New Color() {Color.DarkGreen, Color.White, Color.DarkRed}, New Double() {0, 0.5, 0.5}),
            New cARGBColorRamp(New Color() {Color.DarkGreen, Color.LightGreen, Color.White, Color.OrangeRed, Color.DarkRed}, New Double() {0, 0.25, 0.25, 0.25, 0.25}),
            New cARGBColorRamp(New Color() {Color.DarkGreen, Color.LightGreen, Color.White, Color.LightBlue, Color.DarkBlue}, New Double() {0, 1 / 3, 1 / 6, 1 / 6, 1 / 3}),
            New cARGBColorRamp(New Color() {Color.FromArgb(255, 0, 0), Color.FromArgb(255, 255, 0), Color.FromArgb(0, 255, 0), Color.FromArgb(0, 255, 255), Color.FromArgb(0, 0, 255), Color.FromArgb(255, 0, 255), Color.FromArgb(255, 0, 0)}, New Double() {0, 1 / 6, 1 / 6, 1 / 6, 1 / 6, 1 / 6, 1 / 6})
        }

        Private m_brHightLightDefault As Brush = Brushes.Red

        ''' <summary>Enumerated type providing supported types of brushes.</summary>
        Enum eBrushType As Integer
            ''' <summary>Items are rendered as a single colour.</summary>
            Color
            ''' <summary>Items are rendered as a hatch pattern.</summary>
            HatchPattern
            ''' <summary>Items are rendered as an image.</summary>
            Glyphs
            ''' <summary>Items are rendered as gradients.</summary>
            Gradient
        End Enum

        Public Function GetVisualStyles(ByVal nBrushes As Integer, _
                                        Optional ByVal brushType As eBrushType = eBrushType.Color) As cVisualStyle()
            Dim avs As cVisualStyle() = Nothing

            Select Case brushType
                Case eBrushType.Color
                    Debug.Assert(nBrushes >= 0)
                    ReDim avs(nBrushes)
                    Me.GetColors(avs)

                Case eBrushType.Glyphs
                    If (nBrushes <= 0) Then nBrushes = m_abrDefaultGlyphs.Length
                    ReDim avs(nBrushes)
                    Me.GetGlyphs(avs, m_abrDefaultGlyphs)

                Case eBrushType.HatchPattern
                    If (nBrushes <= 0) Then nBrushes = m_abrDefaultHatchPatterns.Length
                    ReDim avs(nBrushes)
                    Me.GetPatterns(avs, m_abrDefaultHatchPatterns)

                Case eBrushType.Gradient
                    If (nBrushes <= 0) Then nBrushes = m_agrads.Length
                    ReDim avs(nBrushes - 1)
                    Me.GetGradients(avs, m_agrads)

            End Select

            ' ok, done
            Return avs
        End Function

#Region " Internal implementation "

        Private Sub GetColors(ByVal avs() As cVisualStyle)

            Dim vs As cVisualStyle = Nothing
            Dim clrramp As New cEwEColorRamp()

            ' Loop through number of requested visual styles
            For i As Integer = 0 To avs.Length - 1
                ' Build visual style
                vs = New cVisualStyle()
                vs.ForeColour = clrramp.GetColor(i, avs.Length - 1)
                ' Store
                avs(i) = vs
            Next i
        End Sub

        Private Sub GetGlyphs(ByVal avs() As cVisualStyle, ByVal images() As Image)

            Dim vs As cVisualStyle = Nothing
            Dim iGlyphIndex As Integer = 0

            ' Loop through number of brushes
            For i As Integer = 0 To avs.Length - 1

                vs = New cVisualStyle()
                vs.ForeColour = Color.Gray
                vs.BackColour = Color.Transparent
                vs.Image = images(iGlyphIndex)

                avs(i) = vs

                ' increment counter
                iGlyphIndex += 1
                If iGlyphIndex = images.Length Then iGlyphIndex = 0
            Next i

        End Sub

        Private Sub GetPatterns(ByVal avs() As cVisualStyle, ByVal hatches As HatchStyle())

            Dim vs As cVisualStyle = Nothing
            Dim iPatternIndex As Integer = 0

            ' Loop through number of brushes
            For i As Integer = 0 To avs.Length - 1

                vs = New cVisualStyle()
                vs.HatchStyle = hatches(iPatternIndex)
                avs(i) = vs

                ' increment counter
                iPatternIndex += 1
                If iPatternIndex = hatches.Length Then iPatternIndex = 0
            Next i

        End Sub

        Private Sub GetGradients(ByVal avs() As cVisualStyle, ByVal ramps As cARGBColorRamp())

            Dim vs As cVisualStyle = Nothing
            Dim iPatternIndex As Integer = 0

            ' Loop through number of brushes
            For i As Integer = 0 To avs.Length - 1

                vs = New cVisualStyle()
                vs.GradientColors = ramps(i).GradientColors
                vs.GradientBreaks = ramps(i).GradientBreaks

                avs(i) = vs

                ' increment counter
                iPatternIndex += 1
                If iPatternIndex = ramps.Length Then iPatternIndex = 0
            Next i

        End Sub

#End Region ' Internal implementation
#End Region ' Visual styles

#Region " Images "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return a standard image for a given <see cref="eMessageImportance">importance level</see>.
        ''' </summary>
        ''' <param name="importance">The importance level to find the image for.</param>
        ''' <returns>A bitmap, or nothing if not applicable.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function GetImage(importance As eMessageImportance) As Bitmap
            Select Case importance
                Case eMessageImportance.Critical
                    Return My.Resources.Critical
                Case eMessageImportance.Warning
                    Return My.Resources.Warning
                Case eMessageImportance.Information
                    Return My.Resources.Info
                Case eMessageImportance.Question
                    Return My.Resources.Question
            End Select
            Return Nothing
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return an image of requested format, size, at <see cref="PreferredDPI">the preferred dpi</see>.
        ''' The extension of the optionally provided filename will be adjusted
        ''' to match the image format.
        ''' </summary>
        ''' <param name="format">The <see cref="ImageFormat">formats</see> of the image to create.</param>
        ''' <param name="size">The width and height of the image, in pixels.</param>
        ''' <param name="strFileName">Optional filename of the image.</param>
        ''' <returns>A bitmap, or nothing if not applicable.</returns>
        ''' -------------------------------------------------------------------
        Public Function GetImage(size As Size, format As ImageFormat, Optional ByRef strFileName As String = "") As Bitmap

            Return Me.GetImage(size.Width, size.Height, format, strFileName)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Return an image of requested format, size, at <see cref="PreferredDPI">the preferred dpi</see>
        ''' for writing to disk. The extension of the optionally provided filename 
        ''' will be adjusted to match the image format.
        ''' </summary>
        ''' <param name="format">The <see cref="ImageFormat">formats</see> of the image to create.</param>
        ''' <param name="width">The width of the image, in pixels.</param>
        ''' <param name="height">The height of the image, in pixels.</param>
        ''' <param name="strFileName">Optional filename of the image.</param>
        ''' <returns>A bitmap, or nothing if not applicable.</returns>
        ''' <remarks>
        ''' PNG images are 
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Function GetImage(width As Integer, height As Integer, format As ImageFormat, _
                                 Optional ByRef strFileName As String = "") As Bitmap

            Try

                Dim bmp As New Bitmap(width, height, PixelFormat.Format32bppArgb)
                bmp.SetResolution(Me.PreferredDPI, Me.PreferredDPI)

                Using g As Graphics = Graphics.FromImage(bmp)
                    If ((format Is ImageFormat.Gif) Or (format Is ImageFormat.Png)) Then
                        g.FillRectangle(Brushes.Transparent, 0, 0, width, height)
                    Else
                        g.FillRectangle(Brushes.White, 0, 0, width, height)
                    End If
                End Using

                If (Not String.IsNullOrWhiteSpace(strFileName)) Then
                    strFileName = Path.ChangeExtension(strFileName, format.ToString().ToLower)
                End If

                Return bmp
            Catch ex As Exception
                ' Some kind of error?!
                Debug.Assert(False)
            End Try
            Return Nothing

        End Function

        ''' <summary>
        ''' Get the system color to reflect spatial data set compatibility
        ''' </summary>
        ''' <param name="comp"></param>
        ''' <returns></returns>
        Public Shared Function GetColor(comp As SpatialData.cDatasetCompatilibity) As Color
            Select Case comp.Status
                Case eStatusFlags.Null
                    Return Color.LightGray
                Case eStatusFlags.ErrorEncountered
                    Return Color.OrangeRed
                Case eStatusFlags.MissingParameter
                    Return Color.Yellow
                Case eStatusFlags.OK
                    Return Color.LightGreen
                Case Else
                    Debug.Assert(False, "Status not supported")
            End Select
            Return Nothing

        End Function

#End Region ' Images

#Region " Image export settings "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Preferred image output format.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property PreferredImageFormat As Imaging.ImageFormat

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Preferred image density (Dots Per Inch).
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property PreferredDPI As Integer = 220

#End Region ' Image export settings

#Region " Labels, headers and menus "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a string to a Windows Forms Control label.
        ''' </summary>
        ''' <param name="strLabel"></param>
        ''' <param name="parent">Not used yet. At some point, the parent control needs
        ''' to be scanned for existing keyboard shortcuts to prevent duplicates.</param>
        ''' <remarks>Windows Forms control labels are formatted to sentence case.</remarks>
        ''' <returns>A string that can be used as a Windows Forms Control label.</returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToControlLabel(ByVal strLabel As String, _
                                              Optional ByVal parent As Control = Nothing, _
                                              Optional ByVal bAssignShortcut As Boolean = True) As String

            Dim sb As New StringBuilder()

            ' Strip out character sequences that this method will replenish
            If bAssignShortcut Then strLabel = strLabel.Replace("&", "")
            strLabel = strLabel.Replace(":", "")

            If cSystemUtils.IsRightToLeft() Then
                sb.Append(":")
                sb.Append(cStringUtils.ToSentenceCase(strLabel))
                If bAssignShortcut Then sb.Append("&")
            Else
                If bAssignShortcut Then sb.Append("&")
                sb.Append(cStringUtils.ToSentenceCase(strLabel))
                sb.Append(":")
            End If

            Return sb.ToString()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Convert a string to a Windows Forms menu item label.
        ''' </summary>
        ''' <param name="strLabel"></param>
        ''' <param name="parent">Not used yet. At some point, the parent meu needs
        ''' to be scanned for existing keyboard shortcuts to prevent duplicates.</param>
        ''' <remarks>Windows Forms menu labels are formatted to title case.</remarks>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Shared Function ToMenuLabel(ByVal strLabel As String, _
                                           Optional ByVal parent As MenuItem = Nothing, _
                                           Optional ByVal bAssignShortcut As Boolean = True) As String

            Dim sb As New StringBuilder()

            ' Strip out character sequences that this method will replenish
            If bAssignShortcut Then strLabel = strLabel.Replace("&", "")
            strLabel = strLabel.Replace("...", "")

            If cSystemUtils.IsRightToLeft() Then
                sb.Append("...")
                sb.Append(cStringUtils.ToSentenceCase(strLabel))
                If bAssignShortcut Then sb.Append("&")
            Else
                If bAssignShortcut Then sb.Append("&")
                sb.Append(cStringUtils.ToSentenceCase(strLabel))
                sb.Append("...")
            End If

            Return sb.ToString()

        End Function

#End Region ' Labels, headers and menus 

#Region " Ecobase fields "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the collection of values for a given <see cref="eEcobaseFieldType">
        ''' Ecobase field type</see>.
        ''' </summary>
        ''' <param name="ft">The <see cref="eEcobaseFieldType">EcoBase field type</see>
        ''' to filter by.</param>
        ''' -------------------------------------------------------------------
        Public Property EcoBaseFields(ft As eEcobaseFieldType) As StringCollection
            Get
                If (Me.m_dtEcoBaseFields.ContainsKey(ft)) Then
                    Return Me.m_dtEcoBaseFields(ft)
                End If
                Select Case ft
                    Case eEcobaseFieldType.CountryName
                        Return DefaultCountryNames()
                    Case eEcobaseFieldType.EcosystemType
                        Return DefaultEcosystemTypes()
                End Select
                Return New StringCollection()
            End Get
            Set(value As StringCollection)
                If (value Is Nothing) Then
                    If (Me.m_dtEcoBaseFields.ContainsKey(ft)) Then Me.m_dtEcoBaseFields.Remove(ft)
                Else
                    Me.m_dtEcoBaseFields(ft) = value
                End If
            End Set
        End Property

        Public Sub EcoBaseFieldsChanged()
            FireChangeEvent(eChangeType.EcobaseLists)
        End Sub

        Private Function DefaultCountryNames() As StringCollection

            Dim sc As New StringCollection()
            Dim lNames As New List(Of String)

            For Each ci As CultureInfo In CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                Dim ri As New RegionInfo(ci.Name)
                If (Not lNames.Contains(ri.EnglishName)) Then lNames.Add(ri.EnglishName)
            Next

            lNames.Sort()
            sc.AddRange(lNames.ToArray())
            Return sc

        End Function

        Private Function DefaultEcosystemTypes() As StringCollection

            ' Do NOT localize these strings; they serve as keys to EcoBase
            Dim names() As String = New String() {"Estuary", "Open ocean", "Coral reef", "Channel/strait", "Terrestrial", _
                                                  "Reservoir", "River", "Continental shelf", "Bay/fjord", _
                                                  "Coastal lagoon", "Upwelling", "Beach", "Fish farm", "Lake"}
            Dim sc As New StringCollection()
            sc.AddRange(names)
            Return sc

        End Function

#End Region ' Ecobase

#End Region ' Public access

#Region " Internal implementation "

        Private Function DefaultColor(ByVal colorType As eApplicationColorType) As Color
            Select Case colorType
                Case eApplicationColorType.DEFAULT_TEXT : Return Color.Black
                Case eApplicationColorType.DEFAULT_BACKGROUND : Return Color.White
                Case eApplicationColorType.NAMES_TEXT : Return Color.Black
                Case eApplicationColorType.NAMES_BACKGROUND : Return Color.FromArgb(255, 233, 245, 255)
                Case eApplicationColorType.HIGHLIGHT : Return Color.Orange
                Case eApplicationColorType.INVALIDMODELRESULT_TEXT : Return Color.DarkViolet
                Case eApplicationColorType.FAILEDVALIDATION_TEXT : Return Color.DarkGoldenrod
                Case eApplicationColorType.GENERICERROR_BACKGROUND : Return Color.OrangeRed
                Case eApplicationColorType.COMPUTED_TEXT : Return Color.FromArgb(255, 0, 0, 244)
                    'Case eApplicationColorType.FISHINGPRESSURE_TEXT : Return Color.Red
                    'Case eApplicationColorType.PROFIT_TEXT : Return Color.Blue
                    'Case eApplicationColorType.TOTALCATCH_TEXT : Return Color.LightCoral
                    'Case eApplicationColorType.TROPHICLINK_TEXT : Return Color.LavenderBlush
                Case eApplicationColorType.CHECKED_BACKGROUND : Return Color.Coral
                Case eApplicationColorType.REMARKS_BACKGROUND : Return Color.White
                Case eApplicationColorType.SUM_BACKGROUND : Return Color.FromArgb(255, 255, 254, 225)
                Case eApplicationColorType.READONLY_BACKGROUND : Return Color.FromArgb(255, 231, 235, 250)
                Case eApplicationColorType.MISSINGPARAMETER_BACKGROUND : Return Color.FromArgb(255, 182, 134, 221)
                Case eApplicationColorType.IMAGE_BACKGROUND : Return Color.White
                Case eApplicationColorType.PLOT_BACKGROUND : Return Color.White
                Case eApplicationColorType.MAP_BACKGROUND : Return Color.Azure
                Case eApplicationColorType.PEDIGREE : Return Color.Orange
                Case eApplicationColorType.PREDATOR : Return Color.Red
                Case eApplicationColorType.PREY : Return Color.Green
                Case eApplicationColorType.NotSet
                    Return Color.Transparent
            End Select
            ' This should not happen, a default should always be available
            Debug.Assert(False)
            Return Color.Black
        End Function

        Private Function DefaultFontFamilyName(ByVal ft As eApplicationFontType) As String
            Return "Microsoft Sans Serif"
        End Function

        Private Function DefaultFontStyle(ByVal ft As eApplicationFontType) As FontStyle
            Return Drawing.FontStyle.Regular
        End Function

        Private Function DefaultFontSize(ByVal ft As eApplicationFontType) As Single
            Select Case ft
                Case eApplicationFontType.Title
                    Return 12
                Case eApplicationFontType.Legend, eApplicationFontType.SubTitle
                    Return 10
                Case eApplicationFontType.Scale
                    Return 8.25
                Case Else
                    Debug.Assert(False)
            End Select
            Return -1
        End Function

        Private Function DefaultShapeColor(ByVal shapetype As eDataTypes) As Color
            Select Case shapetype
                Case eDataTypes.Forcing : Return Color.FromArgb(255, 236, 55, 12)
                Case eDataTypes.EggProd : Return Color.Orange
                Case eDataTypes.CapacityMediation : Return Drawing.Color.SandyBrown
                Case eDataTypes.FishingEffort : Return Drawing.Color.Coral
                Case eDataTypes.FishMort : Return Color.DarkGray
                Case eDataTypes.PriceMediation : Return Color.FromArgb(255, 41, 233, 41)
                Case eDataTypes.Mediation : Return Color.FromArgb(255, 81, 133, 255)
                Case eDataTypes.GroupTimeSeries, eDataTypes.FleetTimeSeries : Return Color.DarkGreen
                Case Else
                    Debug.Assert(False)
            End Select
            ' Unknown!
            Return Color.Magenta
        End Function

#End Region ' Internal implementation

    End Class

End Namespace
