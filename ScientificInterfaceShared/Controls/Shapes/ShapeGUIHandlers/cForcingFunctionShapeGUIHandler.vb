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
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Definitions
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="cShapeGUIHandler"/> implementation for handling generic 
    ''' <see cref="cForcingFunction">forcing functions</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    <CLSCompliant(True)> _
    Public Class cForcingShapeGUIHandler
        : Inherits cShapeGUIHandler

          ''' <summary>The FF to distribute.</summary>
        Private m_lShapes As New List(Of cShapeData)
        ''' <summary>Shape changed core message handler.</summary>
        Private m_mhShapes As cMessageHandler = Nothing
        ''' <summary>Shape changed core message handler.</summary>
        Private m_mhEcosim As cMessageHandler = Nothing

        Private m_bShowAll As Boolean = False

        Public Sub New(uic As cUIContext)
            MyBase.New(uic)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of this handler.
        ''' </summary>
        ''' <param name="stb"><see cref="ucShapeToolbox">Shape toolbox control </see> to handle, if any.</param>
        ''' <param name="stbtb"><see cref="ucShapeToolboxToolbar">Shape toolbox toolbar control </see> to handle, if any.</param>
        ''' <param name="sp"><see cref="ucSketchPad">Shape sketch pad control </see> to handle, if any.</param>
        ''' <param name="sptb"><see cref="ucSketchPadToolbar">Shape sketch pad toolbar control </see> to handle, if any.</param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Attach(ByVal stb As ucShapeToolbox, _
                                    ByVal stbtb As ucShapeToolboxToolbar, _
                                    ByVal sp As ucSketchPad, _
                                    ByVal sptb As ucSketchPadToolbar)
            MyBase.Attach(stb, stbtb, sp, sptb)
            Me.UpdateShapeList()

            Me.m_mhShapes = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.ShapesManager, eMessageType.Any, Me.UIContext.SyncObject)
            Me.UIContext.Core.Messages.AddMessageHandler(Me.m_mhShapes)
            Me.m_mhEcosim = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.EcoSim, eMessageType.Any, Me.UIContext.SyncObject)
            Me.UIContext.Core.Messages.AddMessageHandler(Me.m_mhEcosim)

            Me.DisplayFullXAxis = Me.m_bShowAll

            Dim shapes As cShapeData() = Me.Shapes()
            If (shapes.Length > 0) And Me.SelectedShape Is Nothing Then Me.SelectedShape = shapes(0)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.Detach"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Detach()
            Me.UIContext.Core.Messages.RemoveMessageHandler(Me.m_mhShapes)
            Me.m_mhShapes = Nothing
            Me.UIContext.Core.Messages.RemoveMessageHandler(Me.m_mhEcosim)
            Me.m_mhEcosim = Nothing
            MyBase.Detach()
        End Sub

#Region " Forcing overrides "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Specifies the shapes manager that delivers the data for this handler.
        ''' </summary>
        ''' <returns>The shapes manager that delivers the data for this handler.</returns>
        ''' -------------------------------------------------------------------
        Protected Overrides Function ShapeManager() As cBaseShapeManager
            Return Me.Core.ForcingShapeManager()
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the name for a new forcing function.
        ''' </summary>
        ''' <returns>The name for a new forcing function.</returns>
        ''' -------------------------------------------------------------------
        Protected Overridable Function NewShapeNameMask() As String
            Return My.Resources.ECOSIM_DEFAULT_NEWFORCINGSHAPE
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Reset a shape to a particular value.
        ''' </summary>
        ''' <param name="ashapes">The <see cref="cShapeData">shape</see> to affect.</param>
        ''' <param name="sDefaultValue">The value to set.</param>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub ResetShapes(ByVal ashapes As cShapeData(), _
                Optional ByVal sDefaultValue As Single = 1.0!)

            Dim sm As cBaseShapeManager = Nothing
            Dim shape As cShapeData = Nothing
            Dim lShapes As List(Of cShapeData) = Nothing

            If (ashapes Is Nothing) Then
                sm = Me.ShapeManager
                lShapes = New List(Of cShapeData)
                For Each shape In sm
                    lShapes.Add(shape)
                Next
                ashapes = lShapes.ToArray()
            End If

            For iShape As Integer = 0 To ashapes.Length - 1
                shape = ashapes(iShape)
                If shape IsNot Nothing Then
                    shape.LockUpdates()
                    For i As Integer = 0 To shape.nPoints ' - 1'jb why the minus one
                        shape.ShapeData(i) = sDefaultValue
                    Next i

                    ' Cheat: update every shape, but only force an update NOTIFICATION
                    ' on the very last shape
                    If iShape < (ashapes.Length - 1) Then
                        shape.Update()
                    End If
                    shape.UnlockUpdates(iShape = (ashapes.Length - 1))
                End If
            Next

            Me.SelectedShapes = Me.SelectedShapes
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Reset all shapes.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub ResetAllShapes()
            Me.ResetShapes(Nothing)
        End Sub

        Public Overrides Function XAxisMaxValue() As Integer
            If (Me.UIContext Is Nothing) Then Return 0
            If Not Me.m_bShowAll Then Return Me.Core.nEcosimTimeSteps
            Dim xMax As Integer = 0
            For Each sh As cShapeData In Me.m_lShapes
                xMax = Math.Max(sh.nPoints, xMax)
            Next
            Return xMax
        End Function

#End Region ' Forcing overrides

#Region " Baseclass overrides "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.Datatypes"/>
        ''' -------------------------------------------------------------------
        Protected Overrides Function Datatypes() As EwEUtils.Core.eDataTypes()
            Return New eDataTypes() {eDataTypes.Forcing, eDataTypes.EggProd}
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public interface to ask whether a given command is supported by this handler.
        ''' Overridden to weed out non-forcing function commands.
        ''' </summary>
        ''' <param name="cmd">The command to test.</param>
        ''' <returns>True if command is supported.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function SupportCommand(ByVal cmd As eShapeCommandTypes) As Boolean

            ' A 101 things you can do with a Forcing shape
            Select Case cmd
                Case eShapeCommandTypes.Add
                    Return True
                Case eShapeCommandTypes.ChangeShape
                    Return True
                Case eShapeCommandTypes.Duplicate
                    Return True
                Case eShapeCommandTypes.Modify
                    Return True
                Case eShapeCommandTypes.DisplayOptions
                    Return True
                Case eShapeCommandTypes.Remove
                    Return True
                Case eShapeCommandTypes.Reset, eShapeCommandTypes.ResetAll
                    Return True
                Case eShapeCommandTypes.SaveAsImage
                    Return True
                Case eShapeCommandTypes.Seasonal
                    Return True
                Case eShapeCommandTypes.ShowExtraData
                    Return True
                Case eShapeCommandTypes.DiscardExtraData
                    Return True
                Case eShapeCommandTypes.FilterName
                    Return True
                Case Else
                    Return False
            End Select
            Return False

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public interface to query the enables state of a given command by this handler.
        ''' Overridden to enable forcing function commands.
        ''' </summary>
        ''' <param name="cmd">The <see cref="eShapeCommandTypes">command</see> to test.</param>
        ''' <returns>True if enabled.</returns>
        ''' -------------------------------------------------------------------
        Public Overrides Function EnableCommand(ByVal cmd As cShapeGUIHandler.eShapeCommandTypes) As Boolean

            Dim bHasSelection As Boolean = (Me.SelectedShapes IsNot Nothing)
            Dim bHasSingleSelection As Boolean = (Me.SelectedShape IsNot Nothing)
            Dim bIsSeasonal As Boolean = False
            Dim cmdX As cCommand = Me.UIContext.CommandHandler.GetCommand("TrimUnusedShapeData")

            If bHasSelection Then
                For Each shape As cShapeData In Me.SelectedShapes
                    bIsSeasonal = bIsSeasonal Or shape.IsSeasonal
                Next
            End If

            Select Case cmd

                Case eShapeCommandTypes.Add, _
                     eShapeCommandTypes.ResetAll
                    Return True

                Case eShapeCommandTypes.Duplicate, _
                     eShapeCommandTypes.Remove, _
                     eShapeCommandTypes.Reset
                    Return bHasSelection

                Case eShapeCommandTypes.ChangeShape, _
                     eShapeCommandTypes.Modify, _
                     eShapeCommandTypes.DisplayOptions, _
                     eShapeCommandTypes.SaveAsImage, _
                     eShapeCommandTypes.Seasonal, _
                     eShapeCommandTypes.SetMaxValue
                    Return bHasSingleSelection

                Case eShapeCommandTypes.ShowExtraData
                    If Not bIsSeasonal Then Return False
                    If (cmdX IsNot Nothing) Then Return cmdX.Enabled

                Case eShapeCommandTypes.DiscardExtraData
                    If (cmdX IsNot Nothing) Then Return cmdX.Enabled

                Case eShapeCommandTypes.FilterName
                    Return True

            End Select
            Return False

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Public interface to execute a given command by this handler. 
        ''' Overridden to implement forcing function commands.
        ''' </summary>
        ''' <param name="cmd">The <see cref="eShapeCommandTypes">command</see> to test.</param>
        ''' <param name="ashapes">The <see cref="EwECore.cShapeData">shape</see> to apply the command to.</param>
        ''' <param name="data">Optional data to accompany the command.</param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub ExecuteCommand(ByVal cmd As eShapeCommandTypes, _
                 Optional ByVal ashapes As EwECore.cShapeData() = Nothing, Optional ByVal data As Object = Nothing)

            Dim cmdX As cCommand = Me.UIContext.CommandHandler.GetCommand("TrimUnusedShapeData")

            If (ashapes Is Nothing) Then ashapes = Me.SelectedShapes

            Select Case cmd
                Case eShapeCommandTypes.Add
                    Me.AddFF()

                Case eShapeCommandTypes.ChangeShape
                    Me.ChangeFFShape()

                Case eShapeCommandTypes.Duplicate
                    Me.DuplicateFF(ashapes)

                Case eShapeCommandTypes.Modify
                    Me.ModifyFF(ashapes(0))

                Case eShapeCommandTypes.DisplayOptions
                    Me.ShapeOptions()

                Case eShapeCommandTypes.Remove
                    Me.RemoveFF(ashapes)

                Case eShapeCommandTypes.Reset
                    Me.ResetShapes(ashapes)

                Case eShapeCommandTypes.ResetAll
                    Me.ResetAllShapes()

                Case eShapeCommandTypes.SaveAsImage
                    Me.SaveAsImage(ashapes(0), Me.SketchPad)

                Case eShapeCommandTypes.Seasonal
                    Me.SetSeasonal(ashapes(0), CBool(data))

                Case eShapeCommandTypes.SetMaxValue
                    Me.ScaleShape(ashapes(0), CSng(data))

                Case eShapeCommandTypes.SetToValue
                    Me.ResetShapePrompted(Me.SelectedShapes)

                Case eShapeCommandTypes.ShowExtraData
                    Me.DisplayFullXAxis = CBool(data)

                Case eShapeCommandTypes.DiscardExtraData
                    Try
                        If (cmdX IsNot Nothing) Then cmdX.Invoke()
                    Catch ex As Exception
                        ' Whoah
                    End Try

                Case Else
                    'Debug.Assert(False, cStringUtils.Localize("Command {0} not supported", cmd))
            End Select
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overridden this to make controls respond to any kind of change in 
        ''' forcing functions data.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Refresh()
            If Me.m_bInUpdate Then
                ' Perform limited update. Sketchpad and Toolbox should take care of themselves
                If (Me.SketchPadToolbar IsNot Nothing) Then Me.SketchPadToolbar.Refresh()
                If (Me.ShapeToolBoxToolbar IsNot Nothing) Then Me.ShapeToolBoxToolbar.Refresh()
            Else
                ' Do full update
                Me.DisplayFullXAxis = Me.m_bShowAll
                Me.UpdateShapeList(Me.SelectedShapes)
            End If
        End Sub

        Protected Overrides Sub OnCoreMessage(ByRef msg As EwECore.cMessage)
            MyBase.OnCoreMessage(msg)

            If (Me.SketchPad IsNot Nothing) Then
                If (msg.Source = eCoreComponentType.ShapesManager And msg.Type = eMessageType.DataModified) Then
                    Try
                        'Me.SketchPad.NumDataYears = Me.NumDataYears
                        'Me.SetDisplayYears(Me.SketchPad.XAxisMaxValue = cCore.NULL_VALUE)
                        Me.Refresh()
                    Catch ex As Exception
                    End Try
                ElseIf msg.Source = eCoreComponentType.EcoSim And msg.Type = eMessageType.EcosimNYearsChanged Then
                    Try
                        Me.SketchPad.NumDataPoints = Me.NumDataYears
                        Me.DisplayFullXAxis = Me.m_bShowAll
                        Me.Refresh()
                    Catch ex As Exception
                    End Try
                End If
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to respond to a local change in the current selected forcing function.
        ''' The forcing function is still being modified; once modifications are complete
        ''' <see cref="OnShapeFinalized">OnShapeFinalized</see> is called.
        ''' </summary>
        ''' <param name="shape">The forcing function that has changed.</param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeChanged(ByVal shape As EwECore.cShapeData)
            If Me.m_bInUpdate Then Return
            If shape IsNot Nothing Then
                Me.m_bInUpdate = True
                Me.UpdateFF(shape)
                Me.m_bInUpdate = False
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to respond to a final change in the current selected forcing function.
        ''' </summary>
        ''' <param name="shape">The forcing function that has changed.</param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeFinalized(ByVal shape As EwECore.cShapeData, ByVal sketchpad As ucSketchPad)
            If Me.m_bInUpdate Then Return
            If shape IsNot Nothing Then
                Me.m_bInUpdate = True
                Me.CommitFF(shape)
                Me.m_bInUpdate = False
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Cascade a newly selected forcing function to the managed controls.
        ''' </summary>
        ''' <param name="shape">The newly selected shape.</param>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeSelected(ByVal shape As EwECore.cShapeData())
            If Me.m_bInUpdate Then Return
            Me.m_bInUpdate = True
            Me.SelectedShapes = shape
            Me.m_bInUpdate = False
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the colour for rendering forcing functions.
        ''' </summary>
        ''' <returns>The color for rendering forcing functions.</returns>
        ''' -----------------------------------------------------------------------
        Public Overrides Function Color() As System.Drawing.Color
            Debug.Assert(Me.UIContext IsNot Nothing)
            Return Me.UIContext.StyleGuide.ShapeColor(eDataTypes.Forcing)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the default sketch mode for forcing functions.
        ''' </summary>
        ''' <param name="shape"></param>
        ''' <returns>The default sketch mode for forcing functions.</returns>
        ''' -----------------------------------------------------------------------
        Public Overrides Function SketchDrawMode(shape As cShapeData) As eSketchDrawModeTypes
            Return eSketchDrawModeTypes.Fill
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the lower limit for the sketch pad Y-axis when displaying 
        ''' forcing functions.
        ''' </summary>
        ''' <returns>The lower limit for the sketch pad Y-axis when displaying 
        ''' forcing functions.</returns>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function MinYScale() As Single
            Return 2.0!
        End Function

        ''' -----------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.NumDataYears"/>
        ''' <remarks>Overridden to limit data years to the number of Ecosim years.</remarks>
        ''' -----------------------------------------------------------------------
        Public Overrides Function NumDataYears() As Integer
            Return Me.UIContext.Core.nEcosimYears
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Set whether all available shape data should be shown in the attached
        ''' controls.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Property DisplayFullXAxis As Boolean
            Get
                Return Me.m_bShowAll
            End Get
            Set(bShowAll As Boolean)
                Me.m_bShowAll = bShowAll
                If (Me.SketchPad IsNot Nothing) Then Me.SketchPad.XAxisMaxValue = Me.XAxisMaxValue
                If (Me.ShapeToolBox IsNot Nothing) Then Me.ShapeToolBox.XAxisMaxValue = Me.XAxisMaxValue
            End Set
        End Property

#End Region ' Baseclass overrides

#Region " Internal implementation "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Add">Add</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub AddFF()
            Me.CreateShape(Me.GetNewShapeName())
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.ChangeShape">Change Shape</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub ChangeFFShape()
            Try
                Dim cmd As cCommand = Me.UIContext.CommandHandler.GetCommand("ChangeEcosimShape")
                cmd.Tag = Me.SelectedShape
                cmd.Invoke()
                cmd.Tag = Nothing
            Catch ex As Exception

            End Try
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Duplicate">Duplicate</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub DuplicateFF(ByVal ashapes As cShapeData())

            ' Sanity check
            Debug.Assert(ashapes IsNot Nothing, "Need valid FF")

            Dim fsm As cBaseShapeManager = Me.ShapeManager
            Dim lffNew As New List(Of cForcingFunction)

            For Each shape As cShapeData In ashapes
                Dim ff As cForcingFunction = DirectCast(shape, cForcingFunction)
                If ff IsNot Nothing Then
                    ff = fsm.CreateNewShape(Me.GetNewShapeName(), ff.ShapeData, ff.ShapeFunctionType, ff.ShapeFunctionParameters)
                    If ff IsNot Nothing Then
                        lffNew.Add(ff)
                    End If
                End If
            Next

            Me.UpdateShapeList(lffNew.ToArray())

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Modify">Modify</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub ModifyFF(ByVal shape As cShapeData)

            ' Sanity check
            Debug.Assert(shape IsNot Nothing, "Need valid FF")
            Debug.Assert(TypeOf shape Is cForcingFunction, "Need valid FF")

            Dim dlg As New frmShapeValue(Me.UIContext, shape)
            dlg.ShowDialog(Me.UIContext.FormMain)

        End Sub

        Private Sub ScaleShape(ByVal shape As cShapeData, ByVal sNewMax As Single)

            ' Sanity check
            Debug.Assert(shape IsNot Nothing, "Need valid FF")
            Debug.Assert(TypeOf shape Is cForcingFunction, "Need valid FF")

            If (sNewMax = 0) Then Return
            Try

                Dim scalar As Single = sNewMax / shape.YMax

                shape.LockUpdates()
                For i As Integer = 1 To shape.nPoints
                    shape.ShapeData(i) *= scalar
                Next
                shape.UnlockUpdates(True)
            Catch ex As Exception

            End Try

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Implementation of the <see cref="eShapeCommandTypes.Remove">Remove</see> commmand.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub RemoveFF(ByVal ashapes As cShapeData())

            Dim fms As cFeedbackMessage = Nothing
            Dim strMessage As String = ""
            Dim bSucces As Boolean = True

            ' Sanity check
            Debug.Assert(ashapes IsNot Nothing, "Need valid FF")

            ' Ask for permission to perform irreversible action
            If ashapes.Length = 1 Then
                strMessage = cStringUtils.Localize(My.Resources.PROMPT_SHAPE_DELETE, ashapes(0).Name)
            Else
                strMessage = cStringUtils.Localize(My.Resources.PROMPT_SHAPE_DELETE_MULTIPLE, ashapes.Length)
            End If
            fms = New cFeedbackMessage(strMessage, _
                                       eCoreComponentType.ShapesManager, _
                                       eMessageType.Any, _
                                       eMessageImportance.Warning, _
                                       eMessageReplyStyle.YES_NO, _
                                       eDataTypes.Forcing, eMessageReply.YES)
            Me.Core.Messages.SendMessage(fms, True)
            If fms.Reply = eMessageReply.NO Then Return

            Me.Core.SetBatchLock(cCore.eBatchLockType.Restructure)
            For Each shape As cShapeData In ashapes
                Debug.Assert(TypeOf shape Is cForcingFunction, "Need valid FF")
                bSucces = bSucces And ShapeManager.Remove(DirectCast(shape, cForcingFunction))
            Next
            Me.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecosim, bSucces)

            ' Refresh
            Me.UpdateShapeList()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; reflect on-going modifications in the selected forcing function.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub UpdateFF(ByVal shape As cShapeData)
            If (Me.ShapeToolBox IsNot Nothing) Then
                Me.ShapeToolBox.UpdateThumbnail(shape)
            End If
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; commit modifications of the selected forcing function to 
        ''' <see cref="ShapeManager">underlying manager</see>.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub CommitFF(ByVal shape As cShapeData)

            If shape IsNot Nothing Then

                Me.m_bInUpdate = True
                shape.Update()
                Me.m_bInUpdate = False

            End If

        End Sub

        Protected Sub ResetShapePrompted(ByVal ashapes As cShapeData())

            Dim strCaption As String = My.Resources.SHAPE_HEADER_SET_TO_VALUE
            Dim strMessage As String = My.Resources.SHAPE_PROMPT_SET_TO_VALUE
            Dim strDefault As String = "1"
            Dim strValue As String = String.Empty
            Dim sValue As Single = 0.0!

            If (ashapes Is Nothing) Then Return
            If (ashapes.Length = 0) Then Return

            Dim box As New frmInputBox()
            If (box.Show(strMessage, strCaption, strDefault) <> DialogResult.OK) Then Return
            strValue = box.Value

            'User clicks OK
            If Not String.IsNullOrEmpty(strValue) Then

                ' Process entered values
                Dim astrEntered As String() = strValue.Split(CChar(","))
                Dim lsEntered As New List(Of Single)

                Try
                    For i As Integer = 0 To astrEntered.Length - 1
                        If Not String.IsNullOrEmpty(astrEntered(i)) Then
                            If Single.TryParse(astrEntered(i), sValue) Then
                                lsEntered.Add(sValue)
                            End If
                        End If
                    Next
                    If lsEntered.Count = 0 Then lsEntered.Add(-1.0!)
                Catch ex As Exception
                    ' Whoah!
                    Return
                End Try

                For Each shape As cShapeData In ashapes
                    ' Repeat values across shape
                    shape.LockUpdates()
                    For iTime As Integer = 0 To shape.nPoints
                        shape.ShapeData(iTime) = lsEntered(iTime Mod lsEntered.Count)
                    Next
                    shape.UnlockUpdates()
                Next

            End If

        End Sub

#End Region ' Internal implementation 

#Region " Helper methods "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Init the forcing shape params like newly added shape names, reset sketchpad, etc.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected Function GetNewShapeName() As String

            Dim lstrFFNames As New List(Of String)
            Dim strNewFFName As String = ""
            Dim iNextShapeNumber As Integer = 0

            ' Collect all current shape names
            For Each s As cShapeData In Me.m_lShapes
                lstrFFNames.Add(s.Name)
            Next

            ' Concoct a new name based on the numbered strings that are found
            iNextShapeNumber = cStringUtils.GetNextNumber(lstrFFNames.ToArray(), Me.NewShapeNameMask)
            Return cStringUtils.Localize(Me.NewShapeNameMask, iNextShapeNumber)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; create a new forcing function.
        ''' </summary>
        ''' <param name="strName">Name of the new forcing function.</param>
        ''' -------------------------------------------------------------------
        Private Sub CreateShape(ByVal strName As String)
            ' Create new shape

            Dim shapeNew As cForcingFunction = ShapeManager.CreateNewShape(strName, Nothing)
            ' Validate
            If shapeNew Is Nothing Then Return
            ' Update 
            Me.UpdateShapeList(New cShapeData() {shapeNew})
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Helper method; updates the list of forcing functions.
        ''' </summary>
        ''' <param name="ashapeSelect">Forcing functions to select.</param>
        ''' -------------------------------------------------------------------
        Private Sub UpdateShapeList(Optional ByVal ashapeSelect As cShapeData() = Nothing)

            Dim bHasSelection As Boolean = False
            Dim bHasShapes As Boolean = False

            Me.m_lShapes.Clear()
            Me.m_lShapes.AddRange(Me.Shapes())

            If ashapeSelect IsNot Nothing Then
                bHasSelection = (ashapeSelect.Length > 0)
            End If
            bHasShapes = (Me.m_lShapes.Count > 0)

            If (bHasShapes And Not bHasSelection) Then
                ashapeSelect = New cShapeData() {Me.m_lShapes(0)}
            End If

            If (Me.ShapeToolBox IsNot Nothing) Then
                Me.ShapeToolBox.SetShapes(Me.m_lShapes, ashapeSelect)
            Else
                Me.SelectedShapes = ashapeSelect
            End If
        End Sub

#End Region ' Helper methods

        Public Overrides Function IsForcing() As Boolean
            Return True
        End Function

        Public Overrides Function IsMediation() As Boolean
            Return False
        End Function

        Public Overrides Function IsTimeSeries() As Boolean
            Return False
        End Function

    End Class

End Namespace ' Controls
