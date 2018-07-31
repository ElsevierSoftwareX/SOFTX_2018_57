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

Option Explicit On
Option Strict On

Namespace Definitions

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type for identifing modifications to a list of items in the user interface, prior to
    ''' updating the list in a batch operation.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eItemStatusTypes As Integer
        ''' <summary>Item belongs to the original list.</summary>
        Original = 0
        ''' <summary>Item is flagged to be added to the list.</summary>
        Added
        ''' <summary>Item is flagged for removal from the list.</summary>
        Removed
        ''' <summary>Item does not belong to the list.</summary>
        Invalid
    End Enum

    Public Enum eMCRunDisplayInputValueTypes As Integer
        B = 0
        PB
        QB
        EE
        BA
        BaBi
        VU
        Landings
        Discards
        Diets
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type that states how line graphs will be rendered.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eSketchDrawModeTypes As Integer
        NotSet
        ''' <summary>The area under a graph will be filled.</summary>
        Fill
        ''' <summary>A graph will be rendered as a line.</summary>
        Line
        ''' <summary>A graph will be rendered as driver time series.</summary>
        TimeSeriesDriver
        ''' <summary>A graph will be rendered as reference time series with relative values.</summary>
        TimeSeriesRefAbs
        ''' <summary>A graph will be rendered as reference time series with absolute values.</summary>
        TimeSeriesRefRel
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type that states how graph tick marks will be scaled.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eAxisTickmarkDisplayModeTypes As Integer
        ''' <summary>Tick marks will be only be displayed for the range with values on an axis.</summary>
        Relative
        ''' <summary>Tick marks will display the full (absolute) range of values on an axis.</summary>
        Absolute
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type that states how the values on a graph axis will be scaled.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eAxisAutoScaleModeTypes As Integer
        ''' <summary>A graph will automatically scale its axis to show the range of values on an axis.</summary>
        Auto
        ''' <summary>A graph will not scale its axis to show the range of values on an axis.</summary>
        Fixed
    End Enum

    Public Enum eRightClickAutoScaleModeTypes As Integer
        Auto
        Fixed
    End Enum

    Public Enum eTracerRunModeTypes As Integer
        Disabled = 0
        RunSim
        RunSpace
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type for identifying the broad categories of time shapes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eShapeCategoryTypes As Integer
        NotSet
        ''' <summary>Shape is a <see cref="EwECore.cForcingFunction">Forcing shape</see>.</summary>
        Forcing
        ''' <summary>Shape is a <see cref="EwECore.cMediationFunction">Mediation shape</see>.</summary>
        Mediation
        ''' <summary>Shape is a Egg production shape.</summary>
        EggProduction
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Supported plot types.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum ePlotTypes As Byte
        ''' <summary>Plot values as a histogram.</summary>
        Histogram = 0
        ''' <summary>Plot values as individual lines for a groups and fleets.</summary>
        Values
        ''' <summary>Plot values as one line per group or fleet.</summary>
        Line
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Supported layer edit operations (bit flags)
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eLayerEditTypes As Byte
        ''' <summary>Edit layer data.</summary>
        EditData = &H1
        ''' <summary>Edit layer visual representation.</summary>
        EditVisuals = &H2
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Types of global, EwE-wide options to configure.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eApplicationOptionTypes As Integer
        ''' <summary>General options.</summary>
        General = 0
        ''' <summary>Colour options.</summary>
        Colours
        ''' <summary>Font options.</summary>
        Fonts
        ''' <summary>Graph options.</summary>
        Graphs
        ''' <summary>Map options.</summary>
        ReferenceMaps
        ''' <summary>Plug-in options.</summary>
        Plugins
        ''' <summary>Message history options.</summary>
        Messages
        ''' <summary>Autosave options.</summary>
        Autosave
        ''' <summary>File location options.</summary>
        FileLocations = Autosave
        ''' <summary>Presentation mode options.</summary>
        PresentationMode
        ''' <summary>EwE main window options.</summary>
        Window
        ''' <summary>Spatial-temporal options.</summary>
        SpatialTemporal
    End Enum

    Public Enum eGroupFilter As Integer
        NotSet = 0
        Consumer
        Producer
        Detritus
    End Enum

    Public Enum eFDNodeTypes As Integer
        Circle = 0
        Rectangle = 1
    End Enum

    Public Enum eFDColorUsageTypes As Integer
        None
        EwE
        Value
        Flow
        TrophicLevel
    End Enum

    Public Enum eFDShowHiddenType As Integer
        GrayedOut
        Invisible
    End Enum

    Public Enum eFDConnectionType As Integer
        Straight = 1
        Arch = 2
    End Enum

    Public Enum eFDNodeValueType As Integer
        Biomass
        Production
    End Enum

    Public Enum eFDNodeScaleType As Integer
        Logarithmic
        SquareRoot
        Linear
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type determining how map layers are drawn.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eLayerRenderType As Integer
        ''' <summary>A map layer always renders.</summary>
        Always = 0
        ''' <summary>Only the map layer that is selected is rendered.</summary>
        Selected
        ''' <summary>All map layers of the same type that is selected are rendered.</summary>
        Grouped
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumerated type, indicating how the user wishes to configure capacity drivers.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Enum eEnvironmentalResponseSelectionType
        ''' <summary>Dialog was invoked for a specific driver / group combination.</summary>
        DriverGroup
        ''' <summary>Dialog was invoked for all groups and a single driver.</summary>
        Driver
    End Enum

End Namespace

