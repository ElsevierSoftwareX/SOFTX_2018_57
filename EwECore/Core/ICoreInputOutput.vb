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
Imports System.ComponentModel
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
Imports EwEUtils.Utilities

#Region " Definition of interfaces "

''' ---------------------------------------------------------------------------
''' <summary>
''' Interface for exposing Core data entities.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface ICoreInterface

    ''' <summary>A human readable name identifying a core data entity.</summary>
    Property Name() As String
    ''' <summary>The ordinal number in the core storage structures for a core data entity.</summary>
    Property Index() As Integer

    ''' <summary>Globally unique ID identifying a core data entity.</summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Function GetID() As String
    ''' <summary>Unique ID per type of core data used to distinguish a core data entity in a storage medium. DBID is short for Database ID</summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Property DBID() As Integer
    ''' <summary><see cref="eDataTypes">Data type</see> identifying the class of a core data entity.</summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    ReadOnly Property DataType() As eDataTypes
    ''' <summary><see cref="eCoreComponentType">Message source</see> identifying the section of core data entity where this logic originates from.</summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    ReadOnly Property CoreComponent() As eCoreComponentType

End Interface ' ICoreInterface

''' ---------------------------------------------------------------------------
''' <summary>
''' Interface for accessing Core input or output objects.
''' </summary>
''' <remarks>
''' This allows all model/scenario input and output entities to be accessed through one interface.
'''</remarks>
''' ---------------------------------------------------------------------------
Public Interface ICoreInputOutput
    Inherits IDisposable

    ''' <summary>
    ''' Returns the value exposed by a Core input or output object.
    ''' </summary>
    ''' <param name="VarName"><see cref="eVarNameFlags">Variable</see> type to access.</param>
    ''' <param name="iIndex2">Optional index of the value to return when accessing an array-type variable.</param>
    ''' <returns>Any loose-typed value, or Nothing if an error occurred.</returns>
    Function GetVariable(ByVal VarName As eVarNameFlags, Optional ByVal iIndex1 As Integer = cCore.NULL_VALUE, Optional ByVal iIndex2 As Integer = cCore.NULL_VALUE, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object

    ''' <summary>
    ''' Sets the value of a variable exposed by a Core input or output object.
    ''' </summary>
    ''' <param name="VarName"><see cref="eVarNameFlags">Variable</see> type to access.</param>
    ''' <param name="iIndex">Optional index of the value to set when accessing an array-type variable.</param>
    ''' <returns>True if successful.</returns>
    Function SetVariable(ByVal VarName As eVarNameFlags, ByVal newValue As Object, Optional ByVal iIndex As Integer = cCore.NULL_VALUE, Optional ByVal iThirdIndex As Integer = -9999) As Boolean

    ''' <summary>
    ''' Returns the <see cref="eStatusFlags">Status</see> of a value exposed by a Core input or output object.
    ''' </summary>
    ''' <param name="VarName"><see cref="eVarNameFlags">Variable</see> type to access.</param>
    ''' <param name="iSecondaryIndex">Optional index of the value status to query when accessing an array-type variable.</param>
    ''' <returns>Any loose-typed value, or Nothing if an error occurred.</returns>
    Function GetStatus(ByVal VarName As eVarNameFlags, Optional ByVal iSecondaryIndex As Integer = -9999,
                                            Optional ByVal iThirdIndex As Integer = -9999) As eStatusFlags

    ''' <summary>
    ''' Sets the <see cref="eStatusFlags">Status</see> of a value exposed by a Core input or output object.
    ''' </summary>
    ''' <param name="VarName"><see cref="eVarNameFlags">Variable</see> type to access.</param>
    ''' <param name="iSecondaryIndex">Optional index of the value status to set when accessing an array-type variable.</param>
    ''' <returns>Any loose-typed value, or Nothing if an error occurred.</returns>
    Function SetStatus(ByVal VarName As eVarNameFlags, ByVal newStatus As eStatusFlags, Optional ByVal iSecondaryIndex As Integer = -9999,
                                            Optional ByVal iThirdIndex As Integer = -9999) As Boolean

    ''' <summary>
    ''' Returns the <see cref="cVariableStatus">result</see> of the most recent 
    ''' attempt to <see cref="SetVariable">Set a variable</see>.
    ''' </summary>
    ''' <returns>A <see cref="cVariableStatus">cVariableStatus</see> containing 
    ''' the result of the most recent attempt to <see cref="SetVariable">Set</see> 
    ''' a variable.</returns>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    ReadOnly Property ValidationStatus() As cVariableStatus

    ''' <summary>
    ''' Clear the content of a ICoreInputOutput. This leaves the object ready to reuse.
    ''' </summary>
    Sub Clear()

    ''' <summary>
    ''' Gets whether the instance is disposed.
    ''' </summary>
    <Browsable(False)>
    ReadOnly Property Disposed As Boolean

End Interface ' ICoreInputOutput

''' ---------------------------------------------------------------------------
''' <summary>
''' Interface for defining a group.
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface ICoreGroup
    Inherits ICoreInputOutput
    Inherits ICoreInterface

    ''' <summary>
    ''' Get/set whether the group is part of a multi-stanza configuration.
    ''' </summary>
    ReadOnly Property IsMultiStanza() As Boolean

    ''' <summary>
    ''' Get/set the zero-based index of the <see cref="cStanzaGroup">Stanza configuration</see> 
    ''' that this group belongs to.
    ''' </summary>
    Property iStanza() As Integer

    ''' <summary>
    ''' The ratio that this group contributes to Primary Production.
    ''' </summary>
    ''' <returns>This method will return one of the following values:
    ''' <list type="bullet">
    ''' <item>0-1 for mixed consumer/producer groups</item>
    ''' <item>1 for primary producers</item>
    ''' <item>2 for detritus groups</item>
    ''' </list>
    ''' </returns>
    ''' <remarks>This can be used as a flag to tell if a group is mixed consumer/producer, primary producer or a detritus group.</remarks>
    Property PP() As Single

    ''' <summary>
    ''' Helper method; gets whether this group is a consumer.
    ''' </summary>
    ReadOnly Property IsConsumer() As Boolean

    ''' <summary>
    ''' Helper method; gets whether this group is a primary producer.
    ''' </summary>
    ReadOnly Property IsProducer() As Boolean

    ''' <summary>
    ''' Helper method; gets whether this group is detritus.
    ''' </summary>
    ReadOnly Property IsDetritus() As Boolean

    ''' <summary>
    ''' Helper method; gets whether this group is a living group.
    ''' </summary>
    ReadOnly Property IsLiving() As Boolean

End Interface ' ICoreGroup

#End Region ' Definition of interfaces 

#Region " cCoreInputOutputBase "

''' ---------------------------------------------------------------------------
''' <summary>
''' Base class implementation of the ICoreInterface, ICoreInputOutput interfaces.
''' </summary>
''' <remarks>
''' <para>This class provides the code that implements the ICoreInputOutput interface.</para>
''' <para>Classes that inherit from this base class need to populate the lookup tables that are
''' used to store the internal data in the New constructor and define a dot (.) operator
''' for any variables that requires to be accessed via Properties.</para>
''' <para>For examples on how to implement this class, refer to <see cref="cEcoPathGroupInput">cEcoPathGroupInput</see>,
''' <see cref="cEcopathFleetInput">cEcopathFleetInput</see>, etc.</para>
'''</remarks>
''' ---------------------------------------------------------------------------
Public MustInherit Class cCoreInputOutputBase
    Implements ICoreInterface
    Implements ICoreInputOutput

#Region " Protected variables "

#Region " DEBUG ONLY "

#If DEBUG Then
    ''' <summary>DEBUG ONLY: instance tracker</summary>
    Protected m_iInstance As Integer = cCore.NULL_VALUE
    ''' <summary>DEBUG ONLY: instance tracker</summary>
    Protected Shared s_iNextInstance As Integer = 1
#End If

#End Region ' DEBUG ONLY

    ''' <summary>
    ''' States whether <see cref="cValue.AllowValidation">Variable validation</see> is enabled for this object.
    ''' </summary>
    ''' <remarks>
    ''' Validation is typically required in response to <see cref="SetVariable">SetVariable</see> 
    ''' calls triggered by user actions. Whenever an object is populated by the 
    ''' <see cref="cCore">EwE Core</see> validation may be temporarily disabled.
    ''' </remarks>
    Protected m_bValidate As Boolean = False

    ''' <summary>
    ''' States whether an object will allow its values to be modified via <see cref="SetVariable">SetVariable</see>.
    ''' </summary>
    Protected Friend m_bReadOnly As Boolean = False

    ''' <summary>
    ''' Container for the <see cref="cCoreInputOutputBase.ValidationStatus">Validation status</see> of the object.
    ''' </summary>
    Protected m_ValidationStatus As cVariableStatus = Nothing

    ''' <summary>
    ''' Container for the <see cref="ICoreInterface.DataType">data type</see> describing the object.
    ''' </summary>
    Protected m_dataType As eDataTypes = eDataTypes.NotSet

    ''' <summary>
    ''' The variables maintained by this object. Implemented as a collection of <see cref="cValue">variable values</see>
    ''' indexed by <see cref="eVarNameFlags">Variable name</see>.
    ''' </summary>
    Friend m_values As New Dictionary(Of eVarNameFlags, cValue)

    ''' <summary>
    ''' The <see cref="eCoreComponentType">EwE core component</see> that this object belongs to
    ''' </summary>
    ''' <remarks></remarks>
    Protected m_coreComponent As eCoreComponentType = eCoreComponentType.NotSet

    ''' <summary>
    ''' Reference to the <see cref="cCore">EwE Core</see> that exposes the object.
    ''' </summary>
    Protected m_core As cCore = Nothing

#End Region ' Protected variables

#Region "Constructor and Initialization"

    ''' <summary>
    ''' Create and populate the Lookup tables, as well as <see cref="cCoreInputOutputBase">cCoreInputOutputBase</see>-defined variables.
    ''' </summary>
    ''' <remarks>A class the inherits from this base class will need to define its own variables in its constructor</remarks>
    Sub New(ByRef TheCore As cCore)

        Dim val As cValue
        Dim name() As Char

        m_core = TheCore

        Me.m_ValidationStatus = New cVariableStatus()
        Me.m_ValidationStatus.CoreDataObject = Me

        val = New cValue(New String(name), eVarNameFlags.Name, eStatusFlags.OK, eValueTypes.Str)
        val.AffectsRunState = False
        m_values.Add(val.varName, val)

        val = New cValue(New Integer, eVarNameFlags.Index, eStatusFlags.OK, eValueTypes.Int)
        val.AffectsRunState = False
        m_values.Add(val.varName, val)

        val = New cValue(New Integer, eVarNameFlags.DBID, eStatusFlags.OK, eValueTypes.Int)
        val.AffectsRunState = False
        m_values.Add(val.varName, val)

#If DEBUG Then
        Me.m_iInstance = cCoreInputOutputBase.s_iNextInstance
        cCoreInputOutputBase.s_iNextInstance += 1
#End If

    End Sub

    ''' <summary>
    ''' Resize any indexed variables i.e. DietComp to the size of the <see cref="eCoreCounterTypes">core counter</see> that it is dimensioned by.
    ''' </summary>
    Friend Overridable Function Resize() As Boolean
        Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
        Dim value As cValue

        For Each keyvalue In m_values
            value = keyvalue.Value
            'only cValueArray objects will actually resize the underlying data
            value.SetSize()
        Next

    End Function

#End Region

#Region " Public Functions/Methods "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the unique ID for this object as a text string.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public Function GetID() As String _
        Implements ICoreInterface.GetID
        Return cValueID.GetDataTypeID(Me.m_dataType, Me.DBID)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="eDataTypes">data type</see> uniquely identifying
    ''' the type of core data that this class implements.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public ReadOnly Property DataType() As eDataTypes _
        Implements ICoreInterface.DataType
        Get
            Return Me.m_dataType
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="eCoreComponentType">core component type</see> that
    ''' this object belongs to. Component types are useful for determining the
    ''' level of impact that objects have on the EwE computing model.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public ReadOnly Property CoreComponent() As eCoreComponentType _
        Implements ICoreInterface.CoreComponent
        Get
            Return Me.m_coreComponent
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set a remark.
    ''' </summary>
    ''' <param name="varName">Variable name that a remark applies to.</param>
    ''' <param name="objSec">Secundary object within the given <paramref name="varName">variable</paramref>
    ''' that a remark applies to.</param>
    ''' -----------------------------------------------------------------------
    Public Property Remark(Optional ByVal varName As eVarNameFlags = eVarNameFlags.Name,
                           Optional ByVal objSec As cCoreInputOutputBase = Nothing) As String
        Get
            Dim key As cValueID = Nothing
            If (objSec Is Nothing) Then
                key = New cValueID(Me.DataType, Me.DBID, varName)
            Else
                key = New cValueID(Me.DataType, Me.DBID, varName, objSec.DataType, objSec.DBID)
            End If
            Return Me.m_core.AuxillaryData(key).Remark
        End Get
        Set(ByVal strRemark As String)
            Dim key As cValueID = Nothing
            If (objSec Is Nothing) Then
                key = New cValueID(Me.DataType, Me.DBID, varName)
            Else
                key = New cValueID(Me.DataType, Me.DBID, varName, objSec.DataType, objSec.DBID)
            End If
            Me.m_core.AuxillaryData(key).Remark = strRemark
        End Set
    End Property

#End Region ' Public Functions/Methods

#Region " Mustoverride Methods "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Public access to set the status flags by calling each validator.
    ''' </summary>
    ''' <returns>True is successful. False otherwise</returns>
    ''' <remarks>This is the default behaviour for Input objects. Output 
    ''' objects will need to provide their own implementation due to the 
    ''' absence of validators.</remarks>
    ''' -----------------------------------------------------------------------
    Friend Overridable Function ResetStatusFlags(Optional ByVal bForceReset As Boolean = False) As Boolean

        Dim i As Integer
        Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
        Dim value As cValue
        Dim status As eStatusFlags = 0

        If Me.m_bReadOnly Then status = eStatusFlags.NotEditable

        For Each keyvalue In m_values
            Try
                value = keyvalue.Value

                Select Case value.varType
                    Case eValueTypes.SingleArray, eValueTypes.IntArray, eValueTypes.BoolArray
                        For i = 0 To value.Length
                            If bForceReset Then
                                value.Status(i) = status
                            Else
                                value.setStatusFlag(i)
                            End If
                        Next i
                    Case Else
                        If bForceReset Then
                            value.Status = status
                        Else
                            value.setStatusFlag()
                        End If
                End Select
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                Return False
            End Try
        Next keyvalue
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Destroy an instance.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub Dispose() Implements ICoreInputOutput.Dispose
        Try
            For Each val As cValue In Me.m_values.Values
                val.Dispose()
            Next
            Me.m_values.Clear()
        Catch ex As Exception
            Debug.Assert(False, "Dispose() Exception: " & ex.Message)
            cLog.Write(ex)
        End Try
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ICoreInputOutput.Disposed"/>"
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Disposed As Boolean Implements ICoreInputOutput.Disposed
        Get
            Return (Me.m_values.Count = 0)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ICoreInputOutput.Clear"/>"
    ''' -----------------------------------------------------------------------
    Public Overridable Sub Clear() Implements ICoreInputOutput.Clear
        ' NOP
    End Sub

#End Region ' Mustoverride Methods

#Region " Get/Set Status"

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the status of a variable in this object.
    ''' </summary>
    ''' <param name="VarName">Variable to request status information for.</param>
    ''' <param name="iIndex">Optional index within <paramref name="VarName">VarName</paramref>.</param>
    ''' -----------------------------------------------------------------------
    Public Overridable Function GetStatus(ByVal VarName As eVarNameFlags,
                                           Optional ByVal iSecondaryIndex As Integer = -9999,
                                            Optional ByVal iThirdIndex As Integer = -9999) As eStatusFlags Implements ICoreInputOutput.GetStatus
        Try
            Dim val As cValue = m_values.Item(VarName)
            Return val.Status(iSecondaryIndex, iThirdIndex) Or CType(If(val.Stored, eStatusFlags.Stored, 0), eStatusFlags)
        Catch ex As Exception
            Debug.Assert(False, "GetStatus() Error " & ex.Message)
            Return Nothing
        End Try
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Replaces current status flags for a given variable with a new set of status flags.
    ''' </summary>
    ''' <param name="VarName">Variable to replace status information for.</param>
    ''' <param name="iIndex">Optional index within <paramref name="VarName">VarName</paramref>.</param>
    ''' <param name="newStatus">The new status values to set.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function SetStatus(ByVal VarName As eVarNameFlags,
                              ByVal newStatus As eStatusFlags,
                              Optional ByVal iIndex As Integer = -9999,
                              Optional ByVal iThirdIndex As Integer = -9999) As Boolean Implements ICoreInputOutput.SetStatus
        Try
            m_values.Item(VarName).Status(iIndex, iThirdIndex) = newStatus
            Return True
        Catch ex As Exception
            Debug.Assert(False, "SetStatus(...) Failed to set Status " & VarName.ToString)
            Return False
        End Try
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Adds a given set of status flags to existing status flags for a given variable.
    ''' </summary>
    ''' <param name="VarName">Variable to add status information for.</param>
    ''' <param name="iIndex">Optional index within <paramref name="VarName">VarName</paramref>.</param>
    ''' <param name="statusFlags">The status values to add.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function SetStatusFlags(ByVal VarName As eVarNameFlags,
                                   ByVal statusFlags As eStatusFlags,
                                   Optional ByVal iIndex As Integer = -9999) As Boolean
        Try
            m_values.Item(VarName).Status(iIndex) = m_values.Item(VarName).Status(iIndex) Or statusFlags
            Return True
        Catch ex As Exception
            Debug.Assert(False, "SetStatusFlags(...) Failed to set status flags " & VarName.ToString)
            Return False
        End Try
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Clears a given set of status flags from existing status flags for a given variable.
    ''' </summary>
    ''' <param name="VarName">Variable to clear status information for.</param>
    ''' <param name="iIndex">Optional index within <paramref name="VarName">VarName</paramref>.</param>
    ''' <param name="statusFlags">The status values to clear.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Friend Function ClearStatusFlags(ByVal VarName As eVarNameFlags, ByVal statusFlags As eStatusFlags, Optional ByVal iIndex As Integer = -9999) As Boolean
        Try
            m_values.Item(VarName).Status(iIndex) = m_values.Item(VarName).Status(iIndex) And (Not statusFlags)
            Return True
        Catch ex As Exception
            Debug.Assert(False, "ClearStatusFlags(...) Failed to clear status flags " & VarName.ToString)
            Return False
        End Try
    End Function

#End Region

#Region " Get/set variable "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return the value of a variable.
    ''' </summary>
    ''' <param name="VarName"><see cref="eVarNameFlags">Name</see> of the variable to set.</param>
    ''' <param name="iIndex">Optional index for indexed variables.</param>
    ''' <param name="iIndex2">Optional index for indexed variables.</param>
    ''' <returns></returns>
    ''' <remarks>This only provides variables for one optional index Override this if you you need access to variables with two indexes</remarks>
    ''' -----------------------------------------------------------------------
    Public Overridable Function GetVariable(ByVal VarName As eVarNameFlags, Optional ByVal iIndex As Integer = cCore.NULL_VALUE, Optional ByVal iIndex2 As Integer = cCore.NULL_VALUE, Optional ByVal iIndex3 As Integer = cCore.NULL_VALUE) As Object Implements ICoreInputOutput.GetVariable

        Try
            ' Debug.Assert(iIndex2 = cCore.NULL_VALUE, "GetVariable(eVarNameFlags,Option Integer, Optional Integer) Called with optional argument iIndex2 this behavior must be implemented in a derived class.")
            Return m_values.Item(VarName).Value(iIndex, iIndex2)
        Catch ex As Exception
            Debug.Assert(False, "GetVariable() Error: " & VarName.ToString & " " & ex.Message)
            Return Nothing
        End Try

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Set the value of a variable.
    ''' </summary>
    ''' <param name="VarName"><see cref="eVarNameFlags">Name</see> of the variable to set.</param>
    ''' <param name="newValue">Value to set.</param>
    ''' <param name="iSecondaryIndex">Optional index for indexed variables.</param>
    ''' <returns>True if a variable is succesfully changed.</returns>
    ''' <remarks>The outcome of the SetVariable call can be examined via 
    ''' <see cref="cValue.ValidationStatus">cValue.ValidationStatus</see>.</remarks>
    ''' -----------------------------------------------------------------------
    Public Overridable Function SetVariable(ByVal VarName As eVarNameFlags,
            ByVal newValue As Object,
            Optional ByVal iSecondaryIndex As Integer = -9999,
            Optional ByVal iThirdIndex As Integer = -9999) As Boolean _
            Implements ICoreInputOutput.SetVariable

        Dim bSucces As Boolean = True
        Dim valueobject As cValue

        'get the cValue object for the dictionary
        Try
            valueobject = m_values.Item(VarName)

            ' Optimization: abort when the set operation will not change the variable value, and when
            '               a value does not need re-validating.
            ' JS 10oct09: if the valueobject holds an ENUMERATED value the Equals logic fails.
            If Object.Equals(newValue, valueobject.Value(iSecondaryIndex, iThirdIndex)) Then
                ' Report that variable has NOT been set.
                Return False
            End If

            'validate the variable by setting its value
            valueobject.Value(iSecondaryIndex, iThirdIndex) = newValue
            If valueobject.ValidationStatus = eStatusFlags.FailedValidation Then bSucces = False

            If AllowValidation Then

                ' Prepare status
                m_ValidationStatus.Copy(valueobject)
                m_ValidationStatus.iArrayIndex = iSecondaryIndex

                ' Notify core, if provided
                If (Me.m_core IsNot Nothing) Then
                    Me.m_core.OnValidated(valueobject, Me)
                End If

            End If

        Catch ex As KeyNotFoundException
            'this is most likely a programming error so assert and try to figure out why
            m_ValidationStatus.Status = eStatusFlags.ErrorEncountered
            m_ValidationStatus.Message = Me.ToString & ".setVariable(...) Failed to find variable: " & VarName.ToString
            Debug.Assert(False, Me.ToString & ".setVariable(...) Failed to find variable: " & VarName.ToString)
            bSucces = False

        Catch ex As Exception
            Debug.Assert(False, Me.ToString & ".setVariable(...) Failed to set variable " & VarName.ToString & " " & ex.Message)
            bSucces = False
        End Try

        Return bSucces

    End Function

#End Region ' Get/set variable

#Region " Metadata "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return <see cref="cVariableMetaData">metadata</see> associated with a 
    ''' variable. If the local instance does not have metadata associated 
    ''' the central <see cref="cVariableMetaData.Get(eVarNameFlags)">metadata 
    ''' repository</see> is consulted.
    ''' </summary>
    ''' <param name="varName">The variable to return metadata for.</param>
    ''' <returns>A <see cref="cVariableMetaData">metadata</see> instance, or
    ''' Null if something went wrong.</returns>
    ''' -----------------------------------------------------------------------
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public Function GetVariableMetadata(ByVal varName As eVarNameFlags) As cVariableMetaData

        If Me.m_values.ContainsKey(varName) Then
            Return m_values.Item(varName).Metadata
        End If
        Return cVariableMetaData.Get(varName)

    End Function

#End Region ' Metadata

#Region " Properties by dot(.) operator "

    ''' <summary>
    ''' Get/set whether <see cref="SetVariable">variable updates</see> should pass
    ''' through the formal variable validation structure.
    ''' </summary>
    ''' <remarks>
    ''' If enabled, validation will allow the core to respond to value changes.
    ''' Typically, validation should be turned OFF when populating variables from
    ''' the EwECore, and should be turned ON to respond to changes made by 
    ''' user interfaces or by remote calculations.
    ''' </remarks>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Friend Overridable Property AllowValidation() As Boolean
        Get
            Return m_bValidate
        End Get
        Set(ByVal value As Boolean)

            m_bValidate = value

            'set the do validation flag in all the values
            Dim valueobject As cValue
            For Each keyvalue As KeyValuePair(Of eVarNameFlags, cValue) In m_values
                valueobject = keyvalue.Value
                valueobject.AllowValidation = m_bValidate
            Next

        End Set
    End Property

    ''' <summary>
    ''' Get/set the <see cref="eVarNameFlags.Name">name</see> of a core object. 
    ''' Every ICoreInterface derived instance in EwE6 has a name.
    ''' </summary>
    ''' <remarks>
    ''' In EwE5, names were required to be unique for a given class of object
    ''' since names served as primary indexes in the database. In EwE6, this
    ''' behaviour has been changed. Names do no longer have to be unique for
    ''' each <see cref="eDataTypes">class of object</see>; names merely serve
    ''' to help identify objects in a user interface. Underneath, every object
    ''' in the EwE6 core has a unique <see cref="eVarNameFlags.DBID">database
    ''' ID</see> for a given datatype.
    ''' </remarks>
    ''' <seealso cref="DBID"/>
    ''' <seealso cref="Index"/>
    Public Property Name() As String Implements ICoreInterface.Name
        Get
            Return DirectCast(GetVariable(eVarNameFlags.Name), String)
        End Get

        Set(ByVal newValue As String)
            SetVariable(eVarNameFlags.Name, newValue)
        End Set
    End Property

    ''' <summary>
    ''' Get/set the one-based <see cref="eVarNameFlags.Index">index</see> of a 
    ''' cCoreInputOutputBase instance in the list that it is contained in.
    ''' </summary>
    ''' <remarks>
    ''' In EwE5, indexes were used to link objects together. In EwE6, this 
    ''' linkage system has been replaced with unique 
    ''' <see cref="eVarNameFlags.DBID">database IDs</see> values per 
    ''' <see cref="eDataTypes">object data type (or object class)</see>.
    ''' </remarks>
    ''' <seealso cref="DBID"/>
    ''' <seealso cref="Name"/>
    Public Property Index() As Integer Implements ICoreInterface.Index
        Get
            Return DirectCast(GetVariable(eVarNameFlags.Index), Integer)
        End Get

        Set(ByVal newValue As Integer)

            Dim keyvalue As KeyValuePair(Of eVarNameFlags, cValue)
            Dim value As cValue
            For Each keyvalue In m_values
                value = keyvalue.Value
                value.Index = newValue
            Next

            SetVariable(eVarNameFlags.Index, newValue)
        End Set
    End Property

    ''' <summary>
    ''' Returns the persistent unique database ID for an ICoreInputOutput.
    ''' </summary>
    ''' <remarks>
    ''' Applicaton layers built on top of the core will probably never need direct 
    ''' access to this property. To abstract its storage methods it seems best to
    ''' restrict access to this property to the Core assembly only.</remarks>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Property DBID() As Integer Implements ICoreInterface.DBID
        Get
            Return DirectCast(GetVariable(eVarNameFlags.DBID), Integer)
        End Get
        Friend Set(ByVal newValue As Integer)
            SetVariable(eVarNameFlags.DBID, newValue)
        End Set
    End Property

    ''' <summary>
    ''' Get the outcome of the most recently performed variable validation 
    ''' attempt on a cCoreInutOutputBase instance.
    ''' </summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public ReadOnly Property ValidationStatus() As cVariableStatus _
        Implements ICoreInputOutput.ValidationStatus
        Get
            Return m_ValidationStatus
        End Get
    End Property

    ''' <summary>
    ''' Returns the <see cref="cValue">Value descriptor</see> for a given
    ''' variable name, associated with this object.
    ''' </summary>
    ''' <param name="varName"><see cref="eVarNameFlags">Variable name</see>
    ''' to retrieve the value descriptor for.</param>
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public ReadOnly Property ValueDescriptor(ByVal varName As eVarNameFlags) As cValue
        Get
            If Me.m_values.ContainsKey(varName) Then Return Me.m_values(varName)
            Return Nothing
        End Get
    End Property

#End Region ' Properties by dot(.) operator

End Class ' CoreInputOutputBase

#End Region

#Region " cCoreGroupBase "

''' ---------------------------------------------------------------------------
''' <summary>
''' Base implementation of a core <see cref="ICoreGroup">group</see>. This
''' class serves as an base class for building dedicated group classes for the
''' various EwE components.
''' </summary>
''' --------------------------------------------------------------------------- 
Public Class cCoreGroupBase
    Inherits cCoreInputOutputBase
    Implements ICoreGroup

    ''' <summary>Zero-based index of the stanza configuration this group belongs to.</summary>
    Protected m_iStanza As Integer = cCore.NULL_VALUE

    ''' <summary>
    ''' Create and populate the Lookup tables 
    ''' </summary>
    ''' <remarks>A class the inherits from this base class will need to define its own variables in its constructor</remarks>
    Sub New(ByRef core As cCore)
        MyBase.New(core)

        Dim val As New cValue(New Single, eVarNameFlags.PP, eStatusFlags.Null, eValueTypes.Sng)
        m_values.Add(val.varName, val)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get whether this group is part of a multi-stanza configuration.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsMultiStanza() As Boolean _
        Implements ICoreGroup.IsMultiStanza
        Get
            Return Me.iStanza <> cCore.NULL_VALUE
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The ratio that this group contributes to Primary Production.
    ''' </summary>
    ''' <returns>This method will return one of the following values:
    ''' <list type="bullet">
    ''' <item>0-1 for mixed consumer/producer groups</item>
    ''' <item>1 for primary producers</item>
    ''' <item>2 for detritus groups</item>
    ''' </list>
    ''' </returns>
    ''' <remarks>This can be used as a flag to tell if a group is mixed consumer/producer, 
    ''' primary producer or a detritus group.</remarks>
    ''' -----------------------------------------------------------------------
    Public Property PP() As Single Implements ICoreGroup.PP
        Get
            Return DirectCast(GetVariable(eVarNameFlags.PP), Single)
        End Get
        Set(ByVal value As Single)
            SetVariable(eVarNameFlags.PP, value)
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the zero-based index of the stanza configuration this group belongs to.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property iStanza() As Integer _
        Implements ICoreGroup.iStanza
        Get
            Return m_iStanza
        End Get
        Set(ByVal value As Integer)
            m_iStanza = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Gets whether this group is a consumer (PP &lt; 1.0)
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsConsumer() As Boolean _
        Implements ICoreGroup.IsConsumer
        Get
            Return (Me.PP < 1.0)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Gets whether this group is detritus (PP = 2.0).
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsDetritus() As Boolean _
        Implements ICoreGroup.IsDetritus
        Get
            Return (Me.PP = 2.0)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Gets whether this group is a producer (0 &lt; PP &lt;= 1.0).
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsProducer() As Boolean _
        Implements ICoreGroup.IsProducer
        Get
            Return (Me.PP > 0 And Me.PP <= 1.0)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="ICoreGroup.IsLiving"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsLiving() As Boolean _
        Implements ICoreGroup.IsLiving
        Get
            Return (Me.Index <= Me.m_core.nLivingGroups)
        End Get
    End Property

End Class ' cCoreGroupBase

#End Region

#Region " cCoreInputOutputList "

''' ---------------------------------------------------------------------------
''' <summary>
''' Strong-typed list that handles item index offset headaches transparently.
''' </summary>
''' <remarks>
''' JS 27Aug07: list change event functionality is suspended to prevent confusion
''' in different methods on how to use these list.
''' </remarks>
''' ---------------------------------------------------------------------------
Public Class cCoreInputOutputList(Of T)
    Implements IList(Of T)

#Region " Construction "

    ''' <summary>
    ''' Offset for items in the list.
    ''' </summary>
    ''' <remarks></remarks>
    Private m_iItemOffset As Integer = 0

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The <see cref="eDataTypes">data type</see> of objects that this list contains.
    ''' </summary>
    ''' <param name="dt">The <see cref="eDataTypes">data type</see> of objects that this list holds.</param>
    ''' <param name="iItemOffset">The offset for items in this list.</param>
    ''' -----------------------------------------------------------------------
    Friend Sub New(ByVal dt As eDataTypes, ByVal iItemOffset As Integer)
        Me.m_dt = dt
        Me.m_iItemOffset = iItemOffset
    End Sub

#End Region ' Construction

#If 0 Then ' JS 27aug07: disabled list events to avoid confusion about possible list interfaces

#Region " Events "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Public event, notifying the world of list changes.
    ''' </summary>
    ''' <param name="list">The list that fired the event.</param>
    ''' -----------------------------------------------------------------------
    Public Event OnListChanged(ByVal list As cCoreInputOutputList(Of T))

    ''' <summary>Event lock flag, stating whether events are allowed to be sent out.</summary>
    ''' <remarks>This flag should be used to suppress events when a list is being configured.</remarks>
    Private m_bAllowEvents As Boolean = True
    ''' <summary>Flag stating whether events have been withheld under an active event lock.</summary>
    Private m_bHasWithheldEvents As Boolean = False

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the event lock flag.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Property AllowEvents() As Boolean

        Get
            Return Me.m_bAllowEvents
        End Get

        Set(ByVal bAllow As Boolean)
            ' Set the flag
            Me.m_bAllowEvents = bAllow
            ' If an event was withheld, send it now.
            If m_bHasWithheldEvents Then Me.FireEvent()
        End Set

    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Fire a change event.
    ''' </summary>
    ''' <remarks>
    ''' If an event lock is active, the withheld event flag is set to make sure
    ''' the event is sent when the lock is released.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Sub FireEvent()
        ' Send event if no lock active
        If Me.m_bAllowEvents Then RaiseEvent OnListChanged(Me)
        ' Update the withheld flag
        Me.m_bHasWithheldEvents = Not Me.m_bAllowEvents
    End Sub

#End Region ' Events

#End If

#Region " Public properties "

    ''' <summary>My datatype.</summary>
    Private m_dt As eDataTypes = eDataTypes.NotSet

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Gets the <see cref="eDataTypes">data type</see> of the 
    ''' <see cref="cCoreInputOutputBase">core objects</see> that this list contains.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property DataType() As eDataTypes
        Get
            Return Me.m_dt
        End Get
    End Property

#End Region ' Public properties

#Region " List interfaces "

    ''' <summary>The actual list.</summary>
    Private m_list As New List(Of T)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.Contains">List.Add</see> impementation.
    ''' Restricted access because the content of this list is managed by the EwE Core.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Overridable Sub Add(ByVal item As T) _
            Implements System.Collections.Generic.ICollection(Of T).Add
        Me.m_list.Add(item)
        ' JS 27aug07: disabled list events to avoid confusion about possible list interfaces
        'Me.FireEvent()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.Clear">List.Clear</see> impementation. 
    ''' Restricted access because the content of this list is managed by the EwE Core.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub Clear() _
             Implements System.Collections.Generic.ICollection(Of T).Clear
        Me.m_list.Clear()
        ' JS 27aug07: disabled list events to avoid confusion about possible list interfaces
        'Me.FireEvent()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.Contains">List.Contains</see> implementation.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable Function Contains(ByVal item As T) As Boolean _
             Implements System.Collections.Generic.ICollection(Of T).Contains
        Return Me.m_list.Contains(item)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.CopyTo">List.CopyTo</see> implementation.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable Sub CopyTo(ByVal aItems() As T, ByVal iIndex As Integer) _
            Implements System.Collections.Generic.ICollection(Of T).CopyTo
        Me.m_list.CopyTo(aItems, iIndex - Me.m_iItemOffset)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.Count">List.Count</see> implementation.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Count() As Integer _
            Implements System.Collections.Generic.ICollection(Of T).Count
        Get
            Return Me.m_list.Count
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.CopyTo">List.IsReadOnly</see> implementation.
    ''' </summary>
    ''' <returns>
    ''' Always true; because the content of this list is managed by the EwE Core.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsReadOnly() As Boolean _
            Implements System.Collections.Generic.ICollection(Of T).IsReadOnly
        Get
            Return True
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.Remove">List.Remove</see> impementation. 
    ''' Restricted access because the content of this list is managed by the EwE Core.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Overridable Function Remove(ByVal item As T) As Boolean _
             Implements System.Collections.Generic.ICollection(Of T).Remove
        Me.m_list.Remove(item)
        ' JS 27aug07: disabled list events to avoid confusion about possible list interfaces
        'Me.FireEvent()
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.GetEnumerator">List.GetEnumerator</see> impementation. 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of T) _
             Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator
        Return Me.m_list.GetEnumerator()
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.IndexOf">List.IndexOf</see> impementation. 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function IndexOf(ByVal item As T) As Integer _
             Implements System.Collections.Generic.IList(Of T).IndexOf
        Return Me.m_list.IndexOf(item) + Me.m_iItemOffset
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.Insert">List.Insert</see> impementation. 
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Insert(ByVal iIndex As Integer, ByVal item As T) _
             Implements System.Collections.Generic.IList(Of T).Insert
        Me.m_list.Insert(iIndex - Me.m_iItemOffset, item)
        ' JS 27aug07: disabled list events to avoid confusion about possible list interfaces
        'Me.FireEvent()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.Item">List.Item</see> impementation. 
    ''' Restricted set access because the content of this list is managed by the EwE Core.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Default Public Property Item(ByVal iIndex As Integer) As T _
            Implements System.Collections.Generic.IList(Of T).Item
        Get
            Try
                Return Me.m_list.Item(iIndex - Me.m_iItemOffset)
            Catch ex As Exception
                Debug.Assert(False, "index out of bounds")
                Return Nothing
            End Try
        End Get
        Friend Set(ByVal value As T)
            Me.m_list.Item(iIndex - Me.m_iItemOffset) = value
            ' JS 27aug07: disabled list events to avoid confusion about possible list interfaces
            'Me.FireEvent()
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Strong-typed <see cref="List.RemoveAt">List.RemoveAt</see> impementation. 
    ''' Restricted access because the content of this list is managed by the EwE Core.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Sub RemoveAt(ByVal iIndex As Integer) _
             Implements System.Collections.Generic.IList(Of T).RemoveAt
        Me.m_list.RemoveAt(iIndex - Me.m_iItemOffset)
        ' JS 27aug07: disabled list events to avoid confusion about possible list interfaces
        'Me.FireEvent()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Obligatory but totally useless list implementation.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Function ComeToAScreechingHaltInTheSandbox() As System.Collections.IEnumerator _
            Implements System.Collections.IEnumerable.GetEnumerator
        Return Nothing
    End Function

#End Region ' List interfaces

End Class ' cCoreInputOutputList

#End Region

#Region "Ecosim and Ecospace Results Wrappers"

''' <summary>
''' Interface for a helper class that wraps Ecosim or EcoSpace data structure results arrays
''' </summary>
''' <remarks>Ouput (model time step results) objects <see cref="cEcoSimGroupOutput">cEcoSimGroupOutput</see> hold a reference to core data that is wrapped for the interface to access via dot operators or getVariable(eVarNameFalgs,index,index)  </remarks>
Friend Interface IResultsWrapper

    Property Value(ByVal Index1 As Integer, Optional ByVal index2 As Integer = cCore.NULL_VALUE, Optional ByVal index3 As Integer = cCore.NULL_VALUE) As Single

End Interface


''' <summary>
''' 4D array with the first two indexes fixed
''' </summary>
''' <remarks> cEcosimDataStrucures.PredPreyResultsOverTime(var,prey,pred,time)</remarks>
Friend Class c4DResultsWrapper
    Implements IResultsWrapper

    'var, group, group, time
    Private m_data(,,,) As Single
    Private m_iVarFixed As Integer
    Private m_iGroupFixed As Integer

    Public Sub New(ByVal TheBuffer(,,,) As Single, ByVal VarIndex As Integer, ByVal GroupIndex As Integer)
        m_data = TheBuffer
        m_iVarFixed = VarIndex
        m_iGroupFixed = GroupIndex
    End Sub

    Public Property Value(ByVal GroupIndex As Integer, Optional ByVal TimeIndex As Integer = cCore.NULL_VALUE, Optional ByVal NotUsedIndex As Integer = cCore.NULL_VALUE) As Single Implements IResultsWrapper.Value
        Get
            Return m_data(m_iVarFixed, m_iGroupFixed, GroupIndex, TimeIndex)
        End Get
        Set(ByVal value As Single)
            m_data(m_iVarFixed, m_iGroupFixed, GroupIndex, TimeIndex) = value
        End Set
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        m_data = Nothing
        ' System.Console.WriteLine("Finalize Var=" & m_FixedVarIndex.ToString & ", group=" & m_FixedGroupIndex)
    End Sub

End Class


''' <summary>
''' 4D array with the first two indexes fixed
''' </summary>
''' <remarks> cEcosimDataStrucures.PredPreyResultsOverTime(var,prey,pred,time)</remarks>
Friend Class c4DResultsWrapperFirstFixed
    Implements IResultsWrapper

    'var, group, group, time
    Private m_data(,,,) As Single
    Private m_FixedIndex As Integer

    Public Sub New(ByVal TheBuffer(,,,) As Single, ByVal FixedIndex As Integer)
        m_data = TheBuffer
        m_FixedIndex = FixedIndex
    End Sub

    Public Property Value(ByVal FirstIndex As Integer, Optional ByVal SecondIndex As Integer = cCore.NULL_VALUE, Optional ByVal ThirdIndex As Integer = cCore.NULL_VALUE) As Single Implements IResultsWrapper.Value
        Get
            Return m_data(m_FixedIndex, FirstIndex, SecondIndex, ThirdIndex)
        End Get
        Set(ByVal value As Single)
            m_data(m_FixedIndex, FirstIndex, SecondIndex, ThirdIndex) = value
        End Set
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        m_data = Nothing
        ' System.Console.WriteLine("Finalize Var=" & m_FixedVarIndex.ToString & ", group=" & m_FixedGroupIndex)
    End Sub

End Class


''' <summary>
''' 2D array with the first index fixed
''' </summary>
''' <remarks></remarks>
Friend Class c2DResultsWrapper
    Implements IResultsWrapper

    ' group, group, time
    Private m_data(,) As Single
    Private m_FixedGroupIndex As Integer

    Public Sub New(ByVal TheBuffer(,) As Single, ByVal FixedGroupIndex As Integer)
        m_data = TheBuffer
        m_FixedGroupIndex = FixedGroupIndex
    End Sub

    Public Property Value(ByVal TimeIndex As Integer, Optional ByVal NotUsedIndex1 As Integer = cCore.NULL_VALUE, Optional ByVal NotUsedIndex2 As Integer = cCore.NULL_VALUE) As Single Implements IResultsWrapper.Value
        Get
            Return m_data(m_FixedGroupIndex, TimeIndex)
        End Get
        Set(ByVal value As Single)
            m_data(m_FixedGroupIndex, TimeIndex) = value
        End Set
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        m_data = Nothing
        ' System.Console.WriteLine("Finalize Var=" & m_FixedVarIndex.ToString & ", group=" & m_FixedGroupIndex)
    End Sub

End Class

''' <summary>
''' 2D array with the 2 fixed indexes
''' </summary>
''' <remarks></remarks>
Friend Class c2DResultsWrapper2Fixed
    Implements IResultsWrapper

    ' group, group, time
    Private m_data(,) As Single
    Private m_Fixed1 As Integer
    Private m_Fixed2 As Integer

    Public Sub New(ByVal TheBuffer(,) As Single, ByVal FirstFixed As Integer, ByVal SecondFixed As Integer)
        m_data = TheBuffer
        m_Fixed1 = FirstFixed
        m_Fixed2 = SecondFixed
    End Sub

    Public Property Value(ByVal NotUsedIndex1 As Integer, Optional ByVal NotUsedIndex12 As Integer = cCore.NULL_VALUE, Optional ByVal NotUsedIndex3 As Integer = cCore.NULL_VALUE) As Single Implements IResultsWrapper.Value
        Get
            Return m_data(m_Fixed1, m_Fixed2)
        End Get
        Set(ByVal value As Single)
            m_data(m_Fixed1, m_Fixed2) = value
        End Set
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        m_data = Nothing
        ' System.Console.WriteLine("Finalize Var=" & m_FixedVarIndex.ToString & ", group=" & m_FixedGroupIndex)
    End Sub

End Class



''' <summary>
''' 3D array with the first index fixed
''' </summary>
''' <remarks></remarks>
Friend Class c3DResultsWrapper
    Implements IResultsWrapper

    ' group, group, time
    Private m_data(,,) As Single
    Private m_FixedGroupIndex As Integer

    Public Sub New(ByVal TheBuffer(,,) As Single, ByVal FixedGroupIndex As Integer)
        m_data = TheBuffer
        m_FixedGroupIndex = FixedGroupIndex
    End Sub

    Public Property Value(ByVal GroupIndex As Integer, Optional ByVal Timeindex As Integer = cCore.NULL_VALUE, Optional ByVal NotUsedIndex As Integer = cCore.NULL_VALUE) As Single Implements IResultsWrapper.Value
        Get
            Return m_data(m_FixedGroupIndex, GroupIndex, Timeindex)
        End Get
        Set(ByVal value As Single)
            m_data(m_FixedGroupIndex, GroupIndex, Timeindex) = value
        End Set
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        m_data = Nothing
        ' System.Console.WriteLine("Finalize Var=" & m_FixedVarIndex.ToString & ", group=" & m_FixedGroupIndex)
    End Sub
End Class

''' <summary>
''' 3D array with the first TWO indexes fixed i.e. ResultsOverTime(FixedVar,FixedGroup,time) 
''' </summary>
''' <remarks>cEcosimDataStructures.ResultsOverTime(var,group,time)</remarks>
Friend Class c3DResultsWrapper2Fixed
    Implements IResultsWrapper

    'var, group, time
    Private m_data(,,) As Single
    Private m_FixedGroupIndex As Integer
    Private m_FixedVarIndex As Integer

    Public Sub New(ByVal TheBuffer(,,) As Single, ByVal FixedVarIndex As Integer, ByVal FixedGroupIndex As Integer)
        m_data = TheBuffer
        m_FixedGroupIndex = FixedGroupIndex
        m_FixedVarIndex = FixedVarIndex
        'System.Console.WriteLine("New Var=" & m_FixedVarIndex.ToString & ", group=" & m_FixedGroupIndex)
    End Sub

    Public Property Value(ByVal TimeIndex As Integer, Optional ByVal index2 As Integer = cCore.NULL_VALUE, Optional ByVal index3 As Integer = cCore.NULL_VALUE) As Single Implements IResultsWrapper.Value
        Get
            Return m_data(m_FixedVarIndex, m_FixedGroupIndex, TimeIndex)
        End Get
        Set(ByVal value As Single)
            m_data(m_FixedVarIndex, m_FixedGroupIndex, TimeIndex) = value
        End Set
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        m_data = Nothing
        ' System.Console.WriteLine("Finalize Var=" & m_FixedVarIndex.ToString & ", group=" & m_FixedGroupIndex)
    End Sub

End Class

#End Region





