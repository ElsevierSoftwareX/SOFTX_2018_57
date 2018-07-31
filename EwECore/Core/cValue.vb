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
Imports EwEUtils.SystemUtilities

Namespace ValueWrapper


    Public Delegate Function CoreCounterDelegate(ByVal SizeType As eCoreCounterTypes) As Integer
    Public Delegate Function CoreIndexedCounterDelegate(ByVal SizeType As eCoreCounterTypes, ByVal iArrayIndex As Integer) As Integer


    ''' <summary>
    ''' Classes used to wrap variables and there associated data used be the ICoreInputOuput objects
    ''' </summary>
    ''' <remarks>
    ''' These classes are defined as Friend so that they are not exposed outside of the core
    ''' </remarks>
    ''' <history>
    ''' <revision>jb 17/mar/06 Added Length of array size</revision>
    ''' </history>

#Region "Enumerators used by Value objects"

    Public Enum eValueTypes
        Int 'integer
        Str 'string
        Sng 'single
        Bool 'boolean

        SingleArray 'array of singles 
        BoolArray 'array of boolean 
        IntArray 'array of integers

        'Histogram
    End Enum

#End Region

#Region "cValue"


    ''' <summary>
    ''' Wraps the Value, Status, Name and Type of a variable used be an ICoreInputOuput object into one place.
    ''' </summary>
    ''' <remarks>
    ''' cValue acts as the base class for other types of value object.
    ''' ToDo:: the varType enumerator could be change to being a System.Type object.
    ''' </remarks>
    Public Class cValue
        Implements IDisposable

        Private m_value As Object
        Protected m_orgvalue As Object
        Protected m_status As eStatusFlags
        Protected m_orgStatus As eStatusFlags
        Protected m_validationstatus As eStatusFlags

        Protected m_varType As eValueTypes
        Protected m_varName As eVarNameFlags
        Protected m_message As String 'message associated with data validation

        Protected m_bStored As Boolean = False
        Protected m_bAffectsRunState As Boolean = True

        ''' <summary>
        ''' Validator supplied in the constructor of the object.
        ''' </summary>
        ''' <remarks>This validator can be specific to the this variable type or it can be the default supplied by the ValidatorManger.</remarks>
        Protected m_validator As cValidatorDefault

        Protected m_metadata As cVariableMetaData

        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        ''' <remarks>
        ''' A default value will not be stored, will affect the core run state.
        ''' A default value has no metadata and no validation.
        ''' </remarks>
        Sub New()
            Me.New(Nothing, eVarNameFlags.NotSet, eStatusFlags.Null, eValueTypes.Sng)
            Me.m_bStored = False
        End Sub

        ''' <summary>
        ''' Constructs a new value instance.
        ''' </summary>
        ''' <param name="Value">The object to hold the value.</param>
        ''' <param name="VarName">The variable name representing the value.</param>
        ''' <param name="Status">Bitwise <see cref="eStatusFlags">status flag pattern</see> to initialize the value with.</param>
        ''' <param name="VarType"><see cref="eValueTypes">Value type</see>.</param>
        ''' <param name="MetaData"><see cref="cVariableMetaData">Value metadata</see> to use.</param>
        ''' <param name="Validator">The <see cref="cValidatorDefault">Validator</see> to use. If no validator is provided
        ''' a default validator will be obtained unless the value is flagged as <see cref="eStatusFlags.NotEditable"/>
        ''' in the <paramref name="Status"/> parameter.</param>
        Sub New(ByVal Value As Object,
                ByVal VarName As eVarNameFlags,
                ByVal Status As eStatusFlags,
                ByVal VarType As eValueTypes,
                Optional ByVal MetaData As cVariableMetaData = Nothing,
                Optional ByVal Validator As cValidatorDefault = Nothing)

            Me.m_value = Value
            Me.m_varType = VarType
            Me.m_varName = VarName
            Me.m_status = Status
            Me.m_metadata = MetaData

            ' Do not validate output values
            Me.AllowValidation = ((Status And eStatusFlags.NotEditable) = 0)

            If (Me.AllowValidation) Then
                ' Set the validator and its properties
                Me.m_validator = Validator
                ' Resolve default validator if missing
                If (Me.m_validator Is Nothing) Then
                    Me.m_validator = cValidatorManager.Get(VarName)
                End If
            End If

            Me.m_bStored = True
            Me.m_bAffectsRunState = True

            ' Is metadata missing?
            If (Me.m_metadata Is Nothing) Then
                ' #Yes: get from standard repository?
                Me.m_metadata = cVariableMetaData.Get(Me.varName)
                ' Is metadata still missing?
                If (Me.m_metadata Is Nothing) Then
                    ' #Yes: erm... last resort: create a local default
                    Me.m_metadata = cVariableMetaData.Default(Me.m_varType, "")
                End If
            End If

            ' Sanity check
            Debug.Assert(Me.m_metadata IsNot Nothing)
            Me.m_metadata.Attach(Me)

        End Sub

        ''' <summary>
        ''' Get/set the Index of a Value
        ''' </summary>
        Public Property Index() As Integer

        ''' <summary>
        ''' Set the size of the array to the new Value based on the CoreCounterDelegate passed in via the consturctor
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>This is for array value objects only.</remarks>
        Public Overridable Function SetSize() As Boolean
            Return False
        End Function

        Public Overridable ReadOnly Property IsArray() As Boolean
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Get/set the status flag for a Value.
        ''' </summary>
        ''' <param name="iSecondIndex">Optional value index.</param>
        Public Overridable Property Status(Optional ByVal iSecondIndex As Integer = cCore.NULL_VALUE, Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As eStatusFlags
            Get
                Return m_status
            End Get
            Friend Set(ByVal value As eStatusFlags)
                m_status = value
            End Set
        End Property

        ''' <summary>
        ''' Get/set the last valation result for a Value.
        ''' </summary>
        ''' <param name="iIndex">Optional value index.</param>
        Public Overridable Property ValidationStatus(Optional ByVal iIndex As Integer = cCore.NULL_VALUE) As eStatusFlags
            Get
                Return m_validationstatus
            End Get
            Set(ByVal value As eStatusFlags)
                m_validationstatus = value
            End Set
        End Property

        ''' <summary>
        ''' Get/set the actual value of a Value.
        ''' </summary>
        ''' <param name="iIndex">Optional value index.</param>
        Public Overridable Property Value(Optional ByVal iIndex As Integer = cCore.NULL_VALUE, Optional ByVal iIndex2 As Integer = cCore.NULL_VALUE) As Object
            Get
                Return m_value
            End Get
            Set(ByVal value As Object)
                Validate(value)
            End Set
        End Property

        Public Property varName() As eVarNameFlags
            Get
                Return Me.m_varName
            End Get
            Friend Set(value As eVarNameFlags)
                Me.m_varName = value
            End Set
        End Property

        Public Property varType() As eValueTypes
            Get
                Return Me.m_varType
            End Get
            Friend Set(value As eValueTypes)
                Me.m_varType = value
            End Set
        End Property

        Public Property ValidationMessage() As String
            Get
                Return m_message
            End Get
            Friend Set(ByVal value As String)
                m_message = value
            End Set
        End Property

        ''' <summary>
        ''' Flag stating whether a variable can be stored in the database.
        ''' </summary>
        Public Property Stored() As Boolean
            Get
                Return Me.m_bStored
            End Get
            Friend Set(ByVal value As Boolean)
                Me.m_bStored = value
            End Set
        End Property

        ''' <summary>
        ''' Flag stating whether a variable will affect the core run state when it is modified.
        ''' </summary>
        Public Property AffectsRunState() As Boolean
            Get
                Return Me.m_bAffectsRunState
            End Get
            Friend Set(ByVal value As Boolean)
                Me.m_bAffectsRunState = value
            End Set
        End Property

        ''' <summary>
        ''' Number of elements in the underlying array for an array object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property Length() As Integer
            Get
                Return 0
            End Get
        End Property

        Public ReadOnly Property Metadata() As cVariableMetaData
            Get
                Return Me.m_metadata
            End Get
        End Property

        Protected Overridable Function Validate(ByRef NewValue As Object,
                                                Optional ByVal iSecondIndex As Integer = cCore.NULL_VALUE,
                                                Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE) As Boolean

            'set the value of this object to the new value passed in 
            'this allows the validator the access the new value via the public interface
            m_orgvalue = m_value
            'convert null or empty inputs into something that can be used
            m_value = Me.convertEmptyInputs(NewValue)

            'is it ok to run the validator?
            If Not Me.AllowValidation Then
                'No Validation set the value without running the validator
                m_validationstatus = eStatusFlags.OK
                Return False 'validation was not run???
            End If

            'not every value object has a validator?
            'outputs are validated by the core once it has run the model because only it knows the working of the models and what the model results mean
            If m_validator Is Nothing Then
                'set the value without running the validator
                m_validationstatus = eStatusFlags.OK
                System.Console.WriteLine(m_varName.ToString & " does not have a validator.")
                Return False 'validation was not run???
            End If

            If m_validator.Validate(Me, m_metadata, iSecondIndex, iThirdIndex) Then
                If m_validationstatus = eStatusFlags.FailedValidation Then
                    'if the new value failed validation then set the value back to its original value
                    m_value = m_orgvalue
                End If

                ' JS 10Jan08: disabled the following logic. Setting a validation status to NULL will 
                '             obscure any failed validation attempts, which in turn prevents the user
                '             from knowing what happened. As such, the Validation status flag can only be OK or Failed.
                '             The Status status flag provides more further detailed information about a variable.

                'If m_status = eStatusFlags.Null Then
                '    m_validationstatus = eStatusFlags.Null
                '    ' m_value = m_metadata.DefaultValue
                'End If
            Else 'If m_validator.Validate(Me, iSecondaryIndex) Then
                'for some reason the validator returned False it could not validate the value
                Debug.Assert(False, "Validator for " & m_varName.ToString & " failed.")
                Return False
            End If

            Return True

        End Function

        ''' <summary>
        ''' Run the validator to set the status flag without setting the value
        ''' </summary>
        ''' <param name="iSecondIndex"></param>
        ''' <remarks>This is use be the cCoreInputOutputBase to set the status flags of all its values </remarks>
        Public Overridable Sub setStatusFlag(Optional ByVal iSecondIndex As Integer = cCore.NULL_VALUE,
                                                Optional ByVal iThirdIndex As Integer = cCore.NULL_VALUE)

            If m_validator IsNot Nothing Then
                m_validator.Validate(Me, m_metadata, iSecondIndex, iThirdIndex)
            Else
                ' System.Console.WriteLine("No validator definded for " & m_varType.ToString)
            End If

        End Sub

        Public Property AllowValidation() As Boolean = False

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub



        ''' <summary>
        ''' Convert from some kind of NULL/Empty into a value of some sort
        ''' </summary>
        ''' <param name="newValue"></param>
        ''' <returns></returns>
        ''' <remarks>This is because different types of controls pass empty values differently</remarks>
        Protected Function convertEmptyInputs(ByVal newValue As Object) As Object

            ' Test whether provided value is empty
            Dim bNeedDefault As Boolean = (newValue Is Nothing) Or (TypeOf newValue Is System.DBNull)

            ' Convert enums to storage types
            If newValue.GetType.IsEnum Then
                Select Case Me.m_varType
                    Case eValueTypes.Int
                        newValue = CInt(newValue)
                    Case eValueTypes.Bool
                        newValue = CBool(newValue)
                    Case eValueTypes.Sng
                        newValue = CSng(newValue)
                    Case Else
                        Debug.Assert(False)
                End Select
            End If

            ' Not an empty value?
            If Not bNeedDefault Then
                ' #Yes: is a numerical variable being set?
                If (Me.varType <> eValueTypes.Str) Then
                    ' #Yes: is a string provided for a numerical variable?
                    If (TypeOf newValue Is String) Then
                        ' #Yes: is the value string empty?
                        If String.IsNullOrEmpty(newValue.ToString) Then
                            ' #Yes: we'll need the default value here to ensure core data remains valid
                            bNeedDefault = True
                        End If
                    Else

                        ' JS 29Jul16: Unique default correction for Single values that cannot be 0,
                        ' that are estimated by the model, and that have a default of cCore.NULL_VALUE.
                        ' These variables, such as BiomassInput, EE, GE, QB, PB, etc, can be cleared
                        ' by entering a 0 in the UI. This rather odd clearing behaviour is maintained for
                        ' backward compatibility with EwE5.

                        ' Is a numerical var?
                        Select Case Me.varType
                            Case eValueTypes.Int, eValueTypes.Sng
                                ' Is 0.0! entered and metadata available?
                                Dim x As Single
                                Single.TryParse(newValue.ToString, x)
                                If x = 0.0F And (Me.Metadata IsNot Nothing) Then
                                    ' #Yes: does metadata NOT allow 0.0?
                                    If Not (Metadata.MinOperator.Compare(0.0!, Metadata.Min) And Metadata.MaxOperator.Compare(0.0!, Metadata.Max)) Then
                                        ' #Yes: Core_NULL is the default? 
                                        If (CSng(Metadata.NullValue) = cCore.NULL_VALUE) Then
                                            ' #Yes: '0' clears the variable
                                            bNeedDefault = True
                                        End If
                                    End If
                                End If
                        End Select
                    End If
                End If
            End If
            If (bNeedDefault) Then
                Select Case Me.varType
                    Case eValueTypes.Str
                        newValue = CStr(Me.m_metadata.NullValue)
                    Case eValueTypes.Int, eValueTypes.IntArray
                        newValue = CInt(Me.m_metadata.NullValue)
                    Case eValueTypes.Sng, eValueTypes.SingleArray
                        newValue = CSng(Me.m_metadata.NullValue)
                    Case eValueTypes.Bool, eValueTypes.BoolArray
                        newValue = CBool(Me.m_metadata.NullValue)
                    Case Else
                        ' JS: status flag is overwritten later on. No need trying to set
                        ' Status = eStatusFlags.ErrorEncountered
                        Debug.Assert(False, Me.ToString & ".convertEmptyInputs(...) unsupported varType " & Me.varType)
                End Select
            End If

            'value that got passed in as a string but it is supposed to be something else
            ' JS 070122: String-to-number implemented with blunt Var() since this method is the most
            '            robust alternative by ignoring rubbish characters on a presumed number string.
            '            For instance, this thing converts "4foo" to 4 and "plop8" to 0.
            '            The calling logic will need to decide whether this is proper behaviour. This
            '            method of conversion is simply selected to keep the core from exploding.

            'jb Mar-2012 Mono compatibility 
            'Val() is in the Microsoft.VisualBasic library
            'So I've replace the Val() code with TryParse(string,x)
            'Hope this works the same...
            If (TypeOf newValue Is System.String) Then

                Select Case Me.varType
                    Case eValueTypes.Str
                        ' Ok
                    Case eValueTypes.Int
                        Dim x As Integer = CInt(cSystemUtils.Val(newValue))
                        newValue = x
                    Case eValueTypes.Sng
                        Dim x As Single = CSng(cSystemUtils.Val(newValue))
                        newValue = x
                    Case eValueTypes.Bool
                        Dim x As Boolean
                        Boolean.TryParse(newValue.ToString, x)
                        newValue = x
                    Case Else
                        ' JS: status flag is overwritten later on. No need trying to set
                        'Status = eStatusFlags.ErrorEncountered
                        Debug.Assert(False, Me.ToString & ".convertEmptyInputs() unsupported varType " & Me.varType)
                End Select
            End If

            Return newValue

        End Function

        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            Me.m_metadata = Nothing
            Me.m_orgStatus = Nothing
            Me.m_status = Nothing
            Me.m_value = Nothing
            Me.m_orgvalue = Nothing
            Me.m_validator = Nothing
        End Sub

    End Class

#End Region

End Namespace
