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

Imports System.Text
Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' On-board helper class that actively updates model-derived settings in the style guide.
''' </summary>
''' -----------------------------------------------------------------------
Friend Class cStyleGuideUpdater

#Region " Private vars "

    Private m_uic As cUIContext = Nothing
    Private m_bIsEcopathLoaded As Boolean = False

    Private m_sm As cCoreStateMonitor = Nothing
    Private m_propNumDigits As cProperty = Nothing
    Private m_propGroupDigits As cProperty = Nothing
    Private m_propUnitTime As cIntegerProperty = Nothing
    Private m_propUnitTimeText As cStringProperty = Nothing
    Private m_propUnitCurrency As cIntegerProperty = Nothing
    Private m_propUnitCurrencyText As cStringProperty = Nothing
    Private m_propUnitMonetary As cStringProperty = Nothing

#End Region ' Private vars

    Public Sub New(ByVal uic As cUIContext)

        ' Sanity check
        Debug.Assert(uic IsNot Nothing)

        Me.m_uic = uic
        Me.m_sm = Me.m_uic.Core.StateMonitor

        AddHandler m_sm.CoreExecutionStateEvent, AddressOf OnCoreStateEvent

    End Sub

    Private Sub OnCoreStateEvent(ByVal csm As cCoreStateMonitor)
        If Me.m_bIsEcopathLoaded <> csm.HasEcopathLoaded Then
            Me.m_bIsEcopathLoaded = csm.HasEcopathLoaded
            Me.Update()
        End If
    End Sub

    Private ReadOnly Property Core() As cCore
        Get
            Return Me.m_uic.Core
        End Get
    End Property

    Private ReadOnly Property StyleGuide() As cStyleGuide
        Get
            Return Me.m_uic.StyleGuide
        End Get
    End Property

    Private Sub Update()

        Dim pm As cPropertyManager = Me.m_uic.PropertyManager

        Me.StyleGuide.SuspendEvents()

        If Me.m_bIsEcopathLoaded Then

            Me.m_propGroupDigits = pm.GetProperty(Core.EwEModel, eVarNameFlags.GroupDigits)
            Me.m_propNumDigits = pm.GetProperty(Core.EwEModel, eVarNameFlags.NumDigits)
            AddHandler Me.m_propGroupDigits.PropertyChanged, AddressOf OnNumberFormatChanged
            AddHandler Me.m_propNumDigits.PropertyChanged, AddressOf OnNumberFormatChanged

            Me.m_propUnitCurrency = DirectCast(pm.GetProperty(Core.EwEModel, eVarNameFlags.UnitCurrency), cIntegerProperty)
            Me.m_propUnitCurrencyText = DirectCast(pm.GetProperty(Core.EwEModel, eVarNameFlags.UnitCurrencyCustomText), cStringProperty)
            AddHandler Me.m_propUnitCurrency.PropertyChanged, AddressOf OnCurrencyUnitChanged
            AddHandler Me.m_propUnitCurrencyText.PropertyChanged, AddressOf OnCurrencyUnitChanged

            Me.m_propUnitTime = DirectCast(pm.GetProperty(Core.EwEModel, eVarNameFlags.UnitTime), cIntegerProperty)
            Me.m_propUnitTimeText = DirectCast(pm.GetProperty(Core.EwEModel, eVarNameFlags.UnitTimeCustomText), cStringProperty)
            AddHandler Me.m_propUnitTime.PropertyChanged, AddressOf OnTimeUnitChanged
            AddHandler Me.m_propUnitTimeText.PropertyChanged, AddressOf OnTimeUnitChanged

            Me.m_propUnitMonetary = DirectCast(pm.GetProperty(Core.EwEModel, eVarNameFlags.UnitMonetary), cStringProperty)
            AddHandler Me.m_propUnitMonetary.PropertyChanged, AddressOf OnMonetaryUnitChanged

            Me.OnCurrencyUnitChanged(m_propUnitCurrency, cProperty.eChangeFlags.All)
            Me.OnTimeUnitChanged(m_propUnitTime, cProperty.eChangeFlags.All)
            Me.OnMonetaryUnitChanged(m_propUnitMonetary, cProperty.eChangeFlags.All)
            Me.OnNumberFormatChanged(m_propNumDigits, cProperty.eChangeFlags.All)

        Else

            RemoveHandler Me.m_propNumDigits.PropertyChanged, AddressOf OnNumberFormatChanged
            RemoveHandler Me.m_propGroupDigits.PropertyChanged, AddressOf OnNumberFormatChanged
            Me.m_propNumDigits = Nothing
            Me.m_propGroupDigits = Nothing

            RemoveHandler Me.m_propUnitCurrency.PropertyChanged, AddressOf OnCurrencyUnitChanged
            RemoveHandler Me.m_propUnitCurrencyText.PropertyChanged, AddressOf OnCurrencyUnitChanged
            Me.m_propUnitCurrency = Nothing
            Me.m_propUnitCurrencyText = Nothing

            RemoveHandler Me.m_propUnitTime.PropertyChanged, AddressOf OnTimeUnitChanged
            RemoveHandler Me.m_propUnitTimeText.PropertyChanged, AddressOf OnTimeUnitChanged
            Me.m_propUnitTime = Nothing
            Me.m_propUnitTimeText = Nothing

            RemoveHandler Me.m_propUnitMonetary.PropertyChanged, AddressOf OnMonetaryUnitChanged
            Me.m_propUnitMonetary = Nothing

        End If

        Me.StyleGuide.ResetVisibleFlags(False)
        Me.StyleGuide.ResumeEvents()

    End Sub

    Private Sub OnCurrencyUnitChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)
        With Me.StyleGuide
            .SuspendEvents()
            .CurrencyUnit = DirectCast(Me.m_propUnitCurrency.GetValue(), eUnitCurrencyType)
            .CustomCurrencyUnitText = CStr(Me.m_propUnitCurrencyText.GetValue())
            .ResumeEvents()
        End With
    End Sub

    Private Sub OnTimeUnitChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)
        With Me.StyleGuide
            .SuspendEvents()
            .TimeUnit = DirectCast(Me.m_propUnitTime.GetValue(), eUnitTimeType)
            .CustomTimeUnitText = CStr(Me.m_propUnitTimeText.GetValue())
            .ResumeEvents()
        End With
    End Sub

    Private Sub OnMonetaryUnitChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)
        With Me.StyleGuide
            .SuspendEvents()
            .MonetaryUnit = DirectCast(Me.m_propUnitMonetary.GetValue(), String)
            .ResumeEvents()
        End With
    End Sub

    Private Sub OnNumberFormatChanged(ByVal prop As cProperty, ByVal ct As cProperty.eChangeFlags)
        With Me.StyleGuide
            .SuspendEvents()
            .NumDigits = CInt(Me.m_propNumDigits.GetValue())
            .GroupDigits = CBool(Me.m_propGroupDigits.GetValue())
            .ResumeEvents()
        End With
    End Sub

    ''' <summary>
    ''' Load the style guide from application settings
    ''' </summary>
    Public Sub Load()

        With Me.StyleGuide

            .SuspendEvents()

            .ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_TEXT) = My.Settings.ColorDefaultText
            .ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_BACKGROUND) = My.Settings.ColorDefaultBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.NAMES_TEXT) = My.Settings.ColorNameText
            .ApplicationColor(cStyleGuide.eApplicationColorType.NAMES_BACKGROUND) = My.Settings.ColorNameBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.INVALIDMODELRESULT_TEXT) = My.Settings.ColorFailedResultText
            .ApplicationColor(cStyleGuide.eApplicationColorType.FAILEDVALIDATION_TEXT) = My.Settings.ColorFailedValidationText
            .ApplicationColor(cStyleGuide.eApplicationColorType.GENERICERROR_BACKGROUND) = My.Settings.ColorErrorBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.COMPUTED_TEXT) = My.Settings.ColorComputedValuesText
            .ApplicationColor(cStyleGuide.eApplicationColorType.REMARKS_BACKGROUND) = My.Settings.ColorRemarksBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.SUM_BACKGROUND) = My.Settings.ColorSumBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.READONLY_BACKGROUND) = My.Settings.ColorReadOnlyBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.CHECKED_BACKGROUND) = My.Settings.ColorCheckedBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.MISSINGPARAMETER_BACKGROUND) = My.Settings.ColorMissingParamBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.IMAGE_BACKGROUND) = My.Settings.ColorImageBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.PLOT_BACKGROUND) = My.Settings.ColorPlotsBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.MAP_BACKGROUND) = My.Settings.ColorMapBackground
            .ApplicationColor(cStyleGuide.eApplicationColorType.PREDATOR) = My.Settings.ColorPredator
            .ApplicationColor(cStyleGuide.eApplicationColorType.PREY) = My.Settings.ColorPrey
            .ApplicationColor(cStyleGuide.eApplicationColorType.PEDIGREE) = My.Settings.ColorPedigree

            .ThumbnailSize = My.Settings.ThumbnailSize
            ' Fix: do not allow disabling of legend viz
            If (My.Settings.ShowLegends = TriState.False) Then My.Settings.ShowLegends = TriState.UseDefault
            .ShowLegends = DirectCast(My.Settings.ShowLegends, TriState)
            .ShowPedigree = My.Settings.ShowPedigree
            .UseTransparentBackgrounds = My.Settings.UseTransparentBackgrounds

            .MapReferenceLayerFile = My.Settings.MapLayerRefFile
            .MapReferenceLayerTL = New PointF(My.Settings.MapLayerRefLonMin, My.Settings.MapLayerRefLatMax)
            .MapReferenceLayerBR = New PointF(My.Settings.MapLayerRefLonMax, My.Settings.MapLayerRefLatMin)
            .ShowMapsExcludedCells = My.Settings.MapShowExcludedCells
            .ShowMapsMPAs = My.Settings.MapShowMPAs
            .ShowMapLabels = My.Settings.MapShowLabels
            .ShowMapsDateInLabels = My.Settings.MapShowLabelDate
            .InvertMapLabelColor = My.Settings.MapShowLabelInvertedColor
            .MapLabelPosHorizontal = CType(My.Settings.MapLabelPosHorz, StringAlignment)
            .MapLabelPosVertical = CType(My.Settings.MapLabelPosVert, StringAlignment)
            .UseHabitatAreaCorrection = My.Settings.UseHabitatAreaCorrection

            .PreferredDPI = My.Settings.OutputDPI

            .EcoBaseFields(cStyleGuide.eEcobaseFieldType.CountryName) = My.Settings.CountryNames
            .EcoBaseFields(cStyleGuide.eEcobaseFieldType.EcosystemType) = My.Settings.EcosystemTypes
        End With

        Me.StringToFontSetting(My.Settings.FontTitle, cStyleGuide.eApplicationFontType.Title)
        Me.StringToFontSetting(My.Settings.FontSubtitle, cStyleGuide.eApplicationFontType.SubTitle)
        Me.StringToFontSetting(My.Settings.FontLegend, cStyleGuide.eApplicationFontType.Legend)

        Me.StringToFontSetting(My.Settings.FontScale, cStyleGuide.eApplicationFontType.Scale)

        Me.StyleGuide.ResumeEvents()

    End Sub

    Public Sub Save()

        With Me.StyleGuide

            My.Settings.ColorDefaultText = .ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_TEXT)
            My.Settings.ColorDefaultBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_BACKGROUND)
            My.Settings.ColorNameText = .ApplicationColor(cStyleGuide.eApplicationColorType.NAMES_TEXT)
            My.Settings.ColorNameBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.NAMES_BACKGROUND)
            My.Settings.ColorFailedResultText = .ApplicationColor(cStyleGuide.eApplicationColorType.INVALIDMODELRESULT_TEXT)
            My.Settings.ColorFailedValidationText = .ApplicationColor(cStyleGuide.eApplicationColorType.FAILEDVALIDATION_TEXT)
            My.Settings.ColorErrorBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.GENERICERROR_BACKGROUND)
            My.Settings.ColorComputedValuesText = .ApplicationColor(cStyleGuide.eApplicationColorType.COMPUTED_TEXT)
            My.Settings.ColorRemarksBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.REMARKS_BACKGROUND)
            My.Settings.ColorSumBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.SUM_BACKGROUND)
            My.Settings.ColorReadOnlyBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.READONLY_BACKGROUND)
            My.Settings.ColorCheckedBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.CHECKED_BACKGROUND)
            My.Settings.ColorMissingParamBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.MISSINGPARAMETER_BACKGROUND)
            My.Settings.ColorImageBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.IMAGE_BACKGROUND)
            My.Settings.ColorPlotsBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.PLOT_BACKGROUND)
            My.Settings.ColorMapBackground = .ApplicationColor(cStyleGuide.eApplicationColorType.MAP_BACKGROUND)
            My.Settings.ColorPredator = .ApplicationColor(cStyleGuide.eApplicationColorType.PREDATOR)
            My.Settings.ColorPrey = .ApplicationColor(cStyleGuide.eApplicationColorType.PREY)
            My.Settings.ColorPedigree = .ApplicationColor(cStyleGuide.eApplicationColorType.PEDIGREE)

            My.Settings.ThumbnailSize = .ThumbnailSize
            My.Settings.ShowLegends = .ShowLegends
            My.Settings.ShowPedigree = .ShowPedigree
            My.Settings.UseTransparentBackgrounds = .UseTransparentBackgrounds

            My.Settings.MapLayerRefFile = .MapReferenceLayerFile
            My.Settings.MapLayerRefLonMin = .MapReferenceLayerTL.X
            My.Settings.MapLayerRefLonMax = .MapReferenceLayerBR.X
            My.Settings.MapLayerRefLatMin = .MapReferenceLayerBR.Y
            My.Settings.MapLayerRefLatMax = .MapReferenceLayerTL.Y
            My.Settings.MapShowExcludedCells = .ShowMapsExcludedCells
            My.Settings.MapShowMPAs = .ShowMapsMPAs
            My.Settings.MapShowLabels = .ShowMapLabels
            My.Settings.MapShowLabelDate = .ShowMapsDateInLabels
            My.Settings.MapLabelPosHorz = .MapLabelPosHorizontal
            My.Settings.MapLabelPosVert = .MapLabelPosVertical
            My.Settings.MapShowLabelInvertedColor = .InvertMapLabelColor

            My.Settings.UseHabitatAreaCorrection = .UseHabitatAreaCorrection

            My.Settings.CountryNames = .EcoBaseFields(cStyleGuide.eEcobaseFieldType.CountryName)
            My.Settings.EcosystemTypes = .EcoBaseFields(cStyleGuide.eEcobaseFieldType.EcosystemType)

            My.Settings.OutputDPI = .PreferredDPI

        End With

        My.Settings.FontTitle = Me.FontSettingToString(cStyleGuide.eApplicationFontType.Title)
        My.Settings.FontSubtitle = Me.FontSettingToString(cStyleGuide.eApplicationFontType.SubTitle)
        My.Settings.FontLegend = Me.FontSettingToString(cStyleGuide.eApplicationFontType.Legend)
        My.Settings.FontScale = Me.FontSettingToString(cStyleGuide.eApplicationFontType.Scale)

    End Sub

    Private Sub StringToFontSetting(ByVal strSetting As String, ByVal ft As cStyleGuide.eApplicationFontType)

        Dim astrBits As String() = strSetting.Split(","c)
        If astrBits.Length >= 1 Then
            Try
                Me.StyleGuide.FontFamilyName(ft) = astrBits(0)
            Catch ex As Exception
                Me.StyleGuide.FontFamilyName(ft) = ""
            End Try
        End If
        If astrBits.Length >= 2 Then
            Try
                Me.StyleGuide.FontStyle(ft) = DirectCast(CInt(astrBits(1)), FontStyle)
            Catch ex As Exception
                Me.StyleGuide.FontStyle(ft) = FontStyle.Regular
            End Try
        End If
        If astrBits.Length >= 3 Then
            Try
                Me.StyleGuide.FontSize(ft) = cStringUtils.ConvertToSingle(astrBits(2))
            Catch ex As Exception
                Me.StyleGuide.FontSize(ft) = 0.0!
            End Try
        End If
    End Sub

    Private Function FontSettingToString(ByVal ft As cStyleGuide.eApplicationFontType) As String

        Dim sb As New StringBuilder()
        sb.Append(Me.StyleGuide.FontFamilyName(ft))
        sb.Append(",")
        sb.Append(CInt(Me.StyleGuide.FontStyle(ft)))
        sb.Append(",")
        sb.Append(cStringUtils.FormatSingle(Me.StyleGuide.FontSize(ft)))
        Return sb.ToString()

    End Function

End Class

