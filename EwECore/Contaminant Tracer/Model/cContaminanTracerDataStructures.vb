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

#End Region ' Imports

''' <summary>
''' Public variables need by the <see cref="cContaminantTracer">contaminant tracing model</see>. 
''' </summary>
Public Class cContaminantTracerDataStructures

    'EwE5 ReadEcoTracer() for variables that are saved to database
    Public m_nGroups As Integer = 0
    Public m_nTime As Integer = 0
    Public m_nRegions As Integer = 0

    ''' <summary>
    ''' Decay rate per year
    ''' </summary>
    ''' <remarks>
    ''' The zero element holds the Enviroment value 
    ''' 1 to ngroups is for the groups
    ''' </remarks>
    Public cdecay() As Single

    ''' <summary>
    ''' Initial concentration of contaminants in t/km2
    ''' </summary>
    ''' <remarks>
    ''' The zero element holds the Environment value 
    ''' 1 to ngroups is for the groups
    ''' </remarks>
    Public Czero() As Single

    ''' <summary>
    ''' Base inflow rate to environment t/km2/year in Zero element
    ''' </summary>
    Public Cinflow() As Single

    ''' <summary>
    ''' Absorption rate t/t/t/year (concentration in prey/consumption/year ???)
    ''' </summary>
    Public Cenv() As Single

    ''' <summary>
    ''' Concentration in immigrating groups t/t
    ''' </summary>
    Public Cimmig() As Single

    ''' <summary>
    ''' Volume exchange loss rate zero element for environment
    ''' </summary>
    Public CoutFlow() As Single

    ''' <summary>
    ''' Pool excretion rate.
    ''' </summary>
    ''' <remarks>
    ''' <para>C.J.Walters, per email 25Nov2007:</para>
    ''' <para>
    ''' One way to improve ecotracer would be to add a pool-specific instantaneous
    ''' excretion rate, with the excretion flow going back into the 0 (environment)
    ''' pool. Such flows should be very low for lipophilic compounds, close to the
    ''' metabolic rate for compounds that are lost in proportion to routine tissue
    ''' metabolism (eg conversion of carbon 14 organic form to c14-02).
    ''' </para>
    ''' <para>Villy, per email 7Jun2016:</para>
    ''' <para>
    ''' CexcretionRate is renamed in the database to: CassimProp, (so existing 
    ''' models don’t need to be changed).
    ''' </para>
    ''' </remarks>
    Public CassimProp() As Single

    ''' <summary>
    ''' Added cMetabolismRate (unit: per year)
    ''' </summary>
    Public CmetabolismRate() As Single

    Public ConForceNumber As Integer

    ''' <summary>
    ''' Plot contr/biomass
    ''' </summary>
    ''' <remarks></remarks>
    Public TracePlotCB As Boolean

    ''' <summary>Results over time (group x time step)</summary>
    Public TracerConc(,) As Single
    ''' <summary>Concentration over time by region (region x group x time).</summary>
    Public TracerConcByRegion(,,) As Single
    ''' <summary>Concentration over biomass (region x group x time).</summary>
    Public TracerCBRegion(,,) As Single

    Public TracerCB(,) As Single

    ''' <summary>Max concentration of contaminant at the current time step by group</summary>
    Public ConcMax() As Single

    ''' <summary>Ecosim tracer enabled state flag.</summary>
    Friend EcoSimConSimOn As Boolean
    ''' <summary>Ecospace tracer enabled state flag.</summary>
    Friend m_bEcoSpaceConSimOn As Boolean

    Public MaxTimeSteps As Integer

    Public Sub New()
        MaxTimeSteps = 1000
    End Sub

    Friend Sub RedimByNGroups(ByVal nGroups As Integer)
        Me.m_nGroups = nGroups
        ReDim Czero(nGroups)
        ReDim Cimmig(nGroups)
        ReDim Cenv(nGroups)
        ReDim cdecay(nGroups)
        ReDim Cinflow(nGroups)
        ReDim CoutFlow(nGroups)
        ReDim CassimProp(nGroups)
        ReDim CmetabolismRate(nGroups)
    End Sub

    Public Sub redimForEcosimRun(ByVal nGroups As Integer, ByVal nTime As Integer)
        Me.m_nTime = nTime
        Me.m_nRegions = 0
        ReDim TracerConc(nGroups + 1, nTime)
        ReDim TracerCB(nGroups + 1, nTime)

    End Sub

    Public Sub redimForEcospaceRun(ByVal nRegions As Integer, ByVal nGroups As Integer, ByVal nTime As Integer)
        Me.m_nTime = nTime
        Me.m_nRegions = nRegions
        ReDim TracerConcByRegion(nRegions, nGroups + 1, nTime)
        ReDim TracerCBRegion(nRegions, nGroups + 1, nTime)
    End Sub

    Public Sub Clear()
        Me.m_nGroups = 0

        TracerConc = Nothing '(nGroups + 1, nTime)
        TracerCB = Nothing
        TracerConcByRegion = Nothing '(nRegions, nGroups + 1, nTime)
        TracerCBRegion = Nothing '(nRegions, nGroups + 1, nTime)

        Czero = Nothing '(nGroups)
        Cimmig = Nothing '(nGroups)
        Cenv = Nothing '(nGroups)
        cdecay = Nothing '(nGroups)
        Cinflow = Nothing '(nGroups)
        CoutFlow = Nothing '(nGroups)
        CassimProp = Nothing '(nGroups)

        'Me.RedimByNGroups(0)
    End Sub

    Public Sub CopyTo(ByRef d As cContaminantTracerDataStructures)
        'EwE5 ReadEcoTracer() for variables that are saved to database

        d.cdecay = CType(cdecay.Clone, Single())
        d.Czero = CType(Czero.Clone, Single())
        d.Cinflow = CType(Cinflow.Clone, Single())
        d.Cenv = CType(Cenv.Clone, Single())
        d.Cimmig = CType(Cimmig.Clone, Single())
        d.CoutFlow = CType(CoutFlow.Clone, Single())
        d.CassimProp = CType(CassimProp.Clone, Single())
        d.ConForceNumber = ConForceNumber
        d.EcoSimConSimOn = EcoSimConSimOn
        d.EcoSpaceConSimOn = EcoSpaceConSimOn

        d.CmetabolismRate = Me.CmetabolismRate

    End Sub

    'Public Property EcoSimConSimOn() As Boolean
    '    Get
    '        Return Me.m_bEcoSimConSimOn
    '        'Return ((Me.m_bEcoSimConSimOn = True) And (Me.m_nGroups > 0))
    '    End Get
    '    Set(ByVal value As Boolean)
    '        m_bEcoSimConSimOn = value
    '    End Set
    'End Property


    Public Property EcoSpaceConSimOn() As Boolean
        Get
            Return m_bEcoSpaceConSimOn
            'jb this is not valid for Ecospace
            'because of the mulithreading Ecospace gets a copy of the data initialized be the database
            'RedimByNGroups() is never called on the actual object so m_nGroups is never set
            '  Return ((Me.m_bEcoSpaceConSimOn = True) And (Me.m_nGroups > 0))
        End Get
        Set(ByVal value As Boolean)
            m_bEcoSpaceConSimOn = value
        End Set
    End Property


End Class



