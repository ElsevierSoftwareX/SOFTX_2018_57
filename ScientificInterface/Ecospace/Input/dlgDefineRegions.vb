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

Option Explicit On
Option Strict On

Imports EwECore
Imports EwEUtils.Core

#End Region

Namespace Ecospace

    Public Class dlgDefineRegions
        Implements IUIElement

        Private m_uic As cUIContext = Nothing

        Public Sub New(uic As cUIContext)
            Me.m_uic = uic
            Me.InitializeComponent()
        End Sub

        Public Property UIContext As ScientificInterfaceShared.Controls.cUIContext _
            Implements ScientificInterfaceShared.Controls.IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(value As ScientificInterfaceShared.Controls.cUIContext)
                Me.m_uic = value
            End Set
        End Property

        Protected Overrides Sub OnLoad(e As System.EventArgs)
            MyBase.OnLoad(e)

            Me.m_nudNoRegions.Value = Me.UIContext.Core.nRegions

            Me.m_rbFromHabitats.Enabled = (Me.UIContext.Core.nHabitats > 0)
            Me.m_rbFromMPAs.Enabled = (Me.UIContext.Core.nMPAs > 0)

            Me.UpdateControls()
            Me.CenterToScreen()

        End Sub

#Region " Events "

        Private Sub m_nudNoRegions_GotFocus(sender As Object, e As System.EventArgs) _
            Handles m_nudNoRegions.GotFocus
            Me.m_rbCustomMax.Checked = True
        End Sub

        Private Sub OnCreateOptionChanged(sender As System.Object, e As System.EventArgs) _
            Handles m_rbCustomMax.CheckedChanged, m_rbFromMPAs.CheckedChanged, m_rbFromHabitats.CheckedChanged
            Try
                Me.UpdateControls()
            Catch ex As Exception

            End Try
        End Sub

        Private Sub OnOK(sender As System.Object, e As System.EventArgs) _
            Handles m_btnOK.Click

            If Me.m_rbCustomMax.Checked Then
                Me.ChangeNumRegions()
            ElseIf Me.m_rbFromMPAs.Checked Then
                Me.CreateMPARegions()
            Else
                Me.CreateHabitatRegions()
            End If
            Me.Close()

        End Sub

        Private Sub OnCancel(sender As System.Object, e As System.EventArgs) _
            Handles m_btnCancel.Click
            Try
                Me.Close()
            Catch ex As Exception

            End Try
        End Sub

#End Region ' Events

#Region " Internals "

        Private Sub UpdateControls()

            Dim bHasSel As Boolean = (Me.m_rbCustomMax.Checked Or Me.m_rbFromMPAs.Checked Or Me.m_rbFromHabitats.Checked)
            Me.m_btnOK.Enabled = bHasSel

        End Sub

        Private Sub ChangeNumRegions()

            Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
            Dim regions As cEcospaceLayerRegion = bm.LayerRegion
            Dim parms As cEcospaceModelParameters = Me.UIContext.Core.EcospaceModelParameters
            Dim nReg As Integer = CInt(Me.m_nudNoRegions.Value)
            Dim iMaxReg As Integer = 0

            ' JS 22Nov14: cannot trust the region layer maxvalue, which is fixed in the case of regions
            '             need to test actual region layer
            For ir As Integer = 1 To bm.InRow
                For ic As Integer = 1 To bm.InCol
                    If bm.IsModelledCell(ir, ic) Then
                        iMaxReg = Math.Max(iMaxReg, CInt(regions.Cell(ir, ic)))
                    End If
                Next
            Next

            If (nReg < iMaxReg) Then
                ' ToDo: globalize this
                Dim fmsg As New cFeedbackMessage("There are cells that will no longer be assigned to regions if you continue.", _
                                                 EwEUtils.Core.eCoreComponentType.EcoSpace, eMessageType.Any, eMessageImportance.Question, _
                                                 eMessageReplyStyle.OK_CANCEL, EwEUtils.Core.eDataTypes.NotSet, eMessageReply.CANCEL)
                fmsg.Suppressable = True
                Me.m_uic.Core.Messages.SendMessage(fmsg)
                If (fmsg.Reply <> eMessageReply.OK) Then Return
            End If

            parms.nRegions = nReg

        End Sub

        Private Sub CreateMPARegions()

            If (Me.UIContext Is Nothing) Then Return

            Dim bm As cEcospaceBasemap = Me.UIContext.Core.EcospaceBasemap
            Dim regions As cEcospaceLayerRegion = bm.LayerRegion
            Dim parms As cEcospaceModelParameters = Me.UIContext.Core.EcospaceModelParameters
            Dim ll As cEcospaceLayer() = bm.Layers(eVarNameFlags.LayerMPA)

            parms.nRegions = Me.UIContext.Core.nMPAs

            For iRow As Integer = 1 To bm.InRow
                For iCol As Integer = 1 To bm.InCol
                    regions.Cell(iRow, iCol) = 0
                    For Each l As cEcospaceLayer In ll
                        Dim iMPA As Single = CSng(l.Cell(iRow, iCol))
                        If iMPA <> 0 Then
                            regions.Cell(iRow, iCol) = l.Index
                        End If
                    Next l
                Next iCol
            Next iRow

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create regions from Habitats.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub CreateHabitatRegions()

            If (Me.UIContext Is Nothing) Then Return

            Dim core As cCore = Me.UIContext.Core
            Dim bm As cEcospaceBasemap = core.EcospaceBasemap
            Dim parms As cEcospaceModelParameters = core.EcospaceModelParameters
            Dim regions As cEcospaceLayerRegion = bm.LayerRegion
            Dim sValMax As Single = 0
            Dim iHabMax As Integer = 0
            Dim nRegions As Integer = Me.UIContext.Core.nHabitats - 1

            parms.nRegions = nRegions
            Try

                For iRow As Integer = 1 To bm.InRow
                    For iCol As Integer = 1 To bm.InCol
                        sValMax = 0
                        iHabMax = 0
                        For iHab As Integer = 1 To nRegions
                            Dim sVal As Single = CSng(bm.LayerHabitat(iHab).Cell(iRow, iCol))
                            If sVal > sValMax Then
                                sValMax = sVal : iHabMax = iHab
                            End If
                        Next
                        regions.Cell(iRow, iCol) = iHabMax
                    Next iCol
                Next iRow
            Catch ex As Exception

            End Try
            regions.Invalidate()
            core.onChanged(regions)

        End Sub

#End Region ' Internals

    End Class

End Namespace
