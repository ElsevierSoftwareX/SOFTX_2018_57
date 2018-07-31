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
Imports System.Globalization
Imports System.Text
Imports EwECore.DataSources
Imports EwECore.Ecopath
Imports EwEUtils.Core
Imports EwEUtils.Extensions
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Samples

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Manager of alternate Ecopath models sampled from Monte Carlo iterations.
    ''' <seealso cref="cEcopathSampleDatastructures"/>.
    ''' <seealso cref="cEcopathSample"/>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cEcopathSampleManager
        Implements IDisposable
        Implements ICoreInterface

#Region " Private vars "

        Private m_core As cCore = Nothing
        Private m_data As cEcopathSampleDatastructures = Nothing
        Private m_rnd As Random = Nothing

        ' -- Batch run ariables --
        Private m_iRunLength As Integer
        Private m_bRandomize As Boolean = False
        Private m_bStopRun As Boolean

#End Region ' Private vars

#Region " Private classes "

        ''' <summary>
        ''' MD5 hash of selected Ecopath inputs rounded
        ''' to three relevant digits. This rounding is performed 
        ''' to allow for single / double imprecisions.
        ''' </summary>
        Private Class cEcopathHash

            Private m_iNumDigits As Integer = 0
            Private m_core As cCore = Nothing

            Public Sub New(core As cCore)
                Me.m_core = core
            End Sub

            Public Function ModelHash() As String

                Dim ecopatDS As cEcopathDataStructures = Me.m_core.m_EcoPathData
                Dim stanzaDS As cStanzaDatastructures = Me.m_core.m_Stanza
                Dim sb As New StringBuilder()

                Me.m_iNumDigits = Me.m_core.EwEModel.NumDigits

                sb.Append(Me.Hash("A", ecopatDS.Area))
                sb.Append(Me.Hash("B", ecopatDS.Binput))
                sb.Append(Me.Hash("BA", ecopatDS.BAInput))
                sb.Append(Me.Hash("BaBi", ecopatDS.BaBi))
                sb.Append(Me.Hash("Dt", ecopatDS.DtImp))
                sb.Append(Me.Hash("Im", ecopatDS.Immig))
                sb.Append(Me.Hash("Em", ecopatDS.Emig))
                sb.Append(Me.Hash("PB", ecopatDS.PBinput))
                sb.Append(Me.Hash("QB", ecopatDS.QBinput))
                sb.Append(Me.Hash("EE", ecopatDS.EEinput))
                sb.Append(Me.Hash("GE", ecopatDS.GEinput))
                sb.Append(Me.Hash("OM", ecopatDS.OtherMortinput))
                sb.Append(Me.Hash("GS", ecopatDS.GS))
                sb.Append(Me.Hash("DC", ecopatDS.DCInput))
                sb.Append(Me.Hash("PP", ecopatDS.PP))
                If (Me.m_core.nStanzas > 0) Then
                    sb.Append(Me.Hash("Sg", ecopatDS.StanzaGroup))
                    sb.Append(Me.Hash("SRp", stanzaDS.RecPowerSplit))
                    sb.Append(Me.Hash("SBa", stanzaDS.BABsplit))
                    sb.Append(Me.Hash("SW@W", stanzaDS.WmatWinf))
                    sb.Append(Me.Hash("SEgg", stanzaDS.EggAtSpawn))
                    sb.Append(Me.Hash("SBB", stanzaDS.BaseStanza))
                    sb.Append(Me.Hash("SBQ", stanzaDS.BaseStanzaCB))
                End If

                '#If DEBUG Then
                '                Console.WriteLine(sb.ToString())
                '#End If
                Return cEncryptionUtilities.MD5(sb.ToString())

            End Function

            Private Function Hash(strVar As String, data As Boolean()) As String
                If (data Is Nothing) Then Return ""
                Dim sb As New StringBuilder()
                sb.Append(strVar)
                For i As Integer = 1 To data.GetUpperBound(0)
                    If (i > 1) Then sb.Append(" ")
                    sb.Append(If(data(i), "1", "0"))
                Next
                'Debug.Print(sb.ToString())
                Return cEncryptionUtilities.MD5(sb.ToString())
            End Function

            Private Function Hash(strVar As String, data As Integer()) As String
                If (data Is Nothing) Then Return ""
                Dim sb As New StringBuilder()
                sb.Append(strVar)
                For i As Integer = 1 To data.GetUpperBound(0)
                    If (i > 1) Then sb.Append(" ")
                    sb.Append(cStringUtils.FormatNumber(data(i)))
                Next
                'Debug.Print(sb.ToString())
                Return cEncryptionUtilities.MD5(sb.ToString())
            End Function

            Private Function Hash(strVar As String, data As Single()) As String
                If (data Is Nothing) Then Return ""
                Dim sb As New StringBuilder()
                sb.Append(strVar)
                For i As Integer = 1 To data.GetUpperBound(0)
                    If (i > 1) Then sb.Append(" ")
                    sb.Append(Me.FormatNumber(data(i)))
                Next
                'Debug.Print(strVar & ": " & sb.ToString())
                Return cEncryptionUtilities.MD5(sb.ToString())
            End Function

            Private Function Hash(strVar As String, data As Single(,)) As String
                If (data Is Nothing) Then Return ""
                Dim sb As New StringBuilder()
                sb.Append(strVar)
                For i As Integer = 1 To data.GetUpperBound(0)
                    If (i > 1) Then sb.Append(" ")
                    For j As Integer = 1 To data.GetUpperBound(1)
                        If (j > 1) Then sb.Append(" ")
                        sb.Append(FormatNumber(data(i, j)))
                    Next
                Next
                Debug.Print(strVar & ": " & sb.ToString())
                Return cEncryptionUtilities.MD5(sb.ToString())
            End Function

            Private Function FormatNumber(sValue As Single) As String

                Dim ci As CultureInfo = CultureInfo.CreateSpecificCulture("en-US")
                Dim nf As NumberFormatInfo = DirectCast(ci.NumberFormat.Clone(), NumberFormatInfo)

                nf.NumberDecimalSeparator = "."
                nf.NumberGroupSeparator = ""
                nf.NumberDecimalDigits = cNumberUtils.NumRelevantDecimals(sValue, Me.m_iNumDigits)

                Return sValue.ToString("N", nf)

            End Function

        End Class

#End Region ' Private classes

#Region " Construction / destruction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="core">The core to initialize to.</param>
        ''' -------------------------------------------------------------------
        Friend Sub New(ByVal core As cCore)
            Me.m_core = core
            Me.m_data = core.m_SampleData
            Me.m_rnd = New Random()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IDisposable.Dispose"/>
        ''' -------------------------------------------------------------------
        Public Sub Dispose() _
            Implements IDisposable.Dispose

            GC.SuppressFinalize(Me)

        End Sub

#End Region ' Construction / destruction

#Region " ICoreInterface implementation "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="ICoreInterface.CoreComponent"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property CoreComponent As eCoreComponentType _
           Implements ICoreInterface.CoreComponent
            Get
                Return eCoreComponentType.EcopathSample
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="ICoreInterface.DataType"/>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property DataType As eDataTypes Implements ICoreInterface.DataType
            Get
                Return eDataTypes.EcopathSample
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="ICoreInterface.DBID"/>
        ''' -------------------------------------------------------------------
        Public Property DBID As Integer Implements ICoreInterface.DBID

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="ICoreInterface.GetID"/>
        ''' -------------------------------------------------------------------
        Public Function GetID() As String Implements ICoreInterface.GetID
            Return Me.ToString
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="ICoreInterface.Index"/>
        ''' -------------------------------------------------------------------
        Public Property Index As Integer Implements ICoreInterface.Index

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="ICoreInterface.Name"/>
        ''' -------------------------------------------------------------------
        Public Property Name() As String Implements ICoreInterface.Name
            Get
                Return Me.ToString
            End Get
            Set(ByVal value As String)
                ' NOP
            End Set
        End Property

#End Region ' ICoreInterface implementation 

#Region " Public bits "

        Public ReadOnly Property MachineName As String
            Get
                Return Environment.MachineName
            End Get
        End Property

#End Region ' Public bits

#Region " Sample management "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Initialize the manager after a model has loaded.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Init()
            Me.m_core.Messages.AddMessage(New cMessage("Samples have been added", eMessageType.DataAddedOrRemoved, eCoreComponentType.EcopathSample, eMessageImportance.Maintenance))
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clear the manager when a model has been closed.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Clear()

            Me.m_data.m_samples.Clear()

            If (Me.HasBackup) Then
                Me.RestoreEcopath()
            End If

            Me.m_data.m_loaded = Nothing
            Me.m_data.m_backup = Nothing

            Me.m_core.Messages.AddMessage(New cMessage("Samples have been removed", eMessageType.DataAddedOrRemoved, eCoreComponentType.EcopathSample, eMessageImportance.Maintenance))

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the MD5 hash for the current loaded Ecopath model.
        ''' </summary>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function ModelHash() As String

            Dim work As New cEcopathHash(Me.m_core)
            Return work.ModelHash()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Record an <see cref="cEcopathSample"/> from the current values in Ecopath.
        ''' <seealso cref="cEcopathSample"/>
        ''' </summary>
        ''' <param name="strBaseHash">The hash code for the original model.</param>
        ''' <returns>A valid sample, or Nothing if an error occurred.</returns>
        ''' -------------------------------------------------------------------
        Public Function Record(strBaseHash As String, mc As cEcosimMonteCarlo) As cEcopathSample

            Dim s As cEcopathSample = Me.MakeSnapshot(True, mc)

            If (s IsNot Nothing) Then

                s.Hash = strBaseHash

                Me.m_data.m_samples.Add(s)

                Dim test As IEwEDataSource = Me.m_core.DataSource
                If (Not TypeOf test Is IEcopathSampleDataSource) Then Return Nothing
                Dim ds As IEcopathSampleDataSource = DirectCast(test, IEcopathSampleDataSource)
                s.AllowValidation = False
                ds.AddSample(s)
                s.AllowValidation = True

                Me.m_core.Messages.SendMessage(New cMessage("Samples have been added", eMessageType.DataAddedOrRemoved, eCoreComponentType.EcopathSample, eMessageImportance.Maintenance))
            End If

            Return s

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Delete a <see cref="cEcopathSample"/>.
        ''' </summary>
        ''' <param name="samples">The <see cref="cEcopathSample">samples</see> to delete.</param>
        ''' <returns>True if the sample was deleted successfully.</returns>
        ''' -------------------------------------------------------------------
        Public Function Delete(samples() As cEcopathSample) As Boolean

            Dim test As IEwEDataSource = Me.m_core.DataSource
            If (Not TypeOf test Is IEcopathSampleDataSource) Then Return False
            Dim ds As IEcopathSampleDataSource = DirectCast(test, IEcopathSampleDataSource)
            Dim bSuccess As Boolean = True

            If (samples Is Nothing) Then Return bSuccess

            Me.m_core.SetBatchLock(cCore.eBatchLockType.Update)
            Try
                For Each s As cEcopathSample In samples
                    If (s IsNot Nothing) Then
                        ' Clean up
                        If Me.IsLoaded(s) Then Me.Load(Nothing, True)
                        If ds.RemoveSample(s) Then
                            Me.m_data.m_samples.Remove(s)
                        End If
                    End If
                Next
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
                bSuccess = False
            End Try
            Me.m_core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.NotSet)

            For i As Integer = 1 To Me.m_data.nSamples
                Me.m_data.m_samples(i - 1).Index = i
            Next

            Me.m_core.Messages.SendMessage(New cMessage("Samples have been deleted", eMessageType.DataAddedOrRemoved, eCoreComponentType.EcopathSample, eMessageImportance.Maintenance))
            Return bSuccess

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the number of available samples.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property nSamples As Integer
            Get
                Return Me.m_data.nSamples
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a single sample.
        ''' </summary>
        ''' <param name="i">The one-based index [1, <see cref="nSamples"/>] of the sample to obtain.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function Sample(i As Integer) As cEcopathSample
            Return Me.m_data.Sample(i)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns a number of randomly sampled <see cref="cEcopathSample">samples</see>.
        ''' </summary>
        ''' <param name="i">The number of samples to return.</param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Public Function RandomSamples(i As Integer) As cEcopathSample()

            Dim lSamples As New List(Of cEcopathSample)

            Throw New NotImplementedException("Not needed at this stage, perhaps for later?")

            Return lSamples.ToArray()

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Import samples from another model.
        ''' </summary>
        ''' <param name="strModel">The model file to import models from.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Public Function ImportFromModel(strModel As String) As Boolean

            Dim core As New cCore()
            Dim ds As IEwEDataSource = cDataSourceFactory.Create(strModel)
            Dim bSuccess As Boolean = False

            If (ds Is Nothing) Then Return bSuccess
            If (ds.Open(strModel, core, eDataSourceTypes.NotSet, True) <> eDatasourceAccessType.Opened) Then Return False

            If (core.LoadModel(ds)) Then

                ' JS 25Apr16: User is responsible for importing from a compatible model

                '' Test compatibility
                'If (core.SampleManager.ModelHash <> Me.ModelHash) Then
                '    Me.m_core.Messages.SendMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.SAMPLES_IMPORT_ERROR_INCOMPATIBLE, strModel),
                '                                                eMessageType.DataValidation, eCoreComponentType.External, eMessageImportance.Warning))
                '    Return False
                'End If

                ' Test if there are models
                If (core.SampleManager.nSamples = 0) Then
                    Me.m_core.Messages.SendMessage(New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.SAMPLES_IMPORT_ERROR_NOSAMPLES, strModel),
                                                                eMessageType.DataValidation, eCoreComponentType.External, eMessageImportance.Warning))
                    Return False
                End If

                ' Perform import
                bSuccess = Me.Import(core.m_SampleData)
                core.CloseModel()

            End If

            If (ds.IsOpen) Then ds.Close()
            ds.Dispose()
            core.Dispose()

            Return bSuccess

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Check if saving will erase stored samples. If so, the user is prompted.
        ''' </summary>
        ''' <returns>True if the save operation should continue.</returns>
        ''' -------------------------------------------------------------------
        Public Function CanSaveModel() As Boolean

            ' ToDo: globalize this method

            ' Build list of current samples that do not hash to the current model
            Dim lSamples As New List(Of cEcopathSample)
            Dim strModelHash As String = Me.ModelHash

            For Each s As cEcopathSample In Me.m_data.m_samples
                If (String.Compare(s.Hash, strModelHash, True) <> 0) Then lSamples.Add(s)
            Next

            ' Are there outdated samples?
            If (lSamples.Count > 0) Then
                ' Ask user what to do
                Dim fmsg As New cFeedbackMessage(cStringUtils.Localize("Your model has changed and is not compatible anymore with {0} sampled models. Do you want to save your changes and lose those samples?", lSamples.Count),
                                                 eCoreComponentType.EcopathSample, eMessageType.Any, eMessageImportance.Question, eMessageReplyStyle.YES_NO)
                fmsg.Reply = eMessageReply.YES
                Me.m_core.Messages.SendMessage(fmsg)
                If (fmsg.Reply = eMessageReply.NO) Then Return False

                Me.Delete(lSamples.ToArray())

            End If
            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load a sample into Ecopath.
        ''' </summary>
        ''' <param name="s">The sample to load, or nothing to unload a sample.</param>
        ''' <returns>True if sample loaded successfully.</returns>
        ''' -------------------------------------------------------------------
        Public Function Load(s As cEcopathSample, bRefresh As Boolean) As Boolean

            If (Me.m_core Is Nothing) Then Return False

            Dim bSucces As Boolean = True
            Dim bChanged As Boolean = False

            If (Me.HasBackup()) Then
                bSucces = bSucces And Me.RestoreEcopath()
                bChanged = True
            End If

            If (s IsNot Nothing) Then
                If (Me.m_data.m_samples.Contains(s)) Then
                    If Me.BackupEcopath() Then
                        bSucces = Me.LoadSnapshot(s)
                        bChanged = True
                    End If
                End If
            End If

            If bChanged Then
                Me.m_core.Messages.SendMessage(New cMessage("Sample load state has changed", eMessageType.DataModified, eCoreComponentType.EcopathSample, eMessageImportance.Maintenance))
                If (bRefresh) Then
                    Me.m_core.LoadEcopathInputs()
                    Me.m_core.LoadEcopathFleetInputs()
                    Me.m_core.RunEcoPath()
#If DEBUG Then
                    ValidateSnapshot(s)
#End If
                End If
            End If

            Return bSucces

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Diagnostics method, returns whether a <see cref="cEcopathSample"/>
        ''' is currently loaded in EwE.
        ''' </summary>
        ''' <param name="s">The sample to test. If no sample is provided the 
        ''' function cannot complete its test and will return a failure.</param>
        ''' <returns>True if the provided sample is currently loaded.</returns>
        ''' -------------------------------------------------------------------
        Public Function IsLoaded(s As cEcopathSample) As Boolean
            If (s Is Nothing) Then Return False
            Return ReferenceEquals(s, Me.m_data.m_loaded)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Diagnostics method, returns whether any <see cref="cEcopathSample"/>
        ''' is currently loaded in EwE.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Function IsLoaded() As Boolean
            Return (Me.m_data.m_loaded IsNot Nothing)
        End Function

#End Region ' Sample management

#Region " Running perturbations "

        Public Sub Run(ByVal iNumSamples As Integer, bRandomize As Boolean)

            If (iNumSamples = 0) Then Return

            Me.m_iRunLength = Math.Min(Me.nSamples, iNumSamples)
            Me.m_bRandomize = bRandomize
            Me.m_bStopRun = False

#If 1 Then
            Dim thread As New Threading.Thread(AddressOf RunBatch)
            thread.Start()
#Else
            Me.RunThreaded()
#End If

        End Sub

        Public Sub StopRun()
            Me.m_bStopRun = True
        End Sub

#End Region ' Running perturbations

#Region " Internals "

        Private Function BackupEcopath() As Boolean
            If Not Me.HasBackup Then
                Me.m_data.m_backup = Me.MakeSnapshot(False, Nothing)
            End If
            Return Me.HasBackup()
        End Function

        Private Function RestoreEcopath() As Boolean
            If Not Me.HasBackup Then Return True
            If Me.LoadSnapshot(Me.m_data.m_backup) Then
                Me.m_data.m_backup = Nothing
                Me.m_data.m_loaded = Nothing
                Return True
            End If
            Return False
        End Function

        Private Function HasBackup() As Boolean
            Return (Me.m_data.m_backup IsNot Nothing)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a model snapshot from Ecopath.
        ''' </summary>
        ''' <param name="bMustBalance"></param>
        ''' <param name="mc"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function MakeSnapshot(bMustBalance As Boolean, mc As cEcosimMonteCarlo) As cEcopathSample

            If (Me.m_core Is Nothing) Then Return Nothing
            If (bMustBalance And Not Me.m_core.IsModelBalanced()) Then Return Nothing

            Dim epdata As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim s As New cEcopathSample(Me.m_core, -1, Me.m_data.nSamples + 1)

            s.Source = Me.MachineName
            s.Generated = Date.Now()
            s.Hash = ""

            ' Grab parameters

            ' Making a copy from Monte Carlo perturbations?
            If (mc IsNot Nothing) Then

                Debug.WriteLine("EcoSampler: making MC snapshot " & s.Name)

                ' #Yes: obtain data from Ecopath output vars that Monte Carlo has produced
                For iGroup As Integer = 1 To epdata.NumGroups

                    s.B(iGroup) = If(mc.IsVariable(iGroup, eMCParams.Biomass), epdata.B(iGroup), cCore.NULL_VALUE)
                    s.PB(iGroup) = If(mc.IsVariable(iGroup, eMCParams.PB), epdata.PB(iGroup), cCore.NULL_VALUE)
                    s.QB(iGroup) = If(mc.IsVariable(iGroup, eMCParams.QB), epdata.QB(iGroup), cCore.NULL_VALUE)
                    s.EE(iGroup) = If(mc.IsVariable(iGroup, eMCParams.EE), epdata.EE(iGroup), cCore.NULL_VALUE)

                    If (mc.IsVariable(iGroup, eMCParams.BaBi)) Then
                        s.BaBi(iGroup) = epdata.BaBi(iGroup)
                        s.BA(iGroup) = cCore.NULL_VALUE
                    Else
                        s.BaBi(iGroup) = cCore.NULL_VALUE
                        s.BA(iGroup) = epdata.BA(iGroup)
                    End If

                    For iFleet As Integer = 1 To epdata.NumFleet
                        s.Landing(iFleet, iGroup) = If(mc.IsVariable(iGroup, eMCParams.Landings), epdata.Landing(iFleet, iGroup), cCore.NULL_VALUE)
                        s.Discard(iFleet, iGroup) = If(mc.IsVariable(iGroup, eMCParams.Discards), epdata.Discard(iFleet, iGroup), cCore.NULL_VALUE)
                    Next

                    For iPred As Integer = 1 To epdata.NumLiving
                        s.DC(iPred, iGroup) = If(mc.IsVariable(iPred, eMCParams.Diets), epdata.DC(iPred, iGroup), cCore.NULL_VALUE)
                    Next
                Next

                ' Diets
                For iPred As Integer = 1 To epdata.NumLiving
                    For iPrey As Integer = 0 To epdata.NumGroups
                        s.DC(iPred, iPrey) = If(mc.IsVariable(iPred, eMCParams.Diets), epdata.DC(iPred, iPrey), cCore.NULL_VALUE)
                    Next
                Next

            Else
                ' #No: obtain data from current input vars
                s.AllowValidation = False
                s.Name = "<backup>"
                s.AllowValidation = True

                Debug.WriteLine("EcoSampler: making backup snapshot")

                For iGroup As Integer = 1 To epdata.NumGroups

                    s.B(iGroup) = epdata.Binput(iGroup)
                    s.PB(iGroup) = epdata.PBinput(iGroup)
                    s.QB(iGroup) = epdata.QBinput(iGroup)
                    s.EE(iGroup) = epdata.EEinput(iGroup)
                    s.BA(iGroup) = epdata.BAInput(iGroup)
                    s.BaBi(iGroup) = epdata.BaBi(iGroup)

                    For iFleet As Integer = 1 To epdata.NumFleet
                        s.Landing(iFleet, iGroup) = epdata.Landing(iFleet, iGroup)
                        s.Discard(iFleet, iGroup) = epdata.Discard(iFleet, iGroup)
                    Next

                Next

                ' Diets
                For iPred As Integer = 1 To epdata.NumLiving
                    For iPrey As Integer = 0 To epdata.NumGroups
                        s.DC(iPred, iPrey) = epdata.DCInput(iPred, iPrey)
                    Next
                Next

            End If

            Return s

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load a snapshot into Ecopath.
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' -------------------------------------------------------------------
        Private Function LoadSnapshot(s As cEcopathSample) As Boolean

            ' Sanity checks
            If (Me.m_core Is Nothing) Then Return False
            If (Not Me.m_core.StateMonitor.HasEcopathLoaded) Then Return False

            Dim epdata As cEcopathDataStructures = Me.m_core.m_EcoPathData
            Dim ecopath As cEcoPathModel = Me.m_core.m_EcoPath

            ' Write sample parameters to Ecopath.
            ' Note that a sample may contain output data (from Monte Carlo), or input data (from local backup)
            ' Values must be properly entered in inputs

            Debug.WriteLine("EcoSampler: Loading " & s.Name)

            ' User wants to keep the best fit parameters
            For iGroup As Integer = 1 To m_core.nGroups

                If (s.B(iGroup) > cCore.NULL_VALUE) Then
                    epdata.Binput(iGroup) = s.B(iGroup)
                    epdata.BHinput(iGroup) = s.B(iGroup) / epdata.Area(iGroup)
                End If

                If (s.PB(iGroup) > cCore.NULL_VALUE) Then
                    epdata.PBinput(iGroup) = s.PB(iGroup)
                End If

                If (s.QB(iGroup) > cCore.NULL_VALUE) Then
                    epdata.QBinput(iGroup) = s.QB(iGroup)
                End If

                If (s.EE(iGroup) > cCore.NULL_VALUE) Then
                    epdata.EEinput(iGroup) = s.EE(iGroup)
                    If (epdata.EEinput(1) > 0) Then Stop
                End If

                If (s.BA(iGroup) > cCore.NULL_VALUE) Then
                    epdata.BAInput(iGroup) = s.BA(iGroup)
                End If

                If (s.BaBi(iGroup) > cCore.NULL_VALUE) Then
                    epdata.BaBi(iGroup) = s.BaBi(iGroup)
                End If

                For iFleet As Integer = 1 To m_core.nFleets
                    If (s.Landing(iFleet, iGroup) > cCore.NULL_VALUE) Then
                        epdata.Landing(iFleet, iGroup) = s.Landing(iFleet, iGroup)
                    End If
                    If (s.Discard(iFleet, iGroup) > cCore.NULL_VALUE) Then
                        epdata.Discard(iFleet, iGroup) = s.Discard(iFleet, iGroup)
                    End If
                Next

                For iPred As Integer = 1 To epdata.NumLiving
                    If (s.DC(iPred, iGroup) > cCore.NULL_VALUE) Then
                        epdata.DCInput(iPred, iGroup) = s.DC(iPred, iGroup)
                    End If
                Next
            Next

            Me.m_core.m_EcoPath.DetritusCalculations()
            Me.m_data.m_loaded = s

            Return True

        End Function

#If DEBUG Then

        Private Sub ValidateSnapshot(s As cEcopathSample)

            If (s Is Nothing) Then Return

            Dim epdata As cEcopathDataStructures = Me.m_core.m_EcoPathData
            For iGroup As Integer = 1 To m_core.nGroups

                Debug.Assert(epdata.B(iGroup).Approximates(s.B(iGroup)) Or s.B(iGroup) = cCore.NULL_VALUE)
                Debug.Assert(epdata.BA(iGroup).Approximates(s.BA(iGroup)) Or s.BA(iGroup) = cCore.NULL_VALUE)
                Debug.Assert(epdata.PB(iGroup).Approximates(s.PB(iGroup)) Or s.PB(iGroup) = cCore.NULL_VALUE)
                Debug.Assert(epdata.QB(iGroup).Approximates(s.QB(iGroup)) Or s.QB(iGroup) = cCore.NULL_VALUE)
                Debug.Assert(epdata.EE(iGroup).Approximates(s.EE(iGroup)) Or s.EE(iGroup) = cCore.NULL_VALUE)

                For iflt As Integer = 1 To Me.m_core.nFleets
                    Debug.Assert(epdata.Landing(iflt, iGroup) = s.Landing(iflt, iGroup) Or s.Landing(iflt, iGroup) = cCore.NULL_VALUE)
                    Debug.Assert(epdata.Discard(iflt, iGroup) = s.Discard(iflt, iGroup) Or s.Discard(iflt, iGroup) = cCore.NULL_VALUE)
                Next

                For iPred As Integer = 1 To Me.m_core.nLivingGroups
                    Debug.Assert(epdata.DCInput(iPred, iGroup) = s.DC(iPred, iGroup) Or s.DC(iPred, iGroup) = cCore.NULL_VALUE)
                Next
            Next
        End Sub

#End If

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Import samples from another data structure.
        ''' </summary>
        ''' <param name="data">The <see cref="cEcopathSampleDatastructures"/> to import from.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Private Function Import(data As cEcopathSampleDatastructures) As Boolean

            If (Me.m_core Is Nothing) Then Return False
            If (data Is Nothing) Then Return False

            Dim test As IEwEDataSource = Me.m_core.DataSource
            If (Not TypeOf test Is IEcopathSampleDataSource) Then Return False

            Dim ds As IEcopathSampleDataSource = DirectCast(test, IEcopathSampleDataSource)
            Dim hash As New Dictionary(Of String, cEcopathSample)
            Dim s As cEcopathSample = Nothing
            Dim n As Integer = 0
            Dim bSuccess As Boolean = True

            ds.BeginTransaction()

            For i As Integer = 1 To Me.m_data.nSamples
                s = Me.m_data.Sample(i)
                hash(s.Hash) = s
            Next

            For i As Integer = 1 To data.nSamples
                s = data.Sample(i)
                If Not hash.ContainsKey(s.Hash) Then

                    If ds.AddSample(s) Then
                        Me.m_data.m_samples.Add(s)
                        s.AllowValidation = False
                        s.Index = Me.m_data.nSamples
                        s.AllowValidation = True
                        n += 1
                    Else
                        bSuccess = False
                    End If

                End If
            Next

            ds.EndTransaction(bSuccess)

            If (n > 0 And bSuccess) Then
                Dim msg As New cMessage(cStringUtils.Localize(My.Resources.CoreMessages.SAMPLES_IMPORT_SUCCESS, n),
                                        eMessageType.DataAddedOrRemoved, eCoreComponentType.EcopathSample, eMessageImportance.Information)
                Me.m_core.Messages.SendMessage(msg)
            End If

            Return True

        End Function

#Region " Batch running "

        Private Sub RunBatch()

            Dim strPathOld As String = Me.m_core.OutputPath
            Dim i As Integer = 1
            Dim msg As cProgressMessage = Nothing
            Dim bIsBalanced As Boolean = False

            Dim iEcosim As Integer = Me.m_core.ActiveEcosimScenarioIndex
            Dim iEcosimTS As Integer = Me.m_core.ActiveTimeSeriesDatasetIndex
            Dim iEcospace As Integer = Me.m_core.ActiveEcospaceScenarioIndex
            Dim iEcotracer As Integer = Me.m_core.ActiveEcotracerScenarioIndex

            Dim samples As cEcopathSample() = Me.m_data.m_samples.ToArray()
            If (Me.m_bRandomize) Then samples.Shuffle()

            If Me.BackupEcopath() Then

                Me.m_core.SetBatchLock(cCore.eBatchLockType.Update)
                Me.m_core.SetStopRunDelegate(AddressOf Me.StopRun)

                Me.LogEvent(My.Resources.CoreMessages.ECOSAMPLER_BATCHRUN_STARTED, eMessageImportance.Information)

                Try
                    ' Run baseline
                    Me.SendProgress(0, My.Resources.CoreMessages.ECOSAMPLER_BATCHRUN_BASELINE)
                    Me.LogEvent(My.Resources.CoreMessages.ECOSAMPLER_BATCHRUN_BASELINE, eMessageImportance.Information)

                    Me.m_core.OutputPath = System.IO.Path.Combine(strPathOld, "Sample_baseline")
                    Me.m_core.RunEcoPath(bIsBalanced)
                    If (bIsBalanced) Then
                        If (iEcosim > 0) Then Me.m_core.RunEcoSim()
                        If (iEcospace > 0) Then Me.m_core.RunEcoSpace()
                    Else
                        Me.SendProgress(100, My.Resources.CoreMessages.ECOSAMPLER_BATCHRUN_ABORT_NOBALANCE)
                        Me.m_bStopRun = True
                    End If

                    While (i <= Me.m_iRunLength) And (Not Me.m_bStopRun)

                        ' Run sample
                        Dim s As cEcopathSample = samples(i)

                        Me.LogEvent(cStringUtils.Localize(My.Resources.CoreMessages.ECOSAMPLER_BATCHRUN_SAMPLE, s.Index), eMessageImportance.Information)
                        Me.SendProgress(CSng(i / Me.m_iRunLength), cStringUtils.Localize(My.Resources.CoreMessages.ECOSAMPLER_BATCHRUN_SAMPLE, s.Index))

                        Me.Load(s, False)

                        Me.m_core.OutputPath = System.IO.Path.Combine(strPathOld, String.Format("Sample_{0:D5}", s.Index))
                        Me.m_core.RunEcoPath(bIsBalanced)
                        If (bIsBalanced) Then
                            If (iEcosim > 0) Then Me.m_core.RunEcoSim()
                            If (iEcospace > 0) Then Me.m_core.RunEcoSpace()
                        Else
                            Me.LogEvent(cStringUtils.Localize(My.Resources.CoreMessages.ECOSAMPLER_BATCHRUN_SAMPLE_NOBALANCE, s.Index), eMessageImportance.Warning)
                        End If

                        i += 1

                    End While

                Catch ex As Exception
                    Debug.Assert(False, ex.Message)
                    cLog.Write(ex, "Ecosampler run error")
                End Try

                Me.LogEvent(My.Resources.CoreMessages.ECOSAMPLER_BATCHRUN_COMPLETED, eMessageImportance.Information)

                Me.RestoreEcopath()
                Me.m_core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.NotSet)
                Me.m_core.OutputPath = strPathOld

                ' Done
                Me.SendProgress(1, "")

            End If

        End Sub

        Private Sub SendProgress(ByVal s As Single, ByVal strStatus As String)

            Dim state As eProgressState
            Select Case s
                Case 0 : state = eProgressState.Start
                Case 1 : state = eProgressState.Finished
                Case Else : state = eProgressState.Running
            End Select

            Dim msg As New cProgressMessage(state, 1.0, s, strStatus)
            Me.m_core.Messages.SendMessage(msg)

        End Sub

        Private Sub LogEvent(strMessage As String, importance As eMessageImportance)

            Dim msg As New cMessage(strMessage, eMessageType.NotSet, eCoreComponentType.External, importance)
            Me.m_core.Messages.SendMessage(msg)

        End Sub

#End Region ' Batch running

#End Region ' Internals

    End Class

End Namespace
