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
''' Manager for data validators. This provides access to data validator objects through its getValidator(eVarNameFlags) method
''' </summary>
''' <remarks>To add a validator create a new instance in the constructor</remarks>
Public Class cValidatorManager

    Private Shared s_inst As cValidatorManager = Nothing
    Private m_validators As Dictionary(Of eVarNameFlags, cValidatorDefault)

    Private Shared Function GetInstance() As cValidatorManager
        Return s_inst
    End Function

    ''' <summary>
    ''' Create an instance of the ValidatorManger. 
    ''' </summary>
    ''' <param name="theCore"></param>
    ''' <remarks>To add data validation. Create an instance of the data validator in this constructor. 
    ''' Add the data validator to the dictionary (m_validators) of validators using its Type (eVarNameFlags) as the key. 
    ''' When getValidator(eVarNameFlags) is called it will return this instance of the validator.
    ''' This way only one instance of a validator need to be created and it can be used to do all the validation of a given variable. </remarks>
    Sub New(ByRef theCore As cCore)

        Dim validator As cValidatorDefault

        s_inst = Me

        m_validators = New Dictionary(Of eVarNameFlags, cValidatorDefault)

        'default validator in the NotSet Key
        validator = New cValidatorDefault(eVarNameFlags.NotSet)
        m_validators.Add(validator.VarName, validator)

        'Numeric validator that sets the Validation status to NULL if the value is less than the Min
        'this is used for variables that will have there value computed by a model if they are not supplied by a user
        'I.e. EE
        'Create one validator and use it for all the variables
        validator = New cValidatorNumericSetToNull()
        m_validators.Add(eVarNameFlags.EEInput, validator)
        m_validators.Add(eVarNameFlags.PBInput, validator)
        m_validators.Add(eVarNameFlags.QBInput, validator)
        m_validators.Add(eVarNameFlags.GEInput, validator)
        m_validators.Add(eVarNameFlags.BiomassAreaInput, validator)
        m_validators.Add(eVarNameFlags.Biomass, validator)

        'the same core validator for all the Ecospace summary data
        'the validator will figure out which varaible is being validated
        validator = New cValidatorCore(theCore)
        m_validators.Add(eVarNameFlags.EcospaceNumberSummaryTimeSteps, validator)
        m_validators.Add(eVarNameFlags.EcospaceSummaryTimeEnd, validator)
        m_validators.Add(eVarNameFlags.EcospaceSummaryTimeStart, validator)
        ''Fishing Policy Search blocks
        m_validators.Add(eVarNameFlags.SearchBlock, validator)
        'MSE FleetWeight must be a valid fleet
        m_validators.Add(eVarNameFlags.MSEFleetWeight, validator)

        m_validators.Add(eVarNameFlags.MSEFixedEscapement, validator)
        m_validators.Add(eVarNameFlags.MSEFixedF, validator)

        'MPAOpt
        m_validators.Add(eVarNameFlags.MPAOptStartYear, validator)
        m_validators.Add(eVarNameFlags.MPAOptEndYear, validator)

        m_validators.Add(eVarNameFlags.EcosimSumNTimeSteps, validator)
        m_validators.Add(eVarNameFlags.EcosimSumStart, validator)
        m_validators.Add(eVarNameFlags.EcosimSumEnd, validator)

        'Fishing Policy search base year validated via a core counter
        validator = New cValidatorCounter(theCore, eCoreCounterTypes.nEcosimYears)
        m_validators.Add(eVarNameFlags.SearchBaseYear, validator)

        'MSE Results and Run start and end year use core counter
        m_validators.Add(eVarNameFlags.MSEResultsStartYear, validator)
        m_validators.Add(eVarNameFlags.MSEResultsEndYear, validator)
        m_validators.Add(eVarNameFlags.MSEStartYear, validator)

        'PSD validator(s)
        validator = New cValidatorOddEven(True)
        m_validators.Add(eVarNameFlags.NumPtsMovAvg, validator)

        validator = New cValidatorCore(theCore)
        m_validators.Add(eVarNameFlags.RelQt, validator)

        m_validators.Add(eVarNameFlags.VariableName, New cValidatorEnum(GetType(eVarNameFlags)))

        validator = New cValidatorCounter(theCore, eCoreCounterTypes.nEcospaceTimeSteps)
        m_validators.Add(eVarNameFlags.EcospaceFirstOutputTimeStep, validator)



        ' Ecospace layers - special cases
        Me.m_validators.Add(eVarNameFlags.LayerRegion, New cValidatorCounter(theCore, eCoreCounterTypes.nRegions))

        Me.m_validators.Add(eVarNameFlags.EcologyType, New cValidatorEnum(GetType(eEcologyTypes)))
        Me.m_validators.Add(eVarNameFlags.IUCNConservationStatus, New cValidatorEnum(GetType(eIUCNConservationStatusTypes)))
        Me.m_validators.Add(eVarNameFlags.OrganismType, New cValidatorEnum(GetType(eOrganismTypes)))
        Me.m_validators.Add(eVarNameFlags.OccurrenceStatus, New cValidatorEnum(GetType(eOccurrenceStatusTypes)))
        ' Bitwise flag enums not supported yet
        'Me.m_validators.Add(eVarNameFlags.EcospaceCapCalType, New cValidatorEnum(GetType(eEcospaceCapacityCalType)))

        ' Fishing policy search
        Me.m_validators.Add(eVarNameFlags.FPSInitOption, New cValidatorEnum(GetType(eInitOption)))
        Me.m_validators.Add(eVarNameFlags.FPSSearchOption, New cValidatorEnum(GetType(eSearchOptionTypes)))
        Me.m_validators.Add(eVarNameFlags.FPSOptimizeApproach, New cValidatorEnum(GetType(eOptimizeApproachTypes)))
        Me.m_validators.Add(eVarNameFlags.FPSOptimizeOptions, New cValidatorEnum(GetType(eOptimizeOptionTypes)))

        ' MPA optimizations
        Me.m_validators.Add(eVarNameFlags.MPAOptSearchType, New cValidatorEnum(GetType(eMPAOptimizationModels)))
        Me.m_validators.Add(eVarNameFlags.iMPAOptToUse, New cValidatorCounter(theCore, eCoreCounterTypes.nMPAs))

        ' MSE
        Me.m_validators.Add(eVarNameFlags.MSEBatchIterCalcType, New cValidatorEnum(GetType(eMSEBatchIterCalcTypes)))
        Me.m_validators.Add(eVarNameFlags.QuotaType, New cValidatorEnum(GetType(eQuotaTypes)))

        ' MSY
        Me.m_validators.Add(eVarNameFlags.MSYFSelectionMode, New cValidatorEnum(GetType(eMSYFSelectionModeType)))
        Me.m_validators.Add(eVarNameFlags.MSYRunLengthMode, New cValidatorEnum(GetType(eMSYRunLengthModeTypes)))

    End Sub

    ''' <summary>
    ''' Return a validator for the specified eVarNameFlags
    ''' </summary>
    ''' <param name="VarName">eVarNameFlags of validator to return</param>
    ''' <returns>A valid validator for this eVarNameFlags type or the default validator if no other validator could be found.</returns>
    ''' <remarks>Validator are created in the constructor and kept in a dictionary. 
    ''' Only one instance of each validator is use. This will return the same validator on each call for a VarName.
    ''' </remarks>
    Public Function getValidator(ByVal VarName As eVarNameFlags) As cValidatorDefault

        Try
            If m_validators.ContainsKey(VarName) Then
                Return m_validators.Item(VarName)
            Else
                'System.Console.WriteLine(VarName.ToString & " No Validator. Default will be used.")
                Return m_validators.Item(eVarNameFlags.NotSet)
            End If

        Catch ex As Exception
            'bummer
            cLog.Write(Me.ToString & "getValidator() Error: " & ex.Message)
            Debug.Assert(False, ex.Message)
            Return Nothing
        End Try

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get any pre-registered validator, or a default validator if no pre-registered
    ''' validator was found.
    ''' </summary>
    ''' <param name="varname"></param>
    ''' -----------------------------------------------------------------------
    Public Shared Function [Get](ByVal varname As eVarNameFlags) As cValidatorDefault
        Return GetInstance().getValidator(varname)
    End Function

End Class

