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
Imports EwEUtils.Core

''' <summary>
''' This class wraps the underlying particle size distribution data structures
''' </summary>
Public Class cPSDDatastructures

    Public EcopathDS As cEcopathDataStructures = Nothing

    ''' <summary>States whether PSD model is enabled.</summary>
    Public Enabled As Boolean = False

    Public NAgeSteps As Integer = 101
    Public MortalityType As ePSDMortalityTypes = ePSDMortalityTypes.GroupZ
    Public ClimateType As eClimateTypes = eClimateTypes.Tropical
    Public NWeightClasses As Integer = 25
    Public FirstWeightClass As Single = 0.125
    Public NPtsMovAvg As Integer = 0
    ''' <summary>Flag (x group) stating whether groups are included in PSD analysis.</summary>
    Public Include() As Boolean

    Public BiomassAvgSzWt() As Single
    Public BiomassSzWt() As Single
    Public AinLW() As Single
    Public BinLW() As Single
    Public Loo() As Single
    Public Winf() As Single
    Public t0() As Single
    Public Tcatch() As Single
    Public Tmax() As Single

    Public AinLWInput() As Single
    Public BinLWInput() As Single
    Public LooInput() As Single
    Public WinfInput() As Single
    Public t0Input() As Single
    Public TcatchInput() As Single
    Public TmaxInput() As Single

    Public EcopathWeight(,) As Single
    Public EcopathNumber(,) As Single
    Public EcopathBiomass(,) As Single
    Public LorenzenMortality(,) As Single
    Public PSD(,) As Single

    Public Sub New(ByVal EcopathDS As cEcopathDataStructures)
        Me.EcopathDS = EcopathDS
    End Sub

    Public Sub Clear()
        Me.BiomassAvgSzWt = Nothing ' (NumGroups)
        Me.BiomassSzWt = Nothing ' (NumGroups)
        Me.AinLW = Nothing ' (NumGroups)
        Me.BinLW = Nothing ' (NumGroups)
        Me.Loo = Nothing ' (NumGroups)
        Me.Winf = Nothing ' (NumGroups)
        Me.t0 = Nothing ' (NumGroups)
        Me.Tcatch = Nothing ' (NumGroups)
        Me.Tmax = Nothing ' (NumGroups)

        Me.AinLWInput = Nothing ' (NumGroups)
        Me.BinLWInput = Nothing ' (NumGroups)
        Me.LooInput = Nothing ' (NumGroups)
        Me.WinfInput = Nothing ' (NumGroups)
        Me.t0Input = Nothing ' (NumGroups)
        Me.TcatchInput = Nothing ' (NumGroups)
        Me.TmaxInput = Nothing ' (NumGroups)

        Me.EcopathWeight = Nothing ' (NumGroups, NAgeSteps)
        Me.EcopathNumber = Nothing ' (NumGroups, NAgeSteps)
        Me.EcopathBiomass = Nothing ' (NumGroups, NAgeSteps)
        Me.LorenzenMortality = Nothing ' (NumGroups, NAgeSteps)

        Me.PSD = Nothing ' (NumGroups, NWeightClasses)
        Me.Include = Nothing ' (NumGroups)
    End Sub

    Public ReadOnly Property NumGroups() As Integer
        Get
            Return Me.EcopathDS.NumGroups
        End Get
    End Property

    Public ReadOnly Property NumLiving() As Integer
        Get
            Return Me.EcopathDS.NumLiving
        End Get
    End Property

    ''' <summary>
    ''' redimension array variables 
    ''' called when a new model is loaded
    ''' </summary>
    ''' <returns>True if no error</returns>
    Public Function redimGroupVariables() As Boolean

        ReDim BiomassAvgSzWt(NumGroups)
        ReDim BiomassSzWt(NumGroups)
        ReDim AinLW(NumGroups)
        ReDim BinLW(NumGroups)
        ReDim Loo(NumGroups)
        ReDim Winf(NumGroups)
        ReDim t0(NumGroups)
        ReDim Tcatch(NumGroups)
        ReDim Tmax(NumGroups)

        ReDim AinLWInput(NumGroups)
        ReDim BinLWInput(NumGroups)
        ReDim LooInput(NumGroups)
        ReDim WinfInput(NumGroups)
        ReDim t0Input(NumGroups)
        ReDim TcatchInput(NumGroups)
        ReDim TmaxInput(NumGroups)

        ReDim EcopathWeight(NumGroups, NAgeSteps)
        ReDim EcopathNumber(NumGroups, NAgeSteps)
        ReDim EcopathBiomass(NumGroups, NAgeSteps)
        ReDim LorenzenMortality(NumGroups, NAgeSteps)

        ReDim PSD(NumGroups, NWeightClasses)
        ReDim Include(NumGroups)
        For i As Integer = 1 To NumGroups
            Include(i) = True
        Next

        Return True

    End Function

    ''' <summary>
    ''' Copy the Input arrays into the arrays that are used for modeling and model output.
    ''' </summary>
    ''' <returns>True if all the values were copied successfully.</returns>
    ''' <remarks>This is call at the start of an Ecopath model run to copy the input data into the arrays that are used
    ''' for model computations and output. I.e. copies EEinput(NumGroups) into EE(NumGroups). In EwE5 this is called MakeUnknownUnknown </remarks>
    Public Function CopyInputToModelArrays() As Boolean

        'Warning EwE5 also included input variables for BA, Immig, and Emigration 
        'See modEcosSense.MakeUnknownUnknown
        Try
            AinLWInput.CopyTo(AinLW, 0)
            BinLWInput.CopyTo(BinLW, 0)
            LooInput.CopyTo(Loo, 0)
            WinfInput.CopyTo(Winf, 0)
            t0Input.CopyTo(t0, 0)
            TcatchInput.CopyTo(Tcatch, 0)
            TmaxInput.CopyTo(Tmax, 0)
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            Return False
        End Try
        Return True

    End Function

    'Friend Sub copyTo(ByRef dest As cPSDDatastructures)
    '    Try
    '        'Joeh
    '        BiomassAvgSzWt.CopyTo(dest.BiomassAvgSzWt, 0)
    '        BiomassSzWt.CopyTo(dest.BiomassSzWt, 0)

    '        AinLW.CopyTo(dest.AinLW, 0)
    '        BinLW.CopyTo(dest.BinLW, 0)
    '        Loo.CopyTo(dest.Loo, 0)
    '        Winf.CopyTo(dest.Winf, 0)
    '        t0.CopyTo(dest.t0, 0)
    '        Tcatch.CopyTo(dest.Tcatch, 0)
    '        Tmax.CopyTo(dest.Tmax, 0)

    '        AinLWInput.CopyTo(dest.AinLWInput, 0)
    '        BinLWInput.CopyTo(dest.BinLWInput, 0)
    '        LooInput.CopyTo(dest.LooInput, 0)
    '        WinfInput.CopyTo(dest.WinfInput, 0)
    '        t0Input.CopyTo(dest.t0Input, 0)
    '        TcatchInput.CopyTo(dest.TcatchInput, 0)
    '        TmaxInput.CopyTo(dest.TmaxInput, 0)

    '        dest.EcopathWeight = DirectCast(EcopathWeight.Clone, Single(,))
    '        dest.EcopathNumber = DirectCast(EcopathNumber.Clone, Single(,))
    '        dest.EcopathBiomass = DirectCast(EcopathBiomass.Clone, Single(,))
    '        dest.LorenzenMortality = DirectCast(LorenzenMortality.Clone, Single(,))

    '        dest.PSD = DirectCast(PSD.Clone, Single(,))
    '    Catch ex2 As Exception
    '        Debug.Assert(False, ex2.Message)
    '    End Try

    'End Sub

End Class
