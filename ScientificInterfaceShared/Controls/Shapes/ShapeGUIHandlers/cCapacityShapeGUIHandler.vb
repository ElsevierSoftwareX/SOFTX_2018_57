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
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="cShapeGUIHandler">cShapeGUIHandler implementation</see> for 
    ''' handling <see cref="cEnviroResponseFunction">environmental response functions</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cCapacityShapeGUIHandler
        Inherits cMediationShapeGUIHandler

        Public Sub New(uic As cUIContext)
            MyBase.New(uic)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Specifies the shapes manager that delivers the data for this handler.
        ''' </summary>
        ''' <returns>The shapes manager that delivers the data for this handler.</returns>
        ''' -------------------------------------------------------------------
        Protected Overrides Function ShapeManager() As cBaseShapeManager
            Return Me.Core.EnviroResponseShapeManager
        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the colour for rendering capacity shapes.
        ''' </summary>
        ''' <returns>The color for rendering capacity shapes.</returns>
        ''' -----------------------------------------------------------------------
        Public Overrides Function Color() As System.Drawing.Color
            Debug.Assert(Me.UIContext IsNot Nothing)
            Return Me.UIContext.StyleGuide.ShapeColor(eDataTypes.CapacityMediation)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.SupportCommand"/>
        ''' -----------------------------------------------------------------------
        Public Overrides Function SupportCommand(ByVal cmd As cShapeGUIHandler.eShapeCommandTypes) As Boolean
            Select Case cmd
                Case eShapeCommandTypes.ViewMode
                    Return False
                Case eShapeCommandTypes.SetMaxValue
                    Return True
                Case eShapeCommandTypes.Import
                    Return True
            End Select
            Return MyBase.SupportCommand(cmd)
        End Function

        ''' -----------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.ExecuteCommand"/>
        ''' -----------------------------------------------------------------------
        Public Overrides Sub ExecuteCommand(ByVal cmd As ScientificInterfaceShared.Controls.cShapeGUIHandler.eShapeCommandTypes, _
                                          Optional ByVal ashapes() As EwECore.cShapeData = Nothing, _
                                          Optional ByVal data As Object = Nothing)

            Try
                Select Case cmd

                    Case eShapeCommandTypes.DefineMediation
                        Debug.Assert((TypeOf Me.SelectedShape Is EwECore.cEnviroResponseFunction), "OPPSSS...")
                        Dim dlgDefBP As New dlgDefineEcospaceForagingResponse(Me.UIContext, DirectCast(Me.SelectedShape, EwECore.cEnviroResponseFunction), UIContext.Core.CapacityMapInteractionManager)
                        If dlgDefBP.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                            Me.MediationAssignments.RefreshContent()
                        End If

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
        Public Overridable Shadows Sub Attach(ByVal stb As ucShapeToolbox,
                                  ByVal stbtb As ucShapeToolboxToolbar,
                                  ByVal sp As ucSketchPad,
                                  ByVal sptb As ucSketchPadToolbar,
                                  ByVal ma As ucMediationAssignments,
                                  ByVal mat As ucMediationAssignmentsToolbar)

            MyBase.Attach(stb, stbtb, sp, sptb, ma, mat)

            Me.MediationAssignments = ma
            Me.MediationAssignmentsToolbar = mat

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the name for a new capacity shape.
        ''' </summary>
        ''' <returns>The name for a new capacity shape.</returns>
        ''' -------------------------------------------------------------------
        Protected Overrides Function NewShapeNameMask() As String
            Return My.Resources.ECOSIM_DEFAULT_NEWCAPACITYSHAPE
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.Datatypes"/>
        ''' <remarks>Overridden to enable handler for 
        ''' <see cref="cEnviroResponseFunction">environmental response functions</see>.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Protected Overrides Function Datatypes() As eDataTypes()
            Return New eDataTypes() {eDataTypes.CapacityMediation}
        End Function

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.OnShapeSelected"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeSelected(ByVal shape() As EwECore.cShapeData)
            MyBase.OnShapeSelected(shape)
            If (Me.MediationAssignments IsNot Nothing) Then
                Dim strTitle As String = ""
                If shape IsNot Nothing Then
                    If shape.Length > 0 Then
                        Dim fmt As New cCoreInterfaceFormatter()
                        strTitle = cStringUtils.Localize(My.Resources.HEADER_ASSIGNED_FORAGING_RESPONSES, fmt.GetDescriptor(shape(0), eDescriptorTypes.Name))
                    End If
                End If
                Me.MediationAssignments.Title = strTitle
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="cShapeGUIHandler.OnShapeFinalized"/>
        ''' -------------------------------------------------------------------
        Public Overrides Sub OnShapeFinalized(ByVal shape As EwECore.cShapeData, ByVal sketchpad As ucSketchPad)
            DirectCast(shape, cMediationBaseFunction).XBaseIndex = CInt(Math.Round(sketchpad.XMarkValue))
            MyBase.OnShapeFinalized(shape, sketchpad)

            If (Me.MediationAssignments IsNot Nothing) Then
                Me.MediationAssignments.RefreshContent()
            End If

        End Sub

    End Class

End Namespace
