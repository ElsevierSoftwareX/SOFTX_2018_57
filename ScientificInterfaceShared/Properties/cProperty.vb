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
Imports EwECore
Imports EwECore.Auxiliary
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Properties

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The one and only property change delegeate
    ''' </summary>
    ''' <param name="prop">The property that fired off the change enent</param>
    ''' <param name="changeFlags">A bit flag pattern that indicates which aspects of the property changed</param>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(True)> _
    Public Delegate Sub PropertyChangeEventHandler(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' A property wraps a core variable in a strong-typed object which will broadcast
    ''' change events whenever its value and/or Style changes!
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(True)> _
    Public MustInherit Class cProperty
        : Implements IDisposable

#Region " Private parts "

        Private m_key As cValueID = Nothing

        ''' <summary>cCoreInputOutputBase object that is the source of this property's data</summary>
        Private m_Source As cCoreInputOutputBase = Nothing
        ''' <summary>VarName within Source</summary>
        Private m_VarName As eVarNameFlags = eVarNameFlags.NotSet
        ''' <summary>Secundary index within VarName, in case this is an object</summary>
        Private m_SourceSec As cCoreInputOutputBase = Nothing
        ''' <summary>Secundary index within VarName, in case this is an enumerated value.</summary>
        Private m_iSecIndex As Integer = cCore.NULL_VALUE
        ''' <summary>Offset for secundary index, for instance to use when the first detritus group should provide index 1.</summary>
        Private m_iSecIndexOffset As Integer = 0
        ''' <summary>Buffered remark value.</summary>
        Private m_strRemark As String = ""
        ''' <summary>Property manager that provided this property.</summary>
        Private m_pm As cPropertyManager = Nothing

        ''' <summary>To detect redundant disposal calls.</summary>
        Private m_bDisposed As Boolean = False

        Private m_bStored As Boolean = True
        Private m_bInUpdate As Boolean = False

#If DEBUG Then
        Private Shared s_iNextID As Long = 0
        Protected m_iID As Long = 0
#End If

#End Region ' Private parts

#Region " Construction & distruction"

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a keyless property.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub New()
#If DEBUG Then
            Me.m_iID = s_iNextID
            s_iNextID += 1
#End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes the property
        ''' </summary>
        ''' <param name="source">The <see cref="cCoreInputOutputBase"/> instance that is the data source for this property.</param>
        ''' <param name="VarName">The <see cref="eVarNameFlags">Variable name</see> in <paramref name="Source"/> that is the data source for this property.</param>
        ''' <param name="srcSec">The object acting as index on <paramref name="VarName"/> in case this is an indexed variable.</param>
        ''' <param name="iSecIndexOffset">An optional offset that defines the diffence between the index provided by
        ''' <paramref name="srcSec"/> and the actual storage position in the underlying arrays.</param>
        ''' <remarks>
        ''' <para>The <paramref name="iSecIndexOffset"/> parameter is useful in cases where secundary
        ''' objects represent array indices other than their ID value.</para>
        ''' <para>A typical example would be the use of groups as secundary indexes to access Detritus fate information.
        ''' The Core detritus fate arrays are indexed by [1, {numdetritusgroups}], while the actual detritus groups that act as
        ''' secundary indexes have an <see cref="cCoreInputOutputBase.Index"/> value that is most likely higher than
        ''' the the detritus fate array index range. To compensate for this difference, a 
        ''' <paramref name="iSecIndexOffset"/> value of {<see cref="cCore.nGroups">numgroups</see>} -
        ''' {<see cref="cCore.nDetritusGroups"/>} will ensure that the <paramref name="srcSec"/>
        ''' object is used to correctly access the underlying array.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal source As EwECore.cCoreInputOutputBase,
                       ByVal VarName As eVarNameFlags,
                       Optional ByVal srcSec As EwECore.cCoreInputOutputBase = Nothing,
                       Optional ByVal iSecIndexOffset As Integer = 0)

            Me.m_key = New cValueID(source, VarName, srcSec)

            Me.m_Source = source
            Me.m_VarName = VarName
            Me.m_SourceSec = srcSec
            Me.m_iSecIndex = cCore.NULL_VALUE
            Me.m_iSecIndexOffset = iSecIndexOffset

#If DEBUG Then
            Me.m_iID = s_iNextID
            s_iNextID += 1
#End If
        End Sub

        Protected Friend Overridable Sub Dispose(ByVal bDisposing As Boolean)
            Me.m_Source = Nothing
            Me.m_SourceSec = Nothing
        End Sub

        Protected Friend Sub Dispose() Implements IDisposable.Dispose
            If Not Me.m_bDisposed Then Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Public Function IsDisposed() As Boolean
            Return Me.m_bDisposed
        End Function

#End Region ' Construction 

#Region " Properties of this property (are you confused yet?)"

        Public ReadOnly Property Key() As cValueID
            Get
                Return Me.m_key
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the ID for this property
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ID() As String
            Get
                If (Me.m_key Is Nothing) Then Return ""
                Return Me.m_key.ToString()
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cCoreInputOutputBase">cCoreInputOutputBase</see>-derived
        ''' source for the property
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Source() As cCoreInputOutputBase
            Get
                Return Me.m_Source
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="eVarNameFlags">Variable Name</see> within the
        ''' <see cref="cProperty.Source">source</see> for the property
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property VarName() As eVarNameFlags
            Get
                Return Me.m_VarName
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the secundary index within <see cref="cProperty.VarName">Variable name</see>
        ''' in case this is an indexed property
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property SourceSec() As cCoreInputOutputBase
            Get
                Return Me.m_SourceSec
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="cValue">Value description</see> of the variable
        ''' in <see cref="Source">data source</see> for the property.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ValueDescriptor() As cValue
            Get
                If Me.m_Source IsNot Nothing Then
                    Return Me.m_Source.ValueDescriptor(Me.m_VarName)
                End If
                Return Nothing
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cPropertyManager">property manager </see>
        ''' that spawned this property.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property PropertyManager() As cPropertyManager
            Get
                Return Me.m_pm
            End Get
            Protected Friend Set(ByVal pm As cPropertyManager)
                Me.m_pm = pm
            End Set
        End Property

#End Region '  Properties

#Region " Refreshing "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Refresh a property from its related core variable
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Refresh()

            Dim newValue As Object = Nothing
            Dim strNewRemark As String = ""
            Dim coreStatus As eStatusFlags = 0
            Dim guiStyle As cStyleGuide.eStyleFlags = 0
            Dim changeFlags As eChangeFlags = 0
            Dim iIndex As Integer = cCore.NULL_VALUE

            If (Me.m_SourceSec IsNot Nothing) Then
                iIndex = Me.m_SourceSec.Index
            End If
            If (Me.m_iSecIndex <> cCore.NULL_VALUE) Then
                iIndex = Me.m_iSecIndex
            End If
            iIndex -= Me.m_iSecIndexOffset

            If (Me.m_Source IsNot Nothing) Then
                ' Bail-out
                If (Me.m_Source.Disposed) Then Return

                ' Get the variable
                newValue = m_Source.GetVariable(Me.m_VarName, iIndex)

                ' Did this entail a change?
                If Not Me.IsValue(newValue) Then
                    ' # Yes: flag as changed
                    changeFlags = eChangeFlags.Value
                    ' Write the new value
                    Me.Value = newValue
                End If

                ' Get the core status
                coreStatus = m_Source.GetStatus(Me.m_VarName, iIndex)

                ' Hard-copy only the core status bits. All other flags are GUI flags and are preserved
                guiStyle = DirectCast((coreStatus And cStyleGuide.eStyleFlags.CoreStatusFlagsMask) Or
                                      (Me.Style And (Not cStyleGuide.eStyleFlags.CoreStatusFlagsMask)), cStyleGuide.eStyleFlags)
                ' Did Style change?
                If Not Me.IsStyle(guiStyle) Then
                    ' # Yes: flag as changed
                    changeFlags = changeFlags Or eChangeFlags.CoreStatus
                    ' Write the new value
                    Me.Style = guiStyle
                End If

                ' Get new remark text
                strNewRemark = Me.Remark()
                If String.Compare(strNewRemark, Me.m_strRemark, False) <> 0 Then
                    changeFlags = changeFlags Or eChangeFlags.Remarks
                    Me.Remark = strNewRemark
                End If
            End If

            ' Get remarks
            Me.UpdateRemarksStyle(TriState.False)

            ' Get stored status
            Me.m_bStored = ((coreStatus And eStatusFlags.Stored) > 0)

            ' Anything changed?
            ' JS 03Jul17: do not send out change notification when the property is actively being edited
            'If (changeFlags <> 0 And Not Me.m_bInUpdate) Then

            'jb 7-Aug-2017 Removed the m_bInUpdate flag 
            'because it was preventing the diet matrix sum col from updating in response to FireChangeNotification(...) when edited
            'In the Diet Matrix edit case the FireChangeNotification() should be handled by SetValue(), not here, 
            'unfortunatly SetValue() calls cCore.OnValidation() which sends a message which causes this to be called which handles the update.
            'Then SetValue(...) can no longer tell there has been an edit to call FireChangeNotification()
            If (changeFlags <> 0) Then
                ' #Yes: fire away
                Me.FireChangeNotification(changeFlags)
            End If

        End Sub

#End Region ' Refreshing

#Region " Value "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the type of the value
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public MustOverride Function GetValueType() As Type

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/Set the actual value for this property
        ''' </summary>
        ''' <param name="bHonourNull">Flag stating whether NULL status flags 
        ''' should return a NULL value.</param>
        ''' -------------------------------------------------------------------
        Protected MustOverride Property Value(Optional ByVal bHonourNull As Boolean = True) As Object

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the value maintained in the property
        ''' </summary>
        ''' <param name="bHonourNull">Flag stating whether NULL status flags 
        ''' should return a NULL value.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function GetValue(Optional ByVal bHonourNull As Boolean = True) As Object
            Return Me.Value(bHonourNull)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set a value in the property and commit it to the EwE core.
        ''' </summary>
        ''' <param name="newValue">The value to set.</param>
        ''' <param name="notify">Flag that states whether a change notification must be
        ''' sent out. Possible values are:
        ''' <list type="bullet">
        ''' <item>
        ''' <term>TriState.True</term>
        ''' <description>Broadcast a change notification, regardless if a value and/or Style change has occured</description>
        ''' </item>
        ''' <item>
        ''' <term>TriState.False</term>
        ''' <description>Do not broadcast a change notification, regardless if a value and/or Style change has occured</description>
        ''' </item>
        ''' <item>
        ''' <term>TriState.UseDefault</term>
        ''' <description>Broadcast a change notification when a value or Style change has occured</description>
        ''' </item>
        ''' </list>
        ''' </param>
        ''' <returns>True if this request resulted in a value and/or Style change.</returns>
        ''' <remarks>
        ''' By default, the Property is left to determine whether an event needs
        ''' to be sent.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overridable Function SetValue(ByVal newValue As Object,
                    Optional ByVal notify As TriState = TriState.UseDefault) As Boolean

            Dim vs As cVariableStatus = Nothing
            Dim changeFlags As eChangeFlags = 0
            Dim iIndex As Integer = cCore.NULL_VALUE

            ' Is this a property associated with core data?
            If (Me.m_Source IsNot Nothing) Then

                ' #Yes: try to set the variable in the core
                If (Me.m_SourceSec IsNot Nothing) Then
                    iIndex = Me.m_SourceSec.Index
                Else
                    iIndex = Me.m_iSecIndex
                End If

                ' Correct for secundary offset
                iIndex -= Me.m_iSecIndexOffset

                Me.m_bInUpdate = True

                'jb 16/mar/06 setVariable() now returns boolean so get the Style object from CurrentStyle
                ' Set new value
                m_Source.SetVariable(Me.m_VarName, newValue, iIndex)

                ' Get the status of this operation
                vs = m_Source.ValidationStatus

                Me.m_bInUpdate = False

                ' Did the core accept this value?
                If ((vs.Status And eStatusFlags.FailedValidation) = 0) Then
                    ' #Yes
                    ' Turn f.v. Style flag off. Do not notify, but check if a change occurred
                    If (Me.SetStyle(cStyleGuide.eStyleFlags.FailedValidation, TriState.False, eBitSetMode.BitwiseOff)) Then
                        changeFlags = eChangeFlags.CoreStatus
                    End If
                Else
                    ' #No
                    ' Turn f.v. Style flag on. Do not notify, but check if a change occurred
                    If (Me.SetStyle(cStyleGuide.eStyleFlags.FailedValidation, TriState.False, eBitSetMode.BitwiseOn)) Then
                        changeFlags = eChangeFlags.CoreStatus
                    End If
                    ' Fetch value corrected by the Core
                    newValue = m_Source.GetVariable(Me.m_VarName, iIndex)
                End If

            End If

            ' Will the value change?
            If (Me.IsValue(newValue) = False) Then
                ' # Yes: flag as changed
                changeFlags = changeFlags Or eChangeFlags.Value
                ' Write the new value
                Me.Value = newValue
            End If

            ' Check whether to send change notification
            If (notify = TriState.True) Or ((notify = TriState.UseDefault) And (changeFlags <> 0)) Then
                ' #Yes: fire away
                Me.FireChangeNotification(changeFlags)
            End If

            ' Return changed state
            Return (changeFlags <> 0)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether a given value is equal to the value maintained in the property
        ''' </summary>
        ''' <param name="value">Value to compare</param>
        ''' <returns>True if the values are considered equal</returns>
        ''' -------------------------------------------------------------------
        Public MustOverride Function IsValue(ByVal value As Object) As Boolean

        Public Overridable Function GetVariableMetadata() As cVariableMetaData
            ' Santiy checks
            If Me.m_Source Is Nothing Then
                Return Nothing
            End If

            Return Me.m_Source.GetVariableMetadata(Me.m_VarName)
        End Function

#End Region ' Value

#Region " Style "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enumerated type, used for setting and clearing bitflags
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eBitSetMode
            ' Set an etire bitpattern
            All = 0
            ' Set all '1' bits in a bit pattern
            BitwiseOn
            ' Clear all '1' bits in a bit pattern
            BitwiseOff
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/Set the Style of the property. This method must be implemented by
        ''' inheriting classes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected MustOverride Property Style() As cStyleGuide.eStyleFlags

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the <see cref="cStyleGuide.eStyleFlags">Style</see> of the property
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function GetStyle() As cStyleGuide.eStyleFlags
            Return Me.Style
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set the style of a Property.
        ''' </summary>
        ''' <param name="newStyle"></param>
        ''' <param name="BitSetMode"></param>
        ''' <param name="notify">
        ''' Flag that states whether a change notification needs to be broadcasted. Possible values are:
        ''' <list type="table">
        ''' <item><term>True</term><description>Always broadcasts a change notification, even when the Style has changed</description></item>
        ''' <item><term>False</term><description>Never broadcasts a change notification</description></item>
        ''' <item><term>UseDefault</term><description>Only broadcasts a change notification the Style has changed</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>True if the Style changed</returns>
        ''' <remarks>Be aware that Style flags set here are not passed down to the Core. Core status bits are exclusively
        ''' managed by the core itself. Rather, this method allows </remarks>
        ''' -------------------------------------------------------------------
        Public Function SetStyle(ByVal newStyle As cStyleGuide.eStyleFlags, _
                    Optional ByVal notify As TriState = TriState.False, _
                    Optional ByVal BitSetMode As eBitSetMode = eBitSetMode.All) As Boolean

            ' Change flag
            Dim bChanged As Boolean = False

            ' Calc what new Style flag will become
            Select Case BitSetMode
                Case eBitSetMode.All
                    ' NOP
                Case eBitSetMode.BitwiseOn
                    newStyle = Me.Style Or newStyle
                Case eBitSetMode.BitwiseOff
                    newStyle = Me.Style And (Not newStyle)
            End Select

            ' Will the style change?
            If (Not Me.IsStyle(newStyle)) Then
                ' #Yes: update the style
                Me.Style = newStyle
                ' Remember that things have changed
                bChanged = True
            End If

            ' Check if notification has to sent out
            If (notify = TriState.True Or (notify = TriState.UseDefault And bChanged = True)) Then
                Me.FireChangeNotification(eChangeFlags.CoreStatus)
            End If

            Return bChanged
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether two Styles equal. This method must be implemented by
        ''' inheriting classes
        ''' </summary>
        ''' <param name="Style">Style to compare</param>
        ''' <returns>True if the Stylees are considered equal</returns>
        ''' -------------------------------------------------------------------
        Protected MustOverride Function IsStyle(ByVal Style As cStyleGuide.eStyleFlags) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update the Remarks style bit based on available remarks and references for this property.
        ''' </summary>
        ''' <param name="notify">
        ''' Flag that states whether a change notification needs to be broadcasted. Possible values are:
        ''' <list type="table">
        ''' <item><term>True</term><description>Always broadcasts a change notification, even when the Style has changed</description></item>
        ''' <item><term>False</term><description>Never broadcasts a change notification</description></item>
        ''' <item><term>UseDefault</term><description>Only broadcasts a change notification the Style has changed</description></item>
        ''' </list>
        ''' </param>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub UpdateRemarksStyle(Optional ByVal notify As TriState = TriState.False)

            Dim nRemarksStyle As eBitSetMode = eBitSetMode.BitwiseOff

            If (Me.HasRemark() Or Me.HasReferences()) Then
                nRemarksStyle = eBitSetMode.BitwiseOn
            End If

            Me.SetStyle(cStyleGuide.eStyleFlags.Remarks, notify, nRemarksStyle)
        End Sub

#End Region ' Style

#Region " Remarks "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set the remarks for this property.
        ''' </summary>
        ''' <param name="strRemark">The remarks text to set.</param>
        ''' <param name="notify">
        ''' Flag that states whether a change notification needs to be broadcasted. Possible values are:
        ''' <list type="table">
        ''' <item><term>True</term><description>Always broadcasts a change notification, even when the Style has changed</description></item>
        ''' <item><term>False</term><description>Never broadcasts a change notification</description></item>
        ''' <item><term>UseDefault</term><description>Only broadcasts a change notification the Style has changed</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>True when Remarks have changed, False otherwise.</returns>
        ''' -------------------------------------------------------------------
        Public Function SetRemark(ByVal strRemark As String, Optional ByVal notify As TriState = TriState.UseDefault) As Boolean

            Dim bChanged As Boolean = False

            ' Prepare remarks
            If String.IsNullOrEmpty(strRemark) Then
                strRemark = ""
            Else
                strRemark = strRemark.Trim()
            End If

            ' Check if this entails a change
            If (String.Compare(strRemark, Me.Remark, False) <> 0) Then
                ' Store remarks
                Me.Remark = strRemark
                ' Update style but do not send out a notification
                Me.UpdateRemarksStyle(TriState.False)
                ' Remember change
                bChanged = True
            End If

            ' Check if remarks notification has to be sent out
            If (notify = TriState.True Or (notify = TriState.UseDefault And bChanged = True)) Then
                Me.FireChangeNotification(eChangeFlags.Remarks)
            End If

            Return bChanged
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether the value wrapped by the property is stored in the
        ''' EwE database system.
        ''' </summary>
        ''' <returns>True if the value wrapped by the property is stored in the
        ''' EwE database system.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsStored() As Boolean
            Return Me.m_bStored
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the remarks for this property.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function GetRemark() As String
            Return Me.Remark
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Performs the actual getting/setting of the remarks for this property.
        ''' Remarks are not stored in the property itself, but are obtained from the Core.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overridable Property Remark() As String
            Get
                If (Me.m_key Is Nothing) Then Return ""
                If (Me.m_pm Is Nothing) Then Return ""
                Return Me.m_pm.Core.AuxillaryData(Me.m_key).Remark
            End Get
            Set(ByVal strRemark As String)
                If (Me.m_key IsNot Nothing) Then
                    If (Me.m_pm IsNot Nothing) Then
                        Me.m_pm.Core.AuxillaryData(Me.m_key).Remark = strRemark
                    End If
                    Me.m_strRemark = strRemark
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether this property has associated remarks.
        ''' </summary>
        ''' <returns>True when remarks are present.</returns>
        ''' -------------------------------------------------------------------
        Public Function HasRemark() As Boolean
            Return (Not String.IsNullOrWhiteSpace(Me.Remark))
        End Function

#End Region ' Remarks

#Region " Pedigree "

        ''' <summary>
        ''' Get the pedigree CV [0, 100] set for the related variable, if any.
        ''' </summary>
        Public ReadOnly Property Pedigree As Integer
            Get
                If (Me.m_pm Is Nothing) Then Return 0
                If (Me.m_Source Is Nothing) Then Return 0

                Dim pedman As cPedigreeManager = Me.m_pm.Core.GetPedigreeManager(Me.m_VarName)
                If (pedman Is Nothing) Then Return 0
                Return pedman.PedigreeGroupCV(Me.m_Source.Index)
            End Get
        End Property

#End Region ' Pedigree

#Region " References "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' States whether this property has associated references
        ''' </summary>
        ''' <returns>True when references are present.</returns>
        ''' -------------------------------------------------------------------
        Public Function HasReferences() As Boolean
            Return False
        End Function

#End Region ' References

#Region " Event "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enumerated type that provides information what section of a property
        ''' has changed: its value, its CoreStatus (and thus most likely its Style)
        ''' or its associated remarks and references.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Enum eChangeFlags As Byte
            Value = 1
            CoreStatus = 2
            Remarks = 4
            Custom = 8 ' Custom data has changed
            All = 255
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Property change notification event
        ''' </summary>
        ''' <param name="prop">Me</param>
        ''' -------------------------------------------------------------------
        Public Event PropertyChanged As PropertyChangeEventHandler

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Broadcast the property change event.
        ''' </summary>
        ''' <param name="changeFlags">Flags that indicate which aspect of the property has changed</param>
        ''' -------------------------------------------------------------------
        Public Sub FireChangeNotification(Optional ByVal changeFlags As eChangeFlags = eChangeFlags.All)
            RaiseEvent PropertyChanged(Me, changeFlags)
        End Sub

#End Region ' Event

    End Class

End Namespace
