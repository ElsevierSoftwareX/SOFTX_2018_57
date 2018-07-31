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
Option Explicit On

Imports EwEUtils.Core

Public Class cMediationDataStructures
    'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    'Mediation vars 

    Public Const MAXFUNCTIONS As Integer = 5

    Public Const N_DEFAULT_MEDIATIONPOINTS As Integer = 1200

    ''' <summary>Number of functions</summary>
    Public MediationShapes As Integer
    ''' <summary>number of points per mediation function</summary>
    Public NMedPoints As Integer
    ''' <summary>mediation function points(iMedPt, iMedFn)</summary>
    Public Medpoints(,) As Single
    ''' <summary>defines biomass weights for med X (iMedGrp, iShp)</summary>
    ''' <remarks>Only used for non-landings mediations</remarks>
    Public MedWeights(,) As Single
    ''' <summary>number of biomasses (mediation weights) in an iMediation</summary>
    Public NMedXused() As Integer
    ''' <summary>groups used in med function X IMedUsed(nGroups + nGear, MediationShapes)</summary>
    Public IMedUsed(,) As Integer
    ''' <summary>ecopath base value of med function(iMedFn)</summary>
    Public MedXbase() As Single
    ''' <summary>value of med function at ecopath base(iMedFn)</summary>
    Public MedYbase() As Single
    ''' <summary>true if med function iMediation is used(iMedFn)</summary>
    Public MedIsUsed() As Boolean
    ''' <summary>current value of mediation function(iMedFn)</summary>
    Public MedVal() As Single

    ''' <summary>IMedBase() index of ecopath base biomass vertical line on mediation plot</summary>
    ''' <remarks>integer X positions for ecopath base X</remarks>
    Public IMedBase() As Integer
    ''' <summary>titles of mediation shapes</summary>
    Public MediationTitles() As String
    ''' <summary>Unique ID from the Database for each function(iMedFN)</summary>
    Public MediationDBIDs() As Integer
    ''' <summary>parameters that where used to create a curve from the Database Table and Fields i.e. EcoSimShapes.YZero</summary>
    Public MediationShapeParams() As cEcosimDatastructures.ShapeParameters
    ''' <summary>defines biomass weights for med XMedGFWeights(iMedGrp, iMedFlt, iShp)</summary>
    ''' <remarks>Only used for Landings mediations</remarks>
    Public MedPriceWeights(,,) As Single
    ''' <summary>?</summary>
    Public IMedFltUsed(,) As Integer

    Public PriceMedFuncNum(,,) As Integer

    Public FunctionNumber(,,) As Integer
    Public IsMedFunction(,,) As Boolean
    Public ApplicationType(,,) As eForcingFunctionApplication

    Protected m_nGroups As Integer
    Protected m_nFleets As Integer

    Public XAxisMin() As Single
    Public XAxisMax() As Single

    Public Sub New()
        NMedPoints = N_DEFAULT_MEDIATIONPOINTS
    End Sub

    Public Overridable Sub ReDimMediation(ByVal nGroups As Integer, ByVal nFleets As Integer)
        Dim i, j As Integer
        'following is for Mediation:
        Me.m_nGroups = nGroups
        Me.m_nFleets = nFleets
        ' JS18apr09: spawning 9 dummy mediation shapes without any valid database IDS screws up the database
        '            I tested Ecosim without mediation shapes and both core and GUI behave well
        'If MediationShapes <= 0 Then MediationShapes = 9
        ReDim Medpoints(NMedPoints, MediationShapes)
        ReDim MedWeights(nGroups + nFleets, MediationShapes)
        ReDim NMedXused(MediationShapes)
        ReDim IMedUsed(nGroups + nFleets, MediationShapes)
        ReDim MedXbase(MediationShapes)
        ReDim MedYbase(MediationShapes)
        ReDim MedIsUsed(MediationShapes)
        ReDim MedVal(MediationShapes)
        ReDim IMedBase(MediationShapes)

        ReDim MedPriceWeights(nGroups, nFleets, MediationShapes)
        ReDim IMedFltUsed(nGroups, MediationShapes)

        'jb added
        ReDim MediationTitles(MediationShapes)
        ReDim MediationShapeParams(MediationShapes)
        ReDim MediationDBIDs(MediationShapes)

        ReDim XAxisMin(MediationShapes)
        ReDim XAxisMax(MediationShapes)

        ReDim PriceMedFuncNum(nGroups, nFleets, MAXFUNCTIONS)

        ReDim FunctionNumber(nGroups, nGroups, cMediationDataStructures.MAXFUNCTIONS)
        ReDim IsMedFunction(nGroups, nGroups, cMediationDataStructures.MAXFUNCTIONS)
        ReDim ApplicationType(nGroups, nGroups, cMediationDataStructures.MAXFUNCTIONS)

        For i = 0 To MediationShapes
            IMedBase(i) = NMedPoints \ 3
            For j = 0 To NMedPoints
                Medpoints(j, i) = 0.5
            Next
        Next

    End Sub

    ''' <summary>
    ''' Populate the meditation multiplier MedVal() passed in as an argument using the current Biomass and/or Effort
    ''' </summary>
    ''' <param name="Biom">Biomass at the current time step</param>
    ''' <param name="FishingEffort">Fishing Effort by fleet, time</param>
    ''' <param name="iEffortTime">Time index in the effort array to get effort for the time step </param>
    ''' <param name="MedVal">Mediation function multiplier calculated by this call</param>
    ''' <remarks>
    ''' Populates MedVal() argument with the current multiplier. This does NOT populate cMediationDataStructures.MedVal() with the values.
    ''' Used by Ecospace threads so each thread can set MedVal() independently.
    '''   </remarks>
    Friend Sub SetMedFunctions(ByVal Biom() As Single, ByVal FishingEffort(,) As Single, ByVal iEffortTime As Integer, ByVal MedVal() As Single)
        'called from derivt, derivtred if MedIsUsed(0)=true to set
        'current Y value of each active trophic mediation function
        Dim iShp As Integer, iGrp As Integer, MedX As Single ', ip As Long
        Try

            For iShp = 1 To Me.MediationShapes
                If Me.MedIsUsed(iShp) Then
                    MedX = 0.0000000001
                    'Get the value on the X axis 
                    'Weighted sum of all the Groups or Fleet that are assigned to the X axis
                    For iGrp = 1 To Me.NMedXused(iShp)
                        If Me.IMedUsed(iGrp, iShp) <= Me.m_nGroups Then
                            MedX = MedX + Biom(Me.IMedUsed(iGrp, iShp)) * Me.MedWeights(Me.IMedUsed(iGrp, iShp), iShp)
                        Else    'a fleet
                            MedX = MedX + FishingEffort(Me.IMedUsed(iGrp, iShp) - Me.m_nGroups, iEffortTime) * Me.MedWeights(Me.IMedUsed(iGrp, iShp), iShp)
                        End If
                    Next

                    'Get the Y of this shape at this X
                    MedVal(iShp) = Me.getMedValue(iShp, MedX)

                End If
            Next
        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".SetMedFunctions() Exception: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Compute the mediation value for a mediation shape index(iMedShapeIndex) from an input value on the X axis (Xvalue) 
    ''' </summary>
    ''' <param name="iMedShapeIndex">Index of the mediation shape to use</param>
    ''' <param name="Xvalue">Value on the X axis to compute the mediation value for</param>
    ''' <returns>Value(Y) on the mediation shape for the input(X)</returns>
    ''' <remarks></remarks>
    Public Function getMedValue(ByVal iMedShapeIndex As Integer, ByVal Xvalue As Single) As Single

        Dim ip As Integer

        ' MDP changed to long int because in rare circumstances the division produced a number so big that an int could not hold it
        ' JS18Sep17: indeed, but a long is not 100% catch for numerical overflows. Try/Catch is.

        Try
            '280306 CJW found that without the +0.01 below it could be unstable when slope
            'was large around Ecopath base point in mediation function, causing instability.
            'This solves it. VC.
            ip = CInt(Math.Truncate(Me.IMedBase(iMedShapeIndex) * Xvalue / Me.MedXbase(iMedShapeIndex) + 0.01))
        Catch ex As Exception
            ' Log numerical overflow
            cLog.Write(ex, "Calculating mediation value for shape " & iMedShapeIndex & ", value " & Xvalue)
        End Try

        ' Truncate to allowed range
        ip = Math.Max(1, Math.Min(Me.NMedPoints, ip))
        Return Me.Medpoints(ip, iMedShapeIndex) / Me.MedYbase(iMedShapeIndex)

    End Function


    ''' <summary>
    ''' Set Mediation function multiplier in <see cref="cMediationDataStructures.MedVal"> cMediationDataStructures.MedVal()</see> for the current Biomass and/or Effort
    ''' </summary>
    ''' <param name="Biom">Biomass at the current time step</param>
    ''' <param name="FishingEffort">Fishing Effort by fleet, time</param>
    ''' <param name="iEffortTime">Time index in the effort array to get effort for the time step </param>
    ''' <remarks>
    ''' Called be Ecosim to set the mediation function multiplier in cMediationDataStructures.
    ''' SetMedFunctions(...) has overloaded function definition so it can be called by both Ecosim and on multiple threads from Ecospace.
    ''' </remarks>
    Friend Sub SetMedFunctions(ByVal Biom() As Single, ByVal FishingEffort(,) As Single, ByVal iEffortTime As Integer)
        'called from derivt, derivtred if MedIsUsed(0)=true to set
        'current Y value of each active trophic mediation function

        'Pass cMediationDataStructures.MedVal() in as an argument
        Me.SetMedFunctions(Biom, FishingEffort, iEffortTime, Me.MedVal)

    End Sub

    ''' <summary>
    ''' Set MedVal() for the applied price elasticity function to the annual catch at the current time step
    ''' </summary>
    ''' <param name="LandingsGroupFleet">Catch by group, fleet</param>
    ''' <remarks>Price mediation function are initialized to Ecopath base values which are annual. 
    ''' This means that the catch must also be the Ecopath annual catch.
    '''  </remarks>
    Friend Sub SetPriceMedFunctions(ByVal LandingsGroupFleet(,) As Single)
        Dim iShp As Integer, iGrp As Integer, MedX As Single ', ip As Long
        Dim iMedGrp As Integer
        Dim iMedFlt As Integer

        Try

            For iShp = 1 To Me.MediationShapes
                If Me.MedIsUsed(iShp) Then
                    MedX = 0.0000000001

                    'Get the weighted sum of all landings for the Group/Fleets assigned to this mediation shape
                    For iGrp = 1 To Me.NMedXused(iShp)
                        If Me.IMedUsed(iGrp, iShp) > 0 Then
                            'Get the Group and Fleet index
                            iMedGrp = Me.IMedUsed(iGrp, iShp)
                            iMedFlt = Me.IMedFltUsed(iGrp, iShp)
                            MedX = MedX + LandingsGroupFleet(iMedGrp, iMedFlt) * Me.MedPriceWeights(iMedGrp, iMedFlt, iShp)
                        End If
                    Next

                    'Get the Y value from the mediation shape for this X
                    MedVal(iShp) = Me.getMedValue(iShp, MedX)
                End If
            Next

        Catch ex As Exception
            '  Debug.Assert(False)
        End Try

    End Sub

    ''' <summary>
    ''' Return the Price Elasticity of Supply multiplier for a Group Fleet
    ''' </summary>
    ''' <param name="iGroup"></param>
    ''' <param name="iFleet"></param>
    ''' <returns>Returns only the PES multiplier</returns>
    ''' <remarks>Value = cEcopathDataStructures.Market(Fleet,Group) * getPESMult(Group,Fleet)</remarks>
    Public Function getPESMult(ByVal iGroup As Integer, ByVal iFleet As Integer) As Single
        Dim pMult As Single
        Dim bFoundMed As Boolean = False

        'Sum the multiplier for all applied price med functions for this Group Fleet
        For iFnt As Integer = 1 To cMediationDataStructures.MAXFUNCTIONS

            If Me.PriceMedFuncNum(iGroup, iFleet, iFnt) <= 0 Then
                Exit For
            End If

            pMult += Me.MedVal(Me.PriceMedFuncNum(iGroup, iFleet, iFnt))
            bFoundMed = True

        Next

        'No price elasticity function found set the multiplier to 1
        If Not bFoundMed Then pMult = 1
        'Return the multiplier
        Return pMult

    End Function

    Public Function getEnviroResponse(ByVal iMedShapeIndex As Integer, ByVal Xvalue As Single) As Single
        Dim ip As Integer

        'Debug.Assert(Xvalue <> cCore.NULL_VALUE, "Core NULL Passed to Response function.")

        'No Data in this cell 
        'So this response function has no affect
        If Xvalue = cCore.NULL_VALUE Then Return 1.0F

        'is the Xvalue in bounds
        If Xvalue <= Me.XAxisMin(iMedShapeIndex) Then Return Me.Medpoints(1, iMedShapeIndex)
        If Xvalue >= Me.XAxisMax(iMedShapeIndex) Then Return Me.Medpoints(NMedPoints, iMedShapeIndex)

        Dim dx As Double = NMedPoints / (Me.XAxisMax(iMedShapeIndex) - Me.XAxisMin(iMedShapeIndex) + 0.00001)
        ip = 1 + CInt(Math.Truncate((Xvalue - Me.XAxisMin(iMedShapeIndex)) * dx))
        'If iMedShapeIndex = 1 Then
        '    Debug.Print(Xvalue & ", " & Me.Medpoints(ip, iMedShapeIndex))
        'End If
        Return Me.Medpoints(ip, iMedShapeIndex)

    End Function

End Class

