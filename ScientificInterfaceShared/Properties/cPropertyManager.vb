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
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style
Imports System.ComponentModel
Imports System.Threading

#End Region ' Imports

Namespace Properties

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Property factory and storage
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cPropertyManager
        Implements IDisposable

#Region " Private vars "

        ''' <summary>Attached core.</summary>
        Private m_core As cCore = Nothing
        ''' <summary>Message handler synchronizer.</summary>
        Private m_SyncObj As SynchronizationContext = Nothing

        ''' <summary>Error property</summary>
        Private m_propNoData As cStringProperty = Nothing

        ' ToDo: ideally this should become a dictionary of dictionaries,
        '       as dict(core component, dict(string, cProp)) in order to
        '       automate the binning process of properties

        ''' <summary>Quick property lookup tables</summary>
        Private m_htGeneric As New Dictionary(Of String, cProperty)
        Private m_htEcopath As New Dictionary(Of String, cProperty)
        Private m_htEcosim As New Dictionary(Of String, cProperty)
        Private m_htEcospace As New Dictionary(Of String, cProperty)
        Private m_htEcotracer As New Dictionary(Of String, cProperty)

        Private m_lMessageHandlers As New List(Of cMessageHandler)

#End Region ' Private vars

#Region " Construction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="core">Core instance providing property data.</param>
        ''' <param name="so">Threading <see cref="SynchronizationContext"/>.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal core As cCore, _
                       ByVal so As SynchronizationContext)

            'Sanity checks
            Debug.Assert(core IsNot Nothing)

            ' Store important refs
            Me.m_core = core
            Me.m_SyncObj = so

            ' Create No Data property
            Me.m_propNoData = New cStringProperty()
            Me.m_propNoData.SetStyle(cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.NotEditable)
            Me.m_propNoData.SetValue(String.Empty)

            ' Start listening to core messages
            Me.SetMessageHandlers()

        End Sub

        Public Sub Dispose() _
            Implements System.IDisposable.Dispose

            ' Stop listening to core messages
            Me.ClearMessageHanders()

            ' Bye default property
            Me.m_propNoData.Dispose()
            Me.m_propNoData = Nothing

            Me.m_SyncObj = Nothing
            Me.m_core = Nothing

        End Sub

#End Region ' Construction

#Region " Config "

        Public Event OnPropertyAdded(ByVal prop As cProperty)
        Public Event OnPropertyRemoved(ByVal prop As cProperty)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clears the properties cache, useful when loading new models.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Clear(ByVal msgSource As eCoreComponentType)

            Select Case msgSource
                Case eCoreComponentType.EcoPath
                    Me.Clear(Me.m_htGeneric)
                    Me.Clear(Me.m_htEcopath)
                    Me.Clear(Me.m_htEcosim)
                    Me.Clear(Me.m_htEcospace)
                    Me.Clear(Me.m_htEcotracer)

                Case eCoreComponentType.EcoSim, eCoreComponentType.MSE
                    Me.Clear(Me.m_htEcosim)
                    Me.Clear(Me.m_htEcospace)
                    Me.Clear(Me.m_htEcotracer)

                Case eCoreComponentType.EcoSpace
                    Me.Clear(Me.m_htEcospace)
                    Me.Clear(Me.m_htEcotracer)

                Case eCoreComponentType.Ecotracer
                    Me.Clear(Me.m_htEcotracer)

            End Select

        End Sub

        Friend ReadOnly Property Core() As cCore
            Get
                Return Me.m_core
            End Get
        End Property

#End Region ' Config

#Region " Public property access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a <see cref="cProperty">property</see> by its underlying ID.
        ''' </summary>
        ''' <param name="strID">The ID of the property</param>
        ''' <returns>A cProperty instance, or Nothing if the instance could not be found.</returns>
        ''' -------------------------------------------------------------------
        Public Function GetProperty(ByVal strID As String) As cProperty
            If (String.IsNullOrEmpty(strID)) Then Return Nothing
            ' Return a property from the internal storage
            If (Me.m_htGeneric.ContainsKey(strID)) Then
                Return Me.m_htGeneric(strID)
            End If
            If (Me.m_htEcopath.ContainsKey(strID)) Then
                Return Me.m_htEcopath(strID)
            End If
            If (Me.m_htEcosim.ContainsKey(strID)) Then
                Return Me.m_htEcosim(strID)
            End If
            If (Me.m_htEcospace.ContainsKey(strID)) Then
                Return Me.m_htEcospace(strID)
            End If
            If (Me.m_htEcotracer.ContainsKey(strID)) Then
                Return Me.m_htEcotracer(strID)
            End If
            Return Nothing

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns <see cref="cProperty">properties</see> by an array of underlying IDs.
        ''' </summary>
        ''' <param name="astrIDs">The IDs of the propertiets to retrieve.</param>
        ''' <returns>An array of cProperty instances.</returns>
        ''' -------------------------------------------------------------------
        Public Function GetProperties(ByVal astrIDs() As String) As cProperty()

            Dim lProps As New List(Of cProperty)
            Dim prop As cProperty = Nothing
            For Each strID As String In astrIDs
                prop = Me.GetProperty(strID)
                If prop IsNot Nothing Then lProps.Add(prop)
            Next
            Return lProps.ToArray

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a property for specific Me.m_core data
        ''' </summary>
        ''' <param name="src">The <see cref="cCoreInputOutputBase">cCoreInputOutputBase</see> instance to generate the property for</param>
        ''' <param name="VarName">The <see cref="eVarNameFlags">Variable Name</see> within the indicated Source to generate the property for</param>
        ''' <param name="srcSec">Optional <see cref="cCoreInputOutputBase">secundary cCoreInputOutputBase data source</see> in case the variable name indicates an indexed variable.</param>
        ''' <param name="bAllowedToCreate">States that the property should be created if it does not exist</param>
        ''' <remarks>The property is generated if it does not exist yet</remarks>
        ''' -------------------------------------------------------------------
        Public Function GetProperty(ByVal src As cCoreInputOutputBase, _
                                    ByVal varname As eVarNameFlags, _
                                    Optional ByVal srcSec As cCoreInputOutputBase = Nothing, _
                                    Optional ByVal bAllowedToCreate As Boolean = True, _
                                    Optional ByVal iSecundaryIndexOffset As Integer = 0) As cProperty

            Dim strID As String = Nothing
            Dim prop As cProperty = Nothing
            Dim iIndex As Integer = cCore.NULL_VALUE
            Dim ValTest As ValueWrapper.cValue = Nothing
            Dim t As Type = Nothing
            Dim ht As Dictionary(Of String, cProperty) = Nothing

            ' Does not source exist?
            If (src Is Nothing) Then
                ' #Yes: return system wide 'No Data' property to prevent
                ' code that expects a property from crashing.
                Return Me.m_propNoData
            End If

            ' Get an ID for this property
            Dim key As New cValueID(src, varname, srcSec)
            strID = key.ToString()
            Select Case src.CoreComponent
                Case eCoreComponentType.EcoPath : ht = Me.m_htEcopath
                Case eCoreComponentType.EcoSim, eCoreComponentType.MSE : ht = Me.m_htEcosim
                Case eCoreComponentType.EcoSpace : ht = Me.m_htEcospace
                Case eCoreComponentType.Ecotracer : ht = Me.m_htEcotracer
                Case Else : ht = Me.m_htGeneric
            End Select

            ' Has property been used already?
            If ht.ContainsKey(strID) Then
                ' #Yes: return it
                Return ht(strID)
            End If

            ' Property does not exist. Allowed to create it?
            If Not bAllowedToCreate Then
                ' #No: abort
                Return Nothing
            End If

            ' Determine source data type
            If srcSec IsNot Nothing Then iIndex = srcSec.Index - iSecundaryIndexOffset

            ValTest = src.ValueDescriptor(varname)

            If ValTest Is Nothing Then
                Debug.Assert(False, String.Format("Source {0} does not support varname {1}", src.Name, varname.ToString()))
            Else
                Select Case ValTest.varType
                    Case ValueWrapper.eValueTypes.Bool, ValueWrapper.eValueTypes.BoolArray
                        prop = New cBooleanProperty(src, varname, srcSec, iSecundaryIndexOffset)
                    Case ValueWrapper.eValueTypes.Int, ValueWrapper.eValueTypes.IntArray
                        prop = New cIntegerProperty(src, varname, srcSec, iSecundaryIndexOffset)
                    Case ValueWrapper.eValueTypes.Sng, ValueWrapper.eValueTypes.SingleArray
                        prop = New cSingleProperty(src, varname, srcSec, iSecundaryIndexOffset)
                    Case ValueWrapper.eValueTypes.Str
                        prop = New cStringProperty(src, varname, srcSec, iSecundaryIndexOffset)
                    Case Else
                        Debug.Assert(False, String.Format("Cannot generate property {0} for cValue type {1}", strID, ValTest.varType))
                End Select
            End If

            If (prop Is Nothing) Then Return Nothing

            ' Store property
            ht(strID) = prop
            ' Attach myself
            prop.PropertyManager = Me
            ' Make sure property is up to date
            prop.Refresh()

            Try
                RaiseEvent OnPropertyAdded(prop)
            Catch ex As Exception
                ' Plop
            End Try

            Return prop

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Extract a value ID from a given <see cref="cVariableStatus">VariableStatus</see>.
        ''' </summary>
        ''' <param name="vs">VariableStatus to find the Property instance for.</param>
        ''' <returns>A property ID, or an empty string if unsuccesful.</returns>
        ''' -------------------------------------------------------------------
        Public Function ExtractPropertyID(ByVal vs As cVariableStatus) As String

            Dim source As cCoreInputOutputBase = Nothing
            Dim sourceSec As cCoreInputOutputBase = Nothing
            Dim strID As String = ""

            ' Attempt to find an existing property for this variable
            source = DirectCast(vs.CoreDataObject, cCoreInputOutputBase)
            sourceSec = DirectCast(vs.CoreDataObjectSecundary, cCoreInputOutputBase)

            ' Does the message contain an accompanying object for a valid secundary index?
            If (vs.iArrayIndex >= 0 And vs.CoreDataObjectSecundary Is Nothing) Then
                ' #No: oops!

                ' Try to figure out the type of the secundary index via Me.m_core counters
                Dim io As cCoreInputOutputBase = DirectCast(source, cCoreInputOutputBase)
                ' Do not attempt to be smart
                If (io Is Nothing) Then Return Nothing
                Dim va As ValueWrapper.cValueArray = DirectCast(io.ValueDescriptor(vs.VarName), ValueWrapper.cValueArray)

                If va IsNot Nothing Then
                    Select Case va.CoreCounterType
                        Case eCoreCounterTypes.nGroups, eCoreCounterTypes.nDetritus, eCoreCounterTypes.nLivingGroups
                            sourceSec = Me.m_core.EcoPathGroupInputs(vs.iArrayIndex)
                        Case eCoreCounterTypes.nFleets
                            sourceSec = Me.m_core.EcopathFleetInputs(vs.iArrayIndex)
                        Case eCoreCounterTypes.nHabitats
                            sourceSec = Me.m_core.EcospaceHabitats(vs.iArrayIndex)
                            'Case eCoreCounterTypes.nRegions
                            '    sourceSec = Me.m_core.EcospaceRegions(vs.iArrayIndex)
                        Case eCoreCounterTypes.nMPAs
                            sourceSec = Me.m_core.EcospaceMPAs(vs.iArrayIndex)
                        Case eCoreCounterTypes.nMonths, _
                             eCoreCounterTypes.nEcosimYears, eCoreCounterTypes.nEcosimTimeSteps, _
                             eCoreCounterTypes.nEcospaceYears, eCoreCounterTypes.nEcospaceTimeSteps, _
                             eCoreCounterTypes.nMSEBATCHFixedF, eCoreCounterTypes.nMSEBATCHTAC, eCoreCounterTypes.nMSEBatchTFM
                            '1-Dec-2011 jb added MSEBatch counters
                            sourceSec = Nothing
                        Case eCoreCounterTypes.nStanzasForStanzaGroup
                            sourceSec = Me.m_core.StanzaGroups(vs.iArrayIndex)
                        Case Else
                            Debug.Assert(False, String.Format("Me.m_core counter type {0} not supported in property manager", va.CoreCounterType))
                    End Select
                Else
                    ' Hmm?
                End If
            End If

            ' js 07/jun/06 type check to cast to cCoreInputOutputBase
            If TypeOf source Is cCoreInputOutputBase Then
                strID = New cValueID(source, vs.VarName, sourceSec).ToString
            End If

            Return strID
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Extract all property IDs from a <see cref="cMessage">message</see>.
        ''' </summary>
        ''' <param name="msg"><see cref="cMessage">Message</see> to analyze.</param>
        ''' <returns>An array of property IDs</returns>
        ''' -------------------------------------------------------------------
        Public Function ExtractPropertyIDs(ByVal msg As cMessage) As String()

            Dim lstrIDs As New List(Of String)
            Dim strID As String = ""

            ' Validate message
            If msg Is Nothing Then Return lstrIDs.ToArray

            ' For all variables in the message
            For Each vs As cVariableStatus In msg.Variables

                ' Resolve property ID for vs
                strID = Me.ExtractPropertyID(vs)
                ' Add to list of props if resolved
                If (Not String.IsNullOrEmpty(strID)) Then lstrIDs.Add(strID)

            Next
            Return lstrIDs.ToArray

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns all registered properties. Eek.
        ''' </summary>
        ''' <returns>All registered properties.</returns>
        ''' -------------------------------------------------------------------
        Public Function GetProperties() As cProperty()
            Dim lProps As New List(Of cProperty)
            lProps.AddRange(Me.m_htGeneric.Values)
            lProps.AddRange(Me.m_htEcopath.Values)
            lProps.AddRange(Me.m_htEcosim.Values)
            lProps.AddRange(Me.m_htEcospace.Values)
            lProps.AddRange(Me.m_htEcotracer.Values)
            Return lProps.ToArray
        End Function

#End Region ' Public property access

#Region " Refresh management "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the Me.m_core values of all properties
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Refresh(ByVal msgSource As eCoreComponentType)

            For Each prop As cProperty In Me.m_htGeneric.Values
                ' Refresh yourself
                prop.Refresh()
            Next

            Select Case msgSource
                Case eCoreComponentType.EcoPath
                    For Each prop As cProperty In Me.m_htEcopath.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next
                    For Each prop As cProperty In Me.m_htEcosim.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next
                    For Each prop As cProperty In Me.m_htEcospace.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next
                    For Each prop As cProperty In Me.m_htEcotracer.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next

                Case eCoreComponentType.EcoSim, eCoreComponentType.MSE, eCoreComponentType.EcoSimMonteCarlo
                    For Each prop As cProperty In Me.m_htEcosim.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next
                    For Each prop As cProperty In Me.m_htEcospace.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next
                    For Each prop As cProperty In Me.m_htEcotracer.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next

                Case eCoreComponentType.EcoSpace
                    For Each prop As cProperty In Me.m_htEcospace.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next
                    For Each prop As cProperty In Me.m_htEcotracer.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next

                Case eCoreComponentType.Ecotracer
                    For Each prop As cProperty In Me.m_htEcotracer.Values
                        ' Refresh yourself
                        prop.Refresh()
                    Next

            End Select
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Hook up to Me.m_core messages
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub SetMessageHandlers()

            Me.m_lMessageHandlers.Add(New cMessageHandler(AddressOf Me.AllMessagesHandler, eCoreComponentType.EcoPath, eMessageType.Any, Me.m_SyncObj))
            Me.m_lMessageHandlers.Add(New cMessageHandler(AddressOf Me.AllMessagesHandler, eCoreComponentType.EcoSim, eMessageType.Any, Me.m_SyncObj))
            Me.m_lMessageHandlers.Add(New cMessageHandler(AddressOf Me.AllMessagesHandler, eCoreComponentType.EcoSpace, eMessageType.Any, Me.m_SyncObj))
            Me.m_lMessageHandlers.Add(New cMessageHandler(AddressOf Me.AllMessagesHandler, eCoreComponentType.Ecotracer, eMessageType.Any, Me.m_SyncObj))
            Me.m_lMessageHandlers.Add(New cMessageHandler(AddressOf Me.AllMessagesHandler, eCoreComponentType.MSE, eMessageType.Any, Me.m_SyncObj))
            Me.m_lMessageHandlers.Add(New cMessageHandler(AddressOf Me.AllMessagesHandler, eCoreComponentType.FishingPolicySearch, eMessageType.Any, Me.m_SyncObj))
            Me.m_lMessageHandlers.Add(New cMessageHandler(AddressOf Me.AllMessagesHandler, eCoreComponentType.EcoSimFitToTimeSeries, eMessageType.Any, Me.m_SyncObj))
            Me.m_lMessageHandlers.Add(New cMessageHandler(AddressOf Me.AllMessagesHandler, eCoreComponentType.EcoSimMonteCarlo, eMessageType.Any, Me.m_SyncObj))

            For Each mh As cMessageHandler In Me.m_lMessageHandlers
                Me.m_core.Messages.AddMessageHandler(mh)
            Next
        End Sub

        Private Sub ClearMessageHanders()
            For Each mh As cMessageHandler In Me.m_lMessageHandlers
                Me.m_core.Messages.RemoveMessageHandler(mh)
            Next
            Me.m_lMessageHandlers.Clear()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Standard Me.m_core messages handler where all property updates are triggered
        ''' </summary>
        ''' <param name="msg">An arriving message</param>
        ''' -------------------------------------------------------------------
        Private Sub AllMessagesHandler(ByRef msg As cMessage)

            ' Get properties related to message
            Dim astrPropIDs() As String = Nothing
            Dim aProps As cProperty() = Nothing
            Dim prop As cProperty = Nothing

            ' Respond to major events
            If (msg.Type = eMessageType.DataAddedOrRemoved) Then
                ' Clear when major changes have happened
                Me.Clear(msg.Source)
                ' No need to proceed since all Properties are gone
                Return
            End If

            ' Ignore irrelevant messages
            If msg.Type = eMessageType.DataImport Then Return

            astrPropIDs = Me.ExtractPropertyIDs(msg)
            aProps = Me.GetProperties(astrPropIDs)

            Try
                If (astrPropIDs.Length = 0) Then
                    ' Update everything (ouch)
                    Me.Refresh(msg.Source)
                Else
                    ' Update each property in this message
                    For Each prop In aProps
                        prop.Refresh()
                    Next
                End If
            Catch ex As Exception
                ' Log entry
                cLog.Write(msg, "cPropertyManager.AllMessagesHandler")
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Erase a property dictionary
        ''' </summary>
        ''' <param name="dic">The dictionary to clear.</param>
        ''' -------------------------------------------------------------------
        Private Sub Clear(ByRef dic As Dictionary(Of String, cProperty))
            Try
                For Each prop As cProperty In dic.Values
                    RaiseEvent OnPropertyRemoved(prop)
                Next
            Catch ex As Exception
                ' MrNiceGuy--
            End Try
            dic.Clear()
        End Sub

#End Region ' Refresh management

    End Class

End Namespace
