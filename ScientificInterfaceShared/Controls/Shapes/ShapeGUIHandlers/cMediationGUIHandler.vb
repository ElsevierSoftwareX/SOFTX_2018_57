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
Imports ScientificInterfaceShared
Imports ScientificInterfaceShared.Controls
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Utilities
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="cShapeGUIHandler">cShapeGUIHandler implementation</see> for 
    ''' handling <see cref="cMediationFunction">mediation shapes</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cMediationShapeGUIHandler
        Inherits cForcingShapeGUIHandler

        ''' <summary>Mediation assignment control to handle.</summary>
        Private m_medass As ucMediationAssignments = Nothing
        ''' <summary>Mediation assignment toolbar to handle.</summary>
        Private m_medasstoolbar As ucMediationAssignmentsToolbar = Nothing

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
        ''' <param name="ma"><see cref="ucMediationAssignments">Mediation assignments control</see> to handle, if any.</param>
        ''' <param name="mat"><see cref="ucMediationAssignmentsToolbar"/> to handle, if any.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Shadows Sub Attach(ByVal stb As ucShapeToolbox, _
                                  ByVal stbtb As ucShapeToolboxToolbar, _
                                  ByVal sp As ucSketchPad, _
                                  ByVal sptb As ucSketchPadToolbar, _
                                  ByVal ma As ucMediationAssignments, _
                                  ByVal mat As ucMediationAssignmentsToolbar)

            MyBase.Attach(stb, stbtb, sp, sptb)

            Me.MediationAssignments = ma
            Me.MediationAssignmentsToolbar = mat

            ' Manually update selection
            Me.MediationAssignments.Shape = DirectCast(Me.SelectedShape, cMediationBaseFunction)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.Detach"/>
        ''' -------------------------------------------------------------------
        Public Overloads Sub Detach()
            Me.MediationAssignments = Nothing
            Me.MediationAssignmentsToolbar = Nothing
            MyBase.Detach()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cForcingShapeGUIHandler.DisplayFullXAxis"/>
        ''' -------------------------------------------------------------------
        Public Overrides Property DisplayFullXAxis As Boolean
            Get
                Return True
            End Get
            Set(value As Boolean)
                If (Me.SketchPad IsNot Nothing) Then
                    Me.SketchPad.XAxisMaxValue = cCore.NULL_VALUE
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.SetSeasonal"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub SetSeasonal(ByVal shape As EwECore.cShapeData, ByVal bSeasonal As Boolean)
            ' Not allowed to do this; it makes absolutely no sense for Mediation shapes
            Debug.Assert(False)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Specifies the shapes manager that delivers the data for this handler.
        ''' </summary>
        ''' <returns>The shapes manager that delivers the data for this handler.</returns>
        ''' -------------------------------------------------------------------
        Protected Overrides Function ShapeManager() As cBaseShapeManager
            Return Me.Core.MediationShapeManager
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="ucMediationAssignments">mediation assingments control</see>
        ''' to for this handler.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Property MediationAssignments() As ucMediationAssignments
            Get
                Return Me.m_medass
            End Get
            Protected Set(ByVal value As ucMediationAssignments)

                If (Me.m_medass IsNot Nothing) Then
                    'Me.m_bp.Handler = Nothing
                End If

                Me.m_medass = value

                If (Me.m_medass IsNot Nothing) Then
                    'Me.m_bp.Handler = Me
                End If

            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="MediationAssignmentsToolbar">mediation assingments 
        ''' toolbar</see> to for this handler.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Overridable Property MediationAssignmentsToolbar() As ucMediationAssignmentsToolbar
            Get
                Return Me.m_medasstoolbar
            End Get
            Protected Set(ByVal value As ucMediationAssignmentsToolbar)

                If (Me.m_medasstoolbar IsNot Nothing) Then
                    Me.m_medasstoolbar.Handler = Nothing
                End If

                Me.m_medasstoolbar = value

                If (Me.m_medasstoolbar IsNot Nothing) Then
                    Me.m_medasstoolbar.Handler = Me
                End If

            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the colour for rendering mediation shapes.
        ''' </summary>
        ''' <returns>The color for rendering mediation shapes.</returns>
        ''' -----------------------------------------------------------------------
        Public Overrides Function Color() As System.Drawing.Color
            Debug.Assert(Me.UIContext IsNot Nothing)
            Return Me.UIContext.StyleGuide.ShapeColor(eDataTypes.Mediation)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the lower limit for the sketch pad Y-axis when displaying 
        ''' mediation shapes.
        ''' </summary>
        ''' <returns>The lower limit for the sketch pad Y-axis when displaying 
        ''' mediation shapes.</returns>
        ''' -----------------------------------------------------------------------
        Protected Overrides Function MinYScale() As Single
            Return 1.0!
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the name for a new Mediation shape..
        ''' </summary>
        ''' <returns>The name for a new Mediation shape.</returns>
        ''' -------------------------------------------------------------------
        Protected Overrides Function NewShapeNameMask() As String
            Return My.Resources.ECOSIM_DEFAULT_NEWMEDIATIONSHAPE
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the selected shape in the GUI. Overridden to synchronize the
        ''' BioPercent control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Property SelectedShapes() As EwECore.cShapeData()
            Get
                Return MyBase.SelectedShapes
            End Get
            Set(ByVal value As EwECore.cShapeData())

                MyBase.SelectedShapes = value

                ' Single selection
                Dim shapeSelected As cMediationBaseFunction = Nothing

                If (value IsNot Nothing) Then
                    If (value.Length = 1) Then
                        If (TypeOf value(0) Is cMediationBaseFunction) Then
                            shapeSelected = DirectCast(value(0), cMediationBaseFunction)
                        End If
                    End If
                End If

                If (Me.SketchPad IsNot Nothing) Then
                    Me.SketchPad.XMarkValue = cCore.NULL_VALUE
                    Me.SketchPad.YMarkValue = 1.0
                    If (shapeSelected IsNot Nothing) Then
                        Me.SketchPad.XMarkValue = CSng(shapeSelected.XBaseIndex)
                        If (Me.SketchPad.XMarkValue >= 0) Then
                            Me.SketchPad.YMarkValue = shapeSelected.ShapeData(Math.Max(0, Math.Min(shapeSelected.XBaseIndex, shapeSelected.ShapeData.Length - 1)))
                        End If
                    End If
                End If

                If (Me.MediationAssignmentsToolbar IsNot Nothing) Then
                    Me.MediationAssignmentsToolbar.Refresh()
                End If

                If (Me.MediationAssignments IsNot Nothing) Then
                    Me.MediationAssignments.Shape = shapeSelected
                End If

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to suppress Seasonal command.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Function SupportCommand(ByVal cmd As cShapeGUIHandler.eShapeCommandTypes) As Boolean
            Select Case cmd
                Case eShapeCommandTypes.Seasonal
                    Return False
                Case eShapeCommandTypes.DefineMediation
                    Return True
                Case eShapeCommandTypes.ShowExtraData, eShapeCommandTypes.DiscardExtraData
                    Return False
                Case eShapeCommandTypes.ViewMode
                    Return True
                Case eShapeCommandTypes.Import
                    Return True
            End Select
            Return MyBase.SupportCommand(cmd)
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.EnableCommand"/>
        ''' -------------------------------------------------------------------
        Public Overrides Function EnableCommand(ByVal cmd As ScientificInterfaceShared.Controls.cShapeGUIHandler.eShapeCommandTypes) As Boolean
            Select Case cmd
                Case eShapeCommandTypes.DefineMediation, _
                     eShapeCommandTypes.ViewMode
                    Return (Me.SelectedShape IsNot Nothing)
                Case eShapeCommandTypes.Import
                    Return True
            End Select
            Return MyBase.EnableCommand(cmd)
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.ExecuteCommand"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub ExecuteCommand(ByVal cmd As ScientificInterfaceShared.Controls.cShapeGUIHandler.eShapeCommandTypes, _
                                             Optional ByVal ashapes() As EwECore.cShapeData = Nothing, _
                                             Optional ByVal data As Object = Nothing)

            Try
                Select Case cmd

                    Case eShapeCommandTypes.DefineMediation
                        Dim dlgDefBP As New dlgDefineMediationAssignments(Me.UIContext, DirectCast(Me.SelectedShape, cMediationBaseFunction))
                        If dlgDefBP.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                            Me.MediationAssignments.RefreshContent()
                        End If

                    Case eShapeCommandTypes.ViewMode
                        Me.m_medass.ViewMode = DirectCast(data, ucMediationAssignments.eViewModeTypes)

                    Case eShapeCommandTypes.Import
                        Dim dlg As New dlgImportShapes(Me.UIContext, Me.ShapeManager)
                        If dlg.ShowDialog() = DialogResult.OK Then
                            Me.MediationAssignments.RefreshContent()
                        End If

                    Case Else
                        MyBase.ExecuteCommand(cmd, ashapes, data)

                End Select
            Catch ex As Exception

            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.OnShapeFinalized"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeFinalized(ByVal shape As EwECore.cShapeData, ByVal sketchpad As ucSketchPad)
            DirectCast(shape, cMediationBaseFunction).XBaseIndex = CInt(Math.Round(sketchpad.XMarkValue))
            MyBase.OnShapeFinalized(shape, sketchpad)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.OnShapeSelected"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeSelected(ByVal shape() As EwECore.cShapeData)
            MyBase.OnShapeSelected(shape)
            If (Me.MediationAssignments IsNot Nothing) Then
                Me.MediationAssignments.Title = My.Resources.HEADER_ASSIGNED_GROUPS_FLEETS
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.Datatypes"/>
        ''' <remarks>Overridden to enable handler for mediation shapes.</remarks>
        ''' -------------------------------------------------------------------
        Protected Overrides Function Datatypes() As EwEUtils.Core.eDataTypes()
            Return New eDataTypes() {eDataTypes.Mediation}
        End Function

        Public Overrides Function XAxisMaxValue() As Integer
            Return cEcosimDatastructures.DEFAULT_N_FORCINGPOINTS
        End Function

        Public Overrides Function IsForcing() As Boolean
            Return False
        End Function

        Public Overrides Function IsMediation() As Boolean
            Return True
        End Function

        Public Overrides Function IsTimeSeries() As Boolean
            Return False
        End Function

    End Class

End Namespace ' Ecosim
